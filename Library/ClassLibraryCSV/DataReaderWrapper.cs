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
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc cref="CsvTools.IFileReader" />
  /// <summary>
  ///   Wrapper around another an open IDataReader adding artificial fields and removing ignored columns
  /// </summary>
  /// <remarks>
  ///   Introduced to allow a stream into SQLBulkCopy and possibly replace CopyToDataTableInfo. <br
  ///   /> This does not need to be disposed the passed in Reader though need to be disposed. <br />
  ///   Closing does close the passed in reader
  /// </remarks>
  public class DataReaderWrapper : DbDataReader, IFileReader
  {
    protected readonly IFileReader? FileReader;
    private readonly long m_RecordLimit;
    public readonly ReaderMapping ReaderMapping;
    protected IDataReader DataReader;

    /// <inheritdoc />
    /// <summary>
    ///   Constructor for a DataReaderWrapper <br /> This wrapper adds artificial fields like Error,
    ///   start and end Line or record number and handles the return of these artificial fields in GetValue
    /// </summary>
    /// <param name="reader">Regular framework IDataReader</param>
    /// <param name="recordLimit">Number of maximum records to read, 0 if there is no limit</param>
    /// <param name="addErrorField">Add artificial field Error</param>
    /// <param name="addStartLine">Add artificial field Start Line</param>
    /// <param name="addEndLine">Add artificial field End Line</param>
    /// <param name="addRecNum">Add artificial field Records Number</param>
    public DataReaderWrapper(
      in IDataReader reader,
      long recordLimit = 0,
      bool addErrorField = false,
      bool addStartLine = false,
      bool addEndLine = false,
      bool addRecNum = false)
    {
      DataReader = reader ?? throw new ArgumentNullException(nameof(reader));
      FileReader = reader as IFileReader;
      if (reader.IsClosed)
        throw new ArgumentException("Reader must be opened");
      m_RecordLimit = recordLimit < 1 ? long.MaxValue : recordLimit;
      ReaderMapping = new ReaderMapping(DataReader, addStartLine, addRecNum, addEndLine, addErrorField);
      if (FileReader != null)
        FileReader.Warning += (o, e) => Warning?.Invoke(o, e);
    }

    /// <summary>
    ///   Constructor for a DataReaderWrapper, this wrapper adds artificial fields like Error, start
    ///   and end Line or record number
    /// </summary>
    /// <param name="fileReader"><see cref="IFileReader" /></param>
    /// <param name="recordLimit">Number of maximum records to read, 0 if there is no limit</param>
    /// <param name="addErrorField">Add artificial field Error</param>
    /// <param name="addStartLine">Add artificial field Start Line</param>
    /// <param name="addEndLine">Add artificial field End Line</param>
    /// <param name="addRecNum">Add artificial field Records Number</param>
    public DataReaderWrapper(
      in IFileReader fileReader,
      long recordLimit = 0,
      bool addErrorField = false,
      bool addStartLine = false,
      bool addEndLine = false,
      bool addRecNum = false) : this(fileReader as IDataReader, recordLimit, addErrorField, addStartLine, addEndLine, addRecNum)
    {
    }

    public override bool HasRows => !DataReader.IsClosed;

    /// <inheritdoc />
    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    public event EventHandler<WarningEventArgs>? Warning;

    public override int Depth => FieldCount;

    public long EndLineNumber => FileReader?.EndLineNumber ?? RecordNumber;

    public bool EndOfFile => FileReader?.EndOfFile ?? (DataReader.IsClosed || RecordNumber >= m_RecordLimit);

    public override int FieldCount => ReaderMapping.Column.Count;

    public override bool IsClosed => DataReader.IsClosed;

    public virtual int Percent
    {
      get
      {
        if (FileReader is null)
          return RecordNumber <= 0 ? 0 : (int) (RecordNumber / (double) m_RecordLimit * 100d);
        return FileReader.Percent;
      }
    }

    public long RecordNumber { get; protected set; }

    public override int RecordsAffected => m_RecordLimit.ToInt();

    public long StartLineNumber => FileReader?.StartLineNumber ?? RecordNumber;

    public Func<Task> OnOpen { set => throw new NotImplementedException(); }

    public bool SupportsReset => throw new NotImplementedException();

    public override object this[int ordinal] => GetValue(ordinal);

    public override object this[string name] => GetValue(GetOrdinal(name));

    public override void Close() => DataReader.Close();

#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    public override async Task CloseAsync()
    {
      if (DataReader is DbDataReader dbDataReader)
        await dbDataReader.CloseAsync();
      else
        DataReader.Close();
    }
#endif

    public override bool GetBoolean(int ordinal) => DataReader.GetBoolean(ReaderMapping.DataTableToReader(ordinal));

    public override byte GetByte(int ordinal) => DataReader.GetByte(ReaderMapping.DataTableToReader(ordinal));

    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) =>
      DataReader.GetBytes(ReaderMapping.DataTableToReader(ordinal), dataOffset, buffer, bufferOffset, length);

    public override char GetChar(int ordinal) => DataReader.GetChar(ReaderMapping.DataTableToReader(ordinal));

    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) =>
      DataReader.GetChars(ReaderMapping.DataTableToReader(ordinal), dataOffset, buffer, bufferOffset, length);

    public new IDataReader? GetData(int i) => DataReader.GetData(i);

    public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

    public override DateTime GetDateTime(int ordinal) =>
      DataReader.GetDateTime(ReaderMapping.DataTableToReader(ordinal));

    public override decimal GetDecimal(int ordinal) => DataReader.GetDecimal(ReaderMapping.DataTableToReader(ordinal));

    public override double GetDouble(int ordinal) => DataReader.GetDouble(ReaderMapping.DataTableToReader(ordinal));

    public override Type GetFieldType(int ordinal) => ReaderMapping.Column[ordinal].ValueFormat.DataType.GetNetType();

    public override float GetFloat(int ordinal) => DataReader.GetFloat(ReaderMapping.DataTableToReader(ordinal));

    public override Guid GetGuid(int ordinal) => DataReader.GetGuid(ReaderMapping.DataTableToReader(ordinal));

    public override short GetInt16(int ordinal) => DataReader.GetInt16(ReaderMapping.DataTableToReader(ordinal));

    public override int GetInt32(int ordinal) => DataReader.GetInt32(ReaderMapping.DataTableToReader(ordinal));

    public override long GetInt64(int ordinal)
    {
      if (ordinal == ReaderMapping.DataTableStartLine)
        return StartLineNumber;
      if (ordinal == ReaderMapping.DataTableEndLine)
        return EndLineNumber;
      if (ordinal == ReaderMapping.DataTableRecNum)
        return RecordNumber;

      return DataReader.GetInt64(ReaderMapping.DataTableToReader(ordinal));
    }

    public override string GetName(int ordinal) => ReaderMapping.Column[ordinal].Name;

    public override int GetOrdinal(string name)
    {
      if (string.IsNullOrEmpty(name))
        return -1;
      var count = 0;
      foreach (var column in ReaderMapping.Column)
      {
        if (name.Equals(column.Name, StringComparison.OrdinalIgnoreCase))
          return count;
        count++;
      }

      return -1;
    }

    public override DataTable GetSchemaTable()
    {
      var dataTable = ReaderConstants.GetEmptySchemaTable();
      var schemaRow = ReaderConstants.GetDefaultSchemaRowArray();

      for (var col = 0; col < FieldCount; col++)
      {
        var column = ReaderMapping.Column[col];

        schemaRow[1] = column.Name; // Column name
        schemaRow[4] = column.Name; // Column name
        schemaRow[5] = col;         // Column ordinal

        if (col == ReaderMapping.DataTableStartLine || col == ReaderMapping.DataTableRecNum
                                                    || col == ReaderMapping.DataTableEndLine)
        {
          schemaRow[7] = typeof(long);
        }
        else
        {
          // If there is a conversion get the information
          if (column.Convert && column.ValueFormat.DataType != DataType.String)
            schemaRow[7] = column.ValueFormat.DataType.GetNetType();
          else
            schemaRow[7] = typeof(string);
        }

        dataTable.Rows.Add(schemaRow);
      }

      return dataTable;
    }

    public override string GetString(int ordinal) => Convert.ToString(GetValue(ordinal));

    public override object GetValue(int ordinal)
    {
      if (ordinal == ReaderMapping.DataTableStartLine)
        return StartLineNumber;
      if (ordinal == ReaderMapping.DataTableEndLine)
        return EndLineNumber;
      if (ordinal == ReaderMapping.DataTableRecNum)
        return RecordNumber;
      if (ordinal == ReaderMapping.DataTableErrorField)
        return ReaderMapping.RowErrorInformation ?? string.Empty;

      return DataReader.GetValue(ReaderMapping.DataTableToReader(ordinal));
    }

    public override int GetValues(object[] values) => DataReader.GetValues(values);

    public override bool IsDBNull(int ordinal)
    {
      if (ordinal == ReaderMapping.DataTableStartLine || ordinal == ReaderMapping.DataTableEndLine || ordinal == ReaderMapping.DataTableRecNum)
        return false;

      return (ordinal == ReaderMapping.DataTableErrorField ? ReaderMapping.HasErrors : DataReader.IsDBNull(ReaderMapping.DataTableToReader(ordinal)));
    }

    public override bool NextResult() => false;

    public override bool Read()
    {
      ReaderMapping.PrepareRead();
      var couldRead = DataReader.Read();
      if (couldRead)
        RecordNumber++;
      return couldRead && RecordNumber <= m_RecordLimit;
    }

    public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
      ReaderMapping.PrepareRead();
      // IDataReader does not support preferred ReadAsync
      var couldRead = DataReader is DbDataReader dbDataReader
                        ? await dbDataReader.ReadAsync(cancellationToken).ConfigureAwait(false)
                        : DataReader.Read();

      if (couldRead)
        RecordNumber++;
      return couldRead && RecordNumber <= m_RecordLimit;
    }

    public IColumn GetColumn(int column) => throw new NotImplementedException();

    public Task OpenAsync(CancellationToken token) => throw new NotImplementedException();

    public void ResetPositionToFirstDataRow() => throw new NotImplementedException();

#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    public override async Task CloseAsync()
    {
      if (DataReader is DbDataReader dbDataReader)
        await dbDataReader.CloseAsync();
      else
        DataReader.Close();
    }
#endif

    public override IEnumerator GetEnumerator() => new DbEnumerator(DataReader, false);
  }
}