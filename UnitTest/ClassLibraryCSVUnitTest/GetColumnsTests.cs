using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class GetColumnsTests
  {
    [TestMethod]
    public async Task GetSqlColumnNamesAsyncParameter()
    {
      try
      {
        FunctionalDI.SqlDataReader = (sql, eh, timeout, limit, token) =>
          throw new FileWriterException("SQL Reader not specified");
        await "test".GetColumnsSqlAsync(60, UnitTestStatic.Token);

        Assert.Fail("Expected Exception not thrown");
      }
      catch (FileWriterException)
      {
        // add good
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong Exception Type: " + ex.GetType());
      }
    }

    [TestMethod()]
    public async Task FillGuessColumnFormatWriterAsyncTest()
    {
      var setting1 = new CsvFile { ID="Test1", FileName = UnitTestStatic.GetTestPath("Sessions.txt"), HasFieldHeader = true, ByteOrderMark = true, FieldDelimiter = "\t" };
      UnitTestStatic.MimicSQLReader.AddSetting(setting1);
      var setting2 = new CsvFile { ID="Test2", FileName = UnitTestStatic.GetTestPath("Sessions2.txt"), HasFieldHeader = true, ByteOrderMark = true, FieldDelimiter = "\t", SqlStatement = "Test1" };
      UnitTestStatic.MimicSql();
      await GetColumns.FillGuessColumnFormatWriterAsync(setting2, UnitTestStatic.Token);
      UnitTestStatic.MimicSQLReader.RemoveSetting(setting1);
      Assert.AreEqual(5, setting2.ColumnCollection.Count);
    }

    [TestMethod()]
    public async Task GetColumnsSqlAsyncTest()
    {
      var setting = new CsvFile { ID="nonsese", FileName = UnitTestStatic.GetTestPath("Sessions.txt"), HasFieldHeader = true, ByteOrderMark = true, FieldDelimiter = "\t" };
      setting.ColumnCollection.Add(new Column("Start Date") { Ignore = true });

      UnitTestStatic.MimicSQLReader.AddSetting(setting);

      UnitTestStatic.MimicSql();
      var res = await GetColumns.GetColumnsSqlAsync(setting.ID, 60, UnitTestStatic.Token);
      Assert.AreEqual(5, res.Count());
      UnitTestStatic.MimicSQLReader.RemoveSetting(setting);
    }

    [TestMethod()]
    public async Task GetWriterColumnInformationAsyncTest()
    {
      var setting1 = new CsvFile { ID="Test1", FileName = UnitTestStatic.GetTestPath("Sessions.txt"), HasFieldHeader = true, ByteOrderMark = true, FieldDelimiter = "\t" };
      UnitTestStatic.MimicSQLReader.AddSetting(setting1);
      UnitTestStatic.MimicSql();
      var res = await GetColumns.GetWriterColumnInformationAsync(setting1.ID, 120, new ImmutableValueFormat(), Array.Empty<IColumn>(), UnitTestStatic.Token);
      UnitTestStatic.MimicSQLReader.RemoveSetting(setting1);
      Assert.AreEqual(5, res.Count());
    }
  }
}