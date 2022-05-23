using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class FindSkipRowsTests
  {
    [TestMethod]
    [Timeout(5000)]
    public void FindSkipRows()
    {
      var csv = new CsvFile(UnitTestStatic.GetTestPath("AllFormatsPipe.txt"));
      using var frm = new FindSkipRows(csv);
      UnitTestStatic.ShowFormAndClose(frm);
    }
    
  }


}