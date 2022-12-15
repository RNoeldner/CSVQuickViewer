/*
* Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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
using System.Data;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class ToolStripDataGridViewColumnFilterTests
  {
    [TestMethod]
    [Timeout(5000)]
    public void ToolStripDataGridViewColumnFilterTest()
    {
      using (var data = UnitTestStaticData.GetDataTable(200))
      using (new DataView(data, null, null, DataViewRowState.CurrentRows))
      {
        var col = new DataGridViewTextBoxColumn { ValueType = typeof(int), Name = "int", DataPropertyName = "int" };
        var test = new ToolStripDataGridViewColumnFilter(col);
        Assert.AreEqual(false, test.ColumnFilterLogic.Active);
        test.ColumnFilterLogic.Active = true;
        Assert.AreEqual(false, test.ColumnFilterLogic.Active);
        test.ColumnFilterLogic.Operator = "=";
        test.ColumnFilterLogic.ValueText = "2";
        test.ColumnFilterLogic.Active = true;
        Assert.AreEqual(true, test.ColumnFilterLogic.Active);

        Assert.IsNotNull(test.ValueClusterCollection);
      }
    }

    [TestMethod()]
    public void ToolStripDataGridViewColumnFilterTest1()
    {
      var test = new ToolStripDataGridViewColumnFilter(new DataGridViewTextBoxColumn()
      {
        ValueType = typeof(int), Name = "int", DataPropertyName = "int"
      });
      Assert.AreEqual(false, test.ColumnFilterLogic.Active);
    }
  }
}