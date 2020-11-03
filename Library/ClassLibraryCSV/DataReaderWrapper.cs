using JetBrains.Annotations;
using System;
using System.Collections;
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
    public readonly ReaderMapping ReaderMapping;
    [CanBeNull] protected readonly IFileReader m_FileReader;
    [NotNull] protected IDataReader m_DataReader;
    protected long m_RecordNumber;
    private readonly long m_RecordLimit;

    public DataReaderWrapper([NotNull] IDataReader reader, long recordLimit = 0, bool addErrorField = false,
          bool addStartLine = false, bool addEndLine = false, bool addRecNum = false)
    {
      m_DataReader = reader ?? throw new ArgumentNullException(nameof(reader));
      m_FileReader = reader as IFileReader;
      if (reader.IsClosed)
        throw new ArgumentException("Reader must be opened");
      m_RecordLimit = recordLimit < 1 ? long.MaxValue : recordLimit;
      ReaderMapping = new ReaderMapping(m_DataReader, addStartLine, addRecNum, addEndLine, addErrorField);
    }

    public override int Depth => FieldCount;

    public long EndLineNumber => m_FileReader?.EndLineNumber ?? m_RecordNumber;

    public bool EndOfFile => m_FileReader?.EndOfFile ?? m_DataReader.IsClosed || m_RecordNumber >= m_RecordLimit;

    public override int FieldCount => ReaderMapping.Column.Count;

    public override bool HasRows => !m_DataReader.IsClosed;

    public override bool IsClosed => m_DataReader.IsClosed;

    public virtual int Percent => RecordNumber > 0 ? (int) (RecordNumber / (double) m_RecordLimit * 100d) : 0;

    public long RecordNumber => m_RecordNumber;

    public override int RecordsAffected => m_RecordLimit.ToInt();

    public long StartLineNumber => m_FileReader?.StartLineNumber ?? m_RecordNumber;

    public override object this[int ordinal] => GetValue(ordinal);

    public override object this[string name] => GetValue(GetOrdinal(name));

    public override void Close()
    {
      base.Close();
      m_DataReader.Close();
    }
    public override bool GetBoolean(int ordinal) => m_DataReader.GetBoolean(ReaderMapping.DataTableToReader(ordinal));

    public override byte GetByte(int ordinal) => m_DataReader.GetByte(ReaderMapping.DataTableToReader(ordinal));

    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) =>
      m_DataReader.GetBytes(ReaderMapping.DataTableToReader(ordinal), dataOffset, buffer, bufferOffset, length);

    public override char GetChar(int ordinal) => m_DataReader.GetChar(ReaderMapping.DataTableToReader(ordinal));

    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) =>
      m_DataReader.GetChars(ReaderMapping.DataTableToReader(ordinal), dataOffset, buffer, bufferOffset, length);

    public new IDataReader GetData(int i) => m_DataReader.GetData(i);

    [NotNull]
    public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

    public override DateTime GetDateTime(int ordinal) => m_DataReader.GetDateTime(ReaderMapping.DataTableToReader(ordinal));

    public override decimal GetDecimal(int ordinal) => m_DataReader.GetDecimal(ReaderMapping.DataTableToReader(ordinal));

    public override double GetDouble(int ordinal) => m_DataReader.GetDouble(ReaderMapping.DataTableToReader(ordinal));

    public override IEnumerator GetEnumerator() => new DbEnumerator(m_DataReader, false);

    [NotNull]
    public override Type GetFieldType(int ordinal) => ReaderMapping.Column[ordinal].ValueFormat.DataType.GetNetType();

    public override float GetFloat(int ordinal) => m_DataReader.GetFloat(ReaderMapping.DataTableToReader(ordinal));

    public override Guid GetGuid(int ordinal) => m_DataReader.GetGuid(ReaderMapping.DataTableToReader(ordinal));

    public override short GetInt16(int ordinal) => m_DataReader.GetInt16(ReaderMapping.DataTableToReader(ordinal));

    public override int GetInt32(int ordinal) => m_DataReader.GetInt32(ReaderMapping.DataTableToReader(ordinal));

    public override long GetInt64(int columnNumber) =>
      columnNumber == ReaderMapping.DataTableStartLine ? StartLineNumber :
      columnNumber == ReaderMapping.DataTableEndLine ? EndLineNumber :
      columnNumber == ReaderMapping.DataTableRecNum ? m_RecordNumber :
      m_DataReader.GetInt64(ReaderMapping.DataTableToReader(columnNumber));
    public override string GetName(int ordinal) => ReaderMapping.Column[ordinal].Name;

    public override int GetOrdinal(string columnName)
    {
      if (string.IsNullOrEmpty(columnName) || ReaderMapping.Column == null)
        return -1;
      var count = 0;
      foreach (var column in ReaderMapping.Column)
      {
        if (columnName.Equals(column.Name, StringComparison.OrdinalIgnoreCase))
          return count;
        count++;
      }

      return -1;
    }

    [NotNull]
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

        if (col == ReaderMapping.DataTableStartLine || col == ReaderMapping.DataTableRecNum ||
            col == ReaderMapping.DataTableEndLine)
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

    public override string GetString(int columnNumber) => GetValue(columnNumber).ToString();

    public override object GetValue(int columnNumber)
    {
      if (columnNumber == ReaderMapping.DataTableStartLine)
        return StartLineNumber;
      if (columnNumber == ReaderMapping.DataTableEndLine)
        return EndLineNumber;
      if (columnNumber == ReaderMapping.DataTableRecNum)
        return m_RecordNumber;
      if (columnNumber == ReaderMapping.DataTableErrorField)
        return ReaderMapping.RowErrorInformation;

      return m_DataReader.GetValue(ReaderMapping.DataTableToReader(columnNumber));
    }

    public override int GetValues(object[] values) => m_DataReader.GetValues(values);

    public override bool IsDBNull(int columnNumber) =>
        columnNumber != ReaderMapping.DataTableStartLine && columnNumber != ReaderMapping.DataTableEndLine &&
        columnNumber != ReaderMapping.DataTableRecNum &&
        (columnNumber == ReaderMapping.DataTableErrorField
          ? ReaderMapping.ColumnErrorDictionary.Count == 0
          : m_DataReader.IsDBNull(ReaderMapping.DataTableToReader(columnNumber)));

    public override bool NextResult() => false;

    public override bool Read() => ReadAsync(CancellationToken.None).Wait(2000);

    public override async Task<bool> ReadAsync(CancellationToken token)
    {
      ReaderMapping.ColumnErrorDictionary?.Clear();
      var couldRead = (m_DataReader is DbDataReader dbDataReader) ? await dbDataReader.ReadAsync(token).ConfigureAwait(false) : m_DataReader.Read();

      if (couldRead)
        m_RecordNumber++;
      return couldRead && m_RecordNumber <= m_RecordLimit;
    }

    public int ReaderToDataTable(int readerColumn) => ReaderMapping.ReaderToDataTable(readerColumn);
  }
}