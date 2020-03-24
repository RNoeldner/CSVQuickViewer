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
    private bool m_DisposedValue; // To detect redundant calls

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue)
        return;
      if (disposing)
      {
        m_DisposedValue = true;
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
      System.Windows.Forms.Label label1;
      System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      this.detailControl = new CsvTools.DetailControl();
      this.comboBoxID = new System.Windows.Forms.ComboBox();
      this.checkBoxIgnoreNull = new System.Windows.Forms.CheckBox();
      label1 = new System.Windows.Forms.Label();
      tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // label1
      // 
      label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
      label1.AutoSize = true;
      label1.Location = new System.Drawing.Point(4, 8);
      label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(43, 20);
      label1.TabIndex = 3;
      label1.Text = "Field";
      // 
      // tableLayoutPanel1
      // 
      tableLayoutPanel1.ColumnCount = 3;
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel1.Controls.Add(this.detailControl, 0, 1);
      tableLayoutPanel1.Controls.Add(label1, 0, 0);
      tableLayoutPanel1.Controls.Add(this.comboBoxID, 1, 0);
      tableLayoutPanel1.Controls.Add(this.checkBoxIgnoreNull, 2, 0);
      tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
      tableLayoutPanel1.Name = "tableLayoutPanel1";
      tableLayoutPanel1.RowCount = 2;
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel1.Size = new System.Drawing.Size(724, 726);
      tableLayoutPanel1.TabIndex = 10;
      // 
      // detailControl
      // 
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.Gainsboro;
      this.detailControl.AlternatingRowDefaultCellStyle = dataGridViewCellStyle1;
      tableLayoutPanel1.SetColumnSpan(this.detailControl, 3);
      this.detailControl.DataTable = null;
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.detailControl.DefaultCellStyle = dataGridViewCellStyle2;
      this.detailControl.Dock = System.Windows.Forms.DockStyle.Fill;
      this.detailControl.Location = new System.Drawing.Point(4, 40);
      this.detailControl.Margin = new System.Windows.Forms.Padding(4);
      this.detailControl.Name = "detailControl";
      this.detailControl.ReadOnly = true;
      this.detailControl.ShowFilter = false;
      this.detailControl.ShowInfoButtons = false;
      this.detailControl.Size = new System.Drawing.Size(716, 682);
      this.detailControl.TabIndex = 2;
      // 
      // comboBoxID
      // 
      this.comboBoxID.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxID.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBoxID.FormattingEnabled = true;
      this.comboBoxID.Location = new System.Drawing.Point(55, 4);
      this.comboBoxID.Margin = new System.Windows.Forms.Padding(4);
      this.comboBoxID.Name = "comboBoxID";
      this.comboBoxID.Size = new System.Drawing.Size(531, 28);
      this.comboBoxID.TabIndex = 0;
      this.comboBoxID.SelectedIndexChanged += new System.EventHandler(this.ComboBoxID_SelectedIndexChanged);
      // 
      // checkBoxIgnoreNull
      // 
      this.checkBoxIgnoreNull.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.checkBoxIgnoreNull.AutoSize = true;
      this.checkBoxIgnoreNull.Location = new System.Drawing.Point(594, 6);
      this.checkBoxIgnoreNull.Margin = new System.Windows.Forms.Padding(4);
      this.checkBoxIgnoreNull.Name = "checkBoxIgnoreNull";
      this.checkBoxIgnoreNull.Size = new System.Drawing.Size(126, 24);
      this.checkBoxIgnoreNull.TabIndex = 1;
      this.checkBoxIgnoreNull.Text = "Ignore NULL";
      this.checkBoxIgnoreNull.UseVisualStyleBackColor = true;
      this.checkBoxIgnoreNull.CheckedChanged += new System.EventHandler(this.ComboBoxID_SelectedIndexChanged);
      // 
      // FormUniqueDisplay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(724, 726);
      this.Controls.Add(tableLayoutPanel1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "FormUniqueDisplay";
      this.Text = "Unique Values Display";
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UniqueDisplay_FormClosing);
      this.Load += new System.EventHandler(this.UniqueDisplay_Load);
      tableLayoutPanel1.ResumeLayout(false);
      tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.ComboBox comboBoxID;
    private DetailControl detailControl;
    private System.Windows.Forms.CheckBox checkBoxIgnoreNull;

  }
}