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
    private readonly Style m_BlueStyle = new TextStyle(Brushes.Blue, null, FontStyle.Regular);
    private readonly Style m_BrownStyle = new TextStyle(Brushes.Brown, null, FontStyle.Italic);
    private readonly Style m_GrayStyle = new TextStyle(Brushes.AntiqueWhite, Brushes.LightGray, FontStyle.Regular);

    private readonly Regex m_JsonKeywordRegex = new Regex(@"(?<range>""([^\\""]|\\"")*"")\s*:",
      RegexOptions.Singleline | RegexOptions.Compiled);

    private readonly Regex m_JsonNumberRegex = new Regex(@"\b(\d+[\.]?\d*|true|false|null)\b",
      RegexOptions.Singleline | RegexOptions.Compiled);

    private readonly Regex m_JsonStringRegex =
      new Regex(@"""([^\\""]|\\"")*""", RegexOptions.Singleline | RegexOptions.Compiled);

    private readonly Style m_MagentaStyle = new TextStyle(Brushes.Magenta, null, FontStyle.Regular);
    private readonly Style m_Space = new SpaceStyle(Brushes.Blue, Brushes.AntiqueWhite);
    private readonly Regex m_SpaceRegex = new Regex(" ", RegexOptions.Singleline | RegexOptions.Compiled);
    private readonly Style m_Tab = new TabStyle(Pens.Blue, Brushes.AntiqueWhite);
    private readonly Regex m_TabRegex = new Regex("\\t", RegexOptions.Singleline | RegexOptions.Compiled);
    private Regex m_CommentRegex = null;


    private Regex m_DelimiterRegex;
    private bool m_Json;
    private Regex m_QuoteRegex;
    private int m_SkipLines;


    /// <summary>
    ///   CTOR CsvTextDisplay
    /// </summary>
    public FormCsvTextDisplay()
    {
      InitializeComponent();
    }

    private void DelimiterHighlight(Range range)
    {
      range.ClearStyle(StyleIndex.All);
      range.SetStyle(m_BlueStyle, m_DelimiterRegex);
      range.SetStyle(m_MagentaStyle, m_QuoteRegex);
      if (m_CommentRegex != null)
        range.SetStyle(m_GrayStyle, m_CommentRegex);
      range.SetStyle(m_Space, m_SpaceRegex);
      range.SetStyle(m_Tab, m_TabRegex);
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
    ///   CSV File to display
    /// </summary>
    public void OpenFile([NotNull] string fullPath, bool json, char qualifierChar, char delimiterChar,
      char escapeChar,
      int codePage, int skipLines, string commemt)
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
        m_SkipLines = skipLines;
        m_DelimiterRegex = new Regex(
          (escapeChar != '\0') ? $"\\{delimiterChar}" : $"(?<!\\{escapeChar})\\{delimiterChar}",
          RegexOptions.Singleline | RegexOptions.Compiled);
        m_QuoteRegex = new Regex(
          (escapeChar != '\0')
            ? $"(?<!|\\{qualifierChar})\\{qualifierChar}"
            : $"(?<!(\\{escapeChar}|\\{qualifierChar}))\\{qualifierChar}",
          RegexOptions.Singleline | RegexOptions.Compiled);
        if (!string.IsNullOrEmpty(commemt))
          m_CommentRegex = new Regex($"\\s*{commemt}.*$", RegexOptions.Multiline | RegexOptions.Compiled);
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

    class SpaceStyle : Style
    {
      private readonly Brush m_BackGround;
      private readonly Brush m_ForeGround;

      public SpaceStyle(Brush foreGround, Brush backGround)
      {
        m_ForeGround = foreGround;
        m_BackGround = backGround;
      }

      public override void Draw(Graphics gr, Point position, Range range)
      {
        //get size of rectangle
        var size = GetSizeOfRange(range);
        var rect = new Rectangle(position, size);
        // background
        rect.Inflate(-1, -1);
        gr.FillRectangle(m_BackGround, rect);

        var sizeChar = size.Width / (range.End.iChar - range.Start.iChar);
        var dotSize = new Size(Math.Min(Math.Max(sizeChar, 8), 3), Math.Min(Math.Max(size.Height, 8), 3));

        var posDot = new Point(position.X + sizeChar / 2 - dotSize.Width / 2,
          position.Y + size.Height / 2 - dotSize.Height / 2);
        for (var pos = range.Start.iChar; pos < range.End.iChar; pos++)
        {
          // draw a dot
          gr.FillEllipse(m_ForeGround, new Rectangle(posDot, dotSize));
          posDot.X += sizeChar;
        }
      }
    }

    class TabStyle : Style
    {
      private readonly Brush m_BackGround;
      private readonly Pen m_ForeGround;

      public TabStyle(Pen foreGround, Brush backGround)
      {
        m_ForeGround = foreGround;
        m_BackGround = backGround;
      }

      public override void Draw(Graphics gr, Point position, Range range)
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
          var point2 = new Point(rect2.X + sizeChar - 2, rect2.Y + height / 2);

          gr.DrawLine(m_ForeGround, new Point(rect2.X + 1, point2.Y), point2);
          gr.DrawLine(m_ForeGround, new Point(rect2.X + sizeChar / 2, rect2.Y + height / 4), point2);
          gr.DrawLine(m_ForeGround, new Point(rect2.X + sizeChar / 2, rect2.Y + (rect2.Height * 3) / 4), point2);

          // double line in case its larger
          if (height > 6)
            gr.DrawLine(m_ForeGround, rect2.X + 1, point2.Y + 1, point2.X, point2.Y + 1);

          if (sizeChar > 6)
          {
            gr.DrawLine(m_ForeGround, rect2.X + sizeChar / 2 + 1, rect2.Y + height / 4, point2.X + 1, point2.Y);
            gr.DrawLine(m_ForeGround, rect2.X + sizeChar / 2 + 1, rect2.Y + (rect2.Height * 3) / 4, point2.X + 1,
              point2.Y);
          }

          position.X += sizeChar;
        }
      }
    }
  }
}