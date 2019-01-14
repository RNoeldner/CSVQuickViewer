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
      System.Windows.Forms.Label label1;
      System.Windows.Forms.Label label2;
      System.Windows.Forms.Label label3;
      System.Windows.Forms.Label label4;
      System.Windows.Forms.Label label6;
      System.Windows.Forms.Label label7;
      this.buttonOK = new System.Windows.Forms.Button();
      this.buttonCancel = new System.Windows.Forms.Button();
      this.trackBarLimit = new System.Windows.Forms.TrackBar();
      this.label5 = new System.Windows.Forms.Label();
      label1 = new System.Windows.Forms.Label();
      label2 = new System.Windows.Forms.Label();
      label3 = new System.Windows.Forms.Label();
      label4 = new System.Windows.Forms.Label();
      label6 = new System.Windows.Forms.Label();
      label7 = new System.Windows.Forms.Label();
      ((System.ComponentModel.ISupportInitialize)(this.trackBarLimit)).BeginInit();
      this.SuspendLayout();
      // 
      // label1
      // 
      label1.AutoSize = true;
      label1.Location = new System.Drawing.Point(9, 9);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(210, 13);
      label1.TabIndex = 3;
      label1.Text = "The size of the file is larger than 20 MByte. ";
      // 
      // label2
      // 
      label2.AutoSize = true;
      label2.Location = new System.Drawing.Point(9, 45);
      label2.Name = "label2";
      label2.Size = new System.Drawing.Size(305, 13);
      label2.TabIndex = 3;
      label2.Text = "Please move the slider to restrict the number of records to view.\r\n";
      // 
      // label3
      // 
      label3.AutoSize = true;
      label3.Location = new System.Drawing.Point(93, 90);
      label3.Name = "label3";
      label3.Size = new System.Drawing.Size(37, 13);
      label3.TabIndex = 3;
      label3.Text = "10000";
      // 
      // label4
      // 
      label4.AutoSize = true;
      label4.Location = new System.Drawing.Point(9, 25);
      label4.Name = "label4";
      label4.Size = new System.Drawing.Size(222, 13);
      label4.TabIndex = 3;
      label4.Text = "Processing this amount of data might be slow.";
      // 
      // label6
      // 
      label6.AutoSize = true;
      label6.Location = new System.Drawing.Point(11, 90);
      label6.Name = "label6";
      label6.Size = new System.Drawing.Size(31, 13);
      label6.TabIndex = 3;
      label6.Text = "5000";
      // 
      // label7
      // 
      label7.AutoSize = true;
      label7.Location = new System.Drawing.Point(179, 90);
      label7.Name = "label7";
      label7.Size = new System.Drawing.Size(37, 13);
      label7.TabIndex = 3;
      label7.Text = "50000";
      // 
      // buttonOK
      // 
      this.buttonOK.Location = new System.Drawing.Point(239, 112);
      this.buttonOK.Name = "buttonOK";
      this.buttonOK.Size = new System.Drawing.Size(75, 23);
      this.buttonOK.TabIndex = 2;
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
      this.buttonCancel.TabIndex = 1;
      this.buttonCancel.Text = "&Cancel";
      this.buttonCancel.UseVisualStyleBackColor = true;
      this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
      // 
      // trackBarLimit
      // 
      this.trackBarLimit.LargeChange = 1;
      this.trackBarLimit.Location = new System.Drawing.Point(12, 63);
      this.trackBarLimit.Maximum = 4;
      this.trackBarLimit.Minimum = 1;
      this.trackBarLimit.Name = "trackBarLimit";
      this.trackBarLimit.Size = new System.Drawing.Size(285, 42);
      this.trackBarLimit.TabIndex = 0;
      this.trackBarLimit.Value = 4;
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(274, 90);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(18, 13);
      this.label5.TabIndex = 3;
      this.label5.Text = "All";
      // 
      // FrmLimitSize
      // 
      this.AcceptButton = this.buttonOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.buttonCancel;
      this.ClientSize = new System.Drawing.Size(320, 141);
      this.Controls.Add(label2);
      this.Controls.Add(label6);
      this.Controls.Add(this.label5);
      this.Controls.Add(label7);
      this.Controls.Add(label3);
      this.Controls.Add(label4);
      this.Controls.Add(label1);
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
  }
}