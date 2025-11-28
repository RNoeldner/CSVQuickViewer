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

namespace CsvTools.Tests;

[TestClass]
public class StringToDateTimeTest
{
  [TestMethod]
  public void CheckTimeConversionNoSeconds()
  {
    _ = new DateTime(2001, 10, 17, 12, 01, 02, 64);
    var dateFormat = "dd.MM.yyyy HH:mm:ss";
    var dateSep = '.';
    var timeSep = ':';
    _ = "10.08.2010 10:38".AsSpan().StringToDateTime(dateFormat.AsSpan(), dateSep, timeSep, false);
  }

  [TestMethod]
  public void ConvertBackAndForth()
  {
    var dateTime = new DateTime(2001, 10, 17, 12, 01, 02);
    var dateFormat = "dd.MM.yyyy HH:mm:ss";
    var dateSep = '.';
    var timeSep = '-';
    var toString = StringConversion.DateTimeToString(dateTime, dateFormat, dateSep, timeSep);
    var fromString = toString.AsSpan().StringToDateTime(dateFormat.AsSpan(), dateSep, timeSep, false);
    Assert.AreEqual(dateTime, fromString, dateFormat);

    dateFormat = @"MM/dd/yyyy HH:mm:ss";
    dateSep = '/';
    timeSep = ':';
    toString = StringConversion.DateTimeToString(dateTime, dateFormat, dateSep, timeSep);
    fromString = toString.AsSpan().StringToDateTime(dateFormat.AsSpan(), dateSep, timeSep, false);
    Assert.AreEqual(dateTime, fromString, dateFormat);

    dateFormat = @"yyyyMMdd HH:mm:ss";
    dateSep = Char.MinValue;
    timeSep = ':';
    toString = StringConversion.DateTimeToString(dateTime, dateFormat, dateSep, timeSep);
    fromString = toString.AsSpan().StringToDateTime(dateFormat.AsSpan(), dateSep, timeSep, false);
    Assert.AreEqual(dateTime, fromString, dateFormat);
  }

  [TestMethod]
  public void ConvertBackAndForthWithMillisecond()
  {
    var dateTime = new DateTime(2001, 10, 17, 12, 01, 02, 64);

    var dateFormat = "dd.MM.yyyy HH:mm:ss.fff";
    var dateSep = '.';
    var timeSep = '-';
    var toString = StringConversion.DateTimeToString(dateTime, dateFormat, dateSep, timeSep);
    var fromString = toString.AsSpan().StringToDateTime(dateFormat.AsSpan(), dateSep, timeSep, false);
    Assert.AreEqual(dateTime, fromString, dateFormat);

    dateFormat = @"MM/dd/yyyy HH:mm:ss.fff";
    dateSep = '/';
    timeSep = ':';
    toString = StringConversion.DateTimeToString(dateTime, dateFormat, dateSep, timeSep);
    fromString = toString.AsSpan().StringToDateTime(dateFormat.AsSpan(), dateSep, timeSep, false);
    Assert.AreEqual(dateTime, fromString, dateFormat);

    dateFormat = @"yyyyMMdd HH:mm:ss.fff";
    dateSep = char.MinValue;
    timeSep = ':';
    toString = StringConversion.DateTimeToString(dateTime, dateFormat, dateSep, timeSep);
    fromString = toString.AsSpan().StringToDateTime(dateFormat.AsSpan(), dateSep, timeSep, false);
    Assert.AreEqual(dateTime, fromString, dateFormat);
  }

  [TestMethod]
  public void NonsensDate()
  {
    NotADate("56/17/2668");
    NotADate("56/17/2668 Hello?");
    NotADate("5 Batches");
    NotADate("5 Friends");
    NotADate("");
    NotADate("5");
  }

  [TestMethod]
  public void ParseStringToDateTimeFormatNotMatchingseparatorOk() =>
    TestDate(new DateTime(1999, 01, 02), @"yyyyMMdd", '.', string.Empty);

  [TestMethod]
  public void ParseStringToDateTimeFormatNotMatchingseparatorOk2() =>
    Assert.IsNull("01-02-1999".AsSpan().StringToDateTime(@"MM/dd/yyyy".AsSpan(), char.MinValue, char.MinValue, false));

  [TestMethod]
  public void ParseStringToDateTimeFormatNotMatchingseparatorOk3() =>
    Assert.IsNull("01-02-1999".AsSpan().StringToDateTime(@"MM/dd/yyyy".AsSpan(), '.', char.MinValue, false));

  [TestMethod]
  public void ParseStringToDateTimeFormatNotMatchingseparatorOk4()
  {
    TestDate(new DateTime(1999, 01, 02), @"MM/dd/yyyy", '/', string.Empty);
    TestDate(new DateTime(1999, 01, 02), @"MM.dd.yyyy", '.', string.Empty);
  }

  [TestMethod]
  public void ParseStringToDateTimeSerialDate()
  {
    // 01:00:00
    var actual = "0.0416666666666667".AsSpan().StringToDateTime(@"mm\dd\yyyy".AsSpan(), '\\', ':', true);
    Assert.AreEqual(new DateTime(1899, 12, 30, 1, 0, 0, 0), actual);

    // 00:00:00
    actual = "1".AsSpan().StringToDateTime(@"mm\dd\yyyy".AsSpan(), '\\', ':', true);
    Assert.AreEqual(new DateTime(1899, 12, 31, 0, 0, 0, 0), actual);

    // 29/06/1968 15:23:00
    actual = "25018.6409722222".AsSpan().StringToDateTime(@"mm\dd\yyyy".AsSpan(), '\\', ':', true);
    Assert.AreEqual(new DateTime(1968, 06, 29, 15, 23, 00, 0), actual);

    //22/07/2014  12:50:00
    actual = "41842.5347222222".AsSpan().StringToDateTime(@"mm\dd\yyyy".AsSpan(), '\\', ':', true);
    Assert.AreEqual(new DateTime(2014, 07, 22, 12, 50, 00, 0), actual);
  }

  [TestMethod]
  public void ParseStringToDateTimeShort()
  {
    // this should work
    var year = DateTime.Now.Year;

    TestDate(new DateTime(year, 01, 02), "d.M.yy", '.', "d/M/yy");
  }

  [TestMethod]
  public void ParseStringToDateTimeDifferentFormats()
  {
    TestDate(new DateTime(1999, 01, 7), @"yyyyddmm", char.MinValue, @"yyyy/dd/MM");
    TestDate(new DateTime(1999, 01, 1), @"mm\dd\yyyy", '\\', @"MM/dd/yyyy");
    TestDate(new DateTime(1999, 01, 2), @"MM/dd/yyyy", '/', @"MM/dd/yyyy");
    TestDate(new DateTime(1999, 01, 3), "dd.mm.yyyy", '.', @"dd/MM/yyyy");
    TestDate(new DateTime(1999, 01, 4), @"dd/mm/yyyy", '/', @"dd/MM/yyyy");
    TestDate(new DateTime(1999, 01, 5), @"yyyy-mm-dd", '-', @"yyyy/MM/dd");
    TestDate(new DateTime(1999, 01, 6), @"yyyymmdd", char.MinValue, @"yyyyMMdd");
  }

  /*[TestMethod]
  [Ignore]
  public void PerformanceTestNew()
  {
    var expected = new DateTime(2010, 10, 17);
    var input = GetFormattedDate(expected, @"MM/dd/yyyy");
    for (var count = 0; count < 100000; count++)
      Assert.AreEqual(expected, StringConversion.StringToDateTime(input, @"MM/dd/yyyy", '/', ":", false));
    for (var count = 0; count < 100000; count++)
      Assert.IsNull(StringConversion.StringToDateTime("Quatsch", @"MM/dd/yyyy", '/', ":", false));
  }

  [TestMethod]
  [Ignore]
  public void PerformanceTestOld()
  {
    var expected = new DateTime(2010, 10, 17);
    var input = GetFormattedDate(expected, @"MM/dd/yyyy");
    for (var count = 0; count < 100000; count++)
      Assert.AreEqual(expected, OldStringToDateTime(input, @"MM/dd/yyyy", '/', ":"));

    for (var count = 0; count < 100000; count++)
      Assert.IsNull(OldStringToDateTime("Quatsch", @"MM/dd/yyyy", '/', ":"));
  }*/

  [TestMethod]
  public void TestConvertDateToString()
  {
    var dateTime = new DateTime(2001, 10, 17, 12, 01, 02, 00);

    var actual = StringConversion.DateTimeToString(dateTime, @"yyyyMMdd", char.MinValue, '-');
    Assert.AreEqual("20011017", actual);

    actual = StringConversion.DateTimeToString(dateTime, @"yyyy/MM/dd HH:mm:ss", '/', ':');
    Assert.AreEqual("2001/10/17 12:01:02", actual);

    dateTime = new DateTime(2001, 10, 17, 12, 01, 02, 09);

    actual = StringConversion.DateTimeToString(dateTime, @"yyyyMMdd HH:mm:ss.fff", char.MinValue, '-');
    Assert.AreEqual("20011017 12-01-02.009", actual);

    actual = StringConversion.DateTimeToString(dateTime, @"yyyy/MM/dd HH:mm:ss.fff", '/', ':');
    Assert.AreEqual("2001/10/17 12:01:02.009", actual);

    // nonsense value for date separator
    actual = StringConversion.DateTimeToString(dateTime, @"yyyy-MM-dd HH:mm:ss.fff", '-', ':');
    Assert.AreEqual("2001-10-17 12:01:02.009", actual);
  }

  [TestMethod]
  public void TryParseGbWithUsDate()
  {
    var expected = new DateTime(1799, 01, 02);
    var dtString = GetFormattedDate(expected, @"MM/dd/yyyy");

    var actual = dtString.AsSpan().StringToDateTime(@"dd/MM/yyyy".AsSpan(), '/', ':', false);
    // this should be parsed
    Assert.IsNotNull(actual);
    // but its wrong
    Assert.AreNotEqual(expected, actual);

    expected = new DateTime(1799, 12, 31);
    dtString = GetFormattedDate(expected, @"MM/dd/yyyy");

    actual = dtString.AsSpan().StringToDateTime(@"dd/MM/yyyy".AsSpan(), '.', ':', false);
    // this should not be read
    Assert.IsNull(actual);
  }

  [TestMethod]
  public void TryParseGermanWithUsDate()
  {
    {
      var expected = new DateTime(1799, 01, 02);
      var dtString = GetFormattedDate(expected, @"MM/dd/yyyy");

      var actual = dtString.AsSpan().StringToDateTime(@"dd/MM/yyyy".AsSpan(), '/', ':', false);
      Assert.IsNotNull(actual);
      // if this passes we would expect the month and day to be swapped
      Assert.AreEqual(expected.Year, ((DateTime) actual).Year);
      Assert.AreEqual(expected.Month, ((DateTime) actual).Day);
      Assert.AreEqual(expected.Day, ((DateTime) actual).Month);
    }
  }

  [TestMethod]
  public void TryParseNoSwapDays()
  {
    var actual = "12/20/1799".AsSpan().StringToDateTime(@"dd/MM/yyyy".AsSpan(), '/', ':', false);

    // It should be null, the 20th can not be a month
    Assert.IsNull(actual);
  }

  /// <summary>
  ///   Gets the formatted date.
  /// </summary>
  /// <param name="dateValue">A Date Time</param>
  /// <param name="shortDateFormat">The short date format.</param>
  /// <returns>The supplied dateTimeValue formatted properly</returns>
  /// <remarks>
  ///   This is a very crude implementation that will ignore errors in the shortDateFormat, this
  ///   is how it should be this way mis formatted strings can be build
  /// </remarks>
  private static string GetFormattedDate(DateTime dateValue, string shortDateFormat)
  {
    var strYear = string.Format(CultureInfo.InvariantCulture, "{0:0000}", dateValue.Year);
    var strMonth = string.Format(CultureInfo.InvariantCulture, "{0:00}", dateValue.Month);
    var strDay = string.Format(CultureInfo.InvariantCulture, "{0:00}", dateValue.Day);
    shortDateFormat = shortDateFormat.ToUpperInvariant();
    // Overwrite longer placeholders
    return shortDateFormat.Replace(@"YYYY", strYear)
      .Replace(@"MM", strMonth)
      .Replace(@"DD", strDay)
      .Replace(@"YY", strYear.Substring(2, 2))
      .Replace(@"M", dateValue.Month.ToString(CultureInfo.InvariantCulture))
      .Replace(@"D", dateValue.Day.ToString(CultureInfo.InvariantCulture));
  }

  private void NotADate(string value)
  {
    var actual = value.AsSpan().StringToDateTime(@"MM/dd/yyyy".AsSpan(), '/', ':', false);
    Assert.IsNull(actual, value + " is not a date");
  }

  /// <summary>
  ///   Tests the date by converting to into the proper format and parsing it again
  /// </summary>
  /// <param name="expected">The date.</param>
  /// <param name="shortDate">The short date format.</param>
  /// <param name="dateSep">The date separator.</param>
  /// <param name="format"></param>
  private static void TestDate(DateTime expected, string shortDate, char dateSep, string format)
  {
    var dtString = GetFormattedDate(expected, shortDate);
    if (format is null || format.Length == 0)
      format = shortDate;

    var actual = dtString.AsSpan().StringToDateTime(format.AsSpan(), dateSep, ':', false);
    Assert.AreEqual(expected, actual);
  }
}