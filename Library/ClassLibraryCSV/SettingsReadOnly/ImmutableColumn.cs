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


using JetBrains.Annotations;

namespace CsvTools
{
  /// <summary>
  ///   Column information like name, Type, Format etc.
  /// </summary>
  public class ImmutableColumn : IColumn
  {
    private readonly bool? m_Convert;

    private readonly ImmutableValueFormat m_ValueFormat;

    public ImmutableColumn([NotNull] string name,
      [NotNull] IValueFormat valueFormat, int columnOrdinal, bool? convert = null, string destinationName = "",
      bool ignore = false, int part = 0,
      char partSplitter = '\0', bool partToEnd = true, string timePart = "", string timePartFormat = "",
      string timeZonePart = "")
    {
      ColumnOrdinal = columnOrdinal;
      m_Convert = convert;
      DestinationName = destinationName;
      Ignore = ignore;
      Name = name;
      Part = part;
      PartSplitter = partSplitter;
      PartToEnd = partToEnd;
      TimePart = timePart;
      TimePartFormat = timePartFormat;
      TimeZonePart = timeZonePart;

      m_ValueFormat = valueFormat is ImmutableValueFormat immutable
        ? immutable
        : new ImmutableValueFormat(valueFormat);
    }

    public ImmutableColumn(IColumn other, int ordinal) : this(other.Name, other.ValueFormat,
      ordinal, other.Convert, other.DestinationName, other.Ignore, other.Part, other.PartSplitter, other.PartToEnd,
      other.TimePart, other.TimePartFormat, other.TimeZonePart)
    {
    }

    public ImmutableColumn(IColumn other, IValueFormat newValueFormat) : this(other.Name, newValueFormat,
      other.ColumnOrdinal, other.Convert, other.DestinationName, other.Ignore, other.Part, other.PartSplitter,
      other.PartToEnd, other.TimePart, other.TimePartFormat, other.TimeZonePart)
    {
    }

    public int ColumnOrdinal { get; }

    public bool Convert => m_Convert ?? m_ValueFormat.DataType != DataType.String;

    public string DestinationName { get; }
    public bool Ignore { get; }
    public string Name { get; }
    public int Part { get; }
    public char PartSplitter { get; }
    public bool PartToEnd { get; }
    public string TimePart { get; }
    public string TimePartFormat { get; }
    public string TimeZonePart { get; }
    public IValueFormat ValueFormat => m_ValueFormat;

    public override string ToString() => $"{Name} ({this.GetTypeAndFormatDescription()})";
  }
}