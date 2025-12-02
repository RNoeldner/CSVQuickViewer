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
  /// Provides extension methods to build groups of numeric, long, date and string values
  /// </summary>
  public static partial class ValueClustersExtension
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
    public static (int nullCount, List<ValueCluster> clusters) BuildValueClustersDateEven(this object[] values, string escapedName,
      int max, double maxSeconds, IProgressWithCancellation progress)
    => BuildValueClustersEven(values, new ClusterResolutionStrategyDateTime(), values.Length / max, escapedName, maxSeconds, progress);

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
    public static (int nullCount, List<ValueCluster> clusters) BuildValueClustersDoubleEven(this object[] values, string escapedName,
      int max, double maxSeconds, IProgressWithCancellation progress)
    => BuildValueClustersEven(values, new ClusterResolutionStrategyDouble(), values.Length / max, escapedName, maxSeconds, progress);

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
    public static (int nullCount, List<ValueCluster> clusters) BuildValueClustersLongEven(this object[] values, string escapedName,
      int max, double maxSeconds, IProgressWithCancellation progress)
    => BuildValueClustersEven(values, new ClusterResolutionStrategyLong(), values.Length / max, escapedName, maxSeconds, progress);

    private static (int nullCount, List<ValueCluster> clusters) BuildValueClustersEven<T>(
     object[] objects,
     IClusterResolutionStrategy<T> strategy,
     int bucketSize,
     string escapedName,
     double maxSeconds,
     IProgressWithCancellation progress)
     where T : struct, IComparable<T>
    {
      var stopwatch = Stopwatch.StartNew();
      (int nullCount, var values) = MakeTypedValues(objects, strategy.Convert, progress);
      if (values.Count == 0)
        return (nullCount, new List<ValueCluster>());
      values.Sort();


      progress.Report(
        new ProgressInfo(
          "Combining values into groups that contain approximately the same number of records, resulting in variable boundary widths.",
          (long) (cTypedProgress * cMaxProgress))
      );

      // Count occurrences per rounded value
      var counter = new Dictionary<T, int>();

      foreach (var v in values)
      {
        CheckTimeout(stopwatch, maxSeconds, progress.CancellationToken);
        var r = strategy.Round(v);

#if NETFRAMEWORK
        if (counter.ContainsKey(r))
          counter[r]++;
        else
          counter.Add(r, 1);
#else
        if (!counter.TryAdd(r, 1))
            counter[r]++;
#endif
      }

      var clusters = new List<ValueCluster>();
      if (counter.Count == 0)
        return (nullCount, clusters);

      // IMPORTANT: ordered iteration
      var orderedKeys = counter.Keys.OrderBy(k => k).ToList();

      var accumulated = 0;
      var percent = cTypedProgress * 2;
      var step = (1.0 - percent) / orderedKeys.Count;
      var start = orderedKeys[0];
      var bucketStart = orderedKeys[0];
      var end = orderedKeys[0];
      for (var i = 0; i < orderedKeys.Count; i++)
      {
        // Number of entries for the start key 
        var count = counter[orderedKeys[i]];
        if (accumulated + count < bucketSize)
        {
          accumulated += count;
          percent += step;
          continue;
        }
        CheckTimeout(stopwatch, maxSeconds, progress.CancellationToken);
        end = i+1<orderedKeys.Count ?
            orderedKeys[i+1] :
            MaxValue<T>();

        AddCluster(accumulated +  count);
        start = end;
      }

      // Final bucket (if anything remains)
      if (accumulated > 0)
        AddCluster(accumulated);

      return (nullCount, clusters);

      void AddCluster(int numRecs)
      {
        string display = strategy.FormatDisplay(start, end);
        clusters.Add(new ValueCluster(display, strategy.FormatSql(escapedName, start, end), numRecs, start, end));
        percent += step;
        progress.Report(new ProgressInfo($"Adding group {display} with {numRecs} entries", (long) Math.Round(percent * cMaxProgress)));
        accumulated =0;
      }
    }
  }
}
