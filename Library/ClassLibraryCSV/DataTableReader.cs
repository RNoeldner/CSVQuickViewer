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
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   IFileReader implementation based on a data table
  /// </summary>
  /// <remarks>Some functionality for progress report are not implemented</remarks>
  public class DataTableReader : BaseFileReader, IFileReader
  {
    private readonly DataTable m_DataTable;
    private DbDataReader m_DbDataReader;

    public DataTableReader(DataTable dt, string id, IProcessDisplay processDisplay) : base(
      new DataTableSetting(id), TimeZoneInfo.Local.Id, processDisplay) =>
      m_DataTable = dt ?? throw new ArgumentNullException(nameof(dt));

    public override async Task<DataTable> GetDataTableAsync(long recordLimit, bool ignore, bool ignore2, bool ignore3, CancellationToken token) => await Task.FromResult(m_DataTable);

    public override string GetName(int i) => m_DbDataReader.GetName(i);

    public string GetDataTypeName(int i) => m_DbDataReader.GetDataTypeName(i);

    public override Type GetFieldType(int i) => m_DbDataReader.GetFieldType(i);

    public override object GetValue(int i) => m_DbDataReader.GetValue(i);

    public override int GetValues(object[] values) => m_DbDataReader.GetValues(values);

    public override int GetOrdinal(string name) => m_DbDataReader.GetOrdinal(name);

    public override bool GetBoolean(int i) => m_DbDataReader.GetBoolean(i);

    public override byte GetByte(int i) => m_DbDataReader.GetByte(i);

    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) =>
      m_DbDataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);

    public override char GetChar(int i) => m_DbDataReader.GetChar(i);

    public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) =>
      m_DbDataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);

    public override Guid GetGuid(int i) => m_DbDataReader.GetGuid(i);

    public override short GetInt16(int i) => m_DbDataReader.GetInt16(i);

    public override int GetInt32(int i) => m_DbDataReader.GetInt32(i);

    public override long GetInt64(int i) => m_DbDataReader.GetInt64(i);

    public override float GetFloat(int i) => m_DbDataReader.GetFloat(i);

    public override double GetDouble(int i) => m_DbDataReader.GetDouble(i);

    public override string GetString(int i) => m_DbDataReader.GetValue(i).ToString();

    public override decimal GetDecimal(int i) => m_DbDataReader.GetDecimal(i);

    public override DateTime GetDateTime(int i) => m_DbDataReader.GetDateTime(i);

    public IDataReader GetData(int i) => m_DbDataReader.GetData(i);

    public override bool IsDBNull(int i) => m_DbDataReader.IsDBNull(i);

    public override int FieldCount => m_DbDataReader.FieldCount;

    public new object this[int i] => m_DbDataReader[i];

    public new object this[string name] => m_DbDataReader[name];

    public override int RecordsAffected => m_DbDataReader.RecordsAffected;
    public override int Depth => m_DbDataReader.Depth;

    public override bool IsClosed => m_DbDataReader?.IsClosed ?? true;

    public override DataTable GetSchemaTable() => m_DbDataReader.GetSchemaTable();

    public override bool NextResult() => m_DbDataReader.NextResult();

    public override async Task OpenAsync()
    {
      BeforeOpen("Opening Data Table");
      InitColumn(m_DataTable.Columns.Count);

      // Initialize the Columns
      foreach (DataColumn col in m_DataTable.Columns)
      {
        var column = Column[col.Ordinal];
        column.ValueFormat.DataType = col.DataType.GetDataType();
        column.Name = col.ColumnName;
      }

      await ResetPositionToFirstDataRowAsync();
    }

    public override void Close()
    {
      EndOfFile = true;
      m_DbDataReader?.Dispose();
    }

    public override long StartLineNumber => RecordNumber;
    public override long EndLineNumber => RecordNumber;

    public override async Task<bool> ReadAsync()
    {
      if (!CancellationToken.IsCancellationRequested)
      {
        EndOfFile = !await m_DbDataReader.ReadAsync();
        if (!EndOfFile) RecordNumber++;

        InfoDisplay(!EndOfFile);
        if (!EndOfFile && !IsClosed)
          return true;
      }

      HandleReadFinished();
      return false;
    }

    public new async Task ResetPositionToFirstDataRowAsync()
    {
      await base.ResetPositionToFirstDataRowAsync();
      m_DbDataReader?.Dispose();
      m_DbDataReader = m_DataTable.CreateDataReader();
    }

    protected override int GetRelativePosition() => (int)((double)RecordNumber / m_DataTable.Rows.Count * cMaxValue);

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (disposing)
        m_DataTable.Dispose();
    }
  }
}