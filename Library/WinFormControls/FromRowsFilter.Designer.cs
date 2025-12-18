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
      try { m_CancellationTokenSource.Cancel(); } catch { /* ignore */ }
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      if (disposing)
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
      components = new System.ComponentModel.Container();
      System.Windows.Forms.ColumnHeader colItems;
      colText = new System.Windows.Forms.ColumnHeader();
      errorProvider = new System.Windows.Forms.ErrorProvider(components);
      buttonFilter = new System.Windows.Forms.Button();
      lblCondition = new System.Windows.Forms.Label();
      comboBoxOperator = new System.Windows.Forms.ComboBox();
      dateTimePickerValue = new System.Windows.Forms.DateTimePicker();
      textBoxValue = new System.Windows.Forms.TextBox();
      listViewCluster = new System.Windows.Forms.ListView();
      panelTop = new System.Windows.Forms.Panel();
      radioButtonEven = new System.Windows.Forms.RadioButton();
      radioButtonCombine = new System.Windows.Forms.RadioButton();
      radioButtonReg = new System.Windows.Forms.RadioButton();
      toolTip = new System.Windows.Forms.ToolTip(components);
      timerFilter = new System.Windows.Forms.Timer(components);
      labelError = new System.Windows.Forms.Label();
      timerRebuild = new System.Windows.Forms.Timer(components);
      colItems = new System.Windows.Forms.ColumnHeader();
      ((System.ComponentModel.ISupportInitialize) errorProvider).BeginInit();
      panelTop.SuspendLayout();
      SuspendLayout();
      // 
      // colItems
      // 
      colItems.Text = "Count";
      colItems.Width = 50;
      // 
      // colText
      // 
      colText.Text = "Filter";
      colText.Width = 200;
      // 
      // errorProvider
      // 
      errorProvider.ContainerControl = this;
      // 
      // buttonFilter
      // 
      buttonFilter.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      buttonFilter.AutoSize = true;
      buttonFilter.DialogResult = System.Windows.Forms.DialogResult.OK;
      buttonFilter.Location = new System.Drawing.Point(598, 2);
      buttonFilter.Name = "buttonFilter";
      buttonFilter.Size = new System.Drawing.Size(87, 25);
      buttonFilter.TabIndex = 3;
      buttonFilter.Text = "&Apply Filter";
      toolTip.SetToolTip(buttonFilter, "Apply the filter for the column");
      buttonFilter.UseVisualStyleBackColor = true;
      buttonFilter.Click += ButtonFilter_Click;
      // 
      // lblCondition
      // 
      lblCondition.AutoSize = true;
      lblCondition.BackColor = System.Drawing.Color.Transparent;
      lblCondition.Location = new System.Drawing.Point(10, 7);
      lblCondition.Margin = new System.Windows.Forms.Padding(3);
      lblCondition.Name = "lblCondition";
      lblCondition.Size = new System.Drawing.Size(51, 13);
      lblCondition.TabIndex = 0;
      lblCondition.Text = "Condition";
      // 
      // comboBoxOperator
      // 
      comboBoxOperator.Location = new System.Drawing.Point(67, 3);
      comboBoxOperator.Name = "comboBoxOperator";
      comboBoxOperator.Size = new System.Drawing.Size(103, 21);
      comboBoxOperator.TabIndex = 2;
      toolTip.SetToolTip(comboBoxOperator, "Operator for comparison");
      comboBoxOperator.SelectedIndexChanged += ComboBoxOperator_SelectedIndexChanged;
      // 
      // dateTimePickerValue
      // 
      dateTimePickerValue.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      dateTimePickerValue.Location = new System.Drawing.Point(176, 4);
      dateTimePickerValue.Name = "dateTimePickerValue";
      dateTimePickerValue.Size = new System.Drawing.Size(129, 20);
      dateTimePickerValue.TabIndex = 0;
      toolTip.SetToolTip(dateTimePickerValue, "Date for Filter");
      dateTimePickerValue.Visible = false;
      // 
      // textBoxValue
      // 
      textBoxValue.Location = new System.Drawing.Point(176, 4);
      textBoxValue.Name = "textBoxValue";
      textBoxValue.Size = new System.Drawing.Size(188, 20);
      textBoxValue.TabIndex = 1;
      toolTip.SetToolTip(textBoxValue, "Text to filter.  Please use decimal point for numbers");
      textBoxValue.Visible = false;
      textBoxValue.TextChanged += TextBoxValue_TextChanged;
      // 
      // listViewCluster
      // 
      listViewCluster.CheckBoxes = true;
      listViewCluster.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { colText, colItems });
      listViewCluster.Dock = System.Windows.Forms.DockStyle.Fill;
      listViewCluster.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      listViewCluster.HideSelection = false;
      listViewCluster.Location = new System.Drawing.Point(0, 30);
      listViewCluster.Name = "listViewCluster";
      listViewCluster.ShowGroups = false;
      listViewCluster.Size = new System.Drawing.Size(689, 352);
      listViewCluster.TabIndex = 1;
      toolTip.SetToolTip(listViewCluster, "Check allowed values. Count is based on filtered records.");
      listViewCluster.UseCompatibleStateImageBehavior = false;
      listViewCluster.View = System.Windows.Forms.View.Details;
      listViewCluster.KeyUp += ListViewCluster_KeyUp;
      // 
      // panelTop
      // 
      panelTop.AutoSize = true;
      panelTop.Controls.Add(radioButtonEven);
      panelTop.Controls.Add(radioButtonCombine);
      panelTop.Controls.Add(radioButtonReg);
      panelTop.Controls.Add(textBoxValue);
      panelTop.Controls.Add(dateTimePickerValue);
      panelTop.Controls.Add(buttonFilter);
      panelTop.Controls.Add(comboBoxOperator);
      panelTop.Controls.Add(lblCondition);
      panelTop.Dock = System.Windows.Forms.DockStyle.Top;
      panelTop.Location = new System.Drawing.Point(0, 0);
      panelTop.Name = "panelTop";
      panelTop.Size = new System.Drawing.Size(689, 30);
      panelTop.TabIndex = 0;
      panelTop.Resize += PanelTop_Resize;
      // 
      // radioButtonEven
      // 
      radioButtonEven.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      radioButtonEven.AutoSize = true;
      radioButtonEven.Location = new System.Drawing.Point(510, 5);
      radioButtonEven.Name = "radioButtonEven";
      radioButtonEven.Size = new System.Drawing.Size(82, 17);
      radioButtonEven.TabIndex = 4;
      radioButtonEven.Text = "By Numbers";
      toolTip.SetToolTip(radioButtonEven, "Adjust border resulting in clusters of comparable number of entries, only available with numbers and dates");
      radioButtonEven.UseVisualStyleBackColor = true;
      radioButtonEven.CheckedChanged += ClusterTypeChanged;
      // 
      // radioButtonCombine
      // 
      radioButtonCombine.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      radioButtonCombine.AutoSize = true;
      radioButtonCombine.Location = new System.Drawing.Point(438, 5);
      radioButtonCombine.Name = "radioButtonCombine";
      radioButtonCombine.Size = new System.Drawing.Size(66, 17);
      radioButtonCombine.TabIndex = 4;
      radioButtonCombine.Text = "Combine";
      toolTip.SetToolTip(radioButtonCombine, "Combine close clusters that do not have many records, only available with numbers and dates");
      radioButtonCombine.UseVisualStyleBackColor = true;
      radioButtonCombine.CheckedChanged += ClusterTypeChanged;
      // 
      // radioButtonReg
      // 
      radioButtonReg.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
      radioButtonReg.AutoSize = true;
      radioButtonReg.Checked = true;
      radioButtonReg.Location = new System.Drawing.Point(370, 5);
      radioButtonReg.Name = "radioButtonReg";
      radioButtonReg.Size = new System.Drawing.Size(62, 17);
      radioButtonReg.TabIndex = 4;
      radioButtonReg.TabStop = true;
      radioButtonReg.Text = "Regular";
      toolTip.SetToolTip(radioButtonReg, "Separate all values into clusters of even ranges, the number of entries may vary a lot, only available with numbers and dates");
      radioButtonReg.UseVisualStyleBackColor = true;
      radioButtonReg.CheckedChanged += ClusterTypeChanged;
      // 
      // timerFilter
      // 
      timerFilter.Enabled = true;
      timerFilter.Interval = 200;
      timerFilter.Tick += TimerFilter_Tick;
      // 
      // labelError
      // 
      labelError.Anchor =  System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
      labelError.AutoSize = true;
      labelError.BackColor = System.Drawing.SystemColors.Info;
      labelError.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      labelError.ForeColor = System.Drawing.SystemColors.InfoText;
      labelError.Location = new System.Drawing.Point(25, 90);
      labelError.Name = "labelError";
      labelError.Padding = new System.Windows.Forms.Padding(5);
      labelError.Size = new System.Drawing.Size(96, 25);
      labelError.TabIndex = 2;
      labelError.Text = "Error Information";
      labelError.Visible = false;
      // 
      // timerRebuild
      // 
      timerRebuild.Enabled = true;
      timerRebuild.Interval = 200;
      timerRebuild.Tick += timerRebuild_Tick;
      // 
      // FromRowsFilter
      // 
      AcceptButton = buttonFilter;
      AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
      BackColor = System.Drawing.SystemColors.Control;
      ClientSize = new System.Drawing.Size(689, 382);
      Controls.Add(labelError);
      Controls.Add(listViewCluster);
      Controls.Add(panelTop);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      MaximizeBox = false;
      MinimizeBox = false;
      MinimumSize = new System.Drawing.Size(705, 260);
      Name = "FromRowsFilter";
      Text = "Filter";
      Activated += FromDataGridViewFilter_Activated;
      FormClosing += FromRowsFilter_FormClosing;
      Load += FromDataGridViewFilter_Load;
      Resize += FromDataGridViewFilter_Resize;
      ((System.ComponentModel.ISupportInitialize) errorProvider).EndInit();
      panelTop.ResumeLayout(false);
      panelTop.PerformLayout();
      ResumeLayout(false);
      PerformLayout();

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