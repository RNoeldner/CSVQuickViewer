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
using System.Collections.Generic;

namespace CsvTools.Tests
{
  [TestClass]
  public class DateTimeFormatInformationTest
  {
    private ICollection<string> GetSamples(string format)
    {
      var l = new List<string>
      {
        new DateTime(1980, 1, 1, 1, 1, 1, 1, DateTimeKind.Local).ToString(format),
        new DateTime(1999, 11, 5, 7, 15, 5, 87, DateTimeKind.Local).ToString(format),
        new DateTime(1999, 11, 6, 9, 5, 15, 67, DateTimeKind.Local).ToString(format),
        new DateTime(1999, 11, 7, 10, 6, 5, 9, DateTimeKind.Local).ToString(format),
        new DateTime(1999, 11, 8, 12, 17, 5, 10, DateTimeKind.Local).ToString(format),
        new DateTime(1999, 11, 9, 13, 55, 25, 1, DateTimeKind.Local).ToString(format),
        new DateTime(1999, 11, 10, 13, 55, 25, 1, DateTimeKind.Local).ToString(format),
        new DateTime(1999, 12, 11, 18, 45, 40, 183, DateTimeKind.Local).ToString(format)
      };
      return l;
    }

    private DateTimeFormatInformation CheckDate(string format)
    {
      var dtm = new DateTimeFormatInformation(format);
      var l = GetSamples(format);

      var minLength = int.MaxValue;
      var maxLength = int.MinValue;

      foreach (var text in l)
      {
        if (text.Length < minLength)
          minLength = text.Length;
        if (text.Length > maxLength)
          maxLength = text.Length;
      }
      Assert.IsTrue(dtm.MinLength <= minLength, $"Minium is {minLength} for {format}");
      Assert.IsTrue(dtm.MaxLength >= maxLength, $"Minium is {maxLength} for {format}");
      return dtm;
    }

    [TestMethod]
    public void DateTimeFormatInformationCheck1()
    {
      var dtm = CheckDate("yyyy/MM/ddTHH:mm:sszzz");

      Assert.AreEqual(25, dtm.MinLength);
      Assert.AreEqual(25, dtm.MaxLength);
    }

    [TestMethod]
    public void DateTimeFormatInformationCheck2()
    {
      var dtm = CheckDate("yyyy/MM/dd");

      Assert.AreEqual(10, dtm.MinLength);
      Assert.AreEqual(10, dtm.MaxLength);
    }

    [TestMethod]
    public void DateTimeFormatInformationCheck3()
    {
      var dtm = CheckDate("yyyy/M/d");

      Assert.AreEqual(8, dtm.MinLength);
      Assert.AreEqual(10, dtm.MaxLength);
    }

    [TestMethod]
    public void DateTimeFormatInformationCheck4()
    {
      var dtm = CheckDate("yyyy/M/dTHH:mm:ss");

      Assert.AreEqual(17, dtm.MinLength);
      Assert.AreEqual(19, dtm.MaxLength);
    }

    [TestMethod]
    public void DateTimeFormatInformationCheck5()
    {
      var dtm = CheckDate("yyyy/M/dTH:m:s");

      Assert.AreEqual(14, dtm.MinLength);
      Assert.AreEqual(19, dtm.MaxLength);
    }
  }
}