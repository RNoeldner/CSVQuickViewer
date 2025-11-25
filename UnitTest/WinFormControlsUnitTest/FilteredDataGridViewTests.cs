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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CsvTools.Tests;

[TestClass]
public class FilteredDataGridViewTests
{

  [TestMethod, Timeout(1000)]
  public void ApplyFiltersFilteredDataGridViewTest()
  {
    Extensions.RunStaThread(() =>
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
    });
  }

  [TestMethod, Timeout(2000)]
  public void ApplyFiltersTest()
  {
    using (var DataTable200 = UnitTestStaticData.GetDataTable(200))
    {
      UnitTestStaticForms.ShowControl(() => new FilteredDataGridView(), 0.5d,
        control =>
        {
          control.DataSource = DataTable200;
          control.ApplyFilters();
        });
    }
  }

  [TestMethod(), Timeout(4000)]
  public void Filter_Test()
  {
    using var dataTable200 = UnitTestStaticData.GetDataTable(200);
    UnitTestStaticForms.ShowControl(() => new FilteredDataGridView(), 0.5d,
      control =>
      {
        control.DataSource = dataTable200;
        control.CurrentCell = control[1, 0];
        control.FilterCurrentCell();

        control.RemoveAllFilter();
      });
  }

  [TestMethod]
  [Timeout(2000)]
  public void FilteredDataGridView_RefreshUI()
  {
    using (var dataTable200 = UnitTestStaticData.GetDataTable(200))
      UnitTestStaticForms.ShowControl(() => new FilteredDataGridView(), 0.5d,
        control =>
        {
          control.DataSource = dataTable200;
          control.RefreshUI();
        });
  }

  [TestMethod]
  [Timeout(1000)]
  public void FilteredDataGridViewShow()
  {
    UnitTestStaticForms.ShowControl(() => new FilteredDataGridView());
  }

  [TestMethod]
  [Timeout(100)]
  public void FilteredDataGridViewTest()
  {
    using var fdgv = new FilteredDataGridView();
    Assert.IsNotNull(fdgv);
  }

  [TestMethod]
  [Timeout(3000)]
  public void FilteredDataGridViewVarious_HighlightText()
  {
    using var dataTable200 = UnitTestStaticData.GetDataTable(200);
    UnitTestStaticForms.ShowControl(() => new FilteredDataGridView(), 0.5d,
      control =>
      {
        control.DataSource = dataTable200;
        control.FrozenColumns = 1;
        control.HighlightText = "HH";
      });
  }

  [TestMethod]
  [Timeout(1000)]
  public void FrozenColumns()
  {
    using var filteredDataGridView = new FilteredDataGridView();
    using var dataTable200 = UnitTestStaticData.GetDataTable(200);
    filteredDataGridView.DataSource = dataTable200;
    filteredDataGridView.FrozenColumns = 2;
    UnitTestStaticForms.WaitSomeTime(.2, UnitTestStatic.Token);
  }

  [TestMethod]
  [Timeout(1000)]
  public void HideEmptyColumnsTest()
  {
    using var dt = UnitTestStaticData.GetDataTable();
    UnitTestStaticForms.ShowControl(() => new FilteredDataGridView { DataSource = dt }, 0.1,
      c =>
      {
        var numCol = c.Columns.Cast<DataGridViewColumn>().Count(col => col.Visible);
        Assert.AreEqual(numCol, dt.Columns.Count);
        c.HideEmptyColumns();

        numCol = c.Columns.Cast<DataGridViewColumn>().Count(col => col.Visible);
        Assert.AreEqual(numCol + 1, dt.Columns.Count);
      });
  }

  [TestMethod]
  [Timeout(1000)]
  public void HighlightText()
  {
    using var filteredDataGridView = new FilteredDataGridView();
    using (var dataTable200 = UnitTestStaticData.GetDataTable(200))
    {
      filteredDataGridView.DataSource = dataTable200;
      filteredDataGridView.HighlightText = "ag";
      UnitTestStaticForms.WaitSomeTime(.2, UnitTestStatic.Token);
      Assert.AreEqual("", filteredDataGridView.CurrentFilter);
    }
  }

  [TestMethod()]
  [Timeout(1000)]
  public void SetColumnFrozenTest()
  {
    using (var DataTable200 = UnitTestStaticData.GetDataTable(200))
      UnitTestStaticForms.ShowControl(() => new FilteredDataGridView(), 0.5d,
        control =>
        {
          control.DataSource = DataTable200;
          control.SetColumnFrozen(0, true);
        });
  }

  [TestMethod()]
  [Timeout(2000)]
  public void SetToolStripMenu()
  {
    using var dataTable200 = UnitTestStaticData.GetDataTable(200);
    UnitTestStaticForms.ShowControl(() => new FilteredDataGridView(), 0.5d,
      control =>
      {
        control.DataSource = dataTable200;
        control.SetToolStripMenu(0, 0, MouseButtons.Right);
        control.SetToolStripMenu(1, -1, MouseButtons.Right);
      });
  }

  [TestMethod()]
  [Timeout(1000)]
  public void ShowHideColumns()
  {

    using (var DataTable200 = UnitTestStaticData.GetDataTable(200))
      UnitTestStaticForms.ShowControl(() => new FilteredDataGridView(), 0.5d,
        control =>
        {
          control.DataSource = DataTable200;
          control.HideEmptyColumns();

          control.SetColumnVisibility(new Dictionary<string, bool>
          {
            { DataTable200.Columns[0].ColumnName, true }, { DataTable200.Columns[1].ColumnName, false }
          });
        });
  }
}