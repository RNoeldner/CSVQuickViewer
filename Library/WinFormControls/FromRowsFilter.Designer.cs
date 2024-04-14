using System.Diagnostics.CodeAnalysis;

namespace CsvTools
{
  sealed partial class FromRowsFilter
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
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
      System.Windows.Forms.ColumnHeader colItems;
      this.colText = ((System.Windows.Forms.ColumnHeader) (new System.Windows.Forms.ColumnHeader()));
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.buttonFilter = new System.Windows.Forms.Button();
      this.lblCondition = new System.Windows.Forms.Label();
      this.comboBoxOperator = new System.Windows.Forms.ComboBox();
      this.dateTimePickerValue = new System.Windows.Forms.DateTimePicker();
      this.textBoxValue = new System.Windows.Forms.TextBox();
      this.listViewCluster = new System.Windows.Forms.ListView();
      this.panelTop = new System.Windows.Forms.Panel();
      this.radioButtonEven = new System.Windows.Forms.RadioButton();
      this.radioButtonCombine = new System.Windows.Forms.RadioButton();
      this.radioButtonReg = new System.Windows.Forms.RadioButton();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.timerFilter = new System.Windows.Forms.Timer(this.components);
      this.labelError = new System.Windows.Forms.Label();
      this.timerRebuild = new System.Windows.Forms.Timer(this.components);
      colItems = ((System.Windows.Forms.ColumnHeader) (new System.Windows.Forms.ColumnHeader()));
      ((System.ComponentModel.ISupportInitialize) (this.errorProvider)).BeginInit();
      this.panelTop.SuspendLayout();
      this.SuspendLayout();
      // 
      // colItems
      // 
      colItems.Text = "Count";
      colItems.Width = 50;
      // 
      // colText
      // 
      this.colText.Text = "Filter";
      this.colText.Width = 200;
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // buttonFilter
      // 
      this.buttonFilter.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonFilter.AutoSize = true;
      this.buttonFilter.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonFilter.Location = new System.Drawing.Point(598, 2);
      this.buttonFilter.Name = "buttonFilter";
      this.buttonFilter.Size = new System.Drawing.Size(87, 25);
      this.buttonFilter.TabIndex = 3;
      this.buttonFilter.Text = "&Apply Filter";
      this.toolTip.SetToolTip(this.buttonFilter, "Apply the filter for the column");
      this.buttonFilter.UseVisualStyleBackColor = true;
      this.buttonFilter.Click += new System.EventHandler(this.ButtonFilter_Click);
      // 
      // lblCondition
      // 
      this.lblCondition.AutoSize = true;
      this.lblCondition.BackColor = System.Drawing.Color.Transparent;
      this.lblCondition.Location = new System.Drawing.Point(10, 7);
      this.lblCondition.Margin = new System.Windows.Forms.Padding(3);
      this.lblCondition.Name = "lblCondition";
      this.lblCondition.Size = new System.Drawing.Size(51, 13);
      this.lblCondition.TabIndex = 0;
      this.lblCondition.Text = "Condition";
      // 
      // comboBoxOperator
      // 
      this.comboBoxOperator.Location = new System.Drawing.Point(67, 3);
      this.comboBoxOperator.Name = "comboBoxOperator";
      this.comboBoxOperator.Size = new System.Drawing.Size(103, 21);
      this.comboBoxOperator.TabIndex = 2;
      this.toolTip.SetToolTip(this.comboBoxOperator, "Operator for comparison");
      this.comboBoxOperator.SelectedIndexChanged += new System.EventHandler(this.ComboBoxOperator_SelectedIndexChanged);
      // 
      // dateTimePickerValue
      // 
      this.dateTimePickerValue.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.dateTimePickerValue.Location = new System.Drawing.Point(176, 4);
      this.dateTimePickerValue.Name = "dateTimePickerValue";
      this.dateTimePickerValue.Size = new System.Drawing.Size(129, 20);
      this.dateTimePickerValue.TabIndex = 0;
      this.toolTip.SetToolTip(this.dateTimePickerValue, "Date for Filter");
      this.dateTimePickerValue.Visible = false;
      // 
      // textBoxValue
      // 
      this.textBoxValue.Location = new System.Drawing.Point(176, 4);
      this.textBoxValue.Name = "textBoxValue";
      this.textBoxValue.Size = new System.Drawing.Size(188, 20);
      this.textBoxValue.TabIndex = 1;
      this.toolTip.SetToolTip(this.textBoxValue, "Text to filter.  Please use decimal point for numbers");
      this.textBoxValue.Visible = false;
      this.textBoxValue.TextChanged += new System.EventHandler(this.TextBoxValue_TextChanged);
      // 
      // listViewCluster
      // 
      this.listViewCluster.CheckBoxes = true;
      this.listViewCluster.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colText,
            colItems});
      this.listViewCluster.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listViewCluster.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.listViewCluster.HideSelection = false;
      this.listViewCluster.Location = new System.Drawing.Point(0, 30);
      this.listViewCluster.Name = "listViewCluster";
      this.listViewCluster.ShowGroups = false;
      this.listViewCluster.Size = new System.Drawing.Size(689, 352);
      this.listViewCluster.TabIndex = 1;
      this.toolTip.SetToolTip(this.listViewCluster, "Check allowed values. Count is based on filtered records.");
      this.listViewCluster.UseCompatibleStateImageBehavior = false;
      this.listViewCluster.View = System.Windows.Forms.View.Details;
      this.listViewCluster.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ListViewCluster_KeyUp);
      // 
      // panelTop
      // 
      this.panelTop.AutoSize = true;
      this.panelTop.Controls.Add(this.radioButtonEven);
      this.panelTop.Controls.Add(this.radioButtonCombine);
      this.panelTop.Controls.Add(this.radioButtonReg);
      this.panelTop.Controls.Add(this.textBoxValue);
      this.panelTop.Controls.Add(this.dateTimePickerValue);
      this.panelTop.Controls.Add(this.buttonFilter);
      this.panelTop.Controls.Add(this.comboBoxOperator);
      this.panelTop.Controls.Add(this.lblCondition);
      this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.panelTop.Location = new System.Drawing.Point(0, 0);
      this.panelTop.Name = "panelTop";
      this.panelTop.Size = new System.Drawing.Size(689, 30);
      this.panelTop.TabIndex = 0;
      this.panelTop.Resize += new System.EventHandler(this.PanelTop_Resize);
      // 
      // radioButtonEven
      // 
      this.radioButtonEven.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.radioButtonEven.AutoSize = true;
      this.radioButtonEven.Location = new System.Drawing.Point(510, 5);
      this.radioButtonEven.Name = "radioButtonEven";
      this.radioButtonEven.Size = new System.Drawing.Size(82, 17);
      this.radioButtonEven.TabIndex = 4;
      this.radioButtonEven.Text = "By Numbers";
      this.toolTip.SetToolTip(this.radioButtonEven, "Adjust border resulting in clusters of comparable number of entries, only availab" +
        "le with numbers and dates");
      this.radioButtonEven.UseVisualStyleBackColor = true;
      this.radioButtonEven.CheckedChanged += new System.EventHandler(this.ClusterTypeChanged);
      // 
      // radioButtonCombine
      // 
      this.radioButtonCombine.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.radioButtonCombine.AutoSize = true;
      this.radioButtonCombine.Location = new System.Drawing.Point(438, 5);
      this.radioButtonCombine.Name = "radioButtonCombine";
      this.radioButtonCombine.Size = new System.Drawing.Size(66, 17);
      this.radioButtonCombine.TabIndex = 4;
      this.radioButtonCombine.Text = "Combine";
      this.toolTip.SetToolTip(this.radioButtonCombine, "Combine close clusters that do not have many records, only available with numbers" +
        " and dates");
      this.radioButtonCombine.UseVisualStyleBackColor = true;
      this.radioButtonCombine.CheckedChanged += new System.EventHandler(this.ClusterTypeChanged);
      // 
      // radioButtonReg
      // 
      this.radioButtonReg.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.radioButtonReg.AutoSize = true;
      this.radioButtonReg.Checked = true;
      this.radioButtonReg.Location = new System.Drawing.Point(370, 5);
      this.radioButtonReg.Name = "radioButtonReg";
      this.radioButtonReg.Size = new System.Drawing.Size(62, 17);
      this.radioButtonReg.TabIndex = 4;
      this.radioButtonReg.TabStop = true;
      this.radioButtonReg.Text = "Regular";
      this.toolTip.SetToolTip(this.radioButtonReg, "Separate all values into clusters of even ranges, the number of entries may vary " +
        "a lot, only available with numbers and dates");
      this.radioButtonReg.UseVisualStyleBackColor = true;
      this.radioButtonReg.CheckedChanged += new System.EventHandler(this.ClusterTypeChanged);
      // 
      // timerFilter
      // 
      this.timerFilter.Enabled = true;
      this.timerFilter.Interval = 200;
      this.timerFilter.Tick += new System.EventHandler(this.TimerFilter_Tick);
      // 
      // labelError
      // 
      this.labelError.Anchor = ((System.Windows.Forms.AnchorStyles) ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
      this.labelError.AutoSize = true;
      this.labelError.BackColor = System.Drawing.SystemColors.Info;
      this.labelError.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.labelError.ForeColor = System.Drawing.SystemColors.InfoText;
      this.labelError.Location = new System.Drawing.Point(25, 90);
      this.labelError.Margin = new System.Windows.Forms.Padding(0);
      this.labelError.Name = "labelError";
      this.labelError.Padding = new System.Windows.Forms.Padding(5);
      this.labelError.Size = new System.Drawing.Size(96, 25);
      this.labelError.TabIndex = 2;
      this.labelError.Text = "Error Information";
      this.labelError.Visible = false;
      // 
      // timerRebuild
      // 
      this.timerRebuild.Enabled = true;
      this.timerRebuild.Interval = 200;
      this.timerRebuild.Tick += new System.EventHandler(this.timerRebuild_Tick);
      // 
      // FromRowsFilter
      // 
      this.AcceptButton = this.buttonFilter;
      this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.ClientSize = new System.Drawing.Size(689, 382);
      this.Controls.Add(this.labelError);
      this.Controls.Add(this.listViewCluster);
      this.Controls.Add(this.panelTop);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(705, 260);
      this.Name = "FromRowsFilter";
      this.Text = "Filter";
      this.Activated += new System.EventHandler(this.FromDataGridViewFilter_Activated);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FromRowsFilter_FormClosing);
      this.Load += new System.EventHandler(this.FromDataGridViewFilter_Load);
      this.Resize += new System.EventHandler(this.FromDataGridViewFilter_Resize);
      ((System.ComponentModel.ISupportInitialize) (this.errorProvider)).EndInit();
      this.panelTop.ResumeLayout(false);
      this.panelTop.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion Windows Form Designer generated code
    private System.Windows.Forms.ErrorProvider errorProvider;
    private System.Windows.Forms.Button buttonFilter;
    private System.Windows.Forms.Label lblCondition;
    private System.Windows.Forms.ComboBox comboBoxOperator;
    private System.Windows.Forms.DateTimePicker dateTimePickerValue;
    private System.Windows.Forms.TextBox textBoxValue;
    private System.Windows.Forms.ListView listViewCluster;
    private System.Windows.Forms.Panel panelTop;
    private System.Windows.Forms.ToolTip toolTip;
    private System.Windows.Forms.Timer timerFilter;
    private System.Windows.Forms.Label labelError;
    private System.Windows.Forms.RadioButton radioButtonEven;
    private System.Windows.Forms.RadioButton radioButtonCombine;
    private System.Windows.Forms.RadioButton radioButtonReg;
    private System.Windows.Forms.Timer timerRebuild;
    private System.Windows.Forms.ColumnHeader colText;
  }
}