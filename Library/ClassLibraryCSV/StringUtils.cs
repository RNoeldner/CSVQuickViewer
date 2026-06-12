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
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CsvTools;

/// <summary>
///   Collection of static functions for string
/// </summary>
[DebuggerStepThrough]
public static class StringUtils
{
  /// <summary>
  /// Any char allowed for Base64 encoded Text
  /// </summary>
  public const string cAllowedBase64 = cAllowedLatinWithout6789 + "/+=";

  /// <summary>
  /// Any char allowed for UL Text
  /// </summary>
  public const string cAllowedURL = cAllowedLatinWithout6789 + "6789/:-._~&?%";

  private const string cAllowedLatinWithout6789 = "012345abcdefghijklmnopqrstuvwxyz6789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

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
  public static string GetShortDisplay(ReadOnlySpan<char> text, int length)
  {
    if (text.IsEmpty || text.IsWhiteSpace() || length < 1)
      return string.Empty;
    // 1. Prepare a buffer for the cleaned text (no LFs, Tabs, or double spaces)
    char[]? pooledArray = null;
    Span<char> buffer = text.Length <= 256
        ? stackalloc char[text.Length]
        : (pooledArray = ArrayPool<char>.Shared.Rent(text.Length));
    try
    {
      int pos = 0;
      bool lastWasSpace = false;

      // 2. Clean text in a single pass (Replaces \r, \n, \t and collapses spaces)
      foreach (var c in text)
      {
        char current = c;
        if (current is '\r' or '\n' or '\t') current = ' ';

        if (current == ' ')
        {
          if (lastWasSpace) continue; // Collapse double spaces
          lastWasSpace = true;
        }
        else
        {
          lastWasSpace = false;
        }

        buffer[pos++] = current;
      }

      var cleaned = buffer.Slice(0, pos).Trim();

      // 3. Handle length constraints
      if (cleaned.Length <= length)
        return cleaned.ToString();

      // 4. Find the best truncation point (Last space within the last 1/8th of the limit)
      var limit = length - 1;
      var lookbackLimit = limit / 8;
      var segmentToSearch = cleaned.Slice(0, limit);

      int spaceIndex = segmentToSearch.LastIndexOf(' ');

      // Only break at space if it's within a reasonable distance of the end
      if (spaceIndex > 1 && spaceIndex >= (limit - lookbackLimit))
      {
        return $"{cleaned.Slice(0, spaceIndex).TrimEnd().ToString()}…";
      }

      return $"{cleaned.Slice(0, limit).TrimEnd().ToString()}…";
    }
    finally
    {
      if (pooledArray != null)
        ArrayPool<char>.Shared.Return(pooledArray);
    }
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
  /// Converts a SecureString to a managed string for use in KDFs.
  /// Warning: The returned string persists in the managed heap until GC collection.
  /// </summary>
  public static string GetText(this System.Security.SecureString secPassword)
  {
    IntPtr unmanagedString = IntPtr.Zero;
    try
    {
      unmanagedString = System.Runtime.InteropServices.Marshal.SecureStringToGlobalAllocUnicode(secPassword);
      return System.Runtime.InteropServices.Marshal.PtrToStringUni(unmanagedString) ?? string.Empty;
    }
    finally
    {
      if (unmanagedString != IntPtr.Zero)
        System.Runtime.InteropServices.Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
    }
  }

  /// <summary>
  ///   All combination of \r \n will be made to a single replacement
  /// </summary>
  /// <param name="text">The Text</param>
  /// <param name="replaceSpan">The replacement value default is \n.</param>
  /// <returns>
  ///   The text with every combination of line feed replaced with the given replacement
  /// </returns>
  public static string HandleCrlfCombinations(this ReadOnlySpan<char> text, ReadOnlySpan<char> replaceSpan)
  {
    if (text.IsEmpty) return string.Empty;
    // Use a pooled buffer to avoid StringBuilder overhead and internal resizes
    char[]? pooledArray = null;

    // Note: The result could technically be longer than the input if 'replace' is long
    int maxPossibleLength = text.Length * Math.Max(1, replaceSpan.Length);

    Span<char> buffer = maxPossibleLength <= 256
        ? stackalloc char[maxPossibleLength]
        : (pooledArray = ArrayPool<char>.Shared.Rent(maxPossibleLength));

    try
    {
      int pos = 0;
      char lastC = '\0';

      foreach (var chr in text)
      {
        // Handle combinations (CRLF or LFCR)
        if ((chr == '\r' && lastC == '\n') || (chr == '\n' && lastC == '\r'))
        {
          lastC = '\0'; // Consume the pair
          continue;
        }

        if (chr is '\r' or '\n')
        {
          // If the buffer is too small (replacement string is long), 
          // we'd need to resize, but for standard replacements, 
          // we can just copy the span.
          replaceSpan.CopyTo(buffer.Slice(pos));
          pos += replaceSpan.Length;
        }
        else
        {
          buffer[pos++] = chr;
        }
        lastC = chr;
      }

      return buffer.Slice(0, pos).ToString();
    }
    finally
    {
      if (pooledArray != null)
        ArrayPool<char>.Shared.Return(pooledArray);
    }
  }

  /// <summary>
  ///   All combination of \r \n will be reagrded as single \n
  /// </summary>
  /// <param name="text">The Text</param>
  /// <returns>
  ///   The text with every combination of line feed replaced
  /// </returns>
  public static string HandleCrlfCombinations(this ReadOnlySpan<char> text)
  {
    if (text.IsEmpty) return string.Empty;
    // Use a pooled buffer to avoid StringBuilder overhead and internal resizes
    char[]? pooledArray = null;

    Span<char> buffer = text.Length <= 256
        ? stackalloc char[text.Length]
        : (pooledArray = ArrayPool<char>.Shared.Rent(text.Length));

    try
    {
      int pos = 0;
      char lastC = '\0';

      foreach (var chr in text)
      {
        // Handle combinations (CRLF or LFCR)
        if ((chr == '\r' && lastC == '\n') || (chr == '\n' && lastC == '\r'))
        {
          lastC = '\0'; // Consume the pair
          continue;
        }

        if (chr is '\r' or '\n')
          buffer[pos++] = '\n';
        else
          buffer[pos++] = chr;
        lastC = chr;
      }

      return buffer.Slice(0, pos).ToString();
    }
    finally
    {
      if (pooledArray != null)
        ArrayPool<char>.Shared.Return(pooledArray);
    }
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
  /// This check is case-insensitive.
  /// </param>
  /// <param name="nameToAdd">The desired name to add to the collection.</param>
  /// <returns>
  /// A name guaranteed to be unique in <paramref name="previousColumns"/>. 
  /// If the name exists, a suffix is added (e.g., "Year2025" becomes "Year2025_1").
  /// If an existing version suffix is detected (e.g., "Field_1"), it is incremented ("Field_2").
  /// </returns>
  /// <exception cref="ArgumentException">Thrown if <paramref name="nameToAdd"/> is null or empty.</exception>
  /// <remarks>
  /// 1. Trailing ellipsis (Unicode … or "...") are preserved and re-applied after the numeric suffix.
  /// 2. Smart Suffix Detection: Trailing digits are only treated as a version suffix if they are preceded 
  ///    by a separator (space, underscore, or hyphen). This prevents names like "Year2025" from 
  ///    being incorrectly stripped to "Year1".
  /// 3. If no existing separator is found, an underscore ("_") is used by default for the new suffix.
  /// </remarks>
  public static string MakeUniqueInCollection(this IEnumerable<string> previousColumns, string nameToAdd)
  {
    // Use a local list to avoid multiple enumerations if previousColumns is a LINQ query
    var existing = previousColumns as ICollection<string> ?? previousColumns.ToList();

    if (!existing.Contains(nameToAdd, StringComparer.OrdinalIgnoreCase))
      return nameToAdd;

    // 1. Handle Ellipsis
    string ellipsis = string.Empty;
    string cleanName = nameToAdd;
    if (nameToAdd.EndsWith("…", StringComparison.Ordinal))
    {
      ellipsis = "…";
      cleanName = nameToAdd.TrimEnd('…');
    }
    else if (nameToAdd.EndsWith("...", StringComparison.Ordinal))
    {
      ellipsis = "...";
      cleanName = nameToAdd.Substring(0, nameToAdd.Length - 3);
    }

    // 2. Character-based Prefix/Separator Detection
    string prefix = cleanName;
    string separator = "_";
    int lastIndex = cleanName.Length - 1;

    if (lastIndex >= 0 && char.IsDigit(cleanName[lastIndex]))
    {
      int digitStart = lastIndex;
      while (digitStart > 0 && char.IsDigit(cleanName[digitStart - 1]))
      {
        digitStart--;
      }

      if (digitStart > 0)
      {
        char c = cleanName[digitStart - 1];
        if (c == ' ' || c == '_' || c == '-')
        {
          separator = c.ToString();
          // TrimEnd ensures "Field _1" doesn't become "Field  _2"
          prefix = cleanName.Substring(0, digitStart - 1).TrimEnd();
        }
      }
    }

    // 3. Increment logic
    int counter = 1;
    while (true)
    {
      // Construct candidate: {Prefix}{Separator}{Number}{Ellipsis}
      string candidate = $"{prefix}{separator}{counter++}{ellipsis}";

      // Clean up any accidental double spaces from the prefix trim
      candidate = candidate.Replace("  ", " ").Trim();

      if (!existing.Contains(candidate, StringComparer.OrdinalIgnoreCase))
        return candidate;
    }
  }

  /// <summary>
  /// Removes Unicode control characters from the character span while preserving standard line breaks.
  /// </summary>
  /// <param name="input">The read-only character span to process.</param>
  /// <returns>
  /// A string containing the original characters, excluding those in the <see cref="UnicodeCategory.Control"/> 
  /// ranges (U+0000..U+001F and U+007F..U+009F), with the exception of carriage returns ('\r') and line feeds ('\n').
  /// </returns>
  /// <remarks>
  /// This method is optimized for performance:
  /// <list type="bullet">
  ///   <item><description>Uses a hybrid allocation strategy: <c>stackalloc</c> for small inputs and <see cref="ArrayPool{T}"/> for larger data.</description></item>
  ///   <item><description>Implements a range-based short-circuit to avoid expensive <see cref="CharUnicodeInfo.GetUnicodeCategory(char)"/> lookups for standard alphanumeric characters.</description></item>
  ///   <item><description>Collapses the result into a single string allocation at the end of the process.</description></item>
  /// </list>
  /// </remarks>
  public static string NoControlCharacters(this ReadOnlySpan<char> input)
  {
    char[]? pooledArray = null;

    // 1. Setup buffer: stack for small, pool for large
    Span<char> buffer = input.Length <= 256
        ? stackalloc char[input.Length]
        : (pooledArray = ArrayPool<char>.Shared.Rent(input.Length));

    try
    {
      int count = 0;
      foreach (char c in input)
      {
        // 2. Direct character check for speed
        // Unicode Control: U+0000-U+001F and U+007F-U+009F
        // We optimize by checking numeric ranges before hitting CharUnicodeInfo
        if (c == '\r' || c == '\n')
        {
          buffer[count++] = c;
        }
        else
        {
          // Most standard characters are > U+001F and not U+007F
          // Only check category if it falls in the known control ranges
          bool isPotentialControl = (c <= '\u001F') || (c >= '\u007F' && c <= '\u009F');

          if (!isPotentialControl || CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.Control)
          {
            buffer[count++] = c;
          }
        }
      }

      // 3. Return the sliced result
      return buffer.Slice(0, count).ToString();
    }
    finally
    {
      if (pooledArray != null)
        ArrayPool<char>.Shared.Return(pooledArray);
    }
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

    char[]? pooledArray = null;
    Span<char> chars = original.Length <= 256
      ? stackalloc char[original.Length]
      : (pooledArray = ArrayPool<char>.Shared.Rent(original.Length));
    try
    {
      var count = 0;
      foreach (var c in original.Where(c => allowedChars.IndexOf(c) != -1))
      {
        chars[count++] = c;
      }
      return chars.Slice(0, count).ToString();
    }
    finally
    {
      // Corrected: Return to pool
      if (pooledArray != null) ArrayPool<char>.Shared.Return(pooledArray);
    }
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
  public static bool PassesFilter(this ReadOnlySpan<char> item, ReadOnlySpan<char> filter, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
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

  /// <summary>
  ///   Processes each charter of the string by category, if the test function return false,
  ///   the charter is omitted
  /// </summary>
  /// <param name="original">The original.</param>
  /// <param name="testFunction">The test function called on each individual char</param>
  /// <returns>A test with only allowed characters</returns>
  public static string ProcessByCategory(this string original, Func<UnicodeCategory, bool> testFunction)
  {
    if (string.IsNullOrEmpty(original)) return string.Empty;
    // Normalization creates a new string
    var span = original.Normalize(NormalizationForm.FormD).AsSpan();
    char[]? pooledArray = null;
    Span<char> destination = span.Length <= 256
    ? stackalloc char[span.Length]
    : (pooledArray = ArrayPool<char>.Shared.Rent(span.Length));
    try
    {
      int count = 0;
      foreach (var c in span)
      {
        if (testFunction(CharUnicodeInfo.GetUnicodeCategory(c)))
          destination[count++] = c;
      }
      return destination.Slice(0, count).ToString();
    }
    finally
    {
      // Corrected: Return to pool
      if (pooledArray != null) ArrayPool<char>.Shared.Return(pooledArray);
    }
  }

  /// <summary>
  ///   Checks if the provided text should be treated as NULL
  /// </summary>
  /// <param name="span">A span with the text</param>
  /// <param name="treatAsNull">
  ///   A semicolon or tab separated list of that should be treated as NULL
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
  /// Escapes a string for use inside T-SQL square brackets by doubling closing brackets.
  /// </summary>
  /// <param name="contents">The raw column or table name to escape.</param>
  /// <returns>The escaped string with closing brackets (']') doubled.</returns>
  public static string SqlName(this ReadOnlySpan<char> contents)
  {
    if (contents.IsEmpty)
      return string.Empty;

    // 1. Scan for closing brackets to determine if escaping is necessary
    int bracketCount = 0;
    foreach (var c in contents)
    {
      if (c == ']') bracketCount++;
    }

    // Fast path: No closing brackets found, return as-is
    if (bracketCount == 0)
      return contents.ToString();

    // 2. Allocate buffer (stack for small names, pool for large/complex names)
    int length = contents.Length + bracketCount;
    char[]? pooledArray = null;

    Span<char> buffer = length <= 256
        ? stackalloc char[length]
        : (pooledArray = ArrayPool<char>.Shared.Rent(length));

    try
    {
      int pos = 0;
      foreach (var c in contents)
      {
        buffer[pos++] = c;
        if (c == ']')
        {
          // Double the closing bracket
          buffer[pos++] = ']';
        }
      }

      return buffer.Slice(0, pos).ToString();
    }
    finally
    {
      if (pooledArray != null)
        ArrayPool<char>.Shared.Return(pooledArray);
    }
  }

  /// <summary>
  /// Handles quotes in SQL strings by doubling single quotes. 
  /// Does not include the outer quotes.
  /// </summary>
  /// <param name="contents">The raw content to escape.</param>
  /// <returns>The escaped string with single quotes doubled.</returns>
  public static string SqlQuote(this ReadOnlySpan<char> contents)
  {
    if (contents.IsEmpty)
      return string.Empty;

    // Check if we even need to escape anything to avoid unnecessary work
    int quoteCount = 0;
    foreach (var c in contents)
    {
      if (c == '\'') quoteCount++;
    }

    // Optimization: If no quotes found, return the original content as a string
    if (quoteCount == 0)
      return contents.ToString();

    int length = contents.Length + quoteCount;
    char[]? pooledArray = null;

    Span<char> buffer = length <= 256
        ? stackalloc char[length]
        : (pooledArray = ArrayPool<char>.Shared.Rent(length));

    try
    {
      int pos = 0;
      foreach (var c in contents)
      {
        if (c == '\'')
        {
          buffer[pos++] = '\'';
          buffer[pos++] = '\'';
        }
        else
        {
          buffer[pos++] = c;
        }
      }

      return buffer.Slice(0, pos).ToString();
    }
    finally
    {
      if (pooledArray != null)
        ArrayPool<char>.Shared.Return(pooledArray);
    }
  }

  /// <summary>
  ///   Strings with the right substitution to be used as filter If a pattern in a LIKE clause
  ///   contains any of these special characters * % [ ], those characters must be escaped in
  ///   brackets [ ] like this [*], [%], [[] or []].
  /// </summary>
  /// <param name="inputValue">The input.</param>
  /// <returns></returns>
  public static string StringEscapeLike(this ReadOnlySpan<char> inputValue)
  {
    if (inputValue.IsEmpty)
      return string.Empty;

    // Worst case: every character is a special char like '%', turning into '[%]'
    // This triples the length.
    int maxLength = inputValue.Length * 3;
    char[]? pooledArray = null;

    Span<char> buffer = maxLength <= 256
        ? stackalloc char[maxLength]
        : (pooledArray = ArrayPool<char>.Shared.Rent(maxLength));

    try
    {
      int pos = 0;
      foreach (var c in inputValue)
      {
        switch (c)
        {
          case '%' or '*' or '[' or ']':
            buffer[pos++] = '[';
            buffer[pos++] = c;
            buffer[pos++] = ']';
            break;

          case '\'':
            // SQL Escape for single quote is two single quotes
            buffer[pos++] = '\'';
            buffer[pos++] = '\'';
            break;

          default:
            buffer[pos++] = c;
            break;
        }
      }

      return buffer.Slice(0, pos).ToString();
    }
    finally
    {
      if (pooledArray != null)
        ArrayPool<char>.Shared.Return(pooledArray);
    }
  }

  /// <summary>
  /// Converts a text to a secured text
  /// </summary>
  /// <param name="text">The plain text</param>
  /// <exception cref="System.ArgumentNullException">text</exception>
  public static System.Security.SecureString ToSecureString(this ReadOnlySpan<char> text)
  {
    var securePassword = new System.Security.SecureString();

    foreach (var c in text)
      securePassword.AppendChar(c);

    securePassword.MakeReadOnly();
    return securePassword;
  }

  /// <summary>
  /// Identifies if the entry is a constant (quoted string or numeric) and returns the inner value.
  /// </summary>
  /// <param name="entry">The raw character span to evaluate.</param>
  /// <param name="result">The identified constant value (unquoted if a string).</param>
  /// <returns>True if the entry is a valid string or numeric constant.</returns>
  public static bool TryGetConstant(this ReadOnlySpan<char> entry, out ReadOnlySpan<char> result)
  {
    result = entry;
    int length = entry.Length;

    if (length == 0)
      return false;

    // 1. Handle Quoted Constants (Strings)
    if (length >= 2)
    {
      char first = entry[0];
      char last = entry[length - 1];
      if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
      {
        result = entry.Slice(1, length - 2);
        return true;
      }
    }

    // 2. Handle Numeric Constants (Manual Parse)
    // Validates: Optional sign, at least one digit, optional single decimal point
    int i = (entry[0] == '+' || entry[0] == '-') ? 1 : 0;

    // If it's just "+" or "-", or empty after the sign, it's not a number
    if (i == length) return false;

    bool hasDecimal = false;
    bool hasDigits = false;

    for (; i < length; i++)
    {
      char c = entry[i];
      if (c >= '0' && c <= '9')
      {
        hasDigits = true;
      }
      else if ((c == '.' || c == ',') && !hasDecimal)
      {
        hasDecimal = true;
      }
      else
      {
        // Found a non-numeric character (e.g., a letter or second dot)
        return false;
      }
    }

    // Ensure it's not just a "." and actually contains digits
    if (!hasDigits) return false;

    // It's a number, result is already set to entry
    return true;
  }

}