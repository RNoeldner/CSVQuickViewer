/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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

namespace CsvTools;

/// <inheritdoc cref="CsvTools.IFileReader" />
/// <summary>
///   Wrapper around another an open IDataReader adding artificial fields and removing ignored columns
/// </summary>
/// <remarks>
///   Allows any IDataReader to be used as IFileReader
/// </remarks>
public class DataReaderWrapper : DbDataReader, IFileReader
{
  /// <summary>
  /// Data Reader, that might be reset by overriding class <see cref="DataTableWrapper"/>
  /// </summary>
  protected IDataReader DataReader;
  private readonly Dictionary<int, ReadOnlyMemory<char>> m_ColumnErrorDictionary = new Dictionary<int, ReadOnlyMemory<char>>();
  private readonly IFileReader? m_FileReader;

  /// <summary>
  /// The mapping for columns between source and destination 
  /// </summary>
  protected readonly ReaderMapping MReaderMapping;
  private readonly long m_RecordLimit;

  /// <summary>
  ///   Constructor for a DataReaderWrapper this wrapper adds artificial fields like Error,
  ///   Start and End Line, or Record number in needed and handles the return of these artificial fields in GetValue
  /// </summary>
  /// <param name="reader">Regular framework IDataReader</param>
  /// <param name="startLine">Add artificial field Start Line</param>
  /// <param name="endLine">Add artificial field End Line</param>
  /// <param name="recNum">Add artificial field Records Number</param>
  /// <param name="errorField">Add artificial field Error</param>
  /// <param name="recordLimit">Maximum number of records to read</param>
  public DataReaderWrapper(IDataReader reader,
    bool startLine = false, bool endLine = false,
    bool recNum = false, bool errorField = false, long recordLimit = -1)
  {
    if (reader.IsClosed)
      throw new InvalidOperationException("Reader can not be status closed");
    DataReader = reader ?? throw new ArgumentNullException(nameof(reader));
    m_FileReader = reader as IFileReader;
    RowErrorInformation = string.Empty;
    m_RecordLimit = recordLimit < 1 ? long.MaxValue : recordLimit;
    var sourceColumns = new List<Column>();
    for (var col = 0; col < reader.FieldCount; col++)
    {
      var column = (m_FileReader != null)
        ? m_FileReader.GetColumn(col)
        : new Column(reader.GetName(col), new ValueFormat(reader.GetFieldType(col).GetDataType()), col);
      sourceColumns.Add(column);
    }

    MReaderMapping = new ReaderMapping(sourceColumns, startLine, endLine, recNum, errorField);
    if (m_FileReader != null && MReaderMapping.ColNumErrorFieldSource == -1)
    {
      m_FileReader.Warning += HandleSourceWarning;
    }
  }

  /// <inheritdoc />
  public event EventHandler<IReadOnlyCollection<Column>>? OpenFinished;

  /// <inheritdoc />
  public event EventHandler? ReadFinished;

  /// <inheritdoc />
  public event EventHandler<WarningEventArgs>? Warning;

  /// <inheritdoc />
  public override int Depth => FieldCount;

  /// <inheritdoc />
  public void HandleReadFinished() => ReadFinished?.SafeInvoke(this);

  /// <inheritdoc />
  public long EndLineNumber => m_FileReader?.EndLineNumber ?? RecordNumber;

  /// <inheritdoc />
  public virtual bool EndOfFile => RecordNumber > m_RecordLimit ||
                                   (m_FileReader?.EndOfFile ?? DataReader.IsClosed);

  /// <inheritdoc />
  public override int FieldCount => MReaderMapping.ResultingColumns.Count;

  /// <inheritdoc />
  public override bool HasRows => !DataReader.IsClosed;

  /// <inheritdoc />
  public override bool IsClosed => DataReader.IsClosed;


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
    set => m_FileReader?.ReportProgress = value;
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
  public Func<Exception, bool> AskRetry { get; set; } = (_) => false;

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
  public override bool GetBoolean(int ordinal) => DataReader.GetBoolean(MReaderMapping.ResultToSource(ordinal));

  /// <inheritdoc />
  public override byte GetByte(int ordinal) => DataReader.GetByte(MReaderMapping.ResultToSource(ordinal));

  /// <inheritdoc />
  public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) =>
    DataReader.GetBytes(MReaderMapping.ResultToSource(ordinal), dataOffset, buffer, bufferOffset, length);

  /// <inheritdoc />
  public override char GetChar(int ordinal) => DataReader.GetChar(MReaderMapping.ResultToSource(ordinal));

  /// <inheritdoc />
  public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) =>
    DataReader.GetChars(MReaderMapping.ResultToSource(ordinal), dataOffset, buffer, bufferOffset, length);

  /// <inheritdoc />
  public Column GetColumn(int column) => MReaderMapping.ResultingColumns[column];

  /// <inheritdoc />
  public new IDataReader GetData(int i) => DataReader.GetData(MReaderMapping.ResultToSource(i))!;

  /// <inheritdoc />
  public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

  /// <inheritdoc />
  public override DateTime GetDateTime(int ordinal) =>
    DataReader.GetDateTime(MReaderMapping.ResultToSource(ordinal));

  /// <inheritdoc />
  public override decimal GetDecimal(int ordinal) => DataReader.GetDecimal(MReaderMapping.ResultToSource(ordinal));

  /// <inheritdoc />
  public override double GetDouble(int ordinal) => DataReader.GetDouble(MReaderMapping.ResultToSource(ordinal));

  /// <inheritdoc />
  public override IEnumerator GetEnumerator() => new DbEnumerator(DataReader, false);

  /// <inheritdoc />
  public override Type GetFieldType(int ordinal) =>
    MReaderMapping.ResultingColumns[ordinal].ValueFormat.DataType.GetNetType();

  /// <inheritdoc />
  public override float GetFloat(int ordinal) => DataReader.GetFloat(MReaderMapping.ResultToSource(ordinal));

  /// <inheritdoc />
  public override Guid GetGuid(int ordinal) => DataReader.GetGuid(MReaderMapping.ResultToSource(ordinal));

  /// <inheritdoc />
  public override short GetInt16(int ordinal) => DataReader.GetInt16(MReaderMapping.ResultToSource(ordinal));


  /// <inheritdoc />
  public override int GetInt32(int ordinal)
  {
    // Return fixed columns are not mapped to the underlying reader, so handle them directly
    if (ordinal == MReaderMapping.ColNumStartLine)
      return StartLineNumber.ToInt();
    if (ordinal == MReaderMapping.ColNumEndLine)
      return EndLineNumber.ToInt();
    if (ordinal == MReaderMapping.ColNumRecNum)
      return RecordNumber.ToInt();
    return DataReader.GetInt32(MReaderMapping.ResultToSource(ordinal));
  }

  /// <inheritdoc />
  public override long GetInt64(int ordinal)
  {
    // Return fixed columns are not mapped to the underlying reader, so handle them directly
    if (ordinal == MReaderMapping.ColNumStartLine)
      return StartLineNumber;
    if (ordinal == MReaderMapping.ColNumEndLine)
      return EndLineNumber;
    if (ordinal == MReaderMapping.ColNumRecNum)
      return RecordNumber;
    // if mapped, use the underlying reader    
    return DataReader.GetInt64(MReaderMapping.ResultToSource(ordinal));
  }

  /// <inheritdoc />
  public override string GetName(int ordinal) => MReaderMapping.ResultingColumns[ordinal].Name;

  /// <inheritdoc />
  public override int GetOrdinal(string name) => MReaderMapping.GetOrdinal(name);

  /// <summary>
  /// Allocation-free column lookup via text spans.
  /// </summary>
  public int GetOrdinalSpan(ReadOnlySpan<char> name) => MReaderMapping.GetOrdinal(name);

  /// <inheritdoc />
  public override DataTable GetSchemaTable()
  {
    var dataTable = ReaderConstants.GetEmptySchemaTable();
    var schemaRow = ReaderConstants.GetDefaultSchemaRowArray();

    for (var col = 0; col < FieldCount; col++)
    {
      var column = MReaderMapping.ResultingColumns[col];
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
    // Error is fixed and not mapped to the underlying reader, so handle them directly
    => (ordinal == MReaderMapping.ColNumErrorField)
      ? RowErrorInformation
      : DataReader.GetString(MReaderMapping.ResultToSource(ordinal));

  /// <inheritdoc />
  public override object GetValue(int ordinal)
  {
    if (ordinal == MReaderMapping.ColNumStartLine)
      return StartLineNumber;
    if (ordinal == MReaderMapping.ColNumEndLine)
      return EndLineNumber;
    if (ordinal == MReaderMapping.ColNumRecNum)
      return RecordNumber;
    if (ordinal == MReaderMapping.ColNumErrorField)
      return RowErrorInformation;
    return DataReader.GetValue(MReaderMapping.ResultToSource(ordinal));
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
    if (ordinal == MReaderMapping.ColNumStartLine || ordinal == MReaderMapping.ColNumEndLine ||
        ordinal == MReaderMapping.ColNumRecNum)
    {
      return false;
    }

    return ordinal == MReaderMapping.ColNumErrorField
      ? RowErrorInformation.Length == 0
      : DataReader.IsDBNull(MReaderMapping.ResultToSource(ordinal));
  }

  /// <inheritdoc cref="IFileReader" />
  public override bool NextResult() => false;

  /// <inheritdoc />
  [Obsolete("No need to open a DataReaderWrapper, passed in reader is open already")]
  public async Task OpenAsync(CancellationToken cancellationToken)
  {
    if (m_FileReader != null)
      await m_FileReader.OpenAsync(cancellationToken).ConfigureAwait(false);
    if (OnOpenAsync !=null)
      await OnOpenAsync().ConfigureAwait(false);
    await ResetPositionToFirstDataRowAsync(cancellationToken).ConfigureAwait(false);
    OpenFinished?.SafeInvoke(this, MReaderMapping.ResultingColumns);
  }

  /// <inheritdoc cref="IDataReader" />
  public override bool Read() => ReadAsync(CancellationToken.None).GetAwaiter().GetResult();


  /// <inheritdoc cref="IFileReader" />
  public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
  {
    if (cancellationToken.IsCancellationRequested || EndOfFile)
      return false;

    if (DataReader is DbDataReader dbDataReader
          ? await dbDataReader.ReadAsync(cancellationToken).ConfigureAwait(false)
          : DataReader.Read())
    {
      RecordNumber++;

      // if we do have source field for error information, use this
      if (MReaderMapping.ColNumErrorFieldSource != -1)
      {
        if (!DataReader.IsDBNull(MReaderMapping.ColNumErrorFieldSource))
        {
          RowErrorInformation = DataReader.GetString(MReaderMapping.ColNumErrorFieldSource);
          if (RowErrorInformation.IsWarningMessage())
            NumberRowWarnings++;
        }
        else
        {
          RowErrorInformation = string.Empty;
        }
      }
      // If we have errors reported through HandleSourceWarning
      else if (m_ColumnErrorDictionary.Count>0)
      {
        // Get the error information from the Dictionary filled by the source reader warnings
        RowErrorInformation = ErrorInformation.ReadErrorInformation(m_ColumnErrorDictionary, i => MReaderMapping.ResultingColumns[i].Name);
        if (RowErrorInformation.IsWarningMessage())
          NumberRowWarnings++;
        m_ColumnErrorDictionary.Clear();
      }
      else
      {
        RowErrorInformation = string.Empty;
      }

      return true;
    }

    HandleReadFinished();
    return false;
  }

  /// <inheritdoc />
  public virtual ValueTask ResetPositionToFirstDataRowAsync(CancellationToken cancellationToken)
  {
    m_ColumnErrorDictionary.Clear();
    RowErrorInformation = string.Empty;
    RecordNumber = 0;
    NumberRowWarnings = 0;
    // Return the task directly from the reader to avoid a local async state machine
    return m_FileReader?.ResetPositionToFirstDataRowAsync(cancellationToken) ?? default;
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      if (m_FileReader != null && MReaderMapping.ColNumErrorFieldSource == -1)
        m_FileReader.Warning -= HandleSourceWarning;

      // This must ALWAYS run on an explicit app Dispose invocation
      DataReader.Dispose();
    }

    base.Dispose(disposing);
  }

  /// <summary>
  /// Handles the warnings raised in the source and adds them to the corresponding columns if so
  /// added to RowErrorInformation for the record
  /// </summary>
  /// <param name="sender">The sender.</param>
  /// <param name="e">The <see cref="WarningEventArgs"/> instance containing the event data.</param>
  private void HandleSourceWarning(object? sender, WarningEventArgs e)
  {
    int ownColumnIndex = -1;
    if (e.ColumnNumber >= 0)
      MReaderMapping.SourceToResult(e.ColumnNumber, out ownColumnIndex);

    m_ColumnErrorDictionary[ownColumnIndex]= e.Message.AsMemory();
    Warning?.Invoke(this, new WarningEventArgs(RecordNumber, ownColumnIndex, e.Message, StartLineNumber, EndLineNumber, GetColumn(ownColumnIndex).Name ?? string.Empty));
  }
}