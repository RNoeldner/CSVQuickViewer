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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CsvTools;

/// <inheritdoc />
/// <summary>
///   Abstract class as base for all DataReaders that are reading a typed value, e.G. Excel
/// </summary>
public abstract class BaseFileReaderTyped : BaseFileReader
{
  private readonly bool m_TreatNbspAsSpace;
  private readonly string m_TreatTextAsNull;
  private readonly bool m_Trim;

  /// <summary>
  /// The typed values of the current row
  /// </summary>
  private object?[] m_CurrentValues;

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
  /// <param name="returnedTimeZone">Name of the time zone datetime values that have a source time zone should be converted to</param>
  /// <param name="allowPercentage">If <c>true</c> percentage symbols are is processed to a decimal 26.7% will become .267</param>
  /// <param name="removeCurrency">If <c>true</c> common currency symbols are removed to parse a currency value as decimal</param>
  protected BaseFileReaderTyped(
    string fileName,
    in IEnumerable<Column>? columnDefinition,
    long recordLimit,
    bool trim,
    string treatTextAsNull,
    bool treatNbspAsSpace,
    string returnedTimeZone,
    bool allowPercentage,
    bool removeCurrency)
    : base(fileName, columnDefinition, recordLimit, returnedTimeZone, allowPercentage, removeCurrency)
  {
    m_TreatNbspAsSpace = treatNbspAsSpace;
    m_Trim = trim;
    m_TreatTextAsNull = treatTextAsNull.Trim();
    m_CurrentValues = Array.Empty<object>();
  }

  /// <inheritdoc />
  public override bool GetBoolean(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    if (val is bool b)
      return b;
    return base.GetBoolean(ordinal);
  }

  /// <inheritdoc />
  /// <exception cref="NotImplementedException"></exception>
  public new IDataReader GetData(int ordinal) => throw new NotImplementedException();

  /// <inheritdoc />
  public override DateTime GetDateTime(int ordinal)
  {
    // 1. Pre-calculate the time column index to reduce redundant array lookups
    int timeColIndex = AssociatedTimeCol[ordinal];
    bool hasTimeCol = timeColIndex > -1;

    // 2. Fetch values. Note: GetSpan(ordinal) is likely called inside SpanToDateTime too, 
    // but we'll grab it once here to check for emptiness later.
    var span = GetSpan(ordinal);
    var col = GetColumn(ordinal);

    // 3. Conditional fetch for time parts (Inline ternary is often cleaner for JIT)
    var timePartValue = hasTimeCol ? GetCurrentValue(timeColIndex) : null;
    var timePartSpan = hasTimeCol ? GetSpan(timeColIndex) : ReadOnlySpan<char>.Empty;

    // 4. Execution
    var dt = SpanToDateTime(col, GetCurrentValue(ordinal), span, timePartValue, timePartSpan, false);

    if (dt.HasValue)
      return dt.Value;

    // 5. Exceptional paths
    if (!GetSpan(ordinal).IsEmpty)
      throw WarnAddFormatException(ordinal, $"'{GetString(ordinal)}' is not a date time");

    throw new FormatException($"Value in column '{col.Name}' is empty and cannot be converted to DateTime");
  }

  /// <inheritdoc />
  public override decimal GetDecimal(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    if (val is decimal or double or float or short or int or long)
      return Convert.ToDecimal(val, CultureInfo.CurrentCulture);
    return base.GetDecimal(ordinal);
  }

  /// <inheritdoc />
  public override double GetDouble(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    if (val is decimal or double or float or short or int or long)
      return Convert.ToDouble(val, CultureInfo.CurrentCulture);
    return base.GetDouble(ordinal);
  }

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

    return base.GetFloat(ordinal);
  }

  /// <inheritdoc />
  public override Guid GetGuid(int ordinal)
  {
    if (GetCurrentValue(ordinal) is Guid val)
      return val;
    return base.GetGuid(ordinal);
  }

  /// <inheritdoc />
  public override short GetInt16(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    try
    {
      if (val is decimal || val is double || val is float || val is short || val is int || val is long)
        return Convert.ToInt16(val, CultureInfo.CurrentCulture);
    }
    catch (Exception e)
    {
      throw WarnAddFormatException(ordinal, $"'{val}' is not a short, {e.Message}");
    }

    return base.GetInt16(ordinal);
  }

  /// <inheritdoc />
  public override int GetInt32(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    try
    {
      if (val is decimal || val is double || val is float || val is short || val is int || val is long)
        return Convert.ToInt32(val, CultureInfo.CurrentCulture);
    }
    catch (Exception e)
    {
      throw WarnAddFormatException(ordinal, $"'{val}' is not an integer, {e.Message}");
    }
    return base.GetInt32(ordinal);
  }

  /// <inheritdoc />
  public override long GetInt64(int ordinal)
  {
    var val = GetCurrentValue(ordinal);
    try
    {
      if (val is decimal || val is double || val is float || val is short || val is int || val is long)
        return Convert.ToInt64(val, CultureInfo.CurrentCulture);
    }
    catch (Exception e)
    {
      throw WarnAddFormatException(ordinal, $"'{val}' is not a long, {e.Message}");
    }

    return base.GetInt64(ordinal);
  }

  /// <inheritdoc />
  public override int GetValues(object[] values)
  {
    Array.Copy(m_CurrentValues, values, FieldCount);
    return FieldCount;
  }

  /// <inheritdoc />
  public override bool IsDBNull(int ordinal)
  {
    if ((ordinal < 0 && ordinal >= FieldCount) || (m_CurrentValues.Length <= ordinal))
      return true;

    if (Column[ordinal].ValueFormat.DataType == DataTypeEnum.DateTime)
    {
      if (AssociatedTimeCol[ordinal] == -1)
        return m_CurrentValues[ordinal] is null;

      return m_CurrentValues[ordinal] is null && m_CurrentValues[AssociatedTimeCol[ordinal]] is null;
    }

    if (m_CurrentValues[ordinal] is null)
      return true;

    if (m_CurrentValues[ordinal] is string str)
      return string.IsNullOrEmpty(str);

    return false;
  }

  /// <summary>
  /// Adds a value to the collection and synchronizes it with <see cref="m_CurrentValues"/>.
  /// </summary>
  /// <param name="text">The string representation of the value as a <see cref="ReadOnlySpan{Char}"/>.</param>
  /// <param name="typedValue">The underlying object value, or <see langword="null"/>.</param>
  /// <returns>The zero-based index at which the value was added.</returns>
  protected override int Add(ReadOnlySpan<char> text, object? typedValue = null)
  {
    var baseIndex = base.Add(text, typedValue);
    m_CurrentValues[baseIndex] = typedValue;
    return baseIndex;
  }

  /// <summary>
  /// Clears the current text data and typed values.
  /// </summary>
  protected override void Clear()
  {
    base.Clear();
    Array.Clear(m_CurrentValues, 0, m_CurrentValues.Length);
  }

  /// <summary>
  /// Gets the current object stored in <see cref="m_CurrentValues"/>.
  /// </summary>
  /// <param name="ordinal">The zero-based column ordinal.</param>
  /// <returns>The value at the specified ordinal.</returns>
  /// <exception cref="IndexOutOfRangeException">Thrown if the ordinal is outside the bounds of the data row.</exception>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  protected object? GetCurrentValue(int ordinal) => m_CurrentValues[ordinal];

  /// <inheritdoc />
  protected override void InitColumn(int fieldCount)
  {
    m_CurrentValues = new object[fieldCount];
    base.InitColumn(fieldCount);
  }

  protected ReadOnlySpan<char> TrimAndNullHandling(ReadOnlySpan<char> inputString)
  {
    if (m_Trim)
      inputString = inputString.Trim();

    // Standard Null Check (no modifications needed)
    if (inputString.IsEmpty || inputString.SequenceEqual(m_TreatTextAsNull.AsSpan()))
      return ReadOnlySpan<char>.Empty;

    return inputString;
  }

  protected ReadOnlySpan<char> TreatNbsp(ReadOnlySpan<char> inputString)
  {
    if (m_TreatNbspAsSpace && inputString.IndexOf((char) 0xA0)!=-1)
    {
      // 2. We convert to an Array first because the String constructor 
      // in your version of .NET only understands arrays, not Spans.
      char[] arrayBuffer = inputString.ToArray();

      // 3. Manual replacement in the array
      for (int i = 0; i < arrayBuffer.Length; i++)
        if (arrayBuffer[i] == (char) 0xA0) arrayBuffer[i] = ' ';

      return arrayBuffer.AsSpan();
    }

    return inputString;
  }
}