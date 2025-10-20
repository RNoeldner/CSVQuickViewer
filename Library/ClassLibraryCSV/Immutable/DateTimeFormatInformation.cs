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
using System;

namespace CsvTools
{
  /// <summary>
  /// Information about the minimum and maximum length of a date/time format string with the current settings.
  /// </summary>
  public record struct DateTimeFormatInformation
  {
    /// <summary>
    /// The maximum length, assuming the longest names are used.
    /// </summary>
    public int MaxLength { get; }

    /// <summary>
    /// The minimum length, assuming the shortest names are used.
    /// </summary>
    public int MinLength { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeFormatInformation" /> struct.
    /// </summary>
    /// <param name="formatSpecifier">The format specifier.</param>
    /// <remarks>Please do not use literal string delimiters; these are not handled properly.</remarks>
    public DateTimeFormatInformation(string formatSpecifier)
    {
      // Handle other chars like mm':'ss' minutes' or mm\:ss\ \m\i\n\u\t\e\s
      // Handle text escaped with \ they should not exist later
      formatSpecifier = formatSpecifier.Trim()
        .Replace("\\y", ".")
        .Replace("\\M", ".")
        .Replace("\\d", ".")
        .Replace("\\H", ".")
        .Replace("\\h", ".")
        .Replace("\\m", ".")
        .Replace("\\s", ".")
        .Replace("\\F", ".")
        .Replace("\\t", ".")
        .Replace(@"\\", ".")
        .Replace("\\", "")
        // Handle literal string delimiter '
        // This is actually not precise enough as ' minutes' will not be handled properly
        .Replace("'", "");

      int minLength = formatSpecifier.Length;
      int maxLength = formatSpecifier.Length;

      SetMinMaxAndRemove("dddd", DateTimeFormatLength.MinDayLong, DateTimeFormatLength.MaxDayLong, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("ddd", DateTimeFormatLength.MinDayMid, DateTimeFormatLength.MaxDayMid, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("dd", 2, 2, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("d", 1, 2, ref formatSpecifier, ref minLength, ref maxLength);

      SetMinMaxAndRemove("yyyy", 4, 4, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("yy", 2, 2, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("y", 1, 2, ref formatSpecifier, ref minLength, ref maxLength);

      SetMinMaxAndRemove("HH", 2, 2, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("H", 1, 2, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("hh", 2, 2, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("h", 1, 2, ref formatSpecifier, ref minLength, ref maxLength);

      SetMinMaxAndRemove("mm", 2, 2, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("m", 1, 2, ref formatSpecifier, ref minLength, ref maxLength);

      SetMinMaxAndRemove("MMMM", DateTimeFormatLength.MinMonthLong, DateTimeFormatLength.MaxMonthLong, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("MMM", DateTimeFormatLength.MinMonthMid, DateTimeFormatLength.MaxMonthMid, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("MM", 2, 2, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("M", 1, 2, ref formatSpecifier, ref minLength, ref maxLength);

      SetMinMaxAndRemove("F", 0, 1, ref formatSpecifier, ref minLength, ref maxLength);

      SetMinMaxAndRemove("ss", 2, 2, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("s", 1, 2, ref formatSpecifier, ref minLength, ref maxLength);

      // interpreted as a standard date and time format specifier
      SetMinMaxAndRemove("K", 0, 6, ref formatSpecifier, ref minLength, ref maxLength);

      // signed offset of the local operating system's time zone from UTC,
      SetMinMaxAndRemove("zzz", 6, 6, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("zz", 3, 3, ref formatSpecifier, ref minLength, ref maxLength);
      SetMinMaxAndRemove("z", 2, 3, ref formatSpecifier, ref minLength, ref maxLength);

      // AM / PM
      SetMinMaxAndRemove("tt", DateTimeFormatLength.MinDesignator, DateTimeFormatLength.MaxDesignator, ref formatSpecifier, ref minLength, ref maxLength);

      MinLength = minLength;
      MaxLength = maxLength;
    }

    private static void SetMinMaxAndRemove(string search, int minLenSearch, int maxLenSearch, ref string format,
      ref int minLength,
      ref int maxLength)
    {
      while (true)
      {
        var pos = format.IndexOf(search, StringComparison.Ordinal);
        if (pos == -1)
          break;
        format = format.Remove(pos, search.Length);
        minLength += minLenSearch - search.Length;
        maxLength += maxLenSearch - search.Length;
      }
    }
  }
}
