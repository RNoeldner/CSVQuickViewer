using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass()]
  public class ImmutableColumnTests
  {
    [TestMethod()]
    public void ImmutableColumnTest()
    {
      var ic = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 0,false, "",true);
      Assert.AreEqual("Name", ic.Name);
      Assert.AreEqual(DataTypeEnum.Integer, ic.ValueFormat.DataType);
    }

    [TestMethod()]
    public void ImmutableColumnTest2()
    {
      var ic = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 1);
      Assert.AreEqual("Name", ic.Name);
      Assert.AreEqual(DataTypeEnum.Integer, ic.ValueFormat.DataType);
    }

    [TestMethod()]
    public void EqualsTest()
    {
      var ic = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 1);
      var ic2 = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 2);
      Assert.IsTrue(ic.Equals(ic),"ic==ic");
      Assert.IsFalse(ic.Equals(ic2),"ic!=ic2");
      Assert.IsFalse(ic2.Equals(null), "ic2!=null");
    }


    [TestMethod()]
    public void GetHashCodeTest()
    {
      var ic = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 1);
      Assert.AreNotEqual(0, ic.GetHashCode());
      var ic2 = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 1);
      Assert.AreEqual(ic2.GetHashCode(), ic.GetHashCode());
    }

    [TestMethod()]
    public void ToStringTest()
    {
      var ic = new Column("Name", new ValueFormat(DataTypeEnum.Integer), 1);
      Assert.IsNotNull((ic.ToString()));
    }
  }
}