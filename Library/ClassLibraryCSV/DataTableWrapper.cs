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
      : base(
        dataTable.CreateDataReader(),
        // passing in number of records so Percent can be calculated
        dataTable.Rows.Count) =>
      DataTable = dataTable;

    public DataTable DataTable { get; }

    public new bool SupportsReset => true;

    public new IColumn GetColumn(int column) => ReaderMapping.Column[column];

    [Obsolete("No need to open a DataTableWrapper, the DataTable is in memory")]
#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
    public new async Task OpenAsync(CancellationToken token)
    {
      // ignored, open is not needed
    }

#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.

    /// <summary>
    ///   Asynchronous Read of next record
    /// </summary>
    /// <param name="token">The cancellation token.</param>
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
    public new void ResetPositionToFirstDataRow()
    {
      Close();
      DataReader = DataTable.CreateDataReader();
    }
  }
}