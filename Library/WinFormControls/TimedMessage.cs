namespace CsvTools
{
  using System;
  using System.ComponentModel;
  using System.Drawing;
  using System.Windows.Forms;

  public class TimedMessage : Form
  {
    private Button m_Button1;

    private Button m_Button2;

    private Button m_Button3;

    private ImageList m_ImageList;

    private IContainer m_Components;

    private Label m_LabelDefault;

    private int m_Counter = 0;

    private MessageBoxButtons m_MessageBoxButtons = MessageBoxButtons.OKCancel;

    private PictureBox m_PictureBox;

    private RichTextBox m_RichTextBox;

    private TableLayoutPanel m_TableLayoutPanel;

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
      Message = message;
      Duration = timeout;

      m_MessageBoxButtons = buttons;

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

      return ShowDialog(owner);
    }

    /// <summary>
    ///   Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        m_Components?.Dispose();
      }

      base.Dispose(disposing);
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
    ///   Required method for Designer support - do not modify
    ///   the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.m_Components = new Container();
      ComponentResourceManager resources = new ComponentResourceManager(typeof(TimedMessage));
      this.m_Button1 = new Button();
      this.m_RichTextBox = new RichTextBox();
      this.m_LabelDefault = new Label();
      this.timer = new Timer(this.m_Components);
      this.m_Button2 = new Button();
      this.m_Button3 = new Button();
      this.m_PictureBox = new PictureBox();
      this.m_TableLayoutPanel = new TableLayoutPanel();
      this.m_ImageList = new ImageList(this.m_Components);
      ((ISupportInitialize)(this.m_PictureBox)).BeginInit();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();

      // button1
      this.m_Button1.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.m_Button1.BackColor = SystemColors.ButtonFace;
      this.m_Button1.Location = new Point(371, 126);
      this.m_Button1.Margin = new Padding(3, 2, 3, 2);
      this.m_Button1.Name = "button1";
      this.m_Button1.Size = new Size(89, 27);
      this.m_Button1.TabIndex = 0;
      this.m_Button1.Text = "button1";
      this.m_Button1.UseVisualStyleBackColor = false;
      this.m_Button1.MouseEnter += new EventHandler(this.MouseEnterElement);
      this.m_Button1.MouseLeave += new EventHandler(this.MouseLeaveElement);

      // richTextBox
      this.m_RichTextBox.BackColor = SystemColors.Control;
      this.m_RichTextBox.BorderStyle = BorderStyle.None;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_RichTextBox, 4);
      this.m_RichTextBox.Dock = DockStyle.Fill;
      this.m_RichTextBox.Location = new Point(65, 4);
      this.m_RichTextBox.Margin = new Padding(4);
      this.m_RichTextBox.Name = "richTextBox";
      this.m_RichTextBox.ReadOnly = true;
      this.m_RichTextBox.Size = new Size(584, 115);
      this.m_RichTextBox.TabIndex = 3;
      this.m_RichTextBox.Text = string.Empty;
      this.m_RichTextBox.MouseEnter += new EventHandler(this.MouseEnterElement);
      this.m_RichTextBox.MouseLeave += new EventHandler(this.MouseLeaveElement);

      // label
      this.m_LabelDefault.BackColor = Color.Transparent;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_LabelDefault, 2);
      this.m_LabelDefault.Dock = DockStyle.Fill;
      this.m_LabelDefault.ForeColor = SystemColors.InfoText;
      this.m_LabelDefault.Location = new Point(8, 123);
      this.m_LabelDefault.Name = "label";
      this.m_LabelDefault.Size = new Size(357, 32);
      this.m_LabelDefault.TabIndex = 2;
      this.m_LabelDefault.Text = "Default in 5 seconds";
      this.m_LabelDefault.TextAlign = ContentAlignment.MiddleLeft;

      // timer
      this.timer.Enabled = true;
      this.timer.Interval = 500;
      this.timer.Tick += new EventHandler(this.Timer_Tick);

      // button2
      this.m_Button2.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.m_Button2.BackColor = SystemColors.ButtonFace;
      this.m_Button2.DialogResult = DialogResult.Cancel;
      this.m_Button2.Location = new Point(466, 126);
      this.m_Button2.Margin = new Padding(3, 2, 3, 2);
      this.m_Button2.Name = "button2";
      this.m_Button2.Size = new Size(89, 27);
      this.m_Button2.TabIndex = 1;
      this.m_Button2.Text = "button2";
      this.m_Button2.UseVisualStyleBackColor = false;
      this.m_Button2.MouseEnter += new EventHandler(this.MouseEnterElement);
      this.m_Button2.MouseLeave += new EventHandler(this.MouseLeaveElement);

      // button3
      this.m_Button3.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
      this.m_Button3.BackColor = SystemColors.ButtonFace;
      this.m_Button3.Location = new Point(561, 126);
      this.m_Button3.Margin = new Padding(3, 2, 3, 2);
      this.m_Button3.Name = "button3";
      this.m_Button3.Size = new Size(89, 27);
      this.m_Button3.TabIndex = 2;
      this.m_Button3.Text = "button3";
      this.m_Button3.UseVisualStyleBackColor = false;
      this.m_Button3.MouseEnter += new EventHandler(this.MouseEnterElement);
      this.m_Button3.MouseLeave += new EventHandler(this.MouseLeaveElement);

      // pictureBox
      this.m_PictureBox.ErrorImage = null;
      this.m_PictureBox.InitialImage = null;
      this.m_PictureBox.Location = new Point(9, 3);
      this.m_PictureBox.Margin = new Padding(4, 3, 4, 3);
      this.m_PictureBox.Name = "pictureBox";
      this.m_PictureBox.Size = new Size(48, 48);
      this.m_PictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
      this.m_PictureBox.TabIndex = 4;
      this.m_PictureBox.TabStop = false;

      // tableLayoutPanel
      this.m_TableLayoutPanel.AutoSize = true;
      this.m_TableLayoutPanel.BackColor = Color.Transparent;
      this.m_TableLayoutPanel.ColumnCount = 5;
      this.m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 27F));
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelDefault, 0, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_Button3, 4, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_Button2, 3, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_RichTextBox, 1, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_Button1, 2, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_PictureBox, 0, 0);
      this.m_TableLayoutPanel.Dock = DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new Point(0, 0);
      this.m_TableLayoutPanel.Margin = new Padding(3, 2, 3, 2);
      this.m_TableLayoutPanel.Name = "tableLayoutPanel";
      this.m_TableLayoutPanel.Padding = new Padding(5, 0, 13, 4);
      this.m_TableLayoutPanel.RowCount = 2;
      this.m_TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
      this.m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      this.m_TableLayoutPanel.Size = new Size(666, 159);
      this.m_TableLayoutPanel.TabIndex = 5;

      // imageList
      this.m_ImageList.ImageStream = ((ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
      this.m_ImageList.TransparentColor = Color.Transparent;
      this.m_ImageList.Images.SetKeyName(0, "Info-icon.bmp");
      this.m_ImageList.Images.SetKeyName(1, "icon-warning.bmp");
      this.m_ImageList.Images.SetKeyName(2, "icon-question.bmp");
      this.m_ImageList.Images.SetKeyName(3, "error-icon.bmp");

      // TimedMessage
      this.AutoScaleDimensions = new SizeF(8F, 16F);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.AutoSize = true;
      this.BackColor = SystemColors.Control;
      this.ClientSize = new Size(666, 159);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
      this.Margin = new Padding(3, 2, 3, 2);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new Size(340, 79);
      this.Name = "TimedMessage";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = SizeGripStyle.Show;
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = "Timed Message";
      this.TopMost = true;
      ((ISupportInitialize)(this.m_PictureBox)).EndInit();
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
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