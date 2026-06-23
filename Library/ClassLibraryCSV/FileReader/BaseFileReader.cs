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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsvTools;

/// <inheritdoc cref="DbDataReader" />
/// <summary>
///   Abstract class as a base for all file-based DataReaders.
///   This combines the former BaseFileReader and BaseFileReaderTyped behavior.
/// </summary>
public abstract class BaseFileReader : DbDataReader, IFileReader
{
  /// <summary>
  ///   An array of column definitions.
  /// </summary>
  public Column[] Column = [];

  /// <summary>
  /// The time zone to convert the read data to, assuming the source time zone is part of the data.
  /// </summary>
  protected readonly string ReturnedTimeZone;

  /// <summary>
  /// The routine used for time zone adjustment.
  /// </summary>
  protected readonly TimeZoneChangeDelegate TimeZoneAdjust;

  /// <summary>
  /// The record limit.
  /// </summary>
  protected long RecordLimit;

  /// <summary>
  /// If the reader opens the stream, this is true.
  /// </summary>
  protected bool SelfOpenedStream;

  /// <summary>
  ///   The maximum value
  /// </summary>
  private const int cMaxProgress = 10000;

  private readonly bool m_AllowPercentage;
  private readonly IReadOnlyCollection<Column> m_ColumnDefinition;
  private readonly DictionaryIgnoreCase<int> m_ColumnIndexMap = new DictionaryIgnoreCase<int>();

  /// <summary>
  ///   An array of current row column text, this is reused to avoid multiple allocations.
  ///   We will have the information in here from ignored columns.
  /// </summary>
  private readonly RowColumnsBuffer m_CurrentRowColumnText = new RowColumnsBuffer();
  private readonly IntervalAction m_IntervalAction = new IntervalAction();
  private readonly bool m_RemoveCurrency;
  private readonly ReadOnlyMemory<char>[] m_TreatAsNullMemories;
  private readonly bool m_TreatNbspAsSpace;
  private readonly bool m_Trim;

  /// <summary>
  ///   An array of associated col
  /// </summary>
  private int[] m_AssociatedTimeZoneCol = [];

  /// <summary>
  ///   An array of associated time columns.
  /// </summary>
  private int[] m_AssociatedTimeCol = [];

  /// <summary>
  ///   Optional typed values of the current row.
  ///   Used by typed readers such as JSON/XML.
  ///   Null for pure text readers such as CSV.
  /// </summary>
  private object?[]? m_CurrentValues;
  
  /// <summary>
  ///   Number of Columns in the reader, including the ones ignored
  ///   e.G. a TimeColum associated with a date could be ignored, we still need the information
  /// </summary>
  private int m_FieldCount;
  private bool m_IsFinished;
  private bool[] m_ParseFromSource = [];
  private IProgress<ProgressInfo> m_ReportProgress = new Progress<ProgressInfo>();

  /// <summary>
  ///   Constructor for abstract base class for <see cref="T:CsvTools.IFileReader" />
  /// </summary>
  /// <param name="fileName">Path to a physical file (if used)</param>
  /// <param name="columnDefinition">List of column definitions</param>
  /// <param name="recordLimit">Number of records that should be read</param>
  /// <param name="trim">If we should trim values</param>
  /// <param name="treatTextAsNull">A semicolon or tab separated list of that should be treated as NULL</param>
  /// <param name="treatNbspAsSpace">Treat an NBSP as regular space</param>
  /// <param name="returnedTimeZone">
  ///   Name of the time zone datetime values that have a source time zone should be converted to
  /// </param>
  /// <param name="allowPercentage">If <c>true</c> percentage symbols are is processed to a decimal 26.7% will become 0.267</param>
  /// <param name="removeCurrency">If <c>true</c> common currency symbols are removed to parse a currency value as decimal</param>
  /// <param name="useTypedValues">If <c>true</c> pass in and store typed values</param>
  protected BaseFileReader(
    string fileName,
    IEnumerable<Column>? columnDefinition,
    long recordLimit,
    bool trim,
    string treatTextAsNull,
    bool treatNbspAsSpace,
    string returnedTimeZone,
    bool allowPercentage,
    bool removeCurrency,
    bool useTypedValues)
  {
    TimeZoneAdjust = FunctionalDI.GetTimeZoneAdjust;
    ReturnedTimeZone = string.IsNullOrEmpty(returnedTimeZone) ? TimeZoneInfo.Local.Id : returnedTimeZone;
    m_ColumnDefinition = columnDefinition is null
      ? []
      : new List<Column>(columnDefinition).ToArray();

    RecordLimit = recordLimit < 1 ? long.MaxValue : recordLimit;
    FullPath = fileName;
    SelfOpenedStream = !string.IsNullOrWhiteSpace(fileName);
    FileName = fileName.GetFileName();

    m_AllowPercentage = allowPercentage;
    m_RemoveCurrency = removeCurrency;
    m_Trim = trim;
    m_TreatNbspAsSpace = treatNbspAsSpace;
    m_CurrentValues = useTypedValues ? Array.Empty<object?>() : null;

    if (string.IsNullOrWhiteSpace(treatTextAsNull))
    {
      m_TreatAsNullMemories = Array.Empty<ReadOnlyMemory<char>>();
    }
    else
    {
      m_TreatAsNullMemories = [.. treatTextAsNull
        .Split(StaticCollections.ListDelimiterChars, StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim().AsMemory())];
    }
  }

  /// <summary>
  ///   Occurs when something went wrong during the opening of the setting, this might be the file
  ///   does not exist or a query ran into a timeout
  /// </summary>
  public event EventHandler<RetryEventArgs>? OnAskRetry;

  /// <summary>
  /// Occurs when the opening is at its end.
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
  public override int Depth => 0;

  /// <summary>
  ///   Current Line Number in the text file, a record can span multiple lines, and lines are
  ///   skipped, this is the ending line
  /// </summary>
  public virtual long EndLineNumber { get; protected set; }

  /// <summary>
  ///   Gets or sets a value indicating whether the reader is at the end of the file.
  /// </summary>
  /// <value><c>true</c> if at the end of file; otherwise, <c>false</c>.</value>
  public virtual bool EndOfFile { get; protected set; } = true;

  /// <inheritdoc />
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
  public override int RecordsAffected => -1;

  /// <summary>
  /// Gets or sets the report progress.
  /// </summary>
  /// <value>
  /// The report progress.
  /// </value>
  public IProgress<ProgressInfo> ReportProgress
  {
    protected get => m_ReportProgress;
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

  /// <summary>
  /// The text values in the row
  /// </summary>
  protected IReadOnlyList<string> CurrentRowText => m_CurrentRowColumnText;

  /// <inheritdoc />
  public override object this[string columnName] => GetValue(GetOrdinal(columnName));

  /// <inheritdoc />
  public override object this[int ordinal] => GetValue(ordinal);

  /// <inheritdoc />
  public override void Close()
  {
    if (!EndOfFile)
      EndOfFile = true;
    base.Close();
  }

  /// <inheritdoc />
  public override bool GetBoolean(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    if (val is bool b)
      return b;

    var col = GetColumn(ordinal);
    var parsed = SpanToBoolean(col, GetSpan(ordinal));
    return parsed ?? throw new FormatException($"'{GetString(ordinal)}' is not a boolean ({col.ValueFormat.True}/{col.ValueFormat.False})");
  }

  /// <inheritdoc />
  public override byte GetByte(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    if (val is byte b)
      return b;
    var result = SpanToLong(GetColumn(ordinal), GetSpan(ordinal));
    return result.HasValue ? (byte) result.Value : throw new FormatException($"'{GetString(ordinal)}' is not a byte");
  }

  /// <inheritdoc />
  public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
  {
    using var stream = GetStream(ordinal);

    if (buffer == null)
      return stream.Length;

    if (bufferOffset < 0 || length < 0 || bufferOffset + length > buffer.Length)
      throw new ArgumentOutOfRangeException(nameof(length), "Invalid buffer range specified.");

    if (dataOffset < 0)
      throw new ArgumentOutOfRangeException(nameof(dataOffset), "Offset cannot be negative.");

    if (dataOffset >= stream.Length)
      return 0;

    stream.Seek(dataOffset, SeekOrigin.Begin);
    return stream.Read(buffer, bufferOffset, length);
  }

  /// <inheritdoc />
  public override char GetChar(int ordinal) => GetString(ordinal)[0];

  /// <inheritdoc />
  public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
  {
    var text = GetString(ordinal);
    var totalLength = text.Length;

    if (buffer == null)
      return totalLength;

    if (dataOffset < 0 || dataOffset > totalLength)
      return 0;

    if (bufferOffset < 0 || length < 0 || bufferOffset + length > buffer.Length)
      throw new ArgumentOutOfRangeException(nameof(length), "Invalid buffer range specified.");

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
  /// <param name="column">The column.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public virtual Column GetColumn(int column) => Column[column];

  /// <inheritdoc />
  public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

  /// <summary>
  ///   Gets the column ordinal of the associated time column.
  /// </summary>
  /// <param name="column">The column that has teh association.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected int GetAssociatedTime(int column) => m_AssociatedTimeCol[column];

  /// <inheritdoc />
  public override DateTime GetDateTime(int ordinal)
  {
    var dateSpan = GetSpan(ordinal);
    var dateColumn = GetColumn(ordinal);
    // Add the associated Time column if set
    int timeColIndex = GetAssociatedTime(ordinal);
    bool hasTimeCol = timeColIndex > -1;

    var timePartValue = hasTimeCol ? GetCurrentValue(timeColIndex) : null;
    var timePartSpan = hasTimeCol ? GetSpan(timeColIndex) : ReadOnlySpan<char>.Empty;

    var dt = SpanToDateTime(dateColumn, GetCurrentValue(ordinal), dateSpan, timePartValue, timePartSpan, false);

    if (dt.HasValue)
      return dt.Value;

    throw WarnAddFormatException(ordinal,
      dateSpan.IsEmpty ? "Value is empty"
      : $"'{dateSpan.ToString()}' is not a date time format '{dateColumn.ValueFormat.DateFormat}'");
  }

  /// <inheritdoc />
  public override decimal GetDecimal(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    if (val is decimal or double or float or short or int or long)
      return Convert.ToDecimal(val, CultureInfo.CurrentCulture);

    var result = SpanToDecimal(GetColumn(ordinal), GetSpan(ordinal));
    return result ?? throw new FormatException($"'{GetString(ordinal)}' is not a decimal");
  }

  /// <inheritdoc />
  public override double GetDouble(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    if (val is decimal or double or float or short or int or long)
      return Convert.ToDouble(val, CultureInfo.CurrentCulture);

    var result = SpanToDouble(GetColumn(ordinal), GetSpan(ordinal));
    return result ?? throw new FormatException($"'{GetString(ordinal)}' is not a double");
  }

  /// <inheritdoc />
  public override IEnumerator GetEnumerator() => new DbEnumerator(this, true);

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override Type GetFieldType(int ordinal) => GetColumn(ordinal).ValueFormat.DataType.GetNetType();

  /// <inheritdoc />
  public override float GetFloat(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    try
    {
      if (val is decimal or double or float or short or int or long)
        return Convert.ToSingle(val, CultureInfo.CurrentCulture);
    }
    catch (Exception e)
    {
      throw WarnAddFormatException(ordinal, $"'{val}' is not a float, {e.Message}");
    }

    var result = SpanToDouble(GetColumn(ordinal), GetSpan(ordinal));
    return result.HasValue ? (float) result.Value
      : throw new FormatException($"'{GetString(ordinal)}' is not a float");
  }

  /// <inheritdoc />
  public override Guid GetGuid(int ordinal)
  {
    if (GetCurrentValue(ordinal) is Guid val)
      return val;

    _ = GetColumn(ordinal);
    return SpanToGuid(ordinal, GetSpan(ordinal))
      ?? throw new FormatException($"'{GetString(ordinal)}' is not an GUID");
  }

  /// <inheritdoc />
  public override short GetInt16(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    try
    {
      if (val is decimal or double or float or short or int or long or byte)
        return Convert.ToInt16(val, CultureInfo.CurrentCulture);
    }
    catch (Exception e)
    {
      throw WarnAddFormatException(ordinal, $"'{val}' is not a short, {e.Message}");
    }

    var result = SpanToLong(GetColumn(ordinal), GetSpan(ordinal));
    return result.HasValue ? (short) result.Value
      : throw new FormatException($"'{GetString(ordinal)}' is not a short");
  }

  /// <inheritdoc />
  public override int GetInt32(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    try
    {
      if (val is decimal or double or float or short or int or long or byte)
        return Convert.ToInt32(val, CultureInfo.CurrentCulture);
    }
    catch (Exception e)
    {
      throw WarnAddFormatException(ordinal, $"'{val}' is not an integer, {e.Message}");
    }

    var result = SpanToLong(GetColumn(ordinal), GetSpan(ordinal));
    return result.HasValue ? (int) result.Value
      : throw new FormatException($"'{GetString(ordinal)}' is not an integer");
  }

  /// <inheritdoc />
  public override long GetInt64(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    try
    {
      if (val is decimal or double or float or short or int or long or byte)
        return Convert.ToInt64(val, CultureInfo.CurrentCulture);
    }
    catch (Exception e)
    {
      throw WarnAddFormatException(ordinal, $"'{val}' is not a long, {e.Message}");
    }

    var result = SpanToLong(GetColumn(ordinal), GetSpan(ordinal));
    return result ?? throw new FormatException($"'{GetString(ordinal)}' is not an long");
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string GetName(int ordinal) => GetColumn(ordinal).Name;

  /// <inheritdoc />
  public override int GetOrdinal(string name)
  {
    if (string.IsNullOrEmpty(name))
      throw new ArgumentException("Column name must not be null or empty.", nameof(name));

    if (m_ColumnIndexMap.TryGetValue(name, out var index))
      return index;

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

      schemaRow[1] = column.Name;
      schemaRow[4] = column.Name;
      schemaRow[5] = col;
      schemaRow[19] = column.Ignore;

      schemaRow[7] = column.Convert && column.ValueFormat.DataType != DataTypeEnum.String
        ? column.ValueFormat.DataType.GetNetType()
        : typeof(string);

      dataTable.Rows.Add(schemaRow);
    }

    return dataTable;
  }

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
      return new FileStream(path: GetString(ordinal), mode: FileMode.Open, access: FileAccess.Read);
    return new MemoryStream(Encoding.UTF8.GetBytes(GetString(ordinal)));
  }

  /// <inheritdoc />
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string GetString(int ordinal) => m_CurrentRowColumnText[ordinal];

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

    var text = GetString(ordinal);

    if (GetColumn(ordinal).ValueFormat.DataType == DataTypeEnum.Binary)
      return File.Exists(text) ? File.OpenText(text) : new StringReader(string.Empty);

    return new StringReader(text);
  }

  /// <inheritdoc />
  public override object GetValue(int ordinal)
  {
    var column = GetColumn(ordinal);
    if (IsDBNull(column))
      return DBNull.Value;

    object ret;
    try
    {
      ret = column.ValueFormat.DataType switch
      {
        DataTypeEnum.DateTime => GetDateTime(ordinal),
        DataTypeEnum.Integer => IntPtr.Size == 4 ? GetInt32(ordinal) : GetInt64(ordinal),
        DataTypeEnum.Double => GetDouble(ordinal),
        DataTypeEnum.Numeric => GetDecimal(ordinal),
        DataTypeEnum.Boolean => GetBoolean(ordinal),
        DataTypeEnum.Guid => GetGuid(ordinal),
        _ => GetString(ordinal),
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

    if (m_CurrentValues is not null)
    {
      Array.Copy(m_CurrentValues, values, count);
      return count;
    }

    for (var ordinal = 0; ordinal < count; ordinal++)
      values[ordinal] = GetValue(ordinal);

    return count;
  }

  /// <summary>
  ///   Handles the Event if reading the file is completed
  /// </summary>
  public virtual void HandleReadFinished()
  {
    if (m_IsFinished)
      return;

    m_IsFinished = true;
    HandleShowProgress("Finished reading", 1);
    ReadFinished?.SafeInvoke(this);
  }

  /// <inheritdoc />
  public override bool IsDBNull(int ordinal)
  {
    if (ordinal < 0 || m_CurrentRowColumnText.Count <= ordinal)
      return true;
    return IsDBNull(GetColumn(ordinal));
  }

  /// <inheritdoc cref="DbDataReader" />
  public override bool NextResult() => false;

  /// <inheritdoc />
  public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => Task.FromResult(false);

  /// <inheritdoc cref="IDataReader" />
  [Obsolete("Use OpenAsync instead for asynchronous, non-blocking operation.")]
  public void Open()
    => OpenAsync(CancellationToken.None).GetAwaiter().GetResult();

  /// <inheritdoc />
  public abstract Task OpenAsync(CancellationToken cancellationToken);

  /// <inheritdoc />
  public sealed override bool Read()
    => ReadCoreAsync(CancellationToken.None).GetAwaiter().GetResult();

  /// <inheritdoc cref="IFileReader" />
  public sealed override Task<bool> ReadAsync(CancellationToken cancellationToken)
    => ReadCoreAsync(cancellationToken).AsTask();

  /// <summary>
  ///   Resets the position to the first data row.
  /// </summary>
  public virtual ValueTask ResetPositionToFirstDataRowAsync(CancellationToken cancellationToken)
  {
    EndLineNumber = 0;
    RecordNumber = 0;
    EndOfFile = false;
    return default;
  }

  /// <summary>
  /// Adds a value to the collection
  /// </summary>
  /// <param name="text">The string representation of the value as a <see cref="ReadOnlySpan{Char}"/>.</param>
  /// <param name="typedValue">The underlying object value</param>
  /// <returns>The zero-based index at which the value was added.</returns>

  protected void Add(ReadOnlySpan<char> text, object? typedValue = null)
  {
    m_CurrentRowColumnText.Add(text);
    if (m_CurrentValues is null)
    {
      if (typedValue is not null)
        throw new InvalidOperationException("Cannot add a typed value if the reader is not supposed to store typed values");
    }
    else
    {
      m_CurrentValues[m_CurrentRowColumnText.Count - 1] = typedValue;
    }
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
  /// Clears the current row data
  /// </summary>
  protected void Clear()
  {
    m_CurrentRowColumnText.Clear();

    if (m_CurrentValues is not null)
      Array.Clear(m_CurrentValues, 0, m_CurrentValues.Length);
  }

  /// <inheritdoc />
  protected override void Dispose(bool disposing)
  {
    if (disposing)
      m_CurrentRowColumnText.Dispose();
    base.Dispose(disposing);
  }

  /// <summary>
  ///   Does set EndOfFile sets the max value for Progress and stores the now known reader columns
  /// </summary>
  protected void FinishOpen()
  {
    m_IsFinished = false;
    EndOfFile = false;

    m_ColumnIndexMap.Clear();
    for (var i = 0; i < m_FieldCount; i++)
      m_ColumnIndexMap[Column[i].Name] = i;

    OpenFinished?.SafeInvoke(this, Column);
  }

  /// <summary>
  /// Gets the current object stored in <see cref="m_CurrentValues"/>.
  /// </summary>
  /// <param name="ordinal">The zero-based column ordinal.</param>
  /// <returns>The value at the specified ordinal.</returns>
  /// <exception cref="IndexOutOfRangeException">Thrown if the ordinal is outside the bounds of the data row.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected object? GetCurrentValue(int ordinal) => m_CurrentValues?[ordinal];

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
  ///   Gets the originally provided text of a column
  /// </summary>
  /// <param name="ordinal">The column number.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected ReadOnlySpan<char> GetSpan(int ordinal) => m_CurrentRowColumnText.GetSpan(ordinal);

  /// <summary>
  ///   Calls the event handler for errors <see cref="Warning"/>
  /// </summary>
  /// <param name="ordinal">The column ordinal number.</param>
  /// <param name="message">The message to raise.</param>
  protected void HandleError(int ordinal, ReadOnlySpan<char> message)
  {
    if (ordinal >= 0 && ordinal < Column.Length && GetColumn(ordinal).Ignore)
      return;

    Warning?.SafeInvoke(this, GetWarningEventArgs(ordinal, message));
  }

  /// <summary>
  ///   Shows the process.
  /// </summary>
  /// <param name="text">Leading Text</param>
  /// <param name="percent">Value between 0 and 1 representing the relative position</param>
  protected void HandleShowProgress(ReadOnlySpan<char> text, double percent) =>
    m_ReportProgress.Report(new ProgressInfo(text, (percent * cMaxProgress).ToInt64()));

  /// <summary>
  ///   Shows the process twice a second
  /// </summary>
  /// <param name="text">Leading Text</param>
  protected void HandleShowProgressPeriodic(string text)
    => m_IntervalAction.Invoke(() => HandleShowProgress(text, GetRelativePosition()));

  /// <summary>
  /// Orchestrates text processing including formatting, null-equivalent detection, 
  /// trimming, and non-breaking space replacement.
  /// </summary>
  /// <param name="textSpan">The raw character span to process.</param>
  /// <param name="columnNo">The column index for metadata and formatter lookups.</param>
  /// <returns>A processed span that is potentially trimmed, formatted, or cleaned.</returns>
  protected string HandleText(ReadOnlySpan<char> textSpan, int columnNo)
  {
    if (textSpan.IsEmpty)
      return string.Empty;

    foreach (var t in m_TreatAsNullMemories)
    {
      if (textSpan.Equals(t.Span, StringComparison.Ordinal))
        return string.Empty;
    }

    var columnFormatter = GetColumn(columnNo).ColumnFormatter;
    var text = !ReferenceEquals(columnFormatter, EmptyFormatter.Instance)
      ? columnFormatter.FormatInputText(textSpan, msg => HandleWarning(columnNo, msg))
      : textSpan.ToString();

    if (string.IsNullOrEmpty(text))
      return string.Empty;

    var processedSpan = text.AsSpan();

    if (m_Trim)
      processedSpan = processedSpan.Trim();

    if (processedSpan.IsEmpty)
      return string.Empty;

    if (m_TreatNbspAsSpace)
    {
      var firstNbsp = processedSpan.IndexOf('\u00A0');
      if (firstNbsp != -1)
      {
        var buffer = processedSpan.ToArray();
        for (var i = firstNbsp; i < buffer.Length; i++)
        {
          if (buffer[i] == '\u00A0')
            buffer[i] = ' ';
        }

        return new string(buffer);
      }
    }

    return processedSpan.Length == text.Length ? text : processedSpan.ToString();
  }

  /// <summary>
  ///   Calls the event handler for warnings <see cref="Warning"/>
  /// </summary>
  /// <param name="ordinal">The column ordinal number</param>
  /// <param name="message">The message to raise.</param>
  protected void HandleWarning(int ordinal, string message) => HandleError(ordinal, message.AddWarningId());

  /// <summary>
  ///   Displays progress, is called after <see langword="abstract" /> row has been read
  /// </summary>
  /// <param name="hasReadRow"><c>true</c> if a row has been read</param>
  protected void InfoDisplay(bool hasReadRow)
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
      throw new ArgumentException($"Too many columns: {fieldCount}. Maximum supported: 1000", nameof(fieldCount));

    m_FieldCount = fieldCount;
    m_ColumnIndexMap.Clear();
    Column = new Column[fieldCount];
    // m_CurrentValues is set to empty array in constructor if we store values 
    if (m_CurrentValues is not null)
      m_CurrentValues = new object[fieldCount];

    m_AssociatedTimeCol = new int[fieldCount];
    m_AssociatedTimeZoneCol = new int[fieldCount];
    m_ParseFromSource = new bool[fieldCount];

    for (var counter = 0; counter < fieldCount; counter++)
    {
      Column[counter] = new Column(GetDefaultName(counter), ValueFormat.Empty, counter);
      m_AssociatedTimeCol[counter] = -1;
      m_AssociatedTimeZoneCol[counter] = -1;
    }
  }

  /// <summary>
  /// Internal method to determine if 
  /// the value of a column in the current row should be treated as <see cref="DBNull"/>.
  /// </summary>
  /// <param name="column">The column metadata providing the index and formatting rules.</param>
  /// <returns>
  /// <c>true</c> if the column is ignored, contains whitespace, or (for split Date-Time columns) 
  /// if both the date and associated time components are empty; otherwise, <c>false</c>.
  /// </returns>
  /// <remarks>
  /// For <see cref="DataTypeEnum.DateTime"/>, this method checks if a split time column exists 
  /// via <c>AssociatedTimeCol</c>. If so, both parts must be empty for the result to be <c>true</c>.
  /// </remarks>
  private bool IsDBNull(Column column)
  {
    if (column.Ignore)
      return true;

    var ordinal = column.ColumnOrdinal;
    var span = GetSpan(ordinal);

    if (m_CurrentValues is not null)
    {
      var dataType = column.ValueFormat.DataType;

      if (dataType == DataTypeEnum.DateTime)
      {
        if (m_CurrentValues[ordinal] is not null || !span.IsEmpty)
          return false;

        var timeOrdinal = m_AssociatedTimeCol[ordinal];

        if ((uint) timeOrdinal >= (uint) m_CurrentValues.Length)
          return true;

        return m_CurrentValues[timeOrdinal] is null && GetSpan(timeOrdinal).IsEmpty;
      }

      if (dataType == DataTypeEnum.String)
        return span.IsEmpty;

      return m_CurrentValues[ordinal] is null && span.IsEmpty;
    }

    if (column.ValueFormat.DataType == DataTypeEnum.DateTime)
    {
      if (!span.IsEmpty)
        return false;

      var timeOrdinal = m_AssociatedTimeCol[ordinal];

      return (uint) timeOrdinal >= (uint) m_CurrentRowColumnText.Count || GetSpan(timeOrdinal).IsEmpty;
    }

    return span.IsEmpty;
  }

  /// <summary>
  /// Parses the column names and sets their data types. Handles TimePart and TimeZone associations.
  /// Column array must be initialized beforehand.
  /// </summary>
  /// <param name="headerRow">The header row.</param>
  /// <param name="dataType">Optionally provided data types.</param>
  /// <param name="hasFieldHeader">if set to <c>true</c> if a file has field header.</param>

  protected void ParseColumnName(IEnumerable<string> headerRow, in IEnumerable<DataTypeEnum>? dataType = null, bool hasFieldHeader = true)
  {
    var adjustedNames = hasFieldHeader
      ? AdjustColumnName(headerRow, Column.Length)
      : [.. Enumerable.Range(0, Column.Length)
        .Select(colIndex =>
        {
          var colName = GetColumn(colIndex).Name;
          if (colName.Equals(GetDefaultName(colIndex), StringComparison.OrdinalIgnoreCase))
          {
            var def = m_ColumnDefinition.FirstOrDefault(x => x.ColumnOrdinal == colIndex);
            if (def != null && !string.IsNullOrEmpty(def.Name))
            {
              HandleError(colIndex, "Using column name from definition");
              return def.Name;
            }
          }

          return colName;
        })];

    var dataTypeL = new DataTypeEnum[adjustedNames.Count];

    for (var col = 0; col < adjustedNames.Count; col++)
      dataTypeL[col] = DataTypeEnum.String;

    if (dataType != null)
    {
      using var enumeratorType = dataType.GetEnumerator();
      var col = 0;
      while (enumeratorType.MoveNext() && col < adjustedNames.Count)
        dataTypeL[col++] = enumeratorType.Current;
    }

    for (var colIndex = 0; colIndex < adjustedNames.Count && colIndex < Column.Length; colIndex++)
    {
      var defined = m_ColumnDefinition.FirstOrDefault(x => x.Name.Equals(adjustedNames[colIndex], StringComparison.OrdinalIgnoreCase)) ??
                    new Column(adjustedNames[colIndex], new ValueFormat(dataTypeL[colIndex]), colIndex);

      Column[colIndex] = new Column(
        adjustedNames[colIndex],
        defined.ValueFormat,
        colIndex,
        defined.Ignore,
        defined.Convert,
        defined.DestinationName,
        defined.TimePart,
        defined.TimePartFormat,
        defined.TimeZonePart);
    }

    if (Column.Length == 0)
    {
      HandleWarning(-1, "Column should be set before using ParseColumnName to handle TimePart and TimeZone");
      return;
    }

    for (var index = 0; index < Column.Length; index++)
      m_ParseFromSource[index] = false;

    for (var index = 0; index < Column.Length; index++)
    {
      var column = GetColumn(index);
      if (column.Ignore)
        continue;

      m_ParseFromSource[index] = true;

      var searchedTimePart = column.TimePart;
      var searchedTimeZonePart = column.TimeZonePart;

      if (!string.IsNullOrEmpty(searchedTimePart))
      {
        for (var indexPoint = 0; indexPoint < Column.Length; indexPoint++)
        {
          if (indexPoint == index)
            continue;

          if (!GetColumn(indexPoint).Name.Equals(searchedTimePart, StringComparison.OrdinalIgnoreCase))
            continue;

          m_AssociatedTimeCol[index] = indexPoint;
          m_ParseFromSource[indexPoint] = true;
          break;
        }
      }

      if (string.IsNullOrEmpty(searchedTimeZonePart))
        continue;

      for (var indexPoint = 0; indexPoint < Column.Length; indexPoint++)
      {
        if (indexPoint == index)
          continue;

        if (!GetColumn(indexPoint).Name.Equals(searchedTimeZonePart, StringComparison.OrdinalIgnoreCase))
          continue;

        m_AssociatedTimeZoneCol[index] = indexPoint;
        m_ParseFromSource[indexPoint] = true;
        break;
      }
    }
  }

  /// <summary>
  /// Performs the actual read operation for the data reader.
  /// </summary>
  /// <param name="cancellationToken">
  /// A token that can be used to cancel the read operation.
  /// Implementations should observe this token and stop reading promptly when cancellation is requested.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{Boolean}"/> that completes with <c>true</c> if a record was read successfully;
  /// <c>false</c> if the end of the data source was reached, reading was canceled, or the reader was closed.
  /// </returns>
  /// <remarks>
  /// This method represents the core read logic and must be implemented by derived classes.
  /// The base class provides the public <see cref="Read"/> and <see cref="ReadAsync(CancellationToken)"/> methods
  /// and delegates to this method to avoid sync-over-async and async-over-sync implementations.
  /// </remarks>/>
  protected abstract ValueTask<bool> ReadCoreAsync(CancellationToken cancellationToken);

  /// <summary>
  /// Determines if a column should be parsed
  /// </summary>
  protected bool ShouldParseFromSource(int columnNo) =>
    m_ParseFromSource.Length == 0 || columnNo >= m_ParseFromSource.Length || m_ParseFromSource[columnNo];

  /// <summary>
  /// Checks if we should retry to access the data
  /// </summary>
  /// <param name="ex">The exception.</param>
  /// <param name="token">The cancellation token.</param>
  protected bool ShouldRetry(in Exception ex, CancellationToken token)
  {
    if (token.IsCancellationRequested)
      return false;

    var eventArgs = new RetryEventArgs(ex) { Retry = false };
    var handler = Volatile.Read(ref OnAskRetry);
    handler?.Invoke(this, eventArgs);
    return eventArgs.Retry;
  }

  /// <summary>
  ///   Gets the boolean value.
  /// </summary>
  /// <param name="column">The column metadata used to determine formatting (separators).</param>
  /// <param name="inputValue">The character span containing the raw string value to parse.</param>
  /// <returns>
  ///   The Boolean, if conversion is not successful: <c>NULL</c> the event handler for warnings
  ///   is called
  /// </returns>
  protected bool? SpanToBoolean(in Column column, ReadOnlySpan<char> inputValue)
  {
    var boolValue = inputValue.StringToBoolean(column.ValueFormat.True.AsSpan(), column.ValueFormat.False.AsSpan());
    if (boolValue.HasValue)
      return boolValue.Value;

    HandleError(column.ColumnOrdinal, $"'{inputValue.ToString()}' is not a boolean ({column.ValueFormat.True}/{column.ValueFormat.False})");
    return null;
  }

  /// <summary>
  ///   This routine will read a date from a typed or untyped reader, will combine date with time,
  ///   and apply timeZone adjustments.
  /// </summary>
  /// <param name="column">The column metadata used to determine formatting (separators).</param>
  /// <param name="inputDate">The original object possibly containing a date</param>
  /// <param name="strInputDate">The text representation of the data</param>
  /// <param name="inputTime">The original object possibly containing a time</param>
  /// <param name="strInputTime">The text representation of the time</param>
  /// <param name="serialDateTime">if <c>true</c> parse dates represented as numbers</param>
  protected DateTime? SpanToDateTime(
    Column column,
    in object? inputDate,
    ReadOnlySpan<char> strInputDate,
    in object? inputTime,
    ReadOnlySpan<char> strInputTime,
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
      ReadOnlySpan<char> passedIn = strInputTime;
      if (inputTime != null)
        passedIn = Convert.ToString(inputTime).AsSpan();

      HandleWarning(column.ColumnOrdinal, $"'{passedIn.ToString()}' is outside expected range 00:00 - 23:59, the date has been adjusted");
    }

    if (!dateTime.HasValue &&
        !strInputDate.IsEmpty &&
        !string.IsNullOrEmpty(column.ValueFormat.DateFormat) &&
        strInputDate.Length > column.ValueFormat.DateFormat.Length)
    {
      var inputDateNew = strInputDate.Slice(0, column.ValueFormat.DateFormat.Length);
      dateTime = inputDateNew.CombineStringsToDateTime(
        column.ValueFormat.DateFormat.AsSpan(),
        strInputTime,
        column.ValueFormat.DateSeparator,
        column.ValueFormat.TimeSeparator,
        serialDateTime);

      if (dateTime.HasValue)
      {
        var display1 = column.ValueFormat.DateFormat.ReplaceDefaults(
          '/', column.ValueFormat.DateSeparator,
          ':', column.ValueFormat.TimeSeparator);

        HandleWarning(column.ColumnOrdinal, strInputTime.Length > 0
          ? $"'{strInputDate.ToString()} {strInputTime.ToString()}' is not a date of the format '{display1}' '{column.TimePartFormat}', used '{inputDateNew.ToString()} {strInputTime.ToString()}'"
          : $"'{strInputDate.ToString()}' is not a date of the format '{display1}', used '{inputDateNew.ToString()}' ");
      }
    }

    if (dateTime?.Year is > 1752 and <= 9999)
    {
      // gte the timezone from TimeZonePart or the assiciated column
      if (!column.TimeZonePart.TryGetConstant(out var timeZone))
      {
        var associatedColIdx = m_AssociatedTimeZoneCol[column.ColumnOrdinal];
        if (associatedColIdx > -1)
          timeZone = GetSpan(associatedColIdx);
      }

      return TimeZoneAdjust(dateTime.Value, timeZone, ReturnedTimeZone, message => HandleWarning(column.ColumnOrdinal, message));
    }

    var display2 = column.ValueFormat.DateFormat.ReplaceDefaults(
      '/', column.ValueFormat.DateSeparator, ':', column.ValueFormat.TimeSeparator);

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
  /// <param name="column">The column metadata used to determine formatting (separators).</param>
  /// <param name="inputValue">The character span containing the raw string value to parse.</param>
  protected decimal? SpanToDecimal(in Column column, ReadOnlySpan<char> inputValue)
  {
    var decimalValue = inputValue.StringToDecimal(
      column.ValueFormat.DecimalSeparator,
      column.ValueFormat.GroupSeparator,
      m_AllowPercentage,
      m_RemoveCurrency);

    if (decimalValue.HasValue)
      return decimalValue.Value;

    HandleError(column.ColumnOrdinal, $"'{inputValue.ToString()}' is not a decimal");
    return null;
  }

  /// <summary>
  /// Gets the double value or null from a character span.
  /// </summary>
  /// <param name="column">The column metadata used to determine formatting (separators).</param>
  /// <param name="inputValue">The character span containing the raw string value to parse.</param>
  protected double? SpanToDouble(in Column column, ReadOnlySpan<char> inputValue)
  {
    var ret = inputValue.StringToDouble(
      column.ValueFormat.DecimalSeparator,
      column.ValueFormat.GroupSeparator,
      m_AllowPercentage,
      m_RemoveCurrency);

    if (ret.HasValue)
      return ret.Value;

    HandleError(column.ColumnOrdinal, $"'{inputValue.ToString()}' is not a double");
    return null;
  }

  /// <summary>
  ///   Gets the Guid value or null.
  /// </summary>
  /// <param name="ordinal">The column ordinal index used for error handling.</param>
  /// <param name="inputValue">The character span containing the raw string value to parse.</param>
  protected Guid? SpanToGuid(int ordinal, ReadOnlySpan<char> inputValue)
  {
    if (inputValue.IsEmpty)
      return null;

    var res = inputValue.StringToGuid();
    if (res.HasValue)
      return res.Value;

    HandleError(ordinal, $"'{inputValue.ToString()}' is not a GUID");
    return null;
  }

  /// <summary>
  /// Parses a 64-bit signed integer from the provided character span.
  /// </summary>
  /// <param name="column">The column metadata containing formatting rules and the ordinal for error reporting.</param>
  /// <param name="inputValue">The read-only character span to parse.</param>
  /// <returns>The parsed <see cref="long"/> value if successful; otherwise, <c>null</c>.</returns>
  /// <remarks>
  /// If parsing fails, <see cref="HandleError"/> is invoked before returning <c>null</c>.
  /// </remarks>
  protected long? SpanToLong(in Column column, ReadOnlySpan<char> inputValue)
  {
    var ret = inputValue.StringToInt64(column.ValueFormat.GroupSeparator);
    if (ret.HasValue)
      return ret.Value;

    HandleError(column.ColumnOrdinal, $"'{inputValue.ToString()}' is not a integer");
    return null;
  }

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

  /// <summary>
  ///   Does look at the provided column names and checks them for valid entry, makes sure the
  ///   column names are unique and not empty, have the right size etc.
  /// </summary>
  /// <param name="columns">The columns as read / provided</param>
  /// <param name="fieldCount">
  ///   The maximum number of fields, if more than this number are provided, it will ignore these columns
  /// </param>
  private List<string> AdjustColumnName(IEnumerable<string> columns, int fieldCount)
  {
    var newNames = new List<string>(fieldCount);
    var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var counter = 0;

    foreach (var column in columns)
    {
      if (counter >= fieldCount)
        break;

      var trimmed = column.Trim();
      string resultingName;

      if (trimmed.Length == 0)
      {
        resultingName = GetDefaultName(counter);
        HandleWarning(counter, $"Column title was empty, set to {resultingName}.");
      }
      else
      {
        resultingName = trimmed;

        if (!string.Equals(column, trimmed, StringComparison.OrdinalIgnoreCase))
          HandleWarning(counter, $"Column title '{column}' had leading or trailing spaces, removed.");

        if (resultingName.Length > 128)
        {
          var preview = resultingName.Length > 20 ? resultingName.Substring(0, 20) + "…" : resultingName;
          resultingName = resultingName.Substring(0, 128);
          HandleWarning(counter, $"Column title '{preview}' too long, cut off after 128 characters.");
        }

        var uniqueName = existingNames.MakeUniqueInCollection(resultingName);
        if (!string.Equals(uniqueName, resultingName, StringComparison.OrdinalIgnoreCase))
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
  /// Gets the warning event arguments.
  /// </summary>
  /// <param name="ordinal">The ordinal.</param>
  /// <param name="message">The message.</param>
  private WarningEventArgs GetWarningEventArgs(int ordinal, ReadOnlySpan<char> message) =>
    new WarningEventArgs(
      RecordNumber,
      ordinal,
      message,
      StartLineNumber,
      EndLineNumber,
      ordinal >= 0 && ordinal < m_FieldCount ? GetColumn(ordinal).Name : null);

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
  public new virtual Task CloseAsync() => Task.Run(() => base.Close());
#endif
}