using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class ControlsTests
  {
    [TestMethod]
    public async Task CsvTextDisplayShow()
    {
      using (var frm = new FormCsvTextDisplay())
      {
        await UnitTestWinFormHelper.ShowFormAndCloseAsync(frm,.2, frm.SetCsvFileAsync(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"), '"', '\t', '\0', 1200));
      }
    }
  }
}