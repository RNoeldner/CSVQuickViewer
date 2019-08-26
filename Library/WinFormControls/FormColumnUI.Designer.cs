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
      if (m_CancellationTokenSource != null)
        m_CancellationTokenSource.Dispose();

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
      this.labelAllowedDateFormats = new System.Windows.Forms.Label();
      this.labelDateOutput = new System.Windows.Forms.Label();
      this.comboBoxTPFormat = new System.Windows.Forms.ComboBox();
      this.columnBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.textBoxTrue = new System.Windows.Forms.TextBox();
      this.textBoxFalse = new System.Windows.Forms.TextBox();
      this.textBoxGroupSeparator = new System.Windows.Forms.TextBox();
      this.textBoxDecimalSeparator = new System.Windows.Forms.TextBox();
      this.textBoxDateSeparator = new System.Windows.Forms.TextBox();
      this.label16 = new System.Windows.Forms.Label();
      this.comboBoxDataType = new System.Windows.Forms.ComboBox();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.labelNumber = new System.Windows.Forms.Label();
      this.labelNumberOutput = new System.Windows.Forms.Label();
      this.labelSample = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.groupBoxDate = new System.Windows.Forms.GroupBox();
      this.comboBoxTimeZone = new System.Windows.Forms.ComboBox();
      this.textBoxTimeSeparator = new System.Windows.Forms.TextBox();
      this.comboBoxTimePart = new System.Windows.Forms.ComboBox();
      this.buttonAddFormat = new System.Windows.Forms.Button();
      this.linkLabel1 = new System.Windows.Forms.LinkLabel();
      this.checkedListBoxDateFormats = new System.Windows.Forms.CheckedListBox();
      this.comboBoxDateFormat = new System.Windows.Forms.ComboBox();
      this.label4 = new System.Windows.Forms.Label();
      this.groupBoxNumber = new System.Windows.Forms.GroupBox();
      this.linkLabel2 = new System.Windows.Forms.LinkLabel();
      this.comboBoxNumberFormat = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.groupBoxBoolean = new System.Windows.Forms.GroupBox();
      this.comboBoxColumnName = new System.Windows.Forms.ComboBox();
      this.buttonGuess = new System.Windows.Forms.Button();
      this.checkBoxIgnore = new System.Windows.Forms.CheckBox();
      this.groupBoxSplit = new System.Windows.Forms.GroupBox();
      this.labelSamplePart = new System.Windows.Forms.Label();
      this.checkBoxPartToEnd = new System.Windows.Forms.CheckBox();
      this.textBoxSplit = new System.Windows.Forms.TextBox();
      this.textBoxPart = new System.Windows.Forms.TextBox();
      this.labelResultPart = new System.Windows.Forms.Label();
      this.textBoxColumnName = new System.Windows.Forms.TextBox();
      this.labelDisplayNullAs = new System.Windows.Forms.Label();
      this.textBoxDisplayNullAs = new System.Windows.Forms.TextBox();
      this.bindingSourceValueFormat = new System.Windows.Forms.BindingSource(this.components);
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.buttonDisplayValues = new System.Windows.Forms.Button();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.buttonOK = new System.Windows.Forms.Button();
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
      ((System.ComponentModel.ISupportInitialize)(this.columnBindingSource)).BeginInit();
      this.groupBoxDate.SuspendLayout();
      this.groupBoxNumber.SuspendLayout();
      this.groupBoxBoolean.SuspendLayout();
      this.groupBoxSplit.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.bindingSourceValueFormat)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      this.tableLayoutPanel1.SuspendLayout();
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
      labelFalse.Location = new System.Drawing.Point(73, 38);
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
      labelPoint.Location = new System.Drawing.Point(34, 55);
      labelPoint.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelPoint.Name = "labelPoint";
      labelPoint.Size = new System.Drawing.Size(75, 13);
      labelPoint.TabIndex = 8;
      labelPoint.Text = "Decimal Point:";
      // 
      // labelDateSep
      // 
      labelDateSep.AutoSize = true;
      labelDateSep.Location = new System.Drawing.Point(25, 13);
      labelDateSep.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelDateSep.Name = "labelDateSep";
      labelDateSep.Size = new System.Drawing.Size(82, 13);
      labelDateSep.TabIndex = 0;
      labelDateSep.Text = "Date Separator:";
      // 
      // labelGroup
      // 
      labelGroup.AutoSize = true;
      labelGroup.Location = new System.Drawing.Point(2, 34);
      labelGroup.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelGroup.Name = "labelGroup";
      labelGroup.Size = new System.Drawing.Size(107, 13);
      labelGroup.TabIndex = 4;
      labelGroup.Text = "Thousand Separator:";
      // 
      // labelTimeSep
      // 
      labelTimeSep.AutoSize = true;
      labelTimeSep.Location = new System.Drawing.Point(25, 34);
      labelTimeSep.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelTimeSep.Name = "labelTimeSep";
      labelTimeSep.Size = new System.Drawing.Size(82, 13);
      labelTimeSep.TabIndex = 5;
      labelTimeSep.Text = "Time Separator:";
      // 
      // labelLessCommon
      // 
      labelLessCommon.AutoSize = true;
      labelLessCommon.Location = new System.Drawing.Point(3, 123);
      labelLessCommon.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelLessCommon.Name = "labelLessCommon";
      labelLessCommon.Size = new System.Drawing.Size(104, 13);
      labelLessCommon.TabIndex = 16;
      labelLessCommon.Text = "Uncommon Formats:";
      // 
      // labelTCFormat
      // 
      labelTCFormat.AutoSize = true;
      labelTCFormat.Location = new System.Drawing.Point(351, 13);
      labelTCFormat.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelTCFormat.Name = "labelTCFormat";
      labelTCFormat.Size = new System.Drawing.Size(106, 13);
      labelTCFormat.TabIndex = 3;
      labelTCFormat.Text = "Time Column Format:";
      // 
      // labelTimeCol
      // 
      labelTimeCol.AutoSize = true;
      labelTimeCol.Location = new System.Drawing.Point(154, 13);
      labelTimeCol.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelTimeCol.Name = "labelTimeCol";
      labelTimeCol.Size = new System.Drawing.Size(71, 13);
      labelTimeCol.TabIndex = 1;
      labelTimeCol.Text = "Time Column:";
      // 
      // labelSepBy
      // 
      labelSepBy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      labelSepBy.AutoSize = true;
      labelSepBy.Location = new System.Drawing.Point(40, 12);
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
      labelPart.Location = new System.Drawing.Point(79, 35);
      labelPart.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelPart.Name = "labelPart";
      labelPart.Size = new System.Drawing.Size(29, 13);
      labelPart.TabIndex = 3;
      labelPart.Text = "Part:";
      labelPart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // label3
      // 
      label3.AutoSize = true;
      label3.Location = new System.Drawing.Point(164, 34);
      label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      label3.Name = "label3";
      label3.Size = new System.Drawing.Size(61, 13);
      label3.TabIndex = 7;
      label3.Text = "TimeZone :";
      // 
      // labelAllowedDateFormats
      // 
      this.labelAllowedDateFormats.AutoSize = true;
      this.labelAllowedDateFormats.Location = new System.Drawing.Point(28, 55);
      this.labelAllowedDateFormats.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.labelAllowedDateFormats.Name = "labelAllowedDateFormats";
      this.labelAllowedDateFormats.Size = new System.Drawing.Size(79, 13);
      this.labelAllowedDateFormats.TabIndex = 9;
      this.labelAllowedDateFormats.Text = "Date Format(s):";
      // 
      // labelDateOutput
      // 
      this.labelDateOutput.AutoSize = true;
      this.labelDateOutput.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelDateOutput.Location = new System.Drawing.Point(301, 82);
      this.labelDateOutput.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.labelDateOutput.Name = "labelDateOutput";
      this.labelDateOutput.Size = new System.Drawing.Size(55, 13);
      this.labelDateOutput.TabIndex = 13;
      this.labelDateOutput.Text = "Output: \"\"";
      // 
      // comboBoxTPFormat
      // 
      this.comboBoxTPFormat.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "TimePartFormat", true));
      this.comboBoxTPFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxTPFormat.FormattingEnabled = true;
      this.comboBoxTPFormat.Location = new System.Drawing.Point(459, 9);
      this.comboBoxTPFormat.Name = "comboBoxTPFormat";
      this.comboBoxTPFormat.Size = new System.Drawing.Size(106, 21);
      this.comboBoxTPFormat.TabIndex = 2;
      this.comboBoxTPFormat.TextUpdate += new System.EventHandler(this.DateFormatChanged);
      // 
      // columnBindingSource
      // 
      this.columnBindingSource.AllowNew = false;
      this.columnBindingSource.DataSource = typeof(CsvTools.Column);
      // 
      // textBoxTrue
      // 
      this.textBoxTrue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "True", true));
      this.textBoxTrue.Location = new System.Drawing.Point(113, 13);
      this.textBoxTrue.Name = "textBoxTrue";
      this.textBoxTrue.Size = new System.Drawing.Size(45, 20);
      this.textBoxTrue.TabIndex = 0;
      // 
      // textBoxFalse
      // 
      this.textBoxFalse.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "False", true));
      this.textBoxFalse.Location = new System.Drawing.Point(113, 34);
      this.textBoxFalse.Name = "textBoxFalse";
      this.textBoxFalse.Size = new System.Drawing.Size(45, 20);
      this.textBoxFalse.TabIndex = 1;
      // 
      // textBoxGroupSeparator
      // 
      this.textBoxGroupSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "GroupSeparator", true));
      this.textBoxGroupSeparator.Location = new System.Drawing.Point(113, 30);
      this.textBoxGroupSeparator.Name = "textBoxGroupSeparator";
      this.textBoxGroupSeparator.Size = new System.Drawing.Size(25, 20);
      this.textBoxGroupSeparator.TabIndex = 1;
      this.textBoxGroupSeparator.Text = ",";
      this.textBoxGroupSeparator.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.textBoxGroupSeparator.TextChanged += new System.EventHandler(this.NumberFormatChanged);
      // 
      // textBoxDecimalSeparator
      // 
      this.textBoxDecimalSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "DecimalSeparator", true));
      this.textBoxDecimalSeparator.Location = new System.Drawing.Point(113, 51);
      this.textBoxDecimalSeparator.Name = "textBoxDecimalSeparator";
      this.textBoxDecimalSeparator.Size = new System.Drawing.Size(25, 20);
      this.textBoxDecimalSeparator.TabIndex = 2;
      this.textBoxDecimalSeparator.Text = ".";
      this.textBoxDecimalSeparator.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.textBoxDecimalSeparator.TextChanged += new System.EventHandler(this.NumberFormatChanged);
      this.textBoxDecimalSeparator.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxDecimalSeparator_Validating);
      // 
      // textBoxDateSeparator
      // 
      this.textBoxDateSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "DateSeparator", true));
      this.textBoxDateSeparator.Location = new System.Drawing.Point(112, 9);
      this.textBoxDateSeparator.Name = "textBoxDateSeparator";
      this.textBoxDateSeparator.Size = new System.Drawing.Size(37, 20);
      this.textBoxDateSeparator.TabIndex = 0;
      this.textBoxDateSeparator.TextChanged += new System.EventHandler(this.DateFormatChanged);
      // 
      // label16
      // 
      this.label16.AutoSize = true;
      this.label16.Location = new System.Drawing.Point(27, 12);
      this.label16.Name = "label16";
      this.label16.Size = new System.Drawing.Size(82, 13);
      this.label16.TabIndex = 0;
      this.label16.Text = "Number Format:";
      // 
      // comboBoxDataType
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.comboBoxDataType, 2);
      this.comboBoxDataType.DisplayMember = "Display";
      this.comboBoxDataType.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxDataType.FormattingEnabled = true;
      this.comboBoxDataType.Location = new System.Drawing.Point(266, 3);
      this.comboBoxDataType.Name = "comboBoxDataType";
      this.comboBoxDataType.Size = new System.Drawing.Size(305, 21);
      this.comboBoxDataType.TabIndex = 2;
      this.comboBoxDataType.ValueMember = "ID";
      this.comboBoxDataType.SelectedIndexChanged += new System.EventHandler(this.ComboBoxDataType_SelectedIndexChanged);
      // 
      // buttonCancel
      // 
      this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCancel.AutoSize = true;
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(504, 473);
      this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(68, 23);
      this.buttonCancel.TabIndex = 4;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
      // 
      // labelNumber
      // 
      this.labelNumber.AutoSize = true;
      this.labelNumber.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelNumber.Location = new System.Drawing.Point(302, 22);
      this.labelNumber.Name = "labelNumber";
      this.labelNumber.Size = new System.Drawing.Size(47, 13);
      this.labelNumber.TabIndex = 6;
      this.labelNumber.Text = "Input: \"\"";
      // 
      // labelNumberOutput
      // 
      this.labelNumberOutput.AutoSize = true;
      this.labelNumberOutput.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelNumberOutput.Location = new System.Drawing.Point(297, 36);
      this.labelNumberOutput.Name = "labelNumberOutput";
      this.labelNumberOutput.Size = new System.Drawing.Size(55, 13);
      this.labelNumberOutput.TabIndex = 7;
      this.labelNumberOutput.Text = "Output: \"\"";
      // 
      // labelSample
      // 
      this.labelSample.AutoSize = true;
      this.labelSample.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelSample.Location = new System.Drawing.Point(307, 68);
      this.labelSample.Name = "labelSample";
      this.labelSample.Size = new System.Drawing.Size(47, 13);
      this.labelSample.TabIndex = 12;
      this.labelSample.Text = "Input: \"\"";
      // 
      // label1
      // 
      this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(3, 5);
      this.label1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(76, 13);
      this.label1.TabIndex = 0;
      this.label1.Text = "Column Name:";
      // 
      // groupBoxDate
      // 
      this.groupBoxDate.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.groupBoxDate, 5);
      this.groupBoxDate.Controls.Add(this.comboBoxTimeZone);
      this.groupBoxDate.Controls.Add(label3);
      this.groupBoxDate.Controls.Add(this.textBoxTimeSeparator);
      this.groupBoxDate.Controls.Add(this.comboBoxTimePart);
      this.groupBoxDate.Controls.Add(labelTimeSep);
      this.groupBoxDate.Controls.Add(this.comboBoxTPFormat);
      this.groupBoxDate.Controls.Add(this.buttonAddFormat);
      this.groupBoxDate.Controls.Add(this.linkLabel1);
      this.groupBoxDate.Controls.Add(this.checkedListBoxDateFormats);
      this.groupBoxDate.Controls.Add(this.comboBoxDateFormat);
      this.groupBoxDate.Controls.Add(labelLessCommon);
      this.groupBoxDate.Controls.Add(this.labelAllowedDateFormats);
      this.groupBoxDate.Controls.Add(this.label4);
      this.groupBoxDate.Controls.Add(this.labelDateOutput);
      this.groupBoxDate.Controls.Add(labelTCFormat);
      this.groupBoxDate.Controls.Add(labelTimeCol);
      this.groupBoxDate.Controls.Add(this.labelSample);
      this.groupBoxDate.Controls.Add(labelDateSep);
      this.groupBoxDate.Controls.Add(this.textBoxDateSeparator);
      this.groupBoxDate.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBoxDate.Location = new System.Drawing.Point(3, 56);
      this.groupBoxDate.Name = "groupBoxDate";
      this.groupBoxDate.Size = new System.Drawing.Size(568, 160);
      this.groupBoxDate.TabIndex = 5;
      this.groupBoxDate.TabStop = false;
      this.groupBoxDate.Text = "Date";
      // 
      // comboBoxTimeZone
      // 
      this.comboBoxTimeZone.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "TimeZonePart", true));
      this.comboBoxTimeZone.FormattingEnabled = true;
      this.comboBoxTimeZone.Location = new System.Drawing.Point(229, 30);
      this.comboBoxTimeZone.Name = "comboBoxTimeZone";
      this.comboBoxTimeZone.Size = new System.Drawing.Size(117, 21);
      this.comboBoxTimeZone.TabIndex = 4;
      // 
      // textBoxTimeSeparator
      // 
      this.textBoxTimeSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "TimeSeparator", true));
      this.textBoxTimeSeparator.Location = new System.Drawing.Point(112, 30);
      this.textBoxTimeSeparator.Name = "textBoxTimeSeparator";
      this.textBoxTimeSeparator.Size = new System.Drawing.Size(37, 20);
      this.textBoxTimeSeparator.TabIndex = 3;
      // 
      // comboBoxTimePart
      // 
      this.comboBoxTimePart.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.comboBoxTimePart.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "TimePart", true));
      this.comboBoxTimePart.FormattingEnabled = true;
      this.comboBoxTimePart.Location = new System.Drawing.Point(229, 9);
      this.comboBoxTimePart.Name = "comboBoxTimePart";
      this.comboBoxTimePart.Size = new System.Drawing.Size(117, 21);
      this.comboBoxTimePart.TabIndex = 1;
      this.comboBoxTimePart.SelectedIndexChanged += new System.EventHandler(this.comboBoxTimePart_SelectedIndexChanged);
      this.comboBoxTimePart.TextUpdate += new System.EventHandler(this.DateFormatChanged);
      // 
      // buttonAddFormat
      // 
      this.buttonAddFormat.Location = new System.Drawing.Point(304, 117);
      this.buttonAddFormat.Name = "buttonAddFormat";
      this.buttonAddFormat.Size = new System.Drawing.Size(85, 24);
      this.buttonAddFormat.TabIndex = 7;
      this.buttonAddFormat.Text = "Add to List";
      this.buttonAddFormat.UseVisualStyleBackColor = true;
      this.buttonAddFormat.Click += new System.EventHandler(this.ButtonAddFormat_Click);
      // 
      // linkLabel1
      // 
      this.linkLabel1.AutoSize = true;
      this.linkLabel1.Location = new System.Drawing.Point(303, 97);
      this.linkLabel1.Name = "linkLabel1";
      this.linkLabel1.Size = new System.Drawing.Size(101, 13);
      this.linkLabel1.TabIndex = 14;
      this.linkLabel1.TabStop = true;
      this.linkLabel1.Text = "Region && Language";
      this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RegionAndLanguageLinkClicked);
      // 
      // checkedListBoxDateFormats
      // 
      this.checkedListBoxDateFormats.FormattingEnabled = true;
      this.checkedListBoxDateFormats.Location = new System.Drawing.Point(112, 55);
      this.checkedListBoxDateFormats.Name = "checkedListBoxDateFormats";
      this.checkedListBoxDateFormats.Size = new System.Drawing.Size(185, 49);
      this.checkedListBoxDateFormats.TabIndex = 5;
      this.checkedListBoxDateFormats.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CheckedListBoxDateFormats_ItemCheck);
      this.checkedListBoxDateFormats.SelectedIndexChanged += new System.EventHandler(this.DateFormatChanged);
      // 
      // comboBoxDateFormat
      // 
      this.comboBoxDateFormat.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      this.comboBoxDateFormat.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
      this.comboBoxDateFormat.FormattingEnabled = true;
      this.comboBoxDateFormat.Location = new System.Drawing.Point(112, 119);
      this.comboBoxDateFormat.Name = "comboBoxDateFormat";
      this.comboBoxDateFormat.Size = new System.Drawing.Size(185, 21);
      this.comboBoxDateFormat.TabIndex = 6;
      this.toolTip.SetToolTip(this.comboBoxDateFormat, global::CsvToolLib.Resources.TimeFomat);
      this.comboBoxDateFormat.TextChanged += new System.EventHandler(this.DateFormatChanged);
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.ForeColor = System.Drawing.SystemColors.Highlight;
      this.label4.Location = new System.Drawing.Point(307, 55);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(184, 13);
      this.label4.TabIndex = 11;
      this.label4.Text = "Value: \"7th April 2013  15:45:50 345\"";
      // 
      // groupBoxNumber
      // 
      this.groupBoxNumber.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.groupBoxNumber, 5);
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
      this.groupBoxNumber.Location = new System.Drawing.Point(3, 222);
      this.groupBoxNumber.Name = "groupBoxNumber";
      this.groupBoxNumber.Size = new System.Drawing.Size(568, 90);
      this.groupBoxNumber.TabIndex = 6;
      this.groupBoxNumber.TabStop = false;
      this.groupBoxNumber.Text = "Number";
      // 
      // linkLabel2
      // 
      this.linkLabel2.AutoSize = true;
      this.linkLabel2.Location = new System.Drawing.Point(303, 53);
      this.linkLabel2.Name = "linkLabel2";
      this.linkLabel2.Size = new System.Drawing.Size(101, 13);
      this.linkLabel2.TabIndex = 3;
      this.linkLabel2.TabStop = true;
      this.linkLabel2.Text = "Region && Language";
      this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.RegionAndLanguageLinkClicked);
      // 
      // comboBoxNumberFormat
      // 
      this.comboBoxNumberFormat.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "NumberFormat", true));
      this.comboBoxNumberFormat.FormattingEnabled = true;
      this.comboBoxNumberFormat.Items.AddRange(new object[] {
            "0.#####",
            "0.00",
            "#,##0.##"});
      this.comboBoxNumberFormat.Location = new System.Drawing.Point(113, 8);
      this.comboBoxNumberFormat.Name = "comboBoxNumberFormat";
      this.comboBoxNumberFormat.Size = new System.Drawing.Size(106, 21);
      this.comboBoxNumberFormat.TabIndex = 0;
      this.comboBoxNumberFormat.TextChanged += new System.EventHandler(this.NumberFormatChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.ForeColor = System.Drawing.SystemColors.Highlight;
      this.label2.Location = new System.Drawing.Point(302, 9);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(95, 13);
      this.label2.TabIndex = 2;
      this.label2.Text = "Value: \"1234.567\"";
      // 
      // groupBoxBoolean
      // 
      this.groupBoxBoolean.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.groupBoxBoolean, 5);
      this.groupBoxBoolean.Controls.Add(labelTrue);
      this.groupBoxBoolean.Controls.Add(labelFalse);
      this.groupBoxBoolean.Controls.Add(this.textBoxTrue);
      this.groupBoxBoolean.Controls.Add(this.textBoxFalse);
      this.groupBoxBoolean.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBoxBoolean.Location = new System.Drawing.Point(3, 318);
      this.groupBoxBoolean.Name = "groupBoxBoolean";
      this.groupBoxBoolean.Size = new System.Drawing.Size(568, 73);
      this.groupBoxBoolean.TabIndex = 7;
      this.groupBoxBoolean.TabStop = false;
      this.groupBoxBoolean.Text = "Boolean";
      // 
      // comboBoxColumnName
      // 
      this.comboBoxColumnName.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "Name", true));
      this.comboBoxColumnName.FormattingEnabled = true;
      this.comboBoxColumnName.Location = new System.Drawing.Point(251, 3);
      this.comboBoxColumnName.Name = "comboBoxColumnName";
      this.comboBoxColumnName.Size = new System.Drawing.Size(160, 21);
      this.comboBoxColumnName.TabIndex = 1;
      this.comboBoxColumnName.SelectedIndexChanged += new System.EventHandler(this.ComboBoxColumnName_SelectedIndexChanged);
      this.comboBoxColumnName.TextUpdate += new System.EventHandler(this.ComboBoxColumnName_TextUpdate);
      // 
      // buttonGuess
      // 
      this.buttonGuess.AutoSize = true;
      this.buttonGuess.Image = global::CsvToolLib.Resources.View;
      this.buttonGuess.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.buttonGuess.Location = new System.Drawing.Point(178, 473);
      this.buttonGuess.Margin = new System.Windows.Forms.Padding(2);
      this.buttonGuess.Name = "buttonGuess";
      this.buttonGuess.Size = new System.Drawing.Size(83, 23);
      this.buttonGuess.TabIndex = 2;
      this.buttonGuess.Text = "&Guess";
      this.buttonGuess.UseVisualStyleBackColor = true;
      this.buttonGuess.Click += new System.EventHandler(this.ButtonGuessClick);
      // 
      // checkBoxIgnore
      // 
      this.checkBoxIgnore.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.columnBindingSource, "Ignore", true));
      this.checkBoxIgnore.Image = global::CsvToolLib.Resources.Close;
      this.checkBoxIgnore.Location = new System.Drawing.Point(178, 29);
      this.checkBoxIgnore.Margin = new System.Windows.Forms.Padding(2);
      this.checkBoxIgnore.Name = "checkBoxIgnore";
      this.checkBoxIgnore.Size = new System.Drawing.Size(83, 20);
      this.checkBoxIgnore.TabIndex = 3;
      this.checkBoxIgnore.Text = "&Ignore";
      this.checkBoxIgnore.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
      this.checkBoxIgnore.UseVisualStyleBackColor = true;
      this.checkBoxIgnore.Visible = false;
      // 
      // groupBoxSplit
      // 
      this.groupBoxSplit.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.groupBoxSplit, 5);
      this.groupBoxSplit.Controls.Add(this.labelSamplePart);
      this.groupBoxSplit.Controls.Add(this.checkBoxPartToEnd);
      this.groupBoxSplit.Controls.Add(labelSepBy);
      this.groupBoxSplit.Controls.Add(labelPart);
      this.groupBoxSplit.Controls.Add(this.textBoxSplit);
      this.groupBoxSplit.Controls.Add(this.textBoxPart);
      this.groupBoxSplit.Controls.Add(this.labelResultPart);
      this.groupBoxSplit.Dock = System.Windows.Forms.DockStyle.Top;
      this.groupBoxSplit.Location = new System.Drawing.Point(3, 397);
      this.groupBoxSplit.Name = "groupBoxSplit";
      this.groupBoxSplit.Size = new System.Drawing.Size(568, 71);
      this.groupBoxSplit.TabIndex = 8;
      this.groupBoxSplit.TabStop = false;
      this.groupBoxSplit.Text = "Text Part";
      // 
      // labelSamplePart
      // 
      this.labelSamplePart.AutoSize = true;
      this.labelSamplePart.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelSamplePart.Location = new System.Drawing.Point(303, 12);
      this.labelSamplePart.Name = "labelSamplePart";
      this.labelSamplePart.Size = new System.Drawing.Size(47, 13);
      this.labelSamplePart.TabIndex = 2;
      this.labelSamplePart.Text = "Input: \"\"";
      // 
      // checkBoxPartToEnd
      // 
      this.checkBoxPartToEnd.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.columnBindingSource, "PartToEnd", true));
      this.checkBoxPartToEnd.Location = new System.Drawing.Point(153, 31);
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
      this.textBoxSplit.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "PartSplitter", true));
      this.textBoxSplit.Location = new System.Drawing.Point(113, 8);
      this.textBoxSplit.MaxLength = 1;
      this.textBoxSplit.Name = "textBoxSplit";
      this.textBoxSplit.Size = new System.Drawing.Size(25, 20);
      this.textBoxSplit.TabIndex = 0;
      this.textBoxSplit.Text = ":";
      this.textBoxSplit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.textBoxSplit.TextChanged += new System.EventHandler(this.SetSamplePart);
      this.textBoxSplit.Validating += new System.ComponentModel.CancelEventHandler(this.TextBoxSplit_Validating);
      // 
      // textBoxPart
      // 
      this.textBoxPart.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "Part", true));
      this.textBoxPart.Location = new System.Drawing.Point(113, 31);
      this.textBoxPart.MaxLength = 2;
      this.textBoxPart.Name = "textBoxPart";
      this.textBoxPart.Size = new System.Drawing.Size(25, 20);
      this.textBoxPart.TabIndex = 1;
      this.textBoxPart.Text = "1";
      this.textBoxPart.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.textBoxPart.TextChanged += new System.EventHandler(this.SetSamplePart);
      this.textBoxPart.Validating += new System.ComponentModel.CancelEventHandler(this.PartValidating);
      // 
      // labelResultPart
      // 
      this.labelResultPart.AutoSize = true;
      this.labelResultPart.ForeColor = System.Drawing.SystemColors.Highlight;
      this.labelResultPart.Location = new System.Drawing.Point(297, 27);
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
      this.labelDisplayNullAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.labelDisplayNullAs.AutoSize = true;
      this.labelDisplayNullAs.Location = new System.Drawing.Point(27, 32);
      this.labelDisplayNullAs.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      this.labelDisplayNullAs.Name = "labelDisplayNullAs";
      this.labelDisplayNullAs.Size = new System.Drawing.Size(52, 13);
      this.labelDisplayNullAs.TabIndex = 10;
      this.labelDisplayNullAs.Text = "NULL as:";
      // 
      // textBoxDisplayNullAs
      // 
      this.textBoxDisplayNullAs.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceValueFormat, "DisplayNullAs", true));
      this.textBoxDisplayNullAs.Location = new System.Drawing.Point(85, 30);
      this.textBoxDisplayNullAs.Name = "textBoxDisplayNullAs";
      this.textBoxDisplayNullAs.Size = new System.Drawing.Size(67, 20);
      this.textBoxDisplayNullAs.TabIndex = 9;
      // 
      // bindingSourceValueFormat
      // 
      this.bindingSourceValueFormat.AllowNew = false;
      this.bindingSourceValueFormat.DataSource = typeof(CsvTools.ValueFormat);
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
      this.buttonDisplayValues.Location = new System.Drawing.Point(84, 473);
      this.buttonDisplayValues.Margin = new System.Windows.Forms.Padding(2);
      this.buttonDisplayValues.Name = "buttonDisplayValues";
      this.buttonDisplayValues.Size = new System.Drawing.Size(90, 23);
      this.buttonDisplayValues.TabIndex = 1;
      this.buttonDisplayValues.Text = "Display &Values";
      this.buttonDisplayValues.UseVisualStyleBackColor = true;
      this.buttonDisplayValues.Click += new System.EventHandler(this.ButtonDisplayValues_Click);
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.AutoSize = true;
      this.tableLayoutPanel1.ColumnCount = 5;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
      this.tableLayoutPanel1.Controls.Add(this.groupBoxSplit, 0, 5);
      this.tableLayoutPanel1.Controls.Add(this.groupBoxBoolean, 0, 4);
      this.tableLayoutPanel1.Controls.Add(this.groupBoxNumber, 0, 3);
      this.tableLayoutPanel1.Controls.Add(this.groupBoxDate, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.labelDisplayNullAs, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.buttonCancel, 4, 6);
      this.tableLayoutPanel1.Controls.Add(this.buttonOK, 3, 6);
      this.tableLayoutPanel1.Controls.Add(this.comboBoxDataType, 3, 0);
      this.tableLayoutPanel1.Controls.Add(this.buttonGuess, 2, 6);
      this.tableLayoutPanel1.Controls.Add(this.buttonDisplayValues, 1, 6);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxIgnore, 2, 1);
      this.tableLayoutPanel1.Controls.Add(this.textBoxDisplayNullAs, 1, 1);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 7;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(574, 498);
      this.tableLayoutPanel1.TabIndex = 5;
      // 
      // buttonOK
      // 
      this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonOK.AutoSize = true;
      this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonOK.Location = new System.Drawing.Point(432, 473);
      this.buttonOK.Margin = new System.Windows.Forms.Padding(2);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(68, 23);
      this.buttonOK.TabIndex = 3;
      this.buttonOK.Text = "&Ok";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.ButtonOKClick);
      // 
      // FormColumnUI
      // 
      this.AcceptButton = this.buttonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(574, 263);
      this.Controls.Add(this.tableLayoutPanel1);
      this.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.columnBindingSource, "Name", true));
      this.MaximizeBox = false;
      this.MaximumSize = new System.Drawing.Size(582, 295);
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(582, 0);
      this.Name = "FormColumnUI";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Column Format";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ColumnFormatUI_FormClosing);
      this.Load += new System.EventHandler(this.ColumnFormatUI_Load);
      ((System.ComponentModel.ISupportInitialize)(this.columnBindingSource)).EndInit();
      this.groupBoxDate.ResumeLayout(false);
      this.groupBoxDate.PerformLayout();
      this.groupBoxNumber.ResumeLayout(false);
      this.groupBoxNumber.PerformLayout();
      this.groupBoxBoolean.ResumeLayout(false);
      this.groupBoxBoolean.PerformLayout();
      this.groupBoxSplit.ResumeLayout(false);
      this.groupBoxSplit.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.bindingSourceValueFormat)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox textBoxTrue;
    private System.Windows.Forms.TextBox textBoxFalse;
    private System.Windows.Forms.TextBox textBoxGroupSeparator;
    private System.Windows.Forms.TextBox textBoxDecimalSeparator;
    private System.Windows.Forms.TextBox textBoxDateSeparator;
    private System.Windows.Forms.Label label16;
    private System.Windows.Forms.ComboBox comboBoxDataType;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.Label labelNumber;
    private System.Windows.Forms.Label labelNumberOutput;
    private System.Windows.Forms.Label labelSample;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.GroupBox groupBoxDate;
    private System.Windows.Forms.GroupBox groupBoxNumber;
    private System.Windows.Forms.GroupBox groupBoxBoolean;
    private System.Windows.Forms.ComboBox comboBoxDateFormat;
    private System.Windows.Forms.ComboBox comboBoxNumberFormat;
    private System.Windows.Forms.ComboBox comboBoxColumnName;
    private System.Windows.Forms.CheckedListBox checkedListBoxDateFormats;
    private System.Windows.Forms.Button buttonAddFormat;
    private System.Windows.Forms.Button buttonGuess;
    private System.Windows.Forms.CheckBox checkBoxIgnore;
    private System.Windows.Forms.BindingSource columnBindingSource;
    private System.Windows.Forms.Label labelDateOutput;
    private System.Windows.Forms.ComboBox comboBoxTPFormat;
    private System.Windows.Forms.TextBox textBoxTimeSeparator;
    private System.Windows.Forms.GroupBox groupBoxSplit;
    private System.Windows.Forms.TextBox textBoxSplit;
    private System.Windows.Forms.TextBox textBoxPart;
    private System.Windows.Forms.CheckBox checkBoxPartToEnd;
    private System.Windows.Forms.Label labelSamplePart;
    private System.Windows.Forms.Label labelResultPart;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.LinkLabel linkLabel1;
    private System.Windows.Forms.LinkLabel linkLabel2;
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.ComboBox comboBoxTimePart;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.TextBox textBoxColumnName;
    private System.Windows.Forms.Label labelAllowedDateFormats;
    private System.Windows.Forms.Button buttonDisplayValues;
    private System.Windows.Forms.ComboBox comboBoxTimeZone;
    private System.Windows.Forms.Label labelDisplayNullAs;
    private System.Windows.Forms.TextBox textBoxDisplayNullAs;
    private System.Windows.Forms.BindingSource bindingSourceValueFormat;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.Button buttonOK;
  }
}