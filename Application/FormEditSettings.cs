/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com/
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
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Form to edit the Settings
  /// </summary>
  public partial class FormEditSettings : ResizeForm
  {
    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();
    private readonly ViewSettings m_ViewSettings;
    private readonly ICsvFile m_CsvFile;
    private bool m_IsDisposed;

    public FormEditSettings(in ViewSettings viewSettings, in ICsvFile csvFile)
    {
      m_ViewSettings = viewSettings ?? throw new ArgumentNullException(nameof(viewSettings));
      m_CsvFile = csvFile ?? throw new ArgumentNullException(nameof(csvFile));

      InitializeComponent();

      if (m_ViewSettings.LimitDuration == ViewSettings.Duration.Unlimited)
        domainUpDownLimit.SelectedIndex = 4;
      else if (m_ViewSettings.LimitDuration == ViewSettings.Duration.TenSecond)
        domainUpDownLimit.SelectedIndex = 3;
      else if (m_ViewSettings.LimitDuration == ViewSettings.Duration.TwoSecond)
        domainUpDownLimit.SelectedIndex = 2;
      else if (m_ViewSettings.LimitDuration == ViewSettings.Duration.Second)
        domainUpDownLimit.SelectedIndex = 1;
      else
        domainUpDownLimit.SelectedIndex = 0;
    }

    private void BtnOpenFile_Click(object? sender, EventArgs e)
    {
      try
      {
        var split = FileSystemUtils.SplitPath(textBoxFile.Text);
        var newFileName = WindowsAPICodePackWrapper.Open(
          split.DirectoryName,
          "Delimited File",
          "Delimited files (*.csv;*.txt;*.tab;*.tsv)|*.csv;*.txt;*.tab;*.tsv|All files (*.*)|*.*",
          split.FileName);

        if (newFileName is null || newFileName.Length == 0)
          return;
        m_CsvFile.FileName = newFileName;
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (m_IsDisposed) return;
      if (disposing)
      {
        m_IsDisposed = true;
        components?.Dispose();
        m_CancellationTokenSource.Cancel();
        m_CancellationTokenSource.Dispose();
      }

      base.Dispose(disposing);
    }

    private async void ButtonGuessCP_ClickAsync(object? sender, EventArgs e)
    {
      await buttonGuessCP.RunWithHourglassAsync(async () =>
      {
        using var improvedStream = new ImprovedStream(new SourceAccess(m_CsvFile));
        var (codepage, bom) = await improvedStream.GuessCodePage(m_CancellationTokenSource.Token);
        m_CsvFile.CodePageId = codepage;
        m_CsvFile.ByteOrderMark = bom;
      });
      bindingSourceViewSetting.ResetBindings(false);
    }

    private async void ButtonGuessDelimiter_ClickAsync(object? sender, EventArgs e)
    {
      await buttonGuessDelimiter.RunWithHourglassAsync(async () =>
      {
        using var improvedStream = new ImprovedStream(new SourceAccess(m_CsvFile));
        var res = await improvedStream.GuessDelimiter(m_CsvFile.CodePageId, m_CsvFile.SkipRows, m_CsvFile.EscapePrefix,
                    m_CancellationTokenSource.Token);
        if (res.Item2)
          m_CsvFile.FieldDelimiter = res.Item1;
      });
    }

    private async void ButtonGuessTextQualifier_Click(object? sender, EventArgs e)
    {
      var qualifier = string.Empty;
      await buttonGuessTextQualifier.RunWithHourglassAsync(async () =>
      {
        using var improvedStream = new ImprovedStream(new SourceAccess(m_CsvFile));
        qualifier = await improvedStream.GuessQualifier(m_CsvFile.CodePageId, m_CsvFile.SkipRows, m_CsvFile.FieldDelimiter, m_CsvFile.EscapePrefix,
                      m_CancellationTokenSource.Token);
      });

      m_CsvFile.FieldQualifier = qualifier;
    }

    private async void ButtonSkipLine_ClickAsync(object? sender, EventArgs e)
    {
      await buttonSkipLine.RunWithHourglassAsync(async () =>
      {
        using var improvedStream = new ImprovedStream(new SourceAccess(m_CsvFile));
        m_CsvFile.SkipRows = await improvedStream.GuessStartRow(m_CsvFile.CodePageId, m_CsvFile.FieldDelimiter, m_CsvFile.FieldQualifier, m_CsvFile.CommentLine, m_CancellationTokenSource.Token);
      });
    }

    private void CboCodePage_SelectedIndexChanged(object? sender, EventArgs e)
    {
      if (cboCodePage.SelectedItem != null)
        m_CsvFile.CodePageId = ((DisplayItem<int>) cboCodePage.SelectedItem).ID;
    }

    private void CboRecordDelimiter_SelectedIndexChanged(object? sender, EventArgs e)
    {
      if (cboRecordDelimiter.SelectedItem != null)
        m_CsvFile.NewLine = (RecordDelimiterTypeEnum) cboRecordDelimiter.SelectedValue;
    }

    private void CheckBoxColumnsProcess_CheckedChanged(object? sender, EventArgs e)
    {
      if (m_CsvFile.TryToSolveMoreColumns || m_CsvFile.AllowRowCombining)
        m_CsvFile.WarnEmptyTailingColumns = true;
    }

    /// <summary>
    ///   Handles the Load event of the EditSettings control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void EditSettings_Load(object? sender, EventArgs e)
    {
      bindingSourceViewSetting.DataSource = m_ViewSettings;
      bindingSourceCsvFile.DataSource = m_CsvFile;
      fillGuessSettingEdit.FillGuessSettings = m_ViewSettings.FillGuessSettings;

      // Fill Drop down
      cboCodePage.SuspendLayout();

      cboCodePage.DataSource = EncodingHelper.CommonCodePages.Select(cp => new DisplayItem<int>(cp, EncodingHelper.GetEncodingName(cp, false))).ToList();
      cboRecordDelimiter.DisplayMember = nameof(DisplayItem<int>.Display);
      cboRecordDelimiter.ValueMember = nameof(DisplayItem<int>.ID);
      cboRecordDelimiter.SelectedValue = m_CsvFile.CodePageId;
      cboCodePage.ResumeLayout(true);

      var descConv = new EnumDescriptionConverter(typeof(RecordDelimiterTypeEnum));
      var di = (from RecordDelimiterTypeEnum item in Enum.GetValues(typeof(RecordDelimiterTypeEnum))
                select new DisplayItem<int>((int) item, descConv.ConvertToString(item))).ToList();

      var selValue = (int) m_CsvFile.NewLine;
      cboRecordDelimiter.SuspendLayout();
      cboRecordDelimiter.DataSource = di;
      cboRecordDelimiter.DisplayMember = nameof(DisplayItem<int>.Display);
      cboRecordDelimiter.ValueMember = nameof(DisplayItem<int>.ID);
      cboRecordDelimiter.SelectedValue = selValue;
      cboRecordDelimiter.ResumeLayout(true);

      quotingControl.CsvFile = m_CsvFile;
    }

    private void FormEditSettings_FormClosing(object? sender, FormClosingEventArgs e)
    {
      m_CancellationTokenSource.Cancel();
      ValidateChildren();
    }

    private async void GuessNewline_Click(object? sender, EventArgs e)
    {
      await buttonNewLine.RunWithHourglassAsync(async () =>
      {
        using var improvedStream = new ImprovedStream(new SourceAccess(m_CsvFile));
        cboRecordDelimiter.SelectedValue = (int) await improvedStream.GuessNewline(m_CsvFile.CodePageId, m_CsvFile.SkipRows,
                                                   m_CsvFile.FieldQualifier, m_CancellationTokenSource.Token);
      });
    }

    private void TextBoxDelimiter_TextChanged(object? sender, EventArgs e)
    {
      if (string.IsNullOrWhiteSpace(textBoxDelimiter.Text))
      {
        errorProvider.SetError(textBoxDelimiter, "The delimiter must be set");
      }
      else
      {
        var delimiter = textBoxDelimiter.Text.WrittenPunctuationToChar();

        if (delimiter != ';' && delimiter != ',' && delimiter != '|' && delimiter != ':' && delimiter != '\t')
          errorProvider.SetError(textBoxDelimiter, "Unusual delimiter character");
        else
          errorProvider.SetError(textBoxDelimiter, string.Empty);
      }
    }

    private void TextBoxFile_Validating(object? sender, CancelEventArgs e)
    {
      if (!FileSystemUtils.FileExists(textBoxFile.Text))
      {
        errorProvider.SetError(textBoxFile, "File does not exist.");
        e.Cancel = true;
      }
      else
      {
        errorProvider.SetError(textBoxFile, string.Empty);
      }
    }

    private void DomainUpDownTime_SelectedItemChanged(object? sender, EventArgs e)
    {
      m_ViewSettings.LimitDuration = domainUpDownLimit.SelectedIndex switch
      {
        4 => ViewSettings.Duration.Unlimited,
        3 => ViewSettings.Duration.TenSecond,
        2 => ViewSettings.Duration.TwoSecond,
        1 => ViewSettings.Duration.Second,
        0 => ViewSettings.Duration.HalfSecond,
        _ => m_ViewSettings.LimitDuration
      };
    }

    private async void ButtonGuessHeader_Click(object? sender, EventArgs e)
    {
      await buttonGuessHeader.RunWithHourglassAsync(async () =>
      {
        using var improvedStream = new ImprovedStream(new SourceAccess(m_CsvFile));
        var res = await improvedStream.GuessHasHeader(m_CsvFile.CodePageId, m_CsvFile.SkipRows, m_CsvFile.CommentLine,
                    m_CsvFile.FieldDelimiter, m_CancellationTokenSource.Token);
        m_CsvFile.HasFieldHeader = string.IsNullOrEmpty(res);
        bindingSourceViewSetting.ResetBindings(false);
        MessageBox.Show(res, "Checking headers");
      });
    }

    private void ButtonInteractiveSettings_Click(object? sender, EventArgs e)
    {
      using var frm = new FindSkipRows(m_CsvFile);
      _ = frm.ShowDialog();
    }

    private async void ButtonGuessLineComment_Click(object? sender, EventArgs e)
    {
      await buttonGuessLineComment.RunWithHourglassAsync(async () =>
      {
        using var improvedStream = new ImprovedStream(new SourceAccess(m_CsvFile));
        m_CsvFile.CommentLine = await improvedStream.GuessLineComment(m_CsvFile.CodePageId, m_CsvFile.SkipRows, m_CancellationTokenSource.Token);
      });
    }
  }
}