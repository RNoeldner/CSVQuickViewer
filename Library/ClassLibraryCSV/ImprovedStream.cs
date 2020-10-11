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
    private readonly bool m_AssumeGZip;
    private readonly bool m_IsReading;
    [NotNull] private readonly Func<Stream> m_OpenBaseStream;
    private bool m_DisposedValue;
    private bool m_IsClosed;

    // ReSharper disable once NotNullMemberIsNotInitialized
    public ImprovedStream([NotNull] string path, bool isReading)
    {
      if (string.IsNullOrEmpty(path))
        throw new ArgumentException("Path must be provided", nameof(path));
      m_IsReading = isReading;
      m_AssumeGZip = path.AssumeGZip();
      // ReSharper disable once VirtualMemberCallInConstructor
      if (isReading)
        m_OpenBaseStream = () => new FileStream(path.LongPathPrefix(), FileMode.Open, FileAccess.Read, FileShare.Read);
      else
        m_OpenBaseStream = () =>
          new FileStream(path.LongPathPrefix(), FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

      BaseStream = m_OpenBaseStream();
      if (m_AssumeGZip)
        OpenZGipOverBase(isReading);
      else
        AccessStream = new BufferedStream(BaseStream, 8192);
    }

    // ReSharper disable once NotNullMemberIsNotInitialized
    public ImprovedStream([NotNull] Func<Stream> openStream, bool isReading, bool assumeGZip)
    {
      m_IsReading = isReading;
      m_AssumeGZip = assumeGZip;
      m_OpenBaseStream = openStream ?? throw new ArgumentNullException(nameof(openStream));
      BaseStream = m_OpenBaseStream();
      if (m_AssumeGZip)
        OpenZGipOverBase(isReading);
      else
        AccessStream = new BufferedStream(BaseStream, 8192);
    }

    [NotNull] protected BufferedStream AccessStream { get; set; }

    [NotNull] protected Stream BaseStream { get; private set; }


    public double Percentage => (double) BaseStream.Position / BaseStream.Length;

    public new void Dispose() => Dispose(true);

    public override long Seek(long offset, SeekOrigin origin)
    {
      // The stream must support seeking to get or set the position
      if (AccessStream.CanSeek && (AccessStream.Position == offset && origin == SeekOrigin.Begin))
        return AccessStream.Position;

      if (AccessStream.CanSeek || origin != SeekOrigin.Begin || offset >= 1)
        return AccessStream.Seek(offset, origin);

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
      get => AccessStream.CanSeek ? AccessStream.Position : BaseStream.Position;
      set
      {
        if (AccessStream.CanSeek)
          AccessStream.Position = value;
        else
          BaseStream.Position = value;
      }
    }

    /// <summary>
    ///   Closes the stream in case of a file opened for writing it would be uploaded to the sFTP
    /// </summary>
    public override void Close()
    {
      AccessStream.Close();
      BaseStream.Close();
      m_IsClosed = true;
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

    private void OpenZGipOverBase(bool isReading)
    {
      if (isReading)
      {
        Logger.Debug("Decompressing from GZip");
        AccessStream = new BufferedStream(new GZipStream(BaseStream, CompressionMode.Decompress), 8192);
      }
      else
      {
        Logger.Debug("Compressing to GZip");
        AccessStream = new BufferedStream(new GZipStream(BaseStream, CompressionMode.Compress), 8192);
      }
    }

    /// <summary>
    /// Initializes Stream that will be used for reading / writing the data (after Encryption or compression)
    /// </summary>
    protected virtual void ResetStreams(bool isReading)
    {
      // Some StreamReader will close the stream, in this case reopen
      // ReSharper disable once ConditionIsAlwaysTrueOrFalse
      if (m_IsClosed || !BaseStream.CanSeek)
      {
        BaseStream = m_OpenBaseStream();
        m_IsClosed = false;
      }
      else if (BaseStream.Position != 0)
        BaseStream.Seek(0, SeekOrigin.Begin);

      if (m_AssumeGZip)
        OpenZGipOverBase(isReading);
      else
        AccessStream = new BufferedStream(BaseStream, 8192);
    }
  }
}