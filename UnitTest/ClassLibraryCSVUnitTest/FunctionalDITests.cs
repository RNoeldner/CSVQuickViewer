using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvTools.Tests
{
  [TestClass]
  public class FunctionalDiTests
  {
    [TestMethod]
    public void GetFileReaderTestCsv()
    {
      var setting = new CsvFile("csv", UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt"));
      using var test =
        FunctionalDI.GetFileReader(setting, UnitTestStatic.Token);
      Assert.IsInstanceOfType(test, typeof(CsvFileReader));
    }

    [TestMethod]
    public void GetFileReaderTestJson()
    {
      var setting = new JsonFile("json", UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt"));
      using var test2 =
        FunctionalDI.GetFileReader(setting, UnitTestStatic.Token);
      Assert.IsInstanceOfType(test2, typeof(JsonFileReader));
    }

    [TestMethod]
    public void GetFileWriterTest()
    {
      var setting = new CsvFile("csv", UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt"));
      var test = FunctionalDI.GetFileWriter(setting, UnitTestStatic.Token);
      Assert.IsInstanceOfType(test, typeof(CsvFileWriter));

      var setting2 = new JsonFile("json", UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt"), "{0}");
      var test2 = FunctionalDI.GetFileWriter(setting2, UnitTestStatic.Token);
      Assert.IsInstanceOfType(test2, typeof(StructuredFileWriter));
    }

    [TestMethod]
    public void AdjustTzTest()
    {
      var srcTime = new DateTime(2020, 02, 20);
      // time zone we convert to can not be changed...
#if Windows
      var test1 = FunctionalDI.AdjustTZImport(srcTime, "Hawaiian Standard Time", 1, null);
      // as the time of the system is not know, we do not know what we are converting to, people in
      // Hawaiian would need no difference
      Assert.IsNotNull(test1);
      // Convert back should give us the original value though
      var test2 = FunctionalDI.AdjustTZExport(test1, "Hawaiian Standard Time", 1, null);
      Assert.AreEqual(srcTime, test2);
#endif
#pragma warning disable CS8625 
      var test3 = StandardTimeZoneAdjust.ChangeTimeZone(srcTime, null, TimeZoneInfo.Local.Id, null);
#pragma warning restore CS8625 
      Assert.AreEqual(srcTime, test3);

      var test4 = StandardTimeZoneAdjust.ChangeTimeZone(srcTime, TimeZoneInfo.Local.Id, TimeZoneInfo.Local.Id, null);
      Assert.AreEqual(srcTime, test4);
    }
  }
}