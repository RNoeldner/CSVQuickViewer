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
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace CsvTools
{
  /// <summary>
  /// Extension methods for <see cref="IDataReader"/> and <see cref="DataReaderWrapper"/>
  /// </summary>
  public static class ReaderExtensionMethods
  {
    /// <summary>
    ///   Gets all not ignored columns from the reader, an ignored column is present in as columns
    ///   but should not be regarded
    /// </summary>
    /// <param name="reader">A file reader or any data reader</param>
    /// <returns>A List of all column, be aware that the format in ValueFormat of might be wrong, if is passed in as IDataReader</returns>
    public static IEnumerable<Column> GetColumnsOfReader(this IDataReader reader)
    {
      if (reader is null)
        throw new ArgumentNullException(nameof(reader));

      var retList = new List<Column>();
      // IFile Reader, supports Ignore
      if (reader is IFileReader fileReader)
      {
        for (var col = 0; col < reader.FieldCount; col++)
        {
          var column = fileReader.GetColumn(col);
          if (!column.Ignore)
            retList.Add(column);
        }
      }
      else
        // IData Reader, any column
      {
        for (var col = 0; col < reader.FieldCount; col++)
        {
          retList.Add(new Column(reader.GetName(col), new ValueFormat(reader.GetFieldType(col).GetDataType()), col));
        }
      }

      return retList;
    }

    /// <summary>
    /// Gets a reader for a source that reads everything as text columns, 
    /// e.g. for usage in ColumnDetection like <see cref="DetermineColumnFormat.GetSampleValuesAsync"/>
    /// </summary>
    /// <param name="source">The initial source setting </param>
    /// <param name="cancellationToken">Token to cancel the long running async method</param>
    /// <returns>The IFileReader to read the data as text</returns>   
    /// <note>Used for ColumnDetection like <see cref="DetermineColumnFormat.GetSampleValuesAsync"/></note>
    public static async Task<IFileReader> GetUntypedFileReaderAsync(this IFileSetting source,
      CancellationToken cancellationToken)
    {
      var fileSettingCopy = (IFileSetting) source.Clone();
      // No column should be type converted 
      fileSettingCopy.ColumnCollection.Clear();

      // Make sure that if we do have a CSV file without header that we will skip the first row
      // that might contain headers, but it's simply set as without headers.
      if (fileSettingCopy is ICsvFile csv)
      {
        // if we do not have a header still ignore the first row, it could be a header that was just not marked
        // Downside is that this does not work with a file hat only has 1 row...
        if (!csv.HasFieldHeader)
          csv.SkipRows++;
        // turn off all warnings as they will cause GetSampleValues to ignore the row        
        csv.WarnDelimiterInValue = false;
        csv.WarnLineFeed = false;
        csv.WarnQuotes = false;
        csv.WarnUnknownCharacter = false;
        csv.WarnNBSP = false;
        csv.WarnQuotesInQuotes = false;
        csv.WarnEmptyTailingColumns = false;

        // Adjusting columns in row only works well if columns are typed
        csv.AllowRowCombining = false;
        csv.TryToSolveMoreColumns = false;
      }

      var reader = FunctionalDI.FileReaderWriterFactory.GetFileReader(fileSettingCopy, cancellationToken);
      await reader.OpenAsync(cancellationToken).ConfigureAwait(false);
      return reader;
    }

    /// <summary>
    /// Gets all reader columns asynchronous.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="cancellationToken">The cancellation token.</param>    
    public static async Task<IEnumerable<Column>> GetAllReaderColumnsAsync(this IFileSetting source,
      CancellationToken cancellationToken)
    {
      var res = new List<Column>();
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
        using var fileReader = FunctionalDI.FileReaderWriterFactory.GetFileReader(source, cancellationToken);
      await fileReader.OpenAsync(cancellationToken).ConfigureAwait(false);
      for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
        res.Add(fileReader.GetColumn(colIndex));
      return res;
    }

    /// <summary>
    ///   Stores all rows from the reader into a DataTable, form the current position of the reader onwards.
    /// </summary>
    /// <param name="reader">
    ///   Any type of <see cref="IFileReader" />, if the source is a DataTableWrapper though the
    ///   original passed in data table is returned, no artificial columns are added
    /// </param>
    /// <param name="maxDuration">
    ///   Timeout duration for reading data, if the reader is slow, or it has many rows make sure the
    ///   timespan is big enough, otherwise the result is cut off
    /// </param>
    /// <param name="startLine">
    ///   if <c>true</c> add a column for the start line: <see cref="ReaderConstants.cStartLineNumberFieldName" /> useful for line based reader like
    ///   delimited text
    /// </param>
    /// <param name="endLine">
    ///   if <c>true</c> add a column for the end line: <see cref="ReaderConstants.cEndLineNumberFieldName" /> useful for line based reader like
    ///   delimited text where a record can span multiple lines
    /// </param>
    /// <param name="recNum">
    ///   if <c>true</c> add a column for the records number: <see cref="ReaderConstants.cRecordNumberFieldName" /> (if the reader was not at the beginning
    ///   it will not start with 1)
    /// </param>
    /// <param name="errorField">
    ///   if <c>true</c> add a column with error information: <see cref="ReaderConstants.cErrorField" />
    /// </param>
    /// <param name="progress">
    ///   Used to pass on progress information with number of records and percentage
    /// </param>
    /// <param name="cancellationToken">Token to cancel the long running async method</param>
    /// 
    /// <returns>A Data Table with all records from the reader</returns>    
    public static Task<DataTable> GetDataTableAsync(this IDataReader reader, TimeSpan maxDuration,
      bool startLine, bool endLine, bool recNum, bool errorField, IProgress<ProgressInfo>? progress,
      CancellationToken cancellationToken)
      => new DataReaderWrapper(reader, startLine, endLine, recNum, errorField).GetDataTableAsync(maxDuration, progress,
        cancellationToken);


    /// <summary>
    /// Reads the data from a <see cref="DataReaderWrapper"/> into a DataTable, handling artificial fields and errors
    /// </summary>
    /// <param name="wrapper"></param>
    /// <param name="maxDuration">Initial Duration for first return</param>    
    /// <param name="progress">Used to pass on progress information with number of records and percentage</param>
    /// <param name="cancellationToken">Token to cancel the long running async method</param>    
    public static async Task<DataTable> GetDataTableAsync(this DataReaderWrapper wrapper, TimeSpan maxDuration,
      IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      // Shortcut if the wrapper is a DataTableWrapper
      if (wrapper is DataTableWrapper dtw)
        return dtw.DataTable;

      var dataTable = new DataTable { Locale = CultureInfo.CurrentCulture, CaseSensitive = false };
      for (var colIndex = 0; colIndex < wrapper.FieldCount; colIndex++)
        dataTable.Columns.Add(new DataColumn(wrapper.GetName(colIndex), wrapper.GetFieldType(colIndex)));

      if (wrapper.EndOfFile)
        return dataTable;

      var intervalAction = IntervalAction.ForProgress(progress);

      try
      {
        try {
        if (maxDuration < TimeSpan.MaxValue)
          Logger.Debug("Reading batch (Limit {durationInitial:F1}s)", maxDuration.TotalSeconds);
        else
          Logger.Debug("Reading all data");
        } catch { }
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

          intervalAction?.Invoke(progress!, $"Record {wrapper.RecordNumber:N0}", wrapper.Percent);
          dataRow.SetErrorInformation(wrapper.RowErrorInformation);
        }
      }
      catch (Exception e)
      {
        Logger.Warning(e, "GetDataTableAsync error");
      }
      finally
      {
        progress?.Report(new ProgressInfo($"Record {wrapper.RecordNumber:N0}", wrapper.Percent));
      }

      return dataTable;
    }
  }
}
