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
using System.Text.RegularExpressions;

namespace CsvTools
{
  public sealed class RegExReplace : IColumnFormatter
  {
    private readonly string m_Replacement = string.Empty;
    private readonly Regex? m_Regex;

    public bool RaiseWarning { get; set; } = true;

    public RegExReplace(string searchPattern, string replace)
    {
      if (string.IsNullOrWhiteSpace(searchPattern))
        return;
      m_Regex = new Regex(searchPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
      m_Replacement = replace;
    }

    public string FormatText(in string inputString, Action<string>? handleWarning)
    {
      if (m_Regex?.IsMatch(inputString) ?? false)
      {
        var output = m_Regex.Replace(inputString, m_Replacement);
        if (RaiseWarning && !output.Equals(inputString))
          handleWarning?.Invoke($"Regular Expression Replace {inputString}");
        return output ?? string.Empty;
      }
      return inputString;
    }
  }
}