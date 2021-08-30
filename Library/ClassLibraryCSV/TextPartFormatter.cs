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

namespace CsvTools
{
  public class TextPartFormatter : IColumnFormatter
  {
    private readonly int m_Part;

    private readonly char m_PartSplitter;

    private readonly bool m_PartToEnd;

    public TextPartFormatter()
      : this(
        ValueFormatExtension.cPartDefault,
        ValueFormatExtension.cPartSplitterDefault,
        ValueFormatExtension.cPartToEndDefault)
    {
    }

    public TextPartFormatter(int part, string partSplitter, bool partToEnd)
    {
      m_Part = part;
      m_PartSplitter = partSplitter.StringToChar();
      m_PartToEnd = partToEnd;
    }

    public string Description => $"Part {m_Part}{(m_PartToEnd ? " To End" : string.Empty)}";

    public string FormatText(in string inputString, Action<string>? handleWarning)
    {
      var output = StringConversion.StringToTextPart(inputString, m_PartSplitter, m_Part, m_PartToEnd);
      if (output is null)
        handleWarning?.Invoke($"Part {m_Part} of text {inputString} is empty.");
      return output ?? string.Empty;
    }
  }
}