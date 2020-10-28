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
    /// <summary>
    /// The Password or Passphase information
    /// </summary>
    [NotNull]
    public string EncryptedPassphrase = string.Empty;

    /// <summary>
    /// Type of the file
    /// </summary>
    public FileTypeEnum FileType = FileTypeEnum.FileSystem;

    /// <summary>
    /// Property used for informational purpose
    /// </summary>
    [CanBeNull]
    public string Identifier;

    /// <summary>
    /// Location or idetifier in the ontainer
    /// </summary>
    [NotNull]
    public string IdentifierInContainer = string.Empty;

    /// <summary>
    /// Determine if the acess is for reading or writing
    /// </summary>
    public bool Reading = true;

    /// <summary>
    /// Information about the recipient for a encryption
    /// </summary>
    [NotNull]
    public string Recipient = string.Empty;

    /// <summary>
    /// Method to open the base stream usually the physical file
    /// </summary>    
    [NotNull]
    private readonly Func<Stream> m_OpenStream;

    public SourceAccess([NotNull]IFileSettingPhysicalFile setting, bool isReading = true)
    {
      FileType = FromExtension(setting.FullPath);
      Identifier = setting.ID;
      m_OpenStream = () => new FileStream(setting.FullPath.LongPathPrefix(),
                            isReading ? FileMode.Open : FileMode.Create,
                            isReading ? FileAccess.Read : FileAccess.Write, FileShare.ReadWrite);
      Reading =isReading;
      EncryptedPassphrase = setting.Passphrase ?? string.Empty;
      if (isReading && string.IsNullOrEmpty(EncryptedPassphrase) && FileType == FileTypeEnum.Pgp)
        EncryptedPassphrase = FunctionalDI.GetEncryptedPassphraseForFile(setting.FullPath);
      Recipient = setting.Recipient ?? string.Empty;
      IdentifierInContainer = setting.IdentifierInContainer?? string.Empty;
    }

    public SourceAccess([NotNull]string fileName, bool isReading = true)
    {
      FileType = FromExtension(fileName);
      Identifier = FileSystemUtils.GetShortDisplayFileName(fileName, 40);
      m_OpenStream = () => new FileStream(fileName.LongPathPrefix(),
                            isReading ? FileMode.Open : FileMode.Create,
                            isReading ? FileAccess.Read : FileAccess.ReadWrite, FileShare.ReadWrite);
      Reading =isReading;
      if (FileType == FileTypeEnum.Zip && !isReading)
        IdentifierInContainer =FileSystemUtils.GetFileName(fileName).ReplaceCaseInsensitive(".zip", "");
      
      // for PGP we need a password/ passphrase for Zip we might need one later
      if (FileType == FileTypeEnum.Pgp && isReading)
        EncryptedPassphrase = FunctionalDI.GetEncryptedPassphraseForFile(fileName);
    }

    public SourceAccess([NotNull]Func<Stream> streamFunc, bool isReading, FileTypeEnum type)
    {
      m_OpenStream = streamFunc;
      Reading=isReading;
      FileType= type;
    }

    public enum FileTypeEnum
    {
      FileSystem,
      GZip,
      Deflate,
      Pgp,
      Zip
    }

    public static FileTypeEnum FromExtension([NotNull]string fileName)
    {
      if (fileName.AssumeGZip())
        return FileTypeEnum.GZip;
      else if (fileName.AssumeDeflate())
        return FileTypeEnum.Deflate;
      else if (fileName.AssumeDeflate())
        return FileTypeEnum.Deflate;
      else if (fileName.AssumePgp())
        return FileTypeEnum.Pgp;
      else if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        return FileTypeEnum.Zip;
      return FileTypeEnum.FileSystem;
    }

    public Stream OpenStream()
    {
      if (m_OpenStream==null)
        return null;

      var stream = m_OpenStream.Invoke();
      if (string.IsNullOrEmpty(Identifier))
        Identifier = (stream is FileStream fs) ? FileSystemUtils.GetShortDisplayFileName(fs.Name) : string.Empty;
      return stream;
    }
  }
}
