using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace CsvTools
{
  /// <summary>
  ///   Static class to deal with value conversion detection, dealing with text to determine if a value format can be used
  /// </summary>
  public static class CheckTexts
  {
    /// <summary>
    ///   Taking a collection of text values, tries parsing them as dates in the provided format, 
    ///   and returns information on which succeeded or failed to help the caller determine if 
    ///   the texts likely contain dates in that format.
    /// </summary>
    /// <param name="samples">The sample values to be checked.</param>
    /// <param name="shortDateFormat">The short date format.</param>
    /// <param name="dateSeparator">The date separator.</param>
    /// <param name="timeSeparator">The time separator.</param>
    /// <param name="culture">the culture to check (important for named Days or month)</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>A CheckResult with information on what was found and what did not match</returns>
    public static CheckResult CheckDate(this IReadOnlyCollection<ReadOnlyMemory<char>> samples,
                                        ReadOnlySpan<char> shortDateFormat, char dateSeparator, char timeSeparator,
                                        in CultureInfo culture, CancellationToken cancellationToken)
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
        var ret = value.Span.StringToDateTimeExact(shortDateFormat, dateSeparator, timeSeparator, culture);
        if (!ret.HasValue)
        {
          allParsed = false;
          checkResult.AddNonMatch(value.ToString());
          // try to get some positive matches, in case the first record is invalid
          if (counter >= 5)
            break;
        }
        else
        {
          positiveMatches++;
          // if we have 5 hits or only one fail (for very low number of sample values, assume it's a
          // possible match
          if (positiveMatches < threshHoldPossible || checkResult.PossibleMatch) continue;
          checkResult.PossibleMatch = true;
          checkResult.ValueFormatPossibleMatch = new ValueFormat(
            DataTypeEnum.DateTime,
            shortDateFormat.ToString(),
            dateSeparator.ToString(),
            timeSeparator.ToString());
        }
      }

      if (allParsed)
        checkResult.FoundValueFormat = new ValueFormat(
          DataTypeEnum.DateTime,
          shortDateFormat.ToString(),
          dateSeparator.ToString(),
          timeSeparator.ToString());

      return checkResult;
    }

    /// <summary>
    ///   Checks if the samples values are GUIDs
    /// </summary>
    /// <param name="samples">The sample values to be checked.</param>
    /// ///
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns><c>true</c> if all values can be interpreted as Guid, <c>false</c> otherwise.</returns>
    public static bool CheckGuid(this IEnumerable<ReadOnlyMemory<char>> samples, CancellationToken cancellationToken)
    {
      var isEmpty = true;
      foreach (var value in samples)
      {
        if (cancellationToken.IsCancellationRequested)
          break;

        isEmpty = false;
        var ret = value.Span.StringToGuid();
        if (!ret.HasValue)
          return false;
      }

      return !isEmpty;
    }

    /// <summary>
    ///   Checks if the sample values are numbers (integer or numeric)
    /// </summary>
    /// <param name="samples">The sample values to be checked.</param>
    /// <param name="decimalSeparator">The decimal separator.</param>
    /// <param name="thousandSeparator">The thousand separator.</param>
    /// <param name="allowPercentage">Allows Percentages</param>
    /// <param name="allowStartingZero">if set to <c>true</c> numbers with leading zeros will still be parsed, if <c>false</c> these numbers will not be seen as
    /// numbers</param>
    /// <param name="removeCurrencySymbols">if set to <c>true</c> common currency symbols are removed.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>A CheckResult with information on what was found and what did not match</returns>
    public static CheckResult CheckNumber(this IReadOnlyCollection<ReadOnlyMemory<char>> samples, char decimalSeparator,
                                          char thousandSeparator, bool allowPercentage, bool allowStartingZero,
                                          bool removeCurrencySymbols, CancellationToken cancellationToken)
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

        var ret = value.Span.StringToDecimal(decimalSeparator, thousandSeparator, allowPercentage,
          removeCurrencySymbols);
        // Any number with leading 0 should NOT be treated as numeric this is to avoid problems with
        // 0002 etc.
        if (!ret.HasValue || (!allowStartingZero && value.Span.StartsWith("0".AsSpan(), StringComparison.Ordinal)
                                                 && Math.Floor(ret.Value) != 0))
        {
          allParsed = false;
          checkResult.AddNonMatch(value.ToString());
          // try to get some positive matches, in case the first record is invalid
          if (checkResult.ExampleNonMatch.Count > 2)
            break;
        }
        else
        {
          // if the value contains the decimal separator or is too large to be an integer, it's not
          // an integer
          if (value.Span.IndexOf(decimalSeparator) != -1)
            assumeInteger = false;
          else
            assumeInteger = assumeInteger && ret.Value == Math.Truncate(ret.Value) && ret.Value <= int.MaxValue
                            && ret.Value >= int.MinValue;
          if (checkResult.PossibleMatch) continue;
          checkResult.PossibleMatch = true;
          checkResult.ValueFormatPossibleMatch = new ValueFormat(
            assumeInteger ? DataTypeEnum.Integer : DataTypeEnum.Numeric,
            groupSeparator: thousandSeparator.ToStringHandle0(),
            decimalSeparator: decimalSeparator.ToStringHandle0());
        }
      }

      if (allParsed)
        checkResult.FoundValueFormat = new ValueFormat(
          assumeInteger ? DataTypeEnum.Integer : DataTypeEnum.Numeric,
          groupSeparator: thousandSeparator.ToStringHandle0(),
          decimalSeparator: decimalSeparator.ToStringHandle0());
      return checkResult;
    }

    /// <summary>
    ///   Checks if the samples values are dates, times or serial dates
    /// </summary>
    /// <param name="samples">The sample values to be checked.</param>
    /// <param name="isCloseToNow">
    ///   Only assume the number is a serial date if the resulting date is around the current date
    ///   (-80 +20 years)
    /// </param>
    /// ///
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns><c>true</c> if all values can be interpreted as date, <c>false</c> otherwise.</returns>
    public static CheckResult CheckSerialDate(this IEnumerable<ReadOnlyMemory<char>> samples, bool isCloseToNow,
                                              CancellationToken cancellationToken)
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
        var ret = value.Span.SerialStringToDateTime();
        if (!ret.HasValue)
        {
          allParsed = false;
          checkResult.AddNonMatch(value.ToString());
          // try to get some positive matches, in case the first record is invalid
          if (counter >= 3)
            break;
        }
        else
        {
          if (isCloseToNow && (ret.Value.Year < DateTime.Now.Year - 80 || ret.Value.Year > DateTime.Now.Year + 20))
          {
            allParsed = false;
            checkResult.AddNonMatch(value.ToString());
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

    /// <summary>
    ///   Check if a text does contain indications that suggest to use something else than DataType.String
    /// </summary>
    /// <param name="samples">The sample values to be checked.</param>
    /// <param name="minRequiredSamples">The minimum required samples.</param>
    /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
    /// <returns>
    ///   <see cref="DataTypeEnum.TextToHtml" /> is to be assumed the text has HTML encoding <see
    ///   cref="DataTypeEnum.TextUnescape" /> is to be assumed the text has C encoding otherwise
    ///   <see cref="DataTypeEnum.String" />
    /// </returns>
    internal static DataTypeEnum CheckUnescaped(this IEnumerable<ReadOnlyMemory<char>> samples, int minRequiredSamples,
                                                CancellationToken cancellationToken)
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
        if (text.Span.IndexOf("\\r".AsSpan(), StringComparison.Ordinal) == -1 &&
            text.Span.IndexOf("\\n".AsSpan(), StringComparison.Ordinal) == -1 &&
            text.Span.IndexOf("\\t".AsSpan(), StringComparison.Ordinal) == -1 &&
            text.Span.IndexOf("\\u".AsSpan(), StringComparison.Ordinal) == -1 &&
            text.Span.IndexOf("\\x".AsSpan(), StringComparison.Ordinal) == -1) continue;
        if (foundUnescaped++ > minRequiredSamples)
          return DataTypeEnum.TextUnescape;
      }

      return DataTypeEnum.String;
    }
  }
}