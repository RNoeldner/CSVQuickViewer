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
  public sealed class FormProgress : Form, IProgressTime
  {
    private System.ComponentModel.IContainer components;
    private bool m_IsClosed;
    private bool m_IsDisposed = false;
    private Label m_LabelEtl;
    private Label m_LabelText;
    private LoggerDisplay m_LoggerDisplay;
    private ProgressBar m_ProgressBar;
    private TableLayoutPanel m_TableLayoutPanel;
    private TimeToCompletion m_TimeToCompletion;

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
      get => m_TimeToCompletion.TargetValue;
      set
      {
        m_TimeToCompletion.TargetValue = value;
        m_ProgressBar.SafeBeginInvoke(
          () =>
          {
            if (value > 1)
            {
              m_ProgressBar.Minimum = 0;
              m_ProgressBar.Maximum = value.ToInt();
              m_ProgressBar.Style = ProgressBarStyle.Continuous;
            }
            else
            {
              m_ProgressBar.Minimum = 0;
              m_ProgressBar.Maximum = 10;
              m_LabelEtl.Text = string.Empty;
              m_ProgressBar.Style = ProgressBarStyle.Marquee;
              // Task.Run(AnimateBackground, CancellationToken);
            }
          });
      }
    }

    /// <summary>
    /// Time-to-completion helper logic.
    /// </summary>
    public TimeToCompletion TimeToCompletion => m_TimeToCompletion;

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

    /// <summary>
    /// Reports progress into the form.
    /// </summary>
    public void Report(ProgressInfo info) => SetProcess(info.Value, info.Text);

    /// <summary>
    /// Sets the informational text without changing progress value.
    /// </summary>
    public void SetProcess(string text) => m_LabelText.SafeInvoke(() => m_LabelText.Text = text);

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
      components = new System.ComponentModel.Container();
      var resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProgress));
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
      m_LabelEtl.AutoSize = true;
      m_LabelEtl.BackColor = SystemColors.Control;
      m_LabelEtl.Location = new Point(4, 59);
      m_LabelEtl.Margin = new Padding(4, 3, 4, 3);
      m_LabelEtl.Name = "m_LabelEtl";
      m_LabelEtl.Size = new Size(126, 13);
      m_LabelEtl.TabIndex = 2;
      m_LabelEtl.Text = "Estimated time remaining:";
      // 
      // m_LabelText
      // 
      m_LabelText.AutoSize = true;
      m_LabelText.BackColor = SystemColors.Control;
      m_LabelText.Location = new Point(4, 3);
      m_LabelText.Margin = new Padding(4, 3, 4, 3);
      m_LabelText.MinimumSize = new Size(0, 28);
      m_LabelText.Name = "m_LabelText";
      m_LabelText.Size = new Size(70, 28);
      m_LabelText.TabIndex = 1;
      m_LabelText.Text = "Please wait...\r\n";
      // 
      // m_ProgressBar
      // 
      m_ProgressBar.Dock = DockStyle.Top;
      m_ProgressBar.Location = new Point(4, 37);
      m_ProgressBar.Margin = new Padding(4, 3, 4, 3);
      m_ProgressBar.Name = "m_ProgressBar";
      m_ProgressBar.Size = new Size(435, 16);
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
      m_TableLayoutPanel.Margin = new Padding(2);
      m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      m_TableLayoutPanel.RowCount = 4;
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
      m_TableLayoutPanel.Size = new Size(433, 215);
      m_TableLayoutPanel.TabIndex = 8;
      // 
      // m_LoggerDisplay
      // 
      m_LoggerDisplay.AllowDrop = false;
      m_LoggerDisplay.Dock = DockStyle.Fill;
      m_LoggerDisplay.Location = new Point(3, 78);
      m_LoggerDisplay.MinLevel = LogLevel.Debug;
      m_LoggerDisplay.Name = "m_LoggerDisplay";
      m_LoggerDisplay.ReadOnly = true;
      m_LoggerDisplay.Size = new Size(437, 134);
      m_LoggerDisplay.TabIndex = 3;

      // 
      // FormProgress
      // 
      AutoScaleDimensions = new SizeF(6F, 13F);
      AutoScaleMode = AutoScaleMode.Font;
      BackColor = SystemColors.Control;
      ClientSize = new Size(433, 215);
      Controls.Add(m_TableLayoutPanel);
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      Margin = new Padding(2, 3, 2, 3);
      MinimumSize = new Size(378, 45);
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
    private void SetProcess(long value, string text)
    {
      if (CancellationToken.IsCancellationRequested)
        return;

      try
      {
        // Calculate TimeRemaining
        m_TimeToCompletion.Value = value;
        WindowsAPICodePackWrapper.SetProgressValue(m_TimeToCompletion.Percent);

        // Raise event for subscribers
        ProgressChanged?.Invoke(this, new ProgressInfo(text, value));

        // Update UI elements on the UI thread
        m_LabelText.SafeBeginInvoke(
          () =>
          {
            if (!Visible)
              Show();
            m_LabelText.Text = text;

            if (value <= 0 || Maximum <= 1)
            {
              // No meaningful ETA available
              m_LabelEtl.Text = string.Empty;
            }
            else
            {
              // Update progress bar
              if (m_TimeToCompletion.Value > 0 && m_TimeToCompletion.Value <= Maximum)
                m_ProgressBar.Value = m_TimeToCompletion.Value.ToInt();

              // Build ETA text
              var sb = new StringBuilder();
              sb.Append(m_TimeToCompletion.PercentDisplay.PadLeft(10));

              var t1 = m_TimeToCompletion.EstimatedTimeRemainingDisplay;
              if (t1.Length > 0)
              {
                sb.Append("   Estimated time remaining: ");
                sb.Append(t1);
              }

              m_LabelEtl.Text = sb.ToString();
            }
            // Append log output
            m_LoggerDisplay.AddHistory(text);
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
