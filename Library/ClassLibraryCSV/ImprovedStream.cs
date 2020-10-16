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
  ///   A wrapper around file streams to handle pre and post processing, needed for sFTP, Encryption
  ///   and Compression
  /// </summary>
  public class ImprovedStream : Stream, IImprovedStream
  {
    const int CBufferSize = 8192;
    private readonly bool m_AssumeDeflate;
    private readonly bool m_AssumeGZip;
    private readonly bool m_IsReading;
    [NotNull] private readonly Func<Stream> m_OpenBaseStream;
    private bool m_DisposedValue;

    // ReSharper disable once NotNullMemberIsNotInitialized
    public ImprovedStream([NotNull] string path, bool isReading) : this(
      () => new FileStream(path.LongPathPrefix(), isReading ? FileMode.Open : FileMode.Create,
        isReading ? FileAccess.Read : FileAccess.Write, FileShare.ReadWrite),
      isReading, path.AssumeGZip(), path.AssumeDeflate())
    {
    }

    // ReSharper disable once NotNullMemberIsNotInitialized
    public ImprovedStream([NotNull] Func<Stream> openStream, bool isReading, bool assumeGZip, bool assumeDeflate)
    {
      m_IsReading = isReading;
      m_OpenBaseStream = openStream ?? throw new ArgumentNullException(nameof(openStream));
      m_AssumeGZip = assumeGZip;
      m_AssumeDeflate = assumeDeflate;
      BaseOpen(isReading);
    }

    [NotNull] protected Stream AccessStream { get; set; }

    [NotNull] protected Stream BaseStream { get; private set; }


    public double Percentage => (double) BaseStream.Position / BaseStream.Length;

    public new void Dispose() => Dispose(true);

    public override long Seek(long offset, SeekOrigin origin)
    {
      // The stream must support seeking to get or set the position
      if (AccessStream.CanSeek)
        return AccessStream.Seek(offset, origin);

      if (origin != SeekOrigin.Begin || offset != 0)
        throw new NotSupportedException("Seek is only allowed to be beginning of the feed");

      // Reopen Completely
      Close();
      ResetStreams(m_IsReading);
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
      if (!ReferenceEquals(AccessStream, BaseStream))
        AccessStream.Close();
      BaseStream.Close();
    }

    protected override void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      if (!disposing) return;
      m_DisposedValue = true;
      Close();
      AccessStream.Dispose();
      BaseStream.Dispose();
    }

    public override void Flush()
    {
      AccessStream.Flush();
      BaseStream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken) =>
      AccessStream.FlushAsync(cancellationToken);

    public override void SetLength(long value) => AccessStream.SetLength(value);

    public override Task CopyToAsync([NotNull] Stream destination, int bufferSize,
      CancellationToken cancellationToken) =>
      AccessStream.CopyToAsync(destination, bufferSize, cancellationToken);

    private void OpenZGipOverBase(bool isReading)
    {
      if (isReading)
      {
        Logger.Debug("Decompressing from GZip {filename}",
          (BaseStream is FileStream fs) ? FileSystemUtils.GetFileName(fs.Name) : string.Empty);
        AccessStream = new BufferedStream(new GZipStream(BaseStream, CompressionMode.Decompress), CBufferSize);
      }
      else
      {
        Logger.Debug("Compressing to GZip {filename}",
          (BaseStream is FileStream fs) ? FileSystemUtils.GetFileName(fs.Name) : string.Empty);
        AccessStream = new BufferedStream(new GZipStream(BaseStream, CompressionMode.Compress), CBufferSize);
      }
    }

    private void OpenDeflateOverBase(bool isReading)
    {
      if (isReading)
      {
        Logger.Debug("Deflating {filename}",
          (BaseStream is FileStream fs) ? FileSystemUtils.GetFileName(fs.Name) : string.Empty);
        AccessStream = new BufferedStream(new DeflateStream(BaseStream, CompressionMode.Decompress), CBufferSize);
      }
      else
      {
        Logger.Debug("Compressing {filename}",
          (BaseStream is FileStream fs) ? FileSystemUtils.GetFileName(fs.Name) : string.Empty);
        AccessStream = new BufferedStream(new DeflateStream(BaseStream, CompressionMode.Compress), CBufferSize);
      }
    }

    protected void BaseOpen(bool isReading)
    {
      BaseStream = m_OpenBaseStream();
      if (m_AssumeGZip)
        OpenZGipOverBase(isReading);
      else if (m_AssumeDeflate)
        OpenDeflateOverBase(isReading);
      else
        AccessStream = BaseStream;
    }

    /// <summary>
    /// Initializes Stream that will be used for reading / writing the data (after Encryption or compression)
    /// </summary>
    protected virtual void ResetStreams(bool isReading) => BaseOpen(isReading);
  }
}