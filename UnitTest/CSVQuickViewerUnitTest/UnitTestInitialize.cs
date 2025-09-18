using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class UnitTestInitialize
  {
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
      
      FunctionalDI.FileReaderWriterFactory = new ViewerFileReaderWriterFactory(StandardTimeZoneAdjust.ChangeTimeZone, new FillGuessSettings(true));
    }
  }
}