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
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CsvTools
{

  /// <summary>
  ///   Helper class
  /// </summary>
  public static class DetermineColumnFormat
  {
    public static ValueFormat CommonDateFormat(IEnumerable<ValueFormat> columns)
    {
      var counterByFormat = new Dictionary<ValueFormat, int>();
      foreach (var newColumn in columns.Where(x => x != null && x.DataType == DataType.DateTime))
      {
        if (!counterByFormat.ContainsKey(newColumn))
          counterByFormat.Add(newColumn, 0);
        counterByFormat[newColumn]++;
      }

      if (counterByFormat.Count == 0)
        return null;

      var maxOccur = counterByFormat.Values.Max();
      return counterByFormat.FirstOrDefault(x => x.Value == maxOccur).Key;
    }

    /// <summary>
    ///   Fills the Column Format for reader fileSettings
    /// </summary>
    /// <param name="fileSetting">The file setting to check, and fill</param>
    /// <param name="addTextColumns">if set to <c>true</c> event string columns are added.</param>
    /// <param name="processDisplay">The process display.</param>
    public static IList<string> FillGuessColumnFormatReader(this IFileSetting fileSetting, bool addTextColumns,
     IProcessDisplay processDisplay)
    {
      if (processDisplay == null)
      {
        throw new ArgumentNullException(nameof(processDisplay));
      }

      Contract.Requires(fileSetting != null);
      var result = new List<string>();

      // if we should not detect, we can finish
      if (!ApplicationSetting.FillGuessSettings.DetectBoolean && !ApplicationSetting.FillGuessSettings.DetectGUID &&
        !ApplicationSetting.FillGuessSettings.DectectNumbers &&
        !ApplicationSetting.FillGuessSettings.DetectDateTime &&
        !ApplicationSetting.FillGuessSettings.DectectPercentage &&
        !ApplicationSetting.FillGuessSettings.SerialDateTime)
      {
        return result;
      }

      var resetSkipRows = false;
      try
      {
        // Make sure that if we do have a CSV file without header that we will skip the first row that
        // might contain headers, but its simply set as without headers.
        if (fileSetting is CsvFile && !fileSetting.HasFieldHeader && fileSetting.SkipRows == 0)
        {
          fileSetting.SkipRows = 1;
          resetSkipRows = true;
        }
        var othersValueFormatDate = CommonDateFormat(fileSetting.Column.Select(x => x.ValueFormat));

        using (var fileReader = fileSetting.GetFileReader())
        {
          Contract.Assume(fileReader != null);
          // fileReader.ProcessDisplay = processDisplay;

          fileReader.Open(false, processDisplay.CancellationToken);
          if (fileReader.FieldCount == 0 || fileReader.EndOfFile)
            return result;
          processDisplay.SetProcess("Getting column headers");
          processDisplay.Maximum = fileReader.FieldCount;

          var columnNamesInFile = new List<string>();
          for (var colindex = 0; colindex < fileReader.FieldCount; colindex++)
          {           
            var newColumn = fileReader.GetColumn(colindex);
            Contract.Assume(newColumn != null);
            columnNamesInFile.Add(newColumn.Name);
            var oldColumn = fileSetting.GetColumn(newColumn.Name);

            processDisplay.SetProcess(newColumn.Name + " – Getting values", colindex);            

            var samples = GetSampleValues(fileReader, ApplicationSetting.FillGuessSettings.CheckedRecords,
              colindex, ApplicationSetting.FillGuessSettings.SampleValues, fileSetting.TreatTextAsNull,
              processDisplay.CancellationToken);

            if (samples.IsEmpty())
            {
              processDisplay.SetProcess(newColumn.Name + " – No values found", colindex);
              if (!addTextColumns) continue;
              result.Add($"{newColumn.Name} – No values found – Format : {newColumn.GetTypeAndFormatDescription()}");
              fileSetting.ColumnAdd(newColumn);
            }
            else
            {
              var detect = !(ApplicationSetting.FillGuessSettings.IgnoreIdColums &&
                      StringUtils.AssumeIDColumn(newColumn.Name) > 0);

              if (samples.Count < 10)
                processDisplay.SetProcess($"{newColumn.Name} – Only {samples.Count} values found in {ApplicationSetting.FillGuessSettings.CheckedRecords} rows", colindex);
              else
                processDisplay.SetProcess($"{newColumn.Name} – {samples.Count} values found – Examining format", colindex);

              var checkResult = GuessValueFormat(samples, ApplicationSetting.FillGuessSettings.MinSamplesForIntDate,
                ApplicationSetting.FillGuessSettings.TrueValue,
                ApplicationSetting.FillGuessSettings.FalseValue,
                ApplicationSetting.FillGuessSettings.DetectBoolean && detect,
                ApplicationSetting.FillGuessSettings.DetectGUID && detect,
                ApplicationSetting.FillGuessSettings.DectectNumbers && detect,
                ApplicationSetting.FillGuessSettings.DetectDateTime && detect,
                ApplicationSetting.FillGuessSettings.DectectPercentage && detect,
                ApplicationSetting.FillGuessSettings.SerialDateTime && detect,
                ApplicationSetting.FillGuessSettings.CheckNamedDates && detect,
                othersValueFormatDate,
                processDisplay.CancellationToken);

              if (checkResult == null)
              {
                if (addTextColumns)
                  checkResult = new CheckResult { FoundValueFormat = new ValueFormat() };
                else
                  continue;
              }

              // if we have a mapping to a template that expects a integer and we only have integers but not enough...
              if (oldColumn != null)
              {
                var oldValueFormat = oldColumn.GetTypeAndFormatDescription();

                // if we have a date value format already store this
                if (othersValueFormatDate == null && checkResult.FoundValueFormat.DataType == DataType.DateTime && checkResult.PossibleMatch)
                  othersValueFormatDate = checkResult.FoundValueFormat;

                if (checkResult.FoundValueFormat.Equals(oldColumn.ValueFormat))
                {
                  processDisplay.SetProcess($"{newColumn.Name} – Format : {oldValueFormat} – not changed",
                   colindex);
                }
                else
                  oldColumn.ValueFormat = checkResult.FoundValueFormat;

                var newValueFormat = checkResult.FoundValueFormat.GetTypeAndFormatDescription();
                if (oldValueFormat.Equals(newValueFormat, StringComparison.Ordinal)) continue;
                var msg = $"{newColumn.Name} – Format : {newValueFormat} – updated from {oldValueFormat}";
                result.Add(msg);
                processDisplay.SetProcess(msg, colindex);
              }
              else
              {
                if (!addTextColumns && checkResult.FoundValueFormat.DataType == DataType.String) continue;
                newColumn.ValueFormat = checkResult.FoundValueFormat;
                var msg = $"{newColumn.Name} – Format : {newColumn.GetTypeAndFormatDescription()}";
                processDisplay.SetProcess(msg, colindex);
                result.Add(msg);
                fileSetting.ColumnAdd(newColumn);
              }
            }
          }

          // The fileReader does not have the column information yet, let the reader know
          fileReader.OverrideColumnFormatFromSetting(fileReader.FieldCount);
          // in case its Excel, check all doubles if they could be integer
          if (fileSetting is IExcelFile)
          {
            for (var colindex = 0; colindex < fileReader.FieldCount; colindex++)
            {
              var oldColumn = fileReader.GetColumn(colindex);
              var detect = !(ApplicationSetting.FillGuessSettings.IgnoreIdColums &&
                              StringUtils.AssumeIDColumn(oldColumn.Name) > 0);

              if (oldColumn != null && oldColumn.DataType == DataType.Double)
              {
                Column newColumn = null;

                if (detect)
                {
                  var samples = GetSampleValues(fileReader, ApplicationSetting.FillGuessSettings.CheckedRecords,
                    colindex, ApplicationSetting.FillGuessSettings.SampleValues, fileSetting.TreatTextAsNull,
                    processDisplay.CancellationToken);

                  if (!samples.IsEmpty())
                  {
                    var checkResult = GuessNumeric(samples, false, true, processDisplay.CancellationToken);
                    if (checkResult != null && checkResult.FoundValueFormat.DataType != DataType.Double)
                    {
                      newColumn = fileSetting.GetColumn(oldColumn.Name);
                      if (newColumn == null)
                        newColumn = fileSetting.ColumnAdd(oldColumn);

                      newColumn.DataType = checkResult.FoundValueFormat.DataType;
                    }
                  }
                }
                else
                {
                  newColumn = fileSetting.GetColumn(oldColumn.Name);
                  if (newColumn == null)
                    newColumn = fileSetting.ColumnAdd(oldColumn);
                  newColumn.DataType = DataType.String;

                }
                if (newColumn != null)
                {
                  var msg = $"{newColumn.Name} – Overwritten Excel Format : {newColumn.GetTypeAndFormatDescription()}";
                  processDisplay.SetProcess(msg, colindex);
                  result.Add(msg);
                }
              }
            }
          }

          if (ApplicationSetting.FillGuessSettings.DateParts)
          {
            // Try to find a time for a date if the date does not already have a time
            // Case a) TimeFormat has already been recognized
            for (var colindex = 0; colindex < fileReader.FieldCount; colindex++)
            {
              var columnDate = fileReader.GetColumn(colindex);

              // Possibly add Time Zone
              if (columnDate.DataType == DataType.DateTime && string.IsNullOrEmpty(columnDate.TimeZonePart))
              {
                for (var coltimeZone = 0; coltimeZone < fileReader.FieldCount; coltimeZone++)
                {
                  var columnTimeZone = fileReader.GetColumn(coltimeZone);
                  var colName = columnTimeZone.Name.NoSpecials().ToUpperInvariant();
                  if (columnTimeZone.DataType != DataType.String && columnTimeZone.DataType != DataType.Integer ||
                    colName != "TIMEZONE" && colName != "TIMEZONEID" && colName != "TIME ZONE" &&
                    colName != "TIME ZONE ID")
                  {
                    continue;
                  }

                  columnDate.TimeZonePart = columnTimeZone.Name;
                  result.Add($"{columnDate.Name} – Added Time Zone : {columnTimeZone.Name}");
                }
              }

              if (columnDate.DataType != DataType.DateTime || !string.IsNullOrEmpty(columnDate.TimePart) ||
                columnDate.ValueFormat.DateFormat.IndexOfAny(new[] { ':', 'h', 'H', 'm', 's', 't' }) != -1)
              {
                continue;
              }
              // We have a date column without time
              for (var coltime = 0; coltime < fileReader.FieldCount; coltime++)
              {
                var columnTime = fileReader.GetColumn(coltime);
                if (columnTime.DataType != DataType.DateTime || !string.IsNullOrEmpty(columnDate.TimePart) ||
                  columnTime.ValueFormat.DateFormat.IndexOfAny(new[] { '/', 'y', 'M', 'd' }) != -1)
                {
                  continue;
                }
                // We now have a time column,
                // checked if the names somehow make sense
                if (!columnDate.Name.NoSpecials().ToUpperInvariant().Replace("DATE", string.Empty).Equals(columnTime.Name.NoSpecials().ToUpperInvariant().Replace("TIME", string.Empty), StringComparison.Ordinal))
                {
                  continue;
                }

                columnDate.TimePart = columnTime.Name;
                columnDate.TimePartFormat = columnTime.ValueFormat.DateFormat;
                result.Add($"{columnDate.Name} – Added Time Part : {columnTime.Name}");
              }
            }

            // Case b) TimeFormat has not been recognized (e.G. all values are 08:00) only look in adjacent fields
            for (var colindex = 0; colindex < fileReader.FieldCount; colindex++)
            {
              var columnDate = fileReader.GetColumn(colindex);
              if (columnDate.DataType != DataType.DateTime || !string.IsNullOrEmpty(columnDate.TimePart) ||
                columnDate.ValueFormat.DateFormat.IndexOfAny(new[] { ':', 'h', 'H', 'm', 's', 't' }) != -1)
              {
                continue;
              }

              if (colindex + 1 < fileReader.FieldCount)
              {
                var columnTime = fileReader.GetColumn(colindex + 1);
                if (columnTime.DataType == DataType.String && columnDate.Name.NoSpecials().ToUpperInvariant()
                   .Replace("DATE", string.Empty)
                   .Equals(columnTime.Name.NoSpecials().ToUpperInvariant().Replace("TIME", string.Empty),
                    StringComparison.OrdinalIgnoreCase))
                {
                  columnDate.TimePart = columnTime.Name;
                  {
                    var samples = GetSampleValues(fileReader, 1, colindex + 1, 1, fileSetting.TreatTextAsNull,
                      processDisplay.CancellationToken);
                    var first = samples.FirstOrDefault();
                    if (first != null)
                    {
                      if (first.Length == 8 || first.Length == 5)
                      {
                        columnTime.DataType = DataType.DateTime;
                        var val = new ValueFormat(DataType.DateTime)
                        {
                          DateFormat = first.Length == 8 ? "HH:mm:ss" : "HH:mm"
                        };
                        columnTime.ValueFormat = val;
                        fileSetting.ColumnAdd(columnTime);
                        result.Add($"{columnTime.Name} – Format : {columnTime.GetTypeAndFormatDescription()}");
                      }
                    }
                  }

                  result.Add($"{columnDate.Name} – Added Time Part : {columnTime.Name}");
                  continue;
                }
              }

              if (colindex <= 0) continue;
              {
                var columnTime = fileReader.GetColumn(colindex - 1);
                if (columnTime.DataType != DataType.String ||
                   !columnDate.Name.NoSpecials().ToUpperInvariant().Replace("DATE", string.Empty).Equals(columnTime.Name.NoSpecials().ToUpperInvariant().Replace("TIME", string.Empty), StringComparison.Ordinal))
                {
                  continue;
                }

                columnDate.TimePart = columnTime.Name;
                {
                  var samples = GetSampleValues(fileReader, 1, colindex - 1, 1, fileSetting.TreatTextAsNull,
                    processDisplay.CancellationToken);
                  var first = samples.FirstOrDefault();
                  if (first != null)
                  {
                    if (first.Length == 8 || first.Length == 5)
                    {
                      var val = new ValueFormat(DataType.DateTime)
                      {
                        DateFormat = first.Length == 8 ? "HH:mm:ss" : "HH:mm"
                      };
                      fileSetting.ColumnAdd(columnTime);
                      columnTime.ValueFormat = val;
                      result.Add($"{columnTime.Name} – Format : {columnTime.GetTypeAndFormatDescription()}");
                    }
                  }
                }
                result.Add($"{columnDate.Name} – Added Time Part : {columnTime.Name}");
              }
            }
          }

          // Sort the columns in fileSetting by order in file
          fileSetting.SortColumnByName(columnNamesInFile);
        }
      }
      finally
      {
        if (resetSkipRows)
          fileSetting.SkipRows = 0;
      }

      return result;
    }

    /// <summary>
    ///  Fills the Column Format for writer fileSettings
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="fileSettings">The file settings.</param>
    /// <param name="all">if set to <c>true</c> event string columns are added.</param>
    public static void FillGuessColumnFormatWriter(this IFileSetting fileSettings, bool all,
      CancellationToken cancellationToken)
    {
      var fileWriter = fileSettings.GetFileWriter(cancellationToken);
      if (string.IsNullOrEmpty(fileSettings.SqlStatement))
        throw new ApplicationException("No SQL Statement given");
      using (var dataReader = ApplicationSetting.SQLDataReader(fileSettings.SqlStatement, cancellationToken))
      {
        // Put the information into the list
        foreach (DataRow schemaRow in dataReader.GetSchemaTable().Rows)
        {
          var header = schemaRow[SchemaTableColumn.ColumnName].ToString();
          var colType = ((Type)schemaRow[SchemaTableColumn.DataType]).GetDataType();

          if (!all && (colType == DataType.String)) continue;
          var fsColumn = new Column
          {
            Name = header,
            DataType = colType
          };
          fileSettings.ColumnAdd(fsColumn);
        }
      }
    }

    /// <summary>
    /// Gets all possible formats based on the provided value
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns></returns>
    public static IEnumerable<ValueFormat> GetAllPossibleFormats(string value, CultureInfo culture = null)
    {
      if (culture == null)
        culture = CultureInfo.CurrentCulture;

      // Standard Date Time formats
      foreach (var fmt in StringConversion.StandardDateTimeFormats.MatchingforLength(value.Length, true))
      {
        foreach (var sep in StringConversion.DateSeparators)
        {
          if (StringConversion.StringToDateTimeExact(value, fmt, sep, culture.DateTimeFormat.TimeSeparator, culture).HasValue)
          {
            yield return new ValueFormat(DataType.DateTime)
            {
              DateFormat = fmt,
              DateSeparator = sep,
            };
          }
        }
      }
    }

    /// <summary>
    ///  Get sample values for a column
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="columnIndex">Index of the column.</param>
    /// <param name="enoughSamples">The enough samples.</param>
    /// <param name="treatAsNull">Text that should be regarded as an empty column</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A collection of distinct not null values</returns>
    public static IEnumerable<string> GetSampleValues(DataTable dataTable, int columnIndex,
      int enoughSamples, string treatAsNull, CancellationToken cancellationToken)
    {
      Contract.Requires(dataTable != null);
      Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

      if (string.IsNullOrEmpty(treatAsNull))
        treatAsNull = "NULL;n/a";

      var samples = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      try
      {
        foreach (DataRow row in dataTable.Rows)
        {
          if (cancellationToken.IsCancellationRequested)
            break;

          if (samples.Count >= enoughSamples)
            break;

          if (row[columnIndex] == DBNull.Value)
            continue;

          var value = row[columnIndex].ToString();
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

          samples.Add(value);
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.InnerExceptionMessages());
      }

      return SchuffleSamples(samples);
    }

    /// <summary>
    ///   Get sample values for a column
    /// </summary>
    /// <param name="dataReader">A <see cref="IFileReader" /> data reader</param>
    /// <param name="maxRecords">The maximum records.</param>
    /// <param name="columnIndex">Index of the column.</param>
    /// <param name="enoughSamples">The number samples.</param>
    /// <param name="treatAsNull">Text that should be regarded as an empty column</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>
    ///   A collection of distinct not null values
    /// </returns>
    public static IList<string> GetSampleValues(IFileReader dataReader, int maxRecords,
      int columnIndex, int enoughSamples, string treatAsNull, CancellationToken cancellationToken)
    {
      Contract.Requires(dataReader != null);
      Contract.Requires(columnIndex >= 0);
      Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);

      if (string.IsNullOrEmpty(treatAsNull))
        treatAsNull = "NULL;n/a";

      var hasWarning = false;
      var startedFormBeginning = false;

      var samples = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      try
      {
        // could already be at EOF need to reset
        if (dataReader.EndOfFile)
        {
          dataReader.ResetPositionToFirstDataRow();
          startedFormBeginning = true;
        }

        var recordNumber = 0;
        dataReader.Warning += delegate { hasWarning = true; };
        // Get distinct sample values
        while (recordNumber++ < maxRecords && samples.Count < enoughSamples &&
            !cancellationToken.IsCancellationRequested)
        {
          // if at the end start from the beginning
          if (!dataReader.Read() && dataReader.EndOfFile)
          {
            if (startedFormBeginning)
              break;
            dataReader.ResetPositionToFirstDataRow();
            startedFormBeginning = true;
            // If still at the end, we do not have a line
            if (!dataReader.Read())
              break;
          }

          // In case there was a warning reading the line, ignore the line
          if (!hasWarning)
          {
            var value = dataReader.GetString(columnIndex);

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
            samples.Add(value);
          }
          else
          {
            // Reset the warning for the next line
            hasWarning = false;
          }
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.InnerExceptionMessages());
      }
      finally
      {
        if (dataReader != null)
          dataReader.Warning -= delegate { hasWarning = true; };
      }

      return SchuffleSamples(samples);
    }

    /// <summary>
    ///   Gets the writer source columns.
    /// </summary>
    /// <param name="fileSettings">The file settings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public static IEnumerable<ColumnInfo> GetWriterSourceColumns(IFileSetting fileSettings,
      CancellationToken cancellationToken)
    {
      Contract.Requires(fileSettings != null);
      var writer = fileSettings.GetFileWriter(cancellationToken);
      return writer.GetColumnInformation(writer.GetSchemaReader());
    }

    public static CheckResult GuessDateTime(IList<string> samples, bool checkNamedDates,
      CancellationToken cancellationToken)
    {
      if (samples == null || samples.IsEmpty())
      {
        throw new ArgumentNullException(nameof(samples));
      }

      var checkResult = new CheckResult();
      var firstValue = samples[0];

      foreach (var fmt in StringConversion.StandardDateTimeFormats.MatchingforLength(firstValue.Length, checkNamedDates))
      {
        if (cancellationToken.IsCancellationRequested)
          return null;

        if (fmt.IndexOf('/') > 0)
        {
          foreach (var sep in StringConversion.DateSeparators)
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

    public static CheckResult GuessNumeric(IList<string> samples, bool guessPercentage,
      bool allowStartingZero, CancellationToken cancellationToken)
    {
      var checkResult = new CheckResult();

      var possibleGrouping = new List<char>();
      // Determine which decimalGrouping could be used
      foreach (var caracter in StringConversion.DecimalGroupings)
      {
        if (caracter == '\0')
          continue;
        foreach (var smp in samples)
        {
          if (smp.IndexOf(caracter) <= -1) continue;
          possibleGrouping.Add(caracter);
          break;
        }
      }

      possibleGrouping.Add('\0');
      var possibleDecimal = new List<char>();
      foreach (var caracter in StringConversion.DecimalSeparators)
      {
        if (caracter == '\0')
          continue;
        foreach (var smp in samples)
        {
          if (smp.IndexOf(caracter) <= -1) continue;
          possibleDecimal.Add(caracter);
          break;
        }
      }

      // Need to have at least one decimal separator
      if (possibleDecimal.Count == 0)
        possibleDecimal.Add('.');

      foreach (var thousandSeparator in possibleGrouping)
      {
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
    /// <returns><c>Null</c> if no format could be determined otherwise a <see cref="ValueFormat" /></returns>
    public static CheckResult GuessValueFormat(IList<string> samples, int minRequiredSamples,
      string trueValue, string falseValue, bool guessBoolean, bool guessGuid, bool guessNumeric, bool guessDateTime,
      bool guessPercentage, bool serialDateTime, bool checkNamedDates, ValueFormat othersValueFormatDate, CancellationToken cancellationToken)
    {
      Contract.Requires(samples != null);

      if (samples.IsEmpty())
        return null;

      var count = samples.Count();
      var checkResult = new CheckResult { FoundValueFormat = new ValueFormat() };

      // if it only one sample value and its false, assume its a boolean
      if (guessBoolean && count == 1 && !string.IsNullOrEmpty(falseValue))
      {
        foreach (var value in samples)
        {
          if (value.Equals(falseValue, StringComparison.OrdinalIgnoreCase))
          {
            checkResult.FoundValueFormat.DataType = DataType.Boolean;
            return checkResult;
          }

          break;
        }
      }

      if (cancellationToken.IsCancellationRequested)
        return null;

      // this could be a boolean
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

      if (guessGuid && StringConversion.CheckGuid(samples))
      {
        checkResult.FoundValueFormat.DataType = DataType.Guid;
        return checkResult;
      }

      if (cancellationToken.IsCancellationRequested)
        return null;

      // in case we have named dates, this is not feasible
      if (!checkNamedDates)
      {
        // Trying some chars, if they are in, assume its a string
        var valuesWithChars = 0;
        foreach (var value in samples)
        {
          // Not having AM PM or T as it might be part of a date Not having E in there as might be
          // part of a number u 1.487% o 6.264% n 2.365% i 6.286% h 7.232% s 6.327% This adds to a
          // 30% chance for each position in the text to determine if a text a regular text,
          if (value.IndexOfAny(new[] { 'u', 'U', 'o', 'O', 'i', 'I', 'n', 'N', 's', 'S', 'h', 'H' }) <= -1) continue;
          valuesWithChars++;
          // Only do so if more then half of the samples are string
          if (valuesWithChars < count / 2 && valuesWithChars < 10) continue;
          checkResult.FoundValueFormat.DataType = DataType.String;
          return checkResult;
        }
      }

      if (count < minRequiredSamples && guessDateTime && othersValueFormatDate != null)
      {
        var res = StringConversion.CheckDate(samples, othersValueFormatDate.DateFormat, othersValueFormatDate.DateSeparator, othersValueFormatDate.TimeSeparator, CultureInfo.CurrentCulture);
        if (res.FoundValueFormat != null)
          return res;
      }

      // if we have less than the required samples values do not try and try to get a type
      if (count < minRequiredSamples || cancellationToken.IsCancellationRequested)
        return null;

      var firstValue = samples.First();

      if (cancellationToken.IsCancellationRequested)
        return null;

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

      // assume dates are of the same format across the files we check if the dates
      // we have would possibly match no matter how many samples we have
      if (guessDateTime && othersValueFormatDate != null)
      {
        var res = StringConversion.CheckDate(samples, othersValueFormatDate.DateFormat, othersValueFormatDate.DateSeparator, othersValueFormatDate.TimeSeparator, CultureInfo.CurrentCulture);
        if (res.FoundValueFormat != null)
          return res;
      }

      if (cancellationToken.IsCancellationRequested)
        return null;

      if (guessNumeric)
      {
        var res = GuessNumeric(samples, guessPercentage, false, cancellationToken);
        if (res.FoundValueFormat != null)
          return res;
        checkResult.KeepBestPossibleMatch(res);
      }

      if (cancellationToken.IsCancellationRequested)
        return null;

      // Minimum length of a date is 4 characters
      if (guessDateTime && firstValue.Length > 3)
      {
        var res = GuessDateTime(samples, checkNamedDates, cancellationToken);
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

      {
        var res = StringConversion.CheckSerialDate(samples, false);
        if (res.FoundValueFormat != null)
          return res;
        checkResult.KeepBestPossibleMatch(res);
      }
      return checkResult;
    }

    private static IList<string> SchuffleSamples(IEnumerable<string> samples) => samples.OrderBy(x => SecureString.Random.Next()).ToList();
  }
}