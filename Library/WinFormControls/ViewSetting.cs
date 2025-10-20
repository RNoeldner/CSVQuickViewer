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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace CsvTools
{
  public static class ViewSetting
  {
    public static void ReStoreViewSetting(string text, DataGridViewColumnCollection columns,
      IDictionary<int, ColumnFilterLogic> columnFilters, Action<DataGridViewColumn, ListSortDirection>? doSort)
    {
      try
      {
        var vst = JsonConvert.DeserializeObject<List<ColumnSetting>>(text) ?? throw new InvalidOperationException();
        var displayIndex = 0;
        foreach (var storedColumn in (vst).OrderBy(x => x.DisplayIndex))
        {
          var col = columns.OfType<DataGridViewColumn>().FirstOrDefault(x =>
            x.DataPropertyName.Equals(storedColumn.DataPropertyName, StringComparison.OrdinalIgnoreCase));
          if (col == null)
            continue;
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

            var newColumnFilterLogic = new ColumnFilterLogic(col.ValueType, col.DataPropertyName);

            foreach (var cluster in storedColumn.ValueFilters)
              columnFilters[col.Index].ValueClusterCollection
                .Add(new ValueCluster(cluster.Display, cluster.SQLCondition, 0, null, null, true));
            if (storedColumn.ValueFilters.Count == 0)
            {
              columnFilters[col.Index].Operator = storedColumn.Operator;
              columnFilters[col.Index].ValueText = storedColumn.ValueText;
              columnFilters[col.Index].ValueDateTime = storedColumn.ValueDate;
            }

            columnFilters[col.Index] = newColumnFilterLogic;

            break;
          }
          catch (Exception ex)
          {
            try { Logger.Information(ex, "ReStoreViewSetting {text} {col}", text, col); } catch { }

          }
        }
      }
      catch (Exception ex)
      {
        try { Logger.Warning(ex, "Restoring View Setting"); } catch { }
      }
    }

    public static ICollection<ColumnSetting>? GetViewSetting(DataGridViewColumnCollection columns,
      IEnumerable<ColumnFilterLogic> columnFilters, DataGridViewColumn? sortedColumn, SortOrder sortOrder)
    {
      try
      {
        var vst = columns.OfType<DataGridViewColumn>().Select(col => new ColumnSetting(col.DataPropertyName,
          col.Visible, col == sortedColumn ? (int) sortOrder : 0, col.DisplayIndex, col.Width)).ToList();
        var colIndex = 0;
        foreach (var columnFilter in columnFilters)
        {
          if (columnFilter is null)
            continue;
          if (columnFilter.Active)
          {
            var hadValueFiler = false;
            foreach (var value in columnFilter.ValueClusterCollection.Where(x =>
                       !string.IsNullOrEmpty(x.SQLCondition) && x.Active))
            {
              vst[colIndex].ValueFilters.Add(new ColumnSetting.ValueFilter(value.SQLCondition, value.Display));
              hadValueFiler = true;
            }

            if (!hadValueFiler)
            {
              vst[colIndex].Operator = columnFilter.Operator;
              vst[colIndex].ValueText = columnFilter.ValueText;
              vst[colIndex].ValueDate = columnFilter.ValueDateTime;
            }
          }

          colIndex++;
        }

        return vst;
      }
      catch (Exception ex)
      {
        try { Logger.Error(ex, "GetViewSetting");} catch { }
        return null;
      }
    }

    /// <summary>
    /// Get an Array of ColumnSetting serialized as Json Text
    /// </summary>
    public static string StoreViewSetting(DataGridViewColumnCollection columns,
      IEnumerable<ColumnFilterLogic> columnFilters, DataGridViewColumn? sortedColumn,
      SortOrder sortOrder)
    {
      var res = GetViewSetting(columns, columnFilters, sortedColumn, sortOrder);
      if (res is null)
        return string.Empty;
      return JsonConvert.SerializeObject(res, Formatting.None);
    }
  }
}
