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
      components = new System.ComponentModel.Container();
      m_BtnOk = new System.Windows.Forms.Button();
      m_BtnCancel = new System.Windows.Forms.Button();
      m_LabelExplain = new System.Windows.Forms.Label();
      m_Label = new System.Windows.Forms.Label();
      m_TableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
      m_ButtonLocalTZ = new System.Windows.Forms.Button();
      m_ComboBoxTimeZoneID = new System.Windows.Forms.ComboBox();
      m_Timer = new System.Windows.Forms.Timer(components);
      m_TableLayoutPanel.SuspendLayout();
      SuspendLayout();
      // 
      // m_BtnOk
      // 
      m_BtnOk.AutoSize = true;
      m_BtnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      m_BtnOk.Location = new System.Drawing.Point(494, 71);
      m_BtnOk.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      m_BtnOk.Name = "m_BtnOk";
      m_BtnOk.Size = new System.Drawing.Size(83, 27);
      m_BtnOk.TabIndex = 4;
      m_BtnOk.Text = "&OK";
      // 
      // m_BtnCancel
      // 
      m_BtnCancel.AutoSize = true;
      m_BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      m_BtnCancel.Location = new System.Drawing.Point(583, 71);
      m_BtnCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      m_BtnCancel.Name = "m_BtnCancel";
      m_BtnCancel.Size = new System.Drawing.Size(83, 27);
      m_BtnCancel.TabIndex = 5;
      m_BtnCancel.Text = "&Cancel";
      // 
      // m_LabelExplain
      // 
      m_LabelExplain.AutoSize = true;
      m_TableLayoutPanel.SetColumnSpan(m_LabelExplain, 3);
      m_LabelExplain.Dock = System.Windows.Forms.DockStyle.Top;
      m_LabelExplain.Location = new System.Drawing.Point(3, 2);
      m_LabelExplain.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      m_LabelExplain.Name = "m_LabelExplain";
      m_LabelExplain.Size = new System.Drawing.Size(663, 34);
      m_LabelExplain.TabIndex = 0;
      m_LabelExplain.Tag = "A column with a date / time value has been found, we do not have information on t" +
    "he time zone.\r\nPlease select the time zone... It will be converted to {0}";
      m_LabelExplain.Text = "A column with a date / time value has been found, we do not have information on t" +
    "he time zone.\r\nPlease select the time zone...";
      // 
      // m_Label
      // 
      m_Label.AutoSize = true;
      m_Label.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
      m_Label.Location = new System.Drawing.Point(3, 76);
      m_Label.Margin = new System.Windows.Forms.Padding(3, 7, 3, 2);
      m_Label.Name = "m_Label";
      m_Label.Size = new System.Drawing.Size(137, 17);
      m_Label.TabIndex = 3;
      m_Label.Text = "Default in 5 seconds";
      // 
      // m_TableLayoutPanel
      // 
      m_TableLayoutPanel.AutoSize = true;
      m_TableLayoutPanel.ColumnCount = 3;
      m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      m_TableLayoutPanel.Controls.Add(m_ButtonLocalTZ, 2, 1);
      m_TableLayoutPanel.Controls.Add(m_BtnCancel, 2, 2);
      m_TableLayoutPanel.Controls.Add(m_Label, 0, 2);
      m_TableLayoutPanel.Controls.Add(m_BtnOk, 1, 2);
      m_TableLayoutPanel.Controls.Add(m_LabelExplain, 0, 0);
      m_TableLayoutPanel.Controls.Add(m_ComboBoxTimeZoneID, 0, 1);
      m_TableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      m_TableLayoutPanel.Location = new System.Drawing.Point(0, 0);
      m_TableLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      m_TableLayoutPanel.Name = "m_TableLayoutPanel";
      m_TableLayoutPanel.RowCount = 3;
      m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
      m_TableLayoutPanel.Size = new System.Drawing.Size(669, 110);
      m_TableLayoutPanel.TabIndex = 0;
      // 
      // m_ButtonLocalTZ
      // 
      m_ButtonLocalTZ.AutoSize = true;
      m_ButtonLocalTZ.Location = new System.Drawing.Point(583, 40);
      m_ButtonLocalTZ.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      m_ButtonLocalTZ.Name = "m_ButtonLocalTZ";
      m_ButtonLocalTZ.Size = new System.Drawing.Size(83, 27);
      m_ButtonLocalTZ.TabIndex = 2;
      m_ButtonLocalTZ.Text = "&Local";
      m_ButtonLocalTZ.UseVisualStyleBackColor = true;
      m_ButtonLocalTZ.Click += new System.EventHandler(buttonLocalTZ_Click);
      // 
      // m_ComboBoxTimeZoneID
      // 
      m_TableLayoutPanel.SetColumnSpan(m_ComboBoxTimeZoneID, 2);
      m_ComboBoxTimeZoneID.Dock = System.Windows.Forms.DockStyle.Top;
      m_ComboBoxTimeZoneID.FormattingEnabled = true;
      m_ComboBoxTimeZoneID.Location = new System.Drawing.Point(3, 40);
      m_ComboBoxTimeZoneID.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      m_ComboBoxTimeZoneID.Name = "m_ComboBoxTimeZoneID";
      m_ComboBoxTimeZoneID.Size = new System.Drawing.Size(574, 24);
      m_ComboBoxTimeZoneID.TabIndex = 1;
      // 
      // m_Timer
      // 
      m_Timer.Enabled = true;
      m_Timer.Interval = 500;
      m_Timer.Tick += new System.EventHandler(timer_Tick);
      // 
      // FormSelectTimeZone
      // 
      AcceptButton = m_BtnOk;
      AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      CancelButton = m_BtnCancel;
      ClientSize = new System.Drawing.Size(669, 110);
      Controls.Add(m_TableLayoutPanel);
      FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
      MaximizeBox = false;
      MinimizeBox = false;
      MinimumSize = new System.Drawing.Size(666, 147);
      Name = "FormSelectTimeZone";
      Text = "Select Time Zone";
      TopMost = true;
      Load += new System.EventHandler(FormSelectTimeZone_Load);
      MouseMove += new System.Windows.Forms.MouseEventHandler(FormSelectTimeZone_MouseMove);
      m_TableLayoutPanel.ResumeLayout(false);
      m_TableLayoutPanel.PerformLayout();
      ResumeLayout(false);
      PerformLayout();

    }

    private void timer_Tick(object sender, EventArgs e)
    {
      m_Counter++;
      UpdateLabel();
      // ReSharper disable once PossibleLossOfFraction
      if (m_Duration > 0 && m_Counter * m_Timer.Interval / 1000 > m_Duration)
      {
        DialogResult = DialogResult.OK;
        Close();
      }
    }

    private void UpdateLabel()
    {
      // ReSharper disable once PossibleLossOfFraction
      var display = Convert.ToInt32(m_Duration - m_Counter * m_Timer.Interval / 1000 + .75);
      m_Label.Text = display > 0 ? $"OK in {display:N0} seconds" : string.Empty;
    }

    private void buttonLocalTZ_Click(object sender, EventArgs e) => TimeZoneID = TimeZoneInfo.Local.Id;

    private void FormSelectTimeZone_Load(object sender, EventArgs e) => TimeZoneID = TimeZoneInfo.Local.Id;
  }
}