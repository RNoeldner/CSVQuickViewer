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
      m_BtnOk = new Button();
      m_TableLayoutPanel = new TableLayoutPanel();
      m_BtnCancel = new Button();
      m_TextBoxKeyFile = new TextBox();
      m_ButtonKeyFile = new Button();
      m_TextBoxPassphrase = new TextBox();
      m_LabelPassphrase = new Label();
      m_LabelKeyFile = new Label();
      m_CheckBoxShowHide = new CheckBox();
      m_TableLayoutPanel.SuspendLayout();
      SuspendLayout();
      // 
      // m_BtnOk
      // 
      m_BtnOk.AutoSize = true;
      m_BtnOk.DialogResult = DialogResult.OK;
      m_BtnOk.Location = new System.Drawing.Point(713, 74);
      m_BtnOk.Name = "m_BtnOk";
      m_BtnOk.Size = new System.Drawing.Size(86, 36);
      m_BtnOk.TabIndex = 5;
      m_BtnOk.Text = "&OK";
      m_BtnOk.Click += BtnOK_Click;
      // 
      // m_TableLayoutPanel
      // 
      m_TableLayoutPanel.ColumnCount = 4;
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.Controls.Add(m_BtnCancel, 3, 2);
      m_TableLayoutPanel.Controls.Add(m_BtnOk, 2, 2);
      m_TableLayoutPanel.Controls.Add(m_TextBoxKeyFile, 1, 1);
      m_TableLayoutPanel.Controls.Add(m_ButtonKeyFile, 2, 1);
      m_TableLayoutPanel.Controls.Add(m_TextBoxPassphrase, 1, 0);
      m_TableLayoutPanel.Controls.Add(m_LabelPassphrase, 0, 0);
      m_TableLayoutPanel.Controls.Add(m_LabelKeyFile, 0, 1);
      m_TableLayoutPanel.Controls.Add(m_CheckBoxShowHide, 1, 2);
      m_TableLayoutPanel.Dock = DockStyle.Fill;
      m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      m_TableLayoutPanel.Margin = new Padding(4, 3, 4, 3);
      m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      m_TableLayoutPanel.RowCount = 3;
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.Size = new System.Drawing.Size(910, 125);
      m_TableLayoutPanel.TabIndex = 0;
      // 
      // m_BtnCancel
      // 
      m_BtnCancel.AutoSize = true;
      m_BtnCancel.DialogResult = DialogResult.Cancel;
      m_BtnCancel.Location = new System.Drawing.Point(805, 74);
      m_BtnCancel.Name = "m_BtnCancel";
      m_BtnCancel.Size = new System.Drawing.Size(102, 36);
      m_BtnCancel.TabIndex = 6;
      m_BtnCancel.Text = "&Cancel";
      m_BtnCancel.Click += BtnCancel_Click;
      // 
      // m_TextBoxKeyFile
      // 
      m_TextBoxKeyFile.AutoCompleteMode = AutoCompleteMode.Suggest;
      m_TextBoxKeyFile.AutoCompleteSource = AutoCompleteSource.FileSystem;
      m_TextBoxKeyFile.Dock = DockStyle.Top;
      m_TextBoxKeyFile.Location = new System.Drawing.Point(108, 35);
      m_TextBoxKeyFile.MinimumSize = new System.Drawing.Size(67, 4);
      m_TextBoxKeyFile.Name = "m_TextBoxKeyFile";
      m_TextBoxKeyFile.Size = new System.Drawing.Size(599, 26);
      m_TextBoxKeyFile.TabIndex = 2;
      // 
      // m_ButtonKeyFile
      // 
      m_ButtonKeyFile.AutoSize = true;
      m_TableLayoutPanel.SetColumnSpan(m_ButtonKeyFile, 2);
      m_ButtonKeyFile.Location = new System.Drawing.Point(713, 35);
      m_ButtonKeyFile.Name = "m_ButtonKeyFile";
      m_ButtonKeyFile.Size = new System.Drawing.Size(193, 33);
      m_ButtonKeyFile.TabIndex = 3;
      m_ButtonKeyFile.Text = "Select File";
      m_ButtonKeyFile.UseVisualStyleBackColor = true;
      m_ButtonKeyFile.Click += ButtonKeyFile_Click;
      // 
      // m_TextBoxPassphrase
      // 
      m_TableLayoutPanel.SetColumnSpan(m_TextBoxPassphrase, 3);
      m_TextBoxPassphrase.Dock = DockStyle.Top;
      m_TextBoxPassphrase.Location = new System.Drawing.Point(109, 3);
      m_TextBoxPassphrase.Margin = new Padding(4, 3, 4, 3);
      m_TextBoxPassphrase.Name = "m_TextBoxPassphrase";
      m_TextBoxPassphrase.PasswordChar = '*';
      m_TextBoxPassphrase.Size = new System.Drawing.Size(797, 26);
      m_TextBoxPassphrase.TabIndex = 0;
      // 
      // m_LabelPassphrase
      // 
      m_LabelPassphrase.Anchor = AnchorStyles.Right;
      m_LabelPassphrase.AutoSize = true;
      m_LabelPassphrase.Location = new System.Drawing.Point(4, 6);
      m_LabelPassphrase.Margin = new Padding(4, 0, 4, 0);
      m_LabelPassphrase.Name = "m_LabelPassphrase";
      m_LabelPassphrase.Size = new System.Drawing.Size(97, 20);
      m_LabelPassphrase.TabIndex = 36;
      m_LabelPassphrase.Text = "Passphrase:";
      // 
      // m_LabelKeyFile
      // 
      m_LabelKeyFile.Anchor = AnchorStyles.Right;
      m_LabelKeyFile.AutoSize = true;
      m_LabelKeyFile.Location = new System.Drawing.Point(33, 41);
      m_LabelKeyFile.Margin = new Padding(4, 0, 4, 0);
      m_LabelKeyFile.Name = "m_LabelKeyFile";
      m_LabelKeyFile.Size = new System.Drawing.Size(68, 20);
      m_LabelKeyFile.TabIndex = 1;
      m_LabelKeyFile.Text = "Key File:";
      // 
      // m_CheckBoxShowHide
      // 
      m_CheckBoxShowHide.Anchor = AnchorStyles.Right;
      m_CheckBoxShowHide.AutoSize = true;
      m_CheckBoxShowHide.Location = new System.Drawing.Point(591, 86);
      m_CheckBoxShowHide.Name = "m_CheckBoxShowHide";
      m_CheckBoxShowHide.Size = new System.Drawing.Size(116, 24);
      m_CheckBoxShowHide.TabIndex = 4;
      m_CheckBoxShowHide.Text = "Show Entry";
      m_CheckBoxShowHide.UseVisualStyleBackColor = true;
      m_CheckBoxShowHide.CheckedChanged += CheckBoxShowHide_CheckedChanged;
      // 
      // FormPasswordAndKey
      // 
      AllowDrop = true;
      AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      AutoScaleMode = AutoScaleMode.Font;
      AutoSize = true;
      ClientSize = new System.Drawing.Size(910, 125);
      Controls.Add(m_TableLayoutPanel);
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      Margin = new Padding(4, 3, 4, 3);
      MinimumSize = new System.Drawing.Size(496, 79);
      Name = "FormPasswordAndKey";
      ShowIcon = false;
      Text = "Security";
      TopMost = true;
      Shown += FormPasswordAndKey_Shown;
      DragDrop += FormPasswordAndKey_DragDrop;
      DragEnter += FormPasswordAndKey_DragEnter;
      m_TableLayoutPanel.ResumeLayout(false);
      m_TableLayoutPanel.PerformLayout();
      ResumeLayout(false);

    }

    private void ButtonKeyFile_Click(object? sender, EventArgs e)
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

    private void CheckBoxShowHide_CheckedChanged(object? sender, EventArgs e)
    {
      m_TextBoxPassphrase.PasswordChar = m_CheckBoxShowHide.Checked ? char.MinValue : '*';
    }

    private void FormPasswordAndKey_DragEnter(object? sender, DragEventArgs e)
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

    private void FormPasswordAndKey_DragDrop(object? sender, DragEventArgs e)
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

    private void FormPasswordAndKey_Shown(object? sender, EventArgs e)
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
