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

namespace CsvTools
{
  /// <summary>
  /// Line/Record Separator for text files
  /// </summary>
  public enum RecordDelimiterTypeEnum
  {
    /// <summary>Unspecified</summary>
    None,

    /// <summary>Use Line feed</summary>
    [ShortDescription("LF")]
    [Description("Line feed (Unix)")]
    Lf,

    /// <summary>Use Carriage Return</summary>
    [ShortDescription("CR")]
    [Description("Carriage Return (uncommon)")]
    Cr = 2,

    /// <summary>Use Carriage Return / Line feed</summary>
    [ShortDescription("CR LF")]
    [Description("Carriage Return / Line feed (Windows)")]
    Crlf = 3,

    /// <summary>Use Line feed / Carriage Return</summary>
    [ShortDescription("LF CR")]
    [Description("Line feed / Carriage Return (rarely used)")]
    Lfcr = 4,

    /// <summary>Use Record Separator</summary>
    [ShortDescription("RS")]
    [Description("Record Separator (QNX rarely used)")]
    Rs = 5,

    /// <summary>Use Unit Separator</summary>
    [ShortDescription("US")]
    [Description("Unit Separator (rarely used)")]
    Us = 6,

    /// <summary>Use NewLine</summary>
    [ShortDescription("NL")]
    [Description("NewLine (IBM mainframe)")]
    Nl = 7
  }
}
