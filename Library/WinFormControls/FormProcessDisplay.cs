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

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   A Po pup Form to display progress information
  /// </summary>
  public class FormProcessDisplay : Form, IProcessDisplayTime
  {
    private bool m_ClosedByUI = true;
    private Label m_LabelEtl;
    private Label m_LabelEtr;
    private Label m_LabelPercent;
    private Label m_LabelText;
    private ProgressBar m_ProgressBar;
    protected TableLayoutPanel tableLayoutPanel;
    private string m_Title;
    private static readonly log4net.ILog m_Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public FormProcessDisplay(string windowTitle) : this(windowTitle, true, CancellationToken.None)
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
      InitializeComponent();

      m_Title = windowTitle;
      Text = windowTitle;

      CancellationTokenSource = cancellationToken == CancellationToken.None
        ? new CancellationTokenSource()
        : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

      TimeToCompletion = new TimeToCompletion();
      Icon = CsvToolLib.Resources.SubFormIcon;

      if (withLoggerDisplay)
      {
        SuspendLayout();
        Width = 400;
        Height = 300;

        var logger = new LoggerDisplay
        {
          Dock = DockStyle.Fill,
          Multiline = true,
          TabIndex = 8
        };
        tableLayoutPanel.SetColumnSpan(logger, 2);
        tableLayoutPanel.Controls.Add(logger, 0, 3);
        ResumeLayout(false);
      }
    }

    public FormProcessDisplay() : this(string.Empty, true, CancellationToken.None)
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
    public CancellationToken CancellationToken => CancellationTokenSource.Token;

    /// <summary>
    ///   Gets or sets the cancellation token.
    /// </summary>
    /// <value>
    ///   The cancellation token.
    /// </value>
    public CancellationTokenSource CancellationTokenSource { get; }

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
          m_ProgressBar.Maximum = value.ToInt();
          m_ProgressBar.Style = ProgressBarStyle.Continuous;
        }
        else
        {
          TimeToCompletion.TargetValue = -1;
          m_ProgressBar.Style = ProgressBarStyle.Marquee;
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
        Location = new Point(value.Location.X + (value.Width - Width) / 2,
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

    public bool LogAsDebug { get; set; } = false;

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
    public virtual void SetProcess(string text, long value, bool log = true)
    {
      // if cancellation is requested do nothing
      if (CancellationToken.IsCancellationRequested)
        return;
      TimeToCompletion.Value = value;
      if (log)
      {
        if (LogAsDebug)
          m_Log.Debug(text);
        else
          m_Log.Info(text);
      }
      m_LabelText.SafeInvoke(() =>
      {
        if (!Visible)
          Show();
        m_LabelText.Text = text;

        if (value <= 0)
        {
          m_LabelEtl.Visible = false;
          m_LabelPercent.Visible = false;
        }
        else
        {
          m_ProgressBar.Value = TimeToCompletion.Value.ToInt();
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

    #region Windows Form Designer generated code

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    private void InitializeComponent()
    {
      m_ProgressBar = new System.Windows.Forms.ProgressBar();
      m_LabelText = new System.Windows.Forms.Label();
      m_LabelEtr = new System.Windows.Forms.Label();
      m_LabelEtl = new System.Windows.Forms.Label();
      m_LabelPercent = new System.Windows.Forms.Label();
      tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      tableLayoutPanel.SuspendLayout();
      SuspendLayout();
      //
      // m_ProgressBar
      //
      tableLayoutPanel.SetColumnSpan(m_ProgressBar, 2);
      m_ProgressBar.Dock = System.Windows.Forms.DockStyle.Top;
      m_ProgressBar.Location = new System.Drawing.Point(4, 59);
      m_ProgressBar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      m_ProgressBar.Name = "m_ProgressBar";
      m_ProgressBar.Size = new System.Drawing.Size(453, 25);
      m_ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
      m_ProgressBar.TabIndex = 0;
      //
      // m_LabelText
      //
      tableLayoutPanel.SetColumnSpan(m_LabelText, 2);
      m_LabelText.Dock = System.Windows.Forms.DockStyle.Fill;
      m_LabelText.Location = new System.Drawing.Point(4, 0);
      m_LabelText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      m_LabelText.Name = "m_LabelText";
      m_LabelText.Size = new System.Drawing.Size(453, 55);
      m_LabelText.TabIndex = 1;
      m_LabelText.Text = "Text";
      m_LabelText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      //
      // m_LabelEtr
      //
      m_LabelEtr.Location = new System.Drawing.Point(182, 88);
      m_LabelEtr.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      m_LabelEtr.Name = "m_LabelEtr";
      m_LabelEtr.Size = new System.Drawing.Size(167, 16);
      m_LabelEtr.TabIndex = 7;
      //
      // m_LabelEtl
      //
      m_LabelEtl.AutoSize = true;
      m_LabelEtl.Location = new System.Drawing.Point(4, 88);
      m_LabelEtl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      m_LabelEtl.Name = "m_LabelEtl";
      m_LabelEtl.Size = new System.Drawing.Size(170, 17);
      m_LabelEtl.TabIndex = 6;
      m_LabelEtl.Text = "Estimated time remaining:";
      m_LabelEtl.Visible = false;
      //
      // m_LabelPercent
      //
      m_LabelPercent.Location = new System.Drawing.Point(360, 86);
      m_LabelPercent.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      m_LabelPercent.Name = "m_LabelPercent";
      m_LabelPercent.Size = new System.Drawing.Size(52, 16);
      m_LabelPercent.TabIndex = 5;
      m_LabelPercent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      //
      // tableLayoutPanel
      //
      tableLayoutPanel.ColumnCount = 2;
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      tableLayoutPanel.Controls.Add(m_ProgressBar, 0, 1);
      tableLayoutPanel.Controls.Add(m_LabelEtr, 1, 2);
      tableLayoutPanel.Controls.Add(m_LabelEtl, 0, 2);
      tableLayoutPanel.Controls.Add(m_LabelText, 0, 0);
      tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      tableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      tableLayoutPanel.Name = "tableLayoutPanel";
      tableLayoutPanel.RowCount = 4;
      tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      tableLayoutPanel.Size = new System.Drawing.Size(461, 109);
      tableLayoutPanel.TabIndex = 8;
      //
      // FormProcessDisplay
      //
      AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      ClientSize = new System.Drawing.Size(461, 109);
      Controls.Add(tableLayoutPanel);
      Controls.Add(m_LabelPercent);
      DoubleBuffered = true;
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
      MinimumSize = new System.Drawing.Size(463, 127);
      Name = "FormProcessDisplay";
      ShowIcon = false;
      ShowInTaskbar = false;
      Text = "Process";
      TopMost = true;
      FormClosing += new System.Windows.Forms.FormClosingEventHandler(ProcessDisplay_FormClosing);
      tableLayoutPanel.ResumeLayout(false);
      tableLayoutPanel.PerformLayout();
      ResumeLayout(false);
    }

    #endregion Windows Form Designer generated code
  }
}