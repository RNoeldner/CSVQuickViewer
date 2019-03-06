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
    private Button m_BtnOk;

    public string TimeZoneID
    {
      set
      {
        timeZoneSelector1.TimeZoneID = value;
      }
      get
      {
        return timeZoneSelector1.TimeZoneID;
      }
    }

    public FormSelectTimeZone()
    {
      InitializeComponent();
    }

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
      this.m_BtnOk = new System.Windows.Forms.Button();
      this.m_BtnCancel = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.timeZoneSelector1 = new CsvTools.TimeZoneSelector();
      this.SuspendLayout();
      // 
      // m_BtnOk
      // 
      this.m_BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.m_BtnOk.Location = new System.Drawing.Point(424, 97);
      this.m_BtnOk.Name = "m_BtnOk";
      this.m_BtnOk.Size = new System.Drawing.Size(61, 23);
      this.m_BtnOk.TabIndex = 2;
      this.m_BtnOk.Text = "OK";
      this.m_BtnOk.Click += new System.EventHandler(this.BtnOK_Click);
      // 
      // m_BtnCancel
      // 
      this.m_BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_BtnCancel.Location = new System.Drawing.Point(361, 97);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new System.Drawing.Size(61, 23);
      this.m_BtnCancel.TabIndex = 1;
      this.m_BtnCancel.Text = "Cancel";
      this.m_BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 13);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(389, 39);
      this.label1.TabIndex = 5;
      this.label1.Text = "At least one column with a time has been found without time zone information.\r\n\r\n" +
    "Please determine the timezone of all date/time columns without explicit time zon" +
    "e.";
      // 
      // timeZoneSelector1
      // 
      this.timeZoneSelector1.Location = new System.Drawing.Point(16, 67);
      this.timeZoneSelector1.Name = "timeZoneSelector1";
      this.timeZoneSelector1.Size = new System.Drawing.Size(469, 29);
      this.timeZoneSelector1.TabIndex = 0;
      this.timeZoneSelector1.TimeZoneID = "(local)";
      // 
      // FormSelectTimeZone
      // 
      this.AcceptButton = this.m_BtnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.m_BtnCancel;
      this.ClientSize = new System.Drawing.Size(488, 125);
      this.ControlBox = false;
      this.Controls.Add(this.timeZoneSelector1);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.m_BtnCancel);
      this.Controls.Add(this.m_BtnOk);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.MaximumSize = new System.Drawing.Size(700, 300);
      this.MinimumSize = new System.Drawing.Size(380, 85);
      this.Name = "FormSelectTimeZone";
      this.ShowIcon = false;
      this.Text = "Select Time Zone";
      this.TopMost = true;
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
  }
}