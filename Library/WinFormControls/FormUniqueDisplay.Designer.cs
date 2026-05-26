namespace CsvTools
{
  /// <summary>
  /// Windows Form showing the Unique values
  /// </summary>
  partial class FormUniqueDisplay
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
      if (disposing)
      {
        components?.Dispose();
        m_CancellationTokenSource?.Dispose();
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
      System.Windows.Forms.Label fieldSelectionLabel;
      System.Windows.Forms.TableLayoutPanel columnConfigTableLayoutPanel;
      var dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      var resources = new System.ComponentModel.ComponentResourceManager(typeof(FormUniqueDisplay));
      var dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      uniqueValuesDetailControl = new DetailControl();
      comboBoxTargetField = new System.Windows.Forms.ComboBox();
      checkBoxIgnoreNull = new System.Windows.Forms.CheckBox();
      fieldSelectionLabel = new System.Windows.Forms.Label();
      columnConfigTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      columnConfigTableLayoutPanel.SuspendLayout();
      SuspendLayout();
      // 
      // label1
      // 
      fieldSelectionLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
      fieldSelectionLabel.AutoSize = true;
      fieldSelectionLabel.Location = new System.Drawing.Point(4, 8);
      fieldSelectionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      fieldSelectionLabel.Name = "fieldSelectionLabel";
      fieldSelectionLabel.Size = new System.Drawing.Size(29, 13);
      fieldSelectionLabel.TabIndex = 3;
      fieldSelectionLabel.Text = "Field";
      // 
      // tableLayoutPanel1
      // 
      columnConfigTableLayoutPanel.ColumnCount = 3;
      columnConfigTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      columnConfigTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      columnConfigTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      columnConfigTableLayoutPanel.Controls.Add(uniqueValuesDetailControl, 0, 1);
      columnConfigTableLayoutPanel.Controls.Add(fieldSelectionLabel, 0, 0);
      columnConfigTableLayoutPanel.Controls.Add(comboBoxTargetField, 1, 0);
      columnConfigTableLayoutPanel.Controls.Add(checkBoxIgnoreNull, 2, 0);
      columnConfigTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      columnConfigTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      columnConfigTableLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
      columnConfigTableLayoutPanel.Name = "columnConfigTableLayoutPanel";
      columnConfigTableLayoutPanel.RowCount = 2;
      columnConfigTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      columnConfigTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      columnConfigTableLayoutPanel.Size = new System.Drawing.Size(724, 350);
      columnConfigTableLayoutPanel.TabIndex = 10;
      // 
      // detailControl
      // 
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gainsboro;
      uniqueValuesDetailControl.AlternatingRowDefaultCellStyle = dataGridViewCellStyle1;
      columnConfigTableLayoutPanel.SetColumnSpan(uniqueValuesDetailControl, 3);
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      uniqueValuesDetailControl.DefaultCellStyle = dataGridViewCellStyle2;
      uniqueValuesDetailControl.Dock = System.Windows.Forms.DockStyle.Fill;
      uniqueValuesDetailControl.Location = new System.Drawing.Point(4, 33);
      uniqueValuesDetailControl.Margin = new System.Windows.Forms.Padding(4);
      uniqueValuesDetailControl.MenuDown = false;
      uniqueValuesDetailControl.Name = "duplicatesDetailControl";
      uniqueValuesDetailControl.ShowButtonAtLength = 1000;
      uniqueValuesDetailControl.Size = new System.Drawing.Size(716, 313);
      uniqueValuesDetailControl.TabIndex = 2;
      // 
      // comboBoxID
      // 
      comboBoxTargetField.Dock = System.Windows.Forms.DockStyle.Top;
      comboBoxTargetField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxTargetField.FormattingEnabled = true;
      comboBoxTargetField.Location = new System.Drawing.Point(41, 4);
      comboBoxTargetField.Margin = new System.Windows.Forms.Padding(4);
      comboBoxTargetField.Name = "comboBoxTargetField";
      comboBoxTargetField.Size = new System.Drawing.Size(584, 21);
      comboBoxTargetField.TabIndex = 0;
      comboBoxTargetField.SelectedIndexChanged += ComboBoxTargetField_SelectedIndexChanged;
      // 
      // checkBoxIgnoreNull
      // 
      checkBoxIgnoreNull.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxIgnoreNull.AutoSize = true;
      checkBoxIgnoreNull.Location = new System.Drawing.Point(633, 6);
      checkBoxIgnoreNull.Margin = new System.Windows.Forms.Padding(4);
      checkBoxIgnoreNull.Name = "checkBoxIgnoreNull";
      checkBoxIgnoreNull.Size = new System.Drawing.Size(87, 17);
      checkBoxIgnoreNull.TabIndex = 1;
      checkBoxIgnoreNull.Text = "Ignore NULL";
      checkBoxIgnoreNull.UseVisualStyleBackColor = true;
      checkBoxIgnoreNull.CheckedChanged += ComboBoxTargetField_SelectedIndexChanged;
      // 
      // FormUniqueDisplay
      // 
      ClientSize = new System.Drawing.Size(724, 350);
      Controls.Add(columnConfigTableLayoutPanel);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      Margin = new System.Windows.Forms.Padding(4);
      Name = "FormUniqueDisplay";
      Text = "Unique Values Display";
      FormClosing += FormUniqueDisplay_FormClosing;
      Load += FormUniqueDisplay_LoadAsync;
      columnConfigTableLayoutPanel.ResumeLayout(false);
      columnConfigTableLayoutPanel.PerformLayout();
      ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox comboBoxTargetField;
    private DetailControl uniqueValuesDetailControl;
    private System.Windows.Forms.CheckBox checkBoxIgnoreNull;

  }
}