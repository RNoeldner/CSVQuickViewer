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

using CsvTools.Properties;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Form to edit the Settings
  /// </summary>
  public partial class FormEditSettings : Form
  {
    private readonly CsvFile m_CsvFileCopy = new CsvFile();
    private readonly CsvFile m_CsvFileRef;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormEditSettings" /> class.
    /// </summary>
    public FormEditSettings(CsvFile fileSettings)
    {
      Contract.Requires(fileSettings != null);
      m_CsvFileRef = fileSettings;
      fileSettings.CopyTo(m_CsvFileCopy);
      m_CsvFileCopy.PropertyChanged += CsvFile_PropertyChanged;

      InitializeComponent();
      GetPrivateKeys();
    }

    private void BtnOpenFile_Click(object sender, EventArgs e)
    {
      try
      {
        using (var openFileDialog = new OpenFileDialog())
        {
          openFileDialog.Filter =
            "Delimited files (*.csv;*.txt;*.tab;*.tsv)|*.csv;*.txt;*.tab;*.tsv|All files (*.*)|*.*";
          openFileDialog.Title = "Delimited File";

          openFileDialog.InitialDirectory = textBoxFile.Text.GetDirectoryName();
          if (openFileDialog.ShowDialog(this) == DialogResult.OK) ChangeFileName(openFileDialog.FileName);
        }
      }
      catch (Exception exc)
      {
        MessageBox.Show(this, exc.ExceptionMessages(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
    }

    private void ButtonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void ButtonGuessCP_Click(object sender, EventArgs e)
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        CsvHelper.GuessCodePage(m_CsvFileCopy);
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private void ButtonGuessDelimiter_Click(object sender, EventArgs e)
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        m_CsvFileCopy.FileFormat.FieldDelimiter = CsvHelper.GuessDelimiter(m_CsvFileCopy);
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    /// <summary>
    ///   Handles the Click event of the buttonOK control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void ButtonOK_Click(object sender, EventArgs e)
    {
      fillGuessSettingEdit.BeforeClose();

      if (!m_CsvFileCopy.Equals(m_CsvFileRef))
        m_CsvFileCopy.CopyTo(m_CsvFileRef);
      Settings.Default.UseFileSettings = chkUseFileSettings.Checked;
      Settings.Default.TrimmingOptions = quotingControl.CsvFile.TrimmingOption.ToString();
      Settings.Default.WarnQuotes = checkBoxWarnQuotes.Checked;
      Settings.Default.WarnNBSP = checkBoxWarnNBSP.Checked;
      Settings.Default.WarnEmptyTailingColumns = checkBoxWarnEmptyTailingColumns.Checked;
      Settings.Default.WarnUnknowCharater = checkBoxWarnUnknowCharater.Checked;
      Settings.Default.WarnDelimiterInValue = checkBoxWarnDelimiterInValue.Checked;
      Settings.Default.DisplayStartLineNo = checkBoxDisplayStartLineNo.Checked;
      Settings.Default.TreatTextAsNull = textBoxTextAsNull.Text;
      Settings.Default.AlternateQuoting = quotingControl.CsvFile.AlternateQuoting;
      Settings.Default.SkipEmptyLines = checkBoxSkipEmptyLines.Checked;
      Settings.Default.GuessCodePage = checkBoxGuessCodePage.Checked;
      Settings.Default.GuessStartRow = checkBoxGuessStartRow.Checked;
      Settings.Default.GuessHasHeader = checkBoxGuessHasHeader.Checked;
      Settings.Default.DetectFileChanges = checkBoxDetectFileChanges.Checked;
      Settings.Default.WarnLineFeed = checkBoxWarnLineFeed.Checked;
      Settings.Default.GuessDelimiter = checkBoxGuessDelimiter.Checked;
      Settings.Default.Comment = textBoxComment.Text;
      Settings.Default.DelimiterPlaceholder = textBoxDelimiterPlaceholder.Text;
      Settings.Default.NLPlaceholder = textBoxNLPlaceholder.Text;
      Settings.Default.TreatUnknowCharaterAsSpace = checkBoxTreatUnknowCharaterAsSpace.Checked;
      Settings.Default.TreatNBSPAsSpace = checkBoxTreatNBSPAsSpace.Checked;
      Settings.Default.EscapeCharacter = quotingControl.CsvFile.FileFormat.EscapeCharacter;
      Settings.Default.QuotePlaceholder = quotingControl.CsvFile.FileFormat.QuotePlaceholder;
      Settings.Default.MenuDown = checkBoxMenuDown.Checked;

      Settings.Default.DectectPercentage = ApplicationSetting.FillGuessSettings.DectectPercentage;
      Settings.Default.DetectBoolean = ApplicationSetting.FillGuessSettings.DetectBoolean;
      Settings.Default.DectectNumbers = ApplicationSetting.FillGuessSettings.DectectNumbers;
      Settings.Default.DetectDateTime = ApplicationSetting.FillGuessSettings.DetectDateTime;
      Settings.Default.DetectGUID = ApplicationSetting.FillGuessSettings.DetectGUID;
      Settings.Default.ExcelSerialDateTime = ApplicationSetting.FillGuessSettings.SerialDateTime;
      Settings.Default.TrueValue = ApplicationSetting.FillGuessSettings.TrueValue;
      Settings.Default.FalseValue = ApplicationSetting.FillGuessSettings.FalseValue;
      Settings.Default.DateTimeValue = ApplicationSetting.FillGuessSettings.DateTimeValue;
      Settings.Default.IgnoreIdColums = ApplicationSetting.FillGuessSettings.IgnoreIdColums;
      Settings.Default.SampleValues = ApplicationSetting.FillGuessSettings.SampleValues;
      Settings.Default.CheckedRecords = ApplicationSetting.FillGuessSettings.CheckedRecords;
      Settings.Default.MinSamplesForIntDate = ApplicationSetting.FillGuessSettings.MinSamplesForIntDate;
      Settings.Default.CheckNamedDates = ApplicationSetting.FillGuessSettings.CheckNamedDates;
      Settings.Default.DateParts = ApplicationSetting.FillGuessSettings.DateParts;
      Settings.Default.DefaultPassphrase = ApplicationSetting.ToolSetting.PGPInformation.EncryptedPassphase;
      Settings.Default.PrivateKey = new StringCollection();
      Settings.Default.PrivateKey.AddRange(ApplicationSetting.ToolSetting.PGPInformation.PrivateKeys);
      Settings.Default.Save();
      if (int.TryParse(textBoxNumWarnings.Text, out int val))
        Settings.Default.NumWarnings = val;

      Settings.Default.Save();
      Close();
    }

    private void ButtonSkipLine_Click(object sender, EventArgs e)
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        m_CsvFileCopy.SkipRows = CsvHelper.GuessStartRow(m_CsvFileCopy);
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private void ChangeFileName(string newFileName)
    {
      m_CsvFileCopy.FileName = newFileName.GetRelativePath(ApplicationSetting.ToolSetting.RootFolder);
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        var csvFile = new CsvFile(newFileName);
        csvFile.GetEncryptedPassphraseFunction = () => csvFile.GetEncryptedPassphraseFunction();
        using (var dummy = new DummyProcessDisplay())
        {
          csvFile.RefreshCsvFile(dummy);
        }

        m_CsvFileCopy.FileFormat.FieldDelimiter = csvFile.FileFormat.FieldDelimiter;
        m_CsvFileCopy.CodePageId = csvFile.CodePageId;
        m_CsvFileCopy.ByteOrderMark = csvFile.ByteOrderMark;
        m_CsvFileCopy.SkipRows = csvFile.SkipRows;
        m_CsvFileCopy.HasFieldHeader = csvFile.HasFieldHeader;
        m_CsvFileCopy.Passphrase = csvFile.Passphrase;
        if (MessageBox.Show(this, "Should the value format of the columns be analyzed?", "Value Format",
              MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
          if (m_CsvFileCopy.Column.Count > 0 &&
              MessageBox.Show(this,
                "Any already typed value will not be analyzed.\r\n Should the existing formats be removed before doing so?",
                "Value Format", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            m_CsvFileCopy.Column.Clear();
          try
          {
            using (var processDisplay = m_CsvFileRef.GetProcessDisplay(this))
            {
              m_CsvFileCopy.FillGuessColumnFormatReader(false, processDisplay);
            }
          }
          catch (Exception exc)
          {
            _MessageBox.Show(this, exc.ExceptionMessages(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
          }
        }

        m_CsvFileCopy.Column.Clear();
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    /// <summary>
    ///   Handles the Load event of the EditSettings control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void EditSettings_Load(object sender, EventArgs e)
    {
      fileSettingBindingSource.DataSource = m_CsvFileCopy;
      fileFormatBindingSource.DataSource = m_CsvFileCopy.FileFormat;

      // Fill Drop down
      foreach (var cp in EncodingHelper.CommonCodePages)
        cboCodePage.Items.Add(new DisplayItem<int>(cp, EncodingHelper.GetEncodingName(cp, false, false)));
      quotingControl.CsvFile = m_CsvFileCopy;
      CsvFile_PropertyChanged(null, new PropertyChangedEventArgs("CodePageId"));
      chkUseFileSettings.Checked = Settings.Default.UseFileSettings;
      checkBoxGuessCodePage.Checked = Settings.Default.GuessCodePage;
      checkBoxGuessDelimiter.Checked = Settings.Default.GuessDelimiter;
      checkBoxGuessStartRow.Checked = Settings.Default.GuessStartRow;
      checkBoxGuessHasHeader.Checked = Settings.Default.GuessHasHeader;
      checkBoxDetectFileChanges.Checked = Settings.Default.DetectFileChanges;
      checkBoxMenuDown.Checked = Settings.Default.MenuDown;

      UpdatePassphraseInfoText();
    }

    private void CsvFile_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName != "CodePageId") return;
      foreach (var ite in cboCodePage.Items)
        if (((DisplayItem<int>)ite).ID == m_CsvFileCopy.CodePageId)
        {
          cboCodePage.SelectedItem = ite;
          break;
        }
    }

    private void PositiveNumberValidating(object sender, CancelEventArgs e)
    {
      var tb = sender as TextBox;

      Debug.Assert(tb != null, nameof(tb) + " != null");
      var ok = int.TryParse(tb.Text, out var parse);
      var reformat = parse.ToString();
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

    private void BtnAddPrivKey_Click(object sender, EventArgs e)
    {
      try
      {
        using (var kf = new FormKeyFile("Private PGP Key", true))
        {
          if (kf.ShowDialog(this) != DialogResult.OK) return;
          ApplicationSetting.ToolSetting.PGPInformation.AddPrivateKey(kf.textBox.Text);
          GetPrivateKeys();
        }
      }
      catch (Exception ex)
      {
        _MessageBox.Show(this, ex.ExceptionMessages(), "Error", timeout: 30);
      }
    }

    private void BtnRemPrivKey_Click(object sender, EventArgs e)
    {
      if (listBoxPrivKeys.SelectedIndex < 0) return;
      ApplicationSetting.ToolSetting.PGPInformation.RemovePrivateKey(listBoxPrivKeys.SelectedIndex);
      GetPrivateKeys();
    }

    private void BtnPassp_Click(object sender, EventArgs e)
    {
      using (var inp = new FormPassphrase("Default Encryption Passphrase"))
      {
        if (inp.ShowDialog(this) == DialogResult.OK)
          ApplicationSetting.ToolSetting.PGPInformation.EncryptedPassphase = inp.EncryptedPassphrase;
      }

      UpdatePassphraseInfoText();
    }

    private void GetPrivateKeys()
    {
      listBoxPrivKeys.Items.Clear();
      foreach (var text in ApplicationSetting.ToolSetting.PGPInformation.GetPrivateKeyRingBundleList())
        listBoxPrivKeys.Items.Add(text);
    }

    private void UpdatePassphraseInfoText()
    {
      if (string.IsNullOrEmpty(ApplicationSetting.ToolSetting.PGPInformation.EncryptedPassphase))
        labelPassphrase.Text = "Default passphrase is not yet set";
      else
        try
        {
          ApplicationSetting.ToolSetting.PGPInformation.EncryptedPassphase.Decrypt();
          labelPassphrase.Text = "Default passphrase is set";
        }
        catch
        {
          labelPassphrase.Text = "Passphrase is set but invalid";
        }
    }
  }
}