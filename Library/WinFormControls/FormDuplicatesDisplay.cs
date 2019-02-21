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

using CsvToolLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Windows Form UI showing duplicate records
  /// </summary>
  public partial class FormDuplicatesDisplay : Form
  {
    private readonly DataRow[] m_DataRow;
    private readonly DataTable m_DataTable;
    private readonly string m_InitialColumn;

    private string m_LastDataColumnName = string.Empty;
    private bool m_LastIgnoreNull = true;

    private readonly CancellationTokenSource m_CancellationTokenSource =
      new CancellationTokenSource();

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormDuplicatesDisplay" /> class.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="dataRows">The filtered rows.</param>
    /// <param name="initialColumn">The starting column</param>
    public FormDuplicatesDisplay(DataTable dataTable, DataRow[] dataRows, string initialColumn)
    {
      Contract.Requires(dataTable != null);
      Contract.Requires(dataRows != null);
      m_DataTable = dataTable;
      m_DataRow = dataRows;
      m_InitialColumn = initialColumn;
      InitializeComponent();
      Icon = Resources.SubFormIcon;
      detailControl.CancellationToken = m_CancellationTokenSource.Token;
      detailControl.DataTable = m_DataTable;
    }

    /// <summary>
    ///   Handles the SelectedIndexChanged event of the comboBoxID control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void comboBoxID_SelectedIndexChanged(object sender, EventArgs e)
    {
      Work(comboBoxID.Text, checkBoxIgnoreNull.Checked);
    }

    private void DuplicatesDisplay_FormClosing(object sender, FormClosingEventArgs e)
    {
      m_CancellationTokenSource.Cancel();
    }

    /// <summary>
    ///   Handles the Load event of the HirachyDisplay control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void DuplicatesDisplay_Load(object sender, EventArgs e)
    {
      var index = 0;
      var current = 0;
      foreach (var columnName in m_DataTable.GetRealColumns())
      {
        if (!string.IsNullOrEmpty(m_InitialColumn) &&
            columnName.Equals(m_InitialColumn, StringComparison.OrdinalIgnoreCase))
          index = current;
        comboBoxID.Items.Add(columnName);
        current++;
      }

      comboBoxID.SelectedIndex = index;
    }

    private void Work(string dataColumnName, bool ignoreNull)
    {
      if (string.IsNullOrEmpty(dataColumnName))
        return;
      if (dataColumnName.Equals(m_LastDataColumnName, StringComparison.OrdinalIgnoreCase) && m_LastIgnoreNull == ignoreNull)
        return;

      m_LastDataColumnName = dataColumnName;
      m_LastIgnoreNull = ignoreNull;
      this.SafeInvoke(() =>
      {
        detailControl.Visible = false;
        detailControl.SuspendLayout();
      });
      try
      {
        var dupliacteList = new List<int>();
        var dictIDToRow = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var dictFirstIDStored = new HashSet<int>();
        var dataColumnID = m_DataTable.Columns[dataColumnName];
        this.SafeInvoke(() =>
          Text = $"Duplicate Display - {dataColumnName}");

        var intervalAction = new IntervalAction();
        using (var display = new FormProcessDisplay($"Processing {dataColumnName}", m_CancellationTokenSource.Token))
        {
          display.Maximum = m_DataRow.Length;
          for (var rowIdex = 0; rowIdex < m_DataRow.Length; rowIdex++)
          {
            if (display.CancellationToken.IsCancellationRequested)
              return;
            intervalAction.Invoke(delegate
            {
              display.SetProcess("Getting duplicate values", rowIdex);
            });

            var id = m_DataRow[rowIdex][dataColumnID.Ordinal].ToString().Trim();
            //if (id != null)
            //  id = id.Trim();
            if (ignoreNull && string.IsNullOrEmpty(id))
              continue;
            if (dictIDToRow.TryGetValue(id, out var dupliacteRowIndex))
            {
              if (!dictFirstIDStored.Contains(dupliacteRowIndex))
              {
                dupliacteList.Add(dupliacteRowIndex);
                dictFirstIDStored.Add(dupliacteRowIndex);
              }

              dupliacteList.Add(rowIdex);
            }
            else
            {
              dictIDToRow.Add(id, rowIdex);
            }
          }

          dictFirstIDStored.Clear();
          dictIDToRow.Clear();

          this.SafeInvoke(() => Text =
            $"Duplicate Display - {dataColumnName} - Rows {dupliacteList.Count} / {m_DataRow.Length}");

          m_DataTable.BeginLoadData();
          m_DataTable.Clear();
          var counter = 0;

          display.Maximum = dupliacteList.Count;
          display.Show(this);
          foreach (var rowIdex in dupliacteList)
          {
            if (display.CancellationToken.IsCancellationRequested)
              return;
            counter++;
            intervalAction.Invoke(delegate
            {
              display.SetProcess("Importing Rows to Grid", counter);
            });
            m_DataTable.ImportRow(m_DataRow[rowIdex]);
          }

          m_DataTable.EndLoadData();
          display.Maximum = 0;
          display.SetProcess("Sorting");

          detailControl.SafeInvoke(() =>
          {
            try
            {
              foreach (DataGridViewColumn col in detailControl.DataGridView.Columns)
                if (col.DataPropertyName == dataColumnName)
                {
                  detailControl.DataGridView.Sort(col, ListSortDirection.Ascending);
                  break;
                }
            }
            catch (Exception ex)
            {
              Debug.WriteLine(ex.InnerExceptionMessages());
            }
          });
        }
      }
      finally
      {
        this.SafeInvoke(() =>
        {
          detailControl.Visible = true;
          detailControl.ResumeLayout(true);
        });
      }
    }
  }
}