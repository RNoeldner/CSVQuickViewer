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

using Newtonsoft.Json;
using System;
using System.ComponentModel;
namespace CsvTools
{
  /// <inheritdoc cref="System.ICloneable" />
  /// <summary>
  ///   Settings how the typed values should be determined
  /// </summary>
  [Serializable]
#pragma warning disable CA1067 // Override Object.Equals(object) when implementing IEquatable<T>
  public sealed class FillGuessSettings : ObservableObject, ICloneable, IEquatable<FillGuessSettings>
#pragma warning restore CA1067 // Override Object.Equals(object) when implementing IEquatable<T>
  {
    private long m_CheckedRecords;
    private bool m_DetectBoolean;
    private bool m_DetectDateTime;
    private bool m_DetectGuid;
    private bool m_DetectNumbers;
    private bool m_DetectPercentage;
    private bool m_Enabled;
    private bool m_IgnoreIdColumns;
    private int m_MinSamples;
    private int m_SampleValues;
    private bool m_SerialDateTime;
    private string m_TrueValue;
    private string m_FalseValue;
    private string m_DateFormat;
    private bool m_DateParts;
    private bool m_RemoveCurrencySymbols;

    /// <summary>
    /// Default instance for FillGuessSettings
    /// </summary>
    public static FillGuessSettings Default = new FillGuessSettings();

    /// <inheritdoc />
    [JsonConstructor]
    public FillGuessSettings(bool? enabled = true, bool? ignoreIdColumns = true, bool? detectBoolean = true,
      bool? detectDateTime = true,
      bool? detectNumbers = true, bool? detectPercentage = true, bool? detectGuid = false, bool? serialDateTime = true,
      bool? dateParts = false, int? minSamples = 3, int? sampleValues = 150, long? checkedRecords = 30000,
      string? trueValue = "True", string? falseValue = "False", string? dateFormat = "",
      bool? removeCurrencySymbols = true)
    {
      m_Enabled = enabled ?? true;
      m_IgnoreIdColumns = ignoreIdColumns ?? true;
      m_DetectBoolean = detectBoolean ?? true;
      m_DetectDateTime = detectDateTime ?? true;
      m_DetectNumbers = detectNumbers ?? true;
      m_DetectPercentage = detectPercentage?? true;
      m_DetectGuid = detectGuid ?? false;
      m_SerialDateTime = serialDateTime?? true;
      m_DateParts=dateParts ?? false;
      m_MinSamples = minSamples ?? 3;
      m_SampleValues = sampleValues ?? 150;
      m_CheckedRecords = checkedRecords ?? 30000;
      m_TrueValue = trueValue ?? "True";
      m_FalseValue = falseValue ?? "False";
      m_DateFormat = dateFormat ?? string.Empty;
      m_RemoveCurrencySymbols = removeCurrencySymbols ?? true;
    }

    /// <summary>
    /// If  guessing the values types is enabled in general
    /// </summary>
    [DefaultValue(true)]
    public bool Enabled
    {
      get => m_Enabled;
      set => SetProperty(ref m_Enabled, value);
    }

    /// <summary>
    /// Allowing currency symbols to be part of the input, storing only the values
    /// </summary>
    [DefaultValue(true)]
    public bool RemoveCurrencySymbols
    {
      get => m_RemoveCurrencySymbols;
      set => SetProperty(ref m_RemoveCurrencySymbols, value);
    }

    /// <summary>
    ///   Number of records to parse to get the sample values, default is <c>30000</c>
    /// </summary>
    [DefaultValue(30000)]
    public long CheckedRecords
    {
      get => m_CheckedRecords;
      set => SetProperty(ref m_CheckedRecords, value);
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they have a date part like a time or time, default is <c>false</c>
    /// </summary>
    [DefaultValue(false)]
    public bool DateParts
    {
      get => m_DateParts;
      set => SetProperty(ref m_DateParts, value);
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be Numeric, default is <c>True</c>
    /// </summary>
    [DefaultValue(true)]
    public bool DetectNumbers
    {
      get => m_DetectNumbers;
      set => SetProperty(ref m_DetectNumbers, value);
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be Percentages, default is <c>True</c>
    /// </summary>
    [DefaultValue(true)]
    public bool DetectPercentage
    {
      get => m_DetectPercentage;
      set => SetProperty(ref m_DetectPercentage, value);
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be Boolean, default is <c>True</c>
    /// </summary>
    [DefaultValue(true)]
    public bool DetectBoolean
    {
      get => m_DetectBoolean;
      set => SetProperty(ref m_DetectBoolean, value);
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be Date or Times, default is <c>True</c>
    /// </summary>
    [DefaultValue(true)]
    public bool DetectDateTime
    {
      get => m_DetectDateTime;
      set => SetProperty(ref m_DetectDateTime, value);
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be GUIDs, default is <c>False</c>
    /// </summary>
    [DefaultValue(false)]
    public bool DetectGuid
    {
      get => m_DetectGuid;
      set => SetProperty(ref m_DetectGuid, value);
    }

    /// <summary>
    ///   Flag to ignore columns that seem to be Identifiers, default is <c>True</c>
    /// </summary>
    [DefaultValue(true)]
    public bool IgnoreIdColumns
    {
      get => m_IgnoreIdColumns;
      set => SetProperty(ref m_IgnoreIdColumns, value);
    }

    /// <summary>
    ///   Number of sample values, default is <c>3</c>
    /// </summary>
    [DefaultValue(3)]
    public int MinSamples
    {
      get => m_MinSamples;
      set => SetProperty(ref m_MinSamples, value);
    }

    /// <summary>
    ///   Number of sample values, default is <c>150</c>
    /// </summary>
    [DefaultValue(150)]
    public int SampleValues
    {
      get => m_SampleValues;
      set => SetProperty(ref m_SampleValues, value);
    }

    /// <summary>
    ///   If set to <c>True</c> values are checked if they could be serial Date or Times
    /// </summary>
    [DefaultValue(true)]
    public bool SerialDateTime
    {
      get => m_SerialDateTime;
      set => SetProperty(ref m_SerialDateTime, value);
    }

    /// <summary>
    ///   List of text to be regarded as <c>true</c>
    /// </summary>
    [DefaultValue("True")]
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string TrueValue
    {
      get => m_TrueValue;
      // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
      set => SetProperty(ref m_TrueValue, value ?? "True");
    }

    /// <summary>
    ///   List of text to be regarded as <c>false</c>, default text is <c>"False"</c>
    /// </summary>
    [DefaultValue("False")]
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string FalseValue
    {
      get => m_FalseValue;
      // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
      set => SetProperty(ref m_FalseValue, value ?? "False");
    }

    /// <summary>
    /// General format for Dates, e.G. YYYY/MM/DD
    /// </summary>
    [DefaultValue("")]
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
    public string DateFormat
    {
      get => m_DateFormat;
      // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
      set => SetProperty(ref m_DateFormat, value ?? string.Empty);
    }

    /// <inheritdoc />
    public object Clone()
    {
      return new FillGuessSettings(enabled: m_Enabled, ignoreIdColumns: m_IgnoreIdColumns, detectBoolean: m_DetectBoolean, detectDateTime: m_DetectDateTime, detectNumbers: m_DetectNumbers, detectPercentage: m_DetectPercentage,
        detectGuid: m_DetectGuid, serialDateTime: m_SerialDateTime, dateParts: m_DateParts, minSamples: m_MinSamples, sampleValues: m_SampleValues, checkedRecords: m_CheckedRecords, trueValue: m_TrueValue, falseValue: m_FalseValue);
    }

    /// <inheritdoc />
    public bool Equals(FillGuessSettings? other)
    {
      if (other is null)
        return false;

      return m_Enabled == other.Enabled &&
             m_CheckedRecords == other.CheckedRecords &&
             m_DateParts == other.DateParts &&
             m_DetectNumbers == other.DetectNumbers &&
             m_DetectPercentage == other.DetectPercentage &&
             m_DetectBoolean == other.DetectBoolean &&
             m_DetectDateTime == other.DetectDateTime &&
             m_DetectGuid == other.DetectGuid &&
             m_IgnoreIdColumns == other.IgnoreIdColumns &&
             m_MinSamples == other.MinSamples &&
             m_SampleValues == other.SampleValues &&
             m_SerialDateTime == other.SerialDateTime &&
             string.Equals(m_FalseValue, other.FalseValue, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(m_TrueValue, other.TrueValue, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(m_DateFormat, other.DateFormat, StringComparison.Ordinal);
    }


    /// <summary>
    ///   Copy all properties to another instance of FillGuessSettings
    /// </summary>
    /// <param name="other"></param>
    public void CopyTo(FillGuessSettings other)
    {
      other.Enabled = m_Enabled;
      other.CheckedRecords = m_CheckedRecords;
      other.DetectNumbers = m_DetectNumbers;
      other.DetectPercentage = m_DetectPercentage;
      other.DetectBoolean = m_DetectBoolean;
      other.DateParts = m_DateParts;
      other.DetectDateTime = m_DetectDateTime;
      other.DetectGuid = m_DetectGuid;
      other.TrueValue = m_TrueValue;
      other.FalseValue = m_FalseValue;
      other.IgnoreIdColumns = m_IgnoreIdColumns;
      other.MinSamples = m_MinSamples;
      other.SampleValues = m_SampleValues;
      other.SerialDateTime = m_SerialDateTime;
      other.DateFormat = m_DateFormat;
    }
  }
}