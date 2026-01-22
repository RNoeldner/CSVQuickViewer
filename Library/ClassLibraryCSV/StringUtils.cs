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
#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CsvTools;

/// <summary>
///   Collection of static functions for string
/// </summary>
[DebuggerStepThrough]
public static class StringUtils
{
  /// <summary>
  /// Gets the not secured text
  /// </summary>
  /// <param name="secPassword">The secured text.</param>    
  public static string GetText(this System.Security.SecureString secPassword)
  {
    var unmanagedString = IntPtr.Zero;
    try
    {
      unmanagedString = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secPassword);
      return System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanagedString) ?? string.Empty;
    }
    finally
    {
      System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
    }
  }

  /// <summary>
  /// Converts a text to a secured text
  /// </summary>
  /// <param name="text">The plain text</param>    
  /// <exception cref="System.ArgumentNullException">text</exception>
  public static System.Security.SecureString ToSecureString(this string text)
  {
    if (text is null)
      throw new ArgumentNullException(nameof(text));
    var securePassword = new System.Security.SecureString();

    foreach (var c in text)
      securePassword.AppendChar(c);

    securePassword.MakeReadOnly();
    return securePassword;
  }

  /// <summary>
  /// Determines if a column name indicates that it's an identifier column.
  /// </summary>
  /// <param name="columnName">The column name</param>
  /// <returns>
  ///   The number of charters at the end that did match, 0 if it does not end on ID
  /// </returns>
  public static int AssumeIdColumn(string columnName) => AssumeIdColumn(columnName.AsSpan());


  /// <summary>
  /// Determines if a column name indicates that it's an identifier column.
  /// </summary>
  /// <param name="columnName">Name of the column.</param>
  /// <returns>
  ///   The number of charters at the end that did match, 0 if it does not end on ID
  /// </returns>
  public static int AssumeIdColumn(in ReadOnlySpan<char> columnName)
  {
    if (columnName.TrimEnd().Length == 0)
      return 0;

    if (columnName.EndsWith(" Text".AsSpan(), StringComparison.OrdinalIgnoreCase))
      return 5;
    if (columnName.EndsWith("Text".AsSpan(), StringComparison.Ordinal))
      return 4;

    if (columnName.EndsWith(" Ref".AsSpan(), StringComparison.OrdinalIgnoreCase))
      return 4;
    if (columnName.EndsWith("Ref".AsSpan(), StringComparison.Ordinal))
      return 3;

    if (columnName.EndsWith(" ID".AsSpan(), StringComparison.OrdinalIgnoreCase))
      return 3;
    if ((columnName.EndsWith("ID".AsSpan(), StringComparison.Ordinal) || columnName.EndsWith("Id".AsSpan(), StringComparison.Ordinal)) &&
        !columnName.EndsWith("GUID".AsSpan(), StringComparison.OrdinalIgnoreCase))
      return 2;
    return 0;
  }

  /// <summary>
  ///   Gets the short representation of the text.
  /// </summary>
  /// <param name="text">The text.</param>
  /// <param name="length">The length.</param>
  /// <returns>The text with the maximum length, in case it has been cut off a … is added</returns>
  public static string GetShortDisplay(string? text, int length)
  {
    var withoutLineFeed = text?.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Replace("  ", " ")
      .Replace("  ", " ") ?? string.Empty;
    if (string.IsNullOrWhiteSpace(withoutLineFeed))
      return string.Empty;
    if (length < 1 || withoutLineFeed.Length <= length)
      return withoutLineFeed;
    withoutLineFeed = withoutLineFeed.Substring(0, length - 1);
    var spaceIndex = withoutLineFeed.LastIndexOf(" ", length - 1 - (length / 8), StringComparison.Ordinal);
    if (spaceIndex > 1)
      return withoutLineFeed.Substring(0, spaceIndex) + "…";

    return withoutLineFeed + "…";
  }

  /// <summary>
  ///   All combination of \r \n will be made to a single replacement
  /// </summary>
  /// <param name="text">The Text</param>
  /// <param name="replace">The replacement value default is \n.</param>
  /// <returns>
  ///   The text with every combination of line feed replaced with the given replacement
  /// </returns>
  public static string HandleCrlfCombinations(this string text, string replace = "\n")
  {
    var sb = new StringBuilder(text.Length);
    var lastC = '\0';
    foreach (var chr in text)
    {
      switch (chr)
      {
        case '\r' when lastC == '\n':
        case '\n' when lastC == '\r':
          lastC = '\0';
          continue;
        case '\r':
        case '\n':
          sb.Append(replace);
          break;
        default:
          sb.Append(chr);
          break;
      }

      lastC  = chr;
    }
    return sb.ToString();
  }

  /// <summary>
  ///   Joins the strings
  /// </summary>
  /// <param name="parts">The parts to be joined.</param>
  /// <param name="joinWith">The join with.</param>
  /// <example>JoinParts(new [] {"My","","Test")=&gt; My, Test</example>
  /// <remarks>Any empty string will be ignored.</remarks>
  /// <returns>A string</returns>
  public static string Join(this IEnumerable<string> parts, string joinWith)
  {
    var sb = new StringBuilder(100);
    foreach (var part in parts)
    {
      if (string.IsNullOrEmpty(part))
        continue;
      if (sb.Length > 0)
        sb.Append(joinWith);
      sb.Append(part);
    }
    return sb.ToString();
  }

  /// <summary>
  ///   Joins the strings
  /// </summary>
  /// <param name="parts">The parts to be joined.</param>
  /// <param name="joinWith">The join with.</param>
  /// <example>JoinParts(new [] {"My","","Test")=&gt; My, Test</example>
  /// <remarks>Any empty string will be ignored.</remarks>
  /// <returns>A string</returns>
  public static string Join(this IEnumerable<string> parts, char joinWith = ',')
  {
    var sb = new StringBuilder(100);
    foreach (var part in parts)
    {
      if (string.IsNullOrEmpty(part))
        continue;
      if (sb.Length > 0)
        sb.Append(joinWith);
      sb.Append(part);
    }
    return sb.ToString();
  }

  /// <summary>
  /// Ensures that a given name is unique within a collection by appending a numeric suffix if necessary.
  /// </summary>
  /// <param name="previousColumns">
  /// A collection of already used names. Names in this collection will not be modified.
  /// Typically, this should be case-insensitive if the caller wants uniqueness to ignore case.
  /// </param>
  /// <param name="nameToAdd">The desired name to add to the collection.</param>
  /// <returns>
  /// A name that is guaranteed to be unique in <paramref name="previousColumns"/>.  
  /// If the original name already exists, a numeric suffix is appended (e.g., "Column1", "Column2", etc.).  
  /// If the original name ended with a trailing ellipsis (…), the ellipsis is preserved in the returned name.
  /// </returns>
  /// <exception cref="ArgumentNullException">Thrown if <paramref name="nameToAdd"/> is null.</exception>
  /// <remarks>
  /// 1. Trailing ellipsis (… or Unicode U+2026) is temporarily removed to avoid interfering with numeric suffix logic, 
  ///    and then re-added to the generated unique name if present.  
  /// 2. Any existing numeric suffix in the name is removed before generating the counter to avoid conflicts.  
  /// </remarks>
  public static string MakeUniqueInCollection(this IReadOnlyCollection<string> previousColumns, string nameToAdd)
  {
    if (nameToAdd is null)
      throw new ArgumentNullException(nameof(nameToAdd));

    // Already unique
    if (!previousColumns.Contains(nameToAdd))
      return nameToAdd;

    // Detect and temporarily remove trailing ellipsis
    bool hadEllipsis = nameToAdd.EndsWith("…");
    string cleanName = hadEllipsis ? nameToAdd.Substring(0, nameToAdd.Length - 1) : nameToAdd;

    // Remove numeric suffix if present
    int lastIndex = cleanName.Length - 1;
    while (lastIndex >= 0 && char.IsDigit(cleanName[lastIndex]))
      lastIndex--;

    var prefix = cleanName.Substring(0, lastIndex + 1);

    // Generate unique name
    int counter = 1;
    string candidate;
    do
    {
      candidate = prefix + counter++;
      if (hadEllipsis)
        candidate += "…"; // Re-add ellipsis
    } while (previousColumns.Contains(candidate));

    return candidate;
  }

  /// <summary>
  ///   Used to get text without control characters
  /// </summary>
  /// <param name="original">The original text.</param>
  /// <returns>The original text without U+0000 through U+001F or U+0080 through U+009F. but keeping \r\n</returns>
  public static string NoControlCharacters(this string original)
  {
    var chars = new char[original.Length];
    var count = 0;
    foreach (var c in from c in original
             let oc = CharUnicodeInfo.GetUnicodeCategory(c)
             where UnicodeCategory.Control != oc || c == '\r' || c == '\n'
             select c)
      chars[count++] = c;

    return new string(chars, 0, count);
  }

  /// <summary>
  ///   Used to get text without control characters, line feeds are passed on
  /// </summary>
  /// <param name="original">The original text.</param>
  /// <returns>The original text without U+0000 through U+001F or U+0080 through U+009F. but keeping \r\n</returns>
  public static ReadOnlySpan<char> NoControlCharacters(this ReadOnlySpan<char> original)
  {
    // not using Stackalloc as chars could not stay on stack anyway.
    var chars = new char[original.Length];
    var count = 0;
    foreach (var c in original)
    {
      var oc = CharUnicodeInfo.GetUnicodeCategory(c);
      if (UnicodeCategory.Control != oc || c == '\r' || c == '\n')
        chars[count++] = c;
    }
    return new ReadOnlySpan<char>(chars, 0, count);
  }

  /// <summary>
  ///   Used to get only text representation without umlaut or accents, allowing upper and lower
  ///   case characters and numbers
  /// </summary>
  /// <param name="original">The original text.</param>
  /// <returns>The Text without special characters</returns>
  public static string NoSpecials(this string original) =>
    ProcessByCategory(
      original,
      x => x == UnicodeCategory.LowercaseLetter || x == UnicodeCategory.UppercaseLetter
                                                || x == UnicodeCategory.DecimalDigitNumber);

  /// <summary>
  ///   Used to get only text representation without umlaut or accents, allowing upper and lower
  ///   case characters and numbers
  /// </summary>    
  public static ReadOnlySpan<char> NoSpecials(this ReadOnlySpan<char> original) =>
    ProcessByCategory(
      original,
      x => x == UnicodeCategory.LowercaseLetter || x == UnicodeCategory.UppercaseLetter
                                                || x == UnicodeCategory.DecimalDigitNumber);
  /// <summary>
  ///   Check if a text would match a filter value,
  /// </summary>
  /// <param name="item">The item of a list that should be checked</param>
  /// <param name="filter">
  ///   Filter value, for OR separate words by space for AND separate words by +
  /// </param>
  /// <param name="stringComparison"></param>
  /// <Note>In case the filter is empty there is no filter it will always return true</Note>
  /// <returns>True if text matches</returns>
  public static bool PassesFilter(
    this string? item,
    string? filter,
    StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
  {
    if (filter is null || filter.Length == 0)
      return true;
    if (item is null || item.Length == 0)
      return false;

    return item.AsSpan().PassesFilter(filter.AsSpan(), stringComparison);
  }

  /// <summary>
  /// Determines whether the specified text matches a filter expression.
  /// </summary>
  /// <param name="item">
  /// The text to test against the filter.
  /// </param>
  /// <param name="filter">
  /// The filter expression.
  /// Multiple terms may be separated by spaces, commas, or semicolons (logical OR).
  /// Terms prefixed with '+' are mandatory and must all be present (logical AND).
  /// </param>
  /// <param name="stringComparison">
  /// Specifies how string comparisons are performed (for example, case sensitivity).
  /// </param>
  /// <remarks>
  /// If <paramref name="filter"/> is empty, the method always returns <see langword="true"/>.
  /// If <paramref name="item"/> is empty and the filter is not, the method returns <see langword="false"/>.
  /// </remarks>
  /// <returns>
  /// <see langword="true"/> if the text satisfies the filter expression; otherwise, <see langword="false"/>.
  /// </returns>
  public static bool PassesFilter(
    this ReadOnlySpan<char> item,
    ReadOnlySpan<char> filter,
    StringComparison stringComparison)
  {
    if (filter.IsEmpty)
      return true;
    if (item.IsEmpty)
      return false;

    var slices = filter.GetSlices(new[] { '+', ' ', ',', ';' });
    // Any part that was started with + is required
    var requiredParts = new List<int>();
    for (int i = 0; i < slices.Count; i++)
    {
      if (slices[i].length == 0 || slices[i].start <= 0)
        continue;
      if (filter[slices[i].start-1]=='+')
        requiredParts.Add(i);
    }

    var allRequiredFound = true;
    foreach (var i in requiredParts)
    {
      if (item.IndexOf(filter.Slice(slices[i].start, slices[i].length), stringComparison) != -1)
        continue;
      allRequiredFound = false;
      break;
    }


    if (!allRequiredFound)
      return allRequiredFound;

    bool hasTests = false;
    for (int i = 0; i < slices.Count; i++)
    {
      if (slices[i].length == 0 || requiredParts.Contains(i)) continue;
      hasTests = true;
      if (item.IndexOf(filter.Slice(slices[i].start, slices[i].length), stringComparison) != -1)
        return true;
    }

    return (allRequiredFound && requiredParts.Count > 0) || !hasTests;
  }

  private const string cAllowedLatinWithout6789 = "012345abcdefghijklmnopqrstuvwxyz6789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
  /// <summary>
  /// Any char allowed for Base64 encoded Text
  /// </summary>
  public const string cAllowedBase64 = cAllowedLatinWithout6789 + "/+=";

  /// <summary>
  /// Any char allowed for UL Text
  /// </summary>
  public const string cAllowedURL = cAllowedLatinWithout6789 + "6789/:-._~&?%";

  /// <summary>
  ///   Processes each charter of the string, if the characters is not part of allowedChars,
  ///   the charter is omitted
  /// </summary>
  /// <param name="original">The original.</param>
  /// <param name="allowedChars">a text containing all allowed characters</param>
  /// <returns>A test with only allowed characters</returns>
  public static string OnlyAllowed(this string original, string allowedChars)
  {
    if (string.IsNullOrEmpty(original))
      return string.Empty;

    var chars = new char[original.Length];
    var count = 0;
    foreach (var c in original)
    {
      if (allowedChars.IndexOf(c) != -1)
        chars[count++] = c;
    }

    return new string(chars, 0, count);
  }


  /// <summary>
  ///   Processes each charter of the string by category, if the test function return false,
  ///   the charter is omitted
  /// </summary>
  /// <param name="original">The original.</param>
  /// <param name="testFunction">The test function called on each individual char</param>
  /// <returns>A test with only allowed characters</returns>
  public static string ProcessByCategory(this string original, Func<UnicodeCategory, bool> testFunction)
  {
    if (string.IsNullOrEmpty(original))
      return string.Empty;
    var normalizedString = original.Normalize(NormalizationForm.FormD);

    var chars = new char[normalizedString.Length];
    var count = 0;
    foreach (var c in from c in normalizedString
             let oc = CharUnicodeInfo.GetUnicodeCategory(c)
             where testFunction(oc)
             select c)
      chars[count++] = c;

    return new string(chars, 0, count);
  }
  /// <summary>Processes each char by category, if the test function return false,
  /// the charter is omitted</summary>
  /// <param name="original">The original text as span</param>
  /// <param name="testFunction">The test function called on each individual char</param>
  /// <returns>A test with only allowed characters</returns>
  private static ReadOnlySpan<char> ProcessByCategory(this ReadOnlySpan<char> original, Func<UnicodeCategory, bool> testFunction)
  {
    if (original.Length==0)
      return original;

    var chars = new char[original.Length];
    var count = 0;
    foreach (var c in original)
    {
      var oc = CharUnicodeInfo.GetUnicodeCategory(c);
      if (testFunction(oc))
        chars[count++] = c;
    }
    return new ReadOnlySpan<char>(chars, 0, count);
  }

  /// <summary>
  ///   Checks if the provided text should be treated as NULL
  /// </summary>
  /// <param name="value">A string with the text</param>
  /// <param name="treatAsNull">
  ///   A semicolon separated list of texts that should be treated as NULL
  /// </param>
  /// <returns>True if the text is null, or empty or in the list of provided texts</returns>
  public static bool ShouldBeTreatedAsNull(string? value, string treatAsNull) =>
    value is null || ShouldBeTreatedAsNull(value.AsSpan(), treatAsNull.AsSpan());

  /// <summary>
  ///   Checks if the provided text should be treated as NULL
  /// </summary>
  /// <param name="span">A span with the text</param>
  /// <param name="treatAsNull">
  ///   A semicolon separated list of that should be treated as NULL
  /// </param>
  /// <returns>True if the text is null, or empty or in the list of provided texts</returns>
  public static bool ShouldBeTreatedAsNull(this ReadOnlySpan<char> span, ReadOnlySpan<char> treatAsNull)
  {
    if (treatAsNull.IsEmpty)
      return false;
    if (span.IsEmpty)
      return true;

    foreach (var (start, length) in treatAsNull.GetSlices(StaticCollections.ListDelimiterChars))
    {
      if (span.Equals(treatAsNull.Slice(start, length), StringComparison.OrdinalIgnoreCase))
        return true;
    }
    return false;
  }

  /// <summary>
  ///   Splits the given string at the given set of delimiters to be used as replacement for Split
  /// </summary>
  /// <param name="inputValue">The string to be split.</param>
  /// <param name="splitter">The characters to split with</param>
  /// <returns>List with the slit information empty elements are returned</returns>
  public static IReadOnlyList<(int start, int length)> GetSlices(this ReadOnlySpan<char> inputValue, ReadOnlySpan<char> splitter)
  {
    var retList = new List<(int start, int length)>();
    var start = 0;
    var length = inputValue.IndexOfAny(splitter);
    while (length != -1)
    {
      retList.Add((start, length));
      start += length+1;
      length = inputValue.Slice(start).IndexOfAny(splitter);
    }
    if (inputValue.Length != start)
      retList.Add((start, inputValue.Length - start));
    return retList;
  }

  /// <summary>
  ///   Escapes SQL names; does not include the brackets or quotes
  /// </summary>
  /// <param name="contents">The column or table name.</param>
  /// <returns>The names as it can be placed into brackets</returns>
  public static string SqlName(this string? contents) =>
    contents is null || contents.Length == 0 ? string.Empty : contents.Replace("]", "]]");

  /// <summary>
  ///   Escapes SQL names; does not include the brackets or quotes
  /// </summary>
  public static ReadOnlySpan<char> SqlName(this ReadOnlySpan<char> contents) =>
    contents.IsEmpty || contents.IndexOf(']') == -1 ? contents : contents.ToString().Replace("]", "]]").AsSpan();

  /// <summary>
  ///   Handles quotes in SQLs, does not include the outer quotes
  /// </summary>
  /// <param name="contents">The contents.</param>
  public static string SqlQuote(this string? contents) =>
    contents is null || contents.Length == 0 ? string.Empty : contents.Replace("'", "''");

  /// <summary>
  ///   Strings with the right substitution to be used as filter If a pattern in a LIKE clause
  ///   contains any of these special characters * % [ ], those characters must be escaped in
  ///   brackets [ ] like this [*], [%], [[] or []].
  /// </summary>
  /// <param name="inputValue">The input.</param>
  /// <returns></returns>
  public static string StringEscapeLike(this string? inputValue)
  {
    if (inputValue == null || inputValue.Length==0)
      return string.Empty;
    var returnVal = new StringBuilder(inputValue.Length);
    foreach (var c in inputValue)
    {
      switch (c)
      {
        case '%':
        case '*':
        case '[':
        case ']':
          returnVal.Append($"[{c}]");
          break;

        case '\'':
          returnVal.Append("''");
          break;

        default:
          returnVal.Append(c);
          break;
      }
    }

    return returnVal.ToString();
  }

  /// <summary>
  ///   Handles quotes in SQLs, does not include the outer quotes
  /// </summary>
  public static ReadOnlySpan<char> SqlQuote(this ReadOnlySpan<char> contents) =>
    contents.IsEmpty || contents.IndexOf('\'') == -1 ? contents : contents.ToString().Replace("'", "''").AsSpan();

  /// <summary>
  ///   Read the value and determine if this could be a constant value ( surrounded by " or ' ) or
  ///   if it's a number; if not its assume is a reference to another field
  /// </summary>
  /// <param name="entry">A text that refers to another column or is possibly a constant</param>
  /// <param name="result">The constant value without the " or '</param>
  /// <returns><c>true</c> if a constant was found</returns>
  public static bool TryGetConstant(this string entry, out string result)
  {
    result = string.Empty;
    if (entry.Length == 0)
      return false;
    if (entry.Length > 2
#pragma warning disable IDE0056
        && ((entry[0] == '"' && entry[entry.Length-1]=='"')
            || (entry[0] == '\'' && entry[entry.Length-1]=='\'')))
#pragma warning restore IDE0056
    {
      result = entry.Substring(1, entry.Length - 2);
      return true;
    }

    if (!Regex.IsMatch(entry, @"-?[0-9]+\.?[0-9]*"))
      return false;

    result = entry;
    return true;
  }

  /// <summary>
  ///   Read the value and determine if this could be a constant value ( surrounded by " or ' ) or
  ///   if it's a number; if not its assume is a reference to another field
  /// </summary>
  public static bool TryGetConstant(this ReadOnlySpan<char> entry, out ReadOnlySpan<char> result)
  {
    result = entry;
    switch (entry.Length)
    {
      case 0:
        return false;
      case > 2
        when ((entry[0] == '"' && entry[entry.Length-1]=='"')
              || (entry[0] == '\'' && entry[entry.Length-1]=='\'')):
        result = entry.Slice(1, entry.Length - 2);
        return true;
      default:
        // Should be a English number
#if NET7_0_OR_GREATER
        return Regex.IsMatch(entry, @"[+\-]?[0-9]+\.?[0-9]*");
#else
      return Regex.IsMatch(entry.ToString(), @"[+\-]?[0-9]+\.?[0-9]*");
#endif
    }
  }

}