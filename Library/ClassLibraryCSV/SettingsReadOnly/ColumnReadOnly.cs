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

namespace CsvTools
{
  /// <summary>
  ///   Column information like name, Type, Format etc.
  /// </summary>
  public class ColumnReadOnly : IColumn
  {
    public ColumnReadOnly(string name,
      IValueFormat valueFormat, int columnOrdinal, bool convert = false, string destinationName = "", bool ignore = false, int part = 0,
      char partSplitter = '\0', bool partToEnd = true, string timePart = "", string timePartFormat = "", string timeZonePart = "")
    {
      ColumnOrdinal = columnOrdinal;
      Convert = convert;
      DestinationName = destinationName;
      Ignore = ignore;
      Name = name;
      Part = part;
      PartSplitter = partSplitter;
      PartToEnd = partToEnd;
      TimePart = timePart;
      TimePartFormat = timePartFormat;
      TimeZonePart = timeZonePart;
      ValueFormat = valueFormat;
    }

    public ColumnReadOnly(IColumn other, int ordinal) : this(other.Name, new ValueFormatReadOnly(other.ValueFormat), ordinal, other.Convert, other.DestinationName, other.Ignore, other.Part, other.PartSplitter, other.PartToEnd, other.TimePart, other.TimePartFormat, other.TimeZonePart)
    {
    }

    public ColumnReadOnly(IColumn other, IValueFormat newValueFormat) : this(other.Name, newValueFormat, other.ColumnOrdinal, other.Convert, other.DestinationName, other.Ignore, other.Part, other.PartSplitter, other.PartToEnd, other.TimePart, other.TimePartFormat, other.TimeZonePart)
    {
    }

    public int ColumnOrdinal { get; }
    public bool Convert { get; }
    public string DestinationName { get; }
    public bool Ignore { get; }
    public string Name { get; }
    public int Part { get; }
    public char PartSplitter { get; }
    public bool PartToEnd { get; }
    public string TimePart { get; }
    public string TimePartFormat { get; }
    public string TimeZonePart { get; }
    public IValueFormat ValueFormat { get; }

    //public bool Equals(IColumn other)
    //{
    //  if (ReferenceEquals(null, other)) return false;
    //  if (ReferenceEquals(this, other)) return true;
    //  return // ColumnOrdinal == other.ColumnOrdinal && 
    //    Convert == other.Convert &&
    //    DestinationName == other.DestinationName && Ignore == other.Ignore &&
    //    Name == other.Name && Part == other.Part &&
    //    PartSplitter == other.PartSplitter && PartToEnd == other.PartToEnd &&
    //    TimePart == other.TimePart && TimePartFormat == other.TimePartFormat &&
    //    TimeZonePart == other.TimeZonePart &&
    //    Equals(ValueFormat, other.ValueFormat);
    //}

    public Column ToMutable() => new Column(Name, new ValueFormat
    {
      DataType = ValueFormat.DataType,
      DateFormat = ValueFormat.DateFormat,
      DateSeparator = ValueFormat.DateSeparator,
      DecimalSeparator = ValueFormat.DecimalSeparatorChar.ToString(),
      DisplayNullAs = ValueFormat.DisplayNullAs,
      False = ValueFormat.False,
      GroupSeparator = ValueFormat.GroupSeparatorChar.ToString(),
      NumberFormat = ValueFormat.NumberFormat,
      TimeSeparator = ValueFormat.TimeSeparator,
      True = ValueFormat.True
    })
    {
      ColumnOrdinal = ColumnOrdinal,
      Convert = Convert,
      DestinationName = DestinationName,
      Ignore = Ignore,
      Part = Part,
      PartSplitter = PartSplitter,
      PartToEnd = PartToEnd,
      TimePart = TimePart,
      TimePartFormat = TimePartFormat,
      TimeZonePart = TimeZonePart
    };
    public override string ToString() => $"{Name} ({this.GetTypeAndFormatDescription()})";

  //  public override bool Equals(object obj)
  //  {
  //    if (ReferenceEquals(null, obj)) return false;
  //    if (ReferenceEquals(this, obj)) return true;
  //    if (obj.GetType() != GetType()) return false;
  //    return Equals((ColumnReadOnly) obj);
  //  }

  //  public override int GetHashCode()
  //  {
  //    unchecked
  //    {
  //      var hashCode = ColumnOrdinal;
  //      hashCode = (hashCode * 397) ^ Convert.GetHashCode();
  //      hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(DestinationName);
  //      hashCode = (hashCode * 397) ^ Ignore.GetHashCode();
  //      hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
  //      hashCode = (hashCode * 397) ^ Part;
  //      hashCode = (hashCode * 397) ^ PartSplitter.GetHashCode();
  //      hashCode = (hashCode * 397) ^ PartToEnd.GetHashCode();
  //      hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(TimePart);
  //      hashCode = (hashCode * 397) ^ TimePartFormat.GetHashCode();
  //      hashCode = (hashCode * 397) ^ TimeZonePart.GetHashCode();
  //      hashCode = (hashCode * 397) ^ ValueFormat.GetHashCode();
  //      return hashCode;
  //    }
  //  }
  }
}