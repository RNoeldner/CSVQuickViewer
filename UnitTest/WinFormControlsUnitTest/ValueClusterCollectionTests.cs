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
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ValueClusterCollectionTests
  {
    [TestMethod]
    public void BuildValueClustersTest()
    {
      using (var data = UnitTestStatic.GetDataTable(200))
      using (var dataView = new DataView(data, null, null, DataViewRowState.CurrentRows))
      {
        var test0a = new ValueClusterCollection(200);
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test0a.BuildValueClusters(dataView, typeof(string), 0));
        Assert.IsNotNull(test0a.ValueClusters);

        var test0b = new ValueClusterCollection(5);
        Assert.AreEqual(BuildValueClustersResult.TooManyValues, test0b.BuildValueClusters(dataView, typeof(string), 0));
        Assert.AreEqual(0, test0b.ValueClusters.Count());

        var test1 = new ValueClusterCollection(200);
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test1.BuildValueClusters(dataView, typeof(int), 1));

        var test2 = new ValueClusterCollection(200);
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test2.BuildValueClusters(dataView, typeof(DateTime), 2));

        var test3 = new ValueClusterCollection(2);
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test3.BuildValueClusters(dataView, typeof(bool), 3));

        var test4 = new ValueClusterCollection(200);
        var res4 = test4.BuildValueClusters(dataView, typeof(double), 4);
        Assert.AreEqual(BuildValueClustersResult.ListFilled, res4);
        string oldSort = null;
        foreach (var cluster in test4.ValueClusters)
        {
          if (oldSort != null)
            Assert.AreEqual(1, String.Compare(cluster.Sort, oldSort, StringComparison.Ordinal));
          oldSort = cluster.Sort;
        }


        var test5 = new ValueClusterCollection(200);
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test5.BuildValueClusters(dataView, typeof(decimal), 5));

        var test7 = new ValueClusterCollection(200);
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test7.BuildValueClusters(dataView, typeof(string), 7));
      }
    }

    [TestMethod]
    public void GetActiveValueClusterTest()
    {
      using (var data = UnitTestStatic.GetDataTable(200))
      using (var dataview = new DataView(data, null, null, DataViewRowState.CurrentRows))
      {
        var test0a = new ValueClusterCollection(200);
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test0a.BuildValueClusters(dataview, typeof(string), 0));
        Assert.AreEqual(0, test0a.GetActiveValueCluster().Count());
        foreach (var iten in test0a.ValueClusters)
          iten.Active = true;
        Assert.AreEqual(test0a.ValueClusters.Count(), test0a.GetActiveValueCluster().Count());
      }
    }
  }
}