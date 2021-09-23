using System;
using System.Collections.Generic;
using System.Data;

namespace CsvTools
{
  /// <summary>
  ///   Handles mapping of a data reader to a resulting data reader columns ignored will be omitted
  ///   and artificial columns for Line, record and error information is added
  /// </summary>
  public class ReaderMapping
  {
    public readonly ColumnErrorDictionary? ColumnErrorDictionary;

    public readonly int DataTableEndLine;

    public readonly int DataTableErrorField;

    public readonly int DataTableRecNum;

    public readonly int DataTableStartLine;

    private readonly List<ImmutableColumn> m_ReaderColumnNotIgnored = new List<ImmutableColumn>();

    private readonly BiDirectionalDictionary<int, int> m_Mapping = new BiDirectionalDictionary<int, int>();

    private readonly List<string> m_ReaderColumnsAll = new List<string>();

    /// <summary>
    ///   Maps the columns of the data reader for an reader warpper, taking care of ignored and
    ///   artifical columns
    /// </summary>
    /// <param name="dataReader">
    ///   <see cref="IDataRecord" /> usually a <see cref="IFileReader" /> or <see cref="IDataReader" />
    /// </param>
    /// <param name="addStartLine">Add artificial field Start Line</param>
    /// <param name="addRecNum">Add artificial field Records Number</param>
    /// <param name="addEndLine">Add artificial field End Line</param>
    /// <param name="addErrorField">Add artificial field Error</param>
    public ReaderMapping(
      in IDataRecord dataReader,
      bool addStartLine,
      bool addRecNum,
      bool addEndLine,
      bool addErrorField)
    {
      var fileReader = dataReader as IFileReader;
      if (fileReader != null)
        ColumnErrorDictionary = new ColumnErrorDictionary(fileReader);

      var readerColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var fieldCount = 0;
      for (var col = 0; col < dataReader.FieldCount; col++)
      {
        ImmutableColumn column;
        if (fileReader != null)
        {
          var iColumn = fileReader.GetColumn(col);
          if (iColumn is ImmutableColumn imcol)
            column = imcol;
          else
            column = new ImmutableColumn(iColumn);
        }
        else
        {
          column = new ImmutableColumn(
            dataReader.GetName(col),
            new ImmutableValueFormat(dataReader.GetFieldType(col).GetDataType()),
            col);
        }

        m_ReaderColumnsAll.Add(column.Name);
        if (column.Ignore) continue;
        m_ReaderColumnNotIgnored.Add(column);
        readerColumns.Add(column.Name);
        m_Mapping.Add(col, fieldCount++);
      }

      // add fields
      if (addStartLine && !readerColumns.Contains(ReaderConstants.cStartLineNumberFieldName))
      {
        DataTableStartLine = fieldCount++;
        m_ReaderColumnNotIgnored.Add(
          new ImmutableColumn(
            ReaderConstants.cStartLineNumberFieldName,
            new ImmutableValueFormat(DataType.Integer),
            DataTableStartLine));
      }
      else
      {
        DataTableStartLine = -1;
      }

      if (addRecNum && !readerColumns.Contains(ReaderConstants.cRecordNumberFieldName))
      {
        DataTableRecNum = fieldCount++;
        m_ReaderColumnNotIgnored.Add(
          new ImmutableColumn(
            ReaderConstants.cRecordNumberFieldName,
            new ImmutableValueFormat(DataType.Integer),
            DataTableRecNum));
      }
      else
      {
        DataTableRecNum = -1;
      }

      if (addEndLine && !readerColumns.Contains(ReaderConstants.cEndLineNumberFieldName))
      {
        DataTableEndLine = fieldCount++;
        m_ReaderColumnNotIgnored.Add(
          new ImmutableColumn(
            ReaderConstants.cEndLineNumberFieldName,
            new ImmutableValueFormat(DataType.Integer),
            DataTableEndLine));
      }
      else
      {
        DataTableEndLine = -1;
      }

      if (addErrorField && !readerColumns.Contains(ReaderConstants.cErrorField))
      {
        DataTableErrorField = fieldCount;
        m_ReaderColumnNotIgnored.Add(
          new ImmutableColumn(ReaderConstants.cErrorField, new ImmutableValueFormat(), DataTableErrorField));
      }
      else
      {
        DataTableErrorField = -1;
      }
    }

    public IReadOnlyList<ImmutableColumn> Column => m_ReaderColumnNotIgnored;

    public string? RowErrorInformation =>
      ColumnErrorDictionary is null || ColumnErrorDictionary.Count == 0
        ? null
        : ErrorInformation.ReadErrorInformation(ColumnErrorDictionary, m_ReaderColumnsAll);

    public int DataTableToReader(int tableColumn) => m_Mapping.GetByValue(tableColumn);

    public int ReaderToDataTable(int readerColumn) => m_Mapping[readerColumn];
  }
}