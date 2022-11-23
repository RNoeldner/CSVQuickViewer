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
  /// <summary>
  /// Base class for all ColumnFormatters
  /// </summary>
  public abstract class BaseColumnFormatter : IColumnFormatter
  {

    /// <inheritdoc/>
    public abstract string FormatInputText(in string inputString, Action<string>? handleWarning);

    /// <inheritdoc/>
    public virtual string Write(object? dataObject, IDataRecord? dataRow, Action<string>? handleWarning)
      => dataObject?.ToString() ?? string.Empty;

    /// <inheritdoc/>
    public bool RaiseWarning { get; set; } = true;
  }
}