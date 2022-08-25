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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc cref="CsvTools.IFileWriter" />
  /// <summary>
  ///   A Class to write CSV Files
  /// </summary>
  public class CsvFileWriter : BaseFileWriter, IFileWriter
  {
    private readonly bool m_ByteOrderMark;
    private readonly int m_CodePageId;
    private readonly bool m_ColumnHeader;
    private readonly string m_DelimiterPlaceholder;
    private readonly string m_FieldDelimiter;
    private readonly char m_FieldDelimiterChar;
    private readonly string m_FieldDelimiterEscaped;
    private readonly string m_FieldQualifier;
    private readonly char m_FieldQualifierChar;
    private readonly string m_FieldQualifierEscaped;
    private readonly bool m_IsFixedLength;
    private readonly string m_NewLine;
    private readonly string m_NewLinePlaceholder;
    private readonly string m_QualifierPlaceholder;
    private readonly bool m_QualifyAlways;
    private readonly char[] m_QualifyCharArray;
    private readonly bool m_QualifyOnlyIfNeeded;

    public CsvFileWriter(in string id,
      in string fullPath,
      bool hasFieldHeader,
      in IValueFormat? valueFormat,
      int codePageId,
      bool byteOrderMark,
      in IEnumerable<IColumn>? columnDefinition,
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
      m_FieldQualifier = fieldQualifierChar.ToStringHandle0();
      m_FieldDelimiter = fieldDelimiterChar.ToStringHandle0();
      m_QualifyAlways = qualifyAlways;
      m_QualifyOnlyIfNeeded = qualifyOnlyIfNeeded;
      m_NewLine = newLine.NewLineString();
      Header = Header.HandleCrlfCombinations(m_NewLine).PlaceholderReplace("Delim", m_FieldDelimiter);
      m_FieldDelimiterChar = fieldDelimiterChar;
      m_IsFixedLength = m_FieldDelimiterChar == '\0';
      m_NewLinePlaceholder = newLinePlaceholder;
      m_DelimiterPlaceholder = delimiterPlaceholder;
      m_QualifierPlaceholder = qualifierPlaceholder;
      m_FieldQualifierChar = fieldQualifierChar;

      if (escapePrefixChar != '\0')
      {
        m_QualifyCharArray = new[] { (char) 0x0a, (char) 0x0d };
        m_FieldQualifierEscaped = escapePrefixChar + m_FieldQualifier;
        m_FieldDelimiterEscaped = escapePrefixChar + m_FieldDelimiter;
      }
      else
      {
        // Delimiters are not escaped
        m_FieldDelimiterEscaped = m_FieldDelimiter;
        // but require quoting
        m_QualifyCharArray = new[] { (char) 0x0a, (char) 0x0d, fieldDelimiterChar };

        // the Qualifier is repeated to so it can be recognized as not to be end the quoting
        m_FieldQualifierEscaped = m_FieldQualifier + m_FieldQualifier;
      }
    }

    /// <summary>
    ///   Writes the specified file reading from the given reader
    /// </summary>
    /// <param name="reader">A Data Reader with the data</param>
    /// <param name="output">The output.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public override async Task WriteReaderAsync(
      IFileReader reader,
      Stream output,
      CancellationToken cancellationToken)
    {
#if NETSTANDARD2_1_OR_GREATER
      await
#endif
      using var writer =
        new StreamWriter(output, EncodingHelper.GetEncoding(m_CodePageId, m_ByteOrderMark), 8192, true);
      SetColumns(reader);

      if (Columns.Count == 0)
        throw new FileWriterException("No columns defined to be written.");

      HandleWriteStart();

      var sb = new StringBuilder();
      if (!string.IsNullOrEmpty(Header))
      {
        sb.Append(Header);
        if (!Header.EndsWith(m_NewLine, StringComparison.Ordinal))
          sb.Append(m_NewLine);
      }

      var lastCol = Columns[Columns.Count - 1];

      if (m_ColumnHeader)
      {
        foreach (var columnInfo in Columns)
        {
          sb.Append(TextEncodeField(columnInfo.Name, columnInfo, true, null, QualifyText));
          if (!m_IsFixedLength && !ReferenceEquals(columnInfo, lastCol))
            sb.Append(m_FieldDelimiterChar);
        }

        sb.Append(m_NewLine);
      }

      while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)
             && !cancellationToken.IsCancellationRequested)
      {
        if (sb.Length > 32768)
        {
          await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
          sb.Length = 0;
        }

        var emptyColumns = 0;

        var row = new StringBuilder();
        foreach (var columnInfo in Columns)
        {
          // Number of columns might be higher than number of reader columns
          var col = reader.GetValue(columnInfo.ColumnOrdinal);
          if (col == DBNull.Value || (col is string text && string.IsNullOrEmpty(text)))
            emptyColumns++;
          else
            row.Append(TextEncodeField(col, columnInfo, false, reader, QualifyText));

          if (!m_IsFixedLength && !ReferenceEquals(columnInfo, lastCol))
            row.Append(m_FieldDelimiterChar);
        }

        if (emptyColumns == Columns.Count()) break;
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
                                                 || displayAs[0].Equals(m_FieldQualifierChar)
                                                 || displayAs[0].Equals(' '));
        else
          // quality any text or something containing a Qualify Char
          qualifyThis = dataType == DataTypeEnum.String || dataType == DataTypeEnum.TextToHtml
                                                        || displayAs.IndexOfAny(m_QualifyCharArray) > -1;
      }

      if (m_FieldDelimiter != m_FieldDelimiterEscaped)
        displayAs = displayAs.Replace(m_FieldDelimiter, m_FieldDelimiterEscaped);

      if (qualifyThis)
        return m_FieldQualifier + displayAs.Replace(m_FieldQualifier, m_FieldQualifierEscaped) + m_FieldQualifier;

      return displayAs;
    }

    private string TextEncodeField(
      object? dataObject,
      WriterColumn columnInfo,
      bool isHeader,
      IDataReader? reader,
      Func<string, DataTypeEnum, string>? handleQualify)
    {
      if (columnInfo is null)
        throw new ArgumentNullException(nameof(columnInfo));

      if (m_IsFixedLength && columnInfo.FieldLength == 0)
        throw new FileWriterException("For fix length output the length of the columns needs to be specified.");

      string displayAs;
      if (isHeader)
      {
        if (dataObject is null)
          throw new ArgumentNullException(nameof(dataObject));
        displayAs = Convert.ToString(dataObject);
      }
      else
      {
        displayAs = TextEncodeField(dataObject, columnInfo, reader);
        if (columnInfo.ValueFormat.DataType == DataTypeEnum.String)
        {
          // a new line of any kind will be replaced with the placeholder if set
          if (m_NewLinePlaceholder.Length > 0)
            displayAs = displayAs.HandleCrlfCombinations(m_NewLinePlaceholder);

          if (m_DelimiterPlaceholder.Length > 0 && m_FieldDelimiterChar != '\0')
            displayAs = displayAs.Replace(m_FieldDelimiterChar.ToString(),
              m_DelimiterPlaceholder);

          if (m_QualifierPlaceholder.Length > 0 && m_FieldQualifierChar != '\0')
            displayAs = displayAs.Replace(m_FieldQualifierChar.ToString(),
              m_QualifierPlaceholder);
        }
      }

      // Adjust the output in case its is fixed length
      if (m_IsFixedLength)
      {
        if (displayAs.Length <= columnInfo.FieldLength || columnInfo.FieldLength <= 0)
          return displayAs.PadRight(columnInfo.FieldLength, ' ');
        HandleWarning(columnInfo.Name,
          $"Text with length of {displayAs.Length} has been cut off after {columnInfo.FieldLength} character");
        return displayAs.Substring(0, columnInfo.FieldLength);
      }

      // Qualify text if required
      if (m_FieldQualifierChar != '\0' && handleQualify != null)
        return handleQualify(displayAs, columnInfo.ValueFormat.DataType);

      return displayAs;
    }
  }
}