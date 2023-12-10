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
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

#if !QUICK
using System.Text;
#endif

namespace CsvTools
{
  public static class ReaderExtensionMethods
  {
    /// <summary>
    ///   Gets all not ignored columns from the reader, an ignored column is present in as columns
    ///   but should not be regarded
    /// </summary>
    /// <param name="reader">A file reader</param>
    /// <returns></returns>
    public static IEnumerable<Column> GetColumnsOfReader(this IFileReader reader)
    {
      if (reader is null) throw new ArgumentNullException(nameof(reader));
      var retList = new List<Column>();
      for (var col = 0; col < reader.FieldCount; col++)
      {
        var column = reader.GetColumn(col);
        if (!column.Ignore)
          retList.Add(column);
      }

      return retList;
    }

#if !QUICK
    /// <summary>
    /// Gets a reader for a source that reads everything as text columns, 
    /// e.g. for usage in ColumnDetection like <see cref="DetermineColumnFormat.GetSampleValuesAsync"/>
    /// </summary>
    /// <param name="source">The initial source setting </param>
    /// <param name="cancellationToken">Token to cancel the long running async method</param>
    /// <returns>The IFileReader to read the data as text</returns>   
    /// <note>Used for ColumnDetection like <see cref="DetermineColumnFormat.GetSampleValuesAsync"/></note>
    public static async Task<IFileReader> GetUntypedFileReaderAsync(this IFileSetting source, CancellationToken cancellationToken)
    {
      var fileSettingCopy = (IFileSetting) source.Clone();
      fileSettingCopy.ColumnCollection.Clear();

      // Make sure that if we do have a CSV file without header that we will skip the first row
      // that might contain headers, but it's simply set as without headers.
      if (fileSettingCopy is ICsvFile csv)
      {
        if (!csv.HasFieldHeader)
          csv.SkipRows++;
        // turn off all warnings as they will cause GetSampleValues to ignore the row
        csv.TryToSolveMoreColumns = false;
        csv.WarnDelimiterInValue = false;
        csv.WarnLineFeed = false;
        csv.WarnQuotes = false;
        csv.WarnUnknownCharacter = false;
        csv.WarnNBSP = false;
        csv.WarnQuotesInQuotes = false;
        csv.WarnEmptyTailingColumns= false;
      }
      var reader = FunctionalDI.FileReaderWriterFactory.GetFileReader(fileSettingCopy, cancellationToken);
      await reader.OpenAsync(cancellationToken).ConfigureAwait(false);
      return reader;
    }

    public static async Task<IEnumerable<Column>> GetAllReaderColumnsAsync(this IFileSetting source, CancellationToken cancellationToken)
    {
      var res = new List<Column>();
#if NET5_0_OR_GREATER
      await
#endif
  using var fileReader = FunctionalDI.FileReaderWriterFactory.GetFileReader(source, cancellationToken);
      await fileReader.OpenAsync(cancellationToken).ConfigureAwait(false);
      for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
        res.Add(fileReader.GetColumn(colIndex));
      return res;
    }

    /// <summary>
    /// Gets a text representing all columns of a reader in a way that its comparable 
    /// </summary>
    /// <param name="dataReader">The dataReader / row with the columns</param>
    /// <param name="columns">A collection of columns indexes to combine</param>
    /// <param name="combineWith">A separator for the columns contend</param>
    /// <param name="trimming">The columns will be trimmed, if trimming happens, this action is to be performed</param>
    /// <returns>An upper case text representation</returns>
    public static string GetCombinedKey(this IDataReader dataReader, IReadOnlyCollection<int>? columns, char combineWith, Action<int>? trimming = null)
    {
      if (columns is null || columns.Count == 0)
        return string.Empty;

      var stringBuilder = new StringBuilder();
      foreach (var columnNo in columns)
      {
        if (!dataReader.IsDBNull(columnNo))
        {
          var currentValue = dataReader.GetValue(columnNo).ToString()!.Replace(combineWith, (char) 0);
          var trimmed = currentValue.Trim();
          if (trimmed.Length != currentValue.Length && trimming != null)
            try
            {
              trimming.Invoke(columnNo);
            }
            catch (Exception ex)
            {
              Logger.Warning(ex, "Get Combined Keys Trimming Notification");
            }


          stringBuilder.Append(trimmed);
        }
        stringBuilder.Append(combineWith);
      }

      return stringBuilder.ToString().ToUpperInvariant();
    }



    /// <summary>
    ///   Stores all rows from te reader into a DataTable, form the current position of the reader onwards.
    /// </summary>
    /// <param name="reader">
    ///   Any type of <see cref="IFileReader" />, if the source is a DataTableWrapper though the
    ///   original passed in data table is returned, no artificial columns are added
    /// </param>
    /// <param name="maxDuration">
    ///   Timeout duration for reading data, if the reader is slow, or it has many rows make sure the
    ///   timespan is big enough, otherwise teh result is cut off
    /// </param>
    /// <param name="restoreErrorsFromColumn">
    ///   if the source is a persisted table, restore the error information
    /// </param>
    /// <param name="includeStartLine">
    ///   if <c>true</c> add a column for the start line: <see
    ///   cref="ReaderConstants.cStartLineNumberFieldName" /> useful for line based reader like
    ///   delimited text
    /// </param>
    /// <param name="includeRecordNo">
    ///   if <c>true</c> add a column for the records number: <see
    ///   cref="ReaderConstants.cRecordNumberFieldName" /> (if the reader was not at the beginning
    ///   it will it will not start with 1)
    /// </param>
    /// <param name="includeEndLineNo">
    ///   if <c>true</c> add a column for the end line: <see
    ///   cref="ReaderConstants.cEndLineNumberFieldName" /> useful for line based reader like
    ///   delimited text where a record can span multiple lines
    /// </param>
    /// <param name="includeErrorField">
    ///   if <c>true</c> add a column with error information: <see
    ///   cref="ReaderConstants.cErrorField" />
    /// </param>
    /// <param name="progress">
    ///   Used to pass on progress information with number of records and percentage
    /// </param>
    /// <param name="cancellationToken">Token to cancel the long running async method</param>
    /// <returns>A Data Table with all records from the reader</returns>
    /// <remarks>In case the reader was not opened before it will be opened automatically</remarks>
    public static async Task<DataTable> GetDataTableAsync(this IFileReader reader,
      TimeSpan maxDuration,
      bool restoreErrorsFromColumn,
      bool includeStartLine,
      bool includeRecordNo,
      bool includeEndLineNo,
      bool includeErrorField,
      IProgress<ProgressInfo>? progress,
      CancellationToken cancellationToken)
    {
      if (reader is DataTableWrapper dtw)
        return dtw.DataTable;

      if (reader.IsClosed)
        await reader.OpenAsync(cancellationToken).ConfigureAwait(false);

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var wrapper = new DataReaderWrapper(
        reader,
        includeStartLine,
        includeEndLineNo,
        includeRecordNo,
        includeErrorField);

      return await wrapper.GetDataTableAsync(maxDuration, restoreErrorsFromColumn, progress, cancellationToken)
        .ConfigureAwait(false);
    }

    public static async Task<ICollection<string>> GetEmptyColumnHeaderAsync(
      this IFileReader fileReader,
      CancellationToken cancellationToken)
    {
      if (fileReader is null) throw new ArgumentNullException(nameof(fileReader));
      var emptyColumns = new List<string>();

      var needToCheck = new List<int>(fileReader.FieldCount);
      for (var column = 0; column < fileReader.FieldCount; column++)
        needToCheck.Add(column);

      while (await fileReader.ReadAsync(cancellationToken).ConfigureAwait(false)
             && !cancellationToken.IsCancellationRequested && needToCheck.Count > 0)
      {
        var check = needToCheck.Where(col => !string.IsNullOrEmpty(fileReader.GetString(col))).ToList();
        foreach (var col in check) needToCheck.Remove(col);
      }

      for (var column = 0; column < fileReader.FieldCount; column++)
        if (needToCheck.Contains(column))
          emptyColumns.Add(fileReader.GetColumn(column).Name);

      return emptyColumns;
    }
#endif
    public static async Task<DataTable> GetDataTableAsync(
      this DataReaderWrapper wrapper,
      TimeSpan maxDuration,
      bool restoreErrorsFromColumn,
      IProgress<ProgressInfo>? progress,
      CancellationToken cancellationToken)
    {
      var dataTable = new DataTable { Locale = CultureInfo.CurrentCulture, CaseSensitive = false };
      for (var colIndex = 0; colIndex < wrapper.FieldCount; colIndex++)
        dataTable.Columns.Add(new DataColumn(wrapper.GetName(colIndex), wrapper.GetFieldType(colIndex)));

      if (wrapper.EndOfFile)
        return dataTable;

      var intervalAction = IntervalAction.ForProgress(progress);

      try
      {
        DataColumn? errorColumn = null;
        var onlyColumnErrors = false;
        if (restoreErrorsFromColumn)
        {
          var cols = dataTable.Columns.OfType<DataColumn>().ToList();

          errorColumn =
            cols.FirstOrDefault(col => col.ColumnName.Equals(ReaderConstants.cErrorField))
            ?? cols.FirstOrDefault(col => col.ColumnName.EndsWith("Errors", StringComparison.InvariantCultureIgnoreCase))
            ?? cols.FirstOrDefault(col => col.ColumnName.StartsWith("Error", StringComparison.InvariantCultureIgnoreCase));

          if (errorColumn != null)
            onlyColumnErrors = errorColumn.ColumnName.Equals(ReaderConstants.cErrorField);
        }

        if (maxDuration < TimeSpan.MaxValue)
          Logger.Debug("Reading batch (Limit {durationInitial:F1}s)", maxDuration.TotalSeconds);
        else
          Logger.Debug("Reading all data");

        var watch = Stopwatch.StartNew();
        while (!cancellationToken.IsCancellationRequested && (watch.Elapsed < maxDuration || wrapper.Percent >= 95)
                                                 && await wrapper.ReadAsync(cancellationToken)
                                                   .ConfigureAwait(false))
        {
          var dataRow = dataTable.NewRow();
          dataTable.Rows.Add(dataRow);
          for (var i = 0; i < wrapper.FieldCount; i++)
            try
            {
              dataRow[i] = wrapper.GetValue(i);
            }
            catch (Exception ex)
            {
              dataRow.SetColumnError(i, ex.Message);
            }

          // This gets the errors from the column #Error that has been filled by the reader
          if (errorColumn != null)
          {
            var text = dataRow[errorColumn].ToString();
            if (!string.IsNullOrEmpty(text))
              dataRow.SetErrorInformation(text, onlyColumnErrors);
          }

          intervalAction?.Invoke(progress!, $"Record {wrapper.RecordNumber:N0}", wrapper.Percent);

          // This gets the errors from the fileReader
          if (wrapper.ReaderMapping.HasErrors)
            wrapper.ReaderMapping.SetDataRowErrors(dataRow);
        }
      }
      finally
      {
        progress?.Report(new ProgressInfo($"Record {wrapper.RecordNumber:N0}", wrapper.Percent));
      }
      return dataTable;
    }
  }
}