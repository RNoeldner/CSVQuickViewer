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
  public sealed class CopyToDataTableInfo : IDisposable
  {
    public readonly DataTable DataTable;
    public readonly DataColumn Error;
    public readonly BiDirectionalDictionary<int, int> Mapping = new BiDirectionalDictionary<int, int>();
    public readonly IList<string> ReaderColumns = new List<string>();
    private readonly DataColumn m_EndLine;
    private readonly DataColumn m_RecordNumber;
    private readonly DataColumn m_StartLine;

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
        ReaderColumns.Add(colName);
        DataTable.Columns.Add(new DataColumn(colName, reader.GetColumn(col).ValueFormat.DataType.GetNetType()));
        Mapping.Add(col, DataTable.Columns[colName].Ordinal);
      }

      if (reader.FileSetting.DisplayStartLineNo && !reader.HasColumnName(BaseFileReader.cRecordNumberFieldName))
      {
        // Append Artificial columns This needs to happen in the same order as we have in
        // CreateTableFromReader otherwise BulkCopy does not work see SqlServerConnector.CreateTable
        m_StartLine = new DataColumn(BaseFileReader.cStartLineNumberFieldName, typeof(long));
        DataTable.Columns.Add(m_StartLine);
        DataTable.PrimaryKey = new[] { m_StartLine };
      }

      if (reader.FileSetting.DisplayRecordNo && !reader.HasColumnName(BaseFileReader.cRecordNumberFieldName))
      {
        m_RecordNumber = new DataColumn(BaseFileReader.cRecordNumberFieldName, typeof(long));
        DataTable.Columns.Add(m_RecordNumber);
      }

      if (reader.FileSetting.DisplayEndLineNo && !reader.HasColumnName(BaseFileReader.cEndLineNumberFieldName))
      {
        m_EndLine = new DataColumn(BaseFileReader.cEndLineNumberFieldName, typeof(long));
        DataTable.Columns.Add(m_EndLine);
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
      if (m_RecordNumber != null)
        dataRow[m_RecordNumber] = m_RecordNumber;

      if (m_EndLine != null)
        dataRow[m_EndLine] = reader.EndLineNumber;

      if (m_StartLine != null)
        dataRow[m_StartLine] = reader.StartLineNumber;

      DataTable.Rows.Add(dataRow);

      foreach (var keyValuePair in Mapping)
        dataRow[keyValuePair.Value] = reader.GetValue(keyValuePair.Key);

      return dataRow;
    }

 
    #region IDisposable Support

    private bool m_DisposedValue; // To detect redundant calls

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above. GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (disposing)
      {
        ReaderColumns.Clear();
        Mapping.Clear();
        DataTable?.Dispose();
      }
      m_DisposedValue = true;
    }

    #endregion IDisposable Support
  }
}