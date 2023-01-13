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
  /// <summary>
  ///   Datastore to pass back information retrieved from Detection or parsing Manifest Information
  /// </summary>
  public class DetectionResult
  {
    public readonly string FileName = string.Empty;
    public int SkipRows = 0;
    public int CodePageId = -1;
    public bool ByteOrderMark = false;
    public string IdentifierInContainer = string.Empty;
    public string CommentLine = "#";
    public string EscapePrefix = "\\";
    public string FieldDelimiter = ",";
    public string FieldQualifier = "\"";
    public bool ContextSensitiveQualifier = false;
    public bool DuplicateQualifierToEscape = true;
    public bool HasFieldHeader = true;
    public bool IsJson = false;
    public bool NoDelimitedFile = false;
    public RecordDelimiterTypeEnum NewLine = RecordDelimiterTypeEnum.None;
    public string ColumnFile = string.Empty;
    public ColumnCollection Columns = new ColumnCollection();

    public DetectionResult(string fileName)
    {
      FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
    }

#if !QUICK
    public virtual IFileSettingPhysicalFile PhysicalFile()
    {
      var ret = (IsJson) ? new JsonFile(string.Empty, FileName) { IdentifierInContainer = IdentifierInContainer } as IFileSettingPhysicalFile : new CsvFile(id: string.Empty, fileName: FileName)
      {
        CommentLine = CommentLine,
        EscapePrefix = GetShortDisplay(EscapePrefix),
        FieldDelimiter = GetShortDisplay(FieldDelimiter),
        FieldQualifier = GetShortDisplay(FieldQualifier),
        ContextSensitiveQualifier = ContextSensitiveQualifier,
        DuplicateQualifierToEscape = DuplicateQualifierToEscape,
        NewLine = NewLine,
        ByteOrderMark = ByteOrderMark,
        CodePageId = CodePageId,
        HasFieldHeader = HasFieldHeader,
        NoDelimitedFile = NoDelimitedFile,
        IdentifierInContainer = IdentifierInContainer,
        SkipRows = SkipRows
      };

      ret.ColumnCollection.AddRangeNoClone(Columns);
      ret.ColumnFile = ColumnFile;

      return ret;
    }


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
#endif
  }
}