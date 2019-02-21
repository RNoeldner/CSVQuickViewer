using System;
using System.Collections.Generic;

namespace CsvTools
{
  public class ReAlignColumns
  {
    private const int MaxGoodRows = 30;
    private readonly ICsvFile m_CsvFile;
    private readonly int m_ExcpectedColumns;
    private readonly List<string[]> m_GoodRows = new List<string[]>();

    public ReAlignColumns(ICsvFile csvFile, int excpectedColumns)
    {
      m_CsvFile = csvFile;
      m_ExcpectedColumns = excpectedColumns;
    }

    [Flags]
    public enum ColumnOption
    {
      None = 0,
      Empty = 1,
#pragma warning disable CA1720 // Identifier contains type name
      Integer = 2,         // Only 0-9
#pragma warning restore CA1720 // Identifier contains type name
      DecimalChars = 4,    // Only . ,  + -
      DateTimeChars = 8,   // Only / \ - . : T (space)
      Word = 16,           // 0-9 A-Z _ - (noSpace)
      ShortText = 32,      // Length < 40
      VeryShortText = 64,  // Length < 10
    }

    public void AddRow(string[] newRow)
    {
      if (m_GoodRows.Count < MaxGoodRows)
        m_GoodRows.Add(newRow);
      else
        // Store the row in our list
        m_GoodRows[SecureString.Random.Next(0, MaxGoodRows)] = newRow;
    }

    public string[] RealignColumn(string[] row, Action<int, string> handleWarning)
    {
      var columns = new List<string>(row);

      //Get the Options for all good rows
      List<ColumnOption> otherColumns = new List<ColumnOption>(m_ExcpectedColumns);
      for (int col2 = 0; col2 < m_ExcpectedColumns; col2++)
        otherColumns.Add(GetColumnOptionAllRows(col2, m_GoodRows));

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
      return columns.ToArray();
    }

    private static ColumnOption GetColumnOption(string text)
    {
      if (string.IsNullOrEmpty(text))
        return ColumnOption.Empty;
      ColumnOption all = ColumnOption.Integer | ColumnOption.DecimalChars | ColumnOption.DateTimeChars | ColumnOption.Word;
      if (text.Length <= 40)
        all |= ColumnOption.ShortText;
      if (text.Length <= 10)
        all |= ColumnOption.VeryShortText;
      foreach (var c in text)
      {
        if (all.HasFlag(ColumnOption.Integer) && "0123456789".IndexOf(c) == -1)
          all &= ~ColumnOption.Integer;
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