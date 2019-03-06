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
using System.Data;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;

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
    public const string cFieldPlaceholderByNumber = "[value{0}]";

    /// <summary>
    ///   The header placeholder
    /// </summary>
    public const string cHeaderPlaceholder = "[column{0}]";

    private readonly StructuredFile m_StructuredWriterFile;

    /// <summary>
    ///   Initializes a new instance of the <see cref="StructuredFileWriter" /> class.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="cancellationToken">A cancellation token to stop writing the file</param>
    public StructuredFileWriter(StructuredFile file, CancellationToken cancellationToken)
      : base(file, cancellationToken)
    {
      Contract.Requires(file != null);
      m_StructuredWriterFile = file;
    }

    /// <summary>
    ///   Stores that data in the given stream.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="writer">The writer.</param>
    /// <param name="readerFileSetting">The file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///   Number of rows written
    /// </returns>
    protected void DataReader2Stream(IDataReader reader, TextWriter writer,
      CancellationToken cancellationToken)
    {
      Contract.Requires(reader != null);
      Contract.Requires(writer != null);

      var columnInfos = GetColumnInformation(reader);
      var enumerable = columnInfos.ToList();
      if (enumerable.IsEmpty())
        throw new ApplicationException("No columns defined to be written.");
      var recordEnd = m_StructuredWriterFile.FileFormat.NewLine.Replace("CR", "\r").Replace("LF", "\n").Replace(" ", "")
        .Replace("\t", "");
      HandleWriteStart();

      var numEmptyRows = 0;
      var numColumns = enumerable.Count();
      var
        sb = new StringBuilder(1024); // Assume a capacity of 1024 characters to start , data is flushed every 512 chars
      if (!string.IsNullOrEmpty(m_StructuredWriterFile.Header))
      {
        sb.Append(ReplacePlaceHolder(m_StructuredWriterFile.Header));
        if (!m_StructuredWriterFile.Header.EndsWith(recordEnd, StringComparison.Ordinal))
          sb.Append(recordEnd);
      }

      var withHeader = m_StructuredWriterFile.Row;
      var colNum = 0;
      foreach (var columnInfo in enumerable)
      {
        var placeHolder = string.Format(System.Globalization.CultureInfo.CurrentCulture, cHeaderPlaceholder, colNum);
        if (m_StructuredWriterFile.XMLEncode)
          withHeader = withHeader.Replace(placeHolder, HTMLStyle.XmlElementName(columnInfo.Header));
        else if (m_StructuredWriterFile.JSONEncode)
          withHeader = withHeader.Replace(placeHolder, HTMLStyle.JsonElementName(columnInfo.Header));
        else
          withHeader = withHeader.Replace(placeHolder, columnInfo.Header);
        colNum++;
      }

      withHeader = withHeader.Trim();
      while (reader.Read() && !cancellationToken.IsCancellationRequested)
      {
        NextRecord();

        if (sb.Length > 512)
        {
          writer.Write(sb.ToString());
          sb.Length = 0;
        }

        // Start a new line
        sb.Append(recordEnd);
        var emptyColumns = 0;
        var row = withHeader;
        colNum = 0;
        foreach (var columnInfo in enumerable)
        {
          Contract.Assume(columnInfo != null);
          var placeHolder1 = string.Format(System.Globalization.CultureInfo.CurrentCulture,  cFieldPlaceholderByNumber, colNum);
          var value = string.Empty;
          var placeHolder2 = string.Format(System.Globalization.CultureInfo.CurrentCulture, cFieldPlaceholderByName, columnInfo.Header);

          var col = reader.GetValue(columnInfo.ColumnOridinalReader);
          if (col == DBNull.Value)
          {
            emptyColumns++;
          }
          else
          {
            value = TextEncodeField(m_StructuredWriterFile.FileFormat, col, columnInfo, false, reader, null);
            if (m_StructuredWriterFile.XMLEncode)
              value = SecurityElement.Escape(value);
            else if (m_StructuredWriterFile.JSONEncode)
              value = HTMLStyle.JsonEncode(value);
          }

          row = row.Replace(placeHolder1, value).Replace(placeHolder2, value);
          colNum++;
        }

        if (emptyColumns == numColumns)
        {
          numEmptyRows++;
        }
        else
        {
          sb.Append(row);
          numEmptyRows = 0;
        }
      }

      sb.Append(ReplacePlaceHolder(m_StructuredWriterFile.Footer));
      writer.Write(sb.ToString());
    }

    /// <summary>
    ///   Writes the specified file reading from the given reader
    /// </summary>
    /// <param name="reader">A Data Reader with the data</param>
    /// <param name="fileSetting">The source setting or the data that could be different than the setting for is writer</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///   Number of records written
    /// </returns>
    protected override void Write(IDataReader reader, Stream output, CancellationToken cancellationToken)
    {
      Contract.Assume(!string.IsNullOrEmpty(m_StructuredWriterFile.FullPath));

      using (var writer = new StreamWriter(output, new UTF8Encoding(true), 4096))
      {
        DataReader2Stream(reader, writer, cancellationToken);
      }
    }
  }
}