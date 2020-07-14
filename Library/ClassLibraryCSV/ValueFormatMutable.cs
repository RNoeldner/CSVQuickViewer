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
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace CsvTools
{
  /// <summary>
  ///   Setting for a value format
  /// </summary>
  [Serializable]
  public sealed class ValueFormatMutable : IValueFormat, IEquatable<ValueFormatMutable>, INotifyPropertyChanged
  {
    private DataType m_DataType = DataType.String;

    private string m_DateFormat = ValueFormatExtension.cDateFormatDefault;

    private string m_DateSeparator = ValueFormatExtension.cDateSeparatorDefault;

    private string m_DecimalSeparator = ValueFormatExtension.cDecimalSeparatorDefault;

    private string m_DisplayNullAs = string.Empty;

    private string m_False = ValueFormatExtension.cFalseDefault;

    private string m_GroupSeparator = ValueFormatExtension.cGroupSeparatorDefault;

    private string m_NumberFormat = ValueFormatExtension.cNumberFormatDefault;

    private string m_TimeSeparator = ValueFormatExtension.cTimeSeparatorDefault;

    private string m_True = ValueFormatExtension.cTrueDefault;

    /// <summary>
    ///   Initializes a new instance of the <see cref="ValueFormatMutable" /> class.
    /// </summary>
    public ValueFormatMutable()
    {
    }

    /// <summary>
    /// Used in Serialization to determine if something needs to be stored
    /// </summary>
    public bool IsDefault =>
      m_DataType == DataType.String &&
      m_DateFormat == ValueFormatExtension.cDateFormatDefault &&
      m_True == ValueFormatExtension.cTrueDefault &&
      m_False == ValueFormatExtension.cFalseDefault &&
      m_TimeSeparator == ValueFormatExtension.cTimeSeparatorDefault &&
      m_NumberFormat == ValueFormatExtension.cNumberFormatDefault &&
      m_DisplayNullAs == string.Empty &&
      m_DecimalSeparator == ValueFormatExtension.cDecimalSeparatorDefault &&
      m_DateSeparator == ValueFormatExtension.cDateSeparatorDefault;

    public ValueFormatMutable([NotNull] IValueFormat other)
    {
      m_DataType = other.DataType;
      m_DateFormat = other.DateFormat;
      m_DateSeparator = other.DateSeparator;
      DecimalSeparatorChar = other.DecimalSeparatorChar;
      m_DisplayNullAs = other.DisplayNullAs;
      m_False = other.False;
      GroupSeparatorChar = other.GroupSeparatorChar;
      m_NumberFormat = other.NumberFormat;
      m_TimeSeparator = other.TimeSeparator;
      m_True = other.True;

      DecimalSeparator = other.DecimalSeparatorChar.ToString();
      GroupSeparator = other.GroupSeparatorChar.ToString();
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="ValueFormatMutable" /> class.
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    public ValueFormatMutable(DataType dataType) => m_DataType = dataType;

    /// <summary>
    ///   Gets or sets the date format.
    /// </summary>
    /// <value>The date format.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cDateFormatDefault)]
    public string DateFormat
    {
      [NotNull]
      get => m_DateFormat;
      [CanBeNull]
      set
      {
        var newVal = value ?? string.Empty;
        if (m_DateFormat.Equals(newVal, StringComparison.Ordinal))
          return;
        m_DateFormat = newVal;
        NotifyPropertyChanged(nameof(DateFormat));
      }
    }

    [UsedImplicitly]
    public  bool DateFormatSpecified => DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the date separator.
    /// </summary>
    /// <value>The date separator.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cDateSeparatorDefault)]
    public string DateSeparator
    {
      [NotNull]
      get => m_DateSeparator;
      [CanBeNull]
      set
      {
        // Translate written punctuation into a character
        var chr = FileFormat.GetChar(value);
        var newVal = chr != '\0' ? chr.ToString(CultureInfo.CurrentCulture) : string.Empty;
        if (m_DateSeparator.Equals(newVal, StringComparison.Ordinal))
          return;
        m_DateSeparator = newVal;
        NotifyPropertyChanged(nameof(DateSeparator));
      }
    }

    [UsedImplicitly]
    public bool DateSeparatorSpecified => m_DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the decimal separator.
    /// </summary>
    /// <value>The decimal separator.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cDecimalSeparatorDefault)]
    public string DecimalSeparator
    {
      [NotNull]
      get => m_DecimalSeparator;

      set
      {
        // Translate written punctuation into a character
        var chr = FileFormat.GetChar(value);
        var newVal = chr != '\0' ? chr.ToString(CultureInfo.CurrentCulture) : string.Empty;
        if (m_DecimalSeparator.Equals(newVal, StringComparison.Ordinal))
          return;
        // If we set the DecimalSeparator to be the Group separator, store the old DecimalSeparator
        // in the group separator;
        if (m_GroupSeparator.Equals(newVal, StringComparison.Ordinal))
        {
          m_GroupSeparator = m_DecimalSeparator;
          GroupSeparatorChar = m_GroupSeparator.GetFirstChar();
          NotifyPropertyChanged(nameof(GroupSeparator));
        }

        m_DecimalSeparator = newVal;
        DecimalSeparatorChar = m_DecimalSeparator.GetFirstChar();
        NotifyPropertyChanged(nameof(DecimalSeparator));
      }
    }

    [UsedImplicitly]
    public bool DecimalSeparatorSpecified => DataType == DataType.Double || DataType == DataType.Numeric;


    /// <summary>
    ///   Gets or sets the representation for false.
    /// </summary>
    /// <value>The false.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cFalseDefault)]
    public string False
    {
      [NotNull]
      get => m_False;
      [CanBeNull]
      set
      {
        var newVal = value ?? string.Empty;
        if (m_False.Equals(newVal, StringComparison.Ordinal))
          return;
        m_False = newVal;
        NotifyPropertyChanged(nameof(False));
      }
    }
    
    [UsedImplicitly] 
    public bool FalseSpecified => m_DataType == DataType.Boolean;

    /// <summary>
    ///   Gets or sets the group separator.
    /// </summary>
    /// <value>The group separator.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cGroupSeparatorDefault)]
    public string GroupSeparator
    {
      [NotNull]
      get => m_GroupSeparator;
      set
      {
        var chr = FileFormat.GetChar(value);
        var newVal = chr != '\0' ? chr.ToString(CultureInfo.CurrentCulture) : string.Empty;

        if (m_GroupSeparator.Equals(newVal, StringComparison.Ordinal))
          return;
        // If we set the DecimalSeparator to be the group separator, store the old DecimalSeparator
        // in the group separator;
        if (m_DecimalSeparator.Equals(newVal, StringComparison.Ordinal))
        {
          m_DecimalSeparator = m_GroupSeparator;
          DecimalSeparatorChar = m_DecimalSeparator.GetFirstChar();
          NotifyPropertyChanged(nameof(DecimalSeparator));
        }

        m_GroupSeparator = newVal;
        GroupSeparatorChar = m_GroupSeparator.GetFirstChar();
        NotifyPropertyChanged(nameof(GroupSeparator));
      }
    }

    [UsedImplicitly]
    public bool GroupSeparatorSpecified => DataType == DataType.Double || DataType == DataType.Numeric || DataType == DataType.Integer;

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(ValueFormatMutable other) => EqualsByInterface(other);

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    [XmlIgnore] public char GroupSeparatorChar { get; private set; } = '\0';

    [XmlIgnore]
    public char DecimalSeparatorChar { get; private set; } = ValueFormatExtension.cDecimalSeparatorDefault[0];

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
    ///   Writing data you can specify how a NULL value should be written, commonly its empty, in
    ///   some circumstances you might want to have n/a etc.
    /// </summary>
    /// <value>Text used if the value is NULL</value>
    [XmlAttribute]
    [DefaultValue("")]
    public string DisplayNullAs
    {
      [NotNull]
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
    ///   Gets or sets the number format.
    /// </summary>
    /// <value>The number format.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cNumberFormatDefault)]
    public string NumberFormat
    {
      [NotNull]
      get => m_NumberFormat;
      [CanBeNull]
      set
      {
        var newVal = value ?? string.Empty;
        if (m_NumberFormat.Equals(newVal, StringComparison.Ordinal))
          return;
        m_NumberFormat = newVal;
        NotifyPropertyChanged(nameof(NumberFormat));
      }
    }

    [UsedImplicitly]
    public bool NumberFormatSpecified =>  DataType == DataType.Double || DataType == DataType.Numeric;

    /// <summary>
    ///   Gets or sets the time separator.
    /// </summary>
    /// <value>The time separator.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cTimeSeparatorDefault)]
    public string TimeSeparator
    {
      [NotNull]
      get => m_TimeSeparator;
      set
      {
        var chr = FileFormat.GetChar(value);
        var newVal = chr != '\0' ? chr.ToString(CultureInfo.CurrentCulture) : string.Empty;
        if (m_TimeSeparator.Equals(newVal, StringComparison.Ordinal))
          return;
        m_TimeSeparator = newVal;
        NotifyPropertyChanged(nameof(TimeSeparator));
      }
    }

    [UsedImplicitly]
    public bool TimeSeparatorSpecified => m_DataType == DataType.DateTime;

    /// <summary>
    ///   Gets or sets the representation for true.
    /// </summary>
    /// <value>The true.</value>
    [XmlElement]
    [DefaultValue(ValueFormatExtension.cTrueDefault)]
    public string True
    {
      [NotNull]
      get => m_True;
      [CanBeNull]
      set
      {
        var newVal = value ?? string.Empty;
        if (m_True.Equals(newVal, StringComparison.Ordinal))
          return;
        m_True = newVal;
        NotifyPropertyChanged(nameof(True));
      }
    }

    [UsedImplicitly]
    public bool TrueSpecified => m_DataType == DataType.Boolean;

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    [NotNull]
    public ValueFormatMutable Clone()
    {
      var other = new ValueFormatMutable();
      CopyTo(other);
      return other;
    }

    public override bool Equals(object other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;

      return other is IValueFormat valueFormat && EqualsByInterface(valueFormat);
    }

    private bool EqualsByInterface(IValueFormat other)
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

    public void CopyFrom(IValueFormat other)
    {
      if (other == null)
        return;
      m_DataType = other.DataType;
      DisplayNullAs = other.DisplayNullAs;
      switch (m_DataType)
      {
        case DataType.Integer:
          NumberFormat = other.NumberFormat;
          break;

        case DataType.Numeric:
        case DataType.Double:
          GroupSeparatorChar = other.GroupSeparatorChar;
          DecimalSeparatorChar = other.DecimalSeparatorChar;

          m_GroupSeparator = FileFormat.GetDescription(other.GroupSeparatorChar.ToString());
          DecimalSeparator = FileFormat.GetDescription(other.DecimalSeparatorChar.ToString());
          NumberFormat = other.NumberFormat;
          break;

        case DataType.DateTime:
          DateFormat = other.DateFormat;
          DateSeparator = other.DateSeparator;
          TimeSeparator = other.TimeSeparator;
          break;

        case DataType.Boolean:
          False = other.False;
          True = other.True;
          break;

        case DataType.Guid:
          break;

        case DataType.String:
          break;

        case DataType.TextToHtml:
          break;

        case DataType.TextToHtmlFull:
          break;

        case DataType.TextPart:
          break;
      }
    }

    /// <summary>
    ///   Copies to.
    /// </summary>
    /// <param name="other">The other.</param>
    public void CopyTo(ValueFormatMutable other)
    {
      if (other == null)
        return;
      other.DataType = m_DataType;
      switch (m_DataType)
      {
        case DataType.Integer:
          other.NumberFormat = m_NumberFormat;
          break;

        case DataType.Numeric:
        case DataType.Double:
          other.GroupSeparator = m_GroupSeparator;
          other.DecimalSeparator = m_DecimalSeparator;
          other.NumberFormat = m_NumberFormat;
          break;

        case DataType.DateTime:
          other.DateFormat = m_DateFormat;
          other.DateSeparator = m_DateSeparator;
          other.TimeSeparator = m_TimeSeparator;
          break;

        case DataType.Boolean:
          other.False = m_False;
          other.True = m_True;
          break;

        default:
          other.DateFormat = m_DateFormat;
          other.DateSeparator = m_DateSeparator;
          other.TimeSeparator = m_TimeSeparator;
          other.False = m_False;
          other.True = m_True;
          other.GroupSeparator = m_GroupSeparator;
          other.DecimalSeparator = m_DecimalSeparator;
          other.NumberFormat = m_NumberFormat;
          break;
      }
    }

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public void NotifyPropertyChanged(string info) =>
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

    /// <summary>
    ///   Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (int) m_DataType;
        hashCode = (hashCode * 397) ^ StringComparer.Ordinal.GetHashCode(m_DisplayNullAs);
        switch (m_DataType)
        {
          case DataType.Integer:
            hashCode = (hashCode * 397) ^ StringComparer.Ordinal.GetHashCode(m_NumberFormat);
            break;

          case DataType.Numeric:
          case DataType.Double:
            hashCode = (hashCode * 397) ^ m_GroupSeparator.GetHashCode();
            hashCode = (hashCode * 397) ^ m_DecimalSeparator.GetHashCode();
            hashCode = (hashCode * 397) ^ m_NumberFormat.GetHashCode();
            break;

          case DataType.DateTime:
            hashCode = (hashCode * 397) ^ m_DateFormat.GetHashCode();
            hashCode = (hashCode * 397) ^ m_DateSeparator.GetHashCode();
            hashCode = (hashCode * 397) ^ m_TimeSeparator.GetHashCode();
            break;

          case DataType.Boolean:
            hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_False);
            hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_True);
            break;

          default:
            hashCode = (hashCode * 397) ^ m_DateFormat.GetHashCode();
            hashCode = (hashCode * 397) ^ m_DateSeparator.GetHashCode();
            hashCode = (hashCode * 397) ^ m_TimeSeparator.GetHashCode();
            hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_False);
            hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_True);
            hashCode = (hashCode * 397) ^ m_TimeSeparator.GetHashCode();
            hashCode = (hashCode * 397) ^ m_GroupSeparator.GetHashCode();
            hashCode = (hashCode * 397) ^ m_DecimalSeparator.GetHashCode();
            hashCode = (hashCode * 397) ^ m_NumberFormat.GetHashCode();
            hashCode = (hashCode * 397) ^ StringComparer.Ordinal.GetHashCode(m_NumberFormat);
            break;
        }

        return hashCode;
      }
    }
  }
}