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
  public class ViewerFileReaderWriterFactory : ClassLibraryCsvFileReaderWriterFactory
  {
    /// <summary>Initializes a new instance of the <see cref="ViewerFileReaderWriterFactory" /> class.</summary>
    /// <inheritdoc />
    public ViewerFileReaderWriterFactory(TimeZoneChangeDelegate timeZoneAdjust, FillGuessSettings fillGuessSettings) : base(timeZoneAdjust, fillGuessSettings)
    {
    }

    /// <inheritdoc />
    public override IFileReader GetFileReader(IFileSetting fileSetting, CancellationToken cancellationToken)
    {
      if (fileSetting is CsvFileDummy csv)
      {
        if (csv.IsJson)
          return new JsonFileReader(fileName: csv.FullPath, csv.ColumnCollection, csv.RecordLimit, csv.Trim, csv.TreatTextAsNull, csv.TreatNBSPAsSpace, TimeZoneAdjust, TimeZoneInfo.Local.Id, FillGuessSettings.DetectPercentage, FillGuessSettings.RemoveCurrencySymbols);
        if (csv.IsXml)
          return new XmlFileReader(fileName:csv.FullPath, csv.ColumnCollection, csv.RecordLimit, csv.Trim, csv.TreatTextAsNull, csv.TreatNBSPAsSpace, TimeZoneAdjust, TimeZoneInfo.Local.Id, FillGuessSettings.DetectPercentage, FillGuessSettings.RemoveCurrencySymbols);

        return new CsvFileReader(fileName: csv.FullPath, csv.CodePageId, csv.SkipRows, csv.HasFieldHeader,
          csv.ColumnCollection, csv.TrimmingOption, csv.FieldDelimiterChar, csv.FieldQualifierChar, csv.EscapePrefixChar,
          csv.RecordLimit, csv.AllowRowCombining, csv.ContextSensitiveQualifier, csv.CommentLine, csv.NumWarnings,
          csv.DuplicateQualifierToEscape, csv.NewLinePlaceholder, csv.DelimiterPlaceholder,
          csv.QualifierPlaceholder, csv.SkipDuplicateHeader, csv.TreatLfAsSpace, csv.TreatUnknownCharacterAsSpace,
          csv.TryToSolveMoreColumns, csv.WarnDelimiterInValue, csv.WarnLineFeed, csv.WarnNBSP, csv.WarnQuotes,
          csv.WarnUnknownCharacter, csv.WarnEmptyTailingColumns, csv.TreatNBSPAsSpace, csv.TreatTextAsNull,
          csv.SkipEmptyLines, csv.ConsecutiveEmptyRows, csv.IdentifierInContainer, TimeZoneAdjust,
          TimeZoneInfo.Local.Id, FillGuessSettings.DetectPercentage, FillGuessSettings.RemoveCurrencySymbols);
      }
      else
        return base.GetFileReader(fileSetting, cancellationToken);
    }
  }
}