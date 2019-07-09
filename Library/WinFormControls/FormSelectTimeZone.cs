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
    private Button m_BtnOk;

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
      components = new System.ComponentModel.Container();
      m_BtnOk = new System.Windows.Forms.Button();
      m_BtnCancel = new System.Windows.Forms.Button();
      label1 = new System.Windows.Forms.Label();
      timer = new System.Windows.Forms.Timer(components);
      timeZoneSelector1 = new CsvTools.TimeZoneSelector();
      SuspendLayout();
      //
      // m_BtnOk
      //
      m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      m_BtnOk.Location = new System.Drawing.Point(356, 84);
      m_BtnOk.Name = "m_BtnOk";
      m_BtnOk.Size = new System.Drawing.Size(72, 23);
      m_BtnOk.TabIndex = 2;
      m_BtnOk.Text = "&OK";
      m_BtnOk.Click += new System.EventHandler(BtnOK_Click);
      //
      // m_BtnCancel
      //
      m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      m_BtnCancel.Location = new System.Drawing.Point(433, 84);
      m_BtnCancel.Name = "m_BtnCancel";
      m_BtnCancel.Size = new System.Drawing.Size(72, 23);
      m_BtnCancel.TabIndex = 1;
      m_BtnCancel.Text = "&Cancel";
      m_BtnCancel.Click += new System.EventHandler(BtnCancel_Click);
      //
      // label1
      //
      label1.AutoSize = true;
      label1.Location = new System.Drawing.Point(7, 9);
      label1.Name = "label1";
      label1.Size = new System.Drawing.Size(389, 39);
      label1.TabIndex = 5;
      label1.Text = "At least one column with a time has been found without time zone information.\r\n\r\n" +
    "Please determine the timezone of all date/time columns without explicit time zon" +
    "e.";
      //
      // timer
      //
      timer.Enabled = true;
      timer.Interval = 10000;
      timer.Tick += new System.EventHandler(timer_Tick);
      //
      // timeZoneSelector1
      //
      timeZoneSelector1.Location = new System.Drawing.Point(6, 55);
      timeZoneSelector1.Name = "timeZoneSelector1";
      timeZoneSelector1.Size = new System.Drawing.Size(500, 29);
      timeZoneSelector1.TabIndex = 0;
      timeZoneSelector1.TimeZoneID = "(local)";
      //
      // FormSelectTimeZone
      //
      AcceptButton = m_BtnOk;
      AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      CancelButton = m_BtnCancel;
      ClientSize = new System.Drawing.Size(511, 111);
      ControlBox = false;
      Controls.Add(timeZoneSelector1);
      Controls.Add(label1);
      Controls.Add(m_BtnCancel);
      Controls.Add(m_BtnOk);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      MaximumSize = new System.Drawing.Size(700, 300);
      MinimumSize = new System.Drawing.Size(380, 85);
      Name = "FormSelectTimeZone";
      Text = "Select Time Zone";
      TopMost = true;
      MouseMove += new System.Windows.Forms.MouseEventHandler(FormSelectTimeZone_MouseMove);
      ResumeLayout(false);
      PerformLayout();
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

    private void timer_Tick(object sender, EventArgs e) => BtnOK_Click(sender, e);

    private void FormSelectTimeZone_MouseMove(object sender, MouseEventArgs e) => timer.Enabled = false;
  }
}