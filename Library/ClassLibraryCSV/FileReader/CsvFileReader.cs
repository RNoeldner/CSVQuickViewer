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

  private static readonly bool[] m_IsLowAsciiWhitespace = new bool[33];
  private readonly bool m_AllowRowCombining;
  private readonly int m_CodePageId;
  private readonly string m_CommentLine;
  private readonly int m_ConsecutiveEmptyRowsMax;
  private readonly bool m_ContextSensitiveQualifier;
  // The m_CurrentRowColumns conatins the columns in the row
  // Here the number of columnns could vary, you woudl see
  // ignored columns or if its not well formed more than expected
  private readonly RowColumnsBuffer m_CurrentRowColumns = new RowColumnsBuffer();

  private readonly bool m_DuplicateQualifierToEscape;
  private readonly char m_EscapePrefix;
  private readonly char m_FieldDelimiter;
  private readonly char m_FieldQualifier;
  private readonly bool m_HasFieldHeader;
  private readonly string m_IdentifierInContainer;
  private readonly int m_NumMaxWarning;
  /// <summary>
  ///   Stores the raw text of the current record before column splitting and trimming.
  ///   Cleared for each new row; used only when <see cref="m_ColumnMerger"/> is active.
  /// </summary>
  private readonly StringBuilder m_RecordSource = new StringBuilder(100);
  private readonly bool m_SkipDuplicateHeader;
  private readonly bool m_SkipEmptyLines;
  private readonly int m_SkipRows;
  private readonly int m_SkipRowsAfterHeader;
  private readonly StreamProviderDelegate m_StreamProvider;
  private readonly (string, string)[] m_TextSpecials;
  private readonly bool m_TreatLinefeedAsSpace;
  private readonly bool m_TreatNbspAsSpace;
  private readonly string m_TreatNbspMessage;
  private readonly bool m_TreatUnknownCharacterAsSpace;
  private readonly string m_TreatUnknownCharacterMessage;
  private readonly TrimmingOptionEnum m_TrimmingOption;
  private readonly bool m_TryToSolveMoreColumns;
  private readonly bool m_WarnDelimiterInValue;
  private readonly bool m_WarnEmptyTailingColumns;
  private readonly bool m_WarnLineFeed;
  private readonly string m_WarnLineFeedMessage;
  private readonly bool m_WarnNbsp;
  private readonly bool m_WarnQuotes;
  private readonly bool m_WarnUnknownCharacter;

  /// <summary>
  ///   Supports column realignment when enabled.  
  ///   Maintains a sliding window of well-formed rows and attempts to correct misaligned ones.
  /// </summary>
  private CsvColumnMerger? m_ColumnMerger;

  private int m_ConsecutiveEmptyRows;
  private long m_EndLineNumber;
  /// <summary>
  ///   Indicates whether the end of the current line has been reached.
  /// </summary>
  private bool m_EndOfLine;

  private IReadOnlyList<string> m_HeaderRow;
  /// <summary>
  ///   Number of records in the source file; set only when the entire file has been read.
  /// </summary>
  private ushort m_NumWarnings;
  private char[] m_PendingWhitespace;
  private char[] m_ProcessingBufferColumn;
  private Stream? m_Stream;

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
  ///   Fully qualified path of the CSV file to read.
  /// </summary>
  /// <param name="codePageId">
  ///   Initializes a new instance of the <see cref="CsvFileReader"/> class.
  /// </param>
  /// <param name="fileName">
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
    : base(fileName, columnDefinition, recordLimit, treatTextAsNull, destinationTimeZone, allowPercentage, removeCurrency)
  {
    SelfOpenedStream = !string.IsNullOrEmpty(fileName);
    m_StreamProvider = FunctionalDI.GetStream;
    m_HeaderRow = [];
    m_EscapePrefix = escapeCharacterChar;
    m_FieldDelimiter = fieldDelimiterChar;
    m_FieldQualifier = fieldQualifierChar;

    if (m_FieldDelimiter == char.MinValue) throw new FileReaderException("All delimited text files do need a delimiter.");
    if (m_FieldQualifier is cCr or cLf) throw new FileReaderException("The text qualifier characters is invalid, please use something else than CR or LF");
    if (m_FieldDelimiter is cCr or cLf or ' ') throw new FileReaderException("The field delimiter character is invalid, please use something else than CR, LF or Space");

    if (m_EscapePrefix != char.MinValue && (m_FieldDelimiter == m_EscapePrefix || m_FieldQualifier == m_EscapePrefix))
      throw new FileReaderException($"The escape character is invalid, please use something else than the field delimiter or qualifier character {m_EscapePrefix.Text()}.");

    if (m_FieldQualifier != char.MinValue && m_FieldQualifier == m_FieldDelimiter)
      throw new ArgumentOutOfRangeException($"The field qualifier and the field delimiter characters of a delimited file cannot be the same character {m_FieldDelimiter.Text()}");

    m_AllowRowCombining = allowRowCombining;
    m_ContextSensitiveQualifier = contextSensitiveQualifier;
    m_CodePageId = codePageId;
    m_CommentLine = commentLine;

    var illegal = new[] { (char) 0x0a, (char) 0x0d, m_FieldDelimiter, m_FieldQualifier };
    var m_DelimiterPlaceholder = delimiterPlaceholder ?? string.Empty;
    if (m_DelimiterPlaceholder.IndexOfAny(illegal) != -1)
      throw new ArgumentException($"Invalid delimiter characters in '{m_DelimiterPlaceholder}'", nameof(delimiterPlaceholder));

    var m_NewLinePlaceholder = newLinePlaceholder ?? string.Empty;
    if (m_NewLinePlaceholder.IndexOfAny(illegal) != -1)
      throw new ArgumentException($"Invalid placeholder characters in '{m_NewLinePlaceholder}'", nameof(newLinePlaceholder));
    m_TextSpecials = [(m_NewLinePlaceholder, "\n"), (m_DelimiterPlaceholder, m_FieldDelimiter.ToStringHandle0()), (quotePlaceholder.HandleLongText(), m_FieldQualifier.ToStringHandle0())];
    m_DuplicateQualifierToEscape = duplicateQualifierToEscape;
    m_NumMaxWarning = numWarning;
    m_SkipDuplicateHeader = skipDuplicateHeader;
    m_SkipRows = skipRows<0 ? 0 : skipRows;
    m_SkipRowsAfterHeader = skipRowsAfterHeader<0 ? 0 : skipRowsAfterHeader;
    if (m_SkipRows>99 || m_SkipRowsAfterHeader>99)
      Logger.Warning("Detected unusually high skip-row values; please verify the configuration.");
    m_TreatLinefeedAsSpace = treatLinefeedAsSpace;
    m_TreatUnknownCharacterAsSpace = treatUnknownCharacterAsSpace;
    m_TryToSolveMoreColumns = tryToSolveMoreColumns;
    m_WarnDelimiterInValue = warnDelimiterInValue;
    m_WarnLineFeed = warnLineFeed;
    m_WarnLineFeedMessage = m_WarnLineFeed ? "Line feed found in field".AddWarningId() : string.Empty;
    m_WarnNbsp = warnNbsp;
    m_WarnQuotes = warnQuotes;
    m_WarnUnknownCharacter = warnUnknownCharacter;
    m_TreatUnknownCharacterMessage = m_WarnUnknownCharacter
      ? m_TreatUnknownCharacterAsSpace ? "Unknown Character '�' found, this character was replaced with space".AddWarningId()
                                       : "Unknown Character '�' found in field".AddWarningId()
      : string.Empty;

    m_HasFieldHeader = hasFieldHeader;
    m_SkipEmptyLines = skipEmptyLines;
    m_ConsecutiveEmptyRowsMax = consecutiveEmptyRowsMax<1 ? int.MaxValue : consecutiveEmptyRowsMax;
    m_TreatNbspAsSpace = treatNbspAsSpace;
    m_TreatNbspMessage = m_WarnNbsp
      ? treatNbspAsSpace ? "Character Non Breaking Space found, this character was treated as space".AddWarningId()
                         : "Character Non Breaking Space found in field".AddWarningId()
      : string.Empty;

    m_TrimmingOption = trimmingOption;
    m_IdentifierInContainer = identifierInContainer ?? string.Empty;
    m_WarnEmptyTailingColumns = warnEmptyTailingColumns;

    m_PendingWhitespace = ArrayPool<char>.Shared.Rent(64);
    m_ProcessingBufferColumn = ArrayPool<char>.Shared.Rent(512);
    // Initialize only the characters we want to treat as whitespace
    // We skip the check if the delimiter itself is one of these
    if (m_FieldDelimiter != ' ') m_IsLowAsciiWhitespace[' ']  = true; // 32
    if (m_FieldDelimiter != '\t') m_IsLowAsciiWhitespace['\t'] = true; // 9
    if (m_FieldDelimiter != '\v') m_IsLowAsciiWhitespace['\v'] = true; // 11
    if (m_FieldDelimiter != '\f') m_IsLowAsciiWhitespace['\f'] = true; // 12
  }

  /// <summary>
  ///   Current Line Number in the text file, a record can span multiple lines and lines are
  ///   skipped, this is he ending line
  /// </summary>
  public override long EndLineNumber { get => m_EndLineNumber; protected set => m_EndLineNumber = value; }

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
    m_NumWarnings = 0;
  }

  /// <inheritdoc cref="IFileReader" />
  public new long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferOffset, int length)
  {
    if (buffer is null) throw new ArgumentNullException(nameof(buffer));
    var format = GetColumn(i).ValueFormat;
    if (format.DataType != DataTypeEnum.Binary ||
        GetSpan(i).IsEmpty) return -1;
    var filePath = FileSystemUtils.GetAbsolutePath(GetSpan(i), format.ReadFolder);
    using var fs = FileSystemUtils.OpenRead(filePath);
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
  /// <remarks>
  /// This is basicall doing the same as GetValue in the base class, but its 
  /// uses the low level methoids directly to avoid the overhead of going through the object conversion twice
  /// </remarks>
  public override object GetValue(int ordinal)
  {
    var column = GetColumn(ordinal);
    if (IsDBNull(column))
      return DBNull.Value;
    object? ret = column.ValueFormat.DataType switch
    {
      DataTypeEnum.DateTime => SpanToDateTime(column, null, GetSpan(ordinal),
                                              null, GetTimeValue(ordinal), true),
      DataTypeEnum.Integer => IntPtr.Size == 4 ? (int) SpanToLong(column, GetSpan(ordinal))!
                                               : SpanToLong(column, GetSpan(ordinal)),
      DataTypeEnum.Double => SpanToDouble(column, GetSpan(ordinal)),
      DataTypeEnum.Numeric => SpanToDecimal(column, GetSpan(ordinal)),
      DataTypeEnum.Boolean => SpanToBoolean(column, GetSpan(ordinal)),
      DataTypeEnum.Guid => SpanToGuid(column.ColumnOrdinal, GetSpan(ordinal)),
      _ => GetString(ordinal)
    };
    return ret ?? DBNull.Value;
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
      m_EndOfLine = false;
      EndOfFile = false;
      _ = ReadNextRow(raiseWarnings: false);
      m_HeaderRow = [.. m_CurrentRowColumns];
      for (int i = 0; i<m_SkipRowsAfterHeader; i++)
        _ = ReadNextRow(raiseWarnings: false);

      InitColumn(ParseFieldCount());

      ParseColumnName(m_HeaderRow, null, m_HasFieldHeader);

      // Turn off un-escaped warning based on WarnLineFeed
      if (!m_WarnLineFeed)
      {
        foreach (var fmt in Column.Select(x => x.ColumnFormatter).OfType<TextUnescapeFormatter>())
          fmt.RaiseWarning = false;
      }

      if (m_TryToSolveMoreColumns && m_FieldDelimiter != char.MinValue)
        m_ColumnMerger = new CsvColumnMerger(FieldCount, m_FieldDelimiter);

      await ResetPositionToFirstDataRowAsync(cancellationToken).ConfigureAwait(false);

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

    int ParseFieldCount()
    {
      var headerLength = m_HeaderRow.Sum(x => x.Length);
      if (m_HeaderRow.Count == 0 || headerLength == 0 ||
          headerLength < Enumerable.Count(m_HeaderRow.TakeWhile(x => !string.IsNullOrEmpty(x))))
        return 0;

      var fields = m_HeaderRow.Count;

      // The last column is empty, but we expect a header column, assume if a trailing separator
      if (fields <= 1)
        return fields;

      // check if the next lines do have data in the last column
      for (var additional = 0; (m_TextReader?.CanSeek ?? false) && !EndOfFile && additional < 10; additional++)
      {
        _ = ReadNextRow(raiseWarnings: false);

        // if we have less columns than in the header exit the loop
        if (m_CurrentRowColumns.Count < fields)
          break;

        // special case of missing linefeed, the line is twice as long minus 1 because of the
        // combined column in the middle
        if (m_CurrentRowColumns.Count == (fields * 2) - 1)
          continue;

        while (m_CurrentRowColumns.Count > fields)
        {
          HandleWarning(fields, $"No header for last {m_CurrentRowColumns.Count - fields} column(s)".AddWarningId());
          fields++;
        }

        // if we have data in the column assume the header was missing
        if (!string.IsNullOrEmpty(m_CurrentRowColumns[fields - 1]))
          return fields;
      }

      // ReSharper disable once InvertIf
      // ReSharper disable once UseIndexFromEndExpression
      if (string.IsNullOrEmpty(m_HeaderRow[m_HeaderRow.Count - 1]))
      {
        HandleWarning(fields, "The last column does not have a column name and seems to be empty, this column will be ignored."
            .AddWarningId());
        return fields - 1;
      }

      return fields;
    }
  }

  /// <inheritdoc cref="IFileReader" />
  public override async ValueTask ResetPositionToFirstDataRowAsync(CancellationToken cancellationToken)
  {
    if (m_TextReader?.CanSeek ?? false)
    {
      m_TextReader.ToBeginning();
    }
    else if (SelfOpenedStream)
    {
      // Dispose and reopen the stream from the start
      if (m_TextReader!=null)
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        await m_TextReader.DisposeAsync().ConfigureAwait(false);
#else
        m_TextReader.Dispose();
#endif

      m_Stream = m_StreamProvider(new SourceAccess(FullPath)
      {
        IdentifierInContainer = m_IdentifierInContainer
      });

      m_TextReader = await m_Stream
        .GetTextReaderAsync(m_CodePageId, m_SkipRows, cancellationToken)
        .ConfigureAwait(false);
    }
    else
      throw new InvalidOperationException("Resetting to the start of the stream is not supported for externally provided streams.");

    EndLineNumber = 1 + m_SkipRows;
    StartLineNumber = EndLineNumber;
    RecordNumber = 0;
    m_EndOfLine = false;
    EndOfFile = false;
    // Read the header currentRow, this could be more than one line
    if (m_HasFieldHeader)
      _=ReadNextRow(raiseWarnings: false);
    // Read the header currentRow, this could be more than one line
    for (int i = 0; i<m_SkipRowsAfterHeader; i++)
      _=ReadNextRow(raiseWarnings: false);
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
      ArrayPool<char>.Shared.Return(m_PendingWhitespace);
      ArrayPool<char>.Shared.Return(m_ProcessingBufferColumn);
      m_CurrentRowColumns.Dispose();
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
  ///   Reads the next record and stores it in CurrentRowColumnText, handling column mismatches.
  /// </summary>
  /// <returns>True if a new record was read; otherwise, false.</returns>
  private bool GetNextRecord()
  {
    try
    {
      bool readRowAgain;
      do
      {
        readRowAgain = false;
        // Fill m_CurrentRowColumns by calling ReadNextRow
        var hasColumns = ReadNextRow(raiseWarnings: true);
        if (!hasColumns)
        {
          m_ConsecutiveEmptyRows++;
          EndOfFile |= m_ConsecutiveEmptyRows >= m_ConsecutiveEmptyRowsMax;

          if (EndOfFile)
            return false;

          // an empty line
          if (m_SkipEmptyLines)
          {
            readRowAgain = true;
            continue;
          }
        }
        else
        {
          m_ConsecutiveEmptyRows = 0;
        }

        if (m_CurrentRowColumns.Count != FieldCount || !m_HasFieldHeader || !m_SkipDuplicateHeader) continue;
        var isRepeatedHeader = true;
        for (var col = 0; col < FieldCount; col++)
        {
          if (!m_HeaderRow[col].Equals(m_CurrentRowColumns[col], StringComparison.OrdinalIgnoreCase))
          {
            isRepeatedHeader = false;
            break;
          }
        }
        if (!isRepeatedHeader) continue;
        HandleWarning(-1, "Repeated Header row is ignored");
        readRowAgain = true;

      } while (readRowAgain);

      // Option a) Supported - We have a break in a middle column, the missing columns are pushed
      // in the next currentRow(s) // Option b) Not Supported - We have a line break in the last column,
      // the text of this currentRow belongs to the last Column of the last records, as the last record
      // had been processed already we can not change it anymore...
      if (m_CurrentRowColumns.Count < FieldCount)
      {
        // if we still have only one column, and we should have a number of columns assume this was
        // nonsense like a report footer
        if (m_CurrentRowColumns.Count == 1 && EndOfFile && m_CurrentRowColumns[0].Length < 10)
        {
          // As the record is ignored this will most likely not be visible
          // -2 to indicate this error could be stored with the previous line....
          if (m_WarnEmptyTailingColumns)
            HandleWarning(-2, $"Last line is '{m_CurrentRowColumns[0]}'. Assumed to be a EOF marker and ignored.");
          return false;
        }

        if (!m_AllowRowCombining)
        {
          HandleWarning(-1, $"Line {cLessColumns} ({m_CurrentRowColumns.Count}/{FieldCount}).");
        }
        else
        {
          var startLine = StartLineNumber;

          // get the next currentRow
          var previousRowParts = m_CurrentRowColumns.ToArray();
          var hasData = ReadNextRow(raiseWarnings: true);
          StartLineNumber = startLine;

          // allow up to two extra columns they can be combined later
          if (hasData && m_CurrentRowColumns.Count > 0 && m_CurrentRowColumns.Count + previousRowParts.Length < FieldCount + 4)
          {
            m_CurrentRowColumns.Clear();
            for (int i = 0; i < previousRowParts.Length - 1; i++)
              m_CurrentRowColumns.Add(previousRowParts[i]);
            m_CurrentRowColumns.Add(previousRowParts[previousRowParts.Length-1] + '\n' + m_CurrentRowColumns[0]);
            // the first column belongs to the last column of the previous ignore
            // NumWarningsLinefeed otherwise as this is important information
            m_NumWarnings++;
            HandleWarning(previousRowParts.Length - 1, $"Combined with line {EndLineNumber}, assuming a linefeed has split the column into additional line.");
          }

          // we have an issue we went into the next Buffer there is no way back.
          HandleError(-1,
            $"Line {cLessColumns}\nAttempting to combine lines; some line(s) have been read and this information is now lost. Please turn off Row Combination.");
        }
      }

      // If more columns are present
      if (m_CurrentRowColumns.Count > FieldCount)
      {
        var text = $"Line {cMoreColumns} ({m_CurrentRowColumns.Count:N0}/{FieldCount:N0}).";
        // check if the additional columns have contents
        var hasContent = false;
        for (var extraCol = FieldCount; extraCol < m_CurrentRowColumns.Count; extraCol++)
        {
          if (string.IsNullOrEmpty(m_CurrentRowColumns[extraCol]))
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
        else if (m_ColumnMerger != null)
        {
          HandleWarning(-1, text + " Trying to realign columns.");

          // determine which column could have caused the issue it could be any column, try to establish
          var mergedResult = m_ColumnMerger.MergeMisalignedColumns(m_CurrentRowColumns, HandleWarning,
            (col, raw) =>
            {
              var pos = 0;
              bool eol = false;
              long ignored = 0;

              // Need to add quoting so parse does not stop with the included delimiter
              if (m_FieldQualifier!=0 && raw[0] != m_FieldQualifier)
                raw = $"{m_FieldQualifier}{raw}{m_FieldQualifier}";

              var len = ParseColumn(readChar: () => raw[pos++],
                peekChar: () => raw[pos], moveNext: (c) => pos++,
                endOfFile: () => pos >= raw.Length, columnNo: col,
                endOfLine: ref eol, ref ignored)!;

              return !len.HasValue || len==0 ? string.Empty : new string(m_ProcessingBufferColumn, 0, len.Value);
            }, m_RecordSource.ToString());
          // 3. Update the internal list directly instead of replacing the reference
          if (mergedResult != m_CurrentRowColumns)
          {
            m_CurrentRowColumns.Clear();
            m_CurrentRowColumns.AddRange(mergedResult);
          }
        }
        else
        {
          HandleWarning(-1, text + " The data in extra columns is not read. Allow 're-align columns' to handle this.");
        }
      }
      Clear();
      // Loop through the expecetd columns and store the values,
      // handle warnings and replacements      
      // Additional fields are ignored, missing fields are set to empty
      // This way the number of columns in CurrentRowColumnText never changes
      for (var columnNo = 0; columnNo < FieldCount; columnNo++)
      {
        // Removed the checking of Ignore here
        // store the values from the currentRow, even ignore rows need to be copied, in case they are referenced like a time column 
        if (columnNo >= m_CurrentRowColumns.Count || m_CurrentRowColumns.GetSpan(columnNo).IsEmpty)
        {
          Add(ReadOnlySpan<char>.Empty);
          continue;
        }
        var adjustedText = HandleText(m_CurrentRowColumns.GetSpan(columnNo).ReplaceMultiple(m_TextSpecials), columnNo);
        if (adjustedText.Length == 0)
        {
          Add(ReadOnlySpan<char>.Empty);
          continue;
        }
        var col = GetColumn(columnNo);
        // Additional warnings not wanted for ignored columns
        if (!col.Ignore)
        {
          if (m_WarnQuotes && adjustedText.IndexOf(m_FieldQualifier) != -1 &&
              (m_NumMaxWarning < 1 || m_NumWarnings++ < m_NumMaxWarning))
            HandleWarning(columnNo, $"Field qualifier '{m_FieldQualifier.Text()}' found in field".AddWarningId());

          if (m_WarnDelimiterInValue && adjustedText.IndexOf(m_FieldDelimiter) != -1 &&
              (m_NumMaxWarning < 1 || m_NumWarnings++ < m_NumMaxWarning))
            HandleWarning(columnNo, $"Field delimiter '{m_FieldDelimiter.Text()}' found in field".AddWarningId());

          // This is a special case of UnknownCharacter
          // soemtimes the UnknownCharacter is raed as simple ? characters
          // , so we need to check for this as well
          if (m_WarnUnknownCharacter)
          {
            var numberQuestionMark = 0;
            var lastPos = -1;
            var length = adjustedText.Length;
            for (var pos = length - 1; pos >= 0; pos--)
            {
              if (adjustedText[pos] != '?')
                continue;
              numberQuestionMark++;

              // If we have at least two and there are two consecutive or more than 3+ in 12
              // characters, or 4+ in 16 characters
              if (numberQuestionMark > 2 && (lastPos == pos + 1 || numberQuestionMark > length / 4))
              {
                if (m_NumMaxWarning < 1 || m_NumWarnings++ < m_NumMaxWarning)
                  HandleWarning(columnNo, "Unusual high occurrence of ? this indicates unmapped characters.".AddWarningId());
                break;
              }

              lastPos = pos;
            }
          }
          if (m_WarnLineFeed && (m_NumMaxWarning < 1 || m_NumWarnings++ <= m_NumMaxWarning) && adjustedText.IndexOfAny(new[] { '\r', '\n' }) != -1)
            HandleWarning(columnNo, m_WarnLineFeedMessage);
        }
        Add(adjustedText);
      }

      RecordNumber++;
      m_ColumnMerger?.AddAlignedRow(CurrentRowText);
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
  ///   A NonBreaking space is not considers as white space
  /// </summary>
  /// <param name="c">A Unicode character.</param>
  /// <returns><character>true</character> if the character is a whitespace</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private bool IsWhiteSpace(char c)
  {
    // 1. Fast Path: Low ASCII (0-32)
    if (c <= 32)
      return m_IsLowAsciiWhitespace[c];

    // 2. Middle Path: Skip Latin-1 and common symbols (including Non-Breaking Space 160)
    // This honors your requirement that Non-Breaking Space is NOT whitespace.
    if (c < 5760)
      return false;

    // 3. Slow Path: High Unicode Space Separators
    return CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator;
  }

  /// <summary>
  ///   Move to next character and store the recent character
  /// </summary>
  /// <param name="current">The recent character that has been read</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private void MoveNext(char current)
  {
    if (m_ColumnMerger != null)
      m_RecordSource.Append(current);
    m_TextReader?.MoveNext();
  }

  /// <summary>
  /// Parses the next column from the input stream using the supplied character access delegates.
  /// </summary>
  /// <remarks>
  /// This method distinguishes between <c>null</c> and <c>0</c> return values:
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// <c>null</c> indicates that no column value was produced.  
  /// This occurs when the end of the current record has been reached
  /// (for example, immediately after a line break or at end-of-file),
  /// and signals the caller to stop reading columns for the current row.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// <c>0</c> indicates a valid column with an empty value.
  /// The column exists and must be added to the current row.
  /// </description>
  /// </item>
  /// </list>
  /// Returning a positive value indicates the number of characters written
  /// for a valid, non-empty column.
  ///
  /// The method handles delimiters, line breaks (CR/LF), quoted fields,
  /// escape sequences, trimming rules, and special-character handling
  /// (such as non-breaking spaces and Unicode replacement characters),
  /// emitting warnings as configured.
  /// </remarks>
  /// <param name="readChar">
  /// Delegate that reads and returns the next character, advancing the reader position.
  /// </param>
  /// <param name="peekChar">
  /// Delegate that returns the next character without advancing the reader position.
  /// </param>
  /// <param name="moveNext">
  /// Delegate that consumes the specified character, advancing the reader.
  /// </param>
  /// <param name="endOfFile">
  /// Delegate that returns <c>true</c> when the end of the input stream has been reached.
  /// </param>
  /// <param name="columnNo">
  /// Zero-based index of the column currently being parsed.
  /// </param>
  /// <param name="endOfLine">
  /// Reference flag that is set to <c>true</c> when the parser encounters a line break,
  /// indicating the end of the current record.
  /// </param>
  /// <param name="endLineNumber">
  /// Reference counter tracking the current line number; incremented when line breaks are encountered.
  /// </param>
  /// <returns>
  /// <para>
  /// <c>null</c> if no column should be produced (end of record or end of file).
  /// </para>
  /// <para>
  /// <c>0</c> if an empty column was parsed.
  /// </para>
  /// <para>
  /// A positive integer representing the number of characters written for a non-empty column.
  /// </para>
  /// </returns>
  private int? ParseColumn(Func<char> readChar, Func<char> peekChar, Action<char> moveNext, Func<bool> endOfFile, int columnNo, ref bool endOfLine, ref long endLineNumber)
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
    bool quoted = false, preData = true, postData = false;
    bool noWarning = columnNo < 0 || columnNo >= FieldCount || GetColumn(columnNo).Ignore;
    // Store everything IF we are combining rows OR solving more columns.
    // OTHERWISE, fall back to checking the m_ParseFromSource map.
    bool storeInformation = m_AllowRowCombining || m_TryToSolveMoreColumns ||
                            m_ParseFromSource.Length == 0 || columnNo >= m_ParseFromSource.Length ||
                            m_ParseFromSource[columnNo];

    int whiteSpaceCount = 0;
    while (!endOfFile())
    {
      char character = readChar();
      // Handle Field Delimiters
      if ((character == m_FieldDelimiter && (postData || !quoted)) || endOfFile())
      {
        break;
      }
      // Handle Line Breaks (CR/LF)
      if (character is cCr or cLf)
      {
        endLineNumber++;
        var nextChar = !endOfFile() ? peekChar() : char.MinValue;
        // Handle Single LF as Space
        if (character == cLf && nextChar != cCr && quoted && !postData && m_TreatLinefeedAsSpace)
          character = ' ';
        // Standard Linebreak Logic
        else if (character is cCr or cLf)
        {
          // Consume secondary part of CRLF or LFCR
          if ((character == cCr && nextChar == cLf) || (character == cLf && nextChar == cCr))
          {
            moveNext(nextChar);
            if (character == cLf && nextChar == cCr) endLineNumber++;

            // If quoted, we keep the linebreak as data
            if (quoted && !postData)
            {
              Append(character);
              Append(nextChar);
              continue;
            }
          }

          // If we reach here and it's not a quoted multi-line, it's a structural break
          if (preData || postData || !quoted)
          {
            endOfLine = true;
            break;
          }
        }
      }
      // Ignore everything after closing quote until delimiter / linefeed
      if (postData)
        continue;

      // Handle Substitutions (Nbsp, Unknown)
      // Needs to be done so in case the chars are replaced with spaces,
      // the spaces will be handled according to the trimming rules
      if (character == cNbsp)
      {
        if (m_WarnNbsp && !noWarning && (m_NumMaxWarning< 1 || m_NumWarnings++ < m_NumMaxWarning))
          HandleWarning(columnNo, m_TreatNbspMessage);
        character = m_TreatNbspAsSpace ? ' ' : character;
      }
      else if (character == cUnknownChar)
      {
        if (m_WarnUnknownCharacter && !noWarning && (m_NumMaxWarning< 1 || m_NumWarnings++ < m_NumMaxWarning))
          HandleWarning(columnNo, m_TreatUnknownCharacterMessage);
        character = m_TreatUnknownCharacterAsSpace ? ' ' : character;
      }
      // Handle Escape Sequences
      else if (character == m_EscapePrefix)
      {
        // Capture the lookahead once
        char nextChar = !endOfFile() ? peekChar() : char.MinValue;

        // Is it a valid sequence we should escape?
        if (nextChar == m_FieldQualifier || nextChar == m_FieldDelimiter || nextChar == m_EscapePrefix)
        {
          moveNext(nextChar); // Consume the escaped character
          Append(nextChar);   // Append the literal value
        }
        else
        {
          // It's either a "dangling" escape prefix at EOF or an 
          // escape prefix followed by a standard character (treat as literal)
          Append(character);
        }
        preData = false;
        continue;
      }

      if (IsWhiteSpace(character) &&
          (m_TrimmingOption == TrimmingOptionEnum.All || (!quoted && m_TrimmingOption == TrimmingOptionEnum.Unquoted)))
      {
        // If we're in the leading zone (preData), just skip.
        // Otherwise, it's internal/potential-trailing, so queue it.
        if (!preData) AppendWhitespace(character);
        continue;
      }

      if (preData)
      {
        preData = false;
        if (character == m_FieldQualifier)
        {
          quoted = true;
          // Reset preData only if 'All' is set, to trim inside the quotes.
          preData = m_TrimmingOption == TrimmingOptionEnum.All;
          continue;
        }
      }

      // 6. Handle Quoted Transitions (Double-quotes or Closing quotes)
      if (character == m_FieldQualifier && quoted)
      {
        var nextChar = !endOfFile() ? peekChar() : char.MinValue;
        if (m_DuplicateQualifierToEscape && nextChar == m_FieldQualifier)
        {
          Append(m_FieldQualifier);
          moveNext(nextChar);

          if (m_ContextSensitiveQualifier)
          {
            var afterDouble = peekChar();
            if (afterDouble == m_FieldDelimiter || afterDouble == cCr || afterDouble == cLf)
            {
              whiteSpaceCount=0;
              postData = true;
            }
          }
          continue;
        }

        // Logic for closing the quote
        if (!m_ContextSensitiveQualifier || (nextChar == m_FieldDelimiter || nextChar == cCr || nextChar == cLf))
        {
          whiteSpaceCount=0;
          postData = true;
          continue;
        }
      }
      // It's a non-whitespace character. Flush the pending queue to the main buffer.
      Append(character);
    } // While

    return charCount;

    // local method that will appendto buffer and resize if needed
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Append(char character)
    {
      if (!storeInformation) return;
      if (whiteSpaceCount > 0)
      {
        EnsureCapacity(ref m_ProcessingBufferColumn, charCount, whiteSpaceCount + 1);
        Array.Copy(m_PendingWhitespace, 0, m_ProcessingBufferColumn, charCount, whiteSpaceCount);
        charCount += whiteSpaceCount;
        whiteSpaceCount=0;
      }
      else
      {
        EnsureCapacity(ref m_ProcessingBufferColumn, charCount, 1);
      }
      m_ProcessingBufferColumn[charCount++] = character;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void AppendWhitespace(char character)
    {
      EnsureCapacity(ref m_PendingWhitespace, whiteSpaceCount, 1);
      m_PendingWhitespace[whiteSpaceCount++] = character;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void EnsureCapacity(ref char[] buffer, int currentCount, int additionalCount)
    {
      int requiredCapacity = currentCount + additionalCount;
      if (requiredCapacity > buffer.Length)
      {
        // Calculate new size: double the current or take required, whichever is larger
        int newSize = Math.Max(buffer.Length * 2, requiredCapacity);

        char[] newBuffer = ArrayPool<char>.Shared.Rent(newSize);
        // Only copy the data currently in use (currentCount)
        if (currentCount > 0)
          Array.Copy(buffer, 0, newBuffer, 0, currentCount);
        ArrayPool<char>.Shared.Return(buffer);
        buffer = newBuffer;
      }
    }
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
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
  ///   Reads the record of the CSV file, which can span multiple lines. This method can return more or fewer columns than expected.
  /// </summary>
  /// <param name="raiseWarnings">Set to true if warnings should be issued.</param>
  /// <returns>
  ///   true if at least one column was not empty.
  /// </returns>
  private bool ReadNextRow(bool raiseWarnings)
  {
    bool restart;
    // Special handling for the first column in the row
    int? len;
    bool oneColumnNotEmpty = false;
    m_CurrentRowColumns.Clear();
    do
    {
      restart = false;
      // Store the starting Line Number
      StartLineNumber = EndLineNumber;
      if (m_ColumnMerger != null)
        m_RecordSource.Clear();

      // If already at end of file, return null
      if (EndOfFile || m_TextReader is null)
        return oneColumnNotEmpty;

      len = ParseColumn(ReadChar, Peek, MoveNext, () => EndOfFile, 0, ref m_EndOfLine, ref m_EndLineNumber);

      // An empty line does not have a first column
      if ((!len.HasValue || len == 0) && m_EndOfLine)
      {
        m_EndOfLine = false;
        if (m_SkipEmptyLines)
          // go to the next line
          restart = true;
        else
          // Return it as array of empty columns
          return oneColumnNotEmpty;
      }
      // And skip commented lines
      else if (m_CommentLine.Length > 0 && len.HasValue && m_ProcessingBufferColumn.AsSpan(0, len.Value).StartsWith(m_CommentLine.AsSpan(), StringComparison.Ordinal))
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
    // We have a proper column now read the row we already have raed the inital column
    while (len != null)
    {
      // If a column is quoted and contains the delimiter and linefeed, issue a warning; we
      // might have an opening delimiter with a missing closing delimiter
      if (raiseWarnings && EndLineNumber > StartLineNumber + 4 && len > 1024
          && m_ProcessingBufferColumn.AsSpan(0, len.Value).IndexOf(m_FieldDelimiter) != -1)
        HandleWarning(m_CurrentRowColumns.Count, $"Column has {EndLineNumber - StartLineNumber + 1} lines and has a length of {len} characters".AddWarningId());
      oneColumnNotEmpty |= len>0;
      m_CurrentRowColumns.Add(m_ProcessingBufferColumn.AsSpan(0, len.Value));
      len = ParseColumn(ReadChar, Peek, MoveNext, () => EndOfFile, m_CurrentRowColumns.Count, ref m_EndOfLine, ref m_EndLineNumber);
    }
    return oneColumnNotEmpty;
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