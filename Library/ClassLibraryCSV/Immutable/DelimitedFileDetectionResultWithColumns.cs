#nullable enable
using System;
using System.Collections.Generic;
// ReSharper disable NotAccessedField.Local

namespace CsvTools
{
  public class DelimitedFileDetectionResultWithColumns : DelimitedFileDetectionResult
  {
    
    private readonly string m_ColumnFile;
    private readonly IEnumerable<IColumn> m_Columns;

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
      bool hasFieldHeader = true,
      bool isJson = false,
      bool noDelimitedFile = false,
      RecordDelimiterTypeEnum recordDelimiterType = RecordDelimiterTypeEnum.None,
      in IEnumerable<IColumn>? columns = null,
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
        hasFieldHeader,
        isJson,
        noDelimitedFile,
        recordDelimiterType)
    {
      m_Columns = columns ?? Array.Empty<IColumn>();
      m_ColumnFile = columnFile ?? string.Empty;
    }

    public DelimitedFileDetectionResultWithColumns(
      DelimitedFileDetectionResult result,
      in IEnumerable<IColumn>? columns = null,
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