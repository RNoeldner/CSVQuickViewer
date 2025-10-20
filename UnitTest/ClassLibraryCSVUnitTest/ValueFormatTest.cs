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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
// ReSharper disable StringLiteralTypo
// ReSharper disable ArrangeObjectCreationWhenTypeEvident
#pragma warning disable IDE0090

namespace CsvTools.Tests
{
  [TestClass]
  public class ValueFormatTest
  {
    
    [TestMethod]
    public void GetFormatDescriptionTest()

    {
      var vf = ValueFormat.Empty;
      Assert.AreEqual(string.Empty, vf.GetFormatDescription());

      var vf2 = new ValueFormat(DataTypeEnum.TextPart, part: 4);
      Assert.AreNotEqual(string.Empty, vf2.GetFormatDescription());

      var vf3 = new ValueFormat(DataTypeEnum.Integer, numberFormat: "000");
      Assert.AreEqual(string.Empty, vf3.GetFormatDescription());

      var vf4 = new ValueFormat(DataTypeEnum.Numeric, numberFormat: "0.##");
      Assert.AreNotEqual(string.Empty, vf4.GetFormatDescription());

      var a = new ValueFormat(dataType: DataTypeEnum.String);
      Assert.IsTrue(string.IsNullOrEmpty(a.GetFormatDescription()));

      var b = new ValueFormat(dataType: DataTypeEnum.DateTime);
      Assert.IsTrue(b.GetFormatDescription().Contains(ValueFormat.Empty.DateFormat));
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

  }
}
