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
      Task refeshFunc(FilterType FilterType, CancellationToken CancellationToken) => Task.Run(() => refreshCalled =true);

      using var tsde = new TwoStepDataTableLoader(dt => myDataTable = dt, () => myDataTable, refeshFunc, null, () => beginCalled=true, (x) => finishedCalled=true);
      var csv = new CsvFile
      {
        FileName = UnitTestStatic.GetTestPath("BasicCSV.txt"),
        FieldDelimiter = ",",
        CommentLine = "#"
      };

      var proc = new CustomProcessDisplay();
      await tsde.StartAsync(csv, true, TimeSpan.FromMilliseconds(20), proc, (sender, args) => { warningCalled =true; }, UnitTestStatic.Token);
      Assert.IsTrue(refreshCalled);
      Assert.IsFalse(warningCalled);
      Assert.IsTrue(beginCalled);
      Assert.IsTrue(finishedCalled);

      Assert.AreEqual(8, myDataTable.Columns.Count());
    }
  }
}