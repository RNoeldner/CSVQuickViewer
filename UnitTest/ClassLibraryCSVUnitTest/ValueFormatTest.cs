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
    private readonly ValueFormat m_ValueFormatGerman = new(dateFormat: "dd/MM/yyyy",
      dateSeparator: ".", decimalSeparator: ",", asFalse: "Falsch", groupSeparator: ".", numberFormat: "0.##",
      timeSeparator: "-", asTrue: "Wahr");

    [TestMethod]
    public void GetFormatDescriptionTest()

    {
      var vf = ValueFormat.Empty;
      Assert.AreEqual(string.Empty, vf.GetFormatDescription());

      var vf2 = new ValueFormat(DataTypeEnum.TextPart, part: 4);
      Assert.AreNotEqual(string.Empty, vf2.GetFormatDescription());

      var vf3 = new ValueFormat(DataTypeEnum.Integer, numberFormat: "000");
      Assert.AreNotEqual(string.Empty, vf3.GetFormatDescription());

      var vf4 = new ValueFormat(DataTypeEnum.Numeric, numberFormat: "0.##");
      Assert.AreNotEqual(string.Empty, vf4.GetFormatDescription());

      var a = new ValueFormat(dataType: DataTypeEnum.String);
      Assert.IsTrue(string.IsNullOrEmpty(a.GetFormatDescription()));

      var b = new ValueFormat(dataType: DataTypeEnum.DateTime);
      Assert.IsTrue(b.GetFormatDescription().Contains(ValueFormat.cDateFormatDefault));
    }

    [TestMethod]
    public void IsMatching()
    {
      var expected = new ValueFormatMut(DataTypeEnum.Markdown2Html);
      var current = new ValueFormatMut(DataTypeEnum.DateTime);

      foreach (DataTypeEnum item in Enum.GetValues(typeof(DataTypeEnum)))
      {
        expected.DataType = item;
        current.DataType = item;
        Assert.IsTrue(current.ToImmutable().IsMatching(expected.ToImmutable()), item.ToString());
      }

      expected.DataType = DataTypeEnum.Integer;
      current.DataType = DataTypeEnum.Numeric;
      Assert.IsTrue(current.ToImmutable().IsMatching(expected.ToImmutable()));

      expected.DataType = DataTypeEnum.Integer;
      current.DataType = DataTypeEnum.Double;
      Assert.IsTrue(current.ToImmutable().IsMatching(expected.ToImmutable()));

      expected.DataType = DataTypeEnum.Numeric;
      current.DataType = DataTypeEnum.Double;
      Assert.IsTrue(current.ToImmutable().IsMatching(expected.ToImmutable()));

      expected.DataType = DataTypeEnum.Double;
      current.DataType = DataTypeEnum.Numeric;
      Assert.IsTrue(current.ToImmutable().IsMatching(expected.ToImmutable()));

      expected.DataType = DataTypeEnum.Numeric;
      current.DataType = DataTypeEnum.Integer;
      Assert.IsTrue(current.ToImmutable().IsMatching(expected.ToImmutable()));

      expected.DataType = DataTypeEnum.DateTime;
      current.DataType = DataTypeEnum.DateTime;
      Assert.IsTrue(current.ToImmutable().IsMatching(expected.ToImmutable()));

      expected.DataType = DataTypeEnum.DateTime;
      current.DataType = DataTypeEnum.String;
      Assert.IsFalse(current.ToImmutable().IsMatching(expected.ToImmutable()));
    }

    [TestMethod]
    public void GroupSeparator()
    {
      var a = new ValueFormatMut(dataType: DataTypeEnum.Numeric, groupSeparator: ",", decimalSeparator: ".");
      Assert.AreEqual(",", a.GroupSeparator);
      a.GroupSeparator = ".";
      Assert.AreEqual(".", a.GroupSeparator);
      Assert.AreEqual(",", a.DecimalSeparator);
    }

    [TestMethod]
    public void DecimalSeparator()
    {
      var a = new ValueFormatMut(dataType: DataTypeEnum.Numeric, groupSeparator: ".", decimalSeparator: ",");
      Assert.AreEqual(",", a.DecimalSeparator);
      a.DecimalSeparator = ".";
      Assert.AreEqual(".", a.DecimalSeparator);
      Assert.AreEqual("", a.GroupSeparator);
    }

    [TestMethod]
    public void GetTypeAndFormatDescriptionTest()
    {
      var a = new ValueFormat(dataType: DataTypeEnum.String);
      Assert.AreEqual("Text", a.GetTypeAndFormatDescription());
      var b = new ValueFormat(dataType: DataTypeEnum.DateTime);
      Assert.IsTrue(b.GetTypeAndFormatDescription().Contains("Date Time"));
      Assert.IsTrue(b.GetTypeAndFormatDescription().Contains("MM/dd/yyyy"));
    }

    [TestMethod]
    public void NotifyPropertyChangedTest()
    {
      var a = new ValueFormatMut(dataType: DataTypeEnum.DateTime);

      var fired = false;
      a.PropertyChanged += delegate { fired = true; };
      Assert.IsFalse(fired);
      a.DataType = DataTypeEnum.Integer;
      Assert.IsTrue(fired);
    }

    [TestMethod]
    public void Ctor2()
    {
      var test2 = new ValueFormatMut(dataType: DataTypeEnum.Integer);
      Assert.AreEqual(DataTypeEnum.Integer, test2.DataType);
    }

    [TestMethod]
    public void ValueFormatCheckDefaults()
    {
      var test = new ValueFormatMut();
      Assert.AreEqual("MM/dd/yyyy", test.DateFormat, "DateFormat");
      Assert.AreEqual("/", test.DateSeparator, "DateSeparator");
      Assert.AreEqual(".", test.DecimalSeparator, "DecimalSeparator");
      Assert.AreEqual("False", test.False, "False");
      Assert.AreEqual(string.Empty, test.GroupSeparator, "GroupSeparator");
      Assert.AreEqual("0.#####", test.NumberFormat, "NumberFormat");
      Assert.AreEqual(":", test.TimeSeparator, "TimeSeparator");
      Assert.AreEqual("True", test.True, "True");
    }

    [TestMethod]
    public void ValueFormatCopyFrom()
    {
      var test1 = new ValueFormat(DataTypeEnum.Double, groupSeparator: ".", decimalSeparator: ",");
      var test2 = new ValueFormatMut(dataType: DataTypeEnum.Boolean);
      test2.CopyFrom(test1);
      Assert.AreEqual(DataTypeEnum.Double, test2.DataType);
      Assert.AreEqual(",", test2.DecimalSeparator);
      Assert.AreEqual(".", test2.GroupSeparator);
    }

    [TestMethod]
    public void ValueFormatCopyFrom2()
    {
      var target = new ValueFormatMut();
      target.CopyFrom(m_ValueFormatGerman);

      Assert.AreEqual(m_ValueFormatGerman.DateFormat, target.DateFormat);
      Assert.AreEqual(m_ValueFormatGerman.DateSeparator, target.DateSeparator);
      Assert.AreEqual(m_ValueFormatGerman.DecimalSeparator, target.DecimalSeparator, "DecimalSeparator");
      Assert.AreEqual(m_ValueFormatGerman.GroupSeparator, target.GroupSeparator, "GroupSeparator");
      Assert.AreEqual(m_ValueFormatGerman.False, target.False, "False");

      Assert.AreEqual(m_ValueFormatGerman.NumberFormat, target.NumberFormat, "NumberFormat");
      Assert.AreEqual(m_ValueFormatGerman.TimeSeparator, target.TimeSeparator, "TimeSeparator");
      Assert.AreEqual(m_ValueFormatGerman.True, target.True, "True");
    }

    [TestMethod]
    public void ValueFormatEquals()
    {
      var target = new ValueFormatMut();
      var target2 = new ValueFormatMut();
      Assert.IsTrue(target2.Equals(target));
    }

    [TestMethod]
    public void ValueFormatNotEquals()
    {
      var target = new ValueFormatMut();
      Assert.IsFalse(m_ValueFormatGerman.Equals(target));
    }

  }
}