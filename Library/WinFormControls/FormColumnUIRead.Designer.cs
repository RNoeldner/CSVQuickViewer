
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
    
    protected override void Dispose(bool disposing)
    {      
      if (disposing)
      {
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
      components = new System.ComponentModel.Container();
      System.Windows.Forms.Label labelGroup;
      System.Windows.Forms.Label labelPoint;
      System.Windows.Forms.Label labelTrue;
      System.Windows.Forms.Label labelFalse;
      System.Windows.Forms.Label labelSepBy;
      System.Windows.Forms.Label labelPart;
      System.Windows.Forms.Label labelReadFolder;
      System.Windows.Forms.Label labelWriteFolder;
      System.Windows.Forms.Label labelPatternWrite;
      System.Windows.Forms.Label labelReplace;
      System.Windows.Forms.Label labelSPattern;
      System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
      System.Windows.Forms.Label labelDateSep;
      System.Windows.Forms.Label labelTimeCol;
      System.Windows.Forms.Label labelTimeSep;
      System.Windows.Forms.Label labelTimeZone;
      System.Windows.Forms.Label labelLessCommon;
      System.Windows.Forms.LinkLabel linkLabelRegion;
      System.Windows.Forms.Label labelDateOutput;
      System.Windows.Forms.Label labelValue;
      System.Windows.Forms.Label labelSample;
      System.Windows.Forms.Label labelTCFormat;
      textBoxDateSeparator = new System.Windows.Forms.TextBox();
      bindingSourceValueFormat = new System.Windows.Forms.BindingSource(components);
      buttonAddFormat = new System.Windows.Forms.Button();
      labelAllowedDateFormats = new System.Windows.Forms.Label();
      checkedListBoxDateFormats = new System.Windows.Forms.CheckedListBox();
      comboBoxTimePart = new System.Windows.Forms.ComboBox();
      columnBindingSource = new System.Windows.Forms.BindingSource(components);
      textBoxTimeSeparator = new System.Windows.Forms.TextBox();
      comboBoxTimeZone = new System.Windows.Forms.ComboBox();
      labelInput = new System.Windows.Forms.Label();
      labelSampleDisplay = new System.Windows.Forms.Label();
      labelDateOutputDisplay = new System.Windows.Forms.Label();
      labelNote = new System.Windows.Forms.Label();
      labelInputTZ = new System.Windows.Forms.Label();
      labelOutPutTZ = new System.Windows.Forms.Label();
      comboBoxTPFormat = new System.Windows.Forms.ComboBox();
      textBoxDateFormat = new System.Windows.Forms.TextBox();
      labelNoteConversion = new System.Windows.Forms.Label();
      comboBoxDataType = new System.Windows.Forms.ComboBox();
      buttonCancel = new System.Windows.Forms.Button();
      labelColName = new System.Windows.Forms.Label();
      comboBoxColumnName = new System.Windows.Forms.ComboBox();
      buttonGuess = new System.Windows.Forms.Button();
      checkBoxIgnore = new System.Windows.Forms.CheckBox();
      textBoxColumnName = new System.Windows.Forms.TextBox();
      errorProvider = new System.Windows.Forms.ErrorProvider(components);
      toolTip = new System.Windows.Forms.ToolTip(components);
      buttonDisplayValues = new System.Windows.Forms.Button();
      textBoxReadFolder = new System.Windows.Forms.TextBox();
      textBoxWriteFolder = new System.Windows.Forms.TextBox();
      textBoxPattern = new System.Windows.Forms.TextBox();
      textBoxRegexSearchPattern = new System.Windows.Forms.TextBox();
      textBoxDisplayNullAs = new System.Windows.Forms.TextBox();
      buttonOK = new System.Windows.Forms.Button();
      panelTop = new System.Windows.Forms.Panel();
      labelDisplayNullAs = new System.Windows.Forms.Label();
      panelBottom = new System.Windows.Forms.Panel();
      flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
      groupBoxDate = new System.Windows.Forms.GroupBox();
      groupBoxNumber = new System.Windows.Forms.GroupBox();
      linkLabelRegionLanguage = new System.Windows.Forms.LinkLabel();
      comboBoxNumberFormat = new System.Windows.Forms.ComboBox();
      labelNumValue = new System.Windows.Forms.Label();
      labelNumberOutput = new System.Windows.Forms.Label();
      labelNumberFormat = new System.Windows.Forms.Label();
      textBoxDecimalSeparator = new System.Windows.Forms.TextBox();
      textBoxGroupSeparator = new System.Windows.Forms.TextBox();
      labelNumber = new System.Windows.Forms.Label();
      groupBoxBoolean = new System.Windows.Forms.GroupBox();
      textBoxTrue = new System.Windows.Forms.TextBox();
      textBoxFalse = new System.Windows.Forms.TextBox();
      groupBoxSplit = new System.Windows.Forms.GroupBox();
      numericUpDownPart = new System.Windows.Forms.NumericUpDown();
      labelSamplePart = new System.Windows.Forms.Label();
      checkBoxPartToEnd = new System.Windows.Forms.CheckBox();
      textBoxSplit = new System.Windows.Forms.TextBox();
      labelResultPart = new System.Windows.Forms.Label();
      groupBoxBinary = new System.Windows.Forms.GroupBox();
      checkBoxOverwrite = new System.Windows.Forms.CheckBox();
      groupBoxRegExReplace = new System.Windows.Forms.GroupBox();
      labelRegEx = new System.Windows.Forms.Label();
      textBoxRegexReplacement = new System.Windows.Forms.TextBox();
      labelGroup = new System.Windows.Forms.Label();
      labelPoint = new System.Windows.Forms.Label();
      labelTrue = new System.Windows.Forms.Label();
      labelFalse = new System.Windows.Forms.Label();
      labelSepBy = new System.Windows.Forms.Label();
      labelPart = new System.Windows.Forms.Label();
      labelReadFolder = new System.Windows.Forms.Label();
      labelWriteFolder = new System.Windows.Forms.Label();
      labelPatternWrite = new System.Windows.Forms.Label();
      labelReplace = new System.Windows.Forms.Label();
      labelSPattern = new System.Windows.Forms.Label();
      tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      labelDateSep = new System.Windows.Forms.Label();
      labelTimeCol = new System.Windows.Forms.Label();
      labelTimeSep = new System.Windows.Forms.Label();
      labelTimeZone = new System.Windows.Forms.Label();
      labelLessCommon = new System.Windows.Forms.Label();
      linkLabelRegion = new System.Windows.Forms.LinkLabel();
      labelDateOutput = new System.Windows.Forms.Label();
      labelValue = new System.Windows.Forms.Label();
      labelSample = new System.Windows.Forms.Label();
      labelTCFormat = new System.Windows.Forms.Label();
      tableLayoutPanel2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) bindingSourceValueFormat).BeginInit();
      ((System.ComponentModel.ISupportInitialize) columnBindingSource).BeginInit();
      ((System.ComponentModel.ISupportInitialize) errorProvider).BeginInit();
      panelTop.SuspendLayout();
      panelBottom.SuspendLayout();
      flowLayoutPanel.SuspendLayout();
      groupBoxDate.SuspendLayout();
      groupBoxNumber.SuspendLayout();
      groupBoxBoolean.SuspendLayout();
      groupBoxSplit.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) numericUpDownPart).BeginInit();
      groupBoxBinary.SuspendLayout();
      groupBoxRegExReplace.SuspendLayout();
      SuspendLayout();
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
      labelSepBy.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
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
      labelPart.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
      labelPart.AutoSize = true;
      labelPart.Location = new System.Drawing.Point(77, 44);
      labelPart.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      labelPart.Name = "labelPart";
      labelPart.Size = new System.Drawing.Size(29, 13);
      labelPart.TabIndex = 3;
      labelPart.Text = "Part:";
      labelPart.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelReadFolder
      // 
      labelReadFolder.AutoSize = true;
      labelReadFolder.Location = new System.Drawing.Point(37, 21);
      labelReadFolder.Margin = new System.Windows.Forms.Padding(3);
      labelReadFolder.Name = "labelReadFolder";
      labelReadFolder.Size = new System.Drawing.Size(68, 13);
      labelReadFolder.TabIndex = 5;
      labelReadFolder.Text = "Read Folder:";
      // 
      // labelWriteFolder
      // 
      labelWriteFolder.AutoSize = true;
      labelWriteFolder.Location = new System.Drawing.Point(36, 47);
      labelWriteFolder.Margin = new System.Windows.Forms.Padding(3);
      labelWriteFolder.Name = "labelWriteFolder";
      labelWriteFolder.Size = new System.Drawing.Size(67, 13);
      labelWriteFolder.TabIndex = 3;
      labelWriteFolder.Text = "Write Folder:";
      // 
      // labelPatternWrite
      // 
      labelPatternWrite.AutoSize = true;
      labelPatternWrite.Location = new System.Drawing.Point(16, 73);
      labelPatternWrite.Margin = new System.Windows.Forms.Padding(3);
      labelPatternWrite.Name = "labelPatternWrite";
      labelPatternWrite.Size = new System.Drawing.Size(84, 13);
      labelPatternWrite.TabIndex = 1;
      labelPatternWrite.Text = "Pattern for write:";
      // 
      // labelReplace
      // 
      labelReplace.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
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
      labelSPattern.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
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
      tableLayoutPanel2.ColumnCount = 7;
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel2.Controls.Add(labelDateSep, 0, 0);
      tableLayoutPanel2.Controls.Add(textBoxDateSeparator, 1, 0);
      tableLayoutPanel2.Controls.Add(buttonAddFormat, 4, 8);
      tableLayoutPanel2.Controls.Add(labelAllowedDateFormats, 0, 3);
      tableLayoutPanel2.Controls.Add(checkedListBoxDateFormats, 1, 3);
      tableLayoutPanel2.Controls.Add(labelTimeCol, 0, 2);
      tableLayoutPanel2.Controls.Add(comboBoxTimePart, 1, 2);
      tableLayoutPanel2.Controls.Add(labelTimeSep, 2, 0);
      tableLayoutPanel2.Controls.Add(textBoxTimeSeparator, 3, 0);
      tableLayoutPanel2.Controls.Add(labelTimeZone, 0, 1);
      tableLayoutPanel2.Controls.Add(comboBoxTimeZone, 1, 1);
      tableLayoutPanel2.Controls.Add(labelLessCommon, 0, 8);
      tableLayoutPanel2.Controls.Add(linkLabelRegion, 4, 7);
      tableLayoutPanel2.Controls.Add(labelDateOutput, 4, 5);
      tableLayoutPanel2.Controls.Add(labelInput, 4, 4);
      tableLayoutPanel2.Controls.Add(labelValue, 4, 3);
      tableLayoutPanel2.Controls.Add(labelSample, 5, 3);
      tableLayoutPanel2.Controls.Add(labelSampleDisplay, 5, 4);
      tableLayoutPanel2.Controls.Add(labelDateOutputDisplay, 5, 5);
      tableLayoutPanel2.Controls.Add(labelNote, 4, 1);
      tableLayoutPanel2.Controls.Add(labelInputTZ, 6, 4);
      tableLayoutPanel2.Controls.Add(labelOutPutTZ, 6, 5);
      tableLayoutPanel2.Controls.Add(labelTCFormat, 4, 2);
      tableLayoutPanel2.Controls.Add(comboBoxTPFormat, 5, 2);
      tableLayoutPanel2.Controls.Add(textBoxDateFormat, 1, 8);
      tableLayoutPanel2.Controls.Add(labelNoteConversion, 4, 6);
      tableLayoutPanel2.Location = new System.Drawing.Point(0, 14);
      tableLayoutPanel2.Name = "tableLayoutPanel2";
      tableLayoutPanel2.RowCount = 9;
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      tableLayoutPanel2.Size = new System.Drawing.Size(616, 255);
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
      textBoxDateSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "DateSeparator", true));
      textBoxDateSeparator.Location = new System.Drawing.Point(113, 3);
      textBoxDateSeparator.Name = "textBoxDateSeparator";
      textBoxDateSeparator.Size = new System.Drawing.Size(35, 20);
      textBoxDateSeparator.TabIndex = 0;
      toolTip.SetToolTip(textBoxDateSeparator, "Separates the components of a date, that is, the year, month, and day");
      textBoxDateSeparator.TextChanged += DateFormatChanged;
      // 
      // bindingSourceValueFormat
      // 
      bindingSourceValueFormat.AllowNew = false;
      bindingSourceValueFormat.DataSource = typeof(ValueFormatMut);
      // 
      // buttonAddFormat
      // 
      tableLayoutPanel2.SetColumnSpan(buttonAddFormat, 2);
      buttonAddFormat.Location = new System.Drawing.Point(284, 231);
      buttonAddFormat.Name = "buttonAddFormat";
      buttonAddFormat.Size = new System.Drawing.Size(103, 21);
      buttonAddFormat.TabIndex = 7;
      buttonAddFormat.Text = "Add to List";
      toolTip.SetToolTip(buttonAddFormat, "Add the selected uncommon date/time format to the checked list box");
      buttonAddFormat.UseVisualStyleBackColor = true;
      buttonAddFormat.Click += ButtonAddFormat_Click;
      // 
      // labelAllowedDateFormats
      // 
      labelAllowedDateFormats.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelAllowedDateFormats.AutoSize = true;
      labelAllowedDateFormats.Location = new System.Drawing.Point(28, 83);
      labelAllowedDateFormats.Margin = new System.Windows.Forms.Padding(3);
      labelAllowedDateFormats.Name = "labelAllowedDateFormats";
      labelAllowedDateFormats.Size = new System.Drawing.Size(79, 13);
      labelAllowedDateFormats.TabIndex = 9;
      labelAllowedDateFormats.Text = "Date Format(s):";
      // 
      // checkedListBoxDateFormats
      // 
      tableLayoutPanel2.SetColumnSpan(checkedListBoxDateFormats, 3);
      checkedListBoxDateFormats.Dock = System.Windows.Forms.DockStyle.Fill;
      checkedListBoxDateFormats.FormattingEnabled = true;
      checkedListBoxDateFormats.Location = new System.Drawing.Point(113, 83);
      checkedListBoxDateFormats.Name = "checkedListBoxDateFormats";
      tableLayoutPanel2.SetRowSpan(checkedListBoxDateFormats, 5);
      checkedListBoxDateFormats.Size = new System.Drawing.Size(165, 142);
      checkedListBoxDateFormats.TabIndex = 5;
      toolTip.SetToolTip(checkedListBoxDateFormats, "Common Date/Time formats, you can choose multiple");
      checkedListBoxDateFormats.ItemCheck += CheckedListBoxDateFormats_ItemCheck;
      checkedListBoxDateFormats.SelectedIndexChanged += CheckedListBoxDateFormats_SelectedIndexChanged;
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
      comboBoxTimePart.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      tableLayoutPanel2.SetColumnSpan(comboBoxTimePart, 3);
      comboBoxTimePart.DataBindings.Add(new System.Windows.Forms.Binding("Text", columnBindingSource, "TimePart", true));
      comboBoxTimePart.Dock = System.Windows.Forms.DockStyle.Top;
      comboBoxTimePart.FormattingEnabled = true;
      comboBoxTimePart.Location = new System.Drawing.Point(113, 56);
      comboBoxTimePart.Name = "comboBoxTimePart";
      comboBoxTimePart.Size = new System.Drawing.Size(165, 21);
      comboBoxTimePart.TabIndex = 1;
      toolTip.SetToolTip(comboBoxTimePart, "Combining a time column will result in a combination of the column and the selected time column\r\ne.G “17/Aug/2019” & “17:54” will become “17/Aug/2019 17:54”\r\n");
      comboBoxTimePart.TextChanged += DateFormatChanged;
      // 
      // columnBindingSource
      // 
      columnBindingSource.AllowNew = false;
      columnBindingSource.DataSource = typeof(ColumnMut);
      // 
      // labelTimeSep
      // 
      labelTimeSep.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelTimeSep.AutoSize = true;
      labelTimeSep.Location = new System.Drawing.Point(153, 6);
      labelTimeSep.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      labelTimeSep.Name = "labelTimeSep";
      labelTimeSep.Size = new System.Drawing.Size(82, 13);
      labelTimeSep.TabIndex = 5;
      labelTimeSep.Text = "Time Separator:";
      // 
      // textBoxTimeSeparator
      // 
      textBoxTimeSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "TimeSeparator", true));
      textBoxTimeSeparator.Location = new System.Drawing.Point(240, 3);
      textBoxTimeSeparator.Name = "textBoxTimeSeparator";
      textBoxTimeSeparator.Size = new System.Drawing.Size(35, 20);
      textBoxTimeSeparator.TabIndex = 3;
      toolTip.SetToolTip(textBoxTimeSeparator, "Separates the components of time, that is, the hour, minutes, and seconds.");
      textBoxTimeSeparator.TextChanged += DateFormatChanged;
      // 
      // labelTimeZone
      // 
      labelTimeZone.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelTimeZone.AutoSize = true;
      labelTimeZone.Location = new System.Drawing.Point(43, 33);
      labelTimeZone.Margin = new System.Windows.Forms.Padding(3);
      labelTimeZone.Name = "labelTimeZone";
      labelTimeZone.Size = new System.Drawing.Size(64, 13);
      labelTimeZone.TabIndex = 7;
      labelTimeZone.Text = "Time Zone :";
      // 
      // comboBoxTimeZone
      // 
      tableLayoutPanel2.SetColumnSpan(comboBoxTimeZone, 3);
      comboBoxTimeZone.DataBindings.Add(new System.Windows.Forms.Binding("Text", columnBindingSource, "TimeZonePart", true));
      comboBoxTimeZone.Dock = System.Windows.Forms.DockStyle.Top;
      comboBoxTimeZone.FormattingEnabled = true;
      comboBoxTimeZone.Location = new System.Drawing.Point(113, 29);
      comboBoxTimeZone.Name = "comboBoxTimeZone";
      comboBoxTimeZone.Size = new System.Drawing.Size(165, 21);
      comboBoxTimeZone.TabIndex = 4;
      comboBoxTimeZone.TextChanged += DateFormatChanged;
      // 
      // labelLessCommon
      // 
      labelLessCommon.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelLessCommon.AutoSize = true;
      labelLessCommon.Location = new System.Drawing.Point(3, 235);
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
      linkLabelRegion.Location = new System.Drawing.Point(284, 186);
      linkLabelRegion.Margin = new System.Windows.Forms.Padding(3);
      linkLabelRegion.Name = "linkLabelRegion";
      linkLabelRegion.Size = new System.Drawing.Size(130, 13);
      linkLabelRegion.TabIndex = 14;
      linkLabelRegion.TabStop = true;
      linkLabelRegion.Text = "Open Region && Language";
      linkLabelRegion.LinkClicked += RegionAndLanguageLinkClicked;
      // 
      // labelDateOutput
      // 
      labelDateOutput.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      labelDateOutput.AutoSize = true;
      labelDateOutput.ForeColor = System.Drawing.SystemColors.ControlText;
      labelDateOutput.Location = new System.Drawing.Point(284, 115);
      labelDateOutput.Margin = new System.Windows.Forms.Padding(3);
      labelDateOutput.Name = "labelDateOutput";
      labelDateOutput.Size = new System.Drawing.Size(42, 13);
      labelDateOutput.TabIndex = 13;
      labelDateOutput.Text = "Output:";
      // 
      // labelInput
      // 
      labelInput.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      labelInput.AutoSize = true;
      labelInput.ForeColor = System.Drawing.SystemColors.ControlText;
      labelInput.Location = new System.Drawing.Point(292, 99);
      labelInput.Name = "labelInput";
      labelInput.Size = new System.Drawing.Size(34, 13);
      labelInput.TabIndex = 12;
      labelInput.Text = "Input:";
      // 
      // labelValue
      // 
      labelValue.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      labelValue.AutoSize = true;
      labelValue.ForeColor = System.Drawing.SystemColors.ControlText;
      labelValue.Location = new System.Drawing.Point(289, 83);
      labelValue.Margin = new System.Windows.Forms.Padding(3);
      labelValue.Name = "labelValue";
      labelValue.Size = new System.Drawing.Size(37, 13);
      labelValue.TabIndex = 11;
      labelValue.Text = "Value:";
      // 
      // labelSample
      // 
      labelSample.AutoSize = true;
      tableLayoutPanel2.SetColumnSpan(labelSample, 2);
      labelSample.ForeColor = System.Drawing.SystemColors.Highlight;
      labelSample.Location = new System.Drawing.Point(332, 83);
      labelSample.Margin = new System.Windows.Forms.Padding(3);
      labelSample.Name = "labelSample";
      labelSample.Size = new System.Drawing.Size(141, 13);
      labelSample.TabIndex = 11;
      labelSample.Text = "7th April 2013  15:45:50 345";
      // 
      // labelSampleDisplay
      // 
      labelSampleDisplay.AutoSize = true;
      labelSampleDisplay.ForeColor = System.Drawing.SystemColors.Highlight;
      labelSampleDisplay.Location = new System.Drawing.Point(332, 99);
      labelSampleDisplay.Name = "labelSampleDisplay";
      labelSampleDisplay.Size = new System.Drawing.Size(17, 13);
      labelSampleDisplay.TabIndex = 12;
      labelSampleDisplay.Text = "\"\"";
      // 
      // labelDateOutputDisplay
      // 
      labelDateOutputDisplay.AutoSize = true;
      labelDateOutputDisplay.ForeColor = System.Drawing.SystemColors.Highlight;
      labelDateOutputDisplay.Location = new System.Drawing.Point(332, 115);
      labelDateOutputDisplay.Margin = new System.Windows.Forms.Padding(3);
      labelDateOutputDisplay.Name = "labelDateOutputDisplay";
      labelDateOutputDisplay.Size = new System.Drawing.Size(17, 13);
      labelDateOutputDisplay.TabIndex = 13;
      labelDateOutputDisplay.Text = "\"\"";
      // 
      // labelNote
      // 
      labelNote.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelNote.AutoSize = true;
      tableLayoutPanel2.SetColumnSpan(labelNote, 3);
      labelNote.ForeColor = System.Drawing.SystemColors.Highlight;
      labelNote.Location = new System.Drawing.Point(284, 33);
      labelNote.Margin = new System.Windows.Forms.Padding(3);
      labelNote.Name = "labelNote";
      labelNote.Size = new System.Drawing.Size(187, 13);
      labelNote.TabIndex = 11;
      labelNote.Text = "Note: Constants in quotes e.G. \"UTC\"";
      // 
      // labelInputTZ
      // 
      labelInputTZ.AutoSize = true;
      labelInputTZ.ForeColor = System.Drawing.SystemColors.Highlight;
      labelInputTZ.Location = new System.Drawing.Point(393, 99);
      labelInputTZ.Name = "labelInputTZ";
      labelInputTZ.Size = new System.Drawing.Size(17, 13);
      labelInputTZ.TabIndex = 12;
      labelInputTZ.Text = "\"\"";
      // 
      // labelOutPutTZ
      // 
      labelOutPutTZ.AutoSize = true;
      labelOutPutTZ.ForeColor = System.Drawing.SystemColors.Highlight;
      labelOutPutTZ.Location = new System.Drawing.Point(393, 115);
      labelOutPutTZ.Margin = new System.Windows.Forms.Padding(3);
      labelOutPutTZ.Name = "labelOutPutTZ";
      labelOutPutTZ.Size = new System.Drawing.Size(17, 13);
      labelOutPutTZ.TabIndex = 13;
      labelOutPutTZ.Text = "\"\"";
      // 
      // labelTCFormat
      // 
      labelTCFormat.Anchor = System.Windows.Forms.AnchorStyles.Right;
      labelTCFormat.AutoSize = true;
      labelTCFormat.Location = new System.Drawing.Point(284, 60);
      labelTCFormat.Margin = new System.Windows.Forms.Padding(3);
      labelTCFormat.Name = "labelTCFormat";
      labelTCFormat.Size = new System.Drawing.Size(42, 13);
      labelTCFormat.TabIndex = 3;
      labelTCFormat.Text = "Format:";
      // 
      // comboBoxTPFormat
      // 
      tableLayoutPanel2.SetColumnSpan(comboBoxTPFormat, 2);
      comboBoxTPFormat.DataBindings.Add(new System.Windows.Forms.Binding("Text", columnBindingSource, "TimePartFormat", true));
      comboBoxTPFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxTPFormat.FormattingEnabled = true;
      comboBoxTPFormat.Location = new System.Drawing.Point(332, 56);
      comboBoxTPFormat.Name = "comboBoxTPFormat";
      comboBoxTPFormat.Size = new System.Drawing.Size(116, 21);
      comboBoxTPFormat.TabIndex = 2;
      toolTip.SetToolTip(comboBoxTPFormat, "Format of the time column");
      comboBoxTPFormat.SelectedIndexChanged += ComboBoxTimePart_SelectedIndexChanged;
      // 
      // textBoxDateFormat
      // 
      tableLayoutPanel2.SetColumnSpan(textBoxDateFormat, 3);
      textBoxDateFormat.Dock = System.Windows.Forms.DockStyle.Top;
      textBoxDateFormat.Location = new System.Drawing.Point(113, 231);
      textBoxDateFormat.Name = "textBoxDateFormat";
      textBoxDateFormat.Size = new System.Drawing.Size(165, 20);
      textBoxDateFormat.TabIndex = 17;
      // 
      // labelNoteConversion
      // 
      labelNoteConversion.AutoSize = true;
      labelNoteConversion.BackColor = System.Drawing.SystemColors.Control;
      tableLayoutPanel2.SetColumnSpan(labelNoteConversion, 3);
      labelNoteConversion.ForeColor = System.Drawing.SystemColors.Highlight;
      labelNoteConversion.Location = new System.Drawing.Point(284, 131);
      labelNoteConversion.Name = "labelNoteConversion";
      labelNoteConversion.Size = new System.Drawing.Size(277, 26);
      labelNoteConversion.TabIndex = 18;
      labelNoteConversion.Text = "Note: Any timezone date will be converted and displayed \r\nin the current setup timezone";
      // 
      // comboBoxDataType
      // 
      comboBoxDataType.DataBindings.Add(new System.Windows.Forms.Binding("SelectedValue", bindingSourceValueFormat, "DataType", true));
      comboBoxDataType.DisplayMember = "Display";
      comboBoxDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxDataType.FormattingEnabled = true;
      comboBoxDataType.Location = new System.Drawing.Point(325, 3);
      comboBoxDataType.Name = "comboBoxDataType";
      comboBoxDataType.Size = new System.Drawing.Size(174, 21);
      comboBoxDataType.TabIndex = 2;
      comboBoxDataType.ValueMember = "ID";
      comboBoxDataType.SelectedIndexChanged += ComboBoxDataType_SelectedIndexChanged;
      // 
      // buttonCancel
      // 
      buttonCancel.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      buttonCancel.Location = new System.Drawing.Point(541, 3);
      buttonCancel.Name = "buttonCancel";
      buttonCancel.Size = new System.Drawing.Size(83, 25);
      buttonCancel.TabIndex = 4;
      buttonCancel.Text = "&Cancel";
      buttonCancel.UseVisualStyleBackColor = true;
      // 
      // labelColName
      // 
      labelColName.AutoSize = true;
      labelColName.Location = new System.Drawing.Point(3, 6);
      labelColName.Margin = new System.Windows.Forms.Padding(3);
      labelColName.Name = "labelColName";
      labelColName.Size = new System.Drawing.Size(76, 13);
      labelColName.TabIndex = 0;
      labelColName.Text = "Column Name:";
      // 
      // comboBoxColumnName
      // 
      comboBoxColumnName.DataBindings.Add(new System.Windows.Forms.Binding("Text", columnBindingSource, "Name", true));
      comboBoxColumnName.FormattingEnabled = true;
      comboBoxColumnName.Location = new System.Drawing.Point(83, 3);
      comboBoxColumnName.Name = "comboBoxColumnName";
      comboBoxColumnName.Size = new System.Drawing.Size(238, 21);
      comboBoxColumnName.TabIndex = 1;
      comboBoxColumnName.SelectedIndexChanged += ComboBoxColumnName_SelectedIndexChanged;
      comboBoxColumnName.TextUpdate += ComboBoxColumnName_TextUpdate;
      // 
      // buttonGuess
      // 
      buttonGuess.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      buttonGuess.Location = new System.Drawing.Point(110, 3);
      buttonGuess.Name = "buttonGuess";
      buttonGuess.Size = new System.Drawing.Size(103, 25);
      buttonGuess.TabIndex = 2;
      buttonGuess.Text = "&Guess";
      toolTip.SetToolTip(buttonGuess, "Read the content of the source and try and find a matching format\r\nNote: Any column that has possible alignment issues will be ignored\r\n");
      buttonGuess.UseVisualStyleBackColor = true;
      buttonGuess.Click += ButtonGuessClick;
      // 
      // checkBoxIgnore
      // 
      checkBoxIgnore.DataBindings.Add(new System.Windows.Forms.Binding("Checked", columnBindingSource, "Ignore", true));
      checkBoxIgnore.Location = new System.Drawing.Point(83, 27);
      checkBoxIgnore.Name = "checkBoxIgnore";
      checkBoxIgnore.Size = new System.Drawing.Size(103, 20);
      checkBoxIgnore.TabIndex = 3;
      checkBoxIgnore.Text = "&Ignore";
      checkBoxIgnore.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
      toolTip.SetToolTip(checkBoxIgnore, "Ignore the content do not display/import this column");
      checkBoxIgnore.UseVisualStyleBackColor = true;
      checkBoxIgnore.Visible = false;
      // 
      // textBoxColumnName
      // 
      textBoxColumnName.DataBindings.Add(new System.Windows.Forms.Binding("Text", columnBindingSource, "Name", true));
      textBoxColumnName.Dock = System.Windows.Forms.DockStyle.Top;
      textBoxColumnName.Location = new System.Drawing.Point(85, 3);
      textBoxColumnName.Name = "textBoxColumnName";
      textBoxColumnName.ReadOnly = true;
      textBoxColumnName.Size = new System.Drawing.Size(160, 20);
      textBoxColumnName.TabIndex = 0;
      textBoxColumnName.WordWrap = false;
      // 
      // errorProvider
      // 
      errorProvider.ContainerControl = this;
      // 
      // buttonDisplayValues
      // 
      buttonDisplayValues.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
      buttonDisplayValues.Location = new System.Drawing.Point(2, 3);
      buttonDisplayValues.Name = "buttonDisplayValues";
      buttonDisplayValues.Size = new System.Drawing.Size(104, 25);
      buttonDisplayValues.TabIndex = 1;
      buttonDisplayValues.Text = "Display &Values";
      toolTip.SetToolTip(buttonDisplayValues, "Read the content of the source and display the read values.\r\nNote: Any column that has possible alignment issues will be ignored\r\n");
      buttonDisplayValues.UseVisualStyleBackColor = true;
      buttonDisplayValues.Click += ButtonDisplayValues_ClickAsync;
      // 
      // textBoxReadFolder
      // 
      textBoxReadFolder.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "ReadFolder", true));
      textBoxReadFolder.Location = new System.Drawing.Point(112, 18);
      textBoxReadFolder.Name = "textBoxReadFolder";
      textBoxReadFolder.Size = new System.Drawing.Size(238, 20);
      textBoxReadFolder.TabIndex = 6;
      toolTip.SetToolTip(textBoxReadFolder, "Folder to look for the files during the import");
      // 
      // textBoxWriteFolder
      // 
      textBoxWriteFolder.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "WriteFolder", true));
      textBoxWriteFolder.Location = new System.Drawing.Point(112, 44);
      textBoxWriteFolder.Name = "textBoxWriteFolder";
      textBoxWriteFolder.Size = new System.Drawing.Size(238, 20);
      textBoxWriteFolder.TabIndex = 4;
      toolTip.SetToolTip(textBoxWriteFolder, "As the data is written the files is sotored in this folder");
      // 
      // textBoxPattern
      // 
      textBoxPattern.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "FileOutPutPlaceholder", true));
      textBoxPattern.Location = new System.Drawing.Point(113, 70);
      textBoxPattern.Name = "textBoxPattern";
      textBoxPattern.Size = new System.Drawing.Size(237, 20);
      textBoxPattern.TabIndex = 2;
      toolTip.SetToolTip(textBoxPattern, "Pattern for the file during write. if left empty the original file name is used, you can use placeholders if data from other columns should be used to get a filename. E.G. {UserID}.docx");
      // 
      // textBoxRegexSearchPattern
      // 
      textBoxRegexSearchPattern.AutoCompleteCustomSource.AddRange(new string[] { "(?#href)<a(?:[^>]*?\\s+)?\\s*href\\s*=((\\\"|')(.*)\\2)\\s*?>[^>]*?<\\/a>", "(?#url)(https?:\\/\\/)[^[\\s)]*", "(?#email)([\\w-]+(?:\\.[\\w-]+)*)@((?:[\\w-]+\\.)*\\w[\\w-]{0,66})", "(?#file)(\\w{1}\\:{1}\\/{2})(\\w+\\/{1})+(\\w+\\.{1}\\w+){1}" });
      textBoxRegexSearchPattern.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
      textBoxRegexSearchPattern.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
      textBoxRegexSearchPattern.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "RegexSearchPattern", true));
      textBoxRegexSearchPattern.Location = new System.Drawing.Point(113, 19);
      textBoxRegexSearchPattern.Name = "textBoxRegexSearchPattern";
      textBoxRegexSearchPattern.Size = new System.Drawing.Size(239, 20);
      textBoxRegexSearchPattern.TabIndex = 7;
      toolTip.SetToolTip(textBoxRegexSearchPattern, "Regex Pattern to look for");
      textBoxRegexSearchPattern.Validating += TextBoxRegexSearchPattern_Validating;
      // 
      // textBoxDisplayNullAs
      // 
      textBoxDisplayNullAs.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "DisplayNullAs", true));
      textBoxDisplayNullAs.Location = new System.Drawing.Point(325, 27);
      textBoxDisplayNullAs.Name = "textBoxDisplayNullAs";
      textBoxDisplayNullAs.Size = new System.Drawing.Size(93, 20);
      textBoxDisplayNullAs.TabIndex = 11;
      toolTip.SetToolTip(textBoxDisplayNullAs, "Wrting data empty field (NULL) can be an empty column or represented by this text \r\ne.G. <NULL>");
      // 
      // buttonOK
      // 
      buttonOK.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
      buttonOK.Location = new System.Drawing.Point(454, 3);
      buttonOK.Name = "buttonOK";
      buttonOK.Size = new System.Drawing.Size(83, 25);
      buttonOK.TabIndex = 3;
      buttonOK.Text = "&Ok";
      buttonOK.UseVisualStyleBackColor = true;
      // 
      // panelTop
      // 
      panelTop.AutoSize = true;
      panelTop.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      panelTop.BackColor = System.Drawing.SystemColors.Control;
      panelTop.Controls.Add(labelDisplayNullAs);
      panelTop.Controls.Add(textBoxDisplayNullAs);
      panelTop.Controls.Add(labelColName);
      panelTop.Controls.Add(comboBoxColumnName);
      panelTop.Controls.Add(comboBoxDataType);
      panelTop.Controls.Add(checkBoxIgnore);
      panelTop.Dock = System.Windows.Forms.DockStyle.Top;
      panelTop.Location = new System.Drawing.Point(0, 0);
      panelTop.Margin = new System.Windows.Forms.Padding(2);
      panelTop.Name = "panelTop";
      panelTop.Size = new System.Drawing.Size(627, 50);
      panelTop.TabIndex = 6;
      // 
      // labelDisplayNullAs
      // 
      labelDisplayNullAs.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      labelDisplayNullAs.AutoSize = true;
      labelDisplayNullAs.Location = new System.Drawing.Point(241, 30);
      labelDisplayNullAs.Margin = new System.Windows.Forms.Padding(3);
      labelDisplayNullAs.Name = "labelDisplayNullAs";
      labelDisplayNullAs.Size = new System.Drawing.Size(80, 13);
      labelDisplayNullAs.TabIndex = 12;
      labelDisplayNullAs.Text = "Write NULL as:";
      // 
      // panelBottom
      // 
      panelBottom.Controls.Add(buttonOK);
      panelBottom.Controls.Add(buttonCancel);
      panelBottom.Controls.Add(buttonGuess);
      panelBottom.Controls.Add(buttonDisplayValues);
      panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
      panelBottom.Location = new System.Drawing.Point(0, 806);
      panelBottom.Margin = new System.Windows.Forms.Padding(2);
      panelBottom.Name = "panelBottom";
      panelBottom.Size = new System.Drawing.Size(627, 28);
      panelBottom.TabIndex = 7;
      // 
      // flowLayoutPanel
      // 
      flowLayoutPanel.Controls.Add(groupBoxDate);
      flowLayoutPanel.Controls.Add(groupBoxNumber);
      flowLayoutPanel.Controls.Add(groupBoxBoolean);
      flowLayoutPanel.Controls.Add(groupBoxSplit);
      flowLayoutPanel.Controls.Add(groupBoxBinary);
      flowLayoutPanel.Controls.Add(groupBoxRegExReplace);
      flowLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
      flowLayoutPanel.Location = new System.Drawing.Point(0, 48);
      flowLayoutPanel.Name = "flowLayoutPanel";
      flowLayoutPanel.Size = new System.Drawing.Size(622, 758);
      flowLayoutPanel.TabIndex = 8;
      flowLayoutPanel.WrapContents = false;
      // 
      // groupBoxDate
      // 
      groupBoxDate.Controls.Add(tableLayoutPanel2);
      groupBoxDate.Location = new System.Drawing.Point(3, 3);
      groupBoxDate.Name = "groupBoxDate";
      groupBoxDate.Padding = new System.Windows.Forms.Padding(0);
      groupBoxDate.Size = new System.Drawing.Size(619, 287);
      groupBoxDate.TabIndex = 15;
      groupBoxDate.TabStop = false;
      groupBoxDate.Text = "Date";
      // 
      // groupBoxNumber
      // 
      groupBoxNumber.Controls.Add(linkLabelRegionLanguage);
      groupBoxNumber.Controls.Add(comboBoxNumberFormat);
      groupBoxNumber.Controls.Add(labelNumValue);
      groupBoxNumber.Controls.Add(labelNumberOutput);
      groupBoxNumber.Controls.Add(labelNumberFormat);
      groupBoxNumber.Controls.Add(labelGroup);
      groupBoxNumber.Controls.Add(textBoxDecimalSeparator);
      groupBoxNumber.Controls.Add(labelPoint);
      groupBoxNumber.Controls.Add(textBoxGroupSeparator);
      groupBoxNumber.Controls.Add(labelNumber);
      groupBoxNumber.Location = new System.Drawing.Point(3, 296);
      groupBoxNumber.Name = "groupBoxNumber";
      groupBoxNumber.Padding = new System.Windows.Forms.Padding(0);
      groupBoxNumber.Size = new System.Drawing.Size(616, 94);
      groupBoxNumber.TabIndex = 7;
      groupBoxNumber.TabStop = false;
      groupBoxNumber.Text = "Number";
      groupBoxNumber.Visible = false;
      // 
      // linkLabelRegionLanguage
      // 
      linkLabelRegionLanguage.AutoSize = true;
      linkLabelRegionLanguage.Location = new System.Drawing.Point(173, 60);
      linkLabelRegionLanguage.Name = "linkLabelRegionLanguage";
      linkLabelRegionLanguage.Size = new System.Drawing.Size(130, 13);
      linkLabelRegionLanguage.TabIndex = 3;
      linkLabelRegionLanguage.TabStop = true;
      linkLabelRegionLanguage.Text = "Open Region && Language";
      linkLabelRegionLanguage.LinkClicked += RegionAndLanguageLinkClicked;
      // 
      // comboBoxNumberFormat
      // 
      comboBoxNumberFormat.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "NumberFormat", true));
      comboBoxNumberFormat.FormattingEnabled = true;
      comboBoxNumberFormat.Items.AddRange(new object[] { "0.#####", "0.00", "#,##0.##" });
      comboBoxNumberFormat.Location = new System.Drawing.Point(116, 18);
      comboBoxNumberFormat.Name = "comboBoxNumberFormat";
      comboBoxNumberFormat.Size = new System.Drawing.Size(162, 21);
      comboBoxNumberFormat.TabIndex = 0;
      comboBoxNumberFormat.TextChanged += NumberFormatChanged;
      // 
      // labelNumValue
      // 
      labelNumValue.AutoSize = true;
      labelNumValue.ForeColor = System.Drawing.SystemColors.Highlight;
      labelNumValue.Location = new System.Drawing.Point(297, 21);
      labelNumValue.Name = "labelNumValue";
      labelNumValue.Size = new System.Drawing.Size(95, 13);
      labelNumValue.TabIndex = 2;
      labelNumValue.Text = "Value: \"1234.567\"";
      // 
      // labelNumberOutput
      // 
      labelNumberOutput.AutoSize = true;
      labelNumberOutput.ForeColor = System.Drawing.SystemColors.Highlight;
      labelNumberOutput.Location = new System.Drawing.Point(297, 68);
      labelNumberOutput.Name = "labelNumberOutput";
      labelNumberOutput.Size = new System.Drawing.Size(55, 13);
      labelNumberOutput.TabIndex = 7;
      labelNumberOutput.Text = "Output: \"\"";
      // 
      // labelNumberFormat
      // 
      labelNumberFormat.AutoSize = true;
      labelNumberFormat.Location = new System.Drawing.Point(25, 21);
      labelNumberFormat.Name = "labelNumberFormat";
      labelNumberFormat.Size = new System.Drawing.Size(82, 13);
      labelNumberFormat.TabIndex = 0;
      labelNumberFormat.Text = "Number Format:";
      // 
      // textBoxDecimalSeparator
      // 
      textBoxDecimalSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "DecimalSeparator", true));
      textBoxDecimalSeparator.Location = new System.Drawing.Point(116, 65);
      textBoxDecimalSeparator.Name = "textBoxDecimalSeparator";
      textBoxDecimalSeparator.Size = new System.Drawing.Size(28, 20);
      textBoxDecimalSeparator.TabIndex = 2;
      textBoxDecimalSeparator.Text = ".";
      textBoxDecimalSeparator.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      textBoxDecimalSeparator.TextChanged += NumberFormatChanged;
      textBoxDecimalSeparator.Validating += TextBoxDecimalSeparator_Validating;
      // 
      // textBoxGroupSeparator
      // 
      textBoxGroupSeparator.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "GroupSeparator", true));
      textBoxGroupSeparator.Location = new System.Drawing.Point(116, 42);
      textBoxGroupSeparator.Name = "textBoxGroupSeparator";
      textBoxGroupSeparator.Size = new System.Drawing.Size(28, 20);
      textBoxGroupSeparator.TabIndex = 1;
      textBoxGroupSeparator.Text = ",";
      textBoxGroupSeparator.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      textBoxGroupSeparator.TextChanged += NumberFormatChanged;
      // 
      // labelNumber
      // 
      labelNumber.AutoSize = true;
      labelNumber.ForeColor = System.Drawing.SystemColors.Highlight;
      labelNumber.Location = new System.Drawing.Point(297, 45);
      labelNumber.Name = "labelNumber";
      labelNumber.Size = new System.Drawing.Size(47, 13);
      labelNumber.TabIndex = 6;
      labelNumber.Text = "Input: \"\"";
      // 
      // groupBoxBoolean
      // 
      groupBoxBoolean.Controls.Add(labelTrue);
      groupBoxBoolean.Controls.Add(labelFalse);
      groupBoxBoolean.Controls.Add(textBoxTrue);
      groupBoxBoolean.Controls.Add(textBoxFalse);
      groupBoxBoolean.Location = new System.Drawing.Point(3, 396);
      groupBoxBoolean.Name = "groupBoxBoolean";
      groupBoxBoolean.Padding = new System.Windows.Forms.Padding(0);
      groupBoxBoolean.Size = new System.Drawing.Size(616, 67);
      groupBoxBoolean.TabIndex = 8;
      groupBoxBoolean.TabStop = false;
      groupBoxBoolean.Text = "Boolean";
      // 
      // textBoxTrue
      // 
      textBoxTrue.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "True", true));
      textBoxTrue.Location = new System.Drawing.Point(113, 13);
      textBoxTrue.Name = "textBoxTrue";
      textBoxTrue.Size = new System.Drawing.Size(45, 20);
      textBoxTrue.TabIndex = 0;
      // 
      // textBoxFalse
      // 
      textBoxFalse.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "False", true));
      textBoxFalse.Location = new System.Drawing.Point(113, 38);
      textBoxFalse.Name = "textBoxFalse";
      textBoxFalse.Size = new System.Drawing.Size(45, 20);
      textBoxFalse.TabIndex = 1;
      // 
      // groupBoxSplit
      // 
      groupBoxSplit.Controls.Add(numericUpDownPart);
      groupBoxSplit.Controls.Add(labelSamplePart);
      groupBoxSplit.Controls.Add(checkBoxPartToEnd);
      groupBoxSplit.Controls.Add(labelSepBy);
      groupBoxSplit.Controls.Add(labelPart);
      groupBoxSplit.Controls.Add(textBoxSplit);
      groupBoxSplit.Controls.Add(labelResultPart);
      groupBoxSplit.Location = new System.Drawing.Point(3, 469);
      groupBoxSplit.Name = "groupBoxSplit";
      groupBoxSplit.Padding = new System.Windows.Forms.Padding(0);
      groupBoxSplit.Size = new System.Drawing.Size(616, 70);
      groupBoxSplit.TabIndex = 9;
      groupBoxSplit.TabStop = false;
      groupBoxSplit.Text = "Text Part";
      groupBoxSplit.Visible = false;
      // 
      // numericUpDownPart
      // 
      numericUpDownPart.DataBindings.Add(new System.Windows.Forms.Binding("Value", bindingSourceValueFormat, "Part", true));
      numericUpDownPart.Location = new System.Drawing.Point(113, 42);
      numericUpDownPart.Maximum = new decimal(new int[] { 9, 0, 0, 0 });
      numericUpDownPart.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
      numericUpDownPart.Name = "numericUpDownPart";
      numericUpDownPart.Size = new System.Drawing.Size(41, 20);
      numericUpDownPart.TabIndex = 7;
      numericUpDownPart.Value = new decimal(new int[] { 1, 0, 0, 0 });
      numericUpDownPart.ValueChanged += SetSamplePart;
      numericUpDownPart.Validating += PartValidating;
      // 
      // labelSamplePart
      // 
      labelSamplePart.AutoSize = true;
      labelSamplePart.ForeColor = System.Drawing.SystemColors.Highlight;
      labelSamplePart.Location = new System.Drawing.Point(297, 19);
      labelSamplePart.Name = "labelSamplePart";
      labelSamplePart.Size = new System.Drawing.Size(47, 13);
      labelSamplePart.TabIndex = 2;
      labelSamplePart.Text = "Input: \"\"";
      // 
      // checkBoxPartToEnd
      // 
      checkBoxPartToEnd.DataBindings.Add(new System.Windows.Forms.Binding("Checked", bindingSourceValueFormat, "PartToEnd", true));
      checkBoxPartToEnd.Location = new System.Drawing.Point(160, 41);
      checkBoxPartToEnd.Name = "checkBoxPartToEnd";
      checkBoxPartToEnd.Size = new System.Drawing.Size(72, 21);
      checkBoxPartToEnd.TabIndex = 2;
      checkBoxPartToEnd.Text = "To End";
      checkBoxPartToEnd.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
      checkBoxPartToEnd.UseVisualStyleBackColor = false;
      checkBoxPartToEnd.CheckedChanged += SetSamplePart;
      checkBoxPartToEnd.Validating += PartValidating;
      // 
      // textBoxSplit
      // 
      textBoxSplit.AutoCompleteCustomSource.AddRange(new string[] { ":", ";", "|" });
      textBoxSplit.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "PartSplitter", true));
      textBoxSplit.Location = new System.Drawing.Point(113, 16);
      textBoxSplit.MaxLength = 1;
      textBoxSplit.Name = "textBoxSplit";
      textBoxSplit.Size = new System.Drawing.Size(25, 20);
      textBoxSplit.TabIndex = 0;
      textBoxSplit.Text = ":";
      textBoxSplit.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      textBoxSplit.TextChanged += SetSamplePart;
      textBoxSplit.Validating += PartValidating;
      // 
      // labelResultPart
      // 
      labelResultPart.AutoSize = true;
      labelResultPart.ForeColor = System.Drawing.SystemColors.Highlight;
      labelResultPart.Location = new System.Drawing.Point(297, 44);
      labelResultPart.Name = "labelResultPart";
      labelResultPart.Size = new System.Drawing.Size(55, 13);
      labelResultPart.TabIndex = 6;
      labelResultPart.Text = "Output: \"\"";
      // 
      // groupBoxBinary
      // 
      groupBoxBinary.Controls.Add(checkBoxOverwrite);
      groupBoxBinary.Controls.Add(textBoxReadFolder);
      groupBoxBinary.Controls.Add(labelReadFolder);
      groupBoxBinary.Controls.Add(textBoxWriteFolder);
      groupBoxBinary.Controls.Add(labelWriteFolder);
      groupBoxBinary.Controls.Add(textBoxPattern);
      groupBoxBinary.Controls.Add(labelPatternWrite);
      groupBoxBinary.Location = new System.Drawing.Point(3, 545);
      groupBoxBinary.Name = "groupBoxBinary";
      groupBoxBinary.Padding = new System.Windows.Forms.Padding(0);
      groupBoxBinary.Size = new System.Drawing.Size(616, 102);
      groupBoxBinary.TabIndex = 13;
      groupBoxBinary.TabStop = false;
      groupBoxBinary.Text = "Binary Data";
      groupBoxBinary.Visible = false;
      // 
      // checkBoxOverwrite
      // 
      checkBoxOverwrite.AutoSize = true;
      checkBoxOverwrite.DataBindings.Add(new System.Windows.Forms.Binding("Checked", bindingSourceValueFormat, "Overwrite", true));
      checkBoxOverwrite.Location = new System.Drawing.Point(363, 74);
      checkBoxOverwrite.Name = "checkBoxOverwrite";
      checkBoxOverwrite.Size = new System.Drawing.Size(71, 17);
      checkBoxOverwrite.TabIndex = 7;
      checkBoxOverwrite.Text = "Overwrite";
      checkBoxOverwrite.UseVisualStyleBackColor = true;
      // 
      // groupBoxRegExReplace
      // 
      groupBoxRegExReplace.AutoSize = true;
      groupBoxRegExReplace.Controls.Add(labelRegEx);
      groupBoxRegExReplace.Controls.Add(textBoxRegexReplacement);
      groupBoxRegExReplace.Controls.Add(labelReplace);
      groupBoxRegExReplace.Controls.Add(textBoxRegexSearchPattern);
      groupBoxRegExReplace.Controls.Add(labelSPattern);
      groupBoxRegExReplace.Location = new System.Drawing.Point(3, 653);
      groupBoxRegExReplace.Name = "groupBoxRegExReplace";
      groupBoxRegExReplace.Padding = new System.Windows.Forms.Padding(0);
      groupBoxRegExReplace.Size = new System.Drawing.Size(616, 80);
      groupBoxRegExReplace.TabIndex = 14;
      groupBoxRegExReplace.TabStop = false;
      groupBoxRegExReplace.Text = "Text Replace";
      groupBoxRegExReplace.Visible = false;
      // 
      // labelRegEx
      // 
      labelRegEx.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelRegEx.AutoSize = true;
      labelRegEx.ForeColor = System.Drawing.SystemColors.Highlight;
      labelRegEx.Location = new System.Drawing.Point(364, 21);
      labelRegEx.Margin = new System.Windows.Forms.Padding(3);
      labelRegEx.Name = "labelRegEx";
      labelRegEx.Size = new System.Drawing.Size(140, 13);
      labelRegEx.TabIndex = 12;
      labelRegEx.Text = "Note: RegEx Pattern Syntax";
      // 
      // textBoxRegexReplacement
      // 
      textBoxRegexReplacement.DataBindings.Add(new System.Windows.Forms.Binding("Text", bindingSourceValueFormat, "RegexReplacement", true));
      textBoxRegexReplacement.Location = new System.Drawing.Point(113, 44);
      textBoxRegexReplacement.Name = "textBoxRegexReplacement";
      textBoxRegexReplacement.Size = new System.Drawing.Size(239, 20);
      textBoxRegexReplacement.TabIndex = 9;
      // 
      // FormColumnUiRead
      // 
      AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      ClientSize = new System.Drawing.Size(627, 834);
      Controls.Add(panelBottom);
      Controls.Add(flowLayoutPanel);
      Controls.Add(panelTop);
      DataBindings.Add(new System.Windows.Forms.Binding("Text", columnBindingSource, "Name", true));
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      MaximizeBox = false;
      MinimizeBox = false;
      MinimumSize = new System.Drawing.Size(519, 186);
      Name = "FormColumnUiRead";
      ShowIcon = false;
      ShowInTaskbar = false;
      Text = "Column Format";
      FormClosing += ColumnFormatUI_FormClosing;
      Load += ColumnFormatUI_Load;
      tableLayoutPanel2.ResumeLayout(false);
      tableLayoutPanel2.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) bindingSourceValueFormat).EndInit();
      ((System.ComponentModel.ISupportInitialize) columnBindingSource).EndInit();
      ((System.ComponentModel.ISupportInitialize) errorProvider).EndInit();
      panelTop.ResumeLayout(false);
      panelTop.PerformLayout();
      panelBottom.ResumeLayout(false);
      flowLayoutPanel.ResumeLayout(false);
      flowLayoutPanel.PerformLayout();
      groupBoxDate.ResumeLayout(false);
      groupBoxNumber.ResumeLayout(false);
      groupBoxNumber.PerformLayout();
      groupBoxBoolean.ResumeLayout(false);
      groupBoxBoolean.PerformLayout();
      groupBoxSplit.ResumeLayout(false);
      groupBoxSplit.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) numericUpDownPart).EndInit();
      groupBoxBinary.ResumeLayout(false);
      groupBoxBinary.PerformLayout();
      groupBoxRegExReplace.ResumeLayout(false);
      groupBoxRegExReplace.PerformLayout();
      ResumeLayout(false);
      PerformLayout();
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
    
    private System.Windows.Forms.Panel panelBottom;
    private System.Windows.Forms.Panel panelTop;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
    private System.Windows.Forms.GroupBox groupBoxNumber;
    private System.Windows.Forms.ComboBox comboBoxNumberFormat;
    private System.Windows.Forms.Label labelNumValue;
    private System.Windows.Forms.Label labelInput;
    private System.Windows.Forms.Label labelNumberOutput;
    private System.Windows.Forms.Label labelNumberFormat;
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
    private System.Windows.Forms.Label labelRegEx;
    private System.Windows.Forms.TextBox textBoxRegexReplacement;
    private System.Windows.Forms.TextBox textBoxRegexSearchPattern;
    private System.Windows.Forms.GroupBox groupBoxDate;
    private System.Windows.Forms.TextBox textBoxDateSeparator;
    private System.Windows.Forms.Button buttonAddFormat;
    private System.Windows.Forms.Label labelAllowedDateFormats;
    private System.Windows.Forms.CheckedListBox checkedListBoxDateFormats;
    private System.Windows.Forms.ComboBox comboBoxTimePart;
    private System.Windows.Forms.TextBox textBoxTimeSeparator;
    private System.Windows.Forms.ComboBox comboBoxTimeZone;
    private System.Windows.Forms.Label labelSampleDisplay;
    private System.Windows.Forms.Label labelDateOutputDisplay;
    private System.Windows.Forms.Label labelNote;
    private System.Windows.Forms.Label labelInputTZ;
    private System.Windows.Forms.Label labelOutPutTZ;
    private System.Windows.Forms.ComboBox comboBoxTPFormat;
    private System.Windows.Forms.LinkLabel linkLabelRegionLanguage;
    private System.Windows.Forms.Label labelDisplayNullAs;
    private System.Windows.Forms.TextBox textBoxDisplayNullAs;
    private System.Windows.Forms.TextBox textBoxDateFormat;
    private System.Windows.Forms.Label labelNoteConversion;
  }
}
