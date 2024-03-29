﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class ColumnExtensionsTests
  {
    [TestMethod]
    public void Get()
    {
      var test = new ColumnCollection();
      var item1 = new Column("Test");
      test.Add(item1);
      Assert.AreEqual(1, test.Count);
      var item2 = new Column("Test2");
      test.Add(item2);
      Assert.IsTrue(item1.Equals(test.GetByName("Test")), "Test found");
      Assert.IsTrue(item1.Equals(test.GetByName("TEST")), "TEST found");
      Assert.IsTrue(item2.Equals(test.GetByName("tEst2")), "Test2 found");

      Assert.IsNull(test.GetByName(""));
      Assert.IsNull(test.GetByName(null));
      Assert.IsNull(test.GetByName("nonsense"));
    }
  }
}