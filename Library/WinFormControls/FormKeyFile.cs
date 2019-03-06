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
    private TextBox textBox;

    /// <summary>
    /// The text shown in this from
    /// </summary>
    public string KeyBlock
    {
      get => textBox.Text;
      set => textBox.Text = value;
    }

    public FormKeyFile() => InitializeComponent();

    public FormKeyFile(string title, bool privateKey)
    {
      InitializeComponent();
      TextBox_Leave(this, null);
      KeyBlock = title;
      m_PrivateKey = privateKey;
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
        if (_MessageBox.Show(this,
          $"The entered text does not seem to be a valid PGP Key Block File.\nThe text should start with -----BEGIN PGP\nException: \n{message}", "Issue with Key", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
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
      m_BtnOk = new System.Windows.Forms.Button();
      textBox = new System.Windows.Forms.TextBox();
      m_BtnCancel = new System.Windows.Forms.Button();
      m_Label1 = new System.Windows.Forms.Label();
      m_SplitContainer1 = new System.Windows.Forms.SplitContainer();
      ((System.ComponentModel.ISupportInitialize)(m_SplitContainer1)).BeginInit();
      m_SplitContainer1.Panel1.SuspendLayout();
      m_SplitContainer1.Panel2.SuspendLayout();
      m_SplitContainer1.SuspendLayout();
      SuspendLayout();
      // 
      // m_BtnOk
      // 
      m_BtnOk.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
      m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      m_BtnOk.Location = new System.Drawing.Point(474, 2);
      m_BtnOk.Name = "m_BtnOk";
      m_BtnOk.Size = new System.Drawing.Size(61, 23);
      m_BtnOk.TabIndex = 0;
      m_BtnOk.Text = "OK";
      m_BtnOk.Click += new System.EventHandler(BtnOK_Click);
      // 
      // textBox
      // 
      textBox.AcceptsReturn = true;
      textBox.AllowDrop = true;
      textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      textBox.Location = new System.Drawing.Point(0, 0);
      textBox.Multiline = true;
      textBox.Name = "textBox";
      textBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      textBox.Size = new System.Drawing.Size(538, 377);
      textBox.TabIndex = 1;
      textBox.DragDrop += new System.Windows.Forms.DragEventHandler(TextBox_DragDrop);
      textBox.DragEnter += new System.Windows.Forms.DragEventHandler(TextBox_DragEnter);
      textBox.Enter += new System.EventHandler(TextBox_Enter);
      textBox.Leave += new System.EventHandler(TextBox_Leave);
      // 
      // m_BtnCancel
      // 
      m_BtnCancel.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
      m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      m_BtnCancel.Location = new System.Drawing.Point(407, 2);
      m_BtnCancel.Name = "m_BtnCancel";
      m_BtnCancel.Size = new System.Drawing.Size(61, 23);
      m_BtnCancel.TabIndex = 4;
      m_BtnCancel.Text = "Cancel";
      m_BtnCancel.Click += new System.EventHandler(BtnCancel_Click);
      // 
      // m_Label1
      // 
      m_Label1.AutoSize = true;
      m_Label1.Location = new System.Drawing.Point(6, 7);
      m_Label1.Name = "m_Label1";
      m_Label1.Size = new System.Drawing.Size(144, 13);
      m_Label1.TabIndex = 5;
      m_Label1.Text = "(The text is stored encrypted)";
      // 
      // m_SplitContainer1
      // 
      m_SplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
      m_SplitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
      m_SplitContainer1.Location = new System.Drawing.Point(0, 0);
      m_SplitContainer1.Name = "m_SplitContainer1";
      m_SplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
      // 
      // m_SplitContainer1.Panel1
      // 
      m_SplitContainer1.Panel1.Controls.Add(textBox);
      // 
      // m_SplitContainer1.Panel2
      // 
      m_SplitContainer1.Panel2.Controls.Add(m_Label1);
      m_SplitContainer1.Panel2.Controls.Add(m_BtnCancel);
      m_SplitContainer1.Panel2.Controls.Add(m_BtnOk);
      m_SplitContainer1.Size = new System.Drawing.Size(538, 408);
      m_SplitContainer1.SplitterDistance = 377;
      m_SplitContainer1.TabIndex = 6;
      // 
      // FormKeyFile
      // 
      AcceptButton = m_BtnOk;
      AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      CancelButton = m_BtnCancel;
      ClientSize = new System.Drawing.Size(538, 408);
      ControlBox = false;
      Controls.Add(m_SplitContainer1);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      MaximumSize = new System.Drawing.Size(700, 500);
      MinimumSize = new System.Drawing.Size(380, 200);
      Name = "FormKeyFile";
      ShowIcon = false;
      TopMost = true;
      Load += new System.EventHandler(FormPassphrase_Load);
      m_SplitContainer1.Panel1.ResumeLayout(false);
      m_SplitContainer1.Panel1.PerformLayout();
      m_SplitContainer1.Panel2.ResumeLayout(false);
      m_SplitContainer1.Panel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(m_SplitContainer1)).EndInit();
      m_SplitContainer1.ResumeLayout(false);
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
            _MessageBox.Show(this, "The dropped file must be less than 32k Byte", "File too large", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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