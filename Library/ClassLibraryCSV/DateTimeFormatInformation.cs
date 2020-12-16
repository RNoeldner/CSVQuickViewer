/*s
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
using System.Diagnostics;

namespace CsvTools
{
  [DebuggerDisplay("{minLength} - {maxLength}")]
  public class DateTimeFormatInformation
  {
    private readonly int maxLength;
    private readonly int minLength;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeFormatInformation"/> class.
    /// </summary>
    /// <param name="format">The format.</param>
    public DateTimeFormatInformation(string format)
    {
      minLength = format.Length;
      maxLength = format.Length;
      NamedDate = format.IndexOf("ddd", StringComparison.Ordinal) != -1 || format.IndexOf("MMM", StringComparison.Ordinal) != -1;


      format = SetMinMax(format, "dddd", DateTimeFormatLength.MinDayLong, DateTimeFormatLength.MaxDayLong);
      format = SetMinMax(format, "ddd", DateTimeFormatLength.MinDayMid, DateTimeFormatLength.MaxDayMid);
      format = SetMinMax(format, "dd", 2, 2);
      format = SetMinMax(format, "d", 1, 2);

      format = SetMinMax(format, "yyyy", 4, 4);
      format = SetMinMax(format, "yy", 2, 2);
      format = SetMinMax(format, "y", 1, 2);

      format = SetMinMax(format, "HH", 2, 2);
      format = SetMinMax(format, "H", 1, 2);
      format = SetMinMax(format, "hh", 2, 2);
      format = SetMinMax(format, "h", 1, 2);

      format = SetMinMax(format, "mm", 2, 2);
      format = SetMinMax(format, "m", 1, 2);

      format = SetMinMax(format, "MMMM", DateTimeFormatLength.MinMonthLong, DateTimeFormatLength.MaxMonthLong);
      format = SetMinMax(format, "MMM", DateTimeFormatLength.MinMonthMid, DateTimeFormatLength.MaxMonthMid);
      format = SetMinMax(format, "MM", 2, 2);
      format = SetMinMax(format, "M", 1, 2);

      format = SetMinMax(format, "F", 0, 1);

      format = SetMinMax(format, "ss", 2, 2);
      format = SetMinMax(format, "s", 1, 2);

      // interpreted as a standard date and time format specifier
      format = SetMinMax(format, "K", 0, 6);

      // signed offset of the local operating system's time zone from UTC,
      format = SetMinMax(format, "zzz", 6, 6);
      format = SetMinMax(format, "zz", 3, 3);
      format = SetMinMax(format, "z", 2, 3);

      // AM / PM
      SetMinMax(format, "tt", DateTimeFormatLength.MinDesignator, DateTimeFormatLength.MaxDesignator);
    }

    // public string Format { get => m_Format; }
    public int MaxLength => maxLength;
    public int MinLength => minLength;
    public bool NamedDate { get; }

    private static string SetMinMax(string format, string search, int minLength, int maxLength)
    {
      var pos = format.IndexOf(search, StringComparison.Ordinal);
      while (pos != -1)
      {
        minLength += minLength - search.Length;
        maxLength += maxLength - search.Length;
        format = format.Remove(pos, search.Length);
        pos = format.IndexOf(search, StringComparison.Ordinal);
      }
      return format;
    }

    public override bool Equals(object obj) => obj is DateTimeFormatInformation information && MaxLength==information.MaxLength && MinLength==information.MinLength && NamedDate==information.NamedDate;

    public override int GetHashCode()
    {
      var hashCode = -60281774;
      hashCode=hashCode*-1521134295+MaxLength.GetHashCode();
      hashCode=hashCode*-1521134295+MinLength.GetHashCode();
      hashCode=hashCode*-1521134295+NamedDate.GetHashCode();
      return hashCode;
    }
  }
}