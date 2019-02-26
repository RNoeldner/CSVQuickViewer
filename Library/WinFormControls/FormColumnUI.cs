/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Windows Form UI editing a <see cref="Column" />
  /// </summary>
  public partial class FormColumnUI : Form
  {
    private const string c_NoSampleDate = "The source does not contain any sample data in the first {0} rows.";
    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();
    private readonly Column m_ColumnEdit = new Column();
    private readonly Column m_ColumnRef;
    private readonly IFileSetting m_FileSetting;
    private readonly bool m_WriteSetting;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormColumnUI" /> class.
    /// </summary>
    /// <param name="column">The column format.</param>
    /// <param name="writeSetting">if set to <c>true</c> [write setting].</param>
    /// <param name="fileSetting">The file setting.</param>
    public FormColumnUI(Column column, bool writeSetting, IFileSetting fileSetting)
    {
      Contract.Requires(column != null);
      m_FileSetting = fileSetting;
      m_ColumnRef = column;
      column.CopyTo(m_ColumnEdit);

      m_WriteSetting = writeSetting;
      InitializeComponent();
    }

    public bool ShowGuess
    {
      get => buttonGuess.Visible;
      set
      {
        buttonGuess.Visible = value;
        buttonDisplayValues.Visible = value;
      }
    }

    /// <summary>
    ///   Show or hide the Ignore Buttons
    /// </summary>
    public bool ShowIgnore
    {
      get => checkBoxIgnore.Visible;
      set => checkBoxIgnore.Visible = value;
    }

    /// <summary>
    ///   Handles the Click event of the buttonCancel control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    public void ButtonCancelClick(object sender, EventArgs e) => Close();

    /// <summary>
    ///   Handles the Click event of the buttonGuess control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    public void ButtonGuessClick(object sender, EventArgs e)
    {
      if (string.IsNullOrEmpty(comboBoxColumnName.Text))
      {
        _MessageBox.Show(this, "Please select a column first", "Guess");
        return;
      }

      buttonGuess.Enabled = false;
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        if (m_WriteSetting)
        {
          var fileWriter = m_FileSetting.GetFileWriter(m_CancellationTokenSource.Token);
          var data = fileWriter.GetSourceDataTable(ApplicationSetting.FillGuessSettings.CheckedRecords.ToUint());
          {
            var found = new Column();
            var colum = data.Columns[comboBoxColumnName.Text];
            if (colum == null)
              throw new ApplicationException($"The file does not contain the column {comboBoxColumnName.Text}.");

            found.DataType = colum.DataType.GetDataType();
            if (found.DataType == DataType.String) return;
            m_ColumnEdit.DataType = found.DataType;
            RefreshData();
            _MessageBox.Show(this,
              $"Based on DataType of the source column this is {m_ColumnEdit.GetTypeAndFormatDescription()}.\nPlease choose the desired output format",
              comboBoxColumnName.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
        }
        else
        {
          var samples = GetSampleValues(comboBoxColumnName.Text);
          // shuffle samples, take some from the end and put it in the first 10
          // 1 - 1
          // 2 - Last
          // 3 - 2
          // 4 - Last - 1

          var enumerable = samples.ToList();
          if (enumerable.IsEmpty())
          {
            _MessageBox.Show(this, string.Format(CultureInfo.CurrentCulture, c_NoSampleDate, ApplicationSetting.FillGuessSettings.CheckedRecords),
              "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
          else
          {
            var checkResult = DetermineColumnFormat.GuessValueFormat(enumerable, ApplicationSetting.FillGuessSettings.MinSamplesForIntDate,
              ApplicationSetting.FillGuessSettings.TrueValue, ApplicationSetting.FillGuessSettings.FalseValue,
              ApplicationSetting.FillGuessSettings.DetectBoolean, ApplicationSetting.FillGuessSettings.DetectGUID,
              ApplicationSetting.FillGuessSettings.DectectNumbers, ApplicationSetting.FillGuessSettings.DetectDateTime,
              ApplicationSetting.FillGuessSettings.DectectPercentage,
              ApplicationSetting.FillGuessSettings.SerialDateTime,
              ApplicationSetting.FillGuessSettings.CheckNamedDates,
              DetermineColumnFormat.CommonDateFormat(m_FileSetting.Column.Select(x => x.ValueFormat)), m_CancellationTokenSource.Token);
            if (checkResult == null)
            {
              _MessageBox.Show(this,
                "No Format could be determined, there are not enough sample values:\n" +
                 enumerable.Take(42).Join("\t"), comboBoxColumnName.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
              if (checkResult.FoundValueFormat != null && checkResult.PossibleMatch)
              {
                m_ColumnEdit.ValueFormat = checkResult.FoundValueFormat;
                if (checkResult.ValueFormatPossibleMatch.DataType == DataType.DateTime)
                  AddFormatToComboBoxDateFormat(checkResult.ValueFormatPossibleMatch.DateFormat);

                if (checkResult.FoundValueFormat.DataType == DataType.DateTime)
                  AddFormatToComboBoxDateFormat(m_ColumnEdit.DateFormat);

                RefreshData();

                var sb = new StringBuilder();
                if (checkResult.ExampleNonMatch.Count > 0)
                  sb.AppendFormat("Not matching\t: {0}\n", checkResult.ExampleNonMatch.Take(3).Join("\t"));
                sb.AppendFormat("Samples     \t: {0}", enumerable.Take(42).Join("\t"));

                _MessageBox.Show(this,
                  $"Determined Format\t: {checkResult.FoundValueFormat.GetTypeAndFormatDescription()}\n\nClose match\t: {checkResult.ValueFormatPossibleMatch.GetTypeAndFormatDescription()}\n\n{sb}",
                  comboBoxColumnName.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
              }
              else
              {
                // add the regular samples to the invalids that are first
                var examples = checkResult.ExampleNonMatch.Concat(enumerable).Take(42);

                if (enumerable.Count() < ApplicationSetting.FillGuessSettings.MinSamplesForIntDate)
                {
                  _MessageBox.Show(this,
                    $"No Format found in:\n{examples.Join("\t")}.\n\nMaybe not enough distinct values have been found.",
                    comboBoxColumnName.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                  if (m_ColumnEdit.ValueFormat.DataType == DataType.String)
                  {
                    _MessageBox.Show(this,
                      $"No Format found in:\n{examples.Join("\t")}",
                      comboBoxColumnName.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                  }
                  else
                  {
                    if (_MessageBox.Show(this,
                          $"No Format found in:\n{examples.Join("\t")}\nShould this be set to text?",
                          comboBoxColumnName.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                      m_ColumnEdit.ValueFormat.DataType = DataType.String;
                    }
                  }
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
      finally
      {
        buttonGuess.Enabled = true;
        Cursor.Current = oldCursor;
      }
    }

    /// <summary>
    ///   Handles the Click event of the buttonOK control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    public void ButtonOKClick(object sender, EventArgs e)
    {
      ValidateChildren();
      try
      {
        if (!m_ColumnEdit.Equals(m_ColumnRef))
        {
          m_ColumnEdit.CopyTo(m_ColumnRef);
          DialogResult = DialogResult.Yes;
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
      finally
      {
        Close();
      }
    }

    private static void AddNotExisting(List<string> list, string value, List<string> otherList = null)
    {
      if (!list.Contains(value) && (otherList == null || !otherList.Contains(value)))
        list.Add(value);
    }

    private void AddFormatToComboBoxDateFormat(string format)
    {
      try
      {
        if (string.IsNullOrEmpty(format))
          return;

        foreach (int ind in checkedListBoxDateFormats.CheckedIndices)
          checkedListBoxDateFormats.SetItemChecked(ind, false);
        var index = checkedListBoxDateFormats.Items.IndexOf(format);
        if (index < 0) index = checkedListBoxDateFormats.Items.Add(format);
        checkedListBoxDateFormats.SetItemChecked(index, true);
        checkedListBoxDateFormats.TopIndex = index;
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    /// <summary>
    ///   Handles the Click event of the buttonAddFormat control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ButtonAddFormat_Click(object sender, EventArgs e) => AddFormatToComboBoxDateFormat(comboBoxDateFormat.Text);

    private void ButtonDisplayValues_Click(object sender, EventArgs e)
    {
      buttonDisplayValues.Enabled = false;
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        var values = GetSampleValues(comboBoxColumnName.Text);

        Cursor.Current = Cursors.Default;
        var enumerable = values.ToList();
        if (enumerable.IsEmpty())
        {
          _MessageBox.Show(this, string.Format(CultureInfo.CurrentCulture, c_NoSampleDate, ApplicationSetting.FillGuessSettings.CheckedRecords),
            comboBoxColumnName.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
          _MessageBox.Show(this, "Found values:\n" + enumerable.Take(42).Join("\t"), comboBoxColumnName.Text,
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
      finally
      {
        buttonDisplayValues.Enabled = true;
        Cursor.Current = oldCursor;
      }
    }

    /// <summary>
    ///   Handles the ItemCheck event of the checkedListBoxDateFormats control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.Windows.Forms.ItemCheckEventArgs" /> instance containing the event data.
    /// </param>
    private void CheckedListBoxDateFormats_ItemCheck(object sender, ItemCheckEventArgs e)
    {
      var format = checkedListBoxDateFormats.Items[e.Index].ToString();
      if (m_WriteSetting)
      {
        var uncheck = new List<int>();
        // disable all other check items
        foreach (int ind in checkedListBoxDateFormats.CheckedIndices)
        {
          if (ind != e.Index)
            uncheck.Add(ind);
        }

        foreach (var ind in uncheck)
          checkedListBoxDateFormats.SetItemCheckState(ind, CheckState.Unchecked);

        m_ColumnEdit.DateFormat = format;
      }
      else
      {
        var parts = new List<string>(StringUtils.SplitByDelimiter(m_ColumnEdit.DateFormat));
        var isInList = parts.Contains(format);

        if (e.NewValue == CheckState.Checked && !isInList)
        {
          parts.Add(format);
          m_ColumnEdit.DateFormat = parts.Join(";");
        }

        if (e.NewValue == CheckState.Checked || !isInList) return;
        parts.Remove(format);
        m_ColumnEdit.DateFormat = parts.Join(";");
      }
    }

    private void ColumnFormatUI_FormClosing(object sender, FormClosingEventArgs e)
    {
      try
      {
        if (!m_CancellationTokenSource.IsCancellationRequested)
          m_CancellationTokenSource.Cancel();
      }
      catch (ObjectDisposedException)
      {
      }
    }

    /// <summary>
    ///   Handles the Load event of the ColumnFormatUI control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ColumnFormatUI_Load(object sender, EventArgs e)
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;

      try
      {
        if (m_WriteSetting)
          labelAllowedDateFormats.Text = "Date Format:";
        Height = 307 - checkBoxIgnore.Height;
        Extensions.ProcessUIElements();

        columnBindingSource.DataSource = m_ColumnEdit;
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

        // Read the column headers if possible
        if (m_FileSetting == null)
        {
          textBoxColumnName.Visible = true;
        }
        else
        {
          comboBoxColumnName.Visible = true;
          ICollection<string> allColumns = new List<string>();
          using (var frm = new FormProcessDisplayLogger("Get Columns"))
          {
            frm.Show(this);
            try
            {
              if (!m_WriteSetting)
              {
                // get the columns from the file
                allColumns = CsvHelper.GetColumnHeader(m_FileSetting, true, true, frm);
              }
              else
              {
                frm.SetProcess("Executing SQL");
                // get the columns from the SQL
                using (var dataReader = ApplicationSetting.SQLDataReader(m_FileSetting.SqlStatement, frm.CancellationToken))
                {
                  for (var i = 0; i < dataReader.FieldCount; i++)
                    allColumns.Add(dataReader.GetName(i));
                }
              }
            }
            catch (Exception ex)
            {
              this.ShowError(ex, "Could not determine columns");
            }
          }

          var columnsConf = allColumns.ToArray();
          var columnsTp = allColumns.ToArray();
          comboBoxColumnName.BeginUpdate();
          // if we have a list of columns add them to fields that show a column name
          if (columnsConf.Length > 0)
          {
            comboBoxColumnName.Items.AddRange(columnsConf);
            comboBoxTimePart.Items.AddRange(columnsTp);
            comboBoxTimeZone.Items.AddRange(columnsTp);
            if (string.IsNullOrEmpty(m_ColumnEdit.Name))
              m_ColumnEdit.Name = columnsConf[0];
          }
          else
          {
            // If list would be empty add at least the current value
            if (!string.IsNullOrEmpty(m_ColumnEdit.Name))
              comboBoxColumnName.Items.Add(m_ColumnEdit.Name);
          }

          if (!m_WriteSetting && comboBoxColumnName.Items.Count > 0)
            comboBoxColumnName.DropDownStyle = ComboBoxStyle.DropDownList;
          comboBoxColumnName.EndUpdate();
        }

        RefreshData();
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private void ComboBoxColumnName_SelectedIndexChanged(object sender, EventArgs e)
    {
      try
      {
        buttonGuess.Enabled = m_FileSetting != null && comboBoxColumnName.SelectedItem != null;
        buttonDisplayValues.Enabled = m_FileSetting != null && comboBoxColumnName.SelectedItem != null;
        ComboBoxColumnName_TextUpdate(sender, e);
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    private void ComboBoxColumnName_TextUpdate(object sender, EventArgs e) => buttonOK.Enabled = m_FileSetting == null || !string.IsNullOrEmpty(comboBoxColumnName.Text);

    /// <summary>
    ///   Handles the SelectedIndexChanged event of the comboBoxDataType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ComboBoxDataType_SelectedIndexChanged(object sender, EventArgs e)
    {
      try
      {
        if (comboBoxDataType.SelectedValue == null)
          return;
        var selType = (DataType)comboBoxDataType.SelectedValue;
        m_ColumnEdit.DataType = selType;

        groupBoxNumber.Visible = selType == DataType.Numeric || selType == DataType.Double;
        if (groupBoxNumber.Visible)
          NumberFormatChanged(null, null);

        groupBoxDate.Visible = selType == DataType.DateTime;
        if (groupBoxDate.Visible)
          DateFormatChanged(null, null);

        groupBoxBoolean.Visible = selType == DataType.Boolean;
        groupBoxSplit.Visible = selType == DataType.TextPart;
        if (groupBoxSplit.Visible)
          SetSamplePart(null, null);
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    /// <summary>
    ///   Reapply formatting to the sample date
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void DateFormatChanged(object sender, EventArgs e)
    {
      try
      {
        var vf = new ValueFormat();
        var hasTimePart = !string.IsNullOrEmpty(comboBoxTimePart.Text);

        vf.DateFormat = sender == comboBoxDateFormat ? comboBoxDateFormat.Text : checkedListBoxDateFormats.Text;
        if (string.IsNullOrEmpty(vf.DateFormat))
          return;
        vf.DateSeparator = textBoxDateSeparator.Text;
        vf.TimeSeparator = textBoxTimeSeparator.Text;

        toolTip.SetToolTip(textBoxDateSeparator, FileFormat.GetDescription(vf.DateSeparator));
        toolTip.SetToolTip(textBoxTimeSeparator, FileFormat.GetDescription(vf.TimeSeparator));

        var sample =
          StringConversion.DateTimeToString(new DateTime(2013, 4, 7, 15, 45, 50, 345, DateTimeKind.Local), vf);
        string sampleTime = null;
        labelSample.Text = $"Input: \"{sample}\"";
        if (hasTimePart)
        {
          var vfTime = new ValueFormat
          {
            DateFormat = comboBoxTPFormat.Text,
            DateSeparator = textBoxDateSeparator.Text,
            TimeSeparator = textBoxTimeSeparator.Text
          };

          sampleTime =
            StringConversion.DateTimeToString(new DateTime(2013, 4, 7, 15, 45, 50, 345, DateTimeKind.Local), vfTime);
          labelSample.Text += $" + \"{sampleTime}\"";
        }

        labelDateOutput.Text =
          $"Output: \"{StringConversion.DisplayDateTime(StringConversion.CombineStringsToDateTime(sample, vf.DateFormat, sampleTime, vf.DateSeparator, vf.TimeSeparator, false).Value, CultureInfo.CurrentCulture)}\"";
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.InnerExceptionMessages());
      }
    }

    /// <summary>
    ///   Gets the sample values for a column.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <returns></returns>
    /// <exception cref="System.ApplicationException">
    ///   Parent FileSetting not set or The file does not contain the column
    /// </exception>
    private IEnumerable<string> GetSampleValues(string columnName)
    {
      Contract.Requires(!string.IsNullOrEmpty(columnName));
      Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
      if (m_FileSetting == null)
        throw new ApplicationException("Parent FileSetting not set");

      var colIndex = CsvHelper.GetColumnIndex(m_FileSetting, columnName);
      if (colIndex < 0)
        throw new ApplicationException($"The file does not contain the column {comboBoxColumnName.Text}.");

      try
      {
        if (m_WriteSetting)
        {
          var fileWriter = m_FileSetting.GetFileWriter(m_CancellationTokenSource.Token);
          var data = fileWriter.GetSourceDataTable((uint)ApplicationSetting.FillGuessSettings.CheckedRecords);
          {
            return DetermineColumnFormat.GetSampleValues(data, colIndex, ApplicationSetting.FillGuessSettings.SampleValues,
              m_FileSetting.TreatTextAsNull, m_CancellationTokenSource.Token);
          }
        }

        using (var fileReader = m_FileSetting.GetFileReader())
        {
          fileReader.Open(false, m_CancellationTokenSource.Token);
          return DetermineColumnFormat.GetSampleValues(fileReader, ApplicationSetting.FillGuessSettings.CheckedRecords,
            colIndex, ApplicationSetting.FillGuessSettings.SampleValues, m_FileSetting.TreatTextAsNull,
            m_CancellationTokenSource.Token);
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
      return new List<string>();
    }

    /// <summary>
    ///   Reapply formatting to the sample number
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void NumberFormatChanged(object sender, EventArgs e)
    {
      try
      {
        if (string.IsNullOrEmpty(textBoxDecimalSeparator.Text))
          return;
        var vf = new ValueFormat
        {
          NumberFormat = comboBoxNumberFormat.Text,
          GroupSeparator = textBoxGroupSeparator.Text,
          DecimalSeparator = textBoxDecimalSeparator.Text
        };

        toolTip.SetToolTip(textBoxDecimalSeparator, FileFormat.GetDescription(vf.DecimalSeparator));
        toolTip.SetToolTip(textBoxGroupSeparator, FileFormat.GetDescription(vf.GroupSeparator));

        var sample = StringConversion.DoubleToString(1234.567, vf);
        labelNumber.Text = $"Input: \"{sample}\"";
        labelNumberOutput.Text =
          $"Output: \"{StringConversion.StringToDecimal(sample, FileFormat.GetChar(vf.DecimalSeparator), FileFormat.GetChar(vf.GroupSeparator), false):N}\"";
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.InnerExceptionMessages());
      }
    }

    private void PartValidating(object sender, CancelEventArgs e)
    {
      var ok = int.TryParse(textBoxPart.Text, out var parse);
      var reformat = parse.ToString(CultureInfo.CurrentCulture);
      ok = ok && parse > 0 && reformat == textBoxPart.Text;

      if (!ok)
      {
        errorProvider.SetError(textBoxPart, "Must be a positive number");
        e.Cancel = true;
      }
      else
      {
        if (parse == 1 && checkBoxPartToEnd.Checked)
        {
          errorProvider.SetError(textBoxPart, "Un-check or choose a later part.");
          errorProvider.SetError(checkBoxPartToEnd, "Un-check or choose a later part.");
        }
        else
        {
          errorProvider.SetError(checkBoxPartToEnd, "");
          errorProvider.SetError(textBoxPart, "");
        }
      }
    }

    private void RefreshData()
    {
      SetDateFormat();
      SetComboBoxDataType();
      ComboBoxColumnName_TextUpdate(null, null);
    }

    private void RegionAndLanguageLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        var cplPath = Path.Combine(Environment.SystemDirectory, "control.exe");
        Process.Start(cplPath, "/name Microsoft.RegionAndLanguage");
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    private void SetComboBoxDataType()
    {
      var di = new List<DisplayItem<int>>();
      foreach (DataType item in Enum.GetValues(typeof(DataType)))
        di.Add(new DisplayItem<int>((int) item, item.DataTypeDisplay()));
      var selValue = (int)m_ColumnEdit.DataType;
      comboBoxDataType.DataSource = di;
      comboBoxDataType.SelectedValue = selValue;
    }

    private void SetDateFormat()
    {
      var formatsTime = new List<string>();
      AddNotExisting(formatsTime, CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.ReplaceDefaults(
        CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, "//",
        CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator, ":"));

      AddNotExisting(formatsTime, CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ReplaceDefaults(
        CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, "//",
        CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator, ":"));

      AddNotExisting(formatsTime, "HH:mm:ss");
      AddNotExisting(formatsTime, "HH:mm");
      AddNotExisting(formatsTime, "h:mm tt");

      comboBoxTPFormat.BeginUpdate();
      comboBoxTPFormat.Items.Clear();
      comboBoxTPFormat.Items.AddRange(formatsTime.ToArray());
      comboBoxTPFormat.EndUpdate();

      var formatsReg = new List<string>();
      var formatsExtra = new List<string>();
      AddNotExisting(formatsReg, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ReplaceDefaults(
        CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, "/",
        CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator, ":"));
      AddNotExisting(formatsReg,
        (CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " +
         CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern).ReplaceDefaults(
          CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, "/",
          CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator, ":"));
      AddNotExisting(formatsReg, "MM/dd/yyyy");
      AddNotExisting(formatsReg, "HH:mm:ss");
      AddNotExisting(formatsReg, "MM/dd/yyyy HH:mm:ss");
      AddNotExisting(formatsReg, "dd/MM/yyyy");
      AddNotExisting(formatsReg, "yyyy/MM/dd");
      var parts = StringUtils.SplitByDelimiter(m_ColumnEdit.DateFormat);
      foreach (var format in parts)
        AddNotExisting(formatsReg, format);

      AddNotExisting(formatsExtra, "M/d/yyyy", formatsReg);
      AddNotExisting(formatsExtra, "M/d/yyyy h:mm tt", formatsReg);
      AddNotExisting(formatsExtra, "M/d/yyyy h:mm:ss tt", formatsReg);
      AddNotExisting(formatsExtra, "yyyyMMdd", formatsReg);
      AddNotExisting(formatsExtra, "yyyyMMddTHH:mm:ss.FFF", formatsReg);
      AddNotExisting(formatsExtra, "yyyy/MM/dd HH:mm:ss.FFF", formatsReg);
      AddNotExisting(formatsExtra, "dd/MM/yy", formatsReg);
      AddNotExisting(formatsExtra, "d/MM/yyyy", formatsReg);
      AddNotExisting(formatsExtra, "d/M/yy", formatsReg);
      AddNotExisting(formatsExtra, "HH:mm:ss.FFF", formatsReg);

      comboBoxDateFormat.BeginUpdate();
      comboBoxDateFormat.Items.Clear();
      comboBoxDateFormat.Items.AddRange(formatsExtra.ToArray());
      comboBoxDateFormat.EndUpdate();

      checkedListBoxDateFormats.BeginUpdate();
      checkedListBoxDateFormats.Items.Clear();
      checkedListBoxDateFormats.Items.AddRange(formatsReg.ToArray());
      // Check all items in parts
      foreach (var format in parts)
      {
        var index = checkedListBoxDateFormats.Items.IndexOf(format);
        checkedListBoxDateFormats.SetItemChecked(index, true);
        checkedListBoxDateFormats.SelectedIndex = index;
      }

      checkedListBoxDateFormats.EndUpdate();
    }

    private void SetSamplePart(object sender, EventArgs e)
    {
      if (string.IsNullOrEmpty(textBoxSplit.Text))
        return;

      var split = textBoxSplit.Text[0];
      var sample = $"This{split}is a{split}concatenated{split}list";

      toolTip.SetToolTip(textBoxSplit, FileFormat.GetDescription(textBoxSplit.Text));

      var part = StringConversion.StringToInt32(textBoxPart.Text, '.', '\0');
      if (!part.HasValue)
        return;
      if (part.Value < 1)
      {
        textBoxPart.Text = "1";
        part = 1;
      }

      if (part.Value == 1)
        checkBoxPartToEnd.Checked = false;
      var toEnd = checkBoxPartToEnd.Checked;

      labelSamplePart.Text = $"Input: \"{sample}\"";
      labelResultPart.Text = $"Output: \"{StringConversion.StringToTextPart(sample, split, part.Value, toEnd)}\"";
    }

    private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
      if (e.Category != UserPreferenceCategory.Locale) return;
      CultureInfo.CurrentCulture.ClearCachedData();
      // Refresh The date formats presented
      SetDateFormat();
      // Update the UI
      ComboBoxDataType_SelectedIndexChanged(null, null);
    }

    private void TextBoxDecimalSeparator_Validating(object sender, CancelEventArgs e)
    {
      errorProvider.SetError(textBoxDecimalSeparator,
        string.IsNullOrEmpty(textBoxDecimalSeparator.Text) ? "Must be provided" : "");
    }

    private void TextBoxSplit_Validating(object sender, CancelEventArgs e) => errorProvider.SetError(textBoxSplit, string.IsNullOrEmpty(textBoxSplit.Text) ? "Must be provided" : "");
  }
}