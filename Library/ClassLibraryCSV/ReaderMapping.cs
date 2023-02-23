#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace CsvTools
{
  /// <summary>
  ///   Handles mapping of a data reader to a resulting data reader columns ignored will be omitted
  ///   and artificial columns for Line, record and error information is added
  /// </summary>
  public sealed class ReaderMapping
  {
    public readonly int ColNumEndLine;
    public readonly int ColNumErrorField;
    public readonly int ColNumErrorFieldSource;
    public readonly int ColNumRecNum;
    public readonly int ColNumStartLine;
    private readonly ColumnErrorDictionary? m_ColumnErrorDictionary;
    private readonly BiDirectionalDictionary<int, int> m_Mapping = new BiDirectionalDictionary<int, int>();
    private readonly List<Column> m_ReaderColumnNotIgnored = new List<Column>();
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

      // TODO: This is not good, artifical fields from source are ignored
      // ----------------------------------------------------------------
      // Better would be to pass them though, at least for Errors
      // Problem is tht the position possibly needs to be adjusted as bulk copy need teh columns in the same order as the table
      var readerColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var fieldCount = 0;
      ColNumErrorFieldSource = -1;
      for (var col = 0; col < dataReader.FieldCount; col++)
      {
        Column column;
        if (fileReader != null)
        {
          column = fileReader.GetColumn(col);
        }
        else
        {
          column = new Column(
            dataReader.GetName(col),
            new ValueFormat(dataReader.GetFieldType(col).GetDataType()),
            col);
        }
        m_ReaderColumnsAll.Add(column.Name);
        if (column.Ignore)
          continue;

        if (column.Name.Equals(ReaderConstants.cErrorField))
        {
          ColNumErrorFieldSource = col;
          ColNumErrorField = col;
        }

        if (// In case we do not add a line, accept the source data
            (column.Name.Equals(ReaderConstants.cStartLineNumberFieldName) && addStartLine)
            || (column.Name.Equals(ReaderConstants.cEndLineNumberFieldName) && addEndLine)
            // An Record number is ignore all the time
            || column.Name.Equals(ReaderConstants.cRecordNumberFieldName) && addRecNum)
          continue;

        m_ReaderColumnNotIgnored.Add(column);
        readerColumns.Add(column.Name);
        m_Mapping.Add(col, fieldCount++);
      }

      // the order of artificial fields must match the order in IDbConnector.CreateTableSQL
      if (addRecNum && !readerColumns.Contains(ReaderConstants.cRecordNumberFieldName))
      {
        ColNumRecNum = fieldCount++;
        m_ReaderColumnNotIgnored.Add(
          new Column(
            ReaderConstants.cRecordNumberFieldName,
            new ValueFormat(DataTypeEnum.Integer),
            ColNumRecNum));
      }
      else
      {
        ColNumRecNum = -1;
      }

      if (addEndLine && !readerColumns.Contains(ReaderConstants.cEndLineNumberFieldName))
      {
        ColNumEndLine = fieldCount++;
        m_ReaderColumnNotIgnored.Add(
          new Column(
            ReaderConstants.cEndLineNumberFieldName,
            new ValueFormat(DataTypeEnum.Integer),
            ColNumEndLine));
      }
      else
      {
        ColNumEndLine = -1;
      }

      if (addErrorField && !readerColumns.Contains(ReaderConstants.cErrorField))
      {
        ColNumErrorField = fieldCount++;
        m_ReaderColumnNotIgnored.Add(
          new Column(ReaderConstants.cErrorField, ValueFormat.Empty, ColNumErrorField));
      }
      else
      {
        ColNumErrorField = -1;
      }

      // add fields
      if (addStartLine && !readerColumns.Contains(ReaderConstants.cStartLineNumberFieldName))
      {
        ColNumStartLine = fieldCount;
        m_ReaderColumnNotIgnored.Add(
          new Column(
            ReaderConstants.cStartLineNumberFieldName,
            new ValueFormat(DataTypeEnum.Integer),
            ColNumStartLine));
      }
      else
      {
        ColNumStartLine = -1;
      }
    }

    public bool HasErrors => !(m_ColumnErrorDictionary is null) && m_ColumnErrorDictionary.Count > 0;

    public IReadOnlyList<Column> Column => m_ReaderColumnNotIgnored;

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
        {
          if (m_Mapping.TryGetValue(keyValuePair.Key, out var column))
            dataRow.SetColumnError(column, keyValuePair.Value);
        }
    }

    public int DataTableToReader(int tableColumn) => m_Mapping.GetByValue(tableColumn);
  }
}