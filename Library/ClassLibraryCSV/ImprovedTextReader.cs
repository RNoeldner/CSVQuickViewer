/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   Wrapper around a TextReader that handles BOM, encoding, and skipped lines.
  ///   Provides a method <see cref="ToBeginning"/> to reset the reader to the start of the stream.
  /// </summary>
  public sealed class ImprovedTextReader : DisposableBase
  {
    /// <summary>
    ///   The carriage return character. Escape code is <c>\r</c>.
    /// </summary>
    private const char cCr = (char) 0x0d;

    /// <summary>
    ///   The line-feed character. Escape code is <c>\n</c>.
    /// </summary>
    private const char cLf = (char) 0x0a;

    private readonly int m_BomLength;
    private readonly int m_SkipLines;
    private readonly Stream m_Stream;
    private readonly int m_CodePageId;
    private int m_LastChar = -1;


    private void PositionAfterBom()
    {
      m_Stream.Seek(0, SeekOrigin.Begin);
#if NET6_0_OR_GREATER
      Span<byte> bomBufferPass = stackalloc byte[m_BomLength];
      m_Stream.Read(bomBufferPass);
#else
            m_Stream.Read(new byte[m_BomLength], 0, m_BomLength);
#endif

    }

    /// <summary>
    ///   Creates an instance of the TextReader
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="codePageId">The assumed code page id</param>
    /// <param name="skipLines">
    ///   Number of lines that should be skipped at the beginning of the file
    /// </param>
    /// <remarks>
    ///   This routine uses a TextReader to allow character decoding, it will read the
    ///   first few bytes of the source stream to look at a possible existing BOM if found, it will
    ///   overwrite the provided data
    ///   Close or Dispose will dispose the underlying stream
    /// </remarks>
    public ImprovedTextReader(in Stream stream, int codePageId = 65001, int skipLines = 0)
    {
      m_Stream = stream;
      m_SkipLines = skipLines;

      try
      {
        _ = Encoding.GetEncoding(codePageId);
        m_CodePageId =codePageId;
      }
      catch (Exception)
      {
        Logger.Warning("Code page {codePageId} not supported, using UTF8", codePageId);
        m_CodePageId = Encoding.UTF8.CodePage;
      }

      // read the BOM if we have seek
      if (m_Stream.CanSeek)
      {
        if (m_Stream.Position != 0L)
          m_Stream.Seek(0, SeekOrigin.Begin);

        // Space for 4 BOM bytes     
#if NET6_0_OR_GREATER
        Span<byte> bomBuffer = stackalloc byte[4];
        int bytesRead = m_Stream.Read(bomBuffer); // read up to 4 bytes

        var intEncodingByBom = EncodingHelper.GetEncodingByByteOrderMark(bomBuffer.Slice(0, bytesRead));
#else
        var bomBuffer = new byte[4];
        int bytesRead = m_Stream.Read(bomBuffer, 0, 4); // read up to 4 bytes

        var intEncodingByBom = EncodingHelper.GetEncodingByByteOrderMark(bomBuffer, bytesRead);
#endif

        if (intEncodingByBom != null)
        {
          m_CodePageId = intEncodingByBom.CodePage;
          m_BomLength = EncodingHelper.BOMLength(m_CodePageId);
        }

        // By reading We have moved the position, so move it back
        PositionAfterBom();
      }

      StreamReader = new StreamReader(m_Stream, Encoding.GetEncoding(m_CodePageId), false, 4096, true);
      AdjustStartLine();
    }

    /// <summary>
    ///   Gets a value indicating whether the current stream supports seeking.
    /// </summary>
    /// <returns><c>true</c> if the stream supports seeking; otherwise, false.</returns>
    public bool CanSeek => m_Stream.CanSeek;

    /// <summary>
    ///   Gets or sets a value indicating whether the reader is at the end of the file.
    /// </summary>
    /// <value><c>true</c> if at the end of file; otherwise, <c>false</c>.</value>
    public bool EndOfStream => StreamReader.EndOfStream;

    /// <summary>
    /// The line number in the current file
    /// </summary>
    public long LineNumber
    {
      get;
      private set;
    }

    /// <summary>
    ///   Gets the stream reader.
    /// </summary>
    /// <value>The stream reader.</value>
    private StreamReader StreamReader { get; set; }

    /// <summary>
    ///   Closes the <see cref="ImprovedTextReader" /> and the underlying stream, and releases any
    ///   system resources associated with the reader.
    /// </summary>
    public void Close()
    {
      // Dispose the StreamReader, but leave the underlying stream open
      StreamReader.Dispose();
      LineNumber = 0;
    }

    /// <summary>
    ///   Increase the position in the text, this is used in case a character that has been looked
    ///   at with <see cref="Peek" /> does not need to be read the next call of <see cref="Read" />
    /// </summary>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    public void MoveNext() => StreamReader.Read();

    /// <summary>
    ///   Gets the next character but does not progress, as this can be done numerous times on the
    ///   same position
    /// </summary>
    /// <returns>
    ///   The next character from the input stream represented as an <see
    ///   cref="T:System.Int32">Int32</see> object, or -1 if no more characters are available.
    /// </returns>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    public int Peek() => StreamReader.Peek();

    /// <summary>
    ///   Reads the next character and progresses one further, and tracks the line number
    /// </summary>
    /// <returns>
    ///   The next character from the input stream represented as an <see
    ///   cref="T:System.Int32">Int32</see> object, or -1 if no more characters are available.
    /// </returns>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    /// <remarks>
    ///   In case the character is a cr or Lf it will increase the lineNumber, to prevent a CR LF
    ///   combination to count as two lines Make sure you "eat" the possible next char using <see
    ///   cref="Peek" /> and <see cref="MoveNext" />
    /// </remarks>
    public int Read()
    {
      var read = StreamReader.Read();
      // Handle CR, LF and CRLF
      if (read == cCr || (read == cLf && m_LastChar != cCr))
        LineNumber++;
      m_LastChar = read;
      return read;
    }


    /// <inheritdoc cref="TextReader" />
    public string ReadLine()
    {
      var line = StreamReader.ReadLine();
      if (line is not null)
        LineNumber++;
      return line ?? string.Empty;
    }

    /// <summary>
    ///   Reads a sequence of characters followed by a linefeed or carriage return or the end of the
    ///   text. The ending char is not part of the returned line
    /// </summary>
    /// <returns>
    ///   A task that represents the asynchronous read operation. The string contains the next line
    ///   from the stream, or is null if all the characters have been read.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   The number of characters in the next line is larger than <see cref="F:System.Int32.MaxValue">MaxValue</see>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">The stream has been disposed.</exception>
    /// <exception cref="InvalidOperationException">
    ///   The reader is currently in use by a previous read operation.
    /// </exception>
    /// <remarks>CR,LF,CRLF will end the line, LFCR is not supported
    /// LFCR  is treated as two line breaks, which is uncommon but can occur in some legacy files</remarks>
    public async Task<string> ReadLineAsync(CancellationToken cancellationToken)
    {
#if NET6_0_OR_GREATER
      var line = await StreamReader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
#else
      var line = await StreamReader.ReadLineAsync().ConfigureAwait(false);
#endif
      if (line is not null)
        LineNumber++;
      return line ?? string.Empty;
    }

    /// <summary>
    ///  Reads a block of characters from the text reader and writes the data to a buffer, beginning at the specified index.
    /// This does not keep track of the line number, use Read, ReadLine or ReadLineAsync for this
    /// </summary>
    /// <param name="buffer">The character array to write the data into.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
    /// <returns>Number of read chars</returns>
#if NET6_0_OR_GREATER

    public ValueTask<int> ReadBlockAsync(Memory<char> buffer, CancellationToken cancellationToken = default)
     => StreamReader.ReadBlockAsync(buffer, cancellationToken);
#else
    public async ValueTask<int> ReadBlockAsync(Memory<char> buffer, CancellationToken cancellationToken = default)
    {
      var temp = System.Buffers.ArrayPool<char>.Shared.Rent(buffer.Length);
      try
      {
        int read = await StreamReader.ReadBlockAsync(temp, 0, buffer.Length).ConfigureAwait(false);
        new Span<char>(temp, 0, read).CopyTo(buffer.Span);
        return read;
      }
      finally
      {
        System.Buffers.ArrayPool<char>.Shared.Return(temp);
      }
    }
#endif

    /// <summary>
    /// Reads a specified maximum number of characters from the current stream and writes the data to a buffer, beginning at the specified index.
    /// This does not keep track of the line number, use Read, ReadLine or ReadLineAsync for this
    /// </summary>
    /// <param name="buffer">When this method returns, contains the specified character array with the values between index and (index + count - 1) replaced by the characters read from the current source.</param>
    /// <param name="index">The position in buffer at which to begin writing.</param>
    /// <param name="count">The maximum number of characters to read.</param>
    /// <returns>The number of characters that have been read. The number will be less than or equal to count, depending on whether all input characters have been read.</returns>
    public int ReadBlock(char[] buffer, int index, int count)
      => StreamReader.ReadBlock(buffer, index, count);

    /// <summary>
    ///   Resets the position of the stream to the beginning, without opening the stream from
    ///   scratch This is fast in case the text fitted into the buffer or the underlying stream
    ///   supports seeking. In case this is not that case it does reopen the text reader
    /// </summary>
    public void ToBeginning()
    {

      if (!m_Stream.CanSeek)
        throw new NotSupportedException("Stream does not allow seek, you can not return to the beginning");

      PositionAfterBom();
      StreamReader.DiscardBufferedData();

      if (LineNumber != 1 || m_SkipLines>0)
        AdjustStartLine();
    }

    /// <inheritdoc cref="IDisposable" />
    protected override void Dispose(bool disposing)
    {
      if (disposing)
        StreamReader.Dispose();
    }

    private void AdjustStartLine()
    {
      LineNumber = 1;
      for (var i = 0; i < m_SkipLines && !StreamReader.EndOfStream; i++)
        _ = ReadLine();
    }
  }
}
