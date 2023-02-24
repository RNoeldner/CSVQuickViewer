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
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace CsvTools
{
  public class ImprovedStream : Stream, IImprovedStream
  {
    protected readonly SourceAccess SourceAccess;

    /// <summary>
    /// Buffer for Compression Streams
    /// </summary>
    private const int cBufferSize = 8192;
    private bool m_DisposedValue;
    private ICSharpCode.SharpZipLib.Zip.ZipFile? m_ZipFile;

    public ImprovedStream(in SourceAccess sourceAccess)
    {
      SourceAccess = sourceAccess;
      BaseStream = SourceAccess.OpenStream();
      // ReSharper disable once VirtualMemberCallInConstructor
      OpenByFileType(SourceAccess.FileType);
    }

    /// <summary>
    ///   Create an improved stream based on another stream
    /// </summary>
    /// <param name="stream">The stream to read data from</param>
    /// <param name="type"></param>
    /// <remarks>Make sure the source stream is disposed</remarks>
    // ReSharper disable once NotNullMemberIsNotInitialized
    public ImprovedStream(in Stream stream, FileTypeEnum type) : this(new SourceAccess(stream, type))
    {
    }

    /// <inheritdoc cref="IImprovedStream" />
    public override bool CanRead => AccessStream!.CanRead && BaseStream.CanRead;

    /// <inheritdoc cref="IImprovedStream" />
    public override bool CanSeek => BaseStream.CanSeek;

    /// <inheritdoc cref="IImprovedStream" />
    public override bool CanWrite => AccessStream!.CanWrite && BaseStream.CanWrite;

    /// <inheritdoc cref="IImprovedStream" />
    public override long Length => BaseStream.Length;

    /// <inheritdoc />
    public double Percentage => BaseStream.Length < 1 || BaseStream.Position >= BaseStream.Length
      ? 1d
      : (double) BaseStream.Position / BaseStream.Length;

    /// <inheritdoc cref="IImprovedStream" />
    /// <summary>
    ///   This is the position in the base stream, Access stream (e.G. gZip stream) might not
    ///   support a position
    /// </summary>
    public override long Position
    {
      get => BaseStream.Position;
      set => BaseStream.Position = value;
    }

    protected Stream? AccessStream { get; set; }
    protected Stream BaseStream { get; private set; }

    /// <inheritdoc cref="Stream.Close()"/>
    /// <summary>
    ///   Closes the stream in case of a file opened for writing it would be uploaded to the sFTP
    /// </summary>
    public override void Close()
    {
      try
      {
        try
        {
          m_ZipFile?.Close();
        }
        catch
        {
          // ignored
        }

        if (!ReferenceEquals(AccessStream, BaseStream))
          try
          {
            // ReSharper disable once ConstantConditionalAccessQualifier
            AccessStream?.Close();
          }
          catch
          {
            // ignored
          }

        if (!SourceAccess.LeaveOpen)
          BaseStream.Close();
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "ImprovedStream.Close()");
      }
    }

    /// <inheritdoc cref="IImprovedStream" />
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) =>
      AccessStream!.CopyToAsync(destination, bufferSize, cancellationToken);

    public new void Dispose() => Dispose(true);

    /// <inheritdoc cref="IImprovedStream" />
    public override void Flush()
    {
      try
      {
        if (!ReferenceEquals(AccessStream, BaseStream))
          AccessStream?.Flush();
        BaseStream.Flush();
      }
      catch (Exception ex)
      {
        Logger.Error(ex, ex.Message);
        // Ignore
      }
    }

    /// <inheritdoc cref="IImprovedStream" />
    public override int Read(byte[] buffer, int offset, int count) => AccessStream!.Read(buffer, offset, count);

    /// <inheritdoc cref="IImprovedStream" />
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
      AccessStream!.ReadAsync(buffer, offset, count, cancellationToken);

    /// <inheritdoc cref="IImprovedStream" />
    public override long Seek(long offset, SeekOrigin origin)
    {
      // The stream must support seeking to get or set the position
      if (AccessStream!.CanSeek)
        return AccessStream.Seek(offset, origin);

      if (origin != SeekOrigin.Begin || offset != 0)
        throw new NotSupportedException("Seek is only allowed to be beginning of the feed");

      // Reopen Completely      
      Close();
      BaseStream = SourceAccess.OpenStream();
      OpenByFileType(SourceAccess.FileType);

      return 0;
    }

    /// <inheritdoc />
    public override void SetLength(long value) => AccessStream!.SetLength(value);

    /// <inheritdoc cref="IImprovedStream" />
    public override void Write(byte[] buffer, int offset, int count) => AccessStream!.Write(buffer, offset, count);

    /// <inheritdoc cref="IImprovedStream" />
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
      AccessStream!.WriteAsync(buffer, offset, count, cancellationToken);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (!disposing) return;

      if (!ReferenceEquals(AccessStream, BaseStream))
        AccessStream?.Dispose();

      if (!SourceAccess.LeaveOpen)
        BaseStream.Dispose();

      m_DisposedValue = true;
    }

    /// <summary>
    /// Depending on type call call other Methods to work with teh stream
    /// </summary>
    protected virtual void OpenByFileType(FileTypeEnum fileType)
    {
      switch (fileType)
      {
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

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    protected virtual async ValueTask DisposeAsyncCore()
    {
      if (AccessStream != null &&  !ReferenceEquals(AccessStream, BaseStream))
      {
        // ReSharper disable once ConstantConditionalAccessQualifier
        await AccessStream.DisposeAsync().ConfigureAwait(false);
        AccessStream = null;
      }
      if (!SourceAccess.LeaveOpen)
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
      if (SourceAccess.Reading)
      {
        Logger.Debug("Deflating {filename}", SourceAccess.Identifier);
        AccessStream = new BufferedStream(new DeflateStream(BaseStream, CompressionMode.Decompress, SourceAccess.LeaveOpen),
          cBufferSize);
      }
      else
      {
        Logger.Debug("Compressing {filename}", SourceAccess.Identifier);
        AccessStream = new BufferedStream(new DeflateStream(BaseStream, CompressionMode.Compress, SourceAccess.LeaveOpen),
          cBufferSize);
      }
    }

    private void OpenZGipOverBase()
    {
      if (SourceAccess.Reading)
      {
        Logger.Debug("Decompressing from GZip {filename}", SourceAccess.Identifier);
        AccessStream = new BufferedStream(new GZipStream(BaseStream, CompressionMode.Decompress, SourceAccess.LeaveOpen),
          cBufferSize);
      }
      else
      {
        Logger.Debug("Compressing to GZip {filename}", SourceAccess.Identifier);
        AccessStream = new BufferedStream(new GZipStream(BaseStream, CompressionMode.Compress, SourceAccess.LeaveOpen),
          cBufferSize);
      }
    }

    private void OpenZipOverBase()
    {
      if (SourceAccess.Reading)
      {
        m_ZipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(BaseStream, SourceAccess.LeaveOpen);

        if (!string.IsNullOrEmpty(SourceAccess.EncryptedPassphrase))
          m_ZipFile.Password = SourceAccess.EncryptedPassphrase;
        var hasFile = false;
        if (string.IsNullOrEmpty(SourceAccess.IdentifierInContainer))
        {
          var entryEnumerator = m_ZipFile.GetEnumerator();
          while (entryEnumerator.MoveNext())
          {
            var entry = entryEnumerator.Current as ZipEntry;
            if (entry?.IsFile ?? false)
            {
              SourceAccess.IdentifierInContainer = entry.Name;
              Logger.Debug(
                "Unzipping {filename} {container}",
                SourceAccess.Identifier,
                SourceAccess.IdentifierInContainer);
              AccessStream = m_ZipFile.GetInputStream(entry);
              hasFile = true;
              break;
            }
          }
        }
        else
        {
          var entryIndex = m_ZipFile.FindEntry(SourceAccess.IdentifierInContainer, true);
          if (entryIndex == -1)
            throw new FileNotFoundException(
              $"Could not find {SourceAccess.IdentifierInContainer} in {SourceAccess.Identifier}");

          Logger.Debug("Unzipping {filename} {container}", SourceAccess.Identifier, SourceAccess.IdentifierInContainer);
          AccessStream = m_ZipFile.GetInputStream(entryIndex);
          hasFile = true;
        }

        if (!hasFile)
          Logger.Warning(
            "No zip entry found in {filename} {container}",
            SourceAccess.Identifier,
            SourceAccess.IdentifierInContainer);
      }
      // is writing
      else
      {
        var zipOutputStream = new ZipOutputStream(BaseStream, cBufferSize);
        if (!string.IsNullOrEmpty(SourceAccess.EncryptedPassphrase))
          zipOutputStream.Password = SourceAccess.EncryptedPassphrase;
        zipOutputStream.IsStreamOwner = false;
        zipOutputStream.SetLevel(5);
        if (SourceAccess.IdentifierInContainer.Length == 0)
          SourceAccess.IdentifierInContainer = "File1.txt";
        var cleanName = ZipEntry.CleanName(SourceAccess.IdentifierInContainer);
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
          Logger.Debug("Keeping already existing entries in {filename}", SourceAccess.Identifier);
          var tmpName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
          try
          {
            File.Copy(SourceAccess.FullPath, tmpName, true);

            // build a new Zip file with the contend of the old one but export the file we are about
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
          SourceAccess.IdentifierInContainer,
          SourceAccess.Identifier);

        zipOutputStream.PutNextEntry(new ZipEntry(cleanName));
        AccessStream = zipOutputStream;
      }
    }
  }
}