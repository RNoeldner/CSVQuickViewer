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

namespace CsvTools
{
  /// <summary>
  /// Interface for a file that could be downloaded from a remote location to the file system
  /// </summary>
  /// <seealso cref="CsvTools.IFileSetting" />
  public interface IFileSettingRemoteDownload : IFileSetting
  {
    /// <summary>
    ///  Path to the file on sFTP Server
    /// </summary>
    string RemoteFileName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether tho throw an error if the remote file could not be found .
    /// </summary>
    /// <value>
    ///  <c>true</c> if throw an error if not exists; otherwise, <c>false</c>.
    /// </value>
    bool ThrowErrorIfNotExists { get; set; }
  }
}