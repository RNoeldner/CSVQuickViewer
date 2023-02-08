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
using Newtonsoft.Json;
using System.ComponentModel;

namespace CsvTools
{
  /// <summary>
  ///   Data store to pass back information retrieved from inspection or parsing Manifest Information
  /// </summary>
  public sealed class InspectionResult
  {
    [DefaultValue(0)] public int SkipRows = 0;
    [DefaultValue(65001)] public int CodePageId = 65001;
    [DefaultValue(false)] public bool ByteOrderMark = false;
    [DefaultValue("")] public string IdentifierInContainer = string.Empty;
    [DefaultValue("#")] public string CommentLine = "#";
    [DefaultValue('\\')] public char EscapePrefix = '\\';
    [DefaultValue(',')] public char FieldDelimiter = ',';
    [DefaultValue('"')] public char FieldQualifier = '"';
    [DefaultValue(false)] public bool ContextSensitiveQualifier = false;
    [DefaultValue(true)] public bool DuplicateQualifierToEscape = true;
    [DefaultValue(true)] public bool HasFieldHeader = true;
    [DefaultValue(RecordDelimiterTypeEnum.None)] public RecordDelimiterTypeEnum NewLine = RecordDelimiterTypeEnum.None;

    [JsonIgnore]
    [DefaultValue("")]
    public string FileName = string.Empty;

    [JsonIgnore]
    [DefaultValue(false)]
    public bool IsJson = false;

    [JsonIgnore]
    [DefaultValue(false)]
    public bool NoDelimitedFile = false;

    [JsonIgnore]
    [DefaultValue("")]
    public string ColumnFile = string.Empty;

    [JsonIgnore]
    public ColumnCollection Columns = new ColumnCollection();

#if !QUICK
    public IFileSettingPhysicalFile PhysicalFile()
    {
      var ret = (IsJson) ? new JsonFile(string.Empty, FileName) { IdentifierInContainer = IdentifierInContainer } as IFileSettingPhysicalFile : new CsvFile(id: string.Empty, fileName: FileName)
      {
        CommentLine = CommentLine,
        EscapePrefixChar = EscapePrefix,
        FieldDelimiterChar = FieldDelimiter,
        FieldQualifierChar = FieldQualifier,
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
#endif
  }
}