using System.Diagnostics.CodeAnalysis;

namespace CsvTools
{
  sealed partial class FromDataGridViewFilter
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
      System.Windows.Forms.ColumnHeader colText;
      System.Windows.Forms.ColumnHeader colItems;
      this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
      this.buttonFilter = new System.Windows.Forms.Button();
      this.lblCondition = new System.Windows.Forms.Label();
      this.comboBoxOperator = new System.Windows.Forms.ComboBox();
      this.dateTimePickerValue = new System.Windows.Forms.DateTimePicker();
      this.textBoxValue = new System.Windows.Forms.TextBox();
      this.listViewCluster = new System.Windows.Forms.ListView();
      this.panelTop = new System.Windows.Forms.Panel();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      colText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      colItems = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
      this.panelTop.SuspendLayout();
      this.SuspendLayout();
      // 
      // colText
      // 
      colText.Text = "Filter";
      colText.Width = 90;
      // 
      // colItems
      // 
      colItems.Text = "Count";
      // 
      // errorProvider
      // 
      this.errorProvider.ContainerControl = this;
      // 
      // buttonFilter
      // 
      this.buttonFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonFilter.AutoSize = true;
      this.buttonFilter.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.buttonFilter.Location = new System.Drawing.Point(483, 2);
      this.buttonFilter.Name = "buttonFilter";
      this.buttonFilter.Size = new System.Drawing.Size(87, 23);
      this.buttonFilter.TabIndex = 1;
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
      this.comboBoxOperator.TabIndex = 1;
      this.toolTip.SetToolTip(this.comboBoxOperator, "Operator for comparsion");
      this.comboBoxOperator.SelectedIndexChanged += new System.EventHandler(this.ComboBoxOperator_SelectedIndexChanged);
      // 
      // dateTimePickerValue
      // 
      this.dateTimePickerValue.Format = System.Windows.Forms.DateTimePickerFormat.Short;
      this.dateTimePickerValue.Location = new System.Drawing.Point(185, 3);
      this.dateTimePickerValue.Name = "dateTimePickerValue";
      this.dateTimePickerValue.Size = new System.Drawing.Size(129, 20);
      this.dateTimePickerValue.TabIndex = 0;
      this.toolTip.SetToolTip(this.dateTimePickerValue, "Date for Filter");
      this.dateTimePickerValue.Visible = false;
      // 
      // textBoxValue
      // 
      this.textBoxValue.Location = new System.Drawing.Point(176, 3);
      this.textBoxValue.Name = "textBoxValue";
      this.textBoxValue.Size = new System.Drawing.Size(250, 20);
      this.textBoxValue.TabIndex = 1;
      this.toolTip.SetToolTip(this.textBoxValue, "Text to filter.  Please use decimal point for numbers");
      this.textBoxValue.Visible = false;
      this.textBoxValue.TextChanged += new System.EventHandler(this.textBoxValue_TextChanged);
      // 
      // listViewCluster
      // 
      this.listViewCluster.CheckBoxes = true;
      this.listViewCluster.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            colText,
            colItems});
      this.listViewCluster.Dock = System.Windows.Forms.DockStyle.Fill;
      this.listViewCluster.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.listViewCluster.HideSelection = false;
      this.listViewCluster.Location = new System.Drawing.Point(0, 28);
      this.listViewCluster.Name = "listViewCluster";
      this.listViewCluster.ShowGroups = false;
      this.listViewCluster.Size = new System.Drawing.Size(574, 380);
      this.listViewCluster.TabIndex = 22;
      this.toolTip.SetToolTip(this.listViewCluster, "Check allowed values. Count is based on filtered records.");
      this.listViewCluster.UseCompatibleStateImageBehavior = false;
      this.listViewCluster.View = System.Windows.Forms.View.Details;
      this.listViewCluster.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ListViewCluster_KeyUp);
      // 
      // panelTop
      // 
      this.panelTop.AutoSize = true;
      this.panelTop.Controls.Add(this.textBoxValue);
      this.panelTop.Controls.Add(this.dateTimePickerValue);
      this.panelTop.Controls.Add(this.buttonFilter);
      this.panelTop.Controls.Add(this.comboBoxOperator);
      this.panelTop.Controls.Add(this.lblCondition);
      this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
      this.panelTop.Location = new System.Drawing.Point(0, 0);
      this.panelTop.Name = "panelTop";
      this.panelTop.Size = new System.Drawing.Size(574, 28);
      this.panelTop.TabIndex = 25;
      this.panelTop.Resize += new System.EventHandler(this.PanelTop_Resize);
      // 
      // FromDataGridViewFilter
      // 
      this.AcceptButton = this.buttonFilter;
      this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.ClientSize = new System.Drawing.Size(574, 408);
      this.Controls.Add(this.listViewCluster);
      this.Controls.Add(this.panelTop);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(551, 260);
      this.Name = "FromDataGridViewFilter";
      this.Text = "Filter";
      this.Load += new System.EventHandler(this.FromDataGridViewFilter_Load);
      this.Resize += new System.EventHandler(this.FromDataGridViewFilter_Resize);
      ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
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
  }
}