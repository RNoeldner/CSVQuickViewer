using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Threading;
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
      bool beginCalled = false;
      bool finishedCalled = false;
      bool warningCalled = false;
      bool refreshCalled = false;

      Task RefreshFunc(FilterTypeEnum filterType, CancellationToken cancellationToken) =>
        Task.Run(() => refreshCalled = true, cancellationToken);

      using var tsde = new TwoStepDataTableLoader(dt => myDataTable = dt, () => myDataTable, RefreshFunc, null,
        () => beginCalled = true, (_) => finishedCalled = true);
      var csv = new CsvFile(UnitTestStatic.GetTestPath("BasicCSV.txt")) { FieldDelimiter = ",", CommentLine = "#" };

      var proc = new Progress<ProgressInfo>();
      await tsde.StartAsync(csv, true, true, TimeSpan.FromMilliseconds(20), proc, (_, _) => { warningCalled = true; },
        UnitTestStatic.Token);
      Assert.IsTrue(refreshCalled);
      Assert.IsFalse(warningCalled);
      Assert.IsTrue(beginCalled);
      Assert.IsTrue(finishedCalled);

      Assert.AreEqual(8, myDataTable.Columns.Count());
    }
  }
}