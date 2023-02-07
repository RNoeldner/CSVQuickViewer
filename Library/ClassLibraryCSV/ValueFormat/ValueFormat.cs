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
using System.Globalization;
using System.Text;
// ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

namespace CsvTools
{
  /// <summary>
  ///   Setting for a value format
  /// </summary>
  public sealed class ValueFormat : IEquatable<ValueFormat>
  {
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
    public const bool cOverwriteDefault = true;

    public static readonly ValueFormat Empty = new ValueFormat();

    [JsonConstructor]
    public ValueFormat(
      in DataTypeEnum? dataType = DataTypeEnum.String,
      in string? dateFormat = cDateFormatDefault,
      in string? dateSeparator = cDateSeparatorDefault,
      in string? timeSeparator = cTimeSeparatorDefault,
      in string? numberFormat = cNumberFormatDefault,
      in string? groupSeparator = cGroupSeparatorDefault,
      in string? decimalSeparator = cDecimalSeparatorDefault,
      in string? asTrue = cTrueDefault,
      in string? asFalse = cFalseDefault,
      in string? displayNullAs = "",
      int? part = cPartDefault,
      in string? partSplitter = cPartSplitterDefault,
      bool? partToEnd = cPartToEndDefault,
      string? regexSearchPattern = "",
      string? regexReplacement = "",
      string? readFolder = "",
      string? writeFolder = "",
      string? fileOutPutPlaceholder = "",
      bool? overwrite = cOverwriteDefault)
    {
      // Set defaults
      DataType = dataType ??  DataTypeEnum.String;

      DateFormat = cDateFormatDefault;
      DateSeparator = cDateSeparatorDefault;
      TimeSeparator = cTimeSeparatorDefault;
      NumberFormat = cNumberFormatDefault;
      DecimalSeparator = cDecimalSeparatorDefault;
      GroupSeparator = cGroupSeparatorDefault;

      // Boolean
      False = asFalse ?? cFalseDefault;
      True = asTrue ?? cTrueDefault;

      Part = cPartDefault;
      PartSplitter = cPartSplitterDefault;
      PartToEnd = cPartToEndDefault;
      // Text
      DisplayNullAs = displayNullAs ?? string.Empty;

      // Regex
      RegexSearchPattern = regexSearchPattern ?? string.Empty;
      RegexReplacement = regexReplacement ?? string.Empty;

      // Binary writer
      ReadFolder = readFolder ?? string.Empty;
      WriteFolder = writeFolder ?? string.Empty;
      FileOutPutPlaceholder = fileOutPutPlaceholder ?? string.Empty;
      Overwrite = cOverwriteDefault;

      // Depending on type set the other corresponding values
      // Json Serializer does ignore defaults in ctor
      if (DataTypeEnum.DateTime.Equals(dataType))
      {
        // Dates
        DateFormat = dateFormat ?? cDateFormatDefault;
        DateSeparator = new Punctuation(dateSeparator ?? cDateSeparatorDefault);
        TimeSeparator = new Punctuation(timeSeparator ?? cTimeSeparatorDefault);
      }
      else if (DataTypeEnum.Integer.Equals(dataType))
      {
        NumberFormat = numberFormat ?? cNumberFormatDefault;
        GroupSeparator = groupSeparator ?? cGroupSeparatorDefault;
      }
      else if (DataTypeEnum.Numeric.Equals(dataType) || DataTypeEnum.Double.Equals(dataType))
      {
        // Numbers
        NumberFormat = numberFormat ?? cNumberFormatDefault;
        DecimalSeparator = new Punctuation(decimalSeparator ?? cDecimalSeparatorDefault);
        GroupSeparator = new Punctuation(groupSeparator ?? cGroupSeparatorDefault);
      }
      else if (DataTypeEnum.TextPart.Equals(dataType))
      {
        // TextPart
        Part = part ?? cPartDefault;
        PartSplitter =  new Punctuation(partSplitter ?? cPartSplitterDefault);
        PartToEnd = partToEnd ?? cPartToEndDefault;
      }
      else if (DataTypeEnum.Binary.Equals(dataType))
      {
        // Binary writer
        Overwrite = overwrite ?? cOverwriteDefault;
      }
    }

    /// <summary>
    ///   Gets or sets the type of the data.
    /// </summary>
    /// <value>The type of the data.</value>
    [DefaultValue(DataTypeEnum.String)]
    public DataTypeEnum DataType { get; }

    /// <summary>
    ///   Gets or sets the date format. 
    /// </summary>
    /// <value>The date format.</value>
    [DefaultValue(cDateFormatDefault)]
    public string DateFormat { get; }

    /// <summary>
    ///   The value will return the resulted Separator, passing in "Colon" will return ":"
    /// </summary>
    [DefaultValue(cDateSeparatorDefault)]
    public string DateSeparator { get; }

    /// <summary>
    ///   Gets or sets the time separator.
    /// </summary>
    /// <value>The time separator.</value>
    [DefaultValue(cTimeSeparatorDefault)]
    public string TimeSeparator { get; }

    /// <summary>
    ///   Gets or sets the number format.
    /// </summary>
    /// <value>The number format.</value>
    [DefaultValue(cNumberFormatDefault)]
    public string NumberFormat { get; }

    /// <summary>
    ///   The value will return the resulted Separator, passing in "Dot" will return "."
    /// </summary>
    [DefaultValue(cDecimalSeparatorDefault)]
    public string DecimalSeparator { get; }

    /// <summary>
    ///   The value will return the resulted Separator, passing in "Dot" will return "."
    /// </summary>
    [DefaultValue(cGroupSeparatorDefault)]
    public string GroupSeparator { get; }

    /// <summary>
    ///   Writing data you can specify how a NULL value should be written, commonly its empty, in
    ///   some circumstances you might want to have n/a etc.
    /// </summary>
    /// <value>Text used if the value is NULL</value>
    [DefaultValue("")]
    public string DisplayNullAs { get; }

    /// <summary>
    ///   Gets or sets the representation for true.
    /// </summary>
    /// <value>The true.</value>
    [DefaultValue(cTrueDefault)]
    public string True { get; }

    /// <summary>
    ///   Gets or sets the representation for false.
    /// </summary>
    [DefaultValue(cFalseDefault)]
    public string False { get; }

    /// <summary>
    ///   Gets or sets the part for splitting.
    /// </summary>
    /// <value>The part starting with 1</value>
    [DefaultValue(cPartDefault)]
    public int Part { get; }

    /// <summary>
    ///   Gets or sets the splitter. 
    /// </summary>
    /// <value>The splitter.</value>
    [DefaultValue(cPartSplitterDefault)]
    public string PartSplitter { get; }

    /// <summary>
    ///   Determine if a part should end with the next splitter
    /// </summary>
    /// <value><c>true</c> if all of the remaining text should be returned in the part</value>
    [DefaultValue(cPartToEndDefault)]
    public bool PartToEnd { get; }

    /// <summary>
    ///   Replace for Regex Replace
    /// </summary>
    [DefaultValue("")]
    public string RegexReplacement { get; }

    /// <summary>
    ///   Search Pattern for Regex Replace
    /// </summary>  
    [DefaultValue("")]
    public string RegexSearchPattern { get; }

    /// <summary>
    /// PlaceHolder for the file name placeholders are replaced with current records fields if empty the source name is used
    /// </summary>
    [DefaultValue("")]
    public string FileOutPutPlaceholder { get; }

    /// <summary>
    /// Folder where the source file should be read from, used for binary reader
    /// </summary>
    [DefaultValue("")]
    public string ReadFolder { get; }

    /// <summary>
    /// Folder where the source file should be written to, used for binary reader
    /// </summary>
    [DefaultValue("")]
    public string WriteFolder { get; }

    /// <summary>
    /// Set to <c>true</c> if binary output file should overwrite any existing file
    /// </summary>
    [DefaultValue(cOverwriteDefault)]
    public bool Overwrite { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is ValueFormat other && Equals(other);

    /// <inheritdoc cref="IEquatable{T}" />
    public bool Equals(ValueFormat? other)
    {
      if (other is null) return false;
      if (ReferenceEquals(this, other)) return true;
      return DataType == other.DataType
             && DateFormat == other.DateFormat
             && DateSeparator == other.DateSeparator
             && DecimalSeparator == other.DecimalSeparator
             && DisplayNullAs == other.DisplayNullAs
             && False == other.False
             && GroupSeparator == other.GroupSeparator
             && NumberFormat == other.NumberFormat
             && Part == other.Part
             && PartSplitter == other.PartSplitter
             && PartToEnd == other.PartToEnd
             && TimeSeparator == other.TimeSeparator
             && True == other.True
             && RegexSearchPattern == other.RegexSearchPattern
             && RegexReplacement == other.RegexReplacement
             && ReadFolder == other.ReadFolder
             && WriteFolder == other.WriteFolder
             && FileOutPutPlaceholder == other.FileOutPutPlaceholder
             && Overwrite == other.Overwrite;
    }

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

    /// <summary>
    ///   Determines whether the ValueFormats does matching and expected format
    /// </summary>
    /// <param name="expectedFormat">The column format to compare to</param>
    /// <returns>
    ///   <c>true</c> if the current format would be acceptable for the expected data type.
    /// </returns>
    /// <remarks>
    ///   Is matching only looks at data type and some formats, it is assumed that we do not
    ///   distinguish between numeric formats, it is O.K. to expect a money value but have a integer
    /// </remarks>
    public bool IsMatching(in ValueFormat expectedFormat)
    {
      if (expectedFormat.DataType == DataType)
        return true;

      // if one is integer but we expect numeric or vice versa, assume its OK, one of the sides does
      // not have a decimal separator
      if ((expectedFormat.DataType == DataTypeEnum.Numeric || expectedFormat.DataType == DataTypeEnum.Double || expectedFormat.DataType == DataTypeEnum.Integer)
          && DataType == DataTypeEnum.Integer)
        return true;

      switch (expectedFormat.DataType)
      {
        case DataTypeEnum.Integer when DataType == DataTypeEnum.Numeric || DataType == DataTypeEnum.Double
                                                                            || DataType == DataTypeEnum.Integer:
          return true;
        // if we have dates, check the formats
        case DataTypeEnum.DateTime when DataType == DataTypeEnum.DateTime:
          return expectedFormat.DateFormat.Equals(DateFormat, StringComparison.Ordinal)
                 && (DateFormat.IndexOf('/') == -1 || expectedFormat.DateSeparator.Equals(
                   DateSeparator,
                   StringComparison.Ordinal)) && (DateFormat.IndexOf(':') == -1
                                                  || expectedFormat.TimeSeparator.Equals(
                                                    TimeSeparator,
                                                    StringComparison.Ordinal));
      }

      // if we have decimals, check the formats
      if ((expectedFormat.DataType == DataTypeEnum.Numeric || expectedFormat.DataType == DataTypeEnum.Double)
          && (DataType == DataTypeEnum.Numeric || DataType == DataTypeEnum.Double))
        return expectedFormat.NumberFormat.Equals(NumberFormat, StringComparison.Ordinal)
               && expectedFormat.DecimalSeparator.Equals(DecimalSeparator)
               && expectedFormat.GroupSeparator.Equals(GroupSeparator);
      // For everything else assume its wrong
      return false;
    }

    /// <summary>
    ///   Gets the a description of the Date or Number format
    /// </summary>
    /// <returns></returns>
    public string GetFormatDescription() =>
      DataType switch
      {
        DataTypeEnum.DateTime => DateFormat.ReplaceDefaults(
          CultureInfo.InvariantCulture.DateTimeFormat.DateSeparator, DateSeparator,
          CultureInfo.InvariantCulture.DateTimeFormat.TimeSeparator, TimeSeparator),
        DataTypeEnum.Numeric => NumberFormat.ReplaceDefaults(
          CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, DecimalSeparator,
          CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator, GroupSeparator),
        DataTypeEnum.Double => NumberFormat.ReplaceDefaults(
          CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator, DecimalSeparator,
          CultureInfo.InvariantCulture.NumberFormat.NumberGroupSeparator, GroupSeparator),
        DataTypeEnum.TextPart => $"{Part}" + (PartToEnd ? " To End" : string.Empty),
        DataTypeEnum.Binary => $"Read file from {ReadFolder}",
        DataTypeEnum.TextReplace =>
          $"Replace {StringUtils.GetShortDisplay(RegexSearchPattern, 10)} with {StringUtils.GetShortDisplay(RegexReplacement, 10)}",
        _ => string.Empty
      };

    /// <summary>
    ///   Gets the description.
    /// </summary>
    /// <returns></returns>
    public string GetTypeAndFormatDescription()
    {
      var sbText = new StringBuilder(DataType.Description());

      var shortDesc = GetFormatDescription();
      if (shortDesc.Length <= 0)
        return sbText.ToString();
      sbText.Append(" (");
      sbText.Append(shortDesc);
      sbText.Append(")");

      return sbText.ToString();
    }
  }
}