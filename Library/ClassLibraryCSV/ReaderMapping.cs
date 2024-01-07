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
    /// <summary>
    /// The col number end line column, -1 if not present
    /// </summary>
    public readonly int ColNumEndLine;
    /// <summary>
    /// The col number of the artificial error column, -1 if not present
    /// </summary>
    public readonly int ColNumErrorField;
    /// <summary>
    /// The col number of the source error column, -1 if not present, in this case use RowErrorInformation
    /// </summary>
    private readonly int m_ColNumErrorFieldSource;
    /// <summary>
    /// The col number of the record number column, -1 if not present
    /// </summary>
    public readonly int ColNumRecNum;
    /// <summary>
    /// The col number of the start line column, -1 if not present
    /// </summary>
    public readonly int ColNumStartLine;
    private readonly ColumnErrorDictionary m_ColumnErrorDictionary;
    private readonly BiDirectionalDictionary<int, int> m_Mapping = new BiDirectionalDictionary<int, int>();
    private readonly List<Column> m_ReaderColumnNotIgnored = new List<Column>();
    private readonly List<string> m_ReaderColumnsAll = new List<string>();
    private readonly IDataRecord m_DataReader;

    /// <summary>
    ///   Maps the columns of the data reader for a reader wrapper, taking care of ignored and
    ///   artificial columns
    /// </summary>
    /// <param name="dataReader">
    ///   <see cref="IDataRecord" /> usually a <see cref="IFileReader" /> or <see cref="IDataReader" />
    /// </param>
    /// <param name="addStartLine">Add artificial field Start Line, if false the data will be passed on from the source (if existing)</param>
    /// <param name="addEndLine">Add artificial field End Line, if false the data will be passed on from the source (if existing)</param>
    /// <param name="addRecNum">Add artificial field Records Number, if false the data will be passed on from the source (if existing)</param>
    /// <param name="addErrorField">Add artificial field Error but only if the source does not have the information</param>
    public ReaderMapping(in IDataRecord dataReader,
      bool addStartLine, bool addEndLine,
      bool addRecNum, bool addErrorField)
    {
      m_DataReader = dataReader;
      var fileReader = dataReader as IFileReader;
      m_ColumnErrorDictionary = new ColumnErrorDictionary(fileReader);

      var fieldCount = 0;

      ColNumEndLine = -1;
      ColNumErrorField = -1;
      m_ColNumErrorFieldSource = -1;
      ColNumRecNum = -1;
      ColNumStartLine = -1;

      for (var col = 0; col < dataReader.FieldCount; col++)
      {
        var column = (fileReader != null) ? fileReader.GetColumn(col) : new Column(dataReader.GetName(col), new ValueFormat(dataReader.GetFieldType(col).GetDataType()), col);
        m_ReaderColumnsAll.Add(column.Name);
        if (column.Ignore)
          continue;

        if (column.Name.Equals(ReaderConstants.cErrorField))
        {
          m_ColNumErrorFieldSource = col;
          addErrorField = true;
        }

        // Do not add a source field in case we have a matching artificial field, unless it's an #Error, this will stay in source and artificial
        if ((column.Name.Equals(ReaderConstants.cStartLineNumberFieldName, StringComparison.OrdinalIgnoreCase) && addStartLine)
         || (column.Name.Equals(ReaderConstants.cEndLineNumberFieldName, StringComparison.OrdinalIgnoreCase) && addEndLine)
         || (column.Name.Equals(ReaderConstants.cRecordNumberFieldName, StringComparison.OrdinalIgnoreCase) && addRecNum)
         || column.Name.Equals(ReaderConstants.cErrorField, StringComparison.OrdinalIgnoreCase))
          continue;

        m_ReaderColumnNotIgnored.Add(column);
        m_Mapping.Add(col, fieldCount++);
      }

      // Possibly add artificial fields
      if (addRecNum)
        m_ReaderColumnNotIgnored.Add(new Column(ReaderConstants.cRecordNumberFieldName, new ValueFormat(DataTypeEnum.Integer), ColNumRecNum = fieldCount++));

      if (addEndLine)
        m_ReaderColumnNotIgnored.Add(new Column(ReaderConstants.cEndLineNumberFieldName, new ValueFormat(DataTypeEnum.Integer), ColNumEndLine = fieldCount++));

      if (addErrorField)
        m_ReaderColumnNotIgnored.Add(new Column(ReaderConstants.cErrorField, ValueFormat.Empty, ColNumErrorField = fieldCount++));

      if (addStartLine)
        m_ReaderColumnNotIgnored.Add(new Column(ReaderConstants.cStartLineNumberFieldName, new ValueFormat(DataTypeEnum.Integer), ColNumStartLine = fieldCount));
    }

    /// <summary>
    /// Gets a value indicating whether this instance has errors.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance has errors; otherwise, <c>false</c>.
    /// </value>
    public bool HasErrors => (m_ColNumErrorFieldSource != -1) ? !m_DataReader.IsDBNull(m_ColNumErrorFieldSource) : m_ColumnErrorDictionary.Count > 0;

    /// <summary>
    /// Gets the columns that can be accessed (not ignored or artificial columns)
    /// </summary>
    /// <value>
    /// The columns
    /// </value>
    public IReadOnlyList<Column> Column => m_ReaderColumnNotIgnored;

    /// <summary>
    /// Gets the row error information.
    /// </summary>
    /// <value>
    /// The row error information.
    /// </value>
    public string RowErrorInformation => (m_ColNumErrorFieldSource != -1)
      ? (m_DataReader.IsDBNull(m_ColNumErrorFieldSource) ? string.Empty : m_DataReader.GetString(m_ColNumErrorFieldSource))
      : ErrorInformation.ReadErrorInformation(m_ColumnErrorDictionary, m_ReaderColumnsAll);

    /// <summary>
    /// Prepares the reading a new records, emptying out the error information
    /// </summary>
    public void PrepareRead() => m_ColumnErrorDictionary.Clear();

    /// <summary>
    /// Stores error information in the data row
    /// </summary>
    /// <param name="dataRow">The data row.</param>
    public void SetErrorInformation(DataRow dataRow)
    {
      if (m_ColNumErrorFieldSource != -1)
      {
        // get the errors from the source
        if (!m_DataReader.IsDBNull(m_ColNumErrorFieldSource))
          dataRow.SetErrorInformation(m_DataReader.GetString(m_ColNumErrorFieldSource));
      }
      else
      {
        // This gets the errors from the fileReader
        foreach (var keyValuePair in m_ColumnErrorDictionary)
          if (keyValuePair.Key == -1)
            dataRow.RowError = keyValuePair.Value;
          else
          {
            if (m_Mapping.TryGetValue(keyValuePair.Key, out var column))
              dataRow.SetColumnError(column, keyValuePair.Value);
          }
      }
    }

    /// <summary>
    /// Get the column number of the source, taking care of ignored columns and artificial columns
    /// </summary>
    /// <param name="tableColumn">The column number in the reader with artificial columns</param>
    /// <returns>The column number in the source</returns>
    public int DataTableToReader(int tableColumn) => m_Mapping.GetByValue(tableColumn);
  }
}