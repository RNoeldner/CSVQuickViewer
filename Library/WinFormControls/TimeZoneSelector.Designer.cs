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
      System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
      this.comboBoxTimeZoneID = new System.Windows.Forms.ComboBox();
      this.buttonLocalTZ = new System.Windows.Forms.Button();
      tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      tableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel
      // 
      tableLayoutPanel.ColumnCount = 2;
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel.Controls.Add(this.comboBoxTimeZoneID, 0, 0);
      tableLayoutPanel.Controls.Add(this.buttonLocalTZ, 1, 0);
      tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
      tableLayoutPanel.Name = "tableLayoutPanel";
      tableLayoutPanel.RowCount = 1;
      tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel.Size = new System.Drawing.Size(305, 29);
      tableLayoutPanel.TabIndex = 30;
      // 
      // comboBoxTimeZoneID
      // 
      this.comboBoxTimeZoneID.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxTimeZoneID.FormattingEnabled = true;
      this.comboBoxTimeZoneID.Location = new System.Drawing.Point(0, 1);
      this.comboBoxTimeZoneID.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
      this.comboBoxTimeZoneID.Name = "comboBoxTimeZoneID";
      this.comboBoxTimeZoneID.Size = new System.Drawing.Size(227, 21);
      this.comboBoxTimeZoneID.TabIndex = 28;
      this.comboBoxTimeZoneID.SelectedIndexChanged += new System.EventHandler(this.comboBoxTimeZoneID_SelectedIndexChanged);
      // 
      // buttonLocalTZ
      // 
      this.buttonLocalTZ.Location = new System.Drawing.Point(230, 0);
      this.buttonLocalTZ.Margin = new System.Windows.Forms.Padding(3, 0, 0, 0);
      this.buttonLocalTZ.Name = "buttonLocalTZ";
      this.buttonLocalTZ.Size = new System.Drawing.Size(75, 23);
      this.buttonLocalTZ.TabIndex = 29;
      this.buttonLocalTZ.Text = "Local";
      this.buttonLocalTZ.UseVisualStyleBackColor = true;
      this.buttonLocalTZ.Click += new System.EventHandler(this.buttonLocalTZ_Click);
      // 
      // TimeZoneSelector
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(tableLayoutPanel);
      this.Name = "TimeZoneSelector";
      this.Size = new System.Drawing.Size(305, 29);
      this.Load += new System.EventHandler(this.TimeZoneSelector_Load);
      tableLayoutPanel.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Button buttonLocalTZ;
    private System.Windows.Forms.ComboBox comboBoxTimeZoneID;
  }
}
