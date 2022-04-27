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
using System.Linq;

namespace CsvTools
{
  /// <summary>
  ///   Class that is used to condense columns of a row in a sensible way, assuming a delimiter in a
  ///   column lead to more than the expected columns This is archived by looking at known good rows
  ///   and trying to find a pattern, this is best when identify able columns alternate, if all rows
  ///   are long text or all empty there is no way to say which column is not aligned.
  /// </summary>
  public class ReAlignColumns
  {
    private const int cMaxGoodRows = 40;
    private static readonly Random m_Random = new Random(Guid.NewGuid().GetHashCode());

    private static readonly string[] m_BoolVal = { "True", "False", "yes", "no", "1", "0", "-1", "y", "n", "", "x", "T", "F" };

    private readonly int m_ExpectedColumns;

    private readonly List<string[]> m_GoodRows = new List<string[]>();

    public ReAlignColumns(int expectedColumns) => m_ExpectedColumns = expectedColumns;

    /// <summary>
    ///   Adds a new row assuming the data is well aligned
    /// </summary>
    /// <param name="newRow">Array with the columns of that row</param>
    public void AddRow(in string[] newRow)
    {
      if (newRow is null) throw new ArgumentNullException(nameof(newRow));
      if (newRow.Length != m_ExpectedColumns)
        return;

      if (m_GoodRows.Count < cMaxGoodRows)
        m_GoodRows.Add(newRow);
      else
        // Store the row in our list
        m_GoodRows[m_Random.Next(0, cMaxGoodRows)] = newRow;
    }

    /// <summary>
    ///   Tries to condense the columns in a way that makes sense.
    /// </summary>
    /// <param name="row">Array with the columns of that row</param>
    /// <param name="handleWarning">Action to be called to store a warning</param>
    /// <param name="rawText">The raw text of the file before splitting it into columns</param>
    /// <returns>A new list of columns</returns>
    public string[] RealignColumn(in string[] row, Action<int, string> handleWarning, in string rawText)
    {
      if (row is null) throw new ArgumentNullException(nameof(row));
      if (handleWarning is null) throw new ArgumentNullException(nameof(handleWarning));

      if (m_GoodRows.Count < 2)
      {
        handleWarning.Invoke(-1, "Not enough error free rows have been read to allow realigning of columns.");
        return row;
      }

      // List is easier to handle than an array
      var columns = new List<string>(row);

      if (row.Length >= (m_ExpectedColumns * 2) - 1)
      {
        // take the columns as is...
        while (columns.Count > m_ExpectedColumns) columns.RemoveAt(m_ExpectedColumns);
        handleWarning.Invoke(m_ExpectedColumns - 1, "Information in following columns has been ignored.");
      }
      else
      {
        for (var colIndex = 0; colIndex < columns.Count; colIndex++) columns[colIndex] ??= string.Empty;
        //Get the Options for all good rows
        var otherColumns = new List<ColumnOption>(m_ExpectedColumns);
        for (var col2 = 0; col2 < m_ExpectedColumns; col2++)
          otherColumns.Add(GetColumnOptionAllRows(col2, m_GoodRows));
        var col = 0;
        // Step 1: try combining
        while (col < columns.Count && col < m_ExpectedColumns && columns.Count != m_ExpectedColumns)
        {
          if (string.IsNullOrEmpty(columns[col]) && !otherColumns[col].HasFlag(ColumnOption.Empty)
                                                 && GetColumnOption(columns[col + 1].Trim()) == otherColumns[col])
          {
            handleWarning.Invoke(col, "Empty column has been removed, assuming the data was misaligned.");
            columns.RemoveAt(col);
            continue;
          }

          if (otherColumns[col] != ColumnOption.None && col > 0)
          {
            var thisCol = GetColumnOption(columns[col].Trim());
            // assume we have to remove this columns
            if (!thisCol.HasFlag(otherColumns[col])
                || (thisCol == ColumnOption.None && thisCol == otherColumns[col - 1]))
            {
              var fromRaw = false;
              if (!string.IsNullOrEmpty(rawText) && columns[col - 1].Length > 0 && columns[col].Length > 0)
              {
                var pos1 = rawText.IndexOf(columns[col - 1], StringComparison.Ordinal);
                if (pos1 != -1)
                {
                  var pos2 = rawText.IndexOf(columns[col], pos1 + columns[col - 1].Length, StringComparison.Ordinal);
                  if (pos2 != -1)
                  {
                    fromRaw = true;
                    columns[col - 1] = rawText.Substring(pos1, pos2 + columns[col].Length - pos1);
                  }
                }
              }

              if (!fromRaw)
                columns[col - 1] += " " + columns[col];
              columns.RemoveAt(col);
              col--;
              handleWarning.Invoke(
                col,
                "Extra information from in next column has been appended, assuming the data was misaligned.");
            }
          }

          col++;
        }
      }

      return columns.ToArray();
    }

    /// <summary>
    ///   Looking ate the text sets certain flags
    /// </summary>
    /// <param name="text">The column information, best is trimmed</param>
    /// <returns>The appropriate column options</returns>
    private static ColumnOption GetColumnOption(string? text)
    {
      if (text is null || text.Length == 0)
        return ColumnOption.Empty;

      var all = ColumnOption.NumbersOnly | ColumnOption.DecimalChars | ColumnOption.DateTimeChars
                | ColumnOption.NoSpace;

      // compare the text as whole
      if (m_BoolVal.Any(test => test.Equals(text, StringComparison.OrdinalIgnoreCase))) all |= ColumnOption.Boolean;

      if (text.Length <= 30)
        all |= ColumnOption.ShortText;
      if (text.Length <= 10)
        all |= ColumnOption.VeryShortText;

      // check individual character
      foreach (var c in text)
      {
        if (all.HasFlag(ColumnOption.NumbersOnly) && "0123456789".IndexOf(c) == -1)
          all &= ~ColumnOption.NumbersOnly;
        if (all.HasFlag(ColumnOption.DecimalChars) && ".,-+ 0123456789".IndexOf(c) == -1)
          all &= ~ColumnOption.DecimalChars;
        if (all.HasFlag(ColumnOption.DateTimeChars) && ":/\\.-T 0123456789".IndexOf(c) == -1)
          all &= ~ColumnOption.DateTimeChars;
        if (all.HasFlag(ColumnOption.NoSpace) && (c < 32 || c > 125))
          all &= ~ColumnOption.NoSpace;

        if (all == ColumnOption.None)
          return ColumnOption.None;
      }

      return all;
    }

    /// <summary>
    ///   Get the combined option over all rows
    /// </summary>
    /// <param name="colNum">The Column Number in the array</param>
    /// <param name="rows">All rows to look at</param>
    private static ColumnOption GetColumnOptionAllRows(in int colNum, in ICollection<string[]> rows)
    {
      var overall = ColumnOption.Empty;
      foreach (var row in rows)
      {
        if (row.Length <= colNum) continue;
        var oneColOption = GetColumnOption(row[colNum].Trim());
        if (oneColOption == ColumnOption.Empty)
          continue;

        if (overall == ColumnOption.Empty)
          overall = oneColOption;
        else
          overall &= oneColOption;
      }

      return overall;
    }

    /// <summary>
    ///   A regular number like 2772 would be NumbersOnly | DecimalChars | DateTimeChars | NoSpace |
    ///   ShortText | VeryShortText
    /// </summary>
    [Flags]
    private enum ColumnOption
    {
      None = 0,

      Empty = 1,

      NumbersOnly = 2, // Only 0-9

      DecimalChars = 4, // Only . ,  + -

      DateTimeChars = 8, // Only / \ - . : T (space)

      NoSpace = 16, // 0-9 A-Z _ - (noSpace)

      ShortText = 32, // Length < 40

      VeryShortText = 64, // Length < 10

      Boolean = 128
    }
  }
}