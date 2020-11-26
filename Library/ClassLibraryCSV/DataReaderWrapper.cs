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
    [CanBeNull] protected readonly IFileReader FileReader;
    private readonly long m_RecordLimit;
    [NotNull] public readonly ReaderMapping ReaderMapping;
    [NotNull] protected IDataReader DataReader;

    public DataReaderWrapper([NotNull] IDataReader reader, long recordLimit = 0, bool addErrorField = false,
                             bool addStartLine = false, bool addEndLine = false, bool addRecNum = false)
    {
      DataReader = reader ?? throw new ArgumentNullException(nameof(reader));
      FileReader = reader as IFileReader;
      if (reader.IsClosed)
        throw new ArgumentException("Reader must be opened");
      m_RecordLimit = recordLimit < 1 ? long.MaxValue : recordLimit;
      ReaderMapping = new ReaderMapping(DataReader, addStartLine, addRecNum, addEndLine, addErrorField);
    }

    public override int Depth => FieldCount;

    public long EndLineNumber => FileReader?.EndLineNumber ?? RecordNumber;

    public bool EndOfFile => FileReader?.EndOfFile ?? DataReader.IsClosed || RecordNumber >= m_RecordLimit;

    public override int FieldCount => ReaderMapping.Column.Count;

    public override bool HasRows => !DataReader.IsClosed;

    public override bool IsClosed => DataReader.IsClosed;

    public virtual int Percent => (FileReader != null && FileReader.Percent != 0 && FileReader.Percent != 100) ? FileReader.Percent : RecordNumber > 0 ? (int) (RecordNumber / (double) m_RecordLimit * 100d) : 0;

    public long RecordNumber { get; protected set; }

    public override int RecordsAffected => m_RecordLimit.ToInt();

    public long StartLineNumber => FileReader?.StartLineNumber ?? RecordNumber;

    public override object this[int ordinal] => GetValue(ordinal);

    public override object this[string name] => GetValue(GetOrdinal(name));

    public override void Close()
    {
      base.Close();
      DataReader.Close();
    }

    public override bool GetBoolean(int ordinal) => DataReader.GetBoolean(ReaderMapping.DataTableToReader(ordinal));

    public override byte GetByte(int ordinal) => DataReader.GetByte(ReaderMapping.DataTableToReader(ordinal));

    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) =>
      DataReader.GetBytes(ReaderMapping.DataTableToReader(ordinal), dataOffset, buffer, bufferOffset, length);

    public override char GetChar(int ordinal) => DataReader.GetChar(ReaderMapping.DataTableToReader(ordinal));

    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) =>
      DataReader.GetChars(ReaderMapping.DataTableToReader(ordinal), dataOffset, buffer, bufferOffset, length);

    public new IDataReader GetData(int i) => DataReader.GetData(i);

    [NotNull]
    public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

    public override DateTime GetDateTime(int ordinal) => DataReader.GetDateTime(ReaderMapping.DataTableToReader(ordinal));

    public override decimal GetDecimal(int ordinal) => DataReader.GetDecimal(ReaderMapping.DataTableToReader(ordinal));

    public override double GetDouble(int ordinal) => DataReader.GetDouble(ReaderMapping.DataTableToReader(ordinal));

    public override IEnumerator GetEnumerator() => new DbEnumerator(DataReader, false);

    [NotNull]
    public override Type GetFieldType(int ordinal) => ReaderMapping.Column[ordinal].ValueFormat.DataType.GetNetType();

    public override float GetFloat(int ordinal) => DataReader.GetFloat(ReaderMapping.DataTableToReader(ordinal));

    public override Guid GetGuid(int ordinal) => DataReader.GetGuid(ReaderMapping.DataTableToReader(ordinal));

    public override short GetInt16(int ordinal) => DataReader.GetInt16(ReaderMapping.DataTableToReader(ordinal));

    public override int GetInt32(int ordinal) => DataReader.GetInt32(ReaderMapping.DataTableToReader(ordinal));

    public override long GetInt64(int columnNumber) =>
      columnNumber == ReaderMapping.DataTableStartLine ? StartLineNumber :
      columnNumber == ReaderMapping.DataTableEndLine ? EndLineNumber :
      columnNumber == ReaderMapping.DataTableRecNum ? RecordNumber :
      DataReader.GetInt64(ReaderMapping.DataTableToReader(columnNumber));

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
        schemaRow[5] = col;         // Column ordinal

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
        return RecordNumber;
      if (columnNumber == ReaderMapping.DataTableErrorField)
        return ReaderMapping.RowErrorInformation;

      return DataReader.GetValue(ReaderMapping.DataTableToReader(columnNumber));
    }

    public override int GetValues(object[] values) => DataReader.GetValues(values);

    public override bool IsDBNull(int columnNumber) =>
      columnNumber != ReaderMapping.DataTableStartLine && columnNumber != ReaderMapping.DataTableEndLine &&
      columnNumber != ReaderMapping.DataTableRecNum &&
      (columnNumber == ReaderMapping.DataTableErrorField
         ? (ReaderMapping.ColumnErrorDictionary?.Count ?? 0) == 0
         : DataReader.IsDBNull(ReaderMapping.DataTableToReader(columnNumber)));

    public override bool NextResult() => false;

    public override bool Read() => ReadAsync(CancellationToken.None).Wait(2000);

    public override async Task<bool> ReadAsync(CancellationToken token)
    {
      ReaderMapping.ColumnErrorDictionary?.Clear();
      var couldRead = (DataReader is DbDataReader dbDataReader) ? await dbDataReader.ReadAsync(token).ConfigureAwait(false) : DataReader.Read();

      if (couldRead)
        RecordNumber++;
      return couldRead && RecordNumber <= m_RecordLimit;
    }

    public int ReaderToDataTable(int readerColumn) => ReaderMapping.ReaderToDataTable(readerColumn);
  }
}