using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FormEditSettingsTests
  {
    [TestMethod]
    public void FormEditSettings()
    {
      UnitTestWinFormHelper.RunSTAThread(() =>
      {
        using (var frm = new FormEditSettings(new ViewSettings()))
        {
          UnitTestWinFormHelper.ShowFormAndClose(frm);
        }
      });
    }

    [TestMethod]
    public void FormEditSettingsTest1()
    {
      UnitTestWinFormHelper.RunSTAThread(() =>
      {
        using (var frm = new FormEditSettings())
        {
          UnitTestWinFormHelper.ShowFormAndClose(frm);
        }
      });
    }
  }
}