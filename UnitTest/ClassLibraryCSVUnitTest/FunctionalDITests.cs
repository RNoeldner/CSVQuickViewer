namespace CsvTools.Tests
{
  using System;

  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass()]
  public class FunctionalDITests
  {
    [TestMethod()]
    public void GetEncryptedPassphraseTest()
    {
      var setting = new CsvFile { Passphrase = "Hello World" };
      var test = FunctionalDI.GetEncryptedPassphrase(setting);
      Assert.AreEqual(setting.Passphrase, test);
    }

    [TestMethod()]
    public async System.Threading.Tasks.Task GetFileReaderTestAsync()
    {

      var setting = new CsvFile {  FileName = UnitTestInitialize.GetTestPath("AlternateTextQualifiers.txt") };
      using (var test = FunctionalDI.GetFileReader(setting, null, new DummyProcessDisplay()))
        Assert.IsInstanceOfType(test, typeof(CsvFileReader));
      
      using (var test3 = await FunctionalDI.ExecuteReaderAsync(setting, null, new DummyProcessDisplay()).ConfigureAwait(false))
        Assert.IsInstanceOfType(test3, typeof(JsonFileReader));

      setting.JsonFormat = true;
      using (var test2 = FunctionalDI.GetFileReader(setting, null, new DummyProcessDisplay()))
        Assert.IsInstanceOfType(test2, typeof(JsonFileReader));

  
    }

    [TestMethod()]
    public void GetFileWriterTest()
    {
      var setting = new CsvFile { FileName = UnitTestInitialize.GetTestPath("AlternateTextQualifiers.txt") };
      var test = FunctionalDI.GetFileWriter(setting, null, new DummyProcessDisplay());
      Assert.IsInstanceOfType(test, typeof(CsvFileWriter));
      
      var setting2 = new StructuredFile { FileName = UnitTestInitialize.GetTestPath("AlternateTextQualifiers.txt") };
      var test2 = FunctionalDI.GetFileWriter(setting2, null, new DummyProcessDisplay());
      Assert.IsInstanceOfType(test2, typeof(StructuredFileWriter));
    }


    [TestMethod()]
    public void AdjustTZTest()
    {
      var srcTime = new DateTime(2020, 02, 20);
      //  (input, srcTimeZone, destTimeZone, columnOrdinal, handleWarning) 
      var test1 = FunctionalDI.AdjustTZ(srcTime, "PST", "PST", 1, null);
      Assert.AreEqual(srcTime, test1);

      var test2 = FunctionalDI.AdjustTZ(srcTime, "PST", null, 1, null);
      Assert.AreEqual(srcTime, test2);

      var test3 = FunctionalDI.AdjustTZ(srcTime, null, "PST", 1, null);
      Assert.AreEqual(srcTime, test3);

      var test4 = FunctionalDI.AdjustTZ(srcTime, "W. Europe Standard Time", "Russian Standard Time", 1, null);
      Assert.AreEqual(srcTime.AddHours(2), test4);

      var test5 = FunctionalDI.AdjustTZ(srcTime, "W. Europe Standard Time", "India Standard Time", 1, null);
      Assert.AreEqual(srcTime.AddHours(4.5), test5);
    }
  }
}