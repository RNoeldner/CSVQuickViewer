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

using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   Abstract class as base for all DataReaders
  /// </summary>
  public abstract class BaseFileReader : DbDataReader
  {
    /// <summary>
    ///   The maximum value
    /// </summary>
    protected const int c_MaxValue = 10000;

    protected readonly long RecordLimit;

    /// <summary>
    ///   An array of associated col
    /// </summary>
    protected int[] AssociatedTimeCol;

    /// <summary>
    ///   An array of column
    /// </summary>
    protected ImmutableColumn[] Column;

    /// <summary>
    ///   An array of current row column text
    /// </summary>
    protected string[] CurrentRowColumnText;

    protected EventHandler<ProgressEventArgs> ReportProgress;
    protected bool SelfOpenedStream;
    protected EventHandler<long> SetMaxProcess;
    [NotNull] private readonly IReadOnlyCollection<ImmutableColumn> m_ColumnDefinition;
    private readonly IntervalAction m_IntervalAction = new IntervalAction();

    /// <summary>
    ///   An array of associated col
    /// </summary>
    private int[] m_AssociatedTimeZoneCol;

    /// <summary>
    ///   Number of Columns in the reader
    /// </summary>
    private int m_FieldCount;

    /// <summary>
    ///   used to avoid reporting a fished execution twice it might be called on error before being
    ///   called once execution is done
    /// </summary>
    private bool m_IsFinished;

    /// <summary>
    ///   Constructor for abstract base call for <see cref="IFileReader" />
    /// </summary>
    /// <param name="fileName">Path to a physical file (if used)</param>
    /// <param name="columnDefinition">List of column definitions</param>
    /// <param name="recordLimit">Number of records that should be read</param>
    protected BaseFileReader([CanBeNull] string fileName, [CanBeNull] IEnumerable<IColumn> columnDefinition,
                             long recordLimit)
    {
      m_ColumnDefinition =  columnDefinition?.Select(col => col is ImmutableColumn immutableColumn ? immutableColumn : new ImmutableColumn(col.Name, col.ValueFormat, col.ColumnOrdinal, col.Convert, col.DestinationName, col.Ignore, col.Part, col.PartSplitter, col.PartToEnd, col.TimePart, col.TimePartFormat, col.TimeZonePart)).ToList() ??
                                 new List<ImmutableColumn>();
      RecordLimit = recordLimit < 1 ? long.MaxValue : recordLimit;
      FullPath = fileName;
      FileName = FileSystemUtils.GetFileName(fileName);
    }

    /// <summary>
    ///   Occurs when something went wrong during opening of the setting, this might be the file
    ///   does not exist or a query ran into a timeout
    /// </summary>
    public virtual event EventHandler<RetryEventArgs> OnAskRetry;

    public virtual event EventHandler<IReadOnlyCollection<IColumn>> OpenFinished;

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
    public override int Depth => 0;

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
    public override int FieldCount => m_FieldCount;

    public override bool HasRows => !EndOfFile;

    public double NotifyAfterSeconds
    {
      set
      {
        if (m_IntervalAction != null) m_IntervalAction.NotifyAfterSeconds = value;
      }
    }

    /// <summary>
    ///   Occurs before the file is opened
    /// </summary>
    public Func<Task> OnOpen { private get; set; }

    /// <summary>
    ///   Gets the percentage as value between 0 and 100
    /// </summary>
    /// <value>The percent.</value>
    public int Percent => (GetRelativePosition() * 100).ToInt();

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
    public override int RecordsAffected => -1;

    /// <summary>
    ///   Current Line Number in the text file where the record has started
    /// </summary>
    public virtual long StartLineNumber { get; protected set; }

    public virtual bool SupportsReset => true;

    public override int VisibleFieldCount => Column.Count(x => !x.Ignore);

    protected string FileName { get; }
    protected string FullPath { get; }

    /// <summary>
    ///   Gets the <see cref="object" /> with the specified name.
    /// </summary>
    /// <value></value>
    public override object this[string columnName] => GetValue(GetOrdinal(columnName));

    /// <summary>
    ///   Gets the <see cref="object" /> with the specified column.
    /// </summary>
    /// <value></value>
    public override object this[int columnNumber] => GetValue(columnNumber);

    [NotNull]
    public static Tuple<IReadOnlyCollection<string>, int> AdjustColumnName(
      [NotNull] IEnumerable<string> columns,
      int fieldCount,
      [CanBeNull] ColumnErrorDictionary warnings,
      [CanBeNull] IReadOnlyCollection<IColumn> columnDefinitions)
    {
      var issuesCounter = 0;
      var previousColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      using (var enumerator = columns.GetEnumerator())
      {
        for (var counter = 0; counter < fieldCount; counter++)
        {
          var columnName = enumerator.MoveNext() ? enumerator.Current : string.Empty;
          string resultingName;
          if (string.IsNullOrEmpty(columnName))
          {
            resultingName = GetDefaultName(counter, columnDefinitions);
            issuesCounter++;
            warnings?.Add(counter, $"Column title was empty, set to {resultingName}.".AddWarningId());
          }
          else
          {
            resultingName = columnName.Trim();

            if (columnName.Length != resultingName.Length)
              warnings?.Add(
                counter,
                $"Column title '{columnName}' had leading or tailing spaces, these have been removed.".AddWarningId());

            if (resultingName.Length > 128)
            {
              resultingName = resultingName.Substring(0, 128);
              issuesCounter++;
              warnings?.Add(
                counter,
                $"Column title '{resultingName.Substring(0, 20)}…' too long, cut off after 128 characters."
                  .AddWarningId());
            }

            var newName = StringUtils.MakeUniqueInCollection(previousColumns, resultingName);
            if (newName != resultingName)
            {
              warnings?.Add(counter, $"Column title '{resultingName}' exists more than once replaced with {newName}");
              issuesCounter++;
              resultingName= newName;
            }
          }

          previousColumns.Add(resultingName);
        }
      }

      return new Tuple<IReadOnlyCollection<string>, int>(previousColumns, issuesCounter);
    }

    /// <summary>
    ///   Closes the <see cref="IDataReader" /> Object.
    /// </summary>
    public override void Close()
    {
      Logger.Debug("Closing {filename}", FileSystemUtils.GetShortDisplayFileName(FileName));
      EndOfFile = true;
    }

    // To detect redundant calls
    /// <summary>
    ///   Gets the boolean.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    public override bool GetBoolean(int i)
    {
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
    public override byte GetByte(int i)
    {
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
    ///   Reads a stream of bytes from the specified column offset into the buffer as an array,
    ///   starting at the given buffer offset.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
    /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
    /// <param name="bufferoffset">
    ///   The index for <paramref name="buffer" /> to start the read operation.
    /// </param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>The actual number of bytes read.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) =>
      throw new NotImplementedException();

    /// <summary>
    ///   Gets the character value of the specified column.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <returns>The character value of the specified column.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public override char GetChar(int i) => CurrentRowColumnText[i][0];

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
    public override long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
    {
      var offset = (int) fieldOffset;
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
    public virtual IColumn GetColumn(int columnNumber) => Column[columnNumber];

    /// <summary>
    ///   Gets the data type information for the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>The data type information for the specified field.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public override string GetDataTypeName(int i) => GetFieldType(i)?.Name;

    /// <summary>
    ///   Gets the date and time data value of the specified field.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>The date and time data value of the specified field.</returns>
    public override DateTime GetDateTime(int columnNumber)
    {
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
    public override decimal GetDecimal(int columnNumber)
    {
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
    public override double GetDouble(int columnNumber)
    {
      var decimalValue = GetDecimalNull(CurrentRowColumnText[columnNumber], columnNumber);
      if (decimalValue.HasValue)
        return Convert.ToDouble(decimalValue.Value);

      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(columnNumber, $"'{CurrentRowColumnText[columnNumber]}' is not a double");
    }

    public override IEnumerator GetEnumerator() => new DbEnumerator(this, true);

    /// <summary>
    ///   Gets the type of the field.
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <returns>The .NET type of the column</returns>
    public override Type GetFieldType(int columnNumber) => GetColumn(columnNumber).ValueFormat.DataType.GetNetType();

    /// <summary>
    ///   Gets the single-precision floating point number of the specified field.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>The single-precision floating point number of the specified field.</returns>
    public override float GetFloat(int columnNumber)
    {
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
    public override Guid GetGuid(int columnNumber)
    {
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
    public override short GetInt16(int columnNumber) => GetInt16(CurrentRowColumnText[columnNumber], columnNumber);

    /// <summary>
    ///   Gets the int32.
    /// </summary>
    /// <param name="columnNumber">The i.</param>
    /// <returns></returns>
    public override int GetInt32(int columnNumber)
    {
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
    public int? GetInt32Null(string inputValue, [NotNull] IColumn column)
    {
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
    public override long GetInt64(int columnNumber)
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
    public long? GetInt64Null(string inputValue, IColumn column)
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
    public override string GetName(int columnNumber) => GetColumn(columnNumber).Name;

    /// <summary>
    ///   Return the index of the named field.
    /// </summary>
    /// <param name="columnName">The name of the field to find.</param>
    /// <returns>The index of the named field. If not found -1</returns>
    public override int GetOrdinal(string columnName)
    {
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
    public override DataTable GetSchemaTable()
    {
      var dataTable = ReaderConstants.GetEmptySchemaTable();
      var schemaRow = ReaderConstants.GetDefaultSchemaRowArray();

      for (var col = 0; col < FieldCount; col++)
      {
        var column = GetColumn(col);

        schemaRow[1] = column.Name; // Column name
        schemaRow[4] = column.Name; // Column name
        schemaRow[5] = col;         // Column ordinal

        // If there is a conversion get the information
        if (column.Convert && column.ValueFormat.DataType != DataType.String)
          schemaRow[7] = column.ValueFormat.DataType.GetNetType();
        else
          schemaRow[7] = typeof(string);

        dataTable.Rows.Add(schemaRow);
      }

      return dataTable;
    }

    public override Stream GetStream(int columnNumber) =>
          new MemoryStream(Encoding.UTF8.GetBytes(CurrentRowColumnText[columnNumber] ?? ""));

    /// <summary>
    ///   Gets the originally provided text in a column
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Row has not been read</exception>
    /// <exception cref="ArgumentOutOfRangeException">ColumnNumber invalid</exception>
    public override string GetString(int columnNumber)
    {
      if (CurrentRowColumnText == null)
        throw new InvalidOperationException("Row has not been read");
      if (columnNumber < 0 || columnNumber >= FieldCount || columnNumber >= CurrentRowColumnText.Length)
        throw new IndexOutOfRangeException(nameof(columnNumber));

      return CurrentRowColumnText[columnNumber];
    }

    public override TextReader GetTextReader(int columnNumber) => new StringReader(CurrentRowColumnText[columnNumber] ?? "");

    /// <summary>
    ///   Gets the value of a column
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <returns>The value of the specific field</returns>
    public override object GetValue(int columnNumber)
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

          case DataType.String:
          case DataType.TextToHtml:
          case DataType.TextToHtmlFull:
          case DataType.TextPart:
            /* TextToHTML and TextToHTMLFull have been handled in the Reader for the column as the length of the fields would change */
            ret = GetString(columnNumber);
            break;

          default:
            throw new ArgumentOutOfRangeException();
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
    public override int GetValues(object[] values)
    {
      if (values is null)
        throw new ArgumentNullException(nameof(values));
      var maxFld = values.Length;
      if (maxFld > FieldCount)
        maxFld = FieldCount;
      for (var col = 0; col < maxFld; col++)
        values[col] = GetValue(col);
      return FieldCount;
    }

    /// <summary>
    ///   Handles the error.
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <param name="message">The message.</param>
    public void HandleError(int columnNumber, [NotNull] string message) =>
      Warning?.Invoke(this, GetWarningEventArgs(columnNumber, message));

    /// <summary>
    ///   Calls the event handler for warnings
    /// </summary>
    /// <param name="columnNumber">The column.</param>
    /// <param name="message">The message.</param>
    public void HandleWarning(int columnNumber, [NotNull] string message) =>
      Warning?.Invoke(this, GetWarningEventArgs(columnNumber, message.AddWarningId()));

    /// <summary>
    ///   Return whether the specified field is set to null.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>true if the specified field is set to null; otherwise, false.</returns>
    /// <exception cref="IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see cref="IDataRecord.FieldCount" />.
    /// </exception>
    public override bool IsDBNull(int columnNumber)
    {
      if (CurrentRowColumnText == null || CurrentRowColumnText.Length <= columnNumber)
        return true;
      if (Column[columnNumber].ValueFormat.DataType != DataType.DateTime)
        return string.IsNullOrWhiteSpace(CurrentRowColumnText[columnNumber]);
      if (AssociatedTimeCol[columnNumber] == -1 || AssociatedTimeCol[columnNumber] >= CurrentRowColumnText.Length)
        return string.IsNullOrEmpty(CurrentRowColumnText[columnNumber]);

      return string.IsNullOrEmpty(CurrentRowColumnText[columnNumber])
             && string.IsNullOrEmpty(CurrentRowColumnText[AssociatedTimeCol[columnNumber]]);
    }

    public override Task<bool> IsDBNullAsync(int columnNumber, CancellationToken cancellationToken)
    {
      if (CurrentRowColumnText == null || CurrentRowColumnText.Length <= columnNumber)
        return Task.FromResult(true);
      if (Column[columnNumber].ValueFormat.DataType != DataType.DateTime)
        return Task.FromResult(string.IsNullOrWhiteSpace(CurrentRowColumnText[columnNumber]));
      if (AssociatedTimeCol[columnNumber] == -1 || AssociatedTimeCol[columnNumber] >= CurrentRowColumnText.Length)
        return Task.FromResult(string.IsNullOrEmpty(CurrentRowColumnText[columnNumber]));

      return Task.FromResult(string.IsNullOrEmpty(CurrentRowColumnText[columnNumber])
                             && string.IsNullOrEmpty(CurrentRowColumnText[AssociatedTimeCol[columnNumber]]));
    }

    /// <summary>
    ///   Advances the data reader to the next result, when reading the results of batch SQL statements.
    /// </summary>
    /// <returns>true if there are more rows; otherwise, false.</returns>
    public override bool NextResult() => false;

    public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => Task.FromResult(false);

    /// <summary>
    ///   Routine to open the reader, each implementation should call BeforeOpenAsync, InitColumns,
    ///   ParseColumnName and last FinishOpen;
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [UsedImplicitly]
    public abstract Task OpenAsync(CancellationToken token);

    /// <summary>
    ///   Overrides the column format from setting.
    /// </summary>
    [UsedImplicitly]
    public virtual bool Read(CancellationToken token) => ReadAsync(token).Wait(2000);

    public override bool Read() => ReadAsync(CancellationToken.None).Wait(2000);

    /// <summary>
    ///   Resets the position to first data row.
    /// </summary>
    public virtual void ResetPositionToFirstDataRow()
    {
      EndLineNumber = 0;
      RecordNumber = 0;
      EndOfFile = false;
    }

    /// <summary>
    ///   Handles input replacements like NBSP and NULL
    /// </summary>
    /// <param name="inputString">The input string.</param>
    /// <param name="treatNbspAsSpace">if set to <c>true</c> treat NBSP as space].</param>
    /// <param name="treatTextAsNull">The treat text as null.</param>
    /// <param name="trim">if set to <c>true</c> [trim].</param>
    /// <returns></returns>
    [CanBeNull]
    protected static string TreatNbspTestAsNullTrim([CanBeNull] string inputString, bool treatNbspAsSpace,
                                                    string treatTextAsNull, bool trim)
    {
      if (string.IsNullOrEmpty(inputString))
        return null;

      if (treatNbspAsSpace && inputString.IndexOf((char) 0xA0) != -1)
        inputString = inputString.Replace((char) 0xA0, ' ');

      if (trim)
        inputString = inputString.Trim();

      if (StringUtils.ShouldBeTreatedAsNull(inputString, treatTextAsNull))
        inputString = null;

      return inputString;
    }

    /// <summary>
    ///   Sets the Progress to marquee, calls OnOpen Event, check if the file does exist if its a
    ///   physical file
    /// </summary>
    protected async Task BeforeOpenAsync(string message)
    {
      SetMaxProcess?.Invoke(this, 0);
      HandleShowProgress(message);

      if (OnOpen != null)
        await OnOpen().ConfigureAwait(false);
    }

    /// <summary>
    ///   Does set EndOfFile sets the max value for Progress and stores the now know reader columns
    /// </summary>
    protected void FinishOpen()
    {
      m_IsFinished = false;
      EndOfFile = false;

      OpenFinished?.Invoke(this, Column);
      SetMaxProcess?.Invoke(this, c_MaxValue);
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
      IColumn column,
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
    /// <summary>
    ///   Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and MaxValue</returns>
    protected abstract double GetRelativePosition();

    /// <summary>
    ///   Gets the associated value.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    protected string GetTimeValue(int i)
    {
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
    [NotNull]
    protected object GetTypedValueFromString(string value, string timeValue, [NotNull] IColumn column)
    {
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
          ret = GetBooleanNull(value, column);
          break;

        case DataType.Guid:
          ret = GetGuidNull(value, column.ColumnOrdinal);
          break;

        case DataType.String:
        case DataType.TextToHtml:
        case DataType.TextToHtmlFull:
        case DataType.TextPart:
          /* TextToHTML and TextToHTMLFull and TextPart have been handled in the CSV Reader as the length of the fields would change */
          ret = value;
          break;

        default:
          throw new ArgumentOutOfRangeException();
      }

      return ret ?? DBNull.Value;
    }

    protected WarningEventArgs GetWarningEventArgs(int columnNumber, [NotNull] string message) => new WarningEventArgs(
                                                                          RecordNumber,
      columnNumber,
      message,
      StartLineNumber,
      EndLineNumber,
      Column != null && columnNumber >= 0 && columnNumber < m_FieldCount && Column[columnNumber] != null
        ? Column[columnNumber].Name
        : null);

    /// <summary>
    ///   Handles the Event if reading the file is completed
    /// </summary>
    protected void HandleReadFinished()
    {
      if (m_IsFinished)
        return;
      m_IsFinished = true;
      HandleShowProgress("Finished Reading from source", RecordNumber, c_MaxValue);
      ReadFinished?.Invoke(this, null);
    }

    /// <summary>
    ///   Shows the process.
    /// </summary>
    /// <param name="text">Leading Text</param>
    /// <param name="recordNumber">The record number.</param>
    /// <param name="progress">The progress (a value between 0 and MaxValue)</param>
    protected virtual void HandleShowProgress(string text, long recordNumber, double progress)
    {
      var rec = recordNumber > 1 ? $"\nRecord {recordNumber:N0}" : string.Empty;
      ReportProgress?.Invoke(this,
        new ProgressEventArgs($"{text}{rec}", (progress * c_MaxValue).ToInt64()));
    }

    /// <summary>
    ///   Shows the process.
    /// </summary>
    /// <param name="text">The text.</param>
    protected void HandleShowProgress(string text) => ReportProgress?.Invoke(this, new ProgressEventArgs(text));

    /// <summary>
    ///   Shows the process twice a second
    /// </summary>
    /// <param name="text">Leading Text</param>
    /// <param name="recordNumber">The record number.</param>
    protected void HandleShowProgressPeriodic(string text, long recordNumber)
    {
      if (ReportProgress != null)
        m_IntervalAction.Invoke(() => HandleShowProgress(text, recordNumber, GetRelativePosition()));
    }

    /// <summary>
    ///   Does handle TextToHML, TextToHtmlFull, TextPart and TreatNBSPAsSpace and does update the
    ///   maximum column size
    ///   Attention: Trimming needs to be handled before hand
    /// </summary>
    /// <param name="inputString">The input string.</param>
    /// <param name="columnNumber">The column number</param>
    /// <returns>The proper encoded or cut text as returned for the column</returns>
    [CanBeNull]
    protected string HandleTextSpecials([NotNull] string inputString, int columnNumber)
    {
      if (!string.IsNullOrEmpty(inputString) && columnNumber < FieldCount)
      {
        var column = GetColumn(columnNumber);

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        if (column.ValueFormat.DataType == DataType.TextToHtml)
        {
          var output = HTMLStyle.TextToHtmlEncode(inputString);
          if (!inputString.Equals(output, StringComparison.Ordinal))
            HandleWarning(columnNumber, $"HTML encoding removed from {inputString}");
          return output;
        }
        else if (column.ValueFormat.DataType == DataType.TextToHtmlFull)
        {
          var output = HTMLStyle.HtmlEncodeShort(inputString);
          if (!inputString.Equals(output, StringComparison.Ordinal))
            HandleWarning(columnNumber, $"HTML encoding removed from {inputString}");
          return output;
        }
        else if (column.ValueFormat.DataType == DataType.TextPart)
        {
          var output =
            StringConversion.StringToTextPart(inputString, column.PartSplitter, column.Part, column.PartToEnd);
          if (output == null)
            HandleWarning(columnNumber, $"Part {column.Part} of text {inputString} is empty.");
          return output;
        }
      }

      return inputString;
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

    protected virtual void InitColumn(int fieldCount)
    {
      m_FieldCount = fieldCount;
      CurrentRowColumnText = new string[fieldCount];

      Column = new ImmutableColumn[fieldCount];
      AssociatedTimeCol = new int[fieldCount];
      m_AssociatedTimeZoneCol = new int[fieldCount];
      for (var counter = 0; counter < fieldCount; counter++)
      {
        Column[counter] = new ImmutableColumn(GetDefaultName(counter, m_ColumnDefinition), new ImmutableValueFormat(),
          counter);
        AssociatedTimeCol[counter] = -1;
        m_AssociatedTimeZoneCol[counter] = -1;
      }
    }

    /// <summary>
    ///   Parses the name of the column by looking at the header row
    /// </summary>
    /// <param name="headerRow">The header row.</param>
    /// <param name="dataType">Type of the data.</param>
    /// <param name="hasFieldHeader">if set to <c>true</c> [has field header].</param>
    protected void ParseColumnName([NotNull] IEnumerable<string> headerRow,
                                   [CanBeNull] IEnumerable<DataType> dataType = null, bool hasFieldHeader = true)
    {
      var issues = new ColumnErrorDictionary();
      var adjusted = (hasFieldHeader
                       ? AdjustColumnName(headerRow, Column.Length, issues, m_ColumnDefinition).Item1
                       : Column.Select(x => x.Name)).ToList();
      var dataTypeL = (dataType == null) ? new List<DataType>(adjusted.Count) : dataType.ToList();

      if (adjusted.Count != dataTypeL.Count())
        throw new ApplicationException("Number of Columns and Types must match");
      for (var colIndex = 0; colIndex<adjusted.Count; colIndex++)
      {
        var name = adjusted[colIndex];
        if (name != null)
        {
          var setting = m_ColumnDefinition.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
          if (setting != null)
            Column[colIndex] = setting;
          else
            Column[colIndex] = new ImmutableColumn(name, new ImmutableValueFormat(dataTypeL[colIndex]), colIndex);
        }
      }

      // Initialize the references for TimePart and TimeZone
      for (var index = 0; index < Column.Length; index++)
      {
        // if the original column that reference other columns is ignored, skip it
        if (Column[index].Ignore) continue;

        var searchedTimePart = Column[index].TimePart;
        var searchedTimeZonePart = Column[index].TimeZonePart;

        if (!string.IsNullOrEmpty(searchedTimePart))
          for (var indexPoint = 0; indexPoint < Column.Length; indexPoint++)
          {
            if (indexPoint == index) continue;
            if (!Column[indexPoint].Name.Equals(searchedTimePart, StringComparison.OrdinalIgnoreCase)) continue;
            AssociatedTimeCol[index] = indexPoint;
            break;
          }

        if (string.IsNullOrEmpty(searchedTimeZonePart))
          continue;
        for (var indexPoint = 0; indexPoint < Column.Length; indexPoint++)
        {
          if (indexPoint == index) continue;

          if (!Column[indexPoint].Name.Equals(searchedTimeZonePart, StringComparison.OrdinalIgnoreCase)) continue;
          m_AssociatedTimeZoneCol[index] = indexPoint;
          break;
        }
      }

      // Now can handle possible warning that have been raised adjusting the names
      foreach (var warning in issues.Where(warning => !Column[warning.Key].Ignore))
        HandleWarning(warning.Key, warning.Value);
    }

    protected void SetProgressActions([CanBeNull] IProcessDisplay processDisplay)
    {
      if (processDisplay == null) return;
      ReportProgress = processDisplay.SetProcess;
      if (!(processDisplay is IProcessDisplayTime processDisplayTime)) return;
      SetMaxProcess = (sender, l) => processDisplayTime.Maximum = l;
      SetMaxProcess(this, 0);
    }

    protected bool ShouldRetry(Exception ex, CancellationToken token)
    {
      if (token.IsCancellationRequested) return false;

      var eventArgs = new RetryEventArgs(ex) { Retry = false };
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

    private static string GetDefaultName(int i, IEnumerable<IColumn> columnDefinitions = null)
    {
      var cd = columnDefinitions?.FirstOrDefault(x => x.ColumnOrdinal == i && !string.IsNullOrEmpty(x.Name));
      if (cd != null)
        return cd.Name;
      return $"Column{i + 1}";
    }

    private DateTime? AdjustTz(DateTime? input, IColumn column)
    {
      if (!input.HasValue)
        return null;
      string timeZone = null;

      if (m_AssociatedTimeZoneCol[column.ColumnOrdinal] > -1)
      {
        timeZone = GetString(m_AssociatedTimeZoneCol[column.ColumnOrdinal]);
      }
      else
      {
        var res = column.TimeZonePart.GetPossiblyConstant();
        // Constant value
        if (res.Item2)
          timeZone = res.Item1;
      }

      return FunctionalDI.AdjustTZImport(input, timeZone, column.ColumnOrdinal, HandleWarning);
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

    private bool? GetBooleanNull(string inputValue, [NotNull] IColumn column)
    {
      var boolValue = StringConversion.StringToBoolean(
        inputValue,
        column.ValueFormat.True,
        column.ValueFormat.False);
      if (boolValue.HasValue)
        return boolValue.Value;

      HandleError(column.ColumnOrdinal, $"'{inputValue}' is not a boolean value");
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
    private decimal? GetDecimalNull(string inputValue, IColumn column)
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
    private double? GetDoubleNull(string inputValue, IColumn column)
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
        HandleError(columnNumber, $"'{inputValue}' is not a GUID");
        return null;
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