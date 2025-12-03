/*
 * CSVQuickViewer - A CSV viewing utility - Copyright (C) 2014 Raphael Nöldner
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
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace CsvTools;

/// <summary>
///   Static class to deal with sample conversion detection, dealing with text to determine if a sample format can be used
/// </summary>
public static class CheckTexts
{
  /// <summary>
  ///   Attempts to parse a collection of text values as dates using the specified format,
  ///   returning information about which values matched or failed.
  /// </summary>
  /// <param name="samples">The sample values to be checked.</param>
  /// <param name="dateFormatPattern">The expected short date format pattern.</param>
  /// <param name="dateSep">The date separator character.</param>
  /// <param name="timeSep">The time separator character.</param>
  /// <param name="culture">The culture used for parsing (important for named days or months).</param>
  /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
  /// <returns>A <see cref="CheckResult"/> with information on confirmed and possible format matches and what did not match.</returns>
  public static CheckResult CheckDate(this IReadOnlyCollection<ReadOnlyMemory<char>> samples,
    ReadOnlySpan<char> dateFormatPattern, char dateSep, char timeSep,
    in CultureInfo culture, CancellationToken cancellationToken)
  {
    var result = new CheckResult();
    if (samples.Count == 0)
      return result;

    var allParsed = true;
    var counter = 0;
    var positiveMatches = 0;
    var thresholdPossible = Math.Max(1, Math.Min(5, samples.Count));

    bool needToCheckLength = dateFormatPattern.IndexOf('F') != -1
        || (dateFormatPattern.IndexOf('M') != -1 && !dateFormatPattern.Contains("MM".AsSpan(), StringComparison.Ordinal))
        || (dateFormatPattern.IndexOf('d') != -1 && !dateFormatPattern.Contains("DD".AsSpan(), StringComparison.Ordinal))
        || (dateFormatPattern.IndexOf('H') != -1 && !dateFormatPattern.Contains("HH".AsSpan(), StringComparison.Ordinal));
    foreach (var sample in samples)
    {
      // in case the sample ends in z remove that.
      if (cancellationToken.IsCancellationRequested)
        break;

      var span = sample.Span;

      // Remove trailing 'Z' or 'z' if present
      if (span.Length > 0 && (span[span.Length - 1] == 'Z'))
        span = span.Slice(0, span.Length - 1);

      counter++;
      var parsedDate = span.StringToDateTimeExact(dateFormatPattern, dateSep, timeSep, culture);
      // Reformat the found date with the format because StringToDateTimeExact would still parse 06/12/2025 even with d/M/yyyy
      if (!parsedDate.HasValue || (needToCheckLength && (span.Length  != parsedDate.Value.ToString(dateFormatPattern.ToString(), culture).Length)))
      {
        allParsed = false;
        result.AddNonMatch(span.ToString());

        // If the first few are invalid, stop early
        if (counter >= 5)
          break;
      }
      else
      {
        positiveMatches++;

        // Mark as possible match if threshold reached
        if (!result.PossibleMatch && positiveMatches >= thresholdPossible)
        {
          result.PossibleMatch = true;
          result.ValueFormatPossibleMatch = new ValueFormat(
            DataTypeEnum.DateTime,
            dateFormatPattern.ToString(),
            dateSep.ToString(),
            timeSep.ToString());
        }
      }
    }

    if (allParsed)
    {
      result.FoundValueFormat = new ValueFormat(
        DataTypeEnum.DateTime,
        dateFormatPattern.ToString(),
        dateSep.ToString(),
        timeSep.ToString());
    }

    return result;
  }


  /// <summary>
  ///   Checks whether all sample values are valid GUIDs.
  /// </summary>
  /// <param name="samples">The sample values to be checked.</param>
  /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
  /// <returns>
  ///   <c>true</c> if all non-empty values can be interpreted as <see cref="Guid"/>; 
  ///   <c>false</c> otherwise.
  /// </returns>
  public static bool CheckGuid(this IEnumerable<ReadOnlyMemory<char>> samples, CancellationToken cancellationToken)
  {
    var hasSamples = false;

    foreach (var sample in samples)
    {
      if (cancellationToken.IsCancellationRequested)
        break;

      hasSamples = true;

      var parsed = sample.Span.StringToGuid();
      if (!parsed.HasValue)
        return false;
    }

    return hasSamples;
  }


  /// <summary>
  ///   Checks if the sample values are numbers (integer or numeric)
  /// </summary>
  /// <param name="samples">The sample values to be checked.</param>
  /// <param name="decimalSep">The decimal separator.</param>
  /// <param name="thousandSep">The thousand separator.</param>
  /// <param name="allowPercentage">Allows Percentages</param>
  /// <param name="allowStartingZero">if set to <c>true</c> numbers with leading zeros will still be parsed, if <c>false</c> these numbers will not be seen as
  /// numbers</param>
  /// <param name="removeCurrencySymbols">if set to <c>true</c> common currency symbols are removed.</param>
  /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
  /// <returns>A <see cref="CheckResult"/> with information on confirmed and possible format matches and what did not match.</returns>
  public static CheckResult CheckNumber(this IReadOnlyCollection<ReadOnlyMemory<char>> samples, char decimalSep,
    char thousandSep, bool allowPercentage, bool allowStartingZero,
    bool removeCurrencySymbols, CancellationToken cancellationToken)
  {
    var result = new CheckResult();
    if (samples.Count == 0)
      return result;
    var allParsed = true;
    var assumeInteger = true;

    foreach (var sample in samples)
    {
      if (cancellationToken.IsCancellationRequested)
        break;

      var ret = sample.Span.StringToDecimal(decimalSep, thousandSep, allowPercentage,
        removeCurrencySymbols);
      // Any number with leading 0 should NOT be treated as numeric this is to avoid problems with
      // 0002 etc.        
      if (!ret.HasValue || (!allowStartingZero && (sample.Span.Length > 1 && sample.Span[0] == '0') && Math.Floor(ret.Value) != 0))
      {
        allParsed = false;
        result.AddNonMatch(sample.ToString());
        // try to get some positive matches, in case the first record is invalid
        if (result.ExampleNonMatch.Count > 2)
          break;
      }
      else
      {
        // if the sample contains the decimal separator or is too large to be an integer, it's not
        // an integer
        if (sample.Span.IndexOf(decimalSep) != -1)
          assumeInteger = false;
        else
          assumeInteger = assumeInteger && ret.Value == Math.Truncate(ret.Value) && ret.Value <= int.MaxValue
                          && ret.Value >= int.MinValue;
        if (result.PossibleMatch) continue;
        result.PossibleMatch = true;
        result.ValueFormatPossibleMatch = new ValueFormat(
          assumeInteger ? DataTypeEnum.Integer : DataTypeEnum.Numeric,
          groupSeparator: thousandSep.ToStringHandle0(),
          decimalSeparator: decimalSep.ToStringHandle0());
      }
    }

    if (allParsed)
      result.FoundValueFormat = new ValueFormat(
        assumeInteger ? DataTypeEnum.Integer : DataTypeEnum.Numeric,
        groupSeparator: thousandSep.ToStringHandle0(),
        decimalSeparator: decimalSep.ToStringHandle0());
    return result;
  }

  /// <summary>
  ///   Checks if the sample values can be interpreted as serial dates.
  ///   Returns a CheckResult indicating the success or possible matches.
  /// </summary>
  /// <param name="samples">The sample values to be checked.</param>
  /// <param name="isCloseToNow">
  ///   If true, only numbers that produce dates within ~80 years past to 20 years future relative to today are considered valid.    
  /// </param>    
  /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
  /// <returns>A <see cref="CheckResult"/> with information on confirmed and possible format matches and what did not match.</returns>
  public static CheckResult CheckSerialDate(this IEnumerable<ReadOnlyMemory<char>> samples, bool isCloseToNow, CancellationToken cancellationToken)
  {
    var result = new CheckResult();

    var allParsed = true;
    var positiveMatches = 0;
    var invalid = 0;

    var now = DateTime.UtcNow;
    var minDate = now.AddYears(-80);
    var maxDate = now.AddYears(20);

    foreach (var sample in samples)
    {
      if (cancellationToken.IsCancellationRequested)
        break;

      var parsed = sample.Span.SerialStringToDateTime();

      if (!parsed.HasValue || (isCloseToNow && (parsed.Value < minDate || parsed.Value > maxDate)))
      {
        allParsed = false;
        invalid++;
        if (result.ExampleNonMatch.Count < 5)
          result.AddNonMatch(sample.Span.ToString());

        if (invalid > 3 && positiveMatches == 0)
          break;
        continue;
      }

      positiveMatches++;
      if (positiveMatches > 5 && !result.PossibleMatch)
      {
        result.PossibleMatch = true;
        result.ValueFormatPossibleMatch = new ValueFormat(DataTypeEnum.DateTime, "SerialDate");
      }
    }

    if (allParsed && positiveMatches > 0)
      result.FoundValueFormat = new ValueFormat(DataTypeEnum.DateTime, "SerialDate");

    return result;
  }


  /// <summary>
  ///   Analyzes sample text values to detect indications that the content
  ///   may be HTML-encoded or contain C-style escaped characters.
  /// </summary>
  /// <param name="samples">The text samples to analyze.</param>
  /// <param name="minRequiredSamples">The minimum number of matches required to decide on a non-string type.</param>
  /// <param name="cancellationToken">Cancellation token to stop a possibly long running process</param>
  /// <returns>
  /// Returns one of the following:
  /// <list type="bullet">
  /// <item><description><see cref="DataTypeEnum.TextToHtml"/> if HTML-like content (e.g. &lt;br&gt; or &lt;![CDATA[) is detected.</description></item>
  /// <item><description><see cref="DataTypeEnum.TextUnescape"/> if C-style escape sequences (e.g. \n, \t, \u) are detected.</description></item>
  /// <item><description><see cref="DataTypeEnum.String"/> if no strong indication for encoding or escaping is found.</description></item>
  /// </list>
  /// </returns>
  internal static DataTypeEnum CheckUnescaped(this IEnumerable<ReadOnlyMemory<char>> samples, int minRequiredSamples, CancellationToken cancellationToken)
  {
    const StringComparison OrdinalIgnoreCase = StringComparison.OrdinalIgnoreCase;
    const StringComparison Ordinal = StringComparison.Ordinal;

    ReadOnlySpan<char> brSpan = "<br>".AsSpan();
    ReadOnlySpan<char> cdataSpan = "<![CDATA[".AsSpan();
    ReadOnlySpan<string> escapeSequences = new[] { "\\r", "\\n", "\\t", "\\u", "\\x" };

    int foundHtml = 0, foundUnescaped = 0;

    foreach (var text in samples)
    {
      if (cancellationToken.IsCancellationRequested)
        break;

      var span = text.Span;

      // HTML-like indicators
      if (span.IndexOf(brSpan, OrdinalIgnoreCase) != -1 || span.StartsWith(cdataSpan, OrdinalIgnoreCase))
      {
        if (++foundHtml > minRequiredSamples)
          return DataTypeEnum.TextToHtml;
      }

      // C-style escape sequences
      foreach (var esc in escapeSequences)
      {
        if (span.IndexOf(esc.AsSpan(), Ordinal) != -1)
        {
          if (++foundUnescaped > minRequiredSamples)
            return DataTypeEnum.TextUnescape;
          break;
        }
      }
    }

    return DataTypeEnum.String;
  }
}