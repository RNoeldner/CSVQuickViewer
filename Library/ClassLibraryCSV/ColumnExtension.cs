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

using System.Text;

namespace CsvTools
{
  public static class ColumnExtension
  {
    /// <summary>
    ///   Gets the a description of the Date or Number format
    /// </summary>
    /// <returns></returns>
    public static string GetFormatDescription(this IColumn column)
    {
      if (column.ValueFormat.DataType == DataType.TextPart)
        return column.Part + (column.PartToEnd ? " To End" : string.Empty);

      return column.ValueFormat.GetFormatDescription();
    }

    /// <summary>
    ///   Gets the description.
    /// </summary>
    /// <returns></returns>
    public static string GetTypeAndFormatDescription(this IColumn column, bool addTime = true)
    {
      var stringBuilder = new StringBuilder(column.ValueFormat.DataType.DataTypeDisplay());

      var shortDesc = column.GetFormatDescription();
      if (shortDesc.Length > 0)
      {
        stringBuilder.Append(" (");
        stringBuilder.Append(shortDesc);
        stringBuilder.Append(")");
      }

      if (addTime && column.ValueFormat.DataType == DataType.DateTime)
      {
        if (column.TimePart.Length > 0)
        {
          stringBuilder.Append(" + ");
          stringBuilder.Append(column.TimePart);
          if (column.TimePartFormat.Length > 0)
          {
            stringBuilder.Append(" (");
            stringBuilder.Append(column.TimePartFormat);
            stringBuilder.Append(")");
          }
        }

        if (column.TimeZonePart.Length > 0)
        {
          stringBuilder.Append(" - ");
          stringBuilder.Append(column.TimeZonePart);
        }
      }

      if (column.Ignore)
        stringBuilder.Append(" (Ignore)");

      return stringBuilder.ToString();
    }

    public static Column ToMutable(this IColumn other) => new Column(other.Name, new ValueFormatMutable
    {
      DataType = other.ValueFormat.DataType,
      DateFormat = other.ValueFormat.DateFormat,
      DateSeparator = other.ValueFormat.DateSeparator,
      DecimalSeparator = other.ValueFormat.DecimalSeparator,
      DisplayNullAs = other.ValueFormat.DisplayNullAs,
      False = other.ValueFormat.False,
      GroupSeparator = other.ValueFormat.GroupSeparator,
      NumberFormat = other.ValueFormat.NumberFormat,
      TimeSeparator = other.ValueFormat.TimeSeparator,
      True = other.ValueFormat.True
    })
    {
      ColumnOrdinal = other.ColumnOrdinal,
      Convert = other.Convert,
      DestinationName = other.DestinationName,
      Ignore = other.Ignore,
      Part = other.Part,
      PartSplitter = other.PartSplitter,
      PartToEnd = other.PartToEnd,
      TimePart = other.TimePart,
      TimePartFormat = other.TimePartFormat,
      TimeZonePart = other.TimeZonePart
    };
  }
}