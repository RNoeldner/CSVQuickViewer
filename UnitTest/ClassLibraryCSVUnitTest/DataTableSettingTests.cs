namespace CsvTools.Tests
{
  using Microsoft.VisualStudio.TestTools.UnitTesting;

  [TestClass()]
  public class DataTableSettingTests
  {
    [TestMethod()]
    public void CloneTest()
    {
      var dt = new DataTableSetting("MyName");
      var dt2 = dt.Clone();
      Assert.IsTrue(dt2 is DataTableSetting);
      Assert.AreEqual(((DataTableSetting)dt2).FileName, dt.FileName);
      Assert.AreEqual(dt2.ID, dt.ID);
      dt2.ID = "xyZ";
      Assert.AreNotEqual(dt2.ID, dt.ID);
    }

    [TestMethod()]
    public void CopyToTest()
    {
      var dt = new DataTableSetting("MyName");
      var dt2 = new DataTableSetting("MyName2");
      dt.CopyTo(dt2);
      Assert.IsTrue(dt.Equals(dt2));
    }

    [TestMethod()]
    public void DataTableSettingTest()
    {
      var dt = new DataTableSetting("MyName");
      Assert.AreEqual("MyName", dt.FileName);
      Assert.IsNotNull(dt.ID);
      Assert.IsNotNull(dt.InternalID);
    }

    [TestMethod()]
    public void EqualsTest()
    {
      var dt = new DataTableSetting("MyName");
      var dt2 = dt.Clone();
      Assert.IsTrue(dt.Equals(dt));
      Assert.IsTrue(dt.Equals(dt2));
      dt2.ID = "xyZ";
      Assert.IsFalse(dt.Equals(dt2));
    }
  }
}