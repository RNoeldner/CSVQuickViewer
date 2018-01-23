using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnInfoTest
  {
    [TestMethod]
    public void ColumnInfoPropertySetGet()
    {
      var ci = new ColumnInfo();
      var col = new Column();
      ci.Column = col;
      Assert.AreEqual(col, ci.Column);
      ci.FieldLength = 10;
      Assert.AreEqual(10, ci.FieldLength);

      ci.IsTimePart = true;
      Assert.AreEqual(true, ci.IsTimePart);

      var vf = new ValueFormat();
      ci.ValueFormat = vf;
      Assert.AreEqual(vf, ci.ValueFormat);
    }
  }
}