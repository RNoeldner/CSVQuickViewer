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
using System.Data;

namespace CsvTools;

/// <summary>
/// Interface for a read and write formatter
/// </summary>
public interface IColumnFormatter
{
  /// <summary>
  /// If <c>true</c> warning are raised with handle Warning
  /// </summary>
  bool RaiseWarning { get; set; }

  /// <summary>
  /// Format the text while reading, if <see cref="RaiseWarning"/> is true, use handleWarning to pass on possible issues
  /// </summary>
  /// <param name="inputString">The input text that need to be processed</param>
  /// <param name="handleWarning">Action to be invoked if a warning needs to be passed on</param>
  /// <returns>The formatted text</returns>
  string FormatInputText(string inputString, Action<string>? handleWarning);

  /// <summary>
  /// Format the text while reading, unlike in the string implementation this is built for speed, no warning will be raised.
  /// </summary>
  /// <param name="inputString">The input span that need to be processed</param>    
  /// <returns>The formatted text span </returns>
  ReadOnlySpan<char> FormatInputText(ReadOnlySpan<char> inputString);

  /// <summary>
  /// Write the dataObject, if <see cref="RaiseWarning"/> is true, use handleWarning to pass on possible issues
  /// </summary>
  /// <param name="dataObject">The data to be processed</param>
  /// <param name="dataRow">All values for the current row to support placeholders, to handle placeholders etc.</param>
  /// <param name="handleWarning"></param>
  /// <returns>An awaitable task with the text a text representation</returns>
  string Write(in object? dataObject, in IDataRecord? dataRow, Action<string>? handleWarning);
}