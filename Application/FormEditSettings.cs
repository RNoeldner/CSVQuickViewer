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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Form to edit the Settings
  /// </summary>
  public partial class FormEditSettings : Form
  {
    private readonly ViewSettings m_ViewSettings;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormEditSettings" /> class.
    /// </summary>
    public FormEditSettings()
    {
      InitializeComponent();
      GetPrivateKeys();
    }

    public FormEditSettings(ViewSettings viewSettings)
    {
      m_ViewSettings = viewSettings;
      InitializeComponent();
      GetPrivateKeys();
    }

    private void BtnAddPrivKey_Click(object sender, EventArgs e)
    {
      try
      {
        using (var kf = new FormKeyFile("Private PGP Key", true))
        {
          if (kf.ShowDialog(this) != DialogResult.OK) return;
          m_ViewSettings.PGPInformation.AddPrivateKey(kf.KeyBlock);
          GetPrivateKeys();
        }
      }
      catch (Exception ex)
      {
        _MessageBox.Show(this, ex.ExceptionMessages(), "Error", timeout: 30);
      }
    }

    private void BtnOpenFile_Click(object sender, EventArgs e)
    {
      try
      {
        var split = FileSystemUtils.SplitPath(textBoxFile.Text);
        var newFileName = WindowsAPICodePackWrapper.Open(split.DirectoryName, "Delimited File", "Delimited files (*.csv;*.txt;*.tab;*.tsv)|*.csv;*.txt;*.tab;*.tsv|All files (*.*)|*.*", split.FileName);
        if (!string.IsNullOrEmpty(newFileName))
          ChangeFileName(newFileName);

      }
      catch (Exception exc)
      {
        MessageBox.Show(this, exc.ExceptionMessages(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
    }

    private void BtnPassp_Click(object sender, EventArgs e)
    {
      using (var inp = new FormPassphrase("Default Encryption Passphrase"))
      {
        if (inp.ShowDialog(this) == DialogResult.OK)
          m_ViewSettings.PGPInformation.EncryptedPassphase = inp.EncryptedPassphrase;
      }

      UpdatePassphraseInfoText();
    }

    private void BtnRemPrivKey_Click(object sender, EventArgs e)
    {
      if (listBoxPrivKeys.SelectedIndex < 0) return;
      m_ViewSettings.PGPInformation.RemovePrivateKey(listBoxPrivKeys.SelectedIndex);
      GetPrivateKeys();
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
        CsvHelper.GuessCodePage(m_ViewSettings);
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
        m_ViewSettings.FileFormat.FieldDelimiter = CsvHelper.GuessDelimiter(m_ViewSettings);
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private void ButtonSkipLine_Click(object sender, EventArgs e)
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        m_ViewSettings.SkipRows = CsvHelper.GuessStartRow(m_ViewSettings);
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

    private void ChangeFileName(string newFileName)
    {
      m_ViewSettings.FileName = newFileName.GetRelativePath(ApplicationSetting.RootFolder);
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        var csvFile = new CsvFile(newFileName);
        csvFile.GetEncryptedPassphraseFunction = csvFile.GetEncryptedPassphraseOpenForm;
        using (var processDisplay = new DummyProcessDisplay())
        {
          csvFile.RefreshCsvFile(processDisplay);
        }

        m_ViewSettings.FileFormat.FieldDelimiter = csvFile.FileFormat.FieldDelimiter;
        m_ViewSettings.CodePageId = csvFile.CodePageId;
        m_ViewSettings.ByteOrderMark = csvFile.ByteOrderMark;
        m_ViewSettings.SkipRows = csvFile.SkipRows;
        m_ViewSettings.HasFieldHeader = csvFile.HasFieldHeader;

        if (MessageBox.Show(this, "Should the value format of the columns be analyzed?", "Value Format",
              MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
          if (m_ViewSettings.ColumnCollection.Count > 0 &&
              MessageBox.Show(this,
                "Any already typed value will not be analyzed.\r\n Should the existing formats be removed before doing so?",
                "Value Format", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            m_ViewSettings.ColumnCollection.Clear();
          try
          {
            using (var processDisplay = m_ViewSettings.GetProcessDisplay(this, true, System.Threading.CancellationToken.None))
            {
              m_ViewSettings.FillGuessColumnFormatReader(false, processDisplay);
            }
          }
          catch (Exception exc)
          {
            _MessageBox.Show(this, exc.ExceptionMessages(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
      if (e.PropertyName != "CodePageId") return;
      foreach (var ite in cboCodePage.Items)
        if (((DisplayItem<int>)ite).ID == m_ViewSettings.CodePageId)
        {
          cboCodePage.SelectedItem = ite;
          break;
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
      quotingControl.CsvFile = m_ViewSettings;
      CsvFile_PropertyChanged(null, new PropertyChangedEventArgs("CodePageId"));

      UpdatePassphraseInfoText();
    }
    private void GetPrivateKeys()
    {
      listBoxPrivKeys.Items.Clear();
      foreach (var text in m_ViewSettings.PGPInformation.GetPrivateKeyRingBundleList())
        listBoxPrivKeys.Items.Add(text);
    }

    private void PositiveNumberValidating(object sender, CancelEventArgs e)
    {
      var tb = sender as TextBox;

      Debug.Assert(tb != null, nameof(tb) + " != null");
      var ok = int.TryParse(tb.Text, out var parse);
      var reformat = parse.ToString(System.Globalization.CultureInfo.CurrentCulture);
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
    private void UpdatePassphraseInfoText()
    {
      if (string.IsNullOrEmpty(m_ViewSettings.PGPInformation.EncryptedPassphase))
        labelPassphrase.Text = "Default passphrase is not yet set";
      else
        try
        {
          m_ViewSettings.PGPInformation.EncryptedPassphase.Decrypt();
          labelPassphrase.Text = "Default passphrase is set";
        }
        catch
        {
          labelPassphrase.Text = "Passphrase is set but invalid";
        }
    }

    private void FormEditSettings_FormClosing(object sender, FormClosingEventArgs e)
    {
      // update the values in not changed setting
      ValidateChildren();
    }
  }
}