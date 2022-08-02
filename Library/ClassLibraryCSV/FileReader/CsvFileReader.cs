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
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc cref="CsvTools.BaseFileReader" />
  /// <summary>
  ///   A data reader for CSV files
  /// </summary>
  public class CsvFileReader : BaseFileReader, IFileReaderWithEvents
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
    private const char cCr = (char) 0x0d;

    /// <summary>
    ///   The line-feed character. Escape code is <c>\n</c>.
    /// </summary>
    private const char cLf = (char) 0x0a;

    /// <summary>
    ///   A non-breaking space..
    /// </summary>
    private const char cNbsp = (char) 0xA0;

    private const char cUnknownChar = (char) 0xFFFD;

    private readonly bool m_AllowRowCombining;

    private readonly int m_CodePageId;

    private readonly string m_CommentLine;

    private readonly int m_ConsecutiveEmptyRowsMax;

    private readonly bool m_ContextSensitiveQualifier;

    private readonly string m_DelimiterPlaceholder;

    private readonly bool m_DuplicateQualifierToEscape;

    private readonly char m_EscapePrefixChar;

    private readonly char m_FieldDelimiterChar;

    private readonly char m_FieldQualifierChar;

    // This is used to report issues with columns
    private readonly Action<int, string> m_HandleMessageColumn;

    private readonly bool m_HasFieldHeader;

    private readonly bool m_HasQualifier;

    private readonly string m_IdentifierInContainer;

    private readonly string m_NewLinePlaceholder;

    private readonly int m_NumWarning;

    private readonly string m_QuotePlaceholder;

    // Store the raw text of the record, before split into columns and trimming of the columns
    private readonly StringBuilder m_RecordSource = new StringBuilder();

    private readonly bool m_SkipDuplicateHeader;

    private readonly bool m_SkipEmptyLines;

    private readonly int m_SkipRows;

    private readonly bool m_TreatLfAsSpace;

    private readonly bool m_TreatNbspAsSpace;

    private readonly string m_TreatTextAsNull;

    private readonly bool m_TreatUnknownCharacterAsSpace;

    private readonly TrimmingOptionEnum m_TrimmingOption;

    private readonly bool m_TryToSolveMoreColumns;

    private readonly bool m_WarnDelimiterInValue;

    private readonly bool m_WarnLineFeed;

    private readonly bool m_WarnNbsp;

    private readonly bool m_WarnQuotes;

    private readonly bool m_WarnUnknownCharacter;

    private int m_ConsecutiveEmptyRows;

    /// <summary>
    ///   If the End of the line is reached this is true
    /// </summary>
    private bool m_EndOfLine;

    private string[] m_HeaderRow;

    private Stream? m_Stream;

    /// <summary>
    ///   Number of Records in the text file, only set if all records have been read
    /// </summary>
    private ushort m_NumWarningsDelimiter;

    private ushort m_NumWarningsLinefeed;

    private ushort m_NumWarningsNbspChar;

    private ushort m_NumWarningsQuote;

    private ushort m_NumWarningsUnknownChar;

    private ReAlignColumns? m_RealignColumns;

    /// <summary>
    ///   The TextReader to read the file
    /// </summary>
    private ImprovedTextReader? m_TextReader;

    public CsvFileReader(in Stream stream, int codePageId, int skipRows,
      bool hasFieldHeader, in IEnumerable<IColumn>? columnDefinition, in TrimmingOptionEnum trimmingOption,
      in string fieldDelimiter, in string fieldQualifier, in string escapeCharacter,
      long recordLimit, bool allowRowCombining, bool contextSensitiveQualifier, in string commentLine,
      int numWarning, bool duplicateQualifierToEscape, in string newLinePlaceholder, in string delimiterPlaceholder,
      in string quotePlaceholder, bool skipDuplicateHeader, bool treatLfAsSpace, bool treatUnknownCharacterAsSpace,
      bool tryToSolveMoreColumns, bool warnDelimiterInValue, bool warnLineFeed, bool warnNbsp, bool warnQuotes,
      bool warnUnknownCharacter, bool warnEmptyTailingColumns, bool treatNbspAsSpace, in string treatTextAsNull,
      bool skipEmptyLines, int consecutiveEmptyRowsMax, in TimeZoneChangeDelegate timeZoneAdjust,
      in string destTimeZone)
      : this(columnDefinition, codePageId, skipRows, hasFieldHeader,
        trimmingOption, fieldDelimiter, fieldQualifier, escapeCharacter, recordLimit, allowRowCombining,
        contextSensitiveQualifier, commentLine, numWarning, duplicateQualifierToEscape, newLinePlaceholder,
        delimiterPlaceholder, quotePlaceholder, skipDuplicateHeader, treatLfAsSpace, treatUnknownCharacterAsSpace,
        tryToSolveMoreColumns, warnDelimiterInValue, warnLineFeed, warnNbsp, warnQuotes, warnUnknownCharacter,
        warnEmptyTailingColumns, treatNbspAsSpace, treatTextAsNull, skipEmptyLines, consecutiveEmptyRowsMax,
        string.Empty,
        string.Empty, timeZoneAdjust, destTimeZone)
    {
      m_Stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    public CsvFileReader(in string fileName, int codePageId, int skipRows, bool hasFieldHeader,
      in IEnumerable<IColumn>? columnDefinition,
      in TrimmingOptionEnum trimmingOption,
      in string fieldDelimiter, in string fieldQualifier, in string escapeCharacter, long recordLimit,
      bool allowRowCombining, bool contextSensitiveQualifier, in string commentLine, int numWarning,
      bool duplicateQualifierToEscape, in string newLinePlaceholder, in string delimiterPlaceholder,
      in string quotePlaceholder,
      bool skipDuplicateHeader, bool treatLfAsSpace, bool treatUnknownCharacterAsSpace, bool tryToSolveMoreColumns,
      bool warnDelimiterInValue, bool warnLineFeed, bool warnNbsp, bool warnQuotes, bool warnUnknownCharacter,
      bool warnEmptyTailingColumns, bool treatNbspAsSpace, in string treatTextAsNull, bool skipEmptyLines,
      int consecutiveEmptyRowsMax, in string identifierInContainer, in TimeZoneChangeDelegate timeZoneAdjust,
      string destTimeZone)
      : this(
        columnDefinition, codePageId, skipRows, hasFieldHeader,
        trimmingOption, fieldDelimiter, fieldQualifier, escapeCharacter, recordLimit, allowRowCombining,
        contextSensitiveQualifier, commentLine, numWarning, duplicateQualifierToEscape, newLinePlaceholder,
        delimiterPlaceholder, quotePlaceholder, skipDuplicateHeader, treatLfAsSpace, treatUnknownCharacterAsSpace,
        tryToSolveMoreColumns,
        warnDelimiterInValue, warnLineFeed, warnNbsp, warnQuotes, warnUnknownCharacter, warnEmptyTailingColumns,
        treatNbspAsSpace, treatTextAsNull, skipEmptyLines, consecutiveEmptyRowsMax, identifierInContainer, fileName,
        timeZoneAdjust, destTimeZone)
    {
      if (fileName is null)
        throw new ArgumentNullException(nameof(fileName));
      if (fileName.Trim().Length == 0)
        throw new ArgumentException("File can not be empty", nameof(fileName));
      if (!FileSystemUtils.FileExists(fileName))
        throw new FileNotFoundException(
          $"The file '{FileSystemUtils.GetShortDisplayFileName(fileName)}' does not exist or is not accessible.",
          fileName);
    }

    private CsvFileReader(IEnumerable<IColumn>? columnDefinition,
      int codePageId,
      int skipRows,
      bool hasFieldHeader,
      TrimmingOptionEnum trimmingOption,
      in string fieldDelimiter,
      in string fieldQualifier,
      in string escapePrefix,
      long recordLimit,
      bool allowRowCombining,
      bool contextSensitiveQualifier,
      in string commentLine,
      int numWarning,
      bool duplicateQualifierToEscape,
      in string newLinePlaceholder,
      in string delimiterPlaceholder,
      in string quotePlaceholder,
      bool skipDuplicateHeader,
      bool treatLfAsSpace,
      bool treatUnknownCharacterAsSpace,
      bool tryToSolveMoreColumns,
      bool warnDelimiterInValue,
      bool warnLineFeed,
      bool warnNbsp,
      bool warnQuotes,
      bool warnUnknownCharacter,
      bool warnEmptyTailingColumns,
      bool treatNbspAsSpace,
      in string treatTextAsNull,
      bool skipEmptyLines,
      int consecutiveEmptyRowsMax,
      in string identifierInContainer,
      in string fileName,
      in TimeZoneChangeDelegate timeZoneAdjust,
      in string destTimeZone)
      : base(fileName, columnDefinition, recordLimit, timeZoneAdjust, destTimeZone)
    {
      SelfOpenedStream = !string.IsNullOrEmpty(fileName);
      m_HeaderRow = Array.Empty<string>();
      m_EscapePrefixChar = escapePrefix.WrittenPunctuationToChar();
      m_FieldDelimiterChar = fieldDelimiter.WrittenPunctuationToChar();
      m_FieldQualifierChar = fieldQualifier.WrittenPunctuationToChar();

      if (m_FieldDelimiterChar == '\0')
        throw new FileReaderException("All delimited text files do need a delimiter.");

      if (m_FieldQualifierChar == cCr || m_FieldQualifierChar == cLf)
        throw new FileReaderException(
          "The text qualifier characters is invalid, please use something else than CR or LF");

      if (m_FieldDelimiterChar == cCr || m_FieldDelimiterChar == cLf || m_FieldDelimiterChar == ' ')
        throw new FileReaderException(
          "The field delimiter character is invalid, please use something else than CR, LF or Space");

      if (m_EscapePrefixChar != '\0' &&
          (m_FieldDelimiterChar == m_EscapePrefixChar || m_FieldQualifierChar == m_EscapePrefixChar))
        throw new FileReaderException(
          $"The escape character is invalid, please use something else than the field delimiter or qualifier character {m_EscapePrefixChar.GetDescription()}.");

      m_HasQualifier = m_FieldQualifierChar != '\0';

      if (m_HasQualifier && m_FieldQualifierChar == m_FieldDelimiterChar)
        throw new ArgumentOutOfRangeException(
          $"The field qualifier and the field delimiter characters of a delimited file cannot be the same character {m_FieldDelimiterChar.GetDescription()}");

      m_AllowRowCombining = allowRowCombining;
      m_ContextSensitiveQualifier = contextSensitiveQualifier;
      m_CodePageId = codePageId;
      m_CommentLine = commentLine;
      m_DelimiterPlaceholder = delimiterPlaceholder;
      m_DuplicateQualifierToEscape = duplicateQualifierToEscape;
      m_NewLinePlaceholder = newLinePlaceholder;
      m_NumWarning = numWarning;
      m_QuotePlaceholder = quotePlaceholder;
      m_SkipDuplicateHeader = skipDuplicateHeader;
      m_SkipRows = skipRows;
      m_TreatLfAsSpace = treatLfAsSpace;
      m_TreatUnknownCharacterAsSpace = treatUnknownCharacterAsSpace;
      m_TryToSolveMoreColumns = tryToSolveMoreColumns;
      m_WarnDelimiterInValue = warnDelimiterInValue;
      m_WarnLineFeed = warnLineFeed;
      m_WarnNbsp = warnNbsp;
      m_WarnQuotes = warnQuotes;
      m_WarnUnknownCharacter = warnUnknownCharacter;
      m_HasFieldHeader = hasFieldHeader;
      m_SkipEmptyLines = skipEmptyLines;
      m_ConsecutiveEmptyRowsMax = consecutiveEmptyRowsMax;
      m_TreatNbspAsSpace = treatNbspAsSpace;
      m_TrimmingOption = trimmingOption;
      m_TreatTextAsNull = treatTextAsNull;
      m_IdentifierInContainer = identifierInContainer;

      // Either we report the issues regularly or at least log it
      if (warnEmptyTailingColumns)
        m_HandleMessageColumn = HandleWarning;
      else
        // or we add them to the log
        m_HandleMessageColumn = (i, s) => Logger.Warning(GetWarningEventArgs(i, s).Display(true, true));
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets a value indicating whether this instance is closed.
    /// </summary>
    /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
    public override bool IsClosed => m_TextReader is null;

    public override void Close()
    {
      base.Close();
      m_NumWarningsQuote = 0;
      m_NumWarningsDelimiter = 0;
      m_NumWarningsUnknownChar = 0;
      m_NumWarningsNbspChar = 0;
    }

    public new void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    /// <exception cref="T:System.NotImplementedException">Always returns</exception>
    public new long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
    {
      if (GetColumn(i).ValueFormat.DataType != DataTypeEnum.Binary ||
          string.IsNullOrEmpty(CurrentRowColumnText[i])) return -1;
      using var fs = FileSystemUtils.OpenRead(CurrentRowColumnText[i]);
      if (fieldOffset > 0)
        fs.Seek(fieldOffset, SeekOrigin.Begin);
      return fs.Read(buffer, bufferoffset, length);
    }

    /// <inheritdoc />
    /// <exception cref="T:System.NotImplementedException">Always returns</exception>
    [Obsolete("Not implemented")]
    public new IDataReader GetData(int i) => throw new NotImplementedException();

    /// <inheritdoc />
    /// <summary>
    ///   Gets the data type information for the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>The .NET type name of the column</returns>
    public new string GetDataTypeName(int i) => Column[i].ValueFormat.DataType.GetNetType().Name;

    /// <inheritdoc />
    /// <summary>
    ///   Return the value of the specified field.
    /// </summary>
    /// <param name="ordinal">The index of the field to find.</param>
    /// <returns>The object will contain the field value upon return.</returns>
    /// <exception cref="T:System.IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see
    ///   cref="P:System.Data.IDataRecord.FieldCount" />.
    /// </exception>
    public override object GetValue(int ordinal)
    {
      if (IsDBNull(ordinal))
        return DBNull.Value;

      var value = CurrentRowColumnText[ordinal];
      var column = Column[ordinal];
      if (column.Ignore)
        return DBNull.Value;
      object? ret = column.ValueFormat.DataType switch
      {
        DataTypeEnum.DateTime => GetDateTimeNull(null, value, null, GetTimeValue(ordinal), column, true),
        DataTypeEnum.Integer => IntPtr.Size == 4 ? GetInt32Null(value, column) : GetInt64Null(value, column),
        DataTypeEnum.Double => GetDoubleNull(value, ordinal),
        DataTypeEnum.Numeric => GetDecimalNull(value, ordinal),
        DataTypeEnum.Boolean => GetBooleanNull(value, ordinal),
        DataTypeEnum.Guid => GetGuidNull(value, column.ColumnOrdinal),
        _ => value
      };
      return ret ?? DBNull.Value;
    }

    /// <inheritdoc cref="IFileReader.OpenAsync" />
    /// <summary>
    ///   Open the file Reader; Start processing the Headers and determine the maximum column size
    /// </summary>
    public override async Task OpenAsync(CancellationToken token)
    {
      // Logger.Information("Opening delimited file {filename}", FileName);
      await BeforeOpenAsync($"Opening delimited file \"{FileSystemUtils.GetShortDisplayFileName(FullPath)}\"")
        .ConfigureAwait(false);
      try
      {
        if (SelfOpenedStream)
        {
          if (m_Stream != null)
#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER
            await m_Stream.DisposeAsync().ConfigureAwait(false);
#else
            m_Stream.Dispose();
#endif
          m_Stream = FunctionalDI.OpenStream(new SourceAccess(FullPath)
          {
            IdentifierInContainer = m_IdentifierInContainer
          });
        }
        else
        {
          if (m_Stream is null)
            throw new FileReaderException("Stream for reading is not provided");
        }

        m_TextReader?.Dispose();
        m_TextReader = new ImprovedTextReader(
          m_Stream,
          await m_Stream.CodePageResolve(m_CodePageId, token).ConfigureAwait(false),
          m_SkipRows);

        ResetPositionToStartOrOpen();

        m_HeaderRow = ReadNextRow(false);

        InitColumn(ParseFieldCount(m_HeaderRow));

        ParseColumnName(m_HeaderRow, null, m_HasFieldHeader);

        // Turn off unescaped warning based on WarnLineFeed
        if (!m_WarnLineFeed)
          foreach (var col in Column)
            if (col.ColumnFormatter is TextUnescapeFormatter unescapeFormatter)
              unescapeFormatter.RaiseWarning = false;

        if (m_TryToSolveMoreColumns && m_FieldDelimiterChar != '\0')
          m_RealignColumns = new ReAlignColumns(FieldCount);

        if (m_TextReader.CanSeek)
          ResetPositionToFirstDataRow();

        FinishOpen();
      }
      catch (Exception ex)
      {
        if (ShouldRetry(ex, token))
        {
          await OpenAsync(token);
          return;
        }

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

    /// <inheritdoc cref="BaseFileReader" />
    public override Task<bool> ReadAsync(CancellationToken cancellationToken) => Task.FromResult(Read(cancellationToken));

    /// <inheritdoc />
    public override bool Read(CancellationToken token) => !token.IsCancellationRequested && Read();

    /// <inheritdoc cref="BaseFileReader" />
    public override bool Read()
    {
      if (!EndOfFile)
      {
        // GetNextRecordAsync does take care of RecordNumber
        var couldRead = GetNextRecord();
        InfoDisplay(couldRead);

        if (couldRead && !IsClosed && RecordNumber <= RecordLimit)
          return true;
      }

      EndOfFile = true;
      HandleReadFinished();
      return false;
    }

    /// <inheritdoc />
    public new void ResetPositionToFirstDataRow()
    {
      ResetPositionToStartOrOpen();
      if (m_HasFieldHeader)
        // Read the header row, this could be more than one line
        ReadNextRow(false);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        m_Stream?.Dispose();
        m_Stream = null;

        m_TextReader?.Dispose();
        m_TextReader = null;
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and MaxValue</returns>
    protected override double GetRelativePosition()
    {
      if (m_Stream is IImprovedStream imp)
        return imp.Percentage;

      if (RecordLimit > 0 && RecordLimit < long.MaxValue)
        // you can either reach the record limit or the end of the stream
        return Math.Max((double) RecordNumber / RecordLimit, 50);
      return 0;
    }

    private bool AllEmptyAndCountConsecutiveEmptyRows(IReadOnlyList<string?>? columns)
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

    private void EatNextCrlf(char character)
    {
      EndLineNumber++;
      if (EndOfFile) return;
      var nextChar = Peek();
      if ((character != cCr || nextChar != cLf) && (character != cLf || nextChar != cCr)) return;

      // New line sequence is either CRLF or LFCR, disregard the character
      MoveNext(nextChar);

      if (character == cLf && nextChar == cCr)
        EndLineNumber++;
    }

    /// <summary>
    ///   This does read the next record and stores it in CurrentRowColumnText /&gt;, it will handle
    ///   column mismatches
    /// </summary>
    /// <returns><c>true</c> if a new record was read</returns>
    private bool GetNextRecord()
    {
      try
      {
        bool readRowAgain;
        do
        {
          readRowAgain = false;
          CurrentRowColumnText = ReadNextRow(true);
          if (AllEmptyAndCountConsecutiveEmptyRows(CurrentRowColumnText))
          {
            if (EndOfFile)
              return false;

            // an empty line
            if (m_SkipEmptyLines)
            {
              readRowAgain = true;
              continue;
            }
          }

          if (CurrentRowColumnText.Length == FieldCount && m_HasFieldHeader && m_SkipDuplicateHeader)
          {
            var isRepeatedHeader = true;
            for (var col = 0; col < FieldCount; col++)
              if (!m_HeaderRow[col].Equals(CurrentRowColumnText[col], StringComparison.OrdinalIgnoreCase))
              {
                isRepeatedHeader = false;
                break;
              }

            if (isRepeatedHeader)
            {
              HandleWarning(-1, "Repeated Header row is ignored");
              readRowAgain = true;
            }
          }
        } while (readRowAgain);

        RecordNumber++;
        if (m_RealignColumns != null)
          m_RealignColumns.AddRow(CurrentRowColumnText);

        // Option a) Supported - We have a break in a middle column, the missing columns are pushed
        // in the next row(s) // Option b) Not Supported - We have a line break in the last column,
        // the text of this row belongs to the last Column of the last records, as the last record
        // had been processed already we can not change it any more...
        if (CurrentRowColumnText.Length < FieldCount)
        {
          // if we still have only one column and we should have a number of columns assume this was
          // nonsense like a report footer
          if (CurrentRowColumnText.Length == 1 && EndOfFile && CurrentRowColumnText[0].Length < 10)
          {
            // As the record is ignored tis will most likely not be visible
            // -2 to indicate this error could be stored with the previous line....
            m_HandleMessageColumn(-2,
              $"Last line is '{CurrentRowColumnText[0]}'. Assumed to be a EOF marker and ignored.");
            return false;
          }

          if (!m_AllowRowCombining)
          {
            m_HandleMessageColumn(-1, $"Line {cLessColumns} ({CurrentRowColumnText.Length}/{FieldCount}).");
          }
          else
          {
            var startLine = StartLineNumber;

            // get the next row
            var nextLine = ReadNextRow(true);
            StartLineNumber = startLine;

            // allow up to two extra columns they can be combined later
            if (nextLine.Length > 0 && nextLine.Length + CurrentRowColumnText.Length < FieldCount + 4)
            {
              var combined = new List<string>(CurrentRowColumnText);

              // the first column belongs to the last column of the previous ignore
              // NumWarningsLinefeed otherwise as this is important information
              m_NumWarningsLinefeed++;
              m_HandleMessageColumn(
                CurrentRowColumnText.Length - 1,
                $"Combined with line {EndLineNumber}, assuming a linefeed has split the column into additional line.");
              combined[CurrentRowColumnText.Length - 1] += '\n' + nextLine[0];

              for (var col = 1; col < nextLine.Length; col++)
                combined.Add(nextLine[col]);

              CurrentRowColumnText = combined.ToArray();
            }

            // we have an issue we went into the next Buffer there is no way back.
            HandleError(-1,
              $"Line {cLessColumns}\nAttempting to combined lines some line(s) have been read this information is now lost, please turn off Row Combination");
          }
        }

        // If more columns are present
        if (CurrentRowColumnText.Length > FieldCount)
        {
          var text = $"Line {cMoreColumns} ({CurrentRowColumnText.Length}/{FieldCount}).";

          if (m_RealignColumns != null)
          {
            m_HandleMessageColumn(-1, text + " Trying to realign columns.");

            // determine which column could have caused the issue it could be any column, try to establish
            CurrentRowColumnText = m_RealignColumns.RealignColumn(
              CurrentRowColumnText,
              m_HandleMessageColumn,
              m_RecordSource.ToString());
          }
          else
          {
            // check if the additional columns have contents
            var hasContent = false;
            for (var extraCol = FieldCount; extraCol < CurrentRowColumnText.Length; extraCol++)
            {
              if (string.IsNullOrEmpty(CurrentRowColumnText[extraCol]))
                continue;
              hasContent = true;
              break;
            }

            if (!hasContent)
            {
              m_HandleMessageColumn(
                -1,
                text + " All additional columns where empty. Allow 're-align columns' to handle this.");
              return true;
            }

            m_HandleMessageColumn(
              -1,
              text + " The data in extra columns is not read. Allow 're-align columns' to handle this.");
          }
        }

        // now handle Text replacements and warning in the read columns
        for (var columnNo = 0; columnNo < FieldCount && columnNo < CurrentRowColumnText.Length; columnNo++)
        {
          if (GetColumn(columnNo).Ignore || string.IsNullOrEmpty(CurrentRowColumnText[columnNo]))
            continue;

          // Handle replacements and warnings etc,
          var adjustedValue = HandleTextSpecials(
            CurrentRowColumnText[columnNo].ReplaceCaseInsensitive(m_NewLinePlaceholder, Environment.NewLine)
              .ReplaceCaseInsensitive(m_DelimiterPlaceholder, m_FieldDelimiterChar)
              .ReplaceCaseInsensitive(m_QuotePlaceholder, m_FieldQualifierChar),
            columnNo);

          if (adjustedValue.Length > 0)
          {
            if (m_WarnQuotes && adjustedValue.IndexOf(m_FieldQualifierChar) != -1 &&
                (m_NumWarning < 1 || m_NumWarningsQuote++ < m_NumWarning))
              HandleWarning(columnNo,
                $"Field qualifier '{m_FieldQualifierChar.GetDescription()}' found in field".AddWarningId());

            if (m_WarnDelimiterInValue && adjustedValue.IndexOf(m_FieldDelimiterChar) != -1 &&
                (m_NumWarning < 1 || m_NumWarningsDelimiter++ < m_NumWarning))
              HandleWarning(columnNo,
                $"Field delimiter '{m_FieldDelimiterChar.GetDescription()}' found in field".AddWarningId());

            if (m_WarnUnknownCharacter)
            {
              var numberQuestionMark = 0;
              var lastPos = -1;
              var length = adjustedValue.Length;
              for (var pos = length - 1; pos >= 0; pos--)
              {
                if (adjustedValue[pos] != '?')
                  continue;
                numberQuestionMark++;

                // If we have at least two and there are two consecutive or more than 3+ in 12
                // characters, or 4+ in 16 characters
                if (numberQuestionMark > 2 && (lastPos == pos + 1 || numberQuestionMark > length / 4))
                {
                  if (m_NumWarning < 1 || m_NumWarningsUnknownChar++ < m_NumWarning)
                    HandleWarning(columnNo,
                      "Unusual high occurrence of ? this indicates unmapped characters.".AddWarningId());
                  break;
                }

                lastPos = pos;
              }
            }

            if (m_WarnLineFeed && (adjustedValue.IndexOf('\r') != -1 || adjustedValue.IndexOf('\n') != -1))
              WarnLinefeed(columnNo);

            if (StringUtils.ShouldBeTreatedAsNull(adjustedValue, m_TreatTextAsNull))
              adjustedValue = string.Empty;
          }

          CurrentRowColumnText[columnNo] = adjustedValue;
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

    /// <summary>
    ///   Move to next character and store the recent character
    /// </summary>
    /// <param name="current">The recent character that has been read</param>
    private void MoveNext(char current)
    {
      if (m_RealignColumns != null)
        m_RecordSource.Append(current);
      m_TextReader?.MoveNext();
    }

    /// <summary>
    ///   Determine the number of columns in the file
    /// </summary>
    /// <param name="headerRow">The initial header</param>
    /// <returns>Number of columns</returns>
    /// <remarks>
    ///   If seek is supported, it will parse a few extra rows to check if teh header and the
    ///   following rows do result in teh same number of columns
    /// </remarks>
    private int ParseFieldCount(IReadOnlyList<string> headerRow)
    {
      if (headerRow.Count == 0 || string.IsNullOrEmpty(headerRow[0]))
        return 0;

      var fields = headerRow.Count;

      // The last column is empty but we expect a header column, assume if a trailing separator
      if (fields <= 1)
        return fields;

      // check if the next lines do have data in the last column
      for (var additional = 0; (m_TextReader?.CanSeek ?? false) && !EndOfFile && additional < 10; additional++)
      {
        var nextLine = ReadNextRow(false);

        // if we have less columns than in the header exit the loop
        if (nextLine.GetLength(0) < fields)
          break;

        // special case of missing linefeed, the line is twice as long minus 1 because of the
        // combined column in the middle
        if (nextLine.Length == (fields * 2) - 1)
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
    private char Peek()
    {
      var res = m_TextReader!.Peek();
      if (res != -1)
        return (char) res;
      EndOfFile = true;

      // return a linefeed to determine the end of a line
      return cLf;
    }

    private char ReadChar()
    {
      if (m_RealignColumns != null)
      {
        var character = Peek();
        m_RecordSource.Append(character);
        m_TextReader!.MoveNext();
        return character;
      }
      var res = m_TextReader!.Read();
      if (res != -1)
        return (char) res;
      EndOfFile = true;
      // return a linefeed to determine the end of a line
      return cLf;
    }

    /// <summary>
    ///   Gets the next column in the buffer.
    /// </summary>
    /// <param name="columnNo">The column number for warnings</param>
    /// <returns>The next column null after the last column</returns>
    /// <remarks>
    ///   If NULL is returned we are at the end of the file, an empty column is read as empty string
    /// </remarks>
    private string? ReadNextColumn(int columnNo)
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
      var quoted = false;
      var preData = true;
      var postData = false;
      var escaped = false;
      while (!EndOfFile)
      {
        // Read a character
        var character = ReadChar();

        // in case we have a single LF
        if (!postData && m_TreatLfAsSpace && character == cLf && quoted)
        {
          var singleLf = true;
          if (!EndOfFile)
          {
            var nextChar = Peek();
            if (nextChar == cCr)
              singleLf = false;
          }

          if (singleLf)
          {
            character = ' ';
            EndLineNumber++;
            if (m_WarnLineFeed)
              WarnLinefeed(columnNo);
          }
        }

        switch (character)
        {
          case cNbsp:
            if (!postData)
            {
              // This is not 100% correct in case we have a misalignment of column that is corrected
              // afterwards warning for NBP need to be issues before trimming as trimming would
              // remove the char
              if (m_WarnNbsp && columnNo < FieldCount && !GetColumn(columnNo).Ignore &&
                  (m_NumWarning < 1 || m_NumWarningsNbspChar++ < m_NumWarning))
                HandleWarning(
                  columnNo,
                  m_TreatNbspAsSpace
                    ? "Character Non Breaking Space found, this character was treated as space".AddWarningId()
                    : "Character Non Breaking Space found in field".AddWarningId());

              if (m_TreatNbspAsSpace)
                character = ' ';
            }

            break;

          case cCr:
          case cLf:
            EndLineNumber++;

            var nextChar = '\0';
            if (!EndOfFile)
            {
              nextChar = Peek();
              if ((character != cCr || nextChar != cLf) && (character != cLf || nextChar != cCr))
              {
                nextChar = '\0';
              }
              else
              {
                MoveNext(nextChar);

                if (character == cLf && nextChar == cCr)
                  EndLineNumber++;
              }
            }

            if (((character == cCr && nextChar == cLf) || (character == cLf && nextChar == cCr)) && quoted && !postData)
            {
              stringBuilder.Append(character);
              stringBuilder.Append(nextChar);
              continue;
            }

            break;
        }

        // Finished with reading the column by Delimiter or EOF
        if ((character == m_FieldDelimiterChar && !escaped && (postData || !quoted)) || EndOfFile)
          break;

        // Finished with reading the column by Linefeed
        if ((character == cCr || character == cLf) && (preData || postData || !quoted))
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
            if (m_TrimmingOption == TrimmingOptionEnum.None)
              // Values will be trimmed later but we need to find out, if the filed is quoted first
              stringBuilder.Append(character);
            continue;
          }

          // data is starting
          preData = false;

          // Can not be escaped here
          if (m_HasQualifier && character == m_FieldQualifierChar && !escaped)
          {
            if (m_TrimmingOption != TrimmingOptionEnum.None)
              stringBuilder.Length = 0;

            // quoted data is starting
            quoted = true;
            continue;
          }
          else
          {
            goto append;
          }
        }

        if (m_HasQualifier && character == m_FieldQualifierChar && quoted && !escaped)
        {
          var peekNextChar = Peek();

          // a "" should be regarded as " if the text is quoted
          if (m_DuplicateQualifierToEscape && peekNextChar == m_FieldQualifierChar)
          {
            // double quotes within quoted string means add a quote
            stringBuilder.Append(m_FieldQualifierChar);
            MoveNext(peekNextChar);

            // handling for "" that is not only representing a " but also closes the text
            peekNextChar = Peek();
            if (m_ContextSensitiveQualifier && (peekNextChar == m_FieldDelimiterChar
                                                || peekNextChar == cCr || peekNextChar == cLf)) postData = true;
            continue;
          }

          // a single " should be regarded as closing when its followed by the delimiter
          if (m_ContextSensitiveQualifier && (peekNextChar == m_FieldDelimiterChar
                                              || peekNextChar == cCr || peekNextChar == cLf))
          {
            postData = true;
            continue;
          }

          // a single " should be regarded as closing if we do not have alternate qualifier
          if (!m_ContextSensitiveQualifier)
          {
            postData = true;
            continue;
          }
        }

        append:
        if (escaped && (character == m_FieldQualifierChar || character == m_FieldDelimiterChar ||
                        character == m_EscapePrefixChar))
          // remove the already added escape char
          stringBuilder.Length--;

        // all cases covered, character must be data
        stringBuilder.Append(character);

        escaped = !escaped && character == m_EscapePrefixChar;
      } // While

      var columnText = stringBuilder.ToString();
      if (columnText.IndexOf(cUnknownChar) != -1)
      {
        if (m_WarnUnknownCharacter && (m_NumWarning < 1 || m_NumWarningsUnknownChar++ < m_NumWarning))
          HandleWarning(
            columnNo,
            m_TreatUnknownCharacterAsSpace
              ? "Unknown Character '�' found, this character was replaced with space".AddWarningId()
              : "Unknown Character '�' found in field".AddWarningId());
        if (m_TreatUnknownCharacterAsSpace)
          columnText = columnText.Replace(cUnknownChar, ' ');
      }

      return m_TrimmingOption == TrimmingOptionEnum.All || (!quoted && m_TrimmingOption == TrimmingOptionEnum.Unquoted)
        ? columnText.Trim()
        : columnText;
    }

    /// <summary>
    ///   Reads the record of the CSV file, this can span over multiple lines
    /// </summary>
    /// <param name="storeWarnings">Set to <c>true</c> if the warnings should be issued.</param>
    /// <returns>
    ///   <c>NULL</c> if the row can not be read, array of string values representing the columns of
    ///   the row
    /// </returns>
    private string[] ReadNextRow(bool storeWarnings)
    {
      bool restart;
      // Special handling for the first column in the row
      string? item;
      do
      {
        restart = false;
        // Store the starting Line Number
        StartLineNumber = EndLineNumber;
        if (m_RealignColumns != null)
          m_RecordSource.Length = 0;

        // If already at end of file, return null
        if (EndOfFile || m_TextReader is null)
          return new string[FieldCount];

        item = ReadNextColumn(0);

        // An empty line does not have a first column
        if ((item is null || item.Length == 0) && m_EndOfLine)
        {
          m_EndOfLine = false;
          if (m_SkipEmptyLines)
            // go to the next line
            restart = true;
          else
            // Return it as array of empty columns
            return new string[FieldCount];
        }
        // And skip commented lines
        else if (m_CommentLine.Length > 0 && item != null && item.StartsWith(m_CommentLine, StringComparison.Ordinal))
        {
          // A commented line does start with the comment
          if (m_EndOfLine)
            m_EndOfLine = false;
          else
            // it might happen that the comment line contains a Delimiter
            while (!EndOfFile)
            {
              var character = Peek();
              MoveNext(character);
              if (character != cCr && character != cLf)
                continue;
              EatNextCrlf(character);
              break;
            }

          restart = true;
        }
      } while (restart);

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
        columns.Add(item);

        col++;
        item = ReadNextColumn(col);
      }

      return columns.ToArray();
    }

    /// <summary>
    ///   Resets the position and buffer to the first line, excluding headers, use
    ///   ResetPositionToStart if you want to go to first data line
    /// </summary>
    private void ResetPositionToStartOrOpen()
    {
      if (m_TextReader!.CanSeek)
        m_TextReader.ToBeginning();

      EndLineNumber = 1 + m_SkipRows;
      StartLineNumber = EndLineNumber;
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

#if NETSTANDARD2_1 || NETSTANDARD2_1_OR_GREATER

    public new async ValueTask DisposeAsync()
    {
      await DisposeAsyncCore();

      Dispose(false);
      GC.SuppressFinalize(this);
    }

    protected async ValueTask DisposeAsyncCore()
    {
      if (m_Stream != null)
        await m_Stream.DisposeAsync().ConfigureAwait(false);
    }

#endif
  }
}