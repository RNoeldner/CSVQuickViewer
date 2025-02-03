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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

// ReSharper disable CoVariantArrayConversion

namespace CsvTools
{
  /// <summary>
  ///   Windows Form UI editing a <see cref="ColumnMut" />
  /// </summary>
  public partial class FormColumnUiRead : ResizeForm
  {
    private const string cNoSampleDate =
      "The source does not contain samples without warnings in the {0:N0} records read";

    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

    private readonly ColumnMut m_ColumnEdit;
    private readonly IFileSetting m_FileSetting;
    private readonly FillGuessSettings m_FillGuessSettings;
    public Column UpdatedColumn => m_ColumnEdit.ToImmutableColumn();

    /// <summary>
    /// Function to get a FileReader for inspection of contend, in case the data is retrieved by a SQL this should be set to a different function.
    /// </summary>
    public Func<IFileSetting, CancellationToken, Task<IFileReader>> GetReaderForDetectionAsync { private get; set; }
    = (source, cancellationToken) => source.GetUntypedFileReaderAsync(cancellationToken);

    /// <summary>
    /// Function to get a Columns of a setting, in case the data is retrieved by a SQL this should be set to a different function.
    /// </summary>
    public Func<IFileSetting, CancellationToken, Task<IEnumerable<Column>>> GetReaderForColumnsAsync { private get; set; }
    = (source, cancellationToken) => source.GetAllReaderColumnsAsync(cancellationToken);

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormColumnUiRead" /> class.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="fileSetting">The file setting (need a setting to actually look into the source).</param>
    /// <param name="fillGuessSettings">The fill guess settings.</param>
    /// <param name="showIgnore">if set to <c>true</c> [show ignore].</param>
    /// <param name="showWriteNull">If set to true, the "write null" option will be shown.</param>
    /// <param name="enableChangeColumn">Allow to choose a different column</param>
    /// <exception cref="ArgumentNullException">fileSetting or fillGuessSettings NULL</exception>
    public FormColumnUiRead(
      Column column,
      IFileSetting fileSetting,
      FillGuessSettings fillGuessSettings,
      bool showIgnore,
      bool showWriteNull,
      bool enableChangeColumn)
    {
      m_ColumnEdit = new ColumnMut(column);
      m_FileSetting = fileSetting ?? throw new ArgumentNullException(nameof(fileSetting));
      m_FillGuessSettings = fillGuessSettings ?? throw new ArgumentNullException(nameof(fillGuessSettings));

      InitializeComponent();
      try
      {
        // needed for TimeZone, Name or TimePart
        columnBindingSource.DataSource = m_ColumnEdit;
        // needed for Formats
        bindingSourceValueFormat.DataSource = m_ColumnEdit.ValueFormatMut;
        comboBoxTPFormat.Text = m_ColumnEdit.TimePartFormat;

        comboBoxColumnName.Enabled = enableChangeColumn;
        checkBoxIgnore.Visible = showIgnore;

        labelDisplayNullAs.Visible = showWriteNull;
        textBoxDisplayNullAs.Visible = showWriteNull;

        toolTip.SetToolTip(
          comboBoxTimeZone,
          showWriteNull
            ? "If a time zone column is specified the datetime will be stored in the time zone specified by the column (Converted from local time zone to this time zone). You can provide a constant value, then all records will be converted."
            : "If a time zone column is specified the datetime is assumed to be in the time zone specified by the column (Converted from this time zone to local time zone). You can provide a constant value, then all records will be converted.");
      }
      catch (Exception e)
      {
        try { Logger.Warning(e, "FormColumnUiRead ctor"); } catch { };
      }
    }

    private void AddDateFormat(string format)
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
        UpdateDateLabel();
      }
      catch (Exception ex)
      {
        try { Logger.Information(ex, "AddDateFormat {format}", format); } catch { };
      }
    }

    private async Task DisplayValues()
    {
      using var formProgress = new FormProgress("Display Values", true, FontConfig, m_CancellationTokenSource.Token);

      formProgress.Show(this);
      var values = await GetSampleValuesAsync(comboBoxColumnName.Text, formProgress, formProgress.CancellationToken);
      formProgress.Hide();
      Cursor.Current = Cursors.Default;
      if (values.Values.Count == 0)
      {
        MessageBox.Show(
          string.Format(CultureInfo.CurrentCulture, cNoSampleDate, values.RecordsRead),
          comboBoxColumnName.Text,
          MessageBoxButtons.OK,
          MessageBoxIcon.Information);
      }
      else
      {
        MessageBox.ShowBigHtml(
          FormColumnUiRead.BuildHtmlText(null, null, 4, "Found values:", values.Values, 4),
          comboBoxColumnName.Text,
          MessageBoxButtons.OK,
          MessageBoxIcon.Information);
      }
    }

    /// <summary>
    ///   Examine the column and guess the format
    /// </summary>
    /// <returns></returns>
    private async Task Guess()
    {
      var columnName = comboBoxColumnName.Text;
      if (string.IsNullOrEmpty(columnName))
      {
        MessageBox.Show("Please select a column first", "Guess");
        return;
      }

      await buttonGuess.RunWithHourglassAsync(async () =>
      {
        using var formProgress = new FormProgress("Guess Value", true, FontConfig, m_CancellationTokenSource.Token);
        formProgress.Show(this);
        {
          var samples = await GetSampleValuesAsync(columnName, formProgress, formProgress.CancellationToken);
          // shuffle samples, take some from the end and put it in the first 10 1 - 1 2 - Last 3 - 2
          // 4 - Last - 1

          if (samples.Values.Count == 0)
          {
            MessageBox.Show(
              string.Format(CultureInfo.CurrentCulture, cNoSampleDate, samples.RecordsRead),
              "Information",
              MessageBoxButtons.OK,
              MessageBoxIcon.Information);
          }
          else
          {

            // detect all (except Serial dates) and be content with 1 records if need be
            var checkResult = DetermineColumnFormat.GuessValueFormat(
              samples: samples.Values,
              minRequiredSamples: 1,
              trueValue: m_FillGuessSettings.TrueValue.AsSpan(),
              falseValue: m_FillGuessSettings.FalseValue.AsSpan(),
              guessBoolean: true,
              guessGuid: true,
              guessNumeric: true,
              guessDateTime: true,
              guessPercentage: m_FillGuessSettings.DetectPercentage,
              serialDateTime: m_FillGuessSettings.SerialDateTime,
              removeCurrencySymbols: m_FillGuessSettings.RemoveCurrencySymbols,
              othersValueFormatDate: DetermineColumnFormat.CommonDateFormat(m_FileSetting.ColumnCollection, m_FillGuessSettings.DateFormat),
              cancellationToken: formProgress.CancellationToken);
            formProgress.Hide();
            if (checkResult.FoundValueFormat is null)
            {
              MessageBox.ShowBigHtml(
                FormColumnUiRead.BuildHtmlText(
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
                  m_ColumnEdit.ValueFormatMut.CopyFrom(checkResult.FoundValueFormat);
                  if (checkResult.FoundValueFormat.DataType == DataTypeEnum.DateTime)
                    AddDateFormat(checkResult.FoundValueFormat.DateFormat);

                  // In case possible match has the same information as FoundValueFormat, disregard
                  // the possible match
                  if (checkResult.FoundValueFormat.Equals(checkResult.ValueFormatPossibleMatch))
                    checkResult.PossibleMatch = false;
                }

                if (checkResult.ValueFormatPossibleMatch.DataType == DataTypeEnum.DateTime)
                  AddDateFormat(checkResult.ValueFormatPossibleMatch.DateFormat);

                var header1 = string.Empty;
                var suggestClosestMatch = checkResult.PossibleMatch
                                          && (checkResult.FoundValueFormat is null
                                              || checkResult.FoundValueFormat.DataType == DataTypeEnum.String);
                header1 += $"Determined Format : {checkResult.FoundValueFormat?.GetTypeAndFormatDescription()}";

                if (checkResult.PossibleMatch)
                  header1 +=
                    $"\r\nClosest match is : {checkResult.ValueFormatPossibleMatch?.GetTypeAndFormatDescription()}";

                if (suggestClosestMatch && checkResult.ValueFormatPossibleMatch != null)
                {
                  if (MessageBox.ShowBigHtml(
                        FormColumnUiRead.BuildHtmlText(header1, "Should the closest match be used?", 4, "Samples:", samples.Values, 4,
                          "Not matching:", checkResult.ExampleNonMatch),
                        $"Column: {columnName}",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                    // use the closest match instead of Text can not use ValueFormat.CopyTo,. Column
                    // is quite specific and need it to be set,
                    m_ColumnEdit.ValueFormatMut = new ValueFormatMut(checkResult.ValueFormatPossibleMatch);
                }
                else
                {
                  MessageBox.ShowBigHtml(
                    FormColumnUiRead.BuildHtmlText(header1, null, 4, "Samples:", samples.Values, 4, "Not matching:",
                      checkResult.ExampleNonMatch),
                    $"Column: {columnName}",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                RefreshData();
              }
              else
              {
                // add the regular samples to the invalids that are first
                var displayMsg =
                  $"No specific format found in {samples.RecordsRead:N0} records. Need {m_FillGuessSettings.MinSamples:N0} distinct values.\n\n{checkResult.ExampleNonMatch.Concat(samples.Values).Take(42).Select(x => x.Span.ToString()).Join("\t")}";

                if (samples.Values.Count() < m_FillGuessSettings.MinSamples)
                {
                  MessageBox.ShowBig(
                    displayMsg,
                    $"Column: {columnName}",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                }
                else
                {
                  if (m_ColumnEdit.ValueFormatMut.DataType == DataTypeEnum.String)
                  {
                    MessageBox.ShowBig(
                      displayMsg,
                      $"Column: {columnName}",
                      MessageBoxButtons.OK,
                      MessageBoxIcon.Information);
                  }
                  else
                  {
                    if (MessageBox.ShowBig(
                          displayMsg + "\n\nShould this be set to text?",
                          $"Column: {columnName}",
                          MessageBoxButtons.YesNo,
                          MessageBoxIcon.Question) == DialogResult.Yes)
                      m_ColumnEdit.ValueFormatMut.DataType = DataTypeEnum.String;
                  }
                }
              }
            }
          }
        }
      });
    }

    private void SetPartLabels(string textBoxSplitText, int part, bool toEnd)
    {
      if (string.IsNullOrEmpty(textBoxSplitText))
        return;
      var split = textBoxSplitText.FromText();
      var sample = $"This{split}is a{split}concatenated{split}list";

      labelSamplePart.SafeInvoke(() =>
      {
        toolTip.SetToolTip(textBoxSplit, split.Description());
        if (part == 1)
          checkBoxPartToEnd.Checked = false;
        labelSamplePart.Text = $@"Input: ""{sample}""";
        labelResultPart.Text = $@"Output: ""{sample.AsSpan().StringToTextPart(split, part, toEnd).ToString()}""";
      });
    }

    private void UpdateDateLabel()
    {
      try
      {
        labelSampleDisplay.SafeInvoke(() =>
        {
          var dateFormats = new List<string>();
          foreach (var item in checkedListBoxDateFormats.CheckedItems)
          {
            var itemStr = item.ToString();
            if (!string.IsNullOrEmpty(itemStr))
              dateFormats.Add(itemStr);
          }


          if (dateFormats.Count==0 && checkedListBoxDateFormats.SelectedIndex!=-1)
          {
            var itemStr = checkedListBoxDateFormats.Items[checkedListBoxDateFormats.SelectedIndex].ToString();
            if (!string.IsNullOrEmpty(itemStr))
              dateFormats.Add(itemStr);
          }
          if (dateFormats.Count==0)
            return;

          var hasTimePart = !string.IsNullOrEmpty(comboBoxTimePart.Text);
          var timePartFormat = comboBoxTPFormat.Text;
          var timeZone = comboBoxTimeZone.Text;
          var dateSeparator = textBoxDateSeparator.Text.FromText();
          var timeSeparator = textBoxTimeSeparator.Text.FromText();

          comboBoxTPFormat.Enabled = hasTimePart;

          toolTip.SetToolTip(textBoxDateSeparator, dateSeparator.Description());
          toolTip.SetToolTip(textBoxTimeSeparator, timeSeparator.Description());

          // using HashSet to get rid of duplicates
          var text = new HashSet<string>();

          var sourceDate = new DateTime(2013, 4, 7, 15, 45, 50, 345, DateTimeKind.Local);

          // if we have different formats, input could be different 
          foreach (var dateFormat in dateFormats)
          {
            var completeFormat = (hasTimePart && dateFormat.IndexOfAny(new[] { 'h', 'H', 'm', 'S', 's', }) == -1)
              ? dateFormat + " " + timePartFormat
              : dateFormat;
            // we always read the current culture as well as invariant
            text.Add(StringConversion.DateTimeToString(sourceDate, completeFormat, dateSeparator,
              timeSeparator, CultureInfo.InvariantCulture));
            text.Add(StringConversion.DateTimeToString(sourceDate, completeFormat, dateSeparator,
              timeSeparator, CultureInfo.CurrentCulture));

            // Reading data with Offset, means that we can have different inputs for the same outcome
            if (completeFormat.IndexOf("zzz", StringComparison.Ordinal) != -1 &&
                TimeZone.CurrentTimeZone.GetUtcOffset(sourceDate).TotalMinutes > 1)
            {
              completeFormat = completeFormat.Replace("zzz", "+00:00");
              text.Add(StringConversion.DateTimeToString(sourceDate.ToUniversalTime(),
                completeFormat, dateSeparator, timeSeparator, CultureInfo.InvariantCulture));
              text.Add(StringConversion.DateTimeToString(sourceDate.ToUniversalTime(),
                completeFormat, dateSeparator, timeSeparator, CultureInfo.CurrentCulture));
            }
          }
          labelSampleDisplay.Text = text.Join(", ");

          if (timeZone.TryGetConstant(out var tz))
          {
            sourceDate = StandardTimeZoneAdjust.ChangeTimeZone(sourceDate, tz, TimeZoneInfo.Local.Id, null);
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
        try { Logger.Information(ex, "UpdateDateLabel"); } catch { };
      }
    }

    private void UpdateNumericLabel(char decimalSeparator, string numberFormat, char groupSeparator)
    {
      try
      {
        if (decimalSeparator== char.MinValue)
          return;
        var vf = new ValueFormat(numberFormat: numberFormat,
          groupSeparator: groupSeparator.Text(),
          decimalSeparator: decimalSeparator.Text());
        var sample = StringConversion.DoubleToString(1234.567, vf);

        labelNumber.SafeInvoke(() =>
        {
          toolTip.SetToolTip(textBoxDecimalSeparator, decimalSeparator.Description());
          toolTip.SetToolTip(textBoxGroupSeparator, groupSeparator.Description());

          labelNumber.Text = $@"Input: ""{sample}""";
          labelNumberOutput.Text =
            $@"Output: ""{sample.AsSpan().StringToDecimal(vf.DecimalSeparator, vf.GroupSeparator, false, false):N}""";
        });
      }
      catch (Exception ex)
      {
        try
        {
          Logger.Information(ex, "UpdateNumericLabel {decimalSeparator} {numberFormat} {groupSeparator}",
          decimalSeparator, numberFormat, groupSeparator);
        }
        catch { };
      }
    }

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static string BuildHtmlText(string? header, string? footer, int rows, string headerList1,
      IReadOnlyCollection<ReadOnlyMemory<char>> values1, int col1, string? headerList2 = null, IReadOnlyCollection<ReadOnlyMemory<char>>? values2 = null,
      int col2 = 2)
    {
      var st = new HtmlStyle("<STYLE type=\"text/css\">\r\n" +
                             "  html * { font-family:'Calibri','Trebuchet MS', Arial, Helvetica, sans-serif; }\r\n" +
                             "  h2 { color:DarkBlue; font-size : 12px; }\r\n" +
                             "  table { border-collapse:collapse; font-size : 11px; }\r\n" +
                             "  td { border: 2px solid lightgrey; padding:3px; }\r\n" +
                             "</STYLE>");
      var stringBuilder = st.StartHtmlDoc(
        $"{System.Drawing.SystemColors.Control.R:X2}{System.Drawing.SystemColors.Control.G:X2}{System.Drawing.SystemColors.Control.B:X2}");

      if (!string.IsNullOrEmpty(header))
#pragma warning disable CS8604 // Possible null reference argument.
        stringBuilder.Append(string.Format(HtmlStyle.H2, HtmlStyle.TextToHtmlEncode(header)));
#pragma warning restore CS8604 // Possible null reference argument.

      FormColumnUiRead.ListSamples(stringBuilder, headerList1, values1, col1, rows);
      FormColumnUiRead.ListSamples(stringBuilder, headerList2, values2, col2, rows);

      if (!string.IsNullOrEmpty(footer))
#pragma warning disable CS8604 // Possible null reference argument.
        stringBuilder.Append(string.Format(HtmlStyle.H2, HtmlStyle.TextToHtmlEncode(footer)));
#pragma warning restore CS8604 // Possible null reference argument.

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
      AddDateFormat(textBoxDateFormat.Text);


    private async void ButtonDisplayValues_ClickAsync(object? sender, EventArgs e) =>
      await buttonDisplayValues.RunWithHourglassAsync(async () => await DisplayValues());

    /// <summary>
    ///   Handles the Click event of the buttonGuess control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private async void ButtonGuessClick(object? sender, EventArgs e) => await Guess();

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
      {
        var parts = new List<string>(m_ColumnEdit.ValueFormatMut.DateFormat.Split(StaticCollections.ListDelimiterChars, StringSplitOptions.RemoveEmptyEntries));
        var isInList = parts.Contains(format);

        if (e.NewValue == CheckState.Checked && !isInList)
        {
          parts.Add(format);
          m_ColumnEdit.ValueFormatMut.DateFormat = parts.Join(';');
        }

        if (e.NewValue == CheckState.Checked || !isInList)
          return;
        parts.Remove(format);
        m_ColumnEdit.ValueFormatMut.DateFormat = parts.Join(';');
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
      await this.RunWithHourglassAsync(async () =>
      {
        columnBindingSource.DataSource = m_ColumnEdit;
        SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;

        using var formProgress = new FormProgress("Getting column headers", false, FontConfig, m_CancellationTokenSource.Token);
        formProgress.Show(this);
        formProgress.SetProcess("Getting columns from source");
        HashSet<string> allColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var col in await GetReaderForColumnsAsync(m_FileSetting, formProgress.CancellationToken))
          allColumns.Add(col.Name);
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
        var selType = (DataTypeEnum) comboBoxDataType.SelectedValue;
        //m_ColumnEdit.ValueFormatMut.DataType = selType;
        var height = 10;

        groupBoxNumber.Visible = selType == DataTypeEnum.Numeric || selType == DataTypeEnum.Double;
        if (groupBoxNumber.Visible)
        {
          height = groupBoxNumber.Height;
          NumberFormatChanged(sender, EventArgs.Empty);
        }

        groupBoxDate.Visible = selType == DataTypeEnum.DateTime;
        if (groupBoxDate.Visible)
        {
          height = groupBoxDate.Height;
          if (string.IsNullOrEmpty(m_ColumnEdit.ValueFormat.DateFormat))
            m_ColumnEdit.ValueFormatMut.DateFormat = ValueFormat.Empty.DateFormat;
          UpdateDateLabel();
        }

        groupBoxBoolean.Visible = selType == DataTypeEnum.Boolean;
        if (groupBoxBoolean.Visible)
          height = groupBoxBoolean.Height;
        groupBoxSplit.Visible = selType == DataTypeEnum.TextPart;
        if (groupBoxSplit.Visible)
        {
          height = groupBoxSplit.Height;
          SetSamplePart(sender, EventArgs.Empty);
        }


        groupBoxRegExReplace.Visible = selType == DataTypeEnum.TextReplace;
        if (groupBoxRegExReplace.Visible)
          height = groupBoxRegExReplace.Height;

        groupBoxBinary.Visible = selType == DataTypeEnum.Binary;
        if (groupBoxBinary.Visible)
          height = groupBoxBinary.Height;

        if (groupBoxBinary.Visible && m_ColumnEdit.ValueFormat.DateFormat == ValueFormat.Empty.DateFormat)
          m_ColumnEdit.ValueFormatMut.DateFormat = string.Empty;
        flowLayoutPanel.Top = panelTop.Height;
        // Depending on OS and scaling a different value might be needed
        Height = height + 10 + (SystemInformation.CaptionHeight * 175 / 100) + panelTop.Height +
                 panelBottom.Height;
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
    /// <param name="args">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void DateFormatChanged(object? sender, EventArgs args)
    {
      UpdateDateLabel();
    }

    /// <summary>
    ///   Gets the sample values.
    /// </summary>
    /// <param name="columnName">Name of the column.</param>
    /// <param name="progress">Process display to pass on progress information</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns></returns>
    /// <exception cref="FileException">
    ///   Column {columnName} not found. or Column {columnName} not found.
    /// </exception>
    private async Task<DetermineColumnFormat.SampleResult> GetSampleValuesAsync(string columnName,
      IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      try
      {
        // must be file reader if this is reached
        var hasRetried = false;

        retry:
#if NET5_0_OR_GREATER
        await
#endif
        // ReSharper disable once ConvertToUsingDeclaration
        // ReSharper disable once UseAwaitUsing
        using (var fileReader = await GetReaderForDetectionAsync(m_FileSetting, cancellationToken))
        {
          if (progress != null)
            fileReader.ReportProgress = progress;

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

            throw new ApplicationException($"Column {columnName} not found.");
          }

          return (await DetermineColumnFormat.GetSampleValuesAsync(fileReader, m_FillGuessSettings.CheckedRecords,
              new[] { colIndex },
              m_FillGuessSettings.SampleValues, m_FileSetting.TreatTextAsNull, 500, cancellationToken)
            .ConfigureAwait(false)).First().Value;
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Getting Sample Values");
      }

      return new DetermineColumnFormat.SampleResult(new List<string>(), 0);
    }

    private static void ListSamples(StringBuilder stringBuilder, string? headerList, IReadOnlyCollection<ReadOnlyMemory<char>>? values, int col,
      int rows)
    {
      if (values is null || values.Count <= 0 || headerList is null || headerList.Length == 0)
        return;
      stringBuilder.Append(string.Format(HtmlStyle.H2, HtmlStyle.TextToHtmlEncode(headerList)));
      stringBuilder.AppendLine(HtmlStyle.TableOpen);
      var texts = values.Take(col * rows).ToArray();
      stringBuilder.AppendLine(HtmlStyle.TrOpen);
      for (var index = 1; index <= texts.Length; index++)
      {
        if (string.IsNullOrEmpty(texts[index - 1].Span.ToString()))
          stringBuilder.AppendLine(HtmlStyle.TdEmpty);
        else
          stringBuilder.AppendLine(string.Format(HtmlStyle.Td,
            HtmlStyle.TextToHtmlEncode(texts[index - 1].Span.ToString())));
        if (index % col == 0)
          stringBuilder.AppendLine(HtmlStyle.TrClose);
      }

      if (texts.Length % col != 0)
        stringBuilder.AppendLine(HtmlStyle.TrClose);
      stringBuilder.AppendLine(HtmlStyle.TableClose);
    }

    /// <summary>
    ///   Reapply formatting to the sample number
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void NumberFormatChanged(object? sender, EventArgs e) => UpdateNumericLabel(textBoxDecimalSeparator.Text.FromText(),
      comboBoxNumberFormat.Text, textBoxDecimalSeparator.Text.FromText());

    private void PartValidating(object? sender, CancelEventArgs e)
    {
      var parse = Convert.ToInt32(numericUpDownPart.Value);
      errorProvider.SetError(textBoxSplit, string.IsNullOrEmpty(textBoxSplit.Text) ? "Must be provided" : "");
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
      comboBoxDataType.SetEnumDataSource(m_ColumnEdit.ValueFormatMut.DataType);
      ComboBoxColumnName_TextUpdate(null, EventArgs.Empty);
    }

    private void SetDateFormat()
    {
      comboBoxTPFormat.BeginUpdate();
      comboBoxTPFormat.Items.Clear();
      comboBoxTPFormat.Items.AddRange(DateTimeConstants.CommonTimeFormats().ToArray());
      comboBoxTPFormat.EndUpdate();

      checkedListBoxDateFormats.BeginUpdate();
      checkedListBoxDateFormats.Items.Clear();
      checkedListBoxDateFormats.Items.AddRange(DateTimeConstants.CommonDateTimeFormats(m_ColumnEdit.ValueFormatMut.DateFormat).ToArray());

      checkedListBoxDateFormats.EndUpdate();

      // Check all items in parts
      var parts = (IEnumerable<string>) m_ColumnEdit.ValueFormatMut.DateFormat.Split(StaticCollections.ListDelimiterChars, StringSplitOptions.RemoveEmptyEntries);
      foreach (var format in parts)
      {
        var index = checkedListBoxDateFormats.Items.IndexOf(format);
        checkedListBoxDateFormats.SetItemChecked(index, true);
        checkedListBoxDateFormats.SelectedIndex = index;
      }
    }

    private void SetSamplePart(object? sender, EventArgs e) =>
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


    private void UpdateColumnList(ICollection<string> allColumns)
    {
      comboBoxColumnName.BeginUpdate();
      // if we have a list of columns add them to fields that show a column name
      if (allColumns.Count > 0)
      {
        var columnsConf = allColumns.ToArray();
        comboBoxColumnName.Items.AddRange(columnsConf);
        comboBoxTimePart.Items.AddRange(columnsConf);
        comboBoxTimeZone.Items.AddRange(columnsConf);
        if (string.IsNullOrEmpty(m_ColumnEdit.Name))
          m_ColumnEdit.Name = columnsConf[0];
      }
      else
      {
        // If list would be empty add at least the current value
        if (!string.IsNullOrEmpty(m_ColumnEdit.Name))
          comboBoxColumnName.Items.Add(m_ColumnEdit.Name);
      }

      if (comboBoxColumnName.Items.Count > 0)
        comboBoxColumnName.DropDownStyle = ComboBoxStyle.DropDownList;
      comboBoxColumnName.EndUpdate();

      RefreshData();
    }

    private void TextBoxRegexSearchPattern_Validating(object sender, CancelEventArgs e)
    {
      errorProvider.SetError(textBoxRegexSearchPattern, string.Empty);
      try
      {
        _ = new Regex(textBoxRegexSearchPattern.Text, RegexOptions.Compiled);
      }
      catch (Exception ex)
      {
        errorProvider.SetError(textBoxRegexSearchPattern, ex.Message);
      }
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

    private void CheckedListBoxDateFormats_SelectedIndexChanged(object sender, EventArgs e)
    {
      UpdateDateLabel();

      if (string.IsNullOrEmpty(textBoxDateFormat.Text) ||
            checkedListBoxDateFormats.Items.IndexOf(textBoxDateFormat.Text) != -1)
        textBoxDateFormat.Text = checkedListBoxDateFormats.Text;
    }
  }
}