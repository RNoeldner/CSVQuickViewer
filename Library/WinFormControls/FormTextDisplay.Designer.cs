using System;

namespace CsvTools
{
  partial class FormTextDisplay
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormTextDisplay));
      this.textBox = new FastColoredTextBoxNS.FastColoredTextBox();
      this.radioButton1 = new System.Windows.Forms.RadioButton();
      this.radioButton2 = new System.Windows.Forms.RadioButton();
      this.radioButton3 = new System.Windows.Forms.RadioButton();
      this.radioButton4 = new System.Windows.Forms.RadioButton();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
      this.webBrowser = new System.Windows.Forms.WebBrowser();
      ((System.ComponentModel.ISupportInitialize)(this.textBox)).BeginInit();
      this.SuspendLayout();
      // 
      // textBox
      // 
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
      this.textBox.AutoScrollMinSize = new System.Drawing.Size(0, 16);
      this.textBox.BackBrush = null;
      this.textBox.CaretColor = System.Drawing.Color.Silver;
      this.textBox.CharHeight = 16;
      this.textBox.CharWidth = 9;
      this.textBox.CommentPrefix = "--";
      this.textBox.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.textBox.DelayedEventsInterval = 50;
      this.textBox.DelayedTextChangedInterval = 50;
      this.textBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.textBox.Font = new System.Drawing.Font("Courier New", 11.25F);
      this.textBox.IsReplaceMode = false;
      this.textBox.Location = new System.Drawing.Point(0, 0);
      this.textBox.Margin = new System.Windows.Forms.Padding(2);
      this.textBox.Name = "textBox";
      this.textBox.Paddings = new System.Windows.Forms.Padding(0);
      this.textBox.ReadOnly = true;
      this.textBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.textBox.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textBox.ServiceColors")));
      this.textBox.ShowFoldingLines = true;
      this.textBox.Size = new System.Drawing.Size(745, 432);
      this.textBox.TabIndex = 1;
      this.textBox.WordWrap = true;
      this.textBox.Zoom = 100;
      this.textBox.TextChangedDelayed += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.TextBox_TextChangedDelayed);
      this.textBox.VisibleRangeChangedDelayed += new System.EventHandler(this.TextBox_VisibleRangeChangedDelayed);
      // 
      // radioButton1
      // 
      this.radioButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.radioButton1.AutoSize = true;
      this.radioButton1.BackColor = System.Drawing.SystemColors.Info;
      this.radioButton1.Checked = true;
      this.radioButton1.ForeColor = System.Drawing.SystemColors.InfoText;
      this.radioButton1.Location = new System.Drawing.Point(551, 413);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(46, 17);
      this.radioButton1.TabIndex = 2;
      this.radioButton1.TabStop = true;
      this.radioButton1.Text = "&Text";
      this.radioButton1.UseVisualStyleBackColor = false;
      this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
      // 
      // radioButton2
      // 
      this.radioButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.radioButton2.AutoSize = true;
      this.radioButton2.BackColor = System.Drawing.SystemColors.Info;
      this.radioButton2.ForeColor = System.Drawing.SystemColors.InfoText;
      this.radioButton2.Location = new System.Drawing.Point(597, 413);
      this.radioButton2.Name = "radioButton2";
      this.radioButton2.Size = new System.Drawing.Size(47, 17);
      this.radioButton2.TabIndex = 3;
      this.radioButton2.Text = "&Json";
      this.radioButton2.UseVisualStyleBackColor = false;
      this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
      // 
      // radioButton3
      // 
      this.radioButton3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.radioButton3.AutoSize = true;
      this.radioButton3.BackColor = System.Drawing.SystemColors.Info;
      this.radioButton3.ForeColor = System.Drawing.SystemColors.InfoText;
      this.radioButton3.Location = new System.Drawing.Point(644, 413);
      this.radioButton3.Name = "radioButton3";
      this.radioButton3.Size = new System.Drawing.Size(42, 17);
      this.radioButton3.TabIndex = 4;
      this.radioButton3.Text = "&Xml";
      this.radioButton3.UseVisualStyleBackColor = false;
      this.radioButton3.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
      // 
      // radioButton4
      // 
      this.radioButton4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.radioButton4.AutoSize = true;
      this.radioButton4.BackColor = System.Drawing.SystemColors.Info;
      this.radioButton4.ForeColor = System.Drawing.SystemColors.InfoText;
      this.radioButton4.Location = new System.Drawing.Point(686, 413);
      this.radioButton4.Name = "radioButton4";
      this.radioButton4.Size = new System.Drawing.Size(46, 17);
      this.radioButton4.TabIndex = 5;
      this.radioButton4.Text = "&Html";
      this.radioButton4.UseVisualStyleBackColor = false;
      this.radioButton4.CheckedChanged += new System.EventHandler(this.radioButton4_CheckedChanged);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(32, 19);
      // 
      // toolStripMenuItem2
      // 
      this.toolStripMenuItem2.Name = "toolStripMenuItem2";
      this.toolStripMenuItem2.Size = new System.Drawing.Size(32, 19);
      // 
      // toolStripMenuItem3
      // 
      this.toolStripMenuItem3.Name = "toolStripMenuItem3";
      this.toolStripMenuItem3.Size = new System.Drawing.Size(32, 19);
      // 
      // toolStripMenuItem4
      // 
      this.toolStripMenuItem4.Name = "toolStripMenuItem4";
      this.toolStripMenuItem4.Size = new System.Drawing.Size(32, 19);
      // 
      // webBrowser
      // 
      this.webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
      this.webBrowser.Location = new System.Drawing.Point(0, 0);
      this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
      this.webBrowser.Name = "webBrowser";
      this.webBrowser.Size = new System.Drawing.Size(745, 432);
      this.webBrowser.TabIndex = 6;
      // 
      // FormTextDisplay
      // 
      this.ClientSize = new System.Drawing.Size(745, 432);
      this.Controls.Add(this.radioButton4);
      this.Controls.Add(this.radioButton3);
      this.Controls.Add(this.radioButton2);
      this.Controls.Add(this.radioButton1);
      this.Controls.Add(this.textBox);
      this.Controls.Add(this.webBrowser);
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "FormTextDisplay";
      this.Shown += new System.EventHandler(this.FormTextDisplay_Shown);
      ((System.ComponentModel.ISupportInitialize)(this.textBox)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    private FastColoredTextBoxNS.FastColoredTextBox textBox;
    private System.Windows.Forms.RadioButton radioButton1;
    private System.Windows.Forms.RadioButton radioButton2;
    private System.Windows.Forms.RadioButton radioButton3;
    private System.Windows.Forms.RadioButton radioButton4;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem3;
    private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
    private System.Windows.Forms.WebBrowser webBrowser;
  }
}
