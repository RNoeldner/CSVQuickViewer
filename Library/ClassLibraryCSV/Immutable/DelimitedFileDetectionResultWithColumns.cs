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
using System.Collections.Generic;
// ReSharper disable NotAccessedField.Local

namespace CsvTools
{
  public sealed class DelimitedFileDetectionResultWithColumns : DelimitedFileDetectionResult
  {

    private readonly string m_ColumnFile;
    private readonly IEnumerable<Column> m_Columns;

    public DelimitedFileDetectionResultWithColumns(
      in string fileName,
      int skipRows = 0,
      int codePageId = -1,
      bool byteOrderMark = false,
      bool qualifyAlways = false,
      in string identifierInContainer = "",
      in string commentLine = "#",
      in string? escapePrefix = "\\",
      in string? fieldDelimiter = "",
      in string? fieldQualifier = "",
      bool contextQualifier = false,
      bool duplicateQualifierToEscape = true,
      bool hasFieldHeader = true,
      bool isJson = false,
      bool noDelimitedFile = false,
      RecordDelimiterTypeEnum recordDelimiterType = RecordDelimiterTypeEnum.None,
      in IEnumerable<Column>? columns = null,
      string? columnFile = ""
    )
      : base(
        fileName,
        skipRows,
        codePageId,
        byteOrderMark,
        qualifyAlways,
        identifierInContainer,
        commentLine,
        escapePrefix,
        fieldDelimiter,
        fieldQualifier,
        contextQualifier,
        duplicateQualifierToEscape,
        hasFieldHeader,
        isJson,
        noDelimitedFile,
        recordDelimiterType)
    {
      m_Columns = columns ?? Array.Empty<Column>();
      m_ColumnFile = columnFile ?? string.Empty;
    }

    public DelimitedFileDetectionResultWithColumns(
      DelimitedFileDetectionResult result,
      in IEnumerable<Column>? columns = null,
      string columnFile = ""
    )
      : this(
        result.FileName,
        result.SkipRows,
        result.CodePageId,
        result.ByteOrderMark,
        result.QualifyAlways,
        result.IdentifierInContainer,
        result.CommentLine,
        result.EscapePrefix,
        result.FieldDelimiter,
        result.FieldQualifier,
        result.QualifierInContext,
        result.DuplicateQualifierToEscape,        
        result.HasFieldHeader,
        result.IsJson,
        result.NoDelimitedFile,
        result.NewLine,
        columns,
        columnFile
      )
    {
    }

#if !QUICK
    public override IFileSettingPhysicalFile PhysicalFile()
    {
      var ret = base.PhysicalFile();
      foreach (var col in m_Columns)
        ret.ColumnCollection.Add(col);
      ret.ColumnFile = m_ColumnFile;
      return ret;
    }
#endif
  }
}