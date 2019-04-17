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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditSettings));
      this.textBoxSkipRows = new System.Windows.Forms.TextBox();
      this.labelSkipFirstLines = new System.Windows.Forms.Label();
      this.labelRecordLimit = new System.Windows.Forms.Label();
      this.textBoxLimitRows = new System.Windows.Forms.TextBox();
      this.labelCodePage = new System.Windows.Forms.Label();
      this.checkBoxBOM = new System.Windows.Forms.CheckBox();
      this.checkBoxHeader = new System.Windows.Forms.CheckBox();
      this.labelDelimitedFile = new System.Windows.Forms.Label();
      this.checkBoxWarnNBSP = new System.Windows.Forms.CheckBox();
      this.checkBoxWarnUnknowCharater = new System.Windows.Forms.CheckBox();
      this.labelWarningLimit = new System.Windows.Forms.Label();
      this.labelDelimiterPlaceholer = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.checkBoxWarnDelimiterInValue = new System.Windows.Forms.CheckBox();
      this.checkBoxWarnLineFeed = new System.Windows.Forms.CheckBox();
      this.checkBoxWarnQuotes = new System.Windows.Forms.CheckBox();
      this.checkBoxWarnEmptyTailingColumns = new System.Windows.Forms.CheckBox();
      this.textBoxNumWarnings = new System.Windows.Forms.TextBox();
      this.tabPageFormat = new System.Windows.Forms.TabPage();
      this.tabPageWarnings = new System.Windows.Forms.TabPage();
      this.tabPageAdvanced = new System.Windows.Forms.TabPage();
      this.checkBoxGuessStartRow = new System.Windows.Forms.CheckBox();
      this.buttonSkipLine = new System.Windows.Forms.Button();
      this.textBoxTextAsNull = new System.Windows.Forms.TextBox();
      this.textBoxDelimiterPlaceholder = new System.Windows.Forms.TextBox();
      this.textBoxNLPlaceholder = new System.Windows.Forms.TextBox();
      this.labelLineFeedPlaceHolder = new System.Windows.Forms.Label();
      this.checkBoxDisplayStartLineNo = new System.Windows.Forms.CheckBox();
      this.tabControl = new System.Windows.Forms.TabControl();
      this.tabPageFile = new System.Windows.Forms.TabPage();
      this.checkBoxGuessHasHeader = new System.Windows.Forms.CheckBox();
      this.checkBoxGuessDelimiter = new System.Windows.Forms.CheckBox();
      this.checkBoxGuessCodePage = new System.Windows.Forms.CheckBox();
      this.buttonGuessDelimiter = new System.Windows.Forms.Button();
      this.buttonGuessCP = new System.Windows.Forms.Button();
      this.label3 = new System.Windows.Forms.Label();
      this.labelDelimiter = new System.Windows.Forms.Label();
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.textBoxDelimiter = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.textBoxComment = new System.Windows.Forms.TextBox();
      this.cboCodePage = new System.Windows.Forms.ComboBox();
      this.textBoxFile = new System.Windows.Forms.TextBox();
      this.btnOpenFile = new System.Windows.Forms.Button();
      this.tabPageQuoting = new System.Windows.Forms.TabPage();
      this.tabPagePGP = new System.Windows.Forms.TabPage();
      this.labelPassphrase = new System.Windows.Forms.Label();
      this.listBoxPrivKeys = new System.Windows.Forms.ListBox();
      this.btnRemPrivKey = new System.Windows.Forms.Button();
      this.label30 = new System.Windows.Forms.Label();
      this.btnAddPrivKey = new System.Windows.Forms.Button();
      this.btnPassp = new System.Windows.Forms.Button();
      this.tabPageBehaviour = new System.Windows.Forms.TabPage();
      this.checkBoxSkipEmptyLines = new System.Windows.Forms.CheckBox();
      this.checkBoxTreatNBSPAsSpace = new System.Windows.Forms.CheckBox();
      this.checkBoxTreatUnknowCharaterAsSpace = new System.Windows.Forms.CheckBox();
      this.checkBoxTreatLFasSpace = new System.Windows.Forms.CheckBox();
      this.checkBoxAllowRowCombining = new System.Windows.Forms.CheckBox();
      this.checkBoxTryToSolveMoreColumns = new System.Windows.Forms.CheckBox();
      this.chkUseFileSettings = new System.Windows.Forms.CheckBox();
      this.checkBoxDetectFileChanges = new System.Windows.Forms.CheckBox();
      this.checkBoxMenuDown = new System.Windows.Forms.CheckBox();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.fileFormatBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.fileSettingBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.quotingControl = new CsvTools.QuotingControl();
      this.fillGuessSettingEdit = new CsvTools.FillGuessSettingEdit();
      this.tabPageFormat.SuspendLayout();
      this.tabPageWarnings.SuspendLayout();
      this.tabPageAdvanced.SuspendLayout();
      this.tabControl.SuspendLayout();
      this.tabPageFile.SuspendLayout();
      this.tabPageQuoting.SuspendLayout();
      this.tabPagePGP.SuspendLayout();
      this.tabPageBehaviour.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fileFormatBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fileSettingBindingSource)).BeginInit();
      this.SuspendLayout();
      // 
      // textBoxSkipRows
      // 
      this.textBoxSkipRows.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "SkipRows", true));
      this.textBoxSkipRows.Location = new System.Drawing.Point(118, 8);
      this.textBoxSkipRows.Name = "textBoxSkipRows";
      this.textBoxSkipRows.Size = new System.Drawing.Size(45, 20);
      this.textBoxSkipRows.TabIndex = 0;
      this.textBoxSkipRows.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.textBoxSkipRows.Validating += new System.ComponentModel.CancelEventHandler(this.PositiveNumberValidating);
      // 
      // labelSkipFirstLines
      // 
      this.labelSkipFirstLines.AutoSize = true;
      this.labelSkipFirstLines.Location = new System.Drawing.Point(31, 11);
      this.labelSkipFirstLines.Name = "labelSkipFirstLines";
      this.labelSkipFirstLines.Size = new System.Drawing.Size(81, 13);
      this.labelSkipFirstLines.TabIndex = 119;
      this.labelSkipFirstLines.Text = "Skip First Lines:";
      // 
      // labelRecordLimit
      // 
      this.labelRecordLimit.AutoSize = true;
      this.labelRecordLimit.Location = new System.Drawing.Point(43, 115);
      this.labelRecordLimit.Name = "labelRecordLimit";
      this.labelRecordLimit.Size = new System.Drawing.Size(69, 13);
      this.labelRecordLimit.TabIndex = 112;
      this.labelRecordLimit.Text = "Record Limit:";
      // 
      // textBoxLimitRows
      // 
      this.textBoxLimitRows.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "RecordLimit", true));
      this.textBoxLimitRows.Location = new System.Drawing.Point(118, 112);
      this.textBoxLimitRows.Name = "textBoxLimitRows";
      this.textBoxLimitRows.Size = new System.Drawing.Size(45, 20);
      this.textBoxLimitRows.TabIndex = 6;
      this.textBoxLimitRows.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.textBoxLimitRows.Validating += new System.ComponentModel.CancelEventHandler(this.PositiveNumberValidating);
      // 
      // labelCodePage
      // 
      this.labelCodePage.AutoSize = true;
      this.labelCodePage.Location = new System.Drawing.Point(21, 62);
      this.labelCodePage.Name = "labelCodePage";
      this.labelCodePage.Size = new System.Drawing.Size(63, 13);
      this.labelCodePage.TabIndex = 44;
      this.labelCodePage.Text = "Code Page:";
      // 
      // checkBoxBOM
      // 
      this.checkBoxBOM.AutoSize = true;
      this.checkBoxBOM.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "ByteOrderMark", true));
      this.checkBoxBOM.Location = new System.Drawing.Point(275, 61);
      this.checkBoxBOM.Name = "checkBoxBOM";
      this.checkBoxBOM.Size = new System.Drawing.Size(50, 17);
      this.checkBoxBOM.TabIndex = 42;
      this.checkBoxBOM.Text = "BOM";
      this.toolTip.SetToolTip(this.checkBoxBOM, "Byte Order Mark");
      this.checkBoxBOM.UseVisualStyleBackColor = true;
      // 
      // checkBoxHeader
      // 
      this.checkBoxHeader.AutoSize = true;
      this.checkBoxHeader.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "HasFieldHeader", true));
      this.checkBoxHeader.Location = new System.Drawing.Point(90, 33);
      this.checkBoxHeader.Name = "checkBoxHeader";
      this.checkBoxHeader.Size = new System.Drawing.Size(126, 17);
      this.checkBoxHeader.TabIndex = 2;
      this.checkBoxHeader.Text = "Has Column Headers";
      this.checkBoxHeader.UseVisualStyleBackColor = true;
      // 
      // labelDelimitedFile
      // 
      this.labelDelimitedFile.AutoSize = true;
      this.labelDelimitedFile.Location = new System.Drawing.Point(12, 9);
      this.labelDelimitedFile.Name = "labelDelimitedFile";
      this.labelDelimitedFile.Size = new System.Drawing.Size(72, 13);
      this.labelDelimitedFile.TabIndex = 39;
      this.labelDelimitedFile.Text = "Delimited File:";
      // 
      // checkBoxWarnNBSP
      // 
      this.checkBoxWarnNBSP.AutoSize = true;
      this.checkBoxWarnNBSP.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnNBSP", true));
      this.checkBoxWarnNBSP.Location = new System.Drawing.Point(8, 120);
      this.checkBoxWarnNBSP.Name = "checkBoxWarnNBSP";
      this.checkBoxWarnNBSP.Size = new System.Drawing.Size(151, 17);
      this.checkBoxWarnNBSP.TabIndex = 5;
      this.checkBoxWarnNBSP.Text = "Warn non-breaking Space";
      this.checkBoxWarnNBSP.UseVisualStyleBackColor = true;
      // 
      // checkBoxWarnUnknowCharater
      // 
      this.checkBoxWarnUnknowCharater.AutoSize = true;
      this.checkBoxWarnUnknowCharater.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnUnknowCharater", true));
      this.checkBoxWarnUnknowCharater.Location = new System.Drawing.Point(8, 97);
      this.checkBoxWarnUnknowCharater.Name = "checkBoxWarnUnknowCharater";
      this.checkBoxWarnUnknowCharater.Size = new System.Drawing.Size(165, 17);
      this.checkBoxWarnUnknowCharater.TabIndex = 4;
      this.checkBoxWarnUnknowCharater.Text = "Warn Unknown Characters �";
      this.checkBoxWarnUnknowCharater.UseVisualStyleBackColor = true;
      // 
      // labelWarningLimit
      // 
      this.labelWarningLimit.AutoSize = true;
      this.labelWarningLimit.Location = new System.Drawing.Point(8, 159);
      this.labelWarningLimit.Name = "labelWarningLimit";
      this.labelWarningLimit.Size = new System.Drawing.Size(74, 13);
      this.labelWarningLimit.TabIndex = 57;
      this.labelWarningLimit.Text = "Warning Limit:";
      // 
      // labelDelimiterPlaceholer
      // 
      this.labelDelimiterPlaceholer.AutoSize = true;
      this.labelDelimiterPlaceholer.Location = new System.Drawing.Point(3, 38);
      this.labelDelimiterPlaceholer.Name = "labelDelimiterPlaceholer";
      this.labelDelimiterPlaceholer.Size = new System.Drawing.Size(109, 13);
      this.labelDelimiterPlaceholer.TabIndex = 56;
      this.labelDelimiterPlaceholer.Text = "Delimiter Placeholder:";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(9, 90);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(104, 13);
      this.label1.TabIndex = 108;
      this.label1.Text = "Treat Text as NULL:";
      // 
      // checkBoxWarnDelimiterInValue
      // 
      this.checkBoxWarnDelimiterInValue.AutoSize = true;
      this.checkBoxWarnDelimiterInValue.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnDelimiterInValue", true));
      this.checkBoxWarnDelimiterInValue.Location = new System.Drawing.Point(8, 28);
      this.checkBoxWarnDelimiterInValue.Name = "checkBoxWarnDelimiterInValue";
      this.checkBoxWarnDelimiterInValue.Size = new System.Drawing.Size(95, 17);
      this.checkBoxWarnDelimiterInValue.TabIndex = 1;
      this.checkBoxWarnDelimiterInValue.Text = "Warn Delimiter";
      this.checkBoxWarnDelimiterInValue.UseVisualStyleBackColor = true;
      // 
      // checkBoxWarnLineFeed
      // 
      this.checkBoxWarnLineFeed.AutoSize = true;
      this.checkBoxWarnLineFeed.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnLineFeed", true));
      this.checkBoxWarnLineFeed.Location = new System.Drawing.Point(8, 51);
      this.checkBoxWarnLineFeed.Name = "checkBoxWarnLineFeed";
      this.checkBoxWarnLineFeed.Size = new System.Drawing.Size(96, 17);
      this.checkBoxWarnLineFeed.TabIndex = 2;
      this.checkBoxWarnLineFeed.Text = "Warn Linefeed";
      this.checkBoxWarnLineFeed.UseVisualStyleBackColor = true;
      // 
      // checkBoxWarnQuotes
      // 
      this.checkBoxWarnQuotes.AutoSize = true;
      this.checkBoxWarnQuotes.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnQuotes", true));
      this.checkBoxWarnQuotes.Location = new System.Drawing.Point(8, 74);
      this.checkBoxWarnQuotes.Name = "checkBoxWarnQuotes";
      this.checkBoxWarnQuotes.Size = new System.Drawing.Size(89, 17);
      this.checkBoxWarnQuotes.TabIndex = 3;
      this.checkBoxWarnQuotes.Text = "Warn Quotes";
      this.checkBoxWarnQuotes.UseVisualStyleBackColor = true;
      // 
      // checkBoxWarnEmptyTailingColumns
      // 
      this.checkBoxWarnEmptyTailingColumns.AutoSize = true;
      this.checkBoxWarnEmptyTailingColumns.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "WarnEmptyTailingColumns", true));
      this.checkBoxWarnEmptyTailingColumns.Location = new System.Drawing.Point(8, 5);
      this.checkBoxWarnEmptyTailingColumns.Name = "checkBoxWarnEmptyTailingColumns";
      this.checkBoxWarnEmptyTailingColumns.Size = new System.Drawing.Size(132, 17);
      this.checkBoxWarnEmptyTailingColumns.TabIndex = 0;
      this.checkBoxWarnEmptyTailingColumns.Text = "Warn Trailing Columns";
      this.checkBoxWarnEmptyTailingColumns.UseVisualStyleBackColor = false;
      // 
      // textBoxNumWarnings
      // 
      this.textBoxNumWarnings.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "NumWarnings", true));
      this.textBoxNumWarnings.Location = new System.Drawing.Point(95, 156);
      this.textBoxNumWarnings.Name = "textBoxNumWarnings";
      this.textBoxNumWarnings.Size = new System.Drawing.Size(45, 20);
      this.textBoxNumWarnings.TabIndex = 6;
      this.textBoxNumWarnings.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.textBoxNumWarnings.Validating += new System.ComponentModel.CancelEventHandler(this.PositiveNumberValidating);
      // 
      // tabPageFormat
      // 
      this.tabPageFormat.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageFormat.Controls.Add(this.fillGuessSettingEdit);
      this.tabPageFormat.Location = new System.Drawing.Point(4, 22);
      this.tabPageFormat.Name = "tabPageFormat";
      this.tabPageFormat.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageFormat.Size = new System.Drawing.Size(724, 326);
      this.tabPageFormat.TabIndex = 0;
      this.tabPageFormat.Text = "Detect Types";
      // 
      // tabPageWarnings
      // 
      this.tabPageWarnings.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageWarnings.Controls.Add(this.checkBoxWarnUnknowCharater);
      this.tabPageWarnings.Controls.Add(this.checkBoxWarnNBSP);
      this.tabPageWarnings.Controls.Add(this.labelWarningLimit);
      this.tabPageWarnings.Controls.Add(this.textBoxNumWarnings);
      this.tabPageWarnings.Controls.Add(this.checkBoxWarnEmptyTailingColumns);
      this.tabPageWarnings.Controls.Add(this.checkBoxWarnQuotes);
      this.tabPageWarnings.Controls.Add(this.checkBoxWarnLineFeed);
      this.tabPageWarnings.Controls.Add(this.checkBoxWarnDelimiterInValue);
      this.tabPageWarnings.Location = new System.Drawing.Point(4, 22);
      this.tabPageWarnings.Name = "tabPageWarnings";
      this.tabPageWarnings.Size = new System.Drawing.Size(724, 326);
      this.tabPageWarnings.TabIndex = 3;
      this.tabPageWarnings.Text = "Warnings";
      // 
      // tabPageAdvanced
      // 
      this.tabPageAdvanced.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageAdvanced.Controls.Add(this.checkBoxGuessStartRow);
      this.tabPageAdvanced.Controls.Add(this.buttonSkipLine);
      this.tabPageAdvanced.Controls.Add(this.textBoxSkipRows);
      this.tabPageAdvanced.Controls.Add(this.labelSkipFirstLines);
      this.tabPageAdvanced.Controls.Add(this.labelRecordLimit);
      this.tabPageAdvanced.Controls.Add(this.textBoxLimitRows);
      this.tabPageAdvanced.Controls.Add(this.textBoxTextAsNull);
      this.tabPageAdvanced.Controls.Add(this.label1);
      this.tabPageAdvanced.Controls.Add(this.labelDelimiterPlaceholer);
      this.tabPageAdvanced.Controls.Add(this.textBoxDelimiterPlaceholder);
      this.tabPageAdvanced.Controls.Add(this.textBoxNLPlaceholder);
      this.tabPageAdvanced.Controls.Add(this.labelLineFeedPlaceHolder);
      this.tabPageAdvanced.Controls.Add(this.checkBoxDisplayStartLineNo);
      this.tabPageAdvanced.Location = new System.Drawing.Point(4, 22);
      this.tabPageAdvanced.Name = "tabPageAdvanced";
      this.tabPageAdvanced.Size = new System.Drawing.Size(724, 326);
      this.tabPageAdvanced.TabIndex = 2;
      this.tabPageAdvanced.Text = "Advanced";
      // 
      // checkBoxGuessStartRow
      // 
      this.checkBoxGuessStartRow.AutoSize = true;
      this.checkBoxGuessStartRow.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessStartRow", true));
      this.checkBoxGuessStartRow.Location = new System.Drawing.Point(337, 10);
      this.checkBoxGuessStartRow.Name = "checkBoxGuessStartRow";
      this.checkBoxGuessStartRow.Size = new System.Drawing.Size(188, 17);
      this.checkBoxGuessStartRow.TabIndex = 2;
      this.checkBoxGuessStartRow.Text = "Determine Start Row automatically";
      this.checkBoxGuessStartRow.UseVisualStyleBackColor = true;
      // 
      // buttonSkipLine
      // 
      this.buttonSkipLine.AutoSize = true;
      this.buttonSkipLine.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.buttonSkipLine.Location = new System.Drawing.Point(189, 6);
      this.buttonSkipLine.Name = "buttonSkipLine";
      this.buttonSkipLine.Size = new System.Drawing.Size(142, 23);
      this.buttonSkipLine.TabIndex = 1;
      this.buttonSkipLine.Text = "   Guess Start Row";
      this.buttonSkipLine.UseVisualStyleBackColor = true;
      this.buttonSkipLine.Click += new System.EventHandler(this.ButtonSkipLine_Click);
      // 
      // textBoxTextAsNull
      // 
      this.textBoxTextAsNull.AutoCompleteCustomSource.AddRange(new string[] {
            "NULL",
            "n.a.",
            "n/a"});
      this.textBoxTextAsNull.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "TreatTextAsNull", true));
      this.textBoxTextAsNull.Location = new System.Drawing.Point(118, 87);
      this.textBoxTextAsNull.Name = "textBoxTextAsNull";
      this.textBoxTextAsNull.Size = new System.Drawing.Size(187, 20);
      this.textBoxTextAsNull.TabIndex = 5;
      // 
      // textBoxDelimiterPlaceholder
      // 
      this.textBoxDelimiterPlaceholder.AutoCompleteCustomSource.AddRange(new string[] {
            "{d}"});
      this.textBoxDelimiterPlaceholder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      this.textBoxDelimiterPlaceholder.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "DelimiterPlaceholder", true));
      this.textBoxDelimiterPlaceholder.Location = new System.Drawing.Point(118, 35);
      this.textBoxDelimiterPlaceholder.Name = "textBoxDelimiterPlaceholder";
      this.textBoxDelimiterPlaceholder.Size = new System.Drawing.Size(45, 20);
      this.textBoxDelimiterPlaceholder.TabIndex = 3;
      // 
      // textBoxNLPlaceholder
      // 
      this.textBoxNLPlaceholder.AutoCompleteCustomSource.AddRange(new string[] {
            "<br>",
            "{n}"});
      this.textBoxNLPlaceholder.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
      this.textBoxNLPlaceholder.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "NewLinePlaceholder", true));
      this.textBoxNLPlaceholder.Location = new System.Drawing.Point(118, 61);
      this.textBoxNLPlaceholder.Name = "textBoxNLPlaceholder";
      this.textBoxNLPlaceholder.Size = new System.Drawing.Size(45, 20);
      this.textBoxNLPlaceholder.TabIndex = 4;
      // 
      // labelLineFeedPlaceHolder
      // 
      this.labelLineFeedPlaceHolder.AutoSize = true;
      this.labelLineFeedPlaceHolder.Location = new System.Drawing.Point(3, 64);
      this.labelLineFeedPlaceHolder.Name = "labelLineFeedPlaceHolder";
      this.labelLineFeedPlaceHolder.Size = new System.Drawing.Size(110, 13);
      this.labelLineFeedPlaceHolder.TabIndex = 55;
      this.labelLineFeedPlaceHolder.Text = "Linefeed Placeholder:\r\n";
      // 
      // checkBoxDisplayStartLineNo
      // 
      this.checkBoxDisplayStartLineNo.AutoSize = true;
      this.checkBoxDisplayStartLineNo.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.checkBoxDisplayStartLineNo.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "DisplayStartLineNo", true));
      this.checkBoxDisplayStartLineNo.Location = new System.Drawing.Point(118, 138);
      this.checkBoxDisplayStartLineNo.Name = "checkBoxDisplayStartLineNo";
      this.checkBoxDisplayStartLineNo.Size = new System.Drawing.Size(161, 17);
      this.checkBoxDisplayStartLineNo.TabIndex = 7;
      this.checkBoxDisplayStartLineNo.Text = "Add Column for Line Number";
      this.checkBoxDisplayStartLineNo.UseVisualStyleBackColor = true;
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
      this.tabControl.Size = new System.Drawing.Size(732, 352);
      this.tabControl.TabIndex = 0;
      // 
      // tabPageFile
      // 
      this.tabPageFile.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageFile.Controls.Add(this.checkBoxGuessHasHeader);
      this.tabPageFile.Controls.Add(this.checkBoxGuessDelimiter);
      this.tabPageFile.Controls.Add(this.checkBoxGuessCodePage);
      this.tabPageFile.Controls.Add(this.buttonGuessDelimiter);
      this.tabPageFile.Controls.Add(this.buttonGuessCP);
      this.tabPageFile.Controls.Add(this.label3);
      this.tabPageFile.Controls.Add(this.labelDelimiter);
      this.tabPageFile.Controls.Add(this.textBox1);
      this.tabPageFile.Controls.Add(this.textBoxDelimiter);
      this.tabPageFile.Controls.Add(this.label2);
      this.tabPageFile.Controls.Add(this.textBoxComment);
      this.tabPageFile.Controls.Add(this.cboCodePage);
      this.tabPageFile.Controls.Add(this.labelCodePage);
      this.tabPageFile.Controls.Add(this.checkBoxBOM);
      this.tabPageFile.Controls.Add(this.checkBoxHeader);
      this.tabPageFile.Controls.Add(this.textBoxFile);
      this.tabPageFile.Controls.Add(this.labelDelimitedFile);
      this.tabPageFile.Controls.Add(this.btnOpenFile);
      this.tabPageFile.Location = new System.Drawing.Point(4, 22);
      this.tabPageFile.Name = "tabPageFile";
      this.tabPageFile.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageFile.Size = new System.Drawing.Size(724, 326);
      this.tabPageFile.TabIndex = 6;
      this.tabPageFile.Text = "File";
      // 
      // checkBoxGuessHasHeader
      // 
      this.checkBoxGuessHasHeader.AutoSize = true;
      this.checkBoxGuessHasHeader.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessHasHeader", true));
      this.checkBoxGuessHasHeader.Location = new System.Drawing.Point(471, 33);
      this.checkBoxGuessHasHeader.Name = "checkBoxGuessHasHeader";
      this.checkBoxGuessHasHeader.Size = new System.Drawing.Size(232, 17);
      this.checkBoxGuessHasHeader.TabIndex = 3;
      this.checkBoxGuessHasHeader.Text = "Determine if Header is present automatically";
      this.checkBoxGuessHasHeader.UseVisualStyleBackColor = true;
      // 
      // checkBoxGuessDelimiter
      // 
      this.checkBoxGuessDelimiter.AutoSize = true;
      this.checkBoxGuessDelimiter.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessDelimiter", true));
      this.checkBoxGuessDelimiter.Location = new System.Drawing.Point(471, 91);
      this.checkBoxGuessDelimiter.Name = "checkBoxGuessDelimiter";
      this.checkBoxGuessDelimiter.Size = new System.Drawing.Size(181, 17);
      this.checkBoxGuessDelimiter.TabIndex = 9;
      this.checkBoxGuessDelimiter.Text = "Determine Delimiter automatically";
      this.checkBoxGuessDelimiter.UseVisualStyleBackColor = true;
      // 
      // checkBoxGuessCodePage
      // 
      this.checkBoxGuessCodePage.AutoSize = true;
      this.checkBoxGuessCodePage.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "GuessCodePage", true));
      this.checkBoxGuessCodePage.Location = new System.Drawing.Point(471, 61);
      this.checkBoxGuessCodePage.Name = "checkBoxGuessCodePage";
      this.checkBoxGuessCodePage.Size = new System.Drawing.Size(194, 17);
      this.checkBoxGuessCodePage.TabIndex = 6;
      this.checkBoxGuessCodePage.Text = "Determine Code Page automatically";
      this.checkBoxGuessCodePage.UseVisualStyleBackColor = true;
      // 
      // buttonGuessDelimiter
      // 
      this.buttonGuessDelimiter.AutoSize = true;
      this.buttonGuessDelimiter.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.buttonGuessDelimiter.Location = new System.Drawing.Point(331, 87);
      this.buttonGuessDelimiter.Name = "buttonGuessDelimiter";
      this.buttonGuessDelimiter.Size = new System.Drawing.Size(134, 23);
      this.buttonGuessDelimiter.TabIndex = 8;
      this.buttonGuessDelimiter.Text = "   Guess Delimiter";
      this.buttonGuessDelimiter.UseVisualStyleBackColor = true;
      this.buttonGuessDelimiter.Click += new System.EventHandler(this.ButtonGuessDelimiter_Click);
      // 
      // buttonGuessCP
      // 
      this.buttonGuessCP.AutoSize = true;
      this.buttonGuessCP.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.buttonGuessCP.Location = new System.Drawing.Point(331, 57);
      this.buttonGuessCP.Name = "buttonGuessCP";
      this.buttonGuessCP.Size = new System.Drawing.Size(134, 23);
      this.buttonGuessCP.TabIndex = 5;
      this.buttonGuessCP.Text = "   Guess Code Page";
      this.buttonGuessCP.UseVisualStyleBackColor = true;
      this.buttonGuessCP.Click += new System.EventHandler(this.ButtonGuessCP_Click);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(170, 92);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(46, 13);
      this.label3.TabIndex = 46;
      this.label3.Text = "Escape:";
      // 
      // labelDelimiter
      // 
      this.labelDelimiter.AutoSize = true;
      this.labelDelimiter.Location = new System.Drawing.Point(34, 92);
      this.labelDelimiter.Name = "labelDelimiter";
      this.labelDelimiter.Size = new System.Drawing.Size(50, 13);
      this.labelDelimiter.TabIndex = 46;
      this.labelDelimiter.Text = "Delimiter:";
      // 
      // textBox1
      // 
      this.textBox1.AutoCompleteCustomSource.AddRange(new string[] {
            "\\"});
      this.textBox1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.textBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
      this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "EscapeCharacter", true));
      this.textBox1.Location = new System.Drawing.Point(222, 89);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(45, 20);
      this.textBox1.TabIndex = 45;
      this.toolTip.SetToolTip(this.textBox1, resources.GetString("textBox1.ToolTip"));
      this.textBox1.TextChanged += new System.EventHandler(this.TextBoxDelimiter_TextChanged);
      // 
      // textBoxDelimiter
      // 
      this.textBoxDelimiter.AutoCompleteCustomSource.AddRange(new string[] {
            "TAB"});
      this.textBoxDelimiter.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.textBoxDelimiter.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
      this.textBoxDelimiter.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "FieldDelimiter", true));
      this.textBoxDelimiter.Location = new System.Drawing.Point(90, 89);
      this.textBoxDelimiter.Name = "textBoxDelimiter";
      this.textBoxDelimiter.Size = new System.Drawing.Size(45, 20);
      this.textBoxDelimiter.TabIndex = 7;
      this.textBoxDelimiter.TextChanged += new System.EventHandler(this.TextBoxDelimiter_TextChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(7, 123);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(77, 13);
      this.label2.TabIndex = 47;
      this.label2.Text = "Line Comment:";
      // 
      // textBoxComment
      // 
      this.textBoxComment.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileFormatBindingSource, "CommentLine", true));
      this.textBoxComment.Location = new System.Drawing.Point(90, 120);
      this.textBoxComment.Name = "textBoxComment";
      this.textBoxComment.Size = new System.Drawing.Size(45, 20);
      this.textBoxComment.TabIndex = 10;
      // 
      // cboCodePage
      // 
      this.cboCodePage.DisplayMember = "Display";
      this.cboCodePage.FormattingEnabled = true;
      this.cboCodePage.Location = new System.Drawing.Point(90, 59);
      this.cboCodePage.Name = "cboCodePage";
      this.cboCodePage.Size = new System.Drawing.Size(179, 21);
      this.cboCodePage.TabIndex = 4;
      this.cboCodePage.ValueMember = "ID";
      this.cboCodePage.SelectedIndexChanged += new System.EventHandler(this.CboCodePage_SelectedIndexChanged);
      // 
      // textBoxFile
      // 
      this.textBoxFile.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.textBoxFile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
      this.textBoxFile.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fileSettingBindingSource, "FileName", true));
      this.textBoxFile.Location = new System.Drawing.Point(90, 6);
      this.textBoxFile.Name = "textBoxFile";
      this.textBoxFile.Size = new System.Drawing.Size(521, 20);
      this.textBoxFile.TabIndex = 0;
      this.textBoxFile.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxFile_Validating);
      // 
      // btnOpenFile
      // 
      this.btnOpenFile.Location = new System.Drawing.Point(617, 6);
      this.btnOpenFile.Name = "btnOpenFile";
      this.btnOpenFile.Size = new System.Drawing.Size(96, 22);
      this.btnOpenFile.TabIndex = 1;
      this.btnOpenFile.Text = "Select";
      this.btnOpenFile.UseVisualStyleBackColor = true;
      this.btnOpenFile.Click += new System.EventHandler(this.BtnOpenFile_Click);
      // 
      // tabPageQuoting
      // 
      this.tabPageQuoting.Controls.Add(this.quotingControl);
      this.tabPageQuoting.Location = new System.Drawing.Point(4, 22);
      this.tabPageQuoting.Name = "tabPageQuoting";
      this.tabPageQuoting.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageQuoting.Size = new System.Drawing.Size(724, 326);
      this.tabPageQuoting.TabIndex = 7;
      this.tabPageQuoting.Text = "Quoting";
      this.tabPageQuoting.UseVisualStyleBackColor = true;
      // 
      // tabPagePGP
      // 
      this.tabPagePGP.BackColor = System.Drawing.SystemColors.Control;
      this.tabPagePGP.Controls.Add(this.labelPassphrase);
      this.tabPagePGP.Controls.Add(this.listBoxPrivKeys);
      this.tabPagePGP.Controls.Add(this.btnRemPrivKey);
      this.tabPagePGP.Controls.Add(this.label30);
      this.tabPagePGP.Controls.Add(this.btnAddPrivKey);
      this.tabPagePGP.Controls.Add(this.btnPassp);
      this.tabPagePGP.Location = new System.Drawing.Point(4, 22);
      this.tabPagePGP.Name = "tabPagePGP";
      this.tabPagePGP.Padding = new System.Windows.Forms.Padding(3);
      this.tabPagePGP.Size = new System.Drawing.Size(724, 326);
      this.tabPagePGP.TabIndex = 8;
      this.tabPagePGP.Text = "PGP";
      // 
      // labelPassphrase
      // 
      this.labelPassphrase.AutoSize = true;
      this.labelPassphrase.BackColor = System.Drawing.SystemColors.Info;
      this.labelPassphrase.ForeColor = System.Drawing.SystemColors.InfoText;
      this.labelPassphrase.Location = new System.Drawing.Point(523, 45);
      this.labelPassphrase.Name = "labelPassphrase";
      this.labelPassphrase.Size = new System.Drawing.Size(133, 13);
      this.labelPassphrase.TabIndex = 123;
      this.labelPassphrase.Text = "A default passphrase is set";
      // 
      // listBoxPrivKeys
      // 
      this.listBoxPrivKeys.FormattingEnabled = true;
      this.listBoxPrivKeys.Location = new System.Drawing.Point(3, 19);
      this.listBoxPrivKeys.Name = "listBoxPrivKeys";
      this.listBoxPrivKeys.Size = new System.Drawing.Size(376, 277);
      this.listBoxPrivKeys.TabIndex = 122;
      // 
      // btnRemPrivKey
      // 
      this.btnRemPrivKey.Location = new System.Drawing.Point(385, 48);
      this.btnRemPrivKey.Name = "btnRemPrivKey";
      this.btnRemPrivKey.Size = new System.Drawing.Size(127, 23);
      this.btnRemPrivKey.TabIndex = 2;
      this.btnRemPrivKey.Text = "Remove Private Key";
      this.btnRemPrivKey.UseVisualStyleBackColor = true;
      this.btnRemPrivKey.Click += new System.EventHandler(this.BtnRemPrivKey_Click);
      // 
      // label30
      // 
      this.label30.AutoSize = true;
      this.label30.Location = new System.Drawing.Point(6, 4);
      this.label30.Name = "label30";
      this.label30.Size = new System.Drawing.Size(135, 13);
      this.label30.TabIndex = 120;
      this.label30.Text = "Private Keys for Decryption";
      // 
      // btnAddPrivKey
      // 
      this.btnAddPrivKey.Location = new System.Drawing.Point(385, 19);
      this.btnAddPrivKey.Name = "btnAddPrivKey";
      this.btnAddPrivKey.Size = new System.Drawing.Size(127, 23);
      this.btnAddPrivKey.TabIndex = 0;
      this.btnAddPrivKey.Text = "Add Private Key";
      this.btnAddPrivKey.UseVisualStyleBackColor = true;
      this.btnAddPrivKey.Click += new System.EventHandler(this.BtnAddPrivKey_Click);
      // 
      // btnPassp
      // 
      this.btnPassp.Location = new System.Drawing.Point(522, 19);
      this.btnPassp.Name = "btnPassp";
      this.btnPassp.Size = new System.Drawing.Size(187, 23);
      this.btnPassp.TabIndex = 1;
      this.btnPassp.Text = "Set Default Decryption Passphrase";
      this.btnPassp.UseVisualStyleBackColor = true;
      this.btnPassp.Click += new System.EventHandler(this.BtnPassp_Click);
      // 
      // tabPageBehaviour
      // 
      this.tabPageBehaviour.BackColor = System.Drawing.SystemColors.Control;
      this.tabPageBehaviour.Controls.Add(this.checkBoxSkipEmptyLines);
      this.tabPageBehaviour.Controls.Add(this.checkBoxTreatNBSPAsSpace);
      this.tabPageBehaviour.Controls.Add(this.checkBoxTreatUnknowCharaterAsSpace);
      this.tabPageBehaviour.Controls.Add(this.checkBoxTreatLFasSpace);
      this.tabPageBehaviour.Controls.Add(this.checkBoxAllowRowCombining);
      this.tabPageBehaviour.Controls.Add(this.checkBoxTryToSolveMoreColumns);
      this.tabPageBehaviour.Controls.Add(this.chkUseFileSettings);
      this.tabPageBehaviour.Controls.Add(this.checkBoxDetectFileChanges);
      this.tabPageBehaviour.Controls.Add(this.checkBoxMenuDown);
      this.tabPageBehaviour.Location = new System.Drawing.Point(4, 22);
      this.tabPageBehaviour.Name = "tabPageBehaviour";
      this.tabPageBehaviour.Padding = new System.Windows.Forms.Padding(3);
      this.tabPageBehaviour.Size = new System.Drawing.Size(724, 326);
      this.tabPageBehaviour.TabIndex = 9;
      this.tabPageBehaviour.Text = "Behavior";
      // 
      // checkBoxSkipEmptyLines
      // 
      this.checkBoxSkipEmptyLines.AutoSize = true;
      this.checkBoxSkipEmptyLines.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "SkipEmptyLines", true));
      this.checkBoxSkipEmptyLines.Location = new System.Drawing.Point(8, 75);
      this.checkBoxSkipEmptyLines.Name = "checkBoxSkipEmptyLines";
      this.checkBoxSkipEmptyLines.Size = new System.Drawing.Size(107, 17);
      this.checkBoxSkipEmptyLines.TabIndex = 3;
      this.checkBoxSkipEmptyLines.Text = "Skip Empty Lines";
      this.checkBoxSkipEmptyLines.UseVisualStyleBackColor = true;
      // 
      // checkBoxTreatNBSPAsSpace
      // 
      this.checkBoxTreatNBSPAsSpace.AutoSize = true;
      this.checkBoxTreatNBSPAsSpace.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "TreatNBSPAsSpace", true));
      this.checkBoxTreatNBSPAsSpace.Location = new System.Drawing.Point(8, 213);
      this.checkBoxTreatNBSPAsSpace.Name = "checkBoxTreatNBSPAsSpace";
      this.checkBoxTreatNBSPAsSpace.Size = new System.Drawing.Size(198, 17);
      this.checkBoxTreatNBSPAsSpace.TabIndex = 8;
      this.checkBoxTreatNBSPAsSpace.Text = "Treat non-breaking Space as Space";
      this.toolTip.SetToolTip(this.checkBoxTreatNBSPAsSpace, "Threat any non-breaking space like a regular space");
      this.checkBoxTreatNBSPAsSpace.UseVisualStyleBackColor = true;
      // 
      // checkBoxTreatUnknowCharaterAsSpace
      // 
      this.checkBoxTreatUnknowCharaterAsSpace.AutoSize = true;
      this.checkBoxTreatUnknowCharaterAsSpace.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
      this.checkBoxTreatUnknowCharaterAsSpace.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "TreatUnknowCharaterAsSpace", true));
      this.checkBoxTreatUnknowCharaterAsSpace.Location = new System.Drawing.Point(8, 190);
      this.checkBoxTreatUnknowCharaterAsSpace.Name = "checkBoxTreatUnknowCharaterAsSpace";
      this.checkBoxTreatUnknowCharaterAsSpace.Size = new System.Drawing.Size(207, 17);
      this.checkBoxTreatUnknowCharaterAsSpace.TabIndex = 7;
      this.checkBoxTreatUnknowCharaterAsSpace.Text = "Treat Unknown Character � as Space";
      this.toolTip.SetToolTip(this.checkBoxTreatUnknowCharaterAsSpace, "Threat any unknown character like a space");
      this.checkBoxTreatUnknowCharaterAsSpace.UseVisualStyleBackColor = true;
      // 
      // checkBoxTreatLFasSpace
      // 
      this.checkBoxTreatLFasSpace.AutoSize = true;
      this.checkBoxTreatLFasSpace.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "TreatLFAsSpace", true));
      this.checkBoxTreatLFasSpace.Location = new System.Drawing.Point(8, 167);
      this.checkBoxTreatLFasSpace.Name = "checkBoxTreatLFasSpace";
      this.checkBoxTreatLFasSpace.Size = new System.Drawing.Size(114, 17);
      this.checkBoxTreatLFasSpace.TabIndex = 6;
      this.checkBoxTreatLFasSpace.Text = "Treat LF as Space";
      this.toolTip.SetToolTip(this.checkBoxTreatLFasSpace, "Threat a single occurrence of a LF as a space");
      this.checkBoxTreatLFasSpace.UseVisualStyleBackColor = true;
      // 
      // checkBoxAllowRowCombining
      // 
      this.checkBoxAllowRowCombining.AutoSize = true;
      this.checkBoxAllowRowCombining.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "AllowRowCombining", true));
      this.checkBoxAllowRowCombining.Location = new System.Drawing.Point(8, 108);
      this.checkBoxAllowRowCombining.Name = "checkBoxAllowRowCombining";
      this.checkBoxAllowRowCombining.Size = new System.Drawing.Size(122, 17);
      this.checkBoxAllowRowCombining.TabIndex = 4;
      this.checkBoxAllowRowCombining.Text = "Try to Combine Row";
      this.toolTip.SetToolTip(this.checkBoxAllowRowCombining, "Try to combine rows, this might happen if the column does contain a linefeed and " +
        "is not quoted.\r\nThis is a very risky option, in some cases rows might be lost.");
      this.checkBoxAllowRowCombining.UseVisualStyleBackColor = true;
      // 
      // checkBoxTryToSolveMoreColumns
      // 
      this.checkBoxTryToSolveMoreColumns.AutoSize = true;
      this.checkBoxTryToSolveMoreColumns.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "TryToSolveMoreColumns", true));
      this.checkBoxTryToSolveMoreColumns.Location = new System.Drawing.Point(8, 131);
      this.checkBoxTryToSolveMoreColumns.Name = "checkBoxTryToSolveMoreColumns";
      this.checkBoxTryToSolveMoreColumns.Size = new System.Drawing.Size(138, 17);
      this.checkBoxTryToSolveMoreColumns.TabIndex = 5;
      this.checkBoxTryToSolveMoreColumns.Text = "Try to Re-Align columns";
      this.toolTip.SetToolTip(this.checkBoxTryToSolveMoreColumns, "Try to realign columns in case the file is not quoted and an extra delimiter has " +
        "caused additional columns\r\nThis is a very risky option, as the alignment wight w" +
        "ell be wrong.");
      this.checkBoxTryToSolveMoreColumns.UseVisualStyleBackColor = true;
      // 
      // chkUseFileSettings
      // 
      this.chkUseFileSettings.AutoSize = true;
      this.chkUseFileSettings.Location = new System.Drawing.Point(8, 52);
      this.chkUseFileSettings.Name = "chkUseFileSettings";
      this.chkUseFileSettings.Size = new System.Drawing.Size(132, 17);
      this.chkUseFileSettings.TabIndex = 2;
      this.chkUseFileSettings.Text = "Persist Settings for File";
      this.toolTip.SetToolTip(this.chkUseFileSettings, "Store the settings for each individual file, do not use this is structure or form" +
        "atting of columns does change over time");
      this.chkUseFileSettings.UseVisualStyleBackColor = true;
      // 
      // checkBoxDetectFileChanges
      // 
      this.checkBoxDetectFileChanges.AutoSize = true;
      this.checkBoxDetectFileChanges.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "DetectFileChanges", true));
      this.checkBoxDetectFileChanges.Location = new System.Drawing.Point(8, 6);
      this.checkBoxDetectFileChanges.Name = "checkBoxDetectFileChanges";
      this.checkBoxDetectFileChanges.Size = new System.Drawing.Size(122, 17);
      this.checkBoxDetectFileChanges.TabIndex = 0;
      this.checkBoxDetectFileChanges.Text = "Detect File Changes";
      this.checkBoxDetectFileChanges.UseVisualStyleBackColor = true;
      // 
      // checkBoxMenuDown
      // 
      this.checkBoxMenuDown.AutoSize = true;
      this.checkBoxMenuDown.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fileSettingBindingSource, "MenuDown", true));
      this.checkBoxMenuDown.Location = new System.Drawing.Point(8, 29);
      this.checkBoxMenuDown.Name = "checkBoxMenuDown";
      this.checkBoxMenuDown.Size = new System.Drawing.Size(182, 17);
      this.checkBoxMenuDown.TabIndex = 1;
      this.checkBoxMenuDown.Text = "Display Actions in Navigation Bar";
      this.checkBoxMenuDown.UseVisualStyleBackColor = true;
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // fileFormatBindingSource
      // 
      this.fileFormatBindingSource.AllowNew = false;
      this.fileFormatBindingSource.DataSource = typeof(CsvTools.FileFormat);
      // 
      // fileSettingBindingSource
      // 
      this.fileSettingBindingSource.AllowNew = false;
      this.fileSettingBindingSource.DataSource = typeof(CsvTools.Properties.ViewSettings);
      // 
      // quotingControl
      // 
      this.quotingControl.BackColor = System.Drawing.SystemColors.Control;
      this.quotingControl.CsvFile = null;
      this.quotingControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.quotingControl.IsWriteSetting = false;
      this.quotingControl.Location = new System.Drawing.Point(3, 3);
      this.quotingControl.Name = "quotingControl";
      this.quotingControl.Size = new System.Drawing.Size(718, 320);
      this.quotingControl.TabIndex = 2;
      // 
      // fillGuessSettingEdit
      // 
      this.fillGuessSettingEdit.Dock = System.Windows.Forms.DockStyle.Fill;
      this.fillGuessSettingEdit.Location = new System.Drawing.Point(3, 3);
      this.fillGuessSettingEdit.Name = "fillGuessSettingEdit";
      this.fillGuessSettingEdit.Size = new System.Drawing.Size(718, 320);
      this.fillGuessSettingEdit.TabIndex = 101;
      // 
      // FormEditSettings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(732, 352);
      this.Controls.Add(this.tabControl);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormEditSettings";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Settings";
      this.TopMost = true;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormEditSettings_FormClosing);
      this.Load += new System.EventHandler(this.EditSettings_Load);
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
      ((System.ComponentModel.ISupportInitialize)(this.fileFormatBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fileSettingBindingSource)).EndInit();
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
  }
}