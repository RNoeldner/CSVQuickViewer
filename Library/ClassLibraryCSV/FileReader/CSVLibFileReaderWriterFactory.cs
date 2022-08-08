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
    public IFileReader GetFileReader(in IFileSetting setting, in IProgress<ProgressInfo>? processDisplay,
      in CancellationToken cancellationToken)
    {
      IFileReader retReader = setting switch
      {
        IJsonFile csv1 => new JsonFileReader(csv1.FullPath, csv1.ColumnCollection, csv1.RecordLimit,
          csv1.TrimmingOption == TrimmingOptionEnum.All, csv1.TreatTextAsNull, csv1.TreatNBSPAsSpace, m_TimeZoneAdjust,
          TimeZoneInfo.Local.Id),
        ICsvFile csv2 => new CsvFileReader(csv2.FullPath, csv2.CodePageId, csv2.SkipRows, csv2.HasFieldHeader,
          csv2.ColumnCollection, csv2.TrimmingOption, csv2.FieldDelimiter, csv2.FieldQualifier, csv2.EscapePrefix,
          csv2.RecordLimit, csv2.AllowRowCombining, csv2.ContextSensitiveQualifier, csv2.CommentLine, csv2.NumWarnings,
          csv2.DuplicateQualifierToEscape, csv2.NewLinePlaceholder, csv2.DelimiterPlaceholder,
          csv2.QualifierPlaceholder, csv2.SkipDuplicateHeader, csv2.TreatLfAsSpace, csv2.TreatUnknownCharacterAsSpace,
          csv2.TryToSolveMoreColumns, csv2.WarnDelimiterInValue, csv2.WarnLineFeed, csv2.WarnNBSP, csv2.WarnQuotes,
          csv2.WarnUnknownCharacter, csv2.WarnEmptyTailingColumns, csv2.TreatNBSPAsSpace, csv2.TreatTextAsNull,
          csv2.SkipEmptyLines, csv2.ConsecutiveEmptyRows, csv2.IdentifierInContainer, m_TimeZoneAdjust,
          TimeZoneInfo.Local.Id),
        _ => throw new FileReaderException($"Reader for {setting} not found")
      };
      if (processDisplay != null)
        retReader.ReportProgress=processDisplay;
      return retReader;
    }

    /// <inheritdoc />
    public IFileWriter GetFileWriter(IFileSetting fileSetting, in IProgress<ProgressInfo>? processDisplay, in CancellationToken cancellationToken)
    {
      IFileWriter? writer = fileSetting switch
      {
        ICsvFile csv => new CsvFileWriter(csv.ID, csv.FullPath, csv.HasFieldHeader, csv.DefaultValueFormatWrite,
          csv.CodePageId, csv.ByteOrderMark, csv.ColumnCollection, csv.KeyID, csv.KeepUnencrypted,
          csv.IdentifierInContainer, csv.Header, csv.Footer, csv.ToString(), csv.NewLine, csv.FieldDelimiterChar,
          csv.FieldQualifierChar, csv.EscapePrefixChar, csv.NewLinePlaceholder, csv.DelimiterPlaceholder,
          csv.QualifierPlaceholder, csv.QualifyAlways, csv.QualifyOnlyIfNeeded, m_TimeZoneAdjust,
          TimeZoneInfo.Local.Id),
        IJsonFile jsonFile => new JsonFileWriter(fileSetting.ID, jsonFile.FullPath, jsonFile.KeyID,
          jsonFile.KeepUnencrypted, jsonFile.IdentifierInContainer, jsonFile.Footer, jsonFile.Header,
          jsonFile.EmptyAsNull, jsonFile.CodePageId, jsonFile.ByteOrderMark, jsonFile.ColumnCollection,
          Convert.ToString(jsonFile), jsonFile.Row, m_TimeZoneAdjust, TimeZoneInfo.Local.Id),
        IXmlFile xmlFile => new XmlFileWriter(xmlFile.ID, xmlFile.FullPath, xmlFile.KeyID, xmlFile.KeepUnencrypted,
          xmlFile.IdentifierInContainer, xmlFile.Footer, xmlFile.Header, xmlFile.CodePageId, xmlFile.ByteOrderMark,
          xmlFile.ColumnCollection, Convert.ToString(xmlFile), xmlFile.Row, m_TimeZoneAdjust,
          TimeZoneInfo.Local.Id),
        _ => null
      };

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

    public Task<IFileReader> SqlDataReader(in string sql, in IProgress<ProgressInfo>? processDisplay, int commandTimeout,
      long recordLimit, CancellationToken cancellationToken) => throw new NotImplementedException();
  }
}