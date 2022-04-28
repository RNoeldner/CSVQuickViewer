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

namespace CsvTools
{
  /// <inheritdoc cref="CsvTools.IValueFormat" />
  public class ImmutableValueFormat : IValueFormat
  {

    public ImmutableValueFormat(IValueFormat valueFormat) : this(valueFormat.DataType, valueFormat.DateFormat,
      valueFormat.DateSeparator, valueFormat.TimeSeparator, valueFormat.NumberFormat, valueFormat.GroupSeparator, valueFormat.DecimalSeparator, valueFormat.True, valueFormat.False, valueFormat.DisplayNullAs,
      valueFormat.Part, valueFormat.PartSplitter, valueFormat.PartToEnd, valueFormat.RegexSearchPattern, valueFormat.RegexReplacement, valueFormat.ReadFolder)
    {
    }

    public ImmutableValueFormat(
    in DataType dataType = DataType.String,
    in string dateFormat = ValueFormatExtension.cDateFormatDefault,
    in string dateSeparator = ValueFormatExtension.cDateSeparatorDefault,
    in string timeSeparator = ValueFormatExtension.cTimeSeparatorDefault,
    in string numberFormat = ValueFormatExtension.cNumberFormatDefault,
    in string groupSeparator = ValueFormatExtension.cGroupSeparatorDefault,
    in string decimalSeparator = ValueFormatExtension.cDecimalSeparatorDefault,
    in string asTrue = ValueFormatExtension.cTrueDefault,
    in string asFalse = ValueFormatExtension.cFalseDefault,
    in string displayNullAs = "",
    int part = ValueFormatExtension.cPartDefault,
    in string partSplitter = ValueFormatExtension.cPartSplitterDefault,
    bool partToEnd = ValueFormatExtension.cPartToEndDefault,
    string regexSearchPattern = "",
    string regexReplacement = "",
    string readFolder = "",
    string writeFolder = "",
    string fileOutPutPlaceholder = "",
    bool overwrite = true)
    {
      DataType = dataType;
      DateFormat = dateFormat ?? throw new ArgumentNullException(nameof(dateFormat));
      DateSeparator = dateSeparator ?? throw new ArgumentNullException(nameof(dateSeparator));
      DecimalSeparator = decimalSeparator ?? throw new ArgumentNullException(nameof(decimalSeparator));
      GroupSeparator = groupSeparator ?? throw new ArgumentNullException(nameof(groupSeparator));
      DisplayNullAs = displayNullAs ?? throw new ArgumentNullException(nameof(displayNullAs));
      False = asFalse ?? throw new ArgumentNullException(nameof(asFalse));
      NumberFormat = numberFormat ?? throw new ArgumentNullException(nameof(numberFormat));
      TimeSeparator = timeSeparator ?? throw new ArgumentNullException(nameof(timeSeparator));
      True = asTrue ?? throw new ArgumentNullException(nameof(asTrue));
      Part = part;
      PartSplitter = partSplitter ?? throw new ArgumentNullException(nameof(partSplitter));
      PartToEnd = partToEnd;
      RegexSearchPattern = regexSearchPattern ?? throw new ArgumentNullException(nameof(regexSearchPattern));
      RegexReplacement = regexReplacement ?? throw new ArgumentNullException(nameof(regexReplacement));
      ReadFolder = readFolder ?? throw new ArgumentNullException(nameof(readFolder));
      WriteFolder =writeFolder ?? throw new ArgumentNullException(nameof(writeFolder));
      FileOutPutPlaceholder =fileOutPutPlaceholder ?? throw new ArgumentNullException(nameof(fileOutPutPlaceholder));
      Overwrite = overwrite;
    }

    /// <inheritdoc />
    public DataType DataType { get; }

    /// <inheritdoc />
    public string DateFormat { get; }

    /// <inheritdoc />
    public string DateSeparator { get; }

    /// <inheritdoc />
    public string DecimalSeparator { get; }

    /// <inheritdoc />
    public string DisplayNullAs { get; }

    /// <inheritdoc />
    public string False { get; }

    /// <inheritdoc />
    public string GroupSeparator { get; }

    /// <inheritdoc />
    public string NumberFormat { get; }

    /// <inheritdoc />
    public int Part { get; }

    /// <inheritdoc />
    public string PartSplitter { get; }

    /// <inheritdoc />
    public bool PartToEnd { get; }

    /// <inheritdoc />
    public string TimeSeparator { get; }

    /// <inheritdoc />
    public string True { get; }

    /// <inheritdoc />
    public string RegexSearchPattern { get; }

    /// <inheritdoc />
    public string RegexReplacement { get; }

    /// <inheritdoc />
    public string ReadFolder { get; }

    /// <inheritdoc />
    public string WriteFolder { get; }

    /// <inheritdoc />
    public string FileOutPutPlaceholder { get; }

    /// <inheritdoc />
    public bool Overwrite { get; }
  }
}