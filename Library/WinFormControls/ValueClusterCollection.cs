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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CsvTools
{
  /// <summary>
  /// Represents a collection of <see cref="ValueCluster"/> objects, providing
  /// functionality to build, manage, and retrieve clusters of values from
  /// heterogeneous data types (strings, numbers, dates, GUIDs, booleans) with
  /// optional even distribution, combining of small clusters, and progress
  /// reporting with cancellation support.
  /// </summary>
  public sealed class ValueClusterCollection : ICollection<ValueCluster>
  {
    private const double cPercentBuild = .5;

    private const double cPercentBuildEven = .6;

    private const double cPercentTyped = .5;

    private const long cProgressMax = 10000;

    private const long cTicksPerGroup = TimeSpan.TicksPerMinute * 30;

    private readonly IList<ValueCluster> m_ValueClusters = new List<ValueCluster>();

    private ValueCluster m_Last = new ValueCluster("Dummy", string.Empty, int.MaxValue, null);

    delegate bool SpanPredicate(ReadOnlySpan<char> span);

    private enum DateTimeRange { Hours, Days, Month, Years, }

    public int Count => m_ValueClusters.Count;

    public bool IsReadOnly => m_ValueClusters.IsReadOnly;

    public void Add(ValueCluster item) => m_ValueClusters.Add(item);

    public void Clear() => m_ValueClusters.Clear();

    public bool Contains(ValueCluster item) => m_ValueClusters.Contains(item);

    public void CopyTo(ValueCluster[] array, int arrayIndex) => m_ValueClusters.CopyTo(array, arrayIndex);

    /// <summary>
    ///   Gets the active keyValue cluster.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ValueCluster> GetActiveValueCluster() =>
      m_ValueClusters.Where(value => !string.IsNullOrEmpty(value.Display) && value.Active);

    public IEnumerator<ValueCluster> GetEnumerator() => m_ValueClusters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => m_ValueClusters.GetEnumerator();

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
        ClearNotActive();
      else
        Clear();

      // For guid it does not make much sense to build clusters, any other type has a limit of 100k, It's just too slow otherwise
      if (values.Length > 50000 && type == DataTypeEnum.Guid)
        return BuildValueClustersResult.TooManyValues;

      try
      {
        progress.SetMaximum(cProgressMax);
        progress.Report($"Collecting typed values for {escapedName} from {values.Length:N0} rows");
        //----------------------------------------------------------------------
        // STRING / GUID / BOOLEAN
        //----------------------------------------------------------------------
        if (type == DataTypeEnum.String ||
            type == DataTypeEnum.Guid ||
            type == DataTypeEnum.Boolean)
        {
          (var countNull, var typedValues)  = MakeTypedValues(values, Convert.ToString, progress.CancellationToken);
          AddValueClusterNull(escapedName, countNull);
          progress.Report("Combining values to clusters");
          return BuildValueClustersString(typedValues.ToArray(), escapedName, maxNumber, maxSeconds, progress);
        }

        //----------------------------------------------------------------------
        // DATE
        //----------------------------------------------------------------------
        if (type == DataTypeEnum.DateTime)
        {
          (var countNull, var typedValues)  =MakeTypedValues(values, Convert.ToDateTime, progress.CancellationToken);
          AddValueClusterNull(escapedName, countNull);
          progress.Report(even ? "Combining dates to clusters of even size" : "Combining dates to clusters");

          return even
            ? BuildValueClustersDateEven(typedValues, escapedName, maxNumber, maxSeconds, progress)
            : BuildValueClustersDate(typedValues, escapedName, maxNumber, combine, maxSeconds, progress);
        }

        //----------------------------------------------------------------------
        // INTEGER
        //----------------------------------------------------------------------
        if (type == DataTypeEnum.Integer)
        {
          (var countNull, var typedValues)  = MakeTypedValues(values, Convert.ToInt64, progress.CancellationToken);
          AddValueClusterNull(escapedName, countNull);
          progress.Report(even ? "Combining integer to clusters of even size" : "Combining integer to clusters");
          return even
            ? BuildValueClustersLongEven(typedValues, escapedName, maxNumber, maxSeconds, progress)
            : BuildValueClustersLong(typedValues, escapedName, maxNumber, combine, maxSeconds, progress);
        }

        //----------------------------------------------------------------------
        // NUMERIC / DOUBLE
        //----------------------------------------------------------------------
        if (type == DataTypeEnum.Numeric || type == DataTypeEnum.Double)
        {
          (var countNull, var typedValues)  = MakeTypedValues(values, obj => Math.Floor(Convert.ToDouble(obj, CultureInfo.CurrentCulture) * 1000d) / 1000d, progress.CancellationToken);
          AddValueClusterNull(escapedName, countNull);
          progress.Report(even ? "Combining numbers to clusters of even size" : "Combining numbers to clusters");
          return even
            ? BuildValueClustersNumericEven(typedValues, escapedName, maxNumber, maxSeconds, progress)
            : BuildValueClustersNumeric(typedValues, escapedName, maxNumber, combine, maxSeconds, progress);
        }

        return BuildValueClustersResult.WrongType;
      }
      catch (Exception ex)
      {
        progress.Report(ex.Message);
        return BuildValueClustersResult.Error;
      }
    }

    /// <summary>
    /// Removes the specified <see cref="ValueCluster"/> from the collection, if it exists.
    /// </summary>
    /// <param name="item">The <see cref="ValueCluster"/> to remove.</param>
    /// <returns><c>true</c> if the item was found and removed; otherwise, <c>false</c>.</returns>
    public bool Remove(ValueCluster item) => m_ValueClusters.Remove(item);

    /// <summary>
    /// Determines whether any cluster in the collection fully encloses the specified range.
    /// </summary>
    /// <typeparam name="T">A value type that implements <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="valueClusters">The collection of <see cref="ValueCluster"/> instances to check.</param>
    /// <param name="minValue">The start of the range to check.</param>
    /// <param name="maxValue">The end of the range to check.</param>
    /// <returns>
    /// <c>true</c> if at least one cluster starts at or before <paramref name="minValue"/> 
    /// and ends at or after <paramref name="maxValue"/> (or is unbounded); otherwise, <c>false</c>.
    /// </returns>
    private static bool HasEnclosingCluster<T>(IEnumerable<ValueCluster> valueClusters, T minValue, T maxValue) where T : struct, IComparable<T>
    {
      foreach (var cluster in valueClusters)
      {
        if (!(cluster.Start is T start))
          continue;

        if (start.CompareTo(minValue) > 0)
          continue; // cluster starts after our range

        // Null or unbounded cluster covers everything
        if (cluster.End is null)
          return true;

        if (cluster.End is T end && end.CompareTo(maxValue) >= 0)
          return true;
      }

      return false;
    }

    /// <summary>
    /// Converts items to typed values and counts how many could not be converted (NULL or invalid).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">The objects to convert.</param>
    /// <param name="typedValues">The list to receive the typed results.</param>
    /// <param name="convert">Conversion delegate.</param>
    /// <param name="progress">Progress/cancellation handler.</param>
    /// <returns>Number of NULL or unconvertible values.</returns>
    private static (int nullCount, List<T> typedValues) MakeTypedValues<T>(object[] values, Func<object, T> convert, CancellationToken cancellationToken)
    {
      var total = values.Length;
      var typedValues = new List<T>(total);
      var nullCount = 0;


      // Pre-calculate scale factor for progress, avoids repeated multiplication
      var scale = cPercentTyped * cProgressMax / (double) total;

      // Assume process is 5% of overall process
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
            typedValues.Add(value);
        }
      }

      return (nullCount, typedValues);
    }

    /// <summary>
    /// Adds a <see cref="ValueCluster"/> to the collection if it has a positive count
    /// and there is no existing cluster with the same display value (case-insensitive).
    /// Prevents duplicate clusters based on display text.
    /// </summary>
    /// <param name="item">The cluster to add.</param>
    private void AddUnique(in ValueCluster item)
    {
      if (item.Count <= 0) return;
      foreach (var existing in m_ValueClusters)
        if (existing.Display.Equals(item.Display, StringComparison.OrdinalIgnoreCase))
          return;
      m_ValueClusters.Add(item);
    }

    /// <summary>
    /// Creates and adds a <see cref="ValueCluster"/> for a range of <see cref="DateTime"/> values.
    /// If a previous cluster has too few values, it merges it with the current range to meet the desired size.
    /// The display format is adjusted based on the <see cref="DateTimeRange"/> type (Hours, Days, Month, Years).
    /// </summary>
    /// <param name="escapedName">The column or field name used in SQL-style statements.</param>
    /// <param name="from">Start of the date/time range (inclusive).</param>
    /// <param name="to">End of the date/time range (exclusive).</param>
    /// <param name="values">Collection of <see cref="DateTime"/> values to count for the cluster.</param>
    /// <param name="displayType">Specifies how the range should be displayed (Hours, Days, Month, Years).</param>
    /// <param name="desiredSize">Minimum desired count of values in a cluster; smaller clusters may be merged.</param>
    private void AddValueClusterDateTime(string escapedName, DateTime from, DateTime to,
      IEnumerable<DateTime> values, DateTimeRange displayType, int desiredSize = int.MaxValue)
    {
      if (HasEnclosingCluster(m_ValueClusters, from, to))
        return;
      var count = values.Count(x => x >= from && x < to);
      if (count <= 0)
        return;
      // Combine buckets if the last and the current do not have many values
      if (m_Last.Count + count < desiredSize && m_Last.Start is DateTime lastFrom)
      {
        from = lastFrom;
        count += m_Last.Count;

        // remove the last cluster it will be included with this new one
        m_ValueClusters.Remove(m_Last);
      }

      m_Last = new ValueCluster(displayType switch
      {
        DateTimeRange.Hours => $"{from:t} to {to:t}",
        //  "on dd/MM/yyyy" for a single day, or "dd/MM/yyyy to dd/MM/yyyy" for multiple days
        DateTimeRange.Days => to == from.AddDays(1) ? $"on {from:d}" : $"{from:d} to {to:d}",
        // "in MMM yyyy" for a single month, or "MMM yyyy to MMM yyyy" for multiple months
        DateTimeRange.Month => to == from.AddMonths(1) ? $"in {from:Y}" : $"{from:Y} to {to:Y}",
        // "in yyyy" for a single year, or "yyyy to yyyy" for multiple years
        DateTimeRange.Years => to == from.AddYears(1) ? $"in {from:yyyy}" : $"{from:yyyy} to {to:yyyy}",
        _ => string.Empty
      }, string.Format(CultureInfo.InvariantCulture,
        "({0} >= #{1:MM/dd/yyyy HH:mm}# AND {0} < #{2:MM/dd/yyyy HH:mm}#)", escapedName, from, to), count, from, to);
      AddUnique(m_Last);
    }

    private void AddValueClusterNull(string escapedName, int count)
    {
      if (count <= 0 || m_ValueClusters.Any(x => x.Start is null))
        return;
      AddUnique(new ValueCluster(ColumnFilterLogic.OperatorIsNull, string.Format($"({escapedName} IS NULL)"), count,
        null));
    }

    /// <summary>
    /// Builds clusters of <see cref="DateTime"/> values based on the specified parameters.
    /// Each cluster groups values into ranges and optionally combines small clusters to respect the maximum size.
    /// </summary>
    /// <param name="values">The list of <see cref="DateTime"/> values to cluster.</param>
    /// <param name="escapedName">The column or field name used for generating expressions (e.g., SQL-like).</param>
    /// <param name="max">The maximum number of clusters allowed.</param>
    /// <param name="combine">If true, clusters with few values may be merged with adjacent clusters.</param>
    /// <param name="maxSeconds">Maximum duration in seconds for a single cluster (used to split large ranges).</param>
    /// <param name="progress">Progress tracker that supports cancellation during clustering.</param>
    /// <returns>A <see cref="BuildValueClustersResult"/> containing the resulting clusters and metadata.</returns>
    private BuildValueClustersResult BuildValueClustersDate(List<DateTime> values, string escapedName,
      int max, bool combine, double maxSeconds, IProgressWithCancellation progress)
    {
      // Get the distinct values and their counts
      var clusterYear = new HashSet<int>();
      var clusterMonth = new HashSet<DateTime>();
      var clusterDay = new HashSet<DateTime>();
      var clusterHour = new HashSet<long>();
      var stopwatch = new Stopwatch();

      var progressAction = IntervalAction.ForProgress(progress);
      var msg = $"Preparing {values.Count:N0} values";
      double percent;
      int counter = 0;

      foreach (var value in values)
      {
        if (stopwatch.Elapsed.TotalSeconds > maxSeconds || progress.CancellationToken.IsCancellationRequested)
          return BuildValueClustersResult.TooManyValues;

        if (clusterHour.Count <= max)
          clusterHour.Add(value.TimeOfDay.Ticks / cTicksPerGroup);
        if (clusterDay.Count <= max)
          clusterDay.Add(value.Date);
        if (clusterMonth.Count <= max)
          clusterMonth.Add(new DateTime(value.Year, value.Month, 1));
        clusterYear.Add(value.Year);

        // if we have more than the maximum entries stop, no keyValue filter will be used
        if (clusterYear.Count > max)
          return BuildValueClustersResult.TooManyValues;

        counter++;
        percent = counter / (double) values.Count * cPercentBuildEven + cPercentTyped;
        progressAction?.Invoke(progress!, msg, (long) Math.Round(percent * cProgressMax));
      }

      if (clusterYear.Count == 0)
        return BuildValueClustersResult.NoValues;

      var desiredSize = 1;
      if (combine)
      {
        desiredSize = values.Count * 3 / (max * 2);
        if (desiredSize < 5)
          desiredSize = 5;
      }

      msg = "Adding cluster";
      percent = cPercentBuildEven;
      if (clusterDay.Count == 1)
      {
        clusterHour.Add(clusterHour.Min() - 1);
        clusterHour.Add(clusterHour.Max() + 1);
        var step = (1.0 - cPercentBuildEven) / clusterHour.Count;
        foreach (var dic in clusterHour.OrderBy(x => x))
        {
          if (stopwatch.Elapsed.TotalSeconds > maxSeconds || progress.CancellationToken.IsCancellationRequested)
            return BuildValueClustersResult.TooManyValues;

          AddValueClusterDateTime(escapedName, (dic * cTicksPerGroup).GetTimeFromTicks(),
            ((dic + 1) * cTicksPerGroup).GetTimeFromTicks(), values, DateTimeRange.Hours, desiredSize);
          percent += step;
          progressAction?.Invoke(progress!, msg, (long) Math.Round(percent * cProgressMax));
        }
      }
      else if (clusterDay.Count < max)
      {
        clusterDay.Add(clusterDay.Min().AddDays(-1));
        clusterDay.Add(clusterDay.Max().AddDays(+1));
        var step = (1.0 - cPercentBuildEven) / clusterDay.Count;
        foreach (var dateTime in clusterDay.OrderBy(x => x))
        {
          if (stopwatch.Elapsed.TotalSeconds > maxSeconds || progress.CancellationToken.IsCancellationRequested)
            return BuildValueClustersResult.TooManyValues;

          AddValueClusterDateTime(escapedName, dateTime, dateTime.AddDays(1), values, DateTimeRange.Days, desiredSize);
          percent += step;
          progressAction?.Invoke(progress!, msg, (long) Math.Round(percent * cProgressMax));
        }
      }
      else if (clusterMonth.Count < max)
      {
        clusterMonth.Add(clusterDay.Min().AddMonths(-1));
        clusterMonth.Add(clusterDay.Max().AddMonths(+1));
        var step = (1.0 - cPercentBuildEven) / clusterMonth.Count;
        foreach (var dateTime in clusterMonth.OrderBy(x => x))
        {
          if (stopwatch.Elapsed.TotalSeconds > maxSeconds || progress.CancellationToken.IsCancellationRequested)
            return BuildValueClustersResult.TooManyValues;

          AddValueClusterDateTime(escapedName, dateTime, dateTime.AddMonths(1), values, DateTimeRange.Month,
            desiredSize);
          percent += step;
          progressAction?.Invoke(progress!, msg, (long) Math.Round(percent * cProgressMax));
        }
      }
      else
      {
        clusterYear.Add(clusterYear.Max() + 1);
        var step = (1.0 - cPercentBuildEven) / clusterYear.Count;
        foreach (var year in clusterYear.OrderBy(x => x))
        {
          if (stopwatch.Elapsed.TotalSeconds > maxSeconds || progress.CancellationToken.IsCancellationRequested)
            return BuildValueClustersResult.TooManyValues;

          AddValueClusterDateTime(escapedName, new DateTime(year, 1, 1, 0, 0, 0, 0, DateTimeKind.Local),
            new DateTime(year + 1, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), values, DateTimeRange.Years, desiredSize);
          percent += step;
          progressAction?.Invoke(progress!, msg, (long) Math.Round(percent * cProgressMax));
        }
      }

      return BuildValueClustersResult.ListFilled;
    }

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
    private BuildValueClustersResult BuildValueClustersDateEven(List<DateTime> values, string escapedName,
      int max, double maxSeconds, IProgressWithCancellation progress)
    {
      return BuildValueClustersEven(values, values.Count / max,
        number => new DateTime(number.Year, number.Month, number.Day, number.Hour, number.Minute, 0),
        (minValue, maxValue) => $"{minValue:d} to {maxValue:d}",
        (minValue, maxValue) => string.Format(CultureInfo.InvariantCulture,
          "({0} >= #{1:MM/dd/yyyy HH:mm}# AND {0} < #{2:MM/dd/yyyy HH:mm}#)", escapedName, minValue, maxValue),
        minValue => $"after {minValue:d}",
        minValue => string.Format(CultureInfo.InvariantCulture, "({0} >= #{1:MM/dd/yyyy HH:mm}#)", escapedName,
          minValue), maxSeconds, progress);
    }

    private BuildValueClustersResult BuildValueClustersEven<T>(List<T> values, int bucketSize,
          Func<T, T> round, Func<T, T, string> getDisplay, Func<T, T, string> getStatement, Func<T, string> getDisplayLast,
          Func<T, string> getStatementLast, double maxSeconds, IProgressWithCancellation progress)
          where T : struct, IComparable<T>
    {
      var counter = new SortedDictionary<T, int>();
      var progressAction = IntervalAction.ForProgress(progress);
      var stopwatch = Stopwatch.StartNew();

      var msg = $"Preparing {values.Count:N0} values";
      double percent;
      int counterValues = 0;
      foreach (var number in values)
      {
        if (stopwatch.Elapsed.TotalSeconds > maxSeconds || progress.CancellationToken.IsCancellationRequested)
          return BuildValueClustersResult.TooManyValues;

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
        counterValues++;
        percent = counterValues / (double) values.Count * cPercentBuildEven + cPercentTyped;
        progressAction!.Invoke(progress, msg, (long) Math.Round(percent * cProgressMax));
      }

      if (counter.Count == 0)
        return BuildValueClustersResult.NoValues;
      var minValue = counter.First().Key;
      var bucketCount = 0;
      var hasPrevious = m_ValueClusters.Any(x => x.Start is T);
      percent = cPercentBuildEven;
      var step = (1.0 - cPercentBuildEven) / counter.Count;
      msg = $"Adding ordered {counter.Count:N0} values";
      foreach (var keyValue in counter)
      {
        if (stopwatch.Elapsed.TotalSeconds > maxSeconds || progress.CancellationToken.IsCancellationRequested)
          return BuildValueClustersResult.TooManyValues;

        bucketCount += keyValue.Value;
        // progress keyValue.Key next bucket
        if (bucketCount <= bucketSize)
          continue;
        // excluding the last value
        bucketCount -= keyValue.Value;
        // In case there is a cluster that is overlapping do not add a cluster
        if (!hasPrevious || !HasEnclosingCluster(m_ValueClusters, minValue, keyValue.Key))
          AddUnique(new ValueCluster(getDisplay(minValue, keyValue.Key), getStatement(minValue, keyValue.Key),
            bucketCount, minValue, keyValue.Key));

        minValue = keyValue.Key;
        // start with last value bucket size
        bucketCount = keyValue.Value;
        percent += step;
        progressAction!.Invoke(progress, msg, (long) Math.Round(percent * cProgressMax));
      }

      // Make one last bucket for the rest
      if (!hasPrevious || !m_ValueClusters.Any(x => x.End == null || x.End is T se && se.CompareTo(minValue) >= 0))
        AddUnique(new ValueCluster(getDisplayLast(minValue), getStatementLast(minValue), bucketCount, minValue));

      return BuildValueClustersResult.ListFilled;
    }

    /// <summary>
    /// Builds evenly sized clusters (buckets) for a generic list of comparable values. 
    /// Clusters are created according to the specified <paramref name="bucketSize"/> and may be adjusted 
    /// using the provided rounding and display/statement functions. Supports progress reporting and cancellation.
    /// </summary>
    /// <typeparam name="T">A value type that implements <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="values">The list of values to cluster.</param>
    /// <param name="bucketSize">The desired number of items per cluster.</param>
    /// <param name="round">A function to adjust values to cluster boundaries.</param>
    /// <param name="getDisplay">Function to generate the display string for a cluster given start and end values.</param>
    /// <param name="getStatement">Function to generate a statement or filter string for a cluster given start and end values.</param>
    /// <param name="getDisplayLast">Function to generate the display string for the last cluster if it contains fewer than <paramref name="bucketSize"/> items.</param>
    /// <param name="getStatementLast">Function to generate the statement for the last cluster if it contains fewer than <paramref name="bucketSize"/> items.</param>
    /// <param name="maxSeconds">Maximum time span in seconds for a cluster before splitting (applicable for date/time types).</param>
    /// <param name="progress">Progress tracker with cancellation support.</param>
    /// <returns>A <see cref="BuildValueClustersResult"/> containing the generated clusters and associated metadata.</returns>
    /// <summary>
    /// Builds clusters (buckets) from a list of <see cref="long"/> values. 
    /// Clusters can be combined or split based on the specified <paramref name="max"/> size 
    /// and <paramref name="maxSeconds"/> thresholds. Supports progress reporting and cancellation.
    /// </summary>
    /// <param name="values">The list of long values to cluster.</param>
    /// <param name="escapedName">The name of the field used in generated statements, already escaped for safe usage.</param>
    /// <param name="max">Maximum number of items allowed per cluster.</param>
    /// <param name="combine">Indicates whether small clusters should be combined with adjacent clusters.</param>
    /// <param name="maxSeconds">Maximum time span in seconds for a cluster (relevant for date/time representations stored as long).</param>
    /// <param name="progress">Progress tracker that also supports cancellation.</param>
    /// <returns>A <see cref="BuildValueClustersResult"/> containing the generated clusters and associated metadata.</returns>
    private BuildValueClustersResult BuildValueClustersLong(List<long> values, string escapedName, int max,
      bool combine, double maxSeconds, IProgressWithCancellation progress)
    {
      // Get the distinct values and their counts
      var clusterOne = new HashSet<long>();
      var progressAction = IntervalAction.ForProgress(progress);
      var msg = $"Preparing {values.Count:N0} values";
      double percent;
      int counter = 0;
      var stopwatch = Stopwatch.StartNew();

      foreach (var value in values)
      {
        if (stopwatch.Elapsed.TotalSeconds > maxSeconds || progress.CancellationToken.IsCancellationRequested)
          return BuildValueClustersResult.TooManyValues;

        if (clusterOne.Count <= max)
          clusterOne.Add(value);
        counter++;
        percent = counter / (double) values.Count * cPercentBuildEven + cPercentTyped;
        progressAction?.Invoke(progress!, msg, (long) Math.Round(percent * cProgressMax));
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
        while (end - start < max * 2 / 3 && factor > 1)
        {
          if (factor > 10)
            factor = Math.Round(factor / 10.0) * 5;
          else
            factor = Math.Round(factor / 4.0) * 2;
          start = (long) (values.Min() / factor);
          end = (long) (values.Max() / factor);
        }

        fittingCluster = new HashSet<long>();
        for (long i = start; i <= end; i++)
          fittingCluster.Add(i);
      }

      fittingCluster.Add(fittingCluster.Max(x => x) + 1);
      fittingCluster.Add(fittingCluster.Min(x => x) - 1);

      var desiredSize = 1;
      if (combine)
      {
        desiredSize = values.Count * 3 / (max * 2);
        if (desiredSize < 5)
          desiredSize = 5;
      }

      percent = cPercentBuildEven;
      var step = (1.0 - cPercentBuildEven) / fittingCluster.Count;
      msg = "Adding cluster";
      foreach (var dic in fittingCluster.OrderBy(x => x))
      {
        if (stopwatch.Elapsed.TotalSeconds > maxSeconds)
          throw new TimeoutException($"Timeout of {maxSeconds}s reached");

        var minValue = (long) (dic * factor);
        var maxValue = (long) (minValue + factor);

        if (HasEnclosingCluster(m_ValueClusters, minValue, maxValue))
          continue;
        if (factor > 1)
        {
          var count = values.Count(value => value >= minValue && value < maxValue);
          if (count <= 0)
            continue;
          // Combine buckets if the last and the current do not have many values
          if (m_Last.Count + count < desiredSize && m_Last.Start is long lastMin)
          {
            minValue = lastMin;
            count += m_Last.Count;
            // remove the last cluster it will be included with this one
            m_ValueClusters.Remove(m_Last);
          }

          m_Last = new ValueCluster($"[{minValue:N0} to {maxValue:N0})",
            string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", escapedName, minValue, maxValue),
            count, minValue, maxValue);
          m_ValueClusters.Add(m_Last);
        }
        else
        {
          var count = values.Count(value => value == minValue);
          if (count > 0)
          {
            m_ValueClusters.Add(new ValueCluster($"{minValue:N0}",
              string.Format(CultureInfo.InvariantCulture, "{0} = {1}", escapedName, minValue),
              count, minValue, maxValue));
          }
        }

        percent += step;
        progressAction?.Invoke(progress!, msg, (long) Math.Round(percent * cProgressMax));
      }

      return BuildValueClustersResult.ListFilled;
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
    private BuildValueClustersResult BuildValueClustersLongEven(List<long> values, string escapedName,
      int max, double maxSeconds, IProgressWithCancellation progress)
    {
      return BuildValueClustersEven(values, values.Count / max, number => number,
        (minValue, maxValue) => $"{minValue:F0} to {maxValue:F0}",
        (minValue, maxValue) => string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", escapedName,
          minValue, maxValue),
        minValue => $">= {minValue:F0}",
        minValue => string.Format(CultureInfo.InvariantCulture, "({0} >= {1})", escapedName, minValue), maxSeconds, progress);
    }

    /// <summary>
    /// Builds clusters from a list of <see cref="double"/> values. 
    /// Clusters are generated based on the value distribution, with options to combine small clusters 
    /// and limit the maximum cluster size and time span.
    /// </summary>
    /// <param name="values">The list of double values to cluster.</param>
    /// <param name="escapedName">The name of the field used in generated statements, already escaped for safe usage.</param>
    /// <param name="max">Maximum number of items allowed per cluster.</param>
    /// <param name="combine">Whether to combine clusters that do not meet the desired size.</param>
    /// <param name="maxSeconds">Maximum time span in seconds for each cluster.</param>
    /// <param name="progress">Progress tracker that also supports cancellation.</param>
    /// <returns>A <see cref="BuildValueClustersResult"/> containing the generated clusters.</returns>
    private BuildValueClustersResult BuildValueClustersNumeric(List<double> values, string escapedName,
      int max, bool combine, double maxSeconds, IProgressWithCancellation progress)
    {
      // Get the distinct values and their counts
      var clusterFractions = new HashSet<double>();
      var clusterOne = new HashSet<long>();
      var hasFactions = false;
      var progressAction = IntervalAction.ForProgress(progress);
      var msg = $"Preparing {values.Count:N0} values";
      double percent;
      int counter = 0;
      var stopwatch = Stopwatch.StartNew();

      foreach (var value in values)
      {
        if (stopwatch.Elapsed.TotalSeconds > maxSeconds || progress.CancellationToken.IsCancellationRequested)
          return BuildValueClustersResult.TooManyValues;

        if (clusterFractions.Count <= max)
        {
          hasFactions |= value % 1 != 0;
          clusterFractions.Add(Math.Floor(value * 10d) / 10d);
        }

        var key = Convert.ToInt64(value, CultureInfo.CurrentCulture);
        if (clusterOne.Count <= max)
          clusterOne.Add(key);
        counter++;
        percent = counter / (double) values.Count * cPercentBuildEven + cPercentTyped;
        progressAction?.Invoke(progress!, msg, (long) Math.Round(percent * cProgressMax));
      }

      if (clusterOne.Count == 0)
        return BuildValueClustersResult.NoValues;

      if (hasFactions && clusterFractions.Count < max && clusterFractions.Count > 0)
      {
        clusterFractions.Add(clusterFractions.Max(x => x) + .1);
        clusterFractions.Add(clusterFractions.Min(x => x) - .1);
        foreach (var minValue in clusterFractions.OrderBy(x => x))
        {
          if (stopwatch.Elapsed.TotalSeconds > maxSeconds || progress.CancellationToken.IsCancellationRequested)
            return BuildValueClustersResult.TooManyValues;

          var maxValue = minValue + .1;
          if (HasEnclosingCluster(m_ValueClusters, minValue, maxValue))
            continue;
          var count = values.Count(value => value >= minValue && value < maxValue);
          if (count > 0)
            Add(new ValueCluster($"{minValue:F1} to {maxValue:F1}", // Fixed Point
              string.Format(CultureInfo.InvariantCulture, "({0} >= {1:F1} AND {0} < {2:F1})", escapedName, minValue,
                maxValue),
              count, minValue, maxValue));
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
        while (end - start < max * 2 / 3 && factor > 1)
        {
          if (factor > 10)
            factor = Math.Round(factor / 10.0) * 5;
          else
            factor = Math.Round(factor / 4.0) * 2;
          start = (long) (values.Min() / factor);
          end = (long) (values.Max() / factor);
        }

        fittingCluster = new HashSet<long>();
        for (long i = start; i <= end; i++)
          fittingCluster.Add(i);
      }

      fittingCluster.Add(fittingCluster.Max(x => x) + 1);
      fittingCluster.Add(fittingCluster.Min(x => x) - 1);

      var desiredSize = 1;
      if (combine)
      {
        desiredSize = values.Count * 3 / (max * 2);
        if (desiredSize < 5)
          desiredSize = 5;
      }

      percent = cPercentBuildEven;
      var step = (1.0 - cPercentBuildEven) / fittingCluster.Count;
      foreach (var dic in fittingCluster.OrderBy(x => x))
      {
        if (stopwatch.Elapsed.TotalSeconds > maxSeconds || progress.CancellationToken.IsCancellationRequested)
          return BuildValueClustersResult.TooManyValues;

        var minValue = (long) (dic * factor);
        var maxValue = (long) (minValue + factor);
        msg = $"Adding cluster {minValue:N0} to {maxValue:N0}";
        if (HasEnclosingCluster(m_ValueClusters, minValue, maxValue))
          continue;
        if (factor > 1)
        {
          var count = values.Count(value => value >= minValue && value < maxValue);
          if (count <= 0)
            continue;
          // Combine buckets if the last and the current do not have many values
          if (m_Last.Count + count < desiredSize && m_Last.Start is long lastMin)
          {
            minValue = lastMin;
            count += m_Last.Count;
            // remove the last cluster it will be included with this one
            m_ValueClusters.Remove(m_Last);
          }

          m_Last = new ValueCluster($"[{minValue:N0} to {maxValue:N0})",
            string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", escapedName, minValue, maxValue),
            count, minValue, maxValue);
          m_ValueClusters.Add(m_Last);
        }
        else
        {
          var count = values.Count(value => Math.Abs(value - minValue) < .1);
          if (count > 0)
          {
            m_ValueClusters.Add(new ValueCluster($"{minValue:N0}",
              string.Format(CultureInfo.InvariantCulture, "{0} = {1}", escapedName, minValue),
              count, minValue, maxValue));
          }
        }

        percent += step;
        progressAction?.Invoke(progress!, msg, (long) Math.Round(percent * cProgressMax));
      }

      return BuildValueClustersResult.ListFilled;
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
    private BuildValueClustersResult BuildValueClustersNumericEven(List<double> values, string escapedName,
      int max, double maxSeconds, IProgressWithCancellation progress)
    {
      return BuildValueClustersEven(values, values.Count / max, number => Math.Floor(number * 10d) / 10d,
        (minValue, maxValue) => $"{minValue:F1} to {maxValue:F1}",
        (minValue, maxValue) => string.Format(CultureInfo.InvariantCulture, "({0} >= {1:F1} AND {0} < {2:F1})",
          escapedName, minValue, maxValue),
        minValue => $">= {minValue:F1}",
        minValue => string.Format(CultureInfo.InvariantCulture, "({0} >= {1:F1})", escapedName, minValue), maxSeconds, progress);
    }

    /// <summary>
    /// Builds clusters from a set of string values. Clusters are based on the occurrence and distribution
    /// of the values, up to a specified maximum per cluster, without combining smaller clusters.
    /// </summary>
    /// <param name="values">The array of string values to cluster.</param>
    /// <param name="escapedName">The name of the field used in generated statements, already escaped for safe usage.</param>
    /// <param name="max">Maximum number of items allowed per cluster.</param>
    /// <param name="maxSeconds">Maximum time span in seconds considered for each cluster.</param>
    /// <param name="progress">Progress tracker that also supports cancellation.</param>
    /// <returns>A <see cref="BuildValueClustersResult"/> containing the generated string clusters.</returns>
    private BuildValueClustersResult BuildValueClustersString(string[] values, string escapedName,
      int max, double maxSeconds, IProgressWithCancellation progress)
    {
      // Define cluster lengths
      int[] clusterLengths = { 1, 3, 5, 10 };
      // Initialize clusters dictionary and allow dictionary
      var clusters = clusterLengths.ToDictionary(
          length => length,
          length => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
      );
      // Get the distinct values and their counts
      var clusterIndividual = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var progressAction = IntervalAction.ForProgress(progress);
      int counterValues = 0;
      var stopwatch = Stopwatch.StartNew();

      var allowedLengths = new List<int>(clusterLengths);
      foreach (var text in values)
      {
        if (stopwatch.Elapsed.TotalSeconds > maxSeconds || progress.CancellationToken.IsCancellationRequested)
          return BuildValueClustersResult.TooManyValues;

        if (text == null)
          continue;
        if (clusters[1].Count > max)
          break;
        if (clusterIndividual.Count < max)
          clusterIndividual.Add(text);
        for (int i = allowedLengths.Count - 1; i >= 0; i--)
        {
          int length = allowedLengths[i];
          if (text.Length >= length)
          {
            if (clusters[length].Count < max)
              clusters[length].Add(text.Substring(0, length));
          }
          else
          {
            // Remove this length from allowedLengths if text is too short
            allowedLengths.RemoveAt(i);
          }
        }
        counterValues++;
        progressAction!.Invoke(progress, "Building clusters", (long) Math.Round(counterValues / (double) values.Length * cPercentBuild));
      }

      if (clusterIndividual.Count == 0)
        return BuildValueClustersResult.NoValues;

      var percent = cPercentBuild;
      // Only a few distinct values
      if (clusterIndividual.Count < max)
      {
        var distinctValues = clusterIndividual.OrderBy(x => x).ToArray();
        var step = (1.0 - cPercentBuild) / clusterIndividual.Count;
        foreach (var text in distinctValues)
        {
          if (stopwatch.Elapsed.TotalSeconds > maxSeconds || progress.CancellationToken.IsCancellationRequested)
            return BuildValueClustersResult.TooManyValues;
          var count = CountPassing(values,
            span => span.Equals(
              text.AsSpan(), StringComparison.OrdinalIgnoreCase), progress.CancellationToken);
          if (count > 0)
            AddUnique(new ValueCluster(text, $"({escapedName} = '{text.SqlQuote()}')", count, text));

          percent += step;
          progressAction!.Invoke(progress, $"Clusters {text.SqlQuote()}", (long) Math.Round(percent * cProgressMax));
        }
      }
      // Using the initial char would already be good
      // maybe we can use even longer text
      else if (clusters[1].Count < max)
      {
        // find the best cluster the longer the better
        var bestCluster = allowedLengths
                            .OrderByDescending(l => l)
                            .Where(x => clusters[x].Count>=1 && clusters[x].Count < max)
                            .Select(x => clusters[x])
                            .First();
        int prefixLength = bestCluster.First().Length; // all same length

        // Build dictionary keyed by ReadOnlyMemory<char>
        var clusterLookup = new Dictionary<ReadOnlyMemory<char>, ValueCluster>(
            bestCluster.Count, new ReadOnlyMemoryCharComparer());

        foreach (var text in bestCluster)
          clusterLookup[text.AsMemory()] = new ValueCluster($"{text}…", $"({escapedName} LIKE '{text.StringEscapeLike()}%')", 0, text);

        _ = CountPassing(values, span =>
        {
          if (span.Length < prefixLength)
            return false;

          // Take the prefix as ReadOnlyMemory<char> and find the right cluster
          if (clusterLookup.TryGetValue(span.Slice(0, prefixLength).ToArray().AsMemory(), out var cluster))
            cluster.Count++;

          return false;
        }, progress.CancellationToken);

        foreach (var cluster in clusterLookup.Values)
          AddUnique(cluster);
      }
      else
      {
        // No suitable start with found
        var step = (1.0 - cPercentBuild) / 8;
        var countC1 = CountPassing(values,
          x =>
          {
            var c = char.ToUpper(x[0]);
            return c >= 'A' && c <= 'E';
          }, progress.CancellationToken);
        percent += step;

        progressAction!.Invoke(progress, "Range clusters A-E", (long) Math.Round(percent * cProgressMax));
        if (countC1 > 0)
          AddUnique(new ValueCluster("A-E",
            $"(SUBSTRING({escapedName},1,1) >= 'a' AND SUBSTRING({escapedName},1,1) <= 'e')", countC1, "a", "b"));

        var countC2 = CountPassing(values,
          x =>
          {
            var c = char.ToUpper(x[0]);
            return c >= 'F' && c <= 'K';
          }, progress.CancellationToken);
        percent += step;
        progressAction!.Invoke(progress, "Range clusters F-K", (long) Math.Round(percent * cProgressMax));
        if (countC2 > 0)
          AddUnique(new ValueCluster("F-K",
            $"(SUBSTRING({escapedName},1,1) >= 'f' AND SUBSTRING({escapedName},1,1) <= 'k')", countC2, "f", "k"));

        var countC3 = CountPassing(values,
          x =>
          {
            var c = char.ToUpper(x[0]);
            return c >= 'L' && c <= 'R';
          }, progress.CancellationToken);
        percent += step;
        progressAction!.Invoke(progress, "Range clusters L-R", (long) Math.Round(percent * cProgressMax));

        if (countC3 > 0)
          AddUnique(new ValueCluster("L-R",
            $"(SUBSTRING({escapedName},1,1) >= 'l' AND SUBSTRING({escapedName},1,1) <= 'r')", countC3, "l", "r"));

        var countC4 = CountPassing(values,
          x =>
          {
            var c = char.ToUpper(x[0]);
            return c >= 'S' && c <= 'Z';
          }, progress.CancellationToken);
        percent += step;
        progressAction!.Invoke(progress, "Range clusters S-Z", (long) Math.Round(percent * cProgressMax));
        if (countC4 > 0)
          AddUnique(new ValueCluster("S-Z",
            $"(SUBSTRING({escapedName},1,1) >= 's' AND SUBSTRING({escapedName},1,1) <= 'z')", countC4, "s", "z"));

        var countN = CountPassing(values, x => x[0] >= 48 && x[0] <= 57, progress.CancellationToken);
        percent += step;
        progressAction!.Invoke(progress, "Range clusters 0-9", (long) Math.Round(percent * cProgressMax));
        if (countN > 0)
          AddUnique(new ValueCluster("0-9",
            $"(SUBSTRING({escapedName},1,1) >= '0' AND SUBSTRING({escapedName},1,1) <= '9')", countN, "0", "9"));

        var countS = CountPassing(values, x => x[0] < 32, progress.CancellationToken);
        percent += step;
        progressAction!.Invoke(progress, "Range clusters Special", (long) Math.Round(percent * cProgressMax));

        if (countS > 0)
          AddUnique(new ValueCluster("Special", $"(SUBSTRING({escapedName},1,1) < ' ')", countS, null));

        var countP = CountPassing(values, x =>
                  x[0] >= ' ' && x[0] <= '/'
               || x[0] >= ':' && x[0] <= '@'
               || x[0] >= '[' && x[0] <= '`'
               || x[0] >= '{' && x[0] <= '~', progress.CancellationToken);
        percent += step;
        progressAction!.Invoke(progress, "Range clusters Punctuation, Marks and Braces", (long) Math.Round(percent * cProgressMax));
        if (countP > 0)
          AddUnique(new ValueCluster("Punctuation & Marks & Braces",
            $"((SUBSTRING({escapedName},1,1) >= ' ' AND SUBSTRING({escapedName},1,1) <= '/') " +
            $"OR (SUBSTRING({escapedName},1,1) >= ':' AND SUBSTRING({escapedName},1,1) <= '@') " +
            $"OR (SUBSTRING({escapedName},1,1) >= '[' AND SUBSTRING({escapedName},1,1) <= '`') " +
            $"OR (SUBSTRING({escapedName},1,1) >= '{{' AND SUBSTRING({escapedName},1,1) <= '~'))", countP, null));

        var countR = values.Length - countS - countN - countC1 - countC2 - countC3 - countC4 - countP;
        progressAction!.Invoke(progress, "Range clusters Other", cProgressMax);
        if (countR > 0)
          AddUnique(new ValueCluster("Other", $"(SUBSTRING({escapedName},1,1) > '~')", countR, null));
      }

      return progress.CancellationToken.IsCancellationRequested
        ? BuildValueClustersResult.TooManyValues
        : BuildValueClustersResult.ListFilled;

      static int CountPassing(string[] values, SpanPredicate passing, CancellationToken cancellationToken)
      {
        int count = 0;
        const int checkInterval = 100;
        for (int i = 0; i < values.Length; i++)
        {
          if ((i % checkInterval) == 0 && cancellationToken.IsCancellationRequested)
            break;

          var xSpan = values[i].AsSpan();
          if (!xSpan.IsEmpty && passing(xSpan))
            count++;
        }
        return count;
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

    // Custom comparer for ReadOnlyMemory<char> that ignores case
    class ReadOnlyMemoryCharComparer : IEqualityComparer<ReadOnlyMemory<char>>
    {
      public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) =>
          x.Span.Equals(y.Span, StringComparison.OrdinalIgnoreCase);

      public int GetHashCode(ReadOnlyMemory<char> obj)
      {
        // FNV-1a hash on lower-case chars
        uint hash = 2166136261;
        foreach (var c in obj.Span)
        {
          char lower = char.ToLowerInvariant(c);
          hash = (hash ^ lower) * 16777619;
        }
        return (int) hash;
      }
    }
  }
}
