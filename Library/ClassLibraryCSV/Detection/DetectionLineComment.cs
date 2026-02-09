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
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools;

/// <summary>
/// Provides zero-allocation detection and validation for line-based comments.
/// </summary>
public static class DetectionLineComment
{
  const int maxRows = 100;

  /// <summary>
  /// Validates a comment prefix by analyzing delimiter density without string allocations.
  /// </summary>
  /// <param name="textReader">The text reader to read the data</param>
  /// <param name="commentLine">The characters for a comment line.</param>
  /// <param name="fieldDelimiterChar">The delimiter to separate columns</param>
  /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
  /// <returns>true if the comment line seems to be ok</returns>
  public static async Task<bool> InspectLineCommentIsValidAsync(
      this ImprovedTextReader textReader,
      string commentLine,
      char fieldDelimiterChar,
      CancellationToken cancellationToken)
  {
    if (string.IsNullOrEmpty(commentLine) || fieldDelimiterChar == char.MinValue)
      return true;

    if (textReader is null) throw new ArgumentNullException(nameof(textReader));

    int dataRowCount = 0;
    int lineCommentedCount = 0;
    int totalDataDelims = 0;
    int firstCommentDelims = -1;

    textReader.ToBeginning();
    var buffer = ArrayPool<char>.Shared.Rent(4096);

    try
    {
      bool isStartOfLine = true;
      bool isCurrentLineComment = false;
      int currentLineDelims = 0;
      char lastChar = '\0';

      while (dataRowCount < maxRows && !cancellationToken.IsCancellationRequested)
      {
        int charsRead = await textReader.ReadBlockAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (charsRead == 0) break;

        for (int i = 0; i < charsRead; i++)
        {
          char c = buffer[i];

          // 1. Detect start of line (ignoring leading whitespace)
          if (isStartOfLine && !char.IsWhiteSpace(c))
          {
            isCurrentLineComment = IsMatch(buffer, i, charsRead, commentLine);
            isStartOfLine = false;
          }

          // 2. Count delimiters for density check
          if (c == fieldDelimiterChar)
          {
            currentLineDelims++;
          }

          // 3. Handle Line Endings
          if (c is '\n' or '\r')
          {
            // Avoid double-counting CRLF
            if ((c == '\n' && lastChar != '\r') || (c == '\r'))
            {
              if (isCurrentLineComment)
              {
                lineCommentedCount++;
                if (firstCommentDelims == -1) firstCommentDelims = currentLineDelims;
              }
              else if (currentLineDelims > 0)
              {
                totalDataDelims += currentLineDelims;
                dataRowCount++;
              }

              // Reset state for next line
              isStartOfLine = true;
              isCurrentLineComment = false;
              currentLineDelims = 0;
              if (dataRowCount >= maxRows) break;
            }
          }
          lastChar = c;
        }
      }
    }
    finally
    {
      ArrayPool<char>.Shared.Return(buffer);
    }

    return lineCommentedCount switch
    {
      0 => false,
      > 2 => true,
      _ => IsStatisticallyComment(firstCommentDelims, totalDataDelims, dataRowCount)
    };
  }

  /// <summary>
  /// Guesses the line comment prefix by inspecting the start of the first maxRows lines.
  /// This implementation is zero-allocation and avoids string materialization.
  /// </summary>
  /// <param name="textReader">The reader to analyze.</param>
  /// <param name="cancellationToken">Cancellation token to stop the process.</param>
  /// <returns>The most frequent comment prefix found, or an empty string.</returns>
  public static async Task<string> InspectLineCommentAsync(this ImprovedTextReader textReader,
    CancellationToken cancellationToken)
  {
    if (textReader is null)
      throw new ArgumentNullException(nameof(textReader));

    // Common comment markers ordered by length descending.
    // This order ensures "##" is checked before "#".
    string[] candidates = { "<!--", "##", "//", "==", @"\\", "''", "#", "/", "\\", "'" };
    int[] counts = new int[candidates.Length];

    textReader.ToBeginning();

    // Use ArrayPool to prevent Gen-0 heap allocations
    char[] buffer = ArrayPool<char>.Shared.Rent(4096);

    try
    {
      int linesRead = 0;
      bool isStartOfLine = true;

      while (linesRead < maxRows && !cancellationToken.IsCancellationRequested)
      {
        int charsRead = await textReader.ReadBlockAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (charsRead == 0) break;

        for (int i = 0; i < charsRead; i++)
        {
          char c = buffer[i];

          // Logic: If we are at the start of a line, find the first non-whitespace character
          if (isStartOfLine && !char.IsWhiteSpace(c))
          {
            for (int j = 0; j < candidates.Length; j++)
            {
              // Check if the buffer at the current position matches the candidate
              if (IsMatch(buffer, i, charsRead, candidates[j]))
              {
                counts[j]++;
                break; // Found the longest match for this line
              }
            }
            isStartOfLine = false; // Moved past the start of this line
          }

          // Monitor for line endings to reset the state
          if (c is '\n' or '\r')
          {
            linesRead++;
            isStartOfLine = true;
            if (linesRead >= maxRows) break;
          }
        }
      }
    }
    finally
    {
      ArrayPool<char>.Shared.Return(buffer);
    }

    // Identify the prefix with the highest occurrence count
    int maxVal = 0;
    int bestIdx = -1;
    for (int k = 0; k < counts.Length; k++)
    {
      if (counts[k] > maxVal)
      {
        maxVal = counts[k];
        bestIdx = k;
      }
    }

    return bestIdx != -1 ? candidates[bestIdx] : string.Empty;
  }

  /// <summary>
  /// Performs a zero-allocation check to see if the buffer at index matches the pattern.
  /// </summary>
  private static bool IsMatch(char[] buffer, int index, int length, string pattern)
  {
    if (index + pattern.Length > length) return false;
    for (int j = 0; j < pattern.Length; j++)
    {
      if (buffer[index + j] != pattern[j]) return false;
    }
    return true;
  }

  private static bool IsStatisticallyComment(int commentDelims, int totalDataDelims, int rows)
  {
    if (rows == 0) return true;
    double avg = (double) totalDataDelims / rows;
    // Comment is valid if its delimiter density is significantly different from data rows.
    return commentDelims < avg * 0.9 || commentDelims > avg * 1.1;
  }
}