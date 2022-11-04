/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CsvTools.Tests
{
  [TestClass]
  public class ColumnMutTest
  {
    private readonly ColumnMut m_Column = new("Dummy");

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
      // ReSharper disable once AssignNullToNotNullAttribute
      Assert.AreEqual(DataTypeEnum.Integer, Type.GetType("System.UInt16").GetDataType());
      // ReSharper disable once AssignNullToNotNullAttribute
      Assert.AreEqual(DataTypeEnum.Integer, Type.GetType("System.Int64").GetDataType());
      Assert.AreEqual(DataTypeEnum.Double, dbl.GetType().GetDataType());
      // ReSharper disable once AssignNullToNotNullAttribute
      Assert.AreEqual(DataTypeEnum.Double, Type.GetType("System.Single").GetDataType());
      Assert.AreEqual(DataTypeEnum.Numeric, dec.GetType().GetDataType());
      Assert.AreEqual(DataTypeEnum.DateTime, dt.GetType().GetDataType());
      Assert.AreEqual(DataTypeEnum.String, s.GetType().GetDataType());
      Assert.AreEqual(DataTypeEnum.Boolean, true.GetType().GetDataType());
      Assert.AreEqual(DataTypeEnum.Guid, g.GetType().GetDataType());
    }

    [TestMethod]
    public void ColumnPropertiesObsolete()
    {
      var target = new ColumnMut("Name", new ValueFormat(DataTypeEnum.Guid));
      target.DataType = DataTypeEnum.Boolean;
      Assert.AreEqual(DataTypeEnum.Boolean, target.DataType);
      target.DateFormat = "xxx";
      Assert.AreEqual("xxx", target.DateFormat);
      target.DateSeparator = "-";
      Assert.AreEqual("-", target.DateSeparator);
      target.DecimalSeparator = "_";
      Assert.AreEqual("_", target.DecimalSeparator);
      target.False = "nö";
      Assert.AreEqual("nö", target.False);
      target.GroupSeparator = "'";
      Assert.AreEqual("'", target.GroupSeparator);
      target.TimeSeparator = "?";
      Assert.AreEqual("?", target.TimeSeparator);
      target.NumberFormat = "yyy";
      Assert.AreEqual("yyy", target.NumberFormat);
      target.PartSplitter = "|";
      Assert.AreEqual("|", target.PartSplitter);
      target.PartToEnd = false;
      Assert.AreEqual(false, target.PartToEnd);
      target.Part = 17;
      Assert.AreEqual(17, target.Part);
      target.True = "Yo";
      Assert.AreEqual("Yo", target.True);
    }

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
    public void ColumnNotEquals()
    {
      var target1 = new ColumnMut("Hello");
      var target2 = new ColumnMut("World");
      Assert.IsFalse(target1.Equals(target2));
    }

    [TestMethod]
    public void ColumnNotEqualsNull() => Assert.IsFalse(m_Column.Equals(null));

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
      Assert.IsTrue(target.GetTypeAndFormatDescription().Contains("TPart", StringComparison.InvariantCultureIgnoreCase),
        "TimePart");
      Assert.IsTrue(
        target.GetTypeAndFormatDescription().Contains("YYYYMMDD", StringComparison.InvariantCultureIgnoreCase),
        "TimePartFormat");
      Assert.IsTrue(target.GetTypeAndFormatDescription().Contains("'UTC'", StringComparison.InvariantCultureIgnoreCase),
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

    [TestInitialize]
    public void Init()
    {
      var valueFormatGerman = new ValueFormat(dataType: DataTypeEnum.DateTime, dateFormat: @"dd/MM/yyyy",
        dateSeparator: ".", decimalSeparator: ",", asFalse: @"Falsch", groupSeparator: ".", numberFormat: "0.##",
        timeSeparator: ":", asTrue: @"Wahr");

      var ff = new CsvFile("Dummy");
      var col = new Column("StartDate", valueFormatGerman, ignore: true);

      ff.ColumnCollection.Add(col);
      Assert.AreEqual("StartDate", col.Name, "Name");
      Assert.AreEqual(DataTypeEnum.DateTime, col.ValueFormat.DataType, "DataType");
      Assert.IsTrue(col.Convert, "Convert");
      Assert.IsTrue(col.Ignore, "Ignore");
    }
  }
}