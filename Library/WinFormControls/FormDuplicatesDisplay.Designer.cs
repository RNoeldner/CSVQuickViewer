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
      System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
      var dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      var resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDuplicatesDisplay));
      var dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.Label label1;
      detailControl = new DetailControl();
      comboBoxID = new System.Windows.Forms.ComboBox();
      checkBoxIgnoreNull = new System.Windows.Forms.CheckBox();
      tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      label1 = new System.Windows.Forms.Label();
      tableLayoutPanel1.SuspendLayout();
      SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      tableLayoutPanel1.ColumnCount = 3;
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.Controls.Add(detailControl, 0, 1);
      tableLayoutPanel1.Controls.Add(label1, 0, 0);
      tableLayoutPanel1.Controls.Add(comboBoxID, 1, 0);
      tableLayoutPanel1.Controls.Add(checkBoxIgnoreNull, 2, 0);
      tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      tableLayoutPanel1.Name = "tableLayoutPanel1";
      tableLayoutPanel1.RowCount = 2;
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel1.Size = new System.Drawing.Size(637, 488);
      tableLayoutPanel1.TabIndex = 10;
      // 
      // detailControl
      // 
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gainsboro;
      detailControl.AlternatingRowDefaultCellStyle = dataGridViewCellStyle1;
      tableLayoutPanel1.SetColumnSpan(detailControl, 3);
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      detailControl.DefaultCellStyle = dataGridViewCellStyle2;
      detailControl.Dock = System.Windows.Forms.DockStyle.Fill;
      detailControl.Location = new System.Drawing.Point(4, 30);
      detailControl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      detailControl.MenuDown = false;
      detailControl.Name = "detailControl";
      detailControl.ShowButtonAtLength = 1000;
      detailControl.Size = new System.Drawing.Size(629, 455);
      detailControl.TabIndex = 2;
      // 
      // label1
      // 
      label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
      label1.AutoSize = true;
      label1.Location = new System.Drawing.Point(4, 7);
      label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(29, 13);
      label1.TabIndex = 3;
      label1.Text = "Field";
      // 
      // comboBoxID
      // 
      comboBoxID.Dock = System.Windows.Forms.DockStyle.Top;
      comboBoxID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      comboBoxID.FormattingEnabled = true;
      comboBoxID.Location = new System.Drawing.Point(41, 3);
      comboBoxID.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      comboBoxID.Name = "comboBoxID";
      comboBoxID.Size = new System.Drawing.Size(497, 21);
      comboBoxID.TabIndex = 0;
      comboBoxID.SelectedIndexChanged += ComboBoxID_SelectedIndexChanged;
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
      checkBoxIgnoreNull.CheckedChanged += ComboBoxID_SelectedIndexChanged;
      // 
      // FormDuplicatesDisplay
      // 
      ClientSize = new System.Drawing.Size(637, 488);
      Controls.Add(tableLayoutPanel1);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      Name = "FormDuplicatesDisplay";
      Text = "Duplicates Display";
      FormClosing += DuplicatesDisplay_FormClosing;
      Load += DuplicatesDisplay_LoadAsync;
      tableLayoutPanel1.ResumeLayout(false);
      tableLayoutPanel1.PerformLayout();
      ResumeLayout(false);
    }

    private System.Windows.Forms.CheckBox checkBoxIgnoreNull;
    private System.Windows.Forms.ComboBox comboBoxID;
    private CsvTools.DetailControl detailControl;

#endregion
  }
}