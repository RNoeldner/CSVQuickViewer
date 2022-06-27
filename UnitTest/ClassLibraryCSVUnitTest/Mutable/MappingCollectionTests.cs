using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class MappingCollectionTests
  {
    [TestMethod()]
    public void CloneTest()
    {
      var mc = new MappingCollection();
      mc.AddIfNew(new Mapping("fc1", "tf1"));
      var mc2 = (MappingCollection) mc.Clone();
      Assert.AreEqual(1, mc2.Count());
      mc.AddIfNew(new Mapping("fc2", "tf2"));
      Assert.AreEqual(1, mc2.Count());
    }

    [TestMethod()]
    public void EqualsTest()
    {
      var mc1 = new MappingCollection();
      mc1.AddIfNew(new Mapping("fc1", "tf1"));
      mc1.AddIfNew(new Mapping("fc2", "tf2"));

      var mc2 = new MappingCollection();
      mc2.AddIfNew(new Mapping("fc1", "tf1"));
      mc2.AddIfNew(new Mapping("fc2", "tf2"));

      Assert.IsTrue(mc1.Equals(mc2));
    }

    [TestMethod()]
    public void AddIfNewTest()
    {
      var mc = new MappingCollection();
      mc.AddIfNew(new Mapping("fc1", "tf1"));
      Assert.AreEqual(1, mc.Count());
      mc.AddIfNew(new Mapping("fc1", "tf1"));
      Assert.AreEqual(1, mc.Count());
    }

    [TestMethod()]
    public void CopyToTest()
    {
      var mc = new MappingCollection();
      mc.AddIfNew(new Mapping("fc1", "tf1"));
      var mc2 = new MappingCollection();
      mc.CopyTo(mc2);
      Assert.AreEqual(1, mc2.Count());
    }

    [TestMethod()]
    public void GetByColumnTest()
    {
      var mapping1 = new Mapping("fc1", "tf1");
      var mapping2 = new Mapping("fc1", "tf2");
      var mc1 = new MappingCollection();
      mc1.AddIfNew(mapping1);
      mc1.AddIfNew(mapping2);
      Assert.AreEqual(mapping1, mc1.GetByColumn("fc1").First());
    }

    [TestMethod()]
    public void GetByFieldTest()
    {
      var mapping1 = new Mapping("fc1", "tf1");
      var mc1 = new MappingCollection();
      mc1.AddIfNew(mapping1);
      Assert.AreEqual(mapping1, mc1.GetByField("tf1"));
    }

    [TestMethod()]
    public void GetColumnNameTest()
    {
      var mc1 = new MappingCollection();
      mc1.AddIfNew(new Mapping("fc1", "tf1"));
      Assert.AreEqual("fc1", mc1.GetColumnName("tf1"));
    }

    [TestMethod()]
    public void RemoveColumnTest()
    {
      var mc1 = new MappingCollection();
      mc1.AddIfNew(new Mapping("fc1", "tf1"));
      mc1.AddIfNew(new Mapping("fc2", "tf2"));
      mc1.RemoveColumn("fc1");
    }
  }
}