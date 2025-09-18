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
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
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
    /// <param name="commentLine">The characters for a comment line.</param>
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
      in string commentLine,
      IEnumerable<char> possibleQuotes,
      in CancellationToken cancellationToken)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));

      var bestQuoteTestResults = new QuoteTestResult();
      foreach (var t in possibleQuotes)
      {
        cancellationToken.ThrowIfCancellationRequested();
        var currentQuote = GetScoreForQuote(textReader, fieldDelimiterChar, escapePrefixChar, t, commentLine, cancellationToken);
        if (currentQuote.Score > bestQuoteTestResults.Score)
          bestQuoteTestResults = currentQuote;
        // Give " a large edge
        if (currentQuote.QuoteChar == '"' && currentQuote.Score >= 25)
          break;
      }

      Logger.Information($"Column Qualifier: {bestQuoteTestResults.QuoteChar.Text()} Score:{bestQuoteTestResults.Score:N0}");
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
      if (fieldQualifierChar == char.MinValue || cancellationToken.IsCancellationRequested)
        return false;

      using var streamReader =
          await stream.GetTextReaderAsync(codePageId, skipRows, cancellationToken).ConfigureAwait(false);

      const int bufferSize = 4096;
      var buffer = ArrayPool<char>.Shared.Rent(bufferSize);

      try
      {
        bool isStartOfColumn = true;
        char lastChar = '\0'; // carry across buffer boundaries

        while (true)
        {
          cancellationToken.ThrowIfCancellationRequested();

          int charsRead = await streamReader.ReadBlockAsync(buffer, cancellationToken).ConfigureAwait(false);
          if (charsRead == 0)
            break;

          for (int i = 0; i < charsRead; i++)
          {
            char c = buffer[i];

            // Handle line breaks robustly (\r, \n, \r\n, \n\r)
            if (c == '\r' || c == '\n')
            {
              // only count new line if not part of pair
              if (!((c == '\n' && lastChar == '\r') || (c == '\r' && lastChar == '\n')))
              {
                isStartOfColumn = true;
              }

              lastChar = c;
              continue;
            }

            // Field delimiter → start of next column
            if (c == fieldDelimiterChar)
            {
              isStartOfColumn = true;
              lastChar = c;
              continue;
            }

            if (!isStartOfColumn)
            {
              lastChar = c;
              continue;
            }

            // Qualifier at start of column → success
            if (c == fieldQualifierChar)
              return true;

            // Whitespace handling (avoid expensive UnicodeCategory when possible)
            if (c == '\t' || c == ' ')
            {
              isStartOfColumn = true;
            }
            else
            {
              isStartOfColumn = !char.IsWhiteSpace(c);
            }

            lastChar = c;
          }
        }
      }
      finally
      {
        ArrayPool<char>.Shared.Return(buffer);
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
    /// <param name="commentLine">The characters for a comment line.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
    /// <returns>The score is between 0 and 99, 99 meaning almost certain, a only text with hardly any quotes will still have a low score</returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static QuoteTestResult GetScoreForQuote(
      in ImprovedTextReader textReader,
      char delimiterChar,
      char escapeChar,
      char quoteChar,
      in string commentLine,
      in CancellationToken cancellationToken)
    {
      if (textReader is null) throw new ArgumentNullException(nameof(textReader));

      const int cBufferMax = 262144; // Defined constant for max length
      const char placeHolderText = 't';
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);

      var buffer = new char[cBufferMax+3];
      // Start with delimiter
      buffer[0]=delimiterChar;
      var bufferPos = 0;
      var lineStart = true;
      var last = char.MinValue;
      var res = new QuoteTestResult(quoteChar);
      var commentLine0 = '\0';
      var commentLine1 = '\0';

      if (!string.IsNullOrEmpty(commentLine))
      {
        commentLine0 = commentLine[0];
        if (commentLine.Length>1)
          commentLine1 = commentLine[1];
        if (commentLine.Length>2)
          Logger.Warning("Quote detection can only use two characters for line comment");
      }
      // Read simplified text from file
      while (!textReaderPosition.AllRead() && bufferPos < cBufferMax && !cancellationToken.IsCancellationRequested)
      {
        var c = (char) textReader.Read();
        if (lineStart && commentLine0 != '\0' && c == commentLine0)
        {
          if (commentLine1 == '\0' || commentLine1 == textReader.Peek())
          {
            // Eat everything till we reach next line
            while (c != '\r' && c != '\n' && !textReaderPosition.AllRead() && !cancellationToken.IsCancellationRequested)
              c = (char) textReader.Read();
            // handle /r /n or /n /r
            c = (char) textReader.Peek();
            while (c == '\r' || c == '\n' && !textReaderPosition.AllRead() && !cancellationToken.IsCancellationRequested)
            {
              textReader.Read();
              c =  (char) textReader.Peek();
            }
            continue;
          }
        }
        switch (c)
        {
          case '\r':
          case '\n':
            lineStart = true;
            continue;

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
            // If we have a duplicate quote, and is not followed by delimiter and started by delimiter
            // .e.G. ,"", is not a duplicate quote, its an empty text
            if (last == quoteChar)
            {
              if (textReader.EndOfStream || delimiterChar != textReader.Peek())
              {
                res.DuplicateQualifier = true;
                // Regard a duplicate Quote as regular text
                c = placeHolderText;
              }

              // Its safe to modify the StringBuilder length as last is '\0' in case nothing was added
              bufferPos--;
            }
            break;


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
        lineStart = false;
        last = c; // Update last character
      }

      var counterTotal = 0;
      var counterOpenAndText = 0;
      var counterOpenSimple = 0;
      var counterCloseSimple = 0;
      var counterCloseAndDelimiter = 0;

      // if there is no suitable text, exit
      if (bufferPos > 3)
      {
        // normalize this, line should start and end with delimiter for out of range safety:
        //  t","t","t",t,t,t"t,t"t,t -> ,t","t","t",t,t,t"t,t"t,t,
        // End with delimiter
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
              counterOpenAndText++;
          }

          // safe we made sure line has the needed length
          if (buffer[index + 1] == delimiterChar)
          {
            counterCloseSimple++;
            if (buffer[index - 1] == placeHolderText)
              counterCloseAndDelimiter++;
          }
        }
        if (counterTotal==0)
          return res;

        var totalScore = counterTotal;

        // Very high rating for starting  of A column with text
        // this is counted again in counterOpenSimple
        if (counterOpenAndText > 0 && counterCloseAndDelimiter >0)
          totalScore += 5 * (counterOpenAndText + counterCloseAndDelimiter);

        // having roughly equal number opening and closing quotes before and after delimiter is adding to score        
        if (counterOpenSimple > 0 && counterCloseSimple >0 && counterOpenSimple >= counterCloseSimple - 2 && counterOpenSimple <= counterCloseSimple + 2)
          totalScore += 3 * counterOpenSimple + counterCloseSimple;

        // If we hardly saw quotes assume DuplicateQualifier
        if (!res.DuplicateQualifier && counterTotal < 50 && bufferPos > 100)
          res.DuplicateQualifier = true;

        // If we have escaped quote add some bonus
        if (res.DuplicateQualifier || res.EscapedQualifier)
          totalScore += 10;

        // try to normalize the score, depending on the length of the filter build a percent score that  should indicate how sure
        res.Score = totalScore > bufferPos ? 99 : Convert.ToInt32(totalScore / (double) bufferPos * 100) + 1;

        // If we have a decent score but simply read a large volume adjust the resulting score
        if (totalScore> 250 && res.Score<2)
          res.Score = 10;
      }

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