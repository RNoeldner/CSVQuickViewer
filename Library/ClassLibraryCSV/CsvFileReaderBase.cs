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

namespace CsvTools
{
  /// <summary>
  ///   A data reader for CSV files
  /// </summary>
  public abstract class CsvFileReaderBase : BaseFileReader
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
    protected const char c_Cr = (char)0x0d;

    /// <summary> The line-feed character. Escape code is <c>\n</c>. </summary> </summary>
    protected const char c_Lf = (char)0x0a;

    /// <summary>
    ///   A non-breaking space..
    /// </summary>
    protected const char c_Nbsp = (char)0xA0;

    protected const char c_UnknownChar = (char)0xFFFD;

    protected readonly ICsvFile m_CsvFile;

    protected int m_ConsecutiveEmptyRows;

    /// <summary>
    ///   If the End of the line is reached this is true
    /// </summary>
    protected bool m_EndOfLine;

    protected bool m_HasQualifier;
    protected string[] m_HeaderRow;
    protected IImprovedStream m_ImprovedStream;
    protected ushort m_NumWarningsLinefeed;
    protected ReAlignColumns m_RealignColumns;

    /// <summary>
    ///   The TextReader to read the file
    /// </summary>
    protected ImprovedTextReader m_TextReader;

    private bool m_DisposedValue;

    /// <summary>
    ///   Number of Records in the text file, only set if all records have been read
    /// </summary>
    private ushort m_NumWarningsDelimiter;

    private ushort m_NumWarningsNbspChar;
    private ushort m_NumWarningsQuote;
    private ushort m_NumWarningsUnknownChar;

    public CsvFileReaderBase(ICsvFile fileSetting, string timeZone, IProcessDisplay processDisplay)
      : base(fileSetting, timeZone, processDisplay)
    {
      m_CsvFile = fileSetting;
      if (string.IsNullOrEmpty(m_CsvFile.FileName))
        throw new FileReaderException("FileName must be set");

      // if it can not be downloaded it has to exist
      if (string.IsNullOrEmpty(m_CsvFile.RemoteFileName) || !HasOpen())
        if (!FileSystemUtils.FileExists(m_CsvFile.FullPath))
          throw new FileNotFoundException(
            $"The file '{FileSystemUtils.GetShortDisplayFileName(m_CsvFile.FileName, 80)}' does not exist or is not accessible.",
            m_CsvFile.FileName);
      if (m_CsvFile.FileFormat.FieldDelimiterChar == c_Cr ||
          m_CsvFile.FileFormat.FieldDelimiterChar == c_Lf ||
          m_CsvFile.FileFormat.FieldDelimiterChar == ' ' ||
          m_CsvFile.FileFormat.FieldDelimiterChar == '\0')
        throw new FileReaderException(
          "The field delimiter character is invalid, please use something else than CR, LF or Space");

      if (m_CsvFile.FileFormat.FieldDelimiterChar == m_CsvFile.FileFormat.EscapeCharacterChar)
        throw new FileReaderException(
          $"The escape character is invalid, please use something else than the field delimiter character {FileFormat.GetDescription(m_CsvFile.FileFormat.EscapeCharacter)}.");

      m_HasQualifier |= m_CsvFile.FileFormat.FieldQualifierChar != '\0';

      if (!m_HasQualifier)
        return;
      if (m_CsvFile.FileFormat.FieldQualifierChar == m_CsvFile.FileFormat.FieldDelimiterChar)
        throw new ArgumentOutOfRangeException(
          $"The text quoting and the field delimiter characters of a delimited file cannot be the same. {m_CsvFile.FileFormat.FieldDelimiterChar}");
      if (m_CsvFile.FileFormat.FieldQualifierChar == c_Cr || m_CsvFile.FileFormat.FieldQualifierChar == c_Lf)
        throw new FileReaderException(
          "The text quoting characters is invalid, please use something else than CR or LF");
    }

    /// <summary>
    ///   Gets a value indicating whether this instance is closed.
    /// </summary>
    /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
    public bool IsClosed => m_TextReader == null;

    public override void Close()
    {
      m_NumWarningsQuote = 0;
      m_NumWarningsDelimiter = 0;
      m_NumWarningsUnknownChar = 0;
      m_NumWarningsNbspChar = 0;

      m_TextReader?.Dispose();
      m_ImprovedStream?.Dispose();

      base.Close();
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
    public string GetDataTypeName(int i) => GetFieldType(i).Name;

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

      return GetTypedValueFromString(CurrentRowColumnText[columnNumber], GetTimeValue(columnNumber),
        GetColumn(columnNumber));
    }

    protected bool AllEmptyAndCountConsecutiveEmptyRows(IReadOnlyList<string> columns)
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
      EndOfFile |= m_ConsecutiveEmptyRows >= m_CsvFile.ConsecutiveEmptyRows;
      return true;
    }

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
      if (disposing)
      {
        m_DisposedValue = true;
        Close();
        if (m_TextReader != null)
        {
          m_TextReader.Dispose();
          m_TextReader = null;
        }

        base.Dispose(true);
      }
    }

    /// <summary>
    ///   Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and MaxValue</returns>
    protected override int GetRelativePosition()
    {
      // if we know how many records to read, use that
      if (m_CsvFile.RecordLimit > 0)
        return (int)((double)RecordNumber / m_CsvFile.RecordLimit * cMaxValue);

      return (int)((m_ImprovedStream?.Percentage ?? 0) * cMaxValue);
    }

    /// <summary>
    ///   Indicates whether the specified Unicode character is categorized as white space.
    /// </summary>
    /// <param name="c">A Unicode character.</param>
    /// <returns><c>true</c> if the character is a whitespace</returns>
    protected bool IsWhiteSpace(char c)
    {
      // Handle cases where the delimiter is a whitespace (e.g. tab)
      if (c == m_CsvFile.FileFormat.FieldDelimiterChar)
        return false;

      // See char.IsLatin1(char c) in Reflector
      if (c <= '\x00ff')
        return c == ' ' || c == '\t';

      return CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator;
    }

    // To detect redundant calls
    protected void OpenStart()
    {
      m_HasQualifier |= m_CsvFile.FileFormat.FieldQualifierChar != '\0';

      if (m_CsvFile.FileFormat.FieldQualifier.Length > 1 &&
          m_CsvFile.FileFormat.FieldQualifier.WrittenPunctuationToChar() == '\0')
        HandleWarning(-1,
          $"Only the first character of '{m_CsvFile.FileFormat.FieldQualifier}' is be used for quoting.");
      if (m_CsvFile.FileFormat.FieldDelimiter.Length > 1 &&
          m_CsvFile.FileFormat.FieldDelimiter.WrittenPunctuationToChar() == '\0')
        HandleWarning(-1,
          $"Only the first character of '{m_CsvFile.FileFormat.FieldDelimiter}' is used as delimiter.");

      BeforeOpen();
    }

    protected string ReadNextColumnEnd(int columnNo, StringBuilder stringBuilder, bool storeWarnings, bool quoted, bool hadUnknownChar, bool hadDelimiterInValue, bool hadNbsp)
    {
      var returnValue = HandleTrimAndNBSP(stringBuilder.ToString(), quoted);

      if (!storeWarnings)
        return returnValue;
      // if (m_HasQualifier && quoted && m_StructuredWriterFile.WarnQuotesInQuotes &&
      // returnValue.IndexOf(m_StructuredWriterFile.FileFormat.FieldQualifierChar) != -1) WarnQuoteInQuotes(columnNo);
      if (m_HasQualifier && m_CsvFile.WarnQuotes && returnValue.IndexOf(m_CsvFile.FileFormat.FieldQualifierChar) > 0)
        WarnQuotes(columnNo);
      if (m_CsvFile.WarnDelimiterInValue && hadDelimiterInValue)
        WarnDelimiter(columnNo);

      if (m_CsvFile.WarnUnknowCharater)
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

      if (m_CsvFile.WarnNBSP && hadNbsp)
        WarnNbsp(columnNo);
      if (m_CsvFile.WarnLineFeed && (returnValue.IndexOf('\r') != -1 || returnValue.IndexOf('\n') != -1))
        WarnLinefeed(columnNo);
      return returnValue;
    }

    /// <summary>
    ///   Resets the position and buffer to the first line, excluding headers, use
    ///   ResetPositionToStart if you want to go to first data line
    /// </summary>
    protected void ResetPositionToStartOrOpen()
    {
      m_TextReader.ToBeginning();

      StartLineNumber = 1 + m_CsvFile.SkipRows;
      EndLineNumber = 1 + m_CsvFile.SkipRows;
      RecordNumber = 0;
      m_EndOfLine = false;
      EndOfFile = false;
    }

    /// <summary>
    ///   Add warnings for delimiter.
    /// </summary>
    /// <param name="column">The column.</param>
    protected void WarnDelimiter(int column)
    {
      m_NumWarningsDelimiter++;
      if (m_CsvFile.NumWarnings >= 1 && m_NumWarningsDelimiter > m_CsvFile.NumWarnings)
        return;
      HandleWarning(column,
        $"Field delimiter '{FileFormat.GetDescription(m_CsvFile.FileFormat.FieldDelimiter)}' found in field"
          .AddWarningId());
    }

    /// <summary>
    ///   Add warnings for Linefeed.
    /// </summary>
    /// <param name="column">The column.</param>
    protected void WarnLinefeed(int column)
    {
      m_NumWarningsLinefeed++;
      if (m_CsvFile.NumWarnings >= 1 && m_NumWarningsLinefeed > m_CsvFile.NumWarnings)
        return;
      HandleWarning(column, "Linefeed found in field".AddWarningId());
    }

    /// <summary>
    ///   Add warnings for NBSP.
    /// </summary>
    /// <param name="column">The column.</param>
    protected void WarnNbsp(int column)
    {
      m_NumWarningsNbspChar++;
      if (m_CsvFile.NumWarnings >= 1 && m_NumWarningsNbspChar > m_CsvFile.NumWarnings)
        return;
      HandleWarning(column,
        m_CsvFile.TreatNBSPAsSpace
          ? "Character Non Breaking Space found, this character was treated as space".AddWarningId()
          : "Character Non Breaking Space found in field".AddWarningId());
    }

    /// <summary>
    ///   Add warnings for quotes.
    /// </summary>
    /// <param name="column">The column.</param>
    protected void WarnQuotes(int column)
    {
      m_NumWarningsQuote++;
      if (m_CsvFile.NumWarnings >= 1 && m_NumWarningsQuote > m_CsvFile.NumWarnings)
        return;
      HandleWarning(column,
        $"Field qualifier '{FileFormat.GetDescription(m_CsvFile.FileFormat.FieldQualifier)}' found in field"
          .AddWarningId());
    }

    /// <summary>
    ///   Add warnings for unknown char.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="questionMark"></param>
    protected void WarnUnknownChar(int column, bool questionMark)
    {
      // in case the column is ignored do not warn
      if (Column[column].Ignore)
        return;

      m_NumWarningsUnknownChar++;
      if (m_CsvFile.NumWarnings >= 1 && m_NumWarningsUnknownChar > m_CsvFile.NumWarnings)
        return;
      if (questionMark)
      {
        HandleWarning(column, "Unusual high occurrence of ? this indicates unmapped characters.".AddWarningId());
        return;
      }

      HandleWarning(column,
        m_CsvFile.TreatUnknowCharaterAsSpace
          ? "Unknown Character '�' found, this character was replaced with space".AddWarningId()
          : "Unknown Character '�' found in field".AddWarningId());
    }
  }
}