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
#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CsvTools
{
  /// <summary>
  ///   Collection of static functions for string in regards to type conversions
  /// </summary>
  public static class StringConversion
  {
    public static string DateTimeToString(this in DateTime dateTime, in ValueFormat format) =>
      DateTimeToString(dateTime, format.DateFormat, format.DateSeparator, format.TimeSeparator);

    /// <summary>
    ///   Converts a dates to string
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <param name="dateFormat">The format.</param>
    /// <param name="dateSeparator">The date separator.</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <param name="cultureInfo"></param>
    /// <returns>Formatted value</returns>
    public static string DateTimeToString(in DateTime dateTime, in string dateFormat, char dateSeparator, char timeSeparator, CultureInfo? cultureInfo = null)
    {
      if (cultureInfo==null)
        cultureInfo = CultureInfo.InvariantCulture;

      // replacing the format placeholder with constants, to be replaced back later
      if (!dateFormat.Contains("HHH"))
        return dateTime.ToString(dateFormat.Replace('/', '\uFFF9').Replace(':', '\uFFFA'), cultureInfo)
               .Replace('\uFFF9', dateSeparator).Replace('\uFFFA', timeSeparator);
      var pad = 2;

      // only allow format that has time values
      // ReSharper disable once StringLiteralTypo
      const string allowed = " Hhmsf:";

      var result = dateFormat.Where(chr => allowed.IndexOf(chr) != -1)
                         .Aggregate(string.Empty, (current, chr) => current + chr).Trim();
      // make them all upper case H lower case does not make sense
      while (result.Contains("h"))
        result = result.Replace("h", "H");

      for (var length = 5; length > 2; length--)
      {
        var search = new string('H', length);
        if (!result.Contains(search))
          continue;
        result = result.Replace(search, "HH");
        pad = length;
        break;
      }

      var strFormat = "{0:" + new string('0', pad) + "}".Replace('/', '\uFFF9').Replace(':', '\uFFFA');
      return dateTime.ToString(
        result.Replace("HH", string.Format(cultureInfo, strFormat, Math.Floor((dateTime - DateTimeConstants.FirstDateTime).TotalHours))), cultureInfo)
        .Replace('\uFFF9', dateSeparator).Replace('\uFFFA', timeSeparator);
    }

    /// <summary>
    ///   Converts a decimals to string.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The <see cref="ValueFormat" />.</param>
    /// <returns>Formatted value</returns>
    public static string DecimalToString(in decimal value, in ValueFormat format)
    => value.ToString(format.NumberFormat.Length == 0 ? ValueFormat.cNumberFormatDefault : format.NumberFormat, CultureInfo.InvariantCulture)
        .ReplaceDefaults('.', format.DecimalSeparator, ',', format.GroupSeparator);
    
    /// <summary>
    ///   Displays the date time in local format
    /// </summary>
    /// <param name="dateTime">The date time.</param>
    /// <param name="culture">The culture.</param>
    /// <returns></returns>
    public static string DisplayDateTime(in DateTime dateTime, in CultureInfo culture)
    {
      // if we only have a time:
      if (IsTimeOnly(dateTime))
        return dateTime.ToString("T", culture);

      if (dateTime.TimeOfDay.TotalSeconds<1)
        return dateTime.ToString("d", culture);

      if (IsDuration(dateTime))
        return (dateTime - DateTimeConstants.FirstDateTime).TotalHours.ToString(
                 (dateTime - DateTimeConstants.FirstDateTime).TotalHours >= 100 ? "000" : "00",
                 CultureInfo.InvariantCulture) + ":"
                                               + (dateTime - DateTimeConstants.FirstDateTime).Minutes.ToString(
                                                 "00",
                                                 CultureInfo.InvariantCulture) + ":"
                                               + (dateTime - DateTimeConstants.FirstDateTime).Seconds.ToString(
                                                 "00",
                                                 CultureInfo.InvariantCulture);
      return dateTime.ToString("G", culture);
    }

    /// <summary>
    ///   Converts a doubles to string.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="format">The <see cref="ValueFormat" />.</param>
    /// <returns>Formatted value</returns>
    public static string DoubleToString(in double value, in ValueFormat format)
    {
      var valueFormat = format.NumberFormat.Length == 0
                          ? ValueFormat.cNumberFormatDefault
                          : format.NumberFormat;
      return value.ToString(valueFormat, CultureInfo.InvariantCulture).ReplaceDefaults(
        '.', format.DecimalSeparator,
        ',', format.GroupSeparator);
    }

    /// <summary>
    ///   Formats a file size in KiloBytes or MegaBytes depending on size
    /// </summary>
    /// <param name="length">Storage size in Bytes</param>
    /// <returns>String representation as binary prefix</returns>
    /// <example>1048576 is 1 MB</example>
    public static string DynamicStorageSize(long length)
    {
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

      return $"{dblScaledValue:N2} {strUnits}";
    }

    /// <summary>
    ///   Gets the time from ticks.
    /// </summary>
    /// <param name="ticks">The ticks.</param>
    /// <returns>A Time</returns>
    public static DateTime GetTimeFromTicks(this long ticks) => DateTimeConstants.FirstDateTime.Add(new TimeSpan(ticks));

    public static bool IsTimeOnly(this in DateTime dateTime) =>
      dateTime >= DateTimeConstants.FirstDateTime && dateTime < DateTimeConstants.FirstDateTime.AddDays(1);

    public static DateTime TimeOnly(this in DateTime dateTime) =>
      DateTimeConstants.FirstDateTime.Add(dateTime.TimeOfDay);


    private static bool IsDuration(in DateTime dateTime) =>
      dateTime >= DateTimeConstants.FirstDateTime && dateTime < DateTimeConstants.FirstDateTime.AddHours(240);
  }
}
