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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc cref="IFileWriter" />
  /// <summary>
  ///   A Class to write CSV Files
  /// </summary>
  public sealed class CsvFileWriter : BaseFileWriter, IFileWriter
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
    /// <param name="id">Information for  Placeholder of ID</param>
    /// <param name="fullPath">Fully qualified path of teh file to write</param>
    /// <param name="hasFieldHeader">Determine if a header row should be created</param>
    /// <param name="valueFormat">Fallback value format for typed values that do not have a column setup</param>
    /// <param name="codePageId">The Code Page for encoding of characters</param>
    /// <param name="byteOrderMark">If <c>true</c>a Byte Order Mark will be added</param>
    /// <param name="columnDefinition">Individual definitions of columns and formats</param>
    /// <param name="unencrypted">If <c>true</c> teh not pgp encrypted file is kept for reference</param>
    /// <param name="identifierInContainer">In case the file is written into an archive that does support multiple files, name of teh file in the archive.</param>
    /// <param name="footer">Footer to be written after all rows are written</param>
    /// <param name="header">Header to be written before data and/or Header is written</param>
    /// <param name="fileSettingDisplay">Info text for logging and process report</param>
    /// <param name="newLine"><see cref="RecordDelimiterTypeEnum"/> written after each record</param>
    /// <param name="fieldDelimiterChar">Column / Field delimiter, if empty the text will be written as fixed length</param>
    /// <param name="fieldQualifierChar">Qualifier for columns that might contain characters that need quoting</param>
    /// <param name="escapePrefixChar">Escape char to include otherwise protected characters </param>
    /// <param name="newLinePlaceholder">Placeholder for a NewLine being part of a text, instead of the new line this text will be written</param>
    /// <param name="delimiterPlaceholder">Placeholder for a delimiter being part of a text, instead of the <see cref="fieldDelimiterChar"/> this text will be written</param>
    /// <param name="qualifierPlaceholder">Placeholder for a qualifier being part of a text, instead of the <see cref="fieldQualifierChar"/> this text will be written</param>
    /// <param name="qualifyAlways">If set <c>true</c> each text will be quoted, even if not quoting is needed</param>
    /// <param name="qualifyOnlyIfNeeded">If set <c>true</c> each text will be quoted only if this is required, if this is <c>true</c> <see cref="fieldQualifierChar"/> is ignored</param>
    /// <param name="fixedLength">If set <c>true</c> do not use delimiter but make column in all rows having the same character length</param>
    /// <param name="timeZoneAdjust">Delegate for TimeZone Conversions</param>
    /// <param name="sourceTimeZone">Identified for the timezone teh values are currently stored as</param>
    /// <param name="publicKey">Key used for encryption of the written data (not implemented in all Libraries)</param>
    public CsvFileWriter(in string id,
      in string fullPath,
      bool hasFieldHeader,
      in ValueFormat? valueFormat,
      int codePageId,
      bool byteOrderMark,
      in IEnumerable<Column>? columnDefinition,
      in string? identifierInContainer,
      in string? header,
      in string? footer,
      in string fileSettingDisplay,
      RecordDelimiterTypeEnum newLine,
      char fieldDelimiterChar,
      char fieldQualifierChar,
      char escapePrefixChar,
      in string newLinePlaceholder,
      in string delimiterPlaceholder,
      in string qualifierPlaceholder,
      bool qualifyAlways,
      bool qualifyOnlyIfNeeded,
      bool fixedLength,
      in TimeZoneChangeDelegate timeZoneAdjust,
      in string sourceTimeZone
#if SupportPGP
      ,in string publicKey, bool unencrypted
#endif
      )
      : base(
        id,
        fullPath,
        valueFormat,
        identifierInContainer,
        footer,
        header,
        columnDefinition,
        fileSettingDisplay,
        timeZoneAdjust,
        sourceTimeZone
#if SupportPGP
        ,publicKey, unencrypted
#endif
        )
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

      var qualifyList = new List<char>(3);

      // need quoting in case of new line that is not handled by placeholder
      if (!string.IsNullOrEmpty(m_NewLinePlaceholder))
      {
        qualifyList.Add((char) 0x0a);
        qualifyList.Add((char) 0x0d);
      }
      // need quoting in case of a not escaped delimiter
      if (!m_HasEscapePrefix)
        qualifyList.Add(m_FieldDelimiter);

      m_QualifyCharArray = qualifyList.ToArray();
    }

    /// <inheritdoc cref="IFileWriter"/>
    public override async Task WriteReaderAsync(IFileReader reader, Stream output, CancellationToken cancellationToken)
    {
      var columns = GetColumnInformation(ValueFormatGeneral, ColumnDefinition, reader);

      if (columns.Count == 0)
        throw new FileWriterException("No columns defined to be written.");

      HandleWriteStart();

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var writer =
        new StreamWriter(output, EncodingHelper.GetEncoding(m_CodePageId, m_ByteOrderMark), 8192, true);

      var sb = new StringBuilder();
      if (!string.IsNullOrEmpty(Header))
      {
        sb.Append(Header);
        if (!Header.EndsWith(m_NewLine, StringComparison.Ordinal))
          sb.Append(m_NewLine);
      }

      var lastCol = columns.Last();

      if (m_ColumnHeader)
      {
        foreach (var columnInfo in columns)
        {
          sb.Append(HandleText(columnInfo.Name, columnInfo.FieldLength, null));
          if (!m_IsFixedLength && columnInfo != lastCol)
            sb.Append(m_FieldDelimiter);
        }
        sb.Append(m_NewLine);
      }

      while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)
             && !cancellationToken.IsCancellationRequested)
      {
        if (sb.Length > 32768)
        {
          ReportProgress?.Report(new ProgressInfo("Writing", reader.RecordNumber));
          await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
          sb.Length = 0;
        }

        var emptyColumns = 0;

        var row = new StringBuilder();
        foreach (var columnInfo in columns)
        {
          // Number of columns might be higher than number of reader columns
          var col = reader.GetValue(columnInfo.ColumnOrdinal);
          if (col == DBNull.Value || col is string text && string.IsNullOrEmpty(text))
            emptyColumns++;
          else
          {
            row.Append(HandleText(TextEncodeField(col, columnInfo, reader),
              columnInfo.FieldLength,
              (msg) => HandleWarning(columnInfo.Name, msg)));
          }
          if (m_FieldDelimiter != char.MinValue && columnInfo != lastCol)
            row.Append(m_FieldDelimiter);
        }

        if (emptyColumns == columns.Count()) break;
        NextRecord();
        sb.Append(row);
        sb.Append(m_NewLine);
      }

      var footer = Footer();
      if (!string.IsNullOrEmpty(footer))
      {
        sb.Append(footer);
        if (!footer.EndsWith(m_NewLine, StringComparison.Ordinal))
          sb.Append(m_NewLine);
      }

      // remove the very last newline
      if (sb.Length > m_NewLine.Length)
      {
        sb.Length -= m_NewLine.Length;
        // and store the possibly remaining data
        await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
      }
#if NET5_0_OR_GREATER
      await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
#else
      await writer.FlushAsync().ConfigureAwait(false);
#endif
    }

    /// <summary>
    /// Handled escaping, placeholders and qualifying 
    /// </summary>
    /// <param name="text">The text to be written</param>    
    /// <param name="fieldLength">The length for fixedLength</param>    
    /// <param name="handleWarning">Action to perform if text is cut off for fixed length</param>
    /// <returns></returns>
    /// <exception cref="FileWriterException"></exception>
    public string HandleText(string text, int fieldLength, Action<string>? handleWarning = null)
    {
      if (m_IsFixedLength && fieldLength == 0)
        throw new FileWriterException("For fix length output the length of the columns needs to be specified.");

      // New line of any kind will be replaced with the placeholder if set
      if (m_NewLinePlaceholder.Length > 0)
        text = StringUtils.HandleCrlfCombinations(text, m_NewLinePlaceholder);

      // Delimiter  e.G. ; replaced with the placeholder or escaped 
      if (m_DelimiterPlaceholder.Length > 0 && m_FieldDelimiter != char.MinValue)
        text = text.Replace(m_FieldDelimiter.ToString(), m_DelimiterPlaceholder);
      else if (m_HasEscapePrefix)
        text = text.Replace(m_FieldDelimiter.ToString(), m_FieldDelimiterEscaped);

      // Qualifier e.G. " replaced with the placeholder if set
      if (m_QualifierPlaceholder.Length > 0 && m_FieldQualifier != char.MinValue)
        text = text.Replace(m_FieldQualifier.ToString(), m_QualifierPlaceholder);

      if (m_IsFixedLength)
      {
        if (text.Length <= fieldLength || fieldLength <= 0)
          return text.PadRight(fieldLength, ' ');
        handleWarning?.Invoke($"Text with length of {text.Length} has been cut off after {fieldLength} character");
        return text.Substring(0, fieldLength);
      }

      /* Old method:
      var qualifyThis = m_QualifyAlways;
      if (!qualifyThis)
      {
        if (m_QualifyOnlyIfNeeded)
          // Qualify the text if the delimiter or Linefeed is present, or if the text starts with
          // the Qualifier or a whitespace that is lost otherwise
          qualifyThis = displayAs.IndexOfAny(m_QualifyCharArray) > -1 || displayAs[0].Equals(m_FieldQualifier) ||
                        displayAs[0].Equals(' ');
        else
          // quality any text or date etc that containing a Qualify Char
          qualifyThis = columnInfo.ValueFormat.DataType == DataTypeEnum.String ||
                        columnInfo.ValueFormat.DataType == DataTypeEnum.TextToHtml ||
                        displayAs.IndexOfAny(m_QualifyCharArray) > -1;
      }
      */

      // Determine if we need to qualify
      var qualifyThis = (m_FieldQualifier != char.MinValue) && (m_QualifyAlways || text.IndexOfAny(m_QualifyCharArray) > -1 || (m_QualifyOnlyIfNeeded && text.Length>0 && (text[0].Equals(m_FieldQualifier) || text[0].Equals(' '))));

      return qualifyThis
        ? $"{m_FieldQualifier}{text.Replace(m_FieldQualifier.ToString(), m_FieldQualifierEscaped)}{m_FieldQualifier}"
        : text;
    }
  }
}