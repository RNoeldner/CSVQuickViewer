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
using System.Globalization;

namespace CsvTools
{
  /// <summary>
  ///   Static class that stores the length of named date parts for the current culture  and English
  /// </summary>
  public static class DateTimeFormatLength
  {
    static DateTimeFormatLength()
    {
      MaxDesignator = 2;
      MaxDayLong = int.MinValue;
      MaxDayMid = int.MinValue;
      MaxMonthLong = int.MinValue;
      MaxMonthMid = int.MinValue;

      MinDesignator = 2;
      MinDayLong = int.MaxValue;
      MinDayMid = int.MaxValue;
      MinMonthLong = int.MaxValue;
      MinMonthMid = int.MaxValue;

      for (var weekday = 0; weekday < 7; weekday++)
      {
        var currentCulture = string.Format(CultureInfo.CurrentCulture, "{0:dddd}", DateTime.Now.AddDays(weekday));
        var invariantCulture = string.Format(CultureInfo.InvariantCulture, "{0:dddd}", DateTime.Now.AddDays(weekday));
        if (currentCulture.Length < MinDayLong)
          MinDayLong = currentCulture.Length;
        if (invariantCulture.Length < MinDayLong)
          MinDayLong = invariantCulture.Length;
        if (currentCulture.Length > MaxDayLong)
          MaxDayLong = currentCulture.Length;
        if (invariantCulture.Length > MaxDayLong)
          MaxDayLong = invariantCulture.Length;

        currentCulture = string.Format(CultureInfo.CurrentCulture, "{0:ddd}", DateTime.Now.AddDays(weekday));
        invariantCulture = string.Format(CultureInfo.InvariantCulture, "{0:ddd}", DateTime.Now.AddDays(weekday));
        if (currentCulture.Length < MinDayMid)
          MinDayMid = currentCulture.Length;
        if (invariantCulture.Length < MinDayMid)
          MinDayMid = invariantCulture.Length;
        if (currentCulture.Length > MaxDayMid)
          MaxDayMid = currentCulture.Length;
        if (invariantCulture.Length > MaxDayMid)
          MaxDayMid = invariantCulture.Length;
      }

      for (var month = 0; month < 12; month++)
      {
        var currentCulture = string.Format(CultureInfo.CurrentCulture, "{0:MMMM}", DateTime.Now.AddMonths(month));
        var invariantCulture = string.Format(CultureInfo.InvariantCulture, "{0:MMMM}", DateTime.Now.AddMonths(month));
        if (currentCulture.Length < MinMonthLong)
          MinMonthLong = currentCulture.Length;
        if (invariantCulture.Length < MinMonthLong)
          MinMonthLong = invariantCulture.Length;
        if (currentCulture.Length > MaxMonthLong)
          MaxMonthLong = currentCulture.Length;
        if (invariantCulture.Length > MaxMonthLong)
          MaxMonthLong = invariantCulture.Length;

        currentCulture = string.Format(CultureInfo.CurrentCulture, "{0:MMM}", DateTime.Now.AddMonths(month));
        invariantCulture = string.Format(CultureInfo.InvariantCulture, "{0:MMM}", DateTime.Now.AddMonths(month));
        if (currentCulture.Length < MinMonthMid)
          MinMonthMid = currentCulture.Length;
        if (invariantCulture.Length < MinMonthMid)
          MinMonthMid = invariantCulture.Length;
        if (currentCulture.Length > MaxMonthMid)
          MaxMonthMid = currentCulture.Length;
        if (invariantCulture.Length > MaxMonthMid)
          MaxMonthMid = invariantCulture.Length;
      }

      if (CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator.Length > MaxDesignator)
        MaxDesignator = CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator.Length;
      if (CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator.Length > MaxDesignator)
        MaxDesignator = CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator.Length;

      if (CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator.Length < MinDesignator)
        MinDesignator = CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator.Length;
      if (CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator.Length < MinDesignator)
        MinDesignator = CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator.Length;
    }

    public static int MaxDayLong { get; }

    public static int MaxDayMid { get; }

    public static int MaxDesignator { get; }

    public static int MaxMonthLong { get; }

    public static int MaxMonthMid { get; }

    public static int MinDayLong { get; }

    public static int MinDayMid { get; }

    public static int MinDesignator { get; }

    public static int MinMonthLong { get; }

    public static int MinMonthMid { get; }
  }
}