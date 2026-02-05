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
 */
#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;

namespace CsvTools;

/// <summary>
/// Defines the contract for a file writer.
/// Implementations are responsible for writing data from a source to a file or stream,
/// reporting progress, and raising warnings or completion events.
/// </summary>
public interface IFileWriter
{
  /// <summary>
  /// Occurs when a warning or error occurs during writing.
  /// </summary>
  event EventHandler<WarningEventArgs>? Warning;

  /// <summary>
  /// Occurs when the writing operation has completed successfully.
  /// </summary>
  event EventHandler? WriteFinished;

  /// <summary>
  /// Writes data from a source to the target file.
  /// </summary>
  /// <param name="source">The data source to write from. Returns -1 if <c>null</c>.</param>
  /// <param name="progress">
  /// Provides progress reporting and cancellation support. 
  /// <see cref="ProgressInfo.Value"/> represents the current record written.
  /// Use <see cref="IProgressWithCancellation.CancellationToken"/> to observe cancellation requests.
  /// Can be <c>null</c> if neither progress nor cancellation support is needed.
  /// </param>
  /// <returns>
  /// The number of records written.
  /// Returns -1 if <paramref name="source"/> is <c>null</c>.
  /// Returns -2 if the operation was canceled.
  /// </returns>
  /// <remarks>
  /// Implementations may internally call <see cref="WriteReaderAsync"/> for the actual writing logic.
  /// </remarks>
  Task<long> WriteAsync(IFileReader? source, IProgressWithCancellation progress);

  /// <summary>
  /// Writes data from a source directly to a <see cref="Stream"/>.
  /// </summary>
  /// <param name="reader">The data source to read from.</param>
  /// <param name="output">The <see cref="Stream"/> to write the data to.</param>
  /// <param name="progress">
  /// Provides progress reporting and cancellation support. 
  /// <see cref="ProgressInfo.Value"/> represents the current record written.
  /// Use <see cref="IProgressWithCancellation.CancellationToken"/> to observe cancellation requests.
  /// Can be <c>null</c> if neither progress nor cancellation support is needed.
  /// </param>
  /// <returns>A task representing the asynchronous write operation.</returns>
  /// <remarks>
  /// Typically called by <see cref="WriteAsync"/>, but can also be used directly if writing
  /// to a specific stream is required.
  /// </remarks>
  Task WriteReaderAsync(IFileReader reader, Stream output, IProgressWithCancellation progress);
}
