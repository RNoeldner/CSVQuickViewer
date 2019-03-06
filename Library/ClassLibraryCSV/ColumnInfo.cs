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

using System.Diagnostics;

namespace CsvTools
{
  /// <summary>
  ///   ColumnInfo
  /// </summary>
  [DebuggerDisplay("ColumnInfo( {Header} {DataType} - {Column.GetFormatDescription})")]
  public class ColumnInfo
  {
    /// <summary>
    ///   Gets or sets the column format.
    /// </summary>
    /// <value>The column format.</value>
    public virtual Column Column { get; set; }

    /// <summary>
    ///   Gets or sets the reader column ordinal
    /// </summary>
    /// <value>The column ordinal.</value>
    public virtual int ColumnOridinalReader { get; set; } = -1;

    public int ColumnOridinalTimeZoneReader { get; set; } = -1;

    /// <summary>
    ///   Gets or sets the type of the data.
    /// </summary>
    /// <value>The type of the data.</value>
    public virtual DataType DataType { get; set; }

    /// <summary>
    ///   Gets or sets the length of the field.
    /// </summary>
    /// <value>The length of the field. 0 means unrestricted length</value>
    public virtual int FieldLength { get; set; }

    /// <summary>
    ///   Gets or sets the header.
    /// </summary>
    /// <value>The header.</value>
    public virtual string Header { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether is a time part
    /// </summary>
    /// <value><c>true</c> if is a time part of another field; otherwise, <c>false</c>.</value>
    public virtual bool IsTimePart { get; set; }

    /// <summary>
    ///   Gets or sets the value format.
    /// </summary>
    /// <value>The value format.</value>
    public virtual ValueFormat ValueFormat { get; set; }
  }
}