#nullable enable
using System;
using System.Collections.Generic;
using System.Data;

namespace CsvTools
{
  /// <summary>
  ///   Handles mapping of a data reader to a resulting data reader columns ignored will be omitted
  ///   and artificial columns for Line, record and error information is added
  /// </summary>
  public sealed class ReaderMapping
  {
    public readonly int DataTableEndLine;

    public readonly int DataTableErrorField;

    public readonly int DataTableRecNum;

    public readonly int DataTableStartLine;
    private readonly ColumnErrorDictionary? m_ColumnErrorDictionary;

    private readonly BiDirectionalDictionary<int, int> m_Mapping = new BiDirectionalDictionary<int, int>();

    private readonly List<ImmutableColumn> m_ReaderColumnNotIgnored = new List<ImmutableColumn>();

    private readonly List<string> m_ReaderColumnsAll = new List<string>();

    /// <summary>
    ///   Maps the columns of the data reader for an reader wrapper, taking care of ignored and
    ///   artificial columns
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
        m_ColumnErrorDictionary = new ColumnErrorDictionary(fileReader);

      var readerColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var fieldCount = 0;
      for (var col = 0; col < dataReader.FieldCount; col++)
      {
        ImmutableColumn column;
        if (fileReader != null)
        {
          var iColumn = fileReader.GetColumn(col);
          if (iColumn is ImmutableColumn immutableColumn)
            column = immutableColumn;
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
            new ImmutableValueFormat(DataTypeEnum.Integer),
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
            new ImmutableValueFormat(DataTypeEnum.Integer),
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
            new ImmutableValueFormat(DataTypeEnum.Integer),
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

    public bool HasErrors => !(m_ColumnErrorDictionary is null) && m_ColumnErrorDictionary.Count > 0;

    public IReadOnlyList<ImmutableColumn> Column => m_ReaderColumnNotIgnored;

    public string RowErrorInformation =>
      ErrorInformation.ReadErrorInformation(m_ColumnErrorDictionary, m_ReaderColumnsAll);

    public void PrepareRead() => m_ColumnErrorDictionary?.Clear();

    public void SetDataRowErrors(DataRow dataRow)
    {
      if (dataRow is null)
        throw new ArgumentNullException(nameof(dataRow));
      // This gets the errors from the fileReader
      if (m_ColumnErrorDictionary is null)
        return;
      foreach (var keyValuePair in m_ColumnErrorDictionary)
        if (keyValuePair.Key == -1)
          dataRow.RowError = keyValuePair.Value;
        else
          dataRow.SetColumnError(m_Mapping[keyValuePair.Key], keyValuePair.Value);
    }

    public int DataTableToReader(int tableColumn) => m_Mapping.GetByValue(tableColumn);
  }
}