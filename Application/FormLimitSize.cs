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
    /// <summary>
    ///   Initializes a new instance of the <see cref="FrmLimitSize" /> class.
    /// </summary>
    public FrmLimitSize()
    {
      InitializeComponent();
    }

    /// <summary>
    ///   The selected record limit
    /// </summary>
    public int RecordLimit { get; set; }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      if (trackBarLimit.Value == 4)
        RecordLimit = 0;
      if (trackBarLimit.Value == 3)
        RecordLimit = 50000;
      if (trackBarLimit.Value == 2)
        RecordLimit = 10000;
      if (trackBarLimit.Value == 1)
        RecordLimit = 5000;
      DialogResult = DialogResult.OK;
      Close();
    }
  }
}