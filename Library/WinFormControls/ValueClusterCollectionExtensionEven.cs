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
using System.Globalization;
using System.Linq;

namespace CsvTools
{
  public static class ValueClusterCollectionExtensionEven
  {
    /// <summary>
    /// Builds evenly distributed clusters of <see cref="DateTime"/> values from the provided list. 
    /// The clusters are created so that each cluster covers a roughly equal range of time, 
    /// constrained by the <paramref name="maxSeconds"/> duration and the maximum number of clusters.
    /// </summary>
    /// <param name="values">The list of <see cref="DateTime"/> values to cluster.</param>
    /// <param name="escapedName">The column or field name used for generating filter expressions.</param>
    /// <param name="max">Maximum number of clusters to create.</param>
    /// <param name="maxSeconds">Maximum duration in seconds for a single cluster before splitting.</param>
    /// <param name="progress">Progress tracker with cancellation support.</param>
    /// <returns>A <see cref="BuildValueClustersResult"/> containing the generated clusters and related metadata.</returns>
    public static bool BuildValueClustersDateEven(this ValueClusterCollection valueClusterCollection, List<DateTime> values, string escapedName,
      int max, double maxSeconds, IProgressWithCancellation progress)
    {
      return BuildValueClustersEven(valueClusterCollection, values, values.Count / max,
        number => new DateTime(number.Year, number.Month, number.Day, number.Hour, number.Minute, 0),
        (minValue, maxValue) => $"{minValue:d} to {maxValue:d}",
        (minValue, maxValue) => string.Format(CultureInfo.InvariantCulture,
          "({0} >= #{1:MM/dd/yyyy HH:mm}# AND {0} < #{2:MM/dd/yyyy HH:mm}#)", escapedName, minValue, maxValue),
        minValue => $"after {minValue:d}",
        minValue => string.Format(CultureInfo.InvariantCulture, "({0} >= #{1:MM/dd/yyyy HH:mm}#)", escapedName,
          minValue), maxSeconds, progress);
    }

    /// <summary>
    /// Builds evenly distributed clusters from a list of <see cref="double"/> values.
    /// Each cluster will contain approximately equal ranges, without combining smaller clusters.
    /// </summary>
    /// <param name="values">The list of double values to cluster.</param>
    /// <param name="escapedName">The name of the field used in generated statements, already escaped for safe usage.</param>
    /// <param name="max">Maximum number of items allowed per cluster.</param>
    /// <param name="maxSeconds">Maximum time span in seconds for each cluster.</param>
    /// <param name="progress">Progress tracker that also supports cancellation.</param>
    /// <returns>A <see cref="BuildValueClustersResult"/> containing the generated clusters.</returns>
    public static bool BuildValueClustersNumericEven(this ValueClusterCollection valueClusterCollection, List<double> values, string escapedName,
      int max, double maxSeconds, IProgressWithCancellation progress)
    {
      return BuildValueClustersEven(valueClusterCollection, values, values.Count / max, number => Math.Floor(number * 10d) / 10d,
        (minValue, maxValue) => $"{minValue:F1} to {maxValue:F1}",
        (minValue, maxValue) => string.Format(CultureInfo.InvariantCulture, "({0} >= {1:F1} AND {0} < {2:F1})",
          escapedName, minValue, maxValue),
        minValue => $">= {minValue:F1}",
        minValue => string.Format(CultureInfo.InvariantCulture, "({0} >= {1:F1})", escapedName, minValue), maxSeconds, progress);
    }


    /// <summary>
    /// Builds evenly distributed clusters from a list of <see cref="long"/> values. 
    /// Each cluster aims to contain a roughly equal number of items while respecting 
    /// the specified maximum size and time span constraints.
    /// </summary>
    /// <param name="values">The list of long values to cluster.</param>
    /// <param name="escapedName">The name of the field used in generated statements, already escaped for safe usage.</param>
    /// <param name="max">Maximum number of items allowed per cluster.</param>
    /// <param name="maxSeconds">Maximum time span in seconds for each cluster.</param>
    /// <param name="progress">Progress tracker that also supports cancellation.</param>
    /// <returns>A <see cref="BuildValueClustersResult"/> containing the generated evenly distributed clusters.</returns>
    public static bool BuildValueClustersLongEven(this ValueClusterCollection valueClusterCollection, List<long> values, string escapedName,
      int max, double maxSeconds, IProgressWithCancellation progress)
    {
      return BuildValueClustersEven(valueClusterCollection, values, values.Count / max, number => number,
        (minValue, maxValue) => $"{minValue:F0} to {maxValue:F0}",
        (minValue, maxValue) => string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", escapedName,
          minValue, maxValue),
        minValue => $">= {minValue:F0}",
        minValue => string.Format(CultureInfo.InvariantCulture, "({0} >= {1})", escapedName, minValue), maxSeconds, progress);
    }


    private static bool BuildValueClustersEven<T>(ValueClusterCollection valueClusterCollection, List<T> values, int bucketSize,
      Func<T, T> round, Func<T, T, string> getDisplay, Func<T, T, string> getStatement, Func<T, string> getDisplayLast,
      Func<T, string> getStatementLast, double maxSeconds, IProgressWithCancellation progress)
      where T : struct, IComparable<T>
    {
      if (values.Count == 0)
        return false;

      var counter = new SortedDictionary<T, int>();
      var stopwatch = Stopwatch.StartNew();
      foreach (var number in values)
      {
        if (stopwatch.Elapsed.TotalSeconds > maxSeconds)
          throw new TimeoutException("Preparing overview took too long.");
        progress.CancellationToken.ThrowIfCancellationRequested();

        var rounded = round(number);
#if NETFRAMEWORK
        if (counter.ContainsKey(rounded))
          counter[rounded]++;
        else
          counter.Add(rounded, 1);
#else
        if (!counter.TryAdd(rounded, 1))
          counter[rounded]++;
#endif
      }

      if (counter.Count == 0)
        return false;
      var minValue = counter.First().Key;
      var bucketCount = 0;
      var hasPrevious = valueClusterCollection.Any(x => x.Start is T);
      var percent = ValueClusterCollection.cPercentTyped *2;
      var step = (1.0 - percent) / counter.Count;
      foreach (var keyValue in counter)
      {
        if (stopwatch.Elapsed.TotalSeconds > maxSeconds)
          throw new TimeoutException("Building groups took too long.");
        progress.CancellationToken.ThrowIfCancellationRequested();

        bucketCount += keyValue.Value;
        // progress keyValue.Key next bucket
        if (bucketCount <= bucketSize)
          continue;
        // excluding the last value
        bucketCount -= keyValue.Value;
        // In case there is a cluster that is overlapping do not add a cluster
        if (!hasPrevious || !valueClusterCollection.HasEnclosingCluster(minValue, keyValue.Key))
          valueClusterCollection.Add(new ValueCluster(getDisplay(minValue, keyValue.Key), getStatement(minValue, keyValue.Key),
            bucketCount, minValue, keyValue.Key));

        minValue = keyValue.Key;
        // start with last value bucket size
        bucketCount = keyValue.Value;
        percent += step;
        progress.Report(new ProgressInfo($"Adding ordered {counter.Count:N0} values", (long) Math.Round(percent * ValueClusterCollection.cProgressMax)));
      }

      // Make one last bucket for the rest
      if (!hasPrevious || !valueClusterCollection.Any(x => x.End == null || x.End is T se && se.CompareTo(minValue) >= 0))
        valueClusterCollection.Add(new ValueCluster(getDisplayLast(minValue), getStatementLast(minValue), bucketCount, minValue));

      return true;
    }
  }
}
