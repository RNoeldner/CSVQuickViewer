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
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   Column information like name, Type, Format etc.
  /// </summary>
  [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class Column : INotifyPropertyChanged, IEquatable<Column>, ICloneable<Column>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    /// <summary>
    ///   The default time part format
    /// </summary>
    public const string cDefaultTimePartFormat = "HH:mm:ss";

    private const int c_PartDefault = 2;
    private const char c_PartSplitterDefault = ':';
    private const bool c_PartToEnd = true;
    private readonly ValueFormat m_ValueFormat = new ValueFormat();
    private int m_ColumnOrdinal;
    private bool? m_Convert;
    private string m_DestinationName = string.Empty;
    private bool m_Ignore;
    private string m_Name;
    private int m_Part = c_PartDefault;
    private char m_PartSplitter = c_PartSplitterDefault;
    private bool m_PartToEnd = c_PartToEnd;
    private int m_Size;
    private string m_TimePart = string.Empty;
    private string m_TimePartFormat = cDefaultTimePartFormat;
    private string m_TimeZonePart = string.Empty;

    public Column() : this(string.Empty)
    {
    }

    public Column(string name, DataType dataType = DataType.String)
    {
      m_Name = name;
      m_ValueFormat.DataType = dataType;
    }

    public Column(string name, string dateFormat, string dateSeparator = ValueFormat.cDateSeparatorDefault) : this(name, DataType.DateTime)
    {
      m_ValueFormat.DateFormat = dateFormat;
      m_ValueFormat.DateSeparator = dateSeparator;
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
    ///   Gets or sets a value indicating whether this <see cref="Column" /> is convert. Only used
    ///   to read a typed value as text 
    /// </summary>
    /// <value><c>true</c> if the column should be convert; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool Convert
    {
      get => m_Convert ?? m_ValueFormat.DataType != DataType.String;
      set
      {
        if (m_Convert.HasValue && m_Convert.Equals(value))
          return;
        m_Convert = value;
        NotifyPropertyChanged(nameof(Convert));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool ConvertSpecified => m_Convert.HasValue;

    /// <summary>
    ///   Gets or sets the type of the data.
    /// </summary>
    /// <value>The type of the data.</value>
    [XmlAttribute("Type")]
    [DefaultValue(DataType.String)]
    [Obsolete("Please use ValueFormat.DataType")]
    public virtual DataType DataType
    {
      get => m_ValueFormat.DataType;

      set => m_ValueFormat.DataType = value;
    }

    /// <summary>
    ///   Gets or sets the date format.
    /// </summary>
    /// <value>The date format.</value>
    [XmlAttribute]
    [DefaultValue(CsvTools.ValueFormat.cDateFormatDefault)]
    [Obsolete("Please use ValueFormat.DateFormat")]
    public virtual string DateFormat
    {
      get => m_ValueFormat.DateFormat;

      set => m_ValueFormat.DateFormat = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool DateFormatSpecified => m_ValueFormat.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the date separator.
    /// </summary>
    /// <value>The date separator.</value>
    [XmlAttribute]
    [DefaultValue(CsvTools.ValueFormat.cDateSeparatorDefault)]
    [Obsolete("Please use ValueFormat.DateSeparator")]
    public virtual string DateSeparator
    {
      get => m_ValueFormat.DateSeparator;
      set => m_ValueFormat.DateSeparator = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool DateSeparatorSpecified => m_ValueFormat.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the decimal separator.
    /// </summary>
    /// <value>The decimal separator.</value>
    [XmlAttribute]
    [DefaultValue(CsvTools.ValueFormat.cDecimalSeparatorDefault)]
    [Obsolete("Please use ValueFormat.DecimalSeparator")]
    public virtual string DecimalSeparator
    {
      get => m_ValueFormat.DecimalSeparator;
      set => m_ValueFormat.DecimalSeparator = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool DecimalSeparatorSpecified => m_ValueFormat.DataType == DataType.Double || m_ValueFormat.DataType == DataType.Numeric;

    /// <summary>
    ///   Gets or sets the name in a destination. This is only used for writing
    /// </summary>
    /// <value>The name of the column in the destination.</value>
    [XmlAttribute]
    public virtual string DestinationName
    {
      get => m_DestinationName;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_DestinationName.Equals(newVal, StringComparison.Ordinal))
          return;
        m_DestinationName = newVal;
        NotifyPropertyChanged(nameof(DestinationName));
      }
    }

    [XmlIgnore]
    public virtual bool DestinationNameSpecified =>
      !m_DestinationName.Equals(m_Name, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///   Gets or sets the representation for false.
    /// </summary>
    /// <value>The false.</value>
    [XmlAttribute]
    [DefaultValue(CsvTools.ValueFormat.cFalseDefault)]
    [Obsolete("Please use ValueFormat.False")]
#pragma warning disable CA1716 // Identifiers should not match keywords
    public virtual string False
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
      get => m_ValueFormat.False;
      set => m_ValueFormat.False = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool FalseSpecified => m_ValueFormat.DataType == DataType.Boolean;

    /// <summary>
    ///   Gets or sets the group separator.
    /// </summary>
    /// <value>The group separator.</value>
    [XmlAttribute]
    [DefaultValue(CsvTools.ValueFormat.cGroupSeparatorDefault)]
    [Obsolete("Please use ValueFormat.GroupSeparator")]
    public virtual string GroupSeparator
    {
      get => m_ValueFormat.GroupSeparator;
      set => m_ValueFormat.GroupSeparator = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool GroupSeparatorSpecified =>
      m_ValueFormat.DataType == DataType.Double || m_ValueFormat.DataType == DataType.Numeric || m_ValueFormat.DataType == DataType.Integer;

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
    ///   Gets or sets the number format.
    /// </summary>
    /// <value>The number format.</value>
    [XmlAttribute]
    [DefaultValue(CsvTools.ValueFormat.cNumberFormatDefault)]
    [Obsolete("Please use ValueFormat.NumberFormat")]
    public virtual string NumberFormat
    {
      get => m_ValueFormat.NumberFormat;
      set => m_ValueFormat.NumberFormat = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if number format is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool NumberFormatSpecified => m_ValueFormat.DataType == DataType.Double || m_ValueFormat.DataType == DataType.Numeric;

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
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool PartSpecified => m_ValueFormat.DataType == DataType.TextPart;

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
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool PartSplitterSpecified =>
      m_ValueFormat.DataType == DataType.TextPart && !m_PartSplitter.Equals(c_PartSplitterDefault);

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
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool PartToEndSpecified => m_ValueFormat.DataType == DataType.TextPart;

    /// <summary>
    ///   Gets or sets the number consecutive empty rows that should finish a read
    /// </summary>
    /// <value>The consecutive empty rows.</value>
    [XmlIgnore]
    public virtual int Size
    {
      get => m_Size;
      set => m_Size = value;
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
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool TimePartFormatSpecified => m_ValueFormat.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool TimePartSpecified => m_ValueFormat.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the time separator.
    /// </summary>
    /// <value>The time separator.</value>
    [XmlAttribute]
    [DefaultValue(CsvTools.ValueFormat.cTimeSeparatorDefault)]
    public virtual string TimeSeparator
    {
      get => m_ValueFormat.TimeSeparator;
      set => m_ValueFormat.TimeSeparator = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool TimeSeparatorSpecified => m_ValueFormat.DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the representation for true.
    /// </summary>
    /// <value>The true.</value>
    [XmlAttribute]
    [DefaultValue(CsvTools.ValueFormat.cTrueDefault)]
#pragma warning disable CA1716 // Identifiers should not match keywords
    public virtual string True
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
      get => m_ValueFormat.True;

      set => m_ValueFormat.True = value;
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value><c>true</c> if field mapping is specified; otherwise, <c>false</c>.</value>
    /// <remarks>Used for XML Serialization</remarks>
    [XmlIgnore]
    public virtual bool TrueSpecified => m_ValueFormat.DataType == DataType.Boolean;

    /// <summary>
    ///   Mimics to get or sets the value format.
    /// </summary>
    /// <value>The value format.</value>
    [XmlIgnore]
    public ValueFormat ValueFormat
    {
      get => m_ValueFormat;
      set
      {
        var newVal = value ?? new ValueFormat();
        newVal.CopyTo(m_ValueFormat);
      }
    }

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
    ///   Copies to.
    /// </summary>
    /// <param name="other">The other.</param>
    public virtual void CopyTo(Column other)
    {
      if (other == null)
        return;
      m_ValueFormat.CopyTo(other.ValueFormat);
      other.ValueFormat = m_ValueFormat;
      other.PartSplitter = m_PartSplitter;
      other.Part = m_Part;
      other.PartToEnd = m_PartToEnd;
      other.TimePartFormat = m_TimePartFormat;
      other.TimePart = m_TimePart;
      other.TimeZonePart = m_TimeZonePart;
      other.ColumnOrdinal = m_ColumnOrdinal;
      other.Size = m_Size;
      other.Name = m_Name;
      other.Ignore = m_Ignore;
      if (m_Convert.HasValue)
        other.Convert = m_Convert.Value;
    }

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

      return m_ColumnOrdinal == other.m_ColumnOrdinal &&
             string.Equals(m_Name, other.m_Name, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(m_DestinationName, other.m_DestinationName, StringComparison.OrdinalIgnoreCase) &&
             m_Ignore == other.m_Ignore &&
             m_Part == other.m_Part &&
             m_PartSplitter == other.m_PartSplitter &&
             m_PartToEnd == other.m_PartToEnd &&
             m_Size == other.m_Size &&
             string.Equals(m_TimePart, other.m_TimePart, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(m_TimePartFormat, other.m_TimePartFormat, StringComparison.Ordinal) &&
             string.Equals(m_TimeZonePart, other.m_TimeZonePart, StringComparison.OrdinalIgnoreCase) &&
             Convert == other.Convert &&
             m_ValueFormat.Equals(other.m_ValueFormat);
    }

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public virtual event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public virtual void NotifyPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

    /// <summary>
    ///   Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    ///   <see langword="true" /> if the specified object is equal to the current object; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public override bool Equals(object obj) => Equals(obj as Column);

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

    #region Display Methods

    /// <summary>
    ///   Gets the a description of the Date or Number format
    /// </summary>
    /// <returns></returns>
    public string GetFormatDescription()
    {
      switch (m_ValueFormat.DataType)
      {
        case DataType.TextPart:
          return Part + (PartToEnd ? " To End" : string.Empty);

        case DataType.DateTime:
          return m_ValueFormat.DateFormat.ReplaceDefaults(CultureInfo.InvariantCulture.DateTimeFormat.DateSeparator, ValueFormat.DateSeparator,
            CultureInfo.InvariantCulture.DateTimeFormat.TimeSeparator, m_ValueFormat.TimeSeparator);

        case DataType.Numeric:
        case DataType.Double:
          return m_ValueFormat.NumberFormat.ReplaceDefaults(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator,
            m_ValueFormat.DecimalSeparator,
            CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator, m_ValueFormat.GroupSeparator);

        default:
          return string.Empty;
      }
    }

    /// <summary>
    ///   Gets the description.
    /// </summary>
    /// <returns></returns>
    public string GetTypeAndFormatDescription(bool addTime = true)
    {
      var stringBuilder = new StringBuilder(m_ValueFormat.DataType.DataTypeDisplay());

      var shortDesc = GetFormatDescription();
      if (shortDesc.Length > 0)
      {
        stringBuilder.Append(" (");
        stringBuilder.Append(shortDesc);
        stringBuilder.Append(")");
      }

      if (addTime && m_ValueFormat.DataType == DataType.DateTime)
      {
        if (TimePart.Length > 0)
        {
          stringBuilder.Append(" + ");
          stringBuilder.Append(TimePart);
          if (TimePartFormat.Length > 0)
          {
            stringBuilder.Append(" (");
            stringBuilder.Append(TimePartFormat);
            stringBuilder.Append(")");
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

    /// <summary>
    ///   Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string" /> that represents this instance.</returns>
    public override string ToString() => $"{Name} ({GetTypeAndFormatDescription()})";

    #endregion Display Methods
  }
}