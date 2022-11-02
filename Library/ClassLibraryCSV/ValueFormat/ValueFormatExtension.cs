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
using System.Globalization;
using System.Text;

namespace CsvTools
{
  public static class ValueFormatExtension
  {
    public static readonly ValueFormat Default = new ValueFormat();

    public const string cDateFormatDefault = "MM/dd/yyyy";
    public const string cDateSeparatorDefault = "/";
    public const string cDecimalSeparatorDefault = ".";
    public const string cFalseDefault = "False";
    public const string cGroupSeparatorDefault = "";
    public const string cNumberFormatDefault = "0.#####";
    public const int cPartDefault = 2;
    public const string cPartSplitterDefault = ":";
    public const bool cPartToEndDefault = true;
    public const string cTimeSeparatorDefault = ":";
    public const string cTrueDefault = "True";

    /// <summary>
    ///   Gets the a description of the Date or Number format
    /// </summary>
    /// <returns></returns>
    public static string GetFormatDescription(this ValueFormat one) =>
      one.DataType switch
      {
        DataTypeEnum.Integer => one.NumberFormat.Replace(
          CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator,
          one.GroupSeparator),
        DataTypeEnum.DateTime => one.DateFormat.ReplaceDefaults(
          CultureInfo.InvariantCulture.DateTimeFormat.DateSeparator,
          one.DateSeparator,
          CultureInfo.InvariantCulture.DateTimeFormat.TimeSeparator,
          one.TimeSeparator),
        DataTypeEnum.Numeric => one.NumberFormat.ReplaceDefaults(
          CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator,
          one.DecimalSeparator,
          CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator,
          one.GroupSeparator),
        DataTypeEnum.Double => one.NumberFormat.ReplaceDefaults(
          CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator,
          one.DecimalSeparator,
          CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator,
          one.GroupSeparator),
        DataTypeEnum.TextPart => $"{one.Part}" + (one.PartToEnd ? " To End" : string.Empty),
        DataTypeEnum.Binary => $"Read file from {one.ReadFolder}",
        DataTypeEnum.TextReplace =>
          $"Replace {StringUtils.GetShortDisplay(one.RegexSearchPattern, 10)} with {StringUtils.GetShortDisplay(one.RegexReplacement, 10)}",
        _ => string.Empty
      };

    /// <summary>
    ///   Gets the description.
    /// </summary>
    /// <returns></returns>
    public static string GetTypeAndFormatDescription(this ValueFormat one)
    {
      var sbText = new StringBuilder(one.DataType.DataTypeDisplay());

      var shortDesc = one.GetFormatDescription();
      if (shortDesc.Length <= 0)
        return sbText.ToString();
      sbText.Append(" (");
      sbText.Append(shortDesc);
      sbText.Append(")");

      return sbText.ToString();
    }

    public static bool IsMatching(this ValueFormatMut one, ValueFormatMut other) =>
      IsMatching(one.ToImmutable(), other.ToImmutable());

    /// <summary>
    ///   Determines whether the specified expected column is matching this column.
    /// </summary>
    /// <param name="one"></param>
    /// <param name="other">The expected column format.</param>
    /// <returns>
    ///   <c>true</c> if the current format would be acceptable for the expected data type.
    /// </returns>
    /// <remarks>
    ///   Is matching only looks at data type and some formats, it is assumed that we do not
    ///   distinguish between numeric formats, it is O.K. to expect a money value but have a integer
    /// </remarks>
    public static bool IsMatching(this ValueFormat one, ValueFormat other)
    {
      if (other.DataType == one.DataType)
        return true;

      // if one is integer but we expect numeric or vice versa, assume its OK, one of the sides does
      // not have a decimal separator
      if ((other.DataType == DataTypeEnum.Numeric || other.DataType == DataTypeEnum.Double
                                                  || other.DataType == DataTypeEnum.Integer)
          && one.DataType == DataTypeEnum.Integer)
        return true;

      // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
      switch (other.DataType)
      {
        case DataTypeEnum.Integer when one.DataType == DataTypeEnum.Numeric || one.DataType == DataTypeEnum.Double
                                                                            || one.DataType == DataTypeEnum.Integer:
          return true;
        // if we have dates, check the formats
        case DataTypeEnum.DateTime when one.DataType == DataTypeEnum.DateTime:
          return other.DateFormat.Equals(one.DateFormat, StringComparison.Ordinal)
                 && (one.DateFormat.IndexOf('/') == -1 || other.DateSeparator.Equals(
                   one.DateSeparator,
                   StringComparison.Ordinal)) && (one.DateFormat.IndexOf(':') == -1
                                                  || other.TimeSeparator.Equals(
                                                    one.TimeSeparator,
                                                    StringComparison.Ordinal));
      }

      // if we have decimals, check the formats
      if ((other.DataType == DataTypeEnum.Numeric || other.DataType == DataTypeEnum.Double)
          && (one.DataType == DataTypeEnum.Numeric || one.DataType == DataTypeEnum.Double))
        return other.NumberFormat.Equals(one.NumberFormat, StringComparison.Ordinal)
               && other.DecimalSeparator.Equals(one.DecimalSeparator)
               && other.GroupSeparator.Equals(one.GroupSeparator);
      // For everything else assume its wrong
      return false;
    }

    public static bool IsDefault(this ValueFormat one) => one.Equals(Default);

    /// <summary>
    /// Returns  an immutable ValueFormat if the source column was immutable the very same is returned, not copy is created
    /// </summary>
    /// <param name="source">an IValueFormat with the all properties for the new format</param>
    /// <returns>and immutable column</returns>
    public static ValueFormat ToImmutable(this ValueFormatMut source)
    => new ValueFormat(source.DataType, source.DateFormat,
        source.DateSeparator, source.TimeSeparator, source.NumberFormat, source.GroupSeparator,
        source.DecimalSeparator, source.True, source.False, source.DisplayNullAs,
        source.Part, source.PartSplitter, source.PartToEnd, source.RegexSearchPattern,
        source.RegexReplacement, source.ReadFolder, source.WriteFolder, source.FileOutPutPlaceholder, source.Overwrite);

    /// <summary>
    /// Returns  mutable ValueFormat that is not related to the original column
    /// </summary>
    /// <param name="source">an IValueFormat with the all properties for the new format</param>
    /// <returns></returns>
    public static ValueFormatMut ToMutable(this ValueFormat source)
    => new ValueFormatMut(source.DataType, source.DateFormat, source.DateSeparator,
          source.TimeSeparator, source.NumberFormat,
          source.GroupSeparator, source.DecimalSeparator,
          source.True, source.False, source.DisplayNullAs, source.Part, source.PartSplitter, source.PartToEnd,
          source.RegexSearchPattern, source.RegexReplacement, source.ReadFolder, source.WriteFolder,
          source.FileOutPutPlaceholder, source.Overwrite);

  }
}