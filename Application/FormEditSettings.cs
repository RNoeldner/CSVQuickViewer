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
    private bool m_IsDisposed;

    public FormEditSettings(in ViewSettings viewSettings)
    {
      InitializeComponent();
      m_ViewSettings = viewSettings??throw new ArgumentNullException(nameof(viewSettings));
      fillGuessSettingEdit.FillGuessSettings = viewSettings.FillGuessSettings;

      if (m_ViewSettings.LimitDuration == ViewSettings.DurationEnum.Unlimited)
        domainUpDownTime.SelectedIndex = 4;
      else if (m_ViewSettings.LimitDuration == ViewSettings.DurationEnum.TenSecond)
        domainUpDownTime.SelectedIndex = 3;
      else if (m_ViewSettings.LimitDuration == ViewSettings.DurationEnum.TwoSecond)
        domainUpDownTime.SelectedIndex = 2;
      else if (m_ViewSettings.LimitDuration == ViewSettings.DurationEnum.Second)
        domainUpDownTime.SelectedIndex = 1;
      else
        domainUpDownTime.SelectedIndex = 0;
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

        if (newFileName is null || newFileName.Length==0)
          return;
        m_ViewSettings.FileName = newFileName;
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
        using var improvedStream = new ImprovedStream(new SourceAccess(m_ViewSettings));
        var (codepage, bom) = await improvedStream.GuessCodePage(m_CancellationTokenSource.Token);
        m_ViewSettings.CodePageId = codepage;
        m_ViewSettings.ByteOrderMark = bom;
      });
      fileSettingBindingSource.ResetBindings(false);
    }

    private async void ButtonGuessDelimiter_ClickAsync(object? sender, EventArgs e)
    {
      await buttonGuessDelimiter.RunWithHourglassAsync(async () =>
      {
        using var improvedStream = new ImprovedStream(new SourceAccess(m_ViewSettings));
        var res = await improvedStream.GuessDelimiter(m_ViewSettings.CodePageId, m_ViewSettings.SkipRows, m_ViewSettings.EscapePrefix, m_CancellationTokenSource.Token);
        if (res.Item2)
          m_ViewSettings.FieldDelimiter = res.Item1;
      });
      
    }

    private async void ButtonGuessTextQualifier_Click(object? sender, EventArgs e)
    {
      var qualifier = string.Empty;
      await buttonGuessTextQualifier.RunWithHourglassAsync(async () =>
      {
        using var improvedStream = new ImprovedStream(new SourceAccess(m_ViewSettings));
        qualifier = await improvedStream.GuessQualifier(m_ViewSettings.CodePageId, m_ViewSettings.SkipRows, m_ViewSettings.FieldDelimiter, m_CancellationTokenSource.Token);
      });

      m_ViewSettings.FieldQualifier = qualifier;
    }

    private async void ButtonSkipLine_ClickAsync(object? sender, EventArgs e)
    {
      await buttonSkipLine.RunWithHourglassAsync(async () =>
      {
        using var improvedStream = new ImprovedStream(new SourceAccess(m_ViewSettings));
        m_ViewSettings.SkipRows = await improvedStream.GuessStartRow(m_ViewSettings.CodePageId, m_ViewSettings.FieldDelimiter, m_ViewSettings.FieldQualifier, m_ViewSettings.CommentLine, m_CancellationTokenSource.Token);
      });
    }

    private void CboCodePage_SelectedIndexChanged(object? sender, EventArgs e)
    {
      if (cboCodePage.SelectedItem != null)
        m_ViewSettings.CodePageId = ((DisplayItem<int>) cboCodePage.SelectedItem).ID;
    }

    private void CboRecordDelimiter_SelectedIndexChanged(object? sender, EventArgs e)
    {
      if (cboRecordDelimiter.SelectedItem != null)
        m_ViewSettings.NewLine = (RecordDelimiterType) cboRecordDelimiter.SelectedValue;
    }

    private void CheckBoxColumnsProcess_CheckedChanged(object? sender, EventArgs e)
    {
      if (m_ViewSettings.TryToSolveMoreColumns || m_ViewSettings.AllowRowCombining)
        m_ViewSettings.WarnEmptyTailingColumns = true;
    }

    /// <summary>
    ///   Handles the Load event of the EditSettings control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void EditSettings_Load(object? sender, EventArgs e)
    {
      fileSettingBindingSource.DataSource = m_ViewSettings;      

      // Fill Drop down
      cboCodePage.SuspendLayout();

      cboCodePage.DataSource = EncodingHelper.CommonCodePages.Select(cp => new DisplayItem<int>(cp, EncodingHelper.GetEncodingName(cp, false))).ToList();
      cboRecordDelimiter.DisplayMember = nameof(DisplayItem<int>.Display);
      cboRecordDelimiter.ValueMember = nameof(DisplayItem<int>.ID);
      cboRecordDelimiter.SelectedValue = m_ViewSettings.CodePageId;
      cboCodePage.ResumeLayout(true);

      var descConv = new EnumDescriptionConverter(typeof(RecordDelimiterType));
      var di = (from RecordDelimiterType item in Enum.GetValues(typeof(RecordDelimiterType))
                select new DisplayItem<int>((int) item, descConv.ConvertToString(item))).ToList();

      var selValue = (int) m_ViewSettings.NewLine;
      cboRecordDelimiter.SuspendLayout();
      cboRecordDelimiter.DataSource = di;
      cboRecordDelimiter.DisplayMember = nameof(DisplayItem<int>.Display);
      cboRecordDelimiter.ValueMember = nameof(DisplayItem<int>.ID);
      cboRecordDelimiter.SelectedValue = selValue;
      cboRecordDelimiter.ResumeLayout(true);

      quotingControl.CsvFile = m_ViewSettings;
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
        using var improvedStream = new ImprovedStream(new SourceAccess(m_ViewSettings));
        cboRecordDelimiter.SelectedValue = (int) await improvedStream.GuessNewline(m_ViewSettings.CodePageId, m_ViewSettings.SkipRows, m_ViewSettings.FieldQualifier, m_CancellationTokenSource.Token);
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
      m_ViewSettings.LimitDuration = domainUpDownTime.SelectedIndex switch
      {
        4 => ViewSettings.DurationEnum.Unlimited,
        3 => ViewSettings.DurationEnum.TenSecond,
        2 => ViewSettings.DurationEnum.TwoSecond,
        1 => ViewSettings.DurationEnum.Second,
        0 => ViewSettings.DurationEnum.HalfSecond,
        _ => m_ViewSettings.LimitDuration
      };
    }

    private async void ButtonGuessHeader_Click(object? sender, EventArgs e)
    {
      await buttonGuessHeader.RunWithHourglassAsync(async () =>
      {
        using var improvedStream = new ImprovedStream(new SourceAccess(m_ViewSettings));
        var res = await improvedStream.GuessHasHeader(m_ViewSettings.CodePageId, m_ViewSettings.SkipRows, m_ViewSettings.CommentLine, m_ViewSettings.FieldDelimiter, m_CancellationTokenSource.Token);
        m_ViewSettings.HasFieldHeader= res.Item1;
        fileSettingBindingSource.ResetBindings(false);
        _MessageBox.Show(res.Item2, "Checking headers");
      });
    }

    private void ButtonInteractiveSettings_Click(object? sender, EventArgs e)
    {
      using var frm = new FindSkipRows(m_ViewSettings);
      _=frm.ShowDialog();
    }

    private async void buttonGuessLineComment_Click(object? sender, EventArgs e)
    {
      await buttonGuessLineComment.RunWithHourglassAsync(async () =>
      {
        using var improvedStream = new ImprovedStream(new SourceAccess(m_ViewSettings));
        m_ViewSettings.CommentLine = await improvedStream.GuessLineComment(m_ViewSettings.CodePageId, m_ViewSettings.SkipRows, m_CancellationTokenSource.Token);
      });      
    }
  }
}