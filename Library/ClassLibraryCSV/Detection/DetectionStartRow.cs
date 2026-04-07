using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace CsvTools;

/// <summary>
/// Analyzes file structure to identify the first row of actual data, skipping metadata or malformed headers.
/// </summary>
public static class DetectionStartRow
{
  /// <summary>
  /// Guesses the start row by analyzing column consistency across the first 50 rows.
  /// </summary>
  /// <returns>The number of rows to skip to reach the data body.</returns>
  public static async Task<int> InspectStartRowAsync(this ImprovedTextReader textReader,
    char fieldDelimiterChar, char fieldQualifierChar, char escapePrefixChar, string commentLine,
    CancellationToken cancellationToken)
  {
    if (textReader is null) throw new ArgumentNullException(nameof(textReader));
    if (commentLine is null) throw new ArgumentNullException(nameof(commentLine));

    const int maxRows = 50;
    var colCounts = new int[maxRows];
    var isComment = new bool[maxRows];

    int currentRow = 0;
    bool quoted = false;
    bool firstCharOfLine = true;
    char lastChar = '\0';
    bool hasFieldQualifier = fieldQualifierChar != char.MinValue;

    textReader.ToBeginning();
    char[] buffer = ArrayPool<char>.Shared.Rent(4096);

    try
    {
      while (currentRow < maxRows && !cancellationToken.IsCancellationRequested)
      {
        int charsRead = await textReader.ReadBlockAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (charsRead == 0) break;

        for (int i = 0; i < charsRead && currentRow < maxRows; i++)
        {
          char c = buffer[i];

          // 1. Handle Comment Detection
          if (firstCharOfLine && !char.IsWhiteSpace(c) && commentLine.Length > 0&&IsMatch(buffer, i, charsRead, commentLine))
            isComment[currentRow] = true;

          if (isComment[currentRow])
          {
            if (c is '\r' or '\n') HandleNewLine();
            continue;
          }

          // 2. Escape Logic
          if (c == escapePrefixChar && escapePrefixChar != char.MinValue)
          {
            i++; // Skip the escaped character
            firstCharOfLine = false;
            continue;
          }

          // 3. Quoting Logic
          if (hasFieldQualifier && c == fieldQualifierChar)
          {
            char next = (i + 1 < charsRead) ? buffer[i + 1] : '\0';
            if (quoted && next == fieldQualifierChar) i++; // Handle ""
            else quoted = !quoted;
            firstCharOfLine = false;
            continue;
          }

          // 4. Structural Delimiters
          if (!quoted)
          {
            if (c == fieldDelimiterChar)
            {
              colCounts[currentRow]++;
              firstCharOfLine = false;
            }
            else if (c is '\r' or '\n')
              HandleNewLine();
            else if (!char.IsWhiteSpace(c))
              firstCharOfLine = false;
          }

          lastChar = c;

          void HandleNewLine()
          {
            // Handle CRLF / LFCR pairs
            if (!((c == '\n' && lastChar == '\r') || (c == '\r' && lastChar == '\n')))
            {
              currentRow++;
              firstCharOfLine = true;
            }
          }
        }
      }
    }
    finally
    {
      ArrayPool<char>.Shared.Return(buffer);
    }

    return CalculateStartRow(colCounts, isComment, currentRow);
  }

  private static int CalculateStartRow(int[] colCounts, bool[] isComment, int totalRows)
  {
    // Filter out comments to find structural rows
    var structuralRows = new List<(int OriginalIndex, int Count)>();
    var commonCount = -1;
    var hasDifferences = false;
    for (int i = 0; i < totalRows; i++)
      if (!isComment[i])
      {
        structuralRows.Add((i, colCounts[i]));
        if (commonCount!=-1 && commonCount!=colCounts[i])
          hasDifferences=true;
        commonCount=colCounts[i];
      }

    if (structuralRows.Count < 2 || !hasDifferences)
      return 0;

    int take = Math.Min(15, structuralRows.Count);
    int start = structuralRows.Count - take;

    // Determine common column count in the tail
    var allowed = new List<int>();
    for (int i = structuralRows.Count - 1; i >= start; i--)
    {
      var count = structuralRows[i].Count;
      if (count > 0 && !allowed.Contains(count))
        allowed.Add(count); // later rows first
    }

    if (allowed.Count==0)
      return 0;
    Logger.Warning($"Detected column count variants: {string.Join(", ", allowed)}.");
    foreach (var row in structuralRows.Where(row => allowed.Contains(row.Count)))
      return row.OriginalIndex;

    return 0;
  }

  private static bool IsMatch(char[] buffer, int index, int length, string pattern)
  {
    if (index + pattern.Length > length) return false;
    for (int i = 0; i < pattern.Length; i++)
      if (buffer[index + i] != pattern[i]) return false;
    return true;
  }
}