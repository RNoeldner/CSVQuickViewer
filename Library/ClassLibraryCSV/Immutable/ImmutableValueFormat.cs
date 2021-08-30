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
// * If not, see http://www.gnu.org/licenses/
// */

namespace CsvTools
{
  public class ImmutableValueFormat : IValueFormat
  {
    public ImmutableValueFormat(DataType dataType = DataType.String,
      string dateFormat = ValueFormatExtension.cDateFormatDefault,
      string dateSeparator = ValueFormatExtension.cDateSeparatorDefault,
      string timeSeparator = ValueFormatExtension.cTimeSeparatorDefault,
      string numberFormat = ValueFormatExtension.cNumberFormatDefault,
      string groupSeparator = ValueFormatExtension.cGroupSeparatorDefault,
      string decimalSeparator = ValueFormatExtension.cDecimalSeparatorDefault,
      string asTrue = ValueFormatExtension.cTrueDefault,
      string asFalse = ValueFormatExtension.cFalseDefault,
      string displayNullAs = "",
      int part = ValueFormatExtension.cPartDefault,
      string partSplitter = ValueFormatExtension.cPartSplitterDefault,
      bool partToEnd = ValueFormatExtension.cPartToEndDefault)
    {
      if (!string.IsNullOrEmpty(decimalSeparator) && decimalSeparator.Equals(groupSeparator))
        throw new FileReaderException("Decimal and Group separator must be different");
      DataType = dataType;
      DateFormat = dateFormat??throw new System.ArgumentNullException(nameof(dateFormat));
      DateSeparator = (dateSeparator??throw new System.ArgumentNullException(nameof(dateSeparator))).WrittenPunctuation();
      DecimalSeparator = (decimalSeparator??throw new System.ArgumentNullException(nameof(decimalSeparator))).WrittenPunctuation();
      GroupSeparator = (groupSeparator??throw new System.ArgumentNullException(nameof(groupSeparator))).WrittenPunctuation();
      DisplayNullAs = displayNullAs??throw new System.ArgumentNullException(nameof(displayNullAs));
      False = asFalse??throw new System.ArgumentNullException(nameof(asFalse));
      NumberFormat = numberFormat??throw new System.ArgumentNullException(nameof(numberFormat));
      TimeSeparator = timeSeparator??throw new System.ArgumentNullException(nameof(timeSeparator));
      True = asTrue??throw new System.ArgumentNullException(nameof(asTrue));
      Part = part;
      PartSplitter = (partSplitter??throw new System.ArgumentNullException(nameof(partSplitter))).WrittenPunctuation();
      PartToEnd= partToEnd;
    }

    public DataType DataType { get; }

    public string DateFormat { get; }

    public string DateSeparator { get; }

    public string DecimalSeparator { get; }

    public string DisplayNullAs { get; }

    public string False { get; }

    public string GroupSeparator { get; }

    public string NumberFormat { get; }

    public int Part { get; }

    public string PartSplitter { get; }

    public bool PartToEnd { get; }

    /// <summary>
    ///   Used in Serialization to determine if something needs to be stored
    /// </summary>
    public bool Specified =>
      DataType == DataType.String &&
      DateFormat == ValueFormatExtension.cDateFormatDefault &&
      DateSeparator == ValueFormatExtension.cDateSeparatorDefault &&
      TimeSeparator == ValueFormatExtension.cTimeSeparatorDefault &&
      NumberFormat == ValueFormatExtension.cNumberFormatDefault &&
      DecimalSeparator == ValueFormatExtension.cDecimalSeparatorDefault &&
      GroupSeparator == ValueFormatExtension.cGroupSeparatorDefault &&
      True == ValueFormatExtension.cTrueDefault &&
      False == ValueFormatExtension.cFalseDefault &&
      Part   == ValueFormatExtension.cPartDefault &&
      PartSplitter   == ValueFormatExtension.cPartSplitterDefault &&
      PartToEnd   == ValueFormatExtension.cPartToEndDefault &&
      DisplayNullAs == string.Empty;

    public string TimeSeparator { get; }
    public string True { get; }

    public bool Equals(IValueFormat? other) => this.ValueFormatEqual(other);
  }
}