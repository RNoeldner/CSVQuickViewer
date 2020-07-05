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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace CsvTools
{
  /// <summary>
  ///   Json text file reader
  /// </summary>
  public class JsonFileReader : BaseFileReaderTyped, IFileReader
  {
    private readonly bool m_TreatNBSPAsSpace;
    private readonly string m_TreatTextAsNull;
    private readonly TrimmingOption m_TrimmingOption;
    private bool m_AssumeLog;
    private IImprovedStream m_ImprovedStream;
    private JsonTextReader m_JsonTextReader;
    private StreamReader m_TextReader;
    private long m_TextReaderLine;


    public JsonFileReader([NotNull] string fullPath,
      [CanBeNull] IEnumerable<IColumn> columnDefinition = null,
      long recordLimit = 0,
      bool treatNBSPAsSpace = false, TrimmingOption trimmingOption = TrimmingOption.None,
      string treatTextAsNull = null) :
      base(fullPath, columnDefinition, recordLimit)
    {
      if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));
      m_TreatNBSPAsSpace = treatNBSPAsSpace;
      m_TrimmingOption = trimmingOption;
      m_TreatTextAsNull = treatTextAsNull;
    }

    public JsonFileReader(IFileSettingPhysicalFile fileSetting,
      IProcessDisplay processDisplay)
      : this(fileSetting.FullPath,
        fileSetting.ColumnCollection, fileSetting.RecordLimit,
        fileSetting.TreatNBSPAsSpace)
    {
      if (processDisplay == null) return;
      ReportProgress = processDisplay.SetProcess;
      if (processDisplay is IProcessDisplayTime processDisplayTime)
      {
        SetMaxProcess = l => processDisplayTime.Maximum = l;
        SetMaxProcess(0);
      }
    }

    /// <summary>
    ///   Gets a value indicating whether this instance is closed.
    /// </summary>
    /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
    public virtual bool IsClosed => m_TextReader == null;

    public override void Close()
    {
      base.Close();

      m_JsonTextReader?.Close();
      ((IDisposable) m_JsonTextReader)?.Dispose();
      m_TextReader?.Dispose();
      m_ImprovedStream?.Dispose();

      m_JsonTextReader = null;
      m_TextReader = null;
      m_TextReader = null;
    }

    public override async Task OpenAsync(CancellationToken token)
    {
      await BeforeOpenAsync(
          $"Opening Json file {FileSystemUtils.GetShortDisplayFileName(FileName, 80)}")
        .ConfigureAwait(false);
      Retry:
      try
      {
        m_AssumeLog = false;
        again:
        ResetPositionToStartOrOpen();

        var line = await GetNextRecordAsync(false, token).ConfigureAwait(false);
        try
        {
          await GetNextRecordAsync(true, token).ConfigureAwait(false);
        }
        catch (JsonReaderException ex)
        {
          Logger.Warning(ex, "Issue reading the JSon file, trying to read it as JSon Log output");
          m_AssumeLog = true;
          goto again;
        }

        // read additional 50 rows to see if we have some extra columns
        for (var row = 1; row < 50; row++)
        {
          var line2 = await GetNextRecordAsync(false, token).ConfigureAwait(false);
          if (line2 == null)
            break;
          if (line2.Count > line.Count)
            line = line2;
        }

        InitColumn(line.Count);
        ParseColumnName(line.Select(colValue => colValue.Key),
          line.Select(colValue => colValue.Value?.GetType().GetDataType() ?? DataType.String));

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

    public override async Task<bool> ReadAsync(CancellationToken token)
    {
      if (!EndOfFile && !token.IsCancellationRequested)
      {
        var couldRead = await GetNextRecordAsync(false, token).ConfigureAwait(false) != null;
        if (couldRead) RecordNumber++;
        InfoDisplay(couldRead);

        if (couldRead && !IsClosed && RecordNumber <= RecordLimit)
          return true;
      }

      EndOfFile = true;
      HandleReadFinished();
      return false;
    }

    public override async Task ResetPositionToFirstDataRowAsync(CancellationToken token) =>
      await Task.Run(ResetPositionToStartOrOpen, token);

    public void Dispose() => Close();

    /// <summary>
    ///   Reads a data row from the JsonTextReader and stores the values and text, this will flatten
    ///   the structure of the Json file
    /// </summary>
    /// <returns>A collection with name and value of the properties</returns>
    private async Task<ICollection<KeyValuePair<string, object>>> GetNextRecordAsync(bool throwError,
      CancellationToken token)
    {
      try
      {
        if (m_AssumeLog)
        {
          SetTextReader();
          StartLineNumber = m_TextReaderLine;
        }

        var headers = new Dictionary<string, bool>();
        var keyValuePairs = new Dictionary<string, object>();
        while (m_JsonTextReader.TokenType != JsonToken.StartObject
               // && m_JsonTextReader.TokenType != JsonToken.PropertyName
               && m_JsonTextReader.TokenType != JsonToken.StartArray)
          if (!await m_JsonTextReader.ReadAsync(token).ConfigureAwait(false))
            return null;

        // sore the parent Property Name in parentKey
        var startKey = string.Empty;
        var endKey = "<dummy>";
        var key = string.Empty;

        // sore the current Property Name in key
        if (!m_AssumeLog)
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
                 && await m_JsonTextReader.ReadAsync(token).ConfigureAwait(false));

        EndLineNumber = !m_AssumeLog ? m_JsonTextReader.LineNumber : m_TextReaderLine;

        foreach (var kv in headers.Where(kv => !kv.Value))
          keyValuePairs.Remove(kv.Key);

        // store the information into our fixed structure, even if the tokens in Json change order
        // they will aligned
        if (Column == null || Column.Length == 0) return keyValuePairs;
        var columnNumber = 0;
        foreach (var col in Column)
        {
          if (keyValuePairs.TryGetValue(col.Name, out CurrentValues[columnNumber]))
            if (CurrentValues[columnNumber] != null)
              CurrentRowColumnText[columnNumber] = HandleText(CurrentValues[columnNumber].ToString(), columnNumber,
                m_TreatNBSPAsSpace, m_TreatTextAsNull, m_TrimmingOption);
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
    protected override int GetRelativePosition()
    {
      // if we know how many records to read, use that
      if (RecordLimit > 0 && RecordLimit < long.MaxValue)
        return base.GetRelativePosition();

      return (int) (m_ImprovedStream.Percentage * cMaxValue);
    }

    /// <summary>
    ///   Resets the position and buffer to the first line, excluding headers, use
    ///   ResetPositionToStart if you want to go to first data line
    /// </summary>
    private void ResetPositionToStartOrOpen()
    {
      m_JsonTextReader?.Close();
      m_TextReader?.Dispose();
      m_TextReader = null;

      if (m_ImprovedStream == null)
        m_ImprovedStream = FunctionalDI.OpenRead(FullPath);

      m_ImprovedStream.ResetToStart(delegate(Stream str)
      {
        // in case we can not seek need to reopen the stream reader
        if (!str.CanSeek || m_TextReader == null)
        {
          m_JsonTextReader?.Close();
          m_TextReader?.Dispose();
          m_TextReader = new StreamReader(str);
        }
        else
        {
          m_JsonTextReader?.Close();
          m_TextReader.BaseStream.Seek(0, SeekOrigin.Begin);
          // only discard the buffer
          m_TextReader.DiscardBufferedData();
        }
      });

      // End Line should be at 1, later on as the line is read the start line s set to this value
      StartLineNumber = 1;
      EndLineNumber = 1;
      RecordNumber = 0;

      m_TextReaderLine = 1;
      m_BufferPos = 0;

      EndOfFile = m_TextReader.EndOfStream;
      m_JsonTextReader = new JsonTextReader(m_TextReader)
      {
        CloseInput = false
      };
    }

#region TextReader

    // Buffer size set to 64kB, if set to large the display in percentage will jump
    private const int c_BufferSize = 65536;

    /// <summary>
    ///   16k Buffer of the file data
    /// </summary>
    private readonly char[] m_Buffer = new char[c_BufferSize];

    /// <summary>
    ///   Length of the buffer (can be smaller then buffer size at end of file)
    /// </summary>
    private int m_BufferFilled;

    /// <summary>
    ///   Position in the buffer
    /// </summary>
    private int m_BufferPos = -1;

    /// <summary>
    ///   The line-feed character. Escape code is <c>\n</c>.
    /// </summary>
    private const char c_Lf = (char) 0x0a;

    /// <summary>
    ///   The carriage return character. Escape code is <c>\r</c>.
    /// </summary>
    private const char c_Cr = (char) 0x0d;

    /// <summary>
    ///   Fills the buffer with data from the reader.
    /// </summary>
    /// <returns><c>true</c> if data was successfully read; otherwise, <c>false</c>.</returns>
    private void ReadIntoBuffer()
    {
      if (EndOfFile)
        return;
      m_BufferFilled = m_TextReader.Read(m_Buffer, 0, c_BufferSize);
      EndOfFile |= m_BufferFilled == 0;
      // Handle double decoding
      if (m_BufferFilled <= 0)
        return;
      var result = Encoding.UTF8.GetChars(Encoding.GetEncoding(28591).GetBytes(m_Buffer, 0, m_BufferFilled));
      // The twice decode text is smaller
      m_BufferFilled = result.GetLength(0);
      result.CopyTo(m_Buffer, 0);
    }

    private char NextChar()
    {
      if (m_BufferPos < m_BufferFilled && m_BufferFilled != 0) return EndOfFile ? c_Lf : m_Buffer[m_BufferPos];
      ReadIntoBuffer();
      m_BufferPos = 0;

      // If of file its does not matter what to return simply return something
      return EndOfFile ? c_Lf : m_Buffer[m_BufferPos];
    }

    private void EatNextCRLF(char character)
    {
      m_TextReaderLine++;
      if (EndOfFile) return;
      var nextChar = NextChar();
      if ((character != c_Cr || nextChar != c_Lf) && (character != c_Lf || nextChar != c_Cr)) return;
      // New line sequence is either CRLF or LFCR, disregard the character
      m_BufferPos++;
      // Very special a LF CR is counted as two lines.
      if (character == c_Lf && nextChar == c_Cr)
        m_TextReaderLine++;
    }

    private void SetTextReader()
    {
      // find the beginning
      while (!EndOfFile)
      {
        var peek = NextChar();
        if (peek == '{' || peek == '[')
          break;
        m_BufferPos++;
        if (peek == c_Cr || peek == c_Lf)
          EatNextCRLF(peek);
      }

      // get a Serialized Jason object it starts with { and ends with }
      var openCurly = 0;
      var openSquare = 0;
      var sb = new StringBuilder();
      while (!EndOfFile)
      {
        var chr = NextChar();
        m_BufferPos++;
        sb.Append(chr);
        switch (chr)
        {
          case '{':
            openCurly++;
            break;

          case '}':
            openCurly--;
            break;

          case '[':
            openSquare++;
            break;

          case ']':
            openSquare--;
            break;

          case c_Cr:
          case c_Lf:
            EatNextCRLF(chr);
            break;
        }

        if (openCurly == 0 && openSquare == 0)
          break;
      }

      m_JsonTextReader = new JsonTextReader(new StringReader(sb.ToString()));
    }

#endregion TextReader
  }
}