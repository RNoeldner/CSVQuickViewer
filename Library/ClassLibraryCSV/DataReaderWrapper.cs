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
    private readonly IFileReader m_FileReader;
    private readonly long m_RecordLimit;
    public readonly ReaderMapping ReaderMapping;

    public DataReaderWrapper([NotNull] IFileReader reader, long recordLimit = 0, bool includeErrorField = false,
      bool addStartLine = false,
      bool addEndLine = false, bool addRecNum = false)
    {
      m_FileReader = reader ?? throw new ArgumentNullException(nameof(reader));
      if (reader.IsClosed)
        throw new ArgumentException("Reader must be opened");
      m_RecordLimit = recordLimit < 1 ? long.MaxValue : recordLimit;
      ReaderMapping = new ReaderMapping(m_FileReader, addStartLine, addRecNum, addEndLine, includeErrorField);
    }

    public override bool HasRows => !m_FileReader.EndOfFile;

    public override bool IsClosed => m_FileReader.IsClosed;

    public int Percent => m_FileReader.Percent;

    public bool EndOfFile => RecordNumber > m_RecordLimit || m_FileReader.EndOfFile;

    public long RecordNumber => m_FileReader.RecordNumber;

    public override int RecordsAffected => m_FileReader.RecordNumber.ToInt();

    public override int FieldCount => ReaderMapping.Column.Count;

    public override object this[int ordinal] => GetValue(ordinal);

    public override object this[string name] => GetValue(GetOrdinal(name));

    public override int Depth => FieldCount;

    public override short GetInt16(int ordinal) => m_FileReader.GetInt16(ReaderMapping.ReaderColumn(ordinal));

    public override int GetInt32(int columnNumber) => GetInt64(columnNumber).ToInt();

    public override long GetInt64(int columnNumber) =>
      columnNumber == ReaderMapping.DataTableStartLine ? m_FileReader.StartLineNumber :
      columnNumber == ReaderMapping.DataTableEndLine ? m_FileReader.EndLineNumber :
      columnNumber == ReaderMapping.DataTableRecNum ? m_FileReader.RecordNumber :
      m_FileReader.GetInt64(ReaderMapping.ReaderColumn(columnNumber));

    public override float GetFloat(int ordinal) => m_FileReader.GetFloat(ReaderMapping.ReaderColumn(ordinal));

    public override Guid GetGuid(int ordinal) => m_FileReader.GetGuid(ReaderMapping.ReaderColumn(ordinal));

    public override double GetDouble(int ordinal) => m_FileReader.GetDouble(ReaderMapping.ReaderColumn(ordinal));

    public override string GetString(int columnNumber) =>
      columnNumber == ReaderMapping.DataTableStartLine
        ? m_FileReader.StartLineNumber.ToString()
        : columnNumber == ReaderMapping.DataTableEndLine
          ? m_FileReader.EndLineNumber.ToString()
          : columnNumber == ReaderMapping.DataTableRecNum
            ? m_FileReader.RecordNumber.ToString()
            : columnNumber == ReaderMapping.DataTableErrorField
              ? ReaderMapping.RowErrorInformation
              : m_FileReader.GetString(ReaderMapping.ReaderColumn(columnNumber));

    public override decimal GetDecimal(int ordinal) => m_FileReader.GetDecimal(ReaderMapping.ReaderColumn(ordinal));

    public override DateTime GetDateTime(int ordinal) => m_FileReader.GetDateTime(ReaderMapping.ReaderColumn(ordinal));

    public override string GetName(int ordinal) => ReaderMapping.Column[ordinal].Name;

    public override int GetValues(object[] values) => throw new NotImplementedException();

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

    public override bool GetBoolean(int ordinal) => m_FileReader.GetBoolean(ReaderMapping.ReaderColumn(ordinal));

    public override byte GetByte(int ordinal) => m_FileReader.GetByte(ReaderMapping.ReaderColumn(ordinal));

    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) =>
      m_FileReader.GetBytes(ReaderMapping.ReaderColumn(ordinal), dataOffset, buffer, bufferOffset, length);

    public override char GetChar(int ordinal) => m_FileReader.GetChar(ReaderMapping.ReaderColumn(ordinal));

    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) =>
      m_FileReader.GetChars(ReaderMapping.ReaderColumn(ordinal), dataOffset, buffer, bufferOffset, length);

    [NotNull]
    public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

    public override IEnumerator GetEnumerator() => throw new NotImplementedException();

    [NotNull]
    public override Type GetFieldType(int ordinal) => ReaderMapping.Column[ordinal].ValueFormat.DataType.GetNetType();

    public override object GetValue(int columnNumber)
    {
      if (columnNumber == ReaderMapping.DataTableStartLine)
        return m_FileReader.StartLineNumber;
      if (columnNumber == ReaderMapping.DataTableEndLine)
        return m_FileReader.EndLineNumber;
      if (columnNumber == ReaderMapping.DataTableRecNum)
        return m_FileReader.RecordNumber;
      if (columnNumber == ReaderMapping.DataTableErrorField)
        return ReaderMapping.RowErrorInformation;

      return m_FileReader.GetValue(ReaderMapping.ReaderColumn(columnNumber));
    }

    public override bool IsDBNull(int columnNumber) =>
      columnNumber != ReaderMapping.DataTableStartLine && columnNumber != ReaderMapping.DataTableEndLine &&
      columnNumber != ReaderMapping.DataTableRecNum &&
      (columnNumber == ReaderMapping.DataTableErrorField
        ? ReaderMapping.ColumnErrorDictionary.Count == 0
        : m_FileReader.IsDBNull(ReaderMapping.ReaderColumn(columnNumber)));

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

    public override bool NextResult() => false;

    public override bool Read() => ReadAsync(CancellationToken.None).Wait(2000);

    public override async Task<bool> ReadAsync(CancellationToken token)
    {
      ReaderMapping.ColumnErrorDictionary.Clear();
      var couldRead = await m_FileReader.ReadAsync(token).ConfigureAwait(false);
      return couldRead && RecordNumber <= m_RecordLimit;
    }


    public int GetColumnIndexFromErrorColumn(int errorCol) => ReaderMapping.DataTableColumn(errorCol);
  }
}