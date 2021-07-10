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
  public class ColumnSizeTest
  {
    [TestMethod]
    public void CloneTest()
    {
      var a = new ColumnSize
      {
        ColumnName = "ColumnName",
        ColumnOrdinal = 1,
        Size = 100
      };
      var b = a.Clone();
      Assert.AreNotSame(a, b);
      Assert.AreEqual(a.ColumnName, b.ColumnName);
      Assert.AreEqual(a.ColumnOrdinal, b.ColumnOrdinal);
      Assert.AreEqual(a.Size, b.Size);
    }

    [TestMethod]
    public void CopyToTest()
    {
      var a = new ColumnSize
      {
        ColumnName = "ColumnName",
        ColumnOrdinal = 1,
        Size = 100
      };

      var b = new ColumnSize
      {
        ColumnName = "ColumnName2",
        ColumnOrdinal = 2,
        Size = 110
      };

      a.CopyTo(b);
      Assert.AreEqual(a.ColumnName, b.ColumnName);
      Assert.AreEqual(a.ColumnOrdinal, b.ColumnOrdinal);
      Assert.AreEqual(a.Size, b.Size);
      Assert.AreEqual(a.Size, b.Size);
    }

    [TestMethod]
    public void EqualsTest()
    {
      var a = new ColumnSize
      {
        ColumnName = "ColumnName",
        ColumnOrdinal = 1,
        Size = 100
      };

      var b = new ColumnSize
      {
        ColumnName = "ColumnName2",
        ColumnOrdinal = 2,
        Size = 110
      };

      var c = new ColumnSize
      {
        ColumnName = "ColumnName",
        ColumnOrdinal = 1,
        Size = 100
      };
      Assert.IsFalse(a.Equals(null));
      Assert.IsFalse(a.Equals(b));
      Assert.IsFalse(b.Equals(c));
      Assert.IsTrue(a.Equals(c));
      Assert.IsTrue(c.Equals(a));
    }
  }
}