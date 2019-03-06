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
      var dataview = new DataView(data, null, null, DataViewRowState.CurrentRows);

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

    [TestMethod()]
    public void GetActiveValueClusterTest()
    {
      var data = UnitTestStatic.GetDataTable(200);
      var dataview = new DataView(data, null, null, DataViewRowState.CurrentRows);

      var test0a = new ValueClusterCollection();
      Assert.AreEqual(BuildValueClustersResult.ListFilled, test0a.BuildValueClusters(dataview, typeof(string), 0, 200));
      Assert.AreEqual(0, test0a.GetActiveValueCluster().Count());
      foreach (var iten in test0a.ValueClusters)
        iten.Active = true;
      Assert.AreEqual(test0a.ValueClusters.Count(), test0a.GetActiveValueCluster().Count());
    }
  }
}