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
using System.Diagnostics;
using System.Globalization;

namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   Abstract class as base for all DataReaders that are reading a typed value, e.G. Excel
  /// </summary>
  public abstract class BaseFileReaderTyped : BaseFileReader
  {
    private readonly bool m_TreatNbspAsSpace;

    private readonly string m_TreatTextAsNull;

    private readonly bool m_Trim;
    protected object?[] CurrentValues;

    /// <inheritdoc />
    /// <summary>
    ///   Constructor for abstract base call for <see cref="T:CsvTools.IFileReader" /> and <see
    ///   cref="T:CsvTools.IFileReaderWithEvents" /> that does read typed values like Excel, SQl
    /// </summary>
    /// <param name="fileName">Path to a physical file (if used)</param>
    /// <param name="columnDefinition">List of column definitions</param>
    /// <param name="recordLimit">Number of records that should be read</param>
    /// <param name="trim">Trim all read text</param>
    /// <param name="treatTextAsNull">Value to be replaced with NULL in Text</param>
    /// <param name="treatNbspAsSpace">nbsp in text will be replaced with Space</param>
    /// <param name="timeZoneAdjust">Class to modify date time for timezones</param>
    /// <param name="destTimeZone">Name of the time zone datetime values that have a source time zone should be converted to</param>
    protected BaseFileReaderTyped(
      in string fileName,
      in IEnumerable<IColumn>? columnDefinition,
      long recordLimit,
      bool trim,
      in string treatTextAsNull,
      bool treatNbspAsSpace,
      in TimeZoneChangeDelegate timeZoneAdjust,
      in string destTimeZone)
      : base(fileName, columnDefinition, recordLimit, timeZoneAdjust,destTimeZone)
    {
      m_TreatNbspAsSpace = treatNbspAsSpace;
      m_Trim = trim;
      m_TreatTextAsNull = treatTextAsNull;
      CurrentValues = Array.Empty<object>();
    }

    /// <inheritdoc />
    public override bool GetBoolean(int ordinal)
    {
      Debug.Assert(ordinal >= 0 && ordinal < FieldCount);
      Debug.Assert(CurrentValues != null && ordinal < CurrentValues.Length);
      if (CurrentValues![ordinal] is bool b)
        return b;
      EnsureTextFilled(ordinal);
      return base.GetBoolean(ordinal);
    }

    /// <inheritdoc cref="BaseFileReader" />
    /// <summary>
    ///   Returns an <see cref="IDataReader" /> for the specified column ordinal.
    /// </summary>
    /// <param name="ordinal">The index of the field to find.</param>
    /// <returns>The <see cref="IDataReader" /> for the specified column ordinal.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public new IDataReader GetData(int ordinal) => throw new NotImplementedException();

    /// <inheritdoc />
    public override DateTime GetDateTime(int ordinal)
    {
      Debug.Assert(ordinal >= 0 && ordinal < FieldCount);
      Debug.Assert(CurrentValues != null && ordinal < CurrentValues.Length);

      object? timePart = null;
      string? timePartText = null;
      EnsureTextFilled(ordinal);

      if (AssociatedTimeCol[ordinal] > -1)
      {
        timePart = CurrentValues![AssociatedTimeCol[ordinal]];
        EnsureTextFilled(AssociatedTimeCol[ordinal]);
        timePartText = CurrentRowColumnText[AssociatedTimeCol[ordinal]];
      }

      var dt = GetDateTimeNull(
        CurrentValues![ordinal],
        CurrentRowColumnText[ordinal],
        timePart,
        timePartText ?? string.Empty,
        GetColumn(ordinal),
        false);
      if (dt.HasValue)
        return dt.Value;
      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not a date time");
    }

    /// <inheritdoc />
    public override decimal GetDecimal(int ordinal)
    {
      Debug.Assert(ordinal >= 0 && ordinal < FieldCount);
      Debug.Assert(CurrentValues != null && ordinal < CurrentValues.Length);

      if (CurrentValues![ordinal] is decimal || CurrentValues[ordinal] is double
                                             || CurrentValues[ordinal] is float
                                             || CurrentValues[ordinal] is short
                                             || CurrentValues[ordinal] is int
                                             || CurrentValues[ordinal] is long)
        return Convert.ToDecimal(CurrentValues[ordinal], CultureInfo.CurrentCulture);
      EnsureTextFilled(ordinal);
      return base.GetDecimal(ordinal);
    }

    /// <inheritdoc />
    public override double GetDouble(int ordinal)
    {
      Debug.Assert(ordinal >= 0 && ordinal < FieldCount);
      Debug.Assert(CurrentValues != null && ordinal < CurrentValues.Length);

      if (CurrentValues![ordinal] is decimal || CurrentValues[ordinal] is double
                                             || CurrentValues[ordinal] is float
                                             || CurrentValues[ordinal] is short
                                             || CurrentValues[ordinal] is int
                                             || CurrentValues[ordinal] is long)
        return Convert.ToDouble(CurrentValues[ordinal], CultureInfo.CurrentCulture);
      EnsureTextFilled(ordinal);
      return base.GetDouble(ordinal);
    }

    /// <inheritdoc />
    public override float GetFloat(int ordinal)
    {
      Debug.Assert(ordinal >= 0 && ordinal < FieldCount);
      Debug.Assert(CurrentValues != null && ordinal < CurrentValues.Length);
      try
      {
        if (CurrentValues![ordinal] is decimal || CurrentValues[ordinal] is double
                                               || CurrentValues[ordinal] is float
                                               || CurrentValues[ordinal] is short
                                               || CurrentValues[ordinal] is int
                                               || CurrentValues[ordinal] is long)
          return Convert.ToSingle(CurrentValues[ordinal], CultureInfo.CurrentCulture);
      }
      catch (Exception e)
      {
        throw WarnAddFormatException(ordinal, $"'{CurrentValues![ordinal]}' is not a float, {e.Message}");
      }

      EnsureTextFilled(ordinal);
      return base.GetFloat(ordinal);
    }

    /// <inheritdoc />
    public override Guid GetGuid(int ordinal)
    {
      Debug.Assert(ordinal >= 0 && ordinal < FieldCount);
      Debug.Assert(CurrentValues != null && ordinal < CurrentValues.Length);

      if (CurrentValues![ordinal] is Guid val)
        return val;
      EnsureTextFilled(ordinal);
      return base.GetGuid(ordinal);
    }

    /// <inheritdoc />
    public override short GetInt16(int ordinal)
    {
      Debug.Assert(ordinal >= 0 && ordinal < FieldCount);
      Debug.Assert(CurrentValues != null && ordinal < CurrentValues.Length);
      try
      {
        if (CurrentValues![ordinal] is decimal || CurrentValues[ordinal] is double
                                               || CurrentValues[ordinal] is float
                                               || CurrentValues[ordinal] is short
                                               || CurrentValues[ordinal] is int
                                               || CurrentValues[ordinal] is long)
          return Convert.ToInt16(CurrentValues[ordinal], CultureInfo.CurrentCulture);
      }
      catch (Exception e)
      {
        throw WarnAddFormatException(ordinal, $"'{CurrentValues![ordinal]}' is not a short, {e.Message}");
      }

      EnsureTextFilled(ordinal);
      return base.GetInt16(ordinal);
    }

    /// <inheritdoc />
    public override int GetInt32(int ordinal)
    {
      Debug.Assert(ordinal >= 0 && ordinal < FieldCount);
      Debug.Assert(CurrentValues != null && ordinal < CurrentValues.Length);

      try
      {
        if (CurrentValues![ordinal] is decimal || CurrentValues[ordinal] is double
                                               || CurrentValues[ordinal] is float
                                               || CurrentValues[ordinal] is short
                                               || CurrentValues[ordinal] is int
                                               || CurrentValues[ordinal] is long)
          return Convert.ToInt32(CurrentValues[ordinal], CultureInfo.CurrentCulture);
      }
      catch (Exception e)
      {
        throw WarnAddFormatException(ordinal, $"'{CurrentValues![ordinal]}' is not an integer, {e.Message}");
      }

      EnsureTextFilled(ordinal);
      return base.GetInt32(ordinal);
    }

    /// <inheritdoc />
    public override long GetInt64(int ordinal)
    {
      Debug.Assert(ordinal >= 0 && ordinal < FieldCount);
      Debug.Assert(CurrentValues != null && ordinal < CurrentValues.Length);
      try
      {
        if (CurrentValues![ordinal] is decimal || CurrentValues[ordinal] is double
                                               || CurrentValues[ordinal] is float
                                               || CurrentValues[ordinal] is short
                                               || CurrentValues[ordinal] is int
                                               || CurrentValues[ordinal] is long)
          return Convert.ToInt64(CurrentValues[ordinal], CultureInfo.CurrentCulture);
      }
      catch (Exception e)
      {
        throw WarnAddFormatException(ordinal, $"'{CurrentValues![ordinal]}' is not a long, {e.Message}");
      }

      EnsureTextFilled(ordinal);
      return base.GetInt64(ordinal);
    }

    public override string GetString(int ordinal)
    {
      Debug.Assert(ordinal >= 0 && ordinal < FieldCount);
      Debug.Assert(CurrentValues != null && ordinal < CurrentValues.Length);

      return Convert.ToString(CurrentValues![ordinal]) ?? string.Empty;
    }

    public override int GetValues(object[] values)
    {
      Array.Copy(CurrentValues, values, FieldCount);
      return FieldCount;
    }

    public override bool IsDBNull(int ordinal)
    {
      Debug.Assert(ordinal >= 0 && ordinal < FieldCount);
      if (CurrentValues.Length <= ordinal)
        return true;
      if (Column[ordinal].ValueFormat.DataType == DataTypeEnum.DateTime)
      {
        if (AssociatedTimeCol[ordinal] == -1)
          return CurrentValues[ordinal] is null || CurrentValues[ordinal] == DBNull.Value;

        return (CurrentValues[ordinal] is null || CurrentValues[ordinal] == DBNull.Value)
               && (CurrentValues[AssociatedTimeCol[ordinal]] is null
                   || CurrentValues[AssociatedTimeCol[ordinal]] == DBNull.Value);
      }

      if (CurrentValues[ordinal] is null || CurrentValues[ordinal] == DBNull.Value)
        return true;

      if (CurrentValues[ordinal] is string str)
        return string.IsNullOrEmpty(str);

      return false;
    }

    protected override void InitColumn(int fieldCount)
    {
      CurrentValues = new object[fieldCount];
      base.InitColumn(fieldCount);
    }

    protected string TreatNbspTestAsNullTrim(string inputString) =>
      TreatNbspAsNullTrim(inputString, m_TreatNbspAsSpace, m_TreatTextAsNull, m_Trim);

    private void EnsureTextFilled(int ordinal)
    {
      if (string.IsNullOrEmpty(CurrentRowColumnText[ordinal]))
        CurrentRowColumnText[ordinal] = Convert.ToString(CurrentValues[ordinal]);
    }
  }
}