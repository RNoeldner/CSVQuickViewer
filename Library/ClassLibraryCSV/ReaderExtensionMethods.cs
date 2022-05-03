using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
    /// <param name="recordLimit">
    ///   Number of records to return, the reader might already have a limit
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
    /// <returns></returns>
    public static async Task<DataTable?> GetDataTableAsync(
      this IFileReader reader,      
      bool restoreErrorsFromColumn,
      bool addStartLine,
      bool includeRecordNo,
      bool includeEndLineNo,
      bool includeErrorField,
      IProcessDisplay? progress,
      CancellationToken cancellationToken)
    {
      if (reader is DataTableWrapper dtw)
        return dtw.DataTable;
      
      if (reader.IsClosed)
        await reader.OpenAsync(cancellationToken).ConfigureAwait(false);
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
      await
#endif
      using var wrapper = new DataReaderWrapper(
        reader,        
        includeErrorField,
        addStartLine,
        includeEndLineNo,
        includeRecordNo);
      
      return await LoadDataTable(wrapper, TimeSpan.MaxValue, restoreErrorsFromColumn, progress, cancellationToken)
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

    public static async Task<DataTable> LoadDataTable(
      this DataReaderWrapper wrapper,
      TimeSpan maxDuration,
      bool restoreErrorsFromColumn,
      IProcessDisplay? processDisplay,
      CancellationToken cancellationToken)
    {
      var dataTable = GetEmptyDataTable(wrapper);
      if (wrapper.EndOfFile) return dataTable;

      var intervalAction = processDisplay != null ? new IntervalAction() : null;
      try
      {
        var errorColumn = restoreErrorsFromColumn ? dataTable.Columns[ReaderConstants.cErrorField] : null;

        var watch = Stopwatch.StartNew();
        while (!cancellationToken.IsCancellationRequested && (watch.Elapsed < maxDuration || wrapper.Percent >= 90)
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
          intervalAction?.Invoke(processDisplay, $"Record {wrapper.RecordNumber:N0}", wrapper.Percent, false);

          // This gets the errors from the fileReader
          if (cancellationToken.IsCancellationRequested)
            break;
          if (wrapper.ReaderMapping.HasErrors)
            wrapper.ReaderMapping.SetDataRowErrors(dataRow);
        }
      }      
      finally
      {
        intervalAction?.Invoke(processDisplay, $"Record {wrapper.RecordNumber:N0}", wrapper.Percent, false);
      }

      return dataTable;
    }

    public static DataTable GetEmptyDataTable(this IDataRecord reader)
    {
      // Special handling for DataTableWrapper, no need to build something
      var dataTable = new DataTable { Locale = CultureInfo.CurrentCulture, CaseSensitive = false };

      for (var colIndex = 0; colIndex < reader.FieldCount; colIndex++)
        dataTable.Columns.Add(new DataColumn(reader.GetName(colIndex), reader.GetFieldType(colIndex)));
      return dataTable;
    }
  }
}