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
  ///  Interface for a file based settings
  /// </summary>
  public interface IFileSettingPhysicalFile : IValidatorSetting
  {
    /// <summary>
    ///   May store information on columns to show, filtering and sorting
    /// </summary>
    string ColumnFile { get; set; }

    /// <summary>
    ///   Gets or sets the name of the file, this value could be a relative path
    /// </summary>
    /// <value>The name of the file.</value>
    string FileName { get; set; }

    /// <summary>
    ///   The Size of the file in Byte
    /// </summary>
    long FileSize { get; set; }

    /// <summary>
    ///   Gets the full path of the Filename
    /// </summary>
    /// <value>The full path of the file <see cref="FileName" /> /&gt;</value>
    string FullPath { get; }

    /// <summary>
    /// If the FullPath determines a container like Zip, this will identify the files  
    /// </summary>
    string IdentifierInContainer { get; set; }

    /// <summary>
    ///   Passphrase for Decryption of Zip or PGP
    /// </summary>
    string Passphrase { get; set; }

    /// <summary>
    ///   Path to the private PGP key used during decryption / read
    /// </summary>
    string KeyFile { get; set; }

    /// <summary>
    ///   Path to the file on sFTP Server
    /// </summary>
    string RemoteFileName { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether the byte order mark should be written in Unicode files.
    /// </summary>
    /// <value><c>true</c> write byte order mark; otherwise, <c>false</c>.</value>
    bool ByteOrderMark { get; set; }

    /// <summary>
    ///   Gets or sets the code page.
    /// </summary>
    /// <value>The code page.</value>
    int CodePageId { get; set; }

    /// <summary>
    /// The base folder for relative path
    /// </summary>
    string RootFolder { get; set; }

    /// <summary>
    ///   In case of creating a file, should the time of the latest source be used?
    ///   Default: <c>false</c> - Use the current datetime for the file, otherwise use the time of
    ///   the latest source
    /// </summary>
    bool SetLatestSourceTimeForWrite { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether tho throw an error if the remote file could not be
    ///   found .
    /// </summary>
    /// <value><c>true</c> if throw an error if not exists; otherwise, <c>false</c>.</value>
    bool ThrowErrorIfNotExists { get; set; }


    /// <summary>
    /// ValueFormat to be used if no column specific ValueFormat is defined 
    /// </summary>
    ValueFormat ValueFormatWrite { get; set; }

    /// <summary>
    ///   Force the refresh of full path information, a filename with placeholders might need to
    ///   check again if there is a new file
    /// </summary>
    void ResetFullPath();
  }
}