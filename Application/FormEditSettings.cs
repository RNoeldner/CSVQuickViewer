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

namespace CsvTools
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Globalization;
  using System.Threading;
  using System.Windows.Forms;

  /// <summary>
  ///   Form to edit the Settings
  /// </summary>
  public partial class FormEditSettings : ResizeForm
  {
    private readonly ViewSettings m_ViewSettings;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormEditSettings" /> class.
    /// </summary>
    public FormEditSettings() : this(new ViewSettings())
    {
    }

    public FormEditSettings(ViewSettings viewSettings)
    {
      InitializeComponent();
      m_ViewSettings = viewSettings;
      fillGuessSettingEdit.FillGuessSettings = viewSettings.FillGuessSettings;
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

    private async void ButtonGuessCP_ClickAsync(object sender, EventArgs e)
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        await CsvHelper.GuessCodePageAsync(m_ViewSettings);
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private async void ButtonGuessDelimiter_ClickAsync(object sender, EventArgs e)
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        m_ViewSettings.FileFormat.FieldDelimiter = await CsvHelper.GuessDelimiterAsync(m_ViewSettings, CancellationToken.None);
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private async void ButtonSkipLine_ClickAsync(object sender, EventArgs e)
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        m_ViewSettings.SkipRows = await CsvHelper.GuessStartRowAsync(m_ViewSettings, CancellationToken.None);
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private void CboCodePage_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (cboCodePage.SelectedItem != null)
        m_ViewSettings.CodePageId = ((DisplayItem<int>)cboCodePage.SelectedItem).ID;
    }

    private async System.Threading.Tasks.Task ChangeFileNameAsync(string newFileName)
    {
      m_ViewSettings.FileName = newFileName.GetRelativePath(ApplicationSetting.RootFolder);
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        var csvFile = new CsvFile(newFileName);
        using (var processDisplay = new DummyProcessDisplay())
        {
          await csvFile.RefreshCsvFileAsync(processDisplay);
        }

        m_ViewSettings.FileFormat.FieldDelimiter = csvFile.FileFormat.FieldDelimiter;
        m_ViewSettings.CodePageId = csvFile.CodePageId;
        m_ViewSettings.ByteOrderMark = csvFile.ByteOrderMark;
        m_ViewSettings.SkipRows = csvFile.SkipRows;
        m_ViewSettings.HasFieldHeader = csvFile.HasFieldHeader;

        if (MessageBox.Show(
              this,
              "Should the value format of the columns be analyzed?",
              "Value Format",
              MessageBoxButtons.YesNo,
              MessageBoxIcon.Question) == DialogResult.Yes)
        {
          if (m_ViewSettings.ColumnCollection.Count > 0 && MessageBox.Show(
                this,
                "Any already typed value will not be analyzed.\r\n Should the existing formats be removed before doing so?",
                "Value Format",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            m_ViewSettings.ColumnCollection.Clear();
          try
          {
            using (var processDisplay = m_ViewSettings.GetProcessDisplay(this, true, CancellationToken.None))
            {
              await m_ViewSettings.FillGuessColumnFormatReaderAsync(
                false,
                false,
                m_ViewSettings.FillGuessSettings,
                processDisplay);
            }
          }
          catch (Exception exc)
          {
            this.ShowError(exc);
          }
        }

        m_ViewSettings.ColumnCollection.Clear();
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private void CsvFile_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(ViewSettings.CodePageId))
      {
        foreach (var ite in cboCodePage.Items)
          if (((DisplayItem<int>)ite).ID == m_ViewSettings.CodePageId)
          {
            cboCodePage.SelectedItem = ite;
            break;
          }
        return;
      }
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
      foreach (var cp in EncodingHelper.CommonCodePages)
        cboCodePage.Items.Add(new DisplayItem<int>(cp, EncodingHelper.GetEncodingName(cp, false, false)));

      var di = new List<DisplayItem<int>>();
      var descConv = new EnumDescriptionConverter(typeof(RecordDelimiterType));
      foreach (RecordDelimiterType item in Enum.GetValues(typeof(RecordDelimiterType)))
      {
        di.Add(new DisplayItem<int>((int)item, descConv.ConvertToString(item)));
      }

      var selValue = (int)m_ViewSettings.FileFormat.NewLine;
      cboRecordDelimiter.DataSource = di;
      cboRecordDelimiter.DisplayMember = nameof(DisplayItem<int>.Display);
      cboRecordDelimiter.ValueMember = nameof(DisplayItem<int>.ID);
      cboRecordDelimiter.SelectedValue = selValue;

      quotingControl.CsvFile = m_ViewSettings;
      
      CsvFile_PropertyChanged(null, new PropertyChangedEventArgs(nameof(ViewSettings.CodePageId)));
    }

    private void FormEditSettings_FormClosing(object sender, FormClosingEventArgs e) => ValidateChildren();

    private void PositiveNumberValidating(object sender, CancelEventArgs e)
    {
      var tb = sender as TextBox;

      Debug.Assert(tb != null, nameof(tb) + " != null");
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
        var delim = FileFormat.GetChar(textBoxDelimiter.Text);

        if (delim != ';' && delim != ',' && delim != '|' && delim != ':' && delim != '\t')
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

    private async void GuessNewline_Click(object sender, EventArgs e)
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        m_ViewSettings.FileFormat.NewLine = await CsvHelper.GuessNewlineAsync(m_ViewSettings, CancellationToken.None);
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private void cboRecordDelimiter_SelectedIndexChanged(object sender, EventArgs e)

    {
      if (cboRecordDelimiter.SelectedItem != null)
        m_ViewSettings.FileFormat.NewLine = (RecordDelimiterType)cboRecordDelimiter.SelectedValue;
    }
  }
}