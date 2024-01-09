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
    private readonly List<Column> m_ReaderColumnNotIgnored;

    /// <summary>
    /// All original reader columns for mapping of columns
    /// </summary>
    public readonly List<string> ReaderColumnsAll;    

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
    public ReaderMapping(in IDataRecord dataReader, bool startLine, bool endLine, bool recNum, bool errorField)
    {           
      m_Mapping = new BiDirectionalDictionary<int, int>();
      m_ReaderColumnNotIgnored = new List<Column>();
      ReaderColumnsAll = new List<string>();
      
      var fieldCount = 0;

      ColNumEndLine = -1;
      ColNumErrorField = -1;
      ColNumErrorFieldSource = -1;
      ColNumRecNum = -1;
      ColNumStartLine = -1;

      // var orgStartLine = -1;
      // var orgRecNum = -2;
      // var orgEndLine = -3;
       var fileReader = dataReader as IFileReader;
      for (var col = 0; col < dataReader.FieldCount; col++)
      {
        var column = (fileReader != null) ? fileReader.GetColumn(col) : new Column(dataReader.GetName(col), new ValueFormat(dataReader.GetFieldType(col).GetDataType()), col);
        ReaderColumnsAll.Add(column.Name);
        if (column.Ignore)
          continue;

        if (column.Name.Equals(ReaderConstants.cErrorField))
        {
          ColNumErrorFieldSource = col;
          errorField = true;
          continue;
        }
        // Do not add a source field in case we have a matching artificial field, unless it's an #Error, this will stay in source and artificial
        if (column.Name.Equals(ReaderConstants.cStartLineNumberFieldName, StringComparison.OrdinalIgnoreCase))
        {
          // orgStartLine = col;
          startLine = true;
          continue;
        }
        if (column.Name.Equals(ReaderConstants.cEndLineNumberFieldName, StringComparison.OrdinalIgnoreCase))
        {
          // orgEndLine = col;
          endLine = true;
          continue;
        }

        if (column.Name.Equals(ReaderConstants.cRecordNumberFieldName, StringComparison.OrdinalIgnoreCase))
        {
          // orgRecNum = col;
          recNum = true;
          continue;
        }

        m_ReaderColumnNotIgnored.Add(column);
        m_Mapping.Add(col, fieldCount++);        
      }

      // Possibly add artificial fields
      if (recNum)
      {
        // m_Mapping.Add(orgRecNum, fieldCount);
        m_ReaderColumnNotIgnored.Add(new Column(ReaderConstants.cRecordNumberFieldName, new ValueFormat(DataTypeEnum.Integer), ColNumRecNum = fieldCount++));
      }

      if (endLine)
      {
        // m_Mapping.Add(orgEndLine, fieldCount);
        m_ReaderColumnNotIgnored.Add(new Column(ReaderConstants.cEndLineNumberFieldName, new ValueFormat(DataTypeEnum.Integer), ColNumEndLine = fieldCount++));
      }

      if (errorField)
      {
        // m_Mapping.Add(ColNumErrorFieldSource, fieldCount);
        m_ReaderColumnNotIgnored.Add(new Column(ReaderConstants.cErrorField, ValueFormat.Empty, ColNumErrorField = fieldCount++));
      }

      if (startLine)
      {
        // m_Mapping.Add(orgStartLine, fieldCount);
        m_ReaderColumnNotIgnored.Add(new Column(ReaderConstants.cStartLineNumberFieldName, new ValueFormat(DataTypeEnum.Integer), ColNumStartLine = fieldCount));
      }
    }

    /// <summary>
    /// Gets the columns that can be accessed (not ignored or artificial columns)
    /// </summary>
    /// <value>
    /// The columns
    /// </value>
    public IReadOnlyList<Column> Column => m_ReaderColumnNotIgnored;    

    /// <summary>
    /// Get the column number of the source, taking care of ignored columns and artificial columns
    /// </summary>
    /// <param name="tableColumn">The column number in the reader with artificial columns</param>
    /// <returns>The column number in the source</returns>
    public int DataTableToReader(int tableColumn) => m_Mapping.GetByValue(tableColumn);
  }
}