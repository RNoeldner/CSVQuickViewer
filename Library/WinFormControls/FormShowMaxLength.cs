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
  using System.Diagnostics.Contracts;
  using System.Drawing;
  using System.Globalization;
  using System.Windows.Forms;

  /// <summary>
  ///   Form showing the length of columns
  /// </summary>
  public class FormShowMaxLength : ResizeForm
  {
    private readonly DataRow[] m_DataRow;

    private readonly DataTable m_DataTable;

    private FilteredDataGridView m_DataGridView;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormShowMaxLength" /> class.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="dataRows">The data rows.</param>
    public FormShowMaxLength(DataTable dataTable, DataRow[] dataRows)
    {
      Contract.Requires(dataTable != null);
      Contract.Requires(dataRows != null);
      m_DataTable = dataTable;
      m_DataRow = dataRows;
      InitializeComponent();
    }

    /// <summary>
    ///   Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) => base.Dispose(disposing);

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    private void InitializeComponent()
    {
      var dataGridViewCellStyle5 = new DataGridViewCellStyle();
      var dataGridViewCellStyle6 = new DataGridViewCellStyle();
      var dataGridViewCellStyle7 = new DataGridViewCellStyle();
      var dataGridViewCellStyle8 = new DataGridViewCellStyle();
      m_DataGridView = new FilteredDataGridView();
      ((ISupportInitialize)(m_DataGridView)).BeginInit();
      SuspendLayout();

      // dataGridView
      m_DataGridView.AllowUserToAddRows = false;
      m_DataGridView.AllowUserToDeleteRows = false;
      m_DataGridView.AllowUserToOrderColumns = true;
      dataGridViewCellStyle5.BackColor = Color.FromArgb(224, 224, 224);
      m_DataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
      dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle6.BackColor = SystemColors.Control;
      dataGridViewCellStyle6.ForeColor = SystemColors.WindowText;
      dataGridViewCellStyle6.SelectionBackColor = SystemColors.Highlight;
      dataGridViewCellStyle6.SelectionForeColor = SystemColors.HighlightText;
      dataGridViewCellStyle6.WrapMode = DataGridViewTriState.True;
      m_DataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
      m_DataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle7.BackColor = SystemColors.Window;
      dataGridViewCellStyle7.ForeColor = Color.Black;
      dataGridViewCellStyle7.SelectionBackColor = SystemColors.Highlight;
      dataGridViewCellStyle7.SelectionForeColor = SystemColors.HighlightText;
      dataGridViewCellStyle7.WrapMode = DataGridViewTriState.False;
      m_DataGridView.DefaultCellStyle = dataGridViewCellStyle7;
      m_DataGridView.Dock = DockStyle.Fill;
      m_DataGridView.Location = new Point(0, 0);
      m_DataGridView.Name = "m_DataGridView";
      dataGridViewCellStyle8.Alignment = DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle8.BackColor = SystemColors.Control;
      dataGridViewCellStyle8.ForeColor = SystemColors.WindowText;
      dataGridViewCellStyle8.SelectionBackColor = SystemColors.Highlight;
      dataGridViewCellStyle8.SelectionForeColor = SystemColors.HighlightText;
      dataGridViewCellStyle8.WrapMode = DataGridViewTriState.True;
      m_DataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle8;
      m_DataGridView.Size = new Size(362, 310);
      m_DataGridView.TabIndex = 0;

      // FormShowMaxLength
      ClientSize = new Size(362, 310);
      Controls.Add(m_DataGridView);
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      MaximizeBox = false;
      MinimizeBox = false;
      Name = "FormShowMaxLength";
      Text = "Column Length";
      Load += new EventHandler(ShowMaxLength_Load);
      ((ISupportInitialize)(m_DataGridView)).EndInit();
      ResumeLayout(false);
    }

    /// <summary>
    ///   Handles the Load event of the ShowMaxLength control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ShowMaxLength_Load(object sender, EventArgs e)
    {
      if (m_DataTable == null || m_DataRow == null || m_DataRow.Length == 0)
        return;

      try
      {
        var dataTable = new DataTable { TableName = "DataTable", Locale = CultureInfo.InvariantCulture };
        var dataColumnName = dataTable.Columns.Add("Name", typeof(string));
        dataColumnName.AllowDBNull = false;

        var dataColumnLength = dataTable.Columns.Add("Length", typeof(int));
        dataColumnLength.AllowDBNull = false;

        var dataColumnNo = dataTable.Columns.Add("ColumnNo", typeof(int));
        dataColumnNo.AllowDBNull = false;

        var maxLength = new Dictionary<string, int>();
        var colIndex = new Dictionary<string, int>();
        var checkCols = new Dictionary<string, int>();

        foreach (var col in m_DataTable.GetRealDataColumns())
        {
          checkCols.Add(col.ColumnName, col.Ordinal);
          maxLength.Add(col.ColumnName, -1);
          colIndex.Add(col.ColumnName, col.Ordinal + 1);
        }

        if (colIndex.Count > 0)
        {
          foreach (var row in m_DataRow)
            foreach (var col in checkCols)
            {
              var cl = (row[col.Value] == DBNull.Value) ? 0 : row[col.Value].ToString().Length;
              if (cl > maxLength[col.Key])
              {
                maxLength[col.Key] = cl;
              }
            }
        }

        var colNo = 1;
        foreach (var len in maxLength)
        {
          var lastRow = dataTable.NewRow();
          lastRow[dataColumnName] = len.Key;

          if (len.Value != -1)
            lastRow[dataColumnLength] = len.Value.ToString(CultureInfo.CurrentCulture);
          else
            lastRow[dataColumnLength] = m_DataTable.Columns[len.Key].DataType.Name;

          lastRow[dataColumnNo] = colNo++;
          dataTable.Rows.Add(lastRow);
        }

        m_DataGridView.DataSource = dataTable;

        m_DataGridView.Columns[dataColumnName.ColumnName].Width = 150;

        m_DataGridView.Columns[dataColumnLength.ColumnName].Width = 60;
        m_DataGridView.Columns[dataColumnLength.ColumnName].DefaultCellStyle.Alignment =
          DataGridViewContentAlignment.MiddleRight;
        m_DataGridView.Columns[dataColumnLength.ColumnName].DefaultCellStyle.Format = "0";

        m_DataGridView.Columns[dataColumnNo.ColumnName].Width = 60;
        m_DataGridView.Columns[dataColumnNo.ColumnName].DefaultCellStyle.Alignment =
          DataGridViewContentAlignment.MiddleRight;
        m_DataGridView.Columns[dataColumnNo.ColumnName].DefaultCellStyle.Format = "0";
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }
  }
}