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

namespace CsvTools.Tests;
#pragma warning disable CS8625, CS8629
[TestClass]
public sealed class ValueToString
{
  [TestMethod]
  public void DecimalToString()
  {

    Assert.AreEqual(
      "1237,6",
      StringConversion.DecimalToString(
        1237.6m,
        new ValueFormat(DataTypeEnum.Numeric, groupSeparator: "", decimalSeparator: ",", numberFormat: "#,####.0")));

    Assert.AreEqual(
      "53.336,24",
      StringConversion.DecimalToString(
        (decimal) 53336.2373,
        new ValueFormat(DataTypeEnum.Numeric, groupSeparator: ".", decimalSeparator: ",", numberFormat: "#,####.00")));

    Assert.AreEqual(
      "20-000-000-000",
      StringConversion.DecimalToString(
        (decimal) 2E10,
        new ValueFormat(DataTypeEnum.Numeric, numberFormat: "#,####", groupSeparator: "-")));


    Assert.AreEqual(
      "17,6",
      StringConversion.DecimalToString(
        17.6m,
        new ValueFormat(DataTypeEnum.Numeric, groupSeparator: ".", decimalSeparator: ",", numberFormat: "#,####.0")));
  }

  [TestMethod]
  public void DoubleToString()
  {
    Assert.AreEqual(
      "1.237,6",
      StringConversion.DoubleToString(
        1237.6,
        new ValueFormat(
          DataTypeEnum.Double,
          groupSeparator: ".",
          decimalSeparator: ",",
          numberFormat: "#,####.0")));

    Assert.AreEqual(
      "17,6",
      StringConversion.DoubleToString(
        17.6,
        new ValueFormat(
          DataTypeEnum.Double,
          groupSeparator: ".",
          decimalSeparator: ",",
          numberFormat: "#,####.0")));
  }
}