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

using System;
using System.Drawing;
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;

namespace CsvTools
{
  public class SyntaxHighlighterDelimitedText : SyntaxHighlighterBase
  {
    private readonly Regex m_CommentRegex;
    private readonly Regex m_DelimiterRegex;
    private readonly Regex m_QuoteRegex;
    private readonly Style m_Space = new SyntaxHighlightStyleStyleSpace(Brushes.Blue, Brushes.AntiqueWhite);
    private readonly Regex m_SpaceRegex = new Regex(" ", RegexOptions.Singleline | RegexOptions.Compiled);
    private readonly Style m_Tab = new SyntaxHighlightStyleTab(Pens.Blue, Brushes.AntiqueWhite);
    private readonly Regex m_TabRegex = new Regex("\\t", RegexOptions.Singleline | RegexOptions.Compiled);

    public SyntaxHighlighterDelimitedText(FastColoredTextBox textBox, string qualifier, string delimiter, string escape, string comment) : base(textBox)
    {
      qualifier=(qualifier??string.Empty).WrittenPunctuation();
      delimiter=(delimiter??string.Empty).WrittenPunctuation();
      escape=(escape??string.Empty).WrittenPunctuation();

      m_DelimiterRegex = new Regex(
        string.IsNullOrEmpty(escape) ? $"\\{delimiter}" : $"(?<!\\{escape})\\{delimiter}",
        RegexOptions.Singleline | RegexOptions.Compiled);
      m_QuoteRegex = new Regex(
        string.IsNullOrEmpty(escape)
          ? $"\\{qualifier}((?:\\{qualifier}\\{qualifier}|(?:(?!\\{qualifier})).)*)\\{qualifier}"
          : $"\\{qualifier}((?:\\{escape}\\{qualifier}|\\{qualifier}\\{qualifier}|(?:(?!\\{qualifier})).)*)\\{qualifier}",
        RegexOptions.Multiline | RegexOptions.Compiled);
      if (!string.IsNullOrEmpty(comment))
        m_CommentRegex = new Regex($"\\s*{comment}.*$", RegexOptions.Multiline | RegexOptions.Compiled);
    }

    public override void Highlight(FastColoredTextBoxNS.Range range)
    {
      range.ClearStyle(StyleIndex.All);
      range.SetStyle(BlueStyle, m_DelimiterRegex);
      range.SetStyle(MagentaStyle, m_QuoteRegex);

      if (m_CommentRegex != null)
        range.SetStyle(GrayStyle, m_CommentRegex);
      range.SetStyle(m_Space, m_SpaceRegex);
      range.SetStyle(m_Tab, m_TabRegex);
    }

    internal class SyntaxHighlightStyleStyleSpace : Style
    {
      private readonly Brush m_BackGround;
      private readonly Brush m_ForeGround;

      public SyntaxHighlightStyleStyleSpace(Brush foreGround, Brush backGround)
      {
        m_ForeGround = foreGround;
        m_BackGround = backGround;
      }

      public override void Draw(Graphics gr, Point position, FastColoredTextBoxNS.Range range)
      {
        //get size of rectangle
        var size = GetSizeOfRange(range);
        var rect = new Rectangle(position, size);
        // background
        rect.Inflate(-1, -1);
        gr.FillRectangle(m_BackGround, rect);

        var sizeChar = size.Width / (range.End.iChar - range.Start.iChar);
        var dotSize = new Size(Math.Min(Math.Max(sizeChar, 8), 3), Math.Min(Math.Max(size.Height, 8), 3));

        var posDot = new Point((position.X + (sizeChar / 2)) - (dotSize.Width / 2),
          (position.Y + (size.Height / 2)) - (dotSize.Height / 2));
        for (var pos = range.Start.iChar; pos < range.End.iChar; pos++)
        {
          // draw a dot
          gr.FillEllipse(m_ForeGround, new Rectangle(posDot, dotSize));
          posDot.X += sizeChar;
        }
      }
    }

    internal class SyntaxHighlightStyleTab : Style
    {
      private readonly Brush m_BackGround;
      private readonly Pen m_ForeGround;

      public SyntaxHighlightStyleTab(Pen foreGround, Brush backGround)
      {
        m_ForeGround = foreGround;
        m_BackGround = backGround;
      }

      public override void Draw(Graphics gr, Point position, FastColoredTextBoxNS.Range range)
      {
        //get size of rectangle
        var size = GetSizeOfRange(range);
        var rect = new Rectangle(position, size);
        rect.Inflate(-1, -1);
        gr.FillRectangle(m_BackGround, rect);

        var sizeChar = size.Width / (range.End.iChar - range.Start.iChar);
        var height = size.Height;

        for (var pos = range.Start.iChar; pos < range.End.iChar; pos++)
        {
          var rect2 = new Rectangle(position, new Size(sizeChar, height));

          // draw an arrow
          var point2 = new Point((rect2.X + sizeChar) - 2, rect2.Y + (height / 2)-1);

          gr.DrawLine(m_ForeGround, new Point(rect2.X + 1, point2.Y), point2);
          gr.DrawLine(m_ForeGround, new Point(rect2.X + (sizeChar / 2), rect2.Y + (height / 4)), point2);
          gr.DrawLine(m_ForeGround, new Point(rect2.X + (sizeChar / 2), rect2.Y + ((rect2.Height * 3) / 4)), point2);

          // double line in case its larger
          if (height > 6)
            gr.DrawLine(m_ForeGround, rect2.X + 1, point2.Y + 1, point2.X, point2.Y + 1);

          if (sizeChar > 6)
          {
            gr.DrawLine(m_ForeGround, rect2.X + (sizeChar / 2) + 1, rect2.Y + (height / 4), point2.X + 1, point2.Y);
            gr.DrawLine(m_ForeGround, rect2.X + (sizeChar / 2) + 1, rect2.Y + ((rect2.Height * 3) / 4), point2.X + 1,
              point2.Y);
          }

          position.X += sizeChar;
        }
      }
    }
  }
}