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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  [SuppressMessage("ReSharper", "UseAwaitUsing")]
  public class JsonFileWriterTests
  {

    [TestMethod]
    public void StructuredFileWriterBuildRow()
    {

      using var dt = UnitTestStaticData.GetDataTable(10, false);
      using var reader = new DataTableWrapper(dt);

      var row = @"{""string"":(string),
""int"":(int),
""DateTime"":(DateTime),
""bool"":(bool),
""double"":(double),
""numeric"":(numeric),
""AllEmpty"":(AllEmpty),
""PartEmpty"":(PartEmpty),
""ID"":(ID)}";

      var writer = new JsonFileWriter("id", "dummy.json", string.Empty, "]", "(", false, Encoding.UTF8.CodePage, false, Array.Empty<Column>(), "Test File2", row, StandardTimeZoneAdjust.ChangeTimeZone,
        TimeZoneInfo.Local.Id, string.Empty, false);

      //var writerCols = new List<WriterColumn>();
      //foreach (var col in reader.GetColumnsOfReader())
      //{
      //  if (col.ValueFormat.DataType== DataTypeEnum.DateTime)
      //    writerCols.Add(new WriterColumn(col.Name, new ValueFormat(DataTypeEnum.DateTime, "yyyy/MM/ddThh:mm:ss.000Z", "-", ":"), col.ColumnOrdinal));
      //  else
      //    writerCols.Add(new WriterColumn(col.Name, col.ValueFormat, col.ColumnOrdinal));
      //}

      writer.SetWriterColumns(reader);
      dt.Rows[0][2] = new DateTime(2022, 10, 17, 10, 12, 45, 0, DateTimeKind.Utc);
      reader.Read();
      var result = writer.BuildRow(row, reader);

      Assert.IsTrue(result.Contains("\"AllEmpty\":\"\","));
      Assert.IsTrue(result.Contains("\"ID\":1"));
      Assert.IsTrue(result.Contains("\"DateTime\":\"2022-10-17T10:12:45"));

      dt.Rows[0][7] = "Part1|Part2|Part3|Part4";
      var row2 = @"{""ID"":(ID), ""PartEmpty"":\[(PartEmpty)\]}";
      var result2 = writer.BuildRow(row2, reader);
      //TODO: What is correct 
      //    Assert.IsTrue(result2.Contains("\"PartEmpty\":[\"Part1|Part2|Part3|Part4\"]"), result2);  

      // Assert.IsTrue(result2.Contains("\"PartEmpty\":[\"Part1\",\"Part2\",\"Part3\",\"Part4\"]"), result2);  


      dt.Rows[0][6] = "Lang1|Lang2";
      dt.Rows[0][7] = "Part1|Part2";
      var row3 = @"{""ID"":(ID), ""Array"":\[{""Part"":(PartEmpty), ""Lang"":(AllEmpty)}\] }";
      var result3 = writer.BuildRow(row3, reader);

      //      Assert.IsTrue(result3.Contains("\"Array\":[{\"Part\":\"Part1|Part2\",\"Lang\":\"Lang1|Lang2\"}]");      
      // Assert.IsTrue(result3.Contains("\"Array\":[{\"Part\":\"Part1\", \"Lang\":\"Lang1\"},{\"Part\":\"Part2\", \"Lang\":\"Lang2\"}]"), result3);  
    }

    [TestMethod()]
    public async Task JsonFileWriterTest_EmptyAsNullFalse()
    {
      var fileName = UnitTestStatic.GetTestPath("abc1.json");

      FileSystemUtils.FileDelete(fileName);

      // source data
      using var reader = new CsvFileReader(UnitTestStatic.GetTestPath("AllFormats.txt"), Encoding.UTF8.CodePage, 0,
        true,
        new Column[]
        {
          new Column("DateTime", new ValueFormat(DataTypeEnum.DateTime), 0, true),
          new Column("Integer", new ValueFormat(DataTypeEnum.Integer), 0, true)
        }, TrimmingOptionEnum.All, '\t', '"', char.MinValue,
        0, false, false, "", 0,
        true, "", "", "", true,
        false, false, true, true, false,
        true, true, true, true, false, treatTextAsNull: "NULL", skipEmptyLines: true, consecutiveEmptyRowsMax: 4,
        identifierInContainer: String.Empty, timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone,
        destTimeZone: TimeZoneInfo.Local.Id, true, true);
      await reader.OpenAsync(UnitTestStatic.Token);

      var row = JsonFileWriter.GetJsonRow(reader.GetColumnsOfReader());


      // writer 
      var writer = new JsonFileWriter("id", fileName, string.Empty, "]",
        "[", false, Encoding.UTF8.CodePage, false, Array.Empty<Column>(), "Test File2", row, StandardTimeZoneAdjust.ChangeTimeZone,
        TimeZoneInfo.Local.Id, string.Empty, false);

      await writer.WriteAsync(reader, UnitTestStatic.Token);

      var fileText = FileSystemUtils.ReadAllText(fileName);
      FileSystemUtils.FileDelete(fileName);
      // can be read as Json
      var jsonResult = fileText.DeserializeJson();

      Assert.IsInstanceOfType(jsonResult, typeof(JArray));
    }

    [TestMethod()]
    public async Task JsonFileWriterTest_EmptyAsNullTrue()
    {
      var fileName = UnitTestStatic.GetTestPath("abc2.json");
      FileSystemUtils.FileDelete(fileName);

      // source data
      using var reader = new CsvFileReader(UnitTestStatic.GetTestPath("AllFormats.txt"), Encoding.UTF8.CodePage, 0,
        true,
        new[]
        {
          new Column("DateTime", new ValueFormat(DataTypeEnum.DateTime), 0, true),
          new Column("Integer", new ValueFormat(DataTypeEnum.Integer), 0, true)
        }, TrimmingOptionEnum.All, '\t', '"', char.MinValue,
        0, false, false, "", 0,
        true, "", "", "", true,
        false, false, true, true, false,
        true, true, true, true, false, treatTextAsNull: "NULL", skipEmptyLines: true, consecutiveEmptyRowsMax: 4,
        identifierInContainer: String.Empty, timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone,
        destTimeZone: TimeZoneInfo.Local.Id, true, true);
      await reader.OpenAsync(UnitTestStatic.Token);

      var row = JsonFileWriter.GetJsonRow(reader.GetColumnsOfReader());

      // writer 
      var writer = new JsonFileWriter("id", fileName, string.Empty, "]",
        "[", true, Encoding.UTF8.CodePage, false, Array.Empty<Column>(), "Test File2", row, StandardTimeZoneAdjust.ChangeTimeZone,
        TimeZoneInfo.Local.Id, string.Empty, false);

      await writer.WriteAsync(reader, UnitTestStatic.Token);

      var fileText = FileSystemUtils.ReadAllText(fileName);
      FileSystemUtils.FileDelete(fileName);
      // can be read as Json
      var jsonResult = fileText.DeserializeJson();

      Assert.IsInstanceOfType(jsonResult, typeof(JArray));
    }

    [TestMethod()]
    public async Task JsonFileWriterTest_DataTable()
    {
      var fileName = UnitTestStatic.GetTestPath("abc3.json");
      FileSystemUtils.FileDelete(fileName);

      using var dt = UnitTestStaticData.GetDataTable(100, false);
      using var reader = new DataTableWrapper(dt);

      var row = JsonFileWriter.GetJsonRow(reader.GetColumnsOfReader());

      // writer 
      var writer = new JsonFileWriter("id", fileName, string.Empty, "]",
        "[", false, Encoding.UTF8.CodePage, false, null, "Test File2", row, StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id, string.Empty, false);

      await writer.WriteAsync(reader, UnitTestStatic.Token);
      Assert.IsTrue(FileSystemUtils.FileExists(fileName));

      var fileText = FileSystemUtils.ReadAllText(fileName);
      FileSystemUtils.FileDelete(fileName);
      // can be read as Json
      var jsonResult = fileText.DeserializeJson();

      Assert.IsInstanceOfType(jsonResult, typeof(JArray));
      Assert.AreEqual(dt.Rows.Count, ((jsonResult as JArray)!).Count);
    }
  }
}