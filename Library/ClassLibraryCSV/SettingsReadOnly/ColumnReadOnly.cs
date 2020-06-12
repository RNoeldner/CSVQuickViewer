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
using System.Runtime.Remoting.Messaging;

namespace CsvTools
{
  /// <summary>
  ///   Column information like name, Type, Format etc.
  /// </summary>
  public class ColumnReadOnly : IColumn, IEquatable<IColumn>
  {
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

    public ColumnReadOnly(int columnOrdinal, bool convert, string destinationName, bool ignore, string name, int part, char partSplitter, bool partToEnd, string timePart, string timePartFormat, string timeZonePart, IValueFormat valueFormat)
    {
      ColumnOrdinal =columnOrdinal;
      Convert =convert;
      DestinationName =destinationName;
      Ignore =ignore;
      Name =name;
      Part =part;
      PartSplitter =partSplitter;
      PartToEnd =partToEnd;
      TimePart =timePart;
      TimePartFormat =timePartFormat;
      TimeZonePart =timeZonePart;
      ValueFormat = valueFormat;
    }

    public static ColumnReadOnly From(Column rw) => new ColumnReadOnly(rw.ColumnOrdinal, rw.Convert, rw.DestinationName,
      rw.Ignore, rw.Name, rw.Part, rw.PartSplitter, rw.PartToEnd, rw.TimePart, rw.TimePartFormat, rw.TimeZonePart,
      ValueFormatReadOnly.ReadOnly(rw.ValueFormat));

    public bool Equals(IColumn other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return ColumnOrdinal == other.ColumnOrdinal && Convert == other.Convert &&
        DestinationName == other.DestinationName && Ignore == other.Ignore &&
        Name == other.Name && Part == other.Part &&
        PartSplitter == other.PartSplitter && PartToEnd == other.PartToEnd &&
        TimePart == other.TimePart && TimePartFormat == other.TimePartFormat &&
        TimeZonePart == other.TimeZonePart &&
        Equals(ValueFormat, other.ValueFormat);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((ColumnReadOnly) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = ColumnOrdinal;
        hashCode = (hashCode * 397) ^ Convert.GetHashCode();
        hashCode = (hashCode * 397) ^ (StringComparer.OrdinalIgnoreCase.GetHashCode(DestinationName));
        hashCode = (hashCode * 397) ^ Ignore.GetHashCode();
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Name);
        hashCode = (hashCode * 397) ^ Part;
        hashCode = (hashCode * 397) ^ PartSplitter.GetHashCode();
        hashCode = (hashCode * 397) ^ PartToEnd.GetHashCode();
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(TimePart);
        hashCode = (hashCode * 397) ^ TimePartFormat.GetHashCode();
        hashCode = (hashCode * 397) ^ TimeZonePart.GetHashCode();
        hashCode = (hashCode * 397) ^ ValueFormat.GetHashCode();
        return hashCode;
      }
    }
  }
}