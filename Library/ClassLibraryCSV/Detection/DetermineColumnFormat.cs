/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Collections.Generic;
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
    private static readonly char[] m_TimeFormat = new[] { ':', 'h', 'H', 'm', 's', 't' };
    private static readonly char[] m_DateFormat = new[] { '/', 'y', 'M', 'd' };

    /// <summary>
    /// Determines the most common date format used in the provided columns,
    /// or falls back to a guessed/default value or system culture format.
    /// </summary>
    /// <param name="columns">A sequence of columns to analyze.</param>
    /// <param name="guessDefault">A fallback date string used to guess format and separator, if needed.</param>
    /// <returns>A <see cref="ValueFormat"/> representing the most likely date format.</returns>
    public static ValueFormat CommonDateFormat(in IEnumerable<Column> columns, string guessDefault)
    {
      // Find the most-used date format among relevant columns.
      var mostUsed = columns
        .Where(x => x is { Ignore: false, ValueFormat.DataType: DataTypeEnum.DateTime })
        .GroupBy(x => x.ValueFormat)
        .OrderByDescending(g => g.Count())
        .Select(g => g.Key)
        .FirstOrDefault();

      if (mostUsed != null)
      {
        // If found, use the most common date format.
        return mostUsed;
      }

      // If no strong candidate, try to infer from the provided guessDefault string.
      if (string.IsNullOrEmpty(guessDefault))
      {
        return new ValueFormat(
          DataTypeEnum.DateTime,
          CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
          CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator,
          CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator
        );
      }

      // Check for any common date separator in the guessDefault.
      var separators = new[] { '/', '\\', '.', '-' };
      var posSep = guessDefault.IndexOfAny(separators);

      // Only proceed if a separator was found.
      if (posSep == -1)
      {
        return new ValueFormat(
          DataTypeEnum.DateTime,
          CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
          CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator,
          CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator
        );
      }

      var separator = guessDefault[posSep].ToString();

      // Normalize the pattern by replacing found separator with "/".
      var normalizedPattern = guessDefault.Replace(separator, "/").Trim();

      // Return constructed ValueFormat based on guessDefault.
      return new ValueFormat(
        DataTypeEnum.DateTime,
        normalizedPattern,
        separator
      );

      // Fallback: Use the system's default short date pattern and separators.      
    }

    /// <summary>
    ///   Goes through the open reader and add found fields to the column collection
    /// </summary>
    /// <param name="fileReader">A ready to read file reader</param>
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

      if (!fillGuessSettings.Enabled || fillGuessSettings is
          {
            DetectNumbers: false, DetectBoolean: false, DetectDateTime: false, DetectGuid: false,
            DetectPercentage: false, SerialDateTime: false
          })
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

      var columnCache = new Dictionary<int, Column>();
      // Pre-fill the cache for all columns
      for (int colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
        columnCache[colIndex] = fileReader.GetColumn(colIndex);

      // Build a list of columns to check
      var getSamples = new List<int>();
      for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
      {
        var column = columnCache[colIndex];
        // Check if column should be ignored        
        if (fillGuessSettings.IgnoreIdColumns && StringUtils.AssumeIdColumn(column.Name) > 0)
        {
          Logger.Information("{column} – ID columns ignored", column.Name);
          result.Add($"{column.Name} – ID columns ignored");

          if (!addTextColumns || columnCollection.Contains(column)) continue;
          columnCollection.Add(column);
          continue;
        }

        // Check for already-known data types
        var dt = column.ValueFormat.DataType;
        bool isKnownType = dt == DataTypeEnum.Guid
                           || dt == DataTypeEnum.Integer
                           || dt == DataTypeEnum.Boolean
                           || dt == DataTypeEnum.DateTime
                           || (dt == DataTypeEnum.Numeric && !checkDoubleToBeInteger)
                           || (dt == DataTypeEnum.Double && !checkDoubleToBeInteger);

        if (isKnownType)
        {
          Logger.Information("{column} - Existing Type : {format}", column.Name,
            column.ValueFormat.GetTypeAndFormatDescription());
          result.Add($"{column.Name} - Existing Type : {column.ValueFormat.GetTypeAndFormatDescription()}");
          continue;
        }

        getSamples.Add(colIndex);
      }

      var sampleList = await GetSampleValuesAsync(
        fileReader,
        fillGuessSettings.CheckedRecords,
        getSamples,
        treatTextAsNull,
        100, cancellationToken).ConfigureAwait(false);

      // In case there are not enough distinct records do not validate
      if (sampleList.Count == 0 || sampleList.Max(x => x.Value.Values.Count) < fillGuessSettings.MinSamples)
      {
        Logger.Information("Not enough records to determine types");
        var allcolumns = new List<Column>();
        for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
          allcolumns.Add(columnCache[colIndex]);

        return (Array.Empty<string>(), allcolumns);
      }

      // Add all columns that will not be guessed
      for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
      {
        if (sampleList.ContainsKey(colIndex)) continue;
        var readerColumn = columnCache[colIndex];
        columnCollection.Add(readerColumn);
      }

      cancellationToken.ThrowIfCancellationRequested();

      // Start Guessing
      var othersValueFormatDate = CommonDateFormat(columnCollection, fillGuessSettings.DateFormat);
      foreach (var colIndex in sampleList.Keys)
      {
        var readerColumn = columnCache[colIndex];
        var samples = sampleList[colIndex];

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
          // if we have a mapping to a template that expects an integer, and we only have integers but
          // not enough
          if (colIndexCurrent != -1)
          {
            if (checkResult.FoundValueFormat.DataType == DataTypeEnum.DateTime)
            {
              // if the date format does not match the last found date format reset the assumed
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

              columnCollection.Replace(columnCollection[colIndexCurrent]
                .ReplaceValueFormat(checkResult.FoundValueFormat));
            }
          }
          // new Column
          else
          {
            var format =
              (checkResult.PossibleMatch ? checkResult.ValueFormatPossibleMatch : checkResult.FoundValueFormat)
              ?? ValueFormat.Empty;

            if (!addTextColumns && format.DataType == DataTypeEnum.String) continue;

            Logger.Information("{column} – Format : {format}", readerColumn.Name,
              format.GetTypeAndFormatDescription());

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
          var readerColumn = columnCache[colIndex];

          if (readerColumn.ValueFormat.DataType != DataTypeEnum.Double
              && readerColumn.ValueFormat.DataType != DataTypeEnum.Numeric) continue;
          if (sampleList.ContainsKey(colIndex + 1))
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

      cancellationToken.ThrowIfCancellationRequested();

      if (fillGuessSettings.DateParts)
      {
        // Case a)
        // Try to find a time for a date if the date does not already have a time Case 
        // a TimeFormat has already been recognized
        for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
        {
          var readerColumn = columnCache[colIndex];
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
                  columnCollection[colIndexSetting].TimePart, columnCollection[colIndexSetting].TimePartFormat,
                  columnTimeZone.Name));
              Logger.Information("{column} – Added Time Zone : {column2}", readerColumn.Name, columnTimeZone.Name);

              result.Add($"{readerColumn.Name} – Added Time Zone : {columnTimeZone.Name}");
            }


          if (columnFormat.DataType != DataTypeEnum.DateTime || !string.IsNullOrEmpty(readerColumn.TimePart)
                                                             || columnFormat.DateFormat.IndexOfAny(
                                                               m_TimeFormat) != -1)
            continue;
          // We have a date column without time
          for (var colTime = 0; colTime < fileReader.FieldCount; colTime++)
          {
            var columnTime = fileReader.GetColumn(colTime);
            var colTimeIndex = columnCollection.IndexOf(columnTime);
            if (colTimeIndex == -1) continue;
            var timeFormat = columnCollection[colTimeIndex].ValueFormat;
            if (timeFormat.DataType != DataTypeEnum.DateTime || !string.IsNullOrEmpty(readerColumn.TimePart)
                                                             || timeFormat.DateFormat.IndexOfAny(m_DateFormat) != -1)
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

        // Case b)
        // TimeFormat has not been recognized (e.G. all values are 08:00) only look in
        // adjacent fields
        for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
        {
          var readerColumn = columnCache[colIndex];
          var colIndexSetting = columnCollection.IndexOf(readerColumn);

          if (colIndexSetting == -1) continue;
          var columnFormat = columnCollection[colIndexSetting].ValueFormat;
          if (columnFormat.DataType != DataTypeEnum.DateTime || !string.IsNullOrEmpty(readerColumn.TimePart)
                                                             || columnFormat.DateFormat.IndexOfAny(m_TimeFormat) != -1)
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
                  columnTime.Name, columnCollection[colIndexSetting].TimePartFormat,
                  columnCollection[colIndexSetting].TimeZonePart));

              var firstValueNewColumn = (sampleList.ContainsKey(colIndex + 1)
                ? sampleList[colIndex + 1]
                : (await GetSampleValuesAsync(
                  fileReader,
                  1,
                  new[] { colIndex + 1 },
                  treatTextAsNull,
                  80, cancellationToken).ConfigureAwait(false)).First().Value).Values.FirstOrDefault();
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
              readerColumnTime.Name, columnCollection[colIndexSetting].TimePartFormat,
              columnCollection[colIndexSetting].TimeZonePart));

          var firstValueNewColumn2 = (sampleList.ContainsKey(colIndex - 1)
            ? sampleList[colIndex - 1]
            : (await GetSampleValuesAsync(fileReader, 1, new[] { colIndex + 1 }, treatTextAsNull, 80, cancellationToken)
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
                columnCollection[colIndexSetting].TimePart, firstValueNewColumn2.Length == 8 ? "HH:mm:ss" : "HH:mm",
                columnCollection[colIndexSetting].TimeZonePart));

            try
            {
              Logger.Information(
                "{column} – Format : {format}",
                columnCollection[colIndexSetting].Name,
                columnCollection[colIndexSetting].GetTypeAndFormatDescription());
            }
            catch { }

            result.Add(
              $"{columnCollection[colIndexSetting].Name} – Format : {columnCollection[colIndexSetting].GetTypeAndFormatDescription()}");
          }

          Logger.Information("{column} – Added Time Part : {column2}", readerColumn.Name, readerColumnTime.Name);
          result.Add($"{readerColumn.Name} – Added Time Part : {readerColumnTime.Name}");
        }
      }

      // Reorder columns to match file order
      return (result, ReorderColumns(columnCollection, fileReader));
    }

    // Reorder columns to match file order
    private static List<Column> ReorderColumns(ColumnCollection columnCollection, IFileReader fileReader)
    {
      var lookup = columnCollection.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
      var ordered = new List<Column>();

      for (int i = 0; i < fileReader.FieldCount; i++)
      {
        if (lookup.TryGetValue(fileReader.GetName(i), out var col))
          ordered.Add(col);
      }

      foreach (var col in columnCollection)
        if (!ordered.Contains(col))
          ordered.Add(col);
      return ordered;
    }

    /// <summary>
    /// Retrieves sample string values for specified columns by looping through the file,
    /// skipping rows with warnings or invalid data, and collecting unique samples per column.
    /// </summary>
    /// <param name="fileReader">An <see cref="IFileReader"/> to read data rows from</param>
    /// <param name="maxRecords">Maximum rows to scan (0 means unlimited)</param>
    /// <param name="columns">Enumerable of column indices to sample values from</param>
    /// <param name="treatAsNull">Semicolon-separated words to treat as null (e.g. "NULL;n/a")</param>
    /// <param name="maxChars">Trim samples to at most this many characters (0=no trim)</param>
    /// <param name="cancellationToken">Allows canceling the operation</param>
    /// 
    /// <returns>Dictionary: column index -> result object containing list of samples and records read</returns>
    public static async Task<IDictionary<int, SampleResult>> GetSampleValuesAsync(
      IFileReader fileReader,
      long maxRecords,
      IEnumerable<int> columns,
      string treatAsNull,
      int maxChars,
      CancellationToken cancellationToken)
    {
      // Argument checks
      if (fileReader is null)
        throw new ArgumentNullException(nameof(fileReader));
      if (columns is null)
        throw new ArgumentNullException(nameof(columns));

      if (maxRecords < 1)
        maxRecords = long.MaxValue;
      if (string.IsNullOrWhiteSpace(treatAsNull))
        treatAsNull = "NULL;n/a";

      // Prepare dictionary to collect unique sample values per column
      var sampleDict = columns
        .Where(col => col >= 0 && col < fileReader.FieldCount)
        .Distinct()
        .ToDictionary(col => col, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));

      // Return empty if no valid columns
      if (sampleDict.Count == 0)
        return new Dictionary<int, SampleResult>();

      // Tracking state for warning events and limiting repeated warning logs
      var hasWarningOnRow = false;
      var warningLogsRemaining = 10;

      // Attach warning event handler
      void OnWarning(object? sender, WarningEventArgs e)
      {
        // Only process warnings from requested columns
        if (e.ColumnNumber != -1 && !sampleDict.ContainsKey(e.ColumnNumber))
          return;
        // More Columns is ignored if the extra column is empty
        if (e.Message.Contains(CsvFileReader.cMoreColumns) && e.Message.Contains("empty"))
          return;
        try
        {
          if (warningLogsRemaining > 0)
          {
            Logger.Debug("Row ignored during sample: " + e.Message);
            warningLogsRemaining--;
            if (warningLogsRemaining == 0)
              Logger.Debug("No further warnings will be shown for sample extraction.");
          }
        }
        catch
        {
          /* Logging shouldn't crash sampling */
        }

        hasWarningOnRow = true;
      }

      fileReader.Warning += OnWarning;

      var recordsScanned = 0;

      // If supported and already at EOF, restart
      if (fileReader is { EndOfFile: true, SupportsReset: true })
        fileReader.ResetPositionToFirstDataRow();

      var startingRow = fileReader.RecordNumber;
      var startPercent = fileReader.Percent;

      // The repeated status-interval logger
      var action = new IntervalAction(1);
      try
      {
        action.Invoke(() => Logger.Information("Starting sample extraction..."));

        while (recordsScanned < maxRecords && !cancellationToken.IsCancellationRequested)
        {
          try
          {
            action.Invoke(() =>
              Logger.Information(
                $"Sampling progress: {(fileReader.Percent < startPercent ? 100 - startPercent + fileReader.Percent : fileReader.Percent - startPercent)}%"
              ));
          }
          catch
          {
            /* Ignore logging exceptions */
          }

          // Handle EOF and looping
          if (!await fileReader.ReadAsync(cancellationToken).ConfigureAwait(false))
          {
            if (!fileReader.SupportsReset)
              break;
            fileReader.ResetPositionToFirstDataRow();
            if (startingRow == 0 || !await fileReader.ReadAsync(cancellationToken).ConfigureAwait(false))
              break; // End of file: no more data
          }

          recordsScanned++;

          // Ignore row if a warning occurred during reading
          if (hasWarningOnRow)
          {
            hasWarningOnRow = false;
            continue;
          }

          try
          {
            // Only process columns that still need more samples
            foreach (var colIdx in sampleDict.Keys)
            {
              if (fileReader.IsDBNull(colIdx))
                continue;

              var value = fileReader.GetValue(colIdx)?.ToString();
              if (string.IsNullOrWhiteSpace(value))
                continue;

              value = value!.Trim();
              if (StringUtils.ShouldBeTreatedAsNull(value, treatAsNull))
                continue;

              // Truncate to maxChars if necessary
              if (maxChars > 0 && value.Length > maxChars)
                value = value.Substring(0, maxChars);

              // Add new (case-insensitive) sample if not yet collected
              if (sampleDict[colIdx].Count < 10000)
                sampleDict[colIdx].Add(value);
            }
          }
          catch (Exception ex)
          {
            Logger.Warning(ex, "Problem parsing a row during sample extraction at line {line}",
              fileReader.StartLineNumber);
          }

          // Detect looped through data (e.g. after reset for non-eof sources)
          if (fileReader.RecordNumber == startingRow)
            break;
        }
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "Exception thrown while extracting sample values");
      }
      finally
      {
        fileReader.Warning -= OnWarning;
      }

      // Construct results: column index => SampleResult
      return sampleDict.ToDictionary(
        kvp => kvp.Key,
        kvp => new SampleResult(kvp.Value, recordsScanned)
      );
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
      var possibleGrouping = StaticCollections.DecimalGroupingChars
        .Where(decGroup => samples.Any(smp => smp.Span.IndexOf(decGroup) > -1)).ToList();
      possibleGrouping.Add('\0');
      var possibleDecimal = StaticCollections.DecimalSeparatorChars
        .Where(decSep => samples.Any(smp => smp.Span.IndexOf(decSep) > -1)).ToList();

      // Need to have at least one decimal separator
      if (possibleDecimal.Count == 0)
        possibleDecimal.Add('.');

      foreach (var thousandSep in possibleGrouping)
        // Try Numbers: Int and Decimal
      foreach (var decimalSep in possibleDecimal)
      {
        if (cancellationToken.IsCancellationRequested)
          return checkResult;
        if (decimalSep.Equals(thousandSep))
          continue;
        var res = samples.CheckNumber(
          decimalSep,
          thousandSep,
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
    ///   Attempts to infer the most likely value format (Boolean, Guid, Numeric, DateTime, etc.) from a collection of sample strings.
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
    /// <param name="removeCurrencySymbols">"If true, currency symbols will be ignored during numeric detection</param>
    /// <param name="othersValueFormatDate">
    ///   The date format found in prior columns, assuming the data format is the same in other
    ///   columns, we do not need that many samples
    /// </param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <exception cref="ArgumentNullException">samples is null or empty</exception>
    /// <exception cref="OperationCanceledException">If cancellation is requested.</exception>
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
      if (guessDateTime && othersValueFormatDate.DataType == DataTypeEnum.DateTime &&
          StaticCollections.StandardDateTimeFormats.DateLengthMatches(firstValue.Length,
            othersValueFormatDate.DateFormat))
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

      // Skip type inference if not enough samples
      if (samples.Count >= minRequiredSamples)
      {
        // Guess a date format that could be interpreted as number before testing numbers
        if (guessDateTime && firstValue.Length == 8)
        {
          var checkResultDateTime = samples.CheckDate("yyyyMMdd".AsSpan(), char.MinValue, ':',
            CultureInfo.InvariantCulture, cancellationToken);
          if (checkResultDateTime.FoundValueFormat != null)
            return checkResultDateTime;
          checkResult.KeepBestPossibleMatch(checkResultDateTime);
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (guessDateTime && serialDateTime)
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
          var checkResultNumeric =
            GuessNumeric(samples, guessPercentage, false, removeCurrencySymbols, cancellationToken);
          if (checkResultNumeric.FoundValueFormat != null)
            return checkResultNumeric;
          checkResult.KeepBestPossibleMatch(checkResultNumeric);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // ---------------- Date --------------------------
        // Minimum length of a date is 4 characters
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

      // if we have dates and allow serial dates, but do not guess numeric (this would be a fit) try
      // if the dates are all serial
      if (!guessDateTime || !serialDateTime || guessNumeric)
        return checkResult;

      cancellationToken.ThrowIfCancellationRequested();

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
#if NET6_0_OR_GREATER
        var random = Random.Shared;
#else
        var random = new Random(Environment.TickCount);
#endif
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
      if (!fillGuessSettings.Enabled || fillGuessSettings is
          {
            DetectNumbers: false, DetectBoolean: false, DetectDateTime: false, DetectGuid: false,
            DetectPercentage: false, SerialDateTime: false
          })
        return (new List<string>(), fileSetting.ColumnCollection);

      // in case there is no delimiter, but it's a delimited file, do nothing
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
  }
}