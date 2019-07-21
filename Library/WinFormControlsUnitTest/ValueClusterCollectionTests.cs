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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace CsvTools.Tests
{
  [TestClass()]
  public class ValueClusterCollectionTests
  {
    [TestMethod()]
    public void BuildValueClustersTest()
    {
      var data = UnitTestStatic.GetDataTable(200);
      using (var dataview = new DataView(data, null, null, DataViewRowState.CurrentRows))
      {
        var test0a = new ValueClusterCollection();
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test0a.BuildValueClusters(dataview, typeof(string), 0, 200));
        Assert.IsNotNull(test0a.ValueClusters);

        var test0b = new ValueClusterCollection();
        Assert.AreEqual(BuildValueClustersResult.TooManyValues, test0b.BuildValueClusters(dataview, typeof(string), 0, 1));
        Assert.AreEqual(0, test0b.ValueClusters.Count());

        var test1 = new ValueClusterCollection();
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test1.BuildValueClusters(dataview, typeof(int), 1, 200));

        var test2 = new ValueClusterCollection();
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test2.BuildValueClusters(dataview, typeof(DateTime), 2, 200));

        var test3 = new ValueClusterCollection();
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test3.BuildValueClusters(dataview, typeof(bool), 3, 2));

        var test4 = new ValueClusterCollection();
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test4.BuildValueClusters(dataview, typeof(double), 4, 200));

        var test5 = new ValueClusterCollection();
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test5.BuildValueClusters(dataview, typeof(decimal), 5, 200));

        var test7 = new ValueClusterCollection();
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test7.BuildValueClusters(dataview, typeof(string), 7, 200));
      }
    }

    [TestMethod()]
    public void GetActiveValueClusterTest()
    {
      var data = UnitTestStatic.GetDataTable(200);
      using (var dataview = new DataView(data, null, null, DataViewRowState.CurrentRows))
      {
        var test0a = new ValueClusterCollection();
        Assert.AreEqual(BuildValueClustersResult.ListFilled, test0a.BuildValueClusters(dataview, typeof(string), 0, 200));
        Assert.AreEqual(0, test0a.GetActiveValueCluster().Count());
        foreach (var iten in test0a.ValueClusters)
          iten.Active = true;
        Assert.AreEqual(test0a.ValueClusters.Count(), test0a.GetActiveValueCluster().Count());
      }
     
    }
  }
}