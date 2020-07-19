using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormEditSettingsTests
  {
    [TestMethod]
    public void FormEditSettings()
    {
      using (var frm = new FormEditSettings(new ViewSettings()))
      {
        UnitTestWinFormHelper.ShowFormAndClose(frm);
      }
    }

    [TestMethod]
    public void FormEditSettingsTest1()
    {
      using (var frm = new FormEditSettings())
      {
        UnitTestWinFormHelper.ShowFormAndClose(frm);
      }
    }
  }
}