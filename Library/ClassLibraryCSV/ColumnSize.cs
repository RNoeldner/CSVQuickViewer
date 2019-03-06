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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   Setting to store a Field Mapping
  /// </summary>
  [DebuggerDisplay("ColumnSize {ColumnName} {Size}")]
  [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class ColumnSize : ICloneable<ColumnSize>, IEquatable<ColumnSize>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    private string m_ColumnName = string.Empty;
    private int m_ColumnOrdinal;
    private int m_Size;

    /// <summary>
    ///   Gets or sets the number consecutive empty rows that should finish a read
    /// </summary>
    /// <value>The consecutive empty rows.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string ColumnName
    {
      get
      {
        Contract.Ensures(Contract.Result<string>() != null);
        Contract.Assume(m_ColumnName != null);
        return m_ColumnName;
      }
      set
      {
        Contract.Ensures(m_ColumnName != null);
        Contract.Assume(m_ColumnName != null);
        var newVal = value ?? string.Empty;
        m_ColumnName = newVal;
      }
    }

    /// <summary>
    ///   The Ordinal Position of the column
    /// </summary>
    [XmlAttribute]
    public virtual int ColumnOrdinal
    {
      get => m_ColumnOrdinal;
      set => m_ColumnOrdinal = value;
    }

    /// <summary>
    ///   Gets or sets the number consecutive empty rows that should finish a read
    /// </summary>
    /// <value>The consecutive empty rows.</value>
    [XmlAttribute]
    public virtual int Size
    {
      get => m_Size;

      set => m_Size = value;
    }

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public virtual ColumnSize Clone()
    {
      Contract.Ensures(Contract.Result<ColumnSize>() != null);
      var other = new ColumnSize();
      CopyTo(other);
      return other;
    }

    /// <summary>
    ///   Copies all properties to the other instance
    /// </summary>
    /// <param name="other">The other instance</param>
    public virtual void CopyTo(ColumnSize other)
    {
      if (other == null)
        return;
      other.ColumnName = m_ColumnName;
      other.ColumnOrdinal = m_ColumnOrdinal;
      other.Size = m_Size;
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public bool Equals(ColumnSize other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;
      return string.Equals(m_ColumnName, other.m_ColumnName, StringComparison.OrdinalIgnoreCase) &&
             m_ColumnOrdinal == other.m_ColumnOrdinal && m_Size == other.m_Size;
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
      return (obj is ColumnSize typed) && Equals(typed);
    }

    /*
    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = m_ColumnName != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(m_ColumnName) : 0;
        hashCode = (hashCode * 397) ^ m_ColumnOrdinal;
        hashCode = (hashCode * 397) ^ m_Size;
        return hashCode;
      }
    }
    */
  }
}