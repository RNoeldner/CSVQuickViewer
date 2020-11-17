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

using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   Json text file reader
  /// </summary>
  public class JsonFileReader : BaseFileReaderTyped, IFileReader
  {
    private IImprovedStream m_ImprovedStream;
    private JsonTextReader m_JsonTextReader;
    private StreamReader m_StreamReader;

    public JsonFileReader([NotNull] IImprovedStream improvedStream,
                          [CanBeNull] IEnumerable<IColumn> columnDefinition = null,
                          long recordLimit = 0,
                          bool treatNbspAsSpace = false, bool trim = false,
                          string treatTextAsNull = null) :
      base("stream", columnDefinition, recordLimit, trim, treatTextAsNull, treatNbspAsSpace)
    {
      m_ImprovedStream = improvedStream;
      m_SelfOpenedStream = false;
    }

    public JsonFileReader([NotNull] string fullPath,
                          [CanBeNull] IEnumerable<IColumn> columnDefinition = null,
                          long recordLimit = 0,
                          bool treatNbspAsSpace = false, bool trim = false,
                          string treatTextAsNull = null) :
      base(fullPath, columnDefinition, recordLimit, trim, treatTextAsNull, treatNbspAsSpace)
    {
      if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
      m_SelfOpenedStream = true;
    }

    public JsonFileReader(IFileSettingPhysicalFile fileSetting,
                          IProcessDisplay processDisplay)
      : this(fileSetting.FullPath,
        fileSetting.ColumnCollection, fileSetting.RecordLimit,
        fileSetting.TreatNBSPAsSpace) => SetProgressActions(processDisplay);

    /// <summary>
    ///   Gets a value indicating whether this instance is closed.
    /// </summary>
    /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
    public override bool IsClosed => m_StreamReader == null;

    public override void Close()
    {
      base.Close();

      m_JsonTextReader?.Close();
      ((IDisposable) m_JsonTextReader)?.Dispose();
      m_JsonTextReader = null;

      m_StreamReader?.Dispose();
      m_StreamReader = null;
      if (!m_SelfOpenedStream) return;
      m_ImprovedStream?.Dispose();
      m_ImprovedStream = null;
    }

    public override async Task OpenAsync(CancellationToken token)
    {
      Logger.Information("Opening JSON file {filename}", FileName);
      await BeforeOpenAsync(
          $"Opening JSON file {FileSystemUtils.GetShortDisplayFileName(FileName)}")
        .ConfigureAwait(false);
      Retry:
      try
      {
        ResetPositionToStartOrOpen();
        var line = GetNextRecord(true, token);

        // get column names for some time
        var colNames = new Dictionary<string, DataType>();
        var stopwatch = new Stopwatch();
        // read additional rows to see if we have some extra columns        
        while (line != null)
        {
          foreach (var keyValue in line)
          {
            if (!colNames.ContainsKey(keyValue.Key))
              colNames.Add(keyValue.Key, keyValue.Value?.GetType().GetDataType() ?? DataType.String);
          }

          if (stopwatch.ElapsedMilliseconds > 200)
            break;
          line = GetNextRecord(true, token);
        }

        InitColumn(colNames.Count);
        ParseColumnName(colNames.Select(colValue => colValue.Key), colNames.Select(colValue => colValue.Value));

        FinishOpen();

        ResetPositionToStartOrOpen();
      }
      catch (Exception ex)
      {
        if (ShouldRetry(ex, token))
          goto Retry;

        Close();
        var appEx = new FileReaderException(
          "Error opening structured text file for reading.\nPlease make sure the file does exist, is of the right type and is not locked by another process.",
          ex);
        HandleError(-1, appEx.ExceptionMessages());
        HandleReadFinished();
        throw appEx;
      }
      finally
      {
        HandleShowProgress("");
      }
    }

    public override Task<bool> ReadAsync(CancellationToken token) => Task.FromResult(Read(token));

    public override void ResetPositionToFirstDataRow() => ResetPositionToStartOrOpen();

    public override bool Read(CancellationToken token)
    {
      if (!EndOfFile && !token.IsCancellationRequested)
      {
        var couldRead = GetNextRecord(false, token) != null;
        if (couldRead) RecordNumber++;
        InfoDisplay(couldRead);

        if (couldRead && !IsClosed && RecordNumber <= RecordLimit)
          return true;
      }

      EndOfFile = true;
      HandleReadFinished();
      return false;
    }

    /// <summary>
    ///   Reads a data row from the JsonTextReader and stores the values and text, this will flatten
    ///   the structure of the Json file
    /// </summary>
    /// <returns>A collection with name and value of the properties</returns>
    private ICollection<KeyValuePair<string, object>> GetNextRecord(bool throwError,
                                                                    CancellationToken token)
    {
      try
      {
        var headers = new Dictionary<string, bool>();
        var keyValuePairs = new Dictionary<string, object>();
        while (m_JsonTextReader.TokenType != JsonToken.StartObject
               // && m_JsonTextReader.TokenType != JsonToken.PropertyName
               && m_JsonTextReader.TokenType != JsonToken.StartArray)
          if (!m_JsonTextReader.Read())
            return null;

        // sore the parent Property Name in parentKey
        var startKey = string.Empty;
        var endKey = "<dummy>";
        var key = string.Empty;

        // sore the current Property Name in key
        StartLineNumber = m_JsonTextReader.LineNumber;
        var inArray = false;
        do
        {
          switch (m_JsonTextReader.TokenType)
          {
            // either the start of the row or a sub object that will be flattened
            case JsonToken.StartObject:
              if (startKey.Length == 0)
                startKey = m_JsonTextReader.Path;
              break;

            case JsonToken.EndObject:
              endKey = m_JsonTextReader.Path;
              break;

            // arrays will be read as multi line columns
            case JsonToken.StartArray:
              inArray = true;
              break;

            case JsonToken.PropertyName:
              key = startKey.Length > 0 ? m_JsonTextReader.Path.Substring(startKey.Length + 1) : m_JsonTextReader.Path;
              if (!headers.ContainsKey(key))
              {
                headers.Add(key, false);
                keyValuePairs.Add(key, null);
              }

              break;

            case JsonToken.Raw:
            case JsonToken.Null:
              headers[key] = true;
              break;

            case JsonToken.Date:
            case JsonToken.Bytes:
            case JsonToken.Integer:
            case JsonToken.Float:
            case JsonToken.String:
            case JsonToken.Boolean:
              // in case there is a property its a real column, otherwise its used for structuring only
              headers[key] = true;

              // in case we are in an array combine all values but separate them with linefeed
              if (inArray && keyValuePairs[key] != null)
                keyValuePairs[key] = keyValuePairs[key].ToString() + '\n' + m_JsonTextReader.Value;
              else
                keyValuePairs[key] = m_JsonTextReader.Value;
              break;

            case JsonToken.EndArray:
              inArray = false;
              break;

            case JsonToken.None:
              break;

            case JsonToken.StartConstructor:
              break;

            case JsonToken.Comment:
              break;

            case JsonToken.Undefined:
              break;

            case JsonToken.EndConstructor:
              break;

            default:
              throw new ArgumentOutOfRangeException();
          }

          token.ThrowIfCancellationRequested();
        } while (!(m_JsonTextReader.TokenType == JsonToken.EndObject && startKey == endKey)
                 && m_JsonTextReader.Read());

        EndLineNumber = m_JsonTextReader.LineNumber;

        foreach (var kv in headers.Where(kv => !kv.Value))
          keyValuePairs.Remove(kv.Key);

        // store the information into our fixed structure, even if the tokens in Json change order
        // they will aligned
        if (Column == null || Column.Length == 0) return keyValuePairs;
        var columnNumber = 0;
        foreach (var col in Column)
        {
          if (keyValuePairs.TryGetValue(col.Name, out CurrentValues[columnNumber]))
          {
            if (CurrentValues[columnNumber] == null)
              CurrentRowColumnText[columnNumber] = null;
            else
            {
              var orgVal = CurrentValues[columnNumber].ToString();
              CurrentRowColumnText[columnNumber] = orgVal;

              if (!string.IsNullOrEmpty(orgVal) && !col.Ignore && col.ValueFormat.DataType >= DataType.String)
              {
                CurrentRowColumnText[columnNumber] = TreatNbspTestAsNullTrim(HandleTextSpecials(orgVal, columnNumber));
                CurrentValues[columnNumber] = CurrentRowColumnText[columnNumber];
              }
            }
          }

          columnNumber++;
        }

        if (keyValuePairs.Count < FieldCount)
          HandleWarning(-1,
            $"Line {StartLineNumber} has fewer columns than expected ({keyValuePairs.Count}/{FieldCount}).");
        else if (keyValuePairs.Count > FieldCount)
          HandleWarning(-1,
            $"Line {StartLineNumber} has more columns than expected ({keyValuePairs.Count}/{FieldCount}). The data in extra columns is not read.");

        return keyValuePairs;
      }
      catch (Exception ex)
      {
        if (throwError)
          throw;
        // A serious error will be logged and its assume the file is ended
        HandleError(-1, ex.Message);
        EndOfFile = true;
        return null;
      }
    }

    /// <summary>
    ///   Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and MaxValue</returns>
    protected override double GetRelativePosition()
    {
      var byFile = m_ImprovedStream?.Percentage ?? 0;
      if (RecordLimit > 0 && RecordLimit < long.MaxValue)
        // you can either reach the record limit or the end of the stream, whatever is faster
        return Math.Max(((double) RecordNumber) / RecordLimit, byFile);
      return byFile;
    }

    /// <summary>
    ///   Resets the position and buffer to the first line, excluding headers, use
    ///   ResetPositionToStart if you want to go to first data line
    /// </summary>
    private void ResetPositionToStartOrOpen()
    {
      if (m_SelfOpenedStream)
      {
        m_ImprovedStream?.Dispose();
        m_ImprovedStream = FunctionalDI.OpenStream(new SourceAccess(FullPath));
      }
      else
        m_ImprovedStream.Seek(0, SeekOrigin.Begin);

      // in case we can not seek need to reopen the stream reader
      m_StreamReader?.Close();
      m_StreamReader = new StreamReader(m_ImprovedStream as Stream ?? throw new InvalidOperationException(), Encoding.UTF8, true, 4096, true);

      // End Line should be at 1, later on as the line is read the start line s set to this value
      StartLineNumber = 1;
      EndLineNumber = 1;
      RecordNumber = 0;

      EndOfFile = m_StreamReader.EndOfStream;

      m_JsonTextReader?.Close();
      m_JsonTextReader = new JsonTextReader(m_StreamReader) { SupportMultipleContent = true };
    }
  }
}