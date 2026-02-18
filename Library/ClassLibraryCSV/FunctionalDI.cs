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
using System;
using System.IO;
namespace CsvTools;

#pragma warning disable MA0048 // File name must match type name
/// <summary>
/// Delegate for retrieving encryption metadata (passphrase and key files) for a specific file path.
/// </summary>
/// <returns>A tuple containing the passphrase, key file path, and the raw key string.</returns>
public delegate (string passphrase, string keyFile, string key) EncryptionInfoDelegate(string fileName);

/// <summary>
/// Delegate for providing an IO <see cref="Stream"/> based on the specified source access settings.
/// </summary>
/// <param name="source">The source access configuration.</param>
/// <returns>A readable <see cref="Stream"/>.</returns>
public delegate Stream StreamProviderDelegate(SourceAccess source);

/// <summary>
/// Delegate for resolving the appropriate column formatter for a given data type.
/// </summary>
/// <param name="valueFormat">The formatting configuration (passed by read-only reference).</param>
/// <returns>An implementation of <see cref="IColumnFormatter"/>.</returns>
public delegate IColumnFormatter ColumnFormatterDelegate(in ValueFormat valueFormat);

/// <summary>
/// Delegate for a routine that handles time zone conversion between two identifiers.
/// </summary>
/// <param name="input">The <see cref="DateTime"/> value to be converted.</param>
/// <param name="sourceTimeZone">The source time zone identifier.</param>
/// <param name="destinationTimeZone">The target time zone identifier.</param>
/// <param name="handleWarning">
/// Optional action invoked to report warnings, such as unknown or invalid time zones.
/// </param>
/// <returns>The converted <see cref="DateTime"/> value in the target time zone.</returns>
public delegate DateTime TimeZoneChangeDelegate(in DateTime input, string sourceTimeZone, string destinationTimeZone, Action<string>? handleWarning);
#pragma warning restore MA0048 // File name must match type name

/// <summary>
///   This class implements a lightweight Dependency Injection (DI) pattern without a framework.
///   It uses static delegates to allow library consumers to override default behavior
///   with implementations unknown to this assembly.
/// </summary>
public static class FunctionalDI
{
  /// <summary>
  /// Delegate that returns encryption related information for a file.
  /// Note: This is currently not utilized within the CSV Viewer itself.
  /// </summary>
  public static EncryptionInfoDelegate GetKeyAndPassphraseForFile { get; set; } =
      _ => (string.Empty, string.Empty, string.Empty);

  /// <summary>
  /// Delegate that opens and returns a <see cref="Stream"/> for the given <see cref="SourceAccess"/>.
  /// </summary>
  public static StreamProviderDelegate GetStream { get; set; } = str => new ImprovedStream(str);

  /// <summary>
  /// Delegate that resolves the concrete <see cref="IColumnFormatter"/> based on <see cref="DataTypeEnum"/>.
  /// </summary>
  /// <remarks>Default Implementation will use CSV File know Formatters</remarks>
  public static ColumnFormatterDelegate GetColumnFormatter { get; set; } = (in ValueFormat valueFormat) =>
    valueFormat.DataType switch
    {
      DataTypeEnum.TextPart => new TextPartFormatter(valueFormat.Part, valueFormat.PartSplitter, valueFormat.PartToEnd),
      DataTypeEnum.TextToHtml => HtmlToTextFormatter.Instance,
      DataTypeEnum.TextToHtmlFull => TextToHtmlFullFormatter.Instance,
      DataTypeEnum.TextUnescape => TextUnescapeFormatter.Instance,
      DataTypeEnum.TextReplace => new TextReplaceFormatter(valueFormat.RegexSearchPattern, valueFormat.RegexReplacement),
      DataTypeEnum.HtmlToText => HtmlToTextFormatter.Instance,
      _ => EmptyFormatter.Instance
    };

  /// <summary>
  /// Delegate for time zone adjustment logic. 
  /// Defaults to <see cref="StandardTimeZoneAdjust.ChangeTimeZone"/>.
  /// </summary>
  public static TimeZoneChangeDelegate GetTimeZoneAdjust { get; set; } = StandardTimeZoneAdjust.ChangeTimeZone;

  /// <summary>
  /// IFileReaderWriterFactory for GetFileReader and GetFileWriter
  /// </summary>
  public static IFileReaderWriterFactory FileReaderWriterFactory { get; set; } = new ClassLibraryCsvFileReaderWriterFactory(new FillGuessSettings());
}