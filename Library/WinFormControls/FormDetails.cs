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
using System.Windows.Forms;

namespace CsvTools
{
  /// <summary>
  ///   A Form to display a Data Table
  /// </summary>
  public partial class FormDetail : Form
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="FormDetail" /> class.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="uniqueFieldName">Name of the unique field.</param>
    /// <param name="setting">a FileSetting</param>
    /// <param name="readOnly">if set to <c>true</c> the data will not be editable</param>
    /// <param name="extendedVersion">if set to <c>true</c> the extended version features are shown.</param>
    /// <param name="onlyErrors">if set to <c>true</c> non error will be hidden</param>
    /// <param name="frozenColumns">The frozen columns.</param>
    public FormDetail(DataTable dataTable, IEnumerable<string> uniqueFieldName, IFileSetting setting, bool readOnly,
      bool extendedVersion, bool onlyErrors, int frozenColumns)
    {
      Contract.Requires(dataTable != null);
      InitializeComponent();
      detailControl.FileSetting = setting;
      detailControl.ReadOnly = readOnly;
      detailControl.ExtendedVersion = extendedVersion;
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