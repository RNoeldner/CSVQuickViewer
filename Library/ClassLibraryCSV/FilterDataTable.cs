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

    private readonly Dictionary<DataColumn, FilterTypeEnum> m_Cache = new Dictionary<DataColumn, FilterTypeEnum>();

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
        m_Cache.Add(col, FilterTypeEnum.None);
    }

    public bool CutAtLimit { get; private set; }

    public bool Filtering => m_Filtering;

    /// <summary>
    ///   Gets the filtered table, need to Filter before 
    /// </summary>
    /// <value>The error table.</value>
    public DataTable? FilterTable { get; private set; }

    public FilterTypeEnum FilterType { get; private set; } = FilterTypeEnum.None;

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
    public IReadOnlyCollection<string> GetColumns(FilterTypeEnum type)
    {
      var result = new HashSet<string>(
          type switch
          {
            FilterTypeEnum.ErrorsAndWarning =>
              m_Cache.Where(x => x.Value.HasFlag(FilterTypeEnum.ShowErrors)  || x.Value.HasFlag(FilterTypeEnum.ShowWarning)).Select(x => x.Key.ColumnName).ToList(),
            FilterTypeEnum.All =>
              m_Cache.Select(x => x.Key.ColumnName).ToList(),
            FilterTypeEnum.None =>
              m_Cache.Select(x => x.Key.ColumnName).ToList(),
            FilterTypeEnum.ShowIssueFree =>
              m_Cache.Where(x => x.Value == FilterTypeEnum.None).Select(x => x.Key.ColumnName).ToList(),
            _ =>
              m_Cache.Where(x => x.Value.HasFlag(type)).Select(x => x.Key.ColumnName).ToList(),
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
      WaitCompeteFilter(0.2);

      m_CurrentFilterCancellationTokenSource.Dispose();
      m_CurrentFilterCancellationTokenSource = null;
    }

    public DataTable Filter(int limit, FilterTypeEnum type, CancellationToken cancellationToken)
    {
      if (limit < 1)
        limit = int.MaxValue;

      if (type == FilterTypeEnum.All || type == FilterTypeEnum.None && m_SourceTable.Rows.Count<=limit)
        return m_SourceTable;

      if (type == FilterType && FilterTable!=null && FilterTable.Rows.Count<=limit)
        return FilterTable;

      foreach (DataColumn col in m_SourceTable.Columns)
        m_Cache[col] = FilterTypeEnum.None;
      m_Filtering = true;

      FilterTable?.Dispose();
      FilterTable = m_SourceTable.Clone();
      try
      {
        FilterType = type;
        for (var counter = 0; counter < m_SourceTable.Rows.Count; counter++)
        {
          cancellationToken.ThrowIfCancellationRequested();
          var row = m_SourceTable.Rows[counter];
          if (type.HasFlag(FilterTypeEnum.OnlyTrueErrors) && row.RowError == "-")
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
            if (row.GetColumnError(col).IsWarningMessage())
            {
              m_Cache[col] |= FilterTypeEnum.ShowWarning;
              rowIssues |= FilterTypeEnum.ShowWarning;
            }
            else
            {
              m_Cache[col] |= FilterTypeEnum.ShowErrors;
              rowIssues |= FilterTypeEnum.ShowErrors;
            }
          }

          if (rowIssues.HasFlag(type) || (rowIssues == FilterTypeEnum.None && type == FilterTypeEnum.ShowIssueFree)
                                      || rowIssues == FilterTypeEnum.ShowErrors && type == FilterTypeEnum.ErrorsAndWarning
                                      || rowIssues == FilterTypeEnum.ShowWarning && type == FilterTypeEnum.ErrorsAndWarning)
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