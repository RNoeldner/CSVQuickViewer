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
#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
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
    public readonly ReaderMapping ReaderMapping;
    protected IDataReader DataReader;

    /// <inheritdoc />
    /// <summary>
    ///   Constructor for a DataReaderWrapper <br /> This wrapper adds artificial fields like Error,
    ///   start and end Line or record number and handles the return of these artificial fields in GetValue
    /// </summary>
    /// <param name="reader">Regular framework IDataReader</param>
    /// <param name="addStartLine">Add artificial field Start Line</param>
    /// <param name="addEndLine">Add artificial field End Line</param>
    /// <param name="addRecNum">Add artificial field Records Number</param>
    /// <param name="addErrorField">Add artificial field Error</param>
    public DataReaderWrapper(
      in IDataReader reader,
      bool addStartLine = false,
      bool addEndLine = false,
      bool addRecNum = false,
      bool addErrorField = false)
    {
      DataReader = reader ?? throw new ArgumentNullException(nameof(reader));
      FileReader = reader as IFileReader;
      if (reader.IsClosed)
        throw new ArgumentException("Reader must be opened");
      ReaderMapping = new ReaderMapping(DataReader, addStartLine, addRecNum, addEndLine, addErrorField);
      if (FileReader != null)
        FileReader.Warning += (o, e) => Warning?.Invoke(o, e);
    }

    /// <inheritdoc />
    public IProgress<ProgressInfo> ReportProgress
    {
      set
      {
        if (FileReader!=null)
          FileReader.ReportProgress = value;
      }
    }

    // Count the warning rows, one row could have multiple issue and should only be once need to
    // track the rows
    private readonly HashSet<long> m_RowsWithIssue = new HashSet<long>();
    public long NumberRowWarnings => m_RowsWithIssue.Count;

    /// <inheritdoc />
    /// <summary>
    ///   Constructor for a DataReaderWrapper, this wrapper adds artificial fields like Error, start
    ///   and end Line or record number
    /// </summary>
    /// <param name="fileReader"><see cref="T:CsvTools.IFileReader" /></param>
    /// <param name="addStartLine">Add artificial field Start Line</param>
    /// <param name="addEndLine">Add artificial field End Line</param>
    /// <param name="addRecNum">Add artificial field Records Number</param>
    /// <param name="addErrorField">Add artificial field Error</param>
    public DataReaderWrapper(
      in IFileReader fileReader,
      bool addStartLine = false,
      bool addEndLine = false,
      bool addRecNum = false,
      bool addErrorField = false) : this(fileReader as IDataReader, addStartLine, addEndLine, addRecNum, addErrorField)
    {
      if (addErrorField)
        fileReader.Warning += (sender, e) => m_RowsWithIssue.Add(e.RecordNumber);
    }

    /// <inheritdoc />
    public override bool HasRows => !DataReader.IsClosed;

    public event EventHandler<RetryEventArgs>? OnAskRetry;
    public event EventHandler<IReadOnlyCollection<Column>>? OpenFinished;
    public event EventHandler? ReadFinished;

    public Func<Task>? OnOpenAsync { get; set; }

    /// <inheritdoc />
    public event EventHandler<WarningEventArgs>? Warning;

    /// <inheritdoc />
    public override int Depth => FieldCount;

    /// <inheritdoc />
    public long EndLineNumber => FileReader?.EndLineNumber ?? RecordNumber;

    /// <inheritdoc />
    public virtual bool EndOfFile => FileReader?.EndOfFile ?? DataReader.IsClosed;

    /// <inheritdoc />
    public override int FieldCount => ReaderMapping.Column.Count;

    /// <inheritdoc />
    public override bool IsClosed => DataReader.IsClosed;

    /// <inheritdoc />
    public virtual int Percent => FileReader?.Percent ?? 50;

    /// <inheritdoc />
    public long RecordNumber { get; protected set; }

    /// <inheritdoc />
    public override int RecordsAffected => RecordNumber.ToInt();

    /// <inheritdoc />
    public long StartLineNumber => FileReader?.StartLineNumber ?? RecordNumber;

    /// <inheritdoc />
    public virtual bool SupportsReset => !(FileReader is null);

    /// <inheritdoc />
    public override object this[int ordinal] => GetValue(ordinal);

    /// <inheritdoc />
    public override object this[string name] => GetValue(GetOrdinal(name));

    /// <inheritdoc />
    public override void Close() => DataReader.Close();

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <inheritdoc />
    public override async Task CloseAsync()
    {
      if (DataReader is DbDataReader dbDataReader)
        await dbDataReader.CloseAsync().ConfigureAwait(false);
      else
        DataReader.Close();
    }

#endif
    /// <inheritdoc />
    public override bool GetBoolean(int ordinal) => DataReader.GetBoolean(ReaderMapping.DataTableToReader(ordinal));
    /// <inheritdoc />
    public override byte GetByte(int ordinal) => DataReader.GetByte(ReaderMapping.DataTableToReader(ordinal));
    /// <inheritdoc />
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) =>
      DataReader.GetBytes(ReaderMapping.DataTableToReader(ordinal), dataOffset, buffer, bufferOffset, length);
    /// <inheritdoc />
    public override char GetChar(int ordinal) => DataReader.GetChar(ReaderMapping.DataTableToReader(ordinal));
    /// <inheritdoc />
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) =>
      DataReader.GetChars(ReaderMapping.DataTableToReader(ordinal), dataOffset, buffer, bufferOffset, length);
    /// <inheritdoc />
    public new IDataReader GetData(int i) => DataReader.GetData(i);
    /// <inheritdoc />
    public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;
    /// <inheritdoc />
    public override DateTime GetDateTime(int ordinal) =>
      DataReader.GetDateTime(ReaderMapping.DataTableToReader(ordinal));
    /// <inheritdoc />
    public override decimal GetDecimal(int ordinal) => DataReader.GetDecimal(ReaderMapping.DataTableToReader(ordinal));
    /// <inheritdoc />
    public override double GetDouble(int ordinal) => DataReader.GetDouble(ReaderMapping.DataTableToReader(ordinal));
    /// <inheritdoc />
    public override Type GetFieldType(int ordinal) => ReaderMapping.Column[ordinal].ValueFormat.DataType.GetNetType();
    /// <inheritdoc />
    public override float GetFloat(int ordinal) => DataReader.GetFloat(ReaderMapping.DataTableToReader(ordinal));
    /// <inheritdoc />
    public override Guid GetGuid(int ordinal) => DataReader.GetGuid(ReaderMapping.DataTableToReader(ordinal));
    /// <inheritdoc />
    public override short GetInt16(int ordinal) => DataReader.GetInt16(ReaderMapping.DataTableToReader(ordinal));
    /// <inheritdoc />
    public override int GetInt32(int ordinal) => DataReader.GetInt32(ReaderMapping.DataTableToReader(ordinal));
    /// <inheritdoc />
    public override long GetInt64(int ordinal)
    {
      if (ordinal == ReaderMapping.ColNumStartLine)
        return StartLineNumber;
      if (ordinal == ReaderMapping.ColNumEndLine)
        return EndLineNumber;
      if (ordinal == ReaderMapping.ColNumRecNum)
        return RecordNumber;

      return DataReader.GetInt64(ReaderMapping.DataTableToReader(ordinal));
    }
    /// <inheritdoc />
    public override string GetName(int ordinal) => ReaderMapping.Column[ordinal].Name;
    /// <inheritdoc />
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
    /// <inheritdoc />
    public override DataTable GetSchemaTable()
    {
      var dataTable = ReaderConstants.GetEmptySchemaTable();
      var schemaRow = ReaderConstants.GetDefaultSchemaRowArray();

      for (var col = 0; col < FieldCount; col++)
      {
        var column = ReaderMapping.Column[col];

        schemaRow[1] = column.Name; // Column name
        schemaRow[4] = column.Name; // Column name
        schemaRow[5] = col; // Column ordinal

        if (col == ReaderMapping.ColNumStartLine || col == ReaderMapping.ColNumRecNum || col == ReaderMapping.ColNumEndLine)
        {
          schemaRow[7] = typeof(long);
        }
        else
        {
          // If there is a conversion get the information
          if (column.Convert && column.ValueFormat.DataType != DataTypeEnum.String)
            schemaRow[7] = column.ValueFormat.DataType.GetNetType();
          else
            schemaRow[7] = typeof(string);
        }

        dataTable.Rows.Add(schemaRow);
      }

      return dataTable;
    }
    /// <inheritdoc />
    public override string GetString(int ordinal) => Convert.ToString(GetValue(ordinal)) ?? string.Empty;

    /// <inheritdoc />
    public override object GetValue(int ordinal)
    {
      if (ordinal == ReaderMapping.ColNumStartLine)
        return StartLineNumber;
      if (ordinal == ReaderMapping.ColNumEndLine)
        return EndLineNumber;
      if (ordinal == ReaderMapping.ColNumRecNum)
        return RecordNumber;
      if (ordinal == ReaderMapping.ColNumErrorField)
      {
        // in case the source did have an #Error column pass this on, in case empty use the error  Information        
        return (ReaderMapping.ColNumErrorFieldSource != -1) ? DataReader.GetValue(ReaderMapping.ColNumErrorFieldSource) : ReaderMapping.RowErrorInformation;
      }
      return DataReader.GetValue(ReaderMapping.DataTableToReader(ordinal)) ?? DBNull.Value;
    }

    /// <inheritdoc />
    public override int GetValues(object[] values)
      => DataReader.GetValues(values);

    /// <inheritdoc />
    public override bool IsDBNull(int ordinal)
    {
      if (ordinal == ReaderMapping.ColNumStartLine || ordinal == ReaderMapping.ColNumEndLine ||
          ordinal == ReaderMapping.ColNumRecNum)
        return false;
      if (ordinal == ReaderMapping.ColNumErrorField)
        // in case the source did have an #Error column pass this on, in case empty use the error  Information
        return (ReaderMapping.ColNumErrorFieldSource != -1) ? DataReader.IsDBNull(ReaderMapping.ColNumErrorFieldSource) : !ReaderMapping.HasErrors;
      return DataReader.IsDBNull(ReaderMapping.DataTableToReader(ordinal));
    }

    /// <inheritdoc cref="IFileReader" />
    public override bool NextResult() => false;

    /// <inheritdoc cref="IFileReader" />
    public override bool Read()
    {
      ReaderMapping.PrepareRead();
      var couldRead = DataReader.Read();
      if (couldRead)
        RecordNumber++;
      return couldRead;
    }

    /// <inheritdoc cref="IFileReader" />
    public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested)
        return false;
      ReaderMapping.PrepareRead();

      // IDataReader does not support preferred ReadAsync but DbDataReader  does
      var couldRead = DataReader is DbDataReader dbDataReader
        ? await dbDataReader.ReadAsync(cancellationToken).ConfigureAwait(false)
        : DataReader.Read();

      if (couldRead)
        RecordNumber++;
      if (!couldRead)
        ReadFinished?.Invoke(this, EventArgs.Empty);
      return couldRead;
    }
    /// <inheritdoc />
    public Column GetColumn(int column) => ReaderMapping.Column[column];

    /// <inheritdoc />
    [Obsolete("No need to open a DataReaderWrapper")]
    public async Task OpenAsync(CancellationToken token)
    {
      if (OnOpenAsync!=null)
        await OnOpenAsync.Invoke().ConfigureAwait(false);
      ReaderMapping.PrepareRead();
      OpenFinished?.Invoke(this, ReaderMapping.Column);
    }

    /// <inheritdoc />
    public virtual void ResetPositionToFirstDataRow()
    {
      FileReader?.ResetPositionToFirstDataRow();
      RecordNumber = 0;
    }
    /// <inheritdoc />
    public override IEnumerator GetEnumerator() => new DbEnumerator(DataReader, false);

    /// <inheritdoc />
    public byte[] GetFile(int ordinal) => FileReader?.GetFile(ordinal) ?? Array.Empty<byte>();
  }
}