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
  private readonly EditSettings m_ReadSettings;
  private readonly EditSettings m_WriteSettings;
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
    m_ReadSettings = new EditSettings(setting);
    m_WriteSettings  = new EditSettings(viewSettings.WriteSetting);
    m_Warnings = warnings;
    m_NumRecords = numRecords;

    if (setting is null)
    {
#if SupportPGP
        if (!string.IsNullOrEmpty(m_ViewSettings.KeyFileRead))
          textBoxKeyFileRead.Text = m_ViewSettings.KeyFileRead;
        if (!string.IsNullOrEmpty(m_ViewSettings.KeyFileWrite))
          textBoxKeyFileWrite.Text = m_ViewSettings.KeyFileWrite;
#endif

      m_ReadSettings.ContextSensitiveQualifier = m_ViewSettings.DefaultInspectionResult.ContextSensitiveQualifier;
      m_ReadSettings.DuplicateQualifierToEscape = m_ViewSettings.DefaultInspectionResult.DuplicateQualifierToEscape;
      m_ReadSettings.FieldQualifierChar = m_ViewSettings.DefaultInspectionResult.FieldQualifier;
      m_ReadSettings.SkipRows = m_ViewSettings.DefaultInspectionResult.SkipRows;
      m_ReadSettings.SkipRowsAfterHeader = m_ViewSettings.DefaultInspectionResult.SkipRowsAfterHeader;
    }

    FontConfig = viewSettings;
    InitializeComponent();

    cboNewLine.SetEnumDataSource(m_WriteSettings.NewLine, new[] { RecordDelimiterTypeEnum.None });
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

    toolTip.SetToolTip(checkBoxAllowRowCombining,
      @"Try to combine rows, it can happen if the column does contain a linefeed and is not properly quoted. 
That column content is moved to the next line.
Note: This does not work if it the issue is in the last column. The extra text of the columns flows into the next row, it cannot be recognized at the time the record is read. As the parser is working as a stream and can not go back it cannot be rectified. 
This is a very risky option, in some cases rows might be lost.");

    toolTip.SetToolTip(checkBoxTryToSolveMoreColumns,
      @"Try to realign columns in case the file is not quoted, and an extra delimiter has caused additional columns.
Re-Aligning works best if columns and their order are easily identifiable, if the columns are very similar e.g., all are text, or all are empty there is a high chance the realignment does fail.");

    var assembly = Assembly.GetExecutingAssembly();
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    labelExecutable.Text= Environment.ProcessPath;
#else
    labelExecutable.Text= assembly.Location;
#endif    
    labelVersion.Text = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? assembly.GetName().Version!.ToString();

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    labelFrameWork.Text = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
#else
    labelFrameWork.Text = Environment.Version.ToString();
#endif
    linkLabelRepository.Links.Add(0, linkLabelRepository.Text.Length, "https://github.com/RNoeldner/CSVQuickViewer");
    linkLabelGnu.Links.Add(0, linkLabelGnu.Text.Length, "http://www.gnu.org/licenses/lgpl-3.0.html");
  }

  public CsvFileDummy EditedSetting => m_ReadSettings.ToCsvFile();

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
      this.SafeInvoke(() =>
      {
        formProgress.Close();

        m_ReadSettings.FileName = newFileName;
        m_ReadSettings.SkipRows = ir.SkipRows;
        m_ReadSettings.CodePageId = ir.CodePageId;
        m_ReadSettings.ByteOrderMark = ir.ByteOrderMark;
        m_ReadSettings.IdentifierInContainer = ir.IdentifierInContainer;
        m_ReadSettings.HasFieldHeader = ir.HasFieldHeader;
        m_ReadSettings.ColumnCollection.Overwrite(ir.Columns);
        m_ReadSettings.CommentLine = ir.CommentLine;
        m_ReadSettings.EscapePrefixChar = ir.EscapePrefix;
        m_ReadSettings.FieldDelimiterChar = ir.FieldDelimiter;
        m_ReadSettings.FieldQualifierChar = ir.FieldQualifier;
        m_ReadSettings.ContextSensitiveQualifier = ir.ContextSensitiveQualifier;
        m_ReadSettings.DuplicateQualifierToEscape = ir.DuplicateQualifierToEscape;
        m_ReadSettings.NewLine = ir.NewLine;
        m_ReadSettings.IsJson = ir.IsJson;
        m_ReadSettings.IsXml = ir.IsXml;

        m_WriteSettings.CodePageId = ir.CodePageId;
        m_WriteSettings.ByteOrderMark = ir.ByteOrderMark;

        m_WriteSettings.ColumnCollection.Overwrite(ir.Columns);

        // Fix No Qualifier
        if (m_WriteSettings.FieldQualifierChar == 0)
          m_WriteSettings.FieldQualifierChar = '"';

        // Fix No DuplicateQualifier
        if (!m_WriteSettings.DuplicateQualifierToEscape && m_WriteSettings.FieldQualifierChar == '"' &&
            m_WriteSettings.EscapePrefixChar == char.MinValue)
          m_WriteSettings.DuplicateQualifierToEscape = true;

        // Fix No Delimiter
        if (m_WriteSettings.FieldDelimiterChar == 0)
          m_WriteSettings.FieldDelimiterChar = ',';

        // NewLine depending on Environment
        if (Environment.NewLine == "\r\n")
          m_WriteSettings.NewLine = RecordDelimiterTypeEnum.Crlf;
        else if (Environment.NewLine == "\n")
          m_WriteSettings.NewLine = RecordDelimiterTypeEnum.Lf;
        else if (Environment.NewLine == "\r")
          m_WriteSettings.NewLine = RecordDelimiterTypeEnum.Cr;

        TextBoxFile_Validating(sender, new CancelEventArgs(false));
        textBoxFileName.Focus();
      });
    }
    catch (Exception ex)
    {
      this.ShowError(ex);
    }
  }

  private async void ButtonEscapeSequence_Click(object sender, EventArgs e)
  {
    if (string.IsNullOrEmpty(m_ReadSettings.FileName))
    { return; }

    await buttonEscapeSequence.RunWithHourglassAsync(async () =>
    {
      // ReSharper disable once UseAwaitUsing
      using var stream = GetStream();
      using var textReader = await GetTextReaderAsync(stream);
      m_ReadSettings.EscapePrefixChar = (await textReader.InspectEscapePrefixAsync(m_ReadSettings.FieldDelimiterChar,
        m_ReadSettings.FieldQualifierChar, m_CancellationTokenSource.Token));
    });
  }

  private async void buttonFileInfo_Click(object sender, EventArgs e)
  {
    var html = new HtmlStyle(string.Empty);
    var stringBuilder = html.StartHtmlDoc(string.Empty);
    await buttonFileInfo.RunWithHourglassAsync(async () =>
    {
      stringBuilder.Append(
        await m_ReadSettings.ToCsvFile().GetFileInformationHtml(m_Warnings, m_NumRecords, !(m_ReadSettings.IsJson || m_ReadSettings.IsXml),
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
    await buttonGuessCP.RunWithHourglassAsync(async () =>
      {
        // ReSharper disable once UseAwaitUsing
        using var improvedStream = GetStream();
        var (codepage, bom) = await improvedStream.InspectCodePageAsync(m_CancellationTokenSource.Token);
        m_ReadSettings.CodePageId = codepage;
        m_ReadSettings.ByteOrderMark = bom;
      });
  }

  private async void ButtonGuessDelimiter_ClickAsync(object? sender, EventArgs e)
  {
    await buttonGuessDelimiter.RunWithHourglassAsync(async () =>
      {
        // ReSharper disable once UseAwaitUsing
        using var stream = GetStream();
        using var textReader = await GetTextReaderAsync(stream);
        var res = await textReader.InspectDelimiterAsync(m_ReadSettings.FieldQualifierChar, m_ReadSettings.EscapePrefixChar,
          Array.Empty<char>(), m_ReadSettings.FileName.GetDelimiterByExtension(), m_CancellationTokenSource.Token);
        if (res.IsDetected)
          m_ReadSettings.FieldDelimiterChar = res.Delimiter;
      });
  }

  private async void ButtonGuessHeader_Click(object? sender, EventArgs e)
  {
    await buttonGuessHeader.RunWithHourglassAsync(async () =>
    {
      // ReSharper disable once UseAwaitUsing
      using var improvedStream = GetStream();
      using var textReader = await GetTextReaderAsync(improvedStream);
      var res = await textReader.InspectHasHeaderAsync(m_ReadSettings.FieldDelimiterChar, m_ReadSettings.FieldQualifierChar,
        m_ReadSettings.EscapePrefixChar, m_ReadSettings.CommentLine, m_CancellationTokenSource.Token);
      if (!string.IsNullOrEmpty(res.message))
        MessageBox.Show(res.message, "Check Header");
      m_ReadSettings.HasFieldHeader = res.hasHeader;
      bindingSourceViewSetting.ResetBindings(false);
    });

  }

  private async void ButtonGuessLineComment_Click(object? sender, EventArgs e)
  {
    await buttonGuessLineComment.RunWithHourglassAsync(async () =>
    {
      // ReSharper disable once UseAwaitUsing
      using var improvedStream = GetStream();
      using var textReader = await GetTextReaderAsync(improvedStream);
      m_ReadSettings.CommentLine = await textReader.InspectLineCommentAsync(m_CancellationTokenSource.Token);
    });

  }

  private async void ButtonGuessTextQualifier_Click(object? sender, EventArgs e)
  {
    await buttonGuessTextQualifier.RunWithHourglassAsync(async () =>
    {
      // ReSharper disable once UseAwaitUsing
      using var improvedStream = GetStream();
      using var textReader = await GetTextReaderAsync(improvedStream);
      var res = textReader.InspectQualifier(m_ReadSettings.FieldDelimiterChar, m_ReadSettings.EscapePrefixChar, m_ReadSettings.CommentLine,
        StaticCollections.PossibleQualifiers, m_CancellationTokenSource.Token);
      m_ReadSettings.FieldQualifierChar = res.QuoteChar;
      if (res.DuplicateQualifier)
        m_ReadSettings.DuplicateQualifierToEscape = res.DuplicateQualifier;
      if (!m_ReadSettings.ContextSensitiveQualifier)
        m_ReadSettings.ContextSensitiveQualifier = !(res.DuplicateQualifier || res.EscapedQualifier);
    });
  }

  private void ButtonInteractiveSettings_Click(object? sender, EventArgs e)
  {
    buttonInteractiveSettings.RunWithHourglass(() =>
    {
      using var frm = new FindSkipRows(m_ReadSettings.FileName, m_ReadSettings.CodePageId, m_ReadSettings.SkipRows, m_ReadSettings.FieldDelimiterChar, m_ReadSettings.EscapePrefixChar,
      m_ReadSettings.FieldQualifierChar, m_ReadSettings.CommentLine);
      if (frm.ShowDialog() == DialogResult.OK)
      {
        m_ReadSettings.SkipRows = frm.SkipRows;
        m_ReadSettings.FieldDelimiterChar = frm.FieldDelimiterChar;
        m_ReadSettings.EscapePrefixChar = frm.EscapePrefixChar;
        m_ReadSettings.FieldQualifierChar = frm.FieldQualifierChar;
        m_ReadSettings.CommentLine = frm.CommentLine;
      }
    });
  }

  private void ButtonKeyFileRead_Click(object sender, EventArgs e)
    => SetPpgFile(textBoxKeyFileRead);

  private void ButtonKeyFileWrite_Click(object sender, EventArgs e)
    => SetPpgFile(textBoxKeyFileWrite);

  private async void ButtonSkipLine_ClickAsync(object? sender, EventArgs e)
  {
    await buttonSkipLine.RunWithHourglassAsync(async () =>
    {
      // ReSharper disable once UseAwaitUsing
      using var improvedStream = GetStream();
      using var textReader = await GetTextReaderAsync(improvedStream).ConfigureAwait(false);
      m_ReadSettings.SkipRows = await textReader.InspectStartRowAsync(m_ReadSettings.FieldDelimiterChar, m_ReadSettings.FieldQualifierChar,
        m_ReadSettings.EscapePrefixChar, m_ReadSettings.CommentLine, m_CancellationTokenSource.Token).ConfigureAwait(false);
    });
  }

  private void CheckBoxColumnsProcess_CheckedChanged(object? sender, EventArgs e)
  {
    if (m_ViewSettings.TryToSolveMoreColumns || m_ViewSettings.AllowRowCombining)
      m_ViewSettings.WarnEmptyTailingColumns = true;
  }

  private void CheckBoxCopySkipped_MouseClick(object sender, MouseEventArgs e)
  {
    if (!checkBoxCopySkipped.Checked)
      m_WriteSettings.SkipRows = 0;
  }

  private void FormEditSettings_FormClosing(object? sender, FormClosingEventArgs e)
  {
    ValidateChildren();
    try { m_CancellationTokenSource.Cancel(); }
    catch
    {
      /* ignore */
    }
    m_ViewSettings.WriteSetting = m_WriteSettings.ToCsvFile();
    SetDefaultInspectionResult();
  }

  /// <summary>
  ///   Handles the Load event of the EditSettings control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
  private void FormEditSettings_Load(object? sender, EventArgs e)
  {
    bindingSourceViewSetting.DataSource = m_ViewSettings;
    bindingSourceWrite.DataSource = m_WriteSettings;
    bindingSourceRead.DataSource = m_ReadSettings;
    fillGuessSettingEdit.FillGuessSettings = m_ViewSettings.FillGuessSettings;

    quotingControlWrite = QuotingControl.AddQuotingControl(tableLayoutPanelWrite, 0, 7, 5, bindingSourceWrite);
    quotingControlWrite.IsWriteSetting = true;

    quotingControlRead = QuotingControl.AddQuotingControl(tableLayoutPanelFile, 0, 9, 7, bindingSourceRead);
    quotingControlRead.IsWriteSetting = false;

    TextBoxFile_Validating(this, new CancelEventArgs(false));
  }
  private Stream GetStream()
  {
#if !SupportPGP
    var sourceAccess = new SourceAccess(m_ReadSettings.FileName, true, "", false, "");
#else
    var sourceAccess = new SourceAccess(FileSetting.FileName, true);
#endif
    return FunctionalDI.GetStream(sourceAccess);
  }

  private Task<ImprovedTextReader> GetTextReaderAsync(Stream stream)
      => stream.GetTextReaderAsync(m_ReadSettings.CodePageId, m_ReadSettings.SkipRows, m_CancellationTokenSource.Token);


  private async void GuessNewline_Click(object? sender, EventArgs e)
  {
    await buttonWriteNewLine.RunWithHourglassAsync(async () =>
    {
      // ReSharper disable once UseAwaitUsing
      using var improvedStream = GetStream();
      using var textReader = await GetTextReaderAsync(improvedStream);
      cboNewLine.SelectedValue =
        await textReader.InspectRecordDelimiterAsync(m_ReadSettings.FieldQualifierChar, m_CancellationTokenSource.Token);
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
      m_ViewSettings.DefaultInspectionResult.CodePageId = m_ReadSettings.CodePageId;
      m_ViewSettings.DefaultInspectionResult.ByteOrderMark = m_ReadSettings.ByteOrderMark;
    }

    if (!m_ViewSettings.GuessEscapePrefix)
      m_ViewSettings.DefaultInspectionResult.EscapePrefix = m_ReadSettings.EscapePrefixChar;

    if (!m_ViewSettings.GuessComment)
      m_ViewSettings.DefaultInspectionResult.CommentLine = m_ReadSettings.CommentLine;

    if (!m_ViewSettings.GuessDelimiter)
      m_ViewSettings.DefaultInspectionResult.FieldDelimiter = m_ReadSettings.FieldDelimiterChar;

    if (!m_ViewSettings.GuessHasHeader)
      m_ViewSettings.DefaultInspectionResult.HasFieldHeader = m_ReadSettings.HasFieldHeader;

    if (!m_ViewSettings.GuessQualifier)
    {
      m_ViewSettings.DefaultInspectionResult.ContextSensitiveQualifier = m_ReadSettings.ContextSensitiveQualifier;
      m_ViewSettings.DefaultInspectionResult.DuplicateQualifierToEscape = m_ReadSettings.DuplicateQualifierToEscape;
      m_ViewSettings.DefaultInspectionResult.FieldQualifier = m_ReadSettings.FieldQualifierChar;
    }

    if (!m_ViewSettings.GuessStartRow)
    {
      m_ViewSettings.DefaultInspectionResult.SkipRows = m_ReadSettings.SkipRows;
      m_ViewSettings.DefaultInspectionResult.SkipRowsAfterHeader = m_ReadSettings.SkipRowsAfterHeader;
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
    var isValidFile = !m_ReadSettings.IsJson
                && !m_ReadSettings.IsXml
                && FileSystemUtils.FileExists(m_ReadSettings.FileName);
    buttonFileInfo.Enabled = isValidFile;
    buttonGuessHeader.Enabled=isValidFile;
    buttonGuessCP.Enabled=isValidFile;
    buttonGuessDelimiter.Enabled=isValidFile;
    buttonGuessLineComment.Enabled=isValidFile;
    buttonSkipLine.Enabled=isValidFile;
    buttonInteractiveSettings.Enabled=isValidFile;
    buttonGuessTextQualifier.Enabled=isValidFile;
    buttonEscapeSequence.Enabled=isValidFile;

    if (!m_ReadSettings.IsJson && !m_ReadSettings.IsXml && !isValidFile)
    {
      errorProvider.SetError(textBoxFileName, "File does not exist.");
      e.Cancel = true;
    }
    else
    {
      errorProvider.SetError(textBoxFileName, string.Empty);
    }
  }

  /// <summary>
  /// Editable setting of CsvFileDummy that will be Observable for Binding
  /// Excluding properties from ViewSetting
  /// </summary>
  private sealed class EditSettings : ObservableObject
  {
    private bool m_ByteOrderMark = true;
    private int m_CodePageId = 65001;
    private string m_CommentLine = string.Empty;
    private bool m_ContextSensitiveQualifier;
    private string m_DelimiterPlaceholder = string.Empty;
    private bool m_DuplicateQualifierToEscape = true;
    private char m_EscapePrefixChar = '\0';
    private char m_FieldDelimiterChar = ',';
    private char m_FieldQualifierChar = '"';
    private string m_FileName = string.Empty;
    private bool m_HasFieldHeader = true;
    private string m_IdentifierInContainer = string.Empty;
    private bool m_IsJson;
    private bool m_IsXml;
    private bool m_KeepUnencrypted;
    private string m_KeyFile = string.Empty;
    private RecordDelimiterTypeEnum m_NewLine = RecordDelimiterTypeEnum.Crlf;
    private string m_NewLinePlaceholder = string.Empty;
    private string m_QualifierPlaceholder = string.Empty;
    private bool m_QualifyAlways;
    private bool m_QualifyOnlyIfNeeded = true;
    private int m_SkipRows;
    private int m_SkipRowsAfterHeader;
    private string m_TreatTextAsNull = "NULL";

    private TrimmingOptionEnum m_TrimmingOption = TrimmingOptionEnum.Unquoted;

    public EditSettings(CsvFileDummy? fileSetting)
    {
      ColumnCollection = new ColumnCollection();
      if (fileSetting!=null)
      {
        ColumnCollection.Overwrite(fileSetting.ColumnCollection);

        // Encoding / IO
        m_CodePageId = fileSetting.CodePageId;
        m_ByteOrderMark = fileSetting.ByteOrderMark;
        m_FileName = fileSetting.FileName;
        m_NewLine = fileSetting.NewLine;
        m_NewLinePlaceholder = fileSetting.NewLinePlaceholder;
        m_TreatTextAsNull = fileSetting.TreatTextAsNull;


        // CSV formatting
        m_FieldDelimiterChar = fileSetting.FieldDelimiterChar;
        m_FieldQualifierChar = fileSetting.FieldQualifierChar;
        m_EscapePrefixChar = fileSetting.EscapePrefixChar;
        m_DuplicateQualifierToEscape = fileSetting.DuplicateQualifierToEscape;
        m_ContextSensitiveQualifier = fileSetting.ContextSensitiveQualifier;
        m_QualifierPlaceholder = fileSetting.QualifierPlaceholder;
        m_DelimiterPlaceholder = fileSetting.DelimiterPlaceholder;
        m_QualifyAlways = fileSetting.QualifyAlways;
        m_QualifyOnlyIfNeeded = fileSetting.QualifyOnlyIfNeeded;
        m_TrimmingOption = fileSetting.TrimmingOption;

        // Records / layout
        m_HasFieldHeader = fileSetting.HasFieldHeader;
        m_SkipRows = fileSetting.SkipRows;
        m_SkipRowsAfterHeader = fileSetting.SkipRowsAfterHeader;
        m_CommentLine = fileSetting.CommentLine;

        // Identification / container
        m_IdentifierInContainer = fileSetting.IdentifierInContainer;

        // Format flags
        m_IsJson = fileSetting.IsJson;
        m_IsXml = fileSetting.IsXml;

        // Security
        m_KeepUnencrypted = fileSetting.KeepUnencrypted;
        m_KeyFile = fileSetting.KeyFile;
      }

      // Not needed here asn teh Ui doe not show columns (yet)
      // ColumnCollection.CollectionChanged += (_, _) => NotifyPropertyChanged(nameof(ColumnCollection));
    }

    public bool ByteOrderMark
    {
      get => m_ByteOrderMark;
      set => SetProperty(ref m_ByteOrderMark, value);
    }

    public int CodePageId
    {
      get => m_CodePageId;
      set => SetProperty(ref m_CodePageId, value);
    }

    public ColumnCollection ColumnCollection { get; }

    public string CommentLine
    {
      get => m_CommentLine;
      set => SetProperty(ref m_CommentLine, value);
    }

    public bool ContextSensitiveQualifier
    {
      get => m_ContextSensitiveQualifier;
      set => SetProperty(ref m_ContextSensitiveQualifier, value);
    }

    public string DelimiterPlaceholder
    {
      get => m_DelimiterPlaceholder;
      set => SetProperty(ref m_DelimiterPlaceholder, value);
    }

    public bool DuplicateQualifierToEscape
    {
      get => m_DuplicateQualifierToEscape;
      set => SetProperty(ref m_DuplicateQualifierToEscape, value);
    }

    public char EscapePrefixChar
    {
      get => m_EscapePrefixChar;
      set => SetProperty(ref m_EscapePrefixChar, value);
    }

    public char FieldDelimiterChar
    {
      get => m_FieldDelimiterChar;
      set => SetProperty(ref m_FieldDelimiterChar, value);
    }

    public char FieldQualifierChar
    {
      get => m_FieldQualifierChar;
      set => SetProperty(ref m_FieldQualifierChar, value);
    }

    public string FileName
    {
      get => m_FileName;
      set => SetProperty(ref m_FileName, value);
    }

    public bool HasFieldHeader
    {
      get => m_HasFieldHeader;
      set => SetProperty(ref m_HasFieldHeader, value);
    }

    public string IdentifierInContainer
    {
      get => m_IdentifierInContainer;
      set => SetProperty(ref m_IdentifierInContainer, value);
    }

    public bool IsJson
    {
      get => m_IsJson;
      set => SetProperty(ref m_IsJson, value);
    }

    public bool IsXml
    {
      get => m_IsXml;
      set => SetProperty(ref m_IsXml, value);
    }

    public bool KeepUnencrypted
    {
      get => m_KeepUnencrypted;
      set => SetProperty(ref m_KeepUnencrypted, value);
    }

    public string KeyFile
    {
      get => m_KeyFile;
      set => SetProperty(ref m_KeyFile, value);
    }

    public RecordDelimiterTypeEnum NewLine
    {
      get => m_NewLine;
      set => SetProperty(ref m_NewLine, value);
    }

    public string NewLinePlaceholder
    {
      get => m_NewLinePlaceholder;
      set => SetProperty(ref m_NewLinePlaceholder, value);
    }

    public string QualifierPlaceholder
    {
      get => m_QualifierPlaceholder;
      set => SetProperty(ref m_QualifierPlaceholder, value);
    }

    public bool QualifyAlways
    {
      get => m_QualifyAlways;
      set => SetProperty(ref m_QualifyAlways, value);
    }

    public bool QualifyOnlyIfNeeded
    {
      get => m_QualifyOnlyIfNeeded;
      set => SetProperty(ref m_QualifyOnlyIfNeeded, value);
    }

    public int SkipRows
    {
      get => m_SkipRows;
      set => SetProperty(ref m_SkipRows, value);
    }

    public int SkipRowsAfterHeader
    {
      get => m_SkipRowsAfterHeader;
      set => SetProperty(ref m_SkipRowsAfterHeader, value);
    }

    public string TreatTextAsNull
    {
      get => m_TreatTextAsNull;
      set => SetProperty(ref m_TreatTextAsNull, value);
    }

    public TrimmingOptionEnum TrimmingOption
    {
      get => m_TrimmingOption;
      set => SetProperty(ref m_TrimmingOption, value);
    }

    public CsvFileDummy ToCsvFile()
    {
      var fileSetting = new CsvFileDummy();
      fileSetting.ColumnCollection.Overwrite(ColumnCollection);
      // Encoding / IO
      fileSetting.ByteOrderMark = ByteOrderMark;
      fileSetting.CodePageId = CodePageId;
      fileSetting.FileName = FileName;
      fileSetting.NewLine = NewLine;
      fileSetting.NewLinePlaceholder = NewLinePlaceholder;

      // CSV formatting
      fileSetting.FieldDelimiterChar = FieldDelimiterChar;
      fileSetting.FieldQualifierChar = FieldQualifierChar;
      fileSetting.EscapePrefixChar = EscapePrefixChar;
      fileSetting.DuplicateQualifierToEscape = DuplicateQualifierToEscape;
      fileSetting.ContextSensitiveQualifier = ContextSensitiveQualifier;
      fileSetting.QualifierPlaceholder = QualifierPlaceholder;
      fileSetting.DelimiterPlaceholder = DelimiterPlaceholder;
      fileSetting.QualifyAlways = QualifyAlways;
      fileSetting.QualifyOnlyIfNeeded = QualifyOnlyIfNeeded;
      fileSetting.TrimmingOption = TrimmingOption;
      fileSetting.TreatTextAsNull = TreatTextAsNull;

      // Records / layout
      fileSetting.HasFieldHeader = HasFieldHeader;
      fileSetting.SkipRows = SkipRows;
      fileSetting.SkipRowsAfterHeader = SkipRowsAfterHeader;
      fileSetting.CommentLine = CommentLine;

      // Identification / container
      fileSetting.IdentifierInContainer = IdentifierInContainer;

      // Format flags
      fileSetting.IsJson = IsJson;
      fileSetting.IsXml = IsXml;

      // Security
      fileSetting.KeepUnencrypted = KeepUnencrypted;
      fileSetting.KeyFile = KeyFile;

      return fileSetting;
    }
  }
}