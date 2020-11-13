namespace CsvTools
{
  partial class FormEditSettings
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;



    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.TableLayoutPanel tableLayoutPanelFile;
			System.Windows.Forms.TableLayoutPanel tableLayoutPanelBehavior;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditSettings));
			System.Windows.Forms.TableLayoutPanel tableLayoutPanelWarnings;
			this.cboRecordDelimiter = new System.Windows.Forms.ComboBox();
			this.labelDelimitedFile = new System.Windows.Forms.Label();
			this.textBoxComment = new System.Windows.Forms.TextBox();
			this.fileFormatBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.label2 = new System.Windows.Forms.Label();
			this.textBoxFile = new System.Windows.Forms.TextBox();
			this.fileSettingBindingSource = new System.Windows.Forms.BindingSource(this.components);
			this.buttonGuessDelimiter = new System.Windows.Forms.Button();
			this.btnOpenFile = new System.Windows.Forms.Button();
			this.labelDelimiter = new System.Windows.Forms.Label();
			this.textBoxDelimiter = new System.Windows.Forms.TextBox();
			this.checkBoxHeader = new System.Windows.Forms.CheckBox();
			this.labelCodePage = new System.Windows.Forms.Label();
			this.cboCodePage = new System.Windows.Forms.ComboBox();
			this.buttonGuessCP = new System.Windows.Forms.Button();
			this.checkBoxGuessHasHeader = new System.Windows.Forms.CheckBox();
			this.label3 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.buttonNewLine = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.m_LabelInfoQuoting = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this.buttonSkipLine = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.textBoxTextAsNull = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.textBoxNLPlaceholder = new System.Windows.Forms.TextBox();
			this.checkBoxGuessDelimiter = new System.Windows.Forms.CheckBox();
			this.buttonGuessHeader = new System.Windows.Forms.Button();
			this.checkBoxBOM = new System.Windows.Forms.CheckBox();
			this.checkBoxGuessCodePage = new System.Windows.Forms.CheckBox();
			this.checkBoxDisplayStartLineNo = new System.Windows.Forms.CheckBox();
			this.checkBoxDetectFileChanges = new System.Windows.Forms.CheckBox();
			this.checkBoxTreatNBSPAsSpace = new System.Windows.Forms.CheckBox();
			this.checkBoxSkipEmptyLines = new System.Windows.Forms.CheckBox();
			this.checkBoxTreatUnknowCharaterAsSpace = new System.Windows.Forms.CheckBox();
			this.checkBoxMenuDown = new System.Windows.Forms.CheckBox();
			this.checkBoxTreatLFasSpace = new System.Windows.Forms.CheckBox();
			this.chkUseFileSettings = new System.Windows.Forms.CheckBox();
			this.checkBoxTryToSolveMoreColumns = new System.Windows.Forms.CheckBox();
			this.checkBoxAllowRowCombining = new System.Windows.Forms.CheckBox();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.label9 = new System.Windows.Forms.Label();
			this.domainUpDownTime = new System.Windows.Forms.DomainUpDown();
			this.checkBoxWarnEmptyTailingColumns = new System.Windows.Forms.CheckBox();
			this.textBoxNumWarnings = new System.Windows.Forms.TextBox();
			this.labelWarningLimit = new System.Windows.Forms.Label();
			this.checkBoxWarnNBSP = new System.Windows.Forms.CheckBox();
			this.checkBoxWarnUnknowCharater = new System.Windows.Forms.CheckBox();
			this.checkBoxWarnDelimiterInValue = new System.Windows.Forms.CheckBox();
			this.checkBoxWarnLineFeed = new System.Windows.Forms.CheckBox();
			this.checkBoxWarnQuotes = new System.Windows.Forms.CheckBox();
			this.tabPageFormat = new System.Windows.Forms.TabPage();
			this.fillGuessSettingEdit = new CsvTools.FillGuessSettingEdit();
			this.tabPageWarnings = new System.Windows.Forms.TabPage();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabPageFile = new System.Windows.Forms.TabPage();
			this.tabPageQuoting = new System.Windows.Forms.TabPage();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.quotingControl = new CsvTools.QuotingControl();
			this.checkBoxCheckQuote = new System.Windows.Forms.CheckBox();
			this.buttonGuessTextQualifier = new System.Windows.Forms.Button();
			this.tabPageBehaviour = new System.Windows.Forms.TabPage();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
			tableLayoutPanelFile = new System.Windows.Forms.TableLayoutPanel();
			tableLayoutPanelBehavior = new System.Windows.Forms.TableLayoutPanel();
			tableLayoutPanelWarnings = new System.Windows.Forms.TableLayoutPanel();
			tableLayoutPanelFile.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.fileFormatBindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.fileSettingBindingSource)).BeginInit();
			tableLayoutPanelBehavior.SuspendLayout();
			tableLayoutPanelWarnings.SuspendLayout();
			this.tabPageFormat.SuspendLayout();
			this.tabPageWarnings.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabPageFile.SuspendLayout();
			this.tabPageQuoting.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tabPageBehaviour.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
			this.SuspendLayout();
			// 
			// tableLayoutPanelFile
			// 
			tableLayoutPanelFile.AutoSize = true;
			tableLayoutPanelFile.ColumnCount = 6;
			tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			tableLayoutPanelFile.Controls.Add(this.cboRecordDelimiter, 1, 5);
			tableLayoutPanelFile.Controls.Add(this.labelDelimitedFile, 0, 0);
			tableLayoutPanelFile.Controls.Add(this.textBoxComment, 1, 4);
			tableLayoutPanelFile.Controls.Add(this.label2, 0, 4);
			tableLayoutPanelFile.Controls.Add(this.textBoxFile, 1, 0);
			tableLayoutPanelFile.Controls.Add(this.buttonGuessDelimiter, 5, 3);
			tableLayoutPanelFile.Controls.Add(this.btnOpenFile, 5, 0);
			tableLayoutPanelFile.Controls.Add(this.labelDelimiter, 0, 3);
			tableLayoutPanelFile.Controls.Add(this.textBoxDelimiter, 1, 3);
			tableLayoutPanelFile.Controls.Add(this.checkBoxHeader, 1, 1);
			tableLayoutPanelFile.Controls.Add(this.labelCodePage, 0, 2);
			tableLayoutPanelFile.Controls.Add(this.cboCodePage, 1, 2);
			tableLayoutPanelFile.Controls.Add(this.buttonGuessCP, 5, 2);
			tableLayoutPanelFile.Controls.Add(this.checkBoxGuessHasHeader, 4, 1);
			tableLayoutPanelFile.Controls.Add(this.label3, 2, 3);
			tableLayoutPanelFile.Controls.Add(this.textBox1, 3, 3);
			tableLayoutPanelFile.Controls.Add(this.buttonNewLine, 5, 5);
			tableLayoutPanelFile.Controls.Add(this.label4, 0, 5);
			tableLayoutPanelFile.Controls.Add(this.m_LabelInfoQuoting, 1, 6);
			tableLayoutPanelFile.Controls.Add(this.label5, 0, 7);
			tableLayoutPanelFile.Controls.Add(this.textBox2, 1, 7);
			tableLayoutPanelFile.Controls.Add(this.checkBox2, 4, 7);
			tableLayoutPanelFile.Controls.Add(this.buttonSkipLine, 5, 7);
			tableLayoutPanelFile.Controls.Add(this.label6, 0, 8);
			tableLayoutPanelFile.Controls.Add(this.textBox3, 1, 8);
			tableLayoutPanelFile.Controls.Add(this.label1, 0, 10);
			tableLayoutPanelFile.Controls.Add(this.textBoxTextAsNull, 1, 10);
			tableLayoutPanelFile.Controls.Add(this.label7, 2, 8);
			tableLayoutPanelFile.Controls.Add(this.textBoxNLPlaceholder, 3, 8);
			tableLayoutPanelFile.Controls.Add(this.checkBoxGuessDelimiter, 4, 3);
			tableLayoutPanelFile.Controls.Add(this.buttonGuessHeader, 5, 1);
			tableLayoutPanelFile.Controls.Add(this.checkBoxBOM, 3, 2);
			tableLayoutPanelFile.Controls.Add(this.checkBoxGuessCodePage, 4, 2);
			tableLayoutPanelFile.Dock = System.Windows.Forms.DockStyle.Top;
			tableLayoutPanelFile.Location = new System.Drawing.Point(2, 2);
			tableLayoutPanelFile.Margin = new System.Windows.Forms.Padding(2);
			tableLayoutPanelFile.Name = "tableLayoutPanelFile";
			tableLayoutPanelFile.RowCount = 12;
			tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelFile.Size = new System.Drawing.Size(804, 263);
			tableLayoutPanelFile.TabIndex = 48;
			// 
			// cboRecordDelimiter
			// 
			this.cboRecordDelimiter.Anchor = System.Windows.Forms.AnchorStyles.Left;
			tableLayoutPanelFile.SetColumnSpan(this.cboRecordDelimiter, 3);
			this.cboRecordDelimiter.DisplayMember = "Display";
			this.cboRecordDelimiter.FormattingEnabled = true;
			this.cboRecordDelimiter.Location = new System.Drawing.Point(135, 142);
			this.cboRecordDelimiter.Margin = new System.Windows.Forms.Padding(2);
			this.cboRecordDelimiter.MinimumSize = new System.Drawing.Size(46, 0);
			this.cboRecordDelimiter.Name = "cboRecordDelimiter";
			this.cboRecordDelimiter.Size = new System.Drawing.Size(212, 21);
			this.cboRecordDelimiter.TabIndex = 49;
			this.cboRecordDelimiter.ValueMember = "ID";
			this.cboRecordDelimiter.SelectedIndexChanged += new System.EventHandler(this.CboRecordDelimiter_SelectedIndexChanged);
			// 
			// labelDelimitedFile
			// 
			this.labelDelimitedFile.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.labelDelimitedFile.AutoSize = true;
			this.labelDelimitedFile.Location = new System.Drawing.Point(45, 7);
			this.labelDelimitedFile.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.labelDelimitedFile.Name = "labelDelimitedFile";
			this.labelDelimitedFile.Size = new System.Drawing.Size(86, 15);
			this.labelDelimitedFile.TabIndex = 39;
			this.labelDelimitedFile.Text = "Delimited File:";
			// 
			// textBoxComment
			// 
			this.textBoxComment.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBoxComment.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "CommentLine", true));
			this.textBoxComment.Location = new System.Drawing.Point(135, 118);
			this.textBoxComment.Margin = new System.Windows.Forms.Padding(2);
			this.textBoxComment.MinimumSize = new System.Drawing.Size(46, 4);
			this.textBoxComment.Name = "textBoxComment";
			this.textBoxComment.Size = new System.Drawing.Size(46, 20);
			this.textBoxComment.TabIndex = 10;
			// 
			// fileFormatBindingSource
			// 
			this.fileFormatBindingSource.AllowNew = false;
			this.fileFormatBindingSource.DataSource = typeof(CsvTools.FileFormat);
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(40, 120);
			this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(91, 15);
			this.label2.TabIndex = 47;
			this.label2.Text = "Line Comment:";
			// 
			// textBoxFile
			// 
			this.textBoxFile.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBoxFile.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.textBoxFile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
			tableLayoutPanelFile.SetColumnSpan(this.textBoxFile, 4);
			this.textBoxFile.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "FileName", true));
			this.textBoxFile.Location = new System.Drawing.Point(135, 4);
			this.textBoxFile.Margin = new System.Windows.Forms.Padding(2);
			this.textBoxFile.MinimumSize = new System.Drawing.Size(46, 4);
			this.textBoxFile.Name = "textBoxFile";
			this.textBoxFile.Size = new System.Drawing.Size(520, 20);
			this.textBoxFile.TabIndex = 0;
			this.textBoxFile.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxFile_Validating);
			// 
			// fileSettingBindingSource
			// 
			this.fileSettingBindingSource.AllowNew = false;
			this.fileSettingBindingSource.DataSource = typeof(CsvTools.ViewSettings);
			// 
			// buttonGuessDelimiter
			// 
			this.buttonGuessDelimiter.AutoSize = true;
			this.buttonGuessDelimiter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonGuessDelimiter.Location = new System.Drawing.Point(723, 89);
			this.buttonGuessDelimiter.Margin = new System.Windows.Forms.Padding(2);
			this.buttonGuessDelimiter.Name = "buttonGuessDelimiter";
			this.buttonGuessDelimiter.Size = new System.Drawing.Size(158, 25);
			this.buttonGuessDelimiter.TabIndex = 8;
			this.buttonGuessDelimiter.Text = "Guess Delimiter";
			this.buttonGuessDelimiter.UseVisualStyleBackColor = true;
			this.buttonGuessDelimiter.Click += new System.EventHandler(this.ButtonGuessDelimiter_ClickAsync);
			// 
			// btnOpenFile
			// 
			this.btnOpenFile.AutoSize = true;
			this.btnOpenFile.Location = new System.Drawing.Point(723, 2);
			this.btnOpenFile.Margin = new System.Windows.Forms.Padding(2);
			this.btnOpenFile.Name = "btnOpenFile";
			this.btnOpenFile.Size = new System.Drawing.Size(158, 25);
			this.btnOpenFile.TabIndex = 1;
			this.btnOpenFile.Text = "Select File";
			this.btnOpenFile.UseVisualStyleBackColor = true;
			this.btnOpenFile.Click += new System.EventHandler(this.BtnOpenFile_ClickAsync);
			// 
			// labelDelimiter
			// 
			this.labelDelimiter.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.labelDelimiter.AutoSize = true;
			this.labelDelimiter.Location = new System.Drawing.Point(71, 94);
			this.labelDelimiter.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.labelDelimiter.Name = "labelDelimiter";
			this.labelDelimiter.Size = new System.Drawing.Size(60, 15);
			this.labelDelimiter.TabIndex = 46;
			this.labelDelimiter.Text = "Delimiter:";
			// 
			// textBoxDelimiter
			// 
			this.textBoxDelimiter.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBoxDelimiter.AutoCompleteCustomSource.AddRange(new string[] {
            "TAB"});
			this.textBoxDelimiter.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.textBoxDelimiter.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
			this.textBoxDelimiter.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "FieldDelimiter", true));
			this.textBoxDelimiter.Location = new System.Drawing.Point(135, 91);
			this.textBoxDelimiter.Margin = new System.Windows.Forms.Padding(2);
			this.textBoxDelimiter.MinimumSize = new System.Drawing.Size(46, 4);
			this.textBoxDelimiter.Name = "textBoxDelimiter";
			this.textBoxDelimiter.Size = new System.Drawing.Size(46, 20);
			this.textBoxDelimiter.TabIndex = 7;
			this.textBoxDelimiter.TextChanged += new System.EventHandler(this.TextBoxDelimiter_TextChanged);
			// 
			// checkBoxHeader
			// 
			this.checkBoxHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxHeader.AutoSize = true;
			tableLayoutPanelFile.SetColumnSpan(this.checkBoxHeader, 3);
			this.checkBoxHeader.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "HasFieldHeader", true));
			this.checkBoxHeader.Location = new System.Drawing.Point(135, 34);
			this.checkBoxHeader.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxHeader.Name = "checkBoxHeader";
			this.checkBoxHeader.Size = new System.Drawing.Size(147, 19);
			this.checkBoxHeader.TabIndex = 2;
			this.checkBoxHeader.Text = "Has Column Headers";
			this.checkBoxHeader.UseVisualStyleBackColor = true;
			// 
			// labelCodePage
			// 
			this.labelCodePage.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.labelCodePage.AutoSize = true;
			this.labelCodePage.Location = new System.Drawing.Point(60, 65);
			this.labelCodePage.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.labelCodePage.Name = "labelCodePage";
			this.labelCodePage.Size = new System.Drawing.Size(71, 15);
			this.labelCodePage.TabIndex = 44;
			this.labelCodePage.Text = "Code Page:";
			// 
			// cboCodePage
			// 
			this.cboCodePage.Anchor = System.Windows.Forms.AnchorStyles.Left;
			tableLayoutPanelFile.SetColumnSpan(this.cboCodePage, 2);
			this.cboCodePage.DataBindings.Add(new System.Windows.Forms.Binding("SelectedValue", this.fileSettingBindingSource, "CodePageId", true));
			this.cboCodePage.DisplayMember = "Display";
			this.cboCodePage.FormattingEnabled = true;
			this.cboCodePage.Location = new System.Drawing.Point(135, 61);
			this.cboCodePage.Margin = new System.Windows.Forms.Padding(2);
			this.cboCodePage.MinimumSize = new System.Drawing.Size(46, 0);
			this.cboCodePage.Name = "cboCodePage";
			this.cboCodePage.Size = new System.Drawing.Size(212, 21);
			this.cboCodePage.TabIndex = 4;
			this.cboCodePage.ValueMember = "ID";
			this.cboCodePage.SelectedIndexChanged += new System.EventHandler(this.CboCodePage_SelectedIndexChanged);
			// 
			// buttonGuessCP
			// 
			this.buttonGuessCP.AutoSize = true;
			this.buttonGuessCP.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonGuessCP.Location = new System.Drawing.Point(723, 60);
			this.buttonGuessCP.Margin = new System.Windows.Forms.Padding(2);
			this.buttonGuessCP.Name = "buttonGuessCP";
			this.buttonGuessCP.Size = new System.Drawing.Size(158, 25);
			this.buttonGuessCP.TabIndex = 5;
			this.buttonGuessCP.Text = "Guess Code Page";
			this.buttonGuessCP.UseVisualStyleBackColor = true;
			this.buttonGuessCP.Click += new System.EventHandler(this.ButtonGuessCP_ClickAsync);
			// 
			// checkBoxGuessHasHeader
			// 
			this.checkBoxGuessHasHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxGuessHasHeader.AutoSize = true;
			this.checkBoxGuessHasHeader.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessHasHeader", true));
			this.checkBoxGuessHasHeader.Location = new System.Drawing.Point(437, 34);
			this.checkBoxGuessHasHeader.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxGuessHasHeader.Name = "checkBoxGuessHasHeader";
			this.checkBoxGuessHasHeader.Size = new System.Drawing.Size(196, 19);
			this.checkBoxGuessHasHeader.TabIndex = 3;
			this.checkBoxGuessHasHeader.Text = "Determine if Header is present";
			this.checkBoxGuessHasHeader.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(296, 94);
			this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(51, 15);
			this.label3.TabIndex = 46;
			this.label3.Text = "Escape:";
			// 
			// textBox1
			// 
			this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBox1.AutoCompleteCustomSource.AddRange(new string[] {
            "\\"});
			this.textBox1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.textBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
			this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "EscapeCharacter", true));
			this.textBox1.Location = new System.Drawing.Point(351, 91);
			this.textBox1.Margin = new System.Windows.Forms.Padding(2);
			this.textBox1.MinimumSize = new System.Drawing.Size(46, 4);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(46, 20);
			this.textBox1.TabIndex = 45;
			this.toolTip.SetToolTip(this.textBox1, "An escape character is used for escaping quotes and delimiters in the regular tex" +
        "t. ");
			this.textBox1.TextChanged += new System.EventHandler(this.TextBoxDelimiter_TextChanged);
			// 
			// buttonNewLine
			// 
			this.buttonNewLine.AutoSize = true;
			this.buttonNewLine.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonNewLine.Location = new System.Drawing.Point(723, 142);
			this.buttonNewLine.Margin = new System.Windows.Forms.Padding(2);
			this.buttonNewLine.Name = "buttonNewLine";
			tableLayoutPanelFile.SetRowSpan(this.buttonNewLine, 2);
			this.buttonNewLine.Size = new System.Drawing.Size(158, 25);
			this.buttonNewLine.TabIndex = 8;
			this.buttonNewLine.Text = "Guess Record Seperation";
			this.buttonNewLine.UseVisualStyleBackColor = true;
			this.buttonNewLine.Click += new System.EventHandler(this.GuessNewline_Click);
			// 
			// label4
			// 
			this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(18, 146);
			this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(113, 15);
			this.label4.TabIndex = 44;
			this.label4.Text = "Record Seperation:";
			// 
			// m_LabelInfoQuoting
			// 
			this.m_LabelInfoQuoting.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.m_LabelInfoQuoting.AutoSize = true;
			this.m_LabelInfoQuoting.BackColor = System.Drawing.SystemColors.Info;
			this.m_LabelInfoQuoting.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			tableLayoutPanelFile.SetColumnSpan(this.m_LabelInfoQuoting, 4);
			this.m_LabelInfoQuoting.ForeColor = System.Drawing.SystemColors.InfoText;
			this.m_LabelInfoQuoting.Location = new System.Drawing.Point(136, 167);
			this.m_LabelInfoQuoting.Name = "m_LabelInfoQuoting";
			this.m_LabelInfoQuoting.Padding = new System.Windows.Forms.Padding(1);
			this.m_LabelInfoQuoting.Size = new System.Drawing.Size(582, 19);
			this.m_LabelInfoQuoting.TabIndex = 50;
			this.m_LabelInfoQuoting.Text = "Note: Any combination of CR and LF will be treated as record separator, no matter" +
    " what specific type is set";
			// 
			// label5
			// 
			this.label5.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(38, 193);
			this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(93, 15);
			this.label5.TabIndex = 120;
			this.label5.Text = "Skip First Lines:";
			// 
			// textBox2
			// 
			this.textBox2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBox2.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "SkipRows", true));
			this.textBox2.Location = new System.Drawing.Point(135, 190);
			this.textBox2.Margin = new System.Windows.Forms.Padding(2);
			this.textBox2.MinimumSize = new System.Drawing.Size(46, 4);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(46, 20);
			this.textBox2.TabIndex = 121;
			this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// checkBox2
			// 
			this.checkBox2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBox2.AutoSize = true;
			this.checkBox2.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessStartRow", true));
			this.checkBox2.Location = new System.Drawing.Point(437, 191);
			this.checkBox2.Margin = new System.Windows.Forms.Padding(2);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(143, 19);
			this.checkBox2.TabIndex = 122;
			this.checkBox2.Text = "Determine Start Row";
			this.checkBox2.UseVisualStyleBackColor = true;
			// 
			// buttonSkipLine
			// 
			this.buttonSkipLine.AutoSize = true;
			this.buttonSkipLine.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonSkipLine.Location = new System.Drawing.Point(723, 188);
			this.buttonSkipLine.Margin = new System.Windows.Forms.Padding(2);
			this.buttonSkipLine.Name = "buttonSkipLine";
			this.buttonSkipLine.Size = new System.Drawing.Size(158, 25);
			this.buttonSkipLine.TabIndex = 123;
			this.buttonSkipLine.Text = "Guess Start Row";
			this.buttonSkipLine.UseVisualStyleBackColor = true;
			this.buttonSkipLine.Click += new System.EventHandler(this.ButtonSkipLine_ClickAsync);
			// 
			// label6
			// 
			this.label6.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(2, 219);
			this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(129, 15);
			this.label6.TabIndex = 124;
			this.label6.Text = "Delimiter Placeholder:";
			// 
			// textBox3
			// 
			this.textBox3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBox3.AutoCompleteCustomSource.AddRange(new string[] {
            "{d}"});
			this.textBox3.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.textBox3.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "DelimiterPlaceholder", true));
			this.textBox3.Location = new System.Drawing.Point(135, 217);
			this.textBox3.Margin = new System.Windows.Forms.Padding(2);
			this.textBox3.MinimumSize = new System.Drawing.Size(46, 4);
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(46, 20);
			this.textBox3.TabIndex = 125;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(16, 243);
			this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(115, 15);
			this.label1.TabIndex = 128;
			this.label1.Text = "Treat Text as NULL:";
			// 
			// textBoxTextAsNull
			// 
			this.textBoxTextAsNull.AutoCompleteCustomSource.AddRange(new string[] {
            "NULL",
            "n.a.",
            "n/a"});
			this.textBoxTextAsNull.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "TreatTextAsNull", true));
			this.textBoxTextAsNull.Location = new System.Drawing.Point(135, 241);
			this.textBoxTextAsNull.Margin = new System.Windows.Forms.Padding(2);
			this.textBoxTextAsNull.MinimumSize = new System.Drawing.Size(46, 4);
			this.textBoxTextAsNull.Name = "textBoxTextAsNull";
			this.textBoxTextAsNull.Size = new System.Drawing.Size(46, 20);
			this.textBoxTextAsNull.TabIndex = 129;
			// 
			// label7
			// 
			this.label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(220, 219);
			this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(127, 15);
			this.label7.TabIndex = 126;
			this.label7.Text = "Linefeed Placeholder:\r\n";
			// 
			// textBoxNLPlaceholder
			// 
			this.textBoxNLPlaceholder.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.textBoxNLPlaceholder.AutoCompleteCustomSource.AddRange(new string[] {
            "<br>",
            "{n}"});
			this.textBoxNLPlaceholder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this.textBoxNLPlaceholder.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "NewLinePlaceholder", true));
			this.textBoxNLPlaceholder.Location = new System.Drawing.Point(351, 217);
			this.textBoxNLPlaceholder.Margin = new System.Windows.Forms.Padding(2);
			this.textBoxNLPlaceholder.MinimumSize = new System.Drawing.Size(46, 4);
			this.textBoxNLPlaceholder.Name = "textBoxNLPlaceholder";
			this.textBoxNLPlaceholder.Size = new System.Drawing.Size(46, 20);
			this.textBoxNLPlaceholder.TabIndex = 127;
			// 
			// checkBoxGuessDelimiter
			// 
			this.checkBoxGuessDelimiter.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxGuessDelimiter.AutoSize = true;
			this.checkBoxGuessDelimiter.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessDelimiter", true));
			this.checkBoxGuessDelimiter.Location = new System.Drawing.Point(437, 92);
			this.checkBoxGuessDelimiter.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxGuessDelimiter.Name = "checkBoxGuessDelimiter";
			this.checkBoxGuessDelimiter.Size = new System.Drawing.Size(140, 19);
			this.checkBoxGuessDelimiter.TabIndex = 9;
			this.checkBoxGuessDelimiter.Text = "Determine Delimiter";
			this.checkBoxGuessDelimiter.UseVisualStyleBackColor = true;
			// 
			// buttonGuessHeader
			// 
			this.buttonGuessHeader.AutoSize = true;
			this.buttonGuessHeader.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonGuessHeader.Location = new System.Drawing.Point(723, 31);
			this.buttonGuessHeader.Margin = new System.Windows.Forms.Padding(2);
			this.buttonGuessHeader.Name = "buttonGuessHeader";
			this.buttonGuessHeader.Size = new System.Drawing.Size(158, 25);
			this.buttonGuessHeader.TabIndex = 130;
			this.buttonGuessHeader.Text = "Guess Header";
			this.buttonGuessHeader.UseVisualStyleBackColor = true;
			this.buttonGuessHeader.Click += new System.EventHandler(this.ButtonGuessHeader_Click);
			// 
			// checkBoxBOM
			// 
			this.checkBoxBOM.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxBOM.AutoSize = true;
			this.checkBoxBOM.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "ByteOrderMark", true));
			this.checkBoxBOM.Location = new System.Drawing.Point(351, 63);
			this.checkBoxBOM.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxBOM.Name = "checkBoxBOM";
			this.checkBoxBOM.Size = new System.Drawing.Size(82, 19);
			this.checkBoxBOM.TabIndex = 42;
			this.checkBoxBOM.Text = "Has BOM";
			this.toolTip.SetToolTip(this.checkBoxBOM, "Byte Order Mark");
			this.checkBoxBOM.UseVisualStyleBackColor = true;
			// 
			// checkBoxGuessCodePage
			// 
			this.checkBoxGuessCodePage.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxGuessCodePage.AutoSize = true;
			this.checkBoxGuessCodePage.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessCodePage", true));
			this.checkBoxGuessCodePage.Location = new System.Drawing.Point(437, 63);
			this.checkBoxGuessCodePage.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxGuessCodePage.Name = "checkBoxGuessCodePage";
			this.checkBoxGuessCodePage.Size = new System.Drawing.Size(151, 19);
			this.checkBoxGuessCodePage.TabIndex = 3;
			this.checkBoxGuessCodePage.Text = "Determine Code Page";
			this.checkBoxGuessCodePage.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanelBehavior
			// 
			tableLayoutPanelBehavior.AutoSize = true;
			tableLayoutPanelBehavior.ColumnCount = 3;
			tableLayoutPanelBehavior.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayoutPanelBehavior.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayoutPanelBehavior.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanelBehavior.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			tableLayoutPanelBehavior.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			tableLayoutPanelBehavior.Controls.Add(this.checkBoxDisplayStartLineNo, 0, 10);
			tableLayoutPanelBehavior.Controls.Add(this.checkBoxDetectFileChanges, 0, 0);
			tableLayoutPanelBehavior.Controls.Add(this.checkBoxTreatNBSPAsSpace, 0, 8);
			tableLayoutPanelBehavior.Controls.Add(this.checkBoxSkipEmptyLines, 0, 3);
			tableLayoutPanelBehavior.Controls.Add(this.checkBoxTreatUnknowCharaterAsSpace, 0, 7);
			tableLayoutPanelBehavior.Controls.Add(this.checkBoxMenuDown, 0, 1);
			tableLayoutPanelBehavior.Controls.Add(this.checkBoxTreatLFasSpace, 0, 6);
			tableLayoutPanelBehavior.Controls.Add(this.chkUseFileSettings, 0, 2);
			tableLayoutPanelBehavior.Controls.Add(this.checkBoxTryToSolveMoreColumns, 0, 5);
			tableLayoutPanelBehavior.Controls.Add(this.checkBoxAllowRowCombining, 0, 4);
			tableLayoutPanelBehavior.Controls.Add(this.checkBox1, 0, 9);
			tableLayoutPanelBehavior.Controls.Add(this.label9, 0, 11);
			tableLayoutPanelBehavior.Controls.Add(this.domainUpDownTime, 1, 11);
			tableLayoutPanelBehavior.Dock = System.Windows.Forms.DockStyle.Top;
			tableLayoutPanelBehavior.Location = new System.Drawing.Point(2, 2);
			tableLayoutPanelBehavior.Margin = new System.Windows.Forms.Padding(2);
			tableLayoutPanelBehavior.Name = "tableLayoutPanelBehavior";
			tableLayoutPanelBehavior.RowCount = 12;
			tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelBehavior.Size = new System.Drawing.Size(804, 279);
			tableLayoutPanelBehavior.TabIndex = 9;
			// 
			// checkBoxDisplayStartLineNo
			// 
			this.checkBoxDisplayStartLineNo.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxDisplayStartLineNo.AutoSize = true;
			this.checkBoxDisplayStartLineNo.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			tableLayoutPanelBehavior.SetColumnSpan(this.checkBoxDisplayStartLineNo, 3);
			this.checkBoxDisplayStartLineNo.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "DisplayRecordNo", true));
			this.checkBoxDisplayStartLineNo.Location = new System.Drawing.Point(2, 232);
			this.checkBoxDisplayStartLineNo.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxDisplayStartLineNo.Name = "checkBoxDisplayStartLineNo";
			this.checkBoxDisplayStartLineNo.Size = new System.Drawing.Size(204, 19);
			this.checkBoxDisplayStartLineNo.TabIndex = 122;
			this.checkBoxDisplayStartLineNo.Text = "Add Column for Record Number";
			this.checkBoxDisplayStartLineNo.UseVisualStyleBackColor = true;
			// 
			// checkBoxDetectFileChanges
			// 
			this.checkBoxDetectFileChanges.AutoSize = true;
			tableLayoutPanelBehavior.SetColumnSpan(this.checkBoxDetectFileChanges, 3);
			this.checkBoxDetectFileChanges.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "DetectFileChanges", true));
			this.checkBoxDetectFileChanges.Location = new System.Drawing.Point(2, 2);
			this.checkBoxDetectFileChanges.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxDetectFileChanges.Name = "checkBoxDetectFileChanges";
			this.checkBoxDetectFileChanges.Size = new System.Drawing.Size(139, 19);
			this.checkBoxDetectFileChanges.TabIndex = 0;
			this.checkBoxDetectFileChanges.Text = "Detect File Changes";
			this.checkBoxDetectFileChanges.UseVisualStyleBackColor = true;
			// 
			// checkBoxTreatNBSPAsSpace
			// 
			this.checkBoxTreatNBSPAsSpace.AutoSize = true;
			tableLayoutPanelBehavior.SetColumnSpan(this.checkBoxTreatNBSPAsSpace, 3);
			this.checkBoxTreatNBSPAsSpace.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "TreatNBSPAsSpace", true));
			this.checkBoxTreatNBSPAsSpace.Location = new System.Drawing.Point(2, 186);
			this.checkBoxTreatNBSPAsSpace.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxTreatNBSPAsSpace.Name = "checkBoxTreatNBSPAsSpace";
			this.checkBoxTreatNBSPAsSpace.Size = new System.Drawing.Size(225, 19);
			this.checkBoxTreatNBSPAsSpace.TabIndex = 8;
			this.checkBoxTreatNBSPAsSpace.Text = "Treat non-breaking Space as Space";
			this.toolTip.SetToolTip(this.checkBoxTreatNBSPAsSpace, "Threat any non-breaking space like a regular space");
			this.checkBoxTreatNBSPAsSpace.UseVisualStyleBackColor = true;
			// 
			// checkBoxSkipEmptyLines
			// 
			this.checkBoxSkipEmptyLines.AutoSize = true;
			tableLayoutPanelBehavior.SetColumnSpan(this.checkBoxSkipEmptyLines, 3);
			this.checkBoxSkipEmptyLines.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "SkipEmptyLines", true));
			this.checkBoxSkipEmptyLines.Location = new System.Drawing.Point(2, 71);
			this.checkBoxSkipEmptyLines.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxSkipEmptyLines.Name = "checkBoxSkipEmptyLines";
			this.checkBoxSkipEmptyLines.Size = new System.Drawing.Size(123, 19);
			this.checkBoxSkipEmptyLines.TabIndex = 3;
			this.checkBoxSkipEmptyLines.Text = "Skip Empty Lines";
			this.checkBoxSkipEmptyLines.UseVisualStyleBackColor = true;
			// 
			// checkBoxTreatUnknowCharaterAsSpace
			// 
			this.checkBoxTreatUnknowCharaterAsSpace.AutoSize = true;
			this.checkBoxTreatUnknowCharaterAsSpace.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			tableLayoutPanelBehavior.SetColumnSpan(this.checkBoxTreatUnknowCharaterAsSpace, 3);
			this.checkBoxTreatUnknowCharaterAsSpace.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "TreatUnknownCharacterAsSpace", true));
			this.checkBoxTreatUnknowCharaterAsSpace.Location = new System.Drawing.Point(2, 163);
			this.checkBoxTreatUnknowCharaterAsSpace.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxTreatUnknowCharaterAsSpace.Name = "checkBoxTreatUnknowCharaterAsSpace";
			this.checkBoxTreatUnknowCharaterAsSpace.Size = new System.Drawing.Size(232, 19);
			this.checkBoxTreatUnknowCharaterAsSpace.TabIndex = 7;
			this.checkBoxTreatUnknowCharaterAsSpace.Text = "Treat Unknown Character � as Space";
			this.toolTip.SetToolTip(this.checkBoxTreatUnknowCharaterAsSpace, "Threat any unknown character like a space");
			this.checkBoxTreatUnknowCharaterAsSpace.UseVisualStyleBackColor = true;
			// 
			// checkBoxMenuDown
			// 
			this.checkBoxMenuDown.AutoSize = true;
			tableLayoutPanelBehavior.SetColumnSpan(this.checkBoxMenuDown, 3);
			this.checkBoxMenuDown.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "MenuDown", true));
			this.checkBoxMenuDown.Location = new System.Drawing.Point(2, 25);
			this.checkBoxMenuDown.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxMenuDown.Name = "checkBoxMenuDown";
			this.checkBoxMenuDown.Size = new System.Drawing.Size(207, 19);
			this.checkBoxMenuDown.TabIndex = 1;
			this.checkBoxMenuDown.Text = "Display Actions in Navigation Bar";
			this.checkBoxMenuDown.UseVisualStyleBackColor = true;
			// 
			// checkBoxTreatLFasSpace
			// 
			this.checkBoxTreatLFasSpace.AutoSize = true;
			tableLayoutPanelBehavior.SetColumnSpan(this.checkBoxTreatLFasSpace, 3);
			this.checkBoxTreatLFasSpace.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "TreatLFAsSpace", true));
			this.checkBoxTreatLFasSpace.Location = new System.Drawing.Point(2, 140);
			this.checkBoxTreatLFasSpace.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxTreatLFasSpace.Name = "checkBoxTreatLFasSpace";
			this.checkBoxTreatLFasSpace.Size = new System.Drawing.Size(128, 19);
			this.checkBoxTreatLFasSpace.TabIndex = 6;
			this.checkBoxTreatLFasSpace.Text = "Treat LF as Space";
			this.toolTip.SetToolTip(this.checkBoxTreatLFasSpace, "Threat a single occurrence of a LF as a space");
			this.checkBoxTreatLFasSpace.UseVisualStyleBackColor = true;
			// 
			// chkUseFileSettings
			// 
			this.chkUseFileSettings.AutoSize = true;
			tableLayoutPanelBehavior.SetColumnSpan(this.chkUseFileSettings, 3);
			this.chkUseFileSettings.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "StoreSettingsByFile", true));
			this.chkUseFileSettings.Location = new System.Drawing.Point(2, 48);
			this.chkUseFileSettings.Margin = new System.Windows.Forms.Padding(2);
			this.chkUseFileSettings.Name = "chkUseFileSettings";
			this.chkUseFileSettings.Size = new System.Drawing.Size(153, 19);
			this.chkUseFileSettings.TabIndex = 2;
			this.chkUseFileSettings.Text = "Persist Settings for File";
			this.toolTip.SetToolTip(this.chkUseFileSettings, "Store the settings for each individual file, do not use this is structure or form" +
        "atting of columns does change over time");
			this.chkUseFileSettings.UseVisualStyleBackColor = true;
			// 
			// checkBoxTryToSolveMoreColumns
			// 
			this.checkBoxTryToSolveMoreColumns.AutoSize = true;
			tableLayoutPanelBehavior.SetColumnSpan(this.checkBoxTryToSolveMoreColumns, 3);
			this.checkBoxTryToSolveMoreColumns.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "TryToSolveMoreColumns", true));
			this.checkBoxTryToSolveMoreColumns.Location = new System.Drawing.Point(2, 117);
			this.checkBoxTryToSolveMoreColumns.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxTryToSolveMoreColumns.Name = "checkBoxTryToSolveMoreColumns";
			this.checkBoxTryToSolveMoreColumns.Size = new System.Drawing.Size(485, 19);
			this.checkBoxTryToSolveMoreColumns.TabIndex = 5;
			this.checkBoxTryToSolveMoreColumns.Text = "Try to Re-Align columns / Handle records that have more than the expected columns" +
    "";
			this.toolTip.SetToolTip(this.checkBoxTryToSolveMoreColumns, resources.GetString("checkBoxTryToSolveMoreColumns.ToolTip"));
			this.checkBoxTryToSolveMoreColumns.UseVisualStyleBackColor = true;
			this.checkBoxTryToSolveMoreColumns.CheckedChanged += new System.EventHandler(this.CheckBoxColumnsProcess_CheckedChanged);
			// 
			// checkBoxAllowRowCombining
			// 
			this.checkBoxAllowRowCombining.AutoSize = true;
			tableLayoutPanelBehavior.SetColumnSpan(this.checkBoxAllowRowCombining, 3);
			this.checkBoxAllowRowCombining.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "AllowRowCombining", true));
			this.checkBoxAllowRowCombining.Location = new System.Drawing.Point(2, 94);
			this.checkBoxAllowRowCombining.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxAllowRowCombining.Name = "checkBoxAllowRowCombining";
			this.checkBoxAllowRowCombining.Size = new System.Drawing.Size(459, 19);
			this.checkBoxAllowRowCombining.TabIndex = 4;
			this.checkBoxAllowRowCombining.Text = "Try to Combine Row / Handle records that have less than the expected columns";
			this.toolTip.SetToolTip(this.checkBoxAllowRowCombining, resources.GetString("checkBoxAllowRowCombining.ToolTip"));
			this.checkBoxAllowRowCombining.UseVisualStyleBackColor = true;
			this.checkBoxAllowRowCombining.CheckedChanged += new System.EventHandler(this.CheckBoxColumnsProcess_CheckedChanged);
			// 
			// checkBox1
			// 
			this.checkBox1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBox1.AutoSize = true;
			this.checkBox1.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			tableLayoutPanelBehavior.SetColumnSpan(this.checkBox1, 3);
			this.checkBox1.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "DisplayStartLineNo", true));
			this.checkBox1.Location = new System.Drawing.Point(2, 209);
			this.checkBox1.Margin = new System.Windows.Forms.Padding(2);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(188, 19);
			this.checkBox1.TabIndex = 121;
			this.checkBox1.Text = "Add Column for Line Number";
			this.checkBox1.UseVisualStyleBackColor = true;
			// 
			// label9
			// 
			this.label9.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(3, 258);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(177, 15);
			this.label9.TabIndex = 124;
			this.label9.Text = "Limit for processing initail open";
			// 
			// domainUpDownTime
			// 
			this.domainUpDownTime.Items.Add("unlimited");
			this.domainUpDownTime.Items.Add("10 seconds");
			this.domainUpDownTime.Items.Add("2 seconds");
			this.domainUpDownTime.Items.Add("1 second");
			this.domainUpDownTime.Items.Add("1/2 second");
			this.domainUpDownTime.Location = new System.Drawing.Point(186, 256);
			this.domainUpDownTime.Name = "domainUpDownTime";
			this.domainUpDownTime.Size = new System.Drawing.Size(101, 20);
			this.domainUpDownTime.TabIndex = 126;
			this.domainUpDownTime.Text = "1 second";
			this.domainUpDownTime.SelectedItemChanged += new System.EventHandler(this.DomainUpDownTime_SelectedItemChanged);
			// 
			// tableLayoutPanelWarnings
			// 
			tableLayoutPanelWarnings.AutoSize = true;
			tableLayoutPanelWarnings.ColumnCount = 2;
			tableLayoutPanelWarnings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			tableLayoutPanelWarnings.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tableLayoutPanelWarnings.Controls.Add(this.checkBoxWarnEmptyTailingColumns, 0, 0);
			tableLayoutPanelWarnings.Controls.Add(this.textBoxNumWarnings, 1, 6);
			tableLayoutPanelWarnings.Controls.Add(this.labelWarningLimit, 0, 6);
			tableLayoutPanelWarnings.Controls.Add(this.checkBoxWarnNBSP, 0, 5);
			tableLayoutPanelWarnings.Controls.Add(this.checkBoxWarnUnknowCharater, 0, 4);
			tableLayoutPanelWarnings.Controls.Add(this.checkBoxWarnDelimiterInValue, 0, 1);
			tableLayoutPanelWarnings.Controls.Add(this.checkBoxWarnLineFeed, 0, 2);
			tableLayoutPanelWarnings.Controls.Add(this.checkBoxWarnQuotes, 0, 3);
			tableLayoutPanelWarnings.Dock = System.Windows.Forms.DockStyle.Top;
			tableLayoutPanelWarnings.Location = new System.Drawing.Point(0, 0);
			tableLayoutPanelWarnings.Margin = new System.Windows.Forms.Padding(2);
			tableLayoutPanelWarnings.Name = "tableLayoutPanelWarnings";
			tableLayoutPanelWarnings.RowCount = 7;
			tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
			tableLayoutPanelWarnings.Size = new System.Drawing.Size(808, 162);
			tableLayoutPanelWarnings.TabIndex = 58;
			// 
			// checkBoxWarnEmptyTailingColumns
			// 
			this.checkBoxWarnEmptyTailingColumns.AutoSize = true;
			tableLayoutPanelWarnings.SetColumnSpan(this.checkBoxWarnEmptyTailingColumns, 2);
			this.checkBoxWarnEmptyTailingColumns.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnEmptyTailingColumns", true));
			this.checkBoxWarnEmptyTailingColumns.Location = new System.Drawing.Point(2, 2);
			this.checkBoxWarnEmptyTailingColumns.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxWarnEmptyTailingColumns.Name = "checkBoxWarnEmptyTailingColumns";
			this.checkBoxWarnEmptyTailingColumns.Size = new System.Drawing.Size(161, 19);
			this.checkBoxWarnEmptyTailingColumns.TabIndex = 0;
			this.checkBoxWarnEmptyTailingColumns.Text = "Warn Column Mismatch";
			this.toolTip.SetToolTip(this.checkBoxWarnEmptyTailingColumns, "It is advised to enable warning in case the number of columns does not match the " +
        "number of expected columns");
			this.checkBoxWarnEmptyTailingColumns.UseVisualStyleBackColor = false;
			// 
			// textBoxNumWarnings
			// 
			this.textBoxNumWarnings.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "NumWarnings", true));
			this.textBoxNumWarnings.Location = new System.Drawing.Point(92, 140);
			this.textBoxNumWarnings.Margin = new System.Windows.Forms.Padding(2);
			this.textBoxNumWarnings.MinimumSize = new System.Drawing.Size(46, 4);
			this.textBoxNumWarnings.Name = "textBoxNumWarnings";
			this.textBoxNumWarnings.Size = new System.Drawing.Size(46, 20);
			this.textBoxNumWarnings.TabIndex = 6;
			this.textBoxNumWarnings.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.textBoxNumWarnings.Validating += new System.ComponentModel.CancelEventHandler(this.PositiveNumberValidating);
			// 
			// labelWarningLimit
			// 
			this.labelWarningLimit.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.labelWarningLimit.AutoSize = true;
			this.labelWarningLimit.Location = new System.Drawing.Point(2, 142);
			this.labelWarningLimit.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.labelWarningLimit.Name = "labelWarningLimit";
			this.labelWarningLimit.Size = new System.Drawing.Size(86, 15);
			this.labelWarningLimit.TabIndex = 57;
			this.labelWarningLimit.Text = "Warning Limit:";
			// 
			// checkBoxWarnNBSP
			// 
			this.checkBoxWarnNBSP.AutoSize = true;
			tableLayoutPanelWarnings.SetColumnSpan(this.checkBoxWarnNBSP, 2);
			this.checkBoxWarnNBSP.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnNBSP", true));
			this.checkBoxWarnNBSP.Location = new System.Drawing.Point(2, 117);
			this.checkBoxWarnNBSP.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxWarnNBSP.Name = "checkBoxWarnNBSP";
			this.checkBoxWarnNBSP.Size = new System.Drawing.Size(172, 19);
			this.checkBoxWarnNBSP.TabIndex = 5;
			this.checkBoxWarnNBSP.Text = "Warn non-breaking Space";
			this.checkBoxWarnNBSP.UseVisualStyleBackColor = true;
			// 
			// checkBoxWarnUnknowCharater
			// 
			this.checkBoxWarnUnknowCharater.AutoSize = true;
			tableLayoutPanelWarnings.SetColumnSpan(this.checkBoxWarnUnknowCharater, 2);
			this.checkBoxWarnUnknowCharater.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnUnknownCharacter", true));
			this.checkBoxWarnUnknowCharater.Location = new System.Drawing.Point(2, 94);
			this.checkBoxWarnUnknowCharater.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxWarnUnknowCharater.Name = "checkBoxWarnUnknowCharater";
			this.checkBoxWarnUnknowCharater.Size = new System.Drawing.Size(185, 19);
			this.checkBoxWarnUnknowCharater.TabIndex = 4;
			this.checkBoxWarnUnknowCharater.Text = "Warn Unknown Characters �";
			this.checkBoxWarnUnknowCharater.UseVisualStyleBackColor = true;
			// 
			// checkBoxWarnDelimiterInValue
			// 
			this.checkBoxWarnDelimiterInValue.AutoSize = true;
			tableLayoutPanelWarnings.SetColumnSpan(this.checkBoxWarnDelimiterInValue, 2);
			this.checkBoxWarnDelimiterInValue.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnDelimiterInValue", true));
			this.checkBoxWarnDelimiterInValue.Location = new System.Drawing.Point(2, 25);
			this.checkBoxWarnDelimiterInValue.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxWarnDelimiterInValue.Name = "checkBoxWarnDelimiterInValue";
			this.checkBoxWarnDelimiterInValue.Size = new System.Drawing.Size(111, 19);
			this.checkBoxWarnDelimiterInValue.TabIndex = 1;
			this.checkBoxWarnDelimiterInValue.Text = "Warn Delimiter";
			this.checkBoxWarnDelimiterInValue.UseVisualStyleBackColor = true;
			// 
			// checkBoxWarnLineFeed
			// 
			this.checkBoxWarnLineFeed.AutoSize = true;
			tableLayoutPanelWarnings.SetColumnSpan(this.checkBoxWarnLineFeed, 2);
			this.checkBoxWarnLineFeed.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnLineFeed", true));
			this.checkBoxWarnLineFeed.Location = new System.Drawing.Point(2, 48);
			this.checkBoxWarnLineFeed.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxWarnLineFeed.Name = "checkBoxWarnLineFeed";
			this.checkBoxWarnLineFeed.Size = new System.Drawing.Size(109, 19);
			this.checkBoxWarnLineFeed.TabIndex = 2;
			this.checkBoxWarnLineFeed.Text = "Warn Linefeed";
			this.checkBoxWarnLineFeed.UseVisualStyleBackColor = true;
			// 
			// checkBoxWarnQuotes
			// 
			this.checkBoxWarnQuotes.AutoSize = true;
			tableLayoutPanelWarnings.SetColumnSpan(this.checkBoxWarnQuotes, 2);
			this.checkBoxWarnQuotes.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnQuotes", true));
			this.checkBoxWarnQuotes.Location = new System.Drawing.Point(2, 71);
			this.checkBoxWarnQuotes.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxWarnQuotes.Name = "checkBoxWarnQuotes";
			this.checkBoxWarnQuotes.Size = new System.Drawing.Size(133, 19);
			this.checkBoxWarnQuotes.TabIndex = 3;
			this.checkBoxWarnQuotes.Text = "Warn Text Qualifier";
			this.checkBoxWarnQuotes.UseVisualStyleBackColor = true;
			// 
			// tabPageFormat
			// 
			this.tabPageFormat.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageFormat.Controls.Add(this.fillGuessSettingEdit);
			this.tabPageFormat.Location = new System.Drawing.Point(4, 22);
			this.tabPageFormat.Margin = new System.Windows.Forms.Padding(2);
			this.tabPageFormat.Name = "tabPageFormat";
			this.tabPageFormat.Padding = new System.Windows.Forms.Padding(2);
			this.tabPageFormat.Size = new System.Drawing.Size(808, 273);
			this.tabPageFormat.TabIndex = 0;
			this.tabPageFormat.Text = "Detect Types";
			// 
			// fillGuessSettingEdit
			// 
			this.fillGuessSettingEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fillGuessSettingEdit.Location = new System.Drawing.Point(2, 2);
			this.fillGuessSettingEdit.Margin = new System.Windows.Forms.Padding(1);
			this.fillGuessSettingEdit.MinimumSize = new System.Drawing.Size(473, 195);
			this.fillGuessSettingEdit.Name = "fillGuessSettingEdit";
			this.fillGuessSettingEdit.Size = new System.Drawing.Size(804, 269);
			this.fillGuessSettingEdit.TabIndex = 101;
			// 
			// tabPageWarnings
			// 
			this.tabPageWarnings.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageWarnings.Controls.Add(tableLayoutPanelWarnings);
			this.tabPageWarnings.Location = new System.Drawing.Point(4, 22);
			this.tabPageWarnings.Margin = new System.Windows.Forms.Padding(2);
			this.tabPageWarnings.Name = "tabPageWarnings";
			this.tabPageWarnings.Size = new System.Drawing.Size(808, 273);
			this.tabPageWarnings.TabIndex = 3;
			this.tabPageWarnings.Text = "Warnings";
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.tabPageFile);
			this.tabControl.Controls.Add(this.tabPageQuoting);
			this.tabControl.Controls.Add(this.tabPageFormat);
			this.tabControl.Controls.Add(this.tabPageBehaviour);
			this.tabControl.Controls.Add(this.tabPageWarnings);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = new System.Drawing.Point(0, 0);
			this.tabControl.Margin = new System.Windows.Forms.Padding(2);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(816, 299);
			this.tabControl.TabIndex = 0;
			// 
			// tabPageFile
			// 
			this.tabPageFile.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageFile.Controls.Add(tableLayoutPanelFile);
			this.tabPageFile.Location = new System.Drawing.Point(4, 22);
			this.tabPageFile.Margin = new System.Windows.Forms.Padding(2);
			this.tabPageFile.Name = "tabPageFile";
			this.tabPageFile.Padding = new System.Windows.Forms.Padding(2);
			this.tabPageFile.Size = new System.Drawing.Size(808, 273);
			this.tabPageFile.TabIndex = 6;
			this.tabPageFile.Text = "File";
			// 
			// tabPageQuoting
			// 
			this.tabPageQuoting.Controls.Add(this.tableLayoutPanel1);
			this.tabPageQuoting.Location = new System.Drawing.Point(4, 22);
			this.tabPageQuoting.Margin = new System.Windows.Forms.Padding(2);
			this.tabPageQuoting.Name = "tabPageQuoting";
			this.tabPageQuoting.Padding = new System.Windows.Forms.Padding(2);
			this.tabPageQuoting.Size = new System.Drawing.Size(808, 273);
			this.tabPageQuoting.TabIndex = 7;
			this.tabPageQuoting.Text = "Text Qualifier";
			this.tabPageQuoting.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
			this.tableLayoutPanel1.ColumnCount = 2;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 46.87189F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 53.12811F));
			this.tableLayoutPanel1.Controls.Add(this.quotingControl, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.checkBoxCheckQuote, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.buttonGuessTextQualifier, 1, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 2);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(804, 269);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// quotingControl
			// 
			this.quotingControl.BackColor = System.Drawing.SystemColors.Control;
			this.tableLayoutPanel1.SetColumnSpan(this.quotingControl, 2);
			this.quotingControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.quotingControl.IsWriteSetting = false;
			this.quotingControl.Location = new System.Drawing.Point(4, 34);
			this.quotingControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.quotingControl.MinimumSize = new System.Drawing.Size(415, 0);
			this.quotingControl.Name = "quotingControl";
			this.quotingControl.Size = new System.Drawing.Size(796, 242);
			this.quotingControl.TabIndex = 3;
			// 
			// checkBoxCheckQuote
			// 
			this.checkBoxCheckQuote.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.checkBoxCheckQuote.AutoSize = true;
			this.checkBoxCheckQuote.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessQualifier", true));
			this.checkBoxCheckQuote.Location = new System.Drawing.Point(2, 5);
			this.checkBoxCheckQuote.Margin = new System.Windows.Forms.Padding(2);
			this.checkBoxCheckQuote.Name = "checkBoxCheckQuote";
			this.checkBoxCheckQuote.Size = new System.Drawing.Size(162, 19);
			this.checkBoxCheckQuote.TabIndex = 10;
			this.checkBoxCheckQuote.Text = "Determine Text Qualifier";
			this.checkBoxCheckQuote.UseVisualStyleBackColor = true;
			// 
			// buttonGuessTextQualifier
			// 
			this.buttonGuessTextQualifier.AutoSize = true;
			this.buttonGuessTextQualifier.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonGuessTextQualifier.Location = new System.Drawing.Point(378, 2);
			this.buttonGuessTextQualifier.Margin = new System.Windows.Forms.Padding(2);
			this.buttonGuessTextQualifier.Name = "buttonGuessTextQualifier";
			this.buttonGuessTextQualifier.Size = new System.Drawing.Size(127, 25);
			this.buttonGuessTextQualifier.TabIndex = 11;
			this.buttonGuessTextQualifier.Text = "Guess Text Qualifier";
			this.buttonGuessTextQualifier.UseVisualStyleBackColor = true;
			this.buttonGuessTextQualifier.Click += new System.EventHandler(this.ButtonGuessTextQualifier_Click);
			// 
			// tabPageBehaviour
			// 
			this.tabPageBehaviour.BackColor = System.Drawing.SystemColors.Control;
			this.tabPageBehaviour.Controls.Add(tableLayoutPanelBehavior);
			this.tabPageBehaviour.Location = new System.Drawing.Point(4, 22);
			this.tabPageBehaviour.Margin = new System.Windows.Forms.Padding(2);
			this.tabPageBehaviour.Name = "tabPageBehaviour";
			this.tabPageBehaviour.Padding = new System.Windows.Forms.Padding(2);
			this.tabPageBehaviour.Size = new System.Drawing.Size(808, 273);
			this.tabPageBehaviour.TabIndex = 9;
			this.tabPageBehaviour.Text = "Behavior";
			// 
			// errorProvider
			// 
			this.errorProvider.ContainerControl = this;
			// 
			// FormEditSettings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(816, 299);
			this.Controls.Add(this.tabControl);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(722, 314);
			this.Name = "FormEditSettings";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Settings";
			this.TopMost = true;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormEditSettings_FormClosing);
			this.Load += new System.EventHandler(this.EditSettings_Load);
			tableLayoutPanelFile.ResumeLayout(false);
			tableLayoutPanelFile.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.fileFormatBindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.fileSettingBindingSource)).EndInit();
			tableLayoutPanelBehavior.ResumeLayout(false);
			tableLayoutPanelBehavior.PerformLayout();
			tableLayoutPanelWarnings.ResumeLayout(false);
			tableLayoutPanelWarnings.PerformLayout();
			this.tabPageFormat.ResumeLayout(false);
			this.tabPageWarnings.ResumeLayout(false);
			this.tabPageWarnings.PerformLayout();
			this.tabControl.ResumeLayout(false);
			this.tabPageFile.ResumeLayout(false);
			this.tabPageFile.PerformLayout();
			this.tabPageQuoting.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.tabPageBehaviour.ResumeLayout(false);
			this.tabPageBehaviour.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
			this.ResumeLayout(false);

    }

    private System.Windows.Forms.Button btnOpenFile;
    private System.Windows.Forms.Button buttonNewLine;
    private System.Windows.Forms.Button buttonSkipLine;
    private System.Windows.Forms.Button buttonGuessCP;
    private System.Windows.Forms.Button buttonGuessDelimiter;
    private System.Windows.Forms.Button buttonGuessTextQualifier;
    private System.Windows.Forms.ComboBox cboCodePage;
    private System.Windows.Forms.ComboBox cboRecordDelimiter;
    private System.Windows.Forms.CheckBox checkBox1;
    private System.Windows.Forms.CheckBox checkBox2;
    private System.Windows.Forms.CheckBox checkBoxAllowRowCombining;
    private System.Windows.Forms.CheckBox checkBoxBOM;
    private System.Windows.Forms.CheckBox checkBoxCheckQuote;
    private System.Windows.Forms.CheckBox checkBoxDetectFileChanges;
    private System.Windows.Forms.CheckBox checkBoxDisplayStartLineNo;
    private System.Windows.Forms.CheckBox checkBoxGuessDelimiter;
    private System.Windows.Forms.CheckBox checkBoxGuessHasHeader;
    private System.Windows.Forms.CheckBox checkBoxHeader;
    private System.Windows.Forms.CheckBox checkBoxMenuDown;
    private System.Windows.Forms.CheckBox checkBoxSkipEmptyLines;
    private System.Windows.Forms.CheckBox checkBoxTreatLFasSpace;
    private System.Windows.Forms.CheckBox checkBoxTreatNBSPAsSpace;
    private System.Windows.Forms.CheckBox checkBoxTreatUnknowCharaterAsSpace;
    private System.Windows.Forms.CheckBox checkBoxTryToSolveMoreColumns;
    private System.Windows.Forms.CheckBox checkBoxWarnDelimiterInValue;
    private System.Windows.Forms.CheckBox checkBoxWarnEmptyTailingColumns;
    private System.Windows.Forms.CheckBox checkBoxWarnLineFeed;
    private System.Windows.Forms.CheckBox checkBoxWarnNBSP;
    private System.Windows.Forms.CheckBox checkBoxWarnQuotes;
    private System.Windows.Forms.CheckBox checkBoxWarnUnknowCharater;
    private System.Windows.Forms.CheckBox chkUseFileSettings;
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.BindingSource fileFormatBindingSource;
    private System.Windows.Forms.BindingSource fileSettingBindingSource;
    private CsvTools.FillGuessSettingEdit fillGuessSettingEdit;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label7;
    private System.Windows.Forms.Label labelCodePage;
    private System.Windows.Forms.Label labelDelimitedFile;
    private System.Windows.Forms.Label labelDelimiter;
    private System.Windows.Forms.Label labelWarningLimit;
    private System.Windows.Forms.Label m_LabelInfoQuoting;
    private CsvTools.QuotingControl quotingControl;
    internal System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.TabPage tabPageBehaviour;
    internal System.Windows.Forms.TabPage tabPageFile;
    private System.Windows.Forms.TabPage tabPageFormat;
    private System.Windows.Forms.TabPage tabPageQuoting;
    private System.Windows.Forms.TabPage tabPageWarnings;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.TextBox textBox2;
    private System.Windows.Forms.TextBox textBox3;
    private System.Windows.Forms.TextBox textBoxComment;
    private System.Windows.Forms.TextBox textBoxDelimiter;
    private System.Windows.Forms.TextBox textBoxFile;
    private System.Windows.Forms.TextBox textBoxNLPlaceholder;
    private System.Windows.Forms.TextBox textBoxNumWarnings;
    private System.Windows.Forms.TextBox textBoxTextAsNull;
    private System.Windows.Forms.ToolTip toolTip;

    #endregion
    private System.Windows.Forms.Label label9;
    private System.Windows.Forms.DomainUpDown domainUpDownTime;
    private System.Windows.Forms.Button buttonGuessHeader;
    private System.Windows.Forms.CheckBox checkBoxGuessCodePage;
  }
}