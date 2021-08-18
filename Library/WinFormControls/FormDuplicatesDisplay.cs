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


namespace CsvTools
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Data;
  using System.Threading;
  using System.Windows.Forms;

  /// <summary>
  ///   Windows Form UI showing duplicate records
  /// </summary>
  public partial class FormDuplicatesDisplay : ResizeForm
  {
    private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

    private readonly DataRow[] m_DataRow;

    private readonly DataTable m_DataTable;

    private readonly string m_InitialColumn;

    private string m_LastDataColumnName = string.Empty;

    private bool m_LastIgnoreNull = true;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormDuplicatesDisplay" /> class.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="dataRows">The filtered rows.</param>
    /// <param name="initialColumn">The starting column</param>
    /// <param name="hTMLStyle">The HTML style.</param>
    /// <exception cref="ArgumentNullException">hTMLStyle or dataTable or dataRows</exception>
    public FormDuplicatesDisplay(DataTable dataTable, DataRow[] dataRows, string? initialColumn, HTMLStyle hTMLStyle)
    {
      if (hTMLStyle is null)
        throw new ArgumentNullException(nameof(hTMLStyle));
      m_DataTable = dataTable??throw new ArgumentNullException(nameof(dataTable));
      m_DataRow = dataRows??throw new ArgumentNullException(nameof(dataRows));
      m_InitialColumn = initialColumn ?? string.Empty;
      InitializeComponent();
      detailControl.HTMLStyle = hTMLStyle;
    }

    /// <summary>
    ///   Handles the SelectedIndexChanged event of the comboBoxID control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ComboBoxID_SelectedIndexChanged(object? sender, EventArgs e) =>
      Work(comboBoxID.Text, checkBoxIgnoreNull.Checked);

    private void DuplicatesDisplay_FormClosing(object? sender, FormClosingEventArgs e) =>
      m_CancellationTokenSource.Cancel();

    /// <summary>
    ///   Handles the Load event of the HirachyDisplay control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private async void DuplicatesDisplay_LoadAsync(object? sender, EventArgs e)
    {
      var index = 0;
      var current = 0;
      foreach (var columnName in m_DataTable.GetRealColumns())
      {
        if (!string.IsNullOrEmpty(m_InitialColumn)
            && columnName.Equals(m_InitialColumn, StringComparison.OrdinalIgnoreCase))
          index = current;
        comboBoxID.Items.Add(columnName);
        current++;
      }
      comboBoxID.SelectedIndex = index;

      detailControl.CancellationToken = m_CancellationTokenSource.Token;
      detailControl.DataTable = m_DataTable;
      await detailControl.RefreshDisplayAsync(FilterType.All, m_CancellationTokenSource.Token);
    }

    private void Work(string dataColumnName, bool ignoreNull)
    {
      if (string.IsNullOrEmpty(dataColumnName))
        return;
      if (dataColumnName.Equals(m_LastDataColumnName, StringComparison.OrdinalIgnoreCase)
          && m_LastIgnoreNull == ignoreNull)
        return;

      m_LastDataColumnName = dataColumnName;
      m_LastIgnoreNull = ignoreNull;
      this.SafeInvoke(
        () =>
          {
            detailControl.Visible = false;
            detailControl.SuspendLayout();
          });
      try
      {
        var duplicateList = new List<int>();
        var dictIDToRow = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var dictFirstIDStored = new HashSet<int>();
        var dataColumnID = m_DataTable.Columns[dataColumnName];
        if (dataColumnID==null)
          return;
        this.SafeInvoke(() => Text = $@"Duplicate Display - {dataColumnName}");

        var intervalAction = new IntervalAction();
        using var display = new FormProcessDisplay(
          $"Processing {dataColumnName}",
          false,
          m_CancellationTokenSource.Token);
        display.Maximum = m_DataRow.Length;
        display.Show(this);
        for (var rowIndex = 0; rowIndex < m_DataRow.Length; rowIndex++)
        {
          if (display.CancellationToken.IsCancellationRequested)
            return;
          // ReSharper disable once AccessToDisposedClosure
          intervalAction.Invoke(row => display.SetProcess("Getting duplicate values", row, false), rowIndex);

          var id = m_DataRow[rowIndex][dataColumnID.Ordinal].ToString()?.Trim();

          if (ignoreNull && string.IsNullOrEmpty(id))
            continue;
          if (dictIDToRow.TryGetValue(id!, out var duplicateRowIndex))
          {
            if (!dictFirstIDStored.Contains(duplicateRowIndex))
            {
              duplicateList.Add(duplicateRowIndex);
              dictFirstIDStored.Add(duplicateRowIndex);
            }

            duplicateList.Add(rowIndex);
          }
          else
          {
            dictIDToRow.Add(id??string.Empty, rowIndex);
          }
        }

        dictFirstIDStored.Clear();
        dictIDToRow.Clear();

        this.SafeInvoke(
          () => Text = $@"Duplicate Display - {dataColumnName} - Rows {duplicateList.Count} / {m_DataRow.Length}");

        m_DataTable.BeginLoadData();
        m_DataTable.Clear();
        var counter = 0;

        display.Maximum = duplicateList.Count;

        foreach (var rowIndex in duplicateList)
        {
          if (display.CancellationToken.IsCancellationRequested)
            return;
          intervalAction.Invoke(row => display.SetProcess("Importing Rows to Grid", row, false), counter++);
          m_DataTable.ImportRow(m_DataRow[rowIndex]);
        }

        m_DataTable.EndLoadData();
        display.Maximum = 0;
        display.SetProcess("Sorting");

        detailControl.SafeInvoke(
          () =>
          {
            try
            {
              foreach (DataGridViewColumn col in detailControl.FilteredDataGridView.Columns)
                if (col.DataPropertyName == dataColumnName)
                {
                  detailControl.FilteredDataGridView.Sort(col, ListSortDirection.Ascending);
                  break;
                }
            }
            catch (Exception ex)
            {
              Logger.Warning(ex, "Sorting duplicate list {exception}", ex.SourceExceptionMessage());
            }
          });
      }
      finally
      {
        this.SafeInvoke(
          () =>
            {
              detailControl.Visible = true;
              detailControl.ResumeLayout(true);
            });
      }
    }
  }
}