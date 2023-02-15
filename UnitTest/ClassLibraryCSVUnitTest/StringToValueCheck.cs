using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CsvTools.Tests
{
#pragma warning disable CS8625, CS8629
  [TestClass]
  public sealed class StringToValueCheck
  {
    [TestMethod]
    public void StringToTextPartExact()
    {
      Assert.AreEqual(string.Empty, ReadOnlySpan<char>.Empty.StringToTextPart(':', 1, false).ToString(), "Value null");
      Assert.AreEqual(string.Empty, "Test".AsSpan().StringToTextPart(':', 0, false).ToString(), "Part invalid");
      Assert.AreEqual(string.Empty, "Test:Hallo".AsSpan().StringToTextPart(':', 3, false).ToString(), "Part not present");
      Assert.AreEqual(string.Empty, "Test".AsSpan().StringToTextPart(':', 2, false).ToString(), "Splitter not present, Part 2");
      Assert.AreEqual("Test", "Test".AsSpan().StringToTextPart(':', 1, false).ToString(), "Splitter not present, Part 1");
      Assert.AreEqual(
        "Test:Hallo",
        "Test:Hallo".AsSpan().StringToTextPart('|', 1, false).ToString(),
        "Splitter not contained");
      Assert.AreEqual("Test", "Test:Hallo".AsSpan().StringToTextPart(':', 1, false).ToString(), "Getting Part 1/2");
      Assert.AreEqual("Hallo", "Test:Hallo".AsSpan().StringToTextPart(':', 2, false).ToString(), "Getting Part 2/2");
      Assert.AreEqual(
        "Hallo",
        "Test:Hallo:Another:Test2".AsSpan().StringToTextPart(':', 2, false).ToString(),
        "Getting Part 2/4");
      Assert.AreEqual(
        "Another",
        "Test:Hallo:Another:Test2".AsSpan().StringToTextPart(':', 3, false).ToString(),
        "Getting Part 3/4");
      Assert.AreEqual(
        "Test2",
        "Test:Hallo:Another:Test2".AsSpan().StringToTextPart(':', 4, false).ToString(),
        "Getting Part 4/4");
    }

    [TestMethod]
    public void StringToTextPartToEnd()
    {

      Assert.AreEqual(
        "Hallo:Another:Test2",
        "Test:Hallo:Another:Test2".AsSpan().StringToTextPart(':', 2, true).ToString(),
        "Getting Part 2-4");

      Assert.AreEqual(
        "Test:Hallo:Another:Test2",
        "Test:Hallo:Another:Test2".AsSpan().StringToTextPart(':', 1, true).ToString(),
        "Getting Part 1-4");


      Assert.AreEqual(
        "Another:Test2",
        "Test:Hallo:Another:Test2".AsSpan().StringToTextPart(':', 3, true).ToString(),
        "Getting Part 3-4");

      Assert.AreEqual(
        "Test2",
        "Test:Hallo:Another:Test2".AsSpan().StringToTextPart(':', 4, true).ToString(),
        "Getting Part 4-4");

      Assert.AreEqual(
        string.Empty,
        "Test:Hallo:Another:Test2".AsSpan().StringToTextPart(':', 5, true).ToString(),
        "Getting Part 5");
    }

    [TestMethod]
    public void StringToTimeSpan_SerialDate()
    {
      // 01:00:00
      var actual = "0.0416666666666667".AsSpan().StringToTimeSpan(':', true);
      Assert.AreEqual(new TimeSpan(1, 0, 0), actual);

      // 02:15:00
      actual = "0.0937500000000000".AsSpan().StringToTimeSpan(':', true);
      Assert.AreEqual(new TimeSpan(2, 15, 0), actual);

      // 29/06/1968 15:23:00
      actual = "0.6409722222".AsSpan().StringToTimeSpan(':', true);
      Assert.AreEqual(new TimeSpan(15, 23, 0), actual);
    }

    [TestMethod]
    public void StringToTimeSpanInvalid()
    {
      Assert.IsNull("Test".AsSpan().StringToTimeSpan(':', false));
      Assert.AreEqual(11, "11:BB:50".AsSpan().StringToTimeSpan(':', false).Value.Hours);
      Assert.IsNull("12".AsSpan().StringToTimeSpan(':', false));
    }

    [TestMethod]
    public void StringToTimeSpanNull()
    {
      Assert.IsNull(StringConversionSpan.StringToTimeSpan(null, char.MinValue, false));
      Assert.IsNull("".AsSpan().StringToTimeSpan(char.MinValue, false));
    }
    [TestMethod]
    public void StringToTimeSpanValid()
    {
      Assert.AreEqual(new TimeSpan(0, 12, 23, 50, 637), "12:23:50.637".AsSpan().StringToTimeSpan(':', false));
      Assert.AreEqual(new TimeSpan(0, 12, 23, 50), "12:23:50".AsSpan().StringToTimeSpan(':', false));
      Assert.AreEqual(new TimeSpan(0, 25, 23, 00), "25:23".AsSpan().StringToTimeSpan(':', false));
      Assert.AreEqual(new TimeSpan(0, 17, 637, 00), "17:637".AsSpan().StringToTimeSpan(':', false));
    }

    [TestMethod]
    public void TestDate1()
    {
      var res1 = "1879-05-01".AsSpan().StringToDateTime(@"yyyy/MM/dd".AsSpan(), '-', ':', false);
      Assert.IsTrue(res1.HasValue);
      Assert.AreEqual(new DateTime(1879, 05, 01), res1.Value);
    }

    [TestMethod]
    public void TestDateTimezone()
    {
      var res1 = "1879-05-01T17:12:00+0000".AsSpan().StringToDateTime(@"yyyy/MM/ddTHH:mm:sszz00".AsSpan(),
        '-',
        ':',
        false);
      Assert.IsTrue(res1.HasValue);
      Assert.AreEqual(new DateTime(1879, 05, 01, 19, 12, 00, DateTimeKind.Utc), res1.Value);
    }

#if Windows
    [TestMethod]
    public void TestDateTimezone2()
    {
      var res1 = StringConversionSpan.StringToDateTime("1879-05-17T17:12:00+02", @"yyyy/MM/ddTHH:mm:sszz", "-", ':', false);
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
      Assert.IsFalse(Array.Empty<string>().Select(x => x.AsMemory()).ToArray().CheckGuid(UnitTestStatic.Token));
      Assert.IsTrue(
        new[] { "{35C1536A-094A-493D-8FED-545A959E167A}" }.Select(x => x.AsMemory()).ToArray().CheckGuid(UnitTestStatic.Token));
      Assert.IsFalse(new[] { "{35C1536A-094A-493D-8FED-545A959E167A}", "A Test" }.Select(x => x.AsMemory()).CheckGuid(
        UnitTestStatic.Token));
      Assert.IsTrue(
        new[] { "{35C1536A-094A-493D-8FED-545A959E167A}", "9B6E2B50-5400-4871-820C-591844B4F0D6" }.Select(x => x.AsMemory()).CheckGuid(UnitTestStatic.Token));
    }

    [TestMethod]
    public void CombineStringsToDateTimeExcelTest()
    {
      Assert.IsFalse(
        StringConversionSpan.CombineObjectsToDateTime(
          null,
          null,
          null,
          null,
          true,
          new ValueFormat(DataTypeEnum.DateTime, dateFormat: "yyyyMMdd", dateSeparator: "", timeSeparator: ":"),
          out _).HasValue);
      Assert.AreEqual(
        new DateTime(2010, 10, 10),
        StringConversionSpan.CombineObjectsToDateTime(new DateTime(2010, 10, 10), null, null, null, true, null, out _)
          .Value);

      Assert.AreEqual(
        DateTime.FromOADate(0) + new TimeSpan(8, 12, 54),
        StringConversionSpan.CombineObjectsToDateTime(
          null,
          null,
          new DateTime(new TimeSpan(8, 12, 54).Ticks),
          null,
          false,
          null,
          out _).Value);
      Assert.AreEqual(
        new DateTime(2010, 10, 13),
        StringConversionSpan.CombineObjectsToDateTime(
          null,
          "2010/10/13".AsSpan(),
          null,
          null,
          false,
          new ValueFormat(DataTypeEnum.DateTime, dateFormat: "yyyy/MM/dd", dateSeparator: "/", timeSeparator: ":"),
          out _).Value);

      Assert.AreEqual(
        new DateTime(2010, 10, 10, 8, 12, 54),
        StringConversionSpan.CombineObjectsToDateTime(
          new DateTime(2010, 10, 10),
          null,
          new DateTime(new TimeSpan(8, 12, 54).Ticks),
          null,
          true,
          null,
          out _).Value);

      Assert.AreEqual(
        new DateTime(2010, 10, 10, 8, 12, 54),
        StringConversionSpan.CombineObjectsToDateTime(
          null,
          "20101010".AsSpan(),
          new DateTime(new TimeSpan(8, 12, 54).Ticks),
          null,
          true,
          new ValueFormat(DataTypeEnum.DateTime, dateFormat: "yyyyMMdd", dateSeparator: "", timeSeparator: ":"),
          out _).Value);

      Assert.AreEqual(
        new DateTime(2010, 10, 13, 8, 12, 54),
        StringConversionSpan.CombineObjectsToDateTime(
          null,
          "2010/10/13".AsSpan(),
          new DateTime(new TimeSpan(8, 12, 54).Ticks).ToOADate(),
          null,
          true,
          new ValueFormat(DataTypeEnum.DateTime, dateFormat: "yyyy/MM/dd", dateSeparator: "/", timeSeparator: ":"),
          out _).Value);

      Assert.AreEqual(
        new DateTime(2010, 10, 13, 8, 12, 54),
        StringConversionSpan.CombineObjectsToDateTime(
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
        StringConversionSpan.CombineObjectsToDateTime(
          null,
          "2010/10/14".AsSpan(),
          null,
          "29:10:00".AsSpan(),
          false,
          new ValueFormat(DataTypeEnum.DateTime, dateFormat: "yyyy/MM/dd", dateSeparator: "/", timeSeparator: ":"),
          out var issues).Value);
      // should issue a warning
      Assert.IsTrue(issues);
    }

    [TestMethod]
    public void CheckNumberTest()
    {
      Assert.IsTrue(new[] { "16673" }.Select(x => x.AsMemory()).ToArray().CheckNumber( '.', char.MinValue, false, false, false, UnitTestStatic.Token)
        .FoundValueFormat != null);
      Assert.AreEqual(
        DataTypeEnum.Integer,
        new[] { "16673" }.Select(x => x.AsMemory()).ToArray().CheckNumber('.', char.MinValue, false, false, false, UnitTestStatic.Token).FoundValueFormat!
          .DataType);
      Assert.IsFalse(
        new[] { "16673", "A Test" }.Select(x => x.AsMemory()).ToArray().CheckNumber('.', char.MinValue, false, false, false, UnitTestStatic.Token)
          .FoundValueFormat != null);
      Assert.AreEqual(
        DataTypeEnum.Numeric,
        new[] { "16673", "-23", "1.4" }.Select(x => x.AsMemory()).ToArray().CheckNumber('.', char.MinValue, false, false, false, UnitTestStatic.Token)
          .FoundValueFormat!
          .DataType);

      Assert.IsFalse(Array.Empty<string>().Select(x => x.AsMemory()).ToArray().CheckNumber('.', char.MinValue, false, false, false, UnitTestStatic.Token)
        .FoundValueFormat != null);
    }


    [TestMethod]
    public void CombineStringsToDateTimeTest()
    {
      Assert.AreEqual(DateTimeConstants.FirstDateTime.AddDays(1), ReadOnlySpan<char>.Empty.CombineStringsToDateTime("HH:mm:ss".AsSpan(), "24:00:00".AsSpan(), 
        char.MinValue, ':', false));

      Assert.IsNull("20161224".AsSpan().CombineStringsToDateTime(null, "17:24".AsSpan(), char.MinValue, ':', false));
      Assert.AreEqual(
        new DateTime(2016, 12, 24, 17, 24, 00, 0),
        "20161224".AsSpan().CombineStringsToDateTime("yyyyMMdd".AsSpan(), "17:24".AsSpan(), char.MinValue, ':', false));
      Assert.AreEqual(
        DateTime.FromOADate(.220),
        ".220".AsSpan().CombineStringsToDateTime("yyyyMMdd".AsSpan(), "".AsSpan(), char.MinValue, ':', true));
      Assert.AreEqual(
        DateTime.FromOADate(0).Add(new TimeSpan(17, 24, 00)),
        "".AsSpan().CombineStringsToDateTime("yyyyMMdd".AsSpan(), "17:24".AsSpan(), char.MinValue, ':', true));
    }

    [TestMethod]
    public void StringToBooleanTest()
    {
      Assert.IsNull(StringConversionSpan.StringToBoolean(null, "y".AsSpan(), "n".AsSpan()));
      Assert.IsTrue("y".AsSpan().StringToBoolean("y".AsSpan(), "n".AsSpan()).Value);
      Assert.IsFalse("n".AsSpan().StringToBoolean("y".AsSpan(), "n".AsSpan()).Value);
    }

    [TestMethod]
    public void StringToGuidInvalid() => Assert.IsNull("Test".AsSpan().StringToGuid());

    [TestMethod]
    public void IdentifyDatesFormatsUs()
    {
      var dateSep = '/';
      var culture = new CultureInfo("en-US");

      var formats = new[] { "dddd, d MMMM, yyyy", "MM/dd/yyyy HH:mm", "MM/dd/yyyy HH:mm", "d/M/yy h:mm tt", "yyyy/MM/dd HH:mm" };
      // Try a fwe date formats
      foreach (var fmt in formats)
      {
        // Fill Samples
        var samples = new HashSet<string>();
        for (var month = 9; month < 10; month++)
          for (var day = 10; day < 15; day++)
            for (var hrs = 11; hrs < 13; hrs++)
              for (var min = 24; min < 26; min++)
                samples.Add(new DateTime(2010, month, day, hrs, min, 10, 876, DateTimeKind.Local).ToString(fmt, culture));

        Assert.IsNotNull(
          samples.Select(x => x.AsMemory()).ToArray().CheckDate(fmt.AsSpan(), dateSep, ':', CultureInfo.CurrentCulture, UnitTestStatic.Token)
            .FoundValueFormat,
          $"Test format {fmt}\nFirst not matching: {samples.First()}");
      }
    }

    [TestMethod]
    public void StringToGuidNull()
    {
      Assert.IsNull(StringConversionSpan.StringToGuid(null));
      Assert.IsNull("".AsSpan().StringToGuid());
    }

    [TestMethod]
    public void StringToGuidValid()
    {
      var test = new Guid(0x23e5ad85, 0x56d9, 0x482e, 0x8f, 0x26, 0x44, 0x1c, 0x4b, 0xbe, 0x89, 0x70);
      Assert.AreEqual(test, "{23E5AD85-56D9-482E-8F26-441C4BBE8970}".AsSpan().StringToGuid());
      Assert.AreEqual(test, "23E5AD85-56D9-482E-8F26-441C4BBE8970".AsSpan().StringToGuid());
    }

    [TestMethod]
    public void StringToInt64()
    {
      Assert.AreEqual(null, StringConversionSpan.StringToInt64(null, '.', char.MinValue));
      Assert.AreEqual(17, "17.4".AsSpan().StringToInt64('.', ','));
      Assert.AreEqual(18, "17.6".AsSpan().StringToInt64('.', ','));
      Assert.AreEqual(-18, "-17.6".AsSpan().StringToInt64('.', ','));
      Assert.AreEqual(
        null,
        "99999999999999999999999999999999999999999999999999".AsSpan().StringToInt64('.', ','));
      Assert.AreEqual(null, "AB".AsSpan().StringToInt64('.', ','));
      Assert.AreEqual(null, "".AsSpan().StringToInt64('.', ','));
    }

    [TestMethod]
    public void CombineStringsToDateTimeExcel()
    {
      var res = StringConversionSpan.CombineObjectsToDateTime(
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
      var res1 = "01/16/2008".AsSpan().StringToDateTimeExact(@"MM/dd/yyyy".AsSpan(),
        '/',
        ':',
        CultureInfo.CurrentCulture);
      Assert.IsTrue(res1.HasValue);
      Assert.AreEqual(16, res1.Value.Day);

      var res2 = "01/16/2008".AsSpan().StringToDateTimeExact(@"MM/dd/yyyy".AsSpan(),
        '/',
        ':',
        CultureInfo.CurrentCulture);
      Assert.IsTrue(res2.HasValue);
      Assert.AreEqual(16, res2.Value.Day);

      var res3 = "01/16/2008 10:25 pm".AsSpan().StringToDateTimeExact(@"MM/dd/yyyy hh:mm tt".AsSpan(),
        '/',
        ':',
        CultureInfo.InvariantCulture);
      Assert.IsTrue(res3.HasValue);
      Assert.AreEqual(16, res3.Value.Day);
      Assert.AreEqual(22, res3.Value.Hour);

      var res4 = @"Freitag, 15. März 2013".AsSpan().StringToDateTimeExact(@"dddd, d. MMMM yyyy".AsSpan(),
        '/',
        ':',
        new CultureInfo("de-DE", false));
      Assert.IsTrue(res4.HasValue);
      Assert.AreEqual(2013, res4.Value.Year);
      Assert.AreEqual(15, res4.Value.Day);
    }

    [TestMethod]
    public void CheckSerialDateFail() =>
      // last value is not a date
      Assert.IsNull(
        new[] { "239324", "239324.344", "4358784" }.Select(x => x.AsMemory()).CheckSerialDate( false, UnitTestStatic.Token)
          .FoundValueFormat);

    [TestMethod]
    public void CheckSerialDateOk() =>
      Assert.IsNotNull(
        new[] { "239324", "239324.344", "235324" }.Select(x => x.AsMemory()).CheckSerialDate(false, UnitTestStatic.Token)
          .FoundValueFormat);


    [TestMethod]
    public void StringToDateTime()
    {
      Assert.AreEqual(new DateTime(2021, 5, 31, 14, 37, 0, 0, DateTimeKind.Unspecified),
        "May 31 2021 2:37PM".AsSpan().StringToDateTime("MMM d yyyy h:mmtt".AsSpan(), '/', ':', false).Value);

      Assert.AreEqual(new DateTime(2020, 10, 8, 16, 04, 0, 0, DateTimeKind.Unspecified),
        "Oct  8 2020  4:04PM".AsSpan().StringToDateTime("MMM d yyyy h:mmtt".AsSpan(), char.MinValue, ':', false).Value);

      Assert.AreEqual(
        new DateTime(2009, 12, 10, 17, 43, 0, 0, DateTimeKind.Unspecified),
        "12/10/2009 17:43".AsSpan().StringToDateTime("MM/dd/yyyy HH:mm".AsSpan(), '/', ':', false).Value);

      Assert.AreEqual(
        new DateTime(2009, 10, 12, 17, 43, 0, 0, DateTimeKind.Unspecified),
        "12/10/2009 17:43".AsSpan().StringToDateTime("dd/MM/yyyy HH:mm".AsSpan(), '/', ':', false).Value);

      Assert.AreEqual(
        new DateTime(2009, 10, 12, 17, 43, 0, 0, DateTimeKind.Unspecified),
        "12/10/2009 17:43 PM".AsSpan().StringToDateTime("dd/MM/yyyy HH:mm tt".AsSpan(), '/', ':', false).Value);
    }


    [TestMethod]
    public void StringToDecimal()
    {
      // allowed grouping
      Assert.AreEqual(634678373m, "634.678.373".AsSpan().StringToDecimal(',', '.', true, true), "634.678.373");
      Assert.AreEqual(634678373.4m, "634.678.373,4".AsSpan().StringToDecimal(',', '.', true, true), "634.678.373,4");
      Assert.AreEqual(6678373.4m, "6.678.373,4".AsSpan().StringToDecimal(',', '.', true, false), "634.678.373,4");

      // Wrong distance between 1st and 2nd grouping
      Assert.IsNull("63.4678.373".AsSpan().StringToDecimal(',', '.', true, true), "63.4678.373");
      // wrong grouping at end
      Assert.IsNull("63.467.8373".AsSpan().StringToDecimal(',', '.', true, true), "63.467.8373");
      Assert.IsNull("63.467.8373,2".AsSpan().StringToDecimal(',', '.', true, true), "63.467.8373,2");

      Assert.AreNotEqual(53m, "5,3".AsSpan().StringToDecimal('.', ',', true, true));
      Assert.AreNotEqual(53m, "5,30".AsSpan().StringToDecimal('.', ',', true, true));
      Assert.AreNotEqual(53m, "5,3000".AsSpan().StringToDecimal('.', ',', true, true));

      Assert.IsNull("".AsSpan().StringToDecimal(',', '.', true, true));
      Assert.AreEqual(5.3m, "5,3".AsSpan().StringToDecimal(',', '.', true, true));



      // Switching grouping and decimal
      Assert.AreEqual(17295.27m, "17,295.27".AsSpan().StringToDecimal('.', ',', true, true));
      Assert.AreEqual(17295.27m, "17.295,27".AsSpan().StringToDecimal(',', '.', true, true));

      // negative Numbers
      Assert.AreEqual(-17m, "-17".AsSpan().StringToDecimal(',', '.', true, true));
      Assert.AreEqual(-17m, "(17)".AsSpan().StringToDecimal(',', '.', true, true));
      // no grouping present but supported
      Assert.AreEqual(53336.7m, "53336,7".AsSpan().StringToDecimal(',', '.', true, false));
      // no grouping present and but supported
      Assert.AreEqual(53336.7m, "53336,7".AsSpan().StringToDecimal(',', '.', true, false));
      Assert.AreEqual(53336.7m, "53336,7".AsSpan().StringToDecimal(',', char.MinValue, true, false));

      Assert.AreEqual(53336.7m, "53336,7".AsSpan().StringToDecimal(',', '.', true, false));
      Assert.AreEqual(52333m, "52.333".AsSpan().StringToDecimal(',', '.', true, false));
      Assert.AreEqual(2.33m, "233%".AsSpan().StringToDecimal(',', '.', true, false));


      Assert.AreEqual(14.56m, "$14.56".AsSpan().StringToDecimal('.', char.MinValue, false, true));
      Assert.AreEqual(-14.56m, "($14.56)".AsSpan().StringToDecimal('.', char.MinValue, false, true));
      Assert.AreEqual(14.56m, "14.56 €".AsSpan().StringToDecimal('.', char.MinValue, false, true));
    }

    [TestMethod]
    public void StringToInt32()
    {
      Assert.AreEqual(-17, "-17".AsSpan().StringToInt32(',', '.'));
      Assert.AreEqual(53337, "53336,7".AsSpan().StringToInt32(',', '.'));
      Assert.AreEqual(52333, "52.333".AsSpan().StringToInt32(',', '.'));
    }

    [TestMethod]
    public void StringToInt16()
    {
      Assert.AreEqual((short) 5333, "5.333".AsSpan().StringToInt16(',', '.'));
      Assert.AreEqual((short) -17, "-17".AsSpan().StringToInt16(',', '.'));
      Assert.AreEqual((short) 5337, "5336,7".AsSpan().StringToInt16(',', '.'));
    }
  }
}