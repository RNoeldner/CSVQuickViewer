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
    private readonly HashSet<string> m_Protected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> m_Checked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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

      foreach (var col in Columns)
      {
        if (col.Visible)
          m_Checked.Add(col.DataPropertyName);
        if (col.Frozen || withFilter.Contains(col.Index))
          m_Protected.Add(col.DataPropertyName);
      }

      InitializeComponent();
      buttonEmpty.Enabled = allDataPresent;
      Filter(string.Empty);
    }



    private void AddItem(DataGridViewColumn item, bool filtered)
    {
      var lv_item = listViewCluster.Items.Add(new ListViewItem(item.Name));
      if (m_Protected.Contains(item.DataPropertyName))
        lv_item.ForeColor = System.Drawing.SystemColors.Highlight;
      else if (!filtered)
        lv_item.ForeColor = System.Drawing.SystemColors.GrayText;

      lv_item.Checked = m_Checked.Contains(item.DataPropertyName);
    }

    private void Filter(string filter)
    {
      listViewCluster.BeginUpdate();
      listViewCluster.Items.Clear();

      var filtered = Columns.Where(x => string.IsNullOrEmpty(filter) || x.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1).ToArray();
      foreach (var item in filtered.OrderBy(x => x.DisplayIndex))
        AddItem(item, true);
      foreach (var item in Columns.Where(x => !filtered.Contains(x)).OrderBy(x => x.DisplayIndex))
        AddItem(item, false);

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
      this.RunWithHourglass(() =>
      {
        m_Checked.Clear();
        foreach (var col in listViewCluster.CheckedItems.OfType<ListViewItem>().Select(x => x.Text))
          m_Checked.Add(col);
        foreach (var col in m_Protected)
          m_Checked.Add(col);

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
          if (m_Protected.Contains(col.DataPropertyName))
            col.Visible = true;
          else
            col.Visible = listViewCluster.Items.OfType<ListViewItem>().First(x => x.Text == col.Name).Checked;
        }
        // if nothing is visible any more unhide first column
        if (Columns.Count>0 && !Columns.Any(x => x.Visible))
          Columns.First().Visible = true;
      });
    }

    private void ButtonUncheck_Click(object sender, EventArgs e)
    {
      buttonUncheck.RunWithHourglass(() =>
     {
       foreach (ListViewItem col in listViewCluster.Items)
         if (col.ForeColor != System.Drawing.SystemColors.GrayText)
           col.Checked = m_Protected.Contains(col.Text);
     });
    }
  }
}