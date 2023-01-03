using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass]
  public class FilterDataTableTests
  {
    private (DataTable table, int errorRows, int warnigRows) GetDataTable(int count)
    {
      var dt = new DataTable("dt");

      dt.Columns.AddRange(new[]
      {
        new DataColumn("ColID", typeof(int)),
        new DataColumn("ColText1", typeof(string)),
        new DataColumn("ColText2", typeof(string)),
        new DataColumn("ColTextDT", typeof(DateTime))
      });
      var errors = 0;
      var warnings = 0;
      var random = new Random(new Guid().GetHashCode());
      for (var i = 0; i < count; i++)
      {
        var row = dt.NewRow();
        row[0] = i + 1;
        row[1] = $"Test{i + 1}";
        row[2] = $"Text {i * 2} !";
        row[3] = new DateTime(random.Next(1900, 2030), random.Next(1, 12), 1).AddDays(random.Next(1, 31));
        dt.Rows.Add(row);

        var errType = random.Next(1, 15);
        // 1/3 are warnings the rest errors
        var asWarning = random.Next(1, 3) == 1;
        if (errType <= 3)
          if (asWarning)
            warnings++;
          else
            errors++;


        // 1/15 would be column && row error
        // 2/15 should be row errors
        // 2/15 should be columns errors
        if (errType == 1 || errType == 2)
        {
          row.RowError = asWarning ? "Row Warning".AddWarningId() : "Row Error";
        }


        if (errType == 2 || errType == 3)
        {
          var colNum = random.Next(1, dt.Columns.Count);
          row.SetColumnError(colNum, asWarning ? "Col Warning".AddWarningId() : "Col Error");
        }
      }

      return (dt, errors, warnings);
    }

    [TestMethod]
    public void FilterWarningTest()
    {
      var (dt, _, warn) = GetDataTable(2000);
      var test = new FilterDataTable(dt);
      test.Filter(0, FilterTypeEnum.ShowWarning, UnitTestStatic.Token);
      Assert.AreEqual(warn, test.FilterTable!.Rows.Count);
    }

    [TestMethod]
    public void FilterErrorsTest()
    {
      var (dt, err, _) = GetDataTable(2000);
      var test = new FilterDataTable(dt);
      test.Filter(0, FilterTypeEnum.ShowErrors, UnitTestStatic.Token);
      Assert.AreEqual(err, test.FilterTable!.Rows.Count);
    }

    [TestMethod]
    public void FilterErrorsAndWarningTest()
    {
      var (dt, err, warn) = GetDataTable(2000);
      var test = new FilterDataTable(dt);
      var res = test.Filter(0, FilterTypeEnum.ErrorsAndWarning, UnitTestStatic.Token);
      Assert.AreEqual(err + warn, res.Rows.Count);
    }

    [TestMethod]
    public void FilterAllTest()
    {
      var (dt, _, _) = GetDataTable(2000);
      var test = new FilterDataTable(dt);
      var res = test.Filter(0, FilterTypeEnum.All, UnitTestStatic.Token);
      Assert.AreEqual(dt.Rows.Count, res.Rows.Count);
    }

    [TestMethod]
    public void FilterNoneTest()
    {
      var (dt, err, warn) = GetDataTable(2000);
      var test = new FilterDataTable(dt);
      var res = test.Filter(0, FilterTypeEnum.None, UnitTestStatic.Token);
      Assert.AreEqual(dt.Rows.Count - err - warn, res.Rows.Count);
    }

    [TestMethod, Timeout(10000)]
    public void CancelTest()
    {
      var (dt, _, _)  = GetDataTable(2000);
      using var test = new FilterDataTable(dt);
      test.Cancel();
      // No effect but no error either
      test.StartFilterAsync(0, FilterTypeEnum.ShowErrors, UnitTestStatic.Token);
      while (!test.Filtering)
      {
      }
      Assert.IsTrue(test.Filtering);
      test.Cancel();
      Assert.IsFalse(test.Filtering);
    }

    [TestMethod]
    public async Task UniqueFieldNameAsync()
    {
      var (dt, _, _) = GetDataTable(10);
      using var test = new FilterDataTable(dt);
      test.UniqueFieldName = new[] { "ColID" };
      await test.StartFilterAsync(0, FilterTypeEnum.ErrorsAndWarning, UnitTestStatic.Token);
      Assert.IsFalse(test.Filtering);
      // A unique column should be displayed so its part of the Error columns 
      Assert.IsTrue(test.GetColumns(FilterTypeEnum.ShowErrors).Any(x => x == "ColID"), "Result does not contain ColID");
    }

    [TestMethod]
    public void GetColumns()
    {
      var (dt, _, _)= GetDataTable(100);
      var test = new FilterDataTable(dt);
      test.Filter(0, FilterTypeEnum.ErrorsAndWarning, UnitTestStatic.Token);

      // not a good test, but its known how many columns will have errors
      Assert.AreEqual(4, test.GetColumns(FilterTypeEnum.All).Count);
      Assert.IsTrue(test.GetColumns(FilterTypeEnum.None).Any(x => x == "ColID"), "Result does not contain ColID");

      Assert.AreEqual(3, test.GetColumns(FilterTypeEnum.ErrorsAndWarning).Count, "ErrorsAndWarning");
      Assert.AreEqual(3, test.GetColumns(FilterTypeEnum.ShowWarning).Count, "Warning");
      Assert.AreEqual(3, test.GetColumns(FilterTypeEnum.ShowErrors).Count, "Errors");
    }


    [TestMethod]
    public void DisposeTest()
    {
      var (dt, _, _)= GetDataTable(2);
      var test = new FilterDataTable(dt);
      test.Dispose();
    }
  }
}
