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
  /// <summary>
  ///   This class implements a lightweight Dependency injection without a framework It uses a
  ///   static delegate function to give the ability to overload the default functionality by
  ///   implementations not know to this library
  /// </summary>
  // ReSharper disable once InconsistentNaming
  public static class FunctionalDI
  {
    /// <summary>
    ///   Retrieve the passphrase for a files
    /// </summary>
    public static Func<string, string> GetEncryptedPassphraseForFile = s => string.Empty;

    /// <summary>
    ///   Open a file for reading, it will take care of things like compression and encryption
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<SourceAccess, Stream> OpenStream = fileAccess => new ImprovedStream(fileAccess);

#if !QUICK

    private static readonly IFileReaderWriterFactory m_FileReaderWriterFactory =
      new ClassLibraryCSVFileReaderWriterFactory(StandardTimeZoneAdjust.ChangeTimeZone);

    /// <summary>
    ///   Return a right writer for a file setting
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<IFileSetting, IProgress<ProgressInfo>?, CancellationToken, IFileWriter> GetFileWriter =
      (setting, processDisplay, cancellationToken) =>
        m_FileReaderWriterFactory.GetFileWriter(setting, processDisplay, cancellationToken);

    /// <summary>
    ///   Return the right reader for a file setting
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<IFileSetting, IProgress<ProgressInfo>?, CancellationToken, IFileReader> GetFileReader =
      (setting, processDisplay, cancellationToken) =>
        m_FileReaderWriterFactory.GetFileReader(setting, processDisplay, cancellationToken);

    /// <summary>
    ///   Gets or sets a data reader
    /// </summary>
    /// <value>The statement for reader the data.</value>
    /// <remarks>Make sure the returned reader is open when needed</remarks>
    public static Func<string, IProgress<ProgressInfo>?, int, long, CancellationToken, Task<IFileReader>> SqlDataReader =
      (sql, processDisplay, commandTimeout, recordLimit, token) =>
        throw new FileWriterException("SQL Reader not specified");

#endif
  }
}