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

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc cref="CsvTools.BaseFileReader" />
  /// <summary>
  ///   A data reader for CSV files
  /// </summary>
  public class CsvFileReader : BaseFileReader
  {
    /// <summary>
    ///   Constant: Line has fewer columns than expected
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public const string cLessColumns = " has fewer columns than expected";

    /// <summary>
    ///   Constant: Line has more columns than expected
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
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

    private readonly char m_EscapePrefix;

    private readonly char m_FieldDelimiter;

    private readonly char m_FieldQualifier;

    private readonly bool m_HasFieldHeader;

    private readonly string m_IdentifierInContainer;

    private readonly string m_NewLinePlaceholder;

    private readonly int m_NumWarning;

    private readonly string m_QuotePlaceholder;

    // Store the raw text of the record, before split into columns and trimming of the columns
    private readonly StringBuilder m_RecordSource = new StringBuilder();

    private readonly bool m_SkipDuplicateHeader;

    private readonly bool m_SkipEmptyLines;

    private readonly int m_SkipRows;

    private readonly bool m_TreatLinefeedAsSpace;

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
    private readonly bool m_WarnEmptyTailingColumns;
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

    /// <inheritdoc />
    public CsvFileReader(in Stream stream, int codePageId, int skipRows,
      bool hasFieldHeader, in IEnumerable<Column>? columnDefinition, in TrimmingOptionEnum trimmingOption,
      char fieldDelimiter, char fieldQualifier, char escapeCharacter,
      long recordLimit, bool allowRowCombining, bool contextSensitiveQualifier, in string commentLine,
      int numWarning, bool duplicateQualifierToEscape, in string newLinePlaceholder, in string delimiterPlaceholder,
      in string quotePlaceholder, bool skipDuplicateHeader, bool treatLinefeedAsSpace,
      bool treatUnknownCharacterAsSpace,
      bool tryToSolveMoreColumns, bool warnDelimiterInValue, bool warnLineFeed, bool warnNbsp, bool warnQuotes,
      bool warnUnknownCharacter, bool warnEmptyTailingColumns, bool treatNbspAsSpace, in string treatTextAsNull,
      bool skipEmptyLines, int consecutiveEmptyRowsMax, in TimeZoneChangeDelegate timeZoneAdjust,
      in string returnedTimeZone, bool allowPercentage, bool removeCurrency)
      : this(columnDefinition, codePageId, skipRows, hasFieldHeader,
        trimmingOption, fieldDelimiter, fieldQualifier, escapeCharacter, recordLimit, allowRowCombining,
        contextSensitiveQualifier, commentLine, numWarning, duplicateQualifierToEscape, newLinePlaceholder,
        delimiterPlaceholder, quotePlaceholder, skipDuplicateHeader, treatLinefeedAsSpace, treatUnknownCharacterAsSpace,
        tryToSolveMoreColumns, warnDelimiterInValue, warnLineFeed, warnNbsp, warnQuotes, warnUnknownCharacter,
        warnEmptyTailingColumns, treatNbspAsSpace, treatTextAsNull, skipEmptyLines, consecutiveEmptyRowsMax,
        string.Empty, string.Empty, timeZoneAdjust, returnedTimeZone, allowPercentage, removeCurrency)
    {
      m_Stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvFileReader"/> class.
    /// </summary>
    /// <param name="fileName">Fully qualified path of the file to write</param>
    /// <param name="codePageId">The code page identifier. UTF8 is 65001</param>
    /// <param name="skipRows">Number of rows that should be ignored in the beginning, e.G. for information not related  to the data</param>
    /// <param name="hasFieldHeader">If set to <c>true</c> assume the name of the columns is in the first read row</param>
    /// <param name="columnDefinition">The column definition for value conversion.</param>
    /// <param name="trimmingOption">How should leading/trailing spaces be trimmed?. Option based on information whether the text is quoted or not</param>
    /// <param name="fieldDelimiterChar">The field delimiter character.</param>
    /// <param name="fieldQualifierChar">The field qualifier character.</param>
    /// <param name="escapeCharacterChar">The escape character an escaped chars is read as is, 2nd method to have quotes or delimiter in column.</param>
    /// <param name="recordLimit">After the giving number of records stop reading</param>
    /// <param name="allowRowCombining">if set to <c>true</c> try to combine rows, assuming not properly quoted the content of a columns has pushed data to next line</param>
    /// <param name="contextSensitiveQualifier">if set to <c>true</c> assume context-sensitive qualifiers, context-sensitive quoting looks at the surrounding character to determine if is a starting or ending quote.</param>
    /// <param name="commentLine">Identifier for a comment line, a line starting with this text will be ignored.</param>
    /// <param name="numWarning">Warning will stop after the given number, information losses its value if repeated over and over</param>
    /// <param name="duplicateQualifierToEscape">if set to <c>true</c> a repeated qualifier are not ending the column but will be regarded as quote contained in the column.</param>
    /// <param name="newLinePlaceholder">The new line placeholder, similar to escaping but does replace the given placeholder with linefeed  e.G. "[LineFeed]" --> \n.</param>
    /// <param name="delimiterPlaceholder">The delimiter placeholder, similar to escaping but does replace the given placeholder with the delimiter  e.G. "{Del}" --> ","</param>
    /// <param name="quotePlaceholder">The quote placeholder, similar to escaping but does replace the given placeholder with the quote e.G. "{Quote}" --> "'"</param>
    /// <param name="skipDuplicateHeader">if set to <c>true</c> does not return a  row if the header is in the file multiple time, used when files where combined without removing the header.</param>
    /// <param name="treatLinefeedAsSpace">if set to <c>true</c> treat any found linefeed as regular space.</param>
    /// <param name="treatUnknownCharacterAsSpace">if set to <c>true</c> treat the unknown character as space.</param>
    /// <param name="tryToSolveMoreColumns">if set to <c>true</c> try and resolved additional columns, this happens when column content is not properly quoted.</param>
    /// <param name="warnDelimiterInValue">if set to <c>true</c> raise a warning if the delimiter is part of a column.</param>
    /// <param name="warnLineFeed">if set to <c>true</c> raise a warning if line feed is part of a column.</param>
    /// <param name="warnNbsp">if set to <c>true</c> raise a warning if a non-breaking space is part of a column.</param>
    /// <param name="warnQuotes">if set to <c>true</c> raise a warning if the quoting is part of a column.</param>
    /// <param name="warnUnknownCharacter">if set to <c>true</c> raise a warning if � is part of a column.</param>
    /// <param name="warnEmptyTailingColumns">if set to <c>true</c> raise a warning if empty tailing columns are found.</param>
    /// <param name="treatNbspAsSpace">if set to <c>true</c> treat a NBSP as regular space.</param>
    /// <param name="treatTextAsNull">The text to treat as null. c.G. "NULL" -> DBNull.Value; "N/A" -> DBNull.Value</param>
    /// <param name="skipEmptyLines">if set to <c>true</c> if empty lines should simply be skipped.</param>
    /// <param name="consecutiveEmptyRowsMax">Number of consecutive empty rows where we should assume the file is at its end.</param>
    /// <param name="identifierInContainer">The identifier in container, in case the file a container file like zip.</param>
    /// <param name="timeZoneAdjust">The routine to do time zone adjustments while reading, only used if you have a date / time column and timezone column in the source, e.G. "24/01/2024 07:56" and "UTC" -->  "24/01/2024 13:26" (if IST is timezone specified by <see cref="returnedTimeZone"/>) </param>
    /// <param name="returnedTimeZone">The time zone input file should be converted to, only viable if you have a date / time column and timezone column in the source.</param>
    /// <param name="allowPercentage">if set to <c>true</c> convert percentages to numeric values e.G. 17% -> 0.17.</param>
    /// <param name="removeCurrency">if set to <c>true</c> removed currency symbols to values can be read as decimals , .e.G. 17.82€ -> 17.82</param>
    /// <exception cref="System.ArgumentNullException">if fileName is not set</exception>
    /// <exception cref="System.ArgumentException">File can not be empty - fileName</exception>
    /// <exception cref="System.IO.FileNotFoundException">The file does not exist or is not accessible.</exception>
    public CsvFileReader(in string fileName,
      int codePageId = 65001,
      int skipRows = 0,
      bool hasFieldHeader = true,
      in IEnumerable<Column>? columnDefinition = null,
      in TrimmingOptionEnum trimmingOption = TrimmingOptionEnum.Unquoted,
      char fieldDelimiterChar = ',',
      char fieldQualifierChar = '"',
      char escapeCharacterChar = '\0',
      long recordLimit = 0,
      bool allowRowCombining = false,
      bool contextSensitiveQualifier = false,
      in string commentLine = "#",
      int numWarning = 0,
      bool duplicateQualifierToEscape = true,
      in string newLinePlaceholder = "",
      in string delimiterPlaceholder = "",
      in string quotePlaceholder = "",
      bool skipDuplicateHeader = true,
      bool treatLinefeedAsSpace = false,
      bool treatUnknownCharacterAsSpace = false,
      bool tryToSolveMoreColumns = false,
      bool warnDelimiterInValue = false,
      bool warnLineFeed = false,
      bool warnNbsp = false,
      bool warnQuotes = false,
      bool warnUnknownCharacter = false,
      bool warnEmptyTailingColumns = true,
      bool treatNbspAsSpace = false,
      in string treatTextAsNull = "null",
      bool skipEmptyLines = true,
      int consecutiveEmptyRowsMax = 5,
      in string identifierInContainer = "",
      in TimeZoneChangeDelegate? timeZoneAdjust = null,
      string returnedTimeZone = "",
      bool allowPercentage = true,
      bool removeCurrency = true)
      : this(
        columnDefinition, codePageId, skipRows, hasFieldHeader,
        trimmingOption, fieldDelimiterChar, fieldQualifierChar, escapeCharacterChar, recordLimit, allowRowCombining,
        contextSensitiveQualifier, commentLine, numWarning, duplicateQualifierToEscape, newLinePlaceholder,
        delimiterPlaceholder, quotePlaceholder, skipDuplicateHeader, treatLinefeedAsSpace, treatUnknownCharacterAsSpace,
        tryToSolveMoreColumns,
        warnDelimiterInValue, warnLineFeed, warnNbsp, warnQuotes, warnUnknownCharacter, warnEmptyTailingColumns,
        treatNbspAsSpace, treatTextAsNull, skipEmptyLines, consecutiveEmptyRowsMax, identifierInContainer, fileName,
        timeZoneAdjust, returnedTimeZone, allowPercentage, removeCurrency)
    {
      if (fileName is null)
        throw new ArgumentNullException(nameof(fileName));
      if (fileName.Trim().Length == 0)
        throw new ArgumentException("File can not be empty", nameof(fileName));
      if (!FileSystemUtils.FileExists(fileName))
        throw new FileNotFoundException(
          $"The file '{fileName.GetShortDisplayFileName()}' does not exist or is not accessible.",
          fileName);
    }

    private CsvFileReader(IEnumerable<Column>? columnDefinition,
      int codePageId,
      int skipRows,
      bool hasFieldHeader,
      TrimmingOptionEnum trimmingOption,
      char fieldDelimiterChar,
      char fieldQualifierChar,
      char escapePrefixChar,
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
      bool treatLinefeedAsSpace,
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
      in TimeZoneChangeDelegate? timeZoneAdjust,
      in string destTimeZone,
      bool allowPercentage,
      bool removeCurrency)
      : base(fileName, columnDefinition, recordLimit, timeZoneAdjust, destTimeZone, allowPercentage, removeCurrency)
    {
      SelfOpenedStream = !string.IsNullOrEmpty(fileName);
      m_HeaderRow = Array.Empty<string>();
      m_EscapePrefix = escapePrefixChar;
      m_FieldDelimiter = fieldDelimiterChar;
      m_FieldQualifier = fieldQualifierChar;

      if (m_FieldDelimiter == char.MinValue)
        throw new FileReaderException("All delimited text files do need a delimiter.");

      if (m_FieldQualifier == cCr || m_FieldQualifier == cLf)
        throw new FileReaderException(
          "The text qualifier characters is invalid, please use something else than CR or LF");

      if (m_FieldDelimiter == cCr || m_FieldDelimiter == cLf || m_FieldDelimiter == ' ')
        throw new FileReaderException(
          "The field delimiter character is invalid, please use something else than CR, LF or Space");

      if (m_EscapePrefix != char.MinValue && (m_FieldDelimiter == m_EscapePrefix || m_FieldQualifier == m_EscapePrefix))
        throw new FileReaderException(
          $"The escape character is invalid, please use something else than the field delimiter or qualifier character {m_EscapePrefix.Text()}.");

      if (m_FieldQualifier != char.MinValue && m_FieldQualifier == m_FieldDelimiter)
        throw new ArgumentOutOfRangeException(
          $"The field qualifier and the field delimiter characters of a delimited file cannot be the same character {m_FieldDelimiter.Text()}");

      m_AllowRowCombining = allowRowCombining;
      m_ContextSensitiveQualifier = contextSensitiveQualifier;
      m_CodePageId = codePageId;
      m_CommentLine = commentLine;

      m_NewLinePlaceholder = newLinePlaceholder.HandleLongText();
      m_DelimiterPlaceholder = delimiterPlaceholder.HandleLongText();

      var illegal = new[] { (char) 0x0a, (char) 0x0d, m_FieldDelimiter, m_FieldQualifier };
      if (m_DelimiterPlaceholder.IndexOfAny(illegal) != -1)
        throw new ArgumentException($"{nameof(delimiterPlaceholder)} invalid characters in '{m_DelimiterPlaceholder}'");

      if (m_NewLinePlaceholder.IndexOfAny(illegal) != -1)
        throw new ArgumentException($"{nameof(newLinePlaceholder)} invalid characters in '{m_NewLinePlaceholder}'");

      m_DuplicateQualifierToEscape = duplicateQualifierToEscape;

      m_NumWarning = numWarning;
      m_QuotePlaceholder = quotePlaceholder;
      m_SkipDuplicateHeader = skipDuplicateHeader;
      m_SkipRows = skipRows;
      m_TreatLinefeedAsSpace = treatLinefeedAsSpace;
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
      m_WarnEmptyTailingColumns = warnEmptyTailingColumns;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets a value indicating whether this instance is closed.
    /// </summary>
    /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
    public override bool IsClosed => m_TextReader is null;

    /// <inheritdoc />
    public override void Close()
    {
      base.Close();
      m_NumWarningsQuote = 0;
      m_NumWarningsDelimiter = 0;
      m_NumWarningsUnknownChar = 0;
      m_NumWarningsNbspChar = 0;
    }

    /// <inheritdoc cref="IFileReader" />
    public new void Dispose()
    {
      Dispose(true);
    }

    /// <inheritdoc cref="IFileReader" />
    public new long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferOffset, int length)
    {
      if (buffer is null) throw new ArgumentNullException(nameof(buffer));
      if (GetColumn(i).ValueFormat.DataType != DataTypeEnum.Binary ||
          string.IsNullOrEmpty(CurrentRowColumnText[i])) return -1;
      using var fs = FileSystemUtils.OpenRead(CurrentRowColumnText[i]);
      if (fieldOffset > 0)
        fs.Seek(fieldOffset, SeekOrigin.Begin);
      return fs.Read(buffer, bufferOffset, length);
    }

    /// <inheritdoc cref="IFileReader" />
    /// <exception cref="T:System.NotImplementedException">Always returns</exception>
    [Obsolete("Not implemented")]
    public new IDataReader GetData(int i) => throw new NotImplementedException();

    /// <inheritdoc cref="IFileReader" />
    /// <summary>
    ///   Gets the data type information for the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>The .NET type name of the column</returns>
    public new string GetDataTypeName(int i) => Column[i].ValueFormat.DataType.GetNetType().Name;

    /// <inheritdoc />
    public override object GetValue(int ordinal)
    {
      if (IsDBNull(ordinal))
        return DBNull.Value;
      var column = Column[ordinal];
      if (column.Ignore)
        return DBNull.Value;
      object? ret = column.ValueFormat.DataType switch
      {
        DataTypeEnum.DateTime => GetDateTimeNull(null, CurrentRowColumnText[ordinal].AsSpan(), null,
          GetTimeValue(ordinal), column, true),
        DataTypeEnum.Integer => IntPtr.Size == 4
          ? GetInt32Null(CurrentRowColumnText[ordinal].AsSpan(), column)
          : GetInt64Null(CurrentRowColumnText[ordinal].AsSpan(), column),
        DataTypeEnum.Double => GetDoubleNull(CurrentRowColumnText[ordinal].AsSpan(), ordinal),
        DataTypeEnum.Numeric => GetDecimalNull(CurrentRowColumnText[ordinal].AsSpan(), ordinal),
        DataTypeEnum.Boolean => GetBooleanNull(CurrentRowColumnText[ordinal].AsSpan(), ordinal),
        DataTypeEnum.Guid => GetGuidNull(CurrentRowColumnText[ordinal].AsSpan(), column.ColumnOrdinal),
        _ => CurrentRowColumnText[ordinal]
      };
      return ret ?? DBNull.Value;
    }

    /// <inheritdoc cref="IFileReader.OpenAsync" />
    /// <summary>
    ///   Open the file Reader; Start processing the Headers and determine the maximum column size
    /// </summary>
    public override async Task OpenAsync(CancellationToken token)
    {
      await BeforeOpenAsync($"Opening delimited file \"{FullPath.GetShortDisplayFileName()}\"")
        .ConfigureAwait(false);
      try
      {
        if (SelfOpenedStream)
        {
          if (m_Stream != null)
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            await m_Stream.DisposeAsync().ConfigureAwait(false);
#else
            m_Stream.Dispose();
#endif
          m_Stream = FunctionalDI.GetStream(new SourceAccess(FullPath)
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
        m_TextReader = await m_Stream.GetTextReaderAsync(m_CodePageId, m_SkipRows, token).ConfigureAwait(false);

        ResetPositionToStartOrOpen();

        m_HeaderRow = ReadNextRow(false);

        InitColumn(ParseFieldCount(m_HeaderRow));

        ParseColumnName(m_HeaderRow, null, m_HasFieldHeader);

        // Turn off un-escaped warning based on WarnLineFeed
        if (!m_WarnLineFeed)
          foreach (var col in Column)
            if (col.ColumnFormatter is TextUnescapeFormatter unescapedFormatter)
              unescapedFormatter.RaiseWarning = false;

        if (m_TryToSolveMoreColumns && m_FieldDelimiter != char.MinValue)
          m_RealignColumns = new ReAlignColumns(FieldCount);

        if (m_TextReader.CanSeek)
          ResetPositionToFirstDataRow();

        FinishOpen();
      }
      catch (Exception ex)
      {
        if (ShouldRetry(ex, token))
        {
          await OpenAsync(token).ConfigureAwait(false);
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
    }

    /// <inheritdoc cref="BaseFileReader" />
    public override Task<bool> ReadAsync(CancellationToken cancellationToken) =>
      Task.FromResult(Read(cancellationToken));

    /// <inheritdoc />
#pragma warning disable CS0618
    public override bool Read(in CancellationToken token) => !token.IsCancellationRequested && Read();
#pragma warning restore CS0618

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

    /// <inheritdoc cref="IFileReader" />
    public new void ResetPositionToFirstDataRow()
    {
      ResetPositionToStartOrOpen();
      if (m_HasFieldHeader)
        // Read the header row, this could be more than one line
        ReadNextRow(false);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
      if (!disposing)
        return;
      if (SelfOpenedStream)
      {
        m_Stream?.Dispose();
        m_Stream = null;
      }

      m_TextReader?.Dispose();
      m_TextReader = null;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and 1</returns>
    protected override double GetRelativePosition()
    {
      if (m_Stream is IImprovedStream imp)
        return imp.Percentage;

      return base.GetRelativePosition();
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
        m_RealignColumns?.AddRow(CurrentRowColumnText);

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
            if (m_WarnEmptyTailingColumns)
              HandleWarning(-2, $"Last line is '{CurrentRowColumnText[0]}'. Assumed to be a EOF marker and ignored.");
            return false;
          }

          if (!m_AllowRowCombining)
          {
            HandleWarning(-1, $"Line {cLessColumns} ({CurrentRowColumnText.Length}/{FieldCount}).");
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
              HandleWarning(CurrentRowColumnText.Length - 1,
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
            HandleWarning(-1, text + " Trying to realign columns.");

            // determine which column could have caused the issue it could be any column, try to establish
            CurrentRowColumnText = m_RealignColumns.RealignColumn(
              CurrentRowColumnText,
              HandleWarning,
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
              if (m_WarnEmptyTailingColumns)
                HandleWarning(-1,
                  text + " All additional columns where empty. Allow 're-align columns' to handle this.");
              return true;
            }

            HandleWarning(-1,
              text + " The data in extra columns is not read. Allow 're-align columns' to handle this.");
          }
        }

        // now handle Text replacements and warning in the read columns
        for (var columnNo = 0; columnNo < FieldCount && columnNo < CurrentRowColumnText.Length; columnNo++)
        {
          if (GetColumn(columnNo).Ignore ||
              string.IsNullOrEmpty(CurrentRowColumnText[columnNo]))
            continue;

          // Handle replacements and warnings etc,
          var adjustedValue = HandleTextSpecials(
            CurrentRowColumnText[columnNo]
              .ReplaceCaseInsensitive(m_NewLinePlaceholder, '\n')
              .ReplaceCaseInsensitive(m_DelimiterPlaceholder, m_FieldDelimiter)
              .ReplaceCaseInsensitive(m_QuotePlaceholder, m_FieldQualifier).AsSpan(),
            columnNo);

          if (adjustedValue.Length > 0)
          {
            if (m_WarnQuotes && adjustedValue.IndexOf(m_FieldQualifier) != -1 &&
                (m_NumWarning < 1 || m_NumWarningsQuote++ < m_NumWarning))
              HandleWarning(columnNo, $"Field qualifier '{m_FieldQualifier.Text()}' found in field".AddWarningId());

            if (m_WarnDelimiterInValue && adjustedValue.IndexOf(m_FieldDelimiter) != -1 &&
                (m_NumWarning < 1 || m_NumWarningsDelimiter++ < m_NumWarning))
              HandleWarning(columnNo, $"Field delimiter '{m_FieldDelimiter.Text()}' found in field".AddWarningId());

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

            if (m_WarnLineFeed && (adjustedValue.IndexOfAny(new[] { '\r', '\n' }) != -1))
              WarnLinefeed(columnNo);

            if (adjustedValue.ShouldBeTreatedAsNull(m_TreatTextAsNull.AsSpan()))
              adjustedValue = Array.Empty<char>();
          }
#if NET7_0_OR_GREATER
          CurrentRowColumnText[columnNo] = new string(adjustedValue);
#else
          CurrentRowColumnText[columnNo] = adjustedValue.ToString();
#endif
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
      if (c == m_FieldDelimiter)
        return false;

      // See char.IsLatin1(char c) in Reflector
      if (c <= '\x00ff')
        // ReSharper disable once MergeIntoLogicalPattern
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
    ///   If seek is supported, it will parse a few extra rows to check if the header and the
    ///   following rows do result in the same number of columns
    /// </remarks>
    private int ParseFieldCount(IReadOnlyList<string> headerRow)
    {
      var headerLength = headerRow.Sum(x => x.Length);
      if (headerRow.Count == 0 || headerLength ==0 || headerLength < Enumerable.Count(headerRow.TakeWhile(x => !string.IsNullOrEmpty(x))))
        return 0;
      
      var fields = headerRow.Count;

      // The last column is empty, but we expect a header column, assume if a trailing separator
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
      // ReSharper disable once UseIndexFromEndExpression
      if (string.IsNullOrEmpty(headerRow[headerRow.Count - 1]))
      {
        HandleWarning(fields,
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
        if (!postData && m_TreatLinefeedAsSpace && character == cLf && quoted)
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

            var nextChar = char.MinValue;
            if (!EndOfFile)
            {
              nextChar = Peek();
              if ((character != cCr || nextChar != cLf) && (character != cLf || nextChar != cCr))
              {
                nextChar = char.MinValue;
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
        if ((character == m_FieldDelimiter && !escaped && (postData || !quoted)) || EndOfFile)
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
          if (character == m_FieldQualifier && !escaped)
          {
            if (m_TrimmingOption != TrimmingOptionEnum.None)
              stringBuilder.Length = 0;

            // quoted data is starting
            quoted = true;
            continue;
          }

          goto append;
        }

        if (character == m_FieldQualifier && quoted && !escaped)
        {
          var peekNextChar = Peek();

          // a "" should be regarded as " if the text is quoted
          if (m_DuplicateQualifierToEscape && peekNextChar == m_FieldQualifier)
          {
            // double quotes within quoted string means add a quote
            stringBuilder.Append(m_FieldQualifier);
            MoveNext(peekNextChar);

            // handling for "" that is not only representing a " but also closes the text
            if (m_ContextSensitiveQualifier)
            {
              peekNextChar = Peek();
              if (peekNextChar == m_FieldDelimiter || peekNextChar == cCr || peekNextChar == cLf)
                postData = true;
            }

            continue;
          }

          // a single " should be regarded as closing when its followed by the delimiter
          if (m_ContextSensitiveQualifier &&
              (peekNextChar == m_FieldDelimiter || peekNextChar == cCr || peekNextChar == cLf))
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
        if (escaped && (character == m_FieldQualifier || character == m_FieldDelimiter ||
                        character == m_EscapePrefix))
          // remove the already added escape char
          stringBuilder.Length--;

        // all cases covered, character must be data
        stringBuilder.Append(character);

        escaped = !escaped && character == m_EscapePrefix;
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
            && item.IndexOf(m_FieldDelimiter) != -1)
          HandleWarning(col,
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

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <inheritdoc />
    public new async ValueTask DisposeAsync()
    {
      await DisposeAsyncCore();

      Dispose(false);
    }

    protected async ValueTask DisposeAsyncCore()
    {
      if (m_Stream != null &&  SelfOpenedStream)
        await m_Stream.DisposeAsync().ConfigureAwait(false);
    }

#endif
  }
}