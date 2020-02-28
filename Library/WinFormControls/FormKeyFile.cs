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
  using Pri.LongPath;
  using System;
  using System.Drawing;
  using System.Windows.Forms;

  public sealed class FormKeyFile : ResizeForm
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
      m_BtnOk = new Button();
      m_TextBox = new TextBox();
      m_BtnCancel = new Button();
      m_Label1 = new Label();
      m_TableLayoutPanel1 = new TableLayoutPanel();
      m_TableLayoutPanel1.SuspendLayout();
      SuspendLayout();

      // m_BtnOk
      m_BtnOk.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
      m_BtnOk.AutoSize = true;
      m_BtnOk.DialogResult = DialogResult.OK;
      m_BtnOk.Location = new Point(457, 468);
      m_BtnOk.Name = "m_BtnOk";
      m_BtnOk.Size = new Size(102, 34);
      m_BtnOk.TabIndex = 0;
      m_BtnOk.Text = "&OK";
      m_BtnOk.Click += new EventHandler(BtnOK_Click);

      // textBox
      m_TextBox.AcceptsReturn = true;
      m_TextBox.AllowDrop = true;
      m_TableLayoutPanel1.SetColumnSpan(m_TextBox, 3);
      m_TextBox.Dock = DockStyle.Fill;
      m_TextBox.Location = new Point(3, 3);
      m_TextBox.Multiline = true;
      m_TextBox.Name = "textBox";
      m_TextBox.ScrollBars = ScrollBars.Both;
      m_TextBox.Size = new Size(664, 459);
      m_TextBox.TabIndex = 1;
      m_TextBox.DragDrop += new DragEventHandler(TextBox_DragDrop);
      m_TextBox.DragEnter += new DragEventHandler(TextBox_DragEnter);
      m_TextBox.Enter += new EventHandler(TextBox_Enter);
      m_TextBox.Leave += new EventHandler(TextBox_Leave);

      // m_BtnCancel
      m_BtnCancel.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
      m_BtnCancel.AutoSize = true;
      m_BtnCancel.DialogResult = DialogResult.Cancel;
      m_BtnCancel.Location = new Point(565, 468);
      m_BtnCancel.Name = "m_BtnCancel";
      m_BtnCancel.Size = new Size(102, 34);
      m_BtnCancel.TabIndex = 4;
      m_BtnCancel.Text = "&Cancel";
      m_BtnCancel.Click += new EventHandler(BtnCancel_Click);

      // m_Label1
      m_Label1.Anchor = AnchorStyles.Left;
      m_Label1.AutoSize = true;
      m_Label1.Location = new Point(3, 475);
      m_Label1.Name = "m_Label1";
      m_Label1.Size = new Size(214, 20);
      m_Label1.TabIndex = 5;
      m_Label1.Text = "(The text is stored encrypted)";

      // tableLayoutPanel1
      m_TableLayoutPanel1.ColumnCount = 3;
      m_TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      m_TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel1.Controls.Add(m_TextBox, 0, 0);
      m_TableLayoutPanel1.Controls.Add(m_Label1, 0, 1);
      m_TableLayoutPanel1.Controls.Add(m_BtnOk, 1, 1);
      m_TableLayoutPanel1.Controls.Add(m_BtnCancel, 2, 1);
      m_TableLayoutPanel1.Dock = DockStyle.Fill;
      m_TableLayoutPanel1.Location = new Point(0, 0);
      m_TableLayoutPanel1.Name = "tableLayoutPanel1";
      m_TableLayoutPanel1.RowCount = 2;
      m_TableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
      m_TableLayoutPanel1.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel1.Size = new Size(670, 505);
      m_TableLayoutPanel1.TabIndex = 7;

      // FormKeyFile
      AcceptButton = m_BtnOk;
      AutoScaleDimensions = new SizeF(9F, 20F);
      AutoScaleMode = AutoScaleMode.Font;
      CancelButton = m_BtnCancel;
      ClientSize = new Size(670, 505);
      ControlBox = false;
      Controls.Add(m_TableLayoutPanel1);
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      MaximumSize = new Size(1027, 747);
      MinimumSize = new Size(548, 285);
      Name = "FormKeyFile";
      ShowIcon = false;
      TopMost = true;
      Load += new EventHandler(FormPassphrase_Load);
      m_TableLayoutPanel1.ResumeLayout(false);
      m_TableLayoutPanel1.PerformLayout();
      ResumeLayout(false);
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