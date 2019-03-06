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
  /// A Po pup Form to display progress information
  /// </summary>
  public class FormProcessDisplay_NEW : Form, IProcessDisplay
  {
    private readonly TimeToCompletion m_TimeToCompletion;
    private Label labelETL;
    private Label labelETR;
    private Label labelPercent;
    private Label labelText;
    private CancellationTokenSource m_CancellationTokenSource;
    private bool m_ClosedByUI = true;
    private ProgressBar progressBar;

    /// <summary>
    /// Initializes a new instance of the <see cref="FormProcessDisplay" /> class.
    /// </summary>
    /// <param name="windowTitle">The description / form title</param>
    /// <param name="owner">
    /// The owner form, in case the owner is minimized or closed this progress will do the same
    /// </param>
    /// <param name="cancellationTokenSource">A <see cref="CancellationTokenSource" /></param>
    public FormProcessDisplay(string windowTitle, System.Threading.CancellationToken cancellationToken)
    {
      InitializeComponent();
      CancellationToken = cancellationToken;
      m_TimeToCompletion = new TimeToCompletion();

      Text = windowTitle;
      Icon = CsvToolLib.Resources.SubFormIcon;
    }

    public FormProcessDisplay() : this(string.Empty, default(System.Threading.CancellationToken))
    {
    }

    public virtual event EventHandler<ProgressEventArgs> Progress = null;

    public virtual event EventHandler<int> SetMaximum = null;

    /// <summary>
    /// Gets or sets the cancellation token.
    /// </summary>
    /// <value>
    /// The cancellation token.
    /// </value>
    public CancellationToken CancellationToken
    {
      get
      {
        return m_CancellationTokenSource.Token;
      }
      set
      {
        if (value == System.Threading.CancellationToken.None)
          m_CancellationTokenSource = new CancellationTokenSource();
        else
          m_CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(value);
      }
    }

    /// <summary>
    /// The CancellationTokenSource of the Process Display, used to signal a cancellation by
    /// closing the process display
    /// </summary>
    public bool IsCancellationRequested
    {
      get
      {
        return m_CancellationTokenSource.IsCancellationRequested;
      }
    }

    /// <summary>
    /// Gets or sets the maximum value for the Progress
    /// </summary>
    /// <value>
    /// The maximum value.
    /// </value>
    public int Maximum
    {
      get
      {
        return m_TimeToCompletion.TargetValue;
      }
      set
      {
        m_TimeToCompletion.TargetValue = (value > 1) ? value : -1;
        progressBar.SafeInvoke(() =>
        {
          if (TimeToCompletion.TargetValue > 1)
          {
            progressBar.Maximum = TimeToCompletion.TargetValue;
            progressBar.Style = ProgressBarStyle.Continuous;
          }
          else
          {
            progressBar.Style = ProgressBarStyle.Marquee;
          }
        });
        SetMaximum?.Invoke(this, m_TimeToCompletion.TargetValue);
      }
    }

    public new Form Owner
    {
      get
      {
        return base.Owner;
      }
      set
      {
        base.Owner = value;
        if (value != null)
        {
          StartPosition = FormStartPosition.Manual;
          Location = new Point(value.Location.X + (value.Width - Width) / 2, value.Location.Y + (value.Height - Height) / 2);
        }
      }
    }

    public TimeToCompletion TimeToCompletion
    {
      get
      {
        return m_TimeToCompletion;
      }
    }

    /// <summary>
    /// Closes the form used by Events
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    public void DoClose(object sender, System.EventArgs e)
    {
      m_ClosedByUI = false;
      Close();
    }

    /// <summary>
    /// Hides the form used by Events
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    public void DoHide(object sender, System.EventArgs e)
    {
      Hide();
    }


    /// <summary>
    /// Sets the process.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="value">The value.</param>
    public void SetProcess(string text, int value = -1)
    {
      SetProcess(text, value);

      // if cancellation is requested do nothing
      if (!IsCancellationRequested)
      {
        TimeToCompletion.Value = value;
        this.SafeInvoke(() =>
        {
          if (!Visible)
            Show();
          labelText.Text = text;
          if (TimeToCompletion.Value > 0)
          {
            progressBar.Value = TimeToCompletion.Value;
            labelPercent.Text = TimeToCompletion.PercentDisplay;
            labelETR.Text = TimeToCompletion.EstimatedTimeRemainingDisplay;
            labelETL.Visible = (labelETR.Text.Length > 0);
          }
        });
      }
      Progress?.Invoke(this, new ProgressEventArgs(text, value));
    }

    /// <summary>
    /// Set the progress used by Events
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void SetProcess(object sender, ProgressEventArgs e)
    {
      if (e == null) return;
      SetProcess(e.Text ?? string.Empty, e.Value);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      m_CancellationTokenSource.Dispose();
      base.Dispose(disposing);
    }

    private void ProcessDisplay_FormClosing(object sender, FormClosingEventArgs e)
    {
      try
      {
        // if the form is closed by the user (UI) signal a cancellation
        if (m_ClosedByUI && !m_CancellationTokenSource.IsCancellationRequested)
          m_CancellationTokenSource.Cancel();
      }
      catch (ObjectDisposedException)
      {
      }
    }

    #region Windows Form Designer generated code

    public void RefreshScreen()
    {
      Application.DoEvents();
    }

    /// <summary>
    /// Required method for Designer support - do not modify the contents of this method with the
    /// code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.progressBar = new System.Windows.Forms.ProgressBar();
      this.labelText = new System.Windows.Forms.Label();
      this.labelPercent = new System.Windows.Forms.Label();
      this.labelETL = new System.Windows.Forms.Label();
      this.labelETR = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // progressBar
      // 
      this.progressBar.Location = new System.Drawing.Point(0, 48);
      this.progressBar.Name = "progressBar";
      this.progressBar.Size = new System.Drawing.Size(309, 20);
      this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
      this.progressBar.TabIndex = 0;
      // 
      // labelText
      // 
      this.labelText.Dock = System.Windows.Forms.DockStyle.Top;
      this.labelText.Location = new System.Drawing.Point(0, 0);
      this.labelText.Name = "labelText";
      this.labelText.Size = new System.Drawing.Size(309, 45);
      this.labelText.TabIndex = 1;
      this.labelText.Text = "Text";
      this.labelText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // labelPercent
      // 
      this.labelPercent.Location = new System.Drawing.Point(265, 71);
      this.labelPercent.Name = "labelPercent";
      this.labelPercent.Size = new System.Drawing.Size(39, 13);
      this.labelPercent.TabIndex = 2;
      this.labelPercent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
      // 
      // labelETL
      // 
      this.labelETL.AutoSize = true;
      this.labelETL.Location = new System.Drawing.Point(2, 71);
      this.labelETL.Name = "labelETL";
      this.labelETL.Size = new System.Drawing.Size(126, 13);
      this.labelETL.TabIndex = 3;
      this.labelETL.Text = "Estimated time remaining:";
      this.labelETL.Visible = false;
      // 
      // labelETR
      // 
      this.labelETR.Location = new System.Drawing.Point(126, 71);
      this.labelETR.Name = "labelETR";
      this.labelETR.Size = new System.Drawing.Size(125, 13);
      this.labelETR.TabIndex = 4;
      // 
      // FormProcessDisplay
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(309, 86);
      this.Controls.Add(this.labelETR);
      this.Controls.Add(this.labelETL);
      this.Controls.Add(this.labelPercent);
      this.Controls.Add(this.progressBar);
      this.Controls.Add(this.labelText);
      this.DoubleBuffered = true;
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      this.Name = "FormProcessDisplay";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "Process";
      this.TopMost = true;
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProcessDisplay_FormClosing);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion Windows Form Designer generated code
  }
}