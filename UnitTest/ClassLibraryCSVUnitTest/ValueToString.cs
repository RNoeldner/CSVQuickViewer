using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvTools.Tests
{
#pragma warning disable CS8625, CS8629
  [TestClass]
  public sealed class ValueToString
  {
    [TestMethod]
    public void DecimalToString()
    {

      Assert.AreEqual(
        "1237,6",
        StringConversion.DecimalToString(
          1237.6m,
          new ValueFormat(DataTypeEnum.Numeric, groupSeparator: "", decimalSeparator: ",", numberFormat: "#,####.0")));

      Assert.AreEqual(
        "53.336,24",
        StringConversion.DecimalToString(
          (decimal) 53336.2373,
          new ValueFormat(DataTypeEnum.Numeric, groupSeparator: ".", decimalSeparator: ",", numberFormat: "#,####.00")));

      Assert.AreEqual(
        "20-000-000-000",
        StringConversion.DecimalToString(
          (decimal) 2E10,
          new ValueFormat(DataTypeEnum.Numeric, numberFormat: "#,####", groupSeparator: "-")));


      Assert.AreEqual(
        "17,6",
        StringConversion.DecimalToString(
          17.6m,
          new ValueFormat(DataTypeEnum.Numeric, groupSeparator: ".", decimalSeparator: ",", numberFormat: "#,####.0")));
    }

    [TestMethod]
    public void DoubleToString()
    {
      Assert.AreEqual(
        "1.237,6",
        StringConversion.DoubleToString(
          1237.6,
          new ValueFormat(
            DataTypeEnum.Double,
            groupSeparator: ".",
            decimalSeparator: ",",
            numberFormat: "#,####.0")));

      Assert.AreEqual(
        "17,6",
        StringConversion.DoubleToString(
          17.6,
          new ValueFormat(
            DataTypeEnum.Double,
            groupSeparator: ".",
            decimalSeparator: ",",
            numberFormat: "#,####.0")));
    }
  }
}