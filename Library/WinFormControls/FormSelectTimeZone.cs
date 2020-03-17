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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CsvTools
{
  public sealed class FormSelectTimeZone : ResizeForm
  {
    private readonly double m_Duration = 5.0;

    private IContainer components;
    private Button m_BtnCancel;
    private Button m_BtnOk;
    private Button m_ButtonLocalTZ;
    private ComboBox m_ComboBoxTimeZoneID;
    private int m_Counter;
    private Label m_Label;
    private Label m_LabelExplain;
    private TableLayoutPanel m_TableLayoutPanel;
    private Timer m_Timer;

    public FormSelectTimeZone() : this("Timezone")
    {
    }

    private FormSelectTimeZone(string title)
    {
      InitializeComponent();
      Text = title;
      m_ComboBoxTimeZoneID.ValueMember = "ID";
      m_ComboBoxTimeZoneID.DisplayMember = "Display";

      var display = new List<DisplayItem<string>>
      {
        new DisplayItem<string>(
          TimeZoneInfo.Local.Id,
          $"{TimeZoneInfo.Local.DisplayName} *[Local System]")
      };

      foreach (var wintz in TimeZoneInfo.GetSystemTimeZones())
        display.Add(new DisplayItem<string>(wintz.Id, wintz.DisplayName));

      m_ComboBoxTimeZoneID.DataSource = display;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
    public string TimeZoneID
    {
      get => (string) m_ComboBoxTimeZoneID.SelectedValue;
      set => m_ComboBoxTimeZoneID.SelectedValue = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
    public string DestTimeZoneID
    {
      set => m_LabelExplain.Text = string.Format(m_LabelExplain.Tag.ToString(), value);
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

    private void FormSelectTimeZone_MouseMove(object sender, MouseEventArgs e) => m_Timer.Enabled = false;

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    private void InitializeComponent()
    {
      components = new Container();
      m_BtnOk = new Button();
      m_BtnCancel = new Button();
      m_LabelExplain = new Label();
      m_Timer = new Timer(components);
      m_Label = new Label();
      m_TableLayoutPanel = new TableLayoutPanel();
      m_ButtonLocalTZ = new Button();
      m_ComboBoxTimeZoneID = new ComboBox();
      m_TableLayoutPanel.SuspendLayout();
      SuspendLayout();
      // 
      // m_BtnOk
      // 
      m_BtnOk.AutoSize = true;
      m_BtnOk.DialogResult = DialogResult.OK;
      m_BtnOk.Location = new Point(802, 119);
      m_BtnOk.Name = "m_BtnOk";
      m_BtnOk.Size = new Size(174, 40);
      m_BtnOk.TabIndex = 4;
      m_BtnOk.Text = "&OK";
      m_BtnOk.Click += BtnOK_Click;
      // 
      // m_BtnCancel
      // 
      m_BtnCancel.AutoSize = true;
      m_BtnCancel.DialogResult = DialogResult.Cancel;
      m_BtnCancel.Location = new Point(982, 119);
      m_BtnCancel.Name = "m_BtnCancel";
      m_BtnCancel.Size = new Size(174, 40);
      m_BtnCancel.TabIndex = 5;
      m_BtnCancel.Text = "&Cancel";
      m_BtnCancel.Click += BtnCancel_Click;
      // 
      // labelExplain
      // 
      m_LabelExplain.AutoSize = true;
      m_TableLayoutPanel.SetColumnSpan(m_LabelExplain, 3);
      m_LabelExplain.Dock = DockStyle.Top;
      m_LabelExplain.Location = new Point(6, 6);
      m_LabelExplain.Margin = new Padding(6, 6, 6, 6);
      m_LabelExplain.Name = "m_LabelExplain";
      m_LabelExplain.Size = new Size(1147, 58);
      m_LabelExplain.TabIndex = 0;
      m_LabelExplain.Tag = "A column with a date / time value has been found, we do not have information on t" +
                           "he time zone.\r\nPlease select the time zone... It will be converted to {0}";
      m_LabelExplain.Text = "A column with a date / time value has been found, we do not have information on t" +
                            "he time zone.\r\nPlease select the time zone...";
      // 
      // timer
      // 
      m_Timer.Enabled = true;
      m_Timer.Interval = 500;
      m_Timer.Tick += timer_Tick;
      // 
      // label
      // 
      m_Label.AutoSize = true;
      m_Label.ForeColor = SystemColors.ControlDarkDark;
      m_Label.Location = new Point(6, 128);
      m_Label.Margin = new Padding(6, 12, 6, 6);
      m_Label.Name = "m_Label";
      m_Label.Size = new Size(229, 29);
      m_Label.TabIndex = 3;
      m_Label.Text = "Default in 5 seconds";
      // 
      // m_TableLayoutPanel
      // 
      m_TableLayoutPanel.AutoSize = true;
      m_TableLayoutPanel.ColumnCount = 3;
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.Controls.Add(m_ButtonLocalTZ, 2, 1);
      m_TableLayoutPanel.Controls.Add(m_BtnCancel, 2, 2);
      m_TableLayoutPanel.Controls.Add(m_Label, 0, 2);
      m_TableLayoutPanel.Controls.Add(m_BtnOk, 1, 2);
      m_TableLayoutPanel.Controls.Add(m_LabelExplain, 0, 0);
      m_TableLayoutPanel.Controls.Add(m_ComboBoxTimeZoneID, 0, 1);
      m_TableLayoutPanel.Dock = DockStyle.Fill;
      m_TableLayoutPanel.Location = new Point(0, 0);
      m_TableLayoutPanel.Margin = new Padding(3, 4, 3, 4);
      m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      m_TableLayoutPanel.RowCount = 3;
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.Size = new Size(1159, 180);
      m_TableLayoutPanel.TabIndex = 0;
      // 
      // buttonLocalTZ
      // 
      m_ButtonLocalTZ.AutoSize = true;
      m_ButtonLocalTZ.Location = new Point(982, 73);
      m_ButtonLocalTZ.Name = "m_ButtonLocalTZ";
      m_ButtonLocalTZ.Size = new Size(174, 40);
      m_ButtonLocalTZ.TabIndex = 2;
      m_ButtonLocalTZ.Text = "&Local";
      m_ButtonLocalTZ.UseVisualStyleBackColor = true;
      m_ButtonLocalTZ.Click += buttonLocalTZ_Click;
      // 
      // comboBoxTimeZoneID
      // 
      m_TableLayoutPanel.SetColumnSpan(m_ComboBoxTimeZoneID, 2);
      m_ComboBoxTimeZoneID.Dock = DockStyle.Top;
      m_ComboBoxTimeZoneID.FormattingEnabled = true;
      m_ComboBoxTimeZoneID.Location = new Point(5, 74);
      m_ComboBoxTimeZoneID.Margin = new Padding(5, 4, 5, 4);
      m_ComboBoxTimeZoneID.Name = "m_ComboBoxTimeZoneID";
      m_ComboBoxTimeZoneID.Size = new Size(969, 37);
      m_ComboBoxTimeZoneID.TabIndex = 1;
      // 
      // FormSelectTimeZone
      // 
      AcceptButton = m_BtnOk;
      AutoScaleDimensions = new SizeF(14F, 29F);
      AutoScaleMode = AutoScaleMode.Font;
      CancelButton = m_BtnCancel;
      ClientSize = new Size(1159, 180);
      Controls.Add(m_TableLayoutPanel);
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      Margin = new Padding(3, 4, 3, 4);
      MaximizeBox = false;
      MinimizeBox = false;
      MinimumSize = new Size(1154, 236);
      Name = "FormSelectTimeZone";
      Text = "Select Time Zone";
      TopMost = true;
      MouseMove += FormSelectTimeZone_MouseMove;
      m_TableLayoutPanel.ResumeLayout(false);
      m_TableLayoutPanel.PerformLayout();
      ResumeLayout(false);
      PerformLayout();
    }

    private void timer_Tick(object sender, EventArgs e)
    {
      m_Counter++;
      UpdateLabel();
      if (m_Duration > 0 && m_Counter * m_Timer.Interval / 1000 > m_Duration) BtnOK_Click(sender, e);
    }

    private void UpdateLabel()
    {
      var display = Convert.ToInt32(m_Duration - m_Counter * m_Timer.Interval / 1000 + .75);
      m_Label.Text = display > 0 ? $"OK in {display:N0} seconds" : string.Empty;
    }

    private void buttonLocalTZ_Click(object sender, EventArgs e)
    {
      TimeZoneID = TimeZoneInfo.Local.Id;
    }
  }
}