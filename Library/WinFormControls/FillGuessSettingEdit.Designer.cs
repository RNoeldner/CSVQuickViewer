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
      System.Windows.Forms.Label labelDetectBoolean;
      System.Windows.Forms.Label labelDetectNumbers;
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
      checkBoxDetectNumbers = new System.Windows.Forms.CheckBox();
      checkBoxDetectPercentage = new System.Windows.Forms.CheckBox();
      checkBoxDetectDateTime = new System.Windows.Forms.CheckBox();
      checkBoxDetectBoolean = new System.Windows.Forms.CheckBox();
      textBoxTrueLiteralValue = new System.Windows.Forms.TextBox();
      textBoxFalseLiteralValue = new System.Windows.Forms.TextBox();
      checkBoxSerialDateTime = new System.Windows.Forms.CheckBox();
      checkBoxDetectGUID = new System.Windows.Forms.CheckBox();
      checkBoxIgnoreId = new System.Windows.Forms.CheckBox();
      toolTip = new System.Windows.Forms.ToolTip(components);
      checkBoxDateParts = new System.Windows.Forms.CheckBox();
      textBoxDateFormat = new System.Windows.Forms.TextBox();
      radioButtonHeuristicsEnabled = new System.Windows.Forms.RadioButton();
      radioButtonHeuristicsDisabled = new System.Windows.Forms.RadioButton();
      numericUpDownMinConfidence = new System.Windows.Forms.NumericUpDown();
      numericUpDownSampleValues = new System.Windows.Forms.NumericUpDown();
      numericUpDownCheckedRecordsCount = new System.Windows.Forms.NumericUpDown();
      checkBoxRemoveCurrencySymbols = new System.Windows.Forms.CheckBox();
      mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      errorProvider = new System.Windows.Forms.ErrorProvider(components);
      labelDetectBoolean = new System.Windows.Forms.Label();
      labelDetectNumbers = new System.Windows.Forms.Label();
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
      ((System.ComponentModel.ISupportInitialize) numericUpDownMinConfidence).BeginInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownSampleValues).BeginInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownCheckedRecordsCount).BeginInit();
      mainTableLayoutPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) errorProvider).BeginInit();
      SuspendLayout();
      // 
      // labelDetectBoolean
      // 
      labelDetectBoolean.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelDetectBoolean.AutoSize = true;
      labelDetectBoolean.Location = new System.Drawing.Point(226, 203);
      labelDetectBoolean.Name = "labelDetectBoolean";
      labelDetectBoolean.Size = new System.Drawing.Size(441, 26);
      labelDetectBoolean.TabIndex = 22;
      labelDetectBoolean.Text = "Detect Boolean values. e.g. Yes/No, True/False, 1/0.  You may add your own values to the text boxes";
      // 
      // labelDetectNumbers
      // 
      labelDetectNumbers.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelDetectNumbers.AutoSize = true;
      labelDetectNumbers.Location = new System.Drawing.Point(226, 87);
      labelDetectNumbers.Name = "labelDetectNumbers";
      labelDetectNumbers.Size = new System.Drawing.Size(210, 13);
      labelDetectNumbers.TabIndex = 7;
      labelDetectNumbers.Text = "Detect Numeric (Integer or Decimal) values";
      // 
      // labelDetectDate
      // 
      labelDetectDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelDetectDate.AutoSize = true;
      labelDetectDate.Location = new System.Drawing.Point(226, 105);
      labelDetectDate.Name = "labelDetectDate";
      labelDetectDate.Size = new System.Drawing.Size(441, 26);
      labelDetectDate.TabIndex = 10;
      labelDetectDate.Text = "Detect Date/Time values in various formats; If a format is entered the inspection of this date format will not require the minimum number of records it only has to be valid for all records.";
      // 
      // labelDetectPercent
      // 
      labelDetectPercent.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelDetectPercent.AutoSize = true;
      labelDetectPercent.Location = new System.Drawing.Point(226, 185);
      labelDetectPercent.Name = "labelDetectPercent";
      labelDetectPercent.Size = new System.Drawing.Size(352, 13);
      labelDetectPercent.TabIndex = 18;
      labelDetectPercent.Text = "Detect Percentage and Permille, stored as decimal value (divided by 100)";
      // 
      // labelSerialDate
      // 
      labelSerialDate.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelSerialDate.AutoSize = true;
      labelSerialDate.Location = new System.Drawing.Point(226, 136);
      labelSerialDate.Name = "labelSerialDate";
      labelSerialDate.Size = new System.Drawing.Size(318, 13);
      labelSerialDate.TabIndex = 14;
      labelSerialDate.Text = "Allow serial Date Time formats, used in Excel and OLE Automation";
      // 
      // labelIgnoreID
      // 
      labelIgnoreID.AutoSize = true;
      labelIgnoreID.Location = new System.Drawing.Point(226, 252);
      labelIgnoreID.Name = "labelIgnoreID";
      labelIgnoreID.Size = new System.Drawing.Size(373, 13);
      labelIgnoreID.TabIndex = 26;
      labelIgnoreID.Text = "Ignore columns that end with Id, Ref or Text and always process these as text";
      // 
      // labelDetectGUID
      // 
      labelDetectGUID.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelDetectGUID.AutoSize = true;
      labelDetectGUID.Location = new System.Drawing.Point(226, 234);
      labelDetectGUID.Name = "labelDetectGUID";
      labelDetectGUID.Size = new System.Drawing.Size(301, 13);
      labelDetectGUID.TabIndex = 24;
      labelDetectGUID.Text = "Detect Globally Unique Identifier / Universally Unique Identifier";
      // 
      // labelMaxRows
      // 
      labelMaxRows.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelMaxRows.AutoSize = true;
      labelMaxRows.Location = new System.Drawing.Point(226, 59);
      labelMaxRows.Name = "labelMaxRows";
      labelMaxRows.Size = new System.Drawing.Size(221, 13);
      labelMaxRows.TabIndex = 5;
      labelMaxRows.Text = "Maximum rows to check to get sample values";
      // 
      // labelMinMax
      // 
      labelMinMax.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelMinMax.AutoSize = true;
      labelMinMax.Location = new System.Drawing.Point(226, 29);
      labelMinMax.Name = "labelMinMax";
      labelMinMax.Size = new System.Drawing.Size(441, 13);
      labelMinMax.TabIndex = 2;
      labelMinMax.Text = "Minimum and maximum number of samples to read for a before trying to determine the format. ";
      // 
      // labelFindTime
      // 
      labelFindTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
      labelFindTime.AutoSize = true;
      labelFindTime.Location = new System.Drawing.Point(226, 154);
      labelFindTime.Name = "labelFindTime";
      labelFindTime.Size = new System.Drawing.Size(423, 26);
      labelFindTime.TabIndex = 16;
      labelFindTime.Text = "Find associated Time and Time Zone for date columns and combine the information to a date with time\r\n";
      // 
      // trackBarCheckedRecords
      // 
      trackBarCheckedRecords.AutoSize = false;
      mainTableLayoutPanel.SetColumnSpan(trackBarCheckedRecords, 2);
      trackBarCheckedRecords.DataBindings.Add(new System.Windows.Forms.Binding("Value", fillGuessSettingsBindingSource, "CheckedRecords", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      trackBarCheckedRecords.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      trackBarCheckedRecords.Dock = System.Windows.Forms.DockStyle.Top;
      trackBarCheckedRecords.LargeChange = 2000;
      trackBarCheckedRecords.Location = new System.Drawing.Point(3, 52);
      trackBarCheckedRecords.Maximum = 50000;
      trackBarCheckedRecords.Name = "trackBarCheckedRecords";
      trackBarCheckedRecords.Size = new System.Drawing.Size(151, 27);
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
      // checkBoxDetectNumbers
      // 
      checkBoxDetectNumbers.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxDetectNumbers.AutoSize = true;
      checkBoxDetectNumbers.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectNumbers.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "DetectNumbers", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectNumbers.Location = new System.Drawing.Point(3, 85);
      checkBoxDetectNumbers.Name = "checkBoxDetectNumbers";
      checkBoxDetectNumbers.Size = new System.Drawing.Size(65, 17);
      checkBoxDetectNumbers.TabIndex = 6;
      checkBoxDetectNumbers.Text = "Numeric";
      toolTip.SetToolTip(checkBoxDetectNumbers, "Numbers with leading 0 will not be regarded as numbers to prevent information loss");
      checkBoxDetectNumbers.UseVisualStyleBackColor = true;
      // 
      // checkBoxDetectPercentage
      // 
      checkBoxDetectPercentage.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxDetectPercentage.AutoSize = true;
      mainTableLayoutPanel.SetColumnSpan(checkBoxDetectPercentage, 3);
      checkBoxDetectPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "DetectPercentage", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectPercentage.Location = new System.Drawing.Point(3, 183);
      checkBoxDetectPercentage.Name = "checkBoxDetectPercentage";
      checkBoxDetectPercentage.Size = new System.Drawing.Size(81, 17);
      checkBoxDetectPercentage.TabIndex = 17;
      checkBoxDetectPercentage.Text = "Percentage";
      toolTip.SetToolTip(checkBoxDetectPercentage, "Detect Percentage % and Permille ‰");
      checkBoxDetectPercentage.UseVisualStyleBackColor = true;
      // 
      // checkBoxDetectDateTime
      // 
      checkBoxDetectDateTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxDetectDateTime.AutoSize = true;
      checkBoxDetectDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "DetectDateTime", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectDateTime.Location = new System.Drawing.Point(3, 109);
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
      checkBoxDetectBoolean.Location = new System.Drawing.Point(3, 207);
      checkBoxDetectBoolean.Name = "checkBoxDetectBoolean";
      checkBoxDetectBoolean.Size = new System.Drawing.Size(65, 17);
      checkBoxDetectBoolean.TabIndex = 19;
      checkBoxDetectBoolean.Text = "Boolean";
      toolTip.SetToolTip(checkBoxDetectBoolean, "Detect Boolean values, the minimum number of samples does not need to be checked to allow detection");
      checkBoxDetectBoolean.UseVisualStyleBackColor = true;
      // 
      // textBoxTrueLiteralValue
      // 
      textBoxTrueLiteralValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
      textBoxTrueLiteralValue.DataBindings.Add(new System.Windows.Forms.Binding("Text", fillGuessSettingsBindingSource, "TrueValue", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      textBoxTrueLiteralValue.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      textBoxTrueLiteralValue.Location = new System.Drawing.Point(92, 206);
      textBoxTrueLiteralValue.Name = "textBoxTrueLiteralValue";
      textBoxTrueLiteralValue.Size = new System.Drawing.Size(58, 20);
      textBoxTrueLiteralValue.TabIndex = 20;
      toolTip.SetToolTip(textBoxTrueLiteralValue, "Value(s) that should be regarded as TRUE, separated by ;");
      // 
      // textBoxFalseLiteralValue
      // 
      textBoxFalseLiteralValue.Anchor = System.Windows.Forms.AnchorStyles.Left;
      textBoxFalseLiteralValue.DataBindings.Add(new System.Windows.Forms.Binding("Text", fillGuessSettingsBindingSource, "FalseValue", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      textBoxFalseLiteralValue.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      textBoxFalseLiteralValue.Location = new System.Drawing.Point(160, 206);
      textBoxFalseLiteralValue.Name = "textBoxFalseLiteralValue";
      textBoxFalseLiteralValue.Size = new System.Drawing.Size(58, 20);
      textBoxFalseLiteralValue.TabIndex = 21;
      toolTip.SetToolTip(textBoxFalseLiteralValue, "Value(s) that should be regarded as FALSE, separated by ;");
      // 
      // checkBoxSerialDateTime
      // 
      checkBoxSerialDateTime.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxSerialDateTime.AutoSize = true;
      mainTableLayoutPanel.SetColumnSpan(checkBoxSerialDateTime, 3);
      checkBoxSerialDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "SerialDateTime", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxSerialDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxSerialDateTime.Location = new System.Drawing.Point(3, 134);
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
      mainTableLayoutPanel.SetColumnSpan(checkBoxDetectGUID, 2);
      checkBoxDetectGUID.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "DetectGUID", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectGUID.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDetectGUID.Location = new System.Drawing.Point(3, 232);
      checkBoxDetectGUID.Name = "checkBoxDetectGUID";
      checkBoxDetectGUID.Size = new System.Drawing.Size(91, 17);
      checkBoxDetectGUID.TabIndex = 23;
      checkBoxDetectGUID.Text = "GUID / UUID";
      toolTip.SetToolTip(checkBoxDetectGUID, "Detect Unique Identifiers like 0fa3e61e-b976-48be-b112-c09bea582ce5 or {37acd583-d099-47f0-a9af-5422fc1cb3ff}");
      checkBoxDetectGUID.UseVisualStyleBackColor = true;
      // 
      // checkBoxIgnoreId
      // 
      checkBoxIgnoreId.AutoSize = true;
      mainTableLayoutPanel.SetColumnSpan(checkBoxIgnoreId, 3);
      checkBoxIgnoreId.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxIgnoreId.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "IgnoreIdColumns", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxIgnoreId.Location = new System.Drawing.Point(3, 255);
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
      mainTableLayoutPanel.SetColumnSpan(checkBoxDateParts, 3);
      checkBoxDateParts.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "DateParts", true));
      checkBoxDateParts.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxDateParts.Location = new System.Drawing.Point(3, 158);
      checkBoxDateParts.Name = "checkBoxDateParts";
      checkBoxDateParts.Size = new System.Drawing.Size(160, 17);
      checkBoxDateParts.TabIndex = 15;
      checkBoxDateParts.Text = "Include Time and Time zone";
      toolTip.SetToolTip(checkBoxDateParts, "Find columns that possible correspond to a date column to combine date and time");
      checkBoxDateParts.UseVisualStyleBackColor = true;
      // 
      // textBoxDateFormat
      // 
      textBoxDateFormat.Anchor = System.Windows.Forms.AnchorStyles.Left;
      mainTableLayoutPanel.SetColumnSpan(textBoxDateFormat, 2);
      textBoxDateFormat.DataBindings.Add(new System.Windows.Forms.Binding("Text", fillGuessSettingsBindingSource, "DateFormat", true));
      textBoxDateFormat.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      textBoxDateFormat.Location = new System.Drawing.Point(92, 108);
      textBoxDateFormat.Name = "textBoxDateFormat";
      textBoxDateFormat.Size = new System.Drawing.Size(124, 20);
      textBoxDateFormat.TabIndex = 31;
      toolTip.SetToolTip(textBoxDateFormat, "Format that does not require the minimum number of samples to be accepted, if left empty the systems short date format is used.");
      // 
      // radioButtonHeuristicsEnabled
      // 
      radioButtonHeuristicsEnabled.AutoSize = true;
      mainTableLayoutPanel.SetColumnSpan(radioButtonHeuristicsEnabled, 2);
      radioButtonHeuristicsEnabled.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      radioButtonHeuristicsEnabled.Location = new System.Drawing.Point(3, 3);
      radioButtonHeuristicsEnabled.Name = "radioButtonHeuristicsEnabled";
      radioButtonHeuristicsEnabled.Size = new System.Drawing.Size(110, 17);
      radioButtonHeuristicsEnabled.TabIndex = 27;
      radioButtonHeuristicsEnabled.TabStop = true;
      radioButtonHeuristicsEnabled.Text = "Enable Inspection";
      toolTip.SetToolTip(radioButtonHeuristicsEnabled, "As a file is opened each column will be checked if the text represents values");
      radioButtonHeuristicsEnabled.UseVisualStyleBackColor = true;
      // 
      // radioButtonHeuristicsDisabled
      // 
      radioButtonHeuristicsDisabled.AutoSize = true;
      mainTableLayoutPanel.SetColumnSpan(radioButtonHeuristicsDisabled, 2);
      radioButtonHeuristicsDisabled.Location = new System.Drawing.Point(160, 3);
      radioButtonHeuristicsDisabled.Name = "radioButtonHeuristicsDisabled";
      radioButtonHeuristicsDisabled.Size = new System.Drawing.Size(112, 17);
      radioButtonHeuristicsDisabled.TabIndex = 27;
      radioButtonHeuristicsDisabled.TabStop = true;
      radioButtonHeuristicsDisabled.Text = "Disable Inspection";
      toolTip.SetToolTip(radioButtonHeuristicsDisabled, "If automated detection is disabled each text will be read as such, this is faster as the data does not need to be read ");
      radioButtonHeuristicsDisabled.UseVisualStyleBackColor = true;
      // 
      // numericUpDownMinConfidence
      // 
      numericUpDownMinConfidence.AutoSize = true;
      numericUpDownMinConfidence.DataBindings.Add(new System.Windows.Forms.Binding("Value", fillGuessSettingsBindingSource, "MinSamples", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      numericUpDownMinConfidence.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      numericUpDownMinConfidence.Location = new System.Drawing.Point(92, 26);
      numericUpDownMinConfidence.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
      numericUpDownMinConfidence.Name = "numericUpDownMinConfidence";
      numericUpDownMinConfidence.Size = new System.Drawing.Size(41, 20);
      numericUpDownMinConfidence.TabIndex = 28;
      toolTip.SetToolTip(numericUpDownMinConfidence, "Text can be ambiguous and match different formats, a higher value makes sure that the found format is correct. e.g. 10/05/2022 could be the 10th May or the 5th Oct. ");
      // 
      // numericUpDownSampleValues
      // 
      numericUpDownSampleValues.AutoSize = true;
      numericUpDownSampleValues.DataBindings.Add(new System.Windows.Forms.Binding("Value", fillGuessSettingsBindingSource, "SampleValues", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      numericUpDownSampleValues.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      numericUpDownSampleValues.Increment = new decimal(new int[] { 10, 0, 0, 0 });
      numericUpDownSampleValues.Location = new System.Drawing.Point(160, 26);
      numericUpDownSampleValues.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
      numericUpDownSampleValues.Name = "numericUpDownSampleValues";
      numericUpDownSampleValues.Size = new System.Drawing.Size(53, 20);
      numericUpDownSampleValues.TabIndex = 29;
      toolTip.SetToolTip(numericUpDownSampleValues, "Stop reading new unique values if this number is reached.");
      // 
      // numericUpDownCheckedRecordsCount
      // 
      numericUpDownCheckedRecordsCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
      numericUpDownCheckedRecordsCount.AutoSize = true;
      numericUpDownCheckedRecordsCount.DataBindings.Add(new System.Windows.Forms.Binding("Value", fillGuessSettingsBindingSource, "CheckedRecords", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      numericUpDownCheckedRecordsCount.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      numericUpDownCheckedRecordsCount.Increment = new decimal(new int[] { 100, 0, 0, 0 });
      numericUpDownCheckedRecordsCount.Location = new System.Drawing.Point(160, 55);
      numericUpDownCheckedRecordsCount.Maximum = new decimal(new int[] { 50000, 0, 0, 0 });
      numericUpDownCheckedRecordsCount.Name = "numericUpDownCheckedRecordsCount";
      numericUpDownCheckedRecordsCount.Size = new System.Drawing.Size(53, 20);
      numericUpDownCheckedRecordsCount.TabIndex = 30;
      toolTip.SetToolTip(numericUpDownCheckedRecordsCount, "Limit the records to look for text in the columns");
      // 
      // checkBoxRemoveCurrencySymbols
      // 
      checkBoxRemoveCurrencySymbols.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxRemoveCurrencySymbols.AutoSize = true;
      mainTableLayoutPanel.SetColumnSpan(checkBoxRemoveCurrencySymbols, 2);
      checkBoxRemoveCurrencySymbols.DataBindings.Add(new System.Windows.Forms.Binding("Checked", fillGuessSettingsBindingSource, "RemoveCurrencySymbols", true));
      checkBoxRemoveCurrencySymbols.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", fillGuessSettingsBindingSource, "DetectNumbers", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      checkBoxRemoveCurrencySymbols.Location = new System.Drawing.Point(92, 85);
      checkBoxRemoveCurrencySymbols.Name = "checkBoxRemoveCurrencySymbols";
      checkBoxRemoveCurrencySymbols.Size = new System.Drawing.Size(110, 17);
      checkBoxRemoveCurrencySymbols.TabIndex = 6;
      checkBoxRemoveCurrencySymbols.Text = "Currency Symbols";
      toolTip.SetToolTip(checkBoxRemoveCurrencySymbols, "Remove common currency symbols to parse the text");
      checkBoxRemoveCurrencySymbols.UseVisualStyleBackColor = true;
      // 
      // mainTableLayoutPanel
      // 
      mainTableLayoutPanel.AutoSize = true;
      mainTableLayoutPanel.ColumnCount = 4;
      mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      mainTableLayoutPanel.Controls.Add(labelDetectDate, 3, 4);
      mainTableLayoutPanel.Controls.Add(labelDetectNumbers, 3, 3);
      mainTableLayoutPanel.Controls.Add(labelMaxRows, 3, 2);
      mainTableLayoutPanel.Controls.Add(labelMinMax, 3, 1);
      mainTableLayoutPanel.Controls.Add(checkBoxDetectDateTime, 0, 4);
      mainTableLayoutPanel.Controls.Add(checkBoxDetectNumbers, 0, 3);
      mainTableLayoutPanel.Controls.Add(trackBarCheckedRecords, 0, 2);
      mainTableLayoutPanel.Controls.Add(radioButtonHeuristicsEnabled, 0, 0);
      mainTableLayoutPanel.Controls.Add(radioButtonHeuristicsDisabled, 2, 0);
      mainTableLayoutPanel.Controls.Add(numericUpDownMinConfidence, 1, 1);
      mainTableLayoutPanel.Controls.Add(numericUpDownSampleValues, 2, 1);
      mainTableLayoutPanel.Controls.Add(numericUpDownCheckedRecordsCount, 2, 2);
      mainTableLayoutPanel.Controls.Add(textBoxDateFormat, 1, 4);
      mainTableLayoutPanel.Controls.Add(checkBoxSerialDateTime, 0, 6);
      mainTableLayoutPanel.Controls.Add(labelSerialDate, 3, 6);
      mainTableLayoutPanel.Controls.Add(checkBoxDateParts, 0, 7);
      mainTableLayoutPanel.Controls.Add(labelFindTime, 3, 7);
      mainTableLayoutPanel.Controls.Add(checkBoxDetectPercentage, 0, 8);
      mainTableLayoutPanel.Controls.Add(labelDetectPercent, 3, 8);
      mainTableLayoutPanel.Controls.Add(checkBoxDetectBoolean, 0, 9);
      mainTableLayoutPanel.Controls.Add(textBoxTrueLiteralValue, 1, 9);
      mainTableLayoutPanel.Controls.Add(textBoxFalseLiteralValue, 2, 9);
      mainTableLayoutPanel.Controls.Add(labelDetectBoolean, 3, 9);
      mainTableLayoutPanel.Controls.Add(checkBoxDetectGUID, 0, 10);
      mainTableLayoutPanel.Controls.Add(checkBoxIgnoreId, 0, 11);
      mainTableLayoutPanel.Controls.Add(labelDetectGUID, 3, 10);
      mainTableLayoutPanel.Controls.Add(labelIgnoreID, 3, 11);
      mainTableLayoutPanel.Controls.Add(checkBoxRemoveCurrencySymbols, 1, 3);
      mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      mainTableLayoutPanel.Name = "mainTableLayoutPanel";
      mainTableLayoutPanel.RowCount = 12;
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      mainTableLayoutPanel.Size = new System.Drawing.Size(670, 336);
      mainTableLayoutPanel.TabIndex = 0;
      // 
      // errorProvider
      // 
      errorProvider.ContainerControl = this;
      // 
      // FillGuessSettingEdit
      // 
      Controls.Add(mainTableLayoutPanel);
      Margin = new System.Windows.Forms.Padding(0);
      MinimumSize = new System.Drawing.Size(40, 32);
      Name = "FillGuessSettingEdit";
      Size = new System.Drawing.Size(670, 336);
      ((System.ComponentModel.ISupportInitialize) trackBarCheckedRecords).EndInit();
      ((System.ComponentModel.ISupportInitialize) fillGuessSettingsBindingSource).EndInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownMinConfidence).EndInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownSampleValues).EndInit();
      ((System.ComponentModel.ISupportInitialize) numericUpDownCheckedRecordsCount).EndInit();
      mainTableLayoutPanel.ResumeLayout(false);
      mainTableLayoutPanel.PerformLayout();
      ((System.ComponentModel.ISupportInitialize) errorProvider).EndInit();
      ResumeLayout(false);
      PerformLayout();
    }

    #endregion
    private System.Windows.Forms.TrackBar trackBarCheckedRecords;
    private System.Windows.Forms.CheckBox checkBoxDetectNumbers;
    private System.Windows.Forms.CheckBox checkBoxDetectGUID;
    private System.Windows.Forms.CheckBox checkBoxDetectPercentage;
    private System.Windows.Forms.CheckBox checkBoxDetectDateTime;
    private System.Windows.Forms.CheckBox checkBoxSerialDateTime;
    private System.Windows.Forms.CheckBox checkBoxDetectBoolean;
    private System.Windows.Forms.TextBox textBoxTrueLiteralValue;
    private System.Windows.Forms.TextBox textBoxFalseLiteralValue;
    private System.Windows.Forms.BindingSource fillGuessSettingsBindingSource;
    private System.Windows.Forms.CheckBox checkBoxIgnoreId;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.CheckBox checkBoxDateParts;
    private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
    private System.Windows.Forms.RadioButton radioButtonHeuristicsEnabled;
    private System.Windows.Forms.RadioButton radioButtonHeuristicsDisabled;
    private System.Windows.Forms.NumericUpDown numericUpDownMinConfidence;
    private System.Windows.Forms.NumericUpDown numericUpDownSampleValues;
    private System.Windows.Forms.NumericUpDown numericUpDownCheckedRecordsCount;
    private System.Windows.Forms.TextBox textBoxDateFormat;
    private System.Windows.Forms.CheckBox checkBoxRemoveCurrencySymbols;
  }
}
