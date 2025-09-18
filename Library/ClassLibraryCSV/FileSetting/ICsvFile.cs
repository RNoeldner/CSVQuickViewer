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
 * If not, see http://www.gnu.org/licenses/.
 */

namespace CsvTools
{
  /// <summary>
  ///   Settings for reading or writing a delimited file.
  /// </summary>
  public interface ICsvFile : IFileSettingPhysicalFile
  {
    /// <summary>
    ///   Gets or sets a value indicating whether rows with fewer columns
    ///   should be combined with the following row.
    /// </summary>
    bool AllowRowCombining { get; set; }

    /// <summary>
    ///   Gets or sets the prefix that marks a line as a comment (non-data).
    ///   Any line starting with this value is ignored when reading.
    /// </summary> 
    string CommentLine { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether qualifiers (quotes) are applied
    ///   depending on context, e.g., only around fields containing delimiters.
    /// </summary>
    bool ContextSensitiveQualifier { get; set; }

    /// <summary>
    ///   Gets or sets the placeholder used to substitute a delimiter character
    ///   inside a field. This can be used as an alternative to escaping,
    ///   e.g., replacing <c>,</c> with "{Delimiter}".
    /// </summary>
    string DelimiterPlaceholder { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether a quote character inside text
    ///   should be escaped by doubling it.
    /// </summary>
    bool DuplicateQualifierToEscape { get; set; }

    /// <summary>
    ///   Gets or sets the escape prefix character.
    ///   If set to <c>\0</c>, escaping is usually done by doubling the qualifier.
    /// </summary>
    char EscapePrefixChar { get; set; }

    /// <summary>
    ///   Gets or sets the field delimiter character (usually <c>,</c>).
    /// </summary>
    char FieldDelimiterChar { get; set; }

    /// <summary>
    ///   Gets or sets the field qualifier character (usually <c>"</c>).
    /// </summary>
    char FieldQualifierChar { get; set; }

    /// <summary>
    ///   Gets or sets the record separator used when writing a file.
    ///   (When reading, any common line ending is accepted.)
    /// </summary>
    RecordDelimiterTypeEnum NewLine { get; set; }

    /// <summary>
    ///   Gets or sets the placeholder used to substitute a line break inside a field.
    /// </summary>
    string NewLinePlaceholder { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether the file is assumed
    ///   to be non-delimited (e.g., fixed-width format).
    /// </summary>
    bool NoDelimitedFile { get; set; }

    /// <summary>
    ///   Gets or sets the maximum number of warnings to raise
    ///   before processing continues without further warnings.
    /// </summary>
    int NumWarnings { get; set; }

    /// <summary>
    ///   Gets or sets the placeholder used to substitute a qualifier character.
    ///   For example, replacing <c>"</c> with "{Quote}".
    /// </summary>
    string QualifierPlaceholder { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether all fields should always
    ///   be qualified, even if not required. Usually <c>false</c>.
    /// </summary>
    bool QualifyAlways { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether fields should only be qualified
    ///   when necessary (e.g., when containing a delimiter or line break).
    ///   Usually <c>true</c>.
    /// </summary>
    bool QualifyOnlyIfNeeded { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether a single LF (line feed) character
    ///   should be treated as a space.
    /// </summary>
    bool TreatLfAsSpace { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether unknown characters should
    ///   be replaced with a space.
    /// </summary>
    bool TreatUnknownCharacterAsSpace { get; set; }

    /// <summary>
    ///   Gets or sets the trimming behavior for leading and trailing spaces.
    ///   The behavior may depend on whether the field is quoted.
    /// </summary>
    TrimmingOptionEnum TrimmingOption { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether the reader should attempt
    ///   to resolve misaligned rows with more columns than expected.
    /// </summary>
    bool TryToSolveMoreColumns { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn when a delimiter
    ///   appears inside a field value.
    /// </summary>
    bool WarnDelimiterInValue { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn about trailing
    ///   empty columns.
    /// </summary>
    bool WarnEmptyTailingColumns { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn when line breaks
    ///   are found inside a field value. Usually <c>false</c>.
    /// </summary>
    bool WarnLineFeed { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn when a non-breaking space (NBSP)
    ///   character is encountered. Usually <c>true</c>.
    /// </summary>
    bool WarnNBSP { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn when fields are unnecessarily quoted.
    ///   Usually <c>false</c>.
    /// </summary>
    bool WarnQuotes { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn when quotes appear inside quoted text.
    ///   Usually <c>true</c>.
    /// </summary>
    bool WarnQuotesInQuotes { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to warn when unknown characters are encountered.
    ///   Usually <c>true</c>.
    /// </summary>
    bool WarnUnknownCharacter { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether to write the file using fixed-width columns
    ///   instead of delimiters. Only supported for writing.
    /// </summary>
    bool WriteFixedLength { get; set; }
  }
}
