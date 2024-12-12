/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  /// Static class with methods for Qualifier Detection
  /// </summary>
  public static class DetectionQualifier
  {
    /// <summary>
    ///   Try to determine quote character, by looking at the file and doing a quick analysis
    /// </summary>
    /// <param name="textReader">The opened TextReader</param>
    /// <param name="fieldDelimiterChar">The char to be used as field delimiter</param>
    /// <param name="escapePrefixChar">Used to escape a delimiter or quoting char</param>
    /// <param name="possibleQuotes">Possibles quotes to test, usually its ' and "</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
    /// <returns>The most likely quoting char</returns>
    /// <remarks>
    ///   Any line feed or carriage return will be regarded as field delimiter, a duplicate quoting will be regarded as
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
        // Give " a large edge
        if (currentQuote.QuoteChar == '"' && currentQuote.Score < 85)
          currentQuote.Score += 25;
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
    /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
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

      using var streamReader =
        await stream.GetTextReaderAsync(codePageId, skipRows, cancellationToken).ConfigureAwait(false);
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
        // Any non whitespace will start a column
        isStartOfColumn = c != '\t' && CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.SpaceSeparator;
      }

      return false;
    }

    /// <summary>
    /// Determine a score for a quote between 0 and 99 in addition it determines if we have escaped quotes and duplicate quotes
    /// </summary>
    /// <param name="textReader">The opened TextReader</param>
    /// <param name="delimiterChar">The char to be used as field delimiter</param>
    /// <param name="escapeChar">Used to escape a delimiter or quoting char</param>
    /// <param name="quoteChar">Possibles quotes to test, usually its ' and "</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
    /// <returns>The score is between 0 and 99, 99 meaning almost certain</returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static QuoteTestResult GetScoreForQuote(
      in ImprovedTextReader textReader,
      char delimiterChar,
      char escapeChar,
      char quoteChar,
      in CancellationToken cancellationToken)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));
      const int cBufferMax = 8192; // Defined constant for max length
      const char placeHolderText = 't';
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);

      var buffer = new char[cBufferMax+3];
      buffer[0]=delimiterChar;
      var bufferPos = 0;
      var last = char.MinValue;
      var res = new QuoteTestResult(quoteChar);

      // Read simplified text from file
      while (!textReaderPosition.AllRead() && bufferPos < cBufferMax && !cancellationToken.IsCancellationRequested)
      {
        var c = (char) textReader.Read();
        switch (c)
        {
          // disregard spaces
          case ' ':
          case char.MinValue:
            continue;
          case var _ when c == escapeChar:
            if (!textReader.EndOfStream && quoteChar == textReader.Read())
              res.EscapedQualifier = true;

            // anything escaped is regarded as text
            c = placeHolderText;
            break;

          case var _ when c == quoteChar:
            if (last == quoteChar)
            {
              res.DuplicateQualifier = true;
              // Its safe to modify the StringBuilder length as last is '\0' in case nothing was added
              bufferPos--;
              c = placeHolderText;
            }
            break;

          case '\r':
          case '\n':
          case var _ when c == delimiterChar:
            c = delimiterChar; // Normalize new line or delimiter characters
            break;

          default:
            c = placeHolderText; // Set to placeholder if none of the above
            break;
        }

        // Add to filter only if current character differs from the last
        if (last != c)
          buffer[++bufferPos]=c;

        last = c; // Update last character
      }
      const double CloseOpenRatioThreshold = 1.5;
      const int MinQuotesForDuplicateCheck = 50;
      const int MinLengthForDuplicateCheck = 100;

      var counterTotal = 0;
      var counterOpenStrict = 0;
      var counterOpenSimple = 0;
      var counterCloseSimple = 0;
      var counterCloseStrict = 0;

      // if there is no suitable text, exit
      if (bufferPos < 3)
        res.Score = 0;
      else
      {
        // normalize this, line should start and end with delimiter for out of range safety:
        //  t","t","t",t,t,t"t,t"t,t -> ,t","t","t",t,t,t"t,t"t,t,
        buffer[++bufferPos]=delimiterChar;
        buffer[++bufferPos]=delimiterChar;

        for (var index = 1; index < bufferPos-2; index++)
        {
          if (buffer[index] != quoteChar)
            continue;

          counterTotal++;

          if (buffer[index - 1] == delimiterChar)
          {
            // having a delimiter before is good, but it would be even better if it's followed by text
            counterOpenSimple++;
            if (buffer[index + 1] == placeHolderText || buffer[index + 1] == quoteChar && buffer[index + 2] != delimiterChar)
              counterOpenStrict++;
          }

          // safe we made sure line has the needed length
          if (buffer[index + 1] == delimiterChar)
          {
            counterCloseSimple++;
            if (buffer[index - 1] != delimiterChar)
              counterCloseStrict++;
          }
        }

        var totalScore = counterTotal;
        if (counterOpenStrict != 0 && counterCloseStrict * CloseOpenRatioThreshold > counterOpenStrict &&
            counterCloseStrict < counterOpenStrict * CloseOpenRatioThreshold)
        {
          totalScore = 2 * counterOpenStrict + 2 * counterCloseStrict;
        }
        else if (!res.DuplicateQualifier && counterOpenSimple != 0)
        {
          totalScore = counterOpenSimple + counterCloseSimple;
        }

        // If we hardly saw quotes assume DuplicateQualifier
        if (counterTotal < MinQuotesForDuplicateCheck && bufferPos > MinLengthForDuplicateCheck)
          res.DuplicateQualifier = true;

        // try to normalize the score, depending on the length of the filter build a percent score that  should indicate how sure
        res.Score = totalScore > bufferPos ? 99 : Convert.ToInt32(totalScore / (double) bufferPos * 100);
      }

      // if we could not find opening and closing because we have a lot of ,", take the absolute numbers
      return res;
    }

    /// <summary>
    /// Outcome of the Tests
    /// </summary>
    public struct QuoteTestResult
    {
      /// <summary>
      /// Duplicate Qualifier found
      /// </summary>
      public bool DuplicateQualifier;

      /// <summary>
      /// Escaped Qualifiers found
      /// </summary>
      public bool EscapedQualifier;

      /// <summary>
      /// Quoting char
      /// </summary>
      public readonly char QuoteChar;

      /// <summary>
      /// Score for the Quote
      /// </summary>
      public int Score;

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="quoteChar"></param>
      /// <param name="score"></param>
      /// <param name="duplicateQualifier"></param>
      /// <param name="escapedQualifier"></param>
      public QuoteTestResult(char quoteChar, int score = 0, bool duplicateQualifier = false,
        bool escapedQualifier = false)
      {
        QuoteChar = quoteChar;
        Score = score;
        DuplicateQualifier = duplicateQualifier;
        EscapedQualifier = escapedQualifier;
      }
    }
  }
}