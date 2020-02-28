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
  using System.Diagnostics.Contracts;
  using System.Windows.Forms;

  /// <summary>
  ///   FilterControl that can live in a ToolStrip
  /// </summary>
  public class ToolStripDataGridViewColumnFilter : ToolStripControlHost
  {
    public ToolStripDataGridViewColumnFilter() : this(new DataGridViewTextBoxColumn()
    {
      ValueType = typeof(int),
      Name = "int",
      DataPropertyName = "int"
    })
    {
    }
    /// <summary>
    ///   Initializes a new instance of the <see cref="ToolStripDataGridViewColumnFilter" /> class.
    /// </summary>
    /// <param name="dataGridViewColumn">The data grid view column.</param>
    public ToolStripDataGridViewColumnFilter(DataGridViewColumn dataGridViewColumn)
      : base(new DataGridViewColumnFilterControl(dataGridViewColumn))
    {
    }

    /// <summary>
    ///   Gets the data grid view column filter.
    /// </summary>
    /// <value>
    ///   The data grid view column filter.
    /// </value>
    public ColumnFilterLogic ColumnFilterLogic
    {
      get
      {
        Contract.Requires(Control != null);
        Contract.Ensures(Contract.Result<ColumnFilterLogic>() != null);
        return ((DataGridViewColumnFilterControl)Control).ColumnFilterLogic;
      }
    }

    /// <summary>
    ///   Gets the value cluster collection.
    /// </summary>
    /// <value>
    ///   The value cluster collection.
    /// </value>
    public ValueClusterCollection ValueClusterCollection
    {
      get
      {
        Contract.Requires(Control != null);
        Contract.Ensures(Contract.Result<ValueClusterCollection>() != null);
        return ((DataGridViewColumnFilterControl)Control).ColumnFilterLogic.ValueClusterCollection;
      }
    }
  }
}