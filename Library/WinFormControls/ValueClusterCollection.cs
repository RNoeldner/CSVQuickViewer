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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace CsvTools
{
  /// <summary>
  ///   ValueClusterCollection
  /// </summary>
  public class ValueClusterCollection
  {
    private const long cTicksPerGroup = TimeSpan.TicksPerMinute * 30;

    private readonly int m_MaxNumber;

    private readonly List<ValueCluster> m_ValueClusters = new List<ValueCluster>();

    private BuildValueClustersResult m_Result = BuildValueClustersResult.NotRun;

    /// <param name="maxNumber">The maximum number.</param>
    public ValueClusterCollection(int maxNumber) => m_MaxNumber = maxNumber < 1 ? int.MaxValue : maxNumber;

    /// <summary>
    ///   Gets the values.
    /// </summary>
    /// <value>The values.</value>
    public ICollection<ValueCluster> ValueClusters
    {
      get
      {
        return m_ValueClusters;
      }
    }

    /// <summary>
    ///   Builds the data grid view column filter values.
    /// </summary>
    /// <param name="dataView">The data view.</param>
    /// <param name="columnType">Type of the column.</param>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns></returns>
    public BuildValueClustersResult BuildValueClusters(
      DataView dataView,
      Type columnType,
      int columnIndex)
    {
      if (dataView.Table is null)
        return BuildValueClustersResult.Error;

      if (m_Result != BuildValueClustersResult.NotRun) return m_Result;
      try
      {
        if (columnType == typeof(string) || columnType == typeof(bool) || columnType == typeof(Guid))
          m_Result = BuildValueClustersString(dataView.Table, columnIndex);
        else if (columnType == typeof(DateTime))
          m_Result = BuildValueClustersDate(dataView.Table, columnIndex);
        else if (columnType == typeof(byte) || columnType == typeof(short) || columnType == typeof(int)
                 || columnType == typeof(uint) || columnType == typeof(int) || columnType == typeof(float)
                 || columnType == typeof(double) || columnType == typeof(long) || columnType == typeof(ulong)
                 || columnType == typeof(decimal))
          m_Result = BuildValueClustersNumeric(dataView.Table, columnIndex, columnType);
        else
          m_Result = BuildValueClustersResult.WrongType;
      }
      catch (Exception ex)
      {
        Logger.Error(ex);
        m_Result = BuildValueClustersResult.Error;
      }

      return m_Result;
    }

    /// <summary>
    ///   Gets the active value cluster.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ValueCluster> GetActiveValueCluster() => m_ValueClusters.Where(value => !string.IsNullOrEmpty(value.Display) && value.Active);

    /// <summary>
    ///   Builds the value clusters date.
    /// </summary>
    /// <param name="dataTable">The data view.</param>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersDate(DataTable dataTable, int columnIndex)
    {
      // Get the distinct values and their counts
      var clusterYear = new HashSet<int>();
      var clusterMonth = new HashSet<DateTime>();
      var clusterDay = new HashSet<DateTime>();
      var clusterHour = new HashSet<long>();
      var hasNull = false;
      var columnName = dataTable.Columns[columnIndex].ColumnName;
      foreach (DataRow dataRow in dataTable.Rows)
      {
        if (dataRow[columnIndex] == DBNull.Value)
        {
          hasNull = true;
          continue;
        }

        var value = (DateTime) dataRow[columnIndex];
        if (clusterHour.Count <= m_MaxNumber)
          clusterHour.Add(value.TimeOfDay.Ticks / cTicksPerGroup);
        if (clusterDay.Count <= m_MaxNumber)
          clusterDay.Add(value.Date);
        if (clusterMonth.Count <= m_MaxNumber)
          clusterMonth.Add(new DateTime(value.Year, value.Month, 1));
        clusterYear.Add(value.Year);

        // if we have more than the maximum entries stop, no value filter will be used
        if (clusterYear.Count <= m_MaxNumber)
          continue;
        m_ValueClusters.Clear();
        return BuildValueClustersResult.TooManyValues;
      }

      if (clusterYear.Count == 0)
      {
        m_ValueClusters.Clear();
        return BuildValueClustersResult.NoValues;
      }

      if (hasNull)
        AddValueClusterNull(dataTable, columnIndex);

      int CountDateTime(DateTime minVal, DateTime maxVal) =>
        dataTable.Rows.Cast<DataRow>()
                 .Where(dataRow => dataRow[columnIndex] != DBNull.Value)
                 .Select(dataRow => Convert.ToDateTime(dataRow[columnIndex], CultureInfo.CurrentCulture))
                 .Count(value => value >= minVal && value < maxVal);

      var colNameEsc = $"[{columnName.SqlName()}]";
      if (clusterDay.Count == 1)
        foreach (var dic in clusterHour.OrderBy(x => x))
        {
          var from = StringConversion.GetTimeFromTicks(dic * cTicksPerGroup);
          var to = StringConversion.GetTimeFromTicks((dic + 1) * cTicksPerGroup);
          var cluster = new ValueCluster($"{from:t} - {to:t}",
            $"({colNameEsc} >= #{from:MM\\/dd\\/yyyy HH:mm}# AND {colNameEsc} < #{to:MM\\/dd\\/yyyy HH:mm}#)",
            dic.ToString("000000", CultureInfo.InvariantCulture),
            CountDateTime(from, to));
          if (cluster.Count > 0)
            m_ValueClusters.Add(cluster);
        }
      else if (clusterDay.Count < m_MaxNumber)
        foreach (var dic in clusterDay.OrderBy(x => x))
        {
          var cluster = new ValueCluster(dic.ToString("d", CultureInfo.CurrentCulture),
            $"({colNameEsc} >= #{dic:MM\\/dd\\/yyyy}# AND {colNameEsc} < #{dic.AddDays(1):MM\\/dd\\/yyyy}#)"
            , dic.ToString("s", CultureInfo.CurrentCulture),
            CountDateTime(dic, dic.AddDays(1)));
          if (cluster.Count > 0)
            m_ValueClusters.Add(cluster);
        }
      else if (clusterMonth.Count < m_MaxNumber)
        foreach (var dic in clusterMonth.OrderBy(x => x))
        {
          var cluster = new ValueCluster(dic.ToString("Y", CultureInfo.CurrentCulture), // Year month pattern
            $"({colNameEsc} >= #{dic:MM\\/dd\\/yyyy}# AND {colNameEsc} < #{dic.AddMonths(1):MM\\/dd\\/yyyy}#)",
            dic.ToString("s", CultureInfo.InvariantCulture),
            CountDateTime(dic, dic.AddMonths(1)));
          if (cluster.Count > 0)
            m_ValueClusters.Add(cluster);
        }
      else
        foreach (var dic in clusterYear.OrderBy(x => x))
        {
          var cluster = new ValueCluster(dic.ToString("D", CultureInfo.CurrentCulture), // Decimal
            $"({colNameEsc} >= #01/01/{dic:d4}# AND {colNameEsc} < #01/01/{dic + 1:d4}#)",
            dic.ToString("000000", CultureInfo.InvariantCulture),
            CountDateTime(new DateTime(dic, 1, 1), new DateTime(dic + 1, 1, 1)));
          if (cluster.Count > 0)
            m_ValueClusters.Add(cluster);
        }

      return BuildValueClustersResult.ListFilled;
    }

    /// <summary>
    ///   Builds the value clusters date.
    /// </summary>
    /// <param name="dataTable">The data view.</param>
    /// <param name="columnIndex">Index of the column.</param>
    /// <param name="columnType">Type of the column.</param>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersNumeric(
      DataTable dataTable,
      int columnIndex,
      Type columnType)
    {
      // Get the distinct values and their counts
      var clusterFractions = new HashSet<double>();
      var clusterOne = new HashSet<long>();
      var clusterTen = new HashSet<long>();
      var clusterHundred = new HashSet<long>();
      var clusterThousand = new HashSet<long>();
      var clusterTenThousand = new HashSet<long>();

      var columnName = dataTable.Columns[columnIndex].ColumnName;

      var values = dataTable.Rows.Cast<DataRow>().Select(dataRow => dataRow[columnIndex]).Where(val => val != DBNull.Value).ToList();

      foreach (var value in values)
      {
        if (clusterFractions.Count <= m_MaxNumber && (columnType == typeof(decimal) || columnType == typeof(float) || columnType == typeof(double)))
        {
          var rounded = Math.Floor(Convert.ToDouble(value, CultureInfo.CurrentCulture) * 10d) / 10d;
          clusterFractions.Add(rounded);
        }

        var key = Convert.ToInt64(value, CultureInfo.CurrentCulture);
        if (clusterOne.Count <= m_MaxNumber)
          clusterOne.Add(key);
        if (clusterTen.Count <= m_MaxNumber)
          clusterTen.Add(key / 10);
        if (clusterHundred.Count <= m_MaxNumber)
          clusterHundred.Add(key / 100);
        if (clusterThousand.Count <= m_MaxNumber)
          clusterThousand.Add(key / 1000);
        clusterTenThousand.Add(key / 10000);

        // if we have more than the maximum entries stop, no value filter will be used
        if (clusterTenThousand.Count > m_MaxNumber)
        {
          m_ValueClusters.Clear();
          return BuildValueClustersResult.TooManyValues;
        }
      }

      if (clusterOne.Count == 0 && clusterFractions.Count == 0)
      {
        m_ValueClusters.Clear();
        return BuildValueClustersResult.NoValues;
      }

      if (dataTable.Rows.Cast<DataRow>().Any(dataRow => dataRow[columnIndex] == DBNull.Value))
        AddValueClusterNull(dataTable, columnIndex);

      var colNameEsc = $"[{columnName.SqlName()}]";

      int counter = 0;
      if (clusterFractions.Count < m_MaxNumber && clusterFractions.Count > 0)
      {
        foreach (var dic in clusterFractions.OrderBy(x => x))
        {
          if (dic < 0 && counter == 0)
          {
            var maxValue2 = dic;
            m_ValueClusters.Add(
              new ValueCluster($"{dic - .1:F1} - {maxValue2:F1}", // Fixed Point
                $"({colNameEsc} >= {dic:F1} AND {colNameEsc} < {dic + .1:F1})",
                counter.ToString("D3"),
                values.Select(dataRow => Convert.ToDouble(dataRow, CultureInfo.CurrentCulture))
                      .Count(value => value >= (dic - .1) && value < maxValue2)));
          }

          counter++;
          var maxValue = dic + .1;
          m_ValueClusters.Add(
            new ValueCluster($"{dic:F1} - {maxValue:F1}", // Fixed Point
              $"({colNameEsc} >= {dic:F1} AND {colNameEsc} < {dic + .1:F1})",
              counter.ToString("D3"),
              values.Select(dataRow => Convert.ToDouble(dataRow, CultureInfo.CurrentCulture))
                    .Count(value => value >= dic && value < maxValue)));
        }
      }
      else
      {
        IEnumerable<long> fittingCluster;
        int factor;
        if (clusterOne.Count < m_MaxNumber)
        {
          factor = 1;
          fittingCluster = clusterOne;
        }
        else if (clusterTen.Count < m_MaxNumber)
        {
          factor = 10;
          fittingCluster = clusterTen;
        }
        else if (clusterHundred.Count < m_MaxNumber)
        {
          factor = 100;
          fittingCluster = clusterHundred;
        }
        else if (clusterThousand.Count < m_MaxNumber)
        {
          factor = 1000;
          fittingCluster = clusterThousand;
        }
        else
        {
          factor = 10000;
          fittingCluster = clusterTenThousand;
        }

        var valuesLong = values.Select(dataRow => Convert.ToInt64(dataRow, CultureInfo.CurrentCulture)).ToList();
        foreach (var dic in fittingCluster.OrderBy(x => x))
        {
          if (dic < 0 && counter == 0)
            AddNumericCluster(valuesLong, dic - 1, factor, colNameEsc, counter++);
          AddNumericCluster(valuesLong, dic, factor, colNameEsc, counter++);
        }
      }

      return BuildValueClustersResult.ListFilled;
    }

    private void AddNumericCluster(IEnumerable<long> values, long dic, int factor, string colNameEsc,
                                   int counter)
    {
      var minValue = dic * factor;
      var maxValue = (dic + 1) * factor;

      var cluster = new ValueCluster((factor > 1) ? $"{minValue:D} to {maxValue:D}" : $"{dic}",
        string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", colNameEsc, minValue, maxValue),
        counter.ToString("D3"),
        values.Count(value => value >= minValue && value < maxValue));

      if (cluster.Count > 0)
        m_ValueClusters.Add(cluster);
    }

    /// <summary>
    ///   Builds the data grid view column filter values.
    /// </summary>
    /// <param name="dataTable">The data view.</param>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersString(DataTable dataTable, int columnIndex)
    {
      // Get the distinct values and their counts
      var cluster = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      var hasNull = false;
      foreach (DataRow dataRow in dataTable.Rows)
      {
        if (dataRow[columnIndex] == DBNull.Value)
        {
          hasNull = true;
          continue;
        }
        var text = dataRow[columnIndex].ToString();
        if (text is null)
          continue;
        cluster.Add(text);

        // if we have more than the maximum entries stop, no value filter will be used
        if (cluster.Count <= m_MaxNumber)
          continue;
        m_ValueClusters.Clear();
        return BuildValueClustersResult.TooManyValues;
      }

      if (cluster.Count == 0)
      {
        m_ValueClusters.Clear();
        return BuildValueClustersResult.NoValues;
      }

      if (hasNull)
        AddValueClusterNull(dataTable, columnIndex);

      var colNameEsc = $"[{dataTable.Columns[columnIndex].ColumnName.SqlName()}]";
      foreach (var text in cluster)
        m_ValueClusters.Add(new ValueCluster(text, $"({colNameEsc} = '{text.SqlQuote()}')", text,
          dataTable.Rows.Cast<DataRow>()
                   .Where(dataRow => dataRow[columnIndex] != DBNull.Value)
                   .Count(dataRow =>
                     string.Equals(dataRow[columnIndex].ToString(), text, StringComparison.OrdinalIgnoreCase))));

      return BuildValueClustersResult.ListFilled;
    }

    private void AddValueClusterNull(DataTable dataTable, int columnIndex)
    {
      m_ValueClusters.Add(
        new ValueCluster(ColumnFilterLogic.OperatorIsNull,
          string.Format($"([{dataTable.Columns[columnIndex].ColumnName.SqlName()}] IS NULL)"),
          string.Empty,
          dataTable.Rows.Cast<DataRow>().Count(dataRow => dataRow[columnIndex] == DBNull.Value)));
    }
  }
}