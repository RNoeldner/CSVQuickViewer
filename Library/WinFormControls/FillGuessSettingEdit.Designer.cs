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
      this.fillGuessSettingsBindingSource = new System.Windows.Forms.BindingSource(this.components);
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
      this.textBox1 = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.textBoxMinSamplesForIntDate = new System.Windows.Forms.TextBox();
      this.checkBoxNamedDates = new System.Windows.Forms.CheckBox();
      this.label4 = new System.Windows.Forms.Label();
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.checkBoxDateParts = new System.Windows.Forms.CheckBox();
      this.label5 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarCheckedRecords)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.fillGuessSettingsBindingSource)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarSampleValues)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      this.SuspendLayout();
      // 
      // trackBarCheckedRecords
      // 
      this.trackBarCheckedRecords.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.fillGuessSettingsBindingSource, "SampleValues", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.trackBarCheckedRecords.LargeChange = 50;
      this.trackBarCheckedRecords.Location = new System.Drawing.Point(6, 33);
      this.trackBarCheckedRecords.Maximum = 500;
      this.trackBarCheckedRecords.Name = "trackBarCheckedRecords";
      this.trackBarCheckedRecords.Size = new System.Drawing.Size(136, 45);
      this.trackBarCheckedRecords.SmallChange = 10;
      this.trackBarCheckedRecords.TabIndex = 96;
      this.trackBarCheckedRecords.TickFrequency = 50;
      this.trackBarCheckedRecords.Value = 250;
      // 
      // fillGuessSettingsBindingSource
      // 
      this.fillGuessSettingsBindingSource.DataSource = typeof(CsvTools.FillGuessSettings);
      // 
      // textBoxCheckedRecords
      // 
      this.textBoxCheckedRecords.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "SampleValues", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.textBoxCheckedRecords.Location = new System.Drawing.Point(148, 39);
      this.textBoxCheckedRecords.Name = "textBoxCheckedRecords";
      this.textBoxCheckedRecords.Size = new System.Drawing.Size(48, 20);
      this.textBoxCheckedRecords.TabIndex = 97;
      // 
      // trackBarSampleValues
      // 
      this.trackBarSampleValues.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.fillGuessSettingsBindingSource, "CheckedRecords", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.trackBarSampleValues.LargeChange = 2000;
      this.trackBarSampleValues.Location = new System.Drawing.Point(6, 0);
      this.trackBarSampleValues.Maximum = 26000;
      this.trackBarSampleValues.Name = "trackBarSampleValues";
      this.trackBarSampleValues.Size = new System.Drawing.Size(136, 45);
      this.trackBarSampleValues.SmallChange = 500;
      this.trackBarSampleValues.TabIndex = 96;
      this.trackBarSampleValues.TickFrequency = 2000;
      this.trackBarSampleValues.Value = 10000;
      // 
      // textBoxSampleValues
      // 
      this.textBoxSampleValues.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "CheckedRecords", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
      this.textBoxSampleValues.Location = new System.Drawing.Point(148, 3);
      this.textBoxSampleValues.Name = "textBoxSampleValues";
      this.textBoxSampleValues.Size = new System.Drawing.Size(48, 20);
      this.textBoxSampleValues.TabIndex = 97;
      // 
      // checkBoxDectectNumbers
      // 
      this.checkBoxDectectNumbers.AutoSize = true;
      this.checkBoxDectectNumbers.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DectectNumbers", true));
      this.checkBoxDectectNumbers.Location = new System.Drawing.Point(11, 73);
      this.checkBoxDectectNumbers.Name = "checkBoxDectectNumbers";
      this.checkBoxDectectNumbers.Size = new System.Drawing.Size(68, 17);
      this.checkBoxDectectNumbers.TabIndex = 99;
      this.checkBoxDectectNumbers.Text = "Numbers";
      this.toolTip.SetToolTip(this.checkBoxDectectNumbers, "Numbers with leading 0 will not be regarded as numbers to prevent information los" +
        "s");
      this.checkBoxDectectNumbers.UseVisualStyleBackColor = true;
      // 
      // label21
      // 
      this.label21.AutoSize = true;
      this.label21.Location = new System.Drawing.Point(209, 228);
      this.label21.Name = "label21";
      this.label21.Size = new System.Drawing.Size(490, 13);
      this.label21.TabIndex = 89;
      this.label21.Text = "Detect Boolean values like: Yes/No, True/False, 1/0.  You may add your own values" +
    " to the text boxes\r\n";
      // 
      // label22
      // 
      this.label22.AutoSize = true;
      this.label22.Location = new System.Drawing.Point(207, 74);
      this.label22.Name = "label22";
      this.label22.Size = new System.Drawing.Size(115, 13);
      this.label22.TabIndex = 89;
      this.label22.Text = "Detect Numeric values";
      // 
      // checkBoxDectectPercentage
      // 
      this.checkBoxDectectPercentage.AutoSize = true;
      this.checkBoxDectectPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DectectPercentage", true));
      this.checkBoxDectectPercentage.Location = new System.Drawing.Point(11, 204);
      this.checkBoxDectectPercentage.Name = "checkBoxDectectPercentage";
      this.checkBoxDectectPercentage.Size = new System.Drawing.Size(81, 17);
      this.checkBoxDectectPercentage.TabIndex = 99;
      this.checkBoxDectectPercentage.Text = "Percentage";
      this.checkBoxDectectPercentage.UseVisualStyleBackColor = true;
      // 
      // checkBoxDetectDateTime
      // 
      this.checkBoxDetectDateTime.AutoSize = true;
      this.checkBoxDetectDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DetectDateTime", true));
      this.checkBoxDetectDateTime.Location = new System.Drawing.Point(11, 95);
      this.checkBoxDetectDateTime.Name = "checkBoxDetectDateTime";
      this.checkBoxDetectDateTime.Size = new System.Drawing.Size(83, 17);
      this.checkBoxDetectDateTime.TabIndex = 99;
      this.checkBoxDetectDateTime.Text = "Date / Time";
      this.checkBoxDetectDateTime.UseVisualStyleBackColor = true;
      // 
      // label23
      // 
      this.label23.AutoSize = true;
      this.label23.Location = new System.Drawing.Point(207, 96);
      this.label23.Name = "label23";
      this.label23.Size = new System.Drawing.Size(268, 13);
      this.label23.TabIndex = 89;
      this.label23.Text = "Detect Date/Time values.  / Number of required values";
      // 
      // label30
      // 
      this.label30.AutoSize = true;
      this.label30.Location = new System.Drawing.Point(207, 205);
      this.label30.Name = "label30";
      this.label30.Size = new System.Drawing.Size(341, 13);
      this.label30.TabIndex = 89;
      this.label30.Text = "Detect Percentages and store them as numeric values (divided by 100)";
      // 
      // checkBoxDetectBoolean
      // 
      this.checkBoxDetectBoolean.AutoSize = true;
      this.checkBoxDetectBoolean.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DetectBoolean", true));
      this.checkBoxDetectBoolean.Location = new System.Drawing.Point(11, 227);
      this.checkBoxDetectBoolean.Name = "checkBoxDetectBoolean";
      this.checkBoxDetectBoolean.Size = new System.Drawing.Size(65, 17);
      this.checkBoxDetectBoolean.TabIndex = 98;
      this.checkBoxDetectBoolean.Text = "Boolean";
      this.checkBoxDetectBoolean.UseVisualStyleBackColor = true;
      // 
      // textBoxTrue
      // 
      this.textBoxTrue.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "TrueValue", true));
      this.textBoxTrue.Location = new System.Drawing.Point(96, 225);
      this.textBoxTrue.Name = "textBoxTrue";
      this.textBoxTrue.Size = new System.Drawing.Size(48, 20);
      this.textBoxTrue.TabIndex = 97;
      this.toolTip.SetToolTip(this.textBoxTrue, "Value(s) that should be regarded as TRUE, separated by ;");
      // 
      // textBoxFalse
      // 
      this.textBoxFalse.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "FalseValue", true));
      this.textBoxFalse.Location = new System.Drawing.Point(150, 225);
      this.textBoxFalse.Name = "textBoxFalse";
      this.textBoxFalse.Size = new System.Drawing.Size(48, 20);
      this.textBoxFalse.TabIndex = 97;
      this.toolTip.SetToolTip(this.textBoxFalse, "Value(s) that should be regarded as FALSE, separated by ;");
      // 
      // checkBoxSerialDateTime
      // 
      this.checkBoxSerialDateTime.AutoSize = true;
      this.checkBoxSerialDateTime.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "SerialDateTime", true));
      this.checkBoxSerialDateTime.Location = new System.Drawing.Point(11, 137);
      this.checkBoxSerialDateTime.Name = "checkBoxSerialDateTime";
      this.checkBoxSerialDateTime.Size = new System.Drawing.Size(129, 17);
      this.checkBoxSerialDateTime.TabIndex = 99;
      this.checkBoxSerialDateTime.Text = "Allow Serial DateTime";
      this.checkBoxSerialDateTime.UseVisualStyleBackColor = true;
      // 
      // label32
      // 
      this.label32.AutoSize = true;
      this.label32.Location = new System.Drawing.Point(207, 138);
      this.label32.Name = "label32";
      this.label32.Size = new System.Drawing.Size(318, 13);
      this.label32.TabIndex = 89;
      this.label32.Text = "Allow serial Date Time formats, used in Excel and OLE Automation";
      // 
      // checkBoxDetectGUID
      // 
      this.checkBoxDetectGUID.AutoSize = true;
      this.checkBoxDetectGUID.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DetectGUID", true));
      this.checkBoxDetectGUID.Location = new System.Drawing.Point(11, 250);
      this.checkBoxDetectGUID.Name = "checkBoxDetectGUID";
      this.checkBoxDetectGUID.Size = new System.Drawing.Size(53, 17);
      this.checkBoxDetectGUID.TabIndex = 99;
      this.checkBoxDetectGUID.Text = "GUID";
      this.checkBoxDetectGUID.UseVisualStyleBackColor = true;
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.checkBox1.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "IgnoreIdColums", true));
      this.checkBox1.Location = new System.Drawing.Point(11, 272);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(112, 17);
      this.checkBox1.TabIndex = 99;
      this.checkBox1.Text = "Ignore ID columns";
      this.checkBox1.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(207, 273);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(466, 13);
      this.label1.TabIndex = 89;
      this.label1.Text = "Columns names that end with Id, Ref or Text will be read as text even if seem to " +
    "contain a number";
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(207, 251);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(301, 13);
      this.label2.TabIndex = 101;
      this.label2.Text = "Detect GUIDs, GUID values cannot be filtered like text though";
      // 
      // label19
      // 
      this.label19.AutoSize = true;
      this.label19.Location = new System.Drawing.Point(207, 42);
      this.label19.Name = "label19";
      this.label19.Size = new System.Drawing.Size(244, 13);
      this.label19.TabIndex = 89;
      this.label19.Text = "Number of rows to check to get the sample values";
      // 
      // label20
      // 
      this.label20.AutoSize = true;
      this.label20.Location = new System.Drawing.Point(207, 6);
      this.label20.Name = "label20";
      this.label20.Size = new System.Drawing.Size(354, 13);
      this.label20.TabIndex = 89;
      this.label20.Text = "Number of different samples to check to determine the format / value type";
      // 
      // textBox1
      // 
      this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "DateTimeValue", true));
      this.textBox1.Location = new System.Drawing.Point(11, 155);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new System.Drawing.Size(185, 20);
      this.textBox1.TabIndex = 102;
      this.toolTip.SetToolTip(this.textBox1, global::CsvToolLib.Resources.TimeFomat);
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(207, 158);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(379, 13);
      this.label3.TabIndex = 89;
      this.label3.Text = "Additional date time format of your choosing, separated by ; e.G. \"M/d/y h:mm\"";
      // 
      // textBoxMinSamplesForIntDate
      // 
      this.textBoxMinSamplesForIntDate.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.fillGuessSettingsBindingSource, "MinSamplesForIntDate", true));
      this.textBoxMinSamplesForIntDate.Location = new System.Drawing.Point(150, 93);
      this.textBoxMinSamplesForIntDate.Name = "textBoxMinSamplesForIntDate";
      this.textBoxMinSamplesForIntDate.Size = new System.Drawing.Size(48, 20);
      this.textBoxMinSamplesForIntDate.TabIndex = 100;
      this.toolTip.SetToolTip(this.textBoxMinSamplesForIntDate, "A higher the number makes sure the guessed format is correct, but columns with th" +
        "at do not contain a variety of values might not recognized.");
      // 
      // checkBoxNamedDates
      // 
      this.checkBoxNamedDates.AutoSize = true;
      this.checkBoxNamedDates.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "CheckNamedDates", true));
      this.checkBoxNamedDates.Location = new System.Drawing.Point(11, 117);
      this.checkBoxNamedDates.Name = "checkBoxNamedDates";
      this.checkBoxNamedDates.Size = new System.Drawing.Size(141, 17);
      this.checkBoxNamedDates.TabIndex = 99;
      this.checkBoxNamedDates.Text = "Named Month and Days";
      this.checkBoxNamedDates.UseVisualStyleBackColor = true;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(207, 118);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(265, 13);
      this.label4.TabIndex = 89;
      this.label4.Text = "Check for named month && days  (this is a slow process)";
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // checkBoxDateParts
      // 
      this.checkBoxDateParts.AutoSize = true;
      this.checkBoxDateParts.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.fillGuessSettingsBindingSource, "DateParts", true));
      this.checkBoxDateParts.Location = new System.Drawing.Point(11, 181);
      this.checkBoxDateParts.Name = "checkBoxDateParts";
      this.checkBoxDateParts.Size = new System.Drawing.Size(157, 17);
      this.checkBoxDateParts.TabIndex = 103;
      this.checkBoxDateParts.Text = "Include Time and Timezone";
      this.checkBoxDateParts.UseVisualStyleBackColor = true;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(207, 182);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(382, 13);
      this.label5.TabIndex = 104;
      this.label5.Text = "Find a separate Time and Time Zone for dates and add combine the information";
      // 
      // FillGuessSettingEdit
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.label5);
      this.Controls.Add(this.checkBoxDateParts);
      this.Controls.Add(this.checkBoxDectectNumbers);
      this.Controls.Add(this.textBoxMinSamplesForIntDate);
      this.Controls.Add(this.label22);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label32);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.label21);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.checkBoxNamedDates);
      this.Controls.Add(this.checkBoxDetectDateTime);
      this.Controls.Add(this.textBoxFalse);
      this.Controls.Add(this.textBoxTrue);
      this.Controls.Add(this.label30);
      this.Controls.Add(this.checkBoxSerialDateTime);
      this.Controls.Add(this.checkBox1);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label23);
      this.Controls.Add(this.checkBoxDetectGUID);
      this.Controls.Add(this.label20);
      this.Controls.Add(this.label19);
      this.Controls.Add(this.checkBoxDetectBoolean);
      this.Controls.Add(this.textBoxCheckedRecords);
      this.Controls.Add(this.textBox1);
      this.Controls.Add(this.trackBarCheckedRecords);
      this.Controls.Add(this.textBoxSampleValues);
      this.Controls.Add(this.trackBarSampleValues);
      this.Controls.Add(this.checkBoxDectectPercentage);
      this.Name = "FillGuessSettingEdit";
      this.Size = new System.Drawing.Size(699, 295);
      ((System.ComponentModel.ISupportInitialize)(this.trackBarCheckedRecords)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.fillGuessSettingsBindingSource)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarSampleValues)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

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
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.TextBox textBoxMinSamplesForIntDate;
    private System.Windows.Forms.CheckBox checkBoxNamedDates;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.CheckBox checkBoxDateParts;
  }
}
