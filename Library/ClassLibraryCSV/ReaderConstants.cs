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
using System.Globalization;

namespace CsvTools
{
  public static class ReaderConstants
  {
    /// <summary>
    ///   Field name of the LineNumber Field
    /// </summary>
    public const string cEndLineNumberFieldName = "#LineEnd";

    /// <summary>
    ///   Field name of the Error Field
    /// </summary>
    public const string cErrorField = "#Error";

    /// <summary>
    ///   Field name of the LineNumber Start Field
    /// </summary>
    public const string cPartitionField = "#Partition";

    /// <summary>
    ///   Field Name of the record number
    /// </summary>
    public const string cRecordNumberFieldName = "#Record";

    /// <summary>
    ///   Field name of the LineNumber Start Field
    /// </summary>
    public const string cStartLineNumberFieldName = "#Line";

    /// <summary>
    ///   Collection of the artificial field names
    /// </summary>
    public static readonly IReadOnlyCollection<string> ArtificialFields =
      new HashSet<string>(StringComparer.OrdinalIgnoreCase) { cRecordNumberFieldName,
                                                              cStartLineNumberFieldName,
                                                              cEndLineNumberFieldName,
                                                              cErrorField,
                                                              cPartitionField };

    /// <summary>
    ///   Gets the default schema row array.
    /// </summary>
    /// <returns>an Array of objects for a new row in a Schema Table</returns>
    public static object?[] GetDefaultSchemaRowArray() =>
      new object?[]
      {
        true, // 00- AllowDBNull
        null, // 01- BaseColumnName
        string.Empty, // 02- BaseSchemaName
        string.Empty, // 03- BaseTableName
        null, // 04- ColumnName
        null, // 05- ColumnOrdinal
        int.MaxValue, // 06- ColumnSize
        typeof(string), // 07- DataType
        false, // 08- IsAliased
        false, // 09- IsExpression
        false, // 10- IsKey
        false, // 11- IsLong
        false, // 12- IsUnique
        DBNull.Value, // 13- NumericPrecision
        DBNull.Value, // 14- NumericScale
        (int) DbType.String, // 15- ProviderType
        string.Empty, // 16- BaseCatalogName
        string.Empty, // 17- BaseServerName
        false, // 18- IsAutoIncrement
        false, // 19- IsHidden
        true, // 20- IsReadOnly
        false // 21- IsRowVersion
      };

    /// <summary>
    ///   Gets the empty schema table.
    /// </summary>
    /// <returns>A Data Table with the columns for a Schema Table</returns>
    public static DataTable GetEmptySchemaTable()
    {
      var dataTable = new DataTable
      {
        TableName = "SchemaTable",
        Locale = CultureInfo.InvariantCulture,
        MinimumCapacity = 10
      };

      dataTable.Columns.Add(SchemaTableColumn.AllowDBNull, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.BaseColumnName, typeof(string)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.BaseSchemaName, typeof(string)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.BaseTableName, typeof(string)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.ColumnName, typeof(string)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.ColumnSize, typeof(int)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.DataType, typeof(object)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.IsAliased, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.IsExpression, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.IsKey, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.IsLong, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.IsUnique, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.NumericPrecision, typeof(short)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.NumericScale, typeof(short)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.ProviderType, typeof(int)).ReadOnly = true;

      dataTable.Columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(string)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(bool)).ReadOnly = true;

      return dataTable;
    }
  }
}