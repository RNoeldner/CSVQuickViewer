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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools;

/// <summary>
/// Static class for high-performance CSV Qualifier (Quote) detection.
/// </summary>
public static class DetectionQualifier
{
  /// <summary>
  /// Try to determine the most likely quote character by performing a weighted analysis of the file.
  /// </summary>
  public static QuoteTestResult InspectQualifier(
      this ImprovedTextReader textReader,
      char fieldDelimiterChar,
      char escapePrefixChar,
      string commentLine,
      IEnumerable<char> possibleQuotes,
      CancellationToken cancellationToken)
  {
    if (textReader is null) throw new ArgumentNullException(nameof(textReader));

    var bestResult = new QuoteTestResult();

    foreach (var quoteChar in possibleQuotes)
    {
      cancellationToken.ThrowIfCancellationRequested();

      // Analyze the file for this specific quote candidate
      var currentResult = GetScoreForQuote(textReader, fieldDelimiterChar, escapePrefixChar, quoteChar, commentLine, cancellationToken);

      if (currentResult.Score > bestResult.Score)
        bestResult = currentResult;

      // Short-circuit: If double-quote looks very solid, don't waste time on other candidates
      if (quoteChar == '"' && currentResult.Score >= 45)
        break;
    }

    return bestResult;
  }

  private static QuoteTestResult GetScoreForQuote(
      ImprovedTextReader textReader,
      char delimiterChar,
      char escapeChar,
      char quoteChar,
      string commentLine,
      CancellationToken cancellationToken)
  {
    const int cBufferMax = 262144;
    const char textPlaceholder = 't';

    textReader.ToBeginning();

    // Use ArrayPool to avoid massive heap allocations for every candidate quote
    var analysisBuffer = ArrayPool<char>.Shared.Rent(cBufferMax + 5);
    var readBuffer = ArrayPool<char>.Shared.Rent(4096);

    try
    {
      int analysisPos = 0;
      char lastNormalized = '\0';
      bool isStartOfLine = true;
      var res = new QuoteTestResult(quoteChar);

      // Analysis state
      analysisBuffer[analysisPos++] = delimiterChar; // Seed with delimiter

      while (analysisPos < cBufferMax && !cancellationToken.IsCancellationRequested)
      {
        int charsRead = textReader.ReadBlock(readBuffer, 0, readBuffer.Length);
        if (charsRead == 0) break;

        for (int i = 0; i < charsRead && analysisPos < cBufferMax; i++)
        {
          char c = readBuffer[i];

          // 1. Skip Comments
          if (isStartOfLine && !string.IsNullOrEmpty(commentLine) && c == commentLine[0])
          {
            // Optimization: Simple skip to end of line
            while (i < charsRead && readBuffer[i] != '\n' && readBuffer[i] != '\r') i++;
            isStartOfLine = true;
            continue;
          }

          char normalized;
          if (c is '\r' or '\n')
          {
            isStartOfLine = true;
            continue;
          }

          isStartOfLine = false;

          if (c == ' ' || c == '\t') continue; // Ignore noise

          if (c == escapeChar)
          {
            // Peek at next char to check for escaped qualifier
            int nextIdx = i + 1;
            if (nextIdx < charsRead && readBuffer[nextIdx] == quoteChar)
            {
              res.EscapedQualifier = true;
              i++; // Skip the escaped char
            }
            normalized = textPlaceholder;
          }
          else if (c == quoteChar)
          {
            // Check for duplicate quote (RFC 4180 style: "")
            int nextIdx = i + 1;
            if (nextIdx < charsRead && readBuffer[nextIdx] == quoteChar)
            {
              res.DuplicateQualifier = true;
              i++; // Treat "" as text
              normalized = textPlaceholder;
            }
            else
            {
              normalized = quoteChar;
            }
          }
          else if (c == delimiterChar)
          {
            normalized = delimiterChar;
          }
          else
          {
            normalized = textPlaceholder;
          }

          // Only add to analysis buffer if it represents a state change (compression)
          if (normalized != lastNormalized)
          {
            analysisBuffer[analysisPos++] = normalized;
            lastNormalized = normalized;
          }
        }
      }

      return CalculateFinalScore(analysisBuffer, analysisPos, quoteChar, delimiterChar, res);
    }
    finally
    {
      ArrayPool<char>.Shared.Return(analysisBuffer);
      ArrayPool<char>.Shared.Return(readBuffer);
    }
  }

  /// <summary>
  /// Checks if the specified qualifier is actually used at the start of fields in the file.
  /// This is an optimized, zero-allocation scan.
  /// </summary>
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

    // Use the ImprovedTextReader to handle encoding and skipping rows efficiently
    using var streamReader = await stream.GetTextReaderAsync(codePageId, skipRows, cancellationToken).ConfigureAwait(false);

    const int bufferSize = 4096;
    var buffer = ArrayPool<char>.Shared.Rent(bufferSize);

    try
    {
      bool isStartOfColumn = true;
      char lastChar = '\0';

      while (true)
      {
        int charsRead = await streamReader.ReadBlockAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (charsRead == 0) break;

        for (int i = 0; i < charsRead; i++)
        {
          char c = buffer[i];

          // 1. Handle line breaks robustly to reset column state
          if (c is '\r' or '\n')
          {
            // Reset column state on new lines (skipping CRLF pairs)
            if (!((c == '\n' && lastChar == '\r') || (c == '\r' && lastChar == '\n')))
            {
              isStartOfColumn = true;
            }
            lastChar = c;
            continue;
          }

          // 2. Delimiter found? The next character is the start of a column
          if (c == fieldDelimiterChar)
          {
            isStartOfColumn = true;
            lastChar = c;
            continue;
          }

          // 3. If we are at the start of a column, check for the qualifier
          if (isStartOfColumn)
          {
            // Skip leading whitespace before the qualifier
            if (c is ' ' or '\t')
            {
              continue;
            }

            // SUCCESS: We found the qualifier exactly where it's supposed to be
            if (c == fieldQualifierChar)
              return true;

            // If we find any other character first, it's not a quoted column
            isStartOfColumn = false;
          }

          lastChar = c;
        }

        // Safety check for long-running scans
        cancellationToken.ThrowIfCancellationRequested();
      }
    }
    finally
    {
      ArrayPool<char>.Shared.Return(buffer);
    }

    return false;
  }

  private static QuoteTestResult CalculateFinalScore(char[] buffer, int length, char quoteChar, char delimiterChar, QuoteTestResult res)
  {
    int openAndText = 0;
    int closeAndDelim = 0;
    int totalQuotes = 0;

    // Add padding for safe look-ahead/behind
    buffer[length] = delimiterChar;
    buffer[length + 1] = delimiterChar;

    for (int i = 1; i < length; i++)
    {
      if (buffer[i] != quoteChar) continue;
      totalQuotes++;

      // Check if quote follows a delimiter (Potential Start)
      if (buffer[i - 1] == delimiterChar && buffer[i + 1] == 't')
        openAndText++;

      // Check if quote precedes a delimiter (Potential End)
      if (buffer[i + 1] == delimiterChar && buffer[i - 1] == 't')
        closeAndDelim++;
    }

    if (totalQuotes == 0) return res;

    // Scoring: Heavily weight cases where quotes wrap text fields correctly
    int rawScore = totalQuotes + (5 * (openAndText + closeAndDelim));

    if (res.DuplicateQualifier || res.EscapedQualifier)
      rawScore += 20;

    res.Score = Math.Min(99, (int) ((rawScore / (double) length) * 100) + 1);
    return res;
  }

  /// <summary>
  /// Outcome of the Tests
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
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