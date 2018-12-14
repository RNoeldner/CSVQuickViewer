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

using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   A Form to display a Data Table
  /// </summary>
  public class FormDetail : Form
  {
    private readonly DetailControl detailControl;

    /// <summary>
    ///   Initializes a new instance of the <see cref="FormDetail" /> class.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="uniqueFieldName">Name of the unique field.</param>
    /// <param name="setting">a FileSetting</param>
    /// <param name="readOnly">if set to <c>true</c> the data will not be editable</param>
    /// <param name="onlyErrors">if set to <c>true</c> non error will be hidden</param>
    /// <param name="frozenColumns">The frozen columns.</param>
    /// <param name="cancellationToken">The cancellation token</param>
    ///
    public FormDetail(DataTable dataTable, IEnumerable<string> uniqueFieldName, IFileSetting setting, bool readOnly,
      bool onlyErrors, int frozenColumns, CancellationToken cancellationToken = default(CancellationToken))
    {
      Contract.Requires(dataTable != null);

      SuspendLayout();
      DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle
      {
        BackColor = System.Drawing.Color.Gainsboro
      };

      DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle
      {
        Alignment = DataGridViewContentAlignment.MiddleLeft,
        BackColor = System.Drawing.SystemColors.Window,
        Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
        ForeColor = System.Drawing.Color.Black,
        SelectionBackColor = System.Drawing.SystemColors.Highlight,
        SelectionForeColor = System.Drawing.SystemColors.HighlightText,
        WrapMode = System.Windows.Forms.DataGridViewTriState.False
      };
      detailControl = new DetailControl
      {
        CancellationToken = cancellationToken,
        AlternatingRowDefaultCellSyle = dataGridViewCellStyle1,
        DefaultCellStyle = dataGridViewCellStyle2,
        Dock = DockStyle.Fill,
        Location = new System.Drawing.Point(0, 0),
        Name = "detailControl",
        ReadOnly = readOnly,
        Size = new System.Drawing.Size(767, 394),
        TabIndex = 0,
        FileSetting = setting
      };

      AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      ClientSize = new System.Drawing.Size(767, 394);
      MinimumSize = new System.Drawing.Size(100, 100);
      Controls.Add(this.detailControl);
      Icon = global::CsvToolLib.Resources.SubFormIcon;
      KeyPreview = true;
      Name = "FormDetail";
      ResumeLayout(false);

      detailControl.DataTable = dataTable;

      // Need to set UniqueFieldName last
      detailControl.UniqueFieldName = uniqueFieldName;
      if (frozenColumns > 0)
        detailControl.FrozenColumns = frozenColumns;
      if (onlyErrors)
        detailControl.OnlyShowErrors();
    }
  }
}