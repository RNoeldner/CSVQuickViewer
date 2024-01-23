using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace CsvTools.Tests
{
  [TestClass()]
  public class ViewerFileReaderWriterFactoryTests
  {

    [TestMethod]
    public void GetFileReaderTestJson()
    {
      var setting = new CsvFileDummy() { FileName= UnitTestStatic.GetTestPath("AllFormatsPipe.csv"), IsJson= true, };
      var fact = new ViewerFileReaderWriterFactory(StandardTimeZoneAdjust.ChangeTimeZone, new FillGuessSettings(true));
      using var test2 = fact.GetFileReader(setting, UnitTestStatic.Token);
      Assert.IsInstanceOfType(test2, typeof(JsonFileReader));
    }

    [TestMethod]
    public void GetFileReaderTestXml()
    {
      var setting = new CsvFileDummy() { FileName= UnitTestStatic.GetTestPath("AllFormatsPipe.csv"), IsXml= true, };
      var fact = new ViewerFileReaderWriterFactory(StandardTimeZoneAdjust.ChangeTimeZone, new FillGuessSettings(true));
      using var test2 = fact.GetFileReader(setting, UnitTestStatic.Token);
      Assert.IsInstanceOfType(test2, typeof(XmlFileReader));
    }
  }
}