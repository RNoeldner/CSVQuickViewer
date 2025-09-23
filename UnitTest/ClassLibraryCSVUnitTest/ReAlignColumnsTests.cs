/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */
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
    public void RealignColumnTest_Combine()
    {
      var dt = UnitTestStaticData.GetDataTable(400, false);
      var test = new ReAlignColumns(dt.Columns.Count);
      Assert.IsNotNull(test);
      var values = new string[dt.Columns.Count];
      foreach (DataRow row in dt.Rows)
      {
        for (var counter = 0; counter < dt.Columns.Count; counter++)
          values[counter] = Convert.ToString(row.ItemArray[counter]) ?? string.Empty;
        test.AddRow(values);
      }

      for (var counter = 0; counter < dt.Columns.Count; counter++)
        values[counter] = string.Empty;

      var rowT = dt.Rows[5];
      var values2 = new string[dt.Columns.Count+1];
      values2[0] = Convert.ToString(rowT[0])!.Substring(0, 5);
      values2[1] = Convert.ToString(rowT[0])!.Substring(5);

      values2[2] = Convert.ToString(rowT[1]) ?? string.Empty;
      values2[3] = Convert.ToString(rowT[2]) ?? string.Empty;
      values2[4] = Convert.ToString(rowT[3]) ?? string.Empty;
      values2[5] = Convert.ToString(rowT[4]) ?? string.Empty;
      values2[6] = Convert.ToString(rowT[5]) ?? string.Empty;
      values2[7] = Convert.ToString(rowT[6]) ?? string.Empty;
      values2[8] = Convert.ToString(rowT[7]) ?? string.Empty;
      values2[9] = Convert.ToString(rowT[8]) ?? string.Empty;

      var raw = values[0] + values[1] + "|" + values[2] + "|" + values[3] + "|" + values[4] + "|" + values[5] + "|" +
                values[6] + "|" + values[7] + "|" + values[8] + "|" + values[9];
      // ReSharper disable once NotAccessedVariable
      string warning = string.Empty;
      var res = test.RealignColumn(values2, (_, s) => { warning = s; }, raw);
      Assert.AreEqual(dt.Columns.Count, res.Count);
      // the column should be moved
      Assert.AreEqual(Convert.ToString(rowT[1]), res[1]);
      Assert.AreEqual(Convert.ToString(rowT[0]), res[0]);
    }

    [TestMethod()]
    public void RealignColumnTest_RemoveEmpty()
    {
      var dt = UnitTestStaticData.GetDataTable(400, false);
      var test = new ReAlignColumns(dt.Columns.Count);
      Assert.IsNotNull(test);
      var values = new string[dt.Columns.Count];
      foreach (DataRow row in dt.Rows)
      {
        for (var counter = 0; counter < dt.Columns.Count; counter++)
          values[counter] = Convert.ToString(row.ItemArray[counter]) ?? string.Empty;
        test.AddRow(values);
      }

      for (var counter = 0; counter < dt.Columns.Count; counter++)
        values[counter] = string.Empty;

      var rowT = dt.Rows[5];
      var values2 = new string[dt.Columns.Count+1];
      values2[0] = Convert.ToString(rowT[0]) ?? string.Empty;
      values2[1] = string.Empty;
      values2[2] = Convert.ToString(rowT[1])?? string.Empty;
      values2[3] = Convert.ToString(rowT[2])?? string.Empty;
      values2[4] = Convert.ToString(rowT[3]) ?? string.Empty;
      values2[5] = Convert.ToString(rowT[4]) ?? string.Empty;
      values2[6] = Convert.ToString(rowT[5]) ?? string.Empty;
      values2[7] = Convert.ToString(rowT[6]) ?? string.Empty;
      values2[8] = Convert.ToString(rowT[7]) ?? string.Empty;
      values2[9] = Convert.ToString(rowT[8]) ?? string.Empty;

      var raw = values[0] + values[1] + "|" + values[2] + "|" + values[3] + "|" + values[4] + "|" + values[5] + "|" +
                values[6] + "|" + values[7] + "|" + values[8] + "|" + values[9];
      // ReSharper disable once NotAccessedVariable
      string warning = string.Empty;
      var res = test.RealignColumn(values2, (_, s) => { warning = s; }, raw);
      Assert.AreEqual(dt.Columns.Count, res.Count);

      // the column should be moved
      Assert.AreEqual(Convert.ToString(rowT[1]), res[1]);
    }
  }
}
