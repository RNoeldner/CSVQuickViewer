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

    private int counter = 0;
    private Label label;
    private MessageBoxDefaultButton m_DefaultButton = MessageBoxDefaultButton.Button1;
    private double m_Duration = 4.0;
    private MessageBoxButtons m_MessageBoxButtons = MessageBoxButtons.OKCancel;
    private MessageBoxIcon m_MessageBoxIcon = MessageBoxIcon.None;
    private RichTextBox richTextBox;
    private Timer timer;

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
      this.button2 = new System.Windows.Forms.Button();
      this.button3 = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // button1
      // 
      this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button1.Location = new System.Drawing.Point(205, 127);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(75, 23);
      this.button1.TabIndex = 0;
      this.button1.Text = "button1";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new System.EventHandler(this.button1_Click);
      this.button1.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.button1.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // richTextBox
      // 
      this.richTextBox.BackColor = System.Drawing.SystemColors.Control;
      this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.richTextBox.Location = new System.Drawing.Point(45, 8);
      this.richTextBox.Name = "richTextBox";
      this.richTextBox.ReadOnly = true;
      this.richTextBox.Size = new System.Drawing.Size(335, 88);
      this.richTextBox.TabIndex = 3;
      this.richTextBox.Text = "";
      this.richTextBox.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.richTextBox.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // label
      // 
      this.label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.label.AutoSize = true;
      this.label.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label.Location = new System.Drawing.Point(2, 132);
      this.label.Name = "label";
      this.label.Size = new System.Drawing.Size(104, 13);
      this.label.TabIndex = 2;
      this.label.Text = "Default in 5 seconds";
      // 
      // timer
      // 
      this.timer.Interval = 500;
      this.timer.Tick += new System.EventHandler(this.timer_Tick);
      // 
      // button2
      // 
      this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.button2.Location = new System.Drawing.Point(285, 127);
      this.button2.Name = "button2";
      this.button2.Size = new System.Drawing.Size(75, 23);
      this.button2.TabIndex = 1;
      this.button2.Text = "button2";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new System.EventHandler(this.button2_Click);
      this.button2.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.button2.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // button3
      // 
      this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.button3.Location = new System.Drawing.Point(366, 127);
      this.button3.Name = "button3";
      this.button3.Size = new System.Drawing.Size(75, 23);
      this.button3.TabIndex = 2;
      this.button3.Text = "button3";
      this.button3.UseVisualStyleBackColor = true;
      this.button3.Click += new System.EventHandler(this.button3_Click);
      this.button3.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.button3.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // TimedMessage
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(445, 152);
      this.Controls.Add(this.button2);
      this.Controls.Add(this.label);
      this.Controls.Add(this.button3);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.richTextBox);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(300, 100);
      this.Name = "TimedMessage";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Timed Message";
      this.TopMost = true;
      this.Paint += new System.Windows.Forms.PaintEventHandler(this.TimedMessage_Paint);
      this.Resize += new System.EventHandler(this.TimedMessage_Resize);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion Windows Form Designer generated code

    public TimedMessage()
    {
      InitializeComponent();
    }

    public double Duration
    {
      get => m_Duration;
      set => m_Duration = value;
    }

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

    public DialogResult Show(Form owner, string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, double timeout)
    {
      Text = title;
      Message = message;
      m_Duration = timeout;
      timer.Enabled = true;
      m_DefaultButton = defaultButton;
      m_MessageBoxIcon = icon;
      m_MessageBoxButtons = buttons;

      UpdateButtons();
      TimedMessage_Resize(this, null);
      return this.ShowDialog(owner);
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

    private void button1_Click(object sender, EventArgs e)
    {
      if ((m_MessageBoxButtons == MessageBoxButtons.OK
        || m_MessageBoxButtons == MessageBoxButtons.OKCancel))
        DialogResult = DialogResult.OK;
      if ((m_MessageBoxButtons == MessageBoxButtons.YesNo
        || m_MessageBoxButtons == MessageBoxButtons.YesNoCancel))
        DialogResult = DialogResult.Yes;
      if (m_MessageBoxButtons == MessageBoxButtons.AbortRetryIgnore)
        DialogResult = DialogResult.Abort;
      if (m_MessageBoxButtons == MessageBoxButtons.RetryCancel)
        DialogResult = DialogResult.Retry;
      Close();
    }

    private void button2_Click(object sender, EventArgs e)
    {
      if (m_MessageBoxButtons == MessageBoxButtons.OKCancel || m_MessageBoxButtons == MessageBoxButtons.RetryCancel)
        DialogResult = DialogResult.Cancel;
      if ((m_MessageBoxButtons == MessageBoxButtons.YesNo
        || m_MessageBoxButtons == MessageBoxButtons.YesNoCancel))
        DialogResult = DialogResult.No;
      if (m_MessageBoxButtons == MessageBoxButtons.AbortRetryIgnore)
        DialogResult = DialogResult.Retry;
      Close();
    }

    private void button3_Click(object sender, EventArgs e)
    {
      if (m_MessageBoxButtons == MessageBoxButtons.AbortRetryIgnore)
        DialogResult = DialogResult.Ignore;
      if (m_MessageBoxButtons == MessageBoxButtons.YesNoCancel)
        DialogResult = DialogResult.Cancel;
      Close();
    }   

    private bool m_IconSet = false;

    private void TimedMessage_Paint(object sender, PaintEventArgs e)
    {
      if (m_IconSet || m_MessageBoxIcon == MessageBoxIcon.None)
        return;

      m_IconSet = true;
      switch (m_MessageBoxIcon)
      {
        case MessageBoxIcon.Question:
          e.Graphics.DrawIconUnstretched(SystemIcons.Question, new Rectangle(10, 10, 32, 32));
          break;

        case MessageBoxIcon.Error:
          e.Graphics.DrawIconUnstretched(SystemIcons.Error, new Rectangle(10, 10, 32, 32));
          break;

        case MessageBoxIcon.Warning:
          e.Graphics.DrawIconUnstretched(SystemIcons.Warning, new Rectangle(10, 10, 32, 32));
          break;

        case MessageBoxIcon.Information:
          e.Graphics.DrawIconUnstretched(SystemIcons.Information, new Rectangle(10, 10, 32, 32));
          break;

        default:
          break;
      }
    }

    private void timer_Tick(object sender, EventArgs e)
    {
      counter++;
      int displ = Convert.ToInt32((m_Duration - (counter * timer.Interval) / 1000 + .75));
      if (displ > 0)
      {
        if (m_DefaultButton == MessageBoxDefaultButton.Button1)
          label.Text = $"{button1.Text.Substring(1)} in {displ:N0} seconds";
        if (m_DefaultButton == MessageBoxDefaultButton.Button2)
          label.Text = $"{button2.Text.Substring(1)} in {displ:N0} seconds";
        if (m_DefaultButton == MessageBoxDefaultButton.Button3)
          label.Text = $"{button3.Text.Substring(1)} in {displ:N0} seconds";
      }
      else
        label.Text = string.Empty;

      if ((counter * timer.Interval) / 1000 > m_Duration)
      {
        if (m_DefaultButton == MessageBoxDefaultButton.Button1)
          button1_Click(sender, e);
        if (m_DefaultButton == MessageBoxDefaultButton.Button2)
          button2_Click(sender, e);
        if (m_DefaultButton == MessageBoxDefaultButton.Button3)
          button3_Click(sender, e);
      }
    }

    private void UpdateButtons()
    {
      // One Button
      if (m_MessageBoxButtons == MessageBoxButtons.OK)
      {
        button2.Visible = false;
        button3.Visible = false;
        button1.Location = button3.Location;
      }

      // Two Button
      if (m_MessageBoxButtons == MessageBoxButtons.YesNo ||
          m_MessageBoxButtons == MessageBoxButtons.OKCancel ||
          m_MessageBoxButtons == MessageBoxButtons.RetryCancel)
      {
        button3.Visible = false;
        button1.Location = button2.Location;
        button2.Location = button3.Location;
      }

      if ((m_MessageBoxButtons == MessageBoxButtons.OK
        || m_MessageBoxButtons == MessageBoxButtons.OKCancel) && button1.Text != "&OK")
      {
        button1.Text = "&OK";
      }
      if ((m_MessageBoxButtons == MessageBoxButtons.YesNo
        || m_MessageBoxButtons == MessageBoxButtons.YesNoCancel) && button1.Text != "&Yes")
      {
        button1.Text = "&Yes";
      }
      if ((m_MessageBoxButtons == MessageBoxButtons.AbortRetryIgnore) && button1.Text != "&Abort")
      {
        button1.Text = "&Abort";
      }
      if ((m_MessageBoxButtons == MessageBoxButtons.RetryCancel) && button1.Text != "&Retry")
      {
        button1.Text = "&Retry";
      }

      // Button 2
      if ((m_MessageBoxButtons == MessageBoxButtons.YesNo
        || m_MessageBoxButtons == MessageBoxButtons.YesNoCancel) && button2.Text != "&No")
      {
        button2.Text = "&No";
      }
      if ((m_MessageBoxButtons == MessageBoxButtons.AbortRetryIgnore) && button2.Text != "&Retry")
      {
        button2.Text = "&Retry";
      }
      if ((m_MessageBoxButtons == MessageBoxButtons.RetryCancel
        || m_MessageBoxButtons == MessageBoxButtons.OKCancel) && button2.Text != "&Cancel")
      {
        button2.Text = "&Cancel";
        CancelButton = button2;
      }

      // Button 3
      if (m_MessageBoxButtons == MessageBoxButtons.AbortRetryIgnore && button3.Text != "&Ignore")
      {
        button3.Text = "&Ignore";
      }
      if (m_MessageBoxButtons == MessageBoxButtons.YesNoCancel && button3.Text != "&Cancel")
      {
        button3.Text = "&Cancel";
        CancelButton = button3;
      }

      if (m_DefaultButton == MessageBoxDefaultButton.Button1)
      {
        AcceptButton = button1;
      }
      else if (m_DefaultButton == MessageBoxDefaultButton.Button2)
      {
        AcceptButton = button2;
      }
      else if (m_DefaultButton == MessageBoxDefaultButton.Button3)
      {
        AcceptButton = button3;
      }

      timer_Tick(this, null);
    }

    private void TimedMessage_Resize(object sender, EventArgs e)
    {
      richTextBox.Width = this.Width - richTextBox.Left - 12;
      richTextBox.Height = button1.Top - richTextBox.Top - 5;
    }

    private void MouseEnterElement(object sender, EventArgs e)
    {
      timer.Stop();
    }

    private void MouseLeaveElement(object sender, EventArgs e)
    {
      timer.Start();
    }
  }

#pragma warning disable CA1707 // Identifiers should not contain underscores
  public static class _MessageBox
#pragma warning restore CA1707 // Identifiers should not contain underscores
  {
    public static DialogResult Show(Form owner, string message, string title,
          MessageBoxButtons buttons = MessageBoxButtons.OKCancel,
          MessageBoxIcon icon = MessageBoxIcon.None,
          MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1,
          double timeout = 4.0)
    {
      using (var tm = new TimedMessage())
      {
        return tm.Show(owner, message, title, buttons, icon, defaultButton, timeout);
      }
    }
  }
}