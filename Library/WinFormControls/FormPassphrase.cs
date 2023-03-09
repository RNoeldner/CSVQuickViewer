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

using System.Security;

namespace CsvTools
{
  using System;
  using System.ComponentModel;
  using System.Diagnostics.CodeAnalysis;
  using System.Windows.Forms;

  public sealed class FormPassphrase : ResizeForm
  {
    private Button m_BtnCancel;
    private Button m_BtnOk;
    private CheckBox m_CheckBoxShowHide;
    private TableLayoutPanel m_TableLayoutPanel;
    private TextBox m_TextBox;

#pragma warning disable CS8618
    public FormPassphrase(string title = "")
#pragma warning restore CS8618 
    {
      InitializeComponent();
      if (!string.IsNullOrEmpty(title))
        Text = title;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
    public SecureString Passphrase =>
      m_TextBox.Text.ToSecureString();

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void BtnOK_Click(object? sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void CheckBoxShowHide_CheckedChanged(object? sender, EventArgs e) =>
      m_TextBox.PasswordChar = m_CheckBoxShowHide.Checked ? char.MinValue : '*';

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "RedundantCast")]
    [SuppressMessage("ReSharper", "RedundantDelegateCreation")]
    private void InitializeComponent()
    {
      this.m_BtnOk = new System.Windows.Forms.Button();
      this.m_TextBox = new System.Windows.Forms.TextBox();
      this.m_CheckBoxShowHide = new System.Windows.Forms.CheckBox();
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.m_BtnCancel = new System.Windows.Forms.Button();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_BtnOk
      // 
      this.m_BtnOk.AutoSize = true;
      this.m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.m_BtnOk.Location = new System.Drawing.Point(346, 26);
      this.m_BtnOk.Margin = new System.Windows.Forms.Padding(2);
      this.m_BtnOk.Name = "m_BtnOk";
      this.m_BtnOk.Size = new System.Drawing.Size(65, 25);
      this.m_BtnOk.TabIndex = 3;
      this.m_BtnOk.Text = "&OK";
      this.m_BtnOk.Click += new System.EventHandler(this.BtnOK_Click);
      // 
      // m_TextBox
      // 
      this.m_TableLayoutPanel.SetColumnSpan(this.m_TextBox, 3);
      this.m_TextBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TextBox.Location = new System.Drawing.Point(3, 2);
      this.m_TextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TextBox.Name = "m_TextBox";
      this.m_TextBox.PasswordChar = '*';
      this.m_TextBox.Size = new System.Drawing.Size(478, 20);
      this.m_TextBox.TabIndex = 0;
      // 
      // m_CheckBoxShowHide
      // 
      this.m_CheckBoxShowHide.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_CheckBoxShowHide.AutoSize = true;
      this.m_CheckBoxShowHide.Location = new System.Drawing.Point(262, 30);
      this.m_CheckBoxShowHide.Margin = new System.Windows.Forms.Padding(2);
      this.m_CheckBoxShowHide.Name = "m_CheckBoxShowHide";
      this.m_CheckBoxShowHide.Size = new System.Drawing.Size(80, 17);
      this.m_CheckBoxShowHide.TabIndex = 1;
      this.m_CheckBoxShowHide.Text = "Show Entry";
      this.m_CheckBoxShowHide.UseVisualStyleBackColor = true;
      this.m_CheckBoxShowHide.CheckedChanged += new System.EventHandler(this.CheckBoxShowHide_CheckedChanged);
      // 
      // m_TableLayoutPanel
      // 
      this.m_TableLayoutPanel.ColumnCount = 3;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnCancel, 2, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBox, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnOk, 1, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_CheckBoxShowHide, 0, 1);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 2;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(484, 54);
      this.m_TableLayoutPanel.TabIndex = 0;
      // 
      // m_BtnCancel
      // 
      this.m_BtnCancel.AutoSize = true;
      this.m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_BtnCancel.Location = new System.Drawing.Point(416, 26);
      this.m_BtnCancel.Margin = new System.Windows.Forms.Padding(2);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new System.Drawing.Size(65, 25);
      this.m_BtnCancel.TabIndex = 4;
      this.m_BtnCancel.Text = "&Cancel";
      this.m_BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
      // 
      // FormPassphrase
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(484, 54);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.MinimumSize = new System.Drawing.Size(338, 71);
      this.Name = "FormPassphrase";
      this.ShowIcon = false;
      this.Text = "Passphrase";
      this.TopMost = true;
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
      this.ResumeLayout(false);

    }
  }
}