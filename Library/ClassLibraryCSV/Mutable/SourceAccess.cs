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
using System;
using System.IO;

namespace CsvTools
{
  /// <summary>
  ///   Contains all information needed to access the input or output
  /// </summary>
  public sealed class SourceAccess
  {
    /// <summary>
    ///   Type of the file
    /// </summary>
    public readonly FileTypeEnum FileType;

    public readonly string FullPath;

    /// <summary>
    ///   Method to open the base stream usually the physical file
    /// </summary>
    private readonly Func<Stream> m_OpenStream;

    /// <summary>
    ///   Determine if the access is for reading or writing
    /// </summary>
    public readonly bool Reading;

    /// <summary>
    ///   Flag <c>true</c> if the not encrypted files should be kept after encryption
    /// </summary>
    public readonly bool KeepEncrypted;

    /// <summary>
    ///   The Private Key for reading PGP files
    /// </summary>
    public readonly string PgpKey;

    public readonly string Passphrase;

    /// <summary>
    ///   Property used for informational purpose
    /// </summary>
    public string Identifier { get; private set; }

    /// <summary>
    ///   Location or identifier in the container
    /// </summary>
    public string IdentifierInContainer;

    /// <summary>
    ///   Get a new SourceAccess helper class
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <param name="isReading"><c>true</c> if the files is for reading</param>
    /// <param name="id">The identifier for the file for logging etc</param>
    /// <param name="passPhrase">Known passphrase for Zip or PGP file</param>
    /// <param name="keepEncrypted"></param>
    /// <param name="pgpKey">Private key when reading pgp encrypted data or public key when writing pgp file</param>
    public SourceAccess(in string fileName, bool isReading = true, in string? id = null, in string passPhrase = "", bool keepEncrypted = false, in string pgpKey = "")
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
      Passphrase = passPhrase;


      PgpKey = string.Empty;
      KeepEncrypted = keepEncrypted;
      LeaveOpen = false;
      FileType = FromExtension(fileName);
      IdentifierInContainer = string.Empty;
      switch (FileType)
      {
        case FileTypeEnum.Zip when !isReading:
          IdentifierInContainer = FileSystemUtils.GetFileName(fileName).ReplaceCaseInsensitive(".zip", "");
          break;

        // for PGP we need a password/ pass phrase for Zip we might need one later
        case FileTypeEnum.Pgp:
          PgpKey = pgpKey;
          break;
      }
      if (!isReading && KeepEncrypted && FileType == FileTypeEnum.Pgp)
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
    /// <inheritdoc />
    /// <summary>
    ///   Create a source access based on a setting, the setting might contain information for
    ///   containers like Zip of PGP
    /// </summary>
    /// <param name="setting">The setting of type <see cref="T:CsvTools.IFileSettingPhysicalFile" /></param>
    /// <param name="isReading"><c>true</c> if used for reading</param>
    public SourceAccess(IFileSettingPhysicalFile setting, bool isReading = true)
      : this(setting.FullPath, isReading, setting.ID, setting.Passphrase, keepEncrypted: setting.KeepUnencrypted)
    {
      if (FileType != FileTypeEnum.Pgp) return;
      PgpKey = FileSystemUtils.ReadAllText(setting.KeyFile);
    }
#endif
    /// <summary>
    ///   Create a source access based on a stream
    /// </summary>
    /// <param name="stream">The source stream, it must support seek if its a read stream</param>
    /// <param name="type">The type of the contents in the stream</param>
    public SourceAccess(Stream stream, FileTypeEnum type = FileTypeEnum.Stream)
    {
      LeaveOpen = true;
      m_OpenStream = () => stream;
      FileType = type;
      Reading = true;
      FullPath = string.Empty;
      Passphrase = string.Empty;
      PgpKey = string.Empty;

      IdentifierInContainer = string.Empty;
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

    internal bool LeaveOpen { get; }

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
      return fileName.AssumeZip() ? FileTypeEnum.Zip : FileTypeEnum.Plain;
    }

    private static Func<Stream> GetOpenStreamFunc(string fileName, bool isReading) =>
      () => new FileStream(
        fileName.LongPathPrefix(),
        isReading ? FileMode.Open : FileMode.OpenOrCreate,
        isReading ? FileAccess.Read : FileAccess.ReadWrite,
        FileShare.ReadWrite);
  }
}