/*s
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

using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   IFileReader implementation based on a data table, this is used to pass on a data table to a writer
  /// </summary>
  /// <remarks>Some functionality for progress reporting are not implemented</remarks>
  public class DataTableWrapper : DbDataReader, IFileReader
  {
    [NotNull] private DbDataReader m_DbDataReader;

    // ReSharper disable once NotNullMemberIsNotInitialized
    public DataTableWrapper([NotNull] DataTable dt) => DataTable = dt ?? throw new ArgumentNullException(nameof(dt));

    [NotNull] public DataTable DataTable { get; }

    public int Percent => 50;
    public override bool HasRows => m_DbDataReader.HasRows;

    public override string GetName(int i) => m_DbDataReader.GetName(i);

    public override string GetDataTypeName(int i) => m_DbDataReader.GetDataTypeName(i);

    public override Type GetFieldType(int i) => m_DbDataReader.GetFieldType(i);

    public override object GetValue(int i) => m_DbDataReader.GetValue(i);

    public override int GetValues(object[] values) => m_DbDataReader.GetValues(values);

    public override int GetOrdinal(string name) => m_DbDataReader.GetOrdinal(name);

    public override bool GetBoolean(int i) => m_DbDataReader.GetBoolean(i);

    public override byte GetByte(int i) => m_DbDataReader.GetByte(i);

    public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) =>
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

    public new IDataReader GetData(int i) => m_DbDataReader.GetData(i);

    public override bool IsDBNull(int i) => m_DbDataReader.IsDBNull(i);

    public override int FieldCount => m_DbDataReader.FieldCount;

    public override object this[int i] => m_DbDataReader[i];

    public override object this[string name] => m_DbDataReader[name];

    public override int RecordsAffected => m_DbDataReader.RecordsAffected;
    public bool SupportsReset => true;

    public override bool Read() => ReadAsync(CancellationToken.None).Wait(2000);

    public override int Depth => m_DbDataReader.Depth;

    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
    public override bool IsClosed => m_DbDataReader == null || m_DbDataReader.IsClosed;

    public override DataTable GetSchemaTable() => m_DbDataReader.GetSchemaTable();

    public override bool NextResult() => false;

    public ImmutableColumn GetColumn(int column) => new ImmutableColumn(m_DbDataReader.GetName(column),
      new ImmutableValueFormat(m_DbDataReader.GetType().GetDataType()), column);

    public async Task OpenAsync(CancellationToken token)
    {
      if (OnOpen != null) await OnOpen.Invoke();
      await ResetPositionToFirstDataRowAsync(token);
    }

    public override void Close()
    {
      EndOfFile = true;
      // ReSharper disable ConstantConditionalAccessQualifier
      m_DbDataReader?.Close();
      m_DbDataReader?.Dispose();
      // ReSharper restore ConstantConditionalAccessQualifier
      // ReSharper disable once AssignNullToNotNullAttribute
      m_DbDataReader = null;
    }

    public long RecordNumber { get; private set; }

    public long StartLineNumber => RecordNumber;
    public long EndLineNumber => RecordNumber;

    public bool EndOfFile { get; private set; }

    public override async Task<bool> ReadAsync(CancellationToken token)
    {
      if (!token.IsCancellationRequested && !EndOfFile)
      {
        var couldRead = await m_DbDataReader.ReadAsync(token).ConfigureAwait(false);
        if (couldRead) RecordNumber++;

        if (couldRead && !IsClosed)
          return true;
      }
      EndOfFile = true;
      ReadFinished?.Invoke(this, new EventArgs());
      return false;
    }

    public event EventHandler<WarningEventArgs> Warning;

    public Func<Task> OnOpen { get; set; }

    public event EventHandler ReadFinished;

    public event EventHandler<ICollection<IColumn>> OpenFinished;

    public event EventHandler<RetryEventArgs> OnAskRetry;

#pragma warning disable 1998

    public async Task ResetPositionToFirstDataRowAsync(CancellationToken token)
#pragma warning restore 1998
    {
      Close();
      m_DbDataReader = DataTable.CreateDataReader();
      EndOfFile = false;
      RecordNumber = 0;
    }

    public override IEnumerator GetEnumerator() => m_DbDataReader.GetEnumerator();

    private bool m_DisposedValue;

    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (disposing)
      {
        m_DisposedValue =true;
        DataTable.Dispose();
      }
      base.Dispose(disposing);
    }
  }
}