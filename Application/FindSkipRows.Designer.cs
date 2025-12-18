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
      tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      textBoxComment = new System.Windows.Forms.TextBox();
      bindingSourceCsvFile = new System.Windows.Forms.BindingSource(components);
      label2 = new System.Windows.Forms.Label();
      m_LabelQuote = new System.Windows.Forms.Label();
      labelDelimiter = new System.Windows.Forms.Label();
      textBoxDelimiter = new PunctuationTextBox();
      label5 = new System.Windows.Forms.Label();
      textBox = new FastColoredTextBoxNS.FastColoredTextBox();
      m_TextBoxQuote = new PunctuationTextBox();
      numericUpDownSkipRows = new System.Windows.Forms.NumericUpDown();
      labelEscape = new System.Windows.Forms.Label();
      textBoxEscape = new PunctuationTextBox();
      buttonSkipLine = new System.Windows.Forms.Button();
      tableLayoutPanel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) bindingSourceCsvFile).BeginInit();
      ((System.ComponentModel.ISupportInitialize) textBox).BeginInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownSkipRows).BeginInit();
      SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      tableLayoutPanel1.AutoSize = true;
      tableLayoutPanel1.ColumnCount = 6;
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.Controls.Add(textBoxComment, 3, 1);
      tableLayoutPanel1.Controls.Add(label2, 2, 1);
      tableLayoutPanel1.Controls.Add(m_LabelQuote, 0, 1);
      tableLayoutPanel1.Controls.Add(labelDelimiter, 2, 0);
      tableLayoutPanel1.Controls.Add(textBoxDelimiter, 3, 0);
      tableLayoutPanel1.Controls.Add(label5, 0, 0);
      tableLayoutPanel1.Controls.Add(textBox, 0, 2);
      tableLayoutPanel1.Controls.Add(m_TextBoxQuote, 1, 1);
      tableLayoutPanel1.Controls.Add(numericUpDownSkipRows, 1, 0);
      tableLayoutPanel1.Controls.Add(labelEscape, 4, 0);
      tableLayoutPanel1.Controls.Add(textBoxEscape, 5, 0);
      tableLayoutPanel1.Controls.Add(buttonSkipLine, 5, 1);
      tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanel1.Name = "tableLayoutPanel1";
      tableLayoutPanel1.RowCount = 3;
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel1.Size = new System.Drawing.Size(616, 551);
      tableLayoutPanel1.TabIndex = 0;
      // 
      // textBoxComment
      // 
      textBoxComment.Anchor = System.Windows.Forms.AnchorStyles.Left;
      textBoxComment.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceCsvFile, "CommentLine", true));
      textBoxComment.Location = new System.Drawing.Point(237, 31);
      textBoxComment.MinimumSize = new System.Drawing.Size(46, 4);
      textBoxComment.Name = "textBoxComment";
      textBoxComment.Size = new System.Drawing.Size(67, 20);
      textBoxComment.TabIndex = 131;
      textBoxComment.TextChanged += DifferentSyntaxHighlighter;
      // 
      // bindingSourceCsvFile
      // 
      bindingSourceCsvFile.AllowNew = false;
      bindingSourceCsvFile.DataSource = typeof(ICsvFile);
      // 
      // label2
      // 
      label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
      label2.AutoSize = true;
      label2.Location = new System.Drawing.Point(154, 35);
      label2.Margin = new System.Windows.Forms.Padding(3);
      label2.Name = "label2";
      label2.Size = new System.Drawing.Size(77, 13);
      label2.TabIndex = 130;
      label2.Text = "Line Comment:";
      // 
      // m_LabelQuote
      // 
      m_LabelQuote.Anchor = System.Windows.Forms.AnchorStyles.Right;
      m_LabelQuote.AutoSize = true;
      m_LabelQuote.Location = new System.Drawing.Point(12, 35);
      m_LabelQuote.Margin = new System.Windows.Forms.Padding(3);
      m_LabelQuote.Name = "m_LabelQuote";
      m_LabelQuote.Size = new System.Drawing.Size(72, 13);
      m_LabelQuote.TabIndex = 127;
      m_LabelQuote.Text = "Text Qualifier:";
      // 
      // labelDelimiter
      // 
      labelDelimiter.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelDelimiter.AutoSize = true;
      labelDelimiter.Location = new System.Drawing.Point(181, 6);
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
      textBoxDelimiter.DataBindings.Add(new System.Windows.Forms.Binding("Character", bindingSourceCsvFile, "FieldDelimiterChar", true));
      textBoxDelimiter.Location = new System.Drawing.Point(236, 3);
      textBoxDelimiter.Margin = new System.Windows.Forms.Padding(3);
      textBoxDelimiter.MinimumSize = new System.Drawing.Size(46, 4);
      textBoxDelimiter.Name = "textBoxDelimiter";
      textBoxDelimiter.Size = new System.Drawing.Size(67, 20);
      textBoxDelimiter.TabIndex = 123;
      textBoxDelimiter.Type = PunctuationTextBox.PunctuationType.Delimiter;
      textBoxDelimiter.TextChanged += DifferentSyntaxHighlighter;
      // 
      // label5
      // 
      label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
      label5.AutoSize = true;
      label5.Location = new System.Drawing.Point(3, 6);
      label5.Margin = new System.Windows.Forms.Padding(3);
      label5.Name = "label5";
      label5.Size = new System.Drawing.Size(81, 13);
      label5.TabIndex = 121;
      label5.Text = "Skip First Lines:";
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
      tableLayoutPanel1.SetColumnSpan(textBox, 6);
      textBox.CommentPrefix = "--";
      textBox.Cursor = System.Windows.Forms.Cursors.IBeam;
      textBox.DelayedEventsInterval = 50;
      textBox.DelayedTextChangedInterval = 50;
      textBox.DisabledColor = System.Drawing.Color.FromArgb(  100,   180,   180,   180);
      textBox.Dock = System.Windows.Forms.DockStyle.Fill;
      textBox.Font = new System.Drawing.Font("Courier New", 9.75F);
      textBox.IsReplaceMode = false;
      textBox.Location = new System.Drawing.Point(2, 59);      
      textBox.Name = "textBox";
      textBox.Paddings = new System.Windows.Forms.Padding(0);
      textBox.ReadOnly = true;
      textBox.SelectionColor = System.Drawing.Color.FromArgb(  60,   0,   0,   255);
      textBox.Size = new System.Drawing.Size(612, 490);
      textBox.TabIndex = 126;
      textBox.WordWrap = true;
      textBox.Zoom = 100;
      textBox.VisibleRangeChangedDelayed += TextBox_VisibleRangeChangedDelayed;
      // 
      // m_TextBoxQuote
      // 
      m_TextBoxQuote.AutoCompleteCustomSource.AddRange(new string[] { "\\", "/", "?" });
      m_TextBoxQuote.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      m_TextBoxQuote.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
      m_TextBoxQuote.DataBindings.Add(new System.Windows.Forms.Binding("Character", bindingSourceCsvFile, "FieldQualifierChar", true));
      m_TextBoxQuote.Location = new System.Drawing.Point(90, 28);
      m_TextBoxQuote.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      m_TextBoxQuote.Name = "m_TextBoxQuote";
      m_TextBoxQuote.Size = new System.Drawing.Size(58, 20);
      m_TextBoxQuote.TabIndex = 129;
      m_TextBoxQuote.Type = PunctuationTextBox.PunctuationType.Escape;
      m_TextBoxQuote.TextChanged += DifferentSyntaxHighlighter;
      // 
      // numericUpDownSkipRows
      // 
      numericUpDownSkipRows.DataBindings.Add(new System.Windows.Forms.Binding("Value", bindingSourceCsvFile, "SkipRows", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      numericUpDownSkipRows.Location = new System.Drawing.Point(90, 3);
      numericUpDownSkipRows.Name = "numericUpDownSkipRows";
      numericUpDownSkipRows.Size = new System.Drawing.Size(58, 20);
      numericUpDownSkipRows.TabIndex = 132;
      numericUpDownSkipRows.ValueChanged += NumericUpDownSkipRows_ValueChanged;
      // 
      // labelEscape
      // 
      labelEscape.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelEscape.AutoSize = true;
      labelEscape.Location = new System.Drawing.Point(310, 6);
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
      textBoxEscape.DataBindings.Add(new System.Windows.Forms.Binding("Character", bindingSourceCsvFile, "EscapePrefixChar", true));
      textBoxEscape.Location = new System.Drawing.Point(361, 3);
      textBoxEscape.Margin = new System.Windows.Forms.Padding(2);
      textBoxEscape.MinimumSize = new System.Drawing.Size(46, 4);
      textBoxEscape.Name = "textBoxEscape";
      textBoxEscape.Size = new System.Drawing.Size(67, 20);
      textBoxEscape.TabIndex = 123;
      textBoxEscape.Type = PunctuationTextBox.PunctuationType.Escape;
      textBoxEscape.TextChanged += DifferentSyntaxHighlighter;
      // 
      // buttonSkipLine
      // 
      buttonSkipLine.AutoSize = true;
      buttonSkipLine.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      buttonSkipLine.Location = new System.Drawing.Point(362, 29);
      buttonSkipLine.Name = "buttonSkipLine";
      buttonSkipLine.Size = new System.Drawing.Size(97, 25);
      buttonSkipLine.TabIndex = 125;
      buttonSkipLine.Text = "Guess Start Row";
      buttonSkipLine.UseVisualStyleBackColor = true;
      buttonSkipLine.Click += ButtonSkipLine_Click;
      // 
      // FindSkipRows
      // 
      ClientSize = new System.Drawing.Size(616, 551);
      Controls.Add(tableLayoutPanel1);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      Name = "FindSkipRows";
      ShowIcon = false;
      ShowInTaskbar = false;
      Text = "Skip Rows Interactive";
      Load += FindSkipRows_Load;
      tableLayoutPanel1.ResumeLayout(false);
      tableLayoutPanel1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) bindingSourceCsvFile).EndInit();
      ((System.ComponentModel.ISupportInitialize) textBox).EndInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownSkipRows).EndInit();
      ResumeLayout(false);
      PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.BindingSource bindingSourceCsvFile;
    private System.Windows.Forms.Label labelDelimiter;
    private PunctuationTextBox textBoxDelimiter;
    private System.Windows.Forms.Button buttonSkipLine;
    private System.Windows.Forms.Label m_LabelQuote;
    private FastColoredTextBoxNS.FastColoredTextBox textBox;
    private PunctuationTextBox m_TextBoxQuote;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox textBoxComment;
    private System.Windows.Forms.NumericUpDown numericUpDownSkipRows;
    private System.Windows.Forms.Label labelEscape;
    private PunctuationTextBox textBoxEscape;
  }
}