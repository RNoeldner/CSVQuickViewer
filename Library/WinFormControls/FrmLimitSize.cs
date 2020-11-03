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

namespace CsvTools
{
  using System;
  using System.Windows.Forms;

  /// <summary>
  ///   A pop up form to set the record limit
  /// </summary>
  public partial class FrmLimitSize : ResizeForm
  {
    private const double c_Duration = 5.0;

    private static readonly int[] m_IntRecords = new int[] { 10000, 20000, 50000, 100000 };

    private int m_Counter;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FrmLimitSize" /> class.
    /// </summary>
    public FrmLimitSize()
    {
      InitializeComponent();
      labelCount1.Text = $@"{m_IntRecords[0]:N0}";
      labelCount2.Text = $@"{m_IntRecords[1]:N0}";
      labelCount3.Text = $@"{m_IntRecords[2]:N0}";
      labelCount4.Text = $@"{m_IntRecords[3]:N0}";
      UpdateLabel();
    }

    /// <summary>
    ///   The selected record limit
    /// </summary>
    public long RecordLimit { get; set; }

    private void ButtonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void ButtonOK_Click(object sender, EventArgs e)
    {
      if (trackBarLimit.Value != 5)
        RecordLimit = m_IntRecords[4 - trackBarLimit.Value];
      Close();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
      m_Counter++;
      UpdateLabel();
      // ReSharper disable once PossibleLossOfFraction
      if (m_Counter * timer.Interval / 1000 > c_Duration)
      {
        ButtonOK_Click(sender, e);
      }
    }

    private void UpdateLabel()
    {
      // ReSharper disable once PossibleLossOfFraction
      var display = Convert.ToInt32((c_Duration - m_Counter * timer.Interval / 1000 + .75));
      label.Text = display > 0 ? $@"Default in {display:N0} seconds" : string.Empty;

      Application.DoEvents();
    }
  }
}