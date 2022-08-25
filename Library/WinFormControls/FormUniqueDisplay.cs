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
#nullable enable

namespace CsvTools
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Data;
  using System.Threading;
  using System.Windows.Forms;

  public partial class FormUniqueDisplay : ResizeForm
  {
    private readonly CancellationTokenSource m_CancellationTokenSource = new();

    private readonly DataRow[] m_DataRow;

    private readonly DataTable m_DataTable;

    private readonly string? m_InitialColumn;

    private string m_LastDataColumnName = string.Empty;

    private bool m_LastIgnoreNull = true;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormDuplicatesDisplay" /> class.
    /// </summary>
    /// <param name="dataTable">The empty data table.</param>
    /// <param name="dataRows">The filtered rows.</param>
    /// <param name="initialColumn">The initial column to use</param>
    /// <param name="hTmlStyle">The h TML style.</param>
    /// <exception cref="ArgumentNullException">hTMLStyle or dataTable or dataRows</exception>
    public FormUniqueDisplay(DataTable dataTable, DataRow[] dataRows, string? initialColumn, HtmlStyle hTmlStyle)
    {
      if (hTmlStyle is null)
        throw new ArgumentNullException(nameof(hTmlStyle));
      m_DataTable = dataTable??throw new ArgumentNullException(nameof(dataTable));
      m_DataRow = dataRows??throw new ArgumentNullException(nameof(dataRows));
      m_InitialColumn = initialColumn;
      InitializeComponent();
      detailControl.HtmlStyle = hTmlStyle;
    }

    /// <summary>
    ///   Handles the SelectedIndexChanged event of the comboBoxID control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ComboBoxID_SelectedIndexChanged(object? sender, EventArgs e) =>
      Work(comboBoxID.Text, checkBoxIgnoreNull.Checked);

    private void UniqueDisplay_FormClosing(object? sender, FormClosingEventArgs e) => m_CancellationTokenSource.Cancel();

    /// <summary>
    ///   Handles the Load event of the FormUniqueDisplay control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private async void FormUniqueDisplay_Load(object? sender, EventArgs e)
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
      await detailControl.RefreshDisplayAsync(FilterTypeEnum.All, m_CancellationTokenSource.Token);
    }

    private void Work(string dataColumnName, bool ignoreNull)
    {
      if (string.IsNullOrEmpty(dataColumnName))
        return;
      if (dataColumnName.Equals(m_LastDataColumnName, StringComparison.Ordinal) && m_LastIgnoreNull == ignoreNull)
        return;

      m_LastDataColumnName = dataColumnName;
      m_LastIgnoreNull = ignoreNull;

      this.SafeInvoke(
        () =>
          {
            detailControl.Visible = false;
            detailControl.SuspendLayout();
          });
      Extensions.ProcessUIElements();
      try
      {
        var dataColumnID = m_DataTable.Columns[dataColumnName];
        if (dataColumnID is null)
          throw new Exception($"Column {dataColumnName} not found");

        this.SafeBeginInvoke(() => Text = $@"Unique Values Display - {dataColumnName} ");

        var dictIDToRow = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        using var formProgress = new FormProgress($"Processing {dataColumnName}", false, m_CancellationTokenSource.Token)
        {
          Maximum = m_DataRow.Length
        };
        formProgress.Show(this);
        Logger.Information("Getting Unique values");
        var intervalAction = new IntervalAction();
        for (var rowIndex = 0; rowIndex < m_DataRow.Length; rowIndex++)
        {
          if (formProgress.CancellationToken.IsCancellationRequested)
            return;
          intervalAction.Invoke(formProgress, "Getting Unique values", rowIndex);
          if (ignoreNull && m_DataRow[rowIndex].IsNull(dataColumnID.Ordinal)) 
            continue;
          var id = m_DataRow[rowIndex][dataColumnID.Ordinal].ToString().Trim();
          if (!dictIDToRow.ContainsKey(id))
            dictIDToRow.Add(id, rowIndex);
        }

        this.SafeInvoke(
          () => Text = $@"Unique Values Display - {dataColumnName} - Rows {dictIDToRow.Count}/{m_DataRow.Length}");

        m_DataTable.BeginLoadData();
        m_DataTable.Clear();
        formProgress.Maximum = dictIDToRow.Count;

        var counter = 0;
        foreach (var rowIndex in dictIDToRow.Values)
        {
          if (formProgress.CancellationToken.IsCancellationRequested)
            return;
          counter++;
          intervalAction.Invoke(formProgress, "Importing Rows to Grid", counter);
          m_DataTable.ImportRow(m_DataRow[rowIndex]);
        }

        m_DataTable.EndLoadData();
        detailControl.CancellationToken = m_CancellationTokenSource.Token;
        formProgress.Maximum = 0;
        formProgress.SetProcess("Sorting");
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
              Logger.Warning(ex, "Processing Unique Sorting {exception}", ex.InnerExceptionMessages());
            }
          });
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
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