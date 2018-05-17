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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CsvTools
{
  public static class TimeZoneMapping
  {
    private static readonly Dictionary<string, string> m_Mapping =
      new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    private static bool m_NeedsInit = true;

    // Storing known TimeZones to speed up the process, this way the info does not have to be created from id
    private static readonly SortedDictionary<string, TimeZoneInfo> m_TimeZones =
      new SortedDictionary<string, TimeZoneInfo>(StringComparer.OrdinalIgnoreCase);

    public static IEnumerable<string> GetAlternateNames(TimeZoneInfo timeZoneInfo)
    {
      if (m_NeedsInit) InitMapping();

      foreach (var item in m_Mapping)
        if (item.Value == timeZoneInfo.Id && timeZoneInfo.Id != item.Key && timeZoneInfo.DisplayName != item.Key &&
            timeZoneInfo.DaylightName != item.Key)
          yield return item.Key;
    }

    public static string GetAbbreviation(this TimeZoneInfo selectedTimeZone)
    {
      // get a 3 letter abbreviation
      foreach (var item in GetAlternateNames(selectedTimeZone))
      {
        if (item.StartsWith("(", StringComparison.Ordinal))
          continue;
        if (int.TryParse(item, out _))
          continue;

        if (item.Length == 3)
        {
          return item;
        }
      }
      // or a 4 letter abbreviation
      foreach (var item in GetAlternateNames(selectedTimeZone))
      {
        if (item.StartsWith("(", StringComparison.Ordinal))
          continue;
        if (int.TryParse(item, out _))
          continue;

        if (item.Length == 4)
        {
          return item;
        }
      }
      // return the name in Windows
      return selectedTimeZone.DisplayName;
    }

    public static IEnumerable<TimeZoneInfo> WithSameRule(TimeZoneInfo timeZoneInfo, int year)
    {
      TimeZoneInfo.AdjustmentRule adjustmentRuleThis = null;
      foreach (var adjustment in timeZoneInfo.GetAdjustmentRules())
      {
        if (adjustment.DateStart.Year > year || adjustment.DateEnd.Year < year) continue;
        adjustmentRuleThis = adjustment;
        break;
      }

      foreach (var timeZoneInfoOther in TimeZoneInfo.GetSystemTimeZones())
      {
        if (timeZoneInfoOther.Id == timeZoneInfo.Id || timeZoneInfoOther.BaseUtcOffset != timeZoneInfo.BaseUtcOffset ||
            timeZoneInfoOther.SupportsDaylightSavingTime != timeZoneInfo.SupportsDaylightSavingTime) continue;
        TimeZoneInfo.AdjustmentRule adjustmentRuleOther = null;
        foreach (var adjustment in timeZoneInfoOther.GetAdjustmentRules())
        {
          if (adjustment.DateStart.Year > year || adjustment.DateEnd.Year < year) continue;
          adjustmentRuleOther = adjustment;
          break;
        }

        if (adjustmentRuleThis == null && adjustmentRuleOther == null)
          yield return timeZoneInfoOther;
        else if (adjustmentRuleThis != null && adjustmentRuleOther != null &&
                 adjustmentRuleThis.Equals(adjustmentRuleOther))
          yield return timeZoneInfoOther;
      }
    }

    public static TimeZoneInfo GetTimeZone(string timeZoneID)
    {
      if (m_NeedsInit) InitMapping();

      if (m_TimeZones.TryGetValue(timeZoneID, out var returnVal))
        return returnVal;

      if (!m_Mapping.TryGetValue(timeZoneID, out var tzID)) return null;
      returnVal = TimeZoneInfo.FindSystemTimeZoneById(tzID);
      m_TimeZones.Add(timeZoneID, returnVal);
      return returnVal;
    }

    private static void AddIfNew<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
    {
      if (!dic.ContainsKey(key))
        dic.Add(key, value);
    }

    private static void InitMapping()
    {
      m_NeedsInit = false;
      var systemIDs = new HashSet<string>();

      foreach (var tz in TimeZoneInfo.GetSystemTimeZones())
      {
        systemIDs.Add(tz.Id);
        m_Mapping.AddIfNew(tz.DisplayName, tz.Id);
        m_Mapping.AddIfNew(tz.Id, tz.Id);
        if (tz.SupportsDaylightSavingTime)
          m_Mapping.AddIfNew(tz.DaylightName, tz.Id);
      }

      var fileName = (FileSystemUtils.ExecutableDirectoryName() + "\\TZMapping.txt").LongPathPrefix();
      if (!File.Exists(fileName)) return;
      try
      {
        using (var reader = new StreamReader(fileName, true))
        {
          while (!reader.EndOfStream)
          {
            var entry = reader.ReadLine();
            if (string.IsNullOrEmpty(entry) || entry[0] == '#')
              continue;
            var tab = entry.IndexOf('\t');
            if (tab <= 0) continue;
            var windowsTimeZoneID = entry.Substring(tab + 1);
            if (systemIDs.Contains(windowsTimeZoneID))
              m_Mapping.AddIfNew(entry.Substring(0, tab), windowsTimeZoneID);
          }
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine("Error reading  " + fileName);
        Debug.WriteLine(ex.Message);
      }
    }
  }
}