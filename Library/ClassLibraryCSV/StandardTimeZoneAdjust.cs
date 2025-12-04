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
using System.Runtime.InteropServices;
using TimeZoneConverter;

namespace CsvTools;

/// <summary>
/// Implementation of <see cref="TimeZoneChangeDelegate"/> that uses the .NET  <see cref="TimeZoneInfo" /> and NuGet package TimeZoneConverter to be able to work on Inara or Windows TimeZone
/// </summary>
public static class StandardTimeZoneAdjust 
{
  /// <summary>
  /// Representation for current system timezone
  /// </summary>
  public const string cIdLocal = "(local)";

  private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    
  private static TimeZoneInfo FindTimeZoneInfo(string timeZone)
  {
    if (timeZone.Equals(cIdLocal, StringComparison.OrdinalIgnoreCase))
      return TimeZoneInfo.Local;
    return IsWindows
      ? TimeZoneInfo.FindSystemTimeZoneById(
        TZConvert.TryIanaToWindows(timeZone, out var winSrc) ? winSrc : timeZone)
      : TimeZoneInfo.FindSystemTimeZoneById(TZConvert.TryWindowsToIana(timeZone, out var inaraSrc)
        ? inaraSrc
        : timeZone);
  }

  /// <summary>
  /// Converts a <see cref="DateTime"/> from a source time zone to a destination time zone.
  /// </summary>
  /// <param name="input">The source <see cref="DateTime"/> value.</param>
  /// <param name="sourceTimeZone">The identifier of the source time zone.</param>
  /// <param name="destinationTimeZone">The identifier of the target time zone.</param>
  /// <param name="handleWarning">
  /// Optional action invoked when a warning occurs, for example if a time zone is not recognized.
  /// </param>
  /// <returns>
  /// The <see cref="DateTime"/> converted to the destination time zone, or the original value if conversion fails.
  /// </returns>
  public static DateTime ChangeTimeZone(in DateTime input, string sourceTimeZone, string destinationTimeZone,
    Action<string>? handleWarning)
  {
    if (string.IsNullOrEmpty(sourceTimeZone) || string.IsNullOrEmpty(destinationTimeZone) ||
        destinationTimeZone.Equals(sourceTimeZone, StringComparison.OrdinalIgnoreCase))
      return input;
    try
    {
      return TimeZoneInfo.ConvertTime(input, FindTimeZoneInfo(sourceTimeZone), FindTimeZoneInfo(destinationTimeZone));
    }
    catch (ArgumentException ex)
    {
      handleWarning?.Invoke(ex.Message);
      return input;
    }
  }
}