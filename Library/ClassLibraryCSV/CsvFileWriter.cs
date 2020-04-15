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
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
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
    private readonly ICsvFile m_CsvFile;
    private readonly string m_FieldDelimiter;
    private readonly string m_FieldDelimiterEscaped;
    private readonly string m_FieldQualifier;
    private readonly string m_FieldQualifierEscaped;
    private readonly char[] m_QualifyCharArray;

    /// <summary>
    ///   Initializes a new instance of the <see cref="CsvFileWriter" /> class.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="timeZone">The timezone to convert to</param>
    /// <param name="processDisplay">The process display.</param>
    public CsvFileWriter(ICsvFile file, string timeZone, IProcessDisplay processDisplay)
      : base(file, timeZone, processDisplay)
    {
      Contract.Requires(file != null);
      m_CsvFile = file;

      m_FieldQualifier = m_CsvFile.FileFormat.FieldQualifierChar.ToString(CultureInfo.CurrentCulture);
      m_FieldDelimiter = m_CsvFile.FileFormat.FieldDelimiterChar.ToString(CultureInfo.CurrentCulture);
      if (!string.IsNullOrEmpty(file.FileFormat.EscapeCharacter))
      {
        m_QualifyCharArray = new[] { (char)0x0a, (char)0x0d };
        m_FieldQualifierEscaped = file.FileFormat.EscapeCharacterChar + m_FieldQualifier;
        m_FieldDelimiterEscaped = file.FileFormat.EscapeCharacterChar + m_FieldDelimiter;
      }
      else
      {
        m_QualifyCharArray = new[] { (char)0x0a, (char)0x0d, m_CsvFile.FileFormat.FieldDelimiterChar };
        m_FieldQualifierEscaped = new string(m_CsvFile.FileFormat.FieldQualifierChar, 2);
        m_FieldDelimiterEscaped = new string(m_CsvFile.FileFormat.FieldDelimiterChar, 1);
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
        EncodingHelper.GetEncoding(m_CsvFile.CodePageId, m_CsvFile.ByteOrderMark), 8192))
      {
        var sb = WriterStart(reader, out var recordEnd);

        while (await reader.ReadAsync() && !cancellationToken.IsCancellationRequested)
        {
          if (sb.Length > 32768)
          {
            await writer.WriteAsync(sb.ToString());
            sb.Length = 0;
          }
          if (WriterProcessRecord(reader, sb, writer, Columns.Count(), recordEnd)) break;
        }


        if (!string.IsNullOrEmpty(m_CsvFile.Footer))
          sb.Append(ReplacePlaceHolder(StringUtils.HandleCRLFCombinations(m_CsvFile.Footer, recordEnd)));
        else if (sb.Length > 0)
          sb.Length -= recordEnd.Length;

        await writer.WriteAsync(sb.ToString());
      }
    }

    protected override void WriteReader(IFileReader reader, Stream output, CancellationToken cancellationToken)
    {
      using (var writer = new StreamWriter(output,
        EncodingHelper.GetEncoding(m_CsvFile.CodePageId, m_CsvFile.ByteOrderMark), 8192))
      {
        var sb = WriterStart(reader, out var recordEnd);

        while (reader.Read() && !cancellationToken.IsCancellationRequested)
        {
          if (sb.Length > 32768)
          {
            writer.Write(sb.ToString());
            sb.Length = 0;
          }
          if (WriterProcessRecord(reader, sb, writer, Columns.Count(), recordEnd)) break;
        }


        if (!string.IsNullOrEmpty(m_CsvFile.Footer))
          sb.Append(ReplacePlaceHolder(StringUtils.HandleCRLFCombinations(m_CsvFile.Footer, recordEnd)));
        else if (sb.Length > 0)
          sb.Length -= recordEnd.Length;

        writer.Write(sb.ToString());
      }
    }

    private bool WriterProcessRecord(IDataReader reader, StringBuilder sb, TextWriter writer,
      int numColumns, string recordEnd)
    {
      NextRecord();
      var emptyColumns = 0;
      foreach (var columnInfo in Columns)
      {
        var col = reader.GetValue(columnInfo.ColumnOrdinalReader);
        if (col == DBNull.Value)
          emptyColumns++;

        sb.Append(TextEncodeField(m_CsvFile.FileFormat, col, columnInfo, false, reader, QualifyText));

        if (!m_CsvFile.FileFormat.IsFixedLength)
          sb.Append(m_CsvFile.FileFormat.FieldDelimiterChar);
      }

      if (!m_CsvFile.FileFormat.IsFixedLength)
        sb.Length--;
      if (emptyColumns == numColumns)
      {
        // Remove the delimiters again
        if (!m_CsvFile.FileFormat.IsFixedLength)
          sb.Length -= numColumns;
        return true;
      }

      sb.Append(recordEnd);
      return false;
    }

    private StringBuilder WriterStart(IDataReader reader, out string recordEnd)
    {
      Columns.Clear();
      Columns.AddRange(ColumnInfo.GetSourceColumnInformation(m_CsvFile, reader));

      if (Columns.Count == 0)
        throw new FileWriterException("No columns defined to be written.");
      
      recordEnd = this.GetRedordEnd();
      HandleWriteStart();

      var sb = new StringBuilder();
      if (!string.IsNullOrEmpty(m_CsvFile.Header))
      {
        sb.Append(ReplacePlaceHolder(StringUtils.HandleCRLFCombinations(m_CsvFile.Header, recordEnd)));
        if (!m_CsvFile.Header.EndsWith(recordEnd, StringComparison.Ordinal))
          sb.Append(recordEnd);
      }

      if (m_CsvFile.HasFieldHeader)
      {
        sb.Append(GetHeaderRow(Columns));
        sb.Append(recordEnd);
      }

      return sb;
    }

    private string GetHeaderRow(IEnumerable<ColumnInfo> columnInfos)
    {
      Contract.Requires(columnInfos != null);

      var sb = new StringBuilder();
      foreach (var columnInfo in columnInfos)
      {
        Contract.Assume(columnInfo != null);
        sb.Append(TextEncodeField(m_CsvFile.FileFormat, columnInfo.Column.Name, columnInfo, true, null, QualifyText));
        if (!m_CsvFile.FileFormat.IsFixedLength)
          sb.Append(m_CsvFile.FileFormat.FieldDelimiterChar);
      }

      if (!m_CsvFile.FileFormat.IsFixedLength)
        sb.Length--;
      return sb.ToString();
    }

    private string QualifyText(string displayAs, DataType dataType, FileFormat fileFormat)
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