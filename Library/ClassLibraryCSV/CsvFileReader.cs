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
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text;

namespace CsvTools
{
  /// <summary>
  ///  A data reader for CSV files
  /// </summary>
  public class CsvFileReader : BaseFileReader, IFileReader
  {
    /// <summary>
    ///  Constant: Line has fewer columns than expected
    /// </summary>
    public const string cLessColumns = " has fewer columns than expected";

    /// <summary>
    ///  Constant: Line has more columns than expected
    /// </summary>
    public const string cMoreColumns = " has more columns than expected";

    // Buffer size set to 64kB, if set to large the display in percentage will jump
    private const int c_Buffersize = 65536;

    /// <summary>
    ///  The carriage return character. Escape code is <c>\r</c>.
    /// </summary>
    private const char c_Cr = (char)0x0d;

    /// <summary>
    ///  The line-feed character. Escape code is <c>\n</c>.
    /// </summary>
    private const char c_Lf = (char)0x0a;

    /// <summary>
    ///  A non-breaking space..
    /// </summary>
    private const char c_Nbsp = (char)0xA0;

    private const char c_UnknownChar = (char)0xFFFD;

    /// <summary>
    ///  16k Buffer of the file data
    /// </summary>
    private readonly char[] m_Buffer = new char[c_Buffersize];

    private readonly ICsvFile m_CsvFile;

    /// <summary>
    ///  Length of the buffer (can be smaller then buffer size at end of file)
    /// </summary>
    private int m_BufferFilled;

    /// <summary>
    ///  Position in the buffer
    /// </summary>
    private int m_BufferPos;

    private int m_ConsecutiveEmptyRows;

    /// <summary>
    ///  If the End of the line is reached this is true
    /// </summary>
    private bool m_EndOfLine;

    private bool m_HasQualifier;
    private string[] m_HeaderRow = null;
    private ImprovedStream m_ImprovedStream;
    private long m_MaxRecordNumber;

    /// <summary>
    ///  Number of Records in the text file, only set if all records have been read
    /// </summary>
    private ushort m_NumWarningsDelimiter;

    private ushort m_NumWarningsLinefeed;
    private ushort m_NumWarningsNbspChar;
    private ushort m_NumWarningsQuote;
    private ushort m_NumWarningsUnknownChar;

    private ReAlignColumns m_RealignColumns = null;

    /// <summary>
    ///  The TextReader to read the file
    /// </summary>
    private StreamReader m_TextReader;

    public CsvFileReader(ICsvFile fileSetting)
     : base(fileSetting)
    {
      m_CsvFile = fileSetting;
      if (string.IsNullOrEmpty(m_CsvFile.FileName))
        throw new ApplicationException("FileName must be set");

      if (ApplicationSetting.RemoteFileHandler != null && string.IsNullOrEmpty(m_CsvFile?.RemoteFileName))
      {
        if (!FileSystemUtils.FileExists(m_CsvFile.FullPath))
          throw new FileNotFoundException(
           $"The file '{FileSystemUtils.GetShortDisplayFileName(m_CsvFile.FileName, 80)}' does not exist or is not accessible.", m_CsvFile.FileName);
      }
      if (m_CsvFile.FileFormat.FieldDelimiterChar == c_Cr ||
        m_CsvFile.FileFormat.FieldDelimiterChar == c_Lf ||
        m_CsvFile.FileFormat.FieldDelimiterChar == ' ' ||
        m_CsvFile.FileFormat.FieldDelimiterChar == '\0')
        throw new ApplicationException(
         "The field delimiter character is invalid, please use something else than CR, LF or Space");

      if (m_CsvFile.FileFormat.FieldDelimiterChar == m_CsvFile.FileFormat.EscapeCharacterChar)
        throw new ApplicationException(
         $"The escape character is invalid, please use something else than the field delimiter character {FileFormat.GetDescription(m_CsvFile.FileFormat.EscapeCharacter)}.");

      m_HasQualifier |= m_CsvFile.FileFormat.FieldQualifierChar != '\0';

      if (!m_HasQualifier) return;
      if (m_CsvFile.FileFormat.FieldQualifierChar == m_CsvFile.FileFormat.FieldDelimiterChar)
        throw new ArgumentOutOfRangeException(
         $"The text quoting and the field delimiter characters of a delimited file cannot be the same. {m_CsvFile.FileFormat.FieldDelimiterChar}");
      if (m_CsvFile.FileFormat.FieldQualifierChar == c_Cr || m_CsvFile.FileFormat.FieldQualifierChar == c_Lf)
        throw new ApplicationException(
         "The text quoting characters is invalid, please use something else than CR or LF");
    }

    /// <summary>
    ///  Gets a value indicating whether this instance is closed.
    /// </summary>
    /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
    public bool IsClosed => m_TextReader == null;

    /// <summary>
    ///  Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing">
    ///  <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
    ///  unmanaged resources.
    /// </param>
    public override void Dispose(bool disposing)
    {
      // Dispose-time code should also set references of all owned objects to null, after disposing
      // them. This will allow the referenced objects to be garbage collected even if not all
      // references to the "parent" are released. It may be a significant memory consumption win if
      // the referenced objects are large, such as big arrays, collections, etc.
      if (disposing)
      {
        IndividualClose();
        if (m_TextReader != null)
        {
          m_TextReader.Dispose();
          m_TextReader = null;
        }
      }

    }

    /// <summary>
    ///  Reads a stream of bytes from the specified column offset into the buffer as an array,
    ///  starting at the given buffer offset.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
    /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
    /// <param name="bufferoffset">The index for <paramref name="buffer" /> to start the read operation.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <exception cref="NotImplementedException"></exception>
    /// <returns>The actual number of bytes read.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///  The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => throw new NotImplementedException();

    /// <summary>
    ///  Returns an <see cref="Data.IDataReader" /> for the specified column ordinal.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <exception cref="NotImplementedException"></exception>
    /// <returns>An <see cref="Data.IDataReader" />.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///  The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public virtual IDataReader GetData(int i) => throw new NotImplementedException();

    /// <summary>
    ///  Gets the data type information for the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>The .NET type name of the column</returns>
    public string GetDataTypeName(int i)
    {
      return GetFieldType(i).Name;
    }

    /// <summary>
    ///  Return the value of the specified field.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>The object will contain the field value upon return.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///  The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public override object GetValue(int columnNumber)
    {
      if (IsDBNull(columnNumber))
        return DBNull.Value;

      return GetTypedValueFromString(CurrentRowColumnText[columnNumber], GetTimeValue(columnNumber),
       GetColumn(columnNumber));
    }

    /// <summary>
    ///  Advances the <see cref="Data.IDataReader" /> to the next record.
    /// </summary>
    /// <returns>true if there are more rows; otherwise, false.</returns>
    public override bool Read()
    {
      var couldRead = GetNextRecord();
      InfoDisplay(couldRead);

      if (couldRead && !CancellationToken.IsCancellationRequested && !IsClosed) return true;
      HandleReadFinished();
      return false;
    }

    protected override void IndividualClose()
    {
      m_NumWarningsQuote = 0;
      m_NumWarningsDelimiter = 0;
      m_NumWarningsUnknownChar = 0;
      m_NumWarningsNbspChar = 0;

      m_TextReader?.Dispose();
      m_ImprovedStream?.Dispose();

      EndOfFile = true;
    }

    /// <summary>
    ///  Open the file Reader; Start processing the Headers and determine the maximum column size
    /// </summary>
    /// <param name="determineColumnSize">
    ///  If set to <c>true</c> go through the file and get the maximum column length for each column
    /// </param>
    /// <returns>Number of records</returns>
    protected override long IndividualOpen(bool determineColumnSize)
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

      long endLineNumberIncudingComments = 0;
      bool needReset = false;
      try
      {
        ResetPositionToStart();
        m_HeaderRow = ReadNextRow(false, false);
        if (m_HeaderRow.IsEmpty())
          InitColumn(0);
        else
        {
          endLineNumberIncudingComments = (m_CsvFile.HasFieldHeader) ? EndLineNumber : 0;
          // Get the column count
          FieldCount = ParseFieldCount(m_HeaderRow, out var hasReadFurther);

          if (hasReadFurther)
            needReset = true;

          // Get the column names
          ParseColumnName(m_HeaderRow);

          // in case there was no header we need to go back
          if (!m_CsvFile.HasFieldHeader)
            needReset = true;

          if (!determineColumnSize)
            return 0;

          m_MaxRecordNumber = ParseColumnSize();
          needReset = true;
          return m_MaxRecordNumber;
        }

        return 0;
      }
      catch (Exception ex)
      {
        needReset = false;
        Close();
        throw new ApplicationException("The CSV Reader could not open the file", ex);
      }
      finally
      {
        if (m_CsvFile.TryToSolveMoreColumns && m_CsvFile.FileFormat.FieldQualifierChar == '\0')
          m_RealignColumns = new ReAlignColumns(m_CsvFile, FieldCount);
        // Need to re-position on the first data row
        if (needReset)
        {
          ResetPositionToStart();
          while (EndLineNumber < endLineNumberIncudingComments)
            ReadToEOL();
        }
      }
    }

    /// <summary>
    ///  Determines the max length of all rows, by reading the data and resetting the position to the front.
    /// </summary>
    /// <returns>
    ///  The number of records in the file
    /// </returns>
    /// <remarks>
    ///  this is a lengthy process, only use this if you need to know the size in advance, as the
    ///  information is maintained reading the file anyway.
    /// </remarks>
    private long ParseColumnSize()
    {
      var numRows = m_CsvFile.RecordLimit;
      if (numRows <= 0)
        numRows = uint.MaxValue;

      while (numRows > 0 && !EndOfFile && !CancellationToken.IsCancellationRequested)
      {
        HandleShowProgressPeriodic("Determine column length", RecordNumber);

        if (!AllEmptyAndCountConsecutiveEmptyRows(ReadNextRow(true, false)))
        {
          // Regular row with data
          RecordNumber++;
          numRows--;
        }
        else
        {
          // an empty line
          if (!EndOfFile && !m_CsvFile.SkipEmptyLines)
            RecordNumber++;
        }
      }

      return RecordNumber;
    }

    #region Parsing

    /// <summary>
    ///  Resets the position and buffer to the header in case the file has a header
    /// </summary>
    public virtual void ResetPositionToFirstDataRow()
    {
      ResetPositionToStart();
      if (m_CsvFile.HasFieldHeader)
        // Read the header row, this could be more than one line
        ReadNextRow(false, false);
    }

    /// <summary>
    ///  Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and MaxValue</returns>
    protected override int GetRelativePosition()
    {
      // if we know how many records to read, use that
      if (m_MaxRecordNumber > 0)
        return (int)((double)RecordNumber / m_MaxRecordNumber * cMaxValue);

      return (int)(m_ImprovedStream.Percentage * cMaxValue);
    }

    private bool AllEmptyAndCountConsecutiveEmptyRows(string[] columns)
    {
      if (columns != null)
      {
        var rowLength = columns.Length;
        for (var col = 0; col < rowLength && col < FieldCount; col++)
        {
          if (!string.IsNullOrEmpty(columns[col]))
          {
            m_ConsecutiveEmptyRows = 0;
            return false;
          }
        }
      }
      m_ConsecutiveEmptyRows++;
      EndOfFile |= m_ConsecutiveEmptyRows >= m_CsvFile.ConsecutiveEmptyRows;
      return true;
    }

    private char EatNextCRLF(char character)
    {
      EndLineNumber++;
      if (!EndOfFile)
      {
        var nextChar = NextChar();
        if (character == c_Cr && nextChar == c_Lf || character == c_Lf && nextChar == c_Cr)
        {
          // New line sequence is either CRLF or LFCR, disregard the character
          m_BufferPos++;
          // Very special a LF CR is counted as two lines.
          if (character == c_Lf && nextChar == c_Cr)
            EndLineNumber++;
          return nextChar;
        }
      }
      return '\0';
    }

    /// <summary>
    ///  Gets a row of the CSV file
    /// </summary>
    /// <returns>
    ///  <c>NULL</c> if the row can not be read, array of string values representing the columns of
    ///  the row
    /// </returns>
    private bool GetNextRecord()
    {
      Restart:
      CurrentRowColumnText = ReadNextRow(true, true);

      if (!AllEmptyAndCountConsecutiveEmptyRows(CurrentRowColumnText))
      {
        // Regular row with data
        RecordNumber++;
      }
      else
      {
        if (EndOfFile)
          return false;

        // an empty line
        if (m_CsvFile.SkipEmptyLines)
        {
          goto Restart;
        }
        else
        {
          RecordNumber++;
        }
      }
      bool hasWarningCombinedWrning = false;
      Restart2:
      var rowLength = CurrentRowColumnText.Length;
      if (rowLength == FieldCount)
      {
        // Check if we have row that matches the header row
        if (m_HeaderRow != null && m_CsvFile.HasFieldHeader && !hasWarningCombinedWrning)
        {
          bool isRepeatedHeader = true;
          for (int col = 0; col < FieldCount; col++)
          {
            if (!m_HeaderRow[col].Equals(CurrentRowColumnText[col], StringComparison.OrdinalIgnoreCase))
            {
              isRepeatedHeader = false;
              break;
            }
          }
          if (isRepeatedHeader && m_CsvFile.SkipDuplicateHeader)
          {
            RecordNumber--;
            goto Restart;
          }
          if (m_RealignColumns != null && !isRepeatedHeader)
            m_RealignColumns.AddRow(CurrentRowColumnText);
        }
      }
      // If less columns are present...
      else if (rowLength < FieldCount)
      {
        // if we still have only one column and we should have a number of columns assume this was nonsense like a report footer
        if (rowLength == 1 && EndOfFile && CurrentRowColumnText[0].Length < 10)
        {
          RecordNumber--;
          HandleWarning(-1, $"Last line {StartLineNumber}{cLessColumns}. Assumed to be a EOF marker and ignored.");
          return false;
        }
        if (!m_CsvFile.AllowRowCombining)
        {
          HandleWarning(-1, $"Line {StartLineNumber}{cLessColumns} ({rowLength}/{FieldCount}).");
        }
        else
        {
          var oldPos = m_BufferPos;
          var startLine = StartLineNumber;
          // get the next row
          var nextLine = ReadNextRow(true, true);
          StartLineNumber = startLine;

          // allow up to two extra columns they can be combined later
          if (nextLine != null && nextLine.Length > 0 && nextLine.Length + rowLength < FieldCount + 4)
          {
            var combined = new List<string>(CurrentRowColumnText);

            // the first column belongs to the last column of the previous
            // ignore NumWarningsLinefeed otherwise as this is important information
            m_NumWarningsLinefeed++;
            HandleWarning(rowLength - 1,
             $"Added first column from line {EndLineNumber}, assuming a linefeed has split the rows into an additional line.");
            combined[rowLength - 1] += ' ' + nextLine[0];

            for (int col = 1; col < nextLine.Length; col++)
              combined.Add(nextLine[col]);

            if (!hasWarningCombinedWrning)
            {
              HandleWarning(-1, $"Line {StartLineNumber}-{EndLineNumber - 1}{cLessColumns}. Lines have been combined.");
              hasWarningCombinedWrning = true;
            }

            CurrentRowColumnText = combined.ToArray();
            goto Restart2;
          }
          else
          {
            if (m_BufferPos < oldPos)
              // we have an issue we went into the next Buffer there is no way back.
              HandleError(-1, $"Line {StartLineNumber}{cLessColumns}\nAttempting to combined lines some line have been read that is now lost, please turn off Row Combination");
            else
            {
              // return to the old position so reading the next row did not matter
              if (!hasWarningCombinedWrning)
                HandleWarning(-1, $"Line {StartLineNumber}{cLessColumns} ({rowLength}/{FieldCount}).");
              m_BufferPos = oldPos;
            }
          }
        }
      }

      // If more columns are present...
      if (rowLength > FieldCount && (m_CsvFile.WarnEmptyTailingColumns || m_RealignColumns != null))
      {
        // check if the additional columns have contents
        var hasContens = false;
        for (var extraCol = FieldCount + 1; extraCol < rowLength; extraCol++)
        {
          if (string.IsNullOrEmpty(CurrentRowColumnText[extraCol])) continue;
          hasContens = true;
          break;
        }

        if (hasContens)
        {
          if (m_RealignColumns != null)
          {
            HandleWarning(-1,
            $"Line {StartLineNumber}{cMoreColumns}. Trying to realign columns.");
            // determine which column could have caused the issue
            // it could be any column, try to establish
            CurrentRowColumnText = m_RealignColumns.RealignColumn(CurrentRowColumnText, HandleWarning);
          }
          else
          {
            HandleWarning(-1,
            $"Line {StartLineNumber}{cMoreColumns} ({rowLength}/{FieldCount}). The data in extra columns is not read.");
          }
        }
      }
      return true;
    }

    /// <summary>
    ///  Indicates whether the specified Unicode character is categorized as white space.
    /// </summary>
    /// <param name="c">A Unicode character.</param>
    /// <returns><c>true</c> if the character is a whitespace</returns>
    private bool IsWhiteSpace(char c)
    {
      // Handle cases where the delimiter is a whitespace (e.g. tab)
      if (c == m_CsvFile.FileFormat.FieldDelimiterChar)
        return false;

      // See char.IsLatin1(char c) in Reflector
      if (c <= '\x00ff')
        return c == ' ' || c == '\t';

      return CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator;
    }

    /// <summary>
    ///  Gets the next char from the buffer, but stay at the current position
    /// </summary>
    /// <returns>The next char</returns>
    private char NextChar()
    {
      Contract.Requires(m_Buffer != null);
      if (m_BufferPos >= m_BufferFilled)
      {
        ReadIntoBuffer();
        m_BufferPos = 0;
      }

      // If of file its does not matter what to return simply return something
      if (EndOfFile)
        return c_Lf;

      return m_Buffer[m_BufferPos];
    }

    /// <summary>
    ///  Gets the number of fields.
    /// </summary>
    private int ParseFieldCount(IList<string> headerRow, out bool readFurther)
    {
      Contract.Ensures(Contract.Result<int>() >= 0);
      readFurther = false;
      if (headerRow.IsEmpty() || string.IsNullOrEmpty(headerRow[0]))
        return 0;

      // The last column is empty but we expect a header column, assume if a trailing separator
      if (headerRow.Count <= 1 || !string.IsNullOrEmpty(headerRow[headerRow.Count - 1]))
        return headerRow.Count;

      readFurther = true;
      // check if the next lines do have data in the last column
      for (int additional = 0; !EndOfFile && additional < 10; additional++)
      {
        var nextLine = ReadNextRow(false, false);
        // if we have less columns than in the header exit the loop
        if (nextLine.GetLength(0) < headerRow.Count)
          break;
        // if we have data in the column assume the header was missing
        if (!string.IsNullOrEmpty(nextLine[headerRow.Count - 1]))
          return headerRow.Count;
      }

      HandleWarning(headerRow.Count,
       "The last column does not have a column name, this column will be ignored.".AddWarningId());
      return headerRow.Count - 1;
    }

    /// <summary>
    ///  Fills the buffer with data from the reader.
    /// </summary>
    /// <returns><c>true</c> if data was successfully read; otherwise, <c>false</c>.</returns>
    private void ReadIntoBuffer()
    {
      Contract.Requires(m_TextReader != null);
      Contract.Requires(m_Buffer != null);

      if (EndOfFile)
        return;
      m_BufferFilled = m_TextReader.Read(m_Buffer, 0, c_Buffersize);
      EndOfFile |= m_BufferFilled == 0;
      // Handle double decoding
      if (!m_CsvFile.DoubleDecode || m_BufferFilled <= 0) return;
      var result = Encoding.UTF8.GetChars(Encoding.GetEncoding(28591).GetBytes(m_Buffer, 0, m_BufferFilled));
      // The twice decode text is smaller
      m_BufferFilled = result.GetLength(0);
      result.CopyTo(m_Buffer, 0);
    }

    /// <summary>
    ///  Gets the next column in the buffer.
    /// </summary>
    /// <param name="columnNo">The column number for warnings</param>
    /// <param name="storeWarnings">If <c>true</c> warnings will be added</param>
    /// <returns>The next column null after the last column</returns>
    /// <remarks>If NULL is returned we are at the end of the file, an empty column is read as empty string</remarks>
    private string ReadNextColumn(int columnNo, bool storeWarnings)
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
      var predata = true;
      var postdata = false;

      while (!EndOfFile)
      {
        // Increase position
        var character = NextChar();
        m_BufferPos++;

        // Handle escaped characters
        if (character == m_CsvFile.FileFormat.EscapeCharacterChar && !postdata)
        {
          var nextChar = NextChar();
          if (!EndOfFile)
          {
            m_BufferPos++;
            stringBuilder.Append(nextChar);
            if (nextChar == c_Cr || nextChar == c_Lf)
            {
              var nextNext = EatNextCRLF(nextChar);
              if (nextNext == c_Cr || nextNext == c_Lf)
              {
                m_EndOfLine = true;
                break;
              }
            }
            else
              predata = false;
            continue;
          }
        }
        // in case we have a single LF
        if (!postdata && m_CsvFile.TreatLFAsSpace && character == c_Lf)
        {
          bool singleLF = true;
          if (!EndOfFile)
          {
            var nextChar = NextChar();
            if (nextChar == c_Cr)
              singleLF = false;
          }
          if (singleLF)
          {
            character = ' ';
            EndLineNumber++;
            if (m_CsvFile.WarnLineFeed)
              WarnLinefeed(columnNo);
          }
        }

        switch (character)
        {
          case c_Nbsp:
            if (!postdata)
            {
              hadNbsp = true;
              if (m_CsvFile.TreatNBSPAsSpace)
                character = ' ';
            }

            break;

          case c_UnknownChar:
            if (!postdata)
            {
              hadUnknownChar = true;
              if (m_CsvFile.TreatUnknowCharaterAsSpace)
                character = ' ';
            }

            break;

          case c_Cr:
          case c_Lf:
            var nextChar = EatNextCRLF(character);
            if (character == c_Cr && nextChar == c_Lf || character == c_Lf && nextChar == c_Cr)
            {
              if (quoted && !postdata)
              {
                stringBuilder.Append(character);
                stringBuilder.Append(nextChar);
                continue;
              }
            }

            break;
        }

        // Finished with reading the column by Delimiter or EOF
        if (character == m_CsvFile.FileFormat.FieldDelimiterChar && (postdata || !quoted) || EndOfFile)
          break;
        // Finished with reading the column by Linefeed
        if ((character == c_Cr || character == c_Lf) && (predata || postdata || !quoted))
        {
          m_EndOfLine = true;
          break;
        }

        // Only check the characters if not past end of data
        if (postdata)
          continue;

        if (predata)
        {
          // whitespace preceding data
          if (IsWhiteSpace(character))
          {
            // Store the white spaces if we do any kind of trimming
            if (m_CsvFile.TrimmingOption == TrimmingOption.None)
              // Values will be trimmed later but we need to find out, if the filed is quoted first
              stringBuilder.Append(character);
            continue;
          }

          // data is starting
          predata = false;
          // Can not be escaped here
          if (m_HasQualifier && character == m_CsvFile.FileFormat.FieldQualifierChar)
          {
            if (m_CsvFile.TrimmingOption != TrimmingOption.None)
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

        if (m_HasQualifier && character == m_CsvFile.FileFormat.FieldQualifierChar && quoted)
        {
          var peekNextChar = NextChar();
          if (m_CsvFile.AlternateQuoting)
          {
            if (peekNextChar == m_CsvFile.FileFormat.FieldDelimiterChar || peekNextChar == c_Cr || peekNextChar == c_Lf)
            {
              postdata = true;
              continue;
            }
          }
          else
          {
            if (peekNextChar == m_CsvFile.FileFormat.FieldQualifierChar)
            {
              // double quotes within quoted string means add a quote
              stringBuilder.Append(m_CsvFile.FileFormat.FieldQualifierChar);
              m_BufferPos++;
            }
            else
            // end-quote reached any additional characters are disregarded
            {
              postdata = true;
            }

            continue;
          }
        }

        hadDelimiterInValue |= character == m_CsvFile.FileFormat.FieldDelimiterChar;
        // all cases covered, character must be data
        stringBuilder.Append(character);
      }

      var returnValue = HandleTrimAndNBSP(stringBuilder.ToString(), quoted);

      if (!storeWarnings) return returnValue;
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
            if (returnValue[pos] != '?') continue;
            numberQuestionMark++;
            // If we have at least two and there are two consecutive or more than 3+ in 12 characters, or 4+ in 16 characters
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
    ///  Reads the row of the CSV file
    /// </summary>
    /// <param name="regularDataRow">
    ///  Set to <c>true</c> if its not the header row and the maximum size should be determined.
    /// </param>
    /// <param name="storeWarnings">Set to <c>true</c> if the warnings should be issued.</param>
    /// <returns>
    ///  <c>NULL</c> if the row can not be read, array of string values representing the columns of
    ///  the row
    /// </returns>
    private string[] ReadNextRow(bool regularDataRow, bool storeWarnings)
    {
      Restart:
      // Store the starting Line Number
      StartLineNumber = EndLineNumber;

      // If already at end of file, return null
      if (EndOfFile || CancellationToken.IsCancellationRequested || m_TextReader == null)
        return null;

      var item = ReadNextColumn(0, storeWarnings);
      // An empty line does not have any data
      if (string.IsNullOrEmpty(item) && m_EndOfLine)
      {
        m_EndOfLine = false;
        if (m_CsvFile.SkipEmptyLines || !regularDataRow)
          // go to the next line
          goto Restart;

        // Return it as array of empty columns
        return new string[FieldCount];
      }

      // Skip commented lines
      if (m_CsvFile.FileFormat.CommentLine.Length > 0 &&
        !string.IsNullOrEmpty(item) &&
        item.StartsWith(m_CsvFile.FileFormat.CommentLine, StringComparison.Ordinal)
      ) // A commented line does start with the comment
      {
        if (m_EndOfLine)
          m_EndOfLine = false;
        else
          // it might happen that the comment line contains a Delimiter
          ReadToEOL();
        goto Restart;
      }

      var col = 0;
      var columns = new List<string>(FieldCount);

      while (item != null)
      {
        // If a column is quoted and does contain the delimiter and linefeed, issue a warning, we
        // might have an opening delimiter with a missing closing delimiter
        if (storeWarnings &&
          EndLineNumber > StartLineNumber + 4 &&
          item.Length > 1024 &&
          item.IndexOf(m_CsvFile.FileFormat.FieldDelimiterChar) != -1)
          HandleWarning(col,
           $"Column has {EndLineNumber - StartLineNumber + 1} lines and has a length of {item.Length} characters".AddWarningId());

        if (item.Length == 0)
        {
          item = null;
        }
        else
        {
          if (StringUtils.ShouldBeTreatedAsNull(item, m_CsvFile.TreatTextAsNull))
          {
            item = null;
          }
          else
          {
            item = item.ReplaceCaseInsensitive(m_CsvFile.FileFormat.NewLinePlaceholder, Environment.NewLine)
             .ReplaceCaseInsensitive(m_CsvFile.FileFormat.DelimiterPlaceholder,
              m_CsvFile.FileFormat.FieldDelimiterChar)
             .ReplaceCaseInsensitive(m_CsvFile.FileFormat.QuotePlaceholder, m_CsvFile.FileFormat.FieldQualifierChar);

            if (regularDataRow && col < FieldCount)
            {
              var column = GetColumn(col);
              switch (column.DataType)
              {
                case DataType.TextToHtml:
                  var newitemE = HTMLStyle.TextToHtmlEncode(item);
                  if (!item.Equals(newitemE, StringComparison.Ordinal))
                    HandleWarning(col, $"HTML encoding removed from {item}");
                  item = newitemE;
                  break;

                case DataType.TextToHtmlFull:
                  var newitemS = HTMLStyle.HtmlEncodeShort(item);
                  if (!item.Equals(newitemS, StringComparison.Ordinal))
                    HandleWarning(col, $"HTML encoding removed from {item}");
                  item = newitemS;
                  break;

                case DataType.TextPart:
                  var part = GetPart(item, column);
                  if (part == null && item.Length > 0)
                    HandleWarning(col,
                     $"Part {column.Part} of text {item} is empty.");
                  item = part;
                  break;
              }

              if (!string.IsNullOrEmpty(item) && column.Size < item.Length)
                column.Size = item.Length;
            }
          }
        }

        columns.Add(item);

        col++;
        item = ReadNextColumn(col, storeWarnings);
      }

      return columns.ToArray();
    }

    /// <summary>
    ///  Reads from the buffer until the line has ended
    /// </summary>
    private void ReadToEOL()
    {
      while (!EndOfFile)
      {
        var character = NextChar();
        m_BufferPos++;
        if (character != c_Cr && character != c_Lf) continue;
        EatNextCRLF(character);
        return;
      }
    }

    /// <summary>
    ///  Resets the position and buffer to the first line, excluding headers, use ResetPositionToStart if you want to go to
    ///  first data line
    /// </summary>
    private void ResetPositionToStart()
    {
      if (m_ImprovedStream == null)
        m_ImprovedStream = ImprovedStream.OpenRead(m_CsvFile);
      m_ImprovedStream.ResetToStart(delegate (Stream str)
      {
        // in case we can not seek need to reopen the stream reader
        if (!str.CanSeek || m_TextReader == null)
        {
          if (m_TextReader != null)
            m_TextReader.Dispose();
          m_TextReader = new StreamReader(str, m_CsvFile.GetEncoding(), m_CsvFile.ByteOrderMark);
        }
        else
          // only need to discard the buffer
          m_TextReader.DiscardBufferedData();
      });

      m_CsvFile.CurrentEncoding = m_TextReader.CurrentEncoding;
      m_BufferPos = 0;
      m_BufferFilled = 0;
      // End Line should be at 1, later on as the line is read the start line s set to this value
      EndLineNumber = 1;
      RecordNumber = 0;
      m_EndOfLine = false;
      EndOfFile = false;

      // Skip the given number of lines
      // <= so we do skip the right number
      while (EndLineNumber <= m_CsvFile.SkipRows && !EndOfFile && !CancellationToken.IsCancellationRequested)
        ReadToEOL();
    }

    #endregion Parsing

    #region Warning

    /// <summary>
    ///  Add warnings for delimiter.
    /// </summary>
    /// <param name="column">The column.</param>
    private void WarnDelimiter(int column)
    {
      m_NumWarningsDelimiter++;
      if (m_CsvFile.NumWarnings < 1 || m_NumWarningsDelimiter <= m_CsvFile.NumWarnings)
        HandleWarning(column,
         $"Field delimiter '{FileFormat.GetDescription(m_CsvFile.FileFormat.FieldDelimiter)}' found in field".AddWarningId());
    }

    /// <summary>
    ///  Add warnings for Linefeed.
    /// </summary>
    /// <param name="column">The column.</param>
    private void WarnLinefeed(int column)
    {
      m_NumWarningsLinefeed++;
      if (m_CsvFile.NumWarnings < 1 || m_NumWarningsLinefeed <= m_CsvFile.NumWarnings)
        HandleWarning(column, "Linefeed found in field".AddWarningId());
    }

    /// <summary>
    ///  Add warnings for NBSP.
    /// </summary>
    /// <param name="column">The column.</param>
    private void WarnNbsp(int column)
    {
      m_NumWarningsNbspChar++;
      if (m_CsvFile.NumWarnings >= 1 && m_NumWarningsNbspChar > m_CsvFile.NumWarnings) return;
      HandleWarning(column,
       m_CsvFile.TreatNBSPAsSpace
        ? "Character Non Breaking Space found, this character was treated as space".AddWarningId()
        : "Character Non Breaking Space found in field".AddWarningId());
    }

    /// <summary>
    ///  Add warnings for quotes.
    /// </summary>
    /// <param name="column">The column.</param>
    private void WarnQuotes(int column)
    {
      m_NumWarningsQuote++;
      if (m_CsvFile.NumWarnings < 1 || m_NumWarningsQuote <= m_CsvFile.NumWarnings)
        HandleWarning(column,
         $"Field qualifier '{FileFormat.GetDescription(m_CsvFile.FileFormat.FieldQualifier)}' found in field".AddWarningId());
    }

    /// <summary>
    ///  Add warnings for unknown char.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="questionMark"></param>
    private void WarnUnknownChar(int column, bool questionMark)
    {
      m_NumWarningsUnknownChar++;
      if (m_CsvFile.NumWarnings >= 1 && m_NumWarningsUnknownChar > m_CsvFile.NumWarnings) return;
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

    #endregion Warning
  }
}