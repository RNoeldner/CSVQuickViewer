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

using CsvToolLib;
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

    public FormProcessDisplay(string windowTitle) : this(windowTitle, CancellationToken.None)
    {      
    }
    
    /// <summary>
    ///   Initializes a new instance of the <see cref="FormProcessDisplay" /> class.
    /// </summary>
    /// <param name="windowTitle">The description / form title</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public FormProcessDisplay(string windowTitle, CancellationToken cancellationToken)
    {
      InitializeComponent();
      m_Title = windowTitle;
      Text = windowTitle;
      CancellationTokenSource = cancellationToken == CancellationToken.None
        ? new CancellationTokenSource()
        : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

      //m_TimeToCompletion = new TimeToCompletion(displayFunction: TimeToCompletion.DefaultDisplayPercentAndSpanSolo);

      TimeToCompletion = new TimeToCompletion();
      //m_TimeToCompletion.PropertyChanged += TimeToCompletionUpdate;

      Icon = Resources.SubFormIcon;
    }

    public FormProcessDisplay() : this(string.Empty, default(CancellationToken))
    {
    }

    /// <summary>
    ///   Event handler called as progress should be displayed
    /// </summary>
    public event EventHandler<ProgressEventArgs> Progress;

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
    public int Maximum
    {
      get => TimeToCompletion.TargetValue;
      set
      {
        if (value > 0)
        {
          TimeToCompletion.TargetValue = value;
          m_ProgressBar.Maximum = value;
          m_ProgressBar.Style = ProgressBarStyle.Continuous;
        }
        else
        {
          TimeToCompletion.TargetValue = -1;
          m_ProgressBar.Style = ProgressBarStyle.Marquee;
        }
      }
    }

    public new Form Owner
    {
      get => base.Owner;
      set
      {
        base.Owner = value;
        if (value == null) return;
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
        if (newVal.Equals(m_Title, StringComparison.Ordinal)) return;
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
    public void DoHide(object sender, EventArgs e)
    {
      Hide();
    }

    /// <summary>
    ///   Sets the process.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="value">The value.</param>
    public virtual void SetProcess(string text, int value)
    {
      // if cancellation is requested do nothing
      if (CancellationToken.IsCancellationRequested) return;
      TimeToCompletion.Value = value;

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
          m_ProgressBar.Value = TimeToCompletion.Value;
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
      if (e == null) return;
      SetProcess(e.Text, e.Value);
    }

    /// <summary>
    ///   Sets the process.
    /// </summary>
    /// <param name="text">The text.</param>
    public void SetProcess(string text)
    {
      SetProcess(text, -1);
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

    #region Windows Form Designer generated code

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
      this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.tableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_ProgressBar
      // 
      this.tableLayoutPanel.SetColumnSpan(this.m_ProgressBar, 2);
      this.m_ProgressBar.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_ProgressBar.Location = new System.Drawing.Point(3, 48);
      this.m_ProgressBar.Name = "m_ProgressBar";
      this.m_ProgressBar.Size = new System.Drawing.Size(340, 20);
      this.m_ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
      this.m_ProgressBar.TabIndex = 0;
      // 
      // m_LabelText
      // 
      this.tableLayoutPanel.SetColumnSpan(this.m_LabelText, 2);
      this.m_LabelText.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_LabelText.Location = new System.Drawing.Point(3, 0);
      this.m_LabelText.Name = "m_LabelText";
      this.m_LabelText.Size = new System.Drawing.Size(340, 45);
      this.m_LabelText.TabIndex = 1;
      this.m_LabelText.Text = "Text";
      this.m_LabelText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // m_LabelEtr
      // 
      this.m_LabelEtr.Location = new System.Drawing.Point(135, 71);
      this.m_LabelEtr.Name = "m_LabelEtr";
      this.m_LabelEtr.Size = new System.Drawing.Size(125, 13);
      this.m_LabelEtr.TabIndex = 7;
      // 
      // m_LabelEtl
      // 
      this.m_LabelEtl.AutoSize = true;
      this.m_LabelEtl.Location = new System.Drawing.Point(3, 71);
      this.m_LabelEtl.Name = "m_LabelEtl";
      this.m_LabelEtl.Size = new System.Drawing.Size(126, 13);
      this.m_LabelEtl.TabIndex = 6;
      this.m_LabelEtl.Text = "Estimated time remaining:";
      this.m_LabelEtl.Visible = false;
      // 
      // m_LabelPercent
      // 
      this.m_LabelPercent.Location = new System.Drawing.Point(270, 70);
      this.m_LabelPercent.Name = "m_LabelPercent";
      this.m_LabelPercent.Size = new System.Drawing.Size(39, 13);
      this.m_LabelPercent.TabIndex = 5;
      this.m_LabelPercent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // tableLayoutPanel
      // 
      this.tableLayoutPanel.ColumnCount = 2;
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel.Controls.Add(this.m_ProgressBar, 0, 1);
      this.tableLayoutPanel.Controls.Add(this.m_LabelEtr, 1, 2);
      this.tableLayoutPanel.Controls.Add(this.m_LabelEtl, 0, 2);
      this.tableLayoutPanel.Controls.Add(this.m_LabelText, 0, 0);
      this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel.Name = "tableLayoutPanel";
      this.tableLayoutPanel.RowCount = 4;
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel.Size = new System.Drawing.Size(346, 84);
      this.tableLayoutPanel.TabIndex = 8;
      // 
      // FormProcessDisplay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(346, 84);
      this.Controls.Add(this.tableLayoutPanel);
      this.Controls.Add(this.m_LabelPercent);
      this.DoubleBuffered = true;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MinimumSize = new System.Drawing.Size(352, 112);
      this.Name = "FormProcessDisplay";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Process";
      this.TopMost = true;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProcessDisplay_FormClosing);
      this.tableLayoutPanel.ResumeLayout(false);
      this.tableLayoutPanel.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion Windows Form Designer generated code
  }
}