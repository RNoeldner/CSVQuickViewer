namespace CsvTools
{
  partial class FormEditSettings
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
      System.Windows.Forms.TableLayoutPanel tableLayoutPanelFile;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditSettings));
      System.Windows.Forms.TableLayoutPanel tableLayoutPanelAdvanced;
      System.Windows.Forms.TableLayoutPanel tableLayoutPanelPGP;
      System.Windows.Forms.TableLayoutPanel tableLayoutPanelBehavior;
      System.Windows.Forms.TableLayoutPanel tableLayoutPanelWarnings;
      this.labelDelimitedFile = new System.Windows.Forms.Label();
      this.textBoxComment = new System.Windows.Forms.TextBox();
      this.fileFormatBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.label2 = new System.Windows.Forms.Label();
      this.textBoxFile = new System.Windows.Forms.TextBox();
      this.fileSettingBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.buttonGuessDelimiter = new System.Windows.Forms.Button();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.btnOpenFile = new System.Windows.Forms.Button();
      this.labelDelimiter = new System.Windows.Forms.Label();
      this.textBoxDelimiter = new System.Windows.Forms.TextBox();
      this.checkBoxHeader = new System.Windows.Forms.CheckBox();
      this.labelCodePage = new System.Windows.Forms.Label();
      this.cboCodePage = new System.Windows.Forms.ComboBox();
      this.checkBoxBOM = new System.Windows.Forms.CheckBox();
      this.buttonGuessCP = new System.Windows.Forms.Button();
      this.checkBoxGuessHasHeader = new System.Windows.Forms.CheckBox();
      this.checkBoxGuessCodePage = new System.Windows.Forms.CheckBox();
      this.checkBoxGuessDelimiter = new System.Windows.Forms.CheckBox();
      this.checkBoxCheckQuote = new System.Windows.Forms.CheckBox();
      this.labelSkipFirstLines = new System.Windows.Forms.Label();
      this.labelDelimiterPlaceholer = new System.Windows.Forms.Label();
      this.labelLineFeedPlaceHolder = new System.Windows.Forms.Label();
      this.checkBoxDisplayStartLineNo = new System.Windows.Forms.CheckBox();
      this.textBoxLimitRows = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.textBoxNLPlaceholder = new System.Windows.Forms.TextBox();
      this.textBoxDelimiterPlaceholder = new System.Windows.Forms.TextBox();
      this.labelRecordLimit = new System.Windows.Forms.Label();
      this.textBoxTextAsNull = new System.Windows.Forms.TextBox();
      this.buttonSkipLine = new System.Windows.Forms.Button();
      this.checkBoxGuessStartRow = new System.Windows.Forms.CheckBox();
      this.textBoxSkipRows = new System.Windows.Forms.TextBox();
      this.btnAddPrivKey = new System.Windows.Forms.Button();
      this.listBoxPrivKeys = new System.Windows.Forms.ListBox();
      this.labelPassphrase = new System.Windows.Forms.Label();
      this.btnRemPrivKey = new System.Windows.Forms.Button();
      this.btnPassp = new System.Windows.Forms.Button();
      this.checkBoxDetectFileChanges = new System.Windows.Forms.CheckBox();
      this.checkBoxTreatNBSPAsSpace = new System.Windows.Forms.CheckBox();
      this.checkBoxSkipEmptyLines = new System.Windows.Forms.CheckBox();
      this.checkBoxTreatUnknowCharaterAsSpace = new System.Windows.Forms.CheckBox();
      this.checkBoxMenuDown = new System.Windows.Forms.CheckBox();
      this.checkBoxTreatLFasSpace = new System.Windows.Forms.CheckBox();
      this.chkUseFileSettings = new System.Windows.Forms.CheckBox();
      this.checkBoxTryToSolveMoreColumns = new System.Windows.Forms.CheckBox();
      this.checkBoxAllowRowCombining = new System.Windows.Forms.CheckBox();
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
      this.tabPageAdvanced = new System.Windows.Forms.TabPage();
      this.tabControl = new System.Windows.Forms.TabControl();
      this.tabPageFile = new System.Windows.Forms.TabPage();
      this.tabPageQuoting = new System.Windows.Forms.TabPage();
      this.quotingControl = new CsvTools.QuotingControl();
      this.tabPagePGP = new System.Windows.Forms.TabPage();
      this.label30 = new System.Windows.Forms.Label();
      this.tabPageBehaviour = new System.Windows.Forms.TabPage();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      tableLayoutPanelFile = new System.Windows.Forms.TableLayoutPanel();
      tableLayoutPanelAdvanced = new System.Windows.Forms.TableLayoutPanel();
      tableLayoutPanelPGP = new System.Windows.Forms.TableLayoutPanel();
      tableLayoutPanelBehavior = new System.Windows.Forms.TableLayoutPanel();
      tableLayoutPanelWarnings = new System.Windows.Forms.TableLayoutPanel();
      tableLayoutPanelFile.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.fileFormatBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fileSettingBindingSource)).BeginInit();
      tableLayoutPanelAdvanced.SuspendLayout();
      tableLayoutPanelPGP.SuspendLayout();
      tableLayoutPanelBehavior.SuspendLayout();
      tableLayoutPanelWarnings.SuspendLayout();
      this.tabPageFormat.SuspendLayout();
      this.tabPageWarnings.SuspendLayout();
      this.tabPageAdvanced.SuspendLayout();
      this.tabControl.SuspendLayout();
      this.tabPageFile.SuspendLayout();
      this.tabPageQuoting.SuspendLayout();
      this.tabPagePGP.SuspendLayout();
      this.tabPageBehaviour.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      this.SuspendLayout();
      // 
      // tableLayoutPanelFile
      // 
      tableLayoutPanelFile.AutoSize = true;
      tableLayoutPanelFile.ColumnCount = 6;
      tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
      tableLayoutPanelFile.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanelFile.Controls.Add(this.labelDelimitedFile, 0, 0);
      tableLayoutPanelFile.Controls.Add(this.textBoxComment, 1, 4);
      tableLayoutPanelFile.Controls.Add(this.label2, 0, 4);
      tableLayoutPanelFile.Controls.Add(this.textBoxFile, 1, 0);
      tableLayoutPanelFile.Controls.Add(this.buttonGuessDelimiter, 5, 3);
      tableLayoutPanelFile.Controls.Add(this.textBox1, 3, 3);
      tableLayoutPanelFile.Controls.Add(this.label3, 2, 3);
      tableLayoutPanelFile.Controls.Add(this.btnOpenFile, 5, 0);
      tableLayoutPanelFile.Controls.Add(this.labelDelimiter, 0, 3);
      tableLayoutPanelFile.Controls.Add(this.textBoxDelimiter, 1, 3);
      tableLayoutPanelFile.Controls.Add(this.checkBoxHeader, 1, 1);
      tableLayoutPanelFile.Controls.Add(this.labelCodePage, 0, 2);
      tableLayoutPanelFile.Controls.Add(this.cboCodePage, 1, 2);
      tableLayoutPanelFile.Controls.Add(this.checkBoxBOM, 2, 2);
      tableLayoutPanelFile.Controls.Add(this.buttonGuessCP, 5, 2);
      tableLayoutPanelFile.Controls.Add(this.checkBoxGuessHasHeader, 4, 1);
      tableLayoutPanelFile.Controls.Add(this.checkBoxGuessCodePage, 4, 2);
      tableLayoutPanelFile.Controls.Add(this.checkBoxGuessDelimiter, 4, 3);
      tableLayoutPanelFile.Controls.Add(this.checkBoxCheckQuote, 4, 4);
      tableLayoutPanelFile.Dock = System.Windows.Forms.DockStyle.Top;
      tableLayoutPanelFile.Location = new System.Drawing.Point(3, 3);
      tableLayoutPanelFile.Name = "tableLayoutPanelFile";
      tableLayoutPanelFile.RowCount = 5;
      tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelFile.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelFile.Size = new System.Drawing.Size(1060, 182);
      tableLayoutPanelFile.TabIndex = 48;
      // 
      // labelDelimitedFile
      // 
      this.labelDelimitedFile.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.labelDelimitedFile.AutoSize = true;
      this.labelDelimitedFile.Location = new System.Drawing.Point(11, 10);
      this.labelDelimitedFile.Name = "labelDelimitedFile";
      this.labelDelimitedFile.Size = new System.Drawing.Size(108, 20);
      this.labelDelimitedFile.TabIndex = 39;
      this.labelDelimitedFile.Text = "Delimited File:";
      // 
      // textBoxComment
      // 
      this.textBoxComment.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.textBoxComment.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "CommentLine", true));
      this.textBoxComment.Location = new System.Drawing.Point(125, 153);
      this.textBoxComment.Name = "textBoxComment";
      this.textBoxComment.Size = new System.Drawing.Size(82, 26);
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
      this.label2.Location = new System.Drawing.Point(3, 156);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(116, 20);
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
      this.textBoxFile.Location = new System.Drawing.Point(125, 7);
      this.textBoxFile.Name = "textBoxFile";
      this.textBoxFile.Size = new System.Drawing.Size(550, 26);
      this.textBoxFile.TabIndex = 0;
      this.textBoxFile.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxFile_Validating);
      // 
      // fileSettingBindingSource
      // 
      this.fileSettingBindingSource.AllowNew = false;
      this.fileSettingBindingSource.DataSource = typeof(CsvTools.Properties.ViewSettings);
      // 
      // buttonGuessDelimiter
      // 
      this.buttonGuessDelimiter.AutoSize = true;
      this.buttonGuessDelimiter.Dock = System.Windows.Forms.DockStyle.Top;
      this.buttonGuessDelimiter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.buttonGuessDelimiter.Location = new System.Drawing.Point(815, 113);
      this.buttonGuessDelimiter.Name = "buttonGuessDelimiter";
      this.buttonGuessDelimiter.Size = new System.Drawing.Size(242, 34);
      this.buttonGuessDelimiter.TabIndex = 8;
      this.buttonGuessDelimiter.Text = "   Guess Delimiter";
      this.buttonGuessDelimiter.UseVisualStyleBackColor = true;
      this.buttonGuessDelimiter.Click += new System.EventHandler(this.ButtonGuessDelimiter_Click);
      // 
      // textBox1
      // 
      this.textBox1.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.textBox1.AutoCompleteCustomSource.AddRange(new string[] {
            "\\"});
      this.textBox1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.textBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
      this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "EscapeCharacter", true));
      this.textBox1.Location = new System.Drawing.Point(515, 117);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(94, 26);
      this.textBox1.TabIndex = 45;
      this.toolTip.SetToolTip(this.textBox1, resources.GetString("textBox1.ToolTip"));
      this.textBox1.TextChanged += new System.EventHandler(this.TextBoxDelimiter_TextChanged);
      // 
      // label3
      // 
      this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(442, 120);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(67, 20);
      this.label3.TabIndex = 46;
      this.label3.Text = "Escape:";
      // 
      // btnOpenFile
      // 
      this.btnOpenFile.AutoSize = true;
      this.btnOpenFile.Dock = System.Windows.Forms.DockStyle.Top;
      this.btnOpenFile.Location = new System.Drawing.Point(815, 3);
      this.btnOpenFile.Name = "btnOpenFile";
      this.btnOpenFile.Size = new System.Drawing.Size(242, 34);
      this.btnOpenFile.TabIndex = 1;
      this.btnOpenFile.Text = "Select";
      this.btnOpenFile.UseVisualStyleBackColor = true;
      this.btnOpenFile.Click += new System.EventHandler(this.BtnOpenFile_Click);
      // 
      // labelDelimiter
      // 
      this.labelDelimiter.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.labelDelimiter.AutoSize = true;
      this.labelDelimiter.Location = new System.Drawing.Point(44, 120);
      this.labelDelimiter.Name = "labelDelimiter";
      this.labelDelimiter.Size = new System.Drawing.Size(75, 20);
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
      this.textBoxDelimiter.Location = new System.Drawing.Point(125, 117);
      this.textBoxDelimiter.Name = "textBoxDelimiter";
      this.textBoxDelimiter.Size = new System.Drawing.Size(82, 26);
      this.textBoxDelimiter.TabIndex = 7;
      this.textBoxDelimiter.TextChanged += new System.EventHandler(this.TextBoxDelimiter_TextChanged);
      // 
      // checkBoxHeader
      // 
      this.checkBoxHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxHeader.AutoSize = true;
      tableLayoutPanelFile.SetColumnSpan(this.checkBoxHeader, 2);
      this.checkBoxHeader.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "HasFieldHeader", true));
      this.checkBoxHeader.Location = new System.Drawing.Point(125, 43);
      this.checkBoxHeader.Name = "checkBoxHeader";
      this.checkBoxHeader.Size = new System.Drawing.Size(187, 24);
      this.checkBoxHeader.TabIndex = 2;
      this.checkBoxHeader.Text = "Has Column Headers";
      this.checkBoxHeader.UseVisualStyleBackColor = true;
      // 
      // labelCodePage
      // 
      this.labelCodePage.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.labelCodePage.AutoSize = true;
      this.labelCodePage.Location = new System.Drawing.Point(27, 80);
      this.labelCodePage.Name = "labelCodePage";
      this.labelCodePage.Size = new System.Drawing.Size(92, 20);
      this.labelCodePage.TabIndex = 44;
      this.labelCodePage.Text = "Code Page:";
      // 
      // cboCodePage
      // 
      this.cboCodePage.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.cboCodePage.DisplayMember = "Display";
      this.cboCodePage.FormattingEnabled = true;
      this.cboCodePage.Location = new System.Drawing.Point(125, 76);
      this.cboCodePage.Name = "cboCodePage";
      this.cboCodePage.Size = new System.Drawing.Size(180, 28);
      this.cboCodePage.TabIndex = 4;
      this.cboCodePage.ValueMember = "ID";
      this.cboCodePage.SelectedIndexChanged += new System.EventHandler(this.CboCodePage_SelectedIndexChanged);
      // 
      // checkBoxBOM
      // 
      this.checkBoxBOM.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxBOM.AutoSize = true;
      tableLayoutPanelFile.SetColumnSpan(this.checkBoxBOM, 2);
      this.checkBoxBOM.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "ByteOrderMark", true));
      this.checkBoxBOM.Location = new System.Drawing.Point(442, 78);
      this.checkBoxBOM.Name = "checkBoxBOM";
      this.checkBoxBOM.Size = new System.Drawing.Size(104, 24);
      this.checkBoxBOM.TabIndex = 42;
      this.checkBoxBOM.Text = "Has BOM";
      this.toolTip.SetToolTip(this.checkBoxBOM, "Byte Order Mark");
      this.checkBoxBOM.UseVisualStyleBackColor = true;
      // 
      // buttonGuessCP
      // 
      this.buttonGuessCP.AutoSize = true;
      this.buttonGuessCP.Dock = System.Windows.Forms.DockStyle.Top;
      this.buttonGuessCP.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.buttonGuessCP.Location = new System.Drawing.Point(815, 73);
      this.buttonGuessCP.Name = "buttonGuessCP";
      this.buttonGuessCP.Size = new System.Drawing.Size(242, 34);
      this.buttonGuessCP.TabIndex = 5;
      this.buttonGuessCP.Text = "   Guess Code Page";
      this.buttonGuessCP.UseVisualStyleBackColor = true;
      this.buttonGuessCP.Click += new System.EventHandler(this.ButtonGuessCP_Click);
      // 
      // checkBoxGuessHasHeader
      // 
      this.checkBoxGuessHasHeader.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxGuessHasHeader.AutoSize = true;
      tableLayoutPanelFile.SetColumnSpan(this.checkBoxGuessHasHeader, 2);
      this.checkBoxGuessHasHeader.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessHasHeader", true));
      this.checkBoxGuessHasHeader.Location = new System.Drawing.Point(615, 43);
      this.checkBoxGuessHasHeader.Name = "checkBoxGuessHasHeader";
      this.checkBoxGuessHasHeader.Size = new System.Drawing.Size(251, 24);
      this.checkBoxGuessHasHeader.TabIndex = 3;
      this.checkBoxGuessHasHeader.Text = "Determine if Header is present";
      this.checkBoxGuessHasHeader.UseVisualStyleBackColor = true;
      // 
      // checkBoxGuessCodePage
      // 
      this.checkBoxGuessCodePage.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxGuessCodePage.AutoSize = true;
      this.checkBoxGuessCodePage.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessCodePage", true));
      this.checkBoxGuessCodePage.Location = new System.Drawing.Point(615, 78);
      this.checkBoxGuessCodePage.Name = "checkBoxGuessCodePage";
      this.checkBoxGuessCodePage.Size = new System.Drawing.Size(192, 24);
      this.checkBoxGuessCodePage.TabIndex = 6;
      this.checkBoxGuessCodePage.Text = "Determine Code Page";
      this.checkBoxGuessCodePage.UseVisualStyleBackColor = true;
      // 
      // checkBoxGuessDelimiter
      // 
      this.checkBoxGuessDelimiter.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxGuessDelimiter.AutoSize = true;
      this.checkBoxGuessDelimiter.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessDelimiter", true));
      this.checkBoxGuessDelimiter.Location = new System.Drawing.Point(615, 118);
      this.checkBoxGuessDelimiter.Name = "checkBoxGuessDelimiter";
      this.checkBoxGuessDelimiter.Size = new System.Drawing.Size(175, 24);
      this.checkBoxGuessDelimiter.TabIndex = 9;
      this.checkBoxGuessDelimiter.Text = "Determine Delimiter";
      this.checkBoxGuessDelimiter.UseVisualStyleBackColor = true;
      // 
      // checkBoxCheckQuote
      // 
      this.checkBoxCheckQuote.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxCheckQuote.AutoSize = true;
      tableLayoutPanelFile.SetColumnSpan(this.checkBoxCheckQuote, 2);
      this.checkBoxCheckQuote.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessQualifier", true));
      this.checkBoxCheckQuote.Location = new System.Drawing.Point(615, 154);
      this.checkBoxCheckQuote.Name = "checkBoxCheckQuote";
      this.checkBoxCheckQuote.Size = new System.Drawing.Size(205, 24);
      this.checkBoxCheckQuote.TabIndex = 9;
      this.checkBoxCheckQuote.Text = "Determine Text Qualifier";
      this.checkBoxCheckQuote.UseVisualStyleBackColor = true;
      // 
      // tableLayoutPanelAdvanced
      // 
      tableLayoutPanelAdvanced.AutoSize = true;
      tableLayoutPanelAdvanced.ColumnCount = 4;
      tableLayoutPanelAdvanced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanelAdvanced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanelAdvanced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanelAdvanced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanelAdvanced.Controls.Add(this.labelSkipFirstLines, 0, 0);
      tableLayoutPanelAdvanced.Controls.Add(this.labelDelimiterPlaceholer, 0, 1);
      tableLayoutPanelAdvanced.Controls.Add(this.labelLineFeedPlaceHolder, 0, 2);
      tableLayoutPanelAdvanced.Controls.Add(this.checkBoxDisplayStartLineNo, 1, 5);
      tableLayoutPanelAdvanced.Controls.Add(this.textBoxLimitRows, 1, 4);
      tableLayoutPanelAdvanced.Controls.Add(this.label1, 0, 3);
      tableLayoutPanelAdvanced.Controls.Add(this.textBoxNLPlaceholder, 1, 2);
      tableLayoutPanelAdvanced.Controls.Add(this.textBoxDelimiterPlaceholder, 1, 1);
      tableLayoutPanelAdvanced.Controls.Add(this.labelRecordLimit, 0, 4);
      tableLayoutPanelAdvanced.Controls.Add(this.textBoxTextAsNull, 1, 3);
      tableLayoutPanelAdvanced.Controls.Add(this.buttonSkipLine, 3, 0);
      tableLayoutPanelAdvanced.Controls.Add(this.checkBoxGuessStartRow, 2, 0);
      tableLayoutPanelAdvanced.Controls.Add(this.textBoxSkipRows, 1, 0);
      tableLayoutPanelAdvanced.Dock = System.Windows.Forms.DockStyle.Top;
      tableLayoutPanelAdvanced.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanelAdvanced.Name = "tableLayoutPanelAdvanced";
      tableLayoutPanelAdvanced.RowCount = 6;
      tableLayoutPanelAdvanced.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelAdvanced.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelAdvanced.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelAdvanced.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelAdvanced.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelAdvanced.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelAdvanced.Size = new System.Drawing.Size(1066, 198);
      tableLayoutPanelAdvanced.TabIndex = 120;
      // 
      // labelSkipFirstLines
      // 
      this.labelSkipFirstLines.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.labelSkipFirstLines.AutoSize = true;
      this.labelSkipFirstLines.Location = new System.Drawing.Point(44, 10);
      this.labelSkipFirstLines.Name = "labelSkipFirstLines";
      this.labelSkipFirstLines.Size = new System.Drawing.Size(121, 20);
      this.labelSkipFirstLines.TabIndex = 119;
      this.labelSkipFirstLines.Text = "Skip First Lines:";
      // 
      // labelDelimiterPlaceholer
      // 
      this.labelDelimiterPlaceholer.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.labelDelimiterPlaceholer.AutoSize = true;
      this.labelDelimiterPlaceholer.Location = new System.Drawing.Point(3, 46);
      this.labelDelimiterPlaceholer.Name = "labelDelimiterPlaceholer";
      this.labelDelimiterPlaceholer.Size = new System.Drawing.Size(162, 20);
      this.labelDelimiterPlaceholer.TabIndex = 56;
      this.labelDelimiterPlaceholer.Text = "Delimiter Placeholder:";
      // 
      // labelLineFeedPlaceHolder
      // 
      this.labelLineFeedPlaceHolder.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.labelLineFeedPlaceHolder.AutoSize = true;
      this.labelLineFeedPlaceHolder.Location = new System.Drawing.Point(3, 78);
      this.labelLineFeedPlaceHolder.Name = "labelLineFeedPlaceHolder";
      this.labelLineFeedPlaceHolder.Size = new System.Drawing.Size(162, 20);
      this.labelLineFeedPlaceHolder.TabIndex = 55;
      this.labelLineFeedPlaceHolder.Text = "Linefeed Placeholder:\r\n";
      // 
      // checkBoxDisplayStartLineNo
      // 
      this.checkBoxDisplayStartLineNo.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxDisplayStartLineNo.AutoSize = true;
      this.checkBoxDisplayStartLineNo.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.checkBoxDisplayStartLineNo.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "DisplayStartLineNo", true));
      this.checkBoxDisplayStartLineNo.Location = new System.Drawing.Point(171, 171);
      this.checkBoxDisplayStartLineNo.Name = "checkBoxDisplayStartLineNo";
      this.checkBoxDisplayStartLineNo.Size = new System.Drawing.Size(239, 24);
      this.checkBoxDisplayStartLineNo.TabIndex = 7;
      this.checkBoxDisplayStartLineNo.Text = "Add Column for Line Number";
      this.checkBoxDisplayStartLineNo.UseVisualStyleBackColor = true;
      // 
      // textBoxLimitRows
      // 
      this.textBoxLimitRows.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.textBoxLimitRows.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "RecordLimit", true));
      this.textBoxLimitRows.Location = new System.Drawing.Point(171, 139);
      this.textBoxLimitRows.Name = "textBoxLimitRows";
      this.textBoxLimitRows.Size = new System.Drawing.Size(0, 26);
      this.textBoxLimitRows.TabIndex = 6;
      this.textBoxLimitRows.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.textBoxLimitRows.Validating += new System.ComponentModel.CancelEventHandler(this.PositiveNumberValidating);
      // 
      // label1
      // 
      this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(15, 110);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(150, 20);
      this.label1.TabIndex = 108;
      this.label1.Text = "Treat Text as NULL:";
      // 
      // textBoxNLPlaceholder
      // 
      this.textBoxNLPlaceholder.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.textBoxNLPlaceholder.AutoCompleteCustomSource.AddRange(new string[] {
            "<br>",
            "{n}"});
      this.textBoxNLPlaceholder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      this.textBoxNLPlaceholder.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "NewLinePlaceholder", true));
      this.textBoxNLPlaceholder.Location = new System.Drawing.Point(171, 75);
      this.textBoxNLPlaceholder.Name = "textBoxNLPlaceholder";
      this.textBoxNLPlaceholder.Size = new System.Drawing.Size(0, 26);
      this.textBoxNLPlaceholder.TabIndex = 4;
      // 
      // textBoxDelimiterPlaceholder
      // 
      this.textBoxDelimiterPlaceholder.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.textBoxDelimiterPlaceholder.AutoCompleteCustomSource.AddRange(new string[] {
            "{d}"});
      this.textBoxDelimiterPlaceholder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      this.textBoxDelimiterPlaceholder.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "DelimiterPlaceholder", true));
      this.textBoxDelimiterPlaceholder.Location = new System.Drawing.Point(171, 43);
      this.textBoxDelimiterPlaceholder.Name = "textBoxDelimiterPlaceholder";
      this.textBoxDelimiterPlaceholder.Size = new System.Drawing.Size(0, 26);
      this.textBoxDelimiterPlaceholder.TabIndex = 3;
      // 
      // labelRecordLimit
      // 
      this.labelRecordLimit.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.labelRecordLimit.AutoSize = true;
      this.labelRecordLimit.Location = new System.Drawing.Point(63, 142);
      this.labelRecordLimit.Name = "labelRecordLimit";
      this.labelRecordLimit.Size = new System.Drawing.Size(102, 20);
      this.labelRecordLimit.TabIndex = 112;
      this.labelRecordLimit.Text = "Record Limit:";
      // 
      // textBoxTextAsNull
      // 
      this.textBoxTextAsNull.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.textBoxTextAsNull.AutoCompleteCustomSource.AddRange(new string[] {
            "NULL",
            "n.a.",
            "n/a"});
      this.textBoxTextAsNull.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "TreatTextAsNull", true));
      this.textBoxTextAsNull.Location = new System.Drawing.Point(171, 107);
      this.textBoxTextAsNull.Name = "textBoxTextAsNull";
      this.textBoxTextAsNull.Size = new System.Drawing.Size(0, 26);
      this.textBoxTextAsNull.TabIndex = 5;
      // 
      // buttonSkipLine
      // 
      this.buttonSkipLine.AutoSize = true;
      this.buttonSkipLine.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.buttonSkipLine.Location = new System.Drawing.Point(833, 3);
      this.buttonSkipLine.Name = "buttonSkipLine";
      this.buttonSkipLine.Size = new System.Drawing.Size(230, 34);
      this.buttonSkipLine.TabIndex = 1;
      this.buttonSkipLine.Text = "   Guess Start Row";
      this.buttonSkipLine.UseVisualStyleBackColor = true;
      this.buttonSkipLine.Click += new System.EventHandler(this.ButtonSkipLine_Click);
      // 
      // checkBoxGuessStartRow
      // 
      this.checkBoxGuessStartRow.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxGuessStartRow.AutoSize = true;
      this.checkBoxGuessStartRow.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessStartRow", true));
      this.checkBoxGuessStartRow.Location = new System.Drawing.Point(643, 8);
      this.checkBoxGuessStartRow.Name = "checkBoxGuessStartRow";
      this.checkBoxGuessStartRow.Size = new System.Drawing.Size(184, 24);
      this.checkBoxGuessStartRow.TabIndex = 2;
      this.checkBoxGuessStartRow.Text = "Determine Start Row";
      this.checkBoxGuessStartRow.UseVisualStyleBackColor = true;
      // 
      // textBoxSkipRows
      // 
      this.textBoxSkipRows.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "SkipRows", true));
      this.textBoxSkipRows.Dock = System.Windows.Forms.DockStyle.Left;
      this.textBoxSkipRows.Location = new System.Drawing.Point(171, 3);
      this.textBoxSkipRows.Name = "textBoxSkipRows";
      this.textBoxSkipRows.Size = new System.Drawing.Size(0, 26);
      this.textBoxSkipRows.TabIndex = 0;
      this.textBoxSkipRows.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.textBoxSkipRows.Validating += new System.ComponentModel.CancelEventHandler(this.PositiveNumberValidating);
      // 
      // tableLayoutPanelPGP
      // 
      tableLayoutPanelPGP.AutoSize = true;
      tableLayoutPanelPGP.ColumnCount = 3;
      tableLayoutPanelPGP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanelPGP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanelPGP.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanelPGP.Controls.Add(this.btnAddPrivKey, 1, 0);
      tableLayoutPanelPGP.Controls.Add(this.listBoxPrivKeys, 0, 0);
      tableLayoutPanelPGP.Controls.Add(this.labelPassphrase, 2, 1);
      tableLayoutPanelPGP.Controls.Add(this.btnRemPrivKey, 1, 1);
      tableLayoutPanelPGP.Controls.Add(this.btnPassp, 2, 0);
      tableLayoutPanelPGP.Dock = System.Windows.Forms.DockStyle.Top;
      tableLayoutPanelPGP.Location = new System.Drawing.Point(3, 3);
      tableLayoutPanelPGP.Name = "tableLayoutPanelPGP";
      tableLayoutPanelPGP.RowCount = 3;
      tableLayoutPanelPGP.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelPGP.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelPGP.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanelPGP.Size = new System.Drawing.Size(1060, 310);
      tableLayoutPanelPGP.TabIndex = 124;
      // 
      // btnAddPrivKey
      // 
      this.btnAddPrivKey.AutoSize = true;
      this.btnAddPrivKey.Location = new System.Drawing.Point(385, 3);
      this.btnAddPrivKey.Name = "btnAddPrivKey";
      this.btnAddPrivKey.Size = new System.Drawing.Size(240, 34);
      this.btnAddPrivKey.TabIndex = 0;
      this.btnAddPrivKey.Text = "Add Private Key";
      this.btnAddPrivKey.UseVisualStyleBackColor = true;
      this.btnAddPrivKey.Click += new System.EventHandler(this.BtnAddPrivKey_Click);
      // 
      // listBoxPrivKeys
      // 
      this.listBoxPrivKeys.Dock = System.Windows.Forms.DockStyle.Top;
      this.listBoxPrivKeys.FormattingEnabled = true;
      this.listBoxPrivKeys.ItemHeight = 20;
      this.listBoxPrivKeys.Location = new System.Drawing.Point(3, 3);
      this.listBoxPrivKeys.Name = "listBoxPrivKeys";
      tableLayoutPanelPGP.SetRowSpan(this.listBoxPrivKeys, 3);
      this.listBoxPrivKeys.Size = new System.Drawing.Size(376, 304);
      this.listBoxPrivKeys.TabIndex = 122;
      // 
      // labelPassphrase
      // 
      this.labelPassphrase.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.labelPassphrase.AutoSize = true;
      this.labelPassphrase.BackColor = System.Drawing.SystemColors.Info;
      this.labelPassphrase.ForeColor = System.Drawing.SystemColors.InfoText;
      this.labelPassphrase.Location = new System.Drawing.Point(631, 50);
      this.labelPassphrase.Name = "labelPassphrase";
      this.labelPassphrase.Size = new System.Drawing.Size(201, 20);
      this.labelPassphrase.TabIndex = 123;
      this.labelPassphrase.Text = "A default passphrase is set";
      // 
      // btnRemPrivKey
      // 
      this.btnRemPrivKey.AutoSize = true;
      this.btnRemPrivKey.Location = new System.Drawing.Point(385, 43);
      this.btnRemPrivKey.Name = "btnRemPrivKey";
      this.btnRemPrivKey.Size = new System.Drawing.Size(240, 34);
      this.btnRemPrivKey.TabIndex = 2;
      this.btnRemPrivKey.Text = "Remove Private Key";
      this.btnRemPrivKey.UseVisualStyleBackColor = true;
      this.btnRemPrivKey.Click += new System.EventHandler(this.BtnRemPrivKey_Click);
      // 
      // btnPassp
      // 
      this.btnPassp.AutoSize = true;
      this.btnPassp.Location = new System.Drawing.Point(631, 3);
      this.btnPassp.Name = "btnPassp";
      this.btnPassp.Size = new System.Drawing.Size(402, 34);
      this.btnPassp.TabIndex = 1;
      this.btnPassp.Text = "Set Default Decryption Passphrase";
      this.btnPassp.UseVisualStyleBackColor = true;
      this.btnPassp.Click += new System.EventHandler(this.BtnPassp_Click);
      // 
      // tableLayoutPanelBehavior
      // 
      tableLayoutPanelBehavior.AutoSize = true;
      tableLayoutPanelBehavior.ColumnCount = 1;
      tableLayoutPanelBehavior.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanelBehavior.Controls.Add(this.checkBoxDetectFileChanges, 0, 0);
      tableLayoutPanelBehavior.Controls.Add(this.checkBoxTreatNBSPAsSpace, 0, 8);
      tableLayoutPanelBehavior.Controls.Add(this.checkBoxSkipEmptyLines, 0, 3);
      tableLayoutPanelBehavior.Controls.Add(this.checkBoxTreatUnknowCharaterAsSpace, 0, 7);
      tableLayoutPanelBehavior.Controls.Add(this.checkBoxMenuDown, 0, 1);
      tableLayoutPanelBehavior.Controls.Add(this.checkBoxTreatLFasSpace, 0, 6);
      tableLayoutPanelBehavior.Controls.Add(this.chkUseFileSettings, 0, 2);
      tableLayoutPanelBehavior.Controls.Add(this.checkBoxTryToSolveMoreColumns, 0, 5);
      tableLayoutPanelBehavior.Controls.Add(this.checkBoxAllowRowCombining, 0, 4);
      tableLayoutPanelBehavior.Dock = System.Windows.Forms.DockStyle.Top;
      tableLayoutPanelBehavior.Location = new System.Drawing.Point(3, 3);
      tableLayoutPanelBehavior.Name = "tableLayoutPanelBehavior";
      tableLayoutPanelBehavior.RowCount = 9;
      tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelBehavior.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelBehavior.Size = new System.Drawing.Size(1060, 270);
      tableLayoutPanelBehavior.TabIndex = 9;
      // 
      // checkBoxDetectFileChanges
      // 
      this.checkBoxDetectFileChanges.AutoSize = true;
      this.checkBoxDetectFileChanges.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "DetectFileChanges", true));
      this.checkBoxDetectFileChanges.Location = new System.Drawing.Point(3, 3);
      this.checkBoxDetectFileChanges.Name = "checkBoxDetectFileChanges";
      this.checkBoxDetectFileChanges.Size = new System.Drawing.Size(180, 24);
      this.checkBoxDetectFileChanges.TabIndex = 0;
      this.checkBoxDetectFileChanges.Text = "Detect File Changes";
      this.checkBoxDetectFileChanges.UseVisualStyleBackColor = true;
      // 
      // checkBoxTreatNBSPAsSpace
      // 
      this.checkBoxTreatNBSPAsSpace.AutoSize = true;
      this.checkBoxTreatNBSPAsSpace.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "TreatNBSPAsSpace", true));
      this.checkBoxTreatNBSPAsSpace.Location = new System.Drawing.Point(3, 243);
      this.checkBoxTreatNBSPAsSpace.Name = "checkBoxTreatNBSPAsSpace";
      this.checkBoxTreatNBSPAsSpace.Size = new System.Drawing.Size(290, 24);
      this.checkBoxTreatNBSPAsSpace.TabIndex = 8;
      this.checkBoxTreatNBSPAsSpace.Text = "Treat non-breaking Space as Space";
      this.toolTip.SetToolTip(this.checkBoxTreatNBSPAsSpace, "Threat any non-breaking space like a regular space");
      this.checkBoxTreatNBSPAsSpace.UseVisualStyleBackColor = true;
      // 
      // checkBoxSkipEmptyLines
      // 
      this.checkBoxSkipEmptyLines.AutoSize = true;
      this.checkBoxSkipEmptyLines.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "SkipEmptyLines", true));
      this.checkBoxSkipEmptyLines.Location = new System.Drawing.Point(3, 93);
      this.checkBoxSkipEmptyLines.Name = "checkBoxSkipEmptyLines";
      this.checkBoxSkipEmptyLines.Size = new System.Drawing.Size(157, 24);
      this.checkBoxSkipEmptyLines.TabIndex = 3;
      this.checkBoxSkipEmptyLines.Text = "Skip Empty Lines";
      this.checkBoxSkipEmptyLines.UseVisualStyleBackColor = true;
      // 
      // checkBoxTreatUnknowCharaterAsSpace
      // 
      this.checkBoxTreatUnknowCharaterAsSpace.AutoSize = true;
      this.checkBoxTreatUnknowCharaterAsSpace.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.checkBoxTreatUnknowCharaterAsSpace.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "TreatUnknowCharaterAsSpace", true));
      this.checkBoxTreatUnknowCharaterAsSpace.Location = new System.Drawing.Point(3, 213);
      this.checkBoxTreatUnknowCharaterAsSpace.Name = "checkBoxTreatUnknowCharaterAsSpace";
      this.checkBoxTreatUnknowCharaterAsSpace.Size = new System.Drawing.Size(302, 24);
      this.checkBoxTreatUnknowCharaterAsSpace.TabIndex = 7;
      this.checkBoxTreatUnknowCharaterAsSpace.Text = "Treat Unknown Character � as Space";
      this.toolTip.SetToolTip(this.checkBoxTreatUnknowCharaterAsSpace, "Threat any unknown character like a space");
      this.checkBoxTreatUnknowCharaterAsSpace.UseVisualStyleBackColor = true;
      // 
      // checkBoxMenuDown
      // 
      this.checkBoxMenuDown.AutoSize = true;
      this.checkBoxMenuDown.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "MenuDown", true));
      this.checkBoxMenuDown.Location = new System.Drawing.Point(3, 33);
      this.checkBoxMenuDown.Name = "checkBoxMenuDown";
      this.checkBoxMenuDown.Size = new System.Drawing.Size(266, 24);
      this.checkBoxMenuDown.TabIndex = 1;
      this.checkBoxMenuDown.Text = "Display Actions in Navigation Bar";
      this.checkBoxMenuDown.UseVisualStyleBackColor = true;
      // 
      // checkBoxTreatLFasSpace
      // 
      this.checkBoxTreatLFasSpace.AutoSize = true;
      this.checkBoxTreatLFasSpace.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "TreatLFAsSpace", true));
      this.checkBoxTreatLFasSpace.Location = new System.Drawing.Point(3, 183);
      this.checkBoxTreatLFasSpace.Name = "checkBoxTreatLFasSpace";
      this.checkBoxTreatLFasSpace.Size = new System.Drawing.Size(166, 24);
      this.checkBoxTreatLFasSpace.TabIndex = 6;
      this.checkBoxTreatLFasSpace.Text = "Treat LF as Space";
      this.toolTip.SetToolTip(this.checkBoxTreatLFasSpace, "Threat a single occurrence of a LF as a space");
      this.checkBoxTreatLFasSpace.UseVisualStyleBackColor = true;
      // 
      // chkUseFileSettings
      // 
      this.chkUseFileSettings.AutoSize = true;
      this.chkUseFileSettings.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "StoreSettingsByFile", true));
      this.chkUseFileSettings.Location = new System.Drawing.Point(3, 63);
      this.chkUseFileSettings.Name = "chkUseFileSettings";
      this.chkUseFileSettings.Size = new System.Drawing.Size(198, 24);
      this.chkUseFileSettings.TabIndex = 2;
      this.chkUseFileSettings.Text = "Persist Settings for File";
      this.toolTip.SetToolTip(this.chkUseFileSettings, "Store the settings for each individual file, do not use this is structure or form" +
        "atting of columns does change over time");
      this.chkUseFileSettings.UseVisualStyleBackColor = true;
      // 
      // checkBoxTryToSolveMoreColumns
      // 
      this.checkBoxTryToSolveMoreColumns.AutoSize = true;
      this.checkBoxTryToSolveMoreColumns.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "TryToSolveMoreColumns", true));
      this.checkBoxTryToSolveMoreColumns.Location = new System.Drawing.Point(3, 153);
      this.checkBoxTryToSolveMoreColumns.Name = "checkBoxTryToSolveMoreColumns";
      this.checkBoxTryToSolveMoreColumns.Size = new System.Drawing.Size(202, 24);
      this.checkBoxTryToSolveMoreColumns.TabIndex = 5;
      this.checkBoxTryToSolveMoreColumns.Text = "Try to Re-Align columns";
      this.toolTip.SetToolTip(this.checkBoxTryToSolveMoreColumns, "Try to realign columns in case the file is not quoted and an extra delimiter has " +
        "caused additional columns\r\nThis is a very risky option, as the alignment wight w" +
        "ell be wrong.");
      this.checkBoxTryToSolveMoreColumns.UseVisualStyleBackColor = true;
      // 
      // checkBoxAllowRowCombining
      // 
      this.checkBoxAllowRowCombining.AutoSize = true;
      this.checkBoxAllowRowCombining.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "AllowRowCombining", true));
      this.checkBoxAllowRowCombining.Location = new System.Drawing.Point(3, 123);
      this.checkBoxAllowRowCombining.Name = "checkBoxAllowRowCombining";
      this.checkBoxAllowRowCombining.Size = new System.Drawing.Size(177, 24);
      this.checkBoxAllowRowCombining.TabIndex = 4;
      this.checkBoxAllowRowCombining.Text = "Try to Combine Row";
      this.toolTip.SetToolTip(this.checkBoxAllowRowCombining, "Try to combine rows, this might happen if the column does contain a linefeed and " +
        "is not quoted.\r\nThis is a very risky option, in some cases rows might be lost.");
      this.checkBoxAllowRowCombining.UseVisualStyleBackColor = true;
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
      tableLayoutPanelWarnings.Name = "tableLayoutPanelWarnings";
      tableLayoutPanelWarnings.RowCount = 7;
      tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelWarnings.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanelWarnings.Size = new System.Drawing.Size(1066, 212);
      tableLayoutPanelWarnings.TabIndex = 58;
      // 
      // checkBoxWarnEmptyTailingColumns
      // 
      this.checkBoxWarnEmptyTailingColumns.AutoSize = true;
      tableLayoutPanelWarnings.SetColumnSpan(this.checkBoxWarnEmptyTailingColumns, 2);
      this.checkBoxWarnEmptyTailingColumns.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnEmptyTailingColumns", true));
      this.checkBoxWarnEmptyTailingColumns.Location = new System.Drawing.Point(3, 3);
      this.checkBoxWarnEmptyTailingColumns.Name = "checkBoxWarnEmptyTailingColumns";
      this.checkBoxWarnEmptyTailingColumns.Size = new System.Drawing.Size(193, 24);
      this.checkBoxWarnEmptyTailingColumns.TabIndex = 0;
      this.checkBoxWarnEmptyTailingColumns.Text = "Warn Trailing Columns";
      this.checkBoxWarnEmptyTailingColumns.UseVisualStyleBackColor = false;
      // 
      // textBoxNumWarnings
      // 
      this.textBoxNumWarnings.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "NumWarnings", true));
      this.textBoxNumWarnings.Location = new System.Drawing.Point(118, 183);
      this.textBoxNumWarnings.Name = "textBoxNumWarnings";
      this.textBoxNumWarnings.Size = new System.Drawing.Size(44, 26);
      this.textBoxNumWarnings.TabIndex = 6;
      this.textBoxNumWarnings.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.textBoxNumWarnings.Validating += new System.ComponentModel.CancelEventHandler(this.PositiveNumberValidating);
      // 
      // labelWarningLimit
      // 
      this.labelWarningLimit.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.labelWarningLimit.AutoSize = true;
      this.labelWarningLimit.Location = new System.Drawing.Point(3, 186);
      this.labelWarningLimit.Name = "labelWarningLimit";
      this.labelWarningLimit.Size = new System.Drawing.Size(109, 20);
      this.labelWarningLimit.TabIndex = 57;
      this.labelWarningLimit.Text = "Warning Limit:";
      // 
      // checkBoxWarnNBSP
      // 
      this.checkBoxWarnNBSP.AutoSize = true;
      tableLayoutPanelWarnings.SetColumnSpan(this.checkBoxWarnNBSP, 2);
      this.checkBoxWarnNBSP.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnNBSP", true));
      this.checkBoxWarnNBSP.Location = new System.Drawing.Point(3, 153);
      this.checkBoxWarnNBSP.Name = "checkBoxWarnNBSP";
      this.checkBoxWarnNBSP.Size = new System.Drawing.Size(220, 24);
      this.checkBoxWarnNBSP.TabIndex = 5;
      this.checkBoxWarnNBSP.Text = "Warn non-breaking Space";
      this.checkBoxWarnNBSP.UseVisualStyleBackColor = true;
      // 
      // checkBoxWarnUnknowCharater
      // 
      this.checkBoxWarnUnknowCharater.AutoSize = true;
      tableLayoutPanelWarnings.SetColumnSpan(this.checkBoxWarnUnknowCharater, 2);
      this.checkBoxWarnUnknowCharater.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnUnknowCharater", true));
      this.checkBoxWarnUnknowCharater.Location = new System.Drawing.Point(3, 123);
      this.checkBoxWarnUnknowCharater.Name = "checkBoxWarnUnknowCharater";
      this.checkBoxWarnUnknowCharater.Size = new System.Drawing.Size(240, 24);
      this.checkBoxWarnUnknowCharater.TabIndex = 4;
      this.checkBoxWarnUnknowCharater.Text = "Warn Unknown Characters �";
      this.checkBoxWarnUnknowCharater.UseVisualStyleBackColor = true;
      // 
      // checkBoxWarnDelimiterInValue
      // 
      this.checkBoxWarnDelimiterInValue.AutoSize = true;
      tableLayoutPanelWarnings.SetColumnSpan(this.checkBoxWarnDelimiterInValue, 2);
      this.checkBoxWarnDelimiterInValue.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnDelimiterInValue", true));
      this.checkBoxWarnDelimiterInValue.Location = new System.Drawing.Point(3, 33);
      this.checkBoxWarnDelimiterInValue.Name = "checkBoxWarnDelimiterInValue";
      this.checkBoxWarnDelimiterInValue.Size = new System.Drawing.Size(139, 24);
      this.checkBoxWarnDelimiterInValue.TabIndex = 1;
      this.checkBoxWarnDelimiterInValue.Text = "Warn Delimiter";
      this.checkBoxWarnDelimiterInValue.UseVisualStyleBackColor = true;
      // 
      // checkBoxWarnLineFeed
      // 
      this.checkBoxWarnLineFeed.AutoSize = true;
      tableLayoutPanelWarnings.SetColumnSpan(this.checkBoxWarnLineFeed, 2);
      this.checkBoxWarnLineFeed.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnLineFeed", true));
      this.checkBoxWarnLineFeed.Location = new System.Drawing.Point(3, 63);
      this.checkBoxWarnLineFeed.Name = "checkBoxWarnLineFeed";
      this.checkBoxWarnLineFeed.Size = new System.Drawing.Size(139, 24);
      this.checkBoxWarnLineFeed.TabIndex = 2;
      this.checkBoxWarnLineFeed.Text = "Warn Linefeed";
      this.checkBoxWarnLineFeed.UseVisualStyleBackColor = true;
      // 
      // checkBoxWarnQuotes
      // 
      this.checkBoxWarnQuotes.AutoSize = true;
      tableLayoutPanelWarnings.SetColumnSpan(this.checkBoxWarnQuotes, 2);
      this.checkBoxWarnQuotes.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnQuotes", true));
      this.checkBoxWarnQuotes.Location = new System.Drawing.Point(3, 93);
      this.checkBoxWarnQuotes.Name = "checkBoxWarnQuotes";
      this.checkBoxWarnQuotes.Size = new System.Drawing.Size(169, 24);
      this.checkBoxWarnQuotes.TabIndex = 3;
      this.checkBoxWarnQuotes.Text = "Warn Text Qualifier";
      this.checkBoxWarnQuotes.UseVisualStyleBackColor = true;
      // 
      // tabPageFormat
      // 
      this.tabPageFormat.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageFormat.Controls.Add(this.fillGuessSettingEdit);
      this.tabPageFormat.Location = new System.Drawing.Point(4, 29);
      this.tabPageFormat.Name = "tabPageFormat";
      this.tabPageFormat.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this.tabPageFormat.Size = new System.Drawing.Size(1066, 416);
      this.tabPageFormat.TabIndex = 0;
      this.tabPageFormat.Text = "Detect Types";
      // 
      // fillGuessSettingEdit
      // 
      this.fillGuessSettingEdit.Dock = System.Windows.Forms.DockStyle.Fill;
      this.fillGuessSettingEdit.Location = new System.Drawing.Point(3, 3);
      this.fillGuessSettingEdit.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.fillGuessSettingEdit.MinimumSize = new System.Drawing.Size(710, 300);
      this.fillGuessSettingEdit.Name = "fillGuessSettingEdit";
      this.fillGuessSettingEdit.Size = new System.Drawing.Size(1060, 410);
      this.fillGuessSettingEdit.TabIndex = 101;
      // 
      // tabPageWarnings
      // 
      this.tabPageWarnings.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageWarnings.Controls.Add(tableLayoutPanelWarnings);
      this.tabPageWarnings.Location = new System.Drawing.Point(4, 29);
      this.tabPageWarnings.Name = "tabPageWarnings";
      this.tabPageWarnings.Size = new System.Drawing.Size(1066, 416);
      this.tabPageWarnings.TabIndex = 3;
      this.tabPageWarnings.Text = "Warnings";
      // 
      // tabPageAdvanced
      // 
      this.tabPageAdvanced.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageAdvanced.Controls.Add(tableLayoutPanelAdvanced);
      this.tabPageAdvanced.Location = new System.Drawing.Point(4, 29);
      this.tabPageAdvanced.Name = "tabPageAdvanced";
      this.tabPageAdvanced.Size = new System.Drawing.Size(1066, 416);
      this.tabPageAdvanced.TabIndex = 2;
      this.tabPageAdvanced.Text = "Advanced";
      // 
      // tabControl
      // 
      this.tabControl.Controls.Add(this.tabPageFile);
      this.tabControl.Controls.Add(this.tabPageQuoting);
      this.tabControl.Controls.Add(this.tabPageAdvanced);
      this.tabControl.Controls.Add(this.tabPageFormat);
      this.tabControl.Controls.Add(this.tabPagePGP);
      this.tabControl.Controls.Add(this.tabPageBehaviour);
      this.tabControl.Controls.Add(this.tabPageWarnings);
      this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tabControl.Location = new System.Drawing.Point(0, 0);
      this.tabControl.Name = "tabControl";
      this.tabControl.SelectedIndex = 0;
      this.tabControl.Size = new System.Drawing.Size(1074, 449);
      this.tabControl.TabIndex = 0;
      // 
      // tabPageFile
      // 
      this.tabPageFile.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageFile.Controls.Add(tableLayoutPanelFile);
      this.tabPageFile.Location = new System.Drawing.Point(4, 29);
      this.tabPageFile.Name = "tabPageFile";
      this.tabPageFile.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this.tabPageFile.Size = new System.Drawing.Size(1066, 416);
      this.tabPageFile.TabIndex = 6;
      this.tabPageFile.Text = "File";
      // 
      // tabPageQuoting
      // 
      this.tabPageQuoting.Controls.Add(this.quotingControl);
      this.tabPageQuoting.Location = new System.Drawing.Point(4, 29);
      this.tabPageQuoting.Name = "tabPageQuoting";
      this.tabPageQuoting.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this.tabPageQuoting.Size = new System.Drawing.Size(1066, 416);
      this.tabPageQuoting.TabIndex = 7;
      this.tabPageQuoting.Text = "Text Qualifier";
      this.tabPageQuoting.UseVisualStyleBackColor = true;
      // 
      // quotingControl
      // 
      this.quotingControl.BackColor = System.Drawing.SystemColors.Control;
      this.quotingControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.quotingControl.IsWriteSetting = false;
      this.quotingControl.Location = new System.Drawing.Point(3, 3);
      this.quotingControl.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
      this.quotingControl.MinimumSize = new System.Drawing.Size(622, 0);
      this.quotingControl.Name = "quotingControl";
      this.quotingControl.Size = new System.Drawing.Size(1060, 410);
      this.quotingControl.TabIndex = 2;
      // 
      // tabPagePGP
      // 
      this.tabPagePGP.BackColor = System.Drawing.SystemColors.Control;
      this.tabPagePGP.Controls.Add(tableLayoutPanelPGP);
      this.tabPagePGP.Controls.Add(this.label30);
      this.tabPagePGP.Location = new System.Drawing.Point(4, 29);
      this.tabPagePGP.Name = "tabPagePGP";
      this.tabPagePGP.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this.tabPagePGP.Size = new System.Drawing.Size(1066, 416);
      this.tabPagePGP.TabIndex = 8;
      this.tabPagePGP.Text = "PGP";
      // 
      // label30
      // 
      this.label30.AutoSize = true;
      this.label30.Location = new System.Drawing.Point(6, 5);
      this.label30.Name = "label30";
      this.label30.Size = new System.Drawing.Size(198, 20);
      this.label30.TabIndex = 120;
      this.label30.Text = "Private Keys for Decryption";
      // 
      // tabPageBehaviour
      // 
      this.tabPageBehaviour.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageBehaviour.Controls.Add(tableLayoutPanelBehavior);
      this.tabPageBehaviour.Location = new System.Drawing.Point(4, 29);
      this.tabPageBehaviour.Name = "tabPageBehaviour";
      this.tabPageBehaviour.Padding = new System.Windows.Forms.Padding(3, 3, 3, 3);
      this.tabPageBehaviour.Size = new System.Drawing.Size(1066, 416);
      this.tabPageBehaviour.TabIndex = 9;
      this.tabPageBehaviour.Text = "Behavior";
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // FormEditSettings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1074, 449);
      this.Controls.Add(this.tabControl);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(1078, 471);
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
      tableLayoutPanelAdvanced.ResumeLayout(false);
      tableLayoutPanelAdvanced.PerformLayout();
      tableLayoutPanelPGP.ResumeLayout(false);
      tableLayoutPanelPGP.PerformLayout();
      tableLayoutPanelBehavior.ResumeLayout(false);
      tableLayoutPanelBehavior.PerformLayout();
      tableLayoutPanelWarnings.ResumeLayout(false);
      tableLayoutPanelWarnings.PerformLayout();
      this.tabPageFormat.ResumeLayout(false);
      this.tabPageWarnings.ResumeLayout(false);
      this.tabPageWarnings.PerformLayout();
      this.tabPageAdvanced.ResumeLayout(false);
      this.tabPageAdvanced.PerformLayout();
      this.tabControl.ResumeLayout(false);
      this.tabPageFile.ResumeLayout(false);
      this.tabPageFile.PerformLayout();
      this.tabPageQuoting.ResumeLayout(false);
      this.tabPagePGP.ResumeLayout(false);
      this.tabPagePGP.PerformLayout();
      this.tabPageBehaviour.ResumeLayout(false);
      this.tabPageBehaviour.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion
    private System.Windows.Forms.TabPage tabPageFormat;
    private FillGuessSettingEdit fillGuessSettingEdit;
    private System.Windows.Forms.TabPage tabPageWarnings;
    private System.Windows.Forms.CheckBox checkBoxWarnUnknowCharater;
    private System.Windows.Forms.CheckBox checkBoxWarnNBSP;
    private System.Windows.Forms.TabPage tabPageAdvanced;
    private System.Windows.Forms.TextBox textBoxDelimiterPlaceholder;
    private System.Windows.Forms.TextBox textBoxNLPlaceholder;
    private System.Windows.Forms.Label labelLineFeedPlaceHolder;
    private System.Windows.Forms.CheckBox checkBoxWarnDelimiterInValue;
    private System.Windows.Forms.CheckBox checkBoxWarnQuotes;
    private System.Windows.Forms.CheckBox checkBoxWarnEmptyTailingColumns;
    private System.Windows.Forms.TextBox textBoxNumWarnings;
    private System.Windows.Forms.CheckBox checkBoxWarnLineFeed;
    private System.Windows.Forms.TextBox textBoxTextAsNull;
    private System.Windows.Forms.CheckBox checkBoxDisplayStartLineNo;
    private System.Windows.Forms.Label labelWarningLimit;
    private System.Windows.Forms.Label labelDelimiterPlaceholer;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TabPage tabPageQuoting;
    private System.Windows.Forms.Button buttonSkipLine;
    private QuotingControl quotingControl;
    private System.Windows.Forms.Button buttonGuessDelimiter;
    private System.Windows.Forms.Button buttonGuessCP;
    private System.Windows.Forms.Label labelDelimiter;
    private System.Windows.Forms.TextBox textBoxDelimiter;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox textBoxComment;
    private System.Windows.Forms.ComboBox cboCodePage;
    private System.Windows.Forms.TextBox textBoxFile;
    private System.Windows.Forms.Button btnOpenFile;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.BindingSource fileFormatBindingSource;
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.TextBox textBoxSkipRows;
    private System.Windows.Forms.Label labelSkipFirstLines;
    private System.Windows.Forms.Label labelRecordLimit;
    private System.Windows.Forms.TextBox textBoxLimitRows;
    private System.Windows.Forms.Label labelCodePage;
    private System.Windows.Forms.CheckBox checkBoxBOM;
    private System.Windows.Forms.CheckBox checkBoxHeader;
    private System.Windows.Forms.Label labelDelimitedFile;
    private System.Windows.Forms.Label labelPassphrase;
    private System.Windows.Forms.ListBox listBoxPrivKeys;
    private System.Windows.Forms.Button btnRemPrivKey;
    private System.Windows.Forms.Label label30;
    private System.Windows.Forms.Button btnAddPrivKey;
    private System.Windows.Forms.Button btnPassp;
    private System.Windows.Forms.TabPage tabPageBehaviour;
    private System.Windows.Forms.CheckBox chkUseFileSettings;
    private System.Windows.Forms.CheckBox checkBoxDetectFileChanges;
    private System.Windows.Forms.CheckBox checkBoxMenuDown;
    private System.Windows.Forms.CheckBox checkBoxGuessStartRow;
    private System.Windows.Forms.CheckBox checkBoxGuessHasHeader;
    private System.Windows.Forms.CheckBox checkBoxGuessDelimiter;
    private System.Windows.Forms.CheckBox checkBoxGuessCodePage;
    private System.Windows.Forms.CheckBox checkBoxSkipEmptyLines;
    private System.Windows.Forms.CheckBox checkBoxTreatNBSPAsSpace;
    private System.Windows.Forms.CheckBox checkBoxTreatUnknowCharaterAsSpace;
    private System.Windows.Forms.CheckBox checkBoxTreatLFasSpace;
    private System.Windows.Forms.CheckBox checkBoxAllowRowCombining;
    private System.Windows.Forms.CheckBox checkBoxTryToSolveMoreColumns;
    internal System.Windows.Forms.TabControl tabControl;
    internal System.Windows.Forms.TabPage tabPageFile;
    internal System.Windows.Forms.TabPage tabPagePGP;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.BindingSource fileSettingBindingSource;
    private System.Windows.Forms.CheckBox checkBoxCheckQuote;
  }
}