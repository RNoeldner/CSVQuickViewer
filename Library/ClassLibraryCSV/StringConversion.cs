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
#nullable enable
using System;
using System.Buffers;
using System.Globalization;
using System.Text;

namespace CsvTools;

/// <summary>
///   Collection of static functions for string in regard to type conversions
/// </summary>
public static class StringConversion
{
  /// <summary>
  /// Convert a DateTime to a text in a given format
  /// </summary>
  /// <param name="dateTime">The date time value</param>
  /// <param name="format">The ValueFormat</param>
  /// <returns>Formatted value</returns>
  public static string DateTimeToString(this DateTime dateTime, ValueFormat format) =>
    DateTimeToString(dateTime, format.DateFormat, format.DateSeparator, format.TimeSeparator);

  /// <summary>
  /// Converts a <see cref="DateTime"/> to a formatted string using custom date and time separators.
  /// </summary>
  /// <remarks>
  /// If the <paramref name="formatSpan"/> contains "HHH", the method calculates the total hours 
  /// elapsed since a defined constant (<see cref="DateTimeConstants.FirstDateTime"/>) and 
  /// applies padding based on the number of 'H' characters, bypassing standard format providers 
  /// for the hour component.
  /// </remarks>
  /// <param name="dateTime">The <see cref="DateTime"/> value to convert.</param>
  /// <param name="formatSpan">The format string (e.g., "yyyy-MM-dd HH:mm").</param>
  /// <param name="dateSeparator">The character used to replace standard date separators.</param>
  /// <param name="timeSeparator">The character used to replace standard time separators.</param>
  /// <param name="cultureInfo">The <see cref="CultureInfo"/> used for formatting. Defaults to <see cref="CultureInfo.InvariantCulture"/> if null.</param>
  /// <returns>A string representation of the <paramref name="dateTime"/> formatted according to the specified parameters.</returns>
  public static string DateTimeToString(DateTime dateTime, ReadOnlySpan<char> formatSpan, char dateSeparator, char timeSeparator, CultureInfo? cultureInfo = null)
  {
    cultureInfo ??= CultureInfo.InvariantCulture;
    if (!formatSpan.Contains("HHH".AsSpan(), StringComparison.OrdinalIgnoreCase))
    {
      // Rent a buffer from the pool to avoid heap allocation
      char[] buffer = ArrayPool<char>.Shared.Rent(formatSpan.Length);
      try
      {
        for (int i = 0; i < formatSpan.Length; i++)
        {
          char c = formatSpan[i];
          if (c == '/') buffer[i] = dateSeparator;
          else if (c == ':') buffer[i] = timeSeparator;
          else buffer[i] = c;
        }
        // Create string from the buffer
        return dateTime.ToString(new string(buffer, 0, formatSpan.Length), cultureInfo);

      }
      finally
      {
        // Always return the buffer to the pool
        ArrayPool<char>.Shared.Return(buffer);
      }
    }

    // Measure the length for the hours block
    var lengthHours = 0;
    var consecutiveHours = true;
    for (int i = 0; i < formatSpan.Length; i++)
    {
      char c = formatSpan[i];
      if (c=='h' || c=='H')
      {
        if (consecutiveHours)
          lengthHours++;
        consecutiveHours = true;
      }
      else
        consecutiveHours = false;
    }

    StringBuilder newFormatTimeOnly = new StringBuilder();
    bool hasHours = false;
    for (int i = 0; i < formatSpan.Length; i++)
    {
      char c = formatSpan[i];
      if (c=='h' || c=='H')
      {
        if (!hasHours)
        {
          var hours = (long) Math.Floor((dateTime - DateTimeConstants.FirstDateTime).TotalHours);
          int digits = (hours == 0) ? 1 : (int) Math.Floor(Math.Log10(hours)) + 1;
          newFormatTimeOnly.Append(lengthHours > digits
            ? hours.ToString(new string('0', lengthHours))
            : hours.ToString(CultureInfo.InvariantCulture));
        }
        hasHours = true;
      }
      if (c=='m' || c=='s' || c=='f' || c==' ' || c=='.')
        newFormatTimeOnly.Append(c);
      if (c == ':')
        newFormatTimeOnly.Append(timeSeparator);
    }

    return dateTime.ToString(newFormatTimeOnly.ToString(), cultureInfo);
  }

  /// <summary>
  ///   Converts a decimal to string.
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
  public static string DisplayDateTime(DateTime dateTime, CultureInfo culture)
  {
    // if we only have a time value:
    if (IsTimeOnly(dateTime))
      return dateTime.ToString("T", culture);

    if (dateTime.TimeOfDay.TotalSeconds<1)
      return dateTime.ToString("d", culture);

    if (dateTime >= DateTimeConstants.FirstDateTime && dateTime < DateTimeConstants.FirstDateTime.AddHours(240))
      return $"{(dateTime - DateTimeConstants.FirstDateTime).TotalHours}:{(dateTime - DateTimeConstants.FirstDateTime).Minutes:00}:{(dateTime - DateTimeConstants.FirstDateTime).Seconds:00}";

    return dateTime.ToString("G", culture);
  }

  /// <summary>
  ///   Converts a 64bit integer value to string.
  /// </summary>
  /// <param name="value">The value.</param>
  /// <param name="format">The <see cref="ValueFormat" />.</param>
  /// <returns>Formatted value</returns>
  public static string LongToString(in long value, in ValueFormat format)
  {
    var valueFormat = format.NumberFormat.Length == 0
      ? ValueFormat.cNumberFormatDefault
      : format.NumberFormat;
    return value.ToString(valueFormat, CultureInfo.InvariantCulture).ReplaceDefaults('.', format.DecimalSeparator, ',', format.GroupSeparator);
  }

  /// <summary>
  ///   Converts a double to string.
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
  /// <returns>String representation as a binary prefix</returns>
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

  /// <summary>
  ///   Determines if the DateTime is indeed a time only
  /// </summary>
  /// <param name="dateTime"></param>
  /// <returns><c>true</c> if it should be treated as Time without date</returns>
  public static bool IsTimeOnly(this DateTime dateTime) =>
    dateTime >= DateTimeConstants.FirstDateTime && dateTime < DateTimeConstants.FirstDateTime.AddDays(1);
}