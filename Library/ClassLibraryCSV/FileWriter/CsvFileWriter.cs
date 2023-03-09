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
using System.Data;
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
    private readonly string m_DelimiterPlaceholder;

    private readonly char m_FieldDelimiter;
    private readonly char m_FieldQualifier;
    private readonly char m_EscapePrefix;

    private readonly string m_FieldDelimiterEscaped;
    private readonly string m_FieldQualifierEscaped;

    private readonly string m_NewLine;
    private readonly string m_NewLinePlaceholder;
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
    /// <param name="pgpKeyId">Passed on to SourceAccess allowing PGP encryption of teh written file (not implemented in all Libraries)</param>
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
    /// <param name="timeZoneAdjust">Delegate for TimeZone Conversions</param>
    /// <param name="sourceTimeZone">Identified for the timezone teh values are currently stored as</param>
    public CsvFileWriter(in string id,
      in string fullPath,
      bool hasFieldHeader,
      in ValueFormat? valueFormat,
      int codePageId,
      bool byteOrderMark,
      in IEnumerable<Column>? columnDefinition,
      // TODO: pgpKeyId goes away replaced by Key
      in long pgpKeyId,
      bool unencrypted,
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
      in TimeZoneChangeDelegate timeZoneAdjust,
      string sourceTimeZone)
      : base(
        id,
        fullPath,
        valueFormat,
        pgpKeyId,
        unencrypted,
        identifierInContainer,
        footer,
        header,
        columnDefinition,
        fileSettingDisplay,
        timeZoneAdjust,
        sourceTimeZone)
    {
      m_CodePageId = codePageId;
      m_ColumnHeader = hasFieldHeader;
      m_ByteOrderMark = byteOrderMark;
      m_FieldQualifier =  fieldQualifierChar;
      m_FieldDelimiter = fieldDelimiterChar;
      m_EscapePrefix = escapePrefixChar;
      m_QualifyAlways = qualifyAlways;
      m_QualifyOnlyIfNeeded = qualifyOnlyIfNeeded;
      if (m_QualifyOnlyIfNeeded && m_QualifyAlways)
        m_QualifyAlways = false;
      m_NewLine = newLine.NewLineString();
      Header = Header.HandleCrlfCombinations(m_NewLine).PlaceholderReplace("Delim", m_FieldDelimiter.Text());

      m_NewLinePlaceholder = newLinePlaceholder;
      m_DelimiterPlaceholder = delimiterPlaceholder;
      m_QualifierPlaceholder = qualifierPlaceholder;

      if (m_EscapePrefix != char.MinValue)
      {
        m_QualifyCharArray = new[] { (char) 0x0a, (char) 0x0d };
        m_FieldQualifierEscaped = m_EscapePrefix.ToString() + m_FieldQualifier;
        m_FieldDelimiterEscaped = m_EscapePrefix.ToString() + m_FieldDelimiter;
      }
      else
      {
        m_FieldDelimiterEscaped = m_FieldDelimiter.ToString();
        // but require quoting
        m_QualifyCharArray = new[] { (char) 0x0a, (char) 0x0d, fieldDelimiterChar };

        // the Qualifier is repeated to so it can be recognized as not to be end the quoting
        m_FieldQualifierEscaped = m_FieldQualifier.ToString() + m_FieldQualifier;
      }
    }

    /// <summary>
    ///   Writes the specified file reading from the given reader
    /// </summary>
    /// <param name="reader">A Data Reader with the data</param>
    /// <param name="output">The output.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
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
          sb.Append(TextEncodeField(columnInfo.Name, columnInfo, true, null, QualifyText));
          if (m_FieldDelimiter != char.MinValue && !ReferenceEquals(columnInfo, lastCol))
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
            row.Append(TextEncodeField(col, columnInfo, false, reader, QualifyText));

          if (m_FieldDelimiter != char.MinValue && !ReferenceEquals(columnInfo, lastCol))
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

      await writer.FlushAsync().ConfigureAwait(false);
    }

    private string QualifyText(string displayAs, DataTypeEnum dataType)
    {
      var qualifyThis = m_QualifyAlways;
      if (!qualifyThis)
      {
        if (m_QualifyOnlyIfNeeded)
          // Qualify the text if the delimiter or Linefeed is present, or if the text starts with
          // the Qualifier
          qualifyThis = displayAs.Length > 0 && (displayAs.IndexOfAny(m_QualifyCharArray) > -1
                                                 || displayAs[0].Equals(m_FieldQualifier)
                                                 || displayAs[0].Equals(' '));
        else
          // quality any text or something containing a Qualify Char
          qualifyThis = dataType == DataTypeEnum.String || dataType == DataTypeEnum.TextToHtml
                                                        || displayAs.IndexOfAny(m_QualifyCharArray) > -1;
      }

      if (m_EscapePrefix != char.MinValue)
        displayAs = displayAs.Replace(m_FieldDelimiter.ToString(), m_FieldDelimiterEscaped);

      if (qualifyThis)
        return m_FieldQualifier + displayAs.Replace(m_FieldQualifier.ToString(), m_FieldQualifierEscaped) + m_FieldQualifier;

      return displayAs;
    }

    private string TextEncodeField(object? dataObject, WriterColumn columnInfo, bool isHeader, IDataReader? reader, Func<string, DataTypeEnum, string>? handleQualify)
    {
      if (columnInfo is null)
        throw new ArgumentNullException(nameof(columnInfo));

      if (m_FieldDelimiter == char.MinValue && columnInfo.FieldLength == 0)
        throw new FileWriterException("For fix length output the length of the columns needs to be specified.");

      string displayAs;
      if (isHeader)
      {
        if (dataObject is null)
          throw new ArgumentNullException(nameof(dataObject));
        displayAs = Convert.ToString(dataObject) ?? string.Empty;
      }
      else
      {
        displayAs = TextEncodeField(dataObject, columnInfo, reader);
        if (columnInfo.ValueFormat.DataType == DataTypeEnum.String)
        {
          // a new line of any kind will be replaced with the placeholder if set
          if (m_NewLinePlaceholder.Length > 0)
            displayAs = displayAs.HandleCrlfCombinations(m_NewLinePlaceholder);

          if (m_DelimiterPlaceholder.Length > 0 && m_FieldDelimiter != char.MinValue)
            displayAs = displayAs.Replace(m_FieldDelimiter.ToString(),
              m_DelimiterPlaceholder);

          if (m_QualifierPlaceholder.Length > 0 && m_FieldQualifier != char.MinValue)
            displayAs = displayAs.Replace(m_FieldQualifier.ToString(),
              m_QualifierPlaceholder);
        }
      }

      // Adjust the output in case its is fixed length
      if (m_FieldQualifier == char.MinValue)
      {
        if (displayAs.Length <= columnInfo.FieldLength || columnInfo.FieldLength <= 0)
          return displayAs.PadRight(columnInfo.FieldLength, ' ');
        HandleWarning(columnInfo.Name,
          $"Text with length of {displayAs.Length} has been cut off after {columnInfo.FieldLength} character");
        return displayAs.Substring(0, columnInfo.FieldLength);
      }

      // Qualify text if required
      if (m_FieldQualifier != char.MinValue && handleQualify != null)
        return handleQualify(displayAs, columnInfo.ValueFormat.DataType);

      return displayAs;
    }
  }
}