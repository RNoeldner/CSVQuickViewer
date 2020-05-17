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
  using System.Text;
  using System.Threading;
  using System.Windows.Forms;

  /// <summary>
  ///   A Po pup Form to display progress information
  /// </summary>
  public class FormProcessDisplay : ResizeForm, IProcessDisplayTime
  {
    private readonly DummyProcessDisplay m_DummyProcessDisplay;
    private readonly LoggerDisplay m_LoggerDisplay;
    private bool m_ClosedByUI = true;

    private Label m_LabelEtl;

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
      Maximum = 0;
      SuspendLayout();
      m_TableLayoutPanel.SuspendLayout();
      if (withLoggerDisplay)
      {
        Width = 400;
        Height = 280;

        m_LoggerDisplay = new LoggerDisplay { MinLevel = Logger.Level.Debug, Dock = DockStyle.Fill, Multiline = true, TabIndex = 8 };
        m_TableLayoutPanel.Controls.Add(m_LoggerDisplay, 0, 3);
        m_TableLayoutPanel.SetColumnSpan(m_LoggerDisplay, 3);
        m_TableLayoutPanel.RowStyles[0] = new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F);
        m_TableLayoutPanel.RowStyles[3] = new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F);
      }

      // Workaround... On Windows 8 / Windows 2012 sizing is off and controls are way too big...
      if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor > 1)
      {
        m_LabelText.Height = (int)(m_LabelText.Font.SizeInPoints * 8);
        m_ProgressBar.Height = (int)(m_LabelText.Font.SizeInPoints * 3.3);
        m_LabelEtl.Height = (int)(m_LabelEtl.Font.SizeInPoints * 3.3);
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
    /// <value>The cancellation token.</value>
    public CancellationToken CancellationToken => m_DummyProcessDisplay.CancellationToken;

    /// <summary>
    ///   Gets or sets the cancellation token.
    /// </summary>
    /// <value>The cancellation token.</value>
    public CancellationTokenSource CancellationTokenSource => m_DummyProcessDisplay.CancellationTokenSource;

    public bool LogAsDebug
    {
      get => m_DummyProcessDisplay.LogAsDebug;
      set => m_DummyProcessDisplay.LogAsDebug = value;
    }

    public Logger.Level LoggerLevel
    {
      get => m_LoggerDisplay != null ? m_LoggerDisplay.MinLevel : Logger.Level.Debug;

      set
      {
        if (m_LoggerDisplay != null)
          m_LoggerDisplay.MinLevel = value;
      }
    }

    /// <summary>
    ///   Gets or sets the maximum value for the Progress
    /// </summary>
    /// <value>The maximum value.</value>
    public long Maximum
    {
      get => TimeToCompletion.TargetValue;
      set
      {
        if (value > 1)
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
          TimeToCompletion.TargetValue = 1;

          m_ProgressBar.SafeInvoke(() =>
          {
            m_ProgressBar.Maximum = 0;
            m_LabelEtl.Text = string.Empty;
            m_ProgressBar.Style = ProgressBarStyle.Marquee;
          });
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

      // This might cause an issue
      m_LabelText.SafeInvoke(
        () =>
          {
            if (!Visible)
              Show();
            m_LabelText.Text = text;

            if (value <= 0 || Maximum <= 1)
            {
              m_LabelEtl.Text = string.Empty;
            }
            else
            {
              m_ProgressBar.Value = this.TimeToCompletion.Value > this.m_ProgressBar.Maximum
                                      ? this.m_ProgressBar.Maximum
                                      : this.TimeToCompletion.Value.ToInt();
              var sb = new StringBuilder(TimeToCompletion.PercentDisplay.PadLeft(10));

              var t1 = TimeToCompletion.EstimatedTimeRemainingDisplay;
              if (t1.Length > 0)
              {
                sb.Append("   Estimated time remaining: ");
                sb.Append(t1);
              }
              m_LabelEtl.Text = sb.ToString();
            }
            m_LabelEtl.Refresh();
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
      this.m_LabelEtl = new System.Windows.Forms.Label();
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // m_ProgressBar
      this.m_ProgressBar.Location = new System.Drawing.Point(3, 48);
      this.m_ProgressBar.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_ProgressBar.Name = "m_ProgressBar";
      this.m_ProgressBar.Size = new System.Drawing.Size(471, 25);
      this.m_ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
      this.m_ProgressBar.TabIndex = 0;
      // m_LabelText
      this.m_LabelText.BackColor = System.Drawing.SystemColors.Control;
      this.m_LabelText.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_LabelText.Location = new System.Drawing.Point(5, 6);
      this.m_LabelText.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
      this.m_LabelText.MaximumSize = new System.Drawing.Size(468, 267);
      this.m_LabelText.Name = "m_LabelText";
      this.m_LabelText.Size = new System.Drawing.Size(468, 34);
      this.m_LabelText.TabIndex = 1;
      this.m_LabelText.Text = "Text\r\nLine 2";
      this.m_LabelText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // m_LabelEtl
      this.m_LabelEtl.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_LabelEtl.Location = new System.Drawing.Point(3, 78);
      this.m_LabelEtl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_LabelEtl.Name = "m_LabelEtl";
      this.m_LabelEtl.Size = new System.Drawing.Size(473, 18);
      this.m_LabelEtl.TabIndex = 6;
      this.m_LabelEtl.Text = "Estimated time remaining:";
      // m_TableLayoutPanel
      this.m_TableLayoutPanel.ColumnCount = 1;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.Controls.Add(this.m_ProgressBar, 0, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelEtl, 0, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelText, 0, 0);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 4;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 24F));
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(477, 100);
      this.m_TableLayoutPanel.TabIndex = 8;
      // FormProcessDisplay
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(477, 100);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.DoubleBuffered = true;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      this.MaximumSize = new System.Drawing.Size(495, 354);
      this.MinimumSize = new System.Drawing.Size(495, 133);
      this.Name = "FormProcessDisplay";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Process";
      this.TopMost = true;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProcessDisplay_FormClosing);
      this.m_TableLayoutPanel.ResumeLayout(false);
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

    #region IDisposable Support

    private bool m_DisposedValue; // To detect redundant calls

    // This code added to correctly implement the disposable pattern.
    public new void Dispose() =>
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);

    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      m_DisposedValue = true;
      try
      {
        if (!CancellationTokenSource.IsCancellationRequested)
          CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
        m_DummyProcessDisplay.Dispose();
        base.Dispose(disposing);
        m_LoggerDisplay.Dispose();
      }
      catch (Exception)
      {
        // Ignore
      }
    }

    #endregion IDisposable Support
  }
}