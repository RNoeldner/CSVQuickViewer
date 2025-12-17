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
      components = new System.ComponentModel.Container();
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
      trackBarCheckedRecords = new System.Windows.Forms.TrackBar();
      fillGuessSettingsBindingSource = new System.Windows.Forms.BindingSource(components);
      checkBoxDectectNumbers = new System.Windows.Forms.CheckBox();
      checkBoxDectectPercentage = new System.Windows.Forms.CheckBox();
      checkBoxDetectDateTime = new System.Windows.Forms.CheckBox();
      checkBoxDetectBoolean = new System.Windows.Forms.CheckBox();
      textBoxTrue = new System.Windows.Forms.TextBox();
      textBoxFalse = new System.Windows.Forms.TextBox();
      checkBoxSerialDateTime = new System.Windows.Forms.CheckBox();
      checkBoxDetectGUID = new System.Windows.Forms.CheckBox();
      checkBoxIgnoreId = new System.Windows.Forms.CheckBox();
      toolTip = new System.Windows.Forms.ToolTip(components);
      checkBoxDateParts = new System.Windows.Forms.CheckBox();
      textBoxDateFormat = new System.Windows.Forms.TextBox();
      radioButtonEnabled = new System.Windows.Forms.RadioButton();
      radioButtonDisabled = new System.Windows.Forms.RadioButton();
      numericUpDownMin = new System.Windows.Forms.NumericUpDown();
      numericUpDownSampleValues = new System.Windows.Forms.NumericUpDown();
      numericUpDownChecked = new System.Windows.Forms.NumericUpDown();
      checkBoxRemoveCurrencySymbols = new System.Windows.Forms.CheckBox();
      tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      errorProvider = new System.Windows.Forms.ErrorProvider(components);
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
      ((System.ComponentModel.ISupportInitialize) trackBarCheckedRecords).BeginInit();
      ((System.ComponentModel.ISupportInitialize) fillGuessSettingsBindingSource).BeginInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownMin).BeginInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownSampleValues).BeginInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownChecked).BeginInit();
      tableLayoutPanel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) errorProvider).BeginInit();
      SuspendLayout();
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
      labelDetectBool.Text = "Detect Boolean values. e.g. Yes/No, True/False, 1/0.  You may add your own values to the text boxes";
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
      labelDetectDate.Text = "Detect Date/Time values in various formats; If a format is entered the inspection of this date format will not require the minimum number of records it only has to be valid for all records.";
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
      labelIgnoreID.Size = new System.Drawing.Size(373, 13);
      labelIgnoreID.TabIndex = 26;
      labelIgnoreID.Text = "Ignore columns that end with Id, Ref or Text and always process these as text";
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
      labelMinMax.Text = "Minimum and maximum number of samples to read for a before trying to determine the format. ";
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
      labelFindTime.Text = "Find associated Time and Time Zone for date columns and combine the information to a date with time\r\n";
      // 
      // trackBarCheckedRecords
      // 
      tableLayoutPanel1.SetColumnSpan(trackBarCheckedRecords, 2);
      trackBarCheckedRecords.DataBindings.Add(new System.Windows.Forms.Binding("Value", fillGuessSettingsBindingSource, "CheckedRecords", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      trackBarCheckedRecords.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      trackBarCheckedRecords.LargeChange = 2000;
      trackBarCheckedRecords.Location = new System.Drawing.Point(2, 51);
      trackBarCheckedRecords.Margin = new System.Windows.Forms.Padding(2);
      trackBarCheckedRecords.Maximum = 50000;
      trackBarCheckedRecords.Name = "trackBarCheckedRecords";
      trackBarCheckedRecords.Size = new System.Drawing.Size(129, 20);
      trackBarCheckedRecords.SmallChange = 100;
      trackBarCheckedRecords.TabIndex = 3;
      trackBarCheckedRecords.TickFrequency = 2000;
      trackBarCheckedRecords.Value = 250;
      // 
      // fillGuessSettingsBindingSource
      // 
      fillGuessSettingsBindingSource.AllowNew = false;
      fillGuessSettingsBindingSource.DataSource = typeof(FillGuessSettings);
      // 
      // checkBoxDectectNumbers
      // 
      checkBoxDectectNumbers.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxDectectNumbers.AutoSize = true;
      checkBoxDectectNumbers.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDectectNumbers.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "DetectNumbers", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDectectNumbers.Location = new System.Drawing.Point(3, 76);
      checkBoxDectectNumbers.Name = "checkBoxDectectNumbers";
      checkBoxDectectNumbers.Size = new System.Drawing.Size(65, 17);
      checkBoxDectectNumbers.TabIndex = 6;
      checkBoxDectectNumbers.Text = "Numeric";
      toolTip.SetToolTip(checkBoxDectectNumbers, "Numbers with leading 0 will not be regarded as numbers to prevent information loss");
      checkBoxDectectNumbers.UseVisualStyleBackColor = true;
      // 
      // checkBoxDectectPercentage
      // 
      checkBoxDectectPercentage.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxDectectPercentage.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(checkBoxDectectPercentage, 3);
      checkBoxDectectPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDectectPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "DetectPercentage", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDectectPercentage.Location = new System.Drawing.Point(3, 186);
      checkBoxDectectPercentage.Name = "checkBoxDectectPercentage";
      checkBoxDectectPercentage.Size = new System.Drawing.Size(81, 17);
      checkBoxDectectPercentage.TabIndex = 17;
      checkBoxDectectPercentage.Text = "Percentage";
      toolTip.SetToolTip(checkBoxDectectPercentage, "Detect Percentage % and Permille ‰");
      checkBoxDectectPercentage.UseVisualStyleBackColor = true;
      // 
      // checkBoxDetectDateTime
      // 
      checkBoxDetectDateTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxDetectDateTime.AutoSize = true;
      checkBoxDetectDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "DetectDateTime", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectDateTime.Location = new System.Drawing.Point(3, 103);
      checkBoxDetectDateTime.Name = "checkBoxDetectDateTime";
      checkBoxDetectDateTime.Size = new System.Drawing.Size(83, 17);
      checkBoxDetectDateTime.TabIndex = 8;
      checkBoxDetectDateTime.Text = "Date / Time";
      toolTip.SetToolTip(checkBoxDetectDateTime, "Detect dates and times on a variety of formats, to make sure the order of day and month is determined correctly enough sample values should be present");
      checkBoxDetectDateTime.UseVisualStyleBackColor = true;
      // 
      // checkBoxDetectBoolean
      // 
      checkBoxDetectBoolean.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxDetectBoolean.AutoSize = true;
      checkBoxDetectBoolean.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "DetectBoolean", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectBoolean.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectBoolean.Location = new System.Drawing.Point(3, 213);
      checkBoxDetectBoolean.Name = "checkBoxDetectBoolean";
      checkBoxDetectBoolean.Size = new System.Drawing.Size(65, 17);
      checkBoxDetectBoolean.TabIndex = 19;
      checkBoxDetectBoolean.Text = "Boolean";
      toolTip.SetToolTip(checkBoxDetectBoolean, "Detect Boolean values, the minimum number of samples does not need to be checked to allow detection");
      checkBoxDetectBoolean.UseVisualStyleBackColor = true;
      // 
      // textBoxTrue
      // 
      textBoxTrue.Anchor = System.Windows.Forms.AnchorStyles.Left;
      textBoxTrue.DataBindings.Add(new System.Windows.Forms.Binding("Text", fillGuessSettingsBindingSource, "TrueValue", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      textBoxTrue.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      textBoxTrue.Location = new System.Drawing.Point(92, 212);
      textBoxTrue.Name = "textBoxTrue";
      textBoxTrue.Size = new System.Drawing.Size(48, 20);
      textBoxTrue.TabIndex = 20;
      toolTip.SetToolTip(textBoxTrue, "Value(s) that should be regarded as TRUE, separated by ;");
      // 
      // textBoxFalse
      // 
      textBoxFalse.Anchor = System.Windows.Forms.AnchorStyles.Left;
      textBoxFalse.DataBindings.Add(new System.Windows.Forms.Binding("Text", fillGuessSettingsBindingSource, "FalseValue", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      textBoxFalse.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      textBoxFalse.Location = new System.Drawing.Point(146, 212);
      textBoxFalse.Name = "textBoxFalse";
      textBoxFalse.Size = new System.Drawing.Size(48, 20);
      textBoxFalse.TabIndex = 21;
      toolTip.SetToolTip(textBoxFalse, "Value(s) that should be regarded as FALSE, separated by ;");
      // 
      // checkBoxSerialDateTime
      // 
      checkBoxSerialDateTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxSerialDateTime.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(checkBoxSerialDateTime, 3);
      checkBoxSerialDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "SerialDateTime", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxSerialDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxSerialDateTime.Location = new System.Drawing.Point(3, 131);
      checkBoxSerialDateTime.Name = "checkBoxSerialDateTime";
      checkBoxSerialDateTime.Size = new System.Drawing.Size(129, 17);
      checkBoxSerialDateTime.TabIndex = 13;
      checkBoxSerialDateTime.Text = "Allow Serial DateTime";
      toolTip.SetToolTip(checkBoxSerialDateTime, "Excel stores dates as number of days after the December 31, 1899: \r\nJanuary 1, 1900  is 1 or \r\nSaturday, 15. December 2018 13:40:10 is 43449.56956\r\n");
      checkBoxSerialDateTime.UseVisualStyleBackColor = true;
      // 
      // checkBoxDetectGUID
      // 
      checkBoxDetectGUID.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxDetectGUID.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(checkBoxDetectGUID, 2);
      checkBoxDetectGUID.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "DetectGUID", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectGUID.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectGUID.Location = new System.Drawing.Point(3, 241);
      checkBoxDetectGUID.Name = "checkBoxDetectGUID";
      checkBoxDetectGUID.Size = new System.Drawing.Size(91, 17);
      checkBoxDetectGUID.TabIndex = 23;
      checkBoxDetectGUID.Text = "GUID / UUID";
      toolTip.SetToolTip(checkBoxDetectGUID, "Detect Unique Identifiers like 0fa3e61e-b976-48be-b112-c09bea582ce5 or {37acd583-d099-47f0-a9af-5422fc1cb3ff}");
      checkBoxDetectGUID.UseVisualStyleBackColor = true;
      // 
      // checkBoxIgnoreId
      // 
      checkBoxIgnoreId.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxIgnoreId.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(checkBoxIgnoreId, 3);
      checkBoxIgnoreId.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxIgnoreId.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "IgnoreIdColumns", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxIgnoreId.Location = new System.Drawing.Point(3, 264);
      checkBoxIgnoreId.Name = "checkBoxIgnoreId";
      checkBoxIgnoreId.Size = new System.Drawing.Size(112, 17);
      checkBoxIgnoreId.TabIndex = 25;
      checkBoxIgnoreId.Text = "Ignore ID columns";
      toolTip.SetToolTip(checkBoxIgnoreId, "Ignore if the name of the column indicates this to be an ID");
      checkBoxIgnoreId.UseVisualStyleBackColor = true;
      // 
      // checkBoxDateParts
      // 
      checkBoxDateParts.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxDateParts.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(checkBoxDateParts, 3);
      checkBoxDateParts.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "DateParts", true));
      checkBoxDateParts.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDateParts.Location = new System.Drawing.Point(3, 158);
      checkBoxDateParts.Name = "checkBoxDateParts";
      checkBoxDateParts.Size = new System.Drawing.Size(157, 17);
      checkBoxDateParts.TabIndex = 15;
      checkBoxDateParts.Text = "Include Time and Timezone";
      toolTip.SetToolTip(checkBoxDateParts, "Find columns that possible correspond to a date column to combine date and time");
      checkBoxDateParts.UseVisualStyleBackColor = true;
      // 
      // textBoxDateFormat
      // 
      textBoxDateFormat.Anchor = System.Windows.Forms.AnchorStyles.Left;
      tableLayoutPanel1.SetColumnSpan(textBoxDateFormat, 2);
      textBoxDateFormat.DataBindings.Add(new System.Windows.Forms.Binding("Text", fillGuessSettingsBindingSource, "DateFormat", true));
      textBoxDateFormat.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      textBoxDateFormat.Location = new System.Drawing.Point(92, 102);
      textBoxDateFormat.Name = "textBoxDateFormat";
      textBoxDateFormat.Size = new System.Drawing.Size(100, 20);
      textBoxDateFormat.TabIndex = 31;
      toolTip.SetToolTip(textBoxDateFormat, "Format that does not require the minimum number of samples to be accepted, if left empty the systems short date format is used.");
      // 
      // radioButtonEnabled
      // 
      radioButtonEnabled.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(radioButtonEnabled, 2);
      radioButtonEnabled.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      radioButtonEnabled.Location = new System.Drawing.Point(3, 3);
      radioButtonEnabled.Name = "radioButtonEnabled";
      radioButtonEnabled.Size = new System.Drawing.Size(110, 17);
      radioButtonEnabled.TabIndex = 27;
      radioButtonEnabled.TabStop = true;
      radioButtonEnabled.Text = "Enable Inspection";
      toolTip.SetToolTip(radioButtonEnabled, "As a file is opened each column will be checked if the text represents values");
      radioButtonEnabled.UseVisualStyleBackColor = true;
      // 
      // radioButtonDisabled
      // 
      radioButtonDisabled.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(radioButtonDisabled, 2);
      radioButtonDisabled.Location = new System.Drawing.Point(146, 3);
      radioButtonDisabled.Name = "radioButtonDisabled";
      radioButtonDisabled.Size = new System.Drawing.Size(112, 17);
      radioButtonDisabled.TabIndex = 27;
      radioButtonDisabled.TabStop = true;
      radioButtonDisabled.Text = "Disable Inspection";
      toolTip.SetToolTip(radioButtonDisabled, "If automated detection is disabled each text will be read as such, this is faster as the data does not need to be read ");
      radioButtonDisabled.UseVisualStyleBackColor = true;
      // 
      // numericUpDownMin
      // 
      numericUpDownMin.DataBindings.Add(new System.Windows.Forms.Binding("Value", fillGuessSettingsBindingSource, "MinSamples", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      numericUpDownMin.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      numericUpDownMin.Location = new System.Drawing.Point(92, 26);
      numericUpDownMin.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
      numericUpDownMin.Name = "numericUpDownMin";
      numericUpDownMin.Size = new System.Drawing.Size(46, 20);
      numericUpDownMin.TabIndex = 28;
      toolTip.SetToolTip(numericUpDownMin, "Text can be ambiguous and match different formats, a higher value makes sure that the found format is correct. e.g. 10/05/2022 could be the 10th May or the 5th Oct. ");
      // 
      // numericUpDownSampleValues
      // 
      numericUpDownSampleValues.DataBindings.Add(new System.Windows.Forms.Binding("Value", fillGuessSettingsBindingSource, "SampleValues", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      numericUpDownSampleValues.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      numericUpDownSampleValues.Increment = new decimal(new int[] { 10, 0, 0, 0 });
      numericUpDownSampleValues.Location = new System.Drawing.Point(146, 26);
      numericUpDownSampleValues.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
      numericUpDownSampleValues.Name = "numericUpDownSampleValues";
      numericUpDownSampleValues.Size = new System.Drawing.Size(46, 20);
      numericUpDownSampleValues.TabIndex = 29;
      toolTip.SetToolTip(numericUpDownSampleValues, "Stop reading new unique values if this number is reached.");
      // 
      // numericUpDownChecked
      // 
      numericUpDownChecked.DataBindings.Add(new System.Windows.Forms.Binding("Value", fillGuessSettingsBindingSource, "CheckedRecords", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      numericUpDownChecked.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      numericUpDownChecked.Increment = new decimal(new int[] { 100, 0, 0, 0 });
      numericUpDownChecked.Location = new System.Drawing.Point(146, 52);
      numericUpDownChecked.Maximum = new decimal(new int[] { 50000, 0, 0, 0 });
      numericUpDownChecked.Name = "numericUpDownChecked";
      numericUpDownChecked.Size = new System.Drawing.Size(46, 20);
      numericUpDownChecked.TabIndex = 30;
      toolTip.SetToolTip(numericUpDownChecked, "Limit the records to look for text in the columns");
      // 
      // checkBoxRemoveCurrencySymbols
      // 
      checkBoxRemoveCurrencySymbols.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxRemoveCurrencySymbols.AutoSize = true;
      tableLayoutPanel1.SetColumnSpan(checkBoxRemoveCurrencySymbols, 2);
      checkBoxRemoveCurrencySymbols.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "RemoveCurrencySymbols", true));
      checkBoxRemoveCurrencySymbols.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "DetectNumbers", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxRemoveCurrencySymbols.Location = new System.Drawing.Point(92, 76);
      checkBoxRemoveCurrencySymbols.Name = "checkBoxRemoveCurrencySymbols";
      checkBoxRemoveCurrencySymbols.Size = new System.Drawing.Size(110, 17);
      checkBoxRemoveCurrencySymbols.TabIndex = 6;
      checkBoxRemoveCurrencySymbols.Text = "Currency Symbols";
      toolTip.SetToolTip(checkBoxRemoveCurrencySymbols, "Remove common currency symbols to parse the text");
      checkBoxRemoveCurrencySymbols.UseVisualStyleBackColor = true;
      // 
      // tableLayoutPanel1
      // 
      tableLayoutPanel1.AutoSize = true;
      tableLayoutPanel1.ColumnCount = 4;
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel1.Controls.Add(labelDetectDate, 3, 4);
      tableLayoutPanel1.Controls.Add(labelDetectNumeric, 3, 3);
      tableLayoutPanel1.Controls.Add(labelMaxRows, 3, 2);
      tableLayoutPanel1.Controls.Add(labelMinMax, 3, 1);
      tableLayoutPanel1.Controls.Add(checkBoxDetectDateTime, 0, 4);
      tableLayoutPanel1.Controls.Add(checkBoxDectectNumbers, 0, 3);
      tableLayoutPanel1.Controls.Add(trackBarCheckedRecords, 0, 2);
      tableLayoutPanel1.Controls.Add(radioButtonEnabled, 0, 0);
      tableLayoutPanel1.Controls.Add(radioButtonDisabled, 2, 0);
      tableLayoutPanel1.Controls.Add(numericUpDownMin, 1, 1);
      tableLayoutPanel1.Controls.Add(numericUpDownSampleValues, 2, 1);
      tableLayoutPanel1.Controls.Add(numericUpDownChecked, 2, 2);
      tableLayoutPanel1.Controls.Add(textBoxDateFormat, 1, 4);
      tableLayoutPanel1.Controls.Add(checkBoxSerialDateTime, 0, 6);
      tableLayoutPanel1.Controls.Add(labelSerialDate, 3, 6);
      tableLayoutPanel1.Controls.Add(checkBoxDateParts, 0, 7);
      tableLayoutPanel1.Controls.Add(labelFindTime, 3, 7);
      tableLayoutPanel1.Controls.Add(checkBoxDectectPercentage, 0, 8);
      tableLayoutPanel1.Controls.Add(labelDetectPercent, 3, 8);
      tableLayoutPanel1.Controls.Add(checkBoxDetectBoolean, 0, 9);
      tableLayoutPanel1.Controls.Add(textBoxTrue, 1, 9);
      tableLayoutPanel1.Controls.Add(textBoxFalse, 2, 9);
      tableLayoutPanel1.Controls.Add(labelDetectBool, 3, 9);
      tableLayoutPanel1.Controls.Add(checkBoxDetectGUID, 0, 10);
      tableLayoutPanel1.Controls.Add(checkBoxIgnoreId, 0, 11);
      tableLayoutPanel1.Controls.Add(labelDetectGUID, 3, 10);
      tableLayoutPanel1.Controls.Add(labelIgnoreID, 3, 11);
      tableLayoutPanel1.Controls.Add(checkBoxRemoveCurrencySymbols, 1, 3);
      tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
      tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
      tableLayoutPanel1.Name = "tableLayoutPanel1";
      tableLayoutPanel1.RowCount = 12;
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.Size = new System.Drawing.Size(656, 284);
      tableLayoutPanel1.TabIndex = 0;
      // 
      // errorProvider
      // 
      errorProvider.ContainerControl = this;
      // 
      // FillGuessSettingEdit
      // 
      AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      Controls.Add(tableLayoutPanel1);
      Margin = new System.Windows.Forms.Padding(2);
      MinimumSize = new System.Drawing.Size(632, 240);
      Name = "FillGuessSettingEdit";
      Size = new System.Drawing.Size(656, 300);
      ((System.ComponentModel.ISupportInitialize) trackBarCheckedRecords).EndInit();
      ((System.ComponentModel.ISupportInitialize) fillGuessSettingsBindingSource).EndInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownMin).EndInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownSampleValues).EndInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownChecked).EndInit();
      tableLayoutPanel1.ResumeLayout(false);
      tableLayoutPanel1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) errorProvider).EndInit();
      ResumeLayout(false);
      PerformLayout();
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
