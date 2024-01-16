using System.Diagnostics.CodeAnalysis;

namespace CsvTools
{
  partial class FillGuessSettingEdit
  {
    /// <summary>
    /// The components
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    // <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "RedundantCast")]
    [SuppressMessage("ReSharper", "RedundantDelegateCreation")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    #region Vom Komponenten-Designer generierter Code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.Label labelDetectBool;
      System.Windows.Forms.Label labelDetectNumeric;
      System.Windows.Forms.Label labelDetectDate;
      System.Windows.Forms.Label labelDetectPercent;
      System.Windows.Forms.Label labelSerialDate;
      System.Windows.Forms.Label labelIgnoreID;
      System.Windows.Forms.Label labelDetectGUID;
      System.Windows.Forms.Label labelMaxRows;
      System.Windows.Forms.Label labelMinMax;
      System.Windows.Forms.Label labelFindTime;
      this.trackBarCheckedRecords = new System.Windows.Forms.TrackBar();
      this.fillGuessSettingsBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.checkBoxDectectNumbers = new System.Windows.Forms.CheckBox();
      this.checkBoxDectectPercentage = new System.Windows.Forms.CheckBox();
      this.checkBoxDetectDateTime = new System.Windows.Forms.CheckBox();
      this.checkBoxDetectBoolean = new System.Windows.Forms.CheckBox();
      this.textBoxTrue = new System.Windows.Forms.TextBox();
      this.textBoxFalse = new System.Windows.Forms.TextBox();
      this.checkBoxSerialDateTime = new System.Windows.Forms.CheckBox();
      this.checkBoxDetectGUID = new System.Windows.Forms.CheckBox();
      this.checkBoxIgnoreId = new System.Windows.Forms.CheckBox();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.checkBoxDateParts = new System.Windows.Forms.CheckBox();
      this.textBoxDateFormat = new System.Windows.Forms.TextBox();
      this.radioButtonEnabled = new System.Windows.Forms.RadioButton();
      this.radioButtonDisabled = new System.Windows.Forms.RadioButton();
      this.numericUpDownMin = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownSampleValues = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownChecked = new System.Windows.Forms.NumericUpDown();
      this.checkBoxRemoveCurrencySymbols = new System.Windows.Forms.CheckBox();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      labelDetectBool = new System.Windows.Forms.Label();
      labelDetectNumeric = new System.Windows.Forms.Label();
      labelDetectDate = new System.Windows.Forms.Label();
      labelDetectPercent = new System.Windows.Forms.Label();
      labelSerialDate = new System.Windows.Forms.Label();
      labelIgnoreID = new System.Windows.Forms.Label();
      labelDetectGUID = new System.Windows.Forms.Label();
      labelMaxRows = new System.Windows.Forms.Label();
      labelMinMax = new System.Windows.Forms.Label();
      labelFindTime = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarCheckedRecords)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fillGuessSettingsBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMin)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSampleValues)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownChecked)).BeginInit();
      this.tableLayoutPanel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      this.SuspendLayout();
      // 
      // labelDetectBool
      // 
      labelDetectBool.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelDetectBool.AutoSize = true;
      labelDetectBool.Location = new System.Drawing.Point(208, 209);
      labelDetectBool.Margin = new System.Windows.Forms.Padding(3);
      labelDetectBool.Name = "labelDetectBool";
      labelDetectBool.Size = new System.Drawing.Size(444, 26);
      labelDetectBool.TabIndex = 22;
      labelDetectBool.Text = "Detect Boolean values. e.g. Yes/No, True/False, 1/0.  You may add your own values" +
    " to the text boxes";
      // 
      // labelDetectNumeric
      // 
      labelDetectNumeric.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelDetectNumeric.AutoSize = true;
      labelDetectNumeric.Location = new System.Drawing.Point(208, 78);
      labelDetectNumeric.Margin = new System.Windows.Forms.Padding(3);
      labelDetectNumeric.Name = "labelDetectNumeric";
      labelDetectNumeric.Size = new System.Drawing.Size(210, 13);
      labelDetectNumeric.TabIndex = 7;
      labelDetectNumeric.Text = "Detect Numeric (Integer or Decimal) values";
      // 
      // labelDetectDate
      // 
      labelDetectDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelDetectDate.AutoSize = true;
      labelDetectDate.Location = new System.Drawing.Point(208, 99);
      labelDetectDate.Margin = new System.Windows.Forms.Padding(3);
      labelDetectDate.Name = "labelDetectDate";
      labelDetectDate.Size = new System.Drawing.Size(441, 26);
      labelDetectDate.TabIndex = 10;
      labelDetectDate.Text = "Detect Date/Time values in various formats; If a format is entered the inspection" +
    " of this date format will not require the minimum number of records it only has " +
    "to be valid for all records.";
      // 
      // labelDetectPercent
      // 
      labelDetectPercent.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelDetectPercent.AutoSize = true;
      labelDetectPercent.Location = new System.Drawing.Point(208, 188);
      labelDetectPercent.Margin = new System.Windows.Forms.Padding(3);
      labelDetectPercent.Name = "labelDetectPercent";
      labelDetectPercent.Size = new System.Drawing.Size(352, 13);
      labelDetectPercent.TabIndex = 18;
      labelDetectPercent.Text = "Detect Percentage and Permille, stored as decimal value (divided by 100)";
      // 
      // labelSerialDate
      // 
      labelSerialDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelSerialDate.AutoSize = true;
      labelSerialDate.Location = new System.Drawing.Point(208, 133);
      labelSerialDate.Margin = new System.Windows.Forms.Padding(3);
      labelSerialDate.Name = "labelSerialDate";
      labelSerialDate.Size = new System.Drawing.Size(318, 13);
      labelSerialDate.TabIndex = 14;
      labelSerialDate.Text = "Allow serial Date Time formats, used in Excel and OLE Automation";
      // 
      // labelIgnoreID
      // 
      labelIgnoreID.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelIgnoreID.AutoSize = true;
      labelIgnoreID.Location = new System.Drawing.Point(208, 266);
      labelIgnoreID.Margin = new System.Windows.Forms.Padding(3);
      labelIgnoreID.Name = "labelIgnoreID";
      labelIgnoreID.Size = new System.Drawing.Size(397, 13);
      labelIgnoreID.TabIndex = 26;
      labelIgnoreID.Text = "Ignore columns that end with “Id”, “Ref” or “Text” and always process these as te" +
    "xt";
      // 
      // labelDetectGUID
      // 
      labelDetectGUID.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelDetectGUID.AutoSize = true;
      labelDetectGUID.Location = new System.Drawing.Point(208, 243);
      labelDetectGUID.Margin = new System.Windows.Forms.Padding(3);
      labelDetectGUID.Name = "labelDetectGUID";
      labelDetectGUID.Size = new System.Drawing.Size(305, 13);
      labelDetectGUID.TabIndex = 24;
      labelDetectGUID.Text = "Detect Globally Unique IDentifier / Universally Unique IDentifier";
      // 
      // labelMaxRows
      // 
      labelMaxRows.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelMaxRows.AutoSize = true;
      labelMaxRows.Location = new System.Drawing.Point(208, 54);
      labelMaxRows.Margin = new System.Windows.Forms.Padding(3);
      labelMaxRows.Name = "labelMaxRows";
      labelMaxRows.Size = new System.Drawing.Size(221, 13);
      labelMaxRows.TabIndex = 5;
      labelMaxRows.Text = "Maximum rows to check to get sample values";
      // 
      // labelMinMax
      // 
      labelMinMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelMinMax.AutoSize = true;
      labelMinMax.Location = new System.Drawing.Point(208, 29);
      labelMinMax.Margin = new System.Windows.Forms.Padding(3);
      labelMinMax.Name = "labelMinMax";
      labelMinMax.Size = new System.Drawing.Size(444, 13);
      labelMinMax.TabIndex = 2;
      labelMinMax.Text = "Minimum and maximum number of samples to read for a before trying to determine th" +
    "e format. ";
      // 
      // labelFindTime
      // 
      labelFindTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelFindTime.AutoSize = true;
      labelFindTime.Location = new System.Drawing.Point(208, 154);
      labelFindTime.Margin = new System.Windows.Forms.Padding(3);
      labelFindTime.Name = "labelFindTime";
      labelFindTime.Size = new System.Drawing.Size(444, 26);
      labelFindTime.TabIndex = 16;
      labelFindTime.Text = "Find associated Time and Time Zone for date columns and combine the information t" +
    "o a date with time\r\n";
      // 
      // trackBarCheckedRecords
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.trackBarCheckedRecords, 2);
      this.trackBarCheckedRecords.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.fillGuessSettingsBindingSource, "CheckedRecords", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.trackBarCheckedRecords.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.trackBarCheckedRecords.LargeChange = 2000;
      this.trackBarCheckedRecords.Location = new System.Drawing.Point(2, 51);
      this.trackBarCheckedRecords.Margin = new System.Windows.Forms.Padding(2);
      this.trackBarCheckedRecords.Maximum = 50000;
      this.trackBarCheckedRecords.Name = "trackBarCheckedRecords";
      this.trackBarCheckedRecords.Size = new System.Drawing.Size(129, 20);
      this.trackBarCheckedRecords.SmallChange = 100;
      this.trackBarCheckedRecords.TabIndex = 3;
      this.trackBarCheckedRecords.TickFrequency = 2000;
      this.trackBarCheckedRecords.Value = 250;
      // 
      // fillGuessSettingsBindingSource
      // 
      this.fillGuessSettingsBindingSource.AllowNew = false;
      this.fillGuessSettingsBindingSource.DataSource = typeof(CsvTools.FillGuessSettings);
      // 
      // checkBoxDectectNumbers
      // 
      this.checkBoxDectectNumbers.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxDectectNumbers.AutoSize = true;
      this.checkBoxDectectNumbers.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxDectectNumbers.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DetectNumbers", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxDectectNumbers.Location = new System.Drawing.Point(3, 76);
      this.checkBoxDectectNumbers.Name = "checkBoxDectectNumbers";
      this.checkBoxDectectNumbers.Size = new System.Drawing.Size(65, 17);
      this.checkBoxDectectNumbers.TabIndex = 6;
      this.checkBoxDectectNumbers.Text = "Numeric";
      this.toolTip.SetToolTip(this.checkBoxDectectNumbers, "Numbers with leading 0 will not be regarded as numbers to prevent information los" +
        "s");
      this.checkBoxDectectNumbers.UseVisualStyleBackColor = true;
      // 
      // checkBoxDectectPercentage
      // 
      this.checkBoxDectectPercentage.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxDectectPercentage.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBoxDectectPercentage, 3);
      this.checkBoxDectectPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxDectectPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DetectPercentage", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxDectectPercentage.Location = new System.Drawing.Point(3, 186);
      this.checkBoxDectectPercentage.Name = "checkBoxDectectPercentage";
      this.checkBoxDectectPercentage.Size = new System.Drawing.Size(81, 17);
      this.checkBoxDectectPercentage.TabIndex = 17;
      this.checkBoxDectectPercentage.Text = "Percentage";
      this.toolTip.SetToolTip(this.checkBoxDectectPercentage, "Detect Percentage % and Permille ‰");
      this.checkBoxDectectPercentage.UseVisualStyleBackColor = true;
      // 
      // checkBoxDetectDateTime
      // 
      this.checkBoxDetectDateTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxDetectDateTime.AutoSize = true;
      this.checkBoxDetectDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxDetectDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DetectDateTime", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxDetectDateTime.Location = new System.Drawing.Point(3, 103);
      this.checkBoxDetectDateTime.Name = "checkBoxDetectDateTime";
      this.checkBoxDetectDateTime.Size = new System.Drawing.Size(83, 17);
      this.checkBoxDetectDateTime.TabIndex = 8;
      this.checkBoxDetectDateTime.Text = "Date / Time";
      this.toolTip.SetToolTip(this.checkBoxDetectDateTime, "Detect dates and times on a variety of formats, to make sure the order of day and" +
        " month is determined correctly enough sample values should be present");
      this.checkBoxDetectDateTime.UseVisualStyleBackColor = true;
      // 
      // checkBoxDetectBoolean
      // 
      this.checkBoxDetectBoolean.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxDetectBoolean.AutoSize = true;
      this.checkBoxDetectBoolean.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DetectBoolean", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxDetectBoolean.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxDetectBoolean.Location = new System.Drawing.Point(3, 213);
      this.checkBoxDetectBoolean.Name = "checkBoxDetectBoolean";
      this.checkBoxDetectBoolean.Size = new System.Drawing.Size(65, 17);
      this.checkBoxDetectBoolean.TabIndex = 19;
      this.checkBoxDetectBoolean.Text = "Boolean";
      this.toolTip.SetToolTip(this.checkBoxDetectBoolean, "Detect Boolean values, the minimum number of samples does not need to be checked " +
        "to allow detection");
      this.checkBoxDetectBoolean.UseVisualStyleBackColor = true;
      // 
      // textBoxTrue
      // 
      this.textBoxTrue.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.textBoxTrue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "TrueValue", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.textBoxTrue.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.textBoxTrue.Location = new System.Drawing.Point(92, 212);
      this.textBoxTrue.Name = "textBoxTrue";
      this.textBoxTrue.Size = new System.Drawing.Size(48, 20);
      this.textBoxTrue.TabIndex = 20;
      this.toolTip.SetToolTip(this.textBoxTrue, "Value(s) that should be regarded as TRUE, separated by ;");
      // 
      // textBoxFalse
      // 
      this.textBoxFalse.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.textBoxFalse.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "FalseValue", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.textBoxFalse.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.textBoxFalse.Location = new System.Drawing.Point(146, 212);
      this.textBoxFalse.Name = "textBoxFalse";
      this.textBoxFalse.Size = new System.Drawing.Size(48, 20);
      this.textBoxFalse.TabIndex = 21;
      this.toolTip.SetToolTip(this.textBoxFalse, "Value(s) that should be regarded as FALSE, separated by ;");
      // 
      // checkBoxSerialDateTime
      // 
      this.checkBoxSerialDateTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxSerialDateTime.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBoxSerialDateTime, 3);
      this.checkBoxSerialDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "SerialDateTime", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxSerialDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxSerialDateTime.Location = new System.Drawing.Point(3, 131);
      this.checkBoxSerialDateTime.Name = "checkBoxSerialDateTime";
      this.checkBoxSerialDateTime.Size = new System.Drawing.Size(129, 17);
      this.checkBoxSerialDateTime.TabIndex = 13;
      this.checkBoxSerialDateTime.Text = "Allow Serial DateTime";
      this.toolTip.SetToolTip(this.checkBoxSerialDateTime, "Excel stores dates as number of days after the December 31, 1899: \r\nJanuary 1, 19" +
        "00  is 1 or \r\nSaturday, 15. December 2018 13:40:10 is 43449.56956\r\n");
      this.checkBoxSerialDateTime.UseVisualStyleBackColor = true;
      // 
      // checkBoxDetectGUID
      // 
      this.checkBoxDetectGUID.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxDetectGUID.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBoxDetectGUID, 2);
      this.checkBoxDetectGUID.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DetectGUID", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxDetectGUID.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxDetectGUID.Location = new System.Drawing.Point(3, 241);
      this.checkBoxDetectGUID.Name = "checkBoxDetectGUID";
      this.checkBoxDetectGUID.Size = new System.Drawing.Size(91, 17);
      this.checkBoxDetectGUID.TabIndex = 23;
      this.checkBoxDetectGUID.Text = "GUID / UUID";
      this.toolTip.SetToolTip(this.checkBoxDetectGUID, "Detect Unique Identifiers like 0fa3e61e-b976-48be-b112-c09bea582ce5 or {37acd583-" +
        "d099-47f0-a9af-5422fc1cb3ff}");
      this.checkBoxDetectGUID.UseVisualStyleBackColor = true;
      // 
      // checkBoxIgnoreId
      // 
      this.checkBoxIgnoreId.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxIgnoreId.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBoxIgnoreId, 3);
      this.checkBoxIgnoreId.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxIgnoreId.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "IgnoreIdColumns", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxIgnoreId.Location = new System.Drawing.Point(3, 264);
      this.checkBoxIgnoreId.Name = "checkBoxIgnoreId";
      this.checkBoxIgnoreId.Size = new System.Drawing.Size(112, 17);
      this.checkBoxIgnoreId.TabIndex = 25;
      this.checkBoxIgnoreId.Text = "Ignore ID columns";
      this.toolTip.SetToolTip(this.checkBoxIgnoreId, "Ignore if the name of the column indicates this to be an ID");
      this.checkBoxIgnoreId.UseVisualStyleBackColor = true;
      // 
      // checkBoxDateParts
      // 
      this.checkBoxDateParts.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxDateParts.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBoxDateParts, 3);
      this.checkBoxDateParts.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DateParts", true));
      this.checkBoxDateParts.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxDateParts.Location = new System.Drawing.Point(3, 158);
      this.checkBoxDateParts.Name = "checkBoxDateParts";
      this.checkBoxDateParts.Size = new System.Drawing.Size(157, 17);
      this.checkBoxDateParts.TabIndex = 15;
      this.checkBoxDateParts.Text = "Include Time and Timezone";
      this.toolTip.SetToolTip(this.checkBoxDateParts, "Find columns that possible correspond to a date column to combine date and time");
      this.checkBoxDateParts.UseVisualStyleBackColor = true;
      // 
      // textBoxDateFormat
      // 
      this.textBoxDateFormat.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.tableLayoutPanel1.SetColumnSpan(this.textBoxDateFormat, 2);
      this.textBoxDateFormat.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "DateFormat", true));
      this.textBoxDateFormat.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.textBoxDateFormat.Location = new System.Drawing.Point(92, 102);
      this.textBoxDateFormat.Name = "textBoxDateFormat";
      this.textBoxDateFormat.Size = new System.Drawing.Size(100, 20);
      this.textBoxDateFormat.TabIndex = 31;
      this.toolTip.SetToolTip(this.textBoxDateFormat, "Format that does not require the minimum number of samples to be accepted, if lef" +
        "t empty the systems short date format is used.");
      // 
      // radioButtonEnabled
      // 
      this.radioButtonEnabled.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.radioButtonEnabled, 2);
      this.radioButtonEnabled.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.radioButtonEnabled.Location = new System.Drawing.Point(3, 3);
      this.radioButtonEnabled.Name = "radioButtonEnabled";
      this.radioButtonEnabled.Size = new System.Drawing.Size(110, 17);
      this.radioButtonEnabled.TabIndex = 27;
      this.radioButtonEnabled.TabStop = true;
      this.radioButtonEnabled.Text = "Enable Inspection";
      this.toolTip.SetToolTip(this.radioButtonEnabled, "As a file is opened each column will be checked if the text represents values");
      this.radioButtonEnabled.UseVisualStyleBackColor = true;
      // 
      // radioButtonDisabled
      // 
      this.radioButtonDisabled.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.radioButtonDisabled, 2);
      this.radioButtonDisabled.Location = new System.Drawing.Point(146, 3);
      this.radioButtonDisabled.Name = "radioButtonDisabled";
      this.radioButtonDisabled.Size = new System.Drawing.Size(112, 17);
      this.radioButtonDisabled.TabIndex = 27;
      this.radioButtonDisabled.TabStop = true;
      this.radioButtonDisabled.Text = "Disable Inspection";
      this.toolTip.SetToolTip(this.radioButtonDisabled, "If automated detection is disabled each text will be read as such, this is faster" +
        " as the data does not need to be read ");
      this.radioButtonDisabled.UseVisualStyleBackColor = true;
      // 
      // numericUpDownMin
      // 
      this.numericUpDownMin.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.fillGuessSettingsBindingSource, "MinSamples", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.numericUpDownMin.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.numericUpDownMin.Location = new System.Drawing.Point(92, 26);
      this.numericUpDownMin.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
      this.numericUpDownMin.Name = "numericUpDownMin";
      this.numericUpDownMin.Size = new System.Drawing.Size(46, 20);
      this.numericUpDownMin.TabIndex = 28;
      this.toolTip.SetToolTip(this.numericUpDownMin, "Text can be ambiguous and match different formats, a higher value makes sure that" +
        " the found format is correct. e.g. 10/05/2022 could be the 10th May or the 5th O" +
        "ct. ");
      // 
      // numericUpDownSampleValues
      // 
      this.numericUpDownSampleValues.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.fillGuessSettingsBindingSource, "SampleValues", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.numericUpDownSampleValues.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.numericUpDownSampleValues.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
      this.numericUpDownSampleValues.Location = new System.Drawing.Point(146, 26);
      this.numericUpDownSampleValues.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
      this.numericUpDownSampleValues.Name = "numericUpDownSampleValues";
      this.numericUpDownSampleValues.Size = new System.Drawing.Size(46, 20);
      this.numericUpDownSampleValues.TabIndex = 29;
      this.toolTip.SetToolTip(this.numericUpDownSampleValues, "Stop reading new unique values if this number is reached.");
      // 
      // numericUpDownChecked
      // 
      this.numericUpDownChecked.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.fillGuessSettingsBindingSource, "CheckedRecords", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.numericUpDownChecked.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.numericUpDownChecked.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
      this.numericUpDownChecked.Location = new System.Drawing.Point(146, 52);
      this.numericUpDownChecked.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
      this.numericUpDownChecked.Name = "numericUpDownChecked";
      this.numericUpDownChecked.Size = new System.Drawing.Size(46, 20);
      this.numericUpDownChecked.TabIndex = 30;
      this.toolTip.SetToolTip(this.numericUpDownChecked, "Limit the records to look for text in the columns");
      // 
      // checkBoxRemoveCurrencySymbols
      // 
      this.checkBoxRemoveCurrencySymbols.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxRemoveCurrencySymbols.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBoxRemoveCurrencySymbols, 2);
      this.checkBoxRemoveCurrencySymbols.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "RemoveCurrencySymbols", true));
      this.checkBoxRemoveCurrencySymbols.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "DetectNumbers", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxRemoveCurrencySymbols.Location = new System.Drawing.Point(92, 76);
      this.checkBoxRemoveCurrencySymbols.Name = "checkBoxRemoveCurrencySymbols";
      this.checkBoxRemoveCurrencySymbols.Size = new System.Drawing.Size(110, 17);
      this.checkBoxRemoveCurrencySymbols.TabIndex = 6;
      this.checkBoxRemoveCurrencySymbols.Text = "Currency Symbols";
      this.toolTip.SetToolTip(this.checkBoxRemoveCurrencySymbols, "Remove common currency symbols to parse the text");
      this.checkBoxRemoveCurrencySymbols.UseVisualStyleBackColor = true;
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.AutoSize = true;
      this.tableLayoutPanel1.ColumnCount = 4;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Controls.Add(labelDetectDate, 3, 4);
      this.tableLayoutPanel1.Controls.Add(labelDetectNumeric, 3, 3);
      this.tableLayoutPanel1.Controls.Add(labelMaxRows, 3, 2);
      this.tableLayoutPanel1.Controls.Add(labelMinMax, 3, 1);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDetectDateTime, 0, 4);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDectectNumbers, 0, 3);
      this.tableLayoutPanel1.Controls.Add(this.trackBarCheckedRecords, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.radioButtonEnabled, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.radioButtonDisabled, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.numericUpDownMin, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.numericUpDownSampleValues, 2, 1);
      this.tableLayoutPanel1.Controls.Add(this.numericUpDownChecked, 2, 2);
      this.tableLayoutPanel1.Controls.Add(this.textBoxDateFormat, 1, 4);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxSerialDateTime, 0, 6);
      this.tableLayoutPanel1.Controls.Add(labelSerialDate, 3, 6);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDateParts, 0, 7);
      this.tableLayoutPanel1.Controls.Add(labelFindTime, 3, 7);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDectectPercentage, 0, 8);
      this.tableLayoutPanel1.Controls.Add(labelDetectPercent, 3, 8);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDetectBoolean, 0, 9);
      this.tableLayoutPanel1.Controls.Add(this.textBoxTrue, 1, 9);
      this.tableLayoutPanel1.Controls.Add(this.textBoxFalse, 2, 9);
      this.tableLayoutPanel1.Controls.Add(labelDetectBool, 3, 9);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDetectGUID, 0, 10);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxIgnoreId, 0, 11);
      this.tableLayoutPanel1.Controls.Add(labelDetectGUID, 3, 10);
      this.tableLayoutPanel1.Controls.Add(labelIgnoreID, 3, 11);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxRemoveCurrencySymbols, 1, 3);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 12;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.Size = new System.Drawing.Size(656, 284);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // FillGuessSettingEdit
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Margin = new System.Windows.Forms.Padding(2);
      this.MinimumSize = new System.Drawing.Size(632, 240);
      this.Name = "FillGuessSettingEdit";
      this.Size = new System.Drawing.Size(656, 300);
      ((System.ComponentModel.ISupportInitialize)(this.trackBarCheckedRecords)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fillGuessSettingsBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMin)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSampleValues)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownChecked)).EndInit();
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private System.Windows.Forms.TrackBar trackBarCheckedRecords;
    private System.Windows.Forms.CheckBox checkBoxDectectNumbers;
    private System.Windows.Forms.CheckBox checkBoxDetectGUID;
    private System.Windows.Forms.CheckBox checkBoxDectectPercentage;
    private System.Windows.Forms.CheckBox checkBoxDetectDateTime;
    private System.Windows.Forms.CheckBox checkBoxSerialDateTime;
    private System.Windows.Forms.CheckBox checkBoxDetectBoolean;
    private System.Windows.Forms.TextBox textBoxTrue;
    private System.Windows.Forms.TextBox textBoxFalse;
    private System.Windows.Forms.BindingSource fillGuessSettingsBindingSource;
    private System.Windows.Forms.CheckBox checkBoxIgnoreId;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.CheckBox checkBoxDateParts;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private System.Windows.Forms.RadioButton radioButtonEnabled;
    private System.Windows.Forms.RadioButton radioButtonDisabled;
    private System.Windows.Forms.NumericUpDown numericUpDownMin;
    private System.Windows.Forms.NumericUpDown numericUpDownSampleValues;
    private System.Windows.Forms.NumericUpDown numericUpDownChecked;
    private System.Windows.Forms.TextBox textBoxDateFormat;
    private System.Windows.Forms.CheckBox checkBoxRemoveCurrencySymbols;
  }
}
