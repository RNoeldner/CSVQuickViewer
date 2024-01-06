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
using System.Threading.Tasks;

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
      new ColumnMut("guid", new ValueFormat(DataTypeEnum.Numeric)), // 6
      new ColumnMut("AllEmpty"), //7
      new ColumnMut("PartEmpty"), //8
      new ColumnMut("ID", new ValueFormat(DataTypeEnum.Integer)) //9
    };
#endif



    /// <summary>
    /// Adds the row to data table, the table should have the right columns already added
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="recNum">The record number.</param>
    /// <param name="addError">if set to <c>true</c> add error column, this is independent from Row And Column Errors</param>
    /// <param name="addStartLineNo">if set to <c>true</c> add start line no.</param>
    /// <param name="addRecordNumber">if set to <c>true</c> add record number.</param>
    /// <param name="addEndLineNo">if set to <c>true</c> add end line no.</param>
    /// <returns>Total number or errors / warning added to the Row</returns>
    public static (int warnings, int errors) AddRowToDataTable(DataTable dataTable, int recNum, bool addError, bool addStartLineNo, bool addRecordNumber, bool addEndLineNo)
    {
      var minDate = DateTime.Now.AddYears(-20).Ticks;
      var maxDate = DateTime.Now.AddYears(5).Ticks;
      var dr = dataTable.NewRow();

      var colIndex = 0;

      // 0
      dr[colIndex] = UnitTestStatic.GetRandomText(25);
      if (recNum % 10 == 0) dr[colIndex] = dr[colIndex] + "\r\nA Second Line";

      // 1
      dr[++colIndex] = UnitTestStatic.Random.Next(-300, +600);

      // 2
      colIndex++;
      if (recNum % 5 == 0)
        dr[colIndex] = new DateTime((((maxDate - minDate) * UnitTestStatic.Random.NextDouble()) + minDate).ToInt64());

      // 3 bool
      dr[++colIndex] = UnitTestStatic.Random.Next(0, 2) == 0;

      // 4 double
      dr[++colIndex] = Math.Round(UnitTestStatic.Random.NextDouble() * 123.78, 4);

      // 5 numeric
      colIndex++;
      if (recNum % 4 == 0) dr[colIndex] = Math.Round(UnitTestStatic.Random.NextDouble(), 4);

      // 6 guid
      colIndex++;
      if (recNum % 3 == 0) dr[colIndex] = Guid.NewGuid();

      // 7 All empty
      colIndex++;

      // 8 PartEmpty
      colIndex++;
      if (recNum % 2 == 0) dr[colIndex] = UnitTestStatic.GetRandomText(100);

      // 9 ID
      dr[++colIndex] = recNum;

      // Add Errors and Warnings to Columns and Rows
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

        // Add a possible second error in the same column
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

      // 10
      if (addError)
        dr[++colIndex] = dr.GetErrorInformation();

      // 11
      if (addStartLineNo)
        dr[++colIndex] = recNum * 2+1;

      // 12
      if (addRecordNumber)
        dr[++colIndex] = recNum;

      // 13
      if (addEndLineNo)
        dr[++colIndex] = recNum * 2;

      dataTable.Rows.Add(dr);
      return (warnings, errors);
    }

    /// <summary>
    /// Get a data table with random sample data
    /// </summary>
    /// <param name="numRecords">The number records the table should have.</param>
    /// <param name="addError">if set to <c>true</c> add an error column.</param>
    /// <returns>A data table with sample data</returns>
    public static DataTable GetDataTable(int numRecords = 100, bool addError = true) => GetDataTable(numRecords, true, false, false, addError, out var errorCount, out var warningCount);

    /// <summary>
    /// Get a data table with random sample data
    /// </summary>
    /// <param name="numRecords">The number records the table should have.</param>
    /// <param name="addStartLineNo">if set to <c>true</c> add a start line column.</param>
    /// <param name="addEndLineNo">if set to <c>true</c> add end line column.</param>
    /// <param name="addRecordNumber">if set to <c>true</c> add a record number column.</param>
    /// <param name="addError">if set to <c>true</c> add an error column.</param>
    /// <param name="errorCount">The error count in the table.</param>
    /// <param name="warningCount">The warning count in the table..</param>
    /// <returns>A data table with sample data</returns>
    public static DataTable GetDataTable(int numRecords, bool addStartLineNo, bool addEndLineNo, bool addRecordNumber, bool addError, out long errorCount, out long warningCount)
    {
      warningCount = 0;
      errorCount = 0;

      var dataTable = new DataTable { TableName = "ArtificialTable", Locale = new CultureInfo("en-gb") };
      dataTable.Columns.Add("string", typeof(string));
      dataTable.Columns.Add("int", typeof(int));
      dataTable.Columns.Add("DateTime", typeof(DateTime));
      dataTable.Columns.Add("bool", typeof(bool));
      dataTable.Columns.Add("double", typeof(double));
      dataTable.Columns.Add("numeric", typeof(decimal));
      dataTable.Columns.Add("guid", typeof(Guid));
      dataTable.Columns.Add("AllEmpty", typeof(string));
      dataTable.Columns.Add("PartEmpty", typeof(string));
      dataTable.Columns.Add("ID", typeof(long));

      // Order of artificial columns based on ReaderMapping
      if (addRecordNumber)
        dataTable.Columns.Add(ReaderConstants.cRecordNumberFieldName, typeof(long));

      if (addEndLineNo)
        dataTable.Columns.Add(ReaderConstants.cEndLineNumberFieldName, typeof(long));

      if (addError)
        dataTable.Columns.Add(ReaderConstants.cErrorField, typeof(string));

      if (addStartLineNo)
        dataTable.Columns.Add(ReaderConstants.cStartLineNumberFieldName, typeof(long));

      dataTable.BeginLoadData();
      for (var i = 1; i <= numRecords; i++)
      {
        (var newWarning, var newErrors) = AddRowToDataTable(dataTable, i, addError, addStartLineNo, addEndLineNo, addRecordNumber);
        warningCount += newWarning;
        errorCount += newErrors;
      }
      dataTable.EndLoadData();
      return dataTable;
    }

    /// <summary>
    /// TODO: Not sure why this is needed if we have GetDataTable
    /// </summary>
    [Obsolete("Use GetDataTable instead")]
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
        HasFieldHeader = true,
        FieldDelimiterChar = '\t'
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