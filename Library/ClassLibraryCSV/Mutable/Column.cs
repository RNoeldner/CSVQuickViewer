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
using System.ComponentModel;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <inheritdoc cref="IColumn" />
  /// <summary>
  ///   Column information like name, Type, Format etc.
  /// </summary>
  [Serializable]
  public sealed class Column : IColumn, INotifyPropertyChanged
  {
    private bool? m_Convert;
    private string m_DestinationName = string.Empty;
    private bool m_Ignore;
    private string m_Name;
    private string m_TimePart = string.Empty;
    private string m_TimePartFormat = ImmutableColumn.cDefaultTimePartFormat;
    private string m_TimeZonePart = string.Empty;

    public Column()
      : this(string.Empty)
    {
    }

    public Column(IColumn source)
    {
      ColumnOrdinal = source.ColumnOrdinal;
      m_Convert = source.Convert;
      m_DestinationName = source.DestinationName;
      m_Ignore = source.Ignore;
      m_Name = source.Name;
      m_TimePart = source.TimePart;
      m_TimePartFormat = source.TimePartFormat;
      m_TimeZonePart = source.TimeZonePart;

      ValueFormatMutable = new ValueFormatMutable(source.ValueFormat);
    }

    public Column(IColumn source, IValueFormat format)
    {
      ColumnOrdinal = source.ColumnOrdinal;
      m_Convert = source.Convert;
      m_DestinationName = source.DestinationName;
      m_Ignore = source.Ignore;
      m_Name = source.Name;
      m_TimePart = source.TimePart;
      m_TimePartFormat = source.TimePartFormat;
      m_TimeZonePart = source.TimeZonePart;
      ValueFormatMutable = new ValueFormatMutable(format);
    }

    public Column(string name, IValueFormat valueFormat)
    {
      m_Name = name;
      ValueFormatMutable = new ValueFormatMutable(valueFormat);
    }

    public Column(string name, DataType dataType = DataType.String)
    {
      m_Name = name;
      ValueFormatMutable = new ValueFormatMutable { DataType = dataType };
    }

    public Column(string name, string dateFormat, string dateSeparator = ValueFormatExtension.cDateSeparatorDefault)
    {
      m_Name = name;
      ValueFormatMutable = new ValueFormatMutable { DataType = DataType.DateTime, DateFormat = dateFormat, DateSeparator = dateSeparator };
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    public bool ConvertSpecified => m_Convert.HasValue;

    /// <summary>
    ///   Gets or sets the type of the data.
    /// </summary>
    /// <value>The type of the data.</value>
    [XmlAttribute("Type")]
    [DefaultValue(DataType.String)]
    public DataType DataType
    {
      get => ValueFormatMutable.DataType;
      set => ValueFormatMutable.DataType = value;
    }

    /// <summary>
    ///   Gets or sets the date format.
    /// </summary>
    /// <value>The date format.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cDateFormatDefault)]
    public string DateFormat
    {
      get => ValueFormatMutable.DateFormat;
      set => ValueFormatMutable.DateFormat = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    public bool DateFormatSpecified => ValueFormatMutable.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the date separator.
    /// </summary>
    /// <value>The date separator.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cDateSeparatorDefault)]
    public string DateSeparator
    {
      get => ValueFormatMutable.DateSeparator;
      set => ValueFormatMutable.DateSeparator = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public bool DateSeparatorSpecified => ValueFormatMutable.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the decimal separator.
    /// </summary>
    /// <value>The decimal separator.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cDecimalSeparatorDefault)]
    public string DecimalSeparator
    {
      get => ValueFormatMutable.DecimalSeparator;
      set => ValueFormatMutable.DecimalSeparator = value;
    }

    public bool DestinationNameSpecified => !m_DestinationName.Equals(m_Name, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///   Gets or sets the representation for false.
    /// </summary>
    /// <value>The false.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cFalseDefault)]
    public string False
    {
      // Identifiers should not match keywords
      get => ValueFormatMutable.False;
      set => ValueFormatMutable.False = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    public bool FalseSpecified => ValueFormatMutable.DataType == DataType.Boolean;

    /// <summary>
    ///   Gets or sets the group separator.
    /// </summary>
    /// <value>The group separator.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cGroupSeparatorDefault)]
    public string GroupSeparator
    {
      get => ValueFormatMutable.GroupSeparator;
      set => ValueFormatMutable.GroupSeparator = value;
    }

    /// <summary>
    ///   Gets or sets the number format.
    /// </summary>
    /// <value>The number format.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cNumberFormatDefault)]
    public string NumberFormat
    {
      get => ValueFormatMutable.NumberFormat;
      set => ValueFormatMutable.NumberFormat = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if number format is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    public bool NumberFormatSpecified =>
      ValueFormatMutable.DataType == DataType.Double || ValueFormatMutable.DataType == DataType.Numeric;

    /// <summary>
    ///   Gets or sets the part for splitting.
    /// </summary>
    /// <value>The part starting with 1</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cPartDefault)]
    public int Part
    {
      get => ValueFormatMutable.Part;
      set => ValueFormatMutable.Part = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    public bool PartSpecified => ValueFormatMutable.DataType == DataType.TextPart;

    /// <summary>
    ///   Gets or sets the splitter.
    /// </summary>
    /// <value>The splitter.</value>
    [DefaultValue(ValueFormatExtension.cPartSplitterDefault)]
    public string PartSplitter
    {
      get => ValueFormatMutable.PartSplitter;
      set => ValueFormatMutable.PartSplitter = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>

    public bool PartSplitterSpecified =>
      ValueFormatMutable.DataType == DataType.TextPart
      && !ValueFormatMutable.PartSplitter.Equals(ValueFormatExtension.cPartSplitterDefault);

    /// <summary>
    ///   Gets or sets the part for splitting.
    /// </summary>
    /// <value>The part starting with 1</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cPartToEndDefault)]
    public bool PartToEnd
    {
      get => ValueFormatMutable.PartToEnd;
      set => ValueFormatMutable.PartToEnd = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>

    public bool PartToEndSpecified => ValueFormatMutable.DataType == DataType.TextPart;

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>

    public bool TimePartFormatSpecified => ValueFormatMutable.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>

    public bool TimePartSpecified => ValueFormatMutable.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the time separator.
    /// </summary>
    /// <value>The time separator.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cTimeSeparatorDefault)]
    public string TimeSeparator
    {
      get => ValueFormatMutable.TimeSeparator;
      set => ValueFormatMutable.TimeSeparator = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>

    public bool TimeSeparatorSpecified => ValueFormatMutable.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the representation for true.
    /// </summary>
    /// <value>The true.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cTrueDefault)]
    public string True
    {
      // Identifiers should not match keywords
      get => ValueFormatMutable.True;

      set => ValueFormatMutable.True = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>

    public bool TrueSpecified => ValueFormatMutable.DataType == DataType.Boolean;

    public ValueFormatMutable ValueFormatMutable { get; }

    /// <summary>
    ///   The Ordinal Position of the column
    /// </summary>
    [XmlIgnore]
    public int ColumnOrdinal
    {
      get;
      set;
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="Column" /> is convert. Only used
    ///   to read a typed value as text
    /// </summary>
    /// <value><c>true</c> if the column should be convert; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public bool Convert
    {
      get => m_Convert ?? ValueFormatMutable.DataType != DataType.String;
      set
      {
        if (m_Convert.HasValue && m_Convert.Equals(value))
          return;
        m_Convert = value;
        NotifyPropertyChanged(nameof(Convert));
      }
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
      set
      {
        if (m_DestinationName.Equals(value, StringComparison.Ordinal))
          return;
        m_DestinationName = value;
        NotifyPropertyChanged(nameof(DestinationName));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether the column should be ignore reading a file
    /// </summary>
    /// <value><c>true</c> if [ignore read]; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public bool Ignore
    {
      get => m_Ignore;

      set
      {
        if (m_Ignore.Equals(value))
          return;
        m_Ignore = value;
        NotifyPropertyChanged(nameof(Ignore));
      }
    }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [XmlAttribute("Column")]
    public string Name
    {
      get => m_Name;
      set
      {
        if (m_Name.Equals(value, StringComparison.Ordinal))
          return;
        m_Name = value;
        NotifyPropertyChanged(nameof(Name));
      }
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

      set
      {
        if (m_TimePart.Equals(value, StringComparison.Ordinal))
          return;
        m_TimePart = value;
        NotifyPropertyChanged(nameof(TimePart));
      }
    }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [XmlAttribute]
    [DefaultValue(ImmutableColumn.cDefaultTimePartFormat)]
    public string TimePartFormat
    {
      get => m_TimePartFormat;
      set
      {
        if (m_TimePartFormat.Equals(value, StringComparison.Ordinal))
          return;
        m_TimePartFormat = value;
        NotifyPropertyChanged(nameof(TimePartFormat));
      }
    }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public string TimeZonePart
    {
      get => m_TimeZonePart;

      set
      {
        if (m_TimeZonePart.Equals(value, StringComparison.Ordinal))
          return;
        m_TimeZonePart = value;
        NotifyPropertyChanged(nameof(TimeZonePart));
      }
    }

    /// <summary>
    ///   Mimics to get or sets the value format.
    /// </summary>
    /// <value>The value format.</value>

    public IValueFormat ValueFormat => ValueFormatMutable;

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
                                                  && string.Equals(
                                                    DestinationName,
                                                    other.DestinationName,
                                                    StringComparison.OrdinalIgnoreCase)
                                                  && Ignore == other.Ignore
                                                  && string.Equals(
                                                    TimePart,
                                                    other.TimePart,
                                                    StringComparison.OrdinalIgnoreCase)
                                                  && string.Equals(
                                                    TimePartFormat,
                                                    other.TimePartFormat,
                                                    StringComparison.Ordinal)
                                                  && string.Equals(
                                                    TimeZonePart,
                                                    other.TimeZonePart,
                                                    StringComparison.OrdinalIgnoreCase)
                                                  && Convert == other.Convert
                                                  && ValueFormatMutable.ValueFormatEqual(other.ValueFormat);
    }



 

    object ICloneable.Clone() => new Column(this);

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public Column Clone()
    {
      var other = new Column();
      CopyTo(other);
      return other;
    }

    /// <summary>
    ///   Copies to.
    /// </summary>
    /// <param name="other">The other.</param>
    public void CopyTo(Column other)
    {
      other.ValueFormatMutable.CopyFrom(ValueFormatMutable);

      other.TimePartFormat = m_TimePartFormat;
      other.TimePart = m_TimePart;
      other.TimeZonePart = m_TimeZonePart;
      other.ColumnOrdinal = ColumnOrdinal;
      other.Name = m_Name;
      other.Ignore = m_Ignore;

      if (m_Convert.HasValue)
        other.Convert = m_Convert.Value;
    }

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public void NotifyPropertyChanged(string info) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

    /// <summary>
    ///   Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string" /> that represents this instance.</returns>
    public override string ToString() => $"{Name} ({this.GetTypeAndFormatDescription()})";
  }
}