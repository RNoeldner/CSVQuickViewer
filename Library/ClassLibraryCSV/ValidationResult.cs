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
using System.Xml.Serialization;

namespace CsvTools
{
  /// <summary>
  ///   Class to store validation result and cache them
  /// </summary>
  [Serializable]
  public class ValidationResult : IValidationResult
  {
    public ValidationResult() : this(string.Empty, 0)
    {
    }

    public ValidationResult(string tableName, long numRecords, long numErrors = -1, long numWarning = -1)
    {
      TableName = tableName;
      NumberRecords = numRecords;
      WarningCount = numWarning;
      ErrorCount = numErrors;
    }

    public ValidationResult(IFileSetting setting) : this(setting.ID, setting.NumRecords)
    {
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
      get;
    }


    /// <summary>
    ///   Gets or sets the error count.
    /// </summary>
    /// <value>
    ///   The error count.
    /// </value>
    [XmlAttribute]
    public long ErrorCount
    {
      get;
      set;
    }

    /// <summary>
    ///   Gets or sets the number records.
    /// </summary>
    /// <value>
    ///   The number records.
    /// </value>
    [XmlAttribute]
    public long NumberRecords
    {
      get;
      set;
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
      get;
      set;
    }
  }
}