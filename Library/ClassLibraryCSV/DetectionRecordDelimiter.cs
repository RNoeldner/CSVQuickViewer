using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class DetectionRecordDelimiter
  {
    /// <summary>
    ///   Determine the new line sequence from TextReader
    /// </summary>
    /// <param name="textReader">The reader to read data from</param>
    /// <param name="fieldQualifier">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The NewLine Combination used</returns>
    /// <returns>The NewLine Combination used</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static RecordDelimiterTypeEnum InspectRecordDelimiter(
          this ImprovedTextReader textReader, string fieldQualifier, CancellationToken cancellationToken)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));
      const int numChars = 8192;

      var currentChar = 0;
      var quoted = false;

      const int cr = 0;
      const int lf = 1;
      const int crLf = 2;
      const int lfCr = 3;
      const int recSep = 4;
      const int unitSep = 5;

      int[] count = { 0, 0, 0, 0, 0, 0 };

      // \r = CR (Carriage Return) \n = LF (Line Feed)
      var fieldQualifierChar = fieldQualifier.WrittenPunctuationToChar();
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
      while (currentChar < numChars && !textReaderPosition.AllRead() && !cancellationToken.IsCancellationRequested)
      {
        var readChar = textReader.Read();
        if (readChar == fieldQualifierChar)
        {
          if (quoted)
          {
            if (textReader.Peek() != fieldQualifierChar)
              quoted = false;
            else
              textReader.MoveNext();
          }
          else
          {
            quoted = true;
          }
        }

        if (quoted)
          continue;

        switch (readChar)
        {
          case 30:
            count[recSep]++;
            continue;
          case 31:
            count[unitSep]++;
            continue;
          case 10:
          {
            if (textReader.Peek() == 13)
            {
              textReader.MoveNext();
              count[lfCr]++;
            }
            else
            {
              count[lf]++;
            }

            currentChar++;
            break;
          }
          case 13:
          {
            if (textReader.Peek() == 10)
            {
              textReader.MoveNext();
              count[crLf]++;
            }
            else
            {
              count[cr]++;
            }

            break;
          }
        }

        currentChar++;
      }

      var maxCount = count.Max();
      if (maxCount == 0)
        return RecordDelimiterTypeEnum.None;

      var res = RecordDelimiterTypeEnum.None;
      if (count[recSep] == maxCount)
        res = RecordDelimiterTypeEnum.Rs;
      else if (count[unitSep] == maxCount)
        res = RecordDelimiterTypeEnum.Us;
      else if (count[cr] == maxCount)
        res = RecordDelimiterTypeEnum.Cr;
      else if (count[lf] == maxCount)
        res = RecordDelimiterTypeEnum.Lf;
      else if (count[crLf] == maxCount)
        res = RecordDelimiterTypeEnum.Crlf;
      else if (count[lfCr] == maxCount)
        res = RecordDelimiterTypeEnum.Lfcr;
      Logger.Information($"Record Delimiter: {res.Description()}");
      return res;
    }

    /// <summary>
    ///   Determine the new line sequence from Stream
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="codePageId">The code page identifier.</param>
    /// <param name="skipRows">The number of lines at beginning to disregard</param>
    /// <param name="fieldQualifier">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The NewLine Combination used</returns>
    public static async Task<RecordDelimiterTypeEnum> InspectRecordDelimiterAsync(
      this Stream stream, int codePageId, int skipRows,
      string fieldQualifier, CancellationToken cancellationToken)
    {
      using var textReader = new ImprovedTextReader(stream, await stream.InspectCodePageAsync(codePageId, cancellationToken).ConfigureAwait(false), skipRows);
      return textReader.InspectRecordDelimiter(fieldQualifier, cancellationToken);
    }
  }
}