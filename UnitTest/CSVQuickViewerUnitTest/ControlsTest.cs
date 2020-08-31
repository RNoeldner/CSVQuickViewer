using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        using (var test = new CustomProcessDisplay(UnitTestInitializeCsv.Token))
          ctrl.SetCsvFile(UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"), '"', '\t', '\0', 65001, test);

        Extensions.ProcessUIElements(500);
        frm.SafeInvoke(() => frm.Close());
      }
    }
  }
}