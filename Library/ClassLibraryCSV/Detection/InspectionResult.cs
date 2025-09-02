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
    /// <summary>Number of rows to skip</summary>
    [DefaultValue(0)] public int SkipRows { get; set; }

    /// <summary>.NET CodePage ID</summary>
    [DefaultValue(65001)] public int CodePageId { get; set; } = 65001;
    
    /// <summary>Does encoding use BOM</summary>
    [DefaultValue(false)] public bool ByteOrderMark { get; set; }
    
    /// <summary>Identifier in container like zip</summary>
    [DefaultValue("")] public string IdentifierInContainer { get; set; } = string.Empty;
    
    /// <summary>Prefix for lines to be ignored</summary>
    [DefaultValue("#")] public string CommentLine { get; set; } = "#";
    
    /// <summary>Prefix for Escaping linefeed or qualifier</summary>
    [DefaultValue('\\')] public char EscapePrefix { get; set; } = '\\';
    
    /// <summary>Delimiter between two columns</summary>
    [DefaultValue(',')] public char FieldDelimiter { get; set; } = ',';
    
    /// <summary>Qualifier of a columns to allow linefeed or delimiter</summary>
    [DefaultValue('"')] public char FieldQualifier { get; set; } = '"';
    
    /// <summary>Context-sensitive quoting looks at eh surrounding area to determine if this is really a quote</summary>
    [DefaultValue(false)] public bool ContextSensitiveQualifier { get; set; }
    
    /// <summary>In case a quote is part of a quoted column, the quote should be repeated</summary>
    [DefaultValue(true)] public bool DuplicateQualifierToEscape { get; set; } = true;
    
    /// <summary>Does the file have a header row</summary>
    [DefaultValue(true)] public bool HasFieldHeader { get; set; } = true;
    
    /// <summary>Record Separator</summary>
    [DefaultValue(RecordDelimiterTypeEnum.None)] 
    public RecordDelimiterTypeEnum NewLine { get; set; } = RecordDelimiterTypeEnum.None;

    /// <summary>
    /// The file name
    /// </summary>
    [JsonIgnore]
    [DefaultValue("")]
    public string FileName { get; set;  } = string.Empty;

    /// <summary>
    /// Flag to indicate that it's a Json file
    /// </summary>    
    [DefaultValue(false)]
    public bool IsJson { get; set; } = false;

    /// <summary>
    /// Flag to indicate that it's an XML file
    /// </summary>    
    [DefaultValue(false)]
    public bool IsXml { get; set; } = false;

    /// <summary>
    /// Flag to indicate that it's not a delimiter, Json or XMl file
    /// </summary>    
    [DefaultValue(false)]
    public bool NoDelimitedFile { get; set; } = false;

    /// <summary>
    /// File containing Column definitions
    /// </summary>    
    [DefaultValue("")]
    public string ColumnFile { get; } = string.Empty;

    /// <summary>
    /// The identified columns
    /// </summary>    
    public ColumnCollection Columns { get; } = new ColumnCollection();

    /// <summary>
    /// Initializes a new instance of the <see cref="InspectionResult"/> class.
    /// </summary>
    public InspectionResult()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InspectionResult"/> class.
    /// </summary>
    /// <param name="fileSetting">The setting.</param>
    public InspectionResult(IFileSetting fileSetting)
    {
      SkipRows = fileSetting.SkipRows;
      Columns.AddRange(fileSetting.ColumnCollection);

      if (fileSetting is IFileSettingPhysicalFile physical)
      {
        FileName = physical.FullPath;
        CodePageId = physical.CodePageId;
        ByteOrderMark = physical.ByteOrderMark;
        IdentifierInContainer = physical.IdentifierInContainer;
        HasFieldHeader = fileSetting.HasFieldHeader;
      }
      if (fileSetting is ICsvFile csvFile)
      {
        CommentLine = csvFile.CommentLine;
        EscapePrefix = csvFile.EscapePrefixChar;
        FieldQualifier = csvFile.FieldQualifierChar;
        FieldDelimiter = csvFile.FieldDelimiterChar;
        ContextSensitiveQualifier = csvFile.ContextSensitiveQualifier;
        DuplicateQualifierToEscape = csvFile.DuplicateQualifierToEscape;
        NewLine = csvFile.NewLine;
      }
    }

    /// <summary>
    /// Copy Inspection results to setting
    /// </summary>
    /// <param name="fileSetting"></param>
    public void CopyToCsv(IFileSettingPhysicalFile fileSetting)
    {
      fileSetting.FileName = FileName;
      fileSetting.SkipRows = SkipRows;
      fileSetting.CodePageId = CodePageId;
      fileSetting.ByteOrderMark = ByteOrderMark;
      fileSetting.IdentifierInContainer = IdentifierInContainer;
      fileSetting.HasFieldHeader = HasFieldHeader;
      fileSetting.ColumnCollection.Clear();
      fileSetting.ColumnCollection.AddRange(Columns);
      if (fileSetting is ICsvFile csvFile)
      {
        csvFile.CommentLine = CommentLine;
        csvFile.EscapePrefixChar = EscapePrefix;
        csvFile.FieldDelimiterChar = FieldDelimiter;
        csvFile.FieldQualifierChar = FieldQualifier;
        csvFile.ContextSensitiveQualifier = ContextSensitiveQualifier;
        csvFile.DuplicateQualifierToEscape = DuplicateQualifierToEscape;
        csvFile.NewLine = NewLine;
      }
    }

  }
}