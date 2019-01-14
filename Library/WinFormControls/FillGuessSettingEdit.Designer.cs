namespace CsvTools
{
  partial class FillGuessSettingEdit
  {
    /// <summary>
    /// The components
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>Dispose
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Vom Komponenten-Designer generierter Code


    /// <summary>
    /// Initializes the component.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.trackBarCheckedRecords = new System.Windows.Forms.TrackBar();
      this.textBoxCheckedRecords = new System.Windows.Forms.TextBox();
      this.trackBarSampleValues = new System.Windows.Forms.TrackBar();
      this.textBoxSampleValues = new System.Windows.Forms.TextBox();
      this.checkBoxDectectNumbers = new System.Windows.Forms.CheckBox();
      this.label21 = new System.Windows.Forms.Label();
      this.label22 = new System.Windows.Forms.Label();
      this.checkBoxDectectPercentage = new System.Windows.Forms.CheckBox();
      this.checkBoxDetectDateTime = new System.Windows.Forms.CheckBox();
      this.label23 = new System.Windows.Forms.Label();
      this.label30 = new System.Windows.Forms.Label();
      this.checkBoxDetectBoolean = new System.Windows.Forms.CheckBox();
      this.textBoxTrue = new System.Windows.Forms.TextBox();
      this.textBoxFalse = new System.Windows.Forms.TextBox();
      this.checkBoxSerialDateTime = new System.Windows.Forms.CheckBox();
      this.label32 = new System.Windows.Forms.Label();
      this.checkBoxDetectGUID = new System.Windows.Forms.CheckBox();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.label19 = new System.Windows.Forms.Label();
      this.label20 = new System.Windows.Forms.Label();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.textBoxMinSamplesForIntDate = new System.Windows.Forms.TextBox();
      this.checkBoxNamedDates = new System.Windows.Forms.CheckBox();
      this.label4 = new System.Windows.Forms.Label();
      this.checkBoxDateParts = new System.Windows.Forms.CheckBox();
      this.label5 = new System.Windows.Forms.Label();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.fillGuessSettingsBindingSource = new System.Windows.Forms.BindingSource(this.components);
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.trackBarCheckedRecords)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarSampleValues)).BeginInit();
      this.tableLayoutPanel1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.fillGuessSettingsBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      this.SuspendLayout();
      // 
      // trackBarCheckedRecords
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.trackBarCheckedRecords, 2);
      this.trackBarCheckedRecords.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.fillGuessSettingsBindingSource, "SampleValues", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.trackBarCheckedRecords.LargeChange = 50;
      this.trackBarCheckedRecords.Location = new System.Drawing.Point(3, 51);
      this.trackBarCheckedRecords.Maximum = 500;
      this.trackBarCheckedRecords.Name = "trackBarCheckedRecords";
      this.trackBarCheckedRecords.Size = new System.Drawing.Size(157, 42);
      this.trackBarCheckedRecords.SmallChange = 10;
      this.trackBarCheckedRecords.TabIndex = 3;
      this.trackBarCheckedRecords.TickFrequency = 50;
      this.trackBarCheckedRecords.Value = 250;
      // 
      // textBoxCheckedRecords
      // 
      this.textBoxCheckedRecords.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "SampleValues", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.textBoxCheckedRecords.Location = new System.Drawing.Point(166, 51);
      this.textBoxCheckedRecords.Name = "textBoxCheckedRecords";
      this.textBoxCheckedRecords.Size = new System.Drawing.Size(48, 20);
      this.textBoxCheckedRecords.TabIndex = 4;
      // 
      // trackBarSampleValues
      // 
      this.tableLayoutPanel1.SetColumnSpan(this.trackBarSampleValues, 2);
      this.trackBarSampleValues.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.fillGuessSettingsBindingSource, "CheckedRecords", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.trackBarSampleValues.LargeChange = 2000;
      this.trackBarSampleValues.Location = new System.Drawing.Point(3, 3);
      this.trackBarSampleValues.Maximum = 26000;
      this.trackBarSampleValues.Name = "trackBarSampleValues";
      this.trackBarSampleValues.Size = new System.Drawing.Size(157, 42);
      this.trackBarSampleValues.SmallChange = 500;
      this.trackBarSampleValues.TabIndex = 0;
      this.trackBarSampleValues.TickFrequency = 2000;
      this.trackBarSampleValues.Value = 10000;
      // 
      // textBoxSampleValues
      // 
      this.textBoxSampleValues.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "CheckedRecords", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.textBoxSampleValues.Location = new System.Drawing.Point(166, 3);
      this.textBoxSampleValues.Name = "textBoxSampleValues";
      this.textBoxSampleValues.Size = new System.Drawing.Size(48, 20);
      this.textBoxSampleValues.TabIndex = 1;
      // 
      // checkBoxDectectNumbers
      // 
      this.checkBoxDectectNumbers.AutoSize = true;
      this.checkBoxDectectNumbers.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DectectNumbers", true));
      this.checkBoxDectectNumbers.Location = new System.Drawing.Point(3, 99);
      this.checkBoxDectectNumbers.Name = "checkBoxDectectNumbers";
      this.checkBoxDectectNumbers.Size = new System.Drawing.Size(68, 17);
      this.checkBoxDectectNumbers.TabIndex = 6;
      this.checkBoxDectectNumbers.Text = "Numbers";
      this.toolTip.SetToolTip(this.checkBoxDectectNumbers, "Numbers with leading 0 will not be regarded as numbers to prevent information los" +
        "s");
      this.checkBoxDectectNumbers.UseVisualStyleBackColor = true;
      // 
      // label21
      // 
      this.label21.AutoSize = true;
      this.label21.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label21.Location = new System.Drawing.Point(220, 242);
      this.label21.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(502, 21);
      this.label21.TabIndex = 22;
      this.label21.Text = "Detect Boolean values like: Yes/No, True/False, 1/0.  You may add your own values" +
    " to the text boxes\r\n";
      // 
      // label22
      // 
      this.label22.AutoSize = true;
      this.label22.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label22.Location = new System.Drawing.Point(220, 101);
      this.label22.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      this.label22.Name = "label22";
      this.label22.Size = new System.Drawing.Size(502, 18);
      this.label22.TabIndex = 7;
      this.label22.Text = "Detect Numeric values";
      // 
      // checkBoxDectectPercentage
      // 
      this.checkBoxDectectPercentage.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBoxDectectPercentage, 2);
      this.checkBoxDectectPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DectectPercentage", true));
      this.checkBoxDectectPercentage.Location = new System.Drawing.Point(3, 217);
      this.checkBoxDectectPercentage.Name = "checkBoxDectectPercentage";
      this.checkBoxDectectPercentage.Size = new System.Drawing.Size(81, 17);
      this.checkBoxDectectPercentage.TabIndex = 17;
      this.checkBoxDectectPercentage.Text = "Percentage";
      this.checkBoxDectectPercentage.UseVisualStyleBackColor = true;
      // 
      // checkBoxDetectDateTime
      // 
      this.checkBoxDetectDateTime.AutoSize = true;
      this.checkBoxDetectDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DetectDateTime", true));
      this.checkBoxDetectDateTime.Location = new System.Drawing.Point(3, 122);
      this.checkBoxDetectDateTime.Name = "checkBoxDetectDateTime";
      this.checkBoxDetectDateTime.Size = new System.Drawing.Size(83, 17);
      this.checkBoxDetectDateTime.TabIndex = 8;
      this.checkBoxDetectDateTime.Text = "Date / Time";
      this.checkBoxDetectDateTime.UseVisualStyleBackColor = true;
      // 
      // label23
      // 
      this.label23.AutoSize = true;
      this.label23.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label23.Location = new System.Drawing.Point(220, 124);
      this.label23.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      this.label23.Name = "label23";
      this.label23.Size = new System.Drawing.Size(502, 21);
      this.label23.TabIndex = 10;
      this.label23.Text = "Detect Date/Time values.  / Number of required values";
      // 
      // label30
      // 
      this.label30.AutoSize = true;
      this.label30.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label30.Location = new System.Drawing.Point(220, 219);
      this.label30.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      this.label30.Name = "label30";
      this.label30.Size = new System.Drawing.Size(502, 18);
      this.label30.TabIndex = 18;
      this.label30.Text = "Detect Percentages and store them as numeric values (divided by 100)";
      // 
      // checkBoxDetectBoolean
      // 
      this.checkBoxDetectBoolean.AutoSize = true;
      this.checkBoxDetectBoolean.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DetectBoolean", true));
      this.checkBoxDetectBoolean.Location = new System.Drawing.Point(3, 240);
      this.checkBoxDetectBoolean.Name = "checkBoxDetectBoolean";
      this.checkBoxDetectBoolean.Size = new System.Drawing.Size(65, 17);
      this.checkBoxDetectBoolean.TabIndex = 19;
      this.checkBoxDetectBoolean.Text = "Boolean";
      this.checkBoxDetectBoolean.UseVisualStyleBackColor = true;
      // 
      // textBoxTrue
      // 
      this.textBoxTrue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "TrueValue", true));
      this.textBoxTrue.Location = new System.Drawing.Point(92, 240);
      this.textBoxTrue.Name = "textBoxTrue";
      this.textBoxTrue.Size = new System.Drawing.Size(48, 20);
      this.textBoxTrue.TabIndex = 20;
      this.toolTip.SetToolTip(this.textBoxTrue, "Value(s) that should be regarded as TRUE, separated by ;");
      // 
      // textBoxFalse
      // 
      this.textBoxFalse.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "FalseValue", true));
      this.textBoxFalse.Location = new System.Drawing.Point(166, 240);
      this.textBoxFalse.Name = "textBoxFalse";
      this.textBoxFalse.Size = new System.Drawing.Size(48, 20);
      this.textBoxFalse.TabIndex = 21;
      this.toolTip.SetToolTip(this.textBoxFalse, "Value(s) that should be regarded as FALSE, separated by ;");
      // 
      // checkBoxSerialDateTime
      // 
      this.checkBoxSerialDateTime.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBoxSerialDateTime, 2);
      this.checkBoxSerialDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "SerialDateTime", true));
      this.checkBoxSerialDateTime.Location = new System.Drawing.Point(3, 171);
      this.checkBoxSerialDateTime.Name = "checkBoxSerialDateTime";
      this.checkBoxSerialDateTime.Size = new System.Drawing.Size(129, 17);
      this.checkBoxSerialDateTime.TabIndex = 13;
      this.checkBoxSerialDateTime.Text = "Allow Serial DateTime";
      this.checkBoxSerialDateTime.UseVisualStyleBackColor = true;
      // 
      // label32
      // 
      this.label32.AutoSize = true;
      this.label32.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label32.Location = new System.Drawing.Point(220, 173);
      this.label32.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      this.label32.Name = "label32";
      this.label32.Size = new System.Drawing.Size(502, 18);
      this.label32.TabIndex = 14;
      this.label32.Text = "Allow serial Date Time formats, used in Excel and OLE Automation";
      // 
      // checkBoxDetectGUID
      // 
      this.checkBoxDetectGUID.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBoxDetectGUID, 2);
      this.checkBoxDetectGUID.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DetectGUID", true));
      this.checkBoxDetectGUID.Location = new System.Drawing.Point(3, 266);
      this.checkBoxDetectGUID.Name = "checkBoxDetectGUID";
      this.checkBoxDetectGUID.Size = new System.Drawing.Size(53, 17);
      this.checkBoxDetectGUID.TabIndex = 23;
      this.checkBoxDetectGUID.Text = "GUID";
      this.checkBoxDetectGUID.UseVisualStyleBackColor = true;
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBox1, 2);
      this.checkBox1.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "IgnoreIdColums", true));
      this.checkBox1.Location = new System.Drawing.Point(3, 289);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(112, 17);
      this.checkBox1.TabIndex = 25;
      this.checkBox1.Text = "Ignore ID columns";
      this.checkBox1.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label1.Location = new System.Drawing.Point(220, 291);
      this.label1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(502, 27);
      this.label1.TabIndex = 26;
      this.label1.Text = "Columns names that end with Id, Ref or Text will be read as text even if seem to " +
    "contain a number";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label2.Location = new System.Drawing.Point(220, 268);
      this.label2.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(502, 18);
      this.label2.TabIndex = 24;
      this.label2.Text = "Detect GUIDs, GUID values cannot be filtered like text though";
      // 
      // label19
      // 
      this.label19.AutoSize = true;
      this.label19.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label19.Location = new System.Drawing.Point(220, 53);
      this.label19.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(502, 43);
      this.label19.TabIndex = 5;
      this.label19.Text = "Number of rows to check to get the sample values";
      // 
      // label20
      // 
      this.label20.AutoSize = true;
      this.label20.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label20.Location = new System.Drawing.Point(220, 5);
      this.label20.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(502, 43);
      this.label20.TabIndex = 2;
      this.label20.Text = "Number of different samples to check to determine the format / value type";
      // 
      // textBoxMinSamplesForIntDate
      // 
      this.textBoxMinSamplesForIntDate.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "MinSamplesForIntDate", true));
      this.textBoxMinSamplesForIntDate.Location = new System.Drawing.Point(166, 122);
      this.textBoxMinSamplesForIntDate.Name = "textBoxMinSamplesForIntDate";
      this.textBoxMinSamplesForIntDate.Size = new System.Drawing.Size(48, 20);
      this.textBoxMinSamplesForIntDate.TabIndex = 9;
      this.toolTip.SetToolTip(this.textBoxMinSamplesForIntDate, "A higher the number makes sure the guessed format is correct, but columns with th" +
        "at do not contain a variety of values might not recognized.");
      // 
      // checkBoxNamedDates
      // 
      this.checkBoxNamedDates.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBoxNamedDates, 2);
      this.checkBoxNamedDates.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "CheckNamedDates", true));
      this.checkBoxNamedDates.Location = new System.Drawing.Point(3, 148);
      this.checkBoxNamedDates.Name = "checkBoxNamedDates";
      this.checkBoxNamedDates.Size = new System.Drawing.Size(141, 17);
      this.checkBoxNamedDates.TabIndex = 11;
      this.checkBoxNamedDates.Text = "Named Month and Days";
      this.checkBoxNamedDates.UseVisualStyleBackColor = true;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label4.Location = new System.Drawing.Point(220, 150);
      this.label4.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(502, 18);
      this.label4.TabIndex = 12;
      this.label4.Text = "Check for named month && days  (this is a slow process)";
      // 
      // checkBoxDateParts
      // 
      this.checkBoxDateParts.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.checkBoxDateParts, 2);
      this.checkBoxDateParts.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DateParts", true));
      this.checkBoxDateParts.Location = new System.Drawing.Point(3, 194);
      this.checkBoxDateParts.Name = "checkBoxDateParts";
      this.checkBoxDateParts.Size = new System.Drawing.Size(157, 17);
      this.checkBoxDateParts.TabIndex = 15;
      this.checkBoxDateParts.Text = "Include Time and Timezone";
      this.checkBoxDateParts.UseVisualStyleBackColor = true;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label5.Location = new System.Drawing.Point(220, 196);
      this.label5.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(502, 18);
      this.label5.TabIndex = 16;
      this.label5.Text = "Find a separate Time and Time Zone for dates and add combine the information";
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.ColumnCount = 4;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.Controls.Add(this.textBoxSampleValues, 2, 0);
      this.tableLayoutPanel1.Controls.Add(this.trackBarCheckedRecords, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.label2, 3, 9);
      this.tableLayoutPanel1.Controls.Add(this.trackBarSampleValues, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.label1, 3, 10);
      this.tableLayoutPanel1.Controls.Add(this.label32, 3, 5);
      this.tableLayoutPanel1.Controls.Add(this.label5, 3, 6);
      this.tableLayoutPanel1.Controls.Add(this.checkBox1, 0, 10);
      this.tableLayoutPanel1.Controls.Add(this.label20, 3, 0);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDetectGUID, 0, 9);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDateParts, 0, 6);
      this.tableLayoutPanel1.Controls.Add(this.label19, 3, 1);
      this.tableLayoutPanel1.Controls.Add(this.label21, 3, 8);
      this.tableLayoutPanel1.Controls.Add(this.label30, 3, 7);
      this.tableLayoutPanel1.Controls.Add(this.textBoxMinSamplesForIntDate, 2, 3);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDectectNumbers, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.textBoxFalse, 2, 8);
      this.tableLayoutPanel1.Controls.Add(this.label22, 3, 2);
      this.tableLayoutPanel1.Controls.Add(this.textBoxTrue, 1, 8);
      this.tableLayoutPanel1.Controls.Add(this.textBoxCheckedRecords, 2, 1);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxNamedDates, 0, 4);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDetectDateTime, 0, 3);
      this.tableLayoutPanel1.Controls.Add(this.label23, 3, 3);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDetectBoolean, 0, 8);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxSerialDateTime, 0, 5);
      this.tableLayoutPanel1.Controls.Add(this.label4, 3, 4);
      this.tableLayoutPanel1.Controls.Add(this.checkBoxDectectPercentage, 0, 7);
      this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 11;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.Size = new System.Drawing.Size(725, 318);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // fillGuessSettingsBindingSource
      // 
      this.fillGuessSettingsBindingSource.DataSource = typeof(CsvTools.FillGuessSettings);
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
      this.Name = "FillGuessSettingEdit";
      this.Size = new System.Drawing.Size(725, 318);
      ((System.ComponentModel.ISupportInitialize)(this.trackBarCheckedRecords)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarSampleValues)).EndInit();
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.fillGuessSettingsBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.TrackBar trackBarSampleValues;
    private System.Windows.Forms.TrackBar trackBarCheckedRecords;
    private System.Windows.Forms.TextBox textBoxSampleValues;
    private System.Windows.Forms.TextBox textBoxCheckedRecords;
    private System.Windows.Forms.Label label19;
    private System.Windows.Forms.Label label20;
    private System.Windows.Forms.CheckBox checkBoxDectectNumbers;
    private System.Windows.Forms.Label label22;
    private System.Windows.Forms.CheckBox checkBoxDetectGUID;
    private System.Windows.Forms.CheckBox checkBoxDectectPercentage;
    private System.Windows.Forms.CheckBox checkBoxDetectDateTime;
    private System.Windows.Forms.Label label23;
    private System.Windows.Forms.Label label30;
    private System.Windows.Forms.CheckBox checkBoxSerialDateTime;
    private System.Windows.Forms.Label label32;
    private System.Windows.Forms.CheckBox checkBoxDetectBoolean;
    private System.Windows.Forms.TextBox textBoxTrue;
    private System.Windows.Forms.TextBox textBoxFalse;
    private System.Windows.Forms.BindingSource fillGuessSettingsBindingSource;
    private System.Windows.Forms.CheckBox checkBox1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label21;
    private System.Windows.Forms.TextBox textBoxMinSamplesForIntDate;
    private System.Windows.Forms.CheckBox checkBoxNamedDates;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.CheckBox checkBoxDateParts;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
  }
}
