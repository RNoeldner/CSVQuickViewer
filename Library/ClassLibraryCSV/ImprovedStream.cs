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
using ICSharpCode.SharpZipLib.Zip;
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
    protected readonly SourceAccess m_SourceAccess;
    private ICSharpCode.SharpZipLib.Zip.ZipFile m_ZipFile;
    private bool m_DisposedValue;

    public ImprovedStream([NotNull] SourceAccess sourceAccess)
    {
      m_SourceAccess= sourceAccess;
      BaseOpen();
    }

    public ImprovedStream([NotNull] Func<Stream> openStream, bool isReading, SourceAccess.FileTypeEnum type)
    {
      m_SourceAccess = new SourceAccess(openStream, isReading, type);
      BaseOpen();
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
      if (!ReferenceEquals(AccessStream, BaseStream))
        AccessStream.Close();
      BaseStream.Close();
      //m_ZipArchive?.Dispose();
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

    private void OpenZGipOverBase()
    {
      if (m_SourceAccess.Reading)
      {
        Logger.Debug("Decompressing from GZip {filename}", m_SourceAccess.Identifier);
        AccessStream = new BufferedStream(new GZipStream(BaseStream, CompressionMode.Decompress), CBufferSize);
      }
      else
      {
        Logger.Debug("Compressing to GZip {filename}", m_SourceAccess.Identifier);
        AccessStream = new BufferedStream(new GZipStream(BaseStream, CompressionMode.Compress), CBufferSize);
      }
    }

    private void OpenDeflateOverBase()
    {
      if (m_SourceAccess.Reading)
      {
        Logger.Debug("Deflating {filename}", m_SourceAccess.Identifier);
        AccessStream = new BufferedStream(new DeflateStream(BaseStream, CompressionMode.Decompress), CBufferSize);
      }
      else
      {
        Logger.Debug("Compressing {filename}", m_SourceAccess.Identifier);
        AccessStream = new BufferedStream(new DeflateStream(BaseStream, CompressionMode.Compress), CBufferSize);
      }
    }
    private void OpenZipOverBase()
    {
      if (m_SourceAccess.Reading)
      {
        Logger.Debug("Unzipping {filename} {incontainer}", m_SourceAccess.Identifier, m_SourceAccess.IdentifierInContainer);

        m_ZipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(BaseStream);

        if (!string.IsNullOrEmpty(m_SourceAccess.EncryptedPassphrase))
          m_ZipFile.Password = m_SourceAccess.EncryptedPassphrase;

        var entry = m_ZipFile.GetEntry(m_SourceAccess.IdentifierInContainer);
        if (entry != null)
          AccessStream = m_ZipFile.GetInputStream(entry);
      }
      else
      {
        Logger.Debug("Zipping {incontainer} into {filename}", m_SourceAccess.IdentifierInContainer, m_SourceAccess.Identifier);

        m_ZipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(BaseStream);
        var newEntry = new ZipEntry(ZipEntry.CleanName(m_SourceAccess.IdentifierInContainer));
        newEntry.DateTime = DateTime.Now;
        m_ZipFile.PutNextEntry(newEntry);
      }
    }

    protected void BaseOpen()
    {
      BaseStream = m_SourceAccess.OpenStream();
      if (m_SourceAccess.FileType== SourceAccess.FileTypeEnum.GZip)
        OpenZGipOverBase();
      else if (m_SourceAccess.FileType== SourceAccess.FileTypeEnum.Deflate)
        OpenDeflateOverBase();
      else if (m_SourceAccess.FileType== SourceAccess.FileTypeEnum.Zip)
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