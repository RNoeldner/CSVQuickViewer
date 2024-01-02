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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    /// <summary>
    /// Get a common date format along the given columns
    /// </summary>
    /// <param name="columns">The columns.</param>
    /// <param name="guessDefault">The default value</param>
    /// <returns></returns>
    public static ValueFormat CommonDateFormat(in IEnumerable<Column> columns, in string guessDefault)
    {
      ValueFormat? best = null;
      var counterByFormat = new Dictionary<ValueFormat, int>();
      var maxValue = int.MinValue;
      foreach (var valueFormat in columns.Where(x => !x.Ignore).Select(x => x.ValueFormat).Where(x => x.DataType == DataTypeEnum.DateTime))
      {
        var found = counterByFormat.Keys.FirstOrDefault(x => x.Equals(valueFormat));
        if (found is null)
        {
          found = valueFormat;
          counterByFormat.Add(found, 0);
        }
        counterByFormat[found]++;
        if (counterByFormat[found] <= maxValue)
          continue;

        maxValue = counterByFormat[found];
        best = found;
      }
      if (best is null)
      {
        if (!string.IsNullOrEmpty(guessDefault))
        {
          // determine possible date separator from text
          var posSep = guessDefault.IndexOfAny(new[] { '/', '\\', '.', '-' });
          if (posSep != 1)
          {
            var separator = guessDefault[posSep].ToString();
            return new ValueFormat(DataTypeEnum.DateTime, guessDefault.Replace(separator, "/").Trim(), separator);
          }

        }
        return new ValueFormat(DataTypeEnum.DateTime, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
          CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator,
          CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator);
      }

      return best;
    }

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
    /// ///
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>A text with the changes that have been made and a list of the determined columns</returns>
    public static async Task<(IList<string>, IReadOnlyCollection<Column>)> FillGuessColumnFormatReaderAsyncReader(
      this IFileReader fileReader,
      FillGuessSettings fillGuessSettings,
      IEnumerable<Column>? columnCollectionInput,
      bool addTextColumns,
      bool checkDoubleToBeInteger,
      string treatTextAsNull,
      CancellationToken cancellationToken)
    {
      if (fileReader is null)
        throw new ArgumentNullException(nameof(fileReader));
      if (fillGuessSettings is null)
        throw new ArgumentNullException(nameof(fillGuessSettings));

      columnCollectionInput ??= Array.Empty<Column>();

      if (!fillGuessSettings.Enabled || fillGuessSettings is { DetectNumbers: false, DetectBoolean: false, DetectDateTime: false, DetectGuid: false, DetectPercentage: false, SerialDateTime: false })
        return (Array.Empty<string>(), new List<Column>(columnCollectionInput));

      if (fileReader.FieldCount == 0)
        return (Array.Empty<string>(), new List<Column>(columnCollectionInput));

      if (fileReader.EndOfFile)
        fileReader.ResetPositionToFirstDataRow();

      if (fileReader.EndOfFile) // still end of file
        return (Array.Empty<string>(), new List<Column>(columnCollectionInput));

      var result = new List<string>();

      var columnCollection = new ColumnCollection();
      // Only use column definition for columns that do not exist
      foreach (var col in columnCollectionInput)
        for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
        {
          if (!col.Name.Equals(fileReader.GetName(colIndex), StringComparison.OrdinalIgnoreCase)) continue;
          columnCollection.Add(col);
          break;
        }

      var othersValueFormatDate = CommonDateFormat(columnCollection, fillGuessSettings.DateFormat);

      // build a list of columns to check
      var getSamples = new List<int>();
      var columnNamesInFile = new List<string>();
      for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
      {
        var readerColumn = fileReader.GetColumn(colIndex);

        columnNamesInFile.Add(readerColumn.Name);

        if (fillGuessSettings.IgnoreIdColumns && StringUtils.AssumeIdColumn(readerColumn.Name) > 0)
        {
          Logger.Information("{column} – ID columns ignored", readerColumn.Name);
          result.Add($"{readerColumn.Name} – ID columns ignored");

          if (!addTextColumns || columnCollection.IndexOf(readerColumn) != -1) continue;
          columnCollection.Add(readerColumn);
          continue;
        }

        // no need to get types that are already found and could now be smaller (e.G. decimal could
        // be a integer)
        if (readerColumn.ValueFormat.DataType == DataTypeEnum.Guid
            || readerColumn.ValueFormat.DataType == DataTypeEnum.Integer
            || readerColumn.ValueFormat.DataType == DataTypeEnum.Boolean
            || readerColumn.ValueFormat.DataType == DataTypeEnum.DateTime
            || (readerColumn.ValueFormat.DataType == DataTypeEnum.Numeric && !checkDoubleToBeInteger)
            || (readerColumn.ValueFormat.DataType == DataTypeEnum.Double && !checkDoubleToBeInteger))
        {
          Logger.Information(
            "{column} - Existing Type : {format}",
            readerColumn.Name,
            readerColumn.ValueFormat.GetTypeAndFormatDescription());
          result.Add($"{readerColumn.Name} - Existing Type : {readerColumn.ValueFormat.GetTypeAndFormatDescription()}");
          continue;
        }

        getSamples.Add(colIndex);
      }

      var sampleList = await GetSampleValuesAsync(
        fileReader,
        fillGuessSettings.CheckedRecords,
        getSamples,
        fillGuessSettings.SampleValues,
        treatTextAsNull, 100,
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
          Logger.Information(
            "{column} – No values found – Format : {format}",
            readerColumn.Name,
            readerColumn.GetTypeAndFormatDescription());
          result.Add($"{readerColumn.Name} – No values found – Format : {readerColumn.GetTypeAndFormatDescription()}");
          if (!addTextColumns)
            continue;
          columnCollection.Add(readerColumn);
        }
        else
        {

          Logger.Information(
            "{column} – {values} values found in {records} row.",
            readerColumn.Name,
            samples.Values.Count(),
            samples.RecordsRead);

          var checkResult = GuessValueFormat(
            samples.Values,
            fillGuessSettings.MinSamples,
            fillGuessSettings.TrueValue.AsSpan(),
            fillGuessSettings.FalseValue.AsSpan(),
            fillGuessSettings.DetectBoolean,
            fillGuessSettings.DetectGuid,
            fillGuessSettings.DetectNumbers,
            fillGuessSettings.DetectDateTime,
            fillGuessSettings.DetectPercentage,
            fillGuessSettings.SerialDateTime,
            fillGuessSettings.RemoveCurrencySymbols,
            othersValueFormatDate,
            cancellationToken);

          // if nothing is found take what was configured before, as the reader could possibly
          // provide typed data (Json, Excel...)
          checkResult.FoundValueFormat ??= readerColumn.ValueFormat;
          var colIndexCurrent = columnCollection.IndexOf(readerColumn);
          // if we have a mapping to a template that expects a integer and we only have integers but
          // not enough
          if (colIndexCurrent != -1)
          {
            if (checkResult.FoundValueFormat.DataType == DataTypeEnum.DateTime)
            {
              // if he date format does not match the last found date format reset the assumed
              // correct format
              if (!othersValueFormatDate.Equals(checkResult.FoundValueFormat))
                othersValueFormatDate = checkResult.FoundValueFormat;
            }

            var oldValueFormat = columnCollection[colIndexCurrent].GetTypeAndFormatDescription();

            if (checkResult.FoundValueFormat.Equals(columnCollection[colIndexCurrent].ValueFormat))
            {
              Logger.Information("{column} – Format : {format} – not changed", readerColumn.Name, oldValueFormat);
            }
            else
            {
              var newValueFormat = checkResult.FoundValueFormat.GetTypeAndFormatDescription();
              if (oldValueFormat.Equals(newValueFormat, StringComparison.Ordinal))
                continue;
              Logger.Information(
                "{column} – Format : {format} – updated from {old format}",
                readerColumn.Name,
                newValueFormat,
                oldValueFormat);
              result.Add($"{readerColumn.Name} – Format : {newValueFormat} – updated from {oldValueFormat}");
              columnCollection.Replace(columnCollection[colIndexCurrent].ReplaceValueFormat(checkResult.FoundValueFormat));
            }
          }
          // new Column
          else
          {
            var format =
              (checkResult.PossibleMatch ? checkResult.ValueFormatPossibleMatch : checkResult.FoundValueFormat)
              ?? ValueFormat.Empty;

            if (!addTextColumns && format.DataType == DataTypeEnum.String) continue;

            Logger.Information("{column} – Format : {format}", readerColumn.Name, format.GetTypeAndFormatDescription());
            result.Add($"{readerColumn.Name} – Format : {format.GetTypeAndFormatDescription()}");
            columnCollection.Add(readerColumn.ReplaceValueFormat(format));

            // Adjust or Set the common date format
            if (format.DataType == DataTypeEnum.DateTime)
              othersValueFormatDate = CommonDateFormat(columnCollection, fillGuessSettings.DateFormat);
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

          if (readerColumn.ValueFormat.DataType != DataTypeEnum.Double
              && readerColumn.ValueFormat.DataType != DataTypeEnum.Numeric) continue;
          if (sampleList.Keys.Contains(colIndex + 1))
          {
            var samples = sampleList[colIndex + 1];

            if (samples.Values.Count <= 0) continue;
            var checkResult = GuessNumeric(
              samples.Values,
              false,
              true,
              fillGuessSettings.RemoveCurrencySymbols,
              cancellationToken);
            if (checkResult.FoundValueFormat is null ||
                checkResult.FoundValueFormat.DataType == DataTypeEnum.Double) continue;
            var colIndexExisting = columnCollection.IndexOf(readerColumn);

            if (colIndexExisting != -1)
            {
              var oldVf = columnCollection[colIndexExisting].ValueFormat;
              if (oldVf.Equals(checkResult.FoundValueFormat)) continue;
              Logger.Information(
                "{column} – Format : {format} – updated from {old format}",
                columnCollection[colIndexExisting].Name,
                checkResult.FoundValueFormat.GetTypeAndFormatDescription(),
                oldVf.GetTypeAndFormatDescription());
              result.Add(
                $"{columnCollection[colIndexExisting].Name} – Format : {checkResult.FoundValueFormat.GetTypeAndFormatDescription()} – updated from {oldVf.GetTypeAndFormatDescription()}");
              columnCollection.Replace(
                columnCollection[colIndexExisting].ReplaceValueFormat(checkResult.FoundValueFormat));
            }
            else
            {
              columnCollection.Add(readerColumn.ReplaceValueFormat(checkResult.FoundValueFormat));
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
          var colIndexSetting = columnCollection.IndexOf(readerColumn);
          if (colIndexSetting == -1) continue;
          var columnFormat = columnCollection[colIndexSetting].ValueFormat;

          // Possibly add Time Zone
          if (columnFormat.DataType == DataTypeEnum.DateTime && string.IsNullOrEmpty(readerColumn.TimeZonePart))
            for (var colTimeZone = 0; colTimeZone < fileReader.FieldCount; colTimeZone++)
            {
              var columnTimeZone = fileReader.GetColumn(colTimeZone);
              var colName = columnTimeZone.Name.NoSpecials().ToUpperInvariant();
              if ((columnTimeZone.ValueFormat.DataType != DataTypeEnum.String
                   && columnTimeZone.ValueFormat.DataType != DataTypeEnum.Integer)
                  || (colName != "TIMEZONE" && colName != "TIMEZONEID" && colName != "TIME ZONE"
                      && colName != "TIME ZONE ID"))
                continue;
              columnCollection.Replace(
                new Column(
                  columnCollection[colIndexSetting].Name,
                  columnCollection[colIndexSetting].ValueFormat,
                  columnCollection[colIndexSetting].ColumnOrdinal,
                  columnCollection[colIndexSetting].Ignore,
                  columnCollection[colIndexSetting].Convert,
                  columnCollection[colIndexSetting].DestinationName,
                  columnCollection[colIndexSetting].TimePart, columnCollection[colIndexSetting].TimePartFormat, columnTimeZone.Name));
              Logger.Information("{column} – Added Time Zone : {column2}", readerColumn.Name, columnTimeZone.Name);
              result.Add($"{readerColumn.Name} – Added Time Zone : {columnTimeZone.Name}");
            }

          if (columnFormat.DataType != DataTypeEnum.DateTime || !string.IsNullOrEmpty(readerColumn.TimePart)
                                                             || columnFormat.DateFormat.IndexOfAny(
                                                               new[] { ':', 'h', 'H', 'm', 's', 't' }) != -1)
            continue;
          // We have a date column without time
          for (var colTime = 0; colTime < fileReader.FieldCount; colTime++)
          {
            var columnTime = fileReader.GetColumn(colTime);
            var colTimeIndex = columnCollection.IndexOf(columnTime);
            if (colTimeIndex == -1) continue;
            var timeFormat = columnCollection[colTimeIndex].ValueFormat;
            if (timeFormat.DataType != DataTypeEnum.DateTime || !string.IsNullOrEmpty(readerColumn.TimePart)
                                                             || timeFormat.DateFormat.IndexOfAny(new[]
                                                             { '/', 'y', 'M', 'd' }) != -1)
              continue;
            // We now have a time column, checked if the names somehow make sense
            if (!readerColumn.Name.NoSpecials().ToUpperInvariant().Replace("DATE", string.Empty).Equals(
                  columnTime.Name.NoSpecials().ToUpperInvariant().Replace("TIME", string.Empty),
                  StringComparison.Ordinal))
              continue;
            columnCollection.Replace(
              new Column(
                columnCollection[colIndexSetting].Name,
                columnCollection[colIndexSetting].ValueFormat,
                columnCollection[colIndexSetting].ColumnOrdinal,
                columnCollection[colIndexSetting].Ignore,
                columnCollection[colIndexSetting].Convert,
                columnCollection[colIndexSetting].DestinationName,
                columnTime.Name, timeFormat.DateFormat, columnCollection[colIndexSetting].TimeZonePart));

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
          var colIndexSetting = columnCollection.IndexOf(readerColumn);

          if (colIndexSetting == -1) continue;
          var columnFormat = columnCollection[colIndexSetting].ValueFormat;
          if (columnFormat.DataType != DataTypeEnum.DateTime || !string.IsNullOrEmpty(readerColumn.TimePart)
                                                             || columnFormat.DateFormat.IndexOfAny(
                                                               new[] { ':', 'h', 'H', 'm', 's', 't' }) != -1)
            continue;

          if (colIndex + 1 < fileReader.FieldCount)
          {
            var columnTime = fileReader.GetColumn(colIndex + 1);
            if (columnTime.ValueFormat.DataType == DataTypeEnum.String && readerColumn.Name.NoSpecials()
                  .ToUpperInvariant()
                  .Replace("DATE", string.Empty).Equals(
                    columnTime.Name.NoSpecials().ToUpperInvariant()
                      .Replace("TIME", string.Empty),
                    StringComparison.OrdinalIgnoreCase))
            {
              columnCollection.Replace(
                new Column(
                  columnCollection[colIndexSetting].Name,
                  columnCollection[colIndexSetting].ValueFormat,
                  columnCollection[colIndexSetting].ColumnOrdinal,
                  columnCollection[colIndexSetting].Ignore,
                  columnCollection[colIndexSetting].Convert,
                  columnCollection[colIndexSetting].DestinationName,
                  columnTime.Name, columnCollection[colIndexSetting].TimePartFormat, columnCollection[colIndexSetting].TimeZonePart));

              var firstValueNewColumn = (sampleList.Keys.Contains(colIndex + 1)
                ? sampleList[colIndex + 1]
                : (await GetSampleValuesAsync(
                  fileReader,
                  1,
                  new[] { colIndex + 1 },
                  1,
                  treatTextAsNull, 80,
                  cancellationToken).ConfigureAwait(false)).First().Value).Values.FirstOrDefault();
              if (!firstValueNewColumn.IsEmpty && (firstValueNewColumn.Length == 8 || firstValueNewColumn.Length == 5))
              {
                columnCollection.Add(columnTime.ReplaceValueFormat(
                    new ValueFormat(DataTypeEnum.DateTime,
                      firstValueNewColumn.Length == 8 ? "HH:mm:ss" : "HH:mm")));
                Logger.Information(
                  "{column} – Format : {format}",
                  columnTime.Name,
                  columnTime.GetTypeAndFormatDescription());
                result.Add($"{readerColumn.Name}  – Format : {columnTime.GetTypeAndFormatDescription()}");
              }

              Logger.Information("{column} – Added Time Part : {column2}", readerColumn.Name, columnTime.Name);
              result.Add($"{readerColumn.Name} – Added Time Part : {columnTime.Name}");
              continue;
            }
          }

          if (colIndex <= 0)
            continue;

          var readerColumnTime = fileReader.GetColumn(colIndex - 1);
          if (readerColumnTime.ValueFormat.DataType != DataTypeEnum.String || !readerColumn.Name.NoSpecials()
                .ToUpperInvariant().Replace("DATE", string.Empty).Equals(
                  readerColumnTime.Name.NoSpecials().ToUpperInvariant()
                    .Replace("TIME", string.Empty),
                  StringComparison.Ordinal))
            continue;
          columnCollection.Replace(
            new Column(
              columnCollection[colIndexSetting].Name,
              columnCollection[colIndexSetting].ValueFormat,
              columnCollection[colIndexSetting].ColumnOrdinal,
              columnCollection[colIndexSetting].Ignore,
              columnCollection[colIndexSetting].Convert,
              columnCollection[colIndexSetting].DestinationName,
              readerColumnTime.Name, columnCollection[colIndexSetting].TimePartFormat, columnCollection[colIndexSetting].TimeZonePart));

          var firstValueNewColumn2 = (sampleList.Keys.Contains(colIndex - 1)
            ? sampleList[colIndex - 1]
            : (await GetSampleValuesAsync(fileReader, 1, new[] { colIndex + 1 }, 1, treatTextAsNull, 80,
                cancellationToken)
              .ConfigureAwait(false)).First().Value).Values.FirstOrDefault();
          if (!firstValueNewColumn2.IsEmpty && (firstValueNewColumn2.Length == 8 || firstValueNewColumn2.Length == 5))
          {
            columnCollection.Replace(
              new Column(
                columnCollection[colIndexSetting].Name,
                columnCollection[colIndexSetting].ValueFormat,
                columnCollection[colIndexSetting].ColumnOrdinal,
                columnCollection[colIndexSetting].Ignore,
                columnCollection[colIndexSetting].Convert,
                columnCollection[colIndexSetting].DestinationName,
                columnCollection[colIndexSetting].TimePart, firstValueNewColumn2.Length == 8 ? "HH:mm:ss" : "HH:mm", columnCollection[colIndexSetting].TimeZonePart));

            Logger.Information(
              "{column} – Format : {format}",
              columnCollection[colIndexSetting].Name,
              columnCollection[colIndexSetting].GetTypeAndFormatDescription());
            result.Add(
              $"{columnCollection[colIndexSetting].Name} – Format : {columnCollection[colIndexSetting].GetTypeAndFormatDescription()}");
          }

          Logger.Information("{column} – Added Time Part : {column2}", readerColumn.Name, readerColumnTime.Name);
          result.Add($"{readerColumn.Name} – Added Time Part : {readerColumnTime.Name}");
        }
      }

      var existing = new Collection<Column>();
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
    /// <param name="maxChars">Examples are cut off if longer than this number of characters</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">dataReader</exception>
    /// <exception cref="ArgumentOutOfRangeException">no valid columns provided</exception>
    public static async Task<IDictionary<int, SampleResult>> GetSampleValuesAsync(
      IFileReader fileReader,
      long maxRecords,
      IEnumerable<int> columns,
      int enoughSamples,
      string treatAsNull,
      int maxChars,
      CancellationToken cancellationToken)
    {
      if (fileReader is null)
        throw new ArgumentNullException(nameof(fileReader));

      if (string.IsNullOrEmpty(treatAsNull))
        treatAsNull = "NULL;n/a";

      if (maxRecords < 1)
        maxRecords = long.MaxValue;

      var samples = columns.Where(col => col > -1 && col < fileReader.FieldCount)
        .ToDictionary<int, int, IList<string>>(col => col, _ => new List<string>());

      if (samples.Keys.Count == 0)
        return new Dictionary<int, SampleResult>();

      if (fileReader.IsClosed)
        await fileReader.OpenAsync(cancellationToken).ConfigureAwait(false);

      var hasWarning = false;
      var remainingShows = 10;

      void WarningEvent(object? sender, WarningEventArgs args)
      {
        if (args.ColumnNumber != -1 && !samples.ContainsKey(args.ColumnNumber))
          return;
        if (remainingShows-- > 0)
          Logger.Debug("Row ignored in inspection: " + args.Message);
        if (remainingShows == 0)
          Logger.Debug("No further warning shown");

        hasWarning = true;
      }

      fileReader.Warning += WarningEvent;
      var action = new IntervalAction(2);
      action.Invoke(() =>
        Logger.Information("Getting sample values"));

      var recordRead = 0;
      try
      {
        // could already be at EOF need to reset
        if (fileReader is { EndOfFile: true, SupportsReset: true })
          fileReader.ResetPositionToFirstDataRow();

        // Ready to start store the record number we are currently at, we could be in the middle of
        // the file already
        var startRecordNumber = fileReader.RecordNumber;

        var maxSamples = 2000;
        if (maxSamples < enoughSamples)
          maxSamples = enoughSamples;
        var startPercent = fileReader.Percent;

        var enough = new List<int>();
        // Get distinct sample values until we have
        // * parsed the maximum number
        // * have enough samples to be satisfied
        // * we are at the beginning record again
        while (recordRead < maxRecords && !cancellationToken.IsCancellationRequested
                                       && samples.Keys.Count > enough.Count)
        {
          action.Invoke(() =>
            Logger.Information(
              $"Getting sample values {(fileReader.Percent < startPercent ? 100 - startPercent + fileReader.Percent : fileReader.Percent - startPercent)}%"));
          // if at the end start from the beginning
          if (!await fileReader.ReadAsync(cancellationToken).ConfigureAwait(false) && fileReader.EndOfFile)
          {
            if (!fileReader.SupportsReset)
              break;
            fileReader.ResetPositionToFirstDataRow();
            // if we started at the beginning, and we are now back, exist if we started, and we can
            // not read a line exist as well.
            if (startRecordNumber == 0 || !await fileReader.ReadAsync(cancellationToken).ConfigureAwait(false))
              break;
          }

          recordRead++;
          // In case there was a warning reading the line, ignore the line
          if (!hasWarning)
            try
            {
              foreach (var columnIndex in samples.Keys.Except(enough))
              {
                // value must be string as spans are not supported in lambda
                var value = fileReader.GetString(columnIndex).Trim();
                // Any non existing value is not of interest
                if (value.Length==0)
                  continue;

                // Always do treat Text "Null" as Null, 
                if (StringUtils.ShouldBeTreatedAsNull(value, treatAsNull))
                  continue;

                // cut of
                if (maxChars > 0 && value.Length > maxChars)
                  value = value.Substring(0, maxChars);

                // collect the value if there are not enough samples and the text is not present yet
                if (samples[columnIndex].Count < maxSamples &&
                    !samples[columnIndex].Contains(value, StringComparer.OrdinalIgnoreCase))
                {
                  samples[columnIndex].Add(value);

                  if (samples[columnIndex].Count >= enoughSamples)
                    enough.Add(columnIndex);
                }
              }
            }
            catch
            {
              Logger.Warning("Issue reading line {line}, possible empty training column", fileReader.StartLineNumber);
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

      return samples.ToDictionary(keyValue => keyValue.Key, keyValue => new SampleResult(keyValue.Value, recordRead));
    }

    /// <summary>
    ///   Guesses the date time format
    /// </summary>
    /// <param name="samples">The sample texts.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>
    ///   Result of a format check, if the samples match a value type this is set, if not an example
    ///   is give what did not match
    /// </returns>
    /// <exception cref="ArgumentNullException">samples</exception>
    public static CheckResult GuessDateTime(IReadOnlyCollection<ReadOnlyMemory<char>> samples,
      in CancellationToken cancellationToken)
    {
      if (samples is null || samples.Count == 0)
        throw new ArgumentNullException(nameof(samples));

      var checkResult = new CheckResult();

      var length = samples.Aggregate<ReadOnlyMemory<char>, long>(0, (current, sample) => current + sample.Length);
      var commonLength = (int) (length / samples.Count);


      // loop through the samples and filter out date separators that are not part of any sample
      var possibleDateSeparators = new List<char>();
      int best = int.MinValue;
      foreach (var kv in StaticCollections.DateSeparatorChars.ToDictionary(sep => sep, sep =>
                 samples.Count(entry => entry.Span.IndexOf(sep) != -1)).OrderByDescending(x => x.Value))
      {
        // only take the separators that have been found, and then only takes the ones for the most rows
        if (kv.Value < best)
          break;
        // in case of a tie, take multiple
        best = kv.Value;
        possibleDateSeparators.Add(kv.Key);
      }


      foreach (var fmt in StaticCollections.StandardDateTimeFormats.MatchingForLength(commonLength))
      {
        if (cancellationToken.IsCancellationRequested)
          return checkResult;

        if (fmt.IndexOf('/') != -1)
        {
          foreach (var sep in possibleDateSeparators)
          {
            var res = samples.CheckDate(fmt.AsSpan(), sep, ':', CultureInfo.CurrentCulture, cancellationToken);
            if (res.FoundValueFormat != null)
              return res;

            checkResult.KeepBestPossibleMatch(res);
          }
        }
        else
        {
          // we have no date separator in the format no need to test different separators
          var res = samples.CheckDate(fmt.AsSpan(), char.MinValue, ':', CultureInfo.CurrentCulture, cancellationToken);
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
    /// <param name="removeCurrencySymbols"></param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>
    ///   Result of a format check, if the samples match a value type this is set, if not an example
    ///   is give what did not match
    /// </returns>
    /// <exception cref="ArgumentNullException">samples is null or empty</exception>
    public static CheckResult GuessNumeric(
      IReadOnlyCollection<ReadOnlyMemory<char>> samples,
      bool guessPercentage,
      bool allowStartingZero,
      bool removeCurrencySymbols,
      in CancellationToken cancellationToken)
    {
      if (samples is null || samples.Count == 0)
        throw new ArgumentNullException(nameof(samples));
      var checkResult = new CheckResult();
      // Determine which decimalGrouping could be used
      var possibleGrouping = StaticCollections.DecimalGroupingChars.Where(
        decGroup => samples.Any(smp => smp.Span.IndexOf(decGroup) > -1)).ToList();
      possibleGrouping.Add('\0');
      var possibleDecimal = StaticCollections.DecimalSeparatorChars.Where(
        decSep => samples.Any(smp => smp.Span.IndexOf(decSep) > -1)).ToList();

      // Need to have at least one decimal separator
      if (possibleDecimal.Count == 0)
        possibleDecimal.Add('.');

      foreach (var thousandSeparator in possibleGrouping)
        // Try Numbers: Int and Decimal
        foreach (var decimalSeparator in possibleDecimal)
        {
          if (cancellationToken.IsCancellationRequested)
            return checkResult;
          if (decimalSeparator.Equals(thousandSeparator))
            continue;
          var res = samples.CheckNumber(
            decimalSeparator,
            thousandSeparator,
            guessPercentage,
            allowStartingZero,
            removeCurrencySymbols,
            cancellationToken);
          if (res.FoundValueFormat != null)
            return res;

          checkResult.KeepBestPossibleMatch(res);
        }

      return checkResult;
    }

    /// <summary>
    ///   Guesses the value format.
    /// </summary>
    /// <param name="samples">The samples.</param>
    /// <param name="minRequiredSamples">The minimum required samples.</param>
    /// <param name="trueValue">The text to be regarded as <c>true</c></param>
    /// <param name="falseValue">The text to be regarded as <c>false</c></param>
    /// <param name="guessBoolean">Try to identify a boolean</param>
    /// <param name="guessGuid">Try to determine if it's a GUID</param>
    /// <param name="guessNumeric">Try to determine if it's a Number</param>
    /// <param name="guessDateTime">Try to determine if it is a date time</param>
    /// <param name="guessPercentage">Accept percentage values</param>
    /// <param name="serialDateTime">Allow serial Date time</param>
    /// <param name="removeCurrencySymbols"></param>
    /// <param name="othersValueFormatDate">
    ///   The date format found in prior columns, assuming the data format is the same in other
    ///   columns, we do not need that many samples
    /// </param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <exception cref="ArgumentNullException">samples is null or empty</exception>
    public static CheckResult GuessValueFormat(IReadOnlyCollection<ReadOnlyMemory<char>> samples,
      int minRequiredSamples,
      ReadOnlySpan<char> trueValue,
      ReadOnlySpan<char> falseValue,
      bool guessBoolean,
      bool guessGuid,
      bool guessNumeric,
      bool guessDateTime,
      bool guessPercentage,
      bool serialDateTime,
      bool removeCurrencySymbols,
      in ValueFormat othersValueFormatDate,
      in CancellationToken cancellationToken)
    {
      if (samples is null || samples.Count == 0)
        throw new ArgumentNullException(nameof(samples));

      var checkResult = new CheckResult();

      // ---------------- Boolean --------------------------
      if (guessBoolean && samples.Count <= 2)
      {
        var allParsed = true;
        var usedTrueValue = ValueFormat.cTrueDefault;
        var usedFalseValue = ValueFormat.cFalseDefault;
        foreach (var value in samples)
        {
          (var resultBool, string val) = value.Span.StringToBooleanWithMatch(trueValue, falseValue);
          if (resultBool.HasValue)
          {
            if (resultBool.Value)
              usedTrueValue = val;
            else
              usedFalseValue = val;
          }
          else
          {
            allParsed = false;
            break;
          }
        }

        if (allParsed)
        {
          checkResult.FoundValueFormat = new ValueFormat(
            DataTypeEnum.Boolean,
            asTrue: usedTrueValue,
            asFalse: usedFalseValue);
          return checkResult;
        }
      }

      cancellationToken.ThrowIfCancellationRequested();

      // ---------------- GUID --------------------------
      if (guessGuid && samples.CheckGuid(cancellationToken))
      {
        checkResult.FoundValueFormat = new ValueFormat(DataTypeEnum.Guid);
        return checkResult;
      }

      cancellationToken.ThrowIfCancellationRequested();

      // ---------------- Confirm old provided format would be OK --------------------------
      // ---------------- No matter if we have enough samples 

      var firstValue = samples.First();

      // Assume dates are of the same format across the files we check if the dates we have would
      // possibly match no matter how many samples we have this time we do not care about matching
      // length Check Date will cut off time information , this is independent of minRequiredSamples
      if (guessDateTime && othersValueFormatDate.DataType == DataTypeEnum.DateTime && StaticCollections.StandardDateTimeFormats.DateLengthMatches(firstValue.Length, othersValueFormatDate.DateFormat))
      {
        var checkResultDateTime = samples.CheckDate(
           othersValueFormatDate.DateFormat.AsSpan(),
          othersValueFormatDate.DateSeparator,
          othersValueFormatDate.TimeSeparator,
          CultureInfo.CurrentCulture, cancellationToken);
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
          var checkResultDateTime = samples.CheckDate("yyyyMMdd".AsSpan(), char.MinValue, ':', CultureInfo.InvariantCulture, cancellationToken);
          if (checkResultDateTime.FoundValueFormat != null)
            return checkResultDateTime;
          checkResult.KeepBestPossibleMatch(checkResultDateTime);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // We need to have at least 10 sample values here it's too dangerous to assume it is a date
        if (guessDateTime && serialDateTime && samples.Count > 10)
        {
          var checkResultDateTime = samples.CheckSerialDate(true, cancellationToken);
          if (checkResultDateTime.FoundValueFormat != null)
            return checkResultDateTime;
          checkResult.KeepBestPossibleMatch(checkResultDateTime);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // ---------------- Decimal / Integer --------------------------
        if (guessNumeric)
        {
          var checkResultNumeric = GuessNumeric(samples, guessPercentage, false, removeCurrencySymbols, cancellationToken);
          if (checkResultNumeric.FoundValueFormat != null)
            return checkResultNumeric;
          checkResult.KeepBestPossibleMatch(checkResultNumeric);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // ---------------- Date -------------------------- Minimum length of a date is 4 characters
        if (guessDateTime && firstValue.Length > 3)
        {
          var checkResultDateTime = GuessDateTime(samples, cancellationToken);
          if (checkResultDateTime.FoundValueFormat != null)
            return checkResultDateTime;
          checkResult.KeepBestPossibleMatch(checkResultDateTime);
        }
      }

      cancellationToken.ThrowIfCancellationRequested();

      if (!checkResult.PossibleMatch)
      {
        var res = samples.CheckUnescaped(minRequiredSamples, cancellationToken);
        if (res != DataTypeEnum.String)
        {
          checkResult.PossibleMatch = true;
          checkResult.ValueFormatPossibleMatch = new ValueFormat(res);
          checkResult.FoundValueFormat = new ValueFormat(res);
          return checkResult;
        }
      }

      cancellationToken.ThrowIfCancellationRequested();

      // if we have dates and allow serial dates, but do not guess numeric (this would be a fit) try
      // if the dates are all serial
      if (!guessDateTime || !serialDateTime || guessNumeric)
        return checkResult;

      // ReSharper disable once InvertIf
      if (samples.Count >= minRequiredSamples)
      {
        var res = samples.CheckSerialDate(false, cancellationToken);
        if (res.FoundValueFormat != null)
          return res;
        checkResult.KeepBestPossibleMatch(res);
      }

      return checkResult;
    }

    /// <summary>
    /// Class storing samples in random order
    /// </summary>
    [DebuggerDisplay("SampleResult: {Values.Count} of {RecordsRead}")]
    public class SampleResult
    {
      /// <summary>
      ///   Initializes a new instance of the class and stores all passed in values in random order
      /// </summary>
      /// <param name="items">The initial set of sample values</param>
      /// <param name="records">The number of records that have been read to obtain the values</param>
      public SampleResult(IEnumerable<string> items, int records)
      {
        RecordsRead = records;
        var random = new Random(Guid.NewGuid().GetHashCode());
        var valueList = new List<ReadOnlyMemory<char>>();
        foreach (var item in items)
          valueList.Insert(random.Next(0, valueList.Count + 1), item.AsMemory());
        Values = valueList;
      }

      /// <summary>
      ///   Gets the records read.
      /// </summary>
      /// <value>The number of records that have been read. this is not the number of values.</value>
      public int RecordsRead { get; }

      /// <summary>
      ///   Gets the values.
      /// </summary>
      /// <value>The unique values read in random order</value>
      public IReadOnlyCollection<ReadOnlyMemory<char>> Values { get; }
    }

#if !QUICK

    /// <summary>
    ///   Fills the ColumnCollection for reader fileSettings
    /// </summary>
    /// <param name="fileSetting">The file setting to check, and columns to be added</param>
    /// <param name="addTextColumns">if set to <c>true</c> event string columns are added.</param>
    /// <param name="checkDoubleToBeInteger">if set to <c>true</c> [check double to be integer].</param>
    /// <param name="fillGuessSettings">The fill guess settings.</param>
    /// ///
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>A list of columns with new format that have been changed</returns>
    public static async Task<(IList<string>, IReadOnlyCollection<Column>)> FillGuessColumnFormatReaderAsync(
      this IFileSetting fileSetting,
      bool addTextColumns,
      bool checkDoubleToBeInteger,
      FillGuessSettings fillGuessSettings,
      CancellationToken cancellationToken)
    {
      if (fileSetting is null)
        throw new ArgumentNullException(nameof(fileSetting));
      if (fillGuessSettings is null)
        throw new ArgumentNullException(nameof(fillGuessSettings));

      // Check if we are supposed to check something
      if (!fillGuessSettings.Enabled || fillGuessSettings is { DetectNumbers: false, DetectBoolean: false, DetectDateTime: false, DetectGuid: false, DetectPercentage: false, SerialDateTime: false })
        return (new List<string>(), fileSetting.ColumnCollection);

      // in case there is no delimiter but it's a delimited file, do nothing
      if (fileSetting is ICsvFile { FieldDelimiterChar: char.MinValue })
        return (new List<string>(), fileSetting.ColumnCollection);

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var fileReader = await fileSetting.GetUntypedFileReaderAsync(cancellationToken);
      return await FillGuessColumnFormatReaderAsyncReader(
        fileReader,
        fillGuessSettings,
        fileSetting.ColumnCollection,
        addTextColumns,
        checkDoubleToBeInteger,
        fileSetting.TreatTextAsNull,
        cancellationToken).ConfigureAwait(false);
    }
#endif

  }
}