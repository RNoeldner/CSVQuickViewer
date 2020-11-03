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
using System.IO;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CsvTools
{
  /// <summary>
  ///   UserControl: CsvTextDisplay
  /// </summary>
  public partial class FormCsvTextDisplay : ResizeForm
  {
    private int m_CodePage;
    private string m_FullPath;
    private ISyntaxHighlighter m_HighLighter;
    private MemoryStream m_MemoryStream;
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
      try
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
      catch (Exception ex)
      {
        Logger.Warning(ex, "FormCsvTextDisplay.HighlightVisibleRange");
      }
    }

    private void OriginalStream()
    {
      try
      {
        m_MemoryStream?.Dispose();
        m_MemoryStream = null;
        m_Stream = new ImprovedStream(new SourceAccess(m_FullPath, true));
        textBox.OpenBindingStream(m_Stream as Stream, Encoding.GetEncoding(m_CodePage));
        HighlightVisibleRange();
        prettyPrintJsonToolStripMenuItem.Checked = false;
        originalFileToolStripMenuItem.Checked = true;
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Original File");
      }
    }

    private void PrettyPrintStream()
    {
      try
      {
        m_Stream.Seek(0, SeekOrigin.Begin);
        m_MemoryStream?.Dispose();
        m_MemoryStream = new MemoryStream();

        var encoding = Encoding.GetEncoding(m_CodePage);
        using (var textReader = new StreamReader((Stream) m_Stream, encoding, true, 4096, true))
        using (var stringWriter = new StreamWriter(m_MemoryStream, encoding, 4096, true))
        {
          var jsonReader = new JsonTextReader(textReader);
          var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
          jsonWriter.WriteToken(jsonReader);
        }

        m_MemoryStream.Seek(0, SeekOrigin.Begin);
        textBox.OpenBindingStream(m_MemoryStream, encoding);
        HighlightVisibleRange();
        prettyPrintJsonToolStripMenuItem.Checked = true;
        originalFileToolStripMenuItem.Checked = false;
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Pretty Print");
      }
    }

    /// <summary>
    ///   CSV File to display
    /// </summary>
    public void OpenFile([NotNull] string fullPath, bool json, char qualifierChar, char delimiterChar,
      char escapeChar,
      int codePage, int skipLines, string comment)
    {
      if (!FileSystemUtils.FileExists(fullPath ?? throw new ArgumentNullException(nameof(fullPath))))
      {
        textBox.Text = $"\nThe file '{fullPath}' does not exist.";
      }
      else
      {
        Text   = FileSystemUtils.GetShortDisplayFileName(fullPath, 80);
        m_FullPath = fullPath;

        try
        {
          if (json)
          {
            m_HighLighter = new SyntaxHighlighterJson(textBox);
            textBox.ContextMenuStrip = contextMenuJson;
          }
          else
            m_HighLighter =
              new SyntaxHighlighterDelimitedText(textBox, qualifierChar, delimiterChar, escapeChar, comment);

          m_Stream = new ImprovedStream(new SourceAccess(fullPath, true));
          m_SkipLines = !json ? skipLines : 0;
          m_CodePage = codePage;

          OriginalStream();
        }
        catch (Exception e)
        {
          textBox.Text = e.Message;
        }
      }
    }

    private void TextBox_TextChangedDelayed(object sender, FastColoredTextBoxNS.TextChangedEventArgs e) => HighlightVisibleRange();

    private void TextBox_VisibleRangeChangedDelayed(object sender, EventArgs e) => HighlightVisibleRange();

    private void PrettyPrintJsonToolStripMenuItem_Click(object sender, EventArgs e) => PrettyPrintStream();

    private void OriginalFileToolStripMenuItem_Click(object sender, EventArgs e) => OriginalStream();
  }
}