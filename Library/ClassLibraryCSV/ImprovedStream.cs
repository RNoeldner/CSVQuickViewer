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
#define WithBuffer
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace CsvTools;

/// <summary>
/// Provides an enhanced <see cref="Stream"/> implementation capable of transparently reading
/// or writing compressed files (ZIP, GZip, Deflate), including progress reporting and container entry handling.
/// </summary>
public class ImprovedStream : Stream, IImprovedStream
{
  /// <summary>
  ///   Contains all information needed to access the input or output
  /// </summary>
  protected readonly SourceAccess SourceAccess;

  /// <summary>
  /// Buffer for Compression Streams
  /// </summary>
  private const int cBufferSize = 8192;

#if WithBuffer
  // Base block size (8 KB) × 512 = 4 MB total buffer
  private readonly byte[] m_SeekBuffer;
  private long m_BufferStartPos = 0;
  private int m_BufferLength = 0;
  private int m_BufferPos = 0;
  private long m_AbsolutePosition = 0;
#endif

  private ICSharpCode.SharpZipLib.Zip.ZipFile? m_ZipFile;
  private long m_ZipEntryIndex = -1;

  /// <summary>
  /// Constructor based on SourceAccess information
  /// </summary>
  /// <param name="sourceAccess">Contains all information needed to access the input or output</param>
  /// <param name="bufferSize">
  /// The size of the optional read buffer in bytes (used only for reading).
  /// Defaults to <c>cBufferSize * 512</c> (≈4 MB).
  /// </param>
  public ImprovedStream(in SourceAccess sourceAccess, int bufferSize = cBufferSize * 512)
  {
    SourceAccess = sourceAccess ?? throw new ArgumentNullException(nameof(sourceAccess));
    BaseStream = SourceAccess.OpenStream() ?? throw new InvalidOperationException("SourceAccess.OpenStream() returned null.");

#if WithBuffer
    // Allocate buffer only if reading
    m_SeekBuffer = SourceAccess.Reading ? new byte[bufferSize] : Array.Empty<byte>();
#endif
    OpenByFileType(SourceAccess.FileType);
  }

  /// <inheritdoc cref="Stream.CanRead"/>
  public override bool CanRead
  {
    get
    {
#if WithBuffer
      // If buffer has unread data, we can still read
      if (m_BufferPos < m_BufferLength)
        return true;
#endif
      return BaseStream.CanRead && AccessStream != null && AccessStream.CanRead;
    }
  }

  /// <inheritdoc cref="Stream.CanSeek"/>
  public override bool CanSeek => true;

  /// <inheritdoc cref="Stream.CanWrite"/>
  public override bool CanWrite => BaseStream.CanWrite && AccessStream!.CanWrite;

  /// <inheritdoc cref="Stream.Length"/>
  public override long Length => BaseStream.Length;

  /// <summary>
  /// Gets the read progress as a fraction of the (compressed) base stream length.
  /// </summary>
  /// <remarks>Decimal between 0.0 and 1.0</remarks>
  public double Percentage
  {
    get
    {
      if (BaseStream.Length < 1)
        return 1d;

#if WithBuffer
      // Calculate position including buffered but not-yet-read bytes
      return (double) Math.Min(m_AbsolutePosition, BaseStream.Length) / BaseStream.Length;
#else
        return (double)BaseStream.Position / BaseStream.Length;
#endif
    }
  }

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

  /// <summary>
  /// The stream to read from, e.g. the file in a zip file
  /// </summary>
  protected Stream? AccessStream { get; set; }

  /// <summary>
  /// The underlying stream, e.g. ZIP file
  /// </summary>
  protected Stream BaseStream { get; set; }

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

      if (!SourceAccess.LeaveOpen)
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

  /// <inheritdoc cref="IDisposable" />
  public new void Dispose() => Dispose(true);

  /// <inheritdoc cref="Stream.Flush()"/>
  public override void Flush()
  {
    try
    {
      if (AccessStream != BaseStream)
        AccessStream?.Flush();
      BaseStream.Flush();
    }
    catch (Exception ex)
    {
      // Ignore
      Logger.Error(ex, ex.Message);
    }
  }

#if WithBuffer
#if NET5_0_OR_GREATER
  /// <inheritdoc cref="Stream.ReadAsync(Memory{byte}, CancellationToken)"/>
  public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
  {
    if (m_BufferPos < m_BufferLength)
    {
      int available = Math.Min(buffer.Length, m_BufferLength - m_BufferPos);
      m_SeekBuffer.AsMemory(m_BufferPos, available).CopyTo(buffer);
      m_BufferPos += available;
      m_AbsolutePosition += available;
      return available;
    }

    m_BufferStartPos = m_AbsolutePosition;
    m_BufferLength = await AccessStream!.ReadAsync(m_SeekBuffer, 0, m_SeekBuffer.Length, cancellationToken)
      .ConfigureAwait(false);
    m_BufferPos = 0;

    if (m_BufferLength == 0)
      return 0;

    int bytesToCopy = Math.Min(buffer.Length, m_BufferLength);
    m_SeekBuffer.AsMemory(0, bytesToCopy).CopyTo(buffer);
    m_BufferPos = bytesToCopy;
    m_AbsolutePosition += bytesToCopy;
    return bytesToCopy;
  }
#endif

  /// <inheritdoc cref="Stream.ReadAsync(byte[], int, int, CancellationToken)"/>
  public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
  {
    if (m_BufferPos < m_BufferLength)
    {
      int available = Math.Min(count, m_BufferLength - m_BufferPos);
      Buffer.BlockCopy(m_SeekBuffer, m_BufferPos, buffer, offset, available);
      m_BufferPos += available;
      m_AbsolutePosition += available;
      return available;
    }

    // Refill buffer asynchronously
    m_BufferStartPos = m_AbsolutePosition;
    m_BufferLength = await AccessStream!.ReadAsync(m_SeekBuffer, 0, m_SeekBuffer.Length, cancellationToken)
      .ConfigureAwait(false);
    m_BufferPos = 0;

    if (m_BufferLength == 0)
      return 0; // EOF

    int bytesToCopy = Math.Min(count, m_BufferLength);
    Buffer.BlockCopy(m_SeekBuffer, 0, buffer, offset, bytesToCopy);
    m_BufferPos = bytesToCopy;
    m_AbsolutePosition += bytesToCopy;
    return bytesToCopy;
  }


  /// <inheritdoc cref="Stream.Read(byte[], int, int)"/>
  public override int Read(byte[] buffer, int offset, int count)
  {
    if (m_BufferPos<m_BufferLength)
    {
      // Read from buffer first
      int available = Math.Min(count, m_BufferLength - m_BufferPos);
      Buffer.BlockCopy(m_SeekBuffer, m_BufferPos, buffer, offset, available);
      m_BufferPos += available;
      m_AbsolutePosition += available;
      return available;
    }

    // Fill new buffer from base stream
    m_BufferStartPos = m_AbsolutePosition;
    m_BufferLength = AccessStream!.Read(m_SeekBuffer, 0, m_SeekBuffer.Length);
    m_BufferPos = 0;

    if (m_BufferLength == 0)
      return 0; // End of stream

    int bytesToCopy = Math.Min(count, m_BufferLength - m_BufferPos);
    Buffer.BlockCopy(m_SeekBuffer, m_BufferPos, buffer, offset, bytesToCopy);
    m_BufferPos += bytesToCopy;
    m_AbsolutePosition += bytesToCopy;
    return bytesToCopy;
  }

  /// <summary>
  /// Sets the position within the current stream. Supports limited seeking for non-seekable compressed streams
  /// by maintaining a local read buffer and transparently reopening the stream when seeking outside that range.
  /// </summary>
  /// <param name="offset">A byte offset relative to the origin parameter.</param>
  /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
  /// <returns>The new absolute position within the logical stream.</returns>
  /// <remarks>
  /// If the target position lies within the buffered range, the stream position is adjusted without reopening.
  /// Otherwise, the stream is reinitialized and fast-forwarded to the requested position.
  /// </remarks>
  public override long Seek(long offset, SeekOrigin origin)
  {
    long targetPos = origin switch
    {
      SeekOrigin.Begin => offset,
      SeekOrigin.Current => m_AbsolutePosition + offset,
      SeekOrigin.End => Length + offset,
      _ => throw new ArgumentOutOfRangeException(nameof(origin))
    };

    // Check if within buffer range
    if (targetPos >= m_BufferStartPos && targetPos < m_BufferStartPos + m_BufferLength)
    {
      m_BufferPos = (int) (targetPos - m_BufferStartPos);
      m_AbsolutePosition = targetPos;
      return m_AbsolutePosition;
    }

    // Outside buffer -> reopen
    ReopenStreamAt(targetPos);
    return m_AbsolutePosition;
  }

  private void ReopenStreamAt(long position)
  {
    Logger.Debug("Reopening stream {identifier} at {position}", SourceAccess.Identifier, position);
    // Custom logic: reopen ZIP/GZip entry, then skip to `position` bytes
    Close();
    BaseStream = SourceAccess.OpenStream();
    OpenByFileType(SourceAccess.FileType);

    // Fast-forward to target position (not ideal, but necessary for non-seekable sources)
    long skipped = 0;
    while (skipped < position)
    {
      int toRead = (int) Math.Min(m_SeekBuffer.Length, position - skipped);
      int read = AccessStream!.Read(m_SeekBuffer, 0, toRead);
      if (read <= 0) break;
      skipped += read;
    }

    m_AbsolutePosition = position;
    m_BufferStartPos = m_AbsolutePosition;
    m_BufferLength = 0;
    m_BufferPos = 0;
  }
#else

#if NET5_0_OR_GREATER
    /// <inheritdoc cref="Stream.ReadAsync(Memory{byte}, CancellationToken)"/>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => AccessStream!.ReadAsync(buffer, cancellationToken);
#endif

    /// <inheritdoc cref="Stream.ReadAsync(byte[], int, int, CancellationToken)"/>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
      AccessStream!.ReadAsync(buffer, offset, count, cancellationToken);

    /// <inheritdoc cref="Stream.Read(byte[], int, int)"/>
    public override int Read(byte[] buffer, int offset, int count) => AccessStream!.Read(buffer, offset, count);

    /// <summary>Sets the position within the current stream.  IImprovedStream will allow you to seek to the beginning of an actually non seekable stream by re-opening the stream </summary>
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

      Logger.Debug("Reopening non-seekable stream {filename} at beginning", SourceAccess.Identifier);
      // Reopen Completely
      Close();
      BaseStream = SourceAccess.OpenStream();
      OpenByFileType(SourceAccess.FileType);

      return 0;
    }
#endif
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
    if (disposing)
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

    if (!SourceAccess.LeaveOpen && disposing)
      BaseStream.Dispose();

    base.Dispose(disposing);
  }

  /// <summary>
  /// Initializes the appropriate access stream based on the specified <see cref="FileTypeEnum"/>.
  /// </summary>
  protected virtual void OpenByFileType(FileTypeEnum fileType)
  {
    switch (fileType)
    {
      case FileTypeEnum.GZip:
        OpenGZipOverBase();
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
  private async ValueTask DisposeAsyncCore()
  {
    if (AccessStream != null &&  AccessStream !=  BaseStream)
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

  /// <inheritdoc />
  public override async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore().ConfigureAwait(false);
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

  private void OpenGZipOverBase()
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


  /// <summary>
  /// Retrieves all file entries from a ZIP archive that are likely suitable for structured text processing,
  /// ordered by their relevance.
  /// </summary>
  /// <param name="zipFile">
  /// The <see cref="ICSharpCode.SharpZipLib.Zip.ZipFile"/> instance to search within.
  /// </param>
  /// <returns>
  /// An <see cref="IOrderedEnumerable{ZipEntry}"/> containing all suitable file entries,
  /// sorted by relevance and size. The sequence may be empty if no file entries are found.
  /// </returns>
  /// <remarks>
  /// The ordering prioritizes files that are most likely to contain structured or delimited data:
  /// <list type="number">
  /// <item><description>Entries whose names suggest delimited content such as <c>.csv</c> or <c>.tsv</c></description></item>
  /// <item><description>Then, text files with a <c>.txt</c> extension.</description></item>
  /// <item><description>Finally, all other file types, ordered by size (largest first) and then by ZIP file index.</description></item>
  /// </list>
  /// This method does not extract or open files; it only returns metadata describing potential candidates.
  /// </remarks>
  public static IOrderedEnumerable<ZipEntry> SuitableZipEntries(ICSharpCode.SharpZipLib.Zip.ZipFile zipFile)
  {
    Logger.Information("Getting suitable delimited text file in {zipFile}", zipFile.Name);
    return zipFile.Cast<ZipEntry>().Where(e => e.IsFile)
      .OrderBy(e => e.Name.AssumeDelimited1() ? 0 : e.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ? 1 : 2)
      .ThenByDescending(x => x.Size)
      .ThenBy(e => e.ZipFileIndex);
  }

  private void OpenZipOverBase()
  {
    if (SourceAccess.Reading)
    {
      m_ZipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(BaseStream, SourceAccess.LeaveOpen);
      var pass = SourceAccess.Passphrase;

      retry:
      m_ZipFile.Password = pass;
      try
      {
        // ReSharper disable once NotDisposedResource
        m_ZipFile.GetEnumerator();
      }
      catch (ZipException)
      {
        var (passF, _, _) = FunctionalDI.GetKeyAndPassphraseForFile(SourceAccess.FullPath);
        if (passF.Length > 0)
        {
          pass = passF;
          goto retry;
        }
        throw;
      }

      if (string.IsNullOrEmpty(SourceAccess.IdentifierInContainer))
      {
        var bestEntry = SuitableZipEntries(m_ZipFile).FirstOrDefault();
        if (bestEntry != null)
        {
          SourceAccess.IdentifierInContainer = bestEntry.Name;
          m_ZipEntryIndex = bestEntry.ZipFileIndex;
        }
      }

      if (m_ZipEntryIndex == -1)
      {
        m_ZipEntryIndex = m_ZipFile.FindEntry(SourceAccess.IdentifierInContainer, true);
        Logger.Information("Using {container}", SourceAccess.IdentifierInContainer);
      }

      if (m_ZipEntryIndex == -1)
        throw new FileNotFoundException(
          $"Could not find {SourceAccess.IdentifierInContainer} in {SourceAccess.Identifier}");

      AccessStream = m_ZipFile.GetInputStream(m_ZipEntryIndex);
    }
    // is writing
    else
    {
      var zipOutputStream = new ZipOutputStream(BaseStream, cBufferSize);
      if (!string.IsNullOrEmpty(SourceAccess.Passphrase))
        zipOutputStream.Password = SourceAccess.Passphrase;
      zipOutputStream.IsStreamOwner = false;
      zipOutputStream.SetLevel(5);
      if (SourceAccess.IdentifierInContainer.Length == 0)
        SourceAccess.IdentifierInContainer = "File1.txt";
      var cleanName = ZipEntry.CleanName(SourceAccess.IdentifierInContainer);
      var copyOtherFiles = false;
      // Check the stream if it already contains the file; if so remove the old file
      using (var zipFileTest = new ICSharpCode.SharpZipLib.Zip.ZipFile(BaseStream, true))
      {
        // ReSharper disable once NotDisposedResource
        var entryEnumerator = zipFileTest.GetEnumerator();

        while (entryEnumerator.MoveNext())
        {
          if (!(entryEnumerator.Current is ZipEntry zipEntry))
            continue;
          if (zipEntry.IsFile && !zipEntry.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase))
          {
            copyOtherFiles = true;
            break;
          }
        }

      }

      if (copyOtherFiles)
      {
        try { Logger.Debug("Keeping already existing entries in {filename}", SourceAccess.Identifier); } catch { }
        var tmpName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
          File.Copy(SourceAccess.FullPath, tmpName, true);

          // build a new Zip file with the contents of the old one but export the file we are about
          // to write
          using var zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(File.OpenRead(tmpName));
          // ReSharper disable once NotDisposedResource
          var entryEnumerator = zipFile.GetEnumerator();
          while (entryEnumerator.MoveNext())
          {
            var zipEntry = entryEnumerator.Current as ZipEntry;
            if (!(zipEntry?.IsFile ?? false) || !zipEntry.Name.Equals(cleanName, StringComparison.OrdinalIgnoreCase))
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
      try
      {
        Logger.Debug("Zipping {container} into {filename}",
          SourceAccess.IdentifierInContainer,
          SourceAccess.Identifier);
      }
      catch { }

      zipOutputStream.PutNextEntry(new ZipEntry(cleanName));
      AccessStream = zipOutputStream;
    }
  }
}