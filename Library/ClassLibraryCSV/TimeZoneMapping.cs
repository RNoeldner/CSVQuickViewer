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

using NodaTime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CsvTools
{
  public static class TimeZoneMapping
  {
    public const string cIdLocal = "(local)";
    public const string cUTC = "Etc/UTC";
    private const int c_MinSavingSeconds = 90; // 15 Min

    private static readonly IDictionary<string, string> m_Mapping = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private static bool m_NeedsInit = true;

    /// <summary>
    /// Gets a list of all time zones that have been used.
    /// </summary>
    /// <value>
    /// The used time zone ids.
    /// </value>
    private static IList<string> UsedTz { get; } = new List<string>();

    /// <summary>
    /// Gets the alternate names for the time zone
    /// </summary>
    /// <param name="timeZoneName">Name of the time zone.</param>
    /// <returns></returns>
    public static IEnumerable<string> AlternateTZNames(this string timeZoneName)
    {
      var dateTimeZone = GetTimeZone(timeZoneName);
      if (m_NeedsInit)
        InitMapping();
      var retList = new List<string>();
      foreach (var keyValue in m_Mapping)
        if (keyValue.Value == dateTimeZone.Id)
          retList.Add(keyValue.Key);
      return retList;
    }

    /// <summary>
    /// Converts the time from one time zone to another
    /// </summary>
    /// <param name="dateTimeSource">The date time in the source time zone.</param>
    /// <param name="sourceTZName">Name of the source tz.</param>
    /// <param name="destTZName">Name of the dest tz.</param>
    /// <returns>
    /// A Datetime value unspecified
    /// </returns>
    public static DateTime ConvertTime(this DateTime dateTimeSource, string sourceTZName, string destTZName)
    {
      if (string.IsNullOrEmpty(sourceTZName))
        return dateTimeSource;

      var sourceTimeZone = GetTimeZone(sourceTZName);
      var destTimeZone = GetTimeZone(destTZName);

      if (sourceTimeZone == destTimeZone)
        return dateTimeSource;

      var from = LocalDateTime.FromDateTime(dateTimeSource).InZoneLeniently(sourceTimeZone);
      return from.WithZone(destTimeZone).LocalDateTime.ToDateTimeUnspecified();
    }

    /// <summary>
    /// Converts the time from UTC to destination timezone
    /// </summary>
    /// <param name="dateTimeUTC">The date time UTC.</param>
    /// <param name="destTZName">The destination time zone name</param>
    /// <returns></returns>
    /// <exception cref="ConversionException"></exception>
    public static DateTime ConvertTimeUTC(this DateTime dateTimeUTC, string destTZName)
    {
      if (dateTimeUTC.Kind != DateTimeKind.Utc)
        dateTimeUTC = DateTime.SpecifyKind(dateTimeUTC, DateTimeKind.Utc);
      return Instant.FromDateTimeUtc(dateTimeUTC).InZone(GetTimeZone(destTZName)).ToDateTimeUnspecified();
    }

    public static string GetTimeZoneID(string timeZoneName) => GetTimeZone(timeZoneName).Id;

    /// <summary>
    /// Gets the time zone savings times in a year.
    /// </summary>
    /// <param name="timeZoneName">Name of the time zone.</param>
    /// <param name="year">The year.</param>
    /// <returns>Item1 is the start of Summertime (Spring), Item2 is the end of Summertime (Autum)</returns>
    /// TODO: Migarting to C# 7.0 Impoove this 
    public static Tuple<DateTime, DateTime> GetTransitionTimes(this string timeZoneName, int year)
    {
      foreach (var saving in IntervalsInYear(timeZoneName, year).Where(x => x.Savings.Seconds >= c_MinSavingSeconds))
        return new Tuple<DateTime, DateTime>(saving.Start.ToDateTimeUtc(), saving.End.ToDateTimeUtc());
      return null;
    }

    /// <summary>
    /// Gets the abbreviation of a timezone
    /// </summary>
    /// <param name="timeZoneName">Name of the time zone.</param>
    /// <returns></returns>
    public static string GetTZAbbreviation(this string timeZoneName)
    {
      var dateTimeZone = GetTimeZone(timeZoneName);
      if (m_NeedsInit)
        InitMapping();
      var matches = m_Mapping.Where(x => x.Value == dateTimeZone.Id && x.Key.Length == 3 && !int.TryParse(x.Key, out _)).Take(1).ToList();
      if (matches.Count != 0)
        return matches[0].Key;

      matches = m_Mapping.Where(x => x.Value == dateTimeZone.Id && x.Key.Length == 4 && !int.TryParse(x.Key, out _)).Take(1).ToList();
      if (matches.Count != 0)
        return matches[0].Key;

      foreach (var other in WithSameRule(timeZoneName, DateTime.Now.Year))
      {
        matches = m_Mapping.Where(x => x.Value == other && x.Key.Length == 3 && !int.TryParse(x.Key, out _)).Take(1).ToList();
        if (matches.Count != 0)
          return matches[0].Key;
      }

      // the intervals have names, pick an interval without offset
      var inter = IntervalsInYear(timeZoneName, DateTime.UtcNow.Year).FirstOrDefault(x => x.Savings.Seconds < c_MinSavingSeconds && !string.IsNullOrEmpty(x.Name));
      return inter != null ? inter.Name : dateTimeZone.Id;
    }

    /// <summary>
    /// Returns true if we have at least 10 minutes daylight savings
    /// </summary>
    /// <param name="timeZoneName">Name of the time zone.</param>
    /// <param name="dateTimeUTC">The date time UTC.</param>
    /// <returns>
    ///   <c>true</c> if [is daylight saving time] [the specified date time UTC]; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsDaylightSavingTime(this string timeZoneName, DateTime dateTimeUTC)
    {
      if (dateTimeUTC.Kind != DateTimeKind.Utc)
        dateTimeUTC = DateTime.SpecifyKind(dateTimeUTC, DateTimeKind.Utc);

      return GetTimeZone(timeZoneName).GetZoneIntervals(Instant.FromDateTimeUtc(dateTimeUTC), Instant.FromDateTimeUtc(dateTimeUTC.AddDays(1))).Any(x => x.Savings.Seconds >= c_MinSavingSeconds);
    }

    /// <summary>
    /// Returns a list of all windows TimeZoneInfos with the matching IANA identifier
    /// </summary>
    /// <returns>Enumeration of all </returns>
    public static IEnumerable<KeyValuePair<TimeZoneInfo, string>> MappedSystemTimeZone()
    {
      var ret = new Dictionary<TimeZoneInfo, string>();
      foreach (var winTimeZone in TimeZoneInfo.GetSystemTimeZones())
      {
        if (NodaTime.TimeZones.TzdbDateTimeZoneSource.Default.WindowsMapping.PrimaryMapping.TryGetValue(winTimeZone.Id, out var zoneID) &&
            DateTimeZoneProviders.Tzdb.Ids.Contains(zoneID))
          ret.Add(winTimeZone, zoneID);
      }
      return ret;
    }

    /// <summary>
    /// Returns the default UTC offset for a year.
    /// </summary>
    /// <param name="timeZoneName">Name of the time zone.</param>
    /// <param name="year">The year.</param>
    /// <returns></returns>
    public static string Offset(this string timeZoneName, int year)
    {
      var std = IntervalsInYear(timeZoneName, year).FirstOrDefault(x => x.Savings.Seconds < c_MinSavingSeconds);
      return std != null ? std.StandardOffset.ToString() : GetTimeZone(timeZoneName).MaxOffset.ToString();
    }

    /// <summary>
    /// Checks if the time zone does have daylight saving in this year
    /// </summary>
    /// <param name="timeZoneName">Name of the time zone.</param>
    /// <param name="year">The year.</param>
    /// <returns></returns>
    public static bool SupportsDaylightSavingTime(this string timeZoneName, int year) => IntervalsInYear(timeZoneName, year).Any(x => x.Savings.Seconds >= c_MinSavingSeconds);

    /// <summary>
    /// Returns a list of time zones that have the same rules for that year
    /// </summary>
    /// <param name="timeZoneName">Name of the time zone.</param>
    /// <param name="year">The year to check the rules.</param>
    /// <returns></returns>
    public static IEnumerable<string> WithSameRule(this string timeZoneName, int year)
    {
      var dateTimeZone = GetTimeZone(timeZoneName);
      var saving = IntervalsInYear(timeZoneName, year).OrderBy(x => x.HasStart ? x.Start.ToDateTimeUtc() : new DateTime(0, DateTimeKind.Utc)).ToList();

      foreach (var otherZoneID in DateTimeZoneProviders.Tzdb.Ids)
      {
        if (otherZoneID == dateTimeZone.Id)
          continue;

        var otherSaving = IntervalsInYear(otherZoneID, year).OrderBy(x => x.HasStart ? x.Start.ToDateTimeUtc() : new DateTime(0, DateTimeKind.Utc)).ToList();

        if (otherSaving.Count() != saving.Count())
          continue;

        var same = true;
        for (var i = 0; i < otherSaving.Count() && same; i++)
        {
          same = (saving[i].Name.Equals(otherSaving[i].Name, StringComparison.OrdinalIgnoreCase) && saving[i].StandardOffset.Equals(otherSaving[i].StandardOffset));
          if (same && saving[i].HasStart && otherSaving[i].HasStart)
            same = (saving[i].Start == otherSaving[i].Start);
          if (same && saving[i].HasEnd && otherSaving[i].HasEnd)
            same = (saving[i].End == otherSaving[i].End);
        }
        if (same)
          yield return otherZoneID;
      }
    }

    private static void AddIfNew<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
    {
      if (!dic.ContainsKey(key))
        dic.Add(key, value);
    }

    /// <summary>
    /// Gets the date time zone by its IANA identifier in addition all windows timezones will be recognized and anything mapped in the mapping file
    /// </summary>
    /// <param name="timeZoneName">The time zone identifier.</param>
    /// <returns></returns>
    private static DateTimeZone GetTimeZone(string timeZoneName)
    {

      // special handling of local timezone, it will be determined each time as this might change
      if (timeZoneName == cIdLocal)
        timeZoneName = TimeZoneInfo.Local.Id;

      string tzdbID;
      if (DateTimeZoneProviders.Tzdb.Ids.Contains(timeZoneName))
      {
        tzdbID = timeZoneName;
      }
      else
      {
        if (m_NeedsInit)
          InitMapping();
        if (!m_Mapping.TryGetValue(timeZoneName, out tzdbID))
          throw new ConversionException($"Time zone adjustment not calculated since time zone {timeZoneName} is unknown.");
      }

      if (!UsedTz.Contains(tzdbID))
        UsedTz.Add(tzdbID);

      return DateTimeZoneProviders.Tzdb[tzdbID];
    }

    private static void InitMapping()
    {
      m_NeedsInit = false;
      foreach (var winTZ in MappedSystemTimeZone())
      {
        m_Mapping.AddIfNew(winTZ.Key.Id, winTZ.Value);
        m_Mapping.AddIfNew(winTZ.Key.DisplayName, winTZ.Value);
      }

      using (var reader = FileSystemUtils.GetStreamReaderForFileOrResource("TZMapping.txt"))
      {
        if (reader == null)
          return;
        while (!reader.EndOfStream)
        {
          var entry = reader.ReadLine();
          if (string.IsNullOrEmpty(entry) || entry[0] == '#')
            continue;
          var tab = entry.IndexOf('\t');
          if (tab <= 0)
            continue;
          var timeZoneID = entry.Substring(tab + 1);
          if (DateTimeZoneProviders.Tzdb.Ids.Contains(timeZoneID))
            m_Mapping.AddIfNew(entry.Substring(0, tab), timeZoneID);
          else
            Debug.WriteLine("Mapping target not found: " + entry);
        }
      }
    }

    private static IEnumerable<NodaTime.TimeZones.ZoneInterval> IntervalsInYear(this string timeZoneName, int year)
    {
      var dateTimeZone = GetTimeZone(timeZoneName);
      return dateTimeZone.GetZoneIntervals(
                  new LocalDateTime(year, 1, 1, 0, 0).InZoneLeniently(dateTimeZone).ToInstant(),
                  new LocalDateTime(year + 1, 1, 1, 0, 0).InZoneLeniently(dateTimeZone).ToInstant());
    }
  }
}