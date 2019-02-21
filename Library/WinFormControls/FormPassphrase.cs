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

using System;
using System.Windows.Forms;

namespace CsvTools
{
  public sealed class FormPassphrase : Form
  {
    private Button m_BtnOk;
    private CheckBox m_CheckBoxShowHide;
    private TableLayoutPanel tableLayoutPanel1;
    private Button m_BtnCancel;
    private TextBox m_TextBox;

    public FormPassphrase()
    {
      InitializeComponent();
    }

    public FormPassphrase(string title)
    {
      InitializeComponent();
      Text = title;
    }

    public string EncryptedPassphrase
    {
      get
      {
        if (string.IsNullOrEmpty(m_TextBox.Text))
          return null;
        else
          return m_TextBox.Text.Encrypt();
      }
    }

    /// <summary>
    ///   Required method for Designer support - do not modify
    ///   the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.m_BtnOk = new System.Windows.Forms.Button();
      this.m_TextBox = new System.Windows.Forms.TextBox();
      this.m_CheckBoxShowHide = new System.Windows.Forms.CheckBox();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.m_BtnCancel = new System.Windows.Forms.Button();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_BtnOk
      // 
      this.m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.m_BtnOk.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_BtnOk.Location = new System.Drawing.Point(245, 29);
      this.m_BtnOk.Name = "m_BtnOk";
      this.m_BtnOk.Size = new System.Drawing.Size(64, 28);
      this.m_BtnOk.TabIndex = 3;
      this.m_BtnOk.Text = "OK";
      this.m_BtnOk.Click += new System.EventHandler(this.BtnOK_Click);
      // 
      // m_TextBox
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.m_TextBox, 3);
      this.m_TextBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TextBox.Location = new System.Drawing.Point(3, 3);
      this.m_TextBox.Name = "m_TextBox";
      this.m_TextBox.PasswordChar = '*';
      this.m_TextBox.Size = new System.Drawing.Size(376, 20);
      this.m_TextBox.TabIndex = 0;
      // 
      // m_CheckBoxShowHide
      // 
      this.m_CheckBoxShowHide.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.m_CheckBoxShowHide.AutoSize = true;
      this.m_CheckBoxShowHide.Location = new System.Drawing.Point(159, 29);
      this.m_CheckBoxShowHide.Name = "m_CheckBoxShowHide";
      this.m_CheckBoxShowHide.Size = new System.Drawing.Size(80, 28);
      this.m_CheckBoxShowHide.TabIndex = 1;
      this.m_CheckBoxShowHide.Text = "Show Entry";
      this.m_CheckBoxShowHide.UseVisualStyleBackColor = true;
      this.m_CheckBoxShowHide.CheckedChanged += new System.EventHandler(this.CheckBoxShowHide_CheckedChanged);
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.tableLayoutPanel1.Controls.Add(this.m_BtnCancel, 2, 1);
      this.tableLayoutPanel1.Controls.Add(this.m_TextBox, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.m_BtnOk, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.m_CheckBoxShowHide, 0, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.Size = new System.Drawing.Size(382, 60);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // m_BtnCancel
      // 
      this.m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_BtnCancel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_BtnCancel.Location = new System.Drawing.Point(315, 29);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new System.Drawing.Size(64, 28);
      this.m_BtnCancel.TabIndex = 4;
      this.m_BtnCancel.Text = "Cancel";
      this.m_BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
      // 
      // FormPassphrase
      // 
      this.AcceptButton = this.m_BtnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(382, 60);
      this.Controls.Add(this.tableLayoutPanel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(346, 85);
      this.Name = "FormPassphrase";
      this.ShowIcon = false;
      this.Text = "PGP Private Key Passphrase";
      this.TopMost = true;
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void CheckBoxShowHide_CheckedChanged(object sender, EventArgs e)
    {
      m_TextBox.PasswordChar = m_CheckBoxShowHide.Checked ? '\0' : '*';
    }
  }
}