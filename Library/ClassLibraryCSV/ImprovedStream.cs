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
    protected readonly string FileName;
    private readonly bool m_IsReading;
    private bool m_DisposedValue;

    public ImprovedStream([NotNull] string path, bool isReading)
    {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentException("Path must be provided", nameof(path));
      FileName = path;
      m_IsReading = isReading;

      // ReSharper disable once VirtualMemberCallInConstructor
      OpenStreams(isReading);
    }

    [NotNull] protected Stream AccessStream { get; set; }

    [NotNull] protected FileStream BaseStream { get; private set; }


    public double Percentage => (double) BaseStream.Position / BaseStream.Length;

    public new void Dispose() => Dispose(true);

    public override long Seek(long offset, SeekOrigin origin)
    {
      if (AccessStream.CanSeek || origin != SeekOrigin.Begin || offset >= 1)
        return AccessStream.Seek(offset, origin);

      // if the steam can seek go to beginning
      if (!ReferenceEquals(AccessStream, BaseStream))
      {
        // Reopen Completely
        Close();
        OpenStreams(m_IsReading);
      }

      return 0;
    }

    public override int Read([NotNull] byte[] buffer, int offset, int count) =>
      AccessStream.Read(buffer, offset, count);

    public override Task<int> ReadAsync([NotNull] byte[] buffer, int offset, int count,
      CancellationToken cancellationToken) =>
      AccessStream.ReadAsync(buffer, offset, count, cancellationToken);

    public override void Write([NotNull] byte[] buffer, int offset, int count) =>
      AccessStream.Write(buffer, offset, count);

    public override Task WriteAsync([NotNull] byte[] buffer, int offset, int count,
      CancellationToken cancellationToken) =>
      AccessStream.WriteAsync(buffer, offset, count, cancellationToken);

    public override bool CanRead => AccessStream.CanRead && BaseStream.CanRead;

    public override bool CanSeek => BaseStream.CanSeek;

    public override bool CanWrite => AccessStream.CanWrite && BaseStream.CanWrite;

    public override long Length => BaseStream.Length;

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
      AccessStream.Close();
      BaseStream.Close();
      base.Close();
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

    public override void Flush() => AccessStream.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) =>
      AccessStream.FlushAsync(cancellationToken);

    public override void SetLength(long value) => AccessStream.SetLength(value);

    public override Task CopyToAsync([NotNull] Stream destination, int bufferSize,
      CancellationToken cancellationToken) =>
      AccessStream.CopyToAsync(destination, bufferSize, cancellationToken);


    /// <summary>
    /// Initializes Stream that will be used for reading / writing the data (after Encryption or compression)
    /// </summary>
    protected virtual void OpenStreams(bool isReading)
    {
      // Some StreamReader will close the stream, in this case reopen
      if (!(BaseStream?.CanSeek ?? false))
      {
        BaseStream = isReading
          ? new FileStream(FileName.LongPathPrefix(), FileMode.Open, FileAccess.Read, FileShare.Read)
          : new FileStream(FileName.LongPathPrefix(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
      }
      else
      {
        if (BaseStream.Position != 0)
          BaseStream.Seek(0, SeekOrigin.Begin);
      }

      if (!FileName.AssumeGZip())
      {
        AccessStream = BaseStream;
        return;
      }

      if (isReading)
      {
        Logger.Debug("Decompressing from GZip {filename}", FileName);
        AccessStream = new GZipStream(BaseStream, CompressionMode.Decompress);
      }
      else
      {
        Logger.Debug("Compressing to GZip {filename}", FileName);
        AccessStream = new GZipStream(BaseStream, CompressionMode.Compress);
      }
    }
  }
}