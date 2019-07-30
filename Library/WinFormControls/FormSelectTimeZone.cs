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
using System.Windows.Forms;

namespace CsvTools
{
  public sealed class FormSelectTimeZone : Form
  {
    private Button m_BtnCancel;
    private Label label1;
    private TimeZoneSelector timeZoneSelector1;
    private Timer timer;
    private System.ComponentModel.IContainer components;
    private Label label;
    private Button m_BtnOk;

    private int m_Counter = 0;
    private readonly double m_Duration = 5.0;

    private void UpdateLabel()
    {
      var displ = Convert.ToInt32((m_Duration - (m_Counter * timer.Interval) / 1000 + .75));
      if (displ > 0)
      {
        label.Text = $"OK in {displ:N0} seconds";
      }
      else
        label.Text = string.Empty;
    }

    public string TimeZoneID
    {
      set => timeZoneSelector1.TimeZoneID = value;
      get => timeZoneSelector1.TimeZoneID;
    }

    public FormSelectTimeZone() => InitializeComponent();

    public FormSelectTimeZone(string title)
    {
      InitializeComponent();
      Text = title;
    }

    /// <summary>
    ///   Required method for Designer support - do not modify
    ///   the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.components = new System.ComponentModel.Container();
            this.m_BtnOk = new System.Windows.Forms.Button();
            this.m_BtnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.timeZoneSelector1 = new CsvTools.TimeZoneSelector();
            this.label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // m_BtnOk
            // 
            this.m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_BtnOk.Location = new System.Drawing.Point(432, 121);
            this.m_BtnOk.Name = "m_BtnOk";
            this.m_BtnOk.Size = new System.Drawing.Size(72, 32);
            this.m_BtnOk.TabIndex = 2;
            this.m_BtnOk.Text = "&OK";
            this.m_BtnOk.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // m_BtnCancel
            // 
            this.m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_BtnCancel.Location = new System.Drawing.Point(516, 121);
            this.m_BtnCancel.Name = "m_BtnCancel";
            this.m_BtnCancel.Size = new System.Drawing.Size(72, 32);
            this.m_BtnCancel.TabIndex = 1;
            this.m_BtnCancel.Text = "&Cancel";
            this.m_BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(581, 60);
            this.label1.TabIndex = 5;
            this.label1.Text = "At least one column with a time has been found without time zone information.\r\n\r\n" +
    "Please determine the timezone of all date/time columns without explicit time zon" +
    "e.";
            // 
            // timer
            // 
            this.timer.Enabled = true;
            this.timer.Interval = 500;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // timeZoneSelector1
            // 
            this.timeZoneSelector1.Location = new System.Drawing.Point(11, 85);
            this.timeZoneSelector1.Margin = new System.Windows.Forms.Padding(4);
            this.timeZoneSelector1.Name = "timeZoneSelector1";
            this.timeZoneSelector1.Size = new System.Drawing.Size(577, 29);
            this.timeZoneSelector1.TabIndex = 0;
            this.timeZoneSelector1.TimeZoneID = "(local)";
            // 
            // label
            // 
            this.label.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label.AutoSize = true;
            this.label.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label.Location = new System.Drawing.Point(10, 132);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(154, 20);
            this.label.TabIndex = 12;
            this.label.Text = "Default in 5 seconds";
            // 
            // FormSelectTimeZone
            // 
            this.AcceptButton = this.m_BtnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_BtnCancel;
            this.ClientSize = new System.Drawing.Size(603, 157);
            this.ControlBox = false;
            this.Controls.Add(this.label);
            this.Controls.Add(this.timeZoneSelector1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_BtnCancel);
            this.Controls.Add(this.m_BtnOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximumSize = new System.Drawing.Size(697, 296);
            this.MinimumSize = new System.Drawing.Size(377, 81);
            this.Name = "FormSelectTimeZone";
            this.Text = "Select Time Zone";
            this.TopMost = true;
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FormSelectTimeZone_MouseMove);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void timer_Tick(object sender, EventArgs e)
    {
      m_Counter++;
      UpdateLabel();
      if (m_Duration > 0 && (m_Counter * timer.Interval) / 1000 > m_Duration)
      {
        BtnOK_Click(sender, e);
      }
    }

    private void FormSelectTimeZone_MouseMove(object sender, MouseEventArgs e) => timer.Enabled = false;
  }
}