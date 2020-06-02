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

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace CsvTools
{
  /// <summary>
  ///   ValueClusterCollection
  /// </summary>
  public class ValueClusterCollection
  {
    private const long c_TicksPerGroup = TimeSpan.TicksPerMinute * 30;

    private readonly List<ValueCluster> m_ValueClusters = new List<ValueCluster>();
    private readonly int m_MaxNumber;

    private BuildValueClustersResult m_Result = BuildValueClustersResult.NotRun;

     
    /// <param name="maxNumber">The maximum number.</param>
    public ValueClusterCollection(int maxNumber)
    {
      m_MaxNumber = maxNumber < 1 ? int.MaxValue : maxNumber;
    }
    /// <summary>
    ///   Gets the values.
    /// </summary>
    /// <value>The values.</value>
    public ICollection<ValueCluster> ValueClusters
    {
      get
      {
        Contract.Ensures(Contract.Result<IEnumerable<ValueCluster>>() != null);
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
      Contract.Requires(dataView != null);
      if (m_Result != BuildValueClustersResult.NotRun) return m_Result;

      try
      {
        if (columnType == typeof(string) || columnType == typeof(bool) || columnType== typeof(Guid))
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
    public IEnumerable<ValueCluster> GetActiveValueCluster()
    {
      return m_ValueClusters.Where(value => !string.IsNullOrEmpty(value.Display) && value.Active);
    }

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
          clusterHour.Add(value.TimeOfDay.Ticks / c_TicksPerGroup);
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
          .Take(m_MaxNumber)
          .Select(dataRow => Convert.ToDateTime(dataRow[columnIndex], CultureInfo.CurrentCulture))
          .Count(value => value >= minVal && value < maxVal);

      if (clusterDay.Count == 1)
        foreach (var dic in clusterHour.OrderBy(x => x))
        {
          var from = StringConversion.GetTimeFromTicks(dic * c_TicksPerGroup);
          var to = StringConversion.GetTimeFromTicks((dic + 1) * c_TicksPerGroup);
          m_ValueClusters.Add(new ValueCluster($"{from:t} - {to:t}", string.Format(
              CultureInfo.InvariantCulture, @"([{0}] >= #{1:MM\/dd\/yyyy HH:mm}# AND {0} < #{2:MM\/dd\/yyyy HH:mm}#)",
              columnName.SqlName(), from, to), dic.ToString("000000", CultureInfo.InvariantCulture),
            CountDateTime(from, to)));
        }
      else if (clusterDay.Count < m_MaxNumber)
        foreach (var dic in clusterDay.OrderBy(x => x))
          m_ValueClusters.Add(
            new ValueCluster(dic.ToString("d", CultureInfo.CurrentCulture), string.Format(
                CultureInfo.InvariantCulture,
                @"([{0}] >= #{1:MM\/dd\/yyyy}# AND {0} < #{2:MM\/dd\/yyyy}#)",
                columnName.SqlName(),
                dic,
                dic.AddDays(1)), dic.ToString("s", CultureInfo.CurrentCulture),
              CountDateTime(dic, dic.AddDays(1))));
      else if (clusterMonth.Count < m_MaxNumber)
        foreach (var dic in clusterMonth.OrderBy(x => x))
          m_ValueClusters.Add(
            new ValueCluster(dic.ToString("Y", CultureInfo.CurrentCulture), // Year month pattern
              string.Format(CultureInfo.InvariantCulture,
                @"([{0}] >= #{1:MM\/dd\/yyyy}# AND {0} < #{2:MM\/dd\/yyyy}#)",
                columnName.SqlName(),
                dic, dic.AddMonths(1)),
              dic.ToString("s", CultureInfo.InvariantCulture),
              CountDateTime(dic, dic.AddMonths(1))));
      else
        foreach (var dic in clusterYear.OrderBy(x => x))
          m_ValueClusters.Add(
            new ValueCluster(dic.ToString("D", CultureInfo.CurrentCulture), // Decimal
              string.Format(
                CultureInfo.InvariantCulture,
                "([{0}] >= #01/01/{1:d4}# AND {0} < #01/01/{2:d4}#)",
                columnName.SqlName(),
                dic,
                dic + 1),
              dic.ToString("000000", CultureInfo.InvariantCulture),
              CountDateTime(new DateTime(dic, 1, 1), new DateTime(dic + 1, 1, 1))));

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
      var hasNull = false;
      var columnName = dataTable.Columns[columnIndex].ColumnName;
      foreach (DataRow dataRow in dataTable.Rows)
      {
        if (dataRow[columnIndex] == DBNull.Value)
        {
          hasNull = true;
          continue;
        }

        if (columnType == typeof(decimal) || columnType == typeof(float) || columnType == typeof(double))
        {
          var rounded = Math.Floor(Convert.ToDouble(dataRow[columnIndex], CultureInfo.CurrentCulture) * 10d) / 10d;
          clusterFractions.Add(rounded);
        }

        var key = Convert.ToInt64(dataRow[columnIndex], CultureInfo.CurrentCulture);
        if (clusterOne.Count <= m_MaxNumber)
          clusterOne.Add(key);
        if (clusterTen.Count <= m_MaxNumber)
          clusterTen.Add(key / 10);
        if (clusterHundred.Count <= m_MaxNumber)
          clusterHundred.Add(key / 100);

        clusterThousand.Add(key / 1000);

        // if we have more than the maximum entries stop, no value filter will be used
        if (clusterThousand.Count <= m_MaxNumber)
          continue;
        m_ValueClusters.Clear();
        return BuildValueClustersResult.TooManyValues;
      }

      if (clusterOne.Count == 0 && clusterFractions.Count == 0)
      {
        m_ValueClusters.Clear();
        return BuildValueClustersResult.NoValues;
      }

      if (hasNull)
        AddValueClusterNull(dataTable, columnIndex);

      var colNameEsc = $"[{columnName.SqlName()}]";
      if (clusterFractions.Count < m_MaxNumber && clusterFractions.Count > 0)
      {
        foreach (var dic in clusterFractions.OrderBy(x => x))
        {
          var maxValue = dic + .1;
          m_ValueClusters.Add(
            new ValueCluster($"{dic:F1} - {dic + .1:F1}", // Fixed Point
              string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", colNameEsc, dic, dic + .1),
              Convert.ToInt64(dic * 10d).ToString("D18", CultureInfo.InvariantCulture),
              dataTable.Rows.Cast<DataRow>()
                .Where(dataRow => dataRow[columnIndex] != DBNull.Value)
                .Take(m_MaxNumber)
                .Select(dataRow => Convert.ToDouble(dataRow[columnIndex], CultureInfo.CurrentCulture))
                .Count(value => value >= dic && value < maxValue)));
        }
      }
      else if (clusterOne.Count < m_MaxNumber)
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
        else
        {
          factor = 1000;
          fittingCluster = clusterThousand;
        }

        foreach (var dic in fittingCluster.OrderBy(x => x))
        {
          var minValue = dic * factor;
          var maxValue = (dic + 1) * factor;
          m_ValueClusters.Add(
            new ValueCluster(factor == 1 ? $"{dic:N}" : $"{dic * factor:N} - {(dic + 1) * factor - 1:N}", // Decimal
              string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", colNameEsc, dic * factor,
                (dic + 1) * factor),
              dic.ToString("D18", CultureInfo.InvariantCulture),
              dataTable.Rows.Cast<DataRow>()
                .Where(dataRow => dataRow[columnIndex] != DBNull.Value)
                .Take(m_MaxNumber)
                .Select(dataRow => Convert.ToInt64(dataRow[columnIndex], CultureInfo.CurrentCulture))
                .Count(value => value >= minValue && value < maxValue)));
        }
      }

      return BuildValueClustersResult.ListFilled;
    }

    /// <summary>
    ///   Builds the data grid view column filter values.
    /// </summary>
    /// <param name="dataTable">The data view.</param>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersString(DataTable dataTable, int columnIndex)
    {
      Contract.Requires(dataTable != null);
      Contract.Requires(dataTable.Rows != null);

      // Get the distinct values and their counts
      var cluster = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      bool hasNull = false;
      foreach (DataRow dataRow in dataTable.Rows)
      {
        if (dataRow[columnIndex] == DBNull.Value)
        {
          hasNull = true;
          continue;
        }

        cluster.Add(dataRow[columnIndex].ToString());

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
            .Take(m_MaxNumber)
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