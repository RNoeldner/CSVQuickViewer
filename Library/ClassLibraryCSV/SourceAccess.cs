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

using System;
using System.IO;

namespace CsvTools
{
  /// <summary>
  ///   Contains all information needed to access the input or output
  /// </summary>
  public class SourceAccess
  {
    public enum FileTypeEnum
    {
      FileSystem,

      GZip,

      Deflate,

      Pgp,

      Zip,

      Stream
    }

    /// <summary>
    ///   Type of the file
    /// </summary>
    public readonly FileTypeEnum FileType;

    public readonly string FullPath;

    /// <summary>
    ///   Flag <c>true</c> if the not encrypted files should be kept after encryption
    /// </summary>
    public readonly bool KeepEncrypted;

    /// <summary>
    ///   Method to open the base stream usually the physical file
    /// </summary>
    private readonly Func<Stream> m_OpenStream;

    /// <summary>
    ///   Determine if the access is for reading or writing
    /// </summary>
    public readonly bool Reading;

    /// <summary>
    ///   Information about the recipient for a encryption
    /// </summary>
    public readonly string Recipient;

    /// <summary>
    ///   The Password or Passphrase information
    /// </summary>
    public string EncryptedPassphrase;

    /// <summary>
    ///   Property used for informational purpose
    /// </summary>
    public string Identifier;

    /// <summary>
    ///   Location or identifier in the container
    /// </summary>
    public string IdentifierInContainer;

    /// <summary>
    ///   Get a new SourceAccess helper class
    /// </summary>
    /// <param name="fileName">Name of teh file</param>
    /// <param name="isReading"><c>true</c> if the files is for reading</param>
    /// <param name="id">The identifier for the file for logging etc</param>
    /// <param name="recipient">Recipient for PGP encryption</param>
    /// <param name="keepEncrypted">
    ///   Do not remove teh not encrypted files once teh encrypted one is created, needed in for
    ///   debugging in case teh private key is not known and the file can not be decrypted
    /// </param>
    public SourceAccess(
      in string fileName,
      bool isReading = true,
      in string? id = null,
      in string? recipient = null,
      bool keepEncrypted = false)
    {
      if (string.IsNullOrWhiteSpace(fileName))
        throw new ArgumentException("File can not be empty", nameof(fileName));

      // as of now a physical file must exist
      if (isReading && !FileSystemUtils.FileExists(fileName))
        throw new FileNotFoundException(
          $"The file '{FileSystemUtils.GetShortDisplayFileName(fileName)}' does not exist or is not accessible.",
          fileName);

      FullPath = fileName;
      Reading = isReading;
      Identifier = id ?? FileSystemUtils.GetShortDisplayFileName(fileName, 40);
      Recipient = recipient ?? string.Empty;
      KeepEncrypted = keepEncrypted;
      LeaveOpen = false;
      FileType = FromExtension(fileName);
      EncryptedPassphrase = string.Empty;
      IdentifierInContainer = string.Empty;
      switch (FileType)
      {
        case FileTypeEnum.Zip when !isReading:
          IdentifierInContainer = FileSystemUtils.GetFileName(fileName).ReplaceCaseInsensitive(".zip", "");

          break;
        // for PGP we need a password/ pass phrase for Zip we might need one later
        case FileTypeEnum.Pgp when isReading:
          EncryptedPassphrase = FunctionalDI.GetEncryptedPassphraseForFile(fileName);
          break;
      }

      if (!isReading && KeepEncrypted)
      {
        // remove extension
        var split = FileSystemUtils.SplitPath(fileName);
        var fn = Path.Combine(split.DirectoryName, split.FileNameWithoutExtension);
        m_OpenStream = GetOpenStreamFunc(fn, false);
      }
      else
      {
        m_OpenStream = GetOpenStreamFunc(fileName, isReading);
      }
    }

#if !QUICK

    /// <summary>
    ///   Create a source access based on a setting, the setting might contain information for
    ///   containers like Zip of PGP
    /// </summary>
    /// <param name="setting">The setting of type <see cref="IFileSettingPhysicalFile" /></param>
    /// <param name="isReading"><c>true</c> if used for reading</param>
    public SourceAccess(IFileSettingPhysicalFile setting, bool isReading = true)
      : this(setting.FullPath, isReading, setting.ID, setting.Recipient, setting.KeepUnencrypted)
    {
    }

#endif

    /// <summary>
    ///   Create a source access based on a stream
    /// </summary>
    /// <param name="stream">The source stream, it must support seek if its a read stream</param>
    /// <param name="type">The type of the contents in the stream</param>
    public SourceAccess(Stream stream, FileTypeEnum type = FileTypeEnum.Stream)
    {
      if (!stream.CanSeek)
        throw new ArgumentException("Source stream must support seek to be used for SourceAccess", nameof(stream));
      LeaveOpen = true;
      m_OpenStream = () => stream;
      FileType = type;
      Reading = true;
      FullPath = string.Empty;
      EncryptedPassphrase = string.Empty;
      IdentifierInContainer = string.Empty;
      Recipient = string.Empty;
      // Overwrite in case we can get more information
      if (stream is FileStream fs)
      {
        Identifier = FileSystemUtils.GetShortDisplayFileName(fs.Name);
        if (type == FileTypeEnum.Stream)
          FileType = FromExtension(fs.Name);
      }
      else
      {
        Identifier = $"{stream.GetType().Name}_{FileType}";
      }
    }

    public bool LeaveOpen { get; }

    /// <summary>
    ///   Get the stream, in case of a read stream it's attempted to be at the beginning of the stream
    /// </summary>
    /// <returns></returns>
    public Stream OpenStream()
    {
      var stream = m_OpenStream.Invoke();
      if (LeaveOpen && Reading && stream.Position != 0)
        stream.Seek(0, SeekOrigin.Begin);

      // in case the SourceAccess initialized with a function, the stream is only known now...
      if (Identifier.Length == 0)
        Identifier = stream is FileStream fs
                       ? FileSystemUtils.GetShortDisplayFileName(fs.Name)
                       : $"{stream.GetType().Name}_{FileType}";
      return stream;
    }

    private static FileTypeEnum FromExtension(in string fileName)
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

    private static Func<Stream> GetOpenStreamFunc(string fileName, bool isReading) =>
      () => new FileStream(
        fileName.LongPathPrefix(),
        isReading ? FileMode.Open : FileMode.OpenOrCreate,
        isReading ? FileAccess.Read : FileAccess.ReadWrite,
        FileShare.ReadWrite);
  }
}