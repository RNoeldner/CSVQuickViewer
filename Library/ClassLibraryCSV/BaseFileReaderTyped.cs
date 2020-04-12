using System;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading.Tasks;

namespace CsvTools
{
  /// <summary>
  ///   Abstract class as base for all DataReaders that are reading a typed value, e.G. Excel
  /// </summary>
  public abstract class BaseFileReaderTyped : BaseFileReader
  {
#pragma warning disable CA1051 // Do not declare visible instance fields
    protected object[] CurrentValues;
#pragma warning restore CA1051 // Do not declare visible instance fields

    protected BaseFileReaderTyped(IFileSetting fileSetting, string timeZone, IProcessDisplay processDisplay) : base(fileSetting, timeZone, processDisplay)
    {
    }

    public async override Task<bool> ReadAsync() => Read();

    /// <summary>
    ///   Gets the type of the column by looking at the first 50 rows
    /// </summary>
    /// <returns>An array with the found data types</returns>
    /// <remarks>In case of mixed types, string is preferred over everything</remarks>
    protected DataType[] GetColumnType()
    {
      Contract.Ensures(Contract.Result<DataType[]>() != null);
      Contract.Ensures(Contract.Result<DataType[]>().Length == FieldCount);

      HandleShowProgress("Reading data to determine type");
      var colType = new DataType[FieldCount];

      // Initialize with DataType.TextPart
      for (var col = 0; col < FieldCount; col++)
        colType[col] = DataType.TextPart;
      for (var row = 1; row < 50; row++)
      {
        for (var col = 0; col < FieldCount; col++)
        {
          // if a column was detected as string, keep it that way
          if (colType[col] == DataType.String)
            continue;

          if (CurrentValues[col] == null)
            continue;

          var detected = CurrentValues[col].GetType().GetDataType();

          // if already set continue
          if (colType[col] == detected)
            continue;

          // String will overwrite all
          if (detected == DataType.String || colType[col] == DataType.TextPart)
            colType[col] = detected;
        }
        // get the next record
        if (!Read())
          break;
      }

      for (var col = 0; col < FieldCount; col++)
        // all rows where empty no data type found
        if (colType[col] == DataType.TextPart)
          // make it a string
          colType[col] = DataType.String;
      return colType;
    }

    /// <summary>
    ///   Gets the boolean.
    /// </summary>
    /// <param name="columnNumber">The i.</param>
    /// <returns></returns>
    public override bool GetBoolean(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentValues != null && columnNumber < CurrentValues.Length);
      if (CurrentValues[columnNumber] is bool b)
        return b;
      return base.GetBoolean(columnNumber);
    }

    protected override void InitColumn(int fieldCount)
    {
      CurrentValues = new object[fieldCount];
      base.InitColumn(fieldCount);
    }

    /// <summary>
    ///   Gets the 8-bit unsigned integer value of the specified column.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <returns>The 8-bit unsigned integer value of the specified column.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public override byte GetByte(int i) => throw new NotImplementedException();

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
    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) =>
      throw new NotImplementedException();

    /// <summary>
    ///   Gets the character value of the specified column.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <returns>The character value of the specified column.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public override char GetChar(int i) => throw new NotImplementedException();

    /// <summary>
    ///   Reads a stream of characters from the specified column offset into the buffer as an array,
    ///   starting at the given buffer offset.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <param name="fieldoffset">The index within the row from which to start the read operation.</param>
    /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
    /// <param name="bufferoffset">
    ///   The index for <paramref name="buffer" /> to start the read operation.
    /// </param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>The actual number of characters read.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) =>
      throw new NotImplementedException();

    /// <summary>
    ///   Returns an <see cref="IDataReader" /> for the specified column ordinal.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>The <see cref="IDataReader" /> for the specified column ordinal.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public IDataReader GetData(int i) => throw new NotImplementedException();

    /// <summary>
    ///   Gets the data type information for the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>The data type information for the specified field.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public string GetDataTypeName(int i) => GetFieldType(i).Name;

    /// <summary>
    ///   Gets the date and time data value of the specified field.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>The date and time data value of the specified field.</returns>
    public override DateTime GetDateTime(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentValues != null && columnNumber < CurrentValues.Length);

      object timePart = null;
      string timePartText = null;
      if (AssociatedTimeCol[columnNumber] > -1)
      {
        timePart = CurrentValues[AssociatedTimeCol[columnNumber]];
        timePartText = CurrentRowColumnText[AssociatedTimeCol[columnNumber]];
      }

      var dt = GetDateTimeNull(CurrentValues[columnNumber], CurrentRowColumnText[columnNumber], timePart, timePartText,
        GetColumn(columnNumber), false);
      if (dt.HasValue)
        return dt.Value;
      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(columnNumber,
        $"'{CurrentRowColumnText[columnNumber]}' is not a datetime");
    }

    /// <summary>
    ///   Gets the decimal.
    /// </summary>
    /// <param name="columnNumber">The i.</param>
    /// <returns></returns>
    public override decimal GetDecimal(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentValues != null && columnNumber < CurrentValues.Length);

      if (CurrentValues[columnNumber] is decimal || CurrentValues[columnNumber] is double ||
          CurrentValues[columnNumber] is float ||
          CurrentValues[columnNumber] is short || CurrentValues[columnNumber] is int ||
          CurrentValues[columnNumber] is long)
        return Convert.ToDecimal(CurrentValues[columnNumber], CultureInfo.CurrentCulture);

      return base.GetDecimal(columnNumber);
    }

    /// <summary>
    ///   Gets the double.
    /// </summary>
    /// <param name="columnNumber">The i.</param>
    /// <returns></returns>
    public override double GetDouble(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentValues != null && columnNumber < CurrentValues.Length);

      if (CurrentValues[columnNumber] is decimal || CurrentValues[columnNumber] is double ||
          CurrentValues[columnNumber] is float ||
          CurrentValues[columnNumber] is short || CurrentValues[columnNumber] is int ||
          CurrentValues[columnNumber] is long)
        return Convert.ToDouble(CurrentValues[columnNumber], CultureInfo.CurrentCulture);

      return base.GetDouble(columnNumber);
    }

    /// <summary>
    ///   Gets the single-precision floating point number of the specified field.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>The single-precision floating point number of the specified field.</returns>
    public override float GetFloat(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentValues != null && columnNumber < CurrentValues.Length);

      if (CurrentValues[columnNumber] is decimal || CurrentValues[columnNumber] is double ||
          CurrentValues[columnNumber] is float ||
          CurrentValues[columnNumber] is short || CurrentValues[columnNumber] is int ||
          CurrentValues[columnNumber] is long)
        return Convert.ToSingle(CurrentValues[columnNumber], CultureInfo.CurrentCulture);

      return base.GetFloat(columnNumber);
    }

    /// <summary>
    ///   Gets the unique identifier.
    /// </summary>
    /// <param name="columnNumber">The i.</param>
    /// <returns></returns>
    public override Guid GetGuid(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentValues != null && columnNumber < CurrentValues.Length);

      if (CurrentValues[columnNumber] is Guid val)
        return val;

      return base.GetGuid(columnNumber);
    }

    /// <summary>
    ///   Gets the 16-bit signed integer value of the specified field.
    /// </summary>
    /// <param name="columnNumber">The index of the field to find.</param>
    /// <returns>The 16-bit signed integer value of the specified field.</returns>
    public override short GetInt16(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentValues != null && columnNumber < CurrentValues.Length);

      if (CurrentValues[columnNumber] is decimal || CurrentValues[columnNumber] is double ||
          CurrentValues[columnNumber] is float ||
          CurrentValues[columnNumber] is short || CurrentValues[columnNumber] is int ||
          CurrentValues[columnNumber] is long)
        return Convert.ToInt16(CurrentValues[columnNumber], CultureInfo.CurrentCulture);

      return base.GetInt16(columnNumber);
    }

    /// <summary>
    ///   Gets the int32.
    /// </summary>
    /// <param name="columnNumber">The i.</param>
    /// <returns></returns>
    public override int GetInt32(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentValues != null && columnNumber < CurrentValues.Length);

      if (CurrentValues[columnNumber] is decimal || CurrentValues[columnNumber] is double ||
          CurrentValues[columnNumber] is float ||
          CurrentValues[columnNumber] is short || CurrentValues[columnNumber] is int ||
          CurrentValues[columnNumber] is long)
        return Convert.ToInt32(CurrentValues[columnNumber], CultureInfo.CurrentCulture);

      return base.GetInt32(columnNumber);
    }

    /// <summary>
    ///   Gets the int64.
    /// </summary>
    /// <param name="columnNumber">The i.</param>
    /// <returns></returns>
    public override long GetInt64(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentValues != null && columnNumber < CurrentValues.Length);

      if (CurrentValues[columnNumber] is decimal || CurrentValues[columnNumber] is double ||
          CurrentValues[columnNumber] is float ||
          CurrentValues[columnNumber] is short || CurrentValues[columnNumber] is int ||
          CurrentValues[columnNumber] is long)
        return Convert.ToInt64(CurrentValues[columnNumber], CultureInfo.CurrentCulture);

      return base.GetInt64(columnNumber);
    }

    public override string GetString(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentValues != null && columnNumber < CurrentValues.Length);
      if (CurrentValues[columnNumber] is string val)
        return val;

      return base.GetString(columnNumber);
    }

    public override int GetValues(object[] values)
    {
      Contract.Assume(CurrentValues != null);
      Array.Copy(CurrentValues, values, FieldCount);
      return FieldCount;
    }

    public override bool IsDBNull(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      if (CurrentValues == null || CurrentValues.Length <= columnNumber)
        return true;
      if (Column[columnNumber].ValueFormat.DataType == DataType.DateTime)
      {
        if (AssociatedTimeCol[columnNumber] == -1)
          return CurrentValues[columnNumber] == null || CurrentValues[columnNumber] == DBNull.Value;

        return (CurrentValues[columnNumber] == null || CurrentValues[columnNumber] == DBNull.Value) &&
               (CurrentValues[AssociatedTimeCol[columnNumber]] == null ||
                CurrentValues[AssociatedTimeCol[columnNumber]] == DBNull.Value);
      }

      if (CurrentValues[columnNumber] == null || CurrentValues[columnNumber] == DBNull.Value)
        return true;

      if (CurrentValues[columnNumber] is string str)
        return string.IsNullOrEmpty(str);

      return false;
    }
  }
}