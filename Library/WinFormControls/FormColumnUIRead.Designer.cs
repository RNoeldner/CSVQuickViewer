namespace CsvTools
{
  /// <summary>
  /// ColumnFormatUI Form from Designer
  /// </summary>
  partial class FormColumnUiRead
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;


    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;

      if (disposing)
      {
        m_DisposedValue = true;
        components?.Dispose();
        m_CancellationTokenSource?.Dispose();
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
      System.Windows.Forms.Label labelGroup;
      System.Windows.Forms.Label labelPoint;
      System.Windows.Forms.Label labelTrue;
      System.Windows.Forms.Label labelFalse;
      System.Windows.Forms.Label labelSepBy;
      System.Windows.Forms.Label labelPart;
      System.Windows.Forms.Label label9;
      System.Windows.Forms.Label label1;
      System.Windows.Forms.Label label7;
      System.Windows.Forms.Label labelReplace;
      System.Windows.Forms.Label labelSPattern;
      System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
      System.Windows.Forms.Label labelDateSep;
      System.Windows.Forms.Label labelTimeCol;
      System.Windows.Forms.Label labelTimeSep;
      System.Windows.Forms.Label label3;
      System.Windows.Forms.Label labelLessCommon;
      System.Windows.Forms.LinkLabel linkLabelRegion;
      System.Windows.Forms.Label labelDateOutput;
      System.Windows.Forms.Label labelSample;
      System.Windows.Forms.Label label4;
      System.Windows.Forms.Label labelTCFormat;
      this.textBoxDateSeparator = new System.Windows.Forms.TextBox();
      this.bindingSourceValueFormat = new System.Windows.Forms.BindingSource(this.components);
      this.comboBoxDateFormat = new System.Windows.Forms.ComboBox();
      this.buttonAddFormat = new System.Windows.Forms.Button();
      this.labelAllowedDateFormats = new System.Windows.Forms.Label();
      this.checkedListBoxDateFormats = new System.Windows.Forms.CheckedListBox();
      this.comboBoxTimePart = new System.Windows.Forms.ComboBox();
      this.columnBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.textBoxTimeSeparator = new System.Windows.Forms.TextBox();
      this.comboBoxTimeZone = new System.Windows.Forms.ComboBox();
      this.label5 = new System.Windows.Forms.Label();
      this.labelSampleDisplay = new System.Windows.Forms.Label();
      this.labelDateOutputDisplay = new System.Windows.Forms.Label();
      this.label6 = new System.Windows.Forms.Label();
      this.labelInputTZ = new System.Windows.Forms.Label();
      this.labelOutPutTZ = new System.Windows.Forms.Label();
      this.comboBoxTPFormat = new System.Windows.Forms.ComboBox();
      this.comboBoxDataType = new System.Windows.Forms.ComboBox();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.labelColName = new System.Windows.Forms.Label();
      this.comboBoxColumnName = new System.Windows.Forms.ComboBox();
      this.buttonGuess = new System.Windows.Forms.Button();
      this.checkBoxIgnore = new System.Windows.Forms.CheckBox();
      this.textBoxColumnName = new System.Windows.Forms.TextBox();
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.buttonDisplayValues = new System.Windows.Forms.Button();
      this.textBoxReadFolder = new System.Windows.Forms.TextBox();
      this.textBoxWriteFolder = new System.Windows.Forms.TextBox();
      this.textBoxPattern = new System.Windows.Forms.TextBox();
      this.textBoxRegexSearchPattern = new System.Windows.Forms.TextBox();
      this.buttonOK = new System.Windows.Forms.Button();
      this.panelTop = new System.Windows.Forms.Panel();
      this.panelBottom = new System.Windows.Forms.Panel();
      this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
      this.groupBoxDate = new System.Windows.Forms.GroupBox();
      this.groupBoxNumber = new System.Windows.Forms.GroupBox();
      this.linkLabel2 = new System.Windows.Forms.LinkLabel();
      this.comboBoxNumberFormat = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.labelNumberOutput = new System.Windows.Forms.Label();
      this.label16 = new System.Windows.Forms.Label();
      this.textBoxDecimalSeparator = new System.Windows.Forms.TextBox();
      this.textBoxGroupSeparator = new System.Windows.Forms.TextBox();
      this.labelNumber = new System.Windows.Forms.Label();
      this.groupBoxBoolean = new System.Windows.Forms.GroupBox();
      this.textBoxTrue = new System.Windows.Forms.TextBox();
      this.textBoxFalse = new System.Windows.Forms.TextBox();
      this.groupBoxSplit = new System.Windows.Forms.GroupBox();
      this.numericUpDownPart = new System.Windows.Forms.NumericUpDown();
      this.labelSamplePart = new System.Windows.Forms.Label();
      this.checkBoxPartToEnd = new System.Windows.Forms.CheckBox();
      this.textBoxSplit = new System.Windows.Forms.TextBox();
      this.labelResultPart = new System.Windows.Forms.Label();
      this.groupBoxBinary = new System.Windows.Forms.GroupBox();
      this.checkBoxOverwrite = new System.Windows.Forms.CheckBox();
      this.groupBoxRegExReplace = new System.Windows.Forms.GroupBox();
      this.label8 = new System.Windows.Forms.Label();
      this.textBoxRegexReplacement = new System.Windows.Forms.TextBox();
      this.labelDisplayNullAs = new System.Windows.Forms.Label();
      this.textBoxDisplayNullAs = new System.Windows.Forms.TextBox();
      labelGroup = new System.Windows.Forms.Label();
      labelPoint = new System.Windows.Forms.Label();
      labelTrue = new System.Windows.Forms.Label();
      labelFalse = new System.Windows.Forms.Label();
      labelSepBy = new System.Windows.Forms.Label();
      labelPart = new System.Windows.Forms.Label();
      label9 = new System.Windows.Forms.Label();
      label1 = new System.Windows.Forms.Label();
      label7 = new System.Windows.Forms.Label();
      labelReplace = new System.Windows.Forms.Label();
      labelSPattern = new System.Windows.Forms.Label();
      tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      labelDateSep = new System.Windows.Forms.Label();
      labelTimeCol = new System.Windows.Forms.Label();
      labelTimeSep = new System.Windows.Forms.Label();
      label3 = new System.Windows.Forms.Label();
      labelLessCommon = new System.Windows.Forms.Label();
      linkLabelRegion = new System.Windows.Forms.LinkLabel();
      labelDateOutput = new System.Windows.Forms.Label();
      labelSample = new System.Windows.Forms.Label();
      label4 = new System.Windows.Forms.Label();
      labelTCFormat = new System.Windows.Forms.Label();
      tableLayoutPanel2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.bindingSourceValueFormat)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.columnBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      this.panelTop.SuspendLayout();
      this.panelBottom.SuspendLayout();
      this.flowLayoutPanel.SuspendLayout();
      this.groupBoxDate.SuspendLayout();
      this.groupBoxNumber.SuspendLayout();
      this.groupBoxBoolean.SuspendLayout();
      this.groupBoxSplit.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPart)).BeginInit();
      this.groupBoxBinary.SuspendLayout();
      this.groupBoxRegExReplace.SuspendLayout();
      this.SuspendLayout();
      // 
      // labelGroup
      // 
      labelGroup.AutoSize = true;
      labelGroup.Location = new System.Drawing.Point(2, 45);
      labelGroup.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelGroup.Name = "labelGroup";
      labelGroup.Size = new System.Drawing.Size(107, 13);
      labelGroup.TabIndex = 4;
      labelGroup.Text = "Thousand Separator:";
      // 
      // labelPoint
      // 
      labelPoint.AutoSize = true;
      labelPoint.Location = new System.Drawing.Point(37, 68);
      labelPoint.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelPoint.Name = "labelPoint";
      labelPoint.Size = new System.Drawing.Size(75, 13);
      labelPoint.TabIndex = 8;
      labelPoint.Text = "Decimal Point:";
      // 
      // labelTrue
      // 
      labelTrue.AutoSize = true;
      labelTrue.Location = new System.Drawing.Point(76, 16);
      labelTrue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelTrue.Name = "labelTrue";
      labelTrue.Size = new System.Drawing.Size(32, 13);
      labelTrue.TabIndex = 0;
      labelTrue.Text = "True:";
      labelTrue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelFalse
      // 
      labelFalse.AutoSize = true;
      labelFalse.Location = new System.Drawing.Point(72, 41);
      labelFalse.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelFalse.Name = "labelFalse";
      labelFalse.Size = new System.Drawing.Size(35, 13);
      labelFalse.TabIndex = 2;
      labelFalse.Text = "False:";
      labelFalse.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelSepBy
      // 
      labelSepBy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      labelSepBy.AutoSize = true;
      labelSepBy.Location = new System.Drawing.Point(38, 19);
      labelSepBy.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelSepBy.Name = "labelSepBy";
      labelSepBy.Size = new System.Drawing.Size(68, 13);
      labelSepBy.TabIndex = 0;
      labelSepBy.Text = "Separate By:";
      labelSepBy.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelPart
      // 
      labelPart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      labelPart.AutoSize = true;
      labelPart.Location = new System.Drawing.Point(77, 44);
      labelPart.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelPart.Name = "labelPart";
      labelPart.Size = new System.Drawing.Size(29, 13);
      labelPart.TabIndex = 3;
      labelPart.Text = "Part:";
      labelPart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label9
      // 
      label9.AutoSize = true;
      label9.Location = new System.Drawing.Point(37, 21);
      label9.Margin = new System.Windows.Forms.Padding(3);
      label9.Name = "label9";
      label9.Size = new System.Drawing.Size(68, 13);
      label9.TabIndex = 5;
      label9.Text = "Read Folder:";
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Location = new System.Drawing.Point(36, 47);
      label1.Margin = new System.Windows.Forms.Padding(3);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(67, 13);
      label1.TabIndex = 3;
      label1.Text = "Write Folder:";
      // 
      // label7
      // 
      label7.AutoSize = true;
      label7.Location = new System.Drawing.Point(16, 73);
      label7.Margin = new System.Windows.Forms.Padding(3);
      label7.Name = "label7";
      label7.Size = new System.Drawing.Size(84, 13);
      label7.TabIndex = 1;
      label7.Text = "Pattern for write:";
      // 
      // labelReplace
      // 
      labelReplace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      labelReplace.AutoSize = true;
      labelReplace.Location = new System.Drawing.Point(59, 47);
      labelReplace.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelReplace.Name = "labelReplace";
      labelReplace.Size = new System.Drawing.Size(50, 13);
      labelReplace.TabIndex = 8;
      labelReplace.Text = "Replace:";
      labelReplace.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelSPattern
      // 
      labelSPattern.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      labelSPattern.AutoSize = true;
      labelSPattern.Location = new System.Drawing.Point(25, 22);
      labelSPattern.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelSPattern.Name = "labelSPattern";
      labelSPattern.Size = new System.Drawing.Size(81, 13);
      labelSPattern.TabIndex = 0;
      labelSPattern.Text = "Search Pattern:";
      labelSPattern.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // tableLayoutPanel2
      // 
      tableLayoutPanel2.AutoSize = true;
      tableLayoutPanel2.ColumnCount = 7;
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
      tableLayoutPanel2.Controls.Add(labelDateSep, 0, 0);
      tableLayoutPanel2.Controls.Add(this.textBoxDateSeparator, 1, 0);
      tableLayoutPanel2.Controls.Add(this.comboBoxDateFormat, 1, 7);
      tableLayoutPanel2.Controls.Add(this.buttonAddFormat, 4, 7);
      tableLayoutPanel2.Controls.Add(this.labelAllowedDateFormats, 0, 3);
      tableLayoutPanel2.Controls.Add(this.checkedListBoxDateFormats, 1, 3);
      tableLayoutPanel2.Controls.Add(labelTimeCol, 0, 2);
      tableLayoutPanel2.Controls.Add(this.comboBoxTimePart, 1, 2);
      tableLayoutPanel2.Controls.Add(labelTimeSep, 2, 0);
      tableLayoutPanel2.Controls.Add(this.textBoxTimeSeparator, 3, 0);
      tableLayoutPanel2.Controls.Add(label3, 0, 1);
      tableLayoutPanel2.Controls.Add(this.comboBoxTimeZone, 1, 1);
      tableLayoutPanel2.Controls.Add(labelLessCommon, 0, 7);
      tableLayoutPanel2.Controls.Add(linkLabelRegion, 4, 6);
      tableLayoutPanel2.Controls.Add(labelDateOutput, 4, 5);
      tableLayoutPanel2.Controls.Add(labelSample, 4, 4);
      tableLayoutPanel2.Controls.Add(label4, 4, 3);
      tableLayoutPanel2.Controls.Add(this.label5, 5, 3);
      tableLayoutPanel2.Controls.Add(this.labelSampleDisplay, 5, 4);
      tableLayoutPanel2.Controls.Add(this.labelDateOutputDisplay, 5, 5);
      tableLayoutPanel2.Controls.Add(this.label6, 4, 1);
      tableLayoutPanel2.Controls.Add(this.labelInputTZ, 6, 4);
      tableLayoutPanel2.Controls.Add(this.labelOutPutTZ, 6, 5);
      tableLayoutPanel2.Controls.Add(labelTCFormat, 4, 2);
      tableLayoutPanel2.Controls.Add(this.comboBoxTPFormat, 5, 2);
      tableLayoutPanel2.Location = new System.Drawing.Point(0, 14);
      tableLayoutPanel2.Name = "tableLayoutPanel2";
      tableLayoutPanel2.RowCount = 8;
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      tableLayoutPanel2.Size = new System.Drawing.Size(616, 274);
      tableLayoutPanel2.TabIndex = 17;
      // 
      // labelDateSep
      // 
      labelDateSep.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelDateSep.AutoSize = true;
      labelDateSep.Location = new System.Drawing.Point(25, 6);
      labelDateSep.Margin = new System.Windows.Forms.Padding(3);
      labelDateSep.Name = "labelDateSep";
      labelDateSep.Size = new System.Drawing.Size(82, 13);
      labelDateSep.TabIndex = 0;
      labelDateSep.Text = "Date Separator:";
      // 
      // textBoxDateSeparator
      // 
      this.textBoxDateSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "DateSeparator", true));
      this.textBoxDateSeparator.Location = new System.Drawing.Point(113, 3);
      this.textBoxDateSeparator.Name = "textBoxDateSeparator";
      this.textBoxDateSeparator.Size = new System.Drawing.Size(28, 20);
      this.textBoxDateSeparator.TabIndex = 0;
      this.toolTip.SetToolTip(this.textBoxDateSeparator, "Separates the components of a date, that is, the year, month, and day");
      this.textBoxDateSeparator.TextChanged += new System.EventHandler(this.DateFormatChanged);
      // 
      // bindingSourceValueFormat
      // 
      this.bindingSourceValueFormat.AllowNew = false;
      this.bindingSourceValueFormat.DataSource = typeof(CsvTools.ValueFormatMut);
      // 
      // comboBoxDateFormat
      // 
      this.comboBoxDateFormat.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.comboBoxDateFormat.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
      tableLayoutPanel2.SetColumnSpan(this.comboBoxDateFormat, 3);
      this.comboBoxDateFormat.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxDateFormat.FormattingEnabled = true;
      this.comboBoxDateFormat.Location = new System.Drawing.Point(113, 246);
      this.comboBoxDateFormat.Name = "comboBoxDateFormat";
      this.comboBoxDateFormat.Size = new System.Drawing.Size(193, 21);
      this.comboBoxDateFormat.TabIndex = 6;
      // 
      // buttonAddFormat
      // 
      tableLayoutPanel2.SetColumnSpan(this.buttonAddFormat, 2);
      this.buttonAddFormat.Location = new System.Drawing.Point(312, 246);
      this.buttonAddFormat.Name = "buttonAddFormat";
      this.buttonAddFormat.Size = new System.Drawing.Size(103, 25);
      this.buttonAddFormat.TabIndex = 7;
      this.buttonAddFormat.Text = "Add to List";
      this.toolTip.SetToolTip(this.buttonAddFormat, "Add the selected uncommon date/time format to the checked list box");
      this.buttonAddFormat.UseVisualStyleBackColor = true;
      this.buttonAddFormat.Click += new System.EventHandler(this.ButtonAddFormat_Click);
      // 
      // labelAllowedDateFormats
      // 
      this.labelAllowedDateFormats.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.labelAllowedDateFormats.AutoSize = true;
      this.labelAllowedDateFormats.Location = new System.Drawing.Point(28, 83);
      this.labelAllowedDateFormats.Margin = new System.Windows.Forms.Padding(3);
      this.labelAllowedDateFormats.Name = "labelAllowedDateFormats";
      this.labelAllowedDateFormats.Size = new System.Drawing.Size(79, 13);
      this.labelAllowedDateFormats.TabIndex = 9;
      this.labelAllowedDateFormats.Text = "Date Format(s):";
      // 
      // checkedListBoxDateFormats
      // 
      tableLayoutPanel2.SetColumnSpan(this.checkedListBoxDateFormats, 3);
      this.checkedListBoxDateFormats.Dock = System.Windows.Forms.DockStyle.Fill;
      this.checkedListBoxDateFormats.FormattingEnabled = true;
      this.checkedListBoxDateFormats.Location = new System.Drawing.Point(113, 83);
      this.checkedListBoxDateFormats.Name = "checkedListBoxDateFormats";
      tableLayoutPanel2.SetRowSpan(this.checkedListBoxDateFormats, 4);
      this.checkedListBoxDateFormats.Size = new System.Drawing.Size(193, 157);
      this.checkedListBoxDateFormats.TabIndex = 5;
      this.toolTip.SetToolTip(this.checkedListBoxDateFormats, "Common Date/Time formats, you can choose multiple");
      this.checkedListBoxDateFormats.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CheckedListBoxDateFormats_ItemCheck);
      this.checkedListBoxDateFormats.SelectedIndexChanged += new System.EventHandler(this.DateFormatChanged);
      // 
      // labelTimeCol
      // 
      labelTimeCol.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelTimeCol.AutoSize = true;
      labelTimeCol.Location = new System.Drawing.Point(14, 60);
      labelTimeCol.Margin = new System.Windows.Forms.Padding(3);
      labelTimeCol.Name = "labelTimeCol";
      labelTimeCol.Size = new System.Drawing.Size(93, 13);
      labelTimeCol.TabIndex = 1;
      labelTimeCol.Text = "Column with Time:";
      // 
      // comboBoxTimePart
      // 
      this.comboBoxTimePart.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      tableLayoutPanel2.SetColumnSpan(this.comboBoxTimePart, 3);
      this.comboBoxTimePart.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "TimePart", true));
      this.comboBoxTimePart.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxTimePart.FormattingEnabled = true;
      this.comboBoxTimePart.Location = new System.Drawing.Point(113, 56);
      this.comboBoxTimePart.Name = "comboBoxTimePart";
      this.comboBoxTimePart.Size = new System.Drawing.Size(193, 21);
      this.comboBoxTimePart.TabIndex = 1;
      this.toolTip.SetToolTip(this.comboBoxTimePart, "Combining a time column will result in a combination of the column and the select" +
        "ed time column\r\ne.G “17/Aug/2019” & “17:54” will become “17/Aug/2019 17:54”\r\n");
      this.comboBoxTimePart.TextChanged += new System.EventHandler(this.DateFormatChanged);
      // 
      // columnBindingSource
      // 
      this.columnBindingSource.AllowNew = false;
      this.columnBindingSource.DataSource = typeof(CsvTools.ColumnMut);
      // 
      // labelTimeSep
      // 
      labelTimeSep.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelTimeSep.AutoSize = true;
      labelTimeSep.Location = new System.Drawing.Point(190, 6);
      labelTimeSep.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      labelTimeSep.Name = "labelTimeSep";
      labelTimeSep.Size = new System.Drawing.Size(82, 13);
      labelTimeSep.TabIndex = 5;
      labelTimeSep.Text = "Time Separator:";
      // 
      // textBoxTimeSeparator
      // 
      this.textBoxTimeSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "TimeSeparator", true));
      this.textBoxTimeSeparator.Location = new System.Drawing.Point(277, 3);
      this.textBoxTimeSeparator.Name = "textBoxTimeSeparator";
      this.textBoxTimeSeparator.Size = new System.Drawing.Size(28, 20);
      this.textBoxTimeSeparator.TabIndex = 3;
      this.toolTip.SetToolTip(this.textBoxTimeSeparator, "Separates the components of time, that is, the hour, minutes, and seconds.");
      this.textBoxTimeSeparator.TextChanged += new System.EventHandler(this.DateFormatChanged);
      // 
      // label3
      // 
      label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
      label3.AutoSize = true;
      label3.Location = new System.Drawing.Point(43, 33);
      label3.Margin = new System.Windows.Forms.Padding(3);
      label3.Name = "label3";
      label3.Size = new System.Drawing.Size(64, 13);
      label3.TabIndex = 7;
      label3.Text = "Time Zone :";
      // 
      // comboBoxTimeZone
      // 
      tableLayoutPanel2.SetColumnSpan(this.comboBoxTimeZone, 3);
      this.comboBoxTimeZone.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "TimeZonePart", true));
      this.comboBoxTimeZone.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxTimeZone.FormattingEnabled = true;
      this.comboBoxTimeZone.Location = new System.Drawing.Point(113, 29);
      this.comboBoxTimeZone.Name = "comboBoxTimeZone";
      this.comboBoxTimeZone.Size = new System.Drawing.Size(193, 21);
      this.comboBoxTimeZone.TabIndex = 4;
      this.comboBoxTimeZone.TextChanged += new System.EventHandler(this.DateFormatChanged);
      // 
      // labelLessCommon
      // 
      labelLessCommon.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelLessCommon.AutoSize = true;
      labelLessCommon.Location = new System.Drawing.Point(3, 252);
      labelLessCommon.Margin = new System.Windows.Forms.Padding(3);
      labelLessCommon.Name = "labelLessCommon";
      labelLessCommon.Size = new System.Drawing.Size(104, 13);
      labelLessCommon.TabIndex = 16;
      labelLessCommon.Text = "Uncommon Formats:";
      // 
      // linkLabelRegion
      // 
      linkLabelRegion.Anchor = System.Windows.Forms.AnchorStyles.Left;
      linkLabelRegion.AutoSize = true;
      tableLayoutPanel2.SetColumnSpan(linkLabelRegion, 3);
      linkLabelRegion.Location = new System.Drawing.Point(312, 180);
      linkLabelRegion.Margin = new System.Windows.Forms.Padding(3);
      linkLabelRegion.Name = "linkLabelRegion";
      linkLabelRegion.Size = new System.Drawing.Size(130, 13);
      linkLabelRegion.TabIndex = 14;
      linkLabelRegion.TabStop = true;
      linkLabelRegion.Text = "Open Region && Language";
      linkLabelRegion.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RegionAndLanguageLinkClicked);
      // 
      // labelDateOutput
      // 
      labelDateOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      labelDateOutput.AutoSize = true;
      labelDateOutput.ForeColor = System.Drawing.SystemColors.ControlText;
      labelDateOutput.Location = new System.Drawing.Point(312, 115);
      labelDateOutput.Margin = new System.Windows.Forms.Padding(3);
      labelDateOutput.Name = "labelDateOutput";
      labelDateOutput.Size = new System.Drawing.Size(42, 13);
      labelDateOutput.TabIndex = 13;
      labelDateOutput.Text = "Output:";
      // 
      // labelSample
      // 
      labelSample.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      labelSample.AutoSize = true;
      labelSample.ForeColor = System.Drawing.SystemColors.ControlText;
      labelSample.Location = new System.Drawing.Point(320, 99);
      labelSample.Name = "labelSample";
      labelSample.Size = new System.Drawing.Size(34, 13);
      labelSample.TabIndex = 12;
      labelSample.Text = "Input:";
      // 
      // label4
      // 
      label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      label4.AutoSize = true;
      label4.ForeColor = System.Drawing.SystemColors.ControlText;
      label4.Location = new System.Drawing.Point(317, 83);
      label4.Margin = new System.Windows.Forms.Padding(3);
      label4.Name = "label4";
      label4.Size = new System.Drawing.Size(37, 13);
      label4.TabIndex = 11;
      label4.Text = "Value:";
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      tableLayoutPanel2.SetColumnSpan(this.label5, 2);
      this.label5.ForeColor = System.Drawing.SystemColors.Highlight;
      this.label5.Location = new System.Drawing.Point(360, 83);
      this.label5.Margin = new System.Windows.Forms.Padding(3);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(141, 13);
      this.label5.TabIndex = 11;
      this.label5.Text = "7th April 2013  15:45:50 345";
      // 
      // labelSampleDisplay
      // 
      this.labelSampleDisplay.AutoSize = true;
      this.labelSampleDisplay.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelSampleDisplay.Location = new System.Drawing.Point(360, 99);
      this.labelSampleDisplay.Name = "labelSampleDisplay";
      this.labelSampleDisplay.Size = new System.Drawing.Size(17, 13);
      this.labelSampleDisplay.TabIndex = 12;
      this.labelSampleDisplay.Text = "\"\"";
      // 
      // labelDateOutputDisplay
      // 
      this.labelDateOutputDisplay.AutoSize = true;
      this.labelDateOutputDisplay.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelDateOutputDisplay.Location = new System.Drawing.Point(360, 115);
      this.labelDateOutputDisplay.Margin = new System.Windows.Forms.Padding(3);
      this.labelDateOutputDisplay.Name = "labelDateOutputDisplay";
      this.labelDateOutputDisplay.Size = new System.Drawing.Size(17, 13);
      this.labelDateOutputDisplay.TabIndex = 13;
      this.labelDateOutputDisplay.Text = "\"\"";
      // 
      // label6
      // 
      this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.label6.AutoSize = true;
      tableLayoutPanel2.SetColumnSpan(this.label6, 3);
      this.label6.ForeColor = System.Drawing.SystemColors.Highlight;
      this.label6.Location = new System.Drawing.Point(312, 33);
      this.label6.Margin = new System.Windows.Forms.Padding(3);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(187, 13);
      this.label6.TabIndex = 11;
      this.label6.Text = "Note: Constants in quotes e.G. \"UTC\"";
      // 
      // labelInputTZ
      // 
      this.labelInputTZ.AutoSize = true;
      this.labelInputTZ.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelInputTZ.Location = new System.Drawing.Point(489, 99);
      this.labelInputTZ.Name = "labelInputTZ";
      this.labelInputTZ.Size = new System.Drawing.Size(17, 13);
      this.labelInputTZ.TabIndex = 12;
      this.labelInputTZ.Text = "\"\"";
      // 
      // labelOutPutTZ
      // 
      this.labelOutPutTZ.AutoSize = true;
      this.labelOutPutTZ.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelOutPutTZ.Location = new System.Drawing.Point(489, 115);
      this.labelOutPutTZ.Margin = new System.Windows.Forms.Padding(3);
      this.labelOutPutTZ.Name = "labelOutPutTZ";
      this.labelOutPutTZ.Size = new System.Drawing.Size(17, 13);
      this.labelOutPutTZ.TabIndex = 13;
      this.labelOutPutTZ.Text = "\"\"";
      // 
      // labelTCFormat
      // 
      labelTCFormat.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelTCFormat.AutoSize = true;
      labelTCFormat.Location = new System.Drawing.Point(312, 60);
      labelTCFormat.Margin = new System.Windows.Forms.Padding(3);
      labelTCFormat.Name = "labelTCFormat";
      labelTCFormat.Size = new System.Drawing.Size(42, 13);
      labelTCFormat.TabIndex = 3;
      labelTCFormat.Text = "Format:";
      // 
      // comboBoxTPFormat
      // 
      tableLayoutPanel2.SetColumnSpan(this.comboBoxTPFormat, 2);
      this.comboBoxTPFormat.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "TimePartFormat", true));
      this.comboBoxTPFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxTPFormat.FormattingEnabled = true;
      this.comboBoxTPFormat.Location = new System.Drawing.Point(360, 56);
      this.comboBoxTPFormat.Name = "comboBoxTPFormat";
      this.comboBoxTPFormat.Size = new System.Drawing.Size(83, 21);
      this.comboBoxTPFormat.TabIndex = 2;
      this.toolTip.SetToolTip(this.comboBoxTPFormat, "Format of the time column");
      this.comboBoxTPFormat.SelectedIndexChanged += new System.EventHandler(this.ComboBoxTimePart_SelectedIndexChanged);
      // 
      // comboBoxDataType
      // 
      this.comboBoxDataType.DataBindings.Add(new System.Windows.Forms.Binding("SelectedValue", this.bindingSourceValueFormat, "DataType", true));
      this.comboBoxDataType.DisplayMember = "Display";
      this.comboBoxDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDataType.FormattingEnabled = true;
      this.comboBoxDataType.Location = new System.Drawing.Point(325, 3);
      this.comboBoxDataType.Name = "comboBoxDataType";
      this.comboBoxDataType.Size = new System.Drawing.Size(174, 21);
      this.comboBoxDataType.TabIndex = 2;
      this.comboBoxDataType.ValueMember = "ID";
      this.comboBoxDataType.SelectedIndexChanged += new System.EventHandler(this.ComboBoxDataType_SelectedIndexChanged);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(541, 3);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(83, 25);
      this.buttonCancel.TabIndex = 4;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      // 
      // labelColName
      // 
      this.labelColName.AutoSize = true;
      this.labelColName.Location = new System.Drawing.Point(3, 6);
      this.labelColName.Margin = new System.Windows.Forms.Padding(3);
      this.labelColName.Name = "labelColName";
      this.labelColName.Size = new System.Drawing.Size(76, 13);
      this.labelColName.TabIndex = 0;
      this.labelColName.Text = "Column Name:";
      // 
      // comboBoxColumnName
      // 
      this.comboBoxColumnName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "Name", true));
      this.comboBoxColumnName.FormattingEnabled = true;
      this.comboBoxColumnName.Location = new System.Drawing.Point(83, 3);
      this.comboBoxColumnName.Name = "comboBoxColumnName";
      this.comboBoxColumnName.Size = new System.Drawing.Size(238, 21);
      this.comboBoxColumnName.TabIndex = 1;
      this.comboBoxColumnName.SelectedIndexChanged += new System.EventHandler(this.ComboBoxColumnName_SelectedIndexChanged);
      this.comboBoxColumnName.TextUpdate += new System.EventHandler(this.ComboBoxColumnName_TextUpdate);
      // 
      // buttonGuess
      // 
      this.buttonGuess.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.buttonGuess.Location = new System.Drawing.Point(110, 3);
      this.buttonGuess.Name = "buttonGuess";
      this.buttonGuess.Size = new System.Drawing.Size(103, 25);
      this.buttonGuess.TabIndex = 2;
      this.buttonGuess.Text = "&Guess";
      this.toolTip.SetToolTip(this.buttonGuess, "Read the content of the source and try and find a matching format\r\nNote: Any colu" +
        "mn that has possible alignment issues will be ignored\r\n");
      this.buttonGuess.UseVisualStyleBackColor = true;
      this.buttonGuess.Click += new System.EventHandler(this.ButtonGuessClick);
      // 
      // checkBoxIgnore
      // 
      this.checkBoxIgnore.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.columnBindingSource, "Ignore", true));
      this.checkBoxIgnore.Location = new System.Drawing.Point(83, 27);
      this.checkBoxIgnore.Name = "checkBoxIgnore";
      this.checkBoxIgnore.Size = new System.Drawing.Size(103, 20);
      this.checkBoxIgnore.TabIndex = 3;
      this.checkBoxIgnore.Text = "&Ignore";
      this.checkBoxIgnore.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
      this.toolTip.SetToolTip(this.checkBoxIgnore, "Ignore the content do not display/import this column");
      this.checkBoxIgnore.UseVisualStyleBackColor = true;
      this.checkBoxIgnore.Visible = false;
      // 
      // textBoxColumnName
      // 
      this.textBoxColumnName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "Name", true));
      this.textBoxColumnName.Dock = System.Windows.Forms.DockStyle.Top;
      this.textBoxColumnName.Location = new System.Drawing.Point(85, 3);
      this.textBoxColumnName.Name = "textBoxColumnName";
      this.textBoxColumnName.ReadOnly = true;
      this.textBoxColumnName.Size = new System.Drawing.Size(160, 20);
      this.textBoxColumnName.TabIndex = 0;
      this.textBoxColumnName.WordWrap = false;
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // buttonDisplayValues
      // 
      this.buttonDisplayValues.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.buttonDisplayValues.Location = new System.Drawing.Point(2, 3);
      this.buttonDisplayValues.Name = "buttonDisplayValues";
      this.buttonDisplayValues.Size = new System.Drawing.Size(104, 25);
      this.buttonDisplayValues.TabIndex = 1;
      this.buttonDisplayValues.Text = "Display &Values";
      this.toolTip.SetToolTip(this.buttonDisplayValues, "Read the content of the source and display the read values.\r\nNote: Any column tha" +
        "t has possible alignment issues will be ignored\r\n");
      this.buttonDisplayValues.UseVisualStyleBackColor = true;
      this.buttonDisplayValues.Click += new System.EventHandler(this.ButtonDisplayValues_ClickAsync);
      // 
      // textBoxReadFolder
      // 
      this.textBoxReadFolder.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "ReadFolder", true));
      this.textBoxReadFolder.Location = new System.Drawing.Point(112, 18);
      this.textBoxReadFolder.Name = "textBoxReadFolder";
      this.textBoxReadFolder.Size = new System.Drawing.Size(238, 20);
      this.textBoxReadFolder.TabIndex = 6;
      this.toolTip.SetToolTip(this.textBoxReadFolder, "Folder to look for the files during the import");
      // 
      // textBoxWriteFolder
      // 
      this.textBoxWriteFolder.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "WriteFolder", true));
      this.textBoxWriteFolder.Location = new System.Drawing.Point(112, 44);
      this.textBoxWriteFolder.Name = "textBoxWriteFolder";
      this.textBoxWriteFolder.Size = new System.Drawing.Size(238, 20);
      this.textBoxWriteFolder.TabIndex = 4;
      this.toolTip.SetToolTip(this.textBoxWriteFolder, "As the data is written the files is sotored in this folder");
      // 
      // textBoxPattern
      // 
      this.textBoxPattern.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "FileOutPutPlaceholder", true));
      this.textBoxPattern.Location = new System.Drawing.Point(113, 70);
      this.textBoxPattern.Name = "textBoxPattern";
      this.textBoxPattern.Size = new System.Drawing.Size(237, 20);
      this.textBoxPattern.TabIndex = 2;
      this.toolTip.SetToolTip(this.textBoxPattern, "Pattern for the file during write. if left empty the original file name is used, " +
        "you can use placeholders if data from other columns should be used to get a file" +
        "name. E.G. {UserID}.docx");
      // 
      // textBoxRegexSearchPattern
      // 
      this.textBoxRegexSearchPattern.AutoCompleteCustomSource.AddRange(new string[] {
            "(?#href)<a(?:[^>]*?\\s+)?\\s*href\\s*=((\\\"|\')(.*)\\2)\\s*?>[^>]*?<\\/a>",
            "(?#url)(https?:\\/\\/)[^[\\s)]*",
            "(?#email)([\\w-]+(?:\\.[\\w-]+)*)@((?:[\\w-]+\\.)*\\w[\\w-]{0,66})",
            "(?#file)(\\w{1}\\:{1}\\/{2})(\\w+\\/{1})+(\\w+\\.{1}\\w+){1}"});
      this.textBoxRegexSearchPattern.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.textBoxRegexSearchPattern.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
      this.textBoxRegexSearchPattern.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "RegexSearchPattern", true));
      this.textBoxRegexSearchPattern.Location = new System.Drawing.Point(113, 19);
      this.textBoxRegexSearchPattern.Name = "textBoxRegexSearchPattern";
      this.textBoxRegexSearchPattern.Size = new System.Drawing.Size(239, 20);
      this.textBoxRegexSearchPattern.TabIndex = 7;
      this.toolTip.SetToolTip(this.textBoxRegexSearchPattern, "Regex Pattern to look for");
      this.textBoxRegexSearchPattern.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxRegexSearchPattern_Validating);
      // 
      // buttonOK
      // 
      this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonOK.Location = new System.Drawing.Point(454, 3);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(83, 25);
      this.buttonOK.TabIndex = 3;
      this.buttonOK.Text = "&Ok";
      this.buttonOK.UseVisualStyleBackColor = true;
      // 
      // panelTop
      // 
      this.panelTop.AutoSize = true;
      this.panelTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.panelTop.BackColor = System.Drawing.SystemColors.Control;
      this.panelTop.Controls.Add(this.labelDisplayNullAs);
      this.panelTop.Controls.Add(this.textBoxDisplayNullAs);
      this.panelTop.Controls.Add(this.labelColName);
      this.panelTop.Controls.Add(this.comboBoxColumnName);
      this.panelTop.Controls.Add(this.comboBoxDataType);
      this.panelTop.Controls.Add(this.checkBoxIgnore);
      this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.panelTop.Location = new System.Drawing.Point(0, 0);
      this.panelTop.Margin = new System.Windows.Forms.Padding(2);
      this.panelTop.Name = "panelTop";
      this.panelTop.Size = new System.Drawing.Size(627, 50);
      this.panelTop.TabIndex = 6;
      // 
      // panelBottom
      // 
      this.panelBottom.Controls.Add(this.buttonOK);
      this.panelBottom.Controls.Add(this.buttonCancel);
      this.panelBottom.Controls.Add(this.buttonGuess);
      this.panelBottom.Controls.Add(this.buttonDisplayValues);
      this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panelBottom.Location = new System.Drawing.Point(0, 806);
      this.panelBottom.Margin = new System.Windows.Forms.Padding(2);
      this.panelBottom.Name = "panelBottom";
      this.panelBottom.Size = new System.Drawing.Size(627, 28);
      this.panelBottom.TabIndex = 7;
      // 
      // flowLayoutPanel
      // 
      this.flowLayoutPanel.AutoSize = true;
      this.flowLayoutPanel.Controls.Add(this.groupBoxDate);
      this.flowLayoutPanel.Controls.Add(this.groupBoxNumber);
      this.flowLayoutPanel.Controls.Add(this.groupBoxBoolean);
      this.flowLayoutPanel.Controls.Add(this.groupBoxSplit);
      this.flowLayoutPanel.Controls.Add(this.groupBoxBinary);
      this.flowLayoutPanel.Controls.Add(this.groupBoxRegExReplace);
      this.flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
      this.flowLayoutPanel.Location = new System.Drawing.Point(0, 48);
      this.flowLayoutPanel.Name = "flowLayoutPanel";
      this.flowLayoutPanel.Size = new System.Drawing.Size(622, 758);
      this.flowLayoutPanel.TabIndex = 8;
      this.flowLayoutPanel.WrapContents = false;
      // 
      // groupBoxDate
      // 
      this.groupBoxDate.Controls.Add(tableLayoutPanel2);
      this.groupBoxDate.Location = new System.Drawing.Point(3, 3);
      this.groupBoxDate.Name = "groupBoxDate";
      this.groupBoxDate.Padding = new System.Windows.Forms.Padding(0);
      this.groupBoxDate.Size = new System.Drawing.Size(616, 287);
      this.groupBoxDate.TabIndex = 15;
      this.groupBoxDate.TabStop = false;
      this.groupBoxDate.Text = "Date";
      // 
      // groupBoxNumber
      // 
      this.groupBoxNumber.Controls.Add(this.linkLabel2);
      this.groupBoxNumber.Controls.Add(this.comboBoxNumberFormat);
      this.groupBoxNumber.Controls.Add(this.label2);
      this.groupBoxNumber.Controls.Add(this.labelNumberOutput);
      this.groupBoxNumber.Controls.Add(this.label16);
      this.groupBoxNumber.Controls.Add(labelGroup);
      this.groupBoxNumber.Controls.Add(this.textBoxDecimalSeparator);
      this.groupBoxNumber.Controls.Add(labelPoint);
      this.groupBoxNumber.Controls.Add(this.textBoxGroupSeparator);
      this.groupBoxNumber.Controls.Add(this.labelNumber);
      this.groupBoxNumber.Location = new System.Drawing.Point(3, 296);
      this.groupBoxNumber.Name = "groupBoxNumber";
      this.groupBoxNumber.Padding = new System.Windows.Forms.Padding(0);
      this.groupBoxNumber.Size = new System.Drawing.Size(616, 94);
      this.groupBoxNumber.TabIndex = 7;
      this.groupBoxNumber.TabStop = false;
      this.groupBoxNumber.Text = "Number";
      this.groupBoxNumber.Visible = false;
      // 
      // linkLabel2
      // 
      this.linkLabel2.AutoSize = true;
      this.linkLabel2.Location = new System.Drawing.Point(173, 60);
      this.linkLabel2.Name = "linkLabel2";
      this.linkLabel2.Size = new System.Drawing.Size(130, 13);
      this.linkLabel2.TabIndex = 3;
      this.linkLabel2.TabStop = true;
      this.linkLabel2.Text = "Open Region && Language";
      this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RegionAndLanguageLinkClicked);
      // 
      // comboBoxNumberFormat
      // 
      this.comboBoxNumberFormat.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "NumberFormat", true));
      this.comboBoxNumberFormat.FormattingEnabled = true;
      this.comboBoxNumberFormat.Items.AddRange(new object[] {
            "0.#####",
            "0.00",
            "#,##0.##"});
      this.comboBoxNumberFormat.Location = new System.Drawing.Point(116, 18);
      this.comboBoxNumberFormat.Name = "comboBoxNumberFormat";
      this.comboBoxNumberFormat.Size = new System.Drawing.Size(162, 21);
      this.comboBoxNumberFormat.TabIndex = 0;
      this.comboBoxNumberFormat.TextChanged += new System.EventHandler(this.NumberFormatChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.ForeColor = System.Drawing.SystemColors.Highlight;
      this.label2.Location = new System.Drawing.Point(297, 21);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(95, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Value: \"1234.567\"";
      // 
      // labelNumberOutput
      // 
      this.labelNumberOutput.AutoSize = true;
      this.labelNumberOutput.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelNumberOutput.Location = new System.Drawing.Point(297, 68);
      this.labelNumberOutput.Name = "labelNumberOutput";
      this.labelNumberOutput.Size = new System.Drawing.Size(55, 13);
      this.labelNumberOutput.TabIndex = 7;
      this.labelNumberOutput.Text = "Output: \"\"";
      // 
      // label16
      // 
      this.label16.AutoSize = true;
      this.label16.Location = new System.Drawing.Point(25, 21);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(82, 13);
      this.label16.TabIndex = 0;
      this.label16.Text = "Number Format:";
      // 
      // textBoxDecimalSeparator
      // 
      this.textBoxDecimalSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "DecimalSeparator", true));
      this.textBoxDecimalSeparator.Location = new System.Drawing.Point(116, 65);
      this.textBoxDecimalSeparator.Name = "textBoxDecimalSeparator";
      this.textBoxDecimalSeparator.Size = new System.Drawing.Size(28, 20);
      this.textBoxDecimalSeparator.TabIndex = 2;
      this.textBoxDecimalSeparator.Text = ".";
      this.textBoxDecimalSeparator.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.textBoxDecimalSeparator.TextChanged += new System.EventHandler(this.NumberFormatChanged);
      this.textBoxDecimalSeparator.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxDecimalSeparator_Validating);
      // 
      // textBoxGroupSeparator
      // 
      this.textBoxGroupSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "GroupSeparator", true));
      this.textBoxGroupSeparator.Location = new System.Drawing.Point(116, 42);
      this.textBoxGroupSeparator.Name = "textBoxGroupSeparator";
      this.textBoxGroupSeparator.Size = new System.Drawing.Size(28, 20);
      this.textBoxGroupSeparator.TabIndex = 1;
      this.textBoxGroupSeparator.Text = ",";
      this.textBoxGroupSeparator.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.textBoxGroupSeparator.TextChanged += new System.EventHandler(this.NumberFormatChanged);
      // 
      // labelNumber
      // 
      this.labelNumber.AutoSize = true;
      this.labelNumber.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelNumber.Location = new System.Drawing.Point(297, 45);
      this.labelNumber.Name = "labelNumber";
      this.labelNumber.Size = new System.Drawing.Size(47, 13);
      this.labelNumber.TabIndex = 6;
      this.labelNumber.Text = "Input: \"\"";
      // 
      // groupBoxBoolean
      // 
      this.groupBoxBoolean.Controls.Add(labelTrue);
      this.groupBoxBoolean.Controls.Add(labelFalse);
      this.groupBoxBoolean.Controls.Add(this.textBoxTrue);
      this.groupBoxBoolean.Controls.Add(this.textBoxFalse);
      this.groupBoxBoolean.Location = new System.Drawing.Point(3, 396);
      this.groupBoxBoolean.Name = "groupBoxBoolean";
      this.groupBoxBoolean.Padding = new System.Windows.Forms.Padding(0);
      this.groupBoxBoolean.Size = new System.Drawing.Size(616, 67);
      this.groupBoxBoolean.TabIndex = 8;
      this.groupBoxBoolean.TabStop = false;
      this.groupBoxBoolean.Text = "Boolean";
      // 
      // textBoxTrue
      // 
      this.textBoxTrue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "True", true));
      this.textBoxTrue.Location = new System.Drawing.Point(113, 13);
      this.textBoxTrue.Name = "textBoxTrue";
      this.textBoxTrue.Size = new System.Drawing.Size(45, 20);
      this.textBoxTrue.TabIndex = 0;
      // 
      // textBoxFalse
      // 
      this.textBoxFalse.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "False", true));
      this.textBoxFalse.Location = new System.Drawing.Point(113, 38);
      this.textBoxFalse.Name = "textBoxFalse";
      this.textBoxFalse.Size = new System.Drawing.Size(45, 20);
      this.textBoxFalse.TabIndex = 1;
      // 
      // groupBoxSplit
      // 
      this.groupBoxSplit.Controls.Add(this.numericUpDownPart);
      this.groupBoxSplit.Controls.Add(this.labelSamplePart);
      this.groupBoxSplit.Controls.Add(this.checkBoxPartToEnd);
      this.groupBoxSplit.Controls.Add(labelSepBy);
      this.groupBoxSplit.Controls.Add(labelPart);
      this.groupBoxSplit.Controls.Add(this.textBoxSplit);
      this.groupBoxSplit.Controls.Add(this.labelResultPart);
      this.groupBoxSplit.Location = new System.Drawing.Point(3, 469);
      this.groupBoxSplit.Name = "groupBoxSplit";
      this.groupBoxSplit.Padding = new System.Windows.Forms.Padding(0);
      this.groupBoxSplit.Size = new System.Drawing.Size(616, 70);
      this.groupBoxSplit.TabIndex = 9;
      this.groupBoxSplit.TabStop = false;
      this.groupBoxSplit.Text = "Text Part";
      this.groupBoxSplit.Visible = false;
      // 
      // numericUpDownPart
      // 
      this.numericUpDownPart.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.bindingSourceValueFormat, "Part", true));
      this.numericUpDownPart.Location = new System.Drawing.Point(113, 42);
      this.numericUpDownPart.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
      this.numericUpDownPart.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownPart.Name = "numericUpDownPart";
      this.numericUpDownPart.Size = new System.Drawing.Size(41, 20);
      this.numericUpDownPart.TabIndex = 7;
      this.numericUpDownPart.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.numericUpDownPart.ValueChanged += new System.EventHandler(this.SetSamplePart);
      this.numericUpDownPart.Validating += new System.ComponentModel.CancelEventHandler(this.PartValidating);
      // 
      // labelSamplePart
      // 
      this.labelSamplePart.AutoSize = true;
      this.labelSamplePart.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelSamplePart.Location = new System.Drawing.Point(297, 19);
      this.labelSamplePart.Name = "labelSamplePart";
      this.labelSamplePart.Size = new System.Drawing.Size(47, 13);
      this.labelSamplePart.TabIndex = 2;
      this.labelSamplePart.Text = "Input: \"\"";
      // 
      // checkBoxPartToEnd
      // 
      this.checkBoxPartToEnd.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceValueFormat, "PartToEnd", true));
      this.checkBoxPartToEnd.Location = new System.Drawing.Point(160, 41);
      this.checkBoxPartToEnd.Name = "checkBoxPartToEnd";
      this.checkBoxPartToEnd.Size = new System.Drawing.Size(72, 21);
      this.checkBoxPartToEnd.TabIndex = 2;
      this.checkBoxPartToEnd.Text = "To End";
      this.checkBoxPartToEnd.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
      this.checkBoxPartToEnd.UseVisualStyleBackColor = false;
      this.checkBoxPartToEnd.CheckedChanged += new System.EventHandler(this.SetSamplePart);
      this.checkBoxPartToEnd.Validating += new System.ComponentModel.CancelEventHandler(this.PartValidating);
      // 
      // textBoxSplit
      // 
      this.textBoxSplit.AutoCompleteCustomSource.AddRange(new string[] {
            ":",
            ";",
            "|"});
      this.textBoxSplit.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "PartSplitter", true));
      this.textBoxSplit.Location = new System.Drawing.Point(113, 16);
      this.textBoxSplit.MaxLength = 1;
      this.textBoxSplit.Name = "textBoxSplit";
      this.textBoxSplit.Size = new System.Drawing.Size(25, 20);
      this.textBoxSplit.TabIndex = 0;
      this.textBoxSplit.Text = ":";
      this.textBoxSplit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.textBoxSplit.TextChanged += new System.EventHandler(this.SetSamplePart);
      this.textBoxSplit.Validating += new System.ComponentModel.CancelEventHandler(this.PartValidating);
      // 
      // labelResultPart
      // 
      this.labelResultPart.AutoSize = true;
      this.labelResultPart.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelResultPart.Location = new System.Drawing.Point(297, 44);
      this.labelResultPart.Name = "labelResultPart";
      this.labelResultPart.Size = new System.Drawing.Size(55, 13);
      this.labelResultPart.TabIndex = 6;
      this.labelResultPart.Text = "Output: \"\"";
      // 
      // groupBoxBinary
      // 
      this.groupBoxBinary.Controls.Add(this.checkBoxOverwrite);
      this.groupBoxBinary.Controls.Add(this.textBoxReadFolder);
      this.groupBoxBinary.Controls.Add(label9);
      this.groupBoxBinary.Controls.Add(this.textBoxWriteFolder);
      this.groupBoxBinary.Controls.Add(label1);
      this.groupBoxBinary.Controls.Add(this.textBoxPattern);
      this.groupBoxBinary.Controls.Add(label7);
      this.groupBoxBinary.Location = new System.Drawing.Point(3, 545);
      this.groupBoxBinary.Name = "groupBoxBinary";
      this.groupBoxBinary.Padding = new System.Windows.Forms.Padding(0);
      this.groupBoxBinary.Size = new System.Drawing.Size(616, 102);
      this.groupBoxBinary.TabIndex = 13;
      this.groupBoxBinary.TabStop = false;
      this.groupBoxBinary.Text = "Binary Data";
      this.groupBoxBinary.Visible = false;
      // 
      // checkBoxOverwrite
      // 
      this.checkBoxOverwrite.AutoSize = true;
      this.checkBoxOverwrite.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceValueFormat, "Overwrite", true));
      this.checkBoxOverwrite.Location = new System.Drawing.Point(363, 74);
      this.checkBoxOverwrite.Name = "checkBoxOverwrite";
      this.checkBoxOverwrite.Size = new System.Drawing.Size(71, 17);
      this.checkBoxOverwrite.TabIndex = 7;
      this.checkBoxOverwrite.Text = "Overwrite";
      this.checkBoxOverwrite.UseVisualStyleBackColor = true;
      // 
      // groupBoxRegExReplace
      // 
      this.groupBoxRegExReplace.AutoSize = true;
      this.groupBoxRegExReplace.Controls.Add(this.label8);
      this.groupBoxRegExReplace.Controls.Add(this.textBoxRegexReplacement);
      this.groupBoxRegExReplace.Controls.Add(labelReplace);
      this.groupBoxRegExReplace.Controls.Add(this.textBoxRegexSearchPattern);
      this.groupBoxRegExReplace.Controls.Add(labelSPattern);
      this.groupBoxRegExReplace.Location = new System.Drawing.Point(3, 653);
      this.groupBoxRegExReplace.Name = "groupBoxRegExReplace";
      this.groupBoxRegExReplace.Padding = new System.Windows.Forms.Padding(0);
      this.groupBoxRegExReplace.Size = new System.Drawing.Size(616, 80);
      this.groupBoxRegExReplace.TabIndex = 14;
      this.groupBoxRegExReplace.TabStop = false;
      this.groupBoxRegExReplace.Text = "Text Replace";
      this.groupBoxRegExReplace.Visible = false;
      // 
      // label8
      // 
      this.label8.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.label8.AutoSize = true;
      this.label8.ForeColor = System.Drawing.SystemColors.Highlight;
      this.label8.Location = new System.Drawing.Point(364, 21);
      this.label8.Margin = new System.Windows.Forms.Padding(3);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(140, 13);
      this.label8.TabIndex = 12;
      this.label8.Text = "Note: RegEx Pattern Syntax";
      // 
      // textBoxRegexReplacement
      // 
      this.textBoxRegexReplacement.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "RegexReplacement", true));
      this.textBoxRegexReplacement.Location = new System.Drawing.Point(113, 44);
      this.textBoxRegexReplacement.Name = "textBoxRegexReplacement";
      this.textBoxRegexReplacement.Size = new System.Drawing.Size(239, 20);
      this.textBoxRegexReplacement.TabIndex = 9;
      // 
      // labelDisplayNullAs
      // 
      this.labelDisplayNullAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelDisplayNullAs.AutoSize = true;
      this.labelDisplayNullAs.Location = new System.Drawing.Point(241, 30);
      this.labelDisplayNullAs.Margin = new System.Windows.Forms.Padding(3);
      this.labelDisplayNullAs.Name = "labelDisplayNullAs";
      this.labelDisplayNullAs.Size = new System.Drawing.Size(80, 13);
      this.labelDisplayNullAs.TabIndex = 12;
      this.labelDisplayNullAs.Text = "Write NULL as:";
      // 
      // textBoxDisplayNullAs
      // 
      this.textBoxDisplayNullAs.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "DisplayNullAs", true));
      this.textBoxDisplayNullAs.Location = new System.Drawing.Point(325, 27);
      this.textBoxDisplayNullAs.Name = "textBoxDisplayNullAs";
      this.textBoxDisplayNullAs.Size = new System.Drawing.Size(93, 20);
      this.textBoxDisplayNullAs.TabIndex = 11;
      this.toolTip.SetToolTip(this.textBoxDisplayNullAs, "Wrting data empty field (NULL) can be an empty column or represented by this text" +
        " \r\ne.G. <NULL>");
      // 
      // FormColumnUiRead
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(627, 834);
      this.Controls.Add(this.panelBottom);
      this.Controls.Add(this.flowLayoutPanel);
      this.Controls.Add(this.panelTop);
      this.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "Name", true));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(519, 186);
      this.Name = "FormColumnUiRead";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Column Format";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ColumnFormatUI_FormClosing);
      this.Load += new System.EventHandler(this.ColumnFormatUI_Load);
      tableLayoutPanel2.ResumeLayout(false);
      tableLayoutPanel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.bindingSourceValueFormat)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.columnBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
      this.panelTop.ResumeLayout(false);
      this.panelTop.PerformLayout();
      this.panelBottom.ResumeLayout(false);
      this.flowLayoutPanel.ResumeLayout(false);
      this.flowLayoutPanel.PerformLayout();
      this.groupBoxDate.ResumeLayout(false);
      this.groupBoxDate.PerformLayout();
      this.groupBoxNumber.ResumeLayout(false);
      this.groupBoxNumber.PerformLayout();
      this.groupBoxBoolean.ResumeLayout(false);
      this.groupBoxBoolean.PerformLayout();
      this.groupBoxSplit.ResumeLayout(false);
      this.groupBoxSplit.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPart)).EndInit();
      this.groupBoxBinary.ResumeLayout(false);
      this.groupBoxBinary.PerformLayout();
      this.groupBoxRegExReplace.ResumeLayout(false);
      this.groupBoxRegExReplace.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    private System.Windows.Forms.BindingSource bindingSourceValueFormat;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonDisplayValues;
    private System.Windows.Forms.Button buttonGuess;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.CheckBox checkBoxIgnore;
    private System.Windows.Forms.BindingSource columnBindingSource;
    private System.Windows.Forms.ComboBox comboBoxColumnName;
    private System.Windows.Forms.ComboBox comboBoxDataType;
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.Label labelColName;
    private System.Windows.Forms.TextBox textBoxColumnName;
    private System.Windows.Forms.ToolTip toolTip;

#endregion

    private System.Boolean m_DisposedValue;
    private System.Windows.Forms.Panel panelBottom;
    private System.Windows.Forms.Panel panelTop;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
    private System.Windows.Forms.GroupBox groupBoxNumber;
    private System.Windows.Forms.ComboBox comboBoxNumberFormat;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label labelNumberOutput;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.TextBox textBoxDecimalSeparator;
    private System.Windows.Forms.TextBox textBoxGroupSeparator;
    private System.Windows.Forms.Label labelNumber;
    private System.Windows.Forms.GroupBox groupBoxBoolean;
    private System.Windows.Forms.TextBox textBoxTrue;
    private System.Windows.Forms.TextBox textBoxFalse;
    private System.Windows.Forms.GroupBox groupBoxSplit;
    private System.Windows.Forms.NumericUpDown numericUpDownPart;
    private System.Windows.Forms.Label labelSamplePart;
    private System.Windows.Forms.CheckBox checkBoxPartToEnd;
    private System.Windows.Forms.TextBox textBoxSplit;
    private System.Windows.Forms.Label labelResultPart;
    private System.Windows.Forms.GroupBox groupBoxBinary;
    private System.Windows.Forms.CheckBox checkBoxOverwrite;
    private System.Windows.Forms.TextBox textBoxReadFolder;
    private System.Windows.Forms.TextBox textBoxWriteFolder;
    private System.Windows.Forms.TextBox textBoxPattern;
    private System.Windows.Forms.GroupBox groupBoxRegExReplace;
    private System.Windows.Forms.Label label8;
    private System.Windows.Forms.TextBox textBoxRegexReplacement;
    private System.Windows.Forms.TextBox textBoxRegexSearchPattern;
    private System.Windows.Forms.GroupBox groupBoxDate;
    private System.Windows.Forms.TextBox textBoxDateSeparator;
    private System.Windows.Forms.ComboBox comboBoxDateFormat;
    private System.Windows.Forms.Button buttonAddFormat;
    private System.Windows.Forms.Label labelAllowedDateFormats;
    private System.Windows.Forms.CheckedListBox checkedListBoxDateFormats;
    private System.Windows.Forms.ComboBox comboBoxTimePart;
    private System.Windows.Forms.TextBox textBoxTimeSeparator;
    private System.Windows.Forms.ComboBox comboBoxTimeZone;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label labelSampleDisplay;
    private System.Windows.Forms.Label labelDateOutputDisplay;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label labelInputTZ;
    private System.Windows.Forms.Label labelOutPutTZ;
    private System.Windows.Forms.ComboBox comboBoxTPFormat;
    private System.Windows.Forms.LinkLabel linkLabel2;
    private System.Windows.Forms.Label labelDisplayNullAs;
    private System.Windows.Forms.TextBox textBoxDisplayNullAs;
  }
}