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

using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

      using (var dt = UnitTestStatic.GetDataTable(100))
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
    public void HideEmptyColumnsTest()
    {
      using (var filteredDataGridView = new FilteredDataGridView())
      {
        var dt = UnitTestStatic.GetDataTable(100);
        filteredDataGridView.DataSource = dt;
        using (var frm = new Form())
        {
          frm.Controls.Add(filteredDataGridView);
          frm.Show();
          var numCol = 0;
          foreach (DataGridViewColumn col in filteredDataGridView.Columns)
            if (col.Visible)
              numCol++;
          Assert.AreEqual(numCol, dt.Columns.Count);
          filteredDataGridView.HideEmptyColumns();

          numCol = 0;
          foreach (DataGridViewColumn col in filteredDataGridView.Columns)
            if (col.Visible)
              numCol++;

          Assert.AreEqual(numCol + 1, dt.Columns.Count);
        }
      }
    }

    [TestMethod]
    public void SetRowHeightTest()
    {
      using (var dt = UnitTestStatic.GetDataTable(100))
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
  }
}