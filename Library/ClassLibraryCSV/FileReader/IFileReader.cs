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
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools;

/// <inheritdoc cref="System.Data.IDataReader" />
/// <summary>
///   Interface for a File Reader.
/// </summary>
public interface IFileReader : IDataReader
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
, IAsyncDisposable
#endif
{
  #region Events and Callbacks

  /// <summary>
  ///   Occurs when an open process failed, allowing the user to change the timeout or provide the
  ///   needed file etc.
  /// </summary>
  event EventHandler<RetryEventArgs>? OnAskRetry;

  /// <summary>
  ///   Event to be raised once the reader is opened, the column information is now known and
  ///   passed to the EventHandler
  /// </summary>
  event EventHandler<IReadOnlyCollection<Column>>? OpenFinished;

  /// <summary>
  ///   Event to be raised once the reader is finished reading the file
  /// </summary>
  event EventHandler? ReadFinished;


  /// <summary>
  ///   Occurs before the initial open. Can be used to prepare the data like download it from a
  ///   remote location
  /// </summary>
  Func<Task>? OnOpenAsync { set; }


  /// <summary>
  ///   Event handler called if a warning or error occurred
  /// </summary>
  public event EventHandler<WarningEventArgs>? Warning;

  /// <summary>
  /// Invokes the read finished
  /// </summary>
  void HandleReadFinished();

  #endregion

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
  ///   Value between 0 and 100 to show the progress of the reader, not all readers do support
  ///   this, readers based on streams usually return the relative position in that stream.
  /// </summary>
  int Percent { get; }

  /// <summary>
  ///   Gets the record number of the record that just had been read (staring with 1)
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
  /// <returns>A <see cref="Column" /> with all information on the column</returns>
  Column GetColumn(int column);

  /// <summary>
  ///   Advances the data reader to the next result, when reading the results of batch SQL statements. This is usually not supported by IFileReader
  /// </summary>
  /// <returns>
  ///   <see langword="true" /> if there are more results; otherwise, <see langword="false" />.
  /// </returns>
  [Obsolete("Not supported")]
  new bool NextResult();

  /// <summary>
  ///   Opens the text file and begins to read the metadata, like columns
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
  ///   Resets the position and buffer to the first data row (handing headers, and skipped rows)
  /// </summary>
  void ResetPositionToFirstDataRow();

  /// <summary>
  ///   Sets the progress reporting action <see cref="ProgressInfo.Value"/> will be between 0 and 10000/>
  /// </summary>
  IProgress<ProgressInfo> ReportProgress { set; }
}