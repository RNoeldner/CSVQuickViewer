// /*
// * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com *
// * This program is free software: you can redistribute it and/or modify it under the terms of the
// GNU Lesser Public
// * License as published by the Free Software Foundation, either version 3 of the License, or (at
// your option) any later version. *
// * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty
// * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for
// more details. *
// * You should have received a copy of the GNU Lesser Public License along with this program.
// * If not, see http://www.gnu.org/licenses/ . *
// */

using System;
using System.Globalization;
using System.Text;

namespace CsvTools
{
  public static class ValueFormatExtension
  {
    public const string cDateFormatDefault = "MM/dd/yyyy";
    public const string cDateSeparatorDefault = "/";
    public const string cDecimalSeparatorDefault = ".";
    public const string cFalseDefault = "False";
    public const string cGroupSeparatorDefault = "";
    public const string cNumberFormatDefault = "0.#####";
    public const string cTimeSeparatorDefault = ":";
    public const string cTrueDefault = "True";


    public static bool ValueFormatEqual(this IValueFormat one, IValueFormat? other)
    {
      if (other is null)
        return false;
      if (ReferenceEquals(one, other))
        return true;

      if (other.DataType != one.DataType || !other.DisplayNullAs.Equals(one.DisplayNullAs, StringComparison.Ordinal))
        return false;

      return one.DataType switch
      {
        DataType.Integer => string.Equals(other.NumberFormat, one.NumberFormat, StringComparison.Ordinal),
        DataType.Numeric => other.GroupSeparator == one.GroupSeparator && other.DecimalSeparator == one.DecimalSeparator &&
                            string.Equals(other.NumberFormat, one.NumberFormat, StringComparison.Ordinal),
        DataType.Double => other.GroupSeparator == one.GroupSeparator && other.DecimalSeparator == one.DecimalSeparator &&
                           string.Equals(other.NumberFormat, one.NumberFormat, StringComparison.Ordinal),
        DataType.DateTime => string.Equals(other.DateFormat, one.DateFormat, StringComparison.Ordinal) &&
                             string.Equals(other.DateSeparator, one.DateSeparator, StringComparison.Ordinal) &&
                             string.Equals(other.TimeSeparator, one.TimeSeparator, StringComparison.Ordinal),
        DataType.Boolean => string.Equals(other.False, one.False, StringComparison.OrdinalIgnoreCase) &&
                            string.Equals(other.True, one.True, StringComparison.OrdinalIgnoreCase),
        _ => string.Equals(other.DateFormat, one.DateFormat, StringComparison.Ordinal) &&
             string.Equals(other.DateSeparator, one.DateSeparator, StringComparison.Ordinal) &&
             string.Equals(other.TimeSeparator, one.TimeSeparator, StringComparison.Ordinal) &&
             string.Equals(other.False, one.False, StringComparison.OrdinalIgnoreCase) &&
             string.Equals(other.True, one.True, StringComparison.OrdinalIgnoreCase) &&
             other.GroupSeparator == one.GroupSeparator && other.DecimalSeparator == one.DecimalSeparator &&
             string.Equals(other.NumberFormat, one.NumberFormat, StringComparison.Ordinal)
      };
    }

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
    public static bool IsMatching(this IValueFormat one, IValueFormat other)
    {
      if (other.DataType == one.DataType)
        return true;

      // if one is integer but we expect numeric or vice versa, assume its OK, one of the sides does
      // not have a decimal separator
      if ((other.DataType == DataType.Numeric || other.DataType == DataType.Double || other.DataType == DataType.Integer)
          && one.DataType == DataType.Integer)
        return true;

      // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
      switch (other.DataType)
      {
        case DataType.Integer when one.DataType == DataType.Numeric || one.DataType == DataType.Double ||
                                   one.DataType == DataType.Integer:
          return true;
        // if we have dates, check the formats
        case DataType.DateTime when one.DataType == DataType.DateTime:
          return other.DateFormat.Equals(one.DateFormat, StringComparison.Ordinal) &&
                 (one.DateFormat.IndexOf('/') == -1 ||
                  other.DateSeparator.Equals(one.DateSeparator, StringComparison.Ordinal)) &&
                 (one.DateFormat.IndexOf(':') == -1 ||
                  other.TimeSeparator.Equals(one.TimeSeparator, StringComparison.Ordinal));
      }

      // if we have decimals, check the formats
      if ((other.DataType == DataType.Numeric || other.DataType == DataType.Double) &&
          (one.DataType == DataType.Numeric || one.DataType == DataType.Double))
        return other.NumberFormat.Equals(one.NumberFormat, StringComparison.Ordinal) &&
               other.DecimalSeparator.Equals(one.DecimalSeparator) &&
               other.GroupSeparator.Equals(one.GroupSeparator);
      // For everything else assume its wrong
      return false;
    }

    /// <summary>
    ///   Gets the a description of the Date or Number format
    /// </summary>
    /// <returns></returns>
    public static string GetFormatDescription(this IValueFormat one) =>
      one.DataType switch
      {
        DataType.Integer => one.NumberFormat.Replace(CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator,
          one.GroupSeparator),
        DataType.DateTime => one.DateFormat.ReplaceDefaults(CultureInfo.InvariantCulture.DateTimeFormat.DateSeparator,
          one.DateSeparator, CultureInfo.InvariantCulture.DateTimeFormat.TimeSeparator, one.TimeSeparator),
        DataType.Numeric => one.NumberFormat.ReplaceDefaults(
          CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, one.DecimalSeparator,
          CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator, one.GroupSeparator),
        DataType.Double => one.NumberFormat.ReplaceDefaults(
          CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, one.DecimalSeparator,
          CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator, one.GroupSeparator),
        _ => string.Empty
      };

    /// <summary>
    ///   Gets the description.
    /// </summary>
    /// <returns></returns>
    public static string GetTypeAndFormatDescription(this IValueFormat one)
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
  }
}