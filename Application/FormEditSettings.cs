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
using System.Drawing;
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
    private IFileSettingPhysicalFile? m_FileSetting;

    public IFileSettingPhysicalFile? FileSetting
    {
      get => m_FileSetting;
      private set
      {
        if (value != null)
        {
          m_FileSetting = value;
          if (m_FileSetting is ICsvFile csvFile)
          {
            bindingSourceCsvFile.DataSource = csvFile;
            cboRecordDelimiter.SelectedValue = (int) csvFile.NewLine;
            quotingControl.CsvFile = csvFile;
          }
        }
      }
    }

    private bool m_IsDisposed;

    public FormEditSettings(in ViewSettings viewSettings, in IFileSettingPhysicalFile? setting)
    {
      m_ViewSettings = viewSettings ?? throw new ArgumentNullException(nameof(viewSettings));
      m_ViewSettings.PropertyChanged += M_ViewSettings_PropertyChanged;
      m_FileSetting = setting;

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

      toolTip.SetToolTip(checkBoxAllowRowCombining,
        @"Try to combine rows, it can happen if the column does contain a linefeed and is not properly quoted. 
That column content is moved to the next line.
Note: This does not work if it the issue is in the last column. The extra text of the columns flows into the next row, it cannot be recognized at the time the record is read. As the parser is working as a stream and can not go back it cannot be rectified. 
This is a very risky option, in some cases rows might be lost.");

      toolTip.SetToolTip(checkBoxTryToSolveMoreColumns,
        @"Try to realign columns in case the file is not quoted, and an extra delimiter has caused additional columns.
Re-Aligning works best if columns and their order are easily identifiable, if the columns are very similar e.g., all are text, or all are empty there is a high chance the realignment does fail.");
    }

    private void SetFont()
    {
      ChangeFont(new Font(m_ViewSettings.Font, m_ViewSettings.FontSize));
    }

    private void M_ViewSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(ViewSettings.Font) || e.PropertyName == nameof(ViewSettings.FontSize))
        SetFont();
    }

    private async void BtnOpenFile_Click(object? sender, EventArgs e)
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

        if (m_FileSetting == null)
        {
          using var formProgress = new FormProgress("Examining file", false, m_CancellationTokenSource.Token);
          formProgress.Maximum = 0;
          formProgress.Show(this);

          FileSetting = (await newFileName.AnalyzeFileAsync(m_ViewSettings.AllowJson,
            m_ViewSettings.GuessCodePage,
            m_ViewSettings.GuessDelimiter, m_ViewSettings.GuessQualifier, m_ViewSettings.GuessStartRow,
            m_ViewSettings.GuessHasHeader, m_ViewSettings.GuessNewLine, m_ViewSettings.GuessComment,
            m_ViewSettings.FillGuessSettings, formProgress.CancellationToken)).PhysicalFile();

          formProgress.Close();
        }

        if (m_FileSetting != null)
          m_FileSetting.FileName = newFileName;
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
      if (m_FileSetting is ICsvFile csvFile)
        await buttonGuessCP.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          var (codepage, bom) = await improvedStream.GuessCodePage(m_CancellationTokenSource.Token);
          csvFile.CodePageId = codepage;
          csvFile.ByteOrderMark = bom;
        });
      bindingSourceViewSetting.ResetBindings(false);
    }

    private async void ButtonGuessDelimiter_ClickAsync(object? sender, EventArgs e)
    {
      if (m_FileSetting is ICsvFile csvFile)
        await buttonGuessDelimiter.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          var res = await improvedStream.GuessDelimiter(csvFile.CodePageId, csvFile.SkipRows,
            csvFile.EscapePrefix,
            m_CancellationTokenSource.Token);
          if (res.Item2)
            csvFile.FieldDelimiter = res.Item1;
        });
    }

    private async void ButtonGuessTextQualifier_Click(object? sender, EventArgs e)
    {
      var qualifier = string.Empty;
      if (m_FileSetting is ICsvFile csvFile)
      {
        await buttonGuessTextQualifier.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          qualifier = await improvedStream.GuessQualifier(csvFile.CodePageId, csvFile.SkipRows,
            csvFile.FieldDelimiter, csvFile.EscapePrefix,
            m_CancellationTokenSource.Token);
        });

        csvFile.FieldQualifier = qualifier;
      }
    }

    private async void ButtonSkipLine_ClickAsync(object? sender, EventArgs e)
    {
      if (m_FileSetting is ICsvFile csvFile)
        await buttonSkipLine.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          csvFile.SkipRows = await improvedStream.GuessStartRow(csvFile.CodePageId, csvFile.FieldDelimiter,
            csvFile.FieldQualifier, csvFile.CommentLine, m_CancellationTokenSource.Token);
        });
    }

    private void CboCodePage_SelectedIndexChanged(object? sender, EventArgs e)
    {
      if (m_FileSetting is ICsvFile csvFile)
        if (cboCodePage.SelectedItem != null)
          csvFile.CodePageId = ((DisplayItem<int>) cboCodePage.SelectedItem).ID;
    }

    private void CboRecordDelimiter_SelectedIndexChanged(object? sender, EventArgs e)
    {
      if (m_FileSetting is ICsvFile csvFile)
        if (cboRecordDelimiter.SelectedValue is RecordDelimiterTypeEnum val)
          csvFile.NewLine = val;
    }

    private void CheckBoxColumnsProcess_CheckedChanged(object? sender, EventArgs e)
    {
      if (m_FileSetting is ICsvFile csvFile)
        if (csvFile.TryToSolveMoreColumns || csvFile.AllowRowCombining)
          csvFile.WarnEmptyTailingColumns = true;
    }

    /// <summary>
    ///   Handles the Load event of the EditSettings control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void EditSettings_Load(object? sender, EventArgs e)
    {
      bindingSourceViewSetting.DataSource = m_ViewSettings;
      fillGuessSettingEdit.FillGuessSettings = m_ViewSettings.FillGuessSettings;

      cboCodePage.SuspendLayout();
      cboCodePage.DataSource = EncodingHelper.CommonCodePages
        .Select(cp => new DisplayItem<int>(cp, EncodingHelper.GetEncodingName(cp, false))).ToList();
      cboCodePage.ResumeLayout(true);

      var descConv = new EnumDescriptionConverter(typeof(RecordDelimiterTypeEnum));
      var di = (from RecordDelimiterTypeEnum item in Enum.GetValues(typeof(RecordDelimiterTypeEnum))
        select new DisplayItem<int>((int) item, descConv?.ConvertToString(item) ?? string.Empty)).ToList();


      cboRecordDelimiter.SuspendLayout();
      cboRecordDelimiter.DataSource = di;
      cboRecordDelimiter.DisplayMember = nameof(DisplayItem<int>.Display);
      cboRecordDelimiter.ValueMember = nameof(DisplayItem<int>.ID);
      cboRecordDelimiter.ResumeLayout(true);


      SetFont();

      FileSetting = m_FileSetting;
    }

    private void FormEditSettings_FormClosing(object? sender, FormClosingEventArgs e)
    {
      m_CancellationTokenSource.Cancel();
      ValidateChildren();
    }

    private async void GuessNewline_Click(object? sender, EventArgs e)
    {
      if (m_FileSetting is ICsvFile csvFile)
        await buttonNewLine.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          cboRecordDelimiter.SelectedValue = (int) await improvedStream.GuessNewline(csvFile.CodePageId,
            csvFile.SkipRows,
            csvFile.FieldQualifier, m_CancellationTokenSource.Token);
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
      if (m_FileSetting is ICsvFile csvFile)
        await buttonGuessHeader.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          var res = await improvedStream.GuessHasHeader(csvFile.CodePageId, csvFile.SkipRows, csvFile.CommentLine,
            csvFile.FieldDelimiter, m_CancellationTokenSource.Token);
          csvFile.HasFieldHeader = string.IsNullOrEmpty(res);
          bindingSourceViewSetting.ResetBindings(false);
          MessageBox.Show(res, "Checking headers");
        });
    }

    private void ButtonInteractiveSettings_Click(object? sender, EventArgs e)
    {
      if (m_FileSetting is ICsvFile csvFile)
      {
        using var frm = new FindSkipRows(csvFile);
        _ = frm.ShowDialog();
      }
    }

    private async void ButtonGuessLineComment_Click(object? sender, EventArgs e)
    {
      if (m_FileSetting is ICsvFile csvFile)
        await buttonGuessLineComment.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          csvFile.CommentLine = await improvedStream.GuessLineComment(csvFile.CodePageId, csvFile.SkipRows,
            m_CancellationTokenSource.Token);
        });
    }

    private void selectFont_ValueChanged(object sender, EventArgs e)
    {
      m_ViewSettings.Font = selectFont.Font;
      m_ViewSettings.FontSize = selectFont.FontSize;
    }
  }
}