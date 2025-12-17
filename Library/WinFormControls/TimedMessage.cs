/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
 * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License along with this program.
 * If not, see http://www.gnu.org/licenses/ .
 *
 */
#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace CsvTools;

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class TimedMessage : ResizeForm
{
  private Button m_Button1;

  private Button m_Button2;

  private Button m_Button3;

  private int m_Counter;

  private Label m_LabelDefault;

  private PictureBox m_PictureBox;

  private TableLayoutPanel m_TableLayoutPanel;

  private TextBox m_TextBox;
  private Timer m_Timer = new Timer();
  private WebBrowser? m_WebBrowser;

#pragma warning disable CS8618
  public TimedMessage() => InitializeComponent();
#pragma warning restore CS8618


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
        m_WebBrowser.IsWebBrowserContextMenuEnabled = false;
        m_WebBrowser.ScriptErrorsSuppressed = true;
        m_WebBrowser.Dock = DockStyle.Fill;
      });
    }
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      components?.Dispose();
      m_WebBrowser?.Dispose();
    }

    base.Dispose(disposing);
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
      try
      {
        var resources = new ComponentResourceManager(typeof(TimedMessage));
        m_PictureBox.Image = icon switch
        {
          MessageBoxIcon.Information => resources.GetObject("info") as Image,
          MessageBoxIcon.Warning => resources.GetObject("warning") as Image,
          MessageBoxIcon.Question => resources.GetObject("question") as Image,
          MessageBoxIcon.Error => resources.GetObject("error") as Image,
          _ => m_PictureBox.Image
        };
      }
      catch
      {
      }
    }

    TopLevel = true;
    return ShowDialog();
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
  [SuppressMessage("ReSharper", "RedundantNameQualifier")]
  [SuppressMessage("ReSharper", "RedundantCast")]
  [SuppressMessage("ReSharper", "RedundantDelegateCreation")]
  [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
  private void InitializeComponent()
  {
    components = new Container();
    m_Timer = new Timer(components);
    m_TableLayoutPanel = new TableLayoutPanel();
    m_LabelDefault = new Label();
    m_Button3 = new Button();
    m_Button2 = new Button();
    m_TextBox = new TextBox();
    m_Button1 = new Button();
    m_PictureBox = new PictureBox();
    m_TableLayoutPanel.SuspendLayout();
    ((ISupportInitialize) m_PictureBox).BeginInit();
    SuspendLayout();
    // 
    // m_Timer
    // 
    m_Timer.Enabled = true;
    m_Timer.Interval = 500;
    m_Timer.Tick += Timer_Tick;
    // 
    // m_TableLayoutPanel
    // 
    m_TableLayoutPanel.AutoSize = true;
    m_TableLayoutPanel.BackColor = Color.Transparent;
    m_TableLayoutPanel.ColumnCount = 5;
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    m_TableLayoutPanel.Controls.Add(m_LabelDefault, 0, 1);
    m_TableLayoutPanel.Controls.Add(m_Button3, 4, 1);
    m_TableLayoutPanel.Controls.Add(m_Button2, 3, 1);
    m_TableLayoutPanel.Controls.Add(m_TextBox, 1, 0);
    m_TableLayoutPanel.Controls.Add(m_Button1, 2, 1);
    m_TableLayoutPanel.Controls.Add(m_PictureBox, 0, 0);
    m_TableLayoutPanel.Dock = DockStyle.Fill;
    m_TableLayoutPanel.Location = new Point(0, 0);
    m_TableLayoutPanel.Margin = new Padding(2);
    m_TableLayoutPanel.Name = "m_TableLayoutPanel";
    m_TableLayoutPanel.Padding = new Padding(4, 0, 10, 3);
    m_TableLayoutPanel.RowCount = 2;
    m_TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
    m_TableLayoutPanel.RowStyles.Add(new RowStyle());
    m_TableLayoutPanel.Size = new Size(446, 217);
    m_TableLayoutPanel.TabIndex = 5;
    // 
    // m_LabelDefault
    // 
    m_LabelDefault.BackColor = Color.Transparent;
    m_TableLayoutPanel.SetColumnSpan(m_LabelDefault, 2);
    m_LabelDefault.Dock = DockStyle.Fill;
    m_LabelDefault.ForeColor = SystemColors.InfoText;
    m_LabelDefault.Location = new Point(6, 182);
    m_LabelDefault.Margin = new Padding(2, 0, 2, 0);
    m_LabelDefault.Name = "m_LabelDefault";
    m_LabelDefault.Size = new Size(161, 32);
    m_LabelDefault.TabIndex = 2;
    m_LabelDefault.Text = "Default in 5 seconds";
    m_LabelDefault.TextAlign = ContentAlignment.MiddleLeft;
    // 
    // m_Button3
    // 
    m_Button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
    m_Button3.AutoSize = true;
    m_Button3.BackColor = SystemColors.ButtonFace;
    m_Button3.Location = new Point(350, 186);
    m_Button3.Name = "m_Button3";
    m_Button3.Size = new Size(83, 25);
    m_Button3.TabIndex = 2;
    m_Button3.Text = "button3";
    m_Button3.UseVisualStyleBackColor = false;
    m_Button3.MouseEnter += MouseEnterElement;
    m_Button3.MouseLeave += MouseLeaveElement;
    // 
    // m_Button2
    // 
    m_Button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
    m_Button2.AutoSize = true;
    m_Button2.BackColor = SystemColors.ButtonFace;
    m_Button2.DialogResult = DialogResult.Cancel;
    m_Button2.Location = new Point(261, 186);
    m_Button2.Name = "m_Button2";
    m_Button2.Size = new Size(83, 25);
    m_Button2.TabIndex = 1;
    m_Button2.Text = "button2";
    m_Button2.UseVisualStyleBackColor = false;
    m_Button2.MouseEnter += MouseEnterElement;
    m_Button2.MouseLeave += MouseLeaveElement;
    // 
    // m_TextBox
    // 
    m_TextBox.AcceptsReturn = true;
    m_TextBox.AcceptsTab = true;
    m_TextBox.BackColor = SystemColors.Control;
    m_TextBox.BorderStyle = BorderStyle.None;
    m_TableLayoutPanel.SetColumnSpan(m_TextBox, 4);
    m_TextBox.Dock = DockStyle.Fill;
    m_TextBox.Location = new Point(70, 3);
    m_TextBox.Margin = new Padding(2, 3, 2, 3);
    m_TextBox.Multiline = true;
    m_TextBox.Name = "m_TextBox";
    m_TextBox.ReadOnly = true;
    m_TextBox.Size = new Size(364, 176);
    m_TextBox.TabIndex = 3;
    m_TextBox.MouseEnter += MouseEnterElement;
    m_TextBox.MouseLeave += MouseLeaveElement;
    // 
    // m_Button1
    // 
    m_Button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
    m_Button1.AutoSize = true;
    m_Button1.BackColor = SystemColors.ButtonFace;
    m_Button1.Location = new Point(172, 186);
    m_Button1.Name = "m_Button1";
    m_Button1.Size = new Size(83, 25);
    m_Button1.TabIndex = 0;
    m_Button1.Text = "button1";
    m_Button1.UseVisualStyleBackColor = false;
    m_Button1.MouseEnter += MouseEnterElement;
    m_Button1.MouseLeave += MouseLeaveElement;
    // 
    // m_PictureBox
    // 
    m_PictureBox.BackgroundImageLayout = ImageLayout.Zoom;
    m_PictureBox.ErrorImage = null;
    m_PictureBox.InitialImage = null;
    m_PictureBox.Location = new Point(6, 2);
    m_PictureBox.Margin = new Padding(2);
    m_PictureBox.Name = "m_PictureBox";
    m_PictureBox.Size = new Size(60, 60);
    m_PictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
    m_PictureBox.TabIndex = 4;
    m_PictureBox.TabStop = false;
    // 
    // TimedMessage
    // 
    AutoScaleDimensions = new SizeF(6F, 13F);
    AutoScaleMode = AutoScaleMode.Dpi;
    AutoSize = true;
    BackColor = SystemColors.Control;
    ClientSize = new Size(446, 217);
    Controls.Add(m_TableLayoutPanel);
    FormBorderStyle = FormBorderStyle.SizableToolWindow;
    Margin = new Padding(2);
    MaximizeBox = false;
    MinimizeBox = false;
    MinimumSize = new Size(258, 66);
    Name = "TimedMessage";
    ShowIcon = false;
    ShowInTaskbar = false;
    SizeGripStyle = SizeGripStyle.Show;
    StartPosition = FormStartPosition.CenterParent;
    Text = "Timed Message";
    TopMost = true;
    m_TableLayoutPanel.ResumeLayout(false);
    m_TableLayoutPanel.PerformLayout();
    ((ISupportInitialize) m_PictureBox).EndInit();
    ResumeLayout(false);
    PerformLayout();
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

  private IContainer components;
}