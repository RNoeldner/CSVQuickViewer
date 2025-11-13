/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  /// Static class with methods for Delimiter Detection
  /// </summary>
  public static class DetectionDelimiter
  {
    /// <summary>Gets the delimiter assuming the extension is "correct"</summary>
    /// <param name="phys">The physical File</param>
    /// <returns>
    /// Comma for .csv, Tab for .tsv or .tab, /0 otherwise
    /// </returns>
    public static char GetDelimiterByExtension(this IFileSettingPhysicalFile phys) => GetDelimiterByExtension(string.IsNullOrEmpty(phys.IdentifierInContainer) ? phys.FileName : phys.IdentifierInContainer);

    /// <summary>Gets the delimiter assuming the extension is "correct"</summary>
    /// <param name="name">The fileName</param>
    /// <returns>
    /// Comma for .csv, Tab for .tsv or .tab, /0 otherwise
    /// </returns>
    public static char GetDelimiterByExtension(string name)
    {
      if (!string.IsNullOrEmpty(name))
      {
        if (name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
          return ',';
        if (name.EndsWith(".tsv", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".tab", StringComparison.OrdinalIgnoreCase))
          return '\t';
      }
      return '\0';
    }

    /// <summary>
    ///   Guesses the delimiter for a files. Done with a rather simple csv parsing, and trying to
    ///   find the delimiter that has the least variance in the read rows, if they are the same it will look at 
    ///   the positioning (Score), as a delimiter is preceded by a text or by a quote will increase the score.
    /// </summary>
    /// <param name="textReader">The text reader to read the data</param>
    /// <param name="fieldQualifierChar">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
    /// <param name="escapePrefixChar">The start of an escape sequence to allow delimiter or qualifier in column</param>
    /// <param name="disallowedDelimiter">Character rules out as possible delimiters</param>
    /// <param name="probableDelimiter">Give this delimiter a higher score, commonly derived from file extension</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
    /// <returns>A character with the assumed delimiter for the file</returns>
    /// <exception cref="ArgumentNullException">streamReader</exception>
    /// <remarks>No Error will not be thrown.</remarks>
    public static async Task<DelimiterDetection> InspectDelimiterAsync(
      this ImprovedTextReader textReader,
      char fieldQualifierChar,
      char escapePrefixChar,
      IEnumerable<char> disallowedDelimiter,
      char probableDelimiter,
      CancellationToken cancellationToken)
    {
      if (textReader is null)
        throw new ArgumentNullException(nameof(textReader));
      var match = char.MinValue;
      var firstLine = string.Empty;
      if (textReader.CanSeek)
      {
        // Read the first line and check if it does contain the magic word sep=
        firstLine = (await textReader.ReadLineAsync(cancellationToken).ConfigureAwait(false)).Trim().Replace(" ", "");
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

      var delimiterCounter = await textReader.GetDelimiterCounterAsync(fieldQualifierChar, escapePrefixChar, 300, disallowedDelimiter, cancellationToken).ConfigureAwait(false);
      var numberOfRows = delimiterCounter.FilledRows;

      // Limit everything to 100 columns max, the sum might get too big otherwise 100 * 100
      var startRow = delimiterCounter.LastRow > 60 ? 15 : delimiterCounter.LastRow > 20 ? 5 : 0;

      var neededRows = (delimiterCounter.FilledRows > 20 ? numberOfRows * 75 : numberOfRows * 50) / 100;
      if (neededRows==1 && delimiterCounter.FilledRows > 1)
        neededRows++;

      cancellationToken.ThrowIfCancellationRequested();
      var validSeparatorIndex = new List<int>(delimiterCounter.Separators.Length);
      for (var index = 0; index < delimiterCounter.Separators.Length; index++)
      {
        // only regard a delimiter if we have 75% of the rows with this delimiter we can still have
        // a lot of commented lines
        if (delimiterCounter.SeparatorRows[index] == 0 || (delimiterCounter.SeparatorRows[index] < neededRows && numberOfRows > 3))
          continue;
        validSeparatorIndex.Add(index);
      }

      switch (validSeparatorIndex.Count)
      {
        case 0:
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

          break;
        }
        case 1:
          // if only one was found done here
          match = delimiterCounter.Separators[validSeparatorIndex[0]];
          break;
        default:
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

            // if a lot of rows do not have a columns disregard the delimiter
            if (intEmptyRows  > totalRows * 4 / 5)
              continue;

            // Get the average of the rows
            var avg = (int) Math.Ceiling(sumCount / (totalRows -intEmptyRows));

            // Only proceed if there is usually more than one occurrence, and we have more then one row
            if (avg < 1 || delimiterCounter.SeparatorRows[index] == 1)
              continue;

            // First determine the variance, low value means and even distribution
            long variance = 0;
            for (var row = startRow; row < delimiterCounter.LastRow; row++)
            {
              if (delimiterCounter.SeparatorsCount[index, row] == avg || delimiterCounter.SeparatorsCount[index, row] == 0)
                continue;
              variance += Math.Abs(delimiterCounter.SeparatorsCount[index, row] - avg);
            }

            // now waith the variance as well, if avg is high the variance is less important            
            sums.Add(index, variance * 4 / avg);

            // handling on probability of delimiter
            if (delimiterCounter.Separators[index]== probableDelimiter)
              sums[index]++;
            else if (delimiterCounter.Separators[index]== '\'' || delimiterCounter.Separators[index]==  '*' || delimiterCounter.Separators[index]==   '`')
              sums[index]--;


            if (firstLine.Length> 0 && !firstLine.StartsWith("#", StringComparison.Ordinal))
            {
              // in case the checked delimiter is in the header it's a good indication that its correct
              if (firstLine.Contains(delimiterCounter.Separators[index]))
                sums[index]++;
              else
                // otherwise its pretty save to say it's not good.
                sums[index]--;
            }
          }

          if (sums.Count > 1)
          {
            foreach (var kv in sums)
              Logger.Information($"Multiple Possible Separator {delimiterCounter.Separators[kv.Key].Description()} -  Variance {kv.Value:N0} Score {delimiterCounter.SeparatorScore[kv.Key]:N0}");
          }

          if (sums.Count!= 0)
          {
            int bestIndex = -1;
            long bestVariance = long.MaxValue;
            int bestScore = int.MinValue;
            foreach (var kv in sums)
            {
              if (kv.Value < bestVariance || (kv.Value == bestVariance && delimiterCounter.SeparatorScore[kv.Key] > bestScore))
              {
                bestIndex = kv.Key;
                bestVariance = kv.Value;
                bestScore = delimiterCounter.SeparatorScore[kv.Key];
              }
            }
            match = delimiterCounter.Separators[bestIndex];
          }

          break;
        }
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
    /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
    /// <returns>
    ///   A <see cref="DelimiterCounter" /> with the information on delimiters
    /// </returns>
    /// <exception cref="System.ArgumentNullException">textReader</exception>

    private static async Task<DelimiterCounter> GetDelimiterCounterAsync(
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

      const int bufferSize = 4096;
      var buffer = ArrayPool<char>.Shared.Rent(bufferSize);

      try
      {
        bool quoted = false;
        char lastChar = ' ';
        bool pendingQuoteCheck = false; // new flag

        while (dc.LastRow < dc.NumRows && !cancellationToken.IsCancellationRequested)
        {
          int charsRead = await textReader.ReadBlockAsync(buffer, cancellationToken).ConfigureAwait(false);
          if (charsRead == 0)
            break;

          int i = 0;

          // Handle pending quote from previous buffer
          if (pendingQuoteCheck)
          {
            if (buffer[0] == quoteCharacter)
            {
              // Escaped quote "" -> skip it
              i = 1;
            }
            else
            {
              // It was actually a closing quote
              quoted = false;
            }

            pendingQuoteCheck = false;
          }

          for (; i < charsRead; i++)
          {
            char readChar = buffer[i];

            // Skip escaped characters
            if (lastChar == escapeCharacter)
            {
              lastChar = readChar;
              continue;
            }

            // Handle quoting
            if (readChar == quoteCharacter)
            {
              if (quoted)
              {
                if (i + 1 < charsRead)
                {
                  if (buffer[i + 1] == quoteCharacter)
                  {
                    i++; // skip escaped quote
                  }
                  else
                  {
                    quoted = false; // closing quote
                  }
                }
                else
                {
                  // The last char in the buffer is a quote,
                  // need to check next buffer
                  pendingQuoteCheck = true;
                }
              }
              else
              {
                quoted = true;
              }

              lastChar = readChar;
              continue;
            }

            if (quoted)
            {
              lastChar = readChar;
              continue;
            }

            // Row ending
            if (readChar is '\n' or '\r')
            {
              if ((readChar == '\n' && lastChar != '\r') ||
                  (readChar == '\r' && lastChar != '\n'))
              {
                dc.LastRow++;
                if (dc.LastRow >= dc.NumRows)
                  return dc; // early exit
              }
            }
            else
            {
              dc.CheckChar(readChar, lastChar);
            }

            lastChar = readChar;
          }
        }

        return dc;
      }
      finally
      {
        ArrayPool<char>.Shared.Return(buffer);
      }
    }



    /// <summary>
    /// Result for Delimiter Detection
    /// </summary>
    public readonly struct DelimiterDetection
    {
      /// <summary>
      /// The determined Delimiter
      /// </summary>
      public readonly char Delimiter;

      /// <summary>
      /// If we detected a value
      /// </summary>
      public readonly bool IsDetected;

      /// <summary>
      /// True if the MagicKeyword was used to determine delimiter
      /// </summary>
      public readonly bool MagicKeyword;

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="delimiter">The determined Delimiter</param>
      /// <param name="isDetected">If detected</param>
      /// <param name="magicKeyword">True if the MagicKeyword was used to determine delimiter</param>
      public DelimiterDetection(char delimiter, bool isDetected, bool magicKeyword)
      {
        Delimiter = delimiter;
        IsDetected = isDetected;
        MagicKeyword = magicKeyword;
      }
    }

    /// <summary>
    /// Class to store information allowing to choose the best delimiter
    /// </summary>
    public sealed class DelimiterCounter
    {
      /// <summary>
      /// Number of read Rows
      /// </summary>
      public readonly int NumRows;

      /// <summary>
      /// Rows that do contain the delimiter
      /// </summary>
      public readonly int[] SeparatorRows;

      /// <summary>
      /// All used Delimiter
      /// </summary>
      public readonly string Separators;

      /// <summary>
      /// Score by delimiterY 
      /// </summary>
      public readonly int[] SeparatorScore;

      /// <summary>
      /// Number of occurrences by row/delimiter
      /// </summary>
      public readonly int[,] SeparatorsCount;

      /// <summary>
      /// Last valid Row
      /// </summary>
      public int LastRow;

      private readonly char m_FieldQualifier;
      /// <summary>
      ///  Creates an instance of a delimiter counter
      /// </summary>
      /// <param name="numRows">Number of rows to expect</param>
      /// <param name="disallowedDelimiter">You can pass in delimiters that should not be detected, 
      /// if you know that a delimiter is defiantly not suitable.</param>
      /// <param name="fieldQualifier">Qualifier / Quoting of column to allow delimiter or linefeed to be contained in column</param>
      public DelimiterCounter(int numRows, IEnumerable<char> disallowedDelimiter, char fieldQualifier)
      {
        NumRows = numRows;
        m_FieldQualifier = fieldQualifier;
        Separators = new string(StaticCollections.DelimiterChars.Where(x => !disallowedDelimiter.Contains(x)).ToArray());
        SeparatorsCount = new int[Separators.Length, NumRows];
        SeparatorRows = new int[Separators.Length];
        SeparatorScore= new int[Separators.Length];
      }

      /// <summary>
      /// Number of rows that are not empty
      /// </summary>
      public int FilledRows
      {
        get
        {
          // Correct the lastRow
          while (LastRow > 1 && RowEmpty(LastRow - 1))
            LastRow--;

          var res = 0;
          for (var line = 0; line < LastRow; line++)
            if (!RowEmpty(line))
              res++;
          return res;
        }
      }

      /// <summary>
      /// Main method called with the current char and the last char
      /// </summary>
      /// <param name="read">The character to check</param>
      /// <param name="last">The previous char, this char allows scoring</param>
      /// <returns><c>true</c> if the char was a delimiter</returns>
      public bool CheckChar(char read, char last)
      {
        var index = Separators.IndexOf(read);
        if (index == -1)
          return false;

        if (SeparatorsCount[index, LastRow] == 0)
          SeparatorRows[index]++;

        ++SeparatorsCount[index, LastRow];
        // A separator its worth more if the previous char was the quote
        if (last == m_FieldQualifier)
          SeparatorScore[index] += 2;
        else if (last != read && last!=' ' && last!='\r' && last!='\n')
          // its also worth something if previous char appears to be a text
          SeparatorScore[index]++;

        return true;
      }

      private bool RowEmpty(int line)
      {
        for (var x = 0; x < Separators.Length; x++)
          if (SeparatorsCount[x, line] != 0)
            return false;
        return true;
      }
    }
  }
}
