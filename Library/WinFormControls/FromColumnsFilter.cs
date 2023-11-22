/*
 * Copyright (C) 2014 Raphael Nöldner : http://CSVReshaper.com
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

namespace CsvTools
{
  using System;
  using System.Collections.Generic;
  using System.Data;
  using System.Linq;
  using System.Windows.Forms;

  /// <summary>
  ///   Control to allow entering filters
  /// </summary>
  public sealed partial class FromColumnsFilter : ResizeForm
  {
    private readonly List<DataGridViewColumn> Columns = new List<DataGridViewColumn>();
    private readonly List<int> Filtered = new List<int>();
    private readonly List<DataRow> Rows = new List<DataRow>();

    /// <summary>
    ///   Initializes a new instance of the <see cref="FromColumnFilter" /> class.
    /// </summary>
    /// <param name="dataGridViewColumnFilter">The data grid view column.</param>
    /// <param name="columnValues">The data in teh column</param>
    public FromColumnsFilter(in DataGridViewColumnCollection columns, in IEnumerable<DataRow> dataRows, in IEnumerable<int> withFilter)
    {
      Columns.AddRange(columns.OfType<DataGridViewColumn>());
      Rows.AddRange(dataRows);
      Filtered.AddRange(withFilter);
      InitializeComponent();

      Filter(string.Empty);
    }


    private void Filter(string filter)
    {
      var filtered = Columns.Where(x => string.IsNullOrEmpty(filter) || x.Name.Contains(filter)).OrderBy(x => x.DisplayIndex).ToArray();
      listViewCluster.BeginUpdate();
      listViewCluster.Items.Clear();
      foreach (var item in filtered)
      {
        var lv_item = listViewCluster.Items.Add(new ListViewItem(item.DataPropertyName));
        lv_item.Checked = item.Visible;
        if (Filtered.Contains(item.Index))
          lv_item.ForeColor = System.Drawing.SystemColors.MenuHighlight;
      }
      foreach (var item in Columns.Where(x => !filtered.Contains(x)).OrderBy(x => x.DisplayIndex))
      {
        var lv_item = listViewCluster.Items.Add(new ListViewItem(item.Name));
        if (Filtered.Contains(item.Index))
          lv_item.ForeColor = System.Drawing.SystemColors.MenuHighlight;
        else
          lv_item.ForeColor = System.Drawing.SystemColors.GrayText;
        lv_item.Checked = item.Visible;
      }
      listViewCluster.EndUpdate();
    }

    private void TextBoxValue_TextChanged(object sender, EventArgs e)
    {
      timerFilter.Stop();
      timerFilter.Start();
    }

    private void TimerFilter_Tick(object sender, EventArgs e)
    {
      timerFilter.Stop();
      try
      {
        // Filter The check boxes
        Filter(textBoxFilter.Text);
      }
      catch (Exception ex)
      {
        ParentForm.ShowError(ex);
      }
    }

    private void ButtonCheck_Click(object sender, EventArgs e)
    {
      buttonCheck.RunWithHourglass(() =>
      {
        foreach (ListViewItem col in listViewCluster.Items)
          if (col.ForeColor != System.Drawing.SystemColors.GrayText)
            col.Checked = true;
      });
    }

    private void ButtonEmpty_Click(object sender, EventArgs e)
    {
      buttonEmpty.RunWithHourglass(() =>
      {
        foreach (ListViewItem col in listViewCluster.Items)
        {
          if (col.Checked)
          {
            var colName = col.SubItems[0].ToString();
            col.Checked = Rows.Any(dataRow => dataRow[colName] != DBNull.Value);
          }
        }
      });
    }

    private void ButtonApply_Click(object sender, EventArgs e)
    {
      buttonApply.RunWithHourglass(() =>
      {
        // Apply the things
        foreach (var col in Columns)
        {
          var lvi = listViewCluster.Items.OfType<ListViewItem>().First(x => x.Text == col.Name);
          col.Visible = lvi.Checked;
        }
      });
    }

    private void ButtonUncheck_Click(object sender, EventArgs e)
    {
      buttonUncheck.RunWithHourglass(() =>
     {
       foreach (ListViewItem col in listViewCluster.Items)
         if (col.ForeColor != System.Drawing.SystemColors.GrayText)
           col.Checked = false;
     });
    }
  }
}