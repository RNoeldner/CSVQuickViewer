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
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   Interface for a File Reader.
  /// </summary>
  public interface IFileReader : IDataReader
  {
    /// <summary>
    ///   Gets the end line number
    /// </summary>
    /// <value>The line number in which the record ended</value>
    long EndLineNumber { get; }

    /// <summary>
    ///   Determine if the data Reader is at the end of the file
    /// </summary>
    /// <returns>True if you can read; otherwise, false.</returns>
    bool EndOfFile { get; }

    /// <summary>
    ///   Gets the record number.
    /// </summary>
    /// <value>The record number.</value>
    long RecordNumber { get; }

    /// <summary>
    ///   Gets the start line number.
    /// </summary>
    /// <value>The line number in which the record started.</value>
    long StartLineNumber { get; }

    /// <summary>
    ///   Gets the file setting.
    /// </summary>
    /// <value>The file setting.</value>
    IFileSetting FileSetting { get; }

    /// <summary>
    ///   Gets the process display.
    /// </summary>
    /// <value>The process display.</value>
    IProcessDisplay ProcessDisplay { get; }

    bool SupportsReset { get; }

    [Obsolete("Use ReadAsync if possible")]
    new bool Read();

    /// <summary>
    ///   Advances the data reader to the next result, when reading the results of batch SQL statements.
    /// </summary>
    /// <returns>
    ///   <see langword="true" /> if there are more rows; otherwise, <see langword="false" />.
    /// </returns>
    [Obsolete("Not supported")]
    new bool NextResult();

    /// <summary>
    ///   Asynchronous method to copy rows from a the reader to a data table
    /// </summary>
    /// <param name="recordLimit">Number of maximum records, 0 for all existing </param>
    /// <param name="cancellationToken">Cancellation toke to stop filling the data table</param>
    /// <returns>A Data Table with teh data</returns>
    Task<DataTable> GetDataTableAsync(long recordLimit, CancellationToken cancellationToken);

    /// <summary>
    /// Create a data table for the reader excluding all ignored columns but adding artificial columns like Line or Rop
    /// </summary>
    /// <param name="dataTable"></param>
    /// <param name="includeErrorField"></param>
    /// <returns></returns>
    CopyToDataTableInfo GetCopyToDataTableInfo(DataTable dataTable, bool includeErrorField);

    /// <summary>
    ///   Copies a row from the reader to the data table, handling artificial fields and storing possible warnings
    /// </summary>
    /// <param name="dataTable"></param>
    /// <param name="columnWarningsReader"></param>
    /// <param name="dataTableInfo"></param>
    /// <param name="handleColumnIssues"></param>
    void CopyRowToTable(DataTable dataTable, ColumnErrorDictionary columnWarningsReader,
      CopyToDataTableInfo dataTableInfo, Action<ColumnErrorDictionary, DataRow> handleColumnIssues);

    /// <summary>
    ///   Determines if the reader has a certain columns, any ignored columns will be treated as not existing
    /// </summary>
    /// <param name="columnName"></param>
    /// <returns>true if present and not ignored</returns>
    bool HasColumnName(string columnName);

    /// <summary>
    ///   Reads the next record of the current result set
    /// </summary>
    /// <returns>Awaitable bool, if true a record was read</returns>
    Task<bool> ReadAsync();

    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    event EventHandler<WarningEventArgs> Warning;

    /// <summary>
    ///   Occurs before the initial open. Can be used to prepare teh data like download it from a
    ///   Remote location
    /// </summary>
    event EventHandler OnOpen;

    /// <summary>
    ///   Occurs when an open process failed, allowing the user to change the timeout or provide the
    ///   needed file etc.
    /// </summary>
    event EventHandler<RetryEventArgs> OnAskRetry;

    /// <summary>
    ///   Gets the column information for a given column number
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>A <see cref="Column" /> with all information on the column</returns>
    Column GetColumn(int column);

    /// <summary>
    ///   Checks if the column should be read
    /// </summary>
    /// <param name="column">The column number.</param>
    /// <returns><c>true</c> if this column should not be read</returns>
    bool IgnoreRead(int column);

    /// <summary>
    ///   Overrides the column format with values from settings
    /// </summary>
    void OverrideColumnFormatFromSetting();

    /// <summary>
    ///   Opens the text file and begins to read the meta data, like columns
    /// </summary>
    /// <returns>Number of records in the file if known (use determineColumnSize), -1 otherwise</returns>
    Task OpenAsync();

    /// <summary>
    ///   Resets the position and buffer to the header in case the file has a header
    /// </summary>
    Task ResetPositionToFirstDataRowAsync();
  }
}