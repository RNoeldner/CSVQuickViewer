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

#nullable enable

using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

// ReSharper disable RedundantDelegateCreation
// ReSharper disable RedundantNameQualifier

namespace CsvTools
{
  /// <summary>
  ///   A Po pup Form to display progress information
  /// </summary>
  public sealed class FormProgress : ResizeForm, IProgressTime, ILogger
  {
    private readonly LoggerDisplay? m_LoggerDisplay;
    private ProgressTime m_Progress = new();
    private Label m_LabelEtl = new();
    private Label m_LabelText = new();
    private ProgressBar m_ProgressBar = new();
    private TableLayoutPanel m_TableLayoutPanel = new();

    /// <summary>Raised for each reported progress value.</summary>
    /// <remarks>
    /// Handlers registered with this event will be invoked on the 
    /// <see cref="System.Threading.SynchronizationContext"/> captured when the instance was constructed.
    /// </remarks>
    public event EventHandler<ProgressInfo>? ProgressChanged;

    public FormProgress(in string windowTitle)
      : this(windowTitle, true, CancellationToken.None)
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormProgress" /> class.
    /// </summary>
    /// <param name="windowTitle">The description / form title</param>
    /// <param name="withLoggerDisplay">True if a debug logging windows should be shown</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public FormProgress(in string? windowTitle, bool withLoggerDisplay, in CancellationToken cancellationToken)
    {
      CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      InitializeComponent();

      Text = windowTitle ?? string.Empty;

      Maximum = 0;


      SuspendLayout();
      m_TableLayoutPanel.SuspendLayout();
      if (withLoggerDisplay)
      {
        Height += 100;

        m_LoggerDisplay = new LoggerDisplay { Dock = DockStyle.Fill, Multiline = true, TabIndex = 8 };
        m_TableLayoutPanel.Controls.Add(m_LoggerDisplay, 0, 3);
        m_LoggerDisplay.Dock = DockStyle.Fill;
      }
      else
      {
        WinAppLogging.AddLog(this);
      }

      m_TableLayoutPanel.ResumeLayout(false);
      m_TableLayoutPanel.PerformLayout();
      ResumeLayout(false);
      PerformLayout();
    }

    public FormProgress()
      : this(string.Empty, true, CancellationToken.None)
    {
    }

    /// <summary>
    ///   Gets or sets the cancellation token.
    /// </summary>
    /// <value>The cancellation token.</value>
    private CancellationTokenSource CancellationTokenSource { get; }

    public new Form? Owner
    {
      set
      {
        base.Owner = value;
        if (value is null)
          return;
        StartPosition = FormStartPosition.Manual;
        Location = new Point(
          value.Location.X + (value.Width - Width) / 2,
          value.Location.Y + (value.Height - Height) / 2);
      }
    }

    /// <summary>
    ///   Gets or sets the cancellation token.
    /// </summary>
    /// <value>The cancellation token.</value>
    public CancellationToken CancellationToken =>
      (!m_DisposedValue) ? CancellationTokenSource.Token : new CancellationToken(true);

    public TimeToCompletion TimeToCompletion => m_Progress.TimeToCompletion;


    /// <summary>
    ///   Gets or sets the maximum value for the Progress
    /// </summary>
    /// <value>The maximum value.</value>
    public long Maximum
    {
      get => m_Progress.Maximum;
      set
      {
        m_Progress.Maximum = value;
        m_ProgressBar.SafeBeginInvoke(
          () =>
          {
            if (value > 1)
            {
              m_ProgressBar.Maximum = value.ToInt();
              m_ProgressBar.Style = ProgressBarStyle.Continuous;
            }
            else
            {
              m_ProgressBar.Maximum = 0;
              m_LabelEtl.Text = string.Empty;
              m_ProgressBar.Style = ProgressBarStyle.Marquee;
            }
          });
      }
    }

    /// <summary>
    /// Sets the process values in the UI
    /// </summary>
    /// <param name="args">The <see cref="ProgressInfo"/> instance containing the event data.</param>
    private void SetProcess(in ProgressInfo args)
    {
      // if cancellation is requested do nothing
      if (CancellationToken.IsCancellationRequested)
        return;
      var value = args.Value;
      var text = args.Text;
      m_Progress.Report(args);
      WindowsAPICodePackWrapper.SetProgressValue(m_Progress.TimeToCompletion.Percent);
      ProgressChanged?.Invoke(this, args);

      // This might cause an issue
      m_LabelText.SafeBeginInvoke(
        () =>
        {
          if (!Visible)
            Show();
          m_LabelText.Text = text;

          if (value <= 0 || Maximum <= 1)
          {
            m_LabelEtl.Text = string.Empty;
            m_ProgressBar.Style = ProgressBarStyle.Marquee;
          }
          else
          {
            m_ProgressBar.Style = Maximum > 1 ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
            m_ProgressBar.Value = m_Progress.TimeToCompletion.Value > m_ProgressBar.Maximum
              ? m_ProgressBar.Maximum
              : m_Progress.TimeToCompletion.Value.ToInt();
            var sb = new StringBuilder();
            sb.Append(m_Progress.TimeToCompletion.PercentDisplay.PadLeft(10));

            var t1 = m_Progress.TimeToCompletion.EstimatedTimeRemainingDisplay;
            if (t1.Length > 0)
            {
              sb.Append("   Estimated time remaining: ");
              sb.Append(t1);
            }

            m_LabelEtl.Text = sb.ToString();
          }
        });
      Application.DoEvents();
    }


    private bool m_IsClosed;

    public new void Close()
    {
      if (m_IsClosed)
        return;
      m_IsClosed = true;
      try
      {
        CancellationTokenSource.Cancel();
        this.SafeInvoke(base.Close);
      }
      catch (ObjectDisposedException)
      {
        // ignore this
      }
    }

    /// <summary>
    ///   Sets the text only not the progress
    /// </summary>
    /// <param name="text">The text.</param>
    public void SetProcess(string text) => m_LabelText.SafeInvoke(() => m_LabelText.Text = text);

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    private void InitializeComponent()
    {
      m_Progress = new ProgressTime();
      m_LabelEtl = new Label();
      m_LabelText = new Label();
      m_ProgressBar = new ProgressBar();
      m_TableLayoutPanel = new TableLayoutPanel();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // m_ProgressBar
      this.m_ProgressBar.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_ProgressBar.Location = new System.Drawing.Point(4, 37);
      this.m_ProgressBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.m_ProgressBar.Name = "m_ProgressBar";
      this.m_ProgressBar.Size = new System.Drawing.Size(435, 16);
      this.m_ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
      this.m_ProgressBar.TabIndex = 0;
      // m_LabelText
      this.m_LabelText.AutoSize = true;
      this.m_LabelText.BackColor = System.Drawing.SystemColors.Control;
      this.m_LabelText.Location = new System.Drawing.Point(4, 3);
      this.m_LabelText.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.m_LabelText.MinimumSize = new System.Drawing.Size(0, 28);
      this.m_LabelText.Name = "m_LabelText";
      this.m_LabelText.Size = new System.Drawing.Size(70, 28);
      this.m_LabelText.TabIndex = 1;
      this.m_LabelText.Text = "Please wait...\r\n";
      // m_LabelEtl
      this.m_LabelEtl.AutoSize = true;
      this.m_LabelEtl.BackColor = System.Drawing.SystemColors.Control;
      this.m_LabelEtl.Location = new System.Drawing.Point(4, 59);
      this.m_LabelEtl.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.m_LabelEtl.Name = "m_LabelEtl";
      this.m_LabelEtl.Size = new System.Drawing.Size(126, 13);
      this.m_LabelEtl.TabIndex = 2;
      this.m_LabelEtl.Text = "Estimated time remaining:";
      // m_TableLayoutPanel
      this.m_TableLayoutPanel.BackColor = System.Drawing.Color.Transparent;
      this.m_TableLayoutPanel.ColumnCount = 1;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.Controls.Add(this.m_ProgressBar, 0, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelEtl, 0, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelText, 0, 0);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(2);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 4;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(
        new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(443, 86);
      this.m_TableLayoutPanel.TabIndex = 8;
      // Formprogress
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.ClientSize = new System.Drawing.Size(443, 86);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.MinimumSize = new System.Drawing.Size(378, 45);
      this.Name = "FormProgress";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Process";
      this.TopMost = true;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Progress_FormClosing);
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
      this.ResumeLayout(false);
    }

    private void Progress_FormClosing(object? sender, FormClosingEventArgs e)
    {
      e.Cancel = false;
      try
      {
        // if the form is closed by the user (UI) signal a cancellation
        if (e.CloseReason == CloseReason.UserClosing && !CancellationTokenSource.IsCancellationRequested)
        {
          if (MessageBox.Show("Cancel running process?", "Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
              DialogResult.Yes)
          {
            CancellationTokenSource.Cancel();
            // Give it time to stop
            Thread.Sleep(200);
          }
          else
            e.Cancel = true;
        }
      }
      catch (ObjectDisposedException)
      {
        //Ignore
      }
    }

    #region IDisposable Support

    private bool m_DisposedValue; // To detect redundant calls

    /// <inheritdoc cref="Form" />
    public new void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      try
      {
        if (disposing)
        {
          m_DisposedValue = true;
          if (!CancellationTokenSource.IsCancellationRequested)
          {
            CancellationTokenSource.Cancel();
            // Give the possibly running threads some time to exit
            Thread.Sleep(100);
          }

          CancellationTokenSource.Dispose();
          m_LoggerDisplay?.Dispose();
        }

        this.SafeBeginInvoke(() => base.Dispose(disposing));
      }
      catch (Exception)
      {
        //Ignore
      }
    }

    #endregion IDisposable Support

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
      Func<TState, Exception?, string> formatter)
    {
      if (!IsEnabled(logLevel))
        return;
      var text = formatter(state, exception);
      if (string.IsNullOrEmpty(text))
        return;
      SetProcess(new ProgressInfo(text));
    }


    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Report(ProgressInfo value) => SetProcess(value);
  }
}