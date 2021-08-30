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

namespace CsvTools
{
  public class ImmutableFileFormat : IFileFormat
  {
    public ImmutableFileFormat(
      bool isFixedLength = false,
      bool qualifyAlways = false,
      bool qualifyOnlyIfNeeded = true,
      char escapeCharacter = '\0',
      char fieldDelimiterChar = ',',
      string delimiterPlaceholder = "",
      char fieldQualifierChar = '"',
      string qualifierPlaceholder = "",
      RecordDelimiterType newLine = RecordDelimiterType.CRLF,
      string newLinePlaceholder = "")
    {
      IsFixedLength = isFixedLength;
      QualifyAlways = qualifyAlways;
      QualifyOnlyIfNeeded = qualifyOnlyIfNeeded;

      FieldDelimiterChar = fieldDelimiterChar;
      DelimiterPlaceholder = delimiterPlaceholder ?? throw new ArgumentNullException(nameof(delimiterPlaceholder));

      FieldQualifierChar = fieldQualifierChar;
      QuotePlaceholder = qualifierPlaceholder ?? throw new ArgumentNullException(nameof(qualifierPlaceholder));

      EscapeChar = escapeCharacter;

      NewLine = newLine;
      NewLinePlaceholder = newLinePlaceholder ?? throw new ArgumentNullException(nameof(newLinePlaceholder));
    }

    public string DelimiterPlaceholder { get; }

    public virtual char EscapeChar { get; }

    public char FieldDelimiterChar { get; }

    public char FieldQualifierChar { get; }

    public bool IsFixedLength { get; }

    public RecordDelimiterType NewLine { get; }

    public string NewLinePlaceholder { get; }

    public bool QualifyAlways { get; }

    public bool QualifyOnlyIfNeeded { get; }

    public string QuotePlaceholder { get; }
  }
}