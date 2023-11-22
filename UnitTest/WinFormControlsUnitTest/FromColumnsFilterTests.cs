using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Globalization;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass()]
  public class FromColumnsFilterTests
  {
    [TestMethod()]
    [Timeout(2000)]
    public void FromDataGridViewFilter_IntData()
    {
      using (var DataTable200 = UnitTestStaticData.GetDataTable(200))
      {
         DataGridView dgv = new DataGridView();        
        foreach(var col in DataTable200.Columns.OfType<DataColumn>().Select(x => new DataGridViewTextBoxColumn() {Name = x.ColumnName, DataPropertyName = x.ColumnName  }))
          dgv.Columns.Add(col);
               
        UnitTestStaticForms.ShowForm(() => new FromColumnsFilter(dgv.Columns, DataTable200.Rows.OfType<DataRow>(), Array.Empty<int>()), 0.5, null);
      }
    }

    [TestMethod()]
    [Timeout(2000)]
    public void FromDataGridViewFilter_TextData()
    {
      using (var DataTable200 = UnitTestStaticData.GetDataTable(200))
      {        
        DataGridView dgv = new DataGridView();        
        foreach(var col in DataTable200.Columns.OfType<DataColumn>().Select(x => new DataGridViewTextBoxColumn() {Name = x.ColumnName, DataPropertyName = x.ColumnName  }))
          dgv.Columns.Add(col);
        dgv.Columns[0].Visible = false;
        dgv.Columns[4].Visible = false;
        UnitTestStaticForms.ShowForm(() => new FromColumnsFilter(dgv.Columns, DataTable200.Rows.OfType<DataRow>(), new[] { 1 }), 0.5, null);
      }
    }    
  }
}