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
  public class DateTimeFormatInformation
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="DateTimeFormatInformation" /> class.
    /// </summary>
    /// <param name="formatSpecifier">The format specifier</param>
    /// <remarks>Please do not use literal string delimiter these are not handled properly</remarks>
    public DateTimeFormatInformation(string formatSpecifier)
    {
      // Handle other chars like mm':'ss' minutes' or mm\:ss\ \m\i\n\u\t\e\s

      // Handle escaped with \
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

      SetMinMax(ref formatSpecifier, "dddd", DateTimeFormatLength.MinDayLong, DateTimeFormatLength.MaxDayLong);
      SetMinMax(ref formatSpecifier, "ddd", DateTimeFormatLength.MinDayMid, DateTimeFormatLength.MaxDayMid);
      SetMinMax(ref formatSpecifier, "dd", 2, 2);
      SetMinMax(ref formatSpecifier, "d", 1, 2);

      SetMinMax(ref formatSpecifier, "yyyy", 4, 4);
      SetMinMax(ref formatSpecifier, "yy", 2, 2);
      SetMinMax(ref formatSpecifier, "y", 1, 2);

      SetMinMax(ref formatSpecifier, "HH", 2, 2);
      SetMinMax(ref formatSpecifier, "H", 1, 2);
      SetMinMax(ref formatSpecifier, "hh", 2, 2);
      SetMinMax(ref formatSpecifier, "h", 1, 2);

      SetMinMax(ref formatSpecifier, "mm", 2, 2);
      SetMinMax(ref formatSpecifier, "m", 1, 2);

      SetMinMax(ref formatSpecifier, "MMMM", DateTimeFormatLength.MinMonthLong, DateTimeFormatLength.MaxMonthLong);
      SetMinMax(ref formatSpecifier, "MMM", DateTimeFormatLength.MinMonthMid, DateTimeFormatLength.MaxMonthMid);
      SetMinMax(ref formatSpecifier, "MM", 2, 2);
      SetMinMax(ref formatSpecifier, "M", 1, 2);

      SetMinMax(ref formatSpecifier, "F", 0, 1);

      SetMinMax(ref formatSpecifier, "ss", 2, 2);
      SetMinMax(ref formatSpecifier, "s", 1, 2);

      // interpreted as a standard date and time format specifier
      SetMinMax(ref formatSpecifier, "K", 0, 6);

      // signed offset of the local operating system's time zone from UTC,
      SetMinMax(ref formatSpecifier, "zzz", 6, 6);
      SetMinMax(ref formatSpecifier, "zz", 3, 3);
      SetMinMax(ref formatSpecifier, "z", 2, 3);

      // AM / PM
      SetMinMax(ref formatSpecifier, "tt", DateTimeFormatLength.MinDesignator, DateTimeFormatLength.MaxDesignator);
    }

    public int MaxLength { get; private set; }

    public int MinLength { get; private set; }

    public bool NamedDate { get; }

    private void SetMinMax(ref string format, string search, int minLen, int maxLen)
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