namespace CsvTools
{
  partial class FindSkipRows
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        components?.Dispose();
        m_HighLighter.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      var resources = new System.ComponentModel.ComponentResourceManager(typeof(FindSkipRows));
      System.Windows.Forms.Button buttonOk;
      System.Windows.Forms.Button buttonCancel;
      tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      textBoxComment = new System.Windows.Forms.TextBox();
      labelComment = new System.Windows.Forms.Label();
      labelQuote = new System.Windows.Forms.Label();
      labelDelimiter = new System.Windows.Forms.Label();
      textBoxDelimiter = new PunctuationTextBox();
      labelSkip = new System.Windows.Forms.Label();
      textBox = new FastColoredTextBoxNS.FastColoredTextBox();
      textBoxQuote = new PunctuationTextBox();
      numericUpDownSkipRows = new System.Windows.Forms.NumericUpDown();
      labelEscape = new System.Windows.Forms.Label();
      textBoxEscape = new PunctuationTextBox();
      buttonSkipLine = new System.Windows.Forms.Button();
      flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
      buttonOk = new System.Windows.Forms.Button();
      buttonCancel = new System.Windows.Forms.Button();
      tableLayoutPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) textBox).BeginInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownSkipRows).BeginInit();
      flowLayoutPanel1.SuspendLayout();
      SuspendLayout();
      // 
      // tableLayoutPanel
      // 
      tableLayoutPanel.AutoSize = true;
      tableLayoutPanel.ColumnCount = 6;
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel.Controls.Add(textBoxComment, 3, 1);
      tableLayoutPanel.Controls.Add(labelComment, 2, 1);
      tableLayoutPanel.Controls.Add(labelQuote, 0, 1);
      tableLayoutPanel.Controls.Add(labelDelimiter, 2, 0);
      tableLayoutPanel.Controls.Add(textBoxDelimiter, 3, 0);
      tableLayoutPanel.Controls.Add(labelSkip, 0, 0);
      tableLayoutPanel.Controls.Add(textBox, 0, 2);
      tableLayoutPanel.Controls.Add(textBoxQuote, 1, 1);
      tableLayoutPanel.Controls.Add(numericUpDownSkipRows, 1, 0);
      tableLayoutPanel.Controls.Add(labelEscape, 4, 0);
      tableLayoutPanel.Controls.Add(textBoxEscape, 5, 0);
      tableLayoutPanel.Controls.Add(buttonSkipLine, 5, 1);
      tableLayoutPanel.Controls.Add(flowLayoutPanel1, 5, 3);
      tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanel.Name = "tableLayoutPanel";
      tableLayoutPanel.RowCount = 4;
      tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel.Size = new System.Drawing.Size(500, 363);
      tableLayoutPanel.TabIndex = 0;
      // 
      // textBoxComment
      // 
      textBoxComment.Anchor = System.Windows.Forms.AnchorStyles.Left;
      textBoxComment.Location = new System.Drawing.Point(220, 31);
      textBoxComment.MinimumSize = new System.Drawing.Size(46, 4);
      textBoxComment.Name = "textBoxComment";
      textBoxComment.Size = new System.Drawing.Size(46, 20);
      textBoxComment.TabIndex = 131;
      textBoxComment.TextChanged += DifferentSyntaxHighlighter;
      // 
      // labelComment
      // 
      labelComment.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelComment.AutoSize = true;
      labelComment.Location = new System.Drawing.Point(137, 35);
      labelComment.Margin = new System.Windows.Forms.Padding(3);
      labelComment.Name = "labelComment";
      labelComment.Size = new System.Drawing.Size(77, 13);
      labelComment.TabIndex = 130;
      labelComment.Text = "Line Comment:";
      // 
      // labelQuote
      // 
      labelQuote.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelQuote.AutoSize = true;
      labelQuote.Location = new System.Drawing.Point(12, 35);
      labelQuote.Margin = new System.Windows.Forms.Padding(3);
      labelQuote.Name = "labelQuote";
      labelQuote.Size = new System.Drawing.Size(72, 13);
      labelQuote.TabIndex = 127;
      labelQuote.Text = "Text Qualifier:";
      // 
      // labelDelimiter
      // 
      labelDelimiter.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelDelimiter.AutoSize = true;
      labelDelimiter.Location = new System.Drawing.Point(164, 6);
      labelDelimiter.Margin = new System.Windows.Forms.Padding(3);
      labelDelimiter.Name = "labelDelimiter";
      labelDelimiter.Size = new System.Drawing.Size(50, 13);
      labelDelimiter.TabIndex = 124;
      labelDelimiter.Text = "Delimiter:";
      // 
      // textBoxDelimiter
      // 
      textBoxDelimiter.Anchor = System.Windows.Forms.AnchorStyles.Left;
      textBoxDelimiter.AutoCompleteCustomSource.AddRange(new string[] { "Tab", ",", ";", "،", "؛", "|", "¦", "￤", "*", "`", "US", "RS", "GS", "FS" });
      textBoxDelimiter.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      textBoxDelimiter.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
      textBoxDelimiter.Location = new System.Drawing.Point(220, 3);
      textBoxDelimiter.MinimumSize = new System.Drawing.Size(46, 4);
      textBoxDelimiter.Name = "textBoxDelimiter";
      textBoxDelimiter.Size = new System.Drawing.Size(46, 20);
      textBoxDelimiter.TabIndex = 123;
      textBoxDelimiter.Type = PunctuationTextBox.PunctuationType.Delimiter;
      textBoxDelimiter.TextChanged += DifferentSyntaxHighlighter;
      // 
      // labelSkip
      // 
      labelSkip.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelSkip.AutoSize = true;
      labelSkip.Location = new System.Drawing.Point(3, 6);
      labelSkip.Margin = new System.Windows.Forms.Padding(3);
      labelSkip.Name = "labelSkip";
      labelSkip.Size = new System.Drawing.Size(81, 13);
      labelSkip.TabIndex = 121;
      labelSkip.Text = "Skip First Lines:";
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
      textBox.AutoSize = true;
      textBox.BackBrush = null;
      textBox.CaretColor = System.Drawing.Color.Silver;
      textBox.CharHeight = 14;
      textBox.CharWidth = 8;
      tableLayoutPanel.SetColumnSpan(textBox, 6);
      textBox.CommentPrefix = "--";
      textBox.Cursor = System.Windows.Forms.Cursors.IBeam;
      textBox.DelayedEventsInterval = 50;
      textBox.DelayedTextChangedInterval = 50;
      textBox.DisabledColor = System.Drawing.Color.FromArgb(  100,   180,   180,   180);
      textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      textBox.Font = new System.Drawing.Font("Courier New", 9.75F);
      textBox.IsReplaceMode = false;
      textBox.Location = new System.Drawing.Point(3, 60);
      textBox.Name = "textBox";
      textBox.Paddings = new System.Windows.Forms.Padding(0);
      textBox.ReadOnly = true;
      textBox.SelectionColor = System.Drawing.Color.FromArgb(  60,   0,   0,   255);
      textBox.Size = new System.Drawing.Size(494, 265);
      textBox.TabIndex = 126;
      textBox.WordWrap = true;
      textBox.Zoom = 100;
      textBox.VisibleRangeChangedDelayed += TextBox_VisibleRangeChangedDelayed;
      // 
      // textBoxQuote
      // 
      textBoxQuote.AutoCompleteCustomSource.AddRange(new string[] { "\\", "/", "?" });
      textBoxQuote.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      textBoxQuote.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
      textBoxQuote.Location = new System.Drawing.Point(90, 28);
      textBoxQuote.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      textBoxQuote.Name = "textBoxQuote";
      textBoxQuote.Size = new System.Drawing.Size(41, 20);
      textBoxQuote.TabIndex = 129;
      textBoxQuote.Type = PunctuationTextBox.PunctuationType.Escape;
      textBoxQuote.TextChanged += DifferentSyntaxHighlighter;
      // 
      // numericUpDownSkipRows
      // 
      numericUpDownSkipRows.AutoSize = true;
      numericUpDownSkipRows.Location = new System.Drawing.Point(90, 3);
      numericUpDownSkipRows.Name = "numericUpDownSkipRows";
      numericUpDownSkipRows.Size = new System.Drawing.Size(41, 20);
      numericUpDownSkipRows.TabIndex = 132;
      numericUpDownSkipRows.ValueChanged += NumericUpDownSkipRows_ValueChanged;
      // 
      // labelEscape
      // 
      labelEscape.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelEscape.AutoSize = true;
      labelEscape.Location = new System.Drawing.Point(267, 6);
      labelEscape.Margin = new System.Windows.Forms.Padding(3);
      labelEscape.Name = "labelEscape";
      labelEscape.Size = new System.Drawing.Size(46, 13);
      labelEscape.TabIndex = 124;
      labelEscape.Text = "Escape:";
      // 
      // textBoxEscape
      // 
      textBoxEscape.Anchor = System.Windows.Forms.AnchorStyles.Left;
      textBoxEscape.AutoCompleteCustomSource.AddRange(new string[] { "\\", "/", "?" });
      textBoxEscape.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      textBoxEscape.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
      textBoxEscape.Location = new System.Drawing.Point(318, 3);
      textBoxEscape.Margin = new System.Windows.Forms.Padding(2);
      textBoxEscape.MinimumSize = new System.Drawing.Size(46, 4);
      textBoxEscape.Name = "textBoxEscape";
      textBoxEscape.Size = new System.Drawing.Size(46, 20);
      textBoxEscape.TabIndex = 123;
      textBoxEscape.Type = PunctuationTextBox.PunctuationType.Escape;
      textBoxEscape.TextChanged += DifferentSyntaxHighlighter;
      // 
      // buttonSkipLine
      // 
      buttonSkipLine.AutoSize = true;
      buttonSkipLine.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      buttonSkipLine.Location = new System.Drawing.Point(319, 29);
      buttonSkipLine.Name = "buttonSkipLine";
      buttonSkipLine.Size = new System.Drawing.Size(97, 25);
      buttonSkipLine.TabIndex = 125;
      buttonSkipLine.Text = "Guess Start Row";
      buttonSkipLine.UseVisualStyleBackColor = true;
      buttonSkipLine.Click += ButtonSkipLine_Click;
      // 
      // flowLayoutPanel1
      // 
      flowLayoutPanel1.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      flowLayoutPanel1.AutoSize = true;
      flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      flowLayoutPanel1.Controls.Add(buttonOk);
      flowLayoutPanel1.Controls.Add(buttonCancel);
      flowLayoutPanel1.Location = new System.Drawing.Point(385, 331);
      flowLayoutPanel1.Name = "flowLayoutPanel1";
      flowLayoutPanel1.Size = new System.Drawing.Size(112, 29);
      flowLayoutPanel1.TabIndex = 133;
      // 
      // buttonOk
      // 
      buttonOk.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      buttonOk.AutoSize = true;
      buttonOk.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      buttonOk.Location = new System.Drawing.Point(3, 3);
      buttonOk.MinimumSize = new System.Drawing.Size(50, 0);
      buttonOk.Name = "buttonOk";
      buttonOk.Size = new System.Drawing.Size(50, 23);
      buttonOk.TabIndex = 0;
      buttonOk.Text = "&OK";
      buttonOk.UseVisualStyleBackColor = true;
      // 
      // buttonCancel
      // 
      buttonCancel.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      buttonCancel.AutoSize = true;
      buttonCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      buttonCancel.Location = new System.Drawing.Point(59, 3);
      buttonCancel.Name = "buttonCancel";
      buttonCancel.Size = new System.Drawing.Size(50, 23);
      buttonCancel.TabIndex = 1;
      buttonCancel.Text = "&Cancel";
      buttonCancel.UseVisualStyleBackColor = true;
      // 
      // FindSkipRows
      // 
      AcceptButton = buttonOk;
      AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      CancelButton = buttonCancel;
      ClientSize = new System.Drawing.Size(500, 363);
      Controls.Add(tableLayoutPanel);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      Name = "FindSkipRows";
      ShowIcon = false;
      ShowInTaskbar = false;
      Text = "Skip Rows Interactive";
      Load += FindSkipRows_Load;
      tableLayoutPanel.ResumeLayout(false);
      tableLayoutPanel.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) textBox).EndInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownSkipRows).EndInit();
      flowLayoutPanel1.ResumeLayout(false);
      flowLayoutPanel1.PerformLayout();
      ResumeLayout(false);
      PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
    private System.Windows.Forms.Label labelSkip;
    private System.Windows.Forms.Label labelDelimiter;
    private PunctuationTextBox textBoxDelimiter;
    private System.Windows.Forms.Button buttonSkipLine;
    private System.Windows.Forms.Label labelQuote;
    private FastColoredTextBoxNS.FastColoredTextBox textBox;
    private PunctuationTextBox textBoxQuote;
    private System.Windows.Forms.Label labelComment;
    private System.Windows.Forms.TextBox textBoxComment;
    private System.Windows.Forms.NumericUpDown numericUpDownSkipRows;
    private System.Windows.Forms.Label labelEscape;
    private PunctuationTextBox textBoxEscape;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    private System.Windows.Forms.Button buttonOk;
  }
}