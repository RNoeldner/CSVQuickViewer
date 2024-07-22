using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class TwoStepDataTableLoaderTests
  {
    [TestMethod()]
    public async Task StartAsyncTestAsync()
    {
      bool warningCalled = false;
      bool refreshCalled = false;


      // ReSharper disable once UseAwaitUsing
      using var tsde = new SteppedDataTableLoader();
      var csv = new CsvFileDummy
      {
        FileName = UnitTestStatic.GetTestPath("BasicCSV.txt"), FieldDelimiterChar = ',', CommentLine = "#"
      };

      var proc = new Progress<ProgressInfo>();
      var myDataTable = await tsde.StartAsync(csv, TimeSpan.FromMilliseconds(20), proc,
        (o, a) => { warningCalled = true; }, UnitTestStatic.Token);
      Assert.IsTrue(refreshCalled);
      Assert.IsFalse(warningCalled);

      Assert.AreEqual(7, myDataTable.Columns.Count());
    }
  }
}