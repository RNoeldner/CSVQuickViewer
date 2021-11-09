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
      using var frm = new FormEditSettings(new ViewSettings(), new CsvFile());
      UnitTestStatic.ShowFormAndClose(frm);
    }
  }
}