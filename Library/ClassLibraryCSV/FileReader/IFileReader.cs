﻿/*
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
  /// <inheritdoc cref="System.Data.IDataReader" />
  /// <summary>
  ///   Interface for a File Reader.
  /// </summary>
  public interface IFileReader : IDataReader
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
                                 , IAsyncDisposable
#endif
  {
    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    public event EventHandler<WarningEventArgs>? Warning;

    /// <summary>
    ///   Gets the end line number, if reading form text file, otherwise <see cref="RecordNumber" />
    /// </summary>
    /// <value>The line number in which the record ended</value>
    long EndLineNumber { get; }

    /// <summary>
    ///   Determine if the data Reader is at the end of the file
    /// </summary>
    /// <returns>True if you can read; otherwise, false.</returns>
    bool EndOfFile { get; }

    /// <summary>
    ///   Occurs before the initial open. Can be used to prepare the data like download it from a
    ///   remote location
    /// </summary>
    void SetOnOpen(Func<Task> value);

    /// <summary>
    ///   Value between 0 and 100 to show the progress of the reader, not all readers do support
    ///   this, readers based on streams usually return the relative position in that stream.
    /// </summary>
    int Percent { get; }

    /// <summary>
    ///   Gets the record number of a the records that just had been read (1 after the first read)
    /// </summary>
    /// <value>The record number.</value>
    long RecordNumber { get; }

    /// <summary>
    ///   Gets the start line number, if reading form text file, otherwise <see cref="RecordNumber" />
    /// </summary>
    /// <value>The line number in which the record started.</value>
    long StartLineNumber { get; }

    /// <summary>
    ///   <c>True</c> if the underlying steam can be reset to start from the beginning without
    ///   re-opening the reader
    /// </summary>
    bool SupportsReset { get; }

    /// <summary>
    ///   Gets the column information for a given column number
    /// </summary>
    /// <param name="column">The column number</param>
    /// <returns>A <see cref="IColumn" /> with all information on the column</returns>
    IColumn GetColumn(int column);

    /// <summary>
    ///   Advances the data reader to the next result, when reading the results of batch SQL statements.
    /// </summary>
    /// <returns>
    ///   <see langword="true" /> if there are more results; otherwise, <see langword="false" />.
    /// </returns>
    [Obsolete("Not supported")]
    new bool NextResult();

    /// <summary>
    ///   Opens the text file and begins to read the meta data, like columns
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>Number of records in the file if known (use determineColumnSize), -1 otherwise</returns>
    Task OpenAsync(CancellationToken token);

    /// <summary>
    ///   Advances the <see cref="T:System.Data.IDataReader" /> to the next record.
    /// </summary>
    /// <returns>
    ///   <see langword="true" /> if there are more rows; otherwise, <see langword="false" />.
    /// </returns>
    [Obsolete("Use ReadAsync if possible")]
    new bool Read();

    /// <summary>
    ///   Reads the next record of the current result set asynchronously
    /// </summary>
    /// <param name="token">The cancellation token</param>
    /// <returns>
    ///   <see langword="true" /> if there are more rows; otherwise, <see langword="false" />.
    /// </returns>
    Task<bool> ReadAsync(CancellationToken token);

    /// <summary>
    /// Get The binary data from a file at the location of the column content
    /// </summary>
    /// <param name="ordinal">The zero-based column ordinal.</param>
    byte[] GetFile(int ordinal);

    /// <summary>
    ///   Resets the position and buffer to the first data row (handing headers, and skipped rows)
    /// </summary>
    void ResetPositionToFirstDataRow();
  }
}