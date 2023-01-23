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
  /// <inheritdoc />
  /// <summary>
  ///   Abstract class as base for all DataReaders
  /// </summary>
  public abstract class BaseFileReader : DbDataReader
  {
    /// <summary>
    ///   The maximum value
    /// </summary>
    public const int cMaxProgress = 10000;

    private readonly IReadOnlyCollection<Column> m_ColumnDefinition;

    private readonly IntervalAction m_IntervalAction = new IntervalAction();

    protected long RecordLimit;

    private IProgress<ProgressInfo>? m_ReportProgress;

    public IProgress<ProgressInfo> ReportProgress
    {
      set
      {
        value.SetMaximum(cMaxProgress);
        m_ReportProgress = value;
      }
    }

    /// <summary>
    ///   An array of associated col
    /// </summary>
    protected int[] AssociatedTimeCol = Array.Empty<int>();

    /// <summary>
    ///   An array of column
    /// </summary>
    protected Column[] Column = Array.Empty<Column>();

    /// <summary>
    ///   An array of current row column text
    /// </summary>
    protected string[] CurrentRowColumnText = Array.Empty<string>();

    /// <summary>
    ///   An array of associated col
    /// </summary>
    private int[] m_AssociatedTimeZoneCol = Array.Empty<int>();

    /// <summary>
    ///   Number of Columns in the reader
    /// </summary>
    private int m_FieldCount;

    /// <summary>
    ///   used to avoid reporting a fished execution twice it might be called on error before being
    ///   called once execution is done
    /// </summary>
    private bool m_IsFinished;

    protected readonly string DestTimeZone;
    protected bool SelfOpenedStream;
    protected readonly TimeZoneChangeDelegate TimeZoneAdjust;

    /// <inheritdoc />
    /// <summary>
    ///   Constructor for abstract base call for <see cref="T:CsvTools.IFileReader" />
    /// </summary>
    /// <param name="fileName">Path to a physical file (if used)</param>
    /// <param name="columnDefinition">List of column definitions</param>
    /// <param name="recordLimit">Number of records that should be read</param>
    /// <param name="timeZoneAdjust">Class to modify date time for timezones</param>
    /// <param name="destTimeZone">
    ///   Name of the time zone datetime values that have a source time zone should be converted to
    /// </param>
    protected BaseFileReader(in string fileName,
      in IEnumerable<Column>? columnDefinition,
      long recordLimit,
      in TimeZoneChangeDelegate timeZoneAdjust,
      in string destTimeZone)
    {
      TimeZoneAdjust = timeZoneAdjust;
      DestTimeZone =destTimeZone;
      m_ColumnDefinition = columnDefinition == null ? Array.Empty<Column>() : new List<Column>(columnDefinition).ToArray();
      RecordLimit = recordLimit < 1 ? long.MaxValue : recordLimit;
      FullPath = fileName;
      SelfOpenedStream = !string.IsNullOrWhiteSpace(fileName);
      FileName = FileSystemUtils.GetFileName(fileName);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    /// <summary>
    ///   Gets the number of fields in the file.
    /// </summary>
    /// <value>Number of field in the file.</value>
    public override int FieldCount => m_FieldCount;

    public override bool HasRows => !EndOfFile;

    public Func<Task>? OnOpenAsync { private get; set; }

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

    /// <inheritdoc />
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

    /// <inheritdoc />
    /// <summary>
    ///   Gets the <see cref="T:System.Object" /> with the specified name.
    /// </summary>
    /// <value></value>
    public override object this[string columnName] => GetValue(GetOrdinal(columnName));

    /// <inheritdoc />
    /// <summary>
    ///   Gets the <see cref="T:System.Object" /> with the specified column.
    /// </summary>
    /// <value></value>
    public override object this[int ordinal] => GetValue(ordinal);

    /// <summary>
    ///   Event to be raised if reading the files is completed
    /// </summary>
    public event EventHandler? ReadFinished;

    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    public event EventHandler<WarningEventArgs>? Warning;

    /// <summary>
    ///   Does look at the provided column names, and checks them for valid entry, makes sure the
    ///   column names are unique and not empty, have the right size etc.
    /// </summary>
    /// <param name="columns">The columns as read / provided</param>
    /// <param name="fieldCount">
    ///   The maximum number of fields, if more than this number are provided, it will ignore these columns
    /// </param>
    /// <param name="warnings">A <see cref="ColumnErrorDictionary" /> to store possible warnings</param>
    /// <returns></returns>
    internal static Tuple<IReadOnlyCollection<string>, int> AdjustColumnName(
      in IEnumerable<string> columns,
      int fieldCount,
      in ColumnErrorDictionary? warnings)
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
            resultingName = GetDefaultName(counter);
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

            var newName = previousColumns.MakeUniqueInCollection(resultingName);
            if (newName != resultingName)
            {
              warnings?.Add(counter, $"Column title '{resultingName}' exists more than once replaced with {newName}");
              issuesCounter++;
              resultingName = newName;
            }
          }

          previousColumns.Add(resultingName);
        }
      }

      return new Tuple<IReadOnlyCollection<string>, int>(previousColumns, issuesCounter);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Closes the <see cref="T:System.Data.IDataReader" /> Object.
    /// </summary>
    public override void Close()
    {
      EndOfFile = true;
      base.Close();
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    public new virtual async Task CloseAsync() => await Task.Run(() => base.Close()).ConfigureAwait(false);
#endif

    /// <inheritdoc />
    /// <summary>
    ///   Gets the boolean.
    /// </summary>
    /// <param name="ordinal">The i.</param>
    /// <returns></returns>
    public override bool GetBoolean(int ordinal)
    {
      var parsed = GetBooleanNull(CurrentRowColumnText[ordinal], ordinal);
      if (parsed.HasValue) return parsed.Value;

      // Warning was added by GetBooleanNull
      throw WarnAddFormatException(
        ordinal,
        $"'{CurrentRowColumnText[ordinal]}' is not a boolean ({GetColumn(ordinal).ValueFormat.True}/{GetColumn(ordinal).ValueFormat.False})");
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets the 8-bit unsigned integer value of the specified column.
    /// </summary>
    /// <param name="ordinal">The zero-based column ordinal.</param>
    /// <returns>The 8-bit unsigned integer value of the specified column.</returns>
    /// <exception cref="T:System.IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see
    ///   cref="P:System.Data.IDataRecord.FieldCount" />.
    /// </exception>
    public override byte GetByte(int ordinal)
    {
      try
      {
        return byte.Parse(CurrentRowColumnText[ordinal], CultureInfo.InvariantCulture);
      }
      catch (Exception)
      {
        throw WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not byte");
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Reads a stream of bytes from the file specified by the value in the specified column into the buffer as an array,
    ///   starting at the given buffer offset.
    /// </summary>
    /// <param name="ordinal">The zero-based column ordinal containing the filename.</param>
    /// <param name="dataOffset">The index within the field from which to start the read operation.</param>
    /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
    /// <param name="bufferOffset">
    ///   The index for <paramref name="buffer" /> to start the read operation.
    /// </param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>The actual number of bytes read.</returns>
    /// <exception cref="T:System.NotImplementedException"></exception>    
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {      
      if (buffer == null) throw new ArgumentNullException(nameof(buffer));
      var fn = GetString(ordinal);
      if (GetColumn(ordinal).ValueFormat.DataType != DataTypeEnum.Binary || string.IsNullOrEmpty(fn))
        return -1;

      using var fileStream = FileSystemUtils.OpenRead(fn);
      if (dataOffset > 0)
        fileStream.Seek(dataOffset, SeekOrigin.Begin);

      return fileStream.Read(buffer, bufferOffset, length);
    }

    /// <inheritdoc cref="IFileReader" />
    public virtual byte[] GetFile(int ordinal)
    {
      var fn = GetString(ordinal);
      if (GetColumn(ordinal).ValueFormat.DataType != DataTypeEnum.Binary || string.IsNullOrEmpty(fn))
        return Array.Empty<byte>();

      var fi = new FileSystemUtils.FileInfo(fn);
      var buffer = new byte[fi.Length];
      GetBytes(ordinal, 0, buffer, 0, buffer.Length);
      return buffer;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets the character value of the specified column.
    /// </summary>
    /// <param name="ordinal">The zero-based column ordinal.</param>
    /// <returns>The character value of the specified column.</returns>
    /// <exception cref="T:System.IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see
    ///   cref="P:System.Data.IDataRecord.FieldCount" />.
    /// </exception>
    public override char GetChar(int ordinal) => GetString(ordinal)[0];

    /// <inheritdoc />
    /// <summary>
    ///   Reads a stream of characters from the specified column offset into the buffer as an array,
    ///   starting at the given buffer offset.
    /// </summary>
    /// <param name="ordinal">The zero-based column ordinal.</param>
    /// <param name="dataOffset">The index within the row from which to start the read operation.</param>
    /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
    /// <param name="bufferOffset">
    ///   The index for <paramref name="buffer" /> to start the read operation.
    /// </param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>The actual number of characters read.</returns>
    /// <exception cref="T:System.IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see
    ///   cref="P:System.Data.IDataRecord.FieldCount" />.
    /// </exception>
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
      if (buffer == null) throw new ArgumentNullException(nameof(buffer));
      var offset = (int) dataOffset;
      var maxLen = CurrentRowColumnText[ordinal].Length - offset;

      if (maxLen > length) maxLen = length;

      CurrentRowColumnText[ordinal].CopyTo(
        offset,
        buffer ?? throw new ArgumentNullException(nameof(buffer)),
        bufferOffset,
        maxLen);
      return maxLen;
    }

    /// <summary>
    ///   Gets the column format.
    /// </summary>
    /// <param name="ordinal">The column.</param>
    /// <returns></returns>
    public virtual Column GetColumn(int ordinal) => Column[ordinal];

    /// <inheritdoc />
    /// <summary>
    ///   Gets the data type information for the specified field.
    /// </summary>
    /// <param name="ordinal">The index of the field to find.</param>
    /// <returns>The data type information for the specified field.</returns>
    /// <exception cref="T:System.NotImplementedException"></exception>
    public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

    /// <inheritdoc />
    /// <summary>
    ///   Gets the date and time data value of the specified field.
    /// </summary>
    /// <param name="ordinal">The index of the field to find.</param>
    /// <returns>The date and time data value of the specified field.</returns>
    public override DateTime GetDateTime(int ordinal)
    {
      var dt = GetDateTimeNull(
        null,
        CurrentRowColumnText[ordinal],
        null,
        GetTimeValue(ordinal),
        Column[ordinal],
        true);
      if (dt.HasValue) return dt.Value;

      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(
        ordinal,
        $"'{CurrentRowColumnText[ordinal]}' is not a date of the format '{GetColumn(ordinal).ValueFormat.DateFormat}'");
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets the decimal.
    /// </summary>
    /// <param name="ordinal">The i.</param>
    /// <returns></returns>
    public override decimal GetDecimal(int ordinal)
    {
      var decimalValue = GetDecimalNull(CurrentRowColumnText[ordinal], ordinal);
      if (decimalValue.HasValue) return decimalValue.Value;

      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not a decimal");
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets the double.
    /// </summary>
    /// <param name="ordinal">The i.</param>
    /// <returns></returns>
    public override double GetDouble(int ordinal)
    {
      var decimalValue = GetDecimalNull(CurrentRowColumnText[ordinal], ordinal);
      if (decimalValue.HasValue) return Convert.ToDouble(decimalValue.Value);

      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not a double");
    }

    public override IEnumerator GetEnumerator() => new DbEnumerator(this, true);

    /// <inheritdoc />
    /// <summary>
    ///   Gets the type of the field.
    /// </summary>
    /// <param name="ordinal">The column number.</param>
    /// <returns>The .NET type of the column</returns>
    public override Type GetFieldType(int ordinal) => GetColumn(ordinal).ValueFormat.DataType.GetNetType();

    /// <inheritdoc />
    /// <summary>
    ///   Gets the single-precision floating point number of the specified field.
    /// </summary>
    /// <param name="ordinal">The index of the field to find.</param>
    /// <returns>The single-precision floating point number of the specified field.</returns>
    public override float GetFloat(int ordinal)
    {
      var decimalValue = GetDecimalNull(CurrentRowColumnText[ordinal], ordinal);
      if (decimalValue.HasValue) return Convert.ToSingle(decimalValue, CultureInfo.InvariantCulture);

      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not a float");
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets the unique identifier.
    /// </summary>
    /// <param name="ordinal">The column number.</param>
    /// <returns></returns>
    public override Guid GetGuid(int ordinal)
    {
      var parsed = GetGuidNull(CurrentRowColumnText[ordinal], ordinal);

      if (parsed.HasValue) return parsed.Value;

      throw WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not an GUID");
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets the 16-bit signed integer value of the specified field.
    /// </summary>
    /// <param name="ordinal">The index of the field to find.</param>
    /// <returns>The 16-bit signed integer value of the specified field.</returns>
    public override short GetInt16(int ordinal) => GetInt16(CurrentRowColumnText[ordinal], ordinal);

    /// <inheritdoc />
    /// <summary>
    ///   Gets the int32.
    /// </summary>
    /// <param name="ordinal">The i.</param>
    /// <returns></returns>
    public override int GetInt32(int ordinal)
    {
      var column = GetColumn(ordinal);

      var parsed = StringConversion.StringToInt32(
        CurrentRowColumnText[ordinal],
        column.ValueFormat.DecimalSeparator,
        column.ValueFormat.GroupSeparator);
      if (parsed.HasValue) return parsed.Value;

      // Warning was added by GetInt32Null
      throw WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not an integer");
    }

    /// <summary>
    ///   Gets the int32 value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="column">The column.</param>
    /// <returns>a nullable integer</returns>
    public int? GetInt32Null(in string inputValue, in Column column)
    {
      var ret = StringConversion.StringToInt32(
        inputValue,
        column.ValueFormat.DecimalSeparator,
        column.ValueFormat.GroupSeparator);
      if (ret.HasValue) return ret.Value;

      HandleError(column.ColumnOrdinal, $"'{inputValue}' is not an integer");
      return null;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets the int64.
    /// </summary>
    /// <param name="ordinal">The i.</param>
    /// <returns></returns>
    public override long GetInt64(int ordinal)
    {
      var column = GetColumn(ordinal);

      var parsed = StringConversion.StringToInt64(
        CurrentRowColumnText[ordinal],
        column.ValueFormat.DecimalSeparator,
        column.ValueFormat.GroupSeparator);
      if (parsed.HasValue) return parsed.Value;

      throw WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not a long integer");
    }

    /// <summary>
    ///   Gets the int32 value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="column">The column.</param>
    /// <returns></returns>
    public long? GetInt64Null(in string inputValue, in Column column)
    {
      var ret = StringConversion.StringToInt64(
        inputValue,
        column.ValueFormat.DecimalSeparator,
        column.ValueFormat.GroupSeparator);
      if (ret.HasValue) return ret.Value;

      HandleError(column.ColumnOrdinal, $"'{inputValue}' is not an long integer");
      return null;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets the name for the field to find.
    /// </summary>
    /// <param name="ordinal">The index of the field to find.</param>
    /// <returns>The name of the field or the empty string (""), if there is no value to return.</returns>
    /// <exception cref="T:System.IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see
    ///   cref="P:System.Data.IDataRecord.FieldCount" />.
    /// </exception>
    public override string GetName(int ordinal) => GetColumn(ordinal).Name;

    /// <inheritdoc />
    /// <summary>
    ///   Return the index of the named field.
    /// </summary>
    /// <param name="name">The name of the field to find.</param>
    /// <returns>The index of the named field. If not found -1</returns>
    public override int GetOrdinal(string name)
    {
      if (string.IsNullOrEmpty(name)) return -1;

      var count = 0;
      foreach (var column in Column)
      {
        if (name.Equals(column.Name, StringComparison.OrdinalIgnoreCase)) return count;

        count++;
      }

      return -1;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Returns a <see cref="T:System.Data.DataTable" /> that describes the column meta data of
    ///   the <see cref="T:System.Data.IDataReader" /> .
    /// </summary>
    /// <returns>A <see cref="T:System.Data.DataTable" /> that describes the column meta data.</returns>
    /// <exception cref="T:System.InvalidOperationException">
    ///   The <see cref="T:System.Data.IDataReader" /> is closed.
    /// </exception>
    public override DataTable GetSchemaTable()
    {
      var dataTable = ReaderConstants.GetEmptySchemaTable();
      var schemaRow = ReaderConstants.GetDefaultSchemaRowArray();

      for (var col = 0; col < FieldCount; col++)
      {
        var column = GetColumn(col);
        
        schemaRow[1] = column.Name; // BaseColumnName
        schemaRow[4] = column.Name; // ColumnName
        schemaRow[5] = col; // ColumnOrdinal
        schemaRow[19] = column.Ignore;  // IsHidden        

        // If there is a conversion get the information
        if (column.Convert && column.ValueFormat.DataType != DataTypeEnum.String)
          schemaRow[7] = column.ValueFormat.DataType.GetNetType();
        else
          schemaRow[7] = typeof(string);

        dataTable.Rows.Add(schemaRow);
      }

      return dataTable;
    }

    public override Stream GetStream(int ordinal) =>
      new MemoryStream(Encoding.UTF8.GetBytes(CurrentRowColumnText[ordinal]));

    /// <inheritdoc />
    /// <summary>
    ///   Gets the originally provided text in a column
    /// </summary>
    /// <param name="ordinal">The column number.</param>
    /// <returns></returns>
    /// <exception cref="T:System.InvalidOperationException">Row has not been read</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">ordinal invalid</exception>
    public override string GetString(int ordinal)
    {
      if (CurrentRowColumnText is null) throw new InvalidOperationException("Row has not been read");

      if (ordinal < 0 || ordinal >= FieldCount || ordinal >= CurrentRowColumnText.Length)
        throw new ArgumentOutOfRangeException(nameof(ordinal));

      return CurrentRowColumnText[ordinal];
    }

    public override TextReader GetTextReader(int ordinal) => new StringReader(CurrentRowColumnText[ordinal]);

    /// <inheritdoc />
    /// <summary>
    ///   Gets the value of a column, any integer will be returned a long integer no matter if 32 or
    ///   64 bit
    /// </summary>
    /// <param name="ordinal">The column number.</param>
    /// <returns>The value of the specific field</returns>
    public override object GetValue(int ordinal)
    {
      if (IsDBNull(ordinal))
        return DBNull.Value;
      var column = GetColumn(ordinal);

      object ret;
      try
      {
        ret = column.ValueFormat.DataType switch
        {
          DataTypeEnum.Binary => GetFile(ordinal),
          DataTypeEnum.DateTime => GetDateTime(ordinal),
          DataTypeEnum.Integer => GetInt64(ordinal),
          DataTypeEnum.Double => GetDouble(ordinal),
          DataTypeEnum.Numeric => GetDecimal(ordinal),
          DataTypeEnum.Boolean => GetBoolean(ordinal),
          DataTypeEnum.Guid => GetGuid(ordinal),
          _ => GetString(ordinal)
        };
      }
      catch (FormatException)
      {
        return DBNull.Value;
      }

      return ret;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets all the attribute fields in the collection for the current record.
    /// </summary>
    /// <param name="values">An array of object to copy the attribute fields into.</param>
    /// <returns>The number of instances of object in the array.</returns>
    public override int GetValues(object[] values)
    {
      if (values is null) throw new ArgumentNullException(nameof(values));

      var maxFld = values.Length;
      if (maxFld > FieldCount) maxFld = FieldCount;

      for (var col = 0; col < maxFld; col++)
        values[col] = GetValue(col);

      return FieldCount;
    }

    /// <summary>
    ///   Handles the error.
    /// </summary>
    /// <param name="ordinal">The column number.</param>
    /// <param name="message">The message.</param>
    public void HandleError(int ordinal, string message) =>
      Warning?.Invoke(this, GetWarningEventArgs(ordinal, message));

    /// <summary>
    ///   Calls the event handler for warnings
    /// </summary>
    /// <param name="ordinal">The column.</param>
    /// <param name="message">The message.</param>
    public void HandleWarning(int ordinal, string message) =>
      Warning?.Invoke(this, GetWarningEventArgs(ordinal, message.AddWarningId()));

    /// <inheritdoc />
    /// <summary>
    ///   Return whether the specified field is set to null.
    /// </summary>
    /// <param name="ordinal">The index of the field to find.</param>
    /// <returns>true if the specified field is set to null; otherwise, false.</returns>
    /// <exception cref="T:System.IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see
    ///   cref="P:System.Data.IDataRecord.FieldCount" />.
    /// </exception>
    public override bool IsDBNull(int ordinal)
    {
      if (CurrentRowColumnText.Length <= ordinal) return true;

      if (Column[ordinal].ValueFormat.DataType != DataTypeEnum.DateTime)
        return string.IsNullOrWhiteSpace(CurrentRowColumnText[ordinal]);

      if (AssociatedTimeCol[ordinal] == -1 || AssociatedTimeCol[ordinal] >= CurrentRowColumnText.Length)
        return string.IsNullOrEmpty(CurrentRowColumnText[ordinal]);

      return string.IsNullOrEmpty(CurrentRowColumnText[ordinal])
             && string.IsNullOrEmpty(CurrentRowColumnText[AssociatedTimeCol[ordinal]]);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Advances the data reader to the next result, when reading the results of batch SQL statements.
    /// </summary>
    /// <returns>true if there are more rows; otherwise, false.</returns>
    public override bool NextResult() => false;

    public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => Task.FromResult(false);

    /// <summary>
    ///   Routine to open the reader, each implementation should call BeforeOpenAsync, InitColumns,
    ///   ParseColumnName and last FinishOpen
    /// </summary>
    /// ///
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public abstract Task OpenAsync(CancellationToken cancellationToken);

    /// <summary>
    ///   Overrides the column format from setting.
    /// </summary>
    /// <returns>true if read was successful</returns>
    /// ///
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    public virtual bool Read(in CancellationToken cancellationToken) =>
      ReadAsync(cancellationToken).GetAwaiter().GetResult();

    public override bool Read() => ReadAsync(CancellationToken.None).GetAwaiter().GetResult();

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
    protected static string TreatNbspAsNullTrim(
      string inputString,
      bool treatNbspAsSpace,
      in string treatTextAsNull,
      bool trim)
    {
      if (inputString.Length == 0)
        return string.Empty;

      if (treatNbspAsSpace && inputString.IndexOf((char) 0xA0) != -1)
        inputString = inputString.Replace((char) 0xA0, ' ');

      if (trim)
        inputString = inputString.Trim();

      return StringUtils.ShouldBeTreatedAsNull(inputString, treatTextAsNull) ? string.Empty : inputString;
    }

    /// <summary>
    ///   Sets the Progress to marquee, calls OnOpen Event, check if the file does exist if its a
    ///   physical file
    /// </summary>
    protected async Task BeforeOpenAsync(string message)
    {
      HandleShowProgress(message, 0);
      if (OnOpenAsync != null)
        await OnOpenAsync.Invoke().ConfigureAwait(false);
    }

    /// <summary>
    ///   Does set EndOfFile sets the max value for Progress and stores the now know reader columns
    /// </summary>
    protected void FinishOpen()
    {
      m_IsFinished = false;
      EndOfFile = false;
      OpenFinished?.Invoke(this, Column);
    }

    /// <summary>
    ///   Gets the boolean value.
    /// </summary>
    /// <param name="inputBoolean">The input.</param>
    /// <param name="ordinal">The column.</param>
    /// <returns>
    ///   The Boolean, if conversion is not successful: <c>NULL</c> the event handler for warnings
    ///   is called
    /// </returns>
    protected bool? GetBooleanNull(in string inputBoolean, int ordinal) =>
      GetBooleanNull(inputBoolean, GetColumn(ordinal));

    /// <summary>
    ///   This routine will read a date from a typed or untyped reader, will combined date with time
    ///   and apply timeZone adjustments.
    /// </summary>
    /// <param name="inputDate">The original object possibly containing a date</param>
    /// <param name="strInputDate">The text representation of the data</param>
    /// <param name="inputTime">The original object possibly containing a time</param>
    /// <param name="strInputTime">The text representation of the time</param>
    /// <param name="column">Column information</param>
    /// <param name="serialDateTime">if <c>true</c> parse dates represented as numbers</param>
    /// <returns></returns>
    protected DateTime? GetDateTimeNull(
      in object? inputDate,
      in string strInputDate,
      in object? inputTime,
      in string strInputTime,
      in Column column,
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
        if (inputTime != null) passedIn = Convert.ToString(inputTime);

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
            strInputTime.Length > 0
              ? $"'{strInputDate} {strInputTime}' is not a date of the format '{display}' '{column.TimePartFormat}', used '{inputDateNew} {strInputTime}'"
              : $"'{strInputDate}' is not a date of the format '{display}', used '{inputDateNew}' ");
        }
      }

      if (dateTime.HasValue && dateTime.Value.Year > 1752 && dateTime.Value.Year <= 9999)
        return AdjustTz(dateTime.Value, column);

      HandleDateError(strInputDate, strInputTime, column.ColumnOrdinal);
      return null;
    }

    /// <summary>
    ///   Gets the decimal value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="ordinal">The column.</param>
    /// <returns>
    ///   The decimal value if conversion is not successful: <c>NULL</c> the event handler for
    ///   warnings is called
    /// </returns>
    protected decimal? GetDecimalNull(in string inputValue, int ordinal)
    {
      var column = GetColumn(ordinal);
      var decimalValue = StringConversion.StringToDecimal(
        inputValue,
        column.ValueFormat.DecimalSeparator,
        column.ValueFormat.GroupSeparator,
        true);
      if (decimalValue.HasValue) return decimalValue.Value;

      HandleError(column.ColumnOrdinal, $"'{inputValue}' is not a decimal");
      return null;
    }

    /// <summary>
    ///   Gets the double value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="ordinal">The column.</param>
    /// <returns>
    ///   The parsed value if conversion is not successful: <c>NULL</c> is returned and the event
    ///   handler for warnings is called
    /// </returns>
    protected double? GetDoubleNull(in string inputValue, int ordinal)
    {
      var decimalValue = GetDecimalNull(inputValue, ordinal);
      if (decimalValue.HasValue) return decimal.ToDouble(decimalValue.Value);

      HandleError(ordinal, $"'{inputValue}' is not a double");
      return null;
    }

    /// <summary>
    ///   Gets the int32 value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="ordinal">The column number.</param>
    /// <returns></returns>
    protected Guid? GetGuidNull(in string inputValue, int ordinal)
    {
      if (string.IsNullOrEmpty(inputValue)) return null;

      try
      {
        return new Guid(inputValue);
      }
      catch
      {
        HandleError(ordinal, $"'{inputValue}' is not a GUID");
        return null;
      }
    }

    /// <summary>
    ///   Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and 1</returns>
    /// <summary>
    ///   Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and 1 for 0% to 100%</returns>
    protected virtual double GetRelativePosition()
    {
      if (RecordLimit > 0 && RecordLimit < long.MaxValue)
        return (double) RecordNumber / RecordLimit;
      return 0;
    }

    /// <summary>
    ///   Gets the associated value.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    protected virtual string GetTimeValue(int i) =>
      AssociatedTimeCol[i] == -1 || AssociatedTimeCol[i] >= CurrentRowColumnText.Length
        ? string.Empty
        : CurrentRowColumnText[AssociatedTimeCol[i]];

    protected WarningEventArgs GetWarningEventArgs(int ordinal, in string message) =>
      new WarningEventArgs(
        RecordNumber,
        ordinal,
        message,
        StartLineNumber,
        EndLineNumber,
        ordinal >= 0 && ordinal < m_FieldCount ? Column[ordinal].Name : null);

    /// <summary>
    ///   Handles the Event if reading the file is completed
    /// </summary>
    protected virtual void HandleReadFinished()
    {
      if (m_IsFinished) return;

      m_IsFinished = true;
      HandleShowProgress("Finished reading", 1);
      ReadFinished?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///   Shows the process.
    /// </summary>
    /// <param name="text">Leading Text</param>
    /// <param name="percent">Value between 0 and 1 representing the relative position</param>
    protected virtual void HandleShowProgress(in string text, double percent) => m_ReportProgress?.Report(new ProgressInfo(text, (percent * cMaxProgress).ToInt64()));


    /// <summary>
    ///   Shows the process twice a second
    /// </summary>
    /// <param name="text">Leading Text</param>
    protected virtual void HandleShowProgressPeriodic(string text)
        => m_IntervalAction.Invoke(() => HandleShowProgress(text, GetRelativePosition()));

    /// <summary>
    ///   Does handle TextToHML, TextToHtmlFull, TextPart and TreatNBSPAsSpace and does update the
    ///   maximum column size
    ///   Attention: Trimming needs to be handled before hand
    /// </summary>
    /// <param name="inputString">The input string.</param>
    /// <param name="ordinal">The column number</param>
    /// <returns>The proper encoded or cut text as returned for the column</returns>
    protected virtual string HandleTextSpecials(in string? inputString, int ordinal)
    {
      if (inputString is null || inputString.Length == 0 || ordinal >= FieldCount)
        return inputString ?? string.Empty;

      return Column[ordinal].ColumnFormatter
        ?.FormatInputText(inputString, message => HandleWarning(ordinal, message)) ?? inputString;
    }

    /// <summary>
    ///   Displays progress, is called after <see langword="abstract" /> row has been read
    /// </summary>
    /// <param name="hasReadRow"><c>true</c> if a row has been read</param>
    protected virtual void InfoDisplay(bool hasReadRow)
    {
      if (!hasReadRow)
        HandleReadFinished();
      else
        HandleShowProgressPeriodic($"Record {RecordNumber:N0}");
    }

    protected virtual void InitColumn(int fieldCount)
    {
      m_FieldCount = fieldCount;
      CurrentRowColumnText = new string[fieldCount];

      Column = new Column[fieldCount];
      AssociatedTimeCol = new int[fieldCount];
      m_AssociatedTimeZoneCol = new int[fieldCount];
      for (var counter = 0; counter < fieldCount; counter++)
      {
        Column[counter] = new Column(GetDefaultName(counter), ValueFormat.Empty, counter);
        AssociatedTimeCol[counter] = -1;
        m_AssociatedTimeZoneCol[counter] = -1;
      }
    }

    /// <summary>
    ///   Parses the name of the columns and sets the data types, it will handle TimePart and
    ///   TimeZone. Column must be set before hand
    /// </summary>
    /// <param name="headerRow">The header row.</param>
    /// <param name="dataType">Type of the data.</param>
    /// <param name="hasFieldHeader">if set to <c>true</c> [has field header].</param>
    protected virtual void ParseColumnName(
      in IEnumerable<string> headerRow,
      in IEnumerable<DataTypeEnum>? dataType = null,
      bool hasFieldHeader = true)
    {
      var issues = new ColumnErrorDictionary();
      var adjustedNames = new List<string>();
      if (hasFieldHeader)
        adjustedNames.AddRange(AdjustColumnName(headerRow, Column.Length, issues).Item1);
      else
        for (var colIndex = 0; colIndex < Column.Length; colIndex++)
        {
          if (Column[colIndex].Name.Equals(GetDefaultName(colIndex)))
          {
            // Might have passed in the column names in m_ColumnDefinition (used with Manifest data
            // accompanying a file without header)
            var newDef = m_ColumnDefinition.FirstOrDefault(x => x.ColumnOrdinal == colIndex);
            if (newDef != null && !string.IsNullOrEmpty(newDef.Name))
            {
              issues.Add(colIndex, "Using column name from definition");
              adjustedNames.Add(newDef.Name);
              continue;
            }
          }

          adjustedNames.Add(Column[colIndex].Name);
        }

      var dataTypeL = new DataTypeEnum[adjustedNames.Count];
      // Initialize as text
      for (var col = 0; col < adjustedNames.Count; col++) dataTypeL[col] = DataTypeEnum.String;
      // get the provided and overwrite
      if (dataType != null)
      {
        using var enumeratorType = dataType.GetEnumerator();
        var col = 0;
        while (enumeratorType.MoveNext() && col < adjustedNames.Count)
          dataTypeL[col++] = enumeratorType.Current;
      }

      // set the data types, either using the definition, or the provided DataType with defaults
      for (var colIndex = 0; colIndex < adjustedNames.Count && colIndex < Column.Length; colIndex++)
      {
        var defined =
          m_ColumnDefinition.FirstOrDefault(
            x => x.Name.Equals(adjustedNames[colIndex], StringComparison.OrdinalIgnoreCase)) ?? new Column(
            adjustedNames[colIndex],
            new ValueFormat(dataTypeL[colIndex]),
            colIndex);
        Column[colIndex] = new Column(
          adjustedNames[colIndex],
          defined.ValueFormat,
          colIndex,
          defined.Ignore,
          defined.Convert,
          defined.DestinationName,
          defined.TimePart, defined.TimePartFormat, defined.TimeZonePart);
      }

      if (Column.Length == 0)
        issues.Add(-1, "Column should be set before using ParseColumnName to handle TimePart and TimeZone");
      else
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

          if (string.IsNullOrEmpty(searchedTimeZonePart)) continue;

          for (var indexPoint = 0; indexPoint < Column.Length; indexPoint++)
          {
            if (indexPoint == index) continue;

            if (!Column[indexPoint].Name.Equals(searchedTimeZonePart, StringComparison.OrdinalIgnoreCase)) continue;
            m_AssociatedTimeZoneCol[index] = indexPoint;
            break;
          }
        }

      // Now can handle possible warning that have been raised adjusting the names
      foreach (var warning in issues.Where(
                 warning => warning.Key < 0 || warning.Key >= Column.Length || !Column[warning.Key].Ignore))
        HandleWarning(warning.Key, warning.Value);
    }

    /// <summary>
    ///   Occurs when something went wrong during opening of the setting, this might be the file
    ///   does not exist or a query ran into a timeout
    /// </summary>
    public event EventHandler<RetryEventArgs>? OnAskRetry;

    protected bool ShouldRetry(in Exception ex, in CancellationToken token)
    {
      if (token.IsCancellationRequested) return false;

      var eventArgs = new RetryEventArgs(ex) { Retry = false };
      OnAskRetry?.Invoke(this, eventArgs);
      return eventArgs.Retry;
    }

    /// <summary>
    ///   Adds a Format exception.
    /// </summary>
    /// <param name="ordinal">The column.</param>
    /// <param name="message">The message.</param>
    protected FormatException WarnAddFormatException(int ordinal, in string message)
    {
      HandleError(ordinal, message);
      return new FormatException(message);
    }

    /// <summary>
    ///   Get the default names, if columnDefinitions is provided try to find the name looking at
    ///   the ColumnOrdinal, otherwise is ColumnX (X being the column number +1)
    /// </summary>
    /// <param name="ordinal">The column number counting from 0</param>
    /// <returns>A string with the column name</returns>
    private static string GetDefaultName(int ordinal) => $"Column{ordinal + 1}";

    private DateTime AdjustTz(in DateTime input, Column column)
    {
      // get the time zone either from constant or from other column
      if (!column.TimeZonePart.TryGetConstant(out var timeZone) &&
          m_AssociatedTimeZoneCol.Length > column.ColumnOrdinal && m_AssociatedTimeZoneCol[column.ColumnOrdinal] > -1)
        timeZone = GetString(m_AssociatedTimeZoneCol[column.ColumnOrdinal]);

      return TimeZoneAdjust(input, timeZone, DestTimeZone,
        (message) => HandleWarning(column.ColumnOrdinal, message));
    }

    private bool? GetBooleanNull(in string inputValue, in Column column)
    {
      var boolValue = StringConversion.StringToBoolean(inputValue, column.ValueFormat.True, column.ValueFormat.False);
      if (boolValue.HasValue) return boolValue.Value;

      HandleError(column.ColumnOrdinal, $"'{inputValue}' is not a boolean");
      return null;
    }

    /// <summary>
    ///   Gets the integer value
    /// </summary>
    /// <param name="value">The input.</param>
    /// <param name="ordinal">The column number for retrieving the Format information.</param>
    /// <returns>
    ///   The parsed value if conversion is not successful: <c>NULL</c> is returned and the event
    ///   handler for warnings is called
    /// </returns>
    private short GetInt16(in string value, int ordinal)
    {
      Debug.Assert(ordinal >= 0 && ordinal < FieldCount);
      var column = GetColumn(ordinal);

      var parsed = StringConversion.StringToInt16(
        value,
        column.ValueFormat.DecimalSeparator,
        column.ValueFormat.GroupSeparator);
      if (parsed.HasValue) return parsed.Value;

      // Warning was added by GetInt32Null
      throw WarnAddFormatException(ordinal, $"'{value}' is not a short");
    }

    /// <summary>
    ///   Handles the waring for a date.
    /// </summary>
    /// <param name="inputDate">The input date.</param>
    /// <param name="inputTime">The input time.</param>
    /// <param name="ordinal">The column.</param>
    private void HandleDateError(in string inputDate, in string inputTime, int ordinal)
    {
      Debug.Assert(ordinal >= 0 && ordinal < FieldCount);
      var column = GetColumn(ordinal);
      if (column.Ignore)
        return;
      var display = column.ValueFormat.DateFormat.ReplaceDefaults(
        "/",
        column.ValueFormat.DateSeparator,
        ":",
        column.ValueFormat.TimeSeparator);

      HandleError(
        ordinal,
        !string.IsNullOrEmpty(inputTime)
          ? $"'{inputDate} {inputTime}' is not a date of the format {display} {column.TimePartFormat}"
          : $"'{inputDate}' is not a date of the format {display}");
    }

    public event EventHandler<IReadOnlyCollection<Column>>? OpenFinished;
  }
}