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
  /// <inheritdoc cref="CsvTools.IFileReader" />
  /// <summary>
  ///   Json text file reader
  /// </summary>
  public sealed class JsonFileReader : BaseFileReaderTyped, IFileReader
  {
    private Stream? m_Stream;

    private JsonTextReader? m_JsonTextReader;

    private StreamReader? m_StreamReader;

    
    /// <summary>
    /// Constructor for Json Reader
    /// </summary>
    /// <param name="stream">Stream to read from</param>
    /// <param name="columnDefinition">List of column definitions</param>
    /// <param name="recordLimit">Number of records that should be read</param>
    /// <param name="trim">Trim all read text</param>
    /// <param name="treatTextAsNull">Value to be replaced with NULL in Text</param>
    /// <param name="treatNbspAsSpace">nbsp in text will be replaced with Space</param>
    /// <param name="timeZoneAdjust">Class to modify date time for timezones</param>
    /// <param name="destTimeZone">Name of the time zone datetime values that have a source time zone should be converted to</param>
    /// <param name="allowPercentage">Allow percentage symbols and adjust read value accordingly 25% is .25</param>
    /// <param name="removeCurrency">Read numeric values even if it contains a currency symbol, the symbol is lost though</param>
    public JsonFileReader(
      in Stream stream,
      in IEnumerable<Column>? columnDefinition,
      long recordLimit,
      bool trim,
      in string treatTextAsNull,
      bool treatNbspAsSpace,
      in TimeZoneChangeDelegate timeZoneAdjust,
      string destTimeZone,
      bool allowPercentage,
      bool removeCurrency)
      : base(string.Empty, columnDefinition, recordLimit, trim, treatTextAsNull, treatNbspAsSpace, timeZoneAdjust, destTimeZone, allowPercentage, removeCurrency) =>
      m_Stream = stream;
    
    /// <summary>
    /// Constructor for Json Reader
    /// </summary>
    /// <param name="fileName">Path to a physical file (if used)</param>
    /// <param name="columnDefinition">List of column definitions</param>
    /// <param name="recordLimit">Number of records that should be read</param>
    /// <param name="trim">Trim all read text</param>
    /// <param name="treatTextAsNull">Value to be replaced with NULL in Text</param>
    /// <param name="treatNbspAsSpace">nbsp in text will be replaced with Space</param>
    /// <param name="timeZoneAdjust">Class to modify date time for timezones</param>
    /// <param name="destTimeZone">Name of the time zone datetime values that have a source time zone should be converted to</param>
    /// <param name="allowPercentage">Allow percentage symbols and adjust read value accordingly 25% is .25</param>
    /// <param name="removeCurrency">Read numeric values even if it contains a currency symbol, the symbol is lost though</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public JsonFileReader(in string fileName,
      in IEnumerable<Column>? columnDefinition,
      long recordLimit,
      bool trim,
      string treatTextAsNull,
      bool treatNbspAsSpace,
      in TimeZoneChangeDelegate timeZoneAdjust,
      string destTimeZone,
      bool allowPercentage,
      bool removeCurrency)
      : base(fileName, columnDefinition, recordLimit, trim, treatTextAsNull, treatNbspAsSpace, timeZoneAdjust, destTimeZone, allowPercentage, removeCurrency)
    {
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentException("File can not be null or empty", nameof(fileName));
      if (!FileSystemUtils.FileExists(fileName))
        throw new FileNotFoundException(
          $"The file '{fileName.GetShortDisplayFileName()}' does not exist or is not accessible.",
          fileName);
    }

    /// <inheritdoc />
    public override bool IsClosed => m_StreamReader is null;

    /// <inheritdoc />
    public override void Close()
    {
      base.Close();
      m_JsonTextReader?.Close();
      (m_JsonTextReader as IDisposable)?.Dispose();
      m_JsonTextReader = null;

      m_StreamReader?.Dispose();
      m_StreamReader = null;
      if (!SelfOpenedStream) return;
      m_Stream?.Dispose();
      m_Stream = null;
    }

    /// <inheritdoc />
    public new void Dispose() => Dispose(true);

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    public new async ValueTask DisposeAsync()
    {
      if (m_Stream != null)
        await m_Stream.DisposeAsync().ConfigureAwait(false);
      Dispose(false);
    }
#endif

    /// <inheritdoc cref="IFileReader" />
    public override async Task OpenAsync(CancellationToken token)
    {
      HandleShowProgress($"Opening JSON file {FileName}", 0);
      await BeforeOpenAsync($"Opening JSON file {FileName.GetShortDisplayFileName()}")
        .ConfigureAwait(false);
      Retry:
      try
      {
        ResetPositionToStartOrOpen();
        var line = await GetNextRecordAsync(token).ConfigureAwait(false);

        // get column names for some time
        var colNames = new Dictionary<string, DataTypeEnum>();
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        // read additional rows to see if we have some extra columns
        while (line.Count != 0)
        {
          foreach (var keyValue in line)
            if (!colNames.ContainsKey(keyValue.Key))
              colNames.Add(keyValue.Key, keyValue.Value?.GetType().GetDataType() ?? DataTypeEnum.String);

          if (stopwatch.ElapsedMilliseconds > 1000)
            break;
          line = await GetNextRecordAsync(token).ConfigureAwait(false);
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
    }

    /// <inheritdoc cref="IFileReader" />
    public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
      if (!EndOfFile && !cancellationToken.IsCancellationRequested)
      {
        var couldRead = (await GetNextRecordAsync(cancellationToken).ConfigureAwait(false)).Count>0;
        if (couldRead) RecordNumber++;
        InfoDisplay(couldRead);

        if (couldRead && !IsClosed && RecordNumber <= RecordLimit)
          return true;
      }

      EndOfFile = true;
      HandleReadFinished();
      return false;
    }

    /// <inheritdoc cref="IFileReader" />
    public override void ResetPositionToFirstDataRow() => ResetPositionToStartOrOpen();

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (disposing) m_Stream?.Dispose();

      m_StreamReader?.Dispose();
      (m_JsonTextReader as IDisposable)?.Dispose();
      m_JsonTextReader = null;
      m_Stream = null;
    }

    /// <inheritdoc />
    protected override double GetRelativePosition()
    {
      if (m_Stream is IImprovedStream imp)
        return imp.Percentage;

      return base.GetRelativePosition();
    }

    /// <summary>
    ///   Reads a data row from the JsonTextReader and stores the values and text, this will flatten
    ///   the structure of the Json file
    /// </summary>
    /// <returns>A collection with name and value of the properties</returns>
    private async Task<IReadOnlyCollection<KeyValuePair<string, object?>>> GetNextRecordAsync(CancellationToken token)
    {
      if (m_JsonTextReader is null)
        throw new FileReaderOpenException();
      try
      {
        var headers = new Dictionary<string, bool>();
        var keyValuePairs = new Dictionary<string, object?>();
        while (m_JsonTextReader.TokenType != JsonToken.StartObject
               // && m_JsonTextReader.TokenType != JsonToken.PropertyName
               && m_JsonTextReader.TokenType != JsonToken.StartArray)
          if (!await m_JsonTextReader.ReadAsync(token).ConfigureAwait(false))
            return Array.Empty<KeyValuePair<string, object?>>();

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
              // in case there is a property it's a real column, otherwise it's used for structuring only
              headers[key] = true;

              // in case we are in an array combine all values but separate them with linefeed
              if (inArray && keyValuePairs[key] != null)
                keyValuePairs[key] = (Convert.ToString(keyValuePairs[key])) + '\n'
                                                                            + m_JsonTextReader.Value;
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
              throw new ArgumentOutOfRangeException($"Unknown TokenType {m_JsonTextReader.TokenType}");
          }

          token.ThrowIfCancellationRequested();
        } while (!(m_JsonTextReader.TokenType == JsonToken.EndObject && startKey == endKey) && await m_JsonTextReader.ReadAsync(token).ConfigureAwait(false));

        EndLineNumber = m_JsonTextReader.LineNumber;

        foreach (var kv in headers.Where(kv => !kv.Value))
          keyValuePairs.Remove(kv.Key);

        // store the information into our fixed structure, even if the tokens in Json change order
        // they will align
        if (Column.Length == 0) return keyValuePairs;
        var columnNumber = 0;
        foreach (var col in Column)
        {
          if (col.Ignore)
            continue;
          if (keyValuePairs.TryGetValue(col.Name, out CurrentValues[columnNumber]))
          {
            if (CurrentValues[columnNumber] is null)
            {
              CurrentRowColumnText[columnNumber] = string.Empty;
            }
            else
            {
              // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
              var orgVal = Convert.ToString(CurrentValues[columnNumber]) ?? string.Empty;
              CurrentRowColumnText[columnNumber] = orgVal;

              // ReSharper disable once MergeIntoPattern
              if (!string.IsNullOrEmpty(orgVal) && col.ValueFormat.DataType >= DataTypeEnum.String)
              {
                CurrentRowColumnText[columnNumber] = TreatNbspTestAsNullTrim(HandleTextSpecials(orgVal.AsSpan(), columnNumber));
                CurrentValues[columnNumber] = CurrentRowColumnText[columnNumber];
              }
            }
          }

          columnNumber++;
        }

        if (keyValuePairs.Count < FieldCount)
          HandleWarning(
            -1,
            $"Line {StartLineNumber} has fewer columns than expected ({keyValuePairs.Count}/{FieldCount}).");
        else if (keyValuePairs.Count > FieldCount)
          HandleWarning(
            -1,
            $"Line {StartLineNumber} has more columns than expected ({keyValuePairs.Count}/{FieldCount}). The data in extra columns is not read.");

        return keyValuePairs;
      }
      catch (Exception ex)
      {
        // A serious error will be logged and its assume the file is ended
        HandleError(-1, ex.Message);
        EndOfFile = true;
        return  Array.Empty<KeyValuePair<string, object?>>();
      }
    }

    /// <summary>
    ///   Resets the position and buffer to the first line, excluding headers, use
    ///   ResetPositionToStart if you want to go to first data line
    /// </summary>
    private void ResetPositionToStartOrOpen()
    {
      if (SelfOpenedStream)
      {
        // Better would be DisposeAsync(), but method is synchronous
        m_Stream?.Dispose();
        m_Stream =  FunctionalDI.GetStream(new SourceAccess(FullPath));
      }
      else
      {
        m_Stream!.Seek(0, SeekOrigin.Begin);
      }

      // in case we can not seek need to reopen the stream reader
      m_StreamReader?.Close();
      m_StreamReader = new StreamReader(
        m_Stream ?? throw new InvalidOperationException(),
        Encoding.UTF8,
        true,
        4096,
        true);

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