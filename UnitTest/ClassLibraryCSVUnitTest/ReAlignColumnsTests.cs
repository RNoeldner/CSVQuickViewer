using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace CsvTools.Tests
{
  [TestClass()]
  public class ReAlignColumnsTests
  {
    [TestMethod()]
    public void ReAlignColumnsTest()
    {
      var test = new ReAlignColumns(5);
      Assert.IsNotNull(test);
    }

    [TestMethod()]
    public void RealignColumnTest()
    {
      var dt = UnitTestStatic.GetDataTable(400, false);
      var test = new ReAlignColumns(dt.Columns.Count);
      Assert.IsNotNull(test);
      var values = new string[dt.Columns.Count];
      foreach (DataRow row in dt.Rows)
      {
        for (var counter = 0; counter < dt.Columns.Count; counter++)
        {
          values[counter] = Convert.ToString(row.ItemArray[counter]);
        }

        test.AddRow(values);
      }

      for (var counter = 0; counter < dt.Columns.Count; counter++)
        values[counter] = string.Empty;
      
      var rowT = dt.Rows[5];
      
      values[0] = Convert.ToString(rowT[0]);
      values[1] = Convert.ToString(rowT[1]);

      values[2] = Convert.ToString(rowT[3]);
      values[3] = Convert.ToString(rowT[4]);
      values[4] = Convert.ToString(rowT[5]);
      values[5] = Convert.ToString(rowT[6]);
      values[6] = Convert.ToString(rowT[7]);
      values[7] = Convert.ToString(rowT[8]);
      
      var raw = values[0] + "|" + values[1] + "|" + Convert.ToString(rowT[2]) + "|" + values[2] + "|" + values[3] + "|" +
                values[4] + "|" + values[5] + "|" + values[6] + "|" + values[7] + "|" + values[8];
      string warning = string.Empty;
      var res = test.RealignColumn(values, (i, s) => { warning = s;}, raw);
      
      // the column should be moved
      Assert.AreEqual(Convert.ToString(rowT[7]), res[7]);
    }
  }
}