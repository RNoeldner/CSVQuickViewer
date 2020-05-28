﻿/*
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
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace CsvTools
{
  public static class ViewSetting
  {
    private static ToolStripDataGridViewColumnFilter GetFilter(string dataPropertyName,
      IList<ToolStripDataGridViewColumnFilter> columnFilters, DataGridViewColumnCollection columns,
      Func<int, ToolStripDataGridViewColumnFilter> createFilterColumn)
    {
      // look in already existing Filters
      foreach (var columnFilter in columnFilters)
      {
        if (columnFilter == null)
          continue;
        if (dataPropertyName.Equals(columnFilter.ColumnFilterLogic.DataPropertyName,
          StringComparison.OrdinalIgnoreCase))
          return columnFilter;
      }

      for (var columnIndex = 0; columnIndex < columnFilters.Count; columnIndex++)
        if (columnFilters[columnIndex] == null && columns[columnIndex].DataPropertyName
          .Equals(dataPropertyName, StringComparison.OrdinalIgnoreCase))
          return createFilterColumn?.Invoke(columnIndex);

      return null;
    }

    public static bool ReStoreViewSetting(string text, DataGridViewColumnCollection columns,
      IList<ToolStripDataGridViewColumnFilter> columnFilters,
      Func<int, ToolStripDataGridViewColumnFilter> createFilterColumn,
      Action<DataGridViewColumn, ListSortDirection> doSort)
    {
      var vst = JsonConvert.DeserializeObject<List<ColumnSetting>>(text);

      var displayIndex = 0;
      foreach (var storedColumn in vst.OrderBy(x => x.DisplayIndex))
        foreach (DataGridViewColumn col in columns)
          if (col.DataPropertyName.Equals(storedColumn.DataPropertyName, StringComparison.OrdinalIgnoreCase))
            try
            {
              if (col.Visible != storedColumn.Visible)
                col.Visible = storedColumn.Visible;

              if (col.Visible)
              {
                col.Width = storedColumn.Width;
                if (storedColumn.Sort == 1)
                  doSort?.Invoke(col, ListSortDirection.Ascending);
                if (storedColumn.Sort == 2)
                  doSort?.Invoke(col, ListSortDirection.Descending);
              }

              col.DisplayIndex = displayIndex++;
              break;
            }
            catch
            {
              // ignore
            }

      var hasFilterSet = false;
      foreach (var storedFilterSetting in vst)
      {
        ToolStripDataGridViewColumnFilter columnFilter = null;

        if (storedFilterSetting.ValueFilters.Count > 0)
        {
          columnFilter = GetFilter(storedFilterSetting.DataPropertyName, columnFilters, columns, createFilterColumn);
          if (columnFilter == null)
            continue;
          foreach (var valueFilter in storedFilterSetting.ValueFilters)
          {
            var cluster = columnFilter.ValueClusterCollection.ValueClusters.FirstOrDefault(x =>
              valueFilter.SQLCondition.Equals(x.SQLCondition, StringComparison.OrdinalIgnoreCase));
            if (cluster != null)
              cluster.Active = true;
            else
              columnFilter.ValueClusterCollection.ValueClusters.Add(new ValueCluster(valueFilter.Display,
                valueFilter.SQLCondition, string.Empty, 0, true));
          }
        }
        // only restore operator based filter if there is no Value Filter
        else if (!string.IsNullOrEmpty(storedFilterSetting.Operator))
        {
          columnFilter = GetFilter(storedFilterSetting.DataPropertyName, columnFilters, columns, createFilterColumn);
          if (columnFilter == null)
            continue;
          columnFilter.ColumnFilterLogic.ValueText = storedFilterSetting.ValueText;
          columnFilter.ColumnFilterLogic.ValueDateTime = storedFilterSetting.ValueDate;
          columnFilter.ColumnFilterLogic.Operator = storedFilterSetting.Operator;
        }

        if (columnFilter != null)
          columnFilter.ColumnFilterLogic.Active = true;

        hasFilterSet = true;
      }

      return hasFilterSet;
    }

    public static string StoreViewSetting(DataGridViewColumnCollection columns,
      ICollection<ToolStripDataGridViewColumnFilter> columnFilters, DataGridViewColumn sortedColumn,
      SortOrder sortOrder)
    {
      if (columnFilters.Count == 0)
        throw new ArgumentException(@"Value cannot be an empty collection.", nameof(columnFilters));

      var vst = (from DataGridViewColumn col in columns
                 select new ColumnSetting(col.DataPropertyName, col.Visible,
                   ReferenceEquals(col, sortedColumn) ? (int)sortOrder : 0, col.DisplayIndex, col.Width)).ToList();
      var colIndex = 0;
      foreach (var columnFilter in columnFilters)
      {
        if (columnFilter != null && columnFilter.ColumnFilterLogic.Active)
        {
          var hadValueFiler = false;
          foreach (var value in columnFilter.ColumnFilterLogic.ValueClusterCollection.ValueClusters.Where(x =>
            !string.IsNullOrEmpty(x.SQLCondition) && x.Active))
          {
            vst[colIndex].ValueFilters.Add(new ColumnSetting.ValueFilter(value.SQLCondition, value.Display));
            hadValueFiler = true;
          }
          if (!hadValueFiler)
          {
            vst[colIndex].Operator = columnFilter.ColumnFilterLogic.Operator;
            vst[colIndex].ValueText = columnFilter.ColumnFilterLogic.ValueText;
            vst[colIndex].ValueDate = columnFilter.ColumnFilterLogic.ValueDateTime;
          }
        }
        colIndex++;
      }

      return JsonConvert.SerializeObject(vst, Formatting.None);
    }
  }
}