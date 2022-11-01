using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnCollectionTest
  {
    [TestMethod]
    public void Add()
    {
      var test = new ColumnCollection();
      Assert.AreEqual(0, test.Count);
      var item1 = new ColumnMut("Test");
      test.Add(item1);
      Assert.AreEqual(1, test.Count);
      var item2 = new Column("Test", new ValueFormat(), 0);
      test.Add(item2);
      Assert.AreEqual(1, test.Count);
      test.Add(new ColumnMut("New"));

      var exception = false;
      try
      {
#pragma warning disable CS8625
        test.Add(null);
#pragma warning restore CS8625
      }
      catch (ArgumentException)
      {
        exception = true;
      }
      catch (Exception ex)
      {
        Assert.Fail("Wrong Exception Type: " + ex.GetType());
      }

      Assert.IsTrue(exception, "No Exception thrown");
    }

    [TestMethod]
    public void TestOrder()
    {
      var test = new ColumnCollection
      {
        new Column("ColA", new ValueFormat(), 1), new Column("ColB", new ValueFormat(), 2),
        new Column("ColC", new ValueFormat(), 3)
      };
      Assert.AreEqual(3, test.Count);

      int oldColOrd = 0;
      foreach (var col in test)
      {
        Assert.IsTrue(col.ColumnOrdinal > oldColOrd);
        oldColOrd = col.ColumnOrdinal;
      }
    }

    [TestMethod]
    public void Replace()
    {
      var test = new ColumnCollection
      {
        new Column("ColA", new ValueFormat(), 1), new Column("ColB", new ValueFormat(), 2),
        new Column("ColC", new ValueFormat(), 3)
      };
      var colBnew = new Column("ColB", new ValueFormat(DataTypeEnum.Boolean), 2);
      test.Replace(colBnew);
      var colB = test.GetByName("ColB");
      Assert.AreEqual(3, test.Count);
      Assert.IsTrue(colB != null && colB.Equals((IColumn) colBnew));
      int oldColOrd = 0;
      foreach (var col in test)
      {
        Assert.IsTrue(col.ColumnOrdinal > oldColOrd);
        oldColOrd = col.ColumnOrdinal;
      }
    }

 

    [TestMethod]
    public void CopyTo()
    {
      var test1 = new ColumnCollection
      {
        new ColumnMut("Test1"),
        new ColumnMut("Test2"),
        new ColumnMut("Test3")
      };
      var test2 = new ColumnCollection();
      test2.AddRange(test1);

      Assert.IsTrue(test2.Equals(test1));
    }
  }
}