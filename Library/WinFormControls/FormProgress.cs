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

using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CsvTools;

/// <summary>
///    Popup form showing progress and optional log output.
/// </summary>
public sealed class FormProgress : ResizeForm, IProgressTime, IProgressWithCancellation
{
  private Label labelProgressMetrics;
  private Label labelText;
  private LoggerDisplay loggerDisplay;
  private volatile bool m_IsClosed;
  private volatile bool m_IsDisposed;
  private ProgressBar progressBar;
  private TableLayoutPanel tableLayoutPanel;

  /// <summary>
  /// Occurs whenever a new progress value is reported.
  /// </summary>
  public event EventHandler<ProgressChangedEventArgs>? ProgressChanged;

  /// <summary>
  /// Creates a new progress form.
  /// </summary>
  /// <param name="windowTitle">Title of the window.</param>
  /// <param name="cancellationToken">Token to cancel long-running operations.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
  public FormProgress(string windowTitle = "Progress", CancellationToken cancellationToken = default)
#pragma warning restore CS8618 
  {
    CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    InitializeComponent();
    TopMost = true;
    Text = windowTitle ?? string.Empty;
    Maximum = 0;
  }

  /// <summary>
  /// Exposes the cancellation token for the running operation safely.
  /// </summary>
  public CancellationToken CancellationToken
  {
    get
    {
      if (m_IsDisposed || m_IsClosed)
        return new CancellationToken(canceled: true);

      try
      {
        return CancellationTokenSource.Token;
      }
      catch (ObjectDisposedException)
      {
        return new CancellationToken(canceled: true);
      }
    }
  }

  /// <summary>
  /// Maximum value of the progress bar.
  /// </summary>
  public long Maximum
  {
    get => TimeToCompletion.TargetValue;
    set
    {
      if (value < 0)
        value = 0;
      if (TimeToCompletion.TargetValue == value)
        return;

      TimeToCompletion.TargetValue = value;
      progressBar.SafeBeginInvoke(
        () =>
        {
          if (m_IsDisposed || IsDisposed) return;

          if (value > 1)
          {
            progressBar.Minimum = 0;
            progressBar.Maximum = (value < int.MaxValue) ? (int) value : int.MaxValue;
            progressBar.Style = ProgressBarStyle.Continuous;
          }
          else
          {
            progressBar.Minimum = 0;
            progressBar.Maximum = 10;
            labelProgressMetrics.Text = string.Empty;
            progressBar.Style = ProgressBarStyle.Marquee;
          }
        });
    }
  }

  /// <summary>
  /// Time-to-completion helper logic.
  /// </summary>
  public TimeToCompletion TimeToCompletion { get; private set; } = new TimeToCompletion();

  /// <summary>
  ///    Gets the linked cancellation token source.
  /// </summary>
  private CancellationTokenSource CancellationTokenSource { get; }

  /// <summary>
  /// Closes the form and triggers cancellation of the process.
  /// </summary>
  public new void Close()
  {
    if (m_IsClosed)
      return;
    m_IsClosed = true;
    try
    {
      CancellationTokenSource.Cancel();
      base.Close();
    }
    catch (ObjectDisposedException)
    {
      // Ignore safely
    }
  }

  /// <summary>
  /// Updates progress, ETA text, and logger output.
  /// </summary>
  public void Report(ProgressInfo progressInfo)
  {
    if (CancellationToken.IsCancellationRequested || m_IsClosed || m_IsDisposed)
      return;

    Logger.Information(progressInfo.Text);
    try
    {
      TimeToCompletion.Value = progressInfo.Value;
      WindowsAPICodePackWrapper.SetProgressValue(TimeToCompletion.Percent);

      // Thread-safe snapshot local assignment copy to prevent cross-thread race conditions
      var localHandler = ProgressChanged;
      localHandler?.Invoke(this, new ProgressChangedEventArgs(progressInfo.Text, TimeToCompletion.Percent, TimeToCompletion.EstimatedTimeRemaining));

      labelText.SafeBeginInvoke(
        () =>
        {
          if (m_IsDisposed || m_IsClosed || IsDisposed) return;
          if (!Visible)
          {
            Show();
            Extensions.ProcessUIElements();
          }

          labelText.Text = progressInfo.Text;

          if (TimeToCompletion.Value <= 0 || Maximum <= 1)
          {
            labelProgressMetrics.Text = string.Empty;
          }
          else
          {
            if (TimeToCompletion.Value > 0 && TimeToCompletion.Value <= Maximum)
            {
              int rawValue = TimeToCompletion.Value.ToInt();

              if (rawValue >= progressBar.Minimum && rawValue <= progressBar.Maximum)
                progressBar.Value = rawValue;
              else if (rawValue > progressBar.Maximum)
                progressBar.Value = progressBar.Maximum;
              else
                progressBar.Value = progressBar.Minimum;
            }

            var sb = new StringBuilder();
            sb.Append(TimeToCompletion.PercentDisplay.PadLeft(10));

            var t1 = TimeToCompletion.EstimatedTimeRemainingDisplay;
            if (t1.Length > 0)
            {
              sb.Append("   Estimated time remaining: ").Append(t1);
            }

            labelProgressMetrics.Text = sb.ToString();
          }
          loggerDisplay.AddHistory(progressInfo.Text);
        });
    }
    catch (InvalidOperationException)
    {
      // Protect against rare asynchronous cross-thread window handle disposal races safely
    }
    finally
    {
      if (!m_IsDisposed && !m_IsClosed)
      {
        Extensions.ProcessUIElements();
      }
    }
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing)
  {
    if (m_IsDisposed)
      return;
    m_IsDisposed = disposing;
    try
    {
      CancellationTokenSource.Cancel();

      if (disposing)
      {
        // base.Dispose is invoked first to safely dismantle UI elements and layout containers
        // before clearing thread-blocking backing synchronization tokens.
        base.Dispose(disposing);
        CancellationTokenSource.Dispose();
      }
    }
    catch
    {
      // Ignore finalization state faults safely
    }
  }

  private void InitializeComponent()
  {
    labelProgressMetrics = new Label();
    labelText = new Label();
    progressBar = new ProgressBar();
    tableLayoutPanel = new TableLayoutPanel();
    loggerDisplay = new LoggerDisplay();
    tableLayoutPanel.SuspendLayout();
    SuspendLayout();
    // 
    // labelProgressMetrics
    // 
    labelProgressMetrics.Dock = DockStyle.Fill;
    labelProgressMetrics.Location = new Point(3, 56);
    labelProgressMetrics.Name = "labelProgressMetrics";
    labelProgressMetrics.Size = new Size(448, 23);
    labelProgressMetrics.TabIndex = 1;
    labelProgressMetrics.Text = "Estimated time remaining:";
    // 
    // labelText
    // 
    labelText.AutoSize = true;
    labelText.BackColor = SystemColors.Control;
    labelText.Dock = DockStyle.Fill;
    labelText.Location = new Point(3, 0);
    labelText.MinimumSize = new Size(0, 28);
    labelText.Name = "labelText";
    labelText.Size = new Size(448, 28);
    labelText.TabIndex = 2;
    labelText.Text = "Please wait...\r\n";
    // 
    // progressBar
    // 
    progressBar.Dock = DockStyle.Top;
    progressBar.Location = new Point(3, 31);
    progressBar.Name = "progressBar";
    progressBar.Size = new Size(448, 22);
    progressBar.Style = ProgressBarStyle.Marquee;
    progressBar.TabIndex = 0;
    // 
    // tableLayoutPanel
    // 
    tableLayoutPanel.BackColor = Color.Transparent;
    tableLayoutPanel.ColumnCount = 1;
    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
    tableLayoutPanel.Controls.Add(progressBar, 0, 1);
    tableLayoutPanel.Controls.Add(labelProgressMetrics, 0, 2);
    tableLayoutPanel.Controls.Add(labelText, 0, 0);
    tableLayoutPanel.Controls.Add(loggerDisplay, 0, 3);
    tableLayoutPanel.Dock = DockStyle.Fill;
    tableLayoutPanel.Location = new Point(0, 0);
    tableLayoutPanel.Name = "tableLayoutPanel";
    tableLayoutPanel.RowCount = 4;
    tableLayoutPanel.RowStyles.Add(new RowStyle());
    tableLayoutPanel.RowStyles.Add(new RowStyle());
    tableLayoutPanel.RowStyles.Add(new RowStyle());
    tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
    tableLayoutPanel.Size = new Size(454, 214);
    tableLayoutPanel.TabIndex = 0;
    // 
    // loggerDisplay
    // 
    loggerDisplay.Dock = DockStyle.Fill;
    loggerDisplay.Location = new Point(3, 82);
    loggerDisplay.MinLevel = LogLevel.Debug;
    loggerDisplay.Multiline = true;
    loggerDisplay.Name = "loggerDisplay";
    loggerDisplay.ReadOnly = true;
    loggerDisplay.ScrollBars = ScrollBars.Vertical;
    loggerDisplay.Size = new Size(448, 129);
    loggerDisplay.TabIndex = 1;
    // 
    // FormProgress
    // 
    BackColor = SystemColors.Control;
    ClientSize = new Size(454, 214);
    Controls.Add(tableLayoutPanel);
    FormBorderStyle = FormBorderStyle.SizableToolWindow;
    Margin = new Padding(2, 3, 2, 3);
    MinimumSize = new Size(470, 114);
    Name = "FormProgress";
    ShowIcon = false;
    ShowInTaskbar = false;
    Text = "Process";
    FormClosing += Progress_FormClosing;
    tableLayoutPanel.ResumeLayout(performLayout: false);
    tableLayoutPanel.PerformLayout();
    ResumeLayout(performLayout: false);
  }

  private void Progress_FormClosing(object? sender, FormClosingEventArgs e)
  {
    e.Cancel = false;
    try
    {
      if (e.CloseReason == CloseReason.UserClosing && !m_IsDisposed && !CancellationTokenSource.IsCancellationRequested)
      {
        CancellationTokenSource.Cancel();
      }
    }
    catch (ObjectDisposedException)
    {
      // Ignore safely
    }
  }
}