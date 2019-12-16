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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   Utility Class to filter a DataTable for Errors
  /// </summary>
  /// <seealso cref="System.IDisposable" />
  public class FilterDataTable : IDisposable
  {
    private readonly DataTable m_SourceTable;
    private readonly List<string> m_UniqueFieldName = new List<string>();
    private HashSet<string> m_ColumnWithoutErrors;
    private volatile bool m_Filtering;
    private readonly CancellationToken m_CancellationToken;
    private CancellationTokenSource m_CurrentFilterCancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterDataTable" /> class.
    /// </summary>
    /// <param name="init">The initial DataTable</param>
    /// <param name="cancellationToken">The cancellation token.</param>
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
        var columnWithErrors = new List<string>();
        var withoutErrors = ColumnsWithoutErrors;
        foreach (DataColumn col in FilterTable.Columns)
        {
          // Always ignore line number and ErrorField
          if (col.ColumnName.Equals(BaseFileReader.cErrorField, StringComparison.OrdinalIgnoreCase))
            continue;

          if (!withoutErrors.Contains(col.ColumnName))
            columnWithErrors.Add(col.ColumnName);
        }

        return columnWithErrors;
      }
    }

    public void Cancel()
    {
      // stop old filtering
      if (m_CurrentFilterCancellationTokenSource != null && !m_CurrentFilterCancellationTokenSource.IsCancellationRequested)
        m_CurrentFilterCancellationTokenSource?.Cancel();
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
          foreach (DataColumn col in FilterTable.Columns)
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

              if (string.IsNullOrEmpty(row.RowError))
                continue;
              if (!row.RowError.Contains(inRowErrorDesc0, StringComparison.OrdinalIgnoreCase) && !row.RowError.Contains(inRowErrorDesc1, StringComparison.OrdinalIgnoreCase) &&
                  !row.RowError.Contains(inRowErrorDesc2, StringComparison.OrdinalIgnoreCase) && !row.RowError.Contains(inRowErrorDesc3, StringComparison.OrdinalIgnoreCase))
                continue;

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

    public bool CutAtLimit { get; private set; }

    /// <summary>
    ///   Gets the error table.
    /// </summary>
    /// <value>
    ///   The error table.
    /// </value>
    public DataTable FilterTable { get; private set; }

    public bool Filtering => m_Filtering;

    public FilterType FilterType { get; private set; }

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
        if (value != null && value.Any())
          m_UniqueFieldName.AddRange(value);
        m_ColumnWithoutErrors = null;
      }
    }

    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() => Dispose(true);

    public void Filter(int limit, FilterType type)
    {
      if (m_Filtering)
        Cancel();

      try
      {
        m_CurrentFilterCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(m_CancellationToken);
        m_Filtering = true;
        m_CancellationToken.ThrowIfCancellationRequested();
        FilterTable = m_SourceTable.Clone();
        FilterType = type;

        var rows = 0;
        var max = m_SourceTable.Rows.Count;

        for (var counter = 0; counter < max && rows < limit; counter++)
        {
          if (m_CurrentFilterCancellationTokenSource.IsCancellationRequested)
            return;
          var errorOrWarning = m_SourceTable.Rows[counter].GetErrorInformation();

          if (type.HasFlag(FilterType.OnlyTrueErrors) && errorOrWarning == "-")
            continue;

          var import = false;
          if (string.IsNullOrEmpty(errorOrWarning))
          {
            if (type.HasFlag(FilterType.ShowIssueFree))
              import = true;
          }
          else
          {
            if (!import && errorOrWarning.IsWarningMessage())
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
            FilterTable.ImportRow(m_SourceTable.Rows[counter]);

          rows++;
        }
        CutAtLimit = (rows >= limit);
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

    public void StartFilter(int limit, FilterType type, Action finishedActionIfResults) => Task.Run(() => Filter(limit, type)).ContinueWith((task =>
                                                                                           {
                                                                                             if (FilterTable.Rows.Count == 0)
                                                                                               finishedActionIfResults.Invoke();
                                                                                           }));

    private bool m_DisposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      Cancel();
      if (m_DisposedValue)
        return;
      if (disposing)
      {
        if (m_CurrentFilterCancellationTokenSource != null)
          m_CurrentFilterCancellationTokenSource.Dispose();
        if (FilterTable != null)
          FilterTable.Dispose();
      }

      m_DisposedValue = true;
    }
  }
}