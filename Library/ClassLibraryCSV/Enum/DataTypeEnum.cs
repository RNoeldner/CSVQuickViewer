/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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

namespace CsvTools;

/// <summary>
///   Enumeration of the supported data types.
/// </summary>
public enum DataTypeEnum
{
  /// <summary>
  ///   A 32-bit integer.
  /// </summary>
  [Description("Integer")]
  Integer = 0,

  /// <summary>
  ///   A decimal value (28–29 significant digits), typically used for precise monetary values.
  /// </summary>
  [Description("Money (High Precision)")]
  [ShortDescription("Numeric")]
  Numeric = 1,

  /// <summary>
  ///   A double-precision floating point value (15–16 significant digits), used for scientific or large-range calculations.
  /// </summary>
  [Description("Floating Point (High Range)")]
  [ShortDescription("Double")]
  Double = 2,

  /// <summary>
  ///   A date or time value.
  /// </summary>
  [Description("Date Time")]
  DateTime = 3,

  /// <summary>
  ///   A boolean value.
  /// </summary>
  [Description("Boolean")]
  Boolean = 4,

  /// <summary>
  ///   A system GUID or UUID.
  /// </summary>
  [Description("GUID / UUID")]
  [ShortDescription("GUID")]
  Guid = 5,

  // ------------------------
  // Text and transformation types
  // ------------------------

  /// <summary>
  ///   Plain text or string value.
  /// </summary>
  [Description("Text")]
  String = 10,

  /// <summary>
  ///   Encodes text for HTML output, preserving line breaks (minimal HTML encoding).
  /// </summary>
  [Description("Encode HTML (Linefeed only)")]
  [ShortDescription("HTML Min")]
  TextToHtml = 11,

  /// <summary>
  ///   Fully encodes text for HTML output (e.g., '&lt;' → '&amp;lt;').
  /// </summary>
  [Description("Encode HTML ('<' → '&lt;')")]
  [ShortDescription("HTML")]
  TextToHtmlFull = 12,

  /// <summary>
  ///   A substring extracted by a specified splitter.
  /// </summary>
  [Description("Text Part")]
  TextPart = 13,

  /// <summary>
  ///   Converts escaped sequences (e.g. '\n', '\r') to their literal characters.
  /// </summary>
  [Description(@"Unescaped Text (\r → ␍)")]
  [ShortDescription("Unescaped Text")]
  TextUnescape = 14,

  /// <summary>
  ///   Binary data stored in a file; the text value refers to the file path.
  /// </summary>
  [Description("Binary (File Reference)")]
  [ShortDescription("Binary")]
  Binary = 15,

#if !QUICK
  /// <summary>
  ///   Converts Markdown text to HTML.
  /// </summary>
  [Description("Markdown to HTML")]
  Markdown2Html = 16,
#endif

  /// <summary>
  ///   Performs a regular expression replacement on the text.
  /// </summary>
  [Description("Text Replace")]
  TextReplace = 17,

  /// <summary>
  ///   Decodes HTML entities to plain text.
  /// </summary>
  [Description("Decode HTML")]
  [ShortDescription("HTML Read")]
  HtmlToText = 18,
}