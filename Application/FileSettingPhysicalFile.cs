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


namespace CsvTools
{
  /// <inheritdoc cref="CsvTools.IJsonFile" />
  public class FileSettingPhysicalFile : IFileSettingPhysicalFile
  {
    private string? m_FullPath;    

    /// <inheritdoc />
    public bool ByteOrderMark { get; set; } = true;

    /// <inheritdoc />
    public int CodePageId { get; set; } = 65001;

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
    public string KeyFile { get; set; } = string.Empty;

    /// <inheritdoc />
    public string Passphrase { get; set; } = string.Empty;

    /// <inheritdoc />
    public string RemoteFileName { get; set; } = string.Empty;

    /// <inheritdoc />
    public string RootFolder { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool SetLatestSourceTimeForWrite { get; set; }

    /// <inheritdoc />
    public bool ThrowErrorIfNotExists { get; set; }

    /// <inheritdoc />
    public ValueFormat ValueFormatWrite { get; set; } = ValueFormat.Empty;

    /// <inheritdoc />
    public void ResetFullPath() => m_FullPath = FileName.FullPath(RootFolder);

    #region IFileSetting

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
    public bool Trim { get; set; } = false;

    /// <inheritdoc />
    public virtual object Clone()
    {
      var res = new FileSettingPhysicalFile();
      CopyTo(res);
      return res;
    }

    /// <inheritdoc />
    public virtual  void CopyTo(IFileSetting target)
    {
      if (!(target is IFileSettingPhysicalFile other)) return;
      other.ByteOrderMark = ByteOrderMark;
      other.CodePageId = CodePageId;
      other.ColumnCollection.AddRange(ColumnCollection);
      other.ColumnCollection.Clear();
      other.ColumnFile = ColumnFile;
      other.ConsecutiveEmptyRows = ConsecutiveEmptyRows;
      other.DisplayEndLineNo = DisplayEndLineNo;
      other.DisplayRecordNo = DisplayRecordNo;
      other.DisplayStartLineNo = DisplayStartLineNo;
      other.FileName = FileName;
      other.FileSize = FileSize;
      other.Footer = Footer;
      other.HasFieldHeader = HasFieldHeader;
      other.Header = Header;
      other.IdentifierInContainer = IdentifierInContainer;
      other.KeepUnencrypted = KeepUnencrypted;
      other.KeyFile = KeyFile;
      other.Passphrase = Passphrase;
      other.RecordLimit = RecordLimit;
      other.RemoteFileName = RemoteFileName;
      other.RootFolder = RootFolder;
      other.SetLatestSourceTimeForWrite = SetLatestSourceTimeForWrite;
      other.SkipDuplicateHeader = SkipDuplicateHeader;
      other.SkipEmptyLines = SkipEmptyLines;
      other.SkipRows = SkipRows;
      other.ThrowErrorIfNotExists = ThrowErrorIfNotExists;
      other.TreatNBSPAsSpace = TreatNBSPAsSpace;
      other.TreatTextAsNull = TreatTextAsNull;
      other.Trim = Trim;
      other.ValueFormatWrite = ValueFormatWrite;
    }

    /// <inheritdoc />
    public virtual bool Equals(IFileSetting? other2)
    {
      if (other2 == null)
        return false;
      if (ReferenceEquals(this, other2))
        return true;
      if (!(other2 is IFileSettingPhysicalFile other))
        return false;
      return
        other.ByteOrderMark  == ByteOrderMark &&
        other.CodePageId  == CodePageId &&
        other.ColumnCollection.CollectionEqualWithOrder(ColumnCollection) &&
        other.ColumnFile  == ColumnFile &&
        other.ConsecutiveEmptyRows  == ConsecutiveEmptyRows &&
        other.DisplayEndLineNo  == DisplayEndLineNo &&
        other.DisplayRecordNo  == DisplayRecordNo &&
        other.DisplayStartLineNo  == DisplayStartLineNo &&
        other.FileName  == FileName &&
        other.FileSize  == FileSize &&
        other.Footer  == Footer &&
        other.HasFieldHeader  == HasFieldHeader &&
        other.Header  == Header &&
        other.IdentifierInContainer  == IdentifierInContainer &&
        other.KeepUnencrypted  == KeepUnencrypted &&
        other.KeyFile  == KeyFile &&
        other.Passphrase  == Passphrase &&
        other.RecordLimit == RecordLimit &&
        other.RemoteFileName == RemoteFileName &&
        other.RootFolder == RootFolder &&
        other.SetLatestSourceTimeForWrite == SetLatestSourceTimeForWrite &&
        other.SkipDuplicateHeader == SkipDuplicateHeader &&
        other.SkipEmptyLines == SkipEmptyLines &&
        other.SkipRows == SkipRows &&
        other.ThrowErrorIfNotExists == ThrowErrorIfNotExists &&
        other.TreatNBSPAsSpace == TreatNBSPAsSpace &&
        other.TreatTextAsNull == TreatTextAsNull &&
        other.Trim == Trim &&
        other.ValueFormatWrite.Equals(ValueFormatWrite);
    }

    /// <inheritdoc />
    public virtual string GetDisplay() => "NonCsv";

    #endregion IFileSetting
  }
}