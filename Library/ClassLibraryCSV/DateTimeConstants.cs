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

namespace CsvTools
{
  public static class DateTimeConstants
  {
    /// <summary>
    ///   A static value any time only value will have this date
    /// </summary>
    internal static readonly DateTime FirstDateTime = new DateTime(1899, 12, 30, 0, 0, 0, 0);    

    public static ICollection<string> CommonDateTimeFormats(string known)
    {
      var formatsTime = new HashSet<string>
      {
        CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern
          .ReplaceDefaults(CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, "/", CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator, ":"),
        (CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern)
          .ReplaceDefaults(CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, "/", CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator, ":"),
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
      foreach (var format in CommonTimeFormats())
        formatsTime.Add(format);
      // gte the existing data as well
      var parts = StringUtils.SplitByDelimiter(known);
      foreach (var format in parts)
        formatsTime.Add(format);

      return formatsTime;
    }

    public static ICollection<string> CommonTimeFormats()
    {
      var formatsTime = new HashSet<string>
      {
        CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern
          .ReplaceDefaults(CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, "/", CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator, ":"),
        CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern
          .ReplaceDefaults(CultureInfo.CurrentCulture.DateTimeFormat.DateSeparator, "/", CultureInfo.CurrentCulture.DateTimeFormat.TimeSeparator, ":"),
        "HH:mm:ss", "HH:mm", "h:mm tt","HH:mm:ss.FFF"};

      return formatsTime;
    }


  }
}