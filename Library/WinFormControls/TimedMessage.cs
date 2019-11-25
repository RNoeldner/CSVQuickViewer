using System;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  public class TimedMessage : Form
  {
    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Button button3;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private Label label;
    private int m_Counter = 0;
    private MessageBoxButtons m_MessageBoxButtons = MessageBoxButtons.OKCancel;
    private PictureBox pictureBox;
    private RichTextBox richTextBox;
    private TableLayoutPanel tableLayoutPanel;
    private ImageList imageList;
    private Timer timer;

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimedMessage));
      this.button1 = new System.Windows.Forms.Button();
      this.richTextBox = new System.Windows.Forms.RichTextBox();
      this.label = new System.Windows.Forms.Label();
      this.timer = new System.Windows.Forms.Timer(this.components);
      this.button2 = new System.Windows.Forms.Button();
      this.button3 = new System.Windows.Forms.Button();
      this.pictureBox = new System.Windows.Forms.PictureBox();
      this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.imageList = new System.Windows.Forms.ImageList(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
      this.tableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button1.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.button1.Location = new System.Drawing.Point(371, 126);
      this.button1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(89, 27);
      this.button1.TabIndex = 0;
      this.button1.Text = "button1";
      this.button1.UseVisualStyleBackColor = false;
      this.button1.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.button1.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // richTextBox
      // 
      this.richTextBox.BackColor = System.Drawing.SystemColors.Control;
      this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.tableLayoutPanel.SetColumnSpan(this.richTextBox, 4);
      this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.richTextBox.Location = new System.Drawing.Point(65, 4);
      this.richTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.richTextBox.Name = "richTextBox";
      this.richTextBox.ReadOnly = true;
      this.richTextBox.Size = new System.Drawing.Size(584, 115);
      this.richTextBox.TabIndex = 3;
      this.richTextBox.Text = "";
      this.richTextBox.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.richTextBox.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // label
      // 
      this.label.BackColor = System.Drawing.Color.Transparent;
      this.tableLayoutPanel.SetColumnSpan(this.label, 2);
      this.label.Dock = System.Windows.Forms.DockStyle.Fill;
      this.label.ForeColor = System.Drawing.SystemColors.InfoText;
      this.label.Location = new System.Drawing.Point(8, 123);
      this.label.Name = "label";
      this.label.Size = new System.Drawing.Size(357, 32);
      this.label.TabIndex = 2;
      this.label.Text = "Default in 5 seconds";
      this.label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // timer
      // 
      this.timer.Enabled = true;
      this.timer.Interval = 500;
      this.timer.Tick += new System.EventHandler(this.Timer_Tick);
      // 
      // button2
      // 
      this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button2.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button2.Location = new System.Drawing.Point(466, 126);
      this.button2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(89, 27);
      this.button2.TabIndex = 1;
      this.button2.Text = "button2";
      this.button2.UseVisualStyleBackColor = false;
      this.button2.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.button2.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // button3
      // 
      this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button3.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.button3.Location = new System.Drawing.Point(561, 126);
      this.button3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(89, 27);
      this.button3.TabIndex = 2;
      this.button3.Text = "button3";
      this.button3.UseVisualStyleBackColor = false;
      this.button3.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.button3.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // pictureBox
      // 
      this.pictureBox.ErrorImage = null;
      this.pictureBox.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox.Image")));
      this.pictureBox.InitialImage = null;
      this.pictureBox.Location = new System.Drawing.Point(9, 3);
      this.pictureBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.pictureBox.Name = "pictureBox";
      this.pictureBox.Size = new System.Drawing.Size(48, 48);
      this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.pictureBox.TabIndex = 4;
      this.pictureBox.TabStop = false;
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.AutoSize = true;
      this.tableLayoutPanel.BackColor = System.Drawing.Color.Transparent;
      this.tableLayoutPanel.ColumnCount = 5;
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
      this.tableLayoutPanel.Controls.Add(this.label, 0, 1);
      this.tableLayoutPanel.Controls.Add(this.button3, 4, 1);
      this.tableLayoutPanel.Controls.Add(this.button2, 3, 1);
      this.tableLayoutPanel.Controls.Add(this.richTextBox, 1, 0);
      this.tableLayoutPanel.Controls.Add(this.button1, 2, 1);
      this.tableLayoutPanel.Controls.Add(this.pictureBox, 0, 0);
      this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.Padding = new System.Windows.Forms.Padding(5, 0, 13, 4);
      this.tableLayoutPanel.RowCount = 2;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel.Size = new System.Drawing.Size(666, 159);
      this.tableLayoutPanel.TabIndex = 5;
      // 
      // imageList
      // 
      this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
      this.imageList.TransparentColor = System.Drawing.Color.Transparent;
      this.imageList.Images.SetKeyName(0, "Info-icon.bmp");
      this.imageList.Images.SetKeyName(1, "icon-warning.bmp");
      this.imageList.Images.SetKeyName(2, "icon-question.bmp");
      this.imageList.Images.SetKeyName(3, "error-icon.bmp");
      // 
      // TimedMessage
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.ClientSize = new System.Drawing.Size(666, 159);
      this.Controls.Add(this.tableLayoutPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(340, 79);
      this.Name = "TimedMessage";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Timed Message";
      this.TopMost = true;
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
      this.tableLayoutPanel.ResumeLayout(false);
      this.tableLayoutPanel.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion Windows Form Designer generated code

    public TimedMessage() => InitializeComponent();

    public double Duration { get; set; } = 4.0;

    public string Message
    {
      get => richTextBox.Text;
      set => richTextBox.Text = value;
    }

    public string MessageRtf
    {
      get => richTextBox.Rtf;
      set => richTextBox.Rtf = value;
    }

    public DialogResult Show(Form owner, string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, double timeout, string button1Text, string button2Text, string button3Text)
    {
      Text = title;
      Message = message;
      Duration = timeout;

      m_MessageBoxButtons = buttons;

      // One Button
      if (buttons == MessageBoxButtons.OK)
      {
        HideColumn(3, false);
        HideColumn(4, false);
        button2.Visible = false;
        button3.Visible = false;
      }

      // Two Button
      if (buttons == MessageBoxButtons.YesNo ||
          buttons == MessageBoxButtons.OKCancel ||
          buttons == MessageBoxButtons.RetryCancel)
      {
        HideColumn(4, false);
        button3.Visible = false;
      }

      if ((buttons == MessageBoxButtons.OK || buttons == MessageBoxButtons.OKCancel))
      {
        button1.Text = "&OK";
        button1.DialogResult = DialogResult.OK;
      }
      if ((buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel))
      {
        button1.Text = "&Yes";
        button1.DialogResult = DialogResult.Yes;
      }
      if ((buttons == MessageBoxButtons.AbortRetryIgnore))
      {
        button1.Text = "&Abort";
        button1.DialogResult = DialogResult.Abort;
      }
      if ((buttons == MessageBoxButtons.RetryCancel))
      {
        button1.Text = "&Retry";
        button1.DialogResult = DialogResult.Retry;
      }

      // Button 2
      if ((buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel))
      {
        button2.Text = "&No";
        button2.DialogResult = DialogResult.No;
      }
      if ((buttons == MessageBoxButtons.AbortRetryIgnore))
      {
        button2.Text = "&Retry";
        button2.DialogResult = DialogResult.Retry;
      }
      if ((buttons == MessageBoxButtons.RetryCancel || buttons == MessageBoxButtons.OKCancel))
      {
        button2.Text = "&Cancel";
        button2.DialogResult = DialogResult.Cancel;
        CancelButton = button2;
      }

      // Button 3
      if (buttons == MessageBoxButtons.AbortRetryIgnore)
      {
        button3.Text = "&Ignore";
        button3.DialogResult = DialogResult.Ignore;
      }
      if (buttons == MessageBoxButtons.YesNoCancel)
      {
        button3.Text = "&Cancel";
        button3.DialogResult = DialogResult.Cancel;
        CancelButton = button3;
      }

      if (!string.IsNullOrEmpty(button1Text))
        button1.Text = button1Text;

      if (!string.IsNullOrEmpty(button2Text))
        button2.Text = button2Text;

      if (!string.IsNullOrEmpty(button3Text))
        button3.Text = button3Text;

      if (defaultButton == MessageBoxDefaultButton.Button1)
      {
        AcceptButton = button1;
      }
      else if (defaultButton == MessageBoxDefaultButton.Button2)
      {
        AcceptButton = button2;
      }
      else if (defaultButton == MessageBoxDefaultButton.Button3)
      {
        AcceptButton = button3;
      }

      if (icon != MessageBoxIcon.None)
      {
        switch (icon)
        {
          case MessageBoxIcon.Error:
            pictureBox.Image = imageList.Images[3];
            break;
          case MessageBoxIcon.Information:
            pictureBox.Image = imageList.Images[0];
            break;
          case MessageBoxIcon.Warning:
            pictureBox.Image = imageList.Images[1];
            break;
          case MessageBoxIcon.Question:
            pictureBox.Image = imageList.Images[2];
            break;
          default:
            break;
        }
      }

      return ShowDialog(owner);
    }

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

    private void HideColumn(int colNumber, bool visible)
    {
      var styles = tableLayoutPanel.ColumnStyles;
      if (visible)
      {
        styles[colNumber].SizeType = SizeType.AutoSize;
      }
      else
      {
        styles[colNumber].SizeType = SizeType.Absolute;
        styles[colNumber].Width = 0;
      }
    }

    private void MouseEnterElement(object sender, EventArgs e)
    {
      timer.Enabled = false;
      UpdateLabel();
    }

    private void MouseLeaveElement(object sender, EventArgs e)
    {
      timer.Enabled = true;
      UpdateLabel();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
      m_Counter++;
      UpdateLabel();

      if (Duration > 0 && (m_Counter * timer.Interval) / 1000 > Duration)
      {
        AcceptButton.PerformClick();
      }
    }

    private void UpdateLabel()
    {
      var displ = Convert.ToInt32((Duration - (m_Counter * timer.Interval) / 1000 + .75));
      if (!timer.Enabled)
        displ = 0;
      if (displ > 0)
      {
        if (AcceptButton == button1)
          label.Text = $"{button1.Text.Substring(1)} in {displ:N0} seconds";
        if (AcceptButton == button2)
          label.Text = $"{button2.Text.Substring(1)} in {displ:N0} seconds";
        if (AcceptButton == button3)
          label.Text = $"{button3.Text.Substring(1)} in {displ:N0} seconds";
      }
      else
        label.Text = string.Empty;
    }
  }

#pragma warning disable CA1707 // Identifiers should not contain underscores
}