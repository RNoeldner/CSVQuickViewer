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

namespace CsvTools
{
  /// <summary>
  /// Information of the length of given formats in with teh current settings
  /// </summary>
  public readonly struct DateTimeFormatInformation
  {
    /// <summary>
    /// The max length always assuming the longest names are used
    /// </summary>
    public readonly int MaxLength;

    /// <summary>
    /// The minimum length always assuming the shortest names are used
    /// </summary>
    public readonly int MinLength;


    /// <summary>
    ///   Initializes a new instance of the <see cref="DateTimeFormatInformation" /> class.
    /// </summary>
    /// <param name="formatSpecifier">The format specifier</param>
    /// <remarks>Please do not use literal string delimiter these are not handled properly</remarks>
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
      // HandleLiteral string delimiter '
      // This is actually not precise enough as ' minutes' will not be handled properly
      .Replace("'", "");

      MinLength = formatSpecifier.Length;
      MaxLength = formatSpecifier.Length;

      SetMinMaxAndRemove("dddd", DateTimeFormatLength.MinDayLong, DateTimeFormatLength.MaxDayLong, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("ddd", DateTimeFormatLength.MinDayMid, DateTimeFormatLength.MaxDayMid, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("dd", 2, 2, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("d", 1, 2, ref formatSpecifier, ref MinLength, ref MaxLength);

      SetMinMaxAndRemove("yyyy", 4, 4, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("yy", 2, 2, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("y", 1, 2, ref formatSpecifier, ref MinLength, ref MaxLength);

      SetMinMaxAndRemove("HH", 2, 2, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("H", 1, 2, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("hh", 2, 2, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("h", 1, 2, ref formatSpecifier, ref MinLength, ref MaxLength);

      SetMinMaxAndRemove("mm", 2, 2, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("m", 1, 2, ref formatSpecifier, ref MinLength, ref MaxLength);

      SetMinMaxAndRemove("MMMM", DateTimeFormatLength.MinMonthLong, DateTimeFormatLength.MaxMonthLong, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("MMM", DateTimeFormatLength.MinMonthMid, DateTimeFormatLength.MaxMonthMid, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("MM", 2, 2, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("M", 1, 2, ref formatSpecifier, ref MinLength, ref MaxLength);

      SetMinMaxAndRemove("F", 0, 1, ref formatSpecifier, ref MinLength, ref MaxLength);

      SetMinMaxAndRemove("ss", 2, 2, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("s", 1, 2, ref formatSpecifier, ref MinLength, ref MaxLength);

      // interpreted as a standard date and time format specifier
      SetMinMaxAndRemove("K", 0, 6, ref formatSpecifier, ref MinLength, ref MaxLength);

      // signed offset of the local operating system's time zone from UTC,
      SetMinMaxAndRemove("zzz", 6, 6, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("zz", 3, 3, ref formatSpecifier, ref MinLength, ref MaxLength);
      SetMinMaxAndRemove("z", 2, 3, ref formatSpecifier, ref MinLength, ref MaxLength);

      // AM / PM
      SetMinMaxAndRemove("tt", DateTimeFormatLength.MinDesignator, DateTimeFormatLength.MaxDesignator, ref formatSpecifier, ref MinLength, ref MaxLength);
    }

    private static void SetMinMaxAndRemove(in string search, int minLenSearch, int maxLenSearch, ref string format,
      ref int minLength,
      ref int maxLength)
    {
      do
      {
        var pos = format.IndexOf(search, StringComparison.Ordinal);
        if (pos == -1)
          break;
        format = format.Remove(pos, search.Length);
        minLength += minLenSearch - search.Length;
        maxLength += maxLenSearch - search.Length;
      } while (true);
    }
  }
}