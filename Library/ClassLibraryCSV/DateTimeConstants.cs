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
using System.Collections.Generic;
using System.Globalization;

namespace CsvTools;

/// <summary>
///  Provides constants for common date and time formats.
/// Includes a static FirstDateTime property, CommonDateTimeFormats method,
/// and CommonTimeFormats method to retrieve standardized formats.
/// The class is static to allow easy access to these constants.
/// </summary>
public static class DateTimeConstants
{
  /// <summary>
  ///   A static value any time only value will have this date, property initialized to 12/30/1899.
  /// </summary>
  public static DateTime FirstDateTime { get; }
  private static readonly HashSet<string> m_CommonDateTimeFormats;
  private static readonly HashSet<string> m_CommonTimeFormats;

  static DateTimeConstants()
  {
    FirstDateTime = new DateTime(1899, 12, 30, 0, 0, 0, 0);
    m_CommonTimeFormats = new HashSet<string>(StringComparer.Ordinal)
    {
      CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern
        .ReplaceDefaults(CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator.FromText(), '/', CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator[0], ':'),
      CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern
        .ReplaceDefaults(CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator.FromText(), '/', CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator[0], ':'),
      "HH:mm:ss", "HH:mm", "h:mm tt","HH:mm:ss.FFF"
    };

    m_CommonDateTimeFormats = new HashSet<string>(StringComparer.Ordinal)
    {
      CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern
        .ReplaceDefaults(CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator.FromText(), '/', CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator[0], ':'),
      (CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern)
      .ReplaceDefaults(CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator.FromText(), '/', CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator[0], ':'),
      "MM/dd/yyyy",
      "MM/dd/yyyy HH:mm:ss",
      "M/d/yyyy",
      "M/d/yyyy h:mm tt",
      "M/d/yyyy h:mm:ss tt",
      "dd/MM/yyyy",
      "dd/MM/yyyy HH:mm:ss",
      "d/MM/yyyy",
      "yyyy/MM/dd",
      "yyyy/MM/ddTHH:mm:ss",
      "yyyy/MM/dd HH:mm:ss.FFF",
      "yyyyMMdd",
      "yyyyMMddTHH:mm:ss.FFF",
    };
    foreach (var format in m_CommonTimeFormats)
      m_CommonDateTimeFormats.Add(format);
  }

  /// <summary>
  ///  Get the most common date time formats
  /// </summary>
  /// <param name="known">Format that should be added to the list if not already present</param>
  /// <remarks>Methods not thread-safe for writes.
  /// </remarks>
  public static IEnumerable<string> CommonDateTimeFormats(string known)
  {
    // add the existing data
    foreach (var format in known.Split(StaticCollections.ListDelimiterChars, StringSplitOptions.RemoveEmptyEntries))
      m_CommonDateTimeFormats.Add(format);
    return m_CommonDateTimeFormats;
  }

  /// <summary>
  ///  Get the most common time formats
  /// </summary>
  /// <remarks>Returning the internal Collection directly exposes mutable state.</remarks>
  public static IReadOnlyCollection<string> CommonTimeFormats() => m_CommonTimeFormats;
}