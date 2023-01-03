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

#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   Utility Class to filter a DataTable for Errors
  /// </summary>
  /// <seealso cref="T:System.IDisposable" />
  public sealed class FilterDataTable : DisposableBase
  {
    private readonly DataTable m_SourceTable;

    private readonly List<string> m_UniqueFieldName = new List<string>();

    private readonly Dictionary<DataColumn, bool> m_CacheColumns = new Dictionary<DataColumn, bool>();

    private CancellationTokenSource? m_CurrentFilterCancellationTokenSource;

    private volatile bool m_Filtering;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FilterDataTable" /> class.
    /// </summary>
    /// <param name="init">The initial DataTable</param>
    public FilterDataTable(in DataTable init)
    {
      m_SourceTable = init ?? throw new ArgumentNullException(nameof(init));
      foreach (DataColumn col in m_SourceTable.Columns)
        m_CacheColumns.Add(col, true);
    }

    public bool CutAtLimit { get; private set; }

    public bool Filtering => m_Filtering;

    /// <summary>
    ///   Gets the filtered table, need to Filter before 
    /// </summary>
    /// <value>The error table.</value>
    public DataTable? FilterTable { get; private set; }

    public FilterTypeEnum FilterType { get; private set; } = FilterTypeEnum.All;

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

    /// <summary>
    ///   Gets the columns that do match the filter.
    /// </summary>
    public IReadOnlyCollection<string> GetColumns(FilterTypeEnum filterType)
    {
      var result = new HashSet<string>(
        filterType switch
          {
            FilterTypeEnum.None =>
              m_CacheColumns.Where(x => x.Value).Select(x => x.Key.ColumnName).ToList(),
            FilterTypeEnum.All =>
              m_CacheColumns.Select(x => x.Key.ColumnName).ToList(),
            _ =>
              m_CacheColumns.Where(x => !x.Value).Select(x => x.Key.ColumnName).ToList(),
          });

      foreach (var fld in m_UniqueFieldName)
        result.Add(fld);
      return result;
    }

    public void Cancel()
    {
      // stop old filtering
      if (m_CurrentFilterCancellationTokenSource?.IsCancellationRequested ?? true) return;

      m_CurrentFilterCancellationTokenSource.Cancel();

      // make sure the filtering is canceled
      WaitCompeteFilter(0.1);

      m_CurrentFilterCancellationTokenSource.Dispose();
      m_CurrentFilterCancellationTokenSource = null;
    }

    public DataTable Filter(int limit, FilterTypeEnum newFilterType, in CancellationToken cancellationToken)
    {
      if (limit < 1)
        limit = int.MaxValue;

      if (newFilterType == FilterTypeEnum.All)
        return m_SourceTable;

      if (newFilterType == FilterType && FilterTable !=null && FilterTable.Rows.Count<=limit)
        return FilterTable;

      foreach (DataColumn col in m_SourceTable.Columns)
        m_CacheColumns[col] = true;

      m_Filtering = true;

      FilterTable?.Dispose();
      FilterTable = m_SourceTable.Clone();
      try
      {
        FilterType = newFilterType;
        for (var counter = 0; counter < m_SourceTable.Rows.Count; counter++)
        {
          cancellationToken.ThrowIfCancellationRequested();
          var row = m_SourceTable.Rows[counter];
          if (newFilterType.HasFlag(FilterTypeEnum.OnlyTrueErrors) && row.RowError == "-")
            continue;

          var rowIssues = FilterTypeEnum.None;
          if (row.RowError.Length != 0)
          {
            if (row.RowError.IsWarningMessage())
              rowIssues |= FilterTypeEnum.ShowWarning;
            else
              rowIssues |= FilterTypeEnum.ShowErrors;
          }

          foreach (var col in row.GetColumnsInError())
          {
            m_CacheColumns[col] = false;
            if (row.GetColumnError(col).IsWarningMessage())
              rowIssues |= FilterTypeEnum.ShowWarning;
            else
              rowIssues |= FilterTypeEnum.ShowErrors;
          }
          
          if ((rowIssues == FilterTypeEnum.None  && newFilterType ==FilterTypeEnum.None) ||
              (rowIssues.HasFlag(FilterTypeEnum.ShowWarning) && newFilterType.HasFlag(FilterTypeEnum.ShowWarning)) ||
              (rowIssues.HasFlag(FilterTypeEnum.ShowErrors) && newFilterType.HasFlag(FilterTypeEnum.ShowErrors))) 
          {
            // Import Row copies the data and the errors information
            FilterTable.ImportRow(row);
            if (FilterTable.Rows.Count >= limit)
              break;
          }
        }

        CutAtLimit = FilterTable.Rows.Count >= limit;
      }
      catch (Exception ex)
      {
        Logger.Warning(ex.SourceExceptionMessage());
      }
      finally
      {
        m_Filtering = false;
      }

      return FilterTable;
    }

    public Task StartFilterAsync(int limit, FilterTypeEnum type, CancellationToken cancellationToken)
    {
      if (m_Filtering)
        Cancel();

      m_CurrentFilterCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

      // ReSharper disable once MethodSupportsCancellation
      return Task.Run(() => Filter(limit, type, m_CurrentFilterCancellationTokenSource.Token));
    }

    private void WaitCompeteFilter(double timeoutInSeconds)
    {
      if (!m_Filtering) return;
      var stopwatch = timeoutInSeconds > 0.01 ? new Stopwatch() : null;
      stopwatch?.Start();
      while (m_Filtering)
      {
        Thread.Sleep(125);
        if (!(stopwatch?.Elapsed.TotalSeconds > timeoutInSeconds)) continue;
        // can not call Cancel as this method is called by timeout
        m_CurrentFilterCancellationTokenSource?.Cancel();
        break;
      }
    }

    protected override void Dispose(bool disposing)
    {
      Cancel();
      if (!disposing) return;
      m_CurrentFilterCancellationTokenSource?.Dispose();
      FilterTable?.Dispose();
    }
  }
}