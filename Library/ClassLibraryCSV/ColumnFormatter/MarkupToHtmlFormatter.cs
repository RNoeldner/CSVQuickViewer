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

#if !QUICK
using HeyRed.MarkdownSharp;
using System;

namespace CsvTools
{

  public class MarkupToHtmlFormatter : BaseColumnFormatter
  {
    private readonly Markdown m_Markdown;

    public MarkupToHtmlFormatter()
    {
      m_Markdown = new Markdown(new MarkdownOptions()
      {
        AllowEmptyLinkText = false,
        AutoHyperlink = true,
        DisableImages = true,
        LinkEmails = true,
        QuoteSingleLine = false,
        AutoNewLines = true
      });
    }

    public override string FormatInputText(in string inputString, Action<string>? handleWarning)
    {
      var output = m_Markdown.Transform(inputString);
      if (RaiseWarning && !inputString.Equals(output, StringComparison.Ordinal))
        handleWarning?.Invoke($"Markdown encoding");
      return output;
    }
  }
}
#endif