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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TimeZoneConverter;

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
    public static Func<DateTime, string, Action<string>?, DateTime> AdjustTZImport =
      (input, srcTimeZone, handleWarning) => ChangeTimeZone(
        input,
        srcTimeZone,
        TimeZoneInfo.Local.Id,
        handleWarning);

    public static Func<DateTime, string, Action<string>?, DateTime> AdjustTZExport =
      (input, destTimeZone, handleWarning) => ChangeTimeZone(
        input,
        TimeZoneInfo.Local.Id,
        destTimeZone,
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
      in Action<string>? handleWarning)
    {
      if (string.IsNullOrEmpty(srcTimeZone) || string.IsNullOrEmpty(destTimeZone) || destTimeZone.Equals(srcTimeZone))
        return input;
      try
      {
        TimeZoneInfo srcTimeZoneInfo;
        TimeZoneInfo destTimeZoneInfo;

        if (srcTimeZone.Equals("(local)", StringComparison.Ordinal))
          srcTimeZoneInfo = TimeZoneInfo.Local;
        else
        {
          srcTimeZoneInfo = IsWindows ? TimeZoneInfo.FindSystemTimeZoneById(TZConvert.TryIanaToWindows(srcTimeZone, out var winSrc) ? winSrc : srcTimeZone) : TimeZoneInfo.FindSystemTimeZoneById(TZConvert.TryWindowsToIana(srcTimeZone, out var inaraSrc) ? inaraSrc : srcTimeZone);
        }

        if (destTimeZone.Equals("(local)", StringComparison.Ordinal))
          destTimeZoneInfo = TimeZoneInfo.Local;
        else
        {
          destTimeZoneInfo = IsWindows ? TimeZoneInfo.FindSystemTimeZoneById(TZConvert.TryIanaToWindows(destTimeZone, out var winSrc) ? winSrc : destTimeZone) : TimeZoneInfo.FindSystemTimeZoneById(TZConvert.TryWindowsToIana(destTimeZone, out var inaraDest) ? inaraDest : destTimeZone);
        }

        return TimeZoneInfo.ConvertTime(input, srcTimeZoneInfo, destTimeZoneInfo);
      }
      catch (Exception ex)
      {
        if (handleWarning is null) throw;
        handleWarning.Invoke(ex.Message);
        return new DateTime();
      }
    }

#if !QUICK
    /// <summary>
    ///   Return a right writer for a file setting
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<IFileSetting, IProcessDisplay?, CancellationToken, IFileWriter> GetFileWriter =
      (setting, processDisplay, cancellationToken) => DefaultFileWriter(setting, processDisplay);

    /// <summary>
    ///   Return the right reader for a file setting
    /// </summary>
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static Func<IFileSetting, string?, IProcessDisplay?, CancellationToken, IFileReader> GetFileReader =
      (setting, timeZone, processDisplay, cancellationToken) => DefaultFileReader(setting, processDisplay);

    /// <summary>
    ///   Gets or sets a data reader
    /// </summary>
    /// <value>The statement for reader the data.</value>
    /// <remarks>Make sure the returned reader is open when needed</remarks>
    public static Func<string, IProcessDisplay?, int, CancellationToken, Task<IFileReader>> SQLDataReader = (sql, processDisplay, limit, token) =>
      throw new FileWriterException("SQL Reader not specified");

    private static IFileReader DefaultFileReader(
      IFileSetting setting,
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
          csv.TreatLfAsSpace,
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

    private static IFileWriter DefaultFileWriter(IFileSetting fileSetting, IProcessDisplay? processDisplay)
    {
      IFileWriter? writer = null;

      switch (fileSetting)
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
            csv.KeyID,
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

        case IJsonFile jsonFile:
          writer = new JsonFileWriter(
            fileSetting.ID,
            jsonFile.FullPath,
            jsonFile.KeyID,
            jsonFile.KeepUnencrypted,
            jsonFile.IdentifierInContainer,
            jsonFile.Footer,
            jsonFile.Header,
            jsonFile.CodePageId,
            jsonFile.ByteOrderMark,
            jsonFile.ColumnCollection,
            Convert.ToString(jsonFile),
            jsonFile.Row,
            processDisplay);
          break;

        case IXmlFile xmlFile:
          writer = new XmlFileWriter(
            xmlFile.ID,
            xmlFile.FullPath,
            xmlFile.KeyID,
            xmlFile.KeepUnencrypted,
            xmlFile.IdentifierInContainer,
            xmlFile.Footer,
            xmlFile.Header,
            xmlFile.CodePageId,
            xmlFile.ByteOrderMark,
            xmlFile.ColumnCollection,
            Convert.ToString(xmlFile),
            xmlFile.Row,
            processDisplay);
          break;
      }

      if (writer is null)
        throw new FileWriterException($"Writer for {fileSetting} not found");

      writer.WriteFinished += (sender, args) =>
      {
        fileSetting.ProcessTimeUtc = DateTime.UtcNow;
        if (fileSetting is IFileSettingPhysicalFile {SetLatestSourceTimeForWrite: true} physicalFile) 
          new FileSystemUtils.FileInfo(physicalFile.FullPath).LastWriteTimeUtc = fileSetting.LatestSourceTimeUtc;
      };
      return writer;
    }

#endif
  }
}