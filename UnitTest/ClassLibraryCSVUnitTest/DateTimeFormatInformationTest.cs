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
using System.Collections.Generic;

namespace CsvTools.Tests;

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

  private DateTimeFormatInformation CheckDateMinMaxCorrect(string format)
  {
    var info = new DateTimeFormatInformation(format);
    var samples = GetSamples(format);

    int minLen = int.MaxValue;
    int maxLen = int.MinValue;

    foreach (var sample in samples)
    {
      int len = sample.Length;

      if (len < minLen)
      {
        minLen = len;
        Assert.IsTrue(info.MinLength <= minLen,
            $"Unexpected minimum length {minLen} for format '{format}'.\nSample: {sample}");
      }

      if (len > maxLen)
      {
        maxLen = len;
        Assert.IsTrue(info.MaxLength >= maxLen,
            $"Unexpected maximum length {maxLen} for format '{format}'.\nSample: {sample}");
      }
    }

    return info;
  }

  [TestMethod]
  public void DateTimeFormatInformationCheck1()
  {
    var dtm = CheckDateMinMaxCorrect("yyyy/MM/ddTHH:mm:sszzz");

    Assert.AreEqual(25, dtm.MinLength);
    Assert.AreEqual(25, dtm.MaxLength);
  }

  [TestMethod]
  public void DateTimeFormatInformationCheck2()
  {
    var dtm = CheckDateMinMaxCorrect("yyyy/MM/dd");

    Assert.AreEqual(10, dtm.MinLength);
    Assert.AreEqual(10, dtm.MaxLength);
  }

  [TestMethod]
  public void DateTimeFormatInformationCheck3()
  {
    var dtm = CheckDateMinMaxCorrect("yyyy/M/d");

    Assert.AreEqual(8, dtm.MinLength);
    Assert.AreEqual(10, dtm.MaxLength);
  }

  [TestMethod]
  public void DateTimeFormatInformationCheck4()
  {
    var dtm = CheckDateMinMaxCorrect("yyyy/M/dTHH:mm:ss");

    Assert.AreEqual(17, dtm.MinLength);
    Assert.AreEqual(19, dtm.MaxLength);
  }

  [TestMethod]
  public void DateTimeFormatInformationCheck5()
  {
    var dtm = CheckDateMinMaxCorrect("yyyy/M/dTH:m:s");

    Assert.AreEqual(14, dtm.MinLength);
    Assert.AreEqual(19, dtm.MaxLength);
  }
}