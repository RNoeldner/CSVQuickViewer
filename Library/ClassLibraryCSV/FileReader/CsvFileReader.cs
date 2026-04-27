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

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools;

/// <inheritdoc cref="CsvTools.BaseFileReader" />
/// <summary>
///   Reader implementation for delimited files like csv or tab
/// </summary>
public class CsvFileReader : BaseFileReader
{
  /// <summary>
  ///   Message suffix: the row contains fewer columns than expected.
  /// </summary>
  public const string cLessColumns = " has fewer columns than expected";

  /// <summary>
  ///   Message suffix: the row contains more columns than expected.
  /// </summary>
  public const string cMoreColumns = " has more columns than expected";

  /// <summary>
  ///   Carriage return character (<c>\r</c>).
  /// </summary>
  private const char cCr = (char) 0x0d;

  /// <summary>
  ///   Line-feed character (<c>\n</c>).
  /// </summary>
  private const char cLf = (char) 0x0a;

  /// <summary>
  ///   Non-breaking space character.
  /// </summary>
  private const char cNbsp = (char) 0xA0;

  /// <summary>
  ///   Unicode replacement character used to represent unknown or invalid input.
  /// </summary>
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

  /// <summary>
  ///   Stores the raw text of the current record before column splitting and trimming.
  ///   Cleared for each new row; used only when <see cref="m_ColumnMerger"/> is active.
  /// </summary>
  private readonly StringBuilder m_RecordSource = new StringBuilder(100);

  private readonly bool m_SkipDuplicateHeader;

  private readonly bool m_SkipEmptyLines;

  private readonly int m_SkipRows;

  private readonly int m_SkipRowsAfterHeader;

  private readonly bool m_TreatLinefeedAsSpace;

  private readonly bool m_TreatNbspAsSpace;
  private readonly string m_TreatNbspMessage;

  private readonly string m_TreatTextAsNull;

  private readonly bool m_TreatUnknownCharacterAsSpace;
  private readonly string m_TreatUnknownCharacterMessage;

  private readonly TrimmingOptionEnum m_TrimmingOption;

  private readonly bool m_TryToSolveMoreColumns;

  private readonly bool m_WarnDelimiterInValue;

  private readonly bool m_WarnLineFeed;

  private readonly bool m_WarnNbsp;

  private readonly bool m_WarnQuotes;

  private readonly bool m_WarnUnknownCharacter;
  private readonly bool m_WarnEmptyTailingColumns;
  private char[] m_ProcessingBuffer;
  private readonly char m_SafeBorder;
  private int m_ConsecutiveEmptyRows;

  /// <summary>
  ///   Tracks typical column lengths during parsing, avoiding repeated allocations.
  /// </summary>
  private int[] MaxColumnLengths = [];

  /// <summary>
  ///   Indicates whether the end of the current line has been reached.
  /// </summary>
  private bool m_EndOfLine;
  private long m_EndLineNumber;

  private IReadOnlyList<string> m_HeaderRow;

  private Stream? m_Stream;
  private readonly StreamProviderDelegate m_StreamProvider;

  /// <summary>
  ///   Number of records in the source file; set only when the entire file has been read.
  /// </summary>
  private ushort m_NumWarningsDelimiter;

  private ushort m_NumWarningsLinefeed;

  private ushort m_NumWarningsNbspChar;

  private ushort m_NumWarningsQuote;

  private ushort m_NumWarningsUnknownChar;

  /// <summary>
  ///   Supports column realignment when enabled.  
  ///   Maintains a sliding window of well-formed rows and attempts to correct misaligned ones.
  /// </summary>
  private CsvColumnMerger? m_ColumnMerger;

  /// <summary>
  ///   Underlying text reader for decoding and reading the CSV data stream.
  /// </summary>
  private ImprovedTextReader? m_TextReader;

  /// <summary>
  ///   Initializes a new instance of the <see cref="CsvFileReader"/> class from a <see cref="Stream"/>.
  /// </summary>
  /// <param name="stream">
  ///   The input stream containing the CSV data.
  /// </param>
  /// <param name="codePageId">
  ///   The code page identifier used for decoding the stream.  
  ///   UTF-8 corresponds to <c>65001</c>.
  /// </param>
  /// <param name="skipRows">
  ///   Number of initial rows to ignore, e.g., introductory text unrelated to the tabular data.
  /// </param>
  /// <param name="skipRowsAfterHeader">
  ///   Number of rows to skip immediately after the header row.  
  ///   This is rarely needed, as comment lines are automatically ignored and empty lines are skipped.  
  ///   It is typically used when the rows after the header contain descriptive information, units, or other metadata about the columns.
  /// </param>
  /// <param name="hasFieldHeader">
  ///   When <c>true</c>, the first data row is interpreted as the column header.
  /// </param>
  /// <param name="columnDefinition">
  ///   Column definitions describing conversions and data types for individual fields.
  /// </param>
  /// <param name="trimmingOption">
  ///   Defines how leading and trailing whitespace should be trimmed, optionally depending on whether a value is quoted.
  /// </param>
  /// <param name="fieldDelimiterChar">
  ///   Character used to separate fields (e.g. <c>,</c>).
  /// </param>
  /// <param name="fieldQualifierChar">
  ///   Character used to quote fields that contain special characters.
  /// </param>
  /// <param name="escapeCharacterChar">
  ///   Escape prefix indicating that the following character is to be interpreted literally.
  ///   Provides an alternative mechanism for including qualifiers or delimiters within a field.
  /// </param>
  /// <param name="recordLimit">
  ///   Maximum number of records to read. Use <c>0</c> to read all records.
  /// </param>
  /// <param name="allowRowCombining">
  ///   When <c>true</c>, attempts to merge rows if malformed quoting has caused a logical row to span multiple physical lines.
  /// </param>
  /// <param name="contextSensitiveQualifier">
  ///   When <c>true</c>, interprets qualifiers based on surrounding context to distinguish opening and closing quotation marks.
  /// </param>
  /// <param name="commentLine">
  ///   Lines beginning with this text are treated as comments and ignored.
  /// </param>
  /// <param name="numWarning">
  ///   Maximum number of warnings to emit. Further occurrences of the same issue are suppressed.
  /// </param>
  /// <param name="duplicateQualifierToEscape">
  ///   When <c>true</c>, doubled qualifier characters are interpreted as literal qualifiers within a field.
  /// </param>
  /// <param name="newLinePlaceholder">
  ///   Placeholder token representing a line break (e.g., <c>"[LineFeed]" → "\n"</c>).
  /// </param>
  /// <param name="delimiterPlaceholder">
  ///   Placeholder token representing the field delimiter (e.g., <c>"{Del}" → ","</c>).
  /// </param>
  /// <param name="quotePlaceholder">
  ///   Placeholder token representing the qualifier character (e.g., <c>"{Quote}" → "\""</c>).
  /// </param>
  /// <param name="skipDuplicateHeader">
  ///   When <c>true</c>, suppresses repeated header rows, which may occur when files have been concatenated without removing headers.
  /// </param>
  /// <param name="treatLinefeedAsSpace">
  ///   When <c>true</c>, treats any line-feed character found inside a field as a regular space.
  /// </param>
  /// <param name="treatUnknownCharacterAsSpace">
  ///   When <c>true</c>, interprets the Unicode replacement character (<c>�</c>) as a space.
  /// </param>
  /// <param name="tryToSolveMoreColumns">
  ///   When <c>true</c>, attempts to reconstruct rows with too many columns, typically caused by incorrectly quoted content.
  /// </param>
  /// <param name="warnDelimiterInValue">
  ///   Raise a warning when the delimiter is found within a field.
  /// </param>
  /// <param name="warnLineFeed">
  ///   Raise a warning when an embedded line-feed is detected.
  /// </param>
  /// <param name="warnNbsp">
  ///   Raise a warning when a non-breaking space is encountered.
  /// </param>
  /// <param name="warnQuotes">
  ///   Raise a warning when unexpected quotation characters appear in a field.
  /// </param>
  /// <param name="warnUnknownCharacter">
  ///   Raise a warning when the Unicode replacement character (<c>�</c>) is encountered.
  /// </param>
  /// <param name="warnEmptyTailingColumns">
  ///   When <c>true</c>, warns when trailing columns are empty.
  /// </param>
  /// <param name="treatNbspAsSpace">
  ///   When <c>true</c>, treats the non-breaking space character as a conventional space.
  /// </param>
  /// <param name="treatTextAsNull">
  ///   Text values to be interpreted as <c>null</c> (e.g., <c>"NULL" → DBNull.Value</c>, <c>"N/A" → DBNull.Value</c>).
  /// </param>
  /// <param name="skipEmptyLines">
  ///   When <c>true</c>, silently skips empty lines.
  /// </param>
  /// <param name="consecutiveEmptyRowsMax">
  ///   Maximum number of consecutive empty rows before assuming end-of-file.
  /// </param>
  /// <param name="destinationTimeZone">
  ///   Target time zone to which date-time values should be converted.
  /// </param>
  /// <param name="allowPercentage">
  ///   When <c>true</c>, converts percentage values to numeric form (e.g., <c>"17%" → 0.17</c>).
  /// </param>
  /// <param name="removeCurrency">
  ///   When <c>true</c>, removes currency symbols so values can be parsed as decimals (e.g., <c>"17.82€" → 17.82</c>).
  /// </param>
  /// <exception cref="ArgumentNullException">
  ///   Thrown when <paramref name="stream"/> is <c>null</c>.
  /// </exception>
  /// <exception cref="FileReaderException">
  ///   Thrown when an invalid field delimiter, qualifier, or escape character is provided.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  ///   Thrown when the field delimiter and qualifier characters are the same.
  /// </exception>
  /// <exception cref="ArgumentException">
  ///   Thrown when <paramref name="newLinePlaceholder"/> or <paramref name="delimiterPlaceholder"/> contains illegal characters.
  /// </exception>
  public CsvFileReader(Stream stream, int codePageId = 65001, int skipRows = 0, int skipRowsAfterHeader = 0,
                       bool hasFieldHeader = true, in IEnumerable<Column>? columnDefinition = null,
                       in TrimmingOptionEnum trimmingOption = TrimmingOptionEnum.Unquoted, char fieldDelimiterChar = ',',
                       char fieldQualifierChar = '"', char escapeCharacterChar = '\0', long recordLimit = 0,
                       bool allowRowCombining = false, bool contextSensitiveQualifier = false, string commentLine = "",
                       int numWarning = 0, bool duplicateQualifierToEscape = true, string newLinePlaceholder = "",
                       string delimiterPlaceholder = "", string quotePlaceholder = "", bool skipDuplicateHeader = false,
                       bool treatLinefeedAsSpace = false, bool treatUnknownCharacterAsSpace = false,
                       bool tryToSolveMoreColumns = false, bool warnDelimiterInValue = false, bool warnLineFeed = false,
                       bool warnNbsp = false, bool warnQuotes = false, bool warnUnknownCharacter = false,
                       bool warnEmptyTailingColumns = false, bool treatNbspAsSpace = false,
                       string treatTextAsNull = "", bool skipEmptyLines = false, int consecutiveEmptyRowsMax = 5,
                       string destinationTimeZone = "", bool allowPercentage = true, bool removeCurrency = true)
    : this(codePageId, skipRows, skipRowsAfterHeader,
           hasFieldHeader, columnDefinition,
           trimmingOption, fieldDelimiterChar,
           fieldQualifierChar, escapeCharacterChar, recordLimit,
           allowRowCombining, contextSensitiveQualifier, commentLine,
           numWarning, duplicateQualifierToEscape, newLinePlaceholder,
           delimiterPlaceholder, quotePlaceholder, skipDuplicateHeader,
           treatLinefeedAsSpace, treatUnknownCharacterAsSpace,
           tryToSolveMoreColumns, warnDelimiterInValue, warnLineFeed,
           warnNbsp, warnQuotes, warnUnknownCharacter,
           warnEmptyTailingColumns, treatNbspAsSpace,
           treatTextAsNull, skipEmptyLines, consecutiveEmptyRowsMax,
           string.Empty, string.Empty, destinationTimeZone,
           allowPercentage, removeCurrency)
  {
    m_Stream = stream ?? throw new ArgumentNullException(nameof(stream));
    m_StreamProvider = FunctionalDI.GetStream;
  }

  /// <summary>
  ///   Initializes a new instance of the <see cref="CsvFileReader"/> class.
  /// </summary>
  /// <param name="fileName">
  ///   Fully qualified path of the CSV file to read.
  /// </param>
  /// <param name="codePageId">
  ///   The code page identifier used for decoding the file.  
  ///   UTF-8 corresponds to <c>65001</c>.
  /// </param>
  /// <param name="skipRows">
  ///   Number of initial rows to ignore, e.g. introductory text unrelated to the tabular data.
  /// </param>
  /// <param name="skipRowsAfterHeader">
  ///   Number of rows to skip immediately after the header row.  
  ///   This is rarely needed, as comment lines are automatically ignored and empty lines are skipped.  
  ///   It is typically used when the rows after the header contain descriptive information, units, or other metadata about the columns.
  /// </param>
  /// <param name="hasFieldHeader">
  ///   When <c>true</c>, the first data row is interpreted as the column header.
  /// </param>
  /// <param name="columnDefinition">
  ///   Column definitions describing conversions and data types for individual fields.
  /// </param>
  /// <param name="trimmingOption">
  ///   Defines how leading and trailing whitespace should be trimmed, optionally
  ///   depending on whether a value is quoted.
  /// </param>
  /// <param name="fieldDelimiterChar">
  ///   Character used to separate fields (e.g. <c>,</c>).
  /// </param>
  /// <param name="fieldQualifierChar">
  ///   Character used to quote fields that contain special characters.
  /// </param>
  /// <param name="escapeCharacterChar">
  ///   Escape prefix indicating that the following character is to be interpreted literally.
  ///   Provides an alternative mechanism for including qualifiers or delimiters within a field.
  /// </param>
  /// <param name="recordLimit">
  ///   Maximum number of records to read. Use <c>0</c> to read all records.
  /// </param>
  /// <param name="allowRowCombining">
  ///   When <c>true</c>, attempts to merge rows if malformed quoting has caused
  ///   a logical row to span multiple physical lines.
  /// </param>
  /// <param name="contextSensitiveQualifier">
  ///   When <c>true</c>, interprets qualifiers based on surrounding context to
  ///   distinguish opening and closing quotation marks.
  /// </param>
  /// <param name="commentLine">
  ///   Lines beginning with this text are treated as comments and ignored.
  /// </param>
  /// <param name="numWarning">
  ///   Maximum number of warnings to emit. Further occurrences of the same issue are suppressed.
  /// </param>
  /// <param name="duplicateQualifierToEscape">
  ///   When <c>true</c>, doubled qualifier characters are interpreted as literal qualifiers
  ///   within a field.
  /// </param>
  /// <param name="newLinePlaceholder">
  ///   Placeholder token representing a line break.  
  ///   For example: <c>"[LineFeed]" → "\n"</c>.
  /// </param>
  /// <param name="delimiterPlaceholder">
  ///   Placeholder token representing the field delimiter.  
  ///   For example: <c>"{Del}" → ","</c>.
  /// </param>
  /// <param name="quotePlaceholder">
  ///   Placeholder token representing the qualifier character.  
  ///   For example: <c>"{Quote}" → "\""</c>.
  /// </param>
  /// <param name="skipDuplicateHeader">
  ///   When <c>true</c>, suppresses repeated header rows, which may occur when files
  ///   have been concatenated without removing headers.
  /// </param>
  /// <param name="treatLinefeedAsSpace">
  ///   When <c>true</c>, treats any line-feed character found inside a field as a regular space.
  /// </param>
  /// <param name="treatUnknownCharacterAsSpace">
  ///   When <c>true</c>, interprets the Unicode replacement character (<c>�</c>) as a space.
  /// </param>
  /// <param name="tryToSolveMoreColumns">
  ///   When <c>true</c>, attempts to reconstruct rows with too many columns,
  ///   typically caused by incorrectly quoted content.
  /// </param>
  /// <param name="warnDelimiterInValue">
  ///   Raise a warning when the delimiter is found within a field.
  /// </param>
  /// <param name="warnLineFeed">
  ///   Raise a warning when an embedded line-feed is detected.
  /// </param>
  /// <param name="warnNbsp">
  ///   Raise a warning when a non-breaking space is encountered.
  /// </param>
  /// <param name="warnQuotes">
  ///   Raise a warning when unexpected quotation characters appear in a field.
  /// </param>
  /// <param name="warnUnknownCharacter">
  ///   Raise a warning when the Unicode replacement character (<c>�</c>) is encountered.
  /// </param>
  /// <param name="warnEmptyTailingColumns">
  ///   When <c>true</c>, warns when trailing columns are empty.
  /// </param>
  /// <param name="treatNbspAsSpace">
  ///   When <c>true</c>, treats the non-breaking space character as a conventional space.
  /// </param>
  /// <param name="treatTextAsNull">
  ///   Text values to be interpreted as <c>null</c>.  
  ///   Example: <c>"NULL" → DBNull.Value</c>, <c>"N/A" → DBNull.Value</c>.
  /// </param>
  /// <param name="skipEmptyLines">
  ///   When <c>true</c>, silently skips empty lines.
  /// </param>
  /// <param name="consecutiveEmptyRowsMax">
  ///   Maximum number of consecutive empty rows before assuming end-of-file.
  /// </param>
  /// <param name="identifierInContainer">
  ///   Identifier of the file within a container (e.g. an entry name inside a ZIP archive).
  /// </param>  
  /// <param name="destinationTimeZone">
  ///   Target time zone to which date-time values should be converted.
  /// </param>
  /// <param name="allowPercentage">
  ///   When <c>true</c>, converts percentage values to numeric form (e.g. <c>"17%" → 0.17</c>).
  /// </param>
  /// <param name="removeCurrency">
  ///   When <c>true</c>, removes currency symbols so values can be parsed as decimals  
  ///   (e.g. <c>"17.82€" → 17.82</c>).
  /// </param>
  ///
  /// <exception cref="ArgumentNullException">
  ///   Thrown when <paramref name="fileName"/> is <c>null</c>.
  /// </exception>
  /// <exception cref="ArgumentException">
  ///   Thrown when <paramref name="fileName"/> is empty or whitespace.
  /// </exception>
  /// <exception cref="FileNotFoundException">
  ///   Thrown when the file does not exist or cannot be accessed.
  /// </exception>
  /// <exception cref="FileReaderException">
  ///   Thrown when an invalid field delimiter, qualifier, or escape character is provided.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  ///   Thrown when the field delimiter and qualifier characters are the same.
  /// </exception>
  /// <exception cref="ArgumentException">
  ///   Thrown when <paramref name="newLinePlaceholder"/> or <paramref name="delimiterPlaceholder"/> contains illegal characters.
  /// </exception>
  public CsvFileReader(string fileName, int codePageId = 65001, int skipRows = 0, int skipRowsAfterHeader = 0,
                       bool hasFieldHeader = true, in IEnumerable<Column>? columnDefinition = null,
                       in TrimmingOptionEnum trimmingOption = TrimmingOptionEnum.Unquoted, char fieldDelimiterChar = ',',
                       char fieldQualifierChar = '"', char escapeCharacterChar = '\0', long recordLimit = 0,
                       bool allowRowCombining = false, bool contextSensitiveQualifier = false, string commentLine = "#",
                       int numWarning = 0, bool duplicateQualifierToEscape = true, string newLinePlaceholder = "",
                       string delimiterPlaceholder = "", string quotePlaceholder = "", bool skipDuplicateHeader = true,
                       bool treatLinefeedAsSpace = false, bool treatUnknownCharacterAsSpace = false,
                       bool tryToSolveMoreColumns = false, bool warnDelimiterInValue = false, bool warnLineFeed = false,
                       bool warnNbsp = false, bool warnQuotes = false, bool warnUnknownCharacter = false,
                       bool warnEmptyTailingColumns = true, bool treatNbspAsSpace = false,
                       string treatTextAsNull = "", bool skipEmptyLines = true, int consecutiveEmptyRowsMax = 0,
                       string identifierInContainer = "", string destinationTimeZone = "", bool allowPercentage = true, bool removeCurrency = true)
    : this(codePageId, skipRows, skipRowsAfterHeader,
           hasFieldHeader, columnDefinition,
           trimmingOption, fieldDelimiterChar,
           fieldQualifierChar, escapeCharacterChar, recordLimit,
           allowRowCombining, contextSensitiveQualifier, commentLine,
           numWarning, duplicateQualifierToEscape, newLinePlaceholder,
           delimiterPlaceholder, quotePlaceholder, skipDuplicateHeader,
           treatLinefeedAsSpace, treatUnknownCharacterAsSpace,
           tryToSolveMoreColumns, warnDelimiterInValue, warnLineFeed,
           warnNbsp, warnQuotes, warnUnknownCharacter,
           warnEmptyTailingColumns, treatNbspAsSpace,
           treatTextAsNull, skipEmptyLines, consecutiveEmptyRowsMax,
           identifierInContainer, fileName, destinationTimeZone, allowPercentage, removeCurrency)
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

  private CsvFileReader(int codePageId, int skipRows, int skipRowsAfterHeader, bool hasFieldHeader,
                        IEnumerable<Column>? columnDefinition, TrimmingOptionEnum trimmingOption,
                        char fieldDelimiterChar, char fieldQualifierChar, char escapeCharacterChar, long recordLimit,
                        bool allowRowCombining, bool contextSensitiveQualifier, string commentLine, int numWarning,
                        bool duplicateQualifierToEscape, string newLinePlaceholder, string delimiterPlaceholder,
                        string quotePlaceholder, bool skipDuplicateHeader, bool treatLinefeedAsSpace,
                        bool treatUnknownCharacterAsSpace, bool tryToSolveMoreColumns, bool warnDelimiterInValue,
                        bool warnLineFeed, bool warnNbsp, bool warnQuotes, bool warnUnknownCharacter,
                        bool warnEmptyTailingColumns, bool treatNbspAsSpace, string treatTextAsNull, bool skipEmptyLines,
                        int consecutiveEmptyRowsMax, string identifierInContainer, string fileName,
                        string destinationTimeZone, bool allowPercentage,
                        bool removeCurrency)
    : base(fileName, columnDefinition, recordLimit, destinationTimeZone, allowPercentage, removeCurrency)
  {
    SelfOpenedStream = !string.IsNullOrEmpty(fileName);
    m_StreamProvider = FunctionalDI.GetStream;
    m_HeaderRow = [];
    m_EscapePrefix = escapeCharacterChar;
    m_FieldDelimiter = fieldDelimiterChar;
    m_FieldQualifier = fieldQualifierChar;

    if (m_FieldDelimiter == char.MinValue)
      throw new FileReaderException("All delimited text files do need a delimiter.");

    if (m_FieldQualifier is cCr or cLf)
      throw new FileReaderException("The text qualifier characters is invalid, please use something else than CR or LF");

    if (m_FieldDelimiter is cCr or cLf or ' ')
      throw new FileReaderException("The field delimiter character is invalid, please use something else than CR, LF or Space");

    if (m_EscapePrefix != char.MinValue && (m_FieldDelimiter == m_EscapePrefix || m_FieldQualifier == m_EscapePrefix))
      throw new FileReaderException($"The escape character is invalid, please use something else than the field delimiter or qualifier character {m_EscapePrefix.Text()}.");

    if (m_FieldQualifier != char.MinValue && m_FieldQualifier == m_FieldDelimiter)
      throw new ArgumentOutOfRangeException($"The field qualifier and the field delimiter characters of a delimited file cannot be the same character {m_FieldDelimiter.Text()}");

    m_AllowRowCombining = allowRowCombining;
    m_ContextSensitiveQualifier = contextSensitiveQualifier;
    m_CodePageId = codePageId;
    m_CommentLine = commentLine;

    m_NewLinePlaceholder = newLinePlaceholder.HandleLongText();
    m_DelimiterPlaceholder = delimiterPlaceholder.HandleLongText();

    var illegal = new[] { (char) 0x0a, (char) 0x0d, m_FieldDelimiter, m_FieldQualifier };
    if (m_DelimiterPlaceholder.IndexOfAny(illegal) != -1)
      throw new ArgumentException($"Invalid delimiter characters in '{m_DelimiterPlaceholder}'", nameof(delimiterPlaceholder));

    if (m_NewLinePlaceholder.IndexOfAny(illegal) != -1)
      throw new ArgumentException($"Invalid placeholder characters in '{m_NewLinePlaceholder}'", nameof(newLinePlaceholder));

    m_DuplicateQualifierToEscape = duplicateQualifierToEscape;

    m_NumWarning = numWarning;
    m_QuotePlaceholder = quotePlaceholder;
    m_SkipDuplicateHeader = skipDuplicateHeader;
    m_SkipRows = skipRows<0 ? 0 : skipRows;
    m_SkipRowsAfterHeader = skipRowsAfterHeader<0 ? 0 : skipRowsAfterHeader;
    if (m_SkipRows>99 || m_SkipRowsAfterHeader>99)
      Logger.Warning("Detected unusually high skip-row values; please verify the configuration.");
    m_TreatLinefeedAsSpace = treatLinefeedAsSpace;
    m_TreatUnknownCharacterAsSpace = treatUnknownCharacterAsSpace;
    m_TreatUnknownCharacterMessage = m_TreatUnknownCharacterAsSpace
          ? "Unknown Character '�' found, this character was replaced with space".AddWarningId()
          : "Unknown Character '�' found in field".AddWarningId();
    m_TryToSolveMoreColumns = tryToSolveMoreColumns;
    m_WarnDelimiterInValue = warnDelimiterInValue;
    m_WarnLineFeed = warnLineFeed;
    m_WarnNbsp = warnNbsp;
    m_WarnQuotes = warnQuotes;
    m_WarnUnknownCharacter = warnUnknownCharacter;
    m_HasFieldHeader = hasFieldHeader;
    m_SkipEmptyLines = skipEmptyLines;
    m_ConsecutiveEmptyRowsMax = consecutiveEmptyRowsMax<1 ? int.MaxValue : consecutiveEmptyRowsMax;
    m_TreatNbspAsSpace = treatNbspAsSpace;
    m_TreatNbspMessage = treatNbspAsSpace ? "Character Non Breaking Space found, this character was treated as space".AddWarningId()
                  : "Character Non Breaking Space found in field".AddWarningId();
    m_TrimmingOption = trimmingOption;
    m_TreatTextAsNull = treatTextAsNull ?? string.Empty;
    m_IdentifierInContainer = identifierInContainer ?? string.Empty;
    m_WarnEmptyTailingColumns = warnEmptyTailingColumns;

    // Init with space which is after cr lf
    int structuralMax = 32;
    if (m_FieldDelimiter>structuralMax)
      structuralMax = m_FieldDelimiter;
    if (m_FieldQualifier>structuralMax)
      structuralMax = m_FieldQualifier;
    if (m_EscapePrefix>structuralMax)
      structuralMax = m_EscapePrefix;
    m_SafeBorder = (char) structuralMax;
    m_ProcessingBuffer = ArrayPool<char>.Shared.Rent(1024);
  }

  /// <inheritdoc />
  /// <summary>
  ///   Gets a value indicating whether this instance is closed.
  /// </summary>
  /// <value><character>true</character> if this instance is closed; otherwise, <character>false</character>.</value>
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
  public new IDataReader GetData(int i) => throw new NotSupportedException();

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

  /// <inheritdoc  />
  protected override void InitColumn(int fieldCount)
  {
    base.InitColumn(fieldCount);

    MaxColumnLengths = new int[fieldCount];
    for (int i = 0; i < fieldCount; i++)
      MaxColumnLengths[i] = 32;
  }

  /// <inheritdoc cref="IFileReader.OpenAsync" />
  /// <summary>
  ///   Open the file Reader; Start processing the Headers and determine the maximum column size
  /// </summary>
  public override async Task OpenAsync(CancellationToken cancellationToken)
  {
    await BeforeOpenAsync($"Opening file \"{FullPath.GetShortDisplayFileName()}\"")
      .ConfigureAwait(false);
    try
    {
      if (SelfOpenedStream)
      {

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        if (m_Stream is not null)
          await m_Stream.DisposeAsync().ConfigureAwait(false);
#else
        m_Stream?.Dispose();
#endif
        m_Stream = m_StreamProvider(new SourceAccess(FullPath)
        {
          IdentifierInContainer = m_IdentifierInContainer
        });
      }
      else
      {
        if (m_Stream is null)
          throw new FileReaderException("Stream for reading is not provided");
      }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      if (m_TextReader is not null)
        await m_TextReader.DisposeAsync().ConfigureAwait(false);
#else
      m_TextReader?.Dispose();
#endif
      m_TextReader = await m_Stream.GetTextReaderAsync(m_CodePageId, m_SkipRows, cancellationToken).ConfigureAwait(false);

      EndLineNumber = 1 + m_SkipRows;
      StartLineNumber = EndLineNumber;
      RecordNumber = 0;
      m_EndOfLine = false;
      EndOfFile = false;

      m_HeaderRow = ReadNextRow(false);
      for (int i = 0; i<m_SkipRowsAfterHeader; i++)
        ReadNextRow(false);

      InitColumn(ParseFieldCount(m_HeaderRow));

      ParseColumnName(m_HeaderRow, null, m_HasFieldHeader);

      // Turn off un-escaped warning based on WarnLineFeed
      if (!m_WarnLineFeed)
        foreach (var col in Column)
          if (col.ColumnFormatter is TextUnescapeFormatter unescapedFormatter)
            unescapedFormatter.RaiseWarning = false;

      if (m_TryToSolveMoreColumns && m_FieldDelimiter != char.MinValue)
        m_ColumnMerger = new CsvColumnMerger(FieldCount, m_FieldDelimiter);

      // if Seek is supported ParseFieldCount has read extra columns, need to return, otherwise we are on the first data row
      if (m_TextReader.CanSeek)
        ResetPositionToFirstDataRow();

      FinishOpen();
    }
    catch (Exception ex)
    {
      if (ShouldRetry(ex, cancellationToken))
      {
        await OpenAsync(cancellationToken).ConfigureAwait(false);
        return;
      }
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
      await CloseAsync().ConfigureAwait(false);
#else
      Close();
#endif
      var appEx = new FileReaderException(
        "Error opening text file for reading.\nPlease make sure the file does exist, is of the right type and is not locked by another process.",
        ex);
      HandleError(-1, appEx.ExceptionMessages());
      HandleReadFinished();
      throw appEx;
    }
  }

  /// <inheritdoc cref="BaseFileReader" />
  protected sealed override ValueTask<bool> ReadCoreAsync(CancellationToken cancellationToken)
  {
    if (!EndOfFile && !cancellationToken.IsCancellationRequested)
    {
      // GetNextRecordAsync does take care of RecordNumber
      var couldRead = GetNextRecord();
      InfoDisplay(couldRead);

      if (couldRead && !IsClosed && RecordNumber <= RecordLimit)
        return new ValueTask<bool>(result: true);
    }

    EndOfFile = true;
    HandleReadFinished();
    return new ValueTask<bool>(false);
  }

  /// <inheritdoc cref="IFileReader" />
  public new void ResetPositionToFirstDataRow()
  {
    ResetPositionToStartOrOpen();
    if (m_HasFieldHeader)
      // Read the header currentRow, this could be more than one line
      ReadNextRow(false);
    // Read the header currentRow, this could be more than one line
    for (int i = 0; i<m_SkipRowsAfterHeader; i++)
      ReadNextRow(false);
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing)
  {
    if (SelfOpenedStream && disposing)
    {
      m_Stream?.Dispose();
      m_Stream = null;
    }

    if (disposing)
    {
      ArrayPool<char>.Shared.Return(m_ProcessingBuffer);
      m_TextReader?.Dispose();
      m_TextReader = null;
    }

    base.Dispose(disposing);
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
  ///   Current Line Number in the text file, a record can span multiple lines and lines are
  ///   skipped, this is he ending line
  /// </summary>
  public override long EndLineNumber { get => m_EndLineNumber; protected set => m_EndLineNumber = value; }

  /// <summary>
  ///   Reads the next record and stores it in CurrentRowColumnText, handling column mismatches.
  /// </summary>
  /// <returns>True if a new record was read; otherwise, false.</returns>
  private bool GetNextRecord()
  {
    try
    {
      bool readRowAgain;
      IReadOnlyList<string> currentRow;
      do
      {
        readRowAgain = false;
        currentRow = ReadNextRow(true);
        if (AllEmptyAndCountConsecutiveEmptyRows(currentRow))
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

        if (currentRow.Count != FieldCount || !m_HasFieldHeader || !m_SkipDuplicateHeader) continue;
        var isRepeatedHeader = true;
        for (var col = 0; col < FieldCount; col++)
          if (!m_HeaderRow[col].Equals(currentRow[col], StringComparison.OrdinalIgnoreCase))
          { isRepeatedHeader = false; break; }

        if (!isRepeatedHeader) continue;
        HandleWarning(-1, "Repeated Header row is ignored");
        readRowAgain = true;

      } while (readRowAgain);

      // Option a) Supported - We have a break in a middle column, the missing columns are pushed
      // in the next currentRow(s) // Option b) Not Supported - We have a line break in the last column,
      // the text of this currentRow belongs to the last Column of the last records, as the last record
      // had been processed already we can not change it anymore...
      if (currentRow.Count < FieldCount)
      {
        // if we still have only one column, and we should have a number of columns assume this was
        // nonsense like a report footer
        if (currentRow.Count == 1 && EndOfFile && currentRow[0].Length < 10)
        {
          // As the record is ignored this will most likely not be visible
          // -2 to indicate this error could be stored with the previous line....
          if (m_WarnEmptyTailingColumns)
            HandleWarning(-2, $"Last line is '{currentRow[0]}'. Assumed to be a EOF marker and ignored.");
          return false;
        }

        if (!m_AllowRowCombining)
        {
          HandleWarning(-1, $"Line {cLessColumns} ({currentRow.Count}/{FieldCount}).");
        }
        else
        {
          var startLine = StartLineNumber;

          // get the next currentRow
          var nextLine = ReadNextRow(true);
          StartLineNumber = startLine;

          // allow up to two extra columns they can be combined later
          if (nextLine.Count > 0 && nextLine.Count + currentRow.Count < FieldCount + 4)
          {
            var combined = new List<string>(currentRow);

            // the first column belongs to the last column of the previous ignore
            // NumWarningsLinefeed otherwise as this is important information
            m_NumWarningsLinefeed++;
            HandleWarning(currentRow.Count - 1, $"Combined with line {EndLineNumber}, assuming a linefeed has split the column into additional line.");
            combined[currentRow.Count - 1] += '\n' + nextLine[0];

            for (var col = 1; col < nextLine.Count; col++)
              combined.Add(nextLine[col]);

            currentRow = combined;
          }

          // we have an issue we went into the next Buffer there is no way back.
          HandleError(-1,
            $"Line {cLessColumns}\nAttempting to combine lines; some line(s) have been read and this information is now lost. Please turn off Row Combination.");
        }
      }

      // If more columns are present
      if (currentRow.Count > FieldCount)
      {
        var text = $"Line {cMoreColumns} ({currentRow.Count:N0}/{FieldCount:N0}).";
        // check if the additional columns have contents
        var hasContent = false;
        for (var extraCol = FieldCount; extraCol < currentRow.Count; extraCol++)
        {
          if (string.IsNullOrEmpty(currentRow[extraCol]))
            continue;
          hasContent = true;
          break;
        }

        if (!hasContent)
        {
          if (m_WarnEmptyTailingColumns)
            HandleWarning(-1, text + " All additional columns were empty.");
        }
        // there is something in the last columns
        else
        {
          if (m_ColumnMerger != null)
          {
            HandleWarning(-1, text + " Trying to realign columns.");

            // determine which column could have caused the issue it could be any column, try to establish
            currentRow = m_ColumnMerger.MergeMisalignedColumns(currentRow, HandleWarning,
              (col, raw) =>
              {
                var pos = 0;
                bool eol = false;
                long ignored = 0;

                // Need to add quoting so parse does not stop with the included delimiter
                if (m_FieldQualifier!=0 && raw[0] != m_FieldQualifier)
                  raw = $"{m_FieldQualifier}{raw}{m_FieldQualifier}";

                return ParseColumn(
                  readChar: () => raw[pos++],
                  peekChar: () => raw[pos],
                  moveNext: (c) => pos++,
                  endOfFile: () => pos >= raw.Length,
                  columnNo: col,
                  endOfLine: ref eol,
                  ref ignored)!;
              }, m_RecordSource.ToString());
          }
          else
          {
            HandleWarning(-1,
              text + " The data in extra columns is not read. Allow 're-align columns' to handle this.");
          }
        }
      }

      // Handle Text replacements and warning in the read columns
      for (var columnNo = 0; columnNo < FieldCount; columnNo++)
      {
        // store the values from the currentRow, even ignore rows need to be copied, in case they are referenced like a time column 
        if (columnNo < currentRow.Count)
          CurrentRowColumnText[columnNo] = currentRow[columnNo];
        else
          CurrentRowColumnText[columnNo] = string.Empty;

        if (GetColumn(columnNo).Ignore || string.IsNullOrEmpty(CurrentRowColumnText[columnNo]))
          continue;

        // Handle replacements and warnings etc.
        var adjustedSpan = HandleTextSpecials(
          CurrentRowColumnText[columnNo]
            .ReplaceCaseInsensitive(m_NewLinePlaceholder, "\n")
            .ReplaceCaseInsensitive(m_DelimiterPlaceholder, m_FieldDelimiter.ToStringHandle0())
            .ReplaceCaseInsensitive(m_QuotePlaceholder, m_FieldQualifier.ToStringHandle0()).AsSpan(),
          columnNo);

        if (adjustedSpan.Length > 0)
        {
          if (m_WarnQuotes && adjustedSpan.IndexOf(m_FieldQualifier) != -1 &&
              (m_NumWarning < 1 || m_NumWarningsQuote++ < m_NumWarning))
            HandleWarning(columnNo, $"Field qualifier '{m_FieldQualifier.Text()}' found in field".AddWarningId());

          if (m_WarnDelimiterInValue && adjustedSpan.IndexOf(m_FieldDelimiter) != -1 &&
              (m_NumWarning < 1 || m_NumWarningsDelimiter++ < m_NumWarning))
            HandleWarning(columnNo, $"Field delimiter '{m_FieldDelimiter.Text()}' found in field".AddWarningId());

          if (m_WarnUnknownCharacter)
          {
            var numberQuestionMark = 0;
            var lastPos = -1;
            var length = adjustedSpan.Length;
            for (var pos = length - 1; pos >= 0; pos--)
            {
              if (adjustedSpan[pos] != '?')
                continue;
              numberQuestionMark++;

              // If we have at least two and there are two consecutive or more than 3+ in 12
              // characters, or 4+ in 16 characters
              if (numberQuestionMark > 2 && (lastPos == pos + 1 || numberQuestionMark > length / 4))
              {
                if (m_NumWarning < 1 || m_NumWarningsUnknownChar++ < m_NumWarning)
                  HandleWarning(columnNo, "Unusual high occurrence of ? this indicates unmapped characters.".AddWarningId());
                break;
              }

              lastPos = pos;
            }
          }

          if (m_WarnLineFeed && (adjustedSpan.IndexOfAny(new[] { '\r', '\n' }) != -1))
            WarnLinefeed(columnNo);

          if (adjustedSpan.ShouldBeTreatedAsNull(m_TreatTextAsNull.AsSpan()))
            adjustedSpan = Array.Empty<char>();
        }
#if NET7_0_OR_GREATER
          CurrentRowColumnText[columnNo] = new string(adjustedSpan);
#else
        CurrentRowColumnText[columnNo] = adjustedSpan.ToString();
#endif
      }

      RecordNumber++;
      m_ColumnMerger?.AddAlignedRow(CurrentRowColumnText);
      for (int i = 0; i < FieldCount; i++)
      {
        int currentLength = CurrentRowColumnText[i]?.Length ?? 0;
        int prevMax = MaxColumnLengths[i];

        // Weighted average update
        int newMax = (prevMax * 3 + currentLength) / 4 + 16;

        // Minimum buffer length
        MaxColumnLengths[i] = Math.Max(32, newMax);
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
  /// <returns><character>true</character> if the character is a whitespace</returns>
  private bool IsWhiteSpace(char c)
  {
    // Handle cases where the delimiter is a whitespace (e.g. tab)
    if (c == m_FieldDelimiter)
      return false;

    // See char.IsLatin1(char character) in Reflector
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
    if (m_ColumnMerger != null)
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
    if (headerRow.Count == 0 || headerLength == 0 ||
        headerLength < Enumerable.Count(headerRow.TakeWhile(x => !string.IsNullOrEmpty(x))))
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
      if (nextLine.Count < fields)
        break;

      // special case of missing linefeed, the line is twice as long minus 1 because of the
      // combined column in the middle
      if (nextLine.Count == (fields * 2) - 1)
        continue;

      while (nextLine.Count > fields)
      {
        HandleWarning(fields, $"No header for last {nextLine.Count - fields} column(s)".AddWarningId());
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
      HandleWarning(fields, "The last column does not have a column name and seems to be empty, this column will be ignored."
          .AddWarningId());
      return fields - 1;
    }

    return fields;
  }

  /// <summary>
  ///   Gets the next char from the buffer, but stay at the current position
  /// </summary>
  /// <returns>The next char</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private char Peek()
  {
    var res = m_TextReader!.Peek();
    if (res != -1)
      return (char) res;
    EndOfFile = true;

    // return a linefeed to determine the end of a line
    return cLf;
  }

  /// <summary>
  /// Reads the next character from the CSV input stream.
  /// Handles end-of-file detection and optionally appends characters
  /// to the record buffer if column merging is enabled.
  /// </summary>
  /// <returns>
  /// The next character read from the input stream.  
  /// If the end of the file is reached, returns <c>cLf</c> to signal
  /// an artificial line break and sets EndOfFile to <c>true</c>.
  /// </returns>
  private char ReadChar()
  {
    int res = m_TextReader!.Read();
    if (res == -1)
    {
      EndOfFile = true;
      return cLf; // Return linefeed at EOF
    }

    var character = (char) res;
    if (m_ColumnMerger != null)
      m_RecordSource.Append(character);
    return character;
  }

  /// <summary>
  ///   Parses the next column from the input stream using provided character access functions.
  ///   Handles field delimiters, line breaks (CR/LF), whitespace trimming, qualifiers (quotes),
  ///   escape sequences, and special characters such as non-breaking spaces and unknown chars.
  ///   
  ///   Behavior:
  ///   - Starts a new line if the previous field ended with a line break.
  ///   - Supports quoted fields with optional escape/duplicate qualifier handling.
  ///   - Treats CR/LF combinations correctly, including mixed order (LF+CR).
  ///   - Can replace line feeds inside quoted fields with spaces (if configured).
  ///   - Emits warnings for special characters (e.g., NBSP, unknown chars) depending on settings.
  ///   - Applies trimming rules (None, All, or Unquoted) before returning the final value.
  /// 
  ///   Returns:
  ///   - The parsed column text, with applied transformations.
  ///   - <c>null</c> if at end of file or if a new line has just started.
  /// </summary>
  /// <param name="readChar">Delegate that returns the next character and advances the reader position.</param>
  /// <param name="peekChar">Delegate that returns the next character without advancing the reader.</param>
  /// <param name="moveNext">Delegate that consumes the specified character, advancing the reader.</param>
  /// <param name="endOfFile">Delegate that indicates whether the end of the input stream has been reached.</param>
  /// <param name="columnNo">Zero-based column index currently being parsed.</param>
  /// <param name="endOfLine">
  ///   Reference flag set to <c>true</c> when the parser encounters a line break,
  ///   indicating the end of the current row.
  /// </param>re
  /// <param name="endLineNumber">
  ///   Reference counter tracking the line number up to and including the parsed value,
  ///   incremented on line breaks.
  /// </param>
  private string? ParseColumn(Func<char> readChar, Func<char> peekChar, Action<char> moveNext, Func<bool> endOfFile, int columnNo, ref bool endOfLine, ref long endLineNumber)
  {
    if (endOfFile())
      return null;
    if (endOfLine)
    {
      // previous item was last in line, start new line
      endOfLine = false;
      return null;
    }
    // Allocate builder with historical capacity to prevent internal array resizing
    int charCount = 0;
    bool quoted = false, preData = true, postData = false, escaped = false;
    // At the top of the method
    bool isValidColumn = columnNo < FieldCount;
    bool isIgnored = isValidColumn && GetColumn(columnNo).Ignore;
    bool canWarn = isValidColumn && !isIgnored;
    while (!endOfFile())
    {
      // Read a character
      char character = readChar();

      // 1. Structural Break Check (The most frequent exit condition)
      if (!escaped)
      {
        if (character == m_FieldDelimiter && (postData || !quoted))
          break;

        if (character is cCr or cLf && (preData || postData || !quoted))
        {
          endOfLine = true;
          break;
        }
      }

      // 2. FAST-PATH (The "Happy Path" for 90% of characters)
      // Avoids all complex logic for standard alphanumeric data.
      if (character > m_SafeBorder && character < cNbsp)
      {
        Append(character);
        preData = false;
        continue;
      }

      // 3. SLOW-PATH (Special Characters & Logic)
      if (character >= cNbsp)
      {
        if (character == cNbsp && !postData)
        {
          if (m_WarnNbsp && canWarn && (m_NumWarning < 1 || m_NumWarningsNbspChar++ < m_NumWarning))
            HandleWarning(columnNo, m_TreatNbspMessage);
          if (m_TreatNbspAsSpace) character = ' ';
        }
        else if (character == cUnknownChar && !postData)
        {
          if (m_WarnUnknownCharacter && canWarn && (m_NumWarning < 1 || m_NumWarningsUnknownChar++ < m_NumWarning))
            HandleWarning(columnNo, m_TreatUnknownCharacterMessage);
          if (m_TreatUnknownCharacterAsSpace) character = ' ';
        }
        else
        {
          Append(character);
          preData = false;
          continue;
        }
      }

      // Linefeed Handling
      if (character is cCr or cLf)
      {
        if (!postData && m_TreatLinefeedAsSpace && character == cLf && quoted && (endOfFile() || peekChar() != cCr))
        {
          character = ' ';
          if (m_WarnLineFeed) WarnLinefeed(columnNo);
        }
        endLineNumber++;

        if (!endOfFile())
        {
          char nextChar = peekChar();
          if ((character == cCr && nextChar == cLf) || (character == cLf && nextChar == cCr))
          {
            moveNext(nextChar);
            if (character == cLf && nextChar == cCr) endLineNumber++;
            if (quoted && !postData)
            {
              Append(character);
              Append(nextChar);
              continue;
            }
          }
        }
      }

      if (postData) continue;

      // 4. Quoting and Leading Whitespace Trimming
      if (preData)
      {
        // Faster check than char.IsWhiteSpace for common ASCII spaces
        if (character == ' ' || character == '\t' || char.IsWhiteSpace(character))
        {
          if (m_TrimmingOption == TrimmingOptionEnum.None) Append(character);
          continue;
        }
        preData = false;
        if (character == m_FieldQualifier && !escaped) { quoted = true; continue; }
      }
      else if (character == m_FieldQualifier && quoted && !escaped)
      {
        char peekNextChar = peekChar();
        if (m_DuplicateQualifierToEscape && peekNextChar == m_FieldQualifier)
        {
          Append(m_FieldQualifier);
          moveNext(peekNextChar);
          continue;
        }
        if (!m_ContextSensitiveQualifier || (peekNextChar == m_FieldDelimiter || peekNextChar is cCr or cLf))
        {
          postData = true;
          continue;
        }
      }

      // 5. Escape handling
      if (escaped && (character == m_FieldQualifier || character == m_FieldDelimiter || character == m_EscapePrefix))
      {
        charCount--; // Overwrite the prefix in the buffer
      }

      Append(character);
      escaped = !escaped && character == m_EscapePrefix;
    }

    // Return null immediately if the column is ignored
    if (isIgnored)
      return null;

    // 2. Virtual Trimming: Find the start and end within our buffer
    int start = 0;
    int end = charCount;

    if (m_TrimmingOption == TrimmingOptionEnum.All || (!quoted && m_TrimmingOption == TrimmingOptionEnum.Unquoted))
    {
      while (start < end && char.IsWhiteSpace(m_ProcessingBuffer[start])) start++;
      while (end > start && char.IsWhiteSpace(m_ProcessingBuffer[end - 1])) end--;
    }

    // Update historical length
    if (columnNo < MaxColumnLengths.Length)
      MaxColumnLengths[columnNo] = Math.Max(MaxColumnLengths[columnNo], end - start);

    // Optimization: Return the constant for empty strings
    if (start >= end)
      return string.Empty;

    return new string(m_ProcessingBuffer, start, end - start);

    // local method that will appendto buffer and resize if needed
    void Append(char character)
    {
      if (isIgnored)
        return;
      if (charCount == m_ProcessingBuffer.Length)
      {
        char[] newBuffer = ArrayPool<char>.Shared.Rent(m_ProcessingBuffer.Length * 2);
        Array.Copy(m_ProcessingBuffer, newBuffer, m_ProcessingBuffer.Length);
        ArrayPool<char>.Shared.Return(m_ProcessingBuffer);
        m_ProcessingBuffer = newBuffer;
      }
      m_ProcessingBuffer[charCount++] = character;
    }
  }

  /// <summary>
  ///   Reads the record of the CSV file, which can span multiple lines. This method can return more or fewer columns than expected.
  /// </summary>
  /// <param name="raiseWarnings">Set to true if warnings should be issued.</param>
  /// <returns>
  ///   An empty list if the row cannot be read, or string values representing the columns of the row.
  /// </returns>
  private IReadOnlyList<string> ReadNextRow(bool raiseWarnings)
  {
    bool restart;
    // Special handling for the first column in the row
    string? item;
    do
    {
      restart = false;
      // Store the starting Line Number
      StartLineNumber = EndLineNumber;
      if (m_ColumnMerger != null)
        m_RecordSource.Clear();

      // If already at end of file, return null
      if (EndOfFile || m_TextReader is null)
        return new string[FieldCount];

      item = ParseColumn(ReadChar, Peek, MoveNext, () => EndOfFile, 0, ref m_EndOfLine, ref m_EndLineNumber);

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
        if (m_EndOfLine) m_EndOfLine = false;
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
      // If a column is quoted and contains the delimiter and linefeed, issue a warning; we
      // might have an opening delimiter with a missing closing delimiter
      if (raiseWarnings && EndLineNumber > StartLineNumber + 4 && item.Length > 1024
          && item.IndexOf(m_FieldDelimiter) != -1)
        HandleWarning(col, $"Column has {EndLineNumber - StartLineNumber + 1} lines and has a length of {item.Length} characters".AddWarningId());
      columns.Add(item);

      col++;
      item = ParseColumn(ReadChar, Peek, MoveNext, () => EndOfFile, col, ref m_EndOfLine, ref m_EndLineNumber);
    }

    return columns;
  }

  /// <summary>
  /// Resets the reading position and internal buffers to the first line of the file
  /// </summary>
  /// <exception cref="InvalidOperationException">
  /// Thrown if the reader cannot seek and the stream was not opened by this instance.
  /// </exception>
  private void ResetPositionToStartOrOpen()
  {
    if (m_TextReader?.CanSeek ?? false)
    {
      m_TextReader.ToBeginning();
    }
    else if (SelfOpenedStream)
    {
      // Dispose and reopen the stream from the start
      // TextReader Dispose will take care of the stream
      m_TextReader?.Dispose();

      m_Stream = m_StreamProvider(new SourceAccess(FullPath)
      {
        IdentifierInContainer = m_IdentifierInContainer
      });

      m_TextReader = m_Stream
        .GetTextReaderAsync(m_CodePageId, m_SkipRows, CancellationToken.None)
        .ConfigureAwait(false)
        .GetAwaiter()
        .GetResult();
    }
    else
    {
      throw new InvalidOperationException(
        "Resetting to the start of the stream is not supported for externally provided streams.");
    }

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
    /// <inheritdoc cref="BaseFileReader" />
    public new async ValueTask DisposeAsync()
    {
      await DisposeAsyncCore().ConfigureAwait(false);

      Dispose(false);
    }

    /// <inheritdoc cref="BaseFileReader" />
    protected async ValueTask DisposeAsyncCore()
    {
      if (m_Stream != null &&  SelfOpenedStream)
        await m_Stream.DisposeAsync().ConfigureAwait(false);
    }

#endif
}