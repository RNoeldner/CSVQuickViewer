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

using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public interface IFileReaderWriterFactory
  {
    /// <summary>
    ///  Get an instance of a <see cref="IFileReader"/> based on the passed in IFileSetting
    /// </summary>
    /// <param name="setting">The setting the reader should read</param>
    /// <param name="timeZone"></param>
    /// <param name="processDisplay">Used Process/Progress reporting</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="IFileReader"/> capable to import data</returns>
    IFileReader GetFileReader(in IFileSetting setting, in string? timeZone, in IProcessDisplay? processDisplay,
      in CancellationToken cancellationToken);

    /// <summary>
    ///  Get an instance of a <see cref="IFileWriter"/> based on the passed in IFileSetting
    /// </summary>
    /// <param name="fileSetting">The setting the reader should read</param>
    /// <param name="processDisplay">Used Process/Progress reporting</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="IFileWriter"/> capable to export data</returns>
    IFileWriter GetFileWriter(IFileSetting fileSetting, in IProcessDisplay? processDisplay,
      in CancellationToken cancellationToken);

    /// <summary>
    ///  Get an instance of a <see cref="IFileReader"/> that does read a SQL statement
    /// </summary>
    /// <param name="sql">SQL statement to execute</param>
    /// <param name="processDisplay">Used Process/Progress reporting</param>
    /// <param name="commandTimeout">Timeout in Seconds to process the SQL statement</param>
    /// <param name="recordLimit">Maximum number of records to be returned</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>A <see cref="IFileReader"/> capable of reading data from the attached database</returns>
    Task<IFileReader> SqlDataReader(in string sql, in IProcessDisplay? processDisplay, int commandTimeout,
      long recordLimit, CancellationToken cancellationToken);
  }
}