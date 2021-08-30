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
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   Setting to store a Field Mapping
  /// </summary>
  [DebuggerDisplay("Mapping(File/Source: {FileColumn} -> Template/Destination {TemplateField})")]
  [Serializable]
  public sealed class Mapping : IEquatable<Mapping>, ICloneable<Mapping>
  {
    public Mapping()
      : this(string.Empty, string.Empty)
    {
    }

    public Mapping(string fileColumn, string templateField, bool update = false, bool attention = false)
    {
      FileColumn = fileColumn;
      TemplateField = templateField;
      Update = update;
      Attention = attention;
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="Mapping" /> required additional attention
    /// </summary>
    /// <value><c>true</c> if attention; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public bool Attention
    {
      get;
      set;
    }

    /// <summary>
    ///   Gets or sets the Source := File Column.
    /// </summary>
    /// <value>The source.</value>
    /// <remarks>The set operator is only present to allow serialization</remarks>
    [XmlAttribute("Column")]
    public string FileColumn { get; set; }

    /// <summary>
    ///   Gets or sets the := Template Column.
    /// </summary>
    /// <value>The destination.</value>
    /// <remarks>The set operator is only present to allow serialization</remarks>
    [XmlAttribute("Field")]
    public string TemplateField { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="Mapping" /> should be used for update
    /// </summary>
    /// <value><c>true</c> if it should be regarded for updates; otherwise, <c>false</c>.</value>
    [XmlAttribute]
    [DefaultValue(false)]
    public bool Update
    {
      get;
      set;
    }

    /// <summary>
    ///   Clones this instance.
    /// </summary>
    /// <returns>A new FieldMapping that is a copy</returns>
    public Mapping Clone() => new Mapping(FileColumn, TemplateField, Update, Attention);

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(Mapping? other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Attention == other.Attention && Update == other.Update
                                          && string.Equals(
                                            FileColumn,
                                            other.FileColumn,
                                            StringComparison.OrdinalIgnoreCase) && string.Equals(
                                            TemplateField,
                                            other.TemplateField,
                                            StringComparison.OrdinalIgnoreCase);
    }

    /*
        /// <summary>
        ///   Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
          unchecked
          {
            var hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(FileColumn);
            hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(TemplateField);
            hashCode = (hashCode * 397) ^ Update.GetHashCode();
            hashCode = (hashCode * 397) ^ Attention.GetHashCode();
            return hashCode;
          }
        }
    */
  }
}