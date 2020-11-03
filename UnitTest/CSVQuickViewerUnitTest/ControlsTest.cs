using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class ControlsTests
  {
    [TestMethod]
    [Timeout(6000)]
    public void CsvTextDisplayShow()
    {
      using (var frm = new FormCsvTextDisplay())
      {
        UnitTestWinFormHelper.ShowFormAndClose(frm, .2,
          (f) => f.OpenFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"), false, '"', '\t', '\0', 1200, 1, "##"));
      }
    }
  }
}