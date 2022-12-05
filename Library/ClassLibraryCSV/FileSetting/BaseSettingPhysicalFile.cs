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

#nullable enable

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace CsvTools
{
  [DebuggerDisplay("File: {ID} {m_FileName} ({ColumnCollection.Count()} Columns)")]
  public abstract class BaseSettingPhysicalFile : BaseSettings, IFileSettingPhysicalFile
  {
    private ValueFormat m_DefaultValueFormatWrite = ValueFormat.Empty;
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
    private long m_KeyID;

    protected BaseSettingPhysicalFile(in string fileName, in string id) : base (id)
    {
      m_FileName = FileNameFix(fileName);
    }

    public override void CalculateLatestSourceTime() =>
      LatestSourceTimeUtc = new FileSystemUtils.FileInfo(FileSystemUtils.ResolvePattern(FullPath)).LastWriteTimeUtc;

    /// <inheritdoc />
    /// <summary>
    ///   Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string ColumnFile
    {
      get => m_ColumnFile;
      set => m_ColumnFile = value ?? string.Empty;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string FileName
    {
      get => m_FileName;
      set
      {
        if (SetField(ref m_FileName, value, StringComparison.Ordinal, true))
        {
          ResetFullPath();
          if (string.IsNullOrEmpty(ID))
            NotifyPropertyChanged(nameof(InternalID));
        }
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets or sets the date the file when it was read
    /// </summary>
    /// <value>The consecutive empty rows.</value>
    [XmlAttribute]
    [DefaultValue(0)]
    public virtual long FileSize
    {
      get => m_FileSize;
      set => SetField(ref m_FileSize, value);
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool ByteOrderMark
    {
      get => m_ByteOrderMark;
      set => SetField(ref m_ByteOrderMark, value);
    }

    /// <inheritdoc />
    [XmlAttribute]
    [DefaultValue(65001)]
    public virtual int CodePageId
    {
      get => m_CodePageId;
      set => SetField(ref m_CodePageId, value);
    }

    /// <summary>
    ///   Gets or sets the value format.
    /// </summary>
    /// <value>The value format.</value>
    [XmlIgnore]
    public virtual ValueFormat ValueFormatWrite
    {
      get => m_DefaultValueFormatWrite;
      set => SetField(ref m_DefaultValueFormatWrite, value);
    }

    /// <summary>
    /// Only used for Serialization
    /// </summary>
    [XmlElement(ElementName = "DefaultValueFormatWrite")]
    [JsonIgnore]
    public virtual ValueFormatMut ValueFormatMut
    {
      get => new ValueFormatMut(m_DefaultValueFormatWrite);
      set => SetField(ref m_DefaultValueFormatWrite, value.ToImmutable());
    }

    [XmlIgnore]
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
    /// <summary>
    ///   Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string IdentifierInContainer
    {
      get => m_IdentifierInContainer;
      set => SetField(ref m_IdentifierInContainer, value, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    /// <summary>
    ///   The identified to find this specific instance
    /// </summary>
    [XmlIgnore]
    [JsonIgnore]
    public override string InternalID => string.IsNullOrEmpty(ID) ? FileName : ID;

    /// <inheritdoc />
    /// <summary>
    ///   PassPhrase for Decryption, will not be stored
    /// </summary>
    [XmlIgnore]
    [JsonIgnore]
    [DefaultValue("")]
    public virtual string Passphrase
    {
      get => m_PassPhrase;
      set => SetField(ref m_PassPhrase, value, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [XmlAttribute]
    [DefaultValue("")]
    public virtual string RemoteFileName
    {
      get => m_RemoteFileName;
      set => SetField(ref m_RemoteFileName, value, StringComparison.Ordinal);
    }

    [XmlIgnore]
    [JsonIgnore]
    [DefaultValue("")]
    public string RootFolder { get; set; } = string.Empty;

    /// <inheritdoc />
    /// <summary>
    ///   Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [XmlAttribute]
    [DefaultValue(true)]
    public virtual bool ThrowErrorIfNotExists
    {
      get => m_ThrowErrorIfNotExists;
      set => SetField(ref m_ThrowErrorIfNotExists, value);
    }

    [XmlAttribute]
    [DefaultValue(0)]
    public long KeyID
    {
      get => m_KeyID;
      set => SetField(ref m_KeyID, value);
    }

    public void ResetFullPath() => m_FullPathInitialized = false;

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
      fileSettingPhysicalFile.IdentifierInContainer = IdentifierInContainer;
      fileSettingPhysicalFile.ThrowErrorIfNotExists = ThrowErrorIfNotExists;
      fileSettingPhysicalFile.Passphrase = Passphrase;
      fileSettingPhysicalFile.KeyID = KeyID;
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

      if (fileSettingPhysicalFile.RemoteFileName != RemoteFileName
          || fileSettingPhysicalFile.ThrowErrorIfNotExists != ThrowErrorIfNotExists)
        return false;

      if (fileSettingPhysicalFile.IdentifierInContainer != IdentifierInContainer
          || fileSettingPhysicalFile.FileSize != FileSize)
        return false;

      if (!fileSettingPhysicalFile.Passphrase.Equals(Passphrase, StringComparison.Ordinal)
          || fileSettingPhysicalFile.KeyID != KeyID)
        return false;

      if (!string.Equals(fileSettingPhysicalFile.ColumnFile, ColumnFile, StringComparison.OrdinalIgnoreCase))
        return false;
      return base.BaseSettingsEquals(other);
    }

    private static string FileNameFix(in string? value)
    {
      var newVal = value ?? string.Empty;

      if (newVal.StartsWith("." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        newVal = newVal.Substring(2);
      return newVal;
    }

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

        if (!physicalFile.IdentifierInContainer.Equals(IdentifierInContainer, StringComparison.OrdinalIgnoreCase))
          yield return
            $"{nameof(IdentifierInContainer)} : {IdentifierInContainer} {physicalFile.IdentifierInContainer}";

        if (physicalFile.ThrowErrorIfNotExists != ThrowErrorIfNotExists)
          yield return
            $"{nameof(ThrowErrorIfNotExists)} : {ThrowErrorIfNotExists} {physicalFile.ThrowErrorIfNotExists}";

        if (physicalFile.Passphrase != Passphrase)
          yield return $"{nameof(Passphrase)}";

        if (!physicalFile.KeyID.Equals(KeyID))
          yield return $"{nameof(KeyID)} : {KeyID} {physicalFile.KeyID}";

        if (!physicalFile.ValueFormatWrite.Equals(ValueFormatWrite))
          yield return $"{nameof(ValueFormatWrite)}";
      }

      foreach (var res in base.GetDifferences(other))
        yield return res;
    }
  }
}