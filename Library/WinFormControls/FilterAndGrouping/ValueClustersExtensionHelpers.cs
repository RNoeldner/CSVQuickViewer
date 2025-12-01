using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

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
namespace CsvTools
{
  /// <summary>
  /// Provides extension methods to build groups of numeric, long, date and string values
  /// </summary>
  public static partial class ValueClustersExtension
  {
    private const int cMaxProgress = 10000;
    private const double cTypedProgress = 0.25;

    private static void CheckTimeout(Stopwatch sw, double limit, CancellationToken token)
    {
      if (sw.Elapsed.TotalSeconds > limit)
        throw new TimeoutException("Preparing overview took too long.");
      token.ThrowIfCancellationRequested();
    }

    /// <summary>
    /// Converts items to typed values and counts how many could not be converted (NULL or invalid).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">The objects to convert.</param>
    /// <param name="unsortedList">The list to receive the typed results.</param>
    /// <param name="convert">Conversion delegate.</param>
    /// <param name="progress">Progress/cancellation handler.</param>
    /// <returns>Number of NULL or unconvertible values.</returns>
    private static (int nullCount, List<T> unsortedList) MakeTypedValues<T>(object[] values, Func<object, T> convert, IProgressWithCancellation progress)
    {
      var total = values.Length;
      var unsortedList = new List<T>(total);
      var nullCount = 0;
      progress.SetMaximum(cMaxProgress);
      progress.Report($"Collecting values from {values.Length:N0} rows");
      foreach (var obj in values)
      {
        progress.CancellationToken.ThrowIfCancellationRequested();
        try
        {
          if (obj is null or DBNull)
          {
            nullCount++;
          }
          else
          {
            var value = convert(obj);
            if (value is null)
              nullCount++;
            else
              unsortedList.Add(value);
          }
        }
        catch (FormatException)
        {
          nullCount++;
        }
        catch (InvalidCastException)
        {
          nullCount++;
        }
      }
      return (nullCount, unsortedList);
    }
  }
}
