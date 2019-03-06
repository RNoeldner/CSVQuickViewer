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

namespace CsvTools
{
  /// <summary>
  ///   Enumeration of the supported Data Types
  /// </summary>
  public enum DataType
  {
#pragma warning disable CA1720 // Identifier contains type name
    /// <summary>
    ///   An 32 Bit Integer
    /// </summary>
    Integer = 0,

    /// <summary>
    ///   A "decimal" value 28-29 significant digits, used for money values
    /// </summary>
    Numeric = 1,

    /// <summary>
    ///   A "Double" 15-16 significant digits, used for floating point calculation
    /// </summary>
    Double = 2,

    /// <summary>
    ///   A Date or Time Values
    /// </summary>
    DateTime = 3,

    /// <summary>
    ///   A boolean
    /// </summary>
    Boolean = 4,

    /// <summary>
    ///   A system GUID
    /// </summary>
    Guid = 5,

    /// <summary>
    ///   A String or VarChar
    /// </summary>
    String = 10,

    /// <summary>
    ///   A String or VarChar, but do some basic HTML encoding, "Encode HTML (Linefeed only)"
    /// </summary>
    TextToHtml = 11,

    /// <summary>
    ///   A String or VarChar, but do some advanced HTML encoding, "Encode HTML"
    /// </summary>
    TextToHtmlFull = 12,

    /// <summary>
    ///   A given part of the text separate be a splitter
    /// </summary>
    TextPart = 13
  }
#pragma warning restore CA1720 // Identifier contains type name
}