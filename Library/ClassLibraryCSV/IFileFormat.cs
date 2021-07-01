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
  public interface IFileFormat
  {
    /// <summary>
    ///   Gets or sets the new delimiter placeholder, the placeholder substitutes a delimiter, its
    ///   similar to escaping but could replace a Quote with something completely different or a
    ///   longer text, e.G. {Delimiter}
    /// </summary>
    /// <value>The new line placeholder.</value>
    string DelimiterPlaceholder { get; }

    /// <summary>
    ///   Gets the escape character, in order to include a delimiter or quote in the text, these could
    ///   be escaped and would not be recognized a end of the text.
    /// </summary>
    /// <value>The field delimiter char.</value>
    /// <remarks>If \0, the quote are often repeated to escape them</remarks>
    char EscapeChar { get; }

    /// <summary>
    ///   Gets the field delimiter character, this delimiter separates two columns.
    /// </summary>
    /// <value>The field delimiter char.</value>
    char FieldDelimiterChar { get; }

    /// <summary>
    ///   Gets the field qualifier character also called quoting character, this surrounds a column
    ///   text so it may contain the delimiter or a linefeed without breaking teh structure
    /// </summary>
    /// <value>The field delimiter char.</value>
    char FieldQualifierChar { get; }

    /// <summary>
    ///   Gets a value indicating whether this it is a fixed length file
    /// </summary>
    /// <value><c>true</c> if this instance is fixed length; otherwise, <c>false</c>.</value>
    bool IsFixedLength { get; }

    RecordDelimiterType NewLine { get; }

    /// <summary>
    ///   Gets or sets the new line placeholder, the placeholder substitutes a linefeed
    /// </summary>
    /// <value>The new line placeholder.</value>
    string NewLinePlaceholder { get; }

    /// <summary>
    ///   Gets a value indicating whether to qualify every text even if number or empty.
    /// </summary>
    /// <value><c>true</c> if qualify only if needed; otherwise, <c>false</c>.</value>
    bool QualifyAlways { get; }

    /// <summary>
    ///   Gets or sets a value indicating whether to qualify only if needed.
    /// </summary>
    /// <value><c>true</c> if qualify only if needed; otherwise, <c>false</c>.</value>
    bool QualifyOnlyIfNeeded { get; }

    /// <summary>
    ///   Gets or sets the quote placeholder, the placeholder substitutes a field qualifier, its
    ///   similar to escaping but could replace a Quote with something completly different or a
    ///   longer text, e.G. {Quote}
    /// </summary>
    /// <value>The quote placeholder.</value>
    string QuotePlaceholder { get; }
  }
}