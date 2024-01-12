﻿/*
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
  /// Formatter to handle HTML, this one handling only some special charters like linefeed, &lt;, &gt;, " 
  /// </summary>
  public class TextToHtmlFormatter : BaseColumnFormatter
  {
    /// <summary>
    /// Static instance of the formatter
    /// </summary>
    public static readonly TextToHtmlFormatter Instance = new TextToHtmlFormatter();

    /// <inheritdoc/>
    public override string Write(in object? dataObject, in IDataRecord? dataRow, in Action<string>? handleWarning)
    {
      if (dataObject is null)
        return string.Empty;
      return HtmlStyle.HtmlEncodeShort(dataObject.ToString()) ?? string.Empty;
    }

    /// <inheritdoc/>
    public override string FormatInputText(in string inputString, in Action<string>? handleWarning)
    {
      var output = HtmlStyle.TextToHtmlEncode(inputString);
      if (RaiseWarning && !inputString.Equals(output, StringComparison.Ordinal))
        handleWarning?.Invoke($"HTML encoding removed from {inputString}");
      return output;
    }

    /// <inheritdoc/>
    public override ReadOnlySpan<char> FormatInputText(ReadOnlySpan<char> inputString)
      => HtmlStyle.TextToHtmlEncode(inputString.ToString()).AsSpan();    
  }
}