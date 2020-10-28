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

using JetBrains.Annotations;
using System;
using System.IO;

namespace CsvTools
{
  /// <summary>
  /// Contains all information needed to access the input or output 
  /// </summary>
  public class SourceAccess
  {
    public enum FileTypeEnum
    {
      FileSystem,
      GZip,
      Deflate,
      Pgp,
      Zip
    }

    /// <summary>
    /// Type of the file
    /// </summary>
    public readonly FileTypeEnum FileType;

    /// <summary>
    /// Method to open the base stream usually the physical file
    /// </summary>    
    [NotNull] private readonly Func<Stream> m_OpenStream;

    /// <summary>
    /// Determine if the access is for reading or writing
    /// </summary>
    public readonly bool Reading;

    /// <summary>
    /// The Password or Passphrase information
    /// </summary>
    [NotNull] public string EncryptedPassphrase = string.Empty;

    /// <summary>
    /// Property used for informational purpose
    /// </summary>
    [CanBeNull] public string Identifier;

    /// <summary>
    /// Location or identifier in the container
    /// </summary>
    [NotNull] public string IdentifierInContainer = string.Empty;

    /// <summary>
    /// Information about the recipient for a encryption
    /// </summary>
    [NotNull] public string Recipient = string.Empty;

    public SourceAccess([NotNull] IFileSettingPhysicalFile setting, bool isReading = true) : this(
      GetOpenStreamFunc(setting.FullPath, isReading), isReading, FromExtension(setting.FullPath))
    {
      Identifier = setting.ID;
      EncryptedPassphrase = setting.Passphrase ?? string.Empty;
      if (isReading && string.IsNullOrEmpty(EncryptedPassphrase) && FileType == FileTypeEnum.Pgp)
        EncryptedPassphrase = FunctionalDI.GetEncryptedPassphraseForFile(setting.FullPath);
      Recipient = setting.Recipient ?? string.Empty;
      IdentifierInContainer = setting.IdentifierInContainer ?? string.Empty;
    }

    public SourceAccess([NotNull] string fileName, bool isReading = true) : this(GetOpenStreamFunc(fileName, isReading),
      isReading, FromExtension(fileName))
    {
      Identifier = FileSystemUtils.GetShortDisplayFileName(fileName, 40);
      switch (FileType)
      {
        case FileTypeEnum.Zip when !isReading:
          IdentifierInContainer = FileSystemUtils.GetFileName(fileName).ReplaceCaseInsensitive(".zip", "");
          break;
        // for PGP we need a password/ passphrase for Zip we might need one later
        case FileTypeEnum.Pgp when isReading:
          EncryptedPassphrase = FunctionalDI.GetEncryptedPassphraseForFile(fileName);
          break;
      }
    }

    public SourceAccess([NotNull] Func<Stream> streamFunc, bool isReading, FileTypeEnum type)
    {
      m_OpenStream = streamFunc;
      Reading = isReading;
      FileType = type;
    }

    private static Func<Stream> GetOpenStreamFunc(string fileName, bool isReading) => () =>
      new FileStream(fileName.LongPathPrefix(),
        isReading ? FileMode.Open : FileMode.Create,
        isReading ? FileAccess.Read : FileAccess.ReadWrite, FileShare.ReadWrite);

    private static FileTypeEnum FromExtension([NotNull] string fileName)
    {
      if (fileName.AssumeGZip())
        return FileTypeEnum.GZip;
      if (fileName.AssumeDeflate())
        return FileTypeEnum.Deflate;
      if (fileName.AssumeDeflate())
        return FileTypeEnum.Deflate;
      if (fileName.AssumePgp())
        return FileTypeEnum.Pgp;
      return fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? FileTypeEnum.Zip : FileTypeEnum.FileSystem;
    }

    public Stream OpenStream()
    {
      var stream = m_OpenStream.Invoke();
      if (string.IsNullOrEmpty(Identifier))
        Identifier = (stream is FileStream fs) ? FileSystemUtils.GetShortDisplayFileName(fs.Name) : string.Empty;
      return stream;
    }
  }
}