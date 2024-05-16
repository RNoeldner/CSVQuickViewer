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
using System.IO;
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
    /// Function that will return encryption related information for a file, this is not used in the Csv Viewer
    /// </summary>
    public static Func<string, (string passphrase, string keyFile, string key)> GetKeyAndPassphraseForFile =
      _ => (string.Empty, string.Empty, string.Empty);

    /// <summary>
    /// Function that will return an open stream for SourceAccess
    /// </summary>
    public static Func<SourceAccess, Stream> GetStream = str => new ImprovedStream(str);

    /// <summary>
    /// Function that will return the proper instance for ColumnFormatters
    /// </summary>
    public static Func<ValueFormat, IColumnFormatter> GetColumnFormatter = valueFormat =>
    valueFormat.DataType switch
      {
        DataTypeEnum.TextPart => new TextPartFormatter(valueFormat.Part, valueFormat.PartSplitter, valueFormat.PartToEnd),
        DataTypeEnum.TextToHtml => TextToHtmlFormatter.Instance,
        DataTypeEnum.TextToHtmlFull => TextToHtmlFullFormatter.Instance,
        DataTypeEnum.TextUnescape => TextUnescapeFormatter.Instance,
        DataTypeEnum.TextReplace => new TextReplaceFormatter(valueFormat.RegexSearchPattern, valueFormat.RegexReplacement),
        _ => EmptyFormatter.Instance
      };

    /// <summary>
    /// IFileReaderWriterFactory for GetFileReader and GetFileWriter
    /// </summary>
    public static IFileReaderWriterFactory FileReaderWriterFactory { get; set; } = new ClassLibraryCsvFileReaderWriterFactory(StandardTimeZoneAdjust.ChangeTimeZone, new FillGuessSettings());
  }
}