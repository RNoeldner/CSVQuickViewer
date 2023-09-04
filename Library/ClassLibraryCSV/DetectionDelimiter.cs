using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class DetectionDelimiter
  {
    /// <summary>
    ///   Guesses the delimiter for a files. Done with a rather simple csv parsing, and trying to
    ///   find the delimiter that has the least variance in the read rows, if they are the same it will look at 
    ///   the positioning (Score), as a delimiter is preceded by a text or by a quote will increase the score.
    /// </summary>
    /// <param name="textReader">The text reader to read the data</param>
    /// <param name="fieldQualifierChar">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="escapePrefixChar">The start of an escape sequence to allow delimiter or qualifier in column</param>
    /// <param name="disallowedDelimiter">Character rules out as possible delimiters</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>A character with the assumed delimiter for the file</returns>
    /// <exception cref="ArgumentNullException">streamReader</exception>
    /// <remarks>No Error will not be thrown.</remarks>
    public static async Task<DelimiterDetection> InspectDelimiterAsync(
      this ImprovedTextReader textReader,
      char fieldQualifierChar,
      char escapePrefixChar,
      IEnumerable<char> disallowedDelimiter,
      CancellationToken cancellationToken)
    {
      if (textReader is null)
        throw new ArgumentNullException(nameof(textReader));
      var match = char.MinValue;

      if (textReader.CanSeek)
      {
        // Read the first line and check if it does contain the magic word sep=
        var firstLine = (await textReader.ReadLineAsync().ConfigureAwait(false)).Trim().Replace(" ", "");
        if (firstLine.StartsWith("sep=", StringComparison.OrdinalIgnoreCase) && firstLine.Length > 4)
        {
          var resultFl = firstLine.Substring(4);
          if (resultFl.Equals("\\t", StringComparison.OrdinalIgnoreCase))
            resultFl = "Tab";
          Logger.Information($"Delimiter from 'sep=' in first line: {resultFl}");
          return new DelimiterDetection(resultFl.FromText(), true, true);
        }

        textReader.ToBeginning();
      }

      var delimiterCounter = textReader.GetDelimiterCounter(fieldQualifierChar, escapePrefixChar, 300, disallowedDelimiter, cancellationToken);
      var numberOfRows = delimiterCounter.FilledRows;

      // Limit everything to 100 columns max, the sum might get too big otherwise 100 * 100
      var startRow = delimiterCounter.LastRow > 60 ? 15 : delimiterCounter.LastRow > 20 ? 5 : 0;

      var neededRows = (delimiterCounter.FilledRows > 20 ? numberOfRows * 75 : numberOfRows * 50) / 100;
      if (neededRows==1 && delimiterCounter.FilledRows > 1)
        neededRows++;

      cancellationToken.ThrowIfCancellationRequested();
      var validSeparatorIndex = new List<int>();
      for (var index = 0; index < delimiterCounter.Separators.Length; index++)
      {
        // only regard a delimiter if we have 75% of the rows with this delimiter we can still have
        // a lot of commented lines
        if (delimiterCounter.SeparatorRows[index] == 0 || (delimiterCounter.SeparatorRows[index] < neededRows && numberOfRows > 3))
          continue;
        validSeparatorIndex.Add(index);
      }

      if (validSeparatorIndex.Count == 0)
      {
        // we can not determine by the number of rows That the delimiter with most occurrence in general
        var maxNum = int.MinValue;
        for (var index = 0; index < delimiterCounter.Separators.Length; index++)
        {
          var sumCount = 0;
          for (var row = startRow; row < delimiterCounter.LastRow; row++)
            sumCount += delimiterCounter.SeparatorsCount[index, row];
          if (sumCount > maxNum)
          {
            maxNum = sumCount;
            match = delimiterCounter.Separators[index];
          }
        }
      }
      else if (validSeparatorIndex.Count == 1)
      {
        // if only one was found done here
        match = delimiterCounter.Separators[validSeparatorIndex[0]];
      }
      else
      {
        // otherwise find the best
        var sums = new Dictionary<int, long>();        
        foreach (var index in validSeparatorIndex)
        {
          var intEmptyRows = 0;
          var totalRows = (double) (delimiterCounter.LastRow - startRow);
          var sumCount = 0;
          // If there are enough rows skip the first rows, there might be a descriptive introduction
          // this can not be done in case there are not many rows
          for (var row = startRow; row < delimiterCounter.LastRow; row++)
          {
            cancellationToken.ThrowIfCancellationRequested();
            // Cut of at 50 Columns in case one row is messed up, this should not mess up everything
            sumCount += delimiterCounter.SeparatorsCount[index, row];
            if (delimiterCounter.SeparatorsCount[index, row] == 0)
              intEmptyRows++;
          }

          // if a lot rows do not have a columns disregard the delimiter
          if (intEmptyRows  > totalRows * 2 /3)
            continue;

          // Get the average of the rows
          var avg = (int) Math.Ceiling(sumCount / totalRows);

          // Only proceed if there is usually more then one occurrence and we have more then one row
          if (avg < 1 || delimiterCounter.SeparatorRows[index] == 1)
            continue;

          // First determine the variance, low value means and even distribution
          long variance = 0;
          for (var row = startRow; row < delimiterCounter.LastRow; row++)
          {
            if (delimiterCounter.SeparatorsCount[index, row]==avg)
              continue;

            if (delimiterCounter.SeparatorsCount[index, row] > avg)
              variance += delimiterCounter.SeparatorsCount[index, row] - avg;
            else
              variance += avg - delimiterCounter.SeparatorsCount[index, row];
          }

          sums.Add(index, variance);
        }

        if (sums.Count > 1)
        {
          foreach (var kv in sums)
            Logger.Information($"Multiple Possible Separator {delimiterCounter.Separators[kv.Key].Text()}");
        }
        if (sums.Count!= 0)
          // get the best result by variance first then if equal by number of records
          match  =  delimiterCounter.Separators[sums
                          .OrderBy(x => x.Value)
                          .ThenByDescending(x => delimiterCounter.SeparatorScore[x.Key]).First().Key];
      }

      if (match == char.MinValue)
      {
        Logger.Information("Not a delimited file");
        return new DelimiterDetection('\t', false, false);
      }

      Logger.Information($"Column Delimiter: {match.Text()}");
      return new DelimiterDetection(match, true, false);
    }


    /// <summary>Counts the delimiters in DelimiterCounter</summary>
    /// <param name="textReader">The text reader to read the data</param>
    /// <param name="quoteCharacter">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="escapeCharacter">The start of an escape sequence to allow delimiter or qualifier in column</param>
    /// <param name="numRows">The number of rows to read</param>
    /// <param name="disallowedDelimiter">You can pass in delimiters that should not be detected, 
    /// if you know that a delimiter is defiantly not suitable.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>
    ///   A <see cref="DelimiterCounter" /> with the information on delimiters
    /// </returns>
    /// <exception cref="System.ArgumentNullException">textReader</exception>
    private static DelimiterCounter GetDelimiterCounter(
      this ImprovedTextReader textReader,
      char quoteCharacter,
      char escapeCharacter,
      int numRows,
      IEnumerable<char> disallowedDelimiter,
      CancellationToken cancellationToken)
    {
      if (textReader is null)
        throw new ArgumentNullException(nameof(textReader));
      var dc = new DelimiterCounter(numRows, disallowedDelimiter, quoteCharacter);

      var quoted = false;
      char readChar = ' ';
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
      while (dc.LastRow < dc.NumRows && !textReaderPosition.AllRead() && !cancellationToken.IsCancellationRequested)
      {
        var lastChar = readChar;
        readChar = (char) textReader.Read();
        if (lastChar == escapeCharacter)
          continue;
        if (readChar == quoteCharacter)
        {
          if (quoted)
          {
            if (textReader.Peek() == quoteCharacter)
              textReader.MoveNext();
            else
              quoted = false;
          }
          else
            quoted = true;
        }
        if (quoted)
          continue;

        if (readChar == '\n' || readChar == '\r')
        {
          if (readChar == '\n' && lastChar != '\r' ||
              readChar == '\r' && lastChar != '\n' )
            dc.LastRow++;          
        }          
        else
          dc.CheckChar(readChar, lastChar);
      }
      return dc;
    }

    public readonly struct DelimiterDetection
    {
      public readonly char Delimiter;
      public readonly bool IsDetected;
      public readonly bool MagicKeyword;

      public DelimiterDetection(char delimiter, bool isDetected, bool magicKeyword)
      {
        Delimiter = delimiter;
        IsDetected = isDetected;
        MagicKeyword = magicKeyword;
      }
    }
  }
}