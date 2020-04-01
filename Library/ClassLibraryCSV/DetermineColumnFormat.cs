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
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   Helper class
  /// </summary>
  public static class DetermineColumnFormat
  {
    public static ValueFormat CommonDateFormat(IEnumerable<Column> columns)
    {
      ValueFormat best = null;
      if (columns == null) return null;
      var counterByFormat = new Dictionary<ValueFormat, int>();
      var maxValue = int.MinValue;
      foreach (var newColumn in columns)
      {
        if (newColumn?.ValueFormat == null || newColumn.ValueFormat.DataType != DataType.DateTime)
          continue;
        var vf = newColumn.ValueFormat;
        if (!counterByFormat.ContainsKey(vf))
          counterByFormat.Add(vf, 0);

        if (++counterByFormat[vf] <= maxValue) continue;
        maxValue = counterByFormat[vf];
        best = vf;
      }

      return best;
    }

    /// <summary>
    ///   Fills the Column Format for reader fileSettings
    /// </summary>
    /// <param name="fileSetting">The file setting to check, and fill</param>
    /// <param name="addTextColumns">if set to <c>true</c> event string columns are added.</param>
    /// <param name="checkDoubleToBeInteger">if set to <c>true</c> [check double to be integer].</param>
    /// <param name="fillGuessSettings">The fill guess settings.</param>
    /// <param name="processDisplay">The process display.</param>
    /// <returns>A list of columns with new format that have been changed</returns>
    /// <exception cref="ArgumentNullException">processDisplay</exception>
    public static async Task<IList<string>> FillGuessColumnFormatReaderAsync(this IFileSetting fileSetting, bool addTextColumns,
      bool checkDoubleToBeInteger, FillGuessSettings fillGuessSettings, IProcessDisplay processDisplay)
    {
      if (processDisplay == null)
        throw new ArgumentNullException(nameof(processDisplay));
      if (fileSetting == null)
        throw new ArgumentNullException(nameof(fileSetting));

      var result = new List<string>();

      // if we should not detect, we can finish
      if (!fillGuessSettings.Enabled || !(fillGuessSettings.DectectNumbers || fillGuessSettings.DetectBoolean || fillGuessSettings.DetectDateTime ||
                                          fillGuessSettings.DetectGUID || fillGuessSettings.DectectPercentage || fillGuessSettings.SerialDateTime))
        return result;

      var present = new Collection<Column>(fileSetting.ColumnCollection);

      var fileSettingCopy = fileSetting.Clone();

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

      var othersValueFormatDate = CommonDateFormat(present);
      // need a dummy process display to have pass in Cancellation token to reader
      using (var prc2 = new DummyProcessDisplay(processDisplay.CancellationToken))
      using (var fileReader = FunctionalDI.GetFileReader(fileSettingCopy, null, prc2))
      {
        Contract.Assume(fileReader != null);
        fileReader.Open();
        if (fileReader.FieldCount == 0 || fileReader.EndOfFile)
          return result;
        processDisplay.SetProcess("Getting column headers", -1, true);
        processDisplay.Maximum = fileReader.FieldCount;

        // build a list of columns to check
        var getSamples = new List<int>();
        var columnNamesInFile = new List<string>();
        for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
        {
          var newColumn = fileReader.GetColumn(colIndex);
          columnNamesInFile.Add(newColumn.Name);

          if (BaseFileReader.cStartLineNumberFieldName.Equals(newColumn.Name, StringComparison.OrdinalIgnoreCase) ||
              BaseFileReader.cErrorField.Equals(newColumn.Name, StringComparison.OrdinalIgnoreCase))
          {
            processDisplay.SetProcess(newColumn.Name + " – Reserved columns ignored", colIndex, true);
            newColumn.Ignore = true;
            fileSetting.ColumnCollection.AddIfNew(newColumn);
          }
          else if (fillGuessSettings.IgnoreIdColums && StringUtils.AssumeIDColumn(newColumn.Name) > 0)
          {
            processDisplay.SetProcess(newColumn.Name + " – ID columns ignored", colIndex, true);
            if (addTextColumns)
              fileSetting.ColumnCollection.AddIfNew(newColumn);
          }
          else
          {
            getSamples.Add(colIndex);
          }
        }

        processDisplay.SetProcess($"Getting sample values for all {getSamples.Count} columns", -1, true);
        var sampleList = await GetSampleValuesAsync(fileReader, fillGuessSettings.CheckedRecords,
          getSamples, fillGuessSettings.SampleValues, fileSetting.TreatTextAsNull,
          processDisplay.CancellationToken);

        foreach (var colIndex in sampleList.Keys)
        {
          processDisplay.CancellationToken.ThrowIfCancellationRequested();

          var newColumn = fileReader.GetColumn(colIndex);
          var samples = sampleList[colIndex];

          processDisplay.CancellationToken.ThrowIfCancellationRequested();
          if (samples.Values.Count == 0)
          {
            processDisplay.SetProcess(newColumn.Name + " – No values found", colIndex, true);
            if (!addTextColumns)
              continue;
            result.Add($"{newColumn.Name} – No values found – Format : {newColumn.GetTypeAndFormatDescription()}");
            fileSetting.ColumnCollection.AddIfNew(newColumn);
          }
          else
          {
            var detect = true;
            if (samples.Values.Count() < fillGuessSettings.MinSamples)
            {
              processDisplay.SetProcess(
                $"{newColumn.Name} – Only {samples.Values.Count()} values found in {samples.RecordsRead:N0} rows",
                colIndex, true);
              detect = false;
            }
            else
            {
              processDisplay.SetProcess(
                $"{newColumn.Name} – {samples.Values.Count()} values found in {samples.RecordsRead:N0} rows – Examining format",
                colIndex, true);
            }

            var checkResult = GuessValueFormat(samples.Values, fillGuessSettings.MinSamples,
              fillGuessSettings.TrueValue,
              fillGuessSettings.FalseValue,
              fillGuessSettings.DetectBoolean,
              fillGuessSettings.DetectGUID && detect,
              fillGuessSettings.DectectNumbers && detect,
              fillGuessSettings.DetectDateTime && detect,
              fillGuessSettings.DectectPercentage && detect,
              fillGuessSettings.SerialDateTime && detect,
              fillGuessSettings.CheckNamedDates && detect,
              othersValueFormatDate,
              processDisplay.CancellationToken);

            if (checkResult == null)
            {
              if (addTextColumns)
                checkResult = new CheckResult { FoundValueFormat = new ValueFormat() };
              else
                continue;
            }

            var oldColumn = fileSetting.ColumnCollection.Get(newColumn.Name);
            // if we have a mapping to a template that expects a integer and we only have integers
            // but not enough
            if (oldColumn != null)
            {
              var oldValueFormat = oldColumn.GetTypeAndFormatDescription();

              if (checkResult.FoundValueFormat.DataType == DataType.DateTime && checkResult.PossibleMatch)
              {
                // if we have a date value format already store this
                if (othersValueFormatDate == null)
                {
                  othersValueFormatDate = checkResult.FoundValueFormat;
                }
                else
                {
                  // if he date format does not match the last found date format reset the assumed
                  // correct format
                  if (!othersValueFormatDate.Equals(checkResult.FoundValueFormat))
                    othersValueFormatDate = null;
                }
              }

              if (checkResult.FoundValueFormat.Equals(oldColumn.ValueFormat))
                processDisplay.SetProcess($"{newColumn.Name} – Format : {oldValueFormat} – not changed",
                  colIndex, true);
              else
                checkResult.FoundValueFormat.CopyTo(oldColumn.ValueFormat);

              var newValueFormat = checkResult.FoundValueFormat.GetTypeAndFormatDescription();
              if (oldValueFormat.Equals(newValueFormat, StringComparison.Ordinal))
                continue;
              var msg = $"{newColumn.Name} – Format : {newValueFormat} – updated from {oldValueFormat}";
              result.Add(msg);
              processDisplay.SetProcess(msg, colIndex, true);
            }
            else
            {
              if (!addTextColumns && checkResult.FoundValueFormat.DataType == DataType.String)
                continue;
              checkResult.FoundValueFormat.CopyTo(newColumn.ValueFormat);
              var msg = $"{newColumn.Name} – Format : {newColumn.GetTypeAndFormatDescription()}";
              processDisplay.SetProcess(msg, colIndex, true);
              result.Add(msg);
              fileSetting.ColumnCollection.AddIfNew(newColumn);

              // Adjust or Set the common date format
              if (newColumn.ValueFormat.DataType == DataType.DateTime)
                othersValueFormatDate = CommonDateFormat(fileSetting.ColumnCollection);
            }
          }
        }

        processDisplay.CancellationToken.ThrowIfCancellationRequested();

        // The fileReader does not have the column information yet, let the reader know
        fileReader.OverrideColumnFormatFromSetting();

        // check all doubles if they could be integer needed for excel files as the typed values do
        // not distinguish between double and integer.
        if (checkDoubleToBeInteger)
          for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
          {
            processDisplay.CancellationToken.ThrowIfCancellationRequested();

            var oldColumn = fileReader.GetColumn(colIndex);
            var detect = !(fillGuessSettings.IgnoreIdColums &&
                           StringUtils.AssumeIDColumn(oldColumn.Name) > 0);

            if (oldColumn == null || oldColumn.ValueFormat.DataType != DataType.Double) continue;
            Column newColumn = null;

            if (detect)
            {
              SampleResult samples;
              if (sampleList.Keys.Contains(colIndex + 1))
                samples = sampleList[colIndex + 1];
              else
                samples = await GetSampleValuesAsync(fileReader, fillGuessSettings.CheckedRecords,
                  colIndex, fillGuessSettings.SampleValues, fileSetting.TreatTextAsNull,
                  processDisplay.CancellationToken);

              if (samples.Values.Count > 0)
              {
                var checkResult = GuessNumeric(samples.Values, false, true, processDisplay.CancellationToken);
                if (checkResult != null && checkResult.FoundValueFormat.DataType != DataType.Double)
                {
                  newColumn = fileSetting.ColumnCollection.Get(oldColumn.Name) ??
                              fileSetting.ColumnCollection.AddIfNew(oldColumn);

                  newColumn.ValueFormat.DataType = checkResult.FoundValueFormat.DataType;
                }
              }
            }
            else
            {
              newColumn = fileSetting.ColumnCollection.Get(oldColumn.Name) ??
                          fileSetting.ColumnCollection.AddIfNew(oldColumn);
              newColumn.ValueFormat.DataType = DataType.String;
            }

            if (newColumn != null)
            {
              var msg = $"{newColumn.Name} – Overwritten Excel Format : {newColumn.GetTypeAndFormatDescription()}";
              processDisplay.SetProcess(msg, colIndex, true);
              result.Add(msg);
            }
          }

        if (fillGuessSettings.DateParts)
        {
          // Try to find a time for a date if the date does not already have a time Case a)
          // TimeFormat has already been recognized
          for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
          {
            processDisplay.CancellationToken.ThrowIfCancellationRequested();
            var columnDate = fileReader.GetColumn(colIndex);

            // Possibly add Time Zone
            if (columnDate.ValueFormat.DataType == DataType.DateTime && string.IsNullOrEmpty(columnDate.TimeZonePart))
              for (var colTimeZone = 0; colTimeZone < fileReader.FieldCount; colTimeZone++)
              {
                var columnTimeZone = fileReader.GetColumn(colTimeZone);
                var colName = columnTimeZone.Name.NoSpecials().ToUpperInvariant();
                if (columnTimeZone.ValueFormat.DataType != DataType.String && columnTimeZone.ValueFormat.DataType != DataType.Integer ||
                    colName != "TIMEZONE" && colName != "TIMEZONEID" && colName != "TIME ZONE" &&
                    colName != "TIME ZONE ID")
                  continue;

                columnDate.TimeZonePart = columnTimeZone.Name;
                result.Add($"{columnDate.Name} – Added Time Zone : {columnTimeZone.Name}");
              }

            if (columnDate.ValueFormat.DataType != DataType.DateTime || !string.IsNullOrEmpty(columnDate.TimePart) ||
                columnDate.ValueFormat.DateFormat.IndexOfAny(new[] { ':', 'h', 'H', 'm', 's', 't' }) != -1)
              continue;
            // We have a date column without time
            for (var colTime = 0; colTime < fileReader.FieldCount; colTime++)
            {
              var columnTime = fileReader.GetColumn(colTime);
              if (columnTime.ValueFormat.DataType != DataType.DateTime || !string.IsNullOrEmpty(columnDate.TimePart) ||
                  columnTime.ValueFormat.DateFormat.IndexOfAny(new[] { '/', 'y', 'M', 'd' }) != -1)
                continue;
              // We now have a time column, checked if the names somehow make sense
              if (!columnDate.Name.NoSpecials().ToUpperInvariant().Replace("DATE", string.Empty).Equals(
                columnTime.Name.NoSpecials().ToUpperInvariant().Replace("TIME", string.Empty),
                StringComparison.Ordinal))
                continue;

              columnDate.TimePart = columnTime.Name;
              columnDate.TimePartFormat = columnTime.ValueFormat.DateFormat;
              result.Add($"{columnDate.Name} – Added Time Part : {columnTime.Name}");
            }
          }

          // Case b) TimeFormat has not been recognized (e.G. all values are 08:00) only look in
          // adjacent fields
          for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
          {
            processDisplay.CancellationToken.ThrowIfCancellationRequested();
            var columnDate = fileReader.GetColumn(colIndex);
            if (columnDate.ValueFormat.DataType != DataType.DateTime || !string.IsNullOrEmpty(columnDate.TimePart) ||
                columnDate.ValueFormat.DateFormat.IndexOfAny(new[] { ':', 'h', 'H', 'm', 's', 't' }) != -1)
              continue;

            if (colIndex + 1 < fileReader.FieldCount)
            {
              var columnTime = fileReader.GetColumn(colIndex + 1);
              if (columnTime.ValueFormat.DataType == DataType.String && columnDate.Name.NoSpecials().ToUpperInvariant()
                    .Replace("DATE", string.Empty)
                    .Equals(columnTime.Name.NoSpecials().ToUpperInvariant().Replace("TIME", string.Empty),
                      StringComparison.OrdinalIgnoreCase))
              {
                columnDate.TimePart = columnTime.Name;
                {
                  var samples = sampleList.Keys.Contains(colIndex + 1)
                    ? sampleList[colIndex + 1]
                    : await GetSampleValuesAsync(fileReader, 1, colIndex + 1, 1, fileSetting.TreatTextAsNull,
                      processDisplay.CancellationToken);

                  foreach (var first in samples.Values)
                  {
                    if (first.Length == 8 || first.Length == 5)
                    {
                      columnTime.ValueFormat.DataType = DataType.DateTime;
                      var val = new ValueFormat(DataType.DateTime)
                      {
                        DateFormat = first.Length == 8 ? "HH:mm:ss" : "HH:mm"
                      };
                      val.CopyTo(columnTime.ValueFormat);
                      fileSetting.ColumnCollection.AddIfNew(columnTime);
                      result.Add($"{columnTime.Name} – Format : {columnTime.GetTypeAndFormatDescription()}");
                    }

                    break;
                  }
                }

                result.Add($"{columnDate.Name} – Added Time Part : {columnTime.Name}");
                continue;
              }
            }

            if (colIndex <= 0)
              continue;
            {
              var columnTime = fileReader.GetColumn(colIndex - 1);
              if (columnTime.ValueFormat.DataType != DataType.String ||
                  !columnDate.Name.NoSpecials().ToUpperInvariant().Replace("DATE", string.Empty).Equals(
                    columnTime.Name.NoSpecials().ToUpperInvariant().Replace("TIME", string.Empty),
                    StringComparison.Ordinal))
                continue;

              columnDate.TimePart = columnTime.Name;
              {
                var samples = sampleList.Keys.Contains(colIndex - 1)
                  ? sampleList[colIndex - 1]
                  : await GetSampleValuesAsync(fileReader, 1, colIndex - 1, 1, fileSetting.TreatTextAsNull,
                    processDisplay.CancellationToken);
                foreach (var first in samples.Values)
                {
                  if (first.Length == 8 || first.Length == 5)
                  {
                    var val = new ValueFormat(DataType.DateTime)
                    {
                      DateFormat = first.Length == 8 ? "HH:mm:ss" : "HH:mm"
                    };
                    fileSetting.ColumnCollection.AddIfNew(columnTime);
                    val.CopyTo(columnTime.ValueFormat);
                    result.Add($"{columnTime.Name} – Format : {columnTime.GetTypeAndFormatDescription()}");
                  }

                  break;
                }
              }
              result.Add($"{columnDate.Name} – Added Time Part : {columnTime.Name}");
            }
          }
        }

        var existing = new Collection<Column>();
        foreach (var colName in columnNamesInFile)
          foreach (var col in fileSetting.ColumnCollection)
          {
            if (!col.Name.Equals(colName, StringComparison.OrdinalIgnoreCase))
              continue;
            existing.Add(col);
            break;
          }

        // 2nd columns defined but not in list
        foreach (var col in fileSetting.ColumnCollection)
          if (!existing.Contains(col))
            existing.Add(col);

        fileSetting.ColumnCollection.Clear();
        if (existing == null) return result;
        foreach (var column in existing)
          fileSetting.ColumnCollection.AddIfNew(column);
      }

      return result;
    }

    public static IList<string> FillGuessColumnFormatReader(this IFileSetting fileSetting, bool addTextColumns,
      bool checkDoubleToBeInteger, FillGuessSettings fillGuessSettings, IProcessDisplay processDisplay)
    {
      if (processDisplay == null)
        throw new ArgumentNullException(nameof(processDisplay));
      if (fileSetting == null)
        throw new ArgumentNullException(nameof(fileSetting));

      var result = new List<string>();

      // if we should not detect, we can finish
      if (!fillGuessSettings.Enabled || !(fillGuessSettings.DectectNumbers || fillGuessSettings.DetectBoolean || fillGuessSettings.DetectDateTime ||
                                          fillGuessSettings.DetectGUID || fillGuessSettings.DectectPercentage || fillGuessSettings.SerialDateTime))
        return result;

      var present = new Collection<Column>(fileSetting.ColumnCollection);

      var fileSettingCopy = fileSetting.Clone();

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

      var othersValueFormatDate = CommonDateFormat(present);
      // need a dummy process display to have pass in Cancellation token to reader
      using (var prc2 = new DummyProcessDisplay(processDisplay.CancellationToken))
      using (var fileReader = FunctionalDI.GetFileReader(fileSettingCopy, null, prc2))
      {
        Contract.Assume(fileReader != null);
        fileReader.Open();
        if (fileReader.FieldCount == 0 || fileReader.EndOfFile)
          return result;
        processDisplay.SetProcess("Getting column headers", -1, true);
        processDisplay.Maximum = fileReader.FieldCount;

        // build a list of columns to check
        var getSamples = new List<int>();
        var columnNamesInFile = new List<string>();
        for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
        {
          var newColumn = fileReader.GetColumn(colIndex);
          columnNamesInFile.Add(newColumn.Name);

          if (BaseFileReader.cStartLineNumberFieldName.Equals(newColumn.Name, StringComparison.OrdinalIgnoreCase) ||
              BaseFileReader.cErrorField.Equals(newColumn.Name, StringComparison.OrdinalIgnoreCase))
          {
            processDisplay.SetProcess(newColumn.Name + " – Reserved columns ignored", colIndex, true);
            newColumn.Ignore = true;
            fileSetting.ColumnCollection.AddIfNew(newColumn);
          }
          else if (fillGuessSettings.IgnoreIdColums && StringUtils.AssumeIDColumn(newColumn.Name) > 0)
          {
            processDisplay.SetProcess(newColumn.Name + " – ID columns ignored", colIndex, true);
            if (addTextColumns)
              fileSetting.ColumnCollection.AddIfNew(newColumn);
          }
          else
          {
            getSamples.Add(colIndex);
          }
        }

        processDisplay.SetProcess($"Getting sample values for all {getSamples.Count} columns", -1, true);
        var sampleList = GetSampleValues(fileReader, fillGuessSettings.CheckedRecords,
          getSamples, fillGuessSettings.SampleValues, fileSetting.TreatTextAsNull,
          processDisplay.CancellationToken);

        foreach (var colIndex in sampleList.Keys)
        {
          processDisplay.CancellationToken.ThrowIfCancellationRequested();

          var newColumn = fileReader.GetColumn(colIndex);
          var samples = sampleList[colIndex];

          processDisplay.CancellationToken.ThrowIfCancellationRequested();
          if (samples.Values.Count == 0)
          {
            processDisplay.SetProcess(newColumn.Name + " – No values found", colIndex, true);
            if (!addTextColumns)
              continue;
            result.Add($"{newColumn.Name} – No values found – Format : {newColumn.GetTypeAndFormatDescription()}");
            fileSetting.ColumnCollection.AddIfNew(newColumn);
          }
          else
          {
            var detect = true;
            if (samples.Values.Count() < fillGuessSettings.MinSamples)
            {
              processDisplay.SetProcess(
                $"{newColumn.Name} – Only {samples.Values.Count()} values found in {samples.RecordsRead:N0} rows",
                colIndex, true);
              detect = false;
            }
            else
            {
              processDisplay.SetProcess(
                $"{newColumn.Name} – {samples.Values.Count()} values found in {samples.RecordsRead:N0} rows – Examining format",
                colIndex, true);
            }

            var checkResult = GuessValueFormat(samples.Values, fillGuessSettings.MinSamples,
              fillGuessSettings.TrueValue,
              fillGuessSettings.FalseValue,
              fillGuessSettings.DetectBoolean,
              fillGuessSettings.DetectGUID && detect,
              fillGuessSettings.DectectNumbers && detect,
              fillGuessSettings.DetectDateTime && detect,
              fillGuessSettings.DectectPercentage && detect,
              fillGuessSettings.SerialDateTime && detect,
              fillGuessSettings.CheckNamedDates && detect,
              othersValueFormatDate,
              processDisplay.CancellationToken);

            if (checkResult == null)
            {
              if (addTextColumns)
                checkResult = new CheckResult { FoundValueFormat = new ValueFormat() };
              else
                continue;
            }

            var oldColumn = fileSetting.ColumnCollection.Get(newColumn.Name);
            // if we have a mapping to a template that expects a integer and we only have integers
            // but not enough
            if (oldColumn != null)
            {
              var oldValueFormat = oldColumn.GetTypeAndFormatDescription();

              if (checkResult.FoundValueFormat.DataType == DataType.DateTime && checkResult.PossibleMatch)
              {
                // if we have a date value format already store this
                if (othersValueFormatDate == null)
                {
                  othersValueFormatDate = checkResult.FoundValueFormat;
                }
                else
                {
                  // if he date format does not match the last found date format reset the assumed
                  // correct format
                  if (!othersValueFormatDate.Equals(checkResult.FoundValueFormat))
                    othersValueFormatDate = null;
                }
              }

              if (checkResult.FoundValueFormat.Equals(oldColumn.ValueFormat))
                processDisplay.SetProcess($"{newColumn.Name} – Format : {oldValueFormat} – not changed",
                  colIndex, true);
              else
                checkResult.FoundValueFormat.CopyTo(oldColumn.ValueFormat);

              var newValueFormat = checkResult.FoundValueFormat.GetTypeAndFormatDescription();
              if (oldValueFormat.Equals(newValueFormat, StringComparison.Ordinal))
                continue;
              var msg = $"{newColumn.Name} – Format : {newValueFormat} – updated from {oldValueFormat}";
              result.Add(msg);
              processDisplay.SetProcess(msg, colIndex, true);
            }
            else
            {
              if (!addTextColumns && checkResult.FoundValueFormat.DataType == DataType.String)
                continue;
              checkResult.FoundValueFormat.CopyTo(newColumn.ValueFormat);
              var msg = $"{newColumn.Name} – Format : {newColumn.GetTypeAndFormatDescription()}";
              processDisplay.SetProcess(msg, colIndex, true);
              result.Add(msg);
              fileSetting.ColumnCollection.AddIfNew(newColumn);

              // Adjust or Set the common date format
              if (newColumn.ValueFormat.DataType == DataType.DateTime)
                othersValueFormatDate = CommonDateFormat(fileSetting.ColumnCollection);
            }
          }
        }

        processDisplay.CancellationToken.ThrowIfCancellationRequested();

        // The fileReader does not have the column information yet, let the reader know
        fileReader.OverrideColumnFormatFromSetting();

        // check all doubles if they could be integer needed for excel files as the typed values do
        // not distinguish between double and integer.
        if (checkDoubleToBeInteger)
          for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
          {
            processDisplay.CancellationToken.ThrowIfCancellationRequested();

            var oldColumn = fileReader.GetColumn(colIndex);
            var detect = !(fillGuessSettings.IgnoreIdColums &&
                           StringUtils.AssumeIDColumn(oldColumn.Name) > 0);

            if (oldColumn == null || oldColumn.ValueFormat.DataType != DataType.Double) continue;
            Column newColumn = null;

            if (detect)
            {
              SampleResult samples;
              if (sampleList.Keys.Contains(colIndex + 1))
                samples = sampleList[colIndex + 1];
              else
                samples = GetSampleValues(fileReader, fillGuessSettings.CheckedRecords,
                  colIndex, fillGuessSettings.SampleValues, fileSetting.TreatTextAsNull,
                  processDisplay.CancellationToken);

              if (samples.Values.Count > 0)
              {
                var checkResult = GuessNumeric(samples.Values, false, true, processDisplay.CancellationToken);
                if (checkResult != null && checkResult.FoundValueFormat.DataType != DataType.Double)
                {
                  newColumn = fileSetting.ColumnCollection.Get(oldColumn.Name) ??
                              fileSetting.ColumnCollection.AddIfNew(oldColumn);

                  newColumn.ValueFormat.DataType = checkResult.FoundValueFormat.DataType;
                }
              }
            }
            else
            {
              newColumn = fileSetting.ColumnCollection.Get(oldColumn.Name) ??
                          fileSetting.ColumnCollection.AddIfNew(oldColumn);
              newColumn.ValueFormat.DataType = DataType.String;
            }

            if (newColumn != null)
            {
              var msg = $"{newColumn.Name} – Overwritten Excel Format : {newColumn.GetTypeAndFormatDescription()}";
              processDisplay.SetProcess(msg, colIndex, true);
              result.Add(msg);
            }
          }

        if (fillGuessSettings.DateParts)
        {
          // Try to find a time for a date if the date does not already have a time Case a)
          // TimeFormat has already been recognized
          for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
          {
            processDisplay.CancellationToken.ThrowIfCancellationRequested();
            var columnDate = fileReader.GetColumn(colIndex);

            // Possibly add Time Zone
            if (columnDate.ValueFormat.DataType == DataType.DateTime && string.IsNullOrEmpty(columnDate.TimeZonePart))
              for (var colTimeZone = 0; colTimeZone < fileReader.FieldCount; colTimeZone++)
              {
                var columnTimeZone = fileReader.GetColumn(colTimeZone);
                var colName = columnTimeZone.Name.NoSpecials().ToUpperInvariant();
                if (columnTimeZone.ValueFormat.DataType != DataType.String && columnTimeZone.ValueFormat.DataType != DataType.Integer ||
                    colName != "TIMEZONE" && colName != "TIMEZONEID" && colName != "TIME ZONE" &&
                    colName != "TIME ZONE ID")
                  continue;

                columnDate.TimeZonePart = columnTimeZone.Name;
                result.Add($"{columnDate.Name} – Added Time Zone : {columnTimeZone.Name}");
              }

            if (columnDate.ValueFormat.DataType != DataType.DateTime || !string.IsNullOrEmpty(columnDate.TimePart) ||
                columnDate.ValueFormat.DateFormat.IndexOfAny(new[] { ':', 'h', 'H', 'm', 's', 't' }) != -1)
              continue;
            // We have a date column without time
            for (var colTime = 0; colTime < fileReader.FieldCount; colTime++)
            {
              var columnTime = fileReader.GetColumn(colTime);
              if (columnTime.ValueFormat.DataType != DataType.DateTime || !string.IsNullOrEmpty(columnDate.TimePart) ||
                  columnTime.ValueFormat.DateFormat.IndexOfAny(new[] { '/', 'y', 'M', 'd' }) != -1)
                continue;
              // We now have a time column, checked if the names somehow make sense
              if (!columnDate.Name.NoSpecials().ToUpperInvariant().Replace("DATE", string.Empty).Equals(
                columnTime.Name.NoSpecials().ToUpperInvariant().Replace("TIME", string.Empty),
                StringComparison.Ordinal))
                continue;

              columnDate.TimePart = columnTime.Name;
              columnDate.TimePartFormat = columnTime.ValueFormat.DateFormat;
              result.Add($"{columnDate.Name} – Added Time Part : {columnTime.Name}");
            }
          }

          // Case b) TimeFormat has not been recognized (e.G. all values are 08:00) only look in
          // adjacent fields
          for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
          {
            processDisplay.CancellationToken.ThrowIfCancellationRequested();
            var columnDate = fileReader.GetColumn(colIndex);
            if (columnDate.ValueFormat.DataType != DataType.DateTime || !string.IsNullOrEmpty(columnDate.TimePart) ||
                columnDate.ValueFormat.DateFormat.IndexOfAny(new[] { ':', 'h', 'H', 'm', 's', 't' }) != -1)
              continue;

            if (colIndex + 1 < fileReader.FieldCount)
            {
              var columnTime = fileReader.GetColumn(colIndex + 1);
              if (columnTime.ValueFormat.DataType == DataType.String && columnDate.Name.NoSpecials().ToUpperInvariant()
                    .Replace("DATE", string.Empty)
                    .Equals(columnTime.Name.NoSpecials().ToUpperInvariant().Replace("TIME", string.Empty),
                      StringComparison.OrdinalIgnoreCase))
              {
                columnDate.TimePart = columnTime.Name;
                {
                  var samples = sampleList.Keys.Contains(colIndex + 1)
                    ? sampleList[colIndex + 1]
                    : GetSampleValues(fileReader, 1, colIndex + 1, 1, fileSetting.TreatTextAsNull,
                      processDisplay.CancellationToken);

                  foreach (var first in samples.Values)
                  {
                    if (first.Length == 8 || first.Length == 5)
                    {
                      columnTime.ValueFormat.DataType = DataType.DateTime;
                      var val = new ValueFormat(DataType.DateTime)
                      {
                        DateFormat = first.Length == 8 ? "HH:mm:ss" : "HH:mm"
                      };
                      val.CopyTo(columnTime.ValueFormat);
                      fileSetting.ColumnCollection.AddIfNew(columnTime);
                      result.Add($"{columnTime.Name} – Format : {columnTime.GetTypeAndFormatDescription()}");
                    }

                    break;
                  }
                }

                result.Add($"{columnDate.Name} – Added Time Part : {columnTime.Name}");
                continue;
              }
            }

            if (colIndex <= 0)
              continue;
            {
              var columnTime = fileReader.GetColumn(colIndex - 1);
              if (columnTime.ValueFormat.DataType != DataType.String ||
                  !columnDate.Name.NoSpecials().ToUpperInvariant().Replace("DATE", string.Empty).Equals(
                    columnTime.Name.NoSpecials().ToUpperInvariant().Replace("TIME", string.Empty),
                    StringComparison.Ordinal))
                continue;

              columnDate.TimePart = columnTime.Name;
              {
                var samples = sampleList.Keys.Contains(colIndex - 1)
                  ? sampleList[colIndex - 1]
                  : GetSampleValues(fileReader, 1, colIndex - 1, 1, fileSetting.TreatTextAsNull,
                    processDisplay.CancellationToken);
                foreach (var first in samples.Values)
                {
                  if (first.Length == 8 || first.Length == 5)
                  {
                    var val = new ValueFormat(DataType.DateTime)
                    {
                      DateFormat = first.Length == 8 ? "HH:mm:ss" : "HH:mm"
                    };
                    fileSetting.ColumnCollection.AddIfNew(columnTime);
                    val.CopyTo(columnTime.ValueFormat);
                    result.Add($"{columnTime.Name} – Format : {columnTime.GetTypeAndFormatDescription()}");
                  }

                  break;
                }
              }
              result.Add($"{columnDate.Name} – Added Time Part : {columnTime.Name}");
            }
          }
        }

        var existing = new Collection<Column>();
        foreach (var colName in columnNamesInFile)
          foreach (var col in fileSetting.ColumnCollection)
          {
            if (!col.Name.Equals(colName, StringComparison.OrdinalIgnoreCase))
              continue;
            existing.Add(col);
            break;
          }

        // 2nd columns defined but not in list
        foreach (var col in fileSetting.ColumnCollection)
          if (!existing.Contains(col))
            existing.Add(col);

        fileSetting.ColumnCollection.Clear();
        if (existing == null) return result;
        foreach (var column in existing)
          fileSetting.ColumnCollection.AddIfNew(column);
      }

      return result;
    }

    /// <summary>
    ///   Fills the Column Format for writer fileSettings
    /// </summary>
    /// <param name="fileSettings">The file settings.</param>
    /// <param name="all">if set to <c>true</c> event string columns are added.</param>
    /// <param name="processDisplay">The process display.</param>
    /// <exception cref="FileWriterException">No SQL Statement given or No SQL Reader set</exception>
    public static void FillGuessColumnFormatWriter(this IFileSetting fileSettings, bool all,
      IProcessDisplay processDisplay)
    {
      if (string.IsNullOrEmpty(fileSettings.SqlStatement))
        throw new FileWriterException("No SQL Statement given");
      if (FunctionalDI.SQLDataReader == null)
        throw new FileWriterException("No SQL Reader set");

      using (var dataReader = FunctionalDI.SQLDataReader(fileSettings.SqlStatement, processDisplay, fileSettings.Timeout))
      {
        // Put the information into the list
        var dataRowCollection = dataReader.GetSchemaTable()?.Rows;
        if (dataRowCollection == null) return;
        foreach (DataRow schemaRow in dataRowCollection)
        {
          var header = schemaRow[SchemaTableColumn.ColumnName].ToString();
          var colType = ((Type)schemaRow[SchemaTableColumn.DataType]).GetDataType();

          if (!all && colType == DataType.String)
            continue;

          fileSettings.ColumnCollection.AddIfNew(new Column(header, colType));
        }
      }
    }

    /// <summary>
    ///   Gets all possible formats based on the provided value
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="culture">The culture.</param>
    /// <returns></returns>
    public static IEnumerable<ValueFormat> GetAllPossibleFormats(string value, CultureInfo culture = null)
    {
      if (culture == null)
        culture = CultureInfo.CurrentCulture;

      // Standard Date Time formats
      foreach (var fmt in StringConversion.StandardDateTimeFormats.MatchingForLength(value.Length, true))
        foreach (var sep in StringConversion.DateSeparators)
          if (StringConversion.StringToDateTimeExact(value, fmt, sep, culture.DateTimeFormat.TimeSeparator, culture)
            .HasValue)
            yield return new ValueFormat(DataType.DateTime) { DateFormat = fmt, DateSeparator = sep };
    }


    /// <summary>
    ///   Get sample values for several columns at once, ignoring rows with issues or warning in the
    ///   columns, looping though all records in the reader
    /// </summary>
    /// <param name="fileReader">A <see cref="IFileReader" /> data reader</param>
    /// <param name="maxRecords">The maximum records.</param>
    /// <param name="columns">
    ///   A Dictionary listing the columns and the number of samples needed for each
    /// </param>
    /// <param name="enoughSamples">The enough samples.</param>
    /// <param name="treatAsNull">Text that should be regarded as an empty column</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">dataReader</exception>
    /// <exception cref="ArgumentOutOfRangeException">no valid columns provided</exception>
    private static async Task<IDictionary<int, SampleResult>> GetSampleValuesAsync(IFileReader fileReader, long maxRecords,
      IEnumerable<int> columns, int enoughSamples, string treatAsNull, CancellationToken cancellationToken)
    {
      var samples = GetSampleValuesValidate(fileReader, ref maxRecords, columns, ref treatAsNull);

      var hasWarning = false;
      var remainingShows = 10;
      void WarningEvent(object sender, WarningEventArgs args)
      {
        if (args.ColumnNumber != -1 && !samples.ContainsKey(args.ColumnNumber))
          return;
        if (remainingShows-- > 0)
          Logger.Debug("Row ignored in detection: " + args.Message);
        if (remainingShows == 0)
          Logger.Debug("No further warning shown");

        hasWarning = true;
      }

      fileReader.Warning += WarningEvent;

      var recordRead = 0;
      try
      {
        // could already be at EOF need to reset
        if (fileReader.EndOfFile && fileReader.SupportsReset)
        {
          Logger.Debug("Resetting read position to the beginning");
          fileReader.ResetPositionToFirstDataRow();
        }

        // Ready to start store the record number we are currently at, we could be in the middle of
        // the file already
        var startRecordNumber = fileReader.RecordNumber;

        var maxSamples = 2000;
        if (maxSamples < enoughSamples)
          maxSamples = enoughSamples;

        var enough = new List<int>();
        // Get distinct sample values until we have
        // * parsed the maximum number
        // * have enough samples to be satisfied
        // * we are at the beginning record again
        while (++recordRead < maxRecords && !cancellationToken.IsCancellationRequested &&
               samples.Keys.Count > enough.Count)
        {
          // if at the end start from the beginning
          if (!await fileReader.ReadAsync() && fileReader.EndOfFile)
          {
            if (!fileReader.SupportsReset)
              break;
            fileReader.ResetPositionToFirstDataRow();
            // If still at the end, we do not have a line
            if (startRecordNumber == 0 || !await fileReader.ReadAsync())
              break;
          }

          // In case there was a warning reading the line, ignore the line
          if (!hasWarning)
            GetSampleValuesProcessRecord(fileReader, enoughSamples, treatAsNull, samples, maxSamples, enough);
          else
            hasWarning = false;

          // Once we arrive the starting row we have read all records
          if (fileReader.RecordNumber == startRecordNumber)
            break;
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.InnerExceptionMessages());
      }
      finally
      {
        fileReader.Warning -= WarningEvent;
      }

      return samples.ToDictionary(keyValue => keyValue.Key, keyValue => new SampleResult(keyValue.Value, recordRead));
    }

    /// <summary>
    ///   Get sample values for several columns at once, ignoring rows with issues or warning in the
    ///   columns, looping though all records in the reader
    /// </summary>
    /// <param name="fileReader">A <see cref="IFileReader" /> data reader</param>
    /// <param name="maxRecords">The maximum records.</param>
    /// <param name="columns">
    ///   A Dictionary listing the columns and the number of samples needed for each
    /// </param>
    /// <param name="enoughSamples">The enough samples.</param>
    /// <param name="treatAsNull">Text that should be regarded as an empty column</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">dataReader</exception>
    /// <exception cref="ArgumentOutOfRangeException">no valid columns provided</exception>
    private static IDictionary<int, SampleResult> GetSampleValues(IFileReader fileReader, long maxRecords,
     IEnumerable<int> columns, int enoughSamples, string treatAsNull, CancellationToken cancellationToken)
    {
      var samples = GetSampleValuesValidate(fileReader, ref maxRecords, columns, ref treatAsNull);

      var hasWarning = false;
      var remainingShows = 10;
      void WarningEvent(object sender, WarningEventArgs args)
      {
        if (args.ColumnNumber != -1 && !samples.ContainsKey(args.ColumnNumber))
          return;
        if (remainingShows-- > 0)
          Logger.Debug("Row ignored in detection: " + args.Message);
        if (remainingShows == 0)
          Logger.Debug("No further warning shown");

        hasWarning = true;
      }

      fileReader.Warning += WarningEvent;
      var recordRead = 0;
      try
      {
        // could already be at EOF need to reset
        if (fileReader.EndOfFile && fileReader.SupportsReset)
        {
          Logger.Debug("Resetting read position to the beginning");
          fileReader.ResetPositionToFirstDataRow();
        }

        // Ready to start store the record number we are currently at, we could be in the middle of
        // the file already
        var startRecordNumber = fileReader.RecordNumber;

        var maxSamples = 2000;
        if (maxSamples < enoughSamples)
          maxSamples = enoughSamples;

        var enough = new List<int>();
        // Get distinct sample values until we have
        // * parsed the maximum number
        // * have enough samples to be satisfied
        // * we are at the beginning record again
        while (++recordRead < maxRecords && !cancellationToken.IsCancellationRequested &&
               samples.Keys.Count > enough.Count)
        {
          // if at the end start from the beginning
          if (!fileReader.Read() && fileReader.EndOfFile)
          {
            if (!fileReader.SupportsReset)
              break;
            fileReader.ResetPositionToFirstDataRow();
            // If still at the end, we do not have a line
            if (startRecordNumber == 0 || !fileReader.Read())
              break;
          }

          if (!hasWarning)
            GetSampleValuesProcessRecord(fileReader, enoughSamples, treatAsNull, samples, maxSamples, enough);
          else
            hasWarning = false;

          // Once we arrive the starting row we have read all records
          if (fileReader.RecordNumber == startRecordNumber)
            break;
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.InnerExceptionMessages());
      }
      finally
      {
        fileReader.Warning -= WarningEvent;
      }

      return samples.ToDictionary(keyValue => keyValue.Key, keyValue => new SampleResult(keyValue.Value, recordRead));
    }

    private static void GetSampleValuesProcessRecord(IDataRecord fileReader, int enoughSamples, string treatAsNull,
      Dictionary<int, ICollection<string>> samples, int maxSamples, ICollection<int> enough)
    {
      foreach (var columnIndex in samples.Keys.Where(x => !enough.Contains(x)))
      {
        var value = fileReader.GetString(columnIndex);

        // Any non existing value is not of interest
        if (string.IsNullOrWhiteSpace(value))
          continue;

        // Always trim
        value = value.Trim();

        // Always do treat Text "Null" as Null
        if (StringUtils.ShouldBeTreatedAsNull(value, treatAsNull))
          continue;

        // cut of after 40 chars
        if (value.Length > 40)
          value = value.Substring(0, 40);

        // Have a max of 2000 values
        if (samples[columnIndex].Count < maxSamples)
          samples[columnIndex].Add(value);

        if (samples[columnIndex].Count >= enoughSamples)
          enough.Add(columnIndex);
      }
    }

    private static Dictionary<int, ICollection<string>> GetSampleValuesValidate(IFileReader fileReader, ref long maxRecords, IEnumerable<int> columns,
      ref string treatAsNull)
    {
      if (fileReader == null)
        throw new ArgumentNullException(nameof(fileReader));

      if (string.IsNullOrEmpty(treatAsNull))
        treatAsNull = "NULL;n/a";

      if (maxRecords < 1)
        maxRecords = long.MaxValue;

      if (fileReader.IsClosed)
        fileReader.Open();

      var samples = columns.Where(col => col > -1 && col < fileReader.FieldCount)
        .ToDictionary<int, int, ICollection<string>>(col => col,
          col => new HashSet<string>(StringComparer.OrdinalIgnoreCase));

      if (samples.Keys.Count == 0)
        throw new ArgumentOutOfRangeException(nameof(columns), "Column Collection can not be empty");
      return samples;
    }

    /// <summary>
    ///   Get sample values for a column, ignoring all rows with issues for the column, looping
    ///   though all records in the reader
    /// </summary>
    /// <param name="fileReader">A <see cref="IFileReader" /> data reader</param>
    /// <param name="maxRecords">The maximum records.</param>
    /// <param name="columnIndex">Index of the column.</param>
    /// <param name="enoughSamples">The number samples.</param>
    /// <param name="treatAsNull">Text that should be regarded as an empty column</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>
    ///   A collection of distinct not null values, in case the text is long only the first 40
    ///   characters are stored
    /// </returns>
    public static async Task<SampleResult> GetSampleValuesAsync(IFileReader fileReader, long maxRecords,
      int columnIndex, int enoughSamples, string treatAsNull, CancellationToken cancellationToken) => (await GetSampleValuesAsync(fileReader, maxRecords, new[] { columnIndex }, enoughSamples, treatAsNull, cancellationToken))[columnIndex];
    public static SampleResult GetSampleValues(IFileReader fileReader, long maxRecords,
      int columnIndex, int enoughSamples, string treatAsNull, CancellationToken cancellationToken) => GetSampleValues(fileReader, maxRecords, new[] { columnIndex }, enoughSamples, treatAsNull, cancellationToken)[columnIndex];

    /// <summary>
    ///   Gets the writer source columns.
    /// </summary>
    /// <param name="fileSettings">The file settings.</param>
    /// <param name="processDisplay">The process display.</param>
    /// <returns></returns>
    public static IEnumerable<ColumnInfo> GetSourceColumnInformation(IFileSetting fileSettings,
    IProcessDisplay processDisplay)
    {
      Contract.Requires(fileSettings != null);

      if (string.IsNullOrEmpty(fileSettings.SqlStatement))
        return new List<ColumnInfo>();
      if (FunctionalDI.SQLDataReader == null)
        throw new FileWriterException("No SQL Reader set");
      using (var data = FunctionalDI.SQLDataReader(fileSettings.SqlStatement.NoRecordSQL(), processDisplay, fileSettings.Timeout))
      {
        return ColumnInfo.GetSourceColumnInformation(fileSettings, data);
      }
    }

    /// <summary>
    ///   Guesses the date time format
    /// </summary>
    /// <param name="samples">The sample texts.</param>
    /// <param name="checkNamedDates">if set to <c>true</c> [check named dates].</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">samples</exception>
    public static CheckResult GuessDateTime(ICollection<string> samples, bool checkNamedDates,
      CancellationToken cancellationToken)
    {
      if (samples == null || samples.Count == 0)
        throw new ArgumentNullException(nameof(samples));

      var checkResult = new CheckResult();

      long length = 0;
      foreach (var sample in samples)
        length += sample.Length;
      var commonLength = (int)(length / samples.Count);

      ICollection<string> possibleDateSeparators = null;
      foreach (var fmt in StringConversion.StandardDateTimeFormats.MatchingForLength(commonLength, checkNamedDates))
      {
        if (cancellationToken.IsCancellationRequested)
          return null;

        if (fmt.IndexOf('/') > 0)
        {
          // if we do not have determined the list of possibleDateSeparators so far do so now, but
          // only once
          if (possibleDateSeparators == null)
          {
            possibleDateSeparators = new List<string>();
            foreach (var sep in StringConversion.DateSeparators)
              foreach (var entry in samples)
              {
                cancellationToken.ThrowIfCancellationRequested();
                if (entry.IndexOf(sep, StringComparison.Ordinal) == -1) continue;
                possibleDateSeparators.Add(sep);
                break;
              }
          }

          foreach (var sep in possibleDateSeparators)
          {
            var res = StringConversion.CheckDate(samples, fmt, sep, ":", CultureInfo.CurrentCulture);
            if (res.FoundValueFormat != null)
              return res;

            checkResult.KeepBestPossibleMatch(res);
          }
        }
        else
        {
          var res = StringConversion.CheckDate(samples, fmt,
            CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, ":", CultureInfo.CurrentCulture);
          if (res.FoundValueFormat != null)
            return res;

          checkResult.KeepBestPossibleMatch(res);
        }
      }

      return checkResult;
    }

    public static CheckResult GuessNumeric(ICollection<string> samples, bool guessPercentage,
      bool allowStartingZero, CancellationToken cancellationToken)
    {
      var checkResult = new CheckResult();
      if (samples == null)
        return checkResult;

      var possibleGrouping = new List<char>();
      // Determine which decimalGrouping could be used
      foreach (var character in StringConversion.DecimalGroupings)
      {
        if (character == '\0')
          continue;
        foreach (var smp in samples)
        {
          if (smp.IndexOf(character) <= -1)
            continue;
          possibleGrouping.Add(character);
          break;
        }
      }

      possibleGrouping.Add('\0');
      var possibleDecimal = new List<char>();
      foreach (var character in StringConversion.DecimalSeparators)
      {
        if (character == '\0')
          continue;
        foreach (var smp in samples)
        {
          if (smp.IndexOf(character) <= -1)
            continue;
          possibleDecimal.Add(character);
          break;
        }
      }

      // Need to have at least one decimal separator
      if (possibleDecimal.Count == 0)
        possibleDecimal.Add('.');

      foreach (var thousandSeparator in possibleGrouping)
        // Try Numbers: Int and Decimal
        foreach (var decimalSeparator in possibleDecimal)
        {
          if (cancellationToken.IsCancellationRequested)
            return null;
          if (decimalSeparator.Equals(thousandSeparator))
            continue;
          var res = StringConversion.CheckNumber(samples, decimalSeparator, thousandSeparator, guessPercentage,
            allowStartingZero);
          if (res.FoundValueFormat != null)
            return res;

          checkResult.KeepBestPossibleMatch(res);
        }

      return checkResult;
    }

    /// <summary>
    ///   Guesses the value format.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <param name="samples">The samples.</param>
    /// <param name="minRequiredSamples">The minimum required samples.</param>
    /// <param name="trueValue">The text to be regarded as <c>true</c></param>
    /// <param name="falseValue">The text to be regarded as <c>false</c></param>
    /// <param name="guessBoolean">Try to identify a boolean</param>
    /// <param name="guessGuid">Try to determine if its a GUID</param>
    /// <param name="guessNumeric">Try to determine if its a Number</param>
    /// <param name="guessDateTime">Try to determine if it is a date time</param>
    /// <param name="guessPercentage">Accept percentage values</param>
    /// <param name="serialDateTime">Allow serial Date time</param>
    /// <param name="checkNamedDates">if set to <c>true</c> [check named dates].</param>
    /// <param name="othersValueFormatDate">
    ///   The date format found in prior columns, assuming the data format is the same in other
    ///   columns, we do not need that many samples
    /// </param>
    /// <returns>
    ///   <c>Null</c> if no format could be determined otherwise a <see cref="ValueFormat" />
    /// </returns>
    public static CheckResult GuessValueFormat(ICollection<string> samples, int minRequiredSamples,
      string trueValue, string falseValue, bool guessBoolean, bool guessGuid, bool guessNumeric, bool guessDateTime,
      bool guessPercentage, bool serialDateTime, bool checkNamedDates, ValueFormat othersValueFormatDate,
      CancellationToken cancellationToken)
    {
      Contract.Requires(samples != null);

      var count = samples.Count();
      if (count == 0)
        return null;

      var checkResult = new CheckResult { FoundValueFormat = new ValueFormat() };

      // ---------------- Boolean --------------------------
      if (guessBoolean && count <= 2)
      {
        var allParsed = true;
        string usedTrueValue = null;
        string usedFalseValue = null;
        foreach (var value in samples)
        {
          var result = StringConversion.StringToBooleanStrict(value, trueValue, falseValue);
          if (result == null)
          {
            allParsed = false;
            break;
          }

          if (result.Item1)
            usedTrueValue = result.Item2;
          else
            usedFalseValue = result.Item2;
        }

        if (allParsed)
        {
          checkResult.FoundValueFormat.DataType = DataType.Boolean;
          if (!string.IsNullOrEmpty(usedTrueValue))
            checkResult.FoundValueFormat.True = usedTrueValue;
          if (!string.IsNullOrEmpty(usedFalseValue))
            checkResult.FoundValueFormat.False = usedFalseValue;
          return checkResult;
        }
      }

      if (cancellationToken.IsCancellationRequested)
        return null;

      // ---------------- GUID --------------------------
      if (guessGuid && StringConversion.CheckGuid(samples))
      {
        checkResult.FoundValueFormat.DataType = DataType.Guid;
        return checkResult;
      }

      if (cancellationToken.IsCancellationRequested)
        return null;

      // ---------------- Text -------------------------- in case we have named dates, this is not feasible
      if (!checkNamedDates)
      {
        // Trying some chars, if they are in, assume its a string
        var valuesWithChars = 0;
        foreach (var value in samples)
        {
          // Not having AM PM or T as it might be part of a date Not having E in there as might be
          // part of a number u 1.487% o 6.264% n 2.365% i 6.286% h 7.232% s 6.327% This adds to a
          // 30% chance for each position in the text to determine if a text a regular text,
          if (value.IndexOfAny(new[] { 'u', 'U', 'o', 'O', 'i', 'I', 'n', 'N', 's', 'S', 'h', 'H' }) <= -1)
            continue;
          valuesWithChars++;
          // Only do so if more then half of the samples are string
          if (valuesWithChars < count / 2 && valuesWithChars < 10)
            continue;
          checkResult.FoundValueFormat.DataType = DataType.String;
          return checkResult;
        }
      }

      // ---------------- Confirm old provided format would be ok --------------------------
      var firstValue = string.Empty;
      foreach (var value in samples)
      {
        firstValue = value;
        break;
      }

      if (guessDateTime && othersValueFormatDate != null &&
          StringConversion.DateLengthMatches(firstValue.Length, othersValueFormatDate.DateFormat))
      {
        var res = StringConversion.CheckDate(samples, othersValueFormatDate.DateFormat,
          othersValueFormatDate.DateSeparator, othersValueFormatDate.TimeSeparator, CultureInfo.CurrentCulture);
        if (res.FoundValueFormat != null)
          return res;
        checkResult.KeepBestPossibleMatch(res);
      }

      if (cancellationToken.IsCancellationRequested)
        return null;

      // if we have less than the required samples values do not try and try to get a type
      if (count >= minRequiredSamples)
      {
        // Guess a date format that could be interpreted as number before testing numbers
        if (guessDateTime && firstValue.Length == 8)
        {
          var res = StringConversion.CheckDate(samples, "yyyyMMdd", string.Empty, ":", CultureInfo.InvariantCulture);
          if (res.FoundValueFormat != null)
            return res;
          checkResult.KeepBestPossibleMatch(res);
        }

        if (cancellationToken.IsCancellationRequested)
          return null;

        // We need to have at least 10 sample values here its too dangerous to assume it is a date
        if (guessDateTime && serialDateTime && count > 10 && count > minRequiredSamples)
        {
          var res = StringConversion.CheckSerialDate(samples, true);
          if (res.FoundValueFormat != null)
            return res;
          checkResult.KeepBestPossibleMatch(res);
        }

        if (cancellationToken.IsCancellationRequested)
          return null;

        // ---------------- Decimal / Integer --------------------------
        if (guessNumeric)
        {
          var res = GuessNumeric(samples, guessPercentage, false, cancellationToken);
          if (res.FoundValueFormat != null)
            return res;
          checkResult.KeepBestPossibleMatch(res);
        }

        if (cancellationToken.IsCancellationRequested)
          return null;

        // ---------------- Date -------------------------- Minimum length of a date is 4 characters
        if (guessDateTime && firstValue.Length > 3)
        {
          var res = GuessDateTime(samples, checkNamedDates, cancellationToken);
          if (res.FoundValueFormat != null)
            return res;
          checkResult.KeepBestPossibleMatch(res);
        }

        if (cancellationToken.IsCancellationRequested)
          return null;
      }

      // Assume dates are of the same format across the files we check if the dates we have would
      // possibly match no matter how many samples we have this time we do not care about matching
      // length Check Date will cut off time information , this is independent from minRequiredSamples
      if (guessDateTime && othersValueFormatDate != null)
      {
        var res = StringConversion.CheckDate(samples, othersValueFormatDate.DateFormat,
          othersValueFormatDate.DateSeparator, othersValueFormatDate.TimeSeparator, CultureInfo.CurrentCulture);
        if (res.FoundValueFormat != null)
          return res;
        checkResult.KeepBestPossibleMatch(res);
      }

      if (cancellationToken.IsCancellationRequested)
        return null;

      // if we have dates and allow serial dates, but do not guess numeric (this would be a fit) try
      // if the dates are all serial
      if (!guessDateTime || !serialDateTime || guessNumeric)
        return checkResult;

      if (count >= minRequiredSamples)
      {
        var res = StringConversion.CheckSerialDate(samples, false);
        if (res.FoundValueFormat != null)
          return res;
        checkResult.KeepBestPossibleMatch(res);
      }

      return checkResult;
    }

    [DebuggerDisplay("SampleResult: {Values.Count} of {RecordsRead}")]
    public class SampleResult
    {
      public SampleResult(IEnumerable<string> samples, int records)
      {
        var source = new List<string>(samples);
        Values = new HashSet<string>();
        while (source.Count > 0)
        {
          var index = new Random(Guid.NewGuid().GetHashCode()).Next(0, source.Count); //pick a random item from the master list
          Values.Add(source[index]); //place it at the end of the randomized list
          source.RemoveAt(index);
        }

        RecordsRead = records;
      }

      public int RecordsRead { get; }
      public ICollection<string> Values { get; }
    }
  }
}