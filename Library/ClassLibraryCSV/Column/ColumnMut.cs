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
using System.Xml.Serialization;

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace CsvTools
{
  /// <inheritdoc cref="IColumn" />
  /// <summary>
  ///   Column information like name, Type, Format etc.
  /// </summary>
  [Serializable]
  public sealed class ColumnMut : NotifyPropertyChangedBase, IColumn
  {
    private bool m_Convert;
    private string m_DestinationName;
    private bool m_Ignore;
    private string m_Name;
    private string m_TimePart;
    private string m_TimePartFormat;
    private string m_TimeZonePart;
    private int m_ColumnOrdinal;

    [Obsolete("Only needed for XML Serialization")]
    public ColumnMut()
      : this(string.Empty)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnMut"/> class.
    /// </summary>
    /// <param name="name">The column name.</param>
    /// <param name="valueFormat">The format of the column.</param>
    /// <param name="columnOrdinal">The column ordinal.</param>
    /// <param name="convert">If Conversion is necessary, usually yes if teh format is non string.</param>
    /// <param name="destinationName">Name of the destination.</param>
    /// <param name="ignore">if set to <c>true</c> the column will be ignored.</param>
    /// <param name="timePart">The time part for date time information provided in two columns.</param>
    /// <param name="timePartFormat">The time part format for date time information provided in two columns</param>
    /// <param name="timeZonePart">The time zone part for date time information provided in multiple columns</param>
    /// <exception cref="System.ArgumentNullException">name</exception>
    [JsonConstructor]
    public ColumnMut(
      in string name,
      in IValueFormat? valueFormat = null,
      int columnOrdinal = -1,
      bool? convert = null,
      in string destinationName = "",
      bool ignore = false,
      in string timePart = "",
      in string timePartFormat = Column.cDefaultTimePartFormat,
      in string timeZonePart = "")
    {
      m_Name = name ?? throw new ArgumentNullException(nameof(name));
      ValueFormatMut = valueFormat is null ? new ValueFormatMut(DataTypeEnum.String) : valueFormat.ToMutable();
      ValueFormatMut.PropertyChanged += (sender, args) =>
      {
        NotifyPropertyChanged(nameof(ValueFormatMut));

        // If the value types changes to to something else but string, assume we need to convert
        if (args.PropertyName.Equals(nameof(IValueFormat.DataType)) && sender is IValueFormat valueFormat)
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
    ///   Gets or sets the type of the data.
    /// </summary>
    /// <value>The type of the data.</value>
    [XmlAttribute("Type")]
    [JsonIgnore]
    //[Obsolete("Use ValueFormat instead")]
    [DefaultValue(DataTypeEnum.String)]
    [JsonProperty]
    public DataTypeEnum DataType
    {
      get => ValueFormatMut.DataType;
      set => ValueFormatMut.DataType = value;
    }

    /// <summary>
    ///   Gets or sets the date format.
    /// </summary>
    /// <value>The date format.</value>
    [XmlAttribute]
    [JsonIgnore]
    //[Obsolete("Use ValueFormat instead")]
    [DefaultValue(ValueFormatExtension.cDateFormatDefault)]
    public string DateFormat
    {
      get => ValueFormatMut.DateFormat;
      set => ValueFormatMut.DateFormat = value;
    }

    /// <summary>
    ///   Gets or sets the date separator.
    /// </summary>
    /// <value>The date separator.</value>
    [XmlAttribute]
    [JsonIgnore]
    //[Obsolete("Use ValueFormat instead")]
    [DefaultValue(ValueFormatExtension.cDateSeparatorDefault)]
    public string DateSeparator
    {
      get => ValueFormatMut.DateSeparator;
      set => ValueFormatMut.DateSeparator = value;
    }

    /// <summary>
    ///   Gets or sets the decimal separator.
    /// </summary>
    /// <value>The decimal separator.</value>
    [XmlAttribute]
    [JsonIgnore]
    //[Obsolete("Use ValueFormat instead")]
    [DefaultValue(ValueFormatExtension.cDecimalSeparatorDefault)]
    public string DecimalSeparator
    {
      get => ValueFormatMut.DecimalSeparator;
      set => ValueFormatMut.DecimalSeparator = value;
    }


    /// <summary>
    ///   Gets or sets the representation for false.
    /// </summary>
    /// <value>The false.</value>
    [XmlAttribute]
    [JsonIgnore]
    //[Obsolete("Use ValueFormat instead")]
    [DefaultValue(ValueFormatExtension.cFalseDefault)]
    public string False
    {
      // Identifiers should not match keywords
      get => ValueFormatMut.False;
      set => ValueFormatMut.False = value;
    }


    /// <summary>
    ///   Gets or sets the group separator.
    /// </summary>
    /// <value>The group separator.</value>
    [XmlAttribute]
    [JsonIgnore]
    //[Obsolete("Use ValueFormat instead")]
    [DefaultValue(ValueFormatExtension.cGroupSeparatorDefault)]
    public string GroupSeparator
    {
      get => ValueFormatMut.GroupSeparator;
      set => ValueFormatMut.GroupSeparator = value;
    }

    /// <summary>
    ///   Gets or sets the number format.
    /// </summary>
    /// <value>The number format.</value>
    [XmlAttribute]
    [JsonIgnore]
    //[Obsolete("Use ValueFormat instead")]
    [DefaultValue(ValueFormatExtension.cNumberFormatDefault)]
    public string NumberFormat
    {
      get => ValueFormatMut.NumberFormat;
      set => ValueFormatMut.NumberFormat = value;
    }

    /// <summary>
    ///   Gets or sets the part for splitting.
    /// </summary>
    /// <value>The part starting with 1</value>
    [XmlAttribute]
    [JsonIgnore]
    //[Obsolete("Use ValueFormat instead")]
    [DefaultValue(ValueFormatExtension.cPartDefault)]
    public int Part
    {
      get => ValueFormatMut.Part;
      set => ValueFormatMut.Part = value;
    }

    /// <summary>
    ///   Gets or sets the splitter.
    /// </summary>
    /// <value>The splitter.</value>
    [XmlElement]
    [JsonIgnore]
    [DefaultValue(ValueFormatExtension.cPartSplitterDefault)]
    //[Obsolete("Use ValueFormat instead")]
    public string PartSplitter
    {
      get => ValueFormatMut.PartSplitter;
      set => ValueFormatMut.PartSplitter = value;
    }


    /// <summary>
    ///   Gets or sets the part for splitting.
    /// </summary>
    /// <value>The part starting with 1</value>
    [XmlAttribute]
    [JsonIgnore]
    //[Obsolete("Use ValueFormat instead")]
    [DefaultValue(ValueFormatExtension.cPartToEndDefault)]
    public bool PartToEnd
    {
      get => ValueFormatMut.PartToEnd;
      set => ValueFormatMut.PartToEnd = value;
    }

    /// <summary>
    ///   Gets or sets the time separator.
    /// </summary>
    /// <value>The time separator.</value>
    [XmlAttribute]
    [JsonIgnore]
    //[Obsolete("Use ValueFormat instead")]
    [DefaultValue(ValueFormatExtension.cTimeSeparatorDefault)]
    public string TimeSeparator
    {
      get => ValueFormatMut.TimeSeparator;
      set => ValueFormatMut.TimeSeparator = value;
    }

    /// <summary>
    ///   Gets or sets the representation for true.
    /// </summary>
    /// <value>The true.</value>
    [XmlAttribute]
    [JsonIgnore]
    //[Obsolete("Use ValueFormat instead")]
    [DefaultValue(ValueFormatExtension.cTrueDefault)]
    public string True
    {
      // Identifiers should not match keywords
      get => ValueFormatMut.True;
      set => ValueFormatMut.True = value;
    }

    [JsonIgnore] public ValueFormatMut ValueFormatMut { get; }

    /// <summary>
    ///   The Ordinal Position of the column
    /// </summary>
    [XmlIgnore]
    public int ColumnOrdinal
    {
      get => m_ColumnOrdinal;
      set => SetField(ref m_ColumnOrdinal, value);
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="ColumnMut" /> is convert. Only used
    ///   to read a typed value as text
    /// </summary>
    /// <value><c>true</c> if the column should be convert; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(true)]
    public bool Convert
    {
      get => m_Convert;
      set => SetField(ref m_Convert, value);
    }

    /// <summary>
    ///   Gets or sets the name in a destination. This is only used for writing
    /// </summary>
    /// <value>The name of the column in the destination.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public string DestinationName
    {
      get => m_DestinationName;
      set => SetField(ref m_DestinationName, value, StringComparison.Ordinal);
    }

    [XmlIgnore]
    [JsonIgnore]
    public bool DestinationNameSpecified => !m_DestinationName.Equals(m_Name, StringComparison.OrdinalIgnoreCase);


    /// <summary>
    ///   Gets or sets a value indicating whether the column should be ignore reading a file
    /// </summary>
    /// <value><c>true</c> if [ignore read]; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public bool Ignore
    {
      get => m_Ignore;
      set => SetField(ref m_Ignore, value);
    }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [XmlAttribute("Column")]
    public string Name
    {
      get => m_Name;
      set => SetField(ref m_Name, value, StringComparison.Ordinal, true);
    }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public string TimePart
    {
      get => m_TimePart;
      set => SetField(ref m_TimePart, value, StringComparison.Ordinal);
    }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [XmlAttribute]
    [DefaultValue(Column.cDefaultTimePartFormat)]
    public string TimePartFormat
    {
      get => m_TimePartFormat;
      set => SetField(ref m_TimePartFormat, value, StringComparison.Ordinal);
    }

    [JsonIgnore]
    public bool TimePartFormatSpecified => ValueFormatMut.DataType == DataTypeEnum.DateTime &&
                                           m_TimePartFormat != Column.cDefaultTimePartFormat;

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public string TimeZonePart
    {
      get => m_TimeZonePart;
      set => SetField(ref m_TimeZonePart, value, StringComparison.Ordinal);
    }

    /// <summary>
    ///   Mimics to get or sets the value format.
    /// </summary>
    /// <value>The value format.</value>

    public IValueFormat ValueFormat => ValueFormatMut;

    /// <inheritdoc />
    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(IColumn? other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;

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
                                                  && ValueFormatMut.Equals(other.ValueFormat);
    }

    /// <summary>
    ///   Copies to.
    /// </summary>
    /// <param name="other">The other.</param>
    public void CopyTo(ColumnMut other)
    {
      other.ValueFormatMut.CopyFrom(ValueFormatMut);
      other.TimePartFormat = m_TimePartFormat;
      other.TimePart = m_TimePart;
      other.TimeZonePart = m_TimeZonePart;
      other.ColumnOrdinal = ColumnOrdinal;
      other.Name = m_Name;
      other.Ignore = m_Ignore;
      other.Convert = m_Convert;
    }

    /// <summary>
    ///   Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string" /> that represents this instance.</returns>
    public override string ToString() => $"{Name} ({this.GetTypeAndFormatDescription()})";

    /// <summary>
    /// Identifier in collections, similar to a hashcode based on a  properties that should be unique in a collection
    /// </summary>
    /// <remarks>
    /// In case a required property is not set, this should raise an error
    /// </remarks>
    [JsonIgnore]
    public int CollectionIdentifier => Name.IdentifierHash();
  }
}