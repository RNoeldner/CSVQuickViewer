#nullable enable

namespace CsvTools
{
  using System;
  using System.ComponentModel;
  using System.Windows.Forms;

  public class TimedMessage : ResizeForm
  {
    private IContainer components;
    private Button m_Button1;

    private Button m_Button2;

    private Button m_Button3;

    private int m_Counter;

    private ImageList m_ImageList;

    private Label m_LabelDefault;

    private PictureBox m_PictureBox;

    private TableLayoutPanel m_TableLayoutPanel;

    private TextBox m_TextBox;
    private Timer m_Timer = new Timer();
    private WebBrowser m_WebBrowser;

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    public TimedMessage() => InitializeComponent();

#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    public double Duration { get; set; } = 4.0;

    public string Message
    {
      set => m_TextBox.Text = value.HandleCrlfCombinations(Environment.NewLine);
    }

    public string Html
    {
      set
      {
        m_TableLayoutPanel.Controls.Remove(m_TextBox);
        // this need to happen here
        Extensions.RunStaThread(() =>
        {
          m_WebBrowser = new WebBrowser();
          m_WebBrowser.Navigate("about:blank");
          m_WebBrowser.Document?.OpenNew(false)?.Write(value);
          m_TableLayoutPanel.Controls.Add(m_WebBrowser, 1, 0);
          m_TableLayoutPanel.SetColumnSpan(m_WebBrowser, 4);
          m_WebBrowser.AllowNavigation = false;
          m_WebBrowser.AllowWebBrowserDrop = false;
          m_WebBrowser.IsWebBrowserContextMenuEnabled = false;
          m_WebBrowser.ScriptErrorsSuppressed = true;
          m_WebBrowser.Dock = DockStyle.Fill;
        });
      }
    }

    private bool m_Disposed;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (m_Disposed)
        return;
      try
      {
        if (disposing)
        {
          components.Dispose();
          m_WebBrowser.Dispose();
        }

        base.Dispose(disposing);
      }
      finally
      {
        m_Disposed =true;
      }
    }

    public DialogResult ShowDialog(
      string message,
      string? title,
      MessageBoxButtons buttons,
      MessageBoxIcon icon,
      MessageBoxDefaultButton defaultButton,
      double timeout,
      string? button1Text,
      string? button2Text,
      string? button3Text)
    {
      Text = title ?? string.Empty;
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

      if (buttons == MessageBoxButtons.OK || buttons == MessageBoxButtons.OKCancel)
      {
        m_Button1.Text = @"&OK";
        m_Button1.DialogResult = DialogResult.OK;
      }

      if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel)
      {
        m_Button1.Text = @"&Yes";
        m_Button1.DialogResult = DialogResult.Yes;
      }

      if (buttons == MessageBoxButtons.AbortRetryIgnore)
      {
        m_Button1.Text = @"&Abort";
        m_Button1.DialogResult = DialogResult.Abort;
      }

      if (buttons == MessageBoxButtons.RetryCancel)
      {
        m_Button1.Text = @"&Retry";
        m_Button1.DialogResult = DialogResult.Retry;
      }

      // Button 2
      if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel)
      {
        m_Button2.Text = @"&No";
        m_Button2.DialogResult = DialogResult.No;
      }

      if (buttons == MessageBoxButtons.AbortRetryIgnore)
      {
        m_Button2.Text = @"&Retry";
        m_Button2.DialogResult = DialogResult.Retry;
      }

      if (buttons == MessageBoxButtons.RetryCancel || buttons == MessageBoxButtons.OKCancel)
      {
        m_Button2.Text = @"&Cancel";
        m_Button2.DialogResult = DialogResult.Cancel;
        CancelButton = m_Button2;
      }

      // Button 3
      if (buttons == MessageBoxButtons.AbortRetryIgnore)
      {
        m_Button3.Text = @"&Ignore";
        m_Button3.DialogResult = DialogResult.Ignore;
      }

      if (buttons == MessageBoxButtons.YesNoCancel)
      {
        m_Button3.Text = @"&Cancel";
        m_Button3.DialogResult = DialogResult.Cancel;
        CancelButton = m_Button3;
      }

      if (!string.IsNullOrEmpty(button1Text))
        m_Button1.Text = button1Text;

      if (!string.IsNullOrEmpty(button2Text))
        m_Button2.Text = button2Text;

      if (!string.IsNullOrEmpty(button3Text))
        m_Button3.Text = button3Text;

      AcceptButton = defaultButton switch
      {
        MessageBoxDefaultButton.Button1 => m_Button1,
        MessageBoxDefaultButton.Button2 => m_Button2,
        MessageBoxDefaultButton.Button3 => m_Button3,
        _ => m_Button1
      };
      if (icon != MessageBoxIcon.None)
      {
        m_PictureBox.Image = icon switch
        {
          MessageBoxIcon.Information => m_ImageList.Images[0],
          MessageBoxIcon.Warning => m_ImageList.Images[1],
          MessageBoxIcon.Question => m_ImageList.Images[2],
          MessageBoxIcon.Error => m_ImageList.Images[3],
          _ => m_PictureBox.Image
        };
      }
      TopLevel = true;

      var result = AcceptButton.DialogResult;
      Extensions.RunStaThread(() => result = ShowDialog());
      return result;
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
      this.m_Timer = new System.Windows.Forms.Timer(this.components);
      this.m_ImageList = new System.Windows.Forms.ImageList(this.components);
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.m_LabelDefault = new System.Windows.Forms.Label();
      this.m_Button3 = new System.Windows.Forms.Button();
      this.m_Button2 = new System.Windows.Forms.Button();
      this.m_TextBox = new System.Windows.Forms.TextBox();
      this.m_Button1 = new System.Windows.Forms.Button();
      this.m_PictureBox = new System.Windows.Forms.PictureBox();
      this.m_TableLayoutPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.m_PictureBox)).BeginInit();
      this.SuspendLayout();
      // 
      // m_Timer
      // 
      this.m_Timer.Enabled = true;
      this.m_Timer.Interval = 500;
      this.m_Timer.Tick += new System.EventHandler(this.Timer_Tick);
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
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelDefault, 0, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_Button3, 4, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_Button2, 3, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_TextBox, 1, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_Button1, 2, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_PictureBox, 0, 0);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(2);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.Padding = new System.Windows.Forms.Padding(4, 0, 10, 3);
      this.m_TableLayoutPanel.RowCount = 2;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(446, 217);
      this.m_TableLayoutPanel.TabIndex = 5;
      // 
      // m_LabelDefault
      // 
      this.m_LabelDefault.BackColor = System.Drawing.Color.Transparent;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_LabelDefault, 2);
      this.m_LabelDefault.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_LabelDefault.ForeColor = System.Drawing.SystemColors.InfoText;
      this.m_LabelDefault.Location = new System.Drawing.Point(6, 182);
      this.m_LabelDefault.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.m_LabelDefault.Name = "m_LabelDefault";
      this.m_LabelDefault.Size = new System.Drawing.Size(161, 32);
      this.m_LabelDefault.TabIndex = 2;
      this.m_LabelDefault.Text = "Default in 5 seconds";
      this.m_LabelDefault.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // m_Button3
      // 
      this.m_Button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Button3.AutoSize = true;
      this.m_Button3.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.m_Button3.Location = new System.Drawing.Point(350, 185);
      this.m_Button3.Name = "m_Button3";
      this.m_Button3.Size = new System.Drawing.Size(83, 26);
      this.m_Button3.TabIndex = 2;
      this.m_Button3.Text = "button3";
      this.m_Button3.UseVisualStyleBackColor = false;
      this.m_Button3.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.m_Button3.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // m_Button2
      // 
      this.m_Button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Button2.AutoSize = true;
      this.m_Button2.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.m_Button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_Button2.Location = new System.Drawing.Point(261, 185);
      this.m_Button2.Name = "m_Button2";
      this.m_Button2.Size = new System.Drawing.Size(83, 26);
      this.m_Button2.TabIndex = 1;
      this.m_Button2.Text = "button2";
      this.m_Button2.UseVisualStyleBackColor = false;
      this.m_Button2.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.m_Button2.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // m_TextBox
      // 
      this.m_TextBox.AcceptsReturn = true;
      this.m_TextBox.AcceptsTab = true;
      this.m_TextBox.BackColor = System.Drawing.SystemColors.Control;
      this.m_TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_TextBox, 4);
      this.m_TextBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TextBox.Location = new System.Drawing.Point(70, 3);
      this.m_TextBox.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.m_TextBox.Multiline = true;
      this.m_TextBox.Name = "m_TextBox";
      this.m_TextBox.ReadOnly = true;
      this.m_TextBox.Size = new System.Drawing.Size(364, 176);
      this.m_TextBox.TabIndex = 3;
      this.m_TextBox.MouseEnter += new System.EventHandler(this.MouseEnterElement);
      this.m_TextBox.MouseLeave += new System.EventHandler(this.MouseLeaveElement);
      // 
      // m_Button1
      // 
      this.m_Button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_Button1.AutoSize = true;
      this.m_Button1.BackColor = System.Drawing.SystemColors.ButtonFace;
      this.m_Button1.Location = new System.Drawing.Point(172, 185);
      this.m_Button1.Name = "m_Button1";
      this.m_Button1.Size = new System.Drawing.Size(83, 26);
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
      this.m_PictureBox.Location = new System.Drawing.Point(6, 2);
      this.m_PictureBox.Margin = new System.Windows.Forms.Padding(2);
      this.m_PictureBox.Name = "m_PictureBox";
      this.m_PictureBox.Size = new System.Drawing.Size(60, 60);
      this.m_PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
      this.m_PictureBox.TabIndex = 4;
      this.m_PictureBox.TabStop = false;
      // 
      // TimedMessage
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.ClientSize = new System.Drawing.Size(446, 217);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(2);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(258, 66);
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

    private void MouseEnterElement(object? sender, EventArgs e)
    {
      m_Timer.Enabled = false;
      UpdateLabel();
    }

    private void MouseLeaveElement(object? sender, EventArgs e)
    {
      m_Timer.Enabled = true;
      UpdateLabel();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
      m_Counter++;
      UpdateLabel();

      // ReSharper disable once PossibleLossOfFraction
      if (Duration > 0 && m_Counter * m_Timer.Interval / 1000 > Duration)
      {
        AcceptButton?.PerformClick();
      }
    }

    private void UpdateLabel()
    {
      // ReSharper disable once PossibleLossOfFraction
      var display = (Duration - m_Counter * m_Timer.Interval / 1000 + .75).ToInt();
      if (!m_Timer.Enabled)
        display = 0;
      var text = string.Empty;
      if (display > 0)
      {
        if (AcceptButton == m_Button1)
          text = $@"{m_Button1.Text} in {display:N0} seconds";
        if (AcceptButton == m_Button2)
          text = $@"{m_Button2.Text} in {display:N0} seconds";
        if (AcceptButton == m_Button3)
          text = $@"{m_Button3.Text} in {display:N0} seconds";
      }

      // Handle & that is used for shortcuts
      m_LabelDefault.Text = text.Replace("&&", "￼").Replace("&", "").Replace("￼", "&");
    }
  }
}