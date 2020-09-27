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
using System.Text;
using JetBrains.Annotations;

namespace CsvTools
{
  /// <summary>
  ///   UserControl: CsvTextDisplay
  /// </summary>
  public partial class FormCsvTextDisplay : ResizeForm
  {
    private ISyntaxHighlighter m_HighLighter;
    private int m_SkipLines;
    private IImprovedStream m_Stream;

    /// <summary>
    ///   CTOR CsvTextDisplay
    /// </summary>
    public FormCsvTextDisplay()
    {
      InitializeComponent();
    }

    private void HighlightVisibleRange()
    {
      //expand visible range (+- margin)
      var startLine = Math.Max(m_SkipLines, textBox.VisibleRange.Start.iLine - 20);
      var endLine = Math.Min(textBox.LinesCount - 1, textBox.VisibleRange.End.iLine + 100);
      if (startLine < endLine)
      {
        var range = new FastColoredTextBoxNS.Range(textBox, 0, startLine, 0, endLine);
        m_HighLighter.Highlight(range);

        if (m_SkipLines <= 0) return;
        range = new FastColoredTextBoxNS.Range(textBox, 0, 0, 0, m_SkipLines);
        m_HighLighter.Comment(range);
      }
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
        if (json)
          m_HighLighter = new SyntaxHighlighterJson(textBox);
        else
          m_HighLighter =
            new SyntaxHighlighterDelimitedText(textBox, qualifierChar, delimiterChar, escapeChar, commemt);
        m_Stream = ImprovedStream.OpenRead(fullPath);

        m_SkipLines = skipLines;
        textBox.OpenBindingStream(m_Stream.Stream, Encoding.GetEncoding(codePage));
      }
    }

    private void TextBox_TextChangedDelayed(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
    {
      HighlightVisibleRange();
    }

    private void TextBox_VisibleRangeChangedDelayed(object sender, EventArgs e)
    {
      HighlightVisibleRange();
    }
  }
}