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
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CsvTools;

/// <summary>
/// Represents a collection of <see cref="ValueCluster"/> objects, providing
/// functionality to build, manage, and retrieve clusters of values from
/// heterogeneous data types (strings, numbers, dates, GUIDs, booleans) with
/// optional even distribution, combining of small clusters, and progress
/// reporting with cancellation support.
/// </summary>
public sealed class ValueClusterCollection : List<ValueCluster>
{
  public const double cPercentTyped = .25;
  public const long cProgressMax = 10000;

  public ValueCluster LastCluster = new("Dummy", string.Empty, 0, null);

  /// <summary>
  ///   Gets the active keyValue cluster.
  /// </summary>
  /// <returns></returns>
  public IEnumerable<ValueCluster> GetActiveValueCluster() =>
    this.Where(value => !string.IsNullOrEmpty(value.Display) && value.Active);

  /// <summary>
  /// Rebuilds the value clusters for the specified data type.
  /// This operation clears any existing clusters and constructs
  /// a new cluster set based on the supplied values and clustering rules.
  /// </summary>
  /// <param name="type">The data type that determines how values are interpreted and clustered.</param>
  /// <param name="values">The collection of raw values from which clusters are derived.</param>
  /// <param name="escapedName">A name identifier used for logging, diagnostics, or UI output.</param>
  /// <param name="keepActive">
  /// If true, retains existing cluster activation states where possible; 
  /// otherwise, all clusters are rebuilt without preserving previous state.
  /// </param>
  /// <param name="maxNumber">The upper bound on the number of clusters to generate.</param>
  /// <param name="combine">
  /// Indicates whether small or low-density clusters should be merged to meet 
  /// the maximum cluster count or to improve distribution.
  /// </param>
  /// <param name="even">
  /// If true, attempts to distribute values evenly across clusters; 
  /// otherwise, natural groupings are preserved.
  /// </param>
  /// <param name="maxSeconds">
  /// The maximum allowed processing time. The method stops early if this limit is reached.
  /// </param>
  /// <param name="progress">
  /// Provides progress updates and allows cancellation during cluster rebuilding.
  /// </param>
  /// <returns>
  /// A <see cref="BuildValueClustersResult"/> containing the newly generated clusters 
  /// and associated metadata.
  /// </returns>
  public BuildValueClustersResult ReBuildValueClusters(DataTypeEnum type, object[] values,
    string escapedName, bool keepActive, int maxNumber, bool combine, bool even, double maxSeconds, IProgressWithCancellation progress)
  {
    if (values is null)
      throw new ArgumentNullException(nameof(values));

    if (maxNumber < 1 || maxNumber > 200)
      maxNumber = 200;

    if (keepActive)
    {
      var keep = GetActiveValueCluster().ToArray();
      Clear();
      foreach (var item in keep)
        Add(item);
    }
    else
      Clear();

    // For guid it does not make much sense to build clusters, any other type has a limit of 100k, It's just too slow otherwise
    if (values.Length > 50000 && type == DataTypeEnum.Guid)
      return BuildValueClustersResult.TooManyValues;

    try
    {
      progress.SetMaximum(cProgressMax);
      progress.Report($"Collecting values for {escapedName} from {values.Length:N0} rows");
      //----------------------------------------------------------------------
      // STRING / GUID / BOOLEAN
      //----------------------------------------------------------------------
      if (type == DataTypeEnum.String ||
          type == DataTypeEnum.Guid ||
          type == DataTypeEnum.Boolean)
      {
        (var countNull, var unsortedList)  = MakeTypedValues(values, Convert.ToString, progress.CancellationToken);
        AddValueClusterNull(escapedName, countNull);

        return this.BuildValueClustersString(unsortedList, escapedName, maxNumber, maxSeconds, progress) ? BuildValueClustersResult.ListFilled : BuildValueClustersResult.NoValues;
      }

      //----------------------------------------------------------------------
      // DATE
      //----------------------------------------------------------------------
      if (type == DataTypeEnum.DateTime)
      {
        (var countNull, var unsortedList)  =MakeTypedValues(values, Convert.ToDateTime, progress.CancellationToken);
        AddValueClusterNull(escapedName, countNull);
        return (even
          ? this.BuildValueClustersDateEven(unsortedList, escapedName, maxNumber, maxSeconds, progress)
          : this.BuildValueClustersDate(unsortedList, escapedName, maxNumber, combine, maxSeconds, progress))
          ? BuildValueClustersResult.ListFilled : BuildValueClustersResult.NoValues;
      }

      //----------------------------------------------------------------------
      // INTEGER
      //----------------------------------------------------------------------
      if (type == DataTypeEnum.Integer)
      {
        (var countNull, var unsortedList)  = MakeTypedValues(values, Convert.ToInt64, progress.CancellationToken);
        AddValueClusterNull(escapedName, countNull);
        return (even
          ? this.BuildValueClustersLongEven(unsortedList, escapedName, maxNumber, maxSeconds, progress)
          : this.BuildValueClustersLong(unsortedList, escapedName, maxNumber, combine, maxSeconds, progress))
          ? BuildValueClustersResult.ListFilled : BuildValueClustersResult.NoValues;
      }

      //----------------------------------------------------------------------
      // NUMERIC / DOUBLE Using 2 decimal places for grouping
      //----------------------------------------------------------------------
      if (type == DataTypeEnum.Numeric || type == DataTypeEnum.Double)
      {
        (var countNull, var unsortedList)  = MakeTypedValues(values, obj => Math.Floor(Convert.ToDouble(obj, CultureInfo.CurrentCulture) * 100d) / 100d, progress.CancellationToken);
        AddValueClusterNull(escapedName, countNull);
        return (even
          ? this.BuildValueClustersNumericEven(unsortedList, escapedName, maxNumber, maxSeconds, progress)
          : this.BuildValueClustersNumeric(unsortedList, escapedName, maxNumber, combine, maxSeconds, progress))
          ? BuildValueClustersResult.ListFilled : BuildValueClustersResult.NoValues;
      }

      return BuildValueClustersResult.WrongType;
    }
    // this makes it backward compatible
    catch (TimeoutException toe)
    {
      progress.Report(toe.Message);
      return BuildValueClustersResult.TooManyValues;
    }
    catch (OperationCanceledException tce)
    {
      progress.Report(tce.Message);
      return BuildValueClustersResult.TooManyValues;
    }
    catch (Exception ex)
    {
      progress.Report(ex.Message);
      return BuildValueClustersResult.Error;
    }
  }

  /// <summary>
  /// Determines whether any cluster in the collection fully encloses the specified range.
  /// </summary>
  /// <typeparam name="T">A value type that implements <see cref="IComparable{T}"/>.</typeparam>
  /// <param name="minValue">The start of the range to check.</param>
  /// <param name="maxValue">The end of the range to check.</param>
  /// 
  /// <returns>
  /// <c>true</c> if at least one cluster starts at or before <paramref name="minValue"/> 
  /// and ends at or after <paramref name="maxValue"/> (or is unbounded); otherwise, <c>false</c>.
  /// </returns>
  public bool HasEnclosingCluster<T>(T minValue, T maxValue) where T : struct, IComparable<T>
  {
    foreach (var cluster in this)
    {
      if (!(cluster.Start is T start))
        continue;

      if (start.CompareTo(minValue) > 0)
        continue; // cluster starts after our range

      // Null or unbounded cluster covers everything
      if (cluster.End is null || (cluster.End is T end && end.CompareTo(maxValue) >= 0))
        return true;
    }

    return false;
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
  private static (int nullCount, List<T> unsortedList) MakeTypedValues<T>(object[] values, Func<object, T> convert, CancellationToken cancellationToken)
  {
    var total = values.Length;
    var unsortedList = new List<T>(total);
    var nullCount = 0;

    int counter = 0;
    foreach (var obj in values)
    {
      cancellationToken.ThrowIfCancellationRequested();

      counter++;
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

    return (nullCount, unsortedList);
  }

  /// <summary>
  /// Adds a <see cref="ValueCluster"/> to the collection if it has a positive count
  /// and there is no existing cluster with the same display value (case-insensitive).
  /// Prevents duplicate clusters based on display text.
  /// </summary>
  /// <param name="item">The cluster to add.</param>
  /// <note>Not sure if we need this...</note>
  public new void Add(ValueCluster item)
  {
    if (item.Count <= 0) return;
    foreach (var existing in this)
      if (existing.Display.Equals(item.Display, StringComparison.OrdinalIgnoreCase))
        return;
    base.Add(item);
  }

  private void AddValueClusterNull(string escapedName, int count)
  {
    if (count <= 0 || this.Any(x => x.Start is null))
      return;
    Add(new ValueCluster(ColumnFilterLogic.OperatorIsNull, string.Format($"({escapedName} IS NULL)"), count, null));
  }
}