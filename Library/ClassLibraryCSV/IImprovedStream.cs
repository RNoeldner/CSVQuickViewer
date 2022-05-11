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

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public interface IImprovedStream : IDisposable
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
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

    /// <inheritdoc cref="Stream.Seek(long, SeekOrigin)"/>
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