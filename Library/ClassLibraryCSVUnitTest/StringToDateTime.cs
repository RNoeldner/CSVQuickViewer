using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace CsvTools.Tests
{
  [TestClass]
  public class StringToDateTimeTest
  {
    [TestMethod]
    public void CheckTimeConversionNoSeconds()
    {
      _ = new DateTime(2001, 10, 17, 12, 01, 02, 64);
      var dateFormat = "dd.MM.yyyy HH:mm:ss";
      var dateSep = ".";
      var timeSep = ":";
      _ = StringConversion.StringToDateTime("10.08.2010 10:38", dateFormat, dateSep, timeSep, false);
    }

    [TestMethod]
    public void ConvertBackAndForth()
    {
      var dateTime = new DateTime(2001, 10, 17, 12, 01, 02);
      var dateFormat = "dd.MM.yyyy HH:mm:ss";
      var dateSep = ".";
      var timeSep = "-";
      var toString = StringConversion.DateTimeToString(dateTime, dateFormat, dateSep, timeSep);
      var fromString = StringConversion.StringToDateTime(toString, dateFormat, dateSep, timeSep, false);
      Assert.AreEqual(dateTime, fromString, dateFormat);

      dateFormat = @"MM/dd/yyyy HH:mm:ss";
      dateSep = "/";
      timeSep = ":";
      toString = StringConversion.DateTimeToString(dateTime, dateFormat, dateSep, timeSep);
      fromString = StringConversion.StringToDateTime(toString, dateFormat, dateSep, timeSep, false);
      Assert.AreEqual(dateTime, fromString, dateFormat);

      dateFormat = @"yyyyMMdd HH:mm:ss";
      dateSep = "";
      timeSep = ":";
      toString = StringConversion.DateTimeToString(dateTime, dateFormat, dateSep, timeSep);
      fromString = StringConversion.StringToDateTime(toString, dateFormat, dateSep, timeSep, false);
      Assert.AreEqual(dateTime, fromString, dateFormat);
    }

    [TestMethod]
    public void ConvertBackAndForthWithMillisecond()
    {
      var dateTime = new DateTime(2001, 10, 17, 12, 01, 02, 64);

      var dateFormat = "dd.MM.yyyy HH:mm:ss.fff";
      var dateSep = ".";
      var timeSep = "-";
      var toString = StringConversion.DateTimeToString(dateTime, dateFormat, dateSep, timeSep);
      var fromString = StringConversion.StringToDateTime(toString, dateFormat, dateSep, timeSep, false);
      Assert.AreEqual(dateTime, fromString, dateFormat);

      dateFormat = @"MM/dd/yyyy HH:mm:ss.fff";
      dateSep = "/";
      timeSep = ":";
      toString = StringConversion.DateTimeToString(dateTime, dateFormat, dateSep, timeSep);
      fromString = StringConversion.StringToDateTime(toString, dateFormat, dateSep, timeSep, false);
      Assert.AreEqual(dateTime, fromString, dateFormat);

      dateFormat = @"yyyyMMdd HH:mm:ss.fff";
      dateSep = "";
      timeSep = ":";
      toString = StringConversion.DateTimeToString(dateTime, dateFormat, dateSep, timeSep);
      fromString = StringConversion.StringToDateTime(toString, dateFormat, dateSep, timeSep, false);
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
    public void ParseStringToDateTimeFormatNotMatchingseparatorOK() => TestDate(new DateTime(1999, 01, 02), @"yyyyMMdd", ".");

    [TestMethod]
    public void ParseStringToDateTimeFormatNotMatchingseparatorOK2() => Assert.IsNull(StringConversion.StringToDateTime("01-02-1999", @"MM/dd/yyyy", "", "", false));

    [TestMethod]
    public void ParseStringToDateTimeFormatNotMatchingseparatorOK3() => Assert.IsNull(StringConversion.StringToDateTime("01-02-1999", @"MM/dd/yyyy", ".", "", false));

    [TestMethod]
    public void ParseStringToDateTimeFormatNotMatchingseparatorOK4()
    {
      TestDate(new DateTime(1999, 01, 02), @"MM/dd/yyyy", "/");
      TestDate(new DateTime(1999, 01, 02), @"MM.dd.yyyy", ".");
    }

    [TestMethod]
    public void ParseStringToDateTimeSerialDate()
    {
      // 01:00:00
      var actual = StringConversion.StringToDateTime("0.0416666666666667", @"mm\dd\yyyy", @"\", ":", true);
      Assert.AreEqual(new DateTime(1899, 12, 30, 1, 0, 0, 0), actual);

      // 00:00:00
      actual = StringConversion.StringToDateTime("1", @"mm\dd\yyyy", @"\", ":", true);
      Assert.AreEqual(new DateTime(1899, 12, 31, 0, 0, 0, 0), actual);

      // 29/06/1968  15:23:00
      actual = StringConversion.StringToDateTime("25018.6409722222", @"mm\dd\yyyy", @"\", ":", true);
      Assert.AreEqual(new DateTime(1968, 06, 29, 15, 23, 00, 0), actual);

      //22/07/2014  12:50:00
      actual = StringConversion.StringToDateTime("41842.5347222222", @"mm\dd\yyyy", @"\", ":", true);
      Assert.AreEqual(new DateTime(2014, 07, 22, 12, 50, 00, 0), actual);
    }

    [TestMethod]
    public void ParseStringToDateTimeShort()
    {
      // this should work
      var year = DateTime.Now.Year;

      TestDate(new DateTime(year, 01, 02), "d.M.yy", ".", "d/M/yy");
    }

    [TestMethod]
    public void ParseStringToDateTimeDifferentFormats()
    {
      TestDate(new DateTime(1999, 01, 1), @"mm\dd\yyyy", @"\", @"MM/dd/yyyy");
      TestDate(new DateTime(1999, 01, 2), @"MM/dd/yyyy", @"/", @"MM/dd/yyyy");
      TestDate(new DateTime(1999, 01, 3), "dd.mm.yyyy", ".", @"dd/MM/yyyy");
      TestDate(new DateTime(1999, 01, 4), @"dd/mm/yyyy", @"/", @"dd/MM/yyyy");
      TestDate(new DateTime(1999, 01, 5), @"yyyy-mm-dd", @"-", @"yyyy/MM/dd");
      TestDate(new DateTime(1999, 01, 6), @"yyyymmdd", "", @"yyyyMMdd");
      TestDate(new DateTime(1999, 01, 7), @"yyyyddmm", "", @"yyyy/dd/MM");
    }

    [TestMethod]
    [Ignore]
    public void PerformanceTestNew()
    {
      var expected = new DateTime(2010, 10, 17);
      var input = GetFormattedDate(expected, @"MM/dd/yyyy");
      for (var count = 0; count < 100000; count++)
        Assert.AreEqual(expected, StringConversion.StringToDateTime(input, @"MM/dd/yyyy", "/", ":", false));
      for (var count = 0; count < 100000; count++)
        Assert.IsNull(StringConversion.StringToDateTime("Quatsch", @"MM/dd/yyyy", "/", ":", false));
    }

    [TestMethod]
    [Ignore]
    public void PerformanceTestOld()
    {
      var expected = new DateTime(2010, 10, 17);
      var input = GetFormattedDate(expected, @"MM/dd/yyyy");
      for (var count = 0; count < 100000; count++)
        Assert.AreEqual(expected, OldStringToDateTime(input, @"MM/dd/yyyy", "/", ":"));

      for (var count = 0; count < 100000; count++)
        Assert.IsNull(OldStringToDateTime("Quatsch", @"MM/dd/yyyy", "/", ":"));
    }

    [TestMethod]
    public void TestConvertDateToString()
    {
      var dateTime = new DateTime(2001, 10, 17, 12, 01, 02, 00);

      var actual = StringConversion.DateTimeToString(dateTime, @"yyyyMMdd", "", "-");
      Assert.AreEqual("20011017", actual);

      actual = StringConversion.DateTimeToString(dateTime, @"yyyy/MM/dd HH:mm:ss", "/", ":");
      Assert.AreEqual("2001/10/17 12:01:02", actual);

      dateTime = new DateTime(2001, 10, 17, 12, 01, 02, 09);

      actual = StringConversion.DateTimeToString(dateTime, @"yyyyMMdd HH:mm:ss.fff", "", "-");
      Assert.AreEqual("20011017 12-01-02.009", actual);

      actual = StringConversion.DateTimeToString(dateTime, @"yyyy/MM/dd HH:mm:ss.fff", "/", ":");
      Assert.AreEqual("2001/10/17 12:01:02.009", actual);

      // nonsense value for date separator
      actual = StringConversion.DateTimeToString(dateTime, @"yyyy-MM-dd HH:mm:ss.fff", "-", ":");
      Assert.AreEqual("2001-10-17 12:01:02.009", actual);
    }

    [TestMethod]
    public void TryParseGBWithUSDate()
    {
      var expected = new DateTime(1799, 01, 02);
      var dtString = GetFormattedDate(expected, @"MM/dd/yyyy");

      var actual = StringConversion.StringToDateTime(dtString, @"dd/MM/yyyy", "/", ":", false);
      // this should be parsed
      Assert.IsNotNull(actual);
      // but its wrong
      Assert.AreNotEqual(expected, actual);

      expected = new DateTime(1799, 12, 31);
      dtString = GetFormattedDate(expected, @"MM/dd/yyyy");

      actual = StringConversion.StringToDateTime(dtString, @"dd/MM/yyyy", ".", ":", false);
      // this should not be read
      Assert.IsNull(actual);
    }

    [TestMethod]
    public void TryParseGermanWithUSDate()
    {
      {
        var expected = new DateTime(1799, 01, 02);
        var dtString = GetFormattedDate(expected, @"MM/dd/yyyy");

        var actual = StringConversion.StringToDateTime(dtString, @"dd/MM/yyyy", "/", ":", false);

        // if this passes we would expect the month and day to be swapped
        Assert.AreEqual(expected.Year, ((DateTime)actual).Year);
        Assert.AreEqual(expected.Month, ((DateTime)actual).Day);
        Assert.AreEqual(expected.Day, ((DateTime)actual).Month);
      }
    }

    [TestMethod]
    public void TryParseNoSwapDays()
    {
      var actual = StringConversion.StringToDateTime("12/20/1799", @"dd/MM/yyyy", "/", ":", false);

      // It should be null, the 20th can not be a month
      Assert.IsNull(actual);
    }

    /// <summary>
    ///   Parses a string to a date time value
    /// </summary>
    /// <param name="originalValue">The original value.</param>
    /// <param name="dateFormat">The short date format.</param>
    /// <param name="dateSeparator">The date separator.</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <returns></returns>
    private static DateTime? OldStringToDateTime(string originalValue, string dateFormat, string dateSeparator,
      string timeSeparator)
    {
      var stringDateValue = originalValue == null ? string.Empty : originalValue.Trim();
      if (string.IsNullOrEmpty(stringDateValue) || stringDateValue == "00000000" ||
          stringDateValue == "99999999")
      {
        return null;
      }

      DateTime? dateFieldValue = null;
      try
      {
        var dateTimeFormats = StringUtils.SplitByDelimiter(dateFormat);
        /*
                 bool hasTime = (shortDateFormat.IndexOf('H') > -1) || (shortDateFormat.IndexOf('h') > -1);
                 bool hasDate = (shortDateFormat.IndexOf('d') > -1);

                // HH:= hour as a number from 00 through 23;  FFF:= Fractions of a second (if present)
                const string timeFormat = "HH:mm:ss.FFF";
                List<string> dateTimeFormats = new List<string>(4);
                dateTimeFormats.Add(shortDateFormat);
                if (hasDate && !hasTime)
                  dateTimeFormats.Add(shortDateFormat + " " + timeFormat);
                if (hasDate)
                {
                  dateTimeFormats.Add(@"yyyyMMdd");
                  dateTimeFormats.Add(@"yyyyMMdd" + timeFormat);
                }
        */
        var dateTimeFormatInfo = new DateTimeFormatInfo();

        dateTimeFormatInfo.SetAllDateTimePatterns(dateTimeFormats, 'd');
        dateTimeFormatInfo.DateSeparator = dateSeparator;
        dateTimeFormatInfo.TimeSeparator = timeSeparator;

        // Use ParseExact since Parse does not work if a date sepertaor is set but
        // the date separator is not part of the date format
        try
        {
          dateFieldValue = DateTime.ParseExact(stringDateValue, dateTimeFormats, dateTimeFormatInfo,
            DateTimeStyles.AllowWhiteSpaces);
          // Times get the current date added, remove it
          //if ((!hasDate) && (dateFieldValue.Value.Date == DateTime.Now.Date))
          //{
          //  dateFieldValue = DateTime.MinValue.Date.Add(dateFieldValue.Value.TimeOfDay);
          //}
        }
        catch (FormatException)
        {
          if (dateSeparator != "/")
          {
            var dateTimeFormatInfoSlash = new DateTimeFormatInfo();

            // Build a new formatter with / but otherwise the same settings
            dateTimeFormatInfoSlash = (DateTimeFormatInfo)dateTimeFormatInfo.Clone();
            dateTimeFormatInfoSlash.DateSeparator = "/";

            // try with date separator of /
            dateFieldValue = DateTime.ParseExact(stringDateValue, dateTimeFormats, dateTimeFormatInfoSlash,
              DateTimeStyles.AllowWhiteSpaces);
          }
        }
      }
      catch (Exception)
      {
        // ignoring all errors
      }

      return dateFieldValue;
    }

    /// <summary>
    ///   Gets the formatted date.
    /// </summary>
    /// <param name="dateTimeValue">The date.</param>
    /// <param name="shortDateFormat">The short date format.</param>
    /// <returns>The supplied dateTimeValue formatted properly</returns>
    /// <remarks>
    ///   This is a very crude implementation that will ignore errors in
    ///   the shortDateFormat, this is how it should be this way mis formatted strings can be build
    /// </remarks>
    private string GetFormattedDate(DateTime dateValue, string shortDateFormat)
    {
      var strYear = string.Format(CultureInfo.InvariantCulture, "{0:0000}", dateValue.Year);
      var strMonth = string.Format(CultureInfo.InvariantCulture, "{0:00}", dateValue.Month);
      var strDay = string.Format(CultureInfo.InvariantCulture, "{0:00}", dateValue.Day);
      shortDateFormat = shortDateFormat.ToUpperInvariant();
      // Replace longer placeholders
      return shortDateFormat.Replace(@"YYYY", strYear)
        .Replace(@"MM", strMonth)
        .Replace(@"DD", strDay)
        .Replace(@"YY", strYear.Substring(2, 2))
        .Replace(@"M", dateValue.Month.ToString(CultureInfo.InvariantCulture))
        .Replace(@"D", dateValue.Day.ToString(CultureInfo.InvariantCulture));
    }

    private void NotADate(string value)
    {
      var actual = StringConversion.StringToDateTime(value, @"MM/dd/yyyy", @"/", ":", false);
      Assert.IsNull(actual, value + " is not a date");
    }

    /// <summary>
    ///   Tests the date by converting to into the proper format and parsing it again
    /// </summary>
    /// <param name="expected">The date.</param>
    /// <param name="shortDate">The short date format.</param>
    /// <param name="dateSep">The date separator.</param>
    private void TestDate(DateTime expected, string shortDate, string dateSep, string format = null)
    {
      var dtString = GetFormattedDate(expected, shortDate);
      if (format == null)
        format = shortDate;

      var actual = StringConversion.StringToDateTime(dtString, format, dateSep, ":", false);
      Assert.AreEqual(expected, actual);
    }
  }
}