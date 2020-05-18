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
using System.Globalization;

namespace CsvTools
{
  public class CopyToDataTableInfo : IDisposable
  {
    public readonly DataTable DataTable;
    public readonly DataColumn Error;
    public readonly BiDirectionalDictionary<int, int> Mapping = new BiDirectionalDictionary<int, int>();
    public readonly IList<string> ReaderColumns = new List<string>();
    private readonly DataColumn EndLine;
    private readonly DataColumn RecordNumber;
    private readonly DataColumn StartLine;

    public CopyToDataTableInfo(IFileReader reader, bool includeErrorField)
    {
      DataTable = new DataTable()
      {
        TableName = reader.FileSetting.ID,
        Locale = CultureInfo.CurrentCulture,
        CaseSensitive = false
      };

      for (var col = 0; col < reader.FieldCount; col++)
      // Initialize a based on file reader
      {
        var colName = reader.GetName(col);
        var vf = reader.GetColumn(col).ValueFormat;
        if (includeErrorField)
          ReaderColumns.Add(colName);
        if (colName.Equals(BaseFileReader.cStartLineNumberFieldName, StringComparison.OrdinalIgnoreCase))
          continue;
        DataTable.Columns.Add(new DataColumn(colName, vf.DataType.GetNetType()));
        Mapping.Add(col, DataTable.Columns[colName].Ordinal);
      }

      // Append Artificial columns This needs to happen in the same order as we have in
      // CreateTableFromReader otherwise BulkCopy does not work see SqlServerConnector.CreateTable
      StartLine = new DataColumn(BaseFileReader.cStartLineNumberFieldName, typeof(long));
      DataTable.Columns.Add(StartLine);

      DataTable.PrimaryKey = new[] { StartLine };

      if (reader.FileSetting.DisplayRecordNo && !reader.HasColumnName(BaseFileReader.cRecordNumberFieldName))
      {
        RecordNumber = new DataColumn(BaseFileReader.cRecordNumberFieldName, typeof(long));
        DataTable.Columns.Add(RecordNumber);
      }

      if (reader.FileSetting.DisplayEndLineNo && !reader.HasColumnName(BaseFileReader.cEndLineNumberFieldName))
      {
        EndLine = new DataColumn(BaseFileReader.cEndLineNumberFieldName, typeof(long));
        DataTable.Columns.Add(EndLine);
      }

      if (includeErrorField && !reader.HasColumnName(BaseFileReader.cErrorField))
      {
        Error = new DataColumn(BaseFileReader.cErrorField, typeof(string));
        DataTable.Columns.Add(Error);
      }
    }

    public DataRow CopyRowToTable(IFileReader reader)
    {
      var dataRow = DataTable.NewRow();
      if (RecordNumber != null)
        dataRow[RecordNumber] = RecordNumber;

      if (EndLine != null)
        dataRow[EndLine] = reader.EndLineNumber;

      if (StartLine != null)
        dataRow[StartLine] = reader.StartLineNumber;

      DataTable.Rows.Add(dataRow);

      foreach (var keyValuePair in Mapping)
        dataRow[keyValuePair.Value] = reader.GetValue(keyValuePair.Key);

      return dataRow;
    }

    public void StoreError(DataRow row, IDictionary<int, string> columnWarningsReader)
    {
      if (Error != null && ReaderColumns.Count > 0)
        row[Error] = ErrorInformation.ReadErrorInformation(columnWarningsReader, ReaderColumns);
    }

    #region IDisposable Support

    private bool disposedValue = false; // To detect redundant calls

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above. GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          ReaderColumns.Clear();
          Mapping.Clear();
          DataTable?.Dispose();
        }
        disposedValue = true;
      }
    }

    #endregion IDisposable Support
  }
}