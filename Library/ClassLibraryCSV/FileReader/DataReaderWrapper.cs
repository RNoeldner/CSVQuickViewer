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
  ///   Allow any IDataReader to be used as IFileReader
  /// </remarks>
  public class DataReaderWrapper : DbDataReader, IFileReader
  {
    /// <summary>
    /// Data Reader, that might be reset by overriding class <see cref="DataTableWrapper"/>
    /// </summary>
    protected IDataReader DataReader;

    private readonly ColumnErrorDictionary m_ColumnErrorDictionary;
    private readonly IFileReader? m_FileReader;
    private readonly ReaderMapping m_ReaderMapping;
    private readonly long m_RecordLimit;

    /// <summary>
    ///   Constructor for a DataReaderWrapper this wrapper adds artificial fields like Error,
    ///   Start and End Line or Record number in needed and handles the return of these artificial fields in GetValue
    /// </summary>
    /// <param name="reader">Regular framework IDataReader</param>
    /// <param name="startLine">Add artificial field Start Line</param>
    /// <param name="endLine">Add artificial field End Line</param>
    /// <param name="recNum">Add artificial field Records Number</param>
    /// <param name="errorField">Add artificial field Error</param>
    /// <param name="recordLimit">Maximum number of records to read</param>
    public DataReaderWrapper(in IDataReader reader,
      bool startLine = false, bool endLine = false,
      bool recNum = false, bool errorField = false, long recordLimit = -1)
    {
      DataReader = reader ?? throw new ArgumentNullException(nameof(reader));
      m_FileReader = reader as IFileReader;
      if (reader.IsClosed)
        throw new InvalidOperationException("Reader can not be closed");
      RowErrorInformation = string.Empty;
      m_ColumnErrorDictionary = new ColumnErrorDictionary(m_FileReader);
      m_RecordLimit = recordLimit < 1 ? long.MaxValue : recordLimit;
      var sourceColumns = new List<Column>();
      for (var col = 0; col < reader.FieldCount; col++)
      {
        var column = (m_FileReader != null)
          ? m_FileReader.GetColumn(col)
          : new Column(reader.GetName(col), new ValueFormat(reader.GetFieldType(col).GetDataType()), col);
        sourceColumns.Add(column);
      }

      m_ReaderMapping = new ReaderMapping(sourceColumns, startLine, endLine, recNum, errorField);

      if (m_FileReader != null)
        m_FileReader.Warning += HandleSourceWarning;
    }

    /// <inheritdoc />
    public event EventHandler<RetryEventArgs>? OnAskRetry;

    /// <inheritdoc />
    public event EventHandler<IReadOnlyCollection<Column>>? OpenFinished;

    /// <inheritdoc />
    public event EventHandler? ReadFinished;

    /// <inheritdoc />
    public event EventHandler<WarningEventArgs>? Warning;

    /// <inheritdoc />
    public override int Depth => FieldCount;

    /// <inheritdoc />
    public void HandleReadFinished() => ReadFinished?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc />
    public long EndLineNumber => m_FileReader?.EndLineNumber ?? RecordNumber;

    /// <inheritdoc />
    public virtual bool EndOfFile => RecordNumber > m_RecordLimit ||
                                     (m_FileReader?.EndOfFile ?? DataReader.IsClosed);

    /// <inheritdoc />
    public override int FieldCount => m_ReaderMapping.ResultingColumns.Count;

    /// <inheritdoc />
    public override bool HasRows => !DataReader.IsClosed;

    /// <inheritdoc />
    public override bool IsClosed => DataReader.IsClosed;

    /// <summary>
    /// Get the number of rows with errors (at least one row is missing)
    /// </summary>
    public long NumberRowError { get; private set; }

    /// <summary>
    /// Get the number of rows with issues
    /// </summary>
    public long NumberRowWarnings { get; private set; }

    /// <inheritdoc />
    public Func<Task>? OnOpenAsync { get; set; }

    /// <inheritdoc />
    public virtual int Percent => m_FileReader?.Percent ??
                                  ((m_RecordLimit < long.MaxValue)
                                    ? ((double) RecordNumber / m_RecordLimit * 100).ToInt()
                                    : 50);

    /// <inheritdoc />
    public long RecordNumber { get; private set; }

    /// <inheritdoc />
    public override int RecordsAffected => RecordNumber.ToInt();

    /// <inheritdoc />
    public IProgress<ProgressInfo> ReportProgress
    {
      set
      {
        if (m_FileReader != null)
          m_FileReader.ReportProgress = value;
      }
    }

    /// <summary>
    /// Gets the error information for the row, this could be filled by an error column or by a reader raising warnings
    /// </summary>
    public string RowErrorInformation { get; private set; }

    /// <inheritdoc />
    public long StartLineNumber => m_FileReader?.StartLineNumber ?? RecordNumber;

    /// <inheritdoc />
    public virtual bool SupportsReset => m_FileReader?.SupportsReset ?? false;

    /// <inheritdoc />
    public override object this[int ordinal] => GetValue(ordinal);

    /// <inheritdoc />
    public override object this[string name] => GetValue(GetOrdinal(name));

    /// <inheritdoc />
    public override void Close() => DataReader.Close();

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <inheritdoc />
    public override Task CloseAsync()
    {
      if (DataReader is DbDataReader dbDataReader)
        return dbDataReader.CloseAsync();
      else
        DataReader.Close();
      return Task.CompletedTask;
    }
#endif

    /// <inheritdoc />
    public override bool GetBoolean(int ordinal) => DataReader.GetBoolean(m_ReaderMapping.ResultToSource(ordinal));

    /// <inheritdoc />
    public override byte GetByte(int ordinal) => DataReader.GetByte(m_ReaderMapping.ResultToSource(ordinal));

    /// <inheritdoc />
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) =>
      DataReader.GetBytes(m_ReaderMapping.ResultToSource(ordinal), dataOffset, buffer, bufferOffset, length);

    /// <inheritdoc />
    public override char GetChar(int ordinal) => DataReader.GetChar(m_ReaderMapping.ResultToSource(ordinal));

    /// <inheritdoc />
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) =>
      DataReader.GetChars(m_ReaderMapping.ResultToSource(ordinal), dataOffset, buffer, bufferOffset, length);

    /// <inheritdoc />
    public Column GetColumn(int column) => m_ReaderMapping.ResultingColumns[column];

    /// <inheritdoc />
    public new IDataReader GetData(int ordinal) => DataReader.GetData(m_ReaderMapping.ResultToSource(ordinal));

    /// <inheritdoc />
    public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

    /// <inheritdoc />
    public override DateTime GetDateTime(int ordinal) =>
      DataReader.GetDateTime(m_ReaderMapping.ResultToSource(ordinal));

    /// <inheritdoc />
    public override decimal GetDecimal(int ordinal) => DataReader.GetDecimal(m_ReaderMapping.ResultToSource(ordinal));

    /// <inheritdoc />
    public override double GetDouble(int ordinal) => DataReader.GetDouble(m_ReaderMapping.ResultToSource(ordinal));

    /// <inheritdoc />
    public override IEnumerator GetEnumerator() => new DbEnumerator(DataReader, false);

    /// <inheritdoc />
    public override Type GetFieldType(int ordinal) =>
      m_ReaderMapping.ResultingColumns[ordinal].ValueFormat.DataType.GetNetType();

    /// <inheritdoc />
    public override float GetFloat(int ordinal) => DataReader.GetFloat(m_ReaderMapping.ResultToSource(ordinal));

    /// <inheritdoc />
    public override Guid GetGuid(int ordinal) => DataReader.GetGuid(m_ReaderMapping.ResultToSource(ordinal));

    /// <inheritdoc />
    public override short GetInt16(int ordinal) => DataReader.GetInt16(m_ReaderMapping.ResultToSource(ordinal));

    /// <inheritdoc />
    public override int GetInt32(int ordinal) => DataReader.GetInt32(m_ReaderMapping.ResultToSource(ordinal));

    /// <inheritdoc />
    public override long GetInt64(int ordinal)
    {
      if (ordinal == m_ReaderMapping.ColNumStartLine)
        return StartLineNumber;
      if (ordinal == m_ReaderMapping.ColNumEndLine)
        return EndLineNumber;
      return ordinal == m_ReaderMapping.ColNumRecNum
        ? RecordNumber
        : DataReader.GetInt64(m_ReaderMapping.ResultToSource(ordinal));
    }

    /// <inheritdoc />
    public override string GetName(int ordinal) => m_ReaderMapping.ResultingColumns[ordinal].Name;

    /// <inheritdoc />
    public override int GetOrdinal(string name)
    {
      if (string.IsNullOrEmpty(name))
        return -1;
      var count = 0;
      foreach (var column in m_ReaderMapping.ResultingColumns)
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
        var column = m_ReaderMapping.ResultingColumns[col];
        schemaRow[1] = column.Name; // ResultingColumns name
        schemaRow[4] = column.Name; // ResultingColumns name         
        schemaRow[5] = col; // ResultingColumns ordinal
        schemaRow[7] = column.ValueFormat.DataType.GetNetType();
        dataTable.Rows.Add(schemaRow);
      }

      return dataTable;
    }

    /// <inheritdoc />
    // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
    public override string GetString(int ordinal)
      => (ordinal == m_ReaderMapping.ColNumErrorField)
        ? RowErrorInformation
        : DataReader.GetString(m_ReaderMapping.ResultToSource(ordinal));

    /// <inheritdoc />
    public override object GetValue(int ordinal)
    {
      if (ordinal == m_ReaderMapping.ColNumStartLine)
        return StartLineNumber;
      if (ordinal == m_ReaderMapping.ColNumEndLine)
        return EndLineNumber;
      if (ordinal == m_ReaderMapping.ColNumRecNum)
        return RecordNumber;
      return ordinal == m_ReaderMapping.ColNumErrorField
        ? RowErrorInformation
        : DataReader.GetValue(m_ReaderMapping.ResultToSource(ordinal));
    }

    /// <inheritdoc />
    public override int GetValues(object[] values)
    {
      if (values is null) throw new ArgumentNullException(nameof(values));

      var maxFld = values.Length;
      if (maxFld > FieldCount) maxFld = FieldCount;

      for (var col = 0; col < maxFld; col++)
        values[col] = GetValue(col);

      return maxFld;
    }

    /// <inheritdoc />
    public override bool IsDBNull(int ordinal)
    {
      if (ordinal == m_ReaderMapping.ColNumStartLine || ordinal == m_ReaderMapping.ColNumEndLine ||
          ordinal == m_ReaderMapping.ColNumRecNum)
        return false;
      if (ordinal == m_ReaderMapping.ColNumErrorField)
        return RowErrorInformation.Length == 0;
      return DataReader.IsDBNull(m_ReaderMapping.ResultToSource(ordinal));
    }

    /// <inheritdoc cref="IFileReader" />
    public override bool NextResult() => false;

    /// <inheritdoc />
    [Obsolete("No need to open a DataReaderWrapper, passed in reader is open already")]
    public Task OpenAsync(CancellationToken token)
    {
      if (m_FileReader == null)
      {
        OnOpenAsync?.Invoke();
        ResetPositionToFirstDataRow();
        OpenFinished?.Invoke(this, m_ReaderMapping.ResultingColumns);

        return Task.CompletedTask;
      }

      return m_FileReader.OpenAsync(token);
    }

    /// <inheritdoc cref="IFileReader" />
    public override bool Read()
    {
      if (EndOfFile)
        return false;
      if (DataReader.Read())
      {
        FinishRead();
        return true;
      }

      return false;
    }

    /// <inheritdoc cref="IFileReader" />
    public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
      if (cancellationToken.IsCancellationRequested || EndOfFile)
        return false;

      if (DataReader is DbDataReader dbDataReader
            ? await dbDataReader.ReadAsync(cancellationToken).ConfigureAwait(false)
            : DataReader.Read())
      {
        FinishRead();
        return true;
      }

      HandleReadFinished();
      return false;
    }

    /// <inheritdoc />
    public virtual void ResetPositionToFirstDataRow()
    {
      m_FileReader?.ResetPositionToFirstDataRow();
      m_ColumnErrorDictionary.Clear();
      RowErrorInformation = string.Empty;
      RecordNumber = 0;
      NumberRowWarnings = 0;
    }

    /// <summary>
    /// Takes care of internal counters being updated
    /// </summary>
    private void FinishRead()
    {
      RecordNumber++;

      if (m_ReaderMapping.ColNumErrorFieldSource != -1)
        RowErrorInformation = DataReader.IsDBNull(m_ReaderMapping.ColNumErrorFieldSource)
          ? string.Empty
          : DataReader.GetValue(m_ReaderMapping.ColNumErrorFieldSource).ToString() ?? string.Empty;
      else
      {
        RowErrorInformation = ErrorInformation.ReadErrorInformation(m_ColumnErrorDictionary,
          i => i >= 0 ? m_ReaderMapping.ResultingColumns[i].Name : string.Empty);
        m_ColumnErrorDictionary.Clear();
      }

      if (string.IsNullOrEmpty(RowErrorInformation))
        return;

      if (RowErrorInformation.IsWarningMessage())
        NumberRowWarnings++;
      else
        NumberRowError++;

      Warning?.Invoke(this,
        new WarningEventArgs(RecordNumber, 0, RowErrorInformation, StartLineNumber, EndLineNumber, string.Empty));
    }

    /// <summary>
    /// Handles the warnings raised in the source and adds them to the corresponding columns
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="WarningEventArgs"/> instance containing the event data.</param>
    private void HandleSourceWarning(object? sender, WarningEventArgs e)
    {
      if (e.ColumnNumber < 0)
        m_ColumnErrorDictionary.Add(-1, e.Message);
      if (m_ReaderMapping.SourceToResult(e.ColumnNumber, out var ownColumnIndex))
        m_ColumnErrorDictionary.Add(ownColumnIndex, e.Message);
    }
  }
}