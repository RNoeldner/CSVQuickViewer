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

namespace CsvTools
{
  using System;
  using System.ComponentModel;
  using System.Diagnostics.CodeAnalysis;
  using System.Windows.Forms;

  public sealed class FormFileSelect : ResizeForm
  {
    private Button m_BtnCancel;
    private Button m_BtnOk;
    private TextBox m_TextBoxKeyFile;
    private Button m_ButtonKeyFile;
    private TableLayoutPanel m_TableLayoutPanel;

#pragma warning disable CS8618
    public FormFileSelect(string title = "")
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
    public string FileName => m_TextBoxKeyFile.Text;

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void BtnOK_Click(object? sender, EventArgs e)
    {
      DialogResult = FileSystemUtils.FileExists(FileName) ? DialogResult.OK : DialogResult.Cancel;
      Close();
    }


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
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.m_BtnCancel = new System.Windows.Forms.Button();
      this.m_TextBoxKeyFile = new System.Windows.Forms.TextBox();
      this.m_ButtonKeyFile = new System.Windows.Forms.Button();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_BtnOk
      // 
      this.m_BtnOk.AutoSize = true;
      this.m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.m_BtnOk.Location = new System.Drawing.Point(364, 29);
      this.m_BtnOk.Margin = new System.Windows.Forms.Padding(2);
      this.m_BtnOk.Name = "m_BtnOk";
      this.m_BtnOk.Size = new System.Drawing.Size(57, 25);
      this.m_BtnOk.TabIndex = 3;
      this.m_BtnOk.Text = "&OK";
      this.m_BtnOk.Click += new System.EventHandler(this.BtnOK_Click);
      // 
      // m_TableLayoutPanel
      // 
      this.m_TableLayoutPanel.ColumnCount = 3;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnCancel, 2, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnOk, 1, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxKeyFile, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_ButtonKeyFile, 1, 0);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 2;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(484, 58);
      this.m_TableLayoutPanel.TabIndex = 0;
      // 
      // m_BtnCancel
      // 
      this.m_BtnCancel.AutoSize = true;
      this.m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_BtnCancel.Location = new System.Drawing.Point(425, 29);
      this.m_BtnCancel.Margin = new System.Windows.Forms.Padding(2);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new System.Drawing.Size(57, 25);
      this.m_BtnCancel.TabIndex = 4;
      this.m_BtnCancel.Text = "&Cancel";
      this.m_BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
      // 
      // textBoxKeyFile
      // 
      this.m_TextBoxKeyFile.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.m_TextBoxKeyFile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
      this.m_TextBoxKeyFile.Location = new System.Drawing.Point(2, 2);
      this.m_TextBoxKeyFile.Margin = new System.Windows.Forms.Padding(2);
      this.m_TextBoxKeyFile.MinimumSize = new System.Drawing.Size(46, 4);
      this.m_TextBoxKeyFile.Name = "m_TextBoxKeyFile";
      this.m_TextBoxKeyFile.Size = new System.Drawing.Size(358, 20);
      this.m_TextBoxKeyFile.TabIndex = 33;
      // 
      // buttonKeyFile
      // 
      this.m_ButtonKeyFile.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_ButtonKeyFile, 2);
      this.m_ButtonKeyFile.Location = new System.Drawing.Point(364, 2);
      this.m_ButtonKeyFile.Margin = new System.Windows.Forms.Padding(2);
      this.m_ButtonKeyFile.Name = "m_ButtonKeyFile";
      this.m_ButtonKeyFile.Size = new System.Drawing.Size(118, 23);
      this.m_ButtonKeyFile.TabIndex = 34;
      this.m_ButtonKeyFile.Text = "Select File";
      this.m_ButtonKeyFile.UseVisualStyleBackColor = true;
      this.m_ButtonKeyFile.Click += new System.EventHandler(this.buttonKeyFile_Click);
      // 
      // FormFileSelect
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(484, 58);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.MinimumSize = new System.Drawing.Size(338, 71);
      this.Name = "FormFileSelect";
      this.ShowIcon = false;
      this.Text = "File Location";
      this.TopMost = true;
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
      this.ResumeLayout(false);

    }

    private void buttonKeyFile_Click(object sender, EventArgs e)
    {
      try
      {
        var split = FileSystemUtils.SplitPath(m_TextBoxKeyFile.Text);
        var newFileName = WindowsAPICodePackWrapper.Open(
          split.DirectoryName,
          "File with PGP Key",
          "Key file (*.asc;*.txt;*.key;*.ascii)|*.ascii;*.txt;*.key;*.asc|All files (*.*)|*.*",
          split.FileName);

        if (newFileName is null || newFileName.Length == 0)
          return;
        m_TextBoxKeyFile.Text = newFileName;
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "File Location");
      }

    }
  }
}