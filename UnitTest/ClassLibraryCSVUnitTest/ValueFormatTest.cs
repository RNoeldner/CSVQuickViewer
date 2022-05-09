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
  public class ValueFormatTest
  {
    private readonly ValueFormatMutable m_ValueFormatMutableGerman = new ValueFormatMutable()
    {
      DateFormat = "dd/MM/yyyy",
      DateSeparator = ".",
      DecimalSeparator = ",",
      False = "Falsch",
      GroupSeparator = ".",
      NumberFormat = "0.##",
      TimeSeparator = "-",
      True = "Wahr",
    };

    [TestMethod]
    public void GetFormatDescriptionTest()

    {
      var vf = new ImmutableValueFormat();
      Assert.AreEqual(string.Empty, vf.GetFormatDescription());

      var vf2 = new ImmutableValueFormat(DataTypeEnum.TextPart, part: 4);
      Assert.AreNotEqual(string.Empty, vf2.GetFormatDescription());

      var vf3 = new ImmutableValueFormat(DataTypeEnum.Integer, numberFormat: "000");
      Assert.AreNotEqual(string.Empty, vf3.GetFormatDescription());

      var vf4 = new ImmutableValueFormat(DataTypeEnum.Numeric, numberFormat: "0.##");
      Assert.AreNotEqual(string.Empty, vf4.GetFormatDescription());

      var a = new ValueFormatMutable { DataType = DataTypeEnum.String };
      Assert.IsTrue(string.IsNullOrEmpty(a.GetFormatDescription()));

      var b = new ValueFormatMutable { DataType = DataTypeEnum.DateTime };
      Assert.IsTrue(b.GetFormatDescription().Contains(ValueFormatExtension.cDateFormatDefault));
    }

    [TestMethod]
    public void IsMatching()
    {
      var expected = new ValueFormatMutable();
      var current = new ValueFormatMutable();

      foreach (DataTypeEnum item in Enum.GetValues(typeof(DataTypeEnum)))
      {
        expected.DataType = item;
        current.DataType = item;
        Assert.IsTrue(current.IsMatching(expected), item.ToString());
      }

      expected.DataType = DataTypeEnum.Integer;
      current.DataType = DataTypeEnum.Numeric;
      Assert.IsTrue(current.IsMatching(expected));

      expected.DataType = DataTypeEnum.Integer;
      current.DataType = DataTypeEnum.Double;
      Assert.IsTrue(current.IsMatching(expected));

      expected.DataType = DataTypeEnum.Numeric;
      current.DataType = DataTypeEnum.Double;
      Assert.IsTrue(current.IsMatching(expected));

      expected.DataType = DataTypeEnum.Double;
      current.DataType = DataTypeEnum.Numeric;
      Assert.IsTrue(current.IsMatching(expected));

      expected.DataType = DataTypeEnum.Numeric;
      current.DataType = DataTypeEnum.Integer;
      Assert.IsTrue(current.IsMatching(expected));

      expected.DataType = DataTypeEnum.DateTime;
      current.DataType = DataTypeEnum.DateTime;
      Assert.IsTrue(current.IsMatching(expected));

      expected.DataType = DataTypeEnum.DateTime;
      current.DataType = DataTypeEnum.String;
      Assert.IsFalse(current.IsMatching(expected));
    }

    [TestMethod]
    public void GroupSeparator()
    {
      var a = new ValueFormatMutable { DataType = DataTypeEnum.Numeric };
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
      var a = new ValueFormatMutable { DataType = DataTypeEnum.Numeric };
      a.GroupSeparator = ".";
      a.DecimalSeparator = ",";
      Assert.AreEqual(",", a.DecimalSeparator);
      a.DecimalSeparator = ".";
      Assert.AreEqual(".", a.DecimalSeparator);
      Assert.AreEqual("", a.GroupSeparator);
    }

    [TestMethod]
    public void GetTypeAndFormatDescriptionTest()
    {
      var a = new ValueFormatMutable { DataType = DataTypeEnum.String };
      Assert.AreEqual("Text", a.GetTypeAndFormatDescription());
      var b = new ValueFormatMutable { DataType = DataTypeEnum.DateTime };
      Assert.IsTrue(b.GetTypeAndFormatDescription().Contains("Date Time"));
      Assert.IsTrue(b.GetTypeAndFormatDescription().Contains("MM/dd/yyyy"));
    }

    [TestMethod]
    public void NotifyPropertyChangedTest()
    {
      var a = new ValueFormatMutable { DataType = DataTypeEnum.DateTime };

      var fired = false;
      a.PropertyChanged += delegate { fired = true; };
      Assert.IsFalse(fired);
      a.DataType = DataTypeEnum.Integer;
      Assert.IsTrue(fired);
    }

    [TestMethod]
    public void Ctor2()
    {
      var test2 = new ValueFormatMutable() { DataType = DataTypeEnum.Integer };
      Assert.AreEqual(DataTypeEnum.Integer, test2.DataType);
    }

    [TestMethod]
    public void ValueFormatCheckDefaults()
    {
      var test = new ValueFormatMutable();
      Assert.AreEqual(test.DateFormat, "MM/dd/yyyy", "DateFormat");
      Assert.AreEqual(test.DateSeparator, "/", "DateSeparator");
      Assert.AreEqual(test.DecimalSeparator, ".", "DecimalSeparator");
      Assert.AreEqual(test.False, "False", "False");
      Assert.AreEqual(test.GroupSeparator, string.Empty, "GroupSeparator");
      Assert.AreEqual(test.NumberFormat, "0.#####", "NumberFormat");
      Assert.AreEqual(test.TimeSeparator, ":", "TimeSeparator");
      Assert.AreEqual(test.True, "True", "True");
    }

    [TestMethod]
    public void ValueFormatCopyFrom()
    {
      var test1 = new ImmutableValueFormat(DataTypeEnum.Double, groupSeparator: ".", decimalSeparator: ",");
      var test2 = new ValueFormatMutable() { DataType=DataTypeEnum.Boolean };
      test2.CopyFrom(test1);
      Assert.AreEqual(DataTypeEnum.Double, test2.DataType);
      Assert.AreEqual(",", test2.DecimalSeparator);
      Assert.AreEqual(".", test2.GroupSeparator);
    }

    [TestMethod]
    public void ValueFormatCopyFrom2()
    {
      var target = new ValueFormatMutable();
      target.CopyFrom(m_ValueFormatMutableGerman);

      Assert.AreEqual(target.DateFormat, "dd/MM/yyyy");
      Assert.AreEqual(target.DateSeparator, ".");
      Assert.AreEqual(target.DecimalSeparator, ",");
      Assert.AreEqual(target.False, "Falsch");
      Assert.AreEqual(target.GroupSeparator, ".");
      Assert.AreEqual(target.NumberFormat, "0.##");
      Assert.AreEqual(target.TimeSeparator, "-");
      Assert.AreEqual(target.True, "Wahr");
    }

    [TestMethod]
    public void ValueFormatEquals()
    {
      var target = new ValueFormatMutable();
      var target2 = new ValueFormatMutable();
      Assert.IsTrue(target2.ValueFormatEqual(target));
    }

    [TestMethod]
    public void ValueFormatNotEquals()
    {
      var target = new ValueFormatMutable();
      Assert.IsFalse(m_ValueFormatMutableGerman.ValueFormatEqual(target));
    }

    [TestMethod]
    public void ValueFormatNotEqualsNull() => Assert.IsFalse(m_ValueFormatMutableGerman.ValueFormatEqual(null));
  }
}