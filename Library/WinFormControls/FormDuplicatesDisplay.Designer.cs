namespace CsvTools
{
  partial class FormDuplicatesDisplay
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      m_CancellationTokenSource?.Cancel();
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
      System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
      var dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      var dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      var resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDuplicatesDisplay));
      System.Windows.Forms.Label fieldSelectionLabel;
      duplicatesDetailControl = new DetailControl();
      comboBoxTargetField = new System.Windows.Forms.ComboBox();
      checkBoxIgnoreNull = new System.Windows.Forms.CheckBox();
      mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      fieldSelectionLabel = new System.Windows.Forms.Label();
      mainTableLayoutPanel.SuspendLayout();
      SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      mainTableLayoutPanel.ColumnCount = 3;
      mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      mainTableLayoutPanel.Controls.Add(duplicatesDetailControl, 0, 1);
      mainTableLayoutPanel.Controls.Add(fieldSelectionLabel, 0, 0);
      mainTableLayoutPanel.Controls.Add(comboBoxTargetField, 1, 0);
      mainTableLayoutPanel.Controls.Add(checkBoxIgnoreNull, 2, 0);
      mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      mainTableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      mainTableLayoutPanel.Name = "mainTableLayoutPanel";
      mainTableLayoutPanel.RowCount = 2;
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      mainTableLayoutPanel.Size = new System.Drawing.Size(637, 488);
      mainTableLayoutPanel.TabIndex = 10;
      // 
      // detailControl
      // 
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gainsboro;
      duplicatesDetailControl.AlternatingRowDefaultCellStyle = dataGridViewCellStyle1;
      mainTableLayoutPanel.SetColumnSpan(duplicatesDetailControl, 3);
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      duplicatesDetailControl.DefaultCellStyle = dataGridViewCellStyle2;
      duplicatesDetailControl.Dock = System.Windows.Forms.DockStyle.Fill;
      duplicatesDetailControl.FillGuessSettings = null;
      duplicatesDetailControl.FrozenColumns = 0;
      duplicatesDetailControl.Location = new System.Drawing.Point(4, 30);
      duplicatesDetailControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      duplicatesDetailControl.MenuDown = false;
      duplicatesDetailControl.Name = "duplicatesDetailControl";
      duplicatesDetailControl.SearchText = "";
      duplicatesDetailControl.ShowButtonAtLength = 1000;
      duplicatesDetailControl.Size = new System.Drawing.Size(629, 455);
      duplicatesDetailControl.TabIndex = 2;
      // 
      // label1
      // 
      fieldSelectionLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
      fieldSelectionLabel.AutoSize = true;
      fieldSelectionLabel.Location = new System.Drawing.Point(4, 7);
      fieldSelectionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      fieldSelectionLabel.Name = "fieldSelectionLabel";
      fieldSelectionLabel.Size = new System.Drawing.Size(29, 13);
      fieldSelectionLabel.TabIndex = 3;
      fieldSelectionLabel.Text = "Field";
      // 
      // comboBoxID
      // 
      comboBoxTargetField.Dock = System.Windows.Forms.DockStyle.Top;
      comboBoxTargetField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxTargetField.FormattingEnabled = true;
      comboBoxTargetField.Location = new System.Drawing.Point(41, 3);
      comboBoxTargetField.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      comboBoxTargetField.Name = "comboBoxTargetField";
      comboBoxTargetField.Size = new System.Drawing.Size(497, 21);
      comboBoxTargetField.TabIndex = 0;
      comboBoxTargetField.SelectedIndexChanged += ComboBoxTargetField_SelectedIndexChanged;
      // 
      // checkBoxIgnoreNull
      // 
      checkBoxIgnoreNull.Anchor = System.Windows.Forms.AnchorStyles.Left;
      checkBoxIgnoreNull.AutoSize = true;
      checkBoxIgnoreNull.Location = new System.Drawing.Point(546, 5);
      checkBoxIgnoreNull.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      checkBoxIgnoreNull.Name = "checkBoxIgnoreNull";
      checkBoxIgnoreNull.Size = new System.Drawing.Size(87, 17);
      checkBoxIgnoreNull.TabIndex = 1;
      checkBoxIgnoreNull.Text = "Ignore NULL";
      checkBoxIgnoreNull.UseVisualStyleBackColor = true;
      checkBoxIgnoreNull.CheckedChanged += CheckBoxIgnoreNull_CheckedChanged;
      // 
      // FormDuplicatesDisplay
      // 
      AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      ClientSize = new System.Drawing.Size(637, 488);
      Controls.Add(mainTableLayoutPanel);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      Name = "FormDuplicatesDisplay";
      Text = "Duplicates Display";
      FormClosing += FormDuplicatesDisplay_FormClosing;
      Load += FormDuplicatesDisplay_LoadAsync;
      mainTableLayoutPanel.ResumeLayout(false);
      mainTableLayoutPanel.PerformLayout();
      ResumeLayout(false);
    }

    private System.Windows.Forms.CheckBox checkBoxIgnoreNull;
    private System.Windows.Forms.ComboBox comboBoxTargetField;
    private CsvTools.DetailControl duplicatesDetailControl;

#endregion
  }
}