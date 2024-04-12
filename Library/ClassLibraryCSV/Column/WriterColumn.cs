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
  /// <inheritdoc />
#if NETFRAMEWORK
  public sealed class WriterColumn : Column
#else
  public record WriterColumn : Column
#endif
  {
    /// <inheritdoc />
    public WriterColumn(in string name,
      in ValueFormat valueFormat,
      int colNum,
      int fieldLength = 0,
      in string constantTimeZone = "",
      int columnOrdinalTimeZone = -1)
      : base(name, valueFormat, colNum)
    {
      FieldLength = fieldLength;
      OutputTimeZone = constantTimeZone;
      ColumnOrdinalTimeZone = columnOrdinalTimeZone;

      // Writing data Formats should not have empty formats
      if (string.IsNullOrEmpty(ValueFormat.True) ||
          string.IsNullOrEmpty(ValueFormat.False) ||
          string.IsNullOrEmpty(ValueFormat.DateFormat) ||
          string.IsNullOrEmpty(ValueFormat.NumberFormat))
      {
        ValueFormat = new ValueFormat(
          ValueFormat.DataType,
          string.IsNullOrEmpty(ValueFormat.DateFormat) ? ValueFormat.Empty.DateFormat : ValueFormat.DateFormat,
          ValueFormat.DateSeparator.ToStringHandle0(),
          ValueFormat.TimeSeparator.ToStringHandle0(),
          string.IsNullOrEmpty(ValueFormat.NumberFormat) ? ValueFormat.Empty.NumberFormat : ValueFormat.NumberFormat,
          ValueFormat.GroupSeparator.ToStringHandle0(),
          ValueFormat.DecimalSeparator.ToStringHandle0(),
          string.IsNullOrEmpty(ValueFormat.True) ? "true" : ValueFormat.True,
          string.IsNullOrEmpty(ValueFormat.False) ? "false" : ValueFormat.False,
          ValueFormat.DisplayNullAs,
          ValueFormat.Part,
          ValueFormat.PartSplitter.ToStringHandle0(),
          ValueFormat.PartToEnd,
          ValueFormat.RegexSearchPattern,
          ValueFormat.RegexReplacement,
          ValueFormat.ReadFolder,
          ValueFormat.WriteFolder,
          ValueFormat.FileOutPutPlaceholder, ValueFormat.Overwrite);
      }
    }

    /// <summary>
    ///   Gets the column ordinal of the time zone column
    /// </summary>
    /// <value>The column ordinal time zone.</value>
    public int ColumnOrdinalTimeZone { get; }

    /// <summary>
    ///   Gets the constant time zone
    /// </summary>
    /// <value>The constant time zone.</value>

    public string OutputTimeZone { get; }

    /// <summary>
    ///   Gets or sets the length of the field, this is needed for writing Fixed Length Text files
    /// </summary>
    /// <value>The length of the field. 0 means unrestricted length</value>
    public int FieldLength { get; }
  }
}