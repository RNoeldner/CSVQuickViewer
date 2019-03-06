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
using System.Threading;
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   Setting for StructuredFile
  /// </summary>
  [Serializable]
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  public class StructuredFile : BaseSettings, IFileSetting, IEquatable<StructuredFile>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
  {
    private readonly string m_Footer = string.Empty;
    private readonly string m_Header = string.Empty;
    private bool m_JSONEncode;
    private string m_Row = string.Empty;
    private bool m_XMLEncode;

    /// <summary>
    ///   Initializes a new instance of the <see cref="StructuredFile" /> class.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public StructuredFile(string fileName)
      : base(fileName)
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="StructuredFile" /> class.
    /// </summary>
    public StructuredFile()
    {
    }

    /// <summary>
    ///   Set to <c>true</c> if the contend needs to be HTML Encoded, needed for XML Files
    /// </summary>
    [XmlAttribute]
    [DefaultValue(true)]
    public bool JSONEncode
    {
      get => m_JSONEncode;
      set
      {
        if (m_JSONEncode == value) return;
        m_JSONEncode = value;
        NotifyPropertyChanged(nameof(JSONEncode));
      }
    }

    /// <summary>
    ///   Template for a row
    /// </summary>
    [XmlElement]
    [DefaultValue("")]
    public string Row
    {
      get => m_Row;

      set
      {
        var newVal = value ?? string.Empty;
        if (m_Row.Equals(newVal, StringComparison.Ordinal)) return;
        m_Row = newVal;
        NotifyPropertyChanged(nameof(Row));
      }
    }

    /// <summary>
    ///   Set to <c>true</c> if the contend needs to be HTML Encoded, needed for XML Files
    /// </summary>
    [XmlAttribute]
    [DefaultValue(true)]
    public bool XMLEncode
    {
      get => m_XMLEncode;
      set
      {
        if (m_XMLEncode == value) return;
        m_XMLEncode = value;
        NotifyPropertyChanged(nameof(XMLEncode));
      }
    }

    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise,
    ///   <see langword="false" />.
    /// </returns>
    public bool Equals(StructuredFile other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) && string.Equals(m_Footer, other.Footer, StringComparison.OrdinalIgnoreCase) && string.Equals(m_Header, other.Header, StringComparison.OrdinalIgnoreCase) &&
             m_JSONEncode == other.JSONEncode && string.Equals(m_Row, other.Row, StringComparison.Ordinal) &&
             m_XMLEncode == other.XMLEncode;
    }

    /// <summary>
    ///   Clones this instance.
    /// </summary>
    /// <returns></returns>
    public override IFileSetting Clone()
    {
      Contract.Ensures(Contract.Result<IFileSetting>() != null);
      var other = new StructuredFile();
      CopyTo(other);
      return other;
    }

    /// <summary>
    ///   Copies all values to other instance
    /// </summary>
    /// <param name="other">The other.</param>
    public override void CopyTo(IFileSetting other)
    {
      if (other == null)
        return;
      base.CopyTo((BaseSettings)other);

      if (!(other is StructuredFile otherSwf)) return;
      otherSwf.Header = m_Header;
      otherSwf.Footer = m_Footer;
      otherSwf.Row = m_Row;
      otherSwf.XMLEncode = m_XMLEncode;
      otherSwf.JSONEncode = m_JSONEncode;
    }

    public override bool Equals(IFileSetting other)
    {
      return Equals(other as StructuredFile);
    }

    /// <summary>
    ///   Gets the file reader.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"> Structured files can not be read they are for writing only</exception>
    public override IFileReader GetFileReader() => throw new NotImplementedException(" Structured files can not be read they are for writing only");

    /// <summary>
    ///   Gets the file writer.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public override IFileWriter GetFileWriter(CancellationToken cancellationToken)
    {
      return new StructuredFileWriter(this, cancellationToken);
    }

    /*
    /// <summary>Serves as the default hash function. </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = base.GetBaseHashCode();
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_Footer);
        hashCode = (hashCode * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(m_Header);
        hashCode = (hashCode * 397) ^ m_JSONEncode.GetHashCode();
        hashCode = (hashCode * 397) ^ m_Row.GetHashCode();
        hashCode = (hashCode * 397) ^ m_XMLEncode.GetHashCode();
        return hashCode;
      }
    }
    */

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object. </param>
    /// <returns>
    ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
    /// </returns>
    public override bool Equals(object obj)
    {
      if (obj is null) return false;
      if (ReferenceEquals(this, obj)) return true;
      return (obj is StructuredFile typed) && Equals(typed);
    }
  }
}