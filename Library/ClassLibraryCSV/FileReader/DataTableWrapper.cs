/*s
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
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   IFileReader implementation based on a data table, this is used to pass on a data table to a writer
  /// </summary>
  /// <remarks>Some functionality for progress reporting are not implemented</remarks>
  public sealed class DataTableWrapper : DataReaderWrapper
  {

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTableWrapper"/> class.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="addStartLine">if set to <c>true</c> [add start line].</param>
    /// <param name="addEndLine">if set to <c>true</c> [add end line].</param>
    /// <param name="addRecNum">if set to <c>true</c> [add record number].</param>
    /// <param name="addErrorField">if set to <c>true</c> [add error field].</param>
    public DataTableWrapper(in DataTable dataTable,
      bool addStartLine = false,
      bool addEndLine = false,
      bool addRecNum = false,
      bool addErrorField = false)
      : base(dataTable.CreateDataReader(), addStartLine, addEndLine, addRecNum, addErrorField) => DataTable = dataTable;

    /// <summary>
    /// Gets the data table that stores the data
    /// </summary>
    /// <value>
    /// The data table.
    /// </value>
    public DataTable DataTable { get; }

    /// <inheritdoc />
    public override int RecordsAffected => DataTable.Rows.Count;

    /// <inheritdoc />
    public override bool EndOfFile => RecordNumber >= DataTable.Rows.Count;

    /// <inheritdoc />
    public override int Percent => RecordNumber <= 0 ? 0 : (int) (RecordNumber / (double) DataTable.Rows.Count * 100d);

    /// <inheritdoc />
    public override bool SupportsReset => true;

    /// <inheritdoc cref="IFileReader" />
    public new Column GetColumn(int column) => ReaderMapping.Column[column];

    /// <inheritdoc cref="IFileReader" />
    [Obsolete("No need to open a DataTableWrapper, the DataTable is in memory")]
    public new Task OpenAsync(CancellationToken token) => Task.CompletedTask;

    /// <inheritdoc />
    public override void ResetPositionToFirstDataRow()
    {
      Close();
      base.ResetPositionToFirstDataRow();
      DataReader = DataTable.CreateDataReader();
    }
  }
}