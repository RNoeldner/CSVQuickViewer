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
using System.Collections.Generic;
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
    /// <summary>
    /// Warnings are passed on to HTML Info
    /// </summary>
    private readonly IEnumerable<string> m_Warnings;

    private readonly int? m_NumRecords;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormEditSettings"/> class.
    /// </summary>
    /// <param name="viewSettings">The default view settings.</param>
    /// <param name="setting">The current setting.</param>
    /// <param name="warnings">The warnings to be listed in HTML Info</param>
    /// <param name="numRecords">The number records that have been read</param>
    /// <exception cref="System.ArgumentNullException">viewSettings</exception>
    public FormEditSettings(ViewSettings viewSettings, CsvFileDummy? setting,
      IEnumerable<string> warnings, int? numRecords)
    {
      m_ViewSettings = viewSettings ?? throw new ArgumentNullException(nameof(viewSettings));
      FileSetting = setting;
      FontConfig = viewSettings;
      InitializeComponent();
      if (setting != null)
        quotingControl.CsvFile = setting;
      buttonFileInfo.Enabled = setting != null;

      m_Warnings = warnings;
#if !SupportPGP
      labelPGPKey.Visible = false;
      textBoxKeyFileWrite.Visible = false;
      buttonKeyFileWrite.Visible = false;
      labelPGPRead.Visible = false;
      textBoxKeyFileRead.Visible = false;
      buttonKeyFileRead.Visible = false;
#endif

      toolTip.SetToolTip(checkBoxAllowRowCombining,
        @"Try to combine rows, it can happen if the column does contain a linefeed and is not properly quoted. 
That column content is moved to the next line.
Note: This does not work if it the issue is in the last column. The extra text of the columns flows into the next row, it cannot be recognized at the time the record is read. As the parser is working as a stream and can not go back it cannot be rectified. 
This is a very risky option, in some cases rows might be lost.");

      toolTip.SetToolTip(checkBoxTryToSolveMoreColumns,
        @"Try to realign columns in case the file is not quoted, and an extra delimiter has caused additional columns.
Re-Aligning works best if columns and their order are easily identifiable, if the columns are very similar e.g., all are text, or all are empty there is a high chance the realignment does fail.");
      m_NumRecords = numRecords;
    }

    public CsvFileDummy? FileSetting { get; private set; }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      try { m_CancellationTokenSource.Cancel(); }
      catch
      {
        /* ignore */
      }

      if (disposing)
      {
        components?.Dispose();
        m_CancellationTokenSource.Dispose();
      }

      base.Dispose(disposing);
    }

    private async void BtnOpenFile_Click(object? sender, EventArgs e)
    {
      try
      {
        var split = FileSystemUtils.SplitPath(textBoxFile.Text);
        var newFileName = WindowsAPICodePackWrapper.Open(
          split.DirectoryName,
          "Delimited File",
          "Delimited files|*.csv;*.txt;*.tab;*.tsv|All files (*.*)|*.*",
          split.FileName);

        if (newFileName is null || newFileName.Length == 0)
          return;
        SetDefaultInspectionResult();

        using var formProgress = new FormProgress("Examining file", false, FontConfig, m_CancellationTokenSource.Token);
        formProgress.Maximum = 0;
        formProgress.Show(this);

        var ir = await newFileName.InspectFileAsync(m_ViewSettings.AllowJson,
          m_ViewSettings.GuessCodePage, m_ViewSettings.GuessEscapePrefix,
          m_ViewSettings.GuessDelimiter, m_ViewSettings.GuessQualifier, m_ViewSettings.GuessStartRow,
          m_ViewSettings.GuessHasHeader, m_ViewSettings.GuessNewLine, m_ViewSettings.GuessComment,
          m_ViewSettings.FillGuessSettings,
          list =>
          {
            if (list.Count == 1)
              return list.First();
            using var frm = new FormSelectInDropdown(list, list.FirstOrDefault(x => x.AssumeDelimited()));
            if (frm.ShowWithFont(this, true) == DialogResult.Cancel)
              throw new OperationCanceledException();
            return frm.SelectedText;
          }, m_ViewSettings.DefaultInspectionResult,
#if SupportPGP
            PgpHelper.GetKeyAndValidate(newFileName, m_ViewSettings.KeyFileRead),
#else
          string.Empty,
#endif
          formProgress.CancellationToken);
        formProgress.Close();

        FileSetting ??= new CsvFileDummy();
        buttonFileInfo.Enabled = true;
        ir.CopyToCsv(FileSetting);
        FileSetting.IsJson = ir.IsJson;
        FileSetting.IsXml = ir.IsXml;
        m_ViewSettings.DeriveWriteSetting(FileSetting);
        UpdateUI();
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    private async void ButtonEscapeSequence_Click(object sender, EventArgs e)
    {
      if (FileSetting is ICsvFile csvFile)
      {
        StoreFromUI(FileSetting);
        await buttonEscapeSequence.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          // ReSharper disable once UseAwaitUsing
          using var stream = FunctionalDI.GetStream(new SourceAccess(csvFile));
#if NET5_0_OR_GREATER
          await
#endif
          using var textReader =
            await stream.GetTextReaderAsync(csvFile.CodePageId, csvFile.SkipRows, m_CancellationTokenSource.Token);
          csvFile.EscapePrefixChar = (await textReader.InspectEscapePrefixAsync(csvFile.FieldDelimiterChar,
            csvFile.FieldQualifierChar, m_CancellationTokenSource.Token));
        });
        UpdateUI();
      }
    }

    private async void ButtonGuessCP_ClickAsync(object? sender, EventArgs e)
    {
      if (FileSetting is ICsvFile csvFile)
      {
        StoreFromUI(FileSetting);
        await buttonGuessCP.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = FunctionalDI.GetStream(new SourceAccess(csvFile));
          var (codepage, bom) = await improvedStream.InspectCodePageAsync(m_CancellationTokenSource.Token);
          csvFile.CodePageId = codepage;
          csvFile.ByteOrderMark = bom;
        });
        UpdateUI();
      }
    }

    private async void ButtonGuessDelimiter_ClickAsync(object? sender, EventArgs e)
    {
      if (FileSetting is ICsvFile csvFile)
      {
        StoreFromUI(FileSetting);
        await buttonGuessDelimiter.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = FunctionalDI.GetStream(new SourceAccess(csvFile));
#if NET5_0_OR_GREATER
          await
#endif
          using var textReader = await improvedStream.GetTextReaderAsync(csvFile.CodePageId, csvFile.SkipRows,
            m_CancellationTokenSource.Token);
          var res = await textReader.InspectDelimiterAsync(csvFile.FieldQualifierChar, csvFile.EscapePrefixChar,
            Array.Empty<char>(), csvFile.GetDelimiterByExtension(), m_CancellationTokenSource.Token);
          if (res.IsDetected)
            csvFile.FieldDelimiterChar = res.Delimiter;
        });
        UpdateUI();
      }
    }

    private async void ButtonGuessHeader_Click(object? sender, EventArgs e)
    {
      if (FileSetting is ICsvFile csvFile)
      {
        StoreFromUI(FileSetting);
        await buttonGuessHeader.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = FunctionalDI.GetStream(new SourceAccess(csvFile));
#if NET5_0_OR_GREATER
          await
#endif
          using var textReader = await improvedStream.GetTextReaderAsync(csvFile.CodePageId, csvFile.SkipRows,
            m_CancellationTokenSource.Token);
          var res = await textReader.InspectHasHeaderAsync(csvFile.FieldDelimiterChar, csvFile.FieldQualifierChar,
            csvFile.EscapePrefixChar, csvFile.CommentLine, m_CancellationTokenSource.Token);
          if (!string.IsNullOrEmpty(res.message))
            MessageBox.Show(res.message, "Check Header");
          csvFile.HasFieldHeader = res.hasHeader;
          bindingSourceViewSetting.ResetBindings(false);
        });
        UpdateUI();
      }
    }

    private async void ButtonGuessLineComment_Click(object? sender, EventArgs e)
    {
      if (FileSetting is ICsvFile csvFile)
      {
        StoreFromUI(FileSetting);
        await buttonGuessLineComment.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = FunctionalDI.GetStream(new SourceAccess(csvFile));
#if NET5_0_OR_GREATER
          await
#endif
          using var textReader = await improvedStream.GetTextReaderAsync(csvFile.CodePageId, csvFile.SkipRows,
            m_CancellationTokenSource.Token);
          csvFile.CommentLine = await textReader.InspectLineCommentAsync(m_CancellationTokenSource.Token);
        });
        UpdateUI();
      }
    }

    private async void ButtonGuessTextQualifier_Click(object? sender, EventArgs e)
    {
      if (FileSetting is ICsvFile csvFile)
      {
        StoreFromUI(FileSetting);
        await buttonGuessTextQualifier.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = FunctionalDI.GetStream(new SourceAccess(csvFile));
#if NET5_0_OR_GREATER
          await
#endif
          using var textReader = await improvedStream.GetTextReaderAsync(csvFile.CodePageId, csvFile.SkipRows,
            m_CancellationTokenSource.Token);
          var res = textReader.InspectQualifier(csvFile.FieldDelimiterChar, csvFile.EscapePrefixChar, csvFile.CommentLine,
            StaticCollections.PossibleQualifiers, m_CancellationTokenSource.Token);
          csvFile.FieldQualifierChar = res.QuoteChar;
          if (res.DuplicateQualifier)
            csvFile.DuplicateQualifierToEscape = res.DuplicateQualifier;
          if (!csvFile.ContextSensitiveQualifier)
            csvFile.ContextSensitiveQualifier = !(res.DuplicateQualifier || res.EscapedQualifier);
        });
        UpdateUI();
      }
    }

    private void ButtonInteractiveSettings_Click(object? sender, EventArgs e)
    {
      if (FileSetting is ICsvFile csvFile)
      {
        StoreFromUI(FileSetting);
        using var frm = new FindSkipRows(csvFile);
        _ = frm.ShowDialog();
        UpdateUI();
      }
    }

    private void ButtonKeyFileRead_Click(object sender, EventArgs e)
      => SetPpgFile(textBoxKeyFileRead);

    private void ButtonKeyFileWrite_Click(object sender, EventArgs e)
      => SetPpgFile(textBoxKeyFileWrite);

    private async void ButtonSkipLine_ClickAsync(object? sender, EventArgs e)
    {
      if (FileSetting is ICsvFile csvFile)
      {
        StoreFromUI(FileSetting);
        await buttonSkipLine.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = FunctionalDI.GetStream(new SourceAccess(csvFile));
#if NET5_0_OR_GREATER
          await
#endif
          using var textReader =
            await improvedStream.GetTextReaderAsync(csvFile.CodePageId, 0, m_CancellationTokenSource.Token);
          csvFile.SkipRows = textReader.InspectStartRow(csvFile.FieldDelimiterChar, csvFile.FieldQualifierChar,
            csvFile.EscapePrefixChar, csvFile.CommentLine, m_CancellationTokenSource.Token);
        });
        UpdateUI();
      }
    }

    private void CheckBoxColumnsProcess_CheckedChanged(object? sender, EventArgs e)
    {
      if (FileSetting is ICsvFile csvFile)
        if (csvFile.TryToSolveMoreColumns || csvFile.AllowRowCombining)
          csvFile.WarnEmptyTailingColumns = true;
    }

    private void CheckBoxCopySkipped_MouseClick(object sender, MouseEventArgs e)
    {
      if (!checkBoxCopySkipped.Checked)
        m_ViewSettings.WriteSetting.SkipRows = 0;
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

      cboCodePage.DataSource = codePages;
      cboWriteCodePage.DataSource = codePages;
      cboCodePage.ResumeLayout(true);
      cboWriteCodePage.ResumeLayout(true);

      cboRecordDelimiter.SuspendLayout();
      if (FileSetting == null)
      {
#if SupportPGP
        if (!string.IsNullOrEmpty(m_ViewSettings.KeyFileRead))
          textBoxKeyFileRead.Text = m_ViewSettings.KeyFileRead;
        if (!string.IsNullOrEmpty(m_ViewSettings.KeyFileWrite))
          textBoxKeyFileWrite.Text = m_ViewSettings.KeyFileWrite;
#endif

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

      cboRecordDelimiter.SetEnumDataSource(m_ViewSettings.WriteSetting.NewLine, new[] { RecordDelimiterTypeEnum.None });
      comboBoxLimitDuration.SetEnumDataSource(m_ViewSettings.LimitDuration);

      quotingControlWrite.CsvFile = m_ViewSettings.WriteSetting;
      quotingControlWrite.IsWriteSetting = true;

      UpdateUI();
    }

    private void FormEditSettings_FormClosing(object? sender, FormClosingEventArgs e)
    {
      ValidateChildren();
      StoreFromUI(FileSetting);
      try { m_CancellationTokenSource.Cancel(); }
      catch
      {
        /* ignore */
      }

      SetDefaultInspectionResult();
      if (FileSetting != null)
        m_ViewSettings.PassOnConfiguration(FileSetting);
    }

    private async void GuessNewline_Click(object? sender, EventArgs e)
    {
      if (FileSetting is ICsvFile csvFile)
      {
        StoreFromUI(FileSetting);
        await buttonNewLine.RunWithHourglassAsync(async () =>
        {
#if NET5_0_OR_GREATER
          await
#endif
          // ReSharper disable once UseAwaitUsing
          using var improvedStream = FunctionalDI.GetStream(new SourceAccess(csvFile));
#if NET5_0_OR_GREATER
          await
#endif
          using var textReader = await improvedStream.GetTextReaderAsync(csvFile.CodePageId, csvFile.SkipRows,
            m_CancellationTokenSource.Token);
          cboRecordDelimiter.SelectedValue =
            textReader.InspectRecordDelimiter(csvFile.FieldQualifierChar, m_CancellationTokenSource.Token);
        });
        UpdateUI();
      }
    }

    private void NumericUpDownSkipRows_ValueChanged(object sender, EventArgs e)
    {
      if (numericUpDownSkipRows.Value > 0)
        checkBoxCopySkipped.Checked = true;
      checkBoxCopySkipped.Enabled = (numericUpDownSkipRows.Value > 0);
    }

    private void SelectFont_ValueChanged(object sender, EventArgs e)
    {
      m_ViewSettings.Font = selectFont.FontName;
      m_ViewSettings.FontSize = selectFont.FontSize;
    }

    private void SetDefaultInspectionResult()
    {
      if (!m_ViewSettings.GuessCodePage)
      {
        if (cboCodePage.SelectedItem != null)
          m_ViewSettings.DefaultInspectionResult.CodePageId = ((DisplayItem<int>) cboCodePage.SelectedItem).ID;
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
        m_ViewSettings.DefaultInspectionResult.ContextSensitiveQualifier =
          quotingControl.CsvFile.ContextSensitiveQualifier;
        m_ViewSettings.DefaultInspectionResult.DuplicateQualifierToEscape =
          quotingControl.CsvFile.DuplicateQualifierToEscape;
        m_ViewSettings.DefaultInspectionResult.FieldQualifier = quotingControl.CsvFile.FieldQualifierChar;
      }

      if (!m_ViewSettings.GuessStartRow)
        m_ViewSettings.DefaultInspectionResult.SkipRows = Convert.ToInt32(numericUpDownSkipRows.Value);
      // if this is not for a specific file store the value in the defaults

#if SupportPGP
      if (FileSetting != null)
        return;
      m_ViewSettings.KeyFileRead = textBoxKeyFileRead.Text;
      m_ViewSettings.KeyFileWrite = textBoxKeyFileWrite.Text;
#endif
    }

    private void SetPpgFile(Control sourceTextBox)
    {
      try
      {
        var split = FileSystemUtils.SplitPath(sourceTextBox.Text);
        var newFileName = WindowsAPICodePackWrapper.Open(
          split.DirectoryName,
          "File with PGP Key",
          "Key file|*.ascii;*.txt;*.key;*.asc|All files (*.*)|*.*",
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

    private void StoreFromUI(ICsvFile? fileSetting)
    {
      if (fileSetting == null)
        return;
      if (cboCodePage.SelectedValue is int cboCodePageId)
        fileSetting.CodePageId = cboCodePageId;
      fileSetting.CommentLine = textBoxComment.Text.Trim();
      fileSetting.FileName = textBoxFile.Text.Trim();
      fileSetting.HasFieldHeader = checkBoxHeader.Checked;
      fileSetting.ByteOrderMark = checkBoxBOM.Checked;
      fileSetting.FieldDelimiterChar = textBoxDelimiter.Text.FromText();
      fileSetting.SkipRows = Convert.ToInt32(numericUpDownSkipRows.Value);
      fileSetting.TreatTextAsNull = textBoxTextAsNull.Text.Trim();
      fileSetting.EscapePrefixChar = textBoxEscapeRead.Text.FromText();
      fileSetting.KeyFile = textBoxKeyFileRead.Text;
      fileSetting.TreatNBSPAsSpace = checkBoxTreatNBSPAsSpace.Checked;
      fileSetting.SkipEmptyLines = checkBoxSkipEmptyLines.Checked;
      fileSetting.TreatUnknownCharacterAsSpace = checkBoxTreatUnknowCharaterAsSpace.Checked;
      fileSetting.TreatLfAsSpace = checkBoxTreatLfAsSpace.Checked;
      fileSetting.TryToSolveMoreColumns = checkBoxTryToSolveMoreColumns.Checked;
      fileSetting.AllowRowCombining = checkBoxAllowRowCombining.Checked;
      fileSetting.NumWarnings = Convert.ToInt32(numericUpDownNumWarnings.Value);
    }

    private void TextBoxFile_TextChanged(object sender, EventArgs e)
    {
      bool isPgp = textBoxFile.Text.AssumePgp();
      textBoxKeyFileRead.Enabled = isPgp;
      buttonKeyFileRead.Enabled = isPgp;
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

    private void UpdateUI()
    {
      if (FileSetting is ICsvFile csvFile)
      {
        //if (cboCodePage.DataSource is List<DisplayItem<int>> list)
        //  cboCodePage.SelectedItem = list.FirstOrDefault(x => x.ID == csvFile.CodePageId)  ?? list.First();
        cboCodePage.SelectedValue = csvFile.CodePageId;
        textBoxComment.Text = csvFile.CommentLine;
        textBoxFile.Text = csvFile.FileName;
        checkBoxHeader.Checked = csvFile.HasFieldHeader;
        checkBoxBOM.Checked = csvFile.ByteOrderMark;
        textBoxDelimiter.Text = csvFile.FieldDelimiterChar.ToString();
        numericUpDownSkipRows.Value = csvFile.SkipRows;
        textBoxTextAsNull.Text = csvFile.TreatTextAsNull;
        textBoxEscapeRead.Text = csvFile.EscapePrefixChar.ToString();
        textBoxKeyFileRead.Text = csvFile.KeyFile;
        checkBoxTreatNBSPAsSpace.Checked = csvFile.TreatNBSPAsSpace;
        checkBoxSkipEmptyLines.Checked = csvFile.SkipEmptyLines;
        checkBoxTreatUnknowCharaterAsSpace.Checked = csvFile.TreatUnknownCharacterAsSpace;
        checkBoxTreatLfAsSpace.Checked = csvFile.TreatLfAsSpace;
        checkBoxTryToSolveMoreColumns.Checked = csvFile.TryToSolveMoreColumns;
        checkBoxAllowRowCombining.Checked = csvFile.AllowRowCombining;
        numericUpDownNumWarnings.Value = csvFile.NumWarnings;
      }
      else
      {
        //if (cboCodePage.DataSource is List<DisplayItem<int>> list)
        //  cboCodePage.SelectedItem = list.FirstOrDefault(x => x.ID == m_ViewSettings.DefaultInspectionResult.CodePageId)  ?? list.First();
        cboCodePage.SelectedValue = m_ViewSettings.DefaultInspectionResult.CodePageId;
        textBoxDelimiter.Character = m_ViewSettings.DefaultInspectionResult.FieldDelimiter;
        textBoxEscapeRead.Character = m_ViewSettings.DefaultInspectionResult.EscapePrefix;
        textBoxComment.Text = m_ViewSettings.DefaultInspectionResult.CommentLine;
        checkBoxHeader.Checked = m_ViewSettings.DefaultInspectionResult.HasFieldHeader;
        if (!m_ViewSettings.GuessCodePage)
          checkBoxBOM.Checked = m_ViewSettings.DefaultInspectionResult.ByteOrderMark;
      }
    }


    private async void buttonFileInfo_Click(object sender, EventArgs e)
    {
      if (FileSetting == null)
        return;
      var info = (ICsvFile) FileSetting.Clone();
      StoreFromUI(info);
      var html = new HtmlStyle(string.Empty);
      var stringBuilder = html.StartHtmlDoc("eeeeee");
      await buttonFileInfo.RunWithHourglassAsync(async () =>
      {
        stringBuilder.Append(
          await info.GetFileInformationHtml(m_Warnings, m_NumRecords, FileSetting.IsCsv,
            m_CancellationTokenSource.Token));
        stringBuilder.AppendLine("</BODY>");
        stringBuilder.AppendLine("</HTML>");
        MessageBox.ShowBigHtml(stringBuilder.ToString(), "Information", MessageBoxButtons.OK,
          MessageBoxIcon.Information,
          MessageBoxDefaultButton.Button1, 120D);
      });
    }
  }
}