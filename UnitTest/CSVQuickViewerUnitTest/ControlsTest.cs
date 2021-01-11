using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ControlsTests
  {
    [TestMethod]
    [Timeout(6000)]
    public void CsvTextDisplayShow()
    {
      using (var frm = new FormCsvTextDisplay(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt")))
      {
        UnitTestWinFormHelper.ShowFormAndClose(frm, .2,
          (f) => f.OpenFile(false, "\"", "\t", "", 1200, 1, "##"));
      }
    }
  }
}