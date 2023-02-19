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
using System.Runtime.InteropServices;
using TimeZoneConverter;

namespace CsvTools
{
  /// <summary>
  /// Implementation of <see cref="TimeZoneChangeDelegate"/> that uses the .NET  <see cref="TimeZoneInfo" /> and NuGet package TimeZoneConverter to be able to work on Inara or Windows TimeZone
  /// </summary>
  public static class StandardTimeZoneAdjust 
  {
    public const string cIdLocal = "(local)";
    private static readonly bool m_IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    
    private static TimeZoneInfo FindTimeZoneInfo(in string timeZone)
    {
      if (timeZone.Equals(cIdLocal, StringComparison.OrdinalIgnoreCase))
        return TimeZoneInfo.Local;
      else
        return m_IsWindows
          ? TimeZoneInfo.FindSystemTimeZoneById(
            TZConvert.TryIanaToWindows(timeZone, out var winSrc) ? winSrc : timeZone)
          : TimeZoneInfo.FindSystemTimeZoneById(TZConvert.TryWindowsToIana(timeZone, out var inaraSrc)
            ? inaraSrc
            : timeZone);
    }

    public static DateTime ChangeTimeZone(in DateTime input, in string srcTimeZone, in string destTimeZone,
      in Action<string>? handleWarning)
    {
      if (string.IsNullOrEmpty(srcTimeZone) || string.IsNullOrEmpty(destTimeZone) ||
          destTimeZone.Equals(srcTimeZone, StringComparison.OrdinalIgnoreCase))
        return input;
      try
      {
        return TimeZoneInfo.ConvertTime(input, FindTimeZoneInfo(srcTimeZone), FindTimeZoneInfo(destTimeZone));
      }
      catch (ArgumentException ex)
      {
        handleWarning?.Invoke(ex.Message);
        return input;
      }
    }
  }
}