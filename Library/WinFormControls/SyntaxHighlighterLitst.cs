// /*
// * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com *
// * This program is free software: you can redistribute it and/or modify it under the terms of the
// GNU Lesser Public
// * License as published by the Free Software Foundation, either version 3 of the License, or (at
// your option) any later version. *
// * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty
// * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for
// more details. *
// * You should have received a copy of the GNU Lesser Public License along with this program.
// * If not, see http://www.gnu.org/licenses/ . *
// */

using System.Drawing;
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;

namespace CsvTools
{
  public class SyntaxHighlighterList : SyntaxHighlighterBase
  {
    private readonly Style Seperator = new TextStyle(Brushes.Blue, Brushes.AntiqueWhite, FontStyle.Regular);

    private readonly Regex m_ODataSperatorRegex = new Regex(@"(,|;|\|)",
      RegexOptions.Singleline | RegexCompiledOption);

    private readonly Regex m_ODataStringRegex =
      new Regex(@"\[*\]", RegexOptions.Singleline | RegexCompiledOption);

    public SyntaxHighlighterList(FastColoredTextBox currentTb) : base(currentTb)
    {
      currentTb.TextChangedDelayed += (sender, e) => Highlight(e.ChangedRange);
    }

    public override void Highlight(Range range)
    {
      range.ClearStyle(MagentaStyle, Seperator);

      // string highlighting
      range.SetStyle(MagentaStyle, m_ODataStringRegex);
      range.SetStyle(Seperator, m_ODataSperatorRegex);
    }
  }
}