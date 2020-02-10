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
  using System.ComponentModel;
  using System.Drawing;
  using System.Windows.Forms;

  public sealed class FormPassphrase : Form
  {
    private Button m_BtnCancel;

    private Button m_BtnOk;

    private CheckBox m_CheckBoxShowHide;

    private TextBox m_TextBox;

    private TableLayoutPanel m_TableLayoutPanel;

    public FormPassphrase() : this("Passphrase")
    {
    }

    public FormPassphrase(string title)
    {
      InitializeComponent();
      Text = title;
    }


    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
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

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void CheckBoxShowHide_CheckedChanged(object sender, EventArgs e) =>
      m_TextBox.PasswordChar = m_CheckBoxShowHide.Checked ? '\0' : '*';

    /// <summary>
    ///   Required method for Designer support - do not modify
    ///   the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.m_BtnOk = new Button();
      this.m_TextBox = new TextBox();
      this.m_CheckBoxShowHide = new CheckBox();
      this.m_TableLayoutPanel = new TableLayoutPanel();
      this.m_BtnCancel = new Button();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();

      // m_BtnOk
      this.m_BtnOk.AutoSize = true;
      this.m_BtnOk.DialogResult = DialogResult.OK;
      this.m_BtnOk.Location = new Point(392, 39);
      this.m_BtnOk.Name = "m_BtnOk";
      this.m_BtnOk.Size = new Size(99, 34);
      this.m_BtnOk.TabIndex = 3;
      this.m_BtnOk.Text = "OK";
      this.m_BtnOk.Click += new EventHandler(this.BtnOK_Click);

      // m_TextBox
      this.m_TableLayoutPanel.SetColumnSpan(this.m_TextBox, 3);
      this.m_TextBox.Dock = DockStyle.Top;
      this.m_TextBox.Location = new Point(4, 5);
      this.m_TextBox.Margin = new Padding(4, 5, 4, 5);
      this.m_TextBox.Name = "m_TextBox";
      this.m_TextBox.PasswordChar = '*';
      this.m_TextBox.Size = new Size(591, 26);
      this.m_TextBox.TabIndex = 0;

      // m_CheckBoxShowHide
      this.m_CheckBoxShowHide.Anchor = AnchorStyles.Right;
      this.m_CheckBoxShowHide.AutoSize = true;
      this.m_CheckBoxShowHide.Location = new Point(270, 45);
      this.m_CheckBoxShowHide.Name = "m_CheckBoxShowHide";
      this.m_CheckBoxShowHide.Size = new Size(116, 24);
      this.m_CheckBoxShowHide.TabIndex = 1;
      this.m_CheckBoxShowHide.Text = "Show Entry";
      this.m_CheckBoxShowHide.UseVisualStyleBackColor = true;
      this.m_CheckBoxShowHide.CheckedChanged += new EventHandler(this.CheckBoxShowHide_CheckedChanged);

      // tableLayoutPanel1
      this.m_TableLayoutPanel.ColumnCount = 3;
      this.m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnCancel, 2, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBox, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnOk, 1, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_CheckBoxShowHide, 0, 1);
      this.m_TableLayoutPanel.Dock = DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new Point(0, 0);
      this.m_TableLayoutPanel.Margin = new Padding(4, 5, 4, 5);
      this.m_TableLayoutPanel.Name = "tableLayoutPanel1";
      this.m_TableLayoutPanel.RowCount = 2;
      this.m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      this.m_TableLayoutPanel.Size = new Size(599, 78);
      this.m_TableLayoutPanel.TabIndex = 0;

      // m_BtnCancel
      this.m_BtnCancel.AutoSize = true;
      this.m_BtnCancel.DialogResult = DialogResult.Cancel;
      this.m_BtnCancel.Location = new Point(497, 39);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new Size(99, 34);
      this.m_BtnCancel.TabIndex = 4;
      this.m_BtnCancel.Text = "Cancel";
      this.m_BtnCancel.Click += new EventHandler(this.BtnCancel_Click);

      // FormPassphrase
      this.AcceptButton = this.m_BtnOk;
      this.AutoScaleDimensions = new SizeF(9F, 20F);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = this.m_BtnCancel;
      this.ClientSize = new Size(599, 78);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
      this.Margin = new Padding(4, 5, 4, 5);
      this.MinimumSize = new Size(500, 88);
      this.Name = "FormPassphrase";
      this.ShowIcon = false;
      this.Text = "PGP Private Key Passphrase";
      this.TopMost = true;
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
      this.ResumeLayout(false);
    }
  }
}