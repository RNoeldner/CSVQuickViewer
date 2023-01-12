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

namespace CsvTools
{
  public abstract class SyntaxHighlighterBase : FastColoredTextBoxNS.SyntaxHighlighter
  {
#pragma warning disable CA1416
    protected readonly FastColoredTextBoxNS.TextStyle SkipStyle = new FastColoredTextBoxNS.TextStyle(Brushes.DarkGray, Brushes.LightGray, FontStyle.Regular);
#pragma warning restore CA1416

    public SyntaxHighlighterBase(in FastColoredTextBoxNS.FastColoredTextBox currentTb) : base(currentTb)
    {
    }

    public abstract void Highlight(FastColoredTextBoxNS.Range range);

    public void SkipRows(int skipRows)
    {
      if (skipRows <= 0) return;
      var range = new FastColoredTextBoxNS.Range(currentTb, 0, 0, 0, skipRows);
      range.ClearStyle(FastColoredTextBoxNS.StyleIndex.All);
      range.SetStyle(SkipStyle);
    }

    public virtual void Comment(FastColoredTextBoxNS.Range range)
    {
      range.ClearStyle(FastColoredTextBoxNS.StyleIndex.All);
      range.SetStyle(GrayStyle);
    }
  }
}