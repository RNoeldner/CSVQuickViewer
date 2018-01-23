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
    private Button m_BtnCancel;
    private Button m_BtnOk;
    private CheckBox m_CheckBoxShowHide;
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

    public string EncryptedPassphrase => m_TextBox.Text.Encrypt();

    /// <summary>
    ///   Required method for Designer support - do not modify
    ///   the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.m_BtnOk = new System.Windows.Forms.Button();
      this.m_TextBox = new System.Windows.Forms.TextBox();
      this.m_BtnCancel = new System.Windows.Forms.Button();
      this.m_CheckBoxShowHide = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      //
      // m_BtnOk
      //
      this.m_BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.m_BtnOk.Location = new System.Drawing.Point(513, 23);
      this.m_BtnOk.Name = "m_BtnOk";
      this.m_BtnOk.Size = new System.Drawing.Size(61, 23);
      this.m_BtnOk.TabIndex = 4;
      this.m_BtnOk.Text = "OK";
      this.m_BtnOk.Click += new System.EventHandler(this.BtnOK_Click);
      //
      // m_TextBox
      //
      this.m_TextBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TextBox.Location = new System.Drawing.Point(0, 0);
      this.m_TextBox.Name = "m_TextBox";
      this.m_TextBox.PasswordChar = '*';
      this.m_TextBox.Size = new System.Drawing.Size(577, 20);
      this.m_TextBox.TabIndex = 0;
      //
      // m_BtnCancel
      //
      this.m_BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_BtnCancel.Location = new System.Drawing.Point(450, 23);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new System.Drawing.Size(61, 23);
      this.m_BtnCancel.TabIndex = 3;
      this.m_BtnCancel.Text = "Cancel";
      this.m_BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
      //
      // m_CheckBoxShowHide
      //
      this.m_CheckBoxShowHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_CheckBoxShowHide.AutoSize = true;
      this.m_CheckBoxShowHide.Location = new System.Drawing.Point(363, 27);
      this.m_CheckBoxShowHide.Name = "m_CheckBoxShowHide";
      this.m_CheckBoxShowHide.Size = new System.Drawing.Size(80, 17);
      this.m_CheckBoxShowHide.TabIndex = 2;
      this.m_CheckBoxShowHide.Text = "Show Entry";
      this.m_CheckBoxShowHide.UseVisualStyleBackColor = true;
      this.m_CheckBoxShowHide.CheckedChanged += new System.EventHandler(this.CheckBoxShowHide_CheckedChanged);
      //
      // FormPassphrase
      //
      this.AcceptButton = this.m_BtnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.m_BtnCancel;
      this.ClientSize = new System.Drawing.Size(577, 51);
      this.ControlBox = false;
      this.Controls.Add(this.m_TextBox);
      this.Controls.Add(this.m_CheckBoxShowHide);
      this.Controls.Add(this.m_BtnCancel);
      this.Controls.Add(this.m_BtnOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximumSize = new System.Drawing.Size(700, 85);
      this.MinimumSize = new System.Drawing.Size(380, 85);
      this.Name = "FormPassphrase";
      this.ShowIcon = false;
      this.Text = "PGP Private Key Passphrase";
      this.TopMost = true;
      this.ResumeLayout(false);
      this.PerformLayout();
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