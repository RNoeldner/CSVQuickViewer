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
  public struct DateTimeFormatInformation
  {
    public readonly int MaxLength;
    public readonly int MinLength;
    public readonly bool NamedDate;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DateTimeFormatInformation" /> class.
    /// </summary>
    /// <param name="formatSpecifier">The format specifier</param>
    /// <remarks>Please do not use literal string delimiter these are not handled properly</remarks>
    public DateTimeFormatInformation(string formatSpecifier)
    {
      // Handle other chars like mm':'ss' minutes' or mm\:ss\ \m\i\n\u\t\e\s

      // Handle text escaped with \
      formatSpecifier = formatSpecifier.Replace("\\y", ".");
      formatSpecifier = formatSpecifier.Replace("\\M", ".");
      formatSpecifier = formatSpecifier.Replace("\\d", ".");

      formatSpecifier = formatSpecifier.Replace("\\H", ".");
      formatSpecifier = formatSpecifier.Replace("\\h", ".");
      formatSpecifier = formatSpecifier.Replace("\\m", ".");
      formatSpecifier = formatSpecifier.Replace("\\s", ".");
      formatSpecifier = formatSpecifier.Replace("\\F", ".");
      formatSpecifier = formatSpecifier.Replace("\\t", ".");
      formatSpecifier = formatSpecifier.Replace("\\\\", ".");
      formatSpecifier = formatSpecifier.Replace("\\", "");

      // HandleLiteral string delimiter '

      // This is actually not precise enough as ' minutes' will not be handled properly
      formatSpecifier = formatSpecifier.Replace("'", "");

      MinLength = formatSpecifier.Length;
      MaxLength = formatSpecifier.Length;
      NamedDate = formatSpecifier.IndexOf("ddd", StringComparison.Ordinal) != -1
                  || formatSpecifier.IndexOf("MMM", StringComparison.Ordinal) != -1;

      SetMinMax(ref formatSpecifier, "dddd", DateTimeFormatLength.MinDayLong, DateTimeFormatLength.MaxDayLong, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "ddd", DateTimeFormatLength.MinDayMid, DateTimeFormatLength.MaxDayMid, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "dd", 2, 2, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "d", 1, 2, ref MinLength, ref MaxLength);

      SetMinMax(ref formatSpecifier, "yyyy", 4, 4, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "yy", 2, 2, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "y", 1, 2, ref MinLength, ref MaxLength);

      SetMinMax(ref formatSpecifier, "HH", 2, 2, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "H", 1, 2, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "hh", 2, 2, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "h", 1, 2, ref MinLength, ref MaxLength);

      SetMinMax(ref formatSpecifier, "mm", 2, 2, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "m", 1, 2, ref MinLength, ref MaxLength);

      SetMinMax(ref formatSpecifier, "MMMM", DateTimeFormatLength.MinMonthLong, DateTimeFormatLength.MaxMonthLong, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "MMM", DateTimeFormatLength.MinMonthMid, DateTimeFormatLength.MaxMonthMid, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "MM", 2, 2, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "M", 1, 2, ref MinLength, ref MaxLength);

      SetMinMax(ref formatSpecifier, "F", 0, 1, ref MinLength, ref MaxLength);

      SetMinMax(ref formatSpecifier, "ss", 2, 2, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "s", 1, 2, ref MinLength, ref MaxLength);

      // interpreted as a standard date and time format specifier
      SetMinMax(ref formatSpecifier, "K", 0, 6, ref MinLength, ref MaxLength);

      // signed offset of the local operating system's time zone from UTC,
      SetMinMax(ref formatSpecifier, "zzz", 6, 6, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "zz", 3, 3, ref MinLength, ref MaxLength);
      SetMinMax(ref formatSpecifier, "z", 2, 3, ref MinLength, ref MaxLength);

      // AM / PM
      SetMinMax(ref formatSpecifier, "tt", DateTimeFormatLength.MinDesignator, DateTimeFormatLength.MaxDesignator, ref MinLength, ref MaxLength);
    }

    private static void SetMinMax(ref string format, in string search, int minLen, int maxLen, ref int MinLength, ref int MaxLength)
    {
      var pos = format.IndexOf(search, StringComparison.Ordinal);
      while (pos != -1)
      {
        MinLength += minLen - search.Length;
        MaxLength += maxLen - search.Length;
        format = format.Remove(pos, search.Length);
        pos = format.IndexOf(search, StringComparison.Ordinal);
      }
    }
  }
}