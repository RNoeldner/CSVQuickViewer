﻿/*
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
    /// <summary>
    /// Routine that will do time zone conversions
    /// </summary>
    protected readonly TimeZoneChangeDelegate TimeZoneAdjust;

    /// <summary>
    /// Setting determining which value types are determined
    /// </summary>
    protected readonly FillGuessSettings FillGuessSettings;

    /// <summary>
    /// While reading converting to this timezone (if a timezone field is defined)
    /// While writing the timezone that is assumed for columns with time
    /// </summary>
    protected string LocalTimeZone = TimeZoneInfo.Local.Id;

    /// <summary>Initializes a new instance of the <see cref="ClassLibraryCsvFileReaderWriterFactory" /> class.</summary>
    /// <param name="timeZoneAdjust">The routine to do time zone adjustments</param>
    /// <param name="fillGuessSettings">The guess settings</param>
    public ClassLibraryCsvFileReaderWriterFactory(TimeZoneChangeDelegate timeZoneAdjust, FillGuessSettings fillGuessSettings)
    {
      TimeZoneAdjust = timeZoneAdjust;
      FillGuessSettings = fillGuessSettings;
    }

    /// <inheritdoc />
    public virtual IFileReader GetFileReader(IFileSetting setting, CancellationToken cancellationToken)
    {
      return setting switch
      {
        IJsonFile json => new JsonFileReader(json.FullPath, json.ColumnCollection, json.RecordLimit, json.Trim, json.TreatTextAsNull, json.TreatNBSPAsSpace, TimeZoneAdjust,
          LocalTimeZone, FillGuessSettings.DetectPercentage, FillGuessSettings.RemoveCurrencySymbols),

        IXmlFile xml => new XmlFileReader(xml.FullPath, xml.ColumnCollection, xml.RecordLimit, xml.Trim, xml.TreatTextAsNull, xml.TreatNBSPAsSpace, TimeZoneAdjust,
          LocalTimeZone, FillGuessSettings.DetectPercentage, FillGuessSettings.RemoveCurrencySymbols),

        ICsvFile csv => new CsvFileReader(csv.FullPath, csv.CodePageId, csv.SkipRows, csv.HasFieldHeader,
          csv.ColumnCollection, csv.TrimmingOption, csv.FieldDelimiterChar, csv.FieldQualifierChar, csv.EscapePrefixChar,
          csv.RecordLimit, csv.AllowRowCombining, csv.ContextSensitiveQualifier, csv.CommentLine, csv.NumWarnings,
          csv.DuplicateQualifierToEscape, csv.NewLinePlaceholder, csv.DelimiterPlaceholder,
          csv.QualifierPlaceholder, csv.SkipDuplicateHeader, csv.TreatLfAsSpace, csv.TreatUnknownCharacterAsSpace,
          csv.TryToSolveMoreColumns, csv.WarnDelimiterInValue, csv.WarnLineFeed, csv.WarnNBSP, csv.WarnQuotes,
          csv.WarnUnknownCharacter, csv.WarnEmptyTailingColumns, csv.TreatNBSPAsSpace, csv.TreatTextAsNull,
          csv.SkipEmptyLines, csv.ConsecutiveEmptyRows, csv.IdentifierInContainer, TimeZoneAdjust,
          LocalTimeZone, FillGuessSettings.DetectPercentage, FillGuessSettings.RemoveCurrencySymbols),

        _ => throw new FileReaderException($"Reader for {setting} not found"),

      };
    }

    /// <inheritdoc />
    public virtual IFileWriter GetFileWriter(IFileSetting fileSetting, CancellationToken cancellationToken)
    {
      var publicKey = string.Empty;

      return fileSetting switch
      {
        ICsvFile csv => new CsvFileWriter(csv.FullPath, csv.HasFieldHeader, csv.ValueFormatWrite, csv.CodePageId,
          csv.ByteOrderMark, csv.ColumnCollection, csv.IdentifierInContainer, csv.Header,
          csv.Footer, csv.GetDisplay(), csv.NewLine, csv.FieldDelimiterChar, csv.FieldQualifierChar, csv.EscapePrefixChar,
          csv.NewLinePlaceholder, csv.DelimiterPlaceholder, csv.QualifierPlaceholder, csv.QualifyAlways,
          csv.QualifyOnlyIfNeeded, csv.WriteFixedLength, TimeZoneAdjust, LocalTimeZone, publicKey, csv.KeepUnencrypted
          ),

        IJsonFile jsonFile => new JsonFileWriter(jsonFile.FullPath, jsonFile.IdentifierInContainer,
          jsonFile.Footer, jsonFile.Header, jsonFile.EmptyAsNull, jsonFile.CodePageId,
          jsonFile.ByteOrderMark, jsonFile.ColumnCollection, jsonFile.GetDisplay(), jsonFile.Row,
          TimeZoneAdjust, LocalTimeZone, publicKey, jsonFile.KeepUnencrypted
          ),

        IXmlFile xmlFile => new XmlFileWriter(xmlFile.FullPath, xmlFile.IdentifierInContainer, xmlFile.Footer,
          xmlFile.Header, xmlFile.CodePageId, xmlFile.ByteOrderMark, xmlFile.ColumnCollection, xmlFile.GetDisplay(),
          xmlFile.Row, TimeZoneAdjust, LocalTimeZone, publicKey, xmlFile.KeepUnencrypted
          ),
        _ => throw new FileWriterException($"Writer for {fileSetting} not found"),
      };
    }
  }
}