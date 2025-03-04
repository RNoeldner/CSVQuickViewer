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
  ///   Setting for reading  or writing a delimited file
  /// </summary>
  public interface ICsvFile : IFileSettingPhysicalFile
  {
    /// <summary>
    ///   Gets or sets a value indicating whether rows should combine if there are fewer columns.
    /// </summary>
    /// <value>
    ///   <c>true</c> if row combining is allowed; otherwise, <c>false</c>.
    /// </value>
    bool AllowRowCombining { get; set; }

    /// <summary>
    ///   Gets or sets the text to indicate that the line is comment line and not contain data. If a
    ///   line starts with the given text, it is ignored in the data grid.
    /// </summary>    
    string CommentLine { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether context-sensitive qualification is used
    /// </summary>    
    bool ContextSensitiveQualifier { get; set; }

    /// <summary>
    ///   Gets or sets the new delimiter placeholder, the placeholder substitutes a delimiter, it's
    ///   similar to escaping but could replace a delimiter with something completely different like a longer text, e.G. {Delimiter}
    /// </summary>
    /// <value>The new line placeholder.</value>
    string DelimiterPlaceholder { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether quotes in text will be represented as double quotes 
    /// </summary>
    bool DuplicateQualifierToEscape { get; set; }

    /// <summary>
    ///   Gets the escape prefix as character
    /// </summary>
    /// <value>The field delimiter char.</value>
    /// <remarks>If \0, the quote are often repeated to escape them</remarks>
    char EscapePrefixChar { get; set; }

    /// <summary>
    ///   Gets the field delimiter as character, usually ,
    /// </summary>
    /// <value>The field delimiter char.</value>
    char FieldDelimiterChar { get; set; }

    /// <summary>
    ///  Gets the qualifier character as character, usually "
    /// </summary>
    /// <value>The field delimiter char.</value>
    char FieldQualifierChar { get; set; }

    /// <summary>
    ///  Determining the record separator used writing a delimited text file, while reading it does not matter
    /// </summary>
    RecordDelimiterTypeEnum NewLine { get; set; }

    /// <summary>
    ///   Gets or sets the new line placeholder, the placeholder substitutes a linefeed
    /// </summary>
    /// <value>The new line placeholder.</value>
    string NewLinePlaceholder { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether a file is most likely not a delimited file
    /// </summary>
    /// <value>
    ///   <c>true</c> if the file is assumed to be a non-delimited file; otherwise, <c>false</c>.
    /// </value>
    bool NoDelimitedFile { get; set; }

    /// <summary>
    ///   Gets or sets the maximum number of warnings being raised
    /// </summary>
    /// <value>The number of warnings.</value>
    int NumWarnings { get; set; }

    /// <summary>
    ///   Gets or sets the quote placeholder, the placeholder substitutes a field qualifier, It's
    ///   similar to escaping but could replace a Quote with something completely different or a
    ///   longer text, e.G. {Quote}
    /// </summary>
    /// <value>The quote placeholder.</value>
    string QualifierPlaceholder { get; set; }

    /// <summary>
    ///   Gets a value indicating whether to qualify every text even if number or empty. Usually false
    /// </summary>
    /// <value><c>true</c> if qualify even when not needed; otherwise, <c>false</c>.</value>
    bool QualifyAlways { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to qualify only if needed. Usually true
    /// </summary>
    /// <value><c>true</c> if qualify only when required; otherwise, <c>false</c>.</value>
    bool QualifyOnlyIfNeeded { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to treat a single LF as space
    /// </summary>
    /// <value>
    ///   <c>true</c> if LF should be treated as space; otherwise, <c>false</c>.
    /// </value>
    bool TreatLfAsSpace { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to replace unknown character.
    /// </summary>
    /// <value><c>true</c> if unknown character should be replaced; otherwise, <c>false</c>.</value>
    bool TreatUnknownCharacterAsSpace { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating when leading spaces should be trimmed.
    ///   Other than general trimming there is the option to do this based on whether the text is quoted or not
    /// </summary>
    TrimmingOptionEnum TrimmingOption { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether the reader should try to solve more columns.
    /// </summary>
    /// <value>
    ///   <c>true</c> if it should be tried to solve misalignment more columns; otherwise, <c>false</c>.
    /// </value>
    bool TryToSolveMoreColumns { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn if delimiter is in a value.
    /// </summary>
    /// <value>
    ///   <c>true</c> if a warning should be issued if a delimiter is encountered; otherwise, <c>false</c>.
    /// </value>
    bool WarnDelimiterInValue { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn empty tailing columns.
    /// </summary>
    /// <value><c>true</c> if [warn empty tailing columns]; otherwise, <c>false</c>.</value>
    bool WarnEmptyTailingColumns { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn line feeds in columns. Usually false
    /// </summary>
    /// <value><c>true</c> if line feed should raise a warning; otherwise, <c>false</c>.</value>
    bool WarnLineFeed { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn occurrence of NBSP. Usually true
    /// </summary>
    /// <value><c>true</c> to issue a writing if there is a NBSP; otherwise, <c>false</c>.</value>
    bool WarnNBSP { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files. Usually false
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
    bool WarnQuotes { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files. Usually true
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
    bool WarnQuotesInQuotes { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn unknown character. Usually true
    /// </summary>
    /// <value><c>true</c> if unknown character should issue a warning; otherwise, <c>false</c>.</value>
    bool WarnUnknownCharacter { get; set; }

    /// <summary>
    ///  The resulting file should not use delimiters but instead use fixed length columns
    /// </summary>
    bool WriteFixedLength { get; set; }
  }
}