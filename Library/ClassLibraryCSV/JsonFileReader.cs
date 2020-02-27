using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace CsvTools
{
	public class JsonFileReader : BaseFileReaderTyped, IFileReader
	{
		private readonly ICsvFile m_StructuredFile;
		private bool m_AssumeLog;
		private bool m_DisposedValue;
		private ImprovedStream m_ImprovedStream;
		private JsonTextReader m_JsonTextReader;
		private StreamReader m_TextReader;
		private long m_TextReaderLine;

		public JsonFileReader(ICsvFile fileSetting, IProcessDisplay processDisplay)
			: base(fileSetting, processDisplay) => m_StructuredFile = fileSetting;

		/// <summary>
		///   Gets a value indicating whether this instance is closed.
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

		public void Open()
		{
			Retry:

			try
			{
				HandleShowProgress("Opening Json file…");
				m_AssumeLog = false;
				ResetPositionToStartOrOpen();

				var line = GetNextRecord(false);
				try
				{
					GetNextRecord(true);
				}
				catch (JsonReaderException ex)
				{
					Logger.Warning(ex, "Issue reading the JSon file, trying to read it as JSon Log output");
					m_AssumeLog = true;
					ResetPositionToStartOrOpen();
				}

				// need to call InitColumn to set the Field Count and initialize all array
				base.InitColumn(line.Count);
				var header = new List<string>();
				foreach (var colValue in line)
					header.Add(colValue.Key);
				ParseColumnName(header);

				// Set CurrentValues as it has been created now
				var col = 0;
				foreach (var colValue in line)
					CurrentValues[col++] = colValue.Value;
				var colType = GetColumnType();

				// Read the types of the first row
				for (var counter = 0; counter < FieldCount; counter++)
					GetColumn(counter).DataType = colType[counter];

				base.FinishOpen();

				ResetPositionToStartOrOpen();
			}
			catch (Exception ex)
			{
				if (ShouldRetry(ex))
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

		/// <summary>
		///   Advances to the next record.
		/// </summary>
		/// <returns>true if there are more rows; otherwise, false.</returns>
		public override bool Read()
		{
			if (!CancellationToken.IsCancellationRequested)
			{
				var couldRead = GetNextRecord(false) != null;
				InfoDisplay(couldRead);

				if (couldRead && !IsClosed)
					return true;
			}

			HandleReadFinished();
			return false;
		}

		public void ResetPositionToFirstDataRow() => ResetPositionToStartOrOpen();

		/// <summary>
		///   Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing">
		///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
		///   unmanaged resources.
		/// </param>
		protected override void Dispose(bool disposing)
		{
			if (m_DisposedValue) return;
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

		/// <summary>
		///   Reads a data row from the JsonTextReader and stores the values and text,
		///   this will flatten the structure of the Json file
		/// </summary>
		/// <returns>A collection with name and value of the properties</returns>
		private ICollection<KeyValuePair<string, object>> GetNextRecord(bool throwError)
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
					if (!m_JsonTextReader.Read())
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
					}

					CancellationToken.ThrowIfCancellationRequested();
				} while (!(m_JsonTextReader.TokenType == JsonToken.EndObject && startKey == endKey)
				         && m_JsonTextReader.Read());

				EndLineNumber = !m_AssumeLog ? m_JsonTextReader.LineNumber : m_TextReaderLine;
				RecordNumber++;

				foreach (var kv in headers)
					if (!kv.Value)
						keyValuePairs.Remove(kv.Key);

				// store the information into our fixed structure, even if the tokens in Json change order they will aligned
				if (Column == null || Column.Length == 0) return keyValuePairs;
				var colNum = 0;
				foreach (var col in Column)
				{
					if (keyValuePairs.TryGetValue(col.Name, out CurrentValues[colNum]))
						if (CurrentValues[colNum] != null)
							CurrentRowColumnText[colNum] = CurrentValues[colNum].ToString();
					colNum++;
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
			if (m_StructuredFile.RecordLimit > 0)
				return (int) (RecordNumber / m_StructuredFile.RecordLimit * cMaxValue);

			return (int) (m_ImprovedStream.Percentage * cMaxValue);
		}

		/// <summary>
		///   Resets the position and buffer to the first line, excluding headers, use ResetPositionToStart if you want to go to
		///   first data line
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
			if (m_BufferPos >= m_BufferFilled || m_BufferFilled == 0)
			{
				ReadIntoBuffer();
				m_BufferPos = 0;
			}

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

#endregion
	}
}