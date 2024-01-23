using Microsoft.VisualStudio.TestTools.UnitTesting;
using CsvTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools.Tests
{
  [TestClass()]
  public class ColumnMutTests
  {

    private readonly ColumnMut m_Column = new ColumnMut("Dummy");

    [TestMethod]
    public void Column_Ctor()
    {
      var target1 = new ColumnMut("Name");
      Assert.IsNotNull(target1);

      var target2 = new ColumnMut("Name2", new ValueFormat(DataTypeEnum.DateTime));
      Assert.AreEqual("Name2", target2.Name);
      Assert.AreEqual(DataTypeEnum.DateTime, target2.ValueFormat.DataType);

      var target3 = new ColumnMut("Name3", ValueFormat.Empty);
      Assert.AreEqual("Name3", target3.Name);
    }

    [TestMethod]
    public void ColumnEquals()
    {
      var target = new ColumnMut("Test");
      m_Column.CopyTo(target);
      Assert.IsTrue(m_Column.Equals(target));
    }

    [TestMethod]
    public void ColumnGetDataType()
    {
      var test = new ValueFormatMut(DataTypeEnum.String);

      Assert.IsInstanceOfType(string.Empty, test.DataType.GetNetType());
      test.DataType = DataTypeEnum.DateTime;
      Assert.IsInstanceOfType(DateTime.Now, test.DataType.GetNetType());
      test.DataType = DataTypeEnum.Boolean;
      Assert.IsInstanceOfType(false, test.DataType.GetNetType());
      test.DataType = DataTypeEnum.Double;
      Assert.IsInstanceOfType(double.MinValue, test.DataType.GetNetType());
      test.DataType = DataTypeEnum.Numeric;
      Assert.IsInstanceOfType(decimal.MaxValue, test.DataType.GetNetType());
    }

    [TestMethod]
    public void ColumnMutProperties()
    {
      var input = new ColumnMut("Näme",
        new ValueFormat(DataTypeEnum.DateTime, "XXX", "-", "?", "xx", "_", "=", "Yo", "Nö", "<N>", 3, "|", false, "pat",
          "erp", "read", "Wr", "ou", false))
      { DestinationName = "->", ColumnOrdinal = 13, Convert = true };
      UnitTestStatic.RunSerializeAllProps(input,
        new[]
        {
          nameof(input.CollectionIdentifier), nameof(input.ColumnOrdinal),
          nameof(input.ValueFormat.DecimalSeparator), nameof(input.ValueFormat.NumberFormat),
          nameof(input.ValueFormat.Part), nameof(input.ValueFormat.PartSplitter),nameof(input.ValueFormat.PartToEnd),
          nameof(input.ValueFormat.False), nameof(input.ValueFormat.True)
        });

      var input2 = new ColumnMut("Näme",
        new ValueFormat(DataTypeEnum.TextPart, "XXX", "-", "?", "xx", "_", "=", "Yo", "Nö", "<N>", 3, "|", false, "pat",
          "erp", "read", "Wr", "ou", false))
      { DestinationName = "->", ColumnOrdinal = 13, Convert = true };
      UnitTestStatic.RunSerializeAllProps(input2,
        new[]
        {
          nameof(input.CollectionIdentifier), nameof(input.ColumnOrdinal),
          nameof(input.ValueFormat.DecimalSeparator), nameof(input.ValueFormat.NumberFormat),
          nameof(input.ValueFormat.DateFormat), nameof(input.ValueFormat.DateSeparator), nameof(input.ValueFormat.TimeSeparator),
          nameof(input.ValueFormat.False), nameof(input.ValueFormat.True)
        });
    }
    [TestMethod()]
    public void ColumnMutPropertyChangedTest()
    {
      var cvm = new ColumnMut("Name", new ValueFormat());
      var changed = false;
      cvm.PropertyChanged += (o, e) => changed = true;
      cvm.Name = "Name2";
      Assert.AreEqual("Name2", cvm.Name);
      Assert.IsTrue(changed);
    }

    [TestMethod]
    public void ColumnNotEquals()
    {
      var target1 = new ColumnMut("Hello");
      var target2 = new ColumnMut("World");
      Assert.IsFalse(target1.Equals(target2));
    }

    [TestMethod]
    public void ColumnNotEqualsNull() => Assert.IsFalse(m_Column.Equals(null));

    [TestMethod]
    public void ColumnProperties()
    {
      var target = new ColumnMut("Name", new ValueFormat(DataTypeEnum.Guid));
      target.ColumnOrdinal = 13;
      Assert.AreEqual(13, target.ColumnOrdinal);
      target.Convert = false;
      Assert.AreEqual(false, target.Convert);
      target.DestinationName = "->";
      Assert.AreEqual("->", target.DestinationName);
      target.Name = "Näme";
      Assert.AreEqual("Näme", target.Name);
    }

    [TestMethod]
    public void ColumnPropertyChanged()
    {
      var numCalled = 0;
      var test = new ColumnMut("Dummy");
      test.PropertyChanged += delegate
      {
        numCalled++;
      };
      test.ValueFormatMut.DataType = DataTypeEnum.DateTime;
      Assert.AreEqual(DataTypeEnum.DateTime, test.ValueFormatMut.DataType);
      Assert.IsTrue(test.Convert);

      Assert.AreEqual(2, numCalled);

      test.Name = "Name";
      Assert.AreEqual(3, numCalled);
    }

    [TestMethod]
    public void ColumnToString() => Assert.IsNotNull(m_Column.ToString());

    [TestMethod()]
    public void CopyToTest()
    {
      var cvm1 = new ColumnMut("Name", new ValueFormat());
      var cvm2 = new ColumnMut("Name2", new ValueFormat());
      cvm1.CopyTo(cvm2);
      Assert.AreEqual(cvm1.Name, cvm2.Name);
    }

    [TestMethod()]
    public void EqualsTest()
    {
      var cvm1 = new ColumnMut("Name", new ValueFormat());
      var cvm2 = new ColumnMut("Name", new ValueFormat());
      Assert.IsTrue(cvm1.Equals(cvm2));
    }

    [TestMethod]
    public void GetDataTypeDescriptionBool()
    {
      var target = new Column("Test", new ValueFormat(DataTypeEnum.Boolean));
      Assert.AreEqual("Boolean", target.GetTypeAndFormatDescription());
    }

    [TestMethod]
    public void GetDataTypeDescriptionDateTime()
    {
      var target =
        new Column("Test", new ValueFormat(DataTypeEnum.DateTime), timePart: "TPart", timePartFormat: "YYYYMMDD",
          timeZonePart: "'UTC'");
      Assert.IsTrue(target.GetTypeAndFormatDescription().IndexOf("TPart", StringComparison.InvariantCultureIgnoreCase)!=-1,
        "TimePart");
      Assert.IsTrue(
        target.GetTypeAndFormatDescription().IndexOf("YYYYMMDD", StringComparison.InvariantCultureIgnoreCase)!=-1,
        "TimePartFormat");
      Assert.IsTrue(target.GetTypeAndFormatDescription().IndexOf("'UTC'", StringComparison.InvariantCultureIgnoreCase) != -1,
        "TimeZonePart");
    }

    [TestMethod]
    public void GetDataTypeDescriptionDouble()
    {
      var target = new Column("Test",
        new ValueFormat(dataType: DataTypeEnum.Numeric, numberFormat: "00.000"));

      Assert.AreEqual("Money (High Precision) (00.000)", target.GetTypeAndFormatDescription());
    }

    [TestMethod]
    public void GetDataTypeDescriptionIgnore()
    {
      var target = new Column("Test", ignore: true);

      Assert.AreEqual("Text (Ignore)", target.GetTypeAndFormatDescription());
    }

    [TestMethod]
    public void GetDataTypeTest()
    {
      var s = "Test";
      var dt = DateTime.Now;
      decimal dec = 0;
      double dbl = 0;
      long lng = -5;
      var int32 = -5;
      var g = Guid.NewGuid();

      Assert.AreEqual(DataTypeEnum.Integer, lng.GetType().GetDataType());
      Assert.AreEqual(DataTypeEnum.Integer, int32.GetType().GetDataType());
      Assert.AreEqual(DataTypeEnum.Integer, Type.GetType("System.UInt16")!.GetDataType());
      Assert.AreEqual(DataTypeEnum.Integer, Type.GetType("System.Int64")!.GetDataType());
      Assert.AreEqual(DataTypeEnum.Double, dbl.GetType().GetDataType());
      Assert.AreEqual(DataTypeEnum.Double, Type.GetType("System.Single")!.GetDataType());
      Assert.AreEqual(DataTypeEnum.Numeric, dec.GetType().GetDataType());
      Assert.AreEqual(DataTypeEnum.DateTime, dt.GetType().GetDataType());
      Assert.AreEqual(DataTypeEnum.String, s.GetType().GetDataType());
      Assert.AreEqual(DataTypeEnum.Boolean, true.GetType().GetDataType());
      Assert.AreEqual(DataTypeEnum.Guid, g.GetType().GetDataType());
    }

    [TestInitialize]
    public void Init()
    {
      var valueFormatGerman = new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: @"dd/MM/yyyy",
        dateSeparator: ".", decimalSeparator: ",", asFalse: @"Falsch", groupSeparator: ".", numberFormat: "0.##",
        timeSeparator: ":", asTrue: @"Wahr");

      var ff = new ColumnCollection();
      var col = new Column("StartDate", valueFormatGerman, ignore: true);
      ff.Add(col);
      Assert.AreEqual("StartDate", col.Name, "Name");
      Assert.AreEqual(DataTypeEnum.DateTime, col.ValueFormat.DataType, "DataType");
      Assert.IsTrue(col.Convert, "Convert");
      Assert.IsTrue(col.Ignore, "Ignore");
    }

    [TestMethod()]
    public void ToImmutableColumnTest()
    {
      var cvm1 = new ColumnMut("Name", new ValueFormat());
      var cv2 = cvm1.ToImmutableColumn();
      Assert.AreEqual(cvm1.Name, cv2.Name);
    }

    [TestMethod()]
    public void ToStringTest()
    {
      var cvm1 = new ColumnMut("Name", new ValueFormat());
      Assert.AreEqual("Name (Text)", cvm1.ToString());
    }
  }
}