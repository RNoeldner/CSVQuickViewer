using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CsvTools
{
  public sealed class ImprovedTextReader : IDisposable
  {
    // Buffer size set to 64kB, if set to large the display in percentage will jump
    private const int cBufferSize = 32768;

    /// <summary>
    ///   The carriage return character. Escape code is <c>\r</c>.
    /// </summary>
    private const char cCr = (char) 0x0d;

    /// <summary>
    ///   The line-feed character. Escape code is <c>\n</c>.
    /// </summary>
    private const char cLf = (char) 0x0a;

    private readonly int m_BomLength;

    /// <summary>
    ///   Buffer of the file data
    /// </summary>
    private readonly char[] m_Buffer = new char[cBufferSize];

    private readonly EncodingHelper.CodePage m_CodePage;

    private readonly Stream m_ImprovedStream;
    private readonly int m_SkipLines;

    /// <summary>
    ///   Position in the buffer
    /// </summary>
    public int BufferPos;

    /// <summary>
    ///   Length of the buffer (can be smaller then buffer size at end of file)
    /// </summary>
    private int m_BufferFilled;

    private bool m_DisposedValue;

    /// <summary>
    ///   Creates an instance of the TextReader
    /// </summary>
    /// <param name="improvedStream">An Improved Stream</param>
    /// <param name="codePageId">The assumed code page id</param>
    /// <param name="skipLines">
    ///   Number of lines that should be skipped at the beginning of the file
    /// </param>
    /// <remarks>
    ///   This routine uses a TextReader to allow character decoding, it will always read they teh
    ///   first few bytes of the source stream to look at a possible existing BOM if found, it will
    ///   overwrite the provided data
    /// </remarks>
    public ImprovedTextReader([NotNull] IImprovedStream improvedStream, int codePageId = 65001, int skipLines = 0)
    {
      m_SkipLines = skipLines;
      m_ImprovedStream = improvedStream as Stream ?? throw new ArgumentNullException(nameof(improvedStream));

      // read the BOM in any case
      var buff = new byte[4];
      m_ImprovedStream.Read(buff, 0, buff.Length);
      var intCodePageByBom = EncodingHelper.GetCodePageByByteOrderMark(buff);
      improvedStream.Seek(0, SeekOrigin.Begin);
      var byteOrderMark = false;

      if (intCodePageByBom != EncodingHelper.CodePage.None)
      {
        byteOrderMark = true;
        m_CodePage = intCodePageByBom;
      }
      else
      {
        try
        {
          m_CodePage = (EncodingHelper.CodePage) codePageId;
        }
        catch (Exception)
        {
          Logger.Warning("Codepage {0} not supported, using UTF8", codePageId);
          m_CodePage = EncodingHelper.CodePage.UTF8;
        }
      }

      m_BomLength = byteOrderMark ? EncodingHelper.BOMLength(m_CodePage) : 0;
    }


    /// <summary>
    ///   Gets or sets a value indicating whether the reader is at the end of the file.
    /// </summary>
    /// <value><c>true</c> if at the end of file; otherwise, <c>false</c>.</value>
    public bool EndOfFile { get; private set; } = true;

    public long LineNumber
    {
      get;
      private set;
    }

    private StreamReader TextReader { get; set; }

    // This code added to correctly implement the disposable pattern.
    public void Dispose() => Dispose(true);

    /// <summary>
    ///   Increase the position in the text, this is used in case a character that has been looked
    ///   at with <see cref="PeekAsync" /> does not need to be read the next call of
    ///   <see
    ///     cref="ReadAsync" />
    /// </summary>
    public void MoveNext() => BufferPos++;

    /// <summary>
    ///   Gets the next character but does not progress, as this can be done numerous times on the
    ///   same position
    /// </summary>
    /// <returns></returns>
    public async Task<int> PeekAsync()
    {
      if (BufferPos >= m_BufferFilled)
        // Prevent marshalling the continuation back to the original context. This is good for
        // performance and to avoid deadlocks
        await ReadIntoBufferAsync().ConfigureAwait(false);
      if (EndOfFile)
        return -1;

      return m_Buffer[BufferPos];
    }

    /// <summary>
    ///   Reads the next character and progresses one further, and tracks the line number
    /// </summary>
    /// <remarks>
    ///   In case the character is a cr or Lf it will increase the lineNumber, to prevent a CR LF
    ///   combination to count as two lines Make sure you "eat" the possible next char using
    ///   <see
    ///     cref="PeekAsync" />
    ///   and <see cref="MoveNext" />
    /// </remarks>
    /// <returns></returns>
    public async Task<int> ReadAsync()
    {
      var character = await PeekAsync().ConfigureAwait(false);

      if (character == cLf || character == cCr)
        LineNumber++;

      if (EndOfFile)
        return -1;
      MoveNext();
      return character;
    }

    /// <summary>
    ///   Reads a sequence of characters followed by a linefeed or carriage return or the end of the
    ///   text. The ending char is not part of the returned line
    /// </summary>
    /// <remarks>CR,LF,CRLF or LFCR will end the line</remarks>
    /// <returns>A string with the contents, or NULL if nothing was read</returns>
    public async Task<string> ReadLineAsync()
    {
      var sb = new StringBuilder();
      while (!EndOfFile)
      {
        var character = await ReadAsync().ConfigureAwait(false);
        switch (character)
        {
          case -1:
            continue;
          case cCr:
          case cLf:
          {
            var nextChar = await PeekAsync().ConfigureAwait(false);
            switch (character)
            {
              case cCr when nextChar == cLf:
                MoveNext();
                break;

              case cLf when nextChar == cCr:
                LineNumber++;
                MoveNext();
                break;
            }

            return sb.ToString();
          }
          default:
            sb.Append((char) character);
            break;
        }
      }

      return sb.Length > 0 ? sb.ToString() : null;
    }

    /// <summary>
    ///   Resets the position of the stream to the beginning, without opening the stream from
    ///   scratch This is fast in case the text fitted into the buffer or the underlying stream
    ///   supports seeking. In case this is not that cae it does reopen the text reader
    /// </summary>
    public async Task ToBeginningAsync()
    {
      BufferPos = 0;
      LineNumber = 1;

      // In case the buffer is bigger than the stream, we do not need to rest
      if (TextReader == null || m_BufferFilled <= 0 || m_ImprovedStream.Length - m_BomLength > m_BufferFilled)
      {
        m_BufferFilled = 0;
        m_ImprovedStream.Seek(0, SeekOrigin.Begin);

        // eat the bom
        if (m_BomLength > 0 && m_ImprovedStream.CanRead)
          await m_ImprovedStream.ReadAsync(new byte[m_BomLength], 0, m_BomLength).ConfigureAwait(false);

        // in case we can not seek need to reopen the stream reader
        if (TextReader == null)
          TextReader = new StreamReader(m_ImprovedStream, Encoding.GetEncoding((int) m_CodePage), false, 4096, true);
        // discard the buffer
        else
          TextReader.DiscardBufferedData();

        EndOfFile = TextReader?.EndOfStream ?? true;
      }
      else
      {
        EndOfFile = false;
      }

      for (var i = 0; i < m_SkipLines && !EndOfFile; i++)
        await ReadLineAsync().ConfigureAwait(false);
    }

    private void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      EndOfFile = true;
      if (disposing)
        TextReader?.Dispose();
      m_DisposedValue = true;
    }

    /// <summary>
    ///   Read the data from the text reader into the buffer
    /// </summary>
    /// <returns></returns>
    private async Task ReadIntoBufferAsync()
    {
      try
      {
        EndOfFile = TextReader.EndOfStream;
        if (EndOfFile)
          return;
        m_BufferFilled = await TextReader.ReadAsync(m_Buffer, 0, cBufferSize).ConfigureAwait(false);
        BufferPos = 0;
      }
      catch (Exception)
      {
        EndOfFile = true;
      }
    }
  }
}