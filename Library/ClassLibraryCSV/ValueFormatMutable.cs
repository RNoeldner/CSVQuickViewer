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

using JetBrains.Annotations;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   Setting for a value format
  /// </summary>
  [Serializable]
  public sealed class ValueFormatMutable : IValueFormat, INotifyPropertyChanged
  {
    private ImmutableValueFormat m_ImmutableValueFormat;

    public ValueFormatMutable() => m_ImmutableValueFormat = new ImmutableValueFormat();

    public ValueFormatMutable([NotNull] IValueFormat other) => CopyFrom(other);

    public bool IsDefault => m_ImmutableValueFormat.IsDefault;

    /// <summary>
    ///   Gets or sets the date format.
    /// </summary>
    /// <value>The date format.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cDateFormatDefault)]
    public string DateFormat
    {
      [NotNull]
      get => m_ImmutableValueFormat.DateFormat;
      [CanBeNull]
      set
      {
        var newVal = value ?? string.Empty;
        if (m_ImmutableValueFormat.DateFormat.Equals(newVal, StringComparison.Ordinal))
          return;
        m_ImmutableValueFormat = new ImmutableValueFormat(m_ImmutableValueFormat.DataType, newVal,
                  m_ImmutableValueFormat.DateSeparator, m_ImmutableValueFormat.DecimalSeparatorChar, m_ImmutableValueFormat.DisplayNullAs,
                  m_ImmutableValueFormat.False, m_ImmutableValueFormat.GroupSeparatorChar, m_ImmutableValueFormat.NumberFormat,
                  m_ImmutableValueFormat.TimeSeparator, m_ImmutableValueFormat.True);
        NotifyPropertyChanged(nameof(DateFormat));
      }
    }

    /// <summary>
    ///   Gets or sets the date separator.
    /// </summary>
    /// <value>The date separator.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cDateSeparatorDefault)]
    public string DateSeparator
    {
      [NotNull]
      get => m_ImmutableValueFormat.DateSeparator;
      [CanBeNull]
      set
      {
        // Translate written punctuation into a character
        var chr = value.WrittenPunctuationToChar();
        var newVal = chr != '\0' ? chr.ToString(CultureInfo.CurrentCulture) : string.Empty;
        if (m_ImmutableValueFormat.DateSeparator.Equals(newVal, StringComparison.Ordinal))
          return;
        m_ImmutableValueFormat = new ImmutableValueFormat(m_ImmutableValueFormat.DataType, m_ImmutableValueFormat.DateFormat,
                  newVal, newVal.GetFirstChar(), m_ImmutableValueFormat.DisplayNullAs,
                  m_ImmutableValueFormat.False, m_ImmutableValueFormat.GroupSeparatorChar, m_ImmutableValueFormat.NumberFormat,
                  m_ImmutableValueFormat.TimeSeparator, m_ImmutableValueFormat.True);
        NotifyPropertyChanged(nameof(DateSeparator));
      }
    }

    /// <summary>
    ///   Gets or sets the decimal separator.
    /// </summary>
    /// <value>The decimal separator.</value>
    [XmlElement]
    public string DecimalSeparator
    {
      [NotNull]
      get => m_ImmutableValueFormat.DecimalSeparatorChar.ToString();
      set
      {
        // Translate written punctuation into a character
        var newValDecimal = value.WrittenPunctuationToChar();
        if (m_ImmutableValueFormat.DecimalSeparatorChar.Equals(newValDecimal))
          return;

        var newValGroup = m_ImmutableValueFormat.GroupSeparatorChar;
        if (newValGroup.Equals(newValDecimal))
        {
          newValGroup = '\0';
          NotifyPropertyChanged(nameof(GroupSeparator));
        }

        m_ImmutableValueFormat = new ImmutableValueFormat(m_ImmutableValueFormat.DataType, m_ImmutableValueFormat.DateFormat,
                  m_ImmutableValueFormat.DateSeparator, newValDecimal, m_ImmutableValueFormat.DisplayNullAs,
                  m_ImmutableValueFormat.False, newValGroup, m_ImmutableValueFormat.NumberFormat,
                  m_ImmutableValueFormat.TimeSeparator, m_ImmutableValueFormat.True);
        NotifyPropertyChanged(nameof(DecimalSeparator));
      }
    }

    [XmlIgnore] [UsedImplicitly] public bool DecimalSeparatorSpecified => m_ImmutableValueFormat.DecimalSeparatorChar != ValueFormatExtension.cDecimalSeparatorDefault;

    /// <summary>
    ///   Gets or sets the representation for false.
    /// </summary>
    /// <value>The false.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cFalseDefault)]
    public string False
    {
      [NotNull]
      get => m_ImmutableValueFormat.False;
      [CanBeNull]
      set
      {
        var newVal = value ?? string.Empty;
        if (m_ImmutableValueFormat.False.Equals(newVal, StringComparison.OrdinalIgnoreCase))
          return;
        m_ImmutableValueFormat = new ImmutableValueFormat(m_ImmutableValueFormat.DataType, m_ImmutableValueFormat.DateFormat,
                  m_ImmutableValueFormat.DateSeparator, m_ImmutableValueFormat.DecimalSeparatorChar, m_ImmutableValueFormat.DisplayNullAs,
                  newVal, m_ImmutableValueFormat.GroupSeparatorChar, m_ImmutableValueFormat.NumberFormat,
                  m_ImmutableValueFormat.TimeSeparator, m_ImmutableValueFormat.True);
        NotifyPropertyChanged(nameof(False));
      }
    }

    /// <summary>
    ///   Gets or sets the group separator.
    /// </summary>
    /// <value>The group separator.</value>
    [XmlElement]
    public string GroupSeparator
    {
      [NotNull]
      get => m_ImmutableValueFormat.GroupSeparatorChar == '\0' ? string.Empty : m_ImmutableValueFormat.GroupSeparatorChar.ToString();
      set
      {
        var newValGroup = value.WrittenPunctuationToChar();
        if (m_ImmutableValueFormat.GroupSeparatorChar.Equals(newValGroup))
          return;
        // If we set the GroupSeparator to be the decimal separator, do not save
        var newValDecimal = m_ImmutableValueFormat.DecimalSeparatorChar;
        if (newValGroup.Equals(newValDecimal))
        {
          newValDecimal=m_ImmutableValueFormat.GroupSeparatorChar;
          NotifyPropertyChanged(nameof(DecimalSeparator));
        }

        m_ImmutableValueFormat = new ImmutableValueFormat(m_ImmutableValueFormat.DataType, m_ImmutableValueFormat.DateFormat,
           m_ImmutableValueFormat.DateSeparator, newValDecimal, m_ImmutableValueFormat.DisplayNullAs,
           m_ImmutableValueFormat.False, newValGroup, m_ImmutableValueFormat.NumberFormat,
           m_ImmutableValueFormat.TimeSeparator, m_ImmutableValueFormat.True);

        NotifyPropertyChanged(nameof(GroupSeparator));
      }
    }

    [XmlIgnore] [UsedImplicitly] public bool GroupSeparatorSpecified => m_ImmutableValueFormat.GroupSeparatorChar != ValueFormatExtension.cGroupSeparatorDefault;

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    [XmlIgnore]
    public char GroupSeparatorChar
    {
      get => m_ImmutableValueFormat.GroupSeparatorChar;
      set
      {
        if (m_ImmutableValueFormat.GroupSeparatorChar== value)
          return;

        m_ImmutableValueFormat = new ImmutableValueFormat(m_ImmutableValueFormat.DataType, m_ImmutableValueFormat.DateFormat,
           m_ImmutableValueFormat.DateSeparator, m_ImmutableValueFormat.DecimalSeparatorChar, m_ImmutableValueFormat.DisplayNullAs,
           m_ImmutableValueFormat.False, value, m_ImmutableValueFormat.NumberFormat,
           m_ImmutableValueFormat.TimeSeparator, m_ImmutableValueFormat.True);

        NotifyPropertyChanged(nameof(GroupSeparatorChar));
      }
    }

    [XmlIgnore]
    public char DecimalSeparatorChar
    {
      get => m_ImmutableValueFormat.DecimalSeparatorChar;
      set
      {
        if (m_ImmutableValueFormat.DecimalSeparatorChar== value)
          return;
        m_ImmutableValueFormat = new ImmutableValueFormat(m_ImmutableValueFormat.DataType, m_ImmutableValueFormat.DateFormat,
                   m_ImmutableValueFormat.DateSeparator, value, m_ImmutableValueFormat.DisplayNullAs,
                   m_ImmutableValueFormat.False, m_ImmutableValueFormat.GroupSeparatorChar, m_ImmutableValueFormat.NumberFormat,
                   m_ImmutableValueFormat.TimeSeparator, m_ImmutableValueFormat.True);

        NotifyPropertyChanged(nameof(DecimalSeparatorChar));
      }
    }

    /// <summary>
    ///   Gets or sets the type of the data.
    /// </summary>
    /// <value>The type of the data.</value>
    [XmlAttribute]
    [DefaultValue(DataType.String)]
    public DataType DataType
    {
      get => m_ImmutableValueFormat.DataType;
      set
      {
        if (m_ImmutableValueFormat.DataType.Equals(value))
          return;
        m_ImmutableValueFormat = new ImmutableValueFormat(value, m_ImmutableValueFormat.DateFormat,
                  m_ImmutableValueFormat.DateSeparator, m_ImmutableValueFormat.DecimalSeparatorChar, m_ImmutableValueFormat.DisplayNullAs,
                  m_ImmutableValueFormat.False, m_ImmutableValueFormat.GroupSeparatorChar, m_ImmutableValueFormat.NumberFormat,
                  m_ImmutableValueFormat.TimeSeparator, m_ImmutableValueFormat.True);
        NotifyPropertyChanged(nameof(DataType));
      }
    }

    /// <summary>
    ///   Writing data you can specify how a NULL value should be written, commonly its empty, in
    ///   some circumstances you might want to have n/a etc.
    /// </summary>
    /// <value>Text used if the value is NULL</value>
    [XmlAttribute]
    [DefaultValue("")]
    public string DisplayNullAs
    {
      [NotNull]
      get => m_ImmutableValueFormat.DisplayNullAs;

      set
      {
        var newVal = value ?? string.Empty;
        if (m_ImmutableValueFormat.DisplayNullAs.Equals(newVal))
          return;
        m_ImmutableValueFormat = new ImmutableValueFormat(m_ImmutableValueFormat.DataType, m_ImmutableValueFormat.DateFormat,
                  m_ImmutableValueFormat.DateSeparator, m_ImmutableValueFormat.DecimalSeparatorChar, newVal,
                  m_ImmutableValueFormat.False, m_ImmutableValueFormat.GroupSeparatorChar, m_ImmutableValueFormat.NumberFormat,
                  m_ImmutableValueFormat.TimeSeparator, m_ImmutableValueFormat.True);
        NotifyPropertyChanged(nameof(DisplayNullAs));
      }
    }

    /// <summary>
    ///   Gets or sets the number format.
    /// </summary>
    /// <value>The number format.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cNumberFormatDefault)]
    public string NumberFormat
    {
      [NotNull]
      get => m_ImmutableValueFormat.NumberFormat;
      [CanBeNull]
      set
      {
        var newVal = value ?? string.Empty;
        if (m_ImmutableValueFormat.NumberFormat.Equals(newVal, StringComparison.Ordinal))
          return;
        m_ImmutableValueFormat = new ImmutableValueFormat(m_ImmutableValueFormat.DataType, m_ImmutableValueFormat.DateFormat,
                  m_ImmutableValueFormat.DateSeparator, m_ImmutableValueFormat.DecimalSeparatorChar, m_ImmutableValueFormat.DisplayNullAs,
                  m_ImmutableValueFormat.False, m_ImmutableValueFormat.GroupSeparatorChar, newVal,
                  m_ImmutableValueFormat.TimeSeparator, m_ImmutableValueFormat.True);
        NotifyPropertyChanged(nameof(NumberFormat));
      }
    }

    /// <summary>
    ///   Gets or sets the time separator.
    /// </summary>
    /// <value>The time separator.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cTimeSeparatorDefault)]
    public string TimeSeparator
    {
      [NotNull]
      get => m_ImmutableValueFormat.TimeSeparator;
      [NotNull]
      set
      {
        var chr = value.WrittenPunctuationToChar();
        var newVal = chr != '\0' ? chr.ToString(CultureInfo.CurrentCulture) : string.Empty;
        if (m_ImmutableValueFormat.TimeSeparator.Equals(newVal, StringComparison.Ordinal))
          return;
        m_ImmutableValueFormat = new ImmutableValueFormat(m_ImmutableValueFormat.DataType, m_ImmutableValueFormat.DateFormat,
                  m_ImmutableValueFormat.DateSeparator, m_ImmutableValueFormat.DecimalSeparatorChar, m_ImmutableValueFormat.DisplayNullAs,
                  m_ImmutableValueFormat.False, m_ImmutableValueFormat.GroupSeparatorChar, m_ImmutableValueFormat.NumberFormat,
                  newVal, m_ImmutableValueFormat.True);
        NotifyPropertyChanged(nameof(TimeSeparator));
      }
    }

    /// <summary>
    ///   Gets or sets the representation for true.
    /// </summary>
    /// <value>The true.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cTrueDefault)]
    public string True
    {
      [NotNull]
      get => m_ImmutableValueFormat.True;
      [CanBeNull]
      set
      {
        var newVal = value ?? string.Empty;
        if (m_ImmutableValueFormat.True.Equals(newVal, StringComparison.OrdinalIgnoreCase))
          return;
        m_ImmutableValueFormat = new ImmutableValueFormat(m_ImmutableValueFormat.DataType, m_ImmutableValueFormat.DateFormat,
                  m_ImmutableValueFormat.DateSeparator, m_ImmutableValueFormat.DecimalSeparatorChar, m_ImmutableValueFormat.DisplayNullAs,
                  m_ImmutableValueFormat.False, m_ImmutableValueFormat.GroupSeparatorChar, m_ImmutableValueFormat.NumberFormat,
                  m_ImmutableValueFormat.TimeSeparator, newVal);
        NotifyPropertyChanged(nameof(True));
      }
    }

    public bool Equals(IValueFormat other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;

      if (other.DataType != DataType || !other.DisplayNullAs.Equals(DisplayNullAs, StringComparison.Ordinal))
        return false;

      switch (DataType)
      {
        case DataType.Integer:
          return string.Equals(other.NumberFormat, NumberFormat, StringComparison.Ordinal);

        case DataType.Numeric:
        case DataType.Double:
          return other.GroupSeparatorChar == GroupSeparatorChar &&
                 other.DecimalSeparatorChar == DecimalSeparatorChar &&
                 string.Equals(other.NumberFormat, NumberFormat, StringComparison.Ordinal);

        case DataType.DateTime:
          return string.Equals(other.DateFormat, DateFormat, StringComparison.Ordinal) &&
                 string.Equals(other.DateSeparator, DateSeparator, StringComparison.Ordinal) &&
                 string.Equals(other.TimeSeparator, TimeSeparator, StringComparison.Ordinal);

        case DataType.Boolean:
          return string.Equals(other.False, False, StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(other.True, True, StringComparison.OrdinalIgnoreCase);

        default:
          // compare everything
          return string.Equals(other.DateFormat, DateFormat, StringComparison.Ordinal) &&
                 string.Equals(other.DateSeparator, DateSeparator, StringComparison.Ordinal) &&
                 string.Equals(other.TimeSeparator, TimeSeparator, StringComparison.Ordinal) &&
                 string.Equals(other.False, False, StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(other.True, True, StringComparison.OrdinalIgnoreCase) &&
                 other.GroupSeparatorChar == GroupSeparatorChar &&
                 other.DecimalSeparatorChar == DecimalSeparatorChar &&
                 string.Equals(other.NumberFormat, NumberFormat, StringComparison.Ordinal);
      }
    }

    public void CopyFrom(IValueFormat other) =>
     m_ImmutableValueFormat = new ImmutableValueFormat(other.DataType, other.DateFormat, other.DateSeparator, other.DecimalSeparatorChar, other.DisplayNullAs, other.False, other.GroupSeparatorChar, other.NumberFormat, other.TimeSeparator, other.True);

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public void NotifyPropertyChanged(string info) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
  }
}