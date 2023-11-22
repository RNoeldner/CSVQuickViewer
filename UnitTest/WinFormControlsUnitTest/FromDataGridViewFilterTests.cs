using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;

namespace CsvTools.Tests
{
  [TestClass()]
  public class FromDataGridViewFilterTests
  {
    [TestMethod()]
    [Timeout(2000)]
    public void FromDataGridViewFilter_IntData()
    {
      var data = new List<object>();
      var random = new Random(new Guid().GetHashCode());
      for (var i = 0; i < 500; i++)
        data.Add(random.Next(0, 5000));

      UnitTestStaticForms.ShowForm(() => new FromColumnFilter(new ColumnFilterLogic(typeof(int), "ID"), data), 0.5, null);
    }

    [TestMethod()]
    [Timeout(2000)]
    public void FromDataGridViewFilter_TextData()
    {
      var data = new List<object>();
      var random = new Random(new Guid().GetHashCode());
      for (var i = 0; i < 45; i++)
        data.Add(UnitTestStatic.GetRandomText(50));

      using var dt = UnitTestStaticData.RandomDataTable(20);
      UnitTestStaticForms.ShowForm(() => new FromColumnFilter(new ColumnFilterLogic(typeof(string), "Text"), data), .5, null);
    }

    [TestMethod()]
    [Timeout(2000)]
    public void FromDataGridViewFilter_DateTime()
    {
      var data = new List<object>();
      var random = new Random(new Guid().GetHashCode());
      for (var i = 0; i < 25; i++)
        data.Add(new DateTime(random.Next(1900, 2030), random.Next(1, 12), 1).AddDays(random.Next(1, 31)));

      UnitTestStaticForms.ShowForm(() => new FromColumnFilter(new ColumnFilterLogic(typeof(DateTime), "DateTime"), data), .5, null);
    }
  }
}