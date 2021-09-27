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
  /// <summary>
  ///   Setting for a value format
  /// </summary>
  [Serializable]
  public sealed class ValueFormatMutable : IValueFormat, INotifyPropertyChanged
  {
    private DataType m_DataType;
    private string m_DateFormat;
    private string m_DateSeparator;
    private string m_DecimalSeparator;
    private string m_DisplayNullAs;
    private string m_False;
    private string m_GroupSeparator;
    private string m_NumberFormat;
    private int m_Part;
    private string m_PartSplitter;
    private bool m_PartToEnd;
    private string m_TimeSeparator;
    private string m_True;

    public ValueFormatMutable() : this(
      DataType.String,
      ValueFormatExtension.cDateFormatDefault,
      ValueFormatExtension.cDateSeparatorDefault,
      ValueFormatExtension.cTimeSeparatorDefault,
      ValueFormatExtension.cNumberFormatDefault,
      ValueFormatExtension.cGroupSeparatorDefault,
      ValueFormatExtension.cDecimalSeparatorDefault,
      ValueFormatExtension.cTrueDefault,
      ValueFormatExtension.cFalseDefault,
      string.Empty,
      ValueFormatExtension.cPartDefault,
      ValueFormatExtension.cPartSplitterDefault,
      ValueFormatExtension.cPartToEndDefault)
    {
    }

    public ValueFormatMutable(
      in DataType dataType,
      in string dateFormat,
      in string dateSeparator,
      in string timeSeparator,
      in string numberFormat,
      in string groupSeparator,
      in string decimalSeparator,
      in string asTrue,
      in string asFalse,
      in string displayNullAs,
      int part,
      in string partSplitter,
      bool partToEnd)
    {
      if (!string.IsNullOrEmpty(decimalSeparator) && decimalSeparator.Equals(groupSeparator))
        throw new FileReaderException("Decimal and Group separator must be different");
      m_DataType = dataType;
      m_DateFormat = dateFormat ?? throw new ArgumentNullException(nameof(dateFormat));
      m_DateSeparator = (dateSeparator ?? throw new ArgumentNullException(nameof(dateSeparator))).WrittenPunctuation();
      m_DecimalSeparator = (decimalSeparator ?? throw new ArgumentNullException(nameof(decimalSeparator)))
        .WrittenPunctuation();
      m_GroupSeparator = (groupSeparator ?? throw new ArgumentNullException(nameof(groupSeparator))).WrittenPunctuation();
      m_DisplayNullAs = displayNullAs ?? throw new ArgumentNullException(nameof(displayNullAs));
      m_False = asFalse ?? throw new ArgumentNullException(nameof(asFalse));
      m_NumberFormat = numberFormat ?? throw new ArgumentNullException(nameof(numberFormat));
      m_TimeSeparator = timeSeparator ?? throw new ArgumentNullException(nameof(timeSeparator));
      m_True = asTrue ?? throw new ArgumentNullException(nameof(asTrue));
      m_Part = part;
      m_PartSplitter = (partSplitter ?? throw new ArgumentNullException(nameof(partSplitter))).WrittenPunctuation();
      m_PartToEnd = partToEnd;
    }

    public ValueFormatMutable(IValueFormat other) : this(other.DataType, other.DateFormat, other.DateSeparator, other.TimeSeparator, other.NumberFormat,
      other.GroupSeparator, other.DecimalSeparator,
      other.True, other.False, other.DisplayNullAs, other.Part, other.PartSplitter, other.PartToEnd)

    {
    }

    /// <summary>
    ///   Determines if anything is different to the default values, commonly used for
    ///   serialisation, avoiding empty elements
    /// </summary>

    public bool Specified => !(DataType == DataType.String && DateFormat == ValueFormatExtension.cDateFormatDefault
                                                           && DateSeparator == ValueFormatExtension.cDateSeparatorDefault
                                                           && TimeSeparator == ValueFormatExtension.cTimeSeparatorDefault
                                                           && NumberFormat == ValueFormatExtension.cNumberFormatDefault
                                                           && DecimalSeparator == ValueFormatExtension.cDecimalSeparatorDefault
                                                           && GroupSeparator == ValueFormatExtension.cGroupSeparatorDefault
                                                           && True == ValueFormatExtension.cTrueDefault
                                                           && False == ValueFormatExtension.cFalseDefault
                                                           && Part == ValueFormatExtension.cPartDefault
                                                           && PartSplitter == ValueFormatExtension.cPartSplitterDefault
                                                           && PartToEnd == ValueFormatExtension.cPartToEndDefault
                                                           && DisplayNullAs == string.Empty);

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///   Gets or sets the type of the data.
    /// </summary>
    /// <value>The type of the data.</value>
    [XmlAttribute]
    [DefaultValue(DataType.String)]
    public DataType DataType
    {
      get => m_DataType;
      set
      {
        if (m_DataType.Equals(value))
          return;
        m_DataType = value;
        NotifyPropertyChanged(nameof(DataType));
      }
    }

    /// <summary>
    ///   Gets or sets the date format.
    /// </summary>
    /// <value>The date format.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cDateFormatDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string DateFormat
    {
      get => m_DateFormat;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_DateFormat.Equals(newVal, StringComparison.Ordinal))
          return;
        m_DateFormat = newVal;
        NotifyPropertyChanged(nameof(DateFormat));
      }
    }

    /// <summary>
    ///   Gets or sets the date separator.
    /// </summary>
    /// <value>The date separator.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cDateSeparatorDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string DateSeparator
    {
      get => m_DateSeparator;
      set
      {
        // Translate written punctuation into a character
        var newVal = (value ?? string.Empty).WrittenPunctuation();
        if (m_DateSeparator.Equals(newVal, StringComparison.Ordinal))
          return;
        m_DateSeparator = newVal;
        NotifyPropertyChanged(nameof(DateSeparator));
      }
    }

    /// <summary>
    ///   Gets or sets the decimal separator.
    /// </summary>
    /// <value>The decimal separator.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cDecimalSeparatorDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string DecimalSeparator
    {
      get => m_DecimalSeparator;
      set
      {
        // Translate written punctuation into a character
        var newValDecimal = (value ?? string.Empty).WrittenPunctuation();
        if (m_DecimalSeparator.Equals(newValDecimal))
          return;

        var newValGroup = m_GroupSeparator;
        if (newValGroup.Equals(newValDecimal))
        {
          m_GroupSeparator = "";
          NotifyPropertyChanged(nameof(GroupSeparator));
        }

        m_DecimalSeparator = newValDecimal;
        NotifyPropertyChanged(nameof(DecimalSeparator));
      }
    }

    /// <summary>
    ///   Writing data you can specify how a NULL value should be written, commonly its empty, in
    ///   some circumstances you might want to have n/a etc.
    /// </summary>
    /// <value>Text used if the value is NULL</value>
    [XmlAttribute]
    [DefaultValue("")]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string DisplayNullAs
    {
      get => m_DisplayNullAs;

      set
      {
        var newVal = value ?? string.Empty;
        if (m_DisplayNullAs.Equals(newVal))
          return;
        m_DisplayNullAs = newVal;
        NotifyPropertyChanged(nameof(DisplayNullAs));
      }
    }

    /// <summary>
    ///   Gets or sets the representation for false.
    /// </summary>
    /// <value>The false.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cFalseDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string False
    {
      get => m_False;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_False.Equals(newVal, StringComparison.OrdinalIgnoreCase))
          return;
        m_False = newVal;
        NotifyPropertyChanged(nameof(False));
      }
    }

    /// <summary>
    ///   Gets or sets the group separator.
    /// </summary>
    /// <value>The group separator.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cGroupSeparatorDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string GroupSeparator
    {
      get => m_GroupSeparator;
      set
      {
        var newValGroup = (value ?? string.Empty).WrittenPunctuation();
        if (m_GroupSeparator.Equals(newValGroup))
          return;
        // If we set the GroupSeparator to be the decimal separator, do not save
        var newValDecimal = m_DecimalSeparator;
        if (newValGroup.Equals(newValDecimal))
        {
          m_DecimalSeparator = m_GroupSeparator;
          NotifyPropertyChanged(nameof(DecimalSeparator));
        }

        m_GroupSeparator = newValGroup;
        NotifyPropertyChanged(nameof(GroupSeparator));
      }
    }

    /// <summary>
    ///   Gets or sets the number format.
    /// </summary>
    /// <value>The number format.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cNumberFormatDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string NumberFormat
    {
      get => m_NumberFormat;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_NumberFormat.Equals(newVal, StringComparison.Ordinal))
          return;
        m_NumberFormat = newVal;
        NotifyPropertyChanged(nameof(NumberFormat));
      }
    }

    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cPartDefault)]
    public int Part
    {
      get => m_Part;
      set
      {
        if (m_Part == value)
          return;
        m_Part = value;
        NotifyPropertyChanged(nameof(Part));
      }
    }

#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif

    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cPartSplitterDefault)]
    public string PartSplitter
    {
      get => m_PartSplitter;
      set
      {
        var newVal = (value ?? string.Empty).WrittenPunctuation();
        if (m_PartSplitter.Equals(newVal, StringComparison.Ordinal))
          return;
        m_PartSplitter = newVal;
        NotifyPropertyChanged(nameof(PartSplitter));
      }
    }

    [XmlAttribute]
    [DefaultValue(ValueFormatExtension.cPartToEndDefault)]
    public bool PartToEnd
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
    ///   Gets or sets the time separator.
    /// </summary>
    /// <value>The time separator.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cTimeSeparatorDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string TimeSeparator
    {
      get => m_TimeSeparator;
      set
      {
        var newVal = (value ?? string.Empty).WrittenPunctuation();
        if (m_TimeSeparator.Equals(newVal, StringComparison.Ordinal))
          return;
        m_TimeSeparator = newVal;
        NotifyPropertyChanged(nameof(TimeSeparator));
      }
    }

    /// <summary>
    ///   Gets or sets the representation for true.
    /// </summary>
    /// <value>The true.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cTrueDefault)]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string True
    {
      get => m_True;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_True.Equals(newVal, StringComparison.OrdinalIgnoreCase))
          return;
        m_True = newVal;
        NotifyPropertyChanged(nameof(True));
      }
    }

    /// <summary>
    ///   On Mutable classes prefer CopyFrom to CopyTo, overwrites the properties from the
    ///   properties in the provided class
    /// </summary>
    /// <param name="other"></param>
    public void CopyFrom(IValueFormat? other)
    {
      if (other is null)
        return;
      DataType = other.DataType;
      DateFormat = other.DateFormat;
      DateSeparator = other.DateSeparator;
      TimeSeparator = other.TimeSeparator;
      NumberFormat = other.NumberFormat;
      GroupSeparator = other.GroupSeparator;
      DecimalSeparator = other.DecimalSeparator;
      True = other.True;
      False = other.False;
      DisplayNullAs = other.DisplayNullAs;
      Part = other.Part;
      PartSplitter = other.PartSplitter;
      PartToEnd = other.PartToEnd;
    }

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public void NotifyPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
  }
}