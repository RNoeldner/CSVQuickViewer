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
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  public class ClassLibraryCSVFileReaderWriterFactory : IFileReaderWriterFactory
  {
    private readonly TimeZoneChangeDelegate m_TimeZoneAdjust;

    public ClassLibraryCSVFileReaderWriterFactory(TimeZoneChangeDelegate timeZoneAdjust) => m_TimeZoneAdjust = timeZoneAdjust;

    /// <inheritdoc />
    public IFileReader GetFileReader(in IFileSetting setting, in IProcessDisplay? processDisplay,
      in CancellationToken cancellationToken) =>
      setting switch
      {
        IJsonFile csv => new JsonFileReader(
          csv.FullPath,
          csv.ColumnCollection,
          csv.RecordLimit,
          csv.TrimmingOption == TrimmingOptionEnum.All,
          csv.TreatTextAsNull,
          csv.TreatNBSPAsSpace,
          m_TimeZoneAdjust, TimeZoneInfo.Local.Id,
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
          m_TimeZoneAdjust, TimeZoneInfo.Local.Id,
          processDisplay),
        _ => throw new FileReaderException($"Reader for {setting} not found")
      };

    /// <inheritdoc />
    public IFileWriter GetFileWriter(IFileSetting fileSetting, in IProcessDisplay? processDisplay,
      in CancellationToken cancellationToken)
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
            m_TimeZoneAdjust, System.TimeZoneInfo.Local.Id,
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
            jsonFile.EmptyAsNull,
            jsonFile.CodePageId,
            jsonFile.ByteOrderMark,
            jsonFile.ColumnCollection,
            Convert.ToString(jsonFile),
            jsonFile.Row,
m_TimeZoneAdjust, System.TimeZoneInfo.Local.Id,
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
            m_TimeZoneAdjust, System.TimeZoneInfo.Local.Id,
            processDisplay);
          break;
      }

      if (writer is null)
        throw new FileWriterException($"Writer for {fileSetting} not found");

      writer.WriteFinished += (sender, args) =>
      {
        fileSetting.ProcessTimeUtc = DateTime.UtcNow;
        if (fileSetting is IFileSettingPhysicalFile { SetLatestSourceTimeForWrite: true } physicalFile)
          new FileSystemUtils.FileInfo(physicalFile.FullPath).LastWriteTimeUtc = fileSetting.LatestSourceTimeUtc;
      };
      return writer;
    }

    public Task<IFileReader> SqlDataReader(in string sql, in IProcessDisplay? processDisplay, int commandTimeout,
      long recordLimit, CancellationToken cancellationToken) => throw new NotImplementedException();
  }
}