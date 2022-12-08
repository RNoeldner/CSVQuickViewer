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
#nullable enable

using System;

namespace CsvTools
{
  public class DelimitedFileDetectionResult
  {
    public readonly bool ByteOrderMark;

    public readonly int CodePageId;

    public readonly string CommentLine;

    public readonly string EscapePrefix;

    public readonly string FieldDelimiter;

    public readonly string FieldQualifier;

    public readonly string FileName;

    public readonly bool HasFieldHeader;

    public readonly string IdentifierInContainer;

    public readonly bool IsJson;

    public readonly RecordDelimiterTypeEnum NewLine;

    public readonly bool NoDelimitedFile;

    public readonly bool QualifyAlways;

    public readonly int SkipRows;

    public DelimitedFileDetectionResult(
      string fileName,
      int skipRows = 0,
      int codePageId = -1,
      bool byteOrderMark = false,
      bool qualifyAlways = false,
      string? identifierInContainer = "",
      string commentLine = "#",
      string? escapePrefix = "\\",
      string? fieldDelimiter = "",
      string? fieldQualifier = "",
      bool hasFieldHeader = true,
      bool isJson = false,
      bool noDelimitedFile = false,
      RecordDelimiterTypeEnum recordDelimiterType = RecordDelimiterTypeEnum.None)
    {
      FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
      IdentifierInContainer = identifierInContainer ?? string.Empty;
      SkipRows = skipRows < 1 ? 0 : skipRows;
      CodePageId = codePageId < 1 ? -1 : codePageId;
      ByteOrderMark = byteOrderMark;
      CommentLine = commentLine;
      EscapePrefix = GetShortDisplay(escapePrefix);
      FieldDelimiter = GetShortDisplay(fieldDelimiter);
      FieldQualifier = GetShortDisplay(fieldQualifier);
      HasFieldHeader = hasFieldHeader;
      IsJson = isJson;
      NoDelimitedFile = noDelimitedFile;
      QualifyAlways = qualifyAlways;
      NewLine = recordDelimiterType;
    }

#if !QUICK
    public virtual IFileSettingPhysicalFile PhysicalFile()
    {
      if (IsJson)
        return new JsonFile(string.Empty, FileName,"") { IdentifierInContainer = IdentifierInContainer };

      return new CsvFile(id: string.Empty, fileName: FileName)
      {
        QualifyAlways = QualifyAlways,
        CommentLine = CommentLine,
        EscapePrefix = GetShortDisplay(EscapePrefix),
        FieldDelimiter = GetShortDisplay(FieldDelimiter),
        FieldQualifier = GetShortDisplay(FieldQualifier),
        NewLine = NewLine,
        ByteOrderMark = ByteOrderMark,
        CodePageId = CodePageId,
        HasFieldHeader = HasFieldHeader,
        NoDelimitedFile = NoDelimitedFile,
        IdentifierInContainer = IdentifierInContainer,
        SkipRows = SkipRows
      };
    }

#endif

    private static string GetShortDisplay(in string? input)
    {
      if (input is null)
        return string.Empty;

      return input.WrittenPunctuation() switch
      {
        "\t" => "Tab",
        " " => "Space",
        "\u00A0" => "NBSP",
        "," => "Comma",
        ";" => "Semicolon",
        "|" => "Pipe",
        _ => input
      };
    }
  }
}