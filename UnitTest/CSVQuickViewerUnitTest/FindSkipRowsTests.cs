using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class FindSkipRowsTests
  {
    [TestMethod]
    [Timeout(5000)]
    public void FindSkipRows()
    {
      var csv = new CsvFile(id: string.Empty, fileName: UnitTestStatic.GetTestPath("AllFormatsPipe.txt"));
      using var frm = new FindSkipRows(csv);
      UnitTestStatic.ShowFormAndClose(frm);
    }
    
  }


}