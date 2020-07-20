using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class FilterDataTableTests
  {
    private Tuple<DataTable, int> GetDataTable(int count)
    {
      var dt = new DataTable("dt");

      var withoutErrors = new List<DataColumn>(new[]
      {
        new DataColumn("ColID", typeof(int)), new DataColumn("ColText1", typeof(string)),
        new DataColumn("ColText2", typeof(string)), new DataColumn("ColTextDT", typeof(DateTime))
      });
      dt.Columns.AddRange(withoutErrors.ToArray());

      var random = new Random(new Guid().GetHashCode());
      for (var i = 0; i < count; i++)
      {
        var row = dt.NewRow();
        row[0] = i + 1;
        row[1] = $"Test{i + 1}";
        row[2] = $"Text {i * 2} !";
        row[3] = new DateTime(random.Next(1900, 2030), random.Next(1, 12), 1).AddDays(random.Next(1, 31));
        dt.Rows.Add(row);
        var errType = random.Next(1, 6);

        if (errType == 1)
        {
          var asWarning = random.Next(1, 3) == 1;

          row.RowError = asWarning ? "Row Warning".AddWarningId() : "Row Error";
        }

        if (errType == 2)
        {
          var asWarning = random.Next(1, 3) == 1;
          var colNum = random.Next(0, dt.Columns.Count);
          if (!asWarning)
            if (withoutErrors.Contains(dt.Columns[colNum]))
              withoutErrors.Remove(dt.Columns[colNum]);
          row.SetColumnError(colNum, asWarning ? "Col Warning".AddWarningId() : "Col Error");
        }
      }

      return new Tuple<DataTable, int>(dt, withoutErrors.Count);
    }

    [TestMethod]
    public async Task FilterDataTableTest()
    {
      var dt = GetDataTable(2000);
      var test = new FilterDataTable(dt.Item1);
      await test.StartFilter(0, FilterType.ErrorsAndWarning, UnitTestInitializeCsv.Token);
      Assert.IsTrue(test.FilterTable.Rows.Count > 0);

      Assert.AreEqual(4, test.ColumnsWithErrors.Count);
    }

    [TestMethod]
    public void CancelTest()
    {
      var dt = GetDataTable(2000);
      var test = new FilterDataTable(dt.Item1);
      test.Cancel();
      // No effect but no error either
      _ = test.StartFilter(0, FilterType.ShowErrors, UnitTestInitializeCsv.Token);
      // Assert.IsTrue(test.Filtering);
      test.Cancel();
      // Assert.IsFalse(test.Filtering);
    }

    [TestMethod]
    public async Task ColumnsWithoutErrorsAsync()
    {
      var dt = GetDataTable(2000);
      var test = new FilterDataTable(dt.Item1);
      await test.StartFilter(0, FilterType.ErrorsAndWarning, UnitTestInitializeCsv.Token);
      // not a good test, but its known how many columns will have errors
      Assert.AreEqual(dt.Item2, test.ColumnsWithoutErrors.Count);
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