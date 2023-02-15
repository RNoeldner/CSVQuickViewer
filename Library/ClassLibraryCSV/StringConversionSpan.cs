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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
// ReSharper disable MemberCanBePrivate.Global


namespace CsvTools
{
  public static class StringConversionSpan
  {
    /// <summary>
    ///   Combine date / time, the individual values could already be typed.
    /// </summary>
    /// <param name="dateColumn">The date column typed value either datetime or double</param>
    /// <param name="dateColumnText">The date column text representation.</param>
    /// <param name="timeColumn">The time column typed value either datetime, TimeStamp or double</param>
    /// <param name="timeColumnText">The time column text representation.</param>
    /// <param name="serialDateTime">if set to <c>true</c> allow serial dates time values (doubles).</param>
    /// <param name="valueFormat">The value format for the date column.</param>
    /// <param name="timeColumnIssues">
    ///   if set to <c>true</c> if the time value is outside range 00:00 - 23:59.
    /// </param>
    /// <returns>A combined date from a date and a time column</returns>
    /// <remarks>This does not have time zone adjustments yet</remarks>
    public static DateTime? CombineObjectsToDateTime(
      in object? dateColumn,
      ReadOnlySpan<char> dateColumnText,
      in object? timeColumn,
      ReadOnlySpan<char> timeColumnText,
      bool serialDateTime,
      in ValueFormat valueFormat,
      out bool timeColumnIssues)
    {
      var dateValue = DateTimeConstants.FirstDateTime;
      // We do have an associated column, with a proper date format
      if (dateColumn != null)
      {
        if (dateColumn is DateTime time)
          dateValue = time;
        else if (serialDateTime && dateColumn is double oaDate && oaDate > -657435.0 && oaDate < 2958466.0)
          dateValue = DateTimeConstants.FirstDateTime.AddDays(oaDate);
      }


      // if we did not convert yet and we have a text use it
      // ReSharper disable once ReplaceWithStringIsNullOrEmpty
      if (dateValue == DateTimeConstants.FirstDateTime && dateColumnText.Length > 0)
      {
        var val = CombineStringsToDateTime(
          dateColumnText,
          valueFormat.DateFormat.AsSpan(),
          ReadOnlySpan<char>.Empty,
          valueFormat.DateSeparator,
          valueFormat.TimeSeparator,
          serialDateTime);
        if (val.HasValue)
          dateValue = val.Value;
      }

      TimeSpan? timeSpanValue = null;
      if (timeColumn != null)
        switch (timeColumn)
        {
          case double oaDate when serialDateTime:
          {
            // ReSharper disable once MergeIntoPattern
            if (oaDate > -657435.0 && oaDate < 2958466.0)
            {
              var timeValue = DateTimeConstants.FirstDateTime.AddDays(oaDate);
              timeSpanValue = new TimeSpan(
                0,
                timeValue.Hour,
                timeValue.Minute,
                timeValue.Second,
                timeValue.Millisecond);
            }

            break;
          }
          case DateTime timeValue:
            timeSpanValue = new TimeSpan(0, timeValue.Hour, timeValue.Minute, timeValue.Second, timeValue.Millisecond);
            break;

          case TimeSpan span:
            timeSpanValue = span;
            break;
        }

      timeSpanValue ??= StringToTimeSpan(timeColumnText, ':', serialDateTime);

      if (timeSpanValue.HasValue)
      {
        timeColumnIssues = timeSpanValue.Value.TotalDays >= 1d || timeSpanValue.Value.TotalSeconds < 0d;
        return dateValue.Add(timeSpanValue.Value);
      }

      timeColumnIssues = false;

      // It could be that the dateValue is indeed m_FirstDateTime, but only if the text matches the
      // proper formatted value
      if (dateValue == DateTimeConstants.FirstDateTime && dateColumn is null
                                                       && (dateColumnText.IsEmpty || dateColumnText.Length == 0
                                                         || !dateColumnText.Equals(
                                                           DateTimeConstants.FirstDateTime.DateTimeToString(valueFormat)
                                                             .AsSpan(), StringComparison.Ordinal)))
        return null;

      return dateValue;
    }
    
    /// <summary>
    ///   Combines two strings to one date time.
    /// </summary>
    /// <param name="dateText">The date part.</param>
    /// <param name="dateFormat">The date format.</param>
    /// <param name="timeText">The time part.</param>
    /// <param name="dateSeparatorChar">The date separator.</param>
    /// <param name="timeSeparatorChar">The time separator.</param>
    /// <param name="serialDateTime">Allow Date Time values ion serial format</param>
    /// <returns></returns>
    public static DateTime? CombineStringsToDateTime(
      this ReadOnlySpan<char> dateText,
      ReadOnlySpan<char> dateFormat,
      ReadOnlySpan<char> timeText,
      char dateSeparatorChar,
      char timeSeparatorChar,
      bool serialDateTime)
    {
      if (dateFormat.IsEmpty)
        return null;

      var date = StringToDateTime(dateText, dateFormat, dateSeparatorChar, timeSeparatorChar, serialDateTime);

      switch (date)
      {
        // we have no date check if we have a time
        case null:
        {
          var timeP = StringToTimeSpan(timeText, timeSeparatorChar, serialDateTime);
          // no date and no time, nothing to do
          if (timeP is null)
            return null;
          return DateTimeConstants.FirstDateTime.Add(timeP.Value);
        }
        // In case a value is read that just is a time, need to adjust c# and Excel behavior the
        // application assumes all dates on cFirstDatetime is a time only
        case { Year: 1, Month: 1 } when timeText.IsWhiteSpace():
          return date.Value.Ticks.GetTimeFromTicks();
      }

      // get the time to add to the date
      var time = StringToTimeSpan(timeText, timeSeparatorChar, serialDateTime);
      return time is null ? date : date.Value.Add(time.Value);
    }

    /// <summary>
    ///   Tries to determine the date time assuming its an Excel serial date time, using regional
    ///   and common decimal separators
    /// </summary>
    /// <param name="text">The Value as string</param>
    /// <returns></returns>
    public static DateTime? SerialStringToDateTime(
      this ReadOnlySpan<char> text)
    {
      var stringDateValue = text.Trim();
      try
      {
        var numberFormatProvider =
          new NumberFormatInfo { NegativeSign = "-", PositiveSign = "+", NumberGroupSeparator = string.Empty };
        foreach (var decimalSeparator in StaticCollections.DecimalSeparatorChars)
        {
          numberFormatProvider.NumberDecimalSeparator = decimalSeparator.ToString(CultureInfo.CurrentCulture);
          if (!double.TryParse(
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
              stringDateValue
#else
                stringDateValue.ToString()
#endif
                , NumberStyles.Float, numberFormatProvider, out var timeSerial))
            continue;
          if (timeSerial >= -657435 && timeSerial < 2958466)
            return DateTime.FromOADate(timeSerial);
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine("{0} is not a serial date. Error: {1}", text.ToString(), ex.Message);
      }

      return null;
    }

    /// <summary>
    ///   Parses a string to a boolean.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="trueValue">An additional value that would evaluate to true</param>
    /// <param name="falseValue">An additional value that would evaluate to false</param>
    /// <returns>
    ///   <c>Null</c> if the value is empty, other wise <c>true</c> if identified as boolean or
    ///   <c>false</c> otherwise
    /// </returns>
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public static bool? StringToBoolean(
      this ReadOnlySpan<char> value, 
      ReadOnlySpan<char> trueValue,
      ReadOnlySpan<char> falseValue)
    {
      return StringToBooleanWithMatch(value,trueValue,falseValue).Item1;
    }
    
    /// <summary>
    ///   Check is a string is a boolean.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="trueValue">An additional value that would evaluate to true</param>
    /// <param name="falseValue">An additional value that would evaluate to false</param>
    /// <returns>
    ///   <c>Null</c> if the value can not be identified as boolean, other wise a tuple with
    ///   <c>true</c> or <c>false</c> and the value that had been used
    /// </returns>
    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public static (bool?, string value) StringToBooleanWithMatch(
      this ReadOnlySpan<char> value, 
      ReadOnlySpan<char> trueValue,
      ReadOnlySpan<char> falseValue)
    {
      if (value.Length == 0)
        return (null, string.Empty);

      foreach ((int start, int length) valueTuple in trueValue.GetSlices(StaticCollections.ListDelimiterChars))
        if (value.Equals(trueValue.Slice(valueTuple.start, valueTuple.length), StringComparison.OrdinalIgnoreCase))
          return (true, trueValue.Slice(valueTuple.start, valueTuple.length).ToString());
      foreach (var text in StaticCollections.TrueValues)
        if (value.Equals(text.AsSpan(), StringComparison.OrdinalIgnoreCase))
          return (true, text);

      foreach ((int start, int length) valueTuple in falseValue.GetSlices(StaticCollections.ListDelimiterChars))
        if (value.Equals(falseValue.Slice(valueTuple.start, valueTuple.length), StringComparison.OrdinalIgnoreCase))
          return (false, falseValue.Slice(valueTuple.start, valueTuple.length).ToString());
      foreach (var text in StaticCollections.FalseValues)
        if (value.Equals(text.AsSpan(), StringComparison.OrdinalIgnoreCase))
          return (false, text);

      return (null, string.Empty);
    }
    
    /// <summary>
    ///   Parses a string to a date time value
    /// </summary>
    /// <param name="text">The original value.</param>
    /// <param name="dateFormats">The date formats, separated by delimiter</param>
    /// <param name="dateSeparatorChar">The date separator used in the conversion</param>
    /// <param name="timeSeparatorChar">The time separator.</param>
    /// <param name="serialDateTime">Allow Date Time values ion serial format</param>
    /// <returns>
    ///   An <see cref="DateTime" /> if the value could be interpreted, <c>null</c> otherwise
    /// </returns>
    /// <remarks>If the date part is not filled its the 1/1/1</remarks>
    public static DateTime? StringToDateTime(
      this ReadOnlySpan<char> text,
      ReadOnlySpan<char> dateFormats,
      char dateSeparatorChar,
      char timeSeparatorChar,
      bool serialDateTime)
    {
      var stringDateValue = text.Trim();

      var result = StringToDateTimeExact(text, dateFormats, dateSeparatorChar, timeSeparatorChar,
        CultureInfo.CurrentCulture);
      if (result.HasValue)
        return result.Value;

      if (serialDateTime
          && (stringDateValue.IndexOf(dateSeparatorChar) == -1)
          && (stringDateValue.IndexOf(timeSeparatorChar) == -1))
        return SerialStringToDateTime(stringDateValue);

      // in case its time only and we do not have any date separator try a timespan
      if (stringDateValue.IndexOf(dateSeparatorChar) != -1 || dateFormats.IndexOf('/') != -1) return null;
      var ts = StringToTimeSpan(stringDateValue, timeSeparatorChar, false);
      if (ts.HasValue)
        return new DateTime(ts.Value.Ticks);

      return null;
    }

    /// <summary>
    ///   Converts Strings to date time using the culture information
    /// </summary>
    /// <param name="text">The original value.</param>
    /// <param name="dateFormats">The date formats.</param>
    /// <param name="dateSeparatorChar">The date separator.</param>
    /// <param name="timeSeparatorChar">The time separator.</param>
    /// <param name="culture">The culture.</param>
    /// <returns></returns>
    /// <remarks>
    ///   Similar to <see cref="StringToDateTimeByCulture" /> but checks if we have a format that
    ///   would fit the length of the value.
    /// </remarks>
    public static DateTime? StringToDateTimeExact(
      this ReadOnlySpan<char> text,
      ReadOnlySpan<char> dateFormats,
      char dateSeparatorChar,
      char timeSeparatorChar,
      in CultureInfo culture)
    {
      var stringDateValue = text.Trim().ToString().Replace('\t', ' ').Replace("  ", " ").AsSpan();

      if (stringDateValue.Length < 4)
        return null;

      // Quick check: If the entry is empty, or a constant string, or the length does not make
      // sense, we do not need to try and parse
      if (stringDateValue.Length < 4 || stringDateValue.SequenceEqual("00000000".AsSpan()) ||
          stringDateValue.SequenceEqual("99999999".AsSpan()))
        return null;

      // get rid of numeric suffixes like 12th or 3rd for dates
      if (stringDateValue.IndexOf("th ".AsSpan(), StringComparison.OrdinalIgnoreCase) != -1
          || stringDateValue.IndexOf("nd ".AsSpan(), StringComparison.OrdinalIgnoreCase) != -1
          || stringDateValue.IndexOf("st ".AsSpan(), StringComparison.OrdinalIgnoreCase) != -1
          || stringDateValue.IndexOf("rd ".AsSpan(), StringComparison.OrdinalIgnoreCase) != -1)
        stringDateValue = StaticCollections.RegExNumberSuffixEnglish.Value.Replace(stringDateValue.ToString(), "$1")
          .AsSpan();
      var matchingDateTimeFormats = new List<string>();
      foreach (var (start, length) in dateFormats.GetSlices(StaticCollections.ListDelimiterChars.AsSpan()))
      {
        matchingDateTimeFormats.Add(dateFormats.Slice(start, length).ToString());
        var dateTimeFormatString = (dateSeparatorChar ==char.MinValue && dateFormats.Slice(start, length).IndexOf('/') != -1)
            ? dateFormats.Slice(start, length).ToString().Replace("/", "")
            : dateFormats.Slice(start, length).ToString();

        if (StaticCollections.StandardDateTimeFormats.DateLengthMatches(stringDateValue.Length, dateTimeFormatString))
          matchingDateTimeFormats.Add(dateTimeFormatString);
        // In case of a date & time format add the date only format separately
        var indexHour =
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
          dateTimeFormatString.IndexOf('h', StringComparison.OrdinalIgnoreCase);
#else
          dateTimeFormatString.IndexOfAny(new[] { 'h', 'H' });
#endif
        // assuming there is a text before the hour that has a reasonable size take it as date
        if (indexHour > 4)
        {
          string dateOnlyFmt = dateTimeFormatString.Substring(0, indexHour - 1).Trim();
          if (StaticCollections.StandardDateTimeFormats.DateLengthMatches(stringDateValue.Length, dateOnlyFmt))
            matchingDateTimeFormats.Add(dateOnlyFmt);
        }
      }

      if (matchingDateTimeFormats.Count == 0)
        return null;

      return StringToDateTimeByCulture(
        stringDateValue,
        matchingDateTimeFormats.ToArray(),
        dateSeparatorChar,
        timeSeparatorChar,
        culture);
    }

    /// <summary>
    ///   Parses a string to a decimal
    /// </summary>
    /// <param name="text">The value.</param>
    /// <param name="decimalSeparatorChar">The decimal separator. Do not pass in written punctuation</param>
    /// <param name="groupSeparatorChar">The thousand separator. Do not pass in written punctuation</param>
    /// <param name="allowPercentage">If set to true, a % or ‰ will be recognized</param>
    /// <param name="currencyRemoval">A list of currency symbols to remove before parsing</param>
    /// <returns>An decimal if the value could be interpreted, <c>null</c> otherwise</returns>
    public static decimal? StringToDecimal(
      this ReadOnlySpan<char> text,
      char decimalSeparatorChar,
      char groupSeparatorChar,
      bool allowPercentage,
      bool currencyRemoval)
    {
      // Remove any white space
      var fieldValueSpan = text.Trim();
      // in case nothing is passed in we are already done here
      if (fieldValueSpan.Length == 0)
        return null;

      if (currencyRemoval)
      {
        var temp = fieldValueSpan.ToString();
        foreach (var currencySymbol in StaticCollections.CurrencySymbols)
        {
          if (temp.IndexOf(currencySymbol) != -1)
            temp= temp.Replace(currencySymbol.ToString(), string.Empty);
        }

        fieldValueSpan = temp.AsSpan().Trim();
      }

      var startDecimal = fieldValueSpan.Length;
      var lastPos = -3;
      // Sanity Check: In case the decimalSeparator occurs multiple times is not a number in case
      // the thousand separator are closer then 3 characters together
      for (var pos = 0; pos < fieldValueSpan.Length; pos++)
      {
        if (fieldValueSpan[pos] == decimalSeparatorChar)
        {
          if (startDecimal < fieldValueSpan.Length)
            // Second decimal seperator
            return null;
          startDecimal = pos;
        }
        else if (fieldValueSpan[pos] == groupSeparatorChar)
        {
          if (lastPos > 0 && pos != lastPos + 4)
            // Distance between group is not correct
            return null;
          lastPos = pos;
        }
      }

      // if we have a decimal point the group seperator has to be 3 places from the right e.g. 63.467.8373 is not ok, but 634.678.373 is
      if (lastPos > 0 && startDecimal != lastPos + 4)
        return null;

      var numberFormatProvider = new NumberFormatInfo
      {
        NegativeSign = "-",
        PositiveSign = "+",
        NumberDecimalSeparator = decimalSeparatorChar.ToStringHandle0(),
        NumberGroupSeparator = groupSeparatorChar.ToStringHandle0()
      };
      var roundBraces = fieldValueSpan[0] == '(' && fieldValueSpan[fieldValueSpan.Length - 1] == ')';
      if (roundBraces)
        fieldValueSpan = fieldValueSpan.Slice(1, fieldValueSpan.Length - 2).Trim();

      var percentage = allowPercentage && fieldValueSpan[fieldValueSpan.Length - 1] == '%';
      // ReSharper disable once IdentifierTypo
      var permille = allowPercentage && fieldValueSpan[fieldValueSpan.Length - 1] == '‰';

      if (percentage || permille)
        fieldValueSpan = fieldValueSpan.Slice(0, fieldValueSpan.Length - 1).Trim();

      //       Pattern 1:           -1,234.00
      //       Pattern 2:           - 1,234.00
      //       Pattern 3:           1,234.00-
      for (numberFormatProvider.NumberNegativePattern = 1;
           numberFormatProvider.NumberNegativePattern <= 3;
           numberFormatProvider.NumberNegativePattern++)
      {
        // Try to convert this value to a decimal value. 
        if (!decimal.TryParse(
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
              fieldValueSpan
#else
              fieldValueSpan.ToString()
#endif
              , NumberStyles.Number, numberFormatProvider, out var result))
          continue;

        // If this works, exit
        if (percentage)
          result /= 100m;
        else if (permille)
          result /= 1000m;
        return (roundBraces) ? -result : result;
      }

      return null;
    }

    /// <summary>
    ///   Parses a string to a guid
    /// </summary>
    /// <param name="text">The original value.</param>
    /// <returns>An <see cref="Guid" /> if the value could be interpreted, <c>null</c> otherwise</returns>
    public static Guid? StringToGuid(
      this ReadOnlySpan<char> text)
    {
      if (Guid.TryParse(
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
              text
#else
            text.ToString()
#endif
            , out var result))
        return result;
      else
        return null;
    }

    /// <summary>
    ///   Parses a strings to an int.
    /// </summary>
    /// <param name="text">The value.</param>
    /// <param name="decimalSeparatorChar">The decimal separator.</param>
    /// <param name="thousandSeparatorChar">The thousand separator.</param>
    /// <returns>An int if the value could be interpreted, <c>null</c> otherwise</returns>
    public static short? StringToInt16(
      this ReadOnlySpan<char> text, 
      char decimalSeparatorChar, 
      char thousandSeparatorChar)
    {
      if (text.IsEmpty)
        return null;
      try
      {
        var dec = StringToDecimal(text, decimalSeparatorChar, thousandSeparatorChar, false, false);
        if (dec.HasValue)
          return Convert.ToInt16(dec.Value, CultureInfo.InvariantCulture);
      }
      catch (OverflowException)
      {
        // The numerical value could not be converted to an integer
      }

      return null;
    }

    /// <summary>
    ///   Parses a strings to an int.
    /// </summary>
    /// <param name="text">The value.</param>
    /// <param name="decimalSeparatorChar">The decimal separator.</param>
    /// <param name="thousandSeparatorChar">The thousand separator.</param>
    /// <returns>An int if the value could be interpreted, <c>null</c> otherwise</returns>
    public static int? StringToInt32(
      this ReadOnlySpan<char> text, 
      char decimalSeparatorChar, 
      char thousandSeparatorChar)
    {
      if (text.IsEmpty)
        return null;
      try
      {
        var dec = StringToDecimal(text, decimalSeparatorChar, thousandSeparatorChar, false, false);
        if (dec.HasValue)
          return dec.Value.ToInt();
      }
      catch (OverflowException)
      {
        // The numerical value could not be converted to an integer
      }

      return null;
    }

    /// <summary>
    ///   Parses a strings to an int.
    /// </summary>
    /// <param name="text">The value.</param>
    /// <param name="decimalSeparatorChar">The decimal separator.</param>
    /// <param name="thousandSeparatorChar">The thousand separator.</param>
    /// <returns>An int if the value could be interpreted, <c>null</c> otherwise</returns>
    public static long? StringToInt64(
      this ReadOnlySpan<char> text, 
      char decimalSeparatorChar, 
      char thousandSeparatorChar)
    {
      if (text.IsEmpty)
        return null;
      try
      {
        var dec = StringToDecimal(text, decimalSeparatorChar, thousandSeparatorChar, false, false);
        if (dec.HasValue)
          return dec.Value.ToInt64();
      }
      catch (OverflowException)
      {
        // The numerical value could not be converted to an integer
      }

      return null;
    }

    /// <summary>
    ///   Splits a string and returns the required part
    /// </summary>
    /// <param name="text">The string value that should be split.</param>
    /// <param name="splitter">The splitter character.</param>
    /// <param name="part">The part that should be returned, starting with 1.</param>
    /// <param name="toEnd">Read the part up to the end</param>
    /// <returns>
    ///   <c>Null</c> if the value is empty or the part can not be found. If the desired part is 1
    ///   and the splitter is not contained the whole value is returned.
    /// </returns>    
    public static ReadOnlySpan<char> StringToTextPart(
      this ReadOnlySpan<char> text, 
      char splitter, 
      int part, 
      bool toEnd)
    {
      if (text.Length == 0 || part < 1)
        return ReadOnlySpan<char>.Empty;
      var list = text.GetSlices(new[] { splitter });

      if (part == 1 && toEnd)
        return text;

      if (part > list.Count)
        return ReadOnlySpan<char>.Empty;
      
      if (!toEnd)
        return text.Slice(list[part - 1].start, list[part - 1].length);
      return text.Slice(list[part - 1].start);
    }

    /// <summary>
    ///   Strings to time span.
    /// </summary>
    /// <param name="text">The original value.</param>
    /// <param name="timeSeparatorChar">The time separator.</param>
    /// <param name="serialDateTime">Allow Date Time values in serial format</param>
    /// <returns></returns>
    public static TimeSpan? StringToTimeSpan(
      this ReadOnlySpan<char> text, 
      char timeSeparatorChar, 
      bool serialDateTime)
    {
      var stringTimeValue = text.Trim();
      if (stringTimeValue.IsEmpty)
        return null;

      var slices = stringTimeValue.GetSlices(new[] { timeSeparatorChar, ' ', '.' }).Where(x => x.length>0).ToList();
      // Either we only have one slice or its two slices but the two slices are separated by .
      if (slices.Count==1 || 
          slices.Count==2 && stringTimeValue[slices[1].start-1] =='.' )
      {
        if (!serialDateTime)
          return null;
        var dt = SerialStringToDateTime(stringTimeValue);
        if (dt.HasValue)
          return new TimeSpan(dt.Value.Ticks - DateTimeConstants.FirstDateTime.Ticks);

        return null;
      }
      
      var slice = stringTimeValue.Slice(slices[0].start, slices[0].length);
      int.TryParse(
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        slice
#else
            slice.ToString()
#endif
        , out var hours);
      var minutes = 0;
      if (slices.Count > 1)
      {
        slice = stringTimeValue.Slice(slices[1].start, slices[1].length);
        int.TryParse(
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
          slice
#else
            slice.ToString()
#endif
          , out minutes);
      }
      var seconds = 0;
      if (slices.Count > 2)
      {
        slice = stringTimeValue.Slice(slices[2].start, slices[2].length);
        int.TryParse(
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
          slice
#else
            slice.ToString()
#endif
          , out seconds);
      }

      var milliseconds = 0;
      if (slices.Count > 3)
      {
        slice = stringTimeValue.Slice(slices[3].start, slices[3].length);
        int.TryParse(
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
          slice
#else
            slice.ToString()
#endif
          , out milliseconds);
      }
      // Handle am / pm to adjust hours
      // 12:00 AM - 12:59 AM --> 00:00 - 00:59
      if (hours == 12 && slices.Count >2 && stringTimeValue.Slice(slices[slices.Count-1].start, slices[slices.Count-1].length).Equals("am".AsSpan(), StringComparison.OrdinalIgnoreCase))
        hours -= 12;

      // 12:00 PM - 12:59 PM 12:00 - 12:59 No change
      // 01:00 pm - 11:59 PM --> 13:00 - 23:59
      if (hours < 12 && slices.Count >2 && stringTimeValue.Slice(slices[slices.Count-1].start, slices[slices.Count-1].length).Equals("pm".AsSpan(), StringComparison.OrdinalIgnoreCase))
        hours += 12;

      return new TimeSpan(0, hours, minutes, seconds, milliseconds);
    }

    private static int IndexOf(
      this ReadOnlySpan<char> text, 
      char charToFind, 
      int start = 0)
    {
      for (int i = start; i < text.Length; i++)
        if (text[i] == charToFind)
          return i;
      return -1;
    }
    /// <summary>
    ///   Converts Strings to date time using the culture information.
    /// </summary>
    /// <param name="stringDateValue">The string date value make sure the text is trimmed</param>
    /// <param name="dateTimeFormats">The date time formats.</param>
    /// <param name="dateSeparatorChar">The date separator.</param>
    /// <param name="timeSeparatorChar">The time separator.</param>
    /// <param name="culture">The culture.</param>
    /// <returns></returns>
    private static DateTime? StringToDateTimeByCulture(
      ReadOnlySpan<char> stringDateValue,
      in string[] dateTimeFormats,
      char dateSeparatorChar,
      char timeSeparatorChar,
      in CultureInfo culture)
    {
      while (true)
      {
        var dateTimeFormatInfo = new DateTimeFormatInfo();

        dateTimeFormatInfo.SetAllDateTimePatterns(dateTimeFormats, 'd');
        dateTimeFormatInfo.DateSeparator = dateSeparatorChar.ToStringHandle0();
        dateTimeFormatInfo.TimeSeparator = timeSeparatorChar.ToStringHandle0();

        dateTimeFormatInfo.AbbreviatedDayNames = culture.DateTimeFormat.AbbreviatedDayNames;
        dateTimeFormatInfo.DayNames = culture.DateTimeFormat.DayNames;
        dateTimeFormatInfo.MonthNames = culture.DateTimeFormat.MonthNames;
        dateTimeFormatInfo.AbbreviatedMonthNames = culture.DateTimeFormat.AbbreviatedMonthNames;

        // Use ParseExact since Parse does not work if a date separator is set but the date
        // separator is not part of the date format
        // Still this does not work properly the separator is often not enforced, assuming if "-" is set and the date contains a "." its still parsed
        if (DateTime.TryParseExact(
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
              stringDateValue
#else
              stringDateValue.ToString()
#endif
              , formats: dateTimeFormats,
              provider: dateTimeFormatInfo,
              style: DateTimeStyles.NoCurrentDateDefault,
              out var result)) return result;

        // try InvariantCulture
        if (culture.Name != "en-US" && !Equals(culture, CultureInfo.InvariantCulture))
        {
          dateTimeFormatInfo.AbbreviatedDayNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames;
          dateTimeFormatInfo.DayNames = CultureInfo.InvariantCulture.DateTimeFormat.DayNames;
          dateTimeFormatInfo.MonthNames = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames;
          dateTimeFormatInfo.AbbreviatedMonthNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames;

          if (DateTime.TryParseExact(
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
              stringDateValue
#else
                stringDateValue.ToString()
#endif
                , formats: dateTimeFormats,
                provider: dateTimeFormatInfo,
                style: DateTimeStyles.NoCurrentDateDefault,
                out result)) return result;
        }

        // In case a date with following time is passed in it would not be parsed, take the part of
        // before the space and try again
        var lastSpace = stringDateValue.LastIndexOf(' ');

        // Only do this if we have at least 6 characters
        if (lastSpace <= 6)
          return null;
        stringDateValue = stringDateValue.Slice(0, lastSpace);
      }
    }
  }
}
