using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class JsonFileWriterTests
  {

    [TestMethod()]
    public async Task JsonFileWriterTest_EmptyAsNullFalse()
    {

      var fileName = UnitTestStatic.GetTestPath("abc1.json");

      FileSystemUtils.FileDelete(fileName);
      var customProcess = new CustomProcessDisplay();

      // source data
      using var reader = new CsvFileReader(UnitTestStatic.GetTestPath("AllFormats.txt"), Encoding.UTF8.CodePage, 0,
        true,
        new IColumn[]
        {
          new ImmutableColumn("DateTime", new ImmutableValueFormat(DataTypeEnum.DateTime), 0, true, "", true),
          new ImmutableColumn("Integer", new ImmutableValueFormat(DataTypeEnum.Integer), 0, true, "", true)
        }, TrimmingOptionEnum.All, "\t", "\"", "",
        0, false, false, "", 0,
        true, "", "", "", true,
        false, false, true, true, false,
        true, true, true, true, false, treatTextAsNull: "NULL", skipEmptyLines: true, consecutiveEmptyRowsMax: 4,
        identifierInContainer: String.Empty, timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone, destTimeZone: System.TimeZoneInfo.Local.Id);
      await reader.OpenAsync(UnitTestStatic.Token);

      var row = StructuredFileWriter.GetJsonRow(reader.GetColumnsOfReader());

      var columns = new List<IColumn>();

      // writer 
      var writer = new JsonFileWriter("id", fileName, 0, false, string.Empty,
        "]", "[", false, Encoding.UTF8.CodePage, false, columns, "Test File2", row, StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id);

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
        new IColumn[]
        {
          new ImmutableColumn("DateTime", new ImmutableValueFormat(DataTypeEnum.DateTime), 0, true, "", true),
          new ImmutableColumn("Integer", new ImmutableValueFormat(DataTypeEnum.Integer), 0, true, "", true)
        }, TrimmingOptionEnum.All, "\t", "\"", "",
        0, false, false, "", 0,
        true, "", "", "", true,
        false, false, true, true, false,
        true, true, true, true, false, treatTextAsNull: "NULL", skipEmptyLines: true, consecutiveEmptyRowsMax: 4,
        identifierInContainer: String.Empty, timeZoneAdjust: StandardTimeZoneAdjust.ChangeTimeZone, destTimeZone: System.TimeZoneInfo.Local.Id);
      await reader.OpenAsync(UnitTestStatic.Token);

      var row = StructuredFileWriter.GetJsonRow(reader.GetColumnsOfReader());

      var columns = new List<IColumn>();

      // writer 
      var writer = new JsonFileWriter("id", fileName, 0, false, string.Empty,
        "]", "[", true, Encoding.UTF8.CodePage, false, columns, "Test File2", row, StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id);

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

      using var dt = UnitTestStatic.GetDataTable(100, false);
      using var reader = new DataTableWrapper(dt);
      
      var row = StructuredFileWriter.GetJsonRow(reader.GetColumnsOfReader());

      // writer 
      var writer = new JsonFileWriter("id", fileName, 0, false, string.Empty,
        "]", "[", false, Encoding.UTF8.CodePage, false, null, "Test File2", row, StandardTimeZoneAdjust.ChangeTimeZone, System.TimeZoneInfo.Local.Id);

      await writer.WriteAsync(reader, UnitTestStatic.Token);

      var fileText = FileSystemUtils.ReadAllText(fileName);
      FileSystemUtils.FileDelete(fileName);
      // can be read as Json
      var jsonResult = fileText.DeserializeJson();

      Assert.IsInstanceOfType(jsonResult, typeof(JArray));
      Assert.AreEqual(dt.Rows.Count, ((jsonResult as JArray)!).Count);
    }
  }
}