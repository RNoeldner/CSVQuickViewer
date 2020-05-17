/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace CsvTools
{
  /// <summary>
  ///   ColumnInfo
  /// </summary>
  [DebuggerDisplay("ColumnInfo( {Column.Name}  - {Column.ValueFormat.GetFormatDescription()})")]
  public sealed class ColumnInfo
  {
    public ColumnInfo(Column column, int fieldLength, bool isTimePart, int columnOrdinalReader = -1,
      string constantTimeZone = null, int columnOrdinalTimeZoneReader = -1)
    {
      Column = column;
      FieldLength = fieldLength;
      IsTimePart = isTimePart;
      ColumnOrdinalReader = columnOrdinalReader;
      ConstantTimeZone = constantTimeZone;
      ColumnOrdinalTimeZoneReader = columnOrdinalTimeZoneReader;
    }

    /// <summary>
    ///   Gets or sets the column format.
    /// </summary>
    /// <value>The column format.</value>
    public Column Column { get; }

    /// <summary>
    ///   Gets or sets the reader column ordinal
    /// </summary>
    /// <value>The column ordinal.</value>
    public int ColumnOrdinalReader { get; }

    public int ColumnOrdinalTimeZoneReader { get; private set; }

    public string ConstantTimeZone { get; private set; }

    /// <summary>
    ///   Gets or sets the length of the field.
    /// </summary>
    /// <value>The length of the field. 0 means unrestricted length</value>
    public int FieldLength { get; }

    /// <summary>
    ///   Gets or sets a value indicating whether is a time part
    /// </summary>
    /// <value><c>true</c> if is a time part of another field; otherwise, <c>false</c>.</value>
    public bool IsTimePart { get; set; }

    /// <summary>
    ///   Gets the column information based on the SQL Source, but overwritten with the definitions
    /// </summary>
    /// <param name="writerFileSetting">The file settings with definitions</param>
    /// <param name="sourceSchemaDataReader">The reader for the source.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">reader</exception>
    public static IEnumerable<ColumnInfo> GetSourceColumnInformation(IFileSetting writerFileSetting,
      IDataReader sourceSchemaDataReader)
    {
      if (sourceSchemaDataReader == null)
        throw new ArgumentNullException(nameof(sourceSchemaDataReader));
      var result = new List<ColumnInfo>();
      using (var dataTable = sourceSchemaDataReader.GetSchemaTable())
      {
        if (dataTable == null)
          throw new ArgumentNullException(nameof(sourceSchemaDataReader));

        var colName = new BiDirectionalDictionary<int, string>();

        // Make names unique and fill the dictionary
        foreach (DataRow schemaRow in dataTable.Rows)
        {
          var colNo = (int) schemaRow[SchemaTableColumn.ColumnOrdinal];
          var newName =
            StringUtils.MakeUniqueInCollection(colName.Values, schemaRow[SchemaTableColumn.ColumnName].ToString());

          colName.Add(colNo, newName);
        }

        foreach (DataRow schemaRow in dataTable.Rows)
        {
          var colNo = (int) schemaRow[SchemaTableColumn.ColumnOrdinal];
          var valueFormat = writerFileSetting.FileFormat.ValueFormat;
          valueFormat.DataType = ((Type) schemaRow[SchemaTableColumn.DataType]).GetDataType();
          var column = writerFileSetting.ColumnCollection.Get(colName[colNo]);
          if (column != null)
          {
            if (column.Ignore)
              continue;
            valueFormat = column.ValueFormat;
          }

          var fieldLength = Math.Max((int) schemaRow[SchemaTableColumn.ColumnSize], 0);
          switch (valueFormat.DataType)
          {
            case DataType.Integer:
              fieldLength = 10;
              break;

            case DataType.Boolean:
            {
              var lenTrue = valueFormat.True.Length;
              var lenFalse = valueFormat.False.Length;
              fieldLength = lenTrue > lenFalse ? lenTrue : lenFalse;
              break;
            }
            case DataType.Double:
            case DataType.Numeric:
              fieldLength = 28;
              break;

            case DataType.DateTime:
              fieldLength = valueFormat.DateFormat.Length;
              break;
          }

          var ci = new ColumnInfo(new Column(colName[colNo], valueFormat), fieldLength, false, colNo);

          // the timezone information
          if (column != null)
          {
            var tz = column.TimeZonePart;
            if (!string.IsNullOrEmpty(tz))
            {
              var tzInfo = tz.GetPossiblyConstant();
              if (tzInfo.Item2)
              {
                ci.ConstantTimeZone = tzInfo.Item1;
              }
              else
              {
                if (colName.TryGetByValue(tzInfo.Item1, out var ordinal))
                  ci.ColumnOrdinalTimeZoneReader = ordinal;
              }
            }
          }

          result.Add(ci);

          // add an extra column for the time, reading columns get combined, writing they get separated
          if (column != null && !string.IsNullOrEmpty(column.TimePart) && !colName.ContainsValue(column.TimePart))
          {
            if (ci.Column.ValueFormat.DateFormat.IndexOfAny(new[] {'h', 'H', 'm', 's'}) != -1)
              Logger.Warning(
                $"'{ci.Column.Name}' will create a separate time column '{column.TimePart}' but seems to write time itself '{ci.Column.ValueFormat.DateFormat}'");
            // In case we have a split column, add the second column (unless the column is also present
            result.Add(new ColumnInfo(
              new Column(column.TimePart, column.TimePartFormat)
                {ValueFormat = {TimeSeparator = column.ValueFormat.TimeSeparator}}, column.TimePartFormat.Length, true,
              colNo));
          }
        }
      }

      return result;
    }
  }
}