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
      this.textBox = new FastColoredTextBoxNS.FastColoredTextBox();
      this.radioButton1 = new System.Windows.Forms.RadioButton();
      this.radioButton2 = new System.Windows.Forms.RadioButton();
      this.radioButton3 = new System.Windows.Forms.RadioButton();
      this.radioButton4 = new System.Windows.Forms.RadioButton();
      this.webBrowser = new System.Windows.Forms.WebBrowser();
      this.fastColoredTextBoxRO = new FastColoredTextBoxNS.FastColoredTextBox();
      ((System.ComponentModel.ISupportInitialize)(this.textBox)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBoxRO)).BeginInit();
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
      this.textBox.IsReplaceMode = false;
      this.textBox.Location = new System.Drawing.Point(0, 0);
      this.textBox.Margin = new System.Windows.Forms.Padding(2);
      this.textBox.Name = "textBox";
      this.textBox.Paddings = new System.Windows.Forms.Padding(0);
      this.textBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.textBox.ShowFoldingLines = true;
      this.textBox.Size = new System.Drawing.Size(745, 432);
      this.textBox.TabIndex = 1;
      this.textBox.WordWrap = true;
      this.textBox.Zoom = 100;
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
      // webBrowser
      // 
      this.webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
      this.webBrowser.Location = new System.Drawing.Point(0, 0);
      this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
      this.webBrowser.Name = "webBrowser";
      this.webBrowser.Size = new System.Drawing.Size(745, 432);
      this.webBrowser.TabIndex = 6;
      // 
      // fastColoredTextBoxRO
      // 
      this.fastColoredTextBoxRO.AutoCompleteBracketsList = new char[] {
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
      this.fastColoredTextBoxRO.AutoIndentCharsPatterns = "\r\n^\\s*[\\w\\.]+(\\s\\w+)?\\s*(?<range>=)\\s*(?<range>.+)\r\n";
      this.fastColoredTextBoxRO.AutoScrollMinSize = new System.Drawing.Size(0, 16);
      this.fastColoredTextBoxRO.BackBrush = null;
      this.fastColoredTextBoxRO.CaretColor = System.Drawing.Color.Silver;
      this.fastColoredTextBoxRO.CharHeight = 16;
      this.fastColoredTextBoxRO.CharWidth = 9;
      this.fastColoredTextBoxRO.CommentPrefix = "--";
      this.fastColoredTextBoxRO.Cursor = System.Windows.Forms.Cursors.IBeam;
      this.fastColoredTextBoxRO.DelayedEventsInterval = 50;
      this.fastColoredTextBoxRO.DelayedTextChangedInterval = 50;
      this.fastColoredTextBoxRO.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
      this.fastColoredTextBoxRO.Dock = System.Windows.Forms.DockStyle.Fill;
      this.fastColoredTextBoxRO.IsReplaceMode = false;
      this.fastColoredTextBoxRO.Location = new System.Drawing.Point(0, 0);
      this.fastColoredTextBoxRO.Margin = new System.Windows.Forms.Padding(2);
      this.fastColoredTextBoxRO.Name = "fastColoredTextBoxRO";
      this.fastColoredTextBoxRO.Paddings = new System.Windows.Forms.Padding(0);
      this.fastColoredTextBoxRO.ReadOnly = true;
      this.fastColoredTextBoxRO.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
      this.fastColoredTextBoxRO.ShowFoldingLines = true;
      this.fastColoredTextBoxRO.Size = new System.Drawing.Size(745, 432);
      this.fastColoredTextBoxRO.TabIndex = 2;
      this.fastColoredTextBoxRO.WordWrap = true;
      this.fastColoredTextBoxRO.Zoom = 100;
      this.fastColoredTextBoxRO.TextChangedDelayed += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.TextBox_TextChangedDelayed);
      this.fastColoredTextBoxRO.VisibleRangeChangedDelayed += new System.EventHandler(this.TextBox_VisibleRangeChangedDelayed);
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
      this.Controls.Add(this.fastColoredTextBoxRO);
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "FormTextDisplay";
      this.Shown += new System.EventHandler(this.FormTextDisplay_Shown);
      ((System.ComponentModel.ISupportInitialize)(this.textBox)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fastColoredTextBoxRO)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }
    #endregion

    private FastColoredTextBoxNS.FastColoredTextBox textBox;
    private System.Windows.Forms.RadioButton radioButton1;
    private System.Windows.Forms.RadioButton radioButton2;
    private System.Windows.Forms.RadioButton radioButton3;
    private System.Windows.Forms.RadioButton radioButton4;
    private System.Windows.Forms.WebBrowser webBrowser;
    private FastColoredTextBoxNS.FastColoredTextBox fastColoredTextBoxRO;
  }
}
