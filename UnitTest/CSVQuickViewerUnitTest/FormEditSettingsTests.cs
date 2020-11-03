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
      Extensions.RunSTAThread(() =>
      {
        using (var frm = new FormEditSettings(new ViewSettings()))
        {
          UnitTestWinFormHelper.ShowFormAndClose(frm);
        }
      });
    }

    [TestMethod]
    [Timeout(5000)]
    public void FormEditSettingsTest1()
    {
      Extensions.RunSTAThread(() =>
      {
        using (var frm = new FormEditSettings())
        {
          UnitTestWinFormHelper.ShowFormAndClose(frm);
        }
      });
    }
  }
}