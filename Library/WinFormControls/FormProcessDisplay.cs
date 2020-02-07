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

      m_Title = windowTitle;
      base.Text = windowTitle;

      TimeToCompletion = new TimeToCompletion();
      if (withLoggerDisplay)
      {
        SuspendLayout();
        Width = 400;
        Height = 300;

        m_LoggerDisplay = new LoggerDisplay { Dock = DockStyle.Fill, Multiline = true, TabIndex = 8 };

        m_TableLayoutPanel.SetColumnSpan(m_LoggerDisplay, 2);
        m_TableLayoutPanel.Controls.Add(m_LoggerDisplay, 0, 3);
        ResumeLayout(false);
      }
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
      m_ProgressBar = new ProgressBar();
      m_LabelText = new Label();
      m_LabelEtr = new Label();
      m_LabelEtl = new Label();
      m_LabelPercent = new Label();
      m_TableLayoutPanel = new TableLayoutPanel();
      m_TableLayoutPanel.SuspendLayout();
      SuspendLayout();

      // m_ProgressBar
      m_TableLayoutPanel.SetColumnSpan(m_ProgressBar, 2);
      m_ProgressBar.Dock = DockStyle.Top;
      m_ProgressBar.Location = new Point(4, 74);
      m_ProgressBar.Margin = new Padding(4, 5, 4, 5);
      m_ProgressBar.Name = "m_ProgressBar";
      m_ProgressBar.Size = new Size(514, 31);
      m_ProgressBar.Style = ProgressBarStyle.Marquee;
      m_ProgressBar.TabIndex = 0;

      // m_LabelText
      m_TableLayoutPanel.SetColumnSpan(m_LabelText, 2);
      m_LabelText.Dock = DockStyle.Fill;
      m_LabelText.Location = new Point(4, 0);
      m_LabelText.Margin = new Padding(4, 0, 4, 0);
      m_LabelText.Name = "m_LabelText";
      m_LabelText.Size = new Size(514, 69);
      m_LabelText.TabIndex = 1;
      m_LabelText.Text = "Text";
      m_LabelText.TextAlign = ContentAlignment.MiddleLeft;

      // m_LabelEtr
      m_LabelEtr.Anchor = AnchorStyles.Left;
      m_LabelEtr.Location = new Point(204, 117);
      m_LabelEtr.Margin = new Padding(4, 0, 4, 0);
      m_LabelEtr.Name = "m_LabelEtr";
      m_LabelEtr.Size = new Size(188, 20);
      m_LabelEtr.TabIndex = 7;

      // m_LabelEtl
      m_LabelEtl.Anchor = AnchorStyles.Right;
      m_LabelEtl.AutoSize = true;
      m_LabelEtl.Location = new Point(4, 117);
      m_LabelEtl.Margin = new Padding(4, 0, 4, 0);
      m_LabelEtl.Name = "m_LabelEtl";
      m_LabelEtl.Size = new Size(192, 20);
      m_LabelEtl.TabIndex = 6;
      m_LabelEtl.Text = "Estimated time remaining:";
      m_LabelEtl.Visible = false;

      // m_LabelPercent
      m_LabelPercent.Location = new Point(405, 108);
      m_LabelPercent.Margin = new Padding(4, 0, 4, 0);
      m_LabelPercent.Name = "m_LabelPercent";
      m_LabelPercent.Size = new Size(58, 20);
      m_LabelPercent.TabIndex = 5;
      m_LabelPercent.TextAlign = ContentAlignment.MiddleCenter;

      // tableLayoutPanel
      m_TableLayoutPanel.ColumnCount = 2;
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.Controls.Add(m_ProgressBar, 0, 1);
      m_TableLayoutPanel.Controls.Add(m_LabelEtr, 1, 2);
      m_TableLayoutPanel.Controls.Add(m_LabelEtl, 0, 2);
      m_TableLayoutPanel.Controls.Add(m_LabelText, 0, 0);
      m_TableLayoutPanel.Dock = DockStyle.Fill;
      m_TableLayoutPanel.Location = new Point(0, 0);
      m_TableLayoutPanel.Margin = new Padding(4, 5, 4, 5);
      m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      m_TableLayoutPanel.RowCount = 3;
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.Size = new Size(522, 145);
      m_TableLayoutPanel.TabIndex = 8;

      // FormProcessDisplay
      AutoScaleDimensions = new SizeF(9F, 20F);
      AutoScaleMode = AutoScaleMode.Font;
      ClientSize = new Size(522, 145);
      Controls.Add(m_TableLayoutPanel);
      Controls.Add(m_LabelPercent);
      DoubleBuffered = true;
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      Margin = new Padding(4, 5, 4, 5);
      MinimumSize = new Size(511, 131);
      Name = "FormProcessDisplay";
      ShowIcon = false;
      ShowInTaskbar = false;
      Text = "Process";
      TopMost = true;
      FormClosing += new FormClosingEventHandler(ProcessDisplay_FormClosing);
      m_TableLayoutPanel.ResumeLayout(false);
      m_TableLayoutPanel.PerformLayout();
      ResumeLayout(false);
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