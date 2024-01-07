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


// Ignore Spelling: Passphrase

#nullable enable

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace CsvTools
{
  /// <summary>
  /// Setting for a file based setting
  /// </summary>
  [DebuggerDisplay("File: {ID} {m_FileName} ({ColumnCollection.Count()} Columns)")]
  public abstract class BaseSettingPhysicalFile : BaseSettings, IFileSettingPhysicalFile
  {
    private ValueFormat m_ValueFormatWrite = ValueFormat.Empty;
    private string m_ColumnFile = string.Empty;
    private string m_FileName;
    private long m_FileSize;
    private string m_FullPath = string.Empty;
    private bool m_FullPathInitialized;
    private string m_IdentifierInContainer = string.Empty;
    private string m_PassPhrase = string.Empty;
    private string m_RemoteFileName = string.Empty;
    private bool m_ThrowErrorIfNotExists = true;
    private bool m_ByteOrderMark = true;
    private int m_CodePageId = 65001;
    private long m_KeyId;
    private string m_KeyFile = string.Empty;

    /// <summary>
    /// New instance of a physical file
    /// </summary>
    /// <param name="id">ID to be passed on to <see cref="BaseSettings"/></param>
    /// <param name="fileName">The path to teh file</param>
    protected BaseSettingPhysicalFile(in string id, in string fileName) : base(id)
    {
      if (fileName.Length>2 && fileName[0] == '.' &&  fileName[1] == Path.DirectorySeparatorChar)
        m_FileName = fileName.Substring(2);
      else
        m_FileName = fileName;
    }

#if !CsvQuickViewer
    /// <inheritdoc />
    public override void CalculateLatestSourceTime() =>
      LatestSourceTimeUtc = new FileSystemUtils.FileInfo(FileSystemUtils.ResolvePattern(FullPath)).LastWriteTimeUtc;
#endif

    /// <inheritdoc />
    [DefaultValue("")]
    public string KeyFile
    {
      get => m_KeyFile;
      set => SetProperty(ref m_KeyFile, value);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>

    [DefaultValue("")]
    public virtual string ColumnFile
    {
      get => m_ColumnFile;
      set => SetProperty(ref m_ColumnFile, value);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [DefaultValue("")]
    public virtual string FileName
    {
      get => m_FileName;
      set
      {
        if (SetProperty(ref m_FileName, value))
          ResetFullPath();
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets or sets the date the file when it was read
    /// </summary>
    /// <value>The consecutive empty rows.</value>
    [DefaultValue(0)]
    public virtual long FileSize
    {
      get => m_FileSize;
      set => SetProperty(ref m_FileSize, value);
    }

    /// <inheritdoc />
    [DefaultValue(true)]
    public virtual bool ByteOrderMark
    {
      get => m_ByteOrderMark;
      set => SetProperty(ref m_ByteOrderMark, value);
    }

    /// <inheritdoc />
    [DefaultValue(65001)]
    public virtual int CodePageId
    {
      get => m_CodePageId;
      set => SetProperty(ref m_CodePageId, value);
    }

    /// <inheritdoc />
    public virtual ValueFormat ValueFormatWrite
    {
      get => m_ValueFormatWrite;
      set
      {
        if (string.IsNullOrEmpty(value.True) ||
           string.IsNullOrEmpty(value.False) ||
           string.IsNullOrEmpty(value.DateFormat) ||
           string.IsNullOrEmpty(value.NumberFormat))
        {
          var valueFormatM = new ValueFormatMut(value);

          if (string.IsNullOrEmpty(valueFormatM.True))
            valueFormatM.True="true";

          if (string.IsNullOrEmpty(valueFormatM.False))
            valueFormatM.False="false";

          if (string.IsNullOrEmpty(valueFormatM.DateFormat))
            valueFormatM.DateFormat=ValueFormat.Empty.DateFormat;

          if (string.IsNullOrEmpty(valueFormatM.NumberFormat))
            valueFormatM.NumberFormat=ValueFormat.Empty.NumberFormat;
          SetProperty(ref m_ValueFormatWrite, valueFormatM.ToImmutable());

        }
        SetProperty(ref m_ValueFormatWrite, value);
      }
    }

    /// <inheritdoc cref="IFileSetting" />
    [JsonIgnore]
    public virtual ValueFormatMut ValueFormatMut
    {
      get => new ValueFormatMut(m_ValueFormatWrite);
      set => ValueFormatWrite = value.ToImmutable();
    }

    /// <inheritdoc />
    [JsonIgnore]
    public virtual string FullPath
    {
      get
      {
        if (m_FullPathInitialized)
          return m_FullPath;
        m_FullPath = FileName.FullPath(RootFolder);
        if (m_FullPath.Length == 0)
          m_FullPath = string.Empty;
        else
          m_FullPathInitialized = true;
        return m_FullPath;
      }
    }

    /// <inheritdoc />
    [DefaultValue("")]
    public virtual string IdentifierInContainer
    {
      get => m_IdentifierInContainer;
      set => SetProperty(ref m_IdentifierInContainer, value ?? string.Empty);
    }


    /// <inheritdoc />
    [JsonIgnore]
    public virtual string Passphrase
    {
      get => m_PassPhrase;
      set => SetProperty(ref m_PassPhrase, value ?? string.Empty);
    }

    /// <inheritdoc />
    [DefaultValue("")]
    public virtual string RemoteFileName
    {
      get => m_RemoteFileName;
      set => SetProperty(ref m_RemoteFileName, value ?? string.Empty);
    }


    /// <inheritdoc />
    [JsonIgnore]
    [DefaultValue("")]
    public string RootFolder { get; set; } = string.Empty;

    /// <inheritdoc />    
    [DefaultValue(true)]
    public virtual bool ThrowErrorIfNotExists
    {
      get => m_ThrowErrorIfNotExists;
      set => SetProperty(ref m_ThrowErrorIfNotExists, value);
    }

    /// <inheritdoc cref="IFileSettingPhysicalFile"/>  
    [DefaultValue(0)]
    public long KeyID
    {
      get => m_KeyId;
      set => SetProperty(ref m_KeyId, value);
    }

    /// <inheritdoc />    
    public void ResetFullPath() => m_FullPathInitialized = false;

    /// <inheritdoc />    
    protected override void BaseSettingsCopyTo(in BaseSettings? other)
    {
      base.BaseSettingsCopyTo(other);

      if (!(other is IFileSettingPhysicalFile fileSettingPhysicalFile))
        return;
      fileSettingPhysicalFile.ByteOrderMark = m_ByteOrderMark;
      fileSettingPhysicalFile.CodePageId = m_CodePageId;
      fileSettingPhysicalFile.RootFolder = RootFolder;
      fileSettingPhysicalFile.FileSize = FileSize;
      fileSettingPhysicalFile.ColumnFile = ColumnFile;
      fileSettingPhysicalFile.FileName = FileName;
      fileSettingPhysicalFile.RemoteFileName = RemoteFileName;
      fileSettingPhysicalFile.KeyFile = KeyFile;
      fileSettingPhysicalFile.IdentifierInContainer = IdentifierInContainer;
      fileSettingPhysicalFile.ThrowErrorIfNotExists = ThrowErrorIfNotExists;
      fileSettingPhysicalFile.Passphrase = Passphrase;
      if (fileSettingPhysicalFile is BaseSettingPhysicalFile phy)
        phy.ValueFormatMut.CopyFrom(ValueFormatWrite);
    }

    /// <inheritdoc />
    public override string ToString()
    {
      var stringBuilder = new StringBuilder(base.ToString());
      stringBuilder.Append(" - ");
      stringBuilder.Append(FileSystemUtils.GetShortDisplayFileName(FileName));
      return stringBuilder.ToString();
    }

    /// <inheritdoc />
    protected override bool BaseSettingsEquals(in BaseSettings? other)
    {
      if (!(other is IFileSettingPhysicalFile fileSettingPhysicalFile))
        return base.BaseSettingsEquals(other);
      if (m_ByteOrderMark != fileSettingPhysicalFile.ByteOrderMark ||
          m_CodePageId != fileSettingPhysicalFile.CodePageId)
        return false;

      if (!fileSettingPhysicalFile.ValueFormatWrite.Equals(ValueFormatWrite))
        return false;

      if (!string.Equals(fileSettingPhysicalFile.FileName, FileName, StringComparison.OrdinalIgnoreCase))
        return false;

      if (!string.Equals(fileSettingPhysicalFile.KeyFile, KeyFile, StringComparison.OrdinalIgnoreCase))
        return false;


      if (fileSettingPhysicalFile.RemoteFileName != RemoteFileName
          || fileSettingPhysicalFile.ThrowErrorIfNotExists != ThrowErrorIfNotExists)
        return false;

      if (fileSettingPhysicalFile.IdentifierInContainer != IdentifierInContainer
          || fileSettingPhysicalFile.FileSize != FileSize)
        return false;

      if (!fileSettingPhysicalFile.Passphrase.Equals(Passphrase, StringComparison.Ordinal))
        return false;

      return string.Equals(fileSettingPhysicalFile.ColumnFile, ColumnFile, StringComparison.OrdinalIgnoreCase)
             && base.BaseSettingsEquals(other);
    }


#if !CsvQuickViewer
    /// <inheritdoc />
    public override IEnumerable<string> GetDifferences(IFileSetting other)
    {
      if (other is IFileSettingPhysicalFile physicalFile)
      {
        if (physicalFile.ByteOrderMark != ByteOrderMark)
          yield return $"{nameof(ByteOrderMark)}: {ByteOrderMark} {physicalFile.ByteOrderMark}";

        if (physicalFile.CodePageId != CodePageId)
          yield return $"{nameof(CodePageId)}: {CodePageId} {physicalFile.CodePageId}";

        if (!physicalFile.ColumnFile.Equals(ColumnFile, StringComparison.OrdinalIgnoreCase))
          yield return $"{nameof(ColumnFile)}: {ColumnFile} {physicalFile.ColumnFile}";

        if (!physicalFile.FileName.Equals(FileName, StringComparison.OrdinalIgnoreCase))
          yield return $"{nameof(FileName)}: {FileName} {physicalFile.FileName}";

        if (!physicalFile.RemoteFileName.Equals(RemoteFileName, StringComparison.OrdinalIgnoreCase))
          yield return $"{nameof(RemoteFileName)} : {RemoteFileName} {physicalFile.RemoteFileName}";

        if (!physicalFile.KeyFile.Equals(KeyFile, StringComparison.OrdinalIgnoreCase))
          yield return $"{nameof(KeyFile)} : {KeyFile} {physicalFile.KeyFile}";

        if (!physicalFile.IdentifierInContainer.Equals(IdentifierInContainer, StringComparison.OrdinalIgnoreCase))
          yield return
            $"{nameof(IdentifierInContainer)} : {IdentifierInContainer} {physicalFile.IdentifierInContainer}";

        if (physicalFile.ThrowErrorIfNotExists != ThrowErrorIfNotExists)
          yield return
            $"{nameof(ThrowErrorIfNotExists)} : {ThrowErrorIfNotExists} {physicalFile.ThrowErrorIfNotExists}";

        if (physicalFile.Passphrase != Passphrase)
          yield return $"{nameof(Passphrase)}";

        if (!physicalFile.ValueFormatWrite.Equals(ValueFormatWrite))
          yield return $"{nameof(ValueFormatWrite)}";
      }

      foreach (var res in base.GetDifferences(other))
        yield return res;
    }
#endif
  }
}