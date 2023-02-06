using System;
using System.Collections.Generic;
using System.Threading;

namespace CsvTools
{
  public static class DetectionStartRow
  {
    /// <summary>
    ///   Guess the start row of a CSV file done with a rather simple csv parsing
    /// </summary>
    /// <param name="textReader">The text reader to read the data</param>
    /// <param name="fieldDelimiterChar">The delimiter.</param>
    /// <param name="fieldQualifierChar">The quoting char</param>
    /// <param name="escapePrefixChar">The start of an escape sequence to allow delimiter or qualifier in column</param>
    /// <param name="commentLine">The characters for a comment line.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The number of rows to skip</returns>
    /// <exception cref="ArgumentNullException">commentLine</exception>
    public static int InspectStartRow(
      this ImprovedTextReader textReader,
      char fieldDelimiterChar,
      char fieldQualifierChar,
      char escapePrefixChar,
      string commentLine,
      CancellationToken cancellationToken)
    {
      if (textReader is null)
        throw new ArgumentNullException(nameof(textReader));
      if (commentLine is null)
        throw new ArgumentNullException(nameof(commentLine));
      const int maxRows = 50;

      textReader.ToBeginning();
      var columnCount = new List<int>(maxRows);
      var rowMapping = new Dictionary<int, int>(maxRows);
      var colCount = new int[maxRows];
      var isComment = new bool[maxRows];
      var quoted = false;
      var firstChar = true;
      var currentRow = 0;
      var retValue = 0;

      while (currentRow < maxRows && !textReader.EndOfStream && !cancellationToken.IsCancellationRequested)
      {
        var readChar = textReader.Read();
        if (readChar==' ' || readChar == char.MinValue)
          continue;

        // Handle Commented lines
        if (firstChar && commentLine.Length > 0 && readChar == commentLine[0])
        {
          isComment[currentRow] = true;

          for (var pos = 1; pos < commentLine.Length; pos++)
          {
            var nextChar = textReader.Peek();
            if (nextChar == commentLine[pos])
            {
              textReader.MoveNext();
              continue;
            }
            isComment[currentRow] = false;
            break;
          }
        }

        if (readChar == escapePrefixChar && !isComment[currentRow])
          continue;

        // Handle Quoting
        if (readChar == fieldQualifierChar && !isComment[currentRow])
        {
          if (quoted)
          {
            if (textReader.Peek() != fieldQualifierChar)
              quoted = false;
            else
              textReader.MoveNext();
          }
          else
            quoted |= firstChar;
          continue;
        }

        switch (readChar)
        {
          // Feed and NewLines
          case '\n':
            if (!quoted)
            {
              currentRow++;
              firstChar = true;
              if (textReader.Peek() == '\r')
                textReader.MoveNext();
            }
            break;

          case '\r':
            if (!quoted)
            {
              currentRow++;
              firstChar = true;
              if (textReader.Peek() == '\n')
                textReader.MoveNext();
            }
            break;

          default:
            if (!isComment[currentRow] && !quoted && readChar == fieldDelimiterChar)
            {
              colCount[currentRow]++;
              firstChar = true;
            }
            break;
        }
      }

      cancellationToken.ThrowIfCancellationRequested();
      // remove all rows that are comment lines...
      for (var row = 0; row < currentRow; row++)
      {
        rowMapping[columnCount.Count] = row;
        if (!isComment[row])
          columnCount.Add(colCount[row]);
      }

      // if we do not more than 4 proper rows do nothing
      if (columnCount.Count > 4)
      {
        // In case we have a row that is exactly twice as long as the row before and row after,
        // assume its missing a linefeed
        for (var row = 1; row < columnCount.Count - 1; row++)
          if (columnCount[row + 1] > 0 && columnCount[row] == columnCount[row + 1] * 2
                                       && columnCount[row] == columnCount[row - 1] * 2)
            columnCount[row] = columnCount[row + 1];
        cancellationToken.ThrowIfCancellationRequested();
        // Get the average of the last 15 rows
        var num = 0;
        var sum = 0;
        for (var row = columnCount.Count - 1; num < 10 && row > 0; row--)
        {
          if (columnCount[row] <= 0)
            continue;
          sum += columnCount[row];
          num++;
        }

        var avg = (int) (sum / (double) (num == 0 ? 1 : num));
        // If there are not many columns do not try to guess
        if (avg > 1 && columnCount[0] < avg)
        {
          for (var row = columnCount.Count - 1; row > 0; row--)
            if (columnCount[row] > 0)
            {
              if (columnCount[row] >= avg - (avg / 10)) continue;
              retValue = rowMapping[row];
              break;
            }
            // In case we have an empty line but the next line are roughly good match take that
            // empty line
            else if (row + 2 < columnCount.Count && columnCount[row + 1] == columnCount[row + 2]
                                                 && columnCount[row + 1] >= avg - 1)
            {
              retValue = rowMapping[row + 1];
              break;
            }

          if (retValue == 0)
            for (var row = 0; row < columnCount.Count; row++)
              if (columnCount[row] > 0)
              {
                retValue = rowMapping[row];
                break;
              }
        }
      }

      Logger.Information($"Start Row: {retValue}");
      return retValue;
    }
  }
}