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
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class ReaderExtensionMethods
  {
    public static string GetCombinedKey(this IDataReader dataReader, ICollection<int>? columns, char combineWith, Action<int>? trimming = null)
    {
      if (columns is null || columns.Count == 0)
        return string.Empty;

      var stringBuilder = new StringBuilder();
      foreach (var columnNo in columns)
      {
        if (!dataReader.IsDBNull(columnNo))
        {
          var currentValue = dataReader.GetValue(columnNo).ToString().Replace(combineWith, (char) 0);
          var trimmed = currentValue.Trim();
          if (trimmed.Length != currentValue.Length)
            trimming?.Invoke(columnNo);

          stringBuilder.Append(trimmed);
        }
        stringBuilder.Append(combineWith);
      }

      return stringBuilder.ToString();
    }

    /// <summary>
    ///   Gets all not ignored columns from the reader, an ignored column is present in as columns
    ///   but should not be regarded
    /// </summary>
    /// <param name="reader">A file reader</param>
    /// <returns></returns>
    public static IEnumerable<IColumn> GetColumnsOfReader(this IFileReader reader)
    {
      if (reader is null) throw new ArgumentNullException(nameof(reader));
      var retList = new List<IColumn>();
      for (var col = 0; col < reader.FieldCount; col++)
      {
        var column = reader.GetColumn(col);
        if (!column.Ignore)
          retList.Add(column);
      }

      return retList;
    }

    /// <summary>
    ///   Stores all rows from te reader into a DataTable, form the current position of the reader onwards.
    /// </summary>
    /// <param name="reader">
    ///   Any type of <see cref="IFileReader" />, if the source is a DataTableWrapper though the
    ///   original passed in data table is returned, no artificial columns are added
    /// </param>
    /// <param name="maxDuration">
    ///   Timeout duration for reading data, if the reader is slow or it has many rows make sure the
    ///   timespan is big enough, otherwise teh result is cut off
    /// </param>
    /// <param name="restoreErrorsFromColumn">
    ///   if the source is a persisted table, restore the error information
    /// </param>
    /// <param name="addStartLine">
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
      bool addStartLine,
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

#if NETSTANDARD2_1_OR_GREATER
      await
#endif
      using var wrapper = new DataReaderWrapper(
        reader,
        includeErrorField,
        addStartLine,
        includeEndLineNo,
        includeRecordNo);

      return await GetDataTableAsync(wrapper, maxDuration, restoreErrorsFromColumn, progress, cancellationToken)
        .ConfigureAwait(false);
    }

    /// <summary>
    ///   Stores all rows from te reader into a DataTable, form the current position of the reader onwards.
    /// </summary>
    /// <param name="reader">
    ///   Any type of <see cref="IFileReader" />, if the source is a DataTableWrapper though the
    ///   original passed in data table is returned, no artificial columns are added
    /// </param>
    /// <param name="maxDuration">
    ///   Timeout duration for reading data, if the reader is slow or it has many rows make sure the
    ///   timespan is big enough, otherwise teh result is cut off
    /// </param>
    /// <param name="restoreErrorsFromColumn">
    ///   if the source is a persisted table, restore the error information
    /// </param>
    /// <param name="addStartLine">
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
    /// <param name="cancellationToken">Token to cancel the long running async method</param>
    /// <returns>A Data Table with all records from the reader</returns>
    /// <remarks>In case the reader was not opened before it will be opened automatically</remarks>
    public static DataTable GetDataTable(this IFileReader reader,
      TimeSpan maxDuration,
      bool restoreErrorsFromColumn,
      bool addStartLine,
      bool includeRecordNo,
      bool includeEndLineNo,
      bool includeErrorField,
      CancellationToken cancellationToken)
    {
      if (reader is DataTableWrapper dtw)
        return dtw.DataTable;

      using var wrapper = new DataReaderWrapper(
        reader,
        includeErrorField,
        addStartLine,
        includeEndLineNo,
        includeRecordNo);

      return GetDataTable(wrapper, maxDuration, restoreErrorsFromColumn, cancellationToken);
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
        var errorColumn = restoreErrorsFromColumn ? dataTable.Columns[ReaderConstants.cErrorField] : null;

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
            dataRow.SetErrorInformation(dataRow[errorColumn].ToString());
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

    /// <summary>
    ///   Loads the data from a wrapper into a DataTable, limited by time span
    /// </summary>
    /// <param name="wrapper">a DataReaderWrapper that helps with the mapping</param>
    /// <param name="maxDuration">The timespan to stop reading</param>
    /// <param name="restoreErrorsFromColumn">Set <c>true</c> if errors in the error column should be restored as column or row errors</param>
    /// <param name="cancellationToken">Token to cancel the long running async method</param>
    /// <returns>A data table with the rows and columns from the reader</returns>
    public static DataTable GetDataTable(
     this DataReaderWrapper wrapper,
     TimeSpan maxDuration,
     bool restoreErrorsFromColumn,
     CancellationToken cancellationToken)
    {
      var dataTable = new DataTable { Locale = CultureInfo.CurrentCulture, CaseSensitive = false };
      for (var colIndex = 0; colIndex < wrapper.FieldCount; colIndex++)
        dataTable.Columns.Add(new DataColumn(wrapper.GetName(colIndex), wrapper.GetFieldType(colIndex)));

      if (wrapper.EndOfFile)
        return dataTable;

      var errorColumn = restoreErrorsFromColumn ? dataTable.Columns[ReaderConstants.cErrorField] : null;

      var watch = Stopwatch.StartNew();
      while (!cancellationToken.IsCancellationRequested && watch.Elapsed < maxDuration && wrapper.Read())
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
          dataRow.SetErrorInformation(dataRow[errorColumn].ToString());

        // This gets the errors from the fileReader
        if (wrapper.ReaderMapping.HasErrors)
          wrapper.ReaderMapping.SetDataRowErrors(dataRow);
      }

      return dataTable;
    }
  }
}