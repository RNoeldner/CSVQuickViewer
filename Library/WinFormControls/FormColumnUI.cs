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

using JetBrains.Annotations;
using Microsoft.Win32;
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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Windows Form UI editing a <see cref="Column" />
  /// </summary>
  public partial class FormColumnUI : ResizeForm
  {
    private const string c_NoSampleDate =
      "The source does not contain any sample data without warnings in the {0:N0} records read";

    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

    private readonly Column m_ColumnEdit = new Column();
    private readonly Column m_ColumnRef;

    private readonly IFileSetting m_FileSetting;

    private readonly FillGuessSettings m_FillGuessSettings;

    private readonly bool m_WriteSetting;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormColumnUI" /> class.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="writeSetting">if set to <c>true</c> this is for writing.</param>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="fillGuessSettings">The fill guess settings.</param>
    /// <param name="showIgnore">if set to <c>true</c> [show ignore].</param>
    /// <exception cref="ArgumentNullException">fileSetting or fillGuessSettings NULL</exception>
    public FormColumnUI(
      Column column,
      bool writeSetting,
      IFileSetting fileSetting,
      FillGuessSettings fillGuessSettings,
      bool showIgnore)
    {
      Contract.Requires(column != null);
      m_FileSetting = fileSetting ?? throw new ArgumentNullException(nameof(fileSetting));
      m_FillGuessSettings = fillGuessSettings ?? throw new ArgumentNullException(nameof(fillGuessSettings));
      m_ColumnRef = column ?? throw new ArgumentNullException(nameof(column));
      column.CopyTo(m_ColumnEdit);

      m_WriteSetting = writeSetting;

      InitializeComponent();
      // needed for TimeZon, Name or TimePart
      columnBindingSource.DataSource = m_ColumnEdit;
      // needed for Formats
      bindingSourceValueFormat.DataSource = m_ColumnEdit.ValueFormatMutable;

      comboBoxColumnName.Enabled = showIgnore;

      toolTip.SetToolTip(
        comboBoxTimeZone,
        !m_WriteSetting
          ? "Assuming the time read is based in the time zone stored in this column or a constant value and being converted to the local time zone of you system"
          : "Converting the time in the local time zone of you system to the time zone in this column or a constant value");

      labelDisplayNullAs.Visible = writeSetting;
      textBoxDisplayNullAs.Visible = writeSetting;
      checkBoxIgnore.Visible = !writeSetting && showIgnore;
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
    private void ButtonCancelClick(object sender, EventArgs e) => Close();

    /// <summary>
    ///   Handles the Click event of the buttonGuess control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    public async void ButtonGuessClick(object sender, EventArgs e)
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
            var hasRetried = false;
            retry:
            using (var sqlReader = await FunctionalDI.SQLDataReader(m_FileSetting.SqlStatement, processDisplay.SetProcess, m_FileSetting.Timeout, processDisplay.CancellationToken))
            {
              var data = await sqlReader.GetDataTableAsync(m_FileSetting.RecordLimit, false,
                m_FileSetting.DisplayStartLineNo, m_FileSetting.DisplayRecordNo, m_FileSetting.DisplayEndLineNo, false, null, null,
                processDisplay.CancellationToken);
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
              }

              found.ValueFormatMutable.DataType = column.DataType.GetDataType();
              if (found.ValueFormatMutable.DataType == DataType.String)
                return;
              m_ColumnEdit.ValueFormatMutable.DataType = found.ValueFormatMutable.DataType;
              processDisplay.Hide();

              RefreshData();
              _MessageBox.Show(
                this,
                $"Based on DataType of the source column this is {m_ColumnEdit.GetTypeAndFormatDescription()}.\nPlease choose the desired output format",
                columnName,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            }
          }
          else
          {
            var samples = await GetSampleValuesAsync(columnName, processDisplay);
            // shuffle samples, take some from the end and put it in the first 10 1 - 1 2 - Last 3 -
            // 2 4 - Last - 1

            if (samples.Values.Count == 0)
            {
              _MessageBox.Show(
                this,
                string.Format(CultureInfo.CurrentCulture, c_NoSampleDate, samples.RecordsRead),
                "Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            }
            else
            {
              var detectBool = true;
              var detectGuid = true;
              var detectNumeric = true;
              var detectDateTime = true;
              if (comboBoxDataType.SelectedValue != null)
              {
                var selectedType = (DataType) comboBoxDataType.SelectedValue;
                if (selectedType != DataType.String && selectedType != DataType.TextToHtml
                                                    && selectedType != DataType.TextToHtmlFull
                                                    && selectedType != DataType.TextPart)
                {
                  var resp = _MessageBox.Show(
                    this,
                    $"Should the system restrict detection to {selectedType}?",
                    "Selected DataType",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);
                  if (resp == DialogResult.Cancel)
                    return;
                  if (resp == DialogResult.Yes)
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
                    }
                }
              }

              // detect all (except Serial dates) and be content with 1 records if need be
              var checkResult = DetermineColumnFormat.GuessValueFormat(
                samples.Values,
                1,
                m_FillGuessSettings.TrueValue,
                m_FillGuessSettings.FalseValue,
                detectBool,
                detectGuid,
                detectNumeric,
                detectDateTime,
                detectNumeric,
                detectDateTime,
                detectDateTime,
                DetermineColumnFormat.CommonDateFormat(m_FileSetting.ColumnCollection),
                processDisplay.CancellationToken);
              processDisplay.Hide();
              if (checkResult.FoundValueFormat == null)
              {
                _MessageBox.ShowBigHtml(
                  this,
                  BuildHTMLText($"No format could be determined in {samples.Values.Count():N0} sample values of {samples.RecordsRead:N0} records.", null, 4, "Examples", samples.Values, 4),
                  $"Column: {columnName}",
                  MessageBoxButtons.OK,
                  MessageBoxIcon.Information);
              }
              else
              {
                if (checkResult.FoundValueFormat != null || checkResult.PossibleMatch)
                {
                  if (checkResult.FoundValueFormat != null)
                  {
                    m_ColumnEdit.ValueFormatMutable.CopyFrom(checkResult.FoundValueFormat);
                    if (checkResult.FoundValueFormat.DataType == DataType.DateTime)
                      AddFormatToComboBoxDateFormat(checkResult.FoundValueFormat.DateFormat);

                    // In case possible match has the same information as FoundValueFormat,
                    // disregard the possible match
                    if (checkResult.FoundValueFormat.Equals(checkResult.ValueFormatPossibleMatch))
                      checkResult.PossibleMatch = false;
                  }
                  else if (checkResult.PossibleMatch && checkResult.ValueFormatPossibleMatch != null)
                  {
                    if (checkResult.ValueFormatPossibleMatch.DataType == DataType.DateTime)
                      AddFormatToComboBoxDateFormat(checkResult.ValueFormatPossibleMatch.DateFormat);
                  }

                  var header1 = string.Empty;
                  var suggestClosestMatch = checkResult.PossibleMatch
                                            && (checkResult.FoundValueFormat == null
                                                || checkResult.FoundValueFormat.DataType == DataType.String);
                  header1 += $"Determined Format : {checkResult.FoundValueFormat?.GetTypeAndFormatDescription()}";

                  if (checkResult.PossibleMatch)
                    header1 += $"\r\nClosest match is : {checkResult.ValueFormatPossibleMatch?.GetTypeAndFormatDescription()}";

                  if (suggestClosestMatch)
                  {
                    if (_MessageBox.ShowBigHtml(
                        this,
                        BuildHTMLText(header1, "Should the closest match be used?", 4, "Samples:", samples.Values, 4, "Not matching:", checkResult.ExampleNonMatch),
                        $"Column: {columnName}",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                      // use the closest match instead of Text can not use ValueFormat.CopyTo,.
                      // Column is quite specific and need it to be set,
                      m_ColumnEdit.ValueFormatMutable.CopyFrom(checkResult.ValueFormatPossibleMatch);
                  }
                  else
                  {
                    _MessageBox.ShowBigHtml(
                      this,
                      BuildHTMLText(header1, null, 4, "Samples:", samples.Values, 4, "Not matching:", checkResult.ExampleNonMatch),
                      $"Column: {columnName}",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Information);
                  }

                  RefreshData();
                }
                else
                {
                  // add the regular samples to the invalids that are first
                  var displayMsg =
                    $"No specific format found in {samples.RecordsRead:N0} records. Need {m_FillGuessSettings.MinSamples:N0} distinct values.\n\n{checkResult.ExampleNonMatch.Concat(samples.Values).Take(42).Join("\t")}";

                  if (samples.Values.Count() < m_FillGuessSettings.MinSamples)
                  {
                    _MessageBox.ShowBig(
                      this,
                      displayMsg,
                      $"Column: {columnName}",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Information);
                  }
                  else
                  {
                    if (m_ColumnEdit.ValueFormatMutable.DataType == DataType.String)
                    {
                      _MessageBox.ShowBig(
                        this,
                        displayMsg,
                        $"Column: {columnName}",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    }
                    else
                    {
                      if (_MessageBox.ShowBig(
                        this,
                        displayMsg + "\n\nShould this be set to text?",
                        $"Column: {columnName}",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                        m_ColumnEdit.ValueFormatMutable.DataType = DataType.String;
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
    private void ButtonAddFormat_Click(object sender, EventArgs e) =>
      AddFormatToComboBoxDateFormat(comboBoxDateFormat.Text);

    private async void ButtonDisplayValues_ClickAsync(object sender, EventArgs e)
    {
      buttonDisplayValues.Enabled = false;
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        using (var processDisplay = new FormProcessDisplay("Display Values", true, m_CancellationTokenSource.Token))
        {
          processDisplay.Show(this);
          var values = await GetSampleValuesAsync(comboBoxColumnName.Text, processDisplay);
          processDisplay.Hide();
          Cursor.Current = Cursors.Default;
          if (values.Values.Count == 0)
          {
            _MessageBox.Show(
              this,
              string.Format(CultureInfo.CurrentCulture, c_NoSampleDate, values.RecordsRead),
              comboBoxColumnName.Text,
              MessageBoxButtons.OK,
              MessageBoxIcon.Information);
          }
          else
          {
            _MessageBox.ShowBigHtml(this,
              BuildHTMLText(null, null, 4, "Found values:", values.Values, 4),
              comboBoxColumnName.Text,
              MessageBoxButtons.OK,
              MessageBoxIcon.Information);
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

    private static string BuildHTMLText(string header, string footer, int rows, string headerList1, ICollection<string> values1, int col1, string headerList2 = null, ICollection<string> values2 = null, int col2 = 2)
    {
      var stringBuilder = HTMLStyle.StartHTMLDoc(System.Drawing.SystemColors.Control, "<STYLE type=\"text/css\">\r\n" +
        "  html * { font-family:'Calibri','Trebuchet MS', Arial, Helvetica, sans-serif; }\r\n" +
        "  h2 { color:DarkBlue; font-size : 12px; }\r\n" +
        "  table { border-collapse:collapse; font-size : 11px; }\r\n" +
        "  td { border: 2px solid lightgrey; padding:3px; }\r\n" +
        "</STYLE>");

      if (!string.IsNullOrEmpty(header))
        stringBuilder.Append(string.Format(ApplicationSetting.HTMLStyle.H2, HTMLStyle.TextToHtmlEncode(header)));

      ListSamples(stringBuilder, headerList1, values1, col1, rows);
      ListSamples(stringBuilder, headerList2, values2, col2, rows);

      if (!string.IsNullOrEmpty(footer))
        stringBuilder.Append(string.Format(ApplicationSetting.HTMLStyle.H2, HTMLStyle.TextToHtmlEncode(footer)));

      stringBuilder.AppendLine("</BODY>");
      stringBuilder.AppendLine("</HTML>");
      return stringBuilder.ToString();
    }

    private static void ListSamples(StringBuilder stringBuilder, string headerList, ICollection<string> values, int col, int rows)
    {
      if (values!=null && values.Count>0)
      {
        if (!string.IsNullOrEmpty(headerList))
          stringBuilder.Append(string.Format(ApplicationSetting.HTMLStyle.H2, HTMLStyle.TextToHtmlEncode(headerList)));

        stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TableOpen);
        var texts = values.Take(col * rows).ToArray();
        stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TROpen);
        for (var index = 1; index <= texts.Length; index++)
        {
          if (string.IsNullOrEmpty(texts[index-1]))
            stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TDEmpty);
          else
            stringBuilder.AppendLine(string.Format(ApplicationSetting.HTMLStyle.TD, HTMLStyle.TextToHtmlEncode(texts[index-1])));
          if (index%col==0)
            stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TRClose);
        }
        if (texts.Length%col != 0)
          stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TRClose);
        stringBuilder.AppendLine(ApplicationSetting.HTMLStyle.TableClose);
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
        if (!ValidateChildren())
          return;
        if (m_ColumnEdit.Equals(m_ColumnRef))
          return;
        Hide();
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

        m_ColumnEdit.ValueFormatMutable.DateFormat = format;
      }
      else
      {
        var parts = new List<string>(StringUtils.SplitByDelimiter(m_ColumnEdit.ValueFormatMutable.DateFormat));
        var isInList = parts.Contains(format);

        if (e.NewValue == CheckState.Checked && !isInList)
        {
          parts.Add(format);
          m_ColumnEdit.ValueFormatMutable.DateFormat = parts.Join(";");
        }

        if (e.NewValue == CheckState.Checked || !isInList)
          return;
        parts.Remove(format);
        m_ColumnEdit.ValueFormatMutable.DateFormat = parts.Join(";");
      }
    }

    private void ColumnFormatUI_FormClosing(object sender, FormClosingEventArgs e)
    {
      try
      {
        SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;

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
    private async void ColumnFormatUI_Load(object sender, EventArgs e)
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      Cursor.Current = Cursors.WaitCursor;

      try
      {
        if (m_WriteSetting)
          labelAllowedDateFormats.Text = @"Date Format:";

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
          ICollection<string> allColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

          try
          {
            if (!m_WriteSetting)
            {
              // Read Settings -- open the source that is a file if there are ignored columns need
              // to open file and get all columns
              if (m_FileSetting.ColumnCollection.Any(x => x.Ignore))
              {
                using (var fileReader = FunctionalDI.GetFileReader(m_FileSetting, null, new CustomProcessDisplay(m_CancellationTokenSource.Token)))
                {
                  await fileReader.OpenAsync(m_CancellationTokenSource.Token);
                  for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
                    allColumns.Add(fileReader.GetColumn(colIndex).Name);
                }
              }
              else
              {
                if (FunctionalDI.GetColumnHeader != null)
                {
                  var cols = await FunctionalDI.GetColumnHeader(m_FileSetting, m_CancellationTokenSource.Token);
                  if (cols != null)
                    foreach (var col in cols)
                      allColumns.Add(col);
                }
              }
            }
            else
            {
              // Write Setting ----- open the source that is SQL
              using (var fileReader = await FunctionalDI.SQLDataReader(m_FileSetting.SqlStatement.NoRecordSQL(), null, m_FileSetting.Timeout, m_CancellationTokenSource.Token))
              {
                await fileReader.OpenAsync(m_CancellationTokenSource.Token);
                for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
                  allColumns.Add(fileReader.GetColumn(colIndex).Name);
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

    private void ComboBoxColumnName_TextUpdate(object sender, EventArgs e) =>
      buttonOK.Enabled = m_FileSetting == null || !string.IsNullOrEmpty(comboBoxColumnName.Text);

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
        var selType = (DataType) comboBoxDataType.SelectedValue;
        m_ColumnEdit.ValueFormatMutable.DataType = selType;

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

        // Depending on OS and scaling a different value might be needed
        Height = tableLayoutPanelForm.Height + SystemInformation.CaptionHeight * 175 / 100;
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    private void ComboBoxTimePart_SelectedIndexChanged(object sender, EventArgs e) =>
      comboBoxTPFormat.Enabled = comboBoxTimePart.SelectedIndex >= 0;

    /// <summary>
    ///   Reapply formatting to the sample date
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void DateFormatChanged(object sender, EventArgs e)
    {
      try
      {
        var hasTimePart = !string.IsNullOrEmpty(comboBoxTimePart.Text);
        var dateFormat = sender == comboBoxDateFormat ? comboBoxDateFormat.Text : checkedListBoxDateFormats.Text;
        if (string.IsNullOrEmpty(dateFormat)) return;

        var vf = new ValueFormatMutable(DataType.DateTime)
        {
          DateFormat = dateFormat,
          DateSeparator = textBoxDateSeparator.Text,
          TimeSeparator = textBoxTimeSeparator.Text
        };
        comboBoxTPFormat.Enabled = hasTimePart;

        toolTip.SetToolTip(textBoxDateSeparator, FileFormat.GetDescription(vf.DateSeparator));
        toolTip.SetToolTip(textBoxTimeSeparator, FileFormat.GetDescription(vf.TimeSeparator));

        var sourceDate = new DateTime(2013, 4, 7, 15, 45, 50, 345, DateTimeKind.Local);

        if (hasTimePart && vf.DateFormat.IndexOfAny(new[] { 'h', 'H', 'm', 'S', 's' }) == -1)
          vf.DateFormat += " " + comboBoxTPFormat.Text;

        labelSampleDisplay.Text = StringConversion.DateTimeToString(sourceDate, vf);

        var res = comboBoxTimeZone.Text.GetPossiblyConstant();
        if (res.Item2)
        {
          // ReSharper disable once PossibleInvalidOperationException
          sourceDate = m_WriteSetting
            ? FunctionalDI.AdjustTZExport(sourceDate, res.Item1, -1, null).Value
            : FunctionalDI.AdjustTZImport(sourceDate, res.Item1, -1, null).Value;
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
    ///   Gets the sample values.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="processDisplay">The process display.</param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException">FileSetting not set</exception>
    /// <exception cref="FileException">
    ///   Column {columnName} not found. or Column {columnName} not found.
    /// </exception>
    [ItemNotNull]
    private async Task<DetermineColumnFormat.SampleResult> GetSampleValuesAsync([NotNull] string columnName,
      [NotNull] IProcessDisplay processDisplay)
    {
      if (m_FileSetting == null)
        throw new ConfigurationException("FileSetting not set");

      try
      {
        if (m_WriteSetting)
          using (var sqlReader =
            await FunctionalDI.SQLDataReader(m_FileSetting.SqlStatement,
              processDisplay.SetProcess, m_FileSetting.Timeout, processDisplay.CancellationToken))
          {
            await sqlReader.OpenAsync(processDisplay.CancellationToken);
            var colIndex = sqlReader.GetOrdinal(columnName);
            if (colIndex < 0)
              throw new FileException($"Column {columnName} not found.");

            return await DetermineColumnFormat.GetSampleValuesAsync(
              sqlReader, 0,
              colIndex,
              m_FillGuessSettings.SampleValues,
              m_FileSetting.TreatTextAsNull,
              processDisplay.CancellationToken);
          }

        // must be file reader if this is reached
        var hasRetried = false;

        var fileSettingCopy = m_FileSetting.Clone();
        // Make sure that if we do have a CSV file without header that we will skip the first row
        // that might contain headers, but its simply set as without headers.
        if (fileSettingCopy is CsvFile csv)
        {
          if (!csv.HasFieldHeader && csv.SkipRows == 0)
            csv.SkipRows = 1;
          // turn off all warnings as they will cause GetSampleValues to ignore the row
          csv.TryToSolveMoreColumns = false;
          csv.WarnDelimiterInValue = false;
          csv.WarnLineFeed = false;
          csv.WarnQuotes = false;
          csv.WarnUnknownCharacter = false;
          csv.WarnNBSP = false;
          csv.WarnQuotesInQuotes = false;
        }

        retry:
        using (var fileReader = FunctionalDI.GetFileReader(fileSettingCopy, null, processDisplay))
        {
          await fileReader.OpenAsync(processDisplay.CancellationToken);
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

          return await DetermineColumnFormat.GetSampleValuesAsync(
            fileReader,
            m_FillGuessSettings.CheckedRecords,
            colIndex,
            m_FillGuessSettings.SampleValues,
            m_FileSetting.TreatTextAsNull,
            processDisplay.CancellationToken);
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
        var vf = new ValueFormatMutable
        {
          NumberFormat = comboBoxNumberFormat.Text,
          GroupSeparator = textBoxGroupSeparator.Text,
          DecimalSeparator = textBoxDecimalSeparator.Text
        };

        toolTip.SetToolTip(textBoxDecimalSeparator, FileFormat.GetDescription(vf.DecimalSeparator));
        toolTip.SetToolTip(textBoxGroupSeparator, FileFormat.GetDescription(vf.GroupSeparator));

        var sample = StringConversion.DoubleToString(1234.567, vf);
        labelNumber.Text = $@"Input: ""{sample}""";
        labelNumberOutput.Text =
          $@"Output: ""{StringConversion.StringToDecimal(sample, FileFormat.GetChar(vf.DecimalSeparator), FileFormat.GetChar(vf.GroupSeparator), false):N}""";
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
      var di = new List<DisplayItem<int>>();
      foreach (DataType item in Enum.GetValues(typeof(DataType)))
        di.Add(new DisplayItem<int>((int) item, item.DataTypeDisplay()));
      var selValue = (int) m_ColumnEdit.ValueFormatMutable.DataType;
      comboBoxDataType.DataSource = di;
      comboBoxDataType.SelectedValue = selValue;
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

    private void SetDateFormat()
    {
      var formatsTime = new List<string>();
      AddNotExisting(
        formatsTime,
        CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.ReplaceDefaults(
          CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator,
          "//",
          CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator,
          ":"));

      AddNotExisting(
        formatsTime,
        CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ReplaceDefaults(
          CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator,
          "//",
          CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator,
          ":"));

      AddNotExisting(formatsTime, "HH:mm:ss");
      AddNotExisting(formatsTime, "HH:mm");
      AddNotExisting(formatsTime, "h:mm tt");

      comboBoxTPFormat.BeginUpdate();
      comboBoxTPFormat.Items.Clear();
      comboBoxTPFormat.Items.AddRange(formatsTime.ToArray());
      comboBoxTPFormat.EndUpdate();

      var formatsReg = new List<string>();
      var formatsExtra = new List<string>();
      AddNotExisting(
        formatsReg,
        CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ReplaceDefaults(
          CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator,
          "/",
          CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator,
          ":"));
      AddNotExisting(
        formatsReg,
        (CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " "
                                                                    + CultureInfo.CurrentCulture.DateTimeFormat
                                                                      .LongTimePattern).ReplaceDefaults(
          CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator,
          "/",
          CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator,
          ":"));
      AddNotExisting(formatsReg, "MM/dd/yyyy");
      AddNotExisting(formatsReg, "HH:mm:ss");
      AddNotExisting(formatsReg, "MM/dd/yyyy HH:mm:ss");
      AddNotExisting(formatsReg, "dd/MM/yyyy");
      AddNotExisting(formatsReg, "yyyy/MM/dd");
      var parts = StringUtils.SplitByDelimiter(m_ColumnEdit.ValueFormatMutable.DateFormat);
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
        textBoxPart.Text = @"1";
        part = 1;
      }

      if (part.Value == 1)
        checkBoxPartToEnd.Checked = false;
      var toEnd = checkBoxPartToEnd.Checked;

      labelSamplePart.Text = $@"Input: ""{sample}""";
      labelResultPart.Text = $@"Output: ""{StringConversion.StringToTextPart(sample, split, part.Value, toEnd)}""";
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

    private void TextBoxDecimalSeparator_Validating(object sender, CancelEventArgs e) =>
      errorProvider.SetError(
        textBoxDecimalSeparator,
        string.IsNullOrEmpty(textBoxDecimalSeparator.Text) ? "Must be provided" : "");

    private void TextBoxSplit_Validating(object sender, CancelEventArgs e) =>
      errorProvider.SetError(textBoxSplit, string.IsNullOrEmpty(textBoxSplit.Text) ? "Must be provided" : "");

    private void UpdateColumnList(ICollection<string> allColumns)
    {
      comboBoxColumnName.BeginUpdate();
      // if we have a list of columns add them to fields that show a column name
      if (allColumns.Count > 0)
      {
        var columnsConf = allColumns.ToArray();
        var columnsTp = allColumns.ToArray();
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