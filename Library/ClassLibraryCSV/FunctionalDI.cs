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
    ///   Timezone conversion, in case the conversion fails a error handler is called that does
    ///   match the base file readers HandleWarning the validation library will overwrite this is an
    ///   implementation using Noda Time
    /// </summary>
    public static Func<DateTime?, string, int, Action<int, string>, DateTime?> AdjustTZImport =
      (input, srcTimeZone, columnOrdinal, handleWarning) =>
        ChangeTimeZone(input, srcTimeZone, TimeZoneInfo.Local.Id, columnOrdinal, handleWarning);

    public static Func<DateTime?, string, int, Action<int, string>, DateTime?> AdjustTZExport =
      (input, destTimeZone, columnOrdinal, handleWarning) =>
        ChangeTimeZone(input, TimeZoneInfo.Local.Id, destTimeZone, columnOrdinal, handleWarning);

    /// <summary>
    ///   Function to retrieve the column in a setting file
    /// </summary>
    public static Func<IFileSetting, CancellationToken, Task<ICollection<string>>>? GetColumnHeader;

    /// <summary>
    ///   Retrieve the passphrase for a files
    /// </summary>
    public static Func<string, string> GetEncryptedPassphraseForFile = s => string.Empty;

    /// <summary>
    ///   Retrieve the passphrase for a setting
    /// </summary>
    public static Func<IFileSettingPhysicalFile, string> GetEncryptedPassphrase = s => s.Passphrase;

    /// <summary>
    ///   Open a file for reading, it will take care of things like compression and encryption
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<SourceAccess, IImprovedStream> OpenStream = fileAccess => new ImprovedStream(fileAccess);

    /// <summary>
    ///   Action to be performed while waiting on a background process, do something like handing
    ///   message queues (WinForms =&gt; DoEvents) call a Dispatcher to take care of the UI or send
    ///   signals that the application is not stale
    /// </summary>
    public static Action SignalBackground = () => { };

    /// <summary>
    ///   Return the right reader for a file setting
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<IFileSetting, string, IProcessDisplay?, IFileReader> GetFileReader = DefaultFileReader;

    /// <summary>
    ///   Return a right writer for a file setting
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<IFileSettingPhysicalFile, IProcessDisplay?, IFileWriter> GetFileWriter = DefaultFileWriter;

    /// <summary>
    ///   Gets or sets a data reader
    /// </summary>
    /// <value>The statement for reader the data.</value>
    /// <remarks>Make sure the returned reader is open when needed</remarks>
    public static Func<string, EventHandler<ProgressEventArgs>?, int, CancellationToken, Task<IFileReader>>
      SQLDataReader = (sql, eh, limit, token) => throw new FileWriterException("SQL Reader not specified");

    private static DateTime? ChangeTimeZone(DateTime? input, string? srcTimeZone,
      string? destTimeZone, int columnOrdinal, Action<int, string>? handleWarning)
    {
      if (!input.HasValue || string.IsNullOrEmpty(srcTimeZone) || string.IsNullOrEmpty(destTimeZone) ||
          destTimeZone!.Equals(srcTimeZone))
        return input;
      try
      {
        // default implementation will convert using the .NET library
        return TimeZoneInfo.ConvertTime(input.Value, TimeZoneInfo.FindSystemTimeZoneById(srcTimeZone!),
          TimeZoneInfo.FindSystemTimeZoneById(destTimeZone));
      }
      catch (Exception ex)
      {
        if (handleWarning == null) throw;
        handleWarning.Invoke(columnOrdinal, ex.Message);
        return null;
      }
    }

    private static IFileReader DefaultFileReader(IFileSetting setting, string? timeZone,
      IProcessDisplay? processDisplay)
    {
      switch (setting)
      {
        case ICsvFile csv when csv.JsonFormat:
          return new JsonFileReader(csv.FullPath, csv.ColumnCollection, csv.RecordLimit, csv.TrimmingOption == TrimmingOption.All, csv.TreatTextAsNull, csv.TreatNBSPAsSpace, processDisplay);

        case ICsvFile csv:
          return new CsvFileReader(csv.FullPath, csv.CodePageId, csv.SkipRows, csv.HasFieldHeader, csv.ColumnCollection, csv.TrimmingOption, csv.FileFormat.FieldDelimiter, csv.FileFormat.FieldQualifier, csv.FileFormat.EscapeCharacter, csv.RecordLimit, csv.AllowRowCombining, csv.FileFormat.AlternateQuoting, csv.FileFormat.CommentLine, csv.NumWarnings, csv.FileFormat.DuplicateQuotingToEscape, csv.FileFormat.NewLinePlaceholder, csv.FileFormat.DelimiterPlaceholder, csv.FileFormat.QuotePlaceholder, csv.SkipDuplicateHeader, csv.TreatLFAsSpace, csv.TreatUnknownCharacterAsSpace, csv.TryToSolveMoreColumns, csv.WarnDelimiterInValue, csv.WarnLineFeed, csv.WarnNBSP, csv.WarnQuotes, csv.WarnUnknownCharacter, csv.WarnEmptyTailingColumns, csv.TreatNBSPAsSpace, csv.TreatTextAsNull, csv.SkipEmptyLines, csv.ConsecutiveEmptyRows, csv.IdentifierInContainer, processDisplay);

        default:
          throw new NotImplementedException($"Reader for {setting} not found");
      }
    }

    private static IFileWriter DefaultFileWriter(IFileSettingPhysicalFile physicalFile,
      IProcessDisplay? processDisplay)
    {
      IFileWriter? writer = null;
      switch (physicalFile)
      {
        case ICsvFile { JsonFormat: false } csv:
          writer = new CsvFileWriter(csv);
          break;

        case StructuredFile structuredFile:
          writer = new StructuredFileWriter(structuredFile, processDisplay);
          break;
      }

      if (writer == null)
        throw new NotImplementedException($"Writer for {physicalFile} not found");

      writer.WriteFinished += (sender, args) =>
      {
        physicalFile.ProcessTimeUtc = DateTime.UtcNow;
        if (physicalFile.SetLatestSourceTimeForWrite)
          new FileSystemUtils.FileInfo(physicalFile.FullPath).LastWriteTimeUtc = physicalFile.LatestSourceTimeUtc;
      };
      return writer;
    }
  }
}