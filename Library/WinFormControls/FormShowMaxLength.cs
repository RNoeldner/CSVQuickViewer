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
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   Form showing the length of columns
  /// </summary>
  public class FormShowMaxLength : Form
  {
    private readonly IContainer components = null;
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
      Icon = Resources.SubFormIcon;
    }

    /// <summary>
    ///   Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing) components?.Dispose();
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 =
        new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 =
        new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 =
        new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 =
        new System.Windows.Forms.DataGridViewCellStyle();
      this.m_DataGridView = new CsvTools.FilteredDataGridView();
      ((System.ComponentModel.ISupportInitialize)(this.m_DataGridView)).BeginInit();
      this.SuspendLayout();
      //
      // dataGridView
      //
      this.m_DataGridView.AllowUserToAddRows = false;
      this.m_DataGridView.AllowUserToDeleteRows = false;
      this.m_DataGridView.AllowUserToOrderColumns = true;
      dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))),
        ((int)(((byte)(224)))), ((int)(((byte)(224)))));
      this.m_DataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle5;
      dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F,
        System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.m_DataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle6;
      this.m_DataGridView.ColumnHeadersHeightSizeMode =
        System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F,
        System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle7.ForeColor = System.Drawing.Color.Black;
      dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.m_DataGridView.DefaultCellStyle = dataGridViewCellStyle7;
      this.m_DataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_DataGridView.Location = new System.Drawing.Point(0, 0);
      this.m_DataGridView.Name = "m_DataGridView";
      dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Control;
      dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F,
        System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
      dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.m_DataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle8;
      this.m_DataGridView.Size = new System.Drawing.Size(362, 310);
      this.m_DataGridView.TabIndex = 0;
      //
      // FormShowMaxLength
      //
      this.ClientSize = new System.Drawing.Size(362, 310);
      this.Controls.Add(this.m_DataGridView);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormShowMaxLength";
      this.Text = "Column Length";
      this.Load += new System.EventHandler(this.ShowMaxLength_Load);
      ((System.ComponentModel.ISupportInitialize)(this.m_DataGridView)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion Windows Form Designer generated code

    /// <summary>
    ///   Handles the Load event of the ShowMaxLength control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void ShowMaxLength_Load(object sender, EventArgs e)
    {
      try
      {
        var dataTable = new DataTable
        {
          TableName = "DataTable",
          Locale = CultureInfo.InvariantCulture
        };
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
          foreach (var row in m_DataRow)
            foreach (var col in checkCols)
            {
              var cl = (row[col.Value] == DBNull.Value) ? 0 : row[col.Value].ToString().Length;
              if (cl > maxLength[col.Key])
              {
                maxLength[col.Key] = cl;
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