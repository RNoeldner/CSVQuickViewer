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
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   A wrapper around streams to handle encryption and Compression
  ///   As some of these additionally used stream do not support seek, someimes seek has to be started from scratch
  /// </summary>
  public class ImprovedStream : Stream, IImprovedStream
  {
    const int c_BufferSize = 8192;    
    protected readonly SourceAccess SourceAccess;
    private bool m_DisposedValue;
    private ICSharpCode.SharpZipLib.Zip.ZipFile m_ZipFile;

    // ReSharper disable once NotNullMemberIsNotInitialized
    public ImprovedStream([NotNull] SourceAccess sourceAccess)
    {
      SourceAccess = sourceAccess;
      BaseOpen();
    }

    /// <summary>
    /// Create an improved stream based on another stream
    /// </summary>
    /// <param name="stream">The source stream, the stream must support seek</param>
    /// <param name="isReading"></param>
    /// <param name="type"></param>
    /// <remarks>Make sure the source stream is disposed</remarks>
    public ImprovedStream([NotNull] Stream stream, bool isReading, SourceAccess.FileTypeEnum type = SourceAccess.FileTypeEnum.Stream)
    {
      SourceAccess = new SourceAccess(stream, isReading, type);      
      BaseOpen();
    }

    [NotNull] protected Stream AccessStream { get; set; }

    [NotNull] protected Stream BaseStream { get; private set; }


    public double Percentage => (double) BaseStream.Position / BaseStream.Length;

    public new void Dispose() => Dispose(true);

    public override long Seek(long offset, SeekOrigin origin)
    {
      if (m_DisposedValue)
        throw new ObjectDisposedException(nameof(ImprovedStream));

      // The stream must support seeking to get or set the position
      if (AccessStream.CanSeek)
        return AccessStream.Seek(offset, origin);

      if (origin != SeekOrigin.Begin || offset != 0)
        throw new NotSupportedException("Seek is only allowed to be beginning of the feed");

      // Reopen Completely
      Close();
      ResetStreams();
      return 0;
    }

    public override int Read(byte[] buffer, int offset, int count) =>
      AccessStream.Read(buffer, offset, count);

    public override Task<int> ReadAsync([NotNull] byte[] buffer, int offset, int count,
      CancellationToken cancellationToken) =>
      AccessStream.ReadAsync(buffer, offset, count, cancellationToken);

    public override void Write(byte[] buffer, int offset, int count) =>
      AccessStream.Write(buffer, offset, count);

    public override Task WriteAsync([NotNull] byte[] buffer, int offset, int count,
      CancellationToken cancellationToken) =>
      AccessStream.WriteAsync(buffer, offset, count, cancellationToken);

    public override bool CanRead => AccessStream.CanRead && BaseStream.CanRead;

    public override bool CanSeek => BaseStream.CanSeek;

    public override bool CanWrite => AccessStream.CanWrite && BaseStream.CanWrite;

    public override long Length => BaseStream.Length;

    /// <summary>
    /// This is the position in the base stream, Access stream (e.G. gZip stream) might not support a position
    /// </summary>
    public override long Position
    {
      get => BaseStream.Position;
      set => BaseStream.Position = value;
    }

    /// <summary>
    ///   Closes the stream in case of a file opened for writing it would be uploaded to the sFTP
    /// </summary>
    public override void Close()
    {
      try
      {
        m_ZipFile?.Close();

        if (!ReferenceEquals(AccessStream, BaseStream))
          AccessStream.Close();

        if (!SourceAccess.LeaveOpen)
          BaseStream.Close();
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "ImprovedStream.Close()");
        // Ignore
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (!disposing) return;
      m_DisposedValue = true;
      Close();
      if (!ReferenceEquals(AccessStream, BaseStream))
        AccessStream.Dispose();
      if (!SourceAccess.LeaveOpen)
        BaseStream.Dispose();
    }

    public override void Flush()
    {
      try
      {
        AccessStream.Flush();
        BaseStream.Flush();
      }
      catch (Exception ex)
      {
        Logger.Warning(ex, "ImprovedStream.Flush()");
        // Ignore
      }
    }

    public override Task FlushAsync(CancellationToken cancellationToken) =>
      AccessStream.FlushAsync(cancellationToken);

    public override void SetLength(long value) => AccessStream.SetLength(value);

    public override Task CopyToAsync([NotNull] Stream destination, int bufferSize,
      CancellationToken cancellationToken) =>
      AccessStream.CopyToAsync(destination, bufferSize, cancellationToken);

    private void OpenZGipOverBase()
    {
      if (SourceAccess.Reading)
      {
        Logger.Debug("Decompressing from GZip {filename}", SourceAccess.Identifier);
        AccessStream = new BufferedStream(new GZipStream(BaseStream, CompressionMode.Decompress, SourceAccess.LeaveOpen), c_BufferSize);
      }
      else
      {
        Logger.Debug("Compressing to GZip {filename}", SourceAccess.Identifier);
        AccessStream = new BufferedStream(new GZipStream(BaseStream, CompressionMode.Compress, SourceAccess.LeaveOpen), c_BufferSize);
      }
    }

    private void OpenDeflateOverBase()
    {
      if (SourceAccess.Reading)
      {
        Logger.Debug("Deflating {filename}", SourceAccess.Identifier);
        AccessStream = new BufferedStream(new DeflateStream(BaseStream, CompressionMode.Decompress, SourceAccess.LeaveOpen), c_BufferSize);
      }
      else
      {
        Logger.Debug("Compressing {filename}", SourceAccess.Identifier);
        AccessStream = new BufferedStream(new DeflateStream(BaseStream, CompressionMode.Compress, SourceAccess.LeaveOpen), c_BufferSize);
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
            var entry = entryEnumerator.Current as ICSharpCode.SharpZipLib.Zip.ZipEntry;
            if (entry?.IsFile ?? false)
            {
              SourceAccess.IdentifierInContainer = entry.Name;
              Logger.Debug("Unzipping {filename} {container}", SourceAccess.Identifier,
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
          if (entryIndex > -1)
          {
            Logger.Debug("Unzipping {filename} {container}", SourceAccess.Identifier,
              SourceAccess.IdentifierInContainer);
            AccessStream = m_ZipFile.GetInputStream(entryIndex);
            hasFile = true;
          }
        }

        if (!hasFile)
          Logger.Warning("No zip entry found in {filename} {container}", SourceAccess.Identifier,
            SourceAccess.IdentifierInContainer);
      }
      else
      {
        var zipOutputStream = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(BaseStream, c_BufferSize);
        if (!string.IsNullOrEmpty(SourceAccess.EncryptedPassphrase))
          zipOutputStream.Password = SourceAccess.EncryptedPassphrase;

        zipOutputStream.SetLevel(8);
        if (SourceAccess.IdentifierInContainer.Length == 0)
          SourceAccess.IdentifierInContainer = "File1.txt";
        Logger.Debug("Zipping {container} into {filename}", SourceAccess.IdentifierInContainer,
          SourceAccess.Identifier);

        zipOutputStream.PutNextEntry(new ICSharpCode.SharpZipLib.Zip.ZipEntry(SourceAccess.IdentifierInContainer));
        AccessStream = zipOutputStream;
      }
    }

    protected void BaseOpen()
    {
      if (m_DisposedValue)
        throw new ObjectDisposedException(nameof(ImprovedStream));
      BaseStream = SourceAccess.OpenStream();

      if (SourceAccess.FileType == SourceAccess.FileTypeEnum.GZip)
        OpenZGipOverBase();
      else if (SourceAccess.FileType == SourceAccess.FileTypeEnum.Deflate)
        OpenDeflateOverBase();
      else if (SourceAccess.FileType == SourceAccess.FileTypeEnum.Zip)
        OpenZipOverBase();
      else
        AccessStream = BaseStream;
    }

    /// <summary>
    /// Initializes Stream that will be used for reading / writing the data (after Encryption or compression)
    /// </summary>
    protected virtual void ResetStreams() => BaseOpen();
  }
}