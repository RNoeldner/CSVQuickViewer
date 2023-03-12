using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class UnitTestInitialize
  {

    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
      Logger.LoggerInstance = UnitTestStatic.SetupTestContextLogger(context);
      FunctionalDI.FileReaderWriterFactory = new ClassLibraryCsvFileReaderWriterFactory(StandardTimeZoneAdjust.ChangeTimeZone, new FillGuessSettings(true));      
    }

    
    public static void SetApplicationPGPSetting()
    {
      FunctionalDI.GetKeyForFile= _ =>  PGPKeyTestHelper.cPrivate;
      FunctionalDI.GetPassphraseForFile = _ =>  PGPKeyTestHelper.Passphrase;
      FunctionalDI.GetKeyAndPassphraseForFile =
        _ => (PGPKeyTestHelper.Passphrase, string.Empty, PGPKeyTestHelper.cPrivate);
    }

  }
}