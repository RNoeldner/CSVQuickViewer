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
using System.Diagnostics.Contracts;

namespace CsvTools
{
  /// <summary>
  ///   Utility Class to filter a DataTable for Errors
  /// </summary>
  /// <seealso cref="System.IDisposable" />
  public class FilterDataTable : IDisposable
  {
    private readonly DataTable m_DataTable;

    private readonly DataRow[] m_ErrorRows;

    private ICollection<string> m_ColumnWithErrors;

    private ICollection<string> m_ColumnWithoutErrors;

    private ICollection<string> m_UniqueFieldName = new List<string>();

    /// <summary>
    ///   Initializes a new instance of the <see cref="FilterDataTable" /> class.
    /// </summary>
    /// <param name="init">The initial DataTable</param>
    /// <param name="limit">The limit.</param>
    public FilterDataTable(DataTable init, int limit)
    {
      m_DataTable = init;

      m_ErrorRows = m_DataTable.GetErrors();
      ErrorTable = m_DataTable.Clone();
      var counter = 0;
      CutAtLimit = false;
      foreach (var r in m_ErrorRows)
      {
        if (counter++ > limit)
        {
          CutAtLimit = true;
          break;
        }

        ErrorTable.ImportRow(r);
      }
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
        if (m_ColumnWithErrors == null)
          SetColumnLists();

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
          SetColumnLists();
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
    public DataTable ErrorTable { get; }

    /// <summary>
    ///   Sets the name of the unique field.
    /// </summary>
    /// <value>The name of the unique field.</value>
    public virtual IEnumerable<string> UniqueFieldName
    {
      set
      {
        m_UniqueFieldName.Clear();
        if (!value.IsEmpty())
          m_UniqueFieldName = new List<string>(value);
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
      m_DataTable?.Dispose();

      ErrorTable?.Dispose();
    }

    /// <summary>
    ///   Sets the column lists <see cref="m_ColumnWithoutErrors" /> and <see cref="m_ColumnWithErrors" />
    /// </summary>
    private void SetColumnLists()
    {
      m_ColumnWithoutErrors = new List<string>();

      // m_ColumnWithoutErrors will not contain UniqueFields nor line number / error
      foreach (DataColumn col in m_DataTable.Columns)
      {
        // Always keep the line number
        if (col.ColumnName.Equals(BaseFileReader.cStartLineNumberFieldName, StringComparison.OrdinalIgnoreCase) ||
            col.ColumnName.Equals(BaseFileReader.cErrorField, StringComparison.OrdinalIgnoreCase))
          continue;

        // If its a unique ID keep it as well
        if (m_UniqueFieldName.Contains(col.ColumnName))
          continue;

        // Check if there are errors in this column
        var hasErrors = false;
        var inRowErrorDesc0 = "[" + col.ColumnName + "]";
        var inRowErrorDesc1 = "[" + col.ColumnName + ",";
        var inRowErrorDesc2 = "," + col.ColumnName + "]";
        var inRowErrorDesc3 = "," + col.ColumnName + ",";
        foreach (var row in m_ErrorRows)
        {
          Contract.Assume(row != null);
          // In case there is a column error..
          if (row.GetColumnError(col).Length > 0)
          {
            hasErrors = true;
            break;
          }

          if (string.IsNullOrEmpty(row.RowError)) continue;
          if (!row.RowError.Contains(inRowErrorDesc0) && !row.RowError.Contains(inRowErrorDesc1) &&
              !row.RowError.Contains(inRowErrorDesc2) && !row.RowError.Contains(inRowErrorDesc3)) continue;
          hasErrors = true;
          break;
        }

        if (!hasErrors)
          m_ColumnWithoutErrors.Add(col.ColumnName);
      }

      m_ColumnWithErrors = new List<string>();
      foreach (DataColumn col in m_DataTable.Columns)
      {
        // Always ignore line number and ErrorField
        if (col.ColumnName.Equals(BaseFileReader.cErrorField, StringComparison.OrdinalIgnoreCase))
          continue;

        if (!m_ColumnWithoutErrors.Contains(col.ColumnName))
          m_ColumnWithErrors.Add(col.ColumnName);
      }
    }
  }
}