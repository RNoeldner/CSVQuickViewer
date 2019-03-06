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

namespace CsvTools
{
  /// <summary>
  ///  Interface for a File Reader.
  /// </summary>
  public interface IFileReader : IDataReader
  {
    /// <summary>
    ///  Event handler called as the read is done
    /// </summary>
    event EventHandler ReadFinished;

    /// <summary>
    ///  Event handler called if a warning or error occurred
    /// </summary>
    event EventHandler<WarningEventArgs> Warning;

    /// <summary>
    ///  The Cancellation Token used by the progress form
    /// </summary>
    CancellationToken CancellationToken { set; }

    /// <summary>
    ///  Gets the end line number
    /// </summary>
    /// <value>The line number in which the record ended</value>
    long EndLineNumber { get; }

    /// <summary>
    ///  Gets the end name of the line number field.
    /// </summary>
    /// <value>The end name of the line number field.</value>
    string EndLineNumberFieldName { get; }

    /// <summary>
    ///  Determine if the data Reader is at the end of the file
    /// </summary>
    /// <returns>True if you can read; otherwise, false.</returns>
    bool EndOfFile { get; }

    /// <summary>
    ///  Gets the field name for persisted error information
    /// </summary>
    /// <value>The error field.</value>
    string ErrorField { get; }

    /// <summary>
    ///  Process display for this File Reader
    /// </summary>
    IProcessDisplay ProcessDisplay { set; }

    /// <summary>
    ///  Gets the record number.
    /// </summary>
    /// <value>The record number.</value>
    long RecordNumber { get; }

    /// <summary>
    ///  Gets the name of the record number field.
    /// </summary>
    /// <value>The name of the record number field.</value>
    string RecordNumberFieldName { get; }

    /// <summary>
    ///  Gets the start line number.
    /// </summary>
    /// <value>The line number in which the record started.</value>
    long StartLineNumber { get; }

    /// <summary>
    ///  Gets the start name of the line number field.
    /// </summary>
    /// <value>The start name of the line number field.</value>
    string StartLineNumberFieldName { get; }

    /// <summary>
    ///  Gets the column information for a given column number
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns>A <see cref="Column" /> with all information on the column</returns>
    Column GetColumn(int column);

    /// <summary>
    ///  Checks if the column should be read
    /// </summary>
    /// <param name="column">The column number.</param>
    /// <returns><c>true</c> if this column should not be read</returns>
    bool IgnoreRead(int column);

    /// <summary>
    /// Opens the text file and begins to read the meta data, like columns
    /// </summary>
    /// <param name="determineColumnSize">Determine the maximum column size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// Number of records in the file if known (use determineColumnSize), -1 otherwise
    /// </returns>
    long Open(bool determineColumnSize, CancellationToken cancellationToken);

    /// <summary>
    ///  Overrides the column format with values from settings
    /// </summary>
    /// <param name="fieldCount">The field count.</param>
    void OverrideColumnFormatFromSetting(int fieldCount);

    /// <summary>
    ///  Resets the position and buffer to the header in case the file has a header
    /// </summary>
    void ResetPositionToFirstDataRow();
  }
}