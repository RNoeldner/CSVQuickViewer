using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FunctionalDITests
  {
    [TestMethod]
    public void GetEncryptedPassphraseTest()
    {
      var setting = new CsvFile {Passphrase = "Hello World"};
      var test = FunctionalDI.GetEncryptedPassphrase(setting);
      Assert.AreEqual(setting.Passphrase, test);
    }

    [TestMethod]
    public async Task GetFileReaderTestAsync()
    {
      var setting = new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt")};
      using (var test = FunctionalDI.GetFileReader(setting, null, new DummyProcessDisplay(UnitTestInitializeCsv.Token)))
      {
        Assert.IsInstanceOfType(test, typeof(CsvFileReader));
      }

      setting.JsonFormat = true;
      using (var test2 = FunctionalDI.GetFileReader(setting, null, new DummyProcessDisplay(UnitTestInitializeCsv.Token)))
      {
        Assert.IsInstanceOfType(test2, typeof(JsonFileReader));
      }
    }

    [TestMethod]
    public async Task ExecuteReaderAsync()
    {
      var setting = new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt")};

      using (var test3 = await FunctionalDI.ExecuteReaderAsync(setting, null, new DummyProcessDisplay(UnitTestInitializeCsv.Token))
        .ConfigureAwait(false))
      {
        Assert.IsInstanceOfType(test3, typeof(CsvFileReader));
      }
    }


    [TestMethod]
    public void GetFileWriterTest()
    {
      var setting = new CsvFile {FileName = UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt")};
      var test = FunctionalDI.GetFileWriter(setting, null, new DummyProcessDisplay(UnitTestInitializeCsv.Token));
      Assert.IsInstanceOfType(test, typeof(CsvFileWriter));

      var setting2 = new StructuredFile {FileName = UnitTestInitializeCsv.GetTestPath("AlternateTextQualifiers.txt")};
      var test2 = FunctionalDI.GetFileWriter(setting2, null, new DummyProcessDisplay(UnitTestInitializeCsv.Token));
      Assert.IsInstanceOfType(test2, typeof(StructuredFileWriter));
    }


    [TestMethod]
    public void AdjustTZTest()
    {
      var srcTime = new DateTime(2020, 02, 20);
      // time zone we convert to can not be changed... 
      var test1 = FunctionalDI.AdjustTZImport(srcTime, "Hawaiian Standard Time",  1, null);
      // as the time of the system is not know, we do not know what we are converting to, people in Hawaiian would need no difference
      Assert.IsNotNull(test1);
      // Convert back should give us the original value though
      var test2 = FunctionalDI.AdjustTZExport(test1, "Hawaiian Standard Time", 1, null);
      Assert.AreEqual(srcTime, test2);

      

      var test3 = FunctionalDI.AdjustTZImport(srcTime, null, 1, null);
      Assert.AreEqual(srcTime, test3);

      var test4 = FunctionalDI.AdjustTZImport(srcTime, TimeZoneInfo.Local.Id, 1, null);
      Assert.AreEqual(srcTime, test4);

    }
  }
}