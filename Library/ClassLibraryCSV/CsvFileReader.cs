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
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CsvTools
{
  /// <summary>
  ///   A data reader for CSV files
  /// </summary>
  public sealed class CsvFileReader : BaseFileReader, IFileReader
  {
    /// <summary>
    ///   Constant: Line has fewer columns than expected
    /// </summary>
    public const string cLessColumns = " has fewer columns than expected";

    /// <summary>
    ///   Constant: Line has more columns than expected
    /// </summary>
    public const string cMoreColumns = " has more columns than expected";

    /// <summary>
    ///   The carriage return character. Escape code is <c>\r</c>.
    /// </summary>
    private const char c_Cr = (char) 0x0d;

    /// <summary>
    ///   The line-feed character. Escape code is <c>\n</c>.
    /// </summary>
    private const char c_Lf = (char) 0x0a;

    /// <summary>
    ///   A non-breaking space..
    /// </summary>
    private const char c_Nbsp = (char) 0xA0;

    private const char c_UnknownChar = (char) 0xFFFD;
    private readonly bool m_AllowRowCombining;
    private readonly bool m_AlternateQuoting;
    private readonly int m_CodePageId;
    private readonly string m_CommentLine;
    private readonly int m_ConsecutiveEmptyRowsMax;
    private readonly string m_DelimiterPlaceholder;
    private readonly bool m_DuplicateQuotingToEscape;
    private readonly char m_EscapeCharacterChar;

    private readonly char m_FieldDelimiterChar;
    private readonly char m_FieldQualifierChar;

    // This is used to report issues with columns
    private readonly Action<int, string> m_HandleMessageColumn;
    private readonly bool m_HasFieldHeader;

    private readonly bool m_HasQualifier;
    private readonly string m_NewLinePlaceholder;
    private readonly int m_NumWarning;
    private readonly string m_QuotePlaceholder;

    // Store teh raw text of the record, before split into columns and trimming of the columns
    private readonly StringBuilder m_RecordSource = new StringBuilder();
    private readonly bool m_SkipDuplicateHeader;
    private readonly bool m_SkipEmptyLines;
    private readonly int m_SkipRows;

    private readonly bool m_TreatLFAsSpace;
    private readonly bool m_TreatNBSPAsSpace;
    private readonly string m_TreatTextAsNull;
    private readonly bool m_TreatUnknownCharacterAsSpace;
    private readonly TrimmingOption m_TrimmingOption;
    private readonly bool m_TryToSolveMoreColumns;
    private readonly bool m_WarnDelimiterInValue;
    private readonly bool m_WarnLineFeed;

    private readonly bool m_WarnNBSP;
    private readonly bool m_WarnQuotes;
    private readonly bool m_WarnUnknownCharacter;

    private int m_ConsecutiveEmptyRows;

    /// <summary>
    ///   If the End of the line is reached this is true
    /// </summary>
    private bool m_EndOfLine;

    private string[] m_HeaderRow;

    private IImprovedStream m_ImprovedStream;

    /// <summary>
    ///   Number of Records in the text file, only set if all records have been read
    /// </summary>
    private ushort m_NumWarningsDelimiter;

    private ushort m_NumWarningsLinefeed;

    private ushort m_NumWarningsNbspChar;

    private ushort m_NumWarningsQuote;

    private ushort m_NumWarningsUnknownChar;

    private ReAlignColumns m_RealignColumns;

    /// <summary>
    ///   The TextReader to read the file
    /// </summary>
    private ImprovedTextReader m_TextReader;

    public CsvFileReader([NotNull] string fileName,
      int codePageId = 650001,
      int skipRows = 0,
      bool hasFieldHeader = true,
      [CanBeNull] IEnumerable<IColumn> columnDefinition = null,
      TrimmingOption trimmingOption = TrimmingOption.Unquoted,
      string fieldDelimiter = "\t", string fieldQualifier = "\"", char escapeCharacterChar = '\0',
      long recordLimit = 0,
      bool allowRowCombining = false, bool alternateQuoting = false,
      [NotNull] string commentLine = "", int numWarning = 0, bool duplicateQuotingToEscape = true,
      string newLinePlaceholder = "", string delimiterPlaceholder = "", string quotePlaceholder = "",
      bool skipDuplicateHeader = true,
      bool treatLFAsSpace = false, bool treatUnknownCharacterAsSpace = false, bool tryToSolveMoreColumns = true,
      bool warnDelimiterInValue = true,
      bool warnLineFeed = false, bool warnNBSP = true, bool warnQuotes = true, bool warnUnknownCharacter = true,
      bool warnEmptyTailingColumns = true,
      bool treatNBSPAsSpace = false,
      string treatTextAsNull = BaseSettings.cTreatTextAsNull, bool skipEmptyLines = true,
      int consecutiveEmptyRowsMax = 4)
      : base(fileName, columnDefinition, recordLimit)
    {
      if (string.IsNullOrEmpty(fileName))
        throw new ArgumentException("Path can not be null or empty", nameof(fileName));

      m_EscapeCharacterChar = escapeCharacterChar;
      m_FieldDelimiterChar = fieldDelimiter.WrittenPunctuationToChar();
      if (fieldDelimiter.Length > 1 && m_FieldDelimiterChar == '\0')
      {
        Logger.Warning($"Only the first character of {fieldDelimiter} is used as delimiter.", fieldDelimiter);
        m_FieldDelimiterChar = fieldDelimiter[0];
      }

      m_FieldQualifierChar = fieldQualifier.WrittenPunctuationToChar();
      if (fieldQualifier.Length > 1 && m_FieldQualifierChar == '\0')
      {
        Logger.Warning($"Only the first character of {fieldQualifier} is be used for quoting.", fieldQualifier);
        m_FieldQualifierChar = fieldQualifier[0];
      }

      if (m_FieldQualifierChar == c_Cr || m_FieldQualifierChar == c_Lf)
        throw new FileReaderException(
          "The text quoting characters is invalid, please use something else than CR or LF");

      if (m_FieldDelimiterChar == c_Cr || m_FieldDelimiterChar == c_Lf
                                       || m_FieldDelimiterChar == ' '
                                       || m_FieldDelimiterChar == '\0')
        throw new FileReaderException(
          "The field delimiter character is invalid, please use something else than CR, LF or Space");

      if (m_FieldDelimiterChar == m_EscapeCharacterChar)
        throw new FileReaderException(
          $"The escape character is invalid, please use something else than the field delimiter character {FileFormat.GetDescription(m_EscapeCharacterChar.ToString())}.");

      m_HasQualifier = m_FieldQualifierChar != '\0';

      if (m_HasQualifier && m_FieldQualifierChar == m_FieldDelimiterChar)
        throw new ArgumentOutOfRangeException(
          $"The text quoting and the field delimiter characters of a delimited file cannot be the same. {m_FieldDelimiterChar}");

      m_AllowRowCombining = allowRowCombining;
      m_AlternateQuoting = alternateQuoting;
      m_CodePageId = codePageId;
      m_CommentLine = commentLine;
      m_DelimiterPlaceholder = delimiterPlaceholder;
      m_DuplicateQuotingToEscape = duplicateQuotingToEscape;
      m_NewLinePlaceholder = newLinePlaceholder;
      m_NumWarning = numWarning;
      m_QuotePlaceholder = quotePlaceholder;
      m_SkipDuplicateHeader = skipDuplicateHeader;
      m_SkipRows = skipRows;
      m_TreatLFAsSpace = treatLFAsSpace;
      m_TreatUnknownCharacterAsSpace = treatUnknownCharacterAsSpace;
      m_TryToSolveMoreColumns = tryToSolveMoreColumns;
      m_WarnDelimiterInValue = warnDelimiterInValue;
      m_WarnLineFeed = warnLineFeed;
      m_WarnNBSP = warnNBSP;
      m_WarnQuotes = warnQuotes;
      m_WarnUnknownCharacter = warnUnknownCharacter;
      m_HasFieldHeader = hasFieldHeader;
      m_SkipEmptyLines = skipEmptyLines;
      m_ConsecutiveEmptyRowsMax = consecutiveEmptyRowsMax;
      m_TreatNBSPAsSpace = treatNBSPAsSpace;
      m_TrimmingOption = trimmingOption;
      m_TreatTextAsNull = treatTextAsNull;

      // Either we report the issues regularly or at least log it
      if (warnEmptyTailingColumns)
        m_HandleMessageColumn = HandleWarning;
      else
        // or we add them to the log
        m_HandleMessageColumn = (i, s) => Logger.Warning(GetWarningEventArgs(i, s).Display(true, true));
    }

    /// <summary>
    ///   Create a delimited text reader for teh given settings
    /// </summary>
    /// <param name="fileSetting"></param>
    /// <param name="processDisplay">Progress and Cancellation</param>
    public CsvFileReader([NotNull] ICsvFile fileSetting,
      [CanBeNull] IProcessDisplay processDisplay)
      : this(fileSetting.FullPath, fileSetting.CodePageId,
        fileSetting.SkipRows, fileSetting.HasFieldHeader,
        fileSetting.ColumnCollection, fileSetting.TrimmingOption, fileSetting.FileFormat.FieldDelimiter,
        fileSetting.FileFormat.FieldQualifier,
        fileSetting.FileFormat.EscapeCharacterChar, fileSetting.RecordLimit, fileSetting.AllowRowCombining,
        fileSetting.FileFormat.AlternateQuoting, fileSetting.FileFormat.CommentLine, fileSetting.NumWarnings,
        fileSetting.FileFormat.DuplicateQuotingToEscape,
        fileSetting.FileFormat.NewLinePlaceholder,
        fileSetting.FileFormat.DelimiterPlaceholder,
        fileSetting.FileFormat.QuotePlaceholder, fileSetting.SkipDuplicateHeader,
        fileSetting.TreatLFAsSpace,
        fileSetting.TreatUnknownCharacterAsSpace,
        fileSetting.TryToSolveMoreColumns,
        fileSetting.WarnDelimiterInValue, fileSetting.WarnLineFeed,
        fileSetting.WarnNBSP, fileSetting.WarnQuotes,
        fileSetting.WarnUnknownCharacter,
        fileSetting.WarnEmptyTailingColumns, fileSetting.TreatNBSPAsSpace,
        fileSetting.TreatTextAsNull, fileSetting.SkipEmptyLines,
        fileSetting.ConsecutiveEmptyRows)
    {
      if (processDisplay == null) return;
      ReportProgress = processDisplay.SetProcess;
      SetMaxProcess = l => processDisplay.Maximum = l;
      SetMaxProcess(0);
    }

    /// <summary>
    ///   Gets a value indicating whether this instance is closed.
    /// </summary>
    /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
    public bool IsClosed => m_TextReader == null;

    public override void Close()
    {
      base.Close();
      m_NumWarningsQuote = 0;
      m_NumWarningsDelimiter = 0;
      m_NumWarningsUnknownChar = 0;
      m_NumWarningsNbspChar = 0;

      m_TextReader?.Dispose();
      m_TextReader = null;
      m_ImprovedStream?.Dispose();
      m_ImprovedStream = null;
    }

    /// <summary>
    ///   Reads a stream of bytes from the specified column offset into the buffer as an array,
    ///   starting at the given buffer offset.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
    /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
    /// <param name="bufferOffset">
    ///   The index for <paramref name="buffer" /> to start the read operation.
    /// </param>
    /// <param name="length">The number of bytes to read.</param>
    /// <exception cref="NotImplementedException"></exception>
    /// <returns>The actual number of bytes read.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length) =>
      throw new NotImplementedException();

    /// <summary>
    ///   Returns an <see cref="IDataReader" /> for the specified column ordinal.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <exception cref="NotImplementedException"></exception>
    /// <returns>An <see cref="IDataReader" />.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public IDataReader GetData(int i) => throw new NotImplementedException();

    /// <summary>
    ///   Gets the data type information for the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>The .NET type name of the column</returns>
    public string GetDataTypeName(int i) => Column[i].ValueFormat.DataType.GetNetType().Name;

    /// <summary>
    ///   Return the value of the specified field.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>The object will contain the field value upon return.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public override object GetValue(int columnNumber)
    {
      if (IsDBNull(columnNumber))
        return DBNull.Value;

      return GetTypedValueFromString(
        CurrentRowColumnText[columnNumber],
        GetTimeValue(columnNumber),
        GetColumn(columnNumber));
    }

    /// <summary>
    ///   Open the file Reader; Start processing the Headers and determine the maximum column size
    /// </summary>
    public override async Task OpenAsync(CancellationToken token)
    {
      //if (m_FieldQualifier.Length > 1
      //    && m_FieldQualifier.WrittenPunctuationToChar() == '\0')
      //  HandleWarning(
      //    -1,
      //    $"Only the first character of '{m_FieldQualifier}' is be used for quoting.");
      //if (m_FieldDelimiter.Length > 1
      //    && m_FieldDelimiter.WrittenPunctuationToChar() == '\0')
      //  HandleWarning(-1, $"Only the first character of '{m_FieldDelimiter}' is used as delimiter.");

      await BeforeOpenAsync($"Opening delimited file {FileSystemUtils.GetShortDisplayFileName(FileName, 80)}")
        .ConfigureAwait(false);
      Retry:
      try
      {
        HandleShowProgress($"Opening text file {FileName}");

        m_ImprovedStream?.Dispose();
        m_ImprovedStream = FunctionalDI.OpenRead(FullPath);
        m_TextReader?.Dispose();
        m_TextReader = new ImprovedTextReader(m_ImprovedStream, m_CodePageId, m_SkipRows);

        ResetPositionToStartOrOpen();

        m_HeaderRow = await ReadNextRowAsync(false, false).ConfigureAwait(false);
        InitColumn(await ParseFieldCountAsync(m_HeaderRow).ConfigureAwait(false));
        ParseColumnName(m_HeaderRow, null, m_HasFieldHeader);

        FinishOpen();

        await ResetPositionToFirstDataRowAsync(token).ConfigureAwait(false);

        if (m_TryToSolveMoreColumns && m_FieldDelimiterChar != '\0')
          m_RealignColumns = new ReAlignColumns(FieldCount);
      }
      catch (Exception ex)
      {
        if (ShouldRetry(ex, token))
          goto Retry;

        Close();
        var appEx = new FileReaderException(
          "Error opening text file for reading.\nPlease make sure the file does exist, is of the right type and is not locked by another process.",
          ex);
        HandleError(-1, appEx.ExceptionMessages());
        HandleReadFinished();
        throw appEx;
      }
      finally
      {
        HandleShowProgress(string.Empty);
      }
    }

    public override async Task<bool> ReadAsync(CancellationToken token)
    {
      if (!EndOfFile && !token.IsCancellationRequested)
      {
        // GetNextRecordAsync does take care of RecordNumber
        var couldRead = await GetNextRecordAsync().ConfigureAwait(false);
        InfoDisplay(couldRead);

        if (couldRead && !IsClosed && RecordNumber <= RecordLimit)
          return true;
      }

      EndOfFile = true;
      HandleReadFinished();
      return false;
    }

    /// <summary>
    ///   Resets the position and buffer to the header in case the file has a header
    /// </summary>
    /// <param name="token"></param>
    public new async Task ResetPositionToFirstDataRowAsync(CancellationToken token)
    {
      ResetPositionToStartOrOpen();
      if (m_HasFieldHeader)
        // Read the header row, this could be more than one line
        await ReadNextRowAsync(false, false).ConfigureAwait(false);
    }

    public void Dispose() => Close();

    /// <summary>
    ///   Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and MaxValue</returns>
    protected override int GetRelativePosition()
    {
      // if we know how many records to read, use that
      if (RecordLimit > 0)
        return base.GetRelativePosition();

      return (int) ((m_ImprovedStream?.Percentage ?? 0) * cMaxValue);
    }

    private bool AllEmptyAndCountConsecutiveEmptyRows(IReadOnlyList<string> columns)
    {
      if (columns != null)
      {
        var rowLength = columns.Count;
        for (var col = 0; col < rowLength && col < FieldCount; col++)
          if (!string.IsNullOrEmpty(columns[col]))
          {
            m_ConsecutiveEmptyRows = 0;
            return false;
          }
      }

      m_ConsecutiveEmptyRows++;
      EndOfFile |= m_ConsecutiveEmptyRows >= m_ConsecutiveEmptyRowsMax;
      return true;
    }

    private async Task EatNextCRLFAsync(char character)
    {
      EndLineNumber++;
      if (EndOfFile) return;
      var nextChar = await PeekAsync().ConfigureAwait(false);
      if ((character != c_Cr || nextChar != c_Lf) && (character != c_Lf || nextChar != c_Cr)) return;

      // New line sequence is either CRLF or LFCR, disregard the character
      MoveNext(nextChar);

      if (character == c_Lf && nextChar == c_Cr)
        EndLineNumber++;
    }

    /// <summary>
    ///   Indicates whether the specified Unicode character is categorized as white space.
    /// </summary>
    /// <param name="c">A Unicode character.</param>
    /// <returns><c>true</c> if the character is a whitespace</returns>
    private bool IsWhiteSpace(char c)
    {
      // Handle cases where the delimiter is a whitespace (e.g. tab)
      if (c == m_FieldDelimiterChar)
        return false;

      // See char.IsLatin1(char c) in Reflector
      if (c <= '\x00ff')
        return c == ' ' || c == '\t';

      return CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator;
    }

    private async Task<int> ParseFieldCountAsync(IList<string> headerRow)
    {
      if (headerRow == null || headerRow.Count == 0 || string.IsNullOrEmpty(headerRow[0]))
        return 0;

      var fields = headerRow.Count;

      // The last column is empty but we expect a header column, assume if a trailing separator
      if (fields <= 1)
        return fields;

      // check if the next lines do have data in the last column
      for (var additional = 0; !EndOfFile && additional < 10; additional++)
      {
        var nextLine = await ReadNextRowAsync(false, false).ConfigureAwait(false);

        // if we have less columns than in the header exit the loop
        if (nextLine.GetLength(0) < fields)
          break;

        // special case of missing linefeed, the line is twice as long minus 1 because of the
        // combined column in the middle
        if (nextLine.Length == fields * 2 - 1)
          continue;

        while (nextLine.GetLength(0) > fields)
        {
          HandleWarning(fields, $"No header for last {nextLine.GetLength(0) - fields} column(s)".AddWarningId());
          fields++;
        }

        // if we have data in the column assume the header was missing
        if (!string.IsNullOrEmpty(nextLine[fields - 1]))
          return fields;
      }

      // ReSharper disable once InvertIf
      if (string.IsNullOrEmpty(headerRow[headerRow.Count - 1]))
      {
        HandleWarning(
          fields,
          "The last column does not have a column name and seems to be empty, this column will be ignored."
            .AddWarningId());
        return fields - 1;
      }

      return fields;
    }

    /// <summary>
    ///   Gets the next char from the buffer, but stay at the current position
    /// </summary>
    /// <returns>The next char</returns>
    private async Task<char> PeekAsync()
    {
      var res = await m_TextReader.PeekAsync().ConfigureAwait(false);
      if (res != -1) return (char) res;
      EndOfFile = true;

      // return a lf to determine the end of quoting easily
      return c_Lf;
    }

    /// <summary>
    ///   Move to next character and store the recent character
    /// </summary>
    /// <param name="current">The recent character that has been read</param>
    private void MoveNext(char current)
    {
      if (m_RealignColumns != null)
        m_RecordSource.Append(current);
      m_TextReader.MoveNext();
    }

    /// <summary>
    ///   Gets the next column in the buffer.
    /// </summary>
    /// <param name="columnNo">The column number for warnings</param>
    /// <param name="storeWarnings">If <c>true</c> warnings will be added</param>
    /// <returns>The next column null after the last column</returns>
    /// <remarks>
    ///   If NULL is returned we are at the end of the file, an empty column is read as empty string
    /// </remarks>
    private async Task<string> ReadNextColumnAsync(int columnNo, bool storeWarnings)
    {
      if (EndOfFile)
        return null;
      if (m_EndOfLine)
      {
        // previous item was last in line, start new line
        m_EndOfLine = false;
        return null;
      }

      var stringBuilder = new StringBuilder(5);
      var hadDelimiterInValue = false;
      var hadUnknownChar = false;
      var hadNbsp = false;
      var quoted = false;
      var preData = true;
      var postData = false;

      while (!EndOfFile)
      {
        // Increase position
        var character = await PeekAsync().ConfigureAwait(false);
        MoveNext(character);

        var escaped = character == m_EscapeCharacterChar && !postData;

        // Handle escaped characters
        if (escaped)
        {
          var nextChar = await PeekAsync().ConfigureAwait(false);
          if (!EndOfFile)
          {
            MoveNext(nextChar);
            switch (m_EscapeCharacterChar)
            {
              // Handle \ Notation of common not visible characters
              case '\\' when nextChar == 'n':
                character = '\n';
                break;

              case '\\' when nextChar == 'r':
                character = '\r';
                break;

              case '\\' when nextChar == 't':
                character = '\t';
                break;

              case '\\' when nextChar == 'b':
                character = '\b';
                break;

              // in case a linefeed actually follows ignore the EscapeCharacterChar but handle the
              // regular processing
              case '\\' when nextChar == 'a':
                character = '\a';
                break;

              default:
                character = nextChar;
                break;
            }
          }
        }

        // in case we have a single LF
        if (!postData && m_TreatLFAsSpace && character == c_Lf && quoted)
        {
          var singleLF = true;
          if (!EndOfFile)
          {
            var nextChar = await PeekAsync().ConfigureAwait(false);
            if (nextChar == c_Cr)
              singleLF = false;
          }

          if (singleLF)
          {
            character = ' ';
            EndLineNumber++;
            if (m_WarnLineFeed)
              WarnLinefeed(columnNo);
          }
        }

        switch (character)
        {
          case c_Nbsp:
            if (!postData)
            {
              hadNbsp = true;
              if (m_TreatNBSPAsSpace)
                character = ' ';
            }

            break;

          case c_UnknownChar:
            if (!postData)
            {
              hadUnknownChar = true;
              if (m_TreatUnknownCharacterAsSpace)
                character = ' ';
            }

            break;

          case c_Cr:
          case c_Lf:
            EndLineNumber++;

            var nextChar = '\0';
            if (!EndOfFile)
            {
              nextChar = await PeekAsync();
              if ((character != c_Cr || nextChar != c_Lf) && (character != c_Lf || nextChar != c_Cr))
              {
                nextChar = '\0';
              }
              else
              {
                MoveNext(nextChar);

                if (character == c_Lf && nextChar == c_Cr)
                  EndLineNumber++;
              }
            }

            if (character == c_Cr && nextChar == c_Lf || character == c_Lf && nextChar == c_Cr)
              if (quoted && !postData)
              {
                stringBuilder.Append(character);
                stringBuilder.Append(nextChar);
                continue;
              }

            break;
        }

        // Finished with reading the column by Delimiter or EOF
        if (character == m_FieldDelimiterChar && !escaped && (postData || !quoted) || EndOfFile)
          break;

        // Finished with reading the column by Linefeed
        if ((character == c_Cr || character == c_Lf) && (preData || postData || !quoted))
        {
          m_EndOfLine = true;
          break;
        }

        // Only check the characters if not past end of data
        if (postData)
          continue;

        if (preData)
        {
          // whitespace preceding data
          if (IsWhiteSpace(character))
          {
            // Store the white spaces if we do any kind of trimming
            if (m_TrimmingOption == TrimmingOption.None)

              // Values will be trimmed later but we need to find out, if the filed is quoted first
              stringBuilder.Append(character);
            continue;
          }

          // data is starting
          preData = false;

          // Can not be escaped here
          if (m_HasQualifier && character == m_FieldQualifierChar && !escaped)
          {
            if (m_TrimmingOption != TrimmingOption.None)
              stringBuilder.Length = 0;

            // quoted data is starting
            quoted = true;
          }
          else
          {
            stringBuilder.Append(character);
          }

          continue;
        }

        if (m_HasQualifier && character == m_FieldQualifierChar && quoted && !escaped)
        {
          var peekNextChar = await PeekAsync().ConfigureAwait(false);

          // a "" should be regarded as " if the text is quoted
          if (m_DuplicateQuotingToEscape && peekNextChar == m_FieldQualifierChar)
          {
            // double quotes within quoted string means add a quote
            stringBuilder.Append(m_FieldQualifierChar);
            MoveNext(peekNextChar);

            // handling for "" that is not only representing a " but also closes the text
            peekNextChar = await PeekAsync().ConfigureAwait(false);
            if (m_AlternateQuoting && (peekNextChar == m_FieldDelimiterChar
                                       || peekNextChar == c_Cr
                                       || peekNextChar == c_Lf)) postData = true;
            continue;
          }

          // a single " should be regarded as closing when its followed by the delimiter
          if (m_AlternateQuoting && (peekNextChar == m_FieldDelimiterChar
                                     || peekNextChar == c_Cr || peekNextChar == c_Lf))
          {
            postData = true;
            continue;
          }

          // a single " should be regarded as closing if we do not have alternate quoting
          if (!m_AlternateQuoting)
          {
            postData = true;
            continue;
          }
        }

        hadDelimiterInValue |= character == m_FieldDelimiterChar;

        // all cases covered, character must be data
        stringBuilder.Append(character);
      }

      var returnValue = stringBuilder.ToString();
      if (stringBuilder.Length > 0)
      {
        if (m_TreatNBSPAsSpace)
          returnValue = returnValue.Replace((char) 0xA0, ' ');

        if (m_TrimmingOption == TrimmingOption.All || !quoted && m_TrimmingOption == TrimmingOption.Unquoted)
          returnValue = returnValue.Trim();
      }

      if (!storeWarnings)
        return returnValue;

      // if (m_HasQualifier && quoted && m_StructuredWriterFile.WarnQuotesInQuotes &&
      // returnValue.IndexOf(m_StructuredWriterFile.FileFormat.FieldQualifierChar) != -1) WarnQuoteInQuotes(columnNo);
      if (m_HasQualifier && m_WarnQuotes && returnValue.IndexOf(m_FieldQualifierChar) > 0)
      {
        m_NumWarningsQuote++;
        if (m_NumWarning < 1 || m_NumWarningsQuote <= m_NumWarning)
          HandleWarning(
            columnNo,
            $"Field qualifier '{FileFormat.GetDescription(m_FieldQualifierChar.ToString())}' found in field"
              .AddWarningId());
      }

      if (m_WarnDelimiterInValue && hadDelimiterInValue)
      {
        m_NumWarningsDelimiter++;
        if (m_NumWarning < 1 || m_NumWarningsDelimiter <= m_NumWarning)
          HandleWarning(
            columnNo,
            $"Field delimiter '{FileFormat.GetDescription(m_FieldDelimiterChar.ToString())}' found in field"
              .AddWarningId());
      }

      if (m_WarnUnknownCharacter)
        if (hadUnknownChar)
        {
          WarnUnknownChar(columnNo, false);
        }
        else
        {
          var numberQuestionMark = 0;
          var lastPos = -1;
          var length = returnValue.Length;
          for (var pos = length - 1; pos >= 0; pos--)
          {
            if (returnValue[pos] != '?')
              continue;
            numberQuestionMark++;

            // If we have at least two and there are two consecutive or more than 3+ in 12
            // characters, or 4+ in 16 characters
            if (numberQuestionMark > 2 && (lastPos == pos + 1 || numberQuestionMark > length / 4))
            {
              WarnUnknownChar(columnNo, true);
              break;
            }

            lastPos = pos;
          }
        }

      if (m_WarnNBSP && hadNbsp)
      {
        m_NumWarningsNbspChar++;
        if (m_NumWarning < 1 || m_NumWarningsNbspChar <= m_NumWarning)
          HandleWarning(
            columnNo,
            m_TreatNBSPAsSpace
              ? "Character Non Breaking Space found, this character was treated as space".AddWarningId()
              : "Character Non Breaking Space found in field".AddWarningId());
      }

      if (m_WarnLineFeed && (returnValue.IndexOf('\r') != -1 || returnValue.IndexOf('\n') != -1))
        WarnLinefeed(columnNo);
      return returnValue;
    }

    /// <summary>
    ///   Reads the record of the CSV file, this can span over multiple lines
    /// </summary>
    /// <param name="regularDataRow">
    ///   Set to <c>true</c> if its not the header row and the maximum size should be determined.
    /// </param>
    /// <param name="storeWarnings">Set to <c>true</c> if the warnings should be issued.</param>
    /// <returns>
    ///   <c>NULL</c> if the row can not be read, array of string values representing the columns of
    ///   the row
    /// </returns>
    private async Task<string[]> ReadNextRowAsync(bool regularDataRow, bool storeWarnings)
    {
      Restart:

      // Store the starting Line Number
      StartLineNumber = EndLineNumber;
      if (m_RealignColumns != null)
        m_RecordSource.Length = 0;

      // If already at end of file, return null
      if (EndOfFile || m_TextReader == null)
        return null;

      var item = await ReadNextColumnAsync(0, storeWarnings).ConfigureAwait(false);

      // An empty line does not have any data
      if (string.IsNullOrEmpty(item) && m_EndOfLine)
      {
        m_EndOfLine = false;
        if (m_SkipEmptyLines || !regularDataRow)

          // go to the next line
          goto Restart;

        // Return it as array of empty columns
        return new string[FieldCount];
      }

      // Skip commented lines
      if (m_CommentLine.Length > 0 && !string.IsNullOrEmpty(item) &&
          item.StartsWith(m_CommentLine, StringComparison.Ordinal))
      {
        // A commented line does start with the comment
        if (m_EndOfLine)
          m_EndOfLine = false;
        else
          // it might happen that the comment line contains a Delimiter
          while (!EndOfFile)
          {
            var character = await PeekAsync().ConfigureAwait(false);
            MoveNext(character);
            if (character != c_Cr && character != c_Lf)
              continue;
            await EatNextCRLFAsync(character);
            break;
          }

        goto Restart;
      }

      var col = 0;
      var columns = new List<string>(FieldCount);

      while (item != null)
      {
        // If a column is quoted and does contain the delimiter and linefeed, issue a warning, we
        // might have an opening delimiter with a missing closing delimiter
        if (storeWarnings && EndLineNumber > StartLineNumber + 4 && item.Length > 1024
            && item.IndexOf(m_FieldDelimiterChar) != -1)
          HandleWarning(
            col,
            $"Column has {EndLineNumber - StartLineNumber + 1} lines and has a length of {item.Length} characters"
              .AddWarningId());

        if (item.Length == 0)
        {
          item = null;
        }
        else
        {
          if (StringUtils.ShouldBeTreatedAsNull(item, m_TreatTextAsNull))
          {
            item = null;
          }
          else
          {
            item = item
              .ReplaceCaseInsensitive(m_NewLinePlaceholder, Environment.NewLine)
              .ReplaceCaseInsensitive(m_DelimiterPlaceholder, m_FieldDelimiterChar)
              .ReplaceCaseInsensitive(m_QuotePlaceholder, m_FieldQualifierChar);

            if (regularDataRow && col < FieldCount)
              item = HandleText(item, col, m_TreatNBSPAsSpace, m_TreatTextAsNull, m_TrimmingOption);
          }
        }

        columns.Add(item);

        col++;
        item = await ReadNextColumnAsync(col, storeWarnings).ConfigureAwait(false);
      }

      return columns.ToArray();
    }

    // This does read the next records, it will handle column mismatches
    private async Task<bool> GetNextRecordAsync()
    {
      try
      {
        Restart:
        CurrentRowColumnText = await ReadNextRowAsync(true, true).ConfigureAwait(false);

        if (AllEmptyAndCountConsecutiveEmptyRows(CurrentRowColumnText))
        {
          if (EndOfFile)
            return false;

          // an empty line
          if (m_SkipEmptyLines)
            goto Restart;
        }

        RecordNumber++;
        var hasWarningCombinedWarning = false;
        Restart2:
        var rowLength = CurrentRowColumnText.Length;
        if (rowLength == FieldCount)
        {
          // Check if we have row that matches the header row
          if (m_HeaderRow != null && m_HasFieldHeader && !hasWarningCombinedWarning)
          {
            var isRepeatedHeader = true;
            for (var col = 0; col < FieldCount; col++)
              if (!m_HeaderRow[col].Equals(CurrentRowColumnText[col], StringComparison.OrdinalIgnoreCase))
              {
                isRepeatedHeader = false;
                break;
              }

            if (isRepeatedHeader && m_SkipDuplicateHeader)
            {
              RecordNumber--;
              goto Restart;
            }

            if (m_RealignColumns != null && !isRepeatedHeader)
              m_RealignColumns.AddRow(CurrentRowColumnText);
          }
        }
        // If less columns are present
        else if (rowLength < FieldCount)
        {
          // if we still have only one column and we should have a number of columns assume this was
          // nonsense like a report footer
          if (rowLength == 1 && EndOfFile && CurrentRowColumnText[0].Length < 10)
          {
            // As the record is ignored tis will most likely not be visible
            // -2 to indicate this error could be stored with teh previous line....
            m_HandleMessageColumn(-2,
              $"Last line is '{CurrentRowColumnText[0]}'. Assumed to be a EOF marker and ignored.");
            return false;
          }

          if (!m_AllowRowCombining)
          {
            m_HandleMessageColumn(-1, $"Line {cLessColumns} ({rowLength}/{FieldCount}).");
          }
          else
          {
            var oldPos = m_TextReader.BufferPos;
            var startLine = StartLineNumber;

            // get the next row
            var nextLine = await ReadNextRowAsync(true, true).ConfigureAwait(false);
            StartLineNumber = startLine;

            // allow up to two extra columns they can be combined later
            if (nextLine != null && nextLine.Length > 0 && nextLine.Length + rowLength < FieldCount + 4)
            {
              var combined = new List<string>(CurrentRowColumnText);

              // the first column belongs to the last column of the previous ignore
              // NumWarningsLinefeed otherwise as this is important information
              m_NumWarningsLinefeed++;
              m_HandleMessageColumn(rowLength - 1,
                $"Added first column from line {EndLineNumber}, assuming a linefeed has split the rows into an additional line.");
              combined[rowLength - 1] += '\n' + nextLine[0];

              for (var col = 1; col < nextLine.Length; col++)
                combined.Add(nextLine[col]);

              if (!hasWarningCombinedWarning)
              {
                m_HandleMessageColumn(-1,
                  $"Line {cLessColumns}\nLines {StartLineNumber}-{EndLineNumber - 1} have been combined.");
                hasWarningCombinedWarning = true;
              }

              CurrentRowColumnText = combined.ToArray();
              goto Restart2;
            }

            // we have an issue we went into the next Buffer there is no way back.
            if (m_TextReader.BufferPos < oldPos)
            {
              HandleError(
                -1,
                $"Line {cLessColumns}\nAttempting to combined lines some line have been read that is now lost, please turn off Row Combination");
            }
            else
            {
              // return to the old position so reading the next row did not matter
              if (!hasWarningCombinedWarning)
                m_HandleMessageColumn(-1, $"Line {cLessColumns} ({rowLength}/{FieldCount}).");
              m_TextReader.BufferPos = oldPos;
            }
          }
        }

        // If more columns are present
        // ReSharper disable once InvertIf
        if (rowLength > FieldCount)
        {
          var text = $"Line {cMoreColumns} ({rowLength}/{FieldCount}).";

          if (m_RealignColumns != null)
          {
            m_HandleMessageColumn(-1, text + " Trying to realign columns.");

            // determine which column could have caused the issue it could be any column, try to establish
            CurrentRowColumnText =
              m_RealignColumns.RealignColumn(CurrentRowColumnText, m_HandleMessageColumn, m_RecordSource.ToString());
          }
          else
          {
            // check if the additional columns have contents
            var hasContent = false;
            for (var extraCol = FieldCount; extraCol < rowLength; extraCol++)
            {
              if (string.IsNullOrEmpty(CurrentRowColumnText[extraCol]))
                continue;
              hasContent = true;
              break;
            }

            if (!hasContent)
            {
              m_HandleMessageColumn(-1,
                text + " All additional columns where empty. Allow 're-align columns' to handle this.");
              return true;
            }

            m_HandleMessageColumn(-1,
              text + " The data in extra columns is not read. Allow 're-align columns' to handle this.");
          }
        }

        return true;
      }
      catch (Exception ex)
      {
        HandleError(-1, ex.Message);

        EndOfFile = true;
        return false;
      }
    }

    /// <summary>
    ///   Resets the position and buffer to the first line, excluding headers, use
    ///   ResetPositionToStart if you want to go to first data line
    /// </summary>
    private void ResetPositionToStartOrOpen()
    {
      m_TextReader.ToBeginning();

      StartLineNumber = 1 + m_SkipRows;
      EndLineNumber = 1 + m_SkipRows;
      RecordNumber = 0;
      m_EndOfLine = false;
      EndOfFile = false;
    }

    /// <summary>
    ///   Add warnings for Linefeed.
    /// </summary>
    /// <param name="column">The column.</param>
    private void WarnLinefeed(int column)
    {
      m_NumWarningsLinefeed++;
      if (m_NumWarning >= 1 && m_NumWarningsLinefeed > m_NumWarning)
        return;
      HandleWarning(column, "Linefeed found in field".AddWarningId());
    }

    /// <summary>
    ///   Add warnings for unknown char.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="questionMark"></param>
    private void WarnUnknownChar(int column, bool questionMark)
    {
      // in case the column is ignored do not warn
      if (Column[column].Ignore)
        return;

      m_NumWarningsUnknownChar++;
      if (m_NumWarning >= 1 && m_NumWarningsUnknownChar > m_NumWarning)
        return;
      if (questionMark)
      {
        HandleWarning(column, "Unusual high occurrence of ? this indicates unmapped characters.".AddWarningId());
        return;
      }

      HandleWarning(
        column,
        m_TreatUnknownCharacterAsSpace
          ? "Unknown Character '�' found, this character was replaced with space".AddWarningId()
          : "Unknown Character '�' found in field".AddWarningId());
    }
  }
}