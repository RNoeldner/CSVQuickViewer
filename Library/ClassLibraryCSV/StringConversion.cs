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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace CsvTools
{
  /// <summary>
  ///   Collection of static functions for string in regards to type conversions
  /// </summary>
  public static class StringConversion
  {
    /// <summary>
    ///   The possible length of a date for a given format
    /// </summary>
    public static readonly DateTimeFormatCollection StandardDateTimeFormats =
      new DateTimeFormatCollection("DateTimeFormats.txt");

#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static HashSet<string> DateSeparators =
          new HashSet<string>(new[] { CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, "/", ".", "-" });

    public static HashSet<char> DecimalGroupings = new HashSet<char>(new[]
          {'\0', CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator[0], '.', ',', ' '});

    public static HashSet<char> DecimalSeparators = new HashSet<char>(new[]
          {CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0], '.', ','});
#pragma warning restore CA2211 // Non-constant fields should not be visible
    private static readonly string[] m_FalseValues =
        {
      "False", "No", "n", "F", "Non", "Nein", "Falsch", "無", "无",
      "假", "없음", "거짓", "ไม่ใช่", "เท็จ", "नहीं", "झूठी", "نہيں", "نه", "نادرست", "لا", "كاذبة",
      "جھوٹا", "שווא", "לא", "いいえ", "Фалшиви", "Ні", "Нет", "Не", "ЛОЖЬ", "Ψευδείς", "Όχι", "Yanlış",
      "Viltus", "Valse", "Vale", "Väärä", "Tidak", "Sai", "Palsu", "nu", "Nr", "nie", "NEPRAVDA", "nem",
      "Nej", "nei", "nē", "Ne", "Não", "na", "le", "Klaidingas", "Không", "inactive", "Hayır", "Hamis",
      "Foloz", "Ffug", "Faux", "Fałszywe", "Falso", "Falske", "Falska", "Falsk", "Fals", "Falošné", "Ei"
    };

    private static readonly DateTime m_FirstDateTimeNextDay = new DateTime(1899, 12, 30, 0, 0, 0, 0).AddDays(1);

    private static readonly string[] m_TrueValues =
        {
      "True", "yes", "y", "t", "Wahr", "Sì", "Si", "Ja",
      "active", "Правда", "Да", "Вярно", "Vero", "Veritable", "Vera", "Jah", "igen", "真實", "真实",
      "真", "是啊", "예", "사실", "อย่างแท้จริง", "ใช่", "हाँ", "सच", "نعم", "صحيح",
      "سچا", "درست است", "جی ہاں", "بله", "נכון", "כן", "はい", "Так", "Ναι", "Αλήθεια", "Ya",
      "Wir", "Waar", "Vrai", "Verdadero", "Verdade", "Totta", "Tõsi", "Tiesa", "Tak", "taip", "Sim",
      "Sí", "Sant", "Sanna", "Sandt", "Res", "Prawdziwe", "Pravda", "Patiess", "Oui", "Kyllä",
      "jā", "Iva", "Igaz", "Ie", "Gerçek", "Evet", "Đúng", "da", "Có", "Benar",
      "áno", "Ano", "Adevărat"
    };

    /// <summary>
    ///   A static value any time only value will have this date
    /// </summary>
    private static readonly DateTime m_FirstDateTime = new DateTime(1899, 12, 30, 0, 0, 0, 0);

    /// <summary>Checks if the values are dates.</summary>
    /// <param name="samples">The sample values to be checked.</param>
    /// <param name="shortDateFormat">The short date format.</param>
    /// <param name="dateSeparator">The date separator.</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <param name="culture">the culture to check (important for named Days or month)</param>
    /// <returns>
    ///   <c>true</c> if all values can be interpreted as date, <c>false</c> otherwise.</returns>
    public static CheckResult CheckDate(ICollection<string> samples, string shortDateFormat, string dateSeparator,
      string timeSeparator, CultureInfo culture)
    {
      var checkResult = new CheckResult();
      if (samples.IsEmpty())
        return checkResult;
      var allParsed = true;
      var counter = 0;
      var positiveMatches = 0;
      foreach (var value in samples)
      {
        counter++;
        var ret = StringToDateTimeExact(value, shortDateFormat, dateSeparator, timeSeparator, culture);
        if (!ret.HasValue)
        {
          allParsed = false;
          checkResult.ExampleNonMatch.Add(value);
          // try to get some positive matches, in case the first record is invalid
          if (counter >= 3)
          {
            if (checkResult.ValueFormatPossibleMatch == null)
            {
              checkResult.ValueFormatPossibleMatch = new ValueFormat
              {
                DataType = DataType.DateTime,
                DateFormat = shortDateFormat,
                DateSeparator = dateSeparator,
                TimeSeparator = timeSeparator
              };
            }
            break;
          }
        }
        else
        {
          positiveMatches++;
          if (positiveMatches > 5 && !checkResult.PossibleMatch)
          {
            checkResult.PossibleMatch = true;
            checkResult.ValueFormatPossibleMatch = new ValueFormat
            {
              DataType = DataType.DateTime,
              DateFormat = shortDateFormat,
              DateSeparator = dateSeparator,
              TimeSeparator = timeSeparator
            };
          }
        }
      }

      if (!allParsed) return checkResult;
      checkResult.FoundValueFormat = new ValueFormat
      {
        DataType = DataType.DateTime,
        DateFormat = shortDateFormat,
        DateSeparator = dateSeparator,
        TimeSeparator = timeSeparator
      };

      return checkResult;
    }

    /// <summary>
    ///   Checks if the values are GUIDs
    /// </summary>
    /// <param name="samples">The sample values to be checked.</param>
    /// <returns>
    ///   <c>true</c> if all values can be interpreted as Guid, <c>false</c> otherwise.
    /// </returns>
    public static bool CheckGuid(ICollection<string> samples)
    {
      if (samples.IsEmpty())
        return false;
      foreach (var value in samples)
      {
        var ret = StringToGuid(value);
        if (!ret.HasValue)
          return false;
      }

      return true;
    }

    /// <summary>
    ///   Checks if the values are numbers
    /// </summary>
    /// <param name="samples">The sample values to be checked.</param>
    /// <param name="decimalSeparator">The decimal separator.</param>
    /// <param name="thousandSeparator">The thousand separator.</param>
    /// <param name="allowPercentage">Allows Percentages</param>
    /// <param name="allowStartingZero">if set to <c>true</c> [allow starting zero].</param>
    /// <returns>
    ///   <c>true</c> if all values can be interpreted as numbers, <c>false</c> otherwise.
    /// </returns>
    public static CheckResult CheckNumber(ICollection<string> samples, char decimalSeparator, char thousandSeparator,
      bool allowPercentage, bool allowStartingZero)
    {
      var checkResult = new CheckResult();
      if (samples.IsEmpty())
        return checkResult;
      var allParsed = true;
      var assumeInteger = true;
      var counter = 0;
      var positiveMatches = 0;

      foreach (var value in samples)
      {
        counter++;
        var ret = StringToDecimal(value, decimalSeparator, thousandSeparator, allowPercentage);
        // Any number with leading 0 should not be treated as numeric this is to avoid problems with
        // 0002 etc.
        if (!ret.HasValue || !allowStartingZero && value.StartsWith("0", StringComparison.Ordinal) &&
            Math.Floor(ret.Value) != 0)
        {
          allParsed = false;
          checkResult.ExampleNonMatch.Add(value);
          // try to get some positive matches, in case the first record is invalid
          if (counter >= 3)
            break;
        }
        else
        {
          positiveMatches++;
          if (positiveMatches > 5 && !checkResult.PossibleMatch)
          {
            checkResult.PossibleMatch = true;
            checkResult.ValueFormatPossibleMatch = new ValueFormat
            {
              DataType = assumeInteger ? DataType.Integer : DataType.Numeric,
              DecimalSeparator = decimalSeparator.ToString(System.Globalization.CultureInfo.CurrentCulture),
              GroupSeparator = thousandSeparator.ToString(System.Globalization.CultureInfo.CurrentCulture)
            };
          }
          // if the value contains the decimal separator or is too large to be an integer, its not
          // an integer
          if (value.IndexOf(decimalSeparator) != -1)
          {
            assumeInteger = false;
          }
          else
          {
            assumeInteger = assumeInteger && ret.Value == Math.Truncate(ret.Value) && ret.Value <= int.MaxValue &&
                            ret.Value >= int.MinValue;
          }
        }
      }

      if (!allParsed) return checkResult;
      checkResult.FoundValueFormat = new ValueFormat
      {
        DataType = assumeInteger ? DataType.Integer : DataType.Numeric,
        DecimalSeparator = decimalSeparator.ToString(System.Globalization.CultureInfo.CurrentCulture),
        GroupSeparator = thousandSeparator.ToString(System.Globalization.CultureInfo.CurrentCulture)
      };

      return checkResult;
    }

    /// <summary>
    ///   Checks if the values are times or serial dates
    /// </summary>
    /// <param name="samples">The sample values to be checked.</param>
    /// <param name="isCloseToNow">
    ///   Only assume the number is a serial date if the resulting date is around the current date
    ///   (-80 +20 years)
    /// </param>
    /// <returns>
    ///   <c>true</c> if all values can be interpreted as date, <c>false</c> otherwise.
    /// </returns>
    public static CheckResult CheckSerialDate(ICollection<string> samples, bool isCloseToNow)
    {
      var checkResult = new CheckResult();
      if (samples.IsEmpty())
        return checkResult;
      var allParsed = true;
      var positiveMatches = 0;
      var counter = 0;

      foreach (var value in samples)
      {
        counter++;
        var ret = SerialStringToDateTime(value);
        if (!ret.HasValue)
        {
          allParsed = false;
          checkResult.ExampleNonMatch.Add(value);
          // try to get some positive matches, in case the first record is invalid
          if (counter >= 3)
            break;
        }
        else
        {
          if (isCloseToNow && (ret.Value.Year < DateTime.Now.Year - 80 || ret.Value.Year > DateTime.Now.Year + 20))
          {
            allParsed = false;
            checkResult.ExampleNonMatch.Add(value);
            // try to get some positive matches, in case the first record is invalid
            if (counter > 3)
              break;
          }
          else
          {
            positiveMatches++;
            if (positiveMatches > 5 && !checkResult.PossibleMatch)
            {
              checkResult.PossibleMatch = true;
              checkResult.ValueFormatPossibleMatch = new ValueFormat
              {
                DataType = DataType.DateTime,
                DateFormat = "SerialDate"
              };
            }
          }
        }
      }

      if (!allParsed) return checkResult;
      checkResult.FoundValueFormat = new ValueFormat
      {
        DataType = DataType.DateTime,
        DateFormat = "SerialDate"
      };

      return checkResult;
    }

    /// <summary>
    ///   Checks if the values are TimeSpans
    /// </summary>
    /// <param name="samples">The sample values to be checked.</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <returns>
    ///   <c>true</c> if all values can be interpreted as time, <c>false</c> otherwise.
    /// </returns>
    public static bool CheckTime(ICollection<string> samples, string timeSeparator)
    {
      if (samples.IsEmpty())
        return false;

      var allParsed = true;
      foreach (var value in samples)
      {
        var ret = StringToTimeSpan(value, timeSeparator, false);
        if (ret.HasValue) continue;
        allParsed = false;
        break;
      }

      return allParsed;
    }

    /// <summary>
    ///   Checks if the values are times or serial dates
    /// </summary>
    /// <param name="samples">The sample values to be checked.</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <param name="serialDateTime">Allow Date Time values in serial format</param>
    /// <returns>
    ///   <c>true</c> if all values can be interpreted as date, <c>false</c> otherwise.
    /// </returns>
    public static bool CheckTimeSpan(ICollection<string> samples, string timeSeparator, bool serialDateTime)
    {
      if (samples.IsEmpty())
        return false;
      var allParsed = true;
      var hasValue = false;
      foreach (var value in samples)
      {
        var ret = StringToTimeSpan(value, timeSeparator, serialDateTime);
        if (!ret.HasValue || ret.Value.TotalHours >= 24.0)
        {
          allParsed = false;
          break;
        }

        hasValue |= ret.Value.TotalSeconds > 0;
      }

      if (hasValue) return allParsed;
      return false;
    }

    /// <summary>
    /// Combine date / time for excel, the individual values could already be typed.
    /// </summary>
    /// <param name="dateColumn">The date column entry</param>
    /// <param name="dateColumnText">The date column text.</param>
    /// <param name="timeColumn">The time column entry</param>
    /// <param name="timeColumnText">The time column text.</param>
    /// <param name="serialDateTime">if set to <c>true</c> [serial date time].</param>
    /// <param name="df">The value format for the date column.</param>
    /// <param name="timeColunIssues">if set to <c>true</c> if the time value is outside range 00:00 - 23:59.</param>
    /// <returns></returns>
    public static DateTime? CombineObjectsToDateTime(object dateColumn, string dateColumnText, object timeColumn,
      string timeColumnText, bool serialDateTime, Column df, out bool timeColunIssues)
    {
      var dateValue = m_FirstDateTime;
      // We do have an associated column, with a proper date format
      if (dateColumn != null)
      {
        if (dateColumn is DateTime time)
        {
          dateValue = time;
        }
        else if (serialDateTime && dateColumn is double)
        {
          var oaDate = (double)dateColumn;
          if (oaDate > -657435.0 && oaDate < 2958466.0)
            dateValue = m_FirstDateTime.AddDays(oaDate);
        }
      }

      // if we did not convert yet and we have a text use it
      if (dateValue == m_FirstDateTime && !string.IsNullOrEmpty(dateColumnText))
      {
        var val = CombineStringsToDateTime(dateColumnText, df.DateFormat, null, df.DateSeparator, df.TimeSeparator,
          serialDateTime);
        if (val.HasValue)
          dateValue = val.Value;
      }

      TimeSpan? timeSpanValue = null;
      if (timeColumn != null)
      {
        if (timeColumn is double oaDate && serialDateTime)
        {
          if (oaDate > -657435.0 && oaDate < 2958466.0)
          {
            var timeValue = m_FirstDateTime.AddDays(oaDate);
            timeSpanValue = new TimeSpan(0, timeValue.Hour, timeValue.Minute, timeValue.Second, timeValue.Millisecond);
          }
        }
        else if (timeColumn is DateTime timeValue)
        {
          timeSpanValue = new TimeSpan(0, timeValue.Hour, timeValue.Minute, timeValue.Second, timeValue.Millisecond);
        }
        else if (timeColumn is TimeSpan)
        {
          timeSpanValue = (TimeSpan)timeColumn;
        }
      }

      if (!timeSpanValue.HasValue)
      {
        timeSpanValue = StringToTimeSpan(timeColumnText, ":", serialDateTime);
      }

      if (timeSpanValue.HasValue)
      {
        timeColunIssues = timeSpanValue.Value.TotalDays >= 1d || timeSpanValue.Value.TotalSeconds < 0d;
        return dateValue.Add(timeSpanValue.Value);
      }
      else
      {
        timeColunIssues = false;
      }

      // It could be that the dateValue is indeed m_FirstDateTime, but only if the text matches the proper formated value
      if (dateValue == m_FirstDateTime && dateColumn == null &&
          (string.IsNullOrEmpty(dateColumnText) || !dateColumnText.Equals(DateTimeToString(m_FirstDateTime, df.ValueFormat), StringComparison.Ordinal)))
      {
        return null;
      }

      return dateValue;
    }

    /// <summary>
    ///   Combines two strings to one date time.
    /// </summary>
    /// <param name="datePart">The date part.</param>
    /// <param name="dateFormat">The date format.</param>
    /// <param name="timePart">The time part.</param>
    /// <param name="dateSeparator">The date separator.</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <param name="serialDateTime">Allow Date Time values ion serial format</param>
    /// <returns></returns>
    public static DateTime? CombineStringsToDateTime(string datePart, string dateFormat, string timePart,
      string dateSeparator, string timeSeparator, bool serialDateTime)
    {
      //DateTime? date = null;
      //TimeSpan? time;
      if (string.IsNullOrEmpty(dateFormat))
        return null;

      var date = StringToDateTime(datePart, dateFormat, dateSeparator, timeSeparator, serialDateTime);

      // In case a value is read that just is a time, need to adjust c# and Excel behavior
      // the application assumes all dates on cFirstDatetime is a time only
      if (date.HasValue && date.Value.Year == 1 && date.Value.Month == 1 && string.IsNullOrWhiteSpace(timePart))
        return GetTimeFromTicks(date.Value.Ticks);

      var time = StringToTimeSpan(timePart, timeSeparator, serialDateTime);

      if (time.HasValue && date.HasValue)
        // this can be problematic if both are in fact times...
        return date.Value.Add(time.Value);
      if (time.HasValue)
        return m_FirstDateTime.Add(time.Value);
      return date;
    }

    /// <summary>
    ///   Check if the length of the provided string could fit to the date format
    /// </summary>
    /// <param name="length">The length.</param>
    /// <param name="dateFormat">The date format.</param>
    /// <returns>
    ///   <c>true</c> if this could possibly be correct, <c>false</c> if the text is too short or too long
    /// </returns>
    public static bool DateLengthMatches(int length, string dateFormat)
    {
      Contract.Requires(!string.IsNullOrEmpty(dateFormat));

      // Either the format is known then use the determined length restrictions
      if (StandardDateTimeFormats.TryGetValue(dateFormat, out var lengthMinMax))
        return length >= lengthMinMax.MinLength && length <= lengthMinMax.MaxLength;
      else
        return true;
    }

    /// <summary>
    ///   Converts a dates to string.
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <param name="format">The <see cref="ValueFormat" />.</param>
    /// <returns>Formatted value</returns>
    public static string DateTimeToString(DateTime dateTime, ValueFormat format)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (string.IsNullOrEmpty(format?.DateFormat))
        // Get the default format
        format = new ValueFormat();

      if (!format.DateFormat.Contains("HHH"))
      {
        return dateTime.ToString(format.DateFormat, CultureInfo.InvariantCulture)
          .ReplaceDefaults("/", format.DateSeparator, ":", format.TimeSeparator);
      }

      var pad = 2;

      // only allow format that has time values
      const string allowed = " Hhmsf:";
      var result = string.Empty;
      foreach (var chr in format.DateFormat)
      {
        if (allowed.IndexOf(chr) != -1)
          result += chr;
      }
      // make them all upper case H lower case does not make sense
      result = result.Trim().RReplace("h", "H");
      for (var length = 5; length > 2; length--)
      {
        var search = new string('H', length);
        if (!result.Contains(search)) continue;
        result = result.Replace(search, "HH");
        pad = length;
        break;
      }

      var strFrmt = "{0:" + new string('0', pad) + "}";
      return dateTime.ToString(
        result.Replace("HH",
          string.Format(CultureInfo.CurrentCulture, strFrmt, Math.Floor((dateTime - m_FirstDateTime).TotalHours))),
        CultureInfo.InvariantCulture).ReplaceDefaults("/", format.DateSeparator,
        ":", format.TimeSeparator);
    }

    /// <summary>
    ///   Converts a dates to string.
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <param name="format">The format.</param>
    /// <param name="dateSeparator">The date separator.</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <returns>Formatted value</returns>
    public static string DateTimeToString(DateTime dateTime, string format, string dateSeparator, string timeSeparator)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      return DateTimeToString(dateTime, new ValueFormat
      {
        DateFormat = format,
        DateSeparator = dateSeparator,
        TimeSeparator = timeSeparator
      });
    }

    /// <summary>
    ///   Converts a decimals to string.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The <see cref="ValueFormat" />.</param>
    /// <returns>Formatted value</returns>
    public static string DecimalToString(decimal value, ValueFormat format)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (string.IsNullOrEmpty(format?.NumberFormat))
        // Get the default format
        format = new ValueFormat();

      return value.ToString(format.NumberFormat, CultureInfo.InvariantCulture).ReplaceDefaults(
        CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, format.DecimalSeparator,
        CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator, format.GroupSeparator);
    }

    /// <summary>
    ///   Displays the date time in local format
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <param name="culture">The culture.</param>
    /// <returns></returns>
    public static string DisplayDateTime(DateTime dateTime, CultureInfo culture)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      // if we only have a time:
      if (IsTimeOnly(dateTime))
        return dateTime.ToString("T", culture);

      if (dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0)
        return dateTime.ToString("d", culture);

      if (IsDuration(dateTime))
      {
        return (dateTime - m_FirstDateTime).TotalHours.ToString((dateTime - m_FirstDateTime).TotalHours >= 100 ? "000" : "00", CultureInfo.InvariantCulture)
              + ":" + (dateTime - m_FirstDateTime).Minutes.ToString("00", CultureInfo.InvariantCulture)
              + ":" + (dateTime - m_FirstDateTime).Seconds.ToString("00", CultureInfo.InvariantCulture);
      }
      return dateTime.ToString("G", culture);
    }

    /// <summary>
    ///   Converts a doubles to string.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The <see cref="ValueFormat" />.</param>
    /// <returns>Formatted value</returns>
    public static string DoubleToString(double value, ValueFormat format)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      if (string.IsNullOrEmpty(format?.NumberFormat))
        // Get the default format
        format = new ValueFormat();

      return value.ToString(format.NumberFormat, CultureInfo.InvariantCulture).ReplaceDefaults(
        CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, format.DecimalSeparator,
        CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator, format.GroupSeparator);
    }

    /// <summary>
    ///   Formats a file size in KiloBytes or MegaBytes depending on size
    /// </summary>
    /// <param name="length">Storage size in Bytes</param>
    /// <returns>String representation as binary prefix</returns>
    /// <example>1048576 is 1 MB</example>
    public static string DynamicStorageSize(long length)
    {
      Contract.Requires(length >= 0L, "Storage size can not be negative.");
      Contract.Ensures(Contract.Result<string>() != null);

      if (length < 1024L)
        return $"{length:N0} Bytes";

      double dblScaledValue;
      string strUnits;

      if (length < 1024L * 1024L)
      {
        dblScaledValue = length / 1024D;
        strUnits = "kB"; // strict speaking its KiB
      }
      else if (length < 1024L * 1024L * 1024L)
      {
        dblScaledValue = length / (1024D * 1024D);
        strUnits = "MB"; // strict speaking its MiB
      }
      else if (length < 1024L * 1024L * 1024L * 1024L)
      {
        dblScaledValue = length / (1024D * 1024D * 1024D);
        strUnits = "GB"; // strict speaking its GiB
      }
      else
      {
        dblScaledValue = length / (1024D * 1024D * 1024D * 1024D);
        strUnits = "TB"; // strict speaking its TiB
      }

      return $"{dblScaledValue:N3} {strUnits}";
    }

    /// <summary>
    ///   Excel NPOI Time is one day off, the time needs to be adjusted
    /// </summary>
    /// <param name="dateTime">The original time.</param>
    /// <returns>The correct date time</returns>
    public static DateTime FixExcelNPOIDate(DateTime dateTime)
    {
      if (dateTime >= m_FirstDateTimeNextDay && dateTime < m_FirstDateTime.AddDays(2))
        return dateTime.AddDays(-1);

      return dateTime;
    }

    /// <summary>
    ///   Gets the time from ticks.
    /// </summary>
    /// <param name="ticks">The ticks.</param>
    /// <returns>A Time</returns>
    public static DateTime GetTimeFromTicks(long ticks) => m_FirstDateTime.Add(new TimeSpan(ticks));

    public static bool IsDuration(DateTime dateTime) => (dateTime >= m_FirstDateTime && dateTime < m_FirstDateTime.AddHours(240));

    public static bool IsTimeOnly(DateTime dateTime) => (dateTime >= m_FirstDateTime && dateTime < m_FirstDateTimeNextDay);

    /// <summary>
    ///   Tries to determine the date time assuming its an Excel serial date time, using regional and
    ///   common decimal separators
    /// </summary>
    /// <param name="value">The Value as string</param>
    /// <returns></returns>
    public static DateTime? SerialStringToDateTime(string value)
    {
      var stringDateValue = value?.Trim() ?? string.Empty;
      try
      {
        var numberFormatProvider = new NumberFormatInfo
        {
          NegativeSign = "-",
          PositiveSign = "+",
          NumberGroupSeparator = string.Empty
        };
        foreach (var decimalSeparator in DecimalSeparators)
        {
          numberFormatProvider.NumberDecimalSeparator = decimalSeparator.ToString(CultureInfo.CurrentCulture);
          if (!double.TryParse(stringDateValue, NumberStyles.Float, numberFormatProvider, out var timeSerial)) continue;
          if (timeSerial >= -657435 && timeSerial < 2958466)
            return DateTime.FromOADate(timeSerial);
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine("{0} is not a serial date. Error: {1}", value, ex.Message);
      }

      return null;
    }

    /// <summary>
    ///   Parses a string to a boolean.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="trueValue">An additional value that would evaluate to true</param>
    /// <param name="falseValue">An additional value that would evaluate to false</param>
    /// <returns>
    ///   <c>Null</c> if the value is empty, other wise <c>true</c> if identified as boolean or
    ///   <c>false</c> otherwise
    /// </returns>
    public static bool? StringToBoolean(string value, string trueValue, string falseValue)
    {
      if (string.IsNullOrEmpty(value))
        return null;

      var strictBool = StringToBooleanStrict(value, trueValue, falseValue);

      if (strictBool != null)
        return strictBool.Item1;

      return false;
    }

    /// <summary>
    ///   Check is a string is a boolean (strict).
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="trueValue">An additional value that would evaluate to true</param>
    /// <param name="falseValue">An additional value that would evaluate to false</param>
    /// <returns>
    ///   <c>Null</c> if the value can not be identified as boolean, other wise <c>true</c> or <c>false</c>
    /// </returns>
    public static Tuple<bool, string> StringToBooleanStrict(string value, string trueValue, string falseValue)
    {
      if (string.IsNullOrEmpty(value))
        return null;

      if (StringUtils.SplitByDelimiter(trueValue).Any(test => value.Equals(test, StringComparison.OrdinalIgnoreCase)))
      {
        return new Tuple<bool, string>(true, value);
      }

      if (StringUtils.SplitByDelimiter(falseValue).Any(test => value.Equals(test, StringComparison.OrdinalIgnoreCase)))
      {
        return new Tuple<bool, string>(false, value);
      }

      if (m_TrueValues.Any(t => value.Equals(t, StringComparison.OrdinalIgnoreCase)))
      {
        return new Tuple<bool, string>(true, value);
      }

      return m_FalseValues.Any(t => value.Equals(t, StringComparison.OrdinalIgnoreCase)) ? new Tuple<bool, string>(false, value) : null;
    }

    /// <summary>
    ///   Parses a string to a date time value
    /// </summary>
    /// <param name="originalValue">The original value.</param>
    /// <param name="dateFormat">The date format, possibly separated by delimiter</param>
    /// <param name="dateSeparator">The date separator used in the conversion</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <param name="serialDateTime">Allow Date Time values ion serial format</param>
    /// <returns>
    ///   An <see cref="DateTime" /> if the value could be interpreted, <c>null</c> otherwise
    /// </returns>
    /// <remarks>
    ///   If the date part is not filled its the 1/1/1
    /// </remarks>
    public static DateTime? StringToDateTime(string originalValue, string dateFormat, string dateSeparator,
      string timeSeparator, bool serialDateTime, CultureInfo culture = null)
    {
      var stringDateValue = originalValue?.Trim() ?? string.Empty;
      if (culture == null)
        culture = CultureInfo.CurrentCulture;

      var result = StringToDateTimeExact(originalValue, dateFormat, dateSeparator, timeSeparator, culture);
      if (result.HasValue)
        return result.Value;

      if (serialDateTime
          && (string.IsNullOrEmpty(dateSeparator) ||
              stringDateValue.IndexOf(dateSeparator, StringComparison.Ordinal) == -1)
          && (string.IsNullOrEmpty(timeSeparator) ||
              stringDateValue.IndexOf(timeSeparator, StringComparison.Ordinal) == -1))
      {
        return SerialStringToDateTime(stringDateValue);
      }

      // in case its time only and we do not have any date separator try a timespan
      if (stringDateValue.IndexOf(dateSeparator, StringComparison.Ordinal) == -1 &&
          dateFormat.IndexOf('/') == -1)
      {
        var ts = StringToTimeSpan(originalValue, timeSeparator, false);
        if (ts.HasValue)
          return new DateTime(ts.Value.Ticks);
      }

      return null;
    }


    /// <summary>
    ///   Converts Strings to date time using the culture information
    /// </summary>
    /// <param name="originalValue">The original value.</param>
    /// <param name="dateFormats">The date formats.</param>
    /// <param name="dateSeparator">The date separator.</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="fallback">if set to <c>true</c> try to fall back to some defaults using / and only parsing the date.</param>
    /// <remarks>Similar to <see cref="StringToDateTimeByCulture"/> but checks if we have a format that would fit the length of the value.
    /// </remarks>
    /// <returns></returns>
    public static DateTime? StringToDateTimeExact(string originalValue, string dateFormats, string dateSeparator,
      string timeSeparator, CultureInfo culture)
    {
      var stringDateValue = originalValue?.Trim() ?? string.Empty;

      // Quick check: If the entry is empty, or a constant string, or the length does not make
      // sense, we do not need to try and parse
      if (stringDateValue == "00000000" || stringDateValue == "99999999" || stringDateValue.Length < 4)
        return null;

      var matchingDateTimeFormats = new List<string>();
      foreach (var format in GetDateFormats(dateFormats))
      {
        if (DateLengthMatches(stringDateValue.Length, format) && (format.IndexOf('/') == -1 || stringDateValue.Contains(dateSeparator)))
          matchingDateTimeFormats.Add(format);
      }

      if (matchingDateTimeFormats.Count == 0)
        return null;

      return StringToDateTimeByCulture(stringDateValue, matchingDateTimeFormats.ToArray(), dateSeparator, timeSeparator,
        culture);
    }

    /// <summary>
    ///   Parses a string to a decimal
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="decimalSeparator">The decimal separator.</param>
    /// <param name="groupSeparator">The thousand separator.</param>
    /// <param name="allowPercentage">If set to true, a % will be recognized</param>
    /// <returns>An decimal if the value could be interpreted, <c>null</c> otherwise</returns>
    public static decimal? StringToDecimal(string value, char decimalSeparator, char groupSeparator,
      bool allowPercentage)
    {
      // in case nothing is passed in we are already done here
      if (string.IsNullOrEmpty(value))
        return null;

      // Remove any white space
      var stringFieldValue = value.Trim();

      var numDecimalSep = 0;
      var lastPos = -3;

      // Sanity Check: In case the decimalSeparator occurs multiple times is not a number in case
      // the thousand separator are closer then 3 characters together
      for (var pos = 0; pos < value.Length; pos++)
      {
        if (value[pos] == decimalSeparator)
        {
          if (numDecimalSep++ == 1)
            return null;
        }

        if (groupSeparator == '\0' || value[pos] != groupSeparator) continue;
        if (pos - lastPos < 4)
          return null;
        lastPos = pos;
      }

      var numberFormatProvider = new NumberFormatInfo
      {
        NegativeSign = "-",
        PositiveSign = "+",
        NumberDecimalSeparator = decimalSeparator.ToString(CultureInfo.CurrentCulture),
        NumberGroupSeparator = groupSeparator == '\0' ? string.Empty : groupSeparator.ToString(CultureInfo.CurrentCulture)
      };

      if (stringFieldValue.StartsWith("(", StringComparison.Ordinal) &&
          stringFieldValue.EndsWith(")", StringComparison.Ordinal))
      {
        stringFieldValue = "-" + stringFieldValue.Substring(1, stringFieldValue.Length - 2).TrimStart();
      }

      var percentage = false;
      var permille = false;
      if (allowPercentage && stringFieldValue.EndsWith("%", StringComparison.Ordinal))
      {
        percentage = true;
        stringFieldValue = stringFieldValue.Substring(0, stringFieldValue.Length - 1);
      }

      if (allowPercentage && stringFieldValue.EndsWith("‰", StringComparison.Ordinal))
      {
        permille = true;
        stringFieldValue = stringFieldValue.Substring(0, stringFieldValue.Length - 1);
      }

      /* 1 -> -x ; 2 -> - x; 3 -> x-;*/
      for (numberFormatProvider.NumberNegativePattern = 1;
          numberFormatProvider.NumberNegativePattern <= 3;
          numberFormatProvider.NumberNegativePattern++)
      {
        // Try to convert this value to a decimal value. Try to convert this value to a decimal value.
        if (decimal.TryParse(stringFieldValue, NumberStyles.Number, numberFormatProvider, out var result))
        {
          if (percentage)
            return result / 100m;
          if (permille)
            return result / 1000m;
          return result;
        }
      }
      // If this works, exit

      return null;
    }

    /// <summary>
    ///   Returns the duration on days for string value.
    /// </summary>
    /// <param name="originalValue">The original value.</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <param name="serialDateTime">Allow Date Time values ion serial format</param>
    /// <returns></returns>
    public static double StringToDurationInDays(string originalValue, string timeSeparator, bool serialDateTime)
    {
      var parsed = StringToTimeSpan(originalValue, timeSeparator, serialDateTime);
      if (parsed.HasValue)
        return parsed.Value.TotalDays;

      return 0D;
    }

    /// <summary>
    ///   Parses a string to a guid
    /// </summary>
    /// <param name="originalValue">The original value.</param>
    /// <returns>
    ///   An <see cref="Guid" /> if the value could be interpreted, <c>null</c> otherwise
    /// </returns>
    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    public static Guid? StringToGuid(string originalValue)
    {
      // only try to do this if we have the right length
      if (string.IsNullOrEmpty(originalValue) || originalValue.Length < 32 || originalValue.Length > 38) return null;
      try
      {
        return new Guid(originalValue);
      }
      catch (Exception)
      {
        return null;
      }
    }

    /// <summary>
    ///   Parses a strings to an int.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="decimalSeparator">The decimal separator.</param>
    /// <param name="thousandSeparator">The thousand separator.</param>
    /// <returns>An int if the value could be interpreted, <c>null</c> otherwise</returns>
    public static short? StringToInt16(string value, char decimalSeparator, char thousandSeparator)
    {
      if (string.IsNullOrEmpty(value))
        return null;
      try
      {
        var dec = StringToDecimal(value, decimalSeparator, thousandSeparator, false);
        if (dec.HasValue)
          return Convert.ToInt16(dec.Value, CultureInfo.InvariantCulture);
      }
      catch (OverflowException)
      {
        // The numerical value could not be converted to an integer
      }

      return null;
    }

    /// <summary>
    ///   Parses a strings to an int.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="decimalSeparator">The decimal separator.</param>
    /// <param name="thousandSeparator">The thousand separator.</param>
    /// <returns>An int if the value could be interpreted, <c>null</c> otherwise</returns>
    public static int? StringToInt32(string value, char decimalSeparator, char thousandSeparator)
    {
      if (string.IsNullOrEmpty(value))
        return null;
      try
      {
        var dec = StringToDecimal(value, decimalSeparator, thousandSeparator, false);
        if (dec.HasValue)
          return Convert.ToInt32(dec.Value, CultureInfo.InvariantCulture);
      }
      catch (OverflowException)
      {
        // The numerical value could not be converted to an integer
      }

      return null;
    }

    /// <summary>
    ///   Parses a strings to an int.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="decimalSeparator">The decimal separator.</param>
    /// <param name="thousandSeparator">The thousand separator.</param>
    /// <returns>An int if the value could be interpreted, <c>null</c> otherwise</returns>
    public static long? StringToInt64(string value, char decimalSeparator, char thousandSeparator)
    {
      if (string.IsNullOrEmpty(value))
        return null;
      try
      {
        var dec = StringToDecimal(value, decimalSeparator, thousandSeparator, false);
        if (dec.HasValue)
          return Convert.ToInt64(dec.Value, CultureInfo.InvariantCulture);
      }
      catch (OverflowException)
      {
        // The numerical value could not be converted to an integer
      }

      return null;
    }

    /// <summary>
    ///   Splits a string and returns the required part
    /// </summary>
    /// <param name="value">The string value that should be split.</param>
    /// <param name="splitter">The splitter character.</param>
    /// <param name="part">The part that should be returned, starting with 1.</param>
    /// <param name="toEnd">Read the part up to the end</param>
    /// <returns>
    ///   <c>Null</c> if the value is empty or the part can not be found. If the desired part is 1 and
    ///   the splitter is not contained the whole value is returned.
    /// </returns>
    public static string StringToTextPart(string value, char splitter, int part, bool toEnd)
    {
      if (string.IsNullOrEmpty(value) || part < 1)
        return null;

      var indexOfSplitter = value.IndexOf(splitter);

      // In case we want the first part but the splitter is not part of the text return the whole value
      if (part == 1 && (indexOfSplitter == -1 || toEnd))
        return value;

      if (!toEnd)
      {
        var valueParts = value.Split(new[] { splitter }, part + 1, StringSplitOptions.None);
        if (valueParts.Length >= part) return valueParts[part - 1];
      }
      else
      {
        while (indexOfSplitter != -1 && part > 2)
        {
          indexOfSplitter = value.IndexOf(splitter, indexOfSplitter + 1);
          part--;
        }

        if (indexOfSplitter != -1)
          return value.Substring(indexOfSplitter + 1);
      }

      return null;
    }

    /// <summary>
    ///   Strings to time span.
    /// </summary>
    /// <param name="originalValue">The original value.</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <param name="serialDateTime">Allow Date Time values in serial format</param>
    /// <returns></returns>
    [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId =
      "System.Int32.TryParse(System.String,System.Int32@)")]
    [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId =
      "System.Int32.TryParse(string,System.Int32@)")]
    public static TimeSpan? StringToTimeSpan(string originalValue, string timeSeparator, bool serialDateTime)
    {
      var stringTimeValue = originalValue?.Trim();
      if (string.IsNullOrEmpty(stringTimeValue))
        return null;

      var separator = string.IsNullOrEmpty(timeSeparator) ? ':' : timeSeparator[0];

      var min = 0;
      var sec = 0;

      var hrsIndex = stringTimeValue.IndexOf(separator, 0);
      if (hrsIndex < 0)
      {
        if (!serialDateTime) return null;
        var dt = SerialStringToDateTime(originalValue);
        if (dt.HasValue)
          return new TimeSpan(dt.Value.Ticks - m_FirstDateTime.Ticks);

        return null;
      }

      var hrs = stringTimeValue.Substring(0, hrsIndex);
      if (hrs.IndexOf(' ') != -1)
        return null;
      // hrs = hrs.Substring(hrs.IndexOf(' ') + 1);

      if (int.TryParse(hrs, out var hours))
      {
        hrsIndex++;
        var minIndex = stringTimeValue.IndexOf(separator, hrsIndex);
        if (minIndex > 0)
        {
          if (int.TryParse(stringTimeValue.Substring(hrsIndex, minIndex - hrsIndex), out min))
            int.TryParse(stringTimeValue.Substring(minIndex + 1), out sec);
        }
        else
        {
          int.TryParse(stringTimeValue.Substring(hrsIndex), out min);
        }
      }

      // Handle am / pm

      // 12:00 AM - 12:59 AM --> 00:00 - 00:59
      if (hours == 12 && originalValue.EndsWith("am", StringComparison.OrdinalIgnoreCase))
        hours -= 12;

      // 12:00 PM - 12:59 PM 12:00 - 12:59
      // No change

      // 01:00 pm - 11:59 PM --> 13:00 - 23:59
      if (hours < 12 && originalValue.EndsWith("pm", StringComparison.OrdinalIgnoreCase))
        hours += 12;

      return new TimeSpan(0, hours, min, sec);
    }

    /// <summary>
    ///   Gets the an array of date formats, splitting the given date formats text by delimiter
    /// </summary>
    /// <param name="dateFormat">The date format, possibly separated by delimiter</param>
    /// <returns>An array of formats</returns>
    private static string[] GetDateFormats(string dateFormat)
    {
      Contract.Ensures(Contract.Result<string[]>() != null);
      var dateTimeFormats = StringUtils.SplitByDelimiter(dateFormat);

      var complete = new List<string>(dateTimeFormats);
      foreach (var dateTimeFormat in dateTimeFormats)
      {
        // In case of a date & time format add the date only format separately
        var indexHour = dateTimeFormat.IndexOf("h", StringComparison.OrdinalIgnoreCase);

        // assuming there is a  text before the hour that has a reasonable size take it as date...
        if (indexHour > 4)
        {
          var dateOnly = dateTimeFormat.Substring(0, indexHour - 1).Trim();
          if (!complete.Contains(dateOnly))
            complete.Add(dateOnly);
        }
      }

      return complete.ToArray();
    }



    /// <summary>
    ///   Converts Strings to date time using the culture information
    /// </summary>
    /// <param name="stringDateValue">The string date value make sure the text is trimmed</param>
    /// <param name="dateTimeFormats">The date time formats.</param>
    /// <param name="dateSeparator">The date separator.</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <param name="culture">The culture.</param>
    /// <returns></returns>
    private static DateTime? StringToDateTimeByCulture(string stringDateValue, string[] dateTimeFormats,
      string dateSeparator, string timeSeparator, CultureInfo culture)
    {
      Contract.Requires(stringDateValue != null);
      Contract.Requires(dateTimeFormats != null && dateTimeFormats.Length > 0);
      Contract.Requires(dateSeparator != null);
      Contract.Requires(timeSeparator != null);
      Contract.Requires(culture != null);

      var dateTimeFormatInfo = new DateTimeFormatInfo();

      dateTimeFormatInfo.SetAllDateTimePatterns(dateTimeFormats, 'd');
      dateTimeFormatInfo.DateSeparator = dateSeparator;
      dateTimeFormatInfo.TimeSeparator = timeSeparator;

      dateTimeFormatInfo.AbbreviatedDayNames = culture.DateTimeFormat.AbbreviatedDayNames;
      dateTimeFormatInfo.DayNames = culture.DateTimeFormat.DayNames;
      dateTimeFormatInfo.MonthNames = culture.DateTimeFormat.MonthNames;
      dateTimeFormatInfo.AbbreviatedMonthNames = culture.DateTimeFormat.AbbreviatedMonthNames;

      // Use ParseExact since Parse does not work if a date separator is set but the date separator
      // is not part of the date format
      if (DateTime.TryParseExact(stringDateValue, dateTimeFormats, dateTimeFormatInfo,
        DateTimeStyles.NoCurrentDateDefault, out var result))
      {
        return result;
      }

      // try InvariantCulture
      if (culture.Name != "en-US" && culture != CultureInfo.InvariantCulture)
      {
        dateTimeFormatInfo.AbbreviatedDayNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames;
        dateTimeFormatInfo.DayNames = CultureInfo.InvariantCulture.DateTimeFormat.DayNames;
        dateTimeFormatInfo.MonthNames = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames;
        dateTimeFormatInfo.AbbreviatedMonthNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames;

        if (DateTime.TryParseExact(stringDateValue, dateTimeFormats, dateTimeFormatInfo,
          DateTimeStyles.NoCurrentDateDefault, out result))
        {
          return result;
        }
      }

      // In case a date with time is passed in it would not be parsed... take the part of before
      // the space and try again
      var foundSpace = stringDateValue.LastIndexOf(' ');

      // Only do this if we have at least 6 characters
      if (foundSpace > 6)
      {
        return StringToDateTimeByCulture(stringDateValue.Substring(0, foundSpace), dateTimeFormats, dateSeparator,
          timeSeparator, culture);
      }

      return null;
    }
  }
}