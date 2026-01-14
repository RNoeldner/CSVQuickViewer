/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace CsvTools;

/// <inheritdoc cref="CsvTools.ICsvFile" />
/// <summary>
///   setting for CSV files
/// </summary>
public sealed class CsvFileDummy : ICsvFile, INotifyPropertyChanged
{
  private bool m_AllowRowCombining;

  private bool m_ByteOrderMark = true;

  private int m_CodePageId = 65001;

  private string m_ColumnFile = string.Empty;

  private string m_CommentLine = string.Empty;

  private int m_ConsecutiveEmptyRows = 5;

  private bool m_ContextSensitiveQualifier;

  private string m_DelimiterPlaceholder = string.Empty;

  private bool m_DisplayEndLineNo;

  private bool m_DisplayRecordNo;

  private bool m_DisplayStartLineNo = true;

  private bool m_DuplicateQualifierToEscape = true;

  private char m_EscapePrefixChar = '\0';

  private char m_FieldDelimiterChar = ',';

  private char m_FieldQualifierChar = '"';

  private string m_FileName = string.Empty;

  private long m_FileSize;

  private string m_Footer = string.Empty;

  private string? m_FullPath;

  private bool m_HasFieldHeader = true;

  private string m_Header = string.Empty;

  private string m_IdentifierInContainer = string.Empty;

  private bool m_IsJson;

  private bool m_IsXml;

  private bool m_KeepUnencrypted;

  private string m_KeyFile = string.Empty;

  private RecordDelimiterTypeEnum m_NewLine = RecordDelimiterTypeEnum.Crlf;

  private string m_NewLinePlaceholder = string.Empty;

  private bool m_NoDelimitedFile;

  private int m_NumWarnings;

  private string m_Passphrase = string.Empty;

  private string m_QualifierPlaceholder = string.Empty;

  private bool m_QualifyAlways;

  private bool m_QualifyOnlyIfNeeded = true;

  private long m_RecordLimit;

  private string m_RemoteFileName = string.Empty;

  private DateTime? m_RemoteFileTimeUtc;

  private string m_RootFolder = string.Empty;

  private bool m_SetLatestSourceTimeForWrite;

  private bool m_SkipDuplicateHeader = true;

  private bool m_SkipEmptyLines = true;

  private int m_SkipRows;

  private int m_SkipRowsAfterHeader;

  private bool m_ThrowErrorIfNotExists;

  private bool m_TreatLfAsSpace;

  private bool m_TreatNBSPAsSpace;

  private string m_TreatTextAsNull = "NULL";

  private bool m_TreatUnknownCharacterAsSpace;

  private bool m_Trim;

  private TrimmingOptionEnum m_TrimmingOption = TrimmingOptionEnum.Unquoted;

  private bool m_TryToSolveMoreColumns;

  private ValueFormat m_ValueFormatWrite = ValueFormat.Empty;

  private bool m_WarnDelimiterInValue;

  private bool m_WarnEmptyTailingColumns = true;

  private bool m_WarnLineFeed;

  private bool m_WarnNBSP = true;

  private bool m_WarnQuotes;

  private bool m_WarnQuotesInQuotes = true;

  private bool m_WarnUnknownCharacter = true;

  private bool m_WriteFixedLength;

  public event PropertyChangedEventHandler? PropertyChanged;

  public bool AllowRowCombining
  {
    get => m_AllowRowCombining;
    set => SetField(ref m_AllowRowCombining, value);
  }

  public bool ByteOrderMark
  {
    get => m_ByteOrderMark;
    set => SetField(ref m_ByteOrderMark, value);
  }

  public int CodePageId
  {
    get => m_CodePageId;
    set => SetField(ref m_CodePageId, value);
  }

  public ColumnCollection ColumnCollection { get; } = new ColumnCollection();

  public string ColumnFile
  {
    get => m_ColumnFile;
    set => SetField(ref m_ColumnFile, value);
  }

  public string CommentLine
  {
    get => m_CommentLine;
    set => SetField(ref m_CommentLine, value);
  }

  public int ConsecutiveEmptyRows
  {
    get => m_ConsecutiveEmptyRows;
    set => SetField(ref m_ConsecutiveEmptyRows, value);
  }

  public bool ContextSensitiveQualifier
  {
    get => m_ContextSensitiveQualifier;
    set => SetField(ref m_ContextSensitiveQualifier, value);
  }

  public string DelimiterPlaceholder
  {
    get => m_DelimiterPlaceholder;
    set => SetField(ref m_DelimiterPlaceholder, value);
  }

  public bool DisplayEndLineNo
  {
    get => m_DisplayEndLineNo;
    set => SetField(ref m_DisplayEndLineNo, value);
  }

  public bool DisplayRecordNo
  {
    get => m_DisplayRecordNo;
    set => SetField(ref m_DisplayRecordNo, value);
  }

  public bool DisplayStartLineNo
  {
    get => m_DisplayStartLineNo;
    set => SetField(ref m_DisplayStartLineNo, value);
  }

  public bool DuplicateQualifierToEscape
  {
    get => m_DuplicateQualifierToEscape;
    set => SetField(ref m_DuplicateQualifierToEscape, value);
  }

  public char EscapePrefixChar
  {
    get => m_EscapePrefixChar;
    set => SetField(ref m_EscapePrefixChar, value);
  }

  public char FieldDelimiterChar
  {
    get => m_FieldDelimiterChar;
    set => SetField(ref m_FieldDelimiterChar, value);
  }

  public char FieldQualifierChar
  {
    get => m_FieldQualifierChar;
    set => SetField(ref m_FieldQualifierChar, value);
  }

  public string FileName
  {
    get => m_FileName;
    set => SetField(ref m_FileName, value);
  }

  public long FileSize
  {
    get => m_FileSize;
    set => SetField(ref m_FileSize, value);
  }

  public string Footer
  {
    get => m_Footer;
    set => SetField(ref m_Footer, value);
  }

  public string FullPath => m_FullPath ?? FileName;

  public bool HasFieldHeader
  {
    get => m_HasFieldHeader;
    set => SetField(ref m_HasFieldHeader, value);
  }

  public string Header
  {
    get => m_Header;
    set => SetField(ref m_Header, value);
  }

  public string IdentifierInContainer
  {
    get => m_IdentifierInContainer;
    set => SetField(ref m_IdentifierInContainer, value);
  }

  public bool IsCsv => !IsJson && !IsXml;

  public bool IsJson
  {
    get => m_IsJson;
    set => SetField(ref m_IsJson, value);
  }

  public bool IsXml
  {
    get => m_IsXml;
    set => SetField(ref m_IsXml, value);
  }

  public bool KeepUnencrypted
  {
    get => m_KeepUnencrypted;
    set => SetField(ref m_KeepUnencrypted, value);
  }

  public string KeyFile
  {
    get => m_KeyFile;
    set => SetField(ref m_KeyFile, value);
  }

  public RecordDelimiterTypeEnum NewLine
  {
    get => m_NewLine;
    set => SetField(ref m_NewLine, value);
  }

  public string NewLinePlaceholder
  {
    get => m_NewLinePlaceholder;
    set => SetField(ref m_NewLinePlaceholder, value);
  }

  public bool NoDelimitedFile
  {
    get => m_NoDelimitedFile;
    set => SetField(ref m_NoDelimitedFile, value);
  }

  public int NumWarnings
  {
    get => m_NumWarnings;
    set => SetField(ref m_NumWarnings, value);
  }

  public string Passphrase
  {
    get => m_Passphrase;
    set => SetField(ref m_Passphrase, value);
  }

  public string QualifierPlaceholder
  {
    get => m_QualifierPlaceholder;
    set => SetField(ref m_QualifierPlaceholder, value);
  }

  public bool QualifyAlways
  {
    get => m_QualifyAlways;
    set => SetField(ref m_QualifyAlways, value);
  }

  public bool QualifyOnlyIfNeeded
  {
    get => m_QualifyOnlyIfNeeded;
    set => SetField(ref m_QualifyOnlyIfNeeded, value);
  }

  public long RecordLimit
  {
    get => m_RecordLimit;
    set => SetField(ref m_RecordLimit, value);
  }

  public string RemoteFileName
  {
    get => m_RemoteFileName;
    set => SetField(ref m_RemoteFileName, value);
  }

  public DateTime? RemoteFileTimeUtc
  {
    get => m_RemoteFileTimeUtc;
    set => SetField(ref m_RemoteFileTimeUtc, value);
  }

  public string RootFolder
  {
    get => m_RootFolder;
    set => SetField(ref m_RootFolder, value);
  }

  public bool SetLatestSourceTimeForWrite
  {
    get => m_SetLatestSourceTimeForWrite;
    set => SetField(ref m_SetLatestSourceTimeForWrite, value);
  }

  public bool SkipDuplicateHeader
  {
    get => m_SkipDuplicateHeader;
    set => SetField(ref m_SkipDuplicateHeader, value);
  }

  public bool SkipEmptyLines
  {
    get => m_SkipEmptyLines;
    set => SetField(ref m_SkipEmptyLines, value);
  }

  public int SkipRows
  {
    get => m_SkipRows;
    set => SetField(ref m_SkipRows, value);
  }

  public int SkipRowsAfterHeader
  {
    get => m_SkipRowsAfterHeader;
    set => SetField(ref m_SkipRowsAfterHeader, value);
  }

  public bool ThrowErrorIfNotExists
  {
    get => m_ThrowErrorIfNotExists;
    set => SetField(ref m_ThrowErrorIfNotExists, value);
  }

  public bool TreatLfAsSpace
  {
    get => m_TreatLfAsSpace;
    set => SetField(ref m_TreatLfAsSpace, value);
  }

  public bool TreatNBSPAsSpace
  {
    get => m_TreatNBSPAsSpace;
    set => SetField(ref m_TreatNBSPAsSpace, value);
  }

  public string TreatTextAsNull
  {
    get => m_TreatTextAsNull;
    set => SetField(ref m_TreatTextAsNull, value);
  }

  public bool TreatUnknownCharacterAsSpace
  {
    get => m_TreatUnknownCharacterAsSpace;
    set => SetField(ref m_TreatUnknownCharacterAsSpace, value);
  }

  public bool Trim
  {
    get => m_Trim;
    set => SetField(ref m_Trim, value);
  }

  public TrimmingOptionEnum TrimmingOption
  {
    get => m_TrimmingOption;
    set => SetField(ref m_TrimmingOption, value);
  }

  public bool TryToSolveMoreColumns
  {
    get => m_TryToSolveMoreColumns;
    set => SetField(ref m_TryToSolveMoreColumns, value);
  }

  public ValueFormat ValueFormatWrite
  {
    get => m_ValueFormatWrite;
    set => SetField(ref m_ValueFormatWrite, value);
  }

  public bool WarnDelimiterInValue
  {
    get => m_WarnDelimiterInValue;
    set => SetField(ref m_WarnDelimiterInValue, value);
  }

  public bool WarnEmptyTailingColumns
  {
    get => m_WarnEmptyTailingColumns;
    set => SetField(ref m_WarnEmptyTailingColumns, value);
  }

  public bool WarnLineFeed
  {
    get => m_WarnLineFeed;
    set => SetField(ref m_WarnLineFeed, value);
  }

  public bool WarnNBSP
  {
    get => m_WarnNBSP;
    set => SetField(ref m_WarnNBSP, value);
  }

  public bool WarnQuotes
  {
    get => m_WarnQuotes;
    set => SetField(ref m_WarnQuotes, value);
  }

  public bool WarnQuotesInQuotes
  {
    get => m_WarnQuotesInQuotes;
    set => SetField(ref m_WarnQuotesInQuotes, value);
  }

  public bool WarnUnknownCharacter
  {
    get => m_WarnUnknownCharacter;
    set => SetField(ref m_WarnUnknownCharacter, value);
  }

  public bool WriteFixedLength
  {
    get => m_WriteFixedLength;
    set => SetField(ref m_WriteFixedLength, value);
  }

  /// <inheritdoc />
  public CsvFileDummy Clone()
  {
    var res = new CsvFileDummy();
    CopyTo(res);
    return res;
  }

  object ICloneable.Clone() => Clone();

  /// <inheritdoc />
  public void CopyTo(IFileSetting target)
  {
    target.DisplayEndLineNo = DisplayEndLineNo;
    target.DisplayRecordNo = DisplayRecordNo;
    target.DisplayStartLineNo = DisplayStartLineNo;
    target.ColumnCollection.Overwrite(ColumnCollection);
    target.ConsecutiveEmptyRows = ConsecutiveEmptyRows;
    target.Footer = Footer;
    target.HasFieldHeader = HasFieldHeader;
    target.Header = Header;
    target.KeepUnencrypted = KeepUnencrypted;
    target.RecordLimit = RecordLimit;
    target.SkipDuplicateHeader = SkipDuplicateHeader;
    target.SkipEmptyLines = SkipEmptyLines;
    target.SkipRows = SkipRows;
    target.SkipRowsAfterHeader = SkipRowsAfterHeader;
    target.TreatNBSPAsSpace = TreatNBSPAsSpace;
    target.TreatTextAsNull = TreatTextAsNull;
    target.Trim = Trim;

    if (target is CsvFileDummy other)
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
      other.IsJson = IsJson;
      other.IsXml = IsXml;
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

  public void ResetFullPath() => m_FullPath = Path.GetFullPath(m_FileName);

  private void OnPropertyChanged(string propertyName) =>
                 PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

  private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
  {
    if (Equals(field, value))
      return false;

    field = value;
    OnPropertyChanged(propertyName);
    return true;
  }
}