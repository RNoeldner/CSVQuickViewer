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
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ValueClusterCollectionTests
  {
    private static readonly DataTable m_Data;
    private static readonly DataView m_DataView;

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
      var test = new ValueClusterCollection(-1);
      Assert.AreEqual(BuildValueClustersResult.ListFilled, test.BuildValueClusters(m_DataView, typeof(string), 0));
      Assert.AreEqual(BuildValueClustersResult.ListFilled, test.BuildValueClusters(m_DataView, typeof(string), 7));
      Assert.IsNotNull(test.ValueClusters);
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_StringTooManyValues()
    {
      var test = new ValueClusterCollection(20);
      Assert.AreEqual(BuildValueClustersResult.TooManyValues, test.BuildValueClusters(m_DataView, typeof(string), 0));
      Assert.AreEqual(0, test.ValueClusters.Count());
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_int()
    {
      var test = new ValueClusterCollection(200);
      Assert.AreEqual(BuildValueClustersResult.ListFilled, test.BuildValueClusters(m_DataView, typeof(int), 1));
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_NoValues()
    {
      var test = new ValueClusterCollection(200);

      using (var dataTable = new DataTable { TableName = "ArtificialTable", Locale = new CultureInfo("en-gb") })
      {
        dataTable.Columns.Add("ID", typeof(long));
        dataTable.Columns.Add("Text", typeof(string));
        dataTable.Columns.Add("STart", typeof(DateTime));
        for (long i = 1; i < 20; i++)
        {
          var row = dataTable.NewRow();
          dataTable.Rows.Add(row);
        }

        using (var dataView = new DataView(dataTable, null, null, DataViewRowState.CurrentRows))
        {
          Assert.AreEqual(BuildValueClustersResult.NoValues, test.BuildValueClusters(dataView, typeof(long), 0));
          Assert.AreEqual(BuildValueClustersResult.NoValues, test.BuildValueClusters(dataView, typeof(string), 1));
          Assert.AreEqual(BuildValueClustersResult.NoValues, test.BuildValueClusters(dataView, typeof(DateTime), 2));
        }
      }
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_LongRangeStep1()
    {
      var test = new ValueClusterCollection(200);

      using (var dataTable = new DataTable { TableName = "ArtificialTable", Locale = new CultureInfo("en-gb") })
      {
        dataTable.Columns.Add("ID", typeof(long));
        for (long i = -19; i < 20; i++)
        {
          var row = dataTable.NewRow();
          row[0] = i; // random.Next(-20, 20);
          dataTable.Rows.Add(row);
        }

        using (var dataView = new DataView(dataTable, null, null, DataViewRowState.CurrentRows))
        {
          test.BuildValueClusters(dataView, typeof(long), 0);
          Assert.AreEqual(39, test.ValueClusters.Count);
          TestSort(test.ValueClusters);
          Assert.AreEqual("-19", test.ValueClusters.First().Display);
          Assert.AreEqual("19", test.ValueClusters.Last().Display);
        }
      }
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_LongRangeStep10()
    {
      var test = new ValueClusterCollection(200);

      using (var dataTable = new DataTable { TableName = "ArtificialTable", Locale = new CultureInfo("en-gb") })
      {
        dataTable.Columns.Add("ID", typeof(long));
        for (long i = -199; i < 200; i++)
        {
          var row = dataTable.NewRow();
          row[0] = i; // random.Next(-20, 20);
          dataTable.Rows.Add(row);
        }

        using (var dataView = new DataView(dataTable, null, null, DataViewRowState.CurrentRows))
        {
          test.BuildValueClusters(dataView, typeof(long), 0);
          Assert.AreEqual(40, test.ValueClusters.Count);
          TestSort(test.ValueClusters);
          Assert.IsTrue(test.ValueClusters.First().Display.Contains("-200") && test.ValueClusters.First().Display.Contains("-190"),
            test.ValueClusters.First().Display);
          Assert.IsTrue(test.ValueClusters.Last().Display.Contains("190") && test.ValueClusters.Last().Display.Contains("200"),
            test.ValueClusters.Last().Display);
        }
      }
    }

    private static void TestSort(IEnumerable<ValueCluster> items)
    {
      string? oldSort = null;

      foreach (var cluster in items)
      {
        if (oldSort != null)
          Assert.IsTrue(string.Compare(cluster.Sort, oldSort, StringComparison.Ordinal) > 0,
            $"Text '{cluster.Sort}' is not later than '{oldSort}'");
        oldSort = cluster.Sort;
      }
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_LongRangeStep100()
    {
      var test = new ValueClusterCollection(200);

      using (var dataTable = new DataTable { TableName = "ArtificialTable", Locale = new CultureInfo("en-gb") })
      {
        dataTable.Columns.Add("ID", typeof(long));
        for (long i = -1999; i < 2000; i++)
        {
          var row = dataTable.NewRow();
          row[0] = i; // random.Next(-20, 20);
          dataTable.Rows.Add(row);
        }

        using (var dataView = new DataView(dataTable, null, null, DataViewRowState.CurrentRows))
        {
          test.BuildValueClusters(dataView, typeof(long), 0);
          Assert.AreEqual(40, test.ValueClusters.Count);
          TestSort(test.ValueClusters);
        }
      }
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_DateTime()
    {
      var test = new ValueClusterCollection(200);
      Assert.AreEqual(BuildValueClustersResult.ListFilled, test.BuildValueClusters(m_DataView, typeof(DateTime), 2));
    }

    [TestMethod]
    [Timeout(1000)]
    public void BuildValueClusters_bool()
    {
      var test = new ValueClusterCollection(200);
      Assert.AreEqual(BuildValueClustersResult.ListFilled, test.BuildValueClusters(m_DataView, typeof(bool), 3));
    }

    [TestMethod]
    [Timeout(5000)]
    public void BuildValueClusters_double()
    {
      var test = new ValueClusterCollection(200);
      var res = test.BuildValueClusters(m_DataView, typeof(double), 4);
      Assert.AreEqual(BuildValueClustersResult.ListFilled, res);
      TestSort(test.ValueClusters);
    }

    [TestMethod]
    [Timeout(5000)]
    public void BuildValueClusters_decimal()
    {
      var test = new ValueClusterCollection(200);
      Assert.AreEqual(BuildValueClustersResult.ListFilled, test.BuildValueClusters(m_DataView, typeof(decimal), 5));
    }

    [TestMethod]
    [Timeout(5000)]
    public void GetActiveValueClusterTest()
    {
      var test = new ValueClusterCollection(200);
      Assert.AreEqual(BuildValueClustersResult.ListFilled, test.BuildValueClusters(m_DataView, typeof(string), 0));
      Assert.AreEqual(0, test.GetActiveValueCluster().Count());
      foreach (var item in test.ValueClusters)
        item.Active = true;
      Assert.AreEqual(test.ValueClusters.Count(), test.GetActiveValueCluster().Count());
    }
  }
}