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

#nullable enable

using System;
using System.Drawing;
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;

namespace CsvTools
{
  public sealed class SyntaxHighlighterDelimitedText : SyntaxHighlighterBase
  {
    private readonly Regex? m_CommentRegex;
    private readonly Regex m_DelimiterRegex;
    private readonly Regex? m_QuoteRegex;
#pragma warning disable CA1416
    private readonly Style m_Space = new SyntaxHighlightStyleStyleSpace(Brushes.Blue, Brushes.AntiqueWhite);
    //private readonly Style m_Tab = new SyntaxHighlightStyleTab(Pens.Blue, Brushes.AntiqueWhite);
    private readonly Style m_Tab2 = new SyntaxHighlightStyleTab(Pens.LightGray, Brushes.AntiqueWhite);
#pragma warning restore CA1416
    private readonly Regex m_SpaceRegex = new Regex(" ", RegexOptions.Singleline | RegexOptions.Compiled);
    //private readonly Regex m_TabRegex1 = new Regex("\\t", RegexOptions.Singleline | RegexOptions.Compiled);
    private readonly Regex m_TabRegex2 = new Regex("⇥", RegexOptions.Singleline | RegexOptions.Compiled);

    public SyntaxHighlighterDelimitedText(FastColoredTextBox textBox, string qualifierText, string delimiterText, string escapeText,
      string comment) : base(textBox)
    {
      var qualifier = new Punctuation(qualifierText);
      var delimiter = new Punctuation(delimiterText);
      if (delimiter.IsEmpty)
        delimiter.Char = '\t';

      var escape = new Punctuation(escapeText);
      m_DelimiterRegex = new Regex(escape.IsEmpty ? $"\\{delimiter.Char}" : $"(?<!\\{escape.Char})\\{delimiter.Char}",
        RegexOptions.Singleline | RegexOptions.Compiled);

      if (!qualifier.IsEmpty)
      {
        m_QuoteRegex = new Regex(
          escape.IsEmpty
            ? $"\\{qualifier.Char}((?:\\{qualifier.Char}\\{qualifier.Char}|(?:(?!\\{qualifier.Char})).)*)\\{qualifier.Char}"
            : $"\\{qualifier.Char}((?:\\{escape.Char}\\{qualifier.Char}|\\{qualifier.Char}\\{qualifier.Char}|(?:(?!\\{qualifier.Char})).)*)\\{qualifier.Char}",
          RegexOptions.Multiline | RegexOptions.Compiled);
      }

      if (!string.IsNullOrEmpty(comment))
        m_CommentRegex = new Regex($"\\s*{comment}.*$", RegexOptions.Multiline | RegexOptions.Compiled);
    }

    // ReSharper disable once RedundantNameQualifier
    public override void Highlight(FastColoredTextBoxNS.Range range)
    {
      range.ClearStyle(StyleIndex.All);
      range.SetStyle(BlueStyle, m_DelimiterRegex);

      if (m_QuoteRegex != null)
        range.SetStyle(MagentaStyle, m_QuoteRegex);

      if (m_CommentRegex != null)
        range.SetStyle(GrayStyle, m_CommentRegex);

      range.SetStyle(m_Space, m_SpaceRegex);
      // range.SetStyle(m_Tab, m_TabRegex1);
      range.SetStyle(m_Tab2, m_TabRegex2);
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
#pragma warning disable CA1416
      // ReSharper disable once RedundantNameQualifier
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
#pragma warning restore CA1416
    internal class SyntaxHighlightStyleTab : Style
    {
      private readonly Brush m_BackGround;
      private readonly Pen m_ForeGround;

      public SyntaxHighlightStyleTab(Pen foreGround, Brush backGround)
      {
        m_ForeGround = foreGround;
        m_BackGround = backGround;
      }

#pragma warning disable CA1416
      // ReSharper disable once RedundantNameQualifier
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
          var point2 = new Point((rect2.X + sizeChar) - 2, rect2.Y + (height / 2) - 1);

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
#pragma warning restore CA1416
    }
  }
}