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

using System.Collections.Generic;

namespace CsvTools
{
  public class DelimitedFileDetectionResult
  {
    public bool ByteOrderMark;
    public int CodePageId = -1;
    public string CommentLine = "#";
    public char EscapeCharacterChar = '\\';
    public string FieldDelimiter = string.Empty;
    public string FieldQualifier = string.Empty;
    public bool HasFieldHeader = true;
    public bool IsJson;
    public RecordDelimiterType NewLine = RecordDelimiterType.None;
    public bool NoDelimitedFile;
    public int SkipRows;
    public string IdentifierInContainer = string.Empty;
    public readonly string FileName;
    public bool QualifyAlways = false;

    public DelimitedFileDetectionResult(string fileName, int skipRows = 0, int codePageId = -1, bool byteOrderMark = false, bool qualifyAlways = false,
string identifierInContainer = null, string commentLine = "#", char escapeCharacterChar = '\\', string fieldDelimiter = null, string fieldQualifier = null, bool hasFieldHeader = true, bool isJson = false, bool noDelimitedFile = false)
    {
      FileName = fileName ?? throw new System.ArgumentNullException(nameof(fileName));
      IdentifierInContainer = identifierInContainer?? string.Empty;
      SkipRows = skipRows<1 ? 0 : skipRows;
      CodePageId = codePageId<1 ? -1 : codePageId;
      ByteOrderMark= byteOrderMark;
      CommentLine = commentLine;
      EscapeCharacterChar= escapeCharacterChar;
      FieldDelimiter = fieldDelimiter?? string.Empty;
      FieldQualifier = fieldQualifier?? string.Empty;
      HasFieldHeader = hasFieldHeader;
      IsJson = isJson;
      NoDelimitedFile = noDelimitedFile;
      QualifyAlways= qualifyAlways;
    }

    public DelimitedFileDetectionResult(ICsvFile fileSettingSer) : this(fileSettingSer.FileName, fileSettingSer.SkipRows, fileSettingSer.CodePageId, fileSettingSer.ByteOrderMark, fileSettingSer.FileFormat.QualifyAlways, fileSettingSer.IdentifierInContainer, fileSettingSer.FileFormat.CommentLine,
fileSettingSer.FileFormat.EscapeCharacterChar, fileSettingSer.FileFormat.FieldDelimiter, fileSettingSer.FileFormat.FieldQualifier, fileSettingSer.HasFieldHeader, fileSettingSer.JsonFormat, fileSettingSer.NoDelimitedFile)
    {
    }

    public ICsvFile CsvFile(IEnumerable<Column> columns)
    {
      var ret = new CsvFile(FileName)
      {
        FileFormat = new FileFormat()
        {
          QualifyAlways= QualifyAlways,
          CommentLine = CommentLine,
          EscapeCharacter= EscapeCharacterChar.ToString(),
          FieldDelimiter = FieldDelimiter,
          FieldQualifier = FieldQualifier,
          NewLine =  NewLine
        },
        ByteOrderMark = ByteOrderMark,
        CodePageId = CodePageId,
        HasFieldHeader = HasFieldHeader,
        JsonFormat= IsJson,
        NoDelimitedFile = NoDelimitedFile,
        SkipRows = SkipRows
      };
      if (columns!= null)
        foreach (var col in columns)
          ret.ColumnCollection.Add(col);
      return ret;
    }
  }
}