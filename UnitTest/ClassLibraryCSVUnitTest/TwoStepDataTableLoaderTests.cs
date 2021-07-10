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
        FileName = UnitTestInitializeCsv.GetTestPath("BasicCSV.txt"),
        FileFormat = { FieldDelimiter = ",", CommentLine = "#" }
      };

      using var proc = new CustomProcessDisplay(UnitTestInitializeCsv.Token);
      await tsde.StartAsync(csv, true, TimeSpan.FromMilliseconds(20), proc, (sender, args) => { warningCalled =true; });
      Assert.IsTrue(refreshCalled);
      Assert.IsFalse(warningCalled);
      Assert.IsTrue(beginCalled);
      Assert.IsTrue(finishedCalled);

      Assert.AreEqual(8, myDataTable.Columns.Count());
    }
	}
}