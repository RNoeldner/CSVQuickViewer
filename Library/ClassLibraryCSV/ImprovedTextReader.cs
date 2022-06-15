using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   Wrapper around a TestReader that handles BOM and Encoding and a has a method called
  ///   ToBeginning to reset to the reader to the start of the stream
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
    private readonly Stream m_Stream;
    private readonly int m_SkipLines;

    /// <summary>
    ///   Creates an instance of the TextReader
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <param name="codePageId">The assumed code page id</param>
    /// <param name="skipLines">
    ///   Number of lines that should be skipped at the beginning of the file
    /// </param>
    /// <remarks>
    ///   This routine uses a TextReader to allow character decoding, it will always read they the
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
        Logger.Warning("Codepage {0} not supported, using UTF8", codePageId);
        codePageId = Encoding.UTF8.CodePage;
      }

      // read the BOM if we have seek
      if (m_Stream.CanSeek)
      {
        if (m_Stream.Position != 0L)
          m_Stream.Seek(0, SeekOrigin.Begin);

        // Space for 4 BOM bytes
        var buff = new byte[4];

        // read only 2 to start
        _ = m_Stream.Read(buff, 0, 2);
        var intEncodingByBom = EncodingHelper.GetEncodingByByteOrderMark(buff, 2);
        // unfortunately Encoding.Unicode can not be determined by only 2 bytes as UTF-32 starts
        // with the same BOM
        if (intEncodingByBom is null || intEncodingByBom.Equals(Encoding.Unicode))
        {
          // read the 3th byte
          _ = m_Stream.Read(buff, 2, 1);
          intEncodingByBom = EncodingHelper.GetEncodingByByteOrderMark(buff, 3);
          if (intEncodingByBom is null || intEncodingByBom.Equals(Encoding.Unicode))
          {
            // read the 4th byte
            _ = m_Stream.Read(buff, 3, 1);
            intEncodingByBom = EncodingHelper.GetEncodingByByteOrderMark(buff, 4);
          }
        }

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
    ///   Gets or sets a value indicating whether the reader is at the end of the file.
    /// </summary>
    /// <value><c>true</c> if at the end of file; otherwise, <c>false</c>.</value>
    public bool EndOfStream => StreamReader.EndOfStream;

    /// <summary>
    ///   Closes the <see cref="ImprovedTextReader" /> and the underlying stream, and releases any
    ///   system resources associated with the reader.
    /// </summary>
    public void Close() => StreamReader.Close();

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
      var character = StreamReader.Read();
      if (character == cLf || character == cCr)
        LineNumber++;

      return character;
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
    /// <remarks>CR,LF,CRLF will end the line, LFCR is not supported</remarks>
    public async Task<string> ReadLineAsync()
    {
      LineNumber++;
      return await StreamReader.ReadLineAsync();
    }

    /// <summary>
    ///   Resets the position of the stream to the beginning, without opening the stream from
    ///   scratch This is fast in case the text fitted into the buffer or the underlying stream
    ///   supports seeking. In case this is not that cae it does reopen the text reader
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
          // ReSharper disable once MustUseReturnValue
          m_Stream.Read(new byte[m_BomLength], 0, m_BomLength);
        StreamReader.DiscardBufferedData();
      }

      AdjustStartLine();
    }

    private void AdjustStartLine()
    {
      LineNumber = 1;
      for (var i = 0; i<m_SkipLines && !StreamReader.EndOfStream; i++)
      {
        StreamReader.ReadLine();
        LineNumber++;
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the current stream supports seeking.
    /// </summary>
    /// <returns><c>true</c> if the stream supports seeking; otherwise, false.</returns>
    public bool CanSeek => m_Stream.CanSeek;

    /// <inheritdoc cref="IDisposable" />
    protected override void Dispose(bool disposing)
    {
      if (disposing)
        StreamReader.Dispose();
    }
  }
}