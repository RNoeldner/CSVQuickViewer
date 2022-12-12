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

using System;
using System.Data;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CsvTools.Tests
{
  [TestClass]
  public class FilteredDataGridViewTests : IDisposable
  {
    private readonly DataTable DataTable200 = UnitTestStatic.GetDataTable(200);
    private bool disposedValue;

    [TestMethod]
    public void ApplyFiltersFilteredDataGridViewTest()
    {
      using (var fdgv1 = new FilteredDataGridView())
      {
        fdgv1.ApplyFilters();
      }

      using (var fdgv2 = new FilteredDataGridView())
      {
        using (var comboBoxColumn = new DataGridViewComboBoxColumn())
        {
          comboBoxColumn.Items.AddRange(Color.Red, Color.Yellow, Color.Green);
          comboBoxColumn.ValueType = typeof(Color);
        }

        var boolColumn = new DataGridViewCheckBoxColumn();
        fdgv2.Columns.Add(boolColumn);

        fdgv2.ApplyFilters();
      }
    }

    [TestMethod()]
    public void ApplyFiltersTest()
    {
      using var ctrl = new FilteredDataGridView();
      UnitTestStatic.ShowControl(ctrl, 0.5d,
        (control) =>
        {
          control.DataSource = DataTable200;
          control.ApplyFilters();
        });
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    [TestMethod()]
    public void Filter_Test()
    {
      using var ctrl = new FilteredDataGridView();
      UnitTestStatic.ShowControl(ctrl, 0.5d,
        (control) =>
        {
          control.DataSource = DataTable200;
          control.SetFilterMenu(0);
          control.CurrentCell = control[1, 0];
          control.FilterCurrentCell();

          control.RemoveAllFilter();
        });
    }

    [TestMethod]
    [Timeout(6000)]
    public void FilteredDataGridView_RefreshUI()
    {
      using var ctrl = new FilteredDataGridView();
      UnitTestStatic.ShowControl(ctrl, 0.5d,
        (control) =>
        {
          control.DataSource = DataTable200;
          control.SetFilterMenu(0);
          control.RefreshUI();
        });
    }

    [TestMethod]
    [Timeout(5000)]
    public void FilteredDataGridViewShow()
    {
      using var ctrl = new FilteredDataGridView();
      UnitTestStatic.ShowControl(ctrl, closeAfterSeconds: 0.2);
    }

    [TestMethod]
    public void FilteredDataGridViewTest()
    {
      using var fdgv = new FilteredDataGridView();
      Assert.IsNotNull(fdgv);
    }

    [TestMethod]
    [Timeout(6000)]
    public void FilteredDataGridViewVarious_HighlightText()
    {
      using var ctrl = new FilteredDataGridView();
      UnitTestStatic.ShowControl(ctrl, 0.5d,
        (control) =>
        {
          control.DataSource = DataTable200;
          control.FrozenColumns = 1;
          control.HighlightText = "HH";
        });
    }

    [TestMethod]
    [Timeout(6000)]
    public void FilteredDataGridViewVarious_SetFilterMenu()
    {
      using var ctrl = new FilteredDataGridView();
      UnitTestStatic.ShowControl(ctrl, 0.5d,
        (control) =>
        {
          control.DataSource = DataTable200;

          // Refresh all columns filters
          for (int col = 0; col < DataTable200.Columns.Count; col++)
            control.SetFilterMenu(col);
        });
    }

    [TestMethod]
    [Timeout(3000)]
    public void FrozenColumns()
    {
      using var filteredDataGridView = new FilteredDataGridView();
      filteredDataGridView.DataSource = DataTable200;
      filteredDataGridView.FrozenColumns = 2;
      UnitTestStatic.WaitSomeTime(.2, UnitTestStatic.Token);
    }

    [TestMethod]
    [Timeout(1000)]
    public void HideEmptyColumnsTest()
    {
      using var filteredDataGridView = new FilteredDataGridView();
      var dt = UnitTestStatic.GetDataTable();
      filteredDataGridView.DataSource = dt;
      using var frm = new Form();
      frm.Controls.Add(filteredDataGridView);
      frm.Show();
      var numCol = filteredDataGridView.Columns.Cast<DataGridViewColumn>().Count(col => col.Visible);
      Assert.AreEqual(numCol, dt.Columns.Count);
      filteredDataGridView.HideEmptyColumns();

      numCol = filteredDataGridView.Columns.Cast<DataGridViewColumn>().Count(col => col.Visible);

      Assert.AreEqual(numCol + 1, dt.Columns.Count);
    }

    [TestMethod]
    [Timeout(3000)]
    public void HighlightText()
    {
      using var filteredDataGridView = new FilteredDataGridView();
      filteredDataGridView.DataSource = DataTable200;
      filteredDataGridView.HighlightText = "ag";
      UnitTestStatic.WaitSomeTime(.2, UnitTestStatic.Token);
      Assert.AreEqual("", filteredDataGridView.CurrentFilter);
    }

    [TestMethod()]
    public void ReStoreViewSettingTest()
    {
    }

    [TestMethod()]
    public void SetColumnFrozenTest()
    {
      using var ctrl = new FilteredDataGridView();
      UnitTestStatic.ShowControl(ctrl, 0.5d,
        (control) =>
        {
          control.DataSource = DataTable200;
          control.SetColumnFrozen(0, true);
        });
    }

    [TestMethod]
    [Timeout(1000)]
    public void SetRowHeightTest()
    {
      using var filteredDataGridView = new FilteredDataGridView();
      UnitTestStatic.ShowControl(filteredDataGridView, 0.2,
        (control) =>
        {
          control.DataSource = DataTable200;
          control.SetRowHeight();
        });
    }

    [TestMethod()]
    public void SetToolStripMenu()
    {
      using var ctrl = new FilteredDataGridView();
      UnitTestStatic.ShowControl(ctrl, 0.5d,
        (control) =>
        {
          control.DataSource = DataTable200;
          control.SetToolStripMenu(0, 0, MouseButtons.Right);
          control.SetToolStripMenu(1, -1, MouseButtons.Right);
        });
    }

    [TestMethod()]
    public void ShowHideColumns()
    {
      using var ctrl = new FilteredDataGridView();
      UnitTestStatic.ShowControl(ctrl, 0.5d,
        (control) =>
        {
          control.DataSource = DataTable200;
          control.HideAllButOne(0);
          control.ShowAllColumns();
          control.HideEmptyColumns();

          control.SetColumnVisibility(new Dictionary<string, bool>()
          {
            { DataTable200.Columns[0].ColumnName, true }, { DataTable200.Columns[1].ColumnName, false }
          });
        });
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // TODO: dispose managed state (managed objects)
          DataTable200.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        disposedValue = true;
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged
    // resources ~FilteredDataGridViewTests() { // Do not change this code. Put cleanup code in
    // 'Dispose(bool disposing)' method Dispose(disposing: false); }
  }
}