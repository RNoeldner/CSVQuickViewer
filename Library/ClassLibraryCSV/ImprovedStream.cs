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
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace CsvTools
{
  public sealed class ImprovedStream : Stream
  {
    private readonly SourceAccess m_SourceAccess;

#if SupportPGP
    /// <summary>
    ///   A PGP stream, has a few underlying streams that need to be closed in the right order
    /// </summary>
    /// <remarks>
    /// This is usually the literal stream that is the Access stream as well</remarks>
    private Stream? m_StreamClosedFirst;

    /// <summary>
    ///   A PGP stream, has a few underlying streams that need to be closed in the right order
    /// </summary>
    /// <remarks>
    /// This is usually the compress stream
    /// </remarks>
    private Stream? m_StreamClosedSecond;

    /// <summary>
    ///   A PGP stream, has a few underlying streams that need to be closed in the right order
    /// </summary>
    /// <remarks>
    /// This is usually the encryption stream
    /// </remarks>
    private Stream? m_StreamClosedThird;
#endif

    /// <summary>
    /// Buffer for Compression Streams
    /// </summary>
    private const int cBufferSize = 8192;
    private bool m_DisposedValue;
    private ICSharpCode.SharpZipLib.Zip.ZipFile? m_ZipFile;

    public ImprovedStream(in SourceAccess sourceAccess)
    {
      m_SourceAccess = sourceAccess;
      BaseStream = m_SourceAccess.OpenStream();
      // ReSharper disable once VirtualMemberCallInConstructor
      OpenByFileType(m_SourceAccess.FileType);
    }

    /// <summary>
    ///   Create an improved stream based on another stream
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="type"></param>
    /// <remarks>Make sure the source stream is disposed</remarks>
    // ReSharper disable once NotNullMemberIsNotInitialized
    // ReSharper disable once UnusedMember.Global
    public ImprovedStream(in Stream stream, FileTypeEnum type) : this(new SourceAccess(stream, type))
    {
    }

    /// <inheritdoc cref="Stream.CanRead"/>
    public override bool CanRead => AccessStream!.CanRead && BaseStream.CanRead;

    /// <inheritdoc cref="Stream.CanSeek"/>
    public override bool CanSeek => BaseStream.CanSeek;

    /// <inheritdoc cref="Stream.CanWrite"/>
    public override bool CanWrite => AccessStream!.CanWrite && BaseStream.CanWrite;

    /// <inheritdoc cref="Stream.Length"/>
    public override long Length => BaseStream.Length;

    /// <summary>
    /// Percentage of read source as decimal between 0.0 and 1.0
    /// </summary>
    public double Percentage => BaseStream.Length < 1 || BaseStream.Position >= BaseStream.Length
      ? 1d
      : (double) BaseStream.Position / BaseStream.Length;

    /// <inheritdoc cref="Stream.Position"/>
    /// <summary>
    ///   This is the position in the base stream, Access stream (e.G. gZip stream) might not
    ///   support a position
    /// </summary>
    public override long Position
    {
      get => BaseStream.Position;
      set => BaseStream.Position = value;
    }

    private Stream? AccessStream { get; set; }
    private Stream BaseStream { get; set; }


#if SupportPGP
    /// <summary>
    ///   Closes the stream in case of a file opened for writing it would be uploaded to the sFTP
    /// </summary>
    private void ClosePgp(bool createdFile)
    {

      if (m_SourceAccess.FileType != FileTypeEnum.Pgp)
        return;
      if (m_StreamClosedFirst != null)
      {
        m_StreamClosedFirst!.Close();
        m_StreamClosedSecond?.Close();
        m_StreamClosedThird?.Close();
        m_StreamClosedThird?.Dispose();
        m_StreamClosedSecond?.Dispose();
        m_StreamClosedFirst!.Dispose();
        m_StreamClosedFirst = null;
      }

      if (m_SourceAccess.KeepEncrypted && createdFile)
      {
        // We have written to the encrypted file, now its time to make an encrypted copy.
        BaseStream.Close();

        var split = FileSystemUtils.SplitPath(m_SourceAccess.FullPath);
        var fn = Path.Combine(split.DirectoryName, split.FileNameWithoutExtension);
        // Encrypt the created not encrypted file file to an encrypted version
        if (!FileSystemUtils.FileExists(fn))
          throw new FileNotFoundException($"Could not find not encrypted file {fn} for encryption");

        var key = m_SourceAccess.PgpKey;
        if (string.IsNullOrEmpty(key))
          key =  FunctionalDI.GetKeyForFile(m_SourceAccess.FullPath);

        var publicKey = PgpHelper.ParsePublicKey(key);
        publicKey.EncryptFileAsync(fn, m_SourceAccess.FullPath, null, System.Threading.CancellationToken.None).GetAwaiter().GetResult();
      }
    }
#endif

    /// <inheritdoc cref="Stream.Close()"/>
    /// <summary>
    ///   Closes the stream in case of a file opened for writing it would be uploaded to the sFTP
    /// </summary>
    public override void Close()
    {
      try
      {
#if SupportPGP
        ClosePgp(true);
#endif
        try
        {
          m_ZipFile?.Close();
        }
        catch
        {
          // ignored
        }

        if (AccessStream != BaseStream)
          try
          {
            // ReSharper disable once ConstantConditionalAccessQualifier
            AccessStream?.Close();
          }
          catch
          {
            // ignored
          }

        if (!m_SourceAccess.LeaveOpen)
          BaseStream.Close();
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "ImprovedStream.Close()");
      }
    }

    /// <inheritdoc cref="Stream.CopyToAsync(Stream, int, CancellationToken)"/>
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) =>
      AccessStream!.CopyToAsync(destination, bufferSize, cancellationToken);

    public new void Dispose() => Dispose(true);

    /// <inheritdoc cref="Stream.Flush()"/>
    public override void Flush()
    {
      try
      {
#if SupportPGP
        m_StreamClosedThird?.Flush();
        m_StreamClosedSecond?.Flush();
#endif
        if (AccessStream != BaseStream)
          AccessStream?.Flush();
        BaseStream.Flush();
      }
      catch (Exception ex)
      {
        Logger.Error(ex, ex.Message);
        // Ignore
      }
    }

    /// <inheritdoc cref="Stream.Read(byte[], int, int)"/>
    public override int Read(byte[] buffer, int offset, int count) => AccessStream!.Read(buffer, offset, count);

    /// <inheritdoc cref="Stream.ReadAsync(byte[], int, int, CancellationToken)"/>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
      AccessStream!.ReadAsync(buffer, offset, count, cancellationToken);


    /// <summary>   Sets the position within the current stream.  IImprovedStream will allow you to seek to the beginning of a actually non seekable stream by re-opening the stream </summary>
    /// <param name="offset"> A byte offset relative to the origin parameter.</param>
    /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
    /// <returns>The new position within the current stream.</returns>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    /// <exception cref="NotSupportedException">The stream does not support seeking, only allowed seek would be to the beginning Offset:0 <see cref="SeekOrigin.Begin"/>.</exception>
    /// <exception cref="ObjectDisposedException"> Methods were called after the stream was closed.</exception>
    public override long Seek(long offset, SeekOrigin origin)
    {
      // The stream must support seeking to get or set the position
      if (AccessStream!.CanSeek)
        return AccessStream.Seek(offset, origin);

      if (origin != SeekOrigin.Begin || offset != 0)
        throw new NotSupportedException("Seek is only allowed to be beginning of the feed");

      // Reopen Completely      
      Close();
      BaseStream = m_SourceAccess.OpenStream();
      OpenByFileType(m_SourceAccess.FileType);

      return 0;
    }

    /// <inheritdoc />
    public override void SetLength(long value) => AccessStream!.SetLength(value);

    /// <inheritdoc cref="Stream.Write(byte[], int, int)"/>
    public override void Write(byte[] buffer, int offset, int count) => AccessStream!.Write(buffer, offset, count);

    /// <inheritdoc cref="Stream.WriteAsync(byte[], int, int, CancellationToken)"/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
      AccessStream!.WriteAsync(buffer, offset, count, cancellationToken);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (!disposing) return;
      try
      {
        AccessStream?.Dispose();
      }
      catch
      {
        // ignored
      }
      finally
      {
        AccessStream = null;
      }
#if SupportPGP
      try
      {
        m_StreamClosedSecond?.Dispose();
      }
      finally
      {
        m_StreamClosedSecond = null;
      }

      try
      {
        m_StreamClosedThird?.Dispose();
      }
      finally
      {
        m_StreamClosedThird = null;
      }
#endif
      if (AccessStream != BaseStream)
        AccessStream?.Dispose();

      if (!m_SourceAccess.LeaveOpen)
        BaseStream.Dispose();

      m_DisposedValue = true;
    }

    /// <summary>
    /// Depending on type call other Methods to work with the stream
    /// </summary>
    private void OpenByFileType(FileTypeEnum fileType)
    {
      switch (fileType)
      {
#if SupportPGP
        case FileTypeEnum.Pgp:
          OpenPgpOverBase();
          break;
#endif
        case FileTypeEnum.GZip:
          OpenZGipOverBase();
          break;

        case FileTypeEnum.Deflate:
          OpenDeflateOverBase();
          break;

        case FileTypeEnum.Zip:
          OpenZipOverBase();
          break;

        default:
          AccessStream = BaseStream;
          break;
      }
    }

#if SupportPGP
    private void OpenPgpOverBase()
    {

      ClosePgp(false);

      // Reading / Decrypting
      if (m_SourceAccess.Reading)
      {
        try
        {
          Logger.Debug("Decrypt from PGP file {filename}", m_SourceAccess.Identifier);

          var key = m_SourceAccess.PgpKey;
          var passphrase = m_SourceAccess.Passphrase;
          var keyFile = string.Empty;

          if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(passphrase))
          {
            var res = FunctionalDI.GetKeyAndPassphraseForFile(m_SourceAccess.FullPath);
            key = res.key;
            passphrase = res.passphrase;
            keyFile = res.keyFile;
          }

          var privateKey = PgpHelper.ParsePrivateKey(key);
          m_StreamClosedFirst = privateKey.GetReadStream(passphrase, BaseStream, out m_StreamClosedSecond, out m_StreamClosedThird);
          AccessStream = m_StreamClosedFirst;

          // Opening the stream did work store the information for later use
          PgpHelper.StorePassphrase(m_SourceAccess.FullPath, passphrase);
          PgpHelper.StoreKeyFile(m_SourceAccess.FullPath, keyFile);
          PgpHelper.StoreKey(m_SourceAccess.FullPath, key);
        }
        catch (Exception ex)
        {
          throw new EncryptionException("Could not decrypt file", ex);
        }
      }

      // Writing / Encrypting
      else
      {
        // Do not write PGP file imitate but encrypt when closing...
        if (!m_SourceAccess.KeepEncrypted)
        {
          Logger.Debug("Encrypt to PGP {filename}", m_SourceAccess.Identifier);

          var key = m_SourceAccess.PgpKey;
          if (string.IsNullOrEmpty(key))
            key = FunctionalDI.GetKeyForFile(m_SourceAccess.FullPath);

          var publicKey = PgpHelper.ParsePublicKey(key);
          // Access Stream will be the PgpLiteralDataGenerator (last stream opened)
          m_StreamClosedFirst = publicKey.GetWriteStream(BaseStream, out m_StreamClosedSecond, out m_StreamClosedThird);
          AccessStream = m_StreamClosedFirst;
          PgpHelper.StoreKey(m_SourceAccess.FullPath, key);
        }
        else
        {
          AccessStream = BaseStream;
        }
      }
    }
#endif

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    private async ValueTask DisposeAsyncCore()
    {
      if (AccessStream != null &&  AccessStream !=  BaseStream)
      {
        // ReSharper disable once ConstantConditionalAccessQualifier
        await AccessStream.DisposeAsync().ConfigureAwait(false);
        AccessStream = null;
      }
      if (!m_SourceAccess.LeaveOpen)
      {
        await BaseStream.DisposeAsync().ConfigureAwait(false);
      }
    }

    public override async ValueTask DisposeAsync()
    {
      await DisposeAsyncCore();
      await base.DisposeAsync().ConfigureAwait(false);
    }

#endif

    private void OpenDeflateOverBase()
    {
      if (m_SourceAccess.Reading)
      {
        Logger.Debug("Deflating {filename}", m_SourceAccess.Identifier);
        AccessStream = new BufferedStream(new DeflateStream(BaseStream, CompressionMode.Decompress, m_SourceAccess.LeaveOpen),
          cBufferSize);
      }
      else
      {
        Logger.Debug("Compressing {filename}", m_SourceAccess.Identifier);
        AccessStream = new BufferedStream(new DeflateStream(BaseStream, CompressionMode.Compress, m_SourceAccess.LeaveOpen),
          cBufferSize);
      }
    }

    private void OpenZGipOverBase()
    {
      if (m_SourceAccess.Reading)
      {
        Logger.Debug("Decompressing from GZip {filename}", m_SourceAccess.Identifier);
        AccessStream = new BufferedStream(new GZipStream(BaseStream, CompressionMode.Decompress, m_SourceAccess.LeaveOpen),
          cBufferSize);
      }
      else
      {
        Logger.Debug("Compressing to GZip {filename}", m_SourceAccess.Identifier);
        AccessStream = new BufferedStream(new GZipStream(BaseStream, CompressionMode.Compress, m_SourceAccess.LeaveOpen),
          cBufferSize);
      }
    }

    private void OpenZipOverBase()
    {
      if (m_SourceAccess.Reading)
      {
        m_ZipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(BaseStream, m_SourceAccess.LeaveOpen);
        var pass = m_SourceAccess.Passphrase;

        retry:
        m_ZipFile.Password = pass;
        try
        {
          m_ZipFile.GetEnumerator();

#if SupportPGP
          // store the password it is correct...
          if (!string.IsNullOrEmpty(pass))
            PgpHelper.StorePassphrase(m_SourceAccess.FullPath, pass);
#endif
        }
        catch (ZipException)
        {
          pass = FunctionalDI.GetPassphraseForFile(m_SourceAccess.FullPath);
          if (pass.Length > 0)
            goto retry;
          throw;
        }

        var hasFile = false;
        if (string.IsNullOrEmpty(m_SourceAccess.IdentifierInContainer))
        {
          // get csv with highest priority
          // get txt with second priority
          // the by index in file
          var bestEntry = m_ZipFile.GetFilesInZip().OrderBy(x => x.ZipFileIndex +
                     (x.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ? 0 :
                      x.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ? 500 :
                      1000)).First();

          m_SourceAccess.IdentifierInContainer = bestEntry.Name;
          Logger.Information("Using {container}", m_SourceAccess.IdentifierInContainer);
          AccessStream = m_ZipFile.GetInputStream(bestEntry);
          hasFile = true;
        }
        else
        {
          var entryIndex = m_ZipFile.FindEntry(m_SourceAccess.IdentifierInContainer, true);
          if (entryIndex == -1)
            throw new FileNotFoundException(
              $"Could not find {m_SourceAccess.IdentifierInContainer} in {m_SourceAccess.Identifier}");

          Logger.Information("Using {container}", m_SourceAccess.IdentifierInContainer);
          AccessStream = m_ZipFile.GetInputStream(entryIndex);
          hasFile = true;
        }

        if (!hasFile)
          Logger.Warning(
            "No zip entry found in {filename} {container}",
            m_SourceAccess.Identifier,
            m_SourceAccess.IdentifierInContainer);
      }
      // is writing
      else
      {
        var zipOutputStream = new ZipOutputStream(BaseStream, cBufferSize);
        if (!string.IsNullOrEmpty(m_SourceAccess.Passphrase))
          zipOutputStream.Password = m_SourceAccess.Passphrase;
        zipOutputStream.IsStreamOwner = false;
        zipOutputStream.SetLevel(5);
        if (m_SourceAccess.IdentifierInContainer.Length == 0)
          m_SourceAccess.IdentifierInContainer = "File1.txt";
        var cleanName = ZipEntry.CleanName(m_SourceAccess.IdentifierInContainer);
        var copyOtherFiles = false;
        // Check the stream if it already contains the file; if so remove the old file
        using (var zipFileTest = new ICSharpCode.SharpZipLib.Zip.ZipFile(BaseStream, true))
        {
          var entryEnumerator = zipFileTest.GetEnumerator();

          while (entryEnumerator.MoveNext())
          {
            if (!(entryEnumerator.Current is ZipEntry zipEntry))
              continue;
            if (zipEntry.IsFile && zipEntry.Name != cleanName)
            {
              copyOtherFiles = true;
              break;
            }
          }

        }

        if (copyOtherFiles)
        {
          Logger.Debug("Keeping already existing entries in {filename}", m_SourceAccess.Identifier);
          var tmpName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
          try
          {
            File.Copy(m_SourceAccess.FullPath, tmpName, true);

            // build a new Zip file with the contents of the old one but export the file we are about
            // to write
            using var zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(File.OpenRead(tmpName));
            var entryEnumerator = zipFile.GetEnumerator();
            while (entryEnumerator.MoveNext())
            {
              var zipEntry = entryEnumerator.Current as ZipEntry;
              if (!(zipEntry?.IsFile ?? false) || zipEntry.Name == cleanName)
                continue;
              using var zipStream = zipFile.GetInputStream(zipEntry);
              // Copy the source data to the new stream
              zipOutputStream.PutNextEntry(
                new ZipEntry(zipEntry.Name) { DateTime = zipEntry.DateTime, Size = zipEntry.Size });
              var buffer = new byte[4096];
              StreamUtils.Copy(zipStream, zipOutputStream, buffer);
              zipOutputStream.CloseEntry();
            }
          }
          finally
          {
            File.Delete(tmpName);
          }
        }

        Logger.Debug(
          "Zipping {container} into {filename}",
          m_SourceAccess.IdentifierInContainer,
          m_SourceAccess.Identifier);

        zipOutputStream.PutNextEntry(new ZipEntry(cleanName));
        AccessStream = zipOutputStream;
      }
    }
  }
}