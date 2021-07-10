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
    public const int cPartDefault = 2;
    public const string cPartSplitterDefault = ":";
    public const string cDefaultTimePartFormat = "HH:mm:ss";
    public const bool cPartToEnd = true;

    public ImmutableColumn(IColumn col) : this(col.Name, col.ValueFormat, col.ColumnOrdinal, col.Convert,
      col.DestinationName, col.Ignore, col.Part, col.PartSplitter, col.PartToEnd, col.TimePart, col.TimePartFormat,
      col.TimeZonePart)
    {
    }

    public ImmutableColumn(IColumn col, IValueFormat format) : this(col.Name, format, col.ColumnOrdinal, col.Convert,
      col.DestinationName, col.Ignore, col.Part, col.PartSplitter, col.PartToEnd, col.TimePart, col.TimePartFormat,
      col.TimeZonePart)
    {
    }

    public ImmutableColumn(string name, IValueFormat valueFormat, int columnOrdinal, bool? convert = null,
      string destinationName = "",
      bool ignore = false, int part = cPartDefault,
      string partSplitter = cPartSplitterDefault, bool partToEnd = cPartToEnd, string timePart = "",
      string timePartFormat = "",
      string timeZonePart = "")
    {
      Name = name ?? throw new System.ArgumentNullException(nameof(name));
      if (valueFormat is null)
        throw new System.ArgumentNullException(nameof(valueFormat));
      ColumnOrdinal = columnOrdinal;
      Convert = convert ?? valueFormat.DataType != DataType.String;
      DestinationName = destinationName;
      Ignore = ignore;

      Part = part;
      PartSplitter = (partSplitter ?? string.Empty).WrittenPunctuation();
      PartToEnd = partToEnd;
      TimePart = timePart;
      TimePartFormat = timePartFormat;
      TimeZonePart = timeZonePart;

      ValueFormat = valueFormat is ImmutableValueFormat immutable
        ? immutable
        : new ImmutableValueFormat(valueFormat.DataType, valueFormat.DateFormat, valueFormat.DateSeparator,
          valueFormat.TimeSeparator, valueFormat.NumberFormat, valueFormat.GroupSeparator, valueFormat.DecimalSeparator,
          valueFormat.True,
          valueFormat.False, valueFormat.DisplayNullAs);
    }

    public int ColumnOrdinal { get;  }
    public bool Convert { get;  }
    public string DestinationName { get;  }
    public bool Ignore { get;  }
    public string Name { get;  }
    public int Part { get;  }
    public string PartSplitter { get;  }
    public bool PartToEnd { get;  }
    public string TimePart { get;  }
    public string TimePartFormat { get;  }
    public string TimeZonePart { get;  }
    public IValueFormat ValueFormat { get;  }
    public IColumn Clone() => new ImmutableColumn(this);

    public override string ToString() => $"{Name} ({this.GetTypeAndFormatDescription()})";

    public bool Equals(IColumn? other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return ColumnOrdinal == other.ColumnOrdinal && Convert == other.Convert &&
             DestinationName == other.DestinationName && Ignore == other.Ignore && Name == other.Name &&
             Part == other.Part && PartSplitter == other.PartSplitter && PartToEnd == other.PartToEnd &&
             TimePart == other.TimePart && TimePartFormat == other.TimePartFormat &&
             TimeZonePart == other.TimeZonePart && ValueFormat.ValueFormatEqual(other.ValueFormat);
    }

    public override bool Equals(object? obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((ImmutableColumn) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = ColumnOrdinal;
        hashCode = (hashCode * 397) ^ Convert.GetHashCode();
        hashCode = (hashCode * 397) ^ DestinationName.GetHashCode();
        hashCode = (hashCode * 397) ^ Ignore.GetHashCode();
        hashCode = (hashCode * 397) ^ Name.GetHashCode();
        hashCode = (hashCode * 397) ^ Part;
        hashCode = (hashCode * 397) ^ PartSplitter.GetHashCode();
        hashCode = (hashCode * 397) ^ PartToEnd.GetHashCode();
        hashCode = (hashCode * 397) ^ TimePart.GetHashCode();
        hashCode = (hashCode * 397) ^ TimePartFormat.GetHashCode();
        hashCode = (hashCode * 397) ^ TimeZonePart.GetHashCode();
        hashCode = (hashCode * 397) ^ ValueFormat.GetHashCode();
        return hashCode;
      }
    }

  }
}