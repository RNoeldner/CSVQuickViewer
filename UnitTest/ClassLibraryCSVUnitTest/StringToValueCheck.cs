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
using System.Globalization;
using System.Linq;

namespace CsvTools.Tests;
#pragma warning disable CS8625, CS8629
[TestClass]
public sealed class StringToValueCheck
{
  [TestMethod]
  public void CheckGuidTest()
  {
    Assert.IsFalse(Array.Empty<string>().Select(x => x.AsMemory()).ToArray().CheckGuid(UnitTestStatic.Token));
    Assert.IsTrue(
      new[] { "{35C1536A-094A-493D-8FED-545A959E167A}" }.Select(x => x.AsMemory()).ToArray().CheckGuid(UnitTestStatic.Token));
    Assert.IsFalse(new[] { "{35C1536A-094A-493D-8FED-545A959E167A}", "A Test" }.Select(x => x.AsMemory()).ToList().CheckGuid(
      UnitTestStatic.Token));
    Assert.IsTrue(
      new[] { "{35C1536A-094A-493D-8FED-545A959E167A}", "9B6E2B50-5400-4871-820C-591844B4F0D6" }.Select(x => x.AsMemory()).ToList().CheckGuid(UnitTestStatic.Token));
  }

  [TestMethod]
  public void CheckNumberTest()
  {
    Assert.IsTrue(new[] { "16673" }.Select(x => x.AsMemory()).ToArray().CheckNumber('.', char.MinValue, false, false, false, UnitTestStatic.Token)
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
  public void CheckSerialDateFail() =>
    // last value is not a date
    Assert.IsNull(
      new[] { "239324", "239324.344", "4358784" }.Select(x => x.AsMemory()).CheckSerialDate(false, UnitTestStatic.Token)
        .FoundValueFormat);

  [TestMethod]
  public void CheckSerialDateOk() =>
    Assert.IsNotNull(
      new[] { "239324", "239324.344", "235324" }.Select(x => x.AsMemory()).CheckSerialDate(false, UnitTestStatic.Token)
        .FoundValueFormat);

  [TestMethod]
  public void CheckStringToDateTimeExact()
  {
    var res1 = "01/16/2008".StringToDateTimeExact(@"MM/dd/yyyy",
      '/', ':', CultureInfo.CurrentCulture);
    Assert.IsTrue(res1.HasValue);
    Assert.AreEqual(16, res1.Value.Day);

    var res2 = "01/16/2008".StringToDateTimeExact(@"MM/dd/yyyy",
      '/', ':',
      CultureInfo.CurrentCulture);
    Assert.IsTrue(res2.HasValue);
    Assert.AreEqual(16, res2.Value.Day);

    var res3 = "01/16/2008 10:25 pm".StringToDateTimeExact(@"MM/dd/yyyy hh:mm tt",
      '/', ':',
      CultureInfo.InvariantCulture);
    Assert.IsTrue(res3.HasValue);
    Assert.AreEqual(16, res3.Value.Day);
    Assert.AreEqual(22, res3.Value.Hour);

    var res4 = @"Freitag, 15. März 2013".StringToDateTimeExact(@"dddd, d. MMMM yyyy",
      '/', ':',
      new CultureInfo("de-DE", false));
    Assert.IsTrue(res4.HasValue);
    Assert.AreEqual(2013, res4.Value.Year);
    Assert.AreEqual(15, res4.Value.Day);
  }

  [TestMethod]
  public void CombineStringsToDateTimeExcel()
  {
    Assert.IsTrue(StringConversionSpan.TryParseCombinedDateTime(
      "yyyy/MM/dd",
      new DateTime(2010, 01, 1),
      ReadOnlySpan<char>.Empty,
      new DateTime(2001, 02, 1, 07, 13, 55, 0),
      ReadOnlySpan<char>.Empty,
      '/', ':', false, out var res,
      out _));
    Assert.AreEqual(new DateTime(2010, 01, 1, 07, 13, 55, 0), res);
  }

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
        samples.Select(x => x.AsMemory()).ToArray().CheckDate(fmt, dateSep, ':', CultureInfo.CurrentCulture, UnitTestStatic.Token)
          .FoundValueFormat,
        $"Test format {fmt}\nFirst not matching: {samples.First()}");
    }
  }

  [TestMethod]
  public void StringToBooleanTest()
  {
    Assert.IsNull(StringConversionSpan.StringToBoolean(null, "y", "n"));
    Assert.IsTrue("y".StringToBoolean("y", "n").Value);
    Assert.IsFalse("n".StringToBoolean("y", "n").Value);
  }

  [TestMethod]
  public void StringToDateTime()
  {
    var res1 = "1879-05-01".StringToDateTime(@"yyyy/MM/dd", '-', ':', false);
    Assert.IsTrue(res1.HasValue);
    Assert.AreEqual(new DateTime(1879, 05, 01), res1.Value);

    Assert.AreEqual(new DateTime(2021, 5, 31, 14, 37, 0, 0, DateTimeKind.Unspecified),
      "May 31 2021 2:37PM".StringToDateTime("MMM d yyyy h:mmtt", '/', ':', false).Value);

    Assert.AreEqual(new DateTime(2020, 10, 8, 16, 04, 0, 0, DateTimeKind.Unspecified),
      "Oct  8 2020  4:04PM".StringToDateTime("MMM d yyyy h:mmtt", char.MinValue, ':', false).Value);

    Assert.AreEqual(
      new DateTime(2009, 12, 10, 17, 43, 0, 0, DateTimeKind.Unspecified),
      "12/10/2009 17:43".StringToDateTime("MM/dd/yyyy HH:mm", '/', ':', false).Value);

    Assert.AreEqual(
      new DateTime(2009, 10, 12, 17, 43, 0, 0, DateTimeKind.Unspecified),
      "12/10/2009 17:43".StringToDateTime("dd/MM/yyyy HH:mm", '/', ':', false).Value);

    Assert.AreEqual(
      new DateTime(2009, 10, 12, 17, 43, 0, 0, DateTimeKind.Unspecified),
      "12/10/2009 17:43 PM".StringToDateTime("dd/MM/yyyy HH:mm tt", '/', ':', false).Value);
  }

  [TestMethod]
  public void StringToDecimal()
  {
    // allowed grouping
    Assert.AreEqual(634678373m, "634.678.373".StringToDecimal(',', '.', true, true), "634.678.373");
    Assert.AreEqual(634678373.4m, "634.678.373,4".StringToDecimal(',', '.', true, true), "634.678.373,4");
    Assert.AreEqual(6678373.4m, "6.678.373,4".StringToDecimal(',', '.', true, false), "634.678.373,4");

    // Wrong distance between 1st and 2nd grouping
    Assert.IsNull("63.4678.373".StringToDecimal(',', '.', true, true), "63.4678.373");
    // wrong grouping at end
    Assert.IsNull("63.467.8373".StringToDecimal(',', '.', true, true), "63.467.8373");
    Assert.IsNull("63.467.8373,2".StringToDecimal(',', '.', true, true), "63.467.8373,2");

    Assert.AreNotEqual(53m, "5,3".StringToDecimal('.', ',', true, true));
    Assert.AreNotEqual(53m, "5,30".StringToDecimal('.', ',', true, true));
    Assert.AreNotEqual(53m, "5,3000".StringToDecimal('.', ',', true, true));

    Assert.IsNull("".StringToDecimal(',', '.', true, true));
    Assert.AreEqual(5.3m, "5,3".StringToDecimal(',', '.', true, true));



    // Switching grouping and decimal
    Assert.AreEqual(17295.27m, "17,295.27".StringToDecimal('.', ',', true, true));
    Assert.AreEqual(17295.27m, "17.295,27".StringToDecimal(',', '.', true, true));

    // negative Numbers
    Assert.AreEqual(-17m, "-17".StringToDecimal(',', '.', true, true));
    Assert.AreEqual(-17m, "(17)".StringToDecimal(',', '.', true, true));
    // no grouping present but supported
    Assert.AreEqual(53336.7m, "53336,7".StringToDecimal(',', '.', true, false));
    // no grouping present and but supported
    Assert.AreEqual(53336.7m, "53336,7".StringToDecimal(',', '.', true, false));
    Assert.AreEqual(53336.7m, "53336,7".StringToDecimal(',', char.MinValue, true, false));

    Assert.AreEqual(53336.7m, "53336,7".StringToDecimal(',', '.', true, false));
    Assert.AreEqual(52333m, "52.333".StringToDecimal(',', '.', true, false));
    Assert.AreEqual(2.33m, "233%".StringToDecimal(',', '.', true, false));


    Assert.AreEqual(14.56m, "$14.56".StringToDecimal('.', char.MinValue, false, true));
    Assert.AreEqual(-14.56m, "($14.56)".StringToDecimal('.', char.MinValue, false, true));
    Assert.AreEqual(14.56m, "14.56 €".StringToDecimal('.', char.MinValue, false, true));
  }

  [TestMethod]
  public void StringToGuidInvalid() => Assert.IsNull("Test".StringToGuid());

  [TestMethod]
  public void StringToGuidNull()
  {
    Assert.IsNull(StringConversionSpan.StringToGuid(null));
    Assert.IsNull("".StringToGuid());
  }

  [TestMethod]
  public void StringToGuidValid()
  {
    var test = new Guid(0x23e5ad85, 0x56d9, 0x482e, 0x8f, 0x26, 0x44, 0x1c, 0x4b, 0xbe, 0x89, 0x70);
    Assert.AreEqual(test, "{23E5AD85-56D9-482E-8F26-441C4BBE8970}".StringToGuid());
    Assert.AreEqual(test, "23E5AD85-56D9-482E-8F26-441C4BBE8970".StringToGuid());
  }

  [TestMethod]
  public void StringToInt16()
  {
    Assert.AreEqual((short) 5333, "5.333".StringToInt16('.'));
    Assert.AreEqual((short) -17, "-17".StringToInt16('.'));
    Assert.AreEqual((short) 5336.7, "5336,7".StringToInt16('.'));
  }

  [TestMethod]
  public void StringToInt32()
  {
    Assert.AreEqual(-17, "-17".StringToInt32('.'));
    Assert.AreEqual(53336, "53336,7".StringToInt32('.'));
    Assert.AreEqual(52333, "52.333".StringToInt32('.'));
  }

  [TestMethod]
  public void StringToInt64()
  {
    Assert.AreEqual(null, StringConversionSpan.StringToInt64(null, '.'));
    Assert.AreEqual(17, "17STUFF".StringToInt64(','));
    Assert.AreEqual(17, "17.999999999999999".StringToInt64(','));
    Assert.AreEqual(7150000, "7,150,000".StringToInt64(','));
    Assert.AreEqual(17, "17.6".StringToInt64(','));
    Assert.AreEqual(-17, "-17.6".StringToInt64(','));
    Assert.AreEqual(null, "99999999999999999999999999999999999999999999999999".StringToInt64(','));
    Assert.AreEqual(null, "AB".StringToInt64(','));
    Assert.AreEqual(null, ".6647".StringToInt64(','));
  }

  [TestMethod]
  public void StringToTextPartExact()
  {
    Assert.AreEqual(string.Empty, ReadOnlySpan<char>.Empty.StringToTextPart(':', 1, false).ToString(), "Value null");
    Assert.AreEqual(string.Empty, "Test".StringToTextPart(':', 0, false).ToString(), "Part invalid");
    Assert.AreEqual(string.Empty, "Test:Hallo".StringToTextPart(':', 3, false).ToString(), "Part not present");
    Assert.AreEqual(string.Empty, "Test".StringToTextPart(':', 2, false).ToString(), "Splitter not present, Part 2");
    Assert.AreEqual("Test", "Test".StringToTextPart(':', 1, false).ToString(), "Splitter not present, Part 1");
    Assert.AreEqual(
      "Test:Hallo",
      "Test:Hallo".StringToTextPart('|', 1, false).ToString(),
      "Splitter not contained");
    Assert.AreEqual("Test", "Test:Hallo".StringToTextPart(':', 1, false).ToString(), "Getting Part 1/2");
    Assert.AreEqual("Hallo", "Test:Hallo".StringToTextPart(':', 2, false).ToString(), "Getting Part 2/2");
    Assert.AreEqual(
      "Hallo",
      "Test:Hallo:Another:Test2".StringToTextPart(':', 2, false).ToString(),
      "Getting Part 2/4");
    Assert.AreEqual(
      "Another",
      "Test:Hallo:Another:Test2".StringToTextPart(':', 3, false).ToString(),
      "Getting Part 3/4");
    Assert.AreEqual(
      "Test2",
      "Test:Hallo:Another:Test2".StringToTextPart(':', 4, false).ToString(),
      "Getting Part 4/4");
  }

  [TestMethod]
  public void StringToTextPartToEnd()
  {

    Assert.AreEqual(
      "Hallo:Another:Test2",
      "Test:Hallo:Another:Test2".StringToTextPart(':', 2, true).ToString(),
      "Getting Part 2-4");

    Assert.AreEqual(
      "Test:Hallo:Another:Test2",
      "Test:Hallo:Another:Test2".StringToTextPart(':', 1, true).ToString(),
      "Getting Part 1-4");

    Assert.AreEqual(
      "Another:Test2",
      "Test:Hallo:Another:Test2".StringToTextPart(':', 3, true).ToString(),
      "Getting Part 3-4");

    Assert.AreEqual(
      "Test2",
      "Test:Hallo:Another:Test2".StringToTextPart(':', 4, true).ToString(),
      "Getting Part 4-4");

    Assert.AreEqual(
      string.Empty,
      "Test:Hallo:Another:Test2".StringToTextPart(':', 5, true).ToString(),
      "Getting Part 5");
  }

  [TestMethod]
  public void StringToTimeSpan_SerialDate()
  {
    // 01:00:00
    var actual = "0.0416666666666667".StringToTimeSpan(':', true);
    Assert.AreEqual(new TimeSpan(1, 0, 0), actual);

    // 02:15:00
    actual = "0.0937500000000000".StringToTimeSpan(':', true);
    Assert.AreEqual(new TimeSpan(2, 15, 0), actual);

    // 29/06/1968 15:23:00
    actual = "0.6409722222".StringToTimeSpan(':', true);
    Assert.AreEqual(new TimeSpan(15, 23, 0), actual);
  }

  [TestMethod]
  public void StringToTimeSpanInvalid()
  {
    Assert.IsNull("Test".StringToTimeSpan(':', false));
    Assert.AreEqual(11, "11:BB:50".StringToTimeSpan(':', false).Value.Hours);
    Assert.IsNull("12".StringToTimeSpan(':', false));
  }

  [TestMethod]
  public void StringToTimeSpanNull()
  {
    Assert.IsNull(StringConversionSpan.StringToTimeSpan(null, char.MinValue, false));
    Assert.IsNull("".StringToTimeSpan(char.MinValue, false));
  }

  [TestMethod]
  public void StringToTimeSpanValid()
  {
    Assert.AreEqual(new TimeSpan(0, 12, 23, 50, 637), "12:23:50.637".StringToTimeSpan(':', false));
    Assert.AreEqual(new TimeSpan(0, 12, 23, 50), "12:23:50".StringToTimeSpan(':', false));
    Assert.AreEqual(new TimeSpan(0, 25, 23, 00), "25:23".StringToTimeSpan(':', false));
    Assert.AreEqual(new TimeSpan(0, 17, 637, 00), "17:637".StringToTimeSpan(':', false));
    Assert.AreEqual(new TimeSpan(0, 324, 637, 00), "324:637".StringToTimeSpan(':', false));
  }

  [TestMethod]
  public void TestDateTimezone()
  {
    var res1 = "1879-05-01T17:12:00+0000".StringToDateTime(@"yyyy/MM/ddTHH:mm:sszz00",
      '-',
      ':',
      false);
    Assert.IsTrue(res1.HasValue);
    Assert.AreEqual(new DateTime(1879, 05, 01, 19, 12, 00, DateTimeKind.Utc), res1.Value);


    var res2 = StringConversionSpan.StringToDateTime("1879-05-17T17:12:00+02", @"yyyy/MM/ddTHH:mm:sszz", '-', ':', false);
    Assert.IsTrue(res2.HasValue);
    var utc = new DateTime(1879, 05, 17, 15, 12, 00, DateTimeKind.Utc);
    // Result of parse on a mac is 17.05.1879 16:06:00 not sure how this comes
    // DateTime.TryParseExact seems to work differently
    Assert.AreEqual(utc, res2.Value.ToUniversalTime());
  }
  
  [TestMethod]
  public void TryParseCombinedDateTime_EdgeCases()
  {
    var twoHundredHours = new TimeSpan(200, 0, 0);


    Assert.IsTrue("yyyy/MM/dd".TryParseCombinedDateTime(
        new DateTime(2010, 1, 1), ReadOnlySpan<char>.Empty, twoHundredHours, ReadOnlySpan<char>.Empty,
        '/', ':', false, out var result, out var issues));

    Assert.IsFalse(issues);
    Assert.AreEqual(new DateTime(2010, 1, 9, 8, 0, 0), result); // 200 hours is 8 days and 8 hours

    Assert.IsTrue("yyyy/MM/dd".TryParseCombinedDateTime(
        null, "2010/01/01", twoHundredHours, ReadOnlySpan<char>.Empty,
        '/', ':', false, out var resultText1, out var issues2));

    Assert.IsFalse(issues2);
    Assert.AreEqual(new DateTime(2010, 1, 9, 8, 0, 0), resultText1); // 200 hours is 8 days and 8 hours
  }

  [DataTestMethod]
  // Scenario: [Format], [DateText], [TimeText]
  [DataRow("", "")]
  [DataRow("2020/17/10", "")]
  [DataRow("InvalidDate", "")]
  [DataRow("", "InvalidTime")]
  public void TryParseCombinedDateTime_InvalidScenarios(string dateText, string timeText)
  {
    bool success = "yyyyMMdd".TryParseCombinedDateTime(
        null, dateText,
        null, timeText,
        '\0', ':',
        true,
        out _, out _);

    // Assert
    Assert.IsFalse(success, "Method should return false for invalid inputs.");
  }

  [DataTestMethod]
  // Format, TypedDate (as string), DateText, TypedTime (as string), TimeText, DateSep, TimeSep, Serial, ExpectedDate
  [DataRow("yyyy/MM/dd", null, "2010-10-10", null, "", '-', ':', true, "2010-10-10 00:00:00")]
  [DataRow("yyyy/MM/dd", null, "2010/10/13", "08:12:54", "", '/', ':', false, "2010-10-13 08:12:54")]
  [DataRow("yyyyMMdd", "2010-10-10", "", null, "", '\0', ':', true, "2010-10-10 00:00:00")]
  [DataRow("yyyyMMdd", "2010-10-10", "", "08:12:54", "", '\0', ':', true, "2010-10-10 08:12:54")]
  [DataRow("yyyyMMdd", null, "", "08:12:54", "", '\0', ':', false, "1899-12-30 08:12:54")]
  [DataRow("yyyyMMdd", null, "20161224", null, "17:24", '\0', ':', false, "2016-12-24 17:24:00")]
  [DataRow("yyyyMMdd", null, ".220", null, "", '\0', ':', true, "1899-12-30 05:16:48")] // .220 days = 5h 16m 48s
  [DataRow("yyyyMMdd", null, "", null, "117:24:00", '\0', ':', true, "1900-01-03 21:24:00")]
  [DataRow("", null, "20161224", null, "17:24", '\0', ':', false, "1899-12-30 17:24:00")] // date can not be parsed but Time is
  public void TryParseCombinedDateTime_ValidScenarios(
    string format, string typedDateStr, string dateText, string typedTimeStr, string timeText,
    char dSep, char tSep, bool serial, string expectedStr)
  {
    // Arrange
    object? typedDate = typedDateStr != null ? DateTime.Parse(typedDateStr, CultureInfo.InvariantCulture) : null;
    object? typedTime = typedTimeStr != null ? TimeSpan.Parse(typedTimeStr, CultureInfo.InvariantCulture) : null;
    DateTime expected = DateTime.Parse(expectedStr, CultureInfo.InvariantCulture);

    // Act
    bool success = format.TryParseCombinedDateTime(
        typedDate, dateText, typedTime, timeText,
        dSep, tSep, serial, out var actual, out _);

    // Assert
    Assert.IsTrue(success);
    Assert.AreEqual(expected, actual);
  }
}