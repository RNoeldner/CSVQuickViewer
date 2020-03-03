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

  public sealed class FormSelectTimeZone : ResizeForm
  {
    private readonly double m_Duration = 5.0;

    private IContainer components;

    private Label label;

    private Label label1;

    private Button m_BtnCancel;

    private Button m_BtnOk;

    private int m_Counter;

    private TableLayoutPanel m_TableLayoutPanel;

    private Timer timer;

    private TimeZoneSelector m_TimeZoneSelector;

    public FormSelectTimeZone() : this("Timezone")
    {
    }

    public FormSelectTimeZone(string title)
    {
      InitializeComponent();
      Text = title;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
    public string TimeZoneID
    {
      get => m_TimeZoneSelector.TimeZoneID;
      set => m_TimeZoneSelector.TimeZoneID = value;
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
      this.components = new System.ComponentModel.Container();
      this.m_BtnOk = new System.Windows.Forms.Button();
      this.m_BtnCancel = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.timer = new System.Windows.Forms.Timer(this.components);
      this.m_TimeZoneSelector = new CsvTools.TimeZoneSelector();
      this.label = new System.Windows.Forms.Label();
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_BtnOk
      // 
      this.m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.m_BtnOk.Location = new System.Drawing.Point(416, 81);
      this.m_BtnOk.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.m_BtnOk.Name = "m_BtnOk";
      this.m_BtnOk.Size = new System.Drawing.Size(111, 28);
      this.m_BtnOk.TabIndex = 2;
      this.m_BtnOk.Text = "&OK";
      this.m_BtnOk.Click += new System.EventHandler(this.BtnOK_Click);
      // 
      // m_BtnCancel
      // 
      this.m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_BtnCancel.Location = new System.Drawing.Point(531, 81);
      this.m_BtnCancel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new System.Drawing.Size(111, 28);
      this.m_BtnCancel.TabIndex = 1;
      this.m_BtnCancel.Text = "&Cancel";
      this.m_BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.label1, 3);
      this.label1.Dock = System.Windows.Forms.DockStyle.Top;
      this.label1.Location = new System.Drawing.Point(2, 0);
      this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(640, 36);
      this.label1.TabIndex = 5;
      this.label1.Text = "At least one column with a time has been found without time zone information.\r\nPl" +
    "ease determine the timezone of all date/time columns without explicit time zone." +
    "";
      // 
      // timer
      // 
      this.timer.Enabled = true;
      this.timer.Interval = 500;
      this.timer.Tick += new System.EventHandler(this.timer_Tick);
      // 
      // m_TimeZoneSelector
      // 
      this.m_TableLayoutPanel.SetColumnSpan(this.m_TimeZoneSelector, 3);
      this.m_TimeZoneSelector.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TimeZoneSelector.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.m_TimeZoneSelector.Location = new System.Drawing.Point(4, 40);
      this.m_TimeZoneSelector.Margin = new System.Windows.Forms.Padding(4);
      this.m_TimeZoneSelector.Name = "m_TimeZoneSelector";
      this.m_TimeZoneSelector.Size = new System.Drawing.Size(636, 34);
      this.m_TimeZoneSelector.TabIndex = 0;
      // 
      // label
      // 
      this.label.Anchor = System.Windows.Forms.AnchorStyles.Left;
      this.label.AutoSize = true;
      this.label.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label.Location = new System.Drawing.Point(2, 86);
      this.label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label.Name = "label";
      this.label.Size = new System.Drawing.Size(142, 18);
      this.label.TabIndex = 12;
      this.label.Text = "Default in 5 seconds";
      // 
      // m_TableLayoutPanel
      // 
      this.m_TableLayoutPanel.AutoSize = true;
      this.m_TableLayoutPanel.ColumnCount = 3;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnCancel, 2, 2);
      this.m_TableLayoutPanel.Controls.Add(this.label, 0, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnOk, 1, 2);
      this.m_TableLayoutPanel.Controls.Add(this.label1, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_TimeZoneSelector, 0, 1);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 3;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(644, 112);
      this.m_TableLayoutPanel.TabIndex = 13;
      // 
      // FormSelectTimeZone
      // 
      this.AcceptButton = this.m_BtnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.m_BtnCancel;
      this.ClientSize = new System.Drawing.Size(644, 119);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "FormSelectTimeZone";
      this.Text = "Select Time Zone";
      this.TopMost = true;
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FormSelectTimeZone_MouseMove);
      this.m_TableLayoutPanel.ResumeLayout(false);
      this.m_TableLayoutPanel.PerformLayout();
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