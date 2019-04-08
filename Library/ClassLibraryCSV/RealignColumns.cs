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

namespace CsvTools
{
  /// <summary>
  /// Class that is used to condense columns of a row in a sensible way, assuming a delimiter in a column lead to more than the expected columns
  /// </summary>
  public class ReAlignColumns
  {
    private const int MaxGoodRows = 40;
    private readonly int m_ExcpectedColumns;
    private readonly List<string[]> m_GoodRows = new List<string[]>();

    public ReAlignColumns(int excpectedColumns)
    {
      m_ExcpectedColumns = excpectedColumns;
    }

    [Flags]
    private enum ColumnOption
    {
      None = 0,
      Empty = 1,
      NumbersOnly = 2,     // Only 0-9
      DecimalChars = 4,    // Only . ,  + -
      DateTimeChars = 8,   // Only / \ - . : T (space)
      Word = 16,           // 0-9 A-Z _ - (noSpace)
      ShortText = 32,      // Length < 40
      VeryShortText = 64,  // Length < 10
    }

    /// <summary>
    /// Adds a new row assuming the data is well aligned
    /// </summary>
    /// <param name="newRow">Array with the columns of that row</param>
    public void AddRow(string[] newRow)
    {
      if (m_GoodRows.Count < MaxGoodRows)
        m_GoodRows.Add(newRow);
      else
        // Store the row in our list
        m_GoodRows[SecureString.Random.Next(0, MaxGoodRows)] = newRow;
    }

    /// <summary>
    /// Tries to condense the columns in a way that makes sense.
    /// </summary>
    /// <param name="row">Array with the columns of that row</param>
    /// <param name="handleWarning">Action to be called to store a warning</param>
    /// <returns>A new list of columns</returns>
    public string[] RealignColumn(string[] row, Action<int, string> handleWarning)
    {
      if (row == null)
        throw new ArgumentNullException(nameof(row));

      if (handleWarning == null)
        throw new ArgumentNullException(nameof(handleWarning));

      var columns = new List<string>(row);

      //Get the Options for all good rows
      List<ColumnOption> otherColumns = new List<ColumnOption>(m_ExcpectedColumns);
      for (int col2 = 0; col2 < m_ExcpectedColumns; col2++)
        otherColumns.Add(GetColumnOptionAllRows(col2, m_GoodRows));

      if (row.Length == m_ExcpectedColumns * 2 - 1 && m_GoodRows.Count == 0)
      {
        // take the columns as is...
        while (columns.Count > m_ExcpectedColumns)
        {
          columns.RemoveAt(m_ExcpectedColumns);
        }
        handleWarning(m_ExcpectedColumns - 1, "Information in following columns has been ignored.");
      }
      else
      {
        int col = 1;
        while (col < columns.Count && col < m_ExcpectedColumns && columns.Count != m_ExcpectedColumns)
        {
          if (otherColumns[col] != ColumnOption.None)
          {
            var thisCol = GetColumnOption(columns[col]);
            // assume we have to remove this columns
            if (!thisCol.HasFlag(otherColumns[col]) || (thisCol == ColumnOption.None && thisCol == otherColumns[col - 1]))
            {
              columns[col - 1] = columns[col - 1] + " " + columns[col];
              handleWarning(col - 1, "Extra information from in next column has been appended, assuming the data was misaligned.");

              // we remove this data to allow realign
              columns.RemoveAt(col);
              // retest the same column
              continue;
            }
          }
          col++;
        }
      }
      return columns.ToArray();
    }

    private static ColumnOption GetColumnOption(string text)
    {
      if (string.IsNullOrEmpty(text))
        return ColumnOption.Empty;
      ColumnOption all = ColumnOption.NumbersOnly | ColumnOption.DecimalChars | ColumnOption.DateTimeChars | ColumnOption.Word;
      if (text.Length <= 40)
        all |= ColumnOption.ShortText;
      if (text.Length <= 10)
        all |= ColumnOption.VeryShortText;
      foreach (var c in text)
      {
        if (all.HasFlag(ColumnOption.NumbersOnly) && "0123456789".IndexOf(c) == -1)
          all &= ~ColumnOption.NumbersOnly;
        if (all.HasFlag(ColumnOption.DecimalChars) && ".,-+ ".IndexOf(c) == -1)
          all &= ~ColumnOption.DecimalChars;
        if (all.HasFlag(ColumnOption.DateTimeChars) && ":/\\.-T ".IndexOf(c) == -1)
          all &= ~ColumnOption.DateTimeChars;
        if (all.HasFlag(ColumnOption.Word) && (c < 32 || c > 125))
          all &= ~ColumnOption.Word;

        if (all == ColumnOption.None)
          return ColumnOption.None;
      }

      return all;
    }

    private static ColumnOption GetColumnOptionAllRows(int colNum, IEnumerable<string[]> rows)
    {
      var overall = ColumnOption.Empty;
      foreach (var row in rows)
      {
        var oneColOption = GetColumnOption(row[colNum]);
        if (oneColOption == ColumnOption.Empty)
          continue;

        if (overall == ColumnOption.Empty)
          overall = oneColOption;
        else
          overall = overall & oneColOption;
      }
      return overall;
    }
  }
}