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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

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
    var col = UnitTestStaticData.Columns.First(x => x.Name == "string");
    var data = GetColumnData(col.ColumnOrdinal);

    (var countNull, var test1) = data.BuildValueClustersString(col.Name, 10, 200.0, UnitTestStatic.TesterProgress);

    Assert.IsTrue(test1.Count>4 && test1.Count<=10);
    foreach (var cluster in test1)
    {
      m_DataView.RowFilter = cluster.SQLCondition;
      Assert.AreEqual(m_DataView.Count, cluster.Count, cluster.SQLCondition, cluster.Display);
    }
    // The number of matches might be lower since special chars are not counted
    var found = countNull + test1.Select(x => x.Count).Sum();
    Assert.IsTrue(found <= m_Data.Rows.Count, "More found than possible");
    Assert.IsTrue(found > m_Data.Rows.Count*.9, "Less than 90% found");
  }

  [TestMethod]
  [Timeout(5000)]
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

  [TestMethod]
  [Timeout(2000)]
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

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name == "string").ColumnOrdinal)!,
        max2, true, false, 10.0, UnitTestStatic.TesterProgress),
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

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_StringEmpty()
  {
    var fl = GetFilterLogic(0);
    Assert.AreEqual(BuildValueClustersResult.NoValues, fl.ReBuildValueClusters(GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "AllEmpty").ColumnOrdinal)!,
      200, true, false, 10.0, UnitTestStatic.TesterProgress), "Column AllEmpty");

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "PartEmpty").ColumnOrdinal)!,
      200, true, false, 10.0, UnitTestStatic.TesterProgress), "Column PartEmpty");

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

    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(value.ToArray(),
      50, true, false, 5.0, UnitTestStatic.TesterProgress), "Column String");

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
    Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ReBuildValueClusters(value.ToArray(),
      50, true, false, 5.0, UnitTestStatic.TesterProgress), "Column String");

    Assert.IsNotNull(fl.ValueClusterCollection);
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_NoValues()
  {
    var test = new ColumnFilterLogic(typeof(decimal), "dummy");
    var empty = new List<object>();

    for (long i = 1; i < 20; i++)
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
      empty.Add(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

    Assert.AreEqual(BuildValueClustersResult.NoValues, test.ReBuildValueClusters(empty.ToArray(), 200, false, false, 5.0, UnitTestStatic.TesterProgress));
  }

  [TestMethod]
  [Timeout(5000)]
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


  [TestMethod]
  [Timeout(5000)]
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

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClustersLong()
  {
    var data = new List<object>();
    for (long i = -200; i < 200; i+=5)
      data.Add(i);

    (var countNull, var test1) = data.ToArray().BuildValueClustersLong("dummy", 100, false, 200.0, UnitTestStatic.TesterProgress);
    var number1 = test1.Select(x => x.Count).Sum();
    Assert.AreEqual(data.Count, countNull + number1, "Overall Number is correct");
    Assert.IsTrue(test1.First().Display.Contains($"{data.First():N0}"));
    // Assert.IsTrue(test1.Last().Display.Contains($"{data.Last():N0}"));

    (var countNull2, var test2) = data.ToArray().BuildValueClustersLong("dummy", 20, true, 200.0, UnitTestStatic.TesterProgress);
    var number2 = test2.Select(x => x.Count).Sum();
    Assert.AreEqual(number1, number2, "Number of entries does not match Combine / Normal");

    (var countNull3, var test3) = data.ToArray().BuildValueClustersLongEven("dummy", 20, 200.0, UnitTestStatic.TesterProgress);
    var number3 = test3.Select(x => x.Count).Sum();
    Assert.AreEqual(number1, number3, "Number of entries does not match Even / Normal");

    (var countNull4, var test4) = data.ToArray().BuildValueClustersLong("dummy", 20, false, 200.0, UnitTestStatic.TesterProgress);
    var number4 = test4.Select(x => x.Count).Sum();
    Assert.AreEqual(number1, number4, "Number of entries does not change on number of groups");
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_LongRangeStep3999()
  {
    var test = new ColumnFilterLogic(typeof(long), "dummy");

    var number = new List<object>();
    for (long i = -1999; i < 2000; i++)
      number.Add(i);
    test.ReBuildValueClusters(number.ToArray(), 50, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(number.Count, test.ValueClusterCollection.Select(x => x.Count).Sum(), "Clusters should cover all entries");
  }



  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_LongRangeStep1990()
  {
    var test = new ColumnFilterLogic(typeof(long), "dummy");

    var number2 = new List<object>();
    for (long i = 10; i < 2000; i++)
      number2.Add(i);
    test.ReBuildValueClusters(number2.ToArray(), 50, false, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(number2.Count, test.ValueClusterCollection.Select(x => x.Count).Sum());

  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_LongRangeStep450()
  {
    var test = new ColumnFilterLogic(typeof(long), "dummy");

    var number3 = new List<object>();
    for (long i = -500; i < -50; i++)
      number3.Add(i);
    test.ReBuildValueClusters(number3.ToArray(), 50, true, false, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(number3.Count, test.ValueClusterCollection.Select(x => x.Count).Sum());

  }

  [TestMethod]
  [Timeout(200000)]
  public void BuildValueClusters_DateTime()
  {
    var data = new object[200];
    for (int i = 0; i < data.Length-2; i++)
      data[i]= (i % 10 == 0) ? DBNull.Value : new DateTime(UnitTestStatic.Random.Next(2000, 2023), UnitTestStatic.Random.Next(1, 12), 1).AddDays(UnitTestStatic.Random.Next(1, 31));

    // AddOrUpdate some dates multiple times to ensure they are clustered together, these dates are outside the random range
    data[1] = "Nonsense";
    data[2] = new DateTime(2025, 11, 25);
    data[data.Length - 2] = new DateTime(2025, 11, 25);
    data[data.Length - 1] = new DateTime(2025, 11, 25);

    (var countNull, var test) = data.BuildValueClustersDate("dummy", 50, false, 200.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(data.Length, test.Select(x => x.Count).Sum() + countNull);
    Assert.AreEqual(3, test.Last().Count); // The last cluster must have 3 entries for 2025-11-25
  }

  [TestMethod]
  [Timeout(5000)]
  public void BuildValueClusters_DateTimeCombine()
  {
    var data = new List<object>();
    data.Add(new DateTime(2025, 11, 25));
    data.Add(new DateTime(2025, 11, 25));

    // make a well defined array
    for (int m = 7; m > 0; m--)
    {
      data.Add(DBNull.Value);
      for (int d = 1; d < 23; d++)
        data.Add(new DateTime(2000, m, d));
    }

    // AddOrUpdate some dates multiple times to ensure they are clustered together, these dates are outside the random range

    data.Add(new DateTime(2025, 11, 25));
    data.Add(new DateTime(2025, 11, 25));

    (var countNull, var test) = data.ToArray().BuildValueClustersDate("dummy", 150, true, 200.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(7, countNull);
    Assert.AreEqual(data.Count, test.Select(x => x.Count).Sum() + countNull);
    Assert.AreEqual(4, test.Last().Count); // The last cluster must have 4 entries for 2025-11-25
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_DateTimeEven()
  {
    var test = new ColumnFilterLogic(typeof(DateTime), "dummy");
    var data = GetColumnData(2);
    test.ReBuildValueClusters(data, 150, true, true, 9999999, UnitTestStatic.TesterProgress);
    Assert.AreEqual(data.Length, test.ValueClusterCollection.Select(x => x.Count).Sum(), "Count in parts must match number ofd records");
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_DateTimeCombineEven()
  {
    var test = new ColumnFilterLogic(typeof(DateTime), "dummy");

    var data = GetColumnData(2);

    test.ReBuildValueClusters(data, 150, false, true, 9999999, UnitTestStatic.TesterProgress);
    Assert.AreEqual(data.Length, test.ValueClusterCollection.Select(x => x.Count).Sum());
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_bool()
  {
    var test = new ColumnFilterLogic(typeof(bool), "dummy");
    Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "bool").ColumnOrdinal), 200, false, true, 5.0, UnitTestStatic.TesterProgress));
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_double()
  {
    var test = new ColumnFilterLogic(typeof(double), "dummy");
    var res = test.ReBuildValueClusters(GetColumnData(4), 200, false, true, 5.0, UnitTestStatic.TesterProgress);
    Assert.AreEqual(BuildValueClustersResult.ListFilled, res);
  }

  [TestMethod]
  [Timeout(1000)]
  public void BuildValueClusters_decimal()
  {
    var test = new ColumnFilterLogic(typeof(decimal), "dummy");
    Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "numeric").ColumnOrdinal), 200, false, false, 5.0, UnitTestStatic.TesterProgress));
  }

}