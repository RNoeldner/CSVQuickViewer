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

namespace CsvTools
{
  /// <summary>
  ///  Interface for a File Writer.
  /// </summary>
  public interface IFileWriter
  {
    /// <summary>
    ///  Event handler called as progress should be displayed
    /// </summary>
    event EventHandler<ProgressEventArgs> Progress;

    /// <summary>
    ///  Event handler called if a warning or error occurred
    /// </summary>
    event EventHandler<WarningEventArgs> Warning;

    /// <summary>
    ///  Event handler called as writing is done
    /// </summary>
    event EventHandler WriteFinished;

    /// <summary>
    ///  Gets the error message.
    /// </summary>
    string ErrorMessage { get; }

    IProcessDisplay ProcessDisplay { get; set; }

    /// <summary>Gets the column information from the reader and overwrite setting with definition from the setting.</summary>
    /// <param name="reader">Any data reader</param>
    IEnumerable<ColumnInfo> GetColumnInformation(IDataReader reader);

    /// <summary>Gets the a data reader for the allowing to look at the schema.</summary>
    IDataReader GetSchemaReader();

    /// <summary>
    ///  Gets the source data table.
    /// </summary>
    /// <param name="recordLimit">The record limit.</param>
    /// <returns>A data table with all source data</returns>
    DataTable GetSourceDataTable(uint recordLimit);

    /// <summary>
    ///  Writes the specified file.
    /// </summary>
    /// <returns>Number of records written</returns>
    long Write();

    /// <summary>
    ///  Writes the specified file reading from the a data table
    /// </summary>
    /// <param name="source">The data that should be written in a <see cref="DataTable" /></param>
    /// <returns>Number of records written</returns>
    long WriteDataTable(DataTable source);
  }
}