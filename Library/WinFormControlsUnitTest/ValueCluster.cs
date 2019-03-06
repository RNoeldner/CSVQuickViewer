using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class ValueClusterTests
  {
    [TestMethod]
    public void ValueClusterCtor()
    {
      var tst1 = new ValueCluster();
      Assert.AreEqual("", tst1.Display);
      Assert.AreEqual(0, tst1.Count);

      var tst2 = new ValueCluster("dis", 10);
      Assert.AreEqual("dis", tst2.Display);
      Assert.AreEqual(10, tst2.Count);
    }

    [TestMethod()]
    public void CloneTest()
    {
      var src = new ValueCluster("dis", 10);
      var tst = src.Clone();

      Assert.AreEqual(src.Display, tst.Display);
      Assert.AreEqual(src.Count, tst.Count);
    }

    [TestMethod()]
    public void CopyToTest()
    {
      var src = new ValueCluster("dis", 10);
      var dest = new ValueCluster("dis2", 20);
      src.CopyTo(dest);

      Assert.AreEqual(src.Display, dest.Display);
      Assert.AreEqual(src.Count, dest.Count);
    }

    [TestMethod()]
    public void EqualsTest()
    {
      var src = new ValueCluster("dis", 10);
      var dest = new ValueCluster("dis2", 20);
      Assert.IsFalse(src.Equals(dest));
      Assert.IsTrue(src.Equals(src));
      src.CopyTo(dest);
      Assert.IsTrue(src.Equals(dest));
    }

    [TestMethod()]
    public void ToStringTest()
    {
      var disp = new ValueCluster("dis2", 20).ToString();
      Assert.AreEqual("dis2 20 items", disp);
    }
  }
}