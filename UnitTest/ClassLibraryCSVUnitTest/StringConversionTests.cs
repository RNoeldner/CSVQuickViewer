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
using System.Globalization;

#pragma warning disable CS8625
#pragma warning disable CS8629

namespace CsvTools.Tests
{
  [TestClass]
  public sealed class StringConversionTests
  {
    [TestMethod]
    [Timeout(500)]
    public void StringToTextPart()
    {
      Assert.AreEqual("Part2;Part3;Part4", "Part1;Part2;Part3;Part4".AsSpan().StringToTextPart(';', 2, true).ToString());
      Assert.AreEqual("Part3;Part4", "Part1;Part2;Part3;Part4".AsSpan().StringToTextPart(';', 3, true).ToString());
      Assert.AreEqual("Part1", "Part1;Part2;Part3;Part4".AsSpan().StringToTextPart(';', 1, false).ToString());
      Assert.AreEqual("Part4", "Part1;Part2;Part3;Part4".AsSpan().StringToTextPart(';', 4, false).ToString());
    }

    [TestMethod]
    public void DisplayDateTime()
    {
      Assert.AreEqual(
        "12/14/2001",
        StringConversion.DisplayDateTime(new DateTime(2001, 12, 14), new CultureInfo("en-US")));
      Assert.AreEqual(
        "01.02.2001 07:13:55",
        StringConversion.DisplayDateTime(new DateTime(2001, 02, 1, 07, 13, 55, 0), new CultureInfo("de-DE")));
      Assert.AreEqual(
        "1:33:00 AM",
        StringConversion.DisplayDateTime(new DateTime(1899, 12, 30).AddHours(1.55), new CultureInfo("en-US")));
    }

    [TestMethod]
    public void DynamicStorageSize()
    {
      Assert.AreEqual(
        "500 Bytes",
        UnitTestStatic.ExecuteWithCulture(() => StringConversion.DynamicStorageSize(500), "de-DE"));
      Assert.AreEqual(
        "1.95 kB",
        UnitTestStatic.ExecuteWithCulture(() => StringConversion.DynamicStorageSize(2000), "en-US"));
      Assert.AreEqual(
        "9,77 kB",
        UnitTestStatic.ExecuteWithCulture(() => StringConversion.DynamicStorageSize(10000), "de-DE"));
      Assert.AreEqual(
        "195,31 kB",
        UnitTestStatic.ExecuteWithCulture(() => StringConversion.DynamicStorageSize(200000), "de-DE"));
      Assert.AreEqual(
        "1,91 MB",
        UnitTestStatic.ExecuteWithCulture(() => StringConversion.DynamicStorageSize(2000000), "de-DE"));
    }

  
    [TestMethod]
    public void GetTimeFromTicks()
    {
      var res = (5 * TimeSpan.TicksPerMillisecond).GetTimeFromTicks();
      Assert.AreEqual(new DateTime(1899, 12, 30, 0, 0, 0, 5), res);
    }


    [TestMethod]
    public void DateTimeToStringOk()
    {
      // Assert.AreEqual("01/01/2010", StringConversion.DateTimeToString(new DateTime(2010, 01, 1), null));
      Assert.AreEqual(
        "13/01/2010 10:11",
        StringConversion.DateTimeToString(
          new DateTime(2010, 1, 13, 10, 11, 14, 0),
           @"dd/MM/yyyy HH:mm", '/', ':', CultureInfo.InvariantCulture));
      // Make sure exchanging the default separators do not mess with the result
      Assert.AreEqual(
        "13:01:2010 10/11",
        new DateTime(2010, 1, 13, 10, 11, 14, 0).DateTimeToString(new ValueFormat(DataTypeEnum.DateTime, @"dd/MM/yyyy HH:mm", ":", "/")));
      // 24 + 24 + 7 = 55 hrs
      Assert.AreEqual(
        "055:11",
        new TimeSpan(2, 7, 11, 0).Ticks.GetTimeFromTicks().DateTimeToString(new ValueFormat(DataTypeEnum.DateTime, @"HHH:mm", timeSeparator: ":", dateSeparator: ".")));
    }
 
    [TestMethod]
    public void TextPartFormatter()
    {
      var called = false;
      var fmter = new TextPartFormatter(2, ':', false);
      Assert.AreEqual("Hallo", fmter.FormatInputText("Test:Hallo", _ => called = true));
      Assert.IsFalse(called);
      Assert.AreEqual("", fmter.FormatInputText("Test", _ => called = true));
      Assert.IsTrue(called);
    }

    [TestMethod]
    public void TextReplaceFormatter()
    {
      var called = false;
      var fmter = new TextReplaceFormatter("(l|L)", "L");
      Assert.AreEqual("HaLLo", fmter.FormatInputText("HaLlo", _ => called = true));
      Assert.IsTrue(called);
      called = false;
      Assert.AreEqual("Test", fmter.FormatInputText("Test", _ => called = true));
      Assert.IsFalse(called);
    }

    
   
  }
}
