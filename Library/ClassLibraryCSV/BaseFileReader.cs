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
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Pri.LongPath;

namespace CsvTools
{
  /// <summary>
  ///   Abstract class as base for all DataReaders
  /// </summary>
  public abstract class BaseFileReader : IDisposable
  {
    /// <summary>
    ///   Field name of the LineNumber Field
    /// </summary>
    public const string cEndLineNumberFieldName = "#LineEnd";

    /// <summary>
    ///   Field name of the Error Field
    /// </summary>
    public const string cErrorField = "#Error";

    /// <summary>
    ///   Field name of the LineNumber Start Field
    /// </summary>
    public const string cPartitionField = "#Partition";

    /// <summary>
    ///   Field Name of the record number
    /// </summary>
    public const string cRecordNumberFieldName = "#Record";

    /// <summary>
    ///   Field name of the LineNumber Start Field
    /// </summary>
    public const string cStartLineNumberFieldName = "#Line";

    /// <summary>
    ///   The maximum value
    /// </summary>
    protected const int cMaxValue = 10000;

    /// <summary>
    ///   Collection of the artificial field names
    /// </summary>
    public static ICollection<string> ArtificalFields = new HashSet<string>
      {cRecordNumberFieldName, cStartLineNumberFieldName, cEndLineNumberFieldName, cErrorField, cPartitionField};

    private readonly IFileSetting m_FileSetting;
    private readonly IntervalAction m_IntervalAction = new IntervalAction();

    /// <summary>
    ///   An array of associated col
    /// </summary>
    protected int[] AssociatedTimeCol;

    /// <summary>
    ///   An array of associated col
    /// </summary>
    protected int[] AssociatedTimeZoneCol;

    /// <summary>
    ///   An array of column
    /// </summary>
    protected Column[] Column;

    /// <summary>
    ///   An array of current row column text
    /// </summary>
    protected string[] CurrentRowColumnText;

    /// <summary>
    ///   Used to avoid duplicate disposal
    /// </summary>
    private bool m_DisposedValue;

    /// <summary>
    ///   Number of Columns in the reader
    /// </summary>
    private int m_FieldCount;

    /// <summary>
    ///   used to avoid reporting a fished execution twice it might be called on error before being called once execution is
    ///   done
    /// </summary>
    private bool m_IsFinished;

    protected BaseFileReader(IFileSetting fileSetting, IProcessDisplay processDisplay)
    {
      if (processDisplay != null)
      {
        processDisplay.Maximum = cMaxValue;
        CancellationToken = processDisplay.CancellationToken;
      }
      else
      {
        CancellationToken = CancellationToken.None;
      }

      ProcessDisplay = processDisplay;

      m_FileSetting = fileSetting ?? throw new ArgumentNullException(nameof(fileSetting));
      Logger.Debug("Created Reader for {filesetting}", fileSetting);
    }

    /// <summary>
    ///   A cancellation token, to stop long running processes
    /// </summary>
    protected CancellationToken CancellationToken { get; }

    /// <summary>
    ///   Gets a value indicating the depth of nesting for the current row.
    /// </summary>
    /// <value></value>
    /// <returns>The level of nesting.</returns>
    public virtual int Depth => 0;

    /// <summary>
    ///   Current Line Number in the text file, a record can span multiple lines and lines are
    ///   skipped, this is he ending line
    /// </summary>
    public virtual long EndLineNumber { get; protected set; }

    /// <summary>
    ///   Gets or sets a value indicating whether the reader is at the end of the file.
    /// </summary>
    /// <value><c>true</c> if at the end of file; otherwise, <c>false</c>.</value>
    public virtual bool EndOfFile { get; protected set; } = true;

    /// <summary>
    ///   Gets the number of fields in the file.
    /// </summary>
    /// <value>Number of field in the file.</value>
    public virtual int FieldCount => m_FieldCount;

    public double NotifyAfterSeconds
    {
      set => m_IntervalAction.NotifyAfterSeconds = value;
    }

    /// <summary>
    ///   Gets the record number.
    /// </summary>
    /// <value>The record number.</value>
    public virtual long RecordNumber { get; protected set; }

    /// <summary>
    ///   Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
    /// </summary>
    /// <value></value>
    /// <returns>
    ///   The number of rows changed, inserted, or deleted; 0 if no rows were affected or the
    ///   statement failed; and -1 for SELECT statements.
    /// </returns>
    public virtual int RecordsAffected => -1;

    /// <summary>
    ///   Current Line Number in the text file where the record has started
    /// </summary>
    public virtual long StartLineNumber { get; protected set; }


    /// <summary>
    ///   A process display to stop long running processes
    /// </summary>
    protected IProcessDisplay ProcessDisplay { get; }

    /// <summary>
    ///   Gets the <see cref="object" /> with the specified name.
    /// </summary>
    /// <value></value>
    public object this[string columnName] => GetValue(GetOrdinal(columnName));

    /// <summary>
    ///   Gets the <see cref="object" /> with the specified column.
    /// </summary>
    /// <value></value>
    public object this[int columnNumber] => GetValue(columnNumber);

    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting
    ///   unmanaged resources.
    /// </summary>
    public virtual void Dispose()
    {
      Dispose(true);
    }

    /// <summary>
    ///   Event to be raised if reading the files is completed
    /// </summary>
    public virtual event EventHandler ReadFinished;

    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    public virtual event EventHandler<WarningEventArgs> Warning;

    /// <summary>
    ///   Closes the <see cref="IDataReader" /> Object.
    /// </summary>
    public virtual void Close()
    {
      EndOfFile = true;
    }

    public virtual void Dispose(bool disposing)
    {
      if (!m_DisposedValue)
      {
        Close();
        m_DisposedValue = true;
      }
    }

    // To detect redundant calls
    /// <summary>
    ///   Gets the boolean.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    public virtual bool GetBoolean(int i)
    {
      Debug.Assert(i >= 0);
      Debug.Assert(CurrentRowColumnText != null);

      var parsed = GetBooleanNull(CurrentRowColumnText[i], i);
      if (parsed.HasValue)
        return parsed.Value;
      // Warning was added by GetBooleanNull
      throw WarnAddFormatException(i,
        $"'{CurrentRowColumnText[i]}' is not a boolean ({GetColumn(i).True}/{GetColumn(i).False})");
    }

    /// <summary>
    ///   Gets the 8-bit unsigned integer value of the specified column.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <returns>The 8-bit unsigned integer value of the specified column.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public virtual byte GetByte(int i)
    {
      Debug.Assert(i >= 0);
      Debug.Assert(i < FieldCount);
      Debug.Assert(CurrentRowColumnText != null);
      try
      {
        return byte.Parse(CurrentRowColumnText[i], CultureInfo.InvariantCulture);
      }
      catch (Exception)
      {
        throw WarnAddFormatException(i,
          $"'{CurrentRowColumnText[i]}' is not byte");
      }
    }

    /// <summary>
    ///   Gets the character value of the specified column.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <returns>The character value of the specified column.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public virtual char GetChar(int i)
    {
      Debug.Assert(i >= 0);
      Debug.Assert(CurrentRowColumnText != null);
      return CurrentRowColumnText[i][0];
    }

    /// <summary>
    ///   Reads a stream of characters from the specified column offset into the buffer as an array,
    ///   starting at the given buffer offset.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <param name="fieldoffset">The index within the row from which to start the read operation.</param>
    /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
    /// <param name="bufferoffset">The index for <paramref name="buffer" /> to start the read operation.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>The actual number of characters read.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public virtual long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
    {
      Debug.Assert(CurrentRowColumnText != null);
      var offset = (int) fieldoffset;
      var maxLen = CurrentRowColumnText[i].Length - offset;
      if (maxLen > length)
        maxLen = length;
      CurrentRowColumnText[i].CopyTo(offset, buffer ?? throw new ArgumentNullException(nameof(buffer)), bufferoffset,
        maxLen);
      return maxLen;
    }

    /// <summary>
    ///   Gets the column format.
    /// </summary>
    /// <param name="columnNumber">The column.</param>
    /// <returns></returns>
    public virtual Column GetColumn(int columnNumber)
    {
      if (Column == null)
        throw new InvalidOperationException("File has not been read");
      if (columnNumber < 0 || columnNumber >= FieldCount || columnNumber >= Column.Length)
        throw new IndexOutOfRangeException(nameof(columnNumber));

      return Column[columnNumber];
    }

    /// <summary>
    ///   Gets the date and time data value of the specified field.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>
    ///   The date and time data value of the specified field.
    /// </returns>
    public virtual DateTime GetDateTime(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentRowColumnText != null && columnNumber < CurrentRowColumnText.Length);

      var dt = GetDateTimeNull(null, CurrentRowColumnText[columnNumber], null, GetTimeValue(columnNumber),
        GetColumn(columnNumber), true);
      if (dt.HasValue)
        return dt.Value;

      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(columnNumber,
        $"'{CurrentRowColumnText[columnNumber]}' is not a date of the format {GetColumn(columnNumber).DateFormat}");
    }

    /// <summary>
    ///   Gets the decimal.
    /// </summary>
    /// <param name="columnNumber">The i.</param>
    /// <returns></returns>
    public virtual decimal GetDecimal(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentRowColumnText != null && columnNumber < CurrentRowColumnText.Length);

      var decimalValue = GetDecimalNull(CurrentRowColumnText[columnNumber], columnNumber);
      if (decimalValue.HasValue)
        return decimalValue.Value;
      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(columnNumber,
        $"'{CurrentRowColumnText[columnNumber]}' is not a decimal");
    }

    /// <summary>
    ///   Gets the double.
    /// </summary>
    /// <param name="columnNumber">The i.</param>
    /// <returns></returns>
    public virtual double GetDouble(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentRowColumnText != null && columnNumber < CurrentRowColumnText.Length);

      var decimalValue = GetDecimalNull(CurrentRowColumnText[columnNumber], columnNumber);
      if (decimalValue.HasValue)
        return Convert.ToDouble(decimalValue.Value);

      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(columnNumber,
        $"'{CurrentRowColumnText[columnNumber]}' is not a double");
    }

    /// <summary>
    ///   Gets the type of the field.
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <returns>The .NET type of the column</returns>
    public virtual Type GetFieldType(int columnNumber) => GetColumn(columnNumber).DataType.GetNetType();

    /// <summary>
    ///   Gets the single-precision floating point number of the specified field.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>
    ///   The single-precision floating point number of the specified field.
    /// </returns>
    public virtual float GetFloat(int columnNumber)
    {
      Debug.Assert(0 <= columnNumber);

      var decimalValue = GetDecimalNull(CurrentRowColumnText[columnNumber], columnNumber);
      if (decimalValue.HasValue)
        return Convert.ToSingle(decimalValue, CultureInfo.InvariantCulture);
      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(columnNumber,
        $"'{CurrentRowColumnText[columnNumber]}' is not a float");
    }

    /// <summary>
    ///   Gets the unique identifier.
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <returns></returns>
    public virtual Guid GetGuid(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentRowColumnText != null && columnNumber < CurrentRowColumnText.Length);

      var parsed = GetGuidNull(CurrentRowColumnText[columnNumber], columnNumber);

      if (parsed.HasValue)
        return parsed.Value;

      throw WarnAddFormatException(columnNumber,
        $"'{CurrentRowColumnText[columnNumber]}' is not an GUID");
    }

    /// <summary>
    ///   Gets the int32 value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="columnNumber">The column number.</param>
    /// <returns></returns>
    public virtual Guid? GetGuidNull(string inputValue, int columnNumber)
    {
      if (string.IsNullOrEmpty(inputValue))
        return null;
      try
      {
        return new Guid(inputValue);
      }
      catch
      {
        throw WarnAddFormatException(columnNumber,
          $"'{inputValue}' is not a Guid");
      }
    }

    /// <summary>
    ///   Gets the 16-bit signed integer value of the specified field.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>
    ///   The 16-bit signed integer value of the specified field.
    /// </returns>
    public virtual short GetInt16(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentRowColumnText != null && columnNumber < CurrentRowColumnText.Length);

      return GetInt16(CurrentRowColumnText[columnNumber], columnNumber);
    }

    /// <summary>
    ///   Gets the int32.
    /// </summary>
    /// <param name="columnNumber">The i.</param>
    /// <returns></returns>
    public virtual int GetInt32(int columnNumber)
    {
      Debug.Assert(CurrentRowColumnText != null && columnNumber < CurrentRowColumnText.Length);
      var column = GetColumn(columnNumber);

      var parsed = StringConversion.StringToInt32(CurrentRowColumnText[columnNumber], column.DecimalSeparatorChar,
        column.GroupSeparatorChar);
      if (parsed.HasValue)
        return parsed.Value;

      // Warning was added by GetInt32Null
      throw WarnAddFormatException(columnNumber,
        $"'{CurrentRowColumnText[columnNumber]}' is not an integer");
    }

    /// <summary>
    ///   Gets the int32 value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="column">The column.</param>
    /// <returns></returns>
    public virtual int? GetInt32Null(string inputValue, Column column)
    {
      Debug.Assert(column != null);
      var ret = StringConversion.StringToInt32(inputValue, column.DecimalSeparatorChar, column.GroupSeparatorChar);
      if (ret.HasValue)
        return ret.Value;

      HandleError(column.ColumnOrdinal, $"'{inputValue}' is not an integer");
      return null;
    }

    /// <summary>
    ///   Gets the int64.
    /// </summary>
    /// <param name="columnNumber">The i.</param>
    /// <returns></returns>
    public virtual long GetInt64(int columnNumber)
    {
      var column = GetColumn(columnNumber);

      var parsed = StringConversion.StringToInt64(CurrentRowColumnText[columnNumber], column.DecimalSeparatorChar,
        column.GroupSeparatorChar);
      if (parsed.HasValue)
        return parsed.Value;

      throw WarnAddFormatException(columnNumber,
        $"'{CurrentRowColumnText[columnNumber]}' is not an long integer");
    }

    /// <summary>
    ///   Gets the int32 value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="column">The column.</param>
    /// <returns></returns>
    public virtual long? GetInt64Null(string inputValue, Column column)
    {
      Debug.Assert(column != null);
      var ret = StringConversion.StringToInt64(inputValue, column.DecimalSeparatorChar, column.GroupSeparatorChar);
      if (ret.HasValue)
        return ret.Value;

      HandleError(column.ColumnOrdinal, $"'{inputValue}' is not an long integer");
      return null;
    }

    /// <summary>
    ///   Gets the name for the field to find.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>The name of the field or the empty string (""), if there is no value to return.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public virtual string GetName(int columnNumber) => GetColumn(columnNumber).Name;

    /// <summary>
    ///   Return the index of the named field.
    /// </summary>
    /// <param name="columnName">The name of the field to find.</param>
    /// <returns>The index of the named field. If not found -1</returns>
    public virtual int GetOrdinal(string columnName)
    {
      Debug.Assert(columnName != null);

      if (string.IsNullOrEmpty(columnName) || Column == null)
        return -1;
      var count = 0;
      foreach (var column in Column)
      {
        if (columnName.Equals(column.Name, StringComparison.OrdinalIgnoreCase))
          return count;
        count++;
      }

      return -1;
    }

    /// <summary>
    ///   Returns a <see cref="DataTable" /> that describes the column meta data of the
    ///   <see cref="IDataReader" />.
    /// </summary>
    /// <returns>
    ///   A <see cref="DataTable" /> that describes the column meta data.
    /// </returns>
    /// <exception cref="InvalidOperationException">The <see cref="IDataReader" /> is closed.</exception>
    public virtual DataTable GetSchemaTable()
    {
      var dataTable = GetEmptySchemaTable();
      var schemaRow = GetDefaultSchemaRowArray();

      for (var col = 0; col < FieldCount; col++)
      {
        var column = GetColumn(col);

        schemaRow[1] = column.Name; // Column name
        schemaRow[4] = column.Name; // Column name
        schemaRow[5] = col; // Column ordinal
        schemaRow[6] = column.Size;

        // If there is a conversion get the information
        if (column.Convert && column.DataType != DataType.String)
          schemaRow[7] = column.DataType.GetNetType();
        else
          schemaRow[7] = typeof(string);

        dataTable.Rows.Add(schemaRow);
      }

      return dataTable;
    }

    /// <summary>
    ///   Gets the originally provided text in a column
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Row has not been read</exception>
    /// <exception cref="ArgumentOutOfRangeException">ColumnNumber invalid</exception>
    public virtual string GetString(int columnNumber)
    {
      if (CurrentRowColumnText == null)
        throw new InvalidOperationException("Row has not been read");
      if (columnNumber < 0 || columnNumber >= FieldCount || columnNumber >= CurrentRowColumnText.Length)
        throw new IndexOutOfRangeException(nameof(columnNumber));

      return CurrentRowColumnText[columnNumber];
    }

    /// <summary>
    ///   Gets the value of a column
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <returns>The value of the specific field</returns>
    public virtual object GetValue(int columnNumber)
    {
      if (IsDBNull(columnNumber))
        return DBNull.Value;
      var column = GetColumn(columnNumber);

      object ret;
      try
      {
        switch (column.DataType)
        {
          case DataType.DateTime:
            ret = GetDateTime(columnNumber);
            break;

          case DataType.Integer:
            ret = IntPtr.Size == 4 ? GetInt32(columnNumber) : GetInt64(columnNumber);
            break;

          case DataType.Double:
            ret = GetDouble(columnNumber);
            break;

          case DataType.Numeric:
            ret = GetDecimal(columnNumber);
            break;

          case DataType.Boolean:
            ret = GetBoolean(columnNumber);
            break;

          case DataType.Guid:
            ret = GetGuid(columnNumber);
            break;

          default:
            /* TextToHTML and TextToHTMLFull have been handled in the Reader for the column as the length of the fields would change */
            ret = GetString(columnNumber);
            break;
        }
      }
      catch (FormatException)
      {
        return DBNull.Value;
      }

      if (ret == null)
        return DBNull.Value;
      return ret;
    }

    /// <summary>
    ///   Gets all the attribute fields in the collection for the current record.
    /// </summary>
    /// <param name="values">
    ///   An array of object to copy the attribute fields into.
    /// </param>
    /// <returns>The number of instances of object in the array.</returns>
    public virtual int GetValues(object[] values)
    {
      if (values is null)
        throw new ArgumentNullException(nameof(values));
      Debug.Assert(CurrentRowColumnText != null);
      for (var col = 0; col < FieldCount; col++)
        values[col] = CurrentRowColumnText[col];
      return FieldCount;
    }

    /// <summary>
    ///   Shows the process.
    /// </summary>
    /// <param name="text">Leading Text</param>
    /// <param name="recordNumber">The record number.</param>
    /// <param name="progress">The progress (a value between 0 and MaxValue)</param>
    public virtual void HandleShowProgress(string text, long recordNumber, int progress)
    {
      var rec = recordNumber > 1 ? $"\nRecord {recordNumber:N0}" : string.Empty;
      ProcessDisplay?.SetProcess($"{text}{rec}", progress, false);
    }

    /// <summary>
    ///   Checks if the column should be read
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <returns><c>true</c> if this column should not be read</returns>
    public virtual bool IgnoreRead(int columnNumber) => GetColumn(columnNumber).Ignore;

    /// <summary>
    ///   Displays progress, is called after <see langword="abstract" />row has been read
    /// </summary>
    /// <param name="hasReadRow"><c>true</c> if a row has been read</param>
    public virtual void InfoDisplay(bool hasReadRow)
    {
      if (!hasReadRow)
        HandleReadFinished();
      else
        HandleShowProgressPeriodic("Reading", RecordNumber);
    }

    /// <summary>
    ///   Return whether the specified field is set to null.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>
    ///   true if the specified field is set to null; otherwise, false.
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through
    ///   <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public virtual bool IsDBNull(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      if (CurrentRowColumnText == null || CurrentRowColumnText.Length <= columnNumber)
        return true;
      if (Column[columnNumber].DataType == DataType.DateTime)
      {
        if (AssociatedTimeCol[columnNumber] == -1 || AssociatedTimeCol[columnNumber] >= CurrentRowColumnText.Length)
          return string.IsNullOrEmpty(CurrentRowColumnText[columnNumber]);

        return string.IsNullOrEmpty(CurrentRowColumnText[columnNumber]) &&
               string.IsNullOrEmpty(CurrentRowColumnText[AssociatedTimeCol[columnNumber]]);
      }

      if (string.IsNullOrEmpty(CurrentRowColumnText[columnNumber]))
        return true;

      if (m_FileSetting.TrimmingOption == TrimmingOption.All && Column[columnNumber].DataType >= DataType.String)
        return CurrentRowColumnText[columnNumber].Trim().Length == 0;

      return false;
    }

    /// <summary>
    ///   Advances the data reader to the next result, when reading the results of batch SQL statements.
    /// </summary>
    /// <returns>true if there are more rows; otherwise, false.</returns>
    public virtual bool NextResult() => false;

    /// <summary>
    ///   Overrides the column format from setting.
    /// </summary>
    /// <param name="fieldCount">The field count.</param>
    public virtual void OverrideColumnFormatFromSetting(int fieldCount)
    {
      Debug.Assert(AssociatedTimeCol != null);

      for (var colindex = 0; colindex < FieldCount; colindex++)
      {
        var setting = m_FileSetting.ColumnCollection.Get(Column[colindex].Name);
        if (setting != null)
        {
          setting.CopyTo(Column[colindex]);
          // Copy to has replaced he column Ordinal but this should be kept
          Column[colindex].ColumnOrdinal = colindex;
        }

        if (Column[colindex].DataType != DataType.DateTime || setting == null || !setting.Convert ||
            setting.Ignore)
          continue;

        AssociatedTimeCol[colindex] = GetOrdinal(Column[colindex].TimePart);
        AssociatedTimeZoneCol[colindex] = GetOrdinal(Column[colindex].TimeZonePart);
      }

      // Any defined column not present in file is removed
      // this can happen if the file is changed or changing the encoding the name might show different W�hrung <> Währung
      var remove = new List<Column>();
      foreach (var setting in m_FileSetting.ColumnCollection)
      {
        var found = false;
        for (var colindex = 0; colindex < FieldCount; colindex++)
          if (Column[colindex].Name.Equals(setting.Name, StringComparison.OrdinalIgnoreCase))
          {
            found = true;
            break;
          }

        if (!found)
          remove.Add(setting);
      }

      foreach (var col in remove)
        // HandleWarning(-1, $"Column \"{col.Name}\" not found in file, this can happen if column name is changed");
        m_FileSetting.ColumnCollection.Remove(col);
    }

    /// <summary>
    ///   Advances the DataReader to the next non empty record
    /// </summary>
    /// <returns>true if there are more rows; otherwise, false.</returns>
    public abstract bool Read();

    protected internal static string GetDefaultName(int i) => $"Column{i + 1}";

    /// <summary>
    ///   Gets the default schema row array.
    /// </summary>
    /// <returns>
    ///   an Array of objects for a new row in a Schema Table
    /// </returns>
    protected internal static object[] GetDefaultSchemaRowArray()
    {
      return new object[]
      {
        true, // 00- AllowDBNull
        null, // 01- BaseColumnName
        string.Empty, // 02- BaseSchemaName
        string.Empty, // 03- BaseTableName
        null, // 04- ColumnName
        null, // 05- ColumnOrdinal
        int.MaxValue, // 06- ColumnSize
        typeof(string), // 07- DataType
        false, // 08- IsAliased
        false, // 09- IsExpression
        false, // 10- IsKey
        false, // 11- IsLong
        false, // 12- IsUnique
        DBNull.Value, // 13- NumericPrecision
        DBNull.Value, // 14- NumericScale
        (int) DbType.String, // 15- ProviderType
        string.Empty, // 16- BaseCatalogName
        string.Empty, // 17- BaseServerName
        false, // 18- IsAutoIncrement
        false, // 19- IsHidden
        true, // 20- IsReadOnly
        false // 21- IsRowVersion
      };
    }

    /// <summary>
    ///   Gets the empty schema table.
    /// </summary>
    /// <returns>
    ///   A Data Table with the columns for a Schema Table
    /// </returns>
    protected internal static DataTable GetEmptySchemaTable()
    {
      var dataTable = new DataTable
      {
        TableName = "SchemaTable",
        Locale = CultureInfo.InvariantCulture,
        MinimumCapacity = 10
      };

      dataTable.Columns.Add(SchemaTableColumn.AllowDBNull, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.BaseColumnName, typeof(string)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.BaseSchemaName, typeof(string)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.BaseTableName, typeof(string)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.ColumnName, typeof(string)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.ColumnOrdinal, typeof(int)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.ColumnSize, typeof(int)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.DataType, typeof(object)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.IsAliased, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.IsExpression, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.IsKey, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.IsLong, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.IsUnique, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.NumericPrecision, typeof(short)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.NumericScale, typeof(short)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableColumn.ProviderType, typeof(int)).ReadOnly = true;

      dataTable.Columns.Add(SchemaTableOptionalColumn.BaseCatalogName, typeof(string)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableOptionalColumn.BaseServerName, typeof(string)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableOptionalColumn.IsAutoIncrement, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableOptionalColumn.IsHidden, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableOptionalColumn.IsReadOnly, typeof(bool)).ReadOnly = true;
      dataTable.Columns.Add(SchemaTableOptionalColumn.IsRowVersion, typeof(bool)).ReadOnly = true;

      return dataTable;
    }

    protected virtual void FinishOpen()
    {
      ApplicationSetting.StoreHeader?.Invoke(m_FileSetting, Column);
      m_IsFinished = false;
      if (FieldCount > 0)
        // Override the column settings and store the columns for later reference
        OverrideColumnFormatFromSetting(FieldCount);
    }

    protected DateTime? GetDateTimeNull(object inputDate, string strInputDate, object inputTime,
      string strInputTime, Column column, bool serialDateTime)
    {
      var dateTime = StringConversion.CombineObjectsToDateTime(inputDate, strInputDate, inputTime, strInputTime,
        serialDateTime, column, out var timeSpanLongerThanDay);
      if (timeSpanLongerThanDay)
      {
        var passedIn = strInputTime;
        if (inputTime != null)
          passedIn = inputTime.ToString();
        HandleWarning(column.ColumnOrdinal,
          $"'{passedIn}' is outside expected range 00:00 - 23:59, the date has been adjusted");
      }

      if (!dateTime.HasValue && !string.IsNullOrEmpty(strInputDate) && !string.IsNullOrEmpty(column.DateFormat) &&
          strInputDate.Length > column.DateFormat.Length)
      {
        var inputDateNew = strInputDate.Substring(0, column.DateFormat.Length);
        dateTime = StringConversion.CombineStringsToDateTime(inputDateNew, column.DateFormat, strInputTime,
          column.DateSeparator, column.TimeSeparator, serialDateTime);
        if (dateTime.HasValue)
        {
          var display = column.DateFormat.ReplaceDefaults("/", column.DateSeparator, ":", column.TimeSeparator);
          HandleWarning(column.ColumnOrdinal,
            !string.IsNullOrEmpty(strInputTime)
              ? $"'{strInputDate} {strInputTime}' is not a date of the format {display} {column.TimePartFormat}, used '{inputDateNew} {strInputTime}'"
              : $"'{strInputDate}' is not a date of the format {display}, used '{inputDateNew}' ");
        }
      }

      if (dateTime.HasValue && dateTime.Value.Year > 1752 && dateTime.Value.Year <= 9999)
        return AdjustTz(dateTime, column);

      HandleDateError(strInputDate, strInputTime, column.ColumnOrdinal);
      return null;
    }

    /// <summary>
    ///   Gets the decimal value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="column">The column.</param>
    /// <returns>
    ///   The decimal value if conversion is not successful: <c>NULL</c> the event handler for
    ///   warnings is called
    /// </returns>
    protected decimal? GetDecimalNull(string inputValue, Column column)
    {
      Debug.Assert(column != null);
      var decimalValue =
        StringConversion.StringToDecimal(inputValue, column.DecimalSeparatorChar, column.GroupSeparatorChar, true);
      if (decimalValue.HasValue)
        return decimalValue.Value;

      HandleError(column.ColumnOrdinal, $"'{inputValue}' is not a decimal value");
      return null;
    }

    /// <summary>
    ///   Gets the decimal value or null.
    /// </summary>
    /// <param name="inputValue">The input value.</param>
    /// <param name="columnNumber">The column.</param>
    /// <returns>
    ///   The decimal value if conversion is not successful: <c>NULL</c> the event handler for
    ///   warnings is called
    /// </returns>
    protected decimal? GetDecimalNull(string inputValue, int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      return GetDecimalNull(inputValue, GetColumn(columnNumber));
    }

    /// <summary>
    ///   Gets the double value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="column">The column.</param>
    /// <returns>
    ///   The parsed value if conversion is not successful: <c>NULL</c> is returned and the event
    ///   handler for warnings is called
    /// </returns>
    protected double? GetDoubleNull(string inputValue, Column column)
    {
      Debug.Assert(column != null);
      var decimalValue = GetDecimalNull(inputValue, column);
      if (decimalValue.HasValue)
        return decimal.ToDouble(decimalValue.Value);

      HandleError(column.ColumnOrdinal, $"'{inputValue}' is not a double value");
      return null;
    }

    /// <summary>
    ///   Gets the integer value
    /// </summary>
    /// <param name="value">The input.</param>
    /// <param name="columnNumber">The column number for retrieving the Format information.</param>
    /// <returns>
    ///   The parsed value if conversion is not successful: <c>NULL</c> is returned and the event
    ///   handler for warnings is called
    /// </returns>
    protected short GetInt16(string value, int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      var column = GetColumn(columnNumber);

      var parsed = StringConversion.StringToInt16(value, column.DecimalSeparatorChar, column.GroupSeparatorChar);
      if (parsed.HasValue)
        return parsed.Value;

      // Warning was added by GetInt32Null
      throw WarnAddFormatException(columnNumber,
        $"'{value}' is not a short");
    }

    /// <summary>
    ///   Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and MaxValue</returns>
    protected abstract int GetRelativePosition();

    /// <summary>
    ///   Gets the associated value.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    protected string GetTimeValue(int i)
    {
      Debug.Assert(i >= 0);
      Debug.Assert(i < FieldCount);
      Debug.Assert(CurrentRowColumnText != null);
      Debug.Assert(AssociatedTimeCol != null);

      var colTime = AssociatedTimeCol[i];
      if (colTime == -1 || AssociatedTimeCol[i] >= CurrentRowColumnText.Length)
        return null;
      return CurrentRowColumnText[colTime];
    }

    /// <summary>
    ///   Gets the typed value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="timeValue">The associated value (e.G. Time).</param>
    /// <param name="column">The Column information</param>
    /// <returns>
    ///   The parsed value if conversion is not successful: <c>DBNull.Value</c> is returned and the
    ///   event handler for warnings is called
    /// </returns>
    protected virtual object GetTypedValueFromString(string value, string timeValue, Column column)
    {
      Debug.Assert(column != null);

      object ret;

      // Get Column Format for Column
      switch (column.DataType)
      {
        case DataType.DateTime:
          ret = GetDateTimeNull(null, value, null, timeValue, column, true);
          break;

        case DataType.Integer:
          ret = IntPtr.Size == 4 ? GetInt32Null(value, column) : GetInt64Null(value, column);
          break;

        case DataType.Double:
          ret = GetDoubleNull(value, column);
          break;

        case DataType.Numeric:
          ret = GetDecimalNull(value, column);
          break;

        case DataType.Boolean:
          ret = StringConversion.StringToBoolean(value, column.True, column.False);
          break;

        case DataType.Guid:
          ret = GetGuidNull(value, column.ColumnOrdinal);
          break;

        default:
          /* TextToHTML and TextToHTMLFull and TextPart have been handled in the CSV Reader as the length of the fields would change */
          ret = value;
          break;
      }

      return ret ?? DBNull.Value;
    }

    protected virtual string GetUniqueName(ICollection<string> previousColumns, int ordinal, string nametoadd)
    {
      var newName = StringUtils.MakeUniqueInCollection(previousColumns, nametoadd);
      if (newName != nametoadd)
        HandleWarning(ordinal, $"Column '{nametoadd}' exists more than once replaced with {newName}");

      return newName;
    }

    /// <summary>
    ///   Handles the waring for a date.
    /// </summary>
    /// <param name="inputDate">The input date.</param>
    /// <param name="inputTime">The input time.</param>
    /// <param name="columnNumber">The column.</param>
    protected virtual void HandleDateError(string inputDate, string inputTime, int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      var column = GetColumn(columnNumber);
      if (column.Ignore)
        return;
      var display = column.DateFormat.ReplaceDefaults("/", column.DateSeparator, ":", column.TimeSeparator);

      HandleError(columnNumber,
        !string.IsNullOrEmpty(inputTime)
          ? $"'{inputDate} {inputTime}' is not a date of the format {display} {column.TimePartFormat}"
          : $"'{inputDate}' is not a date of the format {display}");
    }

    /// <summary>
    ///   Handles the error.
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <param name="message">The message.</param>
    protected virtual void HandleError(int columnNumber, string message)
    {
      Warning?.Invoke(this, new WarningEventArgs(RecordNumber, columnNumber, message, StartLineNumber, EndLineNumber,
        Column != null && columnNumber >= 0 && columnNumber < m_FieldCount && Column[columnNumber] != null
          ? Column[columnNumber].Name
          : null));
    }

    /// <summary>
    ///   Does handle TextToHML, TextToHtmlFull, TextPart and TreatNBSPAsSpace and does update the maximum column size
    ///   Attention: Trimming needs to be handled before hand
    /// </summary>
    /// <param name="inputString">The input string.</param>
    /// <param name="columnNumber">The column number</param>
    /// <param name="handleNullText">if set to <c>true</c> [handle null text].</param>
    /// <returns>
    ///   The proper encoded or cut text as returned for the column
    /// </returns>
    protected string HandleTextAndSetSize(string inputString, int columnNumber, bool handleNullText)
    {
      // in case its not a string
      if (string.IsNullOrEmpty(inputString))
        return inputString;

      if (handleNullText && StringUtils.ShouldBeTreatedAsNull(inputString, m_FileSetting.TreatTextAsNull))
        return null;

      if (m_FileSetting.TrimmingOption == TrimmingOption.All)
        inputString = inputString.Trim();

      var column = GetColumn(columnNumber);
      var output = inputString;

      switch (column.DataType)
      {
        case DataType.TextToHtml:
          output = HTMLStyle.TextToHtmlEncode(inputString);
          if (!inputString.Equals(output, StringComparison.Ordinal))
            HandleWarning(columnNumber, $"HTML encoding removed from {inputString}");
          break;

        case DataType.TextToHtmlFull:
          output = HTMLStyle.HtmlEncodeShort(inputString);
          if (!inputString.Equals(output, StringComparison.Ordinal))
            HandleWarning(columnNumber, $"HTML encoding removed from {inputString}");
          break;

        case DataType.TextPart:
          output = StringConversion.StringToTextPart(inputString, column.PartSplitter, column.Part, column.PartToEnd);
          if (output == null)
            HandleWarning(columnNumber, $"Part {column.Part} of text {inputString} is empty.");
          break;
      }

      if (string.IsNullOrEmpty(output))
        return null;

      if (m_FileSetting.TreatNBSPAsSpace && output.IndexOf((char) 0xA0) != -1)
        output = output.Replace((char) 0xA0, ' ');

      if (output.Length > 0 && column.Size < output.Length)
        column.Size = output.Length;

      return output;
    }

    /// <summary>
    ///   Handles the Event if reading the file is completed
    /// </summary>
    protected virtual void HandleReadFinished()
    {
      if (m_IsFinished)
        return;
      m_IsFinished = true;
      m_FileSetting.ProcessTimeUtc = DateTime.UtcNow;
      if (m_FileSetting is IFileSettingPhysicalFile physicalFile)
      {
        physicalFile.FileSize = new FileInfo(physicalFile.FullPath).Length;
        Logger.Debug("Finished reading {filesetting} Records: {records} in {filesize} Byte", m_FileSetting,
          RecordNumber, physicalFile.FileSize);
      }
      else
      {
        Logger.Debug("Finished reading {filesetting} Records: {records}", m_FileSetting, RecordNumber);
      }

      HandleShowProgress("Finished Reading from source", RecordNumber, cMaxValue);
      ReadFinished?.Invoke(this, null);
    }

    protected void HandleRemoteFile()
    {
      if (ApplicationSetting.RemoteFileHandler == null || !(m_FileSetting is IFileSettingPhysicalFile remote)) return;
      if (string.IsNullOrEmpty(remote.RemoteFileName)) return;
      try
      {
        HandleShowProgress("Handling Remote file…");
        m_FileSetting.LatestSourceTimeUtc = ApplicationSetting.RemoteFileHandler(remote.RemoteFileName, remote.FileName,
          remote.FullPath, ProcessDisplay, remote.ThrowErrorIfNotExists);
      }
      catch (Exception ex)
      {
        if (remote.ThrowErrorIfNotExists)
          throw new FileReaderException(
            $"The file is flagged as required for further processing\nAn error has occurred handling {remote.ID}", ex);
        // ignore otherwise
      }
    }

    /// <summary>
    ///   Shows the process.
    /// </summary>
    /// <param name="text">The text.</param>
    protected virtual void HandleShowProgress(string text)
    {
      ProcessDisplay?.SetProcess(text);
    }

    /// <summary>
    ///   Shows the process twice a second
    /// </summary>
    /// <param name="text">Leading Text</param>
    /// <param name="recordNumber">The record number.</param>
    protected virtual void HandleShowProgressPeriodic(string text, long recordNumber)
    {
      if (ProcessDisplay != null)
        m_IntervalAction.Invoke(delegate { HandleShowProgress(text, recordNumber, GetRelativePosition()); });
    }

    /// <summary>
    ///   Trims the value text based on the File Setting
    /// </summary>
    /// <param name="inputValue">The not trimmed raw value</param>
    /// <param name="quoted">Set to <c>true</c> if the read text was quoted</param>
    /// <returns>The correctly trimmed and Null replaced value</returns>
    protected virtual string HandleTrimAndNBSP(string inputValue, bool quoted = false)
    {
      if (string.IsNullOrEmpty(inputValue))
        return inputValue;

      if (m_FileSetting.TreatNBSPAsSpace)
        inputValue = inputValue.Replace((char) 0xA0, ' ');
      if (m_FileSetting.TrimmingOption == TrimmingOption.All ||
          !quoted && m_FileSetting.TrimmingOption == TrimmingOption.Unquoted)
        return inputValue.Trim();

      return inputValue;
    }

    /// <summary>
    ///   Calls the event handler for warnings
    /// </summary>
    /// <param name="columnNumber">The column.</param>
    /// <param name="message">The message.</param>
    protected virtual void HandleWarning(int columnNumber, string message)
    {
      Warning?.Invoke(this, new WarningEventArgs(RecordNumber, columnNumber, message.AddWarningId(), StartLineNumber,
        EndLineNumber,
        Column != null && columnNumber >= 0 && columnNumber < m_FieldCount && Column[columnNumber] != null
          ? Column[columnNumber].Name
          : null));
    }

    /// <summary>
    ///   Initializes the column array to a give count.
    /// </summary>
    /// <param name="fieldCount">The field count.</param>
    protected virtual void InitColumn(int fieldCount)
    {
      Debug.Assert(fieldCount >= 0);
      m_FieldCount = fieldCount;
      Column = new Column[fieldCount];
      CurrentRowColumnText = new string[fieldCount];
      AssociatedTimeCol = new int[fieldCount];
      AssociatedTimeZoneCol = new int[fieldCount];
      for (var counter = 0; counter < fieldCount; counter++)
      {
        Column[counter] = new Column {ColumnOrdinal = counter};
        AssociatedTimeCol[counter] = -1;
        AssociatedTimeZoneCol[counter] = -1;
      }
    }

    /// <summary>
    ///   Gets the field headers and Initialized the columns
    /// </summary>
    protected void ParseColumnName(IList<string> headerRow)
    {
      Debug.Assert(FieldCount >= 0);
      Debug.Assert(headerRow != null);

      if (headerRow.Count == 0)
        return;
      InitColumn(FieldCount);

      if (!m_FileSetting.HasFieldHeader)
      {
        for (var i = 0; i < FieldCount; i++)
          GetColumn(i).Name = GetDefaultName(i);
        return;
      }

      var previousColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      for (var counter = 0; counter < FieldCount; counter++)
      {
        var columnName = counter < headerRow.Count ? headerRow[counter] : string.Empty;
        string resultingName;
        if (string.IsNullOrEmpty(columnName))
        {
          resultingName = GetDefaultName(counter);
          // HandleWarning(counter, string.Format(CultureInfo.CurrentCulture, "{0}Column title is empty.", CWarningId));
        }
        else
        {
          resultingName = columnName.Trim();
          if (columnName.Length != resultingName.Length)
            HandleWarning(counter,
              $"Column title '{columnName}' had leading or tailing spaces, these have been removed."
                .AddWarningId());

          if (resultingName.Length > 128)
          {
            resultingName = resultingName.Substring(0, 128);
            HandleWarning(counter,
              $"Column title '{resultingName.Substring(0, 20)}…' has been cut off after 128 characters."
                .AddWarningId());
          }

          resultingName = GetUniqueName(previousColumns, counter, resultingName);
        }

        GetColumn(counter).Name = resultingName;
        previousColumns.Add(resultingName);
      }
    }

    /// <summary>
    ///   Adds a Format exception.
    /// </summary>
    /// <param name="columnNumber">The column.</param>
    /// <param name="message">The message.</param>
    protected virtual FormatException WarnAddFormatException(int columnNumber, string message)
    {
      HandleError(columnNumber, message);
      return new FormatException(message);
    }

    private DateTime? AdjustTz(DateTime? input, Column column)
    {
      if (!input.HasValue)
        return null;
      string timeZone = null;
      // Constant value
      if (column.TimeZonePart.Length > 2 && column.TimeZonePart.StartsWith("\"", StringComparison.Ordinal) &&
          column.TimeZonePart.EndsWith("\"", StringComparison.Ordinal))
      {
        timeZone = column.TimeZonePart.Substring(1, column.TimeZonePart.Length - 2);
      }
      // lookup in other column
      else
      {
        var colTimeZone = AssociatedTimeZoneCol[column.ColumnOrdinal];
        if (colTimeZone > -1 && colTimeZone < FieldCount)
          timeZone = GetString(colTimeZone);
      }

      try
      {
        return input.Value.ConvertTime(timeZone, ApplicationSetting.DestinationTimeZone);
      }
      catch (ConversionException ex)
      {
        HandleWarning(column.ColumnOrdinal, ex.Message);
        return null;
      }
    }

    /// <summary>
    ///   Gets the boolean value.
    /// </summary>
    /// <param name="inputBoolean">The input.</param>
    /// <param name="columnNumber">The column.</param>
    /// <returns>
    ///   The Boolean, if conversion is not successful: <c>NULL</c> the event handler for warnings is called
    /// </returns>
    private bool? GetBooleanNull(string inputBoolean, int columnNumber)
    {
      Debug.Assert(columnNumber >= 0);
      Debug.Assert(columnNumber < FieldCount);
      var column = GetColumn(columnNumber);

      var strictBool = StringConversion.StringToBooleanStrict(inputBoolean, column.True, column.False);
      if (strictBool != null)
        return strictBool.Item1;

      HandleError(columnNumber, $"'{inputBoolean}' is not a boolean");
      return null;
    }
  }
}