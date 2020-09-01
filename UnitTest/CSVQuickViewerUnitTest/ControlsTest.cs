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
      var ctrl = new FormCsvTextDisplay();

      using (var frm = new TestForm())
      {
        frm.AddOneControl(ctrl);
        frm.Show();
        ctrl.SetCsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"), '"', '\t', '\0', 65001);

        Extensions.ProcessUIElements(500);
        frm.SafeInvoke(() => frm.Close());
      }
    }
  }
}