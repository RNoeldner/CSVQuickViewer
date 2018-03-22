namespace CsvTools
{
  partial class TimedMessage
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
      this.button1 = new System.Windows.Forms.Button();
      this.richTextBox = new System.Windows.Forms.RichTextBox();
      this.label = new System.Windows.Forms.Label();
      this.timer = new System.Windows.Forms.Timer(this.components);
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.button1.Location = new System.Drawing.Point(317, 89);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "&OK";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // richTextBox
      // 
      this.richTextBox.BackColor = System.Drawing.SystemColors.Control;
      this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.richTextBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.richTextBox.Location = new System.Drawing.Point(0, 0);
      this.richTextBox.Name = "richTextBox";
      this.richTextBox.Size = new System.Drawing.Size(394, 88);
      this.richTextBox.TabIndex = 1;
      this.richTextBox.Text = "";
      // 
      // label
      // 
      this.label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label.AutoSize = true;
      this.label.Location = new System.Drawing.Point(12, 94);
      this.label.Name = "label";
      this.label.Size = new System.Drawing.Size(104, 13);
      this.label.TabIndex = 2;
      this.label.Text = "Closing in 5 seconds";
      // 
      // timer
      // 
      this.timer.Interval = 500;
      this.timer.Tick += new System.EventHandler(this.timer_Tick);
      // 
      // TimedMessage
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(394, 114);
      this.Controls.Add(this.label);
      this.Controls.Add(this.richTextBox);
      this.Controls.Add(this.button1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "TimedMessage";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Timed Message";
      this.TopMost = true;
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.RichTextBox richTextBox;
    private System.Windows.Forms.Label label;
    private System.Windows.Forms.Timer timer;
  }
}