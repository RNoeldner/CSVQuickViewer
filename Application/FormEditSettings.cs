/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools;

/// <summary>
///   Form to edit the Settings
/// </summary>
public partial class FormEditSettings : ResizeForm
{
  private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();
  private readonly int? m_NumRecords;
  private readonly ViewSettings m_ViewSettings;
  /// <summary>
  /// Warnings are passed on to HTML Info
  /// </summary>
  private readonly IEnumerable<string> m_Warnings;
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
    var m_InitialSettingNull = setting is null;
    FileSetting = setting ?? new CsvFileDummy();
    FontConfig = viewSettings;
    InitializeComponent();

    buttonFileInfo.Enabled = !m_InitialSettingNull;

    m_Warnings = warnings;

    cboNewLine.SetEnumDataSource(m_ViewSettings.WriteSetting.NewLine, new[] { RecordDelimiterTypeEnum.None });
    comboBoxLimitDuration.SetEnumDataSource(m_ViewSettings.LimitDuration);

    // Set Code Page Dropdown
    cboCodePageId.SuspendLayout();
    cboWriteCodePageId.SuspendLayout();
    var codePages = EncodingHelper.CommonCodePages.Select(cp => new DisplayItem<int>(cp, EncodingHelper.GetEncodingName(cp))).ToList();
    cboCodePageId.DataSource = codePages;
    cboWriteCodePageId.DataSource = codePages;
    cboCodePageId.ResumeLayout(true);
    cboWriteCodePageId.ResumeLayout(true);

#if !SupportPGP
    labelPGPKey.Visible = false;
    textBoxKeyFileWrite.Visible = false;
    buttonKeyFileWrite.Visible = false;
    labelPGPRead.Visible = false;
    textBoxKeyFileRead.Visible = false;
    buttonKeyFileRead.Visible = false;
#endif

    if (m_InitialSettingNull)
    {
#if SupportPGP
        if (!string.IsNullOrEmpty(m_ViewSettings.KeyFileRead))
          textBoxKeyFileRead.Text = m_ViewSettings.KeyFileRead;
        if (!string.IsNullOrEmpty(m_ViewSettings.KeyFileWrite))
          textBoxKeyFileWrite.Text = m_ViewSettings.KeyFileWrite;
#endif

      FileSetting.ContextSensitiveQualifier = m_ViewSettings.DefaultInspectionResult.ContextSensitiveQualifier;
      FileSetting.DuplicateQualifierToEscape = m_ViewSettings.DefaultInspectionResult.DuplicateQualifierToEscape;
      FileSetting.FieldQualifierChar = m_ViewSettings.DefaultInspectionResult.FieldQualifier;
      FileSetting.SkipRows = m_ViewSettings.DefaultInspectionResult.SkipRows;
      FileSetting.SkipRowsAfterHeader = m_ViewSettings.DefaultInspectionResult.SkipRowsAfterHeader;
    }


    toolTip.SetToolTip(checkBoxAllowRowCombining,
      @"Try to combine rows, it can happen if the column does contain a linefeed and is not properly quoted. 
That column content is moved to the next line.
Note: This does not work if it the issue is in the last column. The extra text of the columns flows into the next row, it cannot be recognized at the time the record is read. As the parser is working as a stream and can not go back it cannot be rectified. 
This is a very risky option, in some cases rows might be lost.");

    toolTip.SetToolTip(checkBoxTryToSolveMoreColumns,
      @"Try to realign columns in case the file is not quoted, and an extra delimiter has caused additional columns.
Re-Aligning works best if columns and their order are easily identifiable, if the columns are very similar e.g., all are text, or all are empty there is a high chance the realignment does fail.");
    m_NumRecords = numRecords;


    var assembly = Assembly.GetExecutingAssembly();
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    labelExecutable.Text= Environment.ProcessPath;
#else
    labelExecutable.Text= assembly.Location;
#endif

    labelVersion.Text =   assembly.GetName().Version!.ToString();

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    labelFrameWork.Text = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
#else
    labelFrameWork.Text = Environment.Version.ToString();
#endif
    linkLabelRepository.Links.Add(0, linkLabelRepository.Text.Length, "https://github.com/RNoeldner/CSVQuickViewer");
    linkLabelGnu.Links.Add(0, linkLabelGnu.Text.Length, "http://www.gnu.org/licenses/lgpl-3.0.html");
  }

  public CsvFileDummy FileSetting { get; }

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
      var split = FileSystemUtils.SplitPath(textBoxFileName.Text);
      var newFileName = WindowsAPICodePackWrapper.Open(
        split.DirectoryName,
        "Delimited File",
        "Delimited files|*.csv;*.txt;*.tab;*.tsv|All files (*.*)|*.*",
        split.FileName);

      if (newFileName is null || newFileName.Length == 0)
        return;

      newFileName = newFileName.GetShortestPath(".");
      SetDefaultInspectionResult();

      using var formProgress = new FormProgress("Examining file", m_CancellationTokenSource.Token);

      formProgress.Show(this);
      formProgress.Report("Inspecting");
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
        formProgress);
      formProgress.Close();

      buttonFileInfo.Enabled = true;
      FileSetting.SkipRows = ir.SkipRows;
      FileSetting.CodePageId = ir.CodePageId;
      FileSetting.ByteOrderMark = ir.ByteOrderMark;
      FileSetting.IdentifierInContainer = ir.IdentifierInContainer;
      FileSetting.HasFieldHeader = ir.HasFieldHeader;
      FileSetting.ColumnCollection.Overwrite(ir.Columns);
      FileSetting.CommentLine = ir.CommentLine;
      FileSetting.EscapePrefixChar = ir.EscapePrefix;
      FileSetting.FieldDelimiterChar = ir.FieldDelimiter;
      FileSetting.FieldQualifierChar = ir.FieldQualifier;
      FileSetting.ContextSensitiveQualifier = ir.ContextSensitiveQualifier;
      FileSetting.DuplicateQualifierToEscape = ir.DuplicateQualifierToEscape;
      FileSetting.NewLine = ir.NewLine;
      FileSetting.IsJson = ir.IsJson;
      FileSetting.IsXml = ir.IsXml;
      m_ViewSettings.DeriveWriteSetting(FileSetting);
      FileSetting.FileName = newFileName;
    }
    catch (Exception ex)
    {
      this.ShowError(ex);
    }
  }

  private async void ButtonEscapeSequence_Click(object sender, EventArgs e)
  {
    if (string.IsNullOrEmpty(FileSetting.FullPath))
    { return; }

    await buttonEscapeSequence.RunWithHourglassAsync(async () =>
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      // ReSharper disable once UseAwaitUsing
      using var stream = GetStream();
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var textReader = await GetTextReaderAsync(stream);
      FileSetting.EscapePrefixChar = (await textReader.InspectEscapePrefixAsync(FileSetting.FieldDelimiterChar,
        FileSetting.FieldQualifierChar, m_CancellationTokenSource.Token));
    });
  }

  private async void buttonFileInfo_Click(object sender, EventArgs e)
  {
    if (FileNameEmpty()) return;

    var info = FileSetting.Clone();
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

  private async void ButtonGuessCP_ClickAsync(object? sender, EventArgs e)
  {
    if (FileNameEmpty()) return;

    await buttonGuessCP.RunWithHourglassAsync(async () =>
      {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        await
#endif
        // ReSharper disable once UseAwaitUsing
        using var improvedStream = GetStream();
        var (codepage, bom) = await improvedStream.InspectCodePageAsync(m_CancellationTokenSource.Token);
        FileSetting.CodePageId = codepage;
        FileSetting.ByteOrderMark = bom;
      });
  }

  private async void ButtonGuessDelimiter_ClickAsync(object? sender, EventArgs e)
  {
    if (FileNameEmpty()) return;

    await buttonGuessDelimiter.RunWithHourglassAsync(async () =>
      {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        await
#endif
        // ReSharper disable once UseAwaitUsing
        using var stream = GetStream();
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        await
#endif
        using var textReader = await GetTextReaderAsync(stream);
        var res = await textReader.InspectDelimiterAsync(FileSetting.FieldQualifierChar, FileSetting.EscapePrefixChar,
          Array.Empty<char>(), FileSetting.GetDelimiterByExtension(), m_CancellationTokenSource.Token);
        if (res.IsDetected)
          FileSetting.FieldDelimiterChar = res.Delimiter;
      });
  }

  private async void ButtonGuessHeader_Click(object? sender, EventArgs e)
  {
    if (FileNameEmpty()) return;

    await buttonGuessHeader.RunWithHourglassAsync(async () =>
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      // ReSharper disable once UseAwaitUsing
      using var improvedStream = GetStream();
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var textReader = await GetTextReaderAsync(improvedStream);
      var res = await textReader.InspectHasHeaderAsync(FileSetting.FieldDelimiterChar, FileSetting.FieldQualifierChar,
        FileSetting.EscapePrefixChar, FileSetting.CommentLine, m_CancellationTokenSource.Token);
      if (!string.IsNullOrEmpty(res.message))
        MessageBox.Show(res.message, "Check Header");
      FileSetting.HasFieldHeader = res.hasHeader;
      bindingSourceViewSetting.ResetBindings(false);
    });

  }

  private async void ButtonGuessLineComment_Click(object? sender, EventArgs e)
  {
    if (FileNameEmpty()) return;

    await buttonGuessLineComment.RunWithHourglassAsync(async () =>
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      // ReSharper disable once UseAwaitUsing
      using var improvedStream = GetStream();
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var textReader = await GetTextReaderAsync(improvedStream);
      FileSetting.CommentLine = await textReader.InspectLineCommentAsync(m_CancellationTokenSource.Token);
    });

  }

  private async void ButtonGuessTextQualifier_Click(object? sender, EventArgs e)
  {
    if (FileNameEmpty()) return;

    await buttonGuessTextQualifier.RunWithHourglassAsync(async () =>
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      // ReSharper disable once UseAwaitUsing
      using var improvedStream = GetStream();
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var textReader = await GetTextReaderAsync(improvedStream);
      var res = textReader.InspectQualifier(FileSetting.FieldDelimiterChar, FileSetting.EscapePrefixChar, FileSetting.CommentLine,
        StaticCollections.PossibleQualifiers, m_CancellationTokenSource.Token);
      FileSetting.FieldQualifierChar = res.QuoteChar;
      if (res.DuplicateQualifier)
        FileSetting.DuplicateQualifierToEscape = res.DuplicateQualifier;
      if (!FileSetting.ContextSensitiveQualifier)
        FileSetting.ContextSensitiveQualifier = !(res.DuplicateQualifier || res.EscapedQualifier);
    });
  }

  private void ButtonInteractiveSettings_Click(object? sender, EventArgs e)
  {
    if (FileNameEmpty()) return;

    using var frm = new FindSkipRows(FileSetting.FullPath, FileSetting.CodePageId, FileSetting.SkipRows, FileSetting.FieldDelimiterChar, FileSetting.EscapePrefixChar,
      FileSetting.FieldQualifierChar, FileSetting.CommentLine);
    if (frm.ShowDialog() == DialogResult.OK)
    {
      FileSetting.SkipRows = frm.SkipRows;
      FileSetting.FieldDelimiterChar = frm.FieldDelimiterChar;
      FileSetting.EscapePrefixChar = frm.EscapePrefixChar;
      FileSetting.FieldQualifierChar = frm.FieldQualifierChar;
      FileSetting.CommentLine = frm.CommentLine;
    }
  }

  private void ButtonKeyFileRead_Click(object sender, EventArgs e)
    => SetPpgFile(textBoxKeyFileRead);

  private void ButtonKeyFileWrite_Click(object sender, EventArgs e)
    => SetPpgFile(textBoxKeyFileWrite);

  private bool FileNameEmpty()
  {
    ValidateChildren();
    return string.IsNullOrEmpty(FileSetting.FileName);
  }

  private async void ButtonSkipLine_ClickAsync(object? sender, EventArgs e)
  {
    if (FileNameEmpty()) return;
    await buttonSkipLine.RunWithHourglassAsync(async () =>
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      // ReSharper disable once UseAwaitUsing
      using var improvedStream = GetStream();
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var textReader = await GetTextReaderAsync(improvedStream);
      FileSetting.SkipRows = textReader.InspectStartRow(FileSetting.FieldDelimiterChar, FileSetting.FieldQualifierChar,
        FileSetting.EscapePrefixChar, FileSetting.CommentLine, m_CancellationTokenSource.Token);
    });
  }

  private void CheckBoxColumnsProcess_CheckedChanged(object? sender, EventArgs e)
  {
    if (FileSetting.TryToSolveMoreColumns || FileSetting.AllowRowCombining)
      FileSetting.WarnEmptyTailingColumns = true;
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
  private void FormEditSettings_Load(object? sender, EventArgs e)
  {
    bindingSourceViewSetting.DataSource = m_ViewSettings;
    bindingSourceWrite.DataSource = m_ViewSettings.WriteSetting;
    bindingSourceRead.DataSource = FileSetting;
    fillGuessSettingEdit.FillGuessSettings = m_ViewSettings.FillGuessSettings;

    quotingControlWrite = QuotingControl.AddQuotingControl(tableLayoutPanelWrite, 0, 7, 5, bindingSourceWrite);
    quotingControlWrite.IsWriteSetting = true;

    quotingControlRead = QuotingControl.AddQuotingControl(tableLayoutPanelFile, 0, 9, 5, bindingSourceRead);
    quotingControlRead.IsWriteSetting = false;

    TextBoxFile_Validating(this, new CancelEventArgs(false));
  }

  private void FormEditSettings_FormClosing(object? sender, FormClosingEventArgs e)
  {
    ValidateChildren();
    try { m_CancellationTokenSource.Cancel(); }
    catch
    {
      /* ignore */
    }

    SetDefaultInspectionResult();
    if (FileSetting != null)
      m_ViewSettings.PassOnConfiguration(FileSetting);
  }

  private Stream GetStream()
  {
#if !SupportPGP
    var sourceAccess = new SourceAccess(FileSetting.FileName, true, "", false, "");
#else
    var sourceAccess = new SourceAccess(FileSetting.FileName, true);
#endif
    return FunctionalDI.GetStream(sourceAccess);
  }

  private Task<ImprovedTextReader> GetTextReaderAsync(Stream stream)
      => stream.GetTextReaderAsync(FileSetting.CodePageId, FileSetting.SkipRows, m_CancellationTokenSource.Token);


  private async void GuessNewline_Click(object? sender, EventArgs e)
  {
    await buttonWriteNewLine.RunWithHourglassAsync(async () =>
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      // ReSharper disable once UseAwaitUsing
      using var improvedStream = GetStream();
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var textReader = await GetTextReaderAsync(improvedStream);
      cboNewLine.SelectedValue =
        textReader.InspectRecordDelimiter(FileSetting.FieldQualifierChar, m_CancellationTokenSource.Token);
    });
  }

  private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
  {
    try
    {
      Process.Start(new ProcessStartInfo { FileName = e.Link!.LinkData!.ToString(), UseShellExecute = true });
    }
    catch (Exception ex)
    {
      this.ShowError(ex);
    }
  }

  private void NumericUpDownSkipRows_ValueChanged(object sender, EventArgs e)
  {
    if (numericUpDownSkipRows.Value > 0)
      checkBoxCopySkipped.Checked = true;
    checkBoxCopySkipped.Enabled = (numericUpDownSkipRows.Value > 0);
  }

  private void pictureBox_Click(object sender, EventArgs e)
  {
    try
    {
      Process.Start(new ProcessStartInfo { FileName = "https://sourceforge.net/projects/CSVQuickViewer/files", UseShellExecute = true });
    }
    catch (Exception ex)
    {
      this.ShowError(ex);
    }
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
      m_ViewSettings.DefaultInspectionResult.CodePageId = FileSetting.CodePageId;
      m_ViewSettings.DefaultInspectionResult.ByteOrderMark = FileSetting.ByteOrderMark;
    }

    if (!m_ViewSettings.GuessEscapePrefix)
      m_ViewSettings.DefaultInspectionResult.EscapePrefix = FileSetting.EscapePrefixChar;

    if (!m_ViewSettings.GuessComment)
      m_ViewSettings.DefaultInspectionResult.CommentLine = FileSetting.CommentLine;

    if (!m_ViewSettings.GuessDelimiter)
      m_ViewSettings.DefaultInspectionResult.FieldDelimiter = FileSetting.FieldDelimiterChar;

    if (!m_ViewSettings.GuessHasHeader)
      m_ViewSettings.DefaultInspectionResult.HasFieldHeader = FileSetting.HasFieldHeader;

    if (!m_ViewSettings.GuessQualifier)
    {
      m_ViewSettings.DefaultInspectionResult.ContextSensitiveQualifier = FileSetting.ContextSensitiveQualifier;
      m_ViewSettings.DefaultInspectionResult.DuplicateQualifierToEscape = FileSetting.DuplicateQualifierToEscape;
      m_ViewSettings.DefaultInspectionResult.FieldQualifier = FileSetting.FieldQualifierChar;
    }

    if (!m_ViewSettings.GuessStartRow)
    {
      m_ViewSettings.DefaultInspectionResult.SkipRows = FileSetting.SkipRows;
      m_ViewSettings.DefaultInspectionResult.SkipRowsAfterHeader = FileSetting.SkipRowsAfterHeader;
    }

#if SupportPGP
      m_ViewSettings.KeyFileRead = textBoxKeyFileRead.Text;
      m_ViewSettings.KeyFileWrite = textBoxKeyFileWrite.Text;
#endif
  }

  private void SetPpgFile(Control sourceTextBox)
  {
#if SupportPGP
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
#endif
  }

  private void TextBoxFile_TextChanged(object sender, EventArgs e)
  {
#if SupportPGP
    bool isPgp = textBoxFile.Text.AssumePgp();
    textBoxKeyFileRead.Enabled = isPgp;
    buttonKeyFileRead.Enabled = isPgp;
#endif
  }

  private void TextBoxFile_Validating(object? sender, CancelEventArgs e)
  {

    var hasFile = FileSystemUtils.FileExists(textBoxKeyFileRead.Text);
    buttonGuessHeader.Enabled=hasFile;
    buttonGuessCP.Enabled=hasFile;
    buttonGuessDelimiter.Enabled=hasFile;
    buttonGuessLineComment.Enabled=hasFile;
    buttonSkipLine.Enabled=hasFile;
    buttonInteractiveSettings.Enabled=hasFile;
    buttonGuessTextQualifier.Enabled=hasFile;
    buttonEscapeSequence.Enabled=hasFile;
    if (!hasFile)
    {
      errorProvider.SetError(textBoxFileName, "File does not exist.");
      e.Cancel = true;
    }
    else
    {
      errorProvider.SetError(textBoxFileName, string.Empty);
    }
  }
}