/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
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

namespace CsvTools
{
  using System;
  using System.Drawing;
  using System.Threading;
  using System.Windows.Forms;

  /// <summary>
  ///   A Po pup Form to display progress information
  /// </summary>
  public class FormProcessDisplay : Form, IProcessDisplayTime
  {
    private readonly DummyProcessDisplay m_DummyProcessDisplay;
    private readonly LoggerDisplay m_LoggerDisplay;
    private bool m_ClosedByUI = true;

    private Label m_LabelEtl;

    private Label m_LabelEtr;

    private Label m_LabelPercent;

    private Label m_LabelText;

    private ProgressBar m_ProgressBar;

    private TableLayoutPanel m_TableLayoutPanel;
    private string m_Title;

    public FormProcessDisplay(string windowTitle)
      : this(windowTitle, true, CancellationToken.None)
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormProcessDisplay" /> class.
    /// </summary>
    /// <param name="windowTitle">The description / form title</param>
    /// <param name="withLoggerDisplay">True if a debug logging windows should be shown</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public FormProcessDisplay(string windowTitle, bool withLoggerDisplay, CancellationToken cancellationToken)
    {
      m_DummyProcessDisplay = new DummyProcessDisplay(cancellationToken);
      InitializeComponent();

      m_LabelPercent.Text = string.Empty;
      m_LabelEtr.Text = string.Empty;

      m_Title = windowTitle;
      base.Text = windowTitle;

      TimeToCompletion = new TimeToCompletion();
      Maximum = 0;
      SuspendLayout();
      m_TableLayoutPanel.SuspendLayout();
      if (withLoggerDisplay)
      {
        Width = 400;
        Height = 260;

        m_LoggerDisplay = new LoggerDisplay { MinLevel = Logger.Level.Debug, Dock = DockStyle.Fill, Multiline = true, TabIndex = 8 };
        m_TableLayoutPanel.Controls.Add(m_LoggerDisplay, 0, 3);
        m_TableLayoutPanel.SetColumnSpan(m_LoggerDisplay, 3);
        m_TableLayoutPanel.RowStyles[0] = new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F);
        m_TableLayoutPanel.RowStyles[3] = new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65F);
      }

      // Workaround... On Windows 8 / Windows 2012 sizing is off and controls are way too big...
      if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor > 1)
      {
        m_LabelText.Height = (int)(m_LabelText.Font.SizeInPoints * 8);
        m_ProgressBar.Height = (int)(m_LabelText.Font.SizeInPoints * 3.3);
        m_LabelEtl.Height = (int)(m_LabelEtl.Font.SizeInPoints * 3.3);
        m_LabelEtr.Height = (int)(m_LabelEtr.Font.SizeInPoints * 3.3);
      }
      m_TableLayoutPanel.ResumeLayout(false);
      m_TableLayoutPanel.PerformLayout();
      ResumeLayout(false);
      PerformLayout();
    }

    public FormProcessDisplay()
      : this(string.Empty, true, CancellationToken.None)
    {
    }

    /// <summary>
    ///   Event handler called as progress should be displayed
    /// </summary>
    public event EventHandler<ProgressEventArgs> Progress;

    public virtual event EventHandler<long> SetMaximum;

    /// <summary>
    ///   Gets or sets the cancellation token.
    /// </summary>
    /// <value>
    ///   The cancellation token.
    /// </value>
    public CancellationToken CancellationToken => m_DummyProcessDisplay.CancellationToken;

    /// <summary>
    ///   Gets or sets the cancellation token.
    /// </summary>
    /// <value>
    ///   The cancellation token.
    /// </value>
    public CancellationTokenSource CancellationTokenSource => m_DummyProcessDisplay.CancellationTokenSource;

    public bool LogAsDebug
    {
      get => m_DummyProcessDisplay.LogAsDebug;
      set => m_DummyProcessDisplay.LogAsDebug = value;
    }

    public Logger.Level LoggerLevel
    {
      get
      {
        if (m_LoggerDisplay != null)
          return m_LoggerDisplay.MinLevel;
        else
          return Logger.Level.Debug;
      }

      set
      {
        if (m_LoggerDisplay != null)
          m_LoggerDisplay.MinLevel = value;
      }
    }

    /// <summary>
    ///   Gets or sets the maximum value for the Progress
    /// </summary>
    /// <value>
    ///   The maximum value.
    /// </value>
    public long Maximum
    {
      get => TimeToCompletion.TargetValue;
      set
      {
        if (value > 0)
        {
          TimeToCompletion.TargetValue = value;
          m_ProgressBar.SafeInvoke(
            () =>
              {
                m_ProgressBar.Maximum = value.ToInt();
                m_ProgressBar.Style = ProgressBarStyle.Continuous;
              });
        }
        else
        {
          TimeToCompletion.TargetValue = -1;
          m_ProgressBar.SafeInvoke(() => { m_ProgressBar.Style = ProgressBarStyle.Marquee; });
        }

        SetMaximum?.Invoke(this, TimeToCompletion.TargetValue);
      }
    }

    public new Form Owner
    {
      get => base.Owner;
      set
      {
        base.Owner = value;
        if (value == null)
          return;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(
          value.Location.X + (value.Width - Width) / 2,
          value.Location.Y + (value.Height - Height) / 2);
      }
    }

    public TimeToCompletion TimeToCompletion { get; }

    public string Title
    {
      get => m_Title;
      set
      {
        var newVal = value ?? string.Empty;
        if (newVal.Equals(m_Title, StringComparison.Ordinal))
          return;
        m_Title = newVal;
        this.SafeInvoke(() => { Text = m_Title; });
      }
    }

    /// <summary>
    ///   Closes the form used by Events
    /// </summary>
    public void Cancel()
    {
      m_ClosedByUI = false;
      CancellationTokenSource.Cancel();
      Close();
    }

    /// <summary>
    ///   Hides the form used by Events
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    public void DoHide(object sender, EventArgs e) => Hide();

    /// <summary>
    ///   Sets the process.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="value">The value.</param>
    /// <param name="log"></param>
    public virtual void SetProcess(string text, long value, bool log)
    {
      // if cancellation is requested do nothing
      if (CancellationToken.IsCancellationRequested)
        return;
      TimeToCompletion.Value = value;

      m_DummyProcessDisplay.SetProcess(text, value, log);

      m_LabelText.SafeInvoke(
        () =>
          {
            if (!Visible)
              Show();
            m_LabelText.Text = text;

            if (value <= 0 || Maximum == 0)
            {
              m_LabelEtl.Visible = false;
              m_LabelPercent.Visible = false;
            }
            else
            {
              m_ProgressBar.Value = (TimeToCompletion.Value > m_ProgressBar.Maximum
                                       ? m_ProgressBar.Maximum
                                       : TimeToCompletion.Value.ToInt());
              m_LabelPercent.Text = TimeToCompletion.PercentDisplay;
              m_LabelEtr.Text = TimeToCompletion.EstimatedTimeRemainingDisplay;
              m_LabelEtl.Visible = m_LabelEtr.Text.Length > 0;
              m_LabelPercent.Visible = m_LabelEtr.Text.Length > 0;
              m_LabelEtr.Refresh();
              m_LabelPercent.Refresh();
            }

            m_LabelText.Refresh();
          });
      Progress?.Invoke(this, new ProgressEventArgs(text, value));
    }

    /// <summary>
    ///   Set the progress used by Events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void SetProcess(object sender, ProgressEventArgs e)
    {
      if (e == null)
        return;
      SetProcess(e.Text, e.Value, e.Log);
    }

    /// <summary>
    ///   Sets the process.
    /// </summary>
    /// <param name="text">The text.</param>
    public void SetProcess(string text) => SetProcess(text, -1, true);

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.m_ProgressBar = new System.Windows.Forms.ProgressBar();
      this.m_LabelText = new System.Windows.Forms.Label();
      this.m_LabelEtr = new System.Windows.Forms.Label();
      this.m_LabelEtl = new System.Windows.Forms.Label();
      this.m_LabelPercent = new System.Windows.Forms.Label();
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_ProgressBar
      // 
      this.m_TableLayoutPanel.SetColumnSpan(this.m_ProgressBar, 3);
      this.m_ProgressBar.Location = new System.Drawing.Point(3, 60);
      this.m_ProgressBar.Name = "m_ProgressBar";
      this.m_ProgressBar.Size = new System.Drawing.Size(471, 24);
      this.m_ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
      this.m_ProgressBar.TabIndex = 0;
      // 
      // m_LabelText
      // 
      this.m_LabelText.BackColor = System.Drawing.SystemColors.Control;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_LabelText, 3);
      this.m_LabelText.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_LabelText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_LabelText.Location = new System.Drawing.Point(6, 6);
      this.m_LabelText.Margin = new System.Windows.Forms.Padding(6);
      this.m_LabelText.MaximumSize = new System.Drawing.Size(468, 267);
      this.m_LabelText.Name = "m_LabelText";
      this.m_LabelText.Size = new System.Drawing.Size(468, 45);
      this.m_LabelText.TabIndex = 1;
      this.m_LabelText.Text = "Text\r\nLine 2";
      this.m_LabelText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // m_LabelEtr
      // 
      this.m_LabelEtr.AutoSize = true;
      this.m_LabelEtr.Location = new System.Drawing.Point(203, 90);
      this.m_LabelEtr.Margin = new System.Windows.Forms.Padding(3);
      this.m_LabelEtr.Name = "m_LabelEtr";
      this.m_LabelEtr.Size = new System.Drawing.Size(48, 17);
      this.m_LabelEtr.TabIndex = 7;
      this.m_LabelEtr.Text = "0:1:17";
      // 
      // m_LabelEtl
      // 
      this.m_LabelEtl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.m_LabelEtl.Location = new System.Drawing.Point(19, 90);
      this.m_LabelEtl.Margin = new System.Windows.Forms.Padding(3);
      this.m_LabelEtl.Name = "m_LabelEtl";
      this.m_LabelEtl.Size = new System.Drawing.Size(178, 17);
      this.m_LabelEtl.TabIndex = 6;
      this.m_LabelEtl.Text = "Estimated time remaining:";
      this.m_LabelEtl.Visible = false;
      // 
      // m_LabelPercent
      // 
      this.m_LabelPercent.AutoSize = true;
      this.m_LabelPercent.Location = new System.Drawing.Point(283, 90);
      this.m_LabelPercent.Margin = new System.Windows.Forms.Padding(3);
      this.m_LabelPercent.Name = "m_LabelPercent";
      this.m_LabelPercent.Size = new System.Drawing.Size(36, 17);
      this.m_LabelPercent.TabIndex = 5;
      this.m_LabelPercent.Text = "50%";
      // 
      // m_TableLayoutPanel
      // 
      this.m_TableLayoutPanel.ColumnCount = 3;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 200F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.Controls.Add(this.m_ProgressBar, 0, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelPercent, 2, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelEtl, 0, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelText, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelEtr, 1, 2);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 4;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(480, 117);
      this.m_TableLayoutPanel.TabIndex = 8;
      // 
      // FormProcessDisplay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(480, 117);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.DoubleBuffered = true;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(4);
      this.MaximumSize = new System.Drawing.Size(498, 361);
      this.MinimumSize = new System.Drawing.Size(498, 140);
      this.Name = "FormProcessDisplay";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Process";
      this.TopMost = true;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProcessDisplay_FormClosing);
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
      this.ResumeLayout(false);

    }

    private void ProcessDisplay_FormClosing(object sender, FormClosingEventArgs e)
    {
      try
      {
        // if the form is closed by the user (UI) signal a cancellation
        if (CancellationTokenSource != null && m_ClosedByUI)
          CancellationTokenSource.Cancel();
      }
      catch (ObjectDisposedException)
      {
      }
    }
  }
}