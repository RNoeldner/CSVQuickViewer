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
    private const int c_BufferSize = 65536;

    /// <summary>
    ///   The carriage return character. Escape code is <c>\r</c>.
    /// </summary>
    private const char c_Cr = (char) 0x0d;

    /// <summary>
    ///   The line-feed character. Escape code is <c>\n</c>.
    /// </summary>
    private const char c_Lf = (char) 0x0a;

    /// <summary>
    ///   Buffer of the file data
    /// </summary>
    public readonly char[] Buffer = new char[c_BufferSize];

    private readonly IImprovedStream m_ImprovedStream;

    private readonly int m_SkipLines;

    /// <summary>
    ///   Length of the buffer (can be smaller then buffer size at end of file)
    /// </summary>
    public int BufferFilled;

    /// <summary>
    ///   Position in the buffer
    /// </summary>
    public int BufferPos;

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
    ///   first few bytes of teh source stream to look at a possible existing BOM if found, it will
    ///   overwrite the provided data
    /// </remarks>
    public ImprovedTextReader([NotNull] IImprovedStream improvedStream, int codePageId = 65001, int skipLines = 0)
    {
      //if (improvedStream.Percentage > 0.00001)
      //  throw new ArgumentException(nameof(improvedStream), @"The stream is not on the start position");

      m_SkipLines = skipLines;
      m_ImprovedStream = improvedStream ?? throw new ArgumentNullException(nameof(improvedStream));

      // read the BOM in any case
      var buff = new byte[4];
      m_ImprovedStream.Stream.Read(buff, 0, buff.Length);
      var intCodePageByBom = EncodingHelper.GetCodePageByByteOrderMark(buff);
      improvedStream.ResetToStart(null);

      if (intCodePageByBom != 0)
      {
        ByteOrderMark = true;
        CodePage = (EncodingHelper.CodePage) intCodePageByBom;
      }
      else
      {
        ByteOrderMark = false;

        try
        {
          CodePage = (EncodingHelper.CodePage) codePageId;
        }
        catch (Exception)
        {
          Logger.Warning("Codepage {0} not supported, using UTF8", codePageId);
          CodePage = EncodingHelper.CodePage.UTF8;
        }
      }

      ToBeginningAsync().Wait();
    }

    public Encoding CurrentEncoding => TextReader.CurrentEncoding;

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

    /// <summary>
    ///   Indicates if we did indeed find a Byte Order Mark
    /// </summary>
    public bool ByteOrderMark
    {
      get;
    }

    /// <summary>
    ///   CodePage
    /// </summary>
    private EncodingHelper.CodePage CodePage
    {
      get;
    }

    private StreamReader TextReader { get; set; }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      Dispose(true);
    }

    /// <summary>
    ///   Increase the position in the text, this is used in case a character that has been looked
    ///   at with <see cref="PeekAsync" /> does not need to be read teh next call of
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
      if (BufferPos >= BufferFilled)
        // Prevent marshalling the continuation back to the original context. This is good for
        // performance and to avoid deadlocks
        await ReadIntoBufferAsync().ConfigureAwait(false);
      if (EndOfFile)
        return -1;

      return Buffer[BufferPos];
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

      if (character == c_Lf || character == c_Cr)
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
          case c_Cr:
          case c_Lf:
          {
            var nextChar = await PeekAsync().ConfigureAwait(false);
            switch (character)
            {
              case c_Cr when nextChar == c_Lf:
                MoveNext();
                break;

              case c_Lf when nextChar == c_Cr:
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
    ///   Resets teh position of teh stream to the beginning, without opening teh stream from
    ///   scratch This is fast in case teh text fitted into the buffer or the underlying stream
    ///   supports seeking. In case this is not that cae it does reopen the text reader
    /// </summary>
    public async Task ToBeginningAsync()
    {
      BufferPos = 0;
      LineNumber = 1;

      var addBom = ByteOrderMark ? EncodingHelper.BOMLength(CodePage) : 0;

      var streamLengthNotOk = true;
      // if we have something in the buffer
      if (BufferFilled > 0)
        // using try catch as some streams do not support length, if so need to open from scratch
        try
        {
          streamLengthNotOk = m_ImprovedStream.Stream.Length - addBom > BufferFilled;
        }
        catch (Exception)
        {
          // ignored
        }

      // In case the buffer is bigger than the stream, we do not need to rest
      if (streamLengthNotOk)
      {
        BufferFilled = 0;
        // Some improved stream might need to reopen the streams
        m_ImprovedStream.ResetToStart(delegate(Stream stream)
        {
          // eat the bom
          if (addBom > 0)
            stream.Read(new byte[addBom], 0, addBom);

          // in case we can not seek need to reopen the stream reader
          if (!stream.CanSeek || TextReader == null)
          {
            TextReader?.Dispose();
            TextReader = new StreamReader(stream, Encoding.GetEncoding((int) CodePage), false);
          }
          else
          {
            // discard the buffer
            TextReader.DiscardBufferedData();
          }
        });

        EndOfFile = TextReader.EndOfStream;
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
      EndOfFile = TextReader.EndOfStream;
      if (EndOfFile)
        return;
      BufferFilled = await TextReader.ReadAsync(Buffer, 0, c_BufferSize).ConfigureAwait(false);
      BufferPos = 0;
    }
  }
}