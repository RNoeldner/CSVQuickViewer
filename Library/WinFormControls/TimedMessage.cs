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
      tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      ((System.ComponentModel.ISupportInitialize)(pictureBox)).BeginInit();
      tableLayoutPanel.SuspendLayout();
      SuspendLayout();
      //
      // button1
      //
      button1.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
      button1.BackColor = System.Drawing.SystemColors.ButtonFace;
      button1.Location = new System.Drawing.Point(419, 157);
      button1.Name = "button1";
      button1.Size = new System.Drawing.Size(100, 34);
      button1.TabIndex = 0;
      button1.Text = "button1";
      button1.UseVisualStyleBackColor = false;
      button1.MouseEnter += new System.EventHandler(MouseEnterElement);
      button1.MouseLeave += new System.EventHandler(MouseLeaveElement);
      //
      // richTextBox
      //
      richTextBox.BackColor = System.Drawing.SystemColors.Control;
      richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      tableLayoutPanel.SetColumnSpan(richTextBox, 4);
      richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      richTextBox.Location = new System.Drawing.Point(74, 5);
      richTextBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      richTextBox.Name = "richTextBox";
      richTextBox.ReadOnly = true;
      richTextBox.Size = new System.Drawing.Size(656, 144);
      richTextBox.TabIndex = 3;
      richTextBox.Text = "";
      richTextBox.MouseEnter += new System.EventHandler(MouseEnterElement);
      richTextBox.MouseLeave += new System.EventHandler(MouseLeaveElement);
      //
      // label
      //
      label.BackColor = System.Drawing.Color.Transparent;
      tableLayoutPanel.SetColumnSpan(label, 2);
      label.Dock = System.Windows.Forms.DockStyle.Fill;
      label.ForeColor = System.Drawing.SystemColors.InfoText;
      label.Location = new System.Drawing.Point(9, 154);
      label.Name = "label";
      label.Size = new System.Drawing.Size(404, 40);
      label.TabIndex = 2;
      label.Text = "Default in 5 seconds";
      label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      //
      // timer
      //
      timer.Enabled = true;
      timer.Interval = 500;
      timer.Tick += new System.EventHandler(Timer_Tick);
      //
      // button2
      //
      button2.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
      button2.BackColor = System.Drawing.SystemColors.ButtonFace;
      button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      button2.Location = new System.Drawing.Point(525, 157);
      button2.Name = "button2";
      button2.Size = new System.Drawing.Size(100, 34);
      button2.TabIndex = 1;
      button2.Text = "button2";
      button2.UseVisualStyleBackColor = false;
      button2.MouseEnter += new System.EventHandler(MouseEnterElement);
      button2.MouseLeave += new System.EventHandler(MouseLeaveElement);
      //
      // button3
      //
      button3.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
      button3.BackColor = System.Drawing.SystemColors.ButtonFace;
      button3.Location = new System.Drawing.Point(631, 157);
      button3.Name = "button3";
      button3.Size = new System.Drawing.Size(100, 34);
      button3.TabIndex = 2;
      button3.Text = "button3";
      button3.UseVisualStyleBackColor = false;
      button3.MouseEnter += new System.EventHandler(MouseEnterElement);
      button3.MouseLeave += new System.EventHandler(MouseLeaveElement);
      //
      // pictureBox
      //
      pictureBox.ErrorImage = null;
      pictureBox.InitialImage = null;
      pictureBox.Location = new System.Drawing.Point(10, 5);
      pictureBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
      pictureBox.Name = "pictureBox";
      pictureBox.Size = new System.Drawing.Size(56, 55);
      pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      pictureBox.TabIndex = 4;
      pictureBox.TabStop = false;
      pictureBox.WaitOnLoad = true;
      //
      // tableLayoutPanel
      //
      tableLayoutPanel.AutoSize = true;
      tableLayoutPanel.BackColor = System.Drawing.Color.Transparent;
      tableLayoutPanel.ColumnCount = 5;
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      tableLayoutPanel.Controls.Add(pictureBox, 0, 0);
      tableLayoutPanel.Controls.Add(label, 0, 1);
      tableLayoutPanel.Controls.Add(button3, 4, 1);
      tableLayoutPanel.Controls.Add(button2, 3, 1);
      tableLayoutPanel.Controls.Add(richTextBox, 1, 0);
      tableLayoutPanel.Controls.Add(button1, 2, 1);
      tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanel.Name = "tableLayoutPanel";
      tableLayoutPanel.Padding = new System.Windows.Forms.Padding(6, 0, 15, 5);
      tableLayoutPanel.RowCount = 2;
      tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel.Size = new System.Drawing.Size(749, 199);
      tableLayoutPanel.TabIndex = 5;
      //
      // TimedMessage
      //
      AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      BackColor = System.Drawing.SystemColors.Control;
      ClientSize = new System.Drawing.Size(749, 199);
      Controls.Add(tableLayoutPanel);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      MaximizeBox = false;
      MinimizeBox = false;
      MinimumSize = new System.Drawing.Size(380, 87);
      Name = "TimedMessage";
      ShowIcon = false;
      ShowInTaskbar = false;
      SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
      StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      Text = "Timed Message";
      TopMost = true;
      ((System.ComponentModel.ISupportInitialize)(pictureBox)).EndInit();
      tableLayoutPanel.ResumeLayout(false);
      tableLayoutPanel.PerformLayout();
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

    public DialogResult Show(Form owner, string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, double timeout, string button1Text, string button2Text, string button3Text)
    {
      Text = title;
      Message = message;
      Duration = timeout;

      m_MessageBoxButtons = buttons;

      if (!string.IsNullOrEmpty(button1Text))
        button1.Text = button1Text;

      if (!string.IsNullOrEmpty(button2Text))
        button2.Text = button2Text;

      if (!string.IsNullOrEmpty(button3Text))
        button3.Text = button3Text;

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
        Icon displayIcon = null;
        switch (icon)
        {
          case MessageBoxIcon.Question:
            displayIcon = new Icon(SystemIcons.Question, 40, 40);
            break;

          case MessageBoxIcon.Error:
            displayIcon = new Icon(SystemIcons.Error, 40, 40);
            // e.Graphics.DrawIcon(SystemIcons.Error, new Rectangle(10, 10, 32, 32));
            break;

          case MessageBoxIcon.Warning:
            displayIcon = new Icon(SystemIcons.Warning, 40, 40);
            // e.Graphics.DrawIcon(SystemIcons.Warning, new Rectangle(10, 10, 32, 32));
            break;

          case MessageBoxIcon.Information:
            displayIcon = new Icon(SystemIcons.Information, 40, 40);
            break;

          default:
            break;
        }
        pictureBox.Image = displayIcon.ToBitmap();
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