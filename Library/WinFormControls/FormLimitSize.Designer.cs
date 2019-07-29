namespace CsvTools
{
  partial class FrmLimitSize
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

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.Label label2;
      System.Windows.Forms.Label label4;
      this.labelCount2 = new System.Windows.Forms.Label();
      this.labelCount3 = new System.Windows.Forms.Label();
      this.buttonOK = new System.Windows.Forms.Button();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.trackBarLimit = new System.Windows.Forms.TrackBar();
      this.label5 = new System.Windows.Forms.Label();
      this.labelCount1 = new System.Windows.Forms.Label();
      this.labelCount4 = new System.Windows.Forms.Label();
      this.timer = new System.Windows.Forms.Timer(this.components);
      this.label = new System.Windows.Forms.Label();
      label2 = new System.Windows.Forms.Label();
      label4 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarLimit)).BeginInit();
      this.SuspendLayout();
      // 
      // label2
      // 
      label2.AutoSize = true;
      label2.Location = new System.Drawing.Point(9, 30);
      label2.Name = "label2";
      label2.Size = new System.Drawing.Size(305, 13);
      label2.TabIndex = 2;
      label2.Text = "Please move the slider to restrict the number of records to view.\r\n";
      // 
      // label4
      // 
      label4.AutoSize = true;
      label4.Location = new System.Drawing.Point(9, 9);
      label4.Name = "label4";
      label4.Size = new System.Drawing.Size(213, 13);
      label4.TabIndex = 1;
      label4.Text = "Display high amounts of data might be slow.";
      // 
      // labelCount2
      // 
      this.labelCount2.Location = new System.Drawing.Point(65, 90);
      this.labelCount2.Name = "labelCount2";
      this.labelCount2.Size = new System.Drawing.Size(52, 15);
      this.labelCount2.TabIndex = 5;
      this.labelCount2.Text = "20.000";
      this.labelCount2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelCount3
      // 
      this.labelCount3.Location = new System.Drawing.Point(129, 90);
      this.labelCount3.Name = "labelCount3";
      this.labelCount3.Size = new System.Drawing.Size(52, 15);
      this.labelCount3.TabIndex = 6;
      this.labelCount3.Text = "50.000";
      this.labelCount3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // buttonOK
      // 
      this.buttonOK.Location = new System.Drawing.Point(239, 112);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(75, 23);
      this.buttonOK.TabIndex = 9;
      this.buttonOK.Text = "&OK";
      this.buttonOK.UseVisualStyleBackColor = true;
      this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
      // 
      // buttonCancel
      // 
      this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.buttonCancel.Location = new System.Drawing.Point(156, 112);
      this.buttonCancel.Name = "buttonCancel";
      this.buttonCancel.Size = new System.Drawing.Size(75, 23);
      this.buttonCancel.TabIndex = 8;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // trackBarLimit
      // 
      this.trackBarLimit.LargeChange = 1;
      this.trackBarLimit.Location = new System.Drawing.Point(12, 60);
      this.trackBarLimit.Maximum = 5;
      this.trackBarLimit.Minimum = 1;
      this.trackBarLimit.Name = "trackBarLimit";
      this.trackBarLimit.Size = new System.Drawing.Size(285, 42);
      this.trackBarLimit.TabIndex = 2;
      this.trackBarLimit.Value = 3;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(274, 90);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(18, 13);
      this.label5.TabIndex = 7;
      this.label5.Text = "All";
      // 
      // labelCount1
      // 
      this.labelCount1.Location = new System.Drawing.Point(0, 90);
      this.labelCount1.Name = "labelCount1";
      this.labelCount1.Size = new System.Drawing.Size(52, 15);
      this.labelCount1.TabIndex = 4;
      this.labelCount1.Text = "10.000";
      this.labelCount1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelCount4
      // 
      this.labelCount4.Location = new System.Drawing.Point(193, 89);
      this.labelCount4.Name = "labelCount4";
      this.labelCount4.Size = new System.Drawing.Size(52, 15);
      this.labelCount4.TabIndex = 10;
      this.labelCount4.Text = "100.000";
      this.labelCount4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // timer
      // 
      this.timer.Enabled = true;
      this.timer.Interval = 500;
      this.timer.Tick += new System.EventHandler(this.Timer_Tick);
      // 
      // label
      // 
      this.label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label.AutoSize = true;
      this.label.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label.Location = new System.Drawing.Point(9, 117);
      this.label.Name = "label";
      this.label.Size = new System.Drawing.Size(104, 13);
      this.label.TabIndex = 11;
      this.label.Text = "Default in 5 seconds";
      // 
      // FrmLimitSize
      // 
      this.AcceptButton = this.buttonOK;
      
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(320, 141);
      this.Controls.Add(this.label);
      this.Controls.Add(this.labelCount4);
      this.Controls.Add(label2);
      this.Controls.Add(this.labelCount1);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.labelCount3);
      this.Controls.Add(this.labelCount2);
      this.Controls.Add(label4);
      this.Controls.Add(this.trackBarLimit);
      this.Controls.Add(this.buttonCancel);
      this.Controls.Add(this.buttonOK);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FrmLimitSize";
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Limit Records";
      this.TopMost = true;
      ((System.ComponentModel.ISupportInitialize)(this.trackBarLimit)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button buttonOK;
    private System.Windows.Forms.Button buttonCancel;
    private System.Windows.Forms.TrackBar trackBarLimit;
    private System.Windows.Forms.Label label5;
    private System.Windows.Forms.Label labelCount2;
    private System.Windows.Forms.Label labelCount3;
    private System.Windows.Forms.Label labelCount1;
    private System.Windows.Forms.Label labelCount4;
    private System.Windows.Forms.Timer timer;
    private System.Windows.Forms.Label label;
  }
}