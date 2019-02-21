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
using System.Diagnostics;
using System.Threading;

namespace CsvTools
{
  [Flags]
  public enum FilterType
  {
    ShowIssueFree = 1,
    ShowWarning = 2,
    ShowErrors = 4,
    ErrorsAndWarning = 2 + 4,
    All = 1 + 2 + 4,
    OnlyTrueErrors = 8
  }

  /// <summary>
  ///   Utility Class to filter a DataTable for Errors
  /// </summary>
  /// <seealso cref="System.IDisposable" />
  public class FilterDataTable : IDisposable
  {
    private readonly DataTable m_SourceTable;
    private readonly List<string> m_UniqueFieldName = new List<string>();
    private HashSet<string> m_ColumnWithoutErrors;
    private bool m_CutAtLimit;
    private DataTable m_FilteredTable;
    private volatile bool m_Filtering = false;
    private FilterType m_FilterType;
    private readonly CancellationToken m_CancellationToken;
    private CancellationTokenSource m_CurrentFilter;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FilterDataTable" /> class.
    /// </summary>
    /// <param name="init">The initial DataTable</param>
    /// <param name="limit">The limit.</param>
    public FilterDataTable(DataTable init, CancellationToken cancellationToken)
    {
      m_SourceTable = init;
      m_CancellationToken = cancellationToken;
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
        foreach (DataColumn col in m_FilteredTable.Columns)
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

    public void Cancel()
    {
      // stop old filtering
      m_CurrentFilter?.Cancel();
      // wait in order to start new one
      WaitForFilter();
    }

    private void WaitForFilter()
    {
      // wait for filtering to finish
      while (m_Filtering)
      {
        m_CancellationToken.ThrowIfCancellationRequested();
        Thread.Sleep(200);
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

          // wait for filtering to finish
          WaitForFilter();

          // m_ColumnWithoutErrors will not contain UniqueFields nor line number / error
          foreach (DataColumn col in m_FilteredTable.Columns)
          {
            m_CancellationToken.ThrowIfCancellationRequested();
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
            foreach (DataRow row in FilterTable.Rows)
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

    public bool CutAtLimit => m_CutAtLimit;

    /// <summary>
    ///   Gets the error table.
    /// </summary>
    /// <value>
    ///   The error table.
    /// </value>
    public DataTable FilterTable { get => m_FilteredTable; }

    public bool Filtering { get => m_Filtering; }

    public FilterType FilterType { get => m_FilterType; }

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
    public void Dispose()
    {
      Dispose(true);
    }

    public void Filter(int limit, FilterType type)
    {
      if (m_Filtering)
        Cancel();

      try
      {
        m_CurrentFilter = CancellationTokenSource.CreateLinkedTokenSource(m_CancellationToken);
        m_Filtering = true;
        m_CancellationToken.ThrowIfCancellationRequested();
        m_FilteredTable = m_SourceTable.Clone();
        m_FilterType = type;

        int rows = 0;
        var max = m_SourceTable.Rows.Count;

        for (int counter = 0; counter < max && rows < limit; counter++)
        {
          if (m_CurrentFilter.IsCancellationRequested)
            return;
          var ErrorOrWarning = m_SourceTable.Rows[counter].GetErrorInformation();

          if (type.HasFlag(FilterType.OnlyTrueErrors) && ErrorOrWarning == "-")
            continue;

          bool import = false;
          if (string.IsNullOrEmpty(ErrorOrWarning))
          {
            if (type.HasFlag(FilterType.ShowIssueFree))
              import = true;
          }
          else
          {
            if (!import && ErrorOrWarning.IsWarningMessage())
            {
              if (type.HasFlag(FilterType.ShowWarning))
                import = true;
            }
            else
            {
              // is an error
              if (type.HasFlag(FilterType.ShowErrors))
                import = true;
            }
          }
          if (import)
            m_FilteredTable.ImportRow(m_SourceTable.Rows[counter]);

          rows++;
        }
        m_CutAtLimit = (rows >= limit);
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.InnerExceptionMessages());
      }
      finally
      {
        m_Filtering = false;
      }
    }

    public void StartFilter(int limit, FilterType type, Action finishedAction)
    {
      ThreadPool.QueueUserWorkItem(delegate
        {
          Filter(limit, type);
          finishedAction?.Invoke();
        });
    }

    /// <summary>
    ///   Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!disposing) return;

      try
      {
        m_CurrentFilter?.Cancel();
        m_CurrentFilter?.Dispose();
      }
      catch
      {
        // ignore
      }

      FilterTable?.Dispose();
    }
  }
}