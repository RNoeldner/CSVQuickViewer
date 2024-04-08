/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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
using System.Text;
using System.Threading;

namespace CsvTools
{
  public sealed class ValueClusterCollection : ICollection<ValueCluster>
  {
    private const float PercentTyped = 5f;
    private const float PercentBuild = 75f;
    private const float PercentBuildEven = 60f;
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
    /// <param name="progress"></param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long-running process</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public BuildValueClustersResult ReBuildValueClusters(DataTypeEnum type, in ICollection<object> values,
      in string escapedName, bool isActive, int maxNumber = 50,
      bool combine = true, bool even = false, IProgress<ProgressInfo>? progress = null,
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
      if ((values.Count > 50000 && type == DataTypeEnum.Guid))
        return BuildValueClustersResult.TooManyValues;

      try
      {
        progress?.SetMaximum(100);
        if (type == DataTypeEnum.String || type == DataTypeEnum.Guid  || type == DataTypeEnum.Boolean)
        {
          var typedValues = new List<string>();
          var countNull = MakeTypedValues(values, typedValues, Convert.ToString, progress, cancellationToken);
          AddValueClusterNull(escapedName, countNull);
          progress?.Report(new ProgressInfo("Combining values to clusters"));
          return BuildValueClustersString(typedValues, escapedName, maxNumber, 5.0, cancellationToken);
        }

        if (type == DataTypeEnum.DateTime)
        {
          var typedValues = new List<DateTime>();
          var countNull = MakeTypedValues(values, typedValues, Convert.ToDateTime, progress, cancellationToken);

          AddValueClusterNull(escapedName, countNull);
          progress?.Report(even
            ? new ProgressInfo("Combining dates to clusters of even size")
            : new ProgressInfo("Combining dates to clusters"));

          return even ? BuildValueClustersDateEven(typedValues, escapedName, maxNumber, progress, cancellationToken) :
                        BuildValueClustersDate(typedValues, escapedName, maxNumber, combine, progress, cancellationToken);
        }

        if (type == DataTypeEnum.Integer)
        {
          var typedValues = new List<long>();
          var countNull = MakeTypedValues(values, typedValues, Convert.ToInt64, progress, cancellationToken);
          AddValueClusterNull(escapedName, countNull);
          progress?.Report(even
            ? new ProgressInfo("Combining integer to clusters of even size")
            : new ProgressInfo("Combining integer to clusters"));
          return even ? BuildValueClustersLongEven(typedValues, escapedName, maxNumber, progress, cancellationToken) :
            BuildValueClustersLong(typedValues, escapedName, maxNumber, combine, progress, cancellationToken);
        }

        if (type == DataTypeEnum.Numeric || type == DataTypeEnum.Double)
        {
          var typedValues = new List<double>();
          var countNull = MakeTypedValues(values, typedValues, (obj) => Math.Floor(Convert.ToDouble(obj, CultureInfo.CurrentCulture) * 1000d) / 1000d, progress, cancellationToken);
          AddValueClusterNull(escapedName, countNull);
          progress?.Report(even
            ? new ProgressInfo("Combining numbers to clusters of even size")
            : new ProgressInfo("Combining numbers to clusters"));
          return even ? BuildValueClustersNumericEven(typedValues, escapedName, maxNumber, progress, cancellationToken) :
            BuildValueClustersNumeric(typedValues, escapedName, maxNumber, combine, progress, cancellationToken);
        }

        return BuildValueClustersResult.WrongType;
      }
      catch (Exception ex)
      {
        Logger.Error(ex);
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
    private static int MakeTypedValues<T>(in ICollection<object> values, in List<T> typedValues, Func<object, T> convert, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      var countNull = 0;
      var ia = IntervalAction.ForProgress(progress);
      var msg = $"Collecting typed values from {values.Count:N0} rows";
      ia?.Invoke(progress!, msg, 0);
      int counter = 0;
      foreach (var obj in values)
      {
        counter++;

        // Assume process is 5% of overall process
        var percent = counter / (float) values.Count * PercentTyped;
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
        ia?.Invoke(progress!, msg, percent);
      }
      progress?.Report(new ProgressInfo(msg, PercentTyped));
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

    private void AddValueClusterDateTime(in string escapedName, DateTime from, DateTime to,
      IEnumerable<DateTime> values, DateTimeRange displayType, int desiredSize = int.MaxValue)
    {
      if (HasOverlappingCluster(from, to))
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

    private void AddValueClusterNull(in string escapedName, int count)
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
    private BuildValueClustersResult BuildValueClustersDate(in ICollection<DateTime> values, in string escapedName, int max, bool combine, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      // Get the distinct values and their counts
      var clusterYear = new HashSet<int>();
      var clusterMonth = new HashSet<DateTime>();
      var clusterDay = new HashSet<DateTime>();
      var clusterHour = new HashSet<long>();

      var ia = IntervalAction.ForProgress(progress);
      var msg = $"Preparing {values.Count:N0} values";
      var percent = 5f;
      int cValues = 0;

      foreach (var value in values)
      {
        if (cancellationToken.IsCancellationRequested)
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

        cValues++;
        percent = cValues / (float) values.Count * PercentBuildEven + PercentTyped;
        ia?.Invoke(progress!, msg, percent);
      }

      if (clusterYear.Count == 0)
        return BuildValueClustersResult.NoValues;

      var desiredSize = 1;
      if (combine)
      {
        desiredSize = (values.Count * 3) / (max *2);
        if (desiredSize < 5)
          desiredSize=5;
      }
      msg = "Adding cluster";
      percent = PercentBuildEven;
      if (clusterDay.Count == 1)
      {
        clusterHour.Add(clusterHour.Min() -1);
        clusterHour.Add(clusterHour.Max() +1);
        var step = (100 - PercentBuildEven) / clusterHour.Count;
        foreach (var dic in clusterHour.OrderBy(x => x))
        {
          if (cancellationToken.IsCancellationRequested)
            break;
          AddValueClusterDateTime(escapedName, (dic * cTicksPerGroup).GetTimeFromTicks(), ((dic + 1) * cTicksPerGroup).GetTimeFromTicks(), values, DateTimeRange.Hours, desiredSize);
          percent += step;
          ia?.Invoke(progress!, msg, percent);
        }

      }
      else if (clusterDay.Count < max)
      {
        clusterDay.Add(clusterDay.Min().AddDays(-1));
        clusterDay.Add(clusterDay.Max().AddDays(+1));
        var step = (100 - PercentBuildEven) / clusterDay.Count;
        foreach (var dateTime in clusterDay.OrderBy(x => x))
        {
          if (cancellationToken.IsCancellationRequested)
            break;
          AddValueClusterDateTime(escapedName, dateTime, dateTime.AddDays(1), values, DateTimeRange.Days, desiredSize);
          percent += step;
          ia?.Invoke(progress!, msg, percent);
        }
      }
      else if (clusterMonth.Count < max)
      {
        clusterMonth.Add(clusterDay.Min().AddMonths(-1));
        clusterMonth.Add(clusterDay.Max().AddMonths(+1));
        var step = (100 - PercentBuildEven) / clusterMonth.Count;
        foreach (var dateTime in clusterMonth.OrderBy(x => x))
        {
          if (cancellationToken.IsCancellationRequested)
            break;
          AddValueClusterDateTime(escapedName, dateTime, dateTime.AddMonths(1), values, DateTimeRange.Month, desiredSize);
          percent += step;
          ia?.Invoke(progress!, msg, percent);
        }
      }
      else
      {
        clusterYear.Add(clusterYear.Max() +1);
        var step = (100 - PercentBuildEven) / clusterYear.Count;
        foreach (var year in clusterYear.OrderBy(x => x))
        {
          if (cancellationToken.IsCancellationRequested)
            break;
          AddValueClusterDateTime(escapedName, new DateTime(year, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), new DateTime(year + 1, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), values, DateTimeRange.Years, desiredSize);
          percent += step;
          ia?.Invoke(progress!, msg, percent);
        }
      }

      return BuildValueClustersResult.ListFilled;
    }

    private BuildValueClustersResult BuildValueClustersDateEven(in ICollection<DateTime> values, string escapedName,
      int max, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      return BuildValueClustersEven(values, values.Count / max,
        (number) => new DateTime(number.Year, number.Month, number.Day, number.Hour, number.Minute, 0),
        (minValue, maxValue) => $"{minValue:d} – {maxValue:d}",
        (minValue, maxValue) => string.Format(CultureInfo.InvariantCulture,
          "({0} >= #{1:MM/dd/yyyy HH:mm}# AND {0} < #{2:MM/dd/yyyy HH:mm}#)", escapedName, minValue, maxValue),
        (minValue) => $"{minValue:d} – ",
        (minValue) => string.Format(CultureInfo.InvariantCulture, "({0} >= #{1:MM/dd/yyyy HH:mm}#)", escapedName,
          minValue), progress, cancellationToken);
    }

    private BuildValueClustersResult BuildValueClustersEven<T>(in ICollection<T> values, int bucketSize,
      Func<T, T> round, Func<T, T, string> getDisplay, Func<T, T, string> getStatement, Func<T, string> getDisplayLast,
      Func<T, string> getStatementLast, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken) where T : IComparable<T>
    {
      var counter = new Dictionary<T, int>();
      var ia = IntervalAction.ForProgress(progress);
      var msg = $"Preparing {values.Count:N0} values";
      var percent = 5f;
      int cValues = 0;

      foreach (var number in values)
      {
        if (cancellationToken.IsCancellationRequested)
          break;
        var rounded = round(number);
        if (!counter.ContainsKey(rounded))
          counter[rounded] = 1;
        else
          counter[rounded]++;
        cValues++;
        percent = cValues / (float) values.Count * PercentBuildEven + PercentTyped;
        ia?.Invoke(progress!, msg, percent);
      }

      var bucketCount = 0;
      var ordered = counter.OrderBy(x => x.Key).ToArray();
      var minValue = ordered[0].Key;

      var hasPrevious = m_ValueClusters.Any(x => x.Start is T);
      percent = PercentBuildEven;
      var step = (100 - PercentBuildEven) / ordered.Length;
      msg = $"Adding ordered {ordered.Length:N0} values";
      foreach (var keyValue in ordered)
      {
        if (cancellationToken.IsCancellationRequested)
          break;
        bucketCount += keyValue.Value;
        // progress keyValue.Key next bucket
        if (bucketCount <= bucketSize)
          continue;

        // In case there is a cluster that is overlapping do not add a cluster
        if (!hasPrevious || !HasOverlappingCluster(minValue, keyValue.Key))
          AddUnique(new ValueCluster(getDisplay(minValue, keyValue.Key), getStatement(minValue, keyValue.Key),
            bucketCount, minValue, keyValue.Key));

        minValue = keyValue.Key;
        bucketCount = keyValue.Value;
        percent += step;
        ia?.Invoke(progress!, msg, percent);
      }

      // Make one last bucket for the rest
      if (!hasPrevious || !m_ValueClusters.Any(x => x.End == null || x.End is T se && se.CompareTo(minValue) >= 0))
        AddUnique(new ValueCluster(getDisplayLast(minValue), getStatementLast(minValue), bucketCount, minValue));

      return BuildValueClustersResult.ListFilled;
    }

    private BuildValueClustersResult BuildValueClustersLong(in ICollection<long> values, in string escapedName, int max,
      bool combine, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      // Get the distinct values and their counts      
      var clusterOne = new HashSet<long>();
      var ia = IntervalAction.ForProgress(progress);
      var msg = $"Preparing {values.Count:N0} values";
      var percent = 5f;
      int cValues = 0;

      foreach (var value in values)
      {
        if (cancellationToken.IsCancellationRequested)
          break;
        if (clusterOne.Count <= max)
          clusterOne.Add(value);
        cValues++;
        percent = cValues / (float) values.Count * PercentBuildEven + PercentTyped;
        ia?.Invoke(progress!, msg, percent);
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
        while (end-start<(max*2)/3 && factor>1)
        {
          if (factor > 10)
            factor = Math.Round(factor / 10.0) * 5;
          else
            factor = Math.Round(factor / 4.0) * 2;
          start = (long) (values.Min() / factor);
          end = (long) (values.Max() / factor);
        }
        fittingCluster = new HashSet<long>();
        for (long i = start; i<=end; i++)
          fittingCluster.Add(i);
      }
      fittingCluster.Add(fittingCluster.Max(x => x)+1);
      fittingCluster.Add(fittingCluster.Min(x => x)-1);

      var desiredSize = 1;
      if (combine)
      {
        desiredSize = (values.Count * 3) / (max * 2);
        if (desiredSize < 5)
          desiredSize=5;
      }
      percent = PercentBuildEven;
      var step = (100 - PercentBuildEven) / fittingCluster.Count;
      msg = "Adding cluster";
      foreach (var dic in fittingCluster.OrderBy(x => x))
      {
        if (cancellationToken.IsCancellationRequested)
          break;
        var minValue = (long) (dic * factor);
        var maxValue = (long) (minValue + factor);

        if (HasOverlappingCluster(minValue, maxValue))
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
          m_Last = new ValueCluster($"[{minValue:N0},{maxValue:N0})", string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", escapedName, minValue, maxValue), count, minValue, maxValue);
          m_ValueClusters.Add(m_Last);
        }
        else
        {
          var count = values.Count(value => value == minValue);
          if (count>0)
          {
            m_ValueClusters.Add(new ValueCluster($"{minValue:N0}",
              string.Format(CultureInfo.InvariantCulture, "{0} = {1}", escapedName, minValue),
              count, minValue, maxValue));
          }
        }
        percent += step;
        ia?.Invoke(progress!, msg, percent);
      }
      return BuildValueClustersResult.ListFilled;
    }

    private BuildValueClustersResult BuildValueClustersLongEven(in ICollection<long> values, string escapedName,
      int max, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      return BuildValueClustersEven(values, values.Count / max, (number) => number,
        (minValue, maxValue) => $"{minValue:F0} - {maxValue:F0}",
        (minValue, maxValue) => string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", escapedName,
          minValue, maxValue),
        (minValue) => $"{minValue:F0} - ",
        (minValue) => string.Format(CultureInfo.InvariantCulture, "({0} >= {1})", escapedName, minValue), progress,
        cancellationToken);
    }

    private BuildValueClustersResult BuildValueClustersNumeric(in ICollection<double> values, in string escapedName,
      int max, bool combine, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      // Get the distinct values and their counts
      var clusterFractions = new HashSet<double>();
      var clusterOne = new HashSet<long>();
      var hasFactions = false;
      var ia = IntervalAction.ForProgress(progress);
      var msg = $"Preparing {values.Count:N0} values";
      var percent = 5f;
      int cValues = 0;

      foreach (var value in values)
      {
        if (cancellationToken.IsCancellationRequested)
          break;
        if (clusterFractions.Count <= max)
        {
          hasFactions |= value % 1 != 0;
          clusterFractions.Add(Math.Floor(value * 10d) / 10d);
        }

        var key = Convert.ToInt64(value, CultureInfo.CurrentCulture);
        if (clusterOne.Count <= max)
          clusterOne.Add(key);
        cValues++;
        percent = cValues / (float) values.Count * PercentBuildEven + PercentTyped;
        ia?.Invoke(progress!, msg, percent);
      }

      if (clusterOne.Count == 0)
        return BuildValueClustersResult.NoValues;

      if (hasFactions && clusterFractions.Count < max && clusterFractions.Count > 0)
      {
        clusterFractions.Add(clusterFractions.Max(x => x) + .1);
        clusterFractions.Add(clusterFractions.Min(x => x) - .1);
        foreach (var minValue in clusterFractions.OrderBy(x => x))
        {
          cancellationToken.ThrowIfCancellationRequested();
          var maxValue = minValue + .1;
          if (HasOverlappingCluster(minValue, maxValue))
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
        while (end-start<(max*2)/3 && factor>1)
        {
          if (factor > 10)
            factor = Math.Round(factor / 10.0) * 5;
          else
            factor = Math.Round(factor / 4.0) * 2;
          start = (long) (values.Min() / factor);
          end = (long) (values.Max() / factor);
        }
        fittingCluster = new HashSet<long>();
        for (long i = start; i<=end; i++)
          fittingCluster.Add(i);
      }
      fittingCluster.Add(fittingCluster.Max(x => x)+1);
      fittingCluster.Add(fittingCluster.Min(x => x)-1);

      var desiredSize = 1;
      if (combine)
      {
        desiredSize = values.Count * 3 / (max * 2);
        if (desiredSize < 5)
          desiredSize=5;
      }

      percent = PercentBuildEven;
      var step = (100 - PercentBuildEven) / fittingCluster.Count;
      foreach (var dic in fittingCluster.OrderBy(x => x))
      {
        if (cancellationToken.IsCancellationRequested)
          break;

        var minValue = (long) (dic * factor);
        var maxValue = (long) (minValue + factor);
        msg = $"Adding cluster {minValue:N0} - {maxValue:N0}";
        if (HasOverlappingCluster(minValue, maxValue))
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
          m_Last = new ValueCluster($"[{minValue:N0},{maxValue:N0})", string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", escapedName, minValue, maxValue), count, minValue, maxValue);
          m_ValueClusters.Add(m_Last);
        }
        else
        {
          var count = values.Count(value => Math.Abs(value - minValue) < .1);
          if (count>0)
          {
            m_ValueClusters.Add(new ValueCluster($"{minValue:N0}",
              string.Format(CultureInfo.InvariantCulture, "{0} = {1}", escapedName, minValue),
              count, minValue, maxValue));
          }
        }
        percent += step;
        ia?.Invoke(progress!, msg, percent);
      }
      return BuildValueClustersResult.ListFilled;
    }

    private BuildValueClustersResult BuildValueClustersNumericEven(in ICollection<double> values, string escapedName,
      int max, IProgress<ProgressInfo>? progress, CancellationToken cancellationToken)
    {
      return BuildValueClustersEven(values, values.Count / max, (number) => Math.Floor(number * 10d) / 10d,
        (minValue, maxValue) => $"{minValue:F1} - {maxValue:F1}",
        (minValue, maxValue) => string.Format(CultureInfo.InvariantCulture, "({0} >= {1:F1} AND {0} < {2:F1})",
          escapedName, minValue, maxValue),
        (minValue) => $"{minValue:F1} - ",
        (minValue) => string.Format(CultureInfo.InvariantCulture, "({0} >= {1:F1})", escapedName, minValue), progress,
        cancellationToken);
    }

    /// <summary>
    ///   Builds the data grid view column filter values.
    /// </summary>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersString(in ICollection<string> values, in string escapedName,
      int max, IProgress<ProgressInfo>? progress, CancellationToken cancellation)
    {
      // Get the distinct values and their counts
      var cluster = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var clusterOne = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var allow2 = true;
      var clusterTwo = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var allow3 = true;
      var clusterThree = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var allow4 = true;
      var clusterFour = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var ia = IntervalAction.ForProgress(progress);
      var msg = $"Building clusters for {values.Count:N0} values";
      var percent = 5f;
      int cValues = 0;
      foreach (var text in values)
      {
        if (cancellation.IsCancellationRequested || clusterOne.Count > max)
          break;
        if (cluster.Count <= max)
          cluster.Add(text);
        if (clusterOne.Count <= max)
          clusterOne.Add(text.Substring(0, 1));
        allow2 &= (text.Length >= 2);
        allow3 &= (text.Length >= 3);
        allow4 &= (text.Length >= 4);

        if (allow2 && clusterTwo.Count <= max)
          clusterTwo.Add(text.Substring(0, 2));
        if (allow3 && clusterThree.Count <= max)
          clusterThree.Add(text.Substring(0, 3));
        if (allow4 && clusterFour.Count <= max)
          clusterFour.Add(text.Substring(0, 4));

        cValues++;
        percent = cValues / (float) values.Count * PercentBuild + PercentTyped;
        ia?.Invoke(progress!, msg, percent);
      }

      if (cluster.Count == 0)
        return BuildValueClustersResult.NoValues;

      if (clusterOne.Count > max)
      {
        NewFunction();

        if (!linkedTokenSource.IsCancellationRequested)
        {
          var countC2 =
            values.Count(x => x.Length > 0 && ((x[0] >= 'f' && x[0] <= 'k') || (x[0] >= 'F' && x[0] <= 'K')));
          AddUnique(new ValueCluster("F-K",
            $"(SUBSTRING({escapedName},1,1) >= 'f' AND SUBSTRING({escapedName},1,1) <= 'k')", countC2, "f", "k"));
          if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
            linkedTokenSource.Cancel();
        }

        if (!linkedTokenSource.IsCancellationRequested)
        {
          var countC3 =
            values.Count(x => x.Length > 0 && ((x[0] >= 'l' && x[0] <= 'r') || (x[0] >= 'L' && x[0] <= 'R')));
          AddUnique(new ValueCluster("L-R",
            $"(SUBSTRING({escapedName},1,1) >= 'l' AND SUBSTRING({escapedName},1,1) <= 'r')", countC2, "l", "r"));
        }

        var countC4 = values.Count(x => x.Length >0 && ((x[0] >='s' && x[0]<='z') || (x[0]>='S' && x[0]<='Z')));
        AddUnique(new ValueCluster("S-Z",
          $"(SUBSTRING({escapedName},1,1) >= 's' AND SUBSTRING({escapedName},1,1) <= 'z')", countC2, "s", "z"));

        var countN = values.Count(x => x.Length >0 && (x[0] > 48 && x[0]< 57));
        AddUnique(new ValueCluster("0-9",
          $"(SUBSTRING({escapedName},1,1) >= '0' AND SUBSTRING({escapedName},1,1) <= '9')", countN, "0", "9"));

        var countS = values.Count(x => x.Length >0 && (x[0] < 32));
        AddUnique(new ValueCluster("Special", $"(SUBSTRING({escapedName},1,1) < ' ')", countS, null));

        var countP = values.Count(x =>
          x.Length > 0 && (x[0] >= 32 && x[0] < 48) || (x[0] >= 58 && x[0] < 65) || (x[0] >= 91 && x[0] <= 96) ||
          (x[0] >= 173 && x[0] <= 176));
        percent += step;
        progress?.Report(new ProgressInfo("Range clusters Punctuation", percent));
        if (countP  > 0)
          AddUnique(new ValueCluster("Punctuation",
          $"((SUBSTRING({escapedName},1,1) >= ' ' AND SUBSTRING({escapedName},1,1) <= '/') " +
          $"OR (SUBSTRING({escapedName},1,1) >= ':' AND SUBSTRING({escapedName},1,1) <= '@') " +
          $"OR (SUBSTRING({escapedName},1,1) >= '[' AND SUBSTRING({escapedName},1,1) <= '`') " +
          $"OR (SUBSTRING({escapedName},1,1) >= '{{' AND SUBSTRING({escapedName},1,1) <= '~'))", countP, null));
        if (cancellation.IsCancellationRequested)
          return BuildValueClustersResult.TooManyValues;
        ia?.Invoke(progress!, msg, 100);
        var countR = values.Count() - countS - countN - countC1 - countC2 - countC3 - countC4 - countP;
        if (countR  > 0)
          AddUnique(new ValueCluster("Other", $"(SUBSTRING({escapedName},1,1) > '~')", countR, null));
        progress?.Report(new ProgressInfo("Range clusters Other", 100));

        return BuildValueClustersResult.ListFilled;
      }

      percent = PercentBuild;
      if (cluster.Count <= max)
      {
        var step = (100f - PercentBuild) /  cluster.Count;
        foreach (var text in cluster.OrderBy(x => x))
        {
          if (cancellation.IsCancellationRequested)
            break;
          if (m_ValueClusters.Any(x => string.Equals(x.Start?.ToString() ?? string.Empty, text))) continue;
          m_Last = new ValueCluster(text, $"({escapedName} = '{text.SqlQuote()}')",
            values.Count(dataRow => string.Equals(dataRow, text, StringComparison.OrdinalIgnoreCase)), text);

          percent += step;
          progress?.Report(new ProgressInfo($"Clusters {text.SqlQuote()}", percent));
          AddUnique(m_Last);
        }
      }
      else
      {
        // Sometimes we still have a single entry for 4 ,
        // make the text as long as possible
        if (clusterFour.Count == 1)
        {
          var test = values.First();
          var i = 4;
          var prevI = -1;
          var maximum = values.Count(x => x.StartsWith(test.Substring(0, 3)));
          while (i < 200)
          {
            if (values.Count(x => x.StartsWith(test.Substring(0, i))) < maximum)
            {
              break;
            }
            prevI = i;
            i++;
            // make the steps bigger later
            if (i>10)
              i++;
            if (i>=50)
              i+=3;
          }

          // TODO: this is not great, since we have only one entry but still better than only having a short text
          AddUnique(new ValueCluster($"{test.Substring(0, prevI - 1)}…",
            $"({escapedName} LIKE '{test.Substring(0, prevI - 1).SqlQuote()}%')", maximum, test.Substring(0, i - 1)));
          return BuildValueClustersResult.ListFilled;
        }

        var clusterBegin = clusterOne;
        if (allow4 && clusterFour.Count <= max)
          clusterBegin = clusterFour;
        else if (allow3 && clusterThree.Count <= max)
          clusterBegin = clusterThree;
        else if (allow2 &&  clusterTwo.Count <= max)
          clusterBegin = clusterTwo;

        // Look at the data "some%", check if there are large blocks in there like somethingXXX is making up 50%
        // add "somethingXXX" and a "something% (without something XXX)"

        var step = (100f - PercentBuild) /  clusterBegin.Count;
        msg = $"Adding Clusters {clusterBegin.Count:N0}";
        ia?.Invoke(progress!, msg, 95);
        foreach (var text in clusterBegin.OrderBy(x => x))
        {
          if (string.IsNullOrEmpty(text))
            continue;
          if (cancellation.IsCancellationRequested)
            break;
          if (m_ValueClusters.Any(x => string.Equals(x.Start?.ToString() ?? string.Empty, text)))
            continue;

          var parts = values.Where(x => x.StartsWith(text, StringComparison.OrdinalIgnoreCase)).ToArray();
          var countAll = parts.Length;
          if (countAll <= 100)
            continue;
          var bigger = new Dictionary<string, int>();
          foreach (var test in parts)
          {
            if (bigger.ContainsKey(test))
              continue;
            var counter = values.Count(y => y.Equals(test, StringComparison.OrdinalIgnoreCase));
            if (counter > countAll / 25)
              bigger.Add(test, counter);
          }

          percent += step;
          progress?.Report(new ProgressInfo($"New Clusters", percent));
          if (bigger.Count > 0)
          {
            var sbExcluded = new StringBuilder();
            sbExcluded.Append($"{escapedName} LIKE '{text.SqlQuote()}%' AND NOT(");
            foreach (var kvp in bigger)
            {
              countAll -= kvp.Value;
              sbExcluded.Append($"{escapedName} = '{kvp.Key.SqlQuote()}' OR");
            }

            sbExcluded.Length -= 3;
            if (countAll > 0)
              AddUnique(new ValueCluster($"{text}… (remaining)", $"({sbExcluded}))", countAll, text));

            foreach (var kvp in bigger)
              AddUnique(
                new ValueCluster($"{kvp.Key}", $"({escapedName} = '{kvp.Key.SqlQuote()}')", kvp.Value, text));
            continue;
          }

          AddUnique(new ValueCluster($"{text}…", $"({escapedName} LIKE '{text.SqlQuote()}%')", countAll, text));
        }
      }

      return cancellation.IsCancellationRequested
        ? BuildValueClustersResult.TooManyValues
        : BuildValueClustersResult.ListFilled;

      void NewFunction(string escapedName, char min1, char max1)
      {
        if (!linkedTokenSource.IsCancellationRequested)
        {
          var countC1 =
            values.Count(x => x.Length > 0 && ((x[0] >= min1 && x[0] <= max1) || (x[0] >= min1. && x[0] <= max2)));
          AddUnique(new ValueCluster("A-E",
            $"(SUBSTRING({escapedName},1,1) >= 'a' AND SUBSTRING({escapedName},1,1) <= 'e')", countC1, min1.ToString(), min2.ToString()));

          if ((DateTime.Now - startTime).TotalSeconds > maxSeconds)
            linkedTokenSource.Cancel();
        }
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
    ///   Determine if there is a cluster present that over arches the given values
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="minValue">Start of Range</param>
    /// <param name="maxVal">End of Range</param>
    /// <returns><c>true</c> if there is already a cluster that covers the range</returns>
    private bool HasOverlappingCluster<T>(T minValue, T maxVal) where T : IComparable<T>
      => m_ValueClusters.Any(x =>
        x.Start is T sv && sv.CompareTo(minValue) <= 0 &&
        (x.End == null || (x.End is T ev && ev.CompareTo(maxVal) > 0)));
  }
}