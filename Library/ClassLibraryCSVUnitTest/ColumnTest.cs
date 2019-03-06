using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnTest
  {
    [TestMethod]
    public void CopyToCloneEquals()
    {
      var col = new Column();
      col.CopyTo(null);
      var col2 = col.Clone();
      Assert.IsTrue(col2.Equals(col));
    }

    [TestMethod]
    public void GetFormatDescriptionTest()
    {
      var col = new Column();
      Assert.AreEqual(string.Empty, col.GetFormatDescription());

      col.DataType = DataType.TextPart;
      Assert.AreNotEqual(string.Empty, col.GetFormatDescription());

      col.DataType = DataType.DateTime;
      Assert.AreNotEqual(string.Empty, col.GetFormatDescription());

      col.DataType = DataType.Numeric;
      Assert.AreNotEqual(string.Empty, col.GetFormatDescription());
    }

    [TestMethod]
    public void IsMatching()
    {
      var expected = new Column();
      var current = new Column();

      foreach (DataType item in Enum.GetValues(typeof(DataType)))
      {
        expected.DataType = item;
        current.DataType = item;
        Assert.IsTrue(current.IsMatching(expected.ValueFormat), item.ToString());
      }

      expected.DataType = DataType.Integer;
      current.DataType = DataType.Numeric;
      Assert.IsTrue(current.IsMatching(expected.ValueFormat));

      expected.DataType = DataType.Integer;
      current.DataType = DataType.Double;
      Assert.IsTrue(current.IsMatching(expected.ValueFormat));

      expected.DataType = DataType.Numeric;
      current.DataType = DataType.Double;
      Assert.IsTrue(current.IsMatching(expected.ValueFormat));

      expected.DataType = DataType.Double;
      current.DataType = DataType.Numeric;
      Assert.IsTrue(current.IsMatching(expected.ValueFormat));

      expected.DataType = DataType.Numeric;
      current.DataType = DataType.Integer;
      Assert.IsTrue(current.IsMatching(expected.ValueFormat));

      expected.DataType = DataType.DateTime;
      current.DataType = DataType.DateTime;
      Assert.IsTrue(current.IsMatching(expected.ValueFormat));

      expected.DataType = DataType.DateTime;
      current.DataType = DataType.String;
      Assert.IsFalse(current.IsMatching(expected.ValueFormat));
    }
  }
}