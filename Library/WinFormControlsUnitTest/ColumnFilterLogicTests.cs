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
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnFilterLogicTests
  {
    [TestMethod]
    public void ColumnFilterLogicTest()
    {
      var crtl = new ColumnFilterLogic(typeof(double), "Column1");
      Assert.IsNotNull(crtl);
    }

    [TestMethod]
    public void ApplyFilterTest()
    {
      var crtl = new ColumnFilterLogic(typeof(double), "Column1");

      var called = false;
      crtl.ColumnFilterApply += delegate
      { called = true; };
      crtl.ApplyFilter();
      Assert.IsTrue(called);
    }

    [TestMethod]
    public void BuildSQLCommandTest()
    {
      {
        var crtl = new ColumnFilterLogic(typeof(double), "Column1");

        Assert.AreEqual("[Column1] = 2", crtl.BuildSQLCommand("2"));
      }

      {
        var crtl = new ColumnFilterLogic(typeof(double), "[Column1]");
        Assert.AreEqual("[Column1] = 2", crtl.BuildSQLCommand("2"));
      }

      {
        var crtl = new ColumnFilterLogic(typeof(string), "Column1");
        Assert.AreEqual("[Column1] = '2'", crtl.BuildSQLCommand("2"));
      }
    }

    [TestMethod]
    public void NotifyPropertyChangedTest()
    {
      var crtl = new ColumnFilterLogic(typeof(double), "Column1");
      string prop = null;
      crtl.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
      { prop = e.PropertyName; };
      crtl.ValueText = "2";
      Assert.AreEqual("ValueText", prop);
      Assert.AreEqual("[Column1] = 2", crtl.BuildSQLCommand(crtl.ValueText));
    }

    [TestMethod]
    public void SetActiveTest()
    {
      var crtl = new ColumnFilterLogic(typeof(DateTime), "Column1");
      var dtm = DateTime.Now;
      crtl.SetFilter(dtm);
      crtl.Active = true;
      Assert.IsTrue(crtl.Active);
    }

    [TestMethod]
    public void SetFilterTest()
    {
      var crtl = new ColumnFilterLogic(typeof(double), "Column1");
      crtl.SetFilter(2);
      Assert.AreEqual("[Column1] = 2", crtl.BuildSQLCommand(crtl.ValueText));
    }
  }
}