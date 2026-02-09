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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools;

/// <summary>
/// Static class with methods for RecordDelimiter Detection
/// </summary>
public static class DetectionRecordDelimiter
{
  /// <summary>
  /// Determines the new line sequence from the text stream while respecting quoted fields.
  /// </summary>
  /// <param name="textReader">The reader to read data from.</param>
  /// <param name="fieldQualifierChar">Qualifier char to allow delimiters inside columns.</param>
  /// <param name="cancellationToken">Cancellation token to stop the process.</param>
  /// <returns>The detected <see cref="RecordDelimiterTypeEnum"/>.</returns>
  public static async Task<RecordDelimiterTypeEnum> InspectRecordDelimiterAsync(
    this ImprovedTextReader textReader, char fieldQualifierChar, CancellationToken cancellationToken)
  {
    if (textReader is null) throw new ArgumentNullException(nameof(textReader));

    // Limits the scan to 8KB of data
    const int maxCharsToScan = 8192;
    int charsProcessed = 0;
    bool quoted = false;

    // Index constants for the count array
    const int idxCr = 0, idxLf = 1, idxCrlf = 2, idxLfCr = 3, idxRs = 4, idxUs = 5, idxNl = 6;
    int[] counts = new int[7];

    textReader.ToBeginning();
    char[] buffer = ArrayPool<char>.Shared.Rent(4096);
    try
    {
      while (charsProcessed < maxCharsToScan && !cancellationToken.IsCancellationRequested)
      {
        int charsRead = await textReader.ReadBlockAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (charsRead == 0) break;

        for (int i = 0; i < charsRead; i++)
        {
          char c = buffer[i];
          charsProcessed++;

          // 1. Handle Quoting State
          if (c == fieldQualifierChar && fieldQualifierChar != char.MinValue)
          {
            // Check for escaped/double quote
            char next = (i + 1 < charsRead) ? buffer[i + 1] : '\0';
            if (quoted && next == fieldQualifierChar)
            {
              i++; // Skip the second quote
              continue;
            }
            quoted = !quoted;
          }

          if (quoted) continue;

          // 2. Detect Record Separators
          switch ((int) c)
          {
            case 21: // NAK / NL
              counts[idxNl]++;
              break;
            case 30: // RS (Record Separator)
              counts[idxRs]++;
              break;
            case 31: // US (Unit Separator)
              counts[idxUs]++;
              break;
            case 10: // LF
            {
              char next = (i + 1 < charsRead) ? buffer[i + 1] : '\0';
              if (next == 13)
              {
                counts[idxLfCr]++;
                i++; // Consume the CR
              }
              else
              {
                counts[idxLf]++;
              }
              break;
            }
            case 13: // CR
            {
              char next = (i + 1 < charsRead) ? buffer[i + 1] : '\0';
              if (next == 10)
              {
                counts[idxCrlf]++;
                i++; // Consume the LF
              }
              else
              {
                counts[idxCr]++;
              }
              break;
            }
          }

          if (charsProcessed >= maxCharsToScan) break;
        }
      }
    }
    finally
    {
      ArrayPool<char>.Shared.Return(buffer);
    }

    int maxCount = counts.Max();
    if (maxCount == 0) return RecordDelimiterTypeEnum.None;
    if (counts[idxCrlf] == maxCount) return RecordDelimiterTypeEnum.Crlf; // Prioritize CRLF
    if (counts[idxLf] == maxCount) return RecordDelimiterTypeEnum.Lf;
    if (counts[idxCr] == maxCount) return RecordDelimiterTypeEnum.Cr;
    if (counts[idxLfCr] == maxCount) return RecordDelimiterTypeEnum.Lfcr;
    if (counts[idxRs] == maxCount) return RecordDelimiterTypeEnum.Rs;
    if (counts[idxUs] == maxCount) return RecordDelimiterTypeEnum.Us;
    if (counts[idxNl] == maxCount) return RecordDelimiterTypeEnum.Nl;

    return RecordDelimiterTypeEnum.None;
  }
}