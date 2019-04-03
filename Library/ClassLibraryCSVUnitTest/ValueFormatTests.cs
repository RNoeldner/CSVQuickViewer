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