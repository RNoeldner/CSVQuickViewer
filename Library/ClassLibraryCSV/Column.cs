﻿/*
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
using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   Column information like name, Type, Format etc.
  /// </summary>
  [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class Column : IColumn, INotifyPropertyChanged, IEquatable<Column>, ICloneable<Column>
#pragma warning restore CS0659
  {
    /// <summary>
    ///   The default time part format
    /// </summary>
    public const string cDefaultTimePartFormat = "HH:mm:ss";
    private const int c_PartDefault = 2;
    private const char c_PartSplitterDefault = ':';
    private const bool c_PartToEnd = true;
    private int m_ColumnOrdinal;
    private bool? m_Convert;
    private string m_DestinationName = string.Empty;
    private bool m_Ignore;
    private string m_Name;
    private int m_Part = c_PartDefault;
    private char m_PartSplitter = c_PartSplitterDefault;
    private bool m_PartToEnd = c_PartToEnd;
    private string m_TimePart = string.Empty;
    private string m_TimePartFormat = cDefaultTimePartFormat;
    private string m_TimeZonePart = string.Empty;

    public Column()
      : this(string.Empty)
    {
    }

    public Column(IColumn source, IValueFormat format)
    {
      m_ColumnOrdinal = source.ColumnOrdinal;
      m_Convert = source.Convert;
      m_DestinationName = source.DestinationName;
      m_Ignore = source.Ignore;
      m_Name = source.Name;
      m_Part = source.Part;
      m_PartSplitter = source.PartSplitter;
      m_PartToEnd = source.PartToEnd;
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
      ValueFormatMutable = new ValueFormatMutable(dataType);
    }

    public Column(string name, string dateFormat, string dateSeparator = ValueFormatExtension.cDateSeparatorDefault)
    {
      m_Name = name;
      ValueFormatMutable =
        new ValueFormatMutable(DataType.DateTime) { DateFormat = dateFormat, DateSeparator = dateSeparator };
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [UsedImplicitly]
    public bool ConvertSpecified => m_Convert.HasValue;

    /// <summary>
    ///   Gets or sets the type of the data.
    /// </summary>
    /// <value>The type of the data.</value>
    [XmlAttribute("Type")]
    [DefaultValue(DataType.String)]
    public virtual DataType DataType
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
    public virtual string DateFormat
    {
      get => ValueFormatMutable.DateFormat;
      set => ValueFormatMutable.DateFormat = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [UsedImplicitly]
    public bool DateFormatSpecified => ValueFormatMutable.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the date separator.
    /// </summary>
    /// <value>The date separator.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cDateSeparatorDefault)]
    public virtual string DateSeparator
    {
      [NotNull]
      get => ValueFormatMutable.DateSeparator;
      set => ValueFormatMutable.DateSeparator = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [UsedImplicitly]
    public bool DateSeparatorSpecified => ValueFormatMutable.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the decimal separator.
    /// </summary>
    /// <value>The decimal separator.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cDecimalSeparatorDefault)]
    public virtual string DecimalSeparator
    {
      [NotNull]
      get => ValueFormatMutable.DecimalSeparator;
      set => ValueFormatMutable.DecimalSeparator = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [UsedImplicitly]
    public bool DecimalSeparatorSpecified =>
      ValueFormatMutable.DataType == DataType.Double || ValueFormatMutable.DataType == DataType.Numeric;

    [UsedImplicitly]
    public bool DestinationNameSpecified =>
      !m_DestinationName.Equals(m_Name, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///   Gets or sets the representation for false.
    /// </summary>
    /// <value>The false.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cFalseDefault)]
#pragma warning disable CA1716 // Identifiers should not match keywords
    public virtual string False
#pragma warning restore CA1716
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
    [UsedImplicitly]
    public bool FalseSpecified => ValueFormatMutable.DataType == DataType.Boolean;

    /// <summary>
    ///   Gets or sets the group separator.
    /// </summary>
    /// <value>The group separator.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cGroupSeparatorDefault)]
    public virtual string GroupSeparator
    {
      get => ValueFormatMutable.GroupSeparator;
      set => ValueFormatMutable.GroupSeparator = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [UsedImplicitly]
    public bool GroupSeparatorSpecified =>
      ValueFormatMutable.DataType == DataType.Double || ValueFormatMutable.DataType == DataType.Numeric
                                                     || ValueFormatMutable.DataType == DataType.Integer;

    /// <summary>
    ///   Gets or sets the number format.
    /// </summary>
    /// <value>The number format.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cNumberFormatDefault)]
    public virtual string NumberFormat
    {
      get => ValueFormatMutable.NumberFormat;
      set => ValueFormatMutable.NumberFormat = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if number format is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [UsedImplicitly]
    public bool NumberFormatSpecified =>
      ValueFormatMutable.DataType == DataType.Double || ValueFormatMutable.DataType == DataType.Numeric;

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [UsedImplicitly]
    public bool PartSpecified => ValueFormatMutable.DataType == DataType.TextPart;

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [UsedImplicitly]
    public bool PartSplitterSpecified =>
      ValueFormatMutable.DataType == DataType.TextPart && !m_PartSplitter.Equals(c_PartSplitterDefault);

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [UsedImplicitly]
    public bool PartToEndSpecified => ValueFormatMutable.DataType == DataType.TextPart;

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [UsedImplicitly]
    public bool TimePartFormatSpecified => ValueFormatMutable.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [UsedImplicitly]
    public bool TimePartSpecified => ValueFormatMutable.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the time separator.
    /// </summary>
    /// <value>The time separator.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cTimeSeparatorDefault)]
    public virtual string TimeSeparator
    {
      get => ValueFormatMutable.TimeSeparator;
      set => ValueFormatMutable.TimeSeparator = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [UsedImplicitly]
    public bool TimeSeparatorSpecified => ValueFormatMutable.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the representation for true.
    /// </summary>
    /// <value>The true.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cTrueDefault)]
#pragma warning disable CA1716 // Identifiers should not match keywords
    public virtual string True
#pragma warning restore CA1716
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
    [UsedImplicitly]
    public bool TrueSpecified => ValueFormatMutable.DataType == DataType.Boolean;

    /// <summary>
    ///   Mimics to get or sets the value format.
    /// </summary>
    /// <value>The value format.</value>


    public ValueFormatMutable ValueFormatMutable { get; }

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public virtual Column Clone()
    {
      var other = new Column();
      CopyTo(other);
      return other;
    }

    /// <summary>
    ///   The Ordinal Position of the column
    /// </summary>
    [XmlIgnore]
    public virtual int ColumnOrdinal
    {
      get => m_ColumnOrdinal;
      set => m_ColumnOrdinal = value;
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="Column" /> is convert. Only used
    ///   to read a typed value as text
    /// </summary>
    /// <value><c>true</c> if the column should be convert; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool Convert
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
    public virtual string DestinationName
    {
      get => m_DestinationName;
      [CanBeNull]
      set
      {
        var newVal = value ?? string.Empty;
        if (m_DestinationName.Equals(newVal, StringComparison.Ordinal))
          return;
        m_DestinationName = newVal;
        NotifyPropertyChanged(nameof(DestinationName));
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether the column should be ignore reading a file
    /// </summary>
    /// <value><c>true</c> if [ignore read]; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool Ignore
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
    public virtual string Name
    {
      get => m_Name;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_Name.Equals(newVal, StringComparison.Ordinal))
          return;
        m_Name = newVal;
        NotifyPropertyChanged(nameof(Name));
      }
    }

    /// <summary>
    ///   Gets or sets the part for splitting.
    /// </summary>
    /// <value>The part starting with 1</value>
    [XmlAttribute]
    [DefaultValue(c_PartDefault)]
    public virtual int Part
    {
      get => m_Part;

      set
      {
        if (m_Part.Equals(value))
          return;
        m_Part = value;
        NotifyPropertyChanged(nameof(Part));
      }
    }

    /// <summary>
    ///   Gets or sets the splitter.
    /// </summary>
    /// <value>The splitter.</value>
    [DefaultValue(c_PartSplitterDefault)]
    public virtual char PartSplitter
    {
      get => m_PartSplitter;

      set
      {
        if (m_PartSplitter.Equals(value))
          return;
        m_PartSplitter = value;
        NotifyPropertyChanged(nameof(PartSplitter));
      }
    }

    /// <summary>
    ///   Gets or sets the part for splitting.
    /// </summary>
    /// <value>The part starting with 1</value>
    [XmlAttribute]
    [DefaultValue(c_PartToEnd)]
    public virtual bool PartToEnd
    {
      get => m_PartToEnd;

      set
      {
        if (m_PartToEnd.Equals(value))
          return;
        m_PartToEnd = value;
        NotifyPropertyChanged(nameof(PartToEnd));
      }
    }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string TimePart
    {
      get => m_TimePart;

      set
      {
        var newVal = value ?? string.Empty;
        if (m_TimePart.Equals(newVal, StringComparison.Ordinal))
          return;
        m_TimePart = newVal;
        NotifyPropertyChanged(nameof(TimePart));
      }
    }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [XmlAttribute]
    [DefaultValue(cDefaultTimePartFormat)]
    public virtual string TimePartFormat
    {
      get => m_TimePartFormat;
      set
      {
        var newVal = value ?? cDefaultTimePartFormat;
        if (m_TimePartFormat.Equals(newVal, StringComparison.Ordinal))
          return;
        m_TimePartFormat = newVal;
        NotifyPropertyChanged(nameof(TimePartFormat));
      }
    }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string TimeZonePart
    {
      get => m_TimeZonePart;

      set
      {
        var newVal = value ?? string.Empty;
        if (m_TimeZonePart.Equals(newVal, StringComparison.Ordinal))
          return;
        m_TimeZonePart = newVal;
        NotifyPropertyChanged(nameof(TimeZonePart));
      }
    }

    public IValueFormat ValueFormat => ValueFormatMutable;

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(Column other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;

      return ColumnOrdinal == other.ColumnOrdinal
             && False == other.False && NumberFormat == other.NumberFormat && True == other.True   && DateFormat == other.DateFormat
             && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
             && string.Equals(DestinationName, other.DestinationName, StringComparison.OrdinalIgnoreCase)
             && Ignore == other.Ignore && Part == other.Part && PartSplitter == other.PartSplitter
             && PartToEnd == other.PartToEnd
             && string.Equals(TimePart, other.TimePart, StringComparison.OrdinalIgnoreCase)
             && string.Equals(TimePartFormat, other.TimePartFormat, StringComparison.Ordinal)
             && string.Equals(TimeZonePart, other.TimeZonePart, StringComparison.OrdinalIgnoreCase)
             && Convert == other.Convert && ValueFormatMutable.Equals(other.ValueFormatMutable);
    }

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public virtual event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Copies to.
    /// </summary>
    /// <param name="other">The other.</param>
    public virtual void CopyTo(Column other)
    {
      if (other == null)
        return;
      ValueFormatMutable.CopyTo(other.ValueFormatMutable);

      // other.ValueFormatMutable = m_ValueFormatMutable;
      other.PartSplitter = m_PartSplitter;
      other.Part = m_Part;
      other.PartToEnd = m_PartToEnd;
      other.TimePartFormat = m_TimePartFormat;
      other.TimePart = m_TimePart;
      other.TimeZonePart = m_TimeZonePart;
      other.ColumnOrdinal = m_ColumnOrdinal;
      other.Name = m_Name;
      other.Ignore = m_Ignore;
      if (m_Convert.HasValue)
        other.Convert = m_Convert.Value;
    }

    /// <summary>
    ///   Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    ///   <see langword="true" /> if the specified object is equal to the current object; otherwise,
    ///   <see langword="false" />.
    /// </returns>
#pragma warning disable 659
    public override bool Equals(object obj) => Equals(obj as Column);

#pragma warning restore 659

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public virtual void NotifyPropertyChanged(string info) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

    /// <summary>
    ///   Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string" /> that represents this instance.</returns>
    public override string ToString() => $"{Name} ({this.GetTypeAndFormatDescription()})";

    /*
    /// <summary>
    ///   Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    ///   A hash code for this instance, suitable for use in hashing algorithms and data structures
    ///   like a hash table.
    /// </returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = m_ColumnOrdinal;
        hashCode = (hashCode * 397) ^ m_Convert.GetHashCode();
        hashCode = (hashCode * 397) ^ (int)m_DataType;
        hashCode = (hashCode * 397) ^ (m_DateFormat != null ? m_DateFormat.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (m_DateSeparator != null ? m_DateSeparator.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (m_DecimalSeparator != null ? m_DecimalSeparator.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ m_DecimalSeparatorChar.GetHashCode();
        hashCode = (hashCode * 397) ^ (m_DestinationName != null
                     ? StringComparer.OrdinalIgnoreCase.GetHashCode(m_DestinationName)
                     : 0);
        hashCode = (hashCode * 397) ^ (m_False != null ? m_False.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (m_GroupSeparator != null ? m_GroupSeparator.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ m_Ignore.GetHashCode();
        hashCode = (hashCode * 397) ^ (m_Name != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(m_Name) : 0);
        hashCode = (hashCode * 397) ^ (m_NumberFormat != null ? m_NumberFormat.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ m_Part;
        hashCode = (hashCode * 397) ^ m_PartSplitter.GetHashCode();
        hashCode = (hashCode * 397) ^ m_PartToEnd.GetHashCode();
        hashCode = (hashCode * 397) ^ m_Size;
        hashCode = (hashCode * 397) ^ (m_TimePart != null ? m_TimePart.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (m_TimePartFormat != null ? m_TimePartFormat.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (m_TimeSeparator != null ? m_TimeSeparator.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^
                   (m_TimeZonePart != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(m_TimeZonePart) : 0);
        hashCode = (hashCode * 397) ^ (m_True != null ? m_True.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ GroupSeparatorChar.GetHashCode();
        return hashCode;
      }
    }
    */
  }
}