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

#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

#endregion

namespace CsvTools
{
  /// <summary>
  ///   Setting for a value format
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{DebuggerDisplay,nq}")]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class ValueFormat : ICloneable<ValueFormat>, IEquatable<ValueFormat>, INotifyPropertyChanged
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    /// <summary>
    ///   The default date format
    /// </summary>
    public const string cDateFormatDefault = "MM/dd/yyyy";

    public const string cDateSeparatorDefault = "/";

    public const string cDecimalSeparatorDefault = ".";

    public const string cFalseDefault = "False";

    public const string cGroupSeparatorDefault = "";

    public const string cNumberFormatDefault = "0.#####";

    public const string cTimeSeparatorDefault = ":";

    public const string cTrueDefault = "True";

    private DataType m_DataType = DataType.String;

    private string m_DateFormat = cDateFormatDefault;

    private string m_DateSeparator = cDateSeparatorDefault;

    private string m_DecimalSeparator = cDecimalSeparatorDefault;

    private string m_False = cFalseDefault;

    private string m_GroupSeparator = cGroupSeparatorDefault;

    private string m_NumberFormat = cNumberFormatDefault;

    private string m_TimeSeparator = cTimeSeparatorDefault;

    private string m_True = cTrueDefault;

    /// <summary>
    ///   Initializes a new instance of the <see cref="ValueFormat" /> class.
    /// </summary>
    public ValueFormat()
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="ValueFormat" /> class.
    /// </summary>
    /// <param name="dataType">Type of the data.</param>
    public ValueFormat(DataType dataType) => m_DataType = dataType;

    /// <summary>
    ///   Gets or sets the type of the data.
    /// </summary>
    /// <value>The type of the data.</value>
    [XmlAttribute]
    [DefaultValue(DataType.String)]
    public virtual DataType DataType
    {
      get => m_DataType;

      set
      {
        if (m_DataType.Equals(value)) return;
        m_DataType = value;
        NotifyPropertyChanged(nameof(DataType));
      }
    }

    /// <summary>
    ///   Gets or sets the date format.
    /// </summary>
    /// <value>The date format.</value>
    [XmlElement]
    [DefaultValue(cDateFormatDefault)]
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
    ///   Gets or sets the date separator.
    /// </summary>
    /// <value>The date separator.</value>
    [XmlElement]
    [DefaultValue(cDateSeparatorDefault)]
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
    ///   Gets or sets the decimal separator.
    /// </summary>
    /// <value>The decimal separator.</value>
    [XmlElement]
    [DefaultValue(cDecimalSeparatorDefault)]
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
        NotifyPropertyChanged(nameof(DecimalSeparator));
      }
    }

    /// <summary>
    ///   Gets or sets the representation for false.
    /// </summary>
    /// <value>The false.</value>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "False")]
    [XmlElement]
    [DefaultValue(cFalseDefault)]
    public virtual string False
    {
      get => m_False;

      set
      {
        var newVal = value ?? string.Empty;
        if (m_False.Equals(newVal, StringComparison.Ordinal)) return;
        m_False = newVal;
        NotifyPropertyChanged(nameof(False));
      }
    }

    /// <summary>
    ///   Gets or sets the group separator.
    /// </summary>
    /// <value>The group separator.</value>
    [XmlElement]
    [DefaultValue(cGroupSeparatorDefault)]
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
        NotifyPropertyChanged(nameof(GroupSeparator));
      }
    }

    /// <summary>
    ///   Gets or sets the number format.
    /// </summary>
    /// <value>The number format.</value>
    [XmlElement]
    [DefaultValue(cNumberFormatDefault)]
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
    ///   Gets or sets the time separator.
    /// </summary>
    /// <value>The time separator.</value>
    [XmlElement]
    [DefaultValue(cTimeSeparatorDefault)]
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
    ///   Gets or sets the representation for true.
    /// </summary>
    /// <value>The true.</value>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "True")]
    [XmlElement]
    [DefaultValue(cTrueDefault)]
    public virtual string True
    {
      get => m_True;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_True.Equals(newVal, StringComparison.Ordinal)) return;
        m_True = newVal;
        NotifyPropertyChanged(nameof(True));
      }
    }

    private string DebuggerDisplay => GetTypeAndFormatDescription();

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public virtual ValueFormat Clone()
    {
      var other = new ValueFormat();
      CopyTo(other);
      return other;
    }

    /// <summary>
    ///   Copies to.
    /// </summary>
    /// <param name="other">The other.</param>
    public virtual void CopyTo(ValueFormat other)
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

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public bool Equals(ValueFormat other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;

      if (other.DataType != m_DataType) return false;

      switch (m_DataType)
      {
        case DataType.Integer:
          return string.Equals(other.NumberFormat, m_NumberFormat, StringComparison.Ordinal);

        case DataType.Numeric:
        case DataType.Double:
          return string.Equals(other.GroupSeparator, m_GroupSeparator, StringComparison.Ordinal) &&
                 string.Equals(other.DecimalSeparator, m_DecimalSeparator, StringComparison.Ordinal) &&
                 string.Equals(other.NumberFormat, m_NumberFormat, StringComparison.Ordinal);

        case DataType.DateTime:
          return string.Equals(other.DateFormat, m_DateFormat, StringComparison.Ordinal) &&
                 string.Equals(other.DateSeparator, m_DateSeparator, StringComparison.Ordinal) &&
                 string.Equals(other.TimeSeparator, m_TimeSeparator, StringComparison.Ordinal);

        case DataType.Boolean:
          return string.Equals(other.False, m_False, StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(other.True, m_True, StringComparison.OrdinalIgnoreCase);

        default:
          // compare everything
          return string.Equals(other.DateFormat, m_DateFormat, StringComparison.Ordinal) &&
                 string.Equals(other.DateSeparator, m_DateSeparator, StringComparison.Ordinal) &&
                 string.Equals(other.TimeSeparator, m_TimeSeparator, StringComparison.Ordinal) &&
                 string.Equals(other.False, m_False, StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(other.True, m_True, StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(other.GroupSeparator, m_GroupSeparator, StringComparison.Ordinal) &&
                 string.Equals(other.DecimalSeparator, m_DecimalSeparator, StringComparison.Ordinal) &&
                 string.Equals(other.NumberFormat, m_NumberFormat, StringComparison.Ordinal);
      }
    }

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Gets the a description of the Date or Number format
    /// </summary>
    /// <returns></returns>
    public string GetFormatDescription()
    {
      switch (m_DataType)
      {
        case DataType.DateTime:
          return m_DateFormat.ReplaceDefaults(CultureInfo.InvariantCulture.DateTimeFormat.DateSeparator,
            m_DateSeparator,
            CultureInfo.InvariantCulture.DateTimeFormat.TimeSeparator, m_TimeSeparator);

        case DataType.Numeric:
        case DataType.Double:
          return m_NumberFormat.ReplaceDefaults(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator,
            m_DecimalSeparator,
            CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator, m_GroupSeparator);

        default:
          return string.Empty;
      }
    }

    /// <summary>
    ///   Gets the description.
    /// </summary>
    /// <returns></returns>
    public string GetTypeAndFormatDescription()
    {
      var sbtext = new StringBuilder(m_DataType.DataTypeDisplay());

      var shortDesc = GetFormatDescription();
      if (shortDesc.Length <= 0) return sbtext.ToString();
      sbtext.Append(" (");
      sbtext.Append(shortDesc);
      sbtext.Append(")");

      return sbtext.ToString();
    }

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public virtual void NotifyPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object obj)
    {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      return (obj is ValueFormat typed) && Equals(typed);
    }

    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (int)m_DataType;

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