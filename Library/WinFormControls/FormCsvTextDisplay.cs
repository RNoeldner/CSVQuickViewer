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

#nullable enable
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace CsvTools
{
  /// <summary>
  ///   UserControl: CsvTextDisplay
  /// </summary>
  public partial class FormCsvTextDisplay : ResizeForm
  {
    private int m_CodePage;
    private readonly string m_FullPath;
    private readonly bool m_IsFile;
    private ISyntaxHighlighter? m_HighLighter;
    private MemoryStream? m_MemoryStream;
    private int m_SkipLines;
    private Stream? m_Stream;


    /// <summary>
    ///   CTOR CsvTextDisplay
    /// </summary>
    public FormCsvTextDisplay(string fullPath, bool isFile)
    {
      m_FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
      InitializeComponent();
      m_IsFile = isFile;
      base.Text = m_IsFile ? FileSystemUtils.GetShortDisplayFileName(m_FullPath) : string.Empty;
    }

    private void HighlightVisibleRange()
    {
      try
      {
        //expand visible range (+- margin)
        var startLine = Math.Max(m_SkipLines, textBox.VisibleRange.Start.iLine - 20);
        var endLine = Math.Min(textBox.LinesCount - 1, textBox.VisibleRange.End.iLine + 100);
        if (startLine >= endLine) return;
        var range = new FastColoredTextBoxNS.Range(textBox, 0, startLine, 0, endLine);
        m_HighLighter?.Highlight(range);
        m_HighLighter?.SkipRows(m_SkipLines);
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "FormCsvTextDisplay.HighlightVisibleRange");
      }
    }

    private void OriginalStream()
    {
      m_MemoryStream?.Dispose();
      m_MemoryStream = null;
      if (m_IsFile)
      {
        var sa = new SourceAccess(m_FullPath);
        m_Stream = new ImprovedStream(sa);
        if (sa.FileType == FileTypeEnum.Zip || sa.FileType == FileTypeEnum.Pgp)
        {
          var encoding = Encoding.GetEncoding(m_CodePage);
          using var textReader = new StreamReader(m_Stream, encoding, true, 4096, true);
          textBox.Text = textReader.ReadToEnd();
        }
        else
        {
          textBox.OpenBindingStream(m_Stream, Encoding.GetEncoding(m_CodePage));
        }
      }

      HighlightVisibleRange();
      prettyPrintJsonToolStripMenuItem.Checked = false;
      originalFileToolStripMenuItem.Checked = true;
    }

    private void PrettyPrintStream()
    {
      if (m_Stream is null)
        return;
      try
      {
        m_Stream.Seek(0, SeekOrigin.Begin);
        m_MemoryStream?.Dispose();
        m_MemoryStream = new MemoryStream();

        var encoding = Encoding.GetEncoding(m_CodePage);
        using var textReader = new StreamReader(m_Stream, encoding, true, 4096, true);
        using var stringWriter = new StreamWriter(m_MemoryStream, encoding, 4096, true);
        var jsonReader = new JsonTextReader(textReader);
        var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
        jsonWriter.WriteToken(jsonReader);

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
    public void OpenFile(bool json, string qualifier, string delimiter, string escape, int codePage, int skipLines,
      string comment)
    {
      if (m_HighLighter is IDisposable disposable)
        disposable.Dispose();

      if (json)
      {
        m_HighLighter = new SyntaxHighlighterJson(textBox);
        textBox.ContextMenuStrip = contextMenuJson;
      }
      else
        m_HighLighter = new SyntaxHighlighterDelimitedText(textBox, qualifier, delimiter, escape, comment);

      if (!m_IsFile)
      {
        textBox.Text = m_FullPath;
        return;
      }

      if (!FileSystemUtils.FileExists(m_FullPath))
      {
        textBox.Text = $@"
The file '{m_FullPath}' does not exist.";
      }
      else
      {
        try
        {
          m_SkipLines = !json ? skipLines : 0;
          m_CodePage = codePage;

          OriginalStream();
        }
        catch (Exception ex)
        {
          m_HighLighter = null;
          textBox.Text = $@"Issue opening the file {m_FullPath} for display:


{ex.Message}";
        }
      }
    }

    private void TextBox_TextChangedDelayed(object? sender, FastColoredTextBoxNS.TextChangedEventArgs e) =>
      HighlightVisibleRange();

    private void TextBox_VisibleRangeChangedDelayed(object? sender, EventArgs e) => HighlightVisibleRange();

    private void PrettyPrintJsonToolStripMenuItem_Click(object? sender, EventArgs e) => PrettyPrintStream();

    private void OriginalFileToolStripMenuItem_Click(object? sender, EventArgs e) => OriginalStream();
  }
}