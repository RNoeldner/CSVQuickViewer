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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CsvTools
{
  /// <summary>
  ///   Windows Form UI editing a <see cref="Column" />
  /// </summary>
  public partial class FormColumnUI : Form
  {
    private const string c_NoSampleDate = "The source does not contain any sample data without warnings in the {0:N0} records read";
    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();
    private readonly Column m_ColumnEdit = new Column();
    private readonly Column m_ColumnRef;
    private readonly IFileSetting m_FileSetting;
    private readonly bool m_WriteSetting;
    private readonly FillGuessSettings m_FillGuessSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormColumnUI" /> class.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="writeSetting">if set to <c>true</c> this is for writing.</param>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="fillGuessSettings">The fill guess settings.</param>
    /// <param name="showIgnore">if set to <c>true</c> [show ignore].</param>
    /// <exception cref="ArgumentNullException">
    /// fileSetting
    /// or
    /// fileSefillGuessSettingstting
    /// </exception>
    public FormColumnUI(Column column, bool writeSetting, IFileSetting fileSetting, FillGuessSettings fillGuessSettings, bool showIgnore)
    {
      Contract.Requires(column != null);
      m_FileSetting = fileSetting?? throw  new ArgumentNullException(nameof(fileSetting));
      m_FillGuessSettings = fillGuessSettings ?? throw new ArgumentNullException(nameof(fillGuessSettings));
      m_ColumnRef = column ?? throw new ArgumentNullException(nameof(column));
      column.CopyTo(m_ColumnEdit);

      m_WriteSetting = writeSetting;


      InitializeComponent();
      
      comboBoxColumnName.Enabled = showIgnore;

      var source = ApplicationSetting.DestinationTimeZone;
      if (source == TimeZoneMapping.cIdLocal)
        source = "the local time zone of you system";

      toolTip.SetToolTip(comboBoxTimeZone,
        !m_WriteSetting
          ? $"Assuming the time read is based in the time zone stored in this column or a constant value and being converted to {source}"
          : $"Converting the time in {source} to the time zone in this column or a constant value");

      labelDisplayNullAs.Visible = writeSetting;
      textBoxDisplayNullAs.Visible = writeSetting;
      checkBoxIgnore.Visible = (!writeSetting && showIgnore);
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
      var columnName = comboBoxColumnName.Text;
      if (string.IsNullOrEmpty(columnName))
      {
        _MessageBox.Show(this, "Please select a column first", "Guess");
        return;
      }

      buttonGuess.Enabled = false;
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        using (var processDisplay = new FormProcessDisplay("Guess Value", true, m_CancellationTokenSource.Token))
        {
          processDisplay.Show();
          if (m_WriteSetting)
          {
            var fileWriter = m_FileSetting.GetFileWriter(processDisplay);
            var hasRetried = false;
            retry:
            var data = fileWriter.GetSourceDataTable(m_FillGuessSettings.CheckedRecords.ToUint());
            {
              var found = new Column();
              var column = data.Columns[columnName];
              if (column == null)
              {
                if (hasRetried)
                  throw new ConfigurationException($"The file does not seem to contain the column {columnName}.");
                var columns = (from DataColumn col in data.Columns select col.ColumnName).ToList();
                UpdateColumnList(columns);
                hasRetried = true;
                goto retry;
                throw new ConfigurationException($"The file does not seem to contain the column {columnName}.");
              }

              found.DataType = column.DataType.GetDataType();
              if (found.DataType == DataType.String)
                return;
              m_ColumnEdit.DataType = found.DataType;
              processDisplay.Hide();

              RefreshData();
              _MessageBox.Show(this,
                $"Based on DataType of the source column this is {m_ColumnEdit.GetTypeAndFormatDescription()}.\nPlease choose the desired output format",
                columnName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
          }
          else
          {
            var samples = GetSampleValues(columnName, processDisplay);
            // shuffle samples, take some from the end and put it in the first 10
            // 1 - 1
            // 2 - Last
            // 3 - 2
            // 4 - Last - 1

            if (samples.Values.IsEmpty())
            {
              _MessageBox.Show(this, string.Format(CultureInfo.CurrentCulture, c_NoSampleDate, samples.RecordsRead),
                "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
              var detectBool = true;
              var detectGuid = true;
              var detectNumeric = true;
              var detectDateTime = true;
              if (comboBoxDataType.SelectedValue != null)
              {
                var selectedType = (DataType)comboBoxDataType.SelectedValue;
                if (selectedType != DataType.String
                    && selectedType != DataType.TextToHtml
                    && selectedType != DataType.TextToHtmlFull
                    && selectedType != DataType.TextPart)
                {
                  var resp = _MessageBox.Show(this, $"Should the system restrict detection to {selectedType}?", "Selected DataType", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                  if (resp == DialogResult.Cancel)
                    return;
                  if (resp == DialogResult.Yes)
                  {
                    switch (selectedType)
                    {
                      case DataType.Integer:
                      case DataType.Numeric:
                      case DataType.Double:
                        detectBool = false;
                        detectDateTime = false;
                        detectGuid = false;
                        break;

                      case DataType.DateTime:
                        detectBool = false;
                        detectNumeric = false;
                        detectGuid = false;
                        break;

                      case DataType.Boolean:
                        detectGuid = false;
                        detectNumeric = false;
                        detectDateTime = false;
                        break;

                      case DataType.Guid:
                        detectBool = false;
                        detectNumeric = false;
                        detectDateTime = false;
                        break;

                      default:
                        break;
                    }
                  }
                }
              }

              // detect all (except Serial dates) and be content with 1 records if need be
              var checkResult = DetermineColumnFormat.GuessValueFormat(samples.Values, 1,
                m_FillGuessSettings.TrueValue, m_FillGuessSettings.FalseValue,
                detectBool, detectGuid, detectNumeric, detectDateTime, detectNumeric, detectDateTime, detectDateTime,
                DetermineColumnFormat.CommonDateFormat(m_FileSetting.ColumnCollection.Select(x => x.ValueFormat)), processDisplay.CancellationToken);
              processDisplay.Hide();
              if (checkResult == null)
              {
                _MessageBox.ShowBig(this, $"No format could be determined in {samples.Values.Count():N0} sample values of {samples.RecordsRead:N0} records.\nSamples:\n" +
                   samples.Values.Take(42).Join("\t"), $"Column: {columnName}", MessageBoxButtons.OK, MessageBoxIcon.Information);
              }
              else
              {
                if (checkResult.FoundValueFormat != null || checkResult.PossibleMatch)
                {
                  if (checkResult.FoundValueFormat != null)
                  {
                    m_ColumnEdit.ValueFormat = checkResult.FoundValueFormat;
                    if (checkResult.FoundValueFormat.DataType == DataType.DateTime)
                      AddFormatToComboBoxDateFormat(checkResult.FoundValueFormat.DateFormat);

                    // In case possible match has the same information as FoundValueFormat,
                    // disregard the possible match
                    if (checkResult.FoundValueFormat.Equals(checkResult.ValueFormatPossibleMatch))
                      checkResult.PossibleMatch = false;
                  }
                  else if (checkResult.PossibleMatch)
                  {
                    if (checkResult.ValueFormatPossibleMatch.DataType == DataType.DateTime)
                      AddFormatToComboBoxDateFormat(checkResult.ValueFormatPossibleMatch.DateFormat);
                  }

                  var sb = new StringBuilder();
                  if (checkResult.ExampleNonMatch.Count > 0)
                    sb.AppendFormat("Not matching:\n{0}\n", checkResult.ExampleNonMatch.Take(4).Join("\t"));
                  sb.AppendFormat("Samples:\n{0}", samples.Values.Take(42).Join("\t"));

                  var suggestClosestMatch = (checkResult.PossibleMatch && (checkResult.FoundValueFormat == null || checkResult.FoundValueFormat.DataType == DataType.String));

                  var msg = $"Determined Format\t: {checkResult.FoundValueFormat.GetTypeAndFormatDescription()}\n";
                  if (checkResult.PossibleMatch)
                    msg += $"          Close match\t: {checkResult.ValueFormatPossibleMatch.GetTypeAndFormatDescription()}\n";
                  msg += "\n" + sb.ToString();
                  if (suggestClosestMatch)
                  {
                    if (_MessageBox.ShowBig(this, $"{msg}\n\nShould the closest match be used?",
                                        $"Column: {columnName}", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                      // use the closest match  instead of Text
                      // can not use ValueFormat.CopyTo,. Column is quite specific and need it to be set,
                      m_ColumnEdit.ValueFormat = checkResult.ValueFormatPossibleMatch;
                    }
                  }
                  else
                  {
                    _MessageBox.ShowBig(this, msg, $"Column: {columnName}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                  }

                  RefreshData();
                }
                else
                {
                  // add the regular samples to the invalids that are first
                  var displayMsg = $"No specific format found in {samples.RecordsRead:N0} records. Need {m_FillGuessSettings.MinSamples:N0} distinct values.\n\n{checkResult.ExampleNonMatch.Concat(samples.Values).Take(42).Join("\t")}";

                  if (samples.Values.Count() < m_FillGuessSettings.MinSamples)
                  {
                    _MessageBox.ShowBig(this, displayMsg, $"Column: {columnName}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                  }
                  else
                  {
                    if (m_ColumnEdit.ValueFormat.DataType == DataType.String)
                    {
                      _MessageBox.ShowBig(this, displayMsg, $"Column: {columnName}", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                      if (_MessageBox.ShowBig(this, displayMsg + "\n\nShould this be set to text?", $"Column: {columnName}", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
    private void ButtonOkClick(object sender, EventArgs e)
    {
      try
      {
        if (!ValidateChildren()) return;
        if (m_ColumnEdit.Equals(m_ColumnRef)) return;
        m_ColumnEdit.CopyTo(m_ColumnRef);
        DialogResult = DialogResult.Yes;
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
        if (index < 0)
          index = checkedListBoxDateFormats.Items.Add(format);
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
        using (var processDisplay = new FormProcessDisplay("Display Values", true, m_CancellationTokenSource.Token))
        {
          processDisplay.Show(this);
          var values = GetSampleValues(comboBoxColumnName.Text, processDisplay);
          processDisplay.Hide();
          Cursor.Current = Cursors.Default;
          if (values.Values.IsEmpty())
          {
            _MessageBox.Show(this, string.Format(CultureInfo.CurrentCulture, c_NoSampleDate, values.RecordsRead),
              comboBoxColumnName.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
          else
          {
            _MessageBox.Show(this, "Found values:\n" + values.Values.Take(42).Join("\t"), comboBoxColumnName.Text,
              MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
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
        var uncheck = checkedListBoxDateFormats.CheckedIndices.Cast<int>().Where(ind => ind != e.Index);
        // disable all other check items

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

        if (e.NewValue == CheckState.Checked || !isInList)
          return;
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
        Extensions.ProcessUIElements();

        columnBindingSource.DataSource = m_ColumnEdit;
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

        // Read the column headers if possible
        if (m_FileSetting == null)
        {
          tableLayoutPanelForm.Controls.Remove(comboBoxColumnName);
          tableLayoutPanelForm.Controls.Add(textBoxColumnName, 1, 0);
          tableLayoutPanelForm.SetColumnSpan(textBoxColumnName, 2);
        }
        else
        {
          using (var processDisplay = new FormProcessDisplay("Retrieving Information", true, m_CancellationTokenSource.Token))
          {
            processDisplay.Show(this);
            ICollection<string> allColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
              if (!m_WriteSetting)
              {
                // if there are ignored columns need to open file and get all columns
                if (m_FileSetting.ColumnCollection.Any(x => x.Ignore) || ApplicationSetting.GetColumnHeader == null)
                {
                  using (var fileReader = m_FileSetting.GetFileReader(processDisplay))
                  {
                    fileReader.Open();
                    for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
                      allColumns.Add(fileReader.GetColumn(colIndex).Name);
                  }
                }
                else
                {
                  var cols = ApplicationSetting.GetColumnHeader.Invoke(m_FileSetting, true, processDisplay);
                  if (cols != null)
                  {
                    foreach (var col in cols)
                    {
                      allColumns.Add(col);
                    }
                  }
                }
              }
              else
              {
                var writer = m_FileSetting.GetFileWriter(processDisplay);
                using (var schemaReader = writer.GetSchemaReader())
                using (var dataTable = schemaReader.GetSchemaTable())
                {
                  foreach (System.Data.DataRow schemaRow in dataTable.Rows)
                    allColumns.Add(schemaRow[System.Data.Common.SchemaTableColumn.ColumnName].ToString());
                }
              }
            }
            catch (Exception ex)
            {
              this.ShowError(ex, "Could not determine columns");
              return;
            }

            UpdateColumnList(allColumns);
          }
        }
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

        //TODO: opening on OS and scaling a different value might be needed
        Height = tableLayoutPanelForm.Height + SystemInformation.CaptionHeight + 13;
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    private void comboBoxTimePart_SelectedIndexChanged(object sender, EventArgs e) => comboBoxTPFormat.Enabled = comboBoxTimePart.SelectedIndex >= 0;

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

        comboBoxTPFormat.Enabled = hasTimePart;
        vf.DateFormat = sender == comboBoxDateFormat ? comboBoxDateFormat.Text : checkedListBoxDateFormats.Text;
        if (string.IsNullOrEmpty(vf.DateFormat))
          return;
        vf.DateSeparator = textBoxDateSeparator.Text;
        vf.TimeSeparator = textBoxTimeSeparator.Text;

        toolTip.SetToolTip(textBoxDateSeparator, FileFormat.GetDescription(vf.DateSeparator));
        toolTip.SetToolTip(textBoxTimeSeparator, FileFormat.GetDescription(vf.TimeSeparator));

        var sourceDate = new DateTime(2013, 4, 7, 15, 45, 50, 345, DateTimeKind.Local);

        if (hasTimePart && vf.DateFormat.IndexOfAny(new char[] { 'h', 'H', 'm', 'S', 's' }) == -1)
          vf.DateFormat += " " + comboBoxTPFormat.Text;

        labelSampleDisplay.Text = StringConversion.DateTimeToString(sourceDate, vf);

        if (comboBoxTimeZone.Text.Length > 2 && comboBoxTimeZone.Text.StartsWith("\"", StringComparison.Ordinal) && comboBoxTimeZone.Text.EndsWith("\"", StringComparison.Ordinal))
        {
          var destTz = TimeZoneMapping.cIdLocal;
          var srcTz = TimeZoneMapping.cIdLocal;
          if (m_WriteSetting)
            srcTz = comboBoxTimeZone.Text.Substring(1, comboBoxTimeZone.Text.Length - 2);
          else
            destTz = comboBoxTimeZone.Text.Substring(1, comboBoxTimeZone.Text.Length - 2);

          labelInputTZ.Text = srcTz;
          labelOutPutTZ.Text = destTz;
          sourceDate = sourceDate.ConvertTime(srcTz, destTz);
        }
        else
        {
          labelInputTZ.Text = string.Empty;
          labelOutPutTZ.Text = string.Empty;
        }
        labelDateOutputDisplay.Text = StringConversion.DisplayDateTime(sourceDate, CultureInfo.CurrentCulture);
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.InnerExceptionMessages());
      }
    }

    /// <summary>
    /// Gets the sample values.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="processDisplay">The process display.</param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException">FileSetting not set</exception>
    /// <exception cref="FileException">
    /// Column {columnName} not found.
    /// or
    /// Column {columnName} not found.
    /// </exception>
    private DetermineColumnFormat.SampleResult GetSampleValues(string columnName, IProcessDisplay processDisplay)
    {
      Contract.Requires(!string.IsNullOrEmpty(columnName));
      Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
      if (m_FileSetting == null)
        throw new ConfigurationException("FileSetting not set");

      try
      {
        if (m_WriteSetting)
        {
          var fileWriter = m_FileSetting.GetFileWriter(processDisplay);
          var data = fileWriter.GetSourceDataTable((uint)m_FillGuessSettings.CheckedRecords);
          {
            var colIndex = data.Columns.IndexOf(columnName);
            if (colIndex < 0)
              throw new FileException($"Column {columnName} not found.");

            return DetermineColumnFormat.GetSampleValues(data, colIndex, m_FillGuessSettings.SampleValues,
              m_FileSetting.TreatTextAsNull, processDisplay.CancellationToken);
          }
        }
        // must be file reader if this is reached
        var hasRetried = false;

        var fileSettingCopy = m_FileSetting.Clone();
        // Make sure that if we do have a CSV file without header that we will skip the first row that
        // might contain headers, but its simply set as without headers.
        if (fileSettingCopy is CsvFile csv)
        {
          if (!csv.HasFieldHeader && csv.SkipRows == 0)
            csv.SkipRows = 1;
          // turn off all warnings as they will cause GetSampleValues to ignore the row
          csv.TryToSolveMoreColumns = false;
          csv.WarnDelimiterInValue = false;
          csv.WarnLineFeed = false;
          csv.WarnQuotes = false;
          csv.WarnUnknowCharater = false;
          csv.WarnNBSP = false;
          csv.WarnQuotesInQuotes = false;
        }

        retry:
        using (var fileReader = fileSettingCopy.GetFileReader(processDisplay))
        {
          fileReader.Open();
          var colIndex = fileReader.GetOrdinal(columnName);
          if (colIndex < 0)
          {
            if (!hasRetried)
            {
              var columns = new List<string>();
              for (var col = 0; col < fileReader.FieldCount; col++)
                columns.Add(fileReader.GetName(col));
              UpdateColumnList(columns);
              hasRetried = true;
              goto retry;
            }
            throw new FileException($"Column {columnName} not found.");
          }

          return DetermineColumnFormat.GetSampleValues(fileReader, m_FillGuessSettings.CheckedRecords,
            colIndex, m_FillGuessSettings.SampleValues, m_FileSetting.TreatTextAsNull, processDisplay.CancellationToken);
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Getting Sample Values");
      }
      return new DetermineColumnFormat.SampleResult(new List<string>(), 0);
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
        di.Add(new DisplayItem<int>((int)item, item.DataTypeDisplay()));
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
      AddNotExisting(formatsExtra, "yyyy/MM/ddTHH:mm:ss", formatsReg);
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
      if (e.Category != UserPreferenceCategory.Locale)
        return;
      CultureInfo.CurrentCulture.ClearCachedData();
      // Refresh The date formats presented
      SetDateFormat();
      // Update the UI
      ComboBoxDataType_SelectedIndexChanged(null, null);
    }

    private void TextBoxDecimalSeparator_Validating(object sender, CancelEventArgs e) => errorProvider.SetError(textBoxDecimalSeparator,
        string.IsNullOrEmpty(textBoxDecimalSeparator.Text) ? "Must be provided" : "");

    private void TextBoxSplit_Validating(object sender, CancelEventArgs e) => errorProvider.SetError(textBoxSplit, string.IsNullOrEmpty(textBoxSplit.Text) ? "Must be provided" : "");

    private void UpdateColumnList(ICollection<string> allColumns)
    {
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

      RefreshData();
    }
  }
}