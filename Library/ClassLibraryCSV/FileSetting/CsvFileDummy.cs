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
using System;

namespace CsvTools
{
  /// <inheritdoc cref="CsvTools.ICsvFile" />
  /// <summary>
  ///   Dummy setting for CSV files
  /// </summary>
  public sealed class CsvFileDummy : ICsvFile
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvFileDummy"/> class.
    /// </summary>
    /// <param name="fileName">Name of the csv file.</param>
    public CsvFileDummy(string fileName = "")
    {
      FileName = fileName;
    }

    /// <summary>
    /// Static CsvFileDummy
    /// </summary>
    public readonly static CsvFileDummy Empty = new CsvFileDummy();

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
    public char EscapePrefixChar { get; set; } = '\0';
    /// <inheritdoc />
    public char FieldDelimiterChar { get; set; } = ',';
    /// <inheritdoc />
    public char FieldQualifierChar { get; set; } = '"';
    /// <inheritdoc />
    public RecordDelimiterTypeEnum NewLine { get; set; } = RecordDelimiterTypeEnum.Crlf;
    /// <inheritdoc />
    public string NewLinePlaceholder { get; set; } = string.Empty;
    /// <inheritdoc />
    public bool NoDelimitedFile { get; set; } = false;
    /// <inheritdoc />
    public int NumWarnings { get; set; } = 0;
    /// <inheritdoc />
    public string QualifierPlaceholder { get; set; } = string.Empty;
    /// <inheritdoc />
    public bool QualifyAlways { get; set; }
    /// <inheritdoc />
    public bool QualifyOnlyIfNeeded { get; set; } = true;
    /// <inheritdoc />
    public bool TreatLfAsSpace { get; set; }
    /// <inheritdoc />
    public bool TreatUnknownCharacterAsSpace { get; set; }
    /// <inheritdoc />
    public bool TryToSolveMoreColumns { get; set; }
    /// <inheritdoc />
    public bool WarnDelimiterInValue { get; set; }
    /// <inheritdoc />
    public bool WarnEmptyTailingColumns { get; set; } = true;
    /// <inheritdoc />
    public bool WarnLineFeed { get; set; }
    /// <inheritdoc />
    public bool WarnNBSP { get; set; } = true;
    /// <inheritdoc />
    public bool WarnQuotes { get; set; }
    /// <inheritdoc />
    public bool WarnQuotesInQuotes { get; set; } = true;
    /// <inheritdoc />
    public bool WarnUnknownCharacter { get; set; } = true;
    /// <inheritdoc />
    public bool WriteFixedLength { get; set; }
    /// <inheritdoc />
    public bool Equals(ICsvFile? other) => ReferenceEquals(this, other);

    #region IFileSettingPhysicalFile        
    private string? m_FullPath = null;

    /// <inheritdoc />
    public string ColumnFile { get; set; } = string.Empty;
    /// <inheritdoc />
    public string FileName { get; set; } = string.Empty;
    /// <inheritdoc />
    public long FileSize { get; set; }
    /// <inheritdoc />
    public string FullPath => m_FullPath ?? FileName;
    /// <inheritdoc />
    public string IdentifierInContainer { get; set; } = string.Empty;
    /// <inheritdoc />
    public string Passphrase { get; set; } = string.Empty;
    /// <inheritdoc />
    public string KeyFile { get; set; } = string.Empty;
    /// <inheritdoc />
    public string RemoteFileName { get; set; } = string.Empty;
    /// <inheritdoc />
    public bool ByteOrderMark { get; set; } = true;
    /// <inheritdoc />
    public int CodePageId { get; set; } = 65001;
    /// <inheritdoc />
    public string RootFolder { get; set; } = string.Empty;
    /// <inheritdoc />
    public bool ThrowErrorIfNotExists { get; set; }
    /// <inheritdoc />
    public ValueFormat ValueFormatWrite { get; set; } = ValueFormat.Empty;
    /// <inheritdoc />
    public bool SetLatestSourceTimeForWrite { get; set; }
    /// <inheritdoc />
    public void ResetFullPath() => m_FullPath = FileSystemUtils.FullPath(FileName, RootFolder);
    #endregion

    #region IFileSetting
    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;
    /// <inheritdoc />
    public ColumnCollection ColumnCollection { get; } = new ColumnCollection();
    /// <inheritdoc />
    public int ConsecutiveEmptyRows { get; set; } = 5;
    /// <inheritdoc />
    public bool DisplayEndLineNo { get; set; }
    /// <inheritdoc />
    public bool DisplayRecordNo { get; set; }
    /// <inheritdoc />
    public bool DisplayStartLineNo { get; set; } = true;
    /// <inheritdoc />
    public string Footer { get; set; } = string.Empty;
    /// <inheritdoc />
    public bool HasFieldHeader { get; set; } = true;
    /// <inheritdoc />
    public string Header { get; set; } = string.Empty;
    /// <inheritdoc />
    public bool KeepUnencrypted { get; set; }
    /// <inheritdoc />
    public long RecordLimit { get; set; }
    /// <inheritdoc />
    public bool SkipDuplicateHeader { get; set; } = true;
    /// <inheritdoc />
    public bool SkipEmptyLines { get; set; } = true;
    /// <inheritdoc />
    public int SkipRows { get; set; }
    /// <inheritdoc />
    public bool TreatNBSPAsSpace { get; set; }
    /// <inheritdoc />
    public string TreatTextAsNull { get; set; } = "NULL";
    /// <inheritdoc />
    public TrimmingOptionEnum TrimmingOption { get; set; } = TrimmingOptionEnum.Unquoted;
    /// <inheritdoc />
    public bool Trim { get; set; } = false;
    /// <inheritdoc />
    public bool Equals(IFileSetting? other) => ReferenceEquals(this, other);
    /// <inheritdoc />
    public object Clone() => throw new NotImplementedException();
    /// <inheritdoc />
    public void CopyTo(IFileSetting other) => throw new NotImplementedException();
    /// <inheritdoc />
    public string GetDisplay() => "CSV";
    #endregion
  }
}