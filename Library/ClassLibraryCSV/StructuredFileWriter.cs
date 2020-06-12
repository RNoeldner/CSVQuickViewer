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
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CsvTools
{
  /// <summary>
  ///   A class to write structured Files like XML or JASON
  /// </summary>
  public class StructuredFileWriter : BaseFileWriter, IFileWriter
  {
    /// <summary>
    ///   The field placeholder
    /// </summary>
    public const string cFieldPlaceholderByName = "[{0}]";

    /// <summary>
    ///   The c field placeholder
    /// </summary>
    private const string c_FieldPlaceholderByNumber = "[value{0}]";

    /// <summary>
    ///   The header placeholder
    /// </summary>
    private const string c_HeaderPlaceholder = "[column{0}]";

    private readonly StructuredFile m_StructuredWriterFile;

    /// <summary>
    ///   Initializes a new instance of the <see cref="StructuredFileWriter" /> class.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="timeZone">The timezone in the source</param>
    /// <param name="processDisplay">The process display.</param>
    public StructuredFileWriter([NotNull] StructuredFile file, [CanBeNull] string timeZone, [CanBeNull] IProcessDisplay processDisplay)
      : base(file, timeZone, processDisplay)
    {
      m_StructuredWriterFile = file;
    }

    /// <summary>
    ///   Writes the specified file reading from the given reader
    /// </summary>
    /// <param name="reader">A Data Reader with the data</param>
    /// <param name="output">The output.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected override async Task WriteReaderAsync([NotNull] IFileReader reader, [NotNull] Stream output,
      CancellationToken cancellationToken)
    {
      Contract.Assume(!string.IsNullOrEmpty(m_StructuredWriterFile.FullPath));

      using (var writer = new StreamWriter(output, new UTF8Encoding(true), 4096))
      {
        Columns.Clear();
        Columns.AddRange(ColumnInfo.GetSourceColumnInformation(m_StructuredWriterFile, reader));
        var numColumns = Columns.Count();
        if (numColumns == 0)
          throw new FileWriterException("No columns defined to be written.");
        var recordEnd = GetRecordEnd();
        HandleWriteStart();

        // Header
        if (!string.IsNullOrEmpty(m_StructuredWriterFile.Header))
        {
          var sbH = new StringBuilder();
          sbH.Append(ReplacePlaceHolder(m_StructuredWriterFile.Header));
          if (!m_StructuredWriterFile.Header.EndsWith(recordEnd, StringComparison.Ordinal))
            sbH.Append(recordEnd);
          await writer.WriteAsync(sbH.ToString()).ConfigureAwait(false);
        }

        // Static template for the row, built once
        var withHeader = m_StructuredWriterFile.Row;
        var colNum = 0;
        var placeHolderLookup1 = new Dictionary<int, string>();
        var placeHolderLookup2 = new Dictionary<int, string>();

        foreach (var columnInfo in Columns)
        {
          var placeHolder = string.Format(CultureInfo.CurrentCulture, c_HeaderPlaceholder, colNum);
          if (m_StructuredWriterFile.XMLEncode)
            withHeader = withHeader.Replace(placeHolder, HTMLStyle.XmlElementName(columnInfo.Column.Name));
          else if (m_StructuredWriterFile.JSONEncode)
            withHeader = withHeader.Replace(placeHolder, HTMLStyle.JsonElementName(columnInfo.Column.Name));
          else
            withHeader = withHeader.Replace(placeHolder, columnInfo.Column.Name);

          placeHolderLookup1.Add(colNum, string.Format(CultureInfo.CurrentCulture, c_FieldPlaceholderByNumber, colNum));
          placeHolderLookup2.Add(colNum,
            string.Format(CultureInfo.CurrentCulture, cFieldPlaceholderByName, columnInfo.Column.Name));
          colNum++;
        }

        withHeader = withHeader.Trim();
        var
          sb = new StringBuilder(
            1024); // Assume a capacity of 1024 characters to start, data is flushed every 512 chars
        while (await reader.ReadAsync().ConfigureAwait(false) && !cancellationToken.IsCancellationRequested)
        {
          NextRecord();

          // Start a new line
          sb.Append(recordEnd);
          var row = withHeader;
          colNum = 0;
          foreach (var value in from columnInfo in Columns
            let col = reader.GetValue(columnInfo.ColumnOrdinalReader)
            select m_StructuredWriterFile.XMLEncode
              ? SecurityElement.Escape(TextEncodeField(m_StructuredWriterFile.FileFormat, col, columnInfo, false,
                reader,
                null))
              : JsonConvert.ToString(col))
          {
            row = row.Replace(placeHolderLookup1[colNum], value).Replace(placeHolderLookup2[colNum], value);
            colNum++;
          }

          sb.Append(row);

          if (sb.Length <= 512) continue;
          await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
          sb.Length = 0;
        }

        if (sb.Length > 0)
          await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);

        // Footer
        if (!string.IsNullOrEmpty(m_StructuredWriterFile.Footer))
          await writer.WriteAsync(ReplacePlaceHolder(m_StructuredWriterFile.Footer)).ConfigureAwait(false);

        await writer.FlushAsync();
      }
    }
  }
}