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

#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnFilterLogicTests
  {
    [TestMethod]
    public void ColumnFilterLogicCtor()
    {
      var columnFilterLogic = new ColumnFilterLogic(typeof(double), "Column1");
      Assert.IsNotNull(columnFilterLogic);
      Assert.AreEqual(typeof(double), columnFilterLogic.ColumnDataType);
      Assert.AreEqual("Column1", columnFilterLogic.DataPropertyName);

      var columnFilterLogic2 = new ColumnFilterLogic(typeof(double), "[Column1]");
      Assert.AreEqual("Column1", columnFilterLogic2.DataPropertyName);
    }

    [TestMethod]
    public void ApplyFilterTest()
    {
      var columnFilterLogic = new ColumnFilterLogic(typeof(double), "Column1");

      var called = false;
      columnFilterLogic.ColumnFilterApply += delegate
      {
        called = true;
      };
      columnFilterLogic.ApplyFilter();
      Assert.IsTrue(called);
    }

    [TestMethod]
    public void BuildSQLCommand()
    {
      {
        var columnFilterLogic = new ColumnFilterLogic(typeof(double), "Column1");

        Assert.AreEqual("[Column1] = 2", columnFilterLogic.BuildSqlCommand("2"));
      }

      {
        var columnFilterLogic = new ColumnFilterLogic(typeof(double), "[Column1]");
        Assert.AreEqual("[Column1] = 2", columnFilterLogic.BuildSqlCommand("2"));
      }

      {
        var columnFilterLogic = new ColumnFilterLogic(typeof(string), "Column1");
        Assert.AreEqual("[Column1] = '2'", columnFilterLogic.BuildSqlCommand("2"));
      }
    }

    [TestMethod]
    public void NotifyPropertyChangedTest()
    {
      var columnFilterLogic = new ColumnFilterLogic(typeof(double), "Column1");
      string? prop = null;
      columnFilterLogic.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
      {
        prop = e.PropertyName;
      };
      columnFilterLogic.ValueText = "2";
      Assert.AreEqual("ValueText", prop);
      Assert.AreEqual("[Column1] = 2", columnFilterLogic.BuildSqlCommand(columnFilterLogic.ValueText));
    }

    [TestMethod, Timeout(1000)]
    public void Active()
    {
      var columnFilterLogic = new ColumnFilterLogic(typeof(DateTime), "Column1");
      var dtm = DateTime.Now;
      columnFilterLogic.SetFilter(dtm);
      columnFilterLogic.Active = true;
      Assert.IsTrue(columnFilterLogic.Active);
    }

    [TestMethod]
    public void SetFilter()
    {
      var control = new ColumnFilterLogic(typeof(double), "Column1");
      control.SetFilter(2);
      Assert.AreEqual("[Column1] = 2", control.BuildSqlCommand(control.ValueText));
    }

    [TestMethod]
    public void IsNullCompareTest() =>
      Assert.IsFalse(ColumnFilterLogic.IsNotNullCompare(ColumnFilterLogic.OperatorIsNull));

    [TestMethod]
    public void GetOperatorsTest()
    {
      var stringType = ColumnFilterLogic.GetOperators(typeof(string));
      Assert.AreEqual(9, stringType.Length);

      var dateTimeType = ColumnFilterLogic.GetOperators(typeof(DateTime));
      Assert.AreEqual(8, dateTimeType.Length);

      var boolType = ColumnFilterLogic.GetOperators(typeof(bool));
      Assert.AreEqual(4, boolType.Length);
    }

    [TestMethod]
    public void FilterExpressionDate()
    {
      var columnFilterLogic = new ColumnFilterLogic(typeof(DateTime), "Column1");
      columnFilterLogic.SetFilter(new DateTime(2020, 02, 20));
      var test = columnFilterLogic.FilterExpression.ToUpperInvariant().Replace(" ", "");
      Assert.IsTrue(test.Contains("[COLUMN1]>=#02/20/2020#"));
      Assert.IsTrue(test.Contains("[COLUMN1]<#02/21/2020#"));
    }

    private void TestFilterExpression(string expected, ColumnFilterLogic columnFilterLogic) => Assert.AreEqual(
      expected.ToUpperInvariant().Replace(" ", ""),
      columnFilterLogic.FilterExpression.ToUpperInvariant().Replace(" ", ""),
      $"Ignoring case and space, Expected: {expected} Actual: {columnFilterLogic.FilterExpression}");

    [TestMethod]
    public void FilterExpressionNumber()
    {
      var columnFilterLogic = new ColumnFilterLogic(typeof(float), "Column1");
      columnFilterLogic.SetFilter(5.0);
      TestFilterExpression("[Column1] = 5", columnFilterLogic);
    }

    [TestMethod]
    public void FilterExpressionBool()
    {
      var columnFilterLogic = new ColumnFilterLogic(typeof(bool), "Column1");
      columnFilterLogic.SetFilter(true);
      TestFilterExpression("[COLUMN1] = 1", columnFilterLogic);
      columnFilterLogic.SetFilter(false);
      TestFilterExpression("[COLUMN1] = 0", columnFilterLogic);
    }

    [TestMethod]
    public void FilterExpressionText()
    {
      var columnFilterLogic = new ColumnFilterLogic(typeof(string), "Column1");
      columnFilterLogic.SetFilter("Hello");
      TestFilterExpression("[COLUMN1]='Hello'", columnFilterLogic);

      columnFilterLogic.SetFilter("He\'llo");
      TestFilterExpression("[Column1]='He\'\'llo'", columnFilterLogic);

      columnFilterLogic.SetFilter(10);
      columnFilterLogic.Operator = "longer";
      TestFilterExpression("Len([Column1]) > 10", columnFilterLogic);
    }

    [TestMethod]
    public void Contains()
    {
      var columnFilterLogic = new ColumnFilterLogic(typeof(string), "strCol");
      columnFilterLogic.SetFilter("Hello");
      columnFilterLogic.Operator = ColumnFilterLogic.GetOperators(typeof(string)).First().ToString();
      TestFilterExpression("[strCol] like '%Hello%'", columnFilterLogic);
    }

    [TestMethod]
    public void AllFilterString()
    {
      var columnFilterLogic = new ColumnFilterLogic(typeof(string), "strCol") { ValueText = "Hello" };

      foreach (var op in ColumnFilterLogic.GetOperators(columnFilterLogic.ColumnDataType))
      {
        columnFilterLogic.Operator = op.ToString();
        columnFilterLogic.Active = true;
        Assert.IsNotNull(columnFilterLogic.FilterExpression);
      }
    }

    [TestMethod]
    public void AllFilterDateTime()
    {
      var dtm = DateTime.Now;
      var columnFilterLogic = new ColumnFilterLogic(typeof(DateTime), "dtmCol") { ValueDateTime = dtm };
      Assert.AreEqual(dtm, columnFilterLogic.ValueDateTime);

      foreach (var op in ColumnFilterLogic.GetOperators(columnFilterLogic.ColumnDataType))
      {
        columnFilterLogic.Operator = op.ToString();
        columnFilterLogic.Active = true;
        Assert.IsNotNull(columnFilterLogic.FilterExpression);
      }
    }

    [TestMethod]
    [Timeout(1000)]
    public void ValueClusterCollection()
    {
      var columnFilterLogic = new ColumnFilterLogic(typeof(long), "intCol");

      using var data = UnitTestStatic.GetDataTable(200);
      using var dataView = new DataView(data, null, null, DataViewRowState.CurrentRows);
      columnFilterLogic.ValueClusterCollection.BuildValueClusters(dataView, typeof(long), 1);
      var i = 0;
      foreach (var cluster in columnFilterLogic.ValueClusterCollection.ValueClusters)
      {
        cluster.Active = true;
        if (i++ > 2) break;
      }

      columnFilterLogic.Active = true;
      Assert.IsNotNull(columnFilterLogic.FilterExpression);
    }

    [TestMethod]
    public void AllFilterInt()
    {
      var columnFilterLogic = new ColumnFilterLogic(typeof(long), "intCol") { ValueText = "-10" };
      Assert.AreEqual("-10", columnFilterLogic.ValueText);
      foreach (var op in ColumnFilterLogic.GetOperators(columnFilterLogic.ColumnDataType))
      {
        columnFilterLogic.Operator = op.ToString();
        columnFilterLogic.Active = true;
        Assert.IsNotNull(columnFilterLogic.FilterExpression);
      }
    }

    [TestMethod]
    public void AllFilterBool()
    {
      var columnFilterLogic = new ColumnFilterLogic(typeof(bool), "strCol");
      columnFilterLogic.SetFilter("true");
      foreach (var op in ColumnFilterLogic.GetOperators(columnFilterLogic.ColumnDataType))
      {
        columnFilterLogic.Operator = op.ToString();
        Assert.IsNotNull(columnFilterLogic.FilterExpression);
      }
    }
  }
}