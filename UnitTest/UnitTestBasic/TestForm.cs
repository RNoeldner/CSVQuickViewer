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

using System.Drawing;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace CsvTools.Tests
{
  public sealed class TestForm : Form
  {
    private readonly Timer m_Timer = new Timer();

    public TestForm()
    {
      SuspendLayout();
      AutoScaleDimensions = new SizeF(8F, 16F);
      AutoScaleMode = AutoScaleMode.Font;
      BackColor = SystemColors.Control;
      ClientSize = new Size(895, 445);
      FormBorderStyle = FormBorderStyle.SizableToolWindow;
      Name = "TestForm";
      ShowInTaskbar = false;
      StartPosition = FormStartPosition.CenterScreen;
      Text = "TestForm";
      TopMost = true;
      FormClosing += TestForm_FormClosing;
      ResumeLayout(false);
    }

    public void AddOneControl(Control ctrl, double totalMilliseconds = 10000)
    {
      if (ctrl == null) return;

      SuspendLayout();
      Text = ctrl.GetType().FullName;
      ctrl.Dock = DockStyle.Fill;
      ctrl.Location = new Point(0, 0);
      ctrl.Size = new Size(790, 790);
      Controls.Add(ctrl);
      ResumeLayout(false);

      m_Timer.Interval = totalMilliseconds;
      m_Timer.Enabled = true;
      m_Timer.Start();
      m_Timer.Elapsed += (sender, args) => Close();
      Show();
    }

    private void TestForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      m_Timer.Stop();
      m_Timer.Enabled = false;
    }
  }
}