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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace CsvTools
{
  /// <summary>
  ///   Setting for StructuredFile
  /// </summary>
  public abstract class StructuredFile : BaseSettingPhysicalFile
  {
    private string m_Row;

    /// <summary>
    ///   Initializes a new instance of the <see cref="StructuredFile" /> class.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="row">Text with Placeholders for a row</param>
    protected StructuredFile(in string id, in string fileName, in string row)
      : base(id, fileName)
    {
      m_Row = row;
    }

    /// <summary>
    ///   Template for a row
    /// </summary>
    [XmlElement]
    [DefaultValue("")]
    public string Row
    {
      get => m_Row;
      set => SetProperty(ref m_Row, value);      
    }

    /// <summary>
    ///   Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    ///   <see langword="true" /> if the current object is equal to the <paramref name="other" />
    ///   parameter; otherwise, <see langword="false" />.
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
      otherSwf.LastChange = LastChange;
    }

    public override IEnumerable<string> GetDifferences(IFileSetting other)
    {
      if (other is StructuredFile structured)
        if (Row != structured.Row)
          yield return $"Row: {Row} {structured.Row}";
      foreach (var res in base.GetDifferences(other))
        yield return res;
    }
  }
}