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
  public static class ValueClusterCollectionExtensionBounds
  {
    /// <summary>
    /// Builds clusters from a list of double values. Clusters are based on value distribution
    /// and may combine small clusters depending on the 'combine' parameter.
    /// </summary>
    public static bool BuildValueClustersNumeric(this ValueClusterCollection valueClusterCollection, List<double> values, string escapedName,
          int max, bool combine, double maxSeconds, IProgressWithCancellation progress)
    {
      // Get the distinct values and their counts
      var clusterFractions = new HashSet<double>();
      var clusterOne = new HashSet<double>();
      var stopwatch = Stopwatch.StartNew();
      if (values.Count == 0)
        return false;
      values.Sort();
      foreach (var value in values)
      {
        if (stopwatch.Elapsed.TotalSeconds > maxSeconds)
          throw new TimeoutException("Preparing overview took too long.");
        progress.CancellationToken.ThrowIfCancellationRequested();
        if (clusterFractions.Count <= 10)
          clusterFractions.Add(Math.Floor(value * 10d) / 10d);
        if (clusterOne.Count <= max)
          clusterOne.Add(Math.Floor(value));
      }
      var endValue = values.Last();
      var startValue = values.First();
      List<double> startValues;
      // Ordered startValue values
      double factor;
      if (clusterOne.Count == 1 && clusterFractions.Count <= 10)
      {
        // Use Fractional filter but only if we have a small range
        factor = 0.1d;
        startValues=clusterFractions.OrderBy(x => x).ToList();
      }
      else if (clusterOne.Count < max || endValue <= startValue)
      {
        factor = 1;
        startValues=clusterOne.OrderBy(x => x).ToList();
      }
      else
      {
        // Dynamic Factor, since we have multiples day endValue and startValue can not be teh same
        var digits = (int) Math.Log10(endValue - startValue);
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

      // Use binary search, assuming values is sorted
      int startIndex = 0;
      Func<double, double, int> count = (start, end) =>
      {
        int endIndex = startIndex;
        while (endIndex < values.Count && values[endIndex] < end)
          endIndex++;
        int bucketCount = endIndex - startIndex;
        startIndex = endIndex; // advance for next bucket
        return bucketCount;
      };

      // Delegate to ProcessBuckets
      ProcessBuckets(valueClusterCollection,
          startValues,
          startValue => startValue + factor,
          (startValue, endValue) => factor<1 ? $"[{startValue:F1} to {endValue:F1})" : $"[{startValue:N0} to {endValue:N0})",
          (startValue, endValue) => $"({escapedName} >= {startValue} AND {escapedName} < {endValue})",
          count,
           combine ? Math.Max(5, values.Count * 3 / (max * 2)) : 1,
          maxSeconds- stopwatch.Elapsed.TotalSeconds,
          progress
      );
      return true;
    }


    /// <summary>
    /// Builds clusters from a list of long values.
    /// Uses dynamic or fixed factor depending on value range and max cluster size.
    /// </summary>
    public static bool BuildValueClustersLong(
     this ValueClusterCollection valueClusterCollection,
     List<long> values,
     string escapedName,
     int max,
     bool combine,
     double maxSeconds,
     IProgressWithCancellation progress)
    {
      ReportStart(progress);
      if (values.Count == 0) return false;
      // Get the distinct values and their counts
      var clusterOne = new HashSet<long>();
      var stopwatch = Stopwatch.StartNew();
      values.Sort();

      foreach (var value in values)
      {
        if (stopwatch.Elapsed.TotalSeconds > maxSeconds)
          throw new TimeoutException("Preparing overview took too long.");
        progress.CancellationToken.ThrowIfCancellationRequested();
        if (clusterOne.Count <= max)
          clusterOne.Add(value);
      }

      if (clusterOne.Count == 0)
        return false;

      // Use Integer filter
      long factor;
      List<long> startValues;
      if (clusterOne.Count < max)
      {
        startValues = clusterOne.OrderBy(x => x).ToList();
        factor = 1;
      }
      else
      {
        // Dynamic Factor
        var digits = (long) Math.Log10(values.Max() - values.Min());
        factor = Convert.ToInt64(Math.Pow(10, digits));
        var endValue = values.Last();
        var startValue = values.First();
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

      Func<long, long, string> formatSQL =
        factor == 1 ? (startValue, _) => $"({escapedName} = {startValue})"
            : (startValue, endValue) => $"({escapedName} >= {startValue} AND {escapedName} < {endValue})";
      Func<long, long, string> getDisplay =
       factor == 1 ? (startValue, _) => $"{startValue:N0}"
           : (startValue, endValue) => $"[{startValue:N0} to {endValue:N0})";
      Func<long, long, int> count =
          factor == 1 ? (startValue, _) => values.Count(s => s == startValue)
          : (startValue, endValue) => values.Count(s => s >= startValue && s < endValue);
      int startIndex = 0;
      Func<long, long, int> countOptimized = (start, end) =>
      {
        int endIndex = startIndex;

        // Advance endIndex until values[endIndex] >= end
        while (endIndex < values.Count && values[endIndex] < end)
          endIndex++;

        int bucketCount = endIndex - startIndex;
        startIndex = endIndex; // advance for next bucket
        return bucketCount;
      };

      // Delegate to ProcessBuckets
      ProcessBuckets(valueClusterCollection,
          startValues,
          startValue => startValue + factor,
          getDisplay,
          formatSQL,
          countOptimized,
          combine ? Math.Max(5, values.Count * 3 / (max * 2)) : 1,
          maxSeconds- stopwatch.Elapsed.TotalSeconds,
          progress
      );

      return true;
    }

    /// <summary>
    /// Builds clusters from a list of DateTime values.
    /// Clusters can be hourly, daily, monthly, yearly, or by decades depending on data.
    /// </summary>
    public static bool BuildValueClustersDate(
        this ValueClusterCollection valueClusterCollection,
        List<DateTime> values, string escapedName,
        int max, bool combine, double maxSeconds, IProgressWithCancellation progress)
    {
      ReportStart(progress);
      if (values.Count == 0) return false;
      var stopwatch = Stopwatch.StartNew();
      var clusterHour = new HashSet<DateTime>();
      var clusterDay = new HashSet<DateTime>();
      var clusterMonth = new HashSet<DateTime>();
      var clusterYear = new HashSet<DateTime>();
      var clusterDecade = new HashSet<DateTime>();
      values.Sort();
      foreach (var value in values)
      {
        if (stopwatch.Elapsed.TotalSeconds > maxSeconds)
          throw new TimeoutException("Preparing overview took too long.");
        progress.CancellationToken.ThrowIfCancellationRequested();
        if (clusterHour.Count<max)
          clusterHour.Add(new DateTime(value.Year, value.Month, value.Day, value.Hour, (value.Minute / 15) * 15, 0));

        if (clusterDay.Count<max)
          clusterDay.Add(value.Date);

        if (clusterMonth.Count<max)
          clusterMonth.Add(new DateTime(value.Year, value.Month, 1));

        if (clusterYear.Count<max)
          clusterYear.Add(new DateTime(value.Year, 1, 1));

        if (clusterDecade.Count<max)
          clusterDecade.Add(new DateTime((value.Year / 10) * 10, 1, 1));
        else
          throw new ArgumentOutOfRangeException(nameof(values));
      }

      var desiredSize = combine ? Math.Max(5, values.Count * 3 / (max * 2)) : 1;
      Func<DateTime, DateTime, string> formatSQL =
          (startValue, endValue) => $"({escapedName} >= #{startValue:MM/dd/yyyy HH:mm}# AND {escapedName} < #{endValue:MM/dd/yyyy HH:mm}#)";
      Func<DateTime, DateTime, int> count =
          (startValue, endValue) => values.Count(v => v.Date >= startValue && v.Date< endValue);

      int startIndex = 0;
      Func<DateTime, DateTime, int> countOptimized = (start, end) =>
      {
        int endIndex = startIndex;

        // Advance endIndex until values[endIndex] >= end
        while (endIndex < values.Count && values[endIndex] < end)
          endIndex++;

        int bucketCount = endIndex - startIndex;
        startIndex = endIndex; // advance for next bucket
        return bucketCount;
      };

      // Hourly
      if (clusterDay.Count == 1 && clusterHour.Count<max)
      {
        ProcessBuckets(valueClusterCollection,
            clusterHour.OrderBy(x => x).ToList(),
            startValue => startValue.AddMinutes(15),
            (startValue, endValue) => $"{startValue} to {endValue}",
            formatSQL,
            countOptimized,
            desiredSize,
            maxSeconds- stopwatch.Elapsed.TotalSeconds,
            progress
        );
      }
      // Daily
      else if (clusterDay.Count < max)
      {
        ProcessBuckets(valueClusterCollection,
            clusterDay.OrderBy(x => x).ToList(),
            startValue => startValue.AddDays(1),
            (startValue, endValue) => startValue.Date.AddDays(1) == endValue.Date ? $"Day {startValue:d}" : $"Days {startValue:d} to {endValue:d}",
            formatSQL,
            countOptimized,
            desiredSize,
            maxSeconds- stopwatch.Elapsed.TotalSeconds,
            progress);
      }
      // Monthly
      else if (clusterMonth.Count < max)
      {
        ProcessBuckets(valueClusterCollection,
            clusterMonth.OrderBy(x => x).ToList(),
            startValue => startValue.AddMonths(1),
            (startValue, endValue) => startValue.Year == endValue.Year && startValue.Month+1 == endValue.Month ? $"Month {startValue:MMM yyyy}" : $"Month {startValue:MMM yyyy} to {endValue:MMM yyyy}",
            formatSQL,
            countOptimized,
            desiredSize,
            maxSeconds- stopwatch.Elapsed.TotalSeconds,
            progress);
      }
      else if (clusterYear.Count < max)
      {
        // Yearly
        ProcessBuckets(valueClusterCollection,
          clusterYear.OrderBy(x => x).ToList(),
          startValue => startValue.AddYears(1),
          (startValue, endValue) => startValue.Year+1 == endValue.Year ? $"Year {startValue.Year}" : $"Years {startValue.Year} to {endValue.Year}",
          formatSQL,
          countOptimized,
          desiredSize,
          maxSeconds- stopwatch.Elapsed.TotalSeconds,
          progress);
      }
      else
      {
        // Decades
        ProcessBuckets(valueClusterCollection,
          clusterDecade.OrderBy(x => x).ToList(),
          startValue => startValue.AddYears(10),
          (startValue, endValue) => $"Years {startValue.Year} to {endValue.Year}",
          formatSQL,
          countOptimized,
          desiredSize,
          maxSeconds- stopwatch.Elapsed.TotalSeconds,
          progress);
      }
      return true;
    }
    /// <summary>
    /// Reports initial progress message when starting cluster creation.
    /// </summary>
    private static void ReportStart(IProgress<ProgressInfo> progress)
    => progress.Report(new ProgressInfo(
        "Combining values into groups with evenly spaced boundaries, resulting in groups that may contain different numbers of rows.",
        (long) (ValueClusterCollection.cPercentTyped * ValueClusterCollection.cProgressMax)));
    
    /// <summary>
    /// Processes sorted start values to build clusters, calling delegate functions for display, SQL formatting, and counting.
    /// </summary>
    private static void ProcessBuckets<T>(
        ValueClusterCollection valueClusterCollection,
        List<T> startValues,
        Func<T, T> nextEnd,
        Func<T, T, string> getDisplay,
        Func<T, T, string> getStatement,
        Func<T, T, int> getCount,
        int desiredSize, double maxRemainingSeconds,
        IProgressWithCancellation progress) where T : struct, IComparable<T>
    {
      var stopwatch = Stopwatch.StartNew();
      if (desiredSize<1)
        desiredSize=1;
      if (startValues.Count==0)
        throw new ArgumentOutOfRangeException("Need start values");
      var endValue = startValues[startValues.Count-1];
      var next = nextEnd(endValue);
      // Add start value entry in case teh very last value is not part of the list
      if (next.CompareTo(endValue) <= 0)
        throw new InvalidOperationException("nextEnd must produce strictly increasing values.");
      else
        startValues.Add(next);

      var percent = ValueClusterCollection.cPercentTyped*2;
      if (percent>1)
        throw new InvalidOperationException("Progress percent exceeded 100%.");
      var step = (1-percent) / startValues.Count;

      for (var i = 0; i<startValues.Count; i++)
      {
        var start = startValues[i];
        if (stopwatch.Elapsed.TotalSeconds > maxRemainingSeconds)
          throw new TimeoutException("Building groups took too long.");
        progress.CancellationToken.ThrowIfCancellationRequested();

        // Determine bucket end
        var end = nextEnd(start);
        RetryNewEnd:
        percent += step;
        var info = getDisplay(start, end);
        var progressNum = (long) Math.Round(percent * ValueClusterCollection.cProgressMax);
        // progress.Report(new ProgressInfo($"Checking group {info}", progressNum));

        // Skip if already clustered
        if (valueClusterCollection.HasEnclosingCluster(start, end))
          continue;

        // Count items in this bucket (time intensive)
        var count = getCount(start, end);
        if (count ==0)
          continue;

        // Increase bucket size if below desired size
        if (count < desiredSize && i + 1 < startValues.Count)
        {
          end = nextEnd(startValues[++i]);
          goto RetryNewEnd;
        }

        // Create and add cluster
        valueClusterCollection.LastCluster = new ValueCluster(
            info,
            getStatement(start, end),
            count,
            start,
            end);

        valueClusterCollection.Add(valueClusterCollection.LastCluster);

        // Progress reporting
        progress.Report(new ProgressInfo($"Adding group {info}", progressNum));
      }
    }
  }
}
