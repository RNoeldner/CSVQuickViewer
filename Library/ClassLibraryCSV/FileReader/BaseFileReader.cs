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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <inheritdoc cref="DbDataReader" />
  /// <summary>
  ///   Abstract class as base for all DataReaders
  /// </summary>
  public abstract class BaseFileReader : DbDataReader, IFileReader
  {
    /// <summary>
    ///   The maximum value
    /// </summary>
    public const int cMaxProgress = 10000;

    /// <summary>
    ///   An array of column
    /// </summary>
    public Column[] Column = [];
    private readonly Dictionary<string, int> m_ColumnIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The time zone to convert the read data to, assuming the source time zone is part of the data
    /// </summary>
    protected readonly string ReturnedTimeZone;

    /// <summary>
    /// The routine used for time zone adjustment
    /// </summary>
    protected readonly TimeZoneChangeDelegate TimeZoneAdjust;

    /// <summary>
    ///   An array of associated col
    /// </summary>
    protected int[] AssociatedTimeCol = [];

    /// <summary>
    ///   An array of current row column text, this is reused to avoid multiple allocations
    /// </summary>
    protected string[] CurrentRowColumnText = [];

    /// <summary>
    /// The record limit
    /// </summary>
    protected long RecordLimit;

    /// <summary>
    /// If the stream is opened by the reader this is true
    /// </summary>
    protected bool SelfOpenedStream;

    private readonly bool m_AllowPercentage;
    private readonly IReadOnlyCollection<Column> m_ColumnDefinition;
    private readonly IntervalAction m_IntervalAction = new IntervalAction();
    private readonly bool m_RemoveCurrency;
    /// <summary>
    ///   An array of associated col
    /// </summary>
    private int[] m_AssociatedTimeZoneCol = [];

    /// <summary>
    ///   Number of Columns in the reader
    /// </summary>
    private int m_FieldCount;

    /// <summary>
    ///   used to avoid reporting a fished execution twice it might be called on error before being
    ///   called once execution is done
    /// </summary>
    private bool m_IsFinished;

    // ReSharper disable once FieldCanBeMadeReadOnly.Global    
    private IProgress<ProgressInfo> m_ReportProgress = new DummyProgress();

    /// <inheritdoc />
    /// <summary>
    ///   Constructor for abstract base call for <see cref="T:CsvTools.IFileReader" />
    /// </summary>
    /// <param name="fileName">Path to a physical file (if used)</param>
    /// <param name="columnDefinition">List of column definitions</param>
    /// <param name="recordLimit">Number of records that should be read</param>
    /// <param name="timeZoneAdjust">Class to modify date time for time zones</param>
    /// <param name="returnedTimeZone">
    ///   Name of the time zone datetime values that have a source time zone should be converted to
    /// </param>
    /// <param name="allowPercentage">If <c>true</c> percentage symbols are is processed to a decimal 26.7% will become .267</param>
    /// <param name="removeCurrency">If <c>true</c> common currency symbols are removed to parse a currency value as decimal</param>
    protected BaseFileReader(string fileName,
      in IEnumerable<Column>? columnDefinition,
      long recordLimit,
      in TimeZoneChangeDelegate? timeZoneAdjust,
      string returnedTimeZone,
      bool allowPercentage,
      bool removeCurrency)
    {
      TimeZoneAdjust = timeZoneAdjust ?? StandardTimeZoneAdjust.ChangeTimeZone;
      ReturnedTimeZone = string.IsNullOrEmpty(returnedTimeZone) ? TimeZoneInfo.Local.Id : returnedTimeZone;
      m_ColumnDefinition = columnDefinition is null
        ? []
        : new List<Column>(columnDefinition).ToArray();
      RecordLimit = recordLimit < 1 ? long.MaxValue : recordLimit;
      FullPath = fileName;
      SelfOpenedStream = !string.IsNullOrWhiteSpace(fileName);
      FileName = FileSystemUtils.GetFileName(fileName);
      m_AllowPercentage = allowPercentage;
      m_RemoveCurrency = removeCurrency;
    }

    /// <summary>
    ///   Occurs when something went wrong during opening of the setting, this might be the file
    ///   does not exist or a query ran into a timeout
    /// </summary>
    public event EventHandler<RetryEventArgs>? OnAskRetry;

    /// <summary>
    /// Occurs when opening is at its end.
    /// </summary>
    public event EventHandler<IReadOnlyCollection<Column>>? OpenFinished;

    /// <summary>
    ///   Event to be raised if reading the files is completed
    /// </summary>
    public event EventHandler? ReadFinished;

    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    public event EventHandler<WarningEventArgs>? Warning;

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

    /// <summary>
    /// Gets a value that indicates whether this <see cref="T:System.Data.Common.DbDataReader"></see> contains one or more rows.
    /// </summary>
    public override bool HasRows => !EndOfFile;

    /// <summary>
    /// Open the reader asynchronously
    /// </summary>
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
    /// Gets or sets the report progress.
    /// </summary>
    /// <value>
    /// The report progress.
    /// </value>
    public IProgress<ProgressInfo> ReportProgress
    {
      protected get
      {
        return m_ReportProgress;
      }
      set
      {
        value.SetMaximum(cMaxProgress);
        m_ReportProgress = value;
      }
    }

    /// <summary>
    ///   Current Line Number in the text file where the record has started
    /// </summary>
    public virtual long StartLineNumber { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether the reader can restart at the beginning. 
    /// </summary>
    /// <value>
    ///   <c>true</c> if supports reset; otherwise, <c>false</c>.
    /// </value>
    public virtual bool SupportsReset => true;

    /// <summary>
    /// Gets the number of fields in the <see cref="T:System.Data.Common.DbDataReader"></see> that are not hidden.
    /// </summary>
    public override int VisibleFieldCount => Column.Count(x => !x.Ignore);

    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    /// <value>
    /// The name of the file.
    /// </value>
    protected string FileName { get; }

    /// <summary>
    /// Gets the full path.
    /// </summary>
    /// <value>
    /// The full path.
    /// </value>
    protected string FullPath { get; }

    /// <inheritdoc />
    public override object this[string columnName] => GetValue(GetOrdinal(columnName));

    /// <inheritdoc />    
    public override object this[int ordinal] => GetValue(ordinal);

    /// <inheritdoc />   
    public override void Close()
    {
      if (!EndOfFile)
        EndOfFile = true;

      if (CurrentRowColumnText.Length > 0 && CurrentRowColumnText != Array.Empty<string>())
      {
        CurrentRowColumnText = [];
      }

      base.Close();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Gets the boolean.
    /// </summary>
    /// <param name="ordinal">The zero-based column ordinal.</param>    
    public override bool GetBoolean(int ordinal)
    {
      var parsed = GetBooleanNull(CurrentRowColumnText[ordinal].AsSpan(), ordinal);
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
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
      using var stream = GetStream(ordinal);

      // Return total length if no buffer is supplied (ADO.NET convention)
      if (buffer == null)
        return stream.Length;

      if (bufferOffset < 0 || length < 0 || bufferOffset + length > buffer.Length)
        throw new ArgumentOutOfRangeException(nameof(length), "Invalid buffer range specified.");

      if (dataOffset < 0)
        throw new ArgumentOutOfRangeException(nameof(dataOffset), "Offset cannot be negative.");

      if (dataOffset >= stream.Length)
        return 0; // No data to read

      stream.Seek(dataOffset, SeekOrigin.Begin);
      return stream.Read(buffer, bufferOffset, length);
    }

    /// <inheritdoc />
    public override char GetChar(int ordinal) => GetString(ordinal)[0];

    /// <inheritdoc />
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
      var text = CurrentRowColumnText[ordinal];
      var totalLength = text.Length;

      // ADO.NET convention: if buffer == null, return total length of data
      if (buffer == null)
        return totalLength;

      if (dataOffset < 0 || dataOffset > totalLength)
        return 0; // Nothing to read or invalid offset

      if (bufferOffset < 0 || length < 0 || bufferOffset + length > buffer.Length)
        throw new ArgumentOutOfRangeException(nameof(length), "Invalid buffer range specified.");

      // Compute how many characters we can actually copy
      var charsAvailable = totalLength - (int) dataOffset;
      var charsToCopy = Math.Min(charsAvailable, length);

      if (charsToCopy <= 0)
        return 0;

      text.CopyTo((int) dataOffset, buffer, bufferOffset, charsToCopy);
      return charsToCopy;
    }

    /// <summary>
    ///   Gets the column format.
    /// </summary>
    /// <param name="ordinal">The column.</param>
    /// <returns></returns>
    public virtual Column GetColumn(int ordinal) => Column[ordinal];

    /// <inheritdoc />
    public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

    /// <inheritdoc />
    public override DateTime GetDateTime(int ordinal)
    {
      var dt = GetDateTimeNull(
        null,
        CurrentRowColumnText[ordinal].AsSpan(),
        null,
        GetTimeValue(ordinal),
        GetColumn(ordinal),
        true);
      if (dt.HasValue) return dt.Value;

      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(
        ordinal,
        $"'{CurrentRowColumnText[ordinal]}' is not a date of the format '{GetColumn(ordinal).ValueFormat.DateFormat}'");
    }

    /// <inheritdoc />
    public override decimal GetDecimal(int ordinal)
    {
      var decimalValue = GetDecimalNull(CurrentRowColumnText[ordinal].AsSpan(), ordinal);
      return decimalValue ?? throw
        // Warning was added by GetDecimalNull
        WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not a decimal");
    }

    /// <inheritdoc />
    public override double GetDouble(int ordinal)
    {
      var decimalValue = GetDecimalNull(CurrentRowColumnText[ordinal].AsSpan(), ordinal);
      return decimalValue.HasValue ? Convert.ToDouble(decimalValue.Value) : throw
        // Warning was added by GetDecimalNull
        WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not a double");
    }

    /// <inheritdoc />
    public override IEnumerator GetEnumerator() => new DbEnumerator(this, true);

    /// <inheritdoc />
    public override Type GetFieldType(int ordinal) => GetColumn(ordinal).ValueFormat.DataType.GetNetType();

    /// <inheritdoc />
    public override float GetFloat(int ordinal)
    {
      var decimalValue = GetDecimalNull(CurrentRowColumnText[ordinal].AsSpan(), ordinal);
      return decimalValue.HasValue ? Convert.ToSingle(decimalValue, CultureInfo.InvariantCulture) : throw
        // Warning was added by GetDecimalNull
        WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not a float");
    }

    /// <inheritdoc />
    public override Guid GetGuid(int ordinal)
    {
      var parsed = GetGuidNull(CurrentRowColumnText[ordinal].AsSpan(), ordinal);

      return parsed ?? throw WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not an GUID");
    }

    /// <inheritdoc />
    public override short GetInt16(int ordinal)
    {
      var column = GetColumn(ordinal);

      var parsed =
        CurrentRowColumnText[ordinal].AsSpan().StringToInt16(column.ValueFormat.GroupSeparator);
      return parsed ?? throw
        // Warning was added by GetInt32Null
        WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not a short");
    }

    /// <inheritdoc />
    public override int GetInt32(int ordinal)
    {
      var column = GetColumn(ordinal);

      var parsed =
        CurrentRowColumnText[ordinal].AsSpan().StringToInt32(column.ValueFormat.GroupSeparator);
      return parsed ?? throw
        // Warning was added by GetInt32Null
        WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not an integer");
    }

    /// <summary>
    ///   Gets the int32 value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="column">The column.</param>
    /// <returns>a nullable integer</returns>
    public int? GetInt32Null(ReadOnlySpan<char> inputValue, in Column column)
    {
      var ret = inputValue.StringToInt32(column.ValueFormat.GroupSeparator);
      if (ret.HasValue) return ret.Value;

      HandleError(column.ColumnOrdinal, $"'{inputValue.ToString()}' is not an integer");
      return null;
    }

    /// <inheritdoc />
    public override long GetInt64(int ordinal)
    {
      var column = GetColumn(ordinal);

      var parsed = CurrentRowColumnText[ordinal].AsSpan().StringToInt64(column.ValueFormat.GroupSeparator);
      return parsed ?? throw WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not a long integer");
    }

    /// <summary>
    ///   Gets the long value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="column">The column.</param>    
    public long? GetInt64Null(ReadOnlySpan<char> inputValue, in Column column)
    {
      var ret = inputValue.StringToInt64(column.ValueFormat.GroupSeparator);
      if (ret.HasValue) return ret.Value;

      HandleError(column.ColumnOrdinal, $"'{inputValue.ToString()}' is not an long integer");
      return null;
    }

    /// <inheritdoc />
    public override string GetName(int ordinal) => GetColumn(ordinal).Name;


    /// <inheritdoc />
    public override int GetOrdinal(string name)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException("Column name must not be null or empty.", nameof(name));

      if (m_ColumnIndexMap.Count>0)
      {
        if (m_ColumnIndexMap.TryGetValue(name, out var index))
          return index;
      }
      else
        for (int i = 0; i < m_FieldCount; i++)
        {
          if (Column[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            return i;
        }

      throw new IndexOutOfRangeException($"The column name '{name}' was not found.");
    }

    /// <inheritdoc />
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
        schemaRow[19] = column.Ignore; // IsHidden        

        // If there is a conversion get the information
        if (column.Convert && column.ValueFormat.DataType != DataTypeEnum.String)
          schemaRow[7] = column.ValueFormat.DataType.GetNetType();
        else
          schemaRow[7] = typeof(string);

        dataTable.Rows.Add(schemaRow);
      }

      return dataTable;
    }

    /// <summary>
    ///   Gets the originally provided text of a column
    /// </summary>
    /// <param name="ordinal">The column number.</param>
    /// <returns></returns>    
    public virtual ReadOnlySpan<char> GetSpan(int ordinal) => CurrentRowColumnText[ordinal].AsSpan();

    /// <summary>
    /// Retrieves data as a <see cref="T:System.IO.Stream"></see>.
    /// </summary>
    /// <param name="ordinal">Retrieves data as a <see cref="T:System.IO.Stream"></see>.</param>
    /// <returns>
    /// The returned object.
    /// </returns>
    public override Stream GetStream(int ordinal)
    {
      if (GetColumn(ordinal).ValueFormat.DataType == DataTypeEnum.Binary)
        return new FileStream(path: CurrentRowColumnText[ordinal], mode: FileMode.Open, access: FileAccess.Read);
      return new MemoryStream(Encoding.UTF8.GetBytes(CurrentRowColumnText[ordinal]));
    }

    /// <inheritdoc />
    public override string GetString(int ordinal) =>
      CurrentRowColumnText[ordinal];

    /// <summary>
    /// Retrieves data as a <see cref="TextReader"/> for the specified column.
    /// </summary>
    /// <param name="ordinal">The zero-based column ordinal.</param>
    /// <returns>
    /// A <see cref="TextReader"/> instance for reading the column value as text.
    /// If the value is <see cref="DBNull"/>, an empty <see cref="StringReader"/> is returned.
    /// </returns>
    public override TextReader GetTextReader(int ordinal)
    {
      if (IsDBNull(ordinal))
        return new StringReader(string.Empty);
      var text = CurrentRowColumnText[ordinal];

      if (GetColumn(ordinal).ValueFormat.DataType == DataTypeEnum.Binary)
      {
        // Interpret as file reference (text-based)
        return File.Exists(text)
          ? File.OpenText(text)
          : new StringReader(string.Empty);
      }

      // Treat as direct textual data
      return new StringReader(text);
    }

    /// <inheritdoc />
    public override object GetValue(int ordinal)
    {
      if (ordinal < 0 || ordinal >= m_FieldCount)
        throw new IndexOutOfRangeException($"The column ordinal {ordinal} is out of range.");
      if (IsDBNull(ordinal))
        return DBNull.Value;
      var column = GetColumn(ordinal);

      object ret;
      try
      {
        ret = column.ValueFormat.DataType switch
        {
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
      catch (OverflowException)
      {
        return DBNull.Value;
      }
      return ret;
    }

    /// <inheritdoc />
    public override int GetValues(object[] values)
    {
      if (values is null)
        throw new ArgumentNullException(nameof(values));

      var count = Math.Min(values.Length, FieldCount);

      for (int ordinal = 0; ordinal < count; ordinal++)
        values[ordinal] = GetValue(ordinal);

      return count;
    }

    /// <summary>
    ///   Calls the event handler for errors <see cref="Warning"/>
    /// </summary>
    /// <param name="ordinal">The column ordinal number.</param>
    /// <param name="message">The message to raise.</param>
    protected void HandleError(int ordinal, string message)
    {
      // Ignore message for ignore columns
      if (ordinal>=0 && ordinal < Column.Length && GetColumn(ordinal).Ignore)
        return;
      Warning?.SafeInvoke(this, GetWarningEventArgs(ordinal, message));
    }

    /// <summary>
    ///   Handles the Event if reading the file is completed
    /// </summary>
    public virtual void HandleReadFinished()
    {
      if (m_IsFinished) return;

      m_IsFinished = true;
      HandleShowProgress("Finished reading", 1);
      ReadFinished?.SafeInvoke(this);
    }

    /// <summary>
    ///   Calls the event handler for warnings <see cref="Warning"/>
    /// </summary>
    /// <param name="ordinal">The column ordinal number</param>
    /// <param name="message">The message to raise.</param>
    protected void HandleWarning(int ordinal, string message) => HandleError(ordinal, message.AddWarningId());

    /// <inheritdoc />
    public override bool IsDBNull(int ordinal)
    {
      if (ordinal<0 || CurrentRowColumnText.Length <= ordinal)
        return true;

      if (GetColumn(ordinal).ValueFormat.DataType != DataTypeEnum.DateTime)
        return string.IsNullOrWhiteSpace(CurrentRowColumnText[ordinal]);

      // Handle combined date-time column
      if (AssociatedTimeCol[ordinal] == -1 || AssociatedTimeCol[ordinal] >= CurrentRowColumnText.Length)
        return string.IsNullOrWhiteSpace(CurrentRowColumnText[ordinal]);

      /* For a combined date column to be empty both parts need to be empty */
      return string.IsNullOrEmpty(CurrentRowColumnText[ordinal])
             && string.IsNullOrWhiteSpace(CurrentRowColumnText[AssociatedTimeCol[ordinal]]);
    }

    /// <inheritdoc cref="DbDataReader" />
    public override bool NextResult() => false;

    /// <inheritdoc />
    public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => Task.FromResult(false);

    /// <inheritdoc cref="IDataReader" />
    [Obsolete("Use OpenAsync instead for asynchronous, non-blocking operation.")]
    public virtual void Open()
    {
      // Synchronously calls async method; safe for non-UI threads.
      OpenAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <inheritdoc cref="IDataReader" />
    public abstract Task OpenAsync(CancellationToken cancellationToken);

    /// <inheritdoc cref="IDataReader" />
    /// <remarks>
    /// This synchronous method blocks until the asynchronous read completes. 
    /// Prefer ReadAsync in async scenarios.
    /// </remarks>
    public virtual bool Read(in CancellationToken cancellationToken) =>
        ReadAsync(cancellationToken).GetAwaiter().GetResult();

    /// <inheritdoc cref="DbDataReader" />
    /// <remarks>
    /// This synchronous method blocks until the asynchronous read completes.
    /// Prefer ReadAsync(CancellationToken) in asynchronous scenarios.
    /// </remarks>
    public override bool Read() =>
        ReadAsync(CancellationToken.None).GetAwaiter().GetResult();

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
    ///   Does look at the provided column names, and checks them for valid entry, makes sure the
    ///   column names are unique and not empty, have the right size etc.
    /// </summary>
    /// <param name="columns">The columns as read / provided</param>
    /// <param name="fieldCount">
    ///   The maximum number of fields, if more than this number are provided, it will ignore these columns
    /// </param>
    private IEnumerable<string> AdjustColumnName(in IEnumerable<string> columns, int fieldCount)
    {
      var newNames = new List<string>(fieldCount);
      var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      int counter = 0;

      foreach (var column in columns)
      {
        if (counter >= fieldCount) break;

        var trimmed = column != null ? column.Trim() : string.Empty;
        string resultingName;

        if (trimmed.Length == 0)
        {
          resultingName = GetDefaultName(counter);
          HandleWarning(counter, $"Column title was empty, set to {resultingName}.");
        }
        else
        {
          resultingName = trimmed;

          if (column != trimmed)
            HandleWarning(counter, $"Column title '{column}' had leading or trailing spaces, removed.");

          // Truncate first
          if (resultingName.Length > 128)
          {
            var preview = resultingName.Length > 20 ? resultingName.Substring(0, 20) + "…" : resultingName;
            resultingName = resultingName.Substring(0, 128);
            HandleWarning(counter, $"Column title '{preview}' too long, cut off after 128 characters.");
          }

          // Ensure uniqueness (handles truncated columns)
          var uniqueName = existingNames.MakeUniqueInCollection(resultingName);
          if (uniqueName != resultingName)
          {
            HandleError(counter, $"Column title '{resultingName}' exists more than once, replaced with {uniqueName}");
            resultingName = uniqueName;
          }
        }

        newNames.Add(resultingName);
        existingNames.Add(resultingName);
        counter++;
      }

      return newNames;
    }



    /// <summary>
    /// Processes a text span by optionally trimming, converting non-breaking spaces to normal spaces,
    /// and checking if the text should be treated as null.
    /// </summary>
    /// <param name="inputString">The input span of characters.</param>
    /// <param name="treatNbspAsSpace">
    /// If <c>true</c>, converts non-breaking spaces (0xA0) to regular spaces.
    /// </param>
    /// <param name="treatTextAsNull">
    /// A span of text which, if matched exactly, should be treated as null.
    /// </param>
    /// <param name="trim">
    /// If <c>true</c>, leading and trailing whitespace will be removed (NBSP not removed by default).
    /// </param>
    /// <returns>
    /// A <see cref="ReadOnlySpan{Char}"/> representing the processed text, or <c>ReadOnlySpan&lt;char&gt;.Empty</c> if the text is considered null.
    /// </returns>
    protected static ReadOnlySpan<char> TreatNbspAsNullTrim(
      ReadOnlySpan<char> inputString,
      bool treatNbspAsSpace,
      ReadOnlySpan<char> treatTextAsNull,
      bool trim)
    {
      if (inputString.Length == 0)
        return [];

      if (trim)
        inputString = inputString.Trim();

      if (treatNbspAsSpace && inputString.IndexOf((char) 0xA0) != -1)
        return inputString.ToString().Replace((char) 0xA0, ' ').AsSpan().ShouldBeTreatedAsNull(treatTextAsNull)
          ? []
          : inputString;
      return inputString.ShouldBeTreatedAsNull(treatTextAsNull) ? [] : inputString;
    }

    /// <summary>
    ///   Sets the Progress to marquee, calls OnOpen Event, check if the file does exist if it's a
    ///   physical file
    /// </summary>
    protected Task BeforeOpenAsync(string message)
    {
      HandleShowProgress(message, 0);
      return OnOpenAsync != null ? OnOpenAsync.Invoke() : Task.CompletedTask;
    }

    /// <summary>
    ///   Does set EndOfFile sets the max value for Progress and stores the now know reader columns
    /// </summary>
    protected void FinishOpen()
    {
      m_IsFinished = false;
      EndOfFile = false;

      m_ColumnIndexMap.Clear();
      // Precomputed name lookup
      for (int i = 0; i < m_FieldCount; i++)
        m_ColumnIndexMap[Column[i].Name] = i;

      OpenFinished?.SafeInvoke(this, Column);
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
    protected bool? GetBooleanNull(ReadOnlySpan<char> inputBoolean, int ordinal)
    {
      var column = GetColumn(ordinal);
      var boolValue = inputBoolean.StringToBoolean(column.ValueFormat.True.AsSpan(), column.ValueFormat.False.AsSpan());
      if (boolValue.HasValue)
        return boolValue.Value;

      HandleError(ordinal, $"'{inputBoolean.ToString()}' is not a boolean");
      return null;
    }


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
      ReadOnlySpan<char> strInputDate,
      in object? inputTime,
      ReadOnlySpan<char> strInputTime,
      Column column,
      bool serialDateTime)
    {
      var dateTime = StringConversionSpan.CombineObjectsToDateTime(
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
          passedIn = Convert.ToString(inputTime).AsSpan();

        HandleWarning(
          column.ColumnOrdinal,
          $"'{passedIn.ToString()}' is outside expected range 00:00 - 23:59, the date has been adjusted");
      }

      if (!dateTime.HasValue && !strInputDate.IsEmpty
                             && !string.IsNullOrEmpty(column.ValueFormat.DateFormat)
                             && strInputDate.Length > column.ValueFormat.DateFormat.Length)
      {
        var inputDateNew = strInputDate.Slice(0, column.ValueFormat.DateFormat.Length);
        dateTime = inputDateNew.CombineStringsToDateTime(column.ValueFormat.DateFormat.AsSpan(),
          strInputTime,
          column.ValueFormat.DateSeparator,
          column.ValueFormat.TimeSeparator,
          serialDateTime);
        if (dateTime.HasValue)
        {
          var display1 = column.ValueFormat.DateFormat.ReplaceDefaults(
            '/', column.ValueFormat.DateSeparator,
            ':', column.ValueFormat.TimeSeparator);
          HandleWarning(
            column.ColumnOrdinal,
            strInputTime.Length > 0
              ? $"'{strInputDate.ToString()} {strInputTime.ToString()}' is not a date of the format '{display1}' '{column.TimePartFormat}', used '{inputDateNew.ToString()} {strInputTime.ToString()}'"
              : $"'{strInputDate.ToString()}' is not a date of the format '{display1}', used '{inputDateNew.ToString()}' ");
        }
      }

      // ReSharper disable once MergeIntoPattern
      if (dateTime.HasValue && dateTime.Value.Year is > 1752 and <= 9999)
      {
        // get the time zone either from constant or from other column
        if (!column.TimeZonePart.TryGetConstant(out var timeZone) &&
            m_AssociatedTimeZoneCol.Length > column.ColumnOrdinal && m_AssociatedTimeZoneCol[column.ColumnOrdinal] > -1)
          timeZone = GetString(m_AssociatedTimeZoneCol[column.ColumnOrdinal]);

        return TimeZoneAdjust(dateTime.Value, timeZone, ReturnedTimeZone, message => HandleWarning(column.ColumnOrdinal, message));
      }

      var display2 = column.ValueFormat.DateFormat.ReplaceDefaults(
        '/', column.ValueFormat.DateSeparator,
        ':', column.ValueFormat.TimeSeparator);

      HandleError(
        column.ColumnOrdinal,
        !strInputTime.IsEmpty
          ? $"'{strInputDate.ToString()} {strInputTime.ToString()}' is not a date of the format {display2} {column.TimePartFormat}"
          : $"'{strInputDate.ToString()}' is not a date of the format {display2}");

      return null;
    }

    /// <summary>
    ///   Gets the decimal value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="ordinal">The column.</param>
    protected decimal? GetDecimalNull(ReadOnlySpan<char> inputValue, int ordinal)
    {
      var column = GetColumn(ordinal);
      var decimalValue = inputValue.StringToDecimal(column.ValueFormat.DecimalSeparator,
        column.ValueFormat.GroupSeparator,
        m_AllowPercentage, m_RemoveCurrency);
      if (decimalValue.HasValue) return decimalValue.Value;

      HandleError(column.ColumnOrdinal, $"'{inputValue.ToString()}' is not a decimal");
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
    protected double? GetDoubleNull(ReadOnlySpan<char> inputValue, int ordinal)
    {
      var decimalValue = GetDecimalNull(inputValue, ordinal);
      if (decimalValue.HasValue) return decimal.ToDouble(decimalValue.Value);

      HandleError(ordinal, $"'{inputValue.ToString()}' is not a double");
      return null;
    }

    /// <summary>
    ///   Gets the int32 value or null.
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="ordinal">The column number.</param>
    protected Guid? GetGuidNull(ReadOnlySpan<char> inputValue, int ordinal)
    {
      if (inputValue.IsEmpty) return null;

      var res = inputValue.StringToGuid();
      if (res.HasValue)
        return res.Value;
      HandleError(ordinal, $"'{inputValue.ToString()}' is not a GUID");
      return null;
    }

    /// <summary>
    ///   Gets the relative position; this only works if RecordLimit is set
    /// </summary>
    /// <returns>A value between 0 and 1</returns>
    /// <summary>
    ///   Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and 1 for 0% to 100%</returns>
    protected virtual double GetRelativePosition()
    {
      if (RecordLimit is > 0 and < long.MaxValue)
        return (double) RecordNumber / RecordLimit;
      return 0;
    }

    /// <summary>
    ///   Gets the associated value.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    protected virtual ReadOnlySpan<char> GetTimeValue(int i) =>
      AssociatedTimeCol[i] == -1 || AssociatedTimeCol[i] >= CurrentRowColumnText.Length
        ? []
        : CurrentRowColumnText[AssociatedTimeCol[i]].AsSpan();

    /// <summary>
    /// Gets the warning event arguments.
    /// </summary>
    /// <param name="ordinal">The ordinal.</param>
    /// <param name="message">The message.</param>
    private WarningEventArgs GetWarningEventArgs(int ordinal, string message) =>
      new WarningEventArgs(
        RecordNumber,
        ordinal,
        message,
        StartLineNumber,
        EndLineNumber,
        ordinal >= 0 && ordinal < m_FieldCount ? GetColumn(ordinal).Name : null);

    /// <summary>
    ///   Shows the process.
    /// </summary>
    /// <param name="text">Leading Text</param>
    /// <param name="percent">Value between 0 and 1 representing the relative position</param>
    protected virtual void HandleShowProgress(string text, double percent) =>
      m_ReportProgress.Report(new ProgressInfo(text, (percent * cMaxProgress).ToInt64()));

    /// <summary>
    ///   Shows the process twice a second
    /// </summary>
    /// <param name="text">Leading Text</param>
    protected virtual void HandleShowProgressPeriodic(string text)
      => m_IntervalAction.Invoke(() => HandleShowProgress(text, GetRelativePosition()));

    /// <summary>
    ///   Does handle TextToHML, TextToHtmlFull, TextPart and TreatNBSPAsSpace and does update the
    ///   maximum column size
    ///   Attention: Trimming needs to be handled beforehand
    /// </summary>
    /// <param name="span">The input string.</param>
    /// <param name="ordinal">The column number</param>
    /// <returns>The proper encoded or cut text as returned for the column</returns>
    protected virtual ReadOnlySpan<char> HandleTextSpecials(ReadOnlySpan<char> span, int ordinal)
    {
      if (span.IsEmpty || ordinal >= FieldCount)
        return [];

      return GetColumn(ordinal).ColumnFormatter.FormatInputText(span);
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

    /// <summary>
    /// Initializes the column and arrays once it's known how many columns there are
    /// </summary>
    /// <param name="fieldCount">The field count.</param>
    protected virtual void InitColumn(int fieldCount)
    {
      if (fieldCount > 1000)
        throw new ArgumentException($"Too many columns: {fieldCount}. Maximum supported: 1000");
      m_FieldCount = fieldCount;
      m_ColumnIndexMap.Clear();
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
    ///   TimeZone. Column must be set beforehand
    /// </summary>
    /// <param name="headerRow">The header row.</param>
    /// <param name="dataType">Type of the data.</param>
    /// <param name="hasFieldHeader">if set to <c>true</c> if file has field header.</param>
    protected virtual void ParseColumnName(
      in IEnumerable<string> headerRow,
      in IEnumerable<DataTypeEnum>? dataType = null,
      bool hasFieldHeader = true)
    {
      var adjustedNames = new List<string>();
      if (hasFieldHeader)
        adjustedNames.AddRange(AdjustColumnName(headerRow, Column.Length));
      else
        for (var colIndex = 0; colIndex < Column.Length; colIndex++)
        {
          if (GetColumn(colIndex).Name.Equals(GetDefaultName(colIndex)))
          {
            // Might have passed in the column names in m_ColumnDefinition (used with Manifest data
            // accompanying a file without header)
            var newDef = m_ColumnDefinition.FirstOrDefault(x => x.ColumnOrdinal == colIndex);
            if (newDef != null && !string.IsNullOrEmpty(newDef.Name))
            {
              HandleError(colIndex, "Using column name from definition");
              adjustedNames.Add(newDef.Name);
              continue;
            }
          }

          adjustedNames.Add(GetColumn(colIndex).Name);
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
        HandleWarning(-1, "Column should be set before using ParseColumnName to handle TimePart and TimeZone");
      else
        // Initialize the references for TimePart and TimeZone
        for (var index = 0; index < Column.Length; index++)
        {
          var column = GetColumn(index);
          // if the original column that reference other columns is ignored, skip it
          if (column.Ignore) continue;

          var searchedTimePart = column.TimePart;
          var searchedTimeZonePart = column.TimeZonePart;

          if (!string.IsNullOrEmpty(searchedTimePart))
            for (var indexPoint = 0; indexPoint < Column.Length; indexPoint++)
            {
              if (indexPoint == index) continue;
              if (!GetColumn(indexPoint).Name.Equals(searchedTimePart, StringComparison.OrdinalIgnoreCase)) continue;
              AssociatedTimeCol[index] = indexPoint;
              break;
            }

          if (string.IsNullOrEmpty(searchedTimeZonePart)) continue;

          for (var indexPoint = 0; indexPoint < Column.Length; indexPoint++)
          {
            if (indexPoint == index) continue;

            if (!GetColumn(indexPoint).Name.Equals(searchedTimeZonePart, StringComparison.OrdinalIgnoreCase)) continue;
            m_AssociatedTimeZoneCol[index] = indexPoint;
            break;
          }
        }
    }

    /// <summary>
    /// Checks if we should retry to access the data
    /// </summary>
    /// <param name="ex">The exception.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    protected bool ShouldRetry(in Exception ex, in CancellationToken token)
    {
      if (token.IsCancellationRequested) return false;

      var eventArgs = new RetryEventArgs(ex) { Retry = false };
      var handler = Volatile.Read(ref OnAskRetry);
      handler?.Invoke(this, eventArgs);
      return eventArgs.Retry;
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    ///   Closes the <see cref="T:System.Data.IDataReader" /> Object.
    /// </summary>
    public new virtual Task CloseAsync() => Task.Run(() => base.Close());
#endif
    /// <summary>
    ///   Adds a Format exception.
    /// </summary>
    /// <param name="ordinal">The column.</param>
    /// <param name="message">The message.</param>
    protected FormatException WarnAddFormatException(int ordinal, string message)
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
  }
}
