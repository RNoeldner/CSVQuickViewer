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

using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   A Po pup Form to display progress information
  /// </summary>
  public sealed class FormProcessDisplay : ResizeForm, IProcessDisplayTime
  {
    private readonly LoggerDisplay? m_LoggerDisplay;
    private readonly ProcessDisplayTime m_ProcessDisplay;
    private Label? m_LabelEtl;
    private Label? m_LabelText;
    private ProgressBar m_ProgressBar = new ProgressBar();
    private TableLayoutPanel m_TableLayoutPanel;
    private string m_Title;

    public FormProcessDisplay(in string windowTitle)
      : this(windowTitle, true, CancellationToken.None)
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormProcessDisplay" /> class.
    /// </summary>
    /// <param name="windowTitle">The description / form title</param>
    /// <param name="withLoggerDisplay">True if a debug logging windows should be shown</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public FormProcessDisplay(in string? windowTitle, bool withLoggerDisplay, CancellationToken cancellationToken)
    {
      CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      m_ProcessDisplay = new ProcessDisplayTime();
      InitializeComponent();

      m_Title = windowTitle ?? string.Empty;
      base.Text = m_Title;

      Maximum = 0;
      SuspendLayout();
      m_TableLayoutPanel!.SuspendLayout();
      if (withLoggerDisplay)
      {
        Height += 100;

        m_LoggerDisplay = new LoggerDisplay { Dock = DockStyle.Fill, Multiline = true, TabIndex = 8 };
        m_TableLayoutPanel.Controls.Add(m_LoggerDisplay, 0, 3);
        m_LoggerDisplay.Dock = DockStyle.Fill;
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

    public TimeToCompletion TimeToCompletion => m_ProcessDisplay.TimeToCompletion;

    public event EventHandler<ProgressWithTimeEventArgs>? ProgressTime
    {
      add => m_ProcessDisplay.ProgressTime += value;
      remove => m_ProcessDisplay.ProgressTime -= value;
    }

    public event EventHandler<long>? SetMaximum
    {
      add => m_ProcessDisplay.SetMaximum += value;
      remove => m_ProcessDisplay.SetMaximum -= value;
    }

    public event EventHandler<ProgressEventArgs>? Progress
    {
      add => m_ProcessDisplay.Progress += value;
      remove => m_ProcessDisplay.Progress -= value;
    }

    /// <summary>
    ///   Gets or sets the maximum value for the Progress
    /// </summary>
    /// <value>The maximum value.</value>
    public long Maximum
    {
      get => m_ProcessDisplay.Maximum;
      set
      {
        m_ProcessDisplay.Maximum = value;
        if (value > 1)
        {
          m_ProgressBar.SafeInvoke(
            () =>
            {
              m_ProgressBar.Maximum = value.ToInt();
              m_ProgressBar.Style = ProgressBarStyle.Continuous;
            });
        }
        else
        {
          m_ProgressBar.SafeInvoke(() =>
          {
            m_ProgressBar.Maximum = 0;
            m_LabelEtl!.Text = string.Empty;
            m_ProgressBar.Style = ProgressBarStyle.Marquee;
          });
        }
      }
    }

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
    ///   Sets the process.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="value">The value.</param>
    /// <param name="log"></param>
    public void SetProcess(string text, long value, bool log)
    {
      // if cancellation is requested do nothing
      if (CancellationToken.IsCancellationRequested)
        return;
      m_ProcessDisplay.SetProcess(text, value, log);
      WindowsAPICodePackWrapper.SetProgressValue(m_ProcessDisplay.TimeToCompletion.Percent);

      // This might cause an issue
      m_LabelText!.SafeInvoke(
        () =>
        {
          if (!Visible)
            Show();
          m_LabelText!.Text = text;

          if (value <= 0 || Maximum <= 1)
          {
            m_LabelEtl!.Text = string.Empty;
            m_ProgressBar.Style = ProgressBarStyle.Marquee;
          }
          else
          {
            m_ProgressBar.Style = Maximum > 1 ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
            m_ProgressBar.Value = m_ProcessDisplay.TimeToCompletion.Value > m_ProgressBar.Maximum
                                    ? m_ProgressBar.Maximum
                                    : m_ProcessDisplay.TimeToCompletion.Value.ToInt();
            var sb = new StringBuilder();
            sb.Append(m_ProcessDisplay.TimeToCompletion.PercentDisplay.PadLeft(10));

            var t1 = m_ProcessDisplay.TimeToCompletion.EstimatedTimeRemainingDisplay;
            if (t1.Length > 0)
            {
              sb.Append("   Estimated time remaining: ");
              sb.Append(t1);
            }

            m_LabelEtl!.Text = sb.ToString();
          }

          m_LabelEtl.Refresh();
          m_LabelText.Refresh();
        });
    }

    /// <summary>
    ///   Set the progress used by Events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void SetProcess(object? sender, ProgressEventArgs e) => SetProcess(e.Text, e.Value, e.Log);

    public new void Close()
    {

      CancellationTokenSource.Cancel();
      this.SafeInvoke(() => base.Close());      
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
      this.m_ProgressBar = new ProgressBar();
      this.m_LabelText = new System.Windows.Forms.Label();
      this.m_LabelEtl = new System.Windows.Forms.Label();
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // m_ProgressBar
      this.m_ProgressBar.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_ProgressBar.Location = new System.Drawing.Point(4, 37);
      this.m_ProgressBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
      this.m_ProgressBar.Name = "m_ProgressBar";
      this.m_ProgressBar.Size = new System.Drawing.Size(354, 16);
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
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(362, 86);
      this.m_TableLayoutPanel.TabIndex = 8;
      // FormProcessDisplay
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.ClientSize = new System.Drawing.Size(362, 86);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.MinimumSize = new System.Drawing.Size(378, 45);
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

    private void ProcessDisplay_FormClosing(object? sender, FormClosingEventArgs e)
    {
      e.Cancel = false;
      try
      {
        // if the form is closed by the user (UI) signal a cancellation
        if (e.CloseReason == CloseReason.UserClosing && !CancellationTokenSource.IsCancellationRequested)
        {
          if (_MessageBox.Show("Cancel running process?", "Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

    /// <inheritdoc />
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

          CancellationTokenSource?.Dispose();
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
  }
}