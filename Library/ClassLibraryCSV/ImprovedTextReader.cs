using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   Wrapper around a TestReader that handles BOM and Encoding and a has a method called
  ///   ToBeginning to reset to the reader to the start of the stream
  /// </summary>
  public sealed class ImprovedTextReader : IDisposable
  {
    /// <summary>
    ///   The carriage return character. Escape code is <c>\r</c>.
    /// </summary>
    private const char c_Cr = (char) 0x0d;

    /// <summary>
    ///   The line-feed character. Escape code is <c>\n</c>.
    /// </summary>
    private const char c_Lf = (char) 0x0a;
    private readonly int m_BomLength;
    private readonly int m_CodePage;
    private readonly Stream m_ImprovedStream;
    private readonly int m_SkipLines;
    private bool m_DisposedValue;
    private bool m_Init = true;

    /// <summary>
    ///   Creates an instance of the TextReader
    /// </summary>
    /// <param name="improvedStream">An Improved Stream</param>
    /// <param name="codePageId">The assumed code page id</param>
    /// <param name="skipLines">
    ///   Number of lines that should be skipped at the beginning of the file
    /// </param>
    /// <remarks>
    ///   This routine uses a TextReader to allow character decoding, it will always read they
    ///   the first few bytes of the source stream to look at a possible existing BOM if found,
    ///   it will overwrite the provided data
    /// </remarks>
#pragma warning disable 8618

    public ImprovedTextReader(in IImprovedStream improvedStream, int codePageId = 65001, int skipLines = 0)
#pragma warning restore 8618
    {
      m_SkipLines = skipLines;
      m_ImprovedStream = improvedStream as Stream ?? throw new ArgumentNullException(nameof(improvedStream));

      // read the BOM in any case
      var buff = new byte[4];
      m_ImprovedStream.Read(buff, 0, buff.Length);
      var intCodePageByBom = EncodingHelper.GetEncodingByByteOrderMark(buff);
      improvedStream.Seek(0, SeekOrigin.Begin);
      var byteOrderMark = false;

      if (intCodePageByBom != null)
      {
        byteOrderMark = true;
        m_CodePage = intCodePageByBom.CodePage;
      }
      else
      {
        try
        {
          m_CodePage = codePageId;
        }
        catch (Exception)
        {
          Logger.Warning("Codepage {0} not supported, using UTF8", codePageId);
          m_CodePage = Encoding.UTF8.CodePage;
        }
      }

      m_BomLength = byteOrderMark ? EncodingHelper.BOMLength(m_CodePage) : 0;
      ToBeginning();
    }

    /// <summary>
    ///   Gets or sets a value indicating whether the reader is at the end of the file.
    /// </summary>
    /// <value><c>true</c> if at the end of file; otherwise, <c>false</c>.</value>
    public bool EndOfStream => TextReader.EndOfStream;

    public long LineNumber
    {
      get;
      private set;
    }

    private StreamReader TextReader { get; set; }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   Increase the position in the text, this is used in case a character that has been
    ///   looked at with <see cref="Peek" /> does not need to be read the next call of <see
    ///   cref="Read" />
    /// </summary>
    public void MoveNext() => TextReader.Read();

    /// <summary>
    ///   Gets the next character but does not progress, as this can be done numerous times on
    ///   the same position
    /// </summary>
    /// <returns></returns>
    public int Peek() => TextReader.Peek();

    /// <summary>
    ///   Reads the next character and progresses one further, and tracks the line number
    /// </summary>
    /// <remarks>
    ///   In case the character is a cr or Lf it will increase the lineNumber, to prevent a CR
    ///   LF combination to count as two lines Make sure you "eat" the possible next char using
    ///   <see cref="Peek" /> and <see cref="MoveNext" />
    /// </remarks>
    /// <returns></returns>
    public int Read()
    {
      var character = TextReader.Read();

      if (character == c_Lf || character == c_Cr)
        LineNumber++;

      return character;
    }

    /// <summary>
    ///   Reads a sequence of characters followed by a linefeed or carriage return or the end of
    ///   the text. The ending char is not part of the returned line
    /// </summary>
    /// <remarks>CR,LF,CRLF will end the line, LFCR is not supported</remarks>
    /// <returns>A string with the contents, or empty string if nothing was read</returns>
    public async Task<string> ReadLineAsync()
    {

      LineNumber++;
      return await TextReader.ReadLineAsync();
    }


    /// <summary>
    ///   Resets the position of the stream to the beginning, without opening the stream from
    ///   scratch This is fast in case the text fitted into the buffer or the underlying stream
    ///   supports seeking. In case this is not that cae it does reopen the text reader
    /// </summary>
    public void ToBeginning()
    {
      LineNumber = 1;

      m_ImprovedStream.Seek(0, SeekOrigin.Begin);

      // eat the bom
      if (m_BomLength > 0 && m_ImprovedStream.CanRead)
        m_ImprovedStream.Read(new byte[m_BomLength], 0, m_BomLength);

      // in case we can not seek need to reopen the stream reader
      if (m_Init)
      {
        TextReader = new StreamReader(m_ImprovedStream, Encoding.GetEncoding(m_CodePage), false, 4096, true);
        m_Init = true;
      }
      // discard the buffer
      else
      {
        TextReader.DiscardBufferedData();
      }

      for (var i = 0; i < m_SkipLines && !TextReader.EndOfStream; i++)
      {
        TextReader.ReadLine();
        LineNumber++;
      }            
    }

    private void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (disposing)
        TextReader.Dispose();
      m_DisposedValue = true;
    }
  }
}