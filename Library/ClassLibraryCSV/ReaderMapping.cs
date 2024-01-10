#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace CsvTools
{
  /// <summary>
  ///   Handles mapping of a data reader to a resulting data reader ignored columns will be omitted
  ///   and artificial columns for Line, record and error information are added at the correct places
  /// </summary>
  [DebuggerDisplay("{m_Mapping}")]
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
    public readonly int ColNumErrorFieldSource;

    /// <summary>
    /// The col number of the record number column, -1 if not present
    /// </summary>
    public readonly int ColNumRecNum;

    /// <summary>
    /// The col number of the start line column, -1 if not present
    /// </summary>
    public readonly int ColNumStartLine;
    private readonly BiDirectionalDictionary<int, int> m_Mapping;
    private readonly List<Column> m_ResultingColumns;

    /// <summary>
    ///   Maps the columns of the data reader for a reader wrapper, taking care of ignored and
    ///   artificial columns
    /// </summary>
    /// <param name="dataReader">
    ///   <see cref="IDataRecord" /> usually a <see cref="IFileReader" /> or <see cref="IDataReader" />
    /// </param>
    /// <param name="startLine">Add artificial field Start Line, if false the data will be passed on from the source (if existing)</param>
    /// <param name="endLine">Add artificial field End Line, if false the data will be passed on from the source (if existing)</param>
    /// <param name="recNum">Add artificial field Records Number, if false the data will be passed on from the source (if existing)</param>
    /// <param name="errorField">Add artificial field Error but only if the source does not have the information</param>
    public ReaderMapping(IEnumerable<Column> columns, bool startLine, bool endLine, bool recNum, bool errorField)
    {
      m_Mapping = new BiDirectionalDictionary<int, int>();
      m_ResultingColumns = new List<Column>();

      var fieldCount = 0;

      var orgEndLine = -1;
      ColNumEndLine = -1;

      ColNumErrorFieldSource = -1;
      ColNumErrorField = -1;

      var orgRecNum = -1;
      ColNumRecNum = -1;

      var orgStartLine = -1;
      ColNumStartLine = -1;

      var index = -1;
      foreach (var column in columns)
      {
        index++;
        if (column.Ignore)
          continue;

        // Handle special columns,  
        if (column.Name.Equals(ReaderConstants.cErrorField, StringComparison.OrdinalIgnoreCase))
        {
          ColNumErrorFieldSource = index;
          errorField = true;
          continue;
        }
        if (column.Name.Equals(ReaderConstants.cStartLineNumberFieldName, StringComparison.OrdinalIgnoreCase))
        {
          orgStartLine = index;
          startLine = true;
          continue;
        }
        if (column.Name.Equals(ReaderConstants.cEndLineNumberFieldName, StringComparison.OrdinalIgnoreCase))
        {
          orgEndLine = index;
          endLine = true;
          continue;
        }

        if (column.Name.Equals(ReaderConstants.cRecordNumberFieldName, StringComparison.OrdinalIgnoreCase))
        {
          orgRecNum = index;
          recNum = true;
          continue;
        }

        // Arriving here we take the source column but make sure the ColumnOrdinal is set properly
        m_ResultingColumns.Add(new Column(column.Name, column.ValueFormat, m_ResultingColumns.Count, false, null, column.DestinationName, column.TimePart, column.TimePartFormat, column.TimeZonePart));
        m_Mapping.Add(index, fieldCount++);
      }

      // Add required columns in the right order at the end
      if (recNum)
      {
        if (orgRecNum>=0) m_Mapping.Add(orgRecNum, fieldCount);
        m_ResultingColumns.Add(new Column(ReaderConstants.cRecordNumberFieldName, new ValueFormat(DataTypeEnum.Integer), ColNumRecNum = fieldCount++));
      }

      if (endLine)
      {
        if (orgEndLine>=0) m_Mapping.Add(orgEndLine, fieldCount);
        m_ResultingColumns.Add(new Column(ReaderConstants.cEndLineNumberFieldName, new ValueFormat(DataTypeEnum.Integer), ColNumEndLine = fieldCount++));
      }

      if (errorField)
      {
        if (ColNumErrorFieldSource>=0) m_Mapping.Add(ColNumErrorFieldSource, fieldCount);
        m_ResultingColumns.Add(new Column(ReaderConstants.cErrorField, ValueFormat.Empty, ColNumErrorField = fieldCount++));
      }

      if (startLine)
      {
        if (orgStartLine>=0) m_Mapping.Add(orgStartLine, fieldCount);
        m_ResultingColumns.Add(new Column(ReaderConstants.cStartLineNumberFieldName, new ValueFormat(DataTypeEnum.Integer), ColNumStartLine = fieldCount));
      }
    }

    /// <summary>
    /// Gets the columns that can be accessed (not ignored or artificial columns)
    /// </summary>
    /// <value>
    /// The columns
    /// </value>
    public IReadOnlyList<Column> ResultingColumns => m_ResultingColumns;

    /// <summary>
    /// Get the column number of the source, taking care of ignored columns and artificial columns
    /// </summary>
    /// <param name="resultColumn">The column number in the reader with artificial columns</param>
    /// <returns>The column number in the source</returns>
    public int ResultToSource(int resultColumn) => m_Mapping.GetByValue(resultColumn);

    /// <summary>
    /// Get the column number of the source, taking care of ignored columns and artificial columns
    /// </summary>
    /// <param name="sourceColumn">The column number in the source reader</param>
    /// <param name="resultColumn">The column number in the reader with artificial columns</param>
    /// <returns>True if found</returns>
    public bool SourceToResult(int sourceColumn, out int resultColumn) => m_Mapping.TryGetValue(sourceColumn, out resultColumn);
  }
}