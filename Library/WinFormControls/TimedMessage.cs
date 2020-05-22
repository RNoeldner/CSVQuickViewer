namespace CsvTools
{
  using System;
  using System.ComponentModel;
  using System.Windows.Forms;

  public class TimedMessage : ResizeForm
  {
    private Button m_Button1;

    private Button m_Button2;

    private Button m_Button3;

    private ImageList m_ImageList;

    private Label m_LabelDefault;

    private int m_Counter = 0;

    private PictureBox m_PictureBox;

    private RichTextBox m_RichTextBox;

    private TableLayoutPanel m_TableLayoutPanel;
    private IContainer components;
    private Timer timer;

    public TimedMessage() => InitializeComponent();

    public double Duration { get; set; } = 4.0;

    public string Message
    {
      get => m_RichTextBox.Text;
      set => m_RichTextBox.Text = value;
    }

    public string MessageRtf
    {
      get => m_RichTextBox.Rtf;
      set => m_RichTextBox.Rtf = value;
    }

    public DialogResult Show(
      Form owner,
      string message,
      string title,
      MessageBoxButtons buttons,
      MessageBoxIcon icon,
      MessageBoxDefaultButton defaultButton,
      double timeout,
      string button1Text,
      string button2Text,
      string button3Text)
    {
      Text = title;
      if (!string.IsNullOrEmpty(message))
        Message = message;
      Duration = timeout;

      // One Button
      if (buttons == MessageBoxButtons.OK)
      {
        HideColumn(3, false);
        HideColumn(4, false);
        m_Button2.Visible = false;
        m_Button3.Visible = false;
      }

      // Two Button
      if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.OKCancel
                                             || buttons == MessageBoxButtons.RetryCancel)
      {
        HideColumn(4, false);
        m_Button3.Visible = false;
      }

      if ((buttons == MessageBoxButtons.OK || buttons == MessageBoxButtons.OKCancel))
      {
        m_Button1.Text = "&OK";
        m_Button1.DialogResult = DialogResult.OK;
      }

      if ((buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel))
      {
        m_Button1.Text = "&Yes";
        m_Button1.DialogResult = DialogResult.Yes;
      }

      if ((buttons == MessageBoxButtons.AbortRetryIgnore))
      {
        m_Button1.Text = "&Abort";
        m_Button1.DialogResult = DialogResult.Abort;
      }

      if ((buttons == MessageBoxButtons.RetryCancel))
      {
        m_Button1.Text = "&Retry";
        m_Button1.DialogResult = DialogResult.Retry;
      }

      // Button 2
      if ((buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel))
      {
        m_Button2.Text = "&No";
        m_Button2.DialogResult = DialogResult.No;
      }

      if ((buttons == MessageBoxButtons.AbortRetryIgnore))
      {
        m_Button2.Text = "&Retry";
        m_Button2.DialogResult = DialogResult.Retry;
      }

      if ((buttons == MessageBoxButtons.RetryCancel || buttons == MessageBoxButtons.OKCancel))
      {
        m_Button2.Text = "&Cancel";
        m_Button2.DialogResult = DialogResult.Cancel;
        CancelButton = m_Button2;
      }

      // Button 3
      if (buttons == MessageBoxButtons.AbortRetryIgnore)
      {
        m_Button3.Text = "&Ignore";
        m_Button3.DialogResult = DialogResult.Ignore;
      }

      if (buttons == MessageBoxButtons.YesNoCancel)
      {
        m_Button3.Text = "&Cancel";
        m_Button3.DialogResult = DialogResult.Cancel;
        CancelButton = m_Button3;
      }

      if (!string.IsNullOrEmpty(button1Text))
        m_Button1.Text = button1Text;

      if (!string.IsNullOrEmpty(button2Text))
        m_Button2.Text = button2Text;

      if (!string.IsNullOrEmpty(button3Text))
        m_Button3.Text = button3Text;

      if (defaultButton == MessageBoxDefaultButton.Button1)
      {
        AcceptButton = m_Button1;
      }
      else if (defaultButton == MessageBoxDefaultButton.Button2)
      {
        AcceptButton = m_Button2;
      }
      else if (defaultButton == MessageBoxDefaultButton.Button3)
      {
        AcceptButton = m_Button3;
      }
      else
        AcceptButton = m_Button1;

      if (icon != MessageBoxIcon.None)
      {
        switch (icon)
        {
          case MessageBoxIcon.Error:
            m_PictureBox.Image = m_ImageList.Images[3];
            break;

          case MessageBoxIcon.Information:
            m_PictureBox.Image = m_ImageList.Images[0];
            break;

          case MessageBoxIcon.Warning:
            m_PictureBox.Image = m_ImageList.Images[1];
            break;

          case MessageBoxIcon.Question:
            m_PictureBox.Image = m_ImageList.Images[2];
            break;

          default:
            break;
        }
      }
      if (owner == null)
        return ShowDialog();
      else
        return ShowDialog(owner);
    }

    private void HideColumn(int colNumber, bool visible)
    {
      var styles = m_TableLayoutPanel.ColumnStyles;
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

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimedMessage));
      this.timer = new System.Windows.Forms.Timer(this.components);
      this.m_ImageList = new System.Windows.Forms.ImageList(this.components);
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.m_LabelDefault = new System.Windows.Forms.Label();
      this.m_Button3 = new System.Windows.Forms.Button();
      this.m_Button2 = new System.Windows.Forms.Button();
      this.m_RichTextBox = new System.Windows.Forms.RichTextBox();
      this.m_Button1 = new System.Windows.Forms.Button();
      this.m_PictureBox = new System.Windows.Forms.PictureBox();
      this.m_TableLayoutPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.m_PictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // timer
      // 
      this.timer.Enabled = true;
      this.timer.Interval = 500;
      this.timer.Tick += new System.EventHandler(this.Timer_Tick);
      // 
      // m_ImageList
      // 
      this.m_ImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("m_ImageList.ImageStream")));
      this.m_ImageList.TransparentColor = System.Drawing.Color.Transparent;
      this.m_ImageList.Images.SetKeyName(0, "Info-icon.bmp");
      this.m_ImageList.Images.SetKeyName(1, "icon-warning.bmp");
      this.m_ImageList.Images.SetKeyName(2, "icon-question.bmp");
      this.m_ImageList.Images.SetKeyName(3, "error-icon.bmp");
      // 
      // m_TableLayoutPanel
      // 
      this.m_TableLayoutPanel.AutoSize = true;
      this.m_TableLayoutPanel.BackColor = System.Drawing.Color.Transparent;
      this.m_TableLayoutPanel.ColumnCount = 5;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 26F));
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelDefault, 0, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_Button3, 4, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_Button2, 3, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_RichTextBox, 1, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_Button1, 2, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_PictureBox, 0, 0);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(2);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.Padding = new System.Windows.Forms.Padding(6, 0, 14, 4);
      this.m_TableLayoutPanel.RowCount = 2;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(662, 164);
      this.m_TableLayoutPanel.TabIndex = 5;
      // 
      // m_LabelDefault
      // 
      this.m_LabelDefault.BackColor = System.Drawing.Color.Transparent;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_LabelDefault, 2);
      this.m_LabelDefault.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_LabelDefault.ForeColor = System.Drawing.SystemColors.InfoText;
      this.m_LabelDefault.Location = new System.Drawing.Point(8, 126);
      this.m_LabelDefault.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.m_LabelDefault.Name = "m_LabelDefault";
      this.m_LabelDefault.Size = new System.Drawing.Size(293, 34);
      this.m_LabelDefault.TabIndex = 2;
      this.m_LabelDefault.Text = "Default in 5 seconds";
      this.m_LabelDefault.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // m_Button3
      // 
      this.m_Button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Button3.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.m_Button3.Location = new System.Drawing.Point(535, 130);
      this.m_Button3.Margin = new System.Windows.Forms.Padding(2);
      this.m_Button3.Name = "m_Button3";
      this.m_Button3.Size = new System.Drawing.Size(111, 28);
      this.m_Button3.TabIndex = 2;
      this.m_Button3.Text = "button3";
      this.m_Button3.UseVisualStyleBackColor = false;
      this.m_Button3.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.m_Button3.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // m_Button2
      // 
      this.m_Button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Button2.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.m_Button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_Button2.Location = new System.Drawing.Point(420, 130);
      this.m_Button2.Margin = new System.Windows.Forms.Padding(2);
      this.m_Button2.Name = "m_Button2";
      this.m_Button2.Size = new System.Drawing.Size(111, 28);
      this.m_Button2.TabIndex = 1;
      this.m_Button2.Text = "button2";
      this.m_Button2.UseVisualStyleBackColor = false;
      this.m_Button2.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.m_Button2.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // m_RichTextBox
      // 
      this.m_RichTextBox.BackColor = System.Drawing.SystemColors.Control;
      this.m_RichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_RichTextBox, 4);
      this.m_RichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_RichTextBox.Location = new System.Drawing.Point(63, 4);
      this.m_RichTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
      this.m_RichTextBox.Name = "m_RichTextBox";
      this.m_RichTextBox.ReadOnly = true;
      this.m_RichTextBox.Size = new System.Drawing.Size(582, 118);
      this.m_RichTextBox.TabIndex = 3;
      this.m_RichTextBox.Text = "";
      this.m_RichTextBox.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.m_RichTextBox.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // m_Button1
      // 
      this.m_Button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Button1.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.m_Button1.Location = new System.Drawing.Point(305, 130);
      this.m_Button1.Margin = new System.Windows.Forms.Padding(2);
      this.m_Button1.Name = "m_Button1";
      this.m_Button1.Size = new System.Drawing.Size(111, 28);
      this.m_Button1.TabIndex = 0;
      this.m_Button1.Text = "button1";
      this.m_Button1.UseVisualStyleBackColor = false;
      this.m_Button1.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.m_Button1.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // m_PictureBox
      // 
      this.m_PictureBox.ErrorImage = null;
      this.m_PictureBox.InitialImage = null;
      this.m_PictureBox.Location = new System.Drawing.Point(9, 3);
      this.m_PictureBox.Name = "m_PictureBox";
      this.m_PictureBox.Size = new System.Drawing.Size(48, 48);
      this.m_PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.m_PictureBox.TabIndex = 4;
      this.m_PictureBox.TabStop = false;
      // 
      // TimedMessage
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.ClientSize = new System.Drawing.Size(662, 164);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(2);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(339, 74);
      this.Name = "TimedMessage";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Timed Message";
      this.TopMost = true;
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.m_PictureBox)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

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
        AcceptButton?.PerformClick();
      }
    }

    private void UpdateLabel()
    {
      var displ = Convert.ToInt32((Duration - (m_Counter * timer.Interval) / 1000 + .75));
      if (!timer.Enabled)
        displ = 0;
      if (displ > 0)
      {
        if (AcceptButton == m_Button1)
          m_LabelDefault.Text = $"{m_Button1.Text.Substring(1)} in {displ:N0} seconds";
        if (AcceptButton == m_Button2)
          m_LabelDefault.Text = $"{m_Button2.Text.Substring(1)} in {displ:N0} seconds";
        if (AcceptButton == m_Button3)
          m_LabelDefault.Text = $"{m_Button3.Text.Substring(1)} in {displ:N0} seconds";
      }
      else
        m_LabelDefault.Text = string.Empty;
    }
  }

#pragma warning disable CA1707 // Identifiers should not contain underscores
}