/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace CsvTools
{
  /// <summary>
  ///   Column information like name, Type, Format etc.
  /// </summary>
  [DebuggerDisplay("Column {Name}")]
#if NETFRAMEWORK
  public class Column : IEquatable<Column>, ICollectionIdentity
#else
  public record Column : ICollectionIdentity
#endif
  {
    /// <summary>
    /// Default Format for Time is 24 hrs clock with seconds
    /// </summary>
    public const string cDefaultTimePartFormat = "HH:mm:ss";

    /// <summary>
    /// Initializes a new instance of the <see cref="Column"/> class.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <param name="valueFormat">The format of the column.</param>
    /// <param name="columnOrdinal">The column ordinal.</param>
    /// <param name="ignore">if set to <c>true</c> the column will be ignored.</param>
    /// <param name="convert">If Conversion is necessary, usually yes if the format is non string.</param>
    /// <param name="destinationName">Name of the destination.</param>
    /// <param name="timePart">The time part for date time information provided in two columns.</param>
    /// <param name="timePartFormat">The time part format for date time information provided in two columns</param>
    /// <param name="timeZonePart">The time zone part for date time information provided in multiple columns</param>
    /// <exception cref="System.ArgumentNullException">name</exception>
    public Column(string name,
      in ValueFormat? valueFormat = null,
      int? columnOrdinal = 0,
      bool? ignore = false,
      bool? convert = null,
      string? destinationName = "",
      string? timePart = "",
      string? timePartFormat = cDefaultTimePartFormat,
      string? timeZonePart = "")
    {
      Name = name ?? throw new ArgumentNullException(nameof(name));
      ValueFormat = valueFormat ?? ValueFormat.Empty;
      ColumnOrdinal = columnOrdinal < 0 ? 0 : columnOrdinal ?? 0;
      Ignore = ignore ?? false;
      Convert = convert ?? ValueFormat.DataType != DataTypeEnum.String;
      DestinationName = destinationName ?? string.Empty;
      TimePart = timePart ?? string.Empty;
      TimePartFormat = timePartFormat ?? cDefaultTimePartFormat;
      TimeZonePart = timeZonePart ?? string.Empty;
      ColumnFormatter = FunctionalDI.GetColumnFormatter(ValueFormat);
    }

    /// <summary>
    ///   Name of the column
    /// </summary>
    [DefaultValue("")]
    public string Name { get; }

    /// <summary>
    ///   Gets the column ordinal
    /// </summary>
    [DefaultValue(0)]
    public int ColumnOrdinal { get; }

    /// <summary>
    ///   Formatting option for values
    /// </summary>
    public ValueFormat ValueFormat { get; protected set;}

    /// <summary>
    /// Identifier in collections, similar to a hashcode based on a  properties that should be unique in a collection
    /// </summary>
    /// <remarks>
    /// In case a required property is not set, this should raise an error
    /// </remarks>
    [JsonIgnore]
    public int CollectionIdentifier => Name.IdentifierHash();

    /// <summary>
    ///  Get the ColumnFormatter Class from <see cref="FunctionalDI.GetColumnFormatter"/>
    ///  Only an Immutable Column does have a ColumnFormatter
    /// </summary>
    [JsonIgnore]
    public IColumnFormatter ColumnFormatter
    {
      get;
    }

    /// <summary>
    ///   Indicating if the column should be ignored during read or write, no conversion is done if
    ///   the column is ignored the target will not show this column
    /// </summary>
    [DefaultValue(false)]
    public bool Ignore { get; }

    /// <summary>
    ///   Indicating if the value or text should be converted
    /// </summary>
    [DefaultValue(false)]
    public bool Convert { get; }

    /// <summary>
    ///   Name of the column in the destination system
    /// </summary>
    [DefaultValue("")]
    public string DestinationName { get; }

    /// <summary>
    ///   For DateTime import you can combine a date and a time column into a single datetime
    ///   column, to do this specify the time column on the column for the date
    /// </summary>
    [DefaultValue("")]
    public string TimePart { get; }

    /// <summary>
    ///   For DateTime import you can combine a date and a time into a single datetime column,
    ///   specify the format of the time here this can different from the format of the time column
    ///   itself that is often ignored
    /// </summary>
    [DefaultValue(cDefaultTimePartFormat)]
    public string TimePartFormat { get; }

    /// <summary>
    ///   For DateTime import you set a time zone, during read the value provided will be assumed to
    ///   be of the timezone specified During write the time will be converted into this time zone
    /// </summary>
    [DefaultValue("")]
    public string TimeZonePart { get; }
#if NETFRAMEWORK
    /// <inheritdoc />
    public bool Equals(Column? other)
    {
      if (other is null) return false;
      
      return ColumnOrdinal == other.ColumnOrdinal
             && Convert == other.Convert
             && DestinationName == other.DestinationName && Ignore == other.Ignore
             && Name == other.Name && TimePart == other.TimePart
             && TimePartFormat == other.TimePartFormat
             && TimeZonePart == other.TimeZonePart
             && ValueFormat.Equals(other.ValueFormat);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = ColumnOrdinal;
        hashCode = (hashCode * 397) ^ Convert.GetHashCode();
        hashCode = (hashCode * 397) ^ DestinationName.GetHashCode();
        hashCode = (hashCode * 397) ^ Ignore.GetHashCode();
        hashCode = (hashCode * 397) ^ Name.GetHashCode();
        hashCode = (hashCode * 397) ^ TimePart.GetHashCode();
        hashCode = (hashCode * 397) ^ TimePartFormat.GetHashCode();
        hashCode = (hashCode * 397) ^ TimeZonePart.GetHashCode();
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        hashCode = (hashCode * 397) ^ ValueFormat.GetHashCode();
        return hashCode;
      }
    }
#endif
    /// <summary>
    ///  Create a copy of the current column with different value format
    /// </summary>
    /// <param name="newFormat"></param>
    /// <returns></returns>
    public Column ReplaceValueFormat(in ValueFormat newFormat) =>
      new Column(Name, newFormat, ColumnOrdinal, Ignore, Convert, DestinationName, TimePart, TimePartFormat, TimeZonePart);

    /// <inheritdoc />
    public override string ToString() => $"{Name} ({GetTypeAndFormatDescription()})";

    /// <summary>
    ///   Gets the description.
    /// </summary>
    /// <returns></returns>
    public string GetTypeAndFormatDescription(bool addTime = true)
    {
      var stringBuilder = new StringBuilder(ValueFormat.DataType.Description());

      var shortDesc = ValueFormat.GetFormatDescription();
      if (shortDesc.Length > 0)
      {
        stringBuilder.Append(" (");
        stringBuilder.Append(shortDesc);
        stringBuilder.Append(')');
      }

      if (addTime && ValueFormat.DataType == DataTypeEnum.DateTime)
      {
        if (TimePart.Length > 0)
        {
          stringBuilder.Append(" + ");
          stringBuilder.Append(TimePart);
          if (TimePartFormat.Length > 0)
          {
            stringBuilder.Append(" (");
            stringBuilder.Append(TimePartFormat);
            stringBuilder.Append(')');
          }
        }

        if (TimeZonePart.Length > 0)
        {
          stringBuilder.Append(" - ");
          stringBuilder.Append(TimeZonePart);
        }
      }

      if (Ignore)
        stringBuilder.Append(" (Ignore)");

      return stringBuilder.ToString();
    }
  }
}
