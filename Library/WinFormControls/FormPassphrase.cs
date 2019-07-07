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

    public FormPassphrase() => InitializeComponent();

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
      m_BtnOk = new System.Windows.Forms.Button();
      m_TextBox = new System.Windows.Forms.TextBox();
      m_CheckBoxShowHide = new System.Windows.Forms.CheckBox();
      tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      m_BtnCancel = new System.Windows.Forms.Button();
      tableLayoutPanel1.SuspendLayout();
      SuspendLayout();
      //
      // m_BtnOk
      //
      m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      m_BtnOk.Dock = System.Windows.Forms.DockStyle.Fill;
      m_BtnOk.Location = new System.Drawing.Point(245, 29);
      m_BtnOk.Name = "m_BtnOk";
      m_BtnOk.Size = new System.Drawing.Size(64, 28);
      m_BtnOk.TabIndex = 3;
      m_BtnOk.Text = "OK";
      m_BtnOk.Click += new System.EventHandler(BtnOK_Click);
      //
      // m_TextBox
      //
      tableLayoutPanel1.SetColumnSpan(m_TextBox, 3);
      m_TextBox.Dock = System.Windows.Forms.DockStyle.Top;
      m_TextBox.Location = new System.Drawing.Point(3, 3);
      m_TextBox.Name = "m_TextBox";
      m_TextBox.PasswordChar = '*';
      m_TextBox.Size = new System.Drawing.Size(376, 20);
      m_TextBox.TabIndex = 0;
      //
      // m_CheckBoxShowHide
      //
      m_CheckBoxShowHide.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right);
      m_CheckBoxShowHide.AutoSize = true;
      m_CheckBoxShowHide.Location = new System.Drawing.Point(159, 29);
      m_CheckBoxShowHide.Name = "m_CheckBoxShowHide";
      m_CheckBoxShowHide.Size = new System.Drawing.Size(80, 28);
      m_CheckBoxShowHide.TabIndex = 1;
      m_CheckBoxShowHide.Text = "Show Entry";
      m_CheckBoxShowHide.UseVisualStyleBackColor = true;
      m_CheckBoxShowHide.CheckedChanged += new System.EventHandler(CheckBoxShowHide_CheckedChanged);
      //
      // tableLayoutPanel1
      //
      tableLayoutPanel1.ColumnCount = 3;
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      tableLayoutPanel1.Controls.Add(m_BtnCancel, 2, 1);
      tableLayoutPanel1.Controls.Add(m_TextBox, 0, 0);
      tableLayoutPanel1.Controls.Add(m_BtnOk, 1, 1);
      tableLayoutPanel1.Controls.Add(m_CheckBoxShowHide, 0, 1);
      tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanel1.Name = "tableLayoutPanel1";
      tableLayoutPanel1.RowCount = 2;
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.Size = new System.Drawing.Size(382, 60);
      tableLayoutPanel1.TabIndex = 0;
      //
      // m_BtnCancel
      //
      m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      m_BtnCancel.Dock = System.Windows.Forms.DockStyle.Fill;
      m_BtnCancel.Location = new System.Drawing.Point(315, 29);
      m_BtnCancel.Name = "m_BtnCancel";
      m_BtnCancel.Size = new System.Drawing.Size(64, 28);
      m_BtnCancel.TabIndex = 4;
      m_BtnCancel.Text = "Cancel";
      m_BtnCancel.Click += new System.EventHandler(BtnCancel_Click);
      //
      // FormPassphrase
      //
      AcceptButton = m_BtnOk;
      AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      ClientSize = new System.Drawing.Size(382, 60);
      Controls.Add(tableLayoutPanel1);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      MinimumSize = new System.Drawing.Size(346, 85);
      Name = "FormPassphrase";
      ShowIcon = false;
      Text = "PGP Private Key Passphrase";
      TopMost = true;
      tableLayoutPanel1.ResumeLayout(false);
      tableLayoutPanel1.PerformLayout();
      ResumeLayout(false);
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

    private void CheckBoxShowHide_CheckedChanged(object sender, EventArgs e) => m_TextBox.PasswordChar = m_CheckBoxShowHide.Checked ? '\0' : '*';
  }
}