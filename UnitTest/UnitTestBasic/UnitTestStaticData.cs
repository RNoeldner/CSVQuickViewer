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

#nullable enable

using System;
using System.Data;
using System.Globalization;
using System.IO;

// ReSharper disable RedundantExplicitArrayCreation
// ReSharper disable ArrangeObjectCreationWhenTypeEvident

namespace CsvTools.Tests
{
  public static class UnitTestStaticData
  {
#if !QUICK
    public static ColumnMut[] ColumnsDt =
    {
      new ColumnMut("string"), //0
      new ColumnMut("int", new ValueFormat(DataTypeEnum.Integer)), //1
      new ColumnMut("DateTime", new ValueFormat(DataTypeEnum.DateTime)), //2
      new ColumnMut("bool", new ValueFormat(DataTypeEnum.Boolean)), //3
      new ColumnMut("double", new ValueFormat(DataTypeEnum.Double)), //4
      new ColumnMut("numeric", new ValueFormat(DataTypeEnum.Numeric)), //5
      new ColumnMut("AllEmpty"), //6
      new ColumnMut("PartEmpty"), //7
      new ColumnMut("ID", new ValueFormat(DataTypeEnum.Integer)) //8
    };

    public static ColumnMut[] ColumnsDt2 = { new ColumnMut("string") };

#endif

    public static void AddRowToDataTable(DataTable dataTable, int recNum, bool addError)
    {
      var minDate = DateTime.Now.AddYears(-20).Ticks;
      var maxDate = DateTime.Now.AddYears(5).Ticks;
      var dr = dataTable.NewRow();
      dr[0] = UnitTestStatic.GetRandomText(50);
      if (recNum % 10 == 0)
        dr[0] = dr[0] + "\r\nA Second Line";

      dr[1] = UnitTestStatic.Random.Next(-300, +600);

      if (UnitTestStatic.Random.NextDouble() > .2)
      {
        var dtm = (((maxDate - minDate) * UnitTestStatic.Random.NextDouble()) + minDate).ToInt64();
        dr[2] = new DateTime(dtm);
      }

      dr[3] = UnitTestStatic.Random.Next(0, 2) == 0;

      dr[4] = UnitTestStatic.Random.NextDouble() * 123.78;

      if (recNum % 3 == 0)
        dr[5] = UnitTestStatic.Random.NextDouble();

      if (UnitTestStatic.Random.NextDouble() > .4) dr[7] = UnitTestStatic.GetRandomText(100);

      dr[8] = recNum; // ID
      dr[9] = recNum * 2; // #Line

      // Add Errors and Warnings to Columns and Rows
      var rand = UnitTestStatic.Random.Next(0, 100);
      if (rand > 70)
      {
        var colNum = UnitTestStatic.Random.Next(0, 10);
        if (rand < 85)
          dr.SetColumnError(colNum, "First Warning".AddWarningId());
        else if (rand > 85) dr.SetColumnError(colNum, @"First Error");

        // Add a possible second error in the same column
        rand = UnitTestStatic.Random.Next(-2, 3);
        if (rand == 1)
          dr.SetColumnError(colNum, dr.GetColumnError(colNum).AddMessage("Second Warning".AddWarningId()));
        else if (rand == 2) dr.SetColumnError(colNum, dr.GetColumnError(colNum).AddMessage("Second Error"));
      }

      if (rand > 80) dr.RowError = rand > 90 ? @"Row Error" : @"Row Warning".AddWarningId();
      if (addError)
        dr[10] = dr.GetErrorInformation();

      dataTable.Rows.Add(dr);
    }

    public static DataTable GetDataTable(int numRecords = 100, bool addError = true)
    {
      var dataTable = new DataTable { TableName = "ArtificialTable", Locale = new CultureInfo("en-gb") };
      dataTable.Columns.Add("string", typeof(string));
      dataTable.Columns.Add("int", typeof(int));
      dataTable.Columns.Add("DateTime", typeof(DateTime));
      dataTable.Columns.Add("bool", typeof(bool));
      dataTable.Columns.Add("double", typeof(double));
      dataTable.Columns.Add("numeric", typeof(decimal));
      dataTable.Columns.Add("AllEmpty", typeof(string));
      dataTable.Columns.Add("PartEmpty", typeof(string));
      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add(ReaderConstants.cStartLineNumberFieldName, typeof(long));
      if (addError)
        dataTable.Columns.Add(ReaderConstants.cErrorField, typeof(string));

      dataTable.BeginLoadData();
      for (var i = 1; i <= numRecords; i++) AddRowToDataTable(dataTable, i, addError);
      dataTable.EndLoadData();
      return dataTable;
    }

    public static DataTable GetDataTable2(long numRecords = 100)
    {
      var dataTable = new DataTable { TableName = "ArtificialTable2", Locale = new CultureInfo("en-gb") };
      dataTable.Columns.Add("string", typeof(string));
      for (long i = 1; i <= numRecords; i++)
      {
        var dr = dataTable.NewRow();
        dr[0] = i.ToString(CultureInfo.InvariantCulture);
        dataTable.Rows.Add(dr);
      }

      return dataTable;
    }



    public static DataTable RandomDataTable(int records)
    {
      var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture };

      dataTable.Columns.Add("ID", typeof(int));
      dataTable.Columns.Add("Text", typeof(string));
      dataTable.Columns.Add("ColText1", typeof(string));
      dataTable.Columns.Add("ColText2", typeof(string));
      dataTable.Columns.Add("ColTextDT", typeof(DateTime));
      var random = new Random(new Guid().GetHashCode());
      for (var i = 0; i < records; i++)
      {
        var row = dataTable.NewRow();
        row["ID"] = i;
        row["Text"] = i.ToString(CultureInfo.CurrentCulture);
        row["ColText1"] = $"Test{i + 1}";
        row["ColText2"] = $"Text {i * 2} !";
        row["ColTextDT"] = new DateTime(random.Next(1900, 2030), random.Next(1, 12), 1).AddDays(random.Next(1, 31));
        dataTable.Rows.Add(row);
      }

      return dataTable;
    }
#if !QUICK
    public static CsvFile ReaderGetAllFormats(string id = "AllFormats")
    {
      var readFile = new CsvFile(id: id, fileName: Path.Combine(UnitTestStatic.GetTestPath("AllFormats.txt")))
      {
        HasFieldHeader = true, FieldDelimiter = "TAB"
      };
      readFile.ColumnCollection.AddRangeNoClone(
        new Column[]
        {
          new Column("DateTime", new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: @"dd/MM/yyyy"),
            timePart: "Time", timePartFormat: "HH:mm:ss"),
          new Column("Integer", new ValueFormat(DataTypeEnum.Integer)),
          new Column("Numeric", new ValueFormat(DataTypeEnum.Numeric, decimalSeparator: ".")),
          new Column("Double", new ValueFormat(dataType: DataTypeEnum.Double, decimalSeparator: ".")),
          new Column("Boolean", new ValueFormat(DataTypeEnum.Boolean)),
          new Column("GUID", new ValueFormat(DataTypeEnum.Guid)),
          new Column("Time", new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: "HH:mm:ss"), ignore: true)
        });

      return readFile;
    }

    public static CsvFile ReaderGetBasicCSV(string id = "BasicCSV")
    {
      var readFile =
        new CsvFile(id: id, fileName: Path.Combine(UnitTestStatic.GetTestPath(("BasicCSV.txt")))) { CommentLine = "#" };
      readFile.ColumnCollection.AddRangeNoClone(
        new Column[]
        {
          new Column("ExamDate", new ValueFormat(DataTypeEnum.DateTime, @"dd/MM/yyyy")),
          new Column("Score", new ValueFormat(DataTypeEnum.Integer)),
          new Column("Proficiency", new ValueFormat(DataTypeEnum.Numeric)),
          new Column("IsNativeLang", new ValueFormat(DataTypeEnum.Boolean))
        });
      return readFile;
    }
#endif
  }
}