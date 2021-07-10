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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if !QUICK
using System.Data;
using System.Data.Common;
#endif

namespace CsvTools
{
  /// <summary>
  ///   Helper class
  /// </summary>
  public static class DetermineColumnFormat
  {
    public static IValueFormat? CommonDateFormat(IEnumerable<IColumn>? columns)
    {
      IValueFormat? best = null;
      if (columns == null) return null;
      var counterByFormat = new Dictionary<IValueFormat, int>();
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

#if !QUICK
    /// <summary>
    ///   Fills the ColumnCollection for reader fileSettings
    /// </summary>
    /// <param name="fileSetting">The file setting to check, and columns to be added</param>
    /// <param name="addTextColumns">if set to <c>true</c> event string columns are added.</param>
    /// <param name="checkDoubleToBeInteger">if set to <c>true</c> [check double to be integer].</param>
    /// <param name="fillGuessSettings">The fill guess settings.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A list of columns with new format that have been changed</returns>
    /// <exception cref="ArgumentNullException">processDisplay</exception>
    public static async Task<(IList<string>, IEnumerable<IColumn>)> FillGuessColumnFormatReaderAsync(this IFileSetting fileSetting,
      bool addTextColumns,
      bool checkDoubleToBeInteger, FillGuessSettings fillGuessSettings,
      CancellationToken cancellationToken)
    {
      if (fileSetting == null)
        throw new ArgumentNullException(nameof(fileSetting));
      if (fillGuessSettings == null)
        throw new ArgumentNullException(nameof(fillGuessSettings));

      // Check if we are supposed to check something
      if (!fillGuessSettings.Enabled || !fillGuessSettings.DetectNumbers && !fillGuessSettings.DetectBoolean &&
        !fillGuessSettings.DetectDateTime && !fillGuessSettings.DetectGUID &&
        !fillGuessSettings.DetectPercentage &&
        !fillGuessSettings.SerialDateTime)
        return (new List<string>(), fileSetting.ColumnCollection);

      // in case there is no delimiter but its a delimited file, do nothing
      if (fileSetting.FileFormat.FieldDelimiterChar == '\0' && fileSetting is ICsvFile)
        return (new List<string>(), fileSetting.ColumnCollection);
      // Open the file setting but change a few settings
      var fileSettingCopy = GetSettingForRead(fileSetting);

      // need a dummy process display to have pass in Cancellation token to reader
      using var prc2 = new CustomProcessDisplay(cancellationToken);
      using var fileReader = FunctionalDI.GetFileReader(fileSettingCopy, string.Empty, prc2);
      await fileReader.OpenAsync(prc2.CancellationToken).ConfigureAwait(false);
      return await FillGuessColumnFormatReaderAsyncReader(fileReader, fillGuessSettings,
        fileSetting.ColumnCollection,
        addTextColumns, checkDoubleToBeInteger, fileSetting.TreatTextAsNull, prc2.CancellationToken);
    }

    public static IFileSetting GetSettingForRead(this IFileSetting fileSetting)
    {
      if (fileSetting == null)
        throw new ArgumentNullException(nameof(fileSetting));

      // Open the file setting but change a few settings
      var fileSettingCopy = fileSetting.Clone();

      // Make sure that if we do have a CSV file without header that we will skip the first row that
      // might contain headers, but its simply set as without headers.
      if (!(fileSettingCopy is CsvFile csv)) return fileSettingCopy;
      if (!csv.HasFieldHeader && csv.SkipRows == 0)
        csv.SkipRows = 1;
      // turn off all warnings as they will cause GetSampleValues to ignore the row
      csv.TryToSolveMoreColumns = false;
      csv.AllowRowCombining = false;
      csv.WarnDelimiterInValue = false;
      csv.WarnLineFeed = false;
      csv.WarnQuotes = false;
      csv.WarnUnknownCharacter = false;
      csv.WarnNBSP = false;
      csv.WarnQuotesInQuotes = false;

      return fileSettingCopy;
    }
#endif

    /// <summary>
    ///   Goes through the open reader and add found fields to the column collection
    /// </summary>
    /// <param name="fileReader">An ready to read file reader</param>
    /// <param name="fillGuessSettings">The fill guess settings.</param>
    /// <param name="columnCollectionInput">Already defined columns</param>
    /// <param name="addTextColumns">
    ///   true if text columns should be added, usually any not defined format is a regarded as text
    /// </param>
    /// <param name="checkDoubleToBeInteger">
    ///   true to make sure a numeric value gets the lowest possible numeric type
    /// </param>
    /// <param name="treatTextAsNull">A text that should be regarded as empty</param>
    /// <param name="cancellationToken">Cancellation support</param>
    /// <returns>A text with the changes that have been made</returns>
    public static async Task<(IList<string>, IEnumerable<IColumn>)> FillGuessColumnFormatReaderAsyncReader(
      this IFileReader fileReader,
      FillGuessSettings fillGuessSettings, IEnumerable<IColumn>? columnCollectionInput,
      bool addTextColumns,
      bool checkDoubleToBeInteger,
      string treatTextAsNull,
      CancellationToken cancellationToken)
    {
      if (fileReader == null)
        throw new ArgumentNullException(nameof(fileReader));
      if (fillGuessSettings == null)
        throw new ArgumentNullException(nameof(fillGuessSettings));

      columnCollectionInput ??= Array.Empty<IColumn>();

      if (!fillGuessSettings.Enabled || !fillGuessSettings.DetectNumbers && !fillGuessSettings.DetectBoolean &&
        !fillGuessSettings.DetectDateTime && !fillGuessSettings.DetectGUID &&
        !fillGuessSettings.DetectPercentage &&
        !fillGuessSettings.SerialDateTime)
        return (Array.Empty<string>(), columnCollectionInput);

      if (fileReader.FieldCount == 0)
        return (Array.Empty<string>(), columnCollectionInput);
      
      if (fileReader.EndOfFile)
        fileReader.ResetPositionToFirstDataRow();

      if (fileReader.EndOfFile) // still end of file
        return (Array.Empty<string>(), columnCollectionInput);

      var result = new List<string>();

      var columnCollection = new ColumnCollection();
      // Only use column definition for columns that do not exist
      foreach (var col in columnCollectionInput)
      {
        for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
        {
          if (!col.Name.Equals(fileReader.GetName(colIndex), StringComparison.OrdinalIgnoreCase)) continue;
          columnCollection.Add(col);
          break;
        }
      }
      var othersValueFormatDate = CommonDateFormat(columnCollection);

      // build a list of columns to check
      var getSamples = new List<int>();
      var columnNamesInFile = new List<string>();
      for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
      {
        var readerColumn = fileReader.GetColumn(colIndex);

        columnNamesInFile.Add(readerColumn.Name);

        if (fillGuessSettings.IgnoreIdColumns && StringUtils.AssumeIDColumn(readerColumn.Name) > 0)
        {
          Logger.Information("{column} – ID columns ignored", readerColumn.Name);
          result.Add($"{readerColumn.Name} – ID columns ignored");

          if (!addTextColumns || columnCollection.Get(readerColumn.Name) != null) continue;
          columnCollection.Add(readerColumn);
          continue;
        }

        // no need to get types that are already found and could now be smaller (e.G. decimal could
        // be a integer)
        if (readerColumn.ValueFormat.DataType == DataType.Guid ||
            readerColumn.ValueFormat.DataType == DataType.Integer ||
            readerColumn.ValueFormat.DataType == DataType.Boolean ||
            readerColumn.ValueFormat.DataType == DataType.DateTime ||
            (readerColumn.ValueFormat.DataType == DataType.Numeric && !checkDoubleToBeInteger) ||
            (readerColumn.ValueFormat.DataType == DataType.Double && !checkDoubleToBeInteger))
        {
          Logger.Information("{column} - Existing Type : {format}", readerColumn.Name,
            readerColumn.ValueFormat.GetTypeAndFormatDescription());
          result.Add($"{readerColumn.Name} - Existing Type : {readerColumn.ValueFormat.GetTypeAndFormatDescription()}");
          continue;
        }

        getSamples.Add(colIndex);
      }

      var sampleList = await GetSampleValuesAsync(fileReader, fillGuessSettings.CheckedRecords,
        getSamples, fillGuessSettings.SampleValues, treatTextAsNull,
        cancellationToken).ConfigureAwait(false);

      // Add all columns that will not be guessed
      for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
      {
        if (sampleList.ContainsKey(colIndex)) continue;
        var readerColumn = fileReader.GetColumn(colIndex);
        columnCollection.Add(readerColumn);
      }

      // Start Guessing
      foreach (var colIndex in sampleList.Keys)
      {
        cancellationToken.ThrowIfCancellationRequested();

        var readerColumn = fileReader.GetColumn(colIndex);
        var samples = sampleList[colIndex];

        cancellationToken.ThrowIfCancellationRequested();
        if (samples.Values.Count == 0)
        {
          Logger.Information("{column} – No values found – Format : {format}", readerColumn.Name,
            readerColumn.GetTypeAndFormatDescription());
          result.Add($"{readerColumn.Name} – No values found – Format : {readerColumn.GetTypeAndFormatDescription()}");
          if (!addTextColumns)
            continue;
          columnCollection.Add(readerColumn);
        }
        else
        {
          var detect = true;
          if (samples.Values.Count() < fillGuessSettings.MinSamples)
          {
            Logger.Information(
              "{column} – {values} values found in {records} rows. Not enough sample values – Format : {format}",
              readerColumn.Name, samples.Values.Count(), samples.RecordsRead,
              readerColumn.GetTypeAndFormatDescription());
            detect = false;
          }
          else
          {
            Logger.Information("{column} – {values} values found in {records} row. Examining format", readerColumn.Name,
              samples.Values.Count(), samples.RecordsRead);
          }

          var checkResult = GuessValueFormat(samples.Values, fillGuessSettings.MinSamples,
            fillGuessSettings.TrueValue,
            fillGuessSettings.FalseValue,
            fillGuessSettings.DetectBoolean,
            fillGuessSettings.DetectGUID && detect,
            fillGuessSettings.DetectNumbers && detect,
            fillGuessSettings.DetectDateTime && detect,
            fillGuessSettings.DetectPercentage && detect,
            fillGuessSettings.SerialDateTime && detect,
            fillGuessSettings.CheckNamedDates && detect,
            othersValueFormatDate,
            cancellationToken);

          // if nothing is found take what was configured before, as the reader could possibly
          // provide typed data (Json, Excel...)
          checkResult.FoundValueFormat ??= readerColumn.ValueFormat;
          var colIndexCurrent = columnCollection.GetIndex(readerColumn.Name);
          // if we have a mapping to a template that expects a integer and we only have integers but
          // not enough
          if (colIndexCurrent != -1)
          {
            if (checkResult.FoundValueFormat.DataType == DataType.DateTime)
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
                if (!othersValueFormatDate.ValueFormatEqual(checkResult.FoundValueFormat))
                  othersValueFormatDate = null;
              }
            }

            var oldValueFormat = columnCollection[colIndexCurrent].GetTypeAndFormatDescription();

            if (checkResult.FoundValueFormat.ValueFormatEqual(columnCollection[colIndexCurrent].ValueFormat))
            {
              Logger.Information("{column} – Format : {format} – not changed", readerColumn.Name, oldValueFormat);
            }
            else
            {
              var newValueFormat = checkResult.FoundValueFormat.GetTypeAndFormatDescription();
              if (oldValueFormat.Equals(newValueFormat, StringComparison.Ordinal))
                continue;
              Logger.Information("{column} – Format : {format} – updated from {oldformat}", readerColumn.Name,
                newValueFormat, oldValueFormat);
              result.Add($"{readerColumn.Name} – Format : {newValueFormat} – updated from {oldValueFormat}");
              columnCollection.Replace(new ImmutableColumn(columnCollection[colIndexCurrent], checkResult.FoundValueFormat));
            }
          }
          // new Column
          else
          {
            var format = (checkResult.PossibleMatch ? checkResult.ValueFormatPossibleMatch : checkResult.FoundValueFormat) ??
                         new ImmutableValueFormat();

            if (!addTextColumns && format.DataType == DataType.String) continue;

            Logger.Information("{column} – Format : {format}", readerColumn.Name, format.GetTypeAndFormatDescription());
            result.Add($"{readerColumn.Name} – Format : {format.GetTypeAndFormatDescription()}");
            columnCollection.Add(new ImmutableColumn(readerColumn, format));

            // Adjust or Set the common date format
            if (format.DataType == DataType.DateTime)
              othersValueFormatDate = CommonDateFormat(columnCollection);
          }
        }
      }

      cancellationToken.ThrowIfCancellationRequested();

      // check all doubles if they could be integer needed for excel files as the typed values do
      // not distinguish between double and integer.
      if (checkDoubleToBeInteger)
        for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
        {
          cancellationToken.ThrowIfCancellationRequested();
          var readerColumn = fileReader.GetColumn(colIndex);

          if (readerColumn.ValueFormat.DataType != DataType.Double &&
              readerColumn.ValueFormat.DataType != DataType.Numeric) continue;
          if (sampleList.Keys.Contains(colIndex + 1))
          {
            var samples = sampleList[colIndex + 1];

            if (samples.Values.Count <= 0) continue;
            var checkResult = GuessNumeric(samples.Values, false, true, fillGuessSettings.MinSamples, cancellationToken);
            if (checkResult.FoundValueFormat != null && checkResult.FoundValueFormat.DataType != DataType.Double)
            {
              var colIndexExisting = columnCollection.GetIndex(readerColumn.Name);

              if (colIndexExisting != -1)
              {
                var oldVf = columnCollection[colIndexExisting].ValueFormat;
                if (oldVf.ValueFormatEqual(checkResult.FoundValueFormat)) continue;
                Logger.Information("{column} – Format : {format} – updated from {oldformat}",
                  columnCollection[colIndexExisting].Name,
                  checkResult.FoundValueFormat.GetTypeAndFormatDescription(),
                  oldVf.GetTypeAndFormatDescription());
                result.Add($"{columnCollection[colIndexExisting].Name} – Format : {checkResult.FoundValueFormat.GetTypeAndFormatDescription()} – updated from {oldVf.GetTypeAndFormatDescription()}");
                columnCollection.Replace(new ImmutableColumn(columnCollection[colIndexExisting],
                  checkResult.FoundValueFormat));
              }
              else
              {
                columnCollection.Add(new ImmutableColumn(readerColumn, checkResult.FoundValueFormat));
              }
            }
          }
        }

      if (fillGuessSettings.DateParts)
      {
        // Try to find a time for a date if the date does not already have a time Case a) TimeFormat
        // has already been recognized
        for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
        {
          cancellationToken.ThrowIfCancellationRequested();
          var readerColumn = fileReader.GetColumn(colIndex);
          var colIndexSetting = columnCollection.GetIndex(readerColumn.Name);
          if (colIndexSetting == -1) continue;
          var columnFormat = columnCollection[colIndexSetting].ValueFormat;

          // Possibly add Time Zone
          if (columnFormat.DataType == DataType.DateTime && string.IsNullOrEmpty(readerColumn.TimeZonePart))
            for (var colTimeZone = 0; colTimeZone < fileReader.FieldCount; colTimeZone++)
            {
              var columnTimeZone = fileReader.GetColumn(colTimeZone);
              var colName = columnTimeZone.Name.NoSpecials().ToUpperInvariant();
              if (columnTimeZone.ValueFormat.DataType != DataType.String &&
                  columnTimeZone.ValueFormat.DataType != DataType.Integer ||
                  colName != "TIMEZONE" && colName != "TIMEZONEID" && colName != "TIME ZONE" &&
                  colName != "TIME ZONE ID")
                continue;
              columnCollection.Replace(new ImmutableColumn(columnCollection[colIndexSetting].Name,
                columnCollection[colIndexSetting].ValueFormat, columnCollection[colIndexSetting].ColumnOrdinal,
                columnCollection[colIndexSetting].Convert, columnCollection[colIndexSetting].DestinationName,
                columnCollection[colIndexSetting].Ignore, columnCollection[colIndexSetting].Part,
                columnCollection[colIndexSetting].PartSplitter, columnCollection[colIndexSetting].PartToEnd,
                columnCollection[colIndexSetting].TimePart, columnCollection[colIndexSetting].TimePartFormat,
                columnTimeZone.Name));
              Logger.Information("{column} – Added Time Zone : {column2}", readerColumn.Name, columnTimeZone.Name);
              result.Add($"{readerColumn.Name} – Added Time Zone : {columnTimeZone.Name}");
            }

          if (columnFormat.DataType != DataType.DateTime || !string.IsNullOrEmpty(readerColumn.TimePart) ||
              columnFormat.DateFormat.IndexOfAny(new[] { ':', 'h', 'H', 'm', 's', 't' }) != -1)
            continue;
          // We have a date column without time
          for (var colTime = 0; colTime < fileReader.FieldCount; colTime++)
          {
            var columnTime = fileReader.GetColumn(colTime);
            var settingTime = columnCollection.Get(columnTime.Name);
            if (settingTime == null) continue;
            var timeFormat = settingTime.ValueFormat;
            if (timeFormat.DataType != DataType.DateTime || !string.IsNullOrEmpty(readerColumn.TimePart) ||
                timeFormat.DateFormat.IndexOfAny(new[] { '/', 'y', 'M', 'd' }) != -1)
              continue;
            // We now have a time column, checked if the names somehow make sense
            if (!readerColumn.Name.NoSpecials().ToUpperInvariant().Replace("DATE", string.Empty).Equals(
              columnTime.Name.NoSpecials().ToUpperInvariant().Replace("TIME", string.Empty),
              StringComparison.Ordinal))
              continue;
            columnCollection.Replace(new ImmutableColumn(columnCollection[colIndexSetting].Name,
              columnCollection[colIndexSetting].ValueFormat, columnCollection[colIndexSetting].ColumnOrdinal,
              columnCollection[colIndexSetting].Convert, columnCollection[colIndexSetting].DestinationName,
              columnCollection[colIndexSetting].Ignore, columnCollection[colIndexSetting].Part,
              columnCollection[colIndexSetting].PartSplitter, columnCollection[colIndexSetting].PartToEnd,
              columnTime.Name, timeFormat.DateFormat,
              columnCollection[colIndexSetting].TimeZonePart));

            Logger.Information("{column} – Added Time Part : {column2}", readerColumn.Name, columnTime.Name);
            result.Add($"{readerColumn.Name} – Added Time Part : {columnTime.Name}");
          }
        }

        // Case b) TimeFormat has not been recognized (e.G. all values are 08:00) only look in
        // adjacent fields
        for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
        {
          cancellationToken.ThrowIfCancellationRequested();
          var readerColumn = fileReader.GetColumn(colIndex);
          var colIndexSetting = columnCollection.GetIndex(readerColumn.Name);

          if (colIndexSetting == -1) continue;
          var columnFormat = columnCollection[colIndexSetting].ValueFormat;
          if (columnFormat.DataType != DataType.DateTime || !string.IsNullOrEmpty(readerColumn.TimePart) ||
              columnFormat.DateFormat.IndexOfAny(new[] { ':', 'h', 'H', 'm', 's', 't' }) != -1)
            continue;

          if (colIndex + 1 < fileReader.FieldCount)
          {
            var columnTime = fileReader.GetColumn(colIndex + 1);

            if (columnTime.ValueFormat.DataType == DataType.String && readerColumn.Name.NoSpecials()
              .ToUpperInvariant()
              .Replace("DATE", string.Empty)
              .Equals(columnTime.Name.NoSpecials().ToUpperInvariant().Replace("TIME", string.Empty),
                StringComparison.OrdinalIgnoreCase))
            {
              columnCollection.Replace(new ImmutableColumn(columnCollection[colIndexSetting].Name,
                columnCollection[colIndexSetting].ValueFormat, columnCollection[colIndexSetting].ColumnOrdinal,
                columnCollection[colIndexSetting].Convert, columnCollection[colIndexSetting].DestinationName,
                columnCollection[colIndexSetting].Ignore, columnCollection[colIndexSetting].Part,
                columnCollection[colIndexSetting].PartSplitter, columnCollection[colIndexSetting].PartToEnd,
                columnTime.Name, columnCollection[colIndexSetting].TimePartFormat,
                columnCollection[colIndexSetting].TimeZonePart));

              var samples = sampleList.Keys.Contains(colIndex + 1)
                ? sampleList[colIndex + 1]
                : (await GetSampleValuesAsync(fileReader, 1, new[] { colIndex + 1 }, 1, treatTextAsNull,
                  cancellationToken).ConfigureAwait(false)).First().Value;

              foreach (var first in samples.Values)
              {
                if (first.Length == 8 || first.Length == 5)
                {
                  columnCollection.Add(new ImmutableColumn(columnTime,
                    new ImmutableValueFormat(DataType.DateTime, first.Length == 8 ? "HH:mm:ss" : "HH:mm")));
                  Logger.Information("{column} – Format : {format}", columnTime.Name,
                    columnTime.GetTypeAndFormatDescription());
                  result.Add($"{readerColumn.Name}  – Format : {columnTime.GetTypeAndFormatDescription()}");
                }

                break;
              }

              Logger.Information("{column} – Added Time Part : {column2}", readerColumn.Name, columnTime.Name);
              result.Add($"{readerColumn.Name} – Added Time Part : {columnTime.Name}");
              continue;
            }
          }

          if (colIndex <= 0)
            continue;
          {
            var readerColumnTime = fileReader.GetColumn(colIndex - 1);

            if (readerColumnTime.ValueFormat.DataType != DataType.String ||
                !readerColumn.Name.NoSpecials().ToUpperInvariant().Replace("DATE", string.Empty).Equals(
                  readerColumnTime.Name.NoSpecials().ToUpperInvariant().Replace("TIME", string.Empty),
                  StringComparison.Ordinal))
              continue;
            columnCollection.Replace(new ImmutableColumn(columnCollection[colIndexSetting].Name,
              columnCollection[colIndexSetting].ValueFormat, columnCollection[colIndexSetting].ColumnOrdinal,
              columnCollection[colIndexSetting].Convert, columnCollection[colIndexSetting].DestinationName,
              columnCollection[colIndexSetting].Ignore, columnCollection[colIndexSetting].Part,
              columnCollection[colIndexSetting].PartSplitter, columnCollection[colIndexSetting].PartToEnd,
              readerColumnTime.Name, columnCollection[colIndexSetting].TimePartFormat,
              columnCollection[colIndexSetting].TimeZonePart));

            var samples = sampleList.Keys.Contains(colIndex - 1)
              ? sampleList[colIndex - 1]
              : (await GetSampleValuesAsync(fileReader, 1, new[] { colIndex + 1 }, 1, treatTextAsNull,
                cancellationToken).ConfigureAwait(false)).First().Value;
            foreach (var first in samples.Values)
            {
              if (first.Length == 8 || first.Length == 5)
              {
                columnCollection.Replace(new ImmutableColumn(columnCollection[colIndexSetting].Name,
                  columnCollection[colIndexSetting].ValueFormat, columnCollection[colIndexSetting].ColumnOrdinal,
                  columnCollection[colIndexSetting].Convert, columnCollection[colIndexSetting].DestinationName,
                  columnCollection[colIndexSetting].Ignore, columnCollection[colIndexSetting].Part,
                  columnCollection[colIndexSetting].PartSplitter, columnCollection[colIndexSetting].PartToEnd,
                  columnCollection[colIndexSetting].TimePart, first.Length == 8 ? "HH:mm:ss" : "HH:mm",
                  columnCollection[colIndexSetting].TimeZonePart));

                Logger.Information("{column} – Format : {format}", columnCollection[colIndexSetting].Name,
                  columnCollection[colIndexSetting].GetTypeAndFormatDescription());
                result.Add($"{columnCollection[colIndexSetting].Name} – Format : {columnCollection[colIndexSetting].GetTypeAndFormatDescription()}");
              }

              break;
            }

            Logger.Information("{column} – Added Time Part : {column2}", readerColumn.Name, readerColumnTime.Name);
            result.Add($"{readerColumn.Name} – Added Time Part : {readerColumnTime.Name}");
          }
        }
      }

      var existing = new Collection<IColumn>();
      foreach (var colName in columnNamesInFile)
        foreach (var col in columnCollection)
        {
          if (!col.Name.Equals(colName, StringComparison.OrdinalIgnoreCase))
            continue;
          existing.Add(col);
          break;
        }

      // 2nd columns defined but not in list
      foreach (var col in columnCollection)
        if (!existing.Contains(col))
          existing.Add(col);

      columnCollection.Clear();
      // ReSharper disable once InvertIf
      foreach (var column in existing)
        columnCollection.Add(column);

      return (result, columnCollection);
    }

#if !QUICK
    /// <summary>
    ///   Fills the Column Format for writer fileSettings
    /// </summary>
    /// <param name="fileSettings">The file settings.</param>
    /// <param name="all">if set to <c>true</c> event string columns are added.</param>
    /// <param name="processDisplay">The process display.</param>
    /// <exception cref="FileWriterException">No SQL Statement given or No SQL Reader set</exception>
    public static async Task FillGuessColumnFormatWriterAsync(this IFileSetting fileSettings, bool all,
      IProcessDisplay processDisplay)
    {
      if (string.IsNullOrEmpty(fileSettings.SqlStatement))
        throw new FileWriterException("No SQL Statement given");
      if (FunctionalDI.SQLDataReader == null)
        throw new FileWriterException("No Async SQL Reader set");

      using var fileReader =
        await FunctionalDI.SQLDataReader(fileSettings.SqlStatement, processDisplay.SetProcess, fileSettings.Timeout,
            processDisplay.CancellationToken)
          .ConfigureAwait(false);
      await fileReader.OpenAsync(processDisplay.CancellationToken).ConfigureAwait(false);
      // Put the information into the list
      var dataRowCollection = fileReader.GetSchemaTable()?.Rows;
      if (dataRowCollection == null) return;
      foreach (DataRow schemaRow in dataRowCollection)
      {
        var header = Convert.ToString(schemaRow[SchemaTableColumn.ColumnName]);
        if (string.IsNullOrEmpty(header))
          continue;
        var colType = ((Type) schemaRow[SchemaTableColumn.DataType]).GetDataType();

        if (!all && colType == DataType.String)
          continue;

        fileSettings.ColumnCollection.Add(new ImmutableColumn(header, new ImmutableValueFormat(colType), (int) schemaRow[SchemaTableColumn.ColumnOrdinal]));
      }
    }
#endif

    /// <summary>
    ///   Gets all possible formats based on the provided value
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="culture">The culture.</param>
    /// <returns></returns>
    public static IEnumerable<IValueFormat> GetAllPossibleFormats(string value,
      CultureInfo? culture = null)
    {
      culture ??= CultureInfo.CurrentCulture;

      // Standard Date Time formats
      foreach (var fmt in StringConversion.StandardDateTimeFormats.MatchingForLength(value.Length, true))
        foreach (var sep in StringConversion.DateSeparators.Where(sep => StringConversion
          .StringToDateTimeExact(value, fmt, sep, culture.DateTimeFormat.TimeSeparator, culture)
          .HasValue))
          yield return new ImmutableValueFormat(DataType.DateTime, fmt, sep);
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
    public static async Task<IDictionary<int, SampleResult>> GetSampleValuesAsync(IFileReader fileReader,
      long maxRecords,
      IEnumerable<int> columns, int enoughSamples, string treatAsNull, CancellationToken cancellationToken)
    {
      if (fileReader == null)
        throw new ArgumentNullException(nameof(fileReader));

      if (string.IsNullOrEmpty(treatAsNull))
        treatAsNull = "NULL;n/a";

      if (maxRecords < 1)
        maxRecords = long.MaxValue;

      var samples = columns.Where(col => col > -1 && col < fileReader.FieldCount)
        .ToDictionary<int, int, ICollection<string>>(col => col,
          col => new HashSet<string>(StringComparer.OrdinalIgnoreCase));

      if (samples.Keys.Count == 0)
        return new Dictionary<int, SampleResult>();

      if (fileReader.IsClosed)
        await fileReader.OpenAsync(cancellationToken).ConfigureAwait(false);

      Logger.Debug("Getting sample values for {numcolumns} columns", samples.Keys.Count);

      var hasWarning = false;
      var remainingShows = 10;

      void WarningEvent(object? sender, WarningEventArgs args)
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
          Logger.Debug("Resetting position to the beginning");
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
        while (recordRead < maxRecords && !cancellationToken.IsCancellationRequested &&
               samples.Keys.Count > enough.Count)
        {
          // if at the end start from the beginning
          if (!await fileReader.ReadAsync(cancellationToken).ConfigureAwait(false) && fileReader.EndOfFile)
          {
            if (!fileReader.SupportsReset)
              break;
            fileReader.ResetPositionToFirstDataRow();
            // If still at the end, we do not have a line
            if (startRecordNumber == 0 || !await fileReader.ReadAsync(cancellationToken).ConfigureAwait(false))
              break;
          }

          recordRead++;
          // In case there was a warning reading the line, ignore the line
          if (!hasWarning)
            foreach (var columnIndex in samples.Keys.Where(x => !((ICollection<int>) enough).Contains(x)))
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
                ((ICollection<int>) enough).Add(columnIndex);
            }
          else
            hasWarning = false;

          // Once we arrive the starting row we have read all records
          if (fileReader.RecordNumber == startRecordNumber)
            break;
        }
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "Reading Samples values");
      }
      finally
      {
        fileReader.Warning -= WarningEvent;
      }

      return samples.ToDictionary(keyValue => keyValue.Key,
        keyValue => new SampleResult(keyValue.Value, recordRead));
    }

#if !QUICK
    /// <summary>
    ///   Gets the writer source columns.
    /// </summary>
    /// <param name="sqlStatement"></param>
    /// <param name="timeout"></param>
    /// <param name="valueFormatGeneral">The general format for the output</param>
    /// <param name="columnDefinitions">Definition for individual columns</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<IEnumerable<IColumn>> GetWriterColumnInformationAsync(
      string? sqlStatement, int timeout,
      IValueFormat valueFormatGeneral, IReadOnlyCollection<IColumn> columnDefinitions,
      CancellationToken token)
    {
      if (valueFormatGeneral == null) throw new ArgumentNullException(nameof(valueFormatGeneral));
      if (columnDefinitions == null) throw new ArgumentNullException(nameof(columnDefinitions));
      if (string.IsNullOrEmpty(sqlStatement))
        return new List<ImmutableColumn>();

      if (FunctionalDI.SQLDataReader == null)
        throw new FileWriterException("No SQL Reader set");

      using var data = await FunctionalDI.SQLDataReader(sqlStatement!.NoRecordSQL(), (sender, s) =>
      {
        if (s.Log) Logger.Debug(s.Text);
      }, timeout, token).ConfigureAwait(false);
      await data.OpenAsync(token).ConfigureAwait(false);
      using var dt = data.GetSchemaTable();
      if (dt == null)
        throw new FileWriterException("Could not get source schema");
      return BaseFileWriter.GetColumnInformation(valueFormatGeneral, columnDefinitions, dt);
    }

    public static async Task<IEnumerable<string>> GetSqlColumnNamesAsync(
      string? sqlStatement, int timeout, CancellationToken token)
    {
      if (string.IsNullOrEmpty(sqlStatement))
        return new List<string>();

      using var data = await FunctionalDI.SQLDataReader(sqlStatement!.NoRecordSQL(), null, timeout, token)
        .ConfigureAwait(false);
      await data.OpenAsync(token).ConfigureAwait(false);
      var list = new List<string>();
      for (var index = 0; index < data.FieldCount; index++)
        list.Add(data.GetColumn(index).Name);
      return list;
    }
#endif

    /// <summary>
    ///   Guesses the date time format
    /// </summary>
    /// <param name="samples">The sample texts.</param>
    /// <param name="checkNamedDates">if set to <c>true</c> [check named dates].</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///   Result of a format check, if the samples match a value type this is set, if not an example
    ///   is give what did not match
    /// </returns>
    /// <exception cref="ArgumentNullException">samples</exception>
    public static CheckResult GuessDateTime(ICollection<string> samples, bool checkNamedDates,
      CancellationToken cancellationToken)
    {
      if (samples == null || samples.Count == 0)
        throw new ArgumentNullException(nameof(samples));

      var checkResult = new CheckResult();

      var length = samples.Aggregate<string, long>(0, (current, sample) => current + sample.Length);
      var commonLength = (int) (length / samples.Count);

      ICollection<string>? possibleDateSeparators = null;
      foreach (var fmt in StringConversion.StandardDateTimeFormats.MatchingForLength(commonLength, checkNamedDates))
      {
        if (cancellationToken.IsCancellationRequested)
          return checkResult;

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

    /// <summary>
    ///   Guesses a numeric format
    /// </summary>
    /// <param name="samples">The sample texts.</param>
    /// <param name="guessPercentage">True to find number between 0% and 100%</param>
    /// <param name="allowStartingZero">True if a leading zero should be considered as number</param>
    /// <param name="minSamples">Number of samples needed to be sure</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///   Result of a format check, if the samples match a value type this is set, if not an example
    ///   is give what did not match
    /// </returns>
    /// <exception cref="ArgumentNullException">samples is null or empty</exception>
    public static CheckResult GuessNumeric(ICollection<string> samples, bool guessPercentage,
      bool allowStartingZero, int minSamples, CancellationToken cancellationToken)
    {
      if (samples == null || samples.Count == 0)
        throw new ArgumentNullException(nameof(samples));
      var checkResult = new CheckResult();
      // Determine which decimalGrouping could be used
      var possibleGrouping = StringConversion.DecimalGroupings.Where(decGroup => !string.IsNullOrEmpty(decGroup) && samples.Any(smp => smp.IndexOf(decGroup, StringComparison.Ordinal) > -1))
        .ToList();
      possibleGrouping.Add(string.Empty);
      var possibleDecimal = StringConversion.DecimalSeparators.Where(decSep => !string.IsNullOrEmpty(decSep) && samples.Any(smp => smp.IndexOf(decSep, StringComparison.Ordinal) > -1)).ToList();

      // Need to have at least one decimal separator
      if (possibleDecimal.Count == 0)
        possibleDecimal.Add(".");

      foreach (var thousandSeparator in possibleGrouping)
        // Try Numbers: Int and Decimal
        foreach (var decimalSeparator in possibleDecimal)
        {
          if (cancellationToken.IsCancellationRequested)
            return checkResult;
          if (decimalSeparator.Equals(thousandSeparator))
            continue;
          var res = StringConversion.CheckNumber(samples, decimalSeparator, thousandSeparator, guessPercentage,
            allowStartingZero, minSamples);
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
    /// <exception cref="ArgumentNullException">samples is null or empty</exception>
    public static CheckResult GuessValueFormat(ICollection<string> samples, int minRequiredSamples,
      string trueValue, string falseValue, bool guessBoolean, bool guessGuid, bool guessNumeric, bool guessDateTime,
      bool guessPercentage, bool serialDateTime, bool checkNamedDates, IValueFormat? othersValueFormatDate,
      CancellationToken cancellationToken)
    {
      if (samples == null || samples.Count == 0)
        throw new ArgumentNullException(nameof(samples));

      var checkResult = new CheckResult();

      // ---------------- Boolean --------------------------
      if (guessBoolean && samples.Count <= 2)
      {
        var allParsed = true;
        var usedTrueValue = ValueFormatExtension.cTrueDefault;
        var usedFalseValue = ValueFormatExtension.cFalseDefault;
        foreach (var value in samples)
        {
          var resultBool = StringConversion.StringToBooleanStrict(value, trueValue, falseValue);
          if (resultBool == null)
          {
            allParsed = false;
            break;
          }

          if (resultBool.Item1)
            usedTrueValue = resultBool.Item2;
          else
            usedFalseValue = resultBool.Item2;
        }

        if (allParsed)
        {
          checkResult.FoundValueFormat =
            new ImmutableValueFormat(DataType.Boolean, asTrue: usedTrueValue, asFalse: usedFalseValue);
          return checkResult;
        }
      }

      cancellationToken.ThrowIfCancellationRequested();

      // ---------------- GUID --------------------------
      if (guessGuid && StringConversion.CheckGuid(samples))
      {
        checkResult.FoundValueFormat = new ImmutableValueFormat(DataType.Guid);
        return checkResult;
      }

      cancellationToken.ThrowIfCancellationRequested();

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
          if (valuesWithChars < samples.Count / 2 && valuesWithChars < 10)
            continue;
          checkResult.FoundValueFormat = new ImmutableValueFormat();
          return checkResult;
        }
      }

      // ---------------- Confirm old provided format would be ok --------------------------
      var firstValue = samples.First();

      if (guessDateTime && othersValueFormatDate != null &&
          StringConversion.DateLengthMatches(firstValue.Length, othersValueFormatDate.DateFormat))
      {
        var checkResultDateTime = StringConversion.CheckDate(samples, othersValueFormatDate.DateFormat,
          othersValueFormatDate.DateSeparator, othersValueFormatDate.TimeSeparator, CultureInfo.CurrentCulture);
        if (checkResultDateTime.FoundValueFormat != null)
          return checkResultDateTime;
        checkResult.KeepBestPossibleMatch(checkResultDateTime);
      }

      cancellationToken.ThrowIfCancellationRequested();

      // if we have less than the required samples values do not try and try to get a type
      if (samples.Count >= minRequiredSamples)
      {
        // Guess a date format that could be interpreted as number before testing numbers
        if (guessDateTime && firstValue.Length == 8)
        {
          var checkResultDateTime =
            StringConversion.CheckDate(samples, "yyyyMMdd", string.Empty, ":", CultureInfo.InvariantCulture);
          if (checkResultDateTime.FoundValueFormat != null)
            return checkResultDateTime;
          checkResult.KeepBestPossibleMatch(checkResultDateTime);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // We need to have at least 10 sample values here its too dangerous to assume it is a date
        if (guessDateTime && serialDateTime && samples.Count > 10 && samples.Count > minRequiredSamples)
        {
          var checkResultDateTime = StringConversion.CheckSerialDate(samples, true);
          if (checkResultDateTime.FoundValueFormat != null)
            return checkResultDateTime;
          checkResult.KeepBestPossibleMatch(checkResultDateTime);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // ---------------- Decimal / Integer --------------------------
        if (guessNumeric)
        {
          var checkResultNumeric = GuessNumeric(samples, guessPercentage, false, minRequiredSamples, cancellationToken);
          if (checkResultNumeric.FoundValueFormat != null)
            return checkResultNumeric;
          checkResult.KeepBestPossibleMatch(checkResultNumeric);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // ---------------- Date -------------------------- Minimum length of a date is 4 characters
        if (guessDateTime && firstValue.Length > 3)
        {
          var checkResultDateTime = GuessDateTime(samples, checkNamedDates, cancellationToken);
          if (checkResultDateTime.FoundValueFormat != null)
            return checkResultDateTime;
          checkResult.KeepBestPossibleMatch(checkResultDateTime);
        }

        cancellationToken.ThrowIfCancellationRequested();
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

      cancellationToken.ThrowIfCancellationRequested();

      // if we have dates and allow serial dates, but do not guess numeric (this would be a fit) try
      // if the dates are all serial
      if (!guessDateTime || !serialDateTime || guessNumeric)
        return checkResult;

      // ReSharper disable once InvertIf
      if (samples.Count >= minRequiredSamples)
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
          var index = new Random(Guid.NewGuid().GetHashCode()).Next(0,
            source.Count); //pick a random item from the master list
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