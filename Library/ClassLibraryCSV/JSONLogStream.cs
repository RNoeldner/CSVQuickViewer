using JetBrains.Annotations;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   This stream, wraps a stream into a steream satrting with '[' and trailing ']' seperating elements with ',' 
  ///   This way the JSON log becomes a proper formatted JSON file
  /// </summary>
  public class JSONLogStreamReader : TextReader, IDisposable
  {
    [NotNull] private readonly StreamReader m_StreamReader;
    private bool hasStarted;
    private bool m_AtStart = true;
    private bool m_DisposedValue = false;
    private int m_OpenCurly;
    private int m_OpenSquare;

    public JSONLogStreamReader([NotNull] StreamReader streamReader) => m_StreamReader=streamReader ?? throw new ArgumentNullException(nameof(streamReader));

		public bool EndOfStream { get; private set; } = false;

    // Summary: Closes the System.IO.StreamReader object and the underlying stream, and releases any
    // system resources associated with the reader.
    public override void Close() => m_StreamReader.Close();

    // Summary: Clears the internal buffer.
    public void DiscardBufferedData()
    {
      m_AtStart = true;
      EndOfStream=false;
      m_StreamReader.DiscardBufferedData();
    }
    public new void Dispose() => Dispose(true);

    // Summary: Reads the next character from the input stream and advances the character position
    // by one character.
    //
    // Returns: The next character from the input stream represented as an System.Int32 object, or
    // -1 if no more characters are available.
    //
    // Exceptions: T:System.IO.IOException: An I/O error occurs.
    public override int Read()
    {
      if (EndOfStream)
        return -1;

      if (m_AtStart)
      {
        m_AtStart=false;
        return '[';
      }
      if (m_StreamReader.EndOfStream)
      {
        EndOfStream = true;
        return ']';
      }
      if (m_OpenCurly==0 && m_OpenSquare==0 && hasStarted)
      {
        hasStarted= false;
        return ',';
      }
      var chr = m_StreamReader.Read();
      switch (chr)
      {
        case '{':
          hasStarted=true;
          m_OpenCurly++;
          break;

        case '}':
          m_OpenCurly--;
          break;

        case '[':
          hasStarted=true;
          m_OpenSquare++;
          break;

        case ']':
          m_OpenSquare--;
          break;
      }

      // Get char from base stream
      return chr;
    }

    // Summary: Reads a specified maximum of characters from the current stream into a buffer,
    // beginning at the specified index.
    //
    // Parameters: buffer: When this method returns, contains the specified character array with the
    // values between index and (index + count - 1) replaced by the characters read from the current source.
    //
    // index: The index of buffer at which to begin writing.
    //
    // count: The maximum number of characters to read.
    //
    // Returns: The number of characters that have been read, or 0 if at the end of the stream and
    // no data was read. The number will be less than or equal to the count parameter, depending on
    // whether the data is available within the stream.
    //
    // Exceptions: T:System.ArgumentException: The buffer length minus index is less than count.
    //
    // T:System.ArgumentNullException: buffer is null.
    //
    // T:System.ArgumentOutOfRangeException: index or count is negative.
    //
    // T:System.IO.IOException: An I/O error occurs, such as the stream is closed.
    public override int Read(char[] buffer, int index, int count)
    {
      if (buffer==null) throw new ArgumentNullException(nameof(buffer));
      if (index<0) new ArgumentOutOfRangeException(nameof(index));
      if (count<0) new ArgumentOutOfRangeException(nameof(count));
      if (buffer.Length-index<count) new ArgumentException(nameof(count));

      int charsRead = 0;
      for (int charPos = index; charsRead<count; charPos++)
      {
        buffer[charPos]= (char) Read();
        charsRead++;
        if (EndOfStream)
          break;
      }
      return charsRead;
    }

    // Summary: Reads a specified maximum number of characters from the current stream
    // asynchronously and writes the data to a buffer, beginning at the specified index.
    //
    // Parameters: buffer: When this method returns, contains the specified character array with the
    // values between index and (index + count - 1) replaced by the characters read from the current source.
    //
    // index: The position in buffer at which to begin writing.
    //
    // count: The maximum number of characters to read. If the end of the stream is reached before
    // the specified number of characters is written into the buffer, the current method returns.
    //
    // Returns: A task that represents the asynchronous read operation. The value of the TResult
    // parameter contains the total number of characters read into the buffer. The result value can
    // be less than the number of characters requested if the number of characters currently
    // available is less than the requested number, or it can be 0 (zero) if the end of the stream
    // has been reached.
    //
    // Exceptions: T:System.ArgumentNullException: buffer is null.
    //
    // T:System.ArgumentOutOfRangeException: index or count is negative.
    //
    // T:System.ArgumentException: The sum of index and count is larger than the buffer length.
    //
    // T:System.ObjectDisposedException: The stream has been disposed.
    //
    // T:System.InvalidOperationException: The reader is currently in use by a previous read operation.
    public override async Task<int> ReadAsync(char[] buffer, int index, int count) => await Task.FromResult(Read(buffer, index, count));

    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (!disposing) return;
      m_DisposedValue = true;
      Close();
      m_StreamReader.Dispose();
    }
  }
}