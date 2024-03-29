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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ValueClusterCollectionTests
  {
    private static readonly DataTable m_Data;
    private static readonly DataView m_DataView;
    private static ICollection<object> GetColumnData(int index) =>
      m_Data.Rows.OfType<DataRow>().Select(x => x[index]).ToArray();

    private static ColumnFilterLogic GetFilterLogic(int index) =>
      new ColumnFilterLogic(m_Data.Columns[index].DataType, m_Data.Columns[index].ColumnName);

    static ValueClusterCollectionTests()
    {
      m_Data = UnitTestStaticData.GetDataTable(200);
      m_DataView = new DataView(m_Data, null, null, DataViewRowState.CurrentRows);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
      m_DataView.Dispose();
      m_Data.Dispose();
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_StringListFilled()
    {
      var fl = GetFilterLogic(0);
      Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ValueClusterCollection.ReBuildValueClusters(DataTypeEnum.String,
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "string").ColumnOrdinal), "d1", false, 200), "Column String");

      Assert.AreEqual(BuildValueClustersResult.NoValues, fl.ValueClusterCollection.ReBuildValueClusters(DataTypeEnum.String,
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "AllEmpty").ColumnOrdinal), "d2", true, 200), "Column AllEmpty");

      Assert.AreEqual(BuildValueClustersResult.ListFilled, fl.ValueClusterCollection.ReBuildValueClusters(DataTypeEnum.String,
        GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "PartEmpty").ColumnOrdinal), "d2", true, 200), "Column PartEmpty");
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
        value, "d1", false, 50, true, false,
        new Progress<ProgressInfo>(info => { Logger.Debug($"{DateTime.Now} : {info.Text} {info.Value}"); }),
        UnitTestStatic.Token), "Column String");

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
        value, "d1", false, 50, true, false,
        new Progress<ProgressInfo>(info => { Logger.Debug($"{DateTime.Now} : {info.Text} {info.Value}"); }), UnitTestStatic.Token) , "Column String");

      Assert.IsNotNull(fl.ValueClusterCollection);
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_int()
    {
      var test = new ValueClusterCollection();
      Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(DataTypeEnum.Integer, GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "int").ColumnOrdinal), "dummy", true, 200));
      Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(DataTypeEnum.Integer, GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "int").ColumnOrdinal), "dummy", true, 200, true));
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_NoValues()
    {
      var test = new ValueClusterCollection();
      var empty = new List<object>();

      for (long i = 1; i < 20; i++)
        empty.Add(null);

      Assert.AreEqual(BuildValueClustersResult.NoValues, test.ReBuildValueClusters(DataTypeEnum.Double, empty, "dummy", true, 200));
      Assert.AreEqual(BuildValueClustersResult.NoValues, test.ReBuildValueClusters(DataTypeEnum.String, empty, "dummy", true, 200));
      Assert.AreEqual(BuildValueClustersResult.NoValues, test.ReBuildValueClusters(DataTypeEnum.DateTime, empty, "dummy", true, 200));
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_LongRangeStep1()
    {
      var test = new ValueClusterCollection();
      var number = new List<object>();
      for (long i = -3; i < 3; i++)
        number.Add(i);

      test.ReBuildValueClusters(DataTypeEnum.Numeric, number, "dummy", true, 40);
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

      test.ReBuildValueClusters(DataTypeEnum.Integer, number, "dummy", true, 100);
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
      test.ReBuildValueClusters(DataTypeEnum.Integer, number, "dummy", true, 50);
      Assert.AreEqual(number.Count, test.Select(x => x.Count).Sum(), "Clusters should cover all entries");

      var number2 = new List<object>();
      for (long i = 10; i < 2000; i++)
        number2.Add(i);
      test.ReBuildValueClusters(DataTypeEnum.Integer, number2, "dummy", true, 50);
      Assert.AreEqual(number2.Count, test.Select(x => x.Count).Sum());

      var number3 = new List<object>();
      for (long i = -500; i < -50; i++)
        number3.Add(i);
      test.ReBuildValueClusters(DataTypeEnum.Integer, number3, "dummy", true, 50);
      Assert.AreEqual(number3.Count, test.Select(x => x.Count).Sum());

    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_DateTime()
    {
      var test = new ValueClusterCollection();

      var data = GetColumnData(2);


      test.ReBuildValueClusters(DataTypeEnum.DateTime, data, "dummy", true, 150);
      Assert.AreEqual(data.Count, test.Select(x => x.Count).Sum());

      var res = test.ReBuildValueClusters(DataTypeEnum.DateTime, data, "dummy", true, 50);
      Assert.AreEqual(BuildValueClustersResult.ListFilled, res);
      Assert.AreEqual(data.Count, test.Select(x => x.Count).Sum());

    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_bool()
    {
      var test = new ValueClusterCollection();
      Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(DataTypeEnum.Boolean, GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "bool").ColumnOrdinal), "dummy", true, 200));
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_double()
    {
      var test = new ValueClusterCollection();
      var res = test.ReBuildValueClusters(DataTypeEnum.Double, GetColumnData(4), "dummy", true, 200);
      Assert.AreEqual(BuildValueClustersResult.ListFilled, res);
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_decimal()
    {
      var test = new ValueClusterCollection();
      Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(DataTypeEnum.Numeric, GetColumnData(UnitTestStaticData.Columns.First(x => x.Name== "numeric").ColumnOrdinal), "dummy", false, 200));
    }

    [TestMethod]
    [Timeout(1000)]
    public void GetActiveValueClusterTest()
    {
      var test = new ValueClusterCollection();
      Assert.AreEqual(BuildValueClustersResult.ListFilled, test.ReBuildValueClusters(DataTypeEnum.String, GetColumnData(0), "dummy", true, 200));
      Assert.AreEqual(0, test.GetActiveValueCluster().Count());
      foreach (var item in test)
        item.Active = true;
      Assert.AreEqual(test.Count(), test.GetActiveValueCluster().Count());
    }
  }
}