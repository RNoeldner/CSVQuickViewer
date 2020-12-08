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

namespace CsvTools
{
  public class ImmutableValueFormat : IValueFormat
  {
    public ImmutableValueFormat(DataType dataType = DataType.String,
                                string dateFormat = ValueFormatExtension.cDateFormatDefault,
                                string dateSeparator = ValueFormatExtension.cDateSeparatorDefault,
                                char decimalSeparatorChar = '.',
                                string displayNullAs = "",
                                string asFalse = ValueFormatExtension.cFalseDefault,
                                char groupSeparatorChar = '\0',
                                string numberFormat = ValueFormatExtension.cNumberFormatDefault,
                                string timeSeparator = ValueFormatExtension.cTimeSeparatorDefault,
                                string asTrue = ValueFormatExtension.cTrueDefault)
    {
      DataType = dataType;
      DateFormat = dateFormat??throw new System.ArgumentNullException(nameof(dateFormat));
      DateSeparator = dateSeparator??throw new System.ArgumentNullException(nameof(dateSeparator));
      DecimalSeparatorChar = decimalSeparatorChar;
      DisplayNullAs = displayNullAs??throw new System.ArgumentNullException(nameof(displayNullAs));
      False = asFalse??throw new System.ArgumentNullException(nameof(asFalse));
      GroupSeparatorChar = groupSeparatorChar;
      NumberFormat = numberFormat??throw new System.ArgumentNullException(nameof(numberFormat));
      TimeSeparator = timeSeparator??throw new System.ArgumentNullException(nameof(timeSeparator));
      True = asTrue??throw new System.ArgumentNullException(nameof(asTrue));
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