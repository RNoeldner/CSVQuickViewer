namespace CsvTools
{
  /// <summary>
  /// ColumnFormatUI Form from Designer
  /// </summary>
  partial class FormColumnUI
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
      System.Windows.Forms.Label labelTrue;
      System.Windows.Forms.Label labelFalse;
      System.Windows.Forms.Label labelPoint;
      System.Windows.Forms.Label labelDateSep;
      System.Windows.Forms.Label labelGroup;
      System.Windows.Forms.Label labelTimeSep;
      System.Windows.Forms.Label labelLessCommon;
      System.Windows.Forms.Label labelTCFormat;
      System.Windows.Forms.Label labelTimeCol;
      System.Windows.Forms.Label labelSepBy;
      System.Windows.Forms.Label labelPart;
      System.Windows.Forms.Label label3;
      System.Windows.Forms.Label labelDateOutput;
      System.Windows.Forms.Label labelSample;
      System.Windows.Forms.LinkLabel linkLabelRegion;
      System.Windows.Forms.Label label4;
      System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
      System.Windows.Forms.Label label7;
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
      this.textBoxTrue = new System.Windows.Forms.TextBox();
      this.textBoxFalse = new System.Windows.Forms.TextBox();
      this.textBoxGroupSeparator = new System.Windows.Forms.TextBox();
      this.textBoxDecimalSeparator = new System.Windows.Forms.TextBox();
      this.label16 = new System.Windows.Forms.Label();
      this.comboBoxDataType = new System.Windows.Forms.ComboBox();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.labelNumber = new System.Windows.Forms.Label();
      this.labelNumberOutput = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBoxDate = new System.Windows.Forms.GroupBox();
      this.groupBoxNumber = new System.Windows.Forms.GroupBox();
      this.linkLabel2 = new System.Windows.Forms.LinkLabel();
      this.comboBoxNumberFormat = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.groupBoxBoolean = new System.Windows.Forms.GroupBox();
      this.comboBoxColumnName = new System.Windows.Forms.ComboBox();
      this.buttonGuess = new System.Windows.Forms.Button();
      this.checkBoxIgnore = new System.Windows.Forms.CheckBox();
      this.groupBoxSplit = new System.Windows.Forms.GroupBox();
      this.numericUpDownPart = new System.Windows.Forms.NumericUpDown();
      this.labelSamplePart = new System.Windows.Forms.Label();
      this.checkBoxPartToEnd = new System.Windows.Forms.CheckBox();
      this.textBoxSplit = new System.Windows.Forms.TextBox();
      this.labelResultPart = new System.Windows.Forms.Label();
      this.textBoxColumnName = new System.Windows.Forms.TextBox();
      this.labelDisplayNullAs = new System.Windows.Forms.Label();
      this.textBoxDisplayNullAs = new System.Windows.Forms.TextBox();
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.buttonDisplayValues = new System.Windows.Forms.Button();
      this.tableLayoutPanelForm = new System.Windows.Forms.TableLayoutPanel();
      this.buttonOK = new System.Windows.Forms.Button();
      this.groupBoxBinary = new System.Windows.Forms.GroupBox();
      this.textBoxPattern = new System.Windows.Forms.TextBox();
      labelTrue = new System.Windows.Forms.Label();
      labelFalse = new System.Windows.Forms.Label();
      labelPoint = new System.Windows.Forms.Label();
      labelDateSep = new System.Windows.Forms.Label();
      labelGroup = new System.Windows.Forms.Label();
      labelTimeSep = new System.Windows.Forms.Label();
      labelLessCommon = new System.Windows.Forms.Label();
      labelTCFormat = new System.Windows.Forms.Label();
      labelTimeCol = new System.Windows.Forms.Label();
      labelSepBy = new System.Windows.Forms.Label();
      labelPart = new System.Windows.Forms.Label();
      label3 = new System.Windows.Forms.Label();
      labelDateOutput = new System.Windows.Forms.Label();
      labelSample = new System.Windows.Forms.Label();
      linkLabelRegion = new System.Windows.Forms.LinkLabel();
      label4 = new System.Windows.Forms.Label();
      tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      label7 = new System.Windows.Forms.Label();
      tableLayoutPanel2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.bindingSourceValueFormat)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.columnBindingSource)).BeginInit();
      this.groupBoxDate.SuspendLayout();
      this.groupBoxNumber.SuspendLayout();
      this.groupBoxBoolean.SuspendLayout();
      this.groupBoxSplit.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPart)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      this.tableLayoutPanelForm.SuspendLayout();
      this.groupBoxBinary.SuspendLayout();
      this.SuspendLayout();
      // 
      // labelTrue
      // 
      labelTrue.AutoSize = true;
      labelTrue.Location = new System.Drawing.Point(77, 17);
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
      labelFalse.Location = new System.Drawing.Point(72, 37);
      labelFalse.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelFalse.Name = "labelFalse";
      labelFalse.Size = new System.Drawing.Size(35, 13);
      labelFalse.TabIndex = 2;
      labelFalse.Text = "False:";
      labelFalse.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelPoint
      // 
      labelPoint.AutoSize = true;
      labelPoint.Location = new System.Drawing.Point(37, 63);
      labelPoint.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelPoint.Name = "labelPoint";
      labelPoint.Size = new System.Drawing.Size(75, 13);
      labelPoint.TabIndex = 8;
      labelPoint.Text = "Decimal Point:";
      // 
      // labelDateSep
      // 
      labelDateSep.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelDateSep.AutoSize = true;
      labelDateSep.Location = new System.Drawing.Point(25, 5);
      labelDateSep.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      labelDateSep.Name = "labelDateSep";
      labelDateSep.Size = new System.Drawing.Size(82, 13);
      labelDateSep.TabIndex = 0;
      labelDateSep.Text = "Date Separator:";
      // 
      // labelGroup
      // 
      labelGroup.AutoSize = true;
      labelGroup.Location = new System.Drawing.Point(2, 43);
      labelGroup.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelGroup.Name = "labelGroup";
      labelGroup.Size = new System.Drawing.Size(107, 13);
      labelGroup.TabIndex = 4;
      labelGroup.Text = "Thousand Separator:";
      // 
      // labelTimeSep
      // 
      labelTimeSep.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelTimeSep.AutoSize = true;
      labelTimeSep.Location = new System.Drawing.Point(160, 5);
      labelTimeSep.Margin = new System.Windows.Forms.Padding(2);
      labelTimeSep.Name = "labelTimeSep";
      labelTimeSep.Size = new System.Drawing.Size(82, 13);
      labelTimeSep.TabIndex = 5;
      labelTimeSep.Text = "Time Separator:";
      // 
      // labelLessCommon
      // 
      labelLessCommon.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelLessCommon.AutoSize = true;
      labelLessCommon.Location = new System.Drawing.Point(3, 149);
      labelLessCommon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      labelLessCommon.Name = "labelLessCommon";
      labelLessCommon.Size = new System.Drawing.Size(104, 13);
      labelLessCommon.TabIndex = 16;
      labelLessCommon.Text = "Uncommon Formats:";
      // 
      // labelTCFormat
      // 
      labelTCFormat.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelTCFormat.AutoSize = true;
      labelTCFormat.Location = new System.Drawing.Point(281, 55);
      labelTCFormat.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      labelTCFormat.Name = "labelTCFormat";
      labelTCFormat.Size = new System.Drawing.Size(42, 13);
      labelTCFormat.TabIndex = 3;
      labelTCFormat.Text = "Format:";
      // 
      // labelTimeCol
      // 
      labelTimeCol.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelTimeCol.AutoSize = true;
      labelTimeCol.Location = new System.Drawing.Point(14, 55);
      labelTimeCol.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      labelTimeCol.Name = "labelTimeCol";
      labelTimeCol.Size = new System.Drawing.Size(93, 13);
      labelTimeCol.TabIndex = 1;
      labelTimeCol.Text = "Column with Time:";
      // 
      // labelSepBy
      // 
      labelSepBy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      labelSepBy.AutoSize = true;
      labelSepBy.Location = new System.Drawing.Point(35, 17);
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
      labelPart.Location = new System.Drawing.Point(79, 41);
      labelPart.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelPart.Name = "labelPart";
      labelPart.Size = new System.Drawing.Size(29, 13);
      labelPart.TabIndex = 3;
      labelPart.Text = "Part:";
      labelPart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label3
      // 
      label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
      label3.AutoSize = true;
      label3.Location = new System.Drawing.Point(43, 30);
      label3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      label3.Name = "label3";
      label3.Size = new System.Drawing.Size(64, 13);
      label3.TabIndex = 7;
      label3.Text = "Time Zone :";
      // 
      // labelDateOutput
      // 
      labelDateOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      labelDateOutput.AutoSize = true;
      labelDateOutput.ForeColor = System.Drawing.SystemColors.ControlText;
      labelDateOutput.Location = new System.Drawing.Point(281, 106);
      labelDateOutput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
      labelSample.Location = new System.Drawing.Point(289, 91);
      labelSample.Name = "labelSample";
      labelSample.Size = new System.Drawing.Size(34, 13);
      labelSample.TabIndex = 12;
      labelSample.Text = "Input:";
      // 
      // linkLabelRegion
      // 
      linkLabelRegion.Anchor = System.Windows.Forms.AnchorStyles.Left;
      linkLabelRegion.AutoSize = true;
      tableLayoutPanel2.SetColumnSpan(linkLabelRegion, 3);
      linkLabelRegion.Location = new System.Drawing.Point(281, 125);
      linkLabelRegion.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      linkLabelRegion.Name = "linkLabelRegion";
      linkLabelRegion.Size = new System.Drawing.Size(130, 13);
      linkLabelRegion.TabIndex = 14;
      linkLabelRegion.TabStop = true;
      linkLabelRegion.Text = "Open Region && Language";
      linkLabelRegion.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RegionAndLanguageLinkClicked);
      // 
      // label4
      // 
      label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      label4.AutoSize = true;
      label4.ForeColor = System.Drawing.SystemColors.ControlText;
      label4.Location = new System.Drawing.Point(286, 76);
      label4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      label4.Name = "label4";
      label4.Size = new System.Drawing.Size(37, 13);
      label4.TabIndex = 11;
      label4.Text = "Value:";
      // 
      // tableLayoutPanel2
      // 
      tableLayoutPanel2.AutoSize = true;
      tableLayoutPanel2.ColumnCount = 7;
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 34F));
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 34F));
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 62F));
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
      tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
      tableLayoutPanel2.Location = new System.Drawing.Point(3, 15);
      tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
      tableLayoutPanel2.Size = new System.Drawing.Size(542, 169);
      tableLayoutPanel2.TabIndex = 17;
      // 
      // textBoxDateSeparator
      // 
      this.textBoxDateSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "DateSeparator", true));
      this.textBoxDateSeparator.Location = new System.Drawing.Point(113, 2);
      this.textBoxDateSeparator.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.textBoxDateSeparator.Name = "textBoxDateSeparator";
      this.textBoxDateSeparator.Size = new System.Drawing.Size(28, 20);
      this.textBoxDateSeparator.TabIndex = 0;
      this.toolTip.SetToolTip(this.textBoxDateSeparator, "Separates the components of a date, that is, the year, month, and day");
      this.textBoxDateSeparator.TextChanged += new System.EventHandler(this.DateFormatChanged);
      // 
      // bindingSourceValueFormat
      // 
      this.bindingSourceValueFormat.AllowNew = false;
      this.bindingSourceValueFormat.DataSource = typeof(CsvTools.ValueFormatMutable);
      // 
      // comboBoxDateFormat
      // 
      this.comboBoxDateFormat.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.comboBoxDateFormat.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
      tableLayoutPanel2.SetColumnSpan(this.comboBoxDateFormat, 3);
      this.comboBoxDateFormat.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxDateFormat.FormattingEnabled = true;
      this.comboBoxDateFormat.Location = new System.Drawing.Point(113, 144);
      this.comboBoxDateFormat.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.comboBoxDateFormat.Name = "comboBoxDateFormat";
      this.comboBoxDateFormat.Size = new System.Drawing.Size(162, 21);
      this.comboBoxDateFormat.TabIndex = 6;
      this.comboBoxDateFormat.TextChanged += new System.EventHandler(this.DateFormatChanged);
      // 
      // buttonAddFormat
      // 
      this.buttonAddFormat.AutoSize = true;
      tableLayoutPanel2.SetColumnSpan(this.buttonAddFormat, 2);
      this.buttonAddFormat.Location = new System.Drawing.Point(281, 144);
      this.buttonAddFormat.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.buttonAddFormat.Name = "buttonAddFormat";
      this.buttonAddFormat.Size = new System.Drawing.Size(85, 23);
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
      this.labelAllowedDateFormats.Location = new System.Drawing.Point(28, 76);
      this.labelAllowedDateFormats.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
      this.checkedListBoxDateFormats.Location = new System.Drawing.Point(113, 76);
      this.checkedListBoxDateFormats.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.checkedListBoxDateFormats.Name = "checkedListBoxDateFormats";
      tableLayoutPanel2.SetRowSpan(this.checkedListBoxDateFormats, 4);
      this.checkedListBoxDateFormats.Size = new System.Drawing.Size(162, 64);
      this.checkedListBoxDateFormats.TabIndex = 5;
      this.toolTip.SetToolTip(this.checkedListBoxDateFormats, "Common Date/Time formats, you can choose multiple");
      this.checkedListBoxDateFormats.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CheckedListBoxDateFormats_ItemCheck);
      this.checkedListBoxDateFormats.SelectedIndexChanged += new System.EventHandler(this.DateFormatChanged);
      // 
      // comboBoxTimePart
      // 
      this.comboBoxTimePart.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      tableLayoutPanel2.SetColumnSpan(this.comboBoxTimePart, 3);
      this.comboBoxTimePart.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "TimePart", true));
      this.comboBoxTimePart.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxTimePart.FormattingEnabled = true;
      this.comboBoxTimePart.Location = new System.Drawing.Point(113, 51);
      this.comboBoxTimePart.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.comboBoxTimePart.Name = "comboBoxTimePart";
      this.comboBoxTimePart.Size = new System.Drawing.Size(162, 21);
      this.comboBoxTimePart.TabIndex = 1;
      this.toolTip.SetToolTip(this.comboBoxTimePart, "Combining a time column will result in a combination of the column and the select" +
        "ed time column\r\ne.G “17/Aug/2019” & “17:54” will become “17/Aug/2019 17:54”\r\n");
      this.comboBoxTimePart.SelectedIndexChanged += new System.EventHandler(this.ComboBoxTimePart_SelectedIndexChanged);
      this.comboBoxTimePart.TextUpdate += new System.EventHandler(this.DateFormatChanged);
      // 
      // columnBindingSource
      // 
      this.columnBindingSource.AllowNew = false;
      this.columnBindingSource.DataSource = typeof(CsvTools.Column);
      // 
      // textBoxTimeSeparator
      // 
      this.textBoxTimeSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "TimeSeparator", true));
      this.textBoxTimeSeparator.Location = new System.Drawing.Point(247, 2);
      this.textBoxTimeSeparator.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.textBoxTimeSeparator.Name = "textBoxTimeSeparator";
      this.textBoxTimeSeparator.Size = new System.Drawing.Size(28, 20);
      this.textBoxTimeSeparator.TabIndex = 3;
      this.toolTip.SetToolTip(this.textBoxTimeSeparator, "Separates the components of time, that is, the hour, minutes, and seconds.");
      // 
      // comboBoxTimeZone
      // 
      tableLayoutPanel2.SetColumnSpan(this.comboBoxTimeZone, 3);
      this.comboBoxTimeZone.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "TimeZonePart", true));
      this.comboBoxTimeZone.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxTimeZone.FormattingEnabled = true;
      this.comboBoxTimeZone.Location = new System.Drawing.Point(113, 26);
      this.comboBoxTimeZone.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.comboBoxTimeZone.Name = "comboBoxTimeZone";
      this.comboBoxTimeZone.Size = new System.Drawing.Size(162, 21);
      this.comboBoxTimeZone.TabIndex = 4;
      this.comboBoxTimeZone.SelectedIndexChanged += new System.EventHandler(this.DateFormatChanged);
      this.comboBoxTimeZone.TextUpdate += new System.EventHandler(this.DateFormatChanged);
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.ForeColor = System.Drawing.SystemColors.Highlight;
      this.label5.Location = new System.Drawing.Point(329, 76);
      this.label5.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(141, 13);
      this.label5.TabIndex = 11;
      this.label5.Text = "7th April 2013  15:45:50 345";
      // 
      // labelSampleDisplay
      // 
      this.labelSampleDisplay.AutoSize = true;
      this.labelSampleDisplay.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelSampleDisplay.Location = new System.Drawing.Point(329, 91);
      this.labelSampleDisplay.Name = "labelSampleDisplay";
      this.labelSampleDisplay.Size = new System.Drawing.Size(17, 13);
      this.labelSampleDisplay.TabIndex = 12;
      this.labelSampleDisplay.Text = "\"\"";
      // 
      // labelDateOutputDisplay
      // 
      this.labelDateOutputDisplay.AutoSize = true;
      this.labelDateOutputDisplay.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelDateOutputDisplay.Location = new System.Drawing.Point(329, 106);
      this.labelDateOutputDisplay.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
      this.label6.Location = new System.Drawing.Point(281, 30);
      this.label6.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(187, 13);
      this.label6.TabIndex = 11;
      this.label6.Text = "Note: Constants in quotes e.G. \"UTC\"";
      // 
      // labelInputTZ
      // 
      this.labelInputTZ.AutoSize = true;
      this.labelInputTZ.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelInputTZ.Location = new System.Drawing.Point(483, 91);
      this.labelInputTZ.Name = "labelInputTZ";
      this.labelInputTZ.Size = new System.Drawing.Size(17, 13);
      this.labelInputTZ.TabIndex = 12;
      this.labelInputTZ.Text = "\"\"";
      // 
      // labelOutPutTZ
      // 
      this.labelOutPutTZ.AutoSize = true;
      this.labelOutPutTZ.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelOutPutTZ.Location = new System.Drawing.Point(483, 106);
      this.labelOutPutTZ.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.labelOutPutTZ.Name = "labelOutPutTZ";
      this.labelOutPutTZ.Size = new System.Drawing.Size(17, 13);
      this.labelOutPutTZ.TabIndex = 13;
      this.labelOutPutTZ.Text = "\"\"";
      // 
      // comboBoxTPFormat
      // 
      tableLayoutPanel2.SetColumnSpan(this.comboBoxTPFormat, 2);
      this.comboBoxTPFormat.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "TimePartFormat", true));
      this.comboBoxTPFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxTPFormat.FormattingEnabled = true;
      this.comboBoxTPFormat.Location = new System.Drawing.Point(329, 51);
      this.comboBoxTPFormat.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.comboBoxTPFormat.Name = "comboBoxTPFormat";
      this.comboBoxTPFormat.Size = new System.Drawing.Size(134, 21);
      this.comboBoxTPFormat.TabIndex = 2;
      this.toolTip.SetToolTip(this.comboBoxTPFormat, "Format of the time column");
      this.comboBoxTPFormat.SelectedIndexChanged += new System.EventHandler(this.DateFormatChanged);
      this.comboBoxTPFormat.TextUpdate += new System.EventHandler(this.DateFormatChanged);
      // 
      // textBoxTrue
      // 
      this.textBoxTrue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "True", true));
      this.textBoxTrue.Location = new System.Drawing.Point(113, 13);
      this.textBoxTrue.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.textBoxTrue.Name = "textBoxTrue";
      this.textBoxTrue.Size = new System.Drawing.Size(45, 20);
      this.textBoxTrue.TabIndex = 0;
      // 
      // textBoxFalse
      // 
      this.textBoxFalse.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "False", true));
      this.textBoxFalse.Location = new System.Drawing.Point(113, 34);
      this.textBoxFalse.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.textBoxFalse.Name = "textBoxFalse";
      this.textBoxFalse.Size = new System.Drawing.Size(45, 20);
      this.textBoxFalse.TabIndex = 1;
      // 
      // textBoxGroupSeparator
      // 
      this.textBoxGroupSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "GroupSeparator", true));
      this.textBoxGroupSeparator.Location = new System.Drawing.Point(116, 40);
      this.textBoxGroupSeparator.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.textBoxGroupSeparator.Name = "textBoxGroupSeparator";
      this.textBoxGroupSeparator.Size = new System.Drawing.Size(28, 20);
      this.textBoxGroupSeparator.TabIndex = 1;
      this.textBoxGroupSeparator.Text = ",";
      this.textBoxGroupSeparator.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.textBoxGroupSeparator.TextChanged += new System.EventHandler(this.NumberFormatChanged);
      // 
      // textBoxDecimalSeparator
      // 
      this.textBoxDecimalSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "DecimalSeparator", true));
      this.textBoxDecimalSeparator.Location = new System.Drawing.Point(116, 61);
      this.textBoxDecimalSeparator.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.textBoxDecimalSeparator.Name = "textBoxDecimalSeparator";
      this.textBoxDecimalSeparator.Size = new System.Drawing.Size(28, 20);
      this.textBoxDecimalSeparator.TabIndex = 2;
      this.textBoxDecimalSeparator.Text = ".";
      this.textBoxDecimalSeparator.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.textBoxDecimalSeparator.TextChanged += new System.EventHandler(this.NumberFormatChanged);
      this.textBoxDecimalSeparator.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxDecimalSeparator_Validating);
      // 
      // label16
      // 
      this.label16.AutoSize = true;
      this.label16.Location = new System.Drawing.Point(27, 20);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(82, 13);
      this.label16.TabIndex = 0;
      this.label16.Text = "Number Format:";
      // 
      // comboBoxDataType
      // 
      this.tableLayoutPanelForm.SetColumnSpan(this.comboBoxDataType, 2);
      this.comboBoxDataType.DisplayMember = "Display";
      this.comboBoxDataType.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDataType.FormattingEnabled = true;
      this.comboBoxDataType.Location = new System.Drawing.Point(342, 2);
      this.comboBoxDataType.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.comboBoxDataType.Name = "comboBoxDataType";
      this.comboBoxDataType.Size = new System.Drawing.Size(209, 21);
      this.comboBoxDataType.TabIndex = 2;
      this.comboBoxDataType.ValueMember = "ID";
      this.comboBoxDataType.SelectedIndexChanged += new System.EventHandler(this.ComboBoxDataType_SelectedIndexChanged);
      // 
      // buttonCancel
      // 
      this.buttonCancel.AutoSize = true;
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(447, 557);
      this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(104, 23);
      this.buttonCancel.TabIndex = 4;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
      // 
      // labelNumber
      // 
      this.labelNumber.AutoSize = true;
      this.labelNumber.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelNumber.Location = new System.Drawing.Point(310, 43);
      this.labelNumber.Name = "labelNumber";
      this.labelNumber.Size = new System.Drawing.Size(47, 13);
      this.labelNumber.TabIndex = 6;
      this.labelNumber.Text = "Input: \"\"";
      // 
      // labelNumberOutput
      // 
      this.labelNumberOutput.AutoSize = true;
      this.labelNumberOutput.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelNumberOutput.Location = new System.Drawing.Point(302, 63);
      this.labelNumberOutput.Name = "labelNumberOutput";
      this.labelNumberOutput.Size = new System.Drawing.Size(55, 13);
      this.labelNumberOutput.TabIndex = 7;
      this.labelNumberOutput.Text = "Output: \"\"";
      // 
      // label1
      // 
      this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(3, 6);
      this.label1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(76, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Column Name:";
      // 
      // groupBoxDate
      // 
      this.groupBoxDate.AutoSize = true;
      this.tableLayoutPanelForm.SetColumnSpan(this.groupBoxDate, 5);
      this.groupBoxDate.Controls.Add(tableLayoutPanel2);
      this.groupBoxDate.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBoxDate.Location = new System.Drawing.Point(3, 51);
      this.groupBoxDate.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBoxDate.Name = "groupBoxDate";
      this.groupBoxDate.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBoxDate.Size = new System.Drawing.Size(548, 186);
      this.groupBoxDate.TabIndex = 5;
      this.groupBoxDate.TabStop = false;
      this.groupBoxDate.Text = "Date";
      // 
      // groupBoxNumber
      // 
      this.groupBoxNumber.AutoSize = true;
      this.tableLayoutPanelForm.SetColumnSpan(this.groupBoxNumber, 5);
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
      this.groupBoxNumber.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBoxNumber.Location = new System.Drawing.Point(3, 241);
      this.groupBoxNumber.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBoxNumber.Name = "groupBoxNumber";
      this.groupBoxNumber.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBoxNumber.Size = new System.Drawing.Size(548, 98);
      this.groupBoxNumber.TabIndex = 6;
      this.groupBoxNumber.TabStop = false;
      this.groupBoxNumber.Text = "Number";
      // 
      // linkLabel2
      // 
      this.linkLabel2.AutoSize = true;
      this.linkLabel2.Location = new System.Drawing.Point(176, 63);
      this.linkLabel2.Name = "linkLabel2";
      this.linkLabel2.Size = new System.Drawing.Size(101, 13);
      this.linkLabel2.TabIndex = 3;
      this.linkLabel2.TabStop = true;
      this.linkLabel2.Text = "Region && Language";
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
      this.comboBoxNumberFormat.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.comboBoxNumberFormat.Name = "comboBoxNumberFormat";
      this.comboBoxNumberFormat.Size = new System.Drawing.Size(162, 21);
      this.comboBoxNumberFormat.TabIndex = 0;
      this.comboBoxNumberFormat.TextChanged += new System.EventHandler(this.NumberFormatChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.ForeColor = System.Drawing.SystemColors.Highlight;
      this.label2.Location = new System.Drawing.Point(308, 20);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(95, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Value: \"1234.567\"";
      // 
      // groupBoxBoolean
      // 
      this.groupBoxBoolean.AutoSize = true;
      this.tableLayoutPanelForm.SetColumnSpan(this.groupBoxBoolean, 5);
      this.groupBoxBoolean.Controls.Add(labelTrue);
      this.groupBoxBoolean.Controls.Add(labelFalse);
      this.groupBoxBoolean.Controls.Add(this.textBoxTrue);
      this.groupBoxBoolean.Controls.Add(this.textBoxFalse);
      this.groupBoxBoolean.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBoxBoolean.Location = new System.Drawing.Point(3, 343);
      this.groupBoxBoolean.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBoxBoolean.Name = "groupBoxBoolean";
      this.groupBoxBoolean.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBoxBoolean.Size = new System.Drawing.Size(548, 71);
      this.groupBoxBoolean.TabIndex = 7;
      this.groupBoxBoolean.TabStop = false;
      this.groupBoxBoolean.Text = "Boolean";
      // 
      // comboBoxColumnName
      // 
      this.tableLayoutPanelForm.SetColumnSpan(this.comboBoxColumnName, 2);
      this.comboBoxColumnName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "Name", true));
      this.comboBoxColumnName.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxColumnName.FormattingEnabled = true;
      this.comboBoxColumnName.Location = new System.Drawing.Point(85, 2);
      this.comboBoxColumnName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.comboBoxColumnName.Name = "comboBoxColumnName";
      this.comboBoxColumnName.Size = new System.Drawing.Size(251, 21);
      this.comboBoxColumnName.TabIndex = 1;
      this.comboBoxColumnName.SelectedIndexChanged += new System.EventHandler(this.ComboBoxColumnName_SelectedIndexChanged);
      this.comboBoxColumnName.TextUpdate += new System.EventHandler(this.ComboBoxColumnName_TextUpdate);
      // 
      // buttonGuess
      // 
      this.buttonGuess.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonGuess.AutoSize = true;
      this.buttonGuess.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.buttonGuess.Location = new System.Drawing.Point(233, 557);
      this.buttonGuess.Margin = new System.Windows.Forms.Padding(2);
      this.buttonGuess.Name = "buttonGuess";
      this.buttonGuess.Size = new System.Drawing.Size(104, 23);
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
      this.checkBoxIgnore.Location = new System.Drawing.Point(221, 27);
      this.checkBoxIgnore.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.checkBoxIgnore.Name = "checkBoxIgnore";
      this.checkBoxIgnore.Size = new System.Drawing.Size(82, 20);
      this.checkBoxIgnore.TabIndex = 3;
      this.checkBoxIgnore.Text = "&Ignore";
      this.checkBoxIgnore.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
      this.toolTip.SetToolTip(this.checkBoxIgnore, "Ignore the content do not display/import this column");
      this.checkBoxIgnore.UseVisualStyleBackColor = true;
      this.checkBoxIgnore.Visible = false;
      // 
      // groupBoxSplit
      // 
      this.groupBoxSplit.AutoSize = true;
      this.tableLayoutPanelForm.SetColumnSpan(this.groupBoxSplit, 5);
      this.groupBoxSplit.Controls.Add(this.numericUpDownPart);
      this.groupBoxSplit.Controls.Add(this.labelSamplePart);
      this.groupBoxSplit.Controls.Add(this.checkBoxPartToEnd);
      this.groupBoxSplit.Controls.Add(labelSepBy);
      this.groupBoxSplit.Controls.Add(labelPart);
      this.groupBoxSplit.Controls.Add(this.textBoxSplit);
      this.groupBoxSplit.Controls.Add(this.labelResultPart);
      this.groupBoxSplit.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBoxSplit.Location = new System.Drawing.Point(3, 418);
      this.groupBoxSplit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBoxSplit.Name = "groupBoxSplit";
      this.groupBoxSplit.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBoxSplit.Size = new System.Drawing.Size(548, 77);
      this.groupBoxSplit.TabIndex = 8;
      this.groupBoxSplit.TabStop = false;
      this.groupBoxSplit.Text = "Text Part";
      // 
      // numericUpDownPart
      // 
      this.numericUpDownPart.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.bindingSourceValueFormat, "Part", true));
      this.numericUpDownPart.Location = new System.Drawing.Point(113, 39);
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
      // 
      // labelSamplePart
      // 
      this.labelSamplePart.AutoSize = true;
      this.labelSamplePart.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelSamplePart.Location = new System.Drawing.Point(306, 19);
      this.labelSamplePart.Name = "labelSamplePart";
      this.labelSamplePart.Size = new System.Drawing.Size(47, 13);
      this.labelSamplePart.TabIndex = 2;
      this.labelSamplePart.Text = "Input: \"\"";
      // 
      // checkBoxPartToEnd
      // 
      this.checkBoxPartToEnd.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceValueFormat, "PartToEnd", true));
      this.checkBoxPartToEnd.Location = new System.Drawing.Point(160, 38);
      this.checkBoxPartToEnd.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
      this.textBoxSplit.Location = new System.Drawing.Point(113, 15);
      this.textBoxSplit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.textBoxSplit.MaxLength = 1;
      this.textBoxSplit.Name = "textBoxSplit";
      this.textBoxSplit.Size = new System.Drawing.Size(25, 20);
      this.textBoxSplit.TabIndex = 0;
      this.textBoxSplit.Text = ":";
      this.textBoxSplit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.textBoxSplit.TextChanged += new System.EventHandler(this.SetSamplePart);
      this.textBoxSplit.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxSplit_Validating);
      // 
      // labelResultPart
      // 
      this.labelResultPart.AutoSize = true;
      this.labelResultPart.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelResultPart.Location = new System.Drawing.Point(297, 41);
      this.labelResultPart.Name = "labelResultPart";
      this.labelResultPart.Size = new System.Drawing.Size(55, 13);
      this.labelResultPart.TabIndex = 6;
      this.labelResultPart.Text = "Output: \"\"";
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
      // labelDisplayNullAs
      // 
      this.labelDisplayNullAs.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.labelDisplayNullAs.AutoSize = true;
      this.labelDisplayNullAs.Location = new System.Drawing.Point(27, 30);
      this.labelDisplayNullAs.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.labelDisplayNullAs.Name = "labelDisplayNullAs";
      this.labelDisplayNullAs.Size = new System.Drawing.Size(52, 13);
      this.labelDisplayNullAs.TabIndex = 10;
      this.labelDisplayNullAs.Text = "NULL as:";
      // 
      // textBoxDisplayNullAs
      // 
      this.textBoxDisplayNullAs.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "DisplayNullAs", true));
      this.textBoxDisplayNullAs.Location = new System.Drawing.Point(85, 27);
      this.textBoxDisplayNullAs.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.textBoxDisplayNullAs.Name = "textBoxDisplayNullAs";
      this.textBoxDisplayNullAs.Size = new System.Drawing.Size(67, 20);
      this.textBoxDisplayNullAs.TabIndex = 9;
      this.toolTip.SetToolTip(this.textBoxDisplayNullAs, "Wrting data empty field (NULL) can be an empty column or represented by this text" +
        " \r\ne.G. <NULL>");
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // buttonDisplayValues
      // 
      this.buttonDisplayValues.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonDisplayValues.AutoSize = true;
      this.buttonDisplayValues.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.buttonDisplayValues.Location = new System.Drawing.Point(84, 557);
      this.buttonDisplayValues.Margin = new System.Windows.Forms.Padding(2);
      this.buttonDisplayValues.Name = "buttonDisplayValues";
      this.buttonDisplayValues.Size = new System.Drawing.Size(132, 23);
      this.buttonDisplayValues.TabIndex = 1;
      this.buttonDisplayValues.Text = "Display &Values";
      this.toolTip.SetToolTip(this.buttonDisplayValues, "Read the content of the source and display the read values.\r\nNote: Any column tha" +
        "t has possible alignment issues will be ignored\r\n");
      this.buttonDisplayValues.UseVisualStyleBackColor = true;
      this.buttonDisplayValues.Click += new System.EventHandler(this.ButtonDisplayValues_ClickAsync);
      // 
      // tableLayoutPanelForm
      // 
      this.tableLayoutPanelForm.AutoSize = true;
      this.tableLayoutPanelForm.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.tableLayoutPanelForm.ColumnCount = 5;
      this.tableLayoutPanelForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanelForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanelForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 53.39233F));
      this.tableLayoutPanelForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 46.60767F));
      this.tableLayoutPanelForm.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanelForm.Controls.Add(this.groupBoxBinary, 0, 6);
      this.tableLayoutPanelForm.Controls.Add(this.comboBoxColumnName, 1, 0);
      this.tableLayoutPanelForm.Controls.Add(this.groupBoxSplit, 0, 5);
      this.tableLayoutPanelForm.Controls.Add(this.groupBoxBoolean, 0, 4);
      this.tableLayoutPanelForm.Controls.Add(this.groupBoxNumber, 0, 3);
      this.tableLayoutPanelForm.Controls.Add(this.groupBoxDate, 0, 2);
      this.tableLayoutPanelForm.Controls.Add(this.labelDisplayNullAs, 0, 1);
      this.tableLayoutPanelForm.Controls.Add(this.label1, 0, 0);
      this.tableLayoutPanelForm.Controls.Add(this.comboBoxDataType, 3, 0);
      this.tableLayoutPanelForm.Controls.Add(this.checkBoxIgnore, 2, 1);
      this.tableLayoutPanelForm.Controls.Add(this.textBoxDisplayNullAs, 1, 1);
      this.tableLayoutPanelForm.Controls.Add(this.buttonDisplayValues, 1, 7);
      this.tableLayoutPanelForm.Controls.Add(this.buttonGuess, 2, 7);
      this.tableLayoutPanelForm.Controls.Add(this.buttonOK, 3, 7);
      this.tableLayoutPanelForm.Controls.Add(this.buttonCancel, 4, 7);
      this.tableLayoutPanelForm.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanelForm.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanelForm.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tableLayoutPanelForm.Name = "tableLayoutPanelForm";
      this.tableLayoutPanelForm.RowCount = 8;
      this.tableLayoutPanelForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanelForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanelForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanelForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanelForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanelForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanelForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanelForm.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanelForm.Size = new System.Drawing.Size(554, 582);
      this.tableLayoutPanelForm.TabIndex = 5;
      // 
      // buttonOK
      // 
      this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOK.AutoSize = true;
      this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonOK.Location = new System.Drawing.Point(357, 557);
      this.buttonOK.Margin = new System.Windows.Forms.Padding(2);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(86, 23);
      this.buttonOK.TabIndex = 3;
      this.buttonOK.Text = "&Ok";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
      // 
      // groupBoxBinary
      // 
      this.groupBoxBinary.AutoSize = true;
      this.tableLayoutPanelForm.SetColumnSpan(this.groupBoxBinary, 5);
      this.groupBoxBinary.Controls.Add(this.textBoxPattern);
      this.groupBoxBinary.Controls.Add(label7);
      this.groupBoxBinary.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBoxBinary.Location = new System.Drawing.Point(3, 499);
      this.groupBoxBinary.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBoxBinary.Name = "groupBoxBinary";
      this.groupBoxBinary.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.groupBoxBinary.Size = new System.Drawing.Size(548, 54);
      this.groupBoxBinary.TabIndex = 11;
      this.groupBoxBinary.TabStop = false;
      this.groupBoxBinary.Text = "Binary Data";
      // 
      // label7
      // 
      label7.Anchor = System.Windows.Forms.AnchorStyles.Right;
      label7.AutoSize = true;
      label7.Location = new System.Drawing.Point(25, 20);
      label7.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      label7.Name = "label7";
      label7.Size = new System.Drawing.Size(84, 13);
      label7.TabIndex = 1;
      label7.Text = "Pattern for write:";
      // 
      // textBoxPattern
      // 
      this.textBoxPattern.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "DateFormat", true));
      this.textBoxPattern.Location = new System.Drawing.Point(113, 17);
      this.textBoxPattern.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.textBoxPattern.Name = "textBoxPattern";
      this.textBoxPattern.Size = new System.Drawing.Size(175, 20);
      this.textBoxPattern.TabIndex = 2;
      // 
      // FormColumnUI
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(554, 499);
      this.Controls.Add(this.tableLayoutPanelForm);
      this.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "Name", true));
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.MaximizeBox = false;
      this.MaximumSize = new System.Drawing.Size(570, 786);
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(570, 50);
      this.Name = "FormColumnUI";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Column Format";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ColumnFormatUI_FormClosing);
      this.Load += new System.EventHandler(this.ColumnFormatUI_Load);
      tableLayoutPanel2.ResumeLayout(false);
      tableLayoutPanel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.bindingSourceValueFormat)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.columnBindingSource)).EndInit();
      this.groupBoxDate.ResumeLayout(false);
      this.groupBoxDate.PerformLayout();
      this.groupBoxNumber.ResumeLayout(false);
      this.groupBoxNumber.PerformLayout();
      this.groupBoxBoolean.ResumeLayout(false);
      this.groupBoxBoolean.PerformLayout();
      this.groupBoxSplit.ResumeLayout(false);
      this.groupBoxSplit.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownPart)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
      this.tableLayoutPanelForm.ResumeLayout(false);
      this.tableLayoutPanelForm.PerformLayout();
      this.groupBoxBinary.ResumeLayout(false);
      this.groupBoxBinary.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    private System.Windows.Forms.BindingSource bindingSourceValueFormat;
    private System.Windows.Forms.Button buttonAddFormat;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Button buttonDisplayValues;
    private System.Windows.Forms.Button buttonGuess;
    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.CheckBox checkBoxIgnore;
    private System.Windows.Forms.CheckBox checkBoxPartToEnd;
    private System.Windows.Forms.CheckedListBox checkedListBoxDateFormats;
    private System.Windows.Forms.BindingSource columnBindingSource;
    private System.Windows.Forms.ComboBox comboBoxColumnName;
    private System.Windows.Forms.ComboBox comboBoxDataType;
    private System.Windows.Forms.ComboBox comboBoxDateFormat;
    private System.Windows.Forms.ComboBox comboBoxNumberFormat;
    private System.Windows.Forms.ComboBox comboBoxTimePart;
    private System.Windows.Forms.ComboBox comboBoxTimeZone;
    private System.Windows.Forms.ComboBox comboBoxTPFormat;
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.GroupBox groupBoxBoolean;
    private System.Windows.Forms.GroupBox groupBoxDate;
    private System.Windows.Forms.GroupBox groupBoxNumber;
    private System.Windows.Forms.GroupBox groupBoxSplit;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label labelAllowedDateFormats;
    private System.Windows.Forms.Label labelDateOutputDisplay;
    private System.Windows.Forms.Label labelDisplayNullAs;
    private System.Windows.Forms.Label labelInputTZ;
    private System.Windows.Forms.Label labelNumber;
    private System.Windows.Forms.Label labelNumberOutput;
    private System.Windows.Forms.Label labelOutPutTZ;
    private System.Windows.Forms.Label labelResultPart;
    private System.Windows.Forms.Label labelSampleDisplay;
    private System.Windows.Forms.Label labelSamplePart;
    private System.Windows.Forms.LinkLabel linkLabel2;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanelForm;
    private System.Windows.Forms.TextBox textBoxColumnName;
    private System.Windows.Forms.TextBox textBoxDateSeparator;
    private System.Windows.Forms.TextBox textBoxDecimalSeparator;
    private System.Windows.Forms.TextBox textBoxDisplayNullAs;
    private System.Windows.Forms.TextBox textBoxFalse;
    private System.Windows.Forms.TextBox textBoxGroupSeparator;
    private System.Windows.Forms.TextBox textBoxSplit;
    private System.Windows.Forms.TextBox textBoxTimeSeparator;
    private System.Windows.Forms.TextBox textBoxTrue;
    private System.Windows.Forms.ToolTip toolTip;

#endregion

    private System.Boolean m_DisposedValue;
    private System.Windows.Forms.NumericUpDown numericUpDownPart;
    private System.Windows.Forms.GroupBox groupBoxBinary;
    private System.Windows.Forms.TextBox textBoxPattern;
  }
}