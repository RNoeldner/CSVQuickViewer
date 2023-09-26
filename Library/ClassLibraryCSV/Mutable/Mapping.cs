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

#nullable enable

using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
#if XmlSerialization
using System.Xml.Serialization;
#endif

namespace CsvTools
{
  /// <inheritdoc cref="System.ICloneable" />
  /// <summary>
  ///   Setting to store a Field Mapping
  /// </summary>
  [DebuggerDisplay("Mapping(File/Source: {FileColumn} -> Template/Destination {TemplateField})")]
  [Serializable]
  public sealed class Mapping : IEquatable<Mapping>, ICloneable, ICollectionIdentity
  {
#if XmlSerialization
    public Mapping()
      : this(string.Empty, string.Empty)
    {
    }
#endif

    [JsonConstructor]
    public Mapping(string? fileColumn, string? templateField, bool? update = false, bool? attention = false)
    {
      FileColumn = fileColumn!;
      TemplateField = templateField!;
      Update = update ?? false;
      Attention = attention ?? false;
    }

    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="Mapping" /> required additional attention
    /// </summary>
    /// <value><c>true</c> if attention; otherwise, <c>false</c>.</value>
#if XmlSerialization
    [XmlAttribute]
#endif
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
#if XmlSerialization
    [XmlAttribute("Column")]
#endif
    public string FileColumn { get; set; }

    /// <summary>
    ///   Gets or sets the := Template Column.
    /// </summary>
    /// <value>The destination.</value>
    /// <remarks>The set operator is only present to allow serialization</remarks>
#if XmlSerialization
    [XmlAttribute("Field")]
#endif
    public string TemplateField { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether this <see cref="Mapping" /> should be used for update
    /// </summary>
    /// <value><c>true</c> if it should be regarded for updates; otherwise, <c>false</c>.</value>
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(false)]
    public bool Update
    {
      get;
      set;
    }

    /// <inheritdoc />
    public object Clone() => new Mapping(FileColumn, TemplateField, Update, Attention);

    /// <inheritdoc />
    public bool Equals(Mapping? other)
    {
      if (other is null)
        return false;
      
      return Attention == other.Attention
             && Update == other.Update
             && string.Equals(FileColumn, other.FileColumn, StringComparison.OrdinalIgnoreCase)
             && string.Equals(TemplateField, other.TemplateField, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    /// <remarks>Combined Column and Field</remarks>
    [JsonIgnore]
    public int CollectionIdentifier => FileColumn.IdentifierHash(TemplateField);
  }
}