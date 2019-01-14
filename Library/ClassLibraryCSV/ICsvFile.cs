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
using System.Text;

namespace CsvTools
{
  /// <summary>
  ///  Interface for the setting of a CSV file
  /// </summary>
  public interface ICsvFile : IFileSettingRemoteDownload, IEquatable<ICsvFile>
  {
    /// <summary>
    /// Gets or sets a value indicating whether rows should combined if there are less columns.
    /// </summary>
    /// <value>
    ///  <c>true</c> if row combining is allowed; otherwise, <c>false</c>.
    /// </value>
    bool AllowRowCombining { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether this instance should use alternate quoting
    /// </summary>
    /// <value><c>true</c> if Alternate quoting should be used, <c>false</c> otherwise.</value>
    /// <example>
    ///  Test,"This is a "Test"",OK
    ///  -- &gt; This is a "Test"
    /// </example>
    bool AlternateQuoting { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
    bool ByteOrderMark { get; set; }

    /// <summary>
    ///  Gets or sets the code page.
    /// </summary>
    /// <value>The code page.</value>
    int CodePageId { get; set; }

    /// <summary>
    ///  Gets current encoding.
    /// </summary>
    /// <value>The current encoding.</value>
    Encoding CurrentEncoding { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether the file is double encoded and need to be double decoded.
    /// </summary>
    /// <value>
    ///  <c>true</c> if double decode; otherwise, <c>false</c>.
    /// </value>
    bool DoubleDecode { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether a file is most likely not a delimited file
    /// </summary>
    /// <value>
    ///  <c>true</c> if the file is assumed to be a non delimited file; otherwise, <c>false</c>.
    /// </value>
    bool NoDelimitedFile { get; set; }

    /// <summary>
    ///  Gets or sets the maximum number of warnings.
    /// </summary>
    /// <value>The number of warnings.</value>
    int NumWarnings { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to treat a single LF as space
    /// </summary>
    /// <value>
    ///  <c>true</c> if LF should be treated as space; otherwise, <c>false</c>.
    /// </value>
    bool TreatLFAsSpace { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether to replace unknown character.
    /// </summary>
    /// <value><c>true</c> if unknown character should be replaced; otherwise, <c>false</c>.</value>
    bool TreatUnknowCharaterAsSpace { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the reader should try to solve more columns.
    /// </summary>
    /// <value>
    ///  <c>true</c> if it should be try to solve misalignment more columns; otherwise, <c>false</c>.
    /// </value>
    bool TryToSolveMoreColumns { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether to warn if delimiter is in a value.
    /// </summary>
    /// <value>
    ///  <c>true</c> if a warning should be issued if a delimiter is encountered; otherwise, <c>false</c>.
    /// </value>
    bool WarnDelimiterInValue { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether to warn empty tailing columns.
    /// </summary>
    /// <value><c>true</c> if [warn empty tailing columns]; otherwise, <c>false</c>.</value>
    bool WarnEmptyTailingColumns { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether to warn line feeds in columns.
    /// </summary>
    /// <value><c>true</c> if line feed should raise a warning; otherwise, <c>false</c>.</value>
    bool WarnLineFeed { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether to warn occurrence of NBSP.
    /// </summary>
    /// <value><c>true</c> to issue a writing if there is a NBSP; otherwise, <c>false</c>.</value>
    bool WarnNBSP { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>

    bool WarnQuotes { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
    bool WarnQuotesInQuotes { get; set; }

    /// <summary>
    ///  Gets or sets a value indicating whether to warn unknown character.
    /// </summary>
    /// <value><c>true</c> if unknown character should issue a warning; otherwise, <c>false</c>.</value>
    bool WarnUnknowCharater { get; set; }
  }
}