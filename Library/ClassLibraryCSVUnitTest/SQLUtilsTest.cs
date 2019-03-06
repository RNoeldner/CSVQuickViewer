using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass]
  public class SQLUtilityTest
  {
    [TestMethod]
    public void GetComamndTextWithoutComment1()
    {
      var test = "SELECT * FROM Test";
      Assert.AreEqual(test, test.GetCommandTextWithoutComment());
    }

    [TestMethod]
    public void RenameSQLTable()
    {
      Assert.AreEqual("SELECT * FROM Hello", "SELECT * FROM Test".RenameSQLTable("Test", "Hello"));
      Assert.AreEqual("SELECT * FROM [Hello] as Testing Inner JOIN Hello as T2 ON Testing.ID = T2.ID",
        "SELECT * FROM [Test] as Testing Inner JOIN tEst as T2 ON Testing.ID = T2.ID".RenameSQLTable("Test", "Hello"));
    }

    [TestMethod]
    public void GetComamndTextWithoutComment2()
    {
      var test = "SELECT * FROM Test -- Hello";
      Assert.AreEqual("SELECT * FROM Test", test.GetCommandTextWithoutComment().TrimEnd());
    }

    [TestMethod]
    public void GetComamndTextWithoutComment3()
    {
      var test = @"/* a comment
line */
SELECT * FROM
Test";
      Assert.AreEqual("\n\nSELECT * FROM\nTest", test.GetCommandTextWithoutComment());
    }

    [TestMethod]
    public void GetComamndTextWithoutComment4()
    {
      var test = @"SELECT * FROM [Test]";
      Assert.AreEqual("SELECT * FROM [Test]", test.GetCommandTextWithoutComment());
    }

    [TestMethod]
    public void GetComamndTextWithoutComment5()
    {
      var test = @"SELECT 'Hallo -- ' as test FROM Test";
      Assert.AreEqual("SELECT 'Hallo -- ' as test FROM Test", test.GetCommandTextWithoutComment());
    }

    [TestMethod]
    public void GetComamndTextWithoutComment6()
    {
      var test = @"SELECT 'Hallo  /*' as test FROM Test";
      Assert.AreEqual("SELECT 'Hallo  /*' as test FROM Test", test.GetCommandTextWithoutComment());
    }

    [TestMethod]
    public void GetComamndTextWithoutComment7()
    {
      var test = @"SELECT /* Comment in here */ 'Hallo --' -- Another commend
as test FROM Test";
      Assert.AreEqual("SELECT  'Hallo --' \nas test FROM Test", test.GetCommandTextWithoutComment());
    }

    [TestMethod]
    public void GetComamndTextWithoutComment8()
    {
      var test = @"SELECT /**/ 'A' as  J FROM -- Line
Test";
      Assert.AreEqual("SELECT  'A' as  J FROM \nTest", test.GetCommandTextWithoutComment());
    }

    [TestMethod]
    public void GetTableNames1()
    {
      var test = @"SELECT * FROM Test";
      var result = test.GetSQLTableNames();
      Assert.AreEqual(1, result.Count());
      Assert.AreEqual("Test", result.First());
    }

    [TestMethod]
    public void GetTableNames2()
    {
      var test = @"SELECT * FROM [Test]";
      var result = test.GetSQLTableNames();
      Assert.AreEqual(1, result.Count());
      Assert.AreEqual("Test", result.First());
    }

    [TestMethod]
    public void GetTableNames3()
    {
      var test = "SELECT * FROM \"Test\"";
      var result = test.GetSQLTableNames();
      Assert.AreEqual(1, result.Count());
      Assert.AreEqual("Test", result.First());
    }

    [TestMethod]
    public void GetTableNames4()
    {
      var test = "SELECT * FROM \"Test\" INNER JOIN Other";
      var result = test.GetSQLTableNames();
      Assert.AreEqual(2, result.Count());
      Assert.AreEqual("Test", result.First());
      Assert.AreEqual("Other", result.Last());
    }

    [TestMethod]
    public void GetTableNames5()
    {
      var test = "SELECT * FROM [TEST] INNER JOIN HAllo OUTER JOIN \"te[st\" FROM Hugo";
      var result = test.GetSQLTableNames();
      //Assert.AreEqual(4, result.Count());
      Assert.IsTrue(result.Contains("TEST"), "Test");
      Assert.IsTrue(result.Contains("HAllo"), "Hallo");
      Assert.IsTrue(result.Contains("te[st"), "te[st");
      Assert.IsTrue(result.Contains("Hugo"), "Hugo");
    }

    [TestMethod]
    public void SplitCommandTextByGo1()
    {
      var test = @"/* a comment
line */
SELECT * FROM
Test";
      Assert.AreEqual("SELECT * FROM\nTest", test.SplitCommandTextByGo().First());
    }

    [TestMethod]
    public void SplitCommandTextByGo2()
    {
      var test = @"/* a comment
line */
SELECT * FROM
Test
GO
INSERT INTO [Hallo] (ColumnName)";
      Assert.AreEqual("SELECT * FROM\nTest", test.SplitCommandTextByGo().First());
      Assert.AreEqual("INSERT INTO [Hallo] (ColumnName)", test.SplitCommandTextByGo().Last());
    }
  }
}