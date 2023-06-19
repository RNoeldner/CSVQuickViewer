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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
    ///   Loops through the reader and invoking recordAction for each row
    /// </summary>
    /// <param name="reader">A data reader with the data</param>
    /// <param name="recordAction">Async action to be performed</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public async Task WriteReaderAsync(IFileReader reader, Func<string, Task> recordAction, CancellationToken cancellationToken)
    {
      var columns = GetColumnInformation(ValueFormatGeneral, ColumnDefinition, reader);
      HandleWriteStart();
      var intervalAction = IntervalAction.ForProgress(ReportProgress);
      while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false) && !cancellationToken.IsCancellationRequested)
      {
        NextRecord();
        intervalAction?.Invoke(ReportProgress!, $"Record {reader.RecordNumber:N0}", reader.Percent);
        await recordAction(BuildRow(m_Row, reader, columns));
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

      var numColumns = columns.Count();
      if (numColumns == 0)
        throw new FileWriterException("No columns defined to be written.");

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await
#endif
      using var writer = new StreamWriter(output, EncodingHelper.GetEncoding(m_CodePageId, m_ByteOrderMark), 4096, true);
      const string recordEnd = "\r\n";

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
      var sb = new StringBuilder(2048);

      await WriteReaderAsync(reader, async row =>
      {
        if (Records > 1)
          sb.Append(RecordDelimiter());
        // Start a new line
        sb.Append(recordEnd);
        sb.Append(BuildRow(m_Row, reader, columns));

        if (sb.Length > 1024)
        {
          await writer.WriteAsync(sb.ToString()).ConfigureAwait(false);
          sb.Length = 0;
        }
      }, cancellationToken);

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
        sb.AppendFormat("\"{0}\":{1},\n", HtmlStyle.JsonElementName(col.Name), string.Format(cFieldPlaceholderByName, col.Name));
      if (sb.Length > 1)
        sb.Length -= 2;
      sb.AppendLine("}");
      return sb.ToString();
    }

    public string BuildRow(in string placeHolderText, in IFileReader reader, in IEnumerable<WriterColumn> columns)
    {
      var row = placeHolderText;
      foreach (var columnInfo in columns)
        row = row.Replace(string.Format(cFieldPlaceholderByName, columnInfo.Name), Escape(reader.GetValue(columnInfo.ColumnOrdinal), columnInfo, reader));

      /*
       * 
       * {
       *   "outer":[outer],
       *   "array":[{
       *    "name":[name],
       *    "id":[id]
       *   }]      
       * } 
       * -->  name="name1|name2" && id ="id1|id2"
       * {
       *   "outer":'outer',
       *   "array":[{
       *    "name":'name1|name2',
       *    "id":'id1|id2'
       *   }]      
       * } 
       
       // So far so good now handle arrays, each property in an array is derived from a split of the field
       * {
       *   "outer":'outer',
       *   array":[
       *    {
       *    "name": 'name1',
       *    "id": 'id1'
       *    },
       *    {
       *    "name": 'name2',
       *    "id": 'id2'
       *    }
       *   ]
       * } 
       * */

      // first check if we have arrays
      if (row.IndexOf('[')!= -1 && row.IndexOf(']')!= -1)
      {
        var startArray = row.IndexOf("[");
        var rep = new Dictionary<string, string>();
        while (startArray != -1)
        {
          var endArray = row.IndexOf("]", startArray);
          var array = row.Substring(startArray, endArray-startArray+1);
          rep.Add(array, HandleArray(array));          
          startArray = row.IndexOf("[", endArray);
        }        
        foreach(var replace in rep)
          row=row.Replace(replace.Key, replace.Value);
      }     
      return row;
    }

    public static string HandleArray(string arrayPart)
    {
      /*
       *  [{
       *    "name":'name1|name2',
       *    "id":'id1|id2'
       *   }]   
       *   
       *  -->
       *   [
       *    {
       *    "name": 'name1',
       *    "id": 'id1'
       *    },
       *    {
       *    "name": 'name2',
       *    "id": 'id2'
       *    }
       *   ]       
       */
      var start = arrayPart.IndexOf('[');
      var end = arrayPart.LastIndexOf(']');
      var oneElement = arrayPart.Substring(start+1, end-start-1).Trim();
      /*
       * {
       *    "name":'name1|name2',
       *    "id":'id1|id2'
       * }
       * or "name":'name1|name2|name3'
       */
      var properties = new Dictionary<string, string[]>();
      var jtoken = JToken.Parse(oneElement);
      foreach (JProperty prop in jtoken.Children().OfType<JProperty>())
      {        
        properties.Add(prop.Value.ToString(), prop.Value.ToString().Split(StaticCollections.ListDelimiterChars) ?? Array.Empty<string>());
      }
      if (properties.Count  == 0)
      {
        var values= (oneElement[0]== '"' || oneElement[0]== '\'') ? oneElement.Substring(1, oneElement.Length-2) : oneElement;        
        properties.Add(oneElement, values.Split(StaticCollections.ListDelimiterChars) ?? Array.Empty<string>());
      }
        

      var result = new StringBuilder();
      result.Append('[');
      var numMatches = int.MaxValue;
      foreach (var prop in properties)
        if (prop.Value.Length <numMatches)
          numMatches =prop.Value.Length;

      for (var index = 0; index < numMatches; index++)
      {
        var resultingElement = oneElement;

        foreach (var prop in properties)
        {
          if (prop.Key[0]== '"')
            resultingElement= resultingElement.ReplaceCaseInsensitive(prop.Key, '"' + prop.Value[index] + '"');
          else if (prop.Key[0]== '\'')
            resultingElement= resultingElement.ReplaceCaseInsensitive(prop.Key, "'" + prop.Value[index] + "'");
          else
            resultingElement= resultingElement.ReplaceCaseInsensitive(prop.Key, prop.Value[index]);
        }
          
        result.Append(resultingElement);
        result.Append(',');
      }
      // remove last ,
      result.Length--;
      result.Append(']');

      return result.ToString();
      
    }
  }
}