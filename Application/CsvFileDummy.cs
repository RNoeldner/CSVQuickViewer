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


using System.ComponentModel;

namespace CsvTools
{
  /// <inheritdoc cref="CsvTools.ICsvFile" />
  /// <summary>
  ///   setting for CSV files
  /// </summary>
  public sealed class CsvFileDummy : ICsvFile
  {
    /// <summary>
    /// Gets or sets a value indicating whether this file is a Json file
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is json; otherwise, <c>false</c>.
    /// </value>
    public bool IsJson { get; set; }

    /// <summary>
    /// Gets a value indicating whether this instance is delimited file
    /// </summary>
    public bool IsCsv => !IsJson && !IsXml;

    /// <summary>
    /// Gets or sets a value indicating whether this file is a XML file
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is XML; otherwise, <c>false</c>.
    /// </value>
    public bool IsXml { get; set; }

    /// <inheritdoc />
    public bool AllowRowCombining { get; set; }

    /// <inheritdoc />
    public string CommentLine { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool ContextSensitiveQualifier { get; set; }

    /// <inheritdoc />
    public string DelimiterPlaceholder { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool DuplicateQualifierToEscape { get; set; } = true;

    /// <inheritdoc />
    [DefaultValue('\0')]
    public char EscapePrefixChar { get; set; } = '\0';

    /// <inheritdoc />
    [DefaultValue(',')]
    public char FieldDelimiterChar { get; set; } = ',';

    /// <inheritdoc />
    [DefaultValue('"')]
    public char FieldQualifierChar { get; set; } = '"';

    /// <inheritdoc />
    [DefaultValue(RecordDelimiterTypeEnum.Crlf)]
    public RecordDelimiterTypeEnum NewLine { get; set; } = RecordDelimiterTypeEnum.Crlf;

    /// <inheritdoc />
    [DefaultValue("")]
    public string NewLinePlaceholder { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool NoDelimitedFile { get; set; }

    /// <inheritdoc />
    [DefaultValue(0)]
    public int NumWarnings { get; set; }

    /// <inheritdoc />
    [DefaultValue("")]
    public string QualifierPlaceholder { get; set; } = string.Empty;

    /// <inheritdoc />
    [DefaultValue(false)]
    public bool QualifyAlways { get; set; }

    /// <inheritdoc />
    [DefaultValue(true)]
    public bool QualifyOnlyIfNeeded { get; set; } = true;

    /// <inheritdoc />
    [DefaultValue(false)]
    public bool TreatLfAsSpace { get; set; }

    /// <inheritdoc />
    [DefaultValue(false)]
    public bool TreatUnknownCharacterAsSpace { get; set; }

    /// <inheritdoc />
    [DefaultValue(TrimmingOptionEnum.Unquoted)]
    public TrimmingOptionEnum TrimmingOption { get; set; } = TrimmingOptionEnum.Unquoted;

    /// <inheritdoc />
    [DefaultValue(false)]
    public bool TryToSolveMoreColumns { get; set; }

    /// <inheritdoc />
    public bool WarnDelimiterInValue { get; set; }

    /// <inheritdoc />
    [DefaultValue(true)]
    public bool WarnEmptyTailingColumns { get; set; } = true;

    /// <inheritdoc />
    [DefaultValue(false)]
    public bool WarnLineFeed { get; set; }

    /// <inheritdoc />
    [DefaultValue(true)]
    public bool WarnNBSP { get; set; } = true;

    /// <inheritdoc />
    [DefaultValue(false)]
    public bool WarnQuotes { get; set; }

    /// <inheritdoc />
    [DefaultValue(true)]
    public bool WarnQuotesInQuotes { get; set; } = true;

    /// <inheritdoc />
    [DefaultValue(true)]
    public bool WarnUnknownCharacter { get; set; } = true;

    /// <inheritdoc />
    [DefaultValue(false)]
    public bool WriteFixedLength { get; set; }

    /// <inheritdoc cref="IWithCopyTo{T}" />
    public bool Equals(ICsvFile? other) => ReferenceEquals(this, other);

    #region IFileSettingPhysicalFile

    private string? m_FullPath;

    /// <inheritdoc />
    [DefaultValue(true)]
    public bool ByteOrderMark { get; set; } = true;

    /// <inheritdoc />
    [DefaultValue(65001)]
    public int CodePageId { get; set; } = 65001;

    /// <inheritdoc />
    [DefaultValue("")]
    public string ColumnFile { get; set; } = string.Empty;

    /// <inheritdoc />
    [DefaultValue("")]
    public string FileName { get; set; } = string.Empty;

    /// <inheritdoc />
    [DefaultValue(0)]
    public long FileSize { get; set; }

    /// <inheritdoc />
    public string FullPath => m_FullPath ?? FileName;

    /// <inheritdoc />
    [DefaultValue("")] public string IdentifierInContainer { get; set; } = string.Empty;

    /// <inheritdoc />
    [DefaultValue("")] public string KeyFile { get; set; } = string.Empty;

    /// <inheritdoc />
    [DefaultValue("")] public string Passphrase { get; set; } = string.Empty;

    /// <inheritdoc />
    [DefaultValue("")] public string RemoteFileName { get; set; } = string.Empty;

    /// <inheritdoc />
    [DefaultValue("")] public string RootFolder { get; set; } = string.Empty;

    /// <inheritdoc />
    [DefaultValue(false)]  public bool SetLatestSourceTimeForWrite { get; set; }

    /// <inheritdoc />
    [DefaultValue(false)] public bool ThrowErrorIfNotExists { get; set; }

    /// <inheritdoc />
    public ValueFormat ValueFormatWrite { get; set; } = ValueFormat.Empty;

    /// <inheritdoc />
    public void ResetFullPath() => m_FullPath = FileName.FullPath(RootFolder);

    #endregion IFileSettingPhysicalFile

    #region IFileSetting

    /// <inheritdoc />
    public ColumnCollection ColumnCollection { get; } = new ColumnCollection();

    /// <inheritdoc />
    [DefaultValue(5)] public int ConsecutiveEmptyRows { get; set; } = 5;

    /// <inheritdoc />
    [DefaultValue(false)] public bool DisplayEndLineNo { get; set; }

    /// <inheritdoc />
    [DefaultValue(false)] public bool DisplayRecordNo { get; set; }

    /// <inheritdoc />
    [DefaultValue(true)] public bool DisplayStartLineNo { get; set; } = true;

    /// <inheritdoc />
    [DefaultValue("")] public string Footer { get; set; } = string.Empty;

    /// <inheritdoc />
    [DefaultValue(true)] public bool HasFieldHeader { get; set; } = true;

    /// <inheritdoc />
    [DefaultValue("")] public string Header { get; set; } = string.Empty;

    /// <inheritdoc />
    [DefaultValue(false)] public bool KeepUnencrypted { get; set; }

    /// <inheritdoc />
    [DefaultValue(0)] public long RecordLimit { get; set; }

    /// <inheritdoc />
    [DefaultValue(true)] public bool SkipDuplicateHeader { get; set; } = true;

    /// <inheritdoc />
    [DefaultValue(true)] public bool SkipEmptyLines { get; set; } = true;

    /// <inheritdoc />
    [DefaultValue(0)] public int SkipRows { get; set; }

    /// <inheritdoc />
    [DefaultValue(false)] public bool TreatNBSPAsSpace { get; set; }

    /// <inheritdoc />
    [DefaultValue("NULL")] public string TreatTextAsNull { get; set; } = "NULL";

    /// <inheritdoc />
    [DefaultValue(false)] public bool Trim { get; set; } = false;

    /// <inheritdoc />
    public object Clone()
    {
      var res = new CsvFileDummy();
      CopyTo(res);
      return res;
    }

    /// <inheritdoc />
    public void CopyTo(IFileSetting target)
    {
      target.DisplayEndLineNo = DisplayEndLineNo;
      target.DisplayRecordNo = DisplayRecordNo;
      target.DisplayStartLineNo = DisplayStartLineNo;
      target.ColumnCollection.Clear();
      target.ColumnCollection.AddRange(ColumnCollection);
      target.ConsecutiveEmptyRows = ConsecutiveEmptyRows;
      target.Footer = Footer;
      target.HasFieldHeader = HasFieldHeader;
      target.Header = Header;
      target.KeepUnencrypted = KeepUnencrypted;
      target.RecordLimit = RecordLimit;
      target.SkipDuplicateHeader = SkipDuplicateHeader;
      target.SkipEmptyLines = SkipEmptyLines;
      target.SkipRows = SkipRows;
      target.TreatNBSPAsSpace = TreatNBSPAsSpace;
      target.TreatTextAsNull = TreatTextAsNull;
      target.Trim = Trim;

      if (target is ICsvFile other)
      {
        other.AllowRowCombining = AllowRowCombining;
        other.ByteOrderMark = ByteOrderMark;
        other.CodePageId = CodePageId;
        other.ColumnFile = ColumnFile;
        other.CommentLine = CommentLine;
        other.ContextSensitiveQualifier = ContextSensitiveQualifier;
        other.DelimiterPlaceholder = DelimiterPlaceholder;
        other.DuplicateQualifierToEscape = DuplicateQualifierToEscape;
        other.EscapePrefixChar = EscapePrefixChar;
        other.FieldDelimiterChar = FieldDelimiterChar;
        other.FieldQualifierChar = FieldQualifierChar;
        other.RootFolder = RootFolder;
        other.FileName = FileName;
        other.FileSize = FileSize;
        other.IdentifierInContainer = IdentifierInContainer;
        other.KeyFile = KeyFile;
        other.NewLinePlaceholder = NewLinePlaceholder;
        other.NoDelimitedFile = NoDelimitedFile;
        other.NumWarnings = NumWarnings;
        other.Passphrase = Passphrase;
        other.QualifierPlaceholder = QualifierPlaceholder;
        other.QualifyAlways = QualifyAlways;
        other.QualifyOnlyIfNeeded = QualifyOnlyIfNeeded;
        other.NewLine = NewLine;
        other.RemoteFileName = RemoteFileName;
        other.SetLatestSourceTimeForWrite = SetLatestSourceTimeForWrite;
        other.ThrowErrorIfNotExists = ThrowErrorIfNotExists;
        other.TreatLfAsSpace = TreatLfAsSpace;
        other.TreatUnknownCharacterAsSpace = TreatUnknownCharacterAsSpace;
        other.TrimmingOption = TrimmingOption;
        other.TryToSolveMoreColumns = TryToSolveMoreColumns;
        other.ValueFormatWrite = ValueFormatWrite;
        other.WarnDelimiterInValue = WarnDelimiterInValue;
        other.WarnEmptyTailingColumns = WarnEmptyTailingColumns;
        other.WarnLineFeed = WarnLineFeed;
        other.WarnNBSP = WarnNBSP;
        other.WarnQuotes = WarnQuotes;
        other.WarnQuotesInQuotes = WarnQuotesInQuotes;
        other.WarnUnknownCharacter = WarnUnknownCharacter;
        other.WriteFixedLength = WriteFixedLength;
      }
      if (target is CsvFileDummy fileDummy)
      {        
        fileDummy.IsJson = IsJson;
        fileDummy.IsXml = IsXml;
      }
    }

    /// <inheritdoc />
    public bool Equals(IFileSetting? other2)
    {
      if (other2 == null)
        return false;
      if (ReferenceEquals(this, other2))
        return true;
      if (!(other2 is ICsvFile other))
        return false;
      return
        other.AllowRowCombining  == AllowRowCombining &&
        other.ByteOrderMark  == ByteOrderMark &&
        other.CodePageId  == CodePageId &&
        other.ColumnCollection.CollectionEqualWithOrder(ColumnCollection) &&
        other.ColumnFile  == ColumnFile &&
        other.CommentLine  == CommentLine &&
        other.ConsecutiveEmptyRows  == ConsecutiveEmptyRows &&
        other.ContextSensitiveQualifier  == ContextSensitiveQualifier &&
        other.DelimiterPlaceholder  == DelimiterPlaceholder &&
        other.DisplayEndLineNo  == DisplayEndLineNo &&
        other.DisplayRecordNo  == DisplayRecordNo &&
        other.DisplayStartLineNo  == DisplayStartLineNo &&
        other.DuplicateQualifierToEscape  == DuplicateQualifierToEscape &&
        other.EscapePrefixChar  == EscapePrefixChar &&
        other.FieldDelimiterChar  == FieldDelimiterChar &&
        other.FieldQualifierChar  == FieldQualifierChar &&
        other.FileName  == FileName &&
        other.FileSize  == FileSize &&
        other.Footer  == Footer &&
        other.HasFieldHeader  == HasFieldHeader &&
        other.Header  == Header &&
        other.IdentifierInContainer  == IdentifierInContainer &&
        other.KeepUnencrypted  == KeepUnencrypted &&
        other.KeyFile  == KeyFile &&
        other.NewLinePlaceholder  == NewLinePlaceholder &&
        other.NoDelimitedFile  == NoDelimitedFile &&
        other.NumWarnings  == NumWarnings &&
        other.Passphrase  == Passphrase &&
        other.QualifierPlaceholder  == QualifierPlaceholder &&
        other.QualifyAlways  == QualifyAlways &&
        other.QualifyOnlyIfNeeded  == QualifyOnlyIfNeeded &&
        other.NewLine  == NewLine &&
        other.RecordLimit == RecordLimit &&
        other.RemoteFileName == RemoteFileName &&
        other.RootFolder == RootFolder &&
        other.SetLatestSourceTimeForWrite == SetLatestSourceTimeForWrite &&
        other.SkipDuplicateHeader == SkipDuplicateHeader &&
        other.SkipEmptyLines == SkipEmptyLines &&
        other.SkipRows == SkipRows &&
        other.ThrowErrorIfNotExists == ThrowErrorIfNotExists &&
        other.TreatLfAsSpace == TreatLfAsSpace &&
        other.TreatNBSPAsSpace == TreatNBSPAsSpace &&
        other.TreatTextAsNull == TreatTextAsNull &&
        other.TreatUnknownCharacterAsSpace == TreatUnknownCharacterAsSpace &&
        other.Trim == Trim &&
        other.TrimmingOption == TrimmingOption &&
        other.TryToSolveMoreColumns == TryToSolveMoreColumns &&
        other.ValueFormatWrite.Equals(ValueFormatWrite) &&
        other.WarnDelimiterInValue == WarnDelimiterInValue &&
        other.WarnEmptyTailingColumns == WarnEmptyTailingColumns &&
        other.WarnLineFeed == WarnLineFeed &&
        other.WarnNBSP == WarnNBSP &&
        other.WarnQuotes == WarnQuotes &&
        other.WarnQuotesInQuotes == WarnQuotesInQuotes &&
        other.WarnUnknownCharacter == WarnUnknownCharacter &&
        other.WriteFixedLength == WriteFixedLength;
    }

    /// <inheritdoc />
    public string GetDisplay() => "CSV";

    #endregion IFileSetting
  }
}