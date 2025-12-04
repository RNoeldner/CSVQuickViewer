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
namespace CsvTools;

/// <summary>
///   Interface for a FileSetting
/// </summary>
public interface IFileSetting : IWithCopyTo<IFileSetting>
{
  /// <summary>
  ///   Gets or sets the column formats
  /// </summary>
  /// <value>The column format.</value>
  ColumnCollection ColumnCollection { get; }

  /// <summary>
  ///   Gets or sets the consecutive empty rows.
  /// </summary>
  /// <value>The consecutive empty rows.</value>
  int ConsecutiveEmptyRows { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether to display line numbers.
  /// </summary>
  /// <value><c>true</c> if line no should be displayed; otherwise, <c>false</c>.</value>
  bool DisplayEndLineNo { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether display record numbers
  /// </summary>
  /// <value><c>true</c> if record no should be displayed; otherwise, <c>false</c>.</value>
  bool DisplayRecordNo { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether to display line numbers.
  /// </summary>
  /// <value><c>true</c> if line no should be displayed; otherwise, <c>false</c>.</value>
  bool DisplayStartLineNo { get; set; }

  /// <summary>
  ///   Gets or sets the Footer.
  /// </summary>
  /// <value>The Footer for outbound data.</value>
  string Footer { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether this instance has field header.
  /// </summary>
  /// <value><c>true</c> if this instance has field header; otherwise, <c>false</c>.</value>
  bool HasFieldHeader { get; set; }

  /// <summary>
  ///   Gets or sets the Header.
  /// </summary>
  /// <value>The Header for outbound data.</value>
  string Header { get; set; }

  /// <summary>
  ///   When a file is encrypted the not encrypted version temporary file is removed When data is
  ///   sent into a steam the data can not be access Set to <c>true</c> a readable file is not
  ///   removed / is created
  /// </summary>
  bool KeepUnencrypted { get; set; }

  /// <summary>
  ///   Gets or sets the record limit.
  /// </summary>
  /// <value>The record limit. if set to 0 there is no limit</value>
  long RecordLimit { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether to ignore rows that match the header row, this
  ///   happens if two delimited files are appended without removing the header of the appended file
  /// </summary>
  bool SkipDuplicateHeader { get; set; }
  /// <summary>
  ///   Gets or sets a value indicating if the reader will skip empty lines.
  /// </summary>
  /// <value>if <c>true</c> the reader or writer will skip empty lines.</value>
  bool SkipEmptyLines { get; set; }

  /// <summary>
  ///   Gets or sets the values of rows that should be ignored in the beginning, e.G. for information not related  to the data
  /// </summary>
  int SkipRows { get; set; }

  /// <summary>
  ///   Number of rows to skip immediately after the header row.  
  ///   In most text-based files, comment lines are automatically ignored and empty lines are skipped. 
  ///   For Excel or similar sources where comments are not supported, skipping rows often achieved by adjusting the starting range.  
  ///   This parameter is typically used when the rows after the header contain descriptive information, units, or other metadata about the columns.
  /// </summary>
  int SkipRowsAfterHeader { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether to treat NBSP as space.
  /// </summary>
  /// <value><c>true</c> if NBSP should be treated as space; otherwise, <c>false</c>.</value>
  bool TreatNBSPAsSpace { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating whether this instance should treat any text listed here as Null
  /// </summary>
  string TreatTextAsNull { get; set; }

  /// <summary>
  ///   Gets or sets a value indicating if values should be trimmed.
  /// </summary>
  bool Trim { get; set; }

  /// <summary>
  ///   Get a description of for debugging and UI display
  /// </summary>
  string GetDisplay();
}