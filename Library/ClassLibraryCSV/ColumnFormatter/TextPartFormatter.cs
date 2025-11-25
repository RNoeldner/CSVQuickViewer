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

namespace CsvTools;

/// <inheritdoc />
public sealed class TextPartFormatter : BaseColumnFormatter
{
  private readonly int m_Part;
  private readonly char m_PartSplitter;
  private readonly bool m_PartToEnd;

  /// <summary>
  /// Constructor
  /// </summary>
  /// <param name="part">Part number to pick</param>
  /// <param name="partSplitter">Splitter to take a text apart with</param>
  /// <param name="partToEnd">if <c>true</c> start at that part but take all till the end</param>
  public TextPartFormatter(int part, char partSplitter, bool partToEnd)
  {
    m_Part = part;
    m_PartSplitter = partSplitter;
    m_PartToEnd = partToEnd;
  }

  /// <inheritdoc/>
  public override string FormatInputText(string inputString, Action<string>? handleWarning)
  {
    var output = inputString.AsSpan().StringToTextPart(m_PartSplitter, m_Part, m_PartToEnd);
    if (RaiseWarning && output.IsEmpty)
      handleWarning?.Invoke($"Part {m_Part} of text {inputString} is empty.");
    return output.ToString();
  }

  /// <inheritdoc/>
  public override ReadOnlySpan<char> FormatInputText(ReadOnlySpan<char> inputString)
    => inputString.StringToTextPart(m_PartSplitter, m_Part, m_PartToEnd);
}