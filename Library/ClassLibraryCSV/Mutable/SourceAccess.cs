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
#if SupportPGP
using Org.BouncyCastle.Bcpg.OpenPgp;
#endif
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
    ///   The Password or Passphrase information
    /// </summary>
    public string Passphrase;

#if SupportPGP
    /// <summary>
    ///   Flag <c>true</c> if the not encrypted files should be kept after encryption
    /// </summary>
    public readonly bool KeepEncrypted;

    /// <summary>
    /// Needed to provide the encryption key
    /// </summary>
    public readonly long KeyID;

    /// <summary>
    ///   The Public Key for writing PGP files
    /// </summary>
    public PgpPublicKey? PublicKey;

    /// <summary>
    ///   The Private Key for reading PGP files
    /// </summary>
    public PgpSecretKeyRingBundle? PrivateKey;
#endif

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
#if SupportPGP
    /// <param name="keyId">PGP encryption key identifier</param>
    /// <param name="keepEncrypted">
    ///   Do not remove the not encrypted files once the encrypted one is created, needed in for
    ///   debugging in case the private key is not known and the file can not be decrypted
    /// </param>
    /// <param name="privateKey"></param>
    /// <param name="publicKey"></param>
#endif
    public SourceAccess(
      in string fileName,
      bool isReading = true,
      in string? id = null
#if SupportPGP
      , long keyId = 0
      , bool keepEncrypted = false
      , in string privateKey = ""
      , in string publicKey = ""
#endif
      )
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

#if !QUICK
      KeyID = keyId;
      KeepEncrypted = keepEncrypted;
#endif
      LeaveOpen = false;
      FileType = FromExtension(fileName);
      Passphrase = string.Empty;
      IdentifierInContainer = string.Empty;
      switch (FileType)
      {
        case FileTypeEnum.Zip when !isReading:
          IdentifierInContainer = FileSystemUtils.GetFileName(fileName).ReplaceCaseInsensitive(".zip", "");

          break;
#if !QUICK
        // for PGP we need a password/ pass phrase for Zip we might need one later
        case FileTypeEnum.Pgp when isReading:
          var res = FunctionalDI.GetPassphraseForFile(fileName);
          Passphrase = res.Item1;
          PrivateKey = PgpHelper.ParsePrivateKey(privateKey.Length==0 ? res.Item2 : privateKey);
          break;
        case FileTypeEnum.Pgp when !isReading:
          if (!string.IsNullOrEmpty(publicKey))
            PublicKey = PgpHelper.ParsePublicKey(publicKey!);
          break;
#endif
      }
#if !QUICK
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
#else
      m_OpenStream = GetOpenStreamFunc(fileName, isReading);
#endif
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
      : this(setting.FullPath, isReading, setting.ID, 0, setting.KeepUnencrypted)
    {
      if (FileType != FileTypeEnum.Pgp) return;
      if (isReading)
        PrivateKey = PgpHelper.ParsePrivateKey(FileSystemUtils.ReadAllText(setting.KeyFileRead));
      else
        PublicKey = PgpHelper.ParsePublicKey(FileSystemUtils.ReadAllText(setting.KeyFileWrite));
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
      return fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? FileTypeEnum.Zip : FileTypeEnum.Plain;
    }

    private static Func<Stream> GetOpenStreamFunc(string fileName, bool isReading) =>
      () => new FileStream(
        fileName.LongPathPrefix(),
        isReading ? FileMode.Open : FileMode.OpenOrCreate,
        isReading ? FileAccess.Read : FileAccess.ReadWrite,
        FileShare.ReadWrite);
  }
}