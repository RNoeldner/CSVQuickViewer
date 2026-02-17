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
// ReSharper disable once RedundantNullableDirective

#nullable enable

namespace CsvTools;

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Windows.Forms;

public partial class FormUniqueDisplay : ResizeForm
{
  private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

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
  /// <param name="htmlStyle">The h TML style.</param>
  /// <exception cref="ArgumentNullException">hTMLStyle or dataTable or dataRows</exception>
  public FormUniqueDisplay(in DataTable dataTable, in DataRow[] dataRows, string? initialColumn,
    in HtmlStyle htmlStyle)
  {
    if (htmlStyle is null)
      throw new ArgumentNullException(nameof(htmlStyle));
    m_DataTable = dataTable ?? throw new ArgumentNullException(nameof(dataTable));
    m_DataRow = dataRows ?? throw new ArgumentNullException(nameof(dataRows));

    var listRem = new List<DataColumn>();
    foreach (DataColumn col in m_DataTable.Columns)
    {
      if (string.Equals(col.ColumnName, ReaderConstants.cRecordNumberFieldName, StringComparison.OrdinalIgnoreCase)||
          string.Equals(col.ColumnName, ReaderConstants.cStartLineNumberFieldName, StringComparison.OrdinalIgnoreCase) ||
          string.Equals(col.ColumnName, ReaderConstants.cErrorField, StringComparison.OrdinalIgnoreCase) ||
          string.Equals(col.ColumnName, ReaderConstants.cEndLineNumberFieldName, StringComparison.OrdinalIgnoreCase))
        listRem.Add(col);
    }

    foreach (var col in listRem)
      m_DataTable.Columns.Remove(col);

    m_InitialColumn = initialColumn;
    InitializeComponent();
    detailControl.ReadOnly = true;
    detailControl.ShowFilter = false;
    detailControl.ShowInfoButtons = false;
    detailControl.HtmlStyle = htmlStyle;
  }

  /// <summary>
  ///   Handles the SelectedIndexChanged event of the comboBoxID control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
  private void ComboBoxID_SelectedIndexChanged(object? sender, EventArgs e) =>
    Work(comboBoxID.Text, checkBoxIgnoreNull.Checked);

  private void UniqueDisplay_FormClosing(object? sender, FormClosingEventArgs e) =>
    m_CancellationTokenSource.Cancel();

  /// <summary>
  ///   Handles the Load event of the FormUniqueDisplay control.
  /// </summary>
  /// <param name="sender">The source of the event.</param>
  /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
  private async void FormUniqueDisplay_Load(object? sender, EventArgs e)
  {
    var index = 0;
    var current = 0;
    foreach (var column in m_DataTable.GetRealColumns())
    {
      if (!string.IsNullOrEmpty(m_InitialColumn)
          && column.ColumnName.Equals(m_InitialColumn, StringComparison.OrdinalIgnoreCase))
        index = current;
      comboBoxID.Items.Add(column.ColumnName);
      current++;
    }
    comboBoxID.SelectedIndex = index;
    await detailControl.LoadDataTableAsync(m_DataTable, RowFilterTypeEnum.All, m_CancellationTokenSource.Token);
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
      DataColumn dataColumnId = m_DataTable.Columns[dataColumnName] ??
                                throw new Exception($"Column {dataColumnName} not found");

      this.SafeBeginInvoke(() => Text = $@"Unique Values Display - {dataColumnName} ");

      var dictIdToRow = new DictionaryIgnoreCase<int>();
      var dictIdToCount = new DictionaryIgnoreCase<int>();

      using var formProgress =
        new FormProgress($"Processing {dataColumnName}", m_CancellationTokenSource.Token)
        {
          Maximum = m_DataRow.Length
        };
      formProgress.Show(this);
      var intervalAction = new IntervalAction();
      for (var rowIndex = 0; rowIndex < m_DataRow.Length; rowIndex++)
      {
        if (formProgress.CancellationToken.IsCancellationRequested)
          return;
        intervalAction.Invoke(formProgress, "Getting Unique values", rowIndex);
        var dataRow = m_DataRow[rowIndex];
        if (ignoreNull && dataRow.IsNull(dataColumnId.Ordinal))
          continue;
        var id = dataRow[dataColumnId.Ordinal].ToString()?.Trim().ToLowerInvariant() ?? string.Empty;
        if (dictIdToRow.ContainsKey(id))
          dictIdToCount[id]++;
        else
        {
          dictIdToRow.Add(id, rowIndex);
          dictIdToCount.Add(id, 1);
        }
      }

      this.SafeInvoke(
        () => Text = $@"Unique Values Display - {dataColumnName} - Rows {dictIdToRow.Count}/{m_DataRow.Length}");

      var countCol = m_DataTable.Columns["Count#"];
      if (countCol is null)
      {
        m_DataTable.Columns.Add(new DataColumn("Count#", typeof(int)));
        countCol = m_DataTable.Columns["Count#"];
      }

      m_DataTable.BeginLoadData();
      m_DataTable.Clear();

      formProgress.Maximum = dictIdToRow.Count;

      var counter = 0;
      foreach (var rowIndex in dictIdToRow)
      {
        if (formProgress.CancellationToken.IsCancellationRequested)
          return;
        counter++;
        intervalAction.Invoke(formProgress, "Importing Rows to Grid", counter);
        m_DataTable.ImportRow(m_DataRow[rowIndex.Value]);
        if (m_DataTable.Rows.Count > 0)
          // add the counter for the values
          m_DataTable.Rows[m_DataTable.Rows.Count - 1][countCol!.Ordinal] = dictIdToCount[rowIndex.Key];
      }

      m_DataTable.EndLoadData();
      formProgress.Maximum = 0;
      formProgress.Report("Sorting");
      detailControl.Sort(dataColumnName);
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