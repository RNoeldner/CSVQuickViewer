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

  public class TextToHtmlFormatter : BaseColumnFormatter
  {
    /// <inheritdoc/>
    public override string Write(object? dataObject, IDataRecord? dataRow, Action<string>? handleWarning)
    {
      if (dataObject is null)
        return string.Empty;
      return HtmlStyle.HtmlEncodeShort(dataObject.ToString()) ?? string.Empty;
    }


    /// <inheritdoc/>
    public override string FormatInputText(in string inputString, Action<string>? handleWarning)
    {
      var output = HtmlStyle.TextToHtmlEncode(inputString);
      if (RaiseWarning && !inputString.Equals(output, StringComparison.Ordinal))
        handleWarning?.Invoke($"HTML encoding removed from {inputString}");
      return output;
    }
  }
}