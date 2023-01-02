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
#if !QUICK
using System.Threading;
#endif

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
    ///   Retrieve the passphrase for a file, a passphrase store can be attached here
    /// </summary>
    /// <note>
    /// Currently only used in <see cref="SourceAccess"/> to gte the passphrase for a PGP encrypted file
    /// </note>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<string, string> GetEncryptedPassphraseForFile = s => string.Empty;

    /// <summary>
    ///   Open a <see cref="SourceAccess"/> for reading in a stream, will take care of things like compression and encryption
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<SourceAccess, Stream> OpenStream = fileAccess => new ImprovedStream(fileAccess);

#if !QUICK

    private static readonly IFileReaderWriterFactory m_FileReaderWriterFactory =
      new ClassLibraryCsvFileReaderWriterFactory(StandardTimeZoneAdjust.ChangeTimeZone);

    /// <summary>
    ///   Return a right writer for a file setting
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<IFileSetting, CancellationToken, IFileWriter> GetFileWriter =
      (setting, cancellationToken) =>
        m_FileReaderWriterFactory.GetFileWriter(setting, cancellationToken);

    /// <summary>
    ///   Return the right reader for a file setting
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<IFileSetting,  CancellationToken, IFileReader> GetFileReader =
      (setting, cancellationToken) =>
        m_FileReaderWriterFactory.GetFileReader(setting, cancellationToken);


#endif
  }
}