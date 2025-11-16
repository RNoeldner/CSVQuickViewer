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

namespace CsvTools
{
  /// <summary>
  ///   Popup form showing progress and optional log output.
  /// </summary>
  public sealed class FormProgress : Form, IProgressTime, IProgressWithCancellation
  {
    private System.ComponentModel.IContainer components;
    private bool m_IsClosed;
    private bool m_IsDisposed = false;
    private Label m_LabelEtl;
    private Label m_LabelText;
    private LoggerDisplay m_LoggerDisplay;
    private ProgressBar m_ProgressBar;
    private TableLayoutPanel m_TableLayoutPanel;

    /// <summary>
    /// Creates a new progress form.
    /// </summary>
    /// <param name="windowTitle">Title of the window.</param>
    /// <param name="cancellationToken">Token to cancel long-running operations.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public FormProgress(string windowTitle = "Progress", CancellationToken cancellationToken = default)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
      CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      InitializeComponent();

      Text = windowTitle ?? string.Empty;
      Maximum = 0;
    }

    /// <summary>
    /// Event raised whenever progress changes.
    /// </summary>
    public event EventHandler<ProgressInfo>? ProgressChanged;

    /// <summary>
    /// Exposes the cancellation token for the running operation.
    /// </summary>
    public CancellationToken CancellationToken =>
      m_IsDisposed ? new CancellationToken(true) : CancellationTokenSource.Token;

    /// <summary>
    /// Maximum value of the progress bar.
    /// </summary>
    public long Maximum
    {
      get => TimeToCompletion.TargetValue;
      set
      {
        if (value<0)
          value=0;
        if (TimeToCompletion.TargetValue == value)
          return;

        TimeToCompletion.TargetValue = value;
        m_ProgressBar.SafeBeginInvoke(
          () =>
          {
            if (value > 1)
            {
              m_ProgressBar.Minimum = 0;
              m_ProgressBar.Maximum = (value < int.MaxValue) ? (int) value : int.MaxValue;
              m_ProgressBar.Style = ProgressBarStyle.Continuous;
            }
            else
            {
              m_ProgressBar.Minimum = 0;
              m_ProgressBar.Maximum = 10;
              m_LabelEtl.Text = string.Empty;
              m_ProgressBar.Style = ProgressBarStyle.Marquee;
            }
          });
      }
    }

    /// <summary>
    /// Time-to-completion helper logic.
    /// </summary>
    public TimeToCompletion TimeToCompletion { get; private set; } = new TimeToCompletion();

    /// <summary>
    ///   Gets or sets the cancellation token.
    /// </summary>
    /// <info>The cancellation token.</info>
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
        // ignore this
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
          CancellationTokenSource.Dispose();


          base.Dispose(disposing);
        }
      }
      catch (Exception)
      {
        //Ignore
      }
    }

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    private void InitializeComponent()
    {
      m_LabelEtl = new Label();
      m_LabelText = new Label();
      m_ProgressBar = new ProgressBar();
      m_TableLayoutPanel = new TableLayoutPanel();
      m_LoggerDisplay = new LoggerDisplay();
      m_TableLayoutPanel.SuspendLayout();
      SuspendLayout();
      // 
      // m_LabelEtl
      // 
      m_LabelEtl.Dock = DockStyle.Fill;
      m_LabelEtl.Location = new Point(3, 56);
      m_LabelEtl.Name = "m_LabelEtl";
      m_LabelEtl.Size = new Size(448, 23);
      m_LabelEtl.TabIndex = 1;
      m_LabelEtl.Text = "Estimated time remaining:";
      // 
      // m_LabelText
      // 
      m_LabelText.AutoSize = true;
      m_LabelText.BackColor = SystemColors.Control;
      m_LabelText.Dock = DockStyle.Fill;
      m_LabelText.Location = new Point(3, 0);
      m_LabelText.MinimumSize = new Size(0, 28);
      m_LabelText.Name = "m_LabelText";
      m_LabelText.Size = new Size(448, 28);
      m_LabelText.TabIndex = 2;
      m_LabelText.Text = "Please wait...\r\n";
      // 
      // m_ProgressBar
      // 
      m_ProgressBar.Dock = DockStyle.Top;
      m_ProgressBar.Location = new Point(3, 31);
      m_ProgressBar.Name = "m_ProgressBar";
      m_ProgressBar.Size = new Size(448, 22);
      m_ProgressBar.Style = ProgressBarStyle.Marquee;
      m_ProgressBar.TabIndex = 0;
      // 
      // m_TableLayoutPanel
      // 
      m_TableLayoutPanel.BackColor = Color.Transparent;
      m_TableLayoutPanel.ColumnCount = 1;
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.Controls.Add(m_ProgressBar, 0, 1);
      m_TableLayoutPanel.Controls.Add(m_LabelEtl, 0, 2);
      m_TableLayoutPanel.Controls.Add(m_LabelText, 0, 0);
      m_TableLayoutPanel.Controls.Add(m_LoggerDisplay, 0, 3);
      m_TableLayoutPanel.Dock = DockStyle.Fill;
      m_TableLayoutPanel.Location = new Point(0, 0);
      m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      m_TableLayoutPanel.RowCount = 4;
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
      m_TableLayoutPanel.Size = new Size(454, 214);
      m_TableLayoutPanel.TabIndex = 0;
      // 
      // m_LoggerDisplay
      // 
      m_LoggerDisplay.Dock = DockStyle.Fill;
      m_LoggerDisplay.Location = new Point(3, 82);
      m_LoggerDisplay.MinLevel = LogLevel.Debug;
      m_LoggerDisplay.Multiline = true;
      m_LoggerDisplay.Name = "m_LoggerDisplay";
      m_LoggerDisplay.ReadOnly = true;
      m_LoggerDisplay.ScrollBars = ScrollBars.Vertical;
      m_LoggerDisplay.Size = new Size(448, 129);
      m_LoggerDisplay.TabIndex = 1;
      // 
      // FormProgress
      // 
      AutoScaleDimensions = new SizeF(6F, 13F);
      AutoScaleMode = AutoScaleMode.Font;
      BackColor = SystemColors.Control;
      ClientSize = new Size(454, 214);
      Controls.Add(m_TableLayoutPanel);
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      Margin = new Padding(2, 3, 2, 3);
      MinimumSize = new Size(470, 114);
      Name = "FormProgress";
      ShowIcon = false;
      ShowInTaskbar = false;
      Text = "Process";
      FormClosing += Progress_FormClosing;
      m_TableLayoutPanel.ResumeLayout(false);
      m_TableLayoutPanel.PerformLayout();
      ResumeLayout(false);
    }

    /// <summary>
    /// Handles closing of the form and triggers cancellation if the user closes it.
    /// </summary>
    private void Progress_FormClosing(object? sender, FormClosingEventArgs e)
    {
      e.Cancel = false;
      try
      {
        // if the form is closed by the user (UI) signal a cancellation
        if (e.CloseReason == CloseReason.UserClosing && !m_IsDisposed &&
            !CancellationTokenSource.IsCancellationRequested)
        {
          CancellationTokenSource.Cancel();
          // Give it time to stop
          Thread.Sleep(100);
        }
      }
      catch (ObjectDisposedException)
      {
        //Ignore
      }
    }

    /// <summary>
    /// Updates progress, ETA text, and logger output.
    /// </summary>
    public void Report(ProgressInfo prof)
    {
      if (CancellationToken.IsCancellationRequested)
        return;

      try
      {
        // Calculate TimeRemaining
        TimeToCompletion.Value = prof.Value;
        WindowsAPICodePackWrapper.SetProgressValue(TimeToCompletion.Percent);

        // Raise event for subscribers
        ProgressChanged?.Invoke(this, prof);

        // Update UI elements on the UI thread
        m_LabelText.SafeBeginInvoke(
          () =>
          {
            if (!Visible)
            {
              Show();
              Extensions.ProcessUIElements();
            }

            m_LabelText.Text = prof.Text;

            if (TimeToCompletion.Value <= 0 || Maximum <= 1)
            {
              // No meaningful ETA available
              m_LabelEtl.Text = string.Empty;
            }
            else
            {
              // Update progress bar
              if (TimeToCompletion.Value > 0 && TimeToCompletion.Value <= Maximum)
                m_ProgressBar.Value = TimeToCompletion.Value.ToInt();

              // Build ETA text
              var sb = new StringBuilder();
              sb.Append(TimeToCompletion.PercentDisplay.PadLeft(10));

              var t1 = TimeToCompletion.EstimatedTimeRemainingDisplay;
              if (t1.Length > 0)
              {
                sb.Append("   Estimated time remaining: ");
                sb.Append(t1);
              }

              m_LabelEtl.Text = sb.ToString();
            }
            // Append log output
            m_LoggerDisplay.AddHistory(prof.Text);
          });
      }
      finally
      {
        // Ensures UI refresh
        Extensions.ProcessUIElements();
      }
    }
  }
}
