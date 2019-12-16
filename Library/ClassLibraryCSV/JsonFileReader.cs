using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace CsvTools
{
  public class JsonFileReader : BaseFileReaderTyped, IFileReader
  {
    private readonly ICsvFile m_StructuredFile;
    private ImprovedStream m_ImprovedStream;
    private StreamReader m_TextReader;
    private JsonTextReader m_JsonTextReader;
    private bool m_DisposedValue;

    public JsonFileReader(ICsvFile fileSetting, IProcessDisplay processDisplay)
    : base(fileSetting, processDisplay) => m_StructuredFile = fileSetting;

    /// <summary>
    ///  Gets a value indicating whether this instance is closed.
    /// </summary>
    /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
    public bool IsClosed => m_TextReader == null;

    public override void Close()
    {
      m_JsonTextReader?.Close();
      m_TextReader?.Dispose();
      m_ImprovedStream?.Dispose();

      base.Close();
    }

    /// <summary>
    ///  Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing">
    ///  <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
    ///  unmanaged resources.
    /// </param>
    public override void Dispose(bool disposing)
    {
      if (!m_DisposedValue)
      {
        // Dispose-time code should also set references of all owned objects to null, after disposing
        // them. This will allow the referenced objects to be garbage collected even if not all
        // references to the "parent" are released. It may be a significant memory consumption win if
        // the referenced objects are large, such as big arrays, collections, etc.
        if (!disposing) return;
        Close();
        if (m_TextReader != null)
        {
          m_TextReader.Dispose();
          m_TextReader = null;
        }
        if (m_JsonTextReader != null)
        {
          m_JsonTextReader.Close();
          m_JsonTextReader = null;
        }
        base.Dispose(true);
        m_DisposedValue = true;
      }
    }

    /// <summary>
    /// Reads a data row from the JsonTextReader and stores the values and text, this will flatten the structure of the Json file
    /// </summary>
    /// <returns>A list of the name of the properties / columns</returns>
    public IList<string> GetNextRecord()
    {
      for (var colNum = 0; colNum < FieldCount; colNum++)
      {
        CurrentValues[colNum] = null;
        CurrentRowColumnText[colNum] = string.Empty;
      }

      var headers = new Dictionary<string, bool>();
      var keyValuePairs = new Dictionary<string, object>();

      // we need the root property
      while (m_JsonTextReader.TokenType != JsonToken.StartObject && m_JsonTextReader.TokenType != JsonToken.PropertyName && m_JsonTextReader.TokenType != JsonToken.StartArray)
      {
        if (!m_JsonTextReader.Read())
          return null;
      }
      while (m_JsonTextReader.TokenType != JsonToken.PropertyName)
      {
        if (!m_JsonTextReader.Read())
          return null;
      }
      // sore the parent Property Name in parentKey
      var parentKey = string.Empty;

      // sore the current Property Name in key
      var key = m_JsonTextReader.Value.ToString().Replace('.', '_');
      if (!m_JsonTextReader.Read())
        return null;

      StartLineNumber = m_JsonTextReader.LineNumber;
      var inArray = false;
      do
      {
        switch (m_JsonTextReader.TokenType)
        {
          // either the start of the row or a sub object that will be flattened
          case JsonToken.StartObject:
            if (string.IsNullOrEmpty(key))
              parentKey = key;

            break;

          case JsonToken.EndObject:
            if (parentKey.IndexOf('.') != -1)
              parentKey = parentKey.Substring(0, parentKey.IndexOf('.') - 1);
            break;

          // arrays will be read as multi line columns
          case JsonToken.StartArray:
            inArray = true;
            break;

          case JsonToken.PropertyName:
            // since we use . to deal with nested objects make sure this char is not part of the name
            if (string.IsNullOrEmpty(parentKey))
              key = m_JsonTextReader.Value.ToString().Replace('.', '_');
            else
              key = parentKey + "." + m_JsonTextReader.Value.ToString().Replace('.', '_');

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
            {
              keyValuePairs[key] = keyValuePairs[key].ToString() + '\n' + m_JsonTextReader.Value.ToString();
            }
            else
            {
              keyValuePairs[key] = m_JsonTextReader.Value;
            }
            break;

          case JsonToken.EndArray:
            inArray = false;
            break;
        }
        CancellationToken.ThrowIfCancellationRequested();
      } while (!(m_JsonTextReader.TokenType == JsonToken.EndObject && parentKey.Length == 0)
              && m_JsonTextReader.Read());

      EndLineNumber = m_JsonTextReader.LineNumber;
      RecordNumber++;

      var realHeaders = new List<string>();
      foreach (var kv in headers)
      {
        if (kv.Value)
          realHeaders.Add(kv.Key);
      }

      // store the information into our fixed structure, even if the tokens in Json change order they will aligned
      if (Column!=null && Column.Length != 0)
      {
        var colNum = 0;
        foreach (var col in Column)
        {
          if (keyValuePairs.TryGetValue(col.Name, out CurrentValues[colNum]))
          {
            if (CurrentValues[colNum] != null)
              CurrentRowColumnText[colNum] = CurrentValues[colNum].ToString();
          }
          colNum++;
        }

        if (realHeaders.Count < FieldCount)
        {
          HandleWarning(-1, $"Line {StartLineNumber} has fewer columns than expected ({keyValuePairs.Count}/{FieldCount}).");
        }
        else if (realHeaders.Count > FieldCount)
        {
          HandleWarning(-1, $"Line {StartLineNumber} has more columns than expected ({keyValuePairs.Count}/{FieldCount}). The data in extra columns is not read.");
        }
      }

      return realHeaders;
    }

    public void Open()
    {
      try
      {
        HandleShowProgress("Opening Json file…");

        ResetPositionToStartOrOpen();

        var line = GetNextRecord();

        base.InitColumn(line.Count);
        ParseColumnName(line);

        var colType = GetColumnType(row => (GetNextRecord() != null));

        // Read the types of the first row
        for (var counter = 0; counter < FieldCount; counter++)
          GetColumn(counter).DataType = colType[counter];

        base.FinishOpen();

        ResetPositionToStartOrOpen();
      }
      catch (Exception ex)
      {
        Close();
        var appEx = new FileReaderException("Error opening structured text file for reading.\nPlease make sure the file does exist, is of the right type and is not locked by another process.", ex);
        HandleError(-1, appEx.ExceptionMessages());
        HandleReadFinished();
        throw appEx;
      }
      finally
      {
        HandleShowProgress("");
      }
    }

    /// <summary>
    ///  Advances to the next record.
    /// </summary>
    /// <returns>true if there are more rows; otherwise, false.</returns>
    public override bool Read()
    {
      if (!CancellationToken.IsCancellationRequested)
      {
        var couldRead = (GetNextRecord() != null);
        InfoDisplay(couldRead);

        if (couldRead && !IsClosed)
          return true;
      }
      HandleReadFinished();
      return false;
    }

    public void ResetPositionToFirstDataRow() => ResetPositionToStartOrOpen();

    /// <summary>
    ///  Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and MaxValue</returns>
    protected override int GetRelativePosition()
    {
      // if we know how many records to read, use that
      if (m_StructuredFile.RecordLimit > 0)
        return (int)(RecordNumber / m_StructuredFile.RecordLimit * cMaxValue);

      return (int)(m_ImprovedStream.Percentage * cMaxValue);
    }

    /// <summary>
    ///  Resets the position and buffer to the first line, excluding headers, use ResetPositionToStart if you want to go to
    ///  first data line
    /// </summary>
    private void ResetPositionToStartOrOpen()
    {
      m_JsonTextReader?.Close();
      m_TextReader?.Dispose();
      m_TextReader = null;
      /*m_ImprovedStream?.Dispose();

      m_ImprovedStream = ImprovedStream.OpenRead(m_StructuredFile);
        */
      if (m_ImprovedStream == null)
        m_ImprovedStream = ImprovedStream.OpenRead(m_StructuredFile);

      m_ImprovedStream.ResetToStart(delegate (Stream str)
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

      EndOfFile = m_TextReader.EndOfStream;
      m_JsonTextReader = new JsonTextReader(m_TextReader)
      {
        CloseInput = false
      };
    }
  }
}