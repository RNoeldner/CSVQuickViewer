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

using Newtonsoft.Json;
using System;
using System.ComponentModel;

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace CsvTools
{
  /// <summary>
  ///   This is a helper class to edit and to serialize into XML
  /// </summary>
  [Serializable]
  public sealed class ColumnMut : ObservableObject, IEquatable<ColumnMut>
  {
    private int m_ColumnOrdinal;
    private bool m_Convert;
    private string m_DestinationName;
    private bool m_Ignore;
    private string m_Name;
    private string m_TimePart;
    private string m_TimePartFormat;
    private string m_TimeZonePart;
    private ValueFormatMut m_ValueFormatMut;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnMut"/> class.
    /// </summary>
    /// <param name="source">The source.</param>
    public ColumnMut(Column source) : this(source.Name, source.ValueFormat, source.ColumnOrdinal, source.Ignore,
      source.Convert, source.DestinationName, source.TimePart, source.TimePartFormat, source.TimeZonePart)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnMut"/> class.
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
    [JsonConstructor]
    public ColumnMut(in string name,
      in ValueFormat? valueFormat = null,
      int columnOrdinal = -1,
      bool ignore = false,
      bool? convert = null,
      in string destinationName = "",
      in string timePart = "",
      in string timePartFormat = Column.cDefaultTimePartFormat,
      in string timeZonePart = "")
    {
      m_Name = name ?? throw new ArgumentNullException(nameof(name));
      m_ValueFormatMut = new ValueFormatMut(valueFormat ?? ValueFormat.Empty);
      m_ValueFormatMut.PropertyChanged += (sender, args) =>
      {
        NotifyPropertyChanged(nameof(ValueFormatMut));
        if (args.PropertyName is null)
          return;
        // If the value types changes to something else but string, assume we need to convert
        if (args.PropertyName.Equals(nameof(ValueFormat.DataType)) && sender is ValueFormatMut valueFormat)
          Convert = valueFormat.DataType != DataTypeEnum.String;
      };
      m_ColumnOrdinal = columnOrdinal;

      m_DestinationName = destinationName;
      m_Ignore = ignore;
      m_TimePart = timePart ?? string.Empty;
      m_TimePartFormat = timePartFormat ?? Column.cDefaultTimePartFormat;
      m_TimeZonePart = timeZonePart ?? string.Empty;
      m_Convert = convert ?? ValueFormat.DataType != DataTypeEnum.String;
    }

    /// <summary>
    /// Identifier in collections, similar to a hashcode based on a  properties that should be unique in a collection
    /// </summary>
    /// <remarks>
    /// In case a required property is not set, this should raise an error
    /// </remarks>
    [JsonIgnore]
    public int CollectionIdentifier => Name.IdentifierHash();

    /// <summary>
    ///   The Ordinal Position of the column
    /// </summary>
    [JsonIgnore]
    public int ColumnOrdinal
    {
      get => m_ColumnOrdinal;
      set => SetProperty(ref m_ColumnOrdinal, value);
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="ColumnMut" /> is convert. Only used
    ///   to read a typed value as text
    /// </summary>
    /// <value><c>true</c> if the column should be converted; otherwise, <c>false</c>.</value>
    [DefaultValue(true)]
    public bool Convert
    {
      get => m_Convert;
      set => SetProperty(ref m_Convert, value);
    }

    /// <summary>
    ///   Gets or sets the name in a destination. This is only used for writing
    /// </summary>
    /// <value>The name of the column in the destination.</value>
    [DefaultValue("")]
    public string DestinationName
    {
      get => m_DestinationName;
      set => SetProperty(ref m_DestinationName, value);
    }

    /// <summary>
    ///   Gets or sets a value indicating whether the column should be ignored reading a file
    /// </summary>
    /// <value><c>true</c> if [ignore read]; otherwise, <c>false</c>.</value>
    [DefaultValue(false)]
    public bool Ignore
    {
      get => m_Ignore;
      set => SetProperty(ref m_Ignore, value);
    }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name
    {
      get => m_Name;
      set => SetProperty(ref m_Name, value);
    }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [DefaultValue("")]
    public string TimePart
    {
      get => m_TimePart;
      set => SetProperty(ref m_TimePart, value);
    }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [DefaultValue(Column.cDefaultTimePartFormat)]
    public string TimePartFormat
    {
      get => m_TimePartFormat;
      set => SetProperty(ref m_TimePartFormat, value);
    }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [DefaultValue("")]
    public string TimeZonePart
    {
      get => m_TimeZonePart;
      set => SetProperty(ref m_TimeZonePart, value);
    }

    /// <summary>
    ///   Mimics to get or sets the value format.
    /// </summary>
    /// <value>The value format.</value>
    public ValueFormat ValueFormat => m_ValueFormatMut.ToImmutable();

    /// <summary>
    ///   Mutable Value format.
    /// </summary>
    [JsonIgnore]
    public ValueFormatMut ValueFormatMut
    {
      get => m_ValueFormatMut;
      set => SetProperty(ref m_ValueFormatMut, value);
    }

    /// <summary>
    ///   Copies to.
    /// </summary>
    /// <param name="other">The other.</param>
    public void CopyTo(ColumnMut other)
    {
      other.ValueFormatMut.CopyFrom(ValueFormat);
      other.TimePartFormat = m_TimePartFormat;
      other.TimePart = m_TimePart;
      other.TimeZonePart = m_TimeZonePart;
      other.ColumnOrdinal = ColumnOrdinal;
      other.Name = m_Name;
      other.Ignore = m_Ignore;
      other.Convert = m_Convert;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(ColumnMut? other)
    {
      if (other is null)
        return false;      

      return ColumnOrdinal == other.ColumnOrdinal && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
                                                  && string.Equals(DestinationName, other.DestinationName,
                                                    StringComparison.OrdinalIgnoreCase)
                                                  && Ignore == other.Ignore
                                                  && string.Equals(TimePart, other.TimePart,
                                                    StringComparison.OrdinalIgnoreCase)
                                                  && string.Equals(TimePartFormat, other.TimePartFormat,
                                                    StringComparison.Ordinal)
                                                  && string.Equals(TimeZonePart, other.TimeZonePart,
                                                    StringComparison.OrdinalIgnoreCase)
                                                  && Convert == other.Convert
                                                  && m_ValueFormatMut.Equals(new ValueFormatMut(other.ValueFormat));
    }
    /// <summary>
    /// Returns  an immutable column 
    /// </summary>
    /// <returns>and immutable column</returns>
    public Column ToImmutableColumn() =>
      new Column(
        Name,
        ValueFormat,
        ColumnOrdinal,
        Ignore,
        Convert,
        DestinationName,
        TimePart, TimePartFormat, TimeZonePart);

    /// <summary>
    ///   Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string" /> that represents this instance.</returns>
    public override string ToString() => $"{Name} ({this.ToImmutableColumn().GetTypeAndFormatDescription()})";
  }
}