/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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

namespace CsvTools;

/// <summary>
///   Contains all information needed to access the input or output
/// </summary>
public sealed class SourceAccess
{
  /// <summary>
  ///   Type of the file
  /// </summary>
  public readonly FileTypeEnum FileType;

  /// <summary>
  /// Full Path of the file
  /// </summary>
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

  /// <summary>
  /// Passphrase for decryption
  /// </summary>
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
  /// Initializes a new instance of the <see cref="SourceAccess"/> class,
  /// encapsulating all parameters required to open a source file for reading or writing.
  /// </summary>
  /// <param name="fileName">
  /// The full path to the file to be accessed. Must not be empty or whitespace.
  /// </param>
  /// <param name="isReading">
  /// Indicates whether the file should be opened for reading (<c>true</c>) or writing (<c>false</c>).
  /// </param>
  /// <param name="passPhrase">
  /// The optional passphrase used to decrypt password-protected ZIP or PGP files.
  /// </param>
  /// <param name="keepEncrypted">
  /// If <c>true</c>, PGP-encrypted output will remain encrypted (no decryption performed before writing).
  /// </param>
  /// <param name="pgpKey">
  /// When reading PGP-encrypted data, specifies the private key to use for decryption;
  /// when writing, specifies the public key used for encryption.
  /// </param>
  /// <param name="identifierInContainer">
  /// An optional identifier for a specific file within a container (e.g., ZIP entry name).
  /// </param>
  /// <remarks>
  /// <para>
  /// If the file is PGP-encrypted and no key or passphrase is provided,
  /// <see cref="FunctionalDI.GetKeyAndPassphraseForFile"/> is invoked to retrieve them dynamically.
  /// </para>
  /// <para>
  /// When <paramref name="keepEncrypted"/> is <c>true</c> for PGP output,
  /// the target file will be created without removing the <c>.pgp</c> extension,
  /// preserving the encrypted format.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">
  /// Thrown when <paramref name="fileName"/> is null, empty, or whitespace.
  /// </exception>
  /// <exception cref="FileNotFoundException">
  /// Thrown when <paramref name="isReading"/> is <c>true</c> and the specified file does not exist.
  /// </exception>
  public SourceAccess(string fileName, bool isReading = true, string passPhrase = "", bool keepEncrypted = false, string pgpKey = "", string identifierInContainer = "")
  {
    if (string.IsNullOrWhiteSpace(fileName))
      throw new ArgumentException("File can not be empty", nameof(fileName));

    // as of now a physical file must exist
    if (isReading && !FileSystemUtils.FileExists(fileName))
      throw new FileNotFoundException(
        $"The file '{fileName.GetShortDisplayFileName()}' does not exist or is not accessible.",
        fileName);

    FullPath = fileName;
    Reading = isReading;
    Identifier = fileName.GetShortDisplayFileName(40);
    Passphrase = passPhrase;


    PgpKey = string.Empty;
    KeepEncrypted = keepEncrypted;
    LeaveOpen = false;
    FileType = GetFileType(fileName);
    IdentifierInContainer = identifierInContainer;
    switch (FileType)
    {
      case FileTypeEnum.Zip when !isReading:
        IdentifierInContainer = FileSystemUtils.GetFileName(fileName).ReplaceCaseInsensitive(".zip", "");
        break;

      // for PGP we need a password/ pass phrase for Zip we might need one later
      case FileTypeEnum.Pgp:
        if (string.IsNullOrEmpty(pgpKey) && isReading)
        {
          var (passPhraseF, pgpKeyF, _) = FunctionalDI.GetKeyAndPassphraseForFile(fileName);
          if (string.IsNullOrEmpty(passPhrase) && !string.IsNullOrEmpty(passPhraseF))
            Passphrase = passPhraseF;
          if (string.IsNullOrEmpty(pgpKey) && !string.IsNullOrEmpty(pgpKeyF))
            PgpKey = pgpKeyF;
        }
        else
        {
          PgpKey = pgpKey;
        }
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
    : this(setting.FullPath, isReading, setting.Passphrase, keepEncrypted: setting.KeepUnencrypted)
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
      Identifier = fs.Name.GetShortDisplayFileName();
      if (type == FileTypeEnum.Stream)
        FileType = GetFileType(fs.Name);
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
        ? fs.Name.GetShortDisplayFileName()
        : $"{stream.GetType().Name}_{FileType}";
    return stream;
  }
    
  private static FileTypeEnum GetFileType(string fileName)
  {
    if (fileName.AssumeGZip())
      return FileTypeEnum.GZip;
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