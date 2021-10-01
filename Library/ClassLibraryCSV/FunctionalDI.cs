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
using TimeZoneConverter;
using System.Runtime.InteropServices;

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
    public static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    ///   Timezone conversion, in case the conversion fails a error handler is called that does
    ///   match the base file readers HandleWarning the validation library will overwrite this is an
    ///   implementation using Noda Time
    /// </summary>
    public static Func<DateTime, string, int, Action<int, string>?, DateTime> AdjustTZImport =
      (input, srcTimeZone, columnOrdinal, handleWarning) => ChangeTimeZone(
        input,
        srcTimeZone,
        TimeZoneInfo.Local.Id,
        columnOrdinal,
        handleWarning);

    public static Func<DateTime, string, int, Action<int, string>?, DateTime> AdjustTZExport =
      (input, destTimeZone, columnOrdinal, handleWarning) => ChangeTimeZone(
        input,
        TimeZoneInfo.Local.Id,
        destTimeZone,
        columnOrdinal,
        handleWarning);

    /// <summary>
    ///   Retrieve the passphrase for a files
    /// </summary>
    public static Func<string, string> GetEncryptedPassphraseForFile = s => string.Empty;

    /// <summary>
    ///   Open a file for reading, it will take care of things like compression and encryption
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<SourceAccess, IImprovedStream> OpenStream = fileAccess => new ImprovedStream(fileAccess);

    private static DateTime ChangeTimeZone(
      in DateTime input,
      in string srcTimeZone,
      in string destTimeZone,
      int columnOrdinal,
      in Action<int, string>? handleWarning)
    {
      if (string.IsNullOrEmpty(srcTimeZone) || string.IsNullOrEmpty(destTimeZone) || destTimeZone.Equals(srcTimeZone))
        return input;
      try
      {
        if (IsWindows)
        {
          TimeZoneInfo srcTimeZoneInfo = (TZConvert.TryIanaToWindows(srcTimeZone, out var winSrc)) ? TimeZoneInfo.FindSystemTimeZoneById(winSrc) : TimeZoneInfo.FindSystemTimeZoneById(srcTimeZone);
          TimeZoneInfo destTimeZoneInfo = (TZConvert.TryIanaToWindows(destTimeZone, out var winDest)) ? TimeZoneInfo.FindSystemTimeZoneById(winDest) : TimeZoneInfo.FindSystemTimeZoneById(destTimeZone);

          return TimeZoneInfo.ConvertTime(input, srcTimeZoneInfo, destTimeZoneInfo);
        }
        else
        {
          TimeZoneInfo srcTimeZoneInfo = TZConvert.TryWindowsToIana(srcTimeZone, out var inaraSrc)
                                           ? TimeZoneInfo.FindSystemTimeZoneById(inaraSrc)
                                           : TimeZoneInfo.FindSystemTimeZoneById(srcTimeZone);
          TimeZoneInfo destTimeZoneInfo = TZConvert.TryWindowsToIana(destTimeZone, out var inaraDest)
                                            ? TimeZoneInfo.FindSystemTimeZoneById(inaraDest)
                                            : TimeZoneInfo.FindSystemTimeZoneById(destTimeZone);
          return TimeZoneInfo.ConvertTime(input, srcTimeZoneInfo, destTimeZoneInfo);
        }
      }
      catch (Exception ex)
      {
        if (handleWarning is null) throw;
        handleWarning.Invoke(columnOrdinal, ex.Message);
        return new DateTime();
      }
    }

#if !QUICK

    /// <summary>
    ///   Function to retrieve the column in a setting file
    /// </summary>
    public static Func<IFileSetting, CancellationToken, Task<ICollection<string>>>? GetColumnHeader;

    /// <summary>
    ///   Retrieve the passphrase for a setting
    /// </summary>
    public static Func<IFileSettingPhysicalFile, string> GetEncryptedPassphrase = s => s.Passphrase;

    /// <summary>
    ///   Return a right writer for a file setting
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<IFileSettingPhysicalFile, IProcessDisplay?, IFileWriter> GetFileWriter = DefaultFileWriter;

    /// <summary>
    ///   Return the right reader for a file setting
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<IFileSetting, string?, IProcessDisplay?, IFileReader> GetFileReader = DefaultFileReader;

    /// <summary>
    ///   Gets or sets a data reader
    /// </summary>
    /// <value>The statement for reader the data.</value>
    /// <remarks>Make sure the returned reader is open when needed</remarks>
    public static Func<string, EventHandler<ProgressEventArgs>?, int, CancellationToken, Task<IFileReader>>
      SQLDataReader = (sql, eh, limit, token) => throw new FileWriterException("SQL Reader not specified");

    /// <summary>
    ///   Action to be performed while waiting on a background process, do something like handing
    ///   message queues (WinForms =&gt; DoEvents) call a Dispatcher to take care of the UI or send
    ///   signals that the application is not stale
    /// </summary>
    public static Action SignalBackground = () => { };

    private static IFileReader DefaultFileReader(
      IFileSetting setting,
      string? timeZone,
      IProcessDisplay? processDisplay) =>
      setting switch
      {
        IJsonFile csv => new JsonFileReader(
          csv.FullPath,
          csv.ColumnCollection,
          csv.RecordLimit,
          csv.TrimmingOption == TrimmingOption.All,
          csv.TreatTextAsNull,
          csv.TreatNBSPAsSpace,
          processDisplay),
        ICsvFile csv => new CsvFileReader(
          csv.FullPath,
          csv.CodePageId,
          csv.SkipRows,
          csv.HasFieldHeader,
          csv.ColumnCollection,
          csv.TrimmingOption,
          csv.FieldDelimiter,
          csv.FieldQualifier,
          csv.EscapePrefix,
          csv.RecordLimit,
          csv.AllowRowCombining,
          csv.ContextSensitiveQualifier,
          csv.CommentLine,
          csv.NumWarnings,
          csv.DuplicateQualifierToEscape,
          csv.NewLinePlaceholder,
          csv.DelimiterPlaceholder,
          csv.QualifierPlaceholder,
          csv.SkipDuplicateHeader,
          csv.TreatLFAsSpace,
          csv.TreatUnknownCharacterAsSpace,
          csv.TryToSolveMoreColumns,
          csv.WarnDelimiterInValue,
          csv.WarnLineFeed,
          csv.WarnNBSP,
          csv.WarnQuotes,
          csv.WarnUnknownCharacter,
          csv.WarnEmptyTailingColumns,
          csv.TreatNBSPAsSpace,
          csv.TreatTextAsNull,
          csv.SkipEmptyLines,
          csv.ConsecutiveEmptyRows,
          csv.IdentifierInContainer,
          processDisplay),
        _ => throw new FileReaderException($"Reader for {setting} not found")
      };

    private static IFileWriter DefaultFileWriter(IFileSettingPhysicalFile physicalFile, IProcessDisplay? processDisplay)
    {
      IFileWriter? writer = null;
      switch (physicalFile)
      {
        case ICsvFile csv:
          writer = new CsvFileWriter(
            csv.ID,
            csv.FullPath,
            csv.HasFieldHeader,
            csv.DefaultValueFormatWrite,
            csv.CodePageId,
            csv.ByteOrderMark,
            csv.ColumnCollection,
            csv.Recipient,
            csv.KeepUnencrypted,
            csv.IdentifierInContainer,
            csv.Header,
            csv.Footer,
            csv.ToString(),
            csv.NewLine,
            csv.FieldDelimiterChar,
            csv.FieldQualifierChar,
            csv.EscapePrefixChar,
            csv.NewLinePlaceholder,
            csv.DelimiterPlaceholder,
            csv.QualifierPlaceholder,
            csv.QualifyAlways,
            csv.QualifyOnlyIfNeeded,
            processDisplay);
          break;

        case IJsonFile fileSetting:
          writer = new JsonFileWriter(
            fileSetting.ID,
            fileSetting.FullPath,
            fileSetting.Recipient,
            fileSetting.KeepUnencrypted,
            fileSetting.IdentifierInContainer,
            fileSetting.Footer,
            fileSetting.Header,
            fileSetting.ColumnCollection,
            Convert.ToString(fileSetting),
            fileSetting.Row,
            processDisplay);
          break;

        case IXMLFile fileSetting:
          writer = new XMLFileWriter(
            fileSetting.ID,
            fileSetting.FullPath,
            fileSetting.Recipient,
            fileSetting.KeepUnencrypted,
            fileSetting.IdentifierInContainer,
            fileSetting.Footer,
            fileSetting.Header,
            fileSetting.ColumnCollection,
            Convert.ToString(fileSetting),
            fileSetting.Row,
            processDisplay);
          break;
      }

      if (writer is null)
        throw new FileWriterException($"Writer for {physicalFile} not found");

      writer.WriteFinished += (sender, args) =>
      {
        physicalFile.ProcessTimeUtc = DateTime.UtcNow;
        if (physicalFile.SetLatestSourceTimeForWrite)
          new FileSystemUtils.FileInfo(physicalFile.FullPath).LastWriteTimeUtc = physicalFile.LatestSourceTimeUtc;
      };
      return writer;
    }

#endif
  }
}