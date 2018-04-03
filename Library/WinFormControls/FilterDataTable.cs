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
using System.Data;

namespace CsvTools
{
  /// <summary>
  ///   Utility Class to filter a DataTable for Errors
  /// </summary>
  /// <seealso cref="System.IDisposable" />
  public class FilterDataTable : IDisposable
  {
    private readonly DataTable m_ErrorTable;
    private readonly List<string> m_UniqueFieldName = new List<string>();
    private HashSet<string> m_ColumnWithoutErrors;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FilterDataTable" /> class.
    /// </summary>
    /// <param name="init">The initial DataTable</param>
    /// <param name="limit">The limit.</param>
    public FilterDataTable(DataTable init, int limit)
    {
      m_ErrorTable = init.Clone();
      var errorRows = init.GetErrors();
      var max = errorRows.GetLength(0);
      CutAtLimit = (max > limit);
      if (CutAtLimit)
        max = limit;

      for (int counter = 0; counter < max; counter++)
        m_ErrorTable.ImportRow(errorRows[counter]);
    }

    /// <summary>
    ///   Gets the columns without errors.
    /// </summary>
    /// <value>
    ///   The columns without errors.
    /// </value>
    public virtual ICollection<string> ColumnsWithErrors
    {
      get
      {
        var m_ColumnWithErrors = new List<string>();
        var withoutErrors = ColumnsWithoutErrors;
        foreach (DataColumn col in m_ErrorTable.Columns)
        {
          // Always ignore line number and ErrorField
          if (col.ColumnName.Equals(BaseFileReader.cErrorField, StringComparison.OrdinalIgnoreCase))
            continue;

          if (!withoutErrors.Contains(col.ColumnName))
            m_ColumnWithErrors.Add(col.ColumnName);
        }

        return m_ColumnWithErrors;
      }
    }

    /// <summary>
    ///   Gets the columns without errors.
    /// </summary>
    /// <value>
    ///   The columns without errors.
    /// </value>
    public virtual ICollection<string> ColumnsWithoutErrors
    {
      get
      {
        if (m_ColumnWithoutErrors == null)
        {
          m_ColumnWithoutErrors = new HashSet<string>();

          // m_ColumnWithoutErrors will not contain UniqueFields nor line number / error
          foreach (DataColumn col in m_ErrorTable.Columns)
          {
            // Always keep the line number, error field and any uniques
            if (col.ColumnName.Equals(BaseFileReader.cStartLineNumberFieldName, StringComparison.OrdinalIgnoreCase) ||
                col.ColumnName.Equals(BaseFileReader.cErrorField, StringComparison.OrdinalIgnoreCase) ||
                m_UniqueFieldName.Contains(col.ColumnName))
              continue;

            // Check if there are errors in this column
            var hasErrors = false;
            var inRowErrorDesc0 = "[" + col.ColumnName + "]";
            var inRowErrorDesc1 = "[" + col.ColumnName + ",";
            var inRowErrorDesc2 = "," + col.ColumnName + "]";
            var inRowErrorDesc3 = "," + col.ColumnName + ",";
            foreach (DataRow row in ErrorTable.Rows)
            {
              // In case there is a column error..
              if (row.GetColumnError(col).Length > 0)
              {
                hasErrors = true;
                break;
              }

              if (string.IsNullOrEmpty(row.RowError)) continue;
              if (!row.RowError.Contains(inRowErrorDesc0, StringComparison.OrdinalIgnoreCase) && !row.RowError.Contains(inRowErrorDesc1, StringComparison.OrdinalIgnoreCase) &&
                  !row.RowError.Contains(inRowErrorDesc2, StringComparison.OrdinalIgnoreCase) && !row.RowError.Contains(inRowErrorDesc3, StringComparison.OrdinalIgnoreCase)) continue;

              hasErrors = true;
              break;
            }

            if (!hasErrors)
              m_ColumnWithoutErrors.Add(col.ColumnName);
          }
        }
        return m_ColumnWithoutErrors;
      }
    }

    public bool CutAtLimit { get; }

    /// <summary>
    ///   Gets the error table.
    /// </summary>
    /// <value>
    ///   The error table.
    /// </value>
    public DataTable ErrorTable { get => m_ErrorTable; }

    /// <summary>
    ///   Sets the name of the unique field.
    /// </summary>
    /// <value>The name of the unique field.</value>
    /// <remarks>Setting the UniqueFieldName will update ColumnWithoutErrors</remarks>
    public virtual IEnumerable<string> UniqueFieldName
    {
      set
      {
        m_UniqueFieldName.Clear();
        if (!value.IsEmpty())
          m_UniqueFieldName.AddRange(value);
        m_ColumnWithoutErrors = null;
      }
    }

    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public virtual void Dispose()
    {
      Dispose(true);
    }

    /// <summary>
    ///   Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    private void Dispose(bool disposing)
    {
      if (!disposing) return;
      ErrorTable?.Dispose();
    }
  }
}