namespace CsvTools
{
  public class FileFormatReadOnly
  {
    public readonly bool AlternateQuoting;
    public readonly string CommentLine;
    public readonly string DelimiterPlaceholder;
    public readonly bool DuplicateQuotingToEscape;
    public readonly char EscapeCharacterChar;
    public readonly char FieldDelimiterChar;
    public readonly char FieldQualifierChar;
    public readonly RecordDelimiterType NewLine;
    public readonly string NewLinePlaceholder;
    public readonly bool QualifyAlways;
    public readonly bool QualifyOnlyIfNeeded;
    public readonly string QuotePlaceholder;
    public readonly ValueFormatReadOnly ValueFormat;

    public FileFormatReadOnly(bool alternateQuoting, string commentLine, string delimiterPlaceholder,
      bool duplicateQuotingToEscape, char escapeCharacterChar, char fieldDelimiterChar, char fieldQualifierChar,
      RecordDelimiterType newLine, string newLinePlaceholder, bool qualifyAlways, bool qualifyOnlyIfNeeded,
      string quotePlaceholder, ValueFormatReadOnly valueFormat)
    {
      AlternateQuoting = alternateQuoting;
      CommentLine = commentLine;
      DelimiterPlaceholder = delimiterPlaceholder;
      DuplicateQuotingToEscape = duplicateQuotingToEscape;
      EscapeCharacterChar = escapeCharacterChar;
      FieldDelimiterChar = fieldDelimiterChar;
      FieldQualifierChar = fieldQualifierChar;
      NewLine = newLine;
      NewLinePlaceholder = newLinePlaceholder;
      QualifyAlways = qualifyAlways;
      QualifyOnlyIfNeeded = qualifyOnlyIfNeeded;
      QuotePlaceholder = quotePlaceholder;
      ValueFormat = valueFormat;
    }
  }
}