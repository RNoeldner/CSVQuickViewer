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
      bool warningCalled = false;
      bool refreshCalled = false;

      Task RefreshFunc(FilterTypeEnum filterType, CancellationToken cancellationToken) =>
        Task.Run(() => refreshCalled = true, cancellationToken);

      // ReSharper disable once UseAwaitUsing
      using var tsde = new SteppedDataTableLoader(dt => myDataTable = dt, RefreshFunc);
      var csv = new CsvFile(id: "Csv", fileName: UnitTestStatic.GetTestPath("BasicCSV.txt"))
      {
        FieldDelimiter = ",",
        CommentLine = "#"
      };

      var proc = new Progress<ProgressInfo>();
      await tsde.StartAsync(csv, true, true, TimeSpan.FromMilliseconds(20), FilterTypeEnum.All, proc,
        (_, _) => { warningCalled = true; },
        UnitTestStatic.Token);
      Assert.IsTrue(refreshCalled);
      Assert.IsFalse(warningCalled);

      Assert.AreEqual(8, myDataTable.Columns.Count());
    }
  }
}