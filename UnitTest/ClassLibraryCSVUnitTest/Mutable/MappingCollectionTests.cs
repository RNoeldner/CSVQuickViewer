using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass()]
  public class MappingCollectionTests
  {
    [TestMethod()]
    public void EqualsTest()
    {
      var mc1 = new MappingCollection();
      mc1.Add(new Mapping("fc1", "tf1"));
      mc1.Add(new Mapping("fc2", "tf2"));

      var mc2 = new MappingCollection();
      mc2.Add(new Mapping("fc1", "tf1"));
      mc2.Add(new Mapping("fc2", "tf2"));

      Assert.IsTrue(mc1.Equals(mc2));
    }

    [TestMethod()]
    public void AddIfNewTest()
    {
      var mc = new MappingCollection();
      mc.Add(new Mapping("fc1", "tf1"));
      Assert.AreEqual(1, mc.Count());
      mc.Add(new Mapping("fc1", "tf1"));
      Assert.AreEqual(1, mc.Count());
    }

    [TestMethod()]
    public void CopyToTest()
    {
      var mc = new MappingCollection();
      mc.Add(new Mapping("fc1", "tf1"));
      var mc2 = new MappingCollection();
      mc2.AddRange(mc);
      Assert.AreEqual(1, mc2.Count());
    }

    [TestMethod()]
    public void GetByColumnTest()
    {
      var mapping1 = new Mapping("fc1", "tf1");
      var mapping2 = new Mapping("fc1", "tf2");
      var mc1 = new MappingCollection { mapping1, mapping2 };
      Assert.IsTrue(mapping1.Equals(mc1.GetByColumn("fc1").First()));
    }

    [TestMethod()]
    public void GetByFieldTest()
    {
      var mapping1 = new Mapping("fc1", "tf1");
      var mc1 = new MappingCollection();
      mc1.Add(mapping1);
      Assert.IsTrue(mapping1.Equals(mc1.GetByField("tf1")));
    }

    [TestMethod()]
    public void GetColumnNameTest()
    {
      var mc1 = new MappingCollection { new Mapping("fc1", "tf1") };
      Assert.AreEqual("fc1", mc1.GetColumnName("tf1"));
    }

    [TestMethod()]
    public void RemoveColumnTest()
    {
      var mc1 = new MappingCollection();
      mc1.Add(new Mapping("fc1", "tf1"));
      mc1.Add(new Mapping("fc2", "tf2"));
      mc1.RemoveColumn("fc1");
    }
  }
}