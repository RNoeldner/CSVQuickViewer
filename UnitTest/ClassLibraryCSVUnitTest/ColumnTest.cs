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
  public class ColumnTest
  {
    [TestMethod]
    public void CopyToCloneEquals()
    {
      var col = new Column();
      col.CopyTo(null);
      var col2 = col.Clone();
      Assert.IsTrue(col2.Equals(col));
    }

    [TestMethod]
    public void GetFormatDescriptionTest()
    {
      var col = new Column();
      Assert.AreEqual(string.Empty, col.GetFormatDescription());

      col.ValueFormatMutable.DataType = DataType.TextPart;
      Assert.AreNotEqual(string.Empty, col.GetFormatDescription());

      col.ValueFormatMutable.DataType = DataType.DateTime;
      Assert.AreNotEqual(string.Empty, col.GetFormatDescription());

      col.ValueFormatMutable.DataType = DataType.Numeric;
      Assert.AreNotEqual(string.Empty, col.GetFormatDescription());
    }
  }
}