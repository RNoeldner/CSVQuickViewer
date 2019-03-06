using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass()]
  public class ToolStripDataGridViewColumnFilterTests
  {
    [TestMethod()]
    public void ToolStripDataGridViewColumnFilterTest()
    {
      var data = UnitTestStatic.GetDataTable(200);
      var dataview = new DataView(data, null, null, DataViewRowState.CurrentRows);
      var col = new DataGridViewTextBoxColumn()
      {
        ValueType = typeof(int),
        Name = "int",
        DataPropertyName = "int"
      };
      var test = new ToolStripDataGridViewColumnFilter(typeof(int), col);
      Assert.AreEqual(true, test.ColumnFilterLogic.Active);
    }
  }
}