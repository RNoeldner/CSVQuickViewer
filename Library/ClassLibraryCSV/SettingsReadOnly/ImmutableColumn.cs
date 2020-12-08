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
  /// <summary>
  ///   Column information like name, Type, Format etc.
  /// </summary>
  public class ImmutableColumn : IColumn
  {

    public ImmutableColumn(string name, IValueFormat valueFormat, int columnOrdinal, bool? convert = null, string destinationName = "",
      bool ignore = false, int part = 0,
      char partSplitter = '\0', bool partToEnd = true, string timePart = "", string timePartFormat = "",
      string timeZonePart = "")
    {
      Name = name??throw new System.ArgumentNullException(nameof(name));
      if (valueFormat is null)
        throw new System.ArgumentNullException(nameof(valueFormat));
      ColumnOrdinal = columnOrdinal;
      Convert = convert??valueFormat.DataType != DataType.String;
      DestinationName = destinationName;
      Ignore = ignore;

      Part = part;
      PartSplitter = partSplitter;
      PartToEnd = partToEnd;
      TimePart = timePart;
      TimePartFormat = timePartFormat;
      TimeZonePart = timeZonePart;

      ValueFormat = valueFormat is ImmutableValueFormat immutable
        ? immutable
        : new ImmutableValueFormat(valueFormat.DataType, valueFormat.DateFormat, valueFormat.DateSeparator,
      valueFormat.DecimalSeparatorChar, valueFormat.DisplayNullAs, valueFormat.False, valueFormat.GroupSeparatorChar, valueFormat.NumberFormat,
      valueFormat.TimeSeparator, valueFormat.True);
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

    public override string ToString() => $"{Name} ({this.GetTypeAndFormatDescription()})";
  }
}