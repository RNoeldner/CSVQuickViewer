/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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


namespace CsvTools;

/// <inheritdoc />
public class ViewerFileReaderWriterFactory : ClassLibraryCsvFileReaderWriterFactory
{
  /// <summary>Initializes a new instance of the <see cref="ViewerFileReaderWriterFactory" /> class.</summary>
  /// <inheritdoc />
  public ViewerFileReaderWriterFactory(TimeZoneChangeDelegate timeZoneAdjust, FillGuessSettings fillGuessSettings) :
    base(timeZoneAdjust, fillGuessSettings)
  {
  }

  /// <inheritdoc />
  public override IFileReader GetFileReader(IFileSetting setting, CancellationToken cancellationToken)
  {
    if (setting is CsvFileDummy csv)
    {
      if (csv.IsJson)
        return new JsonFileReader(fileName: csv.FullPath, csv.ColumnCollection, csv.RecordLimit, csv.Trim,
          csv.TreatTextAsNull, csv.TreatNBSPAsSpace, TimeZoneAdjust, TimeZoneInfo.Local.Id,
          FillGuessSettings.DetectPercentage, FillGuessSettings.RemoveCurrencySymbols);
      if (csv.IsXml)
        return new XmlFileReader(fileName: csv.FullPath, csv.ColumnCollection, csv.RecordLimit, csv.Trim,
          csv.TreatTextAsNull, csv.TreatNBSPAsSpace, TimeZoneAdjust, TimeZoneInfo.Local.Id,
          FillGuessSettings.DetectPercentage, FillGuessSettings.RemoveCurrencySymbols);

      return new CsvFileReader(fileName: csv.FullPath, codePageId: csv.CodePageId, skipRows: csv.SkipRows, skipRowsAfterHeader: csv.SkipRowsAfterHeader, hasFieldHeader: csv.HasFieldHeader,
columnDefinition: csv.ColumnCollection, trimmingOption: csv.TrimmingOption, fieldDelimiterChar: csv.FieldDelimiterChar, fieldQualifierChar: csv.FieldQualifierChar,
        escapeCharacterChar: csv.EscapePrefixChar,
recordLimit: csv.RecordLimit, allowRowCombining: csv.AllowRowCombining, contextSensitiveQualifier: csv.ContextSensitiveQualifier, commentLine: csv.CommentLine, numWarning: csv.NumWarnings,
duplicateQualifierToEscape: csv.DuplicateQualifierToEscape, newLinePlaceholder: csv.NewLinePlaceholder, delimiterPlaceholder: csv.DelimiterPlaceholder,
quotePlaceholder: csv.QualifierPlaceholder, skipDuplicateHeader: csv.SkipDuplicateHeader, treatLinefeedAsSpace: csv.TreatLfAsSpace, treatUnknownCharacterAsSpace: csv.TreatUnknownCharacterAsSpace,
tryToSolveMoreColumns: csv.TryToSolveMoreColumns, warnDelimiterInValue: csv.WarnDelimiterInValue, warnLineFeed: csv.WarnLineFeed, warnNbsp: csv.WarnNBSP, warnQuotes: csv.WarnQuotes,
warnUnknownCharacter: csv.WarnUnknownCharacter, warnEmptyTailingColumns: csv.WarnEmptyTailingColumns, treatNbspAsSpace: csv.TreatNBSPAsSpace, treatTextAsNull: csv.TreatTextAsNull,
skipEmptyLines: csv.SkipEmptyLines, consecutiveEmptyRowsMax: csv.ConsecutiveEmptyRows, identifierInContainer: csv.IdentifierInContainer, timeZoneAdjust: TimeZoneAdjust,
destinationTimeZone: TimeZoneInfo.Local.Id, allowPercentage: FillGuessSettings.DetectPercentage, removeCurrency: FillGuessSettings.RemoveCurrencySymbols);
    }
    else
      return base.GetFileReader(setting, cancellationToken);
  }
}