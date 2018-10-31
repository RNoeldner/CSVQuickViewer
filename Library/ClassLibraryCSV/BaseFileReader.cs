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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;

namespace CsvTools
{
  /// <summary>
  ///   Abstract class as base for all DataReaders
  /// </summary>
  public abstract class BaseFileReader
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
    {
      cRecordNumberFieldName,
      cStartLineNumberFieldName,
      cEndLineNumberFieldName,
      cErrorField
    };

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

    private CancellationToken m_CancellationToken;
    private int m_FieldCount;
    private IProcessDisplay m_ProcessDisplay;

    protected BaseFileReader(IFileSetting fileSetting)
    {
      Debug.Assert(fileSetting != null);
      m_FileSetting = fileSetting;
    }

    /// <summary>
    ///   A cancellation token, to stop long running processes
    /// </summary>
    public virtual CancellationToken CancellationToken
    {
      get => m_CancellationToken;
      set => m_CancellationToken = value;
    }

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
    ///   Gets the end name of the line number field.
    /// </summary>
    /// <value>The end name of the line number field.</value>
    public virtual string EndLineNumberFieldName => cEndLineNumberFieldName;

    /// <summary>
    ///   Gets or sets a value indicating whether the reader is at the end of the file.
    /// </summary>
    /// <value><c>true</c> if at the end of file; otherwise, <c>false</c>.</value>
    public virtual bool EndOfFile { get; protected set; } = true;

    /// <summary>
    ///   Gets the field name for persisted error information
    /// </summary>
    /// <value>The error field.</value>
    public virtual string ErrorField => cErrorField;

    /// <summary>
    ///   Gets the number of fields in the file.
    /// </summary>
    /// <value>Number of field in the file.</value>
    public virtual int FieldCount
    {
      get => m_FieldCount;
      protected set => m_FieldCount = value;
    }

    /// <summary>
    ///   A Process Display
    /// </summary>
    public virtual IProcessDisplay ProcessDisplay
    {
      get => m_ProcessDisplay;
      set
      {
        m_ProcessDisplay = value;
        if (m_ProcessDisplay != null) m_CancellationToken = m_ProcessDisplay.CancellationToken;
      }
    }

    /// <summary>
    ///   Gets the record number.
    /// </summary>
    /// <value>The record number.</value>
    public virtual long RecordNumber { get; protected set; }

    /// <summary>
    ///   Gets the name of the record number field.
    /// </summary>
    /// <value>The name of the record number field.</value>
    public virtual string RecordNumberFieldName => cRecordNumberFieldName;

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
    ///   Gets the start name of the line number field.
    /// </summary>
    /// <value>The start name of the line number field.</value>
    public virtual string StartLineNumberFieldName => cStartLineNumberFieldName;

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
    ///   Event to be raised if reading the files is completed
    /// </summary>
    public virtual event EventHandler ReadFinished;

    /// <summary>
    ///   Event handler called if a warning or error occurred
    /// </summary>
    public virtual event EventHandler<WarningEventArgs> Warning;

    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting
    ///   unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      Close();
      HandleReadFinished();
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    ///   Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing">
    ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
    ///   unmanaged resources.
    /// </param>
    public abstract void Dispose(bool disposing);

    /// <summary>
    ///   Gets the boolean.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    public abstract bool GetBoolean(int i);

    /// <summary>
    ///   Gets the column format.
    /// </summary>
    /// <param name="columnNumber">The column.</param>
    /// <returns></returns>
    public virtual Column GetColumn(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0);
      Debug.Assert(Column != null);
      Debug.Assert(columnNumber < Column.Length);
      return Column[columnNumber];
    }

    /// <summary>
    ///   Gets the date time.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    public abstract DateTime GetDateTime(int i);

    /// <summary>
    ///   Gets the decimal.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    public abstract decimal GetDecimal(int i);

    /// <summary>
    ///   Gets the double.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    public abstract double GetDouble(int i);

    /// <summary>
    ///   Gets the type of the field.
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <returns>The .NET type of the column</returns>
    public virtual Type GetFieldType(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0);
      Debug.Assert(columnNumber < FieldCount);
      return GetColumn(columnNumber).DataType.GetNetType();
    }

    /// <summary>
    ///   Gets the unique identifier.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    public abstract Guid GetGuid(int i);

    /// <summary>
    ///   Gets the int32.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    public abstract int GetInt32(int i);

    /// <summary>
    ///   Gets the int64.
    /// </summary>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    public abstract long GetInt64(int i);

    /// <summary>
    ///   Gets the name for the field to find.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>The name of the field or the empty string (""), if there is no value to return.</returns>
    /// <exception cref="T:System.IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount" />.
    /// </exception>
    public virtual string GetName(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      return GetColumn(columnNumber).Name;
    }

    /// <summary>
    ///   Return the index of the named field.
    /// </summary>
    /// <param name="columnName">The name of the field to find.</param>
    /// <returns>The index of the named field. If not found -1</returns>
    public virtual int GetOrdinal(string columnName)
    {
      Debug.Assert(columnName != null);

      if (string.IsNullOrEmpty(columnName) || Column == null) return -1;
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
    ///   Returns a <see cref="T:System.Data.DataTable" /> that describes the column meta data of the
    ///   <see cref="T:System.Data.IDataReader" />.
    /// </summary>
    /// <returns>A <see cref="T:System.Data.DataTable" /> that describes the column meta data.</returns>
    /// <exception cref="T:System.InvalidOperationException">
    ///   The <see cref="T:System.Data.IDataReader" /> is closed.
    /// </exception>
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
            /* TextToHTML and  TextToHTMLFull have been handled in the CSV Reader as the length of the fields would change */
            ret = CurrentRowColumnText[columnNumber];
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
    ///   Checks if the column should be read
    /// </summary>
    /// <param name="columnNumber">The column number.</param>
    /// <returns><c>true</c> if this column should not be read</returns>
    public virtual bool IgnoreRead(int columnNumber)
    {
      return GetColumn(columnNumber).Ignore;
    }

    /// <summary>
    ///   Displays progress
    /// </summary>
    /// <param name="hasReadRow"><c>true</c> if a row has been read</param>
    public virtual void InfoDisplay(bool hasReadRow)
    {
      if (!hasReadRow)
        HandleReadFinished();
      else
        HandleShowProgressPeriodic("Reading from source", RecordNumber);
    }

    /// <summary>
    ///   Return whether the specified field is set to null.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>
    ///   true if the specified field is set to null; otherwise, false.
    /// </returns>
    /// <exception cref="T:System.IndexOutOfRangeException">
    ///   The index passed was outside the range of 0 through
    ///   <see cref="P:System.Data.IDataRecord.FieldCount" />.
    /// </exception>
    public virtual bool IsDBNull(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      if (CurrentRowColumnText == null || CurrentRowColumnText.Length <= columnNumber)
        return true;
      if (Column[columnNumber].DataType == DataType.DateTime)
      {
        if (AssociatedTimeCol[columnNumber] == -1)
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
    public virtual bool NextResult()
    {
      return false;
    }

    /// <summary>
    ///   Opens the text file and begins to read the meta data, like columns
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="determineColumnSize">Not supported with Excel files</param>
    /// <param name="handleRemoteFile"></param>
    /// <returns></returns>
    /// <exception cref="System.IO.FileNotFoundException"></exception>
    [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public long Open(CancellationToken cancellationToken, bool determineColumnSize)
    {
      m_CancellationToken = cancellationToken;
      try
      {
        if (ApplicationSetting.RemoteFileHandler != null && m_FileSetting is IFileSettingRemoteDownload remote && !m_FileSetting.FileName.IsSFTP())
        {
          if (!string.IsNullOrEmpty(remote?.RemoteFileName))
          {
            try
            {
              HandleShowProgress("Handling Remote file ...");
              ApplicationSetting.RemoteFileHandler(remote.RemoteFileName, m_FileSetting.FullPath, ProcessDisplay, remote.ThrowErrorIfNotExists);
            }
            catch (Exception)
            {
              if (remote.ThrowErrorIfNotExists)
              {
                throw;
              }
            }
          }
        }

        if (m_ProcessDisplay != null)
        {
          m_ProcessDisplay.Maximum = cMaxValue;
        }

        HandleShowProgress("Opening...");

        if (m_CancellationToken.IsCancellationRequested)
        {
          return 0;
        }

        var ret = IndividualOpen(determineColumnSize);

        var valuesInclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var valuesAll = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var colindex = 0; colindex < FieldCount; colindex++)
        {
          if (!string.IsNullOrEmpty(Column[colindex].Name))
          {
            valuesAll.Add(Column[colindex].Name);
            if (!Column[colindex].Ignore)
            {
              valuesInclude.Add(Column[colindex].Name);
            }
          }
        }

        CsvHelper.CacheColumnHeader(m_FileSetting, true, valuesAll);
        CsvHelper.CacheColumnHeader(m_FileSetting, false, valuesInclude);

        if (FieldCount > 0)
        {
          // Override the column settings and store the columns for later reference
          OverrideColumnFormatFromSetting(FieldCount);
        }

        return ret;
      }
      catch (Exception ex)
      {
        var appEx = new ApplicationException(
          m_FileSetting is IFileSettingRemoteDownload // ICsvFile or IExcelFile more meaning full interface is not defined here
            ? "Can not access service for reading.\nPlease make sure the service is reachable and does react to calls."
            : "Can not open file for reading.\nPlease make sure the file does exist, is of the right type and is not locked by another process.",
          ex);
        HandleError(-1, appEx.ExceptionMessages());
        HandleReadFinished();
        throw appEx;
      }
      finally
      {
        HandleShowProgress("");
      }
    }

    /// <summary>
    ///   Overrides the column format from setting.
    /// </summary>
    /// <param name="fieldCount">The field count.</param>
    public virtual void OverrideColumnFormatFromSetting(int fieldCount)
    {
      Debug.Assert(AssociatedTimeCol != null);

      for (var colindex = 0; colindex < FieldCount; colindex++)
      {
        var setting = m_FileSetting.GetColumn(Column[colindex].Name);
        if (setting != null)
        {
          setting.CopyTo(Column[colindex]);
          // Copy to has replaced he column Ordinal but this should be kept...
          Column[colindex].ColumnOrdinal = colindex;
        }

        if (Column[colindex].DataType != DataType.DateTime || setting == null || !setting.Convert ||
            setting.Ignore)
        {
          continue;
        }

        AssociatedTimeCol[colindex] = GetOrdinal(Column[colindex].TimePart);
        AssociatedTimeZoneCol[colindex] = GetOrdinal(Column[colindex].TimeZonePart);
      }
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

    /// <summary>
    ///   Gets the boolean value.
    /// </summary>
    /// <param name="inputBoolean">The input.</param>
    /// <param name="columnNumber">The column.</param>
    /// <returns>
    ///   The Boolean, if conversion is not successful: <c>NULL</c> the event handler for warnings is called
    /// </returns>
    protected virtual bool? GetBooleanNull(string inputBoolean, int columnNumber)
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

    protected virtual DateTime? GetDateTimeNull(object inputDate, string strInputDate, object inputTime,
      string strInputTime, Column column)
    {
      var dateTime = StringConversion.CombineObjectsToDateTime(inputDate, strInputDate, inputTime, strInputTime,
        ApplicationSetting.FillGuessSettings.SerialDateTime, column, out var timeSpanLongerThanDay);
      if (timeSpanLongerThanDay)
      {
        var passedIn = strInputTime;
        if (inputTime != null)
          passedIn = inputTime.ToString();
        HandleWarning(column.ColumnOrdinal, $"'{passedIn}' is outside expected range 00:00 - 23:59, the date has been adjusted");
      }
      if (!dateTime.HasValue && !string.IsNullOrEmpty(strInputDate) && !string.IsNullOrEmpty(column.DateFormat) &&
          strInputDate.Length > column.DateFormat.Length)
      {
        var inputDateNew = strInputDate.Substring(0, column.DateFormat.Length);
        dateTime = StringConversion.CombineStringsToDateTime(inputDateNew, column.DateFormat, strInputTime,
          column.DateSeparator, column.TimeSeparator, ApplicationSetting.FillGuessSettings.SerialDateTime);
        if (dateTime.HasValue)
        {
          var disp = column.DateFormat.ReplaceDefaults("/", column.DateSeparator, ":", column.TimeSeparator);
          HandleWarning(column.ColumnOrdinal,
            !string.IsNullOrEmpty(strInputTime)
              ? $"'{strInputDate} {strInputTime}' is not a date of the format {disp} {column.TimePartFormat}, used '{inputDateNew} {strInputTime}'"
              : $"'{strInputDate}' is not a date of the format {disp}, used '{inputDateNew}' ");
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
    protected virtual decimal? GetDecimalNull(string inputValue, Column column)
    {
      Debug.Assert(column != null);
      var decimalValue =
        StringConversion.StringToDecimal(inputValue, column.DecimalSeparatorChar, column.GroupSeparatorChar, true);
      if (decimalValue.HasValue)
        return decimalValue.Value;

      HandleError(column.ColumnOrdinal,
        $"'{inputValue}' is not a floating point value");
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
    protected virtual decimal? GetDecimalNull(string inputValue, int columnNumber)
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
    protected virtual double? GetDoubleNull(string inputValue, Column column)
    {
      Debug.Assert(column != null);
      var decimalValue = GetDecimalNull(inputValue, column);
      if (decimalValue.HasValue)
        return decimal.ToDouble(decimalValue.Value);
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
    protected virtual short GetInt16(string value, int columnNumber)
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
    ///   Gets the integer value
    /// </summary>
    /// <param name="value">The input.</param>
    /// <param name="columnNumber">The column number for retrieving the Format information.</param>
    /// <returns>
    ///   The parsed value if conversion is not successful: <c>NULL</c> is returned and the event
    ///   handler for warnings is called
    /// </returns>
    protected virtual int GetInt32(string value, int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      var column = GetColumn(columnNumber);

      var parsed = StringConversion.StringToInt32(value, column.DecimalSeparatorChar, column.GroupSeparatorChar);
      if (parsed.HasValue)
        return parsed.Value;

      // Warning was added by GetInt32Null
      throw WarnAddFormatException(columnNumber,
        $"'{value}' is not an integer");
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
    protected virtual long GetInt64(string value, int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      var column = GetColumn(columnNumber);

      var parsed = StringConversion.StringToInt64(value, column.DecimalSeparatorChar, column.GroupSeparatorChar);
      if (parsed.HasValue)
        return parsed.Value;

      throw WarnAddFormatException(columnNumber,
        $"'{value}' is not an long integer");
    }

    /// <summary>
    ///   Gets the part of a text
    /// </summary>
    /// <param name="inputValue">The input.</param>
    /// <param name="column">The column.</param>
    /// <returns>
    ///   The parsed value if conversion is not successful: <c>NULL</c> is returned and the event
    ///   handler for warnings is called
    /// </returns>
    public virtual string GetPart(string inputValue, Column column)
    {
      Debug.Assert(column != null);
      return StringConversion.StringToTextPart(inputValue, column.PartSplitter, column.Part, column.PartToEnd);
    }

    /// <summary>
    ///   Gets the relative position.
    /// </summary>
    /// <returns>A value between 0 and MaxValue</returns>
    protected abstract int GetRelativePosition();

    public abstract string GetString(int i);

    private DateTime? AdjustTz(DateTime? input, Column column)
    {
      if (!input.HasValue) return null;
      string timeZone = null;
      // Constant value
      if (column.TimeZonePart.StartsWith("\"", StringComparison.Ordinal) &&
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
        return TimeZoneMapping.ConvertTime(input.Value, timeZone, ApplicationSetting.ToolSetting.DestinationTimeZone);
      }
      catch (ApplicationException ex)
      {
        HandleWarning(column.ColumnOrdinal, ex.Message);
        return null;
      }
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
          ret = GetDateTimeNull(null, value, null, timeValue, column);
          break;

        case DataType.Integer:
          ret = IntPtr.Size == 4 ? GetInt32Null(value, column) : GetInt64Null(value, column);
          break;

        case DataType.Double:
          ret = GetDoubleNull(value, column);
          break;

        case DataType.TextPart:
          ret = m_FileSetting is CsvFile ? value : GetPart(value, column);
          break;

        case DataType.Numeric:
          ret = GetDecimalNull(value, column);
          break;

        case DataType.Boolean:
          ret = StringConversion.StringToBoolean(value, column.True, column.False);
          break;

        case DataType.Guid:
          ret = StringConversion.StringToGuid(value);
          break;

        default:
          /* TextToHTML and  TextToHTMLFull have been handled in the CSV Reader as the length of the fields would change */
          ret = value;
          break;
      }

      return ret ?? DBNull.Value;
    }

    protected virtual string GetUniqueName(ICollection<string> previousColumns, int ordinal, string nametoadd)
    {
      var newName = StringUtils.MakeUniqueInCollection(previousColumns, nametoadd);
      if (newName != nametoadd)
        HandleWarning(ordinal,
          $"Column '{nametoadd}' exists more than once replaced with {newName}");
      return newName;
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
    ///   Handles the Event if reading the file is completed
    /// </summary>
    protected virtual void HandleReadFinished()
    {
      HandleShowProgress("Finished Reading", RecordNumber, cMaxValue);
      ReadFinished?.Invoke(this, null);
    }

    /// <summary>
    ///   Closes the <see cref="T:System.Data.IDataReader" /> Object.
    /// </summary>
    public void Close()
    {
      IndividualClose();
    }

    /// <summary>
    ///   Shows the process.
    /// </summary>
    /// <param name="text">The text.</param>
    protected virtual void HandleShowProgress(string text)
    {
      m_ProcessDisplay?.SetProcess(text);
    }

    /// <summary>
    ///   Shows the process twice a second
    /// </summary>
    /// <param name="text">Leading Text</param>
    /// <param name="recordNumber">The record number.</param>
    protected virtual void HandleShowProgressPeriodic(string text, long recordNumber)
    {
      if (m_ProcessDisplay != null)
        m_IntervalAction.Invoke(delegate { HandleShowProgress(text, recordNumber, GetRelativePosition()); });
    }

    /// <summary>
    ///   Handled the TreatTextAsNull Setting of the FileSetting
    /// </summary>
    /// <param name="inputValue"></param>
    /// <returns>
    ///   <c>NULL</c> if the text should be treated as NULL or if the value is null, or the input otherwise
    /// </returns>
    protected virtual string HandleTreatNull(string inputValue)
    {
      return StringUtils.ShouldBeTreatedAsNull(inputValue, m_FileSetting.TreatTextAsNull) ? null : inputValue;
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
        inputValue = inputValue.Replace((char)0xA0, ' ');
      if (m_FileSetting.TrimmingOption == TrimmingOption.All ||
          !quoted && m_FileSetting.TrimmingOption == TrimmingOption.Unquoted)
        return inputValue.Trim();

      return inputValue;
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

      var disp = column.DateFormat.ReplaceDefaults("/", column.DateSeparator, ":", column.TimeSeparator);

      HandleError(columnNumber,
        !string.IsNullOrEmpty(inputTime)
          ? $"'{inputDate} {inputTime}' is not a date of the format {disp} {column.TimePartFormat}"
          : $"'{inputDate}' is not a date of the format {disp}");
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
    ///   Open that is specific to an implementation of the reader
    /// </summary>
    /// <param name="determineColumnSize">if set to <c>true</c> [determine column size].</param>
    /// <returns>
    ///   Number of records in the file if known (use determineColumnSize), -1 otherwise
    /// </returns>
    protected abstract long IndividualOpen(bool determineColumnSize);

    protected abstract void IndividualClose();

    /// <summary>
    ///   Initializes the column array to a give count.
    /// </summary>
    /// <param name="fieldCount">The field count.</param>
    protected void InitColumn(int fieldCount)
    {
      Debug.Assert(fieldCount >= 0);
      m_FieldCount = fieldCount;
      Column = new Column[fieldCount];
      AssociatedTimeCol = new int[fieldCount];
      AssociatedTimeZoneCol = new int[fieldCount];
      for (var counter = 0; counter < fieldCount; counter++)
      {
        Column[counter] = new Column
        {
          ColumnOrdinal = counter
        };
        AssociatedTimeCol[counter] = -1;
        AssociatedTimeZoneCol[counter] = -1;
      }
    }

    /// <summary>
    ///   Gets the field headers.
    /// </summary>
    protected void ParseColumnName(IList<string> headerRow)
    {
      Debug.Assert(FieldCount >= 0);
      Debug.Assert(headerRow != null);

      if (headerRow.IsEmpty())
        return;
      InitColumn(FieldCount);

      // The last column is empty but we expect a header column, assume if a trailing separator
      if (!m_FileSetting.HasFieldHeader)
      {
        for (var i = 0; i < FieldCount; i++)
          GetColumn(i).Name = GetDefaultName(i);
        return;
      }

      var previousColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
      for (var counter = 0; counter < FieldCount; counter++)
      {
        var columnname = headerRow[counter];
        string resultingName;
        if (string.IsNullOrEmpty(columnname))
        {
          resultingName = GetDefaultName(counter);
          // HandleWarning(counter, string.Format(CultureInfo.CurrentCulture, "{0}Column title is empty.", CWarningId));
        }
        else
        {
          resultingName = columnname.Trim();
          if (columnname.Length != resultingName.Length)
            HandleWarning(counter,
              $"Column title '{columnname}' had leading or tailing spaces, these have been removed."
                .AddWarningId());
          if (resultingName.Length > 128)
          {
            resultingName = resultingName.Substring(0, 128);
            HandleWarning(counter,
              $"Column title '{resultingName.Substring(0, 20)}...' has been cut off after 128 characters.".AddWarningId());
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

      HandleError(column.ColumnOrdinal,
        $"'{inputValue}' is not an integer");
      return null;
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

      HandleError(column.ColumnOrdinal,
        $"'{inputValue}' is not an integer");
      return null;
    }

    /// <summary>
    ///   Shows the process.
    /// </summary>
    /// <param name="text">Leading Text</param>
    /// <param name="recordNumber">The record number.</param>
    /// <param name="progress">The progress (a value between 0 and MaxValue)</param>
    public void HandleShowProgress(string text, long recordNumber, int progress)
    {
      m_ProcessDisplay?.SetProcess(
        $"{text}\r\nRecord {recordNumber:N0}", progress);
    }
  }
}