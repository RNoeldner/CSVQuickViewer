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

#if NET7_0_OR_GREATER
#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace CsvTools
{
  public static class StringConversionSpan
  {
    public static CheckResult CheckDate(
     in IReadOnlyCollection<ReadOnlyMemory<char>> samples,
     in string shortDateFormat,
     char dateSeparator,
     char timeSeparator,
     in CultureInfo culture,
     in CancellationToken cancellationToken)
    {
      var checkResult = new CheckResult();
      if (samples.Count == 0)
        return checkResult;
      var allParsed = true;
      var counter = 0;
      var positiveMatches = 0;
      var threshHoldPossible = 5;
      if (samples.Count() < threshHoldPossible)
        threshHoldPossible = samples.Count() - 1;

      foreach (var value in samples)
      {
        if (cancellationToken.IsCancellationRequested)
          break;

        counter++;
        var ret = StringToDateTimeExact(value.Span, shortDateFormat, dateSeparator, timeSeparator, culture);
        if (!ret.HasValue)
        {
          allParsed = false;
          checkResult.AddNonMatch(new string(value.Span));
          // try to get some positive matches, in case the first record is invalid
          if (counter >= 5)
            break;
        }
        else
        {
          positiveMatches++;
          // if we have 5 hits or only one fail (for very low number of sample values, assume its a
          // possible match
          if (positiveMatches < threshHoldPossible || checkResult.PossibleMatch) continue;
          checkResult.PossibleMatch = true;
          checkResult.ValueFormatPossibleMatch = new ValueFormat(
            DataTypeEnum.DateTime,
            shortDateFormat,
            dateSeparator.ToStringHandle0(),
            timeSeparator.ToStringHandle0());
        }
      }

      if (allParsed)
        checkResult.FoundValueFormat = new ValueFormat(
          DataTypeEnum.DateTime,
          shortDateFormat,
          dateSeparator.ToStringHandle0(),
          timeSeparator.ToStringHandle0());

      return checkResult;
    }

    public static bool CheckGuid(in IEnumerable<ReadOnlyMemory<char>> samples, in CancellationToken cancellationToken)
    {
      var isEmpty = true;
      foreach (var value in samples)
      {
        if (cancellationToken.IsCancellationRequested)
          break;

        isEmpty = false;
        var ret = StringToGuid(value.Span);
        if (!ret.HasValue)
          return false;
      }

      return !isEmpty;
    }

       public static CheckResult CheckNumber(
      in IReadOnlyCollection<ReadOnlyMemory<char>> samples,
      char decimalSeparator,
      char thousandSeparator,
      bool allowPercentage,
      bool allowStartingZero,
      bool removeCurrencySymbols,
      in CancellationToken cancellationToken)
    {
      var checkResult = new CheckResult();
      if (samples.Count == 0)
        return checkResult;
      var allParsed = true;
      var assumeInteger = true;

      foreach (var value in samples)
      {
        if (cancellationToken.IsCancellationRequested)
          break;

        var ret = StringToDecimal(value.Span, decimalSeparator, thousandSeparator, allowPercentage, removeCurrencySymbols);
        // Any number with leading 0 should not be treated as numeric this is to avoid problems with
        // 0002 etc.
        if (!ret.HasValue || (!allowStartingZero && value.Span.StartsWith("0".AsSpan(), StringComparison.Ordinal)
                                                 && Math.Floor(ret.Value) != 0))
        {
          allParsed = false;
          checkResult.AddNonMatch(new string(value.Span));
          // try to get some positive matches, in case the first record is invalid
          if (checkResult.ExampleNonMatch.Count > 2)
            break;
        }
        else
        {
          // if the value contains the decimal separator or is too large to be an integer, its not
          // an integer
          if (value.Span.IndexOf(decimalSeparator) != -1)
            assumeInteger = false;
          else
            assumeInteger = assumeInteger && ret.Value == Math.Truncate(ret.Value) && ret.Value <= int.MaxValue
                            && ret.Value >= int.MinValue;
          if (!checkResult.PossibleMatch)
          {
            checkResult.PossibleMatch = true;
            checkResult.ValueFormatPossibleMatch = new ValueFormat(
              assumeInteger ? DataTypeEnum.Integer : DataTypeEnum.Numeric,
              groupSeparator: thousandSeparator.ToStringHandle0(),
              decimalSeparator: decimalSeparator.ToStringHandle0());
          }
        }
      }

      if (allParsed)
        checkResult.FoundValueFormat = new ValueFormat(
          assumeInteger ? DataTypeEnum.Integer : DataTypeEnum.Numeric,
          groupSeparator: thousandSeparator.ToStringHandle0(),
          decimalSeparator: decimalSeparator.ToStringHandle0());
      return checkResult;
    }

    public static CheckResult CheckSerialDate(in IEnumerable<ReadOnlyMemory<char>> samples, bool isCloseToNow,
                                              in CancellationToken cancellationToken)
    {
      var checkResult = new CheckResult();

      var allParsed = true;
      var positiveMatches = 0;
      var counter = 0;

      foreach (var value in samples)
      {
        if (cancellationToken.IsCancellationRequested)
          break;
        counter++;
        var ret = SerialStringToDateTime(value.Span);
        if (!ret.HasValue)
        {
          allParsed = false;
          checkResult.AddNonMatch(new string(value.Span));
          // try to get some positive matches, in case the first record is invalid
          if (counter >= 3)
            break;
        }
        else
        {
          if (isCloseToNow && (ret.Value.Year < DateTime.Now.Year - 80 || ret.Value.Year > DateTime.Now.Year + 20))
          {
            allParsed = false;
            checkResult.AddNonMatch(new string(value.Span));
            // try to get some positive matches, in case the first record is invalid
            if (counter > 3)
              break;
          }
          else
          {
            positiveMatches++;
            if (positiveMatches <= 5 || checkResult.PossibleMatch) continue;
            checkResult.PossibleMatch = true;
            checkResult.ValueFormatPossibleMatch = new ValueFormat(DataTypeEnum.DateTime, "SerialDate");
          }
        }
      }

      if (allParsed && counter > 0)
        checkResult.FoundValueFormat = new ValueFormat(DataTypeEnum.DateTime, "SerialDate");

      return checkResult;
    }

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
      if (dateValue == DateTimeConstants.FirstDateTime && dateColumnText != null && dateColumnText.Length > 0)
      {
        var val = CombineStringsToDateTime(
          dateColumnText,
          valueFormat.DateFormat,
          ReadOnlySpan<char>.Empty,
          valueFormat.DateSeparator.FromText(),
          valueFormat.TimeSeparator.FromText(),
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
                                                                    DateTimeConstants.FirstDateTime.DateTimeToString(valueFormat),
                                                                    StringComparison.Ordinal)))
        return null;

      return dateValue;
    }

    public static DateTime? CombineStringsToDateTime(
     ReadOnlySpan<char> datePart,
     in string? dateFormat,
     ReadOnlySpan<char> timePart,
     char dateSeparator,
     char timeSeparator,
     bool serialDateTime)
    {
      if (dateFormat is null || dateFormat.Length == 0)
        return null;

      var date = StringToDateTime(datePart, dateFormat, dateSeparator, timeSeparator, serialDateTime);

      switch (date)
      {
        // we have no date check if we have a time
        case null:
        {
          var timeP = StringToTimeSpan(timePart, timeSeparator, serialDateTime);
          // no date and no time, nothing to do
          if (timeP is null)
            return null;
          return DateTimeConstants.FirstDateTime.Add(timeP.Value);
        }
        // In case a value is read that just is a time, need to adjust c# and Excel behavior the
        // application assumes all dates on cFirstDatetime is a time only
        case { Year: 1, Month: 1 } when timePart.IsWhiteSpace():
          return date.Value.Ticks.GetTimeFromTicks();
      }

      // get the time to add to the date
      var time = StringToTimeSpan(timePart, timeSeparator, serialDateTime);
      return time is null ? date : date.Value.Add(time.Value);
    }

    public static decimal? StringToDecimal(
     ReadOnlySpan<char> value,
     char decimalSeparatorChar,
     char groupSeparatorChar,
     bool allowPercentage,
     bool currencyRemoval)
    {
      // Remove any white space
      var fieldValueSpan = value.Trim();
      // in case nothing is passed in we are already done here
      if (fieldValueSpan.Length == 0)
        return null;

      if (currencyRemoval)
      {
        foreach (var currencySymbol in StringCollections.CurrencySymbols)
        {
          if (fieldValueSpan.IndexOf(currencySymbol, StringComparison.OrdinalIgnoreCase) != -1)
            fieldValueSpan = (new string(fieldValueSpan).Replace(currencySymbol, string.Empty)).AsSpan().Trim();
        }
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
          if (lastPos >0 && pos != lastPos + 4)
            // Distance between group is not correct
            return null;
          lastPos = pos;
        }
      }

      // if we have a decimal point the group seperator has to be 3 places from the right e.g. 63.467.8373 is not ok, but 634.678.373 is
      if (lastPos > 0  && startDecimal != lastPos +4)
        return null;

      var numberFormatProvider = new NumberFormatInfo
      {
        NegativeSign = "-",
        PositiveSign = "+",
        NumberDecimalSeparator = decimalSeparatorChar.ToStringHandle0(),
        NumberGroupSeparator = groupSeparatorChar.ToStringHandle0()
      };

      if (fieldValueSpan.StartsWith("(", StringComparison.Ordinal)
          && fieldValueSpan.EndsWith(")", StringComparison.Ordinal))
      {
        fieldValueSpan = ("-" + new string(fieldValueSpan.Slice(1, fieldValueSpan.Length - 2).Trim())).AsSpan();
      }

      var percentage = false;
      // ReSharper disable once IdentifierTypo
      var permille = false;
      if (allowPercentage && fieldValueSpan.EndsWith("%", StringComparison.Ordinal))
      {
        percentage = true;
        fieldValueSpan = fieldValueSpan.Slice(0, fieldValueSpan.Length - 1);
      }

      if (allowPercentage && fieldValueSpan.EndsWith("‰", StringComparison.Ordinal))
      {
        permille = true;
        fieldValueSpan = fieldValueSpan.Slice(0, fieldValueSpan.Length - 1);
      }

      for (numberFormatProvider.NumberNegativePattern = 1;
           numberFormatProvider.NumberNegativePattern <= 3;
           numberFormatProvider.NumberNegativePattern++)
      {
        // Try to convert this value to a decimal value. Try to convert this value to a decimal value.
        if (!decimal.TryParse(fieldValueSpan, NumberStyles.Number, numberFormatProvider, out var result)) continue;
        if (percentage)
          return result / 100m;
        if (permille)
          return result / 1000m;
        return result;
      }
      // If this works, exit

      return null;
    }

    public static Guid? StringToGuid(ReadOnlySpan<char> originalValue)
    {
      if (Guid.TryParse(originalValue, out var result))
        return result;
      else
        return null;
    }

    public static short? StringToInt16(ReadOnlySpan<char> value, char decimalSeparator, char thousandSeparator)
    {
      if (value.IsEmpty)
        return null;
      try
      {
        var dec = StringToDecimal(value, decimalSeparator, thousandSeparator, false, false);
        if (dec.HasValue)
          return Convert.ToInt16(dec.Value, CultureInfo.InvariantCulture);
      }
      catch (OverflowException)
      {
        // The numerical value could not be converted to an integer
      }

      return null;
    }

    public static int? StringToInt32(ReadOnlySpan<char> value, char decimalSeparator, char thousandSeparator)
    {
      if (value.IsEmpty)
        return null;
      try
      {
        var dec = StringToDecimal(value, decimalSeparator, thousandSeparator, false, false);
        if (dec.HasValue)
          return dec.Value.ToInt();
      }
      catch (OverflowException)
      {
        // The numerical value could not be converted to an integer
      }

      return null;
    }
    public static long? StringToInt64(ReadOnlySpan<char> value, char decimalSeparator, char thousandSeparator)
    {
      if (value.IsEmpty)
        return null;
      try
      {
        var dec = StringToDecimal(value, decimalSeparator, thousandSeparator, false, false);
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
    /// <param name="value">The string value that should be split.</param>
    /// <param name="splitter">The splitter character.</param>
    /// <param name="part">The part that should be returned, starting with 1.</param>
    /// <param name="toEnd">Read the part up to the end</param>
    /// <returns>
    ///   <c>Null</c> if the value is empty or the part can not be found. If the desired part is 1
    ///   and the splitter is not contained the whole value is returned.
    /// </returns>    
    public static ReadOnlySpan<char> StringToTextPart(ReadOnlySpan<char> value, char splitter, int part, bool toEnd)
    {
      // ReSharper disable once ReplaceWithStringIsNullOrEmpty
      if (value.Length == 0 || part < 1)
        return null;

      var indexOfSplitter = value.IndexOf(splitter);

      // In case we want the first part but the splitter is not part of the text return the whole value
      if (part == 1 && (indexOfSplitter == -1 || toEnd))
        return value;

      if (!toEnd)
      {
        int start = 0;
        int end = 0;
        int partNo = 0;
        for (int i = 0; i<value.Length; i++)
        {
          if (value[i]== splitter)
          {
            start = end;
            end = i;
            partNo++;
            /// TODO: TEST this
            if (part == partNo)
              return value.Slice(start, end-start);
          }
        }
        end = value.Length;
        /// TODO: TEST this
        return value.Slice(start, end-start);
      }
      else
      {
        while (indexOfSplitter != -1 && part > 2)
        {
          indexOfSplitter = value.IndexOf(splitter, indexOfSplitter + 1);
          part--;
        }

        if (indexOfSplitter != -1)
          return value.Slice(indexOfSplitter + 1);
      }

      return null;
    }

    private static DateTime? SerialStringToDateTime(ReadOnlySpan<char> value)
    {
      var stringDateValue = value.Trim();
      try
      {
        var numberFormatProvider =
          new NumberFormatInfo { NegativeSign = "-", PositiveSign = "+", NumberGroupSeparator = string.Empty };
        foreach (var decimalSeparator in StringCollections.DecimalSeparators)
        {
          numberFormatProvider.NumberDecimalSeparator = decimalSeparator.ToString(CultureInfo.CurrentCulture);
          if (!double.TryParse(stringDateValue, NumberStyles.Float, numberFormatProvider, out var timeSerial))
            continue;
          if (timeSerial >= -657435 && timeSerial < 2958466)
            return DateTime.FromOADate(timeSerial);
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine("{0} is not a serial date. Error: {1}", new string(value), ex.Message);
      }

      return null;
    }

    internal static DataTypeEnum CheckUnescaped(in IEnumerable<ReadOnlyMemory<char>> samples, int minRequiredSamples,
                                               in CancellationToken cancellationToken)
    {
      var foundUnescaped = 0;
      var foundHtml = 0;
      foreach (var text in samples)
      {
        if (cancellationToken.IsCancellationRequested)
          break;
        if (text.Span.IndexOf("<br>".AsSpan(), StringComparison.OrdinalIgnoreCase) != -1 ||
            text.Span.StartsWith("<![CDATA[".AsSpan(), StringComparison.OrdinalIgnoreCase))
          if (foundHtml++ > minRequiredSamples)
            return DataTypeEnum.TextToHtml;
        if (text.Span.IndexOf("\\r".AsSpan(), StringComparison.Ordinal) != -1 ||
            text.Span.IndexOf("\\n".AsSpan(), StringComparison.Ordinal) != -1 ||
            text.Span.IndexOf("\\t".AsSpan(), StringComparison.Ordinal) != -1 ||
            text.Span.IndexOf("\\u".AsSpan(), StringComparison.Ordinal) != -1 ||
            text.Span.IndexOf("\\x".AsSpan(), StringComparison.Ordinal) != -1)
          if (foundUnescaped++ > minRequiredSamples)
            return DataTypeEnum.TextUnescape;
      }

      return DataTypeEnum.String;
    }


    private static DateTime? StringToDateTimeByCulture(
      ReadOnlySpan<char> stringDateValue,
      in string[] dateTimeFormats,
      in char dateSeparator,
      in char timeSeparator,
      in CultureInfo culture)
    {
      while (true)
      {
        var dateTimeFormatInfo = new DateTimeFormatInfo();

        dateTimeFormatInfo.SetAllDateTimePatterns(dateTimeFormats, 'd');
        dateTimeFormatInfo.DateSeparator = dateSeparator.ToStringHandle0();
        dateTimeFormatInfo.TimeSeparator = timeSeparator.ToStringHandle0();

        dateTimeFormatInfo.AbbreviatedDayNames = culture.DateTimeFormat.AbbreviatedDayNames;
        dateTimeFormatInfo.DayNames = culture.DateTimeFormat.DayNames;
        dateTimeFormatInfo.MonthNames = culture.DateTimeFormat.MonthNames;
        dateTimeFormatInfo.AbbreviatedMonthNames = culture.DateTimeFormat.AbbreviatedMonthNames;

        // Use ParseExact since Parse does not work if a date separator is set but the date
        // separator is not part of the date format
        // Still this does not work properly the separator is often not enforced, assuming if "-" is set and the date contains a "." its still parsed
        if (DateTime.TryParseExact(
              stringDateValue,
              dateTimeFormats,
              dateTimeFormatInfo,
              DateTimeStyles.NoCurrentDateDefault,
              out var result)) return result;

        // try InvariantCulture
        if (culture.Name != "en-US" && !Equals(culture, CultureInfo.InvariantCulture))
        {
          dateTimeFormatInfo.AbbreviatedDayNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames;
          dateTimeFormatInfo.DayNames = CultureInfo.InvariantCulture.DateTimeFormat.DayNames;
          dateTimeFormatInfo.MonthNames = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames;
          dateTimeFormatInfo.AbbreviatedMonthNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames;

          if (DateTime.TryParseExact(
                stringDateValue,
                dateTimeFormats,
                dateTimeFormatInfo,
                DateTimeStyles.NoCurrentDateDefault,
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

    public static bool? StringToBoolean(ReadOnlySpan<char> value, ReadOnlySpan<char> trueValue, ReadOnlySpan<char> falseValue)
    => StringToBooleanStrict2(value, trueValue, falseValue);

    internal static bool? StringToBooleanStrict2(ReadOnlySpan<char> value, ReadOnlySpan<char> trueValue, ReadOnlySpan<char> falseValue)
    {
      if (value.Length == 0)
        return null;

      var start = 0;
      var end = trueValue.IndexOfAny(StringUtils.m_DelimiterChar);
      while (end!=-1)
      {
        if (MemoryExtensions.Equals(value, trueValue.Slice(start, end-start-1), StringComparison.OrdinalIgnoreCase))
          return true;
        start = end;
      }
      end = trueValue.Length;
      if (MemoryExtensions.Equals(value, trueValue.Slice(start, end-start-1), StringComparison.OrdinalIgnoreCase))
        return true;

      start = 0;
      end = falseValue.IndexOfAny(StringUtils.m_DelimiterChar);
      while (end!=-1)
      {
        if (MemoryExtensions.Equals(value, falseValue.Slice(start, end-start-1), StringComparison.OrdinalIgnoreCase))
          return false;
        start = end;
      }
      end = trueValue.Length;
      if (MemoryExtensions.Equals(value, falseValue.Slice(start, end-start-1), StringComparison.OrdinalIgnoreCase))
        return false;

      foreach (var text in StringCollections.m_TrueValues)
        if (MemoryExtensions.Equals(value, text.AsSpan(), StringComparison.OrdinalIgnoreCase))
          return true;

      foreach (var text in StringCollections.m_FalseValues)
        if (MemoryExtensions.Equals(value, text.AsSpan(), StringComparison.OrdinalIgnoreCase))
          return false;

      return null;
    }

    public static DateTime? StringToDateTime(
    ReadOnlySpan<char> originalValue,
    string dateFormat,
    char dateSeparator,
    char timeSeparator,
    bool serialDateTime)
    {
      var stringDateValue = originalValue.Trim();

      var result = StringToDateTimeExact(originalValue, dateFormat, dateSeparator, timeSeparator,
        CultureInfo.CurrentCulture);
      if (result.HasValue)
        return result.Value;

      if (serialDateTime
         && (stringDateValue.IndexOf(dateSeparator) == -1)
         && (stringDateValue.IndexOf(timeSeparator) == -1))
        return SerialStringToDateTime(stringDateValue);

      // in case its time only and we do not have any date separator try a timespan
      if (stringDateValue.IndexOf(dateSeparator) != -1 || dateFormat.IndexOf('/') != -1) return null;
      var ts = StringToTimeSpan(stringDateValue, timeSeparator, false);
      if (ts.HasValue)
        return new DateTime(ts.Value.Ticks);

      return null;
    }

    public static DateTime? StringToDateTimeExact(
         ReadOnlySpan<char> originalValue,
         in string dateFormats,
         char dateSeparator,
         char timeSeparator,
         in CultureInfo culture)
    {
      var stringDateValue = new string(originalValue.Trim()).Replace("\t", " ").Replace("  ", " ").AsSpan();

      if (stringDateValue.Length < 4)
        return null;

      // Quick check: If the entry is empty, or a constant string, or the length does not make
      // sense, we do not need to try and parse
      if (stringDateValue == "00000000" || stringDateValue == "99999999" || stringDateValue.Length < 4)
        return null;

      // get rid of numeric suffixes like 12th or 3rd for dates
      if (stringDateValue.IndexOf("th ".AsSpan(), StringComparison.OrdinalIgnoreCase) != -1
          || stringDateValue.IndexOf("nd ".AsSpan(), StringComparison.OrdinalIgnoreCase) != -1
          || stringDateValue.IndexOf("st ".AsSpan(), StringComparison.OrdinalIgnoreCase) != -1
          || stringDateValue.IndexOf("rd ".AsSpan(), StringComparison.OrdinalIgnoreCase) != -1)
        stringDateValue = StringCollections.m_RegExNumberSuffixEnglish.Value.Replace(new string(stringDateValue), "$1").AsSpan();
      var matchingDateTimeFormats = new List<string>();
      foreach (var dateTimeFormat in StringUtils.SplitByDelimiter((dateSeparator == char.MinValue && dateFormats.IndexOf('/') != -1) ? dateFormats.Replace("/", "") : dateFormats))
      {
        if (StringCollections.StandardDateTimeFormats.DateLengthMatches(stringDateValue.Length, dateTimeFormat))
          matchingDateTimeFormats.Add(dateTimeFormat);
        // In case of a date & time format add the date only format separately
        var indexHour = dateTimeFormat.IndexOf("h", StringComparison.OrdinalIgnoreCase);
        // assuming there is a text before the hour that has a reasonable size take it as date
        if (indexHour > 4)
        {
          var dateOnlyFmt = dateTimeFormat.Substring(0, indexHour - 1).Trim();
          if (StringCollections.StandardDateTimeFormats.DateLengthMatches(stringDateValue.Length, dateOnlyFmt))
            matchingDateTimeFormats.Add(dateOnlyFmt);
        }
      }

      if (matchingDateTimeFormats.Count == 0)
        return null;

      return StringToDateTimeByCulture(
        stringDateValue,
        matchingDateTimeFormats.ToArray(),
        dateSeparator,
        timeSeparator,
        culture);
    }

    private static int IndexOf(this ReadOnlySpan<char> originalValue, char separator, int start = 0)
    {
      for (int i = start; i < originalValue.Length; i++)
        if (originalValue[i]==separator)
          return i;
      return -1;
    }

    public static TimeSpan? StringToTimeSpan(ReadOnlySpan<char> originalValue, char separator, bool serialDateTime)
    {
      var stringTimeValue = originalValue.Trim();
      if (stringTimeValue.IsEmpty)
        return null;

      var min = 0;
      var sec = 0;
      var hrsIndex = IndexOf(stringTimeValue, separator);
      if (hrsIndex < 0)
      {
        if (!serialDateTime)
          return null;
        var dt = SerialStringToDateTime(stringTimeValue);
        if (dt.HasValue)
          return new TimeSpan(dt.Value.Ticks - DateTimeConstants.FirstDateTime.Ticks);

        return null;
      }

      var hrs = stringTimeValue.Slice(0, hrsIndex);
      if (hrs.IndexOf(' ') != -1)
        return null;

      if (int.TryParse(hrs, out var hours))
      {
        hrsIndex++;
        var minIndex = stringTimeValue.IndexOf(separator, hrsIndex);
        if (minIndex > 0)
        {
          if (int.TryParse(stringTimeValue.Slice(hrsIndex, minIndex - hrsIndex), out min))
            int.TryParse(stringTimeValue.Slice(minIndex + 1), out sec);
        }
        else
        {
          int.TryParse(stringTimeValue.Slice(hrsIndex), out min);
        }
      }

      // Handle am / pm

      // 12:00 AM - 12:59 AM --> 00:00 - 00:59
      if (hours == 12 && stringTimeValue.EndsWith("am".AsSpan(), StringComparison.OrdinalIgnoreCase))
        hours -= 12;

      // 12:00 PM - 12:59 PM 12:00 - 12:59 No change

      // 01:00 pm - 11:59 PM --> 13:00 - 23:59
      if (hours < 12 && stringTimeValue.EndsWith("pm".AsSpan(), StringComparison.OrdinalIgnoreCase))
        hours += 12;

      return new TimeSpan(0, hours, min, sec);
    }
  }
}
#endif