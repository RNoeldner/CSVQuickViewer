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
      UnitTestStaticForms.ShowForm(() => new FindSkipRows(new CsvFileDummy() {FileName = UnitTestStatic.GetTestPath("AllFormatsPipe.txt"),}));
    }
  }
}