using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;


namespace CsvTools.Tests
{
  [TestClass()]
  public class TwoStepDataTableLoaderTests
  {

    [TestMethod()]
    public async Task StartAsyncTestAsyncNoWarning()
    {
      bool warningCalled = false;

      // ReSharper disable once UseAwaitUsing
      using var tsde = new SteppedDataTableLoader();
      var csv = new CsvFileDummy
      {
        FileName = UnitTestStatic.GetTestPath("BasicCSV.txt"),
        FieldDelimiterChar = ',',
        CommentLine = "#"
      };

      var proc = new Progress<ProgressInfo>();
      var myDataTable = await tsde.StartAsync(csv, TimeSpan.FromMilliseconds(20), proc,
        (o, a) => { warningCalled = true; }, UnitTestStatic.Token);
      Assert.IsFalse(warningCalled);
      Assert.AreEqual(7, myDataTable.Columns.Count());
    }

    [TestMethod()]
    public async Task StartAsyncTestAsyncWarning()
    {
      bool warningCalled = false;

      // ReSharper disable once UseAwaitUsing
      using var tsde = new SteppedDataTableLoader();
      var csv = new CsvFileDummy
      {
        FileName = UnitTestStatic.GetTestPath("TextQualifiers.txt"),
        WarnQuotesInQuotes = true
      };

      var proc = new Progress<ProgressInfo>();
      var myDataTable = await tsde.StartAsync(csv, TimeSpan.FromMilliseconds(20), proc,
        (o, a) => { warningCalled = true; }, UnitTestStatic.Token);
      Assert.IsTrue(warningCalled);
      Assert.AreEqual(7, myDataTable.Columns.Count());
    }
  }
}