using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;

namespace CsvTools
{
  /// <summary>
  /// Handles mapping of a data ereader to a resulting data reader
  /// Columns ignored will be obmitted and atrifival columns for Lne, record and error inormation is added
  /// </summary>
  public class ReaderMapping
  {
    [CanBeNull]
    public readonly ColumnErrorDictionary ColumnErrorDictionary;
    public readonly int DataTableEndLine;
    public readonly int DataTableErrorField;
    public readonly int DataTableRecNum;
    public readonly int DataTableStartLine;
    private readonly List<ImmutableColumn> m_DateTableColumns = new List<ImmutableColumn>();
    private readonly BiDirectionalDictionary<int, int> m_Mapping = new BiDirectionalDictionary<int, int>();
    private readonly List<string> m_ReaderColumnsAll = new List<string>();

    public ReaderMapping(IDataReader dataReader, bool addStartLine, bool addRecNum, bool addEndLine,
      bool addErrorField)
    {
      var fileReader = dataReader as IFileReader;
      if (fileReader != null)
        ColumnErrorDictionary = new ColumnErrorDictionary(fileReader);

      var m_ReaderColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var fieldCount = 0;
      for (var col = 0; col < dataReader.FieldCount; col++)
      {
        var column = (fileReader==null) ?
          new ImmutableColumn(dataReader.GetName(col), new ImmutableValueFormat(dataReader.GetFieldType(col).GetDataType()), col) :
          new ImmutableColumn(fileReader.GetColumn(col), col);

        m_ReaderColumnsAll.Add(column.Name);
        if (column.Ignore) continue;
        m_DateTableColumns.Add(column);
        m_ReaderColumns.Add(column.Name);
        m_Mapping.Add(column.ColumnOrdinal, fieldCount++);
      }

      // add fields
      if (addStartLine && !m_ReaderColumns.Contains(ReaderConstants.cStartLineNumberFieldName))
      {
        DataTableStartLine = fieldCount++;
        m_DateTableColumns.Add(new ImmutableColumn(ReaderConstants.cStartLineNumberFieldName,
          new ImmutableValueFormat(DataType.Integer), DataTableStartLine));
      }
      else
      {
        DataTableStartLine = -1;
      }

      if (addRecNum && !m_ReaderColumns.Contains(ReaderConstants.cRecordNumberFieldName))
      {
        DataTableRecNum = fieldCount++;
        m_DateTableColumns.Add(new ImmutableColumn(ReaderConstants.cRecordNumberFieldName,
          new ImmutableValueFormat(DataType.Integer), DataTableRecNum));
      }
      else
      {
        DataTableRecNum = -1;
      }

      if (addEndLine && !m_ReaderColumns.Contains(ReaderConstants.cEndLineNumberFieldName))
      {
        DataTableEndLine = fieldCount++;
        m_DateTableColumns.Add(new ImmutableColumn(ReaderConstants.cEndLineNumberFieldName,
          new ImmutableValueFormat(DataType.Integer), DataTableEndLine));
      }
      else
      {
        DataTableEndLine = -1;
      }

      if (addErrorField && !m_ReaderColumns.Contains(ReaderConstants.cErrorField))
      {
        DataTableErrorField = fieldCount;
        m_DateTableColumns.Add(new ImmutableColumn(ReaderConstants.cErrorField, new ImmutableValueFormat(),
          DataTableErrorField));
      }
      else
      {
        DataTableErrorField = -1;
      }
    }


    public string RowErrorInformation =>
      ColumnErrorDictionary == null  || ColumnErrorDictionary.Count == 0
        ? null
        : ErrorInformation.ReadErrorInformation(ColumnErrorDictionary, m_ReaderColumnsAll);

    public IReadOnlyList<ImmutableColumn> Column => m_DateTableColumns;

    public int DataTableToReader(int tableColumn) => m_Mapping.GetByValue(tableColumn);
    public int ReaderToDataTable(int readerColumn) => m_Mapping[readerColumn];
  }
}