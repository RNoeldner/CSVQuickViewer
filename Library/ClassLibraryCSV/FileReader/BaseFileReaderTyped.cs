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
  private readonly bool m_Trim;

  /// <summary>
  /// The typed values of the current row
  /// </summary>
  private object?[] m_CurrentValues;

  /// <summary>
  ///   Constructor for abstract base call for <see cref="T:CsvTools.IFileReader" /> and <see
  ///   cref="T:CsvTools.IFileReader" /> that does read typed values like Excel, SQl
  /// </summary>
  /// <param name="fileName">Path to a physical file (if used)</param>
  /// <param name="columnDefinition">List of column definitions</param>
  /// <param name="recordLimit">Number of records that should be read</param>
  /// <param name="trim">Trim read text</param>
  /// <param name="treatTextAsNull">A semicolon or tab separated list of that should be treated as NULL</param>
  /// <param name="treatNbspAsSpace">nbsp in text will be replaced with Space</param>
  /// <param name="returnedTimeZone">Name of the time zone datetime values that have a source time zone should be converted to</param>
  /// <param name="allowPercentage">If <c>true</c> percentage symbols are is processed to a decimal 26.7% will become .267</param>
  /// <param name="removeCurrency">If <c>true</c> common currency symbols are removed to parse a currency value as decimal</param>
  protected BaseFileReaderTyped(
    string fileName,
    IEnumerable<Column>? columnDefinition,
    long recordLimit,
    bool trim,
    string treatTextAsNull,
    bool treatNbspAsSpace,
    string returnedTimeZone,
    bool allowPercentage,
    bool removeCurrency)
    : base(fileName, columnDefinition, recordLimit, treatTextAsNull, returnedTimeZone, allowPercentage, removeCurrency)
  {
    m_TreatNbspAsSpace = treatNbspAsSpace;
    m_Trim = trim;
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
  protected override bool IsDBNull(in Column column)
  {
    // 1. Exit immediately for ignored columns
    if (column.Ignore) return true;

    var ordinal = column.ColumnOrdinal;
    var dataType = column.ValueFormat.DataType;

    // 2. Specialized DateTime logic
    if (dataType == DataTypeEnum.DateTime)
    {
      // Optimization: Null check first (memory read), then check span (logic/calculation)
      // If the date part has any data, return false immediately.
      if (m_CurrentValues[ordinal] is not null || !GetSpan(ordinal).IsEmpty)
        return false;

      int timeOrdinal = AssociatedTimeCol[ordinal];

      // Fast bounds check: handles both -1 and out-of-range via uint cast
      if ((uint) timeOrdinal >= (uint) m_CurrentValues.Length)
        return true;

      // If Date was empty, both Time reference and Time span must be empty for a true NULL
      return m_CurrentValues[timeOrdinal] is null && GetSpan(timeOrdinal).IsEmpty;
    }

    // 3. String logic: String emptiness is the definition of NULL in this context
    if (dataType == DataTypeEnum.String)
    {
      return GetSpan(ordinal).IsEmpty;
    }

    // 4. Fallback for primitives: A null check on the current value object , but the cell could be a typed cell but teh value came back as text
    return m_CurrentValues[ordinal] is null && GetSpan(ordinal).IsEmpty;
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

  /// <summary>
  /// Orchestrates text processing including formatting, null-equivalent detection, 
  /// trimming, and non-breaking space replacement.
  /// </summary>
  /// <param name="textSpan">The raw character span to process.</param>
  /// <param name="columnNo">The column index for metadata and formatter lookups.</param>
  /// <returns>A processed span that is potentially trimmed, formatted, or cleaned.</returns>
  protected override string HandleText(ReadOnlySpan<char> textSpan, int columnNo)
  {
    // 1. Get base string (Handles formatting and initial null checks)
    string text = base.HandleText(textSpan, columnNo);

    if (string.IsNullOrEmpty(text))
      return string.Empty;

    // Use a span for trimming and scanning
    ReadOnlySpan<char> processedSpan = text.AsSpan();

    // 2. Trimming
    if (m_Trim)
      processedSpan = processedSpan.Trim();

    if (processedSpan.IsEmpty)
      return string.Empty;

    // 3. NBSP Treatment (\u00A0)
    if (m_TreatNbspAsSpace)
    {
      int firstNbsp = processedSpan.IndexOf('\u00A0');
      if (firstNbsp != -1)
      {
        // Fallback: Copy to array and modify
        char[] buffer = processedSpan.ToArray();
        for (int i = firstNbsp; i < buffer.Length; i++)
        {
          if (buffer[i] == '\u00A0')
            buffer[i] = ' ';
        }
        return new string(buffer);
      }
    }

    // 4. Final return optimization: 
    // If length hasn't changed (no trimming happened), return original string object.
    if (processedSpan.Length == text.Length)
      return text;

    return processedSpan.ToString();
  }
}