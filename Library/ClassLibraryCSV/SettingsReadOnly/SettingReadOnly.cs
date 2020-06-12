using System;
using System.Collections.Generic;

namespace CsvTools
{
  public class SettingReadOnly
  {
    public readonly int ConsecutiveEmptyRows;
    public readonly bool DisplayEndLineNo;
    public readonly bool DisplayRecordNo;
    public readonly bool DisplayStartLineNo;
    public readonly IReadOnlyCollection<SampleRecordEntry> Errors;
    public readonly int EvidenceNumberOrIssues;
    public readonly FileFormatReadOnly FileFormat;
    public readonly string FileName;
    public readonly long FileSize;
    public readonly string Footer;
    public readonly string FullPath;
    public readonly bool HasFieldHeader;
    public readonly string Header;
    public readonly string Id;
    public readonly DateTime LatestSourceTimeUtc;
    public readonly string Passphrase;
    public readonly DateTime ProcessTimeUtc;
    public readonly string Recipient;
    public readonly long RecordLimit;
    public readonly string RemoteFileName;
    public readonly IReadOnlyCollection<SampleRecordEntry> Samples;
    public readonly bool SetLatestSourceTimeForWrite;
    public readonly bool SkipDuplicateHeader;
    public readonly bool SkipEmptyLines;
    public readonly int SkipRows;
    public readonly IReadOnlyCollection<IFileSetting> SourceFileSettings;
    public readonly string SqlStatement;
    public readonly string TemplateName;
    public readonly bool ThrowErrorIfNotExists;
    public readonly int Timeout;
    public readonly bool TreatNbspAsSpace;
    public readonly string TreatTextAsNull;
    public readonly TrimmingOption TrimmingOption;
    public ICollection<ColumnReadOnly> ColumnCollection;


    public readonly bool AllowRowCombining;
    public readonly bool ByteOrderMark;
    public readonly int CodePageId;
    public readonly bool JsonFormat;
    public readonly bool NoDelimitedFile;
    public readonly bool TreatLFAsSpace;
    public readonly bool TreatUnknownCharacterAsSpace;
    public readonly bool TryToSolveMoreColumns;
    public readonly bool WarnDelimiterInValue;
    public readonly bool WarnEmptyTailingColumns;
    public readonly bool WarnLineFeed;
    public readonly bool WarnNbsp;
    public readonly bool WarnQuotes;
    public readonly bool WarnQuotesInQuotes;
    public readonly bool WarnUnknownCharacter;

    public SettingReadOnly(int consecutiveEmptyRows, bool displayEndLineNo, bool displayRecordNo, bool displayStartLineNo,
      IReadOnlyCollection<SampleRecordEntry> errors, int evidenceNumberOrIssues, FileFormatReadOnly fileFormat,
      string fileName, long fileSize, string footer, string fullPath, bool hasFieldHeader, string header, string id,
      DateTime latestSourceTimeUtc, string passphrase, DateTime processTimeUtc, string recipient, long recordLimit,
      string remoteFileName, IReadOnlyCollection<SampleRecordEntry> samples, bool setLatestSourceTimeForWrite,
      bool skipDuplicateHeader, bool skipEmptyLines, int skipRows, IReadOnlyCollection<IFileSetting> sourceFileSettings,
      string sqlStatement, string templateName, bool throwErrorIfNotExists, int timeout, bool treatNbspAsSpace,
      string treatTextAsNull, TrimmingOption trimmingOption, ICollection<ColumnReadOnly> columnCollection, bool allowRowCombining, bool byteOrderMark, int codePageId, bool jsonFormat, bool noDelimitedFile, bool treatLFAsSpace, bool treatUnknownCharacterAsSpace, bool tryToSolveMoreColumns, bool warnDelimiterInValue, bool warnEmptyTailingColumns, bool warnLineFeed, bool warnNbsp, bool warnQuotes, bool warnQuotesInQuotes, bool warnUnknownCharacter)
    {
      FileFormat = fileFormat;
      ConsecutiveEmptyRows = consecutiveEmptyRows;
      DisplayEndLineNo = displayEndLineNo;
      DisplayRecordNo = displayRecordNo;
      DisplayStartLineNo = displayStartLineNo;
      SetLatestSourceTimeForWrite = setLatestSourceTimeForWrite;
      ProcessTimeUtc = processTimeUtc;
      LatestSourceTimeUtc = latestSourceTimeUtc;
      FileName = fileName;
      FileSize = fileSize;
      Footer = footer;
      FullPath = fullPath;
      HasFieldHeader = hasFieldHeader;
      Header = header;
      Id = id;
      EvidenceNumberOrIssues = evidenceNumberOrIssues;
      Passphrase = passphrase;
      Recipient = recipient;
      RecordLimit = recordLimit;
      RemoteFileName = remoteFileName;
      ThrowErrorIfNotExists = throwErrorIfNotExists;
      SkipRows = skipRows;
      SqlStatement = sqlStatement;
      Timeout = timeout;
      TemplateName = templateName;
      TreatNbspAsSpace = treatNbspAsSpace;
      TreatTextAsNull = treatTextAsNull;
      SkipDuplicateHeader = skipDuplicateHeader;
      SkipEmptyLines = skipEmptyLines;
      Samples = samples;
      Errors = errors;
      TrimmingOption = trimmingOption;
      SourceFileSettings = sourceFileSettings;
      ColumnCollection = columnCollection;


      AllowRowCombining =allowRowCombining;
      ByteOrderMark =byteOrderMark;
      CodePageId =codePageId;
      JsonFormat =jsonFormat;
      NoDelimitedFile =noDelimitedFile;
      TreatLFAsSpace =treatLFAsSpace;
      TreatUnknownCharacterAsSpace =treatUnknownCharacterAsSpace;
      TryToSolveMoreColumns =tryToSolveMoreColumns;
      WarnDelimiterInValue =warnDelimiterInValue;
      WarnEmptyTailingColumns =warnEmptyTailingColumns;
      WarnLineFeed =warnLineFeed;
      WarnNbsp =warnNbsp;
      WarnQuotes =warnQuotes;
      WarnQuotesInQuotes =warnQuotesInQuotes;
      WarnUnknownCharacter =warnUnknownCharacter;
    }
  }
}