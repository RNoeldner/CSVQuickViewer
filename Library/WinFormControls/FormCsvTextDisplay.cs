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
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;

namespace CsvTools
{
  /// <summary>
  ///   UserControl: CsvTextDisplay
  /// </summary>
  public partial class FormCsvTextDisplay : ResizeForm
  {
    private int m_CodePage;
    private readonly string m_FullPath;
    private ISyntaxHighlighter? m_HighLighter;
    private int m_SkipLines;

    /// <summary>
    ///   CTOR CsvTextDisplay
    /// </summary>
    public FormCsvTextDisplay(in string fullPath)
    {
      m_FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
      InitializeComponent();
      base.Text = FileSystemUtils.GetShortDisplayFileName(m_FullPath);
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

    private async Task<StringBuilder> SourceText(IProgress<ProgressInfo> formProgress,
      CancellationToken cancellationToken)
    {
      formProgress.Report(new ProgressInfo("Accessing source file"));
      var sa = new SourceAccess(m_FullPath);
#if NET5_0_OR_GREATER
      await
#endif
      // ReSharper disable once UseAwaitUsing
      using var stream = FunctionalDI.OpenStream(sa);
      using var textReader = new StreamReader(stream, Encoding.GetEncoding(m_CodePage), true, 4096, false);

      var sb = new StringBuilder();
      char[] buffer = ArrayPool<char>.Shared.Rent(64000);
      int len;
      formProgress.SetMaximum(1000);
      while ((len = await textReader.ReadBlockAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) != 0)
      {
        cancellationToken.ThrowIfCancellationRequested();
        sb.Append(buffer, 0, len);
        formProgress.Report(new ProgressInfo($"Reading source {stream.Position:N0}",
        // ReSharper disable once AccessToDisposedClosure
        (stream is IImprovedStream impStream) ? Convert.ToInt64(impStream.Percentage * 1000) : -1));

      }

      formProgress.SetMaximum(0);
      formProgress.Report(new ProgressInfo($"Finished reading file"));

      return sb;
    }

    private async Task OriginalStream(CancellationToken cancellationToken)
    {
      await textBox.RunWithHourglassAsync(async () =>
      {
        using var formProgress = new FormProgress("Display Source", false, cancellationToken);
        formProgress.ShowWithFont(this);
        var sb = await SourceText(formProgress, formProgress.CancellationToken);
        textBox.ClearUndo();
        formProgress.Report(new ProgressInfo("Display of read file"));
        textBox.Text = sb.ToString();
        textBox.IsChanged = false;
        formProgress.Maximum = 0;
        formProgress.Report(new ProgressInfo("Applying color coding"));
        HighlightVisibleRange();
        prettyPrintJsonToolStripMenuItem.Checked = false;
        originalFileToolStripMenuItem.Checked = true;
      });
    }

    private void PrettyPrintStream(CancellationToken cancellationToken)
    {
      textBox.RunWithHourglass(() =>
      {
        using var formProgress = new FormProgress("Pretty Print Source", false, cancellationToken);
        formProgress.ShowWithFont(this);
        formProgress.Maximum = 0;
        formProgress.Report(new ProgressInfo("Parsing Text as Json"));
        var t = JsonConvert.DeserializeObject<object>(textBox.Text);
        formProgress.Report(new ProgressInfo("Indenting Json"));
        textBox.ClearUndo();
        textBox.Text = JsonConvert.SerializeObject(t, Formatting.Indented);
        textBox.IsChanged = false;
        formProgress.Report(new ProgressInfo("Applying color coding"));
        HighlightVisibleRange();
        prettyPrintJsonToolStripMenuItem.Checked = true;
        originalFileToolStripMenuItem.Checked = false;
      });
    }

    /// <summary>
    ///   CSV File to display
    /// </summary>
    public async Task OpenFileAsync(bool json, string qualifier, string delimiter, string escape, int codePage,
      int skipLines, string comment, CancellationToken cancellationToken)
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

          await OriginalStream(cancellationToken);
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

    private void PrettyPrintJsonToolStripMenuItem_Click(object? sender, EventArgs e) =>
      PrettyPrintStream(CancellationToken.None);

    private async void OriginalFileToolStripMenuItem_Click(object? sender, EventArgs e) =>
      await OriginalStream(CancellationToken.None);
  }
}