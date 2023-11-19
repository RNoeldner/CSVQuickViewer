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
using System.Data;
using System.Globalization;
using System.Linq;

namespace CsvTools
{
  /// <summary>
  ///   ValueClusterCollection
  /// </summary>
  public sealed class ValueClusterCollection : ICollection<ValueCluster>
  {
    private const long cTicksPerGroup = TimeSpan.TicksPerMinute * 30;
    private ICollection<object> m_Values;
    private bool m_HasNull;
    private readonly int m_MaxNumber;
    private readonly IList<ValueCluster> m_ValueClusters = new List<ValueCluster>();
    private BuildValueClustersResult m_Result = BuildValueClustersResult.NotRun;
    private readonly ColumnFilterLogic m_FilterLogic;

    /// <summary>
    /// Constructor for ValueClusterCollection
    /// </summary>
    /// <param name="columnFilterLogic">The parent ColumnFilterLogic</param>
    /// <param name="maxNumber">Maximum number of clusters</param>
    public ValueClusterCollection(in ColumnFilterLogic columnFilterLogic, int maxNumber)
    {
      m_FilterLogic = columnFilterLogic;
      m_MaxNumber = maxNumber < 1 ? int.MaxValue : maxNumber;
    }

    /// <summary>
    ///   Gets the m_Values.
    /// </summary>
    /// <value>The m_Values.</value>
    public ICollection<ValueCluster> ValueClusters => m_ValueClusters;

    public int Count => m_ValueClusters.Count;

    public bool IsReadOnly => m_ValueClusters.IsReadOnly;

    public BuildValueClustersResult ReBuildValueClusters(in ICollection<object> values)
    {
      if (values is null) throw new ArgumentNullException(nameof(values));

      m_Values = values;
      m_HasNull = values.Any(x => x  == DBNull.Value);
      try
      {
        ClearNotActive();
        if (m_FilterLogic.ColumnDataType == typeof(string) || m_FilterLogic.ColumnDataType == typeof(bool) || m_FilterLogic.ColumnDataType == typeof(Guid))
          m_Result = BuildValueClustersString();
        else if (m_FilterLogic.ColumnDataType == typeof(DateTime))
          m_Result = BuildValueClustersDate();
        else if (m_FilterLogic.ColumnDataType == typeof(byte) || m_FilterLogic.ColumnDataType == typeof(short) || m_FilterLogic.ColumnDataType == typeof(int)
                 || m_FilterLogic.ColumnDataType == typeof(uint) || m_FilterLogic.ColumnDataType == typeof(int) || m_FilterLogic.ColumnDataType == typeof(float)
                 || m_FilterLogic.ColumnDataType == typeof(double) || m_FilterLogic.ColumnDataType   == typeof(long) || m_FilterLogic.ColumnDataType == typeof(ulong)
                 || m_FilterLogic.ColumnDataType == typeof(decimal))
          m_Result = BuildValueClustersNumeric();
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
    public IEnumerable<ValueCluster> GetActiveValueCluster() =>
      m_ValueClusters.Where(value => !string.IsNullOrEmpty(value.Display) && value.Active);

    /// <summary>
    ///   Builds the value clusters date.
    /// </summary>
    /// <param name="dataTable">The data view.</param>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersDate()
    {
      // Get the distinct m_Values and their counts
      var clusterYear = new HashSet<int>();
      var clusterMonth = new HashSet<DateTime>();
      var clusterDay = new HashSet<DateTime>();
      var clusterHour = new HashSet<long>();
      
      foreach (var dataRow in m_Values)
      {
        if (dataRow == DBNull.Value)
        {
          m_HasNull = true;
          continue;
        }
        var value = (DateTime) dataRow;
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
        return BuildValueClustersResult.TooManyValues;
      }

      if (clusterYear.Count == 0)
      {        
        return BuildValueClustersResult.NoValues;
      }

      if (m_HasNull)
        AddValueClusterNull();

      int CountDateTime(DateTime minVal, DateTime maxVal) =>
        m_Values.Where(dataRow => dataRow != DBNull.Value)
          .Select(dataRow => Convert.ToDateTime(dataRow, CultureInfo.CurrentCulture))
          .Count(value => value >= minVal && value < maxVal);

      if (clusterDay.Count == 1)
        foreach (var dic in clusterHour.OrderBy(x => x))
        {
          var from = (dic * cTicksPerGroup).GetTimeFromTicks();
          var to = ((dic + 1) * cTicksPerGroup).GetTimeFromTicks();
          var cluster = new ValueCluster($"{from:t} - {to:t}",
            $"({m_FilterLogic.DataPropertyNameEscaped} >= #{from.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture)}# AND {m_FilterLogic.DataPropertyNameEscaped} < #{to.ToString("MM/dd/yyyy HH:mm", CultureInfo.InvariantCulture)}#)",
            dic.ToString("000000", CultureInfo.InvariantCulture),
            CountDateTime(from, to), from, to);
          if (cluster.Count > 0)
            Add(cluster);
        }
      else if (clusterDay.Count < m_MaxNumber)
        foreach (var dic in clusterDay.OrderBy(x => x))
        {
          var cluster = new ValueCluster(dic.ToString("d", CultureInfo.CurrentCulture),
            $"({m_FilterLogic.DataPropertyNameEscaped} >= #{dic.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)}# AND {m_FilterLogic.DataPropertyNameEscaped} < #{dic.AddDays(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)}#)"
            , dic.ToString("s", CultureInfo.CurrentCulture),
            CountDateTime(dic, dic.AddDays(1)), dic, dic.AddDays(1));
          if (cluster.Count > 0)
            Add(cluster);
        }
      else if (clusterMonth.Count < m_MaxNumber)
        foreach (var dic in clusterMonth.OrderBy(x => x))
        {
          var cluster = new ValueCluster(dic.ToString("Y", CultureInfo.CurrentCulture), // Year month pattern
            $"({m_FilterLogic.DataPropertyNameEscaped} >= #{dic.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)}# AND {m_FilterLogic.DataPropertyNameEscaped} < #{dic.AddMonths(1).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)}#)",
            dic.ToString("s", CultureInfo.InvariantCulture),
            CountDateTime(dic, dic.AddMonths(1)), dic, dic.AddMonths(1));
          if (cluster.Count > 0)
            Add(cluster);
        }
      else
        foreach (var dic in clusterYear.OrderBy(x => x))
        {
          var from = new DateTime(dic + 1, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
          var to = new DateTime(dic+1, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
          var cluster = new ValueCluster(dic.ToString("D", CultureInfo.CurrentCulture), // Decimal
            $"({m_FilterLogic.DataPropertyNameEscaped} >= #01/01/{dic:d4}# AND {m_FilterLogic.DataPropertyNameEscaped} < #01/01/{dic + 1:d4}#)",
            dic.ToString("000000", CultureInfo.InvariantCulture),
            CountDateTime(from, to), from, to);
          if (cluster.Count > 0)
            Add(cluster);
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
    private BuildValueClustersResult BuildValueClustersNumeric()
    {
      // Get the distinct m_Values and their counts
      var clusterFractions = new HashSet<double>();
      var clusterOne = new HashSet<long>();
      var clusterTen = new HashSet<long>();
      var clusterHundred = new HashSet<long>();
      var clusterThousand = new HashSet<long>();
      var clusterTenThousand = new HashSet<long>();      

      foreach (var value in m_Values)
      {
        if (value is DBNull)
          continue;

        if (clusterFractions.Count <= m_MaxNumber && (m_FilterLogic.ColumnDataType == typeof(decimal) || m_FilterLogic.ColumnDataType == typeof(float) || m_FilterLogic.ColumnDataType == typeof(double)))
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
          return BuildValueClustersResult.TooManyValues;
        }
      }

      if (clusterOne.Count == 0 && clusterFractions.Count == 0)
      {        
        return BuildValueClustersResult.NoValues;
      }

      if (m_HasNull)
        AddValueClusterNull();

      if (clusterFractions.Count < m_MaxNumber && clusterFractions.Count > 0)
      {
        foreach (var dic in clusterFractions.OrderBy(x => x))
        {
          var minValue = dic;
          var maxValue = dic + .1;
          if (dic < 0 && m_ValueClusters.Count == 0)
          {
            minValue = dic - .1;
            maxValue = dic;
          }

          Add(new ValueCluster($"{minValue:F1} - {maxValue:F1}", // Fixed Point
              $"({m_FilterLogic.DataPropertyNameEscaped} >= {minValue.ToString("F1", CultureInfo.InvariantCulture)} AND {m_FilterLogic.DataPropertyNameEscaped} < {maxValue.ToString("F1", CultureInfo.InvariantCulture)})",
              m_ValueClusters.Count.ToString("D3"),
              m_Values.Select(dataRow => Convert.ToDouble(dataRow, CultureInfo.CurrentCulture))
                .Count(value => value >= minValue && value < maxValue), minValue, maxValue));
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

        var counter = 0;
        var valuesLong = m_Values.Where(x => x != null && x != DBNull.Value).Select(dataRow => Convert.ToInt64(dataRow, CultureInfo.CurrentCulture)).ToList();
        foreach (var dic in fittingCluster.OrderBy(x => x))
        {
          if (dic < 0 && counter == 0)
            AddNumericCluster(valuesLong, dic - 1, factor, counter++);
          AddNumericCluster(valuesLong, dic, factor, counter++);
        }
      }

      return BuildValueClustersResult.ListFilled;
    }
    
    private void AddNumericCluster(IEnumerable<long> values, long dic, int factor,
      int counter)
    {
      var minValue = dic * factor;
      var maxValue = (dic + 1) * factor;

      var cluster = new ValueCluster((factor > 1) ? $"{minValue:D} to {maxValue:D}" : $"{dic}",
        string.Format(CultureInfo.InvariantCulture, "({0} >= {1} AND {0} < {2})", m_FilterLogic.DataPropertyNameEscaped, minValue, maxValue),
        counter.ToString("D3"),
        values.Count(value => value >= minValue && value < maxValue), (double) minValue, (double) maxValue);

      if (cluster.Count > 0)
        Add(cluster);
    }

    /// <summary>
    ///   Builds the data grid view column filter m_Values.
    /// </summary>
    /// <param name="dataTable">The data view.</param>
    /// <param name="columnIndex">Index of the column.</param>
    /// <returns></returns>
    private BuildValueClustersResult BuildValueClustersString()
    {
      // Get the distinct m_Values and their counts
      var cluster = new HashSet<string>(StringComparer.OrdinalIgnoreCase);      

      foreach (var ob in m_Values)
      {
        var text = ob.ToString();
        if (text is null || text.Length == 0)
          continue;
        cluster.Add(text);

        // if we have more than the maximum entries stop, no value filter will be used
        if (cluster.Count <= m_MaxNumber)
          continue;
        
        return BuildValueClustersResult.TooManyValues;
      }

      if (cluster.Count == 0)
      {        
        return BuildValueClustersResult.NoValues;
      }

      if (m_HasNull)
        AddValueClusterNull();

      foreach (var text in cluster)
        Add(new ValueCluster(text, $"({m_FilterLogic.DataPropertyNameEscaped} = '{text.SqlQuote()}')", text,
          m_Values.Where(dataRow => dataRow != DBNull.Value)
          .Count(dataRow => string.Equals(dataRow.ToString(), text, StringComparison.OrdinalIgnoreCase)), text, null));

      return BuildValueClustersResult.ListFilled;
    }

    private void AddValueClusterNull()
    {
      if (!m_ValueClusters.Any(x => x.Start is null))
        m_ValueClusters.Add(
        new ValueCluster(ColumnFilterLogic.OperatorIsNull,
          string.Format($"{m_FilterLogic.DataPropertyNameEscaped} IS NULL)"),
          string.Empty,
          m_Values.Count(x => x == DBNull.Value), null, null));
    }

    public void Add(ValueCluster item)
    {
      if (m_FilterLogic.ColumnDataType == typeof(string) || m_FilterLogic.ColumnDataType == typeof(bool) || m_FilterLogic.ColumnDataType == typeof(Guid))
      {
        if (!m_ValueClusters.Any(x => string.Equals(x.Start?.ToString() ?? string.Empty, item.Start?.ToString() ?? string.Empty)))
          m_ValueClusters.Add(item);
      }
      else if (m_FilterLogic.ColumnDataType == typeof(DateTime))
      {
        //                                                                       x  i   x
        // Do not add if there is a cluster existing that spans the new value   [ [ ]  ]
        // Do not add if there is a cluster existing that spans the new value     [ ]  ]
        // Do not add if there is a cluster existing that spans the new value   [ [ ]
        // Do not add if there is a cluster existing that spans the new value     [ ]
        if (!m_ValueClusters.Any(x => (DateTime) x.Start <= (DateTime) item.Start && (DateTime) x.End<= (DateTime) item.End))
          m_ValueClusters.Add(item);
      }
      else if (m_FilterLogic.ColumnDataType == typeof(float) || m_FilterLogic.ColumnDataType == typeof(double) || m_FilterLogic.ColumnDataType == typeof(decimal) || m_FilterLogic.ColumnDataType == typeof(byte) || m_FilterLogic.ColumnDataType == typeof(short) || m_FilterLogic.ColumnDataType == typeof(int) || m_FilterLogic.ColumnDataType == typeof(long))
      {
        if (!m_ValueClusters.Any(x => x.Start != null &&  (double) x.Start <= (double) item.Start && (double) x.End>= (double) item.End))
          m_ValueClusters.Add(item);
      }
      else
        m_ValueClusters.Add(item);
    }


    /// <summary>
    /// Removes all not active Cluster 
    /// </summary>
    public void ClearNotActive()
    {
      var keep = GetActiveValueCluster().ToArray();
      Clear();
      foreach (var item in keep)
        m_ValueClusters.Add(item);
    }

    public void Clear() => m_ValueClusters.Clear();
    public bool Contains(ValueCluster item) => m_ValueClusters.Contains(item);
    public void CopyTo(ValueCluster[] array, int arrayIndex) => m_ValueClusters.CopyTo(array, arrayIndex);
    public bool Remove(ValueCluster item) => m_ValueClusters.Remove(item);
    public IEnumerator<ValueCluster> GetEnumerator() => m_ValueClusters.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => m_ValueClusters.GetEnumerator();
  }
}