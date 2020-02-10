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
  using System.Drawing;
  using System.Windows.Forms;

  using Pri.LongPath;

  public sealed class FormKeyFile : Form
  {
    private const string c_Default =
      "-----BEGIN PGP PRIVATE KEY BLOCK-----\r\nVersion: GnuPG v2\r\n\r\n.....\r\nPlease copy the text\r\nor drag and drop file.\r\n.....\r\n\r\n-----END PGP PRIVATE KEY BLOCK-----";

    private readonly bool m_PrivateKey = true;

    private Button m_BtnCancel;

    private Button m_BtnOk;

    private Label m_Label1;

    private TableLayoutPanel m_TableLayoutPanel1;

    private TextBox m_TextBox;

    public FormKeyFile() => InitializeComponent();

    public FormKeyFile(string title, bool privateKey)
    {
      InitializeComponent();
      TextBox_Leave(this, null);
      KeyBlock = title;
      m_PrivateKey = privateKey;
    }

    /// <summary>
    ///   The text shown in this from
    /// </summary>
    public string KeyBlock
    {
      get => m_TextBox.Text;
      set => m_TextBox.Text = value;
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
      var isPgpKeyRingBundle = PGPKeyStorage.IsValidKeyRingBundle(m_TextBox.Text, m_PrivateKey, out var message);

      if (!isPgpKeyRingBundle)
      {
        if (_MessageBox.Show(
              this,
              $"The entered text does not seem to be a valid PGP Key Block File.\nThe text should start with -----BEGIN PGP\nException: \n{message}",
              "Issue with Key",
              MessageBoxButtons.OKCancel) == DialogResult.Cancel)
        {
          return;
        }
      }
      else
      {
        DialogResult = DialogResult.OK;
        Close();
      }
    }

    private void FormPassphrase_Load(object sender, EventArgs e) => m_TextBox.Focus();

    /// <summary>
    ///   Required method for Designer support - do not modify
    ///   the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.m_BtnOk = new Button();
      this.m_TextBox = new TextBox();
      this.m_BtnCancel = new Button();
      this.m_Label1 = new Label();
      this.m_TableLayoutPanel1 = new TableLayoutPanel();
      this.m_TableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();

      // m_BtnOk
      this.m_BtnOk.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
      this.m_BtnOk.AutoSize = true;
      this.m_BtnOk.DialogResult = DialogResult.OK;
      this.m_BtnOk.Location = new Point(457, 468);
      this.m_BtnOk.Name = "m_BtnOk";
      this.m_BtnOk.Size = new Size(102, 34);
      this.m_BtnOk.TabIndex = 0;
      this.m_BtnOk.Text = "&OK";
      this.m_BtnOk.Click += new EventHandler(this.BtnOK_Click);

      // textBox
      this.m_TextBox.AcceptsReturn = true;
      this.m_TextBox.AllowDrop = true;
      this.m_TableLayoutPanel1.SetColumnSpan(this.m_TextBox, 3);
      this.m_TextBox.Dock = DockStyle.Fill;
      this.m_TextBox.Location = new Point(3, 3);
      this.m_TextBox.Multiline = true;
      this.m_TextBox.Name = "textBox";
      this.m_TextBox.ScrollBars = ScrollBars.Both;
      this.m_TextBox.Size = new Size(664, 459);
      this.m_TextBox.TabIndex = 1;
      this.m_TextBox.DragDrop += new DragEventHandler(this.TextBox_DragDrop);
      this.m_TextBox.DragEnter += new DragEventHandler(this.TextBox_DragEnter);
      this.m_TextBox.Enter += new EventHandler(this.TextBox_Enter);
      this.m_TextBox.Leave += new EventHandler(this.TextBox_Leave);

      // m_BtnCancel
      this.m_BtnCancel.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
      this.m_BtnCancel.AutoSize = true;
      this.m_BtnCancel.DialogResult = DialogResult.Cancel;
      this.m_BtnCancel.Location = new Point(565, 468);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new Size(102, 34);
      this.m_BtnCancel.TabIndex = 4;
      this.m_BtnCancel.Text = "&Cancel";
      this.m_BtnCancel.Click += new EventHandler(this.BtnCancel_Click);

      // m_Label1
      this.m_Label1.Anchor = AnchorStyles.Left;
      this.m_Label1.AutoSize = true;
      this.m_Label1.Location = new Point(3, 475);
      this.m_Label1.Name = "m_Label1";
      this.m_Label1.Size = new Size(214, 20);
      this.m_Label1.TabIndex = 5;
      this.m_Label1.Text = "(The text is stored encrypted)";

      // tableLayoutPanel1
      this.m_TableLayoutPanel1.ColumnCount = 3;
      this.m_TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      this.m_TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      this.m_TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      this.m_TableLayoutPanel1.Controls.Add(this.m_TextBox, 0, 0);
      this.m_TableLayoutPanel1.Controls.Add(this.m_Label1, 0, 1);
      this.m_TableLayoutPanel1.Controls.Add(this.m_BtnOk, 1, 1);
      this.m_TableLayoutPanel1.Controls.Add(this.m_BtnCancel, 2, 1);
      this.m_TableLayoutPanel1.Dock = DockStyle.Fill;
      this.m_TableLayoutPanel1.Location = new Point(0, 0);
      this.m_TableLayoutPanel1.Name = "tableLayoutPanel1";
      this.m_TableLayoutPanel1.RowCount = 2;
      this.m_TableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
      this.m_TableLayoutPanel1.RowStyles.Add(new RowStyle());
      this.m_TableLayoutPanel1.Size = new Size(670, 505);
      this.m_TableLayoutPanel1.TabIndex = 7;

      // FormKeyFile
      this.AcceptButton = this.m_BtnOk;
      this.AutoScaleDimensions = new SizeF(9F, 20F);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = this.m_BtnCancel;
      this.ClientSize = new Size(670, 505);
      this.ControlBox = false;
      this.Controls.Add(this.m_TableLayoutPanel1);
      this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
      this.MaximumSize = new Size(1027, 747);
      this.MinimumSize = new Size(548, 285);
      this.Name = "FormKeyFile";
      this.ShowIcon = false;
      this.TopMost = true;
      this.Load += new EventHandler(this.FormPassphrase_Load);
      this.m_TableLayoutPanel1.ResumeLayout(false);
      this.m_TableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);
    }

    private void TextBox_DragDrop(object sender, DragEventArgs e)
    {
      var oldCursor = Cursor.Current == Cursors.WaitCursor ? Cursors.WaitCursor : Cursors.Default;
      try
      {
        TextBox_Enter(sender, null);
        foreach (var absoluteFile in (string[])e.Data.GetData(DataFormats.FileDrop))
        {
          if (string.IsNullOrEmpty(absoluteFile))
            continue;
          if (!File.Exists(absoluteFile))
            continue;
          if (new FileInfo(absoluteFile).Length > 32768)
          {
            _MessageBox.Show(
              this,
              "The dropped file must be less than 32k Byte",
              "File too large",
              MessageBoxButtons.OK,
              MessageBoxIcon.Warning);
            continue;
          }

          m_TextBox.Text = File.ReadAllText(absoluteFile);
          break;
        }
      }
      catch (Exception ex)
      {
        this.ShowError(ex);
      }
      finally
      {
        Cursor.Current = oldCursor;
      }
    }

    private void TextBox_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
        e.Effect = DragDropEffects.All;
    }

    private void TextBox_Enter(object sender, EventArgs e)
    {
      if (c_Default == m_TextBox.Text)
        m_TextBox.Text = string.Empty;
      m_TextBox.ForeColor = SystemColors.ControlText;
    }

    private void TextBox_Leave(object sender, EventArgs e)
    {
      if (!string.IsNullOrEmpty(m_TextBox.Text))
        return;
      m_TextBox.Text = c_Default;
      m_TextBox.ForeColor = SystemColors.GrayText;
    }
  }
}