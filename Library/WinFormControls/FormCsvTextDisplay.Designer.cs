using System;

namespace CsvTools
{
  partial class FormCsvTextDisplay
  {

    /// <summary>
    /// The components
    /// </summary>
    private System.ComponentModel.IContainer components = null;


    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (m_HighLighter is IDisposable disposable)
          disposable.Dispose();
        components?.Dispose();
      }
      base.Dispose(disposing);
    }

    #region

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      var resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCsvTextDisplay));
      textBox = new FastColoredTextBoxNS.FastColoredTextBox();
      contextMenuJson = new System.Windows.Forms.ContextMenuStrip(components);
      prettyPrintJsonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      originalFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      ((System.ComponentModel.ISupportInitialize) textBox).BeginInit();
      contextMenuJson.SuspendLayout();
      SuspendLayout();
      // 
      // textBox
      // 
      textBox.AllowDrop = false;
      textBox.AutoCompleteBracketsList = new char[]
  {
    '(',
    ')',
    '{',
    '}',
    '[',
    ']',
    '"',
    '"',
    '\'',
    '\''
  };
      textBox.AutoIndentCharsPatterns = "\r\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>.+)\r\n";
      textBox.AutoScrollMinSize = new System.Drawing.Size(0, 14);
      textBox.BackBrush = null;
      textBox.CaretColor = System.Drawing.Color.Silver;
      textBox.CharHeight = 14;
      textBox.CharWidth = 8;
      textBox.CommentPrefix = "--";
      textBox.ContextMenuStrip = contextMenuJson;
      textBox.Cursor = System.Windows.Forms.Cursors.IBeam;
      textBox.DelayedEventsInterval = 50;
      textBox.DelayedTextChangedInterval = 50;
      textBox.DisabledColor = System.Drawing.Color.FromArgb(  100,   180,   180,   180);
      textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      textBox.Font = new System.Drawing.Font("Courier New", 9.75F);
      textBox.IsReplaceMode = false;
      textBox.Location = new System.Drawing.Point(0, 0);
      textBox.Margin = new System.Windows.Forms.Padding(2);
      textBox.Name = "textBox";
      textBox.Paddings = new System.Windows.Forms.Padding(0);
      textBox.ReadOnly = true;
      textBox.SelectionColor = System.Drawing.Color.FromArgb(  60,   0,   0,   255);
      textBox.ShowFoldingLines = true;
      textBox.Size = new System.Drawing.Size(718, 362);
      textBox.TabIndex = 1;
      textBox.WordWrap = true;
      textBox.Zoom = 100;
      textBox.TextChangedDelayed += TextBox_TextChangedDelayed;
      textBox.VisibleRangeChangedDelayed += TextBox_VisibleRangeChangedDelayed;
      // 
      // contextMenuJson
      // 
      contextMenuJson.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { prettyPrintJsonToolStripMenuItem, originalFileToolStripMenuItem });
      contextMenuJson.Name = "contextMenuStrip";
      contextMenuJson.Size = new System.Drawing.Size(201, 48);
      // 
      // prettyPrintJsonToolStripMenuItem
      // 
      prettyPrintJsonToolStripMenuItem.Name = "prettyPrintJsonToolStripMenuItem";
      prettyPrintJsonToolStripMenuItem.ShortcutKeys =  System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P;
      prettyPrintJsonToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
      prettyPrintJsonToolStripMenuItem.Text = "&Pretty Print Json";
      prettyPrintJsonToolStripMenuItem.Click += PrettyPrintJsonToolStripMenuItem_Click;
      // 
      // originalFileToolStripMenuItem
      // 
      originalFileToolStripMenuItem.Checked = true;
      originalFileToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
      originalFileToolStripMenuItem.Name = "originalFileToolStripMenuItem";
      originalFileToolStripMenuItem.ShortcutKeys =  System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O;
      originalFileToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
      originalFileToolStripMenuItem.Text = "&Original File";
      originalFileToolStripMenuItem.Click += OriginalFileToolStripMenuItem_Click;
      // 
      // FormCsvTextDisplay
      // 
      BackColor = System.Drawing.SystemColors.ControlLightLight;
      ClientSize = new System.Drawing.Size(718, 362);
      Controls.Add(textBox);
      Margin = new System.Windows.Forms.Padding(2);
      Name = "FormCsvTextDisplay";
      ((System.ComponentModel.ISupportInitialize) textBox).EndInit();
      contextMenuJson.ResumeLayout(false);
      ResumeLayout(false);

    }


    #endregion

    private FastColoredTextBoxNS.FastColoredTextBox textBox;
    private System.Windows.Forms.ContextMenuStrip contextMenuJson;
    private System.Windows.Forms.ToolStripMenuItem prettyPrintJsonToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem originalFileToolStripMenuItem;
  }
}
