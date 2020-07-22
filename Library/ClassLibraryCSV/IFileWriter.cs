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
using System.Data;
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
    ///   Gets the error message.
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    event EventHandler<WarningEventArgs> Warning;

    /// <summary>
    /// Event handler called once writing of the file is completed
    /// </summary>
    event EventHandler WriteFinished;

    /// <summary>
    ///   Writes the specified file.
    /// </summary>
    /// <param name="token">A cancellation toke to stop a long running process</param>
    /// <returns>Number of records written</returns>
    Task<long> WriteAsync(CancellationToken token);

    /// <summary>
    ///   Writes the specified file reading from the a data table
    /// </summary>
    /// <param name="source">The data that should be written in a <see cref="DataTable" /></param>
    /// <param name="token">A cancellation toke to stop a long running process</param>
    /// <returns>Number of records written</returns>
    Task<long> WriteAsync([NotNull] IFileReader source, CancellationToken token);
  }
}