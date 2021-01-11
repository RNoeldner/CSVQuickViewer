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

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormEditSettings" /> class.
    /// </summary>
    public FormEditSettings() : this(new ViewSettings())
    {
    }

    public FormEditSettings(ViewSettings viewSettings)
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

    private async void BtnOpenFile_ClickAsync(object sender, EventArgs e)
    {
      try
      {
        var split = FileSystemUtils.SplitPath(textBoxFile.Text);
        var newFileName = WindowsAPICodePackWrapper.Open(
          split.DirectoryName,
          "Delimited File",
          "Delimited files (*.csv;*.txt;*.tab;*.tsv)|*.csv;*.txt;*.tab;*.tsv|All files (*.*)|*.*",
          split.FileName);
        if (!string.IsNullOrEmpty(newFileName))
          await ChangeFileNameAsync(newFileName);
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
        m_CancellationTokenSource?.Cancel();
        m_CancellationTokenSource?.Dispose();
      }

      base.Dispose(disposing);
    }

    private async void ButtonGuessCP_ClickAsync(object sender, EventArgs e)
    {
      await buttonGuessCP.RunWithHourglassAsync(async () =>
      {
        using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(m_ViewSettings)))
        {
          var (codepage, bom) = await CsvHelper.GuessCodePageFromStream(improvedStream, m_CancellationTokenSource.Token);
          m_ViewSettings.CodePageId = codepage;
          m_ViewSettings.ByteOrderMark = bom;
        }
      });
      fileSettingBindingSource.ResetBindings(false);
    }

    private async void ButtonGuessDelimiter_ClickAsync(object sender, EventArgs e)
    {
      await buttonGuessDelimiter.RunWithHourglassAsync(async () =>
      {
        using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(m_ViewSettings)))
        {
          await CsvHelper.GuessDelimiterFromStream(improvedStream, m_ViewSettings.CodePageId, m_ViewSettings.SkipRows, m_ViewSettings.FileFormat.EscapeCharacter, m_CancellationTokenSource.Token);
        }
      });

      // GuessDelimiterAsync does set the values, refresh them
      fileFormatBindingSource.ResetBindings(false);
    }

    private async void ButtonGuessTextQualifier_Click(object sender, EventArgs e)
    {
      var qualifier = string.Empty;
      await buttonGuessTextQualifier.RunWithHourglassAsync(async () =>
      {
        using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(m_ViewSettings)))
          qualifier = await CsvHelper.GuessQualifierFromStream(improvedStream, m_ViewSettings.CodePageId, m_ViewSettings.SkipRows, m_ViewSettings.FileFormat.FieldDelimiter, m_CancellationTokenSource.Token);
      });
      if (qualifier != null)
        m_ViewSettings.FileFormat.FieldQualifier = qualifier;
      else
        _MessageBox.Show("No Column Qualifier found", "Qualifier", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void ButtonSkipLine_ClickAsync(object sender, EventArgs e)
    {
      await buttonSkipLine.RunWithHourglassAsync(async () =>
      {
        using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(m_ViewSettings)))
          m_ViewSettings.SkipRows = await CsvHelper.GuessStartRowFromStream(improvedStream, m_ViewSettings.CodePageId, m_ViewSettings.FileFormat.FieldDelimiter, m_ViewSettings.FileFormat.FieldQualifier, m_ViewSettings.FileFormat.CommentLine, m_CancellationTokenSource.Token);
      });
    }

    private void CboCodePage_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cboCodePage.SelectedItem != null)
        m_ViewSettings.CodePageId = ((DisplayItem<int>) cboCodePage.SelectedItem).ID;
    }

    private void CboRecordDelimiter_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cboRecordDelimiter.SelectedItem != null)
        m_ViewSettings.FileFormat.NewLine = (RecordDelimiterType) cboRecordDelimiter.SelectedValue;
    }

    private async Task ChangeFileNameAsync(string newFileName)
    {
      m_ViewSettings.FileName = newFileName;
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        using (var processDisplay = new CustomProcessDisplay(m_CancellationTokenSource.Token))
        {
          var res = await CsvHelper.GetDetectionResultFromFile(newFileName, processDisplay);
          m_ViewSettings.FileFormat.FieldDelimiter = res.FieldDelimiter;
          m_ViewSettings.CodePageId = res.CodePageId;
          m_ViewSettings.ByteOrderMark = res.ByteOrderMark;
          m_ViewSettings.SkipRows = res.SkipRows;
          m_ViewSettings.HasFieldHeader = res.HasFieldHeader;

          if (MessageBox.Show(
            this,
            @"Should the value format of the columns be analyzed?",
            @"Value Format",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question) == DialogResult.Yes)
          {
            if (m_ViewSettings.ColumnCollection.Count > 0 && MessageBox.Show(
              this,
              @"Any already typed value will not be analyzed.
 Should the existing formats be removed before doing so?",
              @"Value Format",
              MessageBoxButtons.YesNo,
              MessageBoxIcon.Question) == DialogResult.Yes)
              m_ViewSettings.ColumnCollection.Clear();
            try
            {
              Logger.Debug("Determining column format by reading samples");

              await m_ViewSettings.FillGuessColumnFormatReaderAsync(
                false,
                false,
                m_ViewSettings.FillGuessSettings,
                m_CancellationTokenSource.Token);
            }
            catch (Exception exc)
            {
              this.ShowError(exc);
            }
          }
        }

        m_ViewSettings.ColumnCollection.Clear();
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private void CheckBoxColumnsProcess_CheckedChanged(object sender, EventArgs e)
    {
      if (m_ViewSettings.TryToSolveMoreColumns || m_ViewSettings.AllowRowCombining)
        m_ViewSettings.WarnEmptyTailingColumns = true;
    }

    /// <summary>
    ///   Handles the Load event of the EditSettings control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void EditSettings_Load(object sender, EventArgs e)
    {
      fileSettingBindingSource.DataSource = m_ViewSettings;
      fileFormatBindingSource.DataSource = m_ViewSettings.FileFormat;

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

      var selValue = (int) m_ViewSettings.FileFormat.NewLine;
      cboRecordDelimiter.SuspendLayout();
      cboRecordDelimiter.DataSource = di;
      cboRecordDelimiter.DisplayMember = nameof(DisplayItem<int>.Display);
      cboRecordDelimiter.ValueMember = nameof(DisplayItem<int>.ID);
      cboRecordDelimiter.SelectedValue = selValue;
      cboRecordDelimiter.ResumeLayout(true);

      quotingControl.CsvFile = m_ViewSettings;
    }

    private void FormEditSettings_FormClosing(object sender, FormClosingEventArgs e)
    {
      m_CancellationTokenSource.Cancel();
      ValidateChildren();
    }

    private async void GuessNewline_Click(object sender, EventArgs e)
    {
      await buttonNewLine.RunWithHourglassAsync(async () =>
      {
        using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(m_ViewSettings)))
          cboRecordDelimiter.SelectedValue = (int) await CsvHelper.GuessNewlineFromStream(improvedStream, m_ViewSettings.CodePageId, m_ViewSettings.SkipRows, m_ViewSettings.FileFormat.FieldQualifier, m_CancellationTokenSource.Token);
      });
    }

    private void PositiveNumberValidating(object sender, CancelEventArgs e)
    {
      if (!(sender is TextBox tb)) return;

      var ok = int.TryParse(tb.Text, out var parse);
      var reformat = parse.ToString(CultureInfo.CurrentCulture);
      ok = ok && parse >= 0 && reformat == tb.Text;

      if (!ok)
      {
        errorProvider.SetError(tb, "Must be a positive number ");
        e.Cancel = true;
      }
      else
      {
        errorProvider.SetError(tb, string.Empty);
      }
    }

    private void TextBoxDelimiter_TextChanged(object sender, EventArgs e)
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

    private void TextBoxFile_Validating(object sender, CancelEventArgs e)
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

    private void DomainUpDownTime_SelectedItemChanged(object sender, EventArgs e)
    {
      switch (domainUpDownTime.SelectedIndex)
      {
        case 4:
          m_ViewSettings.LimitDuration = ViewSettings.DurationEnum.Unlimited;
          break;

        case 3:
          m_ViewSettings.LimitDuration = ViewSettings.DurationEnum.TenSecond;
          break;

        case 2:
          m_ViewSettings.LimitDuration = ViewSettings.DurationEnum.TwoSecond;
          break;

        case 1:
          m_ViewSettings.LimitDuration = ViewSettings.DurationEnum.Second;
          break;

        case 0:
          m_ViewSettings.LimitDuration = ViewSettings.DurationEnum.HalfSecond;
          break;
      }
    }

    private async void ButtonGuessHeader_Click(object sender, EventArgs e)
    {
      await buttonGuessHeader.RunWithHourglassAsync(async () =>
      {
        using (var improvedStream = FunctionalDI.OpenStream(new SourceAccess(m_ViewSettings)))
        {
          var res = await CsvHelper.GuessHasHeaderFromStream(improvedStream, m_ViewSettings.CodePageId, m_ViewSettings.SkipRows, m_ViewSettings.FileFormat.CommentLine, m_ViewSettings.FileFormat.FieldDelimiter, m_CancellationTokenSource.Token);
          m_ViewSettings.HasFieldHeader= res.Item1;
          fileSettingBindingSource.ResetBindings(false);
          _MessageBox.Show(res.Item2, "Checking headers");
        }
      });
    }

    private void ButtonInteractiveSettings_Click(object sender, EventArgs e)
    {
      using (var frm = new FindSkipRows(m_ViewSettings)) _=frm.ShowDialog();
    }
  }
}