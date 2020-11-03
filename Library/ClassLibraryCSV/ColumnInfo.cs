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

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace CsvTools
{
  /// <summary>
  ///   ColumnInfo
  /// </summary>
  [DebuggerDisplay("ColumnInfo( {Column.Name}  - {Column.ValueFormat})")]
  public sealed class ColumnInfo
  {
    private ColumnInfo([NotNull] IColumn column, int fieldLength, int columnOrdinalReader)
    {
      Column = column;
      FieldLength = fieldLength;
      ColumnOrdinalReader = columnOrdinalReader;
    }

    /// <summary>
    ///   Gets or sets the column format.
    /// </summary>
    /// <value>The column format.</value>
    public IColumn Column { [NotNull] get; }

    /// <summary>
    ///   Gets or sets the reader column ordinal
    /// </summary>
    /// <value>The column ordinal.</value>
    public int ColumnOrdinalReader { get; }

    public int ColumnOrdinalTimeZoneReader { get; private set; } = -1;

    public string ConstantTimeZone { [CanBeNull] get; private set; }

    /// <summary>
    ///   Gets or sets the length of the field.
    /// </summary>
    /// <value>The length of the field. 0 means unrestricted length</value>
    public int FieldLength { get; }

    /// <summary>
    ///   Gets the column information based on the SQL Source, but overwritten with the definitions
    /// </summary>
    /// <param name="generalFormat">general value format for not explicitly specified columns format</param>
    /// <param name="columnDefinitions"></param>
    /// <param name="sourceSchemaDataReader">The reader for the source.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">reader</exception>
    public static IEnumerable<ColumnInfo> GetWriterColumnInformation(IValueFormat generalFormat,
      IReadOnlyCollection<IColumn> columnDefinitions, IDataReader sourceSchemaDataReader)
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
          var column =
            columnDefinitions.FirstOrDefault(x => x.Name.Equals(colName[colNo], StringComparison.OrdinalIgnoreCase));

          if (column != null && column.Ignore)
            continue;

          // Based on the data Type in the reader defined and the general format create the  value format
          var valueFormat = column?.ValueFormat ?? new ImmutableValueFormat(
            ((Type) schemaRow[SchemaTableColumn.DataType]).GetDataType(), generalFormat.DateFormat,
            generalFormat.DateSeparator,
            generalFormat.DecimalSeparatorChar, generalFormat.DisplayNullAs, generalFormat.False,
            generalFormat.GroupSeparatorChar, generalFormat.NumberFormat,
            generalFormat.TimeSeparator, generalFormat.True);

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
            case DataType.Guid:
              fieldLength = 36;
              break;
            case DataType.String:
            case DataType.TextToHtml:
            case DataType.TextToHtmlFull:
            case DataType.TextPart:
              break;
            default:
              throw new ArgumentOutOfRangeException();
          }

          var ci = new ColumnInfo(new ImmutableColumn(colName[colNo], valueFormat, colNo), fieldLength, colNo);

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

          // add an extra column for the time, reading columns they get combined, writing them they get separated again

          if (column == null || string.IsNullOrEmpty(column.TimePart) || colName.ContainsValue(column.TimePart))
            continue;

          if (ci.Column.ValueFormat.DateFormat.IndexOfAny(new[] {'h', 'H', 'm', 's'}) != -1)
            Logger.Warning(
              $"'{ci.Column.Name}' will create a separate time column '{column.TimePart}' but seems to write time itself '{ci.Column.ValueFormat.DateFormat}'");

          // In case we have a split column, add the second column (unless the column is also present
          result.Add(new ColumnInfo(
            new ImmutableColumn(column.TimePart,
              new ImmutableValueFormat(DataType.DateTime, column.TimePartFormat,
                timeSeparator: column.ValueFormat.TimeSeparator), colNo, true), column.TimePartFormat.Length, colNo));
        }
      }

      return result;
    }
  }
}