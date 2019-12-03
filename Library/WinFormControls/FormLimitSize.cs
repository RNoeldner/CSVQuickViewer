/*
 * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com/
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
  /// <summary>
  ///   A pop up form to set the record limit
  /// </summary>
  public partial class FrmLimitSize : Form
  {
    private static int[] intRecords = new int[] { 10000, 20000, 50000, 100000 };
    private int m_Counter = 0;
    private double m_Duration = 5.0;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FrmLimitSize" /> class.
    /// </summary>
    public FrmLimitSize()
    {
      InitializeComponent();
      labelCount1.Text = $"{intRecords[0]:N0}";
      labelCount2.Text = $"{intRecords[1]:N0}";
      labelCount3.Text = $"{intRecords[2]:N0}";
      labelCount4.Text = $"{intRecords[3]:N0}";
      UpdateLabel();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
      m_Counter++;
      UpdateLabel();
      if (m_Duration > 0 && (m_Counter * timer.Interval) / 1000 > m_Duration)
      {
        buttonOK_Click(sender, e);
      }
    }

    /// <summary>
    ///   The selected record limit
    /// </summary>
    public int RecordLimit { get; set; } = 0;

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void UpdateLabel()
    {
      var displ = Convert.ToInt32((m_Duration - (m_Counter * timer.Interval) / 1000 + .75));
      if (displ > 0)
      {
        label.Text = $"Default in {displ:N0} seconds";
      }
      else
        label.Text = string.Empty;
      Application.DoEvents();
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      if (trackBarLimit.Value != 5)
        RecordLimit = intRecords[4 - trackBarLimit.Value];
      Close();
    }
  }
}