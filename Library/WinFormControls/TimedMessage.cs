namespace CsvTools
{
  using System;
  using System.ComponentModel;
  using System.Drawing;
  using System.Windows.Forms;

  public class TimedMessage : ResizeForm
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
      m_Components = new Container();
      var resources = new ComponentResourceManager(typeof(TimedMessage));
      m_Button1 = new Button();
      m_RichTextBox = new RichTextBox();
      m_LabelDefault = new Label();
      timer = new Timer(m_Components);
      m_Button2 = new Button();
      m_Button3 = new Button();
      m_PictureBox = new PictureBox();
      m_TableLayoutPanel = new TableLayoutPanel();
      m_ImageList = new ImageList(m_Components);
      ((ISupportInitialize)(m_PictureBox)).BeginInit();
      m_TableLayoutPanel.SuspendLayout();
      SuspendLayout();

      // button1
      m_Button1.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
      m_Button1.BackColor = SystemColors.ButtonFace;
      m_Button1.Location = new Point(371, 126);
      m_Button1.Margin = new Padding(3, 2, 3, 2);
      m_Button1.Name = "button1";
      m_Button1.Size = new Size(89, 27);
      m_Button1.TabIndex = 0;
      m_Button1.Text = "button1";
      m_Button1.UseVisualStyleBackColor = false;
      m_Button1.MouseEnter += new EventHandler(MouseEnterElement);
      m_Button1.MouseLeave += new EventHandler(MouseLeaveElement);

      // richTextBox
      m_RichTextBox.BackColor = SystemColors.Control;
      m_RichTextBox.BorderStyle = BorderStyle.None;
      m_TableLayoutPanel.SetColumnSpan(m_RichTextBox, 4);
      m_RichTextBox.Dock = DockStyle.Fill;
      m_RichTextBox.Location = new Point(65, 4);
      m_RichTextBox.Margin = new Padding(4);
      m_RichTextBox.Name = "richTextBox";
      m_RichTextBox.ReadOnly = true;
      m_RichTextBox.Size = new Size(584, 115);
      m_RichTextBox.TabIndex = 3;
      m_RichTextBox.Text = string.Empty;
      m_RichTextBox.MouseEnter += new EventHandler(MouseEnterElement);
      m_RichTextBox.MouseLeave += new EventHandler(MouseLeaveElement);

      // label
      m_LabelDefault.BackColor = Color.Transparent;
      m_TableLayoutPanel.SetColumnSpan(m_LabelDefault, 2);
      m_LabelDefault.Dock = DockStyle.Fill;
      m_LabelDefault.ForeColor = SystemColors.InfoText;
      m_LabelDefault.Location = new Point(8, 123);
      m_LabelDefault.Name = "label";
      m_LabelDefault.Size = new Size(357, 32);
      m_LabelDefault.TabIndex = 2;
      m_LabelDefault.Text = "Default in 5 seconds";
      m_LabelDefault.TextAlign = ContentAlignment.MiddleLeft;

      // timer
      timer.Enabled = true;
      timer.Interval = 500;
      timer.Tick += new EventHandler(Timer_Tick);

      // button2
      m_Button2.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
      m_Button2.BackColor = SystemColors.ButtonFace;
      m_Button2.DialogResult = DialogResult.Cancel;
      m_Button2.Location = new Point(466, 126);
      m_Button2.Margin = new Padding(3, 2, 3, 2);
      m_Button2.Name = "button2";
      m_Button2.Size = new Size(89, 27);
      m_Button2.TabIndex = 1;
      m_Button2.Text = "button2";
      m_Button2.UseVisualStyleBackColor = false;
      m_Button2.MouseEnter += new EventHandler(MouseEnterElement);
      m_Button2.MouseLeave += new EventHandler(MouseLeaveElement);

      // button3
      m_Button3.Anchor = (AnchorStyles.Bottom | AnchorStyles.Right);
      m_Button3.BackColor = SystemColors.ButtonFace;
      m_Button3.Location = new Point(561, 126);
      m_Button3.Margin = new Padding(3, 2, 3, 2);
      m_Button3.Name = "button3";
      m_Button3.Size = new Size(89, 27);
      m_Button3.TabIndex = 2;
      m_Button3.Text = "button3";
      m_Button3.UseVisualStyleBackColor = false;
      m_Button3.MouseEnter += new EventHandler(MouseEnterElement);
      m_Button3.MouseLeave += new EventHandler(MouseLeaveElement);

      // pictureBox
      m_PictureBox.ErrorImage = null;
      m_PictureBox.InitialImage = null;
      m_PictureBox.Location = new Point(9, 3);
      m_PictureBox.Margin = new Padding(4, 3, 4, 3);
      m_PictureBox.Name = "pictureBox";
      m_PictureBox.Size = new Size(48, 48);
      m_PictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
      m_PictureBox.TabIndex = 4;
      m_PictureBox.TabStop = false;

      // tableLayoutPanel
      m_TableLayoutPanel.AutoSize = true;
      m_TableLayoutPanel.BackColor = Color.Transparent;
      m_TableLayoutPanel.ColumnCount = 5;
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 27F));
      m_TableLayoutPanel.Controls.Add(m_LabelDefault, 0, 1);
      m_TableLayoutPanel.Controls.Add(m_Button3, 4, 1);
      m_TableLayoutPanel.Controls.Add(m_Button2, 3, 1);
      m_TableLayoutPanel.Controls.Add(m_RichTextBox, 1, 0);
      m_TableLayoutPanel.Controls.Add(m_Button1, 2, 1);
      m_TableLayoutPanel.Controls.Add(m_PictureBox, 0, 0);
      m_TableLayoutPanel.Dock = DockStyle.Fill;
      m_TableLayoutPanel.Location = new Point(0, 0);
      m_TableLayoutPanel.Margin = new Padding(3, 2, 3, 2);
      m_TableLayoutPanel.Name = "tableLayoutPanel";
      m_TableLayoutPanel.Padding = new Padding(5, 0, 13, 4);
      m_TableLayoutPanel.RowCount = 2;
      m_TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.Size = new Size(666, 159);
      m_TableLayoutPanel.TabIndex = 5;

      // imageList
      m_ImageList.ImageStream = ((ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
      m_ImageList.TransparentColor = Color.Transparent;
      m_ImageList.Images.SetKeyName(0, "Info-icon.bmp");
      m_ImageList.Images.SetKeyName(1, "icon-warning.bmp");
      m_ImageList.Images.SetKeyName(2, "icon-question.bmp");
      m_ImageList.Images.SetKeyName(3, "error-icon.bmp");

      // TimedMessage
      AutoScaleDimensions = new SizeF(8F, 16F);
      AutoScaleMode = AutoScaleMode.Font;
      AutoSize = true;
      BackColor = SystemColors.Control;
      ClientSize = new Size(666, 159);
      Controls.Add(m_TableLayoutPanel);
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      Margin = new Padding(3, 2, 3, 2);
      MaximizeBox = false;
      MinimizeBox = false;
      MinimumSize = new Size(340, 79);
      Name = "TimedMessage";
      ShowIcon = false;
      ShowInTaskbar = false;
      SizeGripStyle = SizeGripStyle.Show;
      StartPosition = FormStartPosition.CenterParent;
      Text = "Timed Message";
      TopMost = true;
      ((ISupportInitialize)(m_PictureBox)).EndInit();
      m_TableLayoutPanel.ResumeLayout(false);
      m_TableLayoutPanel.PerformLayout();
      ResumeLayout(false);
      PerformLayout();
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