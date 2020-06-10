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
using System.Globalization;
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
  ///   Helper class
  /// </summary>
  public static class CsvHelper
  {
    /// <summary>
    ///   Gets the <see cref="Encoding" /> of the textFile
    /// </summary>
    /// <param name="setting">The setting.</param>
    /// <returns></returns>
    public static async Task<Encoding> GetEncodingAsync([NotNull] this ICsvFile setting)
    {
      if (setting.CodePageId < 0)
        await GuessCodePageAsync(setting).ConfigureAwait(false);
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

    private static async Task<Tuple<int, bool>> GuessCodePageAsync([NotNull] IImprovedStream stream)
    {
      // Read 256 kBytes
      var buff = new byte[262144];

      var length = await stream.Stream.ReadAsync(buff, 0, buff.Length).ConfigureAwait(false);
      if (length >= 2)
      {
        var byBom = EncodingHelper.GetCodePageByByteOrderMark(buff);
        if (byBom != 0) return new Tuple<int, bool>(byBom, true);
      }

      var detected = EncodingHelper.GuessCodePageNoBom(buff, length);
      if (detected == 20127)
        detected = 65001;
      return new Tuple<int, bool>(detected, false);
    }

    /// <summary>
    ///   Guesses the code page ID of a file
    /// </summary>
    /// <param name="setting">The CSVFile fileSetting</param>
    /// <remarks>No Error will be thrown, the CodePage and the BOM will bet set</remarks>
    public static async Task GuessCodePageAsync([NotNull] ICsvFile setting)
    {


      using (var improvedStream = FunctionalDI.OpenRead(setting))
      {
        var result = await GuessCodePageAsync(improvedStream).ConfigureAwait(false);
        setting.CodePageId = result.Item1;
        setting.ByteOrderMark = result.Item2;
        Logger.Information("Detected Code Page: {codepage}",
          EncodingHelper.GetEncodingName(result.Item1, true, result.Item2));
      }
    }

    /// <summary>
    ///   Guesses the delimiter for a files. Done with a rather simple csv parsing, and trying to
    ///   find the delimiter that has the least variance in the read rows, if that is not possible
    ///   the delimiter with the highest number of occurrences.
    /// </summary>
    /// <param name="setting">The CSVFile fileSetting</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A character with the assumed delimiter for the file</returns>
    /// <remarks>No Error will not be thrown.</remarks>
    [NotNull]
    public static async Task<string> GuessDelimiterAsync([NotNull] ICsvFile setting, CancellationToken cancellationToken)
    {

      using (var improvedStream = FunctionalDI.OpenRead(setting))
      using (var textReader =
        new ImprovedTextReader(improvedStream, (await setting.GetEncodingAsync()).CodePage, setting.SkipRows))
      {
        var result = await GuessDelimiterAsync(textReader, setting.FileFormat.EscapeCharacterChar, cancellationToken).ConfigureAwait(false);
        setting.FileFormat.FieldDelimiter = result.Item1;
        setting.NoDelimitedFile = result.Item2;

        return result.Item1;
      }
    }

    public static async Task<bool> GuessHasHeaderAsync([NotNull] ImprovedTextReader reader, string comment, char delimiter,
      CancellationToken cancellationToken)
    {
      var headerLine = string.Empty;

      while (string.IsNullOrEmpty(headerLine) && !reader.EndOfFile)
      {
        cancellationToken.ThrowIfCancellationRequested();
        headerLine = await reader.ReadLineAsync().ConfigureAwait(false);
        if (!string.IsNullOrEmpty(comment) && headerLine.TrimStart().StartsWith(comment))
          headerLine = string.Empty;
      }

      if (string.IsNullOrEmpty(headerLine))
      {
        Logger.Information("Without Header Row");
        return false;
      }

      var headerRow = headerLine.Split(delimiter);
      // get the average field count looking at the header and 12 additional valid lines
      var fieldCount = headerRow.Length;
      var counter = 1;
      while (counter < 12 && !cancellationToken.IsCancellationRequested && !reader.EndOfFile)
      {
        var dataLine = await reader.ReadLineAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(dataLine)
            || !string.IsNullOrEmpty(comment) && dataLine.TrimStart().StartsWith(comment))
          continue;
        counter++;
        fieldCount += dataLine.Split(delimiter).Length;
      }

      var avgFieldCount = fieldCount / (double) counter;
      // The average should not be smaller than the columns in the initial row
      if (avgFieldCount < headerRow.Length)
        avgFieldCount = headerRow.Length;
      var halfTheColumns = (int) Math.Ceiling(avgFieldCount / 2.0);

      // use the same routine that is used in readers to determine the names of the columns
      var warning = new ColumnErrorDictionary();
      var columns = BaseFileReader.ParseColumnNames(headerRow, (int) avgFieldCount, warning).ToList();

      // looking at the warnings raised
      if (warning.Any(x => x.Value.Contains("exists more than once"))
          || warning.Any(x => x.Value.Contains("too long"))
          || warning.Count(x => x.Value.Contains("title was empty")) >= halfTheColumns)
      {
        Logger.Information("Without Header Row");
        return false;
      }

      // Columns are only one or two char, that does not look descriptive
      if (columns.Count(x => x.Length < 3) > halfTheColumns)
      {
        Logger.Information("Without Header Row");
        return false;
      }

      Logger.Information("With Header Row");
      // if there is only one line assume its does not have a header
      return true;
    }

    /// <summary>
    ///   Guesses if the file is a json file.
    /// </summary>
    /// <param name="setting">The setting.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [NotNull]
    public static async Task<bool> GuessJsonFileAsync([NotNull] IFileSettingPhysicalFile setting,
      CancellationToken cancellationToken)
    {
      using (var improvedStream = FunctionalDI.OpenRead(setting))
      {
        return await IsJsonReadableAsync(improvedStream, cancellationToken).ConfigureAwait(false);
      }
    }

    /// <summary>
    ///   Try to guess the new line sequence
    /// </summary>
    /// <param name="setting"><see cref="ICsvFile" /> with the information</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The NewLine Combination used</returns>
    [NotNull]
    public static async Task<string> GuessQualifierAsync([NotNull] ICsvFile setting,
      CancellationToken cancellationToken)
    {
      using (var improvedStream = FunctionalDI.OpenRead(setting))
      using (var streamReader = new ImprovedTextReader(improvedStream, setting.CodePageId, setting.SkipRows))
      {
        var qualifier = await GuessQualifierAsync(streamReader, setting.FileFormat.FieldDelimiterChar, cancellationToken).ConfigureAwait(false);
        if (qualifier != '\0')
          return char.ToString(qualifier);
      }
      return null;
    }

    /// <summary>
    ///   Try to guess the new line sequence
    /// </summary>
    /// <param name="setting"><see cref="ICsvFile" /> with the information</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The NewLine Combination used</returns>
    [NotNull]
    public static async Task<RecordDelimiterType> GuessNewlineAsync([NotNull] ICsvFile setting,
      CancellationToken cancellationToken)
    {
      
      using (var improvedStream = FunctionalDI.OpenRead(setting))
      using (var streamReader = new ImprovedTextReader(improvedStream, setting.CodePageId, setting.SkipRows))
      {
        return await GuessNewlineAsync(streamReader, setting.FileFormat.FieldQualifierChar, cancellationToken).ConfigureAwait(false);
      }
    }

    /// <summary>
    ///   Determines the start row in the file
    /// </summary>
    /// <param name="setting"><see cref="ICsvFile" /> with the information</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>The number of rows to skip</returns>
    [NotNull]
    public static async Task<int> GuessStartRowAsync([NotNull] ICsvFile setting, CancellationToken cancellationToken)
    {
      using (var improvedStream = FunctionalDI.OpenRead(setting))
      using (var streamReader = new ImprovedTextReader(improvedStream, (await setting.GetEncodingAsync()).CodePage))
      {
        return await GuessStartRowAsync(streamReader, setting.FileFormat.FieldDelimiterChar,
          setting.FileFormat.FieldQualifierChar,
          setting.FileFormat.CommentLine, cancellationToken).ConfigureAwait(false);
      }
    }

    /// <summary>
    ///   Does check if quoting was actually used in the file
    /// </summary>
    /// <param name="setting">The setting.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns><c>true</c> if [has used qualifier] [the specified setting]; otherwise, <c>false</c>.</returns>
    [NotNull]
    public static async Task<bool> HasUsedQualifierAsync([NotNull] ICsvFile setting, CancellationToken cancellationToken)
    {
      // if we do not have a quote defined it does not matter
      if (string.IsNullOrEmpty(setting.FileFormat.FieldQualifier) || cancellationToken.IsCancellationRequested)
        return false;

      using (var improvedStream = FunctionalDI.OpenRead(setting))
      using (var streamReader =
        new ImprovedTextReader(improvedStream, (await setting.GetEncodingAsync().ConfigureAwait(false)).CodePage, setting.SkipRows))
      {
        var isStartOfColumn = true;
        while (!streamReader.EndOfFile)
        {
          if (cancellationToken.IsCancellationRequested)
            return false;
          var c = (char) await streamReader.ReadAsync();
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

      return false;
    }

    /// <summary>
    ///   Refreshes the settings assuming the file has changed, checks CodePage, Delimiter, Start
    ///   Row and Header
    /// </summary>
    /// <param name="setting"></param>
    /// <param name="display">The display.</param>
    /// <param name="guessJson">if true trying to determine if file is a JSOn file</param>
    /// <param name="guessCodePage">if true, try to determine teh codepage</param>
    /// <param name="guessDelimiter">if true, try to determine the delimiter</param>
    /// <param name="guessQualifier">if true, try to determine teh qualifier for text</param>
    /// <param name="guessStartRow">if true, try to determine teh number of skipped rows</param>
    /// <param name="guessHasHeader">
    ///   if true, try to determine if the file does have a header row
    /// </param>
    /// <param name="guessNewLine">if true, try to determine what kind of new line we do use</param>
    [NotNull]
    public static async Task RefreshCsvFileAsync(
      [NotNull] this ICsvFile setting,
      [NotNull] IProcessDisplay display,
      bool guessJson = false,
      bool guessCodePage = true,
      bool guessDelimiter = true,
      bool guessQualifier = true,
      bool guessStartRow = true,
      bool guessHasHeader = true,
      bool guessNewLine = true)
    {
      if (setting == null) throw new ArgumentNullException(nameof(setting));
      if (display == null) throw new ArgumentNullException(nameof(display));

      if (!(guessJson || guessCodePage || guessDelimiter || guessStartRow || guessQualifier || guessHasHeader ||
            guessNewLine))
        return;
      display.SetProcess("Checking delimited file", -1, true);
      using (var improvedStream = FunctionalDI.OpenRead(setting))
      {
        setting.JsonFormat = false;
        if (guessJson)
        {
          display.SetProcess("Checking Json format", -1, true);
          if (await IsJsonReadableAsync(improvedStream, display.CancellationToken).ConfigureAwait(false))
            setting.JsonFormat = true;
        }

        if (setting.JsonFormat)
        {
          display.SetProcess("Detected Json file", -1, true);
          return;
        }

        if (guessCodePage)
        {
          if (display.CancellationToken.IsCancellationRequested)
            return;
          display.SetProcess("Checking Code Page", -1, true);
          improvedStream.ResetToStart(null);
          var result = await GuessCodePageAsync(improvedStream).ConfigureAwait(false);
          setting.CodePageId = result.Item1;
          setting.ByteOrderMark = result.Item2;

          display.SetProcess($"Code Page: {EncodingHelper.GetEncodingName(result.Item1, true, result.Item2)}", -1,
            true);
        }

        // from here on us the encoding to read the stream again
        if (guessStartRow)
        {
          if (display.CancellationToken.IsCancellationRequested)
            return;
          display.SetProcess("Checking Start Row", -1, true);
          improvedStream.ResetToStart(null);
          using (var textReader = new ImprovedTextReader(improvedStream, setting.CodePageId))
          {
            setting.SkipRows = await GuessStartRowAsync(textReader, setting.FileFormat.FieldDelimiterChar,
              setting.FileFormat.FieldQualifierChar,
              setting.FileFormat.CommentLine, display.CancellationToken).ConfigureAwait(false);
          }

          if (setting.SkipRows > 0)
            display.SetProcess("Start Row: " + setting.SkipRows.ToString(CultureInfo.InvariantCulture), -1, true);
        }

        if (guessQualifier || guessDelimiter || guessHasHeader)
        {
          improvedStream.ResetToStart(null);
          using (var textReader = new ImprovedTextReader(improvedStream, setting.CodePageId, setting.SkipRows))
          {
            if (guessDelimiter)
            {
              if (display.CancellationToken.IsCancellationRequested)
                return;
              display.SetProcess("Checking Delimiter", -1, true);
              var result = await GuessDelimiterAsync(textReader, setting.FileFormat.EscapeCharacterChar,
                display.CancellationToken).ConfigureAwait(false);
              setting.NoDelimitedFile = result.Item2;
              setting.FileFormat.FieldDelimiter = result.Item1;
              display.SetProcess("Delimiter: " + setting.FileFormat.FieldDelimiter, -1, true);
            }

            if (guessNewLine)
            {
              if (display.CancellationToken.IsCancellationRequested)
                return;
              display.SetProcess("Checking Record Delimiter", -1, true);
              improvedStream.ResetToStart(null);
              var res = await GuessNewlineAsync(textReader, setting.FileFormat.FieldQualifierChar,
                display.CancellationToken).ConfigureAwait(false);
              if (res != RecordDelimiterType.None)
              {
                setting.FileFormat.NewLine = res;
                display.SetProcess("Record Delimiter: " + res.Description(), -1, true);
              }
              else
              {
                display.SetProcess("Record Delimiter could not be determined", -1, true);
              }
            }

            if (guessQualifier)
            {
              if (display.CancellationToken.IsCancellationRequested)
                return;
              display.SetProcess("Checking Qualifier", -1, true);
              var qualifier = await GuessQualifierAsync(textReader, setting.FileFormat.FieldDelimiterChar,
                display.CancellationToken).ConfigureAwait(false);
              if (qualifier != '\0')
                setting.FileFormat.FieldQualifier = char.ToString(qualifier);
              display.SetProcess("Qualifier: " + setting.FileFormat.FieldQualifier, -1, true);
            }

            if (guessHasHeader)
            {
              if (display.CancellationToken.IsCancellationRequested)
                return;
              display.SetProcess("Checking for Header", -1, true);
              textReader.ToBeginning();
              setting.HasFieldHeader = await GuessHasHeaderAsync(textReader, setting.FileFormat.CommentLine,
                setting.FileFormat.FieldDelimiterChar, display.CancellationToken).ConfigureAwait(false);
              display.SetProcess("Column Header: " + setting.HasFieldHeader, -1, true);
            }
          }
        }
      }
    }

    [NotNull]
    private static async Task<DelimiterCounter> GetDelimiterCounterAsync([NotNull] ImprovedTextReader textReader,
      char escapeCharacter, int numRows, CancellationToken cancellationToken)
    {
      if (textReader == null) throw new ArgumentNullException(nameof(textReader));

      var dc = new DelimiterCounter(numRows);

      var quoted = false;
      var firstChar = true;
      var readChar = -1;
      var contends = new StringBuilder();
      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);

      while (dc.LastRow < dc.NumRows && !textReaderPosition.AllRead && !cancellationToken.IsCancellationRequested)
      {
        var lastChar = readChar;
        readChar = await textReader.ReadAsync().ConfigureAwait(false);
        contends.Append(readChar);
        if (lastChar == escapeCharacter)
          continue;
        switch (readChar)
        {
          case '"':
            if (quoted)
            {
              if (await textReader.PeekAsync().ConfigureAwait(false) != '"')
                quoted = false;
              else
                textReader.MoveNext();
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
              var index = dc.Separators.IndexOf((char) readChar);
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

    /// <summary>
    ///   Guesses the delimiter for a files. Done with a rather simple csv parsing, and trying to
    ///   find the delimiter that has the least variance in the read rows, if that is not possible
    ///   the delimiter with the highest number of occurrences.
    /// </summary>
    /// <param name="textReader">The StreamReader with the data</param>
    /// <param name="escapeCharacter">The escape character.</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A character with the assumed delimiter for the file</returns>
    /// <exception cref="ArgumentNullException">streamReader</exception>
    /// <remarks>No Error will not be thrown.</remarks>
    [NotNull]
    private static async Task<Tuple<string, bool>> GuessDelimiterAsync([NotNull] ImprovedTextReader textReader,
      char escapeCharacter, CancellationToken cancellationToken)
    {
      if (textReader == null)
        throw new ArgumentNullException(nameof(textReader));
      var match = '\0';

      var dc = await GetDelimiterCounterAsync(textReader, escapeCharacter, 300, cancellationToken);

      // Limit everything to 100 columns max, the sum might get too big otherwise 100 * 100
      var startRow = dc.LastRow > 60 ? 15 :
        dc.LastRow > 20 ? 5 : 0;

      cancellationToken.ThrowIfCancellationRequested();

      var validSeparatorIndex = new List<int>();
      for (var index = 0; index < dc.Separators.Length; index++)
      {
        // only regard a delimiter if we have 75% of the rows with this delimiter we can still have
        // a lot of commented lines
        if (dc.SeparatorRows[index] == 0 || dc.SeparatorRows[index] < dc.LastRow * .70d && dc.LastRow > 5)
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
          cancellationToken.ThrowIfCancellationRequested();
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
          var avg = (int) Math.Round((double) sumCount / (dc.LastRow - startRow), 0, MidpointRounding.AwayFromZero);

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
            else
              switch (dist)
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

      var hasDelimiter = match != '\0';
      if (!hasDelimiter)
      {
        Logger.Information("Not a delimited file");
        return new Tuple<string, bool>("TAB", false);
      }

      var result = match == '\t' ? "TAB" : match.ToString(CultureInfo.CurrentCulture);
      Logger.Information("Delimiter: {delimiter}", result);
      return new Tuple<string, bool>(result, true);
    }

    [NotNull]
    private static async Task<RecordDelimiterType> GuessNewlineAsync([NotNull] ImprovedTextReader textReader, char fieldQualifier,
      CancellationToken token)
    {
      if (textReader == null) throw new ArgumentNullException(nameof(textReader));
      const int c_NumChars = 8192;

      var currentChar = 0;
      var quoted = false;

      const int c_Cr = 0;
      const int c_Lf = 1;
      const int c_CrLf = 2;
      const int c_Lfcr = 3;
      const int c_RecSep = 4;
      const int c_UnitSep = 5;

      int[] count = { 0, 0, 0, 0, 0, 0 };

      // \r = CR (Carriage Return) \n = LF (Line Feed)

      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
      while (currentChar < c_NumChars && !textReaderPosition.AllRead && !token.IsCancellationRequested)
      {
        var readChar = await textReader.ReadAsync().ConfigureAwait(false);
        if (readChar == fieldQualifier)
        {
          if (quoted)
          {
            if (await textReader.PeekAsync().ConfigureAwait(false) != fieldQualifier)
              quoted = false;
            else
              textReader.MoveNext();
          }
          else
          {
            quoted = true;
          }
        }

        if (quoted)
          continue;

        switch (readChar)
        {
          case 30:
            count[c_RecSep]++;
            continue;
          case 31:
            count[c_UnitSep]++;
            continue;
          case 10:
          {
            if (await textReader.PeekAsync().ConfigureAwait(false) == 13)
            {
              textReader.MoveNext();
              count[c_Lfcr]++;
            }
            else
            {
              count[c_Lf]++;
            }

            currentChar++;
            break;
          }
          case 13:
          {
            if (await textReader.PeekAsync().ConfigureAwait(false) == 10)
            {
              textReader.MoveNext();
              count[c_CrLf]++;
            }
            else
            {
              count[c_Cr]++;
            }

            break;
          }
        }

        currentChar++;
      }

      var maxCount = count.Max();
      if (maxCount == 0)
        return RecordDelimiterType.None;

      return count[c_RecSep] == maxCount ? RecordDelimiterType.RS
        : count[c_UnitSep] == maxCount ? RecordDelimiterType.US
        : count[c_Cr] == maxCount ? RecordDelimiterType.CR
        : count[c_Lf] == maxCount ? RecordDelimiterType.LF
        : count[c_Lfcr] == maxCount ? RecordDelimiterType.LFCR
        : count[c_CrLf] == maxCount ? RecordDelimiterType.CRLF
        : RecordDelimiterType.None;
    }

    [NotNull]
    private static async Task<char> GuessQualifierAsync([NotNull] ImprovedTextReader textReader, char delimiter, CancellationToken cancellationToken)
    {
      if (textReader == null) throw new ArgumentNullException(nameof(textReader));


      const int c_MaxLine = 30;
      var possibleQuotes = new[] { '"', '\'' };
      var counter = new int[possibleQuotes.Length];

      var textReaderPosition = new ImprovedTextReaderPositionStore(textReader);
      var max = 0;
      // skip the first line it usually a header
      for (var lineNo = 0; lineNo < c_MaxLine && !textReaderPosition.AllRead && !cancellationToken.IsCancellationRequested; lineNo++)
      {
        var line = await textReader.ReadLineAsync().ConfigureAwait(false);
        // EOF
        if (line == null)
        {
          if (textReaderPosition.CanStartFromBeginning)
            continue;
          break;
        }

        var cols = line.Split(delimiter);
        foreach (var col in cols)
        {
          if (string.IsNullOrWhiteSpace(col))
            continue;

          var test = col.Trim();
          for (var testChar = 0; testChar < possibleQuotes.Length; testChar++)
          {
            if (test[0] != possibleQuotes[testChar]) continue;
            counter[testChar]++;
            // Ideally column need to start and end with the same characters (but end quote could be
            // on another line) if the start and end are indeed teh same give it extra credit
            if (test.Length > 1 && test[0] == test[test.Length - 1])
              counter[testChar]++;
            if (counter[testChar] > max)
              max = counter[testChar];
          }
        }
      }

      return max < 1 ? '\0' : possibleQuotes.Where((t, testChar) => counter[testChar] == max).FirstOrDefault();
    }

    /// <summary>
    ///   Guesses the start row of a CSV file Done with a rather simple csv parsing
    /// </summary>
    /// <param name="textReader">The stream reader with the data</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <param name="quoteChar">The quoting char</param>
    /// <param name="commentLine">The characters for a comment line.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The number of rows to skip</returns>
    /// <exception cref="ArgumentNullException">commentLine</exception>
    [NotNull]
    private static async Task<int> GuessStartRowAsync([NotNull] ImprovedTextReader textReader, char delimiter, char quoteChar,
      string commentLine, CancellationToken cancellationToken)
    {
      if (textReader == null) throw new ArgumentNullException(nameof(textReader));
      if (commentLine == null)
        throw new ArgumentNullException(nameof(commentLine));
      const int c_MaxRows = 50;

      textReader.ToBeginning();
      var columnCount = new List<int>(c_MaxRows);
      var rowMapping = new Dictionary<int, int>(c_MaxRows);
      var colCount = new int[c_MaxRows];
      var isComment = new bool[c_MaxRows];
      var quoted = false;
      var firstChar = true;
      var lastRow = 0;

      while (lastRow < c_MaxRows && !textReader.EndOfFile && !cancellationToken.IsCancellationRequested)
      {
        var readChar = await textReader.ReadAsync().ConfigureAwait(false);

        // Handle Commented lines
        if (firstChar && commentLine.Length > 0 && !isComment[lastRow] && readChar == commentLine[0])
        {
          isComment[lastRow] = true;

          for (var pos = 1; pos < commentLine.Length; pos++)
          {
            var nextChar = await textReader.PeekAsync().ConfigureAwait(false);
            if (nextChar == commentLine[pos]) continue;
            isComment[lastRow] = false;
            break;
          }
        }

        // Handle Quoting
        if (readChar == quoteChar && !isComment[lastRow])
        {
          if (quoted)
          {
            if (await textReader.PeekAsync().ConfigureAwait(false) != '"')
              quoted = false;
            else
              textReader.MoveNext();
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
              if (await textReader.PeekAsync().ConfigureAwait(false) == '\r')
                textReader.MoveNext();
            }

            break;

          case '\r':
            if (!quoted)
            {
              lastRow++;
              firstChar = true;
              if (await textReader.PeekAsync().ConfigureAwait(false) == '\n')
                textReader.MoveNext();
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

      cancellationToken.ThrowIfCancellationRequested();
      // remove all rows that are comment lines...
      for (var row = 0; row < lastRow; row++)
      {
        rowMapping[columnCount.Count] = row;
        if (!isComment[row])
          columnCount.Add(colCount[row]);
      }

      // if we do not more than 4 proper rows do nothing
      if (columnCount.Count < 4)
        return 0;

      // In case we have a row that is exactly twice as long as the row before and row after, assume
      // its missing a linefeed
      for (var row = 1; row < columnCount.Count - 1; row++)
        if (columnCount[row + 1] > 0 && columnCount[row] == columnCount[row + 1] * 2 &&
            columnCount[row] == columnCount[row - 1] * 2)
          columnCount[row] = columnCount[row + 1];
      cancellationToken.ThrowIfCancellationRequested();
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

      var avg = (int) (sum / (double) (num == 0 ? 1 : num));
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

    [NotNull]
    private static async Task<bool> IsJsonReadableAsync([NotNull] IImprovedStream impStream, CancellationToken cancellationToken)
    {
      impStream.ResetToStart(null);
      using (var streamReader = new StreamReader(impStream.Stream))
      using (var jsonTextReader = new JsonTextReader(streamReader))
      {
        jsonTextReader.CloseInput = false;
        try
        {
          if (await jsonTextReader.ReadAsync(cancellationToken).ConfigureAwait(false))
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
  }
}