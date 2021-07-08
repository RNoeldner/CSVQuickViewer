using System.Collections.Generic;

namespace CsvTools
{
  public class DelimitedFileDetectionResultWithColumns : DelimitedFileDetectionResult
  {
    public readonly IEnumerable<IColumn> Columns;
    private readonly string m_ColumnFile;

    public DelimitedFileDetectionResultWithColumns(string fileName, int skipRows = 0, int codePageId = -1, bool byteOrderMark = false, bool qualifyAlways = false, string identifierInContainer = "", string commentLine = "#",
            string? escapeCharacter = "\\", string? fieldDelimiter = "", string? fieldQualifier = "", bool hasFieldHeader = true, bool isJson = false, bool noDelimitedFile = false, RecordDelimiterType recordDelimiterType = RecordDelimiterType.None,
            IEnumerable<IColumn>? columns = null, string? columnFile = "") :
        base(fileName, skipRows, codePageId, byteOrderMark, qualifyAlways, identifierInContainer, commentLine, escapeCharacter, fieldDelimiter, fieldQualifier, hasFieldHeader, isJson, noDelimitedFile, recordDelimiterType)
    {
      Columns = columns ?? new List<IColumn>();
      m_ColumnFile = columnFile ?? string.Empty;
    }

    public DelimitedFileDetectionResultWithColumns(DelimitedFileDetectionResult result, IEnumerable<IColumn>? columns = null, string columnFile = "") :
       this(result.FileName, result.SkipRows, result.CodePageId, result.ByteOrderMark, result.QualifyAlways, result.IdentifierInContainer, result.CommentLine, result.EscapeCharacter, result.FieldDelimiter, result.FieldQualifier, result.HasFieldHeader, result.IsJson, result.NoDelimitedFile, result.NewLine, columns, columnFile)
    {
    }
#if !QUICK
    public override ICsvFile CsvFile()
    {
      var ret = base.CsvFile();
      foreach (var col in Columns)
        ret.ColumnCollection.Add(col);
      ret.ColumnFile = m_ColumnFile;
      return ret;
    }
#endif
  }
}