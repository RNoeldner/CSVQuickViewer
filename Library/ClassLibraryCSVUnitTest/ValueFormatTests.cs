using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ValueFormatTests
  {
    [TestMethod]
    public void GroupSeparator()
    {
      var a = new ValueFormat
      {
        DataType = DataType.Numeric
      };
      a.GroupSeparator = ",";
      a.DecimalSeparator = ".";
      Assert.AreEqual(",", a.GroupSeparator);
      a.GroupSeparator = ".";
      Assert.AreEqual(".", a.GroupSeparator);
      Assert.AreEqual(",", a.DecimalSeparator);
    }

    [TestMethod]
    public void DecimalSeparator()
    {
      var a = new ValueFormat
      {
        DataType = DataType.Numeric
      };
      a.GroupSeparator = ".";
      a.DecimalSeparator = ",";
      Assert.AreEqual(",", a.DecimalSeparator);
      a.DecimalSeparator = ".";
      Assert.AreEqual(".", a.DecimalSeparator);
      Assert.AreEqual(",", a.GroupSeparator);
    }

    [TestMethod]
    public void GetFormatDescriptionTest()
    {
      var a = new ValueFormat
      {
        DataType = DataType.String
      };
      Assert.IsTrue(string.IsNullOrEmpty(a.GetFormatDescription()));
      var b = new ValueFormat
      {
        DataType = DataType.DateTime
      };
      Assert.IsTrue(b.GetFormatDescription().Contains("MM/dd/yyyy"));
    }

    [TestMethod]
    public void GetTypeAndFormatDescriptionTest()
    {
      var a = new ValueFormat
      {
        DataType = DataType.String
      };
      Assert.AreEqual("Text", a.GetTypeAndFormatDescription());
      var b = new ValueFormat
      {
        DataType = DataType.DateTime
      };
      Assert.IsTrue(b.GetTypeAndFormatDescription().Contains("Date Time"));
      Assert.IsTrue(b.GetTypeAndFormatDescription().Contains("MM/dd/yyyy"));
    }

    [TestMethod]
    public void NotifyPropertyChangedTest()
    {
      var a = new ValueFormat
      {
        DataType = DataType.DateTime
      };

      var fired = false;
      a.PropertyChanged += delegate { fired = true; };
      Assert.IsFalse(fired);
      a.DataType = DataType.Integer;
      Assert.IsTrue(fired);
    }
  }
}