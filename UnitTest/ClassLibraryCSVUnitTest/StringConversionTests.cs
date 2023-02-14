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
using System.Globalization;
using System.Linq;

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
      Assert.AreEqual("Part2;Part3;Part4", StringConversion.StringToTextPart("Part1;Part2;Part3;Part4", ';', 2, true));
      Assert.AreEqual("Part2;Part3;Part4;", StringConversion.StringToTextPart("Part1;Part2;Part3;Part4", ';', 3, true));
      Assert.AreEqual("Part1", StringConversion.StringToTextPart("Part1;Part2;Part3;Part4", ';', 1, false));
      Assert.AreEqual("Part4", StringConversion.StringToTextPart("Part1;Part2;Part3;Part4", ';', 4, false));
    }

    [TestMethod]
    public void StringToDateTime()
    {
      Assert.AreEqual(new DateTime(2021, 5, 31, 14, 37, 0, 0, DateTimeKind.Unspecified),
        StringConversion.StringToDateTime("May 31 2021 2:37PM", "MMM d yyyy h:mmtt", "/", ":", false).Value);

      Assert.AreEqual(new DateTime(2020, 10, 8, 16, 04, 0, 0, DateTimeKind.Unspecified),
        StringConversion.StringToDateTime("Oct  8 2020  4:04PM", "MMM d yyyy h:mmtt", "", ":", false).Value);

      Assert.AreEqual(
        new DateTime(2009, 12, 10, 17, 43, 0, 0, DateTimeKind.Unspecified),
        StringConversion.StringToDateTime("12/10/2009 17:43", "MM/dd/yyyy HH:mm", "/", ":", false).Value);

      Assert.AreEqual(
        new DateTime(2009, 10, 12, 17, 43, 0, 0, DateTimeKind.Unspecified),
        StringConversion.StringToDateTime("12/10/2009 17:43", "dd/MM/yyyy HH:mm", "/", ":", false).Value);

      Assert.AreEqual(
        new DateTime(2009, 10, 12, 17, 43, 0, 0, DateTimeKind.Unspecified),
        StringConversion.StringToDateTime("12/10/2009 17:43 PM", "dd/MM/yyyy HH:mm tt", "/", ":", false).Value);
    }

    [TestMethod]
    public void DecimalToString()
    {
      Assert.AreEqual(
        "53.336,24",
        StringConversion.DecimalToString(
          (decimal) 53336.2373,
          new ValueFormat(DataTypeEnum.Numeric, groupSeparator: ".", decimalSeparator: ",", numberFormat: "#,####.00")));

      Assert.AreEqual(
        "20-000-000-000",
        StringConversion.DecimalToString(
          (decimal) 2E10,
          new ValueFormat(DataTypeEnum.Numeric,numberFormat: "#,####", groupSeparator: "-")));

      Assert.AreEqual(
        "1237,6",
        StringConversion.DecimalToString(
          1237.6m,
          new ValueFormat(DataTypeEnum.Numeric, groupSeparator: "", decimalSeparator: ",", numberFormat: "#,####.0")));
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

    [TestMethod]
    public void StringToDecimal()
    {
      // allowed grouping
      Assert.AreEqual(634678373m, StringConversion.StringToDecimal("634.678.373", ",", ".", true, true), "634.678.373");
      Assert.AreEqual(634678373.4m, StringConversion.StringToDecimal("634.678.373,4", ",", ".", true, true), "634.678.373,4");
      Assert.AreEqual(6678373.4m, StringConversion.StringToDecimal("6.678.373,4", ",", ".", true, false), "634.678.373,4");
      
      // Wrong distance between 1st and 2nd grouping
      Assert.IsNull(StringConversion.StringToDecimal("63.4678.373", ",", ".", true, true),"63.4678.373");
      // wrong grouping at end
      Assert.IsNull(StringConversion.StringToDecimal("63.467.8373", ",", ".", true, true),"63.467.8373");
      Assert.IsNull(StringConversion.StringToDecimal("63.467.8373,2", ",", ".", true, true), "63.467.8373,2");

      Assert.AreNotEqual(53m, StringConversion.StringToDecimal("5,3", ".", ",", true, true));
      Assert.AreNotEqual(53m, StringConversion.StringToDecimal("5,30", ".", ",", true, true));
      Assert.AreNotEqual(53m, StringConversion.StringToDecimal("5,3000", ".", ",", true, true));

      Assert.IsNull(StringConversion.StringToDecimal("", ",", ".", true, true));
      Assert.AreEqual(5.3m, StringConversion.StringToDecimal("5,3", ",", ".", true, true));
     

      
      // Switching grouping and decimal
      Assert.AreEqual(17295.27m, StringConversion.StringToDecimal("17,295.27", ".", ",", true, true));
      Assert.AreEqual(17295.27m, StringConversion.StringToDecimal("17.295,27", ",", ".", true, true));

      // negative Numbers
      Assert.AreEqual(-17m, StringConversion.StringToDecimal("-17", ",", ".", true, true));
      Assert.AreEqual(-17m, StringConversion.StringToDecimal("(17)", ",", ".", true, true));
      // no grouping present but supported
      Assert.AreEqual(53336.7m, StringConversion.StringToDecimal("53336,7", ",", ".", true, false));
      // no grouping present and but supported
      Assert.AreEqual(53336.7m, StringConversion.StringToDecimal("53336,7", ",", ".", true, false));
      Assert.AreEqual(53336.7m, StringConversion.StringToDecimal("53336,7", ",", "", true, false));

      Assert.AreEqual(53336.7m, StringConversion.StringToDecimal("53336,7", ",", ".", true, false));
      Assert.AreEqual(52333m, StringConversion.StringToDecimal("52.333", ",", ".", true, false));
      Assert.AreEqual(2.33m, StringConversion.StringToDecimal("233%", ",", ".", true, false));


      Assert.AreEqual(14.56m, StringConversion.StringToDecimal("$14.56", ".", "", false, true));
      Assert.AreEqual(-14.56m, StringConversion.StringToDecimal("($14.56)", ".", "", false, true));
      Assert.AreEqual(14.56m, StringConversion.StringToDecimal("14.56 €", ".", "", false, true));
    }

    [TestMethod]
    public void StringToInt32()
    {
      Assert.AreEqual(-17, StringConversion.StringToInt32("-17", ",", "."));
      Assert.AreEqual(53337, StringConversion.StringToInt32("53336,7", ",", "."));
      Assert.AreEqual(52333, StringConversion.StringToInt32("52.333", ",", "."));
    }

    [TestMethod]
    public void StringToInt16()
    {
      Assert.AreEqual((short) 5333, StringConversion.StringToInt16("5.333", ",", "."));
      Assert.AreEqual((short) -17, StringConversion.StringToInt16("-17", ",", "."));
      Assert.AreEqual((short) 5337, StringConversion.StringToInt16("5336,7", ",", "."));
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
    public void CheckSerialDateFail() =>
      // last value is not a date
      Assert.IsNull(
        StringConversion.CheckSerialDate(new[] { "239324", "239324.344", "4358784" }, false, UnitTestStatic.Token)
          .FoundValueFormat);

    [TestMethod]
    public void CheckSerialDateOk() =>
      Assert.IsNotNull(
        StringConversion.CheckSerialDate(new[] { "239324", "239324.344", "235324" }, false, UnitTestStatic.Token)
          .FoundValueFormat);

    [TestMethod]
    public void GetTimeFromTicks()
    {
      var res = StringConversion.GetTimeFromTicks(5 * TimeSpan.TicksPerMillisecond);
      Assert.AreEqual(new DateTime(1899, 12, 30, 0, 0, 0, 5), res);
    }

    [TestMethod]
    public void CombineStringsToDateTimeExcel()
    {
      var res = StringConversion.CombineObjectsToDateTime(
        new DateTime(2010, 01, 1),
        null,
        new DateTime(2001, 02, 1, 07, 13, 55, 0),
        null,
        false,
        new ValueFormat(),
        out _);
      Assert.AreEqual(new DateTime(2010, 01, 1, 07, 13, 55, 0), res);
    }

    
    [TestMethod]
    public void CheckStringToDateTimeExact()
    {
      var res1 = StringConversion.StringToDateTimeExact(
        "01/16/2008",
        @"MM/dd/yyyy",
        "/",
        ":",
        CultureInfo.CurrentCulture);
      Assert.IsTrue(res1.HasValue);
      Assert.AreEqual(16, res1.Value.Day);

      var res2 = StringConversion.StringToDateTimeExact(
        "01/16/2008",
        @"MM/dd/yyyy",
        "/",
        ":",
        CultureInfo.CurrentCulture);
      Assert.IsTrue(res2.HasValue);
      Assert.AreEqual(16, res2.Value.Day);

      var res3 = StringConversion.StringToDateTimeExact(
        "01/16/2008 10:25 pm",
        @"MM/dd/yyyy hh:mm tt",
        "/",
        ":",
        CultureInfo.InvariantCulture);
      Assert.IsTrue(res3.HasValue);
      Assert.AreEqual(16, res3.Value.Day);
      Assert.AreEqual(22, res3.Value.Hour);

      var res4 = StringConversion.StringToDateTimeExact(
        @"Freitag, 15. März 2013",
        @"dddd, d. MMMM yyyy",
        "/",
        ":",
        new CultureInfo("de-DE", false));
      Assert.IsTrue(res4.HasValue);
      Assert.AreEqual(2013, res4.Value.Year);
      Assert.AreEqual(15, res4.Value.Day);
    }

    [TestMethod]
    public void DateTimeToStringOk()
    {
      // Assert.AreEqual("01/01/2010", StringConversion.DateTimeToString(new DateTime(2010, 01, 1), null));
      Assert.AreEqual(
        "13/01/2010 10:11",
        StringConversion.DateTimeToString(
          new DateTime(2010, 1, 13, 10, 11, 14, 0),
           @"dd/MM/yyyy HH:mm", "/", ":", CultureInfo.InvariantCulture));
      // Make sure exchanging the default separators do not mess with the result
      Assert.AreEqual(
        "13:01:2010 10/11",
        StringConversion.DateTimeToString(
          new DateTime(2010, 1, 13, 10, 11, 14, 0),
          new ValueFormat(DataTypeEnum.DateTime, @"dd/MM/yyyy HH:mm", ":", "/")));
      // 24 + 24 + 7 = 55 hrs
      Assert.AreEqual(
        "055:11",
        StringConversion.DateTimeToString(
          StringConversion.GetTimeFromTicks(new TimeSpan(2, 7, 11, 0).Ticks),
          new ValueFormat(DataTypeEnum.DateTime,  @"HHH:mm", timeSeparator: ":", dateSeparator: ".")));
    }

    [TestMethod]
    public void IdentifyDatesFormatsUs()
    {
      var dateSep = "/";
      var culture = new CultureInfo("en-US");

      var formats = new [] {"dddd, d MMMM, yyyy", "MM/dd/yyyy HH:mm", "MM/dd/yyyy HH:mm", "d/M/yy h:mm tt", "yyyy/MM/dd HH:mm"};
      // Try a fwe date formats
      foreach (var fmt in formats )
      {
        // Fill Samples
        var samples = new HashSet<string>();
        for (var month = 9; month < 10; month++)
        for (var day = 10; day < 15; day++)
        for (var hrs = 11; hrs < 13; hrs++)
        for (var min = 24; min < 26; min++)
          samples.Add(new DateTime(2010, month, day, hrs, min, 10, 876, DateTimeKind.Local).ToString(fmt, culture));

        Assert.IsNotNull(
          StringConversion.CheckDate(samples, fmt, dateSep, ":", CultureInfo.CurrentCulture, UnitTestStatic.Token)
            .FoundValueFormat,
          $"Test format {fmt}\nFirst not matching: {samples.First()}");
      }
    }

    [TestMethod]
    public void StringToGuidInvalid() => Assert.IsNull(StringConversion.StringToGuid("Test"));

    [TestMethod]
    public void StringToGuidNull()
    {
      Assert.IsNull(StringConversion.StringToGuid(null));
      Assert.IsNull(StringConversion.StringToGuid(""));
    }

    [TestMethod]
    public void StringToGuidValid()
    {
      var test = new Guid(0x23e5ad85, 0x56d9, 0x482e, 0x8f, 0x26, 0x44, 0x1c, 0x4b, 0xbe, 0x89, 0x70);
      Assert.AreEqual(test, StringConversion.StringToGuid("{23E5AD85-56D9-482E-8F26-441C4BBE8970}"));
      Assert.AreEqual(test, StringConversion.StringToGuid("23E5AD85-56D9-482E-8F26-441C4BBE8970"));
    }

    [TestMethod]
    public void StringToInt64()
    {
      Assert.AreEqual(null, StringConversion.StringToInt64(null, ".", ""));
      Assert.AreEqual(17, StringConversion.StringToInt64("17.4", ".", ","));
      Assert.AreEqual(18, StringConversion.StringToInt64("17.6", ".", ","));
      Assert.AreEqual(-18, StringConversion.StringToInt64("-17.6", ".", ","));
      Assert.AreEqual(
        null,
        StringConversion.StringToInt64("99999999999999999999999999999999999999999999999999", ".", ","));
      Assert.AreEqual(null, StringConversion.StringToInt64("AB", ".", ","));
      Assert.AreEqual(null, StringConversion.StringToInt64("", ".", ","));
    }

    [TestMethod]
    public void TextPartFormatter()
    {
      var called = false;
      var fmter = new TextPartFormatter(2, ":", false);
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

    [TestMethod]
    public void StringToTextPartExact()
    {
      Assert.AreEqual(null, StringConversion.StringToTextPart(null, ':', 1, false), "Value null");
      Assert.AreEqual(null, StringConversion.StringToTextPart("Test", ':', 0, false), "Part invalid");
      Assert.AreEqual(null, StringConversion.StringToTextPart("Test:Hallo", ':', 3, false), "Part not present");
      Assert.AreEqual(null, StringConversion.StringToTextPart("Test", ':', 2, false), "Splitter not present, Part 2");
      Assert.AreEqual("Test", StringConversion.StringToTextPart("Test", ':', 1, false), "Splitter not present, Part 1");
      Assert.AreEqual(
        "Test:Hallo",
        StringConversion.StringToTextPart("Test:Hallo", '|', 1, false),
        "Splitter not contained");
      Assert.AreEqual("Test", StringConversion.StringToTextPart("Test:Hallo", ':', 1, false), "Getting Part 1/2");
      Assert.AreEqual("Hallo", StringConversion.StringToTextPart("Test:Hallo", ':', 2, false), "Getting Part 2/2");
      Assert.AreEqual(
        "Hallo",
        StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 2, false),
        "Getting Part 2/4");
      Assert.AreEqual(
        "Another",
        StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 3, false),
        "Getting Part 3/4");
      Assert.AreEqual(
        "Test2",
        StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 4, false),
        "Getting Part 4/4");
    }

    [TestMethod]
    public void StringToTextPartToEnd()
    {
      Assert.AreEqual(
        "Test:Hallo:Another:Test2",
        StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 1, true),
        "Getting Part 2-4");
      Assert.AreEqual(
        "Hallo:Another:Test2",
        StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 2, true),
        "Getting Part 2-4");
      Assert.AreEqual(
        "Another:Test2",
        StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 3, true),
        "Getting Part 3-4");
      Assert.AreEqual(
        "Test2",
        StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 4, true),
        "Getting Part 4-4");
      Assert.AreEqual(
        null,
        StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 5, true),
        "Getting Part 5");
    }

    [TestMethod]
    public void StringToTimeSpan_SerialDate()
    {
      // 01:00:00
      var actual = StringConversion.StringToTimeSpan("0.0416666666666667", ":", true);
      Assert.AreEqual(new TimeSpan(1, 0, 0), actual);

      // 02:15:00
      actual = StringConversion.StringToTimeSpan("0.0937500000000000", ":", true);
      Assert.AreEqual(new TimeSpan(2, 15, 0), actual);

      // 29/06/1968 15:23:00
      actual = StringConversion.StringToTimeSpan("0.6409722222", ":", true);
      Assert.AreEqual(new TimeSpan(15, 23, 0), actual);
    }

    [TestMethod]
    public void StringToTimeSpanInvalid()
    {
      Assert.IsNull(StringConversion.StringToTimeSpan("Test", ":", false));
      Assert.AreEqual(new TimeSpan(0, 12, 00, 00), StringConversion.StringToTimeSpan("12:BB:50", ":", false));
      Assert.IsNull(StringConversion.StringToTimeSpan("12", ":", false));
    }

    [TestMethod]
    public void StringToTimeSpanNull()
    {
      Assert.IsNull(StringConversion.StringToTimeSpan(null, null, false));
      Assert.IsNull(StringConversion.StringToTimeSpan("", null, false));
    }

    [TestMethod]
    public void StringToTimeSpanValid()
    {
      Assert.AreEqual(new TimeSpan(0, 12, 23, 50), StringConversion.StringToTimeSpan("12:23:50", ":", false));
      Assert.AreEqual(new TimeSpan(0, 25, 23, 00), StringConversion.StringToTimeSpan("25:23", ":", false));
      Assert.AreEqual(new TimeSpan(0, 17, 637, 00), StringConversion.StringToTimeSpan("17:637", ":", false));
    }

    [TestMethod]
    public void TestDate1()
    {
      var res1 = StringConversion.StringToDateTime("1879-05-01", @"yyyy/MM/dd", "-", ":", false);
      Assert.IsTrue(res1.HasValue);
      Assert.AreEqual(new DateTime(1879, 05, 01), res1.Value);
    }

    [TestMethod]
    public void TestDateTimezone()
    {
      var res1 = StringConversion.StringToDateTime(
        "1879-05-01T17:12:00+0000",
        @"yyyy/MM/ddTHH:mm:sszz00",
        "-",
        ":",
        false);
      Assert.IsTrue(res1.HasValue);
      Assert.AreEqual(new DateTime(1879, 05, 01, 19, 12, 00, DateTimeKind.Utc), res1.Value);
    }

#if Windows
    [TestMethod]
    public void TestDateTimezone2()
    {
      var res1 = StringConversion.StringToDateTime("1879-05-17T17:12:00+02", @"yyyy/MM/ddTHH:mm:sszz", "-", ":", false);
      Assert.IsTrue(res1.HasValue);
      var utc = new DateTime(1879, 05, 17, 15, 12, 00, DateTimeKind.Utc);
      // Result of parse on a mac is 17.05.1879 16:06:00 not sure how this comes
      // DateTime.TryParseExact seems to work differently
      Assert.AreEqual(utc, res1.Value.ToUniversalTime());
    }

#endif

    [TestMethod]
    public void CheckGuidTest()
    {
      Assert.IsFalse(StringConversion.CheckGuid(Array.Empty<string>(), UnitTestStatic.Token));
      Assert.IsTrue(
        StringConversion.CheckGuid(new[] { "{35C1536A-094A-493D-8FED-545A959E167A}" }, UnitTestStatic.Token));
      Assert.IsFalse(StringConversion.CheckGuid(new[] { "{35C1536A-094A-493D-8FED-545A959E167A}", "A Test" },
        UnitTestStatic.Token));
      Assert.IsTrue(
        StringConversion.CheckGuid(
          new[] { "{35C1536A-094A-493D-8FED-545A959E167A}", "9B6E2B50-5400-4871-820C-591844B4F0D6" },
          UnitTestStatic.Token));
    }

    [TestMethod]
    public void CombineStringsToDateTimeExcelTest()
    {
      Assert.IsFalse(
        StringConversion.CombineObjectsToDateTime(
          null,
          null,
          null,
          null,
          true,
          new ValueFormat(DataTypeEnum.DateTime, dateFormat: "yyyyMMdd", dateSeparator: "", timeSeparator: ":"),
          out _).HasValue);
      Assert.AreEqual(
        new DateTime(2010, 10, 10),
        StringConversion.CombineObjectsToDateTime(new DateTime(2010, 10, 10), null, null, null, true, null, out _)
          .Value);

      Assert.AreEqual(
        DateTime.FromOADate(0) + new TimeSpan(8, 12, 54),
        StringConversion.CombineObjectsToDateTime(
          null,
          null,
          new DateTime(new TimeSpan(8, 12, 54).Ticks),
          null,
          false,
          null,
          out _).Value);
      Assert.AreEqual(
        new DateTime(2010, 10, 13),
        StringConversion.CombineObjectsToDateTime(
          null,
          "2010/10/13",
          null,
          null,
          false,
          new ValueFormat(DataTypeEnum.DateTime, dateFormat: "yyyy/MM/dd", dateSeparator: "/", timeSeparator: ":"),
          out _).Value);

      Assert.AreEqual(
        new DateTime(2010, 10, 10, 8, 12, 54),
        StringConversion.CombineObjectsToDateTime(
          new DateTime(2010, 10, 10),
          null,
          new DateTime(new TimeSpan(8, 12, 54).Ticks),
          null,
          true,
          null,
          out _).Value);

      Assert.AreEqual(
        new DateTime(2010, 10, 10, 8, 12, 54),
        StringConversion.CombineObjectsToDateTime(
          null,
          "20101010",
          new DateTime(new TimeSpan(8, 12, 54).Ticks),
          null,
          true,
          new ValueFormat(DataTypeEnum.DateTime, dateFormat: "yyyyMMdd", dateSeparator: "", timeSeparator: ":"),
          out _).Value);

      Assert.AreEqual(
        new DateTime(2010, 10, 13, 8, 12, 54),
        StringConversion.CombineObjectsToDateTime(
          null,
          "2010/10/13",
          new DateTime(new TimeSpan(8, 12, 54).Ticks).ToOADate(),
          null,
          true,
          new ValueFormat(DataTypeEnum.DateTime, dateFormat: "yyyy/MM/dd", dateSeparator: "/", timeSeparator: ":"),
          out _).Value);

      Assert.AreEqual(
        new DateTime(2010, 10, 13, 8, 12, 54),
        StringConversion.CombineObjectsToDateTime(
          new DateTime(2010, 10, 13).ToOADate(),
          null,
          new DateTime(new TimeSpan(8, 12, 54).Ticks).ToOADate(),
          null,
          true,
          new ValueFormat(DataTypeEnum.DateTime, dateFormat: "yyyy/MM/dd", dateSeparator: "/", timeSeparator: ":"),
          out _).Value);

      // Pass in a time that is >23:59 to adjust date part
      Assert.AreEqual(
        new DateTime(2010, 10, 15, 5, 10, 00),
        StringConversion.CombineObjectsToDateTime(
          null,
          "2010/10/14",
          null,
          "29:10:00",
          false,
          new ValueFormat(DataTypeEnum.DateTime,dateFormat: "yyyy/MM/dd", dateSeparator: "/", timeSeparator: ":"),
          out var issues).Value);
      // should issue a warning
      Assert.IsTrue(issues);
    }

    [TestMethod]
    public void CheckNumberTest()
    {
      Assert.IsTrue(StringConversion.CheckNumber(new[] { "16673" }, ".", "", false, false, false, UnitTestStatic.Token)
        .FoundValueFormat != null);
      Assert.AreEqual(
        DataTypeEnum.Integer,
        StringConversion.CheckNumber(new[] { "16673" }, ".", "", false, false, false,UnitTestStatic.Token).FoundValueFormat!
          .DataType);
      Assert.IsFalse(
        StringConversion.CheckNumber(new[] { "16673", "A Test" }, ".", "", false, false, false,UnitTestStatic.Token)
          .FoundValueFormat != null);
      Assert.AreEqual(
        DataTypeEnum.Numeric,
        StringConversion.CheckNumber(new[] { "16673", "-23", "1.4" }, ".", "", false, false, false,UnitTestStatic.Token)
          .FoundValueFormat!
          .DataType);

      Assert.IsFalse(StringConversion.CheckNumber(Array.Empty<string>(), ".", "", false, false, false, UnitTestStatic.Token)
        .FoundValueFormat != null);
    }


    [TestMethod]
    public void CombineStringsToDateTimeTest()
    {
      Assert.IsNull(StringConversion.CombineStringsToDateTime("20161224", null, "17:24", "", ":", false));
      Assert.AreEqual(
        new DateTime(2016, 12, 24, 17, 24, 00, 0),
        StringConversion.CombineStringsToDateTime("20161224", "yyyyMMdd", "17:24", "", ":", false));
      Assert.AreEqual(
        DateTime.FromOADate(.220),
        StringConversion.CombineStringsToDateTime(".220", "yyyyMMdd", "", "", ":", true));
      Assert.AreEqual(
        DateTime.FromOADate(0).Add(new TimeSpan(17, 24, 00)),
        StringConversion.CombineStringsToDateTime("", "yyyyMMdd", "17:24", "", ":", true));
    }

    [TestMethod]
    public void StringToBooleanTest()
    {
      Assert.IsNull(StringConversion.StringToBoolean(null, "y", "n"));
      Assert.IsTrue(StringConversion.StringToBoolean("y", "y", "n").Value);
      Assert.IsFalse(StringConversion.StringToBoolean("n", "y", "n").Value);
    }
  }
}