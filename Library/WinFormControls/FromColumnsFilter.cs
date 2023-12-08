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
    public FromColumnsFilter(in DataGridViewColumnCollection columns, in IEnumerable<DataRow> dataRows, in IEnumerable<int> withFilter, bool allDataPresent = true)
    {
      Columns.AddRange(columns.OfType<DataGridViewColumn>());
      Rows.AddRange(dataRows);
      m_Checked.AddRange(Columns.Where(x => x.Visible).Select(x => x.DataPropertyName));
      Filtered.AddRange(withFilter);
      InitializeComponent();
      buttonEmpty.Enabled = allDataPresent;
      Filter(string.Empty);
    }

    private readonly List<string> m_Checked = new List<string>();

    private void Filter(string filter)
    {
      var filtered = Columns.Where(x => string.IsNullOrEmpty(filter) || x.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1).OrderBy(x => x.DisplayIndex).ToArray();
      listViewCluster.BeginUpdate();
      listViewCluster.Items.Clear();
      foreach (var item in filtered)
      {
        var lv_item = listViewCluster.Items.Add(new ListViewItem(item.DataPropertyName));
        lv_item.Checked = m_Checked.Contains(item.DataPropertyName);
        if (Filtered.Contains(item.Index))
          lv_item.ForeColor = System.Drawing.SystemColors.MenuHighlight;
        if (item.Frozen)
          lv_item.ForeColor = System.Drawing.SystemColors.Highlight;
      }
      foreach (var item in Columns.Where(x => !filtered.Contains(x)).OrderBy(x => x.DisplayIndex))
      {
        var lv_item = listViewCluster.Items.Add(new ListViewItem(item.Name));
        if (Filtered.Contains(item.Index))
          lv_item.ForeColor = System.Drawing.SystemColors.MenuHighlight;
        else
          lv_item.ForeColor = System.Drawing.SystemColors.GrayText;
        
        if (item.Frozen)
          lv_item.ForeColor = System.Drawing.SystemColors.Highlight;

        lv_item.Checked = m_Checked.Contains(item.DataPropertyName);
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
      textBoxFilter.RunWithHourglass(() =>
      {
        m_Checked.Clear();
        m_Checked.AddRange(listViewCluster.CheckedItems.OfType<ListViewItem>().Select(x => x.Text));

        // Filter The check boxes
        Filter(textBoxFilter.Text);
      });
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
            var colName = col.SubItems[0].Text ?? col.Text;
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
        // if nothing is visible any more unhide first column
        if (!Columns.Any(x=>x.Visible))
          Columns.First().Visible = true;
      });
    }

    private void ButtonUncheck_Click(object sender, EventArgs e)
    {
      buttonUncheck.RunWithHourglass(() =>
     {
       foreach (ListViewItem col in listViewCluster.Items)
         if (col.ForeColor != System.Drawing.SystemColors.GrayText)
         {
           // Do not hide frozen columns
           if (!Columns.Any(x => x.DataPropertyName == col.Text && x.Frozen))
             col.Checked = false;
         }
     });
    }
  }
}