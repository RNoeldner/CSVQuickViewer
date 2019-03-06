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
  [DebuggerDisplay("Mapping(File/Source: {m_FileColumn} -> Template/Destination {m_TemplateField})")]
  [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class Mapping : IEquatable<Mapping>, ICloneable<Mapping>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    private bool m_Attention;
    private string m_FileColumn = string.Empty;
    private string m_TemplateField = string.Empty;
    private bool m_Update;

    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="Mapping" /> should be used for update
    /// </summary>
    /// <value>
    ///   <c>true</c> if it should be regarded for updates; otherwise, <c>false</c>.
    /// </value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool Update
    {
      get => m_Update;
      set => m_Update = value;
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="Mapping" /> required additional attention
    /// </summary>
    /// <value>
    ///   <c>true</c> if attention; otherwise, <c>false</c>.
    /// </value>
    [XmlAttribute]
    [DefaultValue(false)]
    public virtual bool Attention
    {
      get => m_Attention;
      set => m_Attention = value;
    }

    /// <summary>
    ///   Gets or sets the Source := File Column.
    /// </summary>
    /// <value>The source.</value>
    [XmlAttribute("Column")]
    public string FileColumn
    {
      get
      {
        Contract.Ensures(Contract.Result<string>() != null);
        return m_FileColumn;
      }
      set => m_FileColumn = value ?? string.Empty;
    }

    /// <summary>
    ///   Gets or sets the := Template Column.
    /// </summary>
    /// <value>The destination.</value>
    [XmlAttribute("Field")]
    [DefaultValue("")]
    public virtual string TemplateField
    {
      get
      {
        Contract.Ensures(Contract.Result<string>() != null);
        return m_TemplateField;
      }
      set => m_TemplateField = value ?? string.Empty;
    }

    /// <summary>
    ///   Clones this instance.
    /// </summary>
    /// <returns>A new FieldMapping that is a copy </returns>
    public Mapping Clone()
    {
      var c = new Mapping();
      CopyTo(c);
      return c;
    }

    /// <summary>
    ///   Copies all properties to the other instance
    /// </summary>
    /// <param name="other">The other instance</param>
    public void CopyTo(Mapping other)
    {
      if (other == null)
        return;
      other.Attention = m_Attention;
      other.Update = m_Update;
      other.TemplateField = m_TemplateField;
      other.FileColumn = m_FileColumn;
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public bool Equals(Mapping other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;
      return m_Attention == other.m_Attention && m_Update == other.m_Update &&
             string.Equals(m_FileColumn, other.m_FileColumn, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(m_TemplateField, other.m_TemplateField, StringComparison.OrdinalIgnoreCase);
    }

    [ContractInvariantMethod]
    private void ObjectInvariant()
    {
      Contract.Invariant(m_FileColumn != null);
      Contract.Invariant(m_TemplateField != null);
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
      return (obj is Mapping typed) && Equals(typed);
    }

    /*
    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = m_Attention.GetHashCode();
        hashCode = (hashCode * 397) ^ m_Update.GetHashCode();
        hashCode = (hashCode * 397) ^
                   (m_FileColumn != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(m_FileColumn) : 0);
        hashCode = (hashCode * 397) ^
                   (m_TemplateField != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(m_TemplateField) : 0);
        return hashCode;
      }
    }
    */
  }
}