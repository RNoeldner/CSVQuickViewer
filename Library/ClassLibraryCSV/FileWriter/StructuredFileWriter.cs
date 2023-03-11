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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc cref="BaseFileWriter" />
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
    private const string cFieldPlaceholderByNumber = "[value{0}]";

    /// <summary>
    ///   The header placeholder
    /// </summary>
    private const string cHeaderPlaceholder = "[column{0}]";

    private readonly string m_Row;
    private readonly bool m_ByteOrderMark;
    private readonly int m_CodePageId;

    /// <summary>
    ///   Initializes a new instance class used for <see cref="JsonFileWriter"/> and <see cref="XmlFileWriter"/>/>
    /// </summary>
    /// <param name="id">Information for  Placeholder of ID</param>
    /// <param name="fullPath">Fully qualified path of teh file to write</param>
    /// <param name="unencrypted">If <c>true</c> teh not pgp encrypted file is kept for reference</param>
    /// <param name="identifierInContainer">In case the file is written into an archive that does support multiple files, name of teh file in the archive.</param>
    /// <param name="footer">Footer to be written after all rows are written</param>
    /// <param name="header">Header to be written before data and/or Header is written</param>
    /// <param name="codePageId">The Code Page for encoding of characters</param>
    /// <param name="byteOrderMark">If <c>true</c>a Byte Order Mark will be added</param>
    /// <param name="columnDefinition">Individual column definitions for formatting</param>
    /// <param name="fileSettingDisplay">Info text for logging and process report</param>
    /// <param name="row">Placeholder for a row</param>
    /// <param name="timeZoneAdjust">Delegate for TimeZone Conversions</param>
    /// <param name="sourceTimeZone">Identified for the timezone teh values are currently stored as</param>
    /// <param name="publicKey">Key used for encryption of the written data (not implemented in all Libraries)</param>
    protected StructuredFileWriter(in string id,
      in string fullPath,
      bool unencrypted,
      in string? identifierInContainer,
      in string? footer,
      in string? header,
      int codePageId,
      bool byteOrderMark,
      in IEnumerable<Column>? columnDefinition,
      in string fileSettingDisplay,
      in string row,
      in TimeZoneChangeDelegate timeZoneAdjust,
      in string sourceTimeZone,
      in string publicKey)
      : base(
        id,
        fullPath,
        null,
        unencrypted,
        identifierInContainer,
        footer,
        header,
        columnDefinition,
        fileSettingDisplay,
        timeZoneAdjust,
        sourceTimeZone,
        publicKey)
    {
      if (string.IsNullOrEmpty(row))
        throw new ArgumentException($"{nameof(row)} can not be empty");

      m_CodePageId = codePageId;
      m_ByteOrderMark = byteOrderMark;
      m_Row = row;
    }

    protected abstract string ElementName(string input);

    protected abstract string RecordDelimiter();

    protected abstract string Escape(object? input, in WriterColumn columnInfo, in IFileReader reader);

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
      var columns = GetColumnInformation(ValueFormatGeneral, ColumnDefinition, reader);

      var numColumns = columns.Count();
      if (numColumns == 0)
        throw new FileWriterException("No columns defined to be written.");

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var writer =
        new StreamWriter(output, EncodingHelper.GetEncoding(m_CodePageId, m_ByteOrderMark), 4096, true);
      const string recordEnd = "\r\n";

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

      foreach (var columnName in columns.Select(x => x.Name))
      {
        var placeHolder = string.Format(CultureInfo.CurrentCulture, cHeaderPlaceholder, colNum);
        withHeader = withHeader.Replace(placeHolder, ElementName(columnName));

        placeHolderLookup1.Add(colNum, string.Format(CultureInfo.CurrentCulture, cFieldPlaceholderByNumber, colNum));
        placeHolderLookup2.Add(
          colNum,
          string.Format(CultureInfo.CurrentCulture, cFieldPlaceholderByName, columnName));
        colNum++;
      }

      withHeader = withHeader.Trim();
      var sb = new StringBuilder(
        2048); // Assume a capacity of 2048 characters to start, data is flushed every 1024 chars
      while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)
             && !cancellationToken.IsCancellationRequested)
      {
        NextRecord();
        if (Records > 1)
          sb.Append(RecordDelimiter());

        // Start a new line
        sb.Append(recordEnd);
        var row = withHeader;
        colNum = 0;
        foreach (var columnInfo in columns)
        {
          var value = Escape(reader.GetValue(columnInfo.ColumnOrdinal), columnInfo, reader);
          row = row.Replace(placeHolderLookup1[colNum], value).Replace(placeHolderLookup2[colNum], value);
          colNum++;
        }

        sb.Append(row);

        if (sb.Length <= 1024) continue;
        ReportProgress?.Report(new ProgressInfo("Writing", reader.RecordNumber));
        await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
        sb.Length = 0;
      }

      if (sb.Length > 0)
        await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);

      // Footer
      if (!string.IsNullOrEmpty(Footer()))
        await writer.WriteAsync(Footer()).ConfigureAwait(false);

      await writer.FlushAsync().ConfigureAwait(false);
    }

    public static string GetJsonRow(IEnumerable<Column> cols)
    {
      var sb = new StringBuilder("{");
      // { "firstName":"John", "lastName":"Doe"},
      foreach (var col in cols)
        sb.AppendFormat("\"{0}\":{1},\n", HtmlStyle.JsonElementName(col.Name),
          string.Format(cFieldPlaceholderByName, col.Name));
      if (sb.Length > 1)
        sb.Length -= 2;
      sb.AppendLine("}");

      return sb.ToString();
    }
  }
}