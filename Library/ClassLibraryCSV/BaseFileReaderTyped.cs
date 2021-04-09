using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;

namespace CsvTools
{
  /// <summary>
  ///   Abstract class as base for all DataReaders that are reading a typed value, e.G. Excel
  /// </summary>
  public abstract class BaseFileReaderTyped : BaseFileReader
  {
    private readonly bool m_TreatNbspAsSpace;
    private readonly string m_TreatTextAsNull;
    private readonly bool m_Trim;
    protected object[] CurrentValues;

    /// <summary>
    ///   Constructor for abstract base call for <see cref="IFileReader" /> that does read typed
    ///   values like Excel, SQl
    /// </summary>
    /// <param name="fileName">Path to a physical file (if used)</param>
    /// <param name="columnDefinition">List of column definitions</param>
    /// <param name="recordLimit">Number of records that should be read</param>
    /// <param name="trim">Trim all read text</param>
    /// <param name="treatTextAsNull">Value to be replaced with NULL in Text</param>
    /// <param name="treatNbspAsSpace">nbsp in text will be replaced with Space</param>
    protected BaseFileReaderTyped([CanBeNull] string fileName,
                                  [CanBeNull] IEnumerable<IColumn> columnDefinition,
                                  long recordLimit, bool trim = false,
                                  string treatTextAsNull = BaseSettings.cTreatTextAsNull, bool treatNbspAsSpace = false) :
      base(fileName, columnDefinition, recordLimit)
    {
      m_TreatNbspAsSpace = treatNbspAsSpace;
      m_Trim = trim;
      m_TreatTextAsNull = treatTextAsNull;
    }

    protected string TreatNbspTestAsNullTrim([CanBeNull] string inputString) =>
      BaseFileReader.TreatNbspTestAsNullTrim(inputString, m_TreatNbspAsSpace, m_TreatTextAsNull, m_Trim);

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
      EnsureTextFilled(columnNumber);
      return base.GetBoolean(columnNumber);
    }

    protected override void InitColumn(int fieldCount)
    {
      CurrentValues = new object[fieldCount];
      base.InitColumn(fieldCount);
    }

    /// <summary>
    ///   Returns an <see cref="IDataReader" /> for the specified column ordinal.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>The <see cref="IDataReader" /> for the specified column ordinal.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public new IDataReader GetData(int i) => throw new NotImplementedException();

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
      EnsureTextFilled(columnNumber);

      if (AssociatedTimeCol[columnNumber] > -1)
      {
        timePart = CurrentValues[AssociatedTimeCol[columnNumber]];
        EnsureTextFilled(AssociatedTimeCol[columnNumber]);
        timePartText = CurrentRowColumnText[AssociatedTimeCol[columnNumber]];
      }
      var dt = GetDateTimeNull(CurrentValues[columnNumber], CurrentRowColumnText[columnNumber], timePart, timePartText,
        GetColumn(columnNumber), false);
      if (dt.HasValue)
        return dt.Value;
      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(columnNumber,
        $"'{CurrentRowColumnText[columnNumber]}' is not a date time");
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
      EnsureTextFilled(columnNumber);
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
      EnsureTextFilled(columnNumber);
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
      try
      {
        if (CurrentValues[columnNumber] is decimal || CurrentValues[columnNumber] is double ||
            CurrentValues[columnNumber] is float ||
            CurrentValues[columnNumber] is short || CurrentValues[columnNumber] is int ||
            CurrentValues[columnNumber] is long)
          return Convert.ToSingle(CurrentValues[columnNumber], CultureInfo.CurrentCulture);
      }
      catch (Exception e)
      {
        throw WarnAddFormatException(columnNumber, $"'{CurrentValues[columnNumber]}' is not a float, {e.Message}");
      }

      EnsureTextFilled(columnNumber);
      return base.GetFloat(columnNumber);
    }

    private void EnsureTextFilled(int columnNumber)
    {
      if (string.IsNullOrEmpty(CurrentRowColumnText[columnNumber]) && CurrentValues[columnNumber] != null)
        CurrentRowColumnText[columnNumber] = CurrentValues[columnNumber].ToString();
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
      EnsureTextFilled(columnNumber);
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
      try
      {
        if (CurrentValues[columnNumber] is decimal || CurrentValues[columnNumber] is double ||
            CurrentValues[columnNumber] is float ||
            CurrentValues[columnNumber] is short || CurrentValues[columnNumber] is int ||
            CurrentValues[columnNumber] is long)
          return Convert.ToInt16(CurrentValues[columnNumber], CultureInfo.CurrentCulture);
      }
      catch (Exception e)
      {
        throw WarnAddFormatException(columnNumber, $"'{CurrentValues[columnNumber]}' is not a short, {e.Message}");
      }

      EnsureTextFilled(columnNumber);
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

      try
      {
        if (CurrentValues[columnNumber] is decimal || CurrentValues[columnNumber] is double ||
            CurrentValues[columnNumber] is float ||
            CurrentValues[columnNumber] is short || CurrentValues[columnNumber] is int ||
            CurrentValues[columnNumber] is long)
          return Convert.ToInt32(CurrentValues[columnNumber], CultureInfo.CurrentCulture);
      }
      catch (Exception e)
      {
        throw WarnAddFormatException(columnNumber, $"'{CurrentValues[columnNumber]}' is not an integer, {e.Message}");
      }

      EnsureTextFilled(columnNumber);
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
      try
      {
        if (CurrentValues[columnNumber] is decimal || CurrentValues[columnNumber] is double ||
            CurrentValues[columnNumber] is float ||
            CurrentValues[columnNumber] is short || CurrentValues[columnNumber] is int ||
            CurrentValues[columnNumber] is long)
          return Convert.ToInt64(CurrentValues[columnNumber], CultureInfo.CurrentCulture);
      }
      catch (Exception e)
      {
        throw WarnAddFormatException(columnNumber, $"'{CurrentValues[columnNumber]}' is not a long, {e.Message}");
      }
      EnsureTextFilled(columnNumber);
      return base.GetInt64(columnNumber);
    }

    public override string GetString(int columnNumber)
    {
      Debug.Assert(columnNumber >= 0 && columnNumber < FieldCount);
      Debug.Assert(CurrentValues != null && columnNumber < CurrentValues.Length);

      return CurrentValues[columnNumber]?.ToString();
    }

    public override int GetValues(object[] values)
    {
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