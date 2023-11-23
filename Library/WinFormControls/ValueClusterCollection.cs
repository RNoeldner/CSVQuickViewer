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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace CsvTools
{
  public sealed class ValueClusterCollection : ICollection<ValueCluster>
  {
    private const long cTicksPerGroup = TimeSpan.TicksPerMinute * 30;
    private readonly IList<ValueCluster> m_ValueClusters = new List<ValueCluster>();

    public int Count => m_ValueClusters.Count;

    public bool IsReadOnly => m_ValueClusters.IsReadOnly;
    private ValueCluster lastValueCluster = new ValueCluster("Dummy", string.Empty, int.MaxValue, null);

    public BuildValueClustersResult ReBuildValueClusters(DataTypeEnum type, in ICollection<object> values, in string escapedName, bool isActive, int maxNumber)
    {
      if (values is null)
        throw new ArgumentNullException(nameof(values));

      if (maxNumber < 1 || maxNumber > 500)
        maxNumber = 500;

      if (isActive)
        ClearNotActive();
      else
        Clear();

      try
      {
        if (type == DataTypeEnum.String || type == DataTypeEnum.Guid  || type == DataTypeEnum.Boolean)
        {
          var m_CountNull = 0;
          var typedValues = new List<string>();
          foreach (var obj in values)
          {
            if (obj is DBNull || obj == null)
            {
              m_CountNull++;
              continue;
            }
            var str = obj.ToString();
            if (string.IsNullOrEmpty(str))
            {
              m_CountNull++;
              continue;
            }
            typedValues.Add(str);
          }
          AddValueClusterNull(escapedName, m_CountNull);
          return BuildValueClustersString(typedValues, escapedName, maxNumber);
        }
        if (type == DataTypeEnum.DateTime)
        {
          var m_CountNull = 0;
          var typedValues = new List<DateTime>();
          foreach (var obj in values)
          {
            if (obj is DBNull || obj == null)
            {
              m_CountNull++;
              continue;
            }
            if (obj is DateTime value)
              typedValues.Add(value);
            else
              m_CountNull++;

          }
          AddValueClusterNull(escapedName, m_CountNull);
          return BuildValueClustersDate(typedValues, escapedName, maxNumber);
        }
        else if (type == DataTypeEnum.Integer  || type == DataTypeEnum.Numeric || type == DataTypeEnum.Double)
        {
          var m_CountNull = 0;
          var typedValues = new List<double>();
          foreach (var obj in values)
          {
            if (obj is DBNull || obj == null)
            {
              m_CountNull++;
              continue;
            }
            try
            {
              typedValues.Add(Math.Floor(Convert.ToDouble(obj, CultureInfo.CurrentCulture) * 1000d) / 1000d);
            }
            catch
            {
              m_CountNull++;
            }
          }
          AddValueClusterNull(escapedName, m_CountNull);
          return BuildValueClustersNumeric(typedValues, escapedName, maxNumber, type != DataTypeEnum.Integer);
        }
        else
          return BuildValueClustersResult.WrongType;
      }
      catch (Exception ex)
      {
        Logger.Error(ex);
        return BuildValueClustersResult.Error;
      }
    }

    /// <summary>
    ///   Gets the active value cluster.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ValueCluster> GetActiveValueCluster() =>
      m_ValueClusters.Where(value => !string.IsNullOrEmpty(value.Display) && value.Active);

    /// <summary>
    ///   Builds the value clusters date.
    /// </summary>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersDate(in ICollection<DateTime> values, in string escapedName, int max)
    {
      // Get the distinct values and their counts
      var clusterYear = new HashSet<int>();
      var clusterMonth = new HashSet<DateTime>();
      var clusterDay = new HashSet<DateTime>();
      var clusterHour = new HashSet<long>();

      foreach (var value in values)
      {
        if (clusterHour.Count <= max)
          clusterHour.Add(value.TimeOfDay.Ticks / cTicksPerGroup);
        if (clusterDay.Count <= max)
          clusterDay.Add(value.Date);
        if (clusterMonth.Count <= max)
          clusterMonth.Add(new DateTime(value.Year, value.Month, 1));
        clusterYear.Add(value.Year);

        // if we have more than the maximum entries stop, no value filter will be used
        if (clusterYear.Count <= max)
          continue;
        return BuildValueClustersResult.TooManyValues;
      }

      if (clusterYear.Count == 0)
        return BuildValueClustersResult.NoValues;
      var desiredSize = (values.Count * 3) / (max *2);
      if (clusterDay.Count == 1)
      {
        clusterHour.Add(clusterHour.Min() -1);
        clusterHour.Add(clusterHour.Max() +1);
        foreach (var dic in clusterHour.OrderBy(x => x))
          AddValueClusterDateTime(escapedName, (dic * cTicksPerGroup).GetTimeFromTicks(), ((dic + 1) * cTicksPerGroup).GetTimeFromTicks(), values, DateTimeRange.Hours, desiredSize);
      }
      else if (clusterDay.Count < max)
      {
        clusterDay.Add(clusterDay.Min().AddDays(-1));
        clusterDay.Add(clusterDay.Max().AddDays(+1));
        foreach (var dateTime in clusterDay.OrderBy(x => x))
          AddValueClusterDateTime(escapedName, dateTime, dateTime.AddDays(1), values, DateTimeRange.Days, desiredSize);
      }
      else if (clusterMonth.Count < max)
      {
        clusterMonth.Add(clusterDay.Min().AddMonths(-1));
        clusterMonth.Add(clusterDay.Max().AddMonths(+1));
        foreach (var dateTime in clusterMonth.OrderBy(x => x))
          AddValueClusterDateTime(escapedName, dateTime, dateTime.AddMonths(1), values, DateTimeRange.Month, desiredSize);
      }
      else
      {
        clusterYear.Add(clusterYear.Max() +1);
        foreach (var year in clusterYear.OrderBy(x => x))
          AddValueClusterDateTime(escapedName, new DateTime(year, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), new DateTime(year + 1, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), values, DateTimeRange.Years, desiredSize);
      }


      return BuildValueClustersResult.ListFilled;
    }

    private enum DateTimeRange { Hours, Days, Month, Years }

    private void AddValueClusterDateTime(in string escapedName, DateTime from, DateTime to, ICollection<DateTime> values, DateTimeRange displayType, int desiredSize)
    {
      // Do not add if there is a cluster existing that spans the new value   [ [ ]  ]
      // Do not add if there is a cluster existing that spans the new value     [ ]  ]
      // Do not add if there is a cluster existing that spans the new value   [ [ ]
      // Do not add if there is a cluster existing that spans the new value     [ ]
      if (!m_ValueClusters.Any(x => x.Start != null && (DateTime) x.Start <= from && (DateTime) (x.End ?? DateTime.MaxValue) >= to))
      {
        var count = values.Count(x => x >= from && x<to);
        if (count >0)
        {
          // Combine buckets if the last and the current do not have many values
          if (lastValueCluster.Count + count < desiredSize && lastValueCluster.Start is DateTime lastFrom)
          {
            from = lastFrom;
            count += lastValueCluster.Count;

            // remove the last cluster it will be included with this new one
            m_ValueClusters.Remove(lastValueCluster);
          }
          string display = string.Empty;
          switch (displayType)
          {
            case DateTimeRange.Hours:
              display=$"{from:t} - {to:t}";
              break;
            case DateTimeRange.Days:
              if (to == from.AddDays(1))
                display=$"{from:d}";
              else
                display=$"{from:d} - {to:d}";
              break;
            case DateTimeRange.Month:
              if (to == from.AddMonths(1))
                display=$"{from:Y}";
              else
                display=$"{from:Y} - {to:Y}";
              break;
            case DateTimeRange.Years:
              if (to == from.AddYears(1))
                display=$"{from:yyyy}";
              else
                display=$"{from:yyyy} - {to:yyyy}";
              break;
          }
          lastValueCluster = new ValueCluster(display, $"({escapedName} >= #{from.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture)}# AND {escapedName} < #{to.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture)}#)",
           count, from, to);
          m_ValueClusters.Add(lastValueCluster);
        }
      }
    }

    /// <summary>
    ///   Builds the value clusters date.
    /// </summary>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersNumeric(in ICollection<double> values, in string escapedName, int max, in bool decimals)
    {
      // Get the distinct values and their counts
      var clusterFractions = new HashSet<double>();
      var clusterOne = new HashSet<long>();
      var hasFactions = false;

      foreach (var value in values)
      {
        if (clusterFractions.Count <= max && decimals)
        {
          hasFactions |= value % 1 != 0;
          clusterFractions.Add(Math.Floor(value * 10d) / 10d);
        }
        var key = Convert.ToInt64(value, CultureInfo.CurrentCulture);
        if (clusterOne.Count <= max)
          clusterOne.Add(key);
      }

      if (clusterOne.Count == 0)
        return BuildValueClustersResult.NoValues;

      if (decimals && hasFactions && clusterFractions.Count < max && clusterFractions.Count > 0)
      {
        clusterFractions.Add(clusterFractions.Max(x => x)+.1);
        clusterFractions.Add(clusterFractions.Min(x => x)-.1);
        foreach (var minValue in clusterFractions.OrderBy(x => x))
        {
          var maxValue = minValue + .1;
          var existing = false;
          try
          {
            existing = m_ValueClusters.Any(x => (long) (x.Start ?? long.MinValue) <= minValue && (long) (x.End ?? long.MaxValue) >= maxValue);
          }
          catch
          {
          }
          if (!existing)
          {
            var count = values.Count(value => value >= minValue && value < maxValue);
            if (count>0)
              Add(new ValueCluster($"{minValue:F1} - {maxValue:F1}", // Fixed Point
                   string.Format(CultureInfo.InvariantCulture, "({0} >= {1:F1} AND {0} < {2:F1})", escapedName, minValue, maxValue),
                count, minValue, maxValue));
          }
        }
      }

      // Use Integer filter
      HashSet<long> fittingCluster;
      double factor;
      if (clusterOne.Count < max)
      {
        factor = 1;
        fittingCluster = clusterOne;
      }
      else
      {
        // Dynamic Factor
        var digits = (int) Math.Log10(values.Max() - values.Min());
        factor = Math.Pow(10, digits);

        var start = (long) (values.Min() / factor);
        var end = (long) (values.Max() / factor);
        while (end-start<(max*2)/3)
        {
          factor = factor/2;
          start = (long) (values.Min() / factor);
          end = (long) (values.Max() / factor);
        }
        fittingCluster = new HashSet<long>();
        for (long i = start; i<=end; i++)
          fittingCluster.Add(i);
      }
      fittingCluster.Add(fittingCluster.Max(x => x)+1);
      fittingCluster.Add(fittingCluster.Min(x => x)-1);

      var desiredSize = (values.Count * 3) / (max *2);
      foreach (var dic in fittingCluster.OrderBy(x => x))
      {
        var minValue = (long) (dic * factor);
        var maxValue = (long) (minValue + factor);
        var existing = false;
        try
        {
          existing = m_ValueClusters.Any(x => (long) (x.Start ?? long.MinValue) <= minValue && (long) (x.End ?? long.MaxValue) >= maxValue);
        }
        catch
        {
        }
        if (!existing)
        {
          if (factor > 1)
          {
            var count = values.Count(value => value >= minValue && value < maxValue);
            if (count>0)
            {
              // Combine buckets if teh last and the current do not have many values
              if (lastValueCluster.Count + count < desiredSize && lastValueCluster.Start is long lastMin)
              {
                minValue = lastMin;
                count += lastValueCluster.Count;
                // remove the last cluster it will be included with thie one
                m_ValueClusters.Remove(lastValueCluster);
              }
              lastValueCluster = new ValueCluster($"{minValue:N0} to {maxValue-1:N0}", string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", escapedName, minValue, maxValue), count, minValue, maxValue);
              m_ValueClusters.Add(lastValueCluster);

            }
          }
          else
          {
            var count = values.Count(value => value == minValue);
            if (count>0)
            {
              m_ValueClusters.Add(new ValueCluster($"{minValue:N0}",
                string.Format(CultureInfo.InvariantCulture, "{0} = {1}", escapedName, minValue),
                count, minValue, maxValue));
            }
          }
        }
      }
      return BuildValueClustersResult.ListFilled;
    }

    /// <summary>
    ///   Builds the data grid view column filter values.
    /// </summary>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersString(in IEnumerable<string> values, in string escapedName, int max)
    {
      // Get the distinct values and their counts
      var cluster = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var clusterOne = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var allow2 = true;
      var clusterTwo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var allow3 = true;
      var clusterThree = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var allow4 = true;
      var clusterFour = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

      foreach (var text in values)
      {
        if (cluster.Count <= max)
          cluster.Add(text);
        if (clusterOne.Count <= max)
          clusterOne.Add(text.Substring(0, 1));
        allow2 &= (text.Length>=2);
        allow3 &= (text.Length>=3);
        allow4 &= (text.Length>=4);

        if (allow2 && clusterTwo.Count <= max)
          clusterTwo.Add(text.Substring(0, 2));
        if (allow3 && clusterThree.Count <= max)
          clusterThree.Add(text.Substring(0, 3));
        if (allow4 && clusterFour.Count <= max)
          clusterFour.Add(text.Substring(0, 4));
      }
      if (cluster.Count == 0)
        return BuildValueClustersResult.NoValues;
      if (clusterOne.Count >max)
        return BuildValueClustersResult.TooManyValues;

      if (cluster.Count <= max)
      {
        foreach (var text in cluster.OrderBy(x => x))
        {
          if (!m_ValueClusters.Any(x => string.Equals(x.Start?.ToString() ?? string.Empty, text)))
          {
            lastValueCluster = new ValueCluster(text, $"({escapedName} = '{text.SqlQuote()}')", values.Count(dataRow => string.Equals(dataRow, text, StringComparison.OrdinalIgnoreCase)), text);
            m_ValueClusters.Add(lastValueCluster);
          }
        }
      }
      else
      {
        var clusterBegin = clusterOne;
        if (allow4 && clusterFour.Count <= max)
          clusterBegin = clusterFour;
        else if (allow3 && clusterThree.Count <= max)
          clusterBegin = clusterThree;
        else if (allow2 &&  clusterTwo.Count <= max)
          clusterBegin = clusterTwo;

        foreach (var text in clusterBegin.OrderBy(x => x))
        {
          if (!m_ValueClusters.Any(x => string.Equals(x.Start?.ToString() ?? string.Empty, text)))
          {
            lastValueCluster = new ValueCluster($"{text}…", $"({escapedName} LIKE '{text.SqlQuote()}%')", values.Count(x => x.StartsWith(text, StringComparison.OrdinalIgnoreCase)), text);
            m_ValueClusters.Add(lastValueCluster);
          }
        }
      }
      return BuildValueClustersResult.ListFilled;
    }

    private void AddValueClusterNull(in string escapedName, int count)
    {
      if (!m_ValueClusters.Any(x => x.Start is null) && count>0)
      {
        lastValueCluster = new ValueCluster(ColumnFilterLogic.OperatorIsNull, string.Format($"({escapedName} IS NULL)"), count, null);
        m_ValueClusters.Add(lastValueCluster);
      }
    }

    /// <summary>
    /// Removes all not active Cluster 
    /// </summary>
    private void ClearNotActive()
    {
      var keep = GetActiveValueCluster().ToArray();
      Clear();
      foreach (var item in keep)
        m_ValueClusters.Add(item);
    }

    public void Clear() => m_ValueClusters.Clear();
    public bool Contains(ValueCluster item) => m_ValueClusters.Contains(item);
    public void Add(ValueCluster item) => m_ValueClusters.Add(item);
    public void CopyTo(ValueCluster[] array, int arrayIndex) => m_ValueClusters.CopyTo(array, arrayIndex);
    public bool Remove(ValueCluster item) => m_ValueClusters.Remove(item);
    public IEnumerator<ValueCluster> GetEnumerator() => m_ValueClusters.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => m_ValueClusters.GetEnumerator();
  }
}