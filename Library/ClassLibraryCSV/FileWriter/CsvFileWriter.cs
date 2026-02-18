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
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools;

/// <inheritdoc cref="IFileWriter" />
/// <summary>
///   A Class to write CSV Files
/// </summary>
public sealed class CsvFileWriter : BaseFileWriter
{
  private readonly bool m_ByteOrderMark;
  private readonly int m_CodePageId;
  private readonly bool m_ColumnHeader;
  private readonly char m_FieldDelimiter;
  private readonly char m_FieldQualifier;
  private readonly bool m_IsFixedLength;
  private readonly bool m_HasEscapePrefix;
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
  ///   Constructor for a delimited Text / fixed length text writer
  /// </summary>
  /// <param name="fullPath">Fully qualified path of the file to write</param>
  /// <param name="hasFieldHeader">Determine if a header row should be created</param>
  /// <param name="valueFormat">Fallback value format for typed values that do not have a column setup</param>
  /// <param name="codePageId">The Code Page for encoding of characters</param>
  /// <param name="byteOrderMark">If <c>true</c>a Byte Order Mark will be added</param>
  /// <param name="columnDefinition">Individual definitions of columns and formats</param>
  /// <param name="unencrypted">If <c>true</c> the not pgp encrypted file is kept for reference</param>
  /// <param name="identifierInContainer">In case the file is written into an archive that does support multiple files, name of the file in the archive.</param>
  /// <param name="footer">Footer to be written after all rows are written</param>
  /// <param name="header">Header to be written before data and/or Header is written</param>
  /// <param name="fileSettingDisplay">Info text for logging and process report</param>
  /// <param name="newLine"><see cref="RecordDelimiterTypeEnum"/> written after each record</param>
  /// <param name="fieldDelimiterChar">The delimiter to separate columns, if empty the text will be written as fixed length</param>
  /// <param name="fieldQualifierChar">Qualifier for columns that might contain characters that need quoting</param>
  /// <param name="escapePrefixChar">Escape char to include otherwise protected characters </param>
  /// <param name="newLinePlaceholder">Placeholder for a NewLine being part of a text, instead of the new line this text will be written</param>
  /// <param name="delimiterPlaceholder">Placeholder for a delimiter being part of a text, instead of the delimiter this text will be written</param>
  /// <param name="qualifierPlaceholder">Placeholder for a qualifier being part of a text, instead of the qualifier this text will be written</param>
  /// <param name="qualifyAlways">If set <c>true</c> each text will be quoted, even if not quoting is needed</param>
  /// <param name="qualifyOnlyIfNeeded">If set <c>true</c> each text will be quoted only if this is required, if this is <c>true</c> fieldQualifierChar is ignored</param>
  /// <param name="fixedLength">If set <c>true</c> do not use delimiter but make column in all rows having the same character length</param>
  /// <param name="sourceTimeZone">Identified for the timezone the values are currently stored as</param>
  /// <param name="publicKey">Key used for encryption of the written data (not implemented in all Libraries)</param>
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

    m_HasEscapePrefix = escapePrefixChar != char.MinValue;
    if (m_HasEscapePrefix)
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
      throw new ArgumentException($"{nameof(delimiterPlaceholder)} invalid characters in '{m_DelimiterPlaceholder}'");

    if (m_QualifierPlaceholder.IndexOfAny(illegal)!=-1)
      throw new ArgumentException($"{nameof(qualifierPlaceholder)} invalid characters in  '{m_QualifierPlaceholder}'");

    if (m_NewLinePlaceholder.IndexOfAny(illegal)!=-1)
      throw new ArgumentException($"{nameof(newLinePlaceholder)} invalid characters in '{m_NewLinePlaceholder}'");

    var qualifyList = new List<char>(4);

    // need quoting in case of new line that is not handled by placeholder
    if (string.IsNullOrEmpty(m_NewLinePlaceholder))
    {
      qualifyList.Add((char) 0x0a);
      qualifyList.Add((char) 0x0d);
    }

    // need quoting in case of a not escaped delimiter
    if (!m_HasEscapePrefix)
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
          await writer.WriteAsync(
              HandleText(textValue, columnInfo.FieldLength, msg => HandleWarning(columnInfo.Name, msg))
          ).ConfigureAwait(false);
        }

        if (m_FieldDelimiter != char.MinValue && i != columns.Count() - 1)
          await writer.WriteAsync(m_FieldDelimiter).ConfigureAwait(false);
      }

      if (emptyColumns == columns.Count())
        break;

      NextRecord();
      await writer.WriteAsync(m_NewLine).ConfigureAwait(false);

      // Progress reporting
      intervalAction?.Invoke(progress!, "Writing", reader.RecordNumber);
    }
    progress.Report(new ProgressInfo("Writing", reader.RecordNumber));

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