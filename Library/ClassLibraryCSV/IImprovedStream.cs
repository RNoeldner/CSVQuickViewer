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
    bool CanRead { get; }

    bool CanSeek { get; }

    bool CanWrite { get; }

    long Length { get; }

    double Percentage { get; }

    long Position { get; }

    int Read(byte[] buffer, int offset, int count);

    Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

    long Seek(long offset, SeekOrigin origin);

    void Write(byte[] buffer, int offset, int count);

    Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
  }
}