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
  public class ValueFormatTest
  {
    private readonly ValueFormat m_ValueFormatGerman = new ValueFormat();

    [TestMethod]
    public void CloneTest()
    {
      var clone = m_ValueFormatGerman.Clone();
      Assert.AreNotSame(clone, m_ValueFormatGerman);
      m_ValueFormatGerman.AllPropertiesEqual(clone);
    }

    [TestMethod]
    public void Ctor2()
    {
      var test2 = new ValueFormat(DataType.Integer);
      Assert.AreEqual(DataType.Integer, test2.DataType);
    }

    [TestInitialize]
    public void Init()
    {
      m_ValueFormatGerman.DateFormat = "dd/MM/yyyy";
      m_ValueFormatGerman.DateSeparator = ".";
      m_ValueFormatGerman.DecimalSeparator = ",";
      m_ValueFormatGerman.False = "Falsch";
      m_ValueFormatGerman.GroupSeparator = ".";
      m_ValueFormatGerman.NumberFormat = "0.##";
      m_ValueFormatGerman.TimeSeparator = "-";
      m_ValueFormatGerman.True = "Wahr";
    }

    [TestMethod]
    public void ValueFormatCheckDefaults()
    {
      var test = new ValueFormat();
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
    public void ValueFormatCopyTo()
    {
      var target = new ValueFormat();
      m_ValueFormatGerman.CopyTo(target);

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
    public void ValueFormatCopyToEquals()
    {
      var target = new ValueFormat();
      m_ValueFormatGerman.CopyTo(target);
      Assert.IsTrue(m_ValueFormatGerman.Equals(target));
    }

    [TestMethod]
    public void ValueFormatEquals()
    {
      var target = new ValueFormat();
      var target2 = new ValueFormat();
      Assert.IsTrue(target2.Equals(target));
    }

    [TestMethod]
    public void ValueFormatNotEquals()
    {
      var target = new ValueFormat();
      Assert.IsFalse(m_ValueFormatGerman.Equals(target));
    }

    [TestMethod]
    public void ValueFormatNotEqualsNull()
    {
      Assert.IsFalse(m_ValueFormatGerman.Equals(null));
    }
  }
}