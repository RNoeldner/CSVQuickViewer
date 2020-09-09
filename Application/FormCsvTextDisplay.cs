/*
 * Copyright (C) 2014 Raphael Nöldner
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
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;
using JetBrains.Annotations;

namespace CsvTools
{
  /// <summary>
  ///   UserControl: CsvTextDisplay
  /// </summary>
  public partial class FormCsvTextDisplay : ResizeForm
  {
    private readonly Style m_Space;
    private readonly Style m_Tab;
    private readonly Style m_BrownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Italic);
    private readonly Style m_BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
    private readonly Style m_GrayStyle = new TextStyle(Brushes.AntiqueWhite, Brushes.LightGray, FontStyle.Regular);
    private readonly Style m_MagentaStyle = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);


    private readonly Regex m_JsonStringRegex = new Regex(@"""([^\\""]|\\"")*""", RegexOptions.Compiled);
    private readonly Regex m_JsonNumberRegex = new Regex(@"\b(\d+[\.]?\d*|true|false|null)\b", RegexOptions.Compiled);
    private readonly Regex m_JsonKeywordRegex = new Regex(@"(?<range>""([^\\""]|\\"")*"")\s*:", RegexOptions.Compiled);
    private int m_SkipLines;
    private bool m_Json;
    private char m_Delimiter = ',';
    private bool m_DisplaySpace = true;
    private char m_Escape = '\\';
    private char m_Quote = '"';

    class SpaceStyle : Style
    {
      private readonly FastColoredTextBox m_TextBox;
      public SpaceStyle(FastColoredTextBox textBox)
      {
        m_TextBox = textBox;
      }
      public override void Draw(Graphics gr, Point position, Range range)
      {
        //get size of rectangle
        var size = GetSizeOfRange(range);
        var rect = new Rectangle(position, size);
        // background
        rect.Inflate(-1, -1);
        gr.FillRectangle(Brushes.AntiqueWhite, rect);

        var sizeChar = size.Width / (range.End.iChar - range.Start.iChar);
        var dotSize = new Size(Math.Min(Math.Max(sizeChar, 8), 3), Math.Min(Math.Max(size.Height, 8), 3));

        var posDot = new Point(position.X + sizeChar/2 - dotSize.Width/2, position.Y + size.Height / 2 - dotSize.Height / 2);
        for (var pos = range.Start.iChar; pos < range.End.iChar; pos++)
        {
          // draw a dot
          gr.FillEllipse(Brushes.Blue, new Rectangle(posDot, dotSize));
          posDot.X += sizeChar;
        }
      }
    }
    class TabType : Style
    {
      private readonly FastColoredTextBox m_TextBox;

      public TabType(FastColoredTextBox textBox)
      {
        m_TextBox = textBox;
      }

      public override void Draw(Graphics gr, Point position, Range range)
      {
        //get size of rectangle
        var size = GetSizeOfRange(range);
        var rect = new Rectangle(position, size);
        rect.Inflate(-1, -1);
        gr.FillRectangle(Brushes.AntiqueWhite, rect);
        var sizeChar = size.Width / (range.End.iChar - range.Start.iChar);
        var height = size.Height;

        for (var pos = range.Start.iChar; pos < range.End.iChar; pos++)
        {
          var rect2 = new Rectangle(position, new Size(sizeChar, height));

          // draw an arrow
          var point2 = new Point(rect2.X + sizeChar -2, rect2.Y +height / 2);

          gr.DrawLine(Pens.Blue, new Point(rect2.X + 1, point2.Y), point2);
          gr.DrawLine(Pens.Blue, new Point(rect2.X + sizeChar /2, rect2.Y +height/4), point2);
          gr.DrawLine(Pens.Blue, new Point(rect2.X + sizeChar /2, rect2.Y + (rect2.Height*3)/4), point2);
          
          // double line in case its larger
          if (height>6)
            gr.DrawLine(Pens.Blue, rect2.X + 1 , point2.Y+1, point2.X, point2.Y+1);

          if (sizeChar > 6)
          {
            gr.DrawLine(Pens.Blue, rect2.X + sizeChar / 2 + 1, rect2.Y + height / 4, point2.X + 1, point2.Y);
            gr.DrawLine(Pens.Blue, rect2.X + sizeChar / 2 + 1, rect2.Y + (rect2.Height * 3) / 4, point2.X + 1,
              point2.Y);
          }
          position.X += sizeChar;
        }
      }
    }
    private void DelimiterHighlight(Range range)
    {
      range.ClearStyle(StyleIndex.All);
      range.SetStyle(m_BlueStyle, $"(?<!\\{ m_Escape})\\{m_Delimiter}");
      range.SetStyle(m_MagentaStyle, $"(?<!(\\{ m_Escape}|\\{m_Quote}))\\{m_Quote}");

      if (m_DisplaySpace)
      {
        range.SetStyle(m_Space, " ");
        range.SetStyle(m_Tab, "\\t");
      }
    }

    private void JSONSyntaxHighlight(Range range)
    {
      range.tb.LeftBracket = '[';
      range.tb.RightBracket = ']';
      range.tb.LeftBracket2 = '{';
      range.tb.RightBracket2 = '}';
      range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy2;

      range.tb.AutoIndentCharsPatterns = @"
^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>[^;]+);
";

      //clear style of changed range
      range.ClearStyle(StyleIndex.All);

      //keyword highlighting
      range.SetStyle(m_BlueStyle, m_JsonKeywordRegex);
      //string highlighting
      range.SetStyle(m_BrownStyle, m_JsonStringRegex);
      //number highlighting
      range.SetStyle(m_MagentaStyle, m_JsonNumberRegex);
      //clear folding markers
      range.ClearFoldingMarkers();
      //set folding markers
      range.SetFoldingMarkers("{", "}"); //allow to collapse brackets block
      range.SetFoldingMarkers(@"\[", @"\]"); //allow to collapse comment block
    }

    private void HighlightVisibleRange()
    {
      //expand visible range (+- margin)
      var startLine = Math.Max(m_SkipLines, textBox.VisibleRange.Start.iLine - 20);
      var endLine = Math.Min(textBox.LinesCount - 1, textBox.VisibleRange.End.iLine + 100);
      var range = new Range(textBox, 0, startLine, 0, endLine);
      if (m_Json)
        JSONSyntaxHighlight(range);
      else
        DelimiterHighlight(range);

      if (m_SkipLines <= 0) return;
      range = new Range(textBox, 0, 0, 0, m_SkipLines);
      range.ClearStyle(m_BlueStyle, m_BrownStyle, m_MagentaStyle);
      range.SetStyle(m_GrayStyle);
    }


    /// <summary>
    ///   CTOR CsvTextDisplay
    /// </summary>
    public FormCsvTextDisplay()
    {
      InitializeComponent();
      m_Space = new SpaceStyle(textBox);
      m_Tab = new TabType(textBox);
    }

    /// <summary>
    ///   CSV File to display
    /// </summary>
    public void OpenFile([NotNull] string fullPath, bool json, char qualifierChar, char delimiterChar,
      char escapeChar,
      int codePage, int skipLines)
    {
      Text = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
      var info = new FileSystemUtils.FileInfo(fullPath);
      if (!info.Exists)
      {
        textBox.Text = $@"
The file {fullPath} does not exist.";
      }
      else
      {
        m_Json = json;
        m_DisplaySpace = true;
        m_Quote = qualifierChar;
        m_Delimiter = delimiterChar;
        m_Escape = escapeChar;
        m_SkipLines = skipLines;
        textBox.OpenBindingFile(fullPath, Encoding.GetEncoding(codePage));
      }
    }

    private void textBox_TextChangedDelayed(object sender, TextChangedEventArgs e)
    {
      HighlightVisibleRange();
    }

    private void textBox_VisibleRangeChangedDelayed(object sender, EventArgs e)
    {
      HighlightVisibleRange();
    }
  }
}