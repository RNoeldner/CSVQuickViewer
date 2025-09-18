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
 * If not, see http://www.gnu.org/licenses/.
 */

#nullable enable
namespace CsvTools
{
  /// <summary>
  ///   Defines general settings for handling a file.
  /// </summary>
  public interface IFileSetting : IWithCopyTo<IFileSetting>
  {
    // ========================
    //  Display Options
    // ========================

    /// <summary>
    ///   Gets or sets a value indicating whether the starting line number of each record should be displayed.
    /// </summary>
    bool DisplayStartLineNo { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether the ending line number of each record should be displayed.
    /// </summary>
    bool DisplayEndLineNo { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether the record number should be displayed.
    /// </summary>
    bool DisplayRecordNo { get; set; }

    /// <summary>
    ///   Returns a descriptive string for debugging and UI display purposes.
    /// </summary>
    string GetDisplay();


    // ========================
    //  File Structure
    // ========================

    /// <summary>
    ///   Gets the column format definitions.
    /// </summary>
    ColumnCollection ColumnCollection { get; }

    /// <summary>
    ///   Gets or sets the header text for outbound data.
    /// </summary>
    string Header { get; set; }

    /// <summary>
    ///   Gets or sets the footer text for outbound data.
    /// </summary>
    string Footer { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether the file contains a header row.
    /// </summary>
    bool HasFieldHeader { get; set; }


    // ========================
    //  Processing Rules
    // ========================

    /// <summary>
    ///   Gets or sets the maximum number of records to process.
    ///   A value of <c>0</c> means no limit.
    /// </summary>
    long RecordLimit { get; set; }

    /// <summary>
    ///   Gets or sets the maximum number of consecutive empty rows before processing stops.
    /// </summary>
    int ConsecutiveEmptyRows { get; set; }

    /// <summary>
    ///   Gets or sets the number of initial rows to skip (e.g., metadata or comments before the actual data).
    /// </summary>
    int SkipRows { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether rows identical to the header row should be skipped.
    ///   Useful when multiple delimited files are concatenated without removing duplicate headers.
    /// </summary>
    bool SkipDuplicateHeader { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether empty lines should be skipped.
    /// </summary>
    bool SkipEmptyLines { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether values should be trimmed of leading and trailing whitespace.
    /// </summary>
    bool Trim { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether non-breaking spaces (NBSP) should be treated as regular spaces.
    /// </summary>
    bool TreatNBSPAsSpace { get; set; }

    /// <summary>
    ///   Gets or sets a text marker that should be interpreted as <c>null</c>.
    /// </summary>
    string TreatTextAsNull { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether an unencrypted temporary file should be kept
    ///   when processing encrypted files. If <c>false</c>, the temporary file is deleted.
    /// </summary>
    bool KeepUnencrypted { get; set; }
  }
}
