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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CsvTools
{
	/// <summary>
	///   Helper class
	/// </summary>
	public static class CsvHelper
	{
		public static ICollection<string> GetColumnHeadersFromReader(IFileReader fileReader)
		{
			Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
			var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (fileReader == null)
				return values;
			for (var colIndex = 0; colIndex < fileReader.FieldCount; colIndex++)
			{
				var cf = fileReader.GetColumn(colIndex);
				if (!string.IsNullOrEmpty(cf.Name) && !cf.Ignore)
					values.Add(cf.Name);
			}

			return values;
		}

		/// <summary>
		///   Gets the column header of a file
		/// </summary>
		/// <param name="fileReader">a file reader</param>
		/// <param name="cancellationToken">a cancellation token</param>
		/// <returns>
		///   An array of string with the column headers where the column is empty
		/// </returns>
		public static ICollection<string> GetEmptyColumnHeader(IFileReader fileReader, CancellationToken cancellationToken)
		{
			Contract.Requires(fileReader != null);
			Contract.Ensures(Contract.Result<string[]>() != null);
			var emptyColumns = new List<string>();

			var needToCheck = new List<int>(fileReader.FieldCount);
			for (var column = 0; column < fileReader.FieldCount; column++)
				needToCheck.Add(column);

			while (fileReader.Read() && !cancellationToken.IsCancellationRequested && needToCheck.Count > 0)
			{
				var hasData = needToCheck.Where(col => !string.IsNullOrEmpty(fileReader.GetString(col))).ToList();

				foreach (var col in hasData)
					needToCheck.Remove(col);
			}

			for (var column = 0; column < fileReader.FieldCount; column++)
				if (needToCheck.Contains(column))
					emptyColumns.Add(fileReader.GetColumn(column).Name);

			return emptyColumns;
		}

		/// <summary>
		///   Gets the <see cref="Encoding" /> of the textFile
		/// </summary>
		/// <param name="setting">The setting.</param>
		/// <returns></returns>
		public static Encoding GetEncoding(this ICsvFile setting)
		{
			Contract.Requires(setting != null);
			if (setting.CodePageId < 0)
				GuessCodePage(setting);
			try
			{
				return Encoding.GetEncoding(setting.CodePageId);
			}
			catch (NotSupportedException)
			{
				Logger.Warning("Codepage {codepage} is not supported, using UTF8", setting.CodePageId);
				setting.CodePageId = 65001;
				return new UTF8Encoding(true);
			}
		}

		/// <summary>
		///   Guesses the code page ID of a file
		/// </summary>
		/// <param name="setting">The CSVFile fileSetting</param>
		/// <remarks>
		///   No Error will be thrown, the CodePage and the BOM will bet set
		/// </remarks>
		public static void GuessCodePage(ICsvFile setting)
		{
			Contract.Requires(setting != null);

			// Read 256 kBytes
			var buff = new byte[262144];
			int length;
			using (var fileStream = ImprovedStream.OpenRead(setting))
			{
				length = fileStream.Stream.Read(buff, 0, buff.Length);
			}

			if (length >= 2)
			{
				var byBom = EncodingHelper.GetCodePageByByteOrderMark(buff);
				if (byBom != 0)
				{
					setting.ByteOrderMark = true;
					setting.CodePageId = byBom;
					return;
				}
			}

			setting.ByteOrderMark = false;
			var detected = EncodingHelper.GuessCodePageNoBom(buff, length);

			// ASCII will be reported as UTF-8, UTF8 includes ASCII as subset
			if (detected == 20127)
				detected = 65001;
			Logger.Information("Detected Code Page: {codepage}",
				EncodingHelper.GetEncodingName(detected, true, setting.ByteOrderMark));
			setting.CodePageId = detected;
		}

		/// <summary>
		///   Guesses the delimiter for a files. Done with a rather simple csv parsing, and trying to find
		///   the delimiter that has the least variance in the read rows, if that is not possible the
		///   delimiter with the highest number of occurrences.
		/// </summary>
		/// <param name="setting">The CSVFile fileSetting</param>
		/// <returns>
		///   A character with the assumed delimiter for the file
		/// </returns>
		/// <remarks>
		///   No Error will not be thrown.
		/// </remarks>
		public static string GuessDelimiter(ICsvFile setting)
		{
			Contract.Requires(setting != null);
			Contract.Ensures(Contract.Result<string>() != null);
			using (var improvedStream = ImprovedStream.OpenRead(setting))
			using (var streamReader = new StreamReader(improvedStream.Stream, setting.GetEncoding(), setting.ByteOrderMark))
			{
				for (var i = 0; i < setting.SkipRows; i++)
					streamReader.ReadLine();
				var result = GuessDelimiter(streamReader, setting.FileFormat.EscapeCharacterChar, out var noDelimiter);
				setting.NoDelimitedFile = noDelimiter;
				return result;
			}
		}

		/// <summary>
		/// Guesses the json file.
		/// </summary>
		/// <param name="setting">The setting.</param>
		/// <returns></returns>
		public static bool GuessJsonFile(IFileSettingPhysicalFile setting)
		{
			Contract.Requires(setting != null);
			using (var improvedStream = ImprovedStream.OpenRead(setting))
			using (var streamReader = new StreamReader(improvedStream.Stream))
			{
				return IsJsonReadable(streamReader);
			}
		}

		/// <summary>
		///   Opens the csv file, and tries to read the headers
		/// </summary>
		/// <param name="setting">The CSVFile fileSetting</param>
		/// <param name="cancellationToken">A cancellation token</param>
		/// <returns>
		///   <c>True</c> we could use the first row as header, <c>false</c> should not use first row as header
		/// </returns>
		public static bool GuessHasHeader(ICsvFile setting, CancellationToken cancellationToken)
		{
			Contract.Requires(setting != null);
			// Only do so if HasFieldHeader is still true
			if (!setting.HasFieldHeader)
			{
				Logger.Information("Without Header Row");
				return false;
			}

			using (var dummy = new DummyProcessDisplay(cancellationToken))
			using (var csvDataReader = new CsvFileReader(setting, dummy))
			{
				csvDataReader.Open();

				var defaultNames = 0;

				// In addition check that all columns have real names and did not get an artificial name
				// or are numbers
				for (var counter = 0; counter < csvDataReader.FieldCount; counter++)
				{
					var columnName = csvDataReader.GetName(counter);

					// if replaced by a default assume no header
					if (columnName.Equals(BaseFileReader.GetDefaultName(counter), StringComparison.OrdinalIgnoreCase))
						if (defaultNames++ == (int)Math.Ceiling(csvDataReader.FieldCount / 2.0))
						{
							Logger.Information("Without Header Row");
							return false;
						}

					// if its a number assume no headers
					if (StringConversion.StringToDecimal(columnName, '.', ',', false).HasValue)
					{
						Logger.Information("Without Header Row");
						return false;
					}

					// if its rather long assume no header
					if (columnName.Length > 80)
					{
						Logger.Information("Without Header Row");
						return false;
					}
				}

				Logger.Information("With Header Row");
				// if there is only one line assume its does not have a header
				return true;
			}
		}

		/// <summary>
		///   Try to guess the new line sequence
		/// </summary>
		/// <param name="setting"><see cref="ICsvFile" /> with the information</param>
		/// <returns>The NewLine Combination used</returns>
		public static string GuessNewline(ICsvFile setting)
		{
			Contract.Requires(setting != null);
			using (var improvedStream = ImprovedStream.OpenRead(setting))
			using (var streamReader = new StreamReader(improvedStream.Stream, setting.GetEncoding(), setting.ByteOrderMark))
			{
				for (var i = 0; i < setting.SkipRows; i++)
					streamReader.ReadLine();
				return GuessNewline(streamReader, setting.FileFormat.FieldQualifierChar);
			}
		}

		/// <summary>
		///   Determines the start row in the file
		/// </summary>
		/// <param name="setting"><see cref="ICsvFile" /> with the information</param>
		/// <returns>
		///   The number of rows to skip
		/// </returns>
		private static char GuessQualifier(ICsvFile setting)
		{
			Contract.Requires(setting != null);
			using (var improvedStream = ImprovedStream.OpenRead(setting))
			using (var streamReader = new StreamReader(improvedStream.Stream, setting.GetEncoding(), setting.ByteOrderMark))
			{
				return GuessQualifier(streamReader, setting.FileFormat.FieldDelimiterChar, setting.SkipRows);
			}
		}

		/// <summary>
		///   Determines the start row in the file
		/// </summary>
		/// <param name="setting"><see cref="ICsvFile" /> with the information</param>
		/// <returns>
		///   The number of rows to skip
		/// </returns>
		public static int GuessStartRow(ICsvFile setting)
		{
			Contract.Requires(setting != null);
			using (var improvedStream = ImprovedStream.OpenRead(setting))
			using (var streamReader = new StreamReader(improvedStream.Stream, setting.GetEncoding(), setting.ByteOrderMark))
			{
				return GuessStartRow(streamReader, setting.FileFormat.FieldDelimiterChar, setting.FileFormat.FieldQualifierChar,
					setting.FileFormat.CommentLine);
			}
		}

		/// <summary>
		///   Does check if quoting was actually used in the file
		/// </summary>
		/// <param name="setting">The setting.</param>
		/// <param name="token">The token.</param>
		/// <returns>
		///   <c>true</c> if [has used qualifier] [the specified setting]; otherwise, <c>false</c>.
		/// </returns>
		public static bool HasUsedQualifier(ICsvFile setting, CancellationToken token)
		{
			Contract.Requires(setting != null);
			// if we do not have a quote defined it does not matter
			if (string.IsNullOrEmpty(setting.FileFormat.FieldQualifier) || token.IsCancellationRequested)
				return false;

			using (var improvedStream = ImprovedStream.OpenRead(setting))
			using (var streamReader = new StreamReader(improvedStream.Stream, setting.GetEncoding(), setting.ByteOrderMark))
			{
				for (var i = 0; i < setting.SkipRows; i++)
					streamReader.ReadLine();
				var buff = new char[262144];
				var isStartOfColumn = true;
				while (!streamReader.EndOfStream)
				{
					var read = streamReader.ReadBlock(buff, 0, 262143);

					// Look for Delimiter [Whitespace] Qualifier or Start o fLine [Whitespace] Qualifier
					for (var current = 0; current < read; current++)
					{
						if (token.IsCancellationRequested)
							return false;
						var c = buff[current];
						if (c == '\r' || c == '\n' || c == setting.FileFormat.FieldDelimiterChar)
						{
							isStartOfColumn = true;
							continue;
						}

						// if we are not at the start of a column we can get the next char
						if (!isStartOfColumn)
							continue;
						// If we are at the start of a column and this is a ", we can stop, this is a real qualifier
						if (c == setting.FileFormat.FieldQualifierChar)
							return true;
						// Any non whitespace will reset isStartOfColumn
						if (c <= '\x00ff')
							isStartOfColumn = c == ' ' || c == '\t';
						else
							isStartOfColumn = CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator;
					}
				}
			}

			return false;
		}

		/// <summary>
		///   Refreshes the settings assuming the file has changed, checks CodePage, Delimiter, Start Row and Header
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="display">The display.</param>
		/// <param name="guessJson">if true trying to determine if file is a JSOn file</param>
		/// <param name="guessCodePage">if true, try to determine teh codepage</param>
		/// <param name="guessDelimiter">if true, try to determine the delimiter</param>
		/// <param name="guessQualifier">if true, try to determine teh qualifier for text</param>
		/// <param name="guessStartRow">if true, try to determine teh number of skipped rows</param>
		/// <param name="guessHasHeader">if true, try to determine if the file does have a header row</param>
		public static void RefreshCsvFile(this ICsvFile file, IProcessDisplay display, bool guessJson = false, bool guessCodePage = true, bool guessDelimiter = true, bool guessQualifier = true, bool guessStartRow = true, bool guessHasHeader = true)
		{
			Contract.Requires(file != null);
			Contract.Requires(display != null);

			var root = ApplicationSetting.RootFolder;
			file.FileName.GetAbsolutePath(root);

			display.SetProcess("Checking delimited file", -1, true);
			file.JsonFormat = false;
			if (guessJson)
			{
				if (GuessJsonFile(file))
					file.JsonFormat = true;
			}

			if (file.JsonFormat)
				display.SetProcess("Detected Json file", -1, true);
			else
			{
				if (guessCodePage)
				{
					GuessCodePage(file);
					if (display.CancellationToken.IsCancellationRequested)
						return;
					display.SetProcess("Code Page: " +
														 EncodingHelper.GetEncodingName(file.CurrentEncoding.CodePage, true, file.ByteOrderMark), -1, true);
				}
				if (guessDelimiter)
				{
					file.FileFormat.FieldDelimiter = GuessDelimiter(file);
					if (display.CancellationToken.IsCancellationRequested)
						return;
					display.SetProcess("Delimiter: " + file.FileFormat.FieldDelimiter, -1, true);
				}
				if (guessQualifier)
				{
					var qualifier = GuessQualifier(file);
					file.FileFormat.FieldQualifier = qualifier == '\0' ? string.Empty : char.ToString(qualifier);
				}
				if (guessStartRow)
				{
					file.SkipRows = GuessStartRow(file);
					if (display.CancellationToken.IsCancellationRequested)
						return;
					if (file.SkipRows > 0)
						display.SetProcess("Start Row: " + file.SkipRows.ToString(CultureInfo.InvariantCulture), -1, true);
				}
				if (guessHasHeader)
				{
					file.HasFieldHeader = GuessHasHeader(file, display.CancellationToken);
					display.SetProcess("Column Header: " + file.HasFieldHeader, -1, true);
				}
			}
		}

		private static bool IsJsonReadable(TextReader streamReader)
		{
			if (streamReader == null)
			{
				Logger.Information("File not read");
				return false;
			}

			using (var jsonTextReader = new JsonTextReader(streamReader))
			{
				jsonTextReader.CloseInput = false;
				try
				{
					if (jsonTextReader.Read())
						return jsonTextReader.TokenType == JsonToken.StartObject ||
									 jsonTextReader.TokenType == JsonToken.StartArray ||
									 jsonTextReader.TokenType == JsonToken.StartConstructor;
				}
				catch (JsonReaderException)
				{
					//ignore
				}

				return false;
			}
		}

		/// <summary>
		///   Guesses the delimiter for a files.
		///   Done with a rather simple csv parsing, and trying to find the delimiter that has the least variance in the read rows,
		///   if that is not possible the delimiter with the highest number of occurrences.
		/// </summary>
		/// <param name="streamReader">The StreamReader with the data</param>
		/// <param name="escapeCharacter">The escape character.</param>
		/// <param name="hasDelimiter">if set to <c>true</c> [has delimiter].</param>
		/// <returns>
		///   A character with the assumed delimiter for the file
		/// </returns>
		/// <exception cref="ArgumentNullException">streamReader</exception>
		/// <remarks>
		///   No Error will not be thrown.
		/// </remarks>
		[SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Body")]
		private static string GuessDelimiter(StreamReader streamReader, char escapeCharacter, out bool hasDelimiter)
		{
			if (streamReader == null)
				throw new ArgumentNullException(nameof(streamReader));

			hasDelimiter = false;
			Contract.Ensures(Contract.Result<string>() != null);
			var match = '\0';

			var dc = GetDelimiterCounter(streamReader, escapeCharacter, 300);

			// Limit everything to 100 columns max, the sum might get too big otherwise 100 * 100
			var startRow = dc.LastRow > 60 ? 15 :
				dc.LastRow > 20 ? 5 : 0;

			var validSeparatorIndex = new List<int>();
			for (var index = 0; index < dc.Separators.Length; index++)
			{
				// only regard a delimiter if we have 75% of the rows with this delimiter
				// we can still have a lot of commented lines
				if (dc.SeparatorRows[index] == 0 || dc.SeparatorRows[index] < dc.LastRow * .75d && dc.LastRow > 5)
					continue;
				validSeparatorIndex.Add(index);
			}

			// if only one was found done here
			if (validSeparatorIndex.Count == 1)
			{
				match = dc.Separators[validSeparatorIndex[0]];
			}
			else
			{
				// otherwise find the best
				foreach (var index in validSeparatorIndex)
					for (var row = startRow; row < dc.LastRow; row++)
						if (dc.SeparatorsCount[index, row] > 100)
							dc.SeparatorsCount[index, row] = 100;

				double? bestScore = null;
				var maxCount = 0;

				foreach (var index in validSeparatorIndex)
				{
					var sumCount = 0;
					// If there are enough rows skip the first rows, there might be a descriptive introduction
					// this can not be done in case there are not many rows
					for (var row = startRow; row < dc.LastRow; row++)
						// Cut of at 50 Columns in case one row is messed up, this should not mess up everything
						sumCount += dc.SeparatorsCount[index, row];

					// If we did not find a match with variance use the absolute number of occurrences
					if (sumCount > maxCount && !bestScore.HasValue)
					{
						maxCount = sumCount;
						match = dc.Separators[index];
					}

					// Get the average of the rows
					var avg = (int)Math.Round((double)sumCount / (dc.LastRow - startRow), 0, MidpointRounding.AwayFromZero);

					// Only proceed if there is usually more then one occurrence and we have more then one row
					if (avg < 1 || dc.SeparatorRows[index] == 1)
						continue;

					// First determine the variance, low value means and even distribution
					double cutVariance = 0;
					for (var row = startRow; row < dc.LastRow; row++)
					{
						var dist = dc.SeparatorsCount[index, row] - avg;
						if (dist > 2 || dist < -2)
							cutVariance += 8;
						else switch (dist)
						{
							case 2:
							case -2:
								cutVariance += 4;
								break;
							case 1:
							case -1:
								cutVariance++;
								break;
						}
					}

					// The score is dependent on the average columns found and the regularity
					var score = avg - Math.Round(cutVariance / (dc.LastRow - startRow), 2);
					if (bestScore.HasValue && !(score > bestScore.Value))
						continue;
					match = dc.Separators[index];
					bestScore = score;
				}
			}

			hasDelimiter = match != '\0';
			if (!hasDelimiter)
			{
				Logger.Information("Not a delimited file");
				return "TAB";
			}

			var result = match == '\t' ? "TAB" : match.ToString(CultureInfo.CurrentCulture);
			Logger.Information("Delimiter: {delimiter}", result);
			return result;
		}

		private static string GuessNewline(TextReader streamReader, char fieldQualifier)
		{
			Contract.Requires(streamReader != null);
			Contract.Ensures(Contract.Result<string>() != null);
			const int c_NumRows = 50;

			var lastRow = 0;
			var readChar = 0;
			var quoted = false;

			const int c_Cr = 0;
			const int c_LF = 1;
			const int c_CRLF = 2;
			const int c_Lfcr = 3;
			int[] count = { 0, 0, 0, 0 };

			// \r = CR (Carriage Return) \n = LF (Line Feed)

			while (lastRow < c_NumRows && readChar >= 0)
			{
				readChar = streamReader.Read();
				if (readChar == fieldQualifier)
				{
					if (quoted)
					{
						if (streamReader.Peek() != fieldQualifier)
							quoted = false;
						else
							streamReader.Read();
					}
					else
					{
						quoted = true;
					}
				}

				if (quoted)
					continue;
				if (readChar == '\n')
				{
					if (streamReader.Peek() == '\r')
					{
						streamReader.Read();
						count[c_Lfcr]++;
					}
					else
					{
						count[c_LF]++;
					}

					lastRow++;
				}

				if (readChar != '\r')
					continue;
				if (streamReader.Peek() == '\n')
				{
					streamReader.Read();
					count[c_CRLF]++;
				}
				else
				{
					count[c_Cr]++;
				}

				lastRow++;
			}

			if (count[c_Cr] > count[c_CRLF] && count[c_Cr] > count[c_Lfcr] && count[c_Cr] > count[c_LF])
				return "CR";
			if (count[c_LF] > count[c_CRLF] && count[c_LF] > count[c_Lfcr] && count[c_LF] > count[c_Cr])
				return "LF";
			if (count[c_Lfcr] > count[c_CRLF] && count[c_Lfcr] > count[c_LF] && count[c_Lfcr] > count[c_Cr])
				return "LFCR";

			return "CRLF";
		}

		private static char GuessQualifier(TextReader streamReader, char delimiter, int skipRows)
		{
			if (streamReader == null)
				return '\0';

			const int c_MaxLine = 30;
			var possibleQuotes = new[] { '"', '\'' };

			var counter = new int[possibleQuotes.Length];

			// skip the first line it usually a header
			for (var lineNo = 0; lineNo < c_MaxLine + skipRows; lineNo++)
			{
				var line = streamReader.ReadLine();
				// EOF
				if (line == null)
					break;
				if (lineNo < skipRows)
					continue;

				// Note: Delimiters in quoted text will split the actual column in multiple this will be ignore here, we hope to find columns that do not contain delimiters
				var cols = line.Split(delimiter);
				foreach (var col in cols)
				{
					if (string.IsNullOrEmpty(col))
						continue;
					var test = col.Trim();
					// the column need to start and end with the same characters, its at least 2 char long
					if (test.Length < 2 || test[0] != test[test.Length - 1])
						continue;

					// check all setup test chars
					for (var testChar = 0; testChar < possibleQuotes.Length; testChar++)
						if (test[0] == possibleQuotes[testChar])
							counter[testChar]++;
				}
			}

			// get the highest number of quoted columns
			var max = 1;
			for (var testChar = 0; testChar < possibleQuotes.Length; testChar++)
				if (counter[testChar] > max)
					max = counter[testChar];

			// We need a certain level of confidence only one quoted column is not enough,
			if (max <= 1) return '\0';
			{
				for (var testChar = 0; testChar < possibleQuotes.Length; testChar++)
					if (counter[testChar] == max)
						return possibleQuotes[testChar];
			}
			return '\0';
		}

		/// <summary>
		///   Guesses the start row of a CSV file Done with a rather simple csv parsing
		/// </summary>
		/// <param name="streamReader">The stream reader with the data</param>
		/// <param name="delimiter">The delimiter.</param>
		/// <param name="quoteChar">The quoting char</param>
		/// <param name="commentLine">The characters for a comment line.</param>
		/// <returns>
		///   The number of rows to skip
		/// </returns>
		/// <exception cref="ArgumentNullException">commentLine</exception>
		private static int GuessStartRow(TextReader streamReader, char delimiter, char quoteChar, string commentLine)
		{
			if (commentLine == null)
				throw new ArgumentNullException(nameof(commentLine));
			Contract.Ensures(Contract.Result<int>() >= 0);
			const int c_MaxRows = 50;
			if (streamReader == null)
				return 0;

			var columnCount = new List<int>(c_MaxRows);
			var rowMapping = new Dictionary<int, int>(c_MaxRows);
			{
				var colCount = new int[c_MaxRows];
				var isComment = new bool[c_MaxRows];
				var quoted = false;
				var firstChar = true;
				var lastRow = 0;
				while (lastRow < c_MaxRows && streamReader.Peek() >= 0)
				{
					var readChar = (char)streamReader.Read();

					// Handle Commented lines
					if (firstChar && commentLine.Length > 0 && !isComment[lastRow] && readChar == commentLine[0])
					{
						isComment[lastRow] = true;

						for (var pos = 1; pos < commentLine.Length; pos++)
						{
							var nextChar = (char)streamReader.Peek();
							if (nextChar != commentLine[pos])
							{
								isComment[lastRow] = false;
								break;
							}
						}
					}

					// Handle Quoting
					if (readChar == quoteChar && !isComment[lastRow])
					{
						if (quoted)
						{
							if (streamReader.Peek() != '"')
								quoted = false;
							else
								streamReader.Read();
						}
						else
						{
							quoted |= firstChar;
						}

						continue;
					}

					switch (readChar)
					{
						// Feed and NewLines
						case '\n':
							if (!quoted)
							{
								lastRow++;
								firstChar = true;
								if (streamReader.Peek() == '\r')
									streamReader.Read();
							}

							break;

						case '\r':
							if (!quoted)
							{
								lastRow++;
								firstChar = true;
								if (streamReader.Peek() == '\n')
									streamReader.Read();
							}

							break;

						default:
							if (!isComment[lastRow] && !quoted && readChar == delimiter)
							{
								colCount[lastRow]++;
								firstChar = true;
								continue;
							}

							break;
					}

					// Its still the first char if its a leading space
					if (firstChar && readChar != ' ')
						firstChar = false;
				}

				// remove all rows that are comment lines...
				for (var row = 0; row < lastRow; row++)
				{
					rowMapping[columnCount.Count] = row;
					if (!isComment[row])
						columnCount.Add(colCount[row]);
				}
			}

			// if we do not more than 4 proper rows do nothing
			if (columnCount.Count < 4)
				return 0;

			// In case we have a row that is exactly twice as long as the row
			// before and row after, assume its missing a linefeed
			for (var row = 1; row < columnCount.Count - 1; row++)
				if (columnCount[row + 1] > 0 && columnCount[row] == columnCount[row + 1] * 2 &&
						columnCount[row] == columnCount[row - 1] * 2)
					columnCount[row] = columnCount[row + 1];

			// Get the average of the last 15 rows
			var num = 0;
			var sum = 0;
			for (var row = columnCount.Count - 1; num < 10 && row > 0; row--)
			{
				if (columnCount[row] <= 0)
					continue;
				sum += columnCount[row];
				num++;
			}

			var avg = (int)(sum / (double)(num == 0 ? 1 : num));
			// If there are not many columns do not try to guess
			if (avg <= 1)
				return 0;
			{
				// If the first rows would be a good fit return this
				if (columnCount[0] >= avg)
					return 0;

				for (var row = columnCount.Count - 1; row > 0; row--)
					if (columnCount[row] > 0)
					{
						if (columnCount[row] >= avg - 1) continue;
						Logger.Information("Start Row: {row}", row);
						return rowMapping[row];
					}
					// In case we have an empty line but the next line are roughly good match take that empty line
					else if (row + 2 < columnCount.Count && columnCount[row + 1] == columnCount[row + 2] &&
									 columnCount[row + 1] >= avg - 1)
					{
						Logger.Information("Start Row: {row}", row + 1);
						return rowMapping[row + 1];
					}

				for (var row = 0; row < columnCount.Count; row++)
					if (columnCount[row] > 0)
					{
						Logger.Information("Start Row: {row}", row);
						return rowMapping[row];
					}
			}
			return 0;
		}

		private static DelimiterCounter GetDelimiterCounter(TextReader streamReader, char escapeCharacter, int numRows)
		{
			Contract.Ensures(Contract.Result<DelimiterCounter>() != null);

			var dc = new DelimiterCounter(numRows);

			var quoted = false;
			var firstChar = true;
			var readChar = 0;
			var contends = new StringBuilder();

			while (dc.LastRow < dc.NumRows && readChar >= 0)
			{
				var lastChar = (char)readChar;
				readChar = streamReader.Read();
				contends.Append(readChar);
				if (lastChar == escapeCharacter)
					continue;
				switch (readChar)
				{
					case -1:
						dc.LastRow++;
						break;

					case '"':
						if (quoted)
						{
							if (streamReader.Peek() != '"')
								quoted = false;
							else
								streamReader.Read();
						}
						else
						{
							quoted |= firstChar;
						}

						break;

					case '\n':
					case '\r':
						if (!quoted && !firstChar)
						{
							dc.LastRow++;
							firstChar = true;
							continue;
						}

						break;

					default:
						if (!quoted)
						{
							var index = dc.Separators.IndexOf((char)readChar);
							if (index != -1)
							{
								if (dc.SeparatorsCount[index, dc.LastRow] == 0)
									dc.SeparatorRows[index]++;
								++dc.SeparatorsCount[index, dc.LastRow];
								firstChar = true;
								continue;
							}
						}

						break;
				}

				// Its still the first char if its a leading space
				if (firstChar && readChar != ' ')
					firstChar = false;
			}

			return dc;
		}

		private class DelimiterCounter
		{
			public readonly int NumRows;

			public readonly int[] SeparatorRows;

			public readonly string Separators;
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
			public readonly int[,] SeparatorsCount;

			public int LastRow;

			// Added INFORMATION SEPARATOR ONE to FOUR
			private const string c_DefaultSeparators = "\t,;|¦￤*`\u001F\u001E\u001D\u001C";

			public DelimiterCounter(int numRows)
			{
				NumRows = numRows;
				try
				{
					var listSeparatorr = CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];
					if (c_DefaultSeparators.IndexOf(listSeparatorr) == -1)
						Separators = c_DefaultSeparators + listSeparatorr;
					else
						Separators = c_DefaultSeparators;
					SeparatorsCount = new int[Separators.Length, NumRows];
					SeparatorRows = new int[Separators.Length];
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.InnerExceptionMessages());
				}
			}
		}

#pragma warning restore CA1814 // Prefer jagged arrays over multidimensional
	}
}