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

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using ZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;

namespace CsvTools
{
  /// <summary>
  ///   A wrapper around streams to handle encryption and Compression As some of these additionally
  ///   used stream do not support seek, sometimes seek has to be started from scratch
  /// </summary>
  public class ImprovedStream : Stream, IImprovedStream
  {
    private const int c_BufferSize = 8192;

    protected readonly SourceAccess SourceAccess;

    private ZipFile? m_ZipFile;

    // ReSharper disable once NotNullMemberIsNotInitialized

    public ImprovedStream(in SourceAccess sourceAccess)
    {
      SourceAccess = sourceAccess;
      BaseOpen();
    }

    /// <summary>
    ///   Create an improved stream based on another stream
    /// </summary>
    /// <param name="stream">The source stream, the stream must support seek</param>
    /// <param name="type"></param>
    /// <remarks>Make sure the source stream is disposed</remarks>
    // ReSharper disable once NotNullMemberIsNotInitialized
    public ImprovedStream(in Stream stream, SourceAccess.FileTypeEnum type = SourceAccess.FileTypeEnum.Stream)
    {
      SourceAccess = new SourceAccess(stream, type);
      BaseOpen();
    }

    /// <inheritdoc cref="Stream.CanRead"/>
    public override bool CanRead => AccessStream!.CanRead && BaseStream!.CanRead;

    /// <inheritdoc cref="Stream.CanSeek"/>
    public override bool CanSeek => BaseStream!.CanSeek;

    /// <inheritdoc cref="Stream.CanWrite"/>
    public override bool CanWrite => AccessStream!.CanWrite && BaseStream!.CanWrite;

    /// <inheritdoc cref="Stream.Length()"/>
    public override long Length => BaseStream!.Length;

    public double Percentage => (double) BaseStream!.Position / BaseStream.Length;

    /// <inheritdoc cref="Stream.Position()"/>
    /// <summary>
    ///   This is the position in the base stream, Access stream (e.G. gZip stream) might not
    ///   support a position
    /// </summary>
    public override long Position
    {
      get => BaseStream!.Position;
      set => BaseStream!.Position = value;
    }

    protected Stream? AccessStream { get; set; }

    protected Stream? BaseStream { get; private set; }

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
          BaseStream?.Close();
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "ImprovedStream.Close()");
      }
    }

    /// <inheritdoc cref="Stream.CopyToAsync(Stream, int, CancellationToken)"/>
    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) =>
      AccessStream!.CopyToAsync(destination, bufferSize, cancellationToken);

    public new void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="Stream.Flush()"/>
    public override void Flush()
    {
      try
      {
        if (!ReferenceEquals(AccessStream, BaseStream))
          AccessStream?.Flush();
        BaseStream?.Flush();
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "ImprovedStream.Flush()");
        // Ignore
      }
    }

    /// <inheritdoc cref="Stream.Read(byte[], int, int)"/>
    public override int Read(byte[] buffer, int offset, int count) => AccessStream!.Read(buffer, offset, count);

    /// <inheritdoc cref="Stream.ReadAsync(byte[], int, int, CancellationToken)"/>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
      AccessStream!.ReadAsync(buffer, offset, count, cancellationToken);

    public override long Seek(long offset, SeekOrigin origin)
    {
      // The stream must support seeking to get or set the position
      if (AccessStream!.CanSeek)
        return AccessStream.Seek(offset, origin);

      if (origin != SeekOrigin.Begin || offset != 0)
        throw new NotSupportedException("Seek is only allowed to be beginning of the feed");

      // Reopen Completely
      Close();
      ResetStreams();
      return 0;
    }

    /// <inheritdoc cref="Stream.SetLength(long)"/>
    public override void SetLength(long value) => AccessStream!.SetLength(value);

    /// <inheritdoc cref="Stream.WriteAsync(byte[], int, int)"/>
    public override void Write(byte[] buffer, int offset, int count) => AccessStream!.Write(buffer, offset, count);

    /// <inheritdoc cref="Stream.WriteAsync(byte[], int, int, CancellationToken)"/>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
      AccessStream!.WriteAsync(buffer, offset, count, cancellationToken);

    protected void BaseOpen()
    {
      BaseStream = SourceAccess.OpenStream();

      switch (SourceAccess.FileType)
      {
        case SourceAccess.FileTypeEnum.GZip:
          OpenZGipOverBase();
          break;

        case SourceAccess.FileTypeEnum.Deflate:
          OpenDeflateOverBase();
          break;

        case SourceAccess.FileTypeEnum.Zip:
          OpenZipOverBase();
          break;

        default:
          AccessStream = BaseStream;
          break;
      }
    }

    /// <inheritdoc cref="Stream.Dispose(bool)"/>
    protected override void Dispose(bool disposing)
    {
      if (!disposing) return;

      if (!ReferenceEquals(AccessStream, BaseStream))
        // ReSharper disable once ConstantConditionalAccessQualifier
        AccessStream?.Dispose();

      if (!SourceAccess.LeaveOpen)
        BaseStream?.Dispose();
    }

#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
    protected virtual async ValueTask DisposeAsyncCore()
    {
      if (AccessStream !=null &&  !ReferenceEquals(AccessStream, BaseStream))
      {
        // ReSharper disable once ConstantConditionalAccessQualifier
        await AccessStream.DisposeAsync().ConfigureAwait(false);
        AccessStream = null;
      }
      if (BaseStream !=null && !SourceAccess.LeaveOpen)
      {
        await BaseStream.DisposeAsync().ConfigureAwait(false);
        BaseStream = null;
      }
    }

    public async override ValueTask DisposeAsync()
    {
      await DisposeAsyncCore();
      await base.DisposeAsync().ConfigureAwait(false);
    }

#endif

    /// <summary>
    ///   Initializes Stream that will be used for reading / writing the data (after Encryption or compression)
    /// </summary>
    protected virtual void ResetStreams() => BaseOpen();

    private void OpenDeflateOverBase()
    {
      if (SourceAccess.Reading)
      {
        Logger.Debug("Deflating {filename}", SourceAccess.Identifier);
        AccessStream = new BufferedStream(
          new DeflateStream(BaseStream, CompressionMode.Decompress, SourceAccess.LeaveOpen),
          c_BufferSize);
      }
      else
      {
        Logger.Debug("Compressing {filename}", SourceAccess.Identifier);
        AccessStream = new BufferedStream(
          new DeflateStream(BaseStream, CompressionMode.Compress, SourceAccess.LeaveOpen),
          c_BufferSize);
      }
    }

    private void OpenZGipOverBase()
    {
      if (SourceAccess.Reading)
      {
        Logger.Debug("Decompressing from GZip {filename}", SourceAccess.Identifier);
        AccessStream = new BufferedStream(
          new GZipStream(BaseStream, CompressionMode.Decompress, SourceAccess.LeaveOpen),
          c_BufferSize);
      }
      else
      {
        Logger.Debug("Compressing to GZip {filename}", SourceAccess.Identifier);
        AccessStream = new BufferedStream(
          new GZipStream(BaseStream, CompressionMode.Compress, SourceAccess.LeaveOpen),
          c_BufferSize);
      }
    }

    private void OpenZipOverBase()
    {
      if (SourceAccess.Reading)
      {
        m_ZipFile = new ZipFile(BaseStream, SourceAccess.LeaveOpen);

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
        var zipOutputStream = new ZipOutputStream(BaseStream, c_BufferSize);
        if (!string.IsNullOrEmpty(SourceAccess.EncryptedPassphrase))
          zipOutputStream.Password = SourceAccess.EncryptedPassphrase;
        zipOutputStream.IsStreamOwner = false;
        zipOutputStream.SetLevel(5);
        if (SourceAccess.IdentifierInContainer.Length == 0)
          SourceAccess.IdentifierInContainer = "File1.txt";
        var cleanName = ZipEntry.CleanName(SourceAccess.IdentifierInContainer);
        var copyOtherFiles = false;
        // Check the stream if it already contains the file; if so remove the old file
        using (var zipFileTest = new ZipFile(BaseStream, true))
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

            // build a new Zip file with the contend of the old one but exlode the file we are about
            // to write
            using var zipFile = new ZipFile(File.OpenRead(tmpName));
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