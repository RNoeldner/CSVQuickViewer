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
    {
      return BuildValueClustersEven(values, Convert.ToDateTime, values.Length / max,
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
    public static (int nullCount, List<ValueCluster> clusters) BuildValueClustersNumericEven(this object[] values, string escapedName,
      int max, double maxSeconds, IProgressWithCancellation progress)
    {
      return BuildValueClustersEven(values, obj => Math.Floor(Convert.ToDouble(obj, CultureInfo.CurrentCulture) * 100d) / 100d, values.Length / max, number => Math.Floor(number * 10d) / 10d,
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
    public static (int nullCount, List<ValueCluster> clusters) BuildValueClustersLongEven(this object[] values, string escapedName,
      int max, double maxSeconds, IProgressWithCancellation progress)
    {
      return BuildValueClustersEven(values, Convert.ToInt64, values.Length / max, number => number,
        (minValue, maxValue) => $"{minValue:F0} to {maxValue:F0}",
        (minValue, maxValue) => string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", escapedName,
          minValue, maxValue),
        minValue => $">= {minValue:F0}",
        minValue => string.Format(CultureInfo.InvariantCulture, "({0} >= {1})", escapedName, minValue), maxSeconds, progress);
    }


    private static (int nullCount, List<ValueCluster> clusters) BuildValueClustersEven<T>(
     object[] objects,
     Func<object, T> convert,
     int bucketSize,
     Func<T, T> round,
     Func<T, T, string> getDisplay,
     Func<T, T, string> getStatement,
     Func<T, string> getDisplayLast,
     Func<T, string> getStatementLast,
     double maxSeconds,
     IProgressWithCancellation progress)
     where T : struct, IComparable<T>
    {
      var stopwatch = Stopwatch.StartNew();
      (int nullCount, var values) = MakeTypedValues(objects, convert, progress);

      var clusters = new List<ValueCluster>();
      if (values.Count == 0)
        return (nullCount, clusters);

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

        var r = round(v);

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

      if (counter.Count == 0)
        return (nullCount, clusters);

      // IMPORTANT: ordered iteration
      var orderedKeys = counter.Keys.OrderBy(k => k).ToList();

      int accumulated = 0;
      T start = orderedKeys[0];

      double percent = cTypedProgress * 2;
      double step = (1.0 - percent) / orderedKeys.Count;

      for (int i = 0; i < orderedKeys.Count; i++)
      {
        var key = orderedKeys[i];
        CheckTimeout(stopwatch, maxSeconds, progress.CancellationToken);

        int count = counter[key];
        if (accumulated + count < bucketSize)
        {
          accumulated += count;
          percent += step;
          continue;
        }

        // Close bucket
        clusters.Add(
            new ValueCluster(
                getDisplay(start, key),
                getStatement(start, key),
                accumulated + count,
                start,
                key
        ));

        percent += step;
        progress.Report(
          new ProgressInfo($"Adding group ending at {key}", (long) Math.Round(percent * cMaxProgress)));

        // Start new bucket AFTER this key
        start = key;
        accumulated = 0;
      }

      // Final bucket (if anything remains)
      if (accumulated > 0)
      {
        clusters.Add(
            new ValueCluster(
                getDisplayLast(start),
                getStatementLast(start),
                accumulated,
                start));
      }

      return (nullCount, clusters);
    }
  }
}
