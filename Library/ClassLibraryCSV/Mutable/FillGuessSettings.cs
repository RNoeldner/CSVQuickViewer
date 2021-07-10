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
  ///   Settings how the typed values should be determined
  /// </summary>
  [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class FillGuessSettings : INotifyPropertyChanged, ICloneable<FillGuessSettings>, IEquatable<FillGuessSettings>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    private long m_CheckedRecords = 30000;
    private bool m_CheckNamedDates = true;
    private bool m_DetectNumbers = true;
    private bool m_DetectPercentage = true;
    private bool m_DetectBoolean = true;
    private bool m_DetectDateTime = true;
    private bool m_DetectGuid;
    private string m_FalseValue = "False";
    private bool m_IgnoreIdColumns = true;
    private int m_MinSamples = 5;
    private int m_SampleValues = 150;
    private bool m_SerialDateTime = true;
    private string m_TrueValue = "True";
    private bool m_Enabled = true;

    /// <summary>
    ///   Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///   Number of records to parse to get the sample values, default is <c>30000</c>
    /// </summary>
    [XmlAttribute]
    [DefaultValue(30000)]
    public virtual long CheckedRecords
    {
      get => m_CheckedRecords;

      set
      {
        if (m_CheckedRecords == value)
          return;
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
        if (m_CheckNamedDates == value)
          return;
        m_CheckNamedDates = value;
        NotifyPropertyChanged(nameof(CheckNamedDates));
      }
    }

    [DefaultValue(true)]
    [XmlElement]
    public bool Enabled
    {
      get => m_Enabled; set
      {
        if (m_Enabled == value)
          return;
        m_Enabled = value;
        NotifyPropertyChanged(nameof(Enabled));
      }
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they have a date part, default is <c>false</c>
    /// </summary>
    [DefaultValue(false)]
    [XmlElement]
    public bool DateParts { get; set; }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be Numeric, default is <c>True</c>
    /// </summary>
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool DetectNumbers
    {
      get => m_DetectNumbers;

      set
      {
        if (m_DetectNumbers == value)
          return;
        m_DetectNumbers = value;
        NotifyPropertyChanged(nameof(DetectNumbers));
      }
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be Percentages, default is <c>True</c>
    /// </summary>
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool DetectPercentage
    {
      get => m_DetectPercentage;

      set
      {
        if (m_DetectPercentage == value)
          return;
        m_DetectPercentage = value;
        NotifyPropertyChanged(nameof(DetectPercentage));
      }
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be Boolean, default is <c>True</c>
    /// </summary>
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool DetectBoolean
    {
      get => m_DetectBoolean;

      set
      {
        if (m_DetectBoolean == value)
          return;
        m_DetectBoolean = value;
        NotifyPropertyChanged(nameof(DetectBoolean));
      }
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be Date or Times, default is <c>True</c>
    /// </summary>
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool DetectDateTime
    {
      get => m_DetectDateTime;

      set
      {
        if (m_DetectDateTime == value)
          return;
        m_DetectDateTime = value;
        NotifyPropertyChanged(nameof(DetectDateTime));
      }
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be GUIDs, default is <c>False</c>
    /// </summary>
    [DefaultValue(false)]
    [XmlElement]
    public virtual bool DetectGUID
    {
      get => m_DetectGuid;

      set
      {
        if (m_DetectGuid == value)
          return;
        m_DetectGuid = value;
        NotifyPropertyChanged(nameof(DetectGUID));
      }
    }

    /// <summary>
    ///   List of text to be regarded as <c>false</c>, default text is <c>"False"</c>
    /// </summary>
    [DefaultValue("False")]
    [XmlElement]
    public virtual string FalseValue
    {
      get => m_FalseValue;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_FalseValue.Equals(newVal, StringComparison.Ordinal))
          return;
        m_FalseValue = newVal;
        NotifyPropertyChanged(nameof(FalseValue));
      }
    }

    /// <summary>
    ///   Flag to ignore columns that seem to be Identifiers, default is <c>True</c>
    /// </summary>
    [DefaultValue(true)]
    [XmlElement]
    public virtual bool IgnoreIdColumns
    {
      get => m_IgnoreIdColumns;
      set
      {
        if (m_IgnoreIdColumns == value)
          return;
        m_IgnoreIdColumns = value;
        NotifyPropertyChanged(nameof(IgnoreIdColumns));
      }
    }

    /// <summary>
    ///   Number of sample values, default is <c>5</c>
    /// </summary>
    [DefaultValue(5)]
    [XmlAttribute]
    public virtual int MinSamples
    {
      get => m_MinSamples;

      set
      {
        if (m_MinSamples == value || value <= 0 || value >= m_SampleValues)
          return;
        m_MinSamples = value;
        NotifyPropertyChanged(nameof(MinSamples));
      }
    }

    /// <summary>
    ///   Number of sample values, default is <c>150</c>
    /// </summary>
    [DefaultValue(150)]
    [XmlAttribute]
    public virtual int SampleValues
    {
      get => m_SampleValues;

      set
      {
        if (m_SampleValues == value || value <= 0 || value <= m_MinSamples)
          return;
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
        if (m_SerialDateTime == value)
          return;
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
      get => m_TrueValue;
      set
      {
        var newVal = value ?? string.Empty;
        if (m_TrueValue.Equals(newVal, StringComparison.Ordinal))
          return;
        m_TrueValue = newVal;
        NotifyPropertyChanged(nameof(TrueValue));
      }
    }

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public virtual FillGuessSettings Clone()
    {
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
      other.CheckedRecords = CheckedRecords;
      other.CheckNamedDates = CheckNamedDates;
      other.DetectNumbers = DetectNumbers;
      other.DetectPercentage = DetectPercentage;
      other.DetectBoolean = DetectBoolean;
      other.DateParts = DateParts;
      other.DetectDateTime = DetectDateTime;
      other.DetectGUID = DetectGUID;
      other.FalseValue = FalseValue;
      other.IgnoreIdColumns = IgnoreIdColumns;
      other.MinSamples = MinSamples;
      other.SampleValues = SampleValues;
      other.SerialDateTime = SerialDateTime;
      other.TrueValue = TrueValue;
    }

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(FillGuessSettings? other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Enabled == other.Enabled && CheckedRecords == other.CheckedRecords && CheckNamedDates == other.CheckNamedDates &&
             DateParts == other.DateParts &&
             m_DetectNumbers == other.m_DetectNumbers && DetectPercentage == other.DetectPercentage &&
             m_DetectBoolean == other.m_DetectBoolean && m_DetectDateTime == other.DetectDateTime &&
             DetectGUID == other.DetectGUID &&
             string.Equals(FalseValue, other.FalseValue, StringComparison.OrdinalIgnoreCase) &&
             IgnoreIdColumns == other.IgnoreIdColumns && MinSamples == other.MinSamples &&
             SampleValues == other.SampleValues && SerialDateTime == other.SerialDateTime &&
             string.Equals(TrueValue, other.TrueValue, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Notifies the property changed.
    /// </summary>
    /// <param name="info">The info.</param>
    public virtual void NotifyPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
  }
}