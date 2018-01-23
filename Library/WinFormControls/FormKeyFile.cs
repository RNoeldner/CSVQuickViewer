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

using Pri.LongPath;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  public sealed class FormKeyFile : Form
  {
    private const string c_Default =
      "-----BEGIN PGP PRIVATE KEY BLOCK-----\r\nVersion: GnuPG v2\r\n\r\n.....\r\nPlease copy the text\r\nor drag and drop file.\r\n.....\r\n\r\n-----END PGP PRIVATE KEY BLOCK-----";

    private readonly bool m_PrivateKey = true;
    private Button m_BtnCancel;
    private Button m_BtnOk;
    private Label m_Label1;
    private SplitContainer m_SplitContainer1;
    public TextBox textBox;

    public FormKeyFile()
    {
      InitializeComponent();
    }

    public FormKeyFile(string title, bool privateKey)
    {
      InitializeComponent();
      TextBox_Leave(this, null);
      Text = title;
      m_PrivateKey = privateKey;
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
      var isPgpKeyRingBundle = PGPKeyStorage.IsValidKeyRingBundle(textBox.Text, m_PrivateKey);

      if (!isPgpKeyRingBundle)
      {
        MessageBox.Show(
          "The entered text does not seem to be a valid PGP Key Block File.\nThe text should start with -----BEGIN PGP");
      }
      else
      {
        DialogResult = DialogResult.OK;
        Close();
      }
    }

    private void FormPassphrase_Load(object sender, EventArgs e)
    {
      textBox.Focus();
    }

    /// <summary>
    ///   Required method for Designer support - do not modify
    ///   the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.m_BtnOk = new System.Windows.Forms.Button();
      this.textBox = new System.Windows.Forms.TextBox();
      this.m_BtnCancel = new System.Windows.Forms.Button();
      this.m_Label1 = new System.Windows.Forms.Label();
      this.m_SplitContainer1 = new System.Windows.Forms.SplitContainer();
      ((System.ComponentModel.ISupportInitialize)(this.m_SplitContainer1)).BeginInit();
      this.m_SplitContainer1.Panel1.SuspendLayout();
      this.m_SplitContainer1.Panel2.SuspendLayout();
      this.m_SplitContainer1.SuspendLayout();
      this.SuspendLayout();
      //
      // m_BtnOk
      //
      this.m_BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.m_BtnOk.Location = new System.Drawing.Point(474, 2);
      this.m_BtnOk.Name = "m_BtnOk";
      this.m_BtnOk.Size = new System.Drawing.Size(61, 23);
      this.m_BtnOk.TabIndex = 0;
      this.m_BtnOk.Text = "OK";
      this.m_BtnOk.Click += new System.EventHandler(this.BtnOK_Click);
      //
      // textBox
      //
      this.textBox.AcceptsReturn = true;
      this.textBox.AllowDrop = true;
      this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox.Location = new System.Drawing.Point(0, 0);
      this.textBox.Multiline = true;
      this.textBox.Name = "textBox";
      this.textBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.textBox.Size = new System.Drawing.Size(538, 377);
      this.textBox.TabIndex = 1;
      this.textBox.Enter += new System.EventHandler(this.TextBox_Enter);
      this.textBox.Leave += new System.EventHandler(this.TextBox_Leave);
      //
      // m_BtnCancel
      //
      this.m_BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_BtnCancel.Location = new System.Drawing.Point(407, 2);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new System.Drawing.Size(61, 23);
      this.m_BtnCancel.TabIndex = 4;
      this.m_BtnCancel.Text = "Cancel";
      this.m_BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
      //
      // m_Label1
      //
      this.m_Label1.AutoSize = true;
      this.m_Label1.Location = new System.Drawing.Point(6, 7);
      this.m_Label1.Name = "m_Label1";
      this.m_Label1.Size = new System.Drawing.Size(144, 13);
      this.m_Label1.TabIndex = 5;
      this.m_Label1.Text = "(The text is stored encrypted)";
      //
      // m_SplitContainer1
      //
      this.m_SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      this.m_SplitContainer1.Location = new System.Drawing.Point(0, 0);
      this.m_SplitContainer1.Name = "m_SplitContainer1";
      this.m_SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      //
      // m_SplitContainer1.Panel1
      //
      this.m_SplitContainer1.Panel1.Controls.Add(this.textBox);
      //
      // m_SplitContainer1.Panel2
      //
      this.m_SplitContainer1.Panel2.Controls.Add(this.m_Label1);
      this.m_SplitContainer1.Panel2.Controls.Add(this.m_BtnCancel);
      this.m_SplitContainer1.Panel2.Controls.Add(this.m_BtnOk);
      this.m_SplitContainer1.Size = new System.Drawing.Size(538, 408);
      this.m_SplitContainer1.SplitterDistance = 377;
      this.m_SplitContainer1.TabIndex = 6;
      //
      // FormKeyFile
      //
      this.AcceptButton = this.m_BtnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.m_BtnCancel;
      this.ClientSize = new System.Drawing.Size(538, 408);
      this.ControlBox = false;
      this.Controls.Add(this.m_SplitContainer1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximumSize = new System.Drawing.Size(700, 500);
      this.MinimumSize = new System.Drawing.Size(380, 200);
      this.Name = "FormKeyFile";
      this.ShowIcon = false;
      this.Text = "PGP Key File";
      this.TopMost = true;
      this.m_SplitContainer1.Panel1.ResumeLayout(false);
      this.m_SplitContainer1.Panel1.PerformLayout();
      this.m_SplitContainer1.Panel2.ResumeLayout(false);
      this.m_SplitContainer1.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.m_SplitContainer1)).EndInit();
      this.m_SplitContainer1.ResumeLayout(false);
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
            MessageBox.Show(this, "The dropped file must be less than 32k Byte", "File too large", MessageBoxButtons.OK,
              MessageBoxIcon.Information);
            continue;
          }

          textBox.Text = File.ReadAllText(absoluteFile);
          break;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, ex.ExceptionMessages(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        textBox.Text = "";
      textBox.ForeColor = SystemColors.ControlText;
    }

    private void TextBox_Leave(object sender, EventArgs e)
    {
      if (!string.IsNullOrEmpty(textBox.Text)) return;
      textBox.Text = c_Default;
      textBox.ForeColor = SystemColors.GrayText;
    }
  }
}