using System.Collections.Generic;
using CsvTools;
using Microsoft.VisualStudio.TestTools.UnitTesting; /*
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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class FilteredDataGridViewTests
  {
    [TestMethod]
    public void FilteredDataGridViewTest()
    {
      using (var fdgv = new FilteredDataGridView())
      {
        Assert.IsNotNull(fdgv);
      }
    }

    [TestMethod]
    public void ApplyFiltersFilteredDataGridViewTest()
    {
      using (var fdgv = new FilteredDataGridView())
      {
        fdgv.ApplyFilters();
      }

      using (var fdgv = new FilteredDataGridView())
      {
        using (var comboBoxColumn = new DataGridViewComboBoxColumn())
        {
          comboBoxColumn.Items.AddRange(Color.Red, Color.Yellow, Color.Green);
          comboBoxColumn.ValueType = typeof(Color);
        }

        var boolColumn = new DataGridViewCheckBoxColumn();
        fdgv.Columns.Add(boolColumn);

        fdgv.ApplyFilters();
      }

      using (var dt = UnitTestStatic.GetDataTable())
      using (var filteredDataGridView = new FilteredDataGridView())
      {
        filteredDataGridView.DataSource = dt;
        using (var frm = new Form())
        {
          frm.Controls.Add(filteredDataGridView);
          frm.Show();
          filteredDataGridView.ApplyFilters();
        }
      }
    }

    [TestMethod]
    [Timeout(1000)]
    public void HideEmptyColumnsTest()
    {
      using (var filteredDataGridView = new FilteredDataGridView())
      {
        var dt = UnitTestStatic.GetDataTable();
        filteredDataGridView.DataSource = dt;
        using (var frm = new Form())
        {
          frm.Controls.Add(filteredDataGridView);
          frm.Show();
          var numCol = filteredDataGridView.Columns.Cast<DataGridViewColumn>().Count(col => col.Visible);
          Assert.AreEqual(numCol, dt.Columns.Count);
          filteredDataGridView.HideEmptyColumns();

          numCol = filteredDataGridView.Columns.Cast<DataGridViewColumn>().Count(col => col.Visible);

          Assert.AreEqual(numCol + 1, dt.Columns.Count);
        }
      }
    }

    [TestMethod]
    [Timeout(1000)]
    public void SetRowHeightTest()
    {
      using (var dt = UnitTestStatic.GetDataTable())
      using (var filteredDataGridView = new FilteredDataGridView())
      {
        filteredDataGridView.DataSource = dt;
        using (var frm = new Form())
        {
          frm.Controls.Add(filteredDataGridView);
          frm.Show();
          filteredDataGridView.SetRowHeight();
        }
      }
    }


    [TestMethod]
    [Timeout(3000)]
    public void FrozenColumns()
    {
      using (var dt = UnitTestStatic.GetDataTable())
      using (var filteredDataGridView = new FilteredDataGridView())
      {
        filteredDataGridView.DataSource = dt;
        filteredDataGridView.FrozenColumns = 2;
        UnitTestWinFormHelper.WaitSomeTime(.2, UnitTestInitializeCsv.Token);
      }
    }

    [TestMethod]
    [Timeout(3000)]
    public void HighlightText()
    {
      using (var dt = UnitTestStatic.GetDataTable())
      using (var filteredDataGridView = new FilteredDataGridView())
      {
        filteredDataGridView.DataSource = dt;
        filteredDataGridView.HighlightText = "ag";
        UnitTestWinFormHelper.WaitSomeTime(.2, UnitTestInitializeCsv.Token);
        Assert.AreEqual("", filteredDataGridView.CurrentFilter);
      }
    }
    [TestMethod]
    [Timeout(6000)]
    public void FilteredDataGridViewVarious_SetFilterMenu()
    {

      using (var data = UnitTestStatic.GetDataTable(200))
      using (var ctrl = new FilteredDataGridView())
      {
        UnitTestWinFormHelper.ShowControl(new FilteredDataGridView(), 0.5d,
          (control, form) =>
          {
            if (!(control is FilteredDataGridView ctrl2))
              return;
            ctrl2.DataSource = data;

            // Refresh all columns filters
            for (int col = 0; col< data.Columns.Count; col++)
              ctrl2.SetFilterMenu(col);
          });
      }
    }

    [TestMethod]
    [Timeout(6000)]
    public void FilteredDataGridViewVarious_HighlightText()
    {

      using (var data = UnitTestStatic.GetDataTable(200))
      using (var ctrl = new FilteredDataGridView())
      {
        UnitTestWinFormHelper.ShowControl(new FilteredDataGridView(), 0.5d,
          (control, form) =>
          {
            if (!(control is FilteredDataGridView ctrl2))
              return;
            ctrl2.DataSource = data;

            ctrl2.FrozenColumns = 1;
            ctrl2.HighlightText = "HH";
          });
      }
    }

    [TestMethod]
    [Timeout(6000)]
    public void FilteredDataGridView_RefreshUI()
    {

      using (var data = UnitTestStatic.GetDataTable(200))
      using (var ctrl = new FilteredDataGridView())
      {
        UnitTestWinFormHelper.ShowControl(new FilteredDataGridView(), 0.5d,
          (control, form) =>
          {
            if (!(control is FilteredDataGridView ctrl2))
              return;
            ctrl2.DataSource = data;
            ctrl2.SetFilterMenu(0);
            ctrl2.RefreshUI();
          });
      }
    }

    [TestMethod]
    [Timeout(5000)]
    public void FilteredDataGridViewShow() => UnitTestWinFormHelper.ShowControl(new FilteredDataGridView());

    [TestMethod()]
    public void ApplyFiltersTest()
    {
      using (var data = UnitTestStatic.GetDataTable(200))
      using (var ctrl = new FilteredDataGridView())
      {
        UnitTestWinFormHelper.ShowControl(new FilteredDataGridView(), 0.5d,
          (control, form) =>
          {
            if (!(control is FilteredDataGridView ctrl2))
              return;
            ctrl2.DataSource = data;
            ctrl2.ApplyFilters();
          });
      }
    }

    [TestMethod()]
    public void Filter_Test()
    {
      using (var data = UnitTestStatic.GetDataTable(200))
      using (var ctrl = new FilteredDataGridView())
      {
        UnitTestWinFormHelper.ShowControl(new FilteredDataGridView(), 0.5d,
          (control, form) =>
          {
            if (!(control is FilteredDataGridView ctrl2))
              return;
            ctrl2.DataSource = data;
            ctrl2.SetFilterMenu(0);
            ctrl2.CurrentCell = ctrl2[1, 0];
            ctrl2.FilterCurrentCell();

            ctrl2.RemoveAllFilter();
          });
      }
    }

    [TestMethod()]
    public void SetColumnFrozenTest()
    {
      using (var data = UnitTestStatic.GetDataTable(200))
      using (var ctrl = new FilteredDataGridView())
      {
        UnitTestWinFormHelper.ShowControl(new FilteredDataGridView(), 0.5d,
          (control, form) =>
          {
            if (!(control is FilteredDataGridView ctrl2))
              return;
            ctrl2.DataSource = data;
            ctrl2.SetColumnFrozen(0, true);
          });
      }
    }

    [TestMethod()]
    public void ShowHideColumns()
    {
      using (var data = UnitTestStatic.GetDataTable(200))
      using (var ctrl = new FilteredDataGridView())
      {
        UnitTestWinFormHelper.ShowControl(new FilteredDataGridView(), 0.5d,
          (control, form) =>
          {
            if (!(control is FilteredDataGridView ctrl2))
              return;
            ctrl2.DataSource = data;
            ctrl2.HideAllButOne(0);
            ctrl2.ShowAllColumns();
            ctrl2.HideEmptyColumns();

            ctrl2.SetColumnVisibility(new Dictionary<string, bool>() {{ data.Columns[0].ColumnName, true}, { data.Columns[1].ColumnName, false} });
          });
      }
    }



    [TestMethod()]
    public void SetToolStripMenu()
    {
      using (var data = UnitTestStatic.GetDataTable(200))
      using (var ctrl = new FilteredDataGridView())
      {
        UnitTestWinFormHelper.ShowControl(new FilteredDataGridView(), 0.5d,
          (control, form) =>
          {
            if (!(control is FilteredDataGridView ctrl2))
              return;
            ctrl2.DataSource = data;

            ctrl2.SetToolStripMenu(0,0,true);

            ctrl2.SetToolStripMenu(1, -1, true);
          });
      }
    }
    
    [TestMethod()]
    public void ReStoreViewSettingTest()
    {

    }
  }
}