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
