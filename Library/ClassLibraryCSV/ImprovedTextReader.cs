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

    private readonly int m_BomLength = 0;
    private readonly int m_SkipLines;
    private readonly Stream m_Stream;
    private bool m_LastCharWasCR = false;

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
    /// </remarks>
    public ImprovedTextReader(in Stream stream, int codePageId = 65001, int skipLines = 0)
    {
      m_Stream = stream;
      m_SkipLines = skipLines;

      try
      {
        _ = Encoding.GetEncoding(codePageId);
      }
      catch (Exception)
      {
        Logger.Warning("Code page {codePageId} not supported, using UTF8", codePageId);
        codePageId = Encoding.UTF8.CodePage;
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
          codePageId = intEncodingByBom.CodePage;
          m_BomLength = EncodingHelper.BOMLength(codePageId);
        }
      }

      StreamReader = new StreamReader(m_Stream, Encoding.GetEncoding(codePageId), false, 4096, true);
      if (m_Stream.CanSeek)
        ToBeginning();
      else
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
    private StreamReader StreamReader { get; }

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
      while (true)
      {
        int ch = StreamReader.Read();
        if (ch == -1)
        {
          m_LastCharWasCR = false;
          return -1;
        }

        char c = (char) ch;

        if (m_LastCharWasCR && c == cLf)
        {
          m_LastCharWasCR = false;
          continue; // skip LF
        }

        m_LastCharWasCR = (c == cCr);
        if (c == cCr || c == cLf)
          LineNumber++;

        return ch;
      }
    }


    /// <inheritdoc cref="TextReader" />
    [Obsolete("Better use ReadLineAsync ")]
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
    ///  This does not keep track of the line number
    /// </summary>
    /// <param name="readBuffer">Usage like Memory<char> buffer = new char[128] or ArrayPool<char>.Shared.Rent(4096)</param>
    /// <returns>Number of read chars</returns>
#if NET6_0_OR_GREATER
   
    public ValueTask<int> ReadBlockAsync(Memory<char> buffer, CancellationToken cancellationToken = default)
     => StreamReader.ReadBlockAsync(buffer, cancellationToken);
#else
    public async ValueTask<int> ReadBlockAsync(Memory<char> buffer, CancellationToken cancellationToken = default)
    {
      char[] temp = System.Buffers.ArrayPool<char>.Shared.Rent(buffer.Length);
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

      if (m_Stream.Position != m_BomLength)
      {
        m_Stream.Seek(0, SeekOrigin.Begin);
        // eat the bom
        if (m_BomLength > 0 && m_Stream.CanRead)
        {
#if NET6_0_OR_GREATER
          Span<byte> bom = stackalloc byte[m_BomLength];
          _ = m_Stream.Read(bom);
#else
          byte[] bom = new byte[m_BomLength];
          _ = m_Stream.Read(bom, 0, m_BomLength);
#endif
        }
        StreamReader.DiscardBufferedData();
      }

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
      {
        StreamReader.ReadLine();
        LineNumber++;
      }
    }
  }
}