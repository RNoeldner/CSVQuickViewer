using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnSizeTests
  {
    [TestMethod]
    public void CloneTest()
    {
      var a = new ColumnSize
      {
        ColumnName = "ColumnName",
        ColumnOrdinal = 1,
        Size = 100
      };
      var b = a.Clone();
      Assert.AreNotSame(a, b);
      Assert.AreEqual(a.ColumnName, b.ColumnName);
      Assert.AreEqual(a.ColumnOrdinal, b.ColumnOrdinal);
      Assert.AreEqual(a.Size, b.Size);
    }

    [TestMethod]
    public void CopyToTest()
    {
      var a = new ColumnSize
      {
        ColumnName = "ColumnName",
        ColumnOrdinal = 1,
        Size = 100
      };

      var b = new ColumnSize
      {
        ColumnName = "ColumnName2",
        ColumnOrdinal = 2,
        Size = 110
      };

      a.CopyTo(b);
      Assert.AreEqual(a.ColumnName, b.ColumnName);
      Assert.AreEqual(a.ColumnOrdinal, b.ColumnOrdinal);
      Assert.AreEqual(a.Size, b.Size);
      Assert.AreEqual(a.Size, b.Size);
    }

    [TestMethod]
    public void EqualsTest()
    {
      var a = new ColumnSize
      {
        ColumnName = "ColumnName",
        ColumnOrdinal = 1,
        Size = 100
      };

      var b = new ColumnSize
      {
        ColumnName = "ColumnName2",
        ColumnOrdinal = 2,
        Size = 110
      };

      var c = new ColumnSize
      {
        ColumnName = "ColumnName",
        ColumnOrdinal = 1,
        Size = 100
      };
      Assert.IsFalse(a.Equals(null));
      Assert.IsFalse(a.Equals(b));
      Assert.IsFalse(b.Equals(c));
      Assert.IsTrue(a.Equals(c));
      Assert.IsTrue(c.Equals(a));
    }
  }
}