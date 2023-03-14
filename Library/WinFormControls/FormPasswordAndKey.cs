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

  public sealed class FormPasswordAndKey : ResizeForm
  {
    private Button m_BtnCancel;
    private Button m_BtnOk;
    private TextBox m_TextBoxKeyFile;
    private Button m_ButtonKeyFile;
    private TextBox m_TextBoxPassphrase;
    private Label m_LabelPassphrase;
    private Label m_LabelKeyFile;
    private CheckBox m_CheckBoxShowHide;
    private TableLayoutPanel m_TableLayoutPanel;
    private bool m_ShowFileName = true;
    private bool m_ShowPassphrase = true;

#pragma warning disable CS8618
    public FormPasswordAndKey(string title = "")
#pragma warning restore CS8618 
    {
      InitializeComponent();
      if (!string.IsNullOrEmpty(title))
        Text = title;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [Bindable(true)]
    [Browsable(true)]
    public string Passphrase
    {
      get => m_TextBoxPassphrase.Text;
      set
      {
        m_TextBoxPassphrase.Text = value;
        ShowPassphrase = true;
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [Bindable(true)]
    [Browsable(true)]
    public string FileName
    {
      get => m_TextBoxKeyFile.Text;
      set
      {
        m_TextBoxKeyFile.Text = value;
        ShowFileName = true;
      }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [Bindable(true)]
    [Browsable(true)]
    public bool ShowFileName
    {
      get => m_ShowFileName;
      set
      {
        m_ShowFileName=value;
        FormPasswordAndKey_Shown(this, EventArgs.Empty);
      }
    }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [Bindable(true)]
    [Browsable(true)]
    public bool ShowPassphrase
    {
      get => m_ShowPassphrase;
      set
      {
        m_ShowPassphrase=value;
        FormPasswordAndKey_Shown(this, EventArgs.Empty);
      }
    }
    private void BtnCancel_Click(object? sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void BtnOK_Click(object? sender, EventArgs e)
    {
      DialogResult = !m_TextBoxKeyFile.Visible || string.IsNullOrEmpty(FileName) || FileSystemUtils.FileExists(FileName) ?
        DialogResult.OK :
        DialogResult.Cancel;
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
      this.m_TextBoxPassphrase = new System.Windows.Forms.TextBox();
      this.m_LabelPassphrase = new System.Windows.Forms.Label();
      this.m_LabelKeyFile = new System.Windows.Forms.Label();
      this.m_CheckBoxShowHide = new System.Windows.Forms.CheckBox();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_BtnOk
      // 
      this.m_BtnOk.AutoSize = true;
      this.m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.m_BtnOk.Location = new System.Drawing.Point(487, 53);
      this.m_BtnOk.Margin = new System.Windows.Forms.Padding(2);
      this.m_BtnOk.Name = "m_BtnOk";
      this.m_BtnOk.Size = new System.Drawing.Size(57, 25);
      this.m_BtnOk.TabIndex = 5;
      this.m_BtnOk.Text = "&OK";
      this.m_BtnOk.Click += new System.EventHandler(this.BtnOK_Click);
      // 
      // m_TableLayoutPanel
      // 
      this.m_TableLayoutPanel.ColumnCount = 4;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnCancel, 3, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnOk, 2, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxKeyFile, 1, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_ButtonKeyFile, 2, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBoxPassphrase, 1, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelPassphrase, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelKeyFile, 0, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_CheckBoxShowHide, 1, 2);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 3;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(607, 81);
      this.m_TableLayoutPanel.TabIndex = 0;
      // 
      // m_BtnCancel
      // 
      this.m_BtnCancel.AutoSize = true;
      this.m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_BtnCancel.Location = new System.Drawing.Point(548, 53);
      this.m_BtnCancel.Margin = new System.Windows.Forms.Padding(2);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new System.Drawing.Size(57, 25);
      this.m_BtnCancel.TabIndex = 6;
      this.m_BtnCancel.Text = "&Cancel";
      this.m_BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
      // 
      // m_TextBoxKeyFile
      // 
      this.m_TextBoxKeyFile.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.m_TextBoxKeyFile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
      this.m_TextBoxKeyFile.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TextBoxKeyFile.Location = new System.Drawing.Point(73, 26);
      this.m_TextBoxKeyFile.Margin = new System.Windows.Forms.Padding(2);
      this.m_TextBoxKeyFile.MinimumSize = new System.Drawing.Size(46, 4);
      this.m_TextBoxKeyFile.Name = "m_TextBoxKeyFile";
      this.m_TextBoxKeyFile.Size = new System.Drawing.Size(410, 20);
      this.m_TextBoxKeyFile.TabIndex = 2;
      // 
      // m_ButtonKeyFile
      // 
      this.m_ButtonKeyFile.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_ButtonKeyFile, 2);
      this.m_ButtonKeyFile.Location = new System.Drawing.Point(487, 26);
      this.m_ButtonKeyFile.Margin = new System.Windows.Forms.Padding(2);
      this.m_ButtonKeyFile.Name = "m_ButtonKeyFile";
      this.m_ButtonKeyFile.Size = new System.Drawing.Size(118, 23);
      this.m_ButtonKeyFile.TabIndex = 3;
      this.m_ButtonKeyFile.Text = "Select File";
      this.m_ButtonKeyFile.UseVisualStyleBackColor = true;
      this.m_ButtonKeyFile.Click += new System.EventHandler(this.ButtonKeyFile_Click);
      // 
      // m_TextBoxPassphrase
      // 
      this.m_TableLayoutPanel.SetColumnSpan(this.m_TextBoxPassphrase, 3);
      this.m_TextBoxPassphrase.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TextBoxPassphrase.Location = new System.Drawing.Point(74, 2);
      this.m_TextBoxPassphrase.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_TextBoxPassphrase.Name = "m_TextBoxPassphrase";
      this.m_TextBoxPassphrase.PasswordChar = '*';
      this.m_TextBoxPassphrase.Size = new System.Drawing.Size(530, 20);
      this.m_TextBoxPassphrase.TabIndex = 0;
      // 
      // m_LabelPassphrase
      // 
      this.m_LabelPassphrase.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_LabelPassphrase.AutoSize = true;
      this.m_LabelPassphrase.Location = new System.Drawing.Point(3, 5);
      this.m_LabelPassphrase.Name = "m_LabelPassphrase";
      this.m_LabelPassphrase.Size = new System.Drawing.Size(65, 13);
      this.m_LabelPassphrase.TabIndex = 36;
      this.m_LabelPassphrase.Text = "Passphrase:";
      // 
      // m_LabelKeyFile
      // 
      this.m_LabelKeyFile.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_LabelKeyFile.AutoSize = true;
      this.m_LabelKeyFile.Location = new System.Drawing.Point(21, 31);
      this.m_LabelKeyFile.Name = "m_LabelKeyFile";
      this.m_LabelKeyFile.Size = new System.Drawing.Size(47, 13);
      this.m_LabelKeyFile.TabIndex = 1;
      this.m_LabelKeyFile.Text = "Key File:";
      // 
      // m_CheckBoxShowHide
      // 
      this.m_CheckBoxShowHide.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.m_CheckBoxShowHide.AutoSize = true;
      this.m_CheckBoxShowHide.Location = new System.Drawing.Point(403, 57);
      this.m_CheckBoxShowHide.Margin = new System.Windows.Forms.Padding(2);
      this.m_CheckBoxShowHide.Name = "m_CheckBoxShowHide";
      this.m_CheckBoxShowHide.Size = new System.Drawing.Size(80, 17);
      this.m_CheckBoxShowHide.TabIndex = 4;
      this.m_CheckBoxShowHide.Text = "Show Entry";
      this.m_CheckBoxShowHide.UseVisualStyleBackColor = true;
      this.m_CheckBoxShowHide.CheckedChanged += new System.EventHandler(this.CheckBoxShowHide_CheckedChanged);
      // 
      // FormPasswordAndKey
      // 
      this.AllowDrop = true;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.ClientSize = new System.Drawing.Size(607, 81);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.MinimumSize = new System.Drawing.Size(338, 71);
      this.Name = "FormPasswordAndKey";
      this.ShowIcon = false;
      this.Text = "Security";
      this.TopMost = true;
      this.Shown += new System.EventHandler(this.FormPasswordAndKey_Shown);
      this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FormPasswordAndKey_DragDrop);
      this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FormPasswordAndKey_DragEnter);
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
      this.ResumeLayout(false);

    }

    private void ButtonKeyFile_Click(object sender, EventArgs e)
    {
      try
      {
        var split = FileSystemUtils.SplitPath(m_TextBoxKeyFile.Text);
        var newFileName = WindowsAPICodePackWrapper.Open(
          split.DirectoryName,
          "File with PGP Key",
          "Key file (*.asc;*.key;*.ascii)|*.ascii;*.key;*.asc|All files (*.*)|*.*",
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

    private void CheckBoxShowHide_CheckedChanged(object sender, EventArgs e)
    {
      m_TextBoxPassphrase.PasswordChar = m_CheckBoxShowHide.Checked ? char.MinValue : '*';
    }

    private void FormPasswordAndKey_DragEnter(object sender, DragEventArgs e)
    {
      try
      {
        if (e.Data != null && (e.Data.GetDataPresent(DataFormats.FileDrop, false)
                               || e.Data.GetDataPresent(DataFormats.UnicodeText, true)))
          e.Effect = DragDropEffects.All;
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Drag Drop");
      }
    }

    private void FormPasswordAndKey_DragDrop(object sender, DragEventArgs e)
    {
      try
      {
        var files = (string[]) (e.Data?.GetData(DataFormats.FileDrop) ?? Array.Empty<string>());
        var text = (string[]) (e.Data?.GetData(DataFormats.UnicodeText) ?? Array.Empty<string>());
        if (files.Length <1 && text.Length<1) return;

        if (!WindowsAPICodePackWrapper.IsDialogOpen && files.Length >0)
          m_TextBoxKeyFile.Text = files[0];

        if (text.Length >0)
          m_TextBoxPassphrase.Text = text[0];
      }
      catch (Exception ex)
      {
        this.ShowError(ex, "Drag Drop");
      }
    }

    private void FormPasswordAndKey_Shown(object sender, EventArgs e)
    {
      m_TextBoxPassphrase.Visible  = ShowPassphrase;
      m_LabelPassphrase.Visible  = ShowPassphrase;
      m_CheckBoxShowHide.Visible  = ShowPassphrase;

      m_ButtonKeyFile.Visible  = ShowFileName;
      m_TextBoxKeyFile.Visible = ShowFileName;
      m_LabelKeyFile.Visible = ShowFileName;
    }
  }
}