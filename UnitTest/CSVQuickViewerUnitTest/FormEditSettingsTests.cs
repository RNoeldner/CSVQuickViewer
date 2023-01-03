using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormEditSettingsTests
  {
    [TestMethod]
    [Timeout(5000)]
    public void FormEditSettings()
    {
      UnitTestStaticForms.OpenFormSts(() =>
        new FormEditSettings(new ViewSettings(), new CsvFile(id: "csv", fileName: "Dummy1")));
    }
  }
}