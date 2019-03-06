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
  public partial class FormUniqueDisplay : Form
  {
    private readonly CancellationTokenSource m_CancellationTokenSource =
      new CancellationTokenSource();

    private readonly DataRow[] m_DataRow;
    private readonly DataTable m_DataTable;
    private readonly string m_InitialColumn;
    private string m_LastDataColumnName = string.Empty;
    private bool m_LastIgnoreNull = true;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormDuplicatesDisplay" /> class.
    /// </summary>
    /// <param name="dataTable">The empty data table.</param>
    /// <param name="dataRows">The filtered rows.</param>
    /// <param name="initialColumn">The initial column to use</param>
    public FormUniqueDisplay(DataTable dataTable, DataRow[] dataRows, string initialColumn)
    {
      Contract.Requires(dataTable != null);
      Contract.Requires(dataRows != null);
      m_DataTable = dataTable;
      m_DataRow = dataRows;
      InitializeComponent();
      Icon = Resources.SubFormIcon;
      detailControl.DataTable = dataTable;
      m_InitialColumn = initialColumn;
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

    private void UniqueDisplay_FormClosing(object sender, FormClosingEventArgs e)
    {
      m_CancellationTokenSource.Cancel();
    }

    /// <summary>
    ///   Handles the Load event of the HirachyDisplay control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void UniqueDisplay_Load(object sender, EventArgs e)
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
      if (dataColumnName.Equals(m_LastDataColumnName, StringComparison.Ordinal) && m_LastIgnoreNull == ignoreNull)
        return;

      m_LastDataColumnName = dataColumnName;
      m_LastIgnoreNull = ignoreNull;

      this.SafeInvoke(() =>
      {
        detailControl.Visible = false;
        detailControl.SuspendLayout();
      });
      Extensions.ProcessUIElements();
      try
      {
        this.SafeBeginInvoke(() =>
          Text = $"Unique Values Display - {dataColumnName} ");

        var dataColumnID = m_DataTable.Columns[dataColumnName];
        var dictIDToRow = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var intervalAction = new IntervalAction();

        using (var display = new FormProcessDisplay($"Processing {dataColumnName}", m_CancellationTokenSource.Token))
        {
          display.Maximum = m_DataRow.Length;
          display.Show(this);

          for (var rowIdex = 0; rowIdex < m_DataRow.Length; rowIdex++)
          {
            if (display.CancellationToken.IsCancellationRequested)
              return;
            intervalAction.Invoke(delegate
            {
              display.SetProcess("Getting Unique values", rowIdex);
            });
            var id = m_DataRow[rowIdex][dataColumnID.Ordinal].ToString().Trim();
            if (ignoreNull && string.IsNullOrEmpty(id)) continue;
            if (!dictIDToRow.ContainsKey(id))
              dictIDToRow.Add(id, rowIdex);
          }

          this.SafeInvoke(() => Text =
            $"Unique Values Display - {dataColumnName} - Rows {dictIDToRow.Count}/{m_DataRow.Length}");

          m_DataTable.BeginLoadData();
          m_DataTable.Clear();
          display.Maximum = dictIDToRow.Count;

          var counter = 0;
          foreach (var rowIdex in dictIDToRow.Values)
          {
            if (display.CancellationToken.IsCancellationRequested)
              return;
            counter++;
            if (counter % 100 == 0)
              intervalAction.Invoke(delegate
              {
                display.SetProcess("Importing Rows to Grid", counter);
              });
            m_DataTable.ImportRow(m_DataRow[rowIdex]);
          }

          m_DataTable.EndLoadData();
          detailControl.CancellationToken = m_CancellationTokenSource.Token;
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
      catch (Exception ex)
      {
        this.ShowError(ex);
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