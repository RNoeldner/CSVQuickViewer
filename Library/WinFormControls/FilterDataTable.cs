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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools;

/// <inheritdoc />
/// <summary>
///   Utility Class to filter a DataTable for Errors and Warnings.
///   Tracks column-level health to allow UI synchronization of column visibility.
/// </summary>
/// <seealso cref="T:System.IDisposable" />
public sealed class FilterDataTable : DisposableBase
{
  /// <summary>
  ///   Internal cache mapping each DataColumn to its health status.
  ///   True if the column is "Clean" (no errors/warnings encountered), otherwise false.
  /// </summary>
  private readonly Dictionary<DataColumn, bool> m_CacheColumns = new Dictionary<DataColumn, bool>();

  /// <summary>
  ///   Initializes a new instance of the <see cref="FilterDataTable" /> class.
  /// </summary>
  /// <param name="init">The initial DataTable to be monitored and filtered.</param>
  /// <exception cref="ArgumentNullException">Thrown when init is null.</exception>
  public FilterDataTable(DataTable init)
  {
    OriginalTable = init ?? throw new ArgumentNullException(nameof(init));
    foreach (DataColumn col in OriginalTable.Columns)
      m_CacheColumns.Add(col, true);
  }

  /// <summary>
  ///   Gets a value indicating whether the last filter operation was truncated
  ///   due to reaching the specified row limit.
  /// </summary>
  public bool CutAtLimit { get; private set; }

  /// <summary>
  ///   Gets the most original unfiltered DataTable.
  /// </summary>
  public DataTable OriginalTable { get; }

  /// <summary>
  ///   Gets the most recently generated filtered DataTable.
  ///   Note: Requires a call to <see cref="FilterAsync"/> to populate.
  /// </summary>
  /// <value>The currently filtered result table.</value>
  private DataTable? m_FilteredTable;

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
  public async Task<(DataTable DataTable, int NumWarning, int NumErrors, int NumWarningOrError)>
    FilterAsync(int limit, RowFilterTypeEnum newFilterType, CancellationToken token)
  {
    // 1. Guard against invalid limits and "All" shortcut
    int numWarnings = 0, numErrors = 0, numWarningOrError = 0, effectiveLimit = limit < 1 ? int.MaxValue : limit;
    foreach (DataColumn col in OriginalTable.Columns)
      m_CacheColumns[col] = true;

    if (newFilterType == RowFilterTypeEnum.All)
      (numWarnings, numErrors, numWarningOrError) = await ParseTableAsync(OriginalTable, (col) => m_CacheColumns[col] = false, null, token).ConfigureAwait(false);
    else
    {
      try
      {
        // Run the heavy loop on a background thread to avoid UI stutters
        // Use a list as a buffer. List<T> is faster to add to than a DataTable index.
        var buffer = new List<DataRow>();

        (numWarnings, numErrors, numWarningOrError) =await ParseTableAsync(OriginalTable,
          (col) => m_CacheColumns[col] = false,
          (rowIssues, row) =>
            {
              if ((rowIssues & newFilterType) != 0)
                buffer.Add(row);
              return (buffer.Count >= effectiveLimit);
            }, token).ConfigureAwait(false);

        // 4. Critical Section: Bulk Import
        var resultTable = OriginalTable.Clone();
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

        // 5. Atomic update of state
        m_FilteredTable?.Dispose();
        m_FilteredTable = resultTable;

        CutAtLimit = m_FilteredTable.Rows.Count >= effectiveLimit;

        return (m_FilteredTable, numWarnings, numErrors, numWarningOrError);
      }
      catch (OperationCanceledException)
      {
        // ignore
      }
      catch (Exception ex)
      {
        Logger.Warning($"FilterAsync {newFilterType}", ex);
      }
    }
    return (OriginalTable, numWarnings, numErrors, numWarningOrError);
  }

  /// <summary>
  ///   Retrieves a collection of column names filtered by their health status.
  /// </summary>
  /// <param name="filterType">
  ///   Determines the logic: <see cref="RowFilterTypeEnum.Clean"/> returns healthy columns,
  ///   <see cref="RowFilterTypeEnum.All"/> returns all, any other flag returns problematic columns.
  /// </param>  /// <returns>A unique collection of DataPropertyName/ColumnName strings.</returns>
  public IReadOnlyCollection<string> GetColumns(RowFilterTypeEnum filterType)
  => filterType switch
  {
    RowFilterTypeEnum.Clean =>
      m_CacheColumns.Where(x => x.Value).Select(x => x.Key.ColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase),
    RowFilterTypeEnum.All =>
      m_CacheColumns.Select(x => x.Key.ColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase),
    _ => // This covers Warning, Errors, and ErrorsAndWarning
      m_CacheColumns.Where(x => !x.Value).Select(x => x.Key.ColumnName).ToHashSet(StringComparer.OrdinalIgnoreCase),
  };

  /// <inheritdoc />
  protected override void Dispose(bool disposing)
  {
    if (disposing)
      m_FilteredTable?.Dispose();
  }

  /// <summary>
  ///   Asynchronously parses the table to calculate issue counts and identify problematic columns.
  /// </summary>
  /// <param name="table">The table to parse.</param>
  /// <param name="actionColumn">Callback invoked when a column is found to contain an issue.</param>
  /// <param name="actionRow">Callback invoked for each row, returning true to halt parsing (e.g., limit reached).</param>
  /// <param name="token">Propagates notification that operations should be canceled.</param>
  /// <returns>A tuple containing the total count of warnings, errors, and unique rows with issues.</returns>
  private static async Task<(int NumWarnings, int NumErrors, int NumWarningOrError)> ParseTableAsync(DataTable table,
          Action<DataColumn>? actionColumn,
    Func<RowFilterTypeEnum, DataRow, bool>? actionRow, CancellationToken token)
  {
    int numErrors = 0, numWarnings = 0, numWarningOrError = 0;
    await Task.Run(() =>
    {
      for (var i = 0; i < table.Rows.Count; i++)
      {
        // This is the standard way to stop the task and signal "Canceled"
        if (i % 100 == 0)
          token.ThrowIfCancellationRequested();

        var row = table.Rows[i];
        var rowHasWarning = false;
        var rowHasError = false;
        // Default to 'Clean' (Value 1)
        var rowIssues = RowFilterTypeEnum.Clean;

        // 1. Check Row-Level Errors
        if (row.RowError.Length != 0)
        {
          // Clear the 'None' bit because we found an issue
          rowIssues &= ~RowFilterTypeEnum.Clean;
          var isWarning = row.RowError.IsWarningMessage();
          rowHasWarning |= isWarning;
          rowHasError |= !isWarning;
          if (rowHasError)
            Debug.WriteLine("");
          // Set the bit for Warning or Error based on the message type
          rowIssues |= isWarning ? RowFilterTypeEnum.Warning : RowFilterTypeEnum.Errors;
        }

        // 2. Check Column-Level Errors
        var errorCols = row.GetColumnsInError();
        foreach (var col in errorCols)
        {
          var colError = row.GetColumnError(col);
          // Check if there is actually a message (not just an empty entry)
          if (!string.IsNullOrEmpty(colError))
          {
            actionColumn?.Invoke(col);
            rowIssues &= ~RowFilterTypeEnum.Clean; // Remove 'Clean' flag
            var isWarning = colError.IsWarningMessage();
            rowHasWarning |= isWarning;
            rowHasError |= !isWarning;
            if (rowHasError)
              Debug.WriteLine("");
            rowIssues |= isWarning ? RowFilterTypeEnum.Warning : RowFilterTypeEnum.Errors;
          }
        }
        if (rowHasWarning) numWarnings++;
        if (rowHasError) numErrors++;
        if (rowHasWarning || rowHasError) numWarningOrError++;

        if (actionRow?.Invoke(rowIssues, row) ?? false)
          break;
      }
    }, token).ConfigureAwait(true);

    return new(numWarnings, numErrors, numWarningOrError);
  }
}