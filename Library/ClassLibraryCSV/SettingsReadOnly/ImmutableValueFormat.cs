// /*
//  * Copyright (C) 2014 Raphael Nöldner : http://csvquickviewer.com
//  *
//  * This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser Public
//  * License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//  *
//  * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
//  * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser Public License for more details.
//  *
//  * You should have received a copy of the GNU Lesser Public License along with this program.
//  * If not, see http://www.gnu.org/licenses/ .
//  *
//  */

using JetBrains.Annotations;

namespace CsvTools
{
  public class ImmutableValueFormat : IValueFormat
  {
    public ImmutableValueFormat(DataType dataType = DataType.String,
      [NotNull] string dateFormat = ValueFormatExtension.cDateFormatDefault,
      [NotNull] string dateSeparator = ValueFormatExtension.cDateSeparatorDefault, char decimalSeparatorChar = '.',
      [NotNull] string displayNullAs = "", [NotNull] string asFalse = ValueFormatExtension.cFalseDefault,
      char groupSeparatorChar = '\0', [NotNull] string numberFormat = ValueFormatExtension.cNumberFormatDefault,
      [NotNull] string timeSeparator = ValueFormatExtension.cTimeSeparatorDefault,
      [NotNull] string asTrue = ValueFormatExtension.cTrueDefault)
    {
      DataType = dataType;
      DateFormat = dateFormat;
      DateSeparator = dateSeparator;
      DecimalSeparatorChar = decimalSeparatorChar;
      DisplayNullAs = displayNullAs;
      False = asFalse;
      GroupSeparatorChar = groupSeparatorChar;
      NumberFormat = numberFormat;
      TimeSeparator = timeSeparator;
      True = asTrue;
    }

    public ImmutableValueFormat(IValueFormat other) : this(other.DataType, other.DateFormat, other.DateSeparator,
      other.DecimalSeparatorChar, other.DisplayNullAs, other.False, other.GroupSeparatorChar, other.NumberFormat,
      other.TimeSeparator, other.True)
    {
    }

    

    public DataType DataType { get; }
    public string DateFormat { get; }
    public string DateSeparator { get; }
    public char DecimalSeparatorChar { get; }
    public string DisplayNullAs { get; }
    public string False { get; }
    public char GroupSeparatorChar { get; }
    public string NumberFormat { get; }
    public string TimeSeparator { get; }
    public string True { get; }
  }
}