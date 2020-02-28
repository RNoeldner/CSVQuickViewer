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
      components = new Container();
      m_BtnOk = new Button();
      m_BtnCancel = new Button();
      label1 = new Label();
      timer = new Timer(components);
      m_TimeZoneSelector = new TimeZoneSelector();
      label = new Label();
      m_TableLayoutPanel = new TableLayoutPanel();
      m_TableLayoutPanel.SuspendLayout();
      SuspendLayout();

      // m_BtnOk
      m_BtnOk.AutoSize = true;
      m_BtnOk.DialogResult = DialogResult.OK;
      m_BtnOk.Location = new Point(431, 87);
      m_BtnOk.Name = "m_BtnOk";
      m_BtnOk.Size = new Size(102, 34);
      m_BtnOk.TabIndex = 2;
      m_BtnOk.Text = "&OK";
      m_BtnOk.Click += new EventHandler(BtnOK_Click);

      // m_BtnCancel
      m_BtnCancel.AutoSize = true;
      m_BtnCancel.DialogResult = DialogResult.Cancel;
      m_BtnCancel.Location = new Point(539, 87);
      m_BtnCancel.Name = "m_BtnCancel";
      m_BtnCancel.Size = new Size(102, 34);
      m_BtnCancel.TabIndex = 1;
      m_BtnCancel.Text = "&Cancel";
      m_BtnCancel.Click += new EventHandler(BtnCancel_Click);

      // label1
      label1.AutoSize = true;
      m_TableLayoutPanel.SetColumnSpan(label1, 3);
      label1.Dock = DockStyle.Top;
      label1.Location = new Point(3, 0);
      label1.Name = "label1";
      label1.Size = new Size(638, 40);
      label1.TabIndex = 5;
      label1.Text = "At least one column with a time has been found without time zone information.\r\nPl"
                         + "ease determine the timezone of all date/time columns without explicit time zone."
                         + string.Empty;

      // timer
      timer.Enabled = true;
      timer.Interval = 500;
      timer.Tick += new EventHandler(timer_Tick);

      // timeZoneSelector1
      m_TableLayoutPanel.SetColumnSpan(m_TimeZoneSelector, 3);
      m_TimeZoneSelector.Dock = DockStyle.Top;
      m_TimeZoneSelector.Location = new Point(3, 43);
      m_TimeZoneSelector.Name = "timeZoneSelector1";
      m_TimeZoneSelector.Size = new Size(638, 38);
      m_TimeZoneSelector.TabIndex = 0;
      m_TimeZoneSelector.TimeZoneID = "(local)";

      // label
      label.Anchor = AnchorStyles.Left;
      label.AutoSize = true;
      label.ForeColor = SystemColors.ControlDarkDark;
      label.Location = new Point(3, 94);
      label.Name = "label";
      label.Size = new Size(154, 20);
      label.TabIndex = 12;
      label.Text = "Default in 5 seconds";

      // tableLayoutPanel1
      m_TableLayoutPanel.AutoSize = true;
      m_TableLayoutPanel.ColumnCount = 3;
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
      m_TableLayoutPanel.Controls.Add(m_BtnCancel, 2, 2);
      m_TableLayoutPanel.Controls.Add(label, 0, 2);
      m_TableLayoutPanel.Controls.Add(m_BtnOk, 1, 2);
      m_TableLayoutPanel.Controls.Add(label1, 0, 0);
      m_TableLayoutPanel.Controls.Add(m_TimeZoneSelector, 0, 1);
      m_TableLayoutPanel.Dock = DockStyle.Top;
      m_TableLayoutPanel.Location = new Point(0, 0);
      m_TableLayoutPanel.Name = "tableLayoutPanel1";
      m_TableLayoutPanel.RowCount = 3;
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.RowStyles.Add(new RowStyle());
      m_TableLayoutPanel.Size = new Size(644, 124);
      m_TableLayoutPanel.TabIndex = 13;

      // FormSelectTimeZone
      AcceptButton = m_BtnOk;
      AutoScaleDimensions = new SizeF(9F, 20F);
      AutoScaleMode = AutoScaleMode.Font;
      CancelButton = m_BtnCancel;
      ClientSize = new Size(644, 132);
      Controls.Add(m_TableLayoutPanel);
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      MaximizeBox = false;
      MinimizeBox = false;
      Name = "FormSelectTimeZone";
      Text = "Select Time Zone";
      TopMost = true;
      MouseMove += new MouseEventHandler(FormSelectTimeZone_MouseMove);
      m_TableLayoutPanel.ResumeLayout(false);
      m_TableLayoutPanel.PerformLayout();
      ResumeLayout(false);
      PerformLayout();
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