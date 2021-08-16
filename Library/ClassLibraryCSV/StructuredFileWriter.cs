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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   A class to write structured Files like XML or JASON
  /// </summary>
  public abstract class StructuredFileWriter : BaseFileWriter, IFileWriter
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
    private readonly string m_Row;

    /// <summary>
    ///   Initializes a new instance of the <see cref="StructuredFileWriter" /> class.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="processDisplay">The process display.</param>
    public StructuredFileWriter(string id, string fullPath, IValueFormat? valueFormatGeneral = null,
      IFileFormat? fileFormat = null, string? recipient = null,
      bool unencrypted = false, string? identifierInContainer = null, string? footer = null, string? header = null,
      IEnumerable<IColumn>? columnDefinition = null, string fileSettingDisplay = "", string row = "",
      IProcessDisplay? processDisplay = null) : base(id, fullPath, valueFormatGeneral, fileFormat, recipient, unencrypted, identifierInContainer, footer, header, columnDefinition, fileSettingDisplay, processDisplay)
    {
      if (string.IsNullOrEmpty(row))
        throw new ArgumentNullException(nameof(row));
      m_Row = row;
    }
    protected abstract string ElementName(string input);
    protected abstract string Escape(object input, WriterColumn columnInfo, IFileReader reader);

    /// <summary>
    ///   Writes the specified file reading from the given reader
    /// </summary>
    /// <param name="reader">A Data Reader with the data</param>
    /// <param name="output">The output.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    protected override async Task WriteReaderAsync(IFileReader reader, Stream output,
      CancellationToken cancellationToken)
    {
      using var writer = new StreamWriter(output, new UTF8Encoding(true), 4096);
      SetColumns(reader);
      var numColumns = Columns.Count();
      if (numColumns == 0)
        throw new FileWriterException("No columns defined to be written.");
      var recordEnd = NewLine;
      HandleWriteStart();

      // Header
      if (!string.IsNullOrEmpty(Header))
      {
        var sbH = new StringBuilder();
        sbH.Append(Header);
        if (!Header.EndsWith(recordEnd, StringComparison.Ordinal))
          sbH.Append(recordEnd);
        await writer.WriteAsync(sbH.ToString()).ConfigureAwait(false);
      }

      // Static template for the row, built once
      var withHeader = m_Row;
      var colNum = 0;
      var placeHolderLookup1 = new Dictionary<int, string>();
      var placeHolderLookup2 = new Dictionary<int, string>();

      foreach (var columnInfo in Columns)
      {
        var placeHolder = string.Format(CultureInfo.CurrentCulture, c_HeaderPlaceholder, colNum);
        withHeader = withHeader.Replace(placeHolder, ElementName(columnInfo.Name));

        placeHolderLookup1.Add(colNum, string.Format(CultureInfo.CurrentCulture, c_FieldPlaceholderByNumber, colNum));
        placeHolderLookup2.Add(colNum,
          string.Format(CultureInfo.CurrentCulture, cFieldPlaceholderByName, columnInfo.Name));
        colNum++;
      }

      withHeader = withHeader.Trim();
      var
        sb = new StringBuilder(
          1024); // Assume a capacity of 1024 characters to start, data is flushed every 512 chars
      while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) &&
             !cancellationToken.IsCancellationRequested)
      {
        NextRecord();

        // Start a new line
        sb.Append(recordEnd);
        var row = withHeader;
        colNum = 0;
        foreach (var value in from columnInfo in Columns
                              let col = reader.GetValue(columnInfo.ColumnOrdinal)
                              select Escape(col, columnInfo, reader))
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
      if (!string.IsNullOrEmpty(Footer()))
        await writer.WriteAsync(Footer()).ConfigureAwait(false);

      await writer.FlushAsync();
    }
  }
}