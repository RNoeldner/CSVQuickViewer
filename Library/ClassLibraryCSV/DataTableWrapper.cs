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

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   IFileReader implementation based on a data table, this is used to pass on a data table to a writer
  /// </summary>
  /// <remarks>Some functionality for progress reporting are not implemented</remarks>
  public sealed class DataTableWrapper : DataReaderWrapper, IFileReader
  {
    private bool m_DisposedValue;
    
    public DataTableWrapper([NotNull] DataTable dataTable) : base(dataTable?.CreateDataReader() ?? throw new ArgumentNullException(nameof(dataTable)),
      dataTable.Rows.Count, false, false, false, false) => DataTable = dataTable;

    [Obsolete("Not needed for DataTableWrapper")]
    public event EventHandler<RetryEventArgs> OnAskRetry;

    public event EventHandler<IReadOnlyCollection<IColumn>> OpenFinished;

    public event EventHandler ReadFinished;

    [Obsolete("Not needed for DataTableWrapper")]
    public event EventHandler<WarningEventArgs> Warning;

    [NotNull] public DataTable DataTable { get; }

    public Func<Task> OnOpen { get; set; }

    public bool SupportsReset => true;

    public ImmutableColumn GetColumn(int column) => ReaderMapping.Column[column];

    [Obsolete("No need to open a DataTableWrapper")]
    public async Task OpenAsync(CancellationToken token)
    {
      if (OnOpen != null) await OnOpen.Invoke();
      if (OpenFinished != null) OpenFinished.Invoke(this, base.ReaderMapping.Column);
    }

    public override async Task<bool> ReadAsync(CancellationToken token)
    {
      if (!token.IsCancellationRequested && !EndOfFile)
      {
        var couldRead = await base.ReadAsync(token).ConfigureAwait(false);
        if (couldRead)
          return true;
      }

      ReadFinished?.Invoke(this, new EventArgs());
      return false;
    }

    public async Task ResetPositionToFirstDataRowAsync(CancellationToken token)
    {
      base.m_DataReader.Close();
      base.m_DataReader = DataTable.CreateDataReader();
      base.m_RecordNumber = 0;
    }

    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (disposing)
      {
        m_DisposedValue = true;
        DataTable.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}