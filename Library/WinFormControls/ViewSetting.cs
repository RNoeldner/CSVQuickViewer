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
    public static ToolStripDataGridViewColumnFilter GetFilter(string colName, IList<ToolStripDataGridViewColumnFilter> columnFilters, Func<int, ToolStripDataGridViewColumnFilter> createFilterColumn)
    {
      // look in altready existing Filters
      foreach (var columnFilter in columnFilters)
      {
        if (columnFilter == null)
          continue;
        if (colName.Equals(columnFilter.ColumnFilterLogic.DataPropertyName, StringComparison.OrdinalIgnoreCase))
          return columnFilter;
      }

      for (int columnIndex = 0; columnIndex < columnFilters.Count; columnIndex++)
      {
        if (columnFilters[columnIndex] == null)
        {
          var columnFilter = createFilterColumn?.Invoke(columnIndex);
          if (colName.Equals(columnFilter.ColumnFilterLogic.DataPropertyName, StringComparison.OrdinalIgnoreCase))
            return columnFilter;
        }
      }
      return null;
    }

    public static bool ReStoreViewSetting(string text, DataGridViewColumnCollection columns, IList<ToolStripDataGridViewColumnFilter> columnFilters, Func<int, ToolStripDataGridViewColumnFilter> createFilterColumn)
    {
      using (var reader = new StringReader(text))
      {
        int colIndex = 0;
        var showColumns = SplitQuoted(reader.ReadLine());
        foreach (string colName in showColumns)
        {
          foreach (DataGridViewColumn col in columns)
          {
            if (col.Name.Equals(colName, StringComparison.OrdinalIgnoreCase))
            {
              if (!col.Visible)
                col.Visible = true;
              col.DisplayIndex = colIndex++;
              break;
            }
          }
        }

        var hideColumns = SplitQuoted(reader.ReadLine());
        foreach (string colName in hideColumns)
        {
          foreach (DataGridViewColumn col in columns)
          {
            if (col.Name.Equals(colName, StringComparison.OrdinalIgnoreCase))
            {
              if (col.Visible)
                col.Visible = false;
              col.DisplayIndex = colIndex++;
              break;
            }
          }
        }
        var aLine = reader.ReadLine();
        bool hasFilterSet = false;
        while (aLine != null)
        {
          var filterLineColumns = SplitQuoted(aLine);
          var columnFilter = GetFilter(filterLineColumns[0], columnFilters, createFilterColumn);
          if (columnFilter == null)
            continue;
          if (filterLineColumns.Count == 4)
          {
            columnFilter.ColumnFilterLogic.ValueText = filterLineColumns[2];
            if (DateTime.TryParseExact(filterLineColumns[3], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
              columnFilter.ColumnFilterLogic.ValueDateTime = dt;
            columnFilter.ColumnFilterLogic.Operator = filterLineColumns[1];
          }
          else
          {
            foreach (var condition in filterLineColumns[1].Split('|'))
            {
              var cluster = columnFilter.ValueClusterCollection.ValueClusters.FirstOrDefault(x => condition.Equals(x.SQLCondition, StringComparison.OrdinalIgnoreCase));
              if (cluster != null)
                cluster.Active = true;
              else
              {
                var ind = condition.IndexOf(" = ");
                var display = ind == -1 ? condition : condition.Substring(ind + 4, condition.Length - (ind + 5));
                columnFilter.ValueClusterCollection.ValueClusters.Add(
                  new ValueCluster() { SQLCondition = condition, Display = display, Active = true });
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

    public static string StoreViewSetting(DataGridViewColumnCollection columns, IEnumerable<ToolStripDataGridViewColumnFilter> columnFilters)
    {
      var columnsInOrder = new SortedDictionary<int, DataGridViewColumn>();
      foreach (DataGridViewColumn col in columns)
        columnsInOrder.Add(col.DisplayIndex, col);

      var sb = new StringBuilder();
      bool values = false;
      foreach (var col in columnsInOrder.Values)
      {
        if (col.Visible)
        {
          sb.Append($"\"{col.Name.Replace("\"", "\"\"")}\",");
          values = true;
        }
      }
      if (values)
        sb.Length--;

      sb.AppendLine();

      values = false;
      foreach (var col in columnsInOrder.Values)
      {
        if (!col.Visible)
        {
          sb.Append($"\"{col.Name.Replace("\"", "\"\"")}\",");
          values = true;
        }
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
        var singleValues = filterLogic.ValueClusterCollection.ValueClusters.Where(x => !string.IsNullOrEmpty(x.SQLCondition) && x.Active).Select(x => x.SQLCondition).Join("|");
        if (string.IsNullOrEmpty(singleValues))
          sb.AppendLine($"\"{filterLogic.DataPropertyName.Replace("\"", "\"\"")}\",\"{filterLogic.Operator.Replace("\"", "\"\"")}\",\"{filterLogic.ValueText.Replace("\"", "\"\"")}\",\"{filterLogic.ValueDateTime.ToString("yyyyMMdd")}\"");
        else
          sb.AppendLine($"\"{filterLogic.DataPropertyName.Replace("\"", "\"\"")}\",\"{singleValues.Replace("\"", "\"\"")}\"");
      }
      return sb.ToString();
    }

    private static List<string> SplitQuoted(string line)
    {
      var res = new List<string>();
      while (line.Length > 0)
      {
        var nextEnd = line.IndexOf("\",");
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