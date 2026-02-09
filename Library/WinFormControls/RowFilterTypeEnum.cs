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
using System;
using System.ComponentModel;

namespace CsvTools;

/// <summary>
/// Filter Types supported by DataGrid
/// </summary>
[Flags]
public enum RowFilterTypeEnum
{
  /// <summary>
  /// Display rows that have no error nor warning
  /// </summary>
  [Description("Display rows that have no error nor warning")]
  [ShortDescription("No Error or Warning")]
  Clean = 1,

  /// <summary>
  /// Display rows that have a warning
  /// </summary>
  [Description("Display rows that have a warning")]
  [ShortDescription("Warnings")]
  Warning = Clean << 1,

  /// <summary>
  /// Display rows that have an error
  /// </summary>
  [Description("Display rows that have an error")]
  [ShortDescription("Errors")]
  Errors = Warning << 1,

  /// <summary>
  /// Display rows that have an error or a warning
  /// </summary>
  [Description("Display rows that have an error or a warning")]
  [ShortDescription("Errors or Warnings")]
  ErrorsAndWarning = Errors | Warning,

  /// <summary>
  /// Display rows that either an error, a true error or a warning
  /// </summary>
  [Description("Display rows that either an error, a true error or a warning")]
  [ShortDescription("All Records")]
  All = Errors | Warning | Clean,
}