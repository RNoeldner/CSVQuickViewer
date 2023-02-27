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

using System.ComponentModel;

namespace CsvTools
{
  /// <summary>
  ///   Enumeration of the supported Data Types
  /// </summary>
  public enum DataTypeEnum
  {
    /// <summary>
    ///   An 32 Bit Integer
    /// </summary>
    [Description("Integer")]
    Integer = 0,

    /// <summary>
    ///   A "decimal" value 28-29 significant digits, used for money values
    /// </summary>
    [Description("Money (High Precision)")]
    [ShortDescription("Numeric")]
    Numeric = 1,

    /// <summary>
    ///   A "Double" 15-16 significant digits, used for floating point calculation
    /// </summary>
    [Description("Floating  Point (High Range)")]
    [ShortDescription("Double")]
    Double = 2,

    /// <summary>
    ///   A Date or Time Values
    /// </summary>
    /// 
    [Description("Date Time")]
    DateTime = 3,

    /// <summary>
    ///   A boolean
    /// </summary>
    [Description("Boolean")]    
    Boolean = 4,

    /// <summary>
    ///   A system GUID
    /// </summary>
    [Description("GUID / UUID")]
    [ShortDescription("GUID")]
    Guid = 5,

    /// <summary>
    ///   A String or VarChar
    /// </summary>
    [Description("Text")]
    String = 10,

    /// <summary>
    ///   A String or VarChar, but do some basic HTML encoding, "Encode HTML (Linefeed only)"
    /// </summary>
    [Description("Encode HTML (CData, Linefeed, List)")]
    [ShortDescription("HTML Min")]
    TextToHtml = 11,

    /// <summary>
    ///   A String or VarChar, but do some advanced HTML encoding, "Encode HTML"
    /// </summary>
    [Description("Encode HTML ('<' -> '&lt;')")]
    [ShortDescription("HTML")]
    TextToHtmlFull = 12,

    /// <summary>
    ///   A given part of the text separate be a splitter
    /// </summary>
    [Description("Text Part")]
    TextPart = 13,

    /// <summary>
    ///   Unescape c or c# escaped text to a verbatim text, e.g. \n will become a CR
    /// </summary>
    [Description("Unescape Text ('\\r' -> \u240D)")]
    [ShortDescription("Unescape Text")]
    TextUnescape = 14,

    /// <summary>
    ///   Binary data usually usually stored in a file
    /// </summary>
    [Description("Binary (File Reference)")]
    [ShortDescription("Binary")]
    Binary = 15,

#if !QUICK

    /// <summary>
    ///   Convert Markdown text to HTML
    /// </summary>
    [Description("Markdown to HTML")]    
    Markdown2Html = 16,
#endif

    /// <summary>
    ///   Perform Regex.Replace 
    /// </summary>
    [Description("Text Replace")]
    TextReplace = 17
  }
}