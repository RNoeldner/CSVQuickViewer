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
#nullable enable

using System;
using System.Data;

namespace CsvTools
{
  public interface IColumnFormatter
  {
    /// <summary>
    /// Format the text while reading, if <see cref="RaiseWarning"/> is true, use handleWarning to pass on possible issues
    /// </summary>
    /// <param name="inputString">The input text that need to be processed</param>
    /// <param name="handleWarning">Action to be invoked if a warning needs to be passed on</param>
    /// <returns>The formatted text</returns>
    string FormatInputText(in string inputString, in Action<string>? handleWarning);

    /// <summary>
    /// Write the dataObject, if <see cref="RaiseWarning"/> is true, use handleWarning to pass on possible issues
    /// </summary>
    /// <param name="dataObject">The data to be processed</param>
    /// <param name="dataRow">All values for the current row to support placeholders, to handle placeholders etc.</param>
    /// <param name="handleWarning"></param>
    /// <returns>An awaitable task with the text a text representation</returns>
    string Write(in object? dataObject, in IDataRecord? dataRow, in Action<string>? handleWarning);

    /// <summary>
    /// If <c>true</c> warning are raised with handle Warning
    /// </summary>
    bool RaiseWarning { get; set; }
  }
}