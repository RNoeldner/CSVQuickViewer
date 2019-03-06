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
using System.Diagnostics.Contracts;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   Settings how the typed values should be determined
  /// </summary>
  [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class FillGuessSettings : INotifyPropertyChanged, ICloneable<FillGuessSettings>, IEquatable<FillGuessSettings>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    private int m_CheckedRecords = 30000;
    private bool m_CheckNamedDates = true;
    private bool m_DateParts;    
    private bool m_DectectNumbers = true;
    private bool m_DectectPercentage = true;
    private bool m_DetectBoolean = true;
    private bool m_DetectDateTime = true;
    private bool m_DetectGuid;
    private string m_FalseValue = "False";
    private bool m_IgnoreIdColums = true;
    private int m_MinSamplesForIntDate = 5;
    private int m_SampleValues = 200;
    private bool m_SerialDateTime = true;
    private string m_TrueValue = "True";

    /// <summary>
    ///   Number of records to parse to et the sample values
    /// </summary>
    [XmlAttribute]
    [DefaultValue(30000)]
    public virtual int CheckedRecords
    {
      get => m_CheckedRecords;

      set
      {
        if (m_CheckedRecords == value) return;
        m_CheckedRecords = value;
        NotifyPropertyChanged(nameof(CheckedRecords));
      }
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be Date or Times
    /// </summary>
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool CheckNamedDates
    {
      get => m_CheckNamedDates;

      set
      {
        if (m_CheckNamedDates == value) return;
        m_CheckNamedDates = value;
        NotifyPropertyChanged(nameof(CheckNamedDates));
      }
    }
   

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be Numeric
    /// </summary>
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool DectectNumbers
    {
      get => m_DectectNumbers;

      set
      {
        if (m_DectectNumbers == value) return;
        m_DectectNumbers = value;
        NotifyPropertyChanged(nameof(DectectNumbers));
      }
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be Percentages
    /// </summary>
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool DectectPercentage
    {
      get => m_DectectPercentage;

      set
      {
        if (m_DectectPercentage == value) return;
        m_DectectPercentage = value;
        NotifyPropertyChanged(nameof(DectectPercentage));
      }
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be Boolean
    /// </summary>
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool DetectBoolean
    {
      get => m_DetectBoolean;

      set
      {
        if (m_DetectBoolean == value) return;
        m_DetectBoolean = value;
        NotifyPropertyChanged(nameof(DetectBoolean));
      }
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be Date or Times
    /// </summary>
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool DetectDateTime
    {
      get => m_DetectDateTime;

      set
      {
        if (m_DetectDateTime == value) return;
        m_DetectDateTime = value;
        NotifyPropertyChanged(nameof(DetectDateTime));
      }
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be GUIDs
    /// </summary>
    [DefaultValue(false)]
    [XmlElement]
    public virtual bool DetectGUID
    {
      get => m_DetectGuid;

      set
      {
        if (m_DetectGuid == value) return;
        m_DetectGuid = value;
        NotifyPropertyChanged(nameof(DetectGUID));
      }
    }

    /// <summary>
    ///   List of text to be regarded as <c>false</c>
    /// </summary>
    [DefaultValue("False")]
    [XmlElement]
    public virtual string FalseValue
    {
      get
      {
        Contract.Ensures(Contract.Result<string>() != null);
        Contract.Assume(m_FalseValue != null);
        return m_FalseValue;
      }

      set
      {
        Contract.Ensures(m_FalseValue != null);
        Contract.Assume(m_FalseValue != null);

        var newVal = value ?? string.Empty;
        if (m_FalseValue.Equals(newVal, StringComparison.Ordinal)) return;
        m_FalseValue = newVal;
        NotifyPropertyChanged(nameof(FalseValue));
      }
    }

    /// <summary>
    ///   Flag to ignore columns that seem to be Identifiers
    /// </summary>
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool IgnoreIdColums
    {
      get => m_IgnoreIdColums;

      set
      {
        if (m_IgnoreIdColums == value) return;
        m_IgnoreIdColums = value;
        NotifyPropertyChanged(nameof(IgnoreIdColums));
      }
    }

    /// <summary>
    ///   Number of sample values
    /// </summary>
    [DefaultValue(5)]
    [XmlAttribute]
    public virtual int MinSamplesForIntDate
    {
      get => m_MinSamplesForIntDate;

      set
      {
        if (m_MinSamplesForIntDate == value || value <= 0 || value >= m_SampleValues) return;
        m_MinSamplesForIntDate = value;
        NotifyPropertyChanged(nameof(MinSamplesForIntDate));
      }
    }

    /// <summary>
    ///   Number of sample values
    /// </summary>
    [DefaultValue(200)]
    [XmlAttribute]
    public virtual int SampleValues
    {
      get => m_SampleValues;

      set
      {
        if (m_SampleValues == value || value <= 0 || value <= m_MinSamplesForIntDate) return;
        m_SampleValues = value;
        NotifyPropertyChanged(nameof(SampleValues));
      }
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be serial Date or Times
    /// </summary>
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool SerialDateTime
    {
      get => m_SerialDateTime;

      set
      {
        if (m_SerialDateTime == value) return;
        m_SerialDateTime = value;
        NotifyPropertyChanged(nameof(SerialDateTime));
      }
    }

    /// <summary>
    ///   List of text to be regarded as <c>true</c>
    /// </summary>
    [DefaultValue("True")]
    [XmlElement]
    public virtual string TrueValue
    {
      get
      {
        Contract.Ensures(Contract.Result<string>() != null);
        Contract.Assume(m_TrueValue != null);
        return m_TrueValue;
      }

      set
      {
        Contract.Ensures(m_TrueValue != null);
        Contract.Assume(m_TrueValue != null);

        var newVal = value ?? string.Empty;
        if (m_TrueValue.Equals(newVal, StringComparison.Ordinal)) return;
        m_TrueValue = newVal;
        NotifyPropertyChanged(nameof(TrueValue));
      }
    }

    [DefaultValue(false)]
    [XmlElement]
    public bool DateParts
    {
      get => m_DateParts;

      set => m_DateParts = value;
    }

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public virtual FillGuessSettings Clone()
    {
      Contract.Ensures(Contract.Result<FillGuessSettings>() != null);
      var other = new FillGuessSettings();
      CopyTo(other);
      return other;
    }

    /// <summary>
    ///   Copy all properties to another instance of FillGuessSettings
    /// </summary>
    /// <param name="other"></param>
    public virtual void CopyTo(FillGuessSettings other)
    {
      if (other == null)
        return;

      other.CheckedRecords = m_CheckedRecords;
      other.CheckNamedDates = m_CheckNamedDates;      
      other.DectectNumbers = m_DectectNumbers;
      other.DectectPercentage = m_DectectPercentage;
      other.DetectBoolean = m_DetectBoolean;
      other.DateParts = m_DateParts;
      other.DetectDateTime = m_DetectDateTime;
      other.DetectGUID = m_DetectGuid;
      other.FalseValue = m_FalseValue;
      other.IgnoreIdColums = m_IgnoreIdColums;
      other.MinSamplesForIntDate = m_MinSamplesForIntDate;
      other.SampleValues = m_SampleValues;
      other.SerialDateTime = m_SerialDateTime;
      other.TrueValue = m_TrueValue;
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public bool Equals(FillGuessSettings other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;
      return m_CheckedRecords == other.m_CheckedRecords && m_CheckNamedDates == other.m_CheckNamedDates &&
             m_DateParts == other.m_DateParts &&             
             m_DectectNumbers == other.m_DectectNumbers && m_DectectPercentage == other.m_DectectPercentage &&
             m_DetectBoolean == other.m_DetectBoolean && m_DetectDateTime == other.m_DetectDateTime &&
             m_DetectGuid == other.m_DetectGuid &&
             string.Equals(m_FalseValue, other.m_FalseValue, StringComparison.OrdinalIgnoreCase) &&
             m_IgnoreIdColums == other.m_IgnoreIdColums && m_MinSamplesForIntDate == other.m_MinSamplesForIntDate &&
             m_SampleValues == other.m_SampleValues && m_SerialDateTime == other.m_SerialDateTime &&
             string.Equals(m_TrueValue, other.m_TrueValue, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

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
      return (obj is FillGuessSettings typed) && Equals(typed);
    }

    /*
    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = m_CheckedRecords;
        hashCode = (hashCode * 397) ^ m_CheckNamedDates.GetHashCode();
        hashCode = (hashCode * 397) ^ m_DateParts.GetHashCode();
        hashCode = (hashCode * 397) ^ (m_DateTimeValue != null ? m_DateTimeValue.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ m_DectectNumbers.GetHashCode();
        hashCode = (hashCode * 397) ^ m_DectectPercentage.GetHashCode();
        hashCode = (hashCode * 397) ^ m_DetectBoolean.GetHashCode();
        hashCode = (hashCode * 397) ^ m_DetectDateTime.GetHashCode();
        hashCode = (hashCode * 397) ^ m_DetectGuid.GetHashCode();
        hashCode = (hashCode * 397) ^
                   (m_FalseValue != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(m_FalseValue) : 0);
        hashCode = (hashCode * 397) ^ m_IgnoreIdColums.GetHashCode();
        hashCode = (hashCode * 397) ^ m_MinSamplesForIntDate;
        hashCode = (hashCode * 397) ^ m_SampleValues;
        hashCode = (hashCode * 397) ^ m_SerialDateTime.GetHashCode();
        hashCode = (hashCode * 397) ^
                   (m_TrueValue != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(m_TrueValue) : 0);
        return hashCode;
      }
    }
    */
  }
}