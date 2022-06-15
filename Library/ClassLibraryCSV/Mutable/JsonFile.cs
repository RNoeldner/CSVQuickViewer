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
  /// <inheritdoc cref="CsvTools.StructuredFile" />
  /// <summary>
  ///   Setting file for Json files, its an implementation of <see cref="T:CsvTools.StructuredFile" />
  /// </summary>
  [Serializable]
  public class JsonFile : StructuredFile, IJsonFile
  {
    private bool m_EmptyAsNull;

    [XmlElement]
    [DefaultValue("")]
    public bool EmptyAsNull
    {
      get => m_EmptyAsNull; set
      {
        if (m_EmptyAsNull.Equals(value))
          return;
        m_EmptyAsNull = value;
        NotifyPropertyChanged(nameof(EmptyAsNull));
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Initializes a new instance of the <see cref="T:CsvTools.StructuredFile" /> class.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public JsonFile(string fileName)
      : base(fileName)
    {
    }

    /// <inheritdoc />
    public JsonFile()
      : this(string.Empty)
    {
    }

    public override object Clone()
    {
      var other = new JsonFile();
      CopyTo(other);
      return other;
    }

    public override void CopyTo(IFileSetting other)
    {
      base.CopyTo(other);
      if (!(other is IJsonFile otherJson))
        return;

      otherJson.EmptyAsNull = EmptyAsNull;
    }

    public override bool Equals(IFileSetting? other) =>
      other is IJsonFile json && BaseSettingsEquals(json as StructuredFile) && json.EmptyAsNull == EmptyAsNull;
  }
}