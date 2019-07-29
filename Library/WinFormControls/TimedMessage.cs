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

    private int m_Counter = 0;
    private Label label;
    private MessageBoxDefaultButton m_DefaultButton = MessageBoxDefaultButton.Button1;
    private MessageBoxButtons m_MessageBoxButtons = MessageBoxButtons.OKCancel;
    private MessageBoxIcon m_MessageBoxIcon = MessageBoxIcon.None;
    private RichTextBox richTextBox;
    private PictureBox pictureBox;
    private Timer timer;

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new System.ComponentModel.Container();
      button1 = new System.Windows.Forms.Button();
      richTextBox = new System.Windows.Forms.RichTextBox();
      label = new System.Windows.Forms.Label();
      timer = new System.Windows.Forms.Timer(components);
      button2 = new System.Windows.Forms.Button();
      button3 = new System.Windows.Forms.Button();
      pictureBox = new System.Windows.Forms.PictureBox();
      ((System.ComponentModel.ISupportInitialize)(pictureBox)).BeginInit();
      SuspendLayout();
      //
      // button1
      //
      button1.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
      button1.Location = new System.Drawing.Point(208, 150);
      button1.Margin = new System.Windows.Forms.Padding(4);
      button1.Name = "button1";
      button1.Size = new System.Drawing.Size(100, 28);
      button1.TabIndex = 0;
      button1.Text = "button1";
      button1.UseVisualStyleBackColor = true;
      button1.Click += new System.EventHandler(Button1_Click);
      button1.MouseEnter += new System.EventHandler(MouseEnterElement);
      button1.MouseLeave += new System.EventHandler(MouseLeaveElement);
      //
      // richTextBox
      //
      richTextBox.BackColor = System.Drawing.SystemColors.Control;
      richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      richTextBox.Location = new System.Drawing.Point(71, 10);
      richTextBox.Margin = new System.Windows.Forms.Padding(4);
      richTextBox.Name = "richTextBox";
      richTextBox.ReadOnly = true;
      richTextBox.Size = new System.Drawing.Size(472, 133);
      richTextBox.TabIndex = 3;
      richTextBox.Text = "";
      richTextBox.MouseEnter += new System.EventHandler(MouseEnterElement);
      richTextBox.MouseLeave += new System.EventHandler(MouseLeaveElement);
      //
      // label
      //
      label.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
      label.AutoSize = true;
      label.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      label.Location = new System.Drawing.Point(3, 156);
      label.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      label.Name = "label";
      label.Size = new System.Drawing.Size(137, 17);
      label.TabIndex = 2;
      label.Text = "Default in 5 seconds";
      //
      // timer
      //
      timer.Interval = 500;
      timer.Tick += new System.EventHandler(Timer_Tick);
      //
      // button2
      //
      button2.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
      button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      button2.Location = new System.Drawing.Point(315, 150);
      button2.Margin = new System.Windows.Forms.Padding(4);
      button2.Name = "button2";
      button2.Size = new System.Drawing.Size(100, 28);
      button2.TabIndex = 1;
      button2.Text = "button2";
      button2.UseVisualStyleBackColor = true;
      button2.Click += new System.EventHandler(Button2_Click);
      button2.MouseEnter += new System.EventHandler(MouseEnterElement);
      button2.MouseLeave += new System.EventHandler(MouseLeaveElement);
      //
      // button3
      //
      button3.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
      button3.Location = new System.Drawing.Point(423, 150);
      button3.Margin = new System.Windows.Forms.Padding(4);
      button3.Name = "button3";
      button3.Size = new System.Drawing.Size(100, 28);
      button3.TabIndex = 2;
      button3.Text = "button3";
      button3.UseVisualStyleBackColor = true;
      button3.Click += new System.EventHandler(Button3_Click);
      button3.MouseEnter += new System.EventHandler(MouseEnterElement);
      button3.MouseLeave += new System.EventHandler(MouseLeaveElement);
      //
      // pictureBox
      //
      pictureBox.ErrorImage = null;
      pictureBox.InitialImage = null;
      pictureBox.Location = new System.Drawing.Point(10, 10);
      pictureBox.Margin = new System.Windows.Forms.Padding(0);
      pictureBox.Name = "pictureBox";
      pictureBox.Size = new System.Drawing.Size(56, 56);
      pictureBox.TabIndex = 4;
      pictureBox.TabStop = false;
      //
      // TimedMessage
      //
      
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      ClientSize = new System.Drawing.Size(543, 181);
      Controls.Add(pictureBox);
      Controls.Add(button2);
      Controls.Add(button3);
      Controls.Add(button1);
      Controls.Add(label);
      Controls.Add(richTextBox);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      Margin = new System.Windows.Forms.Padding(4);
      MaximizeBox = false;
      MinimizeBox = false;
      MinimumSize = new System.Drawing.Size(394, 112);
      Name = "TimedMessage";
      ShowIcon = false;
      ShowInTaskbar = false;
      StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      Text = "Timed Message";
      TopMost = true;
      Paint += new System.Windows.Forms.PaintEventHandler(TimedMessage_Paint);
      Resize += new System.EventHandler(TimedMessage_Resize);
      ((System.ComponentModel.ISupportInitialize)(pictureBox)).EndInit();
      ResumeLayout(false);
      PerformLayout();
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

    public DialogResult Show(Form owner, string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, double timeout, string button3Text)
    {
      Text = title;
      Message = message;
      Duration = timeout;
      timer.Enabled = true;
      m_DefaultButton = defaultButton;
      m_MessageBoxIcon = icon;
      m_MessageBoxButtons = buttons;

      UpdateButtons();
      if (!string.IsNullOrEmpty(button3Text))
        button3.Text = button3Text;
      TimedMessage_Resize(this, null);
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

    private void Button1_Click(object sender, EventArgs e)
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

    private void Button2_Click(object sender, EventArgs e)
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

    private void Button3_Click(object sender, EventArgs e)
    {
      if (m_MessageBoxButtons == MessageBoxButtons.AbortRetryIgnore)
        DialogResult = DialogResult.Ignore;
      if (m_MessageBoxButtons == MessageBoxButtons.YesNoCancel)
        DialogResult = DialogResult.Cancel;
      Close();
    }

    private void TimedMessage_Paint(object sender, PaintEventArgs e)
    {
      if (m_MessageBoxIcon == MessageBoxIcon.None)
        return;

      Icon displayIcon = null;
      switch (m_MessageBoxIcon)
      {
        case MessageBoxIcon.Question:
          displayIcon = new Icon(SystemIcons.Question, 40, 40);
          break;

        case MessageBoxIcon.Error:
          displayIcon = new Icon(SystemIcons.Error, 40, 40);
          e.Graphics.DrawIconUnstretched(SystemIcons.Error, new Rectangle(10, 10, 32, 32));
          break;

        case MessageBoxIcon.Warning:
          displayIcon = new Icon(SystemIcons.Warning, 40, 40);
          e.Graphics.DrawIconUnstretched(SystemIcons.Warning, new Rectangle(10, 10, 32, 32));
          break;

        case MessageBoxIcon.Information:
          displayIcon = new Icon(SystemIcons.Information, 40, 40);
          e.Graphics.DrawIconUnstretched(SystemIcons.Information, new Rectangle(10, 10, 32, 32));
          break;

        default:
          break;
      }
      pictureBox.Image = displayIcon.ToBitmap();
    }

    private void UpdateLabel()
    {
      var displ = Convert.ToInt32((Duration - (m_Counter * timer.Interval) / 1000 + .75));
      if (!timer.Enabled)
        displ = 0;
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
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
      m_Counter++;
      UpdateLabel();

      if (Duration > 0 && (m_Counter * timer.Interval) / 1000 > Duration)
      {
        if (m_DefaultButton == MessageBoxDefaultButton.Button1)
          Button1_Click(sender, e);
        if (m_DefaultButton == MessageBoxDefaultButton.Button2)
          Button2_Click(sender, e);
        if (m_DefaultButton == MessageBoxDefaultButton.Button3)
          Button3_Click(sender, e);
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

      Timer_Tick(this, null);
    }

    private void TimedMessage_Resize(object sender, EventArgs e)
    {
      richTextBox.Width = Width - pictureBox.Right - 8;
      richTextBox.Height = button1.Top - richTextBox.Top - 5;
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
        return tm.Show(owner, message, title, buttons, icon, defaultButton, timeout, null);
      }
    }

    public static DialogResult Show(Form owner, string message, string title,
         MessageBoxButtons buttons,
         MessageBoxIcon icon,
         MessageBoxDefaultButton defaultButton,
         double timeout,
         string button3Text)
    {
      using (var tm = new TimedMessage())
      {
        return tm.Show(owner, message, title, buttons, icon, defaultButton, timeout, button3Text);
      }
    }

    public static DialogResult ShowBig(Form owner, string message, string title,
         MessageBoxButtons buttons = MessageBoxButtons.OKCancel,
         MessageBoxIcon icon = MessageBoxIcon.None,
         MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1,
         double timeout = 4.0)
    {
      using (var tm = new TimedMessage())
      {
        tm.Size = new Size(600, 450);
        return tm.Show(owner, message, title, buttons, icon, defaultButton, timeout, null);
      }
    }
  }
}