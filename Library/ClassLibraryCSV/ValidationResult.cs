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
  ///   Class to store validation result and cache them
  /// </summary>
  [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class ValidationResult : INotifyPropertyChanged, IEquatable<ValidationResult>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    private long m_ErrorCount = -1;
    private long m_FileSize = -1;
    private long m_NumberRecords;
    private string m_TableName = string.Empty;
    private long m_WarningCount = -1;

    /// <summary>
    ///   Gets or sets the error count.
    /// </summary>
    /// <value>
    ///   The error count.
    /// </value>
    [XmlAttribute]
    public long ErrorCount
    {
      get => m_ErrorCount;
      set
      {
        if (m_ErrorCount == value) return;
        m_ErrorCount = value;
        NotifyPropertyChanged(nameof(ErrorCount));
        NotifyPropertyChanged(nameof(ErrorRatio));
      }
    }

    /// <summary>
    ///   Gets the error ratio.
    /// </summary>
    /// <value>
    ///   The error ratio.
    /// </value>
    public double ErrorRatio => NumberRecords > 0 ? (double)ErrorCount / NumberRecords : 0d;

    /// <summary>
    ///   Gets or sets the size of the file.
    /// </summary>
    /// <value>
    ///   The size of the file.
    /// </value>
    [XmlIgnore]
    public long FileSize
    {
      get => m_FileSize;
      set
      {
        if (m_FileSize == value) return;
        m_FileSize = value;
        NotifyPropertyChanged(nameof(FileSize));
        NotifyPropertyChanged(nameof(FileSizeDisplay));
      }
    }

    /// <summary>
    ///   Gets the file size display.
    /// </summary>
    /// <value>
    ///   The file size display.
    /// </value>
    public string FileSizeDisplay => FileSize > 0 ? StringConversion.DynamicStorageSize(FileSize) : string.Empty;

    /// <summary>
    ///   Gets or sets the number records.
    /// </summary>
    /// <value>
    ///   The number records.
    /// </value>
    [XmlAttribute]
    public long NumberRecords
    {
      get => m_NumberRecords;
      set
      {
        if (m_NumberRecords == value) return;
        m_NumberRecords = value;
        NotifyPropertyChanged(nameof(NumberRecords));
        NotifyPropertyChanged(nameof(ErrorRatio));
        NotifyPropertyChanged(nameof(WarningRatio));
      }
    }

    /// <summary>
    ///   Gets or sets the name of the table.
    /// </summary>
    /// <value>
    ///   The name of the table.
    /// </value>
    [XmlIgnore]
    public string TableName
    {
      get => m_TableName;
      set => m_TableName = value ?? string.Empty;
    }

    /// <summary>
    ///   Gets or sets the warning count.
    /// </summary>
    /// <value>
    ///   The warning count.
    /// </value>
    [XmlAttribute]
    public long WarningCount
    {
      get => m_WarningCount;
      set
      {
        if (m_WarningCount == value) return;
        m_WarningCount = value;
        NotifyPropertyChanged(nameof(WarningCount));
        NotifyPropertyChanged(nameof(WarningRatio));
      }
    }

    /// <summary>
    ///   Gets the warning ratio.
    /// </summary>
    /// <value>
    ///   The warning ratio.
    /// </value>
    public double WarningRatio
    {
      get
      {
        if (NumberRecords > 0)
          return (double)WarningCount / NumberRecords;

        return 0;
      }
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public bool Equals(ValidationResult other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;
      return m_ErrorCount == other.m_ErrorCount && m_FileSize == other.m_FileSize &&
             m_NumberRecords == other.m_NumberRecords &&
             string.Equals(m_TableName, other.m_TableName, StringComparison.InvariantCultureIgnoreCase) &&
             m_WarningCount == other.m_WarningCount;
    }

    /// <summary>
    ///   Occurs after a property value changes.
    /// </summary>
    public virtual event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    ///   Notifies the completed property changed.
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
      return (obj is ValidationResult typed) && Equals(typed);
    }

    /*
    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = m_ErrorCount.GetHashCode();
        hashCode = (hashCode * 397) ^ m_FileSize.GetHashCode();
        hashCode = (hashCode * 397) ^ m_NumberRecords.GetHashCode();
        hashCode = (hashCode * 397) ^ StringComparer.InvariantCultureIgnoreCase.GetHashCode(m_TableName);
        hashCode = (hashCode * 397) ^ m_WarningCount.GetHashCode();
        return hashCode;
      }
    }
    */
  }
}