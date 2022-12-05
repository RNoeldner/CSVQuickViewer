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
  public sealed class WriterColumn : Column
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
      ConstantTimeZone = constantTimeZone;
      ColumnOrdinalTimeZone = columnOrdinalTimeZone;
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

    public string ConstantTimeZone { get; }

    /// <summary>
    ///   Gets or sets the length of the field, this is needed for writing Fixed Length Text files
    /// </summary>
    /// <value>The length of the field. 0 means unrestricted length</value>
    public int FieldLength { get; }
  }
}