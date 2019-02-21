using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnFormatTest
  {
    private readonly Column m_Column = new Column();

    [TestMethod]
    public void ColumnDetermineDataTypeFromType()
    {
      var b = true;
      var s = "Test";
      var dt = DateTime.Now;
      decimal dec = 0;
      double dbl = 0;
      long lng = -5;
      var int32 = -5;
      var g = Guid.NewGuid();

      Assert.AreEqual(DataType.Integer, lng.GetType().GetDataType());
      Assert.AreEqual(DataType.Integer, int32.GetType().GetDataType());
      Assert.AreEqual(DataType.Integer, Type.GetType("System.UInt16").GetDataType());
      Assert.AreEqual(DataType.Integer, Type.GetType("System.Int64").GetDataType());
      Assert.AreEqual(DataType.Double, dbl.GetType().GetDataType());
      Assert.AreEqual(DataType.Double, Type.GetType("System.Single").GetDataType());
      Assert.AreEqual(DataType.Numeric, dec.GetType().GetDataType());
      Assert.AreEqual(DataType.DateTime, dt.GetType().GetDataType());
      Assert.AreEqual(DataType.String, s.GetType().GetDataType());
      Assert.AreEqual(DataType.Boolean, b.GetType().GetDataType());
      Assert.AreEqual(DataType.Guid, g.GetType().GetDataType());
    }

    [TestMethod]
    public void ColumnEquals()
    {
      var target = new Column();
      m_Column.CopyTo(target);
      Assert.IsTrue(m_Column.Equals(target));
    }

    [TestMethod]
    public void ColumnGetDataType()
    {
      var test = new ValueFormat();

      Assert.IsInstanceOfType(string.Empty, test.DataType.GetNetType());
      test.DataType = DataType.DateTime;
      Assert.IsInstanceOfType(DateTime.Now, test.DataType.GetNetType());
      test.DataType = DataType.Boolean;
      var b = false;
      Assert.IsInstanceOfType(b, test.DataType.GetNetType());
      test.DataType = DataType.Double;
      Assert.IsInstanceOfType(double.MinValue, test.DataType.GetNetType());
      test.DataType = DataType.Numeric;
      Assert.IsInstanceOfType(decimal.MaxValue, test.DataType.GetNetType());
    }

    //[TestMethod]
    //public void ColumnGetHashCode()
    //{
    //  var test1 = new Column ();
    //  var test2 = new Column ();
    //  Assert.AreEqual(test1.GetHashCode(), test2.GetHashCode());
    //  Assert.AreEqual(test1.GetHashCode(), test1.GetHashCode());
    //}

    [TestMethod]
    public void ColumnNotEquals()
    {
      var target = new Column();
      Assert.IsFalse(m_Column.Equals(target));
    }

    [TestMethod]
    public void ColumnNotEqualsNull()
    {
      Assert.IsFalse(m_Column.Equals(null));
    }

    [TestMethod]
    public void ColumnPropertyChanged()
    {
      var numCalled = 0;
      var test = new Column();
      test.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
      {
        Assert.IsTrue(e.PropertyName == nameof(Column.DataType) || e.PropertyName == nameof(Column.Convert));
        numCalled++;
      };
      test.DataType = DataType.DateTime;
      Assert.IsTrue(test.Convert);
      Assert.AreEqual(DataType.DateTime, test.DataType);
      Assert.AreEqual(2, numCalled);
    }

    [TestMethod]
    public void ColumnToString()
    {
      Assert.IsNotNull(m_Column.ToString());
    }

    [TestMethod]
    public void GetDataTypeDescriptionBool()
    {
      var target = new Column();
      target.DataType = DataType.Boolean;
      target.Name = "Test";
      Assert.AreEqual("Boolean", target.GetTypeAndFormatDescription());
    }

    [TestMethod]
    public void GetDataTypeDescriptionDouble()
    {
      var target = new Column();
      target.DataType = DataType.Numeric;
      target.Name = "Test";
      target.NumberFormat = "00.000";

      Assert.AreEqual("Money (High Precision) (00.000)", target.GetTypeAndFormatDescription());
    }

    [TestMethod]
    public void GetDataTypeDescriptionIgnore()
    {
      var target = new Column();
      target.DataType = DataType.String;
      target.Name = "Test";
      target.Ignore = true;

      Assert.AreEqual("Text (Ignore)", target.GetTypeAndFormatDescription());
    }

    [TestInitialize]
    public void Init()
    {
      var m_ValueFormatGerman = new ValueFormat();
      m_ValueFormatGerman.DataType = DataType.DateTime;
      m_ValueFormatGerman.DateFormat = @"dd/MM/yyyy";
      m_ValueFormatGerman.DateSeparator = ".";
      m_ValueFormatGerman.DecimalSeparator = ",";
      m_ValueFormatGerman.False = @"Falsch";
      m_ValueFormatGerman.GroupSeparator = ".";
      m_ValueFormatGerman.NumberFormat = "0.##";
      m_ValueFormatGerman.TimeSeparator = ":";
      m_ValueFormatGerman.True = @"Wahr";

      var ff = new CsvFile();

      m_Column.Name = "StartDate";
      m_Column.Ignore = true;
      m_Column.ValueFormat = m_ValueFormatGerman;

      ff.ColumnAdd(m_Column);
      Assert.AreEqual("StartDate", m_Column.Name, "Name");
      Assert.AreEqual(DataType.DateTime, m_Column.DataType, "DataType");
      Assert.IsTrue(m_Column.Convert, "Convert");
      Assert.IsTrue(m_Column.Ignore, "Ignore");
    }

    //[TestMethod]
    //public void ColumnDetermineDataTypeFromTypeName()
    //{
    //  Assert.AreEqual(DataType.Integer, Column.DetermineDataTypeFromTypeName("System.Int32"));
    //  Assert.AreEqual(DataType.Double, Column.DetermineDataTypeFromTypeName("System.Double"));
    //  Assert.AreEqual(DataType.Numeric, Column.DetermineDataTypeFromTypeName("System.Decimal"));
    //  Assert.AreEqual(DataType.DateTime, Column.DetermineDataTypeFromTypeName("System.DateTime"));
    //  Assert.AreEqual(DataType.String, Column.DetermineDataTypeFromTypeName("string"));
    //  Assert.AreEqual(DataType.Boolean, Column.DetermineDataTypeFromTypeName("System.Boolean"));
    //  Assert.AreEqual(DataType.Guid, Column.DetermineDataTypeFromTypeName("System.Guid"));
    //}
  }
}