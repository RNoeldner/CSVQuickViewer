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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace CsvTools
{
  /// <summary>
  ///   Helper class
  /// </summary>
  public static class CsvHelper
  {
    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    /// <summary>
    /// Caches the column header.
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="includeIgnored">if set to <c>true</c> [include ignored].</param>
    /// <param name="columns">The columns.</param>
    public static void CacheColumnHeader(IFileSetting fileSetting, bool includeIgnored, ICollection<string> columns)
    {
      Contract.Requires(fileSetting != null);
      var key = CacheListKeyColumnHeader(fileSetting.ID, includeIgnored);
      if (key.Length > 2)
        ApplicationSetting.CacheList.Set(key, columns);
    }

    /// <summary>
    /// Gets the column header of a file
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="includeIgnored">Set to <c>true</c> if ignored columns should be listed as well, otherwise they will not be
    /// listed</param>
    /// <param name="processDisplay">The process display.</param>
    /// <returns>
    /// An array of string with the column headers
    /// </returns>
    public static ICollection<string> GetColumnHeader(IFileSetting fileSetting, bool includeIgnored, bool openIfNeeded, IProcessDisplay processDisplay)
    {
      Contract.Requires(fileSetting != null);

      var key = CacheListKeyColumnHeader(fileSetting.ID, includeIgnored);

      if (key.Length > 3 && ApplicationSetting.CacheList.TryGet(key, out var retValue))
        return retValue;

      if (!openIfNeeded)
        return null;

      using (var fileReader = fileSetting.GetFileReader())
      {
        fileReader.ProcessDisplay = processDisplay;
        fileReader.Open(false, processDisplay?.CancellationToken ?? CancellationToken.None);
        // if teh key was long enough it has been stored
        if (key.Length > 3)
          return ApplicationSetting.CacheList.Get(key);
        else
        {
          var header = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
          for (var colindex = 0; colindex < fileReader.FieldCount; colindex++)
          {
            var col = fileReader.GetColumn(colindex);
            if (includeIgnored || !col.Ignore)
              header.Add(col.Name);
          }
          return header;
        }
      }
    }

    public static ICollection<string> GetColumnHeadersFromReader(IFileReader fileReader, bool includeIgnored)
    {
      Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
      var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      if (fileReader == null) return values;
      for (var colindex = 0; colindex < fileReader.FieldCount; colindex++)
      {
        var cf = fileReader.GetColumn(colindex);
        if (!string.IsNullOrEmpty(cf.Name) && (includeIgnored || !cf.Ignore))
          values.Add(cf.Name);
      }

      return values;
    }


    /// <summary>
    ///   Get sample values for a column
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="columnName">the Name of the column</param>
    /// <returns>
    ///   The index of the given name, -1 if not found.
    /// </returns>
    public static int GetColumnIndex(IFileSetting fileSetting, string columnName, bool openIfNeeded = false)
    {
      if (string.IsNullOrEmpty(columnName) || fileSetting == null) return -1;
      var columnIndex = 0;
      var headers = GetColumnHeader(fileSetting, true, openIfNeeded, null);
      if (headers != null)
      {
        foreach (var col in headers)
        {
          if (col.Equals(columnName, StringComparison.OrdinalIgnoreCase))
            return columnIndex;
          columnIndex++;
        }
      }
      return -1;
    }

    /// <summary>
    ///   Gets the column header of a file
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    /// <param name="processDisplay">The get process display.</param>
    /// <returns>
    ///   An array of string with the column headers where the column is empty
    /// </returns>
    public static string[] GetEmptyColumnHeader(IFileSetting fileSetting, IProcessDisplay processDisplay)
    {
      Contract.Requires(fileSetting != null);
      Contract.Requires(processDisplay != null);
      Contract.Ensures(Contract.Result<string[]>() != null);
      var emptyColumns = new List<string>();
      if (!fileSetting.HasFieldHeader) return emptyColumns.ToArray();
      using (var fileReader = fileSetting.GetFileReader())
      {
        Contract.Assume(fileReader != null);
        fileReader.ProcessDisplay = processDisplay;
        fileReader.Open(true, processDisplay.CancellationToken);

        if (fileSetting is CsvFile)
        {
          for (var column = 0; column < fileReader.FieldCount; column++)
          {
            var col = fileReader.GetColumn(column);
            if (col.Size == 0)
              emptyColumns.Add(col.Name);
          }
        }
        else
        {
          var columnHasData = new HashSet<int>();
          for (var row = 0; row < 2000 && fileReader.Read(); row++)
          {
            for (var column = 0; column < fileReader.FieldCount; column++)
            {
              if (columnHasData.Contains(column)) continue;
              if (fileReader[column].ToString().Length > 0)
                columnHasData.Add(column);
            }

            if (columnHasData.Count == fileReader.FieldCount)
              break;
          }

          for (var column = 0; column < fileReader.FieldCount; column++)
            if (!columnHasData.Contains(column))
              emptyColumns.Add(fileReader.GetName(column));
        }
      }

      return emptyColumns.ToArray();
    }

    /// <summary>
    /// Gets the <see cref="Encoding"/> of the textFile
    /// </summary>
    /// <param name="setting">The setting.</param>
    /// <returns></returns>
    public static Encoding GetEncoding(this ICsvFile setting)
    {
      Contract.Requires(setting != null);
      if (setting.CodePageId < 0)
        GuessCodePage(setting);

      return Encoding.GetEncoding(setting.CodePageId);
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

      Log.Info("Detected Code Page: " + EncodingHelper.GetEncodingName(detected, true, setting.ByteOrderMark));
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
        for (int i = 0; i < setting.SkipRows; i++)
          streamReader.ReadLine();
        return GuessDelimiter(streamReader, setting.FileFormat.EscapeCharacterChar);
      }
    }

    /// <summary>
    /// Opens the csv file, and tries to read the headers
    /// </summary>
    /// <param name="setting">The CSVFile fileSetting</param>
    /// <param name="processDisplay">The process display.</param>
    /// <returns>
    ///   <c>True</c> we could use the first row as header, <c>false</c> should not use first row as header
    /// </returns>
    public static bool GuessHasHeader(ICsvFile setting, CancellationToken cancellationToken)
    {
      Contract.Requires(setting != null);
      // Only do so if HasFieldHeader is still true
      if (!setting.HasFieldHeader)
      {
        Log.Info("Without Header Row");
        return false;
      }

      using (var csvDataReader = new CsvFileReader(setting))
      {
        csvDataReader.Open(false, cancellationToken);

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
              Log.Info("Without Header Row");
              return false;
            }

          // if its a number assume no headers
          if (StringConversion.StringToDecimal(columnName, '.', ',', false).HasValue)
          {
            Log.Info("Without Header Row");
            return false;
          }

          // if its rather long assume no header
          if (columnName.Length > 80)
          {
            Log.Info("Without Header Row");
            return false;
          }

        }
        Log.Info("With Header Row");
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
        for (int i = 0; i < setting.SkipRows; i++)
          streamReader.ReadLine();
        return GuessNewline(streamReader, setting.FileFormat.FieldQualifierChar);
      }
    }

    /// <summary>
    ///   Guesses the not a delimited file.
    /// </summary>
    /// <param name="setting"><see cref="ICsvFile" /> with the information</param>
    /// <returns><c>true</c> if this is most likely not a delimited file</returns>
    public static bool GuessNotADelimitedFile(ICsvFile setting)
    {
      Contract.Requires(setting != null);
      using (var improvedStream = ImprovedStream.OpenRead(setting))
      using (var streamReader = new StreamReader(improvedStream.Stream, setting.GetEncoding(), setting.ByteOrderMark))
      {
        for (int i = 0; i < setting.SkipRows; i++)
          streamReader.ReadLine();
        // If the file doe not have a good delimiter
        // has empty lines
        var dc = GetDelimiterCounter(streamReader, '\0', 300);

        // Have a proper delimiter
        for (var sep = 0; sep < dc.Separators.Length; sep++)
          if (dc.SeparatorRows[sep] >= dc.LastRow * 9 / 10)
          {
            Log.Info("Not a delimited file");
            return false;
          }
      }
      Log.Info("Delimited file");
      return true;
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
        return GuessStartRow(streamReader, setting.FileFormat.FieldDelimiterChar,
         setting.FileFormat.FieldQualifierChar);
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
        for (int i = 0; i < setting.SkipRows; i++)
          streamReader.ReadLine();
        var buff = new char[262144];
        var isStartOfColumn = true;
        while (!streamReader.EndOfStream)
        {
          var read = streamReader.ReadBlock(buff, 0, 262143);

          // Look for Delimiter [Whitespace] Qualifier or StartofLine [Whitespace] Qualifier
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
    /// Invalidate the column header cache of a file
    /// </summary>
    /// <param name="fileSetting">The file setting.</param>
    public static void InvalidateColumnHeader(IFileSetting fileSetting)
    {
      Contract.Requires(fileSetting != null);
      if (fileSetting.InternalID.Length > 0)
      {
        ApplicationSetting.CacheList.Remove(CacheListKeyColumnHeader(fileSetting.ID, true));
        ApplicationSetting.CacheList.Remove(CacheListKeyColumnHeader(fileSetting.ID, false));
      }
    }

    /// <summary>
    /// Refreshes the settings assuming the file has changed, checks CodePage, Delimiter, Start Row and Header
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="display">The display.</param>
    public static void RefreshCsvFile(this ICsvFile file, IProcessDisplay display)
    {
      Contract.Requires(file != null);
      Contract.Requires(display != null);

      var root = ApplicationSetting.ToolSetting.RootFolder;
      file.FileName.GetAbsolutePath(root);

      display.SetProcess("Checking delimited file");
      GuessCodePage(file);
      if (display.CancellationToken.IsCancellationRequested) return;
      display.SetProcess("Code Page: " +
                EncodingHelper.GetEncodingName(file.CurrentEncoding.CodePage, true, file.ByteOrderMark));

      file.FileFormat.FieldDelimiter = GuessDelimiter(file);
      if (display.CancellationToken.IsCancellationRequested) return;
      display.SetProcess("Delimiter: " + file.FileFormat.FieldDelimiter);

      file.SkipRows = GuessStartRow(file);
      if (display.CancellationToken.IsCancellationRequested) return;
      if (file.SkipRows > 0)
        display.SetProcess("Start Row: " + file.SkipRows.ToString(CultureInfo.InvariantCulture));

      file.HasFieldHeader = GuessHasHeader(file, display.CancellationToken);
      display.SetProcess("Column Header: " + file.HasFieldHeader);
    }

    /// <summary>
    ///   Guesses the delimiter for a files.
    ///   Done with a rather simple csv parsing, and trying to find the delimiter that has the least variance in the read rows,
    ///   if that is not possible the delimiter with the highest number of occurrences.
    /// </summary>
    /// <param name="streamReader">The StreamReader with the data</param>
    /// <param name="escapeCharacter">The escape character.</param>
    /// <returns>
    ///   A character with the assumed delimiter for the file
    /// </returns>
    /// <remarks>
    ///   No Error will not be thrown.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Body")]
    internal static string GuessDelimiter(StreamReader streamReader, char escapeCharacter)
    {
      Contract.Ensures(Contract.Result<string>() != null);
      var match = '\t';
      if (streamReader == null)
      {
        Log.Info("File not read, assumed Delimiter: TAB");
        return "TAB";
      }

      var dc = GetDelimiterCounter(streamReader, escapeCharacter, 300);

      // Limit everything to 100 columns max, the sum might get too big otherwise 100 * 100
      var startRow = dc.LastRow > 60 ? 15 :
       dc.LastRow > 20 ? 5 : 0;
      for (var index = 0; index < dc.Separators.Length; index++)
      {
        if (dc.SeparatorRows[index] < 1)
          continue;
        for (var row = startRow; row < dc.LastRow; row++)
          if (dc.SeparatorsCount[index, row] > 100)
            dc.SeparatorsCount[index, row] = 100;
      }

      double? bestScore = null;
      var maxCount = 0;

      for (var index = 0; index < dc.Separators.Length; index++)
      {
        if (dc.SeparatorRows[index] < 1)
          continue;

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
          else if (dist == 2 || dist == -2)
            cutVariance += 4;
          else if (dist == 1 || dist == -1)
            cutVariance++;
        }

        // The score is dependent on the average columns found and the regularity
        var score = avg - Math.Round(cutVariance / (dc.LastRow - startRow), 2);
        if (bestScore.HasValue && !(score > bestScore.Value)) continue;
        match = dc.Separators[index];
        bestScore = score;
      }
      var ret = match == '\t' ? "TAB" : match.ToString(CultureInfo.CurrentCulture);
      Log.Info("Delimiter: " + ret);
      return ret;
    }

    internal static string GuessNewline(StreamReader streamReader, char fieldQualifier)
    {
      Contract.Requires(streamReader != null);
      Contract.Ensures(Contract.Result<string>() != null);
      const int numRows = 50;

      var lastRow = 0;
      var readChar = 0;
      var quoted = false;

      const int cr = 0;
      const int lf = 1;
      const int crlf = 2;
      const int lfcr = 3;
      int[] count = { 0, 0, 0, 0 };

      // \r = CR (Carriage Return) \n = LF (Line Feed)

      while (lastRow < numRows && readChar >= 0)
      {
        readChar = streamReader.Read();
        if (readChar == fieldQualifier)
          if (quoted)
            if (streamReader.Peek() != fieldQualifier)
              quoted = false;
            else
              streamReader.Read();
          else
            quoted = true;

        if (quoted) continue;
        if (readChar == '\n')
        {
          if (streamReader.Peek() == '\r')
          {
            streamReader.Read();
            count[lfcr]++;
          }
          else
          {
            count[lf]++;
          }

          lastRow++;
        }

        if (readChar != '\r') continue;
        if (streamReader.Peek() == '\n')
        {
          streamReader.Read();
          count[crlf]++;
        }
        else
        {
          count[cr]++;
        }

        lastRow++;
      }

      if (count[cr] > count[crlf] && count[cr] > count[lfcr] && count[cr] > count[lf])
        return "CR";
      if (count[lf] > count[crlf] && count[lf] > count[lfcr] && count[lf] > count[cr])
        return "LF";
      if (count[lfcr] > count[crlf] && count[lfcr] > count[lf] && count[lfcr] > count[cr])
        return "LFCR";

      return "CRLF";
    }

    /// <summary>
    ///   Guesses the start row of a CSV file Done with a rather simple csv parsing
    /// </summary>
    /// <param name="streamReader">The stream reader with the data</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <param name="quoteChar">The quoting char</param>
    /// <returns>The number of rows to skip</returns>
    internal static int GuessStartRow(StreamReader streamReader, char delimiter, char quoteChar)
    {
      Contract.Ensures(Contract.Result<int>() >= 0);
      const int maxRows = 50;
      if (streamReader == null)
        return 0;

      var columnCount = new int[maxRows];
      var quoted = false;
      var firstChar = true;
      var lastRow = 0;

      while (lastRow < maxRows && streamReader.Peek() >= 0)
      {
        var readChar = (char)streamReader.Read();
        if (readChar == quoteChar)
        {
          if (quoted)
            if (streamReader.Peek() != '"')
              quoted = false;
            else
              streamReader.Read();
          else
            quoted |= firstChar;
          continue;
        }

        switch (readChar)
        {
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
            if (!quoted && readChar == delimiter)
            {
              columnCount[lastRow]++;
              firstChar = true;
              continue;
            }

            break;
        }

        // Its still the first char if its a leading space
        if (firstChar && readChar != ' ')
          firstChar = false;
      }

      // if we do not more than 4 rows we can stop
      if (lastRow < 4)
        return 0;

      // Get the average of the last 15 rows
      var num = 0;
      var sum = 0;
      for (var row = lastRow - 3; num < 10 && row > 0; row--)
      {
        if (columnCount[row] <= 0) continue;
        sum += columnCount[row];
        num++;
      }

      var avg = (int)(sum / (double)(num == 0 ? 1 : num));
      // If there are not many columns do not try to guess
      if (avg <= 1) return 0;
      {
        // If the first rows would be a good fit return this
        if (columnCount[0] >= avg)
          return 0;

        for (var row = lastRow - 3; row > 0; row--)
          if (columnCount[row] > 0)
          {
            if (columnCount[row] < avg - 1)
            {
              Log.Info($"Start Row: {row}");
              return row;
            }

          }
          // In case we have an empty line but the next line are roughly good match take that empty line
          else if (columnCount[row + 1] == columnCount[row + 2]
               && columnCount[row + 1] >= avg - 1)
          {
            Log.Info($"Start Row: {row + 1}");
            return row + 1;
          }

        for (var row = 0; row < lastRow; row++)
          if (columnCount[row] > 0)
          {
            Log.Info($"Start Row: {row}");
            return row;
          }

      }
      return 0;
    }

    public static string CacheListKeyColumnHeader(string tableName, bool evenIgnored)
    {
      return (evenIgnored ? "A:" : "I:") + tableName;
    }

    private static DelimiterCounter GetDelimiterCounter(StreamReader streamReader, char escapeCharacter, int numRows)
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
              if (streamReader.Peek() != '"')
                quoted = false;
              else
                streamReader.Read();
            else
              quoted |= firstChar;
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
          var csls = CultureInfo.CurrentCulture.TextInfo.ListSeparator[0];
          if (c_DefaultSeparators.IndexOf(csls) == -1)
            Separators = c_DefaultSeparators + csls;
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