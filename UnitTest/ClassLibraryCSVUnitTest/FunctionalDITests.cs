using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvTools.Tests
{
  [TestClass]
  public class FunctionalDITests
  {
    [TestMethod]
    public void GetFileReaderTestCsv()
    {
      var setting = new CsvFile { FileName = UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt") };
      using var test =
        FunctionalDI.GetFileReader(setting, null, new CustomProcessDisplay(UnitTestStatic.Token));
      Assert.IsInstanceOfType(test, typeof(CsvFileReader));
    }

    [TestMethod]
    public void GetFileReaderTestJson()
    {
      var setting = new JsonFile { FileName = UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt") };
      using var test2 =
        FunctionalDI.GetFileReader(setting, null, new CustomProcessDisplay(UnitTestStatic.Token));
      Assert.IsInstanceOfType(test2, typeof(JsonFileReader));
    }

    [TestMethod]
    public void GetFileWriterTest()
    {
      var setting = new CsvFile { FileName = UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt") };
      var test = FunctionalDI.GetFileWriter(setting, new CustomProcessDisplay(UnitTestStatic.Token));
      Assert.IsInstanceOfType(test, typeof(CsvFileWriter));

      var setting2 = new JsonFile { FileName = UnitTestStatic.GetTestPath("AlternateTextQualifiers.txt"), Row = "{0}" };
      var test2 = FunctionalDI.GetFileWriter(setting2, new CustomProcessDisplay(UnitTestStatic.Token));
      Assert.IsInstanceOfType(test2, typeof(StructuredFileWriter));
    }

    [TestMethod]
    public void AdjustTZTest()
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
      var test3 = FunctionalDI.AdjustTZImport(srcTime, null, null);
      Assert.AreEqual(srcTime, test3);

      var test4 = FunctionalDI.AdjustTZImport(srcTime, TimeZoneInfo.Local.Id, null);
      Assert.AreEqual(srcTime, test4);
    }
  }
}