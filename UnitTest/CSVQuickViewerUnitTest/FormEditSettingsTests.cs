using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormEditSettingsTests
  {
    [TestMethod]
    [Timeout(4000)]
    public void FormEditSettings()
    {
      UnitTestStaticForms.ShowForm(() => 
        new FormEditSettings(new ViewSettings(), new CsvFile(id: "csv", fileName: "Dummy1")));
    }
  }
}