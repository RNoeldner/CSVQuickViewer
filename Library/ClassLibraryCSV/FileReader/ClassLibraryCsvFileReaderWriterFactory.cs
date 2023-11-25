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

#nullable enable

using System;
using System.Threading;


namespace CsvTools
{
  /// <inheritdoc />
  public class ClassLibraryCsvFileReaderWriterFactory : IFileReaderWriterFactory
  {
    private readonly TimeZoneChangeDelegate m_TimeZoneAdjust;
    private readonly FillGuessSettings m_FillGuessSettings;

    /// <summary>Initializes a new instance of the <see cref="ClassLibraryCsvFileReaderWriterFactory" /> class.</summary>
    /// <param name="timeZoneAdjust">The routine to do time zone adjustments</param>
    /// <param name="fillGuessSettings">The guess settings</param>
    public ClassLibraryCsvFileReaderWriterFactory(TimeZoneChangeDelegate timeZoneAdjust, FillGuessSettings fillGuessSettings)
    {
      m_TimeZoneAdjust = timeZoneAdjust;
      m_FillGuessSettings = fillGuessSettings;
    }

    /// <inheritdoc />
    public IFileReader GetFileReader(IFileSetting setting, CancellationToken cancellationToken)
    {
      IFileReader retReader = setting switch
      {
        IJsonFile csv1 => new JsonFileReader(csv1.FullPath, csv1.ColumnCollection, csv1.RecordLimit,
          csv1.TrimmingOption == TrimmingOptionEnum.All, csv1.TreatTextAsNull, csv1.TreatNBSPAsSpace, m_TimeZoneAdjust,
          TimeZoneInfo.Local.Id, false, false),
        IXmlFile xml => new XmlFileReader(xml.FullPath, xml.ColumnCollection, xml.RecordLimit,
          xml.TrimmingOption == TrimmingOptionEnum.All, xml.TreatTextAsNull, xml.TreatNBSPAsSpace, m_TimeZoneAdjust, TimeZoneInfo.Local.Id, false, false),
        ICsvFile csv2 => new CsvFileReader(csv2.FullPath, csv2.CodePageId, csv2.SkipRows, csv2.HasFieldHeader,
          csv2.ColumnCollection, csv2.TrimmingOption, csv2.FieldDelimiterChar, csv2.FieldQualifierChar, csv2.EscapePrefixChar,
          csv2.RecordLimit, csv2.AllowRowCombining, csv2.ContextSensitiveQualifier, csv2.CommentLine, csv2.NumWarnings,
          csv2.DuplicateQualifierToEscape, csv2.NewLinePlaceholder, csv2.DelimiterPlaceholder,
          csv2.QualifierPlaceholder, csv2.SkipDuplicateHeader, csv2.TreatLfAsSpace, csv2.TreatUnknownCharacterAsSpace,
          csv2.TryToSolveMoreColumns, csv2.WarnDelimiterInValue, csv2.WarnLineFeed, csv2.WarnNBSP, csv2.WarnQuotes,
          csv2.WarnUnknownCharacter, csv2.WarnEmptyTailingColumns, csv2.TreatNBSPAsSpace, csv2.TreatTextAsNull,
          csv2.SkipEmptyLines, csv2.ConsecutiveEmptyRows, csv2.IdentifierInContainer, m_TimeZoneAdjust,
          TimeZoneInfo.Local.Id, m_FillGuessSettings.DetectPercentage, m_FillGuessSettings.RemoveCurrencySymbols),
        _ => throw new FileReaderException($"Reader for {setting} not found")
      };
      return retReader;
    }

    /// <inheritdoc />
    public IFileWriter GetFileWriter(IFileSetting fileSetting, CancellationToken cancellationToken)
    {
      
#if SupportPGP
      var publicKey = string.Empty;
      if (fileSetting is IFileSettingPhysicalFile physicalFile)
        publicKey = PgpHelper.GetKeyAndValidate(physicalFile.FileName, physicalFile.KeyFile);
#endif
      IFileWriter? writer = fileSetting switch
      {
        ICsvFile csv => new CsvFileWriter(csv.ID, csv.FullPath, csv.HasFieldHeader, csv.ValueFormatWrite,
          csv.CodePageId, csv.ByteOrderMark, csv.ColumnCollection, csv.IdentifierInContainer,
          csv.Header, csv.Footer, csv.ToString(), csv.NewLine, csv.FieldDelimiterChar, csv.FieldQualifierChar,
          csv.EscapePrefixChar, csv.NewLinePlaceholder, csv.DelimiterPlaceholder, csv.QualifierPlaceholder,
          csv.QualifyAlways, csv.QualifyOnlyIfNeeded, csv.WriteFixedLength, m_TimeZoneAdjust, TimeZoneInfo.Local.Id
#if SupportPGP
          ,publicKey, csv.KeepUnencrypted
#endif
          ),
        IJsonFile jsonFile => new JsonFileWriter(fileSetting.ID, jsonFile.FullPath,
          jsonFile.IdentifierInContainer, jsonFile.Footer, jsonFile.Header, jsonFile.EmptyAsNull,
          jsonFile.CodePageId, jsonFile.ByteOrderMark, jsonFile.ColumnCollection, jsonFile.ToString(),
          jsonFile.Row, m_TimeZoneAdjust, TimeZoneInfo.Local.Id
#if SupportPGP
          , publicKey, jsonFile.KeepUnencrypted
#endif
          ),
        IXmlFile xmlFile => new XmlFileWriter(xmlFile.ID, xmlFile.FullPath, xmlFile.IdentifierInContainer,
          xmlFile.Footer, xmlFile.Header, xmlFile.CodePageId, xmlFile.ByteOrderMark, xmlFile.ColumnCollection,
          xmlFile.ToString(), xmlFile.Row, m_TimeZoneAdjust, TimeZoneInfo.Local.Id
#if SupportPGP
          , publicKey, xmlFile.KeepUnencrypted
#endif
          ),
        _ => null
      };

      if (writer is null)
        throw new FileWriterException($"Writer for {fileSetting} not found");

      writer.WriteFinished += (sender, args) =>
      {
        fileSetting.ProcessTimeUtc = DateTime.UtcNow;
        if (fileSetting is IFileSettingPhysicalFile { SetLatestSourceTimeForWrite: true } physFile)
          new FileSystemUtils.FileInfo(physFile.FullPath).LastWriteTimeUtc = fileSetting.LatestSourceTimeUtc;
      };
      return writer;
    }
  }
}