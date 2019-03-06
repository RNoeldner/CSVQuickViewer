using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CsvTools.Tests
{
  [TestClass]
  public class StringConversionTests
  {
    [TestMethod]
    public void StringToDateTime()
    {
      Assert.AreEqual(new DateTime(2009, 12, 10, 17, 43, 0, 0, DateTimeKind.Unspecified),
        StringConversion.StringToDateTime("12/10/2009 17:43", "MM/dd/yyyy HH:mm", "/", ":", false).Value);

      Assert.AreEqual(new DateTime(2009, 10, 12, 17, 43, 0, 0, DateTimeKind.Unspecified),
        StringConversion.StringToDateTime("12/10/2009 17:43", "dd/MM/yyyy HH:mm", "/", ":", false).Value);

      Assert.AreEqual(new DateTime(2009, 10, 12, 17, 43, 0, 0, DateTimeKind.Unspecified),
        StringConversion.StringToDateTime("12/10/2009 17:43 PM", "dd/MM/yyyy HH:mm tt", "/", ":", false).Value);
    }

    [TestMethod]
    public void DecimalToString()
    {
      Assert.AreEqual("53.336,24", StringConversion.DecimalToString((decimal)53336.2373, new ValueFormat()
      {
        GroupSeparator = ".",
        DecimalSeparator = ",",
        NumberFormat = "#,####.00"
      }));

      Assert.AreEqual("20-000-000-000", StringConversion.DecimalToString((decimal)2E10, new ValueFormat()
      {
        GroupSeparator = "-",
        NumberFormat = "#,####"
      }));
    }

    [TestMethod]
    public void DoubleToString()
    {
      Assert.AreEqual("17,6", StringConversion.DoubleToString(17.6, new ValueFormat()
      {
        GroupSeparator = ".",
        DecimalSeparator = ",",
        NumberFormat = "#,####.0"
      }));
      Assert.AreEqual("1.237,6", StringConversion.DoubleToString(1237.6, new ValueFormat()
      {
        GroupSeparator = ".",
        DecimalSeparator = ",",
        NumberFormat = "#,####.0"
      }));

      Assert.AreEqual("17,6", StringConversion.DecimalToString(17.6m, new ValueFormat()
      {
        GroupSeparator = ".",
        DecimalSeparator = ",",
        NumberFormat = "#,####.0"
      }));
      Assert.AreEqual("1.237,6", StringConversion.DecimalToString(1237.6m, new ValueFormat()
      {
        GroupSeparator = ".",
        DecimalSeparator = ",",
        NumberFormat = "#,####.0"
      }));
    }

    [TestMethod]
    public void StringToDecimal()
    {
      Assert.IsNull(StringConversion.StringToDecimal("", ',', '.', true));
      Assert.AreEqual(-17m, StringConversion.StringToDecimal("-17", ',', '.', true));
      Assert.AreEqual(-17m, StringConversion.StringToDecimal("(17)", ',', '.', true));
      Assert.AreEqual(53336.7m, StringConversion.StringToDecimal("53336,7", ',', '.', true));
      Assert.AreEqual(52333m, StringConversion.StringToDecimal("52.333", ',', '.', true));
      Assert.AreEqual(2.33m, StringConversion.StringToDecimal("233%", ',', '.', true));
    }

    [TestMethod]
    public void StringToInt32()
    {
      Assert.AreEqual(-17, StringConversion.StringToInt32("-17", ',', '.'));
      Assert.AreEqual(53337, StringConversion.StringToInt32("53336,7", ',', '.'));
      Assert.AreEqual(52333, StringConversion.StringToInt32("52.333", ',', '.'));
    }

    [TestMethod]
    public void StringToInt16()
    {
      Assert.AreEqual((short)-17, StringConversion.StringToInt16("-17", ',', '.'));
      Assert.AreEqual((short)5337, StringConversion.StringToInt16("5336,7", ',', '.'));
      Assert.AreEqual((short)5333, StringConversion.StringToInt16("5.333", ',', '.'));
    }

    [TestMethod]
    public void TestStartDate()
    {
      Assert.AreEqual(0, new DateTime(1899, 12, 30).ToOADate());
      Assert.AreEqual(0, StringConversion.FixExcelNPOIDate(new DateTime(1899, 12, 31)).ToOADate());
    }

    [TestMethod]
    public void DisplayDateTime()
    {
      Assert.AreEqual("12/14/2001",
        StringConversion.DisplayDateTime(new DateTime(2001, 12, 14), new CultureInfo("en-US")));
      Assert.AreEqual("01.02.2001 07:13:55",
        StringConversion.DisplayDateTime(new DateTime(2001, 02, 1, 07, 13, 55, 0), new CultureInfo("de-DE")));
      Assert.AreEqual("1:33:00 AM",
        StringConversion.DisplayDateTime(new DateTime(1899, 12, 30).AddHours(1.55), new CultureInfo("en-US")));
    }

    [TestMethod]
    public void DynamicStorageSize()
    {
      StringConversion.DynamicStorageSize(500);
      StringConversion.DynamicStorageSize(2000);
      StringConversion.DynamicStorageSize(10000);
      StringConversion.DynamicStorageSize(200000);
      StringConversion.DynamicStorageSize(2000000);
      Assert.Inconclusive();
    }

    [TestMethod]
    public void CheckSerialDateFail()
    {
      // last value is not a date
      Assert.IsNull(StringConversion.CheckSerialDate(new[] { "239324", "239324.344", "4358784" }, false)
        .FoundValueFormat);
    }

    [TestMethod]
    public void CheckSerialDateOK()
    {
      Assert.IsNotNull(StringConversion.CheckSerialDate(new[] { "239324", "239324.344", "235324" }, false)
        .FoundValueFormat);
    }

    [TestMethod]
    public void GetTimeFromTicks()
    {
      var res = StringConversion.GetTimeFromTicks(5 * TimeSpan.TicksPerMillisecond);
      Assert.AreEqual(new DateTime(1899, 12, 30, 0, 0, 0, 5), res);
    }

    [TestMethod]
    public void CombineStringsToDateTimeExcel()
    {
      var res = StringConversion.CombineObjectsToDateTime(new DateTime(2010, 01, 1), null,
        new DateTime(2001, 02, 1, 07, 13, 55, 0), null, false, new Column(), out var _);
      Assert.AreEqual(new DateTime(2010, 01, 1, 07, 13, 55, 0), res);
    }

    [TestMethod]
    public void CheckStringToDateTimeGermanIssue()
    {
      var res1 = StringConversion.StringToDateTimeExact("01/07/2008", @"MM/dd/yyyy", ".", ":",
        CultureInfo.CurrentCulture);
      Assert.IsFalse(res1.HasValue);
    }

    [TestMethod]
    public void CheckStringToDateTimeExact()
    {
      var res1 = StringConversion.StringToDateTimeExact("01/16/2008", @"MM/dd/yyyy", "/", ":",
        CultureInfo.CurrentCulture);
      Assert.IsTrue(res1.HasValue);
      Assert.AreEqual(16, res1.Value.Day);

      var res2 = StringConversion.StringToDateTimeExact("01/16/2008", @"MM/dd/yyyy", "/", ":",
        CultureInfo.CurrentCulture);
      Assert.IsTrue(res2.HasValue);
      Assert.AreEqual(16, res2.Value.Day);

      var res3 = StringConversion.StringToDateTimeExact("01/16/2008 10:25 pm", @"MM/dd/yyyy hh:mm tt", "/", ":",
        CultureInfo.CurrentCulture);
      Assert.IsTrue(res3.HasValue);
      Assert.AreEqual(16, res3.Value.Day);
      Assert.AreEqual(22, res3.Value.Hour);

      var res4 = StringConversion.StringToDateTimeExact(@"Freitag, 15. März 2013", @"dddd, d. MMMM yyyy", "/", ":",
        new CultureInfo("de-DE", false));
      Assert.IsTrue(res4.HasValue);
      Assert.AreEqual(2013, res4.Value.Year);
      Assert.AreEqual(15, res4.Value.Day);
    }

    [TestMethod]
    public void CheckTimeDefault()
    {
      Assert.IsTrue(StringConversion.CheckTime(new[] { "10:00:00", "10:00", "1:00" }, ""));
    }

    [TestMethod]
    public void CheckTimeNotOK()
    {
      Assert.IsFalse(StringConversion.CheckTime(new[] { "10:00:00", "Test", "1:00" }, ":"));
    }

    [TestMethod]
    public void CheckTimeNull()
    {
      Assert.IsFalse(StringConversion.CheckTime(null, ""));
    }

    [TestMethod]
    public void CheckTimeOK()
    {
      Assert.IsTrue(StringConversion.CheckTime(new[] { "10:00:00", "10:00", "1:00" }, ":"));
    }

    [TestMethod]
    public void DateTimeToStringOK()
    {
      Assert.AreEqual("01/01/2010", StringConversion.DateTimeToString(new DateTime(2010, 01, 1), null));
      Assert.AreEqual("13/01/2010 10:11", StringConversion.DateTimeToString(new DateTime(2010, 1, 13, 10, 11, 14, 0),
        new ValueFormat
        {
          DateFormat = @"dd/MM/yyyy HH:mm"
        }));
      // Make sure exchanging the default separators do not mess with the result
      Assert.AreEqual("13:01:2010 10/11", StringConversion.DateTimeToString(new DateTime(2010, 1, 13, 10, 11, 14, 0),
        new ValueFormat
        {
          DateFormat = @"dd/MM/yyyy HH:mm",
          TimeSeparator = "/",
          DateSeparator = ":"
        }));
      // 24 + 24 + 7 = 55 hrs
      Assert.AreEqual("055:11", StringConversion.DateTimeToString(
        StringConversion.GetTimeFromTicks(new TimeSpan(2, 7, 11, 0).Ticks), new ValueFormat
        {
          DateFormat = @"HHH:mm",
          TimeSeparator = ":",
          DateSeparator = "."
        }));
    }

    [TestMethod]
    public void IdentifyDatesFormatsUS()
    {
      var dateSep = "/";
      var culture = new CultureInfo("en-US");

      // Try the date formats
      foreach (var fmt in StringConversion.StandardDateTimeFormats.Keys)
      {
        // Fill Samples
        var samples = new HashSet<string>();
        for (var month = 9; month < 10; month++)
          for (var day = 10; day < 15; day++)
            for (var hrs = 11; hrs < 13; hrs++)
              for (var min = 24; min < 26; min++)
                samples.Add(new DateTime(2010, month, day, hrs, min, 10, 16, DateTimeKind.Local).ToString(fmt, culture));

        Assert.IsNotNull(StringConversion.CheckDate(samples, fmt, dateSep, ":", CultureInfo.CurrentCulture).FoundValueFormat,
          $"Test format {fmt}\nFirst not matching: {samples.First()}");
      }
    }

    [TestMethod]
    public void StringToDurationInDays12Hrs()
    {
      Assert.AreEqual(0.5, StringConversion.StringToDurationInDays("12:00", ":", false));
    }

    [TestMethod]
    public void StringToDurationInDays48hrs()
    {
      Assert.AreEqual(2, StringConversion.StringToDurationInDays("48:00", ":", false));
    }

    [TestMethod]
    public void StringToDurationInDaysNull()
    {
      Assert.AreEqual(0, StringConversion.StringToDurationInDays(null, null, false));
    }

    [TestMethod]
    public void StringToGuidInvalid()
    {
      Assert.IsNull(StringConversion.StringToGuid("Test"));
    }

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
      Assert.AreEqual(null, StringConversion.StringToInt64(null, '.', '\0'));
      Assert.AreEqual(17, StringConversion.StringToInt64("17.4", '.', ','));
      Assert.AreEqual(18, StringConversion.StringToInt64("17.6", '.', ','));
      Assert.AreEqual(-18, StringConversion.StringToInt64("-17.6", '.', ','));
      Assert.AreEqual(null,
        StringConversion.StringToInt64("99999999999999999999999999999999999999999999999999", '.', ','));
      Assert.AreEqual(null, StringConversion.StringToInt64("AB", '.', ','));
      Assert.AreEqual(null, StringConversion.StringToInt64("", '.', ','));
    }

    [TestMethod]
    public void StringToTextPartExact()
    {
      Assert.AreEqual(null, StringConversion.StringToTextPart(null, ':', 1, false), "Value null");
      Assert.AreEqual(null, StringConversion.StringToTextPart("Test", ':', 0, false), "Part invalid");
      Assert.AreEqual(null, StringConversion.StringToTextPart("Test:Hallo", ':', 3, false), "Part not present");
      Assert.AreEqual(null, StringConversion.StringToTextPart("Test", ':', 2, false), "Splitter not present, Part 2");
      Assert.AreEqual("Test", StringConversion.StringToTextPart("Test", ':', 1, false), "Splitter not present, Part 1");
      Assert.AreEqual("Test:Hallo", StringConversion.StringToTextPart("Test:Hallo", '|', 1, false),
        "Splitter not contained");
      Assert.AreEqual("Test", StringConversion.StringToTextPart("Test:Hallo", ':', 1, false), "Getting Part 1/2");
      Assert.AreEqual("Hallo", StringConversion.StringToTextPart("Test:Hallo", ':', 2, false), "Getting Part 2/2");
      Assert.AreEqual("Hallo", StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 2, false),
        "Getting Part 2/4");
      Assert.AreEqual("Another", StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 3, false),
        "Getting Part 3/4");
      Assert.AreEqual("Test2", StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 4, false),
        "Getting Part 4/4");
    }

    [TestMethod]
    public void StringToTextPartToEnd()
    {
      Assert.AreEqual("Test:Hallo:Another:Test2",
        StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 1, true), "Getting Part 2-4");
      Assert.AreEqual("Hallo:Another:Test2",
        StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 2, true), "Getting Part 2-4");
      Assert.AreEqual("Another:Test2", StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 3, true),
        "Getting Part 3-4");
      Assert.AreEqual("Test2", StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 4, true),
        "Getting Part 4-4");
      Assert.AreEqual(null, StringConversion.StringToTextPart("Test:Hallo:Another:Test2", ':', 5, true),
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

      // 29/06/1968  15:23:00
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
      var res1 = StringConversion.StringToDateTime("1879-05-01T17:12:00+0000", @"yyyy/MM/ddTHH:mm:sszz00", "-", ":",
        false);
      Assert.IsTrue(res1.HasValue);
      Assert.AreEqual(new DateTime(1879, 05, 01, 19, 12, 00, DateTimeKind.Utc), res1.Value);
    }

    [TestMethod]
    public void TestDateTimezone2()
    {
      var res1 = StringConversion.StringToDateTime("1879-05-01T17:12:00+02", @"yyyy/MM/ddTHH:mm:sszz", "-", ":", false);
      Assert.IsTrue(res1.HasValue);
      var utc = new DateTime(1879, 05, 01, 15, 12, 00, DateTimeKind.Utc);
      Assert.AreEqual(utc, res1.Value.ToUniversalTime());
    }

    [TestMethod]
    public void CheckGuidTest()
    {
      Assert.IsFalse(StringConversion.CheckGuid(null));
      Assert.IsFalse(StringConversion.CheckGuid(new string[] { }));
      Assert.IsTrue(StringConversion.CheckGuid(new[] { "{35C1536A-094A-493D-8FED-545A959E167A}" }));
      Assert.IsFalse(StringConversion.CheckGuid(new[] { "{35C1536A-094A-493D-8FED-545A959E167A}", "A Test" }));
      Assert.IsTrue(StringConversion.CheckGuid(new[]
        {"{35C1536A-094A-493D-8FED-545A959E167A}", "9B6E2B50-5400-4871-820C-591844B4F0D6"}));
    }

    [TestMethod]
    public void CombineStringsToDateTimeExcelTest()
    {
      Assert.IsFalse(StringConversion.CombineObjectsToDateTime(null, null, null, null, true,
        new Column { DateFormat = "yyyyMMdd", DateSeparator = "", TimeSeparator = ":" }, out var _).HasValue);

      Assert.AreEqual(new DateTime(2010, 10, 10),
        StringConversion.CombineObjectsToDateTime(new DateTime(2010, 10, 10),
          null,
          null, null, true, null, out var _).Value);

      Assert.AreEqual(DateTime.FromOADate(0) + new TimeSpan(8, 12, 54),
        StringConversion.CombineObjectsToDateTime(null,
          null,
          new DateTime(new TimeSpan(8, 12, 54).Ticks), null, false, null, out var _).Value);
      Assert.AreEqual(new DateTime(2010, 10, 13),
        StringConversion.CombineObjectsToDateTime(null,
          "2010/10/13",
          null, null, false, new Column { DateFormat = "yyyy/MM/dd", DateSeparator = "/", TimeSeparator = ":" }, out var _).Value);

      Assert.AreEqual(new DateTime(2010, 10, 10, 8, 12, 54),
        StringConversion.CombineObjectsToDateTime(new DateTime(2010, 10, 10),
          null,
          new DateTime(new TimeSpan(8, 12, 54).Ticks), null, true, null, out var _).Value);

      Assert.AreEqual(new DateTime(2010, 10, 10, 8, 12, 54),
        StringConversion.CombineObjectsToDateTime(null, "20101010",
          new DateTime(new TimeSpan(8, 12, 54).Ticks), null, true,
          new Column { DateFormat = "yyyyMMdd", DateSeparator = "", TimeSeparator = ":" }, out var _).Value);

      Assert.AreEqual(new DateTime(2010, 10, 13, 8, 12, 54),
        StringConversion.CombineObjectsToDateTime(null, "2010/10/13",
          new DateTime(new TimeSpan(8, 12, 54).Ticks).ToOADate(), null, true,
          new Column { DateFormat = "yyyy/MM/dd", DateSeparator = "/", TimeSeparator = ":" }, out var _).Value);

      Assert.AreEqual(new DateTime(2010, 10, 13, 8, 12, 54),
        StringConversion.CombineObjectsToDateTime(new DateTime(2010, 10, 13).ToOADate(),
          null,
          new DateTime(new TimeSpan(8, 12, 54).Ticks).ToOADate(), null, true,
          new Column { DateFormat = "yyyy/MM/dd", DateSeparator = "/", TimeSeparator = ":" }, out var _).Value);

      // Pass in a time that is >23:59 to adjust date part
      Assert.AreEqual(new DateTime(2010, 10, 15, 5, 10, 00),
        StringConversion.CombineObjectsToDateTime(null,
          "2010/10/14",
          null, "29:10:00", false,
          new Column { DateFormat = "yyyy/MM/dd", DateSeparator = "/", TimeSeparator = ":", TimePartFormat = "HH:mm:ss" }, out var issues).Value);
      // should issue a warning
      Assert.IsTrue(issues);
    }

    [TestMethod]
    public void CheckNumberTest()
    {
      Assert.IsFalse(StringConversion.CheckNumber(null, '.', '\0', false, false).FoundValueFormat != null);
      Assert.IsFalse(StringConversion.CheckNumber(new string[] { }, '.', '\0', false, false).FoundValueFormat != null);
      Assert.IsTrue(StringConversion.CheckNumber(new[] { "16673" }, '.', '\0', false, false).FoundValueFormat != null);
      Assert.AreEqual(DataType.Integer,
        StringConversion.CheckNumber(new[] { "16673" }, '.', '\0', false, false).FoundValueFormat.DataType);
      Assert.IsFalse(
        StringConversion.CheckNumber(new[] { "16673", "A Test" }, '.', '\0', false, false).FoundValueFormat != null);
      Assert.AreEqual(DataType.Numeric,
        StringConversion.CheckNumber(new[] { "16673", "-23", "1.4" }, '.', '\0', false, false).FoundValueFormat.DataType);
    }

    [TestMethod]
    public void CheckTimeSpanTest()
    {
      Assert.IsTrue(StringConversion.CheckTimeSpan(new[] { "8:20", "10:10", "2:20 pm", "17:30" }, ":", false));
      Assert.IsTrue(StringConversion.CheckTimeSpan(new[] { ".2", ".3", "0.25", "17:30" }, ":", true));
      // 24 hours
      Assert.IsFalse(StringConversion.CheckTimeSpan(new[] { "0.2", "26:20", "0.25", "19:30" }, ":", true));
      Assert.IsFalse(StringConversion.CheckTimeSpan(new[] { "0.2", "2.3", "0.25", "19:30" }, ":", true));
    }

    [TestMethod]
    public void CombineStringsToDateTimeTest()
    {
      Assert.IsNull(StringConversion.CombineStringsToDateTime("20161224", null, "17:24", "", ":", false));
      Assert.AreEqual(new DateTime(2016, 12, 24, 17, 24, 00, 0),
        StringConversion.CombineStringsToDateTime("20161224", "yyyyMMdd", "17:24", "", ":", false));
      Assert.AreEqual(DateTime.FromOADate(.220),
        StringConversion.CombineStringsToDateTime(".220", "yyyyMMdd", "", "", ":", true));
      Assert.AreEqual(DateTime.FromOADate(0).Add(new TimeSpan(17, 24, 00)),
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