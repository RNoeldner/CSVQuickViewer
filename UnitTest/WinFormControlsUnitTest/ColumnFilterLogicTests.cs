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
#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;

namespace CsvTools.Tests;

[TestClass]
public class ColumnFilterLogicTests
{
  private const int NumRecords = 200;
  private static readonly DataTable m_Data;

  static ColumnFilterLogicTests()
  {
    m_Data = UnitTestStaticData.GetDataTable(NumRecords);
  }

  [ClassCleanup]
  public static void ClassCleanup()
  {
    m_Data.Dispose();
  }

  /// <summary>
  /// Tests the SQLCondition against the provided dataTable
  /// </summary>
  /// <param name="dataTable"></param>
  /// <param name="toTest"></param>
  /// <returns></returns>
  public static bool TestRowFilters(DataTable dataTable, IEnumerable<ValueCluster> toTest)
  {
    var hadIssues = false;
    using (var dataView = new DataView(dataTable, null, null, DataViewRowState.CurrentRows))
    {
      foreach (var cluster in toTest)
      {
        dataView.RowFilter = cluster.SQLCondition;
        if (dataView.Count != cluster.Count)
        {
          Logger.Warning($"RowFilter shows {dataView.Count:N0} records Cluster expected {cluster.Count:N0} for '{cluster.SQLCondition}', maybe condition is not well formed");
          hadIssues=true;
        }
      }
    }
    return hadIssues;
  }

  [TestMethod, Timeout(500)]
  public void Active()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(DateTime), "Column1");
    var dtm = DateTime.Now;
    columnFilterLogic.SetFilter(dtm);
    columnFilterLogic.Active = true;
    Assert.IsTrue(columnFilterLogic.Active);
  }

  [TestMethod, Timeout(100)]
  public void AllFilterBool()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(bool), "strCol");
    columnFilterLogic.SetFilter("true");
    foreach (var op in ColumnFilterLogic.GetOperators(columnFilterLogic.DataType))
    {
      columnFilterLogic.Operator = op.ToString();
      Assert.IsNotNull(columnFilterLogic.FilterExpression);
    }
  }

  [TestMethod, Timeout(100)]
  public void AllFilterDateTime()
  {
    var dtm = DateTime.Now;
    var columnFilterLogic = new ColumnFilterLogic(typeof(DateTime), "dtmCol") { ValueDateTime = dtm };
    Assert.AreEqual(dtm, columnFilterLogic.ValueDateTime);

    foreach (var op in ColumnFilterLogic.GetOperators(columnFilterLogic.DataType))
    {
      columnFilterLogic.Operator = op.ToString();
      columnFilterLogic.Active = true;
      Assert.IsNotNull(columnFilterLogic.FilterExpression);
    }
  }

  [TestMethod, Timeout(100)]
  public void AllFilterInt()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(long), "intCol") { ValueText = "-10" };
    Assert.AreEqual("-10", columnFilterLogic.ValueText);
    foreach (var op in ColumnFilterLogic.GetOperators(columnFilterLogic.DataType))
    {
      columnFilterLogic.Operator = op.ToString();
      columnFilterLogic.Active = true;
      Assert.IsNotNull(columnFilterLogic.FilterExpression);
    }
  }

  [TestMethod, Timeout(100)]
  public void AllFilterString()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(string), "strCol") { ValueText = "Hello" };

    foreach (var op in ColumnFilterLogic.GetOperators(columnFilterLogic.DataType))
    {
      columnFilterLogic.Operator = op.ToString();
      // columnFilterLogic.Active = true;
      Assert.IsNotNull(columnFilterLogic.FilterExpression);
    }
  }

  [TestMethod, Timeout(100)]
  public void ApplyFilterTest()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(double), "Column1");
    columnFilterLogic.ApplyFilter();
  }

  [TestMethod, Timeout(500)]
  public void BuildValueClusters_bool()
  {
    var column = UnitTestStaticData.Columns.First(x => x.Name== "bool");
    var test = new ColumnFilterLogic(typeof(bool), column.Name);
    var data = GetColumnData(column.ColumnOrdinal);
    Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(data, 10, false, true, 5.0, UnitTestStatic.TesterProgress));
    var res = TestRowFilters(m_Data, test.ValueClusterCollection);
    if (res)
      Assert.Inconclusive("RowFilter not correct");
  }

  [TestMethod, Timeout(500)]
  public void BuildValueClusters_DateTime_Even()
  {
    var column = UnitTestStaticData.Columns.First(x => x.Name== "DateTime");
    var data = GetColumnData(column.ColumnOrdinal);
    var test = new ColumnFilterLogic(typeof(DateTime), column.Name);

    var testEven = new ColumnFilterLogic(typeof(DateTime), column.Name);
    testEven.ReBuildValueClusters(data, 50, false, true, 9999999, UnitTestStatic.TesterProgress);
    var res = TestRowFilters(m_Data, test.ValueClusterCollection);
    if (res)
      Assert.Inconclusive("RowFilter not correct");
    Assert.AreEqual(data.Length, testEven.ValueClusterCollection.Select(x => x.Count).Sum(), "Count in parts must match number ofd records");
  }

  [TestMethod, Timeout(500)]
  public void BuildValueClusters_DateTime()
  {

    var column = UnitTestStaticData.Columns.First(x => x.Name== "DateTime");
    var data = GetColumnData(column.ColumnOrdinal);
    var test = new ColumnFilterLogic(typeof(DateTime), column.Name);

    test.ReBuildValueClusters(data, 150, false, false, 9999999, UnitTestStatic.TesterProgress);
    var res = TestRowFilters(m_Data, test.ValueClusterCollection);
    if (res)
      Assert.Inconclusive("RowFilter not correct");

    Assert.AreEqual(data.Length, test.ValueClusterCollection.Select(x => x.Count).Sum());
  }

  [TestMethod, Timeout(500)]
  public void BuildValueClusters_DateTime_Combine()
  {
    var column = UnitTestStaticData.Columns.First(x => x.Name== "DateTime");
    var data = GetColumnData(column.ColumnOrdinal);
    var test = new ColumnFilterLogic(typeof(DateTime), column.Name);

    var testCombine = new ColumnFilterLogic(typeof(DateTime), column.Name);
    testCombine.ReBuildValueClusters(data, 50, true, false, 9999999, UnitTestStatic.TesterProgress);
    Assert.AreEqual(data.Length, testCombine.ValueClusterCollection.Select(x => x.Count).Sum(), "Count in parts must match number ofd records");
    var res = TestRowFilters(m_Data, test.ValueClusterCollection);
    if (res)
      Assert.Inconclusive("RowFilter not correct");
  }

  [TestMethod, Timeout(500)]
  public void BuildValueClusters_decimal()
  {
    var column = UnitTestStaticData.Columns.First(x => x.Name== "numeric");
    var data = GetColumnData(column.ColumnOrdinal);
    var test = new ColumnFilterLogic(typeof(decimal), column.Name);
    Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(data, 200, false, false, 5.0, UnitTestStatic.TesterProgress));
    var res = TestRowFilters(m_Data, test.ValueClusterCollection);
    if (res)
      Assert.Inconclusive("RowFilter not correct");
  }

  [TestMethod, Timeout(500)]
  public void BuildValueClusters_double()
  {
    var column = UnitTestStaticData.Columns.First(x => x.Name== "double");
    var data = GetColumnData(column.ColumnOrdinal);
    var test = new ColumnFilterLogic(typeof(double), column.Name);
    
    var res = test.ReBuildValueClusters(data, 50, false, true, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(BuildValueClustersResult.ListFilled, res);
    var res2 = TestRowFilters(m_Data, test.ValueClusterCollection);
    if (res2)
      Assert.Inconclusive("RowFilter not correct");
  }

  [TestMethod, Timeout(100)]
  public void BuildValueClusters_DoubleRangeStep1()
  {
    var test = new ColumnFilterLogic(typeof(decimal), "dummy");

    var number = new List<object>();
    for (long j = 0; j < 10; j++)
    {
      for (long i = -3; i < 3; i++)
        number.Add(i);
      number.Add(DBNull.Value);
    }

    test.ReBuildValueClusters(number.ToArray(), 40, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(7, test.ValueClusterCollection.Count);
    Assert.AreEqual(number.Count, test.ValueClusterCollection.Select(x => x.Count).Sum());
    // We have numeric so they are double, and the display shows a range
    Assert.IsTrue(test.ValueClusterCollection[1].Display.Contains("-3"));
    Assert.IsTrue(test.ValueClusterCollection.Last().Display.Contains("2"));
  }


  [TestMethod, Timeout(5000)]
  public void BuildValueClusters_LargeListManyDuplicates()
  {
    var possible = new List<string>();
    for (int index = 0; index < 20; index++)
      possible.Add(UnitTestStatic.
        GetRandomText(10));

    var value = new List<object>();
    for (int index = 0; index < 500000; index++)
      value.Add(possible[index % (possible.Count-1)]);

    var fl = GetFilterLogic(0);
    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(value.ToArray(),
      50, true, false, 5.0, UnitTestStatic.TesterProgress), "Column String");

    Assert.IsNotNull(fl.ValueClusterCollection);
  }

  [TestMethod, Timeout(10000)]
  public void BuildValueClusters_LargeListMayUnique()
  {
    var value = new List<object>();
    for (int index = 0; index < 500000; index++)
      if (index % 10000 == 0)
        value.Add(DBNull.Value);
      else
        value.Add(UnitTestStatic.GetRandomText(25));


    var fl = GetFilterLogic(0);

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(value.ToArray(),
      50, true, false, 5.0, UnitTestStatic.TesterProgress), "Column String");

    Assert.IsNotNull(fl.ValueClusterCollection);
  }

  [TestMethod, Timeout(500)]
  public void BuildValueClusters_LongRangeStep1()
  {
    var test = new ColumnFilterLogic(typeof(long), "dummy");

    var number = new List<object>();
    for (long j = 0; j < 10; j++)
    {
      for (long i = -3; i < 3; i++)
        number.Add(i); // -3,-2, -1, 0, 1, 2
      number.Add(DBNull.Value);
    }

    test.ReBuildValueClusters(number.ToArray(), 40, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(7, test.ValueClusterCollection.Count);
    Assert.AreEqual(number.Count, test.ValueClusterCollection.Select(x => x.Count).Sum());
    // We have numeric so they are double, and the display shows a range
    Assert.AreEqual("-3", test.ValueClusterCollection[1].Display);
    Assert.AreEqual("2", test.ValueClusterCollection.Last().Display);
  }

  [TestMethod, Timeout(500)]
  public void BuildValueClusters_LongRangeStep1990()
  {
    var test = new ColumnFilterLogic(typeof(long), "dummy");

    var number2 = new List<object>();
    for (long i = 10; i < 2000; i++)
      number2.Add(i);
    test.ReBuildValueClusters(number2.ToArray(), 50, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(number2.Count, test.ValueClusterCollection.Select(x => x.Count).Sum());

  }

  [TestMethod, Timeout(1000)]
  public void BuildValueClusters_LongRangeStep3999()
  {
    var test = new ColumnFilterLogic(typeof(long), "dummy");

    var number = new List<object>();
    for (long i = -1999; i < 2000; i++)
      number.Add(i);
    test.ReBuildValueClusters(number.ToArray(), 50, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(number.Count, test.ValueClusterCollection.Select(x => x.Count).Sum(), "Clusters should cover all entries");
  }

  [TestMethod, Timeout(500)]
  public void BuildValueClusters_LongRangeStep450()
  {
    var test = new ColumnFilterLogic(typeof(long), "dummy");

    var number3 = new List<object>();
    for (long i = -500; i < -50; i++)
      number3.Add(i);
    test.ReBuildValueClusters(number3.ToArray(), 50, true, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(number3.Count, test.ValueClusterCollection.Select(x => x.Count).Sum());

  }

  [TestMethod, Timeout(500)]
  public void BuildValueClusters_NoValues()
  {
    var test = new ColumnFilterLogic(typeof(decimal), "dummy");
    var empty = new List<object>();

    for (long i = 1; i < 20; i++)
      empty.Add(null);

    Assert.AreEqual(BuildValueClustersResult.NoValues, test.ReBuildValueClusters(empty.ToArray(), 200, false, false, 5.0, UnitTestStatic.TesterProgress));
  }

  [TestMethod, Timeout(500)]
  public void BuildValueClusters_String()
  {
    var fl = GetFilterLogic(0);
    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name == "string").ColumnOrdinal)!,
        10, true, false, 10.0, UnitTestStatic.TesterProgress),
      "Column String");

    Assert.IsTrue(fl.ValueClusterCollection.Count>0 && fl.ValueClusterCollection.Count<=10);

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name == "string").ColumnOrdinal)!,
        20, true, false, 10.0, UnitTestStatic.TesterProgress),
      "Column String");

    Assert.IsTrue(fl.ValueClusterCollection.Count>0 && fl.ValueClusterCollection.Count<=20);

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name == "string").ColumnOrdinal)!,
        200, true, false, 10.0, UnitTestStatic.TesterProgress),
      "Column String");

  }

  [TestMethod, Timeout(500)]
  public void BuildValueClusters_StringEmpty()
  {
    var fl = GetFilterLogic(0);
    Assert.AreEqual(BuildValueClustersResult.NoValues, fl.ReBuildValueClusters(GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "AllEmpty").ColumnOrdinal)!,
      200, true, false, 10.0, UnitTestStatic.TesterProgress), "Column AllEmpty");

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "PartEmpty").ColumnOrdinal)!,
      200, true, false, 10.0, UnitTestStatic.TesterProgress), "Column PartEmpty");

    Assert.IsNotNull(fl.ValueClusterCollection);
  }

  [TestMethod, Timeout(500)]
  public void BuildValueClusters_StringUnique()
  {
    var data = new List<object>(40000);
    for (int i = 0; i < 20000; i++)
      data.Add($"Test{i % 15}");

    var fl = GetFilterLogic(0);
    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(
        data.ToArray(), 20, false, false, 5, UnitTestStatic.TesterProgress),
      "Column String");
    Assert.AreEqual(15, fl.ValueClusterCollection.Count);
    Assert.AreEqual(data.Count, fl.ValueClusterCollection.Select(x => x.Count).Sum(), "Clusters should cover all entries");

    for (int i = 0; i < 20000; i++)
      data.Add($"Test{i % 19}");

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(
        data.ToArray(), 20, false, false, 5.0, UnitTestStatic.TesterProgress),
      "Column String");
    Assert.AreEqual(19, fl.ValueClusterCollection.Count);
    Assert.AreEqual(data.Count, fl.ValueClusterCollection.Select(x => x.Count).Sum(), "Clusters should cover all entries");

  }

  [TestMethod, Timeout(100)]
  public void BuildValueClusters_StringUseAlreadyExisting()
  {
    var fl = GetFilterLogic(0);
    const int max1 = 100;
    const int max2 = 200;
    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name == "string").ColumnOrdinal)!,
        max1, true, false, 10.0, UnitTestStatic.TesterProgress),
      "Column String");

    Assert.IsTrue(fl.ValueClusterCollection.Count>4 && fl.ValueClusterCollection.Count<=max1,
      $"Expected {4}-{max1} is: {fl.ValueClusterCollection.Count}");
    var before = fl.ValueClusterCollection.Count;
    Assert.AreEqual(NumRecords, fl.ValueClusterCollection.Sum(x => x.Count), "The cluster should cover each record");

    bool hadIssues = TestRowFilters(m_Data, fl.ValueClusterCollection);

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name == "string").ColumnOrdinal)!,
        max2, true, false, 10.0, UnitTestStatic.TesterProgress),
      "Column String");

    Assert.IsTrue(fl.ValueClusterCollection.Count>=before && fl.ValueClusterCollection.Count<=max2,
      $"Expected {before}-{max2} is: {fl.ValueClusterCollection.Count}");
    if (hadIssues)
      Assert.Inconclusive("Issues with RowFilter Count, please see messages in TestContext Messages");
  }

  [TestMethod, Timeout(100)]
  public void ChangeFilterString()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(string), "strCol") { Operator = "…xxx…", ValueText = "Hello" };
    TestFilterExpression("[strCol] like '%Hello%'", columnFilterLogic);

    columnFilterLogic.ValueText="Test";
    TestFilterExpression("[strCol] like '%Test%'", columnFilterLogic);
  }

  [TestMethod, Timeout(100)]
  public void ColumnFilterLogicCtor()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(double), "Column1");
    Assert.IsNotNull(columnFilterLogic);
    Assert.AreEqual(DataTypeEnum.Numeric, columnFilterLogic.DataType);
    Assert.AreEqual("Column1", columnFilterLogic.DataPropertyName);

    var columnFilterLogic2 = new ColumnFilterLogic(typeof(double), "[Column1]");
    Assert.AreEqual("Column1", columnFilterLogic2.DataPropertyName);
  }

  [TestMethod, Timeout(100)]
  public void Contains()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(string), "strCol");
    columnFilterLogic.SetFilter("Hello");
    columnFilterLogic.Operator = ColumnFilterLogic.GetOperators(DataTypeEnum.String).First().ToString();
    TestFilterExpression("[strCol] like '%Hello%'", columnFilterLogic);
  }


  [TestMethod, Timeout(500)]
  public void FilterExpressionBool()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(bool), "Column1");
    columnFilterLogic.SetFilter(true);
    TestFilterExpression("[COLUMN1] = 1", columnFilterLogic);
    columnFilterLogic.SetFilter(false);
    TestFilterExpression("[COLUMN1] = 0", columnFilterLogic);
  }

  [TestMethod, Timeout(100)]
  public void FilterExpressionDate()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(DateTime), "Column1");
    columnFilterLogic.SetFilter(new DateTime(2020, 02, 20));
    var test = columnFilterLogic.FilterExpression.ToUpperInvariant().Replace(" ", "");
    Assert.IsTrue(test.Contains("[COLUMN1]>=#02/20/2020#"));
    Assert.IsTrue(test.Contains("[COLUMN1]<#02/21/2020#"));
  }

  [TestMethod, Timeout(100)]
  public void FilterExpressionNumber()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(float), "Column1");
    columnFilterLogic.SetFilter(5.0);
    TestFilterExpression("[Column1] = 5", columnFilterLogic);
  }

  [TestMethod, Timeout(100)]
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

  [TestMethod, Timeout(100)]
  public void GetActiveValueClusterTest()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(DateTime), "Column1");
    var items = new List<ValueCluster> {
        new ValueCluster("Test1", "[Column1] >= #2020-01-01# AND [Column1] < #2020-01-02#", 10),
        new ValueCluster("Test2", "[Column1] >= #2021-01-01# AND [Column1] < #2021-01-02#", 20),
        new ValueCluster("Test3", "[Column1] >= #2022-01-01# AND [Column1] < #2022-01-02#", 30),
        new ValueCluster("Test4", "[Column1] >= #2023-01-01# AND [Column1] < #2023-01-02#", 40),
    };
    columnFilterLogic.ValueClusterCollection.AddRange(items);
    Assert.AreEqual(4, columnFilterLogic.ValueClusterCollection.Count);
    Assert.AreEqual(0, columnFilterLogic.ActiveValueClusterCollection.Count);
    columnFilterLogic.SetActiveStatus(items[1], true);
    Assert.AreEqual(3, columnFilterLogic.ValueClusterCollection.Count);
    Assert.AreEqual(1, columnFilterLogic.ActiveValueClusterCollection.Count);

  }

  [TestMethod, Timeout(100)]
  public void GetOperatorsTest()
  {
    var stringType = ColumnFilterLogic.GetOperators(DataTypeEnum.String);
    Assert.AreEqual(11, stringType.Length);

    var dateTimeType = ColumnFilterLogic.GetOperators(DataTypeEnum.DateTime);
    Assert.AreEqual(8, dateTimeType.Length);

    var boolType = ColumnFilterLogic.GetOperators(DataTypeEnum.Boolean);
    Assert.AreEqual(4, boolType.Length);
  }

  [TestMethod, Timeout(100)]
  public void IsNullCompareTest() =>
    Assert.IsFalse(ColumnFilterLogic.IsNotNullCompare(ColumnFilterLogic.OperatorIsNull));

  [TestMethod, Timeout(100)]
  public void NotifyPropertyChangedTest()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(double), "Column1");
    string? prop = null;
    columnFilterLogic.PropertyChanged += delegate (object _, PropertyChangedEventArgs e)
    {
      prop = e.PropertyName;
    };
    columnFilterLogic.ValueText = "2";
    Assert.AreEqual("ValueText", prop);
    //  Assert.AreEqual("[Column1] = 2", columnFilterLogic.BuildSqlCommand(columnFilterLogic.ValueText));
  }

  [TestMethod, Timeout(100)]
  public void SetFilter()
  {
    var control = new ColumnFilterLogic(typeof(double), "Column1");
    control.SetFilter(2);
    //   Assert.AreEqual("[Column1] = 2", control.BuildSqlCommand(control.ValueText));
  }

  [TestMethod, Timeout(100)]
  public void ValueClusterCollection()
  {
    var columnFilterLogic = new ColumnFilterLogic(typeof(long), "intCol");

    using var data = UnitTestStaticData.GetDataTable(200);
    using var dataView = new DataView(data, null, null, DataViewRowState.CurrentRows);
    columnFilterLogic.ReBuildValueClusters(data.Rows.OfType<DataRow>().Select(x => x[1]).ToArray(), 20, false, false, 5.0, UnitTestStatic.TesterProgress);
    var i = 0;
    foreach (var cluster in columnFilterLogic.ValueClusterCollection.ToList())
    {
      columnFilterLogic.SetActiveStatus(cluster, true);
      if (i++ > 2) break;
    }

    columnFilterLogic.Active = true;
    Assert.IsNotNull(columnFilterLogic.FilterExpression);
  }

  private static object[] GetColumnData(int index) =>
    m_Data.Rows.OfType<DataRow>().Select(x => x[index]).ToArray();

  private static ColumnFilterLogic GetFilterLogic(int index) =>
                                                                      new ColumnFilterLogic(m_Data.Columns[index].DataType, m_Data.Columns[index].ColumnName);
  private void TestFilterExpression(string expected, ColumnFilterLogic columnFilterLogic) => Assert.AreEqual(
        expected.ToUpperInvariant().Replace(" ", ""),
    columnFilterLogic.FilterExpression.ToUpperInvariant().Replace(" ", ""),
    $"Ignoring case and space, Expected: {expected} Actual: {columnFilterLogic.FilterExpression}");
}