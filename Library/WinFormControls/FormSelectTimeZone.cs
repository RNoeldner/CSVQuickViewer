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
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Windows.Forms;

  public sealed class FormSelectTimeZone : ResizeForm
  {
    private readonly double m_Duration = 5.0;

    private IContainer components;

    private Label label;

    private Label labelExplain;

    private Button m_BtnCancel;

    private Button m_BtnOk;

    private int m_Counter;

    private TableLayoutPanel m_TableLayoutPanel;
    private Button buttonLocalTZ;
    private ComboBox comboBoxTimeZoneID;
    private Timer timer;

    public FormSelectTimeZone() : this("Timezone")
    {
    }

    public FormSelectTimeZone(string title)
    {
      InitializeComponent();
      Text = title;
      comboBoxTimeZoneID.ValueMember = "ID";
      comboBoxTimeZoneID.DisplayMember = "Display";

      var display = new List<DisplayItem<string>>
                      {
                        new DisplayItem<string>(
                          TimeZoneInfo.Local.Id,
                          $"{TimeZoneInfo.Local.DisplayName} *[Local System]")
                      };

      foreach (var wintz in TimeZoneInfo.GetSystemTimeZones())
        display.Add(new DisplayItem<string>(wintz.Id, wintz.DisplayName));

      comboBoxTimeZoneID.DataSource = display;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
    public string TimeZoneID
    {
      get => (string)comboBoxTimeZoneID.SelectedValue;
      set => comboBoxTimeZoneID.SelectedValue = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
    public string DestTimeZoneID
    {
      set => labelExplain.Text = string.Format(labelExplain.Tag.ToString(), value);
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
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.m_BtnOk = new System.Windows.Forms.Button();
      this.m_BtnCancel = new System.Windows.Forms.Button();
      this.labelExplain = new System.Windows.Forms.Label();
      this.timer = new System.Windows.Forms.Timer(this.components);
      this.label = new System.Windows.Forms.Label();
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.buttonLocalTZ = new System.Windows.Forms.Button();
      this.comboBoxTimeZoneID = new System.Windows.Forms.ComboBox();
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // m_BtnOk
      this.m_BtnOk.AutoSize = true;
      this.m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.m_BtnOk.Location = new System.Drawing.Point(515, 84);
      this.m_BtnOk.Margin = new System.Windows.Forms.Padding(2);
      this.m_BtnOk.Name = "m_BtnOk";
      this.m_BtnOk.Size = new System.Drawing.Size(112, 30);
      this.m_BtnOk.TabIndex = 4;
      this.m_BtnOk.Text = "&OK";
      this.m_BtnOk.Click += new System.EventHandler(this.BtnOK_Click);
      // m_BtnCancel
      this.m_BtnCancel.AutoSize = true;
      this.m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_BtnCancel.Location = new System.Drawing.Point(631, 84);
      this.m_BtnCancel.Margin = new System.Windows.Forms.Padding(2);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new System.Drawing.Size(112, 30);
      this.m_BtnCancel.TabIndex = 5;
      this.m_BtnCancel.Text = "&Cancel";
      this.m_BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
      // labelExplain
      this.labelExplain.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.labelExplain, 3);
      this.labelExplain.Dock = System.Windows.Forms.DockStyle.Top;
      this.labelExplain.Location = new System.Drawing.Point(4, 4);
      this.labelExplain.Margin = new System.Windows.Forms.Padding(4);
      this.labelExplain.Name = "labelExplain";
      this.labelExplain.Size = new System.Drawing.Size(737, 40);
      this.labelExplain.TabIndex = 0;
      this.labelExplain.Tag = "A column with a date / time value has been found, we do not have information on t" +
    "he time zone.\r\nPlease select the time zone... It will be converted to {0}";
      this.labelExplain.Text = "A column with a date / time value has been found, we do not have information on t" +
    "he time zone.\r\nPlease select the time zone...";
      // timer
      this.timer.Enabled = true;
      this.timer.Interval = 500;
      this.timer.Tick += new System.EventHandler(this.timer_Tick);
      // label
      this.label.AutoSize = true;
      this.label.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.label.Location = new System.Drawing.Point(4, 90);
      this.label.Margin = new System.Windows.Forms.Padding(4, 8, 4, 4);
      this.label.Name = "label";
      this.label.Size = new System.Drawing.Size(154, 20);
      this.label.TabIndex = 3;
      this.label.Text = "Default in 5 seconds";
      // m_TableLayoutPanel
      this.m_TableLayoutPanel.AutoSize = true;
      this.m_TableLayoutPanel.ColumnCount = 3;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.Controls.Add(this.buttonLocalTZ, 2, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnCancel, 2, 2);
      this.m_TableLayoutPanel.Controls.Add(this.label, 0, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnOk, 1, 2);
      this.m_TableLayoutPanel.Controls.Add(this.labelExplain, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.comboBoxTimeZoneID, 0, 1);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 3;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(745, 124);
      this.m_TableLayoutPanel.TabIndex = 0;
      // buttonLocalTZ
      this.buttonLocalTZ.AutoSize = true;
      this.buttonLocalTZ.Location = new System.Drawing.Point(631, 50);
      this.buttonLocalTZ.Margin = new System.Windows.Forms.Padding(2);
      this.buttonLocalTZ.Name = "buttonLocalTZ";
      this.buttonLocalTZ.Size = new System.Drawing.Size(112, 30);
      this.buttonLocalTZ.TabIndex = 2;
      this.buttonLocalTZ.Text = "&Local";
      this.buttonLocalTZ.UseVisualStyleBackColor = true;
      this.buttonLocalTZ.Click += new System.EventHandler(this.buttonLocalTZ_Click);
      // comboBoxTimeZoneID
      this.m_TableLayoutPanel.SetColumnSpan(this.comboBoxTimeZoneID, 2);
      this.comboBoxTimeZoneID.Dock = System.Windows.Forms.DockStyle.Top;
      this.comboBoxTimeZoneID.FormattingEnabled = true;
      this.comboBoxTimeZoneID.Location = new System.Drawing.Point(3, 51);
      this.comboBoxTimeZoneID.Name = "comboBoxTimeZoneID";
      this.comboBoxTimeZoneID.Size = new System.Drawing.Size(623, 28);
      this.comboBoxTimeZoneID.TabIndex = 1;
      // FormSelectTimeZone
      this.AcceptButton = this.m_BtnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.m_BtnCancel;
      this.ClientSize = new System.Drawing.Size(745, 124);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(750, 180);
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

    private void buttonLocalTZ_Click(object sender, EventArgs e)
    {
      TimeZoneID = TimeZoneInfo.Local.Id;
    }
  }
}