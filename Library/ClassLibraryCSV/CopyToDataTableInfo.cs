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
using System.Linq;

namespace CsvTools
{
  public sealed class CopyToDataTableInfo : IDisposable
  {
    public readonly DataTable DataTable;
    private readonly DataColumn m_Error;
    private readonly BiDirectionalDictionary<int, int> m_Mapping = new BiDirectionalDictionary<int, int>();
    private readonly IList<string> m_ReaderColumns = new List<string>();
    private readonly DataColumn m_EndLine;
    private readonly DataColumn m_RecordNumber;
    private readonly DataColumn m_StartLine;
    private readonly bool m_IncludeErrorField;
    private readonly bool m_StoreWarningsInDataTable;
    private readonly ColumnErrorDictionary m_ColumnErrorDictionary;


    public CopyToDataTableInfo(IFileReader reader, bool includeErrorField, bool storeWarningsInDataTable, bool addStartLine)
    {
      DataTable = new DataTable
      {
        TableName = reader.FileSetting.ID,
        Locale = CultureInfo.CurrentCulture,
        CaseSensitive = false
      };
      m_ColumnErrorDictionary = new ColumnErrorDictionary(reader);

      m_IncludeErrorField = includeErrorField;
      m_StoreWarningsInDataTable = storeWarningsInDataTable;
      for (var col = 0; col < reader.FieldCount; col++)
      {
        if (reader.IgnoreRead(col)) continue;

        var colName = reader.GetName(col);
        m_ReaderColumns.Add(colName);
        DataTable.Columns.Add(new DataColumn(colName, reader.GetColumn(col).ValueFormat.DataType.GetNetType()));
        m_Mapping.Add(col, DataTable.Columns[colName].Ordinal);
      }

      if (addStartLine && !reader.HasColumnName(BaseFileReader.cStartLineNumberFieldName))
      {
        // Append Artificial columns This needs to happen in the same order as we have in
        // CreateTableFromReader otherwise BulkCopy does not work see SqlServerConnector.CreateTable
        m_StartLine = new DataColumn(BaseFileReader.cStartLineNumberFieldName, typeof(long));
        DataTable.Columns.Add(m_StartLine);
      }

      if (reader.FileSetting.DisplayRecordNo && !reader.HasColumnName(BaseFileReader.cRecordNumberFieldName))
      {
        m_RecordNumber = new DataColumn(BaseFileReader.cRecordNumberFieldName, typeof(long));
        DataTable.Columns.Add(m_RecordNumber);
        DataTable.PrimaryKey = new[] { m_RecordNumber };
      }

      if (reader.FileSetting.DisplayEndLineNo && !reader.HasColumnName(BaseFileReader.cEndLineNumberFieldName))
      {
        m_EndLine = new DataColumn(BaseFileReader.cEndLineNumberFieldName, typeof(long));
        DataTable.Columns.Add(m_EndLine);
      }

      if (includeErrorField && !reader.HasColumnName(BaseFileReader.cErrorField))
      {
        m_Error = new DataColumn(BaseFileReader.cErrorField, typeof(string));
        DataTable.Columns.Add(m_Error);
      }
    }
    /// <summary>
    /// Store the information from teh reader in the data table
    /// </summary>
    /// <param name="reader"></param>
    /// <returns><c>true</c> no warnings, <c>false</c> warnings have been raised</returns>
    public bool CopyRowToTable(IFileReader reader)
    {
      var dataRow = DataTable.NewRow();
      if (m_RecordNumber != null)
        dataRow[m_RecordNumber] = reader.RecordNumber;

      if (m_EndLine != null)
        dataRow[m_EndLine] = reader.EndLineNumber;

      if (m_StartLine != null)
        dataRow[m_StartLine] = reader.StartLineNumber;

      DataTable.Rows.Add(dataRow);

      foreach (var keyValuePair in m_Mapping)
        dataRow[keyValuePair.Value] = reader.GetValue(keyValuePair.Key);

      if (m_ColumnErrorDictionary.Count <= 0) return true;

      if (m_StoreWarningsInDataTable)
      {
        foreach (var keyValuePair in m_ColumnErrorDictionary)
          // Column Error
          if (keyValuePair.Key >= 0 && m_Mapping.TryGetValue(keyValuePair.Key, out var dbCol))
            dataRow.SetColumnError(dbCol, keyValuePair.Value);
          // Row Error
          else if (keyValuePair.Key == -1)
            dataRow.RowError = keyValuePair.Value;
          else if (keyValuePair.Key == -2)
          {
            var previousDataRow = DataTable.Rows[DataTable.Rows.Count - 1];
            previousDataRow.RowError = previousDataRow.RowError.AddMessage(keyValuePair.Value);
          }
      }

      if (m_IncludeErrorField)
      {
        dataRow[m_Error] = ErrorInformation.ReadErrorInformation(m_ColumnErrorDictionary, m_ReaderColumns);
        foreach (var prev in m_ColumnErrorDictionary.Where(x => x.Key == -2).Select(x => x.Value))
        {
          var previousDataRow = DataTable.Rows[DataTable.Rows.Count - 1];
          previousDataRow[m_Error] = previousDataRow[m_Error].ToString().AddMessage(prev);
        }
      }

      m_ColumnErrorDictionary.Clear();
      return false;
    }


    #region IDisposable Support

    private bool m_DisposedValue; // To detect redundant calls

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }

    private void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (disposing)
      {
        m_ReaderColumns.Clear();
        m_Mapping.Clear();
        DataTable?.Dispose();
      }

      m_DisposedValue = true;
    }

    #endregion IDisposable Support
  }
}