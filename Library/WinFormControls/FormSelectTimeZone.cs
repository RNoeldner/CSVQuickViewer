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

namespace CsvTools
{
  using System;
  using System.ComponentModel;
  using System.Drawing;
  using System.Windows.Forms;

  public sealed class FormSelectTimeZone : Form
  {
    private readonly double m_Duration = 5.0;

    private IContainer components;

    private Label label;

    private Label label1;

    private Button m_BtnCancel;

    private Button m_BtnOk;

    private int m_Counter;

    private TableLayoutPanel tableLayoutPanel1;

    private Timer timer;

    private TimeZoneSelector timeZoneSelector1;

    public FormSelectTimeZone() => InitializeComponent();

    public FormSelectTimeZone(string title)
    {
      InitializeComponent();
      Text = title;
    }

    public string TimeZoneID
    {
      get => timeZoneSelector1.TimeZoneID;
      set => this.timeZoneSelector1.TimeZoneID = value;
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
      Close();
    }

    private void FormSelectTimeZone_MouseMove(object sender, MouseEventArgs e) => timer.Enabled = false;

    /// <summary>
    ///   Required method for Designer support - do not modify
    ///   the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new Container();
      this.m_BtnOk = new Button();
      this.m_BtnCancel = new Button();
      this.label1 = new Label();
      this.timer = new Timer(this.components);
      this.timeZoneSelector1 = new TimeZoneSelector();
      this.label = new Label();
      this.tableLayoutPanel1 = new TableLayoutPanel();
      this.tableLayoutPanel1.SuspendLayout();
      this.SuspendLayout();

      // m_BtnOk
      this.m_BtnOk.AutoSize = true;
      this.m_BtnOk.DialogResult = DialogResult.OK;
      this.m_BtnOk.Location = new Point(431, 87);
      this.m_BtnOk.Name = "m_BtnOk";
      this.m_BtnOk.Size = new Size(102, 34);
      this.m_BtnOk.TabIndex = 2;
      this.m_BtnOk.Text = "&OK";
      this.m_BtnOk.Click += new EventHandler(this.BtnOK_Click);

      // m_BtnCancel
      this.m_BtnCancel.AutoSize = true;
      this.m_BtnCancel.DialogResult = DialogResult.Cancel;
      this.m_BtnCancel.Location = new Point(539, 87);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new Size(102, 34);
      this.m_BtnCancel.TabIndex = 1;
      this.m_BtnCancel.Text = "&Cancel";
      this.m_BtnCancel.Click += new EventHandler(this.BtnCancel_Click);

      // label1
      this.label1.AutoSize = true;
      this.tableLayoutPanel1.SetColumnSpan(this.label1, 3);
      this.label1.Dock = DockStyle.Top;
      this.label1.Location = new Point(3, 0);
      this.label1.Name = "label1";
      this.label1.Size = new Size(638, 40);
      this.label1.TabIndex = 5;
      this.label1.Text = "At least one column with a time has been found without time zone information.\r\nPl"
                         + "ease determine the timezone of all date/time columns without explicit time zone."
                         + string.Empty;

      // timer
      this.timer.Enabled = true;
      this.timer.Interval = 500;
      this.timer.Tick += new EventHandler(this.timer_Tick);

      // timeZoneSelector1
      this.tableLayoutPanel1.SetColumnSpan(this.timeZoneSelector1, 3);
      this.timeZoneSelector1.Dock = DockStyle.Top;
      this.timeZoneSelector1.Location = new Point(3, 43);
      this.timeZoneSelector1.Name = "timeZoneSelector1";
      this.timeZoneSelector1.Size = new Size(638, 38);
      this.timeZoneSelector1.TabIndex = 0;
      this.timeZoneSelector1.TimeZoneID = "(local)";

      // label
      this.label.Anchor = AnchorStyles.Left;
      this.label.AutoSize = true;
      this.label.ForeColor = SystemColors.ControlDarkDark;
      this.label.Location = new Point(3, 94);
      this.label.Name = "label";
      this.label.Size = new Size(154, 20);
      this.label.TabIndex = 12;
      this.label.Text = "Default in 5 seconds";

      // tableLayoutPanel1
      this.tableLayoutPanel1.AutoSize = true;
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
      this.tableLayoutPanel1.Controls.Add(this.m_BtnCancel, 2, 2);
      this.tableLayoutPanel1.Controls.Add(this.label, 0, 2);
      this.tableLayoutPanel1.Controls.Add(this.m_BtnOk, 1, 2);
      this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.timeZoneSelector1, 0, 1);
      this.tableLayoutPanel1.Dock = DockStyle.Top;
      this.tableLayoutPanel1.Location = new Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new RowStyle());
      this.tableLayoutPanel1.Size = new Size(644, 124);
      this.tableLayoutPanel1.TabIndex = 13;

      // FormSelectTimeZone
      this.AcceptButton = this.m_BtnOk;
      this.AutoScaleDimensions = new SizeF(9F, 20F);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.CancelButton = this.m_BtnCancel;
      this.ClientSize = new Size(644, 132);
      this.Controls.Add(this.tableLayoutPanel1);
      this.FormBorderStyle = FormBorderStyle.SizableToolWindow;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormSelectTimeZone";
      this.Text = "Select Time Zone";
      this.TopMost = true;
      this.MouseMove += new MouseEventHandler(this.FormSelectTimeZone_MouseMove);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.tableLayoutPanel1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    private void timer_Tick(object sender, EventArgs e)
    {
      m_Counter++;
      UpdateLabel();
      if (m_Duration > 0 && m_Counter * timer.Interval / 1000 > m_Duration)
      {
        BtnOK_Click(sender, e);
      }
    }

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
  }
}