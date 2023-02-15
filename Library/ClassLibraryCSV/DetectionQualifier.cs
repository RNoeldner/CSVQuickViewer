using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class DetectionQualifier
  {
    /// <summary>
    ///   Try to determine quote character, by looking at the file and doing a quick analysis
    /// </summary>
    /// <param name="textReader">The opened TextReader</param>
    /// <param name="fieldDelimiterChar">The char to be used as field delimiter</param>
    /// <param name="escapePrefixChar">Used to escape a delimiter or quoting char</param>
    /// ///
    /// <param name="possibleQuotes">Possibles quotes to test, usually its ' and "</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>The most likely quoting char</returns>
    /// <remarks>
    ///   Any line feed ot carriage return will be regarded as field delimiter, a duplicate quoting will be regarded as
    ///   single quote, an \ escaped quote will be ignored
    /// </remarks>
    public static QuoteTestResult InspectQualifier(
      this ImprovedTextReader textReader,
      char fieldDelimiterChar,
      char escapePrefixChar,
      IEnumerable<char> possibleQuotes,
      in CancellationToken cancellationToken)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));

      var bestQuoteTestResults = new QuoteTestResult();
      foreach (var t in possibleQuotes)
      {
        cancellationToken.ThrowIfCancellationRequested();
        var currentQuote = GetScoreForQuote(textReader, fieldDelimiterChar, escapePrefixChar, t, cancellationToken);
        if (currentQuote.Score > bestQuoteTestResults.Score)
          bestQuoteTestResults = currentQuote;
      }

      Logger.Information($"Column Qualifier: {bestQuoteTestResults.QuoteChar.Text()}");
      return bestQuoteTestResults;
    }

    /// <summary>
    ///   Does check if quoting was actually used in the file
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="codePageId">The code page identifier.</param>
    /// <param name="skipRows">The number of lines at beginning to disregard</param>
    /// <param name="fieldDelimiterChar">The delimiter to separate columns</param>
    /// <param name="fieldQualifierChar">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns><c>true</c> if [has used qualifier] [the specified setting]; otherwise, <c>false</c>.</returns>
    public static async Task<bool> HasUsedQualifierAsync(
      this Stream stream,
      int codePageId,
      int skipRows,
      char fieldDelimiterChar,
      char fieldQualifierChar,
      CancellationToken cancellationToken)
    {
      // if we do not have a quote defined it does not matter
      if (fieldQualifierChar == char.MinValue || cancellationToken.IsCancellationRequested)
        return false;

      using var streamReader = await stream.GetTextReaderAsync(codePageId, skipRows, cancellationToken).ConfigureAwait(false);
      var isStartOfColumn = true;
      while (!streamReader.EndOfStream)
      {
        if (cancellationToken.IsCancellationRequested)
          return false;
        var c = (char) streamReader.Read();
        if (c == '\r' || c == '\n' || c == fieldDelimiterChar)
        {
          isStartOfColumn = true;
          continue;
        }

        // if we are not at the start of a column we can get the next char
        if (!isStartOfColumn)
          continue;
        // If we are at the start of a column and this is a ", we can stop, this is a real qualifier
        if (c == fieldQualifierChar)
          return true;
        // Any non whitespace will reset isStartOfColumn
        if (c <= '\x00ff')
          isStartOfColumn = c == ' ' || c == '\t';
        else
          isStartOfColumn = CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator;
      }

      return false;
    }
    private static QuoteTestResult GetScoreForQuote(
      in ImprovedTextReader textReader,
      char delimiterChar,
      char escapeChar,
      char quoteChar,
      in CancellationToken cancellationToken)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));
      var counterTotal = 0;
      var counterOpenStrict = 0;
      var counterOpenSimple = 0;
      var counterCloseStrict = 0;
      const char placeHolderText = 't';
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
      var filter = new StringBuilder();
      var last = -1;

      var res = new QuoteTestResult { QuoteChar = quoteChar };
      // Read simplified text from file
      while (!textReaderPosition.AllRead() && filter.Length < 2000 && !cancellationToken.IsCancellationRequested)
      {
        var c = (char) textReader.Read();
        // disregard spaces
        if (c == ' ' || c== char.MinValue)
          continue;

        if (c == escapeChar)
        {
          if (!textReader.EndOfStream)
            if (quoteChar == textReader.Read())
              res.EscapedQualifier = true;

          // anything escaped is regarded as text
          c = placeHolderText;
        }
        else if (c == quoteChar)
        {
          if (last == quoteChar)
          {
            res.DuplicateQualifier = true;
            // replace the already added quote with text
            filter.Length--;
            c = placeHolderText;
          }
        }
        else if (c == '\r' || c == '\n')
          c = delimiterChar;
        else if (c != delimiterChar)
          c = placeHolderText;

        if (last != c)
          filter.Append(c);

        last = c;
      }

      // normalize this, line should start and end with delimiter
      //  t","t","t",t,t,t't,t"t,t -> ,t","t","t",t,t,t't,t"t,t,,
      var line = delimiterChar + filter.ToString().Trim(delimiterChar) + delimiterChar + delimiterChar;

      for (var index = 1; index < line.Length - 2; index++)
      {
        if (line[index] != quoteChar)
          continue;
        counterTotal++;
        if (line[index - 1] == delimiterChar)
        {
          // having a delimiter before is good, but it would be even better if its followed by text
          counterOpenSimple++;
          if ((line[index + 1] == placeHolderText ||
              (line[index + 1] == quoteChar &&
               line[index + 2] != delimiterChar)))
            counterOpenStrict++;
        }
        if (line[index + 1] == delimiterChar&&line[index - 1] == quoteChar)
          counterCloseStrict++;
      }

      if (counterOpenStrict != 0 && counterCloseStrict * 1.5 > counterOpenStrict &&
          counterCloseStrict < counterOpenStrict * 1.5)
      {
        res.Score = 5 * counterOpenStrict;
      }
      else if (!res.DuplicateQualifier && !res.DuplicateQualifier && counterOpenSimple != 0)
      {
        res.Score = 3 * counterOpenSimple;
      }
      else
        res.Score = counterTotal;

      // if we could not find opening and closing because we has a lot of ,", take the absolute numbers
      return res;
    }

    public struct QuoteTestResult
    {
      public bool DuplicateQualifier;
      public bool EscapedQualifier;
      public char QuoteChar;
      public int Score;
    }
  }
}