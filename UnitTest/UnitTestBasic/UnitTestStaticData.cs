/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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

#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace CsvTools.Tests;

public static class UnitTestStaticData
{
  // Private column name constants
  public const string ColString = "string";
  public const string ColInt = "int";
  public const string ColDateTime = "DateTime";
  public const string ColBool = "bool";
  public const string ColDouble = "double";
  public const string ColNumeric = "numeric";
  public const string ColGuid = "guid";
  public const string ColAllEmpty = "AllEmpty";
  public const string ColPartEmpty = "PartEmpty";
  public const string ColId = "ID";
  public const string ColTime = "Time";
  public const string ColUserId = "User ID";
  public const string ColParentId = "Parent ID";
  public const string ColManagerId = "Manager ID";
  public const string ColApproverId = "Approver ID";
  public const string DateFormat = @"dd/MM/yyyy";
  public const string DateTimeFormat = "HH:mm:ss";

  public static readonly IReadOnlyList<Column> Columns =
  [
    new Column(ColString, columnOrdinal:0),
    new Column(ColInt, new ValueFormat(DataTypeEnum.Integer), 1),
    new Column(ColDateTime, new ValueFormat(DataTypeEnum.DateTime, dateFormat: DateFormat), 2, timePart: ColTime, timePartFormat: DateTimeFormat),
    new Column(ColBool, new ValueFormat(DataTypeEnum.Boolean),3),
    new Column(ColDouble, new ValueFormat(DataTypeEnum.Double, decimalSeparator: "."),4),
    new Column(ColNumeric, new ValueFormat(DataTypeEnum.Numeric, decimalSeparator: "."),5),
    new Column(ColGuid, new ValueFormat(DataTypeEnum.Guid),6),
    new Column(ColAllEmpty, columnOrdinal:7),
    new Column(ColPartEmpty, columnOrdinal:8),
    new Column(ColId, new ValueFormat(DataTypeEnum.Integer),9),
    new Column(ColTime, new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: DateTimeFormat), 10, ignore: true),
    new Column(ColUserId, new ValueFormat(DataTypeEnum.String),11),
    new Column(ColParentId, new ValueFormat(DataTypeEnum.Integer),12),
    new Column(ColManagerId, new ValueFormat(DataTypeEnum.String),13),
    new Column(ColApproverId, new ValueFormat(DataTypeEnum.String),13),
  ];

  /// <summary>
  /// Adds the row to data table, the table should have the right columns already added
  /// </summary>
  public static (int warnings, int errors) AddRowToDataTable(DataTable dataTable, int recordNumber, bool startLine,
    bool endLine, bool recNum, bool errorField)
  {
    var minDate = DateTime.Now.AddYears(-20).Ticks;
    var maxDate = DateTime.Now.AddYears(5).Ticks;
    var dr = dataTable.NewRow();

    // string
    dr[ColString] = UnitTestStatic.GetRandomText(25);
    if (recordNumber % 10 == 0)
      dr[ColString] = dr[ColString] + "\r\nA Second Line";

    // int
    dr[ColInt] = UnitTestStatic.Random.Next(-300, +600);

    // DateTime
    if (recordNumber % 5 == 0)
      dr[ColDateTime] = new DateTime((((maxDate - minDate) * UnitTestStatic.Random.NextDouble()) + minDate).ToInt64());

    // bool
    dr[ColBool] = UnitTestStatic.Random.Next(0, 2) == 0;

    // double
    dr[ColDouble] = Math.Round(UnitTestStatic.Random.NextDouble() * 123.78, 4);

    // numeric
    if (recordNumber % 4 == 0)
      dr[ColNumeric] = Math.Round(UnitTestStatic.Random.NextDouble(), 4);

    // guid
    if (recordNumber % 3 == 0)
      dr[ColGuid] = Guid.NewGuid();

    // AllEmpty / PartEmpty
    if (recordNumber % 2 == 0)
      dr[ColPartEmpty] = UnitTestStatic.GetRandomText(100);

    // ID
    dr[ColId] = recordNumber;

    // Time
    if (recordNumber % 3 == 0 && dataTable.Columns.Contains(ColTime))
      dr[ColTime] = new DateTime(1, 1, 1, UnitTestStatic.Random.Next(0, 24), UnitTestStatic.Random.Next(0, 660) % 60, 0);

    // User ID
    dr[ColUserId] = $"U{recordNumber}";

    // Parent ID
    if (recordNumber > 0 && UnitTestStatic.Random.Next(0, 2) == 0)
    {
      int randomPreviousId = UnitTestStatic.Random.Next(0, recordNumber);
      dr[ColParentId] = randomPreviousId;
    }

    // Manager ID
    if (recordNumber > 0 && UnitTestStatic.Random.Next(0, 2) == 0)
    {
      int randomPreviousId = UnitTestStatic.Random.Next(0, recordNumber);
      dr[ColManagerId] = $"U{randomPreviousId}";
    }

    // Approver ID
    if (recordNumber > 0 && UnitTestStatic.Random.Next(0, 2) == 0)
    {
      int randomPreviousId = UnitTestStatic.Random.Next(0, recordNumber);
      dr[ColApproverId] = $"U{randomPreviousId}";
    }

    // AddOrUpdate Errors and Warnings to Columns and Rows
    var rand = UnitTestStatic.Random.Next(0, 100);
    int warnings = 0;
    int errors = 0;

    if (rand > 70)
    {
      var colNum = UnitTestStatic.Random.Next(0, 10);
      if (rand < 85)
      {
        dr.SetColumnError(colNum, "First Warning".AddWarningId());
        warnings++;
      }
      else if (rand > 85)
      {
        dr.SetColumnError(colNum, @"First Error");
        errors++;
      }

      rand = UnitTestStatic.Random.Next(-2, 3);
      if (rand == 1)
      {
        dr.SetColumnError(colNum, dr.GetColumnError(colNum).AddMessage("Second Warning".AddWarningId()));
        warnings++;
      }
      else if (rand == 2)
      {
        dr.SetColumnError(colNum, dr.GetColumnError(colNum).AddMessage("Second Error"));
        errors++;
      }
    }

    if (rand > 95)
    {
      dr.RowError = @"Row Error";
      errors++;
    }
    else if (rand > 90)
    {
      dr.RowError = @"Row Warning".AddWarningId();
      warnings++;
    }

    // Artificial metadata columns
    if (errorField)
      dr[ReaderConstants.cErrorField] = dr.GetErrorInformation();

    if (startLine)
      dr[ReaderConstants.cStartLineNumberFieldName] = (long) recordNumber * 2;

    if (recNum)
      dr[ReaderConstants.cRecordNumberFieldName] = (long) recordNumber;

    if (endLine)
      dr[ReaderConstants.cEndLineNumberFieldName] = (long) recordNumber * 2 + 1;

    dataTable.Rows.Add(dr);
    return (warnings, errors);
  }

  public static DataTable GetDataTable(int numRecords = 100, bool addError = true) =>
      GetDataTable(numRecords, true, false, false, addError, out var errorCount, out var warningCount);

  public static DataTable GetDataTable(int numRecords, bool startLine, bool endLine, bool recNum, bool errorField, out long errorCount, out long warningCount)
  {
    warningCount = 0;
    errorCount = 0;

    var dataTable = new DataTable { TableName = "ArtificialTable", Locale = new CultureInfo("en-gb") };
    foreach (var col in Columns)
    {
      dataTable.Columns.Add(col.Name, col.ValueFormat.DataType.GetNetType());
    }

    if (recNum)
      dataTable.Columns.Add(ReaderConstants.cRecordNumberFieldName, typeof(long));

    if (endLine)
      dataTable.Columns.Add(ReaderConstants.cEndLineNumberFieldName, typeof(long));

    if (errorField)
      dataTable.Columns.Add(ReaderConstants.cErrorField, typeof(string));

    if (startLine)
      dataTable.Columns.Add(ReaderConstants.cStartLineNumberFieldName, typeof(long));

    dataTable.BeginLoadData();
    for (var i = 1; i <= numRecords; i++)
    {
      var (newWarning, newErrors) = AddRowToDataTable(dataTable, i, startLine, endLine, recNum, errorField);
      warningCount += newWarning;
      errorCount += newErrors;
    }
    dataTable.EndLoadData();
    return dataTable;
  }

}