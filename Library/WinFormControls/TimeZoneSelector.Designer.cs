namespace CsvTools
{
  partial class TimeZoneSelector
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
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.comboBoxTimeZoneID = new System.Windows.Forms.ComboBox();
      this.buttonLocalTZ = new System.Windows.Forms.Button();
      this.tableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.AutoSize = true;
      this.tableLayoutPanel.ColumnCount = 2;
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 84F));
      this.tableLayoutPanel.Controls.Add(this.comboBoxTimeZoneID, 0, 0);
      this.tableLayoutPanel.Controls.Add(this.buttonLocalTZ, 1, 0);
      this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.RowCount = 1;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel.Size = new System.Drawing.Size(435, 33);
      this.tableLayoutPanel.TabIndex = 30;
      // 
      // comboBoxTimeZoneID
      // 
      this.comboBoxTimeZoneID.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.comboBoxTimeZoneID.FormattingEnabled = true;
      this.comboBoxTimeZoneID.Location = new System.Drawing.Point(3, 2);
      this.comboBoxTimeZoneID.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.comboBoxTimeZoneID.Name = "comboBoxTimeZoneID";
      this.comboBoxTimeZoneID.Size = new System.Drawing.Size(340, 28);
      this.comboBoxTimeZoneID.TabIndex = 28;
      this.comboBoxTimeZoneID.SelectedIndexChanged += new System.EventHandler(this.ComboBoxTimeZoneID_SelectedIndexChanged);
      // 
      // buttonLocalTZ
      // 
      this.buttonLocalTZ.Location = new System.Drawing.Point(354, 2);
      this.buttonLocalTZ.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.buttonLocalTZ.Name = "buttonLocalTZ";
      this.buttonLocalTZ.Size = new System.Drawing.Size(78, 29);
      this.buttonLocalTZ.TabIndex = 29;
      this.buttonLocalTZ.Text = "Local";
      this.buttonLocalTZ.UseVisualStyleBackColor = true;
      this.buttonLocalTZ.Click += new System.EventHandler(this.ButtonLocalTZ_Click);
      // 
      // TimeZoneSelector
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.tableLayoutPanel);
      this.Margin = new System.Windows.Forms.Padding(0);
      this.MinimumSize = new System.Drawing.Size(432, 34);
      this.Name = "TimeZoneSelector";
      this.Size = new System.Drawing.Size(435, 37);
      this.Load += new System.EventHandler(this.TimeZoneSelector_Load);
      this.tableLayoutPanel.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonLocalTZ;
    private System.Windows.Forms.ComboBox comboBoxTimeZoneID;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
  }
}
