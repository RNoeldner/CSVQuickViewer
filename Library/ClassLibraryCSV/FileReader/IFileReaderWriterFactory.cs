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

using System.Threading;

namespace CsvTools
{
  /// <summary>
  /// Factory to create <see cref="IFileReader"/>  or <see cref="IFileWriter"/> 
  /// </summary>
  public interface IFileReaderWriterFactory
  {
    /// <summary>
    ///  Get an instance of a <see cref="IFileReader"/> based on the passed in IFileSetting
    /// </summary>
    /// <param name="fileSetting">The setting for the reader</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>A <see cref="IFileReader"/> capable to import data</returns>
    IFileReader GetFileReader(IFileSetting fileSetting, CancellationToken cancellationToken);

    /// <summary>
    ///  Get an instance of a <see cref="IFileWriter"/> based on the passed in IFileSetting
    /// </summary>
    /// <param name="fileSetting">The setting for the writer</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>A <see cref="IFileWriter"/> capable to export data</returns>
    IFileWriter GetFileWriter(IFileSetting fileSetting, CancellationToken cancellationToken);
  }
}
