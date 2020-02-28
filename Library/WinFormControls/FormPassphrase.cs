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

  public sealed class FormPassphrase : ResizeForm
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
      m_BtnOk = new Button();
      m_TextBox = new TextBox();
      m_CheckBoxShowHide = new CheckBox();
      m_TableLayoutPanel = new TableLayoutPanel();
      m_BtnCancel = new Button();
      m_TableLayoutPanel.SuspendLayout();
      SuspendLayout();

      // m_BtnOk
      m_BtnOk.AutoSize = true;
      m_BtnOk.DialogResult = DialogResult.OK;
      m_BtnOk.Location = new Point(392, 39);
      m_BtnOk.Name = "m_BtnOk";
      m_BtnOk.Size = new Size(99, 34);
      m_BtnOk.TabIndex = 3;
      m_BtnOk.Text = "OK";
      m_BtnOk.Click += new EventHandler(BtnOK_Click);

      // m_TextBox
      m_TableLayoutPanel.SetColumnSpan(m_TextBox, 3);
      m_TextBox.Dock = DockStyle.Top;
      m_TextBox.Location = new Point(4, 5);
      m_TextBox.Margin = new Padding(4, 5, 4, 5);
      m_TextBox.Name = "m_TextBox";
      m_TextBox.PasswordChar = '*';
      m_TextBox.Size = new Size(591, 26);
      m_TextBox.TabIndex = 0;

      // m_CheckBoxShowHide
      m_CheckBoxShowHide.Anchor = AnchorStyles.Right;
      m_CheckBoxShowHide.AutoSize = true;
      m_CheckBoxShowHide.Location = new Point(270, 45);
      m_CheckBoxShowHide.Name = "m_CheckBoxShowHide";
      m_CheckBoxShowHide.Size = new Size(116, 24);
      m_CheckBoxShowHide.TabIndex = 1;
      m_CheckBoxShowHide.Text = "Show Entry";
      m_CheckBoxShowHide.UseVisualStyleBackColor = true;
      m_CheckBoxShowHide.CheckedChanged += new EventHandler(CheckBoxShowHide_CheckedChanged);

      // tableLayoutPanel1
      m_TableLayoutPanel.ColumnCount = 3;
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 105F));
      m_TableLayoutPanel.Controls.Add(m_BtnCancel, 2, 1);
      m_TableLayoutPanel.Controls.Add(m_TextBox, 0, 0);
      m_TableLayoutPanel.Controls.Add(m_BtnOk, 1, 1);
      m_TableLayoutPanel.Controls.Add(m_CheckBoxShowHide, 0, 1);
      m_TableLayoutPanel.Dock = DockStyle.Fill;
      m_TableLayoutPanel.Location = new Point(0, 0);
      m_TableLayoutPanel.Margin = new Padding(4, 5, 4, 5);
      m_TableLayoutPanel.Name = "tableLayoutPanel1";
      m_TableLayoutPanel.RowCount = 2;
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.Size = new Size(599, 78);
      m_TableLayoutPanel.TabIndex = 0;

      // m_BtnCancel
      m_BtnCancel.AutoSize = true;
      m_BtnCancel.DialogResult = DialogResult.Cancel;
      m_BtnCancel.Location = new Point(497, 39);
      m_BtnCancel.Name = "m_BtnCancel";
      m_BtnCancel.Size = new Size(99, 34);
      m_BtnCancel.TabIndex = 4;
      m_BtnCancel.Text = "Cancel";
      m_BtnCancel.Click += new EventHandler(BtnCancel_Click);

      // FormPassphrase
      AcceptButton = m_BtnOk;
      AutoScaleDimensions = new SizeF(9F, 20F);
      AutoScaleMode = AutoScaleMode.Font;
      CancelButton = m_BtnCancel;
      ClientSize = new Size(599, 78);
      Controls.Add(m_TableLayoutPanel);
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      Margin = new Padding(4, 5, 4, 5);
      MinimumSize = new Size(500, 88);
      Name = "FormPassphrase";
      ShowIcon = false;
      Text = "PGP Private Key Passphrase";
      TopMost = true;
      m_TableLayoutPanel.ResumeLayout(false);
      m_TableLayoutPanel.PerformLayout();
      ResumeLayout(false);
    }
  }
}