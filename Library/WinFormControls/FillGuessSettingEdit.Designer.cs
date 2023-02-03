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
      System.Windows.Forms.Label label21;
      System.Windows.Forms.Label label22;
      System.Windows.Forms.Label label23;
      System.Windows.Forms.Label label30;
      System.Windows.Forms.Label label32;
      System.Windows.Forms.Label label1;
      System.Windows.Forms.Label label2;
      System.Windows.Forms.Label label19;
      System.Windows.Forms.Label label20;
      System.Windows.Forms.Label label5;
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
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.checkBoxDateParts = new System.Windows.Forms.CheckBox();
      this.textBoxDateFormat = new System.Windows.Forms.TextBox();
      this.radioButtonEnabled = new System.Windows.Forms.RadioButton();
      this.radioButtonDisabled = new System.Windows.Forms.RadioButton();
      this.numericUpDownMin = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownSampleValues = new System.Windows.Forms.NumericUpDown();
      this.numericUpDownChecked = new System.Windows.Forms.NumericUpDown();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.checkBoxRemoveCurrencySymbols = new System.Windows.Forms.CheckBox();
      label21 = new System.Windows.Forms.Label();
      label22 = new System.Windows.Forms.Label();
      label23 = new System.Windows.Forms.Label();
      label30 = new System.Windows.Forms.Label();
      label32 = new System.Windows.Forms.Label();
      label1 = new System.Windows.Forms.Label();
      label2 = new System.Windows.Forms.Label();
      label19 = new System.Windows.Forms.Label();
      label20 = new System.Windows.Forms.Label();
      label5 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarCheckedRecords)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fillGuessSettingsBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMin)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSampleValues)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.numericUpDownChecked)).BeginInit();
      this.tableLayoutPanel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      this.SuspendLayout();
      // 
      // label21
      // 
      label21.Anchor = System.Windows.Forms.AnchorStyles.Left;
      label21.AutoSize = true;
      label21.Location = new System.Drawing.Point(203, 196);
      label21.Margin = new System.Windows.Forms.Padding(2);
      label21.Name = "label21";
      label21.Size = new System.Drawing.Size(444, 26);
      label21.TabIndex = 22;
      label21.Text = "Detect Boolean values. e.g. Yes/No, True/False, 1/0.  You may add your own values" +
    " to the text boxes";
      // 
      // label22
      // 
      label22.Anchor = System.Windows.Forms.AnchorStyles.Left;
      label22.AutoSize = true;
      label22.Location = new System.Drawing.Point(203, 75);
      label22.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      label22.Name = "label22";
      label22.Size = new System.Drawing.Size(210, 13);
      label22.TabIndex = 7;
      label22.Text = "Detect Numeric (Integer or Decimal) values";
      // 
      // label23
      // 
      label23.Anchor = System.Windows.Forms.AnchorStyles.Left;
      label23.AutoSize = true;
      label23.Location = new System.Drawing.Point(203, 94);
      label23.Margin = new System.Windows.Forms.Padding(2);
      label23.Name = "label23";
      label23.Size = new System.Drawing.Size(441, 26);
      label23.TabIndex = 10;
      label23.Text = "Detect Date/Time values in various formats; If a format is entered the inspection" +
    " of this date format will not require the minimum number of records it only has " +
    "to be valid for all records.";
      // 
      // label30
      // 
      label30.Anchor = System.Windows.Forms.AnchorStyles.Left;
      label30.AutoSize = true;
      label30.Location = new System.Drawing.Point(203, 177);
      label30.Margin = new System.Windows.Forms.Padding(2);
      label30.Name = "label30";
      label30.Size = new System.Drawing.Size(352, 13);
      label30.TabIndex = 18;
      label30.Text = "Detect Percentage and Permille, stored as decimal value (divided by 100)";
      // 
      // label32
      // 
      label32.Anchor = System.Windows.Forms.AnchorStyles.Left;
      label32.AutoSize = true;
      label32.Location = new System.Drawing.Point(203, 126);
      label32.Margin = new System.Windows.Forms.Padding(2);
      label32.Name = "label32";
      label32.Size = new System.Drawing.Size(318, 13);
      label32.TabIndex = 14;
      label32.Text = "Allow serial Date Time formats, used in Excel and OLE Automation";
      // 
      // label1
      // 
      label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
      label1.AutoSize = true;
      label1.Location = new System.Drawing.Point(203, 249);
      label1.Margin = new System.Windows.Forms.Padding(2);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(397, 13);
      label1.TabIndex = 26;
      label1.Text = "Ignore columns that end with “Id”, “Ref” or “Text” and always process these as te" +
    "xt";
      // 
      // label2
      // 
      label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
      label2.AutoSize = true;
      label2.Location = new System.Drawing.Point(203, 228);
      label2.Margin = new System.Windows.Forms.Padding(2);
      label2.Name = "label2";
      label2.Size = new System.Drawing.Size(305, 13);
      label2.TabIndex = 24;
      label2.Text = "Detect Globally Unique IDentifier / Universally Unique IDentifier";
      // 
      // label19
      // 
      label19.Anchor = System.Windows.Forms.AnchorStyles.Left;
      label19.AutoSize = true;
      label19.Location = new System.Drawing.Point(203, 52);
      label19.Margin = new System.Windows.Forms.Padding(2);
      label19.Name = "label19";
      label19.Size = new System.Drawing.Size(221, 13);
      label19.TabIndex = 5;
      label19.Text = "Maximum rows to check to get sample values";
      // 
      // label20
      // 
      label20.Anchor = System.Windows.Forms.AnchorStyles.Left;
      label20.AutoSize = true;
      label20.Location = new System.Drawing.Point(203, 27);
      label20.Margin = new System.Windows.Forms.Padding(2);
      label20.Name = "label20";
      label20.Size = new System.Drawing.Size(444, 13);
      label20.TabIndex = 2;
      label20.Text = "Minimum and maximum number of samples to read for a before trying to determine th" +
    "e format. ";
      // 
      // label5
      // 
      label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
      label5.AutoSize = true;
      label5.Location = new System.Drawing.Point(203, 145);
      label5.Margin = new System.Windows.Forms.Padding(2);
      label5.Name = "label5";
      label5.Size = new System.Drawing.Size(447, 26);
      label5.TabIndex = 16;
      label5.Text = "Find associated Time and Time Zone for date columns and combine the information t" +
    "o a date with time\r\n";
      // 
      // trackBarCheckedRecords
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.trackBarCheckedRecords, 2);
      this.trackBarCheckedRecords.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.fillGuessSettingsBindingSource, "CheckedRecords", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.trackBarCheckedRecords.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.trackBarCheckedRecords.LargeChange = 2000;
      this.trackBarCheckedRecords.Location = new System.Drawing.Point(2, 49);
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
      this.checkBoxDectectNumbers.Location = new System.Drawing.Point(2, 73);
      this.checkBoxDectectNumbers.Margin = new System.Windows.Forms.Padding(2);
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
      this.checkBoxDectectPercentage.Location = new System.Drawing.Point(2, 175);
      this.checkBoxDectectPercentage.Margin = new System.Windows.Forms.Padding(2);
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
      this.checkBoxDetectDateTime.Location = new System.Drawing.Point(2, 98);
      this.checkBoxDetectDateTime.Margin = new System.Windows.Forms.Padding(2);
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
      this.checkBoxDetectBoolean.Location = new System.Drawing.Point(2, 200);
      this.checkBoxDetectBoolean.Margin = new System.Windows.Forms.Padding(2);
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
      this.textBoxTrue.Location = new System.Drawing.Point(89, 199);
      this.textBoxTrue.Margin = new System.Windows.Forms.Padding(2);
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
      this.textBoxFalse.Location = new System.Drawing.Point(141, 199);
      this.textBoxFalse.Margin = new System.Windows.Forms.Padding(2);
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
      this.checkBoxSerialDateTime.Location = new System.Drawing.Point(2, 124);
      this.checkBoxSerialDateTime.Margin = new System.Windows.Forms.Padding(2);
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
      this.checkBoxDetectGUID.Location = new System.Drawing.Point(2, 226);
      this.checkBoxDetectGUID.Margin = new System.Windows.Forms.Padding(2);
      this.checkBoxDetectGUID.Name = "checkBoxDetectGUID";
      this.checkBoxDetectGUID.Size = new System.Drawing.Size(91, 17);
      this.checkBoxDetectGUID.TabIndex = 23;
      this.checkBoxDetectGUID.Text = "GUID / UUID";
      this.toolTip.SetToolTip(this.checkBoxDetectGUID, "Detect Unique Identifiers like 0fa3e61e-b976-48be-b112-c09bea582ce5 or {37acd583-" +
        "d099-47f0-a9af-5422fc1cb3ff}");
      this.checkBoxDetectGUID.UseVisualStyleBackColor = true;
      // 
      // checkBox1
      // 
      this.checkBox1.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBox1.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBox1, 3);
      this.checkBox1.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBox1.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "IgnoreIdColumns", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBox1.Location = new System.Drawing.Point(2, 247);
      this.checkBox1.Margin = new System.Windows.Forms.Padding(2);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(112, 17);
      this.checkBox1.TabIndex = 25;
      this.checkBox1.Text = "Ignore ID columns";
      this.toolTip.SetToolTip(this.checkBox1, "Ignore if the name of the column indicates this to be an ID");
      this.checkBox1.UseVisualStyleBackColor = true;
      // 
      // checkBoxDateParts
      // 
      this.checkBoxDateParts.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxDateParts.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBoxDateParts, 3);
      this.checkBoxDateParts.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DateParts", true));
      this.checkBoxDateParts.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxDateParts.Location = new System.Drawing.Point(2, 149);
      this.checkBoxDateParts.Margin = new System.Windows.Forms.Padding(2);
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
      this.textBoxDateFormat.Location = new System.Drawing.Point(90, 97);
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
      this.radioButtonEnabled.Location = new System.Drawing.Point(2, 2);
      this.radioButtonEnabled.Margin = new System.Windows.Forms.Padding(2);
      this.radioButtonEnabled.Name = "radioButtonEnabled";
      this.radioButtonEnabled.Size = new System.Drawing.Size(107, 17);
      this.radioButtonEnabled.TabIndex = 27;
      this.radioButtonEnabled.TabStop = true;
      this.radioButtonEnabled.Text = "Enable Detection";
      this.toolTip.SetToolTip(this.radioButtonEnabled, "As a file is opened each column will be checked if the text represents values");
      this.radioButtonEnabled.UseVisualStyleBackColor = true;
      // 
      // radioButtonDisabled
      // 
      this.radioButtonDisabled.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.radioButtonDisabled, 2);
      this.radioButtonDisabled.Location = new System.Drawing.Point(141, 2);
      this.radioButtonDisabled.Margin = new System.Windows.Forms.Padding(2);
      this.radioButtonDisabled.Name = "radioButtonDisabled";
      this.radioButtonDisabled.Size = new System.Drawing.Size(163, 17);
      this.radioButtonDisabled.TabIndex = 27;
      this.radioButtonDisabled.TabStop = true;
      this.radioButtonDisabled.Text = "Disable Automated Detection";
      this.toolTip.SetToolTip(this.radioButtonDisabled, "If automated detection is disabled each text will be read as such, this is faster" +
        " as the data does not need to be read ");
      this.radioButtonDisabled.UseVisualStyleBackColor = true;
      // 
      // numericUpDownMin
      // 
      this.numericUpDownMin.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.fillGuessSettingsBindingSource, "MinSamples", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.numericUpDownMin.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "Enabled", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.numericUpDownMin.Location = new System.Drawing.Point(90, 24);
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
      this.numericUpDownSampleValues.Location = new System.Drawing.Point(142, 24);
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
      this.numericUpDownChecked.Location = new System.Drawing.Point(142, 50);
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
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.AutoSize = true;
      this.tableLayoutPanel1.ColumnCount = 4;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Controls.Add(label23, 3, 4);
      this.tableLayoutPanel1.Controls.Add(label22, 3, 3);
      this.tableLayoutPanel1.Controls.Add(label19, 3, 2);
      this.tableLayoutPanel1.Controls.Add(label20, 3, 1);
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
      this.tableLayoutPanel1.Controls.Add(label32, 3, 6);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDateParts, 0, 7);
      this.tableLayoutPanel1.Controls.Add(label5, 3, 7);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDectectPercentage, 0, 8);
      this.tableLayoutPanel1.Controls.Add(label30, 3, 8);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDetectBoolean, 0, 9);
      this.tableLayoutPanel1.Controls.Add(this.textBoxTrue, 1, 9);
      this.tableLayoutPanel1.Controls.Add(this.textBoxFalse, 2, 9);
      this.tableLayoutPanel1.Controls.Add(label21, 3, 9);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDetectGUID, 0, 10);
      this.tableLayoutPanel1.Controls.Add(this.checkBox1, 0, 11);
      this.tableLayoutPanel1.Controls.Add(label2, 3, 10);
      this.tableLayoutPanel1.Controls.Add(label1, 3, 11);
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
      this.tableLayoutPanel1.Size = new System.Drawing.Size(656, 266);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // checkBoxRemoveCurrencySymbols
      // 
      this.checkBoxRemoveCurrencySymbols.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxRemoveCurrencySymbols.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBoxRemoveCurrencySymbols, 2);
      this.checkBoxRemoveCurrencySymbols.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "RemoveCurrencySymbols", true));
      this.checkBoxRemoveCurrencySymbols.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.fillGuessSettingsBindingSource, "DetectNumbers", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.checkBoxRemoveCurrencySymbols.Location = new System.Drawing.Point(89, 73);
      this.checkBoxRemoveCurrencySymbols.Margin = new System.Windows.Forms.Padding(2);
      this.checkBoxRemoveCurrencySymbols.Name = "checkBoxRemoveCurrencySymbols";
      this.checkBoxRemoveCurrencySymbols.Size = new System.Drawing.Size(110, 17);
      this.checkBoxRemoveCurrencySymbols.TabIndex = 6;
      this.checkBoxRemoveCurrencySymbols.Text = "Currency Symbols";
      this.toolTip.SetToolTip(this.checkBoxRemoveCurrencySymbols, "Remove common currency symbols to parse the text");
      this.checkBoxRemoveCurrencySymbols.UseVisualStyleBackColor = true;
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
    private System.Windows.Forms.CheckBox checkBox1;
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
