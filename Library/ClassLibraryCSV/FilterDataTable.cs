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
  public sealed class FilterDataTable : IDisposable
  {
    private readonly DataTable m_SourceTable;

    private readonly List<string> m_UniqueFieldName = new List<string>();

    private HashSet<string>? m_ColumnWithoutErrors;

    private CancellationTokenSource? m_CurrentFilterCancellationTokenSource;

    private bool m_DisposedValue; // To detect redundant calls

    private volatile bool m_Filtering;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FilterDataTable" /> class.
    /// </summary>
    /// <param name="init">The initial DataTable</param>
    public FilterDataTable(in DataTable? init)
    {
      m_SourceTable = init ?? throw new ArgumentNullException(nameof(init));
      FilterTable = m_SourceTable.Clone();
    }

    /// <summary>
    ///   Gets the columns without errors.
    /// </summary>
    /// <value>The columns without errors.</value>
    public ICollection<string> ColumnsWithErrors
    {
      get
      {
        var withoutErrors = ColumnsWithoutErrors;

        return (from DataColumn col in m_SourceTable.Columns
                where !col.ColumnName.Equals(ReaderConstants.cErrorField, StringComparison.OrdinalIgnoreCase)
                where !withoutErrors.Contains(col.ColumnName)
                select col.ColumnName).ToList();
      }
    }

    /// <summary>
    ///   Gets the columns without errors.
    /// </summary>
    /// <value>The columns without errors.</value>
    public ICollection<string> ColumnsWithoutErrors
    {
      get
      {
        if (m_ColumnWithoutErrors != null) return m_ColumnWithoutErrors;

        // Wait until we are actually done filtering, max 60 seconds
        WaitCompeteFilter(60);

        m_ColumnWithoutErrors = new HashSet<string>();

        // m_ColumnWithoutErrors will not contain UniqueFields nor line number / error
        Debug.Assert(FilterTable != null, nameof(FilterTable) + " != null");
        foreach (DataColumn col in FilterTable!.Columns)
        {
          // Always keep the line number, error field and any uniques
          if (col.ColumnName.Equals(ReaderConstants.cStartLineNumberFieldName, StringComparison.OrdinalIgnoreCase)
              || col.ColumnName.Equals(ReaderConstants.cErrorField, StringComparison.OrdinalIgnoreCase)
              || m_UniqueFieldName.Contains(col.ColumnName))
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
            if (!row.RowError.Contains(inRowErrorDesc0, StringComparison.OrdinalIgnoreCase)
                && !row.RowError.Contains(inRowErrorDesc1, StringComparison.OrdinalIgnoreCase)
                && !row.RowError.Contains(inRowErrorDesc2, StringComparison.OrdinalIgnoreCase)
                && !row.RowError.Contains(inRowErrorDesc3, StringComparison.OrdinalIgnoreCase))
              continue;

            hasErrors = true;
            break;
          }

          if (!hasErrors)
            m_ColumnWithoutErrors.Add(col.ColumnName);
        }

        return m_ColumnWithoutErrors;
      }
    }

    public bool CutAtLimit { get; private set; }

    public bool Filtering => m_Filtering;

    /// <summary>
    ///   Gets the error table.
    /// </summary>
    /// <value>The error table.</value>
    public DataTable FilterTable { get; private set; }

    public FilterType FilterType { get; private set; } = FilterType.None;

    /// <summary>
    ///   Sets the name of the unique field.
    /// </summary>
    /// <value>The name of the unique field.</value>
    /// <remarks>Setting the UniqueFieldName will update ColumnWithoutErrors</remarks>
    public IEnumerable<string> UniqueFieldName
    {
      set
      {
        m_UniqueFieldName.Clear();
        if (value.Any())
          m_UniqueFieldName.AddRange(value);
      }
    }

    public void Cancel()
    {
      // stop old filtering
      if (m_CurrentFilterCancellationTokenSource?.IsCancellationRequested ?? true) return;

      m_CurrentFilterCancellationTokenSource.Cancel();

      // make sure the filtering is canceled
      WaitCompeteFilter(0.2);

      m_CurrentFilterCancellationTokenSource.Dispose();
      m_CurrentFilterCancellationTokenSource = null;
    }

    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting
    ///   unmanaged resources.
    /// </summary>
    public void Dispose() => Dispose(true);

    public void Filter(int limit, FilterType type)
    {
      if (limit < 1)
        limit = int.MaxValue;
      m_ColumnWithoutErrors = null;
      m_Filtering = true;
      try
      {
        var rows = 0;
        var max = m_SourceTable.Rows.Count;
        FilterType = type;
        for (var counter = 0; counter < max && rows < limit; counter++)
        {
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
            if (errorOrWarning.IsWarningMessage())
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

        CutAtLimit = rows >= limit;
      }
      catch (Exception ex)
      {
        Logger.Warning(ex.SourceExceptionMessage());
      }
      finally
      {
        m_Filtering = false;
      }
    }

    public async Task FilterAsync(int limit, FilterType type, CancellationToken cancellationToken)
    {
      if (m_Filtering)
        Cancel();

      m_ColumnWithoutErrors = null;
      FilterTable = m_SourceTable.Clone();

      m_CurrentFilterCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

      await Task.Run(() => Filter(limit, type), m_CurrentFilterCancellationTokenSource.Token).ConfigureAwait(false);
    }

    public void WaitCompeteFilter(double timeoutInSeconds)
    {
      if (!m_Filtering) return;
      var stopwatch = timeoutInSeconds > 0.01 ? new Stopwatch() : null;
      stopwatch?.Start();
      while (m_Filtering)
      {
        FunctionalDI.SignalBackground.Invoke();
        if (!(stopwatch?.Elapsed.TotalSeconds > timeoutInSeconds)) continue;
        // can not call Cancel as this method is called by cancel
        m_CurrentFilterCancellationTokenSource?.Cancel();
        break;
      }
    }

    private void Dispose(bool disposing)
    {
      Cancel();
      if (m_DisposedValue)
        return;
      if (!disposing) return;
      m_DisposedValue = true;
      m_CurrentFilterCancellationTokenSource?.Dispose();
      FilterTable.Dispose();
    }
  }
}