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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  [SuppressMessage("ReSharper", "UseAwaitUsing")]
  public class JsonFileWriterTests
  {
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
        }, TrimmingOptionEnum.All, "\t", "\"", "",
        0, false, false, "", 0,
        true, "", "", "", true,
        false, false, true, true, false,
        true, true, true, true, false, treatTextAsNull: "NULL", skipEmptyLines: true, consecutiveEmptyRowsMax: 4,
        identifierInContainer: String.Empty, timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone,
        destTimeZone: TimeZoneInfo.Local.Id, true, true);
      await reader.OpenAsync(UnitTestStatic.Token);

      var row = StructuredFileWriter.GetJsonRow(reader.GetColumnsOfReader());


      // writer 
      var writer = new JsonFileWriter("id", fileName, 0, false, string.Empty,
        "]", "[", false, Encoding.UTF8.CodePage, false, Array.Empty<Column>(), "Test File2", row,
        StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id);

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
        }, TrimmingOptionEnum.All, "\t", "\"", "",
        0, false, false, "", 0,
        true, "", "", "", true,
        false, false, true, true, false,
        true, true, true, true, false, treatTextAsNull: "NULL", skipEmptyLines: true, consecutiveEmptyRowsMax: 4,
        identifierInContainer: String.Empty, timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone,
        destTimeZone: TimeZoneInfo.Local.Id, true, true);
      await reader.OpenAsync(UnitTestStatic.Token);

      var row = StructuredFileWriter.GetJsonRow(reader.GetColumnsOfReader());

      // writer 
      var writer = new JsonFileWriter("id", fileName, 0, false, string.Empty,
        "]", "[", true, Encoding.UTF8.CodePage, false, Array.Empty<Column>(), "Test File2", row,
        StandardTimeZoneAdjust.ChangeTimeZone, TimeZoneInfo.Local.Id);

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

      var row = StructuredFileWriter.GetJsonRow(reader.GetColumnsOfReader());

      // writer 
      var writer = new JsonFileWriter("id", fileName, 0, false, string.Empty,
        "]", "[", false, Encoding.UTF8.CodePage, false, null, "Test File2", row, StandardTimeZoneAdjust.ChangeTimeZone,
        TimeZoneInfo.Local.Id);

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