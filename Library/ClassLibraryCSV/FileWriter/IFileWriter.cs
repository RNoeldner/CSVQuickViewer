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
#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   Interface for a File Writer.
  /// </summary>
  public interface IFileWriter
  {
    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    event EventHandler<WarningEventArgs>? Warning;

    /// <summary>
    ///   Event handler called once writing of the file is completed
    /// </summary>
    event EventHandler? WriteFinished;

    /// <summary>
    ///   Writes data reading from the source
    /// </summary>
    /// <param name="source">The data that should be used as source </param>
    /// <param name="token">A cancellation token to stop a long running process</param>
    /// <returns>Number of records written; -1 if there is no source; -2 if canceled</returns>
    Task<long> WriteAsync(IFileReader? source, CancellationToken token);

    /// <summary>Writes data to the stream reading from the source</summary>
    /// <param name="reader">The data that should be read</param>
    /// <param name="output">The Stream to write to</param>
    /// <param name="cancellationToken">A cancellation token to stop a long running process</param>
    /// <returns>Number of records written</returns>
    Task WriteReaderAsync(IFileReader reader, Stream output, CancellationToken cancellationToken);

    /// <summary>
    ///   Sets the progress reporting action <see cref="ProgressInfo.Value"/> will be the current record that has been written/>
    /// </summary>
    IProgress<ProgressInfo> ReportProgress { set; }
  }
}