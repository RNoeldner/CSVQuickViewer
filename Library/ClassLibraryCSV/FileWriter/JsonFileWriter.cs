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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   A class to write structured Json Files
  /// </summary>
  public sealed class JsonFileWriter : StructuredFileWriter
  {
    private readonly bool m_EmptyAsNull;

    /// <inheritdoc />
    /// <summary>
    ///   Initializes a new instance of the <see cref="T:CsvTools.JsonFileWriter" /> class.
    /// </summary>
    public JsonFileWriter(in string id,
      in string fullPath,
      in string? identifierInContainer,
      in string? footer,
      in string? header,
      bool emptyAsNull,
      int codePageId,
      bool byteOrderMark,
      IEnumerable<Column>? columnDefinition,
      in string fileSettingDisplay,
      in string row,
      TimeZoneChangeDelegate? timeZoneAdjust,
      in string sourceTimeZone,
      in string publicKey,
      bool unencrypted)
      : base(
        id,
        fullPath,
        unencrypted,
        identifierInContainer,
        footer,
        header,
        codePageId,
        byteOrderMark,
        columnDefinition,
        fileSettingDisplay,
        row,
        timeZoneAdjust ?? StandardTimeZoneAdjust.ChangeTimeZone,
        sourceTimeZone,
        publicKey)
    {
      m_EmptyAsNull = emptyAsNull;
    }

    protected override string RecordDelimiter() => ",";

    protected override string ElementName(string input) => HtmlStyle.JsonElementName(input);

    protected override string Escape(object? input, in WriterColumn columnInfo, in IDataRecord? reader)
    {
      if (columnInfo.ValueFormat.DataType == DataTypeEnum.String && string.IsNullOrEmpty(input?.ToString()))
        return m_EmptyAsNull ? JsonConvert.Null : "\"\"";

      var typedValue = ValueConversion(reader, input, columnInfo, TimeZoneAdjust, SourceTimeZone, HandleWarning);

      // sepcial handling of null
      if (m_EmptyAsNull && (typedValue is null  || typedValue == DBNull.Value))
        return "null";

      // Sepcial handling of DateTime
      if (typedValue is DateTime dtmVal)
      {
        if (dtmVal.Minute==0 && dtmVal.Hour==0 && dtmVal.Second==0)
          return JsonConvert.ToString(dtmVal.ToString("yyyy\\-MM\\-dd"));
        else
          return JsonConvert.ToString(dtmVal, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.RoundtripKind);
      }

      // JsonConvert.ToString handles the different types
      return JsonConvert.ToString(typedValue);
    }

    private WriterColumn FindWriterColumn(in string row, int startArray)
    {
      // Determine columnInfo := Name of the column should be before : 
      var sep = row.Substring(0, startArray).LastIndexOf(":");
      if (sep != -1)
      {
        var end = row.Substring(0, sep).LastIndexOf("\"");
        if (end!= -1)
        {


          var start = row.Substring(0, end-1).LastIndexOf("\"");
          if (start!= -1)
          {
            var colName = row.Substring(start+1, end-start-1);

            foreach (var columnInfo in WriterColumns)
              if (columnInfo.Name == colName)
              {
                return columnInfo;
              }
          }
        }
      }
      return new WriterColumn(string.Empty, new ValueFormat(), -1);
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

    public override string BuildRow(in string placeHolderText, in IDataReader reader)
    {
      var row = base.BuildRow(placeHolderText, reader);

      // Replace Array
      var rep = new Dictionary<string, string>();

      // do not use the regular [] as they can occur in text
      var startArray = row.IndexOf("\\[");
      while (startArray != -1)
      {
        var endArray = row.IndexOf("\\]", startArray);
        var array = row.Substring(startArray, endArray-startArray+2);
        rep.Add(array, HandleArray(array, FindWriterColumn(row, startArray)));
        startArray = row.IndexOf("\\[", endArray);
      }
      foreach (var replace in rep)
        row=row.Replace(replace.Key, replace.Value);

      return row;
    }

    private static IReadOnlyList<string> TrimmedSplit(string input, char split = ',', char escape = '/')
    {

      // Remove quoting  
      if (input.Length>= 2 && input[0] == input[input.Length-1] && (input[0] == '"' || input[0] == '\''))
        input = input.Substring(1, input.Length-2);

      if (input.Length==0)
        return Array.Empty<string>();

      var res = new List<string>();
      var sb = new StringBuilder();
      for (var pos = 0; pos<input.Length; pos++)
      {
        if (input[pos] == split)
        {
          // FOR XML always adds a leading seperator, so we ingore an empty part if its the very first one
          if (res.Count == 0 && pos==0)
            continue;
          res.Add(sb.ToString().Trim());
          sb = new StringBuilder();
          continue;
        }
        // Handle escaped 
        if (pos < input.Length-1 && input[pos]== escape && input[pos+1] == split)
          // eat the escape and store the split as is but do not seperate
          pos++;
        sb.Append(input[pos]);
      }
      // End of input store the remains
      res.Add(sb.ToString().Trim());
      return res;
    }


    public string HandleArray(string arrayPart, in WriterColumn columnInfoRoot)
    {
      var start = arrayPart.IndexOf('[');
      var end = arrayPart.LastIndexOf(']');
      var oneElement = arrayPart.Substring(start+1, end-start-2).Trim().Replace("\": \"", "\":\"");

      var slpitList = new Dictionary<WriterColumn, IReadOnlyList<string>>();

      var result = new StringBuilder();
      result.Append('[');

      if (oneElement.StartsWith("{", StringComparison.Ordinal) && oneElement.EndsWith("}", StringComparison.Ordinal))
      {
        try
        {
          var jtoken = JToken.Parse(oneElement);
          foreach (JProperty prop in jtoken.Children())
          {
            var list = prop.Value?.ToString() ?? string.Empty;
            var col = WriterColumns.FirstOrDefault(x => x.Name.Equals(prop.Name, StringComparison.OrdinalIgnoreCase)) ?? new WriterColumn(prop.Name, new ValueFormat(), -1);
            if (slpitList.ContainsKey(col))
              Logger.Error("Duplicate property {property}, property will be ignored", prop.Name);
            else
              slpitList.Add(col, TrimmedSplit(list));
          }
        }
        catch (Exception ex)
        {
          Logger.Warning(ex, "Json object could not be parsed {element}", oneElement);
        }
      }

      // If there are no properties assume we have a list only
      if (slpitList.Count  == 0)
      {
        foreach (var entry in TrimmedSplit(oneElement))
        {
          result.Append(Escape(entry, columnInfoRoot, null));
          result.Append(',');
        }
        if (result.Length>1)
          result.Length--;
      }
      else
      {
        var numMatches = int.MaxValue;
        foreach (var prop in slpitList)
        {
          if (prop.Value.Count <numMatches && prop.Value.Count!=0)
          {
            if (numMatches != int.MaxValue)
              Logger.Warning($"List does not contain the same number of of entries, {prop.Key.Name} has less than before");
            numMatches =prop.Value.Count;
          }
        }

        // now fill the return value
        for (var index = 0; index < numMatches; index++)
        {
          // open Object
          result.Append('{');
          // list properties of object
          foreach (var prop in slpitList)
          {

            JsonConvert.ToString(prop.Key.Name);
            result.Append($"\"{prop.Key.Name}\":");
            try
            {
              result.Append(Escape(prop.Value[index], prop.Key, null));
            }
            catch (Exception)
            {
              result.Append(Escape(null, prop.Key, null));
            }
            result.Append(',');
          }
          // remove last ,
          result.Length--;
          // close object
          result.Append('}');
          result.Append(',');
        }

        // remove last ,
        result.Length--;
      }
      result.Append(']');
      return result.ToString();
    }
  }
}