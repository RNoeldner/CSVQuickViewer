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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   A Class to write CSV Files
  /// </summary>
  public class CsvFileWriter : BaseFileWriter, IFileWriter
  {
    private readonly bool m_ByteOrderMark;
    private readonly int m_CodePageId;
    private readonly string m_FieldDelimiter;
    private readonly string m_FieldDelimiterEscaped;
    private readonly string m_FieldQualifier;
    private readonly string m_FieldQualifierEscaped;
    private readonly char[] m_QualifyCharArray;
    protected readonly bool m_ColumnHeader;

    public CsvFileWriter(string id, string fullPath, bool hasFieldHeader, IValueFormat? valueFormat = null, IFileFormat? fileFormat = null,
      int codePageId = 65001, bool byteOrderMark = true, IEnumerable<IColumn>? columnDefinition = null, string? recipient = null,
      bool unencyrpted = false, string? identifierInContainer = null, string? header = null, string? footer = null, string fileSettingDisplay = "", IProcessDisplay? processDisplay = null)
      : base(id, fullPath, valueFormat, fileFormat, recipient, unencyrpted, identifierInContainer, footer, header, columnDefinition, fileSettingDisplay, processDisplay)
    {
      m_CodePageId = codePageId;
      m_ColumnHeader = hasFieldHeader;
      m_ByteOrderMark = byteOrderMark;
      m_FieldQualifier =  fileFormat?.FieldQualifierChar.ToStringHandle0() ?? string.Empty;
      m_FieldDelimiter =  fileFormat?.FieldDelimiterChar.ToStringHandle0() ?? string.Empty;

      if (fileFormat?.EscapeChar!='\0')
      {
        m_QualifyCharArray = new[] { (char) 0x0a, (char) 0x0d };
        m_FieldQualifierEscaped = fileFormat?.EscapeChar + m_FieldQualifier;
        m_FieldDelimiterEscaped = fileFormat?.EscapeChar + m_FieldDelimiter;
      }
      else
      {
        // Delimiters are not escaped
        m_FieldDelimiterEscaped = m_FieldDelimiter;
        // but require quoting
        m_QualifyCharArray = new[] { (char) 0x0a, (char) 0x0d, fileFormat.FieldDelimiterChar };

        // the Qualifier is repeated to so it can be recognized as not to be end the quoting
        m_FieldQualifierEscaped = m_FieldQualifier + m_FieldQualifier;
      }
    }

    /// <summary>
    ///   Writes the specified file reading from the given reader
    /// </summary>
    /// <param name="reader">A Data Reader with the data</param>
    /// <param name="output">The output.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected override async Task WriteReaderAsync(IFileReader reader, Stream output, CancellationToken cancellationToken)
    {
      using (var writer = new StreamWriter(output,
        EncodingHelper.GetEncoding(m_CodePageId, m_ByteOrderMark), 8192))
      {
        SetColumns(reader);

        if (Columns.Count == 0)
          throw new FileWriterException("No columns defined to be written.");

        HandleWriteStart();

        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(Header))
        {
          sb.Append(Header);
          if (!Header.EndsWith(NewLine, StringComparison.Ordinal))
            sb.Append(NewLine);
        }

        var lastCol = Columns[Columns.Count - 1];

        if (m_ColumnHeader)
        {
          foreach (var columnInfo in Columns)
          {
            sb.Append(TextEncodeField(FileFormat, columnInfo.Name, columnInfo, true, null, QualifyText));
            if (!FileFormat.IsFixedLength && !ReferenceEquals(columnInfo, lastCol))
              sb.Append(FileFormat.FieldDelimiterChar);
          }

          sb.Append(NewLine);
        }

        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) &&
               !cancellationToken.IsCancellationRequested)
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
              row.Append(TextEncodeField(FileFormat, col, columnInfo, false, reader, QualifyText));

            if (!FileFormat.IsFixedLength && !ReferenceEquals(columnInfo, lastCol))
              row.Append(FileFormat.FieldDelimiterChar);
          }

          if (emptyColumns == Columns.Count()) break;
          NextRecord();
          sb.Append(row);
          sb.Append(NewLine);
        }

        var footer = Footer();
        if (!string.IsNullOrEmpty(footer))
        {
          sb.Append(footer);
          if (!footer.EndsWith(NewLine, StringComparison.Ordinal))
            sb.Append(NewLine);
        }

        // remove the very last newline
        if (sb.Length > NewLine.Length)
        {
          sb.Length -= NewLine.Length;
          // and store the possibly remaining data
          await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
        }

        await writer.FlushAsync();
      }
    }

    private string QualifyText(string displayAs, DataType dataType, IFileFormat fileFormat)
    {
      var qualifyThis = fileFormat.QualifyAlways;
      if (!qualifyThis)
      {
        if (fileFormat.QualifyOnlyIfNeeded)
          // Qualify the text if the delimiter or Linefeed is present, or if the text starts with
          // the Qualifier
          qualifyThis = displayAs.Length > 0 && (displayAs.IndexOfAny(m_QualifyCharArray) > -1 ||
                                                 displayAs[0].Equals(fileFormat.FieldQualifierChar) ||
                                                 displayAs[0].Equals(' '));
        else
          // quality any text or something containing a Qualify Char
          qualifyThis = dataType == DataType.String || dataType == DataType.TextToHtml ||
                        displayAs.IndexOfAny(m_QualifyCharArray) > -1;
      }

      if (m_FieldDelimiter != m_FieldDelimiterEscaped)
        displayAs = displayAs.Replace(m_FieldDelimiter, m_FieldDelimiterEscaped);

      if (qualifyThis)
        return m_FieldQualifier + displayAs.Replace(m_FieldQualifier, m_FieldQualifierEscaped) + m_FieldQualifier;

      return displayAs;
    }
  }
}