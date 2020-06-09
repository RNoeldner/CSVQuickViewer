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
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace CsvTools
{
  using System.Linq;

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
    private Timer m_Timer;
    private TableLayoutPanel m_TableLayoutPanel;

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
      display.AddRange(
        TimeZoneInfo.GetSystemTimeZones().Select(wintz => new DisplayItem<string>(wintz.Id, wintz.DisplayName)));

      m_ComboBoxTimeZoneID.DataSource = display;
      m_ComboBoxTimeZoneID.SelectedValue = TimeZoneInfo.Local.Id;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Bindable(false)]
    [Browsable(false)]
    public string TimeZoneID
    {
      get => (string)m_ComboBoxTimeZoneID.SelectedValue;
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

    private void FormSelectTimeZone_MouseMove(object sender, MouseEventArgs e) => m_Timer.Enabled = false;

    /// <summary>
    ///   Required method for Designer support - do not modify the contents of this method with the
    ///   code editor.
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [SuppressMessage("ReSharper", "LocalizableElement")]
    [SuppressMessage("ReSharper", "RedundantDelegateCreation")]
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.m_BtnOk = new System.Windows.Forms.Button();
      this.m_BtnCancel = new System.Windows.Forms.Button();
      this.m_LabelExplain = new System.Windows.Forms.Label();
      this.m_Label = new System.Windows.Forms.Label();
      this.m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      this.m_ButtonLocalTZ = new System.Windows.Forms.Button();
      this.m_ComboBoxTimeZoneID = new System.Windows.Forms.ComboBox();
      this.m_Timer = new System.Windows.Forms.Timer(this.components);
      this.m_TableLayoutPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // m_BtnOk
      // 
      this.m_BtnOk.AutoSize = true;
      this.m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.m_BtnOk.Location = new System.Drawing.Point(500, 73);
      this.m_BtnOk.Margin = new System.Windows.Forms.Padding(2);
      this.m_BtnOk.Name = "m_BtnOk";
      this.m_BtnOk.Size = new System.Drawing.Size(82, 27);
      this.m_BtnOk.TabIndex = 4;
      this.m_BtnOk.Text = "&OK";
      // 
      // m_BtnCancel
      // 
      this.m_BtnCancel.AutoSize = true;
      this.m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_BtnCancel.Location = new System.Drawing.Point(586, 73);
      this.m_BtnCancel.Margin = new System.Windows.Forms.Padding(2);
      this.m_BtnCancel.Name = "m_BtnCancel";
      this.m_BtnCancel.Size = new System.Drawing.Size(82, 27);
      this.m_BtnCancel.TabIndex = 5;
      this.m_BtnCancel.Text = "&Cancel";
      // 
      // m_LabelExplain
      // 
      this.m_LabelExplain.AutoSize = true;
      this.m_TableLayoutPanel.SetColumnSpan(this.m_LabelExplain, 3);
      this.m_LabelExplain.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_LabelExplain.Location = new System.Drawing.Point(3, 3);
      this.m_LabelExplain.Margin = new System.Windows.Forms.Padding(3);
      this.m_LabelExplain.Name = "m_LabelExplain";
      this.m_LabelExplain.Size = new System.Drawing.Size(664, 34);
      this.m_LabelExplain.TabIndex = 0;
      this.m_LabelExplain.Tag = "A column with a date / time value has been found, we do not have information on t" +
    "he time zone.\r\nPlease select the time zone... It will be converted to {0}";
      this.m_LabelExplain.Text = "A column with a date / time value has been found, we do not have information on t" +
    "he time zone.\r\nPlease select the time zone...";
      // 
      // m_Label
      // 
      this.m_Label.AutoSize = true;
      this.m_Label.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      this.m_Label.Location = new System.Drawing.Point(3, 78);
      this.m_Label.Margin = new System.Windows.Forms.Padding(3, 7, 3, 3);
      this.m_Label.Name = "m_Label";
      this.m_Label.Size = new System.Drawing.Size(137, 17);
      this.m_Label.TabIndex = 3;
      this.m_Label.Text = "Default in 5 seconds";
      // 
      // m_TableLayoutPanel
      // 
      this.m_TableLayoutPanel.AutoSize = true;
      this.m_TableLayoutPanel.ColumnCount = 3;
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.m_TableLayoutPanel.Controls.Add(this.m_ButtonLocalTZ, 2, 1);
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnCancel, 2, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_Label, 0, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_BtnOk, 1, 2);
      this.m_TableLayoutPanel.Controls.Add(this.m_LabelExplain, 0, 0);
      this.m_TableLayoutPanel.Controls.Add(this.m_ComboBoxTimeZoneID, 0, 1);
      this.m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      this.m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(2);
      this.m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      this.m_TableLayoutPanel.RowCount = 3;
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.m_TableLayoutPanel.Size = new System.Drawing.Size(670, 107);
      this.m_TableLayoutPanel.TabIndex = 0;
      // 
      // m_ButtonLocalTZ
      // 
      this.m_ButtonLocalTZ.AutoSize = true;
      this.m_ButtonLocalTZ.Location = new System.Drawing.Point(586, 42);
      this.m_ButtonLocalTZ.Margin = new System.Windows.Forms.Padding(2);
      this.m_ButtonLocalTZ.Name = "m_ButtonLocalTZ";
      this.m_ButtonLocalTZ.Size = new System.Drawing.Size(82, 27);
      this.m_ButtonLocalTZ.TabIndex = 2;
      this.m_ButtonLocalTZ.Text = "&Local";
      this.m_ButtonLocalTZ.UseVisualStyleBackColor = true;
      this.m_ButtonLocalTZ.Click += new System.EventHandler(this.buttonLocalTZ_Click);
      // 
      // m_ComboBoxTimeZoneID
      // 
      this.m_TableLayoutPanel.SetColumnSpan(this.m_ComboBoxTimeZoneID, 2);
      this.m_ComboBoxTimeZoneID.Dock = System.Windows.Forms.DockStyle.Top;
      this.m_ComboBoxTimeZoneID.FormattingEnabled = true;
      this.m_ComboBoxTimeZoneID.Location = new System.Drawing.Point(3, 42);
      this.m_ComboBoxTimeZoneID.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      this.m_ComboBoxTimeZoneID.Name = "m_ComboBoxTimeZoneID";
      this.m_ComboBoxTimeZoneID.Size = new System.Drawing.Size(578, 24);
      this.m_ComboBoxTimeZoneID.TabIndex = 1;
      // 
      // m_Timer
      // 
      this.m_Timer.Enabled = true;
      this.m_Timer.Interval = 500;
      this.m_Timer.Tick += new System.EventHandler(this.timer_Tick);
      // 
      // FormSelectTimeZone
      // 
      this.AcceptButton = this.m_BtnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.m_BtnCancel;
      this.ClientSize = new System.Drawing.Size(670, 107);
      this.Controls.Add(this.m_TableLayoutPanel);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      this.Margin = new System.Windows.Forms.Padding(2);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(667, 149);
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
      // ReSharper disable once PossibleLossOfFraction
      if (m_Duration > 0 && m_Counter * m_Timer.Interval / 1000 > m_Duration)
      {
        this.DialogResult = DialogResult.OK;
        Close();
      }
    }

    private void UpdateLabel()
    {
      // ReSharper disable once PossibleLossOfFraction
      var display = Convert.ToInt32(m_Duration - m_Counter * m_Timer.Interval / 1000 + .75);
      m_Label.Text = display > 0 ? $"OK in {display:N0} seconds" : string.Empty;
    }

    private void buttonLocalTZ_Click(object sender, EventArgs e)
    {
      TimeZoneID = TimeZoneInfo.Local.Id;
    }
  }
}