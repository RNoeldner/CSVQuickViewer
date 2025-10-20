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
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CsvTools
{
  public sealed class ValueClusterCollection : ICollection<ValueCluster>
  {
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

    private const long cProgressMax = 10000;
    private const double cPercentTyped = .5;
    private const double cPercentBuild = .5;
    private const double cPercentBuildEven = .6;

    private const long cTicksPerGroup = TimeSpan.TicksPerMinute * 30;
    private readonly IList<ValueCluster> m_ValueClusters = new List<ValueCluster>();

    private ValueCluster m_Last = new ValueCluster("Dummy", string.Empty, int.MaxValue, null);

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
    /// Parse the data to create clusters for these found values
    /// </summary>
    /// <param name="type">The type of the column</param>
    /// <param name="values">The values to look </param>
    /// <param name="escapedName">The escaped name of th column for the build SQL filter</param>
    /// <param name="isActive">Indicating if the filter is currently active</param>
    /// <param name="maxNumber">Maximum number of clusters to return</param>
    /// <param name="combine">In case clusters are every small combine close clusters, the clusters still have even margins</param>
    /// <param name="even">Build clusters that have roughly the same number of elements, the resulting borders can vary a lot, e:g. 1950-1980, 1980-1985, 1986, 1987</param>
    /// <param name="maxSeconds">Maximum number of seconds to be spent trying to build clusters</param>
    /// <param name="progress">Used to pass on progress information with number of records and percentage </param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public BuildValueClustersResult ReBuildValueClusters(DataTypeEnum type, in ICollection<object> values,
      string escapedName, bool isActive, int maxNumber = 50,
      bool combine = true, bool even = false, double maxSeconds = 5.0, IProgress<ProgressInfo>? progress = null,
      CancellationToken cancellationToken = default)
    {
      if (values is null)
        throw new ArgumentNullException(nameof(values));

      if (maxNumber < 1 || maxNumber > 200)
        maxNumber = 200;

      if (isActive)
        ClearNotActive();
      else
        Clear();

      // For guid it does not make much sense to build clusters, any other type has a limit of 100k, It's just too slow otherwise
      if (values.Count > 50000 && type == DataTypeEnum.Guid)
        return BuildValueClustersResult.TooManyValues;

      try
      {
        progress?.SetMaximum(cProgressMax);
        if (type == DataTypeEnum.String || type == DataTypeEnum.Guid || type == DataTypeEnum.Boolean)
        {
          var typedValues = new List<string>();
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
          var countNull = MakeTypedValues(values, typedValues, Convert.ToString, progress, cancellationToken);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
          AddValueClusterNull(escapedName, countNull);
          progress?.Report(new ProgressInfo("Combining values to clusters"));
          return BuildValueClustersString(typedValues.ToArray(), escapedName, maxNumber, maxSeconds, progress, cancellationToken);
        }

        if (type == DataTypeEnum.DateTime)
        {
          var typedValues = new List<DateTime>();
          var countNull = MakeTypedValues(values, typedValues, Convert.ToDateTime, progress, cancellationToken);
          AddValueClusterNull(escapedName, countNull);
          progress?.Report(even
            ? new ProgressInfo("Combining dates to clusters of even size")
            : new ProgressInfo("Combining dates to clusters"));

          return even
            ? BuildValueClustersDateEven(typedValues, escapedName, maxNumber, maxSeconds, progress, cancellationToken)
            : BuildValueClustersDate(typedValues, escapedName, maxNumber, combine, maxSeconds, progress, cancellationToken);
        }

        if (type == DataTypeEnum.Integer)
        {
          var typedValues = new List<long>();
          var countNull = MakeTypedValues(values, typedValues, Convert.ToInt64, progress, cancellationToken);
          AddValueClusterNull(escapedName, countNull);
          progress?.Report(even
            ? new ProgressInfo("Combining integer to clusters of even size")
            : new ProgressInfo("Combining integer to clusters"));
          return even
            ? BuildValueClustersLongEven(typedValues, escapedName, maxNumber, maxSeconds, progress, cancellationToken)
            : BuildValueClustersLong(typedValues, escapedName, maxNumber, combine, maxSeconds, progress, cancellationToken);
        }

        if (type == DataTypeEnum.Numeric || type == DataTypeEnum.Double)
        {
          var typedValues = new List<double>();
          var countNull = MakeTypedValues(values, typedValues,
            obj => Math.Floor(Convert.ToDouble(obj, CultureInfo.CurrentCulture) * 1000d) / 1000d, progress,
            cancellationToken);
          AddValueClusterNull(escapedName, countNull);
          progress?.Report(even
            ? new ProgressInfo("Combining numbers to clusters of even size")
            : new ProgressInfo("Combining numbers to clusters"));
          return even
            ? BuildValueClustersNumericEven(typedValues, escapedName, maxNumber, maxSeconds, progress, cancellationToken)
            : BuildValueClustersNumeric(typedValues, escapedName, maxNumber, combine, maxSeconds, progress, cancellationToken);
        }

        return BuildValueClustersResult.WrongType;
      }
      catch (Exception ex)
      {
        try { Logger.Error(ex); }
        catch { }


        progress?.Report(new ProgressInfo(ex.Message));
        return BuildValueClustersResult.Error;
      }
    }

    public bool Remove(ValueCluster item) => m_ValueClusters.Remove(item);

    /// <summary>
    /// Convert the object to typed values and count the number of NULL values
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">The objects to cast</param>
    /// <param name="typedValues">The resulting List</param>
    /// <param name="convert"></param>
    /// <param name="progress"></param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
    /// <returns></returns>
    private static int MakeTypedValues<T>(in ICollection<object> values, in List<T> typedValues,
      Func<object, T> convert, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      var countNull = 0;
      var progressAction = IntervalAction.ForProgress(progress);
      var msg = $"Collecting typed values from {values.Count:N0} rows";
      progressAction?.Invoke(progress!, msg, 0);
      // Assume process is 5% of overall process
      int counter = 0;
      foreach (var obj in values)
      {
        counter++;
        if (cancellationToken.IsCancellationRequested)
          break;

        if (obj is DBNull || obj == null)
        {
          countNull++;
          continue;
        }

        try
        {
          typedValues.Add(convert(obj));
        }
        catch
        {
          countNull++;
        }

        progressAction?.Invoke(progress!, msg, (long) Math.Round(counter / (float) values.Count * cPercentTyped * cProgressMax));
      }

      progress?.Report(new ProgressInfo(msg, (long) Math.Round(cPercentTyped * cProgressMax)));
      return countNull;
    }

    private void AddUnique(in ValueCluster item)
    {
      if (item.Count <= 0) return;
      foreach (var existing in m_ValueClusters)
        if (existing.Display.Equals(item.Display, StringComparison.OrdinalIgnoreCase))
          return;
      m_ValueClusters.Add(item);
    }

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
        DateTimeRange.Hours => $"{from:t} – {to:t}",
        DateTimeRange.Days => to == from.AddDays(1) ? $"{from:d}" : $"{from:d} – {to:d}",
        DateTimeRange.Month => to == from.AddMonths(1) ? $"{from:Y}" : $"{from:Y} – {to:Y}",
        DateTimeRange.Years => to == from.AddYears(1) ? $"{from:yyyy}" : $"{from:yyyy} – {to:yyyy}",
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
    ///   Builds the keyValue clusters date.
    /// </summary>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersDate(in ICollection<DateTime> values, string escapedName,
      int max, bool combine, double maxSeconds, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      // Get the distinct values and their counts
      var clusterYear = new HashSet<int>();
      var clusterMonth = new HashSet<DateTime>();
      var clusterDay = new HashSet<DateTime>();
      var clusterHour = new HashSet<long>();
      using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      var startTime = DateTime.Now;
      var progressAction = IntervalAction.ForProgress(progress);
      var msg = $"Preparing {values.Count:N0} values";
      double percent;
      int counter = 0;

      foreach (var value in values)
      {
        if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
          linkedTokenSource.Cancel();

        if (linkedTokenSource.IsCancellationRequested)
          break;
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
          if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
            linkedTokenSource.Cancel();
          if (linkedTokenSource.IsCancellationRequested)
            break;
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
          if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
            linkedTokenSource.Cancel();
          if (linkedTokenSource.IsCancellationRequested)
            break;
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
          if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
            linkedTokenSource.Cancel();
          if (linkedTokenSource.IsCancellationRequested)
            break;
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
          if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
            linkedTokenSource.Cancel();
          if (linkedTokenSource.IsCancellationRequested)
            break;
          AddValueClusterDateTime(escapedName, new DateTime(year, 1, 1, 0, 0, 0, 0, DateTimeKind.Local),
            new DateTime(year + 1, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), values, DateTimeRange.Years, desiredSize);
          percent += step;
          progressAction?.Invoke(progress!, msg, (long) Math.Round(percent * cProgressMax));
        }
      }

      return BuildValueClustersResult.ListFilled;
    }

    private BuildValueClustersResult BuildValueClustersDateEven(in ICollection<DateTime> values, string escapedName,
      int max, double maxSeconds, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      return BuildValueClustersEven(values, values.Count / max,
        number => new DateTime(number.Year, number.Month, number.Day, number.Hour, number.Minute, 0),
        (minValue, maxValue) => $"{minValue:d} – {maxValue:d}",
        (minValue, maxValue) => string.Format(CultureInfo.InvariantCulture,
          "({0} >= #{1:MM/dd/yyyy HH:mm}# AND {0} < #{2:MM/dd/yyyy HH:mm}#)", escapedName, minValue, maxValue),
        minValue => $"{minValue:d} – ",
        minValue => string.Format(CultureInfo.InvariantCulture, "({0} >= #{1:MM/dd/yyyy HH:mm}#)", escapedName,
          minValue), maxSeconds, progress, cancellationToken);
    }

    private BuildValueClustersResult BuildValueClustersEven<T>(in ICollection<T> values, int bucketSize,
      Func<T, T> round, Func<T, T, string> getDisplay, Func<T, T, string> getStatement, Func<T, string> getDisplayLast,
      Func<T, string> getStatementLast, double maxSeconds, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
      where T : struct, IComparable<T>
    {
      var counter = new Dictionary<T, int>();
      var progressAction = IntervalAction.ForProgress(progress);
      using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      var startTime = DateTime.Now;
      var msg = $"Preparing {values.Count:N0} values";
      double percent;
      int counterValues = 0;

      foreach (var number in values)
      {
        if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
          linkedTokenSource.Cancel();

        if (linkedTokenSource.IsCancellationRequested)
          break;
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
        progressAction?.Invoke(progress!, msg, (long) Math.Round(percent * cProgressMax));
      }

      if (counter.Count == 0)
        return BuildValueClustersResult.NoValues;

      var bucketCount = 0;
      var ordered = counter.OrderBy(x => x.Key).ToArray();
      var minValue = ordered[0].Key;

      var hasPrevious = m_ValueClusters.Any(x => x.Start is T);
      percent = cPercentBuildEven;
      var step = (1.0 - cPercentBuildEven) / ordered.Length;
      msg = $"Adding ordered {ordered.Length:N0} values";
      foreach (var keyValue in ordered)
      {
        if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
          linkedTokenSource.Cancel();
        if (linkedTokenSource.IsCancellationRequested)
          break;
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
        progressAction?.Invoke(progress!, msg, (long) Math.Round(percent * cProgressMax));
      }

      // Make one last bucket for the rest
      if (!hasPrevious || !m_ValueClusters.Any(x => x.End == null || x.End is T se && se.CompareTo(minValue) >= 0))
        AddUnique(new ValueCluster(getDisplayLast(minValue), getStatementLast(minValue), bucketCount, minValue));

      return BuildValueClustersResult.ListFilled;
    }

    private BuildValueClustersResult BuildValueClustersLong(in ICollection<long> values, string escapedName, int max,
      bool combine, double maxSeconds, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      // Get the distinct values and their counts      
      var clusterOne = new HashSet<long>();
      var progressAction = IntervalAction.ForProgress(progress);
      var msg = $"Preparing {values.Count:N0} values";
      double percent;
      int counter = 0;
      using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      var startTime = DateTime.Now;

      foreach (var value in values)
      {
        if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
          linkedTokenSource.Cancel();
        if (linkedTokenSource.IsCancellationRequested)
          break;
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
        if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
          linkedTokenSource.Cancel();
        if (linkedTokenSource.IsCancellationRequested)
          break;
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

          m_Last = new ValueCluster($"[{minValue:N0},{maxValue:N0})",
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

    private BuildValueClustersResult BuildValueClustersLongEven(in ICollection<long> values, string escapedName,
      int max, double maxSeconds, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      return BuildValueClustersEven(values, values.Count / max, number => number,
        (minValue, maxValue) => $"{minValue:F0} - {maxValue:F0}",
        (minValue, maxValue) => string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", escapedName,
          minValue, maxValue),
        minValue => $"{minValue:F0} - ",
        minValue => string.Format(CultureInfo.InvariantCulture, "({0} >= {1})", escapedName, minValue), maxSeconds, progress,
        cancellationToken);
    }

    private BuildValueClustersResult BuildValueClustersNumeric(in ICollection<double> values, string escapedName,
      int max, bool combine, double maxSeconds, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      // Get the distinct values and their counts
      var clusterFractions = new HashSet<double>();
      var clusterOne = new HashSet<long>();
      var hasFactions = false;
      var progressAction = IntervalAction.ForProgress(progress);
      var msg = $"Preparing {values.Count:N0} values";
      double percent;
      int counter = 0;
      using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      var startTime = DateTime.Now;

      foreach (var value in values)
      {
        if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
          linkedTokenSource.Cancel();
        if (linkedTokenSource.IsCancellationRequested)
          break;
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
          if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
            linkedTokenSource.Cancel();
          if (linkedTokenSource.IsCancellationRequested)
            break;
          var maxValue = minValue + .1;
          if (HasEnclosingCluster(m_ValueClusters, minValue, maxValue))
            continue;
          var count = values.Count(value => value >= minValue && value < maxValue);
          if (count > 0)
            Add(new ValueCluster($"{minValue:F1} - {maxValue:F1}", // Fixed Point
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
        if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
          linkedTokenSource.Cancel();
        if (linkedTokenSource.IsCancellationRequested)
          break;
        var minValue = (long) (dic * factor);
        var maxValue = (long) (minValue + factor);
        msg = $"Adding cluster {minValue:N0} - {maxValue:N0}";
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

          m_Last = new ValueCluster($"[{minValue:N0},{maxValue:N0})",
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
    delegate bool SpanPredicate(ReadOnlySpan<char> span);
    private BuildValueClustersResult BuildValueClustersNumericEven(in ICollection<double> values, string escapedName,
      int max, double maxSeconds, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      return BuildValueClustersEven(values, values.Count / max, number => Math.Floor(number * 10d) / 10d,
        (minValue, maxValue) => $"{minValue:F1} - {maxValue:F1}",
        (minValue, maxValue) => string.Format(CultureInfo.InvariantCulture, "({0} >= {1:F1} AND {0} < {2:F1})",
          escapedName, minValue, maxValue),
        minValue => $"{minValue:F1} - ",
        minValue => string.Format(CultureInfo.InvariantCulture, "({0} >= {1:F1})", escapedName, minValue), maxSeconds, progress,
        cancellationToken);
    }

    /// <summary>
    ///   Builds the data grid view column filter values.
    /// </summary>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersString(string[] values, string escapedName,
      int max, double maxSeconds, IProgress<ProgressInfo>? progress, CancellationToken cancellation)
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

      int counterValues = 0;

      using var buildCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
      buildCancellation.CancelAfter(TimeSpan.FromSeconds(maxSeconds));

      var allowedLengths = new List<int>(clusterLengths);
      foreach (var text in values)
      {
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
        progress?.Report(new ProgressInfo("Building clusters", (long) Math.Round(counterValues / (double) values.Length * cPercentBuild)));
      }

      if (buildCancellation.IsCancellationRequested)
        return BuildValueClustersResult.TooManyValues;

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
          if (buildCancellation.IsCancellationRequested)
            break;
          var count = CountPassing(values,
            span => span.Equals(
              text.AsSpan(), StringComparison.OrdinalIgnoreCase), buildCancellation.Token);
          if (count > 0)
            AddUnique(new ValueCluster(text, $"({escapedName} = '{text.SqlQuote()}')", count, text));

          percent += step;
          progress?.Report(new ProgressInfo($"Clusters {text.SqlQuote()}", (long) Math.Round(percent * cProgressMax)));
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
        }, buildCancellation.Token);

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
          }, buildCancellation.Token);
        percent += step;
        progress?.Report(new ProgressInfo("Range clusters A-E", (long) Math.Round(percent * cProgressMax)));
        if (countC1 > 0)
          AddUnique(new ValueCluster("A-E",
            $"(SUBSTRING({escapedName},1,1) >= 'a' AND SUBSTRING({escapedName},1,1) <= 'e')", countC1, "a", "b"));

        var countC2 = CountPassing(values,
          x =>
          {
            var c = char.ToUpper(x[0]);
            return c >= 'F' && c <= 'K';
          }, buildCancellation.Token);
        percent += step;
        progress?.Report(new ProgressInfo("Range clusters F-K", (long) Math.Round(percent* cProgressMax)));
        if (countC2 > 0)
          AddUnique(new ValueCluster("F-K",
            $"(SUBSTRING({escapedName},1,1) >= 'f' AND SUBSTRING({escapedName},1,1) <= 'k')", countC2, "f", "k"));

        var countC3 = CountPassing(values,
          x =>
          {
            var c = char.ToUpper(x[0]);
            return c >= 'L' && c <= 'R';
          }, buildCancellation.Token);
        percent += step;
        progress?.Report(new ProgressInfo("Range clusters L-R", (long) Math.Round(percent* cProgressMax)));

        if (countC3 > 0)
          AddUnique(new ValueCluster("L-R",
            $"(SUBSTRING({escapedName},1,1) >= 'l' AND SUBSTRING({escapedName},1,1) <= 'r')", countC3, "l", "r"));

        var countC4 = CountPassing(values,
          x =>
          {
            var c = char.ToUpper(x[0]);
            return c >= 'S' && c <= 'Z';
          }, buildCancellation.Token);
        percent += step;
        progress?.Report(new ProgressInfo("Range clusters S-Z", (long) Math.Round(percent * cProgressMax)));
        if (countC4 > 0)
          AddUnique(new ValueCluster("S-Z",
            $"(SUBSTRING({escapedName},1,1) >= 's' AND SUBSTRING({escapedName},1,1) <= 'z')", countC4, "s", "z"));

        var countN = CountPassing(values, x => x[0] >= 48 && x[0] <= 57, buildCancellation.Token);
        percent += step;
        progress?.Report(new ProgressInfo("Range clusters 0-9", (long) Math.Round(percent * cProgressMax)));
        if (countN > 0)
          AddUnique(new ValueCluster("0-9",
            $"(SUBSTRING({escapedName},1,1) >= '0' AND SUBSTRING({escapedName},1,1) <= '9')", countN, "0", "9"));

        var countS = CountPassing(values, x => x[0] < 32, buildCancellation.Token);
        percent += step;
        progress?.Report(new ProgressInfo("Range clusters Special", (long) Math.Round(percent * cProgressMax)));

        if (countS > 0)
          AddUnique(new ValueCluster("Special", $"(SUBSTRING({escapedName},1,1) < ' ')", countS, null));

        var countP = CountPassing(values, x =>
                  x[0] >= ' ' && x[0] <= '/'
               || x[0] >= ':' && x[0] <= '@'
               || x[0] >= '[' && x[0] <= '`'
               || x[0] >= '{' && x[0] <= '~', buildCancellation.Token);
        percent += step;
        progress?.Report(new ProgressInfo("Range clusters Punctuation, Marks and Braces", (long) Math.Round(percent * cProgressMax)));
        if (countP > 0)
          AddUnique(new ValueCluster("Punctuation & Marks & Braces",
            $"((SUBSTRING({escapedName},1,1) >= ' ' AND SUBSTRING({escapedName},1,1) <= '/') " +
            $"OR (SUBSTRING({escapedName},1,1) >= ':' AND SUBSTRING({escapedName},1,1) <= '@') " +
            $"OR (SUBSTRING({escapedName},1,1) >= '[' AND SUBSTRING({escapedName},1,1) <= '`') " +
            $"OR (SUBSTRING({escapedName},1,1) >= '{{' AND SUBSTRING({escapedName},1,1) <= '~'))", countP, null));

        var countR = values.Length - countS - countN - countC1 - countC2 - countC3 - countC4 - countP;
        progress?.Report(new ProgressInfo("Range clusters Other", cProgressMax));
        if (countR > 0)
          AddUnique(new ValueCluster("Other", $"(SUBSTRING({escapedName},1,1) > '~')", countR, null));
      }

      return buildCancellation.IsCancellationRequested
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

    /// <summary>
    /// Determines whether any existing cluster fully contains the given range.
    /// Supports value types like Numeric(int long byte etc), DateTime, TimeSpan, Guid
    /// </summary>
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
  }
}
