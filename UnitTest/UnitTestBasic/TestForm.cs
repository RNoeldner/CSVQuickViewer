/*
 * Copyright (C) 2014 Raphael N�ldner : http://csvquickviewer.com
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
using System.Threading;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace CsvTools.Tests
{
  public sealed class TestForm : Form
  {
    private readonly CancellationTokenSource m_CancellationTokenSource;
    private readonly Timer m_TimerAutoClose = new Timer();

    public CancellationToken CancellationToken => m_CancellationTokenSource.Token;

    public TestForm()
    {
      SuspendLayout();
      m_CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(UnitTestStatic.Token);
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

    public void AddOneControl(Control ctrl, double autoCloseMilliseconds = 10000)
    {
      SuspendLayout();
      Text = ctrl.GetType().FullName;
      ctrl.Dock = DockStyle.Fill;
      ctrl.Location = new Point(0, 0);
      ctrl.Size = new Size(790, 790);
      Controls.Add(ctrl);
      ResumeLayout(false);

      if (!(autoCloseMilliseconds > 0))
        return;
      m_TimerAutoClose.Interval = autoCloseMilliseconds;
      m_TimerAutoClose.Enabled = true;
      m_TimerAutoClose.Start();
      m_TimerAutoClose.Elapsed += (sender, args) =>
      {
        if (InvokeRequired)
          BeginInvoke((MethodInvoker) Close);
        else
          Close();
      };
    }

    private void TestForm_FormClosing(object sender, FormClosingEventArgs e)
    {
      m_TimerAutoClose.Stop();
      m_CancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        m_CancellationTokenSource.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}