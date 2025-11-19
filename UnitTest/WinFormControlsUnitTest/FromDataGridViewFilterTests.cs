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

namespace CsvTools.Tests
{
  [TestClass()]
  public class FromDataGridViewFilterTests
  {
    [TestMethod()]
    [Timeout(2000)]
    public void FromDataGridViewFilter_IntData()
    {
      var data = new List<object>();
      var random = new Random(new Guid().GetHashCode());
      for (var i = 0; i < 500; i++)
        data.Add(random.Next(0, 5000));

      UnitTestStaticForms.ShowForm(() => new FromRowsFilter(new ColumnFilterLogic(typeof(int), "ID"), data.ToArray(), 10), 0.5, null);
    }

    [TestMethod()]
    [Timeout(2000)]
    public void FromDataGridViewFilter_TextData()
    {
      var data = new List<object>();
      var random = new Random(new Guid().GetHashCode());
      for (var i = 0; i < 45; i++)
        data.Add(UnitTestStatic.GetRandomText(50));

      //using var dt = UnitTestStaticData.RandomDataTable(20);
      using var dt = UnitTestStaticData.GetDataTable(100, false, false,true, false, out var _, out var _);
      UnitTestStaticForms.ShowForm(() => new FromRowsFilter(new ColumnFilterLogic(typeof(string), "Text"), data.ToArray(), 20), .5, null);
    }

    [TestMethod()]
    [Timeout(2000)]
    public void FromDataGridViewFilter_DateTime()
    {
      var data = new List<object>();
      var random = new Random(new Guid().GetHashCode());
      for (var i = 0; i < 25; i++)
        data.Add(new DateTime(random.Next(1900, 2030), random.Next(1, 12), 1).AddDays(random.Next(1, 31)));

      UnitTestStaticForms.ShowForm(() => new FromRowsFilter(new ColumnFilterLogic(typeof(DateTime), "DateTime"), data.ToArray(), 40), .5, null);
    }
  }
}
