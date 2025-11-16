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
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCsvTextDisplay));
      this.textBox = new FastColoredTextBoxNS.FastColoredTextBox();
      this.contextMenuJson = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.prettyPrintJsonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.originalFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      ((System.ComponentModel.ISupportInitialize) (this.textBox)).BeginInit();
      this.contextMenuJson.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_HistoryDisplay
      // 
      this.textBox.AllowDrop = false;
      this.textBox.AutoCompleteBracketsList = new char[] {
        '(',
        ')',
        '{',
        '}',
        '[',
        ']',
        '\"',
        '\"',
        '\'',
        '\''};
      this.textBox.AutoIndentCharsPatterns = "\r\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>.+)\r\n";
      this.textBox.AutoScrollMinSize = new System.Drawing.Size(0, 13);
      this.textBox.BackBrush = null;
      this.textBox.CaretColor = System.Drawing.Color.Silver;
      this.textBox.CharHeight = 13;
      this.textBox.CharWidth = 7;
      this.textBox.CommentPrefix = "--";
      this.textBox.ContextMenuStrip = this.contextMenuJson;
      this.textBox.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.textBox.DelayedEventsInterval = 50;
      this.textBox.DelayedTextChangedInterval = 50;
      this.textBox.DisabledColor = System.Drawing.Color.FromArgb(((int) (((byte) (100)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))), ((int) (((byte) (180)))));
      this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox.IsReplaceMode = false;
      this.textBox.Location = new System.Drawing.Point(0, 0);
      this.textBox.Margin = new System.Windows.Forms.Padding(2);
      this.textBox.Name = "textBox";
      this.textBox.Paddings = new System.Windows.Forms.Padding(0);
      this.textBox.ReadOnly = true;
      this.textBox.SelectionColor = System.Drawing.Color.FromArgb(((int) (((byte) (60)))), ((int) (((byte) (0)))), ((int) (((byte) (0)))), ((int) (((byte) (255)))));
      this.textBox.ShowFoldingLines = true;
      this.textBox.Size = new System.Drawing.Size(718, 362);
      this.textBox.TabIndex = 1;
      this.textBox.WordWrap = true;
      this.textBox.Zoom = 100;
      this.textBox.TextChangedDelayed += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.TextBox_TextChangedDelayed);
      this.textBox.VisibleRangeChangedDelayed += new System.EventHandler(this.TextBox_VisibleRangeChangedDelayed);
      // 
      // contextMenuJson
      // 
      this.contextMenuJson.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.prettyPrintJsonToolStripMenuItem,
            this.originalFileToolStripMenuItem});
      this.contextMenuJson.Name = "contextMenuStrip";
      this.contextMenuJson.Size = new System.Drawing.Size(201, 48);
      // 
      // prettyPrintJsonToolStripMenuItem
      // 
      this.prettyPrintJsonToolStripMenuItem.Name = "prettyPrintJsonToolStripMenuItem";
      this.prettyPrintJsonToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
      this.prettyPrintJsonToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
      this.prettyPrintJsonToolStripMenuItem.Text = "&Pretty Print Json";
      this.prettyPrintJsonToolStripMenuItem.Click += new System.EventHandler(this.PrettyPrintJsonToolStripMenuItem_Click);
      // 
      // originalFileToolStripMenuItem
      // 
      this.originalFileToolStripMenuItem.Checked = true;
      this.originalFileToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
      this.originalFileToolStripMenuItem.Name = "originalFileToolStripMenuItem";
      this.originalFileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys) ((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
      this.originalFileToolStripMenuItem.Size = new System.Drawing.Size(200, 22);
      this.originalFileToolStripMenuItem.Text = "&Original File";
      this.originalFileToolStripMenuItem.Click += new System.EventHandler(this.OriginalFileToolStripMenuItem_Click);
      // 
      // FormCsvTextDisplay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.ClientSize = new System.Drawing.Size(718, 362);
      this.Controls.Add(this.textBox);
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "FormCsvTextDisplay";
      ((System.ComponentModel.ISupportInitialize) (this.textBox)).EndInit();
      this.contextMenuJson.ResumeLayout(false);
      this.ResumeLayout(false);

    }


    #endregion

    private FastColoredTextBoxNS.FastColoredTextBox textBox;
    private System.Windows.Forms.ContextMenuStrip contextMenuJson;
    private System.Windows.Forms.ToolStripMenuItem prettyPrintJsonToolStripMenuItem;
    private System.Windows.Forms.ToolStripMenuItem originalFileToolStripMenuItem;
  }
}
