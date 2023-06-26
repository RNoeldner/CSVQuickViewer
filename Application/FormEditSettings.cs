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
    private IFileSettingPhysicalFile? m_FileSetting;

    private void SetFileSetting(IFileSettingPhysicalFile fileSetting)
    {
      m_FileSetting = fileSetting;
      if (m_FileSetting is ICsvFile csvFile)
      {
        bindingSourceCsvFile.DataSource = csvFile;
        quotingControl.CsvFile = csvFile;
        textBoxFile_TextChanged(this, EventArgs.Empty);
      }
    }

    public IFileSettingPhysicalFile? FileSetting
    {
      get => m_FileSetting;
    }

    private bool m_IsDisposed;

    public FormEditSettings(in ViewSettings viewSettings, in IFileSettingPhysicalFile? setting)
    {
      m_ViewSettings = viewSettings ?? throw new ArgumentNullException(nameof(viewSettings));
      m_FileSetting = setting;
      FontConfig = viewSettings;
      InitializeComponent();

      toolTip.SetToolTip(checkBoxAllowRowCombining,
        @"Try to combine rows, it can happen if the column does contain a linefeed and is not properly quoted. 
That column content is moved to the next line.
Note: This does not work if it the issue is in the last column. The extra text of the columns flows into the next row, it cannot be recognized at the time the record is read. As the parser is working as a stream and can not go back it cannot be rectified. 
This is a very risky option, in some cases rows might be lost.");

      toolTip.SetToolTip(checkBoxTryToSolveMoreColumns,
        @"Try to realign columns in case the file is not quoted, and an extra delimiter has caused additional columns.
Re-Aligning works best if columns and their order are easily identifiable, if the columns are very similar e.g., all are text, or all are empty there is a high chance the realignment does fail.");
    }

    private void SetDefaultInspectionResult()
    {
      if (!m_ViewSettings.GuessCodePage)
      {
        if (cboCodePage.SelectedItem != null)
          m_ViewSettings.DefaultInspectionResult.CodePageId=  ((DisplayItem<int>) cboCodePage.SelectedItem).ID;
        m_ViewSettings.DefaultInspectionResult.ByteOrderMark = checkBoxBOM.Checked;
      }

      if (!m_ViewSettings.GuessEscapePrefix)
        m_ViewSettings.DefaultInspectionResult.EscapePrefix = textBoxEscapeRead.Character;

      if (!m_ViewSettings.GuessComment)
        m_ViewSettings.DefaultInspectionResult.CommentLine = textBoxComment.Text;

      if (!m_ViewSettings.GuessDelimiter)
        m_ViewSettings.DefaultInspectionResult.FieldDelimiter = textBoxDelimiter.Character;
      if (!m_ViewSettings.GuessHasHeader)
        m_ViewSettings.DefaultInspectionResult.HasFieldHeader = checkBoxHeader.Checked;

      if (!m_ViewSettings.GuessQualifier)
      {
        m_ViewSettings.DefaultInspectionResult.ContextSensitiveQualifier =  quotingControl.CsvFile.ContextSensitiveQualifier;
        m_ViewSettings.DefaultInspectionResult.DuplicateQualifierToEscape = quotingControl.CsvFile.DuplicateQualifierToEscape;
        m_ViewSettings.DefaultInspectionResult.FieldQualifier = quotingControl.CsvFile.FieldQualifierChar;
      }

      if (!m_ViewSettings.GuessStartRow)
        m_ViewSettings.DefaultInspectionResult.SkipRows = Convert.ToInt32(numericUpDownSkipRows.Value);
      // if this is not for a specific file store teh value in the defaults
      if (m_FileSetting != null)
        return;
      m_ViewSettings.KeyFileRead = textBoxKeyFileRead.Text;
      m_ViewSettings.KeyFileWrite = textBoxKeyFileWrite.Text;
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
          SetDefaultInspectionResult();
          using var formProgress = new FormProgress("Examining file", false, FontConfig, m_CancellationTokenSource.Token);
          formProgress.Maximum = 0;
          formProgress.Show(this);


          SetFileSetting((await newFileName.InspectFileAsync(m_ViewSettings.AllowJson,
            m_ViewSettings.GuessCodePage, m_ViewSettings.GuessEscapePrefix,
            m_ViewSettings.GuessDelimiter, m_ViewSettings.GuessQualifier, m_ViewSettings.GuessStartRow,
            m_ViewSettings.GuessHasHeader, m_ViewSettings.GuessNewLine, m_ViewSettings.GuessComment,
            m_ViewSettings.FillGuessSettings,
            list =>
            {
              if (list.Count==1)
                return list.First();
              using var frm = new FormSelectInDropdown(list, list.First(x => x.AssumeDelimited()));
              if (frm.ShowWithFont(this, true) == DialogResult.Cancel)
                throw new OperationCanceledException();
              return frm.SelectedText;
            }, m_ViewSettings.DefaultInspectionResult,
            PgpHelper.GetKeyAndValidate(newFileName, m_ViewSettings.KeyFileRead), formProgress.CancellationToken)).PhysicalFile());

          formProgress.Close();
        }

        if (m_FileSetting != null)
        {
          m_FileSetting.FileName = newFileName;
          m_ViewSettings.DeriveWriteSetting(m_FileSetting);
        }
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
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          var (codepage, bom) = await improvedStream.InspectCodePageAsync(m_CancellationTokenSource.Token);
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
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          using var textReader = await improvedStream.GetTextReaderAsync(csvFile.CodePageId, csvFile.SkipRows, m_CancellationTokenSource.Token);
          var res = await textReader.InspectDelimiterAsync(csvFile.FieldQualifierChar, csvFile.EscapePrefixChar, Array.Empty<char>(), m_CancellationTokenSource.Token);
          if (res.IsDetected)
            csvFile.FieldDelimiterChar = res.Delimiter;
        });
    }

    private async void ButtonGuessTextQualifier_Click(object? sender, EventArgs e)
    {
      if (m_FileSetting is ICsvFile csvFile)
      {
        await buttonGuessTextQualifier.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          using var textReader = await improvedStream.GetTextReaderAsync(csvFile.CodePageId, csvFile.SkipRows, m_CancellationTokenSource.Token);
          var res = textReader.InspectQualifier(csvFile.FieldDelimiterChar, csvFile.EscapePrefixChar, StaticCollections.PossibleQualifiers, m_CancellationTokenSource.Token);
          csvFile.FieldQualifierChar = res.QuoteChar;
          if (res.DuplicateQualifier)
            csvFile.DuplicateQualifierToEscape = res.DuplicateQualifier;
          if (!csvFile.ContextSensitiveQualifier)
            csvFile.ContextSensitiveQualifier = !(res.DuplicateQualifier || res.EscapedQualifier);
        });
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
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          using var textReader = await improvedStream.GetTextReaderAsync(csvFile.CodePageId, 0, m_CancellationTokenSource.Token);
          csvFile.SkipRows =textReader.InspectStartRow(csvFile.FieldDelimiterChar, csvFile.FieldQualifierChar, csvFile.EscapePrefixChar, csvFile.CommentLine, m_CancellationTokenSource.Token);
        });
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
      bindingSourceWrite.DataSource = m_ViewSettings.WriteSetting;

      cboCodePage.SuspendLayout();
      cboWriteCodePage.SuspendLayout();
      var codePages = EncodingHelper.CommonCodePages
        .Select(cp => new DisplayItem<int>(cp, EncodingHelper.GetEncodingName(cp))).ToList();
      var preselect = codePages.FirstOrDefault(x => x.ID == (m_FileSetting?.CodePageId ?? m_ViewSettings.DefaultInspectionResult.CodePageId)) ?? codePages.First();
      cboCodePage.DataSource = codePages;
      cboWriteCodePage.DataSource = codePages;
      cboCodePage.ResumeLayout(true);
      cboWriteCodePage.ResumeLayout(true);

      cboRecordDelimiter.SuspendLayout();
      if (m_FileSetting != null)
      {
        SetFileSetting(m_FileSetting);
      }
      else
      {
        if (!m_ViewSettings.GuessCodePage)
          checkBoxBOM.Checked = m_ViewSettings.DefaultInspectionResult.ByteOrderMark;

        if (!string.IsNullOrEmpty(m_ViewSettings.KeyFileRead))
          textBoxKeyFileRead.Text = m_ViewSettings.KeyFileRead;
        if (!string.IsNullOrEmpty(m_ViewSettings.KeyFileWrite))
          textBoxKeyFileWrite.Text = m_ViewSettings.KeyFileWrite;

        textBoxDelimiter.Character = m_ViewSettings.DefaultInspectionResult.FieldDelimiter;
        textBoxEscapeRead.Character = m_ViewSettings.DefaultInspectionResult.EscapePrefix;
        textBoxComment.Text = m_ViewSettings.DefaultInspectionResult.CommentLine;

        if (!m_ViewSettings.GuessHasHeader)
          checkBoxHeader.Checked = m_ViewSettings.DefaultInspectionResult.HasFieldHeader;

        if (!m_ViewSettings.GuessQualifier)
        {
          quotingControl.CsvFile.ContextSensitiveQualifier =
            m_ViewSettings.DefaultInspectionResult.ContextSensitiveQualifier;
          quotingControl.CsvFile.DuplicateQualifierToEscape =
            m_ViewSettings.DefaultInspectionResult.DuplicateQualifierToEscape;
          quotingControl.CsvFile.FieldQualifierChar = m_ViewSettings.DefaultInspectionResult.FieldQualifier;
        }
        numericUpDownSkipRows.Value = m_ViewSettings.DefaultInspectionResult.SkipRows;
      }
      cboCodePage.SelectedItem = preselect;
      cboRecordDelimiter.SetEnumDataSource(m_ViewSettings.WriteSetting.NewLine, new[] { RecordDelimiterTypeEnum.None });
      comboBoxLimitDuration.SetEnumDataSource(m_ViewSettings.LimitDuration);

      quotingControlWrite.CsvFile = m_ViewSettings.WriteSetting;
      quotingControlWrite.IsWriteSetting = true;
    }

    private void FormEditSettings_FormClosing(object? sender, FormClosingEventArgs e)
    {
      m_CancellationTokenSource.Cancel();
      int? codePageId = null;
      if (cboCodePage.SelectedItem != null)
        codePageId = ((DisplayItem<int>) cboCodePage.SelectedItem).ID;
      SetDefaultInspectionResult();
      // ReSharper disable once ConvertTypeCheckPatternToNullCheck
      if (m_FileSetting is IFileSettingPhysicalFile physicalFile && codePageId!=null)
        physicalFile.CodePageId = codePageId.Value;
      ValidateChildren();
      if (m_FileSetting != null)
        m_ViewSettings.PassOnConfiguration(m_FileSetting);
    }

    private async void GuessNewline_Click(object? sender, EventArgs e)
    {
      if (m_FileSetting is ICsvFile csvFile)
        await buttonNewLine.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          using var textReader = await improvedStream.GetTextReaderAsync(csvFile.CodePageId, csvFile.SkipRows, m_CancellationTokenSource.Token);
          cboRecordDelimiter.SelectedValue = textReader.InspectRecordDelimiter(csvFile.FieldQualifierChar, m_CancellationTokenSource.Token);
        });
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

    private async void ButtonGuessHeader_Click(object? sender, EventArgs e)
    {
      if (m_FileSetting is ICsvFile csvFile)
        await buttonGuessHeader.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          using var textReader = await improvedStream.GetTextReaderAsync(csvFile.CodePageId, csvFile.SkipRows, m_CancellationTokenSource.Token);
          var res = await textReader.InspectHasHeaderAsync(csvFile.FieldDelimiterChar, csvFile.FieldQualifierChar, csvFile.EscapePrefixChar, csvFile.CommentLine, m_CancellationTokenSource.Token);
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
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = new ImprovedStream(new SourceAccess(csvFile));
          using var textReader = await improvedStream.GetTextReaderAsync(csvFile.CodePageId, csvFile.SkipRows, m_CancellationTokenSource.Token);
          csvFile.CommentLine =  await textReader.InspectLineCommentAsync(m_CancellationTokenSource.Token);
        });
    }

    private void SelectFont_ValueChanged(object sender, EventArgs e)
    {
      m_ViewSettings.Font = selectFont.FontName;
      m_ViewSettings.FontSize = selectFont.FontSize;
    }

    private void checkBoxCopySkipped_MouseClick(object sender, MouseEventArgs e)
    {
      if (!checkBoxCopySkipped.Checked)
        m_ViewSettings.WriteSetting.SkipRows = 0;
    }

    private void numericUpDownSkipRows_ValueChanged(object sender, EventArgs e)
    {
      if (numericUpDownSkipRows.Value > 0)
        checkBoxCopySkipped.Checked = true;
      checkBoxCopySkipped.Enabled = (numericUpDownSkipRows.Value > 0);
    }

    private async void buttonEscapeSequence_Click(object sender, EventArgs e)
    {
      if (m_FileSetting is ICsvFile csvFile)
        await buttonEscapeSequence.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          // ReSharper disable once UseAwaitUsing
          using var stream = new ImprovedStream(new SourceAccess(csvFile));
          using var textReader = await stream.GetTextReaderAsync(csvFile.CodePageId, csvFile.SkipRows, m_CancellationTokenSource.Token);
          csvFile.EscapePrefixChar = (await textReader.InspectEscapePrefixAsync(csvFile.FieldDelimiterChar, csvFile.FieldQualifierChar, m_CancellationTokenSource.Token));
        });
    }

    private void buttonKeyFileRead_Click(object sender, EventArgs e)
     => SetPpgFile(textBoxKeyFileRead);

    private void buttonKeyFileWrite_Click(object sender, EventArgs e)
      => SetPpgFile(textBoxKeyFileWrite);

    private void SetPpgFile(Control sourceTextBox)
    {
      try
      {
        var split = FileSystemUtils.SplitPath(sourceTextBox.Text);
        var newFileName = WindowsAPICodePackWrapper.Open(
          split.DirectoryName,
          "File with PGP Key",
          "Key file (*.ascii;*.txt;*.key;*.asc)|*.ascii;*.txt;*.key;*.asc|All files (*.*)|*.*",
          split.FileName);

        if (newFileName is null || newFileName.Length == 0)
          return;
        sourceTextBox.Text = newFileName;
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    private void textBoxFile_TextChanged(object sender, EventArgs e)
    {
      bool isPgp = textBoxFile.Text.AssumePgp();
      textBoxKeyFileRead.Enabled = isPgp;
      buttonKeyFileRead.Enabled = isPgp;
    }
  }
}