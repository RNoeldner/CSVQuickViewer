using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace CsvTools;

/// <summary>
/// Provides heuristics to detect the first data row in a delimited text file.
/// </summary>
/// <remarks>
/// The detection analyzes:
/// <list type="bullet">
///   <item><description>Column count consistency across early rows</description></item>
///   <item><description>Comment lines at the beginning of the file</description></item>
///   <item><description>Structural stability in the tail of inspected rows</description></item>
/// </list>
/// The result is the number of lines to skip before the actual data section starts.
/// </remarks>
public static class DetectionStartRow
{
  /// <summary>
  /// Analyzes the beginning of a delimited text file and determines the number of lines
  /// that should be skipped before reaching the first valid data row.
  /// </summary>
  /// <param name="textReader">Reader providing sequential access to the input file.</param>
  /// <param name="fieldDelimiterChar">Character used to separate fields (e.g. comma or semicolon).</param>
  /// <param name="fieldQualifierChar">Character used for quoting fields (e.g. ").</param>
  /// <param name="escapePrefixChar">Character used to escape special characters inside fields.</param>
  /// <param name="commentLine">
  /// Prefix that identifies a comment line (e.g. "#", "//"). Lines starting with this
  /// prefix are excluded from structural analysis.
  /// </param>
  /// <param name="cancellationToken">Cancellation token for interrupting processing.</param>
  /// <returns>
  /// The number of leading lines to skip in order to reach the first valid data row.
  /// </returns>
  /// <remarks>
  /// The method processes up to 50 logical rows and accounts for:
  /// <list type="bullet">
  ///   <item>Multiline quoted fields</item>
  ///   <item>Escaped delimiter and quote characters</item>
  ///   <item>Comment-only rows</item>
  /// </list>
  /// </remarks>
  public static async Task<int> InspectStartRowAsync(this ImprovedTextReader textReader,
    char fieldDelimiterChar, char fieldQualifierChar, char escapePrefixChar, string commentLine,
    CancellationToken cancellationToken)
  {
    if (textReader is null) throw new ArgumentNullException(nameof(textReader));
    if (commentLine is null) throw new ArgumentNullException(nameof(commentLine));

    const int maxRows = 50;
    var colCounts = new int[maxRows];
    var isCommentRow = false;
    var rowStartLine = new int[maxRows];
    int currentRow = 0;
    int currentLine = 1;
    bool quoted = false;
    bool escaped = false;
    bool startOfNewRow = true;
    char currentChar = '\0';
    bool hasFieldQualifier = fieldQualifierChar != char.MinValue;
    rowStartLine[currentRow] = currentLine;
    textReader.ToBeginning();
    char[] buffer = ArrayPool<char>.Shared.Rent(4096);
    // This is not quite correct, we want to determine the skip lines
    // but a row should spread over multiple lines, a row is not necessarily a line, it can be multiple
    // lines if there are line breaks in quoted fields
    try
    {
      while (currentRow < maxRows && !cancellationToken.IsCancellationRequested)
      {
        int charsRead = await textReader.ReadBlockAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (charsRead == 0) break;
        var start = 0;
        // Handle the magic key of Sep= like a comment line
        var spanStart = buffer.AsSpan(0, 10);
        if (spanStart.TrimStart().StartsWith("sep=", StringComparison.OrdinalIgnoreCase))
          start = spanStart.IndexOfAny(new[] { '\r', '\n' });
        for (int i = start; i < charsRead && currentRow < maxRows; i++)
        {
          var lastChar = currentChar;
          currentChar = buffer[i];
          if (lastChar == '\r' && currentChar == '\n')
            continue;
          if (startOfNewRow && currentChar==' ')
            continue;
          // 1. Handle Comment Detection
          if (startOfNewRow && commentLine.Length > 0 && IsMatch(buffer, i, charsRead, commentLine))
            isCommentRow = true;
          startOfNewRow = false;
          if (currentChar is '\r' or '\n')
          {
            HandleCRLF();
            isCommentRow = false;
            continue;
          }
          if (isCommentRow)
            continue;
          if (escaped)
          {
            escaped = false;
            continue;
          }
          // 2. Escape Logic
          if (currentChar == escapePrefixChar && escapePrefixChar != char.MinValue)
          {
            escaped = true;
            continue;
          }

          // 3. Quoting Logic
          if (hasFieldQualifier && currentChar == fieldQualifierChar)
          {
            char next = (i + 1 < charsRead) ? buffer[i + 1] : '\0';
            if (quoted && next == fieldQualifierChar) i++; // Handle ""
            else quoted = !quoted;
            continue;
          }

          // 4. Structural Delimiters
          if (!quoted && currentChar == fieldDelimiterChar)
            colCounts[currentRow]++;
        } // for / buffer
      } // while

      void HandleCRLF()
      {
        currentLine++;
        if (!quoted)
        {
          currentRow++;
          startOfNewRow = true;
          if (currentRow < maxRows)
            rowStartLine[currentRow] = currentLine;
        }
      }
    }
    finally
    {
      ArrayPool<char>.Shared.Return(buffer);
    }
    return CalculateSkipLine(colCounts, rowStartLine, currentRow);
  }

  /// <summary>
  /// Determines the first meaningful data row based on column consistency and comment filtering,
  /// then converts it into a line index.
  /// </summary>
  /// <param name="colCounts">Detected column counts per row.</param>
  /// <param name="rowStartLine">Mapping from row index to physical line index.</param>
  /// <param name="totalRows">Number of analyzed rows.</param>
  /// <returns>Line index to start reading actual data from.</returns>
  private static int CalculateSkipLine(int[] colCounts, int[] rowStartLine, int totalRows)
  {
    // Filter out comments to find structural rows
    var structuralRows = new List<(int RowIndex, int Count)>();

    for (int i = 0; i < totalRows; i++)
    {
      int count = colCounts[i];
      if (count > 0)
        structuralRows.Add((i, count));
    }
    if (structuralRows.Count < 2)
      return 0;

    // Determine common column count in the tail
    var allowed = new HashSet<int>();
    // Take up to the first 10
    for (int i = 0; i < Math.Min(10, structuralRows.Count); i++)
      allowed.Add(structuralRows[i].Count);
    // Take up to the last 10
    int lastStart = Math.Max(0, structuralRows.Count - 10);
    for (int i = lastStart; i < structuralRows.Count; i++)
      allowed.Add(structuralRows[i].Count);

    var firstRow = structuralRows.Where(row => allowed.Contains(row.Count)).Select(row => row.RowIndex).FirstOrDefault();
    return rowStartLine[firstRow]-1;
  }

  /// <summary>
  /// Compares a buffer segment with a fixed string pattern.
  /// </summary>
  /// <remarks>
  /// Used for fast detection of comment prefixes at the beginning of a row.
  /// </remarks>
  private static bool IsMatch(char[] buffer, int index, int length, string pattern)
  {
    if (index + pattern.Length > length) return false;
    for (int i = 0; i < pattern.Length; i++)
      if (buffer[index + i] != pattern[i]) return false;
    return true;
  }
}