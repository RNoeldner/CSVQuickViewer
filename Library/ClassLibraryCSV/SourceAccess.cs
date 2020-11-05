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

    /// <summary>
    ///   Method to open the base stream usually the physical file
    /// </summary>
    [NotNull] private readonly Func<Stream> m_OpenStream;

    /// <summary>
    ///   Determine if the access is for reading or writing
    /// </summary>
    public readonly bool Reading;

    /// <summary>
    ///   The Password or Passphrase information
    /// </summary>
    [NotNull] public string EncryptedPassphrase = string.Empty;

    /// <summary>
    ///   Property used for informational purpose
    /// </summary>
    [NotNull] public string Identifier;

    /// <summary>
    ///   Location or identifier in the container
    /// </summary>
    [NotNull] public string IdentifierInContainer = string.Empty;

    /// <summary>
    ///   Information about the recipient for a encryption
    /// </summary>
    [NotNull] public string Recipient = string.Empty;

    /// <summary>
    /// Create a source access based on a setting, the setting might contain information for containers like Zip of PGP
    /// </summary>
    /// <param name="setting">The setting of type <see cref="IFileSettingPhysicalFile"/></param>
    /// <param name="isReading"><c>true</c> if used for reading</param>
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

    /// <summary>
    /// Create a source access based on a file name
    /// </summary>
    /// <param name="fileName">Fully qualified name of the file</param>
    /// <param name="isReading"><c>true</c> if used for reading</param>
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

    /// <summary>
    /// Create a source access based on a stream
    /// </summary>
    /// <param name="stream">The source stream, it must support seek if its a read stream</param>
    /// <param name="isReading"><c>true</c> if used for reading</param>
    /// <param name="type">The type of the contents in the stream</param>
    public SourceAccess([NotNull] Stream stream, bool isReading, FileTypeEnum type = FileTypeEnum.Stream) : this(
      () => stream, isReading, type)
    {
      if (isReading && !stream.CanSeek)
        throw new ArgumentException("Source stream must support seek to be used for SourceAccess", nameof(stream));
      LeaveOpen = true;
      Identifier = $"{stream.GetType().Name}_{FileType}";

      // Overwrite in case we can get more information
      if (stream is FileStream fs)
      {
        Identifier = FileSystemUtils.GetShortDisplayFileName(fs.Name);
        if ((type == FileTypeEnum.Stream))
          FileType = FromExtension(fs.Name);
      }
    }

    /// <summary>
    /// Create a source access based on a function that will return a stream
    /// </summary>
    /// <param name="streamFunc"> A function that will return the stream</param>
    /// <param name="isReading"><c>true</c> if used for reading</param>
    /// <param name="type">The type of the contents in the stream</param>
    private SourceAccess([NotNull] Func<Stream> streamFunc, bool isReading, FileTypeEnum type)
    {
      m_OpenStream = streamFunc;
      Reading = isReading;
      FileType = type;
      LeaveOpen = false;
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
        Identifier = (stream is FileStream fs)
          ? FileSystemUtils.GetShortDisplayFileName(fs.Name)
          : $"{stream.GetType().Name}_{FileType}";
      return stream;
    }

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

    private static Func<Stream> GetOpenStreamFunc(string fileName, bool isReading) => () =>
      new FileStream(fileName.LongPathPrefix(),
        isReading ? FileMode.Open : FileMode.Create,
        isReading ? FileAccess.Read : FileAccess.ReadWrite, FileShare.ReadWrite);
  }
}