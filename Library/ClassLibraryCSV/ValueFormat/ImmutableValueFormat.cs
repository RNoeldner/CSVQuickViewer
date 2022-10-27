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

namespace CsvTools
{
  /// <inheritdoc cref="CsvTools.IValueFormat" />
  public sealed class ImmutableValueFormat : IValueFormat, IEquatable<ImmutableValueFormat>
  {
    public ImmutableValueFormat(IValueFormat valueFormat) : this(valueFormat.DataType, valueFormat.DateFormat,
      valueFormat.DateSeparator, valueFormat.TimeSeparator, valueFormat.NumberFormat, valueFormat.GroupSeparator,
      valueFormat.DecimalSeparator, valueFormat.True, valueFormat.False, valueFormat.DisplayNullAs,
      valueFormat.Part, valueFormat.PartSplitter, valueFormat.PartToEnd, valueFormat.RegexSearchPattern,
      valueFormat.RegexReplacement, valueFormat.ReadFolder, valueFormat.WriteFolder, valueFormat.FileOutPutPlaceholder, valueFormat.Overwrite)
    {
    }

    [JsonConstructor]
    public ImmutableValueFormat(
      in DataTypeEnum dataType = DataTypeEnum.String,
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
      WriteFolder = writeFolder ?? throw new ArgumentNullException(nameof(writeFolder));
      FileOutPutPlaceholder = fileOutPutPlaceholder ?? throw new ArgumentNullException(nameof(fileOutPutPlaceholder));
      Overwrite = overwrite;
    }

    /// <inheritdoc />
    public DataTypeEnum DataType { get; }

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

    /// <inheritdoc />
    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = (int) DataType;
        hashCode = (hashCode * 397) ^ DateFormat.GetHashCode();
        hashCode = (hashCode * 397) ^ DateSeparator.GetHashCode();
        hashCode = (hashCode * 397) ^ DecimalSeparator.GetHashCode();
        hashCode = (hashCode * 397) ^ DisplayNullAs.GetHashCode();
        hashCode = (hashCode * 397) ^ False.GetHashCode();
        hashCode = (hashCode * 397) ^ GroupSeparator.GetHashCode();
        hashCode = (hashCode * 397) ^ NumberFormat.GetHashCode();
        hashCode = (hashCode * 397) ^ Part;
        hashCode = (hashCode * 397) ^ PartSplitter.GetHashCode();
        hashCode = (hashCode * 397) ^ PartToEnd.GetHashCode();
        hashCode = (hashCode * 397) ^ TimeSeparator.GetHashCode();
        hashCode = (hashCode * 397) ^ True.GetHashCode();
        hashCode = (hashCode * 397) ^ RegexSearchPattern.GetHashCode();
        hashCode = (hashCode * 397) ^ RegexReplacement.GetHashCode();
        hashCode = (hashCode * 397) ^ ReadFolder.GetHashCode();
        hashCode = (hashCode * 397) ^ WriteFolder.GetHashCode();
        hashCode = (hashCode * 397) ^ FileOutPutPlaceholder.GetHashCode();
        hashCode = (hashCode * 397) ^ Overwrite.GetHashCode();
        return hashCode;
      }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => this.ValueFormatEqual(obj as IValueFormat);

    /// <inheritdoc cref="IEquatable{T}" />
    public bool Equals(ImmutableValueFormat? other) => this.ValueFormatEqual(other);

    /// <inheritdoc cref="IEquatable{T}" />
    public bool Equals(IValueFormat other) => this.ValueFormatEqual(other);

    /// <inheritdoc />
    public void CopyTo(IValueFormat other)
    {
      if (other is ValueFormatMutable mutable)
        mutable.CopyFrom(this);
      else
        throw new NotSupportedException("Can not copy properties to not mutable instance");
    }

    /// <inheritdoc />
    public object Clone() => new ImmutableValueFormat(this);
  }
}