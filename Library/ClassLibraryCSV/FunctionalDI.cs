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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   This class implements a lightweight Dependency injection without a framework
  ///
  ///   It uses a static delegate function to give the ability to overload the default functionality
  ///   by implementations not know to this library
  /// </summary>
  public static class FunctionalDI
  {
    /// <summary>
    ///   Retrieve the passphrase
    /// </summary>
    public static Func<IFileSetting, string> GetEncryptedPassphrase = (fileSetting) =>
    {
      if (fileSetting == null) return null;
      return !string.IsNullOrEmpty(fileSetting.Passphrase) ? fileSetting.Passphrase : null;
    };

    /// <summary>
    ///   Action to be performed while waiting on a background process, do something like handing
    ///   message queues (WinForms =&gt; DoEvents) call a Dispatcher to take care of the UI or send
    ///   singals that the application is not stale
    /// </summary>
    public static Action SignalBackground = null;

    /// <summary>
    ///   Open a file for reading, it will take care of things like compression and encryption
    /// </summary>
    public static Func<IFileSettingPhysicalFile, IImprovedStream> OpenRead = setting =>
      ImprovedStream.OpenRead(setting);

    /// <summary>
    ///   General function to open a file for writing, it will take care of things like compression
    ///   and encryption
    /// </summary>
    public static Func<IFileSettingPhysicalFile, IImprovedStream> OpenWrite = setting => ImprovedStream.OpenWrite(setting);

    /// <summary>
    ///   Timezone conversion, in case the conversion fails a error handler is called that does
    ///   match the base file readers HandleWarning the validation library will overwrite this is an
    ///   implementation using Noda Time
    /// </summary>
    public static Func<DateTime?, string, string, int, Action<int, string>, DateTime?> AdjustTZ =
      (input, srcTimeZone, destTimeZone, columnOrdinal, handleWarning) =>
      {
        if (!input.HasValue || string.IsNullOrEmpty(srcTimeZone) || string.IsNullOrEmpty(destTimeZone) ||
            srcTimeZone.Equals(destTimeZone))
          return input;
        try
        {
          // default implementation will convert using the .NET library
          return TimeZoneInfo.ConvertTime(input.Value, TimeZoneInfo.FindSystemTimeZoneById(srcTimeZone),
            TimeZoneInfo.FindSystemTimeZoneById(destTimeZone));
        }
        catch (Exception ex)
        {
          if (handleWarning == null) throw;
          handleWarning.Invoke(columnOrdinal, ex.Message);
          return null;
        }
      };

    /// <summary>
    ///   Function to retrieve the column in a setting file
    /// </summary>
    public static Func<IFileSetting, CancellationToken, ICollection<string>> GetColumnHeader;

    /// <summary>
    ///   Action to store the headers of a file in a cache
    /// </summary>
    public static Action<IFileSetting, IEnumerable<Column>> StoreHeader;

    /// <summary>
    ///   Return the right reader for a file setting
    /// </summary>
    public static Func<IFileSetting, string, IProcessDisplay, IFileReader> GetFileReader = DefaultFileReader;


    public static IFileReader DefaultFileReader(IFileSetting setting, string timeZone, IProcessDisplay processDisplay)
    {
      switch (setting)
      {
        case CsvFile csv when csv.JsonFormat:
          return new JsonFileReader(csv, timeZone, processDisplay);

        case CsvFile csv:
          return new CsvFileReader(csv, timeZone, processDisplay);

        default:
          throw new NotImplementedException($"Reader for {setting} not found");
      }
    }

    /// <summary>
    ///   Return a right writer for a file setting
    /// </summary>
    public static Func<IFileSetting, string, IProcessDisplay, IFileWriter> GetFileWriter = DefaultFileWriter;

    public static IFileWriter DefaultFileWriter(IFileSetting setting, string timeZone, IProcessDisplay processDisplay)
    {
      switch (setting)
      {
        case CsvFile csv when !csv.JsonFormat:
          return new CsvFileWriter(csv, timeZone, processDisplay);

        case StructuredFile structuredFile:
          return new StructuredFileWriter(structuredFile, timeZone, processDisplay);

        default:
          throw new NotImplementedException($"Writer for {setting} not found");
      }
    }

    /// <summary>
    ///   Gets or sets the SQL data reader.
    /// </summary>
    /// <value>The SQL data reader.</value>
    /// <exception cref="ArgumentNullException">SQL Data Reader is not set</exception>
    public static Func<string, IProcessDisplay, int, DbDataReader> SQLDataReader;

  }
}