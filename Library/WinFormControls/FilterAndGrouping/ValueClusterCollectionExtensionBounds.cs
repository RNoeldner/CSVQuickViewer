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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CsvTools
{
  /// <summary>
  /// Provides extension methods to build clusters of numeric, long, and date values
  /// with bounds based on value distribution.
  /// </summary>
#pragma warning disable MA0048 // File name must match type name
  public static partial class ValueClustersExtension
#pragma warning restore MA0048 // File name must match type name
  {

    private static (int nullCount, List<T> sortedList) StartProcess<T>(object[] objects, IClusterResolutionStrategy<T> strategy, IProgressWithCancellation progress) where T : struct, IComparable<T>
    {
      (int nullCount, var values) = MakeTypedValues(objects, strategy.Convert, progress);

      values.Sort();
      progress.Report(new ProgressInfo(
              "Combining values into groups with evenly spaced boundaries, resulting in groups that may contain different numbers of rows.",
              (long) (cTypedProgress * cMaxProgress)));

      return (nullCount, values);
    }


    /// <summary>
    /// Builds clusters from a list of double values. Clusters are based on value distribution
    /// and may combine small clusters depending on the 'combine' parameter.
    /// </summary>
    public static (int countNull, List<ValueCluster> clusters) BuildValueClustersDouble(this object[] objects, string escapedName,
          int max, bool combine, double maxSeconds, IProgressWithCancellation progress)
    {
      var strategy = new ClusterResolutionStrategyDouble();
      var stopwatch = Stopwatch.StartNew();
      (int nullCount, var values) = StartProcess(objects, strategy, progress);
      if (values.Count == 0)
        return (nullCount, new List<ValueCluster>());

      // Get the distinct values and their counts
      var smallFractionSet = new HashSet<double>();
      var smallIntegerSet = new HashSet<double>();

      foreach (var value in values)
      {
        CheckTimeout(stopwatch, maxSeconds, progress.CancellationToken);

        if (smallFractionSet.Count <= 10)
          smallFractionSet.Add(Math.Floor(value * 10d) / 10d);
        if (smallIntegerSet.Count <= max)
          smallIntegerSet.Add(Math.Floor(value));
        if (smallFractionSet.Count == 11 && smallIntegerSet.Count == max+1)
          break;
      }

      var startValue = values[0];
      var endValue = values[values.Count-1];

      List<double> startValues;
      // Ordered startValue values
      double factor;
      if (smallIntegerSet.Count == 1 && smallFractionSet.Count <= 10)
      {
        // Use Fractional filter but only if we have a small range
        factor = 0.1d;
        startValues=smallFractionSet.ToList();
      }
      else if (smallIntegerSet.Count < max || endValue <= startValue)
      {
        factor = 1;
        startValues=smallIntegerSet.ToList();
      }
      else
      {
        // Dynamic Factor, since we have multiples day lastStart and startValue can not be teh same
        double range = Math.Max(1.0, endValue - startValue);
        int digits = (int) Math.Floor(Math.Log10(range));
        factor = Math.Pow(10, digits);

        var start = Math.Floor(startValue / factor);
        var end = Math.Floor(endValue / factor);
        while (end - start < max * 2 / 3 && factor > 1)
        {
          if (factor > 10)
            factor = Math.Round(factor / 10.0) * 5;
          else
            factor = Math.Round(factor / 4.0) * 2;
          start = Math.Floor(startValue / factor);
          end = Math.Floor(endValue / factor);
        }
        var steps = (int) Math.Ceiling((endValue - startValue) / factor);
        startValues = Enumerable.Range(0, steps)
            .Select(i => startValue + i * factor)
            .TakeWhile(v => v < endValue)
            .ToList();
      }

      // Delegate to FinishProcess
      return (nullCount, FinishProcess(values,
          startValues,
          startValue => startValue + factor,
          new ClusterResolutionStrategyDouble(),
          escapedName,
          combine ? Math.Max(5, values.Count * 3 / (max * 2)) : 1,
          maxSeconds- stopwatch.Elapsed.TotalSeconds,
          progress
      ));
    }

    /// <summary>
    /// Builds clusters from a list of long values.
    /// Uses dynamic or fixed factor depending on value range and max cluster size.
    /// </summary>
    public static (int countNull, List<ValueCluster> clusters) BuildValueClustersLong(
     this object[] objects,
     string escapedName,
     int max,
     bool combine,
     double maxSeconds,
     IProgressWithCancellation progress)
    {
      var strategy = new ClusterResolutionStrategyLong();
      var stopwatch = Stopwatch.StartNew();
      (int nullCount, var values) = StartProcess(objects, strategy, progress);
      if (values.Count == 0)
        return (nullCount, new List<ValueCluster>());

      // Get the distinct values and their counts
      var distinct = new HashSet<long>();
      foreach (var value in values)
      {
        CheckTimeout(stopwatch, maxSeconds, progress.CancellationToken);
        if (distinct.Count <= max) distinct.Add(value); else break;
      }

      // Use Integer filter
      long factor;
      List<long> startValues;
      if (distinct.Count < max)
      {
        startValues = distinct.OrderBy(x => x).ToList();
        factor = 1;
      }
      else
      {
        var startValue = values[0];
        var endValue = values[values.Count-1];
        // Dynamic Factor
        var digits = (long) Math.Log10(endValue - startValue);
        factor = Convert.ToInt64(Math.Pow(10, digits));

        var start = (startValue / factor);
        var end = (endValue / factor);
        while (end - start < max * 2 / 3 && factor > 1)
        {
          if (factor > 10)
            factor = Convert.ToInt64(Math.Round(factor / 10.0) * 5);
          else
            factor = Convert.ToInt64(Math.Round(factor / 4.0) * 2);
          start = (startValue / factor);
          end = (endValue / factor);
        }
        startValues = Enumerable
              .Range(0, (int) Math.Ceiling((double) (endValue - startValue) / factor))
              .Select(i => startValue + (long) i * factor)
              .TakeWhile(v => v < endValue)
              .ToList();
      }

      // Delegate to FinishProcess
      return (nullCount, FinishProcess(values,
          startValues,
          startValue => startValue + factor,
          new ClusterResolutionStrategyLong(),
          escapedName,
          combine ? Math.Max(5, values.Count * 3 / (max * 2)) : 1,
          maxSeconds- stopwatch.Elapsed.TotalSeconds,
          progress
      ));

    }

    /// <summary>
    /// Builds clusters from a list of DateTime values.
    /// Clusters can be hourly, daily, monthly, yearly, or by decades depending on data.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static (int nullCount, List<ValueCluster> clusters) BuildValueClustersDate(
      this object[] objects, string escapedName,
        int max, bool combine, double maxSeconds, IProgressWithCancellation progress)
    {
      var strategy = new ClusterResolutionStrategyDateTime();
      var stopwatch = Stopwatch.StartNew();
      (int nullCount, var values) = StartProcess(objects, strategy, progress);
      if (values.Count == 0)
        return (nullCount, new List<ValueCluster>());

      var clusterHour = new HashSet<DateTime>();
      var clusterDay = new HashSet<DateTime>();
      var clusterMonth = new HashSet<DateTime>();
      var clusterYear = new HashSet<DateTime>();
      var clusterDecade = new HashSet<DateTime>();
      foreach (var value in values)
      {
        CheckTimeout(stopwatch, maxSeconds, progress.CancellationToken);
        if (clusterHour.Count<max)
          clusterHour.Add(new DateTime(value.Year, value.Month, value.Day, value.Hour, (value.Minute / 15) * 15, 0, DateTimeKind.Local));

        if (clusterDay.Count<max)
          clusterDay.Add(value.Date);

        if (clusterMonth.Count<max)
          clusterMonth.Add(new DateTime(value.Year, value.Month, 1,0,0,0,0, DateTimeKind.Local));

        if (clusterYear.Count<max)
          clusterYear.Add(new DateTime(value.Year, 1, 1, 0, 0, 0, 0, DateTimeKind.Local));

        if (clusterDecade.Count<max)
          clusterDecade.Add(new DateTime((value.Year / 10) * 10, 1, 1, 0, 0, 0, 0, DateTimeKind.Local));
        else
          throw new InvalidOperationException($"Cannot create more than {max} decade clusters from {values.Count} values.");
      }

      var desiredSize = combine ? Math.Max(5, values.Count * 3 / (max * 2)) : 1;


      // Define possible clustering strategies
      var clusterOptions = new (Func<bool> Condition, Func<DateTime, DateTime> Increment, IEnumerable<DateTime> Source)[]
      {
        (() => clusterDay.Count == 1 && clusterHour.Count < max, dt => dt.AddMinutes(15), clusterHour),
        (() => clusterDay.Count < max, dt => dt.AddDays(1), clusterDay),
        (() => clusterMonth.Count < max, dt => dt.AddMonths(1), clusterMonth),
        (() => clusterYear.Count < max, dt => dt.AddYears(1), clusterYear),
        (() => true, dt => dt.AddYears(10), clusterDecade), // fallback
      };
      var selected = clusterOptions.First(opt => opt.Condition());

      // Hourly
      return (nullCount, FinishProcess(values,
          selected.Source.OrderBy(x => x).ToList(),
          selected.Increment,
          strategy,
          escapedName, desiredSize,
          maxSeconds- stopwatch.Elapsed.TotalSeconds,
          progress
      ));
    }

    /// <summary>
    /// Processes sorted start values to build clusters, calling delegate functions for display, SQL formatting, and counting.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private static List<ValueCluster> FinishProcess<T>(
        List<T> allSortedValues,
        List<T> sortedStartValues,
        Func<T, T> nextEnd,
        IClusterResolutionStrategy<T> strategy,
        string escapedName,
        int desiredSize,
        double maxRemainingSeconds,
        IProgressWithCancellation progress) where T : struct, IComparable<T>
    {
      var valueClusterCollection = new List<ValueCluster>();
      var stopwatch = Stopwatch.StartNew();
      if (desiredSize<1)
        desiredSize=1;
      if (sortedStartValues.Count==0)
        throw new ArgumentException("Cannot be empty", nameof(sortedStartValues));
      var lastStart = sortedStartValues[sortedStartValues.Count-1];
      var lastEnd = nextEnd(lastStart);
      if (lastEnd.CompareTo(lastStart) <= 0)
        throw new InvalidOperationException("nextEnd must produce strictly increasing values.");
      sortedStartValues.Add(lastEnd);

      var percent = cTypedProgress*2;
      if (percent>1)
        throw new InvalidOperationException("Progress percent exceeded 100%.");
      var step = (1-percent) / sortedStartValues.Count;

      for (int i = 0; i < sortedStartValues.Count; i++)
      {
        var start = sortedStartValues[i];
        CheckTimeout(stopwatch, maxRemainingSeconds, progress.CancellationToken);

        // Determine initial bucket end
        var end = nextEnd(start);

        // Count items in this bucket
        int count = CountOptimized(allSortedValues, start, end);
        if (count == 0)
          continue;

        // Expand bucket until desired size is reached using a nested loop
        for (int j = i + 1; count < desiredSize && j < sortedStartValues.Count; j++)
        {
          end = nextEnd(sortedStartValues[j]);
          count = CountOptimized(allSortedValues, start, end);
        }

        percent += step;

        var info = strategy.FormatDisplay(start, end);

        // Progress reporting
        progress.Report(new ProgressInfo($"Adding group {info}", (long) (percent * cMaxProgress)));

        // Create and add cluster
        valueClusterCollection.Add(new ValueCluster(
            info,
            strategy.FormatSql(escapedName, start, end),
            count,
            start,
            end));
      }

      // Helper methods remain the same
      static int LowerBound(List<T> list, T value)
      {
        int left = 0;
        int right = list.Count;

        while (left < right)
        {
          int mid = (left + right) / 2;
          if (list[mid].CompareTo(value) < 0)
            left = mid + 1;
          else
            right = mid;
        }
        return left;
      }

      static int CountOptimized(List<T> list, T start, T end)
      {
        int startIndex = LowerBound(list, start); // first index where value >= start
        int endIndex = LowerBound(list, end);     // first index where value >= end

        return Math.Max(0, endIndex - startIndex);
      }

      return valueClusterCollection;
    }
  }
}
