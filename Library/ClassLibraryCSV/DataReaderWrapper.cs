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
  ///   Wrapper around another FileReader adding artificial fields and removing ignored columns
  /// </summary>
  /// <remarks>Introduced to allow a stream into SQLBulkCopy and possibly replace CopyToDataTableInfo</remarks>
  public class DataReaderWrapper : DbDataReader
  {
    public readonly ColumnErrorDictionary ColumnErrorDictionary;
    private readonly bool m_AddEndLine;
    private readonly bool m_AddRecNum;
    private readonly bool m_AddStartLine;
    private readonly IFileReader m_FileReader;
    private readonly bool m_IncludeErrorField;
    private readonly BiDirectionalDictionary<int, int> m_Mapping = new BiDirectionalDictionary<int, int>();
    private readonly IList<string> m_ReaderColumns = new List<string>();
    private readonly long m_RecordLimit;

    private int m_ColEndLine = -1;
    private int m_ColErrorField = -1;
    private int m_ColRecNum = -1;
    private int m_ColStartLine = -1;

    public int GetColumnIndexFromErrorColumn(int errorCol) => m_Mapping[errorCol];

    /// <summary>
    ///   An array of column
    /// </summary>
    private readonly List<ImmutableColumn> m_Column = new List<ImmutableColumn>();

    private int m_FieldCount;

    public DataReaderWrapper([NotNull] IFileReader reader, long recordLimit = 0, bool includeErrorField = false,
      bool addStartLine = false,
      bool addEndLine = false, bool addRecNum = false)
    {
      m_FileReader = reader ?? throw new ArgumentNullException(nameof(reader));
      ColumnErrorDictionary = new ColumnErrorDictionary(reader);
      m_AddStartLine = addStartLine;
      m_AddEndLine = addEndLine;
      m_AddRecNum = addRecNum;
      m_IncludeErrorField = includeErrorField;
      m_RecordLimit = recordLimit<1 ? long.MaxValue : recordLimit;
    }

    public override bool HasRows => m_FileReader.EndOfFile;
    public override bool IsClosed => m_FileReader.IsClosed;
    public long RecordNumber { get; private set; }

    public override int RecordsAffected => RecordNumber.ToInt();
    public override int FieldCount => m_FieldCount;

    public override object this[int ordinal] => GetValue(ordinal);

    public override object this[string name] => GetValue(GetOrdinal(name));

    public override int Depth => FieldCount;

    public override short GetInt16(int ordinal) => m_FileReader.GetInt16(m_Mapping.GetByValue(ordinal));

    public override int GetInt32(int columnNumber) => GetInt64(columnNumber).ToInt();

    public override long GetInt64(int columnNumber) =>
      columnNumber == m_ColStartLine ? m_FileReader.StartLineNumber :
      columnNumber == m_ColEndLine ? m_FileReader.EndLineNumber :
      columnNumber == m_ColRecNum ? m_FileReader.RecordNumber :
      m_FileReader.GetInt64(m_Mapping.GetByValue(columnNumber));

    public override float GetFloat(int ordinal) => m_FileReader.GetFloat(m_Mapping.GetByValue(ordinal));

    public override Guid GetGuid(int ordinal) => m_FileReader.GetGuid(m_Mapping.GetByValue(ordinal));

    public override double GetDouble(int ordinal) => m_FileReader.GetDouble(m_Mapping.GetByValue(ordinal));

    public override string GetString(int columnNumber) =>
      columnNumber == m_ColStartLine
        ? m_FileReader.StartLineNumber.ToString()
        : columnNumber == m_ColEndLine
          ? m_FileReader.EndLineNumber.ToString()
          : columnNumber == m_ColRecNum
            ? m_FileReader.RecordNumber.ToString()
            : columnNumber == m_ColErrorField
              ? ColumnErrorDictionary.Count == 0
                ? null
                : ErrorInformation.ReadErrorInformation(ColumnErrorDictionary, m_ReaderColumns)
              : m_FileReader.GetString(m_Mapping.GetByValue(columnNumber));

    public override decimal GetDecimal(int ordinal) => m_FileReader.GetDecimal(m_Mapping.GetByValue(ordinal));

    public override DateTime GetDateTime(int ordinal) => m_FileReader.GetDateTime(m_Mapping.GetByValue(ordinal));

    public override string GetName(int ordinal) => m_Column[ordinal].Name;

    public override int GetValues(object[] values) => throw new NotImplementedException();

    public override int GetOrdinal(string columnName)
    {
      if (string.IsNullOrEmpty(columnName) || m_Column == null)
        return -1;
      var count = 0;
      foreach (var column in m_Column)
      {
        if (columnName.Equals(column.Name, StringComparison.OrdinalIgnoreCase))
          return count;
        count++;
      }

      return -1;
    }

    public override bool GetBoolean(int ordinal) => m_FileReader.GetBoolean(m_Mapping.GetByValue(ordinal));

    public override byte GetByte(int ordinal) => m_FileReader.GetByte(m_Mapping.GetByValue(ordinal));

    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) =>
      m_FileReader.GetBytes(m_Mapping.GetByValue(ordinal), dataOffset, buffer, bufferOffset, length);

    public override char GetChar(int ordinal) => m_FileReader.GetChar(m_Mapping.GetByValue(ordinal));

    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) =>
      m_FileReader.GetChars(m_Mapping.GetByValue(ordinal), dataOffset, buffer, bufferOffset, length);

    [NotNull] public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

    [NotNull] public override Type GetFieldType(int ordinal) => m_Column[ordinal].ValueFormat.DataType.GetNetType();

    public override IEnumerator GetEnumerator() => throw new NotImplementedException();

    public override object GetValue(int columnNumber)
    {
      if (columnNumber == m_ColStartLine)
        return m_FileReader.StartLineNumber;
      if (columnNumber == m_ColEndLine)
        return m_FileReader.EndLineNumber;
      if (columnNumber == m_ColRecNum)
        return m_FileReader.RecordNumber;
      if (columnNumber == m_ColErrorField)
        return ColumnErrorDictionary.Count == 0 ? null : ErrorInformation.ReadErrorInformation(ColumnErrorDictionary, m_ReaderColumns);

      return m_FileReader.GetValue(m_Mapping.GetByValue(columnNumber));
    }

    public override bool IsDBNull(int columnNumber) =>
      columnNumber != m_ColStartLine && columnNumber != m_ColEndLine && columnNumber != m_ColRecNum &&
      (columnNumber == m_ColErrorField
        ? ColumnErrorDictionary.Count == 0
        : m_FileReader.IsDBNull(m_Mapping.GetByValue(columnNumber)));

    [NotNull]
    public override DataTable GetSchemaTable()
    {
      var dataTable = ReaderConstants.GetEmptySchemaTable();
      var schemaRow = ReaderConstants.GetDefaultSchemaRowArray();

      for (var col = 0; col < FieldCount; col++)
      {
        var column = m_Column[col];

        schemaRow[1] = column.Name; // Column name
        schemaRow[4] = column.Name; // Column name
        schemaRow[5] = col; // Column ordinal

        if (col == m_ColStartLine || col == m_ColRecNum || col == m_ColEndLine)
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

        // set Unique and key
        if (col == m_ColStartLine && !m_AddRecNum || col == m_ColRecNum)
        {
        }

        dataTable.Rows.Add(schemaRow);
      }

      return dataTable;
    }

    public override bool NextResult() => false;

    public override bool Read() => ReadAsync(CancellationToken.None).Wait(2000);

    public async Task OpenAsync(CancellationToken token)
    {
      try
      {
        if (m_FileReader.IsClosed)
          await m_FileReader.OpenAsync(token).ConfigureAwait(false);
        RecordNumber = 0;

        m_ReaderColumns.Clear();
        m_Mapping.Clear();
        m_FieldCount = 0;
        m_Column.Clear();
        foreach (var column in m_FileReader.GetColumnsOfReader())
        {
          m_Column.Add(new ImmutableColumn(column, m_FieldCount));
          m_ReaderColumns.Add(column.Name);
          m_Mapping.Add(column.ColumnOrdinal, m_FieldCount++);
        }

        // add fields
        if (m_AddStartLine && !m_ReaderColumns.Contains(ReaderConstants.cStartLineNumberFieldName))
        {
          m_ColStartLine = m_FieldCount++;
          m_Column.Add(new ImmutableColumn(ReaderConstants.cStartLineNumberFieldName, new ImmutableValueFormat(DataType.Integer), m_ColStartLine));
        }

        if (m_AddRecNum && !m_ReaderColumns.Contains(ReaderConstants.cRecordNumberFieldName))
        {
          m_ColRecNum = m_FieldCount++;
          m_Column.Add(new ImmutableColumn(ReaderConstants.cRecordNumberFieldName, new ImmutableValueFormat(DataType.Integer), m_ColRecNum));
        }

        if (m_AddEndLine && !m_ReaderColumns.Contains(ReaderConstants.cEndLineNumberFieldName))
        {
          m_ColEndLine = m_FieldCount++;
          m_Column.Add(new ImmutableColumn(ReaderConstants.cEndLineNumberFieldName, new ImmutableValueFormat(DataType.Integer), m_ColEndLine));
        }

        if (m_IncludeErrorField && !m_ReaderColumns.Contains(ReaderConstants.cErrorField))
        {
          m_ColErrorField = m_FieldCount++;
          m_Column.Add(new ImmutableColumn(ReaderConstants.cErrorField, new ImmutableValueFormat(), m_ColErrorField));
        }
      }
      catch (Exception)
      {
        Close();
        throw;
      }
    }

    public override async Task<bool> ReadAsync(CancellationToken token)
    {
      if (!token.IsCancellationRequested && !IsClosed)
      {
        ColumnErrorDictionary.Clear();
        var couldRead = await m_FileReader.ReadAsync(token).ConfigureAwait(false);
        if (couldRead) RecordNumber++;
        if (RecordNumber<=m_RecordLimit)
          return couldRead;
      }
      return false;
    }
  }
}