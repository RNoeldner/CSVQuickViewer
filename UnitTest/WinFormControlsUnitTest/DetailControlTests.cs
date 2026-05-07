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
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows;

namespace CsvTools.Tests;

[TestClass]
public class DetailControlTests
{
  private DataTable? m_SharedDataTable;

  [TestInitialize]
  public void Setup()
  {
    // Create the large 5000-row table once per test run
    m_SharedDataTable = UnitTestStaticData.GetDataTable(200, true);
  }

  [TestCleanup]
  public void Cleanup()
  {
    m_SharedDataTable?.Dispose();
  }

  [TestMethod]
  [Timeout(2000)]
  public void DetailControl_FindNextAsync()
  {
    UnitTestStaticForms.ShowControlAsync(() => new DetailControl(), async ctrl =>
    {
      await ctrl.LoadDataTableAsync(m_SharedDataTable!, RowFilterTypeEnum.All, UnitTestStatic.Token);
      ctrl.SearchText = "20";
      await ctrl.FindNextAsync(true);
    });
  }

  [TestMethod]
  [Timeout(2000)]
  public async Task DetailControl_SetFilter()
  {
    UnitTestStaticForms.ShowControlAsync(() => new DetailControl(), async dc =>
    {
      await dc.LoadDataTableAsync(m_SharedDataTable!, RowFilterTypeEnum.All, UnitTestStatic.Token);
      dc.SetFilter(UnitTestStaticData.Columns[0].Name, ">", "Test2");
    });
  }

  [TestMethod]
  [Timeout(2000)]
  public async Task DetailControl_LoadDataTableAsync()
  {
    UnitTestStaticForms.ShowControlAsync(() => new DetailControl(),
        dc => dc.LoadDataTableAsync(m_SharedDataTable!, RowFilterTypeEnum.All, UnitTestStatic.Token));
  }

  [TestMethod]
  [Timeout(2000)]
  public async Task DetailControl_Sort()
  {
    UnitTestStaticForms.ShowControlAsync(() => new DetailControl(),
        async dc =>
        {
          await dc.LoadDataTableAsync(m_SharedDataTable!, RowFilterTypeEnum.All, UnitTestStatic.Token);
          dc.Sort(UnitTestStaticData.Columns[9].Name, ListSortDirection.Ascending);
        });
  }
}