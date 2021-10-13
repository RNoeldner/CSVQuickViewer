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
  ///   Setting for StructuredFile
  /// </summary>
  public abstract class StructuredFile : BaseSettingPhysicalFile
  {
    private string m_Row = string.Empty;

    /// <summary>
    ///   Initializes a new instance of the <see cref="StructuredFile" /> class.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    protected StructuredFile(string fileName)
      : base(fileName)
    {
    }

    /// <summary>
    ///   Template for a row
    /// </summary>
    [XmlElement]
    [DefaultValue("")]
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string Row
    {
      get => m_Row;

      set
      {
        var newVal = value ?? string.Empty;
        if (m_Row.Equals(newVal, StringComparison.Ordinal))
          return;
        m_Row = newVal;
        NotifyPropertyChanged(nameof(Row));
      }
    }

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other"
    ///   /> parameter; otherwise, <see langword="false" />.
    /// </returns>
    public bool BaseSettingsEquals(in StructuredFile? other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return string.Equals(m_Row, other.Row, StringComparison.Ordinal) && base.BaseSettingsEquals(other);
    }

    /// <summary>
    ///   Copies all values to other instance
    /// </summary>
    /// <param name="other">The other.</param>
    public override void CopyTo(IFileSetting other)
    {
      BaseSettingsCopyTo((BaseSettings) other);

      if (!(other is StructuredFile otherSwf))
        return;

      otherSwf.Row = m_Row;
    }
  }
}