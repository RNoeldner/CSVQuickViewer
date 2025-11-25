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
/// Formatter to handle HTML, much slower than <see cref="TextToHtmlFormatter"/> but handling all characters
/// </summary>
public class TextToHtmlFullFormatter : BaseColumnFormatter
{
  /// <summary>
  /// Static instance of the formatter
  /// </summary>
  public static readonly TextToHtmlFullFormatter Instance = new TextToHtmlFullFormatter();

  /// <inheritdoc/>
  public override string Write(in object? dataObject, in IDataRecord? dataRow, Action<string>? handleWarning) => 
    dataObject is null ? string.Empty : HtmlStyle.HtmlEncode(dataObject.ToString() ?? string.Empty);

  /// <inheritdoc/>
  public override string FormatInputText(string inputString, Action<string>? handleWarning)
  {
    var output = HtmlStyle.HtmlEncodeShort(inputString);
    if (RaiseWarning && !inputString.Equals(output, StringComparison.Ordinal))
      handleWarning?.Invoke($"HTML encoding removed from {inputString}");
    return output!;
  }
  /// <inheritdoc/>
  public override ReadOnlySpan<char> FormatInputText(ReadOnlySpan<char> inputString)
    => HtmlStyle.HtmlEncodeShort(inputString.ToString()).AsSpan();

}