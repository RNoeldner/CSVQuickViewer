/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools;

/// <inheritdoc />
/// <summary>
///   Utility Class to filter a DataTable for Errors
/// </summary>
/// <seealso cref="T:System.IDisposable" />
public sealed class FilterDataTable : DisposableBase
{
  private readonly Dictionary<DataColumn, bool> m_CacheColumns = new Dictionary<DataColumn, bool>();
  private readonly DataTable m_SourceTable;

  /// <summary>
  ///   Initializes a new instance of the <see cref="FilterDataTable" /> class.
  /// </summary>
  /// <param name="init">The initial DataTable</param>
  public FilterDataTable(DataTable init)
  {
    m_SourceTable = init ?? throw new ArgumentNullException(nameof(init));
    foreach (DataColumn col in m_SourceTable.Columns)
      m_CacheColumns.Add(col, true);
  }

  /// <summary>
  /// Set to true if we reached the set limit (see Filter)
  /// </summary>
  public bool CutAtLimit { get; private set; }

  /// <summary>
  ///   Gets the filtered table, need to Filter before 
  /// </summary>
  /// <value>The error table.</value>
  public DataTable? FilterTable { get; private set; }

  /// <summary>
  /// The type of Filter
  /// </summary>
  private RowFilterTypeEnum FilterType { get; set; } = RowFilterTypeEnum.All;

  /// <summary>
  /// Asynchronously filters the source data based on the specified error/warning criteria.
  /// </summary>
  /// <param name="limit">The maximum number of rows to include in the filtered result. Use 0 or less for no limit.</param>
  /// <param name="newFilterType">The <see cref="RowFilterTypeEnum"/> criteria used to identify matching rows.</param>
  /// <param name="token">A <see cref="CancellationToken"/> used to abort the filtering process (e.g., if the control is disposed or a new filter is requested).</param>
  /// <returns>
  /// A <see cref="Task{DataTable}"/> containing the filtered rows. 
  /// Returns the original source table if <paramref name="newFilterType"/> is <see cref="RowFilterTypeEnum.All"/>.
  /// </returns>
  /// <remarks>
  /// This method is optimized for large datasets through several key strategies:
  /// <list type="bullet">
  ///   <item>
  ///     <description><b>Off-thread Processing:</b> The heavy row-validation logic runs on a background thread to keep the UI responsive.</description>
  ///   </item>
  ///   <item>
  ///     <description><b>Buffering:</b> Matches are collected in a local list first to minimize <see cref="DataTable"/> index recalculation overhead.</description>
  ///   </item>
  ///   <item>
  ///     <description><b>Atomic Swapping:</b> Resulting data and metadata (column issue cache) are updated only upon successful completion, preventing partial data states in the UI.</description>
  ///   </item>
  ///   <item>
  ///     <description><b>Bulk Loading:</b> Uses <see cref="DataTable.BeginLoadData"/> during the final import to maximize throughput.</description>
  ///   </item>
  /// </list>
  /// </remarks>
  /// <exception cref="OperationCanceledException">Thrown if the <paramref name="token"/> is signaled during processing.</exception>
  public async Task<DataTable> FilterAsync(int limit, RowFilterTypeEnum newFilterType, CancellationToken token)
  {
    // 1. Guard against invalid limits and "All" shortcut
    int effectiveLimit = limit < 1 ? int.MaxValue : limit;

    if (newFilterType == RowFilterTypeEnum.All)
      return m_SourceTable;

    if (newFilterType == FilterType && FilterTable != null && FilterTable.Rows.Count <= limit)
      return FilterTable;

    foreach (DataColumn col in m_SourceTable.Columns)
      m_CacheColumns[col] = true;

    try
    {
      // 3. Prepare fresh metadata tracking
      var localCache = m_SourceTable.Columns.Cast<DataColumn>().ToDictionary(c => c, c => true);
      var resultTable = m_SourceTable.Clone();
      FilterType = newFilterType;
      // Run the heavy loop on a background thread to avoid UI stutters
      await Task.Run(() =>
      {
        // Use a list as a buffer. List<T> is faster to add to than a DataTable index.
        var buffer = new List<DataRow>();

        for (var i = 0; i < m_SourceTable.Rows.Count; i++)
        {
          token.ThrowIfCancellationRequested();
          var row = m_SourceTable.Rows[i];

          // Optimization: Pre-check "OnlyTrueErrors"
          if (newFilterType.HasFlag(RowFilterTypeEnum.OnlyTrueErrors) && row.RowError == "-")
            continue;

          var rowIssues = RowFilterTypeEnum.None;

          // Check Row Errors
          if (row.RowError.Length != 0)
          {
            rowIssues |= row.RowError.IsWarningMessage() ? RowFilterTypeEnum.ShowWarning : RowFilterTypeEnum.ShowErrors;
          }

          // Check Column Errors
          var errorCols = row.GetColumnsInError();
          foreach (var col in errorCols)
          {
            localCache[col] = false;
            rowIssues |= row.GetColumnError(col).IsWarningMessage() ? RowFilterTypeEnum.ShowWarning : RowFilterTypeEnum.ShowErrors;
          }

          // Match Logic
          bool isMatch = (rowIssues == RowFilterTypeEnum.None && newFilterType == RowFilterTypeEnum.None) ||
                         (rowIssues.HasFlag(RowFilterTypeEnum.ShowWarning) && newFilterType.HasFlag(RowFilterTypeEnum.ShowWarning)) ||
                         (rowIssues.HasFlag(RowFilterTypeEnum.ShowErrors) && newFilterType.HasFlag(RowFilterTypeEnum.ShowErrors));

          if (isMatch)
          {
            buffer.Add(row);
            if (buffer.Count >= effectiveLimit) break;
          }
        }

        // 4. Critical Section: Bulk Import
        resultTable.BeginLoadData();
        try
        {
          foreach (var match in buffer)
            resultTable.ImportRow(match);
        }
        finally
        {
          resultTable.EndLoadData();
        }
      }, token);

      // 5. Update state only after successful completion
      FilterType = newFilterType;
      foreach (var kvp in localCache) m_CacheColumns[kvp.Key] = kvp.Value;

      FilterTable?.Dispose();
      FilterTable = resultTable;
      CutAtLimit = FilterTable.Rows.Count >= effectiveLimit;
    }
    catch (OperationCanceledException)
    {
    }
    catch (Exception ex)
    {
      Logger.Warning($"FilterAsync {newFilterType}", ex);
    }

    // Fallback: return existing table or an empty clone of the source schema
    return FilterTable ?? m_SourceTable.Clone();
  }

  /// <summary>
  /// Returns a collection of column names based on the results of the last filter operation.
  /// </summary>
  /// <param name="filterType">
  /// The filter category to retrieve: 
  /// <see cref="RowFilterTypeEnum.None"/> for columns without issues, 
  /// <see cref="RowFilterTypeEnum.All"/> for all columns, 
  /// or others for columns containing errors/warnings.
  /// </param>
  /// <returns>A read-only collection of unique column names.</returns>
  public IReadOnlyCollection<string> GetColumns(RowFilterTypeEnum filterType)
  => filterType switch
  {
    RowFilterTypeEnum.None =>
      m_CacheColumns.Where(x => x.Value).Select(x => x.Key.ColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase),
    RowFilterTypeEnum.All =>
      m_CacheColumns.Select(x => x.Key.ColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase),
    _ =>
      m_CacheColumns.Where(x => !x.Value).Select(x => x.Key.ColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase),
  };

  /// <inheritdoc />
  protected override void Dispose(bool disposing)
  {
    if (disposing)
      FilterTable?.Dispose();
  }
}