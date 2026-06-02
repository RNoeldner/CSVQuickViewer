/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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

namespace CsvTools;

/// <inheritdoc />
/// <summary>
///   IFileReader implementation based on a data table, this is used to pass on a data table to a writer
/// </summary>
/// <remarks>Some functionality for progress reporting are not implemented</remarks>
public sealed class DataTableWrapper : DataReaderWrapper
{
  private readonly bool m_AddErrorField;
  /// <summary>
  /// Initializes a new instance of the <see cref="DataTableWrapper"/> class.
  /// </summary>
  /// <param name="dataTable">The data table.</param>
  /// <param name="addStartLine">if set to <c>true</c> [add start line].</param>
  /// <param name="addEndLine">if set to <c>true</c> [add end line].</param>
  /// <param name="addRecNum">if set to <c>true</c> [add record number].</param>
  /// <param name="addErrorField">if set to <c>true</c> [add error field].</param>
  public DataTableWrapper(in DataTable dataTable,
    bool addStartLine = false, bool addEndLine = false,
    bool addRecNum = false, bool addErrorField = false)
    : base(dataTable.CreateDataReader(), addStartLine, addEndLine, addRecNum, addErrorField, dataTable.Rows.Count)
  {
    DataTable = dataTable;
    m_AddErrorField = addErrorField;
  }

  /// <summary>
  /// Gets the data table that stores the data, only used for shortcuts
  /// </summary>
  /// <value>
  /// The data table.
  /// </value>
  internal DataTable DataTable { get; }

  /// <inheritdoc />
  public override int RecordsAffected => DataTable.Rows.Count;

  /// <inheritdoc />
  public override bool SupportsReset => true;

  /// <inheritdoc />
  public override object GetValue(int ordinal)
  {
    var src = base.GetValue(ordinal);

    // Guard: Exit early if we aren't appending error metrics or this isn't the error target field
    if (!m_AddErrorField || ordinal != m_ReaderMapping.ColNumErrorField)
      return src;

    var rowIndex = (RecordNumber - 1).ToInt();

    // Bounds Check Guard: Prevents off-by-one errors before loops start or after a reset operation
    if (rowIndex >= 0 && rowIndex < DataTable.Rows.Count)
    {
      var row = DataTable.Rows[rowIndex];
      var dbRowError = row.GetErrorInformation();

      if (dbRowError.Length > 0)
      {
        var currentError = src?.ToString() ?? string.Empty;
        src = string.IsNullOrEmpty(currentError)
          ? dbRowError
          : currentError.AddMessage(dbRowError);
      }
    }

    return src;
  }

  /// <inheritdoc cref="IFileReader" />
  [Obsolete("No need to open a DataTableWrapper, the DataTable is in memory")]
  // ReSharper disable once UnusedParameter.Global
  public new Task OpenAsync(CancellationToken token) => Task.CompletedTask;

  /// <inheritdoc />
  public override async ValueTask ResetPositionToFirstDataRowAsync(CancellationToken cancellationToken)
  {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    await CloseAsync().ConfigureAwait(false);
#else
    Close();
#endif
    await base.ResetPositionToFirstDataRowAsync(cancellationToken).ConfigureAwait(false);

    if (DataTable != null)
      DataReader = DataTable.CreateDataReader();
  }
}