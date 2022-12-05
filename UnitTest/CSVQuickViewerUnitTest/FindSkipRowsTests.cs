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
      var csv = new CsvFile(UnitTestStatic.GetTestPath("AllFormatsPipe.txt"), string.Empty);
      using var frm = new FindSkipRows(csv);
      UnitTestStatic.ShowFormAndClose(frm);
    }
    
  }


}