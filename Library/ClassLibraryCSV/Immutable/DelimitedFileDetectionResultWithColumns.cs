using System.Collections.Generic;

namespace CsvTools
{
  public class DelimitedFileDetectionResultWithColumns : DelimitedFileDetectionResult
  {
#if !QUICK
    private readonly string m_ColumnFile;
#endif
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
      RecordDelimiterType recordDelimiterType = RecordDelimiterType.None,
      in IEnumerable<IColumn>? columns = null
#if !QUICK
      , string? columnFile = ""
#endif
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
      m_Columns = columns ?? new List<IColumn>();
#if !QUICK
      m_ColumnFile = columnFile ?? string.Empty;
#endif
    }

    public DelimitedFileDetectionResultWithColumns(
      DelimitedFileDetectionResult result,
      in IEnumerable<IColumn>? columns = null
#if !QUICK
      , string columnFile = ""
#endif
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
        columns
#if !QUICK
        , columnFile
#endif
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