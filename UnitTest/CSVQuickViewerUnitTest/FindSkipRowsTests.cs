using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class FindSkipRowsTests
  {
    [TestMethod]
    [Timeout(1000)]
    public void FindSkipRows()
    {
      var csv = new CsvFile(id: string.Empty, fileName: UnitTestStatic.GetTestPath("AllFormatsPipe.txt"));
      UnitTestStaticForms.OpenFormSts(()=> new FindSkipRows(csv)); 
    }
  }
}