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
        FunctionalDI.SqlDataReader = (a, b, c, d) =>
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
    public async Task GetColumnsSqlAsyncTest()
    {
      var setting = new CsvFile("ID", UnitTestStatic.GetTestPath("Sessions.txt"))
      {
        HasFieldHeader = true, ByteOrderMark = true, FieldDelimiter = "\t"
      };
      setting.ColumnCollection.Add(new Column("Start Date", ValueFormat.Empty, ignore: true));
      UnitTestStaticData.MimicSQLReader.AddSetting(setting);

      UnitTestStaticData.MimicSql();
      var res = await setting.ID.GetColumnsSqlAsync(60, UnitTestStatic.Token);
      Assert.AreEqual(5, res.Count());
      UnitTestStaticData.MimicSQLReader.RemoveSetting(setting);
    }
  }
}