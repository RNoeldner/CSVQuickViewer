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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CsvTools
{
  /// <summary>
  ///   Collection of static functions for string
  /// </summary>
  [DebuggerStepThrough]
  public static class StringUtils
  {
    public static readonly char[] Spaces =
    {
      ' ', '\u00A0', '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007',
      '\u2008', '\u2009', '\u200A', '\u200B', '\u202F', '\u205F', '\u3000', '\uFEFF'
    };

    /// <summary>
    ///   ; | CR LF Tab
    /// </summary>
    private static readonly char[] m_DelimiterChar = { ';', '|', '\r', '\n', '\t' };

    /// <summary>
    ///   Checks whether a column name text ends on the text ID or Ref
    /// </summary>
    /// <param name="columnName">The column name</param>
    /// <returns>
    ///   The number of charters at the end that did match, 0 if it does not end on ID
    /// </returns>
    public static int AssumeIDColumn(in string? columnName)
    {
      if (columnName is null || columnName.TrimEnd().Length == 0)
        return 0;

      if (columnName.EndsWith(" Text", StringComparison.OrdinalIgnoreCase))
        return 5;
      if (columnName.EndsWith("Text", StringComparison.Ordinal))
        return 4;

      if (columnName.EndsWith(" Ref", StringComparison.OrdinalIgnoreCase))
        return 4;
      if (columnName.EndsWith("Ref", StringComparison.Ordinal))
        return 3;

      if (columnName.EndsWith(" ID", StringComparison.OrdinalIgnoreCase))
        return 3;
      if (columnName.EndsWith("ID", StringComparison.Ordinal) || columnName.EndsWith("Id", StringComparison.Ordinal))
        return 2;

      return 0;
    }

    /// <summary>
    ///   Determines whether this text contains the another text
    /// </summary>
    /// <param name="text">The text to be checked</param>
    /// <param name="toCheck">To text find.</param>
    /// <param name="comp">The comparison.</param>
    /// <returns><c>true</c> if the text does contains the check; otherwise, <c>false</c>.</returns>
    public static bool Contains(this string? text, in string toCheck, in StringComparison comp) =>
      text?.IndexOf(toCheck, comp) >= 0;

    /// <summary>
    ///   Gets the a short representation of the text.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="length">The length.</param>
    /// <returns>The text with the maximum length, in case it has been cut off a … is added</returns>
    public static string GetShortDisplay(in string? text, int length)
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
    ///   The text with every combination of line feed replaced with <see cref="replace" />
    /// </returns>
    public static string HandleCrlfCombinations(this string text, in string replace = "\n")
    {
      // Replace everything Unicode LINE SEPARATOR
      const string placeholderStr = "\u2028";
      const char placeholderChar = '\u2028';
      text = text.Replace("\r\n", placeholderStr);
      text = text.Replace("\n\r", placeholderStr);
      text = text.Replace('\r', placeholderChar);
      text = text.Replace('\n', placeholderChar);
      // now replace this with the desired replace (no matter if string or char)
      return text.Replace(placeholderStr, replace);
    }

    /// <summary>
    ///   Joins the strings
    /// </summary>
    /// <param name="parts">The parts to be joined.</param>
    /// <param name="joinWith">The join with.</param>
    /// <example>JoinParts(new [] {"My","","Test")=&gt; My, Test</example>
    /// <remarks>Any empty string will be ignored.</remarks>
    /// <returns>A string</returns>
    public static string Join(this IEnumerable<string?> parts, in string joinWith = ", ")
    {
      var sb = new StringBuilder();
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
    public static string JoinChar(this IEnumerable<string> parts, in char joinWith = ',')
    {
      var sb = new StringBuilder();
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
    ///   Adds a counter to the name until the nae is unique ion the collection
    /// </summary>
    /// <param name="previousColumns">A collection of already used names, these will not be changed</param>
    /// <param name="nameToAdd">The default name</param>
    /// <returns>The unique name</returns>
    public static string MakeUniqueInCollection(this ICollection<string> previousColumns, in string nameToAdd)
    {
      if (nameToAdd is null)
        throw new ArgumentNullException(nameof(nameToAdd));

      if (!previousColumns.Contains(nameToAdd))
        return nameToAdd;

      // The name is present already

      // cut off any trailing numbers
      var nameNoNumber = nameToAdd.TrimEnd('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '\t', ' ');

      // add 1 for the first add 2 for the second etc
      var counterAdd = 1;
      string fieldName;
      do
      {
        fieldName = nameNoNumber + counterAdd++;
      } while (previousColumns.Contains(fieldName));

      return fieldName;
    }

    /// <summary>
    ///   Used to get text without control characters
    /// </summary>
    /// <param name="original">The original text.</param>
    /// <returns>The original text without control characters</returns>
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
      in string? filter,
      StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
    {
      if (filter is null || filter.Length == 0)
        return true;
      if (item is null || item.Length == 0)
        return false;

      if (filter.IndexOf('+') <= -1)
        return filter.Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
          .Any(part => item.IndexOf(part, stringComparison) != -1);
      var parts = filter.Split(new[] { '+', ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
      if (parts.Length == 0)
        return true;

      // 1st part
      var all = item.IndexOf(parts[0], stringComparison) > -1;

      // and all other parts
      for (var index = 1; index < parts.Length && all; index++)
        if (item.IndexOf(parts[index], stringComparison) == -1)
          all = false;
      return all;
    }

    /// <summary>
    ///   Processes the each charter of the string by category, if the test function return false,
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

    /// <summary>
    ///   Checks if the provided text should be treated as NULL
    /// </summary>
    /// <param name="value">A string with the text</param>
    /// <param name="treatAsNull">
    ///   A semicolon separated list of texts that should be treated as NULL
    /// </param>
    /// <returns>True if the text is null, or empty or in the list of provided texts</returns>
    public static bool ShouldBeTreatedAsNull(string? value, in string treatAsNull)
    {
      if (treatAsNull.Length == 0)
        return false;
      return value is null || value.Length == 0 || SplitByDelimiter(treatAsNull)
        .Any(part => value.Equals(part, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///   Splits the given string at fixed delimiters.
    /// </summary>
    /// <param name="inputValue">The string to be split.</param>
    /// <returns>String array with substrings, empty elements are removed</returns>
    public static string[] SplitByDelimiter(in string? inputValue) =>
      inputValue is null || inputValue.Length == 0
        ? Array.Empty<string>()
        : inputValue.Split(m_DelimiterChar, StringSplitOptions.RemoveEmptyEntries);

    /// <summary>
    ///   Escapes SQL names; does not include the brackets or quotes
    /// </summary>
    /// <param name="contents">The column or table name.</param>
    /// <returns>The names as it can be placed into brackets</returns>
    public static string SqlName(this string? contents) =>
      contents is null || contents.Length == 0 ? string.Empty : contents.Replace("]", "]]");

    /// <summary>
    ///   SQLs the quote, does not include the outer quotes
    /// </summary>
    /// <param name="contents">The contents.</param>
    /// <returns></returns>
    public static string SqlQuote(this string? contents) =>
      contents is null || contents.Length == 0 ? string.Empty : contents.Replace("'", "''");

    /// <summary>
    ///   Read the value and determine if this could be a constant value (surrounded by " or ') or
    ///   if its a number; if not its assume is a reference to another field
    /// </summary>
    /// <param name="entry">A text that refers to another column or is possibly a constant</param>
    /// <param name="result">The constant value without the " or '</param>
    /// <returns><c>true</c> if a constant was found</returns>
    public static bool TryGetConstant(this string? entry, out string result)
    {
      result = string.Empty;
      if (entry is null || entry.Length == 0)
        return false;

      if (entry.Length > 2
          && ((entry[0] == '"' && entry.EndsWith("\"", StringComparison.Ordinal))
              || (entry[0] == '\'' && entry.EndsWith("'", StringComparison.Ordinal))))
      {
        result = entry.Substring(1, entry.Length - 2);
        return true;
      }

      if (!Regex.IsMatch(entry, @"-?[0-9]+\.?[0-9]*"))
        return false;

      result = entry;
      return true;
    }
  }
}