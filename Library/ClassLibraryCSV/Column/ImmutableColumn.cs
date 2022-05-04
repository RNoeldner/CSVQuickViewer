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

using System;
using System.Collections.Generic;

namespace CsvTools
{
  /// <inheritdoc cref="CsvTools.IColumn" />
  /// <summary>
  ///   Column information like name, Type, Format etc.
  /// </summary>
  public class ImmutableColumn : IColumn, IEqualityComparer<ImmutableColumn>
  {
    public const string cDefaultTimePartFormat = "HH:mm:ss";

    public ImmutableColumn(in IColumn col)
      : this(
        col.Name,
        col.ValueFormat,
        col.ColumnOrdinal,
        col.Convert,
        col.DestinationName,
        col.Ignore,
        col.TimePart,
        col.TimePartFormat,
        col.TimeZonePart)
    {
    }

    public ImmutableColumn(in IColumn col, in IValueFormat format)
      : this(
        col.Name,
        format,
        col.ColumnOrdinal,
        col.Convert,
        col.DestinationName,
        col.Ignore,
        col.TimePart,
        col.TimePartFormat,
        col.TimeZonePart)
    {
    }

    public ImmutableColumn(
      in string name,
      in IValueFormat valueFormat,
      int columnOrdinal,
      bool? convert = null,
      in string destinationName = "",
      bool ignore = false,
      in string timePart = "",
      in string timePartFormat = "",
      in string timeZonePart = "")
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      if (valueFormat is null)
        throw new ArgumentNullException(nameof(valueFormat));
      ColumnOrdinal = columnOrdinal;
      Convert = convert ?? valueFormat.DataType != DataTypeEnum.String;
      DestinationName = destinationName;
      Ignore = ignore;

      TimePart = timePart;
      TimePartFormat = timePartFormat;
      TimeZonePart = timeZonePart;

      ValueFormat = valueFormat is ImmutableValueFormat immutable
                      ? immutable
                      : new ImmutableValueFormat(valueFormat);
      ColumnFormatter = ColumnFormatterFactory.GetColumnFormatter(columnOrdinal, valueFormat);
    }

    public IColumnFormatter? ColumnFormatter { get; }

    /// <inheritdoc />
    public int ColumnOrdinal { get; }

    /// <inheritdoc />
    public bool Convert { get; }

    /// <inheritdoc />
    public string DestinationName { get; }

    /// <inheritdoc />
    public bool Ignore { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string TimePart { get; }

    /// <inheritdoc />
    public string TimePartFormat { get; }

    /// <inheritdoc />
    public string TimeZonePart { get; }
    
    public IValueFormat ValueFormat { get; }

    public object Clone() => new ImmutableColumn(this);

    public bool Equals(IColumn? other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;
      return ColumnOrdinal == other.ColumnOrdinal && Convert == other.Convert
                                                  && DestinationName == other.DestinationName && Ignore == other.Ignore
                                                  && Name == other.Name && TimePart == other.TimePart
                                                  && TimePartFormat == other.TimePartFormat
                                                  && TimeZonePart == other.TimeZonePart
                                                  && ValueFormat.ValueFormatEqual(other.ValueFormat);
    }

    public bool Equals(ImmutableColumn x, ImmutableColumn y) => x.Equals(y);

    public int GetHashCode(ImmutableColumn obj) => obj.GetHashCode();

    public override bool Equals(object? obj)
    {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((ImmutableColumn) obj);
    }

   

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = ColumnOrdinal;
        hashCode = hashCode * 397 ^ Convert.GetHashCode();
        hashCode = hashCode * 397 ^ DestinationName.GetHashCode();
        hashCode = hashCode * 397 ^ Ignore.GetHashCode();
        hashCode = hashCode * 397 ^ Name.GetHashCode();
        hashCode = hashCode * 397 ^ TimePart.GetHashCode();
        hashCode = hashCode * 397 ^ TimePartFormat.GetHashCode();
        hashCode = hashCode * 397 ^ TimeZonePart.GetHashCode();
        hashCode = hashCode * 397 ^ ValueFormat.GetHashCode();
        return hashCode;
      }
    }

    public override string ToString() => $"{Name} ({this.GetTypeAndFormatDescription()})";
  }
}