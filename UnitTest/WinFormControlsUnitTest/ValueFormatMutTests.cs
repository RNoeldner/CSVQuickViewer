/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using CsvTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CsvTools.Tests;

[TestClass()]
public class ValueFormatMutTests
{
  private readonly ValueFormat m_ValueFormatGerman = new ValueFormat(dateFormat: "dd/MM/yyyy",
    dateSeparator: ".", decimalSeparator: ",", asFalse: "Falsch", groupSeparator: ".", numberFormat: "0.##",
    timeSeparator: "-", asTrue: "Wahr");

  [TestMethod]
  public void Ctor()
  {
    var test2 = new ValueFormatMut(dataType: DataTypeEnum.Integer);
    Assert.AreEqual(DataTypeEnum.Integer, test2.DataType);
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
  public void GroupSeparator()
  {
    var a = new ValueFormatMut(dataType: DataTypeEnum.Numeric, groupSeparator: ",", decimalSeparator: ".");
    Assert.AreEqual(",", a.GroupSeparator);
    a.GroupSeparator = ".";
    Assert.AreEqual(".", a.GroupSeparator);
    Assert.AreEqual(",", a.DecimalSeparator);
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
  public void ValueFormatCheckDefaults()
  {
    var test = new ValueFormatMut(DataTypeEnum.DateTime);
    Assert.AreEqual("MM/dd/yyyy", test.DateFormat, "DateFormat");
    Assert.AreEqual("/", test.DateSeparator, "DateSeparator");
    Assert.AreEqual(".", test.DecimalSeparator, "DecimalSeparator");
    Assert.AreEqual(string.Empty, test.GroupSeparator, "GroupSeparator");
    Assert.AreEqual("0.#####", test.NumberFormat, "NumberFormat");
    Assert.AreEqual(":", test.TimeSeparator, "TimeSeparator");
  }


  [TestMethod]
  public void ValueFormatEquals()
  {
    var target = new ValueFormatMut(DataTypeEnum.DateTime);
    var target2 = new ValueFormatMut(DataTypeEnum.DateTime);
    Assert.IsTrue(target2.Equals(target));
  }

  [TestMethod]
  [TestCategory("Serialization")]
  public void ValueFormatMutable()
  {
    var input = new ValueFormatMut(DataTypeEnum.Numeric, numberFormat: "x.00", decimalSeparator: ",");

    var output = UnitTestStatic.RunSerialize(input);
    Assert.AreEqual(input.NumberFormat, output.NumberFormat);
    Assert.AreEqual(input.DecimalSeparator, output.DecimalSeparator);
    Assert.AreEqual(input.DateFormat, output.DateFormat);
  }

  [TestMethod]
  public void ValueFormatMutableProperties()
  {
    // Arrange: Define test values
    var dataType = DataTypeEnum.DateTime;
    var dateFormat = "yyyy-MM-dd";
    var dateSep = "-";
    var timeSep = ":";
    var numFormat = "N2";
    var groupSep = ".";
    var decimalSep = ",";
    var asTrue = "Yo";
    var asFalse = "Nö";
    var displayNull = "<N>";
    var part = 3;
    var partSplitter = "|";
    var partToEnd = false;
    var regexSearch = "erp";
    var regexReplace = "read";
    var readFolder = "Wr";
    var writeFolder = "ou";
    var filePlaceholder = "pat";
    var overwrite = false;

    // Act
    var input = new ValueFormatMut(
        dataType, dateFormat, dateSep, timeSep, numFormat, groupSep, decimalSep,
        asTrue, asFalse, displayNull, part, partSplitter, partToEnd,
        regexSearch, regexReplace, readFolder, writeFolder, filePlaceholder, overwrite);

    // Assert
    Assert.AreEqual(dataType, input.DataType);
    Assert.AreEqual(dateFormat, input.DateFormat);
    Assert.AreEqual(dateSep, input.DateSeparator);
    Assert.AreEqual(timeSep, input.TimeSeparator);
    Assert.AreEqual(numFormat, input.NumberFormat);
    Assert.AreEqual(groupSep, input.GroupSeparator);
    Assert.AreEqual(decimalSep, input.DecimalSeparator);
    Assert.AreEqual(asTrue, input.True);
    Assert.AreEqual(asFalse, input.False);
    Assert.AreEqual(displayNull, input.DisplayNullAs);
    Assert.AreEqual(part, input.Part);
    Assert.AreEqual(partSplitter, input.PartSplitter);
    Assert.AreEqual(partToEnd, input.PartToEnd);
    Assert.AreEqual(regexSearch, input.RegexSearchPattern);
    Assert.AreEqual(regexReplace, input.RegexReplacement);
    Assert.AreEqual(readFolder, input.ReadFolder);
    Assert.AreEqual(writeFolder, input.WriteFolder);
    Assert.AreEqual(filePlaceholder, input.FileOutPutPlaceholder);
    Assert.AreEqual(overwrite, input.Overwrite);
  }


  [TestMethod]
  public void ValueFormatNotEquals()
  {
    var target = new ValueFormatMut(DataTypeEnum.Binary);
    // ReSharper disable once SuspiciousTypeConversion.Global
    Assert.IsFalse(m_ValueFormatGerman.Equals(target));
  }
}