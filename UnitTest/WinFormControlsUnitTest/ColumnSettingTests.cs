using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnSettingTests
  {
    [TestMethod]
    public void ShouldSerializeSortTest()
    {
      var co1 = new ColumnSetting("proName", true, 0, 10, 100);
      Assert.AreEqual(false, co1.ShouldSerializeSort());
      var co2 = new ColumnSetting("proName", true, 1, 10, 100);
      Assert.AreEqual(true, co2.ShouldSerializeSort());
    }

    [TestMethod]    
    public void ShouldSerializeOperatorTest()
    {
      var co1 = new ColumnSetting("proName", true, 0, 10, 100);
      Assert.AreEqual(false, co1.ShouldSerializeOperator());
      co1.Operator = "=";
      co1.ValueText = "hello";
      Assert.AreEqual(true, co1.ShouldSerializeOperator());
    }

    [TestMethod]    
    public void ShouldSerializeValueTextTest()
    {
      var co1 = new ColumnSetting("proName", true, 0, 10, 100);
      Assert.AreEqual(false, co1.ShouldSerializeValueText());
      co1.ValueText = "hello";
      Assert.AreEqual(true, co1.ShouldSerializeValueText());
    }

    [TestMethod]
    public void ShouldSerializeValueDateTest()
    {
      var co1 = new ColumnSetting("proName", true, 0, 10, 100);
      Assert.AreEqual(false, co1.ShouldSerializeValueDate());
      co1.ValueDate = DateTime.Now;
      Assert.AreEqual(true, co1.ShouldSerializeValueDate());
    }

    [TestMethod]
    public void ShouldSerializeValueFiltersTest()
    {
      var co1 = new ColumnSetting("proName", true, 0, 10, 100);
      Assert.AreEqual(false, co1.ShouldSerializeValueFilters());
      co1.ValueFilters.Add(new ColumnSetting.ValueFilter("Cond", "display"));
      Assert.AreEqual(true, co1.ShouldSerializeValueFilters());
    }

    [TestMethod]
    public void GetHashCodeTest()
    {
      var co1 = new ColumnSetting("proName", true, 10, 10, 100);
      var co2 = new ColumnSetting("proName2", true, 10, 10, 100);
      Assert.AreNotEqual(co1.GetHashCode(), co2.GetHashCode());
    }
  }
}