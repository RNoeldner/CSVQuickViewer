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
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public sealed class ValueClusterCollection : ICollection<ValueCluster>
  {
    private const long cTicksPerGroup = TimeSpan.TicksPerMinute * 30;
    private readonly IList<ValueCluster> m_ValueClusters = new List<ValueCluster>();

    public int Count => m_ValueClusters.Count;

    public bool IsReadOnly => m_ValueClusters.IsReadOnly;
    private ValueCluster m_Last = new ValueCluster("Dummy", string.Empty, int.MaxValue, null);

    /// <summary>
    /// Parse the data to create culsters for these found values
    /// </summary>
    /// <param name="type">The type of the column</param>
    /// <param name="values">The values to look </param>
    /// <param name="escapedName">The escaped name of th column for the build SQL filter</param>
    /// <param name="isActive">Indicating if teh filter is currently active</param>
    /// <param name="maxNumber">Maximum number of clusters to return</param>
    /// <param name="combine">In case custers are every small combine close custers, the csuters are still bind with even margings</param>
    /// <param name="even">Build clusters that have roughly the same number of elements, the restuting borders can vary a lot, e:g. 1950-1980, 1980-1985, 1986, 1987</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public BuildValueClustersResult ReBuildValueClusters(DataTypeEnum type, in ICollection<object> values, in string escapedName, bool isActive, int maxNumber = 50, bool combine = true, bool even = false, CancellationToken cancellationToken = default)
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
          return BuildValueClustersString(typedValues, escapedName, maxNumber, cancellationToken);
        }
        else if (type == DataTypeEnum.DateTime)
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
          if (even)
            return BuildValueClustersDateEven(typedValues, escapedName, maxNumber, cancellationToken);
          else
            return BuildValueClustersDate(typedValues, escapedName, maxNumber, combine, cancellationToken);
        }
        else if (type == DataTypeEnum.Integer)
        {
          var m_CountNull = 0;
          var typedValues = new List<long>();
          foreach (var obj in values)
          {
            if (obj is DBNull || obj == null)
            {
              m_CountNull++;
              continue;
            }
            try
            {
              typedValues.Add(Convert.ToInt64(obj));
            }
            catch
            {
              m_CountNull++;
            }
          }
          AddValueClusterNull(escapedName, m_CountNull);
          if (even)
            return BuildValueClustersLongEven(typedValues, escapedName, maxNumber, cancellationToken);
          else
            return BuildValueClustersLong(typedValues, escapedName, maxNumber, combine, cancellationToken);
        }

        else if (type == DataTypeEnum.Numeric || type == DataTypeEnum.Double)
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
          if (even)
            return BuildValueClustersNumericEven(typedValues, escapedName, maxNumber, cancellationToken);
          else
            return BuildValueClustersNumeric(typedValues, escapedName, maxNumber, combine, cancellationToken);
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

    private BuildValueClustersResult BuildValueClustersDateEven(in ICollection<DateTime> values, in string escapedName, int max, CancellationToken cancellationToken)
    {
      var distinctValues = values.Distinct().OrderBy(x => x).ToList();
      var bucket = new List<(DateTime date, int count)>();
      var bucketCount = 0;
      var bucketSize = values.Count/ max;
      foreach (var value in distinctValues)
      {
        cancellationToken.ThrowIfCancellationRequested();
        var count = values.Count(x => x== value);
        bucket.Add((value, count));
        bucketCount += count;
        // progress to next bucket
        if (bucketCount > bucketSize)
        {

          var from = bucket.First().date;
          var to = bucket.Last().date;
          var display = $"{from:d} – {to:d}";
          if (from.Date == to.Date)
            display = $"{from:d} {from:t} – {to:t}";
          var existing = false;
          try
          {
            existing = m_ValueClusters.Any(x => x.Start is DateTime start &&  start <= from
                                              && x.Start is DateTime end  &&  end >= to);
          }
          catch
          {
          }
          if (!existing)
            m_ValueClusters.Add(new ValueCluster(display, $"({escapedName} >= #{from.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture)}# AND {escapedName} < #{to.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture)}#)",
             bucketCount, from, to));

          bucketCount=0;
        }
      }
      return BuildValueClustersResult.ListFilled;
    }

    private BuildValueClustersResult BuildValueClustersLongEven(in ICollection<long> values, in string escapedName, int max, CancellationToken cancellationToken)
    {
      var distinctValues = values.Distinct().OrderBy(x => x).ToList();
      var minValue = long.MinValue;
      var bucketCount = 0;
      var bucketSize = values.Count/ max;
      foreach (var value in distinctValues)
      {
        cancellationToken.ThrowIfCancellationRequested();
        var count = values.Count(x => x== value);
        bucketCount += count;
        // progress to next bucket
        if (bucketCount > bucketSize)
        {
          var display = $"{minValue:F1} – {value:F1}";
          var existing = false;
          try
          {
            existing = m_ValueClusters.Any(x => (long) (x.Start ?? long.MinValue) <= minValue && (long) (x.End ?? long.MaxValue) >= value);
          }
          catch
          {
          }
          if (!existing)
          {
            if (count>0)
              Add(new ValueCluster($"{minValue:F1} - {value:F1}", // Fixed Point
                   string.Format(CultureInfo.InvariantCulture, "({0} >= {1:F1} AND {0} < {2:F1})", escapedName, minValue, value),
                bucketCount, minValue, value));
          }
          minValue = value;
          bucketCount=count;
        }
      }
      return BuildValueClustersResult.ListFilled;
    }

    private BuildValueClustersResult BuildValueClustersNumericEven(in ICollection<double> values, in string escapedName, int max, CancellationToken cancellationToken)
    {
      var distinctValues = values.Select(x => Math.Floor(x * 10d) / 10d).Distinct().OrderBy(x => x).ToList();
      var minValue = double.MinValue;
      var bucketCount = 0;
      var bucketSize = values.Count/ max;
      foreach (var value in distinctValues)
      {
        cancellationToken.ThrowIfCancellationRequested();
        var count = values.Count(x => x>= value && x< value+.1);
        bucketCount += count;
        // progress to next bucket
        if (bucketCount > bucketSize)
        {
          var display = $"{minValue:F1} – {value:F1}";
          var existing = false;
          try
          {
            existing = m_ValueClusters.Any(x => (long) (x.Start ?? long.MinValue) <= minValue && (long) (x.End ?? long.MaxValue) >= value);
          }
          catch
          {
          }
          if (!existing)
          {
            Add(new ValueCluster($"{minValue:F1} - {value:F1}", // Fixed Point
                 string.Format(CultureInfo.InvariantCulture, "({0} >= {1:F1} AND {0} < {2:F1})", escapedName, minValue, value),
              bucketCount, minValue, value));
          }
          bucketCount=count;
          minValue = value;
        }
      }
      return BuildValueClustersResult.ListFilled;
    }

    /// <summary>
    ///   Builds the value clusters date.
    /// </summary>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersDate(in ICollection<DateTime> values, in string escapedName, int max, bool combine, CancellationToken cancellationToken)
    {
      // Get the distinct values and their counts
      var clusterYear = new HashSet<int>();
      var clusterMonth = new HashSet<DateTime>();
      var clusterDay = new HashSet<DateTime>();
      var clusterHour = new HashSet<long>();

      foreach (var value in values)
      {
        cancellationToken.ThrowIfCancellationRequested();
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
      var desiredSize = 1;
      if (combine)
      {
        desiredSize = (values.Count * 3) / (max *2);
        if (desiredSize < 5)
          desiredSize=5;
      }
      if (clusterDay.Count == 1)
      {
        clusterHour.Add(clusterHour.Min() -1);
        clusterHour.Add(clusterHour.Max() +1);
        foreach (var dic in clusterHour.OrderBy(x => x))
        {
          cancellationToken.ThrowIfCancellationRequested();
          AddValueClusterDateTime(escapedName, (dic * cTicksPerGroup).GetTimeFromTicks(), ((dic + 1) * cTicksPerGroup).GetTimeFromTicks(), values, DateTimeRange.Hours, desiredSize);
        }

      }
      else if (clusterDay.Count < max)
      {
        clusterDay.Add(clusterDay.Min().AddDays(-1));
        clusterDay.Add(clusterDay.Max().AddDays(+1));
        foreach (var dateTime in clusterDay.OrderBy(x => x))
        {
          cancellationToken.ThrowIfCancellationRequested();
          AddValueClusterDateTime(escapedName, dateTime, dateTime.AddDays(1), values, DateTimeRange.Days, desiredSize);
        }
      }
      else if (clusterMonth.Count < max)
      {
        clusterMonth.Add(clusterDay.Min().AddMonths(-1));
        clusterMonth.Add(clusterDay.Max().AddMonths(+1));
        foreach (var dateTime in clusterMonth.OrderBy(x => x))
        {
          cancellationToken.ThrowIfCancellationRequested();
          AddValueClusterDateTime(escapedName, dateTime, dateTime.AddMonths(1), values, DateTimeRange.Month, desiredSize);
        }
      }
      else
      {
        clusterYear.Add(clusterYear.Max() +1);
        foreach (var year in clusterYear.OrderBy(x => x))
        {
          cancellationToken.ThrowIfCancellationRequested();
          AddValueClusterDateTime(escapedName, new DateTime(year, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), new DateTime(year + 1, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), values, DateTimeRange.Years, desiredSize);
        }
      }


      return BuildValueClustersResult.ListFilled;
    }

    private enum DateTimeRange { Hours, Days, Month, Years }

    private void AddValueClusterDateTime(in string escapedName, DateTime from, DateTime to, ICollection<DateTime> values, DateTimeRange displayType, int desiredSize = int.MaxValue)
    {
      // Do not add if there is a cluster existing that spans the new value   [ [ ]  ]
      // Do not add if there is a cluster existing that spans the new value     [ ]  ]
      // Do not add if there is a cluster existing that spans the new value   [ [ ]
      // Do not add if there is a cluster existing that spans the new value     [ ]

      var existing = false;
      try
      {
        existing = m_ValueClusters.Any(x => x.Start is DateTime start &&  start <= from
                                          && x.Start is DateTime end  &&  end >= to);
      }
      catch
      {
      }
      if (!existing)
      {
        var count = values.Count(x => x >= from && x<to);
        if (count >0)
        {
          // Combine buckets if the last and the current do not have many values
          if (m_Last.Count + count < desiredSize && m_Last.Start is DateTime lastFrom)
          {
            from = lastFrom;
            count += m_Last.Count;

            // remove the last cluster it will be included with this new one
            m_ValueClusters.Remove(m_Last);
          }
          string display = string.Empty;
          switch (displayType)
          {
            case DateTimeRange.Hours:
              display=$"{from:t} – {to:t}";
              break;
            case DateTimeRange.Days:
              if (to == from.AddDays(1))
                display=$"{from:d}";
              else
                display=$"{from:d} – {to:d}";
              break;
            case DateTimeRange.Month:
              if (to == from.AddMonths(1))
                display=$"{from:Y}";
              else
                display=$"{from:Y} – {to:Y}";
              break;
            case DateTimeRange.Years:
              if (to == from.AddYears(1))
                display=$"{from:yyyy}";
              else
                display=$"{from:yyyy} – {to:yyyy}";
              break;
          }
          m_Last = new ValueCluster(display, $"({escapedName} >= #{from.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture)}# AND {escapedName} < #{to.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture)}#)",
           count, from, to);
          m_ValueClusters.Add(m_Last);
        }
      }
    }

    /// <summary>
    ///   Builds the value clusters date.
    /// </summary>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersNumeric(in ICollection<double> values, in string escapedName, int max, bool combine, CancellationToken cancellationToken)
    {
      // Get the distinct values and their counts
      var clusterFractions = new HashSet<double>();
      var clusterOne = new HashSet<long>();
      var hasFactions = false;

      foreach (var value in values)
      {
        cancellationToken.ThrowIfCancellationRequested();
        if (clusterFractions.Count <= max)
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

      if (hasFactions && clusterFractions.Count < max && clusterFractions.Count > 0)
      {
        clusterFractions.Add(clusterFractions.Max(x => x)+.1);
        clusterFractions.Add(clusterFractions.Min(x => x)-.1);
        foreach (var minValue in clusterFractions.OrderBy(x => x))
        {
          cancellationToken.ThrowIfCancellationRequested();
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
        while (end-start<(max*2)/3 && factor>1)
        {
          if (factor > 10)
            factor = Math.Round(factor / 10.0) * 5;
          else
            factor = Math.Round(factor / 4.0) * 2;
          start = (long) (values.Min() / factor);
          end = (long) (values.Max() / factor);
        }
        fittingCluster = new HashSet<long>();
        for (long i = start; i<=end; i++)
          fittingCluster.Add(i);
      }
      fittingCluster.Add(fittingCluster.Max(x => x)+1);
      fittingCluster.Add(fittingCluster.Min(x => x)-1);

      var desiredSize = 1;
      if (combine)
      {
        desiredSize = (values.Count * 3) / (max *2);
        if (desiredSize < 5)
          desiredSize=5;
      }

      foreach (var dic in fittingCluster.OrderBy(x => x))
      {
        cancellationToken.ThrowIfCancellationRequested();

        var minValue = (long) (dic * factor);
        var maxValue = (long) (minValue + factor);

        var existing = false;
        try
        {
          existing = m_ValueClusters.Any(x => x.Start is long start &&  start <= minValue
                                            && x.Start is long end  &&  end >= maxValue);
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
              if (m_Last.Count + count < desiredSize && m_Last.Start is long lastMin)
              {
                minValue = lastMin;
                count += m_Last.Count;
                // remove the last cluster it will be included with thie one
                m_ValueClusters.Remove(m_Last);
              }
              m_Last = new ValueCluster($"[{minValue:N0},{maxValue:N0})", string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", escapedName, minValue, maxValue), count, minValue, maxValue);
              m_ValueClusters.Add(m_Last);

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
    private BuildValueClustersResult BuildValueClustersLong(in ICollection<long> values, in string escapedName, int max, bool combine, CancellationToken cancellationToken)
    {
      // Get the distinct values and their counts      
      var clusterOne = new HashSet<long>();

      foreach (var value in values)
      {
        cancellationToken.ThrowIfCancellationRequested();
        if (clusterOne.Count <= max)
          clusterOne.Add(value);
      }

      if (clusterOne.Count == 0)
        return BuildValueClustersResult.NoValues;

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
        while (end-start<(max*2)/3 && factor>1)
        {
          if (factor > 10)
            factor = Math.Round(factor / 10.0) * 5;
          else
            factor = Math.Round(factor / 4.0) * 2;
          start = (long) (values.Min() / factor);
          end = (long) (values.Max() / factor);
        }
        fittingCluster = new HashSet<long>();
        for (long i = start; i<=end; i++)
          fittingCluster.Add(i);
      }
      fittingCluster.Add(fittingCluster.Max(x => x)+1);
      fittingCluster.Add(fittingCluster.Min(x => x)-1);

      var desiredSize = 1;
      if (combine)
      {
        desiredSize = (values.Count * 3) / (max *2);
        if (desiredSize < 5)
          desiredSize=5;
      }

      foreach (var dic in fittingCluster.OrderBy(x => x))
      {
        cancellationToken.ThrowIfCancellationRequested();
        var minValue = (long) (dic * factor);
        var maxValue = (long) (minValue + factor);

        var existing = false;
        try
        {
          existing = m_ValueClusters.Any(x => x.Start is long start &&  start <= minValue
                                            && x.Start is long end  &&  end >= maxValue);
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
              if (m_Last.Count + count < desiredSize && m_Last.Start is long lastMin)
              {
                minValue = lastMin;
                count += m_Last.Count;
                // remove the last cluster it will be included with thie one
                m_ValueClusters.Remove(m_Last);
              }
              m_Last = new ValueCluster($"[{minValue:N0},{maxValue:N0})", string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", escapedName, minValue, maxValue), count, minValue, maxValue);
              m_ValueClusters.Add(m_Last);

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
    private BuildValueClustersResult BuildValueClustersString(in IEnumerable<string> values, in string escapedName, int max, CancellationToken cancellationToken)
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
        cancellationToken.ThrowIfCancellationRequested();
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
            m_Last = new ValueCluster(text, $"({escapedName} = '{text.SqlQuote()}')", values.Count(dataRow => string.Equals(dataRow, text, StringComparison.OrdinalIgnoreCase)), text);
            m_ValueClusters.Add(m_Last);
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
          cancellationToken.ThrowIfCancellationRequested();
          if (!m_ValueClusters.Any(x => string.Equals(x.Start?.ToString() ?? string.Empty, text)))
          {
            m_Last = new ValueCluster($"{text}…", $"({escapedName} LIKE '{text.SqlQuote()}%')", values.Count(x => x.StartsWith(text, StringComparison.OrdinalIgnoreCase)), text);
            m_ValueClusters.Add(m_Last);
          }
        }
      }
      return BuildValueClustersResult.ListFilled;
    }

    private void AddValueClusterNull(in string escapedName, int count)
    {
      if (!m_ValueClusters.Any(x => x.Start is null) && count>0)
      {
        m_Last = new ValueCluster(ColumnFilterLogic.OperatorIsNull, string.Format($"({escapedName} IS NULL)"), count, null);
        m_ValueClusters.Add(m_Last);
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