using System;
using System.Data;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FilterDataTableTests
  {
    private Tuple<DataTable, int, int, int> GetDataTable(int count)
    {
      var countRows = 0;
      var countErrors = 0;
      var countWarning = 0;
      var dt = new DataTable("dt");
      dt.Columns.AddRange(new[]
      {
        new DataColumn("ColID", typeof(int)),
        new DataColumn("ColText1", typeof(string)),
        new DataColumn("ColText2", typeof(string)),
        new DataColumn("ColTextDT", typeof(DateTime))
      });
      var random = new Random(new Guid().GetHashCode());
      for (var i = 0; i < count; i++)
      {
        var row = dt.NewRow();
        row[0] = i + 1;
        row[1] = $"Test{i + 1}";
        row[2] = $"Text {i * 2} !";
        row[3] = new DateTime(random.Next(1900, 2030), random.Next(1, 12), 1).AddDays(random.Next(1, 31));
        dt.Rows.Add(row);
        var errType = random.Next(1, 7);
        var isError = false;
        var isWarning = false;

        if (errType < 4) countRows++;

        if (errType == 1 || errType == 2)
        {
          var asWarning = random.Next(1, 3) == 1;
          if (asWarning)
            isWarning = true;
          else
            isError = true;
          row.RowError = asWarning ? "Row Warning".AddWarningId() : "Row Error";
        }

        if (errType == 2 || errType == 3)
        {
          var asWarning = random.Next(1, 3) == 1;
          if (asWarning)
            isWarning = true;
          else
            isError = true;
          row.SetColumnError(random.Next(0, 4), asWarning ? "Col Warning".AddWarningId() : "Col Error");
        }

        if (isError)
          countErrors++;
        if (isWarning)
          countWarning++;
      }

      return new Tuple<DataTable, int, int, int>(dt, countRows, countErrors, countWarning);
    }


    [TestMethod]
    public void FilterDataTableTest()
    {
      var dt = GetDataTable(2000);
      var test = new FilterDataTable(dt.Item1);
      test.StartFilter(0, FilterType.ErrorsAndWarning, CancellationToken.None).WaitToCompleteTask(360);
      Assert.IsTrue(test.FilterTable.Rows.Count > 0);

      Assert.AreEqual(4, test.ColumnsWithErrors.Count);
    }

    [TestMethod]
    public void CancelTest()
    {
      var dt = GetDataTable(2000);
      var test = new FilterDataTable(dt.Item1);
      test.StartFilter(0, FilterType.ShowErrors, CancellationToken.None);
      Assert.IsTrue(test.Filtering);
      test.Cancel();
      Assert.IsFalse(test.Filtering);
    }

    [TestMethod]
    public void ColumnsWithoutErrors()
    {
      var dt = GetDataTable(2000);
      var test = new FilterDataTable(dt.Item1);
      test.StartFilter(0, FilterType.ErrorsAndWarning, CancellationToken.None);
      Assert.AreEqual(0, test.ColumnsWithoutErrors.Count);
    }

    [TestMethod]
    public void DisposeTest()
    {
      var dt = GetDataTable(2);
      var test = new FilterDataTable(dt.Item1);
      test.Dispose();
    }
  }
}