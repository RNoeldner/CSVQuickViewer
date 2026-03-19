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
  /// Writes data from the specified <see cref="IFileReader"/> to the target file.
  /// </summary>
  /// <param name="reader">The data source containing records to be written.</param>
  /// <param name="sourceTimeZone">
  /// The time zone ID of the date/time fields currently held in the <paramref name="reader"/>. 
  /// This is used to ensure timestamps are correctly interpreted or converted before being written.
  /// </param>
  /// <param name="progress">Provides progress reporting and cancellation support.</param>
  /// <returns>
  /// A task containing the number of records written. 
  /// Returns -1 if source is null; -2 if the operation was canceled.
  /// </returns>
  Task<long> WriteAsync(IFileReader? reader, string sourceTimeZone, IProgressWithCancellation progress);

  /// <summary>
  /// Writes data from a source directly to a <see cref="Stream"/>.
  /// </summary>
  /// <param name="reader">The data source to read from.</param>
  /// <param name="output">The <see cref="Stream"/> to write the data to.</param>
  /// <param name="sourceTimeZone">
  /// The time zone ID of the date/time fields currently held in the <paramref name="reader"/>. 
  /// This is used to ensure timestamps are correctly interpreted or converted before being written to the stream.
  /// </param>
  /// <param name="progress">
  /// Provides progress reporting and cancellation support. 
  /// <see cref="ProgressInfo.Value"/> represents the current record written.
  /// Use <see cref="IProgressWithCancellation.CancellationToken"/> to observe cancellation requests.
  /// Can be <c>null</c> if neither progress nor cancellation support is needed.
  /// </param>
  /// <returns>A task representing the asynchronous write operation, returning the number of records processed.</returns>
  /// <remarks>
  /// Typically called by <see cref="WriteAsync"/>, but can also be used directly if writing
  /// to a specific stream is required. The method assumes that any time-sensitive data 
  /// aligns with the provided <paramref name="sourceTimeZone"/>.
  /// </remarks>
  Task WriteReaderAsync(IFileReader reader, Stream output, string sourceTimeZone, IProgressWithCancellation progress);
}
