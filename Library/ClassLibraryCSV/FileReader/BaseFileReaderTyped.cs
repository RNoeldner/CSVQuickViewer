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
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace CsvTools
{
  /// <inheritdoc />
  /// <summary>
  ///   Abstract class as base for all DataReaders that are reading a typed value, e.G. Excel
  /// </summary>
  public abstract class BaseFileReaderTyped : BaseFileReader
  {
    /// <summary>
    /// The values of the current row
    /// </summary>
    protected object?[] CurrentValues;

    private readonly bool m_TreatNbspAsSpace;
    private readonly string m_TreatTextAsNull;
    private readonly bool m_Trim;

    /// <inheritdoc />
    /// <summary>
    ///   Constructor for abstract base call for <see cref="T:CsvTools.IFileReader" /> and <see
    ///   cref="T:CsvTools.IFileReader" /> that does read typed values like Excel, SQl
    /// </summary>
    /// <param name="fileName">Path to a physical file (if used)</param>
    /// <param name="columnDefinition">List of column definitions</param>
    /// <param name="recordLimit">Number of records that should be read</param>
    /// <param name="trim">Trim read text</param>
    /// <param name="treatTextAsNull">Value to be replaced with NULL in Text</param>
    /// <param name="treatNbspAsSpace">nbsp in text will be replaced with Space</param>
    /// <param name="timeZoneAdjust">Class to modify date time for timezones</param>
    /// <param name="returnedTimeZone">Name of the time zone datetime values that have a source time zone should be converted to</param>
    /// <param name="allowPercentage">If <c>true</c> percentage symbols are is processed to a decimal 26.7% will become .267</param>
    /// <param name="removeCurrency">If <c>true</c> common currency symbols are removed to parse a currency value as decimal</param>
    protected BaseFileReaderTyped(
      in string fileName,
      in IEnumerable<Column>? columnDefinition,
      long recordLimit,
      bool trim,
      in string treatTextAsNull,
      bool treatNbspAsSpace,
      in TimeZoneChangeDelegate? timeZoneAdjust,
      in string returnedTimeZone,
      bool allowPercentage,
      bool removeCurrency)
      : base(fileName, columnDefinition, recordLimit, timeZoneAdjust, returnedTimeZone, allowPercentage, removeCurrency)
    {
      m_TreatNbspAsSpace = treatNbspAsSpace;
      m_Trim = trim;
      m_TreatTextAsNull = treatTextAsNull;
      CurrentValues = Array.Empty<object>();
    }

    /// <inheritdoc />
    public override bool GetBoolean(int ordinal)
    {
      var val = GetCurrentValue(ordinal);
      if (val is bool b)
        return b;
      EnsureTextFilled(ordinal);
      return base.GetBoolean(ordinal);
    }

    /// <exception cref="NotImplementedException"></exception>
    public new IDataReader GetData(int ordinal) => throw new NotImplementedException();

    /// <inheritdoc />
    public override DateTime GetDateTime(int ordinal)
    {
      var val = GetCurrentValue(ordinal);

      object? timePart;
      string timePartText;
      EnsureTextFilled(ordinal);

      if (AssociatedTimeCol[ordinal] > -1)
      {
        timePart = GetCurrentValue(AssociatedTimeCol[ordinal]);
        EnsureTextFilled(AssociatedTimeCol[ordinal]);
        timePartText = CurrentRowColumnText[AssociatedTimeCol[ordinal]];
      }
      else
      {
        timePart = null;
        timePartText = string.Empty;
      }

      var dt = GetDateTimeNull(
          val,
        CurrentRowColumnText[ordinal].AsSpan(),
        timePart,
        timePartText.AsSpan(),
        GetColumn(ordinal),
        false);
      if (dt.HasValue)
        return dt.Value;
      // Warning was added by GetDecimalNull
      throw WarnAddFormatException(ordinal, $"'{CurrentRowColumnText[ordinal]}' is not a date time");
    }

    /// <summary>
    /// Shared numeric conversion logic with error handling.
    /// </summary>
    private T ConvertNumeric<T>(int ordinal, string typeName, Func<object, IFormatProvider, T> converter)
    {
      var val = GetCurrentValue(ordinal);

      try
      {
        if (val is decimal || val is double || val is float || val is short || val is int || val is long)
          return converter(val, CultureInfo.CurrentCulture);
      }
      catch (Exception e)
      {
        throw WarnAddFormatException(ordinal, $"'{val}' is not a {typeName}, {e.Message}");
      }

      EnsureTextFilled(ordinal);
      return converter(base.GetValue(ordinal), CultureInfo.CurrentCulture);
    }

    /// <inheritdoc />
    /// <inheritdoc />
    public override decimal GetDecimal(int ordinal) =>
      ConvertNumeric(ordinal, "decimal", Convert.ToDecimal);

    /// <inheritdoc />
    public override double GetDouble(int ordinal) =>
      ConvertNumeric(ordinal, "double", Convert.ToDouble);

    /// <inheritdoc />
    public override float GetFloat(int ordinal) =>
      ConvertNumeric(ordinal, "float", Convert.ToSingle);

    /// <inheritdoc />
    public override Guid GetGuid(int ordinal)
    {
      if (GetCurrentValue(ordinal) is Guid val)
        return val;
      EnsureTextFilled(ordinal);
      return base.GetGuid(ordinal);
    }

    /// <inheritdoc />
    public override short GetInt16(int ordinal) =>
      ConvertNumeric(ordinal, "short", Convert.ToInt16);

    /// <inheritdoc />
    /// <inheritdoc />
    public override int GetInt32(int ordinal) =>
      ConvertNumeric(ordinal, "int", Convert.ToInt32);

    /// <inheritdoc />
    /// <inheritdoc />
    public override long GetInt64(int ordinal) =>
      ConvertNumeric(ordinal, "long", Convert.ToInt64);


    /// <inheritdoc />
    public override ReadOnlySpan<char> GetSpan(int ordinal)
      => GetString(ordinal).AsSpan();

    /// <inheritdoc />
    public override string GetString(int ordinal)
      => Convert.ToString(GetCurrentValue(ordinal)) ?? string.Empty;

    /// <inheritdoc />
    public override int GetValues(object[] values)
    {
      var len = Math.Min(values.Length, FieldCount);
      Array.Copy(CurrentValues, values, len);
      return len;
    }

    /// <inheritdoc />
    public override bool IsDBNull(int ordinal)
    {
      if ((ordinal < 0 || ordinal >= FieldCount) || (CurrentValues.Length <= ordinal))
        return true;



      if (Column[ordinal].ValueFormat.DataType == DataTypeEnum.DateTime)
      {
        if (AssociatedTimeCol[ordinal] == -1)
          return CurrentValues[ordinal] is null;

        return CurrentValues[ordinal] is null && CurrentValues[AssociatedTimeCol[ordinal]] is null;
      }

      if (CurrentValues[ordinal] is null)
        return true;

      if (CurrentValues[ordinal] is string str)
        return string.IsNullOrEmpty(str);

      return false;
    }

    /// <summary>
    /// Gets the current object stored in CurrentValues and does check
    /// </summary>
    /// <param name="ordinal">The ordinal of the column</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentOutOfRangeException">ordinal - Value is out of range 0-{FieldCount}</exception>
    /// <exception cref="System.NullReferenceException">CurrentValues is not set, please open the reader before accessing data</exception>
    private object? GetCurrentValue(int ordinal)
    {
      if (ordinal < 0 || ordinal >= FieldCount)
        throw new ArgumentOutOfRangeException(nameof(ordinal), $"Value is out of range 0-{FieldCount}");

      if (CurrentValues is null || ordinal >= CurrentValues.Length)
        throw new NullReferenceException("CurrentValues is not set, please open the reader before accessing data");

      return CurrentValues[ordinal];
    }

    /// <inheritdoc />
    protected override void InitColumn(int fieldCount)
    {
      CurrentValues = new object[fieldCount];
      base.InitColumn(fieldCount);
    }

    /// <inheritdoc cref="BaseFileReader" />
    protected string TreatNbspTestAsNullTrim(ReadOnlySpan<char> inputString)
    {
      {
        if (inputString.Length == 0)
          return string.Empty;

        if (m_Trim)
          inputString = inputString.Trim();

        if (m_TreatNbspAsSpace && inputString.IndexOf((char) 0xA0)!=-1)
          return (inputString.ToString().Replace((char) 0xA0, ' ').AsSpan().ShouldBeTreatedAsNull(m_TreatTextAsNull.AsSpan()) ? Array.Empty<char>() : inputString).ToString();

        return (inputString.ShouldBeTreatedAsNull(m_TreatTextAsNull.AsSpan()) ? Array.Empty<char>() : inputString).ToString();
      }
    }

    private void EnsureTextFilled(int ordinal)
    {
      if (string.IsNullOrEmpty(CurrentRowColumnText[ordinal]))
        CurrentRowColumnText[ordinal] = Convert.ToString(GetCurrentValue(ordinal)) ?? string.Empty;
    }
  }
}