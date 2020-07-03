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
using JetBrains.Annotations;

namespace CsvTools
{
  public sealed class CopyToDataTableInfo
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

    public CopyToDataTableInfo([NotNull] IFileReader reader, string tableName, bool addErrorField, bool addRecordNo,
      bool addStartLine, bool addEndLineNo, bool storeWarningsInDataTable)
    {
      if (reader == null) throw new ArgumentNullException(nameof(reader));

      DataTable = new DataTable
      {
        TableName = tableName,
        Locale = CultureInfo.CurrentCulture,
        CaseSensitive = false
      };
      m_ColumnErrorDictionary = new ColumnErrorDictionary(reader);
      //TODO open if need and make sure warning from this are captured

      m_IncludeErrorField = addErrorField;
      m_StoreWarningsInDataTable = storeWarningsInDataTable;
      for (var col = 0; col < reader.FieldCount; col++)
      {
        // Store ignored columns, so reference from index to name is maintained
        var column = reader.GetColumn(col);
        m_ReaderColumns.Add(column.Name);

        if (column.Ignore) continue;
        DataTable.Columns.Add(new DataColumn(column.Name, column.ValueFormat.DataType.GetNetType()));
        m_Mapping.Add(col, DataTable.Columns[column.Name].Ordinal);
      }

      if (addStartLine && !m_ReaderColumns.Contains(ReaderConstants.cStartLineNumberFieldName, StringComparer.OrdinalIgnoreCase))
      {
        // Append Artificial columns This needs to happen in the same order as we have in
        // CreateTableFromReader otherwise BulkCopy does not work see SqlServerConnector.CreateTable
        m_StartLine = new DataColumn(ReaderConstants.cStartLineNumberFieldName, typeof(long));
        DataTable.Columns.Add(m_StartLine);
      }

      if (addRecordNo && !m_ReaderColumns.Contains(ReaderConstants.cRecordNumberFieldName, StringComparer.OrdinalIgnoreCase))
      {
        m_RecordNumber = new DataColumn(ReaderConstants.cRecordNumberFieldName, typeof(long));
        DataTable.Columns.Add(m_RecordNumber);
        DataTable.PrimaryKey = new[] { m_RecordNumber };
      }

      if (addEndLineNo && !m_ReaderColumns.Contains(ReaderConstants.cEndLineNumberFieldName ,StringComparer.OrdinalIgnoreCase))
      {
        m_EndLine = new DataColumn(ReaderConstants.cEndLineNumberFieldName, typeof(long));
        DataTable.Columns.Add(m_EndLine);
      }

      if (!addErrorField || m_ReaderColumns.Contains(ReaderConstants.cErrorField, StringComparer.OrdinalIgnoreCase)) return;
      m_Error = new DataColumn(ReaderConstants.cErrorField, typeof(string));
      DataTable.Columns.Add(m_Error);
    }

    /// <summary>
    /// Store the information from the reader in the data table
    /// </summary>
    /// <param name="reader"></param>
    /// <returns><c>true</c> no warnings, <c>false</c> warnings have been raised</returns>
    public bool CopyRowToTable([NotNull] IFileReader reader)
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
      }

      if (m_IncludeErrorField)
      {
        dataRow[m_Error] = ErrorInformation.ReadErrorInformation(m_ColumnErrorDictionary, m_ReaderColumns);
      }

      HandlePreviousRow();

      m_ColumnErrorDictionary.Clear();
      return false;
    }

    public void HandlePreviousRow()
    {
      if (!m_StoreWarningsInDataTable && !m_IncludeErrorField)
        return;
      if (m_ColumnErrorDictionary.Count == 0)
        return;
      var allValues = m_ColumnErrorDictionary.Where(x => x.Key == -2).Select(x => x.Value).ToList();
      if (allValues.Count == 0)
        return;

      var previousDataRow = DataTable.Rows[DataTable.Rows.Count - 1];
      if (m_StoreWarningsInDataTable)
      {
        foreach (var prev in allValues)
          previousDataRow.RowError = previousDataRow.RowError.AddMessage(prev);
      }

      if (!m_IncludeErrorField) return;

      foreach (var prev in allValues)
        previousDataRow[m_Error] = previousDataRow[m_Error].ToString().AddMessage(prev);
    }
  }
}