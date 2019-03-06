namespace CsvTools
{
  partial class DataGridViewColumnFilterControl
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
      m_DataGridViewColumnFilter.Dispose();
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
      this.textBoxValue = new System.Windows.Forms.TextBox();
      this.dateTimePickerValue = new System.Windows.Forms.DateTimePicker();
      this.comboBoxOperator = new System.Windows.Forms.ComboBox();
      this.lblCondition = new System.Windows.Forms.Label();
      this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
      this.SuspendLayout();
      // 
      // textBoxValue
      // 
      this.textBoxValue.Location = new System.Drawing.Point(105, 21);
      this.textBoxValue.Name = "textBoxValue";
      this.textBoxValue.Size = new System.Drawing.Size(116, 20);
      this.textBoxValue.TabIndex = 1;
      this.textBoxValue.Visible = false;
      this.textBoxValue.TextChanged += new System.EventHandler(this.FilterValueChanged);
      this.textBoxValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HandleEnterKeyPress);
      this.textBoxValue.Validated += new System.EventHandler(this.TextBoxValue_Validated);
      // 
      // dateTimePickerValue
      // 
      this.dateTimePickerValue.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.dateTimePickerValue.Location = new System.Drawing.Point(105, 21);
      this.dateTimePickerValue.Name = "dateTimePickerValue";
      this.dateTimePickerValue.Size = new System.Drawing.Size(116, 20);
      this.dateTimePickerValue.TabIndex = 9;
      this.dateTimePickerValue.Visible = false;
      this.dateTimePickerValue.ValueChanged += new System.EventHandler(this.FilterValueChanged);
      this.dateTimePickerValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HandleEnterKeyPress);
      // 
      // comboBoxOperator
      // 
      this.comboBoxOperator.Location = new System.Drawing.Point(5, 21);
      this.comboBoxOperator.Name = "comboBoxOperator";
      this.comboBoxOperator.Size = new System.Drawing.Size(97, 21);
      this.comboBoxOperator.TabIndex = 0;
      this.comboBoxOperator.SelectedIndexChanged += new System.EventHandler(this.ComboBoxOperator_SelectedIndexChanged);
      this.comboBoxOperator.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HandleEnterKeyPress);
      // 
      // lblCondition
      // 
      this.lblCondition.AutoSize = true;
      this.lblCondition.Location = new System.Drawing.Point(5, 5);
      this.lblCondition.Name = "lblCondition";
      this.lblCondition.Size = new System.Drawing.Size(76, 13);
      this.lblCondition.TabIndex = 12;
      this.lblCondition.Text = "Field Condition";
      // 
      // errorProvider1
      // 
      this.errorProvider1.ContainerControl = this;
      // 
      // DataGridViewColumnFilterControl
      // 
      this.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.Controls.Add(this.lblCondition);
      this.Controls.Add(this.textBoxValue);
      this.Controls.Add(this.dateTimePickerValue);
      this.Controls.Add(this.comboBoxOperator);
      this.Name = "DataGridViewColumnFilterControl";
      this.Size = new System.Drawing.Size(224, 49);
      this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HandleEnterKeyPress);
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox textBoxValue;
    private System.Windows.Forms.DateTimePicker dateTimePickerValue;
    private System.Windows.Forms.ComboBox comboBoxOperator;
    private System.Windows.Forms.Label lblCondition;
    private System.Windows.Forms.ErrorProvider errorProvider1;

  }
}