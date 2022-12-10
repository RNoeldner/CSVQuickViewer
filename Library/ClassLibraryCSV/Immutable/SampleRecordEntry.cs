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
#if XmlSerialization
using System.Xml.Serialization;
#endif

namespace CsvTools
{
  [Serializable]
  public sealed class SampleRecordEntry : IEquatable<SampleRecordEntry>, ICloneable, ICollectionIdentity
  {
#if XmlSerialization
    [Obsolete("Only needed for XmlSerialization")]
    public SampleRecordEntry()
      : this(0, true, string.Empty)
    {
    }
#endif

    [JsonConstructor]
    public SampleRecordEntry(long? recordNumber, bool? provideEvidence = true, string? error = "")
    {
      RecordNumber = recordNumber ?? 0;
      ProvideEvidence = provideEvidence ?? false;
      Error = error ?? string.Empty;
    }

    /// <summary>
    ///   Gets or sets the record number.
    /// </summary>
    /// <value>The record number.</value>
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(0)]
    public long RecordNumber
    {
      get;
#if XmlSerialization
      set;
#endif
    }

    /// <summary>
    ///   Gets or sets a value indicating whether [provide evidence].
    /// </summary>
    /// <value><c>true</c> if [provide evidence]; otherwise, <c>false</c>.</value>
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue(true)]
    public bool ProvideEvidence
    {
      get;
#if XmlSerialization
      set;
#endif
    }

    /// <summary>
    ///   Gets or sets the error.
    /// </summary>
    /// <value>The error.</value>
#if XmlSerialization
    [XmlAttribute]
#endif
    [DefaultValue("")]
    public string Error
    {
      get;
#if XmlSerialization
      set;
#endif
    }

    /// <summary>
    ///   Clones this instance into a new instance of the same type
    /// </summary>
    /// <returns></returns>
    public object Clone() => new SampleRecordEntry(RecordNumber, ProvideEvidence, Error);

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(SampleRecordEntry? other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return RecordNumber == other.RecordNumber
             && ProvideEvidence == other.ProvideEvidence
             && string.Equals(Error, other.Error, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///   Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    ///   <see langword="true" /> if the specified object is equal to the current object; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public override bool Equals(object? obj) => Equals(obj as SampleRecordEntry);

    /// <summary>
    ///   Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        var hashCode = RecordNumber.GetHashCode();
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        hashCode = (hashCode * 397) ^ ProvideEvidence.GetHashCode();
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Error);
        return hashCode;
      }
    }

    [JsonIgnore] public int CollectionIdentifier => RecordNumber.GetHashCode();
  }
}