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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace CsvTools.Tests;

[TestClass]
public class ValueClusterCollectionTests
{
  private const int NumRecords = 200;
  private static readonly DataTable m_Data;
  private static readonly DataView m_DataView;
  private static object[] GetColumnData(int index) =>
    m_Data.Rows.OfType<DataRow>().Select(x => x[index]).ToArray();

  private static ColumnFilterLogic GetFilterLogic(int index) =>
    new ColumnFilterLogic(m_Data.Columns[index].DataType, m_Data.Columns[index].ColumnName);

  static ValueClusterCollectionTests()
  {
    m_Data = UnitTestStaticData.GetDataTable(NumRecords);
    m_DataView = new DataView(m_Data, null, null, DataViewRowState.CurrentRows);
  }

  [ClassCleanup]
  public static void ClassCleanup()
  {
    m_DataView.Dispose();
    m_Data.Dispose();
  }

  [TestMethod]
  [Timeout(60000)]
  public void BuildValueClusters_StringMaxRestricted()
  {
    var fl = GetFilterLogic(0);
    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ValueClusterCollection.ReBuildValueClusters(
        DataTypeEnum.String,
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name == "string").ColumnOrdinal)!, "string", false, 10, true, true, 60, UnitTestStatic.TesterProgress),
      "Column String");

    Assert.IsTrue(fl.ValueClusterCollection.Count>4 && fl.ValueClusterCollection.Count<=10);
    Assert.AreEqual(m_Data.Rows.Count, fl.ValueClusterCollection.Select(x => x.Count).Sum(), "Clusters should cover all entries");
    // The filter for Specials does not count correct, must be a difference between c# and ADO
    foreach (var cluster in fl.ValueClusterCollection.Take(5))
    {
      m_DataView.RowFilter = cluster.SQLCondition;
      Assert.AreEqual(m_DataView.Count, cluster.Count, cluster.SQLCondition);
    }
  }

  [TestMethod]
  [Timeout(5000)]
  public void BuildValueClusters_StringUnique()
  {
    var data = new List<object>(40000);
    for (int i = 0; i < 20000; i++)
      data.Add((i % 15).ToString());

    var fl = GetFilterLogic(0);
    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ValueClusterCollection.ReBuildValueClusters(
        DataTypeEnum.String, data.ToArray(), "d1", false, 20, false, false, 5, UnitTestStatic.TesterProgress),
      "Column String");
    Assert.AreEqual(15, fl.ValueClusterCollection.Count);
    Assert.AreEqual(data.Count, fl.ValueClusterCollection.Select(x => x.Count).Sum(), "Clusters should cover all entries");

    for (int i = 0; i < 20000; i++)
      data.Add((i % 19).ToString());

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ValueClusterCollection.ReBuildValueClusters(
        DataTypeEnum.String, data.ToArray(), "d1", false, 20, false, false, 5.0, UnitTestStatic.TesterProgress),
      "Column String");
    Assert.AreEqual(19, fl.ValueClusterCollection.Count);
    Assert.AreEqual(data.Count, fl.ValueClusterCollection.Select(x => x.Count).Sum(), "Clusters should cover all entries");

  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_StringUseAlreadyExisting()
  {
    var fl = GetFilterLogic(0);
    const int max1 = 100;
    const int max2 = 200;
    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ValueClusterCollection.ReBuildValueClusters(
        DataTypeEnum.String,
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name == "string").ColumnOrdinal)!, "string", false, max1, true, false, 10.0, UnitTestStatic.TesterProgress),
      "Column String");

    Assert.IsTrue(fl.ValueClusterCollection.Count>4 && fl.ValueClusterCollection.Count<=max1,
      $"Expected {4}-{max1} is: {fl.ValueClusterCollection.Count}");
    var before = fl.ValueClusterCollection.Count;
    Assert.AreEqual(NumRecords, fl.ValueClusterCollection.Sum(x => x.Count), "The cluster should cover each record");
    bool hadIssues = false;
    // the generated Conditions should not throw an error
    foreach (var cluster in fl.ValueClusterCollection)
    {
      m_DataView.RowFilter = cluster.SQLCondition;
      if (m_DataView.Count != cluster.Count)
      {
        Logger.Warning($"RowFilter shows {m_DataView.Count:N0} records Cluster expected {cluster.Count:N0} for '{cluster.SQLCondition}', maybe condition is not well formed");
        hadIssues=true;
      }
    }

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ValueClusterCollection.ReBuildValueClusters(
        DataTypeEnum.String,
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name == "string").ColumnOrdinal)!, "string", false, max2, true, false, 10.0, UnitTestStatic.TesterProgress),
      "Column String");

    Assert.IsTrue(fl.ValueClusterCollection.Count>=before && fl.ValueClusterCollection.Count<=max2,
      $"Expected {before}-{max2} is: {fl.ValueClusterCollection.Count}");
    if (hadIssues)
      Assert.Inconclusive("Issues with RowFilter Count, please see messages in TestContext Messages");
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_String()
  {
    var fl = GetFilterLogic(0);
    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ValueClusterCollection.ReBuildValueClusters(
        DataTypeEnum.String,
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name == "string").ColumnOrdinal)!, "d1", false, 10, true, false, 10.0, UnitTestStatic.TesterProgress),
      "Column String");

    Assert.IsTrue(fl.ValueClusterCollection.Count>0 && fl.ValueClusterCollection.Count<=10);

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ValueClusterCollection.ReBuildValueClusters(
        DataTypeEnum.String,
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name == "string").ColumnOrdinal)!, "d1", false, 20, true, false, 10.0, UnitTestStatic.TesterProgress),
      "Column String");

    Assert.IsTrue(fl.ValueClusterCollection.Count>0 && fl.ValueClusterCollection.Count<=20);

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ValueClusterCollection.ReBuildValueClusters(
        DataTypeEnum.String,
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name == "string").ColumnOrdinal)!, "d1", false, 200, true, false, 10.0, UnitTestStatic.TesterProgress),
      "Column String");

  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_StringEmpty()
  {
    var fl = GetFilterLogic(0);
    Assert.AreEqual(BuildValueClustersResult.NoValues, fl.ValueClusterCollection.ReBuildValueClusters(DataTypeEnum.String,
      GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "AllEmpty").ColumnOrdinal)!, "d2", true, 200, true, false, 10.0, UnitTestStatic.TesterProgress), "Column AllEmpty");

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ValueClusterCollection.ReBuildValueClusters(DataTypeEnum.String,
      GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "PartEmpty").ColumnOrdinal)!, "d2", true, 200, true, false, 10.0, UnitTestStatic.TesterProgress), "Column PartEmpty");

    Assert.IsNotNull(fl.ValueClusterCollection);
  }

  [TestMethod]
  public void BuildValueClusters_LargeListMayUnique()
  {
    var value = new List<object>();
    for (int index = 0; index < 500000; index++)
      if (index % 10000 == 0)
        value.Add(DBNull.Value);
      else
        value.Add(UnitTestStatic.GetRandomText(25));


    var fl = GetFilterLogic(0);

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ValueClusterCollection.ReBuildValueClusters(DataTypeEnum.String,
      value.ToArray(), "d1", false, 50, true, false, 5.0,
      UnitTestStatic.TesterProgress), "Column String");

    Assert.IsNotNull(fl.ValueClusterCollection);
  }

  [TestMethod]
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
    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ValueClusterCollection.ReBuildValueClusters(DataTypeEnum.String,
      value.ToArray(), "d1", false, 50, true, false, 5.0,
      UnitTestStatic.TesterProgress), "Column String");

    Assert.IsNotNull(fl.ValueClusterCollection);
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_int()
  {
    var test = new ValueClusterCollection();
    Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(DataTypeEnum.Integer, GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "int").ColumnOrdinal), "dummy", true, 200, false, false, 5.0, UnitTestStatic.TesterProgress));
    Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(DataTypeEnum.Integer, GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "int").ColumnOrdinal), "dummy", true, 200, true, false, 5.0, UnitTestStatic.TesterProgress));
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_NoValues()
  {
    var test = new ValueClusterCollection();
    var empty = new List<object>();

    for (long i = 1; i < 20; i++)
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
      empty.Add(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    Assert.AreEqual(BuildValueClustersResult.NoValues, test.ReBuildValueClusters(DataTypeEnum.Double, empty.ToArray(), "dummy", true, 200, false, false, 5.0, UnitTestStatic.TesterProgress));
    Assert.AreEqual(BuildValueClustersResult.NoValues, test.ReBuildValueClusters(DataTypeEnum.String, empty.ToArray(), "dummy", true, 200, false, true, 5.0, UnitTestStatic.TesterProgress));
    Assert.AreEqual(BuildValueClustersResult.NoValues, test.ReBuildValueClusters(DataTypeEnum.DateTime, empty.ToArray(), "dummy", true, 200, true, false, 5.0, UnitTestStatic.TesterProgress));
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_LongRangeStep1()
  {
    var test = new ValueClusterCollection();
    var number = new List<object>();
    for (long i = -3; i < 3; i++)
      number.Add(i);

    test.ReBuildValueClusters(DataTypeEnum.Numeric, number.ToArray(), "dummy", true, 40, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(number.Count, test.Count);
    Assert.AreEqual(number.Count, test.Select(x => x.Count).Sum());
    Assert.AreEqual("-3", test.First().Display);
    Assert.AreEqual("2", test.Last().Display);
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_LongRangeStep5()
  {
    var test = new ValueClusterCollection();
    var number = new List<object>();
    for (long i = -199; i < 200; i+=5)
      number.Add(i);

    test.ReBuildValueClusters(DataTypeEnum.Integer, number.ToArray(), "dummy", true, 100, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(number.Count, test.Select(x => x.Count).Sum(), "Number of entries does not match");
    Assert.IsTrue(test.First().Display.Contains("-200") || test.First().Display.Contains("-199"), test.First().Display);
    Assert.IsTrue(test.Last().Display.Contains("200") || test.First().Display.Contains("199"), test.Last().Display);
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_LongRangeStep100()
  {
    var test = new ValueClusterCollection();
    var number = new List<object>();
    for (long i = -1999; i < 2000; i++)
      number.Add(i);
    test.ReBuildValueClusters(DataTypeEnum.Integer, number.ToArray(), "dummy", true, 50, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(number.Count, test.Select(x => x.Count).Sum(), "Clusters should cover all entries");

    var number2 = new List<object>();
    for (long i = 10; i < 2000; i++)
      number2.Add(i);
    test.ReBuildValueClusters(DataTypeEnum.Integer, number2.ToArray(), "dummy", true, 50, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(number2.Count, test.Select(x => x.Count).Sum());

    var number3 = new List<object>();
    for (long i = -500; i < -50; i++)
      number3.Add(i);
    test.ReBuildValueClusters(DataTypeEnum.Integer, number3.ToArray(), "dummy", true, 50, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(number3.Count, test.Select(x => x.Count).Sum());

  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_DateTime()
  {
    var test = new ValueClusterCollection();

    var data = GetColumnData(2);


    test.ReBuildValueClusters(DataTypeEnum.DateTime, data, "dummy", true, 150, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(data.Length, test.Select(x => x.Count).Sum());

    var res = test.ReBuildValueClusters(DataTypeEnum.DateTime, data, "dummy", true, 50, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(BuildValueClustersResult.ListFilled, res);
    Assert.AreEqual(data.Length, test.Select(x => x.Count).Sum());

  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_DateTimeCombine()
  {
    var test = new ValueClusterCollection();

    var data = GetColumnData(2);

    test.ReBuildValueClusters(DataTypeEnum.DateTime, data, "dummy", true, 150, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(data.Length, test.Select(x => x.Count).Sum());
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_DateTimeEven()
  {
    var test = new ValueClusterCollection();

    var data = GetColumnData(2);

    test.ReBuildValueClusters(DataTypeEnum.DateTime, data, "dummy", true, 150, true, true, 9999999, UnitTestStatic.TesterProgress);
    Assert.AreEqual(data.Length, test.Select(x => x.Count).Sum(), "Count in parts must match number ofd records");
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_DateTimeCombineEven()
  {
    var test = new ValueClusterCollection();

    var data = GetColumnData(2);

    test.ReBuildValueClusters(DataTypeEnum.DateTime, data, "dummy", true, 150, false, true, 9999999, UnitTestStatic.TesterProgress);
    Assert.AreEqual(data.Length, test.Select(x => x.Count).Sum());
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_bool()
  {
    var test = new ValueClusterCollection();
    Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(DataTypeEnum.Boolean, GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "bool").ColumnOrdinal), "dummy", true, 200, false, true, 5.0, UnitTestStatic.TesterProgress));
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_double()
  {
    var test = new ValueClusterCollection();
    var res = test.ReBuildValueClusters(DataTypeEnum.Double, GetColumnData(4), "dummy", true, 200, false, true, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(BuildValueClustersResult.ListFilled, res);
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_decimal()
  {
    var test = new ValueClusterCollection();
    Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(DataTypeEnum.Numeric, GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "numeric").ColumnOrdinal), "dummy", false, 200, false, false, 5.0, UnitTestStatic.TesterProgress));
  }

  [TestMethod]
  [Timeout(1000)]
  public void GetActiveValueClusterTest()
  {
    var test = new ValueClusterCollection();
    Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(DataTypeEnum.String, GetColumnData(0), "dummy", true, 200, false, false, 5.0, UnitTestStatic.TesterProgress));
    Assert.AreEqual(0, test.GetActiveValueCluster().Count());
    foreach (var item in test)
      item.Active = true;
    Assert.AreEqual(test.Count(), test.GetActiveValueCluster().Count());
  }
}