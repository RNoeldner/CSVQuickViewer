using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class TwoStepDataTableLoaderTests
  {
    [TestMethod()]
    public async Task StartAsyncTestAsync()
    {
      var myDataTable = new DataTable();
      bool warningCalled = false;
      bool refreshCalled = false;


      // ReSharper disable once UseAwaitUsing
      using var tsde = new SteppedDataTableLoader();
      var csv = new CsvFileDummy
      {
        FileName = UnitTestStatic.GetTestPath("BasicCSV.txt"),
        FieldDelimiterChar = ',',
        CommentLine = "#"
      };

      var proc = new Progress<ProgressInfo>();
      await tsde.StartAsync(csv, dt => myDataTable = dt, cancellationToken => { refreshCalled = true; }, TimeSpan.FromMilliseconds(20), proc,
        (o, a) => { warningCalled = true; }, UnitTestStatic.Token);
      Assert.IsTrue(refreshCalled);
      Assert.IsFalse(warningCalled);

      Assert.AreEqual(7, myDataTable.Columns.Count());
    }
  }
}