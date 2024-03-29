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
using System;
using System.ComponentModel;
using System.Data;

namespace CsvTools.Tests
{
  [TestClass]
  public class DetailControlTests
  {
    private readonly Random m_Random = new Random(Guid.NewGuid().GetHashCode());

    [TestMethod]
    [Timeout(2000)]
    public void SearchText()
    {
      using var dt = UnitTestStaticData.RandomDataTable(1000);
      UnitTestStaticForms.ShowControl(()=>new DetailControl(), .1, ctrl =>
      {
        ctrl.DataTable = dt;
        ctrl.RefreshDisplay(FilterTypeEnum.All, UnitTestStatic.Token);
        ctrl.SearchText("212");
      });
    }

    [TestMethod]
    [Timeout(1000)]
    public void FilterColumn()
    {
      using var dt = UnitTestStaticData.RandomDataTable(500);

      UnitTestStaticForms.ShowControl(()=>new DetailControl(), .1, ctrl =>
      {
        ctrl.DataTable = dt;
        ctrl.RefreshDisplay(FilterTypeEnum.All, UnitTestStatic.Token);
        ctrl.SetFilter(dt.Columns[2].ColumnName, ">", "Test2");
      });
    }

    [TestMethod]
    [Timeout(1000)]
    public void DetailControlTestAsync()
    {
      using var dt = new DataTable();
      dt.Columns.Add(new DataColumn { ColumnName = "ID", DataType = typeof(int) });
      dt.Columns.Add(new DataColumn { ColumnName = "Text", DataType = typeof(string) });
      dt.Columns.Add(new DataColumn { ColumnName = "Date", DataType = typeof(DateTime) });
      dt.Columns.Add(new DataColumn { ColumnName = "Bool", DataType = typeof(bool) });
      for (var line = 1; line < 5000; line++)
      {
        var row = dt.NewRow();
        row[0] = line;
        row[1] = $"This is text {line / 2}";
        row[2] = new DateTime(2001, 6, 6).AddHours(line * 3);
        row[3] = line % 3 == 0;
        if (m_Random.Next(1, 10) == 5)
          row.SetColumnError(m_Random.Next(0, 3), "Error");
        if (m_Random.Next(1, 50) == 5)
          row.RowError = "Row Error";
        dt.Rows.Add(row);
      }

      using var dc = new DetailControl();
      dc.HtmlStyle = HtmlStyle.Default;
      dc.Show();
      dc.DataTable = dt;

      dc.RefreshDisplay(FilterTypeEnum.All, UnitTestStatic.Token);
    }

    [TestMethod]
    [Timeout(1000)]
    public void SortTestAsync()
    {
      using var dt = new DataTable();
      dt.Columns.Add(new DataColumn { ColumnName = "ID", DataType = typeof(int) });
      dt.Columns.Add(new DataColumn { ColumnName = "Text", DataType = typeof(string) });
      dt.Columns.Add(new DataColumn { ColumnName = "Date", DataType = typeof(DateTime) });
      dt.Columns.Add(new DataColumn { ColumnName = "Bool", DataType = typeof(bool) });
      for (var line = 1; line < 5000; line++)
      {
        var row = dt.NewRow();
        row[0] = m_Random.Next(1, 5000);
        row[1] = $"This is text {line / 2}";
        row[2] = new DateTime(2001, 6, 6).AddHours(line * 3);
        row[3] = line % 3 == 0;
        dt.Rows.Add(row);
      }

      using var dc = new DetailControl();
      dc.HtmlStyle = HtmlStyle.Default;
      dc.Show();
      dc.DataTable = dt;
      dc.RefreshDisplay(FilterTypeEnum.All, UnitTestStatic.Token);
      dc.Sort("ID", ListSortDirection.Ascending);
    }
  }
}