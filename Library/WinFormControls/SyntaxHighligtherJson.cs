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
using FastColoredTextBoxNS;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CsvTools;

public sealed class SyntaxHighlighterJson : SyntaxHighlighterBase
{
  private readonly Regex m_JsonKeywordRegex = new Regex(@"(?<range>""([^\\""]|\\"")*"")\s*:",
    RegexOptions.Singleline | RegexCompiledOption, TimeSpan.FromSeconds(1));

  private readonly Regex m_JsonNumberRegex = new Regex(@"\b(\d+[\.]?\d*|true|false|null)\b",
    RegexOptions.Singleline | RegexCompiledOption, TimeSpan.FromSeconds(1));

  private readonly Regex m_JsonStringRegex =
    new Regex("\\\"((?:\\\\\\\"|(?:(?!\\\")).)*)\\\"", RegexOptions.Singleline | RegexCompiledOption, TimeSpan.FromSeconds(1));

  public SyntaxHighlighterJson(FastColoredTextBox fastColoredTextBox) : base(fastColoredTextBox)
  {
    if (fastColoredTextBox is null)
      return;
    fastColoredTextBox.LeftBracket = '[';
    fastColoredTextBox.RightBracket = ']';
    fastColoredTextBox.LeftBracket2 = '{';
    fastColoredTextBox.RightBracket2 = '}';
    fastColoredTextBox.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;

    fastColoredTextBox.AutoIndentCharsPatterns = @"^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>[^;]+);";
  }

  public override void Highlight(FastColoredTextBoxNS.Range range)
  {
    try
    {
      //clear style of changed range
      range.ClearStyle(StyleIndex.All);

      //keyword highlighting
      range.SetStyle(BlueStyle, m_JsonKeywordRegex);
      //string highlighting
      range.SetStyle(BrownStyle, m_JsonStringRegex);
      //number highlighting
      range.SetStyle(MagentaStyle, m_JsonNumberRegex);
      //clear folding markers
      range.ClearFoldingMarkers();
      //set folding markers
      range.SetFoldingMarkers("{", "}"); //allow to collapse brackets block
      range.SetFoldingMarkers(@"\[", @"\]"); //allow to collapse comment block
    }
    catch (Exception e)
    {
      Debug.WriteLine($"Issue in Highlight {e.Message}");
    }
  }
}