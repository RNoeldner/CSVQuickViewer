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
#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Form showing the length of columns
  /// </summary>
  public class FormShowMaxLength : ResizeForm
  {
    private readonly DataRow[] m_DataRow;
    private readonly DataTable m_DataTable;
    private readonly IList<string> m_VisibleColumns;
    private FilteredDataGridView m_DataGridView;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormShowMaxLength" /> class.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="dataRows">The data rows.</param>
    /// <param name="visibleColumns">The visible columns.</param>
    /// <param name="hTmlStyle">The h TML style.</param>
    /// <exception cref="ArgumentNullException">dataTable or dataRows or visibleColumns</exception>
    public FormShowMaxLength(in DataTable? dataTable, in DataRow[] dataRows, in IList<string>? visibleColumns,
      in HtmlStyle hTmlStyle)
    {
      m_DataTable = dataTable ?? throw new ArgumentNullException(nameof(dataTable));
      m_DataRow = dataRows ?? throw new ArgumentNullException(nameof(dataRows));
      m_VisibleColumns = visibleColumns ?? throw new ArgumentNullException(nameof(visibleColumns));
      InitializeComponent();
      m_DataGridView!.HtmlStyle = hTmlStyle;
    }

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "RedundantCast")]
    [SuppressMessage("ReSharper", "RedundantDelegateCreation")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    private void InitializeComponent()
    {
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 =
        new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 =
        new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 =
        new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 =
        new System.Windows.Forms.DataGridViewCellStyle();
      this.m_DataGridView = new CsvTools.FilteredDataGridView();
      ((System.ComponentModel.ISupportInitialize) (this.m_DataGridView)).BeginInit();
      this.SuspendLayout();
      // m_DataGridView
      this.m_DataGridView.AllowUserToAddRows = false;
      this.m_DataGridView.AllowUserToDeleteRows = false;
      this.m_DataGridView.AllowUserToOrderColumns = true;
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int) (((byte) (224)))),
        ((int) (((byte) (224)))), ((int) (((byte) (224)))));
      this.m_DataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.m_DataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
      this.m_DataGridView.ColumnHeadersHeightSizeMode =
        System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.m_DataGridView.DefaultCellStyle = dataGridViewCellStyle3;
      this.m_DataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_DataGridView.FileSetting = null;
      this.m_DataGridView.Location = new System.Drawing.Point(0, 0);
      this.m_DataGridView.Name = "m_DataGridView";
      dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.m_DataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
      this.m_DataGridView.RowHeadersWidth = 51;
      this.m_DataGridView.Size = new System.Drawing.Size(387, 310);
      this.m_DataGridView.TabIndex = 0;
      // FormShowMaxLength
      this.ClientSize = new System.Drawing.Size(387, 310);
      this.Controls.Add(this.m_DataGridView);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormShowMaxLength";
      this.Text = "Column Length";
      this.Load += new System.EventHandler(this.ShowMaxLength_Load);
      ((System.ComponentModel.ISupportInitialize) (this.m_DataGridView)).EndInit();
      this.ResumeLayout(false);
    }

    /// <summary>
    ///   Handles the Load event of the ShowMaxLength control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ShowMaxLength_Load(object? sender, EventArgs e)
    {
      if (m_DataRow.Length == 0)
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

        var dataColumnOrder = dataTable.Columns.Add("Order", typeof(int));
        dataColumnNo.AllowDBNull = true;

        var maxLength = new DictionaryIgnoreCase<int>();
        var colIndex = new DictionaryIgnoreCase<int>();
        var checkCols = new DictionaryIgnoreCase<int>();

        foreach (var col in m_DataTable.GetRealColumns())
        {
          checkCols.Add(col.ColumnName, col.Ordinal);
          maxLength.Add(col.ColumnName, -1);
          colIndex.Add(col.ColumnName, col.Ordinal + 1);
        }

        if (colIndex.Count > 0)
          foreach (var row in m_DataRow)
          foreach (var col in checkCols)
          {
            var cl = row[col.Value] == DBNull.Value || row[col.Value] is null
              ? 0
              : row[col.Value].ToString()?.Length ?? 0;
            if (cl > maxLength[col.Key]) maxLength[col.Key] = cl;
          }

        var colNo = 1;
        foreach (var len in maxLength)
        {
          var lastRow = dataTable.NewRow();
          lastRow[dataColumnName] = len.Key;

          if (len.Value != -1)
            lastRow[dataColumnLength] = len.Value.ToString(CultureInfo.CurrentCulture);
          else
            lastRow[dataColumnLength] = m_DataTable.Columns[len.Key]?.DataType.Name;

          lastRow[dataColumnNo] = colNo++;
          var index = m_VisibleColumns.IndexOf(len.Key);
          if (index != -1)
            lastRow[dataColumnOrder] = index + 1;
          dataTable.Rows.Add(lastRow);
        }

        m_DataGridView.DataSource = dataTable;
        // ReSharper disable PossibleNullReferenceException
        m_DataGridView.Columns[dataColumnName.ColumnName].Width = 150;

        m_DataGridView.Columns[dataColumnLength.ColumnName].Width = 60;
        m_DataGridView.Columns[dataColumnLength.ColumnName].DefaultCellStyle.Alignment =
          DataGridViewContentAlignment.MiddleRight;
        m_DataGridView.Columns[dataColumnLength.ColumnName].DefaultCellStyle.Format = "0";

        m_DataGridView.Columns[dataColumnNo.ColumnName].Width = 60;
        m_DataGridView.Columns[dataColumnNo.ColumnName].DefaultCellStyle.Alignment =
          DataGridViewContentAlignment.MiddleRight;
        m_DataGridView.Columns[dataColumnNo.ColumnName].DefaultCellStyle.Format = "0";
        // ReSharper restore PossibleNullReferenceException
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
    }
  }
}
