/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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

namespace CsvTools;

/// <inheritdoc cref="CsvTools.IFileReader" />
/// <summary>
///   Json text file reader, this reader is a synchronous reader
/// </summary>
public sealed class JsonFileReader : BaseFileReaderTyped
{
  private Stream? m_Stream;
  private readonly StreamProviderDelegate m_StreamProvider;

  private IEnumerator<JObject>? m_EnumeratorJson = null;
  // Storage for already read columns 
  private readonly List<JObject> m_SampleRows = new List<JObject>();
  // Need to keep the StreamReader so its not disposed in between
  private StreamReader? m_StreamReader;
  private IReadOnlyList<JsonTabularConverter.JsonColumn> m_JsonColumns = Array.Empty<JsonTabularConverter.JsonColumn>();

  /// <summary>
  /// Constructor for Json Reader
  /// </summary>
  /// <param name="stream">Stream to read from</param>
  /// <param name="columnDefinition">List of column definitions</param>
  /// <param name="recordLimit">Number of records that should be read</param>
  /// <param name="trim">Trim all read text</param>
  /// <param name="treatTextAsNull">Value to be replaced with NULL in Text</param>
  /// <param name="treatNbspAsSpace">nbsp in text will be replaced with Space</param>
  /// <param name="returnedTimeZone">Name of the time zone datetime values that have a source time zone should be converted to</param>
  /// <param name="allowPercentage">Allow percentage symbols and adjust read value accordingly 25% is .25</param>
  /// <param name="removeCurrency">Read numeric values even if it contains a currency symbol, the symbol is lost though</param>
  public JsonFileReader(
    in Stream stream,
    in IEnumerable<Column>? columnDefinition,
    long recordLimit,
    bool trim,
    string treatTextAsNull,
    bool treatNbspAsSpace,
    string returnedTimeZone,
    bool allowPercentage,
    bool removeCurrency)
    : base(string.Empty, columnDefinition, recordLimit, trim, treatTextAsNull, treatNbspAsSpace, returnedTimeZone, allowPercentage, removeCurrency)
  {
    m_Stream = stream;
    m_StreamProvider= FunctionalDI.GetStream;
  }

  /// <summary>
  /// Constructor for Json Reader
  /// </summary>
  /// <param name="fileName">Path to a physical file (if used)</param>
  /// <param name="columnDefinition">List of column definitions</param>
  /// <param name="recordLimit">Number of records that should be read</param>
  /// <param name="trim">Trim all read text</param>
  /// <param name="treatTextAsNull">Value to be replaced with NULL in Text</param>
  /// <param name="treatNbspAsSpace">nbsp in text will be replaced with Space</param>  
  /// <param name="returnedTimeZone">Name of the time zone datetime values that have a source time zone should be converted to</param>
  /// <param name="allowPercentage">Allow percentage symbols and adjust read value accordingly 25% is .25</param>
  /// <param name="removeCurrency">Read numeric values even if it contains a currency symbol, the symbol is lost though</param>
  /// <exception cref="ArgumentException"></exception>
  /// <exception cref="FileNotFoundException"></exception>
  public JsonFileReader(string fileName,
    in IEnumerable<Column>? columnDefinition = null,
    long recordLimit = 0,
    bool trim = false,
    string treatTextAsNull = "null",
    bool treatNbspAsSpace = false,
    string returnedTimeZone = "",
    bool allowPercentage = true,
    bool removeCurrency = true)
    : base(fileName, columnDefinition, recordLimit, trim, treatTextAsNull, treatNbspAsSpace, returnedTimeZone, allowPercentage, removeCurrency)
  {
    if (string.IsNullOrEmpty(fileName))
      throw new ArgumentException("File can not be null or empty", nameof(fileName));
    if (!FileSystemUtils.FileExists(fileName))
      throw new FileNotFoundException(
        $"The file '{fileName.GetShortDisplayFileName()}' does not exist or is not accessible.",
        fileName);
    m_StreamProvider= FunctionalDI.GetStream;
  }

  /// <inheritdoc />
  public override bool IsClosed => m_Stream is null;

  /// <inheritdoc />
  public override void Close()
  {
    base.Close();

    m_StreamReader?.Dispose();
    m_StreamReader = null;
    m_EnumeratorJson?.Dispose();
    m_EnumeratorJson = null;

    if (!SelfOpenedStream) return;
    m_Stream?.Dispose();
    m_Stream = null;
  }


#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
  /// <inheritdoc cref="IFileReader" />
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
    // Make sure to free old resources
    Close();

    await BeforeOpenAsync($"Opening JSON file {FileName.GetShortDisplayFileName()}")
      .ConfigureAwait(false);
    Retry:
    try
    {
      ResetPositionToStartOrOpen();
      if (m_EnumeratorJson is null)
        throw new InvalidOperationException("JSON enumerator not initialized.");

      // Discover columns from first N rows
      for (int i = 0; i < 5 && m_EnumeratorJson.MoveNext(); i++)
        m_SampleRows.Add(m_EnumeratorJson.Current);
      m_JsonColumns = m_SampleRows.DiscoverColumns(4, token);
      InitColumn(m_JsonColumns.Count);
      ParseColumnName(m_JsonColumns.Select(x => x.HeaderName), m_JsonColumns.Select(x => x.PropertyType.GetDataType()));

      FinishOpen();
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

  /// <inheritdoc cref="BaseFileReader" />
  protected sealed override ValueTask<bool> ReadCoreAsync(CancellationToken cancellationToken)
  {
      if (!EndOfFile && !cancellationToken.IsCancellationRequested && m_EnumeratorJson !=null)
    {
      JObject? json = null;
      if (RecordNumber < m_SampleRows.Count)
        json = m_SampleRows[(int) RecordNumber];
      else if (m_EnumeratorJson.MoveNext())
        json = m_EnumeratorJson.Current;

      bool couldRead = json != null;
      if (couldRead)
      {
        RecordNumber++;
        EndLineNumber++;
        StartLineNumber++;
        json!.HandleRow(m_JsonColumns, ',', (idx, val) => CurrentRowColumnText[idx]= val, (idx, val) => CurrentValues[idx]= val);
      }

      InfoDisplay(couldRead);
      if (couldRead && !IsClosed && RecordNumber <= RecordLimit)
        return new ValueTask<bool>(true);
    }

    EndOfFile = true;
    HandleReadFinished();
    return new ValueTask<bool>(false);
  }

  /// <inheritdoc cref="IFileReader" />
  public override void ResetPositionToFirstDataRow() => ResetPositionToStartOrOpen();

  /// <inheritdoc />
  protected override void Dispose(bool disposing)
  {
    if (disposing)
    {
      m_Stream?.Dispose();
      m_StreamReader?.Dispose();
      m_EnumeratorJson?.Dispose();
      m_EnumeratorJson = null;
      m_StreamReader = null;
      m_Stream = null;
    }

    base.Dispose(disposing);
  }

  /// <inheritdoc />
  protected override double GetRelativePosition()
  {
    if (m_Stream is IImprovedStream imp)
      return imp.Percentage;

    return base.GetRelativePosition();
  }

  /// <summary>
  ///   Resets the position and buffer to the first line, excluding headers, use
  ///   ResetPositionToStart if you want to go to first data line
  /// </summary>
  private void ResetPositionToStartOrOpen()
  {
    if (SelfOpenedStream)
    {
      m_Stream?.Dispose();
      m_Stream = m_StreamProvider(new SourceAccess(FullPath));
    }
    else
    {
      m_Stream!.Seek(0, SeekOrigin.Begin);
    }

    EndLineNumber = 1;
    StartLineNumber = EndLineNumber;
    RecordNumber = 0;

    m_StreamReader = new StreamReader(m_Stream!, Encoding.UTF8, true, 4096, true);

    // Initialize the JsonTextReader for streaming
    m_EnumeratorJson = m_StreamReader.StreamJsonObjects().Items.GetEnumerator();
    m_SampleRows.Clear();
  }
}