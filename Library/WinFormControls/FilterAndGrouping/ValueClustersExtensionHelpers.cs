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
using System.Threading;

namespace CsvTools
{
  /// <summary>
  /// Provides extension methods to build groups of numeric, long, date and string values
  /// </summary>
  public static partial class ValueClustersExtension
  {
    private const int cMaxProgress = 10000;
    private const double cTypedProgress = 0.25;

    public static T MaxValue<T>() where T : struct
    {
      return typeof(T) switch
      {
        Type t when t == typeof(long) => (T) (object) long.MaxValue,
        Type t when t == typeof(double) => (T) (object) double.MaxValue,
        Type t when t == typeof(DateTime) => (T) (object) DateTime.MaxValue,
        _ => throw new InvalidOperationException($"Cannot determine MaxValue for type {typeof(T)}")
      };
    }

    private interface IClusterResolutionStrategy<T> where T : struct, IComparable<T>
    {
      Func<object, T> Convert { get; }
      Func<T, T> Round { get; }

      string FormatDisplay(T start, T end);

      string FormatSql(string escapedName, T start, T end);

    }

    private sealed class ClusterResolutionStrategyLong : IClusterResolutionStrategy<long>
    {
      public Func<object, long> Convert => System.Convert.ToInt64;

      public Func<long, long> Round => number => number;

      public string FormatDisplay(long startValue, long endValue)
      {
        if (endValue<long.MaxValue)
          return endValue == startValue + 1 ? $"{startValue}" : $"[{startValue:N0} to {endValue:N0})";
        return $"[{startValue:N0} …)";
      }

      public string FormatSql(string escapedName, long startValue, long endValue)
      {
        if (endValue<long.MaxValue)
          return endValue == startValue + 1 ? $"({escapedName} = {startValue})" : $"({escapedName} >= {startValue} AND {escapedName} < {endValue})";
        return $"({escapedName} >= {startValue})";
      }
    }

    private sealed class ClusterResolutionStrategyDouble : IClusterResolutionStrategy<double>
    {
      public Func<object, double> Convert => obj => Math.Floor(System.Convert.ToDouble(obj, CultureInfo.CurrentCulture) * 100d) / 100d;
      public Func<double, double> Round => number => Math.Floor(number * 10d) / 10d;

      public string FormatDisplay(double startValue, double endValue) => endValue<double.MaxValue ? $"[{startValue:F1} to {endValue:F1})" : $"[{startValue:F1 …)}";

      public string FormatSql(string escapedName, double startValue, double endValue) => endValue<double.MaxValue ? $"({escapedName} >= {startValue} AND {escapedName} < {endValue})" : $"({escapedName} >= {startValue})";
    }

    private sealed class ClusterResolutionStrategyDateTime : IClusterResolutionStrategy<DateTime>
    {
      public Func<object, DateTime> Convert => System.Convert.ToDateTime;

      // 10:14 → 10:00; 10:32 → 10:30
      public Func<DateTime, DateTime> Round => (dt => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute / 15 * 15, 0, 0, DateTimeKind.Local));

      public string FormatDisplay(DateTime startValue, DateTime endValue)
      {
        if (endValue == DateTime.MaxValue)
          return $"after {startValue}";

        // Years: All values guaranteed to start in 1st Jan
        if (startValue.Day==1 && startValue.Month==1 &&  endValue.Year > startValue.Year)
        {
          if (endValue ==startValue.AddYears(1))
            return $"in {startValue.Year}";
          return $"{startValue.Year} to {endValue.Year}";
        }
        var months = (endValue.Year - startValue.Year) * 12 + (endValue.Month - startValue.Month);
        // Month: All values guaranteed to start in 1st of Month
        if (startValue.Day==1 && months > 1)
        {
          if (endValue ==startValue.AddMonths(1))
            return $"in {startValue:MMM yyyy}";
          return $"{startValue:MMM yyyy} to {endValue:MMM yyyy}";
        }

        // Days: All values guaranteed to not have a time
        if (startValue == startValue.Date)
        {
          if (startValue.AddDays(1) == endValue)
            return $"On {startValue:d}";
          return $"Days {startValue:d} to {endValue:d}";
        }

        // The remaining must be times, we have only ranges with no seconds
        return $"{startValue:t} to {endValue:t}";
      }

      public string FormatSql(string escapedName, DateTime startValue, DateTime endValue) =>
         (endValue<DateTime.MaxValue) ? $"({escapedName} >= #{startValue:MM/dd/yyyy HH:mm}# AND {escapedName} < #{endValue:MM/dd/yyyy HH:mm}#)" : $"({escapedName} >= #{startValue:MM/dd/yyyy HH:mm}#)";
    }

    private static void CheckTimeout(Stopwatch sw, double limit, CancellationToken token)
    {
      if (sw.Elapsed.TotalSeconds > limit)
        throw new TimeoutException("Preparing overview took too long.");
      token.ThrowIfCancellationRequested();
    }

    /// <summary>
    /// Converts items to typed values and counts how many could not be converted (NULL or invalid).
    /// </summary>
    /// <param name="values">The objects to convert.</param>
    /// <param name="convert">Conversion delegate.</param>
    /// <param name="progress">Progress/cancellation handler.</param>
    /// <returns>Number of NULL or unconvertible values.</returns>
    private static (int nullCount, List<T> unsortedList) MakeTypedValues<T>(object[] values, Func<object, T> convert, IProgressWithCancellation progress) where T : notnull
    {
      if (values is null) throw new ArgumentNullException(nameof(values));
      if (convert is null) throw new ArgumentNullException(nameof(convert));
      if (progress is null) throw new ArgumentNullException(nameof(progress));
      int total = values.Length;
      // Start with a high capacity to avoid frequent re-allocations
      var typedList = new List<T>((int) (total * 0.9));
      var nullCount = 0;

      progress.SetMaximum(cMaxProgress);
      progress.Report($"Collecting values from {total:N0} rows");

      // Track iterations to reduce cancellation check frequency
      int count = 0;
      foreach (var obj in values)
      {
        // Check cancellation every 128 rows (using bitwise & for performance)
        if ((count++ & 127) == 0)
        {
          progress.CancellationToken.ThrowIfCancellationRequested();
        }
        try
        {
          if (obj is null or DBNull)
          {
            nullCount++;
          }
          else
          {
            T value = convert(obj);
            if (value is null)
              nullCount++;
            else
              typedList.Add(value);
          }
        }
        catch (Exception ex) when (ex is FormatException or InvalidCastException)
        {
          nullCount++;
        }
      }
      return (nullCount, typedList);
    }
  }
}
