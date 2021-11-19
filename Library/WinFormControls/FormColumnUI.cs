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

#nullable enable

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
    private const string cNoSampleDate =
      "The source does not contain samples without warnings in the {0:N0} records read";

    private readonly HTMLStyle HTMLStyle;

    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

    private readonly Column m_ColumnEdit;
    private readonly IColumn? m_ColumnRef;
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
    /// <param name="hTMLStyle">The HTML style.</param>
    /// <exception cref="ArgumentNullException">fileSetting or fillGuessSettings NULL</exception>
    public FormColumnUI(
      IColumn? column,
      bool writeSetting,
      IFileSetting fileSetting,
      FillGuessSettings fillGuessSettings,
      bool showIgnore,
      HTMLStyle hTMLStyle)
    {
      m_ColumnRef = column;
      if (column == null)
        m_ColumnEdit = new Column();
      else
        m_ColumnEdit = new Column(column);
      m_FileSetting = fileSetting ?? throw new ArgumentNullException(nameof(fileSetting));
      m_FillGuessSettings = fillGuessSettings ?? throw new ArgumentNullException(nameof(fillGuessSettings));
      HTMLStyle = hTMLStyle ?? throw new ArgumentNullException(nameof(hTMLStyle));

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

    public IColumn EditedColumn => m_ColumnEdit;

    public bool ShowGuess
    {
      get => buttonGuess.Visible;
      set
      {
        buttonGuess.Visible = value;
        buttonDisplayValues.Visible = value;
      }
    }

    public void AddDateFormat(string format)
    {
      if (string.IsNullOrEmpty(format))
        return;
      try
      {
        checkedListBoxDateFormats.SafeInvoke(() =>
        {
          foreach (int ind in checkedListBoxDateFormats.CheckedIndices)
            checkedListBoxDateFormats.SetItemChecked(ind, false);

          var index = checkedListBoxDateFormats.Items.IndexOf(format);
          if (index < 0)
            index = checkedListBoxDateFormats.Items.Add(format);
          checkedListBoxDateFormats.SetItemChecked(index, true);
          checkedListBoxDateFormats.TopIndex = index;
        });
      }
      catch (Exception ex)
      {
        Logger.Information(ex, "AddDateFormat {format}", format);
      }
    }

    public async Task DisplayValues()
    {
      using var processDisplay = new FormProcessDisplay("Display Values", true, m_CancellationTokenSource.Token);
      processDisplay.Show(this);
      var values = await GetSampleValuesAsync(comboBoxColumnName.Text, processDisplay, processDisplay.CancellationToken);
      processDisplay.Hide();
      Cursor.Current = Cursors.Default;
      if (values.Values.Count == 0)
      {
        _MessageBox.Show(
          string.Format(CultureInfo.CurrentCulture, cNoSampleDate, values.RecordsRead),
          comboBoxColumnName.Text,
          MessageBoxButtons.OK,
          MessageBoxIcon.Information);
      }
      else
      {
        _MessageBox.ShowBigHtml(
          BuildHTMLText(null, null, 4, "Found values:", values.Values, 4),
          comboBoxColumnName.Text,
          MessageBoxButtons.OK,
          MessageBoxIcon.Information);
      }
    }

    /// <summary>
    ///   Examine the column and guess the format
    /// </summary>
    /// <returns></returns>
    public async Task Guess()
    {
      var columnName = comboBoxColumnName.Text;
      if (string.IsNullOrEmpty(columnName))
      {
        _MessageBox.Show("Please select a column first", "Guess");
        return;
      }

      await buttonGuess.RunWithHourglassAsync(async () =>
      {
        using var processDisplay = new FormProcessDisplay("Guess Value", true, m_CancellationTokenSource.Token);
        processDisplay.Show();
        if (m_WriteSetting)
        {
          var hasRetried = false;
          retry:
#if NET5_0_OR_GREATER
          await
#endif
          using (var sqlReader = await FunctionalDI.SQLDataReader(m_FileSetting.SqlStatement,
                                   processDisplay, m_FileSetting.Timeout, processDisplay.CancellationToken))
          {
            DataTable? data = await sqlReader.GetDataTableAsync(m_FileSetting.RecordLimit, false,
                                m_FileSetting.DisplayStartLineNo, m_FileSetting.DisplayRecordNo, m_FileSetting.DisplayEndLineNo, false,
                                null,
                                processDisplay.CancellationToken);
            var found = new Column();
            var column = data?.Columns[columnName];
            if (column is null)
            {
              if (hasRetried)
                throw new FileReaderException($"The file does not seem to contain the column {columnName}.");
              var columns = (from DataColumn col in data!.Columns select col.ColumnName).ToList();
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
              $"Based on DataType of the source column this is {m_ColumnEdit.GetTypeAndFormatDescription()}.\nPlease choose the desired output format",
              columnName,
              MessageBoxButtons.OK,
              MessageBoxIcon.Information);
          }
        }
        else
        {
          var samples = await GetSampleValuesAsync(columnName, processDisplay, processDisplay.CancellationToken);
          // shuffle samples, take some from the end and put it in the first 10 1 - 1 2 - Last 3 - 2
          // 4 - Last - 1

          if (samples.Values.Count == 0)
          {
            _MessageBox.Show(
              string.Format(CultureInfo.CurrentCulture, cNoSampleDate, samples.RecordsRead),
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
              if (selectedType < DataType.String)
              {
                var resp = _MessageBox.Show(
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
            if (checkResult.FoundValueFormat is null)
            {
              _MessageBox.ShowBigHtml(
                BuildHTMLText(
                  $"No format could be determined in {samples.Values.Count():N0} sample values of {samples.RecordsRead:N0} records.",
                  null, 4, "Examples", samples.Values, 4),
                $"Column: {columnName}",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            }
            else
            {
              if (checkResult.ValueFormatPossibleMatch != null &&
                  (checkResult.FoundValueFormat != null || checkResult.PossibleMatch))
              {
                if (checkResult.FoundValueFormat != null)
                {
                  m_ColumnEdit.ValueFormatMutable.CopyFrom(checkResult.FoundValueFormat);

                  if (checkResult.FoundValueFormat.DataType == DataType.DateTime)
                    AddDateFormat(checkResult.FoundValueFormat.DateFormat);

                  // In case possible match has the same information as FoundValueFormat, disregard
                  // the possible match
                  if (checkResult.FoundValueFormat.Equals(checkResult.ValueFormatPossibleMatch))
                    checkResult.PossibleMatch = false;
                }

                if (checkResult.ValueFormatPossibleMatch.DataType == DataType.DateTime)
                  AddDateFormat(checkResult.ValueFormatPossibleMatch.DateFormat);

                var header1 = string.Empty;
                var suggestClosestMatch = checkResult.PossibleMatch
                                          && (checkResult.FoundValueFormat is null
                                              || checkResult.FoundValueFormat.DataType == DataType.String);
                header1 += $"Determined Format : {checkResult.FoundValueFormat?.GetTypeAndFormatDescription()}";

                if (checkResult.PossibleMatch)
                  header1 +=
                    $"\r\nClosest match is : {checkResult.ValueFormatPossibleMatch?.GetTypeAndFormatDescription()}";

                if (suggestClosestMatch && checkResult.ValueFormatPossibleMatch != null)
                {
                  if (_MessageBox.ShowBigHtml(
                        BuildHTMLText(header1, "Should the closest match be used?", 4, "Samples:", samples.Values, 4,
                          "Not matching:", checkResult.ExampleNonMatch),
                        $"Column: {columnName}",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                    // use the closest match instead of Text can not use ValueFormat.CopyTo,. Column
                    // is quite specific and need it to be set,
                    m_ColumnEdit.ValueFormatMutable.CopyFrom(checkResult.ValueFormatPossibleMatch);
                }
                else
                {
                  _MessageBox.ShowBigHtml(
                    BuildHTMLText(header1, null, 4, "Samples:", samples.Values, 4, "Not matching:",
                      checkResult.ExampleNonMatch),
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
                      displayMsg,
                      $"Column: {columnName}",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Information);
                  }
                  else
                  {
                    if (_MessageBox.ShowBig(
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
      });
    }

    public void SetPartLabels(string textBoxSplitText, int part, bool toEnd)
    {
      if (string.IsNullOrEmpty(textBoxSplitText))
        return;
      var split = textBoxSplitText.WrittenPunctuationToChar();
      var sample = $"This{split}is a{split}concatenated{split}list";

      labelSamplePart.SafeInvoke(() =>
      {
        toolTip.SetToolTip(textBoxSplit, textBoxSplitText.GetDescription());
        if (part == 1)
          checkBoxPartToEnd.Checked = false;
        labelSamplePart.Text = $@"Input: ""{sample}""";
        labelResultPart.Text = $@"Output: ""{StringConversion.StringToTextPart(sample, split, part, toEnd)}""";
      });
    }

    public void UpdateDateLabel(ValueFormatMutable vf, bool hasTimePart, string timePartFormat, string timeZone)
    {
      try
      {
        var sourceDate = new DateTime(2013, 4, 7, 15, 45, 50, 345, DateTimeKind.Local);

        if (hasTimePart && vf.DateFormat.IndexOfAny(new[] { 'h', 'H', 'm', 'S', 's' }) == -1)
          vf.DateFormat += " " + timePartFormat;

        labelSampleDisplay.SafeInvoke(() =>
        {
          comboBoxTPFormat.Enabled = hasTimePart;

          toolTip.SetToolTip(textBoxDateSeparator, vf.DateSeparator.GetDescription());
          toolTip.SetToolTip(textBoxTimeSeparator, vf.TimeSeparator.GetDescription());

          labelSampleDisplay.Text = StringConversion.DateTimeToString(sourceDate, vf);

          if (timeZone.TryGetConstant(out var tz))
          {
            sourceDate = m_WriteSetting
                           ? FunctionalDI.AdjustTZExport(sourceDate, tz, null)
                           : FunctionalDI.AdjustTZImport(sourceDate, tz, null);
          }
          else
          {
            labelInputTZ.Text = string.Empty;
            labelOutPutTZ.Text = string.Empty;
          }

          labelDateOutputDisplay.Text = StringConversion.DisplayDateTime(sourceDate, CultureInfo.CurrentCulture);
        });
      }
      catch (Exception ex)
      {
        Logger.Information(ex, "UpdateDateLabel {format}", vf);
      }
    }

    public void UpdateNumericLabel(string decimalSeparator, string numberFormat, string groupSeparator)
    {
      try
      {
        if (string.IsNullOrEmpty(decimalSeparator))
          return;
        var vf = new ValueFormatMutable { NumberFormat = numberFormat, GroupSeparator = numberFormat, DecimalSeparator = groupSeparator };
        var sample = StringConversion.DoubleToString(1234.567, vf);

        labelNumber.SafeInvoke(() =>
        {
          toolTip.SetToolTip(textBoxDecimalSeparator, vf.DecimalSeparator.GetDescription());
          toolTip.SetToolTip(textBoxGroupSeparator, vf.GroupSeparator.GetDescription());

          labelNumber.Text = $@"Input: ""{sample}""";
          labelNumberOutput.Text =
            $@"Output: ""{StringConversion.StringToDecimal(sample, vf.DecimalSeparator, vf.GroupSeparator, false):N}""";
        });
      }
      catch (Exception ex)
      {
        Logger.Information(ex, "UpdateNumericLabel {decimalSeparator} {numberFormat} {groupSeparator}", decimalSeparator, numberFormat, groupSeparator);
      }
    }

    private static void AddNotExisting(List<string> list, string value, List<string>? otherList = null)
    {
      if (!list.Contains(value) && (otherList is null || !otherList.Contains(value)))
        list.Add(value);
    }

    private string BuildHTMLText(string? header, string? footer, int rows, string headerList1,
                                 ICollection<string> values1, int col1, string? headerList2 = null, ICollection<string>? values2 = null,
                                 int col2 = 2)
    {
      var stringBuilder = HTMLStyle.StartHTMLDoc(
        $"{System.Drawing.SystemColors.Control.R:X2}{System.Drawing.SystemColors.Control.G:X2}{System.Drawing.SystemColors.Control.B:X2}",
        "<STYLE type=\"text/css\">\r\n" +
        "  html * { font-family:'Calibri','Trebuchet MS', Arial, Helvetica, sans-serif; }\r\n" +
        "  h2 { color:DarkBlue; font-size : 12px; }\r\n" +
        "  table { border-collapse:collapse; font-size : 11px; }\r\n" +
        "  td { border: 2px solid lightgrey; padding:3px; }\r\n" +
        "</STYLE>");

      if (!string.IsNullOrEmpty(header))
        stringBuilder.Append(string.Format(HTMLStyle.H2, HTMLStyle.TextToHtmlEncode(header!)));

      ListSamples(stringBuilder, headerList1, values1, col1, rows);
      ListSamples(stringBuilder, headerList2, values2, col2, rows);

      if (!string.IsNullOrEmpty(footer))
        stringBuilder.Append(string.Format(HTMLStyle.H2, HTMLStyle.TextToHtmlEncode(footer!)));

      stringBuilder.AppendLine("</BODY>");
      stringBuilder.AppendLine("</HTML>");
      return stringBuilder.ToString();
    }

    /// <summary>
    ///   Handles the Click event of the buttonAddFormat control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ButtonAddFormat_Click(object? sender, EventArgs e) =>
      AddDateFormat(comboBoxDateFormat.Text);

    /// <summary>
    ///   Handles the Click event of the buttonCancel control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ButtonCancelClick(object? sender, EventArgs e) => Close();

    private async void ButtonDisplayValues_ClickAsync(object? sender, EventArgs e) =>
      await buttonDisplayValues.RunWithHourglassAsync(async () => await DisplayValues());

    /// <summary>
    ///   Handles the Click event of the buttonGuess control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private async void ButtonGuessClick(object? sender, EventArgs e) => await Guess();

    /// <summary>
    ///   Handles the Click event of the buttonOK control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ButtonOkClick(object? sender, EventArgs e)
    {
      try
      {
        if (!ValidateChildren())
          return;
        if (m_ColumnEdit.Equals(m_ColumnRef))
        {
          DialogResult = DialogResult.No;
          return;
        }

        Hide();
        DialogResult = DialogResult.Yes;
        if (m_ColumnRef is Column refCol)
        {
          m_ColumnEdit.CopyTo(refCol);
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

    /// <summary>
    ///   Handles the ItemCheck event of the checkedListBoxDateFormats control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///   The <see cref="System.Windows.Forms.ItemCheckEventArgs" /> instance containing the event data.
    /// </param>
    private void CheckedListBoxDateFormats_ItemCheck(object? sender, ItemCheckEventArgs e)
    {
      var format = checkedListBoxDateFormats.Items[e.Index].ToString();
      if (string.IsNullOrEmpty(format))
        return;
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

#if !NETFRAMEWORK
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

    private void ColumnFormatUI_FormClosing(object? sender, FormClosingEventArgs e)
    {
      try
      {
        SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;

        if (!m_CancellationTokenSource.IsCancellationRequested)
          m_CancellationTokenSource.Cancel();
      }
      catch (ObjectDisposedException)
      {
        // ignore this
      }
    }

    /// <summary>
    ///   Handles the Load event of the ColumnFormatUI control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
#if !NETFRAMEWORK
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

    private async void ColumnFormatUI_Load(object? sender, EventArgs e)
    {
      if (m_WriteSetting)
        labelAllowedDateFormats.Text = @"Date Format:";

      columnBindingSource.DataSource = m_ColumnEdit;
      SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

      using var formProcessDisplay = new FormProcessDisplay("Getting column headers", false, m_CancellationTokenSource.Token);
      formProcessDisplay.Show();
      formProcessDisplay.SetProcess($"Getting columns from source");
      // Read the column headers if possible
      ICollection<string> allColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      await this.RunWithHourglassAsync(async () =>
      {
        if (!m_WriteSetting)
        {
#if NET5_0_OR_GREATER
              await
#endif
          using var fileReader = FunctionalDI.GetFileReader(m_FileSetting, null, null, formProcessDisplay.CancellationToken);
          await fileReader.OpenAsync(formProcessDisplay.CancellationToken);
          for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
            allColumns.Add(fileReader.GetColumn(colIndex).Name);
        }
        else
        {
#if NET5_0_OR_GREATER
            await
#endif
          // Write Setting ----- open the source that is SQL
          using var fileReader = await FunctionalDI.SQLDataReader(m_FileSetting.SqlStatement.NoRecordSQL(), null,
                                   m_FileSetting.Timeout, formProcessDisplay.CancellationToken);
          await fileReader.OpenAsync(formProcessDisplay.CancellationToken);
          for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
            allColumns.Add(fileReader.GetColumn(colIndex).Name);
        }
        UpdateColumnList(allColumns);
      });
    }

    private void ComboBoxColumnName_SelectedIndexChanged(object? sender, EventArgs e)
    {
      try
      {
        buttonGuess.Enabled = comboBoxColumnName.SelectedItem != null;
        buttonDisplayValues.Enabled = comboBoxColumnName.SelectedItem != null;
        ComboBoxColumnName_TextUpdate(sender, e);
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    private void ComboBoxColumnName_TextUpdate(object? sender, EventArgs? e) =>
      buttonOK.Enabled = !string.IsNullOrEmpty(comboBoxColumnName.Text);

    /// <summary>
    ///   Handles the SelectedIndexChanged event of the comboBoxDataType control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ComboBoxDataType_SelectedIndexChanged(object? sender, EventArgs e)
    {
      try
      {
        if (comboBoxDataType.SelectedValue is null)
          return;
        var selType = (DataType) comboBoxDataType.SelectedValue;
        m_ColumnEdit.ValueFormatMutable.DataType = selType;

        groupBoxNumber.Visible = selType == DataType.Numeric || selType == DataType.Double;
        if (groupBoxNumber.Visible)
          NumberFormatChanged(sender, EventArgs.Empty);

        groupBoxDate.Visible = selType == DataType.DateTime;
        if (groupBoxDate.Visible)
          DateFormatChanged(sender, EventArgs.Empty);

        groupBoxBoolean.Visible = selType == DataType.Boolean;
        groupBoxSplit.Visible = selType == DataType.TextPart;

        if (groupBoxSplit.Visible)
          SetSamplePart(sender, EventArgs.Empty);

        // Depending on OS and scaling a different value might be needed
        Height = tableLayoutPanelForm.Height + SystemInformation.CaptionHeight * 175 / 100;
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }

    private void ComboBoxTimePart_SelectedIndexChanged(object? sender, EventArgs e) =>
      comboBoxTPFormat.Enabled = comboBoxTimePart.SelectedIndex >= 0;

    /// <summary>
    ///   Reapply formatting to the sample date
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void DateFormatChanged(object? sender, EventArgs? e)
    {
      var dateFormat = sender == comboBoxDateFormat ? comboBoxDateFormat.Text : checkedListBoxDateFormats.Text;
      if (string.IsNullOrEmpty(dateFormat)) return;

      UpdateDateLabel(
        new ValueFormatMutable()
        {
          DataType = DataType.DateTime,
          DateFormat = dateFormat,
          DateSeparator = textBoxDateSeparator.Text,
          TimeSeparator = textBoxTimeSeparator.Text
        }, !string.IsNullOrEmpty(comboBoxTimePart.Text), comboBoxTPFormat.Text, comboBoxTimeZone.Text);
    }

    /// <summary>
    ///   Gets the sample values.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="processDisplay">The process display.</param>
    /// <returns></returns>
    /// <exception cref="FileException">
    ///   Column {columnName} not found. or Column {columnName} not found.
    /// </exception>
    private async Task<DetermineColumnFormat.SampleResult> GetSampleValuesAsync(string columnName,
                                                                                IProcessDisplay? processDisplay, CancellationToken cancellationToken)
    {
      try
      {
        if (m_WriteSetting)
        {
#if NET5_0_OR_GREATER
          await
#endif
          using var sqlReader =
            await FunctionalDI.SQLDataReader(m_FileSetting.SqlStatement, processDisplay, m_FileSetting.Timeout, cancellationToken);
          await sqlReader.OpenAsync(cancellationToken);
          var colIndex = sqlReader.GetOrdinal(columnName);
          if (colIndex < 0)
            throw new FileException($"Column {columnName} not found.");
          return (await DetermineColumnFormat.GetSampleValuesAsync(sqlReader, 0, new[] { colIndex },
                                               m_FillGuessSettings.SampleValues, m_FileSetting.TreatTextAsNull, cancellationToken)
                                             .ConfigureAwait(false)).First().Value;
        }

        // must be file reader if this is reached
        var hasRetried = false;

        var fileSettingCopy = (IFileSetting) m_FileSetting.Clone();
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

#if NET5_0_OR_GREATER
        await
#endif
        // ReSharper disable once ConvertToUsingDeclaration
        using (var fileReader = FunctionalDI.GetFileReader(fileSettingCopy, null, processDisplay, cancellationToken))
        {
          await fileReader.OpenAsync(cancellationToken);
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

          return (await DetermineColumnFormat.GetSampleValuesAsync(fileReader, m_FillGuessSettings.CheckedRecords,
                                               new[] { colIndex },
                                               m_FillGuessSettings.SampleValues, m_FileSetting.TreatTextAsNull, cancellationToken)
                                             .ConfigureAwait(false)).First().Value;
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Getting Sample Values");
      }

      return new DetermineColumnFormat.SampleResult(new List<string>(), 0);
    }

    private void ListSamples(StringBuilder stringBuilder, string? headerList, ICollection<string>? values, int col,
                             int rows)
    {
      if (values is null || values.Count <= 0 || string.IsNullOrEmpty(headerList))
        return;
      if (!string.IsNullOrEmpty(headerList))
        stringBuilder.Append(string.Format(HTMLStyle.H2, HTMLStyle.TextToHtmlEncode(headerList!)));

      stringBuilder.AppendLine(HTMLStyle.TableOpen);
      var texts = values.Take(col * rows).ToArray();
      stringBuilder.AppendLine(HTMLStyle.TROpen);
      for (var index = 1; index <= texts.Length; index++)
      {
        if (string.IsNullOrEmpty(texts[index - 1]))
          stringBuilder.AppendLine(HTMLStyle.TDEmpty);
        else
          stringBuilder.AppendLine(string.Format(HTMLStyle.TD,
            HTMLStyle.TextToHtmlEncode(texts[index - 1])));
        if (index % col == 0)
          stringBuilder.AppendLine(HTMLStyle.TRClose);
      }

      if (texts.Length % col != 0)
        stringBuilder.AppendLine(HTMLStyle.TRClose);
      stringBuilder.AppendLine(HTMLStyle.TableClose);
    }

    /// <summary>
    ///   Reapply formatting to the sample number
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void NumberFormatChanged(object? sender, EventArgs? e) => UpdateNumericLabel(textBoxDecimalSeparator.Text,
      comboBoxNumberFormat.Text, textBoxDecimalSeparator.Text);

    private void PartValidating(object? sender, CancelEventArgs e)
    {
      var parse = Convert.ToInt32(numericUpDownPart.Value);

      if (parse == 1 && checkBoxPartToEnd.Checked)
      {
        errorProvider.SetError(numericUpDownPart, "Un-check or choose a later part.");
        errorProvider.SetError(checkBoxPartToEnd, "Un-check or choose a later part.");
      }
      else
      {
        errorProvider.SetError(checkBoxPartToEnd, "");
        errorProvider.SetError(numericUpDownPart, "");
      }
    }

    private void RefreshData()
    {
      SetDateFormat();
      var di = (from DataType item in Enum.GetValues(typeof(DataType)) select new DisplayItem<int>((int) item, item.DataTypeDisplay())).ToList();
      var selValue = (int) m_ColumnEdit.ValueFormatMutable.DataType;
      comboBoxDataType.DataSource = di;
      comboBoxDataType.SelectedValue = selValue;
      ComboBoxColumnName_TextUpdate(null, EventArgs.Empty);
    }

    private void RegionAndLanguageLinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
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

    private void SetSamplePart(object? sender, EventArgs? e) =>
      SetPartLabels(textBoxSplit.Text, Convert.ToInt32(numericUpDownPart.Value), checkBoxPartToEnd.Checked);

#if !NETFRAMEWORK
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif

    private void SystemEvents_UserPreferenceChanged(object? sender, UserPreferenceChangedEventArgs e)
    {
      if (e.Category != UserPreferenceCategory.Locale)
        return;
      CultureInfo.CurrentCulture.ClearCachedData();
      // Refresh The date formats presented
      SetDateFormat();
      // Update the UI
      ComboBoxDataType_SelectedIndexChanged(null, EventArgs.Empty);
    }

    private void TextBoxDecimalSeparator_Validating(object? sender, CancelEventArgs e) =>
      errorProvider.SetError(
        textBoxDecimalSeparator,
        string.IsNullOrEmpty(textBoxDecimalSeparator.Text) ? "Must be provided" : "");

    private void TextBoxSplit_Validating(object? sender, CancelEventArgs e) =>
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