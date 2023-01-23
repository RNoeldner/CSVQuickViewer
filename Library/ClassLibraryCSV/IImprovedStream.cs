/*
 * Copyright (C) 2014 Raphael N�ldner : http://csvquickviewer.com
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

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  /// IImprovedStream is an interface for a stream that has a Percentage property and seek allows to jump to the beginning even if stream is not really seekable.
  /// </summary>
  public interface IImprovedStream : IDisposable
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    , IAsyncDisposable
#endif
  {
    /// <inheritdoc cref="Stream.CanRead"/>
    bool CanRead { get; }

    /// <inheritdoc cref="Stream.CanSeek"/>
    bool CanSeek { get; }

    /// <inheritdoc cref="Stream.CanWrite"/>
    bool CanWrite { get; }

    /// <inheritdoc cref="Stream.Length"/>
    long Length { get; }

    /// <summary>
    /// Percentage of read source as decimal between 0.0 and 1.0
    /// </summary>
    double Percentage { get; }

    /// <inheritdoc cref="Stream.Position"/>
    long Position { get; }

    /// <inheritdoc cref="Stream.Read(byte[], int, int)"/>
    int Read(byte[] buffer, int offset, int count);

    /// <inheritdoc cref="Stream.ReadAsync(byte[], int, int, CancellationToken)"/>
    Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

    /// <summary>   Sets the position within the current stream.  IImprovedStream will allow you to seek to the beginning of a actually non seekable stream by re-opening the stream </summary>
    /// <param name="offset"> A byte offset relative to the origin parameter.</param>
    /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
    /// <returns>The new position within the current stream.</returns>
    /// <exception cref="IOException">An I/O error occurs.</exception>
    /// <exception cref="NotSupportedException">The stream does not support seeking, only allowed seek would be to the beginning Offset:0 <see cref="SeekOrigin.Begin"/>.</exception>
    /// <exception cref="ObjectDisposedException"> Methods were called after the stream was closed.</exception>
    long Seek(long offset, SeekOrigin origin);

    /// <inheritdoc cref="Stream.Write(byte[], int, int)"/>
    void Write(byte[] buffer, int offset, int count);

    /// <inheritdoc cref="Stream.Flush()"/>
    void Flush();

    /// <inheritdoc cref="Stream.Close()"/>
    void Close();

    /// <inheritdoc cref="Stream.CopyToAsync(Stream, int, CancellationToken)"/>
    Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken);

    /// <inheritdoc cref="Stream.WriteAsync(byte[], int, int, CancellationToken)"/>
    Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
  }
}