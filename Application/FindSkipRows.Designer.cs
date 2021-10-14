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
      m_Stream?.Dispose();
      m_Stream = null;
      if (disposing && (components != null))
      {
        components.Dispose();
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
            this.components = new System.ComponentModel.Container();
            
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxComment = new System.Windows.Forms.TextBox();
            this.fileSettingBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.m_LabelQuote = new System.Windows.Forms.Label();
            this.buttonSkipLine = new System.Windows.Forms.Button();
            this.labelDelimiter = new System.Windows.Forms.Label();
            this.textBoxDelimiter = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox = new FastColoredTextBoxNS.FastColoredTextBox();
            this.m_TextBoxQuote = new System.Windows.Forms.TextBox();
            this.numericUpDownSkipRows = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileSettingBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSkipRows)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.textBoxComment, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.m_LabelQuote, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.buttonSkipLine, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelDelimiter, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxDelimiter, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBox, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.m_TextBoxQuote, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.numericUpDownSkipRows, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(613, 599);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // textBoxComment
            // 
            this.textBoxComment.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBoxComment.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "CommentLine", true));
            this.textBoxComment.Location = new System.Drawing.Point(219, 29);
            this.textBoxComment.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxComment.MinimumSize = new System.Drawing.Size(46, 4);
            this.textBoxComment.Name = "textBoxComment";
            this.textBoxComment.Size = new System.Drawing.Size(46, 20);
            this.textBoxComment.TabIndex = 131;
            this.textBoxComment.TextChanged += new System.EventHandler(this.DifferentSyntaxHighlighter);
            // 
            // fileSettingBindingSource
            // 
            this.fileSettingBindingSource.AllowNew = false;
            this.fileSettingBindingSource.DataSource = typeof(CsvTools.CsvFile);
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(138, 32);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 130;
            this.label2.Text = "Line Comment:";
            // 
            // m_LabelQuote
            // 
            this.m_LabelQuote.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.m_LabelQuote.AutoSize = true;
            this.m_LabelQuote.Location = new System.Drawing.Point(10, 32);
            this.m_LabelQuote.Name = "m_LabelQuote";
            this.m_LabelQuote.Size = new System.Drawing.Size(72, 13);
            this.m_LabelQuote.TabIndex = 127;
            this.m_LabelQuote.Text = "Text Qualifier:";
            // 
            // buttonSkipLine
            // 
            this.buttonSkipLine.AutoSize = true;
            this.buttonSkipLine.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonSkipLine.Location = new System.Drawing.Point(269, 2);
            this.buttonSkipLine.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSkipLine.Name = "buttonSkipLine";
            this.buttonSkipLine.Size = new System.Drawing.Size(97, 23);
            this.buttonSkipLine.TabIndex = 125;
            this.buttonSkipLine.Text = "Guess Start Row";
            this.buttonSkipLine.UseVisualStyleBackColor = true;
            this.buttonSkipLine.Click += new System.EventHandler(this.ButtonSkipLine_Click);
            // 
            // labelDelimiter
            // 
            this.labelDelimiter.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelDelimiter.AutoSize = true;
            this.labelDelimiter.Location = new System.Drawing.Point(165, 7);
            this.labelDelimiter.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelDelimiter.Name = "labelDelimiter";
            this.labelDelimiter.Size = new System.Drawing.Size(50, 13);
            this.labelDelimiter.TabIndex = 124;
            this.labelDelimiter.Text = "Delimiter:";
            // 
            // textBoxDelimiter
            // 
            this.textBoxDelimiter.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBoxDelimiter.AutoCompleteCustomSource.AddRange(new string[] {
            "Tab"});
            this.textBoxDelimiter.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.textBoxDelimiter.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.textBoxDelimiter.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "FieldDelimiter", true));
            this.textBoxDelimiter.Location = new System.Drawing.Point(219, 3);
            this.textBoxDelimiter.Margin = new System.Windows.Forms.Padding(2);
            this.textBoxDelimiter.MinimumSize = new System.Drawing.Size(46, 4);
            this.textBoxDelimiter.Name = "textBoxDelimiter";
            this.textBoxDelimiter.Size = new System.Drawing.Size(46, 20);
            this.textBoxDelimiter.TabIndex = 123;
            this.textBoxDelimiter.TextChanged += new System.EventHandler(this.DifferentSyntaxHighlighter);
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(2, 7);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 13);
            this.label5.TabIndex = 121;
            this.label5.Text = "Skip First Lines:";
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
            this.textBox.AutoScrollMinSize = new System.Drawing.Size(0, 13);
            this.textBox.AutoSize = true;
            this.textBox.BackBrush = null;
            this.textBox.CaretColor = System.Drawing.Color.Silver;
            this.textBox.CharHeight = 13;
            this.textBox.CharWidth = 7;
            this.tableLayoutPanel1.SetColumnSpan(this.textBox, 5);
            this.textBox.CommentPrefix = "--";
            this.textBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox.DelayedEventsInterval = 50;
            this.textBox.DelayedTextChangedInterval = 50;
            this.textBox.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.textBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox.Font = new System.Drawing.Font("Courier New", 9F);
            this.textBox.IsReplaceMode = false;
            this.textBox.Location = new System.Drawing.Point(2, 53);
            this.textBox.Margin = new System.Windows.Forms.Padding(2);
            this.textBox.Name = "textBox";
            this.textBox.Paddings = new System.Windows.Forms.Padding(0);
            this.textBox.ReadOnly = true;
            this.textBox.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));            
            this.textBox.Size = new System.Drawing.Size(609, 544);
            this.textBox.TabIndex = 126;
            this.textBox.WordWrap = true;
            this.textBox.Zoom = 100;
            this.textBox.VisibleRangeChangedDelayed += new System.EventHandler(this.TextBox_VisibleRangeChangedDelayed);
            // 
            // m_TextBoxQuote
            // 
            this.m_TextBoxQuote.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "FieldQualifier", true));
            this.m_TextBoxQuote.Location = new System.Drawing.Point(88, 29);
            this.m_TextBoxQuote.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.m_TextBoxQuote.Name = "m_TextBoxQuote";
            this.m_TextBoxQuote.Size = new System.Drawing.Size(45, 20);
            this.m_TextBoxQuote.TabIndex = 129;
            this.m_TextBoxQuote.TextChanged += new System.EventHandler(this.DifferentSyntaxHighlighter);
            // 
            // numericUpDownSkipRows
            // 
            this.numericUpDownSkipRows.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.fileSettingBindingSource, "SkipRows", true));
            this.numericUpDownSkipRows.Location = new System.Drawing.Point(88, 3);
            this.numericUpDownSkipRows.Name = "numericUpDownSkipRows";
            this.numericUpDownSkipRows.Size = new System.Drawing.Size(45, 20);
            this.numericUpDownSkipRows.TabIndex = 132;
            this.numericUpDownSkipRows.ValueChanged += new System.EventHandler(this.NumericUpDownSkipRows_ValueChanged);
            // 
            // FindSkipRows
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(613, 599);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FindSkipRows";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Skip Rows Interactive";
            this.Load += new System.EventHandler(this.FindSkipRows_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileSettingBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSkipRows)).EndInit();
            this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.BindingSource fileSettingBindingSource;
    private System.Windows.Forms.Label labelDelimiter;
    private System.Windows.Forms.TextBox textBoxDelimiter;
    private System.Windows.Forms.Button buttonSkipLine;
    private System.Windows.Forms.Label m_LabelQuote;
    private FastColoredTextBoxNS.FastColoredTextBox textBox;
    private System.Windows.Forms.TextBox m_TextBoxQuote;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox textBoxComment;    
    private System.Windows.Forms.NumericUpDown numericUpDownSkipRows;
  }
}