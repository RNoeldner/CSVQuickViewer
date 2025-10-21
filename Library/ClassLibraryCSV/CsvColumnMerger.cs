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
#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CsvTools
{
  /// <summary>
  ///   Class that is used to condense columns of a unalignedRow in a sensible way, assuming a delimiter in a
  ///   column lead to more than the expected columns This is archived by looking at known good rows
  ///   and trying to find a pattern, this is best when identify able columns alternate, if all rows
  ///   are long text or all empty there is no way to say which column is not aligned.
  /// </summary>
  public sealed class CsvColumnMerger
  {
    private const int cMaxGoodRows = 40;
#if NET6_0_OR_GREATER
    private static readonly Random Random = Random.Shared;
#else
    private static readonly Random Random = new Random(Environment.TickCount);
#endif
    private readonly char m_Delimiter;
    private static readonly HashSet<string> BoolSet = new HashSet<string>(
    new[] { "True", "False", "yes", "no", "1", "0", "-1", "y", "n", "", "x", "T", "F" },
    StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<char> DigitsSet = new("0123456789");
    private static readonly HashSet<char> DecimalCharsSet = new(".,-+ 0123456789");
    private static readonly HashSet<char> DateTimeCharsSet = new(":/\\.-T 0123456789");

    private static readonly Regex EmailRegex = new Regex(
    @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
    RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex UrlRegex = new Regex(
        @"^(https?://)?([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}(:\d+)?(/[\w\-.~:/?#[\]@!$&'()*+,;=%]*)?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly int m_ExpectedColumns;

    private readonly List<string[]> m_GoodRows = new List<string[]>(cMaxGoodRows);

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvColumnMerger"/> class.
    /// </summary>
    /// <param name="expectedColumns">The expected columns.</param>
    /// <param name="delimiter">The delimiter used in the CSV file.</param>
    public CsvColumnMerger(int expectedColumns, char delimiter)
    {
      m_ExpectedColumns = expectedColumns;
      m_Delimiter=delimiter;
    }

    /// <summary>
    ///   Adds a new unalignedRow assuming the data is well aligned
    /// </summary>
    /// <param name="newRow">Array with the columns of that unalignedRow</param>
    public void AddAlignedRow(string[] newRow)
    {
      if (newRow is null) throw new ArgumentNullException(nameof(newRow));
      if (newRow.Length != m_ExpectedColumns)
        return;

      if (m_GoodRows.Count < cMaxGoodRows)
        m_GoodRows.Add(newRow);
      else
        // Store the unalignedRow in our list at some random location
        m_GoodRows[Random.Next(0, cMaxGoodRows)] = newRow;
    }

    /// <summary>
    ///   Tries to condense (combine two columns into one) in a way that makes sense.
    /// </summary>
    /// <param name="unalignedRow">Array with the columns of that unalignedRow</param>
    /// <param name="handleWarning">Action to be called to store a warning</param>
    /// <param name="rawText">The raw text of the file before splitting it into columns</param>
    /// <param name="parseColumn">
    /// A delegate used to re-parse the merged raw substring into a properly
    /// normalized column value (handles trimming, escaping, qualifiers, etc.).
    /// </param>
    /// <returns>A new list of columns</returns>
    public IReadOnlyList<string> MergeMisalignedColumns(
    IReadOnlyList<string> unalignedRow,
    Action<int, string> handleWarning, Func<int, string, string> parseColumn,
    in string rawText)
    {
      if (unalignedRow is null) throw new ArgumentNullException(nameof(unalignedRow));
      if (handleWarning is null) throw new ArgumentNullException(nameof(handleWarning));

      if (m_GoodRows.Count < 3)
      {
        handleWarning.Invoke(-1, "Not enough error free rows have been read to allow realigning of columns.");
        return unalignedRow;
      }

      // List is easier to handle than an array
      var columns = new List<string>(unalignedRow);

      // Too many columns → truncate
      if (unalignedRow.Count >= m_ExpectedColumns * 2 - 1)
      {
        while (columns.Count > m_ExpectedColumns) columns.RemoveAt(m_ExpectedColumns);
        handleWarning.Invoke(m_ExpectedColumns - 1, "Information in following columns has been ignored.");
        return columns;
      }

      // Ensure no nulls
      for (var colIndex = 0; colIndex < columns.Count; colIndex++)
        columns[colIndex] ??= string.Empty;

      // Expected column types from good rows
      var expected = new List<ColumnCharacteristic>(m_ExpectedColumns);
      for (var col2 = 0; col2 < m_ExpectedColumns; col2++)
        expected.Add(GetColumnOptionAllRows(col2, m_GoodRows));

      // Try merging until we match expected count
      while (columns.Count > m_ExpectedColumns)
      {
        var bestIndex = SelectBestMergeIndex(columns, expected);
        if (bestIndex < 1)
          break;
        columns[bestIndex-1] = MergeUsingRawText(columns, bestIndex, rawText, parseColumn);
        columns.RemoveAt(bestIndex);

        handleWarning.Invoke(
          bestIndex - 1,
          "Extra information from next column has been appended, assuming the data was misaligned.");
      }

      return (columns.Count == unalignedRow.Count && unalignedRow.Count == m_ExpectedColumns)
          ? unalignedRow : columns.ToArray();
    }

    /// <summary>
    /// Merges two adjacent columns from a CSV row by locating their exact positions
    /// in the original raw CSV text. This ensures that the merge preserves the
    /// original formatting, including embedded delimiters, quotes, and whitespace.
    /// </summary>
    /// <param name="columns">
    /// The list of already-split column values for the row. These are the parsed
    /// values, which may differ from the raw text due to trimming or unescaping.
    /// </param>
    /// <param name="mergeIndex">
    /// The index of the column to merge into its left neighbor (i.e., merges
    /// <c>columns[mergeIndex]</c> into <c>columns[mergeIndex - 1]</c>).
    /// </param>
    /// <param name="rawText">
    /// The unmodified CSV line from which the <paramref name="columns"/> were
    /// originally parsed. Used to find the exact substring between the two columns.
    /// </param>
    /// <param name="parseColumn">
    /// A delegate used to re-parse the merged raw substring into a properly
    /// normalized column value (handles trimming, escaping, qualifiers, etc.).
    /// </param>
    /// <returns>
    /// A new array of strings with <c>columns[mergeIndex]</c> merged into its
    /// left neighbor, resulting in one fewer element than <paramref name="columns"/>.
    /// If the column boundaries cannot be reliably located in
    /// <paramref name="rawText"/>, the method falls back to a simple delimiter-based
    /// merge.
    /// </returns>

    private string MergeUsingRawText(List<string> columns, int mergeIndex, string rawText, Func<int, string, string> parseColumn)
    {
      // Compute start positions
      var starts = new int[mergeIndex+2];
      int pos = 0;

      // columns handled escaped quotes etc, so searching for the column text is not reliable
      for (int i = 0; i < mergeIndex+2; i++)
      {
        if (columns.Count== i)
        {
          starts[i] = pos + 1;
          continue;
        }
        if (!string.IsNullOrEmpty(columns[i]))
        {
          starts[i] = rawText.IndexOf(columns[i], pos, StringComparison.Ordinal);
          if (starts[i] != -1)
            pos = starts[i] + columns[i].Length;
        }
        else
        {
          starts[i] = rawText.IndexOf(m_Delimiter, pos);
          if (starts[i] != -1)
            pos = starts[i] + 1;
        }

      }
      // Could we locate the part of raw text we need?
      if (starts[mergeIndex - 1]!=-1 && starts[mergeIndex+1]!=-1)
      {
        var newColumText = parseColumn(mergeIndex - 1, rawText.Substring(starts[mergeIndex - 1], starts[mergeIndex + 1] - starts[mergeIndex - 1] -1));
        // Only if the new text is different to what we had
        if (!string.IsNullOrEmpty(newColumText) && newColumText!=columns[mergeIndex - 1])
          return newColumText;
      }

      // Simple delimiter based merge
      return columns[mergeIndex - 1] + m_Delimiter + columns[mergeIndex];
    }

    /// <summary>
    ///   Scores a list row by comparing detected column characteristics to expected characteristics.
    ///   Positive scores favor good matches; negative scores penalize mismatches.
    ///   Soft scoring allows "almost matching" characteristics to gain partial credit.
    /// </summary>
    private int ScoreCandidate(string[] candidate, IReadOnlyList<ColumnCharacteristic> expected)
    {
      int score = 0;

#if DEBUG
      var sb = new System.Text.StringBuilder();
#endif

      for (int colNo = 0; colNo < Math.Min(candidate.Length, expected.Count); colNo++)
      {
        var thisCol = GetCharacteristicScore(expected[colNo], GetColumnOption(candidate[colNo]));
#if DEBUG
        sb.AppendFormat("Col {0}: Expected '{1}', Actual '{2}' => {3}", colNo, expected[colNo], GetColumnOption(candidate[colNo]), thisCol);
        sb.AppendLine();
#endif
        score += thisCol;
      }
#if DEBUG
      System.Diagnostics.Debugger.Log(1, "Debug", sb.ToString());
#endif
      return score;
    }

    private static int GetCharacteristicScore(ColumnCharacteristic expected, ColumnCharacteristic actual)
    {
      int score = 0;
      // if we have NumbersOnly and DecimalChars in both its counts extra
      if (expected.HasFlag(ColumnCharacteristic.NumbersOnly) && actual.HasFlag(ColumnCharacteristic.DecimalChars)
        ||expected.HasFlag(ColumnCharacteristic.DecimalChars) && actual.HasFlag(ColumnCharacteristic.NumbersOnly))
        score+=1;

      if ((expected.HasFlag(ColumnCharacteristic.Short) && actual.HasFlag(ColumnCharacteristic.VeryShort)) ||
          (expected.HasFlag(ColumnCharacteristic.VeryShort) && actual.HasFlag(ColumnCharacteristic.Short)))
        score+=1;

      foreach (ColumnCharacteristic flag in Enum.GetValues(typeof(ColumnCharacteristic)))
      {
        if (flag == ColumnCharacteristic.None)
          continue;
        if (expected.HasFlag(flag) && actual.HasFlag(flag))
        {
          score+=2;
          // very strong characteristic value more
          if (flag is ColumnCharacteristic.Email or ColumnCharacteristic.Url or ColumnCharacteristic.Long or ColumnCharacteristic.Empty)
            score++;
        }

      }
      return score;
    }

    /// <summary>
    ///   Tests all possible merge operations (concatenating column <c>colNo</c> into <c>colNo-1</c>)
    ///   and selects the index that produces the highest scoring list.
    /// </summary>
    /// <param name="columns">The current row as a list of columns.</param>
    /// <param name="expected">The expected column type information derived from good rows.</param>
    /// <returns>
    ///   The index of the column to merge into its left neighbor, or -1 if no suitable merge is found.
    /// </returns>
    private int SelectBestMergeIndex(List<string> columns, IReadOnlyList<ColumnCharacteristic> expected)
    {
      int bestScore = int.MinValue;
      int bestIndex = -1;

      // Initialize candidate array once
      var candidate = new string[columns.Count - 1];
      for (int colNo = 1; colNo < columns.Count; colNo++)
      {
        // Fill candidate array by merging colNo into colNo-1
        for (int i = 0, j = 0; i < columns.Count; i++)
        {
          if (i == colNo) continue; // skip the column to merge

          if (i == colNo - 1)
            candidate[j++] = columns[i] + m_Delimiter + columns[colNo]; // merge into previous and add an artificial separator
          else
            candidate[j++] = columns[i]; // copy as-is
        }

        int score = ScoreCandidate(candidate, expected);
        if (expected.Count>colNo-1)
        {
          // If the column that is going to get the content added was text column give it a bonus
          if (!expected[colNo-1].HasFlag(ColumnCharacteristic.NumbersOnly)
            && !expected[colNo-1].HasFlag(ColumnCharacteristic.DecimalChars)
            && !expected[colNo-1].HasFlag(ColumnCharacteristic.DateTimeChars)
            && !expected[colNo-1].HasFlag(ColumnCharacteristic.NoSpace))
            score += 4;
        }
        if (score > bestScore)
        {
          bestScore = score;
          bestIndex = colNo;
        }
      }

      return bestIndex;
    }

    /// <summary>
    ///   Analyzes the content of a single column and returns its characteristics
    ///   (e.g., numeric, boolean, short text, contains date characters).
    /// </summary>
    /// <param name="text">The text of the column, preferably trimmed.</param>
    /// <returns>A set of <see cref="ColumnCharacteristic"/> flags describing the detected characteristics.</returns>
    private static ColumnCharacteristic GetColumnOption(string? text)
    {
      if (text is null || text.Length == 0)
        return ColumnCharacteristic.Empty;

      var all = ColumnCharacteristic.NumbersOnly | ColumnCharacteristic.DecimalChars | ColumnCharacteristic.DateTimeChars | ColumnCharacteristic.NoSpace;

      // compare the text as whole
      if (BoolSet.Contains(text))
        all |= ColumnCharacteristic.Boolean;

      switch (text.Length)
      {
        case <= 10:
          all |= ColumnCharacteristic.VeryShort;
          break;
        case <= 30:
          all |= ColumnCharacteristic.Short;
          break;
        case > 80:
          all |= ColumnCharacteristic.Long;
          break;
      }

      // Detect Email
      if (EmailRegex.IsMatch(text)) all |= ColumnCharacteristic.Email;

      // Detect URL
      if (UrlRegex.IsMatch(text)) all |= ColumnCharacteristic.Url;

      // check individual character
      foreach (var c in text)
      {
        if (all.HasFlag(ColumnCharacteristic.NumbersOnly) &&  !DigitsSet.Contains(c))
          all &= ~ColumnCharacteristic.NumbersOnly;
        if (all.HasFlag(ColumnCharacteristic.DecimalChars) && !DecimalCharsSet.Contains(c))
          all &= ~ColumnCharacteristic.DecimalChars;
        if (all.HasFlag(ColumnCharacteristic.DateTimeChars) && !DateTimeCharsSet.Contains(c))
          all &= ~ColumnCharacteristic.DateTimeChars;
        if (all.HasFlag(ColumnCharacteristic.NoSpace) && char.IsWhiteSpace(c))
          all &= ~ColumnCharacteristic.NoSpace;

        if (all == ColumnCharacteristic.None)
          return ColumnCharacteristic.None;
      }

      return all;
    }

    /// <summary>
    ///   Aggregates column characteristics across all "good" rows.
    ///   The result represents the intersection of detected options (i.e., the common traits).
    /// </summary>
    /// <param name="colNum">The column index to evaluate.</param>
    /// <param name="rows">The set of rows considered "well-aligned".</param>
    /// <returns>The combined <see cref="ColumnCharacteristic"/> flags for the given column.</returns>
    private static ColumnCharacteristic GetColumnOptionAllRows(int colNum, in ICollection<string[]> rows)
    {
      var overall = ColumnCharacteristic.Empty;
      foreach (var row in rows)
      {
        if (row.Length <= colNum) continue;
        var oneColOption = GetColumnOption(row[colNum]);
        if (oneColOption == ColumnCharacteristic.Empty)
          continue;

        if (overall == ColumnCharacteristic.Empty)
          overall = oneColOption;
        else
          overall &= oneColOption;

        if (overall == ColumnCharacteristic.None)
          break;
      }

      return overall;
    }

    /// <summary>
    ///   A regular number like 2772 would be NumbersOnly | DecimalChars | DateTimeChars | NoSpace |
    ///   Short | VeryShort
    /// </summary>
    [Flags]
    private enum ColumnCharacteristic
    {
      None = 0,

      Empty = 1,

      NumbersOnly = 1 << 1, // Only 0-9

      DecimalChars = 1 << 2, // Only . ,  + - and numbers

      DateTimeChars = 1 << 3, // Only / \ - . : T (space)

      NoSpace = 1 << 4, // Does not contain spaces

      Long = 1 << 5, // Length > 80

      Short = 1 << 6, // Length < 40

      VeryShort = 1 << 7, // Length < 10

      Boolean = 1 << 8,

      Email = 1 << 9, // Looks like an email address

      Url = 1 << 10,    // Looks like a URL
    }
  }
}
