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

namespace CsvTools
{
	/// <summary>
	///  Interface for a File Reader.
	/// </summary>
	public interface IFileReader : IDataReader
	{		
		/// <summary>
		///  Event handler called if a warning or error occurred
		/// </summary>
		event EventHandler<WarningEventArgs> Warning;

		/// <summary>
		///  Gets the end line number
		/// </summary>
		/// <value>The line number in which the record ended</value>
		long EndLineNumber { get; }

		/// <summary>
		///  Determine if the data Reader is at the end of the file
		/// </summary>
		/// <returns>True if you can read; otherwise, false.</returns>
		bool EndOfFile { get; }

		/// <summary>
		///  Gets the record number.
		/// </summary>
		/// <value>The record number.</value>
		long RecordNumber { get; }

		/// <summary>
		///  Gets the start line number.
		/// </summary>
		/// <value>The line number in which the record started.</value>
		long StartLineNumber { get; }

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
		/// <returns>
		/// Number of records in the file if known (use determineColumnSize), -1 otherwise
		/// </returns>
		void Open();

		/// <summary>
		///  Overrides the column format with values from settings
		/// </summary>
		void OverrideColumnFormatFromSetting();

		/// <summary>
		///  Resets the position and buffer to the header in case the file has a header
		/// </summary>
		void ResetPositionToFirstDataRow();

		/// <summary>
		/// Underlying FileSetting
		/// </summary>
		IFileSetting FileSetting { get; }
	}
}