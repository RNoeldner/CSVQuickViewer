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

namespace CsvTools
{
  using System;
  using System.Collections.Generic;
  using System.Data;
  using System.Data.Common;
  using System.Diagnostics;
  using System.Globalization;
  using System.IO;
  using System.Threading;
  using System.Threading.Tasks;

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
    public static readonly ICollection<string> ArtificialFields = new HashSet<string>
                                                                    {
                                                                      cRecordNumberFieldName,
                                                                      cStartLineNumberFieldName,
                                                                      cEndLineNumberFieldName,
                                                                      cErrorField,
                                                                      cPartitionField
                                                                    };

    protected readonly string DestinationTimeZone;

    /// <summary>
    ///   An array of associated col
    /// </summary>
    protected int[] AssociatedTimeCol;

    /// <summary>
    ///   An array of column
    /// </summary>
    protected Column[] Column;

    /// <summary>
    ///   An array of current row column text
    /// </summary>
    protected string[] CurrentRowColumnText;

    private readonly IntervalAction m_IntervalAction = new IntervalAction();

    /// <summary>
    ///   An array of associated col
    /// </summary>
    private int[] m_AssociatedTimeZoneCol;

    /// <summary>
    ///   Used to avoid duplicate disposal
    /// </summary>
    private bool m_DisposedValue;

    /// <summary>
    ///   Number of Columns in the reader
    /// </summary>
    private int m_FieldCount;

    /// <summary>
    ///   used to avoid reporting a fished execution twice it might be called on error before being
    ///   called once execution is done
    /// </summary>
    private bool m_IsFinished;

    protected BaseFileReader(IFileSetting fileSetting, string destinationTimeZone, IProcessDisplay processDisplay)
    {
      FileSetting = fileSetting ?? throw new ArgumentNullException(nameof(fileSetting));
      if (FileSetting is IFileSettingPhysicalFile fileSettingPhysical)
      {
        if (string.IsNullOrEmpty(fileSettingPhysical.FileName))
          throw new FileReaderException("FileName must be set");

        if (OnOpen == null)
          if (!FileSystemUtils.FileExists(fileSettingPhysical.FullPath))
            throw new FileNotFoundException(
              $"The file '{FileSystemUtils.GetShortDisplayFileName(fileSettingPhysical.FileName, 80)}' does not exist or is not accessible.",
              fileSettingPhysical.FileName);
      }
      DestinationTimeZone = string.IsNullOrEmpty(destinationTimeZone) ? TimeZoneInfo.Local.Id : destinationTimeZone;
      if (processDisplay != null)
      {
        processDisplay.Maximum = 0;
        CancellationToken = processDisplay.CancellationToken;
      }
      else
      {
        CancellationToken = CancellationToken.None;
      }

      ProcessDisplay = processDisplay;
      Logger.Debug("Created Reader for {filesetting}", fileSetting.ToString());
    }

    /// <summary>
    ///   Occurs when something went wrong during opening of the setting, this might be the file
    ///   does not exist or a query ran into a timeout
    /// </summary>
    public virtual event EventHandler<RetryEventArgs> OnAskRetry;

    /// <summary>
    ///   Occurs before the file is opened
    /// </summary>
    public event EventHandler OnOpen;

    /// <summary>
    ///   Event to be raised if reading the files is completed
    /// </summary>
    public event EventHandler ReadFinished;

    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    public virtual event EventHandler<WarningEventArgs> Warning;

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

    /// <summary>
    ///   Gets the file setting used in this reader
    /// </summary>
    /// <value>The file setting.</value>
    public IFileSetting FileSetting
    {
      get;
    }

    public abstract bool IsClosed
    {
      get;
    }

    public double NotifyAfterSeconds
    {
      set
      {
        if (m_IntervalAction != null) m_IntervalAction.NotifyAfterSeconds = value;
      }
    }

    /// <summary>
    ///   A process display to stop long running processes
    /// </summary>
    public IProcessDisplay ProcessDisplay { get; }

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

    public virtual bool SupportsReset => true;

    /// <summary>
    ///   A cancellation token, to stop long running processes
    /// </summary>
    protected CancellationToken CancellationToken { get; }

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

    public static IEnumerable<string> ParseColumnNames(
      IEnumerable<string> columns,
      int fieldCount,
      ColumnErrorDictionary warnings)
    {
      var previousColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      using (var enumerator = columns.GetEnumerator())
        for (var counter = 0; counter < fieldCount; counter++)
        {
          var columnName = (enumerator.MoveNext()) ? enumerator.Current : string.Empty;
          string resultingName;
          if (string.IsNullOrEmpty(columnName))
          {
            resultingName = GetDefaultName(counter);
            warnings.Add(counter, $"Column title was empty, set to {resultingName}.".AddWarningId());
          }
          else
          {
            resultingName = columnName.Trim();
            if (columnName.Length != resultingName.Length)
              warnings.Add(
                counter,
                $"Column title '{columnName}' had leading or tailing spaces, these have been removed.".AddWarningId());

            if (resultingName.Length > 128)
            {
              resultingName = resultingName.Substring(0, 128);
              warnings.Add(
                counter,
                $"Column title '{resultingName.Substring(0, 20)}…' too long, cut off after 128 characters."
                  .AddWarningId());
            }

            var newName = StringUtils.MakeUniqueInCollection(previousColumns, resultingName);
            if (newName != resultingName)
              warnings.Add(counter, $"Column title '{resultingName}' exists more than once replaced with {newName}");
          }

          yield return resultingName;
          previousColumns.Add(resultingName);
        }
    }

    /// <summary>
    ///   Closes the <see cref="IDataReader" /> Object.
    /// </summary>
    public virtual void Close() => EndOfFile = true;

    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting
    ///   unmanaged resources.
    /// </summary>
    public virtual void Dispose() => Dispose(true);

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
      throw WarnAddFormatException(
        i,
        $"'{CurrentRowColumnText[i]}' is not a boolean ({GetColumn(i).ValueFormat.True}/{GetColumn(i).ValueFormat.False})");
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
        throw WarnAddFormatException(i, $"'{CurrentRowColumnText[i]}' is not byte");
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
    /// <param name="fieldOffset">The index within the row from which to start the read operation.</param>
    /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
    /// <param name="bufferOffset">
    ///   The index for <paramref name="buffer" /> to start the read operation.
    /// </param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>The actual number of characters read.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public virtual long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
    {
      Debug.Assert(CurrentRowColumnText != null);
      var offset = (int)fieldOffset;
      var maxLen = CurrentRowColumnText[i].Length - offset;
      if (maxLen > length)
        maxLen = length;
      CurrentRowColumnText[i].CopyTo(
        offset,
        buffer ?? throw new ArgumentNullException(nameof(buffer)),
        bufferOffset,
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
      Debug.Assert(Column != null);
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount && columnNumber < Column.Length);
      return Column[columnNumber];
    }

    public virtual DataTable GetDataTable(long recordLimit)
    {
      if (IsClosed)
        Open();

      var dataTable = new DataTable();
      try
      {
        for (var col = 0; col < FieldCount; col++)
          dataTable.Columns.Add(new DataColumn(GetName(col), GetFieldType(col)));

        if (!CancellationToken.IsCancellationRequested)
        {
          dataTable.BeginLoadData();
          if (recordLimit < 1)
            recordLimit = long.MaxValue;

          while (Read() && dataTable.Rows.Count < recordLimit && !CancellationToken.IsCancellationRequested)
          {
            var readerValues = new object[FieldCount];
            if (GetValues(readerValues) > 0)
              dataTable.Rows.Add(readerValues);
          }
        }
      }
      finally
      {
        dataTable.EndLoadData();
      }

      return dataTable;
    }

    public virtual async Task<DataTable> GetDataTableAsync(long recordLimit)
    {
      if (IsClosed)
        Open();

      var dataTable = new DataTable
      {
        TableName = FileSetting.ID,
        Locale = CultureInfo.CurrentCulture,
        CaseSensitive = false
      };
      try
      {
        // create columns, it is been relied on that teh column names are unique
        for (var col = 0; col < FieldCount; col++)
          dataTable.Columns.Add(new DataColumn(GetName(col), GetFieldType(col)));

        if (!CancellationToken.IsCancellationRequested)
        {
          dataTable.BeginLoadData();
          if (recordLimit < 1)
            recordLimit = long.MaxValue;

          while (await ReadAsync() && dataTable.Rows.Count < recordLimit && !CancellationToken.IsCancellationRequested)
          {
            var readerValues = new object[FieldCount];
            if (GetValues(readerValues) > 0)
              dataTable.Rows.Add(readerValues);
          }
        }
      }
      finally
      {
        dataTable.EndLoadData();
      }

      return dataTable;
    }

    /// <summary>
    ///   Gets the date and time data value of the specified field.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>The date and time data value of the specified field.</returns>
    public virtual DateTime GetDateTime(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentRowColumnText != null && columnNumber < CurrentRowColumnText.Length);

      var dt = GetDateTimeNull(
        null,
        CurrentRowColumnText[columnNumber],
        null,
        GetTimeValue(columnNumber),
        GetColumn(columnNumber),
        true);
      if (dt.HasValue)
        return dt.Value;

      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(
        columnNumber,
        $"'{CurrentRowColumnText[columnNumber]}' is not a date of the format {GetColumn(columnNumber).ValueFormat.DateFormat}");
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
      throw WarnAddFormatException(columnNumber, $"'{CurrentRowColumnText[columnNumber]}' is not a decimal");
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
      throw WarnAddFormatException(columnNumber, $"'{CurrentRowColumnText[columnNumber]}' is not a double");
    }

    /// <summary>
    ///   Gets the type of the field.
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <returns>The .NET type of the column</returns>
    public virtual Type GetFieldType(int columnNumber) => GetColumn(columnNumber).ValueFormat.DataType.GetNetType();

    /// <summary>
    ///   Gets the single-precision floating point number of the specified field.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>The single-precision floating point number of the specified field.</returns>
    public virtual float GetFloat(int columnNumber)
    {
      Debug.Assert(0 <= columnNumber);

      var decimalValue = GetDecimalNull(CurrentRowColumnText[columnNumber], columnNumber);
      if (decimalValue.HasValue)
        return Convert.ToSingle(decimalValue, CultureInfo.InvariantCulture);

      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(columnNumber, $"'{CurrentRowColumnText[columnNumber]}' is not a float");
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

      throw WarnAddFormatException(columnNumber, $"'{CurrentRowColumnText[columnNumber]}' is not an GUID");
    }

    /// <summary>
    ///   Gets the 16-bit signed integer value of the specified field.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>The 16-bit signed integer value of the specified field.</returns>
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

      var parsed = StringConversion.StringToInt32(
        CurrentRowColumnText[columnNumber],
        column.ValueFormat.DecimalSeparatorChar,
        column.ValueFormat.GroupSeparatorChar);
      if (parsed.HasValue)
        return parsed.Value;

      // Warning was added by GetInt32Null
      throw WarnAddFormatException(columnNumber, $"'{CurrentRowColumnText[columnNumber]}' is not an integer");
    }

    /// <summary>
    ///   Gets the int32 value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="column">The column.</param>
    /// <returns></returns>
    public int? GetInt32Null(string inputValue, Column column)
    {
      Debug.Assert(column != null);
      var ret = StringConversion.StringToInt32(
        inputValue,
        column.ValueFormat.DecimalSeparatorChar,
        column.ValueFormat.GroupSeparatorChar);
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

      var parsed = StringConversion.StringToInt64(
        CurrentRowColumnText[columnNumber],
        column.ValueFormat.DecimalSeparatorChar,
        column.ValueFormat.GroupSeparatorChar);
      if (parsed.HasValue)
        return parsed.Value;

      throw WarnAddFormatException(columnNumber, $"'{CurrentRowColumnText[columnNumber]}' is not an long integer");
    }

    /// <summary>
    ///   Gets the int32 value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="column">The column.</param>
    /// <returns></returns>
    public long? GetInt64Null(string inputValue, Column column)
    {
      Debug.Assert(column != null);
      var ret = StringConversion.StringToInt64(
        inputValue,
        column.ValueFormat.DecimalSeparatorChar,
        column.ValueFormat.GroupSeparatorChar);
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
    ///   Returns a <see cref="DataTable" /> that describes the column meta data of the <see
    ///   cref="IDataReader" /> .
    /// </summary>
    /// <returns>A <see cref="DataTable" /> that describes the column meta data.</returns>
    /// <exception cref="InvalidOperationException">The <see cref="IDataReader" /> is closed.</exception>
    public virtual DataTable GetSchemaTable()
    {
      if (IsClosed)
        Open();
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
        if (column.Convert && column.ValueFormat.DataType != DataType.String)
          schemaRow[7] = column.ValueFormat.DataType.GetNetType();
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
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);

      if (IsDBNull(columnNumber))
        return DBNull.Value;
      var column = GetColumn(columnNumber);

      object ret;
      try
      {
        switch (column.ValueFormat.DataType)
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

      return ret ?? DBNull.Value;
    }

    /// <summary>
    ///   Gets all the attribute fields in the collection for the current record.
    /// </summary>
    /// <param name="values">An array of object to copy the attribute fields into.</param>
    /// <returns>The number of instances of object in the array.</returns>
    public virtual int GetValues(object[] values)
    {
      if (values is null)
        throw new ArgumentNullException(nameof(values));
      for (var col = 0; col < values.Length; col++)
        values[col] = GetValue(col);
      return FieldCount;
    }

    /// <summary>
    ///   Handles the error.
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <param name="message">The message.</param>
    public void HandleError(int columnNumber, string message) =>
      Warning?.Invoke(
        this,
        new WarningEventArgs(
          RecordNumber,
          columnNumber,
          message,
          StartLineNumber,
          EndLineNumber,
          Column != null && columnNumber >= 0 && columnNumber < m_FieldCount && Column[columnNumber] != null
            ? Column[columnNumber].Name
            : null));

    /// <summary>
    ///   Calls the event handler for warnings
    /// </summary>
    /// <param name="columnNumber">The column.</param>
    /// <param name="message">The message.</param>
    public void HandleWarning(int columnNumber, string message) =>
      Warning?.Invoke(
        this,
        new WarningEventArgs(
          RecordNumber,
          columnNumber,
          message.AddWarningId(),
          StartLineNumber,
          EndLineNumber,
          Column != null && columnNumber >= 0 && columnNumber < m_FieldCount && Column[columnNumber] != null
            ? Column[columnNumber].Name
            : null));

    /// <summary>
    ///   Checks if the column should be read
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <returns><c>true</c> if this column should not be read</returns>
    public virtual bool IgnoreRead(int columnNumber) => GetColumn(columnNumber).Ignore;

    /// <summary>
    ///   Return whether the specified field is set to null.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>true if the specified field is set to null; otherwise, false.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public virtual bool IsDBNull(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      if (CurrentRowColumnText == null || CurrentRowColumnText.Length <= columnNumber)
        return true;
      if (Column[columnNumber].ValueFormat.DataType == DataType.DateTime)
      {
        if (AssociatedTimeCol[columnNumber] == -1 || AssociatedTimeCol[columnNumber] >= CurrentRowColumnText.Length)
          return string.IsNullOrEmpty(CurrentRowColumnText[columnNumber]);

        return string.IsNullOrEmpty(CurrentRowColumnText[columnNumber])
               && string.IsNullOrEmpty(CurrentRowColumnText[AssociatedTimeCol[columnNumber]]);
      }

      if (string.IsNullOrEmpty(CurrentRowColumnText[columnNumber]))
        return true;

      if (FileSetting.TrimmingOption == TrimmingOption.All
          && Column[columnNumber].ValueFormat.DataType >= DataType.String)
        return CurrentRowColumnText[columnNumber].Trim().Length == 0;

      return false;
    }

    /// <summary>
    ///   Advances the data reader to the next result, when reading the results of batch SQL statements.
    /// </summary>
    /// <returns>true if there are more rows; otherwise, false.</returns>
    public virtual bool NextResult() => false;

    public abstract void Open();

    /// <summary>
    ///   Overrides the column format from setting.
    /// </summary>
    public virtual void OverrideColumnFormatFromSetting()
    {
      Debug.Assert(AssociatedTimeCol != null);

      for (var colIndex = 0; colIndex < FieldCount; colIndex++)
      {
        var setting = FileSetting.ColumnCollection.Get(Column[colIndex].Name);
        if (setting != null)
        {
          setting.CopyTo(Column[colIndex]);

          // Copy to has replaced he column Ordinal but this should be kept
          Column[colIndex].ColumnOrdinal = colIndex;
        }

        if (Column[colIndex].ValueFormat.DataType != DataType.DateTime || setting == null || !setting.Convert
            || setting.Ignore)
          continue;

        AssociatedTimeCol[colIndex] = GetOrdinal(Column[colIndex].TimePart);
        m_AssociatedTimeZoneCol[colIndex] = GetOrdinal(Column[colIndex].TimeZonePart);
      }

      // Any defined column not present in file is removed this can happen if the file is changed or
      // changing the encoding the name might show different W�hrung <> Währung
      var remove = new List<Column>();
      foreach (var setting in FileSetting.ColumnCollection)
      {
        var found = false;
        for (var colIndex = 0; colIndex < FieldCount; colIndex++)
          if (Column[colIndex].Name.Equals(setting.Name, StringComparison.OrdinalIgnoreCase))
          {
            found = true;
            break;
          }

        if (!found)
          remove.Add(setting);
      }

      foreach (var col in remove)

        // HandleWarning(-1, $"Column \"{col.Name}\" not found in file, this can happen if column
        // name is changed");
        FileSetting.ColumnCollection.Remove(col);
    }

    public abstract bool Read();

    public abstract Task<bool> ReadAsync();

    /// <summary>
    ///   Resets the position and buffer to the header in case the file has a header
    /// </summary>
    public void ResetPositionToFirstDataRow()
    {
      EndLineNumber = 0;
      RecordNumber = 0;
      EndOfFile = false;
    }

    private static string GetDefaultName(int i) => $"Column{i + 1}";

    /// <summary>
    ///   Gets the default schema row array.
    /// </summary>
    /// <returns>an Array of objects for a new row in a Schema Table</returns>
    protected static object[] GetDefaultSchemaRowArray() =>
      new object[]
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
          (int)DbType.String, // 15- ProviderType
          string.Empty, // 16- BaseCatalogName
          string.Empty, // 17- BaseServerName
          false, // 18- IsAutoIncrement
          false, // 19- IsHidden
          true, // 20- IsReadOnly
          false // 21- IsRowVersion
        };

    /// <summary>
    ///   Gets the empty schema table.
    /// </summary>
    /// <returns>A Data Table with the columns for a Schema Table</returns>
    protected static DataTable GetEmptySchemaTable()
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

    protected void BeforeOpen(string message)
    {
      HandleShowProgress(message);

      OnOpen?.Invoke(this, new EventArgs());

      // now a physical file must exist
      if (FileSetting is IFileSettingPhysicalFile fileSettingPhysical && !FileSystemUtils.FileExists(fileSettingPhysical.FullPath))
        throw new FileNotFoundException(
          $"The file '{FileSystemUtils.GetShortDisplayFileName(fileSettingPhysical.FileName, 80)}' does not exist or is not accessible.",
          fileSettingPhysical.FileName);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (m_DisposedValue) return;
      Close();
      m_DisposedValue = true;
    }

    protected virtual void FinishOpen()
    {
      m_IsFinished = false;
      if (FieldCount > 0)

        // Override the column settings and store the columns for later reference
        OverrideColumnFormatFromSetting();

      // in case caching is setup store the headers
      if (FunctionalDI.GetColumnHeader != null)
        FunctionalDI.StoreHeader?.Invoke(FileSetting, Column);

      if (ProcessDisplay != null)
        ProcessDisplay.Maximum = cMaxValue;
    }

    /// <summary>
    ///   This routine will read a date from a typed or untyped reader, will combined date with time
    ///   and apply timeZone adjustments
    /// </summary>
    /// <param name="inputDate"></param>
    /// <param name="strInputDate"></param>
    /// <param name="inputTime"></param>
    /// <param name="strInputTime"></param>
    /// <param name="column"></param>
    /// <param name="serialDateTime"></param>
    /// <returns></returns>
    protected DateTime? GetDateTimeNull(
      object inputDate,
      string strInputDate,
      object inputTime,
      string strInputTime,
      Column column,
      bool serialDateTime)
    {
      var dateTime = StringConversion.CombineObjectsToDateTime(
        inputDate,
        strInputDate,
        inputTime,
        strInputTime,
        serialDateTime,
        column.ValueFormat,
        out var timeSpanLongerThanDay);
      if (timeSpanLongerThanDay)
      {
        var passedIn = strInputTime;
        if (inputTime != null)
          passedIn = inputTime.ToString();
        HandleWarning(
          column.ColumnOrdinal,
          $"'{passedIn}' is outside expected range 00:00 - 23:59, the date has been adjusted");
      }

      if (!dateTime.HasValue && !string.IsNullOrEmpty(strInputDate)
                             && !string.IsNullOrEmpty(column.ValueFormat.DateFormat)
                             && strInputDate.Length > column.ValueFormat.DateFormat.Length)
      {
        var inputDateNew = strInputDate.Substring(0, column.ValueFormat.DateFormat.Length);
        dateTime = StringConversion.CombineStringsToDateTime(
          inputDateNew,
          column.ValueFormat.DateFormat,
          strInputTime,
          column.ValueFormat.DateSeparator,
          column.ValueFormat.TimeSeparator,
          serialDateTime);
        if (dateTime.HasValue)
        {
          var display = column.ValueFormat.DateFormat.ReplaceDefaults(
            "/",
            column.ValueFormat.DateSeparator,
            ":",
            column.ValueFormat.TimeSeparator);
          HandleWarning(
            column.ColumnOrdinal,
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
    protected object GetTypedValueFromString(string value, string timeValue, Column column)
    {
      Debug.Assert(column != null);

      object ret;

      // Get Column Format for Column
      switch (column.ValueFormat.DataType)
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
          ret = StringConversion.StringToBoolean(value, column.ValueFormat.True, column.ValueFormat.False);
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

    /// <summary>
    ///   Handles the Event if reading the file is completed
    /// </summary>
    protected void HandleReadFinished()
    {
      if (m_IsFinished)
        return;
      m_IsFinished = true;
      FileSetting.ProcessTimeUtc = DateTime.UtcNow;
      if (FileSetting is IFileSettingPhysicalFile physicalFile)
      {
        physicalFile.FileSize = FileSystemUtils.GetFileInfo(physicalFile.FullPath).Length;
        Logger.Debug(
          "Finished reading {filesetting} Records: {records} in {filesize} Byte",
          FileSetting.ToString(),
          RecordNumber,
          physicalFile.FileSize);
      }
      else
      {
        Logger.Debug("Finished reading {filesetting} Records: {records}", FileSetting.ToString(), RecordNumber);
      }

      HandleShowProgress("Finished Reading from source", RecordNumber, cMaxValue);
      ReadFinished?.Invoke(this, null);
    }

    /// <summary>
    ///   Shows the process.
    /// </summary>
    /// <param name="text">Leading Text</param>
    /// <param name="recordNumber">The record number.</param>
    /// <param name="progress">The progress (a value between 0 and MaxValue)</param>
    protected virtual void HandleShowProgress(string text, long recordNumber, int progress)
    {
      var rec = recordNumber > 1 ? $"\nRecord {recordNumber:N0}" : string.Empty;
      ProcessDisplay?.SetProcess($"{text}{rec}", progress, false);
    }

    /// <summary>
    ///   Shows the process.
    /// </summary>
    /// <param name="text">The text.</param>
    protected void HandleShowProgress(string text) => ProcessDisplay?.SetProcess(text, -1, true);

    /// <summary>
    ///   Shows the process twice a second
    /// </summary>
    /// <param name="text">Leading Text</param>
    /// <param name="recordNumber">The record number.</param>
    protected void HandleShowProgressPeriodic(string text, long recordNumber)
    {
      if (ProcessDisplay != null)
        m_IntervalAction.Invoke(delegate { HandleShowProgress(text, recordNumber, GetRelativePosition()); });
    }

    /// <summary>
    ///   Does handle TextToHML, TextToHtmlFull, TextPart and TreatNBSPAsSpace and does update the
    ///   maximum column size
    ///   Attention: Trimming needs to be handled before hand
    /// </summary>
    /// <param name="inputString">The input string.</param>
    /// <param name="columnNumber">The column number</param>
    /// <param name="handleNullText">if set to <c>true</c> [handle null text].</param>
    /// <returns>The proper encoded or cut text as returned for the column</returns>
    protected string HandleTextAndSetSize(string inputString, int columnNumber, bool handleNullText)
    {
      // in case its not a string
      if (string.IsNullOrEmpty(inputString))
        return inputString;

      if (handleNullText && StringUtils.ShouldBeTreatedAsNull(inputString, FileSetting.TreatTextAsNull))
        return null;

      if (FileSetting.TrimmingOption == TrimmingOption.All)
        inputString = inputString.Trim();

      var column = GetColumn(columnNumber);
      var output = inputString;

      switch (column.ValueFormat.DataType)
      {
        case DataType.TextToHtml:
        {
          output = HTMLStyle.TextToHtmlEncode(inputString);
          if (!inputString.Equals(output, StringComparison.Ordinal))
            HandleWarning(columnNumber, $"HTML encoding removed from {inputString}");
          break;
        }

        case DataType.TextToHtmlFull:
        {
          output = HTMLStyle.HtmlEncodeShort(inputString);
          if (!inputString.Equals(output, StringComparison.Ordinal))
            HandleWarning(columnNumber, $"HTML encoding removed from {inputString}");
          break;
        }

        case DataType.TextPart:
        {
          output = StringConversion.StringToTextPart(inputString, column.PartSplitter, column.Part, column.PartToEnd);
          if (output == null)
            HandleWarning(columnNumber, $"Part {column.Part} of text {inputString} is empty.");
          break;
        }
      }

      if (string.IsNullOrEmpty(output))
        return null;

      if (FileSetting.TreatNBSPAsSpace && output.IndexOf((char)0xA0) != -1)
        output = output.Replace((char)0xA0, ' ');

      if (output.Length > 0 && column.Size < output.Length)
        column.Size = output.Length;

      return output;
    }

    /// <summary>
    ///   Trims the value text based on the File Setting
    /// </summary>
    /// <param name="inputValue">The not trimmed raw value</param>
    /// <param name="quoted">Set to <c>true</c> if the read text was quoted</param>
    /// <returns>The correctly trimmed and Null replaced value</returns>
    protected string HandleTrimAndNBSP(string inputValue, bool quoted = false)
    {
      if (string.IsNullOrEmpty(inputValue))
        return inputValue;

      if (FileSetting.TreatNBSPAsSpace)
        inputValue = inputValue.Replace((char)0xA0, ' ');
      if (FileSetting.TrimmingOption == TrimmingOption.All
          || !quoted && FileSetting.TrimmingOption == TrimmingOption.Unquoted)
        return inputValue.Trim();

      return inputValue;
    }

    /// <summary>
    ///   Displays progress, is called after <see langword="abstract" /> row has been read
    /// </summary>
    /// <param name="hasReadRow"><c>true</c> if a row has been read</param>
    protected void InfoDisplay(bool hasReadRow)
    {
      if (!hasReadRow)
        HandleReadFinished();
      else
        HandleShowProgressPeriodic("Reading", RecordNumber);
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
      m_AssociatedTimeZoneCol = new int[fieldCount];
      for (var counter = 0; counter < fieldCount; counter++)
      {
        Column[counter] = new Column { ColumnOrdinal = counter };
        AssociatedTimeCol[counter] = -1;
        m_AssociatedTimeZoneCol[counter] = -1;
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

      if (!FileSetting.HasFieldHeader)
      {
        for (var i = 0; i < FieldCount; i++)
          GetColumn(i).Name = GetDefaultName(i);
        return;
      }

      var warnings = new ColumnErrorDictionary();
      var counter = 0;
      foreach (var columnName in ParseColumnNames(headerRow, FieldCount, warnings))
        GetColumn(counter++).Name = columnName;

      foreach (var warning in warnings)
        HandleWarning(warning.Key, warning.Value);
    }

    protected bool ShouldRetry(Exception ex)
    {
      if (CancellationToken.IsCancellationRequested) return false;

      var eventArgs = new RetryEventArgs(ex);
      OnAskRetry?.Invoke(this, eventArgs);
      return eventArgs.Retry;
    }

    /// <summary>
    ///   Adds a Format exception.
    /// </summary>
    /// <param name="columnNumber">The column.</param>
    /// <param name="message">The message.</param>
    protected FormatException WarnAddFormatException(int columnNumber, string message)
    {
      HandleError(columnNumber, message);
      return new FormatException(message);
    }

    private DateTime? AdjustTz(DateTime? input, Column column)
    {
      if (!input.HasValue)
        return null;
      string timeZone = null;
      var res = column.TimeZonePart.GetPossiblyConstant();

      // Constant value
      if (res.Item2)
        timeZone = res.Item1;

      // lookup in other column
      else
      {
        var colTimeZone = m_AssociatedTimeZoneCol[column.ColumnOrdinal];
        if (colTimeZone > -1 && colTimeZone < FieldCount)
          timeZone = GetString(colTimeZone);
      }

      return FunctionalDI.AdjustTZ(input, timeZone, DestinationTimeZone, column.ColumnOrdinal, HandleWarning);
    }

    /// <summary>
    ///   Gets the boolean value.
    /// </summary>
    /// <param name="inputBoolean">The input.</param>
    /// <param name="columnNumber">The column.</param>
    /// <returns>
    ///   The Boolean, if conversion is not successful: <c>NULL</c> the event handler for warnings
    ///   is called
    /// </returns>
    private bool? GetBooleanNull(string inputBoolean, int columnNumber)
    {
      Debug.Assert(columnNumber >= 0);
      Debug.Assert(columnNumber < FieldCount);
      var column = GetColumn(columnNumber);

      var strictBool = StringConversion.StringToBooleanStrict(
        inputBoolean,
        column.ValueFormat.True,
        column.ValueFormat.False);
      if (strictBool != null)
        return strictBool.Item1;

      HandleError(columnNumber, $"'{inputBoolean}' is not a boolean");
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
    private decimal? GetDecimalNull(string inputValue, Column column)
    {
      Debug.Assert(column != null);
      var decimalValue = StringConversion.StringToDecimal(
        inputValue,
        column.ValueFormat.DecimalSeparatorChar,
        column.ValueFormat.GroupSeparatorChar,
        true);
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
    private decimal? GetDecimalNull(string inputValue, int columnNumber)
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
    private double? GetDoubleNull(string inputValue, Column column)
    {
      Debug.Assert(column != null);
      var decimalValue = GetDecimalNull(inputValue, column);
      if (decimalValue.HasValue)
        return decimal.ToDouble(decimalValue.Value);

      HandleError(column.ColumnOrdinal, $"'{inputValue}' is not a double value");
      return null;
    }

    /// <summary>
    ///   Gets the int32 value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="columnNumber">The column number.</param>
    /// <returns></returns>
    private Guid? GetGuidNull(string inputValue, int columnNumber)
    {
      if (string.IsNullOrEmpty(inputValue))
        return null;
      try
      {
        return new Guid(inputValue);
      }
      catch
      {
        throw WarnAddFormatException(columnNumber, $"'{inputValue}' is not a Guid");
      }
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
    private short GetInt16(string value, int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      var column = GetColumn(columnNumber);

      var parsed = StringConversion.StringToInt16(
        value,
        column.ValueFormat.DecimalSeparatorChar,
        column.ValueFormat.GroupSeparatorChar);
      if (parsed.HasValue)
        return parsed.Value;

      // Warning was added by GetInt32Null
      throw WarnAddFormatException(columnNumber, $"'{value}' is not a short");
    }

    /// <summary>
    ///   Handles the waring for a date.
    /// </summary>
    /// <param name="inputDate">The input date.</param>
    /// <param name="inputTime">The input time.</param>
    /// <param name="columnNumber">The column.</param>
    private void HandleDateError(string inputDate, string inputTime, int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      var column = GetColumn(columnNumber);
      if (column.Ignore)
        return;
      var display = column.ValueFormat.DateFormat.ReplaceDefaults(
        "/",
        column.ValueFormat.DateSeparator,
        ":",
        column.ValueFormat.TimeSeparator);

      HandleError(
        columnNumber,
        !string.IsNullOrEmpty(inputTime)
          ? $"'{inputDate} {inputTime}' is not a date of the format {display} {column.TimePartFormat}"
          : $"'{inputDate}' is not a date of the format {display}");
    }
  }
}