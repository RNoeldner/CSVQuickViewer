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
using System.IO;
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
    private const string c_FieldPlaceholderByNumber = "[value{0}]";

    /// <summary>
    ///   The header placeholder
    /// </summary>
    private const string c_HeaderPlaceholder = "[column{0}]";

    private readonly StructuredFile m_StructuredWriterFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="StructuredFileWriter" /> class.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="processDisplay">The process display.</param>
    public StructuredFileWriter(StructuredFile file, IProcessDisplay processDisplay)
      : base(file, processDisplay)
    {
      Contract.Requires(file != null);
      m_StructuredWriterFile = file;
    }

    /// <summary>
    /// Stores that data in the given stream.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="writer">The writer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="FileWriterException">No columns defined to be written.</exception>
    private void DataReader2Stream(IDataReader reader, TextWriter writer,
      CancellationToken cancellationToken)
    {
      Contract.Requires(reader != null);
      Contract.Requires(writer != null);

      var columnInfos = GetSourceColumnInformation(reader);
      var numColumns = columnInfos.Count();
      if (numColumns == 0)
        throw new FileWriterException("No columns defined to be written.");
      var recordEnd = m_StructuredWriterFile.FileFormat.NewLine.Replace("CR", "\r").Replace("LF", "\n").Replace(" ", "")
        .Replace("\t", "");
      HandleWriteStart();
      

      // Header
      if (!string.IsNullOrEmpty(m_StructuredWriterFile.Header))
      {
        var sbH = new StringBuilder(1024);
        sbH.Append(ReplacePlaceHolder(m_StructuredWriterFile.Header));
        if (!m_StructuredWriterFile.Header.EndsWith(recordEnd, StringComparison.Ordinal))
          sbH.Append(recordEnd);
        writer.Write(sbH.ToString());
      }

      // Static template for the row, built once
      var withHeader = m_StructuredWriterFile.Row;
      var colNum = 0;
      var placeHolderLookup1 = new Dictionary<int, string>();
      var placeHolderLookup2 = new Dictionary<int, string>();

      foreach (var columnInfo in columnInfos)
      {
        var placeHolder = string.Format(System.Globalization.CultureInfo.CurrentCulture, c_HeaderPlaceholder, colNum);
        if (m_StructuredWriterFile.XMLEncode)
          withHeader = withHeader.Replace(placeHolder, HTMLStyle.XmlElementName(columnInfo.Header));
        else if (m_StructuredWriterFile.JSONEncode)
          withHeader = withHeader.Replace(placeHolder, HTMLStyle.JsonElementName(columnInfo.Header));
        else
          withHeader = withHeader.Replace(placeHolder, columnInfo.Header);

        placeHolderLookup1.Add(colNum, string.Format(System.Globalization.CultureInfo.CurrentCulture, c_FieldPlaceholderByNumber, colNum));
        placeHolderLookup2.Add(colNum, string.Format(System.Globalization.CultureInfo.CurrentCulture, cFieldPlaceholderByName, columnInfo.Header));
        colNum++;
      }

      withHeader = withHeader.Trim();
      
      var sb = new StringBuilder(1024); // Assume a capacity of 1024 characters to start, data is flushed every 512 chars
      while (reader.Read() && !cancellationToken.IsCancellationRequested)
      {
        NextRecord();

        // Start a new line
        sb.Append(recordEnd);
        var row = withHeader;
        colNum = 0;
        foreach (var columnInfo in columnInfos)
        {
          var col = reader.GetValue(columnInfo.ColumnOrdinalReader);
          var value = (m_StructuredWriterFile.XMLEncode) ?
                    SecurityElement.Escape(TextEncodeField(m_StructuredWriterFile.FileFormat, col, columnInfo, false, reader, null)) :
                    Newtonsoft.Json.JsonConvert.ToString(col);

          row = row.Replace(placeHolderLookup1[colNum], value).Replace(placeHolderLookup2[colNum], value);
          colNum++;
        }
        sb.Append(row);

        if (sb.Length <= 512) continue;
        writer.Write(sb.ToString());
        sb.Length = 0;
      }

      if (sb.Length > 0)
        writer.Write(sb.ToString());

      // Footer
      if (!string.IsNullOrEmpty(m_StructuredWriterFile.Footer))
        writer.Write(ReplacePlaceHolder(m_StructuredWriterFile.Footer));
    }

    /// <summary>
    /// Writes the specified file reading from the given reader
    /// </summary>
    /// <param name="reader">A Data Reader with the data</param>
    /// <param name="output">The output.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
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