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
      in DelimitedFileDetectionResult result,
      in IEnumerable<Column>? columns = null,
      string? columnFile = ""
    ) : base(result.FileName)
    {
      SkipRows = result.SkipRows;
      CodePageId = result.CodePageId;
      ByteOrderMark = result.ByteOrderMark;
      QualifyAlways=result.QualifyAlways;
      IdentifierInContainer=result.IdentifierInContainer;
      CommentLine=result.CommentLine;
      EscapePrefix=result.EscapePrefix;
      FieldDelimiter=result.FieldDelimiter;
      FieldQualifier=result.FieldQualifier;
      QualifierInContext=result.QualifierInContext;
      DuplicateQualifierToEscape=result.DuplicateQualifierToEscape;
      HasFieldHeader=result.HasFieldHeader;
      IsJson=result.IsJson;
      NoDelimitedFile=result.NoDelimitedFile;
      NewLine= result.NewLine;
      m_Columns = columns ?? Array.Empty<Column>();
      m_ColumnFile = columnFile ?? string.Empty;
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