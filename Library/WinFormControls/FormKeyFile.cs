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

    private TableLayoutPanel tableLayoutPanel1;

    private TextBox textBox;

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
      get => textBox.Text;
      set => textBox.Text = value;
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
      var isPgpKeyRingBundle = PGPKeyStorage.IsValidKeyRingBundle(textBox.Text, m_PrivateKey, out var message);

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

    private void FormPassphrase_Load(object sender, EventArgs e) => textBox.Focus();

    /// <summary>
    ///   Required method for Designer support - do not modify
    ///   the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.m_BtnOk = new Button();
      this.textBox = new TextBox();
      this.m_BtnCancel = new Button();
      this.m_Label1 = new Label();
      this.tableLayoutPanel1 = new TableLayoutPanel();
      this.tableLayoutPanel1.SuspendLayout();
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
      this.textBox.AcceptsReturn = true;
      this.textBox.AllowDrop = true;
      this.tableLayoutPanel1.SetColumnSpan(this.textBox, 3);
      this.textBox.Dock = DockStyle.Fill;
      this.textBox.Location = new Point(3, 3);
      this.textBox.Multiline = true;
      this.textBox.Name = "textBox";
      this.textBox.ScrollBars = ScrollBars.Both;
      this.textBox.Size = new Size(664, 459);
      this.textBox.TabIndex = 1;
      this.textBox.DragDrop += new DragEventHandler(this.TextBox_DragDrop);
      this.textBox.DragEnter += new DragEventHandler(this.TextBox_DragEnter);
      this.textBox.Enter += new EventHandler(this.TextBox_Enter);
      this.textBox.Leave += new EventHandler(this.TextBox_Leave);

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
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      this.tableLayoutPanel1.Controls.Add(this.textBox, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.m_Label1, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.m_BtnOk, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.m_BtnCancel, 2, 1);
      this.tableLayoutPanel1.Dock = DockStyle.Fill;
      this.tableLayoutPanel1.Location = new Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
      this.tableLayoutPanel1.Size = new Size(670, 505);
      this.tableLayoutPanel1.TabIndex = 7;

      // FormKeyFile
      this.AcceptButton = this.m_BtnOk;
      this.AutoScaleDimensions = new SizeF(9F, 20F);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = this.m_BtnCancel;
      this.ClientSize = new Size(670, 505);
      this.ControlBox = false;
      this.Controls.Add(this.tableLayoutPanel1);
      this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
      this.MaximumSize = new Size(1027, 747);
      this.MinimumSize = new Size(548, 285);
      this.Name = "FormKeyFile";
      this.ShowIcon = false;
      this.TopMost = true;
      this.Load += new EventHandler(this.FormPassphrase_Load);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
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

          textBox.Text = File.ReadAllText(absoluteFile);
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
      if (c_Default == textBox.Text)
        textBox.Text = string.Empty;
      textBox.ForeColor = SystemColors.ControlText;
    }

    private void TextBox_Leave(object sender, EventArgs e)
    {
      if (!string.IsNullOrEmpty(textBox.Text))
        return;
      textBox.Text = c_Default;
      textBox.ForeColor = SystemColors.GrayText;
    }
  }
}