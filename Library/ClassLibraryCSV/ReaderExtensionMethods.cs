using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public static class ReaderExtensionMethods
  {
    public static async Task<ICollection<string>> GetEmptyColumnHeaderAsync([NotNull] this IFileReader fileReader,
      CancellationToken cancellationToken)
    {
      if (fileReader == null) throw new ArgumentNullException(nameof(fileReader));
      var emptyColumns = new List<string>();

      var needToCheck = new List<int>(fileReader.FieldCount);
      for (var column = 0; column < fileReader.FieldCount; column++)
        needToCheck.Add(column);

      while (await fileReader.ReadAsync(cancellationToken).ConfigureAwait(false) && !cancellationToken.IsCancellationRequested && needToCheck.Count > 0)
      {
        var check = needToCheck.Where(col => !string.IsNullOrEmpty(fileReader.GetString(col))).ToList();
        foreach (var col in check) needToCheck.Remove(col);
      }

      for (var column = 0; column < fileReader.FieldCount; column++)
        if (needToCheck.Contains(column))
          emptyColumns.Add(fileReader.GetColumn(column).Name);

      return emptyColumns;
    }

    /// <summary>
    ///   Gets all not ignored columns from the reader, an ignored column is present in as columns
    ///   but should not be regarded
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static IEnumerable<IColumn> GetColumnsOfReader([NotNull] this IFileReader reader)
    {
      if (reader == null) throw new ArgumentNullException(nameof(reader));
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
    /// <param name="storeWarningsInDataTable">
    ///   if <c>true</c> Row and Column errors are created in the data table
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
    /// <param name="previewAction">Called after 500 records beeing read</param>
    /// <param name="progress">
    ///   Used to pass on progress information with number of records and percentage
    /// </param>
    /// <param name="cancellationToken">Token to cancel the long running async method</param>
    /// <returns></returns>
    public static async Task<DataTable> GetDataTableAsync([NotNull] this IFileReader reader, long recordLimit,
      bool storeWarningsInDataTable, bool addStartLine,
      bool includeRecordNo, bool includeEndLineNo, bool includeErrorField, Action<DataTable> previewAction,
      Action<long, int> progress,
      CancellationToken cancellationToken)
    {
      // Special handling for DataTableWrapper, no need to build something
      if (reader is DataTableWrapper dtWrapper)
        return await Task.FromResult(dtWrapper.DataTable);

      if (recordLimit < 1)
        recordLimit = long.MaxValue;

      var dataTable = new DataTable
      {
        Locale = CultureInfo.CurrentCulture,
        CaseSensitive = false
      };
      dataTable.BeginLoadData();
      try
      {
        var intervalAction = progress!=null ? new IntervalAction() : null;
        // Check if we need the wrapper, in case of new additional columns and not storing errors
        if (!storeWarningsInDataTable && !addStartLine && !includeRecordNo && !includeEndLineNo && !includeErrorField)
        {
          if (reader.IsClosed)
            await reader.OpenAsync(cancellationToken);

          var notIgnored = reader.GetColumnsOfReader().ToList();
          foreach (var column in notIgnored)
            dataTable.Columns.Add(new DataColumn(column.Name, column.ValueFormat.DataType.GetNetType()));

          var record = 0;
          while (record++<recordLimit && await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
          {
            var dataRow = dataTable.NewRow();
            dataTable.Rows.Add(dataRow);
            var i = 0;
            foreach (var column in notIgnored)
              dataRow[i++] = reader.GetValue(column.ColumnOrdinal);
            intervalAction?.Invoke(() => progress(reader.RecordNumber, reader.Percent));
            if (previewAction != null && dataTable.Rows.Count == 500)
            {
              var copy = dataTable.Copy();
              progress?.Invoke(reader.RecordNumber, reader.Percent);
#pragma warning disable 4014
              Task.Run(() => previewAction(copy), cancellationToken);
#pragma warning restore 4014
            }
          }
        }
        else
        {
          using (var wrapper = new DataReaderWrapper(reader, recordLimit, includeErrorField, addStartLine,
            includeEndLineNo, includeRecordNo))
          {
            await wrapper.OpenAsync(cancellationToken).ConfigureAwait(false);

            // create fields
            for (var colIndex = 0; colIndex < wrapper.FieldCount; colIndex++)
              dataTable.Columns.Add(new DataColumn(wrapper.GetName(colIndex), wrapper.GetFieldType(colIndex)));

            while (await wrapper.ReadAsync(cancellationToken).ConfigureAwait(false))
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

              intervalAction?.Invoke(() => progress(reader.RecordNumber, reader.Percent));
              if (previewAction != null && dataTable.Rows.Count == 500)
              {
                var copy = dataTable.Copy();
                progress?.Invoke(reader.RecordNumber, reader.Percent);
#pragma warning disable 4014
                Task.Run(() => previewAction(copy), cancellationToken);
#pragma warning restore 4014
              }

              if (!storeWarningsInDataTable || wrapper.ColumnErrorDictionary.Count <= 0 || cancellationToken.IsCancellationRequested)
                continue;

              foreach (var keyValuePair in wrapper.ColumnErrorDictionary)
                if (keyValuePair.Key == -1)
                  dataRow.RowError = keyValuePair.Value;
                else
                  dataRow.SetColumnError(wrapper.GetColumnIndexFromErrorColumn(keyValuePair.Key), keyValuePair.Value);
            }
          }
        }

        dataTable.EndLoadData();
        return dataTable;
      }
      catch
      {
        dataTable.Dispose();
        throw;
      }
    }
  }
}