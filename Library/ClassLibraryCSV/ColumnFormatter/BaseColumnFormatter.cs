#nullable enable
using System;
using System.Data;

namespace CsvTools
{
  /// <summary>
  /// Base class for all ColumnFormatters
  /// </summary>
  public abstract class BaseColumnFormatter : IColumnFormatter
  {

    /// <inheritdoc/>
    public abstract string FormatInputText(in string inputString, Action<string>? handleWarning);

    /// <inheritdoc/>
    public virtual string Write(object? dataObject, IDataRecord? dataRow, Action<string>? handleWarning)
      => dataObject?.ToString() ?? string.Empty;

    /// <inheritdoc/>
    public bool RaiseWarning { get; set; } = true;
  }
}