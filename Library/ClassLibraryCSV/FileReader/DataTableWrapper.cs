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
    public DataTableWrapper(in DataTable dataTable)
      : base(dataTable.CreateDataReader()) => DataTable = dataTable;

    public DataTable DataTable { get; }

    public override int RecordsAffected => DataTable.Rows.Count;

    public override bool EndOfFile => RecordNumber >= DataTable.Rows.Count;

    /// <summary>
    /// Gets the percentage of teh processed records in the data table
    /// </summary>
    /// <value>
    /// The percent as value between 0 and 100
    /// </value>
    public override int Percent => RecordNumber <= 0 ? 0 : (int) (RecordNumber / (double) DataTable.Rows.Count * 100d);

    public override bool SupportsReset => true;

    public new IColumn GetColumn(int column) => ReaderMapping.Column[column];

    [Obsolete("No need to open a DataTableWrapper, the DataTable is in memory")]
    public override Task OpenAsync(CancellationToken token) => Task.CompletedTask;
    
    /// <summary>
    ///   Asynchronous Read of next record
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns></returns>
    public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
      if (!cancellationToken.IsCancellationRequested && !EndOfFile)
      {
        var couldRead = await base.ReadAsync(cancellationToken).ConfigureAwait(false);
        if (couldRead)
          return true;
      }

      return false;
    }

    /// <summary>
    ///   Resets the position and buffer to the first data row (handing headers, and skipped rows)
    /// </summary>
    public override void ResetPositionToFirstDataRow()
    {
      Close();
      base.ResetPositionToFirstDataRow();
      DataReader = DataTable.CreateDataReader();
    }
  }
}