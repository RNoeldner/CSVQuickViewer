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

    private int m_ColumnOrdinal;
    private bool m_Convert;
    private DataType m_DataType = DataType.String;
    private string m_DateFormat = ValueFormat.cDateFormatDefault;
    private string m_DateSeparator = ValueFormat.cDateSeparatorDefault;
    private string m_DecimalSeparator = ValueFormat.cDecimalSeparatorDefault;
    private char m_DecimalSeparatorChar = ValueFormat.cDecimalSeparatorDefault.GetFirstChar();
    private string m_DestinationName = string.Empty;
    private string m_False = ValueFormat.cFalseDefault;
    private string m_GroupSeparator = ValueFormat.cGroupSeparatorDefault;
    private bool m_Ignore;
    private string m_Name = string.Empty;
    private string m_NumberFormat = ValueFormat.cNumberFormatDefault;
    private int m_Part = c_PartDefault;
    private char m_PartSplitter = c_PartSplitterDefault;
    private bool m_PartToEnd = c_PartToEnd;
    private int m_Size;
    private string m_TimePart = string.Empty;
    private string m_TimePartFormat = cDefaultTimePartFormat;
    private string m_TimeSeparator = ValueFormat.cTimeSeparatorDefault;
    private string m_TimeZonePart = string.Empty;
    private string m_True = ValueFormat.cTrueDefault;

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
    ///   Gets or sets a value indicating whether this <see cref="Column" /> is convert. Only
    ///   used to untype a typed value (reading typed value from Excel to make it a string)
    /// </summary>
    /// <value><c>true</c> if the column should be convert; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool Convert
    {
      get => m_Convert;
      set
      {
        if (m_Convert.Equals(value)) return;
        m_Convert = value;
        NotifyPropertyChanged(nameof(Convert));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool ConvertSpecified => DataType == DataType.String;

    /// <summary>
    ///   Gets or sets the type of the data.
    /// </summary>
    /// <value>The type of the data.</value>
    [XmlAttribute("Type")]
    [DefaultValue(DataType.String)]
    public virtual DataType DataType
    {
      get => m_DataType;

      set
      {
        if (m_DataType.Equals(value)) return;
        m_DataType = value;
        Convert |= m_DataType != DataType.String;
        NotifyPropertyChanged(nameof(DataType));
      }
    }

    /// <summary>
    ///   Gets or sets the date format.
    /// </summary>
    /// <value>The date format.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormat.cDateFormatDefault)]
    public virtual string DateFormat
    {
      get => m_DateFormat;

      set
      {
        var newVal = value ?? string.Empty;
        if (m_DateFormat.Equals(newVal, StringComparison.Ordinal)) return;
        m_DateFormat = newVal;
        NotifyPropertyChanged(nameof(DateFormat));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool DateFormatSpecified => DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the date separator.
    /// </summary>
    /// <value>The date separator.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormat.cDateSeparatorDefault)]
    public virtual string DateSeparator
    {
      get => m_DateSeparator;

      set
      {
        // Translate written punctuation into a character
        var chr = FileFormat.GetChar(value);
        var newVal = chr != '\0' ? chr.ToString(CultureInfo.CurrentCulture) : string.Empty;
        if (m_DateSeparator.Equals(newVal, StringComparison.Ordinal)) return;
        m_DateSeparator = newVal;
        NotifyPropertyChanged(nameof(DateSeparator));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool DateSeparatorSpecified => DataType == DataType.DateTime;

    [XmlIgnore] public virtual char DecimalSeparatorChar => m_DecimalSeparatorChar;

    /// <summary>
    ///   Gets or sets the decimal separator.
    /// </summary>
    /// <value>The decimal separator.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormat.cDecimalSeparatorDefault)]
    public virtual string DecimalSeparator
    {
      get => m_DecimalSeparator;

      set
      {
        // Translate written punctuation into a character
        var chr = FileFormat.GetChar(value);
        var newVal = chr != '\0' ? chr.ToString(CultureInfo.CurrentCulture) : string.Empty;
        if (m_DecimalSeparator.Equals(newVal, StringComparison.Ordinal)) return;
        // If we set the DecimalSeparator to be the Group separator, store the old
        // DecimalSeparator in the group separator;
        if (m_GroupSeparator.Equals(newVal, StringComparison.Ordinal))
        {
          m_GroupSeparator = m_DecimalSeparator;
          NotifyPropertyChanged(nameof(GroupSeparator));
        }

        m_DecimalSeparator = newVal;
        m_DecimalSeparatorChar = m_DecimalSeparator.GetFirstChar();
        NotifyPropertyChanged(nameof(DecimalSeparator));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool DecimalSeparatorSpecified => DataType == DataType.Double || DataType == DataType.Numeric;

    /// <summary>
    ///   Gets or sets the name in a destination. This is only used for writing
    /// </summary>
    /// <value>
    ///   The name of the column in the destination.
    /// </value>
    [XmlAttribute]
    public virtual string DestinationName
    {
      get => m_DestinationName;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_DestinationName.Equals(newVal, StringComparison.Ordinal)) return;
        m_DestinationName = newVal;
        NotifyPropertyChanged(nameof(DestinationName));
      }
    }

    [XmlIgnore] public virtual bool DestinationNameSpecified => !m_DestinationName.Equals(m_Name, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    ///   Gets or sets the representation for false.
    /// </summary>
    /// <value>The false.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormat.cFalseDefault)]
#pragma warning disable CA1716 // Identifiers should not match keywords
    public virtual string False
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
      get => m_False;

      set
      {
        if (m_False != null && m_False.Equals(value, StringComparison.Ordinal)) return;
        m_False = value;
        NotifyPropertyChanged(nameof(False));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool FalseSpecified => DataType == DataType.Boolean;

    [XmlIgnore]
    public char GroupSeparatorChar { get; private set; } = ValueFormat.cGroupSeparatorDefault.GetFirstChar();

    /// <summary>
    ///   Gets or sets the group separator.
    /// </summary>
    /// <value>The group separator.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormat.cGroupSeparatorDefault)]
    public virtual string GroupSeparator
    {
      get => m_GroupSeparator;
      set
      {
        var chr = FileFormat.GetChar(value);
        var newVal = chr != '\0' ? chr.ToString(CultureInfo.CurrentCulture) : string.Empty;

        if (m_GroupSeparator.Equals(newVal, StringComparison.Ordinal)) return;
        // If we set the DecimalSeparator to be the group separator, store the old
        // DecimalSeparator in the group separator;
        if (m_DecimalSeparator.Equals(newVal, StringComparison.Ordinal))
        {
          m_DecimalSeparator = m_GroupSeparator;
          NotifyPropertyChanged(nameof(DecimalSeparator));
        }

        m_GroupSeparator = newVal;
        GroupSeparatorChar = m_GroupSeparator.GetFirstChar();
        NotifyPropertyChanged(nameof(GroupSeparator));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool GroupSeparatorSpecified =>
      DataType == DataType.Double || DataType == DataType.Numeric || DataType == DataType.Integer;

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
        if (m_Ignore.Equals(value)) return;
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
        if (m_Name.Equals(newVal, StringComparison.Ordinal)) return;
        m_Name = newVal;

        NotifyPropertyChanged(nameof(Name));
        if (m_DestinationName.Length == 0)
          DestinationName = newVal;
      }
    }

    /// <summary>
    ///   Gets or sets the number format.
    /// </summary>
    /// <value>The number format.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormat.cNumberFormatDefault)]
    public virtual string NumberFormat
    {
      get => m_NumberFormat;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_NumberFormat.Equals(newVal, StringComparison.Ordinal)) return;
        m_NumberFormat = newVal;
        NotifyPropertyChanged(nameof(NumberFormat));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if number format is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool NumberFormatSpecified => DataType == DataType.Double || DataType == DataType.Numeric;

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
        if (m_Part.Equals(value)) return;
        m_Part = value;
        NotifyPropertyChanged(nameof(Part));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool PartSpecified => DataType == DataType.TextPart;

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
        if (m_PartSplitter.Equals(value)) return;
        m_PartSplitter = value;
        NotifyPropertyChanged(nameof(PartSplitter));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool PartSplitterSpecified =>
      DataType == DataType.TextPart && !m_PartSplitter.Equals(c_PartSplitterDefault);

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
        if (m_PartToEnd.Equals(value)) return;
        m_PartToEnd = value;
        NotifyPropertyChanged(nameof(PartToEnd));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool PartToEndSpecified => DataType == DataType.TextPart;

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
        if (m_TimePart.Equals(newVal, StringComparison.Ordinal)) return;
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
        if (m_TimeZonePart.Equals(newVal, StringComparison.Ordinal)) return;
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
        if (m_TimePartFormat.Equals(newVal, StringComparison.Ordinal)) return;
        m_TimePartFormat = newVal;
        NotifyPropertyChanged(nameof(TimePartFormat));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool TimePartFormatSpecified => DataType == DataType.DateTime;

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool TimePartSpecified => DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the time separator.
    /// </summary>
    /// <value>The time separator.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormat.cTimeSeparatorDefault)]
    public virtual string TimeSeparator
    {
      get => m_TimeSeparator;
      set
      {
        var chr = FileFormat.GetChar(value);
        var newval = chr != '\0' ? chr.ToString(CultureInfo.CurrentCulture) : string.Empty;
        if (m_TimeSeparator.Equals(newval, StringComparison.Ordinal)) return;
        m_TimeSeparator = newval;
        NotifyPropertyChanged(nameof(TimeSeparator));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool TimeSeparatorSpecified => DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the representation for true.
    /// </summary>
    /// <value>The true.</value>
    [XmlAttribute]
    [DefaultValue(ValueFormat.cTrueDefault)]
#pragma warning disable CA1716 // Identifiers should not match keywords
    public virtual string True
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
      get => m_True;

      set
      {
        if (m_True != null && m_True.Equals(value, StringComparison.Ordinal)) return;
        m_True = value;
        NotifyPropertyChanged(nameof(True));
      }
    }

    /// <summary>
    ///   Gets a value indicating whether the Xml field is specified.
    /// </summary>
    /// <value>
    ///   <c>true</c> if field mapping is specified; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///   Used for XML Serialization
    /// </remarks>
    [XmlIgnore]
    public virtual bool TrueSpecified => DataType == DataType.Boolean;

    /// <summary>
    ///   Mimics to get or sets the value format.
    /// </summary>
    /// <value>
    ///   The value format.
    /// </value>
    [XmlIgnore]
    public ValueFormat ValueFormat
    {
      get => new ValueFormat
      {
        DataType = DataType,
        DateFormat = DateFormat,
        DateSeparator = DateSeparator,
        DecimalSeparator = DecimalSeparator,
        False = False,
        True = True,
        GroupSeparator = GroupSeparator,
        NumberFormat = NumberFormat
      };
      set
      {
        if (value.DataType != DataType.String)
          DataType = value.DataType;
        DateFormat = value.DateFormat;
        DateSeparator = value.DateSeparator;
        DecimalSeparator = value.DecimalSeparator;
        False = value.False;
        GroupSeparator = value.GroupSeparator;
        NumberFormat = value.NumberFormat;
        True = value.True;
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
      other.DataType = DataType;
      other.DateFormat = DateFormat;
      other.DateSeparator = DateSeparator;
      other.DecimalSeparator = DecimalSeparator;
      other.False = False;
      other.GroupSeparator = GroupSeparator;
      other.NumberFormat = NumberFormat;
      other.PartSplitter = PartSplitter;
      other.Part = Part;
      other.PartToEnd = PartToEnd;
      other.TimeSeparator = TimeSeparator;
      other.True = True;
      other.TimePartFormat = TimePartFormat;
      other.TimePart = TimePart;
      other.TimeZonePart = TimeZonePart;

      other.ColumnOrdinal = ColumnOrdinal;
      other.Size = Size;

      other.Name = Name;
      other.Ignore = Ignore;
      other.Convert = Convert;
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public bool Equals(Column other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;
      return m_ColumnOrdinal == other.m_ColumnOrdinal && m_Convert == other.m_Convert &&
             m_DataType == other.m_DataType && string.Equals(m_DateFormat, other.m_DateFormat, StringComparison.Ordinal) &&
             string.Equals(m_DateSeparator, other.m_DateSeparator, StringComparison.Ordinal) &&
             string.Equals(m_DecimalSeparator, other.m_DecimalSeparator, StringComparison.Ordinal) &&
             m_DecimalSeparatorChar == other.m_DecimalSeparatorChar &&
             string.Equals(m_DestinationName, other.m_DestinationName, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(m_False, other.m_False, StringComparison.Ordinal) &&
             string.Equals(m_GroupSeparator, other.m_GroupSeparator, StringComparison.Ordinal) && m_Ignore == other.m_Ignore &&
             string.Equals(m_Name, other.m_Name, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(m_NumberFormat, other.m_NumberFormat, StringComparison.Ordinal) &&
             m_Part == other.m_Part && m_PartSplitter == other.m_PartSplitter && m_PartToEnd == other.m_PartToEnd &&
             m_Size == other.m_Size && string.Equals(m_TimePart, other.m_TimePart, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(m_TimePartFormat, other.m_TimePartFormat, StringComparison.Ordinal) &&
             string.Equals(m_TimeSeparator, other.m_TimeSeparator, StringComparison.Ordinal) &&
             string.Equals(m_TimeZonePart, other.m_TimeZonePart, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(m_True, other.m_True, StringComparison.Ordinal) &&
             GroupSeparatorChar == other.GroupSeparatorChar;
    }

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public virtual event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Determines whether the specified expected column is matching this column.
    /// </summary>
    /// <param name="expected">The expected column format.</param>
    /// <returns><c>true</c> if the current format would be acceptable for the expected data type.</returns>
    /// <remarks>
    ///   Is matching only looks at data type and some formats, it is assumed that we do not distinguish between numeric
    ///   formats, it is O.K. to expect a money value but have a integer
    /// </remarks>
    public bool IsMatching(ValueFormat expected)
    {
      if (expected.DataType == DataType)
        return true;

      // if one is integer but we expect numeric or vice versa, assume its OK, one of the sides does
      // not have a decimal separator
      if ((expected.DataType == DataType.Numeric || expected.DataType == DataType.Double ||
           expected.DataType == DataType.Integer)
          && DataType == DataType.Integer)
        return true;
      if (expected.DataType == DataType.Integer
          && (DataType == DataType.Numeric || DataType == DataType.Double || DataType == DataType.Integer))
        return true;
      // if we have dates, check the formats
      if (expected.DataType == DataType.DateTime && DataType == DataType.DateTime)
        return expected.DateFormat.Equals(m_DateFormat, StringComparison.Ordinal) &&
               (m_DateFormat.IndexOf('/') == -1 ||
                expected.DateSeparator.Equals(DateSeparator, StringComparison.Ordinal)) &&
               (m_DateFormat.IndexOf(':') == -1 ||
                expected.TimeSeparator.Equals(TimeSeparator, StringComparison.Ordinal));
      // if we have decimals, check the formats
      if ((expected.DataType == DataType.Numeric || expected.DataType == DataType.Double) &&
          (DataType == DataType.Numeric || DataType == DataType.Double))
        return expected.NumberFormat.Equals(NumberFormat, StringComparison.Ordinal) &&
               expected.DecimalSeparator.Equals(DecimalSeparator, StringComparison.Ordinal) &&
               expected.GroupSeparator.Equals(GroupSeparator, StringComparison.Ordinal);
      // For everything else assume its wrong
      return false;
    }

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public virtual void NotifyPropertyChanged(string info)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
    }

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object obj)
    {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      return (obj is Column typed) && Equals(typed);
    }

    /*
    /// <summary>
    ///   Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    ///   A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
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
      switch (DataType)
      {
        case DataType.TextPart:
          return Part + (PartToEnd ? " To End" : string.Empty);

        case DataType.DateTime:
          return DateFormat.ReplaceDefaults(CultureInfo.InvariantCulture.DateTimeFormat.DateSeparator, DateSeparator,
            CultureInfo.InvariantCulture.DateTimeFormat.TimeSeparator, TimeSeparator);

        case DataType.Numeric:
        case DataType.Double:
          return NumberFormat.ReplaceDefaults(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator,
            DecimalSeparator,
            CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator, GroupSeparator);

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
      var sbtext = new StringBuilder(DataType.DataTypeDisplay());

      var shortDesc = GetFormatDescription();
      if (shortDesc.Length > 0)
      {
        sbtext.Append(" (");
        sbtext.Append(shortDesc);
        sbtext.Append(")");
      }

      if (addTime && DataType == DataType.DateTime)
      {
        if (TimePart.Length > 0)
        {
          sbtext.Append(" + ");
          sbtext.Append(TimePart);
          if (TimePartFormat.Length > 0)
          {
            sbtext.Append(" (");
            sbtext.Append(TimePartFormat);
            sbtext.Append(")");
          }
        }

        if (TimeZonePart.Length > 0)
        {
          sbtext.Append(" - ");
          sbtext.Append(TimeZonePart);
        }
      }

      if (Ignore)
        sbtext.Append(" (Ignore)");

      return sbtext.ToString();
    }

    /// <summary>
    ///   Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="string" /> that represents this instance.</returns>
    public override string ToString()
    {
      return $"{Name} ({GetTypeAndFormatDescription()})";
    }

    #endregion Display Methods
  }
}