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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvTools;

/// <summary>
/// A specialized implementation of <see cref="BaseFileWriter"/> for generating CSV files.
/// Supports configurable delimiters, qualifiers, and fixed-length field constraints.
/// </summary>
public sealed class CsvFileWriter : BaseFileWriter
{
  private readonly bool m_ByteOrderMark;
  private readonly int m_CodePageId;
  private readonly bool m_ColumnHeader;
  private readonly char m_FieldDelimiter;
  private readonly char m_FieldQualifier;
  private readonly bool m_IsFixedLength;
  private readonly string m_FieldDelimiterEscaped;
  private readonly string m_FieldQualifierEscaped;
  private readonly string m_NewLine;
  private readonly string m_NewLinePlaceholder;
  private readonly string m_DelimiterPlaceholder;
  private readonly string m_QualifierPlaceholder;
  private readonly bool m_QualifyAlways;
  private readonly char[] m_QualifyCharArray;
  private readonly bool m_QualifyOnlyIfNeeded;

  /// <summary>
  /// Initializes a new instance of the <see cref="CsvFileWriter"/> class for writing delimited text or fixed-length files.
  /// </summary>
  /// <param name="fullPath">The fully qualified path of the file to be written.</param>
  /// <param name="hasFieldHeader">Indicates whether a header row containing column names should be created.</param>
  /// <param name="valueFormat">The fallback value format for typed values that do not have a specific column configuration.</param>
  /// <param name="codePageId">The Code Page ID used for character encoding (default is 65001 for UTF-8).</param>
  /// <param name="byteOrderMark">If <c>true</c>, a Byte Order Mark (BOM) will be added to the beginning of the file.</param>
  /// <param name="columnDefinition">A collection of individual column definitions and formatting rules.</param>
  /// <param name="identifierInContainer">The name of the file within the archive, if writing to a container that supports multiple files.</param>
  /// <param name="header">The header text to be written before the data records or column headers.</param>
  /// <param name="footer">The footer text to be written after all records have been processed.</param>
  /// <param name="fileSettingDisplay">Descriptive text used for logging and progress reporting.</param>
  /// <param name="newLine">The <see cref="RecordDelimiterTypeEnum"/> to be used as a line terminator after each record.</param>
  /// <param name="fieldDelimiterChar">The character used to separate columns. If empty, the file is treated as fixed-length.</param>
  /// <param name="fieldQualifierChar">The character used to enclose fields that contain delimiters or line breaks.</param>
  /// <param name="escapePrefixChar">The character used to escape qualifiers or delimiters within a text field.</param>
  /// <param name="newLinePlaceholder">A string that replaces any actual new line characters found within the data text.</param>
  /// <param name="delimiterPlaceholder">A string that replaces any actual delimiter characters found within the data text.</param>
  /// <param name="qualifierPlaceholder">A string that replaces any actual qualifier characters found within the data text.</param>
  /// <param name="qualifyAlways">If <c>true</c>, encloses every field in qualifiers regardless of content.</param>
  /// <param name="qualifyOnlyIfNeeded">If <c>true</c>, encloses fields in qualifiers only when they contain special characters.</param>
  /// <param name="fixedLength">If <c>true</c>, writes columns with a fixed character width instead of using a delimiter.</param>
  /// <param name="sourceTimeZone">The time zone ID representing the current state of the source data.</param>
  /// <param name="publicKey">The public key used for encrypting the output file (if supported by the writer).</param>
  /// <param name="unencrypted">If <c>true</c>, the unencrypted version of the file is preserved for reference when encryption is used.</param>
  public CsvFileWriter(
    string fullPath,
    bool hasFieldHeader = true,
    in ValueFormat? valueFormat = null,
    int codePageId = 65001,
    bool byteOrderMark = true,
    in IEnumerable<Column>? columnDefinition = null,
    string? identifierInContainer = "",
    string? header = "",
    string? footer = "",
    string fileSettingDisplay = "",
    in RecordDelimiterTypeEnum newLine = RecordDelimiterTypeEnum.Crlf,
    char fieldDelimiterChar = ',',
    char fieldQualifierChar = '"',
    char escapePrefixChar = '\0',
    string newLinePlaceholder = "",
    string delimiterPlaceholder = "",
    string qualifierPlaceholder = "",
    bool qualifyAlways = false,
    bool qualifyOnlyIfNeeded = true,
    bool fixedLength = false,
    string sourceTimeZone = "",
    string publicKey = "",
    bool unencrypted = false
  )
    : base(fullPath, valueFormat, identifierInContainer,
      footer, header, columnDefinition,
      fileSettingDisplay, sourceTimeZone, publicKey, unencrypted)
  {
    m_CodePageId = codePageId;
    m_ColumnHeader = hasFieldHeader;
    m_ByteOrderMark = byteOrderMark;
    m_FieldQualifier =  fieldQualifierChar;
    m_FieldQualifierEscaped =  new string(m_FieldQualifier, 2);

    m_FieldDelimiter = fieldDelimiterChar;
    m_FieldDelimiterEscaped = m_FieldDelimiter.ToString();

    var hasEscapePrefix = escapePrefixChar != char.MinValue;
    if (hasEscapePrefix)
    {
      m_FieldQualifierEscaped = $"{escapePrefixChar}{m_FieldQualifier}";
      m_FieldDelimiterEscaped = $"{escapePrefixChar}{m_FieldDelimiter}";
    }

    m_QualifyAlways = qualifyAlways && !qualifyOnlyIfNeeded;
    m_QualifyOnlyIfNeeded = qualifyOnlyIfNeeded;

    m_NewLine = newLine.NewLineString();
    Header = Header.HandleCrlfCombinations(m_NewLine).PlaceholderReplace("Delim", m_FieldDelimiter.Text());
    m_IsFixedLength  = fixedLength;

    m_NewLinePlaceholder = newLinePlaceholder.HandleLongText();
    m_DelimiterPlaceholder = delimiterPlaceholder.HandleLongText();
    m_QualifierPlaceholder = qualifierPlaceholder.HandleLongText();

    // check the validity of placeholders
    var illegal = new[] { (char) 0x0a, (char) 0x0d, m_FieldDelimiter, m_FieldQualifier };
    if (m_DelimiterPlaceholder.IndexOfAny(illegal)!=-1)
      throw new ArgumentException($"{nameof(delimiterPlaceholder)} invalid characters in '{m_DelimiterPlaceholder}'", nameof(delimiterPlaceholder));

    if (m_QualifierPlaceholder.IndexOfAny(illegal)!=-1)
      throw new ArgumentException($"{nameof(qualifierPlaceholder)} invalid characters in  '{m_QualifierPlaceholder}'", nameof(qualifierPlaceholder));

    if (m_NewLinePlaceholder.IndexOfAny(illegal)!=-1)
      throw new ArgumentException($"{nameof(newLinePlaceholder)} invalid characters in '{m_NewLinePlaceholder}'", nameof(newLinePlaceholder));

    var qualifyList = new List<char>(4);

    // need quoting in case of new line that is not handled by placeholder
    if (string.IsNullOrEmpty(m_NewLinePlaceholder))
    {
      qualifyList.Add((char) 0x0a);
      qualifyList.Add((char) 0x0d);
    }

    // need quoting in case of a not escaped delimiter
    if (!hasEscapePrefix)
    {
      qualifyList.Add(m_FieldQualifier);
      qualifyList.Add(m_FieldDelimiter);
    }
    m_QualifyCharArray = qualifyList.ToArray();
  }

  /// <inheritdoc cref="IFileWriter"/>
  public override async Task WriteReaderAsync(IFileReader reader, Stream output, IProgressWithCancellation progress)
  {
    var columns = GetColumnInformation(ValueFormatGeneral, ColumnDefinition, reader);
    if (columns.Count == 0)
      throw new FileWriterException("No columns defined to be written.");

    HandleWriteStart();
    using var writer = new StreamWriter(output, EncodingHelper.GetEncoding(m_CodePageId, m_ByteOrderMark), 64 * 1024, true);

    // --- Write header ---
    if (!string.IsNullOrEmpty(Header))
    {
      await writer.WriteAsync(Header).ConfigureAwait(false);
      if (!Header.EndsWith(m_NewLine, StringComparison.Ordinal))
        await writer.WriteAsync(m_NewLine).ConfigureAwait(false);
    }

    var lastCol = columns.Last();
    // --- Write column header ---
    if (m_ColumnHeader)
    {
      foreach (var columnInfo in columns)
      {
        await writer.WriteAsync(HandleText(columnInfo.Name, columnInfo.FieldLength)).ConfigureAwait(false);
        if (!m_IsFixedLength && columnInfo != lastCol)
          await writer.WriteAsync(m_FieldDelimiter).ConfigureAwait(false);
      }
      await writer.WriteAsync(m_NewLine).ConfigureAwait(false);
    }

    var intervalAction = IntervalAction.ForProgress(progress);
    // --- Write rows ---
    while (await reader.ReadAsync(progress.CancellationToken).ConfigureAwait(false)
       && !progress.CancellationToken.IsCancellationRequested)
    {
      int emptyColumns = 0;

      for (int i = 0; i < columns.Count(); i++)
      {
        var columnInfo = columns.ElementAt(i);
        var col = reader.GetValue(columnInfo.ColumnOrdinal);

        if (col == DBNull.Value || col is string text && string.IsNullOrEmpty(text))
        {
          emptyColumns++;
        }
        else
        {
          var textValue = TextEncodeField(col, columnInfo, reader);
          await writer.WriteAsync(HandleText(textValue, columnInfo.FieldLength, msg => HandleWarning(columnInfo.Name, msg))).ConfigureAwait(false);
        }

        if (m_FieldDelimiter != char.MinValue && i != columns.Count() - 1)
          await writer.WriteAsync(m_FieldDelimiter).ConfigureAwait(false);
      }

      if (emptyColumns == columns.Count())
        break;

      Records++;
      await writer.WriteAsync(m_NewLine).ConfigureAwait(false);

      // Progress reporting
      intervalAction?.Invoke(progress, $"Record {reader.RecordNumber:N0}", reader.Percent);
    }
    progress.Report(new ProgressInfo($"Record {reader.RecordNumber:N0}", reader.Percent));

    // --- Write footer ---
    var footer = Footer();
    if (!string.IsNullOrEmpty(footer))
    {
      await writer.WriteAsync(footer).ConfigureAwait(false);
      if (!footer.EndsWith(m_NewLine, StringComparison.Ordinal))
        await writer.WriteAsync(m_NewLine).ConfigureAwait(false);
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await writer.FlushAsync(progress.CancellationToken).ConfigureAwait(false);
#else
    await writer.FlushAsync().ConfigureAwait(false);
#endif
    HandleWriteEnd();
  }

  /// <summary>
  /// Escapes, replaces placeholders, and applies fixed-length padding or field qualification to a text value
  /// for writing to a CSV or fixed-length file.
  /// </summary>
  /// <param name="text">The input text to format.</param>
  /// <param name="fieldLength">
  /// The target length for fixed-length fields. Must be greater than 0 if <see cref="m_IsFixedLength"/> is true.
  /// </param>
  /// <param name="handleWarning">
  /// Optional callback invoked if the text is truncated due to fixed-length constraints.
  /// </param>
  /// <returns>
  /// The processed text, ready to write, with applied padding, escaping, placeholders, and/or field qualification.
  /// </returns>
  /// <exception cref="FileWriterException">
  /// Thrown if <paramref name="fieldLength"/> is zero while fixed-length output is required.
  /// </exception>
  private string HandleText(string text, int fieldLength, Action<string>? handleWarning = null)
  {
    if (m_IsFixedLength && fieldLength <= 0)
      throw new FileWriterException("For fixed-length output the length of the columns needs to be specified.");

    // --- Replace CR/LF and special chars using StringBuilder ---
    var sb = new StringBuilder(text.Length + 16);

    foreach (var c in text)
    {
      if ((c == '\r' || c == '\n') && !string.IsNullOrEmpty(m_NewLinePlaceholder))
      {
        sb.Append(m_NewLinePlaceholder);
        continue;
      }

      if (c == m_FieldDelimiter)
      {
        sb.Append(!string.IsNullOrEmpty(m_DelimiterPlaceholder) ? m_DelimiterPlaceholder : m_FieldDelimiterEscaped);
        continue;
      }

      if (c == m_FieldQualifier && !string.IsNullOrEmpty(m_QualifierPlaceholder))
      {
        sb.Append(m_QualifierPlaceholder);
        continue;
      }

      sb.Append(c);
    }

    var processed = sb.ToString();

    // --- Fixed-length padding / truncation ---
    if (m_IsFixedLength)
    {
      if (processed.Length > fieldLength)
      {
        handleWarning?.Invoke($"Text with length {processed.Length} has been cut off after {fieldLength} characters.");
        processed = processed.Substring(0, fieldLength);
      }
      if (processed.Length < fieldLength)
        processed = processed.PadRight(fieldLength, ' ');
      return processed;
    }

    // --- Qualification ---
    bool qualifyThis = (m_FieldQualifier != char.MinValue) &&
                       (m_QualifyAlways ||
                        processed.IndexOfAny(m_QualifyCharArray) >= 0 ||
                        (m_QualifyOnlyIfNeeded && processed.Length > 0 &&
                         (processed[0] == m_FieldQualifier || processed[0] == ' ')));

    if (!qualifyThis) return processed;

    return m_FieldQualifier + processed.Replace(m_FieldQualifier.ToString(), m_FieldQualifierEscaped) + m_FieldQualifier;
  }
}