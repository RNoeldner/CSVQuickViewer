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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CsvTools
{
  public static class ViewSetting
  {
    private static ToolStripDataGridViewColumnFilter GetFilter(string colName,
      IList<ToolStripDataGridViewColumnFilter> columnFilters, DataGridViewColumnCollection columns,
      Func<int, ToolStripDataGridViewColumnFilter> createFilterColumn)
    {
      var cleanName = colName.StartsWith("[", StringComparison.Ordinal) &&
                      colName.EndsWith("]", StringComparison.Ordinal)
        ? colName.Substring(1, colName.Length - 2).Replace(@"\]", "]").Replace(@"\\", @"\")
        : colName;

      var sqlName = $"[{cleanName.SqlName()}]";

      // look in already existing Filters
      foreach (var columnFilter in columnFilters)
      {
        if (columnFilter == null)
          continue;
        if (sqlName.Equals(columnFilter.ColumnFilterLogic.DataPropertyName, StringComparison.OrdinalIgnoreCase))
          return columnFilter;
      }


      for (var columnIndex = 0; columnIndex < columnFilters.Count; columnIndex++)
        if (columnFilters[columnIndex] == null && columns[columnIndex].DataPropertyName
          .Equals(cleanName, StringComparison.OrdinalIgnoreCase))
          return createFilterColumn?.Invoke(columnIndex);
      return null;
    }

    public static bool ReStoreViewSetting(string text, DataGridViewColumnCollection columns,
      IList<ToolStripDataGridViewColumnFilter> columnFilters,
      Func<int, ToolStripDataGridViewColumnFilter> createFilterColumn)
    {
      using (var reader = new StringReader(text))
      {
        var colIndex = 0;
        var showColumns = SplitQuoted(reader.ReadLine());
        foreach (var colName in showColumns)
          foreach (DataGridViewColumn col in columns)
            if (col.Name.Equals(colName, StringComparison.OrdinalIgnoreCase))
            {
              if (!col.Visible)
                col.Visible = true;
              col.DisplayIndex = colIndex++;
              break;
            }

        var hideColumns = SplitQuoted(reader.ReadLine());
        foreach (var colName in hideColumns)
          foreach (DataGridViewColumn col in columns)
            if (col.Name.Equals(colName, StringComparison.OrdinalIgnoreCase))
            {
              if (col.Visible)
                col.Visible = false;
              col.DisplayIndex = colIndex++;
              break;
            }

        var aLine = reader.ReadLine();
        var hasFilterSet = false;
        while (aLine != null)
        {
          var filterLineColumns = SplitQuoted(aLine);
          var columnFilter = GetFilter(filterLineColumns[0], columnFilters, columns, createFilterColumn);
          if (columnFilter == null)
            continue;
          if (filterLineColumns.Count == 4)
          {
            columnFilter.ColumnFilterLogic.ValueText = filterLineColumns[2];
            if (DateTime.TryParseExact(filterLineColumns[3], "yyyyMMdd", CultureInfo.InvariantCulture,
              DateTimeStyles.AssumeLocal, out var dt))
              columnFilter.ColumnFilterLogic.ValueDateTime = dt;
            columnFilter.ColumnFilterLogic.Operator = filterLineColumns[1];
          }
          else
          {
            foreach (var condition in filterLineColumns[1].Split('|'))
            {
              var cluster = columnFilter.ValueClusterCollection.ValueClusters.FirstOrDefault(x =>
                condition.Equals(x.SQLCondition, StringComparison.OrdinalIgnoreCase));
              if (cluster != null)
              {
                cluster.Active = true;
              }
              else
              {
                var ind = condition.IndexOf(" = ", StringComparison.Ordinal);
                var display = ind == -1 ? condition : condition.Substring(ind + 4, condition.Length - (ind + 5));
                columnFilter.ValueClusterCollection.ValueClusters.Add(new ValueCluster(display, condition, display, 0, true));
              }
            }
          }

          columnFilter.ColumnFilterLogic.Active = true;
          hasFilterSet = true;
          aLine = reader.ReadLine();
        }

        return hasFilterSet;
      }
    }

    public static string StoreViewSetting(DataGridViewColumnCollection columns,
      IEnumerable<ToolStripDataGridViewColumnFilter> columnFilters)
    {
      var columnsInOrder = new SortedDictionary<int, DataGridViewColumn>();
      foreach (DataGridViewColumn col in columns)
        columnsInOrder.Add(col.DisplayIndex, col);

      var sb = new StringBuilder();
      var values = false;
      foreach (var col in columnsInOrder.Values)
        if (col.Visible)
        {
          sb.Append($"\"{col.Name.Replace("\"", "\"\"")}\",");
          values = true;
        }

      if (values)
        sb.Length--;

      sb.AppendLine();

      values = false;
      foreach (var col in columnsInOrder.Values)
        if (!col.Visible)
        {
          sb.Append($"\"{col.Name.Replace("\"", "\"\"")}\",");
          values = true;
        }

      if (values)
        sb.Length--;
      sb.AppendLine();

      foreach (var columnFilter in columnFilters)
      {
        if (columnFilter == null)
          continue;
        var filterLogic = columnFilter.ColumnFilterLogic;
        if (!filterLogic.Active)
          continue;
        var singleValues = filterLogic.ValueClusterCollection.ValueClusters
          .Where(x => !string.IsNullOrEmpty(x.SQLCondition) && x.Active).Select(x => x.SQLCondition).Join("|");
        sb.AppendLine(
          string.IsNullOrEmpty(singleValues)
            ? $"\"{filterLogic.DataPropertyName.Replace("\"", "\"\"")}\",\"{filterLogic.Operator.Replace("\"", "\"\"")}\",\"{filterLogic.ValueText.Replace("\"", "\"\"")}\",\"{filterLogic.ValueDateTime:yyyyMMdd}\""
            : $"\"{filterLogic.DataPropertyName.Replace("\"", "\"\"")}\",\"{singleValues.Replace("\"", "\"\"")}\"");
      }

      return sb.ToString();
    }

    private static List<string> SplitQuoted(string line)
    {
      var res = new List<string>();
      while (line.Length > 0)
      {
        var nextEnd = line.IndexOf("\",", StringComparison.Ordinal);
        if (nextEnd == -1)
          nextEnd = line.Length - 1;
        var part = line.Substring(1, nextEnd - 1);
        res.Add(part.Replace("\"\"", "\""));
        line = line.Substring(nextEnd + 1);
        while (line.Length > 0 && line[0] != '"')
          line = line.Substring(1);
      }

      return res;
    }
  }
}