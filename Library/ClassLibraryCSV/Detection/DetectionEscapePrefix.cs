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
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools;

/// <summary>
/// Static class with methods to detect the correct delimiter char
/// </summary>
public static class DetectionEscapePrefix
{
  /// <summary>
  /// Analyzes a text stream to determine the most likely escape character used in the CSV data.
  /// </summary>
  /// <param name="textReader">The custom reader used to access the file content.</param>
  /// <param name="fieldDelimiterChar">The previously detected column separator.</param>
  /// <param name="fieldQualifierChar">The character used to wrap fields (e.g., double quotes).</param>
  /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
  /// <returns>
  /// The detected escape character (often '\' or '"'). 
  /// Returns <see cref="char.MinValue"/> if no prefix escape is found.
  /// </returns>
  /// <remarks>
  /// This method uses an <see cref="ArrayPool{T}"/> for buffer management to avoid heap allocations
  /// during the analysis of the first 500 lines. It employs a weighted scoring system that 
  /// prioritizes escape sequences found inside quoted blocks.
  /// </remarks>
  public static async Task<char> InspectEscapePrefixAsync(this ImprovedTextReader textReader,
    char fieldDelimiterChar, char fieldQualifierChar, CancellationToken cancellationToken)
  {
    if (textReader is null)
      throw new ArgumentNullException(nameof(textReader));

    var checkedEscapeChars = StaticCollections.EscapePrefixChars;
    var possibleEscaped = new HashSet<char>(checkedEscapeChars);

    // Targets that make an escape "meaningful"
    if (fieldDelimiterChar != char.MinValue) possibleEscaped.Add(fieldDelimiterChar);
    if (fieldQualifierChar != char.MinValue) possibleEscaped.Add(fieldQualifierChar);
    foreach (var c in StaticCollections.DelimiterChars) possibleEscaped.Add(c);

    var scores = new int[checkedEscapeChars.Length];
    const int bufferSize = 4096;
    var buffer = ArrayPool<char>.Shared.Rent(bufferSize);

    bool isQuoted = false;
    char lastChar = '\0';
    int linesRead = 0;

    try
    {
      while (linesRead < 500 && !cancellationToken.IsCancellationRequested)
      {
        int charsRead = await textReader.ReadBlockAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (charsRead == 0) break;

        for (int i = 0; i < charsRead; i++)
        {
          char current = buffer[i];
          char next = (i + 1 < charsRead) ? buffer[i + 1] : '\0';

          // --- 1. HANDLE QUALIFIER / QUOTED STATE ---
          if (current == fieldQualifierChar)
          {
            // RFC 4180: If qualifier is escaped by itself ("")
            if (isQuoted && next == fieldQualifierChar)
            {
              // This is an escaped quote. We skip the next char and stay in 'isQuoted'
              i++;
              lastChar = current;
              continue;
            }

            isQuoted = !isQuoted;
            lastChar = current;
            continue;
          }

          // --- 2. EVALUATE PREFIX ESCAPES ---
          for (int e = 0; e < checkedEscapeChars.Length; e++)
          {
            char candidate = checkedEscapeChars[e];

            // We don't treat the qualifier as a prefix escape here 
            // because we handled the "" case above.
            if (current == candidate && candidate != fieldQualifierChar)
            {
              if (next != '\0' && possibleEscaped.Contains(next))
              {
                // High bonus for escapes used inside quoted fields
                scores[e] += isQuoted ? 5 : 2;

                // Consume the escaped character
                i++;
                current = next;
              }
              else if (next != '\0' && !char.IsWhiteSpace(next))
              {
                scores[e]--; // Penalty for noise (like C:\path)
              }
            }
          }

          // --- 3. LINE COUNTING ---
          if (current is '\n' or '\r')
          {
            if (current == '\n' && lastChar != '\r') linesRead++;
            else if (current == '\r') linesRead++;

            if (linesRead >= 500) break;
          }

          lastChar = current;
        }
      }
    }
    finally
    {
      ArrayPool<char>.Shared.Return(buffer);
    }

    // Winner selection
    char bestEscape = char.MinValue;
    int bestScore = 0;
    for (int i = 0; i < checkedEscapeChars.Length; i++)
    {
      if (scores[i] > bestScore)
      {
        bestScore = scores[i];
        bestEscape = checkedEscapeChars[i];
      }
    }

    return bestEscape;
  }
}