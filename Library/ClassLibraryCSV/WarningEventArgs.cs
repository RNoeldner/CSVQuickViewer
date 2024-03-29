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

#nullable enable

using System;
using System.Text;

namespace CsvTools
{
  /// <summary>
  ///   Argument for a WarningEvent
  /// </summary>
  public sealed class WarningEventArgs : EventArgs
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="WarningEventArgs" /> class.
    /// </summary>
    /// <param name="recordNumber">Number of the record</param>
    /// <param name="columnNumber">Ordinal number of the column</param>
    /// <param name="warningMessage">Message to be stored for the column</param>
    /// <param name="lineNumberEnd">Line Number where the record ended</param>
    /// <param name="lineNumberStart">Line Number where the record started</param>
    /// <param name="columnName">Name of the column</param>
    public WarningEventArgs(
      long recordNumber,
      int columnNumber,
      in string warningMessage,
      long lineNumberStart,
      long lineNumberEnd,
      in string? columnName)
    {
      if (string.IsNullOrEmpty(warningMessage)) throw new ArgumentException("message", nameof(warningMessage));

      RecordNumber = recordNumber;
      ColumnNumber = columnNumber;
      Message = warningMessage;
      LineNumberStart = lineNumberStart;
      LineNumberEnd = lineNumberEnd;
      ColumnName = columnName;
    }

    /// <summary>
    ///   Gets or sets the name of the column.
    /// </summary>
    /// <value>The name of the column.</value>
    public string? ColumnName { get; set; }

    /// <summary>
    ///   Gets or sets the column number.
    /// </summary>
    /// <value>The column number.</value>
    public int ColumnNumber { get; set; }

    /// <summary>
    ///   Gets or sets the line number the record ends.
    /// </summary>
    /// <value>The line number end.</value>
    public long LineNumberEnd { get; set; }

    /// <summary>
    ///   Gets or sets the line number the record started.
    /// </summary>
    /// <value>The line number start.</value>
    public long LineNumberStart { get; set; }

    /// <summary>
    ///   Gets or sets the Message to be stored for the column.
    /// </summary>
    /// <value>The message.</value>
    public string Message { get; set; }

    /// <summary>
    ///   Gets or sets the record number.
    /// </summary>
    /// <value>The record number.</value>
    public long RecordNumber { get; set; }

    /// <summary>
    ///   Gets the information for display
    /// </summary>
    /// <param name="addLocationInfoToWarning">Add line number information if set true</param>
    /// <param name="addColumnInfoToWarning">Add column information if set true</param>
    /// <value>A nicely formatted representation of the information</value>
    public string Display(bool addLocationInfoToWarning, bool addColumnInfoToWarning)
    {
      var sb = new StringBuilder();

      if (addLocationInfoToWarning && (LineNumberEnd > 0 || LineNumberStart > 0))
      {
        sb.Append("Line ");
        if (LineNumberStart > 0)
        {
          sb.Append(LineNumberStart);
          if (LineNumberStart < LineNumberEnd)
            sb.Append(" - ");
        }

        if (LineNumberStart < LineNumberEnd)
          sb.Append(LineNumberEnd);
      }

      if (addColumnInfoToWarning && !string.IsNullOrEmpty(ColumnName))
      {
        if (sb.Length > 0)
          sb.Append(' ');

        sb.Append("Column [");
        if (ColumnName?.Length > 40)
        {
          sb.Append(ColumnName.Substring(0, 39));
          sb.Append('…');
        }
        else
        {
          sb.Append(ColumnName);
        }

        sb.Append(']');
      }

      if (sb.Length > 0)
        sb.Append(": ");
      sb.Append(Message);

      return sb.ToString();
    }
  }
}